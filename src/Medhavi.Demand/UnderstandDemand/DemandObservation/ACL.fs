module Medhavi.Demand.UnderstandDemand.DemandObservation.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

let private invalid (message: string) : Validation<'T, DomainError> = Invalid [ DomainError.validation message ]

/// Translates ReceiveObservationReq into domain command using Applicative validation.
let toReceiveCmd (req: ReceiveObservationReq) : Validation<ReceiveObservationCmd, DomainError> =
    let validateObsId = DemandObservationId.create req.ObservationId |> fromResult

    let validateQty =
        UnitOfMeasureId.create req.UnitOfMeasure
        |> Result.bind(fun uom -> Quantity.createWithUoM req.Quantity uom)
        |> Result.mapError(fun e -> DomainError.validation $"Invalid Quantity: {e}")
        |> fromResult

    let validateObsType =
        VocabularyEntryId.create req.ObservationType |> Result.mapError mapSemanticValidationToDomainError |> fromResult

    let validateBizTime = Timestamp.create req.BusinessTime |> Result.mapError DomainError.validation |> fromResult

    let validateObsTime = Timestamp.create req.ObservationTime |> Result.mapError DomainError.validation |> fromResult

    let create obsId item loc qty obsType bizTime obsTime source =
        { ObservationId = obsId
          Item = item
          Location = loc
          Quantity = qty
          ObservationType = obsType
          BusinessTime = bizTime
          ObservationTime = obsTime
          SourceSystemProvenance = source }

    create <!> validateObsId
    <*> validateItemId req.Item
    <*> validateLocationId req.Location
    <*> validateQty
    <*> validateObsType
    <*> validateBizTime
    <*> validateObsTime
    <*> (Valid req.SourceSystemProvenance)

/// Translates EvaluateObservationReq into domain command.
let toEvaluateCmd (req: EvaluateObservationReq) : Validation<EvaluateObservationCmd, DomainError> =
    let validateObsId = DemandObservationId.create req.ObservationId |> fromResult

    let create obsId evalTime =
        { ObservationId = obsId
          EvaluationTime = evalTime }

    create <!> validateObsId <*> validateTimestamp req.EvaluationTime
