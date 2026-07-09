namespace Medhavi.SharedKernel.Failure

open System
open Medhavi.SharedKernel.ExecutionContext

type FailureInfo = {
    CorrelationId: CorrelationId
    Timestamp: DateTimeOffset
    ErrorType: string
    ErrorMessage: string
    StackTrace: string option
    InnerException: string option
    ContextData: Map<string, obj>
}
