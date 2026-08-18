namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Governed Enums ----------
type ContinuityStatus =
    | Stable
    | Increasing
    | Declining
    | Volatile
    | Incomplete

type PatternStatus =
    | Normal
    | Seasonal
    | Irregular
    | StepChange
    | Incomplete

type HealthStatus =
    | Healthy
    | AtRisk
    | Critical
    | Incomplete

type VolatilityLevel =
    | Low
    | Medium
    | High
    | Incomplete

type ConfidenceLevel =
    | High
    | Medium
    | Low

type InterpretationStatus<'T> =
    | Known of 'T
    | Incomplete of reason: string

/// SE-D-012 — the four-dimensional demand interpretation.
type Interpretation =
    { Continuity: InterpretationStatus<ContinuityStatus>
      ContinuityDrivers: string list
      Pattern: InterpretationStatus<PatternStatus>
      PatternConfidence: InterpretationStatus<ConfidenceLevel>
      Health: InterpretationStatus<HealthStatus>
      HealthConcerns: string list
      Volatility: InterpretationStatus<VolatilityLevel>
      VolatilityDrivers: string list
      ReasonCodes: string list }

// ---------- DTO ----------
type DemandUnderstandingDto =
    { PlanningScopeId: string
      VersionNumber: int
      Interpretation: Interpretation
      LastPublishedTime: DateTimeOffset option
      State: string } // Draft / Published / Superseded

// ---------- Commands ----------
type ReviseDemandUnderstandingReq =
    { PlanningScopeId: string
      EvidencePictureVersion: int64 }

type PublishDemandUnderstandingReq =
    { PlanningScopeId: string
      IsPeriodicRefresh: bool }

// ---------- Notifications ----------
type DemandUnderstandingPublishedNotification =
    { PlanningScopeId: string
      VersionNumber: int
      PublicationTime: DateTimeOffset
      SupersededVersion: int option
      MaterialChangeSummary: string }

// ---------- API Record ----------
type DemandUnderstandingApi =
    { Revise: ReviseDemandUnderstandingReq -> Task<Result<DemandUnderstandingDto, ApiError>>
      Publish: PublishDemandUnderstandingReq -> Task<Result<DemandUnderstandingDto, ApiError>> }
