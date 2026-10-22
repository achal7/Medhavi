/// CA-S-003 Manage Inventory Composition Root
module Medhavi.Supply.ManageInventory.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Capabilities
open Workflows

type ManageInventoryContext =
    { Commands: ManageInventoryApis
      PositionQueries: QueryService<InventoryPositionAssessmentDto, ItemLocationId>
      PolicyQueries: QueryService<InventoryPolicyAssignmentDto, ItemLocationId>
      HealthQueries: QueryService<InventoryHealthAssessmentDto, InventoryHealthAssessmentId>
      Dispose: unit -> unit }

let create
    (positionRepo: Repository<InventoryPosition.Model.InventoryPositionAssessment, ItemLocationId, InventoryPosition.Model.InventoryPositionEvent>)
    (positionDeps: EnvelopeStoreDependencies<InventoryPosition.Model.InventoryPositionEvent>)
    (positionQueryService: CreateQueryService<InventoryPosition.Model.InventoryPositionEvent, InventoryPositionAssessmentDto, ItemLocationId>)
    (policyRepo: Repository<InventoryPolicy.Model.InventoryPolicyAssignment, ItemLocationId, InventoryPolicy.Model.InventoryPolicyEvent>)
    (policyDeps: EnvelopeStoreDependencies<InventoryPolicy.Model.InventoryPolicyEvent>)
    (policyQueryService: CreateQueryService<InventoryPolicy.Model.InventoryPolicyEvent, InventoryPolicyAssignmentDto, ItemLocationId>)
    (healthRepo: Repository<InventoryHealth.Model.InventoryHealthAssessment, InventoryHealthAssessmentId, InventoryHealth.Model.InventoryHealthEvent>)
    (healthDeps: EnvelopeStoreDependencies<InventoryHealth.Model.InventoryHealthEvent>)
    (healthQueryService: CreateQueryService<InventoryHealth.Model.InventoryHealthEvent, InventoryHealthAssessmentDto, InventoryHealthAssessmentId>)
    (policyGovernance: InventoryPolicy.Policies.InventoryPolicyGovernancePolicy)
    (healthPolicy: InventoryHealth.Policies.InventoryHealthAssessmentPolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: SupplyPorts)
    (defaultScopeId: string)
    (defaultPeriodId: string)
    (defaultLocationId: string)
    : TaskResult<ManageInventoryContext, ApplicationError> =
    taskResult {

        // 1. Projections
        let! posAggs: InventoryPosition.Model.InventoryPositionAssessment list =
            positionRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun err -> Infrastructure(Database(sprintf "%A" err)))

        let posInitial = InventoryPosition.Projections.seedFromAggregates posAggs
        let posFilter = [ ArsIdentifiers.EnterpriseEvents.inventoryPositionAssessmentChanged.Id ] |> EnvelopeFilter.EventTypes
        let! posQueryCtx: ProjectionContext<InventoryPositionAssessmentDto, ItemLocationId> =
            positionQueryService InventoryPosition.Projections.apply posFilter posInitial "InventoryPosition"

        let! polAggs: InventoryPolicy.Model.InventoryPolicyAssignment list =
            policyRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun err -> Infrastructure(Database(sprintf "%A" err)))

        let polInitial = InventoryPolicy.Projections.seedFromAggregates polAggs
        let polFilter = [ ArsIdentifiers.EnterpriseEvents.inventoryPolicyAssignmentUpdated.Id ] |> EnvelopeFilter.EventTypes
        let! polQueryCtx: ProjectionContext<InventoryPolicyAssignmentDto, ItemLocationId> =
            policyQueryService InventoryPolicy.Projections.apply polFilter polInitial "InventoryPolicy"

        let! healthAggs : InventoryHealth.Model.InventoryHealthAssessment list =
            healthRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun err -> Infrastructure(Database(sprintf "%A" err)))

        let healthInitial = InventoryHealth.Projections.seedFromAggregates healthAggs
        let healthFilter = [ ArsIdentifiers.EnterpriseEvents.inventoryHealthAssessmentPublished.Id ] |> EnvelopeFilter.EventTypes
        let! healthQueryCtx: ProjectionContext<InventoryHealthAssessmentDto, InventoryHealthAssessmentId> =
            healthQueryService InventoryHealth.Projections.apply healthFilter healthInitial "InventoryHealth"

        // 2. Child Aggregate APIs
        let posApi = InventoryPosition.Capabilities.create positionRepo publishKnowledge positionDeps ports
        let polApi = InventoryPolicy.Capabilities.create policyRepo publishKnowledge policyGovernance policyDeps ports
        let healthApi = InventoryHealth.Capabilities.create healthRepo publishKnowledge healthPolicy healthDeps ports

        // 3. Parent Capability API
        let apis = Capabilities.create posApi polApi healthApi

        // 4. Background Workflows
        let wfDeps: ManageInventoryWorkflowDependencies =
            { Subscribe = positionDeps.Subscribe
              DispatchEnvelope = positionDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId
              DefaultLocationId = defaultLocationId
              DefaultPeriodId = defaultPeriodId }

        let! (sub1: IDisposable) = createPositionUpdateWorkflow wfDeps CancellationToken.None
        let! (sub2: IDisposable) = createPolicyUpdateWorkflow wfDeps CancellationToken.None
        let! (sub3: IDisposable) = createHealthAssessmentWorkflow wfDeps CancellationToken.None
        let! (sub4: IDisposable) = createNotificationNodeWorkflow wfDeps CancellationToken.None

        let dispose () =
            posQueryCtx.Dispose ()
            polQueryCtx.Dispose ()
            healthQueryCtx.Dispose ()
            sub1.Dispose ()
            sub2.Dispose ()
            sub3.Dispose ()
            sub4.Dispose ()

        return
            { Commands = apis
              PositionQueries = posQueryCtx.QueryService
              PolicyQueries = polQueryCtx.QueryService
              HealthQueries = healthQueryCtx.QueryService
              Dispose = dispose }
    }
