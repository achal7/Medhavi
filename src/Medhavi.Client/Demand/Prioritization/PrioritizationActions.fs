module Medhavi.Client.Demand.Prioritization.PrioritizationActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveAssignment (ctx: ActionExecutionContext) : Task<PlanningPriorityDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.PrioritizeDemand.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let prioritizeEntity: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.PrioritizeDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let entityType, entityId =
                match existingOpt with
                | Some dto -> dto.EntityType, dto.EntityId
                | None ->
                    let eType =
                        match ctx.Payload with
                        | CustomParameters p ->
                            match Map.tryFind "EntityType" p with
                            | Some (StringValue t) -> t
                            | _ -> "Item"
                        | _ -> "Item"

                    let eId = ctx.TargetKey |> Option.defaultValue "SKU-DRIVE-500GB"
                    eType, eId

            let rev =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "RevenueContribution" p with
                    | Some (DecimalValue d) -> Some d
                    | _ -> Some 0.85m
                | _ -> Some 0.85m

            let strat =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "StrategicImportance" p with
                    | Some (DecimalValue d) -> Some d
                    | _ -> Some 0.90m
                | _ -> Some 0.90m

            let risk =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "RiskExposure" p with
                    | Some (DecimalValue d) -> Some d
                    | _ -> Some 0.20m
                | _ -> Some 0.20m

            let contract =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "ContractualObligation" p with
                    | Some (DecimalValue d) -> Some d
                    | _ -> Some 0.95m
                | _ -> Some 0.95m

            let req: PrioritizePlanningEntityReq =
                { EntityType = entityType
                  EntityId = entityId
                  RevenueContribution = rev
                  StrategicImportance = strat
                  RiskExposure = risk
                  ContractualObligation = contract }

            match! api.PrioritizeEntity req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Planning entity '{dto.EntityId}' prioritized as '{dto.CurrentPriority}' (Composite Score: {dto.PriorityScore:F2}).",
                            [ PrioritizationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let overridePriority: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.PrioritizeDemand.Commands
            let! existingOpt = resolveAssignment ctx

            let entityType, entityId =
                match existingOpt with
                | Some dto -> dto.EntityType, dto.EntityId
                | None ->
                    let eId = ctx.TargetKey |> Option.defaultValue "SKU-DRIVE-500GB"
                    "Item", eId

            let newPriority =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "NewPriority" p with
                    | Some (StringValue pri) -> pri
                    | _ -> "Tier-1-Critical"
                | ReasonedAction(code, _) when not (String.IsNullOrWhiteSpace code) -> code
                | _ -> "Tier-1-Critical"

            let justification =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "Justification" p with
                    | Some (StringValue j) -> j
                    | _ -> "Executive escalation SLA override"
                | ReasonedAction(_, Some comment) -> comment
                | _ -> "Executive escalation SLA override"

            let req: OverridePlanningPriorityReq =
                { EntityType = entityType
                  EntityId = entityId
                  NewPriority = newPriority
                  Justification = justification
                  PlannerId = "Planner-Live" }

            match! api.OverridePriority req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Priority for '{dto.EntityId}' overridden to '{dto.CurrentPriority}'.",
                            [ PrioritizationGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ PrioritizationGrid.PrioritizationActions.prioritizeEntity.Id, prioritizeEntity
      PrioritizationGrid.PrioritizationActions.overridePriority.Id, overridePriority ]
