module Medhavi.SharedKernel.InMemRepository

open System.Collections.Concurrent
open Medhavi.SharedKernel.Contracts.Aggregate
let createInMemoryRepository<'Aggregate, 'Id, 'Event when 'Id: not null> () =

    // Use versioned store for optimistic concurrency
    let store = ConcurrentDictionary<'Id, VersionedAggregate<'Aggregate>>()
    let eventStore = ConcurrentDictionary<'Id, 'Event list>()
    let locks = ConcurrentDictionary<'Id, obj>() // Simple lock objects per ID

    { Get =
        fun id ->
            task {
                match store.TryGetValue id with
                | true, versioned -> return Ok(Some versioned.Aggregate)
                | false, _ -> return Ok(None)
            }

      Save =
        fun (id, aggregate, events) ->
            task {
                // Get or create a lock for this ID
                let lockObj = locks.GetOrAdd(id, fun _ -> obj ())

                // Lock to prevent concurrent modifications
                return
                    lock lockObj (fun () ->
                        match store.TryGetValue id with
                        | true, existing ->
                            // Optimistic concurrency check - version would be passed in real scenario. For now, just update
                            let newVersioned =
                                { Aggregate = aggregate
                                  Version = existing.Version + 1 }

                            store[id] <- newVersioned
                        | false, _ ->
                            let newVersioned = { Aggregate = aggregate; Version = 1 }
                            store[id] <- newVersioned

                        let existingEvents =
                            match eventStore.TryGetValue id with
                            | true, evts -> evts
                            | false, _ -> []

                        eventStore[id] <- existingEvents @ events

                        Ok())
            }

      SaveBatch =
        fun batch ->
            task {
                for (id, aggregate, events) in batch do
                    let lockObj = locks.GetOrAdd(id, fun _ -> obj ())
                    lock lockObj (fun () ->
                        match store.TryGetValue id with
                        | true, existing ->
                            let newVersioned =
                                { Aggregate = aggregate
                                  Version = existing.Version + 1 }
                            store[id] <- newVersioned
                        | false, _ ->
                            let newVersioned = { Aggregate = aggregate; Version = 1 }
                            store[id] <- newVersioned

                        let existingEvents =
                            match eventStore.TryGetValue id with
                            | true, evts -> evts
                            | false, _ -> []

                        eventStore[id] <- existingEvents @ events
                        ())
                return Ok()
            }

      Delete =
        fun id ->
            task {
                let lockObj = locks.GetOrAdd(id, fun _ -> obj ())

                return
                    lock lockObj (fun () ->
                        let isSuccess, _ = store.TryRemove id
                        eventStore.TryRemove id |> ignore
                        locks.TryRemove id |> ignore

                        if isSuccess then
                            Ok()
                        else
                            Error(RepositoryError.NotFound $"Aggregate with id {id} not found"))
            }

      GetEvents =
        fun id ->
            task {
                match eventStore.TryGetValue id with
                | true, events -> return Ok events
                | false, _ -> return Ok []
            }

      GetEventsByType =
        fun predicate ->
            task {
                let allEvents =
                    eventStore.Values
                    |> Seq.collect id
                    |> Seq.filter predicate
                    |> Seq.toList

                return Ok allEvents
            }

      GetAll =
        fun () ->
            task {
                return
                    store.Values
                    |> Seq.map (fun v -> v.Aggregate)
                    |> Seq.toList
                    |> Ok
            } }
