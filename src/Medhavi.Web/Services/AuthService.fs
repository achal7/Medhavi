namespace Medhavi.Web.Services

open System
open Medhavi.Web
open Medhavi.Contracts

type AuthService() =
    interface SystemShell.IAuthApplicationService with
        member _.Authenticate (userName: string) (pwd: string) =
            async {
                do! Async.Sleep 500
                let normalizedName = if isNull userName then "" else userName.Trim()

                if String.Equals(normalizedName, "admin", StringComparison.OrdinalIgnoreCase) then
                    return
                        Ok
                            { Name = normalizedName
                              Role = Role.Administrator }
                else
                    return Error "Invalid User name/password"
            }
