/// Anti-Corruption Layer (ACL) for Supplier Commitment Assessment Commands
module Medhavi.Supply.CollaborateWithSuppliers.SupplierCommitmentAssessment.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model

let toUpdateCmd (req: UpdateSupplierCommitmentAssessmentReq) : Validation<UpdateSupplierCommitmentAssessmentCmd, DomainError> =
    let supplierV = validateSupplierId req.SupplierId
    let timestampV = validateTimestamp req.AssessmentTimestamp

    (fun s t ->
        { SupplierId = s
          AssessmentTimestamp = t })
    <!> supplierV
    <*> timestampV
