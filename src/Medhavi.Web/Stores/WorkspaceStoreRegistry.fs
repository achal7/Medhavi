namespace Medhavi.Web.Stores

open System
open System.Collections.Concurrent
open Medhavi.Contracts.Scenario

type PlanningContextStore =
    { Get: unit -> PlanningContext
      Set: PlanningContext -> unit
      Update: (PlanningContext -> PlanningContext) -> unit
      Subscribe: (PlanningContext -> unit) -> SubscriptionId
      Unsubscribe: SubscriptionId -> unit }

module PlanningContextStore =
    let create (initialContext: PlanningContext) =
        let gate = obj()
        let mutable context = initialContext
        let listeners = ConcurrentDictionary<SubscriptionId, PlanningContext -> unit>()

        let notify newContext =
            listeners.Values
            |> Seq.iter(fun listener ->
                try
                    listener newContext
                with ex ->
                    printfn $"[PlanningContextStore] Listener failed: {ex.Message}")

        let get () = lock gate (fun () -> context)

        let set newContext =
            lock gate (fun () -> context <- newContext)
            notify newContext

        let update updater =
            let newContext =
                lock gate (fun () ->
                    context <- updater context
                    context)

            notify newContext

        let subscribe listener =
            let id = SubscriptionId(Guid.NewGuid())
            listeners[id] <- listener
            id

        let unsubscribe (SubscriptionId id) = listeners.TryRemove(SubscriptionId id) |> ignore

        { Get = get
          Set = set
          Update = update
          Subscribe = subscribe
          Unsubscribe = unsubscribe }

type WorkspaceStoreRegistry =
    { ContextStore: PlanningContextStore
      Register: WorkspaceKind * obj -> unit
      TryGet: WorkspaceKind -> obj option
      MarkAllStale: unit -> unit
      ClearAll: unit -> unit }

module WorkspaceStoreRegistry =
    let create (contextStore: PlanningContextStore) =
        let stores = ConcurrentDictionary<WorkspaceKind, obj>()
        let _ = obj()

        let register (kind, store) = stores[kind] <- store

        let tryGet kind =
            match stores.TryGetValue kind with
            | true, store -> Some store
            | _ -> None

        let markAllStale () =
            stores.Values
            |> Seq.iter(fun storeObj ->
                // Use reflection to call MarkStale
                let storeType = storeObj.GetType()
                let markStaleMethod = storeType.GetMethod("MarkStale")

                if markStaleMethod <> null then
                    markStaleMethod.Invoke(storeObj, [||]) |> ignore)

        let clearAll () =
            stores.Values
            |> Seq.iter(fun storeObj ->
                let storeType = storeObj.GetType()
                let clearMethod = storeType.GetMethod("Clear")

                if clearMethod <> null then
                    clearMethod.Invoke(storeObj, [||]) |> ignore)

        // Auto-subscribe: when context changes, mark all stores as stale
        contextStore.Subscribe(fun _ -> markAllStale()) |> ignore

        { ContextStore = contextStore
          Register = register
          TryGet = tryGet
          MarkAllStale = markAllStale
          ClearAll = clearAll }
