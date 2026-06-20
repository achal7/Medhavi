module Medhavi.Web.Stores.WorkspaceStore

open System
open System.Threading
open System.Collections.Concurrent
open Medhavi.Common.Patterns
open Medhavi.Contracts.Scenario
let create<'TState>
    (loadFromBackend: PlanningContext -> TaskResult<'TState, string>)
    (_: PlanningContext)
    (_: (('TState option -> 'TState option) -> unit) option)
    : WorkspaceStore<'TState> * (('TState option -> 'TState option) -> unit) =

    // Encapsulated mutable state (hidden from outside)
    let gate = obj()
    let refreshLock = new SemaphoreSlim(1, 1)
    let listeners = ConcurrentDictionary<SubscriptionId, StoreEvent<WorkspaceSnapshot<'TState>> -> unit>()
    let mutable snapshot = WorkspaceSnapshot<'TState>.Default()

    let notify event =
        listeners.Values
        |> Seq.iter(fun listener ->
            try
                listener event
            with ex ->
                printfn $"[WorkspaceStore] Listener failed: {ex.Message}")

    let get () = lock gate (fun () -> snapshot)

    let markStale () =
        lock gate (fun () ->
            snapshot <-
                { snapshot with
                    Freshness = Stale
                    Version = snapshot.Version + 1L })

        notify(StateChanged snapshot)

    let clear () =
        lock gate (fun () -> snapshot <- WorkspaceSnapshot<'TState>.Default())
        notify(StateChanged snapshot)

    let subscribe listener =
        let id = SubscriptionId(Guid.NewGuid())
        listeners[id] <- listener
        id

    let unsubscribe (SubscriptionId id) = listeners.TryRemove(SubscriptionId id) |> ignore

    let updateStore update =
        lock gate (fun () ->
            snapshot <-
                { snapshot with
                    Data = update snapshot.Data
                    Version = snapshot.Version + 1L })

        notify(StateChanged snapshot)

    let refresh (context: PlanningContext) =
        task {
            do! refreshLock.WaitAsync() |> Async.AwaitTask
            try
                // Update snapshot to Loading state
                lock gate (fun () ->
                    snapshot <-
                        { snapshot with
                            Freshness = Loading
                            Error = None
                            Version = snapshot.Version + 1L })

                notify(StateChanged snapshot)

                // Load from backend
                let! result = loadFromBackend context

                match result with
                | Ok data ->
                    let now = DateTime.UtcNow

                    lock gate (fun () ->
                        snapshot <-
                            { snapshot with
                                Data = Some data
                                Freshness = Fresh
                                Error = None
                                Version = snapshot.Version + 1L
                                LastRefreshUtc = Some now })

                    notify(StateChanged snapshot)
                    return Ok snapshot

                | Error err ->
                    lock gate (fun () ->
                        snapshot <-
                            { snapshot with
                                Freshness = Failed err
                                Error = Some err
                                Version = snapshot.Version + 1L })

                    notify(StateChanged snapshot)
                    notify(ErrorOccurred err)
                    return Error err

            finally
                refreshLock.Release() |> ignore
        }

    let store =
        { Get = get
          Refresh = refresh
          MarkStale = markStale
          Subscribe = subscribe
          Unsubscribe = unsubscribe
          Clear = clear }

    store, updateStore
