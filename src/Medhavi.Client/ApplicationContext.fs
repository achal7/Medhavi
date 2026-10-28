namespace Medhavi.Client

open System
open System.Globalization
open Medhavi.Contracts
open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Data
open Medhavi.Client.ActionExecution

// ============================================================================
// Data Store Semantics
// ============================================================================

type DataStore =
    { Id: DataStoreId
      Fetch: ApplicationContext -> Async<DataPayload> }

and DataStoreRegistry = Map<DataStoreId, DataStore>

and ApplicationContext =
    { PlanningContextService: Services.PlanningContextService
      ComponentRegistry: ComponentRegistry
      SchemaRegistry: SchemaRegistry
      DataStoreRegistry: DataStoreRegistry
      ActionRegistry: ActionRegistry
      Services: IServiceProvider
      DomainServices: DomainServices option }

    member this.Domain: DomainServices =
        match this.DomainServices with
        | Some ds -> ds
        | None -> invalidOp "DomainServices is not configured on ApplicationContext."

    member this.Resolve<'T when 'T : not struct>() : 'T option =
        match box this.Services with
        | null -> None
        | _ ->
            match this.Services.GetService(typeof<'T>) with
            | null -> None
            | s -> Some(s :?> 'T)

module DataStore =
    /// Declaratively creates a DataStore that queries a QueryService and projects items via a ProjectionAdapter.
    let fromQuery
        (id: DataStoreId)
        (getQuery: DomainServices -> QueryService<'TDto, 'TKey>)
        (adapter: ProjectionAdapter<'TDto>)
        : DataStore =
        { Id = id
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    let queries = getQuery ctx.Domain
                    let! dtos = queries.GetAll() |> Async.AwaitTask
                    let rows = dtos |> List.map adapter.ToUniversal
                    return Tabular rows
                } }

    /// Declaratively creates a DataStore from an arbitrary async fetch function returning UniversalRows.
    let create (id: DataStoreId) (fetch: ApplicationContext -> Async<UniversalRow list>) : DataStore =
        { Id = id
          Fetch =
            fun (ctx: ApplicationContext) ->
                async {
                    let! rows = fetch ctx
                    return Tabular rows
                } }

// ============================================================================
// Strongly-Typed Feature Manifest
// ============================================================================

type FeatureManifest =
    { Stores: (DataStoreId * DataStore) list
      Actions: (ActionDefId * ActionHandler) list
      Components: ComponentRegistration list }

    member this.Schemas: SchemaDefinition list =
        this.Components |> List.map (fun c -> c.Schema)
