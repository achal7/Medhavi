/// PO-S-034 — Inventory Health Assessment Policy
module Medhavi.Supply.ManageInventory.InventoryHealth.Policies

open Medhavi.Supply.ManageInventory

type InventoryHealthAssessmentPolicy =
    {
        MinEvaluationPeriodDays: int
        MinConfidenceThreshold: decimal
        CadenceIntervalDays: int
        PrecedenceOrder: InventoryHealthStatus list
    }

module InventoryHealthAssessmentPolicy =
    let defaultPolicy =
        {
            MinEvaluationPeriodDays = 30
            MinConfidenceThreshold = 0.70m
            CadenceIntervalDays = 30
            PrecedenceOrder =
                [
                    InventoryHealthStatus.Obsolete
                    InventoryHealthStatus.AtRisk
                    InventoryHealthStatus.UnderStocked
                    InventoryHealthStatus.OverStocked
                    InventoryHealthStatus.Optimal
                ]
        }
