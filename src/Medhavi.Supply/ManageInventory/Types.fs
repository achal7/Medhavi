namespace Medhavi.Supply.ManageInventory


// ---------- Governed Enums ----------

type InventoryCoverageStatus =
    | Understocked
    | Optimal
    | Overstocked

type InventoryRiskLevel =
    | VeryLow
    | Low
    | Medium
    | High
    | Critical

type InventoryHealthStatus =
    | Optimal
    | UnderStocked
    | OverStocked
    | AtRisk
    | Obsolete
    | Unassessed

type PolicyComplianceStatus =
    | Conformant
    | PolicyBandBreach
    | BelowSafetyStock
    | Unassessed

type ReplenishmentStrategy =
    | ReorderPoint
    | MinMax
    | PeriodicReview
    | BaseStock