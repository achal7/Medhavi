namespace Medhavi.Client

open System

/// Error categories for classification.
type ErrorCategory =
    | Transient // Network timeout, temporary unavailability
    | Validation // User input errors
    | Business // Domain rule violations
    | Infrastructure // Server errors, database failures

/// Error entry for tracking.
type ErrorEntry =
    { Category: ErrorCategory
      Message: string
      Details: string option
      Timestamp: DateTimeOffset
      Source: string option }

type ErrorService =
    { Handle: ErrorEntry -> unit
      Subscribe: (ErrorEntry -> unit) -> IDisposable }

module ErrorService =
    /// Generic retry function with exponential backoff.
    let retry
        (operation: 'T -> System.Threading.Tasks.Task<Result<'T, string>>)
        (input: 'T)
        (maxRetries: int)
        : System.Threading.Tasks.Task<Result<'T, string>> =
        task {
            let mutable attempt = 0
            let mutable result = Error "Not attempted"

            while attempt < maxRetries && (result |> Result.isError) do
                let! r = operation input
                result <- r

                if result |> Result.isError then
                    do! System.Threading.Tasks.Task.Delay(200 * (pown 2 attempt))
                    attempt <- attempt + 1

            return result
        }

    let create () : ErrorService =
        let mutable nextId = 0
        let mutable listeners: Map<int, ErrorEntry -> unit> = Map.empty
        let lockObj = obj()

        let handle (entry: ErrorEntry) : unit =
            lock lockObj (fun () ->
                listeners
                |> Map.iter(fun _ listener ->
                    try
                        listener entry
                    with ex ->
                        printfn "[ErrorService] Listener error: %s" ex.Message))

        let subscribe (listener: ErrorEntry -> unit) : IDisposable =
            let id =
                lock lockObj (fun () ->
                    let id = nextId
                    nextId <- nextId + 1
                    listeners <- listeners |> Map.add id listener
                    id)

            { new IDisposable with
                member _.Dispose() = lock lockObj (fun () -> listeners <- listeners |> Map.remove id) }

        { Handle = handle
          Subscribe = subscribe }
