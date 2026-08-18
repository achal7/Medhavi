/// CA-D-011 Model Demand Interventions Workflows
/// Traces to: FS-D-018, FS-D-019
module Medhavi.Demand.ModelDemandInterventions.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

type DemandInterventionWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      CapabilityApi: DemandInterventionApi
      DefaultItemId: string
      DefaultLocationId: string }

/// FS-D-018: Assess Demand Intervention Impact Workflow
/// Triggered when a Scenario Adjustment is published/active (SE-C-039)
let createDemandInterventionAssessmentWorkflow
    (deps: DemandInterventionWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let adjId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace adjId) then
                    let now = DateTimeOffset.UtcNow

                    let req: AssessInterventionImpactReq =
                        { ImpactId = "IMP-" + Guid.NewGuid().ToString("N").[..7]
                          InterventionReference = adjId
                          Item = deps.DefaultItemId
                          Location = deps.DefaultLocationId
                          InterventionType = "Promotion"
                          InterventionMagnitude = 0.20m
                          TemporalValidityStart = now
                          TemporalValidityEnd = now.AddDays(14.0)
                          HistoricalPairs = [ (100.0m, 10.0m); (120.0m, 8.0m); (110.0m, 9.0m) ]
                          BaselineDemand = Some 100.0m }

                    let! _ = deps.CapabilityApi.AssessImpact req
                    return ()
                else
                    return ()
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.demandUnderstandingPublished.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

/// FS-D-019: Publish Demand Intervention Impact Workflow
/// Triggered when a Draft impact assessment has been computed (EV-D-023)
let createDemandInterventionPublishWorkflow
    (deps: DemandInterventionWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let impactId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace impactId) then
                    let req: PublishInterventionImpactReq = { ImpactId = impactId }
                    let! _ = deps.CapabilityApi.PublishImpact req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.demandInterventionImpactPublished.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
