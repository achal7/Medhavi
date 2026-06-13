namespace Medhavi.Web.CapacityWorkbench

open Medhavi.Web.Panels
open Medhavi.Contracts.Capacity

type Msg =
    | LoadSummary
    | LoadedSummary of OperationView list
    | LoadFailed of string
    | SearchTextChanged of string
    | TriggerSearch of string
    | RowSelected of OperationView
    | DetailsLoaded of string
    | CloseDetails
