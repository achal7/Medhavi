module Medhavi.Client.Shells.SchemaRegistration

open Medhavi.Common.Validation
open Medhavi.Client.ViewComposition.Schema
open Medhavi.Client.Registries
open Medhavi.Client.Rendering

/// Validates a schema and registers it if valid.
/// Combines structural validation (core) with RenderHint resolution (composition root).
let registerSchema
    (schema: SchemaDefinition)
    (rendererRegistry: CellRendererRegistry)
    (schemaRegistry: SchemaRegistry)
    : Validation<SchemaRegistry, string> =
    // Structural validation
    let structuralValidation = SchemaValidation.validate schema |> mapError(fun e -> $"Structural error: {e}")

    // RenderHint resolution validation
    let hintValidation = CellRendererRegistry.validateSchemaHints schema rendererRegistry

    // Combine both validations applicatively
    match structuralValidation, hintValidation with
    | Valid _, Valid() ->
        match SchemaRegistry.tryRegister schema schemaRegistry with
        | Ok registry -> Valid registry
        | Error e -> Invalid [ $"Registration error: {e}" ]
    | Invalid errors, Valid() -> Invalid errors
    | Valid _, Invalid errors -> Invalid errors
    | Invalid errors1, Invalid errors2 -> Invalid(errors1 @ errors2)
