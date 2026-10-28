namespace Medhavi.Client.ActionExecution

open System
open System.Threading.Tasks
open Medhavi.Client
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component

/// Immutable execution context provided to every action handler at dispatch time.
type ActionExecutionContext =
    { DomainServices: DomainServices
      OriginComponentId: ViewComponentId
      TargetKey: string option
      Payload: ActionPayload
      PlanningContext: PlanningContext }
    with
        member this.Domain: DomainServices = this.DomainServices

/// Governed outcome of a domain action dispatch that UserShell coordinates generically.
[<RequireQualifiedAccess>]
type ActionExecutionOutcome =
    | Completed of 
        SuccessMessage: string * 
        RefreshedStores: DataStoreId list
    | JobInitiated of 
        JobId: string * 
        Description: string * 
        PollIntervalMs: int
    | ExplainRequested of 
        EntityId: string * 
        EntityKind: string
    | NavigationRequested of 
        TargetView: ViewDefId
    | ClientSideDialogRequested of 
        Title: string * 
        Message: string

/// A pure action execution function: context -> async outcome or error message.
type ActionHandler = 
    ActionExecutionContext -> Task<Result<ActionExecutionOutcome, string>>

/// Pure functional registry storing action handlers mapped by ActionDefId.
type ActionRegistry = ActionRegistry of Map<ActionDefId, ActionHandler>

module ActionRegistry =
    let empty : ActionRegistry = ActionRegistry Map.empty

    let register (actionId: ActionDefId) (handler: ActionHandler) (ActionRegistry reg) : ActionRegistry =
        ActionRegistry (Map.add actionId handler reg)

    let registerAll (actions: (ActionDefId * ActionHandler) list) (ActionRegistry reg) : ActionRegistry =
        ActionRegistry (List.fold (fun acc (id, handler) -> Map.add id handler acc) reg actions)

    let contains (actionId: ActionDefId) (ActionRegistry reg) : bool =
        Map.containsKey actionId reg

    let tryFind (actionId: ActionDefId) (ActionRegistry reg) : ActionHandler option =
        Map.tryFind actionId reg

    let merge (ActionRegistry a) (ActionRegistry b) : ActionRegistry =
        ActionRegistry (Map.fold (fun acc k v -> Map.add k v acc) b a)

module ActionRouter =
    let execute 
        (registry: ActionRegistry) 
        (ctx: ActionExecutionContext) 
        (actionId: ActionDefId) 
        : Task<Result<ActionExecutionOutcome, string>> =
        match ActionRegistry.tryFind actionId registry with
        | Some handler -> handler ctx
        | None -> Task.FromResult (Error (sprintf "Action '%s' is not registered in ActionRegistry." (ActionDefId.value actionId)))
