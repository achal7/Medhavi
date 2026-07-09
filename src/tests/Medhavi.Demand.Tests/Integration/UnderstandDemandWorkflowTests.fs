module Medhavi.Demand.Tests.Integration.UnderstandDemandWorkflowTests

open System
open Expecto
open Medhavi.SharedKernel
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.Contracts.Demand.Edp
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.Demand
open Medhavi.Demand.DemandObservation
open Medhavi.Demand.PlanningScope
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.Tests.DemandObservation.EvolveTests
open Medhavi.Demand.EnterpriseDemandPicture
open Medhavi.Demand.ForecastPublication
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Contracts
open Medhavi.Demand.Tests.Builders
open Medhavi.Demand.Tests.PlanningScope.EvolveTests

let mockQueries (pubMap: Map<string, ForecastPublication>) : ForecastPublicationQueries =
    { GetAll = fun () -> task { return Map.values pubMap |> Seq.toList }
      GetById = fun id -> task { return Map.tryFind id pubMap }
      Exists = fun id -> task { return Map.containsKey id pubMap }
      Filter = fun f -> task { return Map.values pubMap |> Seq.filter f |> Seq.toList }
      SubscribeApiEvents =
        fun _ ->
            { new IDisposable with
                member _.Dispose() = () } }

let getForecastCtx () =
    let compService = ComputationService.create(fun _ _ -> task { return [ 100m ] })

    let forecastRepo =
        createInMemoryRepository<
            Model.ForecastPublication,
            string,
            Model.ForecastPublicationEvent
         >()

    Context.create forecastRepo compService (fun _ -> ())

[<Tests>]
let tests =
    let forecastCtx = getForecastCtx()

    testList
        "UnderstandDemandWorkflow Integration"
        [ testCaseTask "DetermineScopeAndAssign creates planning scope and assigns it"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<Model.DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<Model.PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<Model.EnterpriseDemandPicture, string, EdpEvent>()

                  // 1. Pre-seed observation in Accepted status (required for assignment)
                  let acceptedObs = defaultReceived |> Tests.DemandObservation.EvolveTests.withStatus Accepted
                  let! _ = obsRepo.Save("OBS-001", acceptedObs, [])

                  // 2. Construct Context/Workflow
                  let ctx = UnderstandDemandContext.create obsRepo scopeRepo edpRepo forecastCtx.Queries (fun _ -> ())

                  let assignReq: AssignScopeReq =
                      { ObservationId = "OBS-001"
                        PlanningScopeId = "" } // will be filled by workflow

                  let scopeReq: DeterminePlanningScopeReq =
                      { SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        BucketType = "Weekly"
                        CustomerId = None
                        PlanningPeriod = PlanningPeriod.PlanningWeek(2027, 27) }

                  // 3. Run Workflow
                  let! result = ctx.Workflow.DetermineScopeAndAssign assignReq scopeReq

                  Expect.isOk result "Workflow should complete successfully"

                  let expectedScopeKey =
                      let custStr = "ALL"
                      $"SKU-001-SP-001-{custStr}-W-2027-27"

                  // 4. Verify Planning Scope is created
                  let! scopeOptRes = scopeRepo.Get expectedScopeKey

                  match scopeOptRes with
                  | Error e -> failwith $"Failed to get scope from repository: {e.ToString()}"
                  | Ok scopeOpt ->
                      Expect.isSome scopeOpt "Planning Scope should exist"
                      Expect.equal scopeOpt.Value.Status Active "Planning Scope status should be Active"

                      // 5. Verify Observation has PlanningScopeId assigned
                      let! obsOptRes = obsRepo.Get "OBS-001"

                      match obsOptRes with
                      | Error e -> failwith $"Failed to get observation from repository: {e.ToString()}"
                      | Ok obsOpt ->

                          Expect.isSome obsOpt "Observation should exist"

                          Expect.equal
                              obsOpt.Value.PlanningScopeId
                              (Some(scopeId expectedScopeKey))
                              "Observation should be assigned to the scope"

                  return ()
              }

          testCaseTask "BuildAndPublishEdp revises, calculates and publishes EDP"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()

                  // 1. Pre-seed observation in Accepted status
                  let acceptedObs = defaultReceived |> Tests.DemandObservation.EvolveTests.withStatus Accepted
                  let! _ = obsRepo.Save("OBS-001", acceptedObs, [])

                  // 2. Pre-seed active planning scope
                  let scope = defaultScope |> withStatus Active
                  let! _ = scopeRepo.Save("SCOPE-001", scope, [])

                  // 3. Setup context and listen for EDP publish notification
                  let ctx = UnderstandDemandContext.create obsRepo scopeRepo edpRepo forecastCtx.Queries (fun _ -> ())

                  let mutable publishedNotif = None

                  use _sub =
                      DomainEventBus.Subscribe<EnterpriseDemandPicturePublishedNotification>(fun ev ->
                          if ev.PlanningScopeId = "SCOPE-001" then
                              publishedNotif <- Some ev)

                  let reviseReq: ReviseEnterpriseDemandPictureReq =
                      { PlanningScopeId = "SCOPE-001"
                        Period = PlanningPeriod.PlanningWeek(2027, 27)
                        Quantity = 50m
                        ObservationId = "OBS-001" }

                  // 4. Run Workflow
                  let! result = ctx.Workflow.BuildAndPublishEdp reviseReq

                  match result with
                  | Error e -> failwithf "Workflow failed: %A" e
                  | Ok scopeId ->
                      Expect.equal scopeId "SCOPE-001" "Should return scope ID"

                      // Verify EDP is published
                      let! edpOptRes = edpRepo.Get "SCOPE-001"

                      match edpOptRes with
                      | Error e -> failwith $"Failed to get EDP from repository: {e.ToString()}"
                      | Ok edpOpt ->
                          Expect.isSome edpOpt "EDP should exist"
                          Expect.equal edpOpt.Value.Status Published "EDP should be Published"

                          // Verify notification published
                          Expect.isSome publishedNotif "EDP publish notification should be triggered"

                      let notif = publishedNotif.Value
                      Expect.equal notif.PlanningScopeId "SCOPE-001" "Notification scopeId should match"
                      Expect.equal notif.Version 1 "Notification version should be 1"
              }

          testCaseTask "OnForecastPublished ingests published forecast lines as a batch"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()

                  // 1. Prepare published forecast publication mock
                  let fc1: Forecast =
                      { ForecastId = "FC-001"
                        SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        PlanningPeriod = PlanningPeriod.PlanningWeek(2027, 27)
                        Mean = 120m
                        LowerBound = 100m
                        UpperBound = 140m
                        Confidence = 0.9m
                        ModelId = "Model-A"
                        OverrideReason = None }

                  let pub: ForecastPublication =
                      { PublicationId = "PUB-001"
                        Version = 1
                        Status = "Published"
                        PlanningScopeIds = [ "SCOPE-001" ]
                        ForecastHorizon = "7.00:00:00"
                        ChampionModelId = Some "Model-A"
                        OverallConfidenceIndex = Some 0.9m
                        Coverage = []
                        Forecasts = [ fc1 ]
                        Assumptions = []
                        Overrides = []
                        TransactionTime = DateTimeOffset.UtcNow
                        PublicationTime = Some DateTimeOffset.UtcNow }

                  let forecastQueries = mockQueries(Map.ofList [ "PUB-001", pub ])

                  // 2. Setup context and subscriber for batch notification
                  let ctx = UnderstandDemandContext.create obsRepo scopeRepo edpRepo forecastQueries (fun _ -> ())

                  let mutable batchNotif = None

                  use _sub =
                      DomainEventBus.Subscribe<ObservationBatchReceivedNotification>(fun ev ->
                          if ev.ObservationIds |> List.exists (fun id -> id.StartsWith "FCAST-PUB-001") then
                              batchNotif <- Some ev)

                  let notification: ForecastPublishedNotification =
                      { PublicationId = "PUB-001"
                        Version = 1
                        PublicationTime = DateTimeOffset.UtcNow }

                  // 3. Run Workflow
                  let! result = ctx.Workflow.OnForecastPublished notification

                  Expect.isOk result "Forecast ingestion should succeed"

                  // 4. Verify batch notification published
                  Expect.isSome batchNotif "ObservationBatchReceivedNotification should be published"
                  let notif = batchNotif.Value
                  Expect.hasLength notif.ObservationIds 1 "Batch should contain one observation"

                  // 5. Verify observation created in repository
                  let obsId = notif.ObservationIds[0]
                  let! obsOptRes = obsRepo.Get obsId
 
                  match obsOptRes with
                  | Error e -> failwith $"Failed to get observation from repository: {e.ToString()}"
                  | Ok obsOpt ->
                      Expect.isSome obsOpt "Ingested observation should exist in repository"
                      let obs = obsOpt.Value
                      Expect.equal (SkuId.value obs.SkuId) "SKU-001" "SKU should match"
                      Expect.equal (StockingPointId.value obs.StockingPointId) "SP-001" "Stocking point should match"
                      Expect.equal (Quantity.value obs.Quantity) 120m "Quantity should match forecast Mean"

                      Expect.equal obs.ObservationType ObservationType.Signal "ObservationType should be Signal"
              } ]
