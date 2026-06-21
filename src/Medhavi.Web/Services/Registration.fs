namespace Medhavi.Web

open Microsoft.Extensions.DependencyInjection
open Medhavi.Web.Services
open Medhavi.Contracts.Demand
open Medhavi.Contracts.MasterData
open Medhavi.Nexus

[<AutoOpen>]
module ServiceRegistration =
    open Medhavi.Web.Stores

    type IServiceCollection with
        member services.AddMedhaviWebServices(nexus: NexusService) =
            services.AddScoped<SystemShell.IAuthApplicationService, AuthService>() |> ignore

            // Use Medhavi.Nexus demand service
            services.AddScoped<DemandLineQueries>(fun _ -> nexus.DemandService.Context.Queries) |> ignore
            services.AddScoped<DemandLineApi>(fun _ -> nexus.DemandService.Context.Commands) |> ignore

            // Add Master Data Services
            let mdContext = nexus.MasterDataService.Context
            services.AddScoped<Uom.UomQueryService>(fun _ -> mdContext.Queries.Uom) |> ignore
            services.AddScoped<Network.PlantQueryService>(fun _ -> mdContext.Queries.Plant) |> ignore
            services.AddScoped<Network.StockingPointQueryService>(fun _ -> mdContext.Queries.StockingPoint) |> ignore

            services.AddScoped<MasterDataService>(fun _ ->
                { PlantQueryService = mdContext.Queries.Plant
                  StockingPointQueryService = mdContext.Queries.StockingPoint
                  UomQueryService = mdContext.Queries.Uom
                  SkuQueryService = mdContext.Queries.Sku
                  BomQueryService = mdContext.Queries.Bom
                  RoutingQueryService = mdContext.Queries.Routing
                  TransportLegQueryService = mdContext.Queries.TransportLeg })
            |> ignore

            services
