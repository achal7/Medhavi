/// CA-P-011 Learn & Calibrate Promising Models — ACL
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.ACL

open System
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toProposeCmd (req: ProposeCalibrationReq) : Validation<ProposeModelCalibrationCmd, DomainError> =
    let create modelId scopeId evalTime =
        { ModelId = modelId
          ScopeId = scopeId
          ObservationWindowDays = req.ObservationWindowDays
          EvaluationTime = evalTime }

    create <!> validatePromiseLearningId req.ModelId
           <*> validatePlanningScopeId req.PlanningScopeId
           <*> validateTimestamp req.EvaluationTime

let toApplyCmd (req: ApplyCalibrationReq) : Validation<ApplyModelCalibrationCmd, DomainError> =
    let create modelId actTime =
        { ModelId = modelId
          ApprovedBySteward = req.ApprovedBySteward
          ActivationTime = actTime }

    create <!> validatePromiseLearningId req.ModelId
           <*> validateTimestamp req.ActivationTime

let toArsEventId = function
    | PromiseCalibrationProposed _ -> ArsIdentifiers.EnterpriseEvents.promiseCalibrationProposed.Id
    | PromiseCalibrationApplied _ -> ArsIdentifiers.EnterpriseEvents.promiseCalibrationApplied.Id

