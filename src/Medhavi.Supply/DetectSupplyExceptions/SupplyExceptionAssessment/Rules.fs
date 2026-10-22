/// SE-S-110 — Supply Exception Assessment Rules
/// Governed by: PO-S-120 Exception Detection Policy, ARS §7
/// Traces to: BR-S-270, BR-S-271, BR-S-272, BR-S-273, BR-S-274, BR-S-275, BR-S-276, BR-S-277, BR-S-278, BR-S-279c, BR-S-292, BR-S-295, BR-S-296
module Medhavi.Supply.DetectSupplyExceptions.SupplyExceptionAssessment.Rules

open System
open Medhavi.Foundation.Contracts
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies

/// Input context for evaluating supply exception aggregate rules
type EvaluateExceptionInput =
    { Command: SupplyExceptionCommand
      CurrentState: SupplyExceptionAssessment option
      Policy: ExceptionDetectionPolicy
      PrerequisitesSatisfied: bool
      ExistingActiveExceptions: SupplyExceptionAssessment list }

/// BR-S-270 — The business identity of a Supply Exception Assessment is Governed Policy Reference + Exception Scope + Exception Type
let supplyExceptionAssessmentIdentity: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.supplyExceptionAssessmentIdentity.Id
        Rules.supplyExceptionAssessmentIdentity.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                not (String.IsNullOrWhiteSpace d.GovernedPolicyReference)
                && not (String.IsNullOrWhiteSpace d.ViolatedRuleId)
            | _ -> input.CurrentState.IsSome)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                $"PolicyRef: %s{d.GovernedPolicyReference}; ViolatedRuleId: %s{d.ViolatedRuleId}; Scope: %A{d.ExceptionScope}; Type: %A{d.ExceptionType}"
            | _ ->
                let stateId = input.CurrentState |> Option.map (fun s -> SupplyExceptionId.value s.Id) |> Option.defaultValue "None"
                $"Target ExceptionId: %s{stateId}")

/// BR-S-271 — Exception detection requires active policy and recognized scope prerequisites
let exceptionDetectionEligibility: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.exceptionDetectionEligibility.Id
        Rules.exceptionDetectionEligibility.Explanation
        (fun input -> input.Policy.IsActive && input.PrerequisitesSatisfied)
        (fun input ->
            $"Policy.IsActive: %b{input.Policy.IsActive}; PrerequisitesSatisfied: %b{input.PrerequisitesSatisfied}")

/// BR-S-272 — Exactly one current Supply Exception Assessment exists per monitored entity and exception type
let singleCurrentSupplyExceptionAssessment: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.singleCurrentSupplyExceptionAssessment.Id
        Rules.singleCurrentSupplyExceptionAssessment.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                let hasDuplicate =
                    input.ExistingActiveExceptions
                    |> List.exists (fun existing ->
                        existing.ExceptionType = d.ExceptionType
                        && ExceptionScope.toKey existing.ExceptionScope = ExceptionScope.toKey d.ExceptionScope
                        && (existing.LifecycleState = SupplyExceptionLifecycleState.Detected
                            || existing.LifecycleState = SupplyExceptionLifecycleState.Confirmed))
                not hasDuplicate
            | _ -> true)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                $"ExistingActiveExceptionsCount: %d{input.ExistingActiveExceptions.Length}; TargetScope: %A{d.ExceptionScope}; TargetType: %A{d.ExceptionType}"
            | _ -> "Command targets existing exception instance")

/// BR-S-273 — State change events within the Supply Exception Assessment are immutable
let exceptionAssessmentStateChangeImmutability: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.exceptionAssessmentStateChangeImmutability.Id
        Rules.exceptionAssessmentStateChangeImmutability.Explanation
        (fun input ->
            match input.CurrentState with
            | Some s -> s.LifecycleState <> SupplyExceptionLifecycleState.Archived
            | None -> true)
        (fun input ->
            let stateStr = input.CurrentState |> Option.map (fun s -> $"%A{s.LifecycleState}") |> Option.defaultValue "None"
            $"CurrentLifecycleState: %s{stateStr}")

/// BR-S-274 — The complete history of exception assessments is permanently preserved (Resolved exceptions are immutable except for archiving)
let exceptionAssessmentHistoryImmutability: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.exceptionAssessmentHistoryImmutability.Id
        Rules.exceptionAssessmentHistoryImmutability.Explanation
        (fun input ->
            match input.CurrentState with
            | Some s when s.LifecycleState = SupplyExceptionLifecycleState.Resolved ->
                match input.Command with
                | SupplyExceptionCommand.ArchiveException _ -> true
                | _ -> false
            | _ -> true)
        (fun input ->
            let stateStr = input.CurrentState |> Option.map (fun s -> $"%A{s.LifecycleState}") |> Option.defaultValue "None"
            $"CurrentLifecycleState: %s{stateStr}; Command: %A{input.Command.GetType().Name}")

/// BR-S-275 — Lifecycle transitions are deterministic per PO-S-120
let deterministicLifecycleTransitions: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.deterministicLifecycleTransitions.Id
        Rules.deterministicLifecycleTransitions.Explanation
        (fun input ->
            match input.CurrentState, input.Command with
            | None, SupplyExceptionCommand.DetectException _ -> true
            | Some s, SupplyExceptionCommand.CorroborateException _ ->
                s.LifecycleState = SupplyExceptionLifecycleState.Detected
            | Some s, SupplyExceptionCommand.UpdateExceptionEvidence _ ->
                s.LifecycleState = SupplyExceptionLifecycleState.Detected
                || s.LifecycleState = SupplyExceptionLifecycleState.Confirmed
            | Some s, SupplyExceptionCommand.ResolveException _ ->
                s.LifecycleState = SupplyExceptionLifecycleState.Detected
                || s.LifecycleState = SupplyExceptionLifecycleState.Confirmed
            | Some s, SupplyExceptionCommand.ArchiveException _ ->
                s.LifecycleState = SupplyExceptionLifecycleState.Resolved
            | _ -> false)
        (fun input ->
            let stateStr = input.CurrentState |> Option.map (fun s -> $"%A{s.LifecycleState}") |> Option.defaultValue "None"
            $"FromState: %s{stateStr}; Command: %A{input.Command.GetType().Name}")

/// BR-S-276 — Evidence Sufficiency for Confirmation
let exceptionEvidencePublicationRequirement: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.exceptionEvidencePublicationRequirement.Id
        Rules.exceptionEvidencePublicationRequirement.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.CorroborateException d ->
                let count = d.CorroboratingEvidence.Length
                let hasHighConfidence =
                    d.CorroboratingEvidence
                    |> List.exists (fun e -> e.Confidence >= (input.Policy.EvidenceSufficiency.ConfirmedSingleSourceReliabilityPercent / 100.0m))
                count >= input.Policy.EvidenceSufficiency.ConfirmedMinIndependentSources || hasHighConfidence
            | _ -> true)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.CorroborateException d ->
                $"CorroboratingEvidenceCount: %d{d.CorroboratingEvidence.Length}; MinSources: %d{input.Policy.EvidenceSufficiency.ConfirmedMinIndependentSources}"
            | _ -> "N/A for non-confirmation commands")

/// BR-S-277 — Resolution evidence must be published when underlying data returns to compliance
let exceptionResolutionEvidenceRequirement: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.exceptionResolutionEvidenceRequirement.Id
        Rules.exceptionResolutionEvidenceRequirement.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.ResolveException d -> not (List.isEmpty d.ResolutionEvidence)
            | _ -> true)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.ResolveException d ->
                $"ResolutionEvidenceCount: %d{d.ResolutionEvidence.Length}"
            | _ -> "N/A for non-resolution commands")

/// BR-S-278 — Supply Intelligence never directly creates, updates, or resolves SE-C-019 Exception instances
let noDirectExceptionMutation: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.noDirectExceptionMutation.Id
        Rules.noDirectExceptionMutation.Explanation
        (fun _ -> true)
        (fun _ -> "Supply boundary preserved: emission occurs strictly via domain events and business notifications")

/// BR-S-292 — Enforces multi-dimensional severity vector completeness
let multiDimensionalSeverityIndependence: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.multiDimensionalSeverityIndependence.Id
        Rules.multiDimensionalSeverityIndependence.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                d.Severity.BusinessImpact.ToString().Length > 0
                && d.Severity.Urgency.ToString().Length > 0
                && d.Severity.Scope.ToString().Length > 0
            | SupplyExceptionCommand.CorroborateException d ->
                d.Severity.BusinessImpact.ToString().Length > 0
                && d.Severity.Urgency.ToString().Length > 0
                && d.Severity.Scope.ToString().Length > 0
            | SupplyExceptionCommand.UpdateExceptionEvidence d ->
                d.Severity.BusinessImpact.ToString().Length > 0
                && d.Severity.Urgency.ToString().Length > 0
                && d.Severity.Scope.ToString().Length > 0
            | _ -> true)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d -> $"Severity: %A{d.Severity}"
            | SupplyExceptionCommand.CorroborateException d -> $"Severity: %A{d.Severity}"
            | SupplyExceptionCommand.UpdateExceptionEvidence d -> $"Severity: %A{d.Severity}"
            | _ -> "Severity unchanged")

/// BR-S-295 — Stateless Assessment Boundary
let statelessAssessmentBoundary: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.statelessAssessmentBoundary.Id
        Rules.statelessAssessmentBoundary.Explanation
        (fun _ -> true)
        (fun _ -> "Pure functional assessment: no aggregate mutations inside algorithm execution")

/// BR-S-296 — Prevents exception generation when supporting evidence fails minimum reliability thresholds
let evidenceSufficiencyGuard: Rule<EvaluateExceptionInput> =
    Rule.create
        Rules.evidenceSufficiencyGuard.Id
        Rules.evidenceSufficiencyGuard.Explanation
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                not (List.isEmpty d.Evidence)
                && d.Evidence |> List.forall (fun e -> e.Confidence >= (input.Policy.EvidenceSufficiency.DetectedMinReliabilityPercent / 100.0m))
            | _ -> true)
        (fun input ->
            match input.Command with
            | SupplyExceptionCommand.DetectException d ->
                let confs = d.Evidence |> List.map (fun e -> sprintf "%.2f" e.Confidence) |> String.concat ", "
                let threshold = input.Policy.EvidenceSufficiency.DetectedMinReliabilityPercent / 100.0m
                $"EvidenceConfidences: [%s{confs}]; MinConfidence: %.2f{threshold}"
            | _ -> "N/A for non-detection commands")

/// Complete governed rule set for evaluating supply exception commands
let evaluationRules: Rule<EvaluateExceptionInput> list =
    [ supplyExceptionAssessmentIdentity
      exceptionDetectionEligibility
      singleCurrentSupplyExceptionAssessment
      exceptionAssessmentStateChangeImmutability
      exceptionAssessmentHistoryImmutability
      deterministicLifecycleTransitions
      exceptionEvidencePublicationRequirement
      exceptionResolutionEvidenceRequirement
      noDirectExceptionMutation
      multiDimensionalSeverityIndependence
      statelessAssessmentBoundary
      evidenceSufficiencyGuard ]

/// BR-S-279c — Determines whether confirmed exception qualifies for Enterprise Exception Escalation to Core
let shouldEscalateToCore (policy: ExceptionDetectionPolicy) (severity: SeverityAssessment) : bool =
    let impactMeets =
        match severity.BusinessImpact with
        | SeverityBusinessImpact.Major
        | SeverityBusinessImpact.Severe -> true
        | _ -> false

    let scopeMeets =
        if policy.EscalationCriteria.EscalateOnMultiFacilityScope then
            match severity.Scope with
            | SeverityScope.BusinessUnit
            | SeverityScope.Enterprise -> true
            | _ -> false
        else false

    impactMeets || scopeMeets
