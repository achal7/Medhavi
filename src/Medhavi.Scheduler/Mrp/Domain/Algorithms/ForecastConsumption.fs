/// Forecast Consumption Algorithms — Prevents double-counting of demand by consuming forecasts with customer orders
module Medhavi.Scheduler.Mrp.Domain.Algorithms.ForecastConsumption

open System
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Scheduler.Mrp.Domain.Policies

/// Check if a forecast falls within the consumption window of a customer order
let isWithinWindow
    (strategy: ForecastConsumptionStrategy)
    (windowSize: TimeSpan)
    (orderDate: DateTimeOffset)
    (forecastStart: DateTimeOffset)
    (forecastEnd: DateTimeOffset)
    : bool =
    let (wStart, wEnd) =
        match strategy with
        | BackwardConsumption -> (orderDate - windowSize, orderDate)
        | ForwardConsumption -> (orderDate, orderDate + windowSize)
        | BidirectionalConsumption -> (orderDate - windowSize, orderDate + windowSize)

    forecastStart <= wEnd && forecastEnd >= wStart

/// Consume forecasts with customer orders based on policy
let consumeForecasts
    (policy: ForecastConsumptionPolicy)
    (forecasts: Forecast list)
    (orders: CustomerOrder list)
    : Forecast list =

    if not policy.Enabled then
        forecasts
    else
        let forecastsGrouped =
            forecasts
            |> List.groupBy (fun f -> f.SkuId, f.NodeId)
            |> Map.ofList

        let ordersGrouped =
            orders
            |> List.groupBy (fun o -> o.SkuId, o.NodeId)
            |> Map.ofList

        let allKeys =
            Set.union (forecastsGrouped |> Map.keys |> Set.ofSeq) (ordersGrouped |> Map.keys |> Set.ofSeq)

        allKeys
        |> Seq.toList
        |> List.collect (fun (skuId, nodeId) ->
            let groupForecasts =
                forecastsGrouped
                |> Map.tryFind (skuId, nodeId)
                |> Option.defaultValue []

            let groupOrders =
                ordersGrouped
                |> Map.tryFind (skuId, nodeId)
                |> Option.defaultValue []

            // Sort orders chronologically to consume forecast in order
            let sortedOrders = groupOrders |> List.sortBy (fun o -> o.DueDate)

            let finalForecasts =
                (groupForecasts, sortedOrders)
                ||> List.fold (fun currentForecasts order ->
                    let orderDate = order.DueDate

                    let candidates =
                        currentForecasts
                        |> List.filter (fun f ->
                            isWithinWindow
                                policy.Strategy
                                policy.ConsumptionWindow
                                orderDate
                                f.PeriodStart
                                f.PeriodEnd)
                        |> List.sortBy (fun f -> abs (f.PeriodStart - orderDate).Ticks)

                    let rec consume
                        (remainingQty: decimal)
                        (accForecasts: Forecast list)
                        (candidatesLeft: Forecast list)
                        =
                        if remainingQty <= 0m then
                            accForecasts @ candidatesLeft
                        else
                            match candidatesLeft with
                            | [] -> accForecasts
                            | f :: rest ->
                                let fQty = Quantity.value f.Quantity

                                if remainingQty >= fQty then
                                    // Fully consume this forecast period
                                    consume (remainingQty - fQty) accForecasts rest
                                else
                                    // Partially consume this forecast period
                                    let newQty = Quantity.clampToZero (fQty - remainingQty)
                                    let updatedForecast = { f with Quantity = newQty }
                                    consume 0m (updatedForecast :: accForecasts) rest

                    let nonCandidates =
                        currentForecasts
                        |> List.filter (fun f ->
                            not (
                                isWithinWindow
                                    policy.Strategy
                                    policy.ConsumptionWindow
                                    orderDate
                                    f.PeriodStart
                                    f.PeriodEnd
                            ))

                    let consumedCandidates = consume (Quantity.value order.Quantity) [] candidates
                    nonCandidates @ consumedCandidates)

            finalForecasts)
        |> List.filter (fun f -> Quantity.isPositive f.Quantity)
