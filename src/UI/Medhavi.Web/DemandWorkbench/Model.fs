namespace Medhavi.Web.DemandWorkbench

open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Contracts.Demand

type Model =
    { WorkspaceId: WorkspaceId
      Context: WorkspaceContext
      SummaryData: RemoteData<DemandLine list>
      PendingSearchText: string
      SearchText: string
      SelectedDemand: DemandLine option
      IsLoadingDetails: bool
      DetailsText: string option
      OverrideQtyInput: string
      OverrideReasonInput: string
      IsSubmittingOverride: bool
      OverrideError: string option }
