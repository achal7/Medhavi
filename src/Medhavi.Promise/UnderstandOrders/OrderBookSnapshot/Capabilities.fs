/// CA-P-001 Ingest and Understand Orders Aggregate Capabilities
module Medhavi.Promise.UnderstandOrders.OrderBookSnapshot.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model
open Policies

type OrderBookAggregateApi =
    { Ingest: IngestOrderRequest -> Task<Result<OrderBookSnapshot, ApplicationError>>
      Validate: ValidateOrderLineReq -> Task<Result<OrderBookSnapshot, ApplicationError>>
      Cancel: CancelOrderLineReq -> Task<Result<OrderBookSnapshot, ApplicationError>> }

let create
    (repo: Repository<OrderBookSnapshot, OrderBookSnapshotId, OrderBookEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (validationPolicy: OrderValidationPolicy)
    (lifecyclePolicy: OrderLifecyclePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<OrderBookEvent>)
    (ports: PromisePorts)
    : OrderBookAggregateApi =

    let ingest (req: IngestOrderRequest) : Task<Result<OrderBookSnapshot, ApplicationError>> =
        taskResult {
            let! cmd: IngestOrderRequestCmd = liftValidation (ACL.toIngestCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value
            do! requireEntityExists ports.CustomerExists cmd.CustomerId "Customer" CustomerId.value

            let pipeline = CommandPipeline.create repo (fun (c: IngestOrderRequestCmd) -> c.SnapshotId) Behaviors.ingest deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let validate (req: ValidateOrderLineReq) : Task<Result<OrderBookSnapshot, ApplicationError>> =
        taskResult {
            let! cmd: ValidateOrderTermsCmd = liftValidation (ACL.toValidateCmd req)

            let! (snapshot: OrderBookSnapshot) =
                repo.Get cmd.SnapshotId
                |> TaskResult.mapError Repository.mapRepositoryErrorToApplicationError
                |> TaskResult.ofOption (DomainError.notFound ("OrderBookSnapshot", OrderBookSnapshotId.value cmd.SnapshotId) |> ApplicationError.fromDomainError)

            let targetLine = snapshot.Lines |> List.tryFind (fun l -> l.OrderLineId = cmd.OrderLineId)

            let! itemExists =
                match targetLine with
                | Some l -> protect (ports.ItemExists l.Item)
                | None -> TaskResult.return' false

            let! locationExists =
                match targetLine with
                | Some l -> protect (ports.LocationExists l.DestinationLocation)
                | None -> TaskResult.return' false

            let! customerProfile = protect (ports.GetCustomerProfileFact snapshot.CustomerId)

            let decideFn = Behaviors.validate validationPolicy itemExists locationExists customerProfile
            let pipeline = CommandPipeline.create repo (fun (c: ValidateOrderTermsCmd) -> c.SnapshotId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let cancel (req: CancelOrderLineReq) : Task<Result<OrderBookSnapshot, ApplicationError>> =
        taskResult {
            let! cmd: CancelOrderLineCmd = liftValidation (ACL.toCancelCmd req)
            let decideFn = Behaviors.cancel lifecyclePolicy
            let pipeline = CommandPipeline.create repo (fun (c: CancelOrderLineCmd) -> c.SnapshotId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Ingest = ingest
      Validate = validate
      Cancel = cancel }
