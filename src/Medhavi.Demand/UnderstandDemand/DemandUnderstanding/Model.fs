/// SE-D-002 — Demand Understanding Aggregate Model
/// Traces to: Demand Intelligence Specification (SE-D-002, SE-D-012, AB-D-003, AB-D-004)
module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Model

open Medhavi.SemanticModel
open Medhavi.Demand

/// SE-D-012 — Demand Continuity Interpretation status.
type ContinuityStatus =
    | Stable
    | Increasing
    | Declining
    | Volatile

/// SE-D-012 — Demand Pattern Interpretation status.
type PatternStatus =
    | Normal
    | Seasonal
    | Irregular
    | StepChange

/// SE-D-012 — Demand Health Interpretation status.
type HealthStatus =
    | Healthy
    | AtRisk
    | Critical

/// SE-D-012 — Demand Volatility Interpretation level.
type VolatilityLevel =
    | Low
    | Medium
    | High

/// SE-D-012 — Pattern confidence level.
type ConfidenceLevel =
    | High
    | Medium
    | Low

/// BR-D-205 — A dimension may carry an "Incomplete" status when evidence is unavailable.
type InterpretationStatus<'T> =
    | Known of 'T
    | Incomplete of reason: string

/// SE-D-012 — the four-dimensional demand interpretation.
type Interpretation =
    { Continuity: InterpretationStatus<ContinuityStatus>
      ContinuityDrivers: string list
      Pattern: InterpretationStatus<PatternStatus>
      PatternConfidence: InterpretationStatus<ConfidenceLevel>
      Health: InterpretationStatus<HealthStatus>
      HealthConcerns: string list
      Volatility: InterpretationStatus<VolatilityLevel>
      VolatilityDrivers: string list
      ReasonCodes: string list }

module Interpretation =
    /// BR-D-205 — completeness fraction over the four mandatory interpretation dimensions.
    let completenessRatio (i: Interpretation) : decimal =
        let complete (s: InterpretationStatus<'T>) =
            match s with
            | Known _ -> true
            | Incomplete _ -> false

        let patternComplete = complete i.Pattern && complete i.PatternConfidence

        let completeCount =
            (if complete i.Continuity then 1 else 0)
            + (if complete i.Health then 1 else 0)
            + (if complete i.Volatility then 1 else 0)
            + (if patternComplete then 1 else 0)

        decimal completeCount / 4m

/// SE-D-012 — version lifecycle state.
type VersionState =
    | Draft
    | Published
    | Superseded

/// SE-D-012 — Demand Understanding Version.
type DemandUnderstandingVersion =
    { VersionNumber: int
      Interpretation: Interpretation
      EvidencePictureVersion: int
      TransactionTime: Timestamp
      PublicationTime: Timestamp option
      State: VersionState }

/// SE-D-002 — Demand Understanding aggregate root.
type DemandUnderstanding =
    { PlanningScopeId: PlanningScopeId
      Versions: DemandUnderstandingVersion list
      CurrentPublishedVersion: int option }

/// AB-D-003 — Revise command carrying picture-derived demand facts (BR-D-400).
type ReviseCmd =
    { PlanningScopeId: PlanningScopeId
      PictureFacts: PictureFacts
      TransactionTime: Timestamp }

/// AB-D-004 — Publish command.
type PublishCmd =
    { PlanningScopeId: PlanningScopeId
      IsPeriodicRefresh: bool
      PublicationTime: Timestamp }

/// EV-D-003 / EV-D-004 — Demand Understanding events.
type DemandUnderstandingEvent =
    | DemandUnderstandingRevised of DemandUnderstanding * previousPublished: int option
    | DemandUnderstandingPublished of DemandUnderstanding * previousPublished: int option * publicationTime: Timestamp

/// Pure evolution (catamorphism). No validation.
let evolve (state: DemandUnderstanding option) (event: DemandUnderstandingEvent) : DemandUnderstanding option =
    match event with
    | DemandUnderstandingRevised(du, _) -> Some du
    | DemandUnderstandingPublished(du, _, _) -> Some du

let replay (events: DemandUnderstandingEvent seq) : DemandUnderstanding option = Seq.fold evolve None events
