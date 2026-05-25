module Medhavi.Common.Serialization

open System.Text.Json
open System.Text.Json.Serialization

type SerializationError =
    | SerializationFailed of string
    | DeserializationFailed of string
    | NullResult

let private tryExecute
    (toError: string -> SerializationError)
    (label: string)
    (f: unit -> 'T)
    : Result<'T, SerializationError> =
    try
        Ok(f ())
    with ex ->
        Error(toError (sprintf "%s error: %s" label ex.Message))

let private ensureNotNull (value: 'T) : Result<'T, SerializationError> =
    // use box to handle value types safely; obj.ReferenceEquals for null-check
    if obj.ReferenceEquals(box value, null) then
        Error NullResult
    else
        Ok value

// ---------------------------
// Options construction
// ---------------------------
let private buildOptions () : JsonSerializerOptions =
    JsonFSharpOptions
        .Default()
        .WithUnionEncoding(JsonUnionEncoding.AdjacentTag)
        .WithUnionTagName("Case")
        .WithUnionFieldsName("Fields")
        .WithUnionTagCaseInsensitive(true)
        .ToJsonSerializerOptions()
    |> fun opts ->
        opts.PropertyNamingPolicy <- JsonNamingPolicy.CamelCase
        opts.WriteIndented <- true
        opts

// lazy to ensure it's built once and immutable after creation
let private options = lazy (buildOptions ())

/// Public read-only options (useful for registering converters into DI)
let jsonOptions: JsonSerializerOptions = options.Value

// Core API
let serialize<'T> (value: 'T) : Result<string, SerializationError> =
    tryExecute (fun msg -> SerializationFailed msg) "Serialization" (fun () ->
        JsonSerializer.Serialize<'T>(value, jsonOptions))

let deserialize<'T> (json: string) : Result<'T, SerializationError> =
    tryExecute (fun msg -> DeserializationFailed msg) "Deserialization" (fun () ->
        JsonSerializer.Deserialize<'T>(json, jsonOptions))
    |> Result.bind ensureNotNull

let deserializeOrDefault<'T> (defaultValue: 'T) (json: string) : 'T =
    match deserialize<'T> json with
    | Ok v -> v
    | Error _ -> defaultValue

// ---------------------------
// Map helpers (thin wrappers)
// ---------------------------
let serializeMap (map: Map<string, 'v>) : Result<string, SerializationError> = serialize map

let deserializeMap<'v> (json: string) : Result<Map<string, 'v>, SerializationError> = deserialize<Map<string, 'v>> json

type JsonString = private JsonString of string

module JsonString =
    open System

    let create str = JsonString str
    let value (JsonString str) = str

    let tryCreate str =
        if String.IsNullOrWhiteSpace str then
            Error(SerializationFailed "JSON string cannot be null or empty")
        else
            Ok(JsonString str)

    let serializeJson (value: 'T) : Result<JsonString, SerializationError> =
        try
            JsonSerializer.Serialize(value, jsonOptions)
            |> tryCreate
        with ex ->
            Error(SerializationFailed ex.Message)

    /// Pure deserialization function
    let deserializeJson (json: JsonString) : Result<'T, SerializationError> = value json |> deserialize

    /// Safe deserialization with fallback
    let deserializeOrElse (fallback: 'T) (json: JsonString) : 'T =
        deserializeJson json
        |> Result.defaultValue fallback

// ---------------------------
// Result utilities
// ---------------------------
[<AutoOpen>]
module ResultExtras =
    // Railway helper alias for bind
    let (>>=) result binder = Result.bind binder result

    // Try multiple deserialization/approaches: fallback if original error
    let tryWith (fallback: unit -> Result<'T, 'E>) (result: Result<'T, 'E>) : Result<'T, 'E> =
        match result with
        | Ok v -> Ok v
        | Error _ -> fallback ()

// ---------------------------
// Convenience extensions / module
// ---------------------------

type System.String with
    /// Try to deserialize into T, returning Result
    member this.TryDeserialize<'T>() : Result<'T, SerializationError> = deserialize<'T> this

    /// Deserialize or return default value
    member this.DeserializeOrDefault<'T>(defaultValue: 'T) : 'T = deserializeOrDefault defaultValue this

[<RequireQualifiedAccess>]
module Json =
    /// Serialize a value to JSON (Result)
    let toJson (value: 'T) : Result<string, SerializationError> = serialize value

    /// Quick debug string (best-effort; returns "null" on error)
    let toJsonString (value: 'T) : string =
        match serialize value with
        | Ok s -> s
        | Error _ -> "null"

// ---------------------------
// ASP.NET Core helper (usage example)
// ---------------------------
// Call this from Program.fs to ensure MVC/minimal API uses the F# converters.
//
// Example for Controllers:
// builder.Services.AddControllers()
//     .AddJsonOptions(fun opts ->
//         for c in jsonOptions.Converters do
//             opts.JsonSerializerOptions.Converters.Add(c)
//     )
//
// Example for Minimal APIs (ConfigureHttpJsonOptions):
// builder.Services.ConfigureHttpJsonOptions(fun httpOptions ->
//     for c in jsonOptions.Converters do
//         httpOptions.SerializerOptions.Converters.Add(c)
// )
let addFSharpConvertersTo (targetOptions: JsonSerializerOptions) =
    for c in JsonFSharpOptions.Default().ToJsonSerializerOptions().Converters do
        targetOptions.Converters.Add(c)

(*

/// <summary>
/// Higher-order functions and composition
/// </summary>
module Transform =
    open Domain

    /// Map over serialized JSON (lens-like operation)
    let map (f: 'T -> 'U) (json: JsonString) : Result<JsonString, SerializationError> =
        json
        |> Core.deserialize
        |> Result.map f
        |> Result.bind Core.serialize

    /// Bimap for error handling
    let bimap (success: 'T -> 'U) (error: SerializationError -> SerializationError) 
        (result: Result<'T, SerializationError>) : Result<'U, SerializationError> =
        Result.map success result
        |> Result.mapError error

    /// Compose serialization operations
    let compose (f: 'T -> Result<'U, SerializationError>) (g: 'U -> Result<'V, SerializationError>) : 'T -> Result<'V, SerializationError> =
        f >> Result.bind g

    /// Pipeline helper for serialization flows
    let (|>>) (value: 'T) (serializer: 'T -> Result<JsonString, SerializationError>) = 
        serializer value

    /// Try multiple deserialization strategies
    let tryAll (strategies: (JsonString -> Result<'T, SerializationError>) list) (json: JsonString) : Result<'T, SerializationError> =
        strategies
        |> List.tryPick (fun strategy -> 
            match strategy json with
            | Ok result -> Some (Ok result)
            | Error _ -> None)
        |> Option.defaultWith (fun () -> Error (DeserializationException "All deserialization strategies failed"))

/// <summary>
/// Domain-specific serialization (if needed)
/// </summary>
module Specialized =
    open Domain

    /// Validated serialization for specific domains
    let serializeWithValidator (validator: 'T -> Result<unit, string>) (value: 'T) : Result<JsonString, SerializationError> =
        validator value
        |> Result.mapError SerializationException
        |> Result.bind (fun () -> Core.serialize value)

    /// Partial application for common validation patterns
    let createValidatedSerializer validator =
        serializeWithValidator validator

/// <summary>
/// Public API - functional interface
/// </summary>
[<AutoOpen>]
module PublicApi =
    open Domain

    /// Alias for cleaner usage
    let toJson = Core.serialize
    let fromJson = Core.deserialize
    let fromJsonOrElse = Core.deserializeOrElse

    /// Functional extensions for strings
    type String with
        member this.TryParseJson<'T>() : Result<'T, SerializationError> =
            JsonString.tryCreate this
            |> Result.bind Core.deserialize

        member this.ParseJsonOrElse<'T>(fallback: 'T) : 'T =
            JsonString.tryCreate this
            |> Result.bind Core.deserialize
            |> Result.defaultValue fallback

    /// Functional extensions for values
    type 'T with
        member this.ToJson() : Result<JsonString, SerializationError> = 
            Core.serialize this

        member this.ToJsonString() : string =  // For interop with non-FP code
            Core.serialize this
            |> Result.map JsonString.value
            |> Result.defaultWith (fun _ -> "null")

    /// Railway-oriented programming helpers
    let (|Serialize|_|) value = 
        match Core.serialize value with
        | Ok json -> Some json
        | Error _ -> None

    let (|Deserialize|_|) (json: JsonString) = 
        match Core.deserialize json with
        | Ok value -> Some value
        | Error _ -> None

/// <summary>
/// Usage examples and documentation
/// </summary>
module Examples =
    open Domain

    // Domain types
    type TestId = private TestId of string
    module TestId =
        let create = TestId
        let value (TestId id) = id

    type TestUnion =
        | Case1 of string
        | Case2 of int * string
        | Case3

    type ComplexType =
        { Id: TestId
          Name: string
          Status: TestUnion
          Items: TestUnion list }

    // Railway-oriented programming example
    let processComplexData (input: ComplexType) : Result<JsonString, SerializationError> =
        input
        |> toJson
        |> Transform.bimap 
            (fun json -> printfn "Serialized successfully"; json)
            (fun err -> printfn "Error: %A" err; err)

    // Composition example
    let validateAndSerialize (data: ComplexType) : Result<JsonString, SerializationError> =
        let validator (ct: ComplexType) =
            if String.IsNullOrWhiteSpace ct.Name then
                Error "Name cannot be empty"
            else
                Ok ()

        let serializer = 
            Specialized.createValidatedSerializer validator

        data |>> serializer

    // Pipeline example
    let fullProcessingPipeline (data: ComplexType) =
        data
        |> validateAndSerialize
        |> Result.bind (Transform.map (fun (json: JsonString) -> 
            printfn "Processing complete"; json))

    // Usage in real code
    let exampleUsage() =
        let testData = 
            { Id = TestId.create "test-123"
              Name = "Functional Test"
              Status = Case2(42, "hello")
              Items = [ Case1 "item1"; Case3 ] }

        // Railway-oriented approach
        match testData.ToJson() with
        | Ok json ->
            match json.TryParseJson<ComplexType>() with
            | Ok deserialized -> 
                printfn "Round-trip successful: %A" deserialized
                Ok deserialized
            | Error err -> 
                printfn "Deserialization failed: %A" err
                Error err
        | Error err ->
            printfn "Serialization failed: %A" err
            Error err

        // Or using computation expressions (if you define a ResultBuilder)
        // result {
        //     let! json = testData.ToJson()
        //     let! roundTrip = json.TryParseJson<ComplexType>()
        //     return roundTrip
        // }
*)
