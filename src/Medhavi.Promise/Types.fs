namespace Medhavi.Promise

open System
open Medhavi.SemanticModel

/// Order line priority classification
[<RequireQualifiedAccess>]
type OrderPriorityClass =
    | Critical
    | Expedited
    | Standard
    | Promotional

/// Validation status of an individual order line within an order book snapshot
[<RequireQualifiedAccess>]
type LineValidationStatus =
    | PendingValidation
    | Validated
    | InactiveSku
    | InvalidDestination
    | CreditHold
    | ZeroQuantity
    | PastDueDeliveryDate
    | Quarantined of Reason: string

/// Fulfillment mode for a sourcing leg
[<RequireQualifiedAccess>]
type FulfillmentMode =
    | WarehouseGroundFreight
    | WarehouseAirExpress
    | FactoryDirectDispatch
    | SupplierDropShipment

    static member OfString (str: string) =
        match (if isNull str then "" else str.Trim().ToLowerInvariant()) with
        | "air" | "airfreight" | "warehouseairexpress" -> WarehouseAirExpress
        | "factory" | "factorydirect" | "factorydirectdispatch" -> FactoryDirectDispatch
        | "dropship" | "dropshipment" | "supplierdropshipment" -> SupplierDropShipment
        | _ -> WarehouseGroundFreight

/// Feasible sourcing leg within a promise option or commitment revision
type SourcingLeg =
    { LegId: string
      OriginLocation: LocationId
      Item: ItemId
      SourcedQuantity: Quantity
      FulfillmentMode: FulfillmentMode
      EstimatedShipDate: Timestamp
      EstimatedDeliveryDate: Timestamp
      FreightCost: decimal
      CarbonEmissionsKg: decimal }

/// Candidate promise option evaluated across fulfillment alternatives
type CandidatePromiseOption =
    { OptionId: string
      Category: string
      EarliestDeliveryDate: Timestamp
      PromisedQuantity: Quantity
      TotalLandedCost: decimal
      GrossMarginPct: decimal
      CarbonEmissionsKg: decimal
      SpeedScore: decimal
      CostScore: decimal
      MarginScore: decimal
      SustainabilityScore: decimal
      CompositeRankScore: decimal
      SourcingLegs: SourcingLeg list
      FeasibilityPassed: bool
      RejectionReasons: string list }

/// Commitment revision record in the immutable Order Commitment ledger
type CommitmentRevision =
    { RevisionIndex: int
      CommittedDeliveryDate: Timestamp
      PromisedQuantity: Quantity
      ProjectedLandedCost: decimal
      GrossMarginPct: decimal
      DecisionConfidence: decimal
      SourcingLegs: SourcingLeg list
      RevisionReason: string
      RevisionTimestamp: Timestamp }

/// Ephemeral capacity reservation slot at a plant
type CapacityReservationSlot =
    { SlotId: string
      PhysicalResourceId: PhysicalResourceId
      ReservedHours: decimal
      WorkCenterShiftDate: Timestamp }

/// Ephemeral inventory hold at a distribution center
type InventoryReservationHold =
    { HoldId: string
      Location: LocationId
      Item: ItemId
      ReservedQuantity: Quantity
      BatchOrLotNumber: string option }

/// Promise risk breach severity tier
[<RequireQualifiedAccess>]
type PromiseRiskTier =
    | Negligible
    | Low
    | Medium
    | High
    | Critical

/// Operational disruption category
[<RequireQualifiedAccess>]
type PromiseDisruptionType =
    | InboundSupplyShortfall
    | WorkCenterCapacityBreakdown
    | LogisticsTransitBottleneck
    | CustomerCommercialHold
    | AllocationPoolExhausted

/// Recovery mitigation action plan
type MitigationActionPlan =
    { PlanId: string
      ActionType: string
      EstimatedExpediteCost: decimal
      NewProjectedDeliveryDate: Timestamp
      ProjectedGrossMarginPct: decimal
      Authorized: bool }

/// Audit step in the causal explainability graph
type LineageNode =
    { StepIndex: int
      StageName: string
      EntityReference: string
      ActionTaken: string
      EvidenceSummary: string }

/// Machine-learning calibrated parameter tuning record
type CalibratedParameter =
    { ParameterName: string
      TargetEntityType: string
      EntityIdentifier: string
      BaselineValue: decimal
      EmpiricalObservedValue: decimal
      RecommendedCalibratedValue: decimal
      DampenedStepDelta: decimal }

