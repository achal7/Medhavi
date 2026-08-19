/// CA-C-021 Anti-Corruption Layer
module Medhavi.Core.ItemTransitionManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.IdsFactory
open Medhavi.Contracts.Core.ItemTransition
open Model
open Medhavi.Core

/// BR-C-013: TransitionId is deterministically derived from business identity.
let validateTransitionId (supersededItemId: string) (supersedingItemId: string) =
    let derived = generalDeterministicId "itemTransition" [ supersededItemId; supersedingItemId ]

    TransitionId.create derived
    |> Result.mapError(fun err -> DomainError.validation $"TransitionId: {err}")
    |> fromResult

/// Translates recognition request into domain command.
let toRecognizeCmd (req: RecognizeItemTransitionReq) : Validation<RecognizeItemTransitionCmd, DomainError> =

    let validateTransitionType =
        VocabularyEntryId.create req.TransitionType
        |> Result.mapError(fun err -> DomainError.validation $"TransitionType: {err}")
        |> fromResult

    let validateEndDate =
        match req.EndDate with
        | None -> Valid None
        | Some d ->
            Timestamp.create d
            |> Result.mapError(fun err -> DomainError.validation $"EndDate: {err}")
            |> Result.map Some
            |> fromResult

    let create transitionId supersededItem supersedingItem transitionType effectiveDate endDate =
        { TransitionId = transitionId
          SupersededItem = supersededItem
          SupersedingItem = supersedingItem
          TransitionType = transitionType
          EffectiveDate = effectiveDate
          EndDate = endDate }

    create <!> validateTransitionId req.SupersededItem req.SupersedingItem
    <*> validateItemId req.SupersededItem
    <*> validateItemId req.SupersedingItem
    <*> validateTransitionType
    <*> validateTimestamp req.EffectiveDate
    <*> validateEndDate

/// Translates suspension request into domain command.
let toSuspendCmd (req: SuspendItemTransitionReq) : Validation<SuspendItemTransitionCmd, DomainError> =

    let create transitionId suspensionTime =
        { TransitionId = transitionId
          SuspensionTime = suspensionTime }

    create <!> validateTransitionId req.SupersededItem req.SupersedingItem <*> validateTimestamp req.SuspensionTime

/// Translates reinstatement request into domain command.
let toReinstateCmd (req: ReinstateItemTransitionReq) : Validation<ReinstateItemTransitionCmd, DomainError> =

    let create transitionId reinstatementTime =
        { TransitionId = transitionId
          ReinstatementTime = reinstatementTime }

    create <!> validateTransitionId req.SupersededItem req.SupersedingItem <*> validateTimestamp req.ReinstatementTime

/// Translates retirement request into domain command.
let toRetireCmd (req: RetireItemTransitionReq) : Validation<RetireItemTransitionCmd, DomainError> =
    let create transitionId retirementTime =
        { TransitionId = transitionId
          RetirementTime = retirementTime }

    create <!> validateTransitionId req.SupersededItem req.SupersedingItem <*> validateTimestamp req.RetirementTime
