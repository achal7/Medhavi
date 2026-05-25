namespace Medhavi.Hub

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

module Program =
    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)
        builder.Services.AddControllers() |> ignore
        
        let app = builder.Build()
        
        if app.Environment.IsDevelopment() then
            app.UseDeveloperExceptionPage() |> ignore
            
        app.UseRouting() |> ignore
        app.MapGet("/", Func<string>(fun () -> "Medhāvī Event Hub Gateway Running")) |> ignore
        
        app.Run()
        0
