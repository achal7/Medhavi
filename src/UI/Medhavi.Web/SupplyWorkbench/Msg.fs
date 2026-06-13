namespace Medhavi.Web.SupplyWorkbench

open Medhavi.Web.Panels
open Medhavi.Contracts.Supply

type Msg =
    | LoadSummary
    | LoadedSummary of SupplyOrder list
    | LoadFailed of string
    | SearchTextChanged of string
    | TriggerSearch of string
    | RowSelected of SupplyOrder
    | DetailsLoaded of string
    | CloseDetails
