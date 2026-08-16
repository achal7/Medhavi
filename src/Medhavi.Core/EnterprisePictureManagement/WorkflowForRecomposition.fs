/// CA-C-019 Enterprise Picture Management Workflows
module Medhavi.Core.EnterprisePictureManagement.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Core
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Core.ArsIdentifiers

module PictureRecomposition =

    type PendingChanges =
        { DemandChanged: bool
          SupplyChanged: bool
          InventoryChanged: bool
          LastChangeTime: Timestamp option }

        static member Empty =
            { DemandChanged = false
              SupplyChanged = false
              InventoryChanged = false
              LastChangeTime = None }

        member this.HasChanges = this.DemandChanged || this.SupplyChanged || this.InventoryChanged

    type WorkflowState =
        { PlanningScopeId: PlanningScopeId
          PendingChanges: PendingChanges
          CompositionInProgress: bool
          DebounceStartedAt: Timestamp option }

    type WorkflowEvent =
        | MaterialChangeDetected of domain: string * timestamp: Timestamp
        | DebounceWindowExpired of currentTime: Timestamp
        | CompositionCompleted of timestamp: Timestamp
        | CompositionFailed of reason: string * timestamp: Timestamp

    type WorkflowAction =
        | StartDebounceWindow
        | ResetDebounceWindow
        | TriggerRecomposition
        | LogCompositionFailure of reason: string

    /// Dependencies required by this workflow
    /// Ports are injected functions that query other bounded contexts
    type WorkflowDependencies =
        {
            Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
            EnterprisePictureApi: EnterprisePictureApi
            DebounceWindow: TimeSpan
            PlanningScopeId: PlanningScopeId
            GetCurrentTime: unit -> Timestamp
            /// Query port: Get active demand references for the planning scope
            GetActiveDemandReferences: PlanningScopeId -> Task<DemandId list>
            /// Query port: Get available supply references for the planning scope
            GetAvailableSupplyReferences: PlanningScopeId -> Task<SupplyId list>
            /// Query port: Get current inventory references for the planning scope
            GetCurrentInventoryReferences: PlanningScopeId -> Task<InventoryIdentity list>
            Codec: Medhavi.Common.Codec<DemandUnderstandingPublishedNotification>
        }

    /// Pure step function
    let step (state: WorkflowState) (event: WorkflowEvent) : WorkflowState * WorkflowAction list =

        match event with
        | MaterialChangeDetected(domain, timestamp) ->
            let updatedPending =
                match domain with
                | "demand" ->
                    { state.PendingChanges with
                        DemandChanged = true
                        LastChangeTime = Some timestamp }
                | "supply" ->
                    { state.PendingChanges with
                        SupplyChanged = true
                        LastChangeTime = Some timestamp }
                | "inventory" ->
                    { state.PendingChanges with
                        InventoryChanged = true
                        LastChangeTime = Some timestamp }
                | _ -> state.PendingChanges

            if state.CompositionInProgress then
                ({ state with
                    PendingChanges = updatedPending },
                 [])
            else
                match state.DebounceStartedAt with
                | None ->
                    let newState =
                        { state with
                            PendingChanges = updatedPending
                            DebounceStartedAt = Some timestamp }

                    (newState, [ StartDebounceWindow ])
                | Some _ ->
                    let newState =
                        { state with
                            PendingChanges = updatedPending
                            DebounceStartedAt = Some timestamp }

                    (newState, [ ResetDebounceWindow ])

        | DebounceWindowExpired _ ->
            if state.PendingChanges.HasChanges && not state.CompositionInProgress then
                let newState =
                    { state with
                        CompositionInProgress = true
                        DebounceStartedAt = None }

                (newState, [ TriggerRecomposition ])
            else
                ({ state with DebounceStartedAt = None }, [])

        | CompositionCompleted _ ->
            let newState =
                { state with
                    PendingChanges = PendingChanges.Empty
                    CompositionInProgress = false
                    DebounceStartedAt = None }

            (newState, [])

        | CompositionFailed(reason, _) ->
            let newState =
                { state with
                    CompositionInProgress = false
                    DebounceStartedAt = None }

            (newState, [ LogCompositionFailure reason ])

    /// Creates and subscribes the picture recomposition workflow
    let create (deps: WorkflowDependencies) (cancellationToken: CancellationToken) : Task<IDisposable> =

        task {
            let mutable currentState: WorkflowState =
                { PlanningScopeId = deps.PlanningScopeId
                  PendingChanges = PendingChanges.Empty
                  CompositionInProgress = false
                  DebounceStartedAt = None }

            let handleMaterialChange (domain: string) (envelope: Envelope) : Task<unit> =
                task {
                    let currentTime = deps.GetCurrentTime()
                    let event = MaterialChangeDetected(domain, currentTime)
                    let newState, actions = step currentState event
                    currentState <- newState

                    for action in actions do
                        match action with
                        | StartDebounceWindow
                        | ResetDebounceWindow ->
                            // Infrastructure scheduler handles debounce timing
                            ()
                        | TriggerRecomposition ->
                            // Query all three bounded contexts to gather references
                            let! demandRefs = deps.GetActiveDemandReferences deps.PlanningScopeId
                            let! supplyRefs = deps.GetAvailableSupplyReferences deps.PlanningScopeId
                            let! inventoryRefs = deps.GetCurrentInventoryReferences deps.PlanningScopeId

                            // Call Compose with the gathered references
                            let req: ComposePictureVersionReq =
                                { PlanningScopeId = Identities.planningScopeIdValue deps.PlanningScopeId
                                  DemandReferences = demandRefs |> List.map Identities.demandIdValue
                                  SupplyReferences = supplyRefs |> List.map Identities.supplyIdValue
                                  InventoryReferences =
                                    inventoryRefs
                                    |> List.map(fun inv ->
                                        sprintf
                                            "%s:%s:%s"
                                            (Identities.itemIdValue inv.Item)
                                            (Identities.locationIdValue inv.Location)
                                            (Identities.batchIdentifierValue inv.Batch))
                                  CompositionTime = System.DateTimeOffset.UtcNow }

                            let! result = deps.EnterprisePictureApi.Compose req

                            match result with
                            | Ok _ ->
                                let completedEvent = CompositionCompleted(deps.GetCurrentTime())
                                let newState2, _ = step currentState completedEvent
                                currentState <- newState2
                            | Error err ->
                                let failedEvent = CompositionFailed(sprintf "%A" err, deps.GetCurrentTime())
                                let newState2, _ = step currentState failedEvent
                                currentState <- newState2
                        | LogCompositionFailure reason -> printfn $"[Workflow FS-C-019] Composition failed: {reason}"
                }

            let handleDemandPublished (envelope: Envelope) : Task<unit> =
                task {
                    match deps.Codec.Decode envelope.DataJson with
                    | Ok notification when notification.MaterialChangeDetected ->
                        do! handleMaterialChange "demand" envelope
                    | _ -> ()
                }

            let handleSupplyPublished (envelope: Envelope) : Task<unit> =
                task {
                    match deps.Codec.Decode envelope.DataJson with
                    | Ok notification when notification.MaterialChangeDetected ->
                        do! handleMaterialChange "supply" envelope
                    | _ -> ()
                }

            let handleInventoryPublished (envelope: Envelope) : Task<unit> =
                task {
                    match deps.Codec.Decode envelope.DataJson with
                    | Ok notification when notification.MaterialChangeDetected ->
                        do! handleMaterialChange "inventory" envelope
                    | _ -> ()
                }

            let handleCompositionCompleted (envelope: Envelope) : Task<unit> =
                task {
                    let currentTime = deps.GetCurrentTime()
                    let event = CompositionCompleted currentTime
                    let newState, _ = step currentState event
                    currentState <- newState
                }

            let! demandSubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ BusinessNotifications.demandUnderstandingPublished.Id ])
                    handleDemandPublished
                    cancellationToken

            let! supplySubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ BusinessNotifications.supplyUnderstandingPublished.Id ])
                    handleSupplyPublished
                    cancellationToken

            let! inventorySubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ BusinessNotifications.inventorySnapshotPublished.Id ])
                    handleInventoryPublished
                    cancellationToken

            let! compositionSubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ EnterpriseEvents.pictureVersionPublished.Id ])
                    handleCompositionCompleted
                    cancellationToken

            return
                { new IDisposable with
                    member _.Dispose() =
                        demandSubscription.Dispose()
                        supplySubscription.Dispose()
                        inventorySubscription.Dispose()
                        compositionSubscription.Dispose() }
        }
