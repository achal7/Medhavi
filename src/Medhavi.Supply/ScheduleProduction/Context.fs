namespace Medhavi.Supply.ScheduleProduction

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ScheduleProductionContext =
    {
        Commands: ScheduleProductionApis
        ScheduleQueries: QueryService<ProductionScheduleDto, string>
        PublishedQueries: QueryService<PublishedProductionScheduleDto, string>
        Dispose: unit -> unit
    }

/// CA-S-007 Schedule Production Composition Root
module Context =

    let create
        (scheduleRepo:
            Repository<
                ProductionSchedule.Model.ProductionScheduleAggregate,
                string,
                ProductionSchedule.Model.ProductionScheduleEvent
             >)
        (scheduleDeps: EnvelopeStoreDependencies<ProductionSchedule.Model.ProductionScheduleEvent>)
        (scheduleQueryService:
            CreateQueryService<ProductionSchedule.Model.ProductionScheduleEvent, ProductionScheduleDto, string>)
        (publishedRepo:
            Repository<
                PublishedProductionSchedule.Model.PublishedProductionScheduleAggregate,
                string,
                PublishedProductionSchedule.Model.PublishedProductionScheduleEvent
             >)
        (publishedDeps: EnvelopeStoreDependencies<PublishedProductionSchedule.Model.PublishedProductionScheduleEvent>)
        (publishedQueryService:
            CreateQueryService<
                PublishedProductionSchedule.Model.PublishedProductionScheduleEvent,
                PublishedProductionScheduleDto,
                string
             >)
        (publishKnowledge: KnowledgeRepresentation)
        (ports: SupplyPorts)
        (defaultResourceId: string)
        (defaultPlanningPeriodId: string)
        (defaultPlanningScopeId: string)
        : TaskResult<ScheduleProductionContext, ApplicationError> =
        taskResult {
            // 1. Projections
            let! (schedAggregates: ProductionSchedule.Model.ProductionScheduleAggregate list) =
                scheduleRepo.GetAll()
                |> TaskResult.ofTask
                |> TaskResult.mapError(fun e -> Infrastructure(Database(sprintf "%A" e)))

            let schedInitial = ProductionSchedule.Projections.seedFromAggregates schedAggregates

            let schedFilter =
                [ ArsIdentifiers.EnterpriseEvents.productionScheduleActivated.Id ] |> EnvelopeFilter.EventTypes

            let! (schedQueryCtx: ProjectionContext<ProductionScheduleDto, string>) =
                scheduleQueryService ProductionSchedule.Projections.apply schedFilter schedInitial "ProductionSchedule"

            let! (pubAggregates: PublishedProductionSchedule.Model.PublishedProductionScheduleAggregate list) =
                publishedRepo.GetAll()
                |> TaskResult.ofTask
                |> TaskResult.mapError(fun e -> Infrastructure(Database(sprintf "%A" e)))

            let pubInitial = PublishedProductionSchedule.Projections.seedFromAggregates pubAggregates

            let pubFilter =
                [ ArsIdentifiers.EnterpriseEvents.productionSchedulePublished.Id ] |> EnvelopeFilter.EventTypes

            let! (pubQueryCtx: ProjectionContext<PublishedProductionScheduleDto, string>) =
                publishedQueryService
                    PublishedProductionSchedule.Projections.apply
                    pubFilter
                    pubInitial
                    "PublishedProductionSchedule"

            // 2. Child Aggregate APIs
            let schedApi =
                ProductionSchedule.Capabilities.create
                    scheduleRepo
                    publishKnowledge
                    ProductionSchedule.Policies.defaultSchedulingPolicy
                    scheduleDeps
                    ports

            let pubApi =
                PublishedProductionSchedule.Capabilities.create
                    publishedRepo
                    publishKnowledge
                    PublishedProductionSchedule.Policies.defaultPublicationPolicy
                    publishedDeps
                    ports

            // 3. Parent Capability API
            let apis = Capabilities.create schedApi pubApi

            // 4. Workflows
            let wfDeps: ScheduleProductionWorkflowDependencies =
                {
                    Subscribe = scheduleDeps.Subscribe
                    DispatchEnvelope = publishedDeps.DispatchEnvelope
                    Apis = apis
                    DefaultResourceId = defaultResourceId
                    DefaultPlanningPeriodId = defaultPlanningPeriodId
                    DefaultPlanningScopeId = defaultPlanningScopeId
                    DefaultScheduleType = Medhavi.Contracts.Supply.ScheduleType.Primary
                }

            let! (sub1: IDisposable) = Workflows.createScheduleProductionWorkflow wfDeps CancellationToken.None
            let! (sub2: IDisposable) = Workflows.createPublishedProductionScheduleWorkflow wfDeps CancellationToken.None
            let! (sub3: IDisposable) = Workflows.createNotificationNodeWorkflow wfDeps CancellationToken.None

            let cleanup () =
                schedQueryCtx.Dispose()
                pubQueryCtx.Dispose()
                sub1.Dispose()
                sub2.Dispose()
                sub3.Dispose()

            let resultContext: ScheduleProductionContext =
                {
                    Commands = apis
                    ScheduleQueries = schedQueryCtx.QueryService
                    PublishedQueries = pubQueryCtx.QueryService
                    Dispose = cleanup
                }

            return resultContext
        }
