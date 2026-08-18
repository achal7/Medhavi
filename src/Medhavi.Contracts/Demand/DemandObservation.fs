namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-D-001 Demand Observation Data Transfer Object
type DemandObservationDto =
    { ObservationId: string
      Item: string
      Location: string
      Quantity: decimal
      UnitOfMeasure: string
      ObservationType: string
      BusinessTime: DateTimeOffset
      ObservationTime: DateTimeOffset
      SourceSystemProvenance: string
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to receive a new demand observation
type ReceiveObservationReq =
    { ObservationId: string
      Item: string
      Location: string
      Quantity: decimal
      UnitOfMeasure: string
      ObservationType: string
      BusinessTime: DateTimeOffset
      ObservationTime: DateTimeOffset
      SourceSystemProvenance: string }

/// External request to evaluate an existing demand observation
type EvaluateObservationReq =
    { ObservationId: string
      EvaluationTime: DateTimeOffset }

/// Public API for Demand Observation (SE-D-001)
type DemandObservationApi =
    { Receive: ReceiveObservationReq -> Task<Result<DemandObservationDto, ApiError>>
      Evaluate: EvaluateObservationReq -> Task<Result<DemandObservationDto, ApiError>> }

/// Query service alias
type DemandObservationQueries = QueryService<DemandObservationDto, string>
