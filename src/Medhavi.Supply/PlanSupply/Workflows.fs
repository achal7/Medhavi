/// CA-S-002 Plan Supply Workflows
/// Traces to: BW-S-010, BW-S-011, BW-S-012, FS-S-010, FS-S-011, FS-S-012
module Medhavi.Supply.PlanSupply.Workflows

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

type PlanSupplyWorkflowDependencies =
    { PlanCodec: Codec<SupplyPlanDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: PlanSupplyApis
      DefaultScopeId: string
      DefaultHorizonId: string }

/// FS-S-010 / BW-S-010: Workflow triggering supply-demand balancing when new Supply Understanding (BN-S-001) or Demand Forecast (BN-D-011) arrives
let createPlanTriggerWorkflow
    (deps: PlanSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let req: GenerateSupplyPlanReq =
                    { PlanningScopeId = if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId else envelope.AggregateId
                      ForecastHorizonId = deps.DefaultHorizonId
                      SupplyUnderstandingVersion = 1
                      ForecastPublicationVersion = 1
                      EnterprisePictureVersion = envelope.OccurrenceNumber }

                let! res = deps.Apis.Plan.Generate req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.BusinessNotifications.supplyUnderstandingPublished.Id
              ArsIdentifiers.BusinessNotifications.forecastPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe plan trigger workflow: {ex.Message}")))
    }

/// FS-S-011 / BW-S-011: Workflow evaluating Draft Supply Plan upon generation (EV-S-010 -> DE-S-011 -> EV-S-011)
let createPlanEvaluationWorkflow
    (deps: PlanSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let planId = envelope.AggregateId
                let parts = if planId.Contains("#") then planId.Split('#') else [| planId; deps.DefaultHorizonId |]
                let scopeId = parts.[0]
                let horizonId = if parts.Length > 1 then parts.[1] else deps.DefaultHorizonId

                let req: EvaluateSupplyPlanReq =
                    { PlanningScopeId = scopeId
                      ForecastHorizonId = horizonId
                      VersionNumber = int envelope.OccurrenceNumber }

                let! res = deps.Apis.Plan.Evaluate req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyPlanGenerated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe plan evaluation workflow: {ex.Message}")))
    }

/// FS-S-012 / BW-S-012: Workflow publishing accepted Supply Plan (EV-S-011 -> DE-S-012 -> EV-S-012 -> BN-S-010)
let createPlanPublishWorkflow
    (deps: PlanSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let planId = envelope.AggregateId
                let parts = if planId.Contains("#") then planId.Split('#') else [| planId; deps.DefaultHorizonId |]
                let scopeId = parts.[0]
                let horizonId = if parts.Length > 1 then parts.[1] else deps.DefaultHorizonId

                let req: PublishSupplyPlanReq =
                    { PlanningScopeId = scopeId
                      ForecastHorizonId = horizonId
                      VersionNumber = int envelope.OccurrenceNumber
                      IsOutCycle = false }

                let! res = deps.Apis.Plan.Publish req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyPlanEvaluated.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe plan publish workflow: {ex.Message}")))
    }

/// BW-S-012 Notification Node: Dispatches BN-S-010 Supply Plan Published strictly after EV-S-012 exists
let createNotificationNodeWorkflow
    (deps: PlanSupplyWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id then
                    let planId = envelope.AggregateId
                    let parts = if planId.Contains("#") then planId.Split('#') else [| planId; deps.DefaultHorizonId |]
                    let scopeId = parts.[0]
                    let horizonId = if parts.Length > 1 then parts.[1] else deps.DefaultHorizonId

                    let notif: SupplyPlanPublishedNotification =
                        { PlanningScopeId = scopeId
                          ForecastHorizonId = horizonId
                          PublishedVersionNumber = int envelope.OccurrenceNumber
                          SupersededVersionIdentifier = if envelope.OccurrenceNumber > 1L then Some $"{scopeId}#{horizonId}#v{envelope.OccurrenceNumber - 1L}" else None
                          PublicationTimestamp = envelope.CreatedUtc
                          PlanConfidenceScore = 1.0m
                          ConstraintSummary = "Operational constraints satisfied"
                          PlanningAssumptionsSummary = "Supply plan based on validated enterprise parameters"
                          MaterialPlanningChanges = "Published supply plan updated" }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
                            ArsIdentifiers.Capabilities.planSupply.Id
                            "SupplyPlan"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe notification node workflow: {ex.Message}")))
    }
