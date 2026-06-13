namespace Medhavi.Web.DemandWorkbench

open Medhavi.Web.Panels
open Medhavi.Contracts.Demand

type Msg =
    | LoadSummary
    | LoadedSummary of DemandLine list
    | LoadFailed of string
    | SearchTextChanged of string
    | TriggerSearch of string
    | RowSelected of DemandLine
    | DetailsLoaded of string
    | CloseDetails
