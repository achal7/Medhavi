/// CA-P-002 Allocate Supply and Manage Priority Aggregate Capabilities
module Medhavi.Promise.AllocateSupply.AllocationPool.Capabilities

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

type AllocationAggregateApi =
    { Initialize: InitializeAllocationPoolReq -> Task<Result<AllocationPool, ApplicationError>>
      DrawQuota: DrawAllocationQuotaReq -> Task<Result<AllocationPool, ApplicationError>>
      Rebalance: RebalancePoolsReq -> Task<Result<AllocationPool, ApplicationError>>
      CalculatePriority: CalculateDynamicPriorityReq -> Task<Result<DynamicOrderPriority, ApplicationError>> }

let create
    (repo: Repository<AllocationPool, AllocationPoolId, AllocationPoolEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (govPolicy: AllocationGovernancePolicy)
    (overdrawPolicy: ChannelOverdrawPolicy)
    (priorityPolicy: DynamicOrderPriorityPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<AllocationPoolEvent>)
    (ports: PromisePorts)
    : AllocationAggregateApi =

    let initialize (req: InitializeAllocationPoolReq) : Task<Result<AllocationPool, ApplicationError>> =
        taskResult {
            let! cmd: InitializeAllocationPoolCmd = liftValidation (ACL.toInitializeCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value
            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let decideFn = Behaviors.initialize govPolicy true true true
            let pipeline = CommandPipeline.create repo (fun (c: InitializeAllocationPoolCmd) -> c.PoolId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let drawQuota (req: DrawAllocationQuotaReq) : Task<Result<AllocationPool, ApplicationError>> =
        taskResult {
            let! cmd: DrawAllocationQuotaCmd = liftValidation (ACL.toDrawQuotaCmd req)
            let decideFn = Behaviors.drawQuota govPolicy
            let pipeline = CommandPipeline.create repo (fun (c: DrawAllocationQuotaCmd) -> c.PoolId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let rebalance (req: RebalancePoolsReq) : Task<Result<AllocationPool, ApplicationError>> =
        taskResult {
            let! cmd: RebalanceAllocationBucketsCmd = liftValidation (ACL.toRebalanceCmd req)
            let decideFn = Behaviors.rebalance govPolicy overdrawPolicy
            let pipeline = CommandPipeline.create repo (fun (c: RebalanceAllocationBucketsCmd) -> c.SourcePoolId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let calculatePriority (req: CalculateDynamicPriorityReq) : Task<Result<DynamicOrderPriority, ApplicationError>> =
        taskResult {
            let! cmd: CalculateDynamicPriorityCmd = liftValidation (ACL.toCalculatePriorityCmd req)

            let! customerExists = protect (ports.CustomerExists cmd.CustomerId)
            let! itemExists = protect (ports.ItemExists cmd.Item)
            let! customerProfile = protect (ports.GetCustomerProfileFact cmd.CustomerId)
            let! itemCost = protect (ports.GetItemCostFact cmd.Item)

            let customerTier = customerProfile |> Option.map (fun p -> p.CustomerTier) |> Option.defaultValue "Tier 3"
            let penaltyRate = customerProfile |> Option.map (fun p -> p.DailyPenaltyRatePct) |> Option.defaultValue 0.0m
            let standardCogs = itemCost |> Option.map (fun c -> c.StandardCogs) |> Option.defaultValue 0.0m

            let priorityResult =
                Behaviors.calculatePriority
                    priorityPolicy
                    customerExists
                    itemExists
                    customerTier
                    standardCogs
                    penaltyRate
                    cmd

            return! priorityResult |> Result.mapError ApplicationError.fromDomainError |> TaskResult.ofResult
        }

    { Initialize = initialize
      DrawQuota = drawQuota
      Rebalance = rebalance
      CalculatePriority = calculatePriority }

