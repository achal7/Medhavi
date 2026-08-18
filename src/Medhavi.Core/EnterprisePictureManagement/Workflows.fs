/// CA-C-019 Workflows
/// FS-C-001 = SCHEDULED composition (PO-C-001 cadence).
/// FS-C-002 = EVENT-DRIVEN publication, triggered by EV-C-001, invoking AB-C-002 (materiality-gated).
module Medhavi.Core.EnterprisePictureManagement.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Core
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Core.ArsIdentifiers

/// FS-C-001 dependencies. The scheduler (infrastructure) invokes RunOnce per cadence.
type CompositionDependencies =
    { EnterprisePictureApi: EnterprisePictureApi
      PlanningScopeId: PlanningScopeId
      GetCurrentTime: unit -> Timestamp
      GetActiveDemandReferences: PlanningScopeId -> Task<DemandId list>
      GetAvailableSupplyReferences: PlanningScopeId -> Task<SupplyId list>
      GetCurrentInventoryReferences: PlanningScopeId -> Task<InventoryIdentity list> }

module PictureComposition =
    /// One scheduled composition cycle (FS-C-001). Pure orchestration; no business reasoning here.
    let runOnce (deps: CompositionDependencies) : Task<Result<unit, string>> =
        task {
            let! demandRefs = deps.GetActiveDemandReferences deps.PlanningScopeId
            let! supplyRefs = deps.GetAvailableSupplyReferences deps.PlanningScopeId
            let! inventoryRefs = deps.GetCurrentInventoryReferences deps.PlanningScopeId

            let req: ComposePictureVersionReq =
                { PlanningScopeId = PlanningScopeId.value deps.PlanningScopeId
                  DemandReferences = demandRefs |> List.map DemandId.value
                  SupplyReferences = supplyRefs |> List.map SupplyId.value
                  InventoryReferences = inventoryRefs |> List.map InventoryIdentity.toString
                  CompositionTime = System.DateTimeOffset.UtcNow }

            let! result = deps.EnterprisePictureApi.Compose req
            return result |> Result.map(fun _ -> ()) |> Result.mapError(sprintf "%A")
        }

/// FS-C-002: publication pipeline. AB-C-002 internally applies BA-C-001 + DE-C-001.
/// The notification node publishes BN-C-001 only after EV-C-002 exists.
module PicturePublication =
    type PublicationDependencies =
        { EnterprisePictureApi: EnterprisePictureApi
          Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
          Codec: Medhavi.Common.Codec<PictureVersionComposedNotification> }

    let create (deps: PublicationDependencies) (ct: CancellationToken) : Task<IDisposable> =
        task {
            let handleComposed (envelope: Envelope) : Task<unit> =
                task {
                    match deps.Codec.Decode envelope.DataJson with
                    | Ok composed ->
                        // Invoke AB-C-002 via the public API; materiality gate is inside the behavior.
                        let pubReq: PublishPictureVersionReq =
                            { PlanningScopeId = composed.PlanningScopeId
                              VersionNumber = composed.VersionNumber
                              PublicationTime = System.DateTimeOffset.UtcNow }

                        let! _ = deps.EnterprisePictureApi.Publish pubReq
                        return ()
                    | Error _ -> return ()
                }

            let! subscription =
                deps.Subscribe
                    (EnvelopeFilter.EventTypes [ EnterpriseEvents.pictureVersionComposed.Id ])
                    handleComposed
                    ct

            return subscription
        }
