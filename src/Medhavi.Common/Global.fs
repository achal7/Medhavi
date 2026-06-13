namespace Medhavi.Common

open Medhavi.Common.Patterns.StateMonad

[<AutoOpen>]
module ComputationExpression =
    let state = StateBuilder()
    let result = Result.ResultBuilder()
    let asyncResult = Patterns.AsyncResultBuilder()
    let taskResult = Patterns.TaskResultBuilder()

[<AutoOpen>]
module ResultExtensions =
    module Result =
        let get = function
            | Ok x -> x
            | Error e -> failwithf "Expected Ok, got Error: %A" e
