/// CA-P-005 Evaluate Capable-to-Promise — Model
/// Traces to: SE-P-020, CR-P-016, CR-P-017, CR-P-018
module Medhavi.Promise.EvaluateCTP.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.EvaluateATP.Model

type CTPFeasibilityAssessment =
    { OrderLineId: OrderLineId
      Item: ItemId
      PlantLocation: LocationId
      RequestedQuantity: Quantity
      EarliestProductionFinishDate: Timestamp
      RequiredLeadTimeDays: int
      ComponentStockoutGaps: string list
      BottleneckWorkCenter: string option
      CandidateOption: CandidatePromiseOption option
      EvaluatedAt: Timestamp }

type EvaluateCTPOptionsCmd =
    { OrderLineId: OrderLineId
      CustomerId: CustomerId
      Item: ItemId
      DestinationLocation: LocationId
      PlantLocation: LocationId
      RequestedQuantity: Quantity
      RequestedDeliveryDate: Timestamp }

type CTPEvent =
    | CTPProductionSlotEvaluated of OrderLineId: OrderLineId * Plant: LocationId * FinishDate: Timestamp * Feasible: bool * TraceId: string

