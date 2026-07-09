namespace Medhavi.SharedKernel.Execution

open System.Threading

type ExecutionContextHolder =
    static member val private CurrentContext = AsyncLocal<ExecutionContext>() with get, set

    static member Set(ctx: ExecutionContext) =
        ExecutionContextHolder.CurrentContext.Value <- ctx

    static member TryGet() =
        let value = ExecutionContextHolder.CurrentContext.Value
        if obj.ReferenceEquals(value, null) then None
        else Some value

    static member Clear() =
        ExecutionContextHolder.CurrentContext.Value <- Unchecked.defaultof<ExecutionContext>
