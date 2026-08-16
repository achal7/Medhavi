/// CA-C-020 Aggregate Behaviors
module Medhavi.Core.ExceptionManagement.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Model
open Rules
open Decisions
open Policies

/// AB-C-003: Process Exception Detection Evidence (dedup via deterministic identity).
let processEvidence
    (policy: ExceptionManagementPolicy)
    (currentSeverity: VocabularyEntryId option)
    (cmd: ProcessExceptionEvidenceCmd)
    (state: CoreException option)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    result {
        let input: ProcessInput =
            { Cmd = cmd
              CurrentState = state
              CurrentSeverity = currentSeverity
              Policy = policy }

        let! (decision: DecisionOutcome<EvidenceOutcome>) = Decisions.evaluateExceptionEvidence Rules.registrationRules input

        match decision.Outcome with
        | RejectEvidence reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Decisions.evaluateExceptionEvidence.Id
                    )
                )

        | RegisterNewException ->
            let newException: CoreException =
                { ExceptionIdentifier = cmd.ExceptionId
                  ConstraintReference = cmd.ConstraintReference
                  Classification = cmd.Classification
                  AffectedScopeType = cmd.AffectedScopeType
                  AffectedScopeIdentifier = cmd.AffectedScopeIdentifier
                  EvidenceReference = cmd.EvidenceReference
                  LifecycleState = ExceptionLifecycleState.Active }
            // EV-C-003 carries severity as evidence metadata
            let events = [ ExceptionActivated(newException, cmd.Severity) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.coreExceptionManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Registered"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Registered Exception %A" cmd.ExceptionId
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [ ("UpdateExistingException", "No existing active exception found") ] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.exceptionObject.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Exception state must exist after registration")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }

        | UpdateExistingException effectiveSeverity ->
            // EV-C-004 carries the effective severity; aggregate state updates EvidenceReference only
            let events =
                [ ExceptionUpdated(cmd.ExceptionId, cmd.EvidenceReference, effectiveSeverity, cmd.EvidenceTime) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.coreExceptionManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Updated"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Updated Exception %A" cmd.ExceptionId
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [ ("RegisterNewException", "Existing active exception updated with latest evidence") ] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.exceptionObject.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Exception state must exist after update")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-004: Process Exception Resolution Evidence.
let resolve
    (policy: ExceptionManagementPolicy)
    (cmd: ResolveExceptionCmd)
    (state: CoreException option)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    result {
        let input: ResolveInput = { Cmd = cmd; CurrentState = state }
        let! (decision: DecisionOutcome<ResolutionOutcome>) = Decisions.evaluateExceptionResolution Rules.resolutionRules input

        match decision.Outcome with
        | RejectResolution reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Decisions.evaluateExceptionResolution.Id
                    )
                )
        | ResolveException ->
            // EV-C-005
            let events = [ ExceptionResolved(cmd.ExceptionId, cmd.ResolutionTime, cmd.ResolutionEvidence) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.coreExceptionManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Resolved"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Resolved Exception %A" cmd.ExceptionId
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [ ("RejectResolution", "Resolution criteria met") ] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.exceptionObject.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Exception state must exist after resolution")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// Unified decide. currentSeverity is injected by the FS (read-model lookup).
let decide
    (policy: ExceptionManagementPolicy)
    (currentSeverity: VocabularyEntryId option)
    (cmd: ExceptionCmd)
    (state: CoreException option)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    match cmd with
    | ProcessEvidence c -> processEvidence policy currentSeverity c state
    | Resolve c -> resolve policy c state
