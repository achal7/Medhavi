module Medhavi.Demand.Tests.DemandObservation.WorkflowTests

open Expecto
open Medhavi.Demand.Tests.DemandObservation.EvolveTests
open Medhavi.Demand.Tests.DemandObservation.DecisionsTests
open Medhavi.Demand.Tests.Helpers
open Medhavi.Demand.DemandObservation.Projection
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.DemandObservation.Decisions

[<Tests>]
let tests =
    testList
        "DemandObservation Pipeline"
        [ testCaseTask "FS-D-001: Establish observation → events + projection"
          <| fun () ->
              task {
                  let repo =
                      Medhavi.SharedKernel.InMemRepository.createInMemoryRepository<
                          DemandObservation,
                          string,
                          ObservationEvent
                       >()

                  let cmd = Establish defaultEstablishCmd

                  let! result =
                      runCommand
                          repo
                          decide
                          (fun c ->
                              match c with
                              | Establish e -> DemandObservationId.value e.ObservationId
                              | _ -> failwith "unexpected")
                          None
                          cmd

                  match result with
                  | Error e -> failwithf "Establish failed: %A" e
                  | Ok(state, events) ->
                      Expect.equal state.Status Received "Aggregate should be Received"
                      Expect.hasLength events 1 "Should emit one event"
                      let projState = events |> List.fold evolveProjection Map.empty
                      let id = DemandObservationId.value state.Id
                      Expect.isTrue (projState.ContainsKey id) "Projection should contain observation"

                      Expect.equal
                          projState[id].Status
                          Medhavi.Contracts.Demand.DemandObservation.ObservationStatus.Received
                          "Projection status should be Received"
              }

          testCaseTask "FS-D-002: Evaluate → projection updated"
          <| fun () ->
              task {
                  let repo =
                      Medhavi.SharedKernel.InMemRepository.createInMemoryRepository<
                          DemandObservation,
                          string,
                          ObservationEvent
                       >()

                  let establishCmd = ObservationCommand.Establish defaultEstablishCmd

                  let! establishResult =
                      runCommand
                          repo
                          decide
                          (fun c ->
                              match c with
                              | Establish e -> DemandObservationId.value e.ObservationId
                              | _ -> failwith "")
                          None
                          establishCmd

                  let establishEvents =
                      match establishResult with
                      | Ok(_, evts) -> evts
                      | _ -> []

                  let receivedState =
                      { defaultReceived with
                          Id = defaultEstablishCmd.ObservationId }

                  let evalCmd =
                      ObservationCommand.Evaluate
                          { ObservationId = defaultEstablishCmd.ObservationId
                            Signal = None }

                  let! evalResult =
                      runCommand
                          repo
                          decide
                          (fun c ->
                              match c with
                              | Evaluate e -> DemandObservationId.value e.ObservationId
                              | _ -> failwith "")
                          (Some receivedState)
                          evalCmd

                  match evalResult with
                  | Error e -> failwithf $"Evaluate failed: %A{e}"
                  | Ok(state, evalEvents) ->
                      Expect.equal state.Status Accepted "Aggregate should be Accepted"
                      let allEvents = establishEvents @ evalEvents
                      let projState = allEvents |> List.fold evolveProjection Map.empty
                      let id = DemandObservationId.value state.Id

                      Expect.equal
                          projState[id].Status
                          Medhavi.Contracts.Demand.DemandObservation.ObservationStatus.Accepted
                          "Projection status should be Accepted"
              } ]
