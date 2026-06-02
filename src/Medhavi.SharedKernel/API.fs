namespace Medhavi.SharedKernel.API

open Medhavi.Common.Patterns
open Medhavi.Contracts
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Projections

type UomApi =
    { Define: UomDefineReq -> TaskResult<UnitOfMeasure, ApplicationError>
      DefineBulk: UomDefineReq list -> TaskResult<UnitOfMeasure list, ApplicationError>
      ChangeConversionFactor: UomChangeConversionFactorReq -> TaskResult<UnitOfMeasure, ApplicationError>
      Retire: string -> TaskResult<UnitOfMeasure, ApplicationError>
      Activate: string -> TaskResult<UnitOfMeasure, ApplicationError> }

type UnitConversionApi =
    { Define: UnitConversionDefineReq -> TaskResult<UnitConversion, ApplicationError>
      DefineBulk: UnitConversionDefineReq list -> TaskResult<UnitConversion list, ApplicationError>
      UpdateRatio: UnitConversionUpdateReq -> TaskResult<UnitConversion, ApplicationError>
      Retire: UnitConversionRetireReq -> TaskResult<UnitConversion, ApplicationError> }

type PlantApi =
    { Define: PlantDefineReq -> TaskResult<Plant, ApplicationError>
      DefineBulk: PlantDefineReq list -> TaskResult<Plant list, ApplicationError>
      Rename: PlantRenameReq -> TaskResult<Plant, ApplicationError>
      Retire: PlantRetireReq -> TaskResult<Plant, ApplicationError> }

type StockingPointApi =
    { Define: StockingPointDefineReq -> TaskResult<StockingPoint, ApplicationError>
      DefineBulk: StockingPointDefineReq list -> TaskResult<StockingPoint list, ApplicationError>
      Rename: StockingPointRenameReq -> TaskResult<StockingPoint, ApplicationError>
      Retire: StockingPointRetireReq -> TaskResult<StockingPoint, ApplicationError> }

type SkuApi =
    { Define: SkuDefineReq -> TaskResult<Sku, ApplicationError>
      DefineBulk: SkuDefineReq list -> TaskResult<Sku list, ApplicationError>
      Rename: SkuRenameReq -> TaskResult<Sku, ApplicationError>
      Retire: SkuRetireReq -> TaskResult<Sku, ApplicationError> }

type BomApi =
    { Define: BomDefineReq -> TaskResult<Bom, ApplicationError>
      DefineBulk: BomDefineReq list -> TaskResult<Bom list, ApplicationError>
      Activate: BomActivateReq -> TaskResult<Bom, ApplicationError>
      Deactivate: BomDeactivateReq -> TaskResult<Bom, ApplicationError> }

type ResourceGroupApi =
    { Define: ResourceGroupDefineReq -> TaskResult<ResourceGroup, ApplicationError>
      DefineBulk: ResourceGroupDefineReq list -> TaskResult<ResourceGroup list, ApplicationError>
      Rename: ResourceGroupRenameReq -> TaskResult<ResourceGroup, ApplicationError>
      Retire: ResourceGroupRetireReq -> TaskResult<ResourceGroup, ApplicationError> }

type StandardResourceApi =
    { Define: StandardResourceDefineReq -> TaskResult<StandardResource, ApplicationError>
      DefineBulk: StandardResourceDefineReq list -> TaskResult<StandardResource list, ApplicationError>
      Rename: StandardResourceRenameReq -> TaskResult<StandardResource, ApplicationError>
      Retire: StandardResourceRetireReq -> TaskResult<StandardResource, ApplicationError> }

type PhysicalResourceApi =
    { Define: PhysicalResourceDefineReq -> TaskResult<PhysicalResource, ApplicationError>
      DefineBulk: PhysicalResourceDefineReq list -> TaskResult<PhysicalResource list, ApplicationError>
      Rename: PhysicalResourceRenameReq -> TaskResult<PhysicalResource, ApplicationError>
      Retire: PhysicalResourceRetireReq -> TaskResult<PhysicalResource, ApplicationError> }

type RoutingApi =
    { Define: RoutingDefineReq -> TaskResult<Routing, ApplicationError>
      DefineBulk: RoutingDefineReq list -> TaskResult<Routing list, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<Routing, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<Routing, ApplicationError> }

type TransportLegApi =
    { Define: TransportLegDefineReq -> TaskResult<TransportLeg, ApplicationError>
      DefineBulk: TransportLegDefineReq list -> TaskResult<TransportLeg list, ApplicationError>
      Update: TransportLegUpdateReq -> TaskResult<TransportLeg, ApplicationError>
      Deactivate: TransportLegDeactivateReq -> TaskResult<TransportLeg, ApplicationError> }

type InventoryApi =
    { Define: InventoryDefineReq -> TaskResult<Inventory, ApplicationError>
      DefineBulk: InventoryDefineReq list -> TaskResult<Inventory list, ApplicationError>
      Remove: string -> TaskResult<Inventory, ApplicationError> }

type InventoryTargetApi =
    { Define: InventoryTargetDefineReq -> TaskResult<InventoryTarget, ApplicationError>
      DefineBulk: InventoryTargetDefineReq list -> TaskResult<InventoryTarget list, ApplicationError>
      Update: InventoryTargetUpdateReq -> TaskResult<InventoryTarget, ApplicationError>
      Activate: string -> TaskResult<InventoryTarget, ApplicationError>
      Deactivate: string -> TaskResult<InventoryTarget, ApplicationError> }

type SupplierOfferApi =
    { Define: SupplierOfferDefineReq -> TaskResult<SupplierOffer, ApplicationError>
      DefineBulk: SupplierOfferDefineReq list -> TaskResult<SupplierOffer list, ApplicationError>
      Update: SupplierOfferUpdateReq -> TaskResult<SupplierOffer, ApplicationError>
      Revoke: string -> TaskResult<SupplierOffer, ApplicationError>
      ChangeStatus: SupplierOfferChangeStatusReq -> TaskResult<SupplierOffer, ApplicationError> }

type SupplyOrderApi =
    { Create: SupplyOrderCreateReq -> TaskResult<SupplyOrder, ApplicationError>
      CreateBulk: SupplyOrderCreateReq list -> TaskResult<SupplyOrder list, ApplicationError>
      ProcessStatusUpdates: SupplyOrderUpdateReq list -> TaskResult<SupplyOrder list, ApplicationError>
      Start: SupplyOrderStartReq -> TaskResult<SupplyOrder, ApplicationError>
      PartialComplete: SupplyOrderPartialCompleteReq -> TaskResult<SupplyOrder, ApplicationError>
      Complete: SupplyOrderCompleteReq -> TaskResult<SupplyOrder, ApplicationError>
      Plan: SupplyOrderPlanReq -> TaskResult<SupplyOrder, ApplicationError>
      Confirm: SupplyOrderConfirmReq -> TaskResult<SupplyOrder, ApplicationError>
      Release: SupplyOrderReleaseReq -> TaskResult<SupplyOrder, ApplicationError>
      Cancel: SupplyOrderCancelReq -> TaskResult<SupplyOrder, ApplicationError>
      Lock: SupplyOrderLockReq -> TaskResult<SupplyOrder, ApplicationError> }

type MaterialProviderApi =
    { GetSnapshot: string -> string -> Timestamp -> Async<Result<MaterialSnapshot, ApplicationError>>
      GetNetAvailable: string -> string -> Timestamp -> Async<Result<decimal, ApplicationError>>
      GetTimePhasedAvailability:
          string -> string -> Timestamp -> int -> int -> Async<Result<(Timestamp * decimal) list, ApplicationError>>
      GetDateWiseAvailability:
          string -> string -> Timestamp -> int -> Async<Result<(Timestamp * decimal) list, ApplicationError>>
      GetSupplierOptions:
          string -> string option -> decimal -> Timestamp -> Async<Result<SupplierOffer list, ApplicationError>> }

type MaterialReservationApi =
    { CreateTentative: MaterialReservationCreateReq -> TaskResult<MaterialReservation, ApplicationError>
      Confirm: MaterialReservationConfirmReq -> TaskResult<MaterialReservation, ApplicationError>
      Release: MaterialReservationReleaseReq -> TaskResult<MaterialReservation, ApplicationError>
      Reduce: MaterialReservationReduceReq -> TaskResult<MaterialReservation, ApplicationError>
      Expire: MaterialReservationExpireReq -> TaskResult<MaterialReservation, ApplicationError> }

