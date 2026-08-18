/// CA-D-002 Forecast Demand Workflows
/// Traces to: FS-D-005, FS-D-006, FS-D-008, PO-D-032
module Medhavi.Demand.ForecastDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.SemanticModel
open Medhavi.Demand

/// Dependencies for FS-D-005 / FS-D-008 Forecast workflows
type CriticalDemandForecastWorkflowDependencies =
    { Codec: Codec<CriticalDemandBehaviorNotification>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ForecastApi: ForecastPublicationApi
      DefaultScopeId: string }

/// FS-D-005 / FS-D-010: Workflow reacting to Critical Demand Behavior detected in Sensing (EV-D-016)
/// Automatically initiates an out-of-cycle forecast refresh cycle for the affected Planning Scope.
let createCriticalDemandForecastWorkflow
    (deps: CriticalDemandForecastWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                match deps.Codec.Decode envelope.DataJson with
                | Ok notif ->
                    let now = DateTimeOffset.UtcNow
                    let hStart = now
                    let hEnd = now.AddDays(30.0)

                    let initReq: InitiateForecastCycleReq =
                        { PlanningScopeId = deps.DefaultScopeId
                          HorizonStart = hStart
                          HorizonEnd = hEnd
                          InitiationReason =
                            sprintf
                                "Out-of-cycle forecast refresh triggered by critical demand behavior on %s at %s (FS-D-010)"
                                (ItemId.value notif.Item)
                                (LocationId.value notif.Location) }

                    let! initResult = deps.ForecastApi.InitiateCycle initReq

                    match initResult with
                    | Ok initDto ->
                        let projReq: ProduceForecastProjectionReq = { PublicationId = initDto.PublicationId }
                        let! _ = deps.ForecastApi.ProduceProjection projReq
                        return ()
                    | Error _ -> return ()
                | Error _ -> return ()
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.criticalDemandBehaviorDetected.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

/// FS-D-008: Workflow auto-evaluating and publishing forecast projections once produced (EV-D-011)
let createForecastAutoPublishWorkflow
    (deps: CriticalDemandForecastWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let pubId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace pubId) then
                    let req: PublishForecastPublicationReq = { PublicationId = pubId }
                    let! _ = deps.ForecastApi.Publish req
                    return ()
                else
                    return ()
            }

        let filter = [ ArsIdentifiers.EnterpriseEvents.forecastProjectionProduced.Id ] |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
