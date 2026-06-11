namespace Medhavi.Scheduler.Planning.Application

open Medhavi.Scheduler.Planning.Domain

type PlanVersionStore =
    { Save: PlanningResult -> Async<Result<unit, string>>
      Load: PlanVersionId -> Async<PlanningResult option>
      Exists: PlanVersionId -> Async<bool>
      Delete: PlanVersionId -> Async<Result<unit, string>> }

/// A simple thread-safe in-memory plan version store implementation.
module InMemoryPlanVersionStore =
    let create () : PlanVersionStore =
        let store =
            System.Collections.Concurrent.ConcurrentDictionary<PlanVersionId, PlanningResult>()

        { Save =
            fun result ->
                async {
                    let id = PlanVersionId.create result.InputFingerprintHash
                    store.[id] <- result
                    return Ok()
                }
          Load =
            fun versionId ->
                async {
                    match store.TryGetValue(versionId) with
                    | true, result -> return Some result
                    | _ -> return None
                }
          Exists = fun versionId -> async { return store.ContainsKey(versionId) }
          Delete =
            fun versionId ->
                async {
                    store.TryRemove(versionId) |> ignore
                    return Ok()
                } }
