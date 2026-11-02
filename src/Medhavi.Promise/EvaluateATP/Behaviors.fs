/// CA-P-004 Evaluate Multi-Option ATP Behaviors
/// Traces to: CR-P-012, CR-P-013, CR-P-014, CR-P-015
module Medhavi.Promise.EvaluateATP.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let evaluateATP
    (policy: MultiOptionPromisingPolicy)
    (itemExists: bool)
    (destinationExists: bool)
    (customerExists: bool)
    (inventoryFacts: NetworkInventoryFact list)
    (laneFinder: LocationId -> LocationId -> TransportationLaneFact option)
    (standardCogs: decimal)
    (listPrice: decimal)
    (cmd: EvaluateATPOptionsCmd)
    : Result<ATPFeasibilityAssessment, DomainError> =
    result {
        let input: EvaluateATPInput =
            { Cmd = cmd
              ItemExists = itemExists
              DestinationExists = destinationExists
              CustomerExists = customerExists
              AvailableStockCount = inventoryFacts.Length
              Policy = policy }

        let reqQtyVal = Quantity.value cmd.RequestedQuantity
        let uom = Quantity.unitOfMeasure cmd.RequestedQuantity

        // 1. Formulate Direct Ship Candidate Options from each DC with stock and valid lane
        let directOptions =
            inventoryFacts
            |> List.choose (fun inv ->
                match laneFinder inv.Location cmd.DestinationLocation with
                | Some lane ->
                    let availableVal = Quantity.value inv.AvailableQuantity
                    let sourcedVal = Math.Min(reqQtyVal, availableVal)
                    if sourcedVal > 0m then
                        let shipDate = cmd.RequestedDeliveryDate
                        let deliveryDate =
                            let arr = (Timestamp.value shipDate).AddDays(float lane.TransitLeadTimeDays)
                            Timestamp.create arr |> Result.defaultWith (fun _ -> shipDate)

                        let leg: SourcingLeg =
                            { LegId = Guid.NewGuid().ToString("N").[0..7]
                              OriginLocation = inv.Location
                              Item = cmd.Item
                              SourcedQuantity = Quantity.ofDecimal sourcedVal uom
                              FulfillmentMode = lane.FulfillmentMode
                              EstimatedShipDate = shipDate
                              EstimatedDeliveryDate = deliveryDate
                              FreightCost = lane.FreightCostPerUnit * sourcedVal
                              CarbonEmissionsKg = lane.CarbonEmissionsKgPerUnit * sourcedVal }

                        let option =
                            Algorithms.evaluateCandidateOption
                                policy
                                DirectShip
                                [ leg ]
                                cmd.RequestedQuantity
                                cmd.RequestedDeliveryDate
                                standardCogs
                                listPrice
                        Some option
                    else None
                | None -> None)

        // 2. Formulate Split Shipment Option if allowed and direct fulfillment is insufficient
        let splitOption =
            if cmd.AllowSplits && directOptions |> List.forall (fun o -> not o.FeasibilityPassed) then
                let availableLocations =
                    inventoryFacts
                    |> List.choose (fun inv ->
                        laneFinder inv.Location cmd.DestinationLocation
                        |> Option.map (fun lane -> inv, lane))

                let rec fillRemaining (rem: decimal) (acc: SourcingLeg list) locs =
                    match locs with
                    | [] -> acc
                    | (inv: NetworkInventoryFact, lane: TransportationLaneFact) :: tail ->
                        if rem <= 0m then acc
                        else
                            let avail = Quantity.value inv.AvailableQuantity
                            let take = Math.Min(rem, avail)
                            if take > 0m then
                                let shipDate = cmd.RequestedDeliveryDate
                                let deliveryDate =
                                    let arr = (Timestamp.value shipDate).AddDays(float lane.TransitLeadTimeDays)
                                    Timestamp.create arr |> Result.defaultWith (fun _ -> shipDate)

                                let leg: SourcingLeg =
                                    { LegId = Guid.NewGuid().ToString("N").[0..7]
                                      OriginLocation = inv.Location
                                      Item = cmd.Item
                                      SourcedQuantity = Quantity.ofDecimal take uom
                                      FulfillmentMode = lane.FulfillmentMode
                                      EstimatedShipDate = shipDate
                                      EstimatedDeliveryDate = deliveryDate
                                      FreightCost = lane.FreightCostPerUnit * take
                                      CarbonEmissionsKg = lane.CarbonEmissionsKgPerUnit * take }

                                fillRemaining (rem - take) (leg :: acc) tail
                            else fillRemaining rem acc tail

                let legs = fillRemaining reqQtyVal [] availableLocations
                if not legs.IsEmpty && legs.Length <= policy.MaxSplitShipments then
                    let option =
                        Algorithms.evaluateCandidateOption
                            policy
                            SplitShipment
                            legs
                            cmd.RequestedQuantity
                            cmd.RequestedDeliveryDate
                            standardCogs
                            listPrice
                    Some option
                else None
            else None

        let candidateOptions =
            directOptions @ (splitOption |> Option.toList)

        let! decision = evaluateCandidateOptions atpEvaluationRules input candidateOptions

        match decision.Outcome with
        | OptionsSelected sortedOptions ->
            let totalAvailable =
                let sum = inventoryFacts |> List.sumBy (fun i -> Quantity.value i.AvailableQuantity)
                Quantity.ofDecimal sum uom

            let assessment: ATPFeasibilityAssessment =
                { OrderLineId = cmd.OrderLineId
                  Item = cmd.Item
                  DestinationLocation = cmd.DestinationLocation
                  RequestedQuantity = cmd.RequestedQuantity
                  RequestedDeliveryDate = cmd.RequestedDeliveryDate
                  TotalAvailableNetStock = totalAvailable
                  CandidateOptions = sortedOptions
                  EvaluatedAt = cmd.RequestedDeliveryDate }

            return assessment

        | OptionsRejected reason ->
            return! Error(DomainError.rule (reason, rule = Rules.multiOptionAtpNetworkRouteEligibility.Id))
    }

