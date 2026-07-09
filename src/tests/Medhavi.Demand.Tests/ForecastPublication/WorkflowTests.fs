module Medhavi.Demand.Tests.ForecastPublication.WorkflowTests

open Expecto
open Medhavi.SharedKernel
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastPublication
open Medhavi.Demand.Tests.Builders.ForecastPub

let compService = ComputationService.create(fun _ _ -> task { return [ 100m ] })

[<Tests>]
let tests =
    testList
        "ForecastPublication Workflow Integration"
        [ testCaseTask "FS-D-007 success publishes BN-D-010 Cycle Initialised"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()
                  let ctx = Context.create repo compService (fun _ -> ())
                  let mutable notified = None

                  use _sub =
                      DomainEventBus.Subscribe<ForecastCycleInitialisedNotification>(fun ev -> notified <- Some ev)

                  let req = defaultInitiateReq
                  let! res = ctx.Commands.InitiateCycle req

                  match res with
                  | Error e -> failwithf "InitiateCycle failed: %A" e
                  | Ok pubId ->
                      Expect.equal pubId "PUB-001" "Should return publication ID"
                      Expect.isSome notified "Should publish ForecastCycleInitialisedNotification"
                      let notif = notified.Value
                      Expect.equal notif.PublicationId "PUB-001" "ID should match"
                      Expect.equal notif.PlanningScopeIds [ "SCOPE-001" ] "PlanningScopeIds should match"
              }

          testCaseTask "FS-D-010 success publishes BN-D-012 Override Recorded"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let ctx = Context.create repo compService (fun _ -> ())

                  // Pre-seed a draft publication containing a forecast
                  let pubWithForecast =
                      { defaultPub with
                          Forecasts = Map.ofList [ ForecastId.value defaultForecast.ForecastId, defaultForecast ] }

                  let! _ = repo.Save("PUB-001", pubWithForecast, [])

                  let mutable notified = None

                  use _sub =
                      DomainEventBus.Subscribe<ForecastOverrideRecordedNotification>(fun ev -> notified <- Some ev)

                  let req: RecordForecastOverrideReq =
                      { PublicationId = "PUB-001"
                        ForecastId = ForecastId.value defaultForecast.ForecastId
                        NewValue = 130m
                        Justification = "customer request"
                        PlannerIdentity = "PL-01" }

                  let! res = ctx.Commands.RecordOverride req

                  match res with
                  | Error e -> failwithf "RecordOverride failed: %A" e
                  | Ok pubId ->
                      Expect.equal pubId "PUB-001" "Should return publication ID"
                      Expect.isSome notified "Should publish ForecastOverrideRecordedNotification"
                      let notif = notified.Value
                      Expect.equal notif.PublicationId "PUB-001" "PublicationId should match"

                      Expect.equal
                          notif.ForecastId
                          (ForecastId.value defaultForecast.ForecastId)
                          "ForecastId should match"

                      Expect.equal notif.OverrideValue 130m "OverrideValue should match"
                      Expect.equal notif.PlannerIdentity "PL-01" "PlannerIdentity should match"
              }

          testCaseTask "FS-D-011 success publishes BN-D-011 Forecast Published"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()
                  let ctx = Context.create repo compService (fun _ -> ())

                  // Pre-seed a draft publication
                  let! _ = repo.Save("PUB-001", defaultPub, [])

                  let mutable notified = None
                  use _sub = DomainEventBus.Subscribe<ForecastPublishedNotification>(fun ev -> notified <- Some ev)

                  let req: PublishForecastPublicationReq = { PublicationId = "PUB-001" }
                  let! res = ctx.Commands.Publish req

                  match res with
                  | Error e -> failwithf "Publish failed: %A" e
                  | Ok pubId ->
                      Expect.equal pubId "PUB-001" "Should return publication ID"
                      Expect.isSome notified "Should publish ForecastPublishedNotification"
                      let notif = notified.Value
                      Expect.equal notif.PublicationId "PUB-001" "PublicationId should match"
                      Expect.equal notif.Version 1 "Version should be 1"
              }

          testCaseTask "FS-D-011 failure publishes BN-D-013 Forecast Publication Failed"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()
                  let ctx = Context.create repo compService (fun _ -> ())

                  // Pre-seed a publication that is ALREADY published (so publishing again fails BR-D-029)
                  let publishedPub = { defaultPub with Status = Published }
                  let! _ = repo.Save("PUB-001", publishedPub, [])

                  let mutable notified = None

                  use _sub =
                      DomainEventBus.Subscribe<ForecastPublicationFailedNotification>(fun ev -> notified <- Some ev)

                  let req: PublishForecastPublicationReq = { PublicationId = "PUB-001" }
                  let! res = ctx.Commands.Publish req

                  // Workflow will run and return Error(ApplicationError.Domain(...))
                  Expect.isError res "Publishing already published should fail"
                  Expect.isSome notified "Should publish ForecastPublicationFailedNotification on failure"
                  let notif = notified.Value
                  Expect.equal notif.PublicationId "PUB-001" "PublicationId should match"

                  Expect.stringContains
                      notif.Reason
                      "Cannot modify a published Forecast Publication"
                      "Reason should indicate rule violation"
              } ]
