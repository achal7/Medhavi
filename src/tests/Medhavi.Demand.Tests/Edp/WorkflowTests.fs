module Medhavi.Demand.Tests.Edp.WorkflowTests

open Expecto
open Medhavi.Contracts
open Medhavi.Contracts.Demand.Edp
open Medhavi.Demand.Tests.Builders
open Medhavi.Demand.EnterpriseDemandPicture
open Medhavi.Demand.EnterpriseDemandPicture.Projection
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.EnterpriseDemandPicture.Decisions
open Medhavi.SharedKernel.InMemRepository
open Medhavi.SharedKernel
open Builders
open Medhavi.Demand
open Medhavi.Demand.Tests.Helpers

[<Tests>]
let tests =
    testList
        "EnterpriseDemandPicture Pipeline"
        [ testCaseTask "FS-D-004 → FS-D-006: Revise → Calculate → Publish"
          <| fun () ->
              task {
                  let repo =
                      Medhavi.SharedKernel.InMemRepository.createInMemoryRepository<
                          EnterpriseDemandPicture,
                          string,
                          EdpEvent
                       >()

                  let getId (cmd: EdpCommand) =
                      match cmd with
                      | EdpCommand.Revise r -> PlanningScopeId.value r.PlanningScopeId
                      | EdpCommand.Calculate c -> PlanningScopeId.value c.PlanningScopeId
                      | EdpCommand.Publish p -> PlanningScopeId.value p.PlanningScopeId

                  // Revise
                  let! reviseResult = runCommand repo decide getId None (EdpCommand.Revise defaultReviseCmd)

                  let edp1 =
                      match reviseResult with
                      | Ok(s, _) -> s
                      | Error e -> failwithf "Revise failed: %A" e

                  Expect.equal edp1.Version 1 "Version should be 1"

                  // Calculate
                  let calcCmd = EdpCommand.Calculate { PlanningScopeId = edp1.PlanningScopeId; Adjustments = Map.empty; Overrides = Map.empty }
                  let! calcResult = runCommand repo decide getId (Some edp1) calcCmd

                  let edp2 =
                      match calcResult with
                      | Ok(s, _) -> s
                      | Error e -> failwithf "Calculate failed: %A" e

                  Expect.equal edp2.Status ReadyForPublication "Status should be ReadyForPublication"

                  // Publish
                  let pubCmd = EdpCommand.Publish { PlanningScopeId = edp2.PlanningScopeId }
                  let! pubResult = runCommand repo decide getId (Some edp2) pubCmd

                  let edp3 =
                      match pubResult with
                      | Ok(s, _) -> s
                      | Error e -> failwithf "Publish failed: %A" e

                  Expect.equal edp3.Status Published "Status should be Published"

                  // Projection
                  let events =
                      [ match reviseResult with
                        | Ok(_, evts) -> yield! evts
                        | _ -> ()
                        match calcResult with
                        | Ok(_, evts) -> yield! evts
                        | _ -> ()
                        match pubResult with
                        | Ok(_, evts) -> yield! evts
                        | _ -> () ]

                  let projState = events |> List.fold evolveProjection Map.empty
                  let id = PlanningScopeId.value edp3.PlanningScopeId
                  Expect.isTrue (projState.ContainsKey id) "Projection should contain EDP"
                  Expect.equal projState.[id].Status "Published" "Projection status should be Published"
              } ]

[<Tests>]
let workflowTests =
    testList
        "EnterpriseDemandPicture Workflow Integration"
        [ testCaseTask "FS-D-005 failure publishes BN-D-004 Recalculation Failed"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let ctx = Context.create repo (fun _ -> task { return Map.empty }) (fun _ -> task { return Map.empty }) (fun _ -> ())
                  let api = ctx.Commands

                  let mutable notified = None

                  use _sub =
                      DomainEventBus.Subscribe<EnterpriseDemandPictureRecalculationFailedNotification>(fun ev ->
                          notified <- Some ev)

                  // Attempting to calculate for a scope that has not been revised (state is None)
                  let req: CalculateEnterpriseDemandPictureReq = { PlanningScopeId = "SCOPE-999" }
                  let! res = api.Calculate req

                  Expect.isError res "Calculate should fail for non-existent scope"

                  Expect.isSome
                      notified
                      "Should publish EnterpriseDemandPictureRecalculationFailedNotification on failure"

                  let notif = notified.Value
                  Expect.equal notif.PlanningScopeId "SCOPE-999" "PlanningScopeId should match"
                  Expect.stringContains notif.Reason "Cannot calculate" "Reason should describe the missing EDP"
              }

          testCaseTask "FS-D-006 success publishes BN-D-001 EDP Published"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let ctx = Context.create repo (fun _ -> task { return Map.empty }) (fun _ -> task { return Map.empty }) (fun _ -> ())
                  let api = ctx.Commands

                  // Pre-seed an EDP in ReadyForPublication status
                  let readyEdp = defaultEdp |> withStatus ReadyForPublication
                  let! _ = repo.Save("SCOPE-001", readyEdp, [])

                  let mutable notified = None

                  use _sub =
                      DomainEventBus.Subscribe<EnterpriseDemandPicturePublishedNotification>(fun ev ->
                          if ev.PlanningScopeId = "SCOPE-001" then
                              notified <- Some ev)

                  let req: PublishEnterpriseDemandPictureReq = { PlanningScopeId = "SCOPE-001" }
                  let! res = api.Publish req

                  match res with
                  | Error e -> failwithf "Publish failed: %A" e
                  | Ok(scopeId, version) ->
                      Expect.equal scopeId "SCOPE-001" "Should return scope ID"
                      Expect.equal version 1 "Version should be 1"
                      Expect.isSome notified "Should publish EnterpriseDemandPicturePublishedNotification"
                      let notif = notified.Value
                      Expect.equal notif.PlanningScopeId "SCOPE-001" "PlanningScopeId should match"
                      Expect.equal notif.Version 1 "Version should be 1"
              }

          testCaseTask "FS-D-005 calculate incorporates adjustments and overrides"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let getAdjustments _ = task {
                      return Map.ofList [ PlanningPeriod.PlanningWeek(2027, 27), Quantity.create 15.0m |> okOrFail ]
                  }
                  let getOverrides _ = task {
                      return Map.ofList [ PlanningPeriod.PlanningWeek(2027, 27), Quantity.create 25.0m |> okOrFail ]
                  }
                  let ctx = Context.create repo getAdjustments getOverrides (fun _ -> ())
                  let api = ctx.Commands

                  // Pre-seed an EDP in AwaitingPlanningDemandCalculation status with operational demand
                  let edp = { defaultEdp with Status = AwaitingPlanningDemandCalculation; OperationalDemand = Map.ofList [ PlanningPeriod.PlanningWeek(2027, 27), Quantity.create 100.0m |> okOrFail ] }
                  let! _ = repo.Save("SCOPE-001", edp, [])

                  let req: CalculateEnterpriseDemandPictureReq = { PlanningScopeId = "SCOPE-001" }
                  let! res = api.Calculate req
                  Expect.isOk res "Calculate should succeed"

                  let! loadedRes = repo.Get "SCOPE-001"
                  match loadedRes with
                  | Ok (Some updatedEdp) ->
                      let period = PlanningPeriod.PlanningWeek(2027, 27)
                      Expect.isTrue (updatedEdp.PlanningDemand.ContainsKey period) "Should calculate period line"
                      let line = updatedEdp.PlanningDemand.[period]
                      Expect.equal (Quantity.value line.OperationalDemand) 100.0m "Operational demand should match"
                      Expect.equal (Quantity.value line.Adjustment) 15.0m "Adjustment should match"
                      Expect.equal (Quantity.value line.Override) 25.0m "Override should match"
                      Expect.equal (Quantity.value line.FinalQuantity) 140.0m "Final Quantity should be Operational + Adjustment + Override (140.0m)"
                  | _ -> failwith "Failed to load updated EDP"
              } ]
