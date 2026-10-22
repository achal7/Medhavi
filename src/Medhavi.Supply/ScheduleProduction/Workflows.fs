namespace Medhavi.Supply.ScheduleProduction

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ScheduleProductionWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ScheduleProductionApis
      DefaultResourceId: string
      DefaultPlanningPeriodId: string
      DefaultPlanningScopeId: string
      DefaultScheduleType: Medhavi.Contracts.Supply.ScheduleType }

/// CA-S-007 Schedule Production Workflows
/// Traces to: BW-S-070, BW-S-071, FS-S-070, FS-S-071
module Workflows =

    /// FS-S-070 / BW-S-070: Create Production Schedules Workflow
    /// Subscribes to Supply Plan Published (BN-S-010), Capacity Position Assessment Changed (BN-S-030),
    /// and Inventory Position Assessment Changed (BN-S-020)
    let createScheduleProductionWorkflow
        (deps: ScheduleProductionWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let req: CreateProductionScheduleReq =
                        { ResourceId = deps.DefaultResourceId
                          PlanningPeriodId = deps.DefaultPlanningPeriodId
                          ScheduleType = deps.DefaultScheduleType
                          Timestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.Schedule.Create req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                }

            let filter =
                [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
                  ArsIdentifiers.BusinessNotifications.capacityPositionAssessmentChanged.Id
                  ArsIdentifiers.BusinessNotifications.inventoryPositionAssessmentChanged.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe schedule production workflow: {ex.Message}")))
        }

    /// FS-S-071 / BW-S-071: Publish Production Schedule Baseline Workflow
    /// Triggered on governed planning cadence following supply plan publication
    let createPublishedProductionScheduleWorkflow
        (deps: ScheduleProductionWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let req: PublishProductionScheduleReq =
                        { PlanningScopeId = deps.DefaultPlanningScopeId
                          PlanningPeriodId = deps.DefaultPlanningPeriodId
                          TriggerType = "CadenceScheduled"
                          Timestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.PublishedBaseline.Publish req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                }

            let filter =
                [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe published production schedule workflow: {ex.Message}")))
        }

    /// BW-S-070 / BW-S-071 Notification Node: Dispatches BN-S-060, BN-S-061 strictly after events exist
    let createNotificationNodeWorkflow
        (deps: ScheduleProductionWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    if envelope.EventType = ArsIdentifiers.EnterpriseEvents.productionScheduleActivated.Id then
                        let notif: ProductionScheduleActivatedNotification =
                            { ResourceId = envelope.AggregateId
                              PlanningPeriodId = deps.DefaultPlanningPeriodId
                              ScheduleId = envelope.AggregateId
                              VersionNumber = int envelope.OccurrenceNumber
                              ScheduleType = deps.DefaultScheduleType
                              OverallFeasibility = ScheduleFeasibilityStatus.FullyFeasible
                              OverallConfidence = ScheduleConfidenceLevel.High
                              StabilityScore = 1.0m
                              TotalScheduledHours = 0.0m
                              TotalChangeoverHours = 0.0m
                              DeliveryGuarantee = "At-least-once, per resource-period, near-real-time" }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.productionScheduleActivated.Id
                                ArsIdentifiers.Capabilities.scheduleProduction.Id
                                "ProductionSchedule"
                                envelope.AggregateId
                                notif
                        ()
                    elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.productionSchedulePublished.Id then
                        let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultPlanningPeriodId |]
                        let scopeId = parts.[0]
                        let periodId = if parts.Length > 1 then parts.[1] else deps.DefaultPlanningPeriodId

                        let notif: ProductionSchedulePublishedNotification =
                            { BaselineId = envelope.AggregateId
                              PlanningScopeId = scopeId
                              PlanningPeriodId = periodId
                              PublishedVersionNumber = int envelope.OccurrenceNumber
                              FeasibleResourceCount = 1
                              ConditionallyFeasibleResourceCount = 0
                              InfeasibleResourceCount = 0
                              AverageStabilityScore = 1.0m
                              HighVolatilityResources = []
                              ResourcesAtRiskOfDelay = []
                              MaterialsAtRiskOfUnavailability = []
                              BottleneckResources = []
                              OvertimeDependentSchedules = []
                              DeliveryGuarantee = "At-least-once, per scope-period, batch" }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.productionSchedulePublished.Id
                                ArsIdentifiers.Capabilities.scheduleProduction.Id
                                "PublishedProductionSchedule"
                                envelope.AggregateId
                                notif
                        ()
                }

            let filter =
                [ ArsIdentifiers.EnterpriseEvents.productionScheduleActivated.Id
                  ArsIdentifiers.EnterpriseEvents.productionSchedulePublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe schedule production notification node workflow: {ex.Message}")))
        }
