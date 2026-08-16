namespace Medhavi.Foundation.ExecutionContext

type ExecutionContextHolder =
    static member val private CurrentContext = System.Threading.AsyncLocal<ExecutionContext>() with get, set

    static member Set(ctx: ExecutionContext) = ExecutionContextHolder.CurrentContext.Value <- ctx

    static member TryGet() =
        let value = ExecutionContextHolder.CurrentContext.Value
        if obj.ReferenceEquals(value, null) then None else Some value

    static member Clear() = ExecutionContextHolder.CurrentContext.Value <- Unchecked.defaultof<ExecutionContext>

    static member GetCausalIds() =
        ExecutionContextHolder.TryGet() |> Option.map(fun ctx -> ctx.CausalDecisionIds) |> Option.defaultValue []
