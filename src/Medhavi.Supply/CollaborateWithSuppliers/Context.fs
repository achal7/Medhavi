namespace Medhavi.Supply.CollaborateWithSuppliers

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Model
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Decisions
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Model
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Policies
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Model
open Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.Policies

type CollaborateWithSuppliersContext =
    { Commands: CollaborateWithSuppliersApis
      CommitmentQueries: QueryService<SupplierCommitmentDto, SupplierCommitmentId>
      AssessmentQueries: QueryService<SupplierCommitmentAssessmentDto, SupplierId>
      HealthQueries: QueryService<SupplierCollaborationHealthAssessmentDto, SupplierCollaborationHealthAssessmentId>
      Dispose: unit -> unit }

/// CA-S-005 Collaborate with Suppliers Composition Root
module Context =

    let create
        (commitmentRepo: Repository<SupplierCommitment, SupplierCommitmentId, SupplierCommitmentEvent>)
        (commitmentDeps: EnvelopeStoreDependencies<SupplierCommitmentEvent>)
        (commitmentQueryService: CreateQueryService<SupplierCommitmentEvent, SupplierCommitmentDto, SupplierCommitmentId>)
        (assessmentRepo: Repository<SupplierCommitmentAssessment, SupplierId, SupplierCommitmentAssessmentEvent>)
        (assessmentDeps: EnvelopeStoreDependencies<SupplierCommitmentAssessmentEvent>)
        (assessmentQueryService: CreateQueryService<SupplierCommitmentAssessmentEvent, SupplierCommitmentAssessmentDto, SupplierId>)
        (healthRepo: Repository<SupplierCollaborationHealthAssessment, SupplierCollaborationHealthAssessmentId, SupplierCollaborationHealthEvent>)
        (healthDeps: EnvelopeStoreDependencies<SupplierCollaborationHealthEvent>)
        (healthQueryService: CreateQueryService<SupplierCollaborationHealthEvent, SupplierCollaborationHealthAssessmentDto, SupplierCollaborationHealthAssessmentId>)
        (supplierPolicy: SupplierCollaborationPolicy)
        (healthPolicy: SupplierCollaborationHealthPolicy)
        (publishKnowledge: KnowledgeRepresentation)
        (ports: SupplyPorts)
        (defaultSupplierId: string)
        (defaultItemId: string)
        (defaultScopeId: string)
        (defaultPeriodId: string)
        : TaskResult<CollaborateWithSuppliersContext, ApplicationError> =
        taskResult {

            // 1. Projections
            let! commitmentAggs: SupplierCommitment list =
                commitmentRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let commitmentInitial = SupplierCommitment.Projections.seedFromAggregates commitmentAggs
            let commitmentFilter = [ ArsIdentifiers.EnterpriseEvents.supplierCommitmentChanged.Id ] |> EnvelopeFilter.EventTypes
            let! commitmentQueryCtx: ProjectionContext<SupplierCommitmentDto, SupplierCommitmentId> =
                commitmentQueryService SupplierCommitment.Projections.apply commitmentFilter commitmentInitial "SupplierCommitment"

            let! assessmentAggs: SupplierCommitmentAssessment list =
                assessmentRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let assessmentInitial = SupplierCommitmentAssessment.Projections.seedFromAggregates assessmentAggs
            let assessmentFilter = [ ArsIdentifiers.EnterpriseEvents.supplierCommitmentAssessmentChanged.Id ] |> EnvelopeFilter.EventTypes
            let! assessmentQueryCtx: ProjectionContext<SupplierCommitmentAssessmentDto, SupplierId> =
                assessmentQueryService SupplierCommitmentAssessment.Projections.apply assessmentFilter assessmentInitial "SupplierCommitmentAssessment"

            let! healthAggs: SupplierCollaborationHealthAssessment list =
                healthRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let healthInitial = SupplierCollaborationHealth.Projections.seedFromAggregates healthAggs
            let healthFilter = [ ArsIdentifiers.EnterpriseEvents.supplierCollaborationHealthAssessmentPublished.Id ] |> EnvelopeFilter.EventTypes
            let! healthQueryCtx: ProjectionContext<SupplierCollaborationHealthAssessmentDto, SupplierCollaborationHealthAssessmentId> =
                healthQueryService SupplierCollaborationHealth.Projections.apply healthFilter healthInitial "SupplierCollaborationHealthAssessment"

            // 2. Fact Providers
            let factsProvider: SupplierCommitment.Behaviors.SupplierEvaluationFactsProvider =
                fun supplierId itemId ->
                    let telemetryOpt = (ports.GetSupplierTelemetry supplierId).GetAwaiter().GetResult()
                    match telemetryOpt with
                    | Some t ->
                        { AcknowledgeHours = t.AverageAcknowledgementHours
                          CommitHours = t.AverageCommitmentHours
                          HistoricalOtif = t.HistoricalOTIF
                          IsCriticalItem = false
                          ActiveRiskSignals = [] }
                    | None ->
                        { AcknowledgeHours = 24.0m
                          CommitHours = 48.0m
                          HistoricalOtif = Some 0.96m
                          IsCriticalItem = false
                          ActiveRiskSignals = [] }

            let assessmentFactsProvider: SupplierCommitmentAssessment.Behaviors.SupplierAssessmentFactsProvider =
                fun supplierId ->
                    let portfolioOpt = (ports.GetSupplierPortfolio supplierId).GetAwaiter().GetResult()
                    let telemetryOpt = (ports.GetSupplierTelemetry supplierId).GetAwaiter().GetResult()
                    let portfolio =
                        defaultArg portfolioOpt
                            { OpenCommitmentsCount = 5
                              OpenCommitmentsQuantity = 500.0m
                              EstimatedPortfolioValue = 25000.0m }
                    let telemetry =
                        defaultArg telemetryOpt
                            { HistoricalOTIF = Some 0.95m
                              HistoricalLeadTimeStdDevDays = Some 1.5m
                              MeanPromiseDateSlippageDays = Some 0.5m
                              QualifiedAlternateSuppliersCount = 2
                              CategoryTotalSpend = 100000.0m
                              ContractedVolumeSurgeTolerance = 0.20m
                              QualityAcceptanceRate = Some 0.98m
                              RevisionsInsideFrozenWindow = 0
                              AverageAcknowledgementHours = 18.0m
                              AverageCommitmentHours = 36.0m
                              DormancyDays = 3.0m }
                    portfolio, telemetry

            let scopeAssessmentsProvider: SupplierCollaborationHealth.Behaviors.ScopeAssessmentsProvider =
                fun scopeId ->
                    let suppliers = (ports.GetScopeSuppliers scopeId).GetAwaiter().GetResult()
                    let (assessments: Result<SupplierCommitmentAssessment option, RepositoryError> array) =
                        suppliers
                        |> List.map (fun sid -> assessmentRepo.Get sid)
                        |> Task.WhenAll
                        |> fun t -> t.GetAwaiter().GetResult()

                    assessments
                    |> Array.choose (function Ok(Some a) -> Some a | _ -> None)
                    |> Array.map (fun a ->
                        { SupplierId = a.SupplierId
                          OnTimeDeliveryRate = a.OnTimeDeliveryRate
                          AverageAcknowledgementHours = a.AverageAcknowledgementHours
                          ActiveRiskSignalsCount = a.ActiveRiskSignals.Length
                          AuthoritativeHealthState = a.AuthoritativeHealthState
                          OverallConfidence = a.OverallConfidence })
                    |> Array.toList

            // 3. Child Aggregate APIs
            let commitmentApi = SupplierCommitment.Capabilities.create commitmentRepo publishKnowledge commitmentDeps factsProvider
            let assessmentApi = SupplierCommitmentAssessment.Capabilities.create assessmentRepo publishKnowledge assessmentDeps assessmentFactsProvider supplierPolicy
            let healthApi = SupplierCollaborationHealth.Capabilities.create healthRepo publishKnowledge healthDeps scopeAssessmentsProvider healthPolicy

            // 4. Parent Capability API
            let apis =
                Capabilities.create
                    { CommitmentAggregate = commitmentApi
                      AssessmentAggregate = assessmentApi
                      HealthAggregate = healthApi }

            // 5. Background Workflows
            let wfDeps: CollaborateWithSuppliersWorkflowDependencies =
                { Subscribe = commitmentDeps.Subscribe
                  DispatchEnvelope = commitmentDeps.DispatchEnvelope
                  Apis = apis
                  DefaultSupplierId = defaultSupplierId
                  DefaultItemId = defaultItemId
                  DefaultScopeId = defaultScopeId
                  DefaultPeriodId = defaultPeriodId }

            let! (sub1: IDisposable) = Workflows.createCommitmentCollaborationWorkflow wfDeps CancellationToken.None
            let! (sub2: IDisposable) = Workflows.createHealthCadenceWorkflow wfDeps CancellationToken.None
            let! (sub3: IDisposable) = Workflows.createNotificationNodeWorkflow wfDeps CancellationToken.None

            let dispose () =
                commitmentQueryCtx.Dispose ()
                assessmentQueryCtx.Dispose ()
                healthQueryCtx.Dispose ()
                sub1.Dispose ()
                sub2.Dispose ()
                sub3.Dispose ()

            return
                { Commands = apis
                  CommitmentQueries = commitmentQueryCtx.QueryService
                  AssessmentQueries = assessmentQueryCtx.QueryService
                  HealthQueries = healthQueryCtx.QueryService
                  Dispose = dispose }
        }
