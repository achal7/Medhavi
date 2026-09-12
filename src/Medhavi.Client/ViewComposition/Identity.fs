namespace Medhavi.Client.ViewComposition.Identity

open System

[<Struct>]
type ComponentDefId = private ComponentDefId of string

module ComponentDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "Component definition id must not be empty"
        else Ok(ComponentDefId value)

    let value (ComponentDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type SchemaDefId = private SchemaDefId of string

module SchemaDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "Schema definition id must not be empty"
        else Ok(SchemaDefId value)

    let value (SchemaDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type DataStoreId = private DataStoreId of string

module DataStoreId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "Data store id must not be empty"
        else Ok(DataStoreId value)

    let value (DataStoreId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewDefId = private ViewDefId of string

module ViewDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "View definition id must not be empty"
        else Ok(ViewDefId value)

    let value (ViewDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewComponentId = private ViewComponentId of string

module ViewComponentId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "View component id must not be empty"
        else Ok(ViewComponentId value)

    let value (ViewComponentId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type TabId = private TabId of string

module TabId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "Tab id must not be empty"
        else Ok(TabId value)

    let value (TabId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ViewSessionId = private ViewSessionId of string

module ViewSessionId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "View session id must not be empty"
        else Ok(ViewSessionId value)

    let value (ViewSessionId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)

[<Struct>]
type ActionDefId = private ActionDefId of string

module ActionDefId =
    let create value =
        if String.IsNullOrWhiteSpace value then Error "Action definition id must not be empty"
        else Ok(ActionDefId value)

    let value (ActionDefId value) = value

    let literal value =
        match create value with
        | Ok id -> id
        | Error error -> raise(ArgumentException error)
