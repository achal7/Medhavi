namespace Medhavi.Nexus

open System

[<AutoOpen>]
module ResultExtensions =
    module Result =
        let get =
            function
            | Ok x -> x
            | Error e -> failwithf "Expected Ok, got Error: %A" e
