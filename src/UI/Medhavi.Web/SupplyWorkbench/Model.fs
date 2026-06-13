namespace Medhavi.Web.SupplyWorkbench

open Medhavi.Web.WorkspaceEngine
open Medhavi.Web.Panels
open Medhavi.Contracts.Supply

type Model =
    { WorkspaceId: WorkspaceId
      Context: WorkspaceContext
      SummaryData: RemoteData<SupplyOrder list>
      PendingSearchText: string
      SearchText: string
      SelectedSupply: SupplyOrder option
      IsLoadingDetails: bool
      DetailsText: string option }
