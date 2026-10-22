/// AB-S-080 — Manage Supply Change Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-080, EV-S-080)
module Medhavi.Supply.SenseSupplyChanges.SupplyChangeAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.Supply.ArsIdentifiers
open Model
open Policies
open Rules
open Decisions
open Algorithms
open Medhavi.Supply

/// AB-S-080: Assess Supply Change Command Handler
let assess
    (rules: Rule<AssessmentRuleInput> list)
    (policy: SupplyChangePolicyConfiguration)
    : Decide<SupplyChangeAssessmentAggregate, AssessSupplyChangeCmd, SupplyChangeEvent> =
    fun (cmd: AssessSupplyChangeCmd) (stateOpt: SupplyChangeAssessmentAggregate option) ->
        result {
            // Stage 1-5: Evaluate algorithm BA-S-060
            let evalResult =
                Algorithms.assessSupplyChange
                    cmd.Signal
                    cmd.Baseline
                    policy
                    cmd.CorroboratingSignalsCount
                    cmd.Timestamp

            let ruleInput =
                {
                    Command = cmd
                    ExistingAggregate = stateOpt
                    Result = evalResult
                }

            let! decisionOutcome = evaluateAssessment rules ruleInput

            let hasFailed = decisionOutcome.Evaluations |> List.exists(fun e -> not e.Passed)

            if hasFailed then
                let failureMsgs =
                    decisionOutcome.Evaluations
                    |> List.filter(fun e -> not e.Passed)
                    |> List.map(fun e ->
                        sprintf
                            "[%s] Reason: %s (Evidence: %s)"
                            e.RuleId
                            (e.ReasonCode |> Option.defaultValue "")
                            (e.Evidence |> String.concat ", "))
                    |> String.concat "; "

                return! Error(DomainError.validation failureMsgs)
            else
                let signalSummary: SignalSummary =
                    {
                        SignalId = cmd.Signal.SignalId
                        SourceId = cmd.Signal.SourceId
                        SourceType = cmd.Signal.SourceType
                        SourceReliabilityPrior = cmd.Signal.SourceReliabilityPrior
                        ObservedQuantity = cmd.Signal.ObservedQuantity
                        ObservedDate = cmd.Signal.ObservedDate
                        SignalGenerationTime = cmd.Signal.SignalGenerationTime
                    }

                match stateOpt with
                | None ->
                    // Initial creation of assessment in Active state
                    let newAggregate: SupplyChangeAssessmentAggregate =
                        {
                            AssessmentId = cmd.AssessmentId
                            MonitoredEntityType = cmd.MonitoredEntityType
                            MonitoredEntityId = cmd.MonitoredEntityId
                            ChangeType = cmd.ChangeType
                            CurrentEpistemicState = evalResult.SuggestedEpistemicState
                            ChangeClassification = cmd.Classification
                            ChangeDescription = evalResult.ChangeDescription
                            Materiality = evalResult.Materiality
                            OccurrenceTime = cmd.Signal.ObservedDate
                            DetectionTime = cmd.Timestamp
                            ExpectedDuration = cmd.ExpectedDuration
                            SignalSummary = signalSummary
                            AssessmentConfidence = evalResult.Confidence
                            DaysVariance = evalResult.DaysVariance
                            QuantityVariance = evalResult.QuantityVariance
                            QuantityVariancePercent = evalResult.QuantityVariancePercent
                            ThreatenedObjectives = evalResult.ThreatenedObjectives
                            CorrelatedChangeIds = cmd.CorrelatedChangeIds
                            History = []
                            LifecycleState = SupplyChangeLifecycleState.Active
                            CreatedTimestamp = cmd.Timestamp
                            UpdatedTimestamp = cmd.Timestamp
                        }

                    let events = [ SupplyChangeAssessmentCreated newAggregate ]
                    let summary = sprintf "Supply Change Assessment created: %s" evalResult.ChangeDescription

                    return
                        buildDecisionWithTrace
                            evolve
                            stateOpt
                            events
                            "AB-S-080"
                            []
                            Capabilities.senseSupplyChanges.Id
                            decisionOutcome
                            (Some Policies.supplyChangePolicy.Id)
                            (Some 1)
                            [ SemanticObjects.supplyChangeAssessment.Id ]
                            (Some summary)

                | Some existing ->
                    // Update existing assessment & append immutable AssessmentChangeEvent
                    let changeEvent: AssessmentChangeEvent =
                        {
                            EventId = Guid.NewGuid().ToString()
                            PreviousEpistemicState = existing.CurrentEpistemicState
                            NewEpistemicState = evalResult.SuggestedEpistemicState
                            TriggeringEvidence = evalResult.ChangeDescription
                            ReasonCode = evalResult.ReasonCodes |> String.concat ","
                            Timestamp = cmd.Timestamp
                        }

                    let updatedAggregate: SupplyChangeAssessmentAggregate =
                        { existing with
                            CurrentEpistemicState = evalResult.SuggestedEpistemicState
                            ChangeClassification = cmd.Classification
                            ChangeDescription = evalResult.ChangeDescription
                            Materiality = evalResult.Materiality
                            OccurrenceTime = cmd.Signal.ObservedDate
                            DetectionTime = cmd.Timestamp
                            ExpectedDuration = cmd.ExpectedDuration
                            SignalSummary = signalSummary
                            AssessmentConfidence = evalResult.Confidence
                            DaysVariance = evalResult.DaysVariance
                            QuantityVariance = evalResult.QuantityVariance
                            QuantityVariancePercent = evalResult.QuantityVariancePercent
                            ThreatenedObjectives = evalResult.ThreatenedObjectives
                            CorrelatedChangeIds = cmd.CorrelatedChangeIds
                            History = changeEvent :: existing.History
                            UpdatedTimestamp = cmd.Timestamp
                        }

                    let events = [ SupplyChangeAssessmentUpdated(updatedAggregate, changeEvent) ]
                    let summary = sprintf "Supply Change Assessment updated: %s" evalResult.ChangeDescription

                    return
                        buildDecisionWithTrace
                            evolve
                            stateOpt
                            events
                            "AB-S-080"
                            []
                            Capabilities.senseSupplyChanges.Id
                            decisionOutcome
                            (Some Policies.supplyChangePolicy.Id)
                            (Some 1)
                            [ SemanticObjects.supplyChangeAssessment.Id ]
                            (Some summary)
        }

/// AB-S-080: Archive Supply Change Command Handler
let archive: Decide<SupplyChangeAssessmentAggregate, ArchiveSupplyChangeCmd, SupplyChangeEvent> =
    fun (cmd: ArchiveSupplyChangeCmd) (stateOpt: SupplyChangeAssessmentAggregate option) ->
        result {
            match stateOpt with
            | None ->
                let (SupplyChangeAssessmentId id) = cmd.AssessmentId
                return! Error(DomainError.notFound("SupplyChangeAssessment", id))
            | Some existing ->
                if existing.LifecycleState = SupplyChangeLifecycleState.Archived then
                    return! Error(DomainError.conflict "Supply Change Assessment is already archived")
                else
                    let archivedAggregate =
                        { existing with
                            LifecycleState = SupplyChangeLifecycleState.Archived
                            UpdatedTimestamp = cmd.Timestamp
                        }

                    let events = [ SupplyChangeAssessmentArchived(archivedAggregate, cmd.Reason) ]
                    let outcome = SupplyChangeAssessmentDecisionOutcome.AssessmentArchived cmd.Reason
                    let traceOutcome = { Outcome = outcome; Evaluations = [] }
                    let summary = sprintf "Supply Change Assessment archived: %s" cmd.Reason

                    return
                        buildDecisionWithTrace
                            evolve
                            stateOpt
                            events
                            "AB-S-080"
                            []
                            Capabilities.senseSupplyChanges.Id
                            traceOutcome
                            (Some Policies.supplyChangePolicy.Id)
                            (Some 1)
                            [ SemanticObjects.supplyChangeAssessment.Id ]
                            (Some summary)
        }
