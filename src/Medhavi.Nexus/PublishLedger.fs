namespace Medhavi.Nexus

open System
open System.Collections.Concurrent
open Medhavi.SharedKernel.ScenarioContracts

type PublishLedger() =
    let publishRecords = ConcurrentDictionary<string, ScenarioPublishRecord>()
    let rollbackPackages = ConcurrentDictionary<string, RollbackPackage>()

    member _.SaveRecord(record: ScenarioPublishRecord) =
        publishRecords.TryAdd(record.PublishId, record) |> ignore

    member _.GetRecord(publishId: string) =
        match publishRecords.TryGetValue(publishId) with
        | true, record -> Some record
        | _ -> None

    member _.SavePackage(package: RollbackPackage) =
        rollbackPackages.TryAdd(package.PublishId, package) |> ignore

    member _.GetPackage(publishId: string) =
        match rollbackPackages.TryGetValue(publishId) with
        | true, package -> Some package
        | _ -> None
