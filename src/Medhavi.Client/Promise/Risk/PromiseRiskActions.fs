module Medhavi.Client.Promise.Risk.PromiseRiskActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let assessRisk: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.AssessPromiseRisk.Risk
            let id = $"RISK-AUTO-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 10000L}"
            let req: AssessCommitmentRiskReq =
                { AssessmentId = id
                  CommitmentId = "CMT-TITAN-7001"
                  OrderLineId = "LINE-TITAN-7001-01"
                  Item = "SKU-DRIVE-1TB"
                  AssessmentTime = DateTimeOffset.UtcNow }

            match! api.Assess req with
            | Ok dto ->
                let pct = float (dto.BreachProbabilityPct * 100.0m)
                let msg = sprintf "Risk evaluated for '%s'. Breach probability: %.1f%%. Severity: %s." dto.CommitmentId pct dto.SeverityTier
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            msg,
                            [ PromiseRiskGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let escalate: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessId ->
                let api = ctx.Domain.Promise.AssessPromiseRisk.Risk
                let req: EscalateRiskBreachReq =
                    { AssessmentId = assessId
                      EscalationReason = "Longitudinal buffer degradation exceeds critical safety threshold"
                      EscalationTime = DateTimeOffset.UtcNow }

                match! api.Escalate req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Risk assessment '{dto.AssessmentId}' escalated to exception triage.",
                                [ PromiseRiskGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No risk assessment selected."
        }

let explainRisk: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some assessId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(assessId, "PromiseRisk"))
            | None -> return Error "No risk assessment selected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ PromiseRiskGrid.PromiseRiskActions.assessRisk.Id, assessRisk
      PromiseRiskGrid.PromiseRiskActions.escalate.Id, escalate
      PromiseRiskGrid.PromiseRiskActions.explainRisk.Id, explainRisk ]

