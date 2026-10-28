module Medhavi.Client.Demand.Understanding.DomainHealthActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Core
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

/// Authoritative Application Command Handlers for Demand Understanding & Domain Health.
let reviseUnderstanding: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.UnderstandDemand.Understanding.Commands

            let scopeIdOpt =
                ctx.TargetKey
                |> Option.map(fun k -> if k.Contains("#") then k.Split('#').[0] else k)
                |> Option.filter(not << String.IsNullOrWhiteSpace)
                |> Option.orElse ctx.PlanningContext.ScopeId

            match scopeIdOpt with
            | None ->
                return
                    Error
                        "No planning scope selected or configured in PlanningContext to revise demand understanding."
            | Some scopeId ->
                let! pictureVersion =
                    task {
                        let keyVersionOpt =
                            match ctx.TargetKey with
                            | Some key when key.Contains("#") ->
                                match Int64.TryParse(key.Split('#').[1]) with
                                | true, v -> Some v
                                | false, _ -> None
                            | _ -> None

                        match keyVersionOpt with
                        | Some v -> return v
                        | None ->
                            let! picOpt = ctx.Domain.Core.EnterprisePicture.Queries.GetById scopeId

                            match picOpt with
                            | Some pic ->
                                match pic.CurrentPublishedVersion with
                                | Some v -> return int64 v
                                | None ->
                                    match pic.Versions |> List.tryLast with
                                    | Some v -> return int64 v.VersionNumber
                                    | None -> return 1L
                            | None -> return 1L
                    }

                let req: ReviseDemandUnderstandingReq =
                    {
                        PlanningScopeId = scopeId
                        EvidencePictureVersion = pictureVersion
                    }

                match! api.Revise req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Demand understanding revised for scope '{dto.PlanningScopeId}' against picture baseline v{pictureVersion} (Understanding v{dto.VersionNumber}).",
                                [ DomainHealthRadar.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
        }

let publishUnderstanding: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.UnderstandDemand.Understanding.Commands

            let scopeIdOpt =
                ctx.TargetKey
                |> Option.map(fun k -> if k.Contains("#") then k.Split('#').[0] else k)
                |> Option.filter(not << String.IsNullOrWhiteSpace)
                |> Option.orElse ctx.PlanningContext.ScopeId

            match scopeIdOpt with
            | Some scopeId ->
                let req: PublishDemandUnderstandingReq =
                    {
                        PlanningScopeId = scopeId
                        IsPeriodicRefresh = false
                    }

                match! api.Publish req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Demand understanding published for scope '{dto.PlanningScopeId}' (v{dto.VersionNumber}).",
                                [ DomainHealthRadar.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No planning scope selected or configured to publish understanding."
        }

let explainUnderstanding: ActionHandler =
    fun ctx ->
        task {
            let scopeIdOpt =
                ctx.TargetKey
                |> Option.map(fun k -> if k.Contains("#") then k.Split('#').[0] else k)
                |> Option.filter(not << String.IsNullOrWhiteSpace)
                |> Option.orElse ctx.PlanningContext.ScopeId

            match scopeIdOpt with
            | Some scopeId -> return Ok(ActionExecutionOutcome.ExplainRequested(scopeId, "DemandUnderstanding"))
            | None -> return Error "No planning scope selected for understanding explanation."
        }

let all: (ActionDefId * ActionHandler) list =
    [
        DomainHealthRadar.DomainHealthActions.revise.Id, reviseUnderstanding
        DomainHealthRadar.DomainHealthActions.publish.Id, publishUnderstanding
        DomainHealthRadar.DomainHealthActions.explain.Id, explainUnderstanding
    ]
