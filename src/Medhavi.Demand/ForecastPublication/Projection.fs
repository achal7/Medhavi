module Medhavi.Demand.ForecastPublication.Projection

open Medhavi.Infrastructure.Projections
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.ForecastPublication.Model

type ForecastPublicationProjectionState = Map<string, ForecastPublication.ForecastPublication>

let mapToContract (pub: ForecastPublication) : ForecastPublication.ForecastPublication =
    let forecastList =
        pub.Forecasts
        |> Map.toList
        |> List.map(fun (_, f) ->
            { ForecastId = ForecastId.value f.ForecastId
              SkuId = SkuId.value f.SkuId
              StockingPointId = StockingPointId.value f.StockingPointId
              PlanningPeriod = f.PlanningPeriod //.ToDateTimeOffset()
              Mean = f.Mean
              LowerBound = PositiveDecimal.value f.PredictionInterval.LowerBound
              UpperBound = PositiveDecimal.value f.PredictionInterval.UpperBound
              Confidence = PositiveDecimal.value f.Confidence
              ModelId = f.ModelId
              OverrideReason = f.OverrideReason }
            : ForecastPublication.Forecast)

    let assumptionList =
        pub.Assumptions
        |> Map.toList
        |> List.map(fun (_, a) ->
            { AssumptionId = a.AssumptionId
              Statement = a.Statement
              DeclaredBy = a.DeclaredBy
              LifecycleState = a.LifecycleState.ToString()
              LinkedDriverRef = a.LinkedDriverRef }
            : ForecastPublication.Assumption)

    let overrideList =
        pub.Overrides
        |> Map.toList
        |> List.map(fun (_, o) ->
            { ForecastId = ForecastId.value o.ForecastId
              OriginalValue = o.OriginalValue
              OverrideValue = o.OverrideValue
              Justification = o.Justification
              PlannerIdentity = o.PlannerIdentity
              OverrideTimestamp = Timestamp.value o.OverrideTimestamp }
            : ForecastPublication.Override)

    let coverageList =
        pub.Coverage
        |> List.map(fun (sku, sp) ->
            { SkuId = SkuId.value sku
              StockingPointId = StockingPointId.value sp }
            : ForecastPublication.CoverageItem)

    { PublicationId = ForecastPublicationId.value pub.Id
      Version = pub.Version
      Status = pub.Status.ToString()
      PlanningScopeIds = pub.PlanningScopeIds |> List.map PlanningScopeId.value
      ForecastHorizon = pub.ForecastHorizon.ToString()
      ChampionModelId = pub.ChampionModelId
      OverallConfidenceIndex = pub.OverallConfidenceIndex
      Coverage = coverageList
      Forecasts = forecastList
      Assumptions = assumptionList
      Overrides = overrideList
      TransactionTime = Timestamp.value pub.TransactionTime
      PublicationTime = pub.PublicationTime |> Option.map Timestamp.value }

let evolveProjection (state: ForecastPublicationProjectionState) (evt: ForecastPublicationEvent) =
    match evt with
    | ForecastCycleInitiated(pub, _, _) -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | ForecastContextPrepared pub -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | ChampionModelSelected(pub, _, _, _) -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | BaselineForecastsGenerated(pub, _) -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | ForecastOverrideRecorded(pub, _) -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | ForecastHierarchyReconciled pub -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state
    | ForecastPublicationPublished(pub, _) -> Map.add (ForecastPublicationId.value pub.Id) (mapToContract pub) state

type ForecastPublicationAgent = ProjectionAgent<ForecastPublicationProjectionState, ForecastPublicationEvent>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "ForecastPublicationReadModel")

let createQueryService (agent: ForecastPublicationAgent) = QueryServiceBase.getQueryService agent id

let seedProjections (agent: ForecastPublicationAgent) (list: ForecastPublication list) =
    let m =
        list
        |> List.map(fun pub ->
            let key = ForecastPublicationId.value pub.Id
            key, mapToContract pub)
        |> Map.ofList

    agent.SetState m
