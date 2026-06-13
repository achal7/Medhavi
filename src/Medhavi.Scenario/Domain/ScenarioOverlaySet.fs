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

    let isMatching newOv existingOv =
        match newOv, existingOv with
        | DemandOverride(id1, _, _), DemandOverride(id2, _, _) -> id1 = id2
        | InventoryOverride(sku1, sp1, _), InventoryOverride(sku2, sp2, _) -> sku1 = sku2 && sp1 = sp2
        | LeadTimeOverride(sku1, _, _), LeadTimeOverride(sku2, _, _) -> sku1 = sku2
        | CapacityOverride(res1, buck1, _), CapacityOverride(res2, buck2, _) -> res1 = res2 && buck1 = buck2
        | SupplierSuspension(sup1, buck1), SupplierSuspension(sup2, buck2) -> sup1 = sup2 && buck1 = buck2
        
        | SupplierReactivation(sup1, _), SupplierReactivation(sup2, _) -> sup1 = sup2
        | SupplierLeadTimeOverride(sup1, sku1, _, _), SupplierLeadTimeOverride(sup2, sku2, _, _) -> sup1 = sup2 && sku1 = sku2
        | SupplierCapacityOverride(sup1, sku1, _, _), SupplierCapacityOverride(sup2, sku2, _, _) -> sup1 = sup2 && sku1 = sku2
        | SupplierPriceOverride(sup1, sku1, _, _), SupplierPriceOverride(sup2, sku2, _, _) -> sup1 = sup2 && sku1 = sku2
        
        | (BomOverride(p1, c1, _) | BomComponentAddition(p1, c1, _, _) | BomComponentRemoval(p1, c1, _)),
          (BomOverride(p2, c2, _) | BomComponentAddition(p2, c2, _, _) | BomComponentRemoval(p2, c2, _)) -> p1 = p2 && c1 = c2
        
        | BomAlternateSelection(p1, _, _), BomAlternateSelection(p2, _, _) -> p1 = p2
        
        | KpiWeightOverride(k1, _), KpiWeightOverride(k2, _) -> k1 = k2
        | ServiceLevelTargetOverride(t1, _), ServiceLevelTargetOverride(t2, _) -> t1 = t2
        | CostRiskTradeoffOverride(p1, _), CostRiskTradeoffOverride(p2, _) -> p1 = p2
        | CarbonWeightOverride(p1, _), CarbonWeightOverride(p2, _) -> p1 = p2
        | FreezePolicyOverride(p1, _), FreezePolicyOverride(p2, _) -> p1 = p2
        | ApprovalThresholdOverride(p1, _), ApprovalThresholdOverride(p2, _) -> p1 = p2
        
        | TagAddedOverride(c1, t1, _), TagAddedOverride(c2, t2, _) -> c1 = c2 && t1 = t2
        | AnnotationAddedOverride(c1, _, _), AnnotationAddedOverride(c2, _, _) -> c1 = c2
        | RelationHintAddedOverride(s1, r1, o1, _), RelationHintAddedOverride(s2, r2, o2, _) -> s1 = s2 && r1 = r2 && o1 = o2
        | _ -> false

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
                    let filtered = state.Overrides |> List.filter (fun o -> not (isMatching override_ o))
                    let updated =
                        { state with
                            Overrides = filtered @ [ override_ ]
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
            let filtered = s.Overrides |> List.filter (fun o -> not (isMatching override_ o))
            Some { s with Overrides = filtered @ [ override_ ]; Version = s.Version + 1 }

        | OverrideRemoved(_, hash), Some s ->
            let remaining = s.Overrides |> List.filter (fun o -> ScenarioDataOverride.contentHash o <> hash)
            Some { s with Overrides = remaining; Version = s.Version + 1 }

        | AllOverridesCleared _, Some s ->
            Some { s with Overrides = []; Version = s.Version + 1 }

        | _, _ -> stateOpt
