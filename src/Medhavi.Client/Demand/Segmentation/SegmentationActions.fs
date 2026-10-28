module Medhavi.Client.Demand.Segmentation.SegmentationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveAssignment (ctx: ActionExecutionContext) : Task<PlanningClassificationDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.SegmentDemand.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let classifyEntity: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.SegmentDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let entityType, entityId, classificationType =
                match existingOpt with
                | Some dto -> dto.EntityType, dto.EntityId, dto.ClassificationType
                | None ->
                    let eType =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "EntityType" p with
                            | Some (StringValue t) -> t
                            | _ -> "Item"
                        | _ -> "Item"

                    let eId = ctx.TargetKey |> Option.defaultValue "SKU-DRIVE-500GB"

                    let cType =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "ClassificationType" p with
                            | Some (StringValue t) -> t
                            | _ -> "ABC"
                        | _ -> "ABC"

                    eType, eId, cType

            let revenuePct =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "VolumeOrRevenuePercentage" p with
                    | Some (DecimalValue d) -> Some d
                    | _ -> Some 0.82m
                | _ -> Some 0.82m

            let req: ClassifyPlanningEntityReq =
                { EntityType = entityType
                  EntityId = entityId
                  ClassificationType = classificationType
                  VolumeOrRevenuePercentage = revenuePct
                  HistoricalDemandValues = None
                  AnalogItemId = None }

            match! api.ClassifyEntity req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Entity '{dto.EntityId}' classified as '{dto.CurrentClassification}' ({dto.ClassificationType}) with {dto.ClassificationConfidence} confidence.",
                            [ SegmentationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let overrideClassification: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.SegmentDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let entityType, entityId, classificationType =
                match existingOpt with
                | Some dto -> dto.EntityType, dto.EntityId, dto.ClassificationType
                | None ->
                    let eId = ctx.TargetKey |> Option.defaultValue "SKU-DRIVE-500GB"
                    "Item", eId, "ABC"

            let newClass =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "NewClassification" p with
                    | Some (StringValue c) -> c
                    | _ -> "A"
                | ReasonedAction(code, _) when not (String.IsNullOrWhiteSpace code) -> code
                | _ -> "A"

            let justification =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "Justification" p with
                    | Some (StringValue j) -> j
                    | _ -> "Strategic volume reclassification override"
                | ReasonedAction(_, Some comment) -> comment
                | _ -> "Strategic volume reclassification override"

            let req: OverridePlanningClassificationReq =
                { EntityType = entityType
                  EntityId = entityId
                  ClassificationType = classificationType
                  NewClassification = newClass
                  Justification = justification
                  PlannerId = "Planner-Live" }

            match! api.OverrideClassification req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Planning classification for '{dto.EntityId}' overridden to '{dto.CurrentClassification}'.",
                            [ SegmentationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ SegmentationGrid.SegmentationActions.classifyEntity.Id, classifyEntity
      SegmentationGrid.SegmentationActions.overrideClassification.Id, overrideClassification ]
