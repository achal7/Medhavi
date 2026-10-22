namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type SupplyEntityType =
    | Supplier
    | PhysicalResource
    | TransportationLane
    | Location
    | Item

[<RequireQualifiedAccess>]
type ChangeType =
    | QuantityReduction
    | QuantityIncrease
    | DeliveryDelay
    | DeliveryExpedite
    | CapacityReduction
    | CapacityIncrease
    | Disruption
    | QualityIssue
    | PriceChange
    | RegulatoryChange

[<RequireQualifiedAccess>]
type ChangeEpistemicState =
    | NoChange
    | Observed
    | Corroborated
    | Confirmed
    | Resolved

[<RequireQualifiedAccess>]
type ChangeClassification =
    | Planned
    | Unplanned

[<RequireQualifiedAccess>]
type PlanningMateriality =
    | MaterialToPlanning
    | NonMaterial
    | Unassessed

[<RequireQualifiedAccess>]
type AssessmentConfidence =
    | Confirmed
    | High
    | Medium
    | Low
    | Unconfirmed

[<RequireQualifiedAccess>]
type SupplyChangeLifecycleState =
    | Active
    | Archived

type AssessmentChangeEventDto =
    { EventId: string
      PreviousEpistemicState: string
      NewEpistemicState: string
      TriggeringEvidence: string
      ReasonCode: string
      Timestamp: DateTimeOffset }

type SignalSummaryDto =
    { SignalId: string
      SourceId: string
      SourceType: string
      SourceReliabilityPrior: decimal
      ObservedQuantity: decimal
      ObservedDate: DateTimeOffset
      SignalGenerationTime: DateTimeOffset }

type SupplyChangeAssessmentDto =
    { AssessmentId: string
      MonitoredEntityType: string
      MonitoredEntityId: string
      ChangeType: string
      CurrentEpistemicState: string
      ChangeClassification: string
      ChangeDescription: string
      Materiality: string
      OccurrenceTime: DateTimeOffset
      DetectionTime: DateTimeOffset
      ExpectedDuration: TimeSpan option
      SignalSummary: SignalSummaryDto
      AssessmentConfidence: string
      DaysVariance: decimal
      QuantityVariance: decimal
      QuantityVariancePercent: decimal
      ThreatenedObjectives: string list
      CorrelatedChangeIds: string list
      History: AssessmentChangeEventDto list
      LifecycleState: string
      CreatedTimestamp: DateTimeOffset
      UpdatedTimestamp: DateTimeOffset }

type AssessSupplyChangeReq =
    { AssessmentId: string
      MonitoredEntityType: string
      MonitoredEntityId: string
      ChangeType: string
      SignalId: string
      SourceId: string
      SourceType: string
      SourceReliabilityPrior: decimal
      ObservedQuantity: decimal
      ObservedDate: DateTimeOffset
      SignalGenerationTime: DateTimeOffset
      Classification: string
      ExpectedDuration: TimeSpan option
      CorrelatedChangeIds: string list
      Timestamp: DateTimeOffset }

type ArchiveSupplyChangeReq =
    { AssessmentId: string
      Reason: string
      Timestamp: DateTimeOffset }

/// BN-S-080 — Supply Change Assessment Updated Notification Payload
type SupplyChangeAssessmentUpdatedNotification =
    { NotificationId: string
      ReferencedEventId: string
      SupplyEntityType: string
      SupplyEntityIdentifier: string
      ChangeType: string
      ChangeState: string
      Materiality: string
      Confidence: string
      ChangeDescription: string
      ChangeClassification: string
      CorrelatedChangeIds: string list
      Timestamp: DateTimeOffset }

// Capability & Aggregate Command API Contracts
type SupplyChangeApi =
    { Assess: AssessSupplyChangeReq -> Task<Result<SupplyChangeAssessmentDto, ApiError>>
      Archive: ArchiveSupplyChangeReq -> Task<Result<SupplyChangeAssessmentDto, ApiError>> }

/// Query service alias
type SupplyChangeQueries = QueryService<SupplyChangeAssessmentDto, string>
