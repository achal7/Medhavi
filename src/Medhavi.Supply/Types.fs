namespace Medhavi.Supply

/// Governed supply transaction categories (Supply Intelligence Specification §4.2.1, SE-S-010)
[<RequireQualifiedAccess>]
type SupplyType =
    | Inventory
    | PurchaseOrder
    | ProductionOrder
    | Transfer
    | SupplierCommitment

type SupplierCollaborationHealthState =
    | CriticallyDependent
    | Underperforming
    | Unpredictable
    | AtRisk
    | Inactive
    | Healthy
    | Unassessed

/// Governed procurement feasibility status (Supply Intelligence Specification §7, SE-S-050)
[<RequireQualifiedAccess>]
type ProcurementFeasibility =
    | Feasible
    | ConditionallyFeasible of Constraints: string list
    | Infeasible of Reason: string

/// Governed sourcing strategy category (Supply Intelligence Specification §7, SE-S-050)
[<RequireQualifiedAccess>]
type SourcingStrategy =
    | SingleSource
    | DualSource
    | Competitive
    | StrategicPartnership

/// Governed procurement recommendation confidence (Supply Intelligence Specification §7, SE-S-050)
[<RequireQualifiedAccess>]
type RecommendationConfidence =
    | High
    | Medium
    | Low

/// Capacity utilization classification (Supply Intelligence Specification §6, SE-S-040, SE-S-041)
type CapacityUtilizationStatus =
    | Underutilized
    | Optimal
    | NearCapacity
    | Overloaded

/// Capacity constraint classification (Supply Intelligence Specification §6, SE-S-040, SE-S-041)
type CapacityConstraintStatus =
    | NonConstraint
    | NearConstraint
    | ActiveConstraint

/// Bottleneck assessment classification (Supply Intelligence Specification §6, SE-S-040, SE-S-041)
type BottleneckAssessmentStatus =
    | NotABottleneck
    | PotentialBottleneck
    | CurrentBottleneck

/// Capacity risk level (Supply Intelligence Specification §6, SE-S-040)
type CapacityRiskLevel =
    | Negligible
    | Low
    | Medium
    | High
    | Critical

/// Governed production schedule type (Supply Intelligence Specification §4.3.1, SE-S-070)
[<RequireQualifiedAccess>]
type ScheduleType =
    | Primary
    | Recovery
    | Frozen
    | Scenario

/// Governed schedule feasibility status (Supply Intelligence Specification §6, SE-S-070, DE-S-062)
[<RequireQualifiedAccess>]
type ScheduleFeasibility =
    | FullyFeasible
    | ConditionallyFeasible of Conditions: string list
    | Infeasible of Reasons: string list

/// Schedule confidence level (Supply Intelligence Specification §4.3.1, SE-S-070)
[<RequireQualifiedAccess>]
type ScheduleConfidence =
    | High
    | Medium
    | Low

/// Governed distribution feasibility classification (Supply Intelligence Specification §8, SE-S-080, SE-S-081)
[<RequireQualifiedAccess>]
type DistributionFeasibility =
    | FullyFeasible
    | ConditionallyFeasible
    | Infeasible

/// Governed distribution confidence level (Supply Intelligence Specification §8, SE-S-080, SE-S-081)
[<RequireQualifiedAccess>]
type DistributionConfidence =
    | High
    | Medium
    | Low

/// Governed distribution strategy category (Supply Intelligence Specification §8, SE-S-080)
[<RequireQualifiedAccess>]
type DistributionStrategy =
    | PreferredSource
    | AlternateSource
    | CrossDock
    | Deferral

/// Governed deployment priority category (Supply Intelligence Specification §8, SE-S-080)
[<RequireQualifiedAccess>]
type DeploymentPriority =
    | ServiceProtection
    | PromotionSupport
    | NewProductLaunch
    | SeasonalPositioning
    | InventoryBalancing

/// Governed distribution risk severity (Supply Intelligence Specification §8, SE-S-080, SE-S-081)
[<RequireQualifiedAccess>]
type RiskSeverity =
    | Low
    | Medium
    | High

/// Governed exception types (PO-S-120 Taxonomy)
[<RequireQualifiedAccess>]
type ExceptionType =
    | ServicePolicyViolation
    | SupplyDisruption
    | InventoryPolicyViolation
    | CapacityPolicyViolation
    | SupplierCollaborationBreach
    | ExternalEvent
    | EmergingRisk

/// Multi-dimensional severity: Business Impact
[<RequireQualifiedAccess>]
type SeverityBusinessImpact =
    | Negligible
    | Minor
    | Moderate
    | Major
    | Severe

/// Multi-dimensional severity: Urgency
[<RequireQualifiedAccess>]
type SeverityUrgency =
    | Immediate
    | ShortTerm
    | MediumTerm
    | Low

/// Multi-dimensional severity: Scope
[<RequireQualifiedAccess>]
type SeverityScope =
    | SingleItem
    | ProductFamily
    | Supplier
    | Resource
    | Location
    | BusinessUnit
    | Enterprise

/// Multi-dimensional severity: Potential Impact
[<RequireQualifiedAccess>]
type SeverityPotentialImpact =
    | Negligible
    | Minor
    | Moderate
    | Major
    | Severe

/// Multi-dimensional severity: Current Impact
[<RequireQualifiedAccess>]
type SeverityCurrentImpact =
    | None
    | Buffered
    | PartiallyMaterialized
    | Materialized

/// Governed exception horizon
[<RequireQualifiedAccess>]
type ExceptionHorizon =
    | Immediate
    | ShortTerm
    | MediumTerm
    | Future
    | Uncertain

/// Evaluation confidence
[<RequireQualifiedAccess>]
type ExceptionConfidence =
    | High
    | Medium
    | Low


