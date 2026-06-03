module Medhavi.Planning.Mrp.Steps.BomExplosionStep

open System
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.Errors
open Medhavi.Scheduler.Mrp.Pipeline
open Medhavi.Scheduler.Mrp.Domain.Algorithms

/// Create the BOM explosion step with injected lookup dependency
let createStep (bomLookup: BomExplosion.BomLookup) : MrpStepAsync<MrpDemand list, ExplodedComponent list> =
    fun demands ctx ->
        task {
            let startTime = DateTimeOffset.UtcNow

            match BomExplosion.explodeAll bomLookup ctx.Policy.BomSelectionPolicy demands with
            | Error errs -> return Error(BomExplosion errs)
            | Ok components ->
                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (BomExplosionCompleted(List.length components))
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with
                            BomExplosionDuration = duration
                            ComponentsProcessed = t.ComponentsProcessed + List.length components })

                return Ok(components, updatedCtx)
        }
