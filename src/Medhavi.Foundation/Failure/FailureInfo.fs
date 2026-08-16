namespace Medhavi.Foundation.Failure

open System
open Medhavi.Foundation.ExecutionContext

type FailureInfo =
    { CorrelationId: CorrelationId
      Timestamp: DateTimeOffset
      ErrorType: string
      ErrorMessage: string
      StackTrace: string option
      InnerException: string option
      ContextData: Map<string, obj> }
