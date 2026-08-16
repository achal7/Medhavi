namespace Medhavi.Core

open Medhavi.Foundation.Failure
open Medhavi.Contracts

[<AutoOpen>]
module Helpers =

    let mapAppErrorToApiError (appError: ApplicationError) : ApiError =
        match appError with
        | Domain d -> ApiError.validation d.Message
        | Validation errs -> ApiError.validation(String.concat "; " (errs |> List.map snd))
        | Infrastructure i -> ApiError.infrastructureError i.Message
