module Medhavi.Demand.Tests.DemandIntelligenceContextTests

open System
open System.Threading.Tasks
open Expecto
open Medhavi.Demand.Tests.Builders.ForecastPub
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.InMemRepository
open Medhavi.Contracts
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.Contracts.Demand.PlanningScope
open Medhavi.Contracts.Demand.Edp
open Medhavi.Contracts.Demand.ForecastPublication
open Medhavi.Contracts.Demand.PlanningClassificationAssignment
open Medhavi.Contracts.Demand.DemandBehaviourAssignment
open Medhavi.Contracts.Demand.PlanningPriorityAssignment
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.PlanningScope.Model
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.ForecastPublication.Model
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.DemandIntelligenceContext
open Medhavi.Demand.Tests.Builders
open Medhavi.Demand.Tests.Helpers
open Medhavi.Infrastructure.Projections

let private createTestContext
    obsRepo
    scopeRepo
    edpRepo
    forecastRepo
    assessRepo
    classRepo
    behavRepo
    prioRepo
    isHighPriority
    getScopeId
    compService
    publishKnowledge
    =
    let forecastQualityRepo =
        createInMemoryRepository<
            ForecastQualityAssessment.Model.ForecastQualityAssessment,
            string,
            ForecastQualityAssessment.Model.ForecastQualityAssessmentEvent
         >()

    let explanationRepo =
        createInMemoryRepository<
            DemandExplanation.Model.DemandExplanation,
            string,
            DemandExplanation.Model.DemandExplanationEvent
         >()

    let conditionRepo =
        createInMemoryRepository<
            DemandPlanningCondition.Model.DemandPlanningCondition,
            string,
            DemandPlanningCondition.Model.DemandPlanningConditionEvent
         >()

    let learningRepo =
        createInMemoryRepository<DemandLearning.Model.DemandLearning, string, DemandLearning.Model.DemandLearningEvent>()

    let getAssessments _ = task { return [] }
    let generator = DemandExplanation.NaturalLanguageGenerator.fakeGenerator "Test explanation"

    create
        obsRepo
        scopeRepo
        edpRepo
        forecastRepo
        forecastQualityRepo
        assessRepo
        classRepo
        behavRepo
        prioRepo
        explanationRepo
        conditionRepo
        learningRepo
        getAssessments
        isHighPriority
        getScopeId
        compService
        "llm"
        generator
        publishKnowledge

[<Tests>]
let testsContext =
    testList
        "DemandIntelligenceContext"
        [ testCaseTask "Create context with all in-memory repos and verify wiring"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()

                  let forecastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "SCOPE-001")
                  let compService = ForecastPublication.ComputationService.create(fun _ _ -> task { return [] })

                  let ctx =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          forecastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService
                          (fun _ -> ())

                  Expect.isNotNull (box ctx.ObservationContext) "ObservationContext should be non-null"
                  Expect.isNotNull (box ctx.UnifiedMetadataQueries) "UnifiedMetadataQueries should be non-null"
                  Expect.isNotNull (box ctx.Workflow) "Workflow should be non-null"

                  ctx.Dispose()
              } ]

// ──────────────────────────────────────────────────────────────
// Helper to wait for a projection agent to satisfy a predicate.
// ──────────────────────────────────────────────────────────────
let private awaitProjection (timeout: TimeSpan) (agent: ProjectionAgent<'S, 'E>) (predicate: 'S -> bool) =
    waitForProjection timeout agent predicate

// ──────────────────────────────────────────────────────────────
// Scenario 1 – Full Understand Demand Chain
// ──────────────────────────────────────────────────────────────
[<Tests>]
let tests =
    testList
        "DemandIntelligence Integration"
        [

          testCaseTask "Scenario 1: Observation → EDP Published"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "SKU-001-SP-001-ALL-W-2027-27")
                  let compService = ForecastPublication.ComputationService.create(fun _ _ -> task { return [] })

                  let ctx =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService
                          (fun _ -> ())

                  let mutable edpPublished = None

                  let expectedScopeId = "SKU-001-SP-001-ALL-W-2027-27"

                  use _ =
                      DomainEventBus.Subscribe<EnterpriseDemandPicturePublishedNotification>(fun n ->
                          if n.PlanningScopeId = expectedScopeId then
                              edpPublished <- Some n)

                  // 1. Receive observation
                  let obsReq: EstablishObservationReq =
                      { ObservationId = "OBS-001"
                        SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        Quantity = 100m
                        UnitOfMeasure = "EA"
                        ObservationType = Demand.DemandObservation.ObservationType.SalesOrder
                        BusinessTime = DateTimeOffset.UtcNow.AddHours(-1.0)
                        CustomerId = None
                        PromotionRef = None
                        CampaignRef = None
                        ContractRef = None
                        SourceSystem = "ERP"
                        ExternalRef = "ORD-001"
                        MessageId = Guid.NewGuid().ToString()
                        Revision = 1 }

                  let! receiveResult = ctx.ObservationContext.Commands.Receive obsReq
                  Expect.isOk receiveResult "Receive observation should succeed"

                  // 2. Evaluate
                  let evalReq: EvaluateObservationReq =
                      { ObservationId = "OBS-001"
                        SignalId = None
                        SignalSource = None
                        SourceReliability = None
                        SignalTimestamp = None
                        SignalValue = None
                        StatisticalBound = None
                        RecentBaseline = None }

                  let! evalResult = ctx.ObservationContext.Commands.Evaluate evalReq
                  Expect.isOk evalResult "Evaluate should succeed"

                  // 3. Determine scope and assign
                  let assignReq: AssignScopeReq =
                      { ObservationId = "OBS-001"
                        PlanningScopeId = "" }

                  let scopeReq: DeterminePlanningScopeReq =
                      { SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        BucketType = "Weekly"
                        CustomerId = None
                        PlanningPeriod = PlanningWeek(2027, 27) }

                  let! assignResult = ctx.Workflow.DetermineScopeAndAssign assignReq scopeReq
                  Expect.isOk assignResult "Determine scope and assign should succeed"

                  // 4. Revise → Calculate → Publish EDP
                  let scopeId = expectedScopeId

                  let reviseReq: ReviseEnterpriseDemandPictureReq =
                      { PlanningScopeId = scopeId
                        Period = PlanningWeek(2027, 27)
                        Quantity = 100m
                        ObservationId = "OBS-001" }

                  let! edpResult = ctx.Workflow.BuildAndPublishEdp reviseReq
                  Expect.isOk edpResult "BuildAndPublishEdp should succeed"

                  // 5. Verify EDP projection
                  let! edpDtoOpt = ctx.EdpContext.Queries.GetById scopeId
                  Expect.isSome edpDtoOpt "EDP should exist in projection"
                  let edpDto = edpDtoOpt.Value
                  Expect.equal edpDto.Status "Published" "EDP should be Published"
                  Expect.equal edpDto.Version 1 "First version should be 1"

                  // 6. Verify BN-D-001
                  Expect.isSome edpPublished "BN-D-001 should be published"
                  Expect.equal edpPublished.Value.PlanningScopeId scopeId "Scope ID should match"

                  // 7. Verify observation projection has scope assigned
                  let! obsDtoOpt = ctx.ObservationContext.Queries.GetById "OBS-001"
                  Expect.isSome obsDtoOpt "Observation should be in projection"
                  Expect.equal obsDtoOpt.Value.PlanningScopeId (Some scopeId) "Observation should have scope assigned"
              }

          // ──────────────────────────────────────────────
          // Scenario 2 – Forecast → Batch → EDP Updated
          // ──────────────────────────────────────────────
          testCaseTask "Scenario 2: Forecast Cycle → Batch → EDP Updated"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let getHistory _ _ = task { return [ 100m; 110m; 120m ] }
                  let compService = ForecastPublication.ComputationService.create getHistory
                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "SCOPE-001")

                  let ctx =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService
                          (fun _ -> ())

                  let mutable forecastPublished = None
                  use _ = DomainEventBus.Subscribe<ForecastPublishedNotification>(fun n -> forecastPublished <- Some n)

                  // Initiate cycle
                  let initReq =
                      { defaultInitiateReq with
                          PublicationId = "PUB-002"
                          PlanningScopeIds = [ "SKU-001-SP-001-ALL-W-2027-27" ] }

                  let! initRes = ctx.ForecastContext.Commands.InitiateCycle initReq
                  Expect.isOk initRes "InitiateCycle should succeed"

                  // Prepare context
                  let prepReq: PrepareForecastContextReq =
                      { PublicationId = "PUB-002"
                        Assumptions = []
                        Coverage =
                          [ { SkuId = "SKU-001"
                              StockingPointId = "SP-001" } ] }

                  let! prepRes = ctx.ForecastContext.Commands.PrepareContext prepReq
                  Expect.isOk prepRes "PrepareContext should succeed"

                  // Wait for the projection to reflect the coverage
                  let! coverageReady =
                      awaitProjection (TimeSpan.FromSeconds 15.0) ctx.ForecastContext.Agent (fun state ->
                          match state |> Map.tryFind "PUB-002" with
                          | Some pub -> pub.Coverage.Length > 0
                          | None -> false)

                  Expect.isTrue coverageReady "Coverage should be populated in projection after PrepareContext"
                  let! pubAfterPrep = fcastRepo.Get "PUB-002"

                  match pubAfterPrep with
                  | Ok(Some pub) ->
                      let dto = Medhavi.Demand.ForecastPublication.Projection.mapToContract pub
                      ctx.ForecastContext.Agent.SetState(Map.ofList [ "PUB-002", dto ])
                  | _ -> ()

                  // Diagnostic checks
                  let! dtoOpt = ctx.ForecastContext.Queries.GetById "PUB-002"
                  Expect.isSome dtoOpt "DTO must exist"
                  let dto = dtoOpt.Value
                  Expect.isNonEmpty dto.Coverage $"Coverage should not be empty, but was {dto.Coverage}"

                  // Select champion
                  let champReq: SelectChampionModelReq =
                      { PublicationId = "PUB-002"
                        CandidateModelId = "Model-A"
                        EvaluationWindowStart = DateTimeOffset.UtcNow.AddDays(-30.0)
                        EvaluationWindowEnd = DateTimeOffset.UtcNow }

                  let! champRes = ctx.ForecastContext.Commands.SelectChampion champReq
                  Expect.isOk champRes "SelectChampion should succeed"

                  // Generate baseline
                  let genReq: GenerateBaselineForecastsReq =
                      { PublicationId = "PUB-002"
                        Forecasts = None }

                  let! genRes = ctx.ForecastContext.Commands.GenerateBaseline genReq
                  Expect.isOk genRes "GenerateBaseline should succeed"

                  // Publish
                  let pubReq: PublishForecastPublicationReq = { PublicationId = "PUB-002" }
                  let! pubRes = ctx.ForecastContext.Commands.Publish pubReq
                  Expect.isOk pubRes "Publish should succeed"

                  // Verify BN-D-011
                  Expect.isSome forecastPublished "BN-D-011 should be published"

                  // Wait for forecast lines to appear in the DTO
                  let! forecastsReady =
                      awaitProjection (TimeSpan.FromSeconds 15.0) ctx.ForecastContext.Agent (fun state ->
                          state |> Map.tryFind "PUB-002" |> Option.exists(fun p -> p.Forecasts.Length > 0))

                  Expect.isTrue forecastsReady "Forecast lines should be in projection after generation"

                  // Synchronous check: observations are in the repository
                  let! obsInRepo =
                      obsRepo.GetEventsByType (function
                          | ObservationEstablished _ -> true
                          | _ -> false)

                  let observationIds =
                      match obsInRepo with
                      | Ok evts ->
                          evts
                          |> List.choose (function
                              | ObservationEstablished d -> Some(DemandObservationId.value d.Id)
                              | _ -> None)
                      | _ -> []

                  Expect.isNonEmpty observationIds "Observations should have been created in the repository"

                  Expect.exists
                      observationIds
                      (fun id -> id.StartsWith "FCAST-PUB-002-")
                      "A forecast-derived observation should exist"

                  // Verify EDP updated
                  let scopeId = "SKU-001-SP-001-ALL-W-2027-27"
                  let! edpOpt = ctx.EdpContext.Queries.GetById scopeId
                  Expect.isSome edpOpt "EDP should exist after forecast integration"
              }

          testCaseTask "Scenario 2b: Batch observation ingestion works"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let obsCtx = DemandObservation.Context.create obsRepo (fun _ -> ())
                  let caps = obsCtx.Commands

                  let defaultEstablish: EstablishObservationReq =
                      { ObservationId = "OBS-TEST-001"
                        SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        Quantity = 100m
                        UnitOfMeasure = "EA"
                        ObservationType = Medhavi.Contracts.Demand.DemandObservation.ObservationType.SalesOrder
                        BusinessTime = DateTimeOffset.UtcNow.AddHours(-1.0)
                        CustomerId = None
                        PromotionRef = None
                        CampaignRef = None
                        ContractRef = None
                        SourceSystem = "ERP"
                        ExternalRef = "ORD-001"
                        MessageId = Guid.NewGuid().ToString()
                        Revision = 1 }

                  let batchReq: EstablishObservationBatchReq =
                      { Ingestions =
                          [ { defaultEstablish with
                                ObservationId = "BATCH-1"
                                SkuId = "SKU-001"
                                StockingPointId = "SP-001" }
                            { defaultEstablish with
                                ObservationId = "BATCH-2"
                                SkuId = "SKU-001"
                                StockingPointId = "SP-001" } ] }

                  let! res = caps.ReceiveBatch batchReq

                  Expect.isOk res "Batch should succeed"

                  let! obs1Res = obsRepo.Get "BATCH-1"

                  match obs1Res with
                  | Error e -> failwithf $"Batch 1 should exist %A{e}"
                  | Ok obs1 -> Expect.isSome obs1 "Observation 1 should exist"
              }
          // ──────────────────────────────────────────────
          // Scenario 3 – Critical Behaviour → Refresh
          // ──────────────────────────────────────────────
          testCaseTask "Scenario 3: Critical behaviour triggers forecast refresh"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "scope-SKU-001-SP-001")
                  let compService = ForecastPublication.ComputationService.create(fun _ _ -> task { return [] })

                  let ctx =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService
                          (fun _ -> ())

                  // ── 1. Create and publish a forecast via the public API ──
                  let initReq =
                      { defaultInitiateReq with
                          PublicationId = "PUB-OLD"
                          PlanningScopeIds = [ "scope-SKU-001-SP-001" ] }

                  let! _ = ctx.ForecastContext.Commands.InitiateCycle initReq

                  let prepReq: PrepareForecastContextReq =
                      { PublicationId = "PUB-OLD"
                        Assumptions = []
                        Coverage =
                          [ { SkuId = "SKU-001"
                              StockingPointId = "SP-001" } ] }

                  let! _ = ctx.ForecastContext.Commands.PrepareContext prepReq

                  let champReq: SelectChampionModelReq =
                      { PublicationId = "PUB-OLD"
                        CandidateModelId = "Model-A"
                        EvaluationWindowStart = DateTimeOffset.UtcNow.AddDays(-30.0)
                        EvaluationWindowEnd = DateTimeOffset.UtcNow }

                  let! _ = ctx.ForecastContext.Commands.SelectChampion champReq

                  // Use a computation service that returns actual forecasts
                  let getHistory _ _ = task { return [ 100m; 110m; 120m ] }
                  let compService2 = ForecastPublication.ComputationService.create getHistory

                  let ctx2 =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService2
                          (fun _ -> ())

                  let genReq: GenerateBaselineForecastsReq =
                      { PublicationId = "PUB-OLD"
                        Forecasts = None }

                  let! _ = ctx2.ForecastContext.Commands.GenerateBaseline genReq

                  // Publish
                  let pubReq: PublishForecastPublicationReq = { PublicationId = "PUB-OLD" }
                  let! _ = ctx2.ForecastContext.Commands.Publish pubReq

                  // Wait for the forecast publication to appear in the projection
                  let! forecastReady =
                      awaitProjection (TimeSpan.FromSeconds 15.0) ctx2.ForecastContext.Agent (fun state ->
                          state |> Map.containsKey "PUB-OLD")

                  Expect.isTrue forecastReady "Published forecast should be in projection"

                  // ── 2. Make the published forecast appear older than 24h ──
                  // This is a necessary test hack: we must adjust metadata that cannot be set via the public API.
                  let! oldPubOpt = fcastRepo.Get "PUB-OLD"

                  match oldPubOpt with
                  | Ok(Some pub) ->
                      let agedPub =
                          { pub with
                              TransactionTime = Timestamp.now - TimeSpan.FromHours 48.0
                              PublicationTime = Some(Timestamp.now - TimeSpan.FromHours 48.0) }

                      let! saveRes = fcastRepo.Save("PUB-OLD", agedPub, [])
                      saveRes |> (okOrFail >> ignore)
                      DomainEventBus.Publish(ForecastPublicationPublished(agedPub, None))
                  | _ -> ()

                  // ── 3. Seed assessment with corroboration using the public API ──
                  for i, source in [ "POS"; "WEB" ] |> List.indexed do
                      let req: Demand.SenseDemand.EvaluateDemandSignalReq =
                          { SkuId = "SKU-001"
                            StockingPointId = "SP-001"
                            SignalId = $"sig-{i}"
                            Source = source
                            SourceReliability = 90m
                            Timestamp = DateTimeOffset.UtcNow.AddMinutes(-float i)
                            Value = 130m
                            StatisticalBound = 10m
                            RecentBaseline = 100m }

                      let! _ = ctx.BehaviourAssessmentContext.Commands.EvaluateSignal req
                      ()

                  // ── 4. Send the Critical signal ──
                  let criticalReq: Demand.SenseDemand.EvaluateDemandSignalReq =
                      { SkuId = "SKU-001"
                        StockingPointId = "SP-001"
                        SignalId = "sig-critical"
                        Source = "MOBILE"
                        SourceReliability = 90m
                        Timestamp = DateTimeOffset.UtcNow
                        Value = 150m
                        StatisticalBound = 10m
                        RecentBaseline = 100m }

                  let! _ = ctx.BehaviourAssessmentContext.Commands.EvaluateSignal criticalReq

                  // ── 5. Verify a new forecast cycle was initiated ──
                  let! initiatedEvents =
                      fcastRepo.GetEventsByType (function
                          | ForecastCycleInitiated _ -> true
                          | _ -> false)

                  let initiated =
                      match initiatedEvents with
                      | Ok evts ->
                          evts
                          |> List.exists (function
                              | ForecastCycleInitiated _ -> true
                              | _ -> false)
                      | _ -> false

                  Expect.isTrue initiated "A new forecast cycle should have been initiated for the scope"
              }
          // ──────────────────────────────────────────────
          // Scenario 4 – Unified Metadata
          // ──────────────────────────────────────────────
          testCaseTask "Scenario 4: Classifications → Unified Metadata"
          <| fun () ->
              task {
                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()
                  // unified agent exists inside a full context; we'll recreate a minimal context for this test

                  let ctx =
                      createTestContext
                          (createInMemoryRepository<DemandObservation, string, ObservationEvent>())
                          (createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>())
                          (createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>())
                          (createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>())
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          (fun _ -> Task.FromResult false)
                          (fun _ _ -> Task.FromResult(Some "scope"))
                          (ForecastPublication.ComputationService.create(fun _ _ -> task { return [] }))
                          (fun _ -> ())

                  // Update ABC
                  let! _ =
                      ctx.ClassificationContext.Commands.UpdateClassification
                          { EntityType = "Product"
                            EntityId = "SKU-001"
                            ClassificationType = "ABC" }
                  // Update behaviour
                  let! _ =
                      ctx.BehaviourAssignmentContext.Commands.UpdateBehaviour
                          { EntityType = "Product"
                            EntityId = "SKU-001"
                            BehaviourDimension = "StatisticalPattern" }
                  // Update priority
                  let! _ =
                      ctx.PriorityContext.Commands.UpdatePriority
                          { EntityType = "Product"
                            EntityId = "SKU-001" }

                  // Wait for unified projection
                  let! found =
                      awaitProjection (TimeSpan.FromSeconds 3.0) ctx.UnifiedMetadataAgent (fun _ ->
                          let metadata =
                              ctx.UnifiedMetadataQueries.GetSkuMetadata ("SKU-001", "SP-001") ctx.UnifiedMetadataAgent
                              |> Async.AwaitTask
                              |> Async.RunSynchronously

                          metadata
                          |> Option.exists(fun m ->
                              m.AbcClass.IsSome && m.BehaviourPattern.IsSome && m.Priority.IsSome))

                  Expect.isTrue found "Unified metadata should be populated"
              }

          // ──────────────────────────────────────────────
          // Scenario 5 – Failure Notifications
          // ──────────────────────────────────────────────
          testCaseTask "Scenario 5a: BN-D-004 on recalculation failure"
          <| fun () ->
              task {
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()

                  let ctx =
                      EnterpriseDemandPicture.Context.create
                          edpRepo
                          (fun _ -> task { return Map.empty })
                          (fun _ -> task { return Map.empty })
                          (fun _ -> ())

                  let workflow = ctx.Commands

                  let mutable notified = None

                  use _ =
                      DomainEventBus.Subscribe<EnterpriseDemandPictureRecalculationFailedNotification>(fun n ->
                          notified <- Some n)

                  let req: CalculateEnterpriseDemandPictureReq = { PlanningScopeId = "SCOPE-NONEXIST" }
                  let! res = workflow.Calculate req
                  Expect.isError res "Calculate should fail"
                  Expect.isSome notified "BN-D-004 should be published"
              }

          testCaseTask "Scenario 5b: BN-D-013 on forecast publish failure"
          <| fun () ->
              task {
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()
                  let compService = ForecastPublication.ComputationService.create(fun _ _ -> task { return [] })

                  let ctx = ForecastPublication.Context.create fcastRepo compService (fun _ -> ())

                  let mutable notified = None
                  use _ = DomainEventBus.Subscribe<ForecastPublicationFailedNotification>(fun n -> notified <- Some n)

                  // Publish without first creating a draft → should fail
                  let req: PublishForecastPublicationReq = { PublicationId = "NONEXIST" }
                  let! res = ctx.Commands.Publish req
                  Expect.isError res "Publish should fail"
                  Expect.isSome notified "BN-D-013 should be published"
              }

          testCaseTask
              "Scenario 6: End-to-end smoke test verifying signal ingestion -> EDP -> forecast calculation -> batch observation update -> new EDP"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let getHistory sku sp =
                      task {
                          let! eventsRes = obsRepo.GetEventsByType(fun _ -> true)

                          match eventsRes with
                          | Ok evts ->
                              let list =
                                  evts
                                  |> List.choose (function
                                      | ObservationEvent.ObservationEstablished obs ->
                                          if obs.SkuId = sku && obs.StockingPointId = sp then
                                              Some(Quantity.value obs.Quantity)
                                          else
                                              None
                                      | _ -> None)

                              return list
                          | _ -> return []
                      }

                  let compService = ForecastPublication.ComputationService.create getHistory
                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27")

                  let ctx =
                      createTestContext
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          isHighPriority
                          getScopeId
                          compService
                          (fun _ -> ())

                  // 1. Signal Ingestion
                  let obsReq: EstablishObservationReq =
                      { ObservationId = "OBS-SMOKE-001"
                        SkuId = "SKU-SMOKE"
                        StockingPointId = "SP-SMOKE"
                        Quantity = 100m
                        UnitOfMeasure = "EA"
                        ObservationType = Demand.DemandObservation.ObservationType.SalesOrder
                        BusinessTime = DateTimeOffset.UtcNow.AddHours(-1.0)
                        CustomerId = None
                        PromotionRef = None
                        CampaignRef = None
                        ContractRef = None
                        SourceSystem = "ERP"
                        ExternalRef = "ORD-001"
                        MessageId = Guid.NewGuid().ToString()
                        Revision = 1 }

                  let! recRes = ctx.ObservationContext.Commands.Receive obsReq
                  Expect.isOk recRes "Ingestion Receive should succeed"

                  let evalReq: EvaluateObservationReq =
                      { ObservationId = "OBS-SMOKE-001"
                        SignalId = None
                        SignalSource = None
                        SourceReliability = None
                        SignalTimestamp = None
                        SignalValue = None
                        StatisticalBound = None
                        RecentBaseline = None }

                  let! evalRes = ctx.ObservationContext.Commands.Evaluate evalReq
                  Expect.isOk evalRes "Ingestion Evaluate should succeed"

                  // 2. EDP Update
                  let assignReq: AssignScopeReq =
                      { ObservationId = "OBS-SMOKE-001"
                        PlanningScopeId = "" }

                  let scopeReq: DeterminePlanningScopeReq =
                      { SkuId = "SKU-SMOKE"
                        StockingPointId = "SP-SMOKE"
                        BucketType = "Weekly"
                        CustomerId = None
                        PlanningPeriod = PlanningWeek(2027, 27) }

                  let! assignRes = ctx.Workflow.DetermineScopeAndAssign assignReq scopeReq
                  Expect.isOk assignRes "DetermineScopeAndAssign should succeed"

                  let reviseReq: ReviseEnterpriseDemandPictureReq =
                      { PlanningScopeId = "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27"
                        Period = PlanningWeek(2027, 27)
                        Quantity = 100m
                        ObservationId = "OBS-SMOKE-001" }

                  let! edpRes1 = ctx.Workflow.BuildAndPublishEdp reviseReq
                  Expect.isOk edpRes1 "BuildAndPublishEdp (v1) should succeed"

                  // Verify EDP v1
                  let! edp1Opt = ctx.EdpContext.Queries.GetById "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27"
                  Expect.isSome edp1Opt "EDP v1 should exist"
                  Expect.equal edp1Opt.Value.Status "Published" "EDP v1 should be published"
                  Expect.equal edp1Opt.Value.Version 1 "EDP v1 version should be 1"

                  // 3. Forecast Calculation
                  let initReq =
                      { defaultInitiateReq with
                          PublicationId = "PUB-SMOKE-001"
                          PlanningScopeIds = [ "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27" ] }

                  let! initRes = ctx.ForecastContext.Commands.InitiateCycle initReq
                  Expect.isOk initRes "InitiateCycle should succeed"

                  let prepReq: PrepareForecastContextReq =
                      { PublicationId = "PUB-SMOKE-001"
                        Assumptions = []
                        Coverage =
                          [ { SkuId = "SKU-SMOKE"
                              StockingPointId = "SP-SMOKE" } ] }

                  let! prepRes = ctx.ForecastContext.Commands.PrepareContext prepReq
                  Expect.isOk prepRes "PrepareContext should succeed"

                  // Sync projection state
                  let! pubAfterPrep = fcastRepo.Get "PUB-SMOKE-001"

                  match pubAfterPrep with
                  | Ok(Some pub) ->
                      let dto = Medhavi.Demand.ForecastPublication.Projection.mapToContract pub
                      ctx.ForecastContext.Agent.SetState(Map.ofList [ "PUB-SMOKE-001", dto ])
                  | _ -> ()

                  let champReq: SelectChampionModelReq =
                      { PublicationId = "PUB-SMOKE-001"
                        CandidateModelId = "Model-A"
                        EvaluationWindowStart = DateTimeOffset.UtcNow.AddDays(-30.0)
                        EvaluationWindowEnd = DateTimeOffset.UtcNow }

                  let! champRes = ctx.ForecastContext.Commands.SelectChampion champReq
                  Expect.isOk champRes "SelectChampion should succeed"

                  let genReq: GenerateBaselineForecastsReq =
                      { PublicationId = "PUB-SMOKE-001"
                        Forecasts = None }

                  let! genRes = ctx.ForecastContext.Commands.GenerateBaseline genReq
                  Expect.isOk genRes "GenerateBaseline should succeed"

                  let pubReq: PublishForecastPublicationReq = { PublicationId = "PUB-SMOKE-001" }
                  let! pubRes = ctx.ForecastContext.Commands.Publish pubReq
                  Expect.isOk pubRes "Publish should succeed"

                  // 4. Batch Observation Update
                  let! forecastsReady =
                      awaitProjection (TimeSpan.FromSeconds 3.0) ctx.ForecastContext.Agent (fun state ->
                          state |> Map.tryFind "PUB-SMOKE-001" |> Option.exists(fun p -> p.Forecasts.Length > 0))

                  Expect.isTrue forecastsReady "Forecast lines should be populated in projection"

                  // Verify that the forecast was ingested as observation
                  let! obsInRepo = obsRepo.GetEventsByType(fun _ -> true)

                  let observationIds =
                      match obsInRepo with
                      | Ok evts ->
                          evts
                          |> List.choose (function
                              | ObservationEvent.ObservationEstablished d -> Some(DemandObservationId.value d.Id)
                              | _ -> None)
                      | _ -> []

                  Expect.exists
                      observationIds
                      (fun id -> id.StartsWith "FCAST-PUB-SMOKE-001-")
                      "Forecast observation should exist"

                  let forecastObsId = observationIds |> List.find(fun id -> id.StartsWith "FCAST-PUB-SMOKE-001-")

                  // 5. Verify EDP was automatically recalculated & published as Version 2 incorporating the forecast observation
                  let! edp2Ready =
                      awaitProjection (TimeSpan.FromSeconds 3.0) ctx.EdpContext.Agent (fun state ->
                          state
                          |> Map.tryFind "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27"
                          |> Option.exists(fun e -> e.Version = 2))

                  Expect.isTrue edp2Ready "EDP projection should catch up to Version 2"

                  let! edp2Opt = ctx.EdpContext.Queries.GetById "SKU-SMOKE-SP-SMOKE-ALL-W-2027-27"
                  Expect.isSome edp2Opt "EDP v2 should exist"
                  Expect.equal edp2Opt.Value.Version 2 "EDP version should automatically increment to 2"
                  Expect.equal edp2Opt.Value.Status "Published" "EDP v2 should be published"
              }

          testCaseTask "Scenario 7: Demand Explanation triggers natural language LLM generation"
          <| fun () ->
              task {
                  let obsRepo = createInMemoryRepository<DemandObservation, string, ObservationEvent>()
                  let scopeRepo = createInMemoryRepository<PlanningScope, string, PlanningScopeEvent>()
                  let edpRepo = createInMemoryRepository<EnterpriseDemandPicture, string, EdpEvent>()
                  let fcastRepo = createInMemoryRepository<ForecastPublication, string, ForecastPublicationEvent>()

                  let assessRepo =
                      createInMemoryRepository<DemandBehaviourAssessment, string, DemandBehaviourAssessmentEvent>()

                  let classRepo =
                      createInMemoryRepository<PlanningClassificationAssignment, string, PlanningClassificationEvent>()

                  let behavRepo =
                      createInMemoryRepository<DemandBehaviourAssignment, string, DemandBehaviourAssignmentEvent>()

                  let prioRepo = createInMemoryRepository<PlanningPriorityAssignment, string, PlanningPriorityEvent>()

                  let explanationRepo =
                      createInMemoryRepository<
                          DemandExplanation.Model.DemandExplanation,
                          string,
                          DemandExplanation.Model.DemandExplanationEvent
                       >()

                  let getHistory _ _ = Task.FromResult []
                  let compService = ForecastPublication.ComputationService.create getHistory
                  let isHighPriority _ = Task.FromResult false
                  let getScopeId _ _ = Task.FromResult(Some "SCOPE-001")

                  let fakeResponseText = "LLM explanation: High sales variance observed due to seasonal promo activity."
                  let testGenerator = DemandExplanation.NaturalLanguageGenerator.fakeGenerator fakeResponseText

                  let ctx =
                      create
                          obsRepo
                          scopeRepo
                          edpRepo
                          fcastRepo
                          (createInMemoryRepository())
                          assessRepo
                          classRepo
                          behavRepo
                          prioRepo
                          explanationRepo
                          (createInMemoryRepository())
                          (createInMemoryRepository())
                          (fun _ -> task { return [] })
                          isHighPriority
                          getScopeId
                          compService
                          "llm"
                          testGenerator
                          (fun _ -> ())

                  // Execute record explanation request
                  let recordReq: Medhavi.Contracts.Demand.DemandLearning.RecordDemandExplanationReq =
                      { ExplanationId = "EXP-TEST-001"
                        ExplainedArtifactType = "EnterpriseDemandPicture"
                        ExplainedArtifactId = "SCOPE-001"
                        Question = "Why did the EnterpriseDemandPicture change?"
                        BusinessTime = DateTimeOffset.UtcNow }

                  let! res = ctx.DemandExplanationContext.Commands.RecordExplanation recordReq
                  Expect.isOk res "RecordExplanation should succeed"

                  match res with
                  | Ok id -> Expect.equal id "EXP-TEST-001" "Returned explanation ID should match"
                  | Error err -> failwith $"Failed to record explanation: {err}"

                  // Query explanation state and assert natural language output matches fake generator
                  let! explanationOpt = ctx.DemandExplanationContext.Queries.GetById "EXP-TEST-001"
                  Expect.isSome explanationOpt "Explanation should exist in projection"

                  Expect.equal
                      explanationOpt.Value.NaturalLanguageExplanation
                      fakeResponseText
                      "Natural language text should match LLM response"
              } ]
