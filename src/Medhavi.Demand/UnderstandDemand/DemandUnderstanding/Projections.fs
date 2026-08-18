module Medhavi.Demand.UnderstandDemand.DemandUnderstanding.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts

let private mapContinuity =
    function
    | Model.Known Model.Stable -> Demand.InterpretationStatus.Known Demand.ContinuityStatus.Stable
    | Model.Known Model.Increasing -> Demand.InterpretationStatus.Known Demand.ContinuityStatus.Increasing
    | Model.Known Model.Declining -> Demand.InterpretationStatus.Known Demand.ContinuityStatus.Declining
    | Model.Known Model.Volatile -> Demand.InterpretationStatus.Known Demand.ContinuityStatus.Volatile
    | Model.Incomplete reason -> Demand.InterpretationStatus.Incomplete reason

let private mapPattern =
    function
    | Model.Known Model.Normal -> Demand.InterpretationStatus.Known Demand.PatternStatus.Normal
    | Model.Known Model.Seasonal -> Demand.InterpretationStatus.Known Demand.PatternStatus.Seasonal
    | Model.Known Model.Irregular -> Demand.InterpretationStatus.Known Demand.PatternStatus.Irregular
    | Model.Known Model.StepChange -> Demand.InterpretationStatus.Known Demand.PatternStatus.StepChange
    | Model.Incomplete reason -> Demand.InterpretationStatus.Incomplete reason

let private mapHealth =
    function
    | Model.Known Model.Healthy -> Demand.InterpretationStatus.Known Demand.HealthStatus.Healthy
    | Model.Known Model.AtRisk -> Demand.InterpretationStatus.Known Demand.HealthStatus.AtRisk
    | Model.Known Model.Critical -> Demand.InterpretationStatus.Known Demand.HealthStatus.Critical
    | Model.Incomplete reason -> Demand.InterpretationStatus.Incomplete reason

let private mapVolatility =
    function
    | Model.Known Model.Low -> Demand.InterpretationStatus.Known Demand.VolatilityLevel.Low
    | Model.Known Model.Medium -> Demand.InterpretationStatus.Known Demand.VolatilityLevel.Medium
    | Model.Known Model.High -> Demand.InterpretationStatus.Known Demand.VolatilityLevel.High
    | Model.Incomplete reason -> Demand.InterpretationStatus.Incomplete reason

let private mapConfidence =
    function
    | Model.Known Model.ConfidenceLevel.High -> Demand.InterpretationStatus.Known Demand.ConfidenceLevel.High
    | Model.Known Model.ConfidenceLevel.Medium -> Demand.InterpretationStatus.Known Demand.ConfidenceLevel.Medium
    | Model.Known Model.ConfidenceLevel.Low -> Demand.InterpretationStatus.Known Demand.ConfidenceLevel.Low
    | Model.Incomplete reason -> Demand.InterpretationStatus.Incomplete reason

let private emptyInterpretation: Model.Interpretation =
    { Continuity = Model.InterpretationStatus.Incomplete "Not done"
      ContinuityDrivers = []
      Pattern = Model.InterpretationStatus.Incomplete "Not done"
      PatternConfidence = Model.InterpretationStatus.Incomplete "Not done"
      Health = Model.InterpretationStatus.Incomplete "Not done"
      HealthConcerns = []
      Volatility = Model.InterpretationStatus.Incomplete "Not done"
      VolatilityDrivers = []
      ReasonCodes = [] }

let private mapInterpretation (domainInter: Model.Interpretation) : Demand.Interpretation =
    { Continuity = mapContinuity domainInter.Continuity
      ContinuityDrivers = domainInter.ContinuityDrivers
      Pattern = mapPattern domainInter.Pattern
      PatternConfidence = mapConfidence domainInter.PatternConfidence
      Health = mapHealth domainInter.Health
      HealthConcerns = domainInter.HealthConcerns
      Volatility = mapVolatility domainInter.Volatility
      VolatilityDrivers = domainInter.VolatilityDrivers
      ReasonCodes = domainInter.ReasonCodes }

/// Map domain aggregate state to DTO.
let mapToDto (du: Model.DemandUnderstanding) : Demand.DemandUnderstandingDto =
    let currentVersion = du.Versions |> List.sortByDescending(fun v -> v.VersionNumber) |> List.tryHead

    let interp = currentVersion |> Option.map(fun v -> v.Interpretation) |> Option.defaultValue emptyInterpretation

    { PlanningScopeId = PlanningScopeId.value du.PlanningScopeId
      VersionNumber = currentVersion |> Option.map(fun v -> v.VersionNumber) |> Option.defaultValue 0
      Interpretation = mapInterpretation interp
      LastPublishedTime =
        du.Versions
        |> List.tryFind(fun v -> v.State = Model.VersionState.Published)
        |> Option.bind(fun v -> v.PublicationTime)
        |> Option.map Timestamp.value
      State = currentVersion |> Option.map(fun v -> v.State.ToString()) |> Option.defaultValue "Draft" }

/// Projection state: map of Planning Scope to DTO.
type State = Map<string, Demand.DemandUnderstandingDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism).
let apply (state: State) (event: Model.DemandUnderstandingEvent) : State =
    match event with
    | Model.DemandUnderstandingRevised(du, _)
    | Model.DemandUnderstandingPublished(du, _, _) ->
        state |> Map.add (PlanningScopeId.value du.PlanningScopeId) (mapToDto du)

/// Seed projection from existing aggregates.
let seedFromAggregates (aggregates: Model.DemandUnderstanding list) : State =
    aggregates
    |> List.fold (fun state agg -> Map.add (PlanningScopeId.value agg.PlanningScopeId) (mapToDto agg) state) initial
