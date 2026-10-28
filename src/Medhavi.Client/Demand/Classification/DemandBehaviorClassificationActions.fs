module Medhavi.Client.Demand.Classification.DemandBehaviorClassificationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveAssignment (ctx: ActionExecutionContext) : Task<DemandBehaviorAssignmentDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.ClassifyDemand.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let classifyBehavior: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ClassifyDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let itemId, locId, dimension =
                match existingOpt with
                | Some dto -> dto.ItemId, dto.LocationId, dto.Dimension
                | None ->
                    let item, loc =
                        match ctx.TargetKey with
                        | Some key when key.Contains("@") ->
                            let parts = key.Split('@')
                            parts.[0], parts.[1]
                        | Some key when key.Contains("#") ->
                            let parts = key.Split('#')
                            parts.[0], parts.[1]
                        | Some key -> key, "LOC-CENTRAL-WH"
                        | None -> "SKU-DRIVE-500GB", "LOC-CENTRAL-WH"

                    let dim =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "Dimension" p with
                            | Some(StringValue d) -> d
                            | _ -> "Volume"
                        | _ -> "Volume"

                    item, loc, dim

            let req: ClassifyDemandBehaviorReq =
                {
                    ItemId = itemId
                    LocationId = locId
                    Dimension = dimension
                    DemandQuantities = [ 120.0m; 135.0m; 110.0m; 140.0m; 125.0m; 130.0m ]
                }

            match! api.ClassifyBehavior req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand behavior classified for {dto.ItemId} @ {dto.LocationId}: Pattern is '{dto.CurrentClassification}' ({dto.ClassificationConfidence} confidence).",
                            [ DemandBehaviorClassificationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let overrideBehavior: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ClassifyDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let itemId, locId, dimension =
                match existingOpt with
                | Some dto -> dto.ItemId, dto.LocationId, dto.Dimension
                | None ->
                    let item, loc =
                        match ctx.TargetKey with
                        | Some key when key.Contains("@") ->
                            let parts = key.Split('@')
                            parts.[0], parts.[1]
                        | Some key when key.Contains("#") ->
                            let parts = key.Split('#')
                            parts.[0], parts.[1]
                        | Some key -> key, "LOC-CENTRAL-WH"
                        | None -> "SKU-DRIVE-500GB", "LOC-CENTRAL-WH"

                    item, loc, "Volume"

            let newClass =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "NewClassification" p with
                    | Some(StringValue c) -> c
                    | _ -> "Smooth"
                | ReasonedAction(code, _) when not(String.IsNullOrWhiteSpace code) -> code
                | _ -> "Smooth"

            let justification =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "Justification" p with
                    | Some(StringValue j) -> j
                    | _ -> "Contractual baseload smoothing override"
                | ReasonedAction(_, Some comment) -> comment
                | _ -> "Contractual baseload smoothing override"

            let req: OverrideDemandBehaviorReq =
                {
                    ItemId = itemId
                    LocationId = locId
                    Dimension = dimension
                    NewClassification = newClass
                    Justification = justification
                    PlannerId = "Planner-Live"
                }

            match! api.OverrideBehavior req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand behavior pattern for {dto.ItemId} @ {dto.LocationId} overridden to '{dto.CurrentClassification}'.",
                            [ DemandBehaviorClassificationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [
        DemandBehaviorClassificationGrid.DemandClassificationActions.classifyBehavior.Id, classifyBehavior
        DemandBehaviorClassificationGrid.DemandClassificationActions.overrideBehavior.Id, overrideBehavior
    ]
