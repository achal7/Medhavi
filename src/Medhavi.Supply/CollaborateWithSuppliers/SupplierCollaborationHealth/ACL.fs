/// Anti-Corruption Layer (ACL) for Supplier Collaboration Health Commands
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCollaborationHealth.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model

let toPublishCmd (req: PublishSupplierCollaborationHealthAssessmentReq) : Validation<PublishSupplierCollaborationHealthCmd, DomainError> =
    let scopeV = validatePlanningScopeId req.AssessmentScopeId
    let periodV = validateEvaluationPeriodId req.EvaluationPeriod
    let timestampV = validateTimestamp req.PublicationTimestamp

    (fun s p t ->
        { AssessmentScopeId = s
          EvaluationPeriodId = p
          PublicationTimestamp = t })
    <!> scopeV
    <*> periodV
    <*> timestampV
