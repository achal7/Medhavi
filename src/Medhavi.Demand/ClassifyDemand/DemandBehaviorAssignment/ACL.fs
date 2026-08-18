/// Anti-Corruption Layer (ACL) for Demand Behavior Assignment
module Medhavi.Demand.ClassifyDemand.DemandBehaviorAssignment.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model

/// Translates ClassifyDemandBehaviorReq into ClassifyDemandBehaviorCmd
let toClassifyCmd (req: ClassifyDemandBehaviorReq) : Validation<ClassifyDemandBehaviorCmd, DomainError> =
    let create item loc dim =
        let id = DemandBehaviorAssignmentId.ofComponents item loc req.Dimension

        { AssignmentId = id
          Item = item
          Location = loc
          Dimension = dim
          DemandQuantities = req.DemandQuantities
          ClassificationTime = Timestamp.now() }

    create <!> validateItemId req.ItemId
    <*> validateLocationId req.LocationId
    <*> (BehaviorDimension.FromString req.Dimension |> fromResult)

/// Translates OverrideDemandBehaviorReq into OverrideDemandBehaviorCmd
let toOverrideCmd (req: OverrideDemandBehaviorReq) : Validation<OverrideDemandBehaviorCmd, DomainError> =
    let create item loc dim newClass just planner =
        let id = DemandBehaviorAssignmentId.ofComponents item loc req.Dimension

        { AssignmentId = id
          Item = item
          Location = loc
          Dimension = dim
          NewClassification = newClass
          Justification = just
          PlannerId = planner
          OverrideTime = Timestamp.now() }

    create <!> validateItemId req.ItemId
    <*> validateLocationId req.LocationId
    <*> (BehaviorDimension.FromString req.Dimension |> fromResult)
    <*> (BehaviorClass.FromString req.NewClassification |> fromResult)
    <*> required "Justification" req.Justification
    <*> required "PlannerId" req.PlannerId
