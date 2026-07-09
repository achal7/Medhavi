module Medhavi.Demand.Domain.DemandObservatoinDecisions

open System
open Medhavi.Common
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.Domain.DemandObservationAgg   // contains types, evolve, commands, events

// =============================================================================
// DE‑D‑010 — Accept Demand Observation
// =============================================================================
let acceptDemandObservation
    (signal: DemandSignal option)
    (obs: DemandObservation)
    : Result<ObservationEvent list, DomainError> =

    // Evaluate signal-based rules if signal present
    result {
        match signal with
        | Some s ->
            do! DemandObservationRules.signalTimeliness s (TimeSpan.FromHours 1.0)
            do! DemandObservationRules.signalRange s
            do! DemandObservationRules.signalSourceReliability s 60.0
        | None -> ()

        // Simple acceptance logic – later we can add more sophisticated weighting
        let confidence = 0.95  // placeholder
        let decision : ObservationDecision = {
            DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
            Timestamp = DateTimeOffset.UtcNow
            Confidence = confidence
            Rationale = "Observation accepted based on signal evaluation."
            WarningCode = None
        }
        return [ ObservationAccepted (obs.Id, decision) ]
    }

// =============================================================================
// Establish Observation (AB‑D‑001)
// =============================================================================
let establishObservation (cmd: EstablishObservationCmd) : Result<ObservationEvent list, DomainError> =
    let obs : DemandObservation = {
        Id = cmd.ObservationId
        SkuId = cmd.SkuId
        StockingPointId = cmd.StockingPointId
        Quantity = cmd.Quantity
        ObservationType = cmd.ObservationType
        BusinessTime = cmd.BusinessTime
        CustomerId = cmd.CustomerId
        PromotionRef = cmd.PromotionRef
        CampaignRef = cmd.CampaignRef
        ContractRef = cmd.ContractRef
        PlanningScopeId = None
        Status = Received
        Decision = None
        Provenance = cmd.Provenance
    }
    Ok [ ObservationEstablished obs ]

// =============================================================================
// Aggregate Decide function (maps ObservationCommand -> Decision)
// =============================================================================
let decide : Decide<DemandObservation, ObservationCommand, ObservationEvent> =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | ObservationCommand.Establish cmd, None ->
            establishObservation cmd
            |> Result.map (fun events ->
                let trace: DecisionTrace = {
                        DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation  // reused; could be own ID
                        CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                        RulesEvaluated = []
                        PolicyId = None
                        SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandObservation ]
                        Rationale = { Summary = "Observation established"; Evidence = []; Alternatives = [] }
                    }
                buildDecision evolve None events (Some trace))

        | ObservationCommand.Evaluate evalCmd, Some obs ->
            // BR‑D‑014: must be in Received
            DemandObservationRules.evaluateOnlyFromReceived obs.Status
            |> Result.bind (fun () -> acceptDemandObservation evalCmd.Signal obs)
            |> Result.map (fun events ->
                let trace = {
                        DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
                        CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                        RulesEvaluated = [
                            ArsIdentifiers.Demand.Rules.signalTimeliness
                            ArsIdentifiers.Demand.Rules.signalQuantityBound
                            ArsIdentifiers.Demand.Rules.signalSourceReliability
                            ArsIdentifiers.Demand.Rules.observationEvaluatedOnce  // BR-D-014
                            ArsIdentifiers.Demand.Rules.decisionExactlyOneOutcome  // BR-D-016
                        ]
                        PolicyId = Some ArsIdentifiers.Demand.Policies.onlyAuthorisedObservations
                        SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandObservation ]
                        Rationale = { Summary = "Observation evaluated"; Evidence = []; Alternatives = [] }
                    }
                buildDecision
                    evolve
                    (Some obs)
                    events
                    (Some trace))

        | _ -> Error(DomainError.validation "Command invalid for current observation state")
