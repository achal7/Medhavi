/// Forecast Demand Anti-Corruption Layer (ACL)
module Medhavi.Demand.ForecastDemand.ForecastPublication.ACL

open System
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.Contracts.Demand
open Medhavi.SemanticModel
open Medhavi.Demand
open Model

let private defaultUom = UnitOfMeasureId.create "EA" |> Result.defaultWith(fun _ -> failwith "Invalid default UOM")

/// Helper to generate daily buckets across horizon
let private generateBuckets (startTs: Timestamp) (endTs: Timestamp) : ForecastBucket list =
    let rec generate (curr: Timestamp) (acc: ForecastBucket list) =
        if curr >= endTs then
            List.rev acc
        else
            let next = Timestamp.addDays 1.0 curr
            let bEnd = if next > endTs then endTs else next
            let bucket = { Start = curr; End = bEnd }
            generate next (bucket :: acc)

    generate startTs []

/// Translates InitiateForecastCycleReq into InitiateForecastCycleCmd using Applicative validation.
let toInitiateCycleCmd (req: InitiateForecastCycleReq) : Validation<InitiateForecastCycleCmd, DomainError> =

    let validateHorizon =
        if req.HorizonEnd > req.HorizonStart then
            Timestamp.create req.HorizonStart
            |> Result.bind(fun hstart -> Timestamp.create req.HorizonEnd |> Result.map(fun hend -> hstart, hend))
            |> Result.mapError(fun e -> DomainError.validation $"{e}")
            |> fromResult
        else
            Invalid [ DomainError.validation "HorizonEnd must be strictly after HorizonStart" ]

    let create scope (hStart, hEnd) reason =
        let pubId =
            ForecastPublicationId.create(sprintf "PUB-%s-%A" (PlanningScopeId.value scope) hStart)
            |> Result.defaultWith(fun e -> failwith $"Failed to create ForecastPublicationId: {e}")

        { PublicationId = pubId
          PlanningScope = scope
          HorizonStart = hStart
          HorizonEnd = hEnd
          InitiationReason = reason
          InitiationTime = Timestamp.now() }

    create <!> validatePlanningScopeId req.PlanningScopeId
    <*> validateHorizon
    <*> required "InitiationReason" req.InitiationReason

/// Translates SelectChampionModelReq into SelectChampionModelCmd.
let toSelectChampionCmd (req: SelectChampionModelReq) : Validation<SelectChampionModelCmd, DomainError> =
    let create pubId modelId =
        { PublicationId = pubId
          ChampionModelId = modelId }

    create <!> validateForecastPublicationId req.PublicationId <*> required "ChampionModelId" req.ChampionModelId

/// Translates ProduceForecastProjectionReq into ProduceForecastProjectionCmd with historical data fetching.
let toProduceProjectionCmd
    (ports: DemandPorts)
    (pub: ForecastPublication)
    (req: ProduceForecastProjectionReq)
    : Task<Result<ProduceForecastProjectionCmd, ApplicationError>> =
    task {
        let! rawData = ports.GetHistoricalDemandData pub.PlanningScope 24

        // Group historical points by SKU-Location
        let groupedData =
            rawData
            |> List.groupBy(fun dp -> sprintf "%s-%s" dp.ItemId dp.LocationId)
            |> List.choose(fun (key, dps) ->
                let parsedPoints =
                    dps
                    |> List.choose(fun dp ->
                        match
                            ItemId.create dp.ItemId,
                            LocationId.create dp.LocationId,
                            Quantity.create dp.Quantity,
                            Timestamp.create dp.BusinessTime
                        with
                        | Ok item, Ok loc, Ok qty, Ok bt ->
                            Some
                                { Item = item
                                  Location = loc
                                  Quantity = qty
                                  BusinessTime = bt }
                        | _ -> None)

                if parsedPoints.IsEmpty then None else Some(key, parsedPoints))
            |> Map.ofList

        let buckets = generateBuckets pub.HorizonStart pub.HorizonEnd
        let activeVersion = pub.Versions |> List.head

        return
            Ok
                { PublicationId = pub.PublicationId
                  HistoricalData = groupedData
                  Buckets = buckets
                  ChampionModelId = activeVersion.ChampionModelId }
    }

/// Translates ApplyPlannerOverrideReq into ApplyPlannerOverrideCmd.
let toApplyOverrideCmd (req: ApplyPlannerOverrideReq) : Validation<ApplyPlannerOverrideCmd, DomainError> =
    let create pubId item loc qty just planner bstart =
        { PublicationId = pubId
          Item = item
          Location = loc
          BucketStart = bstart
          NewValue = qty
          Justification = just
          PlannerId = planner
          OverrideTime = Timestamp.now() }

    create <!> validateForecastPublicationId req.PublicationId
    <*> validateItemId req.ItemId
    <*> validateLocationId req.LocationId
    <*> validateQty req.NewValue
    <*> required "Justification" req.Justification
    <*> required "PlannerId" req.PlannerId
    <*> validateTimestamp req.BucketStart

/// Translates PublishForecastPublicationReq into PublishForecastPublicationCmd.
let toPublishCmd (req: PublishForecastPublicationReq) : Validation<PublishForecastPublicationCmd, DomainError> =
    let create pubId =
        { PublicationId = pubId
          PublicationTime = Timestamp.now() }

    create <!> validateForecastPublicationId req.PublicationId
