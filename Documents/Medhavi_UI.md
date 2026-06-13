```markdown
# Medhavi UI Operating System  
## Workspace, Workbench, Components, AI, Knowledge Navigation, and Bolero Implementation  

**Audience**: Medhavi architecture and engineering team  
**Scope**: The complete UI architecture for a next‑generation AI‑native APS platform  
**Style**: Implementation‑driven, F#‑first, strict functional, Bolero/Blazor/Radzen  
**Purpose**: This book explains how to build the Medhavi user interface as a workspace‑centric planning operating system, grounded in concrete implementation patterns.

---

# Preface

Medhavi is not being built as a classic enterprise application with a menu tree and static screens. It is being built as a **planning operating system**.

That means the UI must do more than render tables. It must:

- understand planner intent,
- assemble the right context,
- navigate through knowledge relationships,
- show explanations and alternatives,
- react to live planning events,
- collaborate with AI copilots,
- support scenario branching and comparison,
- preserve human control and auditability.

This book describes how to implement that UI in detail.  
It supplements the architectural vision with concrete patterns for Bolero, Radzen, state management, and integration with the backend domain.

The key design choice is this:

> **The user does not begin with screens. The user begins with intent.**

Examples of intent:

- Why are we missing Customer ABC?
- What if Supplier X is delayed by two weeks?
- Which resource is the bottleneck?
- Why was this promise rejected?
- How do Scenario A and Scenario B differ?
- What action should I take next?

The UI must respond by assembling an actionable workspace containing the right panels, the right data, the right explanation, and the right navigation paths.

---

# 1. Why Workspace beats Workbook

## 1.1 The old model

Traditional APS software usually follows a workbook or page‑centric model:

- open demand screen
- open supply screen
- open capacity screen
- open scenario screen
- export Excel
- compare manually
- return to system

This creates several problems:

1. The user must know where the answer lives.
2. The user must manually navigate across bounded contexts.
3. The user must mentally join data from multiple screens.
4. The system does not help the user follow the root cause.
5. AI becomes a side feature rather than a first‑class collaborator.

That model is too rigid for AI‑native planning.

## 1.2 The Medhavi model

Medhavi should use a **workspace‑centric** interaction model.

A workspace is:

- context‑aware,
- task‑oriented,
- composable,
- persistent,
- explainable,
- AI‑augmented,
- event‑driven.

A workspace is **not** a page. It is a live planning canvas.

### Workspace example

When the planner asks:

> Why are we missing Customer ABC?

the system should open or assemble an investigation workspace containing:

- demand summary panel
- supply coverage panel
- capacity bottleneck panel
- scenario comparison panel
- root cause explanation panel
- AI copilot panel
- timeline or event panel
- graph navigation panel

The user should not need to search the application for the correct screen.

## 1.3 Why workspace is the correct abstraction

A workspace matches the way planners think.

Planning is not a single action. It is a sequence:

1. notice symptom
2. investigate context
3. inspect evidence
4. compare alternatives
5. test a scenario
6. choose a response
7. execute and monitor

A workspace is the UI structure that supports that sequence.

It also matches AI behavior.

An AI copilot does not need a screen tree. It needs:

- current context,
- available tools,
- available panels,
- available relations,
- and a way to open them.

This makes workspace the natural shared environment for human, workflow, and AI actors.

---

# 2. Human planning workflow model

## 2.1 The planner’s mental loop

A planner typically works through a repeated loop:

- a signal appears,
- a question is formed,
- evidence is gathered,
- causes are ranked,
- options are compared,
- a decision is taken,
- an action is applied,
- the result is monitored.

This sequence should directly shape the UI architecture.

## 2.2 Planning states

The UI should explicitly model planning states such as:

- observing
- investigating
- comparing
- simulating
- deciding
- applying
- monitoring
- explaining
- resolving

This prevents the UI from becoming a static dashboard.

## 2.3 Investigation‑first UI

Many APS tasks are investigations.

Examples:

- why late?
- why shortage?
- why overload?
- why promise miss?
- why inventory mismatch?
- why scenario divergence?

These are not “view only” tasks. They are investigation tasks.

So Medhavi should provide an **Investigation Workspace** as a first‑class UI concept.

An investigation workspace should:
- start from a symptom,
- bring in the connected data,
- show the relationship chain,
- expose root‑cause candidates,
- allow scenario tests,
- allow action and rollback.

---

# 3. UI architecture principles

## 3.1 Capability‑first UI

The UI should be organized around capabilities rather than pages.

Capabilities include:

- View
- Edit
- Analyze
- Explain
- Compare
- Simulate
- Optimize
- Navigate
- Publish
- Roll back
- Investigate

A capability should be usable by:

- human users,
- AI copilots,
- automation workflows.

## 3.2 Context‑first UI

Every UI action should know the current context:

- selected customer
- selected product
- selected order
- selected scenario
- selected resource
- selected location
- selected plan version
- selected time window

No component should be context‑free if it belongs to planning.

## 3.3 Progressive disclosure

The user should first see the minimum needed to understand the issue.

Then, only when needed, deeper detail should appear.

This prevents planner overload.

## 3.4 Explainability by design

Every important output should be explainable.

If the UI shows:
- a promise date,
- a shortage,
- a bottleneck,
- a scenario delta,
- a KPI change,

then the user should be able to inspect:
- what caused it,
- which events contributed,
- which entities are connected,
- which scenario deltas changed it,
- what the recommended next step is.

## 3.5 Event‑driven rendering

The UI must not rely on manual refresh loops.

It should subscribe to:
- read model updates,
- scenario changes,
- knowledge graph updates,
- AI recommendation updates,
- planning event notifications.

When those change, only affected parts of the workspace should refresh.

---

# 4. Medhavi UI structure

## 4.1 High‑level hierarchy

The UI should be built as:

- Shell
- Workspace
- Workbench
- Panel
- Component

### Shell
The outer frame of the application.

Contains:
- top bar,
- navigation,
- user identity,
- scenario selector,
- notifications,
- AI entry point.

### Workspace
The active planning context.

Contains:
- context,
- current task,
- panels,
- layout,
- pinned entities,
- AI state.

### Workbench
A domain‑specific set of tools inside a workspace.

Examples:
- Demand Workbench
- Supply Workbench
- Capacity Workbench
- Promise Workbench
- Scenario Workbench
- Knowledge Workbench
- Analytics Workbench
- AI Workbench

### Panel
A functional area within a workbench.

Examples:
- grid panel
- chart panel
- graph panel
- explanation panel
- compare panel
- timeline panel
- AI panel
- command panel

### Component
The smallest building block.

Examples:
- KPI card
- filter chip
- node card
- edge card
- breadcrumb
- diff cell
- status badge
- action button

---

# 5. Workspace operating system model

## 5.1 What this means (and what it does not mean)

The UI should behave like an operating system for planning tasks: it holds context, manages tools, and orchestrates panels. However, **this is not a literal OS**; it is a disciplined MVU shell with a registry of workbenches. Start simple, evolve the engine as patterns stabilise. Do not build a generic “panel manager” before you have a concrete workbench working end‑to‑end.

## 5.2 Workspace responsibilities

A workspace engine should manage:

- active task,
- selected entity,
- active scenario,
- open panels,
- panel order,
- pinned data,
- local filters,
- local compare state,
- local simulation state,
- collaboration presence (deferred),
- AI conversation state.

## 5.3 Workspace state model

A workspace state should be explicit and typed.

```fsharp
module Medhavi.Ui.Workspace.Domain

type WorkspaceId = WorkspaceId of System.Guid

type WorkspaceKind =
    | DemandWorkspace
    | SupplyWorkspace
    | CapacityWorkspace
    | PromiseWorkspace
    | ScenarioWorkspace
    | KnowledgeWorkspace
    | AnalyticsWorkspace
    | AiWorkspace
    | InvestigationWorkspace

type WorkspaceIntent =
    | Browse
    | Investigate
    | Compare
    | Simulate
    | Explain
    | Optimize
    | Resolve

type WorkspaceContext =
    { SelectedCustomerId: string option
      SelectedProductId: string option
      SelectedOrderId: string option
      SelectedScenarioId: string option
      SelectedResourceId: string option
      SelectedLocationId: string option
      SelectedTimeRange: TimeRange option }

type WorkspaceState =
    { Id: WorkspaceId
      Kind: WorkspaceKind
      Intent: WorkspaceIntent
      Context: WorkspaceContext
      Layout: WorkspaceLayout
      Panels: PanelInstance list
      PinnedEntities: string list
      ActiveConversationId: string option
      ActiveCollaborationSessionId: string option
      LastUpdatedAt: System.DateTimeOffset }
```

## 5.4 Workspace lifecycle

Workspace lifecycle stages:

1. created
2. initialized
3. hydrated with data
4. user selected entity or task
5. panels assembled
6. live updates applied
7. AI intents handled
8. preserved or discarded

## 5.5 Workspace creation flow

When the user opens the application:

1. Shell loads.
2. User identity and permissions are resolved.
3. Default workspace is selected from role and preference.
4. Workspace context is hydrated from route, last session, or user action.
5. Workbench is chosen.
6. Panels are assembled according to intent.
7. Query services load data.
8. Live subscriptions start.
9. Workspace becomes interactive.

## 5.6 Workspace state serialisation

Workspace layouts and user‑specific state (panel positions, filters, pinned entities) should be persisted server‑side in a lightweight JSON document per user/workspace combination.  
During rehydration, if a referenced entity or panel no longer exists (e.g., after a schema change), the system must gracefully ignore that part of the state and fall back to defaults.  
Early implementation can use the browser’s `localStorage` for prototyping, but the final target is a server‑side profile store, which also supports cross‑device continuity.

---

# 6. Workbench framework

## 6.1 What a workbench is

A workbench is the domain‑specific toolset inside a workspace.

It defines:
- which capabilities are relevant,
- which panels are available,
- which default filters are used,
- which navigation rules apply,
- which actions are permitted.

## 6.2 Workbench kinds

Recommended workbench kinds:

- Demand
- Supply
- Capacity
- Promise
- Scenario
- Knowledge
- Analytics
- AI
- Investigation

## 6.3 Workbench registration

Workbenches should be registered in a central registry.

```fsharp
type WorkbenchSpec =
    { Kind: WorkspaceKind
      Title: string
      DefaultPanels: PanelSpec list
      SupportedCapabilities: CapabilityKind list
      DefaultIntent: WorkspaceIntent
      Build: WorkspaceContext -> WorkspaceState }
```

## 6.4 Workbench responsibilities

A workbench should:
- define panel templates,
- define data sources,
- define commands available,
- define navigation rules,
- define what context fields matter,
- define AI tool availability.

## 6.5 Workbench composition

Workbench composition should be declarative.

For example:

- Demand workbench = forecast grid + sales order grid + demand trend chart + explanation panel
- Capacity workbench = resource timeline + bottleneck chart + resource detail panel + compare panel
- Knowledge workbench = graph view + path view + semantic search + root cause panel

---

# 7. Capability architecture

## 7.1 Capability is the shared contract

Capabilities are the universal interaction contract between:
- human UI,
- AI copilot,
- workflow engine,
- automation.

A capability should not be implemented separately for each actor.

## 7.2 Capability model

```fsharp
type CapabilityKind =
    | View
    | Edit
    | Analyze
    | Explain
    | Compare
    | Simulate
    | Optimize
    | Publish
    | Rollback
    | Navigate
    | Investigate

type CapabilityRequest =
    { CapabilityId: string
      WorkspaceId: WorkspaceId
      Context: WorkspaceContext
      Parameters: Map<string, string> }

type CapabilityResponse =
    { Title: string
      PayloadJson: string
      SuggestedPanels: string list
      SuggestedNavigation: string list
      Confidence: decimal option }

type Capability =
    { Id: string
      Kind: CapabilityKind
      Name: string
      Description: string
      Execute: CapabilityRequest -> Async<Result<CapabilityResponse, string>> }
```

## 7.3 Dual entry point

Every capability should have two entry points:

1. UI entry point
2. AI tool entry point

The same underlying function powers both.

Example:
- `ShowInventoryPosition`
- UI: opens inventory panel
- AI: tool returns inventory position and can suggest opening the panel

## 7.4 Capability catalog

The UI must know available capabilities per workbench.

Examples:
- Demand: analyze forecast, compare scenarios, edit forecast, explain demand variance
- Supply: view stock, inspect supply chain, explain shortages, simulate delays
- Capacity: view resource load, analyze bottlenecks, compare scheduling options
- Promise: calculate promise, explain rejection, compare alternatives
- Knowledge: traverse graph, find root cause, explain chain, navigate relations

---

# 8. Panels and panel lifecycle

## 8.1 Panel structure

A panel is a self‑contained view with local state.

```fsharp
type PanelKind =
    | GridPanel
    | ChartPanel
    | GraphPanel
    | GanttPanel
    | DetailPanel
    | ExplanationPanel
    | ComparePanel
    | SearchPanel
    | CommandPanel
    | CopilotPanel
    | TimelinePanel
    | MetricsPanel

type PanelSpec =
    { PanelId: string
      PanelKind: PanelKind
      Title: string
      Capabilities: CapabilityKind list
      QueryKey: string
      IsClosable: bool
      IsPinned: bool
      DefaultWidth: int option
      DefaultHeight: int option }

type PanelInstance =
    { InstanceId: string
      Spec: PanelSpec
      LocalStateJson: string option }
```

## 8.2 Panel lifecycle

Panel lifecycle should be explicit:

1. registered
2. instantiated
3. initialized
4. loaded
5. rendered
6. updated
7. refreshed
8. disposed

## 8.3 Panel communication

Panels should not directly reach into each other.

Preferred communication:
- workspace message dispatch
- shared context updates
- event subscriptions
- parent orchestration

Example:
- a selection in a grid panel dispatches `EntitySelected`
- the workspace updates context
- other panels re‑query based on new context

## 8.4 Panel update rules

A panel should refresh when:
- its query key changes,
- its context changes,
- a relevant event arrives,
- the active scenario changes,
- a selected entity changes.

It should not refresh because a random unrelated event happened.  
That distinction is critical.

## 8.5 Reusable error and loading states

Every panel must handle async data in a consistent way. Use a **state wrapper component** that takes a `RemoteData<'T>` (or a DU) and renders the appropriate Radzen UI:

```fsharp
type RemoteData<'T> =
    | NotRequested
    | Loading
    | Loaded of 'T
    | Error of string

// A thin Blazor component: RemoteState.razor / RemoteState.fs
// It receives a RemoteData<'T> and a RenderFragment<'T> for the loaded state.
```

This ensures all panels share the same loading spinners, error banners, and empty‑state visuals without duplication.

---

# 9. Planner interaction patterns

## 9.1 Common planner tasks

Typical APS tasks:
- review demand
- inspect shortage
- explain promise
- compare scenarios
- reschedule operation
- investigate bottleneck
- approve recommendation
- rollback bad change

## 9.2 Interaction loop

The user interaction flow should be:

1. symptom
2. explore
3. compare
4. simulate
5. decide
6. act
7. monitor

The UI should support every step.

## 9.3 Investigation workspace

Investigation is a first‑class UI mode.

An investigation workspace should contain:
- symptom summary,
- ranked causes,
- evidence chain,
- impacted entities,
- scenario comparison,
- recommended action,
- explainability panel.

## 9.4 Comparison workspace

Comparison should support:
- base vs scenario,
- scenario vs scenario,
- version vs version,
- before vs after planning,
- before vs after replanning.

Comparison should be available as:
- table,
- chart,
- timeline,
- graph,
- summary card.

---

# 10. Knowledge navigation UI

## 10.1 Why Knowledge needs special UI

The Knowledge bounded context is semantically rich.  
The user should explore it visually and interactively.

## 10.2 Graph browsing patterns

Knowledge UI should support:
- neighborhood browsing,
- upstream traversal,
- downstream traversal,
- shortest explanation path,
- cause chain,
- impact chain,
- scenario overlay chain.

## 10.3 Recommended first version

Start with a pragmatic split view:

- left: semantic list or tree
- center: selected node detail
- right: relation list or path panel
- bottom: explanation/evidence panel

This is easier than jumping immediately into a complex force‑directed graph.

## 10.4 Graph visualization later

Later, add:
- force‑directed graph,
- radial graph,
- layered dependency map,
- path highlighting,
- animated traversal.

## 10.5 Graph node interactions

A node should allow:
- open,
- pin,
- expand,
- focus,
- explain,
- compare,
- navigate to related workbench.

## 10.6 Semantic search

Knowledge workspace should expose semantic search.

Search should work across:
- entity names,
- aliases,
- relations,
- alerts,
- KPIs,
- scenario names,
- user‑generated notes.

Search result actions:
- open entity,
- open related workspace,
- explain relation,
- compare with another node,
- investigate cause chain.

---

# 11. Scenario UI

## 11.1 Scenario design principle

Scenario should be delta‑based, not copied data.

UI must reflect that clearly.

## 11.2 Scenario panel requirements

A scenario workspace should provide:
- branch selector,
- current delta list,
- delta editor,
- scenario compare,
- scenario publish,
- merge,
- rollback,
- simulation execution,
- impact summary.

## 11.3 Scenario edit model

A scenario edit should be explicit and typed.

```fsharp
type ScenarioDelta =
    | DemandOverride of demandLineId:string * newQty:decimal
    | SupplyDelay of supplyOrderId:string * delayDays:int
    | CapacityChange of resourceId:string * deltaHours:decimal
    | RoutingOverride of productId:string * routingId:string
    | PriorityChange of demandLineId:string * priority:int

type ScenarioEditorState =
    { ScenarioId: string
      BaseScenarioId: string option
      Deltas: ScenarioDelta list
      Dirty: bool
      LastPreviewJson: string option }
```

## 11.4 Compare panel

The compare panel should show:
- changed rows,
- before and after values,
- impact summary,
- numerical delta,
- service impact,
- cost impact,
- risk impact.

## 11.5 Publish and rollback

Publishing scenario changes should require:
- validation,
- preview,
- approval,
- logging,
- rollback path.

Rollback should be first‑class, not an afterthought.

---

# 12. Root‑cause UI

## 12.1 Root cause panel

Root cause should be presented as:
- symptom header,
- ranked cause list,
- selected cause path,
- supporting evidence,
- recommended action.

## 12.2 Root cause card structure

```fsharp
type RootCauseCandidate =
    { CauseId: string
      Title: string
      Score: decimal
      EvidenceCount: int
      Path: string list
      Summary: string }
```

## 12.3 Interaction model

The user should be able to:
- select a cause,
- inspect the graph path,
- open related workbench,
- simulate corrective action,
- accept recommendation,
- apply action.

## 12.4 Root cause and AI

The AI should not invent causes.  
It should summarize ranked candidates derived from the Knowledge graph and analytics evidence.

---

# 13. AI workspace and copilot UI

## 13.1 AI is a workspace participant

AI should not be a floating chat bubble only.

The AI should be able to:
- open workspaces,
- select entities,
- request analysis,
- show explanation cards,
- compare scenarios,
- recommend actions,
- trigger simulations.

## 13.2 AI intents

```fsharp
type AiIntent =
    | OpenWorkspace of WorkspaceKind * WorkspaceContext
    | FocusEntity of string
    | OpenExplainPanel of string
    | OpenComparePanel of string * string
    | RunSimulation of string
    | OpenScenario of string
    | ShowRootCause of string
```

## 13.3 AI output contract

AI output should be structured:
- text summary,
- evidence references,
- panel suggestions,
- action suggestions,
- confidence score.

## 13.4 Human approval

High‑impact AI suggestions should not execute automatically unless explicitly permitted by guardrails.

The UI should show:
- proposed action,
- expected effect,
- evidence,
- rollback option,
- approve button,
- reject button.

## 13.5 Integration with the Elmish loop

The copilot panel dispatches messages like `CopilotIntent(OpenWorkspace(...))`. The top‑level `update` function translates these intents into the appropriate workspace/panel state changes. This keeps the AI bound to the same MVU architecture; it does not directly manipulate the UI.

---

# 14. Live updates and subscriptions

## 14.1 Why live updates matter

Planning data changes constantly:
- inventory,
- orders,
- capacity,
- scenarios,
- promises,
- knowledge facts.

The UI must reflect those changes quickly.

## 14.2 Update strategy

Use event‑driven subscriptions.

Recommended sequence:
1. backend event changes state,
2. projection updates,
3. live update notification sent,
4. relevant workspace receives message,
5. affected panel re‑queries data,
6. UI re‑renders only the impacted area.

## 14.3 Avoid full refreshes

Do not reload the entire workspace on every event.

Instead, use panel‑scoped refresh:
- demand panel refreshes if demand changes,
- capacity panel refreshes if capacity changes,
- graph panel refreshes if knowledge changes,
- scenario panel refreshes if scenario changes.

## 14.4 SignalR model

SignalR is a good fit for live notifications.

Each workspace can subscribe to a channel or topic such as:
- tenant,
- scenario,
- customer,
- order,
- resource,
- graph shard.

## 14.5 Backend API requirements for live UI

To support these subscriptions, the backend must expose:

- Read‑model endpoints per panel (e.g. `GET /api/demand/summary?filter=...`),
- SignalR hubs that emit change notifications scoped to the appropriate context (e.g. `scenario/{id}/changes`, `resource/{id}/capacity`).

Panels must never call domain aggregates directly; all data access goes through the read‑model API.

---

# 15. Search and command palette

## 15.1 Search is a top‑level interaction model

The UI should have:
- global search,
- semantic search,
- command search,
- entity search,
- navigation search.

## 15.2 Command palette

The command palette should allow:
- opening workbenches,
- jumping to entities,
- running analyses,
- launching comparisons,
- starting simulations,
- asking AI questions.

## 15.3 Search result types

Results can be:
- entity
- workbench
- capability
- analysis
- scenario
- alert
- graph node
- AI suggestion

## 15.4 Search interaction

When a result is selected:
- open the correct workspace,
- focus the entity,
- load the appropriate panels,
- highlight the relevant path.

---

# 16. Bolero / Blazor implementation

## 16.1 Why Bolero fits

Bolero fits because it supports:
- F#,
- functional UI composition,
- Elmish/MVU patterns,
- integration with Blazor ecosystem,
- strong component modularity.

## 16.2 Target hosting model

The initial target is **Blazor Server**, which is ideal for enterprise applications with large datasets that should not be loaded entirely into the browser. All panel rendering and SignalR subscriptions run on the server, with minimal latency for UI updates.  
If a switch to WebAssembly is later required, the architecture will need to be re‑evaluated to use HTTP‑based API servers instead of direct server‑side query calls.

## 16.3 Project structure

Keep feature‑oriented folder organisation. Avoid scattering models across a global `State` folder; each feature (workbench, panel) should contain its own Model, Msg, Update, and View files.

```text
Medhavi.Ui/
  Shell/
    AppShell.fs           // top-level AppModel, AppMsg, update, view
    TopBar.fs
    Sidebar.fs
    NotificationTray.fs
  WorkspaceEngine/
    WorkspaceModel.fs
    WorkspaceUpdate.fs
    WorkspaceView.fs
    WorkspaceRegistry.fs
    WorkspacePersistence.fs
  WorkbenchFramework/
    WorkbenchSpec.fs
    WorkbenchRegistry.fs
    WorkbenchHost.fs
  Capabilities/
    CapabilityModel.fs
    CapabilityRegistry.fs
    CapabilityRunner.fs
  Panels/
    PanelHost.fs
    GridPanel.fs
    ChartPanel.fs
    GraphPanel.fs
    ComparePanel.fs
    ExplanationPanel.fs
    CopilotPanel.fs
    SearchPanel.fs
    TimelinePanel.fs
    RemoteData.fs          // shared RemoteData<T> and StateWrapper component
  DemandWorkbench/
    DemandWorkspace.fs     // contains Model, Msg, Update, View for demand
    DemandGridPanel.fs
    DemandChartPanel.fs
  SupplyWorkbench/
    SupplyWorkspace.fs
    SupplyGridPanel.fs
    InventoryDetailPanel.fs
  CapacityWorkbench/
    ...
  ScenarioWorkbench/
    ScenarioWorkspace.fs
    ScenarioDiffPanel.fs
    ScenarioEditorPanel.fs
    ScenarioPublishDialog.fs
  KnowledgeWorkbench/
    KnowledgeWorkspace.fs
    KnowledgeGraphPanel.fs
    RootCausePanel.fs
    SemanticSearchPanel.fs
  AiWorkbench/
    AiWorkspace.fs
    AiIntent.fs
    AiToolRegistry.fs
    AiConversationPanel.fs
  Services/
    WorkspaceService.fs
    QueryService.fs          // thin wrapper over HTTP / SignalR calls
    CommandService.fs
    LiveUpdateService.fs
    AiService.fs
  Styles/
    Theme.fs
    Layout.fs
    Tokens.fs
```

## 16.4 Per‑feature structure

Within each workbench folder, follow the same pattern:

- **Types.fs** – local domain types (e.g. `DemandRow`)
- **Model.fs** – the workbench model record
- **Msg.fs** – discriminated union for messages
- **Update.fs** – pure update function
- **View.fs** – Bolero/Blazor rendering code

Example for DemandWorkbench:

```
DemandWorkbench/
  Types.fs
  Model.fs
  Msg.fs
  Update.fs
  View.fs
  Services.fs      // any workbench-specific API helpers
```

## 16.5 Nested state

Do not use one giant object for everything. Use nested state:

- app state,
- workspace state,
- workbench state,
- panel state.

This keeps the UI manageable.

## 16.6 Sample top‑level model and messages

```fsharp
type AppModel =
    { User: UserContext option
      ActiveWorkspace: WorkspaceState option
      OpenWorkspaces: WorkspaceState list
      Notifications: Notification list
      GlobalSearch: GlobalSearchState
      Copilot: CopilotState }

type AppMsg =
    | WorkspaceOpened of WorkspaceState
    | WorkspaceClosed of WorkspaceId
    | OpenRequested of WorkspaceKind * WorkspaceContext
    | GlobalSearchChanged of string
    | ReceiveLiveUpdate of LiveUpdate
    | CopilotIntentReceived of AiIntent
```

## 16.7 Message routing

Message routing should follow the hierarchy:
- app messages,
- workspace messages,
- workbench messages,
- panel messages.

Each level should only know its own responsibilities and parent/child message contracts.

## 16.8 Command handling

Commands should be used for side effects:
- load read model,
- call API,
- send command,
- subscribe to updates,
- save layout,
- persist scenario delta.

---

# 17. Radzen strategy

## 17.1 Where Radzen is useful

Radzen is good for:
- data grids,
- dialogs,
- dropdowns,
- tabs,
- splitters,
- auto‑complete,
- charts,
- drawers,
- forms,
- timelines.

## 17.2 Where custom components are needed

You should build custom Medhavi components for:
- graph navigation,
- explanation cards,
- investigation workspace,
- scenario diff composer,
- AI recommendation cards,
- workspace canvas,
- panel host management.

## 17.3 Thin wrappers

Do not over‑engineer wrappers. Wrap only to:
- standardize styling,
- standardize event wiring,
- standardize MVU integration,
- standardize empty/loading/error states.

## 17.4 Grid strategy

Use grids with:
- virtualized rows,
- server‑side paging,
- filtering,
- sorting,
- row actions,
- selected row state,
- context menu actions,
- navigation hooks.

## 17.5 Dialog strategy

Use dialogs for:
- edit flows,
- compare confirmation,
- publish confirmation,
- rollback confirmation,
- simulation parameter input,
- AI explanation prompts.

## 17.6 Example: wrapping RadzenDataGrid for MVU

A thin wrapper that translates Radzen events into MVU messages:

```fsharp
type GridConfig<'T> =
    { Columns: RadzenColumn<'T> list
      Data: IReadOnlyList<'T>
      IsLoading: bool
      PageSize: int
      Count: int
      OnPageChanged: int -> 'Msg
      OnSortChanged: string * SortOrder -> 'Msg
      OnRowSelected: 'T -> 'Msg }

// In the View function, you use RadzenDataGrid with callbacks:
RadzenDataGrid.Create<'T>(fun grid ->
    grid.Data(config.Data)
        .Columns(cols -> ... )
        .PageSize(config.PageSize)
        .Count(config.Count)
        .PageChanged(fun args -> dispatch (config.OnPageChanged args.Page))
        .SortChanged(fun args -> dispatch (config.OnSortChanged(args.ColumnName, args.SortOrder)))
        // ...
)
```

This pattern keeps Radzen interaction purely within the MVU loop and prevents imperative state manipulation.

---

# 18. Filters, criteria, and selection logic

## 18.1 Filtering should be domain‑aware

Filters are not generic UI add‑ons.

Examples:
- filter demand by customer, product, date range, priority, scenario
- filter supply by material, location, status, arrival date
- filter capacity by resource, group, shift, overload state
- filter knowledge by relation kind, source context, scenario scope

## 18.2 Filter state

Filter state should be explicit and serializable.

```fsharp
type GridFilter =
    { Field: string
      Operator: string
      Value: string }

type FilterState =
    { Filters: GridFilter list
      SortBy: string option
      SortDirection: string option
      SearchText: string option }
```

## 18.3 Selection behavior

Selection should be consistent:
- click row selects entity,
- double‑click opens detail workspace,
- right‑click opens context menu,
- keyboard shortcut supports action.

## 18.4 Context menu behavior

Context menus should expose:
- open,
- inspect,
- explain,
- compare,
- simulate,
- navigate,
- pin,
- add to scenario,
- copy identifier,
- open in AI assistant.

---

# 19. Navigation model

## 19.1 Navigation is semantic

Navigation should not be random page switching. It should follow planning semantics.

Examples:
- Customer → Demand Orders
- Demand Order → Supply Coverage
- Supply Coverage → Production Order
- Production Order → Operation Schedule
- Operation Schedule → Resource Bottleneck
- Resource Bottleneck → Knowledge Path
- Knowledge Path → Root Cause
- Root Cause → Simulation / Action

## 19.2 Breadcrumb model

Breadcrumbs should reflect semantic path, not just screen hierarchy.

## 19.3 Navigation graph

The navigation graph should be driven by Knowledge and domain relationships.

This allows the UI to offer:
- related items,
- impacted items,
- upstream causes,
- downstream effects.

---

# 20. Collaboration and multi‑user UX (deferred)

## 20.1 Defer heavy collaboration features

Collaboration features (presence, shared editing, conflict resolution) are valuable but complex. They should not be implemented until single‑user workspaces are production‑hardened and the core workspace engine is stable.

## 20.2 Early collaboration scope

The initial collaborative capability can be limited to:

- view‑only sharing of a workspace (read‑only mode for another user),
- simple locking of scenarios to prevent simultaneous edits.

## 20.3 Future states (post‑Phase 9)

Once the core is mature, the UI can introduce:
- presence indicators,
- real‑time co‑editing with operational transforms,
- commenting on entities,
- approval workflows for scenario publishing.

---

# 21. Persistence of UI state

The workspace should persist:
- layout,
- open panels,
- selected context,
- pinned entities,
- active scenario,
- filters,
- panel positions,
- compare state,
- investigation state.

This means the user can return later and resume.

Recommended persistence:
- server‑side workspace profile store (primary),
- `localStorage` cache for short‑term restore (optional),
- user preference store for defaults.

---

# 22. Performance design

## 22.1 Performance goals

The UI should be fast even on large planning datasets.

Key strategies:
- virtualized grids,
- paged queries,
- incremental panel loading,
- cached panel data,
- lazy graph expansion,
- precomputed projections.

## 22.2 Loading strategy

Panel load phases:
1. placeholder
2. metadata
3. light summary
4. full detail
5. live update subscription

## 22.3 Avoid expensive client models

Do not load entire datasets into the browser.  
Use server‑side query models and stream only what is required.

---

# 23. Error handling and resilience

## 23.1 Loading errors

Each panel should support:
- loading state,
- empty state,
- error state,
- retry state,
- stale state.

## 23.2 Failure patterns

If a panel fails:
- it should not crash the entire workspace,
- the error should be localized,
- the workspace should remain usable.

## 23.3 Partial availability

If AI is unavailable, the workspace should still function.  
If a graph subquery fails, the UI should degrade gracefully.

---

# 24. Backend requirements for the UI

To make this UI architecture work, the backend must provide:

- **Read‑model endpoints per panel**: Each panel (grid, chart, detail) fetches data via a dedicated API that returns only the projection needed for that panel. Example: `GET /api/demand/summary?customerId=X&scenarioId=Y`.
- **SignalR hubs for live updates**: Hubs that push domain‑scoped change notifications. Topics like `scenario/{id}/changes`, `resource/{id}/capacity`.
- **Command API**: A separate set of endpoints for mutating state (e.g. `POST /api/scenario/{id}/publish`). The UI never calls domain aggregates directly.
- **Semantic search endpoint**: `GET /api/knowledge/search?q=...` that returns entities, workbench suggestions, and navigation paths.

These services should be implemented as a lightweight BFF (Backend For Frontend) layer that sits between the UI and the core bounded contexts, enforcing that the front‑end only sees read‑optimised projections.

---

# 25. Implementation roadmap

## Phase 1: Core workspace shell and first workbench
**Goal**: Prove the MVU nesting, Radzen integration, and read‑model API end‑to‑end.

- Build shell (`AppShell`, top bar, sidebar, notification tray).
- Implement workspace engine (creation, panel host, layout persistence).
- Create the **Demand Workbench** with a single `DemandGridPanel` that loads paginated data via a read‑model API.
- Integrate a `StateWrapper` component for loading/error states.
- Test the whole flow with Blazor Server and SignalR subscriptions.

## Phase 2: Panel infrastructure and command palette
- Extract reusable grid, chart, and detail panel components with `GridConfig` pattern.
- Build the command palette (RadzenAutoComplete) that can open workbenches and focus entities.
- Implement simple filtering and context‑based query refresh.

## Phase 3: Supply, Capacity, and Promise workbenches
- Roll out the same pattern to these domains.
- Add live update subscriptions scoped by scenario and entity.

## Phase 4: Scenario workspace
- Delta editor, compare panel, publish/rollback flows.
- Persistence of scenario‑specific state.

## Phase 5: Knowledge workspace and root‑cause UI
- Semantic search, split‑view graph navigation, root‑cause panel.
- Integrate with the Knowledge bounded context’s read models.

## Phase 6: AI workspace and copilot
- Copilot panel, tool registry, `AiIntent` dispatching.
- Human‑approval flows for high‑impact suggestions.

## Phase 7: Investigation orchestration
- Investigation workspace that assembles panels automatically from a symptom.
- Cross‑workbench jump flows (e.g., from bottleneck to capacity workbench).

## Phase 8: Collaboration (deferred)
- View‑only sharing, locking, commenting, and approval workflows.

## Phase 9: Performance and polish
- Lazy loading, incremental graph expansion, keyboard shortcuts, extensive caching.

---

# 26. Implementation sequence (condensed)

1. Define UI primitives (workspace, panel, capability, filter types).
2. Build shell and workspace engine.
3. Implement Demand Workbench as MVP.
4. Extract reusable panel infrastructure (`RemoteData`, `GridConfig`).
5. Add workbench registry and command palette.
6. Build remaining workbenches (Supply, Capacity, Promise, Scenario).
7. Add Knowledge navigation and root‑cause panels.
8. Integrate AI copilot and AI intents.
9. Implement investigation workspace orchestration.
10. Add collaboration and performance optimizations.

---

# 27. Recommended file structure (detailed)

```text
src/Medhavi.Ui/
  Shell/
    AppShell.fs
    TopBar.fs
    Sidebar.fs
    NotificationTray.fs
  WorkspaceEngine/
    WorkspaceModel.fs
    WorkspaceUpdate.fs
    WorkspaceView.fs
    WorkspaceRegistry.fs
    WorkspacePersistence.fs
  WorkbenchFramework/
    WorkbenchSpec.fs
    WorkbenchRegistry.fs
    WorkbenchHost.fs
  Capabilities/
    CapabilityModel.fs
    CapabilityRegistry.fs
    CapabilityRunner.fs
  Panels/
    PanelHost.fs
    GridPanel.fs
    ChartPanel.fs
    GraphPanel.fs
    ComparePanel.fs
    ExplanationPanel.fs
    CopilotPanel.fs
    SearchPanel.fs
    TimelinePanel.fs
    RemoteData.fs
  DemandWorkbench/
    Types.fs
    Model.fs
    Msg.fs
    Update.fs
    View.fs
    Services.fs
  SupplyWorkbench/
    Types.fs
    Model.fs
    Msg.fs
    Update.fs
    View.fs
    Services.fs
  CapacityWorkbench/
    ...
  ScenarioWorkbench/
    ScenarioWorkspace.fs
    ScenarioDiffPanel.fs
    ScenarioEditorPanel.fs
    ScenarioPublishDialog.fs
  KnowledgeWorkbench/
    KnowledgeWorkspace.fs
    KnowledgeGraphPanel.fs
    RootCausePanel.fs
    SemanticSearchPanel.fs
  AiWorkbench/
    AiWorkspace.fs
    AiIntent.fs
    AiToolRegistry.fs
    AiConversationPanel.fs
  Services/
    WorkspaceService.fs
    QueryService.fs
    CommandService.fs
    LiveUpdateService.fs
    AiService.fs
  Styles/
    Theme.fs
    Layout.fs
    Tokens.fs
```

---

# 28. Design guardrails

Do not build the UI as:
- a pile of unrelated pages,
- a generic form generator,
- a hard‑coded workbook clone,
- a chatbot with a few charts,
- a collection of one‑off screens.

Do not over‑engineer the “operating system” metaphor. Start with a concrete workbench, then extract abstractions only when patterns repeat three times.

Instead build:
- a workspace engine,
- a workbench registry,
- a panel ecosystem,
- a capability model,
- an AI intent system,
- a semantic navigation layer.

---

# 29. Final architectural statement

The Medhavi UI should be a **workspace operating system for planning**.

Its job is to help the planner:
- understand the current state,
- explore related data,
- find root cause,
- compare alternatives,
- simulate changes,
- collaborate with AI,
- take action safely,
- and preserve a full audit trail.

That is how Medhavi becomes materially different from a classic APS product.

---

# Appendix A — Example F# message hierarchy

```fsharp
type AppMsg =
    | WorkspaceRequested of WorkspaceKind * WorkspaceContext
    | WorkspaceOpened of WorkspaceState
    | WorkspaceClosed of WorkspaceId
    | LiveUpdateReceived of LiveUpdate
    | GlobalSearchTextChanged of string
    | GlobalSearchSelected of string
    | CopilotIntent of AiIntent
    | OpenNotification of string
```

# Appendix B — Example panel message hierarchy

```fsharp
type DemandPanelMsg =
    | Load
    | Loaded of DemandRow list
    | LoadFailed of string
    | FilterChanged of FilterState
    | RowSelected of string
    | ExplainRequested of string
    | CompareRequested of string
    | NavigateRequested of string
```

# Appendix C — Example AI tool contract

```fsharp
type AiTool =
    { Name: string
      Description: string
      Invoke: Map<string,string> -> Async<Result<string, string>> }
```

# Appendix D — Example workspace event flow

```text
User intent
  → Workspace request
  → Workbench selection
  → Panel assembly
  → Query execution
  → Live subscription
  → AI intents / navigation / action
```

# Appendix E — RadzenDataGrid MVU wrapper sketch

```fsharp
type GridConfig<'T, 'Msg> =
    { Columns: ColumnConfig<'T> list
      Data: IReadOnlyList<'T>
      IsLoading: bool
      TotalCount: int
      Page: int
      PageSize: int
      OnPageChanged: int -> 'Msg
      OnSortChanged: (string * SortOrder) -> 'Msg
      OnRowSelected: 'T -> 'Msg
      RowClass: 'T -> string option }

// Usage inside a workbench view:
let demandGridConfig (model: DemandModel) (dispatch: DemandMsg -> unit) =
    { Columns = [ ... ]
      Data = model.Rows
      IsLoading = model.LoadingState = Loading
      TotalCount = model.TotalCount
      Page = model.Page
      PageSize = model.PageSize
      OnPageChanged = (fun p -> dispatch (DemandPageChanged p))
      OnSortChanged = (fun (col, ord) -> dispatch (DemandSortChanged(col, ord)))
      OnRowSelected = (fun r -> dispatch (DemandRowSelected r))
      RowClass = fun r -> if r.IsLate then Some "text-danger" else None }

// The Radzen component then binds these callbacks.
```

This pattern keeps all logic inside the MVU loop, making the UI predictable and testable.
```