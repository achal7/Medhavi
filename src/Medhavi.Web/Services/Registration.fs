namespace Medhavi.Web

open Microsoft.Extensions.DependencyInjection
open Medhavi.Web.Services
open Medhavi.Contracts.Demand
open Medhavi.Nexus

[<AutoOpen>]
module ServiceRegistration =
    type IServiceCollection with
        member services.AddMedhaviWebServices(nexus: NexusService) =
            services.AddScoped<SystemShell.IAuthApplicationService, AuthService>() |> ignore

            // Use Medhavi.Nexus demand service
            services.AddScoped<DemandLineQueries>(fun _ -> nexus.DemandService.Context.Queries) |> ignore
            services.AddScoped<DemandLineApi>(fun _ -> nexus.DemandService.Context.Commands) |> ignore

            services
