module Medhavi.SharedKernel.Validations

open System
open Medhavi.Common.Validation
open Medhavi.SharedKernel.Failure

/// Reusable validation helpers for command/input checks
let required (field: string) (value: string) =
    if String.IsNullOrWhiteSpace value then
        Invalid([ DomainError.validation $"{field} is required" ])
    else
        Valid(value.Trim())

let nonNegativeDecimal (field: string) (value: decimal) =
    if value < 0m then
        Invalid([ DomainError.validation $"{field} must be >= 0" ])
    else
        Valid value

let positiveDecimal (field: string) (value: decimal) =
    if value <= 0m then
        Invalid([ DomainError.validation $"{field} must be > 0" ])
    else
        Valid value

let positiveFloat (field: string) (value: float) =
    if value <= 0.0 then
        Invalid([ DomainError.validation $"{field} must be > 0" ])
    else
        Valid value

let inline greaterThan field threshold value =
    if value > threshold then
        Valid value
    else
        Invalid [ DomainError.validation $"{field} must be > {threshold}" ]

let inline positive field value = greaterThan field LanguagePrimitives.GenericZero value

let inRange (field: string) (minValue: decimal) (maxValue: decimal) (value: decimal) =
    if value < minValue || value > maxValue then
        Invalid([ DomainError.validation $"{field} must be between {minValue} and {maxValue}" ])
    else
        Valid value

let dateNotPast (field: string) (now: DateTimeOffset) (value: DateTimeOffset) =
    if value < now then
        Invalid([ DomainError.validation $"{field} must not be in the past" ])
    else
        Valid value

let dateRange (startField: string) (endField: string) (startDate: DateTimeOffset) (endDate: DateTimeOffset) =
    if endDate < startDate then
        Invalid([ DomainError.validation "${endField} must be greater than or equal to ${startField}" ])
    else
        Valid(startDate, endDate)

let nonNegativeInt (field: string) (value: int) =
    if value < 0 then
        Invalid([ DomainError.validation $"{field} must be >= 0" ])
    else
        Valid value

let positiveInt (field: string) (value: int) =
    if value <= 0 then
        Invalid([ DomainError.validation $"{field} must be > 0" ])
    else
        Valid value
