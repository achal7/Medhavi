/// Sense Demand Workflows
/// Traces to: FS-D-009, FS-D-010, CR-D-009, CR-D-010 (Specification Chapter 9.3)
module Medhavi.Demand.SenseDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ArsIdentifiers

/// Dependencies for Sense Demand workflows
type CriticalBehaviorWorkflowDependencies =
    { Codec: Codec<CriticalDemandBehaviorNotification>
      ObservationCodec: Codec<DemandObservationDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      CapabilityApi: DemandBehaviorAssessmentApi }

/// FS-D-009: Workflow evaluating streaming/accepted demand signals against behavior baseline (EV-D-002)
let createDemandObservationSensingWorkflow
    (deps: CriticalBehaviorWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                match deps.ObservationCodec.Decode envelope.DataJson with
                | Ok obs ->
                    let req: EvaluateDemandSignalReq =
                        { Item = obs.Item
                          Location = obs.Location
                          Quantity = obs.Quantity
                          SignalTimestamp = obs.BusinessTime
                          CorroboratingSources = [ obs.SourceSystemProvenance ]
                          IsHighPriority = false }

                    let! _ = deps.CapabilityApi.EvaluateSignal req
                    return ()
                | Error _ -> return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.demandObservationEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }

/// FS-D-010: Escalate Critical Demand Behavior Workflow
/// Realises CR-D-010 — connects event subscription to trigger forecast refresh evaluation.
let createCriticalBehaviorWorkflow
    (deps: CriticalBehaviorWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                match deps.Codec.Decode envelope.DataJson with
                | Ok notif ->
                    let req: EvaluateForecastRefreshReq =
                        { Item = ItemId.value notif.Item
                          Location = LocationId.value notif.Location
                          ForecastAgeHours = 5
                          ExpectedAccuracyImprovementWape = 0.05m }

                    let! _ = deps.CapabilityApi.EvaluateForecastRefresh req
                    return ()
                | Error _ -> return ()
            }

        let! sub =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ ArsIdentifiers.EnterpriseEvents.criticalDemandBehaviorDetected.Id ])
                processEnvelope
                ct

        return sub
    }
