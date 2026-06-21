namespace Medhavi.Web.Stores

open System
open Medhavi.Common.Patterns
open Medhavi.Contracts.Scenario

type Freshness =
    | Fresh
    | Stale
    | Loading
    | Failed of string

type Workspace =
    | MaterialReservation
    | ResourceScheduling
    | ScenarioManagement
    | MasterData

type WorkspaceAction =
    | NavigateTo of Workspace
    | RefreshActiveWorkspace
    | RefreshAllWorkspaces
    | ApplyContext of PlanningContext
    | OpenCopilot
    | ShowWorkspaceEvents
    | Help

type WorkspaceSnapshot<'data> =
    { Data: 'data option
      Freshness: Freshness
      Version: int64
      LastRefreshUtc: DateTime option
      Error: string option }

    static member Default() =
        { Data = None
          Freshness = Stale
          Version = 0L
          LastRefreshUtc = None
          Error = None }

type StoreEvent<'TState> =
    | StateChanged of 'TState
    | ContextChanged of PlanningContext
    | ErrorOccurred of string

type WorkspaceKind =
    | DemandWorkspace
    | SupplyWorkspace
    | MaterialReservationWorkspace
    | CapacityWorkspace
    | ScenarioWorkspace
    | PromiseWorkspace
    | AnalyticsWorkspace
    | KnowledgeWorkspace
    | AiWorkspace
    | InvestigationWorkspace

type SubscriptionId = SubscriptionId of Guid

// =============================================================================
// Pure Functional Workspace Store Definition (Read-Only, Refresh-Only)
// =============================================================================
//
// STORE PHILOSOPHY:
// - Stores are READ MODELS ONLY - no direct mutation!
// - AI/UI send Workspace Actions → Services → Backend → Store refreshes
// - Stores hold cached snapshots + manage subscriptions + freshness
//
// =============================================================================

type WorkspaceStore<'TState> =
    {
        /// Get the current snapshot
        Get: unit -> WorkspaceSnapshot<'TState>

        /// Refresh from backend using current/planning context
        Refresh: PlanningContext -> TaskResult<WorkspaceSnapshot<'TState>, string>

        /// Mark as stale (e.g., when context changes or backend event received)
        MarkStale: unit -> unit

        /// Subscribe to store events
        Subscribe: (StoreEvent<WorkspaceSnapshot<'TState>> -> unit) -> SubscriptionId

        /// Unsubscribe from store events
        Unsubscribe: SubscriptionId -> unit

        /// Clear the store (reset to default)
        Clear: unit -> unit
    }

open Medhavi.Contracts.MasterData

type MasterDataService =
    { UomQueryService: Uom.UomQueryService
      PlantQueryService: Network.PlantQueryService
      StockingPointQueryService: Network.StockingPointQueryService
      SkuQueryService: Sku.SkuQueryService
      BomQueryService: Bom.BomQueryService
      RoutingQueryService: Routing.RoutingQueryService
      TransportLegQueryService: Transport.TransportLegQueryService }
