/// CA-D-004 — Segment Demand Workflows
/// Traces to: FS-D-011 (Classify Planning Entity Workflow)
module Medhavi.Demand.SegmentDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers

/// Dependencies for FS-D-011 Classify Planning Entity workflow
type PlanningClassificationWorkflowDependencies =
    { Codec: Codec<DemandUnderstandingPublishedNotification>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ClassificationApi: PlanningClassificationApi }

/// FS-D-011: Automated Classify Planning Entity Workflow
/// Triggered when Demand Understanding is published (EV-D-004 / BN-D-001)
let createPlanningClassificationWorkflow
    (deps: PlanningClassificationWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace scopeId) then
                    let req: ClassifyPlanningEntityReq =
                        { EntityType = "Item"
                          EntityId = scopeId
                          ClassificationType = "ABC"
                          VolumeOrRevenuePercentage = Some 15.0m
                          HistoricalDemandValues = None
                          AnalogItemId = None }

                    let! _ = deps.ClassificationApi.ClassifyEntity req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.demandUnderstandingPublished.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
