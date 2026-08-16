namespace Medhavi.Common

[<AutoOpen>]
module ComputationExpression =
    let state = StateBuilder()
    let result = Result.ResultBuilder()
    let asyncResult = AsyncResultBuilder()
    let taskResult = TaskResultBuilder()
