module Medhavi.Nexus.DemandService

open System.Threading
open System.Threading.Tasks
open System
open Medhavi.Common.Patterns
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Medhavi.Integration
open Medhavi.Infrastructure.Stores.EnvelopeStore
open Medhavi.SharedKernel

type Service =
    { Context: DemandContext
      IntegrationHandler: SubscriptionHandle }

let handler (context: DemandContext) (event: IntegrationEvent) : TaskResult<unit, ApplicationError> =
    taskResult {
        match event with
        | DemandsImported demands ->
            for d in demands do
                let req: DemandDefineReq =
                    { DemandLineId = d.DemandLineId
                      DemandOrderId = d.DemandOrderId
                      SkuId = d.SkuId
                      StockingPointId = d.StockingPointId
                      CustomerId = d.CustomerId
                      Quantity = d.Quantity
                      UnitOfMeasure = d.UnitOfMeasure
                      OrderDate = d.OrderDate
                      EarliestDeliveryDate = d.EarliestDeliveryDate
                      RequestedDeliveryDate = d.RequestedDeliveryDate
                      LatestDeliveryDate = d.LatestDeliveryDate
                      ConfirmedDeliveryDate = d.ConfirmedDeliveryDate
                      ActualDeliveryDate = d.ActualDeliveryDate
                      Priority = d.Priority
                      DemandCategory = d.DemandCategory.ToLower()
                      IsFirm = d.IsFirm
                      IsFrozen = d.IsFrozen }

                do!
                    context.Commands.Define(req)
                    |> TaskResult.mapError(fun err -> ApplicationError.Unknown $"{err.ToString()}")
        | _ -> return ()
    }

let startSimulator (context: DemandContext) =
    let rec loop () =
        async {
            do! Async.Sleep(60000)
            printfn "[Demand Simulator] Running simulation tick..."
            let simId = $"SIM-{DateTime.UtcNow.Ticks}"

            // 1. Create a demand
            let defineReq =
                { DemandLineId = simId
                  DemandOrderId = $"ORD-{simId}"
                  SkuId = "SKU-BIKE"
                  StockingPointId = "SP-FACTORY"
                  CustomerId = "CUST-SIM"
                  Quantity = 10m
                  UnitOfMeasure = "UOM-PCS"
                  OrderDate = DateTimeOffset.Now
                  EarliestDeliveryDate = None
                  RequestedDeliveryDate = DateTimeOffset.Now
                  LatestDeliveryDate = None
                  ConfirmedDeliveryDate = None
                  ActualDeliveryDate = None
                  Priority = 1
                  DemandCategory = "customerorder"
                  IsFirm = true
                  IsFrozen = false }

            printfn $"[Demand Simulator] Creating demand %s{simId}..."
            let! res1 = context.Commands.Define(defineReq) |> Async.AwaitTask

            match res1 with
            | Error err -> printfn $"[Demand Simulator] Error defining demand: %A{err}"
            | Ok() ->
                printfn $"[Demand Simulator] Demand %s{simId} created successfully."

                // 2. Fulfill the demand after 30 seconds
                do! Async.Sleep(30000)
                let fulfillReq = { DemandLineId = simId; Quantity = 10m }
                printfn $"[Demand Simulator] Fulfilling demand %s{simId}..."
                let! res3 = context.Commands.Fulfill(fulfillReq) |> Async.AwaitTask

                match res3 with
                | Error err -> printfn $"[Demand Simulator] Error fulfilling demand: %A{err}"
                | Ok() -> printfn $"[Demand Simulator] Demand %s{simId} fulfilled successfully."

            return! loop()
        }

    Async.Start(loop())

let create
    (integrationStore: EnvelopeStoreOps)
    (extractEnvelope: IntegrationService.ExtractEnvelope)
    : TaskResult<Service, ApplicationError> =
    taskResult {
        let context = BoundedContext.create()

        let handleEvents (envelopedEvent: EnvelopedEvent) : Task<unit> =
            task {
                match extractEnvelope envelopedEvent with
                | Error e -> printfn $"[ Demand ] Error while deserializing envelope: {e.ToString()}"
                | Ok event ->
                    let! res = handler context event

                    match res with
                    | Ok() -> ()
                    | Error err ->
                        printfn
                            $"[ Demand ] Error while processing event: Code={err.Code}, Message={err.Message} ({err.ToString()})"
            }

        let! integrationHandler =
            integrationStore.Subscribe SubscriptionMode.All None handleEvents CancellationToken.None
            |> TaskResult.mapError(fun e -> ApplicationError.Unknown $"{e.ToString()}")

        return
            { Context = context
              IntegrationHandler = integrationHandler }
    }

(*let getDemands (scenarioId: string option) (demandCtx: DemandContext) : Task<DemandLine list> =
    task {
        let! stateMap = demandCtx.DemandAgent.GetStateAsync()

        return
            stateMap.Values
            |> Seq.toList
            |> List.map(fun (d: Domain.DemandLine) ->
                let skuIdStr = SkuId.value d.SkuId

                { DemandLineId = d.DemandLineId
                  DemandOrderId = d.DemandOrderId
                  SkuId = skuIdStr
                  SkuCode = skuIdStr
                  SkuName = skuIdStr
                  CustomerId = d.CustomerId
                  CustomerName = d.CustomerId
                  StockingPointId = StockingPointId.value d.StockingPointId
                  Priority = d.Priority
                  DemandCategory = d.DemandCategory.ToString()
                  IsFirm = d.IsFirm
                  EarliestDeliveryDate =
                    d.EarliestDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
                  RequestedDeliveryDate = DateOnly.FromDateTime(d.RequestedDeliveryDate.DateTime)
                  LatestDeliveryDate = d.LatestDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
                  ConfirmedDeliveryDate =
                    d.ConfirmedDeliveryDate |> Option.map(fun dt -> DateOnly.FromDateTime(dt.DateTime))
                  RequestedQty = Quantity.value d.Quantity
                  OpenQty = Quantity.value d.OpenQuantity
                  FulfilledQty = Quantity.value d.FulfilledQuantity
                  ConfirmedQty = Quantity.value d.Quantity - Quantity.value d.OpenQuantity
                  ShortfallQty = Quantity.value d.OpenQuantity
                  LatenessRisk = LatenessRisk.OnTrack
                  Status = d.Status.ToString()
                  UnitOfMeasure = d.UnitOfMeasure
                  PeggedSupply = [] })

    // match scenarioId with
    // | None
    // | Some "BASELINE" -> return baselineDemands
    // | Some id ->
    //     let! scOpt = scenarioContext.Queries.GetById id
    //     match scOpt with
    //     | None -> return baselineDemands
    //     | Some sc ->
    //         let overlay = ScenarioAdapter.toScenarioOverlay id sc.Overrides
    //         return baselineDemands |> List.map (ScenarioAdapter.applyDemandOverlay overlay)
    }

let createDemandService (demandQueries: DemandQueries) : IDemandService =
    let mockDemands =
        [ { DemandLineId = "DL-1002"
            DemandOrderId = "DO-5002"
            SkuId = "SKU-002"
            SkuCode = "STEEL-PL-02"
            SkuName = "Premium Steel Plate 20mm"
            CustomerId = "CUST-302"
            CustomerName = "Global Logistics Corp"
            StockingPointId = "SP-WEST-01"
            Priority = 2
            DemandCategory = "CustomerOrder"
            IsFirm = true
            EarliestDeliveryDate = Some(DateOnly.FromDateTime(DateTime.Today.AddDays(2.0)))
            RequestedDeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(14.0))
            LatestDeliveryDate = Some(DateOnly.FromDateTime(DateTime.Today.AddDays(20.0)))
            ConfirmedDeliveryDate = Some(DateOnly.FromDateTime(DateTime.Today.AddDays(16.0)))
            RequestedQty = 180m
            OpenQty = 180m
            FulfilledQty = 0m
            ConfirmedQty = 150m
            ShortfallQty = 30m
            LatenessRisk = LatenessRisk.AtRisk 2
            Status = "PartiallyConfirmed"
            UnitOfMeasure = "PCS"
            PeggedSupply = [] }
          { DemandLineId = "DL-1003"
            DemandOrderId = "DO-5003"
            SkuId = "SKU-003"
            SkuCode = "COP-ROD-01"
            SkuName = "Industrial Copper Rod 5m"
            CustomerId = "CUST-303"
            CustomerName = "Apex Power Systems"
            StockingPointId = "SP-EAST-01"
            Priority = 3
            DemandCategory = "Forecast"
            IsFirm = false
            EarliestDeliveryDate = None
            RequestedDeliveryDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5.0))
            LatestDeliveryDate = None
            ConfirmedDeliveryDate = None
            RequestedQty = 500m
            OpenQty = 500m
            FulfilledQty = 0m
            ConfirmedQty = 0m
            ShortfallQty = 500m
            LatenessRisk = LatenessRisk.Critical
            Status = "Unfulfilled"
            UnitOfMeasure = "M"
            PeggedSupply = [] } ]

    { GetById =
        fun demandLineId ->
            task {
                match mockDemands |> List.tryFind(fun d -> d.DemandLineId = demandLineId) with
                | Some d -> return Ok d
                | None -> return Error(sprintf "Demand line %s not found" demandLineId)
            }
      GetAll = fun _ -> task { return Ok mockDemands }
      Create =
        fun req ->
            task {
                return
                    Ok(
                        { DemandLineId = req.DemandLineId
                          DemandOrderId = req.DemandOrderId
                          SkuId = req.SkuId
                          SkuCode = "MOCK-CODE"
                          SkuName = "Mock Product"
                          CustomerId = req.CustomerId
                          CustomerName = "Mock Customer"
                          StockingPointId = req.StockingPointId
                          Priority = req.Priority
                          DemandCategory = req.DemandCategory
                          IsFirm = req.IsFirm
                          EarliestDeliveryDate =
                            req.EarliestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          RequestedDeliveryDate = DateOnly.FromDateTime(req.RequestedDeliveryDate.DateTime)
                          LatestDeliveryDate =
                            req.LatestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          ConfirmedDeliveryDate =
                            req.ConfirmedDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          RequestedQty = req.Quantity
                          OpenQty = req.Quantity
                          FulfilledQty = 0m
                          ConfirmedQty = 0m
                          ShortfallQty = req.Quantity
                          LatenessRisk = LatenessRisk.Critical
                          Status = "Unfulfilled"
                          UnitOfMeasure = req.UnitOfMeasure
                          PeggedSupply = [] }
                    )
            }
      Update =
        fun req ->
            task {
                return
                    Ok(
                        { DemandLineId = req.DemandLineId
                          DemandOrderId = req.DemandOrderId
                          SkuId = req.SkuId
                          SkuCode = "MOCK-CODE"
                          SkuName = "Mock Product"
                          CustomerId = req.CustomerId
                          CustomerName = "Mock Customer"
                          StockingPointId = req.StockingPointId
                          Priority = req.Priority
                          DemandCategory = req.DemandCategory
                          IsFirm = req.IsFirm
                          EarliestDeliveryDate =
                            req.EarliestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          RequestedDeliveryDate = DateOnly.FromDateTime(req.RequestedDeliveryDate.DateTime)
                          LatestDeliveryDate =
                            req.LatestDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          ConfirmedDeliveryDate =
                            req.ConfirmedDeliveryDate |> Option.map(fun d -> DateOnly.FromDateTime(d.DateTime))
                          RequestedQty = req.Quantity
                          OpenQty = req.Quantity
                          FulfilledQty = 0m
                          ConfirmedQty = 0m
                          ShortfallQty = req.Quantity
                          LatenessRisk = LatenessRisk.Critical
                          Status = "Unfulfilled"
                          UnitOfMeasure = req.UnitOfMeasure
                          PeggedSupply = [] }
                    )
            }
      Delete = fun _ -> task { return Ok() } }
*)
