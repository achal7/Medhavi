/// Forecast Publication Read Model Projections
/// Catamorphic fold over ForecastPublicationEvent
module Medhavi.Demand.ForecastDemand.ForecastPublication.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

type State = Map<ForecastPublicationId, ForecastPublicationDto>

let initial: State = Map.empty

let mapToDto (pub: ForecastPublication) : ForecastPublicationDto =
    let activeVersion = pub.Versions |> List.head

    let linesDto =
        activeVersion.Lines
        |> List.map(fun l ->
            let intervalDto =
                l.Interval
                |> Option.map(fun pi ->
                    { Lower = Quantity.value pi.Lower
                      Upper = Quantity.value pi.Upper
                      ConfidenceLevel = pi.ConfidenceLevel }
                    : PredictionIntervalDto)

            { ItemId = ItemId.value l.Item
              LocationId = LocationId.value l.Location
              BucketStart = Timestamp.value l.Bucket.Start
              BucketEnd = Timestamp.value l.Bucket.End
              Mean = Quantity.value l.Mean
              Interval = intervalDto
              ConfidenceScore = l.ConfidenceScore
              ModelProvenance = l.ModelProvenance
              IsOverride = l.IsOverride
              OriginalMean = l.OriginalMean |> Option.map Quantity.value
              UnforecastableFlag = l.UnforecastableFlag
              UnforecastableReason = l.UnforecastableReason
              FallbackMethod = l.FallbackMethod })

    let assumptionsDto =
        activeVersion.Assumptions
        |> List.map(fun a ->
            { AssumptionId = a.AssumptionId
              Category = a.Category
              Statement = a.Statement
              SignoffStatus = sprintf "%A" a.SignoffStatus
              SignoffAuthority = a.SignoffAuthority
              SignoffTimestamp = a.SignoffTimestamp |> Option.map Timestamp.value }
            : ForecastAssumptionDto)

    let overridesDto =
        activeVersion.Overrides
        |> List.map(fun o ->
            { OverrideId = o.OverrideId
              ItemId = ItemId.value o.Item
              LocationId = LocationId.value o.Location
              BucketStart = Timestamp.value o.BucketStart
              OriginalValue = Quantity.value o.OriginalValue
              OverrideValue = Quantity.value o.OverrideValue
              PlannerId = o.PlannerId
              Justification = o.Justification
              Timestamp = Timestamp.value o.Timestamp })

    { PublicationId = ForecastPublicationId.value pub.PublicationId
      PlanningScopeId = PlanningScopeId.value pub.PlanningScope
      HorizonStart = Timestamp.value pub.HorizonStart
      HorizonEnd = Timestamp.value pub.HorizonEnd
      VersionNumber = activeVersion.VersionNumber
      Lines = linesDto
      Assumptions = assumptionsDto
      Overrides = overridesDto
      ConfidenceIndex = activeVersion.ConfidenceIndex
      CompletenessScore = activeVersion.CompletenessScore
      ChampionModelId = activeVersion.ChampionModelId
      GenerationStatus = sprintf "%A" activeVersion.GenerationStatus
      PublicationTime = activeVersion.PublicationTime |> Option.map Timestamp.value
      LifecycleState = sprintf "%A" pub.CurrentState }

let apply (state: State) (event: ForecastPublicationEvent) : State =
    let pub =
        match event with
        | ForecastCycleEstablished p -> p
        | ChampionModelSelected(p, _) -> p
        | ForecastProjectionProduced p -> p
        | ForecastOverrideRecorded(p, _) -> p
        | ForecastPublicationPublished(p, _, _) -> p

    let dto = mapToDto pub
    state |> Map.add pub.PublicationId dto

let seedFromAggregates (aggregates: ForecastPublication list) : State =
    aggregates
    |> List.map(fun a ->
        let dto = mapToDto a
        a.PublicationId, dto)
    |> Map.ofList
