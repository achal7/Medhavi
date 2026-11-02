namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Evaluated candidate promise option synthesized by multi-echelon ATP or CTP
type CandidatePromiseOptionDto =
    { OptionId: string
      Category: string
      EarliestDeliveryDate: DateTimeOffset
      PromisedQuantity: decimal
      TotalLandedCost: decimal
      GrossMarginPct: decimal
      CarbonEmissionsKg: decimal
      SpeedScore: decimal
      CostScore: decimal
      MarginScore: decimal
      SustainabilityScore: decimal
      CompositeRankScore: decimal
      SourcingLegs: SourcingLegDto list
      FeasibilityPassed: bool
      RejectionReasons: string list }

/// Assessment result of ATP evaluation
type ATPFeasibilityAssessmentDto =
    { OrderLineId: string
      Item: string
      DestinationLocation: string
      RequestedQuantity: decimal
      RequestedDeliveryDate: DateTimeOffset
      TotalAvailableNetStock: decimal
      CandidateOptions: CandidatePromiseOptionDto list
      EvaluatedAt: DateTimeOffset }

/// Assessment result of CTP manufacturing evaluation
type CTPFeasibilityAssessmentDto =
    { OrderLineId: string
      Item: string
      PlantLocation: string
      RequestedQuantity: decimal
      EarliestProductionFinishDate: DateTimeOffset
      RequiredLeadTimeDays: int
      ComponentStockoutGaps: string list
      BottleneckWorkCenter: string option
      CandidateOption: CandidatePromiseOptionDto option
      EvaluatedAt: DateTimeOffset }

/// External request to evaluate multi-option ATP availability
type EvaluateATPOptionsReq =
    { OrderLineId: string
      CustomerId: string
      Item: string
      DestinationLocation: string
      RequestedQuantity: decimal
      RequestedDeliveryDate: DateTimeOffset
      AllowSubstitutions: bool
      AllowSplits: bool }

/// External request to evaluate capable-to-promise manufacturing slot
type EvaluateCTPOptionsReq =
    { OrderLineId: string
      CustomerId: string
      Item: string
      DestinationLocation: string
      PlantLocation: string
      RequestedQuantity: decimal
      RequestedDeliveryDate: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-010: ATP Promise Options Generated Notification
type ATPPromiseOptionsGeneratedNotification =
    { OrderLineId: string
      Item: string
      OptionCount: int
      TopOptionDeliveryDate: DateTimeOffset option
      NotificationTime: DateTimeOffset }

/// BN-P-011: CTP Feasibility Evaluated Notification
type CTPFeasibilityEvaluatedNotification =
    { OrderLineId: string
      Item: string
      Feasible: bool
      EarliestFinishDate: DateTimeOffset option
      NotificationTime: DateTimeOffset }

/// Public API for Promising Evaluation Engine (CA-P-004, CA-P-005)
type PromiseEvaluationApi =
    { EvaluateATP: EvaluateATPOptionsReq -> Task<Result<ATPFeasibilityAssessmentDto, ApiError>>
      EvaluateCTP: EvaluateCTPOptionsReq -> Task<Result<CTPFeasibilityAssessmentDto, ApiError>> }

