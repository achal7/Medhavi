/// SE-S-032 — Inventory Health Anti-Corruption Layer
module Medhavi.Supply.ManageInventory.InventoryHealth.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model

let toPublishCmd (req: PublishInventoryHealthReq) : Validation<PublishInventoryHealthCmd, DomainError> =
    let scopeV = validatePlanningScopeId req.AssessmentScopeId
    let periodV = validateEvaluationPeriodId req.EvaluationPeriodId

    (fun scope period ->
        { AssessmentScopeId = scope
          EvaluationPeriodId = period
          Timestamp = Timestamp.now () })
    <!> scopeV
    <*> periodV
