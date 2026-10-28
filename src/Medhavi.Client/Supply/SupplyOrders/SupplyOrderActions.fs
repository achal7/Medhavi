module Medhavi.Client.Supply.SupplyOrders.SupplyOrderActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let generatePlan: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.PlanSupply.SupplyPlanApi
            let req: GenerateSupplyPlanReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  ForecastHorizonId = "HORIZON-90D"
                  SupplyUnderstandingVersion = 1
                  ForecastPublicationVersion = 1
                  EnterprisePictureVersion = 1L }

            match! api.Generate req with
            | Ok planDto ->
                let verNum =
                    planDto.CurrentVersion
                    |> Option.map (fun v -> string v.VersionNumber)
                    |> Option.defaultValue "1"
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Supply plan generated successfully (Version {verNum}, Scope '{planDto.PlanningScopeId}').",
                            [ SupplyGrid.storeId
                              DataStoreId.literal "inventory-store"
                              DataStoreId.literal "capacity-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let evaluatePlan: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.PlanSupply.SupplyPlanApi
            let req: EvaluateSupplyPlanReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  ForecastHorizonId = "HORIZON-90D"
                  VersionNumber = 1 }

            match! api.Evaluate req with
            | Ok planDto ->
                let outcomeStr =
                    planDto.CurrentVersion
                    |> Option.bind (fun v -> v.EvaluationOutcome)
                    |> Option.map (fun o -> string o)
                    |> Option.defaultValue "Evaluated"
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Supply plan evaluated: Outcome = {outcomeStr}.",
                            [ SupplyGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let publishPlan: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.PlanSupply.SupplyPlanApi
            let req: PublishSupplyPlanReq =
                { PlanningScopeId = "PS-GLOBAL-SUPPLY-CHAIN"
                  ForecastHorizonId = "HORIZON-90D"
                  VersionNumber = 1
                  IsOutCycle = false }

            match! api.Publish req with
            | Ok planDto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Supply plan published to enterprise picture ledger for '{planDto.PlanningScopeId}'.",
                            [ SupplyGrid.storeId
                              DataStoreId.literal "inventory-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let explainDelay: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some orderId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(orderId, "SupplyOrderFeasibility"))
            | None -> return Error "No planned order selected to explain."
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyGrid.SupplyOrderActions.generatePlan.Id, generatePlan
      SupplyGrid.SupplyOrderActions.evaluatePlan.Id, evaluatePlan
      SupplyGrid.SupplyOrderActions.publishPlan.Id, publishPlan
      SupplyGrid.SupplyOrderActions.explainDelay.Id, explainDelay ]
