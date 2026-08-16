/// CA-C-020 Exception Management Aggregate Behaviors
module Medhavi.Core.ExceptionManagement.Behaviors

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Core
open Model
open Rules
open Decisions
open Policies

/// AB-C-020a: Register Exception Behavior
let register
    (policy: ExceptionManagementPolicy)
    (cmd: RegisterExceptionCmd)
    (state: CoreException option)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    result {
        let input: RegisterInput =
            { Cmd = cmd
              CurrentState = state
              Policy = policy }

        let! decision = Decisions.decideRegistration Rules.registrationRules input

        match decision.Outcome with
        | RegistrationRejected reasons ->
            return!
                Error(DomainError.rule((String.concat "; " reasons), ArsIdentifiers.Decisions.decideRegistration.Id))

        | RegisteredSuccessfully ->
            let newException: CoreException =
                { ExceptionIdentifier = cmd.ExceptionId
                  ConstraintReference = cmd.ConstraintReference
                  Classification = cmd.Classification
                  AffectedScopeType = cmd.AffectedScopeType
                  AffectedScopeIdentifier = cmd.AffectedScopeIdentifier
                  EvidenceReference = cmd.EvidenceReference
                  Severity = cmd.Severity
                  LifecycleState = ExceptionLifecycleState.Active }

            let events = [ ExceptionRegistered newException ]
            let evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = ArsIdentifiers.Capabilities.coreExceptionManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Succeeded"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Registered Exception %A" cmd.ExceptionId
                      Evidence = evidence
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ ArsIdentifiers.SemanticObjects.exceptionObject.Id ] }

            // Safe state evolution without failwith
            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(
                    DomainError.invariant "Exception state must exist after applying registration events"
                )

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-020b: Resolve Exception Behavior
let resolve
    (cmd: ResolveExceptionCmd)
    (state: CoreException option)
    (policy: Policies.ExceptionManagementPolicy)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    result {
        let input: ResolveInput = { Cmd = cmd; CurrentState = state }
        let! decision = Decisions.decideResolution Rules.resolutionRules input

        match decision.Outcome with
        | ResolutionRejected reasons ->
            return! Error(DomainError.rule((String.concat "; " reasons), ArsIdentifiers.Decisions.decideResolution.Id))

        | ResolvedSuccessfully ->
            let events = [ ExceptionResolved(cmd.ExceptionId, cmd.ResolutionTime, cmd.ResolutionEvidence) ]
            let evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = ArsIdentifiers.Capabilities.coreExceptionManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Succeeded"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Resolved Exception %A" cmd.ExceptionId
                      Evidence = evidence
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ ArsIdentifiers.SemanticObjects.exceptionObject.Id ] }

            // Safe state evolution without failwith
            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "Exception state must exist after applying resolution events")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// Unified decide function with injected policy
let decide
    (policy: ExceptionManagementPolicy)
    (cmd: ExceptionCmd)
    (state: CoreException option)
    : Result<Decision<CoreException, ExceptionEvent>, DomainError> =
    match cmd with
    | Register registerCmd -> register policy registerCmd state
    | Resolve resolveCmd -> resolve resolveCmd state policy
