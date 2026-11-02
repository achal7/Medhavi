/// CA-P-005 Evaluate Capable-to-Promise Projections
module Medhavi.Promise.EvaluateCTP.Projections

open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let mapToDto (a: CTPFeasibilityAssessment) : CTPFeasibilityAssessmentDto =
    { OrderLineId = OrderLineId.value a.OrderLineId
      Item = ItemId.value a.Item
      PlantLocation = LocationId.value a.PlantLocation
      RequestedQuantity = Quantity.value a.RequestedQuantity
      EarliestProductionFinishDate = Timestamp.value a.EarliestProductionFinishDate
      RequiredLeadTimeDays = a.RequiredLeadTimeDays
      ComponentStockoutGaps = a.ComponentStockoutGaps
      BottleneckWorkCenter = a.BottleneckWorkCenter
      CandidateOption = a.CandidateOption |> Option.map EvaluateATP.Projections.mapCandidateOptionToDto
      EvaluatedAt = Timestamp.value a.EvaluatedAt }

