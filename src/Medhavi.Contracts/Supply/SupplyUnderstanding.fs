namespace Medhavi.Contracts.Supply

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums for 6 Dimensions ----------

type SupplyContinuityStatus =
    | Stable
    | AtRisk
    | Disrupted

type CapacityAdequacyStatus =
    | Sufficient
    | Constrained
    | Critical

type InventoryPositionStatus =
    | Healthy
    | AtRisk
    | Critical

type SupplierCommitmentPostureStatus =
    | HighConfidence
    | MediumConfidence
    | LowConfidence

type OverallSupplyRiskLevel =
    | Low
    | Medium
    | High
    | Critical

type SupplyUnderstandingConfidenceLevel =
    | High
    | Medium
    | Low

/// BR-S-015b / BR-S-205 — Dimension wrapper supporting Incomplete status when evidence is unavailable
type InterpretationStatus<'T> =
    | Known of 'T
    | Incomplete of reason: string

/// SE-S-001 — the six-dimensional supply interpretation.
type SupplyInterpretation =
    { Continuity: InterpretationStatus<SupplyContinuityStatus>
      ContinuityDrivers: string list
      CapacityAdequacy: InterpretationStatus<CapacityAdequacyStatus>
      Bottlenecks: string list
      InventoryPosition: InterpretationStatus<InventoryPositionStatus>
      StockoutRiskItems: string list
      ExcessInventoryItems: string list
      SupplierPosture: InterpretationStatus<SupplierCommitmentPostureStatus>
      CriticalDependencies: string list
      OverallRisk: InterpretationStatus<OverallSupplyRiskLevel>
      RiskDrivers: string list
      Confidence: InterpretationStatus<SupplyUnderstandingConfidenceLevel>
      ConfidenceFactors: string list
      ReasonCodes: string list }

// ---------- DTO ----------
type SupplyUnderstandingDto =
    { PlanningScopeId: string
      VersionNumber: int
      Interpretation: SupplyInterpretation
      EvidencePictureVersion: int64
      TransactionTime: DateTimeOffset
      LastPublishedTime: DateTimeOffset option
      State: string } // Draft / Published / Superseded

// ---------- Commands ----------
type ReviseSupplyUnderstandingReq =
    { PlanningScopeId: string
      EvidencePictureVersion: int64 }

type PublishSupplyUnderstandingReq =
    { PlanningScopeId: string
      IsPeriodicRefresh: bool }

// ---------- Notifications ----------
type SupplyUnderstandingPublishedNotification =
    { PlanningScopeId: string
      VersionNumber: int
      PublicationTime: DateTimeOffset
      SupersededVersion: int option
      MaterialChangeSummary: string
      PeriodicRefreshFlag: bool
      CompletenessScore: decimal }

// ---------- API Record ----------
type SupplyUnderstandingApi =
    { Revise: ReviseSupplyUnderstandingReq -> Task<Result<SupplyUnderstandingDto, ApiError>>
      Publish: PublishSupplyUnderstandingReq -> Task<Result<SupplyUnderstandingDto, ApiError>> }

type SupplyUnderstandingQueries = QueryService<SupplyUnderstandingDto, string>
