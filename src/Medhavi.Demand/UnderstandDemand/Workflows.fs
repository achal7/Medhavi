/// CA-D-001 Understand Demand Workflows
/// Traces to: FS-D-001, FS-D-002, FS-D-003, FS-D-004
module Medhavi.Demand.UnderstandDemand.Workflows

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
open Capabilities

type UnderstandDemandWorkflowDependencies =
    { ObservationCodec: Codec<DemandObservationDto>
      UnderstandingCodec: Codec<DemandUnderstandingDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      Apis: UnderstandDemandApis
      DefaultScopeId: string }

/// FS-D-002: Workflow evaluating received observations (EV-D-001 -> DE-D-001 -> EV-D-002)
let createDemandObservationWorkflow
    (deps: UnderstandDemandWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let obsId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace obsId) then
                    let req: EvaluateObservationReq =
                        { ObservationId = obsId
                          EvaluationTime = DateTimeOffset.UtcNow }

                    let! _ = deps.Apis.Observations.Evaluate req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.demandObservationReceived.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

/// FS-D-003: Workflow revising Demand Understanding when Enterprise Picture is published (BN-C-001)
let createEnterprisePictureWorkflow
    (deps: UnderstandDemandWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let req: ReviseDemandUnderstandingReq =
                    { PlanningScopeId = deps.DefaultScopeId
                      EvidencePictureVersion = envelope.OccurrenceNumber }

                let! _ = deps.Apis.Understanding.Revise req
                return ()
            }

        let filter =
            [ "BN-C-001"; "EV-C-021" ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

/// FS-D-004: Workflow evaluating materiality and publishing Demand Understanding (EV-D-003)
let createDemandUnderstandingPublishWorkflow
    (deps: UnderstandDemandWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: PublishDemandUnderstandingReq =
                    { PlanningScopeId = scopeId
                      IsPeriodicRefresh = false }

                let! _ = deps.Apis.Understanding.Publish req
                return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.demandUnderstandingRevised.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
