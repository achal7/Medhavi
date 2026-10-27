namespace Medhavi.Client.ActionExecution

open System
open System.Threading.Tasks
open Medhavi.Client
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component

/// Immutable execution context provided to every action handler at dispatch time.
type ActionExecutionContext =
    { Services: IServiceProvider
      OriginComponentId: ViewComponentId
      TargetKey: string option
      Payload: ActionPayload
      PlanningContext: PlanningContext }

    member this.Resolve<'T when 'T : not struct>() : 'T option =
        match box this.Services with
        | null -> None
        | _ ->
            match this.Services.GetService(typeof<'T>) with
            | null -> None
            | s -> Some(s :?> 'T)

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
type ActionRegistry = private ActionRegistry of Map<ActionDefId, ActionHandler>

module ActionRegistry =
    let empty : ActionRegistry = ActionRegistry Map.empty

    let tryFind (actionId: ActionDefId) (ActionRegistry m) : ActionHandler option =
        Map.tryFind actionId m

    let register (actionId: ActionDefId) (handler: ActionHandler) (ActionRegistry m) : ActionRegistry =
        ActionRegistry (Map.add actionId handler m)

    let registerAll (entries: (ActionDefId * ActionHandler) list) (registry: ActionRegistry) : ActionRegistry =
        entries |> List.fold (fun reg (id, h) -> register id h reg) registry

    let contains (actionId: ActionDefId) (ActionRegistry m) : bool =
        Map.containsKey actionId m

    let count (ActionRegistry m) : int =
        Map.count m

module ActionRouter =
    let execute 
        (registry: ActionRegistry)
        (ctx: ActionExecutionContext)
        (actionId: ActionDefId) 
        : Task<Result<ActionExecutionOutcome, string>> =
        
        match ActionRegistry.tryFind actionId registry with
        | Some handler -> handler ctx
        | None -> 
            Task.FromResult(
                Error $"Action '{ActionDefId.value actionId}' has no registered handler.")
