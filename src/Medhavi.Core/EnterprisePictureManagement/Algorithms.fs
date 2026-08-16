/// CA-C-019 Business Algorithms
module Medhavi.Core.EnterprisePictureManagement.Algorithms

open Medhavi.SemanticModel

/// Output contract of BA-C-001. Each reference set is assessed independently (BR-C-014).
type ReferenceSetMateriality =
    { SetName: string
      DraftCount: int
      PublishedCount: int
      ChangedCount: int
      Delta: decimal
      IsMaterial: bool }

type MaterialityAssessment =
    { HasMaterialChange: bool
      Demand: ReferenceSetMateriality
      Supply: ReferenceSetMateriality
      Inventory: ReferenceSetMateriality
      Reason: string }

/// Symmetric-difference ratio: |Draft △ Published| / max(|Draft|,|Published|,1)
let private deltaRatio (draft: Set<'a>) (published: Set<'a>) : decimal =
    let changed = Set.difference draft published |> Set.union(Set.difference published draft) |> Set.count
    let denom = max (max draft.Count published.Count) 1
    decimal changed / decimal denom

let private assessSet
    (name: string)
    (threshold: decimal)
    (draft: 'a list)
    (published: 'a list)
    : ReferenceSetMateriality =
    let d = Set.ofList draft
    let p = Set.ofList published
    let delta = deltaRatio d p

    { SetName = name
      DraftCount = d.Count
      PublishedCount = p.Count
      ChangedCount = Set.count(Set.difference d p |> Set.union(Set.difference p d))
      Delta = delta
      IsMaterial = delta >= threshold }

/// BA-C-001: Evaluate Picture Materiality.
/// Pure, deterministic, policy-driven. First publication is always material (BR-C-012).
let evaluatePictureMateriality
    (policy: EnterprisePicturePolicy)
    (draft: PictureVersion)
    (published: PictureVersion option)
    : MaterialityAssessment =

    match published with
    | None ->
        let mk n c =
            { SetName = n
              DraftCount = c
              PublishedCount = 0
              ChangedCount = c
              Delta = 1m
              IsMaterial = true }

        { HasMaterialChange = true
          Demand = mk "Demand" draft.DemandReferences.Length
          Supply = mk "Supply" draft.SupplyReferences.Length
          Inventory = mk "Inventory" draft.InventoryReferences.Length
          Reason = "First publication (BR-C-012)" }
    | Some pub ->
        let demand = assessSet "Demand" policy.DemandMaterialityThreshold draft.DemandReferences pub.DemandReferences
        let supply = assessSet "Supply" policy.SupplyMaterialityThreshold draft.SupplyReferences pub.SupplyReferences

        let inventory =
            assessSet "Inventory" policy.InventoryMaterialityThreshold draft.InventoryReferences pub.InventoryReferences

        let material = demand.IsMaterial || supply.IsMaterial || inventory.IsMaterial

        let reason =
            if material then
                sprintf "Material delta (D=%.3f,S=%.3f,I=%.3f)" demand.Delta supply.Delta inventory.Delta
            else
                "Below materiality threshold; draft retained"

        { HasMaterialChange = material
          Demand = demand
          Supply = supply
          Inventory = inventory
          Reason = reason }
