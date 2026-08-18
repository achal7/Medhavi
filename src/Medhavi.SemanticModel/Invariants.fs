module Medhavi.SemanticModel.Invariants

open System

let createStringId (ctor: string -> 'Id) (fieldName: string) (value: string) : Result<'Id, SemanticValidationError> =
    if System.String.IsNullOrWhiteSpace value then
        Error(EmptyIdentifier fieldName)
    else
        Ok(ctor value)

let firstError (checks: Result<unit, SemanticValidationError> list) : Result<unit, SemanticValidationError> =
    checks
    |> List.tryPick (function
        | Error e -> Some e
        | Ok() -> None)
    |> function
        | Some e -> Error e
        | None -> Ok()

let nonEmptyIdentifier (fieldName: string) (value: string) : Result<unit, SemanticValidationError> =
    if String.IsNullOrWhiteSpace value then
        Error(EmptyIdentifier fieldName)
    else
        Ok()

let nonEmptyField (objectName: string) (fieldName: string) (value: string) : Result<unit, SemanticValidationError> =
    if String.IsNullOrWhiteSpace value then
        Error(EmptyRequiredField(objectName, fieldName))
    else
        Ok()

let noEmptyStrings
    (objectName: string)
    (fieldName: string)
    (values: string list)
    : Result<unit, SemanticValidationError> =
    if values |> List.exists String.IsNullOrWhiteSpace then
        Error(InvariantViolation(objectName, sprintf "%s must not contain empty values." fieldName))
    else
        Ok()

let nonNegativeInt (objectName: string) (fieldName: string) (value: int) : Result<unit, SemanticValidationError> =
    if value < 0 then
        Error(InvariantViolation(objectName, sprintf "%s must be non-negative." fieldName))
    else
        Ok()

let hasDuplicatesBy (projection: 'a -> 'b) (items: 'a list) : bool =
    let projected = items |> List.map projection
    projected.Length <> (projected |> List.distinct |> List.length)
