namespace Medhavi.Common

open Medhavi.Common.Patterns.StateMonad

[<AutoOpen>]
module ComputationExpression =
    let state = StateBuilder()
    let result = Result.ResultBuilder()
    let asyncResult = Patterns.AsyncResultBuilder()
    let taskResult = Patterns.TaskResultBuilder()
