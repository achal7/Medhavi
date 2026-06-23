namespace Medhavi.Scheduler.Replenishment

open Medhavi.Contracts.Supply
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.SharedKernel

type ReplenishmentTarget =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      SafetyStock: Quantity
      MinStock: Quantity
      MaxStock: Quantity option
      LotSize: Quantity option }

type ShortfallAlert =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      NetStock: Quantity
      SafetyStock: Quantity
      MinStock: Quantity
      ShortfallQuantity: Quantity
      Timestamp: Timestamp }

[<RequireQualifiedAccess>]
type ReplenishmentTrigger =
    | StockLevel
    | ForecastBased of lookAheadDays: int

module ReplenishmentDomain =
    /// Get seasonal adjustment factor for a given date
    let getSeasonalAdjustmentFactor (target: InventoryTarget) (currentDate: Timestamp) : decimal =
        let currentDateDto = Timestamp.value currentDate

        target.SeasonalAdjustments
        |> List.tryFind (fun adj ->
            currentDateDto >= adj.PeriodStart
            && currentDateDto <= adj.PeriodEnd)
        |> Option.map (fun adj -> adj.AdjustmentFactor)
        |> Option.defaultValue 1.0m

    /// Calculate average daily demand from forecasts
    let calculateAverageDailyDemandFromForecast
        (forecasts: MrpDemand list)
        (lookbackDays: int)
        (asOf: Timestamp)
        : decimal =
        let cutoffDate =
            (Timestamp.value asOf)
                .AddDays(-float lookbackDays)

        let recentForecasts =
            forecasts
            |> List.filter (fun f -> Timestamp.value f.RequiredDate >= cutoffDate)

        if List.isEmpty recentForecasts then
            0m
        else
            let totalDemand =
                recentForecasts
                |> List.sumBy (fun f -> Quantity.value f.Quantity)

            let days =
                let minDate =
                    recentForecasts
                    |> List.map (fun f -> Timestamp.value f.RequiredDate)
                    |> List.min

                let maxDate =
                    recentForecasts
                    |> List.map (fun f -> Timestamp.value f.RequiredDate)
                    |> List.max

                max 1.0 ((maxDate - minDate).TotalDays + 1.0)

            totalDemand / decimal days

    /// Pure target calculation incorporating seasonal adjustments, dynamic safety stock, and cover days
    let calculateTargets
        (skuId: SkuId)
        (spId: StockingPointId)
        (target: InventoryTarget)
        (forecasts: MrpDemand list)
        (dailyDemandRateOverride: decimal option)
        (targetAsOf: Timestamp)
        : ReplenishmentTarget =

        // 1. Resolve seasonal adjustment factor
        let seasonalFactor = getSeasonalAdjustmentFactor target targetAsOf

        // 2. Resolve safety stock (dynamic from service level or static override)
        let baseSafety =
            match target.SafetyStockQty with
            | Some qty -> Quantity.clampToZero qty
            | None ->
                match target.TargetServiceLevel with
                | Some serviceLevel ->
                    // Auto-calculate dynamic safety stock using the MRP algorithms
                    let params' =
                        { SafetyStock.SafetyStockParameters.empty with
                            ServiceLevel = Some(float serviceLevel)
                            StaticOverride =
                                target.ReplenishmentPolicy
                                |> Option.map (fun p -> Quantity.clampToZero p.Safety) }

                    SafetyStock.calculateFromParameters params'
                | None ->
                    target.ReplenishmentPolicy
                    |> Option.map (fun p -> Quantity.clampToZero p.Safety)
                    |> Option.defaultValue Quantity.Zero

        // Apply seasonal factor to safety stock
        let computedSafety = baseSafety * seasonalFactor

        // 3. Resolve cover days daily rate
        let coverDaysVal =
            target.CoverDays
            |> Option.orElse (
                target.ReplenishmentPolicy
                |> Option.bind (fun p -> p.CoverDays)
            )

        let dailyRate =
            match dailyDemandRateOverride with
            | Some rate -> Some rate
            | None ->
                if not (List.isEmpty forecasts) then
                    Some(calculateAverageDailyDemandFromForecast forecasts 90 targetAsOf)
                else
                    None

        // Incorporate cover days if configured
        let computedSafetyWithCover =
            match coverDaysVal, dailyRate with
            | Some days, Some rate when rate > 0m ->
                let dynamicSafety = Quantity.clampToZero (days * rate)
                Quantity.maxOf computedSafety dynamicSafety
            | _ -> computedSafety

        // 4. Resolve min & max quantities
        let baseMin =
            target.MinQty
            |> Option.map Quantity.clampToZero
            |> Option.orElse (
                target.ReplenishmentPolicy
                |> Option.bind (fun p -> p.MinQty |> Option.map Quantity.clampToZero)
            )

        // Scale min quantity with seasonal adjustment
        let computedMin =
            match baseMin with
            | Some m -> Quantity.maxOf (m * seasonalFactor) computedSafetyWithCover
            | None -> computedSafetyWithCover

        let baseMax =
            target.MaxQty
            |> Option.map Quantity.clampToZero
            |> Option.orElse (
                target.ReplenishmentPolicy
                |> Option.bind (fun p -> p.MaxQty |> Option.map Quantity.clampToZero)
            )

        let computedMax =
            baseMax
            |> Option.map (fun m -> m * seasonalFactor)

        let lotSize =
            target.ReplenishmentPolicy
            |> Option.bind (fun p -> p.LotSize |> Option.map Quantity.clampToZero)

        { SkuId = skuId
          StockingPointId = spId
          SafetyStock = computedSafetyWithCover
          MinStock = computedMin
          MaxStock = computedMax
          LotSize = lotSize }

    /// Calculate projected stockout date from forecast
    let calculateProjectedStockoutDate
        (currentStock: decimal)
        (forecasts: MrpDemand list)
        (lookAheadDays: int)
        (asOf: Timestamp)
        : Timestamp option =
        let cutoffDate = Timestamp.value asOf
        let lookAheadDate = cutoffDate.AddDays(float lookAheadDays)

        let relevantForecasts =
            forecasts
            |> List.filter (fun f ->
                Timestamp.value f.RequiredDate >= cutoffDate
                && Timestamp.value f.RequiredDate <= lookAheadDate)
            |> List.sortBy (fun f -> f.RequiredDate)

        // Calculate cumulative demand and find stockout point
        let stockoutDate, _ =
            relevantForecasts
            |> List.fold
                (fun (stockoutOpt, remainingStock) forecast ->
                    match stockoutOpt with
                    | Some _ -> (stockoutOpt, remainingStock)
                    | None ->
                        let demand = Quantity.value forecast.Quantity
                        let newRemaining = remainingStock - demand

                        if newRemaining <= 0m then
                            (Some forecast.RequiredDate, newRemaining)
                        else
                            (None, newRemaining))
                (None, currentStock)

        stockoutDate

    /// Pure shortfall detection logic (reactive)
    let detectShortfall
        (snapshot: MaterialSnapshot)
        (target: ReplenishmentTarget)
        (timestamp: Timestamp)
        : ShortfallAlert option =

        // Net stock calculation before subtracting safety: OnHand + Inbound - Reservations
        let totalInbound = snapshot.Inbound |> List.sumBy snd
        let totalReservations = snapshot.Reservations |> List.sumBy snd
        let onHand = snapshot.OnHand

        let netStock = onHand + totalInbound - totalReservations

        // Shortfall occurs if net stock < MinStock OR net stock < SafetyStock
        let triggerLevel =
            max (Quantity.value target.MinStock) (Quantity.value target.SafetyStock)

        if netStock < triggerLevel then
            let shortfallVal = triggerLevel - netStock
            let shortfallQty = Quantity.clampToZero shortfallVal

            if Quantity.isPositive shortfallQty then
                Some
                    { SkuId = target.SkuId
                      StockingPointId = target.StockingPointId
                      NetStock = Quantity.clampToZero netStock
                      SafetyStock = target.SafetyStock
                      MinStock = target.MinStock
                      ShortfallQuantity = shortfallQty
                      Timestamp = timestamp }
            else
                None
        else
            None

    /// Enhanced shortfall detection logic with forecast-based proactive triggers
    let detectShortfallWithForecast
        (snapshot: MaterialSnapshot)
        (target: ReplenishmentTarget)
        (forecasts: MrpDemand list)
        (trigger: ReplenishmentTrigger)
        (timestamp: Timestamp)
        : ShortfallAlert option =

        match trigger with
        | ReplenishmentTrigger.StockLevel -> detectShortfall snapshot target timestamp
        | ReplenishmentTrigger.ForecastBased lookAheadDays ->
            // Net stock = OnHand + Inbound - Reservations
            let totalInbound = snapshot.Inbound |> List.sumBy snd
            let totalReservations = snapshot.Reservations |> List.sumBy snd
            let netStock = snapshot.OnHand + totalInbound - totalReservations

            let stockoutDateOpt =
                calculateProjectedStockoutDate netStock forecasts lookAheadDays timestamp

            match stockoutDateOpt with
            | Some stockoutDate ->
                // Calculate forecast demand up to stockout date
                let totalForecastDemand =
                    forecasts
                    |> List.filter (fun f ->
                        f.RequiredDate >= timestamp
                        && f.RequiredDate <= stockoutDate)
                    |> List.sumBy (fun f -> Quantity.value f.Quantity)

                // Needed quantity to prevent stockout and maintain safety stock
                let triggerLevel =
                    totalForecastDemand
                    + Quantity.value target.SafetyStock

                let shortfallVal = triggerLevel - netStock
                let shortfallQty = Quantity.clampToZero shortfallVal

                if Quantity.isPositive shortfallQty then
                    Some
                        { SkuId = target.SkuId
                          StockingPointId = target.StockingPointId
                          NetStock = Quantity.clampToZero netStock
                          SafetyStock = target.SafetyStock
                          MinStock = target.MinStock
                          ShortfallQuantity = shortfallQty
                          Timestamp = stockoutDate }
                else
                    None
            | None ->
                // No stockout projected, fallback to stock level check
                detectShortfall snapshot target timestamp
