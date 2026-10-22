module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Algorithms

open Model
open Policies

// =============================================================================
// SE-S-001 — Supply Understanding Business Algorithms (Chapter 10)
// Traces to: BA-S-001 (Evaluate Understanding Change Materiality)
// =============================================================================

/// BA-S-001 — per-dimension materiality result
type MaterialityResult =
    | Material of reasonCode: string * evidence: string
    | NotMaterial of evidence: string
    | NotApplicable of reason: string

module MaterialityResult =
    let isMaterial (r: MaterialityResult) =
        match r with
        | Material _ -> true
        | NotMaterial _
        | NotApplicable _ -> false

/// BA-S-001 §9 — structured materiality assessment across the six interpretation dimensions
type MaterialityAssessment =
    { Continuity: MaterialityResult
      CapacityAdequacy: MaterialityResult
      InventoryPosition: MaterialityResult
      SupplierPosture: MaterialityResult
      OverallRisk: MaterialityResult
      Confidence: MaterialityResult
      HasMaterialChange: bool }

/// BA-S-001 §8 — input contract
type MaterialityInput =
    { Draft: Interpretation
      Published: Interpretation option }

let private evaluateContinuity (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.Continuity, pub.Continuity with
        | Incomplete r, _ -> NotApplicable $"DraftContinuityIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedContinuityIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("ContinuityStatusShift", $"Continuity changed from %A{p} to %A{d}")
            elif input.Draft.ContinuityDrivers <> pub.ContinuityDrivers then
                Material("ContinuityDriversShift", "Supply continuity drivers have changed")
            else
                NotMaterial "WithinNominalTolerances"

let private evaluateCapacity (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.CapacityAdequacy, pub.CapacityAdequacy with
        | Incomplete r, _ -> NotApplicable $"DraftCapacityIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedCapacityIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("CapacityStatusShift", $"Capacity adequacy changed from %A{p} to %A{d}")
            elif input.Draft.Bottlenecks <> pub.Bottlenecks then
                Material("BottleneckShiftExceeded", $"Bottleneck resources changed: %A{input.Draft.Bottlenecks}")
            else
                NotMaterial "WithinNominalTolerances"

let private evaluateInventory (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.InventoryPosition, pub.InventoryPosition with
        | Incomplete r, _ -> NotApplicable $"DraftInventoryIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedInventoryIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("InventoryStatusShift", $"Inventory position changed from %A{p} to %A{d}")
            elif input.Draft.StockoutRiskItems <> pub.StockoutRiskItems then
                Material("StockoutExposureDeltaExceeded", "Stockout exposure item list changed")
            elif input.Draft.ExcessInventoryItems <> pub.ExcessInventoryItems then
                Material("ExcessExposureDeltaExceeded", "Excess inventory item list changed")
            else
                NotMaterial "WithinNominalTolerances"

let private evaluateSupplier (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.SupplierPosture, pub.SupplierPosture with
        | Incomplete r, _ -> NotApplicable $"DraftSupplierIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedSupplierIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("SupplierConfidenceDeltaExceeded", $"Supplier confidence posture changed from %A{p} to %A{d}")
            elif input.Draft.CriticalDependencies <> pub.CriticalDependencies then
                Material("CriticalSupplierDependenciesShift", "Critical supplier dependencies changed")
            else
                NotMaterial "WithinNominalTolerances"

let private evaluateRisk (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.OverallRisk, pub.OverallRisk with
        | Incomplete r, _ -> NotApplicable $"DraftRiskIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedRiskIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("RiskLevelTransition", $"Overall supply risk escalated/de-escalated from %A{p} to %A{d}")
            elif input.Draft.RiskDrivers <> pub.RiskDrivers then
                Material("RiskDriversShift", "Primary risk drivers have changed")
            else
                NotMaterial "WithinNominalTolerances"

let private evaluateConfidence (input: MaterialityInput) : MaterialityResult =
    match input.Published with
    | None -> NotApplicable "Initial baseline publication"
    | Some pub ->
        match input.Draft.Confidence, pub.Confidence with
        | Incomplete r, _ -> NotApplicable $"DraftConfidenceIncomplete: {r}"
        | _, Incomplete r -> NotApplicable $"PublishedConfidenceIncomplete: {r}"
        | Known d, Known p ->
            if d <> p then
                Material("ConfidenceDegraded", $"Confidence shifted from %A{p} to %A{d}")
            else
                NotMaterial "WithinNominalTolerances"

/// BA-S-001: Evaluate Understanding Change Materiality
let evaluateUnderstandingChangeMateriality
    (_: MaterialityPolicy)
    (input: MaterialityInput)
    : MaterialityAssessment =
    match input.Published with
    | None ->
        // First publication is always material per BR-S-111 and BA-S-001 initial publication rule
        { Continuity = NotApplicable "Initial baseline publication"
          CapacityAdequacy = NotApplicable "Initial baseline publication"
          InventoryPosition = NotApplicable "Initial baseline publication"
          SupplierPosture = NotApplicable "Initial baseline publication"
          OverallRisk = NotApplicable "Initial baseline publication"
          Confidence = NotApplicable "Initial baseline publication"
          HasMaterialChange = true }
    | Some _ ->
        let continuity = evaluateContinuity input
        let capacity = evaluateCapacity input
        let inventory = evaluateInventory input
        let supplier = evaluateSupplier input
        let risk = evaluateRisk input
        let confidence = evaluateConfidence input

        let hasMaterialChange =
            [ continuity; capacity; inventory; supplier; risk; confidence ]
            |> List.exists MaterialityResult.isMaterial

        { Continuity = continuity
          CapacityAdequacy = capacity
          InventoryPosition = inventory
          SupplierPosture = supplier
          OverallRisk = risk
          Confidence = confidence
          HasMaterialChange = hasMaterialChange }
