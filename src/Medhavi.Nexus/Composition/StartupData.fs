module Medhavi.Nexus.Composition.StartupData

open System
open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Core
open Medhavi.Contracts.Core.Demand
open Medhavi.Contracts.Core.Exception
open Medhavi.Contracts.Core.ItemTransition
open Medhavi.Contracts.Demand
open Medhavi.Contracts.Supply
open Medhavi.Nexus.Composition.ComposeNexus

type EnterprisePictureSeed =
    { PlanningScopeId: string
      DemandReferences: string list
      SupplyReferences: string list
      InventoryReferences: string list
      CompositionTime: DateTimeOffset
      Publish: bool
      VersionNumber: int
      PublicationTime: DateTimeOffset }

type CoreSeedDocument =
    { Demands: RecordDemandReq list
      Exceptions: ExceptionEvidenceReq list
      ItemTransitions: RecognizeItemTransitionReq list
      EnterprisePictures: EnterprisePictureSeed list }

type UnderstandingSeed =
    { PlanningScopeId: string
      EvidencePictureVersion: int64
      Publish: bool
      IsPeriodicRefresh: bool }

type DemandSeedDocument =
    { Observations: ReceiveObservationReq list
      Understanding: UnderstandingSeed list }

type SupplySeedDocument =
    { Records: ReceiveSupplyDataReq list
      Understanding: UnderstandingSeed list }

let private jsonOptions =
    let opt = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
    opt.Converters.Add(JsonFSharpConverter())
    opt

let private resolveDataFilePath (filename: string) : string =
    let candidates =
        [ Path.Combine(AppContext.BaseDirectory, "Data", filename)
          Path.Combine(AppContext.BaseDirectory, filename)
          Path.Combine(Directory.GetCurrentDirectory(), "Data", filename)
          Path.Combine(Directory.GetCurrentDirectory(), "src", "Medhavi.Nexus", "Data", filename)
          Path.Combine(@"d:\RnD\Medhavi\src\Medhavi.Nexus\Data", filename) ]

    candidates
    |> List.tryFind File.Exists
    |> Option.defaultValue (Path.Combine(AppContext.BaseDirectory, "Data", filename))

let private loadJson<'T> (filename: string) : 'T option =
    try
        let filePath = resolveDataFilePath filename
        if File.Exists filePath then
            let json = File.ReadAllText filePath
            let doc = JsonSerializer.Deserialize<'T>(json, jsonOptions)
            Some doc
        else
            printfn $"[StartupData] [WARN] Seed data file not found: {filePath}"
            None
    with ex ->
        printfn $"[StartupData] [ERROR] Failed to read or parse {filename}: {ex.Message}"
        None

let private seedCoreData
    (demandApi: DemandApi)
    (exceptionApi: ExceptionApi)
    (transitionApi: ItemTransitionApi)
    (epicApi: EnterprisePictureApi)
    (doc: CoreSeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Core Demands..."
        for req in doc.Demands do
            let! res = demandApi.Record req
            match res with
            | Ok dto -> printfn $"  [OK] Demand recorded: {dto.Id} ({dto.Item}, Qty: {dto.Quantity})"
            | Error err -> printfn $"  [FAIL] Demand record error for {req.DemandId}: {err.Message}"

        printfn "[StartupData] Seeding Core Exceptions..."
        for req in doc.Exceptions do
            let! res = exceptionApi.ProcessEvidence req
            match res with
            | Ok dto -> printfn $"  [OK] Exception registered: {dto.ExceptionId} ({dto.Classification}, Scope: {dto.AffectedScopeIdentifier})"
            | Error err -> printfn $"  [FAIL] Exception evidence error for {req.ConstraintReference}: {err.Message}"

        printfn "[StartupData] Seeding Core Item Transitions..."
        for req in doc.ItemTransitions do
            let! res = transitionApi.Recognize req
            match res with
            | Ok dto -> printfn $"  [OK] Item transition recognized: {dto.TransitionId} ({dto.SupersededItem} -> {dto.SupersedingItem})"
            | Error err -> printfn $"  [FAIL] Item transition error for {req.SupersededItem}: {err.Message}"

        printfn "[StartupData] Seeding Core Enterprise Pictures..."
        for pic in doc.EnterprisePictures do
            let composeReq: ComposePictureVersionReq =
                { PlanningScopeId = pic.PlanningScopeId
                  DemandReferences = pic.DemandReferences
                  SupplyReferences = pic.SupplyReferences
                  InventoryReferences = pic.InventoryReferences
                  CompositionTime = pic.CompositionTime }

            let! composeRes = epicApi.Compose composeReq
            match composeRes with
            | Ok epDto ->
                printfn $"  [OK] Enterprise Picture composed: {epDto.PlanningScopeId} (Versions: {epDto.Versions.Length})"
                if pic.Publish then
                    do! Task.Delay 300
                    let publishReq: PublishPictureVersionReq =
                        { PlanningScopeId = pic.PlanningScopeId
                          VersionNumber = pic.VersionNumber
                          PublicationTime = pic.PublicationTime }
                    let! pubRes = epicApi.Publish publishReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Enterprise Picture published: {pubDto.PlanningScopeId} (Version: {pic.VersionNumber})"
                    | Error err ->
                        printfn $"  [FAIL] Enterprise Picture publish error for {pic.PlanningScopeId}: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Enterprise Picture compose error for {pic.PlanningScopeId}: {err.Message}"
    }

let private seedDemandData
    (obsApi: DemandObservationApi)
    (undApi: DemandUnderstandingApi)
    (doc: DemandSeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Demand Observations..."
        for req in doc.Observations do
            let! res = obsApi.Receive req
            match res with
            | Ok dto -> printfn $"  [OK] Observation received: {dto.ObservationId} ({dto.Item}, Qty: {dto.Quantity} {dto.UnitOfMeasure})"
            | Error err -> printfn $"  [FAIL] Observation receive error for {req.ObservationId}: {err.Message}"

        printfn "[StartupData] Seeding Demand Understanding..."
        for und in doc.Understanding do
            let revReq: ReviseDemandUnderstandingReq =
                { PlanningScopeId = und.PlanningScopeId
                  EvidencePictureVersion = und.EvidencePictureVersion }
            let! revRes = undApi.Revise revReq
            match revRes with
            | Ok dto ->
                printfn $"  [OK] Demand Understanding revised: {dto.PlanningScopeId} (Version: {dto.VersionNumber}, State: {dto.State})"
                if und.Publish then
                    do! Task.Delay 500
                    let pubReq: PublishDemandUnderstandingReq =
                        { PlanningScopeId = und.PlanningScopeId
                          IsPeriodicRefresh = und.IsPeriodicRefresh }
                    let! pubRes = undApi.Publish pubReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Demand Understanding published: {pubDto.PlanningScopeId} (State: {pubDto.State})"
                    | Error err ->
                        printfn $"  [FAIL] Demand Understanding publish error: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Demand Understanding revise error: {err.Message}"
    }

let private seedSupplyData
    (recApi: SupplyDataRecordApi)
    (undApi: SupplyUnderstandingApi)
    (doc: SupplySeedDocument)
    : Task<unit> =
    task {
        printfn "[StartupData] Seeding Supply Data Records..."
        for req in doc.Records do
            let! res = recApi.Receive req
            match res with
            | Ok dto -> printfn $"  [OK] Supply record received: {dto.RecordId} ({dto.Item} @ {dto.Location}, Qty: {dto.Quantity} {dto.UnitOfMeasure}, Type: {dto.SupplyType})"
            | Error err -> printfn $"  [FAIL] Supply record receive error for {req.RecordId}: {err.Message}"

        printfn "[StartupData] Seeding Supply Understanding..."
        for und in doc.Understanding do
            let revReq: ReviseSupplyUnderstandingReq =
                { PlanningScopeId = und.PlanningScopeId
                  EvidencePictureVersion = und.EvidencePictureVersion }
            let! revRes = undApi.Revise revReq
            match revRes with
            | Ok dto ->
                printfn $"  [OK] Supply Understanding revised: {dto.PlanningScopeId} (Version: {dto.VersionNumber}, State: {dto.State})"
                if und.Publish then
                    do! Task.Delay 500
                    let pubReq: PublishSupplyUnderstandingReq =
                        { PlanningScopeId = und.PlanningScopeId
                          IsPeriodicRefresh = und.IsPeriodicRefresh }
                    let! pubRes = undApi.Publish pubReq
                    match pubRes with
                    | Ok pubDto ->
                        printfn $"  [OK] Supply Understanding published: {pubDto.PlanningScopeId} (State: {pubDto.State})"
                    | Error err ->
                        printfn $"  [FAIL] Supply Understanding publish error: {err.Message}"
            | Error err ->
                printfn $"  [FAIL] Supply Understanding revise error: {err.Message}"
    }

let seed (nexusCtx: NexusContext) : Task<unit> =
    task {
        printfn "========================================================="
        printfn "  Medhavi.Nexus Startup Data Seeding (JSON-Driven)"
        printfn "========================================================="

        match loadJson<CoreSeedDocument> "CoreSeedData.json" with
        | Some coreDoc ->
            do! seedCoreData
                    nexusCtx.Core.DemandManagement.Commands
                    nexusCtx.Core.ExceptionManagement.Commands
                    nexusCtx.Core.ItemTransitionManagement.Commands
                    nexusCtx.Core.EnterprisePictureManagement.Commands
                    coreDoc
        | None ->
            printfn "[StartupData] Skipping Core seeding due to missing or invalid CoreSeedData.json"

        // Allow projection agents to process mailboxes from Core events
        do! Task.Delay 600

        match loadJson<DemandSeedDocument> "DemandSeedData.json" with
        | Some demandDoc ->
            do! seedDemandData
                    nexusCtx.Demand.UnderstandDemand.Commands.Observations
                    nexusCtx.Demand.UnderstandDemand.Commands.Understanding
                    demandDoc
        | None ->
            printfn "[StartupData] Skipping Demand seeding due to missing or invalid DemandSeedData.json"

        // Allow projection agents to process mailboxes from Demand events
        do! Task.Delay 600

        match loadJson<SupplySeedDocument> "SupplySeedData.json" with
        | Some supplyDoc ->
            do! seedSupplyData
                    nexusCtx.Supply.UnderstandSupply.Commands.Records
                    nexusCtx.Supply.UnderstandSupply.Commands.Understanding
                    supplyDoc
        | None ->
            printfn "[StartupData] Skipping Supply seeding due to missing or invalid SupplySeedData.json"

        // Final drain of projection mailboxes
        do! Task.Delay 500
        printfn "[StartupData] Seeding completed. All projection mailboxes processed."
        printfn "========================================================="
    }
