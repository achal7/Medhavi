module Medhavi.Demand.DemandObservation.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Demand.DemandObservation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.Model

let mapToObservationTypeAgg (otype: DemandObservation.ObservationType) : Model.ObservationType =
    match otype with
    | DemandObservation.SalesOrder -> Model.SalesOrder
    | DemandObservation.Shipment -> Model.Shipment
    | DemandObservation.POS -> Model.POS
    | DemandObservation.Return -> Model.Return
    | DemandObservation.Correction -> Model.Correction
    | DemandObservation.Signal -> Model.Signal

let mapToObservationTypeContract (otype: Model.ObservationType) : DemandObservation.ObservationType =
    match otype with
    | Model.SalesOrder -> DemandObservation.SalesOrder
    | Model.Shipment -> DemandObservation.Shipment
    | Model.POS -> DemandObservation.POS
    | Model.Return -> DemandObservation.Return
    | Model.Correction -> DemandObservation.Correction
    | Model.Signal -> DemandObservation.Signal

let mapToStatusContract (s: Model.ObservationStatus) : DemandObservation.ObservationStatus =
    match s with
    | Received -> DemandObservation.Received
    | Accepted -> DemandObservation.Accepted
    | Quarantined -> DemandObservation.Quarantined
    | Rejected -> DemandObservation.Rejected

let private notEmpty field =
    validate (fun s -> not (System.String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let toAssignScopeCmd (req: AssignScopeReq) : Validation<AssignScopeCmd, DomainError> =
    let make obsId scopeId =
        { ObservationId = obsId
          PlanningScopeId = scopeId }

    make <!> (DemandObservationId.create req.ObservationId |> fromResult)
    <*> (PlanningScopeId.fromString req.PlanningScopeId |> fromResult)

let toEstablishCmd (req: EstablishObservationReq) : Validation<EstablishObservationCmd, DomainError> =
    let obsType = mapToObservationTypeAgg req.ObservationType

    let make obsId skuId spId qty sourceSystem =
        let provenance: Provenance =
            { SourceSystem = sourceSystem
              ExternalRef = req.ExternalRef
              MessageId = req.MessageId
              Revision = Revision.createClamp req.Revision
              ScenarioId = None }

        { ObservationId = obsId
          SkuId = skuId
          StockingPointId = spId
          Quantity = qty
          ObservationType = obsType
          BusinessTime = Timestamp.create req.BusinessTime
          CustomerId = req.CustomerId |> Option.map CustomerId
          PromotionRef = req.PromotionRef
          CampaignRef = req.CampaignRef
          ContractRef = req.ContractRef
          Provenance = provenance }

    make <!> (DemandObservationId.create req.ObservationId |> fromResult)
    <*> (SkuId.create req.SkuId |> fromResult)
    <*> (StockingPointId.create req.StockingPointId |> fromResult)
    <*> (Quantity.create req.Quantity |> fromResult)
    <*> notEmpty "SourceSystem" req.SourceSystem

let toEstablishBatchCmd (req: EstablishObservationBatchReq) : Validation<EstablishObservationCmd list, DomainError> =
    req.Ingestions |> List.map toEstablishCmd |> sequence

let toEvaluateCmd (req: EvaluateObservationReq) : Validation<EvaluateObservationCmd, DomainError> =
    let signal =
        req.SignalId
        |> Option.map(fun id ->
            { SignalId = id
              Source = req.SignalSource |> Option.defaultValue "unknown"
              SourceReliability = req.SourceReliability |> Option.defaultValue 0.0M
              Timestamp = req.SignalTimestamp |> Option.defaultValue DateTimeOffset.MinValue
              Value = req.SignalValue |> Option.defaultValue 0m
              StatisticalBound = req.StatisticalBound |> Option.defaultValue 0m
              RecentBaseline = req.RecentBaseline |> Option.defaultValue 0m }
            : DemandSignal)

    let make obsId =
        { ObservationId = obsId
          Signal = signal }
        : EvaluateObservationCmd

    make <!> (DemandObservationId.create req.ObservationId |> fromResult)
