namespace Medhavi.Contracts.Demand.Edp

open System
open System.Threading.Tasks
open Medhavi.Contracts

type EnterpriseDemandPicture =
    { PlanningScopeId: string
      Version: int
      Status: string
      Periods: EnterpriseDemandPicturePeriod list
      TransactionTime: DateTimeOffset
      PublicationTime: DateTimeOffset option }

and EnterpriseDemandPicturePeriod =
    { Period: PlanningPeriod
      OperationalDemand: decimal
      Adjustment: decimal
      Override: decimal
      FinalQuantity: decimal }

type ReviseEnterpriseDemandPictureReq =
    { PlanningScopeId: string
      Period: PlanningPeriod
      Quantity: decimal
      ObservationId: string }

type CalculateEnterpriseDemandPictureReq = { PlanningScopeId: string }

type PublishEnterpriseDemandPictureReq = { PlanningScopeId: string }

type EnterpriseDemandPicturePublishedNotification =
    { PlanningScopeId: string
      Version: int
      PublicationTime: DateTimeOffset }

type EnterpriseDemandPictureRecalculationFailedNotification =
    { PlanningScopeId: string
      Reason: string }

type EnterpriseDemandPictureQueries = QueryService<EnterpriseDemandPicture, string>

type EnterpriseDemandPictureApi =
    { Revise: ReviseEnterpriseDemandPictureReq -> Task<Result<string * int, ApiError>>
      Calculate: CalculateEnterpriseDemandPictureReq -> Task<Result<string * int, ApiError>>
      Publish: PublishEnterpriseDemandPictureReq -> Task<Result<string * int, ApiError>> }
