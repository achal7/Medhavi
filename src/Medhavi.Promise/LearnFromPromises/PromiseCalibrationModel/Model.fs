/// CA-P-011 Learn & Calibrate Promising Models — SE-P-100 Domain Model
/// Architectural Lineage: ARS §3, §4, §5, §14, §16, §17, §18; Table A.1 SE-P-100; AB-P-026, AB-P-027
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Model

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise

type CalibrationDomainClassification =
    | TransitLane
    | WorkCenterCapacity
    | SupplierLeadTime
    | SafetyBuffer

type LearningLifecycleState =
    | DriftDetected
    | CalibrationProposed of ProposedAt: Timestamp
    | Ratified of Steward: string * RatifiedAt: Timestamp
    | Applied of AppliedAt: Timestamp
    | Dismissed of Reason: string * DismissedAt: Timestamp

type DriftObservationRecord =
    { ObservationId: string
      CommitmentRef: string
      PlannedBaselineValue: decimal
      ObservedActualValue: decimal
      VarianceDelta: decimal
      ObservationEpoch: Timestamp }

type CalibratedParameter =
    { ParameterName: string
      TargetEntityType: string
      EntityIdentifier: string
      BaselineValue: decimal
      EmpiricalObservedValue: decimal
      RecommendedCalibratedValue: decimal
      DampenedStepDelta: decimal }

type PromiseCalibrationModel =
    { ModelId: PromiseLearningId
      ScopeId: PlanningScopeId
      ObservationWindowDays: int
      SampleSizeCount: int
      Parameters: CalibratedParameter list
      LifecycleState: LearningLifecycleState
      DecisionTraceability: string option }

type ProposeModelCalibrationCmd =
    { ModelId: PromiseLearningId
      ScopeId: PlanningScopeId
      ObservationWindowDays: int
      EvaluationTime: Timestamp }

type ApplyModelCalibrationCmd =
    { ModelId: PromiseLearningId
      ApprovedBySteward: string
      ActivationTime: Timestamp }

type PromiseCalibrationEvent =
    | PromiseCalibrationProposed of
        {| ModelId: PromiseLearningId
           ScopeId: PlanningScopeId
           AdjustedParametersCount: int
           EvaluationTime: Timestamp |}
    | PromiseCalibrationApplied of
        {| ModelId: PromiseLearningId
           ScopeId: PlanningScopeId
           ApprovedBy: string
           ActivationTime: Timestamp |}
