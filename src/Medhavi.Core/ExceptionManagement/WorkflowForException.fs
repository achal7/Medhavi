/// CA-C-020 Exception Management Workflows
module Medhavi.Core.ExceptionManagement.Workflows.Evidence

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Core.Exception
open Medhavi.Core.ArsIdentifiers

type WorkflowDependencies =
    {
        /// Codec for decoding demand exception evidence notifications
        DemandCodec: Codec<DemandExceptionEvidenceNotification>
        /// Codec for decoding supply exception evidence notifications
        SupplyCodec: Codec<SupplyExceptionEvidenceNotification>
        /// Codec for decoding inventory exception evidence notifications
        InventoryCodec: Codec<InventoryExceptionEvidenceNotification>
        /// Subscription function from envelope store
        Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
        /// The public API for exception management
        ExceptionApi: ExceptionApi
    }

/// Translates a demand exception evidence notification to a register request
let private translateDemandToRegisterReq (notification: DemandExceptionEvidenceNotification) : RegisterExceptionReq =
    { ExceptionId = Identities.exceptionIdValue notification.ExceptionId
      ConstraintReference = notification.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue notification.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue notification.AffectedScopeType
      AffectedScopeIdentifier = notification.AffectedScopeIdentifier
      EvidenceReference = notification.EvidenceReference
      Severity = notification.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value notification.EvidenceTime }

/// Translates a supply exception evidence notification to a register request
let private translateSupplyToRegisterReq (notification: SupplyExceptionEvidenceNotification) : RegisterExceptionReq =
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
    : RegisterExceptionReq =
    { ExceptionId = Identities.exceptionIdValue notification.ExceptionId
      ConstraintReference = notification.ConstraintReference
      Classification = Identities.vocabularyEntryIdValue notification.Classification
      AffectedScopeType = Identities.vocabularyEntryIdValue notification.AffectedScopeType
      AffectedScopeIdentifier = notification.AffectedScopeIdentifier
      EvidenceReference = notification.EvidenceReference
      Severity = notification.Severity |> Option.map Identities.vocabularyEntryIdValue
      RegistrationTime = Timestamp.value notification.EvidenceTime }

/// FS-C-020a: Creates and subscribes the exception evidence ingestion workflow
/// Returns an IDisposable that unsubscribes from all event streams when disposed
let create (deps: WorkflowDependencies) (cancellationToken: CancellationToken) : Task<IDisposable> =

    task {
        // Handler for demand exception evidence
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

        // Handler for supply exception evidence
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

        // Handler for inventory exception evidence
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

        // Subscribe to all three event types
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

        // Return composite disposable
        return
            { new IDisposable with
                member _.Dispose() =
                    demandSubscription.Dispose()
                    supplySubscription.Dispose()
                    inventorySubscription.Dispose() }
    }
