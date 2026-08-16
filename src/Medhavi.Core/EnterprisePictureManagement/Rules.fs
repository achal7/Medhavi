/// CA-C-019 Enterprise Picture Management Rules
module Medhavi.Core.EnterprisePictureManagement.Rules

open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Model
open Medhavi.Core.ArsIdentifiers

type ComposeInput =
    { Cmd: ComposePictureVersionCmd
      CurrentState: EnterprisePicture option }

type PublishInput =
    { Cmd: PublishPictureVersionCmd
      CurrentState: EnterprisePicture option }

let compositionRequiresReferences: Medhavi.Foundation.Contracts.Rule<ComposeInput> =
    Rule.create
        Rules.compositionRequiresReferences.Id
        Rules.compositionRequiresReferences.Explanation
        (fun input ->
            not input.Cmd.DemandReferences.IsEmpty
            || not input.Cmd.SupplyReferences.IsEmpty
            || not input.Cmd.InventoryReferences.IsEmpty)
        (fun input ->
            sprintf
                "Demand: %d, Supply: %d, Inventory: %d"
                input.Cmd.DemandReferences.Length
                input.Cmd.SupplyReferences.Length
                input.Cmd.InventoryReferences.Length)

let demandReferencesMustBeUnique: Rule<ComposeInput> =
    Rule.create
        Rules.demandReferencesMustBeUnique.Id
        Rules.demandReferencesMustBeUnique.Explanation
        (fun input ->
            let distinctCount = input.Cmd.DemandReferences |> List.distinct |> List.length
            distinctCount = input.Cmd.DemandReferences.Length)
        (fun input ->
            sprintf
                "Total: %d, Distinct: %d"
                input.Cmd.DemandReferences.Length
                (input.Cmd.DemandReferences |> List.distinct |> List.length))

let supplyReferencesMustBeUnique: Rule<ComposeInput> =
    Rule.create
        Rules.supplyReferencesMustBeUnique.Id
        Rules.supplyReferencesMustBeUnique.Explanation
        (fun input ->
            let distinctCount = input.Cmd.SupplyReferences |> List.distinct |> List.length
            distinctCount = input.Cmd.SupplyReferences.Length)
        (fun input ->
            sprintf
                "Total: %d, Distinct: %d"
                input.Cmd.SupplyReferences.Length
                (input.Cmd.SupplyReferences |> List.distinct |> List.length))

let inventoryReferencesMustBeUnique: Rule<ComposeInput> =
    Rule.create
        Rules.inventoryReferencesMustBeUnique.Id
        Rules.inventoryReferencesMustBeUnique.Explanation
        (fun input ->
            let distinctCount = input.Cmd.InventoryReferences |> List.distinct |> List.length
            distinctCount = input.Cmd.InventoryReferences.Length)
        (fun input ->
            sprintf
                "Total: %d, Distinct: %d"
                input.Cmd.InventoryReferences.Length
                (input.Cmd.InventoryReferences |> List.distinct |> List.length))

let versionMustExist: Rule<PublishInput> =
    Rule.create
        Rules.versionMustExist.Id
        Rules.versionMustExist.Explanation
        (fun input ->
            match input.CurrentState with
            | Some picture -> picture.Versions |> List.exists(fun v -> v.VersionNumber = input.Cmd.VersionNumber)
            | None -> false)
        (fun input -> sprintf "VersionNumber: %A" input.Cmd.VersionNumber)

let onlyDraftVersionsCanBePublished: Rule<PublishInput> =
    Rule.create
        Rules.onlyDraftVersionsCanBePublished.Id
        Rules.onlyDraftVersionsCanBePublished.Explanation
        (fun input ->
            match input.CurrentState with
            | Some picture ->
                picture.Versions
                |> List.tryFind(fun v -> v.VersionNumber = input.Cmd.VersionNumber)
                |> Option.map(fun v -> v.LifecycleState = PictureVersionLifecycleState.Draft)
                |> Option.defaultValue false
            | None -> false)
        (fun input -> sprintf "VersionNumber: %A" input.Cmd.VersionNumber)

let compositionRules: Rule<ComposeInput> list =
    [ compositionRequiresReferences
      demandReferencesMustBeUnique
      supplyReferencesMustBeUnique
      inventoryReferencesMustBeUnique ]

let publicationRules: Rule<PublishInput> list = [ versionMustExist; onlyDraftVersionsCanBePublished ]
