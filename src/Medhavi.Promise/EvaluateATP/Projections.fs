/// CA-P-004 Evaluate Multi-Option ATP Projections
module Medhavi.Promise.EvaluateATP.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapLegToDto (l: SourcingLeg) : SourcingLegDto =
    { LegId = l.LegId
      OriginLocation = LocationId.value l.OriginLocation
      Item = ItemId.value l.Item
      SourcedQuantity = Quantity.value l.SourcedQuantity
      FulfillmentMode = sprintf "%A" l.FulfillmentMode
      EstimatedShipDate = Timestamp.value l.EstimatedShipDate
      EstimatedDeliveryDate = Timestamp.value l.EstimatedDeliveryDate
      FreightCost = l.FreightCost
      CarbonEmissionsKg = l.CarbonEmissionsKg }

let mapCandidateOptionToDto (o: CandidatePromiseOption) : CandidatePromiseOptionDto =
    { OptionId = o.OptionId
      Category =
        match o.Category with
        | DirectShip -> "DirectShip"
        | SplitShipment -> "SplitShipment"
        | Substitution -> "Substitution"
        | CrossDock -> "CrossDock"
      EarliestDeliveryDate = Timestamp.value o.EarliestDeliveryDate
      PromisedQuantity = Quantity.value o.PromisedQuantity
      TotalLandedCost = o.TotalLandedCost
      GrossMarginPct = o.GrossMarginPct
      CarbonEmissionsKg = o.CarbonEmissionsKg
      SpeedScore = o.SpeedScore
      CostScore = o.CostScore
      MarginScore = o.MarginScore
      SustainabilityScore = o.SustainabilityScore
      CompositeRankScore = o.CompositeRankScore
      SourcingLegs = o.SourcingLegs |> List.map mapLegToDto
      FeasibilityPassed = o.FeasibilityPassed
      RejectionReasons = o.RejectionReasons }

let mapAssessmentToDto (a: ATPFeasibilityAssessment) : ATPFeasibilityAssessmentDto =
    { OrderLineId = OrderLineId.value a.OrderLineId
      Item = ItemId.value a.Item
      DestinationLocation = LocationId.value a.DestinationLocation
      RequestedQuantity = Quantity.value a.RequestedQuantity
      RequestedDeliveryDate = Timestamp.value a.RequestedDeliveryDate
      TotalAvailableNetStock = Quantity.value a.TotalAvailableNetStock
      CandidateOptions = a.CandidateOptions |> List.map mapCandidateOptionToDto
      EvaluatedAt = Timestamp.value a.EvaluatedAt }

