/// CA-S-001 Understand Supply Workflows
/// Traces to: BW-S-001, BW-S-002, BW-S-003, BW-S-004, FS-S-001, FS-S-002, FS-S-003, FS-S-004
module Medhavi.Supply.UnderstandSupply.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers
open Capabilities

type UnderstandSupplyWorkflowDependencies =
    { RecordCodec: Codec<SupplyDataRecordDto>
      UnderstandingCodec: Codec<SupplyUnderstandingDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: UnderstandSupplyApis
      DefaultScopeId: string }

/// FS-S-002 / BW-S-002: Workflow evaluating received supply records (EV-S-001 -> DE-S-001 -> EV-S-002)
let createSupplyDataWorkflow
    (deps: UnderstandSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let recId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace recId) then
                    let req: EvaluateSupplyDataReq =
                        { RecordId = recId
                          EvaluationTime = DateTimeOffset.UtcNow }

                    let! res = deps.Apis.Records.Evaluate req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                else
                    ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyDataRecordReceived.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe supply data workflow: {ex.Message}")))
    }

/// FS-S-003 / BW-S-003: Workflow revising Supply Understanding when Enterprise Picture is published (BN-C-001 / EV-C-002)
let createEnterprisePictureWorkflow
    (deps: UnderstandSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let req: ReviseSupplyUnderstandingReq =
                    { PlanningScopeId = deps.DefaultScopeId
                      EvidencePictureVersion = envelope.OccurrenceNumber }

                let! res = deps.Apis.Understanding.Revise req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.ExternalNotifications.enterprisePicturePublished.Id
              ArsIdentifiers.ExternalNotifications.pictureVersionPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe enterprise picture workflow: {ex.Message}")))
    }

/// FS-S-004 / BW-S-004: Workflow evaluating materiality and publishing Supply Understanding (EV-S-003 -> DE-S-002 -> EV-S-004)
let createSupplyUnderstandingPublishWorkflow
    (deps: UnderstandSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: PublishSupplyUnderstandingReq =
                    { PlanningScopeId = scopeId
                      IsPeriodicRefresh = false }

                let! res = deps.Apis.Understanding.Publish req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyUnderstandingRevised.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe understanding publish workflow: {ex.Message}")))
    }

/// BW-S-004 Notification Node: Dispatches BN-S-001 Supply Understanding Published strictly after EV-S-004 exists
let createNotificationNodeWorkflow
    (deps: UnderstandSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = ArsIdentifiers.EnterpriseEvents.supplyUnderstandingPublished.Id then
                    let notif: SupplyUnderstandingPublishedNotification =
                        { PlanningScopeId = envelope.AggregateId
                          VersionNumber = int envelope.OccurrenceNumber
                          PublicationTime = envelope.CreatedUtc
                          SupersededVersion = if envelope.OccurrenceNumber > 1L then Some(int envelope.OccurrenceNumber - 1) else None
                          MaterialChangeSummary = "Authoritative supply interpretation published"
                          PeriodicRefreshFlag = false
                          CompletenessScore = 1.0m }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.supplyUnderstandingPublished.Id
                            ArsIdentifiers.Capabilities.understandSupply.Id
                            "SupplyUnderstanding"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyUnderstandingPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe notification node workflow: {ex.Message}")))
    }
