namespace Medhavi.Domain.Tests

open System
open Expecto
open Swensen.Unquote
open Medhavi.SharedKernel
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise
open Medhavi.Promise.PromiseTypes
open Medhavi.Promise.PromiseService
open Medhavi.Promise.CostCalculation
open Medhavi.Promise.LimiterSelection
open Medhavi.Transport

module PromiseTests =

    let getOk res =
        match res with
        | Ok v -> v
        | Error e -> failwithf "Failed to create ID: %A" e

    // Setup Mock Providers
    let createMockMaterialProvider onHand inbound =
        { GetSnapshot =
            fun (skuId, stockingPointId, _asOf) ->
                async {
                    let snap: MaterialSnapshot =
                        { SkuId = skuId
                          StockingPointId = stockingPointId
                          OnHand = onHand
                          Inbound = inbound
                          Reservations = 0m
                          Safety = 0m }
                    return Ok snap
                }
          GetSupplierOptions =
            fun (_skuId, _stockingPointId, _qty, _asOf) ->
                async { return Ok [] } }

    let createMockCapacityProvider suggestedDate bottleneck =
        { CheckCapacity =
            fun (_skuId, _qty, _asOf) ->
                async {
                    let result: CapacityCheckResult =
                        { IsFeasible = true
                          SuggestedDate = suggestedDate
                          RequiredLoads = Map.empty
                          BottleneckResourceId = bottleneck
                          LatenessReason = None
                          EarliestAvailable = DateTimeOffset.UtcNow }
                    return Ok result
                } }

    let createMockTransportProvider leadTimeMinutes fixedCost varCost =
        { GetOptions =
            fun (origin, dest, _asOf) ->
                async {
                    let itinerary =
                        { Id = ItineraryId.generate()
                          SkuId = None
                          FromNode = origin
                          ToNode = dest
                          Hops = []
                          TotalLeadTimeMinutes = leadTimeMinutes
                          TotalFixedCost = fixedCost
                          TotalVariableCostPerUnit = Some varCost
                          TotalCO2 = None
                          TotalReliability = 0.95m
                          HopCount = 0 }
                    return Ok [ itinerary ]
                } }

    let createMockRoutingProvider () : RoutingProvider =
        let dur = TimeSpan.FromHours(24.0)
        let routingId =
            match RoutingId.create "RT-DEFAULT" with
            | Ok id -> id
            | Error _ -> failwith "Invalid routing id"
        
        let routingSelection =
            { Primary =
                { RoutingId = routingId
                  AlternateUsed = false
                  EstimatedDuration = Some dur
                  Reliability = Some 0.95m }
              Alternates = [] }
        
        { Select =
            fun (_skuId, _stockingPointId) ->
                async {
                    return Ok routingSelection
          } }

    let mockTenantProvider =
        { GetTenant = fun () -> "tenant-default", TimeZoneInfo.Utc, Some "USD" }

    let defaultOrderLine due sku sp qty =
        { LineId = "line-1"
          SkuId = sku
          StockingPointId = sp
          Quantity = Quantity.clampToZero qty
          DueDate = due
          Priority = 1
          IsExpedited = false
          Origin = Some sp
          Destination = Some sp }

    [<Tests>]
    let tests =
        testList
            "Promise Orchestrator Tests"
            [ testCase "should successfully promise and calculate correct date, cost, and confidence" (fun () ->
                  let sku = SkuId.create "SKU-A" |> getOk
                  let sp = StockingPointId.create "SP-1" |> getOk
                  let asOf = DateTimeOffset.UtcNow
                  let due = asOf.AddDays(5.0)

                  // Local refs to avoid cross-test concurrency issues in Expecto
                  let createdRequests = ref []
                  let releasedResIds = ref []
                  let resvProv =
                      { CreateTentative =
                          fun reqs ->
                              async {
                                  createdRequests := !createdRequests @ reqs
                                  let ids = reqs |> List.map (fun r -> $"res-{r.Scope}-{Guid.NewGuid()}")
                                  return Ok ids
                              }
                        Confirm = fun _ -> async { return Ok() }
                        Release =
                          fun ids ->
                              async {
                                  releasedResIds := !releasedResIds @ ids
                                  return Ok()
                              } }

                  let matProv = createMockMaterialProvider 100m []
                  let capProv = createMockCapacityProvider (asOf.AddDays(2.0)) (Some "RES-1")
                  let transProv = createMockTransportProvider 1440m 50m 5m // 1 day, $50 fixed, $5 var/unit
                  let routingProv = createMockRoutingProvider()

                  let line = defaultOrderLine due sku sp 10m
                  let orderId = OrderId.create (Guid.NewGuid().ToString()) |> getOk
                  let order = { OrderId = orderId; Lines = [ line ]; CustomerId = Some "CUST-1"; RequestDate = asOf }
                  let req = { Order = order; AsOfDate = asOf; CustomerTier = None; SkuTier = None; Currency = None }

                  let response =
                      tryPromiseOrder matProv capProv transProv routingProv resvProv mockTenantProvider req
                      |> Async.RunSynchronously

                  match response with
                  | Error e -> failwithf "Failed with error: %A" e
                  | Ok resp ->
                      test <@ resp.Decision = PromiseDecisionStatus.Accepted @>
                      
                      let expectedCommitted = asOf.AddDays(2.0)
                      match resp.PromiseDate with
                      | Some d ->
                          test <@ d.Committed = expectedCommitted @>
                          test <@ d.Earliest = expectedCommitted @>
                          test <@ d.Latest = expectedCommitted @>
                      | None -> failwith "Expected promise date"

                      // Check that reservations are created in all 3 domains
                      test <@ List.length resp.Reservations = 3 @>
                      test <@ List.length !createdRequests = 3 @>

                      // Check cost calculation
                      match resp.Cost with
                      | Some cost ->
                          test <@ cost.MaterialCost = 100m @>
                          test <@ cost.TransportCost = 100m @>
                          test <@ cost.TotalCost = 200m @>
                      | None -> failwith "Expected cost breakdown"

                      test <@ resp.Confidence = Some 0.95 @>
                      test <@ List.isEmpty !releasedResIds @>
              )

              testCase "should trigger FullOrder rollback if one line fails" (fun () ->
                  let sku = SkuId.create "SKU-A" |> getOk
                  let skuB = SkuId.create "SKU-B" |> getOk
                  let sp = StockingPointId.create "SP-1" |> getOk
                  let asOf = DateTimeOffset.UtcNow
                  let due = asOf.AddDays(5.0)

                  let createdRequests = ref []
                  let releasedResIds = ref []
                  let resvProv =
                      { CreateTentative =
                          fun reqs ->
                              async {
                                  createdRequests := !createdRequests @ reqs
                                  let ids = reqs |> List.map (fun r -> $"res-{r.Scope}-{Guid.NewGuid()}")
                                  return Ok ids
                              }
                        Confirm = fun _ -> async { return Ok() }
                        Release =
                          fun ids ->
                              async {
                                  releasedResIds := !releasedResIds @ ids
                                  return Ok()
                              } }

                  let matProv =
                      { GetSnapshot =
                          fun (skuId, stockingPointId, asOf) ->
                              async {
                                  if SkuId.value skuId = "SKU-B" then
                                      return Error ProviderError.Unavailable
                                  else
                                      return Ok { SkuId = skuId; StockingPointId = stockingPointId; OnHand = 100m; Inbound = []; Reservations = 0m; Safety = 0m }
                              }
                        GetSupplierOptions = fun _ -> async { return Ok [] } }

                  let capProv = createMockCapacityProvider asOf None
                  let transProv = createMockTransportProvider 0m 0m 0m
                  let routingProv = createMockRoutingProvider()

                  let line1 = { defaultOrderLine due sku sp 10m with LineId = "line-1" }
                  let line2 = { defaultOrderLine due skuB sp 10m with LineId = "line-2" }
                  
                  let orderId = OrderId.create (Guid.NewGuid().ToString()) |> getOk
                  let order = { OrderId = orderId; Lines = [ line1; line2 ]; CustomerId = Some "CUST-1"; RequestDate = asOf }
                  
                  let req = { Order = order; AsOfDate = asOf; CustomerTier = Some "gold"; SkuTier = None; Currency = None }

                  let response =
                      tryPromiseOrder matProv capProv transProv routingProv resvProv mockTenantProvider req
                      |> Async.RunSynchronously

                  match response with
                  | Error e -> failwithf "Failed: %A" e
                  | Ok resp ->
                      test <@ resp.Decision = PromiseDecisionStatus.Rejected @>
                      test <@ resp.Limiter |> Option.map (fun l -> l.Code) = Some PromiseReasonCode.FullOrderViolation @>
                      test <@ not (List.isEmpty !releasedResIds) @>
              )

              testCase "should reject and roll back if CostCap is exceeded" (fun () ->
                  let sku = SkuId.create "SKU-A" |> getOk
                  let sp = StockingPointId.create "SP-1" |> getOk
                  let asOf = DateTimeOffset.UtcNow
                  let due = asOf.AddDays(5.0)

                  let matProv = createMockMaterialProvider 100m []
                  let capProv = createMockCapacityProvider asOf None
                  let transProv = createMockTransportProvider 0m 1000m 0m
                  let resvProv =
                      { CreateTentative = fun reqs -> async { return Ok (reqs |> List.map (fun r -> $"res-{r.Scope}")) }
                        Confirm = fun _ -> async { return Ok() }
                        Release = fun _ -> async { return Ok() } }
                  let routingProv = createMockRoutingProvider()

                  let line = defaultOrderLine due sku sp 10m
                  let orderId = OrderId.create (Guid.NewGuid().ToString()) |> getOk
                  let order = { OrderId = orderId; Lines = [ line ]; CustomerId = Some "CUST-1"; RequestDate = asOf }
                  
                  let customPolicy = { PolicyPresets.silverPreset with CostCap = Some 500m }
                  
                  let dates = asOf
                  let cost = CostCalculation.calculateCost customPolicy 10m (Some { SkuId = sku; StockingPointId = sp; OnHand = 100m; Inbound = []; Reservations = 0m; Safety = 0m }) None (Some { Id = ItineraryId.generate(); SkuId = None; FromNode = "SP-1"; ToNode = "SP-1"; Hops = []; TotalLeadTimeMinutes = 0m; TotalFixedCost = 1000m; TotalVariableCostPerUnit = None; TotalCO2 = None; TotalReliability = 1m; HopCount = 0 }) dates due 100.0m None None
                  test <@ cost.TotalCost > 500m @>
              )

              testCase "should select correct limiter when date is pushed beyond due date" (fun () ->
                  let asOf = DateTimeOffset.UtcNow
                  let due = asOf.AddDays(1.0)
                  
                  let (dates: LimiterSelection.ReadyDates) =
                      { MaterialReady = Some asOf
                        CapacityReady = Some(asOf.AddDays(10.0))
                        TransportReady = Some asOf
                        AsOf = asOf }

                  let limiter = LimiterSelection.selectLimiter dates
                  match limiter with
                  | Some l ->
                      test <@ l.Domain = PromiseLimiterDomain.Capacity @>
                      test <@ l.Code = PromiseReasonCode.CapacityShortfall @>
                  | None -> failwith "Expected capacity limiter"
              )
            ]