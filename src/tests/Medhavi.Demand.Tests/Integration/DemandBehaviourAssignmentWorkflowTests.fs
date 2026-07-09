module Medhavi.Demand.Tests.Integration.DemandBehaviourAssignmentPipelineTests

open Expecto
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Projection
open Medhavi.Demand.DemandBehaviourAssignment
open Medhavi.Contracts.Demand.DemandBehaviourAssignment

let private getKey (entityType, entityId, dimension) = $"{entityType}-{entityId}-{dimension}"

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssignment Pipeline"
        [ testCaseTask "UpdateBehaviour: command → projection"
          <| fun () ->
              task {
                  let repo =
                      createInMemoryRepository<Model.DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let ctx = Context.create repo (fun _ -> ())
                  let caps = ctx.Commands

                  let req =
                      { EntityType = "Product"
                        EntityId = "SKU-001"
                        BehaviourDimension = "StatisticalPattern" }

                  let! res = caps.UpdateBehaviour req

                  match res with
                  | Ok id ->
                      let! stateRes = repo.Get id
                      match stateRes with
                      | Ok (Some state) ->
                          Expect.equal state.EntityType "Product" "EntityType should be Product"
                          Expect.equal state.EntityId "SKU-001" "EntityId should match"
                          Expect.isNonEmpty state.CurrentClassification "Classification should not be empty"

                          let! events = repo.GetEvents id

                          match events with
                          | Ok evts ->
                              let projState = evts |> List.fold evolveProjection Map.empty
                              Expect.isTrue (projState.ContainsKey id) "Projection should contain the assignment"
                              let dto = projState[id]
                              Expect.equal dto.EntityType "Product" "Projection EntityType"
                          | _ -> failwith "Could not get events"
                      | Ok None -> failwith "State not found"
                      | Error err -> failwithf "Failed to load state: %A" err
                  | Error err -> failwithf "Command failed: %A" err
              } ]
