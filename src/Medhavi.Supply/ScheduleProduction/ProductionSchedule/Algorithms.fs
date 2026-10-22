/// BA-S-040 — Construct Production Schedule
/// BA-S-041 — Assess Schedule Stability
/// Traces to: Supply Intelligence Specification §10 (BA-S-040, BA-S-041)
module Medhavi.Supply.ScheduleProduction.ProductionSchedule.Algorithms

open System
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies

type SchedulingInput =
    { ResourceId: PhysicalResourceId
      PeriodStart: Timestamp
      PeriodEnd: Timestamp
      Requirements: PlannedProductionRequirementFact list
      Availability: ResourceAvailabilityFact
      ChangeoverMatrix: ChangeoverMatrixFact
      PriorSchedule: ActiveScheduleFact option
      Policy: SchedulingPolicyConfig }

type ConstructedScheduleResult =
    { Activities: ProductionActivity list
      Changeovers: ChangeoverDetail list
      TotalScheduledHours: decimal
      TotalChangeoverHours: decimal
      OvertimeHours: decimal
      Feasibility: FeasibilityEvaluation
      Confidence: ConfidenceEvaluation
      Stability: StabilityMetrics
      ReasonCodes: SchedulingReasonCode list }

/// Computes the sequence-dependent changeover duration between two items
let getChangeoverDuration
    (matrix: ChangeoverMatrixFact)
    (fromItemOpt: ItemId option)
    (toItem: ItemId)
    : decimal =
    match fromItemOpt with
    | None -> 0.0M // First job starts clean or setup included in initial shift
    | Some fromItem ->
        if fromItem = toItem then
            0.0M // Same product, no changeover required
        else
            match matrix.Matrix.TryFind(fromItem, toItem) with
            | Some duration -> duration
            | None -> matrix.DefaultChangeoverHours

/// Stage 5 Stability calculation: computes S_stab in [0.0, 100.0] against prior active schedule
let computeScheduleStability
    (currentActivities: ProductionActivity list)
    (priorFactOpt: ActiveScheduleFact option)
    (policy: SchedulingPolicyConfig)
    : StabilityMetrics =
    match priorFactOpt with
    | None ->
        // No prior schedule exists: first publication has stability 100.0
        { StabilityScore = 100.0M
          ChangeCount = 0
          ChangeMagnitude = 0.0M }
    | Some prior ->
        let currCount = decimal currentActivities.Length
        let priorCount = decimal prior.ActivityCount
        let maxCount = Math.Max(1.0M, Math.Max(currCount, priorCount))
        let deltaCountRatio = Math.Abs(currCount - priorCount) / maxCount

        let currHours = currentActivities |> List.sumBy (fun a -> a.DurationHours)
        let priorHours = prior.TotalScheduledHours
        let maxHours = Math.Max(1.0M, Math.Max(currHours, priorHours))
        let deltaHoursRatio = Math.Abs(currHours - priorHours) / maxHours

        let penalty = (policy.WeightJobCountW1 * deltaCountRatio + policy.WeightQuantityW2 * deltaHoursRatio) * 100.0M
        let rawScore = 100.0M - penalty
        let stabilityScore = Math.Round(Math.Clamp(rawScore, 0.0M, 100.0M), 2)
        let changeCount = int (Math.Abs(currCount - priorCount))
        let changeMagnitude = Math.Round(Math.Abs(currHours - priorHours), 2)

        { StabilityScore = stabilityScore
          ChangeCount = changeCount
          ChangeMagnitude = changeMagnitude }

type private PlacementAccumulator =
    { CurrentTime: Timestamp
      LastActivityIdOpt: ActivityId option
      LastItemOpt: ItemId option
      ActivitiesRev: ProductionActivity list
      ChangeoversRev: ChangeoverDetail list
      SequenceCounter: int
      HasLateJobs: bool
      HasMaterialDelay: bool
      HasFrozenProtection: bool }

/// BA-S-040: Construct Production Schedule (5-Stage Implementation)
let constructProductionSchedule (input: SchedulingInput) : ConstructedScheduleResult =
    let p = input.Policy
    let runRate = Math.Max(0.0001M, input.Availability.NominalRunRateUnitsPerHour)
    let frozenCutoff = Timestamp.add input.PeriodStart (TimeSpan.FromHours(float p.FrozenHorizonHours))
    let horizonSpan = Timestamp.diff input.PeriodEnd input.PeriodStart
    let horizonHours = Math.Max(1.0M, decimal horizonSpan.TotalHours)

    // Stage 1: Validation and Material Arrival Binding
    let validReqs =
        input.Requirements
        |> List.filter (fun r -> r.Quantity > 0.0M)

    // Stage 2 & 3: Heuristic Sequencing balancing Due Date and Changeover
    // Sort unfrozen jobs using bi-criterion score
    let sortedReqs =
        let alpha = Math.Clamp(p.DueDateWeightAlpha, 0.50M, 1.0M)
        let maxMatrixDuration =
            if input.ChangeoverMatrix.Matrix.IsEmpty then
                p.DefaultChangeoverHours
            else
                input.ChangeoverMatrix.Matrix.Values |> Seq.max |> fun m -> Math.Max(m, p.DefaultChangeoverHours)

        let rec sequenceRemaining
            (scheduled: PlannedProductionRequirementFact list)
            (lastItemOpt: ItemId option)
            (remaining: PlannedProductionRequirementFact list)
            : PlannedProductionRequirementFact list =
            match remaining with
            | [] -> List.rev scheduled
            | [ single ] -> List.rev (single :: scheduled)
            | pool ->
                let best =
                    pool
                    |> List.minBy (fun req ->
                        let dueDiff = Timestamp.diff req.DueDate input.PeriodStart
                        let dueOffset = decimal dueDiff.TotalHours
                        let normalizedDue = Math.Clamp(dueOffset / horizonHours, 0.0M, 2.0M)
                        let setupTime = getChangeoverDuration input.ChangeoverMatrix lastItemOpt req.ItemId
                        let normalizedSetup = setupTime / Math.Max(0.001M, maxMatrixDuration)
                        let priorityPenalty = decimal req.ProductionPriority * 0.05M
                        alpha * normalizedDue + (1.0M - alpha) * normalizedSetup - priorityPenalty)

                let rest = pool |> List.filter (fun r -> r.RequirementId <> best.RequirementId)
                sequenceRemaining (best :: scheduled) (Some best.ItemId) rest

        sequenceRemaining [] None validReqs

    // Stage 4: Finite Working Window Time-Placement
    let initialState: PlacementAccumulator =
        { CurrentTime = input.PeriodStart
          LastActivityIdOpt = None
          LastItemOpt = None
          ActivitiesRev = []
          ChangeoversRev = []
          SequenceCounter = 1
          HasLateJobs = false
          HasMaterialDelay = false
          HasFrozenProtection = false }

    let finalPlacement =
        sortedReqs
        |> List.fold (fun state req ->
            let runHours = Math.Round(req.Quantity / runRate, 4)
            let setupHours = getChangeoverDuration input.ChangeoverMatrix state.LastItemOpt req.ItemId

            // Gating 1: Material arrival must not be violated
            let (timeAfterMaterial, hasMaterialDelay) =
                if Timestamp.isBefore state.CurrentTime req.MaterialAvailableDate then
                    (req.MaterialAvailableDate, true)
                else
                    (state.CurrentTime, state.HasMaterialDelay)

            let actId = ActivityId $"ACT-{PhysicalResourceId.value input.ResourceId}-{state.SequenceCounter:D4}"

            // If changeover is required between jobs
            let (timeAfterSetup, newChangeoversRev) =
                match state.LastActivityIdOpt with
                | Some lastActId when setupHours > 0.0M ->
                    let chgId = ChangeoverId $"CHG-{PhysicalResourceId.value input.ResourceId}-{state.SequenceCounter:D4}"
                    let chgDetail =
                        { ChangeoverId = chgId
                          FromActivityId = lastActId
                          ToActivityId = actId
                          ChangeoverType = ChangeoverType.StandardWashdown
                          SetupHours = setupHours
                          CleaningHours = None
                          ToolingChangeHours = None }
                    (Timestamp.add timeAfterMaterial (TimeSpan.FromHours(float setupHours)), chgDetail :: state.ChangeoversRev)
                | _ ->
                    (timeAfterMaterial, state.ChangeoversRev)

            let startTime = timeAfterSetup
            let endTime = Timestamp.add startTime (TimeSpan.FromHours(float runHours))
            let hasLateJobs = state.HasLateJobs || Timestamp.isAfter endTime req.DueDate
            let isFrozen = Timestamp.isBefore startTime frozenCutoff || Timestamp.isEqual startTime frozenCutoff
            let hasFrozenProtection = state.HasFrozenProtection || isFrozen

            let criticality =
                if endTime > req.DueDate then
                    ScheduleCriticality.CriticalPath
                elif runHours > 24.0M then
                    ScheduleCriticality.BottleneckConstrained
                else
                    ScheduleCriticality.Flexible

            let qty =
                Quantity.create req.Quantity
                |> Result.defaultValue Quantity.zero

            let activity: ProductionActivity =
                { ActivityId = actId
                  SequenceNumber = state.SequenceCounter
                  Item = req.ItemId
                  Quantity = qty
                  StartTime = startTime
                  EndTime = endTime
                  DurationHours = runHours
                  PrecedingActivityId = state.LastActivityIdOpt
                  SucceedingActivityId = None
                  CampaignId = None
                  Criticality = criticality
                  IsFrozen = isFrozen }

            { CurrentTime = endTime
              LastActivityIdOpt = Some actId
              LastItemOpt = Some req.ItemId
              ActivitiesRev = activity :: state.ActivitiesRev
              ChangeoversRev = newChangeoversRev
              SequenceCounter = state.SequenceCounter + 1
              HasLateJobs = hasLateJobs
              HasMaterialDelay = hasMaterialDelay
              HasFrozenProtection = hasFrozenProtection }
        ) initialState

    let activities = List.rev finalPlacement.ActivitiesRev
    let changeovers = List.rev finalPlacement.ChangeoversRev
    let hasLateJobs = finalPlacement.HasLateJobs
    let hasMaterialDelay = finalPlacement.HasMaterialDelay
    let hasFrozenProtection = finalPlacement.HasFrozenProtection

    // Stage 5: Packaging, Variance & Feasibility Evaluation
    let totalRunHours = activities |> List.sumBy (fun a -> a.DurationHours)
    let totalSetupHours = changeovers |> List.sumBy (fun c -> c.SetupHours)
    let totalScheduledHours = totalRunHours + totalSetupHours
    let baseCapacity = Math.Max(0.0M, input.Availability.BaseCapacityHours)
    let maxOvertime = Math.Max(0.0M, input.Availability.MaxOvertimeHours)
    let capacityVariance = totalScheduledHours - baseCapacity
    let overtimeRequired = Math.Max(0.0M, Math.Min(capacityVariance, maxOvertime))

    let stability = computeScheduleStability activities input.PriorSchedule p

    // Feasibility classification
    let capacityFeasibility =
        if capacityVariance <= 0.0M then
            DimensionFeasibility.Satisfied
        elif capacityVariance <= maxOvertime then
            DimensionFeasibility.Conditional $"Overtime of {capacityVariance:F2} hrs required within limit {maxOvertime:F2} hrs"
        else
            DimensionFeasibility.Violated $"Capacity overloaded by {capacityVariance:F2} hrs (exceeds max overtime {maxOvertime:F2} hrs)"

    let materialFeasibility =
        if hasMaterialDelay then
            DimensionFeasibility.Conditional "Production activities delayed to wait for material arrival"
        else
            DimensionFeasibility.Satisfied

    let maintenanceFeasibility = DimensionFeasibility.Satisfied
    let toolFeasibility = DimensionFeasibility.Satisfied
    let laborFeasibility = DimensionFeasibility.Satisfied
    let frozenFeasibility =
        if hasFrozenProtection then
            DimensionFeasibility.Satisfied
        else
            DimensionFeasibility.Satisfied

    let overallFeasibility =
        match capacityFeasibility with
        | DimensionFeasibility.Violated reason ->
            ScheduleFeasibility.Infeasible [ reason ]
        | DimensionFeasibility.Conditional cond ->
            let conditions = [ cond ] @ (if hasLateJobs then [ "Late delivery risk detected on one or more jobs" ] else [])
            ScheduleFeasibility.ConditionallyFeasible conditions
        | _ ->
            if hasLateJobs then
                ScheduleFeasibility.ConditionallyFeasible [ "Late delivery risk detected on one or more jobs" ]
            else
                ScheduleFeasibility.FullyFeasible

    let reasonCodes =
        [ SchedulingReasonCode.EarliestDueDateSequenced
          if totalSetupHours > 0.0M then SchedulingReasonCode.ChangeoverMinimized
          if hasMaterialDelay then SchedulingReasonCode.MaterialArrivalConstrained
          if hasFrozenProtection then SchedulingReasonCode.FrozenHorizonProtected
          if overtimeRequired > 0.0M then SchedulingReasonCode.OvertimeCapacityUtilized
          if hasLateJobs then SchedulingReasonCode.LateDeliveryRiskDetected
          if capacityVariance > maxOvertime then SchedulingReasonCode.CapacityOverloadInfeasible ]

    let feasibilityEval: FeasibilityEvaluation =
        { FiniteCapacity = capacityFeasibility
          MaterialAvailability = materialFeasibility
          ToolAvailability = toolFeasibility
          LaborAvailability = laborFeasibility
          MaintenanceAndCalendar = maintenanceFeasibility
          FrozenHorizon = frozenFeasibility
          Overall = overallFeasibility }

    let confidenceEval: ConfidenceEvaluation =
        { MaterialCertainty = ScheduleConfidence.High
          ResourceReliability = ScheduleConfidence.High
          ChangeoverAccuracy = ScheduleConfidence.High
          DemandStability = ScheduleConfidence.High
          Overall = ScheduleConfidence.High }

    { Activities = activities
      Changeovers = changeovers
      TotalScheduledHours = totalScheduledHours
      TotalChangeoverHours = totalSetupHours
      OvertimeHours = overtimeRequired
      Feasibility = feasibilityEval
      Confidence = confidenceEval
      Stability = stability
      ReasonCodes = reasonCodes }
