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

/// Lifecycle states for Scenario
type ScenarioLifecycleState =
    | Draft
    | Active
    | Archived

/// Lifecycle states for Plan
type PlanLifecycleState =
    | Draft
    | Approved
    | Superseded
    | Archived

/// Lifecycle states for Bill of Materials
type BomLifecycleState =
    | Draft
    | Active
    | Superseded
    | Archived

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

type ObligationDirection =
    | Inbound
    | Outbound

/// Adoption states for Unit of Measure and Planning Period
type AdoptionState =
    | Admitted
    | Deprecated
    | Retired

/// Adoption states for Calendar
type CalendarAdoptionState =
    | Active
    | Superseded
    | Retired

/// Lifecycle states for Governed Catalogs (Vocabulary, PI Catalog)
type GovernedCatalogState =
    | Active
    | Deprecated
    | Retired

/// Lifecycle states for Risk objects.
type RiskLifecycleState =
    | Active
    | Retired

/// SE-C-040 Item Transition Lifecycle State
type ItemTransitionLifecycleState =
    | Active
    | Inactive
    | Retired
