namespace Medhavi.Scenario.Domain

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.ScenarioContracts

type ScenarioOverlaySet =
    { Id: ScenarioOverlaySetId
      ScenarioId: ScenarioId
      ScenarioType: ScenarioType
      Overrides: ScenarioDataOverride list
      Version: int
      LastModifiedAt: DateTimeOffset }

type ScenarioOverlayCommand =
    | CreateOverlaySet of id: ScenarioOverlaySetId * scenarioId: ScenarioId * scenarioType: ScenarioType
    | AddOverride of override_: ScenarioDataOverride
    | RemoveOverride of contentHash: string
    | ClearAll

type ScenarioOverlayEvent =
    | OverlaySetCreated of ScenarioOverlaySetId * ScenarioId
    | OverrideAdded of ScenarioOverlaySetId * ScenarioDataOverride
    | OverrideRemoved of ScenarioOverlaySetId * contentHash: string
    | AllOverridesCleared of ScenarioOverlaySetId

module ScenarioOverlaySetAgg =
    let private errConflict msg = Error (DomainError.conflict msg)
    let private errNotFound msg = Error (DomainError.notFound msg)
    let private errInvariant msg = Error (DomainError.invariant msg)

    let handle: Decide<ScenarioOverlaySet, ScenarioOverlayCommand, ScenarioOverlayEvent> =
        fun command stateOpt ->
            match command, stateOpt with
            | CreateOverlaySet(id, scenarioId, scenarioType), None ->
                let state =
                    { Id = id
                      ScenarioId = scenarioId
                      ScenarioType = scenarioType
                      Overrides = []
                      Version = 0
                      LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = state; Events = [ OverlaySetCreated(id, scenarioId) ] }

            | CreateOverlaySet _, Some _ -> errConflict "ScenarioOverlaySet already exists"

            | AddOverride override_, Some state ->
                match state.ScenarioType with
                | Baseline -> errInvariant "Baseline scenarios cannot have data overlays — only WhatIf and Sandbox are allowed"
                | WhatIf | Sandbox ->
                    let updated =
                        { state with
                            Overrides = state.Overrides @ [ override_ ]
                            Version = state.Version + 1
                            LastModifiedAt = DateTimeOffset.UtcNow }
                    Ok { NewState = updated; Events = [ OverrideAdded(state.Id, override_) ] }

            | RemoveOverride hash, Some state ->
                let remaining = state.Overrides |> List.filter (fun o -> ScenarioDataOverride.contentHash o <> hash)
                if List.length remaining = List.length state.Overrides then
                    errNotFound (sprintf "No override with hash '%s' found" hash)
                else
                    let updated =
                        { state with
                            Overrides = remaining
                            Version = state.Version + 1
                            LastModifiedAt = DateTimeOffset.UtcNow }
                    Ok { NewState = updated; Events = [ OverrideRemoved(state.Id, hash) ] }

            | ClearAll, Some state ->
                let updated =
                    { state with
                        Overrides = []
                        Version = state.Version + 1
                        LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = updated; Events = [ AllOverridesCleared state.Id ] }

            | _, None -> errNotFound "ScenarioOverlaySet not found"

    let evolve (event: ScenarioOverlayEvent) (stateOpt: ScenarioOverlaySet option) : ScenarioOverlaySet option =
        match event, stateOpt with
        | OverlaySetCreated(id, scenarioId), None ->
            Some
                { Id = id
                  ScenarioId = scenarioId
                  ScenarioType = WhatIf
                  Overrides = []
                  Version = 0
                  LastModifiedAt = DateTimeOffset.UtcNow }

        | OverrideAdded(_, override_), Some s ->
            Some { s with Overrides = s.Overrides @ [ override_ ]; Version = s.Version + 1 }

        | OverrideRemoved(_, hash), Some s ->
            let remaining = s.Overrides |> List.filter (fun o -> ScenarioDataOverride.contentHash o <> hash)
            Some { s with Overrides = remaining; Version = s.Version + 1 }

        | AllOverridesCleared _, Some s ->
            Some { s with Overrides = []; Version = s.Version + 1 }

        | _, _ -> stateOpt
