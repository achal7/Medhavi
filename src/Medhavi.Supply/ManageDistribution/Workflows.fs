/// CA-S-008 — Manage Distribution Workflows
/// Traces to: BW-S-080, BW-S-081, FS-S-080, FS-S-081
module Medhavi.Supply.ManageDistribution.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ManageDistributionWorkflowDependencies =
    { RecommendationCodec: Codec<DistributionRecommendationDto>
      PlanCodec: Codec<DistributionPlanDto>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ManageDistributionApis
      DefaultScopeId: string
      DefaultCycleId: string }

/// BW-S-080: Workflow creating a Distribution Recommendation when a distribution requirement is identified
let createDistributionRequirementWorkflow
    (deps: ManageDistributionWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let reqId = envelope.AggregateId
                if not (String.IsNullOrWhiteSpace reqId) then
                    let req: CreateDistributionRecommendationReq =
                        { RequirementReference = reqId
                          Timestamp = DateTimeOffset.UtcNow }

                    let! res = deps.Apis.Recommendation.Create req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                else
                    ()
            }

        let filter =
            [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
              ArsIdentifiers.EnterpriseEvents.inventoryPositionAssessmentChanged.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe distribution requirement workflow: {ex.Message}")))
    }

/// BW-S-081: Workflow composing and publishing Distribution Plan baseline when triggered by cadence or supply plan publication
let createDistributionPlanPublishWorkflow
    (deps: ManageDistributionWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let scopeId =
                    if String.IsNullOrWhiteSpace envelope.AggregateId then deps.DefaultScopeId
                    else envelope.AggregateId

                let req: PublishDistributionPlanReq =
                    { PlanningScopeId = scopeId
                      PlanningCycle = deps.DefaultCycleId
                      PublicationReason = PublicationReasonType.ScheduledCadence
                      Timestamp = DateTimeOffset.UtcNow }

                let! res = deps.Apis.Plan.Publish req
                match res with
                | Ok _ -> ()
                | Error _ -> ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.supplyPlanPublished.Id
              ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe distribution plan publish workflow: {ex.Message}")))
    }

/// BW-S-080 / BW-S-081 Notification Node: Dispatches BN-S-070, BN-S-071 strictly after events exist
let createNotificationNodeWorkflow
    (deps: ManageDistributionWorkflowDependencies)
    (ct: CancellationToken)
    : TaskResult<IDisposable, ApplicationError> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                if envelope.EventType = ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id then
                    let notif: DistributionRecommendationCreatedNotification =
                        { NotificationId = Guid.NewGuid().ToString()
                          ReferencedEventId = ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id
                          RequirementReference = envelope.AggregateId
                          VersionNumber = int envelope.OccurrenceNumber
                          ItemId = ""
                          RequiredQuantity = 0.0m
                          DestinationLocationId = ""
                          DepartureTiming = envelope.CreatedUtc
                          ExpectedArrivalTiming = envelope.CreatedUtc
                          FeasibilityStatus = DistributionFeasibilityStatus.FullyFeasible
                          ConfidenceLevel = DistributionConfidenceLevel.High
                          Timestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.distributionRecommendationCreated.Id
                            ArsIdentifiers.Capabilities.manageDistribution.Id
                            "DistributionRecommendation"
                            envelope.AggregateId
                            notif
                    ()
                elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.distributionPlanPublished.Id then
                    let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultCycleId |]
                    let scopeId = parts.[0]
                    let cycleId = if parts.Length > 1 then parts.[1] else deps.DefaultCycleId

                    let notif: DistributionPlanPublishedNotification =
                        { NotificationId = Guid.NewGuid().ToString()
                          ReferencedEventId = ArsIdentifiers.EnterpriseEvents.distributionPlanPublished.Id
                          PlanningScopeId = scopeId
                          PlanningCycle = cycleId
                          VersionNumber = int envelope.OccurrenceNumber
                          TotalMovementCount = 0
                          TotalQuantityMoved = 0.0m
                          FeasibilityPercentage = 100.0m
                          AverageStabilityScore = 1.0m
                          Timestamp = envelope.CreatedUtc }

                    let! _ =
                        dispatchNotification
                            deps.DispatchEnvelope
                            ArsIdentifiers.BusinessNotifications.distributionPlanPublished.Id
                            ArsIdentifiers.Capabilities.manageDistribution.Id
                            "DistributionPlan"
                            envelope.AggregateId
                            notif
                    ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id
              ArsIdentifiers.EnterpriseEvents.distributionPlanPublished.Id ]
            |> EnvelopeFilter.EventTypes

        try
            let! sub = deps.Subscribe filter processEnvelope ct
            return Ok sub
        with ex ->
            return Error (Infrastructure(EventStore($"Failed to subscribe distribution notification node workflow: {ex.Message}")))
    }
