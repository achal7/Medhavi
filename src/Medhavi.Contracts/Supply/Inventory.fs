namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Policy & Position DTOs ----------

type InventoryPolicyParametersDto =
    { ReplenishmentStrategy: string
      SafetyStockQuantity: decimal
      ReorderPointQuantity: decimal option
      MinimumQuantity: decimal option
      MaximumQuantity: decimal option
      TargetStockLevel: decimal
      TargetServiceLevel: decimal
      ReviewFrequencyDays: int option
      LeadTimeDays: int
      DemandVariability: decimal }

type InventoryAssessmentChangeEventDto =
    { SequenceNumber: int
      ChangeTimestamp: DateTimeOffset
      PreviousHealthStatus: string
      NewHealthStatus: string
      PreviousRiskLevel: string
      NewRiskLevel: string
      TriggeringEvidence: string list
      ReasonCode: string }

type InventoryPositionAssessmentDto =
    { ItemId: string
      LocationId: string
      LifecycleState: string
      OnHandQuantity: decimal
      AllocatedQuantity: decimal
      InTransitQuantity: decimal
      AvailableQuantity: decimal
      DaysOfSupply: decimal
      CoverageStatus: string
      RiskLevel: string
      HealthStatus: string
      PolicyComplianceStatus: string
      FinancialExposure: decimal option
      SourceSupplyUnderstandingVersion: int option
      SourceSupplyPlanVersion: int option
      AssessmentTimestamp: DateTimeOffset
      ChangeHistory: InventoryAssessmentChangeEventDto list }

type InventoryPolicyChangeEventDto =
    { SequenceNumber: int
      ChangeTimestamp: DateTimeOffset
      PreviousParameters: InventoryPolicyParametersDto option
      NewParameters: InventoryPolicyParametersDto
      TriggeringEvidence: string list
      ReasonCode: string }

type InventoryPolicyAssignmentDto =
    { ItemId: string
      LocationId: string
      LifecycleState: string
      Parameters: InventoryPolicyParametersDto
      EffectiveDate: DateTimeOffset
      AssignmentTimestamp: DateTimeOffset
      ChangeHistory: InventoryPolicyChangeEventDto list }

// ---------- Health Assessment DTOs ----------

type ItemHealthSummaryDto =
    { ItemId: string
      LocationId: string
      HealthClassification: string
      DaysOfSupply: decimal
      PolicyComplianceStatus: string
      StockoutRiskExposure: decimal
      ExcessValue: decimal }

type InventoryHealthAssessmentVersionDto =
    { VersionNumber: int
      LifecycleState: string // Draft, Published, Superseded
      AssessmentScopeId: string
      EvaluationPeriodId: string
      ItemHealthSummaries: ItemHealthSummaryDto list
      TotalExcessValue: decimal
      TotalStockoutRiskExposure: decimal
      AssessmentConfidence: decimal
      AssessmentRationale: string
      PublicationTimestamp: DateTimeOffset option }

type InventoryHealthAssessmentDto =
    { AssessmentScopeId: string
      EvaluationPeriodId: string
      CurrentVersion: InventoryHealthAssessmentVersionDto option
      AllVersions: InventoryHealthAssessmentVersionDto list }

// ---------- Commands / Requests ----------

type UpdateInventoryPositionReq =
    { ItemId: string
      LocationId: string
      TriggeringEvidence: string list }

type UpdateInventoryPolicyReq =
    { ItemId: string
      LocationId: string
      TargetServiceLevel: decimal option
      ManualOverrideParameters: InventoryPolicyParametersDto option }

type PublishInventoryHealthReq =
    { AssessmentScopeId: string
      EvaluationPeriodId: string }

// ---------- Business Notifications ----------

/// BN-S-020: Inventory Position Assessment Changed
type InventoryPositionAssessmentChangedNotification =
    { ItemId: string
      LocationId: string
      HealthStatus: string
      RiskLevel: string
      CoverageStatus: string
      ChangeTimestamp: DateTimeOffset
      ReasonCode: string }

/// BN-S-021: Inventory Policy Assignment Updated
type InventoryPolicyAssignmentUpdatedNotification =
    { ItemId: string
      LocationId: string
      ReplenishmentStrategy: string
      SafetyStockQuantity: decimal
      ReorderPointQuantity: decimal option
      AssignmentTimestamp: DateTimeOffset }

/// BN-S-022: Inventory Health Assessment Published
type InventoryHealthAssessmentPublishedNotification =
    { AssessmentScopeId: string
      EvaluationPeriodId: string
      VersionNumber: int
      TotalExcessValue: decimal
      TotalStockoutRiskExposure: decimal
      PublicationTimestamp: DateTimeOffset }

// ---------- API Records ----------

type InventoryPositionApi =
    { Update: UpdateInventoryPositionReq -> Task<Result<InventoryPositionAssessmentDto, ApiError>> }

type InventoryPolicyApi =
    { Update: UpdateInventoryPolicyReq -> Task<Result<InventoryPolicyAssignmentDto, ApiError>> }

type InventoryHealthApi =
    { Publish: PublishInventoryHealthReq -> Task<Result<InventoryHealthAssessmentDto, ApiError>> }

type ManageInventoryApis =
    { Position: InventoryPositionApi
      Policy: InventoryPolicyApi
      Health: InventoryHealthApi }

/// Query service aliases
type InventoryPositionQueries = QueryService<InventoryPositionAssessmentDto, string>
type InventoryPolicyQueries = QueryService<InventoryPolicyAssignmentDto, string>
type InventoryHealthQueries = QueryService<InventoryHealthAssessmentDto, string>
