/// CA-P-001 Ingest and Understand Orders Composition Root
module Medhavi.Promise.UnderstandOrders.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Model
open Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Policies
open Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Model
open Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Policies
open Medhavi.Promise.UnderstandOrders.Workflows
open Capabilities

/// Public context exposing commands, queries, and lifecycle management for Understand Orders
type UnderstandOrdersContext =
    { Commands: UnderstandOrdersApis
      OrderBookQueries: QueryService<OrderBookSnapshotDto, OrderBookSnapshotId>
      CustomerProfileQueries: QueryService<CustomerPromiseProfileDto, CustomerId>
      Dispose: unit -> unit }

/// Creates the complete Understand Orders context with all child aggregates, projections, and workflows wired
let create
    (orderBookRepo: Repository<OrderBookSnapshot, OrderBookSnapshotId, OrderBookEvent>)
    (orderBookDeps: EnvelopeStoreDependencies<OrderBookEvent>)
    (orderBookQueryService: CreateQueryService<OrderBookEvent, OrderBookSnapshotDto, OrderBookSnapshotId>)
    (orderValidationPolicy: OrderValidationPolicy)
    (orderLifecyclePolicy: OrderLifecyclePolicy)
    (customerProfileRepo: Repository<CustomerPromiseProfile, CustomerId, CustomerProfileEvent>)
    (customerProfileDeps: EnvelopeStoreDependencies<CustomerProfileEvent>)
    (customerProfileQueryService: CreateQueryService<CustomerProfileEvent, CustomerPromiseProfileDto, CustomerId>)
    (customerProfilePolicy: CustomerProfilePolicy)
    (publishKnowledge: KnowledgeRepresentation)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<UnderstandOrdersContext, ApplicationError> =
    taskResult {

        // 1. Initialize OrderBookSnapshot projection
        let! orderBookAggregates: OrderBookSnapshot list =
            orderBookRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let orderBookInitialState = OrderBookSnapshot.Projections.seedFromAggregates orderBookAggregates

        let orderBookValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.orderBookIngested.Id
              ArsIdentifiers.EnterpriseEvents.orderValidationPassed.Id
              ArsIdentifiers.EnterpriseEvents.orderValidationFailed.Id
              ArsIdentifiers.EnterpriseEvents.orderLineCancelled.Id ]
            |> EnvelopeFilter.EventTypes

        let! orderBookQueryCtx: ProjectionContext<OrderBookSnapshotDto, OrderBookSnapshotId> =
            orderBookQueryService OrderBookSnapshot.Projections.apply orderBookValidEvents orderBookInitialState "OrderBookSnapshot"

        // 2. Initialize CustomerPromiseProfile projection
        let! profileAggregates: CustomerPromiseProfile list =
            customerProfileRepo.GetAll ()
            |> TaskResult.ofTask
            |> TaskResult.mapError (fun repoErr -> Infrastructure(Database(sprintf "%A" repoErr)))

        let profileInitialState = CustomerPromiseProfile.Projections.seedFromAggregates profileAggregates

        let profileValidEvents =
            [ ArsIdentifiers.EnterpriseEvents.customerPromiseProfileConfigured.Id
              ArsIdentifiers.EnterpriseEvents.customerServiceTermsUpdated.Id ]
            |> EnvelopeFilter.EventTypes

        let! profileQueryCtx: ProjectionContext<CustomerPromiseProfileDto, CustomerId> =
            customerProfileQueryService CustomerPromiseProfile.Projections.apply profileValidEvents profileInitialState "CustomerPromiseProfile"

        // 3. Instantiate Child Aggregate APIs
        let orderBookAggApi =
            OrderBookSnapshot.Capabilities.create
                orderBookRepo
                publishKnowledge
                orderValidationPolicy
                orderLifecyclePolicy
                orderBookDeps
                ports

        let profileAggApi =
            CustomerPromiseProfile.Capabilities.create
                customerProfileRepo
                publishKnowledge
                customerProfilePolicy
                customerProfileDeps
                ports

        // 4. Instantiate Parent Capability API
        let apis = Capabilities.create orderBookAggApi profileAggApi

        // 5. Wire workflow dependencies
        let workflowDeps: UnderstandOrdersWorkflowDependencies =
            { SnapshotCodec = Medhavi.Foundation.Codec.json
              ProfileCodec = Medhavi.Foundation.Codec.json
              Subscribe = orderBookDeps.Subscribe
              DispatchEnvelope = orderBookDeps.DispatchEnvelope
              Apis = apis
              DefaultScopeId = defaultScopeId }

        let! orderValSub: IDisposable =
            createOrderValidationWorkflow workflowDeps CancellationToken.None

        // 6. Combine lifecycle disposables
        let dispose () =
            orderBookQueryCtx.Dispose ()
            profileQueryCtx.Dispose ()
            orderValSub.Dispose ()

        return
            { Commands = apis
              OrderBookQueries = orderBookQueryCtx.QueryService
              CustomerProfileQueries = profileQueryCtx.QueryService
              Dispose = dispose }
    }
