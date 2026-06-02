/// BOM Explosion Step — Explodes demands recursively through the BOM hierarchy
/// FP Pattern: Railway-Oriented Programming (ROP) with async pipelines
module Medhavi.Planning.Mrp.Steps.BomExplosionStep

open System
open Medhavi.SharedKernel
open Medhavi.Planning.Mrp.Domain.Types
open Medhavi.Planning.Mrp.Domain.Errors
open Medhavi.Planning.Mrp.Pipeline.PipelineTypes
open Medhavi.Planning.Mrp.Domain.Algorithms

/// Create the BOM explosion step with injected lookup dependency
let createStep (bomLookup: BomExplosion.BomLookup) : MrpStepAsync<MrpDemand list, ExplodedComponent list> =
    fun demands ctx ->
        async {
            let startTime = DateTimeOffset.UtcNow

            match BomExplosion.explodeAll bomLookup ctx.Policy.BomSelectionPolicy demands with
            | Error errs ->
                return Error (BomExplosion errs)
            | Ok components ->
                let endTime = DateTimeOffset.UtcNow
                let duration = endTime - startTime

                let updatedCtx =
                    ctx
                    |> MrpContext.addEvent (BomExplosionCompleted (List.length components))
                    |> MrpContext.updateTelemetry (fun t ->
                        { t with
                            BomExplosionDuration = duration
                            ComponentsProcessed = t.ComponentsProcessed + List.length components })

                return Ok (components, updatedCtx)
        }
