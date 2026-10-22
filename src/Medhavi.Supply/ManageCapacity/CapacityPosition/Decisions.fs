/// DE-S-031 — Determine Capacity Position Status
/// Traces to: Supply Intelligence Specification §6 (DE-S-031)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Decisions

open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

/// DE-S-031: Pure decision logic for determining capacity position status and detecting material change
let evaluatePositionStatus
    (resourceId: PhysicalResourceId)
    (capacityFacts: ResourceCapacityFacts)
    (loadFact: ResourceLoadFact)
    (existingOpt: CapacityPositionAssessment option)
    (evidence: string list)
    (timestamp: Timestamp)
    (policy: CapacityManagementPolicy)
    : CapacityPositionAssessment * CapacityAssessmentChangeEvent option =
    
    // 1. Compute scheduled operating hours from shifts
    let scheduledHours =
        capacityFacts.Shifts
        |> List.sumBy (fun s -> decimal (Timestamp.value s.EndTime - Timestamp.value s.StartTime).TotalHours)

    // 2. Extract outage intervals
    let outageIntervals =
        capacityFacts.Outages
        |> List.map (fun o -> (Timestamp.value o.StartTime, Timestamp.value o.EndTime))

    // 3. Compute available capacity (BA-S-010)
    let acResult =
        Algorithms.computeAvailableCapacity scheduledHours outageIntervals policy.ResourceCapacityRating

    // 4. Compute operating envelope (BA-S-011)
    let envelope =
        Algorithms.computeOperatingEnvelope acResult.AvailableHours policy

    // 5. Evaluate exposure across 6 dimensions (BA-S-012)
    let totalLoad = loadFact.CurrentCommittedHours + loadFact.ProjectedPlannedHours
    let exposure =
        Algorithms.evaluateCapacityExposure
            acResult.AvailableHours
            totalLoad
            loadFact.DownstreamDependentJobsCount
            capacityFacts.HasAlternateResource
            capacityFacts.IsUniqueCapability
            envelope
            policy

    // 6. Build time-phased bucket
    let bucket : CapacityPeriodBucket =
        {   BucketIndex = 1
            StartTime = timestamp
            EndTime = timestamp
            ScheduledOperatingHours = scheduledHours
            DowntimeHours = acResult.TotalDowntimeHours
            AvailableHours = acResult.AvailableHours
            CommittedPlannedHours = totalLoad
            ProjectedUtilization = exposure.EffectiveUtilization
            UtilizationStatus = exposure.UtilizationStatus
            ConstraintStatus = exposure.ConstraintStatus
            BottleneckStatus = exposure.BottleneckStatus }

    // 7. Check for material change and construct change event if applicable
    let materialChange =
        Rules.isMaterialChange existingOpt exposure.UtilizationStatus exposure.ConstraintStatus exposure.BottleneckStatus

    let changeEventOpt, updatedHistory =
        if materialChange then
            let prevHistory =
                match existingOpt with
                | Some e -> e.ChangeHistory
                | None -> []

            let nextSeq =
                match prevHistory with
                | [] -> 1
                | head :: _ -> head.SequenceNumber + 1

            let prevUtil =
                match existingOpt with
                | Some e -> e.UtilizationStatus
                | None -> CapacityUtilizationStatus.Optimal

            let prevConstraint =
                match existingOpt with
                | Some e -> e.ConstraintStatus
                | None -> CapacityConstraintStatus.NonConstraint

            let prevBottleneck =
                match existingOpt with
                | Some e -> e.BottleneckStatus
                | None -> BottleneckAssessmentStatus.NotABottleneck

            let reason =
                if existingOpt.IsNone then "Initial capacity position assessment established"
                else $"Capacity position transitioned to {exposure.UtilizationStatus} / {exposure.ConstraintStatus}"

            let changeEvent : CapacityAssessmentChangeEvent =
                {   SequenceNumber = nextSeq
                    ChangeTimestamp = timestamp
                    PreviousUtilizationStatus = prevUtil
                    NewUtilizationStatus = exposure.UtilizationStatus
                    PreviousConstraintStatus = prevConstraint
                    NewConstraintStatus = exposure.ConstraintStatus
                    PreviousBottleneckStatus = prevBottleneck
                    NewBottleneckStatus = exposure.BottleneckStatus
                    TriggeringEvidence = evidence
                    ReasonCode = reason }

            Some changeEvent, changeEvent :: prevHistory
        else
            None, (match existingOpt with Some e -> e.ChangeHistory | None -> [])

    let assessment : CapacityPositionAssessment =
        {   ResourceId = resourceId
            LifecycleState = PositionLifecycleState.Active
            ScheduledOperatingTimeHours = scheduledHours
            TotalDowntimeHours = acResult.TotalDowntimeHours
            AvailableCapacityHours = acResult.AvailableHours
            OperatingEnvelope = envelope
            CurrentCommittedHours = loadFact.CurrentCommittedHours
            ProjectedPlannedHours = loadFact.ProjectedPlannedHours
            EffectiveUtilization = exposure.EffectiveUtilization
            UtilizationStatus = exposure.UtilizationStatus
            ConstraintStatus = exposure.ConstraintStatus
            BottleneckStatus = exposure.BottleneckStatus
            RiskLevel = exposure.RiskLevel
            PolicyCompliance = exposure.PolicyCompliance
            HasSinglePointOfFailureRisk = exposure.HasSinglePointOfFailure
            HasAlternateResource = capacityFacts.HasAlternateResource
            Buckets = [bucket]
            SourceSupplyUnderstandingVersion = None
            SourceSupplyPlanVersion = None
            AssessmentTimestamp = timestamp
            ChangeHistory = updatedHistory }

    assessment, changeEventOpt
