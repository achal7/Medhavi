/// SE-S-040 — Capacity Position Projections
/// Maps pure domain models to Medhavi.Contracts.Supply DTOs and provides projection reducers
module Medhavi.Supply.ManageCapacity.CapacityPosition.Projections

open Medhavi.Contracts.Supply
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
let mapEnvelopeToDto (env: CapacityOperatingEnvelope) : CapacityOperatingEnvelopeDto =
    { TargetLowerHours = env.TargetLowCapacity
      TargetUpperHours = env.TargetHighCapacity
      OvertimeThresholdHours = env.OvertimeCapacity
      MaxPlannedHours = env.MaxPlannedCapacity
      AbsoluteCapacityHours = env.AbsolutePhysicalCapacity
      MinEconomicHours = env.MinEconomicCapacity }

let mapUtilizationStatus (status: CapacityUtilizationStatus) : Medhavi.Contracts.Supply.CapacityUtilizationStatus =
    match status with
    | CapacityUtilizationStatus.Underutilized -> Medhavi.Contracts.Supply.CapacityUtilizationStatus.Underutilized
    | CapacityUtilizationStatus.Optimal -> Medhavi.Contracts.Supply.CapacityUtilizationStatus.Optimal
    | CapacityUtilizationStatus.NearCapacity -> Medhavi.Contracts.Supply.CapacityUtilizationStatus.NearCapacity
    | CapacityUtilizationStatus.Overloaded -> Medhavi.Contracts.Supply.CapacityUtilizationStatus.Overloaded

let mapConstraintStatus (status: CapacityConstraintStatus) : Medhavi.Contracts.Supply.CapacityConstraintStatus =
    match status with
    | CapacityConstraintStatus.NonConstraint -> Medhavi.Contracts.Supply.CapacityConstraintStatus.NonConstraint
    | CapacityConstraintStatus.NearConstraint -> Medhavi.Contracts.Supply.CapacityConstraintStatus.NearConstraint
    | CapacityConstraintStatus.ActiveConstraint -> Medhavi.Contracts.Supply.CapacityConstraintStatus.ActiveConstraint

let mapBottleneckStatus (status: BottleneckAssessmentStatus) : Medhavi.Contracts.Supply.BottleneckAssessmentStatus =
    match status with
    | BottleneckAssessmentStatus.NotABottleneck -> Medhavi.Contracts.Supply.BottleneckAssessmentStatus.NotABottleneck
    | BottleneckAssessmentStatus.PotentialBottleneck -> Medhavi.Contracts.Supply.BottleneckAssessmentStatus.PotentialBottleneck
    | BottleneckAssessmentStatus.CurrentBottleneck -> Medhavi.Contracts.Supply.BottleneckAssessmentStatus.CurrentBottleneck

let mapRiskLevel (level: CapacityRiskLevel) : Medhavi.Contracts.Supply.CapacityRiskLevel =
    match level with
    | CapacityRiskLevel.Negligible -> Medhavi.Contracts.Supply.CapacityRiskLevel.Negligible
    | CapacityRiskLevel.Low -> Medhavi.Contracts.Supply.CapacityRiskLevel.Low
    | CapacityRiskLevel.Medium -> Medhavi.Contracts.Supply.CapacityRiskLevel.Medium
    | CapacityRiskLevel.High -> Medhavi.Contracts.Supply.CapacityRiskLevel.High
    | CapacityRiskLevel.Critical -> Medhavi.Contracts.Supply.CapacityRiskLevel.Critical

let mapBucketToDto (b: CapacityPeriodBucket) (envelopeDto: CapacityOperatingEnvelopeDto) : CapacityPeriodBucketDto =
    let healthState =
        if b.UtilizationStatus = CapacityUtilizationStatus.Overloaded || b.BottleneckStatus = BottleneckAssessmentStatus.CurrentBottleneck then
            Medhavi.Contracts.Supply.CapacityHealthState.CriticallyConstrained
        elif b.ConstraintStatus = CapacityConstraintStatus.ActiveConstraint then
            Medhavi.Contracts.Supply.CapacityHealthState.Constrained
        elif b.UtilizationStatus = CapacityUtilizationStatus.Underutilized then
            Medhavi.Contracts.Supply.CapacityHealthState.Underutilized
        elif b.UtilizationStatus = CapacityUtilizationStatus.NearCapacity then
            Medhavi.Contracts.Supply.CapacityHealthState.AtRisk
        else
            Medhavi.Contracts.Supply.CapacityHealthState.Balanced

    let bucketDto: CapacityPeriodBucketDto =
        { BucketIndex = b.BucketIndex
          PeriodStart = Timestamp.value b.StartTime
          PeriodEnd = Timestamp.value b.EndTime
          AvailableCapacityHours = b.AvailableHours
          PlannedLoadHours = b.CommittedPlannedHours
          UtilizationPercentage = b.ProjectedUtilization * 100.0m
          OperatingEnvelope = envelopeDto
          UtilizationStatus = mapUtilizationStatus b.UtilizationStatus
          ConstraintStatus = mapConstraintStatus b.ConstraintStatus
          BottleneckStatus = mapBottleneckStatus b.BottleneckStatus
          HealthState = healthState }
    bucketDto

let mapChangeEventToDto (ev: CapacityAssessmentChangeEvent) : CapacityAssessmentChangeEventDto =
    let changeDto: CapacityAssessmentChangeEventDto =
        { SequenceNumber = ev.SequenceNumber
          ChangeTimestamp = Timestamp.value ev.ChangeTimestamp
          PreviousUtilizationStatus = mapUtilizationStatus ev.PreviousUtilizationStatus
          NewUtilizationStatus = mapUtilizationStatus ev.NewUtilizationStatus
          PreviousConstraintStatus = mapConstraintStatus ev.PreviousConstraintStatus
          NewConstraintStatus = mapConstraintStatus ev.NewConstraintStatus
          PreviousBottleneckAssessment = mapBottleneckStatus ev.PreviousBottleneckStatus
          NewBottleneckAssessment = mapBottleneckStatus ev.NewBottleneckStatus
          TriggeringEvidence = ev.TriggeringEvidence
          ReasonCode = ev.ReasonCode }
    changeDto

let mapToDto (a: CapacityPositionAssessment) : CapacityPositionAssessmentDto =
    let envDto = mapEnvelopeToDto a.OperatingEnvelope
    let underloadRisk =
        if a.UtilizationStatus = CapacityUtilizationStatus.Underutilized then
            Medhavi.Contracts.Supply.CapacityRiskLevel.Medium
        else
            Medhavi.Contracts.Supply.CapacityRiskLevel.Low

    let dto: CapacityPositionAssessmentDto =
        { PhysicalResourceId = PhysicalResourceId.value a.ResourceId
          LifecycleState = sprintf "%A" a.LifecycleState
          AvailableCapacityHours = a.AvailableCapacityHours
          CurrentLoadHours = a.CurrentCommittedHours
          CurrentUtilizationPercentage = (if a.AvailableCapacityHours > 0m then (a.CurrentCommittedHours / a.AvailableCapacityHours) * 100.0m else 0m)
          ProjectedLoadHours = a.ProjectedPlannedHours
          ProjectedUtilizationPercentage = a.EffectiveUtilization * 100.0m
          OperatingEnvelope = envDto
          TimePhasedBuckets = a.Buckets |> List.map (fun b -> mapBucketToDto b envDto)
          UtilizationStatus = mapUtilizationStatus a.UtilizationStatus
          ConstraintStatus = mapConstraintStatus a.ConstraintStatus
          BottleneckAssessment = mapBottleneckStatus a.BottleneckStatus
          OverloadRisk = mapRiskLevel a.RiskLevel
          UnderloadRisk = underloadRisk
          SinglePointOfFailureRisk = a.HasSinglePointOfFailureRisk
          AssessmentConfidence = "High"
          SourceSupplyUnderstandingVersion = a.SourceSupplyUnderstandingVersion
          SourceSupplyPlanVersion = a.SourceSupplyPlanVersion
          AssessmentTimestamp = Timestamp.value a.AssessmentTimestamp
          ChangeHistory = a.ChangeHistory |> List.map mapChangeEventToDto }
    dto

let seedFromAggregates (aggregates: CapacityPositionAssessment list) : Map<PhysicalResourceId, CapacityPositionAssessmentDto> =
    aggregates
    |> List.map (fun a -> a.ResourceId, mapToDto a)
    |> Map.ofList

let apply (state: Map<PhysicalResourceId, CapacityPositionAssessmentDto>) (event: CapacityPositionEvent) : Map<PhysicalResourceId, CapacityPositionAssessmentDto> =
    match event with
    | CapacityPositionAssessmentChanged(assessment, _) ->
        Map.add assessment.ResourceId (mapToDto assessment) state
