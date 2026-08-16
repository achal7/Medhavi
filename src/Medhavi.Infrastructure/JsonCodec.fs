module Medhavi.Infrastructure.JsonCodec

open System.Text.Json
open System.Text.Json.Serialization
open Medhavi.Common

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

let private options = lazy (buildOptions())

let create<'a> () : Codec<'a> =
    { Encode =
        fun value ->
            try
                JsonSerializer.Serialize<'a>(value, options.Value) |> Ok
            with ex ->
                Error(EncodeError ex.Message)

      Decode =
        fun json ->
            try
                JsonSerializer.Deserialize<'a>(json, options.Value) |> Ok
            with ex ->
                Error(DecodeError ex.Message) }

let addConverters (targetOptions: JsonSerializerOptions) =
    for c in JsonFSharpOptions.Default().ToJsonSerializerOptions().Converters do
        targetOptions.Converters.Add c
