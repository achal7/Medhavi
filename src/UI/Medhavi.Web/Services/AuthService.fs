namespace Medhavi.Web.Services

open System
open System.Net.Http
open System.Net.Http.Json
open System.Threading.Tasks
open Microsoft.AspNetCore.Components
open Medhavi.Web

type LoginRequest = {
    Username: string
    Password: string
}

type LoginResponse = {
    Username: string
    Email: string
    Role: string
}

type AuthService(nav: NavigationManager) =
    member _.Authenticate(username: string, password: string) : Task<Result<User, string>> =
        task {
            try
                use http = new HttpClient()
                http.BaseAddress <- Uri(nav.BaseUri)
                
                let req = { Username = username; Password = password }
                let! response = http.PostAsJsonAsync("/api/auth/login", req)
                
                if response.IsSuccessStatusCode then
                    let! res = response.Content.ReadFromJsonAsync<LoginResponse>()
                    let role = 
                        match res.Role with
                        | "Planner" -> Role.Planner
                        | "Supervisor" -> Role.Supervisor
                        | "Manager" -> Role.Manager
                        | "Administrator" -> Role.Administrator
                        | _ -> Role.Supervisor
                    let user : User = { Username = res.Username; Email = res.Email; Role = role }
                    return Ok user
                else
                    let! err = response.Content.ReadAsStringAsync()
                    return Error (if String.IsNullOrWhiteSpace(err) then "Authentication failed" else err)
            with ex ->
                return Error (sprintf "Authentication service offline: %s" ex.Message)
        }
