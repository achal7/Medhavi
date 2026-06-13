namespace Medhavi.Web.CapacityWorkbench

open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Contracts.Capacity

type Model =
    { WorkspaceId: WorkspaceId
      Context: WorkspaceContext
      SummaryData: RemoteData<OperationView list>
      PendingSearchText: string
      SearchText: string
      SelectedOperation: OperationView option
      IsLoadingDetails: bool
      DetailsText: string option }
