module Medhavi.Demand.PlanningClassificationAssignment.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts.Demand.PlanningClassificationAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningClassificationAssignment.ACL
open Medhavi.Demand.PlanningClassificationAssignment.Model

let private publishChanged (ass: PlanningClassificationAssignment) =
    // Publish BN-D-017 Planning Classification Changed
    DomainEventBus.Publish(
        { EntityType = ass.EntityType
          EntityId = ass.EntityId
          ClassificationType = ass.ClassificationType.AsString()
          PreviousClassification =
            ass.LastChangeEvent |> Option.bind(fun e -> e.PreviousClassification) |> Option.defaultValue ""
          NewClassification = ass.CurrentClassification
          Reason = ass.LastChangeEvent |> Option.map(fun e -> e.Reason) |> Option.defaultValue ""
          Confidence = PositiveDecimal.value ass.ClassificationConfidence }
        : PlanningClassificationChangedNotification
    )

let private handleSuccess (ass: PlanningClassificationAssignment) =
    task {
        publishChanged ass
        return Ok ass.AssignmentId
    }

// Traceability: Implements CA-D-004 (Segment Demand) Capabilities API for SE-D-036 (Planning Classification Assignment)
// Exposes the workflow layer: validates raw requests, calls CommandHandler, publishes BN-D-017 notifications.

let createCapabilities
    (execute: PlanningClassificationCommand -> Task<ExecutionOutcome<PlanningClassificationAssignment, ApplicationError>>)
    : PlanningClassificationApi =

    /// FS-D-016 — Update Planning Classification (evaluates ABC/XYZ or Strategic segmentation rules)
    let updateClassification =
        Helpers.runWorkflow toUpdateCmd (UpdatePlanningClassification >> execute) handleSuccess

    /// FS-D-016 — Override Planning Classification (allows manual override of system-calculated segmentation)
    let overrideClassification =
        Helpers.runWorkflow toOverrideCmd (OverridePlanningClassification >> execute) handleSuccess

    { UpdateClassification = updateClassification
      OverrideClassification = overrideClassification }
