namespace Medhavi.Web.WorkspaceEngine

open System
open Medhavi.Web

type WorkspaceId = WorkspaceId of Guid

type WorkspaceKind =
    | DemandWorkspace
    | SupplyWorkspace
    | CapacityWorkspace
    | ScenarioWorkspace
    | AnalyticsWorkspace
    | KnowledgeWorkspace
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
    { CurrentScope: QueryScope
      SelectedCustomerId: string option
      SelectedProductId: string option
      SelectedOrderId: string option
      SelectedResourceId: string option
      SelectedLocationId: string option }

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

type WorkspaceLayout =
    { Columns: int
      PanelIds: string list }

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
      LastUpdatedAt: DateTimeOffset }
