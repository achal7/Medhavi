/// AB-S-040 — Record Supplier Commitment Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-040, EV-S-040)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Rules
open Decisions

type SupplierEvaluationFactsProvider = SupplierId -> ItemId -> ResponseEvaluationFacts

type CommitmentOutcome =
    | RequestRecorded of commitmentId: SupplierCommitmentId * sequence: int
    | ResponseRecorded of commitmentId: SupplierCommitmentId * state: SupplierCommitmentState * outcome: SupplierResponseAcceptanceOutcome

/// AB-S-040: Record initial enterprise request
let recordRequest
    : Decide<SupplierCommitment, RecordEnterpriseRequestCmd, SupplierCommitmentEvent> =
    fun (cmd: RecordEnterpriseRequestCmd) (stateOpt: SupplierCommitment option) ->
        result {
            do! validateIdentity cmd.SupplierId cmd.ItemId cmd.RequiredDate

            let commitmentId = SupplierCommitmentId.ofComposite cmd.SupplierId cmd.ItemId cmd.RequiredDate
            let currentSeq =
                match stateOpt with
                | None -> 0
                | Some c -> c.CollaborationEvents.Length

            let nextSeq = currentSeq + 1
            let history = match stateOpt with Some c -> c.CollaborationEvents | None -> []
            do! validateNextSequenceNumber history nextSeq

            let reqDateStr = (Timestamp.value cmd.RequestedDate).ToString("yyyy-MM-dd")
            let childEvent: CollaborationEvent =
                { SequenceNumber = nextSeq
                  EventType = CollaborationEventType.EnterpriseRequest
                  EventTimestamp = cmd.RequestTimestamp
                  Actor = CollaborationActor.Enterprise
                  Details = $"Requested {Quantity.value cmd.RequestedQuantity} units by {reqDateStr}" }

            let commitment: SupplierCommitment =
                { CommitmentId = commitmentId
                  SupplierId = cmd.SupplierId
                  ItemId = cmd.ItemId
                  RequiredDate = cmd.RequiredDate
                  RequestedQuantity = cmd.RequestedQuantity
                  RequestedDate = cmd.RequestedDate
                  RequestTimestamp = cmd.RequestTimestamp
                  CurrentState = SupplierCommitmentState.Proposed
                  CommittedQuantity = None
                  CommittedDate = None
                  ResponseTimestamp = None
                  Confidence = CommitmentConfidenceLevel.Estimated
                  AlternativeDescription = None
                  AlternativeQuantity = None
                  AlternativeDate = None
                  SupplierAssumptions = []
                  CollaborationEvents = [ childEvent ]
                  AcceptanceOutcome = None
                  DataQualityFlag = None }

            let events = [ SupplierCommitmentChanged(commitment, childEvent) ]
            let outcome = RequestRecorded(commitmentId, nextSeq)
            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.acceptSupplierResponse.Id
                    []
                    ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.supplierCollaborationPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.supplierCommitment.Id ]
                    (Some $"Enterprise request recorded for {SupplierId.value cmd.SupplierId}: {Quantity.value cmd.RequestedQuantity} units")
        }

/// AB-S-040: Record supplier response and evaluate acceptance via DE-S-041
let recordResponse
    (factsProvider: SupplierEvaluationFactsProvider)
    : Decide<SupplierCommitment, RecordSupplierResponseCmd, SupplierCommitmentEvent> =
    fun (cmd: RecordSupplierResponseCmd) (stateOpt: SupplierCommitment option) ->
        result {
            match stateOpt with
            | None ->
                return! Error(DomainError.notFound ("SupplierCommitment", (ItemId.value cmd.ItemId), $"{SupplierId.value cmd.SupplierId}#{ItemId.value cmd.ItemId}"))
            | Some current ->
                do! validateUpdateEligibility (Some current.CurrentState)

                let nextSeq = current.CollaborationEvents.Length + 1
                do! validateNextSequenceNumber current.CollaborationEvents nextSeq

                let facts = factsProvider cmd.SupplierId cmd.ItemId

                let evaluation =
                    acceptResponse
                        cmd.ResponseEventType
                        current.RequestedQuantity
                        cmd.CommittedQuantity
                        current.RequestedDate
                        cmd.CommittedDate
                        facts

                let details =
                    match cmd.ResponseEventType with
                    | CollaborationEventType.SupplierCommitment ->
                        $"Committed: Qty={cmd.CommittedQuantity}, Date={cmd.CommittedDate}"
                    | CollaborationEventType.SupplierRejection ->
                        "Supplier rejected requirement"
                    | CollaborationEventType.SupplierAlternative ->
                        $"Alternative proposed: {cmd.AlternativeDescription}"
                    | CollaborationEventType.SupplierAcknowledgement ->
                        "Supplier acknowledged receipt"
                    | _ -> "Supplier interaction recorded"

                let childEvent: CollaborationEvent =
                    { SequenceNumber = nextSeq
                      EventType = cmd.ResponseEventType
                      EventTimestamp = cmd.ResponseTimestamp
                      Actor = CollaborationActor.Supplier
                      Details = details }

                let updatedCommitment: SupplierCommitment =
                    { current with
                        CurrentState = evaluation.NewState
                        CommittedQuantity = cmd.CommittedQuantity |> Option.orElse current.CommittedQuantity
                        CommittedDate = cmd.CommittedDate |> Option.orElse current.CommittedDate
                        ResponseTimestamp = Some cmd.ResponseTimestamp
                        Confidence = evaluation.Confidence
                        AlternativeDescription = cmd.AlternativeDescription |> Option.orElse current.AlternativeDescription
                        AlternativeQuantity = cmd.AlternativeQuantity |> Option.orElse current.AlternativeQuantity
                        AlternativeDate = cmd.AlternativeDate |> Option.orElse current.AlternativeDate
                        SupplierAssumptions = if cmd.SupplierAssumptions.IsEmpty then current.SupplierAssumptions else cmd.SupplierAssumptions
                        CollaborationEvents = childEvent :: current.CollaborationEvents
                        AcceptanceOutcome = Some evaluation.AcceptanceOutcome
                        DataQualityFlag = evaluation.DataQualityFlag }

                let events = [ SupplierCommitmentChanged(updatedCommitment, childEvent) ]
                let outcome = ResponseRecorded(current.CommitmentId, evaluation.NewState, evaluation.AcceptanceOutcome)
                let traceOutcome = { Outcome = outcome; Evaluations = [] }

                return
                    buildDecisionWithTrace
                        evolve
                        stateOpt
                        events
                        ArsIdentifiers.Decisions.acceptSupplierResponse.Id
                        []
                        ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                        traceOutcome
                        (Some ArsIdentifiers.Policies.supplierCollaborationPolicy.Id)
                        (Some 1)
                        [ ArsIdentifiers.SemanticObjects.supplierCommitment.Id ]
                        (Some $"Supplier response recorded: State={evaluation.NewState}, Outcome={evaluation.AcceptanceOutcome}")
        }
