namespace Medhavi.Contracts.API

open System
open System.Threading.Tasks
open Medhavi.Contracts
open Medhavi.Contracts.Supply
open Medhavi.Contracts.MasterData
open Medhavi.Contracts.Transport
open Medhavi.Contracts.Integration

type UomApi =
    { Define: UomDefineReq -> Task<Result<UnitOfMeasure, ApiError>>
      DefineBulk: UomDefineReq list -> Task<Result<UnitOfMeasure list, ApiError>>
      ChangeConversionFactor: UomChangeConversionFactorReq -> Task<Result<UnitOfMeasure, ApiError>>
      Retire: string -> Task<Result<UnitOfMeasure, ApiError>>
      Activate: string -> Task<Result<UnitOfMeasure, ApiError>> }

type UnitConversionApi =
    { Define: UnitConversionDefineReq -> Task<Result<UnitConversion, ApiError>>
      DefineBulk: UnitConversionDefineReq list -> Task<Result<UnitConversion list, ApiError>>
      UpdateRatio: UnitConversionUpdateReq -> Task<Result<UnitConversion, ApiError>>
      Retire: UnitConversionRetireReq -> Task<Result<UnitConversion, ApiError>> }

type PlantApi =
    { Define: PlantDefineReq -> Task<Result<Plant, ApiError>>
      DefineBulk: PlantDefineReq list -> Task<Result<Plant list, ApiError>>
      Rename: PlantRenameReq -> Task<Result<Plant, ApiError>>
      Retire: PlantRetireReq -> Task<Result<Plant, ApiError>> }

type StockingPointApi =
    { Define: StockingPointDefineReq -> Task<Result<StockingPoint, ApiError>>
      DefineBulk: StockingPointDefineReq list -> Task<Result<StockingPoint list, ApiError>>
      Rename: StockingPointRenameReq -> Task<Result<StockingPoint, ApiError>>
      Retire: StockingPointRetireReq -> Task<Result<StockingPoint, ApiError>> }

type SkuApi =
    { Define: SkuDefineReq -> Task<Result<Sku, ApiError>>
      DefineBulk: SkuDefineReq list -> Task<Result<Sku list, ApiError>>
      Rename: SkuRenameReq -> Task<Result<Sku, ApiError>>
      Retire: SkuRetireReq -> Task<Result<Sku, ApiError>> }

type BomApi =
    { Define: BomDefineReq -> Task<Result<Bom, ApiError>>
      DefineBulk: BomDefineReq list -> Task<Result<Bom list, ApiError>>
      Activate: BomActivateReq -> Task<Result<Bom, ApiError>>
      Deactivate: BomDeactivateReq -> Task<Result<Bom, ApiError>> }

type ResourceGroupApi =
    { Define: ResourceGroupDefineReq -> Task<Result<ResourceGroup, ApiError>>
      DefineBulk: ResourceGroupDefineReq list -> Task<Result<ResourceGroup list, ApiError>>
      Rename: ResourceGroupRenameReq -> Task<Result<ResourceGroup, ApiError>>
      Retire: ResourceGroupRetireReq -> Task<Result<ResourceGroup, ApiError>> }

type StandardResourceApi =
    { Define: StandardResourceDefineReq -> Task<Result<StandardResource, ApiError>>
      DefineBulk: StandardResourceDefineReq list -> Task<Result<StandardResource list, ApiError>>
      Rename: StandardResourceRenameReq -> Task<Result<StandardResource, ApiError>>
      Retire: StandardResourceRetireReq -> Task<Result<StandardResource, ApiError>> }

type PhysicalResourceApi =
    { Define: PhysicalResourceDefineReq -> Task<Result<PhysicalResource, ApiError>>
      DefineBulk: PhysicalResourceDefineReq list -> Task<Result<PhysicalResource list, ApiError>>
      Rename: PhysicalResourceRenameReq -> Task<Result<PhysicalResource, ApiError>>
      Retire: PhysicalResourceRetireReq -> Task<Result<PhysicalResource, ApiError>> }

type RoutingApi =
    { Define: RoutingDefineReq -> Task<Result<Routing, ApiError>>
      DefineBulk: RoutingDefineReq list -> Task<Result<Routing list, ApiError>>
      Activate: RoutingActivateReq -> Task<Result<Routing, ApiError>>
      Deactivate: RoutingDeactivateReq -> Task<Result<Routing, ApiError>> }

type TransportLegApi =
    { Define: TransportLegDefineReq -> Task<Result<TransportLeg, ApiError>>
      DefineBulk: TransportLegDefineReq list -> Task<Result<TransportLeg list, ApiError>>
      Update: TransportLegUpdateReq -> Task<Result<TransportLeg, ApiError>>
      Deactivate: TransportLegDeactivateReq -> Task<Result<TransportLeg, ApiError>> }

type InventoryApi =
    { Define: InventoryDefineReq -> Task<Result<Inventory, ApiError>>
      DefineBulk: InventoryDefineReq list -> Task<Result<Inventory list, ApiError>>
      Remove: string -> Task<Result<Inventory, ApiError>> }

type InventoryTargetApi =
    { Define: InventoryTargetDefineReq -> Task<Result<InventoryTarget, ApiError>>
      DefineBulk: InventoryTargetDefineReq list -> Task<Result<InventoryTarget list, ApiError>>
      Update: InventoryTargetUpdateReq -> Task<Result<InventoryTarget, ApiError>>
      Activate: string -> Task<Result<InventoryTarget, ApiError>>
      Deactivate: string -> Task<Result<InventoryTarget, ApiError>> }

type SupplierOfferApi =
    { Define: SupplierOfferDefineReq -> Task<Result<SupplierOffer, ApiError>>
      DefineBulk: SupplierOfferDefineReq list -> Task<Result<SupplierOffer list, ApiError>>
      Update: SupplierOfferUpdateReq -> Task<Result<SupplierOffer, ApiError>>
      Revoke: string -> Task<Result<SupplierOffer, ApiError>>
      ChangeStatus: SupplierOfferChangeStatusReq -> Task<Result<SupplierOffer, ApiError>> }

type SupplyOrderApi =
    { Create: SupplyOrderCreateReq -> Task<Result<SupplyOrder, ApiError>>
      CreateBulk: SupplyOrderCreateReq list -> Task<Result<SupplyOrder list, ApiError>>
      ProcessStatusUpdates: SupplyOrderUpdateReq list -> Task<Result<SupplyOrder list, ApiError>>
      Start: SupplyOrderStartReq -> Task<Result<SupplyOrder, ApiError>>
      PartialComplete: SupplyOrderPartialCompleteReq -> Task<Result<SupplyOrder, ApiError>>
      Complete: SupplyOrderCompleteReq -> Task<Result<SupplyOrder, ApiError>>
      Plan: SupplyOrderPlanReq -> Task<Result<SupplyOrder, ApiError>>
      Confirm: SupplyOrderConfirmReq -> Task<Result<SupplyOrder, ApiError>>
      Release: SupplyOrderReleaseReq -> Task<Result<SupplyOrder, ApiError>>
      Cancel: SupplyOrderCancelReq -> Task<Result<SupplyOrder, ApiError>>
      Lock: SupplyOrderLockReq -> Task<Result<SupplyOrder, ApiError>>
      AutoFirmOrders: DateTimeOffset -> int -> Task<Result<unit, ApiError>> }

type MaterialProviderApi =
    { GetSnapshot: string -> string -> DateTimeOffset -> Async<Result<MaterialSnapshot, ApiError>>
      GetNetAvailable: string -> string -> DateTimeOffset -> Async<Result<decimal, ApiError>>
      GetTimePhasedAvailability:
          string -> string -> DateTimeOffset -> int -> int -> Async<Result<(DateTimeOffset * decimal) list, ApiError>>
      GetDateWiseAvailability:
          string -> string -> DateTimeOffset -> int -> Async<Result<(DateTimeOffset * decimal) list, ApiError>>
      GetSupplierOptions:
          string -> string option -> decimal -> DateTimeOffset -> Async<Result<SupplierOffer list, ApiError>> }

type MaterialReservationApi =
    { CreateTentative: MaterialReservationCreateReq -> Task<Result<MaterialReservation, ApiError>>
      Confirm: MaterialReservationConfirmReq -> Task<Result<MaterialReservation, ApiError>>
      Release: MaterialReservationReleaseReq -> Task<Result<MaterialReservation, ApiError>>
      Reduce: MaterialReservationReduceReq -> Task<Result<MaterialReservation, ApiError>>
      Expire: MaterialReservationExpireReq -> Task<Result<MaterialReservation, ApiError>> }
