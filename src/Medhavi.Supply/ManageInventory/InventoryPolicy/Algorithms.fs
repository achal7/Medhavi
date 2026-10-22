/// SE-S-031 — Inventory Policy Algorithms
/// Traces to: BA-S-002 (Compute Safety Stock), BA-S-003 (Compute Reorder Point)
module Medhavi.Supply.ManageInventory.InventoryPolicy.Algorithms

open System
open Policies

type SafetyStockInput =
    { AverageDemand: decimal
      DemandStdDev: decimal
      LeadTimeDays: decimal
      LeadTimeStdDevDays: decimal
      ServiceLevelTarget: decimal
      ObservationCount: int }

type SafetyStockResult =
    { SafetyStockQuantity: decimal
      ServiceFactorZ: decimal
      CombinedStdDev: decimal
      IsColdStart: bool }

/// BA-S-002: Compute Safety Stock (Deterministic, pure mathematical calculation)
let computeSafetyStock (policy: InventoryPolicyGovernancePolicy) (input: SafetyStockInput) : SafetyStockResult =
    let clampedSL = min policy.MaxServiceLevelTarget (max policy.MinServiceLevelTarget input.ServiceLevelTarget)
    let z = InventoryPolicyGovernancePolicy.getZScore clampedSL

    if input.AverageDemand <= 0m || input.LeadTimeDays <= 0m then
        { SafetyStockQuantity = 0m
          ServiceFactorZ = z
          CombinedStdDev = 0m
          IsColdStart = false }
    elif input.ObservationCount < policy.MinHistoryObservations then
        // Cold-start fallback
        let rawSS = input.AverageDemand * policy.ColdStartSegmentCV * z
        let ss = max 0m (Math.Round(rawSS, 0, MidpointRounding.AwayFromZero))
        { SafetyStockQuantity = ss
          ServiceFactorZ = z
          CombinedStdDev = input.AverageDemand * policy.ColdStartSegmentCV
          IsColdStart = true }
    else
        // Bivariate convolution: sqrt(L * sigma_D^2 + D^2 * sigma_L^2)
        let demandTerm = input.LeadTimeDays * (input.DemandStdDev * input.DemandStdDev)
        let leadTimeTerm = (input.AverageDemand * input.AverageDemand) * (input.LeadTimeStdDevDays * input.LeadTimeStdDevDays)
        let variance = demandTerm + leadTimeTerm
        let stdDev = decimal (Math.Sqrt(float variance))
        let rawSS = z * stdDev
        let ss = max 0m (Math.Round(rawSS, 0, MidpointRounding.AwayFromZero))
        { SafetyStockQuantity = ss
          ServiceFactorZ = z
          CombinedStdDev = stdDev
          IsColdStart = false }

type ReorderPointInput =
    { AverageDemand: decimal
      LeadTimeDays: decimal
      SafetyStock: decimal
      ReviewIntervalDays: decimal option }

type ReorderPointResult =
    { ReorderPointQuantity: decimal
      ExpectedLeadTimeDemand: decimal
      SafetyStockComponent: decimal }

/// BA-S-003: Compute Reorder Point (Deterministic, pure threshold calculation)
let computeReorderPoint (input: ReorderPointInput) : ReorderPointResult =
    let effectiveLeadTime =
        match input.ReviewIntervalDays with
        | Some reviewInterval when reviewInterval > 0m -> input.LeadTimeDays + reviewInterval
        | _ -> input.LeadTimeDays

    let ltd = input.AverageDemand * effectiveLeadTime
    let rawROP = ltd + input.SafetyStock
    let rop = max 0m (Math.Round(rawROP, 0, MidpointRounding.AwayFromZero))

    { ReorderPointQuantity = rop
      ExpectedLeadTimeDemand = ltd
      SafetyStockComponent = input.SafetyStock }
