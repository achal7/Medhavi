module Medhavi.Promise.LimiterSelection

open System
open Medhavi.Contracts.Promise

type ReadyDates =
    { MaterialReady: DateTimeOffset option
      CapacityReady: DateTimeOffset option
      TransportReady: DateTimeOffset option
      AsOf: DateTimeOffset }

let selectLimiter (dates: ReadyDates) : PromiseLimiter option =
    let deltas =
        [ ("Material", dates.MaterialReady)
          ("Capacity", dates.CapacityReady)
          ("Transport", dates.TransportReady) ]
        |> List.map (fun (name, ready) ->
            let delta =
                match ready with
                | Some r -> (r - dates.AsOf).TotalMinutes
                | None -> infinity

            name, delta)

    match deltas with
    | [] -> None
    | xs ->
        let hasAllInfinite = xs |> List.forall (fun (_, d) -> d = infinity)

        if hasAllInfinite then
            Some
                { Domain = PromiseLimiterDomain.System
                  Code = PromiseReasonCode.SearchTimeout
                  Message = "No valid readiness dates available"
                  Suggestions = [] }
        else
            let maxDelta = xs |> List.maxBy snd |> fst

            match maxDelta with
            | "Material" ->
                Some
                    { Domain = PromiseLimiterDomain.Material
                      Code = PromiseReasonCode.MaterialShortfall
                      Message = "Material availability is the bottleneck"
                      Suggestions = [ "expediteInbound"; "sourceAlternate" ] }
            | "Capacity" ->
                Some
                    { Domain = PromiseLimiterDomain.Capacity
                      Code = PromiseReasonCode.CapacityShortfall
                      Message = "Capacity is the bottleneck"
                      Suggestions = [ "extendTime"; "addShift" ] }
            | "Transport" ->
                Some
                    { Domain = PromiseLimiterDomain.Transport
                      Code = PromiseReasonCode.NoTransportCapacity
                      Message = "Transport is the bottleneck"
                      Suggestions = [ "expediteOrder"; "changeRouting" ] }
            | _ -> None
