namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// =============================================================================
// CA-D-008 & SE-D-009 — Demand Exception Public Contracts
// =============================================================================

/// Demand Exception Evidence DTO representing detection or resolution published to Core Exception Management
type DemandExceptionEvidenceDto =
    { EvidenceId: string
      ExceptionType: string
      EntityType: string
      EntityId: string
      ScopeId: string
      Severity: string
      TriggeringMetric: string
      MetricValue: decimal
      ThresholdValue: decimal
      Rationale: string
      IsResolution: bool
      Timestamp: DateTimeOffset }

// ---------- Commands / Requests ----------

/// Request payload to evaluate demand exception evidence against governed detection policies
type EvaluateDemandExceptionReq =
    { ScopeId: string
      EntityType: string
      EntityId: string
      ExceptionType: string
      TriggeringMetric: string
      MetricValue: decimal
      HistoricalValues: decimal list option }

// ---------- API Record ----------

type DemandExceptionApi =
    { EvaluateException: EvaluateDemandExceptionReq -> Task<Result<DemandExceptionEvidenceDto option, ApiError>> }

/// Query service alias
type DemandExceptionQueries = QueryService<DemandExceptionEvidenceDto, string>
