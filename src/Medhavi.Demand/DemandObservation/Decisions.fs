module Medhavi.Demand.DemandObservation.Decisions

open System
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.Model

// =============================================================================
// DE‑D‑010 — Accept Demand Observation
// =============================================================================

let calculateConfidence (signal: DemandSignal option) : decimal =
    match signal with
    | None -> 1.0M // Standard observation types default to 1.0M (fully trusted)
    | Some s ->
        let reliabilityFactor = s.SourceReliability / 100.0M

        let deviationFactor =
            if s.StatisticalBound > 0.0M then
                let dev = abs(s.Value - s.RecentBaseline) / s.StatisticalBound
                1.0M - dev
            else
                1.0M

        let rawConfidence = reliabilityFactor * deviationFactor
        Math.Max(0.0M, Math.Min(1.0M, rawConfidence))

let acceptDemandObservation
    (signal: DemandSignal option)
    (obs: DemandObservation)
    : Result<ObservationEvent list, DomainError> =

    let confidence = calculateConfidence signal

    match signal with
    | None ->
        let decision: ObservationDecision =
            { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
              Timestamp = Timestamp.now
              Confidence = confidence
              Rationale = "Standard business observation automatically accepted."
              WarningCode = None }

        Ok [ ObservationAccepted(obs.Id, decision) ]

    | Some s ->
        // Lift rules into Validation Functor to evaluate them in parallel and accumulate errors
        let timelinessVal = Rules.signalTimeliness s (TimeSpan.FromHours 1.0) |> fromResult
        let reliabilityVal = Rules.signalSourceReliability s 60.0M |> fromResult
        let rangeVal = Rules.signalRange s |> fromResult

        let combinedValidation = (fun _ _ _ -> ()) <!> timelinessVal <*> reliabilityVal <*> rangeVal

        match combinedValidation with
        | Valid() ->
            let confidence = calculateConfidence(Some s)
            let warningCode = if confidence < 0.6M then Some "LOW_CONFIDENCE" else None

            let decision: ObservationDecision =
                { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
                  Timestamp = Timestamp.now
                  Confidence = confidence
                  Rationale = "Observation evaluated and accepted."
                  WarningCode = warningCode }

            match warningCode with
            | None -> Ok [ ObservationAccepted(obs.Id, decision) ]
            | Some w -> Ok [ ObservationWarningRecorded(obs.Id, w, decision) ]

        | Invalid errors ->
            let rationaleMsg = errors |> List.map(fun (err: DomainError) -> err.Message) |> String.concat "; "

            let decision: ObservationDecision =
                { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
                  Timestamp = Timestamp.now
                  Confidence = confidence
                  Rationale = rationaleMsg
                  WarningCode = None }

            let isRangeViolation (err: DomainError) = err.Message.Contains("outside") && err.Message.Contains("bound")

            if errors |> List.exists isRangeViolation then
                Ok [ ObservationRejected(obs.Id, decision) ]
            else
                Ok [ ObservationQuarantined(obs.Id, decision) ]

// =============================================================================
// Establish Observation (AB‑D‑001)
// =============================================================================
let establishObservation (cmd: EstablishObservationCmd) : Result<ObservationEvent list, DomainError> =
    let obs: DemandObservation =
        { Id = cmd.ObservationId
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
          Provenance = cmd.Provenance }

    Ok [ ObservationEstablished obs ]

let assignScope (cmd: AssignScopeCmd) (obs: DemandObservation) : Result<ObservationEvent list, DomainError> =
    Rules.mustBeAcceptedAndUnassigned obs.Status obs.PlanningScopeId
    |> Result.map(fun () -> [ ObservationScopeAssigned(obs.Id, cmd.PlanningScopeId) ])

// =============================================================================
// Aggregate Decide function (maps ObservationCommand -> Decision)
// =============================================================================
let decide: Decide<DemandObservation, ObservationCommand, ObservationEvent> =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | Establish cmd, None ->
            establishObservation cmd
            |> Result.map(fun events ->
                let trace: DecisionTrace =
                    { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation // reused; could be own ID
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                      RulesEvaluated = []
                      PolicyId = None
                      PolicyVersion = None
                      SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandObservation ]
                      Rationale =
                        { Summary = "Observation established"
                          Evidence = []
                          Alternatives = [] } }

                buildDecision evolve None events (Some trace))

        | Establish cmd, Some obs ->
            // BR-D-001 Idempotent: observation already exists, return no new events.
            Ok(
                buildDecision
                    evolve
                    (Some obs)
                    []
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                          RulesEvaluated = []
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandObservation ]
                          Rationale =
                            { Summary = "Observation already exists"
                              Evidence = []
                              Alternatives = [] } })
            )

        | Evaluate evalCmd, Some obs ->
            // BR‑D‑014: must be in Received
            Rules.evaluateOnlyFromReceived obs.Status
            |> Result.bind(fun () -> acceptDemandObservation evalCmd.Signal obs)
            |> Result.map(fun events ->
                let trace =
                    { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                      RulesEvaluated =
                        [ (ArsIdentifiers.Demand.Rules.signalTimeliness, 1)
                          (ArsIdentifiers.Demand.Rules.signalQuantityBound, 1)
                          (ArsIdentifiers.Demand.Rules.signalSourceReliability, 1)
                          (ArsIdentifiers.Demand.Rules.observationEvaluatedOnce, 1) // BR-D-014
                          (ArsIdentifiers.Demand.Rules.decisionExactlyOneOutcome, 1) ] // BR-D-016
                      PolicyId = Some ArsIdentifiers.Demand.Policies.onlyAuthorisedObservations
                      PolicyVersion = Some 1
                      SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandObservation ]
                      Rationale =
                        { Summary = "Observation evaluated"
                          Evidence = []
                          Alternatives = [] } }

                buildDecision evolve (Some obs) events (Some trace))
        | AssignScope cmd, Some obs ->
            assignScope cmd obs
            |> Result.map(fun events ->
                let trace =
                    { DecisionId = ArsIdentifiers.Demand.Decisions.acceptDemandObservation // or a dedicated ID; using the evaluation ID for simplicity
                      CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                      RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.acceptedBelongsToExactlyOneScope, 1) ]
                      PolicyId = None
                      PolicyVersion = None
                      SemanticObjectIds =
                        [ ArsIdentifiers.Demand.SemanticObjects.demandObservation
                          ArsIdentifiers.Demand.SemanticObjects.planningScope ]
                      Rationale =
                        { Summary = "Scope assigned"
                          Evidence = []
                          Alternatives = [] } }

                buildDecision evolve (Some obs) events (Some trace))
        | _ -> Error(DomainError.validation "Command invalid for current observation state")
