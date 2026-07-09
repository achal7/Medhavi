module Medhavi.Demand.Tests.DemandBehaviourAssessment.EvolveTests

open Expecto
open Medhavi.SharedKernel
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.Tests.Builders

let defaultAssessment: DemandBehaviourAssessment =
    { SkuId = skuId "SKU-001"
      StockingPointId = stockingPointId "SP-001"
      CurrentState = Normal
      LastUpdated = Timestamp.now
      CurrentDeviation = None
      Confidence = PositiveDecimal.Zero
      CorroboratingSignalCount = 0
      BaselineReference = "Baseline-1"
      ActiveSources = []
      LastSignalTime = None
      LastStateChange = None
      BusinessTime = Timestamp.now
      TransactionTime = Timestamp.now }

let someChangeEvent: StateChangeEvent =
    { EventId = "evt-1"
      Timestamp = Timestamp.now
      PreviousState = Normal
      NewState = Elevated
      DeviationMagnitude = PositiveDecimal.createSafe 3.0m
      DeviationDirection = Increase
      ConfidenceScore = PositiveDecimal.createSafe 0.9m
      CorroboratingSignalCount = 2
      BaselineReference = "Baseline-1"
      TriggeringSignalId = Some "sig-1" }

[<Tests>]
let tests =
    testList
        "DemandBehaviourAssessment Evolve"
        [ testCase "BehaviourStateChanged updates state"
          <| fun _ ->
              let updateAssessment = { defaultAssessment with CurrentState = Elevated }
              let evt = BehaviourStateChanged(updateAssessment, someChangeEvent)
              let result = evolve evt None
              Expect.isSome result "State should be created"
              Expect.equal result.Value.CurrentState Elevated "State should be Elevated"

          testCase "BehaviourAssessmentAcknowledged leaves state unchanged"
          <| fun _ ->
              let state = defaultAssessment

              let evt =
                  BehaviourAssessmentAcknowledged(
                      state.SkuId,
                      state.StockingPointId,
                      "planner",
                      "reason",
                      Timestamp.now
                  )

              let result = evolve evt (Some state)
              Expect.equal result.Value.CurrentState Normal "State should remain Normal" ]
