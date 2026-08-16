/// CA-C-020 Exception Management Workflows
/// FS-C-020a: Exception Evidence Ingestion Workflow
/// Stateless reactive handler that translates cross-domain exception evidence
module Medhavi.Core.ExceptionManagement.Workflows.ExceptionEvidenceIngestion

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Contracts.Core
open Medhavi.Core.ArsIdentifiers
open Medhavi.Core.ExceptionManagement.Policies

/// Dependencies required by this workflow
type WorkflowDependencies =
    { DemandCodec: Codec<DemandExceptionEvidenceNotification>
      SupplyCodec: Codec<SupplyExceptionEvidenceNotification>
      InventoryCodec: Codec<InventoryExceptionEvidenceNotification>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ExceptionApi: Exception.ExceptionApi }

/// Translates a demand exception evidence notification to a register request
let private translateDemandToRegisterReq
    (notification: DemandExceptionEvidenceNotification)
    : Exception.RegisterExceptionReq =
    { ExceptionId = Identities.exceptionIdValue notification.ExceptionId
      ConstraintReference = notification.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue notification.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue notification.AffectedScopeType
      AffectedScopeIdentifier = notification.AffectedScopeIdentifier
      EvidenceReference = notification.EvidenceReference
      Severity = notification.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value notification.EvidenceTime }

/// Translates a supply exception evidence notification to a register request
let private translateSupplyToRegisterReq
    (notification: SupplyExceptionEvidenceNotification)
    : Exception.RegisterExceptionReq =
    { ExceptionId = Identities.exceptionIdValue notification.ExceptionId
      ConstraintReference = notification.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue notification.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue notification.AffectedScopeType
      AffectedScopeIdentifier = notification.AffectedScopeIdentifier
      EvidenceReference = notification.EvidenceReference
      Severity = notification.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value notification.EvidenceTime }

/// Translates an inventory exception evidence notification to a register request
let private translateInventoryToRegisterReq
    (notification: InventoryExceptionEvidenceNotification)
    : Exception.RegisterExceptionReq =
    { ExceptionId = Identities.exceptionIdValue notification.ExceptionId
      ConstraintReference = notification.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue notification.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue notification.AffectedScopeType
      AffectedScopeIdentifier = notification.AffectedScopeIdentifier
      EvidenceReference = notification.EvidenceReference
      Severity = notification.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value notification.EvidenceTime }

/// FS-C-020a: Creates and subscribes the exception evidence ingestion workflow
let create (deps: WorkflowDependencies) (cancellationToken: CancellationToken) : Task<IDisposable> =

    task {
        let handleDemandEvidence (envelope: Envelope) : Task<unit> =
            task {
                match deps.DemandCodec.Decode envelope.DataJson with
                | Ok notification ->
                    let req = translateDemandToRegisterReq notification
                    let! result = deps.ExceptionApi.Register req

                    match result with
                    | Ok _ -> return ()
                    | Error err -> printfn $"[Workflow FS-C-020a] Failed to register demand exception: {err}"
                | Error err -> printfn $"[Workflow FS-C-020a] Failed to decode demand exception evidence: {err}"
            }

        let handleSupplyEvidence (envelope: Envelope) : Task<unit> =
            task {
                match deps.SupplyCodec.Decode envelope.DataJson with
                | Ok notification ->
                    let req = translateSupplyToRegisterReq notification
                    let! result = deps.ExceptionApi.Register req

                    match result with
                    | Ok _ -> return ()
                    | Error err -> printfn $"[Workflow FS-C-020a] Failed to register supply exception: {err}"
                | Error err -> printfn $"[Workflow FS-C-020a] Failed to decode supply exception evidence: {err}"
            }

        let handleInventoryEvidence (envelope: Envelope) : Task<unit> =
            task {
                match deps.InventoryCodec.Decode envelope.DataJson with
                | Ok notification ->
                    let req = translateInventoryToRegisterReq notification
                    let! result = deps.ExceptionApi.Register req

                    match result with
                    | Ok _ -> return ()
                    | Error err -> printfn $"[Workflow FS-C-020a] Failed to register inventory exception: {err}"
                | Error err -> printfn $"[Workflow FS-C-020a] Failed to decode inventory exception evidence: {err}"
            }

        let! demandSubscription =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.demandExceptionEvidence.Id ])
                handleDemandEvidence
                cancellationToken

        let! supplySubscription =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.supplyExceptionEvidence.Id ])
                handleSupplyEvidence
                cancellationToken

        let! inventorySubscription =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.inventoryExceptionEvidence.Id ])
                handleInventoryEvidence
                cancellationToken

        return
            { new IDisposable with
                member _.Dispose() =
                    demandSubscription.Dispose()
                    supplySubscription.Dispose()
                    inventorySubscription.Dispose() }
    }

/// FS-C-020b: Exception SLA Escalation Workflow
/// Stateful process manager that monitors unresolved exceptions and escalates
/// them when SLA thresholds are breached based on severity level
module ExceptionSlaEscalation =

    /// Monitoring state for a single exception being tracked for SLA
    type SlaMonitoringState =
        { ExceptionId: ExceptionId
          Severity: string option
          RegisteredAt: Timestamp
          SlaDeadline: Timestamp
          WarningIssued: bool
          Escalated: bool }

    /// Workflow state: a map of exception IDs to their monitoring state
    type WorkflowState = Map<ExceptionId, SlaMonitoringState>

    /// Events that trigger workflow state transitions
    type WorkflowEvent =
        | ExceptionRegisteredEvent of ExceptionId * Severity: string option * Timestamp
        | ExceptionResolvedEvent of ExceptionId * Timestamp
        | CheckSlaDeadlinesEvent of CurrentTime: Timestamp

    /// Actions emitted by the workflow
    type WorkflowAction =
        | IssueSlaWarning of ExceptionId * Severity: string option
        | EscalateException of ExceptionId * Severity: string option * OverdueBy: TimeSpan
        | CancelMonitoring of ExceptionId

    /// Dependencies required by this workflow
    type WorkflowDependencies =
        {
            Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
            ExceptionApi: Exception.ExceptionApi
            Policy: ExceptionManagementPolicy
            /// Publishes SLA escalation notifications to downstream systems
            PublishEscalation: ExceptionId -> string option -> TimeSpan -> Task<unit>
            /// Publishes SLA warning notifications to downstream systems
            PublishWarning: ExceptionId -> string option -> Task<unit>
            /// Provides the current time (injected for testability)
            GetCurrentTime: unit -> Timestamp
        }

    /// Calculates the SLA deadline based on registration time, severity, and policy
    let private calculateSlaDeadline
        (registeredAt: Timestamp)
        (severity: string option)
        (policy: ExceptionManagementPolicy)
        : Timestamp =
        let slaHours = resolveSlaHours severity policy.SlaThresholds
        let deadlineOffset = TimeSpan.FromHours(float slaHours)
        Timestamp.add registeredAt deadlineOffset

    /// Checks if the warning threshold has been reached
    let private isWarningThresholdReached
        (registeredAt: Timestamp)
        (deadline: Timestamp)
        (currentTime: Timestamp)
        (warningPercent: int)
        : bool =
        let totalDuration = Timestamp.diff deadline registeredAt
        let elapsed = Timestamp.diff currentTime registeredAt
        let threshold = TimeSpan.FromTicks(int64(float totalDuration.Ticks * float warningPercent / 100.0))
        elapsed >= threshold && currentTime < deadline

    /// Checks if the SLA deadline has been breached
    let private isSlaBreached (deadline: Timestamp) (currentTime: Timestamp) : bool = currentTime >= deadline

    /// Pure step function: processes an event and returns new state + actions
    /// This is the core state machine logic - no side effects
    let step
        (state: WorkflowState)
        (event: WorkflowEvent)
        (policy: ExceptionManagementPolicy)
        : WorkflowState * WorkflowAction list =

        match event with
        | ExceptionRegisteredEvent(exceptionId, severity, registeredAt) ->
            // Start monitoring this exception
            let deadline = calculateSlaDeadline registeredAt severity policy

            let monitoringState =
                { ExceptionId = exceptionId
                  Severity = severity
                  RegisteredAt = registeredAt
                  SlaDeadline = deadline
                  WarningIssued = false
                  Escalated = false }

            let newState = state |> Map.add exceptionId monitoringState
            (newState, [])

        | ExceptionResolvedEvent(exceptionId, _) ->
            // Stop monitoring this exception
            let newState = state |> Map.remove exceptionId
            (newState, [ CancelMonitoring exceptionId ])

        | CheckSlaDeadlinesEvent currentTime ->
            // Evaluate all monitored exceptions for SLA breaches
            let actions =
                state
                |> Map.toList
                |> List.collect(fun (_, monitoring) ->
                    if monitoring.Escalated then
                        [] // Already escalated, no further action
                    elif isSlaBreached monitoring.SlaDeadline currentTime then
                        // SLA breached - escalate
                        let overdueBy = Timestamp.diff currentTime monitoring.SlaDeadline
                        [ EscalateException(monitoring.ExceptionId, monitoring.Severity, overdueBy) ]
                    elif
                        not monitoring.WarningIssued
                        && isWarningThresholdReached
                            monitoring.RegisteredAt
                            monitoring.SlaDeadline
                            currentTime
                            policy.WarningThresholdPercent
                    then
                        // Warning threshold reached
                        [ IssueSlaWarning(monitoring.ExceptionId, monitoring.Severity) ]
                    else
                        [])

            // Update state based on actions
            let newState =
                actions
                |> List.fold
                    (fun currentState action ->
                        match action with
                        | IssueSlaWarning(exceptionId, _) ->
                            currentState
                            |> Map.change exceptionId (Option.map(fun m -> { m with WarningIssued = true }))
                        | EscalateException(exceptionId, _, _) ->
                            currentState |> Map.change exceptionId (Option.map(fun m -> { m with Escalated = true }))
                        | CancelMonitoring exceptionId -> currentState |> Map.remove exceptionId)
                    state

            (newState, actions)

    /// Creates and subscribes the SLA escalation workflow
    /// Returns an IDisposable that unsubscribes from all event streams when disposed
    let create (deps: WorkflowDependencies) (cancellationToken: CancellationToken) : Task<IDisposable> =

        task {
            // Mutable workflow state (managed by the workflow, not the domain aggregate)
            let mutable currentState: WorkflowState = Map.empty

            // Handler for exception registered events
            let handleExceptionRegistered (envelope: Envelope) : Task<unit> =
                task {
                    // Decode the exception registered notification
                    match Medhavi.Foundation.Codec.json<ExceptionRegisteredNotification>.Decode envelope.DataJson with
                    | Ok notification ->
                        let event =
                            ExceptionRegisteredEvent(
                                notification.ExceptionId,
                                notification.Severity |> Option.map Identities.vocabularyEntryIdValue,
                                notification.RegistrationTime
                            )

                        let newState, actions = step currentState event deps.Policy
                        currentState <- newState

                        // Execute actions
                        for action in actions do
                            match action with
                            | IssueSlaWarning(exceptionId, severity) -> do! deps.PublishWarning exceptionId severity
                            | EscalateException(exceptionId, severity, overdueBy) ->
                                do! deps.PublishEscalation exceptionId severity overdueBy
                            | CancelMonitoring _ -> ()
                    | Error err -> printfn $"[Workflow FS-C-020b] Failed to decode exception registered event: {err}"
                }

            // Handler for exception resolved events
            let handleExceptionResolved (envelope: Envelope) : Task<unit> =
                task {
                    match Medhavi.Foundation.Codec.json<ExceptionResolvedNotification>.Decode envelope.DataJson with
                    | Ok notification ->
                        let event = ExceptionResolvedEvent(notification.ExceptionId, notification.ResolutionTime)
                        let newState, actions = step currentState event deps.Policy
                        currentState <- newState
                    | Error err -> printfn $"[Workflow FS-C-020b] Failed to decode exception resolved event: {err}"
                }

            // Handler for periodic SLA checks
            // This should be triggered by a scheduler or timer service
            let handleCheckSlaDeadlines () : Task<unit> =
                task {
                    let currentTime = deps.GetCurrentTime()
                    let event = CheckSlaDeadlinesEvent currentTime
                    let newState, actions = step currentState event deps.Policy
                    currentState <- newState

                    // Execute actions
                    for action in actions do
                        match action with
                        | IssueSlaWarning(exceptionId, severity) -> do! deps.PublishWarning exceptionId severity
                        | EscalateException(exceptionId, severity, overdueBy) ->
                            do! deps.PublishEscalation exceptionId severity overdueBy
                        | CancelMonitoring _ -> ()
                }

            // Subscribe to exception lifecycle events
            let! registeredSubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ EnterpriseEvents.exceptionRegistered.Id ])
                    handleExceptionRegistered
                    cancellationToken

            let! resolvedSubscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ EnterpriseEvents.exceptionResolved.Id ])
                    handleExceptionResolved
                    cancellationToken

            // Note: The periodic SLA check should be triggered by a scheduler service.
            // For now, we expose the check function for external invocation.
            // In production, Nexus will wire a timer that calls handleCheckSlaDeadlines periodically.

            return
                { new IDisposable with
                    member _.Dispose() =
                        registeredSubscription.Dispose()
                        resolvedSubscription.Dispose() }
        }
