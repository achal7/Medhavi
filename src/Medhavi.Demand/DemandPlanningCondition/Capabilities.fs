module Medhavi.Demand.DemandPlanningCondition.Capabilities

open System.Threading.Tasks
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Execution
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.DemandPlanningCondition.Model
open Medhavi.Demand.DemandPlanningCondition.ACL

let private publishRecognized (condition: DemandPlanningCondition) =
    let notif: DemandPlanningConditionDetectedNotification =
        { ConditionId = DemandPlanningConditionId.value condition.Id
          PlanningEntity = condition.PlanningEntity
          ConditionType = condition.ConditionType
          Severity = condition.Severity.ToString()
          DetectionEvidence = condition.DetectionEvidence
          DetectionTimestamp = Timestamp.value condition.DetectionTimestamp }

    DomainEventBus.Publish notif

let private publishResolved (condition: DemandPlanningCondition) =
    let notif: DemandPlanningConditionResolvedNotification =
        { ConditionId = DemandPlanningConditionId.value condition.Id
          PlanningEntity = condition.PlanningEntity
          ConditionType = condition.ConditionType
          ResolutionEvidence = condition.ResolutionEvidence |> Option.defaultValue ""
          ResolutionTimestamp =
            condition.ResolutionTimestamp
            |> Option.map Timestamp.value
            |> Option.defaultValue(Timestamp.value condition.TransactionTime) }

    DomainEventBus.Publish notif

let createCapabilities
    (execute: DemandPlanningConditionCommand -> Task<ExecutionOutcome<DemandPlanningCondition, ApplicationError>>)
    : DemandPlanningConditionApi =

    let handleRecognizeSuccess (condition: DemandPlanningCondition) =
        task {
            publishRecognized condition
            return Ok(DemandPlanningConditionId.value condition.Id)
        }

    let recognizeCondition = Helpers.runWorkflow toRecognizeCmd (Recognize >> execute) handleRecognizeSuccess

    let handleResolveSuccess (condition: DemandPlanningCondition) =
        task {
            publishResolved condition
            return Ok()
        }

    let resolveCondition = Helpers.runWorkflow toResolveCmd (Resolve >> execute) handleResolveSuccess

    { Recognize = recognizeCondition
      Resolve = resolveCondition }
