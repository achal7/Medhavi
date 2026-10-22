module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Model

open Medhavi.SemanticModel
open Medhavi.Supply

// =============================================================================
// SE-S-110 — Supply Exception Assessment Aggregate Model
// Governed by: PO-S-120 Exception Detection Policy, ARS Appendix A.2
// Traces to: SE-S-110, AB-S-100, EV-S-100, EV-S-101, EV-S-102
// =============================================================================

/// Governed lifecycle states of a Supply Exception Assessment
[<RequireQualifiedAccess>]
type SupplyExceptionLifecycleState =
    | Detected
    | Confirmed
    | Resolved
    | Archived

/// Governed scope of a supply exception
[<RequireQualifiedAccess>]
type ExceptionScope =
    | ItemScope of Item: ItemId * Location: LocationId option
    | LocationScope of Location: LocationId
    | SupplierScope of Supplier: SupplierId
    | ResourceScope of Resource: PhysicalResourceId
    | PlanningScope of Scope: PlanningScopeId
    | EnterpriseScope

module ExceptionScope =
    let toKey (scope: ExceptionScope) =
        match scope with
        | ExceptionScope.ItemScope(item, Some loc) -> $"Item::{ItemId.value item}::{LocationId.value loc}"
        | ExceptionScope.ItemScope(item, None) -> $"Item::{ItemId.value item}"
        | ExceptionScope.LocationScope loc -> $"Location::{LocationId.value loc}"
        | ExceptionScope.SupplierScope sup -> $"Supplier::{SupplierId.value sup}"
        | ExceptionScope.ResourceScope res -> $"Resource::{PhysicalResourceId.value res}"
        | ExceptionScope.PlanningScope scp -> $"Scope::{PlanningScopeId.value scp}"
        | ExceptionScope.EnterpriseScope -> "Enterprise"

    let entityTypeAndId (scope: ExceptionScope) : (string * string) =
        match scope with
        | ExceptionScope.ItemScope(item, _) -> ("Item", ItemId.value item)
        | ExceptionScope.LocationScope loc -> ("Location", LocationId.value loc)
        | ExceptionScope.SupplierScope sup -> ("Supplier", SupplierId.value sup)
        | ExceptionScope.ResourceScope res -> ("Resource", PhysicalResourceId.value res)
        | ExceptionScope.PlanningScope scp -> ("PlanningScope", PlanningScopeId.value scp)
        | ExceptionScope.EnterpriseScope -> ("Enterprise", "Global")

/// Multi-dimensional severity vector per PO-S-120
type SeverityAssessment =
    {
        BusinessImpact: SeverityBusinessImpact
        Urgency: SeverityUrgency
        Scope: SeverityScope
        PotentialImpact: SeverityPotentialImpact
        CurrentImpact: SeverityCurrentImpact
    }

/// Reference to upstream assessment providing risk or disruption signals
type EvidenceReference =
    {
        AssessmentId: string
        AssessmentType: string
        Description: string
        Confidence: decimal
        Timestamp: Timestamp
    }

/// Immutable state change audit log entry appended to assessment history (BR-S-273)
type StateChangeEvent =
    {
        EventId: string
        PreviousState: SupplyExceptionLifecycleState option
        NewState: SupplyExceptionLifecycleState
        TriggeringEvidence: string
        Rationale: string
        Timestamp: Timestamp
    }

/// SE-S-110 Supply Exception Assessment aggregate root
type SupplyExceptionAssessment =
    {
        Id: SupplyExceptionId
        GovernedPolicyReference: string
        ViolatedRuleId: string
        ExceptionType: ExceptionType
        ExceptionScope: ExceptionScope
        PlanningScopeId: PlanningScopeId
        LifecycleState: SupplyExceptionLifecycleState
        CurrentSeverity: SeverityAssessment
        ExceptionHorizon: ExceptionHorizon
        DetectionTimestamp: Timestamp
        ResolutionTimestamp: Timestamp option
        EvidenceReferences: EvidenceReference list
        EvaluationConfidence: ExceptionConfidence
        ReasonCodes: string list
        CorrelatedExceptionIds: SupplyExceptionId list
        StateChangeHistory: StateChangeEvent list
        CreatedTimestamp: Timestamp
        UpdatedTimestamp: Timestamp
    }

// ---------- Commands targeting SE-S-110 ----------

[<RequireQualifiedAccess>]
type SupplyExceptionCommand =
    | DetectException of
        {|
            Id: SupplyExceptionId
            GovernedPolicyReference: string
            ViolatedRuleId: string
            ExceptionType: ExceptionType
            ExceptionScope: ExceptionScope
            PlanningScopeId: PlanningScopeId
            Severity: SeverityAssessment
            Horizon: ExceptionHorizon
            Evidence: EvidenceReference list
            Confidence: ExceptionConfidence
            ReasonCodes: string list
            CorrelatedExceptionIds: SupplyExceptionId list
            Rationale: string
            Timestamp: Timestamp
        |}
    | CorroborateException of
        {|
            Id: SupplyExceptionId
            CorroboratingEvidence: EvidenceReference list
            Severity: SeverityAssessment
            Horizon: ExceptionHorizon
            Confidence: ExceptionConfidence
            ReasonCodes: string list
            Rationale: string
            Timestamp: Timestamp
        |}
    | UpdateExceptionEvidence of
        {|
            Id: SupplyExceptionId
            UpdatedEvidence: EvidenceReference list
            Severity: SeverityAssessment
            Horizon: ExceptionHorizon
            Confidence: ExceptionConfidence
            ReasonCodes: string list
            Rationale: string
            Timestamp: Timestamp
        |}
    | ResolveException of
        {|
            Id: SupplyExceptionId
            ResolutionEvidence: EvidenceReference list
            Rationale: string
            Timestamp: Timestamp
        |}
    | ArchiveException of
        {|
            Id: SupplyExceptionId
            Rationale: string
            Timestamp: Timestamp
        |}

// ---------- Enterprise Domain Events for SE-S-110 ----------

[<RequireQualifiedAccess>]
type SupplyExceptionEvent =
    | SupplyExceptionDetected of Snapshot: SupplyExceptionAssessment
    | SupplyExceptionConfirmed of Snapshot: SupplyExceptionAssessment * EscalatedToCore: bool
    | SupplyExceptionEvidenceUpdated of Snapshot: SupplyExceptionAssessment
    | SupplyExceptionResolved of Snapshot: SupplyExceptionAssessment * WasEscalatedToCore: bool
    | SupplyExceptionArchived of Snapshot: SupplyExceptionAssessment

module SupplyExceptionEvent =
    let snapshot (evt: SupplyExceptionEvent) : SupplyExceptionAssessment =
        match evt with
        | SupplyExceptionEvent.SupplyExceptionDetected s -> s
        | SupplyExceptionEvent.SupplyExceptionConfirmed(s, _) -> s
        | SupplyExceptionEvent.SupplyExceptionEvidenceUpdated s -> s
        | SupplyExceptionEvent.SupplyExceptionResolved(s, _) -> s
        | SupplyExceptionEvent.SupplyExceptionArchived s -> s

    let aggregateId (evt: SupplyExceptionEvent) : SupplyExceptionId = (snapshot evt).Id

// ---------- Pure Evolve Function (Canonical Foundation Catamorphism) ----------

let evolve (state: SupplyExceptionAssessment option) (event: SupplyExceptionEvent) : SupplyExceptionAssessment option =
    match event with
    | SupplyExceptionEvent.SupplyExceptionDetected s -> Some s
    | SupplyExceptionEvent.SupplyExceptionConfirmed(s, _) -> Some s
    | SupplyExceptionEvent.SupplyExceptionEvidenceUpdated s -> Some s
    | SupplyExceptionEvent.SupplyExceptionResolved(s, _) -> Some s
    | SupplyExceptionEvent.SupplyExceptionArchived s -> Some s

let replay (events: SupplyExceptionEvent seq) : SupplyExceptionAssessment option = Seq.fold evolve None events
