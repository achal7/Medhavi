module Medhavi.Client.Components.Core.Demand.DemandOrdersGrid

open System
open Medhavi.Contracts.Core.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Geometry
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.Rendering
open Medhavi.Client.Registries
open Medhavi.Client.Controls
open Medhavi.Client

let definitionId = ComponentDefId.literal "demand-grid"
let storeId = DataStoreId.literal "demo-demand-store"

let definition: ComponentDefinition =
    {
        Id = definitionId
        DisplayName = "Demand Orders Workbench"
        Icon = "view_list"
        Category = ComponentCategory.DataDisplay
        RequiredSchemaCapabilities = [ "Tabular" ]
        DefaultGridSpan = GridSpan.literal 12 6
        DefaultSchemaId = Some DemandOrdersSchema.schemaId
        DefaultStoreId = Some storeId
        IsResizable = true
        IsDraggable = true
        SelectionMode = SingleSelection
    }

let store: DataStore =
    {
        Id = storeId
        Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    match ctx.Resolve<DemandQueries>() with
                    | Some queries ->
                        let! dtos = queries.GetAll() |> Async.AwaitTask
                        let rows = dtos |> List.map DemandOrdersSchema.adapter.ToUniversal
                        return Tabular rows
                    | None ->
                        match ctx.DomainServices with
                        | Some ds ->
                            let! dtos = ds.Core.DemandManagement.Queries.GetAll() |> Async.AwaitTask
                            let rows = dtos |> List.map DemandOrdersSchema.adapter.ToUniversal
                            return Tabular rows
                        | None ->
                            //let rows = fallbackDtos |> List.map DemandOrdersSchema.adapter.ToUniversal
                            return Tabular [] //rows
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
    open Medhavi.Client.ActionExecution

    let satisfyDemand: ActionHandler =
        fun ctx ->
            task {
                match ctx.Resolve<DemandApi>() with
                | Some api ->
                    match ctx.TargetKey with
                    | Some demandId ->
                        let req: SatisfyDemandReq =
                            {
                                DemandId = demandId
                                SatisfactionTime = DateTimeOffset.Now
                            }

                        match! api.Satisfy(req) with
                        | Ok dto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Demand '{dto.Id}' fulfilled and marked satisfied.",
                                        [ storeId; DataStoreId.literal "core-exception-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error "No demand order selected to satisfy."
                | None -> return Error "DemandApi capability is not connected."
            }

    let cancelDemand: ActionHandler =
        fun ctx ->
            task {
                match ctx.Resolve<DemandApi>() with
                | Some api ->
                    match ctx.TargetKey with
                    | Some demandId ->
                        let req: CancelDemandReq =
                            {
                                DemandId = demandId
                                CancellationTime = DateTimeOffset.Now
                                Reason = "Cancelled by planner"
                            }

                        match! api.Cancel(req) with
                        | Ok dto ->
                            return
                                Ok(
                                    ActionExecutionOutcome.Completed(
                                        $"Demand '{dto.Id}' cancelled in enterprise ledger.",
                                        [ storeId; DataStoreId.literal "core-exception-store" ]
                                    )
                                )
                        | Error err -> return Error err.Message
                    | None -> return Error "No demand order selected to cancel."
                | None -> return Error "DemandApi capability is not connected."
            }

    let explainShortage: ActionHandler =
        fun ctx ->
            task {
                match ctx.TargetKey with
                | Some demandId -> return Ok(ActionExecutionOutcome.ExplainRequested(demandId, "Demand"))
                | None -> return Error "No demand order selected for shortage explanation."
            }

    let all: (ActionDefId * ActionHandler) list =
        [
            DemandOrdersActions.satisfy.Id, satisfyDemand
            DemandOrdersActions.cancel.Id, cancelDemand
            DemandOrdersActions.explainShortage.Id, explainShortage
        ]
