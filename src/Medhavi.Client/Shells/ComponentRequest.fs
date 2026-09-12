namespace Medhavi.Client.Shells

open Medhavi.Client.Registries
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ViewComposition.Context
open Medhavi.Client.Shells.ViewSpace
open Medhavi.Common.Validation

type ComponentRequestError =
    | ComponentNotRegistered of ComponentDefId
    | ComponentHasNoDefaultSchema of ComponentDefId
    | SchemaNotRegistered of SchemaDefId
    | ComponentHasNoDefaultStore of ComponentDefId
    | BindingRejected of ComponentDefId * SchemaDefId * CompatibilityError list

module ComponentRequest =
    let prepareAddComponent
        (viewComponentId: ViewComponentId)
        (componentDefId: ComponentDefId)
        (componentRegistry: ComponentRegistry)
        (schemaRegistry: SchemaRegistry)
        : Result<ViewSpace.Request, ComponentRequestError> =
        match ComponentRegistry.find componentDefId componentRegistry with
        | Error _ -> Error(ComponentNotRegistered componentDefId)
        | Ok registration ->
            match registration.Definition.DefaultSchemaId with
            | None -> Error(ComponentHasNoDefaultSchema componentDefId)
            | Some schemaDefId ->
                match SchemaRegistry.find schemaDefId schemaRegistry with
                | Error _ -> Error(SchemaNotRegistered schemaDefId)
                | Ok schema ->
                    match registration.Definition.DefaultStoreId with
                    | None -> Error(ComponentHasNoDefaultStore componentDefId)
                    | Some storeId ->
                        let configuration =
                            { LocalFilters = schema.DefaultFilters
                              Parameters = Map.empty }

                        match ComponentBinding.create registration.Definition schema storeId configuration with
                        | Invalid errors -> Error(BindingRejected(componentDefId, schemaDefId, errors))
                        | Valid binding -> Ok(ViewSpace.AddComponent(viewComponentId, binding))
