namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Individual parameter calibration record
type CalibratedParameterDto =
    { ParameterName: string
      TargetEntityType: string
      EntityIdentifier: string
      BaselineValue: decimal
      EmpiricalObservedValue: decimal
      RecommendedCalibratedValue: decimal
      DampenedStepDelta: decimal }

/// SE-P-100 Promise Calibration Model Data Transfer Object
type PromiseCalibrationModelDto =
    { ModelId: string
      PlanningScopeId: string
      ObservationWindowDays: int
      SampleSizeCount: int
      AdjustedParametersCount: int
      AverageBufferDeltaDays: decimal
      Parameters: CalibratedParameterDto list
      LifecycleState: string
      DecisionTraceability: string option }

/// External request to evaluate historical fulfillment variance and propose calibrations
type ProposeCalibrationReq =
    { ModelId: string
      PlanningScopeId: string
      ObservationWindowDays: int
      EvaluationTime: DateTimeOffset }

/// External request to activate calibrated model parameters in active promising engines
type ApplyCalibrationReq =
    { ModelId: string
      ApprovedBySteward: string
      ActivationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-020: Promising Parameters Calibrated Notification
type PromisingParametersCalibratedNotification =
    { ModelId: string
      PlanningScopeId: string
      AdjustedParameterCount: int
      EffectiveTimestamp: DateTimeOffset }

/// BN-P-021: Systematic Promise Bias Detected Notification
type SystematicPromiseBiasDetectedNotification =
    { PlanningScopeId: string
      AffectedEntityType: string
      AffectedEntityId: string
      MeanDelayVarianceDays: decimal
      StatisticalSignificancePct: decimal
      NotificationTime: DateTimeOffset }

/// BN-P-022: Calibration Recommendation Published Notification
type CalibrationRecommendationPublishedNotification =
    { ModelId: string
      PlanningScopeId: string
      RecommendationSummary: string
      ReviewActionRequired: bool
      NotificationTime: DateTimeOffset }

/// Public API for Promising Model Calibration & Learning (CA-P-011)
type PromiseLearningApi =
    { ProposeCalibration: ProposeCalibrationReq -> Task<Result<PromiseCalibrationModelDto, ApiError>>
      ApplyCalibration: ApplyCalibrationReq -> Task<Result<PromiseCalibrationModelDto, ApiError>> }

/// Query service alias for Promise Calibrations
type PromiseLearningQueries = QueryService<PromiseCalibrationModelDto, string>

