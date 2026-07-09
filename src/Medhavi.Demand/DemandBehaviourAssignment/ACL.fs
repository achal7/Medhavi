module Medhavi.Demand.DemandBehaviourAssignment.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.DemandBehaviourAssignment
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandBehaviourAssignment.Model

let private notEmpty field =
    validate
        (fun s -> not(System.String.IsNullOrWhiteSpace s))
        (DomainError.validation(field, $"{field} cannot be empty"))

let toUpdateCmd
    (req: UpdateDemandBehaviourClassificationReq)
    : Validation<UpdateBehaviourClassificationCmd, DomainError> =
    let createCmd entityType entityId dimension =
        { EntityType = entityType
          EntityId = entityId
          BehaviourDimension = dimension }

    createCmd <!> notEmpty "EntityType" req.EntityType
    <*> notEmpty "EntityId" req.EntityId
    <*> notEmpty "BehaviourDimension" req.BehaviourDimension

let toOverrideCmd
    (req: OverrideDemandBehaviourClassificationReq)
    : Validation<OverrideBehaviourClassificationCmd, DomainError> =
    let createCmd entityType entityId dimension classification justification =
        { EntityType = entityType
          EntityId = entityId
          BehaviourDimension = dimension
          NewClassification = classification
          Justification = justification }

    createCmd <!> notEmpty "EntityType" req.EntityType
    <*> notEmpty "EntityId" req.EntityId
    <*> notEmpty "BehaviourDimension" req.BehaviourDimension
    <*> notEmpty "NewClassification" req.NewClassification
    <*> notEmpty "Justification" req.Justification
