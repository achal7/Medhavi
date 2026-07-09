module Medhavi.Demand.DemandIntelligenceContext

open System
open System.Threading.Tasks
open Medhavi.Demand.EnterpriseDemandPicture.Model
open Medhavi.Demand.Projections.DecisionTraceProjection
open Medhavi.SharedKernel
open Medhavi.Contracts
open Medhavi.SharedKernel.Contracts
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandObservation
open Medhavi.Demand.PlanningScope
open Medhavi.Demand.EnterpriseDemandPicture
open Medhavi.Demand.ForecastPublication
open Medhavi.Demand.DemandBehaviourAssessment
open Medhavi.Demand.PlanningClassificationAssignment
open Medhavi.Demand.DemandBehaviourAssignment
open Medhavi.Demand.PlanningPriorityAssignment
open Medhavi.Demand.Projections.UnifiedPlanningMetadataProjection
open Medhavi.Demand.DemandObservation.Context
open Medhavi.Demand.PlanningScope.Context
open Medhavi.Demand.EnterpriseDemandPicture.Context
open Medhavi.Demand.ForecastPublication.Context
open Medhavi.Demand.DemandBehaviourAssessment.Context
open Medhavi.Demand.PlanningClassificationAssignment.Context
open Medhavi.Demand.DemandBehaviourAssignment.Context
open Medhavi.Demand.PlanningPriorityAssignment.Context
open Medhavi.Demand.Application.UnderstandDemandWorkflow
open Medhavi.Demand.DemandLearning
open DemandLearningAlgorithms
open Medhavi.Demand.ForecastQualityAssessment
open Medhavi.Demand.DemandExplanation
open Medhavi.Demand.DemandPlanningCondition
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.Contracts.Demand.ForecastPublication

type DemandIntelligenceContext =
    { ObservationContext: ObservationContext
      PlanningScopeContext: PlanningScopeContext
      EdpContext: EdpContext
      ForecastContext: ForecastPublicationContext
      ForecastQualityContext: Context.ForecastQualityAssessmentContext
      BehaviourAssessmentContext: DemandBehaviourAssessmentContext
      ClassificationContext: PlanningClassificationContext
      BehaviourAssignmentContext: DemandBehaviourAssignmentContext
      PriorityContext: PlanningPriorityAssignmentContext
      DemandExplanationContext: Context.DemandExplanationContext
      DemandPlanningConditionContext: Context.DemandPlanningConditionContext
      DemandLearningContext: Context.DemandLearningContext
      UnifiedMetadataQueries: UnifiedMetadataQueries
      Workflow: UnderstandDemandWorkflow
      UnifiedMetadataAgent: UnifiedMetadataAgent
      Dispose: unit -> unit }

// ForecastQualityAssessment context with real data access
let createForecastQualityCtx
    (forecastQualityRepo: Repository<Model.ForecastQualityAssessment, string, Model.ForecastQualityAssessmentEvent>)
    (edpCtx: EdpContext)
    (forecastCtx: ForecastPublicationContext)
    publishKnowledge
    =
    let getActuals (scopeId: PlanningScopeId) (start: Timestamp) (end_: Timestamp) =
        task {
            // Operational demand from the Enterprise Demand Picture projection
            let! edpOpt = edpCtx.Queries.GetById(PlanningScopeId.value scopeId)

            return
                edpOpt
                |> Option.map(fun edp ->
                    edp.Periods
                    |> List.filter(fun p ->
                        p.Period.ToDateTimeOffset() >= (Timestamp.value start)
                        && p.Period.ToDateTimeOffset() <= (Timestamp.value end_))
                    |> List.map(fun p -> p.OperationalDemand))
                |> Option.defaultValue []
        }

    let getForecasts (scopeId: PlanningScopeId) (start: Timestamp) (end_: Timestamp) =
        task {
            // Forecast values from the latest published ForecastPublication projection
            let! pubs =
                forecastCtx.Queries.Filter(fun p ->
                    p.Status = "Published" && p.PlanningScopeIds |> List.contains(PlanningScopeId.value scopeId))

            let latestPub =
                pubs
                |> List.sortByDescending(fun p -> p.PublicationTime |> Option.defaultValue DateTimeOffset.MinValue)
                |> List.tryHead

            return
                latestPub
                |> Option.map(fun pub ->
                    pub.Forecasts
                    |> List.filter(fun f ->
                        f.PlanningPeriod.ToDateTimeOffset() >= (Timestamp.value start)
                        && f.PlanningPeriod.ToDateTimeOffset() <= (Timestamp.value end_))
                    |> List.map(fun f -> f.Mean))
                |> Option.defaultValue []
        }

    let getNaiveForecasts (scopeId: PlanningScopeId) (start: Timestamp) (end_: Timestamp) = task { return None } // naive forecasts not yet modelled; FVA is optional, algorithm handles None

    let getOverrideHistory (scopeId: PlanningScopeId) (start: Timestamp) (end_: Timestamp) = task { return [] } // override history retrieval to be added when planner overrides are stored in a queryable form

    let getHistoricalForecasts (scopeId: PlanningScopeId) (periods: int) = task { return [] } // historical forecast sequences not yet queryable; stability metric is optional

    ForecastQualityAssessment.Context.create
        forecastQualityRepo
        getActuals
        getForecasts
        getNaiveForecasts
        getOverrideHistory
        getHistoricalForecasts
        0.95m // policyThreshold (95% completeness required)
        7 // policyMinPeriod (7 days minimum evaluation period)
        { WAPE = 0.25m
          MAPE = 0.25m
          ForecastBias = 0.25m
          ForecastAccuracy = 0.25m } // policyWeights
        publishKnowledge

let createDemandLearningCtx
    (learningRepo: Repository<Model.DemandLearning, string, Model.DemandLearningEvent>)
    (forecastQualityCtx: Context.ForecastQualityAssessmentContext)
    publishKnowledge
    =
    let getAssessments (scopeId: PlanningScopeId) =
        task {
            let scopeIdStr = PlanningScopeId.value scopeId
            let! dtos = forecastQualityCtx.Queries.Filter(fun a -> a.PlanningScopeId = scopeIdStr)

            return
                dtos
                |> List.map(fun dto ->
                    { QualityAssessmentSnapshot.ScopeId = dto.PlanningScopeId
                      PeriodEnd = dto.EvaluationPeriodEnd
                      Wape = dto.WAPE })
        }

    DemandLearning.Context.create learningRepo getAssessments publishKnowledge

let createDemandExplanationCtx
    (explanationRepo: Repository<Model.DemandExplanation, string, Model.DemandExplanationEvent>)
    (generator: DemandExplanation.NaturalLanguageGenerator)
    (traceAgent: TraceAgent)
    (edpQuery: string -> Medhavi.Common.Patterns.TaskResult<Model.EdpEvent list, RepositoryError>)
    (forecastQuery: string -> Medhavi.Common.Patterns.TaskResult<Model.ForecastPublication option, RepositoryError>)
    (obsQuery: string -> Medhavi.Common.Patterns.TaskResult<Model.DemandObservation option, RepositoryError>)
    (obsQueries: QueryService<Medhavi.Contracts.Demand.DemandObservation.DemandObservation, string>)
    (templateVersionRef: string)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    =
    let getDecisionTraces (artifactType: string) (artifactId: string) =
        let streamName =
            match artifactType with
            | "EnterpriseDemandPicture" -> "EnterpriseDemandPicture-" + artifactId
            | "ForecastPublication" -> "ForecastPublication-" + artifactId
            | "DemandObservation" -> "DemandObservation-" + artifactId
            | "DemandBehaviourAssessment" -> "DemandBehaviourAssessment-" + artifactId
            | _ -> artifactType + "-" + artifactId

        traceAgent.QueryAsync(fun state -> state |> Map.tryFind streamName |> Option.defaultValue [])

    let getSourceArtifactRefs
        (artifactType: string)
        (artifactId: string)
        : Task<DemandExplanation.Model.ExplanationSourceArtifactRef list> =
        task {
            match artifactType with
            | "EnterpriseDemandPicture" ->
                let! obsList = obsQueries.Filter(fun obs -> obs.PlanningScopeId = Some artifactId)

                let obsRefs =
                    obsList
                    |> List.map(fun obs ->
                        let props = [ "Quantity", string(obs.Quantity); "Status", string obs.Status ] |> Map.ofList

                        { ArtifactType = "DemandObservation"
                          ArtifactId = obs.Id
                          Version = 1
                          Properties = props }
                        : DemandExplanation.Model.ExplanationSourceArtifactRef)

                return
                    ({ ArtifactType = "EnterpriseDemandPicture"
                       ArtifactId = artifactId
                       Version = 1
                       Properties = Map.empty }
                    : DemandExplanation.Model.ExplanationSourceArtifactRef)
                    :: obsRefs
            | "ForecastPublication" ->
                let! pubOpt = forecastQuery artifactId

                let baseRefs =
                    [ ({ ArtifactType = "ForecastPublication"
                         ArtifactId = artifactId
                         Version = 1
                         Properties = Map.empty }
                      : DemandExplanation.Model.ExplanationSourceArtifactRef) ]

                let lineRefs =
                    pubOpt
                    |> Result.map(fun pubOpt ->
                        pubOpt
                        |> Option.map(fun pub ->
                            pub.Forecasts
                            |> Seq.map(fun f ->
                                ({ ArtifactType = "ForecastLine"
                                   ArtifactId = ForecastId.value f.Value.ForecastId
                                   Version = 1
                                   Properties = Map.empty }
                                : DemandExplanation.Model.ExplanationSourceArtifactRef))
                            |> List.ofSeq)
                        |> Option.defaultValue [])
                    |> Result.defaultValue []

                return baseRefs @ lineRefs

            | "DemandObservation" ->
                let! obsOpt = obsQuery artifactId

                let baseRefs =
                    [ ({ ArtifactType = "DemandObservation"
                         ArtifactId = artifactId
                         Version = 1
                         Properties = Map.empty }
                      : DemandExplanation.Model.ExplanationSourceArtifactRef) ]

                let decisionRefs =
                    obsOpt
                    |> Result.map(fun obsOpt ->
                        obsOpt
                        |> Option.map(fun obs ->
                            obs.Decision
                            |> Option.map(fun d ->
                                let props =
                                    [ "Quantity", string(Quantity.value obs.Quantity); "Status", string obs.Status ]
                                    |> Map.ofList

                                [ ({ ArtifactType = "ObservationDecision"
                                     ArtifactId = d.DecisionId
                                     Version = 1
                                     Properties = props }
                                  : DemandExplanation.Model.ExplanationSourceArtifactRef) ])
                            |> Option.defaultValue [])
                        |> Option.defaultValue [])
                    |> Result.defaultValue []

                return baseRefs @ decisionRefs
            | _ ->
                return
                    [ ({ ArtifactType = artifactType
                         ArtifactId = artifactId
                         Version = 1
                         Properties = Map.empty }
                      : DemandExplanation.Model.ExplanationSourceArtifactRef) ]
        }

    DemandExplanation.Context.create
        explanationRepo
        getDecisionTraces
        getSourceArtifactRefs
        templateVersionRef
        generator
        publishKnowledge

let create
    (obsRepo: Repository<Model.DemandObservation, string, Model.ObservationEvent>)
    (scopeRepo: Repository<Model.PlanningScope, string, Model.PlanningScopeEvent>)
    (edpRepo: Repository<Model.EnterpriseDemandPicture, string, Model.EdpEvent>)
    (forecastRepo: Repository<Model.ForecastPublication, string, Model.ForecastPublicationEvent>)
    (forecastQualityRepo: Repository<Model.ForecastQualityAssessment, string, Model.ForecastQualityAssessmentEvent>)
    (assessmentRepo: Repository<Model.DemandBehaviourAssessment, string, Model.DemandBehaviourAssessmentEvent>)
    (classRepo: Repository<Model.PlanningClassificationAssignment, string, Model.PlanningClassificationEvent>)
    (behaviourRepo: Repository<Model.DemandBehaviourAssignment, string, Model.DemandBehaviourAssignmentEvent>)
    (priorityRepo: Repository<Model.PlanningPriorityAssignment, string, Model.PlanningPriorityEvent>)
    (explanationRepo: Repository<Model.DemandExplanation, string, Model.DemandExplanationEvent>)
    (conditionRepo: Repository<Model.DemandPlanningCondition, string, Model.DemandPlanningConditionEvent>)
    (learningRepo: Repository<Model.DemandLearning, string, Model.DemandLearningEvent>)
    (getAssessments: PlanningScopeId -> Task<QualityAssessmentSnapshot list>)
    (isHighPriority: SkuId -> Task<bool>)
    (getScopeId: SkuId -> StockingPointId -> Task<string option>)
    (computationService: ComputationService.Service)
    (templateVersionRef: string)
    (generator: DemandExplanation.NaturalLanguageGenerator)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandIntelligenceContext =

    let obsCtx = DemandObservation.Context.create obsRepo publishKnowledge
    let scopeCtx = PlanningScope.Context.create scopeRepo publishKnowledge
    let forecastCtx = ForecastPublication.Context.create forecastRepo computationService publishKnowledge

    let getAdjustments (scopeId: PlanningScopeId) =
        task {
            let! res = obsRepo.GetAll()

            match res with
            | Error _ -> return Map.empty
            | Ok observations ->
                let scopeIdStr = PlanningScopeId.value scopeId
                let parts = scopeIdStr.Split('-')

                let periodOpt =
                    if parts.Length >= 6 then
                        let bucket = parts.[parts.Length - 3]
                        let year = int parts.[parts.Length - 2]
                        let num = int parts.[parts.Length - 1]

                        match bucket with
                        | "W" -> Some(PlanningPeriod.PlanningWeek(year, num))
                        | "D" -> Some(PlanningPeriod.PlanningDay(DateOnly(year, 1, 1).AddDays(num - 1)))
                        | "M" -> Some(PlanningPeriod.PlanningMonth(year, num))
                        | "Q" -> Some(PlanningPeriod.PlanningQuarter(year, num))
                        | _ -> None
                    else
                        None

                match periodOpt with
                | None -> return Map.empty
                | Some period ->
                    let totalQtyVal =
                        observations
                        |> List.filter(fun o ->
                            o.PlanningScopeId = Some scopeId
                            && (Option.isSome o.PromotionRef || Option.isSome o.CampaignRef))
                        |> List.sumBy(fun o -> Quantity.value o.Quantity)

                    if totalQtyVal > 0m then
                        let qty =
                            match Quantity.create totalQtyVal with
                            | Ok q -> q
                            | Error err -> failwith err.Message

                        return Map.ofList [ period, qty ]
                    else
                        return Map.empty
        }

    let getOverrides (scopeId: PlanningScopeId) =
        task {
            let scopeIdStr = PlanningScopeId.value scopeId

            let! pubs =
                forecastCtx.Queries.Filter(fun p ->
                    p.Status = "Published" && List.contains scopeIdStr p.PlanningScopeIds)

            match pubs |> List.sortByDescending(fun p -> p.Version) |> List.tryHead with
            | None -> return Map.empty
            | Some pub ->
                let overridesMap =
                    pub.Overrides
                    |> List.choose(fun o ->
                        pub.Forecasts
                        |> List.tryFind(fun f -> f.ForecastId = o.ForecastId)
                        |> Option.map(fun f ->
                            let qty =
                                match Quantity.create o.OverrideValue with
                                | Ok q -> q
                                | Error err -> failwith err.Message

                            f.PlanningPeriod, qty))
                    |> Map.ofList

                return overridesMap
        }

    let edpCtx = EnterpriseDemandPicture.Context.create edpRepo getAdjustments getOverrides publishKnowledge

    let assessmentCtx =
        DemandBehaviourAssessment.Context.create
            assessmentRepo
            isHighPriority
            forecastCtx.Queries
            forecastCtx.Commands
            getScopeId
            publishKnowledge

    let classCtx = PlanningClassificationAssignment.Context.create classRepo publishKnowledge
    let behaviourCtx = DemandBehaviourAssignment.Context.create behaviourRepo publishKnowledge
    let priorityCtx = PlanningPriorityAssignment.Context.create priorityRepo publishKnowledge

    let forecastQualityCtx = createForecastQualityCtx forecastQualityRepo edpCtx forecastCtx publishKnowledge
    let demandPlanningConditionCtx = Context.create conditionRepo publishKnowledge
    let demandLearningCtx = createDemandLearningCtx learningRepo forecastQualityCtx publishKnowledge

    let unifiedAgent = createProjectionAgent()
    let unifiedQueries = new UnifiedMetadataQueries()

    let workflow = createUnderstandDemandWorkflow obsCtx.Commands scopeCtx.Commands edpCtx.Commands forecastCtx.Queries

    let mutable subscriptions: IDisposable list = []

    let traceAgent = createTraceAgent()

    let demandExplanationCtx =
        createDemandExplanationCtx
            explanationRepo
            generator
            traceAgent
            edpRepo.GetEvents
            forecastRepo.Get
            obsRepo.Get
            obsCtx.Queries
            templateVersionRef
            publishKnowledge

    let envelopeSub = DomainEventBus.Subscribe<Envelope>(fun env -> traceAgent.Post(env, Guid.NewGuid(), None))
    subscriptions <- envelopeSub :: subscriptions

    // Subscribe unified metadata agent to all relevant events
    let sub1 = DomainEventBus.Subscribe<obj>(fun ev -> unifiedAgent.Post(ev, Guid.NewGuid(), None))
    subscriptions <- sub1 :: subscriptions

    // Subscribe to all individual aggregate events for projections
    let subObs = DomainEventBus.Subscribe<Model.ObservationEvent>(fun ev -> obsCtx.Agent.Post(ev, Guid.NewGuid(), None))
    subscriptions <- subObs :: subscriptions

    let subScope =
        DomainEventBus.Subscribe<Model.PlanningScopeEvent>(fun ev -> scopeCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subScope :: subscriptions
    let subEdp = DomainEventBus.Subscribe<Model.EdpEvent>(fun ev -> edpCtx.Agent.Post(ev, Guid.NewGuid(), None))
    subscriptions <- subEdp :: subscriptions

    let subForecast =
        DomainEventBus.Subscribe<Model.ForecastPublicationEvent>(fun ev ->
            forecastCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subForecast :: subscriptions

    let forecastSub =
        DomainEventBus.Subscribe<Medhavi.Contracts.Demand.ForecastPublication.ForecastPublishedNotification>(fun n ->
            workflow.OnForecastPublished n |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    subscriptions <- forecastSub :: subscriptions

    let subAssess =
        DomainEventBus.Subscribe<Model.DemandBehaviourAssessmentEvent>(fun ev ->
            assessmentCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subAssess :: subscriptions

    let subClass =
        DomainEventBus.Subscribe<Model.PlanningClassificationEvent>(fun ev ->
            classCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subClass :: subscriptions

    let subBehave =
        DomainEventBus.Subscribe<Model.DemandBehaviourAssignmentEvent>(fun ev ->
            behaviourCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subBehave :: subscriptions

    let subPrio =
        DomainEventBus.Subscribe<Model.PlanningPriorityEvent>(fun ev ->
            priorityCtx.Agent.Post(ev, Guid.NewGuid(), None))

    subscriptions <- subPrio :: subscriptions

    // Auto‑explain on forecast publication (PO‑D‑047)
    let autoExplainForecastSub =
        DomainEventBus.Subscribe<ForecastPublishedNotification>(fun n ->
            let req: RecordDemandExplanationReq =
                { ExplanationId = $"EXPL-FCAST-{n.PublicationId}"
                  ExplainedArtifactType = "ForecastPublication"
                  ExplainedArtifactId = n.PublicationId
                  Question = ""
                  BusinessTime = n.PublicationTime }

            demandExplanationCtx.Commands.RecordExplanation req |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    subscriptions <- autoExplainForecastSub :: subscriptions

    // Auto‑explain on critical planning condition detected (PO‑D‑047)
    let autoExplainConditionSub =
        DomainEventBus.Subscribe<DemandPlanningConditionDetectedNotification>(fun n ->
            let req: RecordDemandExplanationReq =
                { ExplanationId = $"EXPL-COND-{n.ConditionId}"
                  ExplainedArtifactType = "DemandPlanningCondition"
                  ExplainedArtifactId = n.ConditionId
                  Question = ""
                  BusinessTime = n.DetectionTimestamp }

            demandExplanationCtx.Commands.RecordExplanation req |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    subscriptions <- autoExplainConditionSub :: subscriptions

    let dispose () =
        subscriptions |> List.iter(fun s -> s.Dispose())
        subscriptions <- []
        obsCtx.Dispose()
        scopeCtx.Dispose()
        edpCtx.Dispose()
        forecastCtx.Dispose()
        assessmentCtx.Dispose()
        classCtx.Dispose()
        behaviourCtx.Dispose()
        priorityCtx.Dispose()
        demandLearningCtx.Dispose()
        demandExplanationCtx.Dispose()
        demandPlanningConditionCtx.Dispose()

    { ObservationContext = obsCtx
      PlanningScopeContext = scopeCtx
      EdpContext = edpCtx
      ForecastContext = forecastCtx
      ForecastQualityContext = forecastQualityCtx
      BehaviourAssessmentContext = assessmentCtx
      ClassificationContext = classCtx
      BehaviourAssignmentContext = behaviourCtx
      PriorityContext = priorityCtx
      DemandLearningContext = demandLearningCtx
      DemandExplanationContext = demandExplanationCtx
      DemandPlanningConditionContext = demandPlanningConditionCtx
      UnifiedMetadataQueries = unifiedQueries
      Workflow = workflow
      UnifiedMetadataAgent = unifiedAgent
      Dispose = dispose }
