namespace Medhavi.Client.Registries

open Medhavi.Client.SemanticModel
open Medhavi.Client.Rendering

type ComponentRegistration =
    { Definition: ComponentDefinition
      Runtime: ComponentRuntime }

type ComponentRegistryError =
    | ComponentNotRegistered of ComponentDefId
    | DuplicateRegistration of ComponentDefId

type ComponentRegistry = private ComponentRegistry of Map<ComponentDefId, ComponentRegistration>

module ComponentRegistry =
    let empty: ComponentRegistry = ComponentRegistry Map.empty
    let tryFind (id: ComponentDefId) (ComponentRegistry m) = Map.tryFind id m

    let find (id: ComponentDefId) (registry: ComponentRegistry) =
        match tryFind id registry with
        | Some r -> Ok r
        | None -> Error(ComponentNotRegistered id)

    let tryRegister (registration: ComponentRegistration) (ComponentRegistry m) =
        let id = registration.Definition.Id

        if Map.containsKey id m then
            Error(DuplicateRegistration id)
        else
            Ok(ComponentRegistry(Map.add id registration m))

    let ofList (registrations: ComponentRegistration list) =
        List.fold
            (fun acc r ->
                match acc with
                | Error e -> Error e
                | Ok reg -> tryRegister r reg)
            (Ok empty)
            registrations

    let contains (id: ComponentDefId) (ComponentRegistry m) = Map.containsKey id m
    let count (ComponentRegistry m) = Map.count m
    let all (ComponentRegistry m) = m |> Map.toList |> List.map snd
