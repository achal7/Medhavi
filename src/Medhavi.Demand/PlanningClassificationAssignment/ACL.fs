module Medhavi.Demand.PlanningClassificationAssignment.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.PlanningClassificationAssignment
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.PlanningClassificationAssignment.Model

open System

let private notEmpty field =
    validate (fun s -> not (System.String.IsNullOrWhiteSpace s)) (DomainError.validation(field, $"{field} cannot be empty"))

let private parseClassificationType c =
    match c with
    | "ABC" -> Valid ABC
    | "XYZ" -> Valid XYZ
    | "Strategic" -> Valid Strategic
    | invalid -> Invalid [ DomainError.validation("ClassificationType", $"Invalid classification type '{invalid}'") ]

let toUpdateCmd (req: UpdatePlanningClassificationReq) : Validation<UpdatePlanningClassificationCmd, DomainError> =
    let createCmd entityType entityId classificationType =
        { EntityType = entityType
          EntityId = entityId
          ClassificationType = classificationType }

    createCmd
    <!> notEmpty "EntityType" req.EntityType
    <*> notEmpty "EntityId" req.EntityId
    <*> parseClassificationType req.ClassificationType

let toOverrideCmd
    (req: OverridePlanningClassificationReq)
    : Validation<OverridePlanningClassificationCmd, DomainError> =
    let createCmd entityType entityId classificationType newClassification justification =
        { EntityType = entityType
          EntityId = entityId
          ClassificationType = classificationType
          NewClassification = newClassification
          Justification = justification }

    createCmd
    <!> notEmpty "EntityType" req.EntityType
    <*> notEmpty "EntityId" req.EntityId
    <*> parseClassificationType req.ClassificationType
    <*> notEmpty "NewClassification" req.NewClassification
    <*> notEmpty "Justification" req.Justification
