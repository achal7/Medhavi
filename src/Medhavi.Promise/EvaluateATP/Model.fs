/// CA-P-004 Evaluate Multi-Option ATP — Model
/// Traces to: SE-P-020, CR-P-012, CR-P-013, CR-P-014, CR-P-015
module Medhavi.Promise.EvaluateATP.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers

type PromiseOptionCategory =
    | DirectShip
    | SplitShipment
    | Substitution
    | CrossDock

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

type CandidatePromiseOption =
    { OptionId: string
      Category: PromiseOptionCategory
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

type ATPFeasibilityAssessment =
    { OrderLineId: OrderLineId
      Item: ItemId
      DestinationLocation: LocationId
      RequestedQuantity: Quantity
      RequestedDeliveryDate: Timestamp
      TotalAvailableNetStock: Quantity
      CandidateOptions: CandidatePromiseOption list
      EvaluatedAt: Timestamp }

type EvaluateATPOptionsCmd =
    { OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      DestinationLocation: LocationId
      RequestedQuantity: Quantity
      RequestedDeliveryDate: Timestamp
      AllowSubstitutions: bool
      AllowSplits: bool }

type PromiseEvaluationEvent =
    | ATPOptionsGenerated of OrderLineId: OrderLineId * OptionCount: int * TopDeliveryDate: Timestamp option * TraceId: string
    | ProductSubstitutionEvaluated of OrderLineId: OrderLineId * OriginalItem: ItemId * SubstituteItem: ItemId * TraceId: string

