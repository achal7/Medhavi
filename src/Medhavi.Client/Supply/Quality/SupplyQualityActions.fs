module Medhavi.Client.Supply.Quality.SupplyQualityActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let evaluateQuality: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Supply.EvaluateSupplyQuality.SupplyQualityApi
            let scopeId = "PS-GLOBAL-SUPPLY-CHAIN"
            let horizon = "HORIZON-90D"
            let periodId = "P-2026-09"

            let req: EvaluateSupplyQualityReq =
                { PlanningScopeId = scopeId
                  PlanningHorizon = horizon
                  EvaluationPeriodId = periodId
                  ForceMajeureDeclared = false
                  StrategicInvestmentNotes = None }

            match! api.Evaluate req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Supply quality assessed: Classification = {dto.OverallQuality.OverallClassification}, Score = {dto.OverallQuality.OverallScore:F1} (Assessment: {dto.AssessmentId}).",
                            [ SupplyQualityGrid.storeId
                              DataStoreId.literal "demo-domain-health-store" ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ SupplyQualityGrid.SupplyQualityActions.evaluateQuality.Id, evaluateQuality ]
