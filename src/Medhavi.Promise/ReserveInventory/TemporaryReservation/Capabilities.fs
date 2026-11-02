/// CA-P-003 Reserve Inventory and Capacity Temporarily Aggregate Capabilities
module Medhavi.Promise.ReserveInventory.TemporaryReservation.Capabilities

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

type ReservationAggregateApi =
    { CreateHold: CreateTemporaryHoldReq -> Task<Result<TemporaryReservation, ApplicationError>>
      ConfirmHold: ConfirmHoldToCommitmentReq -> Task<Result<TemporaryReservation, ApplicationError>>
      ReleaseHold: ReleaseTemporaryHoldReq -> Task<Result<TemporaryReservation, ApplicationError>>
      ExpireHold: ExpireTemporaryHoldReq -> Task<Result<TemporaryReservation, ApplicationError>> }

let create
    (repo: Repository<TemporaryReservation, TemporaryReservationId, TemporaryReservationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: TemporaryReservationPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<TemporaryReservationEvent>)
    (ports: PromisePorts)
    : ReservationAggregateApi =

    let createHold (req: CreateTemporaryHoldReq) : Task<Result<TemporaryReservation, ApplicationError>> =
        taskResult {
            let! cmd: CreateTemporaryHoldCmd = liftValidation (ACL.toCreateCmd req)

            do! requireEntityExists ports.CustomerExists cmd.CustomerId "Customer" CustomerId.value
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let! customerProfile = protect (ports.GetCustomerProfileFact cmd.CustomerId)
            let creditHoldActive = customerProfile |> Option.map (fun p -> p.CreditHoldActive) |> Option.defaultValue false

            let decideFn = Behaviors.createHold policy true true true creditHoldActive
            let pipeline = CommandPipeline.create repo (fun (c: CreateTemporaryHoldCmd) -> c.ReservationToken) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let confirmHold (req: ConfirmHoldToCommitmentReq) : Task<Result<TemporaryReservation, ApplicationError>> =
        taskResult {
            let! cmd: ConfirmHoldToCommitmentCmd = liftValidation (ACL.toConfirmCmd req)
            let decideFn = Behaviors.confirmHold
            let pipeline = CommandPipeline.create repo (fun (c: ConfirmHoldToCommitmentCmd) -> c.ReservationToken) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let releaseHold (req: ReleaseTemporaryHoldReq) : Task<Result<TemporaryReservation, ApplicationError>> =
        taskResult {
            let! cmd: ReleaseTemporaryHoldCmd = liftValidation (ACL.toReleaseCmd req)
            let decideFn = Behaviors.releaseHold
            let pipeline = CommandPipeline.create repo (fun (c: ReleaseTemporaryHoldCmd) -> c.ReservationToken) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let expireHold (req: ExpireTemporaryHoldReq) : Task<Result<TemporaryReservation, ApplicationError>> =
        taskResult {
            let! cmd: ExpireTemporaryHoldCmd = liftValidation (ACL.toExpireCmd req)
            let decideFn = Behaviors.expireHold
            let pipeline = CommandPipeline.create repo (fun (c: ExpireTemporaryHoldCmd) -> c.ReservationToken) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { CreateHold = createHold
      ConfirmHold = confirmHold
      ReleaseHold = releaseHold
      ExpireHold = expireHold }

