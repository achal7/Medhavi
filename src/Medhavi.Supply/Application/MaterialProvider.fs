namespace Medhavi.Supply.Application

open System
open System.Threading.Tasks
open Medhavi.Contracts.Domain
open Medhavi.Supply

module MaterialProvider =

    let getSnapshot
        (caps: Supply)
        (productId: string)
        (stockingPointId: string)
        (asOf: DateTimeOffset)
        : Async<Result<MaterialSnapshot, ProviderError>> =
        task {
            try
                // 1. Get OnHand Inventory
                let invId = $"INV-{productId}-{stockingPointId}"
                let! invOpt = caps.Inventory.QueryService.GetById invId
                let onHand =
                    match invOpt with
                    | Some inv -> inv.Quantity
                    | None -> 0.0m

                // 2. Get Firm Inbound (PO, WO, TO)
                let! allOrders =
                    caps.SupplyOrder.QueryService.Filter (fun o ->
                        o.SkuId = productId &&
                        o.StockingPointId = stockingPointId &&
                        (o.IsFirm || o.State = "Confirmed" || o.State = "Released" || o.State = "InProgress")
                    )

                // Sort by expected delivery date
                let inbound =
                    allOrders
                    |> List.choose (fun o -> o.RequiredDeliveryDate |> Option.map (fun d -> d, o.Quantity))
                    |> List.sortBy fst

                // 3. Get Safety Stock
                let targetId = $"{productId}-{stockingPointId}"
                let! targetOpt = caps.InventoryTarget.QueryService.GetById targetId
                let safety =
                    match targetOpt with
                    | Some (t: InventoryTarget) when t.IsActive ->
                        t.SafetyStockQty |> Option.defaultValue 0.0m
                    | _ -> 0.0m

                // 4. Get active reservations (Tentative or Confirmed)
                let! activeReservations =
                    caps.MaterialReservation.QueryService.Filter (fun r ->
                        r.SkuId = productId &&
                        r.StockingPointId = stockingPointId &&
                        (r.State = "Tentative" || r.State = "Confirmed")
                    )

                let reservations =
                    activeReservations
                    |> List.map (fun r -> r.RequiredDate, r.Quantity)
                    |> List.sortBy fst

                let snapshot : MaterialSnapshot = {
                    OnHand = onHand
                    Inbound = inbound
                    Reservations = reservations
                    Safety = safety
                }
                return Ok snapshot
            with ex ->
                return Error (UnknownError ex.Message)
        } |> Async.AwaitTask

    let getSupplierOptions
        (caps: Supply)
        (productId: string)
        (stockingPointId: string option)
        (quantity: decimal)
        (needDate: DateTimeOffset)
        : Async<Result<SupplierOffer list, ProviderError>> =
        task {
            try
                let! offers =
                    caps.SupplierOffer.QueryService.Filter (fun o ->
                        o.SkuId = productId &&
                        (match stockingPointId, o.StockingPointId with
                         | Some sp, Some offerSp -> sp = offerSp
                         | None, _ -> true
                         | _, None -> true) &&
                        o.IsActive
                    )
                return Ok offers
            with ex ->
                return Error (UnknownError ex.Message)
        } |> Async.AwaitTask

    /// Calculates the net available quantity from a snapshot
    let calculateNetAvailable (snapshot: MaterialSnapshot) : decimal =
        let totalInbound = snapshot.Inbound |> List.sumBy snd
        let totalReservations = snapshot.Reservations |> List.sumBy snd
        snapshot.OnHand + totalInbound - totalReservations - snapshot.Safety

    /// Generates a time-phased bucketed view of net availability over a horizon
    let getTimePhasedAvailability
        (caps: Supply)
        (productId: string)
        (stockingPointId: string)
        (startDate: DateTimeOffset)
        (bucketDays: int)
        (horizonDays: int)
        : Async<Result<(DateTimeOffset * decimal) list, ProviderError>> =
        async {
            let! snapRes = getSnapshot caps productId stockingPointId startDate
            match snapRes with
            | Error e -> return Error e
            | Ok (snap: MaterialSnapshot) ->
                let buckets = [
                    for i in 0 .. (horizonDays / bucketDays) - 1 do
                        yield startDate.AddDays(float (i * bucketDays))
                ]

                let result =
                    buckets
                    |> List.map (fun bucketStart ->
                        let bucketEnd = bucketStart.AddDays(float bucketDays)
                        let inboundUpTo =
                            snap.Inbound
                            |> List.filter (fun (date, _) -> date < bucketEnd)
                            |> List.sumBy snd
                        let reservationsUpTo =
                            snap.Reservations
                            |> List.filter (fun (date, _) -> date < bucketEnd)
                            |> List.sumBy snd
                        let net = snap.OnHand + inboundUpTo - reservationsUpTo - snap.Safety
                        (bucketStart, net)
                    )
                return Ok result
        }

    /// Generates a daily date-wise availability step-curve over a horizon
    let getDateWiseAvailability
        (caps: Supply)
        (productId: string)
        (stockingPointId: string)
        (startDate: DateTimeOffset)
        (horizonDays: int)
        : Async<Result<(DateTimeOffset * decimal) list, ProviderError>> =
        async {
            let! snapRes = getSnapshot caps productId stockingPointId startDate
            match snapRes with
            | Error e -> return Error e
            | Ok (snap: MaterialSnapshot) ->
                let endDate = startDate.AddDays(float horizonDays)

                // Collect all event dates in horizon, including startDate
                let eventDates =
                    [ startDate ]
                    @ (snap.Inbound |> List.map fst)
                    @ (snap.Reservations |> List.map fst)
                    |> List.filter (fun d -> d >= startDate && d <= endDate)
                    |> List.map (fun d -> d.Date |> DateTimeOffset) // normalize to start of day
                    |> List.distinct
                    |> List.sort

                let result =
                    eventDates
                    |> List.map (fun d ->
                        let endOfDay = d.AddDays(1.0).AddTicks(-1L)
                        let inboundUpTo =
                            snap.Inbound
                            |> List.filter (fun (date, _) -> date <= endOfDay)
                            |> List.sumBy snd
                        let reservationsUpTo =
                            snap.Reservations
                            |> List.filter (fun (date, _) -> date <= endOfDay)
                            |> List.sumBy snd
                        let net = snap.OnHand + inboundUpTo - reservationsUpTo - snap.Safety
                        (d, net)
                    )
                return Ok result
        }

    /// Creates an instance of MaterialProvider record-of-functions
    let createMaterialProvider (caps: Supply) : MaterialProvider =
        { GetSnapshot = getSnapshot caps
          GetSupplierOptions = getSupplierOptions caps
          GetDateWiseAvailability = getDateWiseAvailability caps }
