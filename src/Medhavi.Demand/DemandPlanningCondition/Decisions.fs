module Medhavi.Demand.DemandPlanningCondition.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandPlanningCondition.Model
open Medhavi.Demand.DemandPlanningCondition.Rules

let recognize (cmd: RecognizeConditionCmd) : Result<DemandPlanningConditionEvent list, DomainError> =
    result {
        let! _ = detectionThresholdsMet cmd.DetectionEvidence

        let changeEvent: ConditionChangeEvent =
            { EventId = System.Guid.NewGuid().ToString()
              Timestamp = cmd.DetectionTimestamp
              EventType = "Detected"
              CurrentStateAfterEvent = Active
              PreviousSeverity = None
              NewSeverity = cmd.NewSeverity
              Evidence = cmd.DetectionEvidence
              PolicyVersionRef = cmd.PolicyVersionRef }

        let condition: DemandPlanningCondition =
            { Id = cmd.ConditionId
              PlanningEntity = cmd.PlanningEntity
              ConditionType = cmd.ConditionType
              CurrentStatus = Active
              Severity = cmd.NewSeverity
              DetectionEvidence = cmd.DetectionEvidence
              DetectionTimestamp = cmd.DetectionTimestamp
              ResolutionTimestamp = None
              ResolutionEvidence = None
              LastChangeEvent = Some changeEvent
              BusinessTime = cmd.BusinessTime
              TransactionTime = cmd.DetectionTimestamp }

        return [ ConditionRecognized(condition, changeEvent) ]
    }

let resolve
    (cmd: ResolveConditionCmd)
    (state: DemandPlanningCondition)
    : Result<DemandPlanningConditionEvent list, DomainError> =
    result {
        let! _ = activeOrResolved state.CurrentStatus
        let! _ = resolvedIsTerminal state.CurrentStatus

        let changeEvent: ConditionChangeEvent =
            { EventId = System.Guid.NewGuid().ToString()
              Timestamp = cmd.ResolutionTimestamp
              EventType = "Resolved"
              CurrentStateAfterEvent = Resolved
              PreviousSeverity = Some state.Severity
              NewSeverity = state.Severity
              Evidence = cmd.ResolutionEvidence
              PolicyVersionRef = "v1" }

        let resolvedCondition =
            { state with
                CurrentStatus = Resolved
                ResolutionTimestamp = Some cmd.ResolutionTimestamp
                ResolutionEvidence = Some cmd.ResolutionEvidence
                LastChangeEvent = Some changeEvent
                TransactionTime = cmd.ResolutionTimestamp }

        return [ ConditionResolved(resolvedCondition, changeEvent) ]
    }

let decide
    (cmd: DemandPlanningConditionCommand)
    (stateOpt: DemandPlanningCondition option)
    : Result<Decision<DemandPlanningCondition, DemandPlanningConditionEvent>, DomainError> =
    match cmd, stateOpt with
    | Recognize cmd, None ->
        recognize cmd
        |> Result.map(fun events ->
            //let derivedState = events |> List.fold (fun acc e -> evolve e acc) stateOpt
            let trace =
                { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandPlanningCondition
                  CapabilityId = ArsIdentifiers.Demand.Capabilities.detectDemandExceptions
                  RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.exceptionDetectionThresholds, 1) ]
                  PolicyId = Some ArsIdentifiers.Demand.Policies.exceptionDetectionPolicyGoverned
                  PolicyVersion = Some 1
                  SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandPlanningCondition ]
                  Rationale =
                    { Summary =
                        $"Condition {cmd.ConditionType} detected for {cmd.PlanningEntity} with severity {cmd.NewSeverity}."
                      Evidence = [ cmd.DetectionEvidence ]
                      Alternatives = [] } }

            buildDecision evolve stateOpt events (Some trace))
    | Resolve cmd, Some state ->
        resolve cmd state
        |> Result.map(fun events ->
            let trace =
                { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandPlanningCondition
                  CapabilityId = ArsIdentifiers.Demand.Capabilities.detectDemandExceptions
                  RulesEvaluated =
                    [ (ArsIdentifiers.Demand.Rules.conditionActiveOrResolved, 1)
                      (ArsIdentifiers.Demand.Rules.resolvedConditionTerminal, 1) ]
                  PolicyId = None
                  PolicyVersion = None
                  SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandPlanningCondition ]
                  Rationale =
                    { Summary = $"Condition {state.ConditionType} resolved for {state.PlanningEntity}."
                      Evidence = [ cmd.ResolutionEvidence ]
                      Alternatives = [] } }

            buildDecision evolve (Some state) events (Some trace))
    | _ -> Error(DomainError.validation "Command invalid for current state")
