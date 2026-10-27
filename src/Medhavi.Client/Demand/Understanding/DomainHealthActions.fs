module Medhavi.Client.Demand.Understanding.DomainHealthActions

open System
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Authoritative Application Command Handlers for Demand Understanding & Domain Health.
let reviseUnderstanding: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandUnderstandingApi>() with
                | Some api -> Some api
                | None ->
                    ctx.Resolve<DomainServices>()
                    |> Option.map (fun ds -> ds.Demand.UnderstandDemand.Understanding.Commands)

            match apiOpt with
            | Some api ->
                let scopeId = ctx.TargetKey |> Option.defaultValue "DEFAULT-SCOPE"
                let req: ReviseDemandUnderstandingReq =
                    { PlanningScopeId = scopeId
                      EvidencePictureVersion = 1L }

                match! api.Revise req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Demand understanding revised for scope '{dto.PlanningScopeId}' (v{dto.VersionNumber}).",
                                [ DomainHealthStore.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "DemandUnderstandingApi capability is not connected."
        }

let publishUnderstanding: ActionHandler =
    fun ctx ->
        task {
            let apiOpt =
                match ctx.Resolve<DemandUnderstandingApi>() with
                | Some api -> Some api
                | None ->
                    ctx.Resolve<DomainServices>()
                    |> Option.map (fun ds -> ds.Demand.UnderstandDemand.Understanding.Commands)

            match apiOpt with
            | Some api ->
                match ctx.TargetKey with
                | Some scopeId ->
                    let req: PublishDemandUnderstandingReq =
                        { PlanningScopeId = scopeId
                          IsPeriodicRefresh = false }

                    match! api.Publish req with
                    | Ok dto ->
                        return
                            Ok(
                                ActionExecutionOutcome.Completed(
                                    $"Demand understanding published for scope '{dto.PlanningScopeId}' (v{dto.VersionNumber}).",
                                    [ DomainHealthStore.storeId ]
                                )
                            )
                    | Error err -> return Error err.Message
                | None -> return Error "No planning scope selected to publish understanding."
            | None -> return Error "DemandUnderstandingApi capability is not connected."
        }

let explainUnderstanding: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some scopeId ->
                return Ok(ActionExecutionOutcome.ExplainRequested(scopeId, "DemandUnderstanding"))
            | None ->
                return Error "No planning scope selected for understanding explanation."
        }

let all: (ActionDefId * ActionHandler) list =
    [ DomainHealthSchema.DomainHealthActions.revise.Id, reviseUnderstanding
      DomainHealthSchema.DomainHealthActions.publish.Id, publishUnderstanding
      DomainHealthSchema.DomainHealthActions.explain.Id, explainUnderstanding ]
