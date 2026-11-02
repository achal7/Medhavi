/// CA-P-002 Allocate Supply and Manage Priority Behaviors
/// Traces to: AB-P-004 Initialize Allocation Pool, AB-P-011 Calculate Dynamic Order Priority, AB-P-012 Consume Allocation Quota, AB-P-013 Rebalance Allocation Buckets
module Medhavi.Promise.AllocateSupply.AllocationPool.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId (decisionOutcome: DecisionOutcome<'T>) state events summary =
    let policyId = policy |> Option.map (fun _ -> Policies.allocationGovernancePolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.allocateSupplyAndManagePriority.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.allocationPool.Id ]
        (Some summary)

/// AB-P-004: Initialize Allocation Pool
let initialize
    (policy: AllocationGovernancePolicy)
    (scopeExists: bool)
    (itemExists: bool)
    (locationExists: bool)
    : Decide<AllocationPool, InitializeAllocationPoolCmd, AllocationPoolEvent> =
    fun (cmd: InitializeAllocationPoolCmd) (state: AllocationPool option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict ($"AllocationPool already exists: {AllocationPoolId.value cmd.PoolId}", Capabilities.allocateSupplyAndManagePriority.Id))
            | None ->
                let input: InitializePoolInput =
                    { Cmd = cmd
                      ScopeExists = scopeExists
                      ItemExists = itemExists
                      LocationExists = locationExists
                      Policy = policy }

                let! decision = evaluateEstablishQuota initPoolRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | QuotaEstablished ->
                    let pool: AllocationPool =
                        { PoolId = cmd.PoolId
                          PlanningScopeId = cmd.PlanningScopeId
                          Item = cmd.Item
                          Location = cmd.Location
                          PlanningPeriod = cmd.PlanningPeriod
                          TotalPlannedSupply = cmd.TotalPlannedSupply
                          Buckets = cmd.Buckets
                          LifecycleState = Active
                          DecisionTraceability = Some traceId }

                    let events = [ AllocationPoolInitialized pool ]
                    return buildTrace (Some policy) traceId decision state events "Allocation pool initialized"

                | QuotaRejected reasons ->
                    return! Error(DomainError.validation (String.concat "; " reasons))
        }

/// AB-P-012: Consume Allocation Quota
let drawQuota
    (policy: AllocationGovernancePolicy)
    : Decide<AllocationPool, DrawAllocationQuotaCmd, AllocationPoolEvent> =
    fun (cmd: DrawAllocationQuotaCmd) (state: AllocationPool option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound ("AllocationPool", AllocationPoolId.value cmd.PoolId))
            | Some pool ->
                let input: DrawQuotaInput =
                    { Cmd = cmd
                      CurrentPool = state
                      Policy = policy }

                let! decision = evaluateDrawQuota drawQuotaRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | DrawAuthorized ->
                    let targetBucket = pool.Buckets |> List.find (fun b -> b.BucketId = cmd.BucketId)
                    let remVal = Math.Max(0m, Quantity.value targetBucket.RemainingQuantity - Quantity.value cmd.Quantity)
                    let remaining = Quantity.ofDecimal remVal (Quantity.unitOfMeasure targetBucket.RemainingQuantity)
                    let events = [ AllocationQuotaConsumed(cmd.PoolId, cmd.BucketId, cmd.OrderLineId, cmd.Quantity, remaining, traceId) ]
                    return buildTrace (Some policy) traceId decision state events $"Quota drawn for order line {OrderLineId.value cmd.OrderLineId}"

                | DrawRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.allocationNonNegativityInvariant.Id))
        }

/// AB-P-013: Rebalance Allocation Buckets
let rebalance
    (govPolicy: AllocationGovernancePolicy)
    (overdrawPolicy: ChannelOverdrawPolicy)
    : Decide<AllocationPool, RebalanceAllocationBucketsCmd, AllocationPoolEvent> =
    fun (cmd: RebalanceAllocationBucketsCmd) (state: AllocationPool option) ->
        result {
            match state with
            | None ->
                return! Error(DomainError.notFound (SemanticObjects.allocationPool.Id, AllocationPoolId.value cmd.SourcePoolId))
            | Some _ ->
                let input: RebalanceInput =
                    { Cmd = cmd
                      CurrentPool = state
                      GovPolicy = govPolicy
                      OverdrawPolicy = overdrawPolicy }

                let! decision = evaluateRebalance rebalanceRules input
                let traceId = Guid.NewGuid().ToString()

                match decision.Outcome with
                | RebalanceAuthorized ->
                    let events =
                        [ AllocationBucketsRebalanced(
                            cmd.SourcePoolId,
                            cmd.SourceBucketId,
                            cmd.TargetPoolId,
                            cmd.TargetBucketId,
                            cmd.TransferQuantity,
                            cmd.RebalanceReason,
                            traceId) ]
                    return buildTrace (Some govPolicy) traceId decision state events $"Allocation rebalanced: {cmd.RebalanceReason}"

                | RebalanceRejected reason ->
                    return! Error(DomainError.rule (reason, rule = Rules.allocationRebalanceApproval.Id))
        }

/// AB-P-011: Calculate Dynamic Order Priority (Domain Service evaluation)
let calculatePriority
    (policy: DynamicOrderPriorityPolicy)
    (customerExists: bool)
    (itemExists: bool)
    (customerTier: string)
    (standardCogs: decimal)
    (dailyPenaltyRatePct: decimal)
    (cmd: CalculateDynamicPriorityCmd)
    : Result<DynamicOrderPriority, DomainError> =
    result {
        let input: CalculatePriorityInput =
            { Cmd = cmd
              CustomerExists = customerExists
              ItemExists = itemExists
              Policy = policy }

        let score =
            Algorithms.calculateCompositePriority
                cmd.OrderLineId
                policy
                customerTier
                cmd.UnitSellingPrice
                standardCogs
                dailyPenaltyRatePct
                cmd.OrderSubmissionTime
                cmd.RequestedDeliveryDate

        let! decision = evaluateDynamicPriority calculatePriorityRules input score

        match decision.Outcome with
        | PriorityScored evaluated -> return evaluated
        | PriorityRejected reason -> return! Error(DomainError.rule (reason, rule = Rules.compositePriorityScoreDerivation.Id))
    }

