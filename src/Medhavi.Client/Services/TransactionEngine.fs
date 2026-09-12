namespace Medhavi.Client.Services

open System
open System.Threading.Tasks

/// Transaction state. Lifecycle of a server operation.
type TransactionState =
    | Pending
    | Running of startTime: DateTimeOffset
    | Completed of OperationResult
    | Failed of error: string * retryCount: int
    | Cancelled of reason: string option

/// Operation result. Outcome of a transaction.
and OperationResult =
    | Success of data: string
    | Failure of error: string
    | PartialSuccess of data: string * warnings: string list

/// Retry policy. Rules for retrying failed operations.
type RetryPolicy =
    { MaxRetries: int
      InitialDelay: TimeSpan
      BackoffMultiplier: float
      MaxDelay: TimeSpan }

/// Transaction request. Input to submit an operation.
type TransactionRequest =
    { OperationType: string
      Payload: string
      RetryPolicy: RetryPolicy
      SourceComponentId: string option }

/// Transaction. A trackable server operation.
type Transaction =
    { TransactionId: string
      OperationType: string
      Payload: string
      State: TransactionState
      CreatedAt: DateTimeOffset
      StartedAt: DateTimeOffset option
      CompletedAt: DateTimeOffset option
      RetryPolicy: RetryPolicy
      RetryCount: int
      SourceComponentId: string option }

/// TransactionEngine record. Manages all server operations.
type TransactionEngine =
    { Submit: TransactionRequest -> string
      Cancel: string -> unit
      GetStatus: string -> TransactionState
      Subscribe: string -> (TransactionState -> unit) -> IDisposable
      SubscribeAll: (string * TransactionState -> unit) -> IDisposable }

module TransactionEngine =
    let create () : TransactionEngine =
        let mutable nextId = 0
        let mutable transactions: Map<string, Transaction> = Map.empty
        let mutable listeners: Map<int, string * TransactionState -> unit> = Map.empty
        let lockObj = obj()

        let notifySubscribers (transactionId: string) (state: TransactionState) : unit =
            lock lockObj (fun () ->
                listeners
                |> Map.iter(fun _ listener ->
                    try
                        listener(transactionId, state)
                    with ex ->
                        printfn "[TransactionEngine] Listener error: %s" ex.Message))

        let submit (request: TransactionRequest) : string =
            let id = Guid.NewGuid().ToString()

            let transaction: Transaction =
                { TransactionId = id
                  OperationType = request.OperationType
                  Payload = request.Payload
                  State = Pending
                  CreatedAt = DateTimeOffset.UtcNow
                  StartedAt = None
                  CompletedAt = None
                  RetryPolicy = request.RetryPolicy
                  RetryCount = 0
                  SourceComponentId = request.SourceComponentId }

            lock lockObj (fun () -> transactions <- transactions |> Map.add id transaction)
            notifySubscribers id Pending
            id

        let cancel (id: string) : unit =
            lock lockObj (fun () ->
                transactions <-
                    transactions
                    |> Map.change
                        id
                        (Option.map(fun t ->
                            { t with
                                State = Cancelled(Some "User cancelled") })))

            notifySubscribers id (Cancelled(Some "User cancelled"))

        let getStatus (id: string) : TransactionState =
            lock lockObj (fun () ->
                match transactions |> Map.tryFind id with
                | Some t -> t.State
                | None -> Cancelled(Some "Transaction not found"))

        let subscribe (id: string) (listener: TransactionState -> unit) : IDisposable =
            let wrappedListener (txId: string, state: TransactionState) =
                if txId = id then
                    listener state

            let listenerId =
                lock lockObj (fun () ->
                    let listenerId = nextId
                    nextId <- nextId + 1
                    listeners <- listeners |> Map.add listenerId wrappedListener
                    listenerId)

            { new IDisposable with
                member _.Dispose() = lock lockObj (fun () -> listeners <- listeners |> Map.remove listenerId) }

        let subscribeAll (listener: string * TransactionState -> unit) : IDisposable =
            let listenerId =
                lock lockObj (fun () ->
                    let listenerId = nextId
                    nextId <- nextId + 1
                    listeners <- listeners |> Map.add listenerId listener
                    listenerId)

            { new IDisposable with
                member _.Dispose() = lock lockObj (fun () -> listeners <- listeners |> Map.remove listenerId) }

        { Submit = submit
          Cancel = cancel
          GetStatus = getStatus
          Subscribe = subscribe
          SubscribeAll = subscribeAll }
