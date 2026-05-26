module Medhavi.SharedKernel.Validations

open System
open Medhavi.Common.Validation
open Medhavi.SharedKernel

/// Reusable validation helpers for command/input checks
let required (field: string) (value: string) =
    if String.IsNullOrWhiteSpace value then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} is required", Map.empty) ])
    else
        Valid(value.Trim())

let nonNegativeDecimal (field: string) (value: decimal) =
    if value < 0m then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must be >= 0", Map.empty) ])
    else
        Valid value

let positiveDecimal (field: string) (value: decimal) =
    if value <= 0m then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must be > 0", Map.empty) ])
    else
        Valid value

let positiveFloat (field: string) (value: float) =
    if value <= 0.0 then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must be > 0", Map.empty) ])
    else
        Valid value

let inline greaterThan field threshold value =
    if value > threshold then
        Valid value
    else
        Invalid [ DomainError(ErrorCodes.ValidationFailed, $"{field} must be > {threshold}", Map.empty) ]

let inline positive field value = greaterThan field LanguagePrimitives.GenericZero value

let inRange (field: string) (minValue: decimal) (maxValue: decimal) (value: decimal) =
    if value < minValue || value > maxValue then
        Invalid(
            [ DomainError(ErrorCodes.ValidationFailed, $"{field} must be between {minValue} and {maxValue}", Map.empty) ]
        )
    else
        Valid value

let dateNotPast (field: string) (now: DateTimeOffset) (value: DateTimeOffset) =
    if value < now then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must not be in the past", Map.empty) ])
    else
        Valid value

let dateRange (startField: string) (endField: string) (startDate: DateTimeOffset) (endDate: DateTimeOffset) =
    if endDate < startDate then
        Invalid(
            [ DomainError(
                  ErrorCodes.ValidationFailed,
                  $"{endField} must be greater than or equal to {startField}",
                  Map.empty
              ) ]
        )
    else
        Valid(startDate, endDate)

let nonNegativeInt (field: string) (value: int) =
    if value < 0 then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must be >= 0", Map.empty) ])
    else
        Valid value

let positiveInt (field: string) (value: int) =
    if value <= 0 then
        Invalid([ DomainError(ErrorCodes.ValidationFailed, $"{field} must be > 0", Map.empty) ])
    else
        Valid value
