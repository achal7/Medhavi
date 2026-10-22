/// CA-S-012 Explain Supply Decisions Workflows (BW-S-110, FS-S-110)
module Medhavi.Supply.ExplainSupplyDecisions.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type SupplyExplanationWorkflowDependencies =
    {
        Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
        DispatchNotification: SupplyExplanationNotification -> Task<unit>
        Apis: Capabilities.ExplainSupplyDecisionsApis
    }

/// BW-S-110: Automated Supply Explanation Workflow
/// Triggered per PO-S-130 when Supply Plan, Quality Assessment, Exception, or Procurement Recommendation is published
let createSupplyExplanationWorkflow
    (deps: SupplyExplanationWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let artifactId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace artifactId) then
                    let req: RecordSupplyExplanationReq =
                        {
                            ExplainedArtifactType = envelope.AggregateType
                            ExplainedArtifactId = artifactId
                            ExplainedArtifactVersion = envelope.OccurrenceNumber
                            TemplateVersion = Some "v1.0"
                            IsNegativeExplanation = false
                            NegativeAlternativeId = None
                            NegativeAlternativeType = None
                            NegativeReasonCategory = None
                            WhatIfAssumption = None
                            EvidenceRefs = []
                        }

                    let! res = deps.Apis.Explanation.RecordExplanation req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                else
                    ()
            }

        let filter =
            [
                ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id
                ArsIdentifiers.EnterpriseEvents.procurementRecommendationCreated.Id
                ArsIdentifiers.EnterpriseEvents.supplyQualityAssessmentPublished.Id
                ArsIdentifiers.EnterpriseEvents.supplyExceptionAssessmentUpdated.Id
            ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply explanation workflow: {ex.Message}")))
    }

/// BW-S-110 Notification Node: Dispatches BN-S-110 after EV-S-110 Supply Explanation Recorded exists
let createNotificationNodeWorkflow
    (deps: SupplyExplanationWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; envelope.AggregateId |]
                let explanationId = parts.[0]
                let explainedId = if parts.Length > 1 then parts.[1] else parts.[0]

                let notification: SupplyExplanationNotification =
                    {
                        ExplanationId = explanationId
                        ExplainedArtifactType = envelope.AggregateType
                        ExplainedArtifactId = explainedId
                        ExplainedArtifactVersion = envelope.OccurrenceNumber
                        IsNegativeExplanation = false
                        ExplanationConfidence = "Assessed"
                        PublishedAt = envelope.CreatedUtc
                    }

                do! deps.DispatchNotification notification
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.supplyExplanationRecorded.Id ] |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply explanation notification node: {ex.Message}")))
    }
