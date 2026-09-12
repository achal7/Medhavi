namespace Medhavi.Client.ViewComposition.DisplayRules

open System
open Medhavi.Client.ViewComposition.Data

/// Conditions that trigger special display
type DisplayCondition =
    | Always
    | WhenNull
    | WhenMinValue
    | WhenMaxValue
    | WhenEquals of CellValue
    | WhenGreaterThan of CellValue
    | WhenLessThan of CellValue
    | WhenEmpty

/// The action to take when condition matches
type DisplayAction =
    | ShowText of string
    | ShowEmpty
    | ApplyCssClass of string

/// A complete display rule
type DisplayRule =
    { Id: string
      Condition: DisplayCondition
      Action: DisplayAction
      Priority: int }

/// Rule engine that evaluates rules against cell values
module DisplayRuleEngine =
    let matchesCondition (condition: DisplayCondition) (cell: CellValue) : bool =
        match condition, cell with
        | Always, _ -> true
        | WhenNull, EmptyValue -> true
        | WhenEmpty, EmptyValue -> true
        | WhenMinValue, DateTimeValue d -> d = DateTimeOffset.MinValue
        | WhenMaxValue, DateTimeValue d -> d = DateTimeOffset.MaxValue
        | WhenMinValue, NumberValue n -> n = Decimal.MinValue
        | WhenMaxValue, NumberValue n -> n = Decimal.MaxValue
        | WhenEquals target, cell -> cell = target
        | WhenGreaterThan target, NumberValue n ->
            match target with
            | NumberValue t -> n > t
            | _ -> false
        | WhenLessThan target, NumberValue n ->
            match target with
            | NumberValue t -> n < t
            | _ -> false
        | _ -> false

    let findMatchingRule (rules: DisplayRule list) (cell: CellValue) : DisplayRule option =
        rules
        |> List.filter(fun rule -> matchesCondition rule.Condition cell)
        |> List.sortByDescending(fun rule -> rule.Priority)
        |> List.tryHead

    let applyAction (action: DisplayAction) (defaultText: string) : string * string option =
        match action with
        | ShowText text -> text, None
        | ShowEmpty -> "", None
        | ApplyCssClass cls -> defaultText, Some cls

open Medhavi.Client.ViewComposition.Schema

module CommonRules =
    let dateColumnRules: DisplayRule list =
        [ { Id = "min-date"
            Condition = WhenMinValue
            Action = ShowText "-"
            Priority = 100 }
          { Id = "max-date"
            Condition = WhenMaxValue
            Action = ShowText "N.A."
            Priority = 100 }
          { Id = "null-date"
            Condition = WhenNull
            Action = ShowText "-"
            Priority = 90 } ]

    let quantityColumnRules: DisplayRule list =
        [ { Id = "negative-qty"
            Condition = WhenLessThan(NumberValue 0m)
            Action = ApplyCssClass "text-danger"
            Priority = 50 }
          { Id = "zero-qty"
            Condition = WhenEquals(NumberValue 0m)
            Action = ShowText "-"
            Priority = 40 } ]

    let defaultRulesForCellType (cellType: CellType) : DisplayRule list =
        match cellType with
        | DateTimeType -> dateColumnRules
        | NumberType -> quantityColumnRules
        | _ -> []
