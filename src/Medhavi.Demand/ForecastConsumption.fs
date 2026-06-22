namespace Medhavi.Demand.Domain

open System
open Medhavi.SharedKernel

type ForecastBucket =
    { ForecastId: string
      SkuId: SkuId
      StockingPointId: StockingPointId
      PeriodStart: DateTimeOffset
      PeriodEnd: DateTimeOffset
      OriginalQuantity: Quantity
      RemainingQuantity: Quantity }

type ConsumptionAllocation =
    { DemandLineId: string
      ForecastId: string
      AllocatedQuantity: Quantity }

type ConsumptionResult =
    { ResidualForecasts: ForecastBucket list
      Allocations: ConsumptionAllocation list }

type ConsumptionPolicy =
    | StrictBucket
    | BackwardOnly
    | ForwardOnly
    | BackwardThenForward

module ForecastConsumptionService =

    /// Evaluates if a demand date falls within a forecast's time period adjusted by policy and backward/forward day windows
    let isWithinWindow
        (policy: ConsumptionPolicy)
        (backwardDays: int)
        (forwardDays: int)
        (demandDate: DateTimeOffset)
        (forecast: ForecastBucket)
        =
        match policy with
        | StrictBucket -> demandDate >= forecast.PeriodStart && demandDate <= forecast.PeriodEnd
        | BackwardOnly ->
            let lowerBound = forecast.PeriodStart
            let upperBound = forecast.PeriodEnd.AddDays(float backwardDays)
            demandDate >= lowerBound && demandDate <= upperBound
        | ForwardOnly ->
            let lowerBound = forecast.PeriodStart.AddDays(float -forwardDays)
            let upperBound = forecast.PeriodEnd
            demandDate >= lowerBound && demandDate <= upperBound
        | BackwardThenForward ->
            let lowerBound = forecast.PeriodStart.AddDays(float -backwardDays)
            let upperBound = forecast.PeriodEnd.AddDays(float forwardDays)
            demandDate >= lowerBound && demandDate <= upperBound

    /// Pure forecast consumption logic using tail-recursive functional matching
    let consume
        (policy: ConsumptionPolicy)
        (backwardDays: int)
        (forwardDays: int)
        (forecasts: ForecastBucket list)
        (demands: DemandLine list)
        : ConsumptionResult =

        let initialForecasts = forecasts |> List.map(fun f -> f.ForecastId, f) |> Map.ofList

        // Sort demands by priority first (highest priority consumes forecast first), then requested date
        let sortedDemands =
            demands
            |> List.filter(fun d -> d.DemandCategory = CustomerOrderDemand)
            |> List.sortBy(fun d -> DemandPriority.weight d.Priority, d.RequestedDeliveryDate)

        // Recursive loop over demand lines
        let rec consumeDemands
            (demandsList: DemandLine list)
            (currentForecasts: Map<string, ForecastBucket>)
            (accAllocations: ConsumptionAllocation list)
            =
            match demandsList with
            | [] ->
                { ResidualForecasts = currentForecasts |> Map.toList |> List.map snd
                  Allocations = accAllocations |> List.rev }
            | demand: DemandLine :: restDemands ->
                let requestedDeliveryDate = Timestamp.value demand.RequestedDeliveryDate
                // Find matching candidate forecast buckets within time boundaries
                let candidateForecastIds =
                    currentForecasts
                    |> Map.filter(fun _ f ->
                        f.SkuId = demand.SkuId
                        && f.StockingPointId = demand.StockingPointId
                        && not f.RemainingQuantity.IsZero
                        && isWithinWindow policy backwardDays forwardDays requestedDeliveryDate f)
                    |> Map.toSeq
                    // Order candidates: prioritize exact time overlap first, then closest period start
                    |> Seq.sortBy(fun (_, f) ->
                        let isExact =
                            requestedDeliveryDate >= f.PeriodStart
                            && requestedDeliveryDate <= f.PeriodEnd

                        let exactScore = if isExact then 0 else 1

                        let policyScore =
                            match policy with
                            | BackwardThenForward ->
                                let isPast = f.PeriodEnd <= requestedDeliveryDate
                                if isPast then 0 else 1
                            | _ -> 0

                        let distanceScore = abs (requestedDeliveryDate - f.PeriodStart).Ticks
                        exactScore, policyScore, distanceScore)
                    |> Seq.map fst
                    |> Seq.toList

                // Recursive loop to allocate a single demand against candidates
                let rec allocateDemand (remainingQty: Quantity) candidates forecastsMap allocationsList =
                    match candidates with
                    | _ when remainingQty.IsZero -> (forecastsMap, allocationsList)
                    | [] -> (forecastsMap, allocationsList)
                    | fid :: restCandidates ->
                        let forecast = Map.find fid forecastsMap
                        let allocatedQty = Quantity.minOf remainingQty forecast.RemainingQuantity

                        if Quantity.isPositive allocatedQty then
                            let updatedForecast =
                                { forecast with
                                    RemainingQuantity = forecast.RemainingQuantity - allocatedQty }

                            let nextForecasts = Map.add fid updatedForecast forecastsMap

                            let newAllocation =
                                { DemandLineId = demand.DemandLineId
                                  ForecastId = fid
                                  AllocatedQuantity = allocatedQty }

                            allocateDemand
                                (remainingQty - allocatedQty)
                                restCandidates
                                nextForecasts
                                (newAllocation :: allocationsList)
                        else
                            allocateDemand remainingQty restCandidates forecastsMap allocationsList

                let nextForecasts, nextAllocations =
                    allocateDemand demand.Quantity candidateForecastIds currentForecasts accAllocations

                consumeDemands restDemands nextForecasts nextAllocations

        consumeDemands sortedDemands initialForecasts []
