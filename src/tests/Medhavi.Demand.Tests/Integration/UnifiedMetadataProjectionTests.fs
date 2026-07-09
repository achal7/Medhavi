module Medhavi.Demand.Tests.Integration.UnifiedMetadataProjectionTests

open System
open Expecto
open Medhavi.SharedKernel
open Medhavi.Contracts.Demand
open Medhavi.Demand.Projections.UnifiedPlanningMetadataProjection
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.Tests.Builders
open System.Threading.Tasks

[<Tests>]
let tests =
    testList
        "Unified Metadata Projection"
        [ testCaseTask "Events from all sources populate unified metadata"
          <| fun () ->
              task {
                  let agent = createProjectionAgent()
                  let queries = new UnifiedMetadataQueries()

                  // --- Classification ---
                  let classAss: PlanningClassificationAssignment =
                      { EntityType = "Product"
                        EntityId = "SKU-001"
                        ClassificationType = ABC
                        CurrentClassification = "A"
                        ClassificationConfidence = positiveDecimal 0.9m
                        LastClassified = Timestamp.now
                        LastChangeEvent = None
                        BusinessTime = Timestamp.now
                        TransactionTime = Timestamp.now }

                  let classChange =
                      { Timestamp = Timestamp.now
                        PreviousClassification = None
                        NewClassification = "A"
                        Reason = "test"
                        OverrideJustification = None
                        ClassificationConfidence = positiveDecimal 0.9m
                        PolicyVersionRef = "v1" }

                  let classEvt = PlanningClassificationUpdated(classAss, classChange)
                  agent.Post(box classEvt, Guid.NewGuid(), None)

                  // --- Behaviour Assignment ---
                  let behavAss: DemandBehaviourAssignment =
                      { EntityType = "Product"
                        EntityId = "SKU-001"
                        BehaviourDimension = "StatisticalPattern"
                        CurrentClassification = "Seasonal"
                        ClassificationConfidence = positiveDecimal 0.85m
                        EvidenceSummary = "test"
                        LastClassified = Timestamp.now
                        LastChangeEvent = None
                        BusinessTime = Timestamp.now
                        TransactionTime = Timestamp.now }

                  let behavChange =
                      { Timestamp = Timestamp.now
                        PreviousClassification = None
                        NewClassification = "Seasonal"
                        Reason = "test"
                        OverrideJustification = None
                        ClassificationConfidence = positiveDecimal 0.85m
                        EvidenceSummary = "test"
                        PolicyVersionRef = "v1" }

                  let behavEvt = DemandBehaviourClassificationUpdated(behavAss, behavChange)
                  agent.Post(box behavEvt, Guid.NewGuid(), None)

                  // --- Priority ---
                  let prioAss: PlanningPriorityAssignment =
                      { EntityType = "Product"
                        EntityId = "SKU-001"
                        CurrentPriority = High
                        PriorityScore = (PositiveDecimal.createSafe 85m)
                        DecisionRationale = "test"
                        BusinessValidity = "valid"
                        LastEvaluated = Timestamp.now
                        LastChangeEvent = None
                        BusinessTime = Timestamp.now
                        TransactionTime = Timestamp.now }

                  let prioChange =
                      { Timestamp = Timestamp.now
                        PreviousPriority = None
                        NewPriority = "High"
                        PreviousScore = None
                        NewScore = (PositiveDecimal.createSafe 85m)
                        DecisionRationale = "test"
                        BusinessValidity = "valid"
                        Reason = "test"
                        OverrideJustification = None
                        PolicyVersionRef = "v1" }

                  let prioEvt = PlanningPriorityUpdated(prioAss, prioChange)
                  agent.Post(box prioEvt, Guid.NewGuid(), None)

                  // --- Behaviour Assessment ---
                  let assessState: DemandBehaviourAssessment =
                      { SkuId = skuId "SKU-001"
                        StockingPointId = stockingPointId "SP-001"
                        CurrentState = Elevated
                        LastUpdated = Timestamp.now
                        CurrentDeviation = Some(positiveDecimal 3.0m)
                        Confidence = positiveDecimal 0.8m
                        CorroboratingSignalCount = 2
                        BaselineReference = "baseline"
                        ActiveSources = []
                        LastSignalTime = None
                        LastStateChange = None
                        BusinessTime = Timestamp.now
                        TransactionTime = Timestamp.now }

                  let assessChange =
                      { EventId = "evt"
                        Timestamp = Timestamp.now
                        PreviousState = Normal
                        NewState = Elevated
                        DeviationMagnitude = positiveDecimal 3.0m
                        DeviationDirection = Increase
                        ConfidenceScore = positiveDecimal 0.8m
                        CorroboratingSignalCount = 2
                        BaselineReference = "baseline"
                        TriggeringSignalId = None }

                  let assessEvt = BehaviourStateChanged(assessState, assessChange)
                  agent.Post(box assessEvt, Guid.NewGuid(), None)

                  // Let the agent process
                  do! Task.Delay 200

                  let! metadata = queries.GetSkuMetadata ("SKU-001", "SP-001") agent
                  Expect.isSome metadata "Unified metadata should exist for SKU-001/SP-001"
                  let m = metadata.Value
                  Expect.equal m.AbcClass (Some "A") "ABC class should be A"
                  Expect.equal m.BehaviourPattern (Some "Seasonal") "Behaviour pattern should be Seasonal"
                  Expect.equal m.Priority (Some "High") "Priority should be High"
                  Expect.equal m.PriorityScore (Some 85m) "Priority score should match"
                  Expect.equal m.DemandBehaviourState (Some "Elevated") "Behaviour state should be Elevated"
              } ]
