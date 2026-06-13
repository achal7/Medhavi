module Medhavi.Web.Services.ServiceRegistration

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Medhavi.Web.Stores
open Medhavi.Web.Services
open Medhavi.Web.Services.PlanningService
open Medhavi.Nexus

let configureServices (builder: WebApplicationBuilder) =
    // Context & Command Services
    builder.Services.AddScoped<WorkspaceContextService>() |> ignore
    builder.Services.AddScoped<PlanningCommandService>() |> ignore
    builder.Services.AddScoped<AuthService>() |> ignore
    builder.Services.AddScoped<GlobalSearchService>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        GlobalSearchService.create eng) |> ignore

    // Query Stores
    builder.Services.AddScoped<DemandStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        DemandStore.create eng) |> ignore

    builder.Services.AddScoped<SupplyStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        SupplyStore.create eng) |> ignore

    builder.Services.AddScoped<CapacityStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        CapacityStore.create eng) |> ignore

    builder.Services.AddScoped<ScenarioStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        ScenarioStore.create eng) |> ignore

    builder.Services.AddScoped<ActivityStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        ActivityStore.create eng) |> ignore

    builder.Services.AddScoped<PromiseStore>(fun sp ->
        let eng = sp.GetRequiredService<MedhaviEngine>()
        PromiseStore.create eng) |> ignore
