open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Bolero.Server
open Medhavi.Web
open Radzen

[<EntryPoint>]
let main args =
    let builder = WebApplication.CreateBuilder(args)
    builder.WebHost.UseStaticWebAssets() |> ignore

    builder.Services.AddRazorComponents().AddInteractiveServerComponents() |> ignore

    builder.Services.AddServerSideBlazor() |> ignore
    builder.Services.AddRadzenComponents() |> ignore

    builder.Services.AddAuthorization().AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie()
    |> ignore

    let nexusBootstrap = Medhavi.Nexus.Bootstrap.start() |> Async.AwaitTask |> Async.RunSynchronously

    match nexusBootstrap with
    | Error e ->
        printfn $"Error while starting Nexus... {e.ToString()}"
        1
    | Ok nexus ->
        builder.Services.AddSingleton<Medhavi.Nexus.NexusService>(nexus) |> ignore
        builder.Services.AddMedhaviWebServices nexus |> ignore
        builder.Services.AddBoleroComponents() |> ignore

        // Configure fast shutdown for local development
        builder.Services.Configure<HostOptions>(fun (options: HostOptions) ->
            options.ShutdownTimeout <- System.TimeSpan.FromSeconds(2.0))
        |> ignore

        let app = builder.Build()

        app.MapStaticAssets() |> ignore

        app.UseAuthentication().UseStaticFiles().UseRouting().UseAuthorization().UseAntiforgery() |> ignore

        app.MapRazorComponents<SystemShell.Root>().AddInteractiveServerRenderMode() |> ignore

        app.Run()
        0
