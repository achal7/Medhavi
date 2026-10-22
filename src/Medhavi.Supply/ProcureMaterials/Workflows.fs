namespace Medhavi.Supply.ProcureMaterials

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Medhavi.Supply.ArsIdentifiers

type ProcureMaterialsWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      DispatchEnvelope: Envelope -> Task<unit>
      Apis: ProcureMaterialsApis
      DefaultRequirementId: string
      DefaultPlanId: string
      DefaultItemId: string
      DefaultLocationId: string
      DefaultScopeId: string
      DefaultCycleId: string
      DefaultAuthority: string
      DefaultRequiredQuantity: decimal
      DefaultLeadTimeDays: float }

/// CA-S-006 Procure Materials Workflows
/// Traces to: BW-S-060, BW-S-061, FS-S-060, FS-S-061
module Workflows =

    /// FS-S-060 / BW-S-060: Create Procurement Recommendations Workflow
    /// Subscribes to Supply Plan (BN-S-010), Inventory (BN-S-020), Capacity (BN-S-030), Supplier Commitments (BN-S-040)
    let createProcurementRecommendationsWorkflow
        (deps: ProcureMaterialsWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let req: CreateProcurementRecommendationReq =
                        { RequirementId = deps.DefaultRequirementId
                          SourceSupplyPlanId = deps.DefaultPlanId
                          ItemId = deps.DefaultItemId
                          LocationId = deps.DefaultLocationId
                          RequiredDate = envelope.CreatedUtc.AddDays(deps.DefaultLeadTimeDays)
                          RequiredQuantity = deps.DefaultRequiredQuantity
                          EvaluationTimestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.Recommendation.Create req
                    match res with
                    | Ok _ -> ()
                    | Error _ -> ()
                }

            let filter =
                [ ArsIdentifiers.BusinessNotifications.supplyPlanPublished.Id
                  ArsIdentifiers.BusinessNotifications.inventoryPositionAssessmentChanged.Id
                  ArsIdentifiers.BusinessNotifications.capacityPositionAssessmentChanged.Id
                  ArsIdentifiers.BusinessNotifications.supplierCommitmentChanged.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe procurement recommendations workflow: {ex.Message}")))
        }

    /// FS-S-061 / BW-S-061: Publish Procurement Plan Workflow
    /// Triggered on governed planning cadence
    let createProcurementPlanPublicationWorkflow
        (deps: ProcureMaterialsWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    let req: PublishProcurementPlanReq =
                        { PlanningScopeId = deps.DefaultScopeId
                          PlanningCycleId = deps.DefaultCycleId
                          PublicationAuthority = deps.DefaultAuthority
                          PublicationTimestamp = envelope.CreatedUtc }

                    let! res = deps.Apis.Plan.Publish req
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
                return Error (Infrastructure(EventStore($"Failed to subscribe procurement plan publication workflow: {ex.Message}")))
        }

    /// BW-S-060 / BW-S-061 Notification Node: Dispatches BN-S-050, BN-S-051 strictly after events exist
    let createNotificationNodeWorkflow
        (deps: ProcureMaterialsWorkflowDependencies)
        (ct: CancellationToken)
        : TaskResult<IDisposable, ApplicationError> =
        task {
            let processEnvelope (envelope: Envelope) : Task<unit> =
                task {
                    if envelope.EventType = ArsIdentifiers.EnterpriseEvents.procurementRecommendationCreated.Id then
                        let notif: ProcurementRecommendationCreatedNotification =
                            { RequirementId = envelope.AggregateId
                              VersionNumber = int envelope.OccurrenceNumber
                              SourcingStrategy = SourcingStrategyType.SingleSource
                              SupplierAllocations = []
                              ProcurementFeasibility = ProcurementFeasibilityStatus.Feasible
                              OverallConfidence = RecommendationConfidenceLevel.High
                              GovernanceStatus = ProcurementGovernanceStatus.Authorized
                              DeliveryGuarantee = "At-least-once, per requirement, near-real-time" }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.procurementRecommendationCreated.Id
                                ArsIdentifiers.Capabilities.procureMaterials.Id
                                "ProcurementRecommendation"
                                envelope.AggregateId
                                notif
                        ()
                    elif envelope.EventType = ArsIdentifiers.EnterpriseEvents.procurementPlanPublished.Id then
                        let parts = if envelope.AggregateId.Contains("#") then envelope.AggregateId.Split('#') else [| envelope.AggregateId; deps.DefaultCycleId |]
                        let scopeId = parts.[0]
                        let cycleId = if parts.Length > 1 then parts.[1] else deps.DefaultCycleId

                        let metrics: ProcurementAggregateMetricsDto =
                            { TotalSpend = 0.0m
                              SupplierAllocationPercentages = []
                              ConfidenceDistribution = []
                              FeasibilitySummary = [] }

                        let risks: ProcurementRiskSignalsDto =
                            { SupplierConcentration = []
                              SingleSourceExposure = []
                              GeographicConcentration = []
                              ProcurementConfidenceDistribution = [] }

                        let recommendations: ProcurementRecommendationSignalsDto =
                            { ItemsRequiringPlannerReview = []
                              SupplierConsolidationOpportunities = []
                              RiskConcentrationAlerts = []
                              UnresolvedFeasibilityIssues = [] }

                        let notif: ProcurementPlanPublishedNotification =
                            { PlanningScopeId = scopeId
                              PlanningCycleId = cycleId
                              VersionNumber = int envelope.OccurrenceNumber
                              AggregateMetrics = metrics
                              RiskSignals = risks
                              RecommendationSignals = recommendations
                              DeliveryGuarantee = "At-least-once, per plan, near-real-time" }

                        let! _ =
                            dispatchNotification
                                deps.DispatchEnvelope
                                ArsIdentifiers.BusinessNotifications.procurementPlanPublished.Id
                                ArsIdentifiers.Capabilities.procureMaterials.Id
                                "ProcurementPlan"
                                envelope.AggregateId
                                notif
                        ()
                }

            let filter =
                [ ArsIdentifiers.EnterpriseEvents.procurementRecommendationCreated.Id
                  ArsIdentifiers.EnterpriseEvents.procurementPlanPublished.Id ]
                |> EnvelopeFilter.EventTypes

            try
                let! sub = deps.Subscribe filter processEnvelope ct
                return Ok sub
            with ex ->
                return Error (Infrastructure(EventStore($"Failed to subscribe procurement notification node workflow: {ex.Message}")))
        }
