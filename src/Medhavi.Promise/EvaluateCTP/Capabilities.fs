/// CA-P-005 Evaluate Capable-to-Promise Capabilities
module Medhavi.Promise.EvaluateCTP.Capabilities

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

type EvaluateCTPAggregateApi =
    { Evaluate: EvaluateCTPOptionsReq -> Task<Result<CTPFeasibilityAssessment, ApplicationError>> }

let create
    (policy: CapableToPromisePolicy)
    (atpPolicy: EvaluateATP.Policies.MultiOptionPromisingPolicy)
    (ports: PromisePorts)
    : EvaluateCTPAggregateApi =

    let evaluate (req: EvaluateCTPOptionsReq) : Task<Result<CTPFeasibilityAssessment, ApplicationError>> =
        taskResult {
            let! cmd: EvaluateCTPOptionsCmd = liftValidation (ACL.toEvaluateCmd req)

            let! itemExists = protect (ports.ItemExists cmd.Item)
            let! plantExists = protect (ports.LocationExists cmd.PlantLocation)
            let! custExists = protect (ports.CustomerExists cmd.CustomerId)
            let! bomComponents = protect (ports.GetBomComponentFacts cmd.Item)
            let! itemCost = protect (ports.GetItemCostFact cmd.Item)
            let! lane = protect (ports.GetTransportationLaneFact cmd.PlantLocation cmd.DestinationLocation)

            let! defaultResource = liftValidation (validatePhysicalResourceId "PLANT-MACH-01")
            let shiftEnd =
                let fin = (Timestamp.value cmd.RequestedDeliveryDate).AddDays(30.0)
                Timestamp.create fin |> Result.defaultWith (fun _ -> cmd.RequestedDeliveryDate)

            let! capacitySlots = protect (ports.GetCapacitySlotFacts defaultResource cmd.RequestedDeliveryDate shiftEnd)

            let standardCogs = itemCost |> Option.map (fun c -> c.StandardCogs) |> Option.defaultValue 0.0m
            let listPrice = itemCost |> Option.map (fun c -> c.ListPrice) |> Option.defaultValue 0.0m

            let assessmentResult =
                Behaviors.evaluateCTP
                    policy
                    atpPolicy
                    itemExists
                    plantExists
                    custExists
                    bomComponents
                    capacitySlots
                    lane
                    standardCogs
                    listPrice
                    cmd

            return! assessmentResult |> Result.mapError ApplicationError.fromDomainError |> TaskResult.ofResult
        }

    { Evaluate = evaluate }

