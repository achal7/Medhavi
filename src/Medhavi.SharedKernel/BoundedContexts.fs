namespace Medhavi.SharedKernel.BoundedContexts

open System

type IngestionLogger =
    { LogInfo: string -> unit
      LogSuccess: string -> unit
      LogError: string -> unit }

type DomainEventBus =
    static let eventObj = new Event<obj>()

    static member Publish(evt: obj) =
        eventObj.Trigger(evt)

    static member Subscribe<'T>(handler: 'T -> unit) : IDisposable =
        eventObj.Publish
        |> Observable.choose (fun o ->
            match o with
            | :? 'T as e -> Some e
            | _ -> None)
        |> Observable.subscribe handler
