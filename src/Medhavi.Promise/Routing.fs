module Medhavi.Promise.Routing

open Medhavi.SharedKernel.PromisePolicy
open Medhavi.Promise.PromiseTypes

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
