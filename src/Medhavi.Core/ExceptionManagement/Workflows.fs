/// CA-C-020 Workflows
/// FS-C-003: Exception evidence ingestion (detection).
/// FS-C-004: Exception resolution ingestion.
module Medhavi.Core.ExceptionManagement.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Contracts.Core.Exception
open Medhavi.Core.ArsIdentifiers

type EvidenceWorkflowDependencies =
    { DemandCodec: Codec<DemandExceptionEvidenceNotification>
      SupplyCodec: Codec<SupplyExceptionEvidenceNotification>
      InventoryCodec: Codec<InventoryExceptionEvidenceNotification>
      Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      ExceptionApi: ExceptionApi }

let private toDemandEvidenceReq (n: DemandExceptionEvidenceNotification) : ExceptionEvidenceReq =
    { ConstraintReference = n.ConstraintReference
      Classification = VocabularyEntryId.value n.Classification
      AffectedScopeType = VocabularyEntryId.value n.AffectedScopeType
      AffectedScopeIdentifier = n.AffectedScopeIdentifier
      EvidenceReference = n.EvidenceReference
      Severity = n.Severity |> Option.map VocabularyEntryId.value
      EvidenceTime = Timestamp.value n.EvidenceTime }

let private toSupplyEvidenceReq (n: SupplyExceptionEvidenceNotification) : ExceptionEvidenceReq =
    { ConstraintReference = n.ConstraintReference
      Classification = VocabularyEntryId.value n.Classification
      AffectedScopeType = VocabularyEntryId.value n.AffectedScopeType
      AffectedScopeIdentifier = n.AffectedScopeIdentifier
      EvidenceReference = n.EvidenceReference
      Severity = n.Severity |> Option.map VocabularyEntryId.value
      EvidenceTime = Timestamp.value n.EvidenceTime }

let private toInventoryEvidenceReq (n: InventoryExceptionEvidenceNotification) : ExceptionEvidenceReq =
    { ConstraintReference = n.ConstraintReference
      Classification = VocabularyEntryId.value n.Classification
      AffectedScopeType = VocabularyEntryId.value n.AffectedScopeType
      AffectedScopeIdentifier = n.AffectedScopeIdentifier
      EvidenceReference = n.EvidenceReference
      Severity = n.Severity |> Option.map VocabularyEntryId.value
      EvidenceTime = Timestamp.value n.EvidenceTime }

/// FS-C-003: ingest detection evidence from all authorized domains.
let createEvidenceWorkflow (deps: EvidenceWorkflowDependencies) (ct: CancellationToken) : Task<IDisposable> =
    task {
        let processEnvelope (codec: Codec<'n>) (map: 'n -> ExceptionEvidenceReq) (envelope: Envelope) : Task<unit> =
            task {
                match codec.Decode envelope.DataJson with
                | Ok notification ->
                    let req = map notification
                    let! _ = deps.ExceptionApi.ProcessEvidence req
                    return ()
                | Error _ -> return ()
            }

        let! demandSub =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.demandExceptionEvidence.Id ])
                (processEnvelope deps.DemandCodec toDemandEvidenceReq)
                ct

        let! supplySub =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.supplyExceptionEvidence.Id ])
                (processEnvelope deps.SupplyCodec toSupplyEvidenceReq)
                ct

        let! inventorySub =
            deps.Subscribe
                (EnvelopeFilter.EventTypes [ BusinessNotifications.inventoryExceptionEvidence.Id ])
                (processEnvelope deps.InventoryCodec toInventoryEvidenceReq)
                ct

        return
            { new IDisposable with
                member _.Dispose() =
                    demandSub.Dispose()
                    supplySub.Dispose()
                    inventorySub.Dispose() }
    }
