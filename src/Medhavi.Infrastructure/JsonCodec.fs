module Medhavi.Infrastructure.JsonCodec

open System.Text.Json
open System.Text.Json.Serialization
open Medhavi.Common

let create<'a> () : Codec<'a> = Medhavi.Foundation.Codec.json<'a>
