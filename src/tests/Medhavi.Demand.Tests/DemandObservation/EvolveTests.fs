module Medhavi.Demand.Tests.DemandObservation.EvolveTests

open System
open Expecto
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.Demand.DemandObservation
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.Tests.Builders

let defaultReceived: DemandObservation =
    { Id = obsId "OBS-001"
      SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      Quantity = quantity 100m
      ObservationType = ObservationType.SalesOrder
      BusinessTime = timestamp(DateTimeOffset.UtcNow.AddHours(-1.0))
      CustomerId = None
      PromotionRef = None
      CampaignRef = None
      ContractRef = None
      PlanningScopeId = None
      Status = Received
      Decision = None
      Provenance =
        { SourceSystem = "ERP"
          ExternalRef = "ORD-001"
          MessageId = "msg-001"
          Revision = Revision 1
          ScenarioId = None } }

let withStatus status (obs: Model.DemandObservation) = { obs with Status = status }
let withId id (obs: Model.DemandObservation) = { obs with Id = obsId id }
let withQuantity q (obs: Model.DemandObservation) = { obs with Quantity = quantity q }

let withScope (scope: string) (obs: Model.DemandObservation) =
    { obs with
        PlanningScopeId = Some(scopeId scope) }

[<Tests>]
let tests =
    testList
        "DemandObservation Evolve"
        [

          testCase "ObservationEstablished creates state with Received status"
          <| fun _ ->
              let obs = defaultReceived
              let event = ObservationEstablished obs
              let result = evolve event None
              Expect.isSome result "State should be created"
              Expect.equal result.Value.Status Received "Status should be Received"
              Expect.equal result.Value.Id obs.Id "Id should match"

          testCase "ObservationAccepted transitions to Accepted and records decision"
          <| fun _ ->
              let state = defaultReceived

              let decision =
                  { DecisionId = "DE-D-010"
                    Timestamp = Timestamp.now
                    Confidence = 0.95M
                    Rationale = "ok"
                    WarningCode = None }

              let event = ObservationAccepted(state.Id, decision)
              let result = evolve event (Some state)
              Expect.isSome result "State should exist"
              Expect.equal result.Value.Status Accepted "Status should be Accepted"
              Expect.equal result.Value.Decision (Some decision) "Decision should be recorded"

          testCase "ObservationQuarantined transitions to Quarantined"
          <| fun _ ->
              let state = defaultReceived

              let decision =
                  { DecisionId = "DE-D-010"
                    Timestamp = Timestamp.now
                    Confidence = 0.3M
                    Rationale = "unreliable"
                    WarningCode = None }

              let event = ObservationQuarantined(state.Id, decision)
              let result = evolve event (Some state)
              Expect.equal result.Value.Status Quarantined "Status should be Quarantined"

          testCase "ObservationRejected transitions to Rejected"
          <| fun _ ->
              let state = defaultReceived

              let decision =
                  { DecisionId = "DE-D-010"
                    Timestamp = Timestamp.now
                    Confidence = 0.1M
                    Rationale = "bad"
                    WarningCode = None }

              let event = ObservationRejected(state.Id, decision)
              let result = evolve event (Some state)
              Expect.equal result.Value.Status Rejected "Status should be Rejected"

          testCase "ObservationWarningRecorded stays Accepted with warning code"
          <| fun _ ->
              let state = defaultReceived

              let decision =
                  { DecisionId = "DE-D-010"
                    Timestamp = Timestamp.now
                    Confidence = 0.9M
                    Rationale = "ok"
                    WarningCode = None }

              let event = ObservationWarningRecorded(state.Id, "WARN-01", decision)
              let result = evolve event (Some state)
              Expect.equal result.Value.Status Accepted "Status should be Accepted"
              Expect.equal result.Value.Decision.Value.WarningCode (Some "WARN-01") "Warning code should be recorded"

          testCase "ObservationScopeAssigned sets PlanningScopeId"
          <| fun _ ->
              let state = defaultReceived |> withStatus Accepted
              let scope = scopeId "SCOPE-001"
              let event = ObservationScopeAssigned(state.Id, scope)
              let result = evolve event (Some state)
              Expect.equal result.Value.PlanningScopeId (Some scope) "PlanningScopeId should be set"

          testCase "evolve returns None when state is None for non-Establish event"
          <| fun _ ->
              let event =
                  ObservationAccepted(
                      obsId "OBS-001",
                      { DecisionId = "DE-D-010"
                        Timestamp = Timestamp.now
                        Confidence = 0.9M
                        Rationale = ""
                        WarningCode = None }
                  )

              let result = evolve event None
              Expect.isNone result "Should remain None for non-Establish on missing state" ]
