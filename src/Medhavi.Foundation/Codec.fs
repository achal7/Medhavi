module Medhavi.Foundation.Codec

open System.Text.Json
open Medhavi.Common

/// Creates a JSON codec for any type using System.Text.Json
let json<'T> : Codec<'T> =
    { Encode =
        fun value ->
            try
                Ok(JsonSerializer.Serialize(value))
            with ex ->
                Error(EncodeError $"Failed to encode {typeof<'T>.Name}: {ex.Message}")
      Decode =
        fun json ->
            try
                Ok(JsonSerializer.Deserialize<'T>(json))
            with ex ->
                Error(DecodeError $"Failed to decode {typeof<'T>.Name}: {ex.Message}") }

/// Creates a codec with custom serializer options
let jsonWithOptions<'T> (options: JsonSerializerOptions) : Codec<'T> =
    { Encode =
        fun value ->
            try
                Ok(JsonSerializer.Serialize(value, options))
            with ex ->
                Error(EncodeError $"Failed to encode {typeof<'T>.Name}: {ex.Message}")
      Decode =
        fun json ->
            try
                Ok(JsonSerializer.Deserialize<'T>(json, options))
            with ex ->
                Error(DecodeError $"Failed to decode {typeof<'T>.Name}: {ex.Message}") }
