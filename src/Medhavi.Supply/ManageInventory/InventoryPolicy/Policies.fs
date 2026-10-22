/// PO-S-033 — Inventory Policy Governance
module Medhavi.Supply.ManageInventory.InventoryPolicy.Policies

open System

type InventoryPolicyGovernancePolicy =
    { DefaultServiceLevelTarget: decimal
      MinServiceLevelTarget: decimal
      MaxServiceLevelTarget: decimal
      ColdStartSegmentCV: decimal
      MaxAllowedVolatilityThreshold: decimal
      MinHistoryObservations: int
      DefaultReviewIntervalDays: int }

module InventoryPolicyGovernancePolicy =
    let defaultPolicy =
        { DefaultServiceLevelTarget = 0.95m
          MinServiceLevelTarget = 0.50m
          MaxServiceLevelTarget = 0.999m
          ColdStartSegmentCV = 0.50m
          MaxAllowedVolatilityThreshold = 0.30m
          MinHistoryObservations = 12
          DefaultReviewIntervalDays = 7 }

    let getZScore (serviceLevel: decimal) : decimal =
        if serviceLevel >= 0.999m then 3.09m
        elif serviceLevel >= 0.990m then 2.33m
        elif serviceLevel >= 0.980m then 2.05m
        elif serviceLevel >= 0.950m then 1.65m
        elif serviceLevel >= 0.900m then 1.28m
        elif serviceLevel >= 0.850m then 1.04m
        elif serviceLevel >= 0.800m then 0.84m
        else 0.00m
