module Medhavi.Demand.Tests.Builders

open System
open Medhavi.SharedKernel
open Medhavi.Demand
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.ForecastQualityAssessment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.DemandExplanation.Model
open Medhavi.Demand.DemandPlanningCondition.Model
open Medhavi.Demand.DemandLearning.Model
open Medhavi.SharedKernel.Contracts.Aggregate

/// Unwrap a Result in test code – fails the test if Error.
let inline okOrFail (result: Result<'T, _>) =
    match result with
    | Ok v -> v
    | Error e -> failwithf "Test data invalid: %A" e

// Convenience constructors
let skuId (s: string) = SkuId.create s |> okOrFail
let stockingPointId (s: string) = StockingPointId.create s |> okOrFail
let quantity (q: decimal) = Quantity.create q |> okOrFail
let obsId (s: string) = DemandObservationId.create s |> okOrFail
let scopeId (s: string) = PlanningScopeId.fromString s |> okOrFail
let timestamp (dt: DateTimeOffset) = Timestamp.create dt
let positiveDecimal (d: decimal) = PositiveDecimal.create d |> okOrFail

let createObsRepo () = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
let createScopeRepo () = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
let createEdpRepo () = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
let createFcastRepo () = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()
let createFqRepo () = createInMemoryRepository<ForecastQualityAssessment, string, ForecastQualityAssessmentEvent>()
let createAssessRepo () = createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

let createClassRepo () =
    createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

let createBehavRepo () = createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()
let createPrioRepo () = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()
let createExplRepo () = createInMemoryRepository<DemandExplanation, string, DemandExplanationEvent>()
let createCondRepo () = createInMemoryRepository<DemandPlanningCondition, string, DemandPlanningConditionEvent>()
let createLearnRepo () = createInMemoryRepository<DemandLearning, string, DemandLearningEvent>()

type TestContext =
    { DemandIntelligenceContext: DemandIntelligenceContext.DemandIntelligenceContext
      DemandObservationRepo: Repository<DemandObservation, string, ObservationEvent>
      PlanningScopeRepo: Repository<PlanningScope, string, PlanningScopeEvent>
      EnterpriseDemandPictureRepo: Repository<EnterpriseDemandPicture, string, EdpEvent>
      ForecastPublicationRepo: Repository<ForecastPublication, string, ForecastPublicationEvent>
      ForecastQualityAssessmentRepo: Repository<ForecastQualityAssessment, string, ForecastQualityAssessmentEvent>
      DemandBehaviourAssessmentRepo: Repository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>
      PlanningClassificationAssignmentRepo:
          Repository<PlanningClassificationAssignment, string, PlanningClassificationEvent>
      DemandBehaviourAssignmentRepo: Repository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>
      PlanningPriorityAssignmentRepo: Repository<PlanningPriorityAssignment, string, PlanningPriorityEvent>
      DemandExplanationRepo: Repository<DemandExplanation, string, DemandExplanationEvent>
      DemandPlanningConditionRepo: Repository<DemandPlanningCondition, string, DemandPlanningConditionEvent>
      DemandLearningRepo: Repository<DemandLearning, string, DemandLearningEvent> }

module TestContext =
    open Medhavi.SharedKernel.Observation

    let createContext
        getAssessments
        isHighPriority
        getScopeId
        compService
        templateVersionRef
        generator
        (publishKnowledge: (ArchitecturalKnowledge -> unit) option)
        =
        let obsRepo = createObsRepo()
        let scopeRepo = createScopeRepo()
        let edpRepo = createEdpRepo()
        let fcastRepo = createFcastRepo()
        let fqRepo = createFqRepo()
        let assessRepo = createAssessRepo()
        let classRepo = createClassRepo()
        let behavRepo = createBehavRepo()
        let prioRepo = createPrioRepo()
        let explRepo = createExplRepo()
        let condRepo = createCondRepo()
        let learnRepo = createLearnRepo()

        let publishKnowledge =
            match publishKnowledge with
            | Some pk -> pk
            | None -> (fun _ -> ())

        let ctx =
            DemandIntelligenceContext.create
                obsRepo
                scopeRepo
                edpRepo
                fcastRepo
                fqRepo
                assessRepo
                classRepo
                behavRepo
                prioRepo
                explRepo
                condRepo
                learnRepo
                getAssessments
                isHighPriority
                getScopeId
                compService
                templateVersionRef
                generator
                publishKnowledge

        { DemandIntelligenceContext = ctx
          DemandObservationRepo = obsRepo
          PlanningScopeRepo = scopeRepo
          EnterpriseDemandPictureRepo = edpRepo
          ForecastPublicationRepo = fcastRepo
          ForecastQualityAssessmentRepo = fqRepo
          DemandBehaviourAssessmentRepo = assessRepo
          PlanningClassificationAssignmentRepo = classRepo
          DemandBehaviourAssignmentRepo = behavRepo
          PlanningPriorityAssignmentRepo = prioRepo
          DemandExplanationRepo = explRepo
          DemandPlanningConditionRepo = condRepo
          DemandLearningRepo = learnRepo }

module DemandSignalBuilder =

    let defaultSignal: DemandSignal =
        { SignalId = "SIG-001"
          Source = "TestSource"
          SourceReliability = 80.0M
          Timestamp = DateTimeOffset.UtcNow.AddMinutes(-30.0)
          Value = 100m
          StatisticalBound = 50m
          RecentBaseline = 90m }

    let withReliability r (s: DemandSignal) = { s with SourceReliability = r }
    let withTimestamp t (s: DemandSignal) = { s with Timestamp = t }
    let withValue v (s: DemandSignal) = { s with Value = v }

module ForecastPub =
    open Medhavi.Demand.ForecastPublication.Model

    let forecastPubId (s: string) = ForecastPublicationId.create s |> okOrFail
    let forecastId (s: string) = ForecastId.create s |> okOrFail

    let defaultPub: ForecastPublication =
        { Id = forecastPubId "PUB-001"
          PlanningScopeIds = [ scopeId "SCOPE-001" ]
          ForecastHorizon = TimeSpan.FromDays(7.0)
          TimeBucketConfig = "Weekly"
          Status = Draft
          Version = 1
          ChampionModelId = None
          OverallConfidenceIndex = None
          Forecasts = Map.empty
          Coverage = []
          Assumptions = Map.empty
          Overrides = Map.empty
          TransactionTime = Timestamp.now
          PublicationTime = None
          SupersededPublicationId = None }

    let defaultForecast: Forecast =
        { ForecastId = forecastId "FC-001"
          SkuId = skuId "SKU-001"
          StockingPointId = stockingPointId "SP-001"
          PlanningPeriod = Medhavi.Contracts.PlanningWeek(2027, 27)
          Mean = 100m
          PredictionInterval =
            { LowerBound = positiveDecimal 80m
              UpperBound = positiveDecimal 120m
              ConfidenceLevel = positiveDecimal 0.95m }
          Confidence = positiveDecimal 0.95m
          ModelId = "Model-A"
          GeneratedAt = Timestamp.now
          OverrideReason = None }

    open Medhavi.Contracts.Demand.ForecastPublication

    let defaultInitiateReq: InitiateForecastCycleReq =
        { PublicationId = "PUB-001"
          PlanningScopeIds = [ "SCOPE-001" ]
          ForecastHorizon = "7.00:00:00"
          TimeBucketConfig = "Weekly" }
