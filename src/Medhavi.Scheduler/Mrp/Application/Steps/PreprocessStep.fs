/// Preprocess Step — Input validation, forecast consumption, and demand grouping
module Medhavi.Scheduler.Mrp.Steps.PreprocessStep

open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Domain.Policies
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Domain.Algorithms

/// Validate a single demand signal
let validateDemand (demand: MrpDemand) : Result<MrpDemand, PreprocessError> =
    if Quantity.value demand.Quantity <= 0m then
        Error(InvalidDemand(SkuId.value demand.SkuId, "Quantity must be positive"))
    else
        Ok demand

/// Validate list of demands
let validateDemands (demands: MrpDemand list) : Result<MrpDemand list, PreprocessError list> =
    if List.isEmpty demands then
        Error [ EmptyDemandList ]
    else
        let results = demands |> List.map validateDemand

        let errors =
            results
            |> List.choose (function
                | Error e -> Some e
                | _ -> None)

        let valid =
            results
            |> List.choose (function
                | Ok d -> Some d
                | _ -> None)

        if List.isEmpty errors then Ok valid
        else if List.isEmpty valid then Error errors
        else Ok valid // Continue with valid, errors can be warnings

/// Group demands by SKU, Node, Stocking Point, and Required Date
let groupDemands (demands: MrpDemand list) : MrpDemand list =
    demands
    |> List.groupBy (fun d -> (d.SkuId, d.NodeId, d.StockingPointId, d.RequiredDate))
    |> List.map (fun ((skuId, nodeId, spId, reqDate), grouped) ->
        let totalQty =
            grouped
            |> List.map (fun d -> d.Quantity)
            |> Quantity.sum

        let highestPriority =
            grouped
            |> List.choose (fun d -> d.Priority)
            |> function
                | [] -> None
                | list -> Some(List.min list) // lower number = higher priority

        let source =
            match grouped with
            | [ single ] -> single.Source
            | multiple ->
                let first = List.head multiple
                first.Source

        { DemandId = (List.head grouped).DemandId
          SkuId = skuId
          NodeId = nodeId
          StockingPointId = spId
          Quantity = totalQty
          RequiredDate = reqDate
          Source = source
          Priority = highestPriority })

/// Preprocess step execution
let execute: MrpStepAsync<MrpDemand list, MrpDemand list> =
    fun demands ctx ->
        task {
            let startTime = Timestamp.now

            match validateDemands demands with
            | Error errs -> return Error(Preprocess errs)
            | Ok validDemands ->
                // Apply forecast consumption if enabled in policy
                let consumedDemands =
                    match ctx.Policy.ForecastConsumption with
                    | Some policy when policy.Enabled ->
                        // Partition demands
                        let (coDemands, nonCoDemands) =
                            validDemands
                            |> List.partition (fun d ->
                                match d.Source with
                                | CustomerOrder _ -> true
                                | _ -> false)

                        let (fcDemands, otherDemands) =
                            nonCoDemands
                            |> List.partition (fun d ->
                                match d.Source with
                                | Forecast _ -> true
                                | _ -> false)

                        let demandOrders =
                            coDemands
                            |> List.map (fun d ->
                                match d.Source with
                                | CustomerOrder(orderId, lineId) ->
                                    { OrderId =
                                        OrderId.create orderId
                                        |> Result.defaultWith (fun _ -> failwith "Invalid")
                                      LineId = lineId
                                      SkuId = d.SkuId
                                      NodeId = d.NodeId
                                      Quantity = d.Quantity
                                      DueDate = d.RequiredDate
                                      Priority = d.Priority |> Option.defaultValue 3
                                      IsExpedited = false }
                                | _ -> failwith "Unreachable")

                        let demandForecasts =
                            fcDemands
                            |> List.map (fun d ->
                                match d.Source with
                                | Forecast forecastId ->
                                    { ForecastId = forecastId
                                      SkuId = d.SkuId
                                      NodeId = d.NodeId
                                      Quantity = d.Quantity
                                      PeriodStart = d.RequiredDate
                                      PeriodEnd = d.RequiredDate }
                                | _ -> failwith "Unreachable")

                        let consumed =
                            ForecastConsumption.consumeForecasts policy demandForecasts demandOrders

                        let remainingFcDemands =
                            consumed
                            |> List.map (fun f ->
                                let spId =
                                    fcDemands
                                    |> List.tryFind (fun d -> d.DemandId = f.ForecastId)
                                    |> Option.map (fun d -> d.StockingPointId)
                                    |> Option.defaultWith (fun () ->
                                        StockingPointId.create (NodeId.value f.NodeId)
                                        |> Result.defaultWith (fun _ -> failwith "Invalid"))

                                { DemandId = f.ForecastId
                                  SkuId = f.SkuId
                                  NodeId = f.NodeId
                                  StockingPointId = spId
                                  Quantity = f.Quantity
                                  RequiredDate = f.PeriodStart
                                  Source = Forecast f.ForecastId
                                  Priority = None })

                        coDemands @ remainingFcDemands @ otherDemands
                    | _ -> validDemands

                let grouped = groupDemands consumedDemands
                let endTime = Timestamp.now

                let duration =
                    Timestamp.value endTime
                    - Timestamp.value startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (MrpRunStarted(MrpRunId.value ctx.RunId, startTime))
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with
                            ComponentsProcessed = List.length grouped })

                return Ok(grouped, updatedCtx)
        }
