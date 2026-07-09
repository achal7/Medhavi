module Medhavi.Contracts.Demand.DemandObservation

open System
open System.Threading.Tasks
open Medhavi.Contracts

type ObservationStatus =
    | Accepted
    | Received
    | Quarantined
    | Rejected

type ObservationType =
    | SalesOrder
    | Shipment
    | POS
    | Return
    | Correction
    | Signal

// ========== DemandObservation (read model) ==========

type DemandObservation =
    { Id: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      ObservationType: ObservationType
      BusinessTime: DateTimeOffset
      CustomerId: string option
      PromotionRef: string option
      CampaignRef: string option
      ContractRef: string option
      PlanningScopeId: string option
      Status: ObservationStatus
      DecisionRationale: string option
      Confidence: decimal option
      WarningCode: string option
      SourceSystem: string
      ExternalRef: string }

// ========== Commands (Requests) ==========
type EstablishObservationReq =
    { ObservationId: string
      SkuId: string
      StockingPointId: string
      Quantity: decimal
      UnitOfMeasure: string
      ObservationType: ObservationType
      BusinessTime: DateTimeOffset
      CustomerId: string option
      PromotionRef: string option
      CampaignRef: string option
      ContractRef: string option
      SourceSystem: string
      ExternalRef: string
      MessageId: string
      Revision: int }

type EvaluateObservationReq =
    { ObservationId: string
      SignalId: string option
      SignalSource: string option
      SourceReliability: decimal option
      SignalTimestamp: DateTimeOffset option
      SignalValue: decimal option
      StatisticalBound: decimal option
      RecentBaseline: decimal option }

type AssignScopeReq =
    { ObservationId: string
      PlanningScopeId: string }

type EstablishObservationBatchReq =
    { Ingestions: EstablishObservationReq list }

type ObservationReceivedNotification = { ObservationId: string }
type ObservationBatchReceivedNotification = { ObservationIds: string list }
type ObservationAcceptedNotification = { ObservationId: string }
type ObservationQuarantinedNotification = { ObservationId: string }
type ObservationRejectedNotification = { ObservationId: string }

type ObservationAcceptedWithWarningNotification =
    { ObservationId: string
      WarningCode: string }

type DemandObservationQuries = QueryService<DemandObservation, string>

type DemandObservationApi =
    { Receive: EstablishObservationReq -> Task<Result<string, ApiError>>
      ReceiveBatch: EstablishObservationBatchReq -> Task<Result<string list, ApiError>>
      Evaluate: EvaluateObservationReq -> Task<Result<string, ApiError>>
      AssignScope: AssignScopeReq -> Task<Result<string, ApiError>> }
