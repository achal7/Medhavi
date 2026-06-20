module Medhavi.Supply.Application.MaterialProvider

open System
open System.Threading.Tasks
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.Contracts.API
open Medhavi.Contracts.Projections
open Medhavi.Contracts.Supply
open Medhavi.Supply

type MaterialSnapshot =
    { SkuId: SkuId
      StockingPointId: StockingPointId
      AsOf: Timestamp
      OnHand: Quantity
      Inbound: (Timestamp * Quantity) list // Supply orders arriving (date, quantity)
      Reservations: (Timestamp * Quantity) list // Active reservations (date, quantity)
      Safety: Quantity } // Safety stock target

module DomainCore =
    /// Calculates the net available quantity from a snapshot
    let calculateNetAvailable (snapshot: MaterialSnapshot) : Quantity =
        let totalInbound = snapshot.Inbound |> List.map snd |> Quantity.sum

        let totalReservations = snapshot.Reservations |> List.map snd |> Quantity.sum

        // Saturating subtraction (+) and (-) defined on Quantity type
        snapshot.OnHand + totalInbound - totalReservations - snapshot.Safety

    /// Generates a time-phased bucketed view of net availability over a horizon
    let getTimePhasedAvailability
        (snapshot: MaterialSnapshot)
        (startDate: Timestamp)
        (bucketDays: int)
        (horizonDays: int)
        : (Timestamp * Quantity) list =
        let startDto = Timestamp.value startDate

        let buckets =
            [ for i in 0 .. (horizonDays / bucketDays) - 1 do
                  yield Timestamp.create(startDto.AddDays(float(i * bucketDays))) ]

        buckets
        |> List.map(fun bucketStart ->
            let bucketEnd = (Timestamp.value bucketStart).AddDays(float bucketDays)

            let inboundUpTo =
                snapshot.Inbound
                |> List.filter(fun (date, _) -> Timestamp.value date < bucketEnd)
                |> List.map snd
                |> Quantity.sum

            let reservationsUpTo =
                snapshot.Reservations
                |> List.filter(fun (date, _) -> Timestamp.value date < bucketEnd)
                |> List.map snd
                |> Quantity.sum

            let net = snapshot.OnHand + inboundUpTo - reservationsUpTo - snapshot.Safety

            (bucketStart, net))

    /// Generates a daily date-wise availability step-curve over a horizon
    let getDateWiseAvailability
        (snapshot: MaterialSnapshot)
        (startDate: Timestamp)
        (horizonDays: int)
        : (Timestamp * Quantity) list =
        let startDto = Timestamp.value startDate
        let endDate = startDto.AddDays(float horizonDays)

        // Collect all event dates in horizon, including startDate
        let eventDates =
            [ startDto ]
            @ (snapshot.Inbound |> List.map(fst >> Timestamp.value))
            @ (snapshot.Reservations |> List.map(fst >> Timestamp.value))
            |> List.filter(fun d -> d >= startDto && d <= endDate)
            |> List.map(fun d -> DateTimeOffset(d.Date, TimeSpan.Zero)) // normalize to start of day
            |> List.distinct
            |> List.sort
            |> List.map Timestamp.create

        eventDates
        |> List.map(fun d ->
            let dVal = Timestamp.value d
            let endOfDay = dVal.AddDays(1.0).AddTicks(-1L)

            let inboundUpTo =
                snapshot.Inbound
                |> List.filter(fun (date, _) -> Timestamp.value date <= endOfDay)
                |> List.map snd
                |> Quantity.sum

            let reservationsUpTo =
                snapshot.Reservations
                |> List.filter(fun (date, _) -> Timestamp.value date <= endOfDay)
                |> List.map snd
                |> Quantity.sum

            let net = snapshot.OnHand + inboundUpTo - reservationsUpTo - snapshot.Safety

            (d, net))

// ============================================================================
// 3. Adapter Layer (Queries execution and mapping to/from Domain types)
// ============================================================================

let private findInventoryBySkuAndSp (inventoryQuery: InventoryQueryService) (skuId: SkuId) (spId: StockingPointId) =
    task {
        let! allInventories = inventoryQuery.GetAll()

        return
            allInventories
            |> List.filter(fun inv -> inv.SkuId = SkuId.value skuId && inv.StockingPointId = StockingPointId.value spId)
    }

let private findSafetyStock
    (inventoryTargetQuery: InventoryTargetQueryService)
    (skuId: SkuId)
    (spId: StockingPointId)
    (asOf: Timestamp)
    : Task<Quantity> =
    task {
        let! allTargets = inventoryTargetQuery.GetAll()
        let asOfDto = Timestamp.value asOf

        let rawSafety =
            allTargets
            |> List.tryFind(fun target ->
                target.SkuId = SkuId.value skuId
                && target.StockingPointId = StockingPointId.value spId
                && target.IsActive
                && (target.EffectiveStart |> Option.map(fun x -> x <= asOfDto) |> Option.defaultValue true)
                && (target.EffectiveEnd |> Option.map(fun x -> x >= asOfDto) |> Option.defaultValue true))
            |> Option.bind(fun target -> target.SafetyStockQty)
            |> Option.defaultValue 0.0m

        return Quantity.clampToZero rawSafety
    }

/// Fetches a type-safe snapshot of material availability from the read projections
let getSnapshotInternal
    (inventoryQuery: InventoryQueryService)
    (supplyOrderQuery: SupplyOrderQueryService)
    (inventoryTargetQuery: InventoryTargetQueryService)
    (reservationQuery: MaterialReservationQueryService)
    (skuId: SkuId)
    (stockingPointId: StockingPointId)
    (asOf: Timestamp)
    : TaskResult<MaterialSnapshot, ApplicationError> =
    task {
        try
            // 1. Get OnHand Inventory (mapped safely)
            let! inventories = findInventoryBySkuAndSp inventoryQuery skuId stockingPointId

            let onHand = inventories |> List.map(fun inv -> Quantity.clampToZero inv.Quantity) |> Quantity.sum

            // 2. Get Firm Inbound Orders (PO, WO, TO)
            let! allOrders =
                supplyOrderQuery.Filter(fun o ->
                    o.SkuId = SkuId.value skuId
                    && o.StockingPointId = StockingPointId.value stockingPointId
                    && o.State <> "Completed"
                    && o.State <> "Cancelled"
                    && (o.IsFirm || o.State = "Confirmed" || o.State = "Released" || o.State = "InProgress"))

            let inbound =
                allOrders
                |> List.choose(fun o ->
                    let remaining = o.Quantity - o.CompletedQuantity - o.ScrapQuantity

                    if remaining > 0.0m then
                        o.RequiredDeliveryDate
                        |> Option.map(fun d -> Timestamp.create d, Quantity.clampToZero remaining)
                    else
                        None)
                |> List.sortBy fst

            // 3. Get Safety Stock Target (mapped safely)
            let! safety = findSafetyStock inventoryTargetQuery skuId stockingPointId asOf

            // 4. Get active reservations (Tentative or Confirmed, mapped safely)
            let! activeReservations =
                reservationQuery.Filter(fun r ->
                    r.SkuId = SkuId.value skuId
                    && r.StockingPointId = StockingPointId.value stockingPointId
                    && (r.State = "Tentative" || r.State = "Confirmed"))

            let reservations =
                activeReservations
                |> List.map(fun r -> Timestamp.create r.RequiredDate, Quantity.clampToZero r.Quantity)
                |> List.sortBy fst

            let snapshot: MaterialSnapshot =
                { SkuId = skuId
                  StockingPointId = stockingPointId
                  AsOf = asOf
                  OnHand = onHand
                  Inbound = inbound
                  Reservations = reservations
                  Safety = safety }

            return Ok snapshot
        with ex ->
            return Error(ApplicationError.Unknown ex.Message)
    }

/// Fetches supplier options for a SKU
let getSupplierOptionsInternal
    (supplierOfferQuery: SupplierOfferQueryService)
    (skuId: SkuId)
    (stockingPointId: StockingPointId option)
    : TaskResult<SupplierOffer list, ApplicationError> =
    task {
        try
            let! offers =
                supplierOfferQuery.Filter(fun o ->
                    o.SkuId = SkuId.value skuId
                    && (match stockingPointId, o.StockingPointId with
                        | Some sp, Some offerSp -> StockingPointId.value sp = offerSp
                        | None, _ -> true
                        | _, None -> true)
                    && o.IsActive)

            return Ok offers
        with ex ->
            return Error(ApplicationError.Unknown ex.Message)
    }

// ============================================================================
// 4. Backward-Compatible API Wrapper (createMaterialProvider)
// ============================================================================
let createMaterialProvider
    (inventoryQuery: InventoryQueryService)
    (supplyOrderQuery: SupplyOrderQueryService)
    (inventoryTargetQuery: InventoryTargetQueryService)
    (reservationQuery: MaterialReservationQueryService)
    (supplierOfferQuery: SupplierOfferQueryService)
    : MaterialProviderApi =

    let toContractSnap (snap: MaterialSnapshot) : Medhavi.Contracts.Supply.MaterialSnapshot =
        { OnHand = Quantity.value snap.OnHand
          Inbound = snap.Inbound |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)
          Reservations = snap.Reservations |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)
          Safety = Quantity.value snap.Safety }

    { GetSnapshot =
        fun skuIdStr spIdStr asOf ->
            async {
                match SkuId.create skuIdStr, StockingPointId.create spIdStr with
                | Ok skuId, Ok spId ->
                    let! result =
                        getSnapshotInternal
                            inventoryQuery
                            supplyOrderQuery
                            inventoryTargetQuery
                            reservationQuery
                            skuId
                            spId
                            (Timestamp.create asOf)
                        |> Async.AwaitTask

                    match result with
                    | Ok snap -> return Ok(toContractSnap snap)
                    | Error e -> return Error(ApplicationError.mapToApiError e)
                | Error e, _
                | _, Error e -> return Error(ApplicationError.Domain e |> ApplicationError.mapToApiError)
            }

      GetNetAvailable =
        fun skuIdStr spIdStr asOf ->
            async {
                match SkuId.create skuIdStr, StockingPointId.create spIdStr with
                | Ok skuId, Ok spId ->
                    let! result =
                        getSnapshotInternal
                            inventoryQuery
                            supplyOrderQuery
                            inventoryTargetQuery
                            reservationQuery
                            skuId
                            spId
                            (Timestamp.create asOf)
                        |> Async.AwaitTask

                    match result with
                    | Ok snap ->
                        let net = DomainCore.calculateNetAvailable snap
                        return Ok(Quantity.value net)
                    | Error e -> return Error(ApplicationError.mapToApiError e)
                | Error e, _
                | _, Error e -> return Error(ApplicationError.Domain e |> ApplicationError.mapToApiError)

            }

      GetTimePhasedAvailability =
        fun skuIdStr spIdStr asOf bucketDays horizonDays ->
            async {
                match SkuId.create skuIdStr, StockingPointId.create spIdStr with
                | Ok skuId, Ok spId ->
                    let asofTime = Timestamp.create asOf
                    let! result =
                        getSnapshotInternal
                            inventoryQuery
                            supplyOrderQuery
                            inventoryTargetQuery
                            reservationQuery
                            skuId
                            spId
                            asofTime
                        |> Async.AwaitTask

                    match result with
                    | Ok snap ->
                        let availability = DomainCore.getTimePhasedAvailability snap asofTime bucketDays horizonDays

                        let contractAvailability = availability |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)

                        return Ok contractAvailability
                    | Error e -> return Error(ApplicationError.mapToApiError e)
                | Error e, _
                | _, Error e -> return Error(ApplicationError.Domain e |> ApplicationError.mapToApiError)
            }

      GetDateWiseAvailability =
        fun skuIdStr spIdStr asOf horizonDays ->
            async {
                match SkuId.create skuIdStr, StockingPointId.create spIdStr with
                | Ok skuId, Ok spId ->
                    let asofTime = Timestamp.create asOf
                    let! result =
                        getSnapshotInternal
                            inventoryQuery
                            supplyOrderQuery
                            inventoryTargetQuery
                            reservationQuery
                            skuId
                            spId
                            asofTime
                        |> Async.AwaitTask

                    match result with
                    | Ok snap ->
                        let availability = DomainCore.getDateWiseAvailability snap asofTime horizonDays

                        let contractAvailability = availability |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)

                        return Ok contractAvailability
                    | Error e -> return Error(ApplicationError.mapToApiError e)
                | Error e, _
                | _, Error e -> return Error(ApplicationError.Domain e |> ApplicationError.mapToApiError)
            }

      GetSupplierOptions =
        fun skuIdStr spIdStr _ _ ->
            async {
                match SkuId.create skuIdStr with
                | Error e -> return Error(ApplicationError.Domain e |> ApplicationError.mapToApiError)
                | Ok skuId ->
                    let spId =
                        spIdStr
                        |> Option.map(fun s ->
                            StockingPointId.create s |> Result.defaultWith(fun _ -> failwith "Invalid StockingPointId"))

                    let! result = getSupplierOptionsInternal supplierOfferQuery skuId spId |> TaskResult.mapError ApplicationError.mapToApiError |> Async.AwaitTask

                    return result
            } }

let getSnapshot
    (caps: SupplyContext)
    (productId: string)
    (stockingPointId: string)
    (asOf: DateTimeOffset)
    : Async<Result<Medhavi.Contracts.Supply.MaterialSnapshot, ApplicationError>> =
    async {
        match SkuId.create productId, StockingPointId.create stockingPointId with
        | Ok skuId, Ok spId ->
            let! res =
                getSnapshotInternal
                    caps.Queries.Inventory
                    caps.Queries.SupplyOrder
                    caps.Queries.InventoryTarget
                    caps.Queries.MaterialReservation
                    skuId
                    spId
                    (Timestamp.create asOf)
                |> Async.AwaitTask

            match res with
            | Ok snap ->
                let contractSnap: Medhavi.Contracts.Supply.MaterialSnapshot =
                    { OnHand = Quantity.value snap.OnHand
                      Inbound = snap.Inbound |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)
                      Reservations = snap.Reservations |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)
                      Safety = Quantity.value snap.Safety }

                return Ok contractSnap
            | Error e -> return Error e
        | Error e, _
        | _, Error e -> return Error(ApplicationError.Domain e)
    }

let getSupplierOptions
    (caps: SupplyContext)
    (productId: string)
    (stockingPointId: string option)
    (_: decimal)
    (_: DateTimeOffset)
    : Async<Result<SupplierOffer list, ApplicationError>> =
    async {
        match SkuId.create productId with
        | Ok skuId ->
            let spId =
                stockingPointId
                |> Option.map(fun s ->
                    StockingPointId.create s |> Result.defaultWith(fun _ -> failwith "Invalid StockingPointId"))

            let! res = getSupplierOptionsInternal caps.Queries.SupplierOffer skuId spId |> Async.AwaitTask

            return res
        | Error e -> return Error(ApplicationError.Domain e)
    }

let calculateNetAvailable (snapshot: Medhavi.Contracts.Supply.MaterialSnapshot) : decimal =
    let totalInbound = snapshot.Inbound |> List.sumBy snd
    let totalReservations = snapshot.Reservations |> List.sumBy snd

    snapshot.OnHand + totalInbound - totalReservations - snapshot.Safety

let getTimePhasedAvailability
    (caps: SupplyContext)
    (productId: string)
    (stockingPointId: string)
    (startDate: DateTimeOffset)
    (bucketDays: int)
    (horizonDays: int)
    : Async<Result<(DateTimeOffset * decimal) list, ApplicationError>> =
    async {
        match SkuId.create productId, StockingPointId.create stockingPointId with
        | Ok skuId, Ok spId ->
            let! res =
                getSnapshotInternal
                    caps.Queries.Inventory
                    caps.Queries.SupplyOrder
                    caps.Queries.InventoryTarget
                    caps.Queries.MaterialReservation
                    skuId
                    spId
                    (Timestamp.create startDate)
                |> Async.AwaitTask

            match res with
            | Ok snap ->
                let availability =
                    DomainCore.getTimePhasedAvailability snap (Timestamp.create startDate) bucketDays horizonDays

                let mappedAvailability = availability |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)

                return Ok mappedAvailability
            | Error e -> return Error e
        | Error e, _
        | _, Error e -> return Error(ApplicationError.Domain e)
    }

let getDateWiseAvailability
    (caps: SupplyContext)
    (productId: string)
    (stockingPointId: string)
    (startDate: DateTimeOffset)
    (horizonDays: int)
    : Async<Result<(DateTimeOffset * decimal) list, ApplicationError>> =
    async {
        match SkuId.create productId, StockingPointId.create stockingPointId with
        | Ok skuId, Ok spId ->
            let! res =
                getSnapshotInternal
                    caps.Queries.Inventory
                    caps.Queries.SupplyOrder
                    caps.Queries.InventoryTarget
                    caps.Queries.MaterialReservation
                    skuId
                    spId
                    (Timestamp.create startDate)
                |> Async.AwaitTask

            match res with
            | Ok snap ->
                let availability = DomainCore.getDateWiseAvailability snap (Timestamp.create startDate) horizonDays

                let mappedAvailability = availability |> List.map(fun (t, q) -> Timestamp.value t, Quantity.value q)

                return Ok mappedAvailability
            | Error e -> return Error e
        | Error e, _
        | _, Error e -> return Error(ApplicationError.Domain e)
    }
