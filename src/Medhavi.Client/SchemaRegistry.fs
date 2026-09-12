namespace Medhavi.Client.Registries

open Medhavi.Client.SemanticModel

type SchemaRegistryError =
    | SchemaNotRegistered of SchemaDefId
    | DuplicateSchemaRegistration of SchemaDefId

type SchemaRegistry = private SchemaRegistry of Map<SchemaDefId, SchemaDefinition>

module SchemaRegistry =
    let empty = SchemaRegistry Map.empty
    let tryFind (id: SchemaDefId) (SchemaRegistry m) = Map.tryFind id m

    let find (id: SchemaDefId) (SchemaRegistry m) =
        match Map.tryFind id m with
        | Some s -> Ok s
        | None -> Error(SchemaNotRegistered id)

    let tryRegister (def: SchemaDefinition) (SchemaRegistry m) =
        if Map.containsKey def.Id m then
            Error(DuplicateSchemaRegistration def.Id)
        else
            Ok(SchemaRegistry(Map.add def.Id def m))

    let ofList (definitions: SchemaDefinition list) =
        List.fold
            (fun acc def ->
                match acc with
                | Error e -> Error e
                | Ok reg -> tryRegister def reg)
            (Ok empty)
            definitions
