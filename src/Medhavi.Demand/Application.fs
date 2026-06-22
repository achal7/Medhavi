module Medhavi.Demand.Application

open System
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Demand.Domain
open Medhavi.Demand.Domain.DemandLineAgg
open Medhavi.Contracts.Demand

module ACL =
    let toIngestCommand (req: DemandDefineReq) : Validation<IngestDemandLineCmd, DomainError> =
        let cat =
            match req.DemandCategory.ToLower() with
            | "customerorderdemand"
            | "customerorder" -> Medhavi.Demand.Domain.DemandCategory.CustomerOrderDemand
            | "salesorderforecast"
            | "forecast" -> Medhavi.Demand.Domain.DemandCategory.SalesOrderForecast
            | "interplanttransfer"
            | "transfer" -> Medhavi.Demand.Domain.DemandCategory.InterplantTransfer
            | "servicepart"
            | "sparepart" -> Medhavi.Demand.Domain.DemandCategory.ServicePart
            | "internalconsumption"
            | "consumption" -> Medhavi.Demand.Domain.DemandCategory.InternalConsumption
            | "dependentdemand"
            | "dependent" -> Medhavi.Demand.Domain.DemandCategory.DependentDemand
            | _ -> Medhavi.Demand.Domain.DemandCategory.CustomerOrderDemand

        let priorityVal =
            match req.Priority with
            | 1 -> DemandPriority.Critical
            | 2 -> DemandPriority.High
            | 3 -> DemandPriority.Normal
            | _ -> DemandPriority.Low

        let make (skuId: SkuId) (spId: StockingPointId) (qty: Quantity) : IngestDemandLineCmd =
            { DemandLineId = req.DemandLineId
              DemandOrderId = req.DemandOrderId
              SkuId = skuId
              StockingPointId = spId
              CustomerId = req.CustomerId
              Quantity = qty
              UnitOfMeasure = req.UnitOfMeasure
              OrderDate = Timestamp.create req.OrderDate
              EarliestDeliveryDate = req.EarliestDeliveryDate |> Option.map Timestamp.create
              RequestedDeliveryDate = Timestamp.create req.RequestedDeliveryDate
              LatestDeliveryDate = req.LatestDeliveryDate |> Option.map Timestamp.create
              Priority = priorityVal
              DemandCategory = cat
              IsFirm = req.IsFirm
              IsFrozen = req.IsFrozen
              Provenance =
                { SourceSystem = "ERP_Ingest"
                  ExternalRef = req.DemandOrderId
                  MessageId = Guid.NewGuid().ToString()
                  Revision = Revision.initial
                  ScenarioId = None } }

        make <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)

    let toFulfillCommand (req: FulfillDemandLineReq) : Validation<RecordExecutionFulfillmentCmd, DomainError> =
        let make qty : RecordExecutionFulfillmentCmd =
            { DemandLineId = req.DemandLineId
              Quantity = qty
              ActualDeliveryDate = Timestamp.now }

        make <!> (Quantity.create req.Quantity |> fromResult)

    let toPromiseCommand (req: PromiseDemandReq) : Validation<PromiseDemandLineCmd, DomainError> =
        let rec convertPegs
            (items: PeggedSupplySummary list)
            (acc: PeggedSupply list)
            : Validation<PeggedSupply list, DomainError> =
            match items with
            | [] -> Valid(List.rev acc)
            | p: PeggedSupplySummary :: rest ->
                match Quantity.create p.Quantity with
                | Error err -> Invalid [ err ]
                | Ok qty ->
                    let (peg: PeggedSupply) =
                        { SupplyOrderId = p.SupplyOrderId
                          SupplyType = p.SupplyType
                          Quantity = qty
                          PlannedDate = DateTimeOffset(p.PlannedDate.ToDateTime(TimeOnly.MinValue)) }

                    convertPegs rest (peg :: acc)

        let make cmdQty : PromiseDemandLineCmd =
            { DemandLineId = req.DemandLineId
              PromisedDate = Timestamp.create req.PromisedDate
              ConfirmedQty = cmdQty }

        make <!> (Quantity.create req.ConfirmedQty |> fromResult)

    let toFreezeCommand (req: FreezeDemandReq) : Validation<FreezeDemandLineCmd, DomainError> =
        Valid
            { DemandLineId = req.DemandLineId
              FrozenUntilUtc = Timestamp.create req.FrozenUntilUtc }

    let toReleaseCommand (req: ReleaseDemandReq) : Validation<ReleaseDemandLineCmd, DomainError> =
        Valid
            { DemandLineId = req.DemandLineId
              ReleaseFromHold = req.ReleaseFromHold
              Unfreeze = req.Unfreeze }

    let toCancelCommand (req: CancelDemandReq) : Validation<CancelDemandLineCmd, DomainError> =
        Valid
            { DemandLineId = req.DemandLineId
              Reason = req.Reason
              CancelledAtUtc = Timestamp.create req.CancelledAtUtc
              ForceOverride = req.ForceOverride }

type Decision = Decision<Domain.DemandLine, DemandLineEvent>

// TODO - Open implementation for tracking
type Provenance =
    { SourceSystem: string
      ExternalRef: string
      MessageId: string

      CorrelationId: string option
      CausationId: string option

      Revision: Revision
      ScenarioId: ScenarioId option }

type DemandLineCapabilities =
    { Ingest: DemandDefineReq -> TaskResult<Decision, ApplicationError>
      Promise: PromiseDemandReq -> TaskResult<Decision, ApplicationError>
      Freeze: FreezeDemandReq -> TaskResult<Decision, ApplicationError>
      Release: ReleaseDemandReq -> TaskResult<Decision, ApplicationError>
      Cancel: CancelDemandReq -> TaskResult<Decision, ApplicationError>
      Fulfill: FulfillDemandLineReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Domain.DemandLine, string, DemandLineEvent>) =
    { Ingest =
        liftCmdValidation ACL.toIngestCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo IngestDemandLine decide

      Promise =
        liftCmdValidation ACL.toPromiseCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo PromiseDemandLine decide

      Freeze =
        liftCmdValidation ACL.toFreezeCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo FreezeDemandLine decide

      Release =
        liftCmdValidation ACL.toReleaseCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo ReleaseDemandLine decide

      Cancel =
        liftCmdValidation ACL.toCancelCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo CancelDemandLine decide

      Fulfill =
        liftCmdValidation ACL.toFulfillCommand
        >=> handleCommand (fun cmd -> cmd.DemandLineId) repo RecordExecutionFulfillment decide }

let createDemandLineApi (capabilities: DemandLineCapabilities) =
    { Define =
        fun req ->
            capabilities.Ingest req |> TaskResult.map(fun _ -> ()) |> TaskResult.mapError ApplicationError.mapToApiError
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Ingest
            |> TaskResult.sequence
            |> TaskResult.map(fun _ -> ())
            |> TaskResult.mapError ApplicationError.mapToApiError
      Fulfill =
        fun req ->
            capabilities.Fulfill req
            |> TaskResult.map(fun _ -> ())
            |> TaskResult.mapError ApplicationError.mapToApiError
      Promise =
        fun req ->
            capabilities.Promise req
            |> TaskResult.map(fun _ -> ())
            |> TaskResult.mapError ApplicationError.mapToApiError
      Freeze =
        fun req ->
            capabilities.Freeze req |> TaskResult.map(fun _ -> ()) |> TaskResult.mapError ApplicationError.mapToApiError
      Release =
        fun req ->
            capabilities.Release req
            |> TaskResult.map(fun _ -> ())
            |> TaskResult.mapError ApplicationError.mapToApiError
      Cancel =
        fun req ->
            capabilities.Cancel req |> TaskResult.map(fun _ -> ()) |> TaskResult.mapError ApplicationError.mapToApiError }
    : DemandLineApi
