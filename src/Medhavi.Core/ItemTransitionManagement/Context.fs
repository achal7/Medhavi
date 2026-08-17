/// CA-C-021 Item Transition Management Composition Root
module Medhavi.Core.ItemTransitionManagement.Context

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.SemanticModel
open Medhavi.Core
open Model
open Policies
open Projections

/// Public context exposing commands and queries.
type ItemTransitionManagementContext =
    { Commands: ItemTransitionApi
      Queries: QueryService<ItemTransitionDto, TransitionId>
      Dispose: unit -> unit }

/// Creates the complete context with all dependencies wired.
let create
    (repo: Repository<ItemTransition, TransitionId, ItemTransitionEvent>)
    (deps: EnvelopeStoreDependencies<ItemTransitionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (createQueryService: CreateQueryService<ItemTransitionEvent, ItemTransitionDto, TransitionId>)
    (policy: ItemTransitionPolicy)
    (getItemState: ItemId -> Task<ReferenceLifecycleState option>)
    : TaskResult<ItemTransitionManagementContext, ApplicationError> =
    taskResult {

        // 1. Projection
        let! aggregates =
            repo.GetAll()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let state = seedFromAggregates aggregates

        let validEventTypes =
            [ ArsIdentifiers.EnterpriseEvents.itemTransitionRecognized.Id
              ArsIdentifiers.EnterpriseEvents.itemTransitionSuspended.Id
              ArsIdentifiers.EnterpriseEvents.itemTransitionReinstated.Id
              ArsIdentifiers.EnterpriseEvents.itemTransitionRetired.Id ]
            |> EnvelopeFilter.EventTypes

        let! queryCtx : ProjectionContext<ItemTransitionDto, TransitionId> =
            createQueryService Projections.apply validEventTypes state "ItemTransition"

        // 2. Helper to query active transition for a given Superseded Item (BR-C-016 FIRST GATE)
        let getActiveTransitionForItem (itemId: ItemId) : Task<ItemTransitionDto option> =
            task {
                let itemIdStr = Identities.itemIdValue itemId
                let! transitions =
                    queryCtx.QueryService.Filter (fun dto ->
                        dto.SupersededItem = itemIdStr && dto.LifecycleState = "Active")
                return transitions |> List.tryHead
            }

        // 3. Create capabilities
        let commands =
            Capabilities.create repo publishKnowledge policy deps getItemState getActiveTransitionForItem

        // 4. No workflows needed — Item Transitions are triggered by internal stewardship API calls.

        // 5. Dispose
        let dispose () = queryCtx.Dispose()

        return
            { Commands = commands
              Queries = queryCtx.QueryService
              Dispose = dispose }
    }