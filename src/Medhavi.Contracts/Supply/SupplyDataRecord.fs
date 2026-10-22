namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Governed supply transaction categories
type SupplyType =
    | Inventory
    | PurchaseOrder
    | ProductionOrder
    | Transfer
    | SupplierCommitment

/// SE-S-010 Supply Data Record Data Transfer Object
type SupplyDataRecordDto =
    { RecordId: string
      Item: string
      Location: string
      SupplyType: string
      Quantity: decimal
      UnitOfMeasure: string
      BatchIdentifier: string option
      OrderIdentifier: string option
      CommitmentIdentifier: string option
      SupplierIdentifier: string option
      ExpectedDeliveryDate: DateTimeOffset option
      BusinessTime: DateTimeOffset
      ObservationTime: DateTimeOffset
      SourceSystemProvenance: string
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to receive a new supply data record
type ReceiveSupplyDataReq =
    { RecordId: string
      Item: string
      Location: string
      SupplyType: string
      Quantity: decimal
      UnitOfMeasure: string
      BatchIdentifier: string option
      OrderIdentifier: string option
      CommitmentIdentifier: string option
      SupplierIdentifier: string option
      ExpectedDeliveryDate: DateTimeOffset option
      BusinessTime: DateTimeOffset
      ObservationTime: DateTimeOffset
      SourceSystemProvenance: string }

/// External request to evaluate an existing supply data record
type EvaluateSupplyDataReq =
    { RecordId: string
      EvaluationTime: DateTimeOffset }

/// Public API for Supply Data Record (SE-S-010)
type SupplyDataRecordApi =
    { Receive: ReceiveSupplyDataReq -> Task<Result<SupplyDataRecordDto, ApiError>>
      Evaluate: EvaluateSupplyDataReq -> Task<Result<SupplyDataRecordDto, ApiError>> }

/// Query service alias
type SupplyDataRecordQueries = QueryService<SupplyDataRecordDto, string>
