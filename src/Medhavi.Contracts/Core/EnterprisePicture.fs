namespace Medhavi.Contracts.Core

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-C-021 Picture Version Data Transfer Object
type PictureVersion =
    { VersionNumber: int
      DemandReferences: string list
      SupplyReferences: string list
      InventoryReferences: string list
      CompositionTime: DateTimeOffset
      PublicationTime: DateTimeOffset option
      LifecycleState: string }

/// SE-C-021 Enterprise Picture Data Transfer Object
type EnterprisePicture =
    { PlanningScopeId: string
      Versions: PictureVersion list
      CurrentPublishedVersion: int option }

/// External request to compose a new Enterprise Picture Version
type ComposePictureVersionReq =
    { PlanningScopeId: string
      DemandReferences: string list
      SupplyReferences: string list
      InventoryReferences: string list
      CompositionTime: DateTimeOffset }

/// External request to publish an Enterprise Picture Version
type PublishPictureVersionReq =
    { PlanningScopeId: string
      VersionNumber: int
      PublicationTime: DateTimeOffset }

/// Material change summary for Enterprise Picture publication
type MaterialChangeSummary =
    { DemandChanged: bool
      SupplyChanged: bool
      InventoryChanged: bool
      ChangeDetails: Map<string, string> }

/// BN-C-019a: Enterprise Picture Published Notification
type EnterprisePicturePublishedNotification =
    { PlanningScopeId: string
      Version: int
      SupersededVersion: int option
      PublicationTime: DateTimeOffset
      DemandReferences: string list
      SupplyReferences: string list
      InventoryReferences: string list
      MaterialChangeSummary: MaterialChangeSummary
      PeriodicRefreshFlag: bool }

/// Public API for Enterprise Picture Management (CA-C-019)
type EnterprisePictureApi =
    { Compose: ComposePictureVersionReq -> Task<Result<EnterprisePicture, ApiError>>
      Publish: PublishPictureVersionReq -> Task<Result<EnterprisePicture, ApiError>> }

/// Query service alias for Enterprise Picture
type EnterprisePictureQueries = QueryService<EnterprisePicture, string>
