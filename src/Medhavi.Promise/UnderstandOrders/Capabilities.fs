/// CA-P-001 Ingest and Understand Orders — Capability Parent API
/// Traces to: CR-P-001, CR-P-002, CR-P-003, CR-P-004, FS-P-001, FS-P-002, FS-P-003, FS-P-004
module Medhavi.Promise.UnderstandOrders.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts
open Medhavi.Contracts.Promise
open Medhavi.Promise.AclHelpers

type UnderstandOrdersApis =
    { OrderBook: OrderBookApi
      CustomerProfiles: CustomerPromiseProfileApi }

let create
    (orderBookAggApi: OrderBookSnapshot.Capabilities.OrderBookAggregateApi)
    (profileAggApi: CustomerPromiseProfile.Capabilities.CustomerPromiseProfileAggregateApi)
    : UnderstandOrdersApis =

    let ingestOrder (req: IngestOrderRequest) : Task<Result<OrderBookSnapshotDto, ApiError>> =
        orderBookAggApi.Ingest req
        |> TaskResult.map OrderBookSnapshot.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let validateOrder (req: ValidateOrderLineReq) : Task<Result<OrderBookSnapshotDto, ApiError>> =
        orderBookAggApi.Validate req
        |> TaskResult.map OrderBookSnapshot.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let cancelOrder (req: CancelOrderLineReq) : Task<Result<OrderBookSnapshotDto, ApiError>> =
        orderBookAggApi.Cancel req
        |> TaskResult.map OrderBookSnapshot.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let orderBookApi: OrderBookApi =
        { Ingest = ingestOrder
          Validate = validateOrder
          Cancel = cancelOrder }

    let configureProfile (req: ConfigureCustomerPromiseProfileReq) : Task<Result<CustomerPromiseProfileDto, ApiError>> =
        profileAggApi.Configure req
        |> TaskResult.map CustomerPromiseProfile.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let updateProfileTerms (req: UpdateCustomerServiceTermsReq) : Task<Result<CustomerPromiseProfileDto, ApiError>> =
        profileAggApi.UpdateTerms req
        |> TaskResult.map CustomerPromiseProfile.Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let profileApi: CustomerPromiseProfileApi =
        { Configure = configureProfile
          UpdateTerms = updateProfileTerms }

    { OrderBook = orderBookApi
      CustomerProfiles = profileApi }

