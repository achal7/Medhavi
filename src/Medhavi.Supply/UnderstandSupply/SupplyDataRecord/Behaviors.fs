module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map(fun _ -> ArsIdentifiers.Policies.supplyDataAcceptancePolicy.Id)
    let policyVersion = policy |> Option.map(fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        ArsIdentifiers.Capabilities.understandSupply.Id
        decisionOutcome
        policyId
        policyVersion
        [ ArsIdentifiers.SemanticObjects.supplyDataRecord.Id ]
        (Some summary)

/// AB-S-001: Receive Supply Data
let receive: Decide<SupplyDataRecord, ReceiveRecordCmd, RecordEvent> =
    fun (cmd: ReceiveRecordCmd) (state: SupplyDataRecord option) ->
        result {
            match state with
            | Some _ ->
                return! Error(DomainError.conflict("Supply data record already exists", ArsIdentifiers.Capabilities.understandSupply.Id))
            | None ->
                let newRecord: SupplyDataRecord =
                    { RecordId = cmd.RecordId
                      Item = cmd.Item
                      Location = cmd.Location
                      SupplyType = cmd.SupplyType
                      Quantity = cmd.Quantity
                      UnitOfMeasure = cmd.UnitOfMeasure
                      BatchIdentifier = cmd.BatchIdentifier
                      OrderIdentifier = cmd.OrderIdentifier
                      CommitmentIdentifier = cmd.CommitmentIdentifier
                      SupplierIdentifier = cmd.SupplierIdentifier
                      ExpectedDeliveryDate = cmd.ExpectedDeliveryDate
                      BusinessTime = cmd.BusinessTime
                      ObservationTime = cmd.ObservationTime
                      SourceSystemProvenance = cmd.SourceSystemProvenance
                      LifecycleState = Received
                      DecisionTraceability = None }

                let events = [ RecordReceived newRecord ]
                let decision = { Outcome = Receive; Evaluations = [] }

                return buildTrace None ArsIdentifiers.Capabilities.understandSupply.Id decision None events "Supply data record received"
        }

/// AB-S-002: Evaluate Supply Data
let evaluate
    (policy: SupplyDataAcceptancePolicy)
    (sourceReliability: decimal)
    (hasDuplicate: bool)
    : Decide<SupplyDataRecord, EvaluateRecordCmd, RecordEvent> =
    fun (cmd: EvaluateRecordCmd) (state: SupplyDataRecord option) ->
        result {
            let input: EvaluateInput =
                { Cmd = cmd
                  CurrentState = state
                  Policy = policy
                  SourceReliability = sourceReliability
                  HasDuplicateInWindow = hasDuplicate }

            let! decision = evaluateSupplyData evaluationRules input

            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | Accept ->
                let events = [ RecordAccepted(cmd.RecordId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events "Supply data record accepted"

            | Quarantine reasons ->
                let events = [ RecordQuarantined(cmd.RecordId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events (String.concat "; " reasons)

            | Reject reasons ->
                let events = [ RecordRejected(cmd.RecordId, cmd.EvaluationTime, traceId) ]
                return buildTrace (Some policy) traceId decision state events (String.concat "; " reasons)
        }
