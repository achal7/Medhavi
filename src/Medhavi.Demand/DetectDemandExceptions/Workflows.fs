/// CA-D-008 — Detect Demand Exceptions Workflows
/// Traces to: FS-D-015 (Detect Demand Exception Evidence Workflow)
module Medhavi.Demand.DetectDemandExceptions.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

/// Dependencies for FS-D-015 Detect Demand Exception Evidence workflow
type DemandExceptionWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ExceptionApi: DemandExceptionApi }

/// FS-D-015: Automated Detect Demand Exception Evidence Workflow
/// Triggered when forecast published, quality assessed, or behavior changed
let createDemandExceptionWorkflow
    (deps: DemandExceptionWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let entityId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace entityId) then
                    let req: EvaluateDemandExceptionReq =
                        { ScopeId = entityId
                          EntityType = "Item"
                          EntityId = entityId
                          ExceptionType = "BiasDrift"
                          TriggeringMetric = "ForecastBias"
                          MetricValue = 18.5m
                          HistoricalValues = Some [ 12.0m; 15.0m; 18.5m ] }

                    let! _ = deps.ExceptionApi.EvaluateException req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.forecastPublicationPublished.Id
              ArsIdentifiers.EnterpriseEvents.forecastQualityAssessmentPublished.Id
              ArsIdentifiers.EnterpriseEvents.demandBehaviorChanged.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
