/// CA-D-005 — Classify Demand Workflows
/// Traces to: FS-D-012 (Classify Demand Behavior Workflow)
module Medhavi.Demand.ClassifyDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand

/// Dependencies for FS-D-012 Classify Demand Behavior workflow
type DemandBehaviorClassificationWorkflowDependencies =
    { Codec: Codec<DemandUnderstandingPublishedNotification>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ClassificationApi: DemandBehaviorClassificationApi
      DefaultLocationId: string }

/// FS-D-012: Automated Classify Demand Behavior Workflow
/// Triggered when Demand Understanding is published (EV-D-004 / BN-D-001)
let createDemandBehaviorClassificationWorkflow
    (deps: DemandBehaviorClassificationWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace scopeId) then
                    let req: ClassifyDemandBehaviorReq =
                        { ItemId = scopeId
                          LocationId = deps.DefaultLocationId
                          Dimension = "StatisticalPattern"
                          DemandQuantities = [ 100.0m; 105.0m; 98.0m; 110.0m; 95.0m; 102.0m ] }

                    let! _ = deps.ClassificationApi.ClassifyBehavior req
                    return ()
                else
                    return ()
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.demandUnderstandingPublished.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
