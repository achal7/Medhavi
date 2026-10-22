module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let private mapContinuity =
    function
    | Model.Known Model.ContinuityStatus.Stable -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyContinuityStatus.Stable
    | Model.Known Model.ContinuityStatus.AtRisk -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyContinuityStatus.AtRisk
    | Model.Known Model.ContinuityStatus.Disrupted -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyContinuityStatus.Disrupted
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private mapCapacity =
    function
    | Model.Known Model.CapacityAdequacyStatus.Sufficient -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.CapacityAdequacyStatus.Sufficient
    | Model.Known Model.CapacityAdequacyStatus.Constrained -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.CapacityAdequacyStatus.Constrained
    | Model.Known Model.CapacityAdequacyStatus.Critical -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.CapacityAdequacyStatus.Critical
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private mapInventory =
    function
    | Model.Known Model.InventoryPositionStatus.Healthy -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.InventoryPositionStatus.Healthy
    | Model.Known Model.InventoryPositionStatus.AtRisk -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.InventoryPositionStatus.AtRisk
    | Model.Known Model.InventoryPositionStatus.Critical -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.InventoryPositionStatus.Critical
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private mapSupplier =
    function
    | Model.Known Model.SupplierCommitmentPostureStatus.HighConfidence -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplierCommitmentPostureStatus.HighConfidence
    | Model.Known Model.SupplierCommitmentPostureStatus.MediumConfidence -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplierCommitmentPostureStatus.MediumConfidence
    | Model.Known Model.SupplierCommitmentPostureStatus.LowConfidence -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplierCommitmentPostureStatus.LowConfidence
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private mapRisk =
    function
    | Model.Known Model.OverallSupplyRiskLevel.Low -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.OverallSupplyRiskLevel.Low
    | Model.Known Model.OverallSupplyRiskLevel.Medium -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.OverallSupplyRiskLevel.Medium
    | Model.Known Model.OverallSupplyRiskLevel.High -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.OverallSupplyRiskLevel.High
    | Model.Known Model.OverallSupplyRiskLevel.Critical -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.OverallSupplyRiskLevel.Critical
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private mapConfidence =
    function
    | Model.Known Model.ConfidenceLevel.High -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyUnderstandingConfidenceLevel.High
    | Model.Known Model.ConfidenceLevel.Medium -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyUnderstandingConfidenceLevel.Medium
    | Model.Known Model.ConfidenceLevel.Low -> Medhavi.Contracts.Supply.InterpretationStatus.Known Medhavi.Contracts.Supply.SupplyUnderstandingConfidenceLevel.Low
    | Model.Incomplete reason -> Medhavi.Contracts.Supply.InterpretationStatus.Incomplete reason

let private emptyInterpretation: Model.Interpretation =
    { Continuity = Model.InterpretationStatus.Incomplete "Not initialized"
      ContinuityDrivers = []
      CapacityAdequacy = Model.InterpretationStatus.Incomplete "Not initialized"
      Bottlenecks = []
      InventoryPosition = Model.InterpretationStatus.Incomplete "Not initialized"
      StockoutRiskItems = []
      ExcessInventoryItems = []
      SupplierPosture = Model.InterpretationStatus.Incomplete "Not initialized"
      CriticalDependencies = []
      OverallRisk = Model.InterpretationStatus.Incomplete "Not initialized"
      RiskDrivers = []
      Confidence = Model.InterpretationStatus.Incomplete "Not initialized"
      ConfidenceFactors = []
      ReasonCodes = [] }

let private mapInterpretation (domainInter: Model.Interpretation) : SupplyInterpretation =
    { Continuity = mapContinuity domainInter.Continuity
      ContinuityDrivers = domainInter.ContinuityDrivers
      CapacityAdequacy = mapCapacity domainInter.CapacityAdequacy
      Bottlenecks = domainInter.Bottlenecks
      InventoryPosition = mapInventory domainInter.InventoryPosition
      StockoutRiskItems = domainInter.StockoutRiskItems
      ExcessInventoryItems = domainInter.ExcessInventoryItems
      SupplierPosture = mapSupplier domainInter.SupplierPosture
      CriticalDependencies = domainInter.CriticalDependencies
      OverallRisk = mapRisk domainInter.OverallRisk
      RiskDrivers = domainInter.RiskDrivers
      Confidence = mapConfidence domainInter.Confidence
      ConfidenceFactors = domainInter.ConfidenceFactors
      ReasonCodes = domainInter.ReasonCodes }

/// Map domain aggregate state to DTO.
let mapToDto (su: Model.SupplyUnderstanding) : SupplyUnderstandingDto =
    let currentVersion = su.Versions |> List.sortByDescending(fun v -> v.VersionNumber) |> List.tryHead

    let interp = currentVersion |> Option.map(fun v -> v.Interpretation) |> Option.defaultValue emptyInterpretation

    { PlanningScopeId = PlanningScopeId.value su.PlanningScopeId
      VersionNumber = currentVersion |> Option.map(fun v -> v.VersionNumber) |> Option.defaultValue 0
      Interpretation = mapInterpretation interp
      EvidencePictureVersion = currentVersion |> Option.map(fun v -> v.EvidencePictureVersion) |> Option.defaultValue 0L
      TransactionTime = currentVersion |> Option.map(fun v -> Timestamp.value v.TransactionTime) |> Option.defaultValue System.DateTimeOffset.UtcNow
      LastPublishedTime =
        su.Versions
        |> List.tryFind(fun v -> v.State = Model.VersionState.Published)
        |> Option.bind(fun v -> v.PublicationTime)
        |> Option.map Timestamp.value
      State = currentVersion |> Option.map(fun v -> v.State.ToString()) |> Option.defaultValue "Draft" }

/// Projection state: map of Planning Scope to DTO.
type State = Map<PlanningScopeId, SupplyUnderstandingDto>

let initial: State = Map.empty

/// Pure projection fold (catamorphism).
let apply (state: State) (event: Model.SupplyUnderstandingEvent) : State =
    match event with
    | Model.SupplyUnderstandingRevised(su, _)
    | Model.SupplyUnderstandingPublished(su, _, _) -> state |> Map.add su.PlanningScopeId (mapToDto su)

/// Seed projection from existing aggregates.
let seedFromAggregates (aggregates: Model.SupplyUnderstanding list) : State =
    aggregates |> List.fold(fun state agg -> Map.add agg.PlanningScopeId (mapToDto agg) state) initial
