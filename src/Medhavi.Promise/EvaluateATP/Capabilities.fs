/// CA-P-004 Evaluate Multi-Option ATP Capabilities
module Medhavi.Promise.EvaluateATP.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model
open Policies

type EvaluateATPAggregateApi =
    { Evaluate: EvaluateATPOptionsReq -> Task<Result<ATPFeasibilityAssessment, ApplicationError>> }

let create
    (policy: MultiOptionPromisingPolicy)
    (ports: PromisePorts)
    : EvaluateATPAggregateApi =

    let evaluate (req: EvaluateATPOptionsReq) : Task<Result<ATPFeasibilityAssessment, ApplicationError>> =
        taskResult {
            let! cmd: EvaluateATPOptionsCmd = liftValidation (ACL.toEvaluateCmd req)

            let! itemExists = protect (ports.ItemExists cmd.Item)
            let! destExists = protect (ports.LocationExists cmd.DestinationLocation)
            let! custExists = protect (ports.CustomerExists cmd.CustomerId)
            let! inventoryFacts = protect (ports.GetNetworkInventoryFacts cmd.Item)
            let! itemCost = protect (ports.GetItemCostFact cmd.Item)

            let standardCogs = itemCost |> Option.map (fun c -> c.StandardCogs) |> Option.defaultValue 0.0m
            let listPrice = itemCost |> Option.map (fun c -> c.ListPrice) |> Option.defaultValue 0.0m

            let laneFinder origin dest =
                ports.GetTransportationLaneFact origin dest |> Async.AwaitTask |> Async.RunSynchronously

            let assessmentResult =
                Behaviors.evaluateATP
                    policy
                    itemExists
                    destExists
                    custExists
                    inventoryFacts
                    laneFinder
                    standardCogs
                    listPrice
                    cmd

            return! assessmentResult |> Result.mapError ApplicationError.fromDomainError |> TaskResult.ofResult
        }

    { Evaluate = evaluate }

