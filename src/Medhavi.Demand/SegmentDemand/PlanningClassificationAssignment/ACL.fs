/// Anti-Corruption Layer (ACL) for Planning Classification Assignment
/// Uses Applicative validation combinators and shared Helpers
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

let private validateOptionalAnalogItem (analogIdOpt: string option) : Validation<ItemId option, DomainError> =
    match analogIdOpt with
    | None -> Valid None
    | Some id when System.String.IsNullOrWhiteSpace id -> Valid None
    | Some id -> validateItemId id |> Validation.map Some

/// Translates ClassifyPlanningEntityReq into ClassifyPlanningEntityCmd using Applicative validation
let toClassifyCmd (req: ClassifyPlanningEntityReq) : Validation<ClassifyPlanningEntityCmd, DomainError> =
    let create entityType entityId classType analogItem =
        let id = PlanningClassificationAssignmentId.ofComponents req.EntityType entityId req.ClassificationType

        { AssignmentId = id
          EntityType = entityType
          EntityId = entityId
          ClassificationType = classType
          VolumeOrRevenuePercentage = req.VolumeOrRevenuePercentage
          HistoricalDemandValues = req.HistoricalDemandValues
          AnalogItemId = analogItem
          ClassificationTime = Timestamp.now() }

    create <!> (EntityType.FromString req.EntityType |> fromResult)
    <*> required "EntityId" req.EntityId
    <*> (ClassificationType.FromString req.ClassificationType |> fromResult)
    <*> validateOptionalAnalogItem req.AnalogItemId

/// Translates OverridePlanningClassificationReq into OverridePlanningClassificationCmd
let toOverrideCmd
    (req: OverridePlanningClassificationReq)
    : Validation<OverridePlanningClassificationCmd, DomainError> =
    let create entityType entityId classType newClass just planner =
        let id = PlanningClassificationAssignmentId.ofComponents req.EntityType entityId req.ClassificationType

        { AssignmentId = id
          EntityType = entityType
          EntityId = entityId
          ClassificationType = classType
          NewClassification = newClass
          Justification = just
          PlannerId = planner
          OverrideTime = Timestamp.now() }

    create <!> (EntityType.FromString req.EntityType |> fromResult)
    <*> required "EntityId" req.EntityId
    <*> (ClassificationType.FromString req.ClassificationType |> fromResult)
    <*> (PlanningClassification.FromString req.NewClassification |> fromResult)
    <*> required "Justification" req.Justification
    <*> required "PlannerId" req.PlannerId
