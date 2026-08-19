namespace Medhavi.Contracts.Core.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-C-013 Demand Data Transfer Object
type DemandDto =
    { Id: string
      Item: string
      Location: string
      Customer: string option
      Quantity: decimal
      NeedWindowLatest: DateTimeOffset
      NeedWindowEarliest: DateTimeOffset option
      NeedWindowPreferred: DateTimeOffset option
      DemandOrigin: string
      ParentDemand: string option
      LifecycleState: string }

/// Request to record a new demand fact
type RecordDemandReq =
    { DemandId: string
      Item: string
      Location: string
      Customer: string option
      Quantity: decimal
      NeedWindowLatest: DateTimeOffset
      NeedWindowEarliest: DateTimeOffset option
      NeedWindowPreferred: DateTimeOffset option
      DemandOrigin: string
      ParentDemand: string option }

/// Request to satisfy an existing demand fact
type SatisfyDemandReq =
    { DemandId: string
      SatisfactionTime: DateTimeOffset }

/// Request to cancel an existing demand fact
type CancelDemandReq =
    { DemandId: string
      CancellationTime: DateTimeOffset
      Reason: string }

/// Public API for Enterprise Demand Management (CA-C-022)
type DemandApi =
    { Record: RecordDemandReq -> Task<Result<DemandDto, ApiError>>
      Satisfy: SatisfyDemandReq -> Task<Result<DemandDto, ApiError>>
      Cancel: CancelDemandReq -> Task<Result<DemandDto, ApiError>> }

/// Query service for Demand read model
type DemandQueries = QueryService<DemandDto, string>

/// Data point consumed by Demand Intelligence for forecasting baselines
/// Note: Uses NeedWindowLatest as the primary time reference for the demand event.
type DemandDataPoint =
    { Item: string
      Location: string
      Quantity: decimal
      NeedWindowLatest: DateTimeOffset
      DemandOrigin: string }
