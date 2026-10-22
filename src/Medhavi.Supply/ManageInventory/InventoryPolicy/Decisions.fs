/// SE-S-031 — Inventory Policy Decisions
/// Traces to: DE-S-022 (Determine Inventory Policy)
module Medhavi.Supply.ManageInventory.InventoryPolicy.Decisions

open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply.ManageInventory
open Model
open Policies
open Algorithms
open Rules

type PolicyMethodologyOutcome =
    | ApplyStatisticalFormula of InventoryPolicyParameters
    | ApplySimulationBasedOptimization of InventoryPolicyParameters
    | ApplyManualOverride of InventoryPolicyParameters
    | MaintainCurrentPolicy of reason: string

type PolicyDeterminationInput =
    {
        DemandHistoryPeriods: int
        AverageDemand: decimal
        DemandStdDev: decimal
        LeadTimeDays: int
        LeadTimeStdDevDays: decimal
        TargetServiceLevel: decimal
        ManualOverride: InventoryPolicyParameters option
    }

/// DE-S-022: Determine Inventory Policy
let determineInventoryPolicy
    (policy: InventoryPolicyGovernancePolicy)
    (currentPolicyOpt: InventoryPolicyParameters option)
    (input: PolicyDeterminationInput)
    : Result<PolicyMethodologyOutcome, DomainError> =

    match input.ManualOverride with
    | Some overrideParams ->
        match validatePolicyParameters overrideParams with
        | Ok() -> Ok(ApplyManualOverride overrideParams)
        | Error err -> Error err
    | None ->
        if input.DemandHistoryPeriods < 3 && currentPolicyOpt.IsSome then
            Ok(MaintainCurrentPolicy "InsufficientDemandHistory")
        else
            let ssInput: SafetyStockInput =
                {
                    AverageDemand = input.AverageDemand
                    DemandStdDev = input.DemandStdDev
                    LeadTimeDays = decimal input.LeadTimeDays
                    LeadTimeStdDevDays = input.LeadTimeStdDevDays
                    ServiceLevelTarget = input.TargetServiceLevel
                    ObservationCount = input.DemandHistoryPeriods
                }

            let ssRes = computeSafetyStock policy ssInput

            let ropInput: ReorderPointInput =
                {
                    AverageDemand = input.AverageDemand
                    LeadTimeDays = decimal input.LeadTimeDays
                    SafetyStock = ssRes.SafetyStockQuantity
                    ReviewIntervalDays = Some(decimal policy.DefaultReviewIntervalDays)
                }

            let ropRes = computeReorderPoint ropInput

            let targetStock =
                ropRes.ReorderPointQuantity + (input.AverageDemand * decimal policy.DefaultReviewIntervalDays)

            let ssQty = Quantity.create ssRes.SafetyStockQuantity |> Result.defaultWith(fun _ -> Quantity.zero)
            let ropQty = Quantity.create ropRes.ReorderPointQuantity |> Result.defaultWith(fun _ -> Quantity.zero)
            let targetQty = Quantity.create targetStock |> Result.defaultWith(fun _ -> Quantity.zero)

            let newParams: InventoryPolicyParameters =
                {
                    ReplenishmentStrategy = ReplenishmentStrategy.ReorderPoint
                    SafetyStockQuantity = ssQty
                    ReorderPointQuantity = Some ropQty
                    MinimumQuantity = Some ssQty
                    MaximumQuantity = Some targetQty
                    TargetStockLevel = targetQty
                    TargetServiceLevel = input.TargetServiceLevel
                    ReviewFrequencyDays = Some policy.DefaultReviewIntervalDays
                    LeadTimeDays = input.LeadTimeDays
                    DemandVariability =
                        if input.AverageDemand > 0m then
                            input.DemandStdDev / input.AverageDemand
                        else
                            0m
                }

            match validatePolicyParameters newParams with
            | Ok() -> Ok(ApplyStatisticalFormula newParams)
            | Error err -> Error err
