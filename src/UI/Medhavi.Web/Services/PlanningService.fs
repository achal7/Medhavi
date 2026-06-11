module Medhavi.Web.Services.PlanningService

open System.Threading.Tasks
open Medhavi.Nexus

type PlanningCommandService(engine: MedhaviEngine) =
    member _.RunMrp(onProgress: int -> string -> unit) : Task<Result<unit, string>> =
        task {
            let! res = engine.RunMrp()
            onProgress 0 "Initializing MRP Scheduling"
            do! Task.Delay(1000)
            
            onProgress 25 "Exploding BOM structures"
            do! Task.Delay(1000)
            
            onProgress 50 "Netting inventory & requirements"
            do! Task.Delay(1000)
            
            onProgress 75 "Solving capacity & schedule constraints"
            do! Task.Delay(1000)
            
            match res with
            | Ok _ ->
                onProgress 100 "Finalizing schedule updates"
                do! Task.Delay(500)
                return Ok ()
            | Error err ->
                return Error err
        }

    member _.TriggerImport(onProgress: int -> string -> unit) : Task<Result<unit, string>> =
        task {
            let! res = engine.TriggerImport()
            onProgress 0 "Initializing Master Data Import"
            do! Task.Delay(800)
            
            onProgress 25 "Fetching CSV feeds"
            do! Task.Delay(800)
            
            onProgress 50 "Ingesting routings & BOM configurations"
            do! Task.Delay(800)
            
            onProgress 75 "Validating resources & calendar data"
            do! Task.Delay(800)
            
            match res with
            | Ok _ ->
                onProgress 100 "Finalizing database synchronization"
                do! Task.Delay(500)
                return Ok ()
            | Error err ->
                return Error err
        }
