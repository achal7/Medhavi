namespace Medhavi.Web.Services

open System
open Medhavi.Web
open Medhavi.Web.WorkspaceEngine
open Medhavi.Nexus
open Medhavi.Contracts.Domain

type GlobalSearchResult =
    | WorkbenchResult of WorkspaceKind * string      // kind and title
    | EntityResult of EntityRef * string              // entity reference and display name
    | CapabilityResult of string * string             // capability id and name

type GlobalSearchQuery =
    { SearchText: string
      MaxResults: int
      Context: WorkspaceContext option }

type GlobalSearchService =
    { SearchAsync: GlobalSearchQuery -> Async<GlobalSearchResult list> }

module GlobalSearchService =
    let create (engine: MedhaviEngine) : GlobalSearchService =
        let workbenchEntries = 
            [ WorkspaceKind.DemandWorkspace, "Demand Workbench"
              WorkspaceKind.SupplyWorkspace, "Supply Workbench"
              WorkspaceKind.CapacityWorkspace, "Capacity Workbench"
              WorkspaceKind.ScenarioWorkspace, "Scenario Workbench" ]

        let search (query: GlobalSearchQuery) =
            async {
                if String.IsNullOrWhiteSpace(query.SearchText) then
                    return []
                else
                    let term = query.SearchText.ToLower()
                    
                    let workbenchResults = 
                        workbenchEntries
                        |> List.filter (fun (_, title) -> title.ToLower().Contains(term))
                        |> List.map (fun (kind, title) -> WorkbenchResult (kind, title))
                        
                    // Query live projections via clean Nexus facade queries
                    let! skus = engine.GetSkus() |> Async.AwaitTask
                    let! plants = engine.GetPlants() |> Async.AwaitTask
                    let! sps = engine.GetStockingPoints() |> Async.AwaitTask
                    let! stdResources = engine.GetResources() |> Async.AwaitTask
                    let! demands = engine.GetDemands() |> Async.AwaitTask
                    let! supplyOrders = engine.GetSupplyOrders() |> Async.AwaitTask

                    let skuResults =
                        skus
                        |> List.filter (fun s -> s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term) || s.Id.ToLower().Contains(term))
                        |> List.map (fun s -> EntityResult (EntityRef ("Sku", s.Id), sprintf "%s (%s) (Product)" s.Name s.Code))

                    let plantResults =
                        plants
                        |> List.filter (fun p -> p.Name.ToLower().Contains(term) || p.Code.ToLower().Contains(term) || p.Id.ToLower().Contains(term))
                        |> List.map (fun p -> EntityResult (EntityRef ("Plant", p.Id), sprintf "%s (%s) (Location)" p.Name p.Code))

                    let spResults =
                        sps
                        |> List.filter (fun sp -> sp.Name.ToLower().Contains(term) || sp.Code.ToLower().Contains(term) || sp.Id.ToLower().Contains(term))
                        |> List.map (fun sp -> EntityResult (EntityRef ("StockingPoint", sp.Id), sprintf "%s (%s) (Stocking Point)" sp.Name sp.Code))

                    let resourceResults =
                        stdResources
                        |> List.filter (fun r -> r.Name.ToLower().Contains(term) || r.Id.ToLower().Contains(term))
                        |> List.map (fun r -> EntityResult (EntityRef ("Resource", r.Id), sprintf "%s (Resource)" r.Name))

                    let demandResults =
                        demands
                        |> List.filter (fun d -> d.DemandOrderId.ToLower().Contains(term) || d.DemandLineId.ToLower().Contains(term) || d.SkuId.ToLower().Contains(term))
                        |> List.map (fun d -> 
                            EntityResult (EntityRef ("DemandLine", d.DemandLineId), sprintf "Demand Line: %s / %s (Product: %s, Qty: %M)" d.DemandOrderId d.DemandLineId d.SkuId d.RequestedQty))

                    let supplyResults =
                        supplyOrders
                        |> List.filter (fun o -> o.Id.ToLower().Contains(term) || o.SkuId.ToLower().Contains(term))
                        |> List.map (fun o -> 
                            EntityResult (EntityRef ("SupplyOrder", o.Id), sprintf "Supply Order: %s (Product: %s, Qty: %M)" o.Id o.SkuId o.Quantity))

                    let entityResults = skuResults @ plantResults @ spResults @ resourceResults @ demandResults @ supplyResults

                    return workbenchResults @ entityResults |> List.truncate query.MaxResults
            }

        { SearchAsync = search }
