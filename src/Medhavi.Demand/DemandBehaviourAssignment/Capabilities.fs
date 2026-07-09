module Medhavi.Demand.DemandBehaviourAssignment.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts.Demand.DemandBehaviourAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandBehaviourAssignment.ACL
open Medhavi.Demand.DemandBehaviourAssignment.Model

let private publishChanged (ass: DemandBehaviourAssignment) =
    // Publish BN-D-019 Demand Behaviour Classification Changed
    DomainEventBus.Publish(
        { EntityType = ass.EntityType
          EntityId = ass.EntityId
          BehaviourDimension = ass.BehaviourDimension
          PreviousClassification =
            ass.LastChangeEvent |> Option.bind(fun e -> e.PreviousClassification) |> Option.defaultValue ""
          NewClassification = ass.CurrentClassification
          Confidence = PositiveDecimal.value ass.ClassificationConfidence }
        : DemandBehaviourClassificationChangedNotification
    )

let private handleSuccess (ass: DemandBehaviourAssignment) =
    task {
        publishChanged ass
        return Ok ass.AssignmentId
    }

// Traceability: Implements CA-D-005 (Classify Demand) Capabilities API for SE-D-037 (Demand Behaviour Assignment)
// Exposes the workflow layer: validates raw requests, calls CommandHandler, publishes BN-D-019 notifications.

let createCapabilities
    (execute: DemandBehaviourAssignmentCommand -> Task<ExecutionOutcome<DemandBehaviourAssignment, ApplicationError>>)
    : DemandBehaviourAssignmentApi =

    /// FS-D-017 — Update Demand Behaviour Classification (evaluates pattern or lifecycle behavior rules)
    let updateBehaviour =
        Helpers.runWorkflow toUpdateCmd (UpdateBehaviourClassification >> execute) handleSuccess

    /// FS-D-017 — Override Demand Behaviour Classification (allows manual override of system-calculated pattern/lifecycle classification)
    let overrideBehaviour =
        Helpers.runWorkflow toOverrideCmd (OverrideBehaviourClassification >> execute) handleSuccess

    { UpdateBehaviour = updateBehaviour
      OverrideBehaviour = overrideBehaviour }
