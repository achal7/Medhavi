namespace Medhavi.Web.Stores

open Medhavi.Common
open Medhavi.Contracts.Scenario
open Medhavi.Contracts.Demand
open Medhavi.Common.Patterns
open Medhavi.Web

type DemandData =
    { DemandsByLineId: Map<string, DemandLine> }

module DemandData =
    let ofList (demands: DemandLine list) =
        { DemandsByLineId = demands |> List.map(fun d -> d.DemandLineId, d) |> Map.ofList }

    let addOrUpdate (demand: DemandLine) (data: DemandData) =
        { data with
            DemandsByLineId = data.DemandsByLineId |> Map.add demand.DemandLineId demand }

    let remove (demandLineId: string) (data: DemandData) =
        { data with
            DemandsByLineId = data.DemandsByLineId |> Map.remove demandLineId }

    let toList (data: DemandData) = data.DemandsByLineId |> Map.values |> List.ofSeq

module DemandStore =
    /// Creates the demand store, returning the store and a set of handlers for projection updates
    let create (commandService: DemandLineApi) (queryService: DemandLineQueries) (initialContext: PlanningContext) =
        let loadFromBackend (context: PlanningContext) =
            taskResult {
                //let! result = demandService.GetAll context |> Async.AwaitTask
                let! demands = queryService.GetAll()
                let data = DemandData.ofList demands
                return data
            }

        let store, updateStore = WorkspaceStore.create loadFromBackend initialContext None

        let onDemandCreated demandLineId =
            taskResult {
                let! demandOpt = queryService.GetById(demandLineId)
                match demandOpt with
                | Some demand ->
                    updateStore(fun currentData ->
                        let newData =
                            match currentData with
                            | Some d -> DemandData.addOrUpdate demand d
                            | None -> DemandData.ofList [ demand ]

                        Some newData)
                | None -> printfn $"[DemandStore] Demand not found for loading demand {demandLineId}"
            }

        let onDemandUpdated demandLineId =
            taskResult {
                let! demandOpt = queryService.GetById(demandLineId)
                match demandOpt with
                | Some demand -> updateStore(fun currentData -> currentData |> Option.map(DemandData.addOrUpdate demand))
                | None -> printfn $"[DemandStore] Demand not found for loading updated demand {demandLineId}"
            }

        let onDemandDeleted demandLineId =
            updateStore(fun currentData -> currentData |> Option.map(DemandData.remove demandLineId))
            TaskResult.ofResult (Ok ())

        let handlers =
            { OnCreated = onDemandCreated
              OnUpdated = onDemandUpdated
              OnDeleted = onDemandDeleted }

        store, handlers
