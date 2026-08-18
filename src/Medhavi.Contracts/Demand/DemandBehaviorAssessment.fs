namespace Medhavi.Contracts.Demand

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-D-004 Demand Behavior State
type DemandBehaviorState =
    | Normal
    | Elevated
    | Depressed
    | Critical

/// Deviation Direction relative to baseline
type DeviationDirection =
    | Increase
    | Decrease

/// Assessment Confidence
type AssessmentConfidence =
    | High
    | Medium
    | Low

/// State change event history record
type StateChangeEventDto =
    { Timestamp: DateTimeOffset
      FromState: string
      ToState: string
      DeviationMagnitude: decimal
      Direction: string
      Confidence: string
      CorroboratingSources: string list
      BaselineReference: string
      DecisionTraceId: string }

/// SE-D-004 Demand Behavior Assessment Data Transfer Object
type DemandBehaviorAssessmentDto =
    { AssessmentId: string
      Item: string
      Location: string
      CurrentState: string
      BaselineMean: decimal
      BaselineStdDev: decimal
      LastDeviationMagnitude: decimal option
      LastDeviationDirection: string option
      CorroborationCount: int
      AssessmentConfidence: string
      StateChangeEvents: StateChangeEventDto list }

/// External request to initialize baseline parameters for an Item-Location
type InitializeBaselineReq =
    { Item: string
      Location: string
      BaselineMean: decimal
      BaselineStdDev: decimal }

/// External request to evaluate an incoming demand signal against the baseline
type EvaluateDemandSignalReq =
    { Item: string
      Location: string
      Quantity: decimal
      SignalTimestamp: DateTimeOffset
      CorroboratingSources: string list
      IsHighPriority: bool }

/// External request to evaluate whether Critical state warrants an out-of-cycle forecast refresh (DE-D-007)
type EvaluateForecastRefreshReq =
    { Item: string
      Location: string
      ForecastAgeHours: int
      ExpectedAccuracyImprovementWape: decimal }

/// DTO representing the outcome of forecast refresh evaluation (DE-D-007)
type ForecastRefreshDecisionDto =
    { SelectedAlternative: string
      Rationale: string
      DecisionTraceId: string }

/// Public API for Demand Behavior Assessment (SE-D-004)
type DemandBehaviorAssessmentApi =
    { InitializeBaseline: InitializeBaselineReq -> Task<Result<DemandBehaviorAssessmentDto, ApiError>>
      EvaluateSignal: EvaluateDemandSignalReq -> Task<Result<DemandBehaviorAssessmentDto, ApiError>>
      EvaluateForecastRefresh: EvaluateForecastRefreshReq -> Task<Result<ForecastRefreshDecisionDto, ApiError>> }

/// Query service alias
type DemandBehaviorAssessmentQueries = QueryService<DemandBehaviorAssessmentDto, string>
