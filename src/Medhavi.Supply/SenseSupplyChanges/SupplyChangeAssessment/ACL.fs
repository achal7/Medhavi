/// SE-S-090 — Supply Change Assessment Inbound/Outbound ACL
/// Traces to: FS-S-085
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let private parseEntityType = function
    | "Supplier" -> Ok SupplyEntityType.Supplier
    | "PhysicalResource" -> Ok SupplyEntityType.PhysicalResource
    | "TransportationLane" -> Ok SupplyEntityType.TransportationLane
    | "Location" -> Ok SupplyEntityType.Location
    | "Item" -> Ok SupplyEntityType.Item
    | other -> Error(DomainError.validation $"Unrecognized SupplyEntityType: {other}")

let private parseChangeType = function
    | "QuantityReduction" -> Ok ChangeType.QuantityReduction
    | "QuantityIncrease" -> Ok ChangeType.QuantityIncrease
    | "DeliveryDelay" -> Ok ChangeType.DeliveryDelay
    | "DeliveryExpedite" -> Ok ChangeType.DeliveryExpedite
    | "CapacityReduction" -> Ok ChangeType.CapacityReduction
    | "CapacityIncrease" -> Ok ChangeType.CapacityIncrease
    | "Disruption" -> Ok ChangeType.Disruption
    | "QualityIssue" -> Ok ChangeType.QualityIssue
    | "PriceChange" -> Ok ChangeType.PriceChange
    | "RegulatoryChange" -> Ok ChangeType.RegulatoryChange
    | other -> Error(DomainError.validation $"Unrecognized ChangeType: {other}")

let private parseClassification = function
    | "Planned" -> Ok ChangeClassification.Planned
    | "Unplanned" -> Ok ChangeClassification.Unplanned
    | other -> Error(DomainError.validation $"Unrecognized ChangeClassification: {other}")

/// Inbound ACL: Translate external AssessSupplyChangeReq into domain AssessSupplyChangeCmd
let toAssessCmd
    (req: AssessSupplyChangeReq)
    (baseline: BaselineEntityFact)
    (corroboratingCount: int)
    : Validation<AssessSupplyChangeCmd, DomainError> =
    (fun entityType changeType classification obsTs sigTs reqTs ->
        let signal: IncomingSupplySignal =
            { SignalId = req.SignalId
              EntityReference = req.MonitoredEntityId
              SourceId = req.SourceId
              SourceType = req.SourceType
              SourceReliabilityPrior = req.SourceReliabilityPrior
              ObservedQuantity = req.ObservedQuantity
              ObservedDate = obsTs
              SignalGenerationTime = sigTs }

        let correlatedIds = req.CorrelatedChangeIds |> List.map SupplyChangeAssessmentId

        { AssessmentId = SupplyChangeAssessmentId req.AssessmentId
          MonitoredEntityType = entityType
          MonitoredEntityId = req.MonitoredEntityId
          ChangeType = changeType
          Signal = signal
          Baseline = baseline
          CorroboratingSignalsCount = corroboratingCount
          Classification = classification
          ExpectedDuration = req.ExpectedDuration
          CorrelatedChangeIds = correlatedIds
          Timestamp = reqTs })
    <!> (parseEntityType req.MonitoredEntityType |> fromResult)
    <*> (parseChangeType req.ChangeType |> fromResult)
    <*> (parseClassification req.Classification |> fromResult)
    <*> validateTimestamp req.ObservedDate
    <*> validateTimestamp req.SignalGenerationTime
    <*> validateTimestamp req.Timestamp

/// Inbound ACL: Translate external ArchiveSupplyChangeReq into domain ArchiveSupplyChangeCmd
let toArchiveCmd (req: ArchiveSupplyChangeReq) : Validation<ArchiveSupplyChangeCmd, DomainError> =
    (fun ts ->
        { AssessmentId = SupplyChangeAssessmentId req.AssessmentId
          Reason = req.Reason
          Timestamp = ts })
    <!> validateTimestamp req.Timestamp

/// Outbound ACL: Maps domain supply change events to canonical ARS Enterprise Events (EV-S-080).
let toArsEventId = function
    | SupplyChangeAssessmentCreated _ -> ArsIdentifiers.EnterpriseEvents.supplyChangeAssessmentUpdated.Id
    | SupplyChangeAssessmentUpdated _ -> ArsIdentifiers.EnterpriseEvents.supplyChangeAssessmentUpdated.Id
    | SupplyChangeAssessmentArchived _ -> ArsIdentifiers.EnterpriseEvents.supplyChangeAssessmentUpdated.Id
