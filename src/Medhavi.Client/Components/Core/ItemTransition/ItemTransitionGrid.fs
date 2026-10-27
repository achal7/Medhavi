module Medhavi.Client.Components.Core.ItemTransition.ItemTransitionGrid

open System
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

let definitionId = ComponentDefId.literal "item-transition-grid"
let storeId = DataStoreId.literal "core-item-transition-store"
let schemaId = ItemTransitionSchema.schemaId

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Item Transitions Workbench"
        Icon = "swap_calls"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let private fallbackDtos: ItemTransitionDto list =
    [
        {
            TransitionId = "TR-001"
            SupersededItem = "A100 Power Module"
            SupersedingItem = "H100 GPU Board"
            TransitionType = "Supersession"
            EffectiveDate = DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero)
            EndDate = None
            LifecycleState = "Active"
        }
        {
            TransitionId = "TR-002"
            SupersededItem = "B200 Inverter"
            SupersedingItem = "C300 Sensor Unit"
            TransitionType = "PhaseOut"
            EffectiveDate = DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero)
            EndDate = Some(DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero))
            LifecycleState = "Active"
        }
    ]

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<ItemTransitionQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map ItemTransitionSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Core.ItemTransitionManagement.Queries.GetAll() |> Async.AwaitTask
                            let rows = dtos |> List.map ItemTransitionSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            let rows = fallbackDtos |> List.map ItemTransitionSchema.adapter.ToUniversal
                            return Tabular rows
                }
    }

let runtime: ComponentRuntime =
    {
        Render = UniversalEnterpriseGrid.render
    }

let registration: ComponentRegistration =
    {
        Definition = definition
        Runtime = runtime
    }

module ActionHandlers =
    open System.Threading.Tasks
    open Medhavi.Client.ActionExecution

    let suspend : ActionHandler =
        fun ctx -> task {
            match ctx.Resolve<ItemTransitionApi>() with
            | Some api ->
                match ctx.TargetKey with
                | Some transitionId ->
                    let! dtoOpt =
                        match ctx.Resolve<ItemTransitionQueries>() with
                        | Some q -> q.GetById transitionId
                        | None -> Task.FromResult None

                    match dtoOpt with
                    | Some dto ->
                        let req : SuspendItemTransitionReq =
                            { SupersededItem = dto.SupersededItem
                              SupersedingItem = dto.SupersedingItem
                              SuspensionTime = DateTimeOffset.Now }
                        match! api.Suspend(req) with
                        | Ok resDto ->
                            return Ok (ActionExecutionOutcome.Completed(
                                $"Item transition '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}) suspended.",
                                [ storeId; DataStoreId.literal "demo-demand-store" ]
                            ))
                        | Error err ->
                            return Error err.Message
                    | None ->
                        return Error $"Item transition '{transitionId}' not found in query projection."
                | None ->
                    return Error "No item transition selected to suspend."
            | None ->
                return Error "ItemTransitionApi capability is not connected."
        }

    let reinstate : ActionHandler =
        fun ctx -> task {
            match ctx.Resolve<ItemTransitionApi>() with
            | Some api ->
                match ctx.TargetKey with
                | Some transitionId ->
                    let! dtoOpt =
                        match ctx.Resolve<ItemTransitionQueries>() with
                        | Some q -> q.GetById transitionId
                        | None -> Task.FromResult None

                    match dtoOpt with
                    | Some dto ->
                        let req : ReinstateItemTransitionReq =
                            { SupersededItem = dto.SupersededItem
                              SupersedingItem = dto.SupersedingItem
                              ReinstatementTime = DateTimeOffset.Now }
                        match! api.Reinstate(req) with
                        | Ok resDto ->
                            return Ok (ActionExecutionOutcome.Completed(
                                $"Item transition '{resDto.TransitionId}' ({resDto.SupersededItem} -> {resDto.SupersedingItem}) reinstated to active.",
                                [ storeId; DataStoreId.literal "demo-demand-store" ]
                            ))
                        | Error err ->
                            return Error err.Message
                    | None ->
                        return Error $"Item transition '{transitionId}' not found in query projection."
                | None ->
                    return Error "No item transition selected to reinstate."
            | None ->
                return Error "ItemTransitionApi capability is not connected."
        }

    let all : (ActionDefId * ActionHandler) list =
        [ ItemTransitionActions.suspend.Id, suspend
          ItemTransitionActions.reinstate.Id, reinstate ]
