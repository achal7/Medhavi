module Medhavi.Core.DemandManagement.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Contracts.Core.Demand
open Medhavi.SharedKernel.BusinessNotifications

type DemandRecordingWorkflowDependencies =
    { Codec: Codec<DemandObservationAcceptedNotification>
      DemandApi: DemandApi
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable> }

/// FS-C-009: Consumes BN-D-006 (Demand Observation Accepted) from Demand Intelligence
/// and records the accepted observation as an authoritative enterprise demand fact (SE-C-013).
let createDemandRecordingWorkflow
    (deps: DemandRecordingWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                match deps.Codec.Decode envelope.DataJson with
                | Ok notif ->
                    let req: RecordDemandReq =
                        { DemandId = notif.ObservationId
                          Item = ItemId.value notif.Item
                          Location = LocationId.value notif.Location
                          Customer = notif.Customer |> Option.map CustomerId.value
                          Quantity = Quantity.value notif.Quantity
                          NeedWindowLatest = Timestamp.value notif.BusinessTime
                          NeedWindowEarliest = None
                          NeedWindowPreferred = Some(Timestamp.value notif.BusinessTime)
                          DemandOrigin = VocabularyEntryId.value notif.ObservationType
                          ParentDemand = None }

                    let! _ = deps.DemandApi.Record req
                    return ()
                | Error _ -> return () // Decode failure logged by infrastructure
            }

        let! subscription = deps.Subscribe (EnvelopeFilter.EventTypes [ "BN-D-006" ]) processEnvelope ct
        return subscription
    }
