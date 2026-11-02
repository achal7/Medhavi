/// CA-P-005 Evaluate Capable-to-Promise Behaviors
/// Traces to: CR-P-016, CR-P-017, CR-P-018
module Medhavi.Promise.EvaluateCTP.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Medhavi.Promise.EvaluateATP.Model
open Model
open Rules
open Decisions
open Policies

let evaluateCTP
    (policy: CapableToPromisePolicy)
    (atpPolicy: EvaluateATP.Policies.MultiOptionPromisingPolicy)
    (itemExists: bool)
    (plantExists: bool)
    (customerExists: bool)
    (bomComponents: BomComponentFact list)
    (capacitySlots: CapacitySlotFact list)
    (lane: TransportationLaneFact option)
    (standardCogs: decimal)
    (listPrice: decimal)
    (cmd: EvaluateCTPOptionsCmd)
    : Result<CTPFeasibilityAssessment, DomainError> =
    result {
        let input: EvaluateCTPInput =
            { Cmd = cmd
              ItemExists = itemExists
              PlantExists = plantExists
              CustomerExists = customerExists
              HasBomComponents = not bomComponents.IsEmpty
              HasCapacitySlots = not capacitySlots.IsEmpty
              Policy = policy }

        let feasible, finishDate, leadDays, stockoutGaps, bottleneck =
            Algorithms.calculateCtpSchedule
                policy
                bomComponents
                capacitySlots
                cmd.RequestedQuantity
                cmd.RequestedDeliveryDate

        let transitDays = lane |> Option.map (fun l -> l.TransitLeadTimeDays) |> Option.defaultValue 2
        let estDeliveryDate =
            let arr = (Timestamp.value finishDate).AddDays(float transitDays)
            Timestamp.create arr |> Result.defaultWith (fun _ -> finishDate)

        let candidateOption =
            let freightCost = lane |> Option.map (fun l -> l.FreightCostPerUnit * Quantity.value cmd.RequestedQuantity) |> Option.defaultValue 150.0m
            let carbon = lane |> Option.map (fun l -> l.CarbonEmissionsKgPerUnit * Quantity.value cmd.RequestedQuantity) |> Option.defaultValue 50.0m

            let leg: SourcingLeg =
                { LegId = Guid.NewGuid().ToString("N").[0..7]
                  OriginLocation = cmd.PlantLocation
                  Item = cmd.Item
                  SourcedQuantity = cmd.RequestedQuantity
                  FulfillmentMode = lane |> Option.map (fun l -> l.FulfillmentMode) |> Option.defaultValue FulfillmentMode.WarehouseGroundFreight
                  EstimatedShipDate = finishDate
                  EstimatedDeliveryDate = estDeliveryDate
                  FreightCost = freightCost
                  CarbonEmissionsKg = carbon }

            let opt =
                EvaluateATP.Algorithms.evaluateCandidateOption
                    atpPolicy
                    DirectShip
                    [ leg ]
                    cmd.RequestedQuantity
                    cmd.RequestedDeliveryDate
                    standardCogs
                    listPrice
            Some opt

        let rawAssessment: CTPFeasibilityAssessment =
            { OrderLineId = cmd.OrderLineId
              Item = cmd.Item
              PlantLocation = cmd.PlantLocation
              RequestedQuantity = cmd.RequestedQuantity
              EarliestProductionFinishDate = finishDate
              RequiredLeadTimeDays = leadDays
              ComponentStockoutGaps = stockoutGaps
              BottleneckWorkCenter = bottleneck
              CandidateOption = candidateOption
              EvaluatedAt = cmd.RequestedDeliveryDate }

        let! decision = evaluateCTPDecision ctpRules input rawAssessment

        match decision.Outcome with
        | CTPSlotAuthorized assessment -> return assessment
        | CTPRejected reason -> return! Error(DomainError.rule (reason, rule = Rules.capableToPromiseManufacturingEligibility.Id))
    }

