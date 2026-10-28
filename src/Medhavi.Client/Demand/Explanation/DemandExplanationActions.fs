module Medhavi.Client.Demand.Explanation.DemandExplanationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveExplanation (ctx: ActionExecutionContext) : Task<DemandExplanationDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.ExplainDemand.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let establishExplanation: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ExplainDemand.Commands
            let! existingOpt = resolveExplanation ctx

            let artifactType, artifactId, version, templateOpt, whatIfOpt =
                match existingOpt with
                | Some dto ->
                    dto.ExplainedArtifactType,
                    dto.ExplainedArtifactId,
                    dto.Version,
                    Some dto.TemplateVersion,
                    dto.WhatIfAssumption
                | None ->
                    let aType =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "ExplainedArtifactType" p with
                            | Some(StringValue t) -> t
                            | _ -> "ForecastPublication"
                        | _ -> "ForecastPublication"

                    let aId = ctx.TargetKey |> Option.defaultValue "PUB-DEMAND-2026-M11"

                    let whatIf =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "WhatIfAssumption" p with
                            | Some(StringValue w) -> Some w
                            | _ -> None
                        | _ -> None

                    aType, aId, 1, Some "v1.0", whatIf

            let req: EstablishDemandExplanationReq =
                {
                    ExplainedArtifactType = artifactType
                    ExplainedArtifactId = artifactId
                    Version = version
                    TemplateVersion = templateOpt
                    WhatIfAssumption = whatIfOpt
                    EvidenceRefs = []
                }

            match! api.EstablishExplanation req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand explanation established for '{dto.ExplainedArtifactType}:{dto.ExplainedArtifactId}' (Explainability Score: {dto.ExplainabilityScore:P1}).",
                            [ DemandExplanationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [
        DemandExplanationGrid.DemandExplanationActions.establishExplanation.Id, establishExplanation
    ]
