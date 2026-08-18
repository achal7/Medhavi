/// CA-D-009 — Explain Demand Workflows
/// Traces to: FS-D-016 (Establish Demand Explanation Workflow)
module Medhavi.Demand.ExplainDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

/// Dependencies for FS-D-016 Establish Demand Explanation workflow
type DemandExplanationWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ExplanationApi: DemandExplanationApi }

/// FS-D-016: Automated Establish Demand Explanation Workflow
/// Triggered when critical conditions, quality anomalies, or exception alerts are published
let createDemandExplanationWorkflow
    (deps: DemandExplanationWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let artifactId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace artifactId) then
                    let req: EstablishDemandExplanationReq =
                        { ExplainedArtifactType = envelope.AggregateType
                          ExplainedArtifactId = artifactId
                          Version = int envelope.OccurrenceNumber
                          TemplateVersion = Some "1.0"
                          WhatIfAssumption = None
                          EvidenceRefs = [] }

                    let! _ = deps.ExplanationApi.EstablishExplanation req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.forecastPublicationPublished.Id
              ArsIdentifiers.EnterpriseEvents.forecastQualityAssessmentPublished.Id
              ArsIdentifiers.EnterpriseEvents.criticalDemandBehaviorDetected.Id
              ArsIdentifiers.EnterpriseEvents.demandExceptionEvidenceEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
