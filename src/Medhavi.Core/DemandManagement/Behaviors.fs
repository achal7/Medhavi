module Medhavi.Core.DemandManagement.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Core
open Model
open Rules
open Decisions
open Policies

/// AB-C-009: Record Demand
let record (policy: DemandManagementPolicy) : Decide<Demand, RecordDemandCmd, DemandEvent> =
    fun (cmd: RecordDemandCmd) (state: Demand option) ->
        result {
            let input: RecordInput =
                { Cmd = cmd
                  CurrentState = state
                  AllowDuplicate = policy.AllowDuplicateRecording }

            let! decision = Decisions.evaluateRecording Rules.recordingRules input

            match decision.Outcome with
            | RejectRecording reasons ->
                return!
                    Error(
                        DomainError.rule(
                            (String.concat "; " reasons),
                            ArsIdentifiers.Decisions.evaluateDemandRecording.Id
                        )
                    )
            | RecordDemand ->
                let newDemand: Demand =
                    { Id = cmd.DemandId
                      Item = cmd.Item
                      Location = cmd.Location
                      Customer = cmd.Customer
                      Quantity = cmd.Quantity
                      NeedWindow = cmd.NeedWindow
                      DemandOrigin = cmd.DemandOrigin
                      ParentDemand = cmd.ParentDemand
                      LifecycleState = DemandLifecycleState.Active }

                let events = [ DemandRecorded newDemand ]

                let! newState =
                    events |> List.fold evolve state |> Result.ofOption(DomainError.invariant "State must exist")

                let trace: DecisionTrace =
                    { DecisionId = Guid.NewGuid().ToString()
                      CapabilityId = ArsIdentifiers.Capabilities.manageEnterpriseDemand.Id
                      CausalDecisionIds = []
                      Outcome = "Recorded"
                      PolicyId = Some policy.PolicyId
                      PolicyVersion = Some policy.Version
                      Rationale =
                        { Summary = sprintf "Recorded demand %s" (DemandId.value cmd.DemandId)
                          Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                          Alternatives = [] }
                      RulesEvaluated = decision.Evaluations
                      SemanticObjectIds = [ ArsIdentifiers.SemanticObjects.demandObject.Id ] }

                return
                    { NewState = newState
                      Events = events
                      Trace = Some trace }
        }

/// AB-C-010: Satisfy Demand
let satisfy: Decide<Demand, SatisfyDemandCmd, DemandEvent> =
    fun (cmd: SatisfyDemandCmd) (state: Demand option) ->
        result {
            let input: SatisfyInput = { Cmd = cmd; CurrentState = state }
            let! decision = Decisions.evaluateSatisfaction Rules.satisfactionRules input

            match decision.Outcome with
            | RejectSatisfaction reasons ->
                return!
                    Error(
                        DomainError.rule(
                            (String.concat "; " reasons),
                            ArsIdentifiers.Decisions.evaluateDemandSatisfaction.Id
                        )
                    )
            | SatisfyDemand ->
                let events = [ DemandSatisfied(cmd.DemandId, cmd.SatisfactionTime) ]

                let! newState =
                    events |> List.fold evolve state |> Result.ofOption(DomainError.invariant "State must exist")

                let trace: DecisionTrace =
                    { DecisionId = Guid.NewGuid().ToString()
                      CapabilityId = ArsIdentifiers.Capabilities.manageEnterpriseDemand.Id
                      CausalDecisionIds = []
                      Outcome = "Satisfied"
                      PolicyId = None
                      PolicyVersion = None
                      Rationale =
                        { Summary = sprintf "Satisfied demand %s" (DemandId.value cmd.DemandId)
                          Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                          Alternatives = [] }
                      RulesEvaluated = decision.Evaluations
                      SemanticObjectIds = [ ArsIdentifiers.SemanticObjects.demandObject.Id ] }

                return
                    { NewState = newState
                      Events = events
                      Trace = Some trace }
        }

/// AB-C-011: Cancel Demand
let cancel: Decide<Demand, CancelDemandCmd, DemandEvent> =
    fun (cmd: CancelDemandCmd) (state: Demand option) ->
        result {
            let input: CancelInput = { Cmd = cmd; CurrentState = state }
            let! decision = Decisions.evaluateCancellation Rules.cancellationRules input

            match decision.Outcome with
            | RejectCancellation reasons ->
                return!
                    Error(
                        DomainError.rule(
                            (String.concat "; " reasons),
                            ArsIdentifiers.Decisions.evaluateDemandCancellation.Id
                        )
                    )
            | CancelDemand ->
                let events = [ DemandCancelled(cmd.DemandId, cmd.CancellationTime, cmd.Reason) ]

                let! newState =
                    events |> List.fold evolve state |> Result.ofOption(DomainError.invariant "State must exist")

                let trace: DecisionTrace =
                    { DecisionId = Guid.NewGuid().ToString()
                      CapabilityId = ArsIdentifiers.Capabilities.manageEnterpriseDemand.Id
                      CausalDecisionIds = []
                      Outcome = "Cancelled"
                      PolicyId = None
                      PolicyVersion = None
                      Rationale =
                        { Summary = sprintf "Cancelled demand %s" (DemandId.value cmd.DemandId)
                          Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                          Alternatives = [] }
                      RulesEvaluated = decision.Evaluations
                      SemanticObjectIds = [ ArsIdentifiers.SemanticObjects.demandObject.Id ] }

                return
                    { NewState = newState
                      Events = events
                      Trace = Some trace }
        }
