/// CA-C-021 Anti-Corruption Layer
module Medhavi.Core.ItemTransitionManagement.ACL

open Medhavi.SemanticModel
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.IdsFactory
open Medhavi.Contracts.Core.ItemTransition
open Model

/// BR-C-013: TransitionId is deterministically derived from business identity.
let deriveTransitionId (supersededItemId: string) (supersedingItemId: string) : string =
    generalDeterministicId "itemTransition" [ supersededItemId; supersedingItemId ]

/// Translates recognition request into domain command.
let toRecognizeCmd (req: RecognizeItemTransitionReq) : Validation<RecognizeItemTransitionCmd, DomainError> =
    let validateTransitionId =
        let derived = deriveTransitionId req.SupersededItem req.SupersedingItem

        TransitionId.create derived
        |> Result.mapError(fun err -> DomainError.validation $"TransitionId: {err}")
        |> fromResult

    let validateSupersededItem =
        ItemId.create req.SupersededItem
        |> Result.mapError(fun err -> DomainError.validation $"SupersededItem: {err}")
        |> fromResult

    let validateSupersedingItem =
        ItemId.create req.SupersedingItem
        |> Result.mapError(fun err -> DomainError.validation $"SupersedingItem: {err}")
        |> fromResult

    let validateTransitionType =
        VocabularyEntryId.create req.TransitionType
        |> Result.mapError(fun err -> DomainError.validation $"TransitionType: {err}")
        |> fromResult

    let validateEffectiveDate =
        Timestamp.create req.EffectiveDate |> Result.mapError DomainError.validation |> fromResult

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

    create <!> validateTransitionId
    <*> validateSupersededItem
    <*> validateSupersedingItem
    <*> validateTransitionType
    <*> validateEffectiveDate
    <*> validateEndDate

/// Translates suspension request into domain command.
let toSuspendCmd (req: SuspendItemTransitionReq) : Validation<SuspendItemTransitionCmd, DomainError> =
    let validateTransitionId =
        let derived = deriveTransitionId req.SupersededItem req.SupersedingItem

        TransitionId.create derived
        |> Result.mapError(fun err -> DomainError.validation $"TransitionId: {err}")
        |> fromResult

    let validateSuspensionTime =
        Timestamp.create req.SuspensionTime |> Result.mapError DomainError.validation |> fromResult

    let create transitionId suspensionTime =
        { TransitionId = transitionId
          SuspensionTime = suspensionTime }

    create <!> validateTransitionId <*> validateSuspensionTime

/// Translates reinstatement request into domain command.
let toReinstateCmd (req: ReinstateItemTransitionReq) : Validation<ReinstateItemTransitionCmd, DomainError> =
    let validateTransitionId =
        let derived = deriveTransitionId req.SupersededItem req.SupersedingItem

        TransitionId.create derived
        |> Result.mapError(fun err -> DomainError.validation $"TransitionId: {err}")
        |> fromResult

    let validateReinstatementTime =
        Timestamp.create req.ReinstatementTime |> Result.mapError DomainError.validation |> fromResult

    let create transitionId reinstatementTime =
        { TransitionId = transitionId
          ReinstatementTime = reinstatementTime }

    create <!> validateTransitionId <*> validateReinstatementTime

/// Translates retirement request into domain command.
let toRetireCmd (req: RetireItemTransitionReq) : Validation<RetireItemTransitionCmd, DomainError> =
    let validateTransitionId =
        let derived = deriveTransitionId req.SupersededItem req.SupersedingItem

        TransitionId.create derived
        |> Result.mapError(fun err -> DomainError.validation $"TransitionId: {err}")
        |> fromResult

    let validateRetirementTime =
        Timestamp.create req.RetirementTime |> Result.mapError DomainError.validation |> fromResult

    let create transitionId retirementTime =
        { TransitionId = transitionId
          RetirementTime = retirementTime }

    create <!> validateTransitionId <*> validateRetirementTime
