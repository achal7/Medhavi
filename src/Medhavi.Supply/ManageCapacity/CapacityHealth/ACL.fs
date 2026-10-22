module Medhavi.Supply.ManageCapacity.CapacityHealth.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let toPublishCmd (req: PublishCapacityHealthReq) : Validation<PublishCapacityHealthCmd, DomainError> =
    let scopeV = validatePlanningScopeId req.AssessmentScopeId
    let periodV = validateEvaluationPeriodId req.EvaluationPeriodId

    (fun scope period ->
        { AssessmentScopeId = scope
          EvaluationPeriodId = period
          Timestamp = Timestamp.now () })
    <!> scopeV
    <*> periodV
