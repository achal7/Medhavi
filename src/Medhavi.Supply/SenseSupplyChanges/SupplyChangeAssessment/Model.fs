/// SE-S-090 — Supply Change Assessment Domain Model
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-090, AB-S-080, EV-S-080)
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Model

open System
open Medhavi.SemanticModel
open Medhavi.Supply

[<RequireQualifiedAccess>]
type SupplyEntityType =
    | Supplier
    | PhysicalResource
    | TransportationLane
    | Location
    | Item

[<RequireQualifiedAccess>]
type ChangeType =
    | QuantityReduction
    | QuantityIncrease
    | DeliveryDelay
    | DeliveryExpedite
    | CapacityReduction
    | CapacityIncrease
    | Disruption
    | QualityIssue
    | PriceChange
    | RegulatoryChange

[<RequireQualifiedAccess>]
type ChangeEpistemicState =
    | NoChange
    | Observed
    | Corroborated
    | Confirmed
    | Resolved

[<RequireQualifiedAccess>]
type ChangeClassification =
    | Planned
    | Unplanned

[<RequireQualifiedAccess>]
type PlanningMateriality =
    | MaterialToPlanning
    | NonMaterial
    | Unassessed

[<RequireQualifiedAccess>]
type AssessmentConfidence =
    | Confirmed
    | High
    | Medium
    | Low
    | Unconfirmed

[<RequireQualifiedAccess>]
type SupplyChangeLifecycleState =
    | Active
    | Archived

/// Immutable Assessment Change Event in history (ARS §4.3.1)
type AssessmentChangeEvent =
    {
        EventId: string
        PreviousEpistemicState: ChangeEpistemicState
        NewEpistemicState: ChangeEpistemicState
        TriggeringEvidence: string
        ReasonCode: string
        Timestamp: Timestamp
    }

/// Structured incoming supply signal
type IncomingSupplySignal =
    {
        SignalId: string
        EntityReference: string
        SourceId: string
        SourceType: string
        SourceReliabilityPrior: decimal
        ObservedQuantity: decimal
        ObservedDate: Timestamp
        SignalGenerationTime: Timestamp
    }

/// Signal summary embedded in assessment
type SignalSummary =
    {
        SignalId: string
        SourceId: string
        SourceType: string
        SourceReliabilityPrior: decimal
        ObservedQuantity: decimal
        ObservedDate: Timestamp
        SignalGenerationTime: Timestamp
    }

/// Authoritative baseline entity fact from current supply understanding
type BaselineEntityFact =
    {
        EntityReference: string
        CommittedDeliveryDate: Timestamp
        CommittedQuantity: decimal
        IsCriticalPath: bool
        BufferDays: decimal
    }

/// SE-S-090: Supply Change Assessment Aggregate Root
type SupplyChangeAssessmentAggregate =
    {
        AssessmentId: SupplyChangeAssessmentId
        MonitoredEntityType: SupplyEntityType
        MonitoredEntityId: string
        ChangeType: ChangeType
        CurrentEpistemicState: ChangeEpistemicState
        ChangeClassification: ChangeClassification
        ChangeDescription: string
        Materiality: PlanningMateriality
        OccurrenceTime: Timestamp
        DetectionTime: Timestamp
        ExpectedDuration: TimeSpan option
        SignalSummary: SignalSummary
        AssessmentConfidence: AssessmentConfidence
        DaysVariance: decimal
        QuantityVariance: decimal
        QuantityVariancePercent: decimal
        ThreatenedObjectives: string list
        CorrelatedChangeIds: SupplyChangeAssessmentId list
        History: AssessmentChangeEvent list
        LifecycleState: SupplyChangeLifecycleState
        CreatedTimestamp: Timestamp
        UpdatedTimestamp: Timestamp
    }

/// Commands for AB-S-080
type AssessSupplyChangeCmd =
    {
        AssessmentId: SupplyChangeAssessmentId
        MonitoredEntityType: SupplyEntityType
        MonitoredEntityId: string
        ChangeType: ChangeType
        Signal: IncomingSupplySignal
        Baseline: BaselineEntityFact
        CorroboratingSignalsCount: int
        Classification: ChangeClassification
        ExpectedDuration: TimeSpan option
        CorrelatedChangeIds: SupplyChangeAssessmentId list
        Timestamp: Timestamp
    }

type ArchiveSupplyChangeCmd =
    {
        AssessmentId: SupplyChangeAssessmentId
        Reason: string
        Timestamp: Timestamp
    }

/// Domain Events for EV-S-080
type SupplyChangeEvent =
    | SupplyChangeAssessmentCreated of SupplyChangeAssessmentAggregate
    | SupplyChangeAssessmentUpdated of SupplyChangeAssessmentAggregate * ChangeEvent: AssessmentChangeEvent
    | SupplyChangeAssessmentArchived of SupplyChangeAssessmentAggregate * Reason: string

/// Pure catamorphism for state evolution (zero side-effects)
let evolve
    (stateOpt: SupplyChangeAssessmentAggregate option)
    (event: SupplyChangeEvent)
    : SupplyChangeAssessmentAggregate option =
    match event with
    | SupplyChangeAssessmentCreated agg -> Some agg
    | SupplyChangeAssessmentUpdated(agg, _) -> Some agg
    | SupplyChangeAssessmentArchived(agg, _) -> Some agg

/// Catamorphic replay
let replay (events: SupplyChangeEvent seq) : SupplyChangeAssessmentAggregate option = Seq.fold evolve None events
