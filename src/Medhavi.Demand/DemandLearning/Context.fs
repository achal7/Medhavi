module Medhavi.Demand.DemandLearning.Context

open System
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Observation
open Medhavi.Demand.DemandLearning.Model
open Medhavi.Demand.DemandLearning.Projection
open Medhavi.Demand.DemandLearning.CommandHandler
open Medhavi.Demand.DemandLearning.Capabilities
open Medhavi.Demand
open System.Threading.Tasks
open Medhavi.Demand.DemandLearningAlgorithms
open Medhavi.Contracts.Demand.ForecastQualityAssessment

type DemandLearningContext =
    { Commands: DemandLearningApi
      Queries: DemandLearningQueries
      Agent: LearningAgent
      Dispose: unit -> unit }

let create
    (repo: Repository<DemandLearning, string, DemandLearningEvent>)
    (getAssessments: PlanningScopeId -> Task<QualityAssessmentSnapshot list>)
    (publishKnowledge: ArchitecturalKnowledge -> unit)
    : DemandLearningContext =

    let executeCmd = CommandHandler.execute repo publishKnowledge
    let agent = createProjectionAgent()
    let queries = createQueryService agent
    let api = createCapabilities executeCmd getAssessments

    let mutable subscriptions: IDisposable list = []

    // Subscribe to quality assessment publications → trigger learning analysis
    let qualitySub =
        DomainEventBus.Subscribe<ForecastQualityAssessmentPublishedNotification>(fun n ->
            let req: RecordDemandLearningReq =
                { LearningId = $"LEARN-{Guid.NewGuid():N}"
                  PlanningScopeId = n.PlanningScopeId
                  LearningType = "PostMortemQuality"
                  LearningStatement = "Automatic quality analysis triggered"
                  SupportingEvidence = [ n.KeyMetricsSummary ]
                  EvidenceStrength = "Pending"
                  SourceAnalysisRef = n.AssessmentId
                  BusinessTime = n.EvaluationPeriodEnd }

            api.RecordLearning req |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    subscriptions <- qualitySub :: subscriptions

    // Subscribe to planning condition resolutions → trigger learning analysis
    let conditionSub =
        DomainEventBus.Subscribe<DemandPlanningConditionResolvedNotification>(fun n ->
            let req: RecordDemandLearningReq =
                { LearningId = $"LEARN-{Guid.NewGuid():N}"
                  PlanningScopeId = "" // no scope available; algorithm will skip trend analysis
                  LearningType = "PostMortemCondition"
                  LearningStatement = $"Condition {n.ConditionType} resolved for {n.PlanningEntity}"
                  SupportingEvidence = [ n.ResolutionEvidence ]
                  EvidenceStrength = "Pending"
                  SourceAnalysisRef = n.ConditionId
                  BusinessTime = n.ResolutionTimestamp }

            api.RecordLearning req |> Async.AwaitTask |> Async.RunSynchronously |> ignore)

    subscriptions <- conditionSub :: subscriptions

    // Seed projection and subscribe to own events
    task {
        let! all = repo.GetAll()

        match all with
        | Ok learnings ->
            let map = learnings |> List.map(fun l -> DemandLearningId.value l.Id, mapToContract l) |> Map.ofList
            agent.SetState map
        | _ -> ()

        let ownSub = DomainEventBus.Subscribe<DemandLearningEvent>(fun ev -> agent.Post(ev, Guid.NewGuid(), None))
        subscriptions <- ownSub :: subscriptions
    }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    let dispose () =
        for sub in subscriptions do
            sub.Dispose()

        subscriptions <- []

    { Commands = api
      Queries = queries
      Agent = agent
      Dispose = dispose }
