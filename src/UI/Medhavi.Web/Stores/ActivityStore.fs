namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Web
open Medhavi.Nexus

type ActivityStore = {
    GetSnapshot : unit -> UIEventLogItem list
    Refresh     : unit -> Task<unit>
    Subscribe   : (unit -> unit) -> IDisposable
}

module ActivityStore =
    let create (engine: MedhaviEngine) : ActivityStore =
        let mutable cache : UIEventLogItem list = []
        let listeners = System.Collections.Generic.List<unit -> unit>()

        let notifySubscribers () =
            for listener in listeners do
                listener ()

        let getSnapshot () = cache

        let subscribe listener =
            listeners.Add(listener)
            { new IDisposable with
                member _.Dispose() = listeners.Remove(listener) |> ignore }

        let refresh () =
            task {
                try
                    let! events = engine.GetEvents()
                    cache <- events
                    notifySubscribers ()
                with ex ->
                    printfn "[ActivityStore] Error during refresh: %s" ex.Message
            }

        { GetSnapshot = getSnapshot
          Subscribe = subscribe
          Refresh = refresh }
