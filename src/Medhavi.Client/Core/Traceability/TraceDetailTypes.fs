namespace Medhavi.Client.Core.Traceability

open System
open Medhavi.Contracts.Traceability

/// Visual node representation on the multi-swimlane causal canvas
type TraceCanvasNode =
    { NodeId: string
      EventId: string
      BoundedContext: string
      AggregateType: string
      AggregateId: string
      CommandType: string
      Message: string
      Severity: string
      Timestamp: DateTimeOffset
      HasDecision: bool
      HasErrors: bool
      StageCount: int
      Stages: string list
      TotalDurationMs: float
      UnderlyingEvents: TelemetryItemDto list
      X: float
      Y: float
      Width: float
      Height: float }

/// Directional edge between two causal events
type TraceCanvasEdge =
    { EdgeId: string
      SourceNodeId: string
      TargetNodeId: string
      SourceContext: string
      TargetContext: string
      IsLoopback: bool
      LatencyMs: float
      HasErrors: bool }

/// Visual tab selection in the detail studio
type TraceDetailTab =
    | FlowCanvasTab = 0
    | WaterfallTab = 1
    | RuleInspectorTab = 2
    | RawDossierTab = 3

/// Detail studio Elmish Model
type TraceStudioModel =
    { SelectedCorrelationId: string option
      ActiveDossier: TraceDossierDto option
      HighFidelityDossier: HighFidelityTraceDossierDto option
      IsLoadingDossier: bool
      ErrorMessage: string option
      ActiveTab: TraceDetailTab
      SelectedNodeId: string option
      SelectedDecisionId: string option
      ZoomLevel: float }

/// Detail studio Elmish Messages
type TraceStudioMsg =
    | LoadTraceDossier of correlationId: string
    | DossierLoaded of Result<TraceDossierDto option * HighFidelityTraceDossierDto option, string>
    | SelectDetailTab of TraceDetailTab
    | SelectTraceNode of nodeId: string
    | SelectDecision of decisionId: string
    | SetZoomLevel of float
