# Medhavi APS - Store Architecture

## Core Principles (From Your Design)

### The Key Insight
> **Stores are not a replacement for Elmish. Stores complete Elmish.**

### Layer Boundaries
```
┌─────────────────────────────────────────────────────────────────┐
│  Backend / EventStore / Database                                │
│  = SOURCE OF TRUTH for business data                            │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Services                                                       │
│  = Backend API contracts + implementations                      │
│  = IDemandService, ISupplyService, etc.                         │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  STORES                                                         │
│  = SHARED READ MODELS + CACHE + SUBSCRIPTIONS                   │
│  = No direct mutation! Only refresh from backend or mark stale  │
│  = AI and UI both read from same stores                         │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Root Composition                                               │
│  = Creates services, stores, registry                           │
│  = No business logic, just wiring                               │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Workspaces (Elmish MVU)                                        │
│  = ORCHESTRATION LAYER                                          │
│  = Holds UI/interaction state only (selected rows, open panels) │  
│  = Calls services when actions happen                           │
│  = Refreshes stores when data changes                           │
│  = Subscribes to stores and updates model on changes            │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Panels                                                         │
│  = UI SLICES ONLY                                               │
│  = No service calls directly                                    │
│  = Emit INTENTS upward to workspace                             │
│  = Receive data from workspace model                            │
└──────────────────────┬──────────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────────┐
│  Components                                                     │
│  = PURE UI ONLY                                                 │
│  = No business knowledge                                        │
│  = Just render props                                            │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  AI Copilot                                                     │
│  = EMITS SAME WORKSPACE ACTIONS AS USER                         │
│  = Reads from stores, but never writes to them directly         │
└─────────────────────────────────────────────────────────────────┘
```

## Data Flow (One-Way Only!)

### User Edits a Demand
```
User interacts with Panel
  ↓
Panel emits local Msg
  ↓
Workspace converts Msg → Workspace Action
  ↓
Workspace calls Service.UpdateDemand(...)
  ↓
Service persists to Backend (source of truth!)
  ↓
Workspace calls Store.Refresh()
  ↓
Store loads new snapshot from Backend
  ↓
Store notifies all subscribers
  ↓
Workspace updates its Elmish model
  ↓
Panels re-render with new data
```

### AI Asks to "Explain Shortage"
```
AI sends Workspace Action (ExplainShortage demandId)
  ↓
(EXACT SAME PATH AS USER!)
  ↓
Workspace calls Service.AnalyzeShortage(...)
  ↓
Service persists/logs the analysis
  ↓
Workspace refreshes relevant stores
  ↓
Store notifies subscribers
  ↓
Panels re-render with explanation
```

## Store Types

### `WorkspaceStore<'TState>`
**What it does**:
- Holds a cached `WorkspaceSnapshot<'TState>`
- Manages freshness (Fresh/Stale/Loading/Failed)
- Notifies subscribers of changes
- Loads from backend on `Refresh()`

**What it DOES NOT do**:
- ❌ Accept commands directly
- ❌ Mutate business data
- ❌ Call services on its own (only loads)

### `PlanningContextStore`
**What it does**:
- Holds the current `PlanningContext`
- Notifies subscribers when context changes
- Auto-marks all stores as stale when context changes

## File Structure

```
Stores/
├── Types.fs                    # Core types (Freshness, WorkspaceSnapshot, etc.)
├── WorkspaceStore.fs          # Generic store factory
├── WorkspaceStoreRegistry.fs   # Registry + PlanningContextStore
├── DemandStore.fs             # Demand-specific store
├── SupplyStore.fs             # Supply-specific store
├── CapacityStore.fs           # Capacity-specific store
├── MaterialReservationStore.fs # Material Reservation-specific store
├── StoreComposition.fs        # Composition root
└── ARCHITECTURE.md            # This file!
```

## Key Rules to Remember

1. **Backend is ALWAYS Source of Truth**
   - Stores are just cached read models
   - Never treat store data as authoritative

2. **No Direct Store Mutation**
   - AI/UI send Workspace Actions
   - Actions go through Services → Backend
   - Then Stores refresh from Backend

3. **Elmish = UI State Only**
   - Selected rows, open panels, form inputs go here
   - Business data lives in Stores

4. **Same Actions for User + AI**
   - AI uses exactly the same Workspace Action types
   - No separate AI path

5. **Stores = Shared Read Models**
   - Multiple workspaces can subscribe to same store
   - One refresh updates all interested parties

## Example Usage in a Workspace

```fsharp
// In MaterialReservation.fs
type Model = {
    Context: PlanningContext
    StoreSnapshot: WorkspaceSnapshot<MaterialReservationData> option
    Subscriptions: SubscriptionId list
    SelectedDemandId: string option // UI state only!
}

type Msg =
    | Initialize
    | StoreUpdated of WorkspaceSnapshot<MaterialReservationData>
    | SelectDemand of string
    | RefreshRequested

let init (context: PlanningContext) (registry: WorkspaceStoreRegistry) =
    let store = WorkspaceStoreRegistry.getMaterialReservationStore registry

    let snapshot = store |> Option.map (fun s -> s.Get())

    let subId =
        store
        |> Option.map (fun s ->
            s.Subscribe(function
                | StateChanged newSnapshot ->
                    // In real app, dispatch StoreUpdated to Elmish
                    ()
                | _ -> ()))

    {
        Context = context
        StoreSnapshot = snapshot
        Subscriptions = subId |> Option.toList
        SelectedDemandId = None
    }, Cmd.none

let update (env: ReservationEnv) (msg: Msg) (model: Model) =
    match msg with
    | Initialize ->
        let store = WorkspaceStoreRegistry.getMaterialReservationStore env.StoreRegistry
        match store with
        | Some s ->
            let cmd =
                Cmd.OfAsync.either
                    (fun () -> s.Refresh model.Context)
                    ()
                    (function Ok snap -> StoreUpdated snap | Error _ -> StoreUpdated model.StoreSnapshot.Value)
                    (fun ex -> StoreUpdated model.StoreSnapshot.Value)
            model, cmd, None
        | None -> model, Cmd.none, None

    | StoreUpdated snapshot ->
        { model with StoreSnapshot = Some snapshot }, Cmd.none, None

    | SelectDemand demandId ->
        // Update UI state only!
        { model with SelectedDemandId = Some demandId }, Cmd.none, None

    | RefreshRequested ->
        // Tell store to refresh (will notify subscribers)
        let store = WorkspaceStoreRegistry.getMaterialReservationStore env.StoreRegistry
        match store with
        | Some s ->
            let cmd =
                Cmd.OfAsync.either
                    (fun () -> s.Refresh model.Context)
                    ()
                    (function Ok snap -> StoreUpdated snap | Error _ -> StoreUpdated model.StoreSnapshot.Value)
                    (fun ex -> StoreUpdated model.StoreSnapshot.Value)
            model, cmd, None
        | None -> model, Cmd.none, None
```

## Why This Works for AI

1. **AI Reads from Same Stores as UI**: AI has access to the same data view
2. **AI Emits Same Actions as User**: No special AI path to maintain
3. **AI Doesn't Need UI Knowledge**: Just works with Workspace Actions
4. **Traceability**: Same command history for both user and AI actions

## Why This Works for Scalability

1. **Independent Workspaces**: Add new workspaces without touching core
2. **Independent Stores**: Add new stores without breaking existing ones
3. **Shared Context**: One place to manage scenario/plant/horizon
4. **Fine-Grained Subscriptions**: Panels only subscribe to what they need
