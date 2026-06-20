namespace Medhavi.Web.Services

open System
open System.Threading
open Medhavi.Contracts
open Medhavi.Contracts.Demand

/// Dummy demand service for testing - generates fake data and auto-adds new demands
type DummyDemandService() =

    let mutable demands =
        [ { DemandLineId = "D-001"
            DemandOrderId = "ORD-001"
            SkuId = "SKU-001"
            SkuCode = "PROD-X"
            SkuName = "Product X"
            CustomerId = "CUST-001"
            CustomerName = "Acme Corporation"
            StockingPointId = "SP-001"
            Priority = 1
            DemandCategory = "CustomerOrder"
            IsFirm = true
            EarliestDeliveryDate = Some(DateOnly(2025, 1, 1))
            RequestedDeliveryDate = DateOnly(2025, 1, 15)
            LatestDeliveryDate = Some(DateOnly(2025, 1, 20))
            ConfirmedDeliveryDate = None
            RequestedQty = 100.0m
            OpenQty = 100.0m
            FulfilledQty = 0.0m
            ConfirmedQty = 0.0m
            ShortfallQty = 100.0m
            LatenessRisk = LatenessRisk.OnTrack
            Status = "New"
            UnitOfMeasure = "EA"
            PeggedSupply = [] }

          { DemandLineId = "D-002"
            DemandOrderId = "ORD-002"
            SkuId = "SKU-002"
            SkuCode = "PROD-Y"
            SkuName = "Product Y"
            CustomerId = "CUST-002"
            CustomerName = "Beta Industries"
            StockingPointId = "SP-001"
            Priority = 2
            DemandCategory = "CustomerOrder"
            IsFirm = true
            EarliestDeliveryDate = Some(DateOnly(2025, 1, 5))
            RequestedDeliveryDate = DateOnly(2025, 1, 20)
            LatestDeliveryDate = Some(DateOnly(2025, 1, 25))
            ConfirmedDeliveryDate = None
            RequestedQty = 50.0m
            OpenQty = 50.0m
            FulfilledQty = 0.0m
            ConfirmedQty = 0.0m
            ShortfallQty = 50.0m
            LatenessRisk = LatenessRisk.OnTrack
            Status = "New"
            UnitOfMeasure = "EA"
            PeggedSupply = [] } ]

    let demandIdCounter = ref 2
    let timerLock = obj()
    let mutable timer: Timer option = None
    let subscribers = ResizeArray<unit -> unit>()

    let createNewDemand () =
        lock demandIdCounter (fun () ->
            demandIdCounter := !demandIdCounter + 1
            let id = !demandIdCounter

            { DemandLineId = sprintf "D-%03d" id
              DemandOrderId = sprintf "ORD-%03d" id
              SkuId = sprintf "SKU-%03d" (id % 5 + 1)
              SkuCode = sprintf "PROD-%c" (char(65 + id % 26))
              SkuName = sprintf "Product %c" (char(65 + id % 26))
              CustomerId = sprintf "CUST-%03d" (id % 10 + 1)
              CustomerName = sprintf "Customer %d" (id % 10 + 1)
              StockingPointId = "SP-001"
              Priority = (id % 5) + 1
              DemandCategory = "CustomerOrder"
              IsFirm = true
              EarliestDeliveryDate = Some(DateOnly(2025, 1, 1).AddDays(id))
              RequestedDeliveryDate = DateOnly(2025, 1, 15).AddDays(id)
              LatestDeliveryDate = Some(DateOnly(2025, 1, 20).AddDays(id))
              ConfirmedDeliveryDate = None
              RequestedQty = decimal(id * 10 + 50)
              OpenQty = decimal(id * 10 + 50)
              FulfilledQty = 0.0m
              ConfirmedQty = 0.0m
              ShortfallQty = decimal(id * 10 + 50)
              LatenessRisk =
                if id % 3 = 0 then LatenessRisk.Critical
                elif id % 2 = 0 then LatenessRisk.AtRisk(id % 7 + 1)
                else LatenessRisk.OnTrack
              Status = [ "New"; "InProgress"; "Hold" ].[id % 3]
              UnitOfMeasure = "EA"
              PeggedSupply = [] })

    let notifySubscribers () =
        subscribers
        |> Seq.iter(fun notify ->
            try
                notify()
            with ex ->
                printfn $"[DummyDemandService] Subscriber error: {ex.Message}")

    let addDemand () =
        let newDemand = createNewDemand()
        lock demands (fun () -> demands <- newDemand :: demands)
        notifySubscribers()

    let getById (id: string) =
        async {
            let result = lock demands (fun () -> demands |> List.tryFind(fun d -> d.DemandLineId = id))

            match result with
            | Some demand -> return Ok demand
            | None -> return Error $"Demand {id} not found"
        }
        |> Async.StartAsTask

    let getAll (context: PlanningContext) =
        async {
            do! Async.Sleep(100)
            let result = lock demands (fun () -> List.ofSeq demands)
            return Ok result
        }
        |> Async.StartAsTask

    let create (req: DemandDefineReq) =
        async {
            let newDemand =
                { DemandLineId = req.DemandLineId
                  DemandOrderId = req.DemandOrderId
                  SkuId = req.SkuId
                  SkuCode = req.SkuId
                  SkuName = req.SkuId
                  CustomerId = req.CustomerId
                  CustomerName = req.CustomerId
                  StockingPointId = req.StockingPointId
                  Priority = req.Priority
                  DemandCategory = req.DemandCategory
                  IsFirm = req.IsFirm
                  EarliestDeliveryDate = req.EarliestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.Date))
                  RequestedDeliveryDate = DateOnly.FromDateTime(req.RequestedDeliveryDate.Date)
                  LatestDeliveryDate = req.LatestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.Date))
                  ConfirmedDeliveryDate = None
                  RequestedQty = req.Quantity
                  OpenQty = req.Quantity
                  FulfilledQty = 0.0m
                  ConfirmedQty = 0.0m
                  ShortfallQty = req.Quantity
                  LatenessRisk = LatenessRisk.OnTrack
                  Status = "New"
                  UnitOfMeasure = req.UnitOfMeasure
                  PeggedSupply = [] }

            lock demands (fun () -> demands <- newDemand :: demands)

            notifySubscribers()
            return Ok newDemand
        }
        |> Async.StartAsTask

    let update (req: DemandDefineReq) = async { return Error "Not implemented" } |> Async.StartAsTask

    let delete (id: string) = async { return Ok() } |> Async.StartAsTask

    member this.IdemandService =
        { GetById = getById
          GetAll = getAll
          Create = create
          Update = update
          Delete = delete }

    member this.Subscribe(notify: unit -> unit) = lock timerLock (fun () -> subscribers.Add(notify))

    member this.StartAutoUpdate(intervalMs: int) =
        lock timerLock (fun () ->
            match timer with
            | Some _ -> ()
            | None ->
                let newTimer = new Timer((fun _ -> addDemand()), null, intervalMs, intervalMs)
                timer <- Some newTimer)

    member this.StopAutoUpdate() =
        lock timerLock (fun () ->
            match timer with
            | Some t ->
                t.Dispose()
                timer <- None
            | None -> ())
