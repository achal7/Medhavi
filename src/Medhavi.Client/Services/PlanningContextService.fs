namespace Medhavi.Client.Services

open System
open Medhavi.Client
open Medhavi.Client.Primitives

module Horizon =
    let presets () : (string * DateRange) list =
        let today = DateTimeOffset.Now.Date

        [ "Last 7 + Next 90 days",
          { Start = DateTimeOffset.op_Implicit(today.AddDays -7.0)
            End = DateTimeOffset.op_Implicit(today.AddDays 90.0) }
          "Next 30 days",
          { Start = DateTimeOffset.op_Implicit(today)
            End = DateTimeOffset.op_Implicit(today.AddDays 30.0) }
          "Next 90 days",
          { Start = DateTimeOffset.op_Implicit(today)
            End = DateTimeOffset.op_Implicit(today.AddDays 90.0) } ]

    let nameOf (range: DateRange) : string =
        presets() |> List.tryFind(fun (_, r) -> r = range) |> Option.map fst |> Option.defaultValue "Custom"

type PlanningContextService =
    { Get: unit -> PlanningContext
      Update: (PlanningContext -> PlanningContext) -> unit
      Subscribe: (PlanningContext -> unit) -> (unit -> unit) }

module PlanningContextService =
    let create (initial: PlanningContext) : PlanningContextService =
        let lockObj = obj()
        let mutable current = initial
        let listeners = System.Collections.Generic.List<PlanningContext -> unit>()

        { Get = fun () -> lock lockObj (fun () -> current)
          Update =
            fun f ->
                let next =
                    lock lockObj (fun () ->
                        current <- f current
                        current)

                lock lockObj (fun () -> listeners |> Seq.iter(fun l -> l next))
          Subscribe =
            fun l ->
                lock lockObj (fun () -> listeners.Add(l))
                fun () -> lock lockObj (fun () -> listeners.Remove(l) |> ignore) }

    let createDefault () : PlanningContext =
        { Scenario = ScenarioId.baseline
          Horizon = snd(List.head(Horizon.presets()))
          PlantId = None }
