/// SE-S-040 — Capacity Position Assessment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-040, AB-S-030, EV-S-030)
module Medhavi.Supply.ManageCapacity.CapacityPosition.Model

open Medhavi.SemanticModel
open Medhavi.Supply

type PositionLifecycleState =
    | Active
    | Archived

type PolicyComplianceStatus =
    | Conforming
    | NonConforming of reasons: string list

/// Multi-tier capacity operating envelope derived from BA-S-011
type CapacityOperatingEnvelope =
    { MinEconomicCapacity: decimal
      TargetLowCapacity: decimal
      TargetHighCapacity: decimal
      OvertimeCapacity: decimal
      MaxPlannedCapacity: decimal
      AbsolutePhysicalCapacity: decimal }

/// Time-phased capacity bucket to prevent deceptive period averaging
type CapacityPeriodBucket =
    { BucketIndex: int
      StartTime: Timestamp
      EndTime: Timestamp
      ScheduledOperatingHours: decimal
      DowntimeHours: decimal
      AvailableHours: decimal
      CommittedPlannedHours: decimal
      ProjectedUtilization: decimal
      UtilizationStatus: CapacityUtilizationStatus
      ConstraintStatus: CapacityConstraintStatus
      BottleneckStatus: BottleneckAssessmentStatus }

/// Child entity: immutable change event per ARS Continuous Assessment pattern (ARS Appendix A.2)
type CapacityAssessmentChangeEvent =
    { SequenceNumber: int
      ChangeTimestamp: Timestamp
      PreviousUtilizationStatus: CapacityUtilizationStatus
      NewUtilizationStatus: CapacityUtilizationStatus
      PreviousConstraintStatus: CapacityConstraintStatus
      NewConstraintStatus: CapacityConstraintStatus
      PreviousBottleneckStatus: BottleneckAssessmentStatus
      NewBottleneckStatus: BottleneckAssessmentStatus
      TriggeringEvidence: string list
      ReasonCode: string }

/// SE-S-040 Aggregate Root
type CapacityPositionAssessment =
    { ResourceId: PhysicalResourceId
      LifecycleState: PositionLifecycleState
      ScheduledOperatingTimeHours: decimal
      TotalDowntimeHours: decimal
      AvailableCapacityHours: decimal
      OperatingEnvelope: CapacityOperatingEnvelope
      CurrentCommittedHours: decimal
      ProjectedPlannedHours: decimal
      EffectiveUtilization: decimal
      UtilizationStatus: CapacityUtilizationStatus
      ConstraintStatus: CapacityConstraintStatus
      BottleneckStatus: BottleneckAssessmentStatus
      RiskLevel: CapacityRiskLevel
      PolicyCompliance: PolicyComplianceStatus
      HasSinglePointOfFailureRisk: bool
      HasAlternateResource: bool
      Buckets: CapacityPeriodBucket list
      SourceSupplyUnderstandingVersion: int option
      SourceSupplyPlanVersion: int option
      AssessmentTimestamp: Timestamp
      ChangeHistory: CapacityAssessmentChangeEvent list }

type UpdateCapacityPositionCmd =
    { ResourceId: PhysicalResourceId
      TriggeringEvidence: string list
      Timestamp: Timestamp }

/// EV-S-030: Capacity Position Assessment Changed
type CapacityPositionEvent =
    | CapacityPositionAssessmentChanged of assessment: CapacityPositionAssessment * changeEventOpt: CapacityAssessmentChangeEvent option

let evolve (stateOpt: CapacityPositionAssessment option) (event: CapacityPositionEvent) : CapacityPositionAssessment option =
    match event with
    | CapacityPositionAssessmentChanged(assessment, _) -> Some assessment

let replay (events: CapacityPositionEvent seq) : CapacityPositionAssessment option =
    Seq.fold evolve None events
