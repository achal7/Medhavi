namespace Medhavi.Analytics.KPI

open System.Collections.Concurrent

module KpiRefreshEngine =

    let private cache = ConcurrentDictionary<KpiCacheKey, decimal>()

    let invalidateKeys (keys: KpiCacheKey list) : unit =
        for key in keys do
            cache.TryRemove(key) |> ignore

    let getCachedOrCompute (key: KpiCacheKey) (compute: unit -> decimal) : decimal =
        cache.GetOrAdd(key, fun _ -> compute())

    let clearAll () : unit =
        cache.Clear()
