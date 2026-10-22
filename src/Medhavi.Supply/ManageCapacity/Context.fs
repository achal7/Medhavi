namespace Medhavi.Supply.ManageCapacity

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
open Medhavi.Supply.ArsIdentifiers
open Workflows
open Medhavi.Supply.ManageCapacity.CapacityHealth.Model
open Medhavi.Supply.ManageCapacity.CapacityHealth.Policies
open Medhavi.Supply.ManageCapacity.CapacityPosition.Model
open Medhavi.Supply.ManageCapacity.CapacityPosition.Policies

type ManageCapacityContext =
    { Commands: ManageCapacityApis
      PositionQueries: QueryService<CapacityPositionAssessmentDto, PhysicalResourceId>
      HealthQueries: QueryService<CapacityHealthAssessmentDto, CapacityHealthAssessmentId>
      Dispose: unit -> unit }

/// CA-S-004 Manage Capacity Composition Root
module Context =

    let create
        (positionRepo: Repository<CapacityPositionAssessment, PhysicalResourceId, CapacityPositionEvent>)
        (positionDeps: EnvelopeStoreDependencies<CapacityPositionEvent>)
        (positionQueryService: CreateQueryService<CapacityPositionEvent, CapacityPositionAssessmentDto, PhysicalResourceId>)
        (healthRepo: Repository<CapacityHealthAssessment, CapacityHealthAssessmentId, CapacityHealthEvent>)
        (healthDeps: EnvelopeStoreDependencies<CapacityHealthEvent>)
        (healthQueryService: CreateQueryService<CapacityHealthEvent, CapacityHealthAssessmentDto, CapacityHealthAssessmentId>)
        (positionPolicy: CapacityManagementPolicy)
        (healthPolicy: CapacityHealthPolicy)
        (publishKnowledge: KnowledgeRepresentation)
        (ports: SupplyPorts)
        (defaultResourceId: string)
        (defaultScopeId: string)
        (defaultPeriodId: string)
        : TaskResult<ManageCapacityContext, ApplicationError> =
        taskResult {

            // 1. Projections
            let! posAggs: CapacityPositionAssessment list =
                positionRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let posInitial = CapacityPosition.Projections.seedFromAggregates posAggs
            let posFilter = [ ArsIdentifiers.EnterpriseEvents.capacityPositionAssessmentChanged.Id ] |> EnvelopeFilter.EventTypes
            let! posQueryCtx: ProjectionContext<CapacityPositionAssessmentDto, PhysicalResourceId> =
                positionQueryService CapacityPosition.Projections.apply posFilter posInitial "CapacityPosition"

            let! healthAggs: CapacityHealthAssessment list =
                healthRepo.GetAll ()
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError

            let healthInitial = CapacityHealth.Projections.seedFromAggregates healthAggs
            let healthFilter = [ ArsIdentifiers.EnterpriseEvents.capacityHealthAssessmentPublished.Id ] |> EnvelopeFilter.EventTypes
            let! healthQueryCtx: ProjectionContext<CapacityHealthAssessmentDto, CapacityHealthAssessmentId> =
                healthQueryService CapacityHealth.Projections.apply healthFilter healthInitial "CapacityHealth"

            // 2. Scope Positions Provider for Health synthesis
            let scopePositionsProvider (scopeId: PlanningScopeId) : Task<CapacityPositionFact list> =
                task {
                    let! resourceIds = ports.GetScopeResources scopeId
                    let! (assessments: Result<CapacityPositionAssessment option, RepositoryError> array) =
                        resourceIds
                        |> List.map (fun rid -> positionRepo.Get rid)
                        |> Task.WhenAll

                    return
                        assessments
                        |> Array.choose (function Ok(Some a) -> Some a | _ -> None)
                        |> Array.map (fun a ->
                            { ResourceId = a.ResourceId
                              EffectiveUtilization = a.EffectiveUtilization
                              UtilizationStatus = a.UtilizationStatus
                              ConstraintStatus = a.ConstraintStatus
                              BottleneckStatus = a.BottleneckStatus
                              HasSinglePointOfFailureRisk = a.HasSinglePointOfFailureRisk
                              ChangeEventsCount = a.ChangeHistory.Length
                              AvailableCapacityHours = a.AvailableCapacityHours
                              CurrentCommittedHours = a.CurrentCommittedHours
                              ProjectedPlannedHours = a.ProjectedPlannedHours })
                        |> Array.toList
                }

            // 3. Child Aggregate APIs
            let posApi = CapacityPosition.Capabilities.create positionRepo publishKnowledge positionDeps ports positionPolicy
            let healthApi = CapacityHealth.Capabilities.create healthRepo publishKnowledge healthPolicy healthDeps ports scopePositionsProvider

            // 4. Parent Capability API
            let apis = Capabilities.create posApi healthApi

            // 5. Background Workflows
            let wfDeps: ManageCapacityWorkflowDependencies =
                { Subscribe = positionDeps.Subscribe
                  DispatchEnvelope = positionDeps.DispatchEnvelope
                  Apis = apis
                  DefaultResourceId = defaultResourceId
                  DefaultScopeId = defaultScopeId
                  DefaultPeriodId = defaultPeriodId }

            let! (sub1: IDisposable) = Workflows.createPositionUpdateWorkflow wfDeps CancellationToken.None
            let! (sub2: IDisposable) = Workflows.createHealthAssessmentWorkflow wfDeps CancellationToken.None
            let! (sub3: IDisposable) = Workflows.createNotificationNodeWorkflow wfDeps CancellationToken.None

            let dispose () =
                posQueryCtx.Dispose ()
                healthQueryCtx.Dispose ()
                sub1.Dispose ()
                sub2.Dispose ()
                sub3.Dispose ()

            return
                { Commands = apis
                  PositionQueries = posQueryCtx.QueryService
                  HealthQueries = healthQueryCtx.QueryService
                  Dispose = dispose }
        }
