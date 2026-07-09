module Medhavi.Demand.Tests.PlanningScope.WorkflowTests

open Expecto
open Medhavi.Demand
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.PlanningScope.Decisions
open Medhavi.Demand.PlanningScope.Projection
open Medhavi.Demand.Tests.Helpers
open Medhavi.Demand.Tests.PlanningScope.DecisionsTests

[<Tests>]
let tests =
    testList
        "PlanningScope Pipeline"
        [ testCaseTask "FS-D-003: Determine scope → projection"
          <| fun () ->
              task {
                  let repo =
                      Medhavi.SharedKernel.InMemRepository.createInMemoryRepository<
                          PlanningScope,
                          string,
                          PlanningScopeEvent
                       >()

                  let cmd = PlanningScopeCommand.Determine defaultDetermineCmd

                  let! result =
                      runCommand
                          repo
                          decide
                          (fun c ->
                              match c with
                              | PlanningScopeCommand.Determine d -> PlanningScopeId.value d.ScopeId
                              | PlanningScopeCommand.Archive a -> PlanningScopeId.value a.ScopeId)
                          None
                          cmd

                  match result with
                  | Error e -> failwithf "Determine failed: %A" e
                  | Ok(state, events) ->
                      Expect.equal state.Status Active "Aggregate should be Active"
                      let projState = events |> List.fold evolveProjection Map.empty
                      let id = PlanningScopeId.value state.Id
                      Expect.isTrue (projState.ContainsKey id) "Projection should contain scope"
                      Expect.equal projState[id].Status "Active" "Projection status should be Active"
              } ]
