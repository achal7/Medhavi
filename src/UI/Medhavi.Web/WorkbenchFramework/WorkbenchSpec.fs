namespace Medhavi.Web.WorkbenchFramework

open Medhavi.Web.WorkspaceEngine
open Bolero

type WorkbenchSpec =
    { Kind: WorkspaceKind
      Title: string
      DefaultPanels: PanelSpec list
      SupportedCapabilities: CapabilityKind list
      DefaultIntent: WorkspaceIntent
      BuildDefaultState: WorkspaceContext -> WorkspaceState
      RenderView: WorkspaceState -> (obj -> unit) -> Node }
