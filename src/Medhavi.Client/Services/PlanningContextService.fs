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

    type private ContextMsg =
        | Get of AsyncReplyChannel<PlanningContext>
        | Update of (PlanningContext -> PlanningContext)
        | Subscribe of (PlanningContext -> unit) * AsyncReplyChannel<unit -> unit>
        | Unsubscribe of Guid

    let create (initial: PlanningContext) : PlanningContextService =
        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop (current: PlanningContext) (subscribers: Map<Guid, PlanningContext -> unit>) =
                    async {
                        let! msg = inbox.Receive()
                        match msg with
                        | Get reply ->
                            reply.Reply current
                            return! loop current subscribers
                        | Update transform ->
                            let next = transform current
                            for KeyValue(_, sub) in subscribers do
                                sub next
                            return! loop next subscribers
                        | Subscribe(sub, reply) ->
                            let id = Guid.NewGuid()
                            let unsubscribe () = inbox.Post(Unsubscribe id)
                            reply.Reply unsubscribe
                            return! loop current (Map.add id sub subscribers)
                        | Unsubscribe id ->
                            return! loop current (Map.remove id subscribers)
                    }
                loop initial Map.empty)

        { Get = fun () -> agent.PostAndReply Get
          Update = fun f -> agent.Post(Update f)
          Subscribe = fun sub -> agent.PostAndReply(fun reply -> Subscribe(sub, reply)) }

    let createDefault () : PlanningContext =
        let defaultHorizon =
            match Horizon.presets() with
            | (_, r) :: _ -> r
            | [] ->
                let now = DateTimeOffset.Now
                { Start = now; End = now.AddDays 30.0 }

        { Scenario = ScenarioId.baseline
          Horizon = defaultHorizon
          PlantId = None }
