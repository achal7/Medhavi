module Medhavi.Demand.Tests.Integration.PlanningClassificationAssignmentPipelineTests

open Expecto
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.PlanningClassificationAssignment.Projection
open Medhavi.Contracts.Demand.PlanningClassificationAssignment
open Medhavi.Demand.PlanningClassificationAssignment

let private getKey (entityType, entityId, classificationType) = $"{entityType}-{entityId}-{classificationType}"

[<Tests>]
let tests =
    testList
        "PlanningClassificationAssignment Pipeline"
        [ testCaseTask "UpdateClassification: command → projection"
          <| fun () ->
              task {
                  let repo =
                      createInMemoryRepository<
                          Model.PlanningClassificationAssignment,
                          string,
                          PlanningClassificationEvent
                       >()

                  let ctx = Context.create repo (fun _ -> ())
                  let caps = ctx.Commands

                  let req =
                      { EntityType = "Product"
                        EntityId = "SKU-001"
                        ClassificationType = "ABC" }

                  let! res = caps.UpdateClassification req

                  match res with
                  | Ok id ->
                      let! stateRes = repo.Get id
                      match stateRes with
                      | Ok (Some state) ->
                          Expect.equal state.EntityType "Product" "EntityType should be Product"
                          Expect.equal state.EntityId "SKU-001" "EntityId should match"

                          Expect.isTrue
                              ([ "A"; "B"; "C" ] |> List.contains state.CurrentClassification)
                              "Classification should be A, B, or C"

                          let! events = repo.GetEvents id

                          match events with
                          | Ok evts ->
                              let projState = evts |> List.fold evolveProjection Map.empty
                              Expect.isTrue (projState.ContainsKey id) "Projection should contain the assignment"
                              let dto = projState[id]
                              Expect.equal dto.EntityType "Product" "Projection EntityType"
                              Expect.equal dto.EntityId "SKU-001" "Projection EntityId"
                          | _ -> failwith "Could not get events"
                      | Ok None -> failwith "State not found"
                      | Error err -> failwithf "Failed to load state: %A" err
                  | Error err -> failwithf "Command failed: %A" err
              } ]
