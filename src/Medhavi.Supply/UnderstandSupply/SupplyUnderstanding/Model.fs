/// SE-S-001 — Supply Understanding Aggregate Model
/// Traces to: Supply Intelligence Specification (SE-S-001, AB-S-003, AB-S-005)
module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Model

open Medhavi.SemanticModel
open Medhavi.Supply

/// Supply Continuity Interpretation status
type ContinuityStatus =
    | Stable
    | AtRisk
    | Disrupted

/// Capacity Adequacy Interpretation status
type CapacityAdequacyStatus =
    | Sufficient
    | Constrained
    | Critical

/// Inventory Position Interpretation status
type InventoryPositionStatus =
    | Healthy
    | AtRisk
    | Critical

/// Supplier Commitment Posture Interpretation status
type SupplierCommitmentPostureStatus =
    | HighConfidence
    | MediumConfidence
    | LowConfidence

/// Overall Supply Risk level
type OverallSupplyRiskLevel =
    | Low
    | Medium
    | High
    | Critical

/// Supply Understanding Confidence level
type ConfidenceLevel =
    | High
    | Medium
    | Low

/// BR-S-015b — A dimension may carry an Incomplete status when evidence is unavailable
type InterpretationStatus<'T> =
    | Known of 'T
    | Incomplete of reason: string

/// SE-S-001 — the six-dimensional supply interpretation
type Interpretation =
    { Continuity: InterpretationStatus<ContinuityStatus>
      ContinuityDrivers: string list
      CapacityAdequacy: InterpretationStatus<CapacityAdequacyStatus>
      Bottlenecks: string list
      InventoryPosition: InterpretationStatus<InventoryPositionStatus>
      StockoutRiskItems: string list
      ExcessInventoryItems: string list
      SupplierPosture: InterpretationStatus<SupplierCommitmentPostureStatus>
      CriticalDependencies: string list
      OverallRisk: InterpretationStatus<OverallSupplyRiskLevel>
      RiskDrivers: string list
      Confidence: InterpretationStatus<ConfidenceLevel>
      ConfidenceFactors: string list
      ReasonCodes: string list }

module Interpretation =
    /// BR-S-015 — completeness fraction over the six mandatory interpretation dimensions
    let completenessRatio (i: Interpretation) : decimal =
        let isComplete (s: InterpretationStatus<'T>) =
            match s with
            | Known _ -> true
            | Incomplete _ -> false

        let completeCount =
            (if isComplete i.Continuity then 1 else 0)
            + (if isComplete i.CapacityAdequacy then 1 else 0)
            + (if isComplete i.InventoryPosition then 1 else 0)
            + (if isComplete i.SupplierPosture then 1 else 0)
            + (if isComplete i.OverallRisk then 1 else 0)
            + (if isComplete i.Confidence then 1 else 0)

        decimal completeCount / 6.0m

/// Version lifecycle state
type VersionState =
    | Draft
    | Published
    | Superseded

/// SE-S-001 — Supply Understanding Version entity
type SupplyUnderstandingVersion =
    { VersionNumber: int
      Interpretation: Interpretation
      EvidencePictureVersion: int64
      SupplyChangeAssessmentReferences: string list
      TransactionTime: Timestamp
      PublicationTime: Timestamp option
      State: VersionState }

/// SE-S-001 — Supply Understanding aggregate root
type SupplyUnderstanding =
    { PlanningScopeId: PlanningScopeId
      Versions: SupplyUnderstandingVersion list
      CurrentPublishedVersion: int option }

/// AB-S-003 — Revise command carrying picture supply facts and change signals
type ReviseCmd =
    { PlanningScopeId: PlanningScopeId
      PictureSupplyFacts: PictureSupplyFacts
      SupplyChangeAssessments: string list
      TransactionTime: Timestamp }

/// AB-S-005 — Publish command
type PublishCmd =
    { PlanningScopeId: PlanningScopeId
      IsPeriodicRefresh: bool
      PublicationTime: Timestamp }

/// EV-S-003 / EV-S-004 — Supply Understanding events
type SupplyUnderstandingEvent =
    | SupplyUnderstandingRevised of SupplyUnderstanding * previousPublished: int option
    | SupplyUnderstandingPublished of SupplyUnderstanding * previousPublished: int option * publicationTime: Timestamp

/// Pure evolution (catamorphism). No validation.
let evolve (_: SupplyUnderstanding option) (event: SupplyUnderstandingEvent) : SupplyUnderstanding option =
    match event with
    | SupplyUnderstandingRevised(su, _) -> Some su
    | SupplyUnderstandingPublished(su, _, _) -> Some su

let replay (events: SupplyUnderstandingEvent seq) : SupplyUnderstanding option = Seq.fold evolve None events
