namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

[<RequireQualifiedAccess>]
type SupplierCommitmentState =
    | Proposed
    | Acknowledged
    | Committed
    | PartiallyCommitted
    | Rejected
    | AlternativeProposed
    | Confirmed

[<RequireQualifiedAccess>]
type CollaborationEventType =
    | EnterpriseRequest
    | SupplierAcknowledgement
    | SupplierCommitment
    | SupplierRejection
    | SupplierAlternative
    | EnterpriseConfirmation

[<RequireQualifiedAccess>]
type CollaborationActor =
    | Enterprise
    | Supplier

[<RequireQualifiedAccess>]
type CommitmentConfidenceLevel =
    | Firm
    | Conditional
    | Tentative
    | Estimated

[<RequireQualifiedAccess>]
type SupplierResponseAcceptanceOutcome =
    | AcceptAsFirm
    | AcceptWithBuffer of AdditionalSafetyStock: decimal * ExtendedLeadTimeDays: decimal
    | EscalateForReview of Reason: string
    | RejectAndSeekAlternative of Reason: string

type CollaborationEventDto =
    {
        SequenceNumber: int
        EventType: CollaborationEventType
        EventTimestamp: DateTimeOffset
        Actor: CollaborationActor
        Details: string
    }

type SupplierCommitmentDto =
    {
        SupplierId: string
        ItemId: string
        RequiredDate: DateTimeOffset
        RequestedQuantity: decimal
        RequestedDate: DateTimeOffset
        RequestTimestamp: DateTimeOffset
        CurrentState: SupplierCommitmentState
        CommittedQuantity: decimal option
        CommittedDate: DateTimeOffset option
        ResponseTimestamp: DateTimeOffset option
        Confidence: CommitmentConfidenceLevel
        AlternativeDescription: string option
        AlternativeQuantity: decimal option
        AlternativeDate: DateTimeOffset option
        SupplierAssumptions: string list
        CollaborationEvents: CollaborationEventDto list
        AcceptanceOutcome: SupplierResponseAcceptanceOutcome option
        DataQualityFlag: string option
    }

type DimensionalExposureDto =
    {
        DimensionName: string
        Level: string
        QuantityAtRisk: decimal
        ValuationAtRisk: decimal
        ReasonCodes: string list
        ThreatenedObjectives: string list
    }

type SupplierCommitmentAssessmentDto =
    {
        SupplierId: string
        LifecycleState: string
        AssessmentTimestamp: DateTimeOffset
        OverallConfidence: string
        Exposures: DimensionalExposureDto list
        OnTimeDeliveryRate: decimal
        QuantityFulfillmentRate: decimal
        AverageAcknowledgementDurationHours: decimal
        AverageCommitmentDurationHours: decimal
        ActiveRiskSignals: string list
        AuthoritativeHealthState: string
    }

type SupplierCollaborationHealthAssessmentDto =
    {
        AssessmentScopeId: string
        EvaluationPeriod: string
        AssessmentVersionId: string
        VersionNumber: int
        LifecycleState: string
        PublicationTimestamp: DateTimeOffset
        OverallPublicationOutcome: string
        SupplierClassifications: (string * string) list
        ResponsivenessTrends: (string * string) list
        CommitmentAccuracyTrends: (string * string) list
        RecommendationSignals: string list
    }

// API Request Records
type RecordEnterpriseRequestReq =
    {
        SupplierId: string
        ItemId: string
        RequiredDate: DateTimeOffset
        RequestedQuantity: decimal
        RequestedDate: DateTimeOffset
        RequestTimestamp: DateTimeOffset
    }

type RecordSupplierResponseReq =
    {
        SupplierId: string
        ItemId: string
        RequiredDate: DateTimeOffset
        ResponseEventType: CollaborationEventType
        CommittedQuantity: decimal option
        CommittedDate: DateTimeOffset option
        AlternativeDescription: string option
        AlternativeQuantity: decimal option
        AlternativeDate: DateTimeOffset option
        SupplierAssumptions: string list
        ResponseTimestamp: DateTimeOffset
    }

type UpdateSupplierCommitmentAssessmentReq =
    {
        SupplierId: string
        AssessmentTimestamp: DateTimeOffset
    }

type PublishSupplierCollaborationHealthAssessmentReq =
    {
        AssessmentScopeId: string
        EvaluationPeriod: string
        PublicationTimestamp: DateTimeOffset
    }

// Business Notifications (Dispatched post-persistence)
type SupplierCommitmentChangedNotification =
    {
        SupplierId: string
        ItemId: string
        RequiredDate: DateTimeOffset
        PreviousState: SupplierCommitmentState
        NewState: SupplierCommitmentState
        CommittedQuantity: decimal option
        CommittedDate: DateTimeOffset option
        Confidence: CommitmentConfidenceLevel
        SequenceNumber: int
    }

type SupplierCommitmentAssessmentChangedNotification =
    {
        SupplierId: string
        EvaluationPeriod: string
        CommitmentReliabilityScore: decimal
        CommitmentRiskScore: decimal
        AssessmentState: string
        AuthoritativeHealthState: string
    }

type SupplierCollaborationHealthAssessmentPublishedNotification =
    {
        AssessmentScopeId: string
        EvaluationPeriod: string
        VersionNumber: int
        AssessmentVersionId: string
        PublicationTimestamp: DateTimeOffset
        PublicationOutcome: string
        SupplierCount: int
    }

// Capability & Aggregate Command API Contracts
type SupplierCommitmentApi =
    {
        RecordEnterpriseRequest: RecordEnterpriseRequestReq -> Task<Result<SupplierCommitmentDto, ApiError>>
        RecordSupplierResponse: RecordSupplierResponseReq -> Task<Result<SupplierCommitmentDto, ApiError>>
    }

type SupplierCommitmentAssessmentApi =
    {
        Update: UpdateSupplierCommitmentAssessmentReq -> Task<Result<SupplierCommitmentAssessmentDto, ApiError>>
    }

type SupplierCollaborationHealthApi =
    {
        Publish:
            PublishSupplierCollaborationHealthAssessmentReq
                -> Task<Result<SupplierCollaborationHealthAssessmentDto, ApiError>>
    }

type CollaborateWithSuppliersApis =
    {
        Commitment: SupplierCommitmentApi
        Assessment: SupplierCommitmentAssessmentApi
        Health: SupplierCollaborationHealthApi
    }

/// Query service aliases
type SupplierCommitmentQueries = QueryService<SupplierCommitmentDto, string>
type SupplierCommitmentAssessmentQueries = QueryService<SupplierCommitmentAssessmentDto, string>
type SupplierCollaborationHealthQueries = QueryService<SupplierCollaborationHealthAssessmentDto, string>
