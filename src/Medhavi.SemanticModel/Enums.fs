namespace Medhavi.SemanticModel

/// Lifecycle states for Reference Objects (Item, Location, Customer, Supplier)
type ReferenceLifecycleState =
    | Active
    | Inactive
    | Retired

/// Lifecycle states for Location (Closed instead of Retired per ESM)
type LocationLifecycleState =
    | Active
    | Inactive
    | Closed

/// Lifecycle states for Planning Objects (Scenario, Plan)
type PlanningLifecycleState =
    | Draft
    | Active
    | Archived
    | Superseded

/// Lifecycle states for Enterprise Facts (Demand, Supply, Commitment)
type DemandLifecycleState =
    | Active
    | Satisfied
    | Cancelled

type SupplyLifecycleState =
    | Available
    | Consumed
    | Withdrawn
    | Expired

type CommitmentLifecycleState =
    | Committed
    | Fulfilled
    | Cancelled

/// Lifecycle states for Core Intelligence (Exception, Enterprise Picture)
type ExceptionLifecycleState =
    | Active
    | Resolved

type PictureVersionLifecycleState =
    | Draft
    | Published
    | Superseded

/// Structural Classifications
type LocationType =
    | Plant
    | DistributionCenter
    | Warehouse
    | Store
    | CustomerSite
    | SupplierSite
    | Port
    | Depot
    | Terminal
    | Other

type CustomerClass =
    | ClassA
    | ClassB
    | ClassC
    | ClassD

type DemandOrigin =
    | CustomerOrder
    | Forecast
    | ProductionRequirement
    | Transfer
    | Other

type SupplyProvenanceClassification =
    | PurchaseOrder
    | ProductionOrder
    | TransferOrder
    | InventoryAdjustment
    | Other

type ObligationDirection =
    | Inbound
    | Outbound

/// Adoption states for Governed Vocabularies and Units of Measure
type AdoptionState =
    | Admitted
    | Deprecated
    | Retired

/// Lifecycle states for Risk objects.
type RiskLifecycleState =
    | Identified
    | Assessed
    | Mitigating
    | Closed
    | Retired

/// Planning period granularity.
type PeriodType =
    | Day
    | Week
    | Month
    | Quarter
    | Year
    | Custom

/// Scenario adjustment operator.
/// This is a coproduct, allowing what-if scenario adjustments to remain explicit
/// and AI-interpretable without hidden mutation.
type AdjustmentOperator =
    | Increase
    | Decrease
    | Replace
    | Constrain
