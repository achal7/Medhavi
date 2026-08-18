/// CA-D-007 — Evaluate Demand Quality Workflows
/// Traces to: FS-D-014 (Assess Forecast Quality Workflow)
module Medhavi.Demand.EvaluateDemandQuality.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

/// Dependencies for FS-D-014 Assess Forecast Quality workflow
type ForecastQualityWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      QualityApi: ForecastQualityApi }

/// FS-D-014: Automated Assess Forecast Quality Workflow
/// Triggered when a Forecast Publication is published (EV-D-013 / BN-D-011)
let createForecastQualityWorkflow
    (deps: ForecastQualityWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let pubId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace pubId) then
                    let now = DateTimeOffset.UtcNow

                    let req: EvaluateForecastQualityReq =
                        { ScopeId = pubId
                          EvaluationPeriodStart = now.AddDays(-30.0)
                          EvaluationPeriodEnd = now
                          ForecastPublicationId = Some pubId
                          Observations = []
                          CompletenessScore = 1.0m }

                    let! evalResult = deps.QualityApi.EvaluateQuality req

                    match evalResult with
                    | Ok dto ->
                        let pubReq: PublishForecastQualityAssessmentReq =
                            { ScopeId = dto.ScopeId
                              EvaluationPeriodStart = dto.EvaluationPeriodStart
                              EvaluationPeriodEnd = dto.EvaluationPeriodEnd
                              VersionNumber = 1 }

                        let! _ = deps.QualityApi.PublishAssessment pubReq
                        return ()
                    | Error _ -> return ()
                else
                    return ()
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.forecastPublicationPublished.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
