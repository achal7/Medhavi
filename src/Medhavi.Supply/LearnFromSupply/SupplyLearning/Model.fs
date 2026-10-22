// Medhavi.Supply.LearnFromSupply.SupplyLearning.Model
// Traceability: SE-S-130 Supply Learning, AB-S-120, AB-S-121, EV-S-120, EV-S-121
module Medhavi.Supply.LearnFromSupply.SupplyLearning.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

/// Lifecycle state of SE-S-130 Supply Learning aggregate (ARS Appendix A.2)
[<RequireQualifiedAccess>]
type LearningLifecycleState =
    | PendingOutcome
    | OutcomeRecorded
    | Superseded

    member this.AsString =
        match this with
        | PendingOutcome -> "PendingOutcome"
        | OutcomeRecorded -> "OutcomeRecorded"
        | Superseded -> "Superseded"

    static member FromString(s: string) : LearningLifecycleState =
        match s with
        | "PendingOutcome" -> PendingOutcome
        | "OutcomeRecorded" -> OutcomeRecorded
        | "Superseded" -> Superseded
        | _ -> PendingOutcome

/// Governance outcome determination (DE-S-121)
[<RequireQualifiedAccess>]
type LearningOutcome =
    | Pending
    | Accepted
    | Rejected
    | Deferred

    member this.AsString =
        match this with
        | Pending -> "Pending"
        | Accepted -> "Accepted"
        | Rejected -> "Rejected"
        | Deferred -> "Deferred"

    static member FromString(s: string) : LearningOutcome =
        match s with
        | "Accepted" -> Accepted
        | "Rejected" -> Rejected
        | "Deferred" -> Deferred
        | _ -> Pending

/// Qualitative confidence levels backed by continuous index scores
[<RequireQualifiedAccess>]
type ConfidenceLevel =
    | High
    | Medium
    | Low

    member this.AsString =
        match this with
        | High -> "High"
        | Medium -> "Medium"
        | Low -> "Low"

    member this.NumericScore =
        match this with
        | High -> 1.0m
        | Medium -> 0.70m
        | Low -> 0.40m

    static member FromString(s: string) : ConfidenceLevel =
        match s with
        | "High" -> High
        | "Medium" -> Medium
        | "Low" -> Low
        | _ -> Low

/// Governed taxonomy of recognized Supply Learning Types (SE-C-037)
[<RequireQualifiedAccess>]
type SupplyLearningType =
    | PlanStabilityAndNervousness
    | BottleneckPersistence
    | SupplierReliabilityDrift
    | InventoryPositionChurn
    | RecurringExceptionPattern
    | PlannerOverrideEffectiveness
    | FrozenZoneViolations
    | DistributionImbalance
    | Custom of string

    member this.AsString =
        match this with
        | PlanStabilityAndNervousness -> "PlanStabilityAndNervousness"
        | BottleneckPersistence -> "BottleneckPersistence"
        | SupplierReliabilityDrift -> "SupplierReliabilityDrift"
        | InventoryPositionChurn -> "InventoryPositionChurn"
        | RecurringExceptionPattern -> "RecurringExceptionPattern"
        | PlannerOverrideEffectiveness -> "PlannerOverrideEffectiveness"
        | FrozenZoneViolations -> "FrozenZoneViolations"
        | DistributionImbalance -> "DistributionImbalance"
        | Custom name -> name

    static member FromString(s: string) : SupplyLearningType =
        match s with
        | "PlanStabilityAndNervousness" -> PlanStabilityAndNervousness
        | "BottleneckPersistence" -> BottleneckPersistence
        | "SupplierReliabilityDrift" -> SupplierReliabilityDrift
        | "InventoryPositionChurn" -> InventoryPositionChurn
        | "RecurringExceptionPattern" -> RecurringExceptionPattern
        | "PlannerOverrideEffectiveness" -> PlannerOverrideEffectiveness
        | "FrozenZoneViolations" -> FrozenZoneViolations
        | "DistributionImbalance" -> DistributionImbalance
        | other -> Custom other

/// Enterprise observation scope
[<RequireQualifiedAccess>]
type LearningScope =
    | Enterprise
    | BusinessUnit of string
    | ProductFamily of string
    | SupplierNetwork of string
    | DistributionNetwork of string

    member this.AsString =
        match this with
        | Enterprise -> "Enterprise"
        | BusinessUnit name -> $"BusinessUnit:{name}"
        | ProductFamily name -> $"ProductFamily:{name}"
        | SupplierNetwork name -> $"SupplierNetwork:{name}"
        | DistributionNetwork name -> $"DistributionNetwork:{name}"

    static member FromString(s: string) : LearningScope =
        if String.IsNullOrWhiteSpace s || s = "Enterprise" then
            Enterprise
        elif s.StartsWith("BusinessUnit:", StringComparison.OrdinalIgnoreCase) then
            BusinessUnit(s.Substring(13).Trim())
        elif s.StartsWith("ProductFamily:", StringComparison.OrdinalIgnoreCase) then
            ProductFamily(s.Substring(14).Trim())
        elif s.StartsWith("SupplierNetwork:", StringComparison.OrdinalIgnoreCase) then
            SupplierNetwork(s.Substring(16).Trim())
        elif s.StartsWith("DistributionNetwork:", StringComparison.OrdinalIgnoreCase) then
            DistributionNetwork(s.Substring(20).Trim())
        else
            Enterprise

/// Historical evidence reference spanning multiple periods
type HistoricalEvidenceReference =
    {
        CapabilityId: string
        ArtifactType: string
        ArtifactId: string
        PeriodId: EvaluationPeriodId
        PeriodWindow: TemporalWindow
        SummaryMetrics: Map<string, decimal>
    }

/// Actionable improvement opportunity derived from the learning
type ImprovementOpportunity =
    {
        OpportunityId: string
        TargetCapability: string
        TargetPolicyId: string
        TargetParameter: string
        CurrentBaselineValue: string
        RecommendedValue: string
        ExpectedBenefit: string
        InterventionConfidence: ConfidenceLevel
        AnalogueSuccessRate: decimal
        OperationalFeasibility: decimal
    }

/// SE-S-130 Supply Learning Aggregate Root (Immutable Continuous Assessment Record)
type SupplyLearning =
    {
        Id: SupplyLearningId
        SubjectType: string
        SubjectId: string
        LearningType: SupplyLearningType
        Version: int64
        LifecycleState: LearningLifecycleState
        PatternDescription: string
        RecurrenceCount: int
        RecurrenceFrequency: decimal
        StatisticalSignificance: decimal
        Scope: LearningScope
        TimeHorizon: TemporalWindow
        LearningStatement: string
        PatternConfidence: ConfidenceLevel
        PatternConfidenceScore: decimal
        ImprovementOpportunities: ImprovementOpportunity list
        Outcome: LearningOutcome
        OutcomeTimestamp: Timestamp option
        OutcomeAuthority: string option
        OutcomeRationale: string option
        SupportingEvidence: HistoricalEvidenceReference list
        SourceEvidenceVersionSet: string list
        PolicyVersion: string
        GenerationTimestamp: Timestamp
        SupersededLearningId: SupplyLearningId option
    }

// =========================================================================
// Aggregate Commands
// =========================================================================

/// AB-S-120 Command: Record Supply Learning in Pending Outcome state
type RecordSupplyLearningCmd =
    {
        LearningId: SupplyLearningId
        SubjectType: string
        SubjectId: string
        LearningType: SupplyLearningType
        Scope: LearningScope
        TimeHorizon: TemporalWindow
        PatternDescription: string
        RecurrenceCount: int
        RecurrenceFrequency: decimal
        StatisticalSignificance: decimal
        LearningStatement: string
        PatternConfidence: ConfidenceLevel
        PatternConfidenceScore: decimal
        ImprovementOpportunities: ImprovementOpportunity list
        SupportingEvidence: HistoricalEvidenceReference list
        SourceEvidenceVersionSet: string list
        PolicyVersion: string
        Timestamp: Timestamp
    }

/// AB-S-121 Command: Record Governance Outcome on a candidate Supply Learning
type RecordLearningOutcomeCmd =
    {
        LearningId: SupplyLearningId
        Outcome: LearningOutcome
        Authority: string
        Rationale: string option
        Timestamp: Timestamp
    }

// =========================================================================
// Enterprise Events (EV-S-120 & EV-S-121)
// =========================================================================

type OutcomePayload =
    {
        LearningId: SupplyLearningId
        Outcome: LearningOutcome
        Authority: string
        Rationale: string option
        Timestamp: Timestamp
    }

type SupersedePayload =
    {
        LearningId: SupplyLearningId
        SupersededByLearningId: SupplyLearningId
        Timestamp: Timestamp
    }

/// Enterprise Events emitted by SE-S-130 Aggregate Root
type SupplyLearningEvent =
    | SupplyLearningRecorded of SupplyLearning
    | SupplyLearningOutcomeRecorded of OutcomePayload
    | SupplyLearningSuperseded of SupersedePayload

// =========================================================================
// Pure Evolution (Catamorphism)
// =========================================================================

let evolve: Medhavi.Foundation.Contracts.Evolve<SupplyLearning, SupplyLearningEvent> =
    fun (stateOpt: SupplyLearning option) (event: SupplyLearningEvent) ->
        match event with
        | SupplyLearningRecorded learning -> Some learning

        | SupplyLearningOutcomeRecorded payload ->
            match stateOpt with
            | Some current ->
                Some
                    { current with
                        LifecycleState = LearningLifecycleState.OutcomeRecorded
                        Outcome = payload.Outcome
                        OutcomeTimestamp = Some payload.Timestamp
                        OutcomeAuthority = Some payload.Authority
                        OutcomeRationale = payload.Rationale
                    }
            | None -> None

        | SupplyLearningSuperseded payload ->
            match stateOpt with
            | Some current ->
                Some
                    { current with
                        LifecycleState = LearningLifecycleState.Superseded
                    }
            | None -> None

/// Rehydrate aggregate state by folding events
let replay (events: SupplyLearningEvent seq) : SupplyLearning option = Seq.fold evolve None events
