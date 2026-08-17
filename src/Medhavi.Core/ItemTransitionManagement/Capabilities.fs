/// CA-C-021 Item Transition Management Capabilities
module Medhavi.Core.ItemTransitionManagement.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Core
open Medhavi.Core.ArsIdentifiers
open Model
open Policies

/// Creates the public API for Item Transition Management.
let create
    (repo: Repository<ItemTransition, TransitionId, ItemTransitionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ItemTransitionPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<ItemTransitionEvent>)
    (getItemState: ItemId -> Task<ReferenceLifecycleState option>)
    (getActiveTransitionForItem: ItemId -> Task<ItemTransitionDto option>)
    : ItemTransitionApi =

    let decider = Behaviors.decide policy
    let pipeline = CommandPipeline.create repo ItemTransitionCmd.GetId decider deps

    /// FIRST GATE: BR-C-014, BR-C-015, BR-C-016.
    /// Validates item states and conflicting transitions before pipeline execution.
    let validateRecognitionPreConditions (cmd: RecognizeItemTransitionCmd) : Task<Result<unit, ApiError>> =
        task {
            let! supersededState = getItemState cmd.SupersededItem
            let! supersedingState = getItemState cmd.SupersedingItem
            let! existingTransition = getActiveTransitionForItem cmd.SupersededItem

            match supersededState with
            | None -> return Error(ApiError.notFound "Item" (Identities.itemIdValue cmd.SupersededItem))
            | Some ReferenceLifecycleState.Retired ->
                return Error(ApiError.businessRule $"{Rules.supersededItemValidity.Id}: Superseded Item is Retired")
            | Some _ ->
                match supersedingState with
                | None -> return Error(ApiError.notFound "Item" (Identities.itemIdValue cmd.SupersedingItem))
                | Some ReferenceLifecycleState.Active ->
                    match existingTransition with
                    | Some _ ->
                        return
                            Error(
                                ApiError.conflict
                                    $"{Rules.singleActiveTransitionPerItem.Id}: Active transition already exists"
                            )
                    | None -> return Ok()
                | Some _ ->
                    return
                        Error(
                            ApiError.businessRule $"{Rules.supersedingItemValidity.Id}: Superseding Item must be Active"
                        )
        }

    { Recognize =
        fun req ->
            task {
                match ACL.toRecognizeCmd req with
                | Invalid errs ->
                    return
                        Error(
                            DomainError.combineValidationErrors errs
                            |> ApplicationError.fromDomainError
                            |> mapAppErrorToApiError
                        )
                | Valid cmd ->
                    let! preConditionResult = validateRecognitionPreConditions cmd

                    match preConditionResult with
                    | Error apiErr -> return Error apiErr
                    | Ok() ->
                        return!
                            CommandCapabilities.runCapability
                                (fun c -> Valid(ItemTransitionCmd.Recognize c))
                                pipeline
                                publishKnowledge
                                Projections.mapToDto
                                mapAppErrorToApiError
                                cmd
            }
      Suspend =
        let cmd = ACL.toSuspendCmd >> Validation.map ItemTransitionCmd.Suspend

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projections.mapToDto
                mapAppErrorToApiError
                req
      Reinstate =
        let cmd = ACL.toReinstateCmd >> Validation.map ItemTransitionCmd.Reinstate

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projections.mapToDto
                mapAppErrorToApiError
                req
      Retire =
        let cmd = ACL.toRetireCmd >> Validation.map ItemTransitionCmd.Retire

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projections.mapToDto
                mapAppErrorToApiError
                req }
