/// AB-S-041 — Update Supplier Commitment Assessment Aggregate Behavior
/// Traces to: Supply Intelligence Specification §4.3.1 (AB-S-041, EV-S-041)
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Rules
open Decisions
open Algorithms

type SupplierAssessmentFactsProvider = SupplierId -> SupplierOpenPortfolioFact * SupplierTelemetryFact

type AssessmentOutcome =
    | AssessmentUpdated of supplierId: SupplierId * healthState: SupplierCollaborationHealthState * isMaterial: bool

/// AB-S-041: Update Supplier Commitment Assessment
let update
    (factsProvider: SupplierAssessmentFactsProvider)
    (policy: SupplierCollaborationPolicy)
    : Decide<SupplierCommitmentAssessment, UpdateSupplierCommitmentAssessmentCmd, SupplierCommitmentAssessmentEvent> =
    fun (cmd: UpdateSupplierCommitmentAssessmentCmd) (stateOpt: SupplierCommitmentAssessment option) ->
        result {
            do! validateIdentity cmd.SupplierId

            let currentLifecycle =
                match stateOpt with
                | Some s -> s.State
                | None -> AssessmentLifecycleState.Active

            do! validateEligibility currentLifecycle

            let portfolio, telemetry = factsProvider cmd.SupplierId

            let exposure =
                evaluateCommitmentExposure
                    cmd.SupplierId
                    portfolio.OpenCommitmentsQuantity
                    portfolio.EstimatedPortfolioValue
                    telemetry
                    policy
                    cmd.AssessmentTimestamp

            let healthDetermination =
                determineHealthState exposure portfolio.OpenCommitmentsCount telemetry.DormancyDays

            let material = isMaterialChange stateOpt healthDetermination.AuthoritativeState exposure.Exposures

            let history =
                match stateOpt with
                | Some s -> s.ChangeHistory
                | None -> []

            let nextSeq = history.Length + 1

            let changeEventOpt =
                if material then
                    let prevHealth = stateOpt |> Option.map(fun s -> s.AuthoritativeHealthState)

                    Some
                        {
                            SequenceNumber = nextSeq
                            ChangeTimestamp = cmd.AssessmentTimestamp
                            PreviousHealthState = prevHealth
                            NewHealthState = healthDetermination.AuthoritativeState
                            ChangeSummary =
                                $"Health state evaluated as {healthDetermination.AuthoritativeState} (Rule: {healthDetermination.TriggeringRuleId})"
                        }
                else
                    None

            let updatedHistory =
                match changeEventOpt with
                | Some chg -> chg :: history
                | None -> history

            let otifRate = telemetry.HistoricalOTIF |> Option.defaultValue 1.0m
            let qarRate = telemetry.QualityAcceptanceRate |> Option.defaultValue 1.0m

            let updatedAssessment: SupplierCommitmentAssessment =
                {
                    SupplierId = cmd.SupplierId
                    State = AssessmentLifecycleState.Active
                    AssessmentTimestamp = cmd.AssessmentTimestamp
                    OverallConfidence = exposure.OverallConfidence
                    Exposures = exposure.Exposures
                    OnTimeDeliveryRate = otifRate
                    QuantityFulfillmentRate = qarRate
                    AverageAcknowledgementHours = telemetry.AverageAcknowledgementHours
                    AverageCommitmentHours = telemetry.AverageCommitmentHours
                    ActiveRiskSignals = exposure.ReasonCodes
                    AuthoritativeHealthState = healthDetermination.AuthoritativeState
                    ChangeHistory = updatedHistory
                }

            let events = [ SupplierCommitmentAssessmentChanged(updatedAssessment, changeEventOpt) ]
            let outcome = AssessmentUpdated(cmd.SupplierId, healthDetermination.AuthoritativeState, material)
            let traceOutcome = { Outcome = outcome; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    ArsIdentifiers.Decisions.classifyCollaborationHealth.Id
                    []
                    ArsIdentifiers.Capabilities.collaborateWithSuppliers.Id
                    traceOutcome
                    (Some ArsIdentifiers.Policies.supplierCollaborationPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.supplierCommitmentAssessment.Id ]
                    (Some
                        $"Supplier commitment assessment updated for {SupplierId.value cmd.SupplierId}: Health={healthDetermination.AuthoritativeState}")
        }
