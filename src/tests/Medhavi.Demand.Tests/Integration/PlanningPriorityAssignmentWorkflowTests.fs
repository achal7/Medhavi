module Medhavi.Demand.Tests.Integration.PlanningPriorityAssignmentPipelineTests

open Expecto
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Projection
open Medhavi.Demand.PlanningPriorityAssignment
open Medhavi.Contracts.Demand.PlanningPriorityAssignment

let private getKey (entityType, entityId) = $"{entityType}-{entityId}"

[<Tests>]
let tests =
    testList
        "PlanningPriorityAssignment Pipeline"
        [ testCaseTask "UpdatePriority: command → projection"
          <| fun () ->
              task {
                  let repo = createInMemoryRepository<Model.PlanningPriorityAssignment, string, PlanningPriorityEvent>()
                  let ctx = Context.create repo (fun _ -> ())
                  let caps = ctx.Commands

                  let req =
                      { EntityType = "Product"
                        EntityId = "SKU-001" }

                  let! res = caps.UpdatePriority req

                  match res with
                  | Ok id ->
                      let! stateRes = repo.Get id
                      match stateRes with
                      | Ok (Some state) ->
                          Expect.equal state.EntityType "Product" "EntityType should be Product"
                          Expect.isNonEmpty (state.CurrentPriority.AsString()) "Priority should not be empty"

                          let! events = repo.GetEvents id

                          match events with
                          | Ok evts ->
                              let projState = evts |> List.fold evolveProjection Map.empty
                              Expect.isTrue (projState.ContainsKey id) "Projection should contain the assignment"
                          | _ -> failwith "Could not get events"
                      | Ok None -> failwith "State not found"
                      | Error err -> failwithf "Failed to load state: %A" err
                  | Error err -> failwithf "Command failed: %A" err
              } ]
