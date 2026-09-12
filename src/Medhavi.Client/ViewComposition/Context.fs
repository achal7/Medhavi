namespace Medhavi.Client.ViewComposition.Context

open Medhavi.Client.Primitives
open Medhavi.Client.ViewComposition.Identity

// DateRange is now in Primitives; no need to redefine.

type FilterOperator =
    | Equals
    | NotEquals
    | GreaterThan
    | GreaterThanOrEqual
    | LessThan
    | LessThanOrEqual
    | Contains
    | IsEmpty
    | IsNotEmpty

type FilterCriterion =
    { Field: string
      Operator: FilterOperator
      Value: string option }

type SortDirection =
    | Ascending
    | Descending

type SortCriterion =
    { Field: string
      Direction: SortDirection
      Priority: int }

type FilterValueKind =
    | Text
    | DateTimeOffset
    | Number
    | Boolean
    | Enum of string list

type SharedContext =
    { Filters: FilterCriterion list }
