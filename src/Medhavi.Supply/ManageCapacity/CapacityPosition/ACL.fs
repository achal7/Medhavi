module Medhavi.Supply.ManageCapacity.CapacityPosition.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
let toUpdateCmd (req: UpdateCapacityPositionReq) : Validation<UpdateCapacityPositionCmd, DomainError> =
    let resourceV = validatePhysicalResourceId req.PhysicalResourceId

    (fun resource ->
        { ResourceId = resource
          TriggeringEvidence = req.TriggeringEvidence
          Timestamp = Timestamp.now () })
    <!> resourceV
