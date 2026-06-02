module Medhavi.Promise.Routing

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise.PromiseTypes

let createInMemoryRoutingProvider () : RoutingProvider =
    let dur = TimeSpan.FromHours(24.0)
    let routingId =
        match RoutingId.create "RT-DEFAULT" with
        | Ok id -> id
        | Error _ -> failwith "Invalid routing id"
    
    let routingSelection =
        { Primary =
            { RoutingId = routingId
              AlternateUsed = false
              EstimatedDuration = Some dur
              Reliability = Some 0.95 }
          Alternates = [] }
    
    { Select =
        fun (_skuId, _stockingPointId) ->
            async {
                return Ok routingSelection
      } }

/// Select the best routing choice based on policy (used by orchestrator)
let selectBestRoutingChoice (policy: PromisePolicy) (selection: RoutingSelection) : RoutingChoice =
    match policy.TimePreference with
    | TimeVsCost.Fastest ->
        let sorted =
            selection.Primary :: selection.Alternates
            |> List.sortBy (fun c ->
                match c.EstimatedDuration with
                | Some d -> float d.TotalMinutes
                | None -> 1e6)
        match sorted with
        | x :: _ -> x
        | [] -> selection.Primary
    | _ -> selection.Primary