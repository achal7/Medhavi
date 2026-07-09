module Medhavi.Demand.PlanningPriorityAssignment.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts.Demand.PlanningPriorityAssignment
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.PlanningPriorityAssignment.ACL
open Medhavi.Demand.PlanningPriorityAssignment.Model

let private publishChanged (ass: PlanningPriorityAssignment) =
    // Publish BN-D-020 Planning Priority Changed
    DomainEventBus.Publish(
        { EntityType = ass.EntityType
          EntityId = ass.EntityId
          PreviousPriority = ass.LastChangeEvent |> Option.bind(fun e -> e.PreviousPriority) |> Option.defaultValue ""
          NewPriority = ass.CurrentPriority.AsString()
          PriorityScore = PositiveDecimal.value ass.PriorityScore
          DecisionRationale = ass.DecisionRationale
          BusinessValidity = ass.BusinessValidity }
        : PlanningPriorityChangedNotification
    )

let private handleSuccess (ass: PlanningPriorityAssignment) =
    task {
        publishChanged ass
        return Ok ass.AssignmentId
    }

// Traceability: Implements CA-D-006 (Prioritize Demand) Capabilities API for SE-D-038 (Planning Priority Assignment)
// Exposes the workflow layer: validates raw requests, calls CommandHandler, publishes BN-D-020 notifications.

let createCapabilities
    (execute: PlanningPriorityCommand -> Task<ExecutionOutcome<PlanningPriorityAssignment, ApplicationError>>)
    : PlanningPriorityAssignmentApi =

    /// FS-D-018 — Update Planning Priority (evaluates priority scoring rules)
    let updatePriority =
        Helpers.runWorkflow toUpdateCmd (UpdatePlanningPriority >> execute) handleSuccess

    /// FS-D-018 — Override Planning Priority (allows manual override of system-calculated priority)
    let overridePriority =
        Helpers.runWorkflow toOverrideCmd (OverridePlanningPriority >> execute) handleSuccess

    { UpdatePriority = updatePriority
      OverridePriority = overridePriority }
