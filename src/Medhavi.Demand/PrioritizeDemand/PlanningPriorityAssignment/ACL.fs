/// Anti-Corruption Layer (ACL) for Planning Priority Assignment
module Medhavi.Demand.PrioritizeDemand.PlanningPriorityAssignment.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

/// Translates PrioritizePlanningEntityReq into PrioritizePlanningEntityCmd
let toPrioritizeCmd (req: PrioritizePlanningEntityReq) : Validation<PrioritizePlanningEntityCmd, DomainError> =
    let create entityType entityId =
        let id = PlanningPriorityAssignmentId.ofComponents req.EntityType entityId

        { AssignmentId = id
          EntityType = entityType
          EntityId = entityId
          RevenueContribution = req.RevenueContribution
          StrategicImportance = req.StrategicImportance
          RiskExposure = req.RiskExposure
          ContractualObligation = req.ContractualObligation
          PrioritizationTime = Timestamp.now() }

    create <!> (EntityType.FromString req.EntityType |> fromResult) <*> required "EntityId" req.EntityId

/// Translates OverridePlanningPriorityReq into OverridePlanningPriorityCmd
let toOverrideCmd (req: OverridePlanningPriorityReq) : Validation<OverridePlanningPriorityCmd, DomainError> =
    let create entityType entityId newPriority just planner =
        let id = PlanningPriorityAssignmentId.ofComponents req.EntityType entityId

        { AssignmentId = id
          EntityType = entityType
          EntityId = entityId
          NewPriority = newPriority
          Justification = just
          PlannerId = planner
          OverrideTime = Timestamp.now() }

    create <!> (EntityType.FromString req.EntityType |> fromResult)
    <*> required "EntityId" req.EntityId
    <*> (PriorityLevel.FromString req.NewPriority |> fromResult)
    <*> required "Justification" req.Justification
    <*> required "PlannerId" req.PlannerId
