module Medhavi.Demand.PlanningPriorityAssignment.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.PlanningPriorityAssignment
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningPriorityAssignment.Model

let private notEmpty field =
    validate
        (fun s -> not(System.String.IsNullOrWhiteSpace s))
        (DomainError.validation(field, $"{field} cannot be empty"))

let private parsePriority p =
    match p with
    | "Critical" -> Valid Critical
    | "High" -> Valid High
    | "Medium" -> Valid Medium
    | "Low" -> Valid Low
    | "Unclassified" -> Valid Unclassified
    | invalid -> Invalid [ DomainError.validation("NewPriority", $"Invalid priority value '{invalid}'") ]

let toUpdateCmd (req: UpdatePlanningPriorityReq) : Validation<UpdatePlanningPriorityCmd, DomainError> =
    let createCmd entityType entityId =
        { EntityType = entityType
          EntityId = entityId }

    createCmd <!> notEmpty "EntityType" req.EntityType <*> notEmpty "EntityId" req.EntityId

let toOverrideCmd (req: OverridePlanningPriorityReq) : Validation<OverridePlanningPriorityCmd, DomainError> =
    let createCmd entityType entityId priority justification =
        { EntityType = entityType
          EntityId = entityId
          NewPriority = priority
          Justification = justification }

    createCmd <!> notEmpty "EntityType" req.EntityType
    <*> notEmpty "EntityId" req.EntityId
    <*> parsePriority req.NewPriority
    <*> notEmpty "Justification" req.Justification
