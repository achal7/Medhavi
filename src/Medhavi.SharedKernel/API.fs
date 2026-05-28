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
      Activate: string -> TaskResult<UnitOfMeasure, ApplicationError>
      QueryService: QueryService<UnitOfMeasure, string> }

type PlantApi =
    { Define: PlantDefineReq -> TaskResult<Plant, ApplicationError>
      DefineBulk: PlantDefineReq list -> TaskResult<Plant list, ApplicationError>
      Rename: PlantRenameReq -> TaskResult<Plant, ApplicationError>
      Retire: PlantRetireReq -> TaskResult<Plant, ApplicationError>
      QueryService: QueryService<Plant, string> }

type UnitConversionApi =
    { Define: UnitConversionDefineReq -> TaskResult<UnitConversion, ApplicationError>
      DefineBulk: UnitConversionDefineReq list -> TaskResult<UnitConversion list, ApplicationError>
      UpdateRatio: UnitConversionUpdateReq -> TaskResult<UnitConversion, ApplicationError>
      Retire: UnitConversionRetireReq -> TaskResult<UnitConversion, ApplicationError>
      QueryService: QueryService<UnitConversion, string> }

type SkuApi =
    { Define: SkuDefineReq -> TaskResult<Sku, ApplicationError>
      DefineBulk: SkuDefineReq list -> TaskResult<Sku list, ApplicationError>
      Rename: SkuRenameReq -> TaskResult<Sku, ApplicationError>
      Retire: SkuRetireReq -> TaskResult<Sku, ApplicationError>
      QueryService: QueryService<Sku, string> }

type StockingPointApi =
    { Define: StockingPointDefineReq -> TaskResult<StockingPoint, ApplicationError>
      DefineBulk: StockingPointDefineReq list -> TaskResult<StockingPoint list, ApplicationError>
      Rename: StockingPointRenameReq -> TaskResult<StockingPoint, ApplicationError>
      Retire: StockingPointRetireReq -> TaskResult<StockingPoint, ApplicationError>
      QueryService: QueryService<StockingPoint, string> }

type BomApi =
    { Define: BomDefineReq -> TaskResult<Bom, ApplicationError>
      DefineBulk: BomDefineReq list -> TaskResult<Bom list, ApplicationError>
      Activate: BomActivateReq -> TaskResult<Bom, ApplicationError>
      Deactivate: BomDeactivateReq -> TaskResult<Bom, ApplicationError>
      QueryService: QueryService<Bom, string> }

type RoutingApi =
    { Define: RoutingDefineReq -> TaskResult<Routing, ApplicationError>
      DefineBulk: RoutingDefineReq list -> TaskResult<Routing list, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<Routing, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<Routing, ApplicationError>
      QueryService: QueryService<Routing, string> }

type TransportLegApi =
    { Define: TransportLegDefineReq -> TaskResult<TransportLeg, ApplicationError>
      DefineBulk: TransportLegDefineReq list -> TaskResult<TransportLeg list, ApplicationError>
      Update: TransportLegUpdateReq -> TaskResult<TransportLeg, ApplicationError>
      Deactivate: TransportLegDeactivateReq -> TaskResult<TransportLeg, ApplicationError>
      QueryService: QueryService<TransportLeg, string> }

type InventoryApi =
    { Define: InventoryDefineReq -> TaskResult<Inventory, ApplicationError>
      DefineBulk: InventoryDefineReq list -> TaskResult<Inventory list, ApplicationError>
      Remove: string -> TaskResult<Inventory, ApplicationError>
      QueryService: QueryService<Inventory, string> }

type InventoryTargetApi =
    { Define: InventoryTargetDefineReq -> TaskResult<InventoryTarget, ApplicationError>
      DefineBulk: InventoryTargetDefineReq list -> TaskResult<InventoryTarget list, ApplicationError>
      Update: InventoryTargetUpdateReq -> TaskResult<InventoryTarget, ApplicationError>
      Activate: string -> TaskResult<InventoryTarget, ApplicationError>
      Deactivate: string -> TaskResult<InventoryTarget, ApplicationError>
      QueryService: QueryService<InventoryTarget, string> }

type SupplierOfferApi =
    { Define: SupplierOfferDefineReq -> TaskResult<SupplierOffer, ApplicationError>
      DefineBulk: SupplierOfferDefineReq list -> TaskResult<SupplierOffer list, ApplicationError>
      Update: SupplierOfferUpdateReq -> TaskResult<SupplierOffer, ApplicationError>
      Revoke: string -> TaskResult<SupplierOffer, ApplicationError>
      ChangeStatus: SupplierOfferChangeStatusReq -> TaskResult<SupplierOffer, ApplicationError>
      QueryService: QueryService<SupplierOffer, string> }

type SupplyOrderApi =
    { Create: SupplyOrderCreateReq -> TaskResult<SupplyOrder, ApplicationError>
      CreateBulk: SupplyOrderCreateReq list -> TaskResult<SupplyOrder list, ApplicationError>
      ProcessStatusUpdates: SupplyOrderStatusPayload list -> TaskResult<SupplyOrder list, ApplicationError>
      Start: SupplyOrderStartReq -> TaskResult<SupplyOrder, ApplicationError>
      PartialComplete: SupplyOrderPartialCompleteReq -> TaskResult<SupplyOrder, ApplicationError>
      Complete: SupplyOrderCompleteReq -> TaskResult<SupplyOrder, ApplicationError>
      Plan: SupplyOrderPlanReq -> TaskResult<SupplyOrder, ApplicationError>
      Confirm: SupplyOrderConfirmReq -> TaskResult<SupplyOrder, ApplicationError>
      Release: SupplyOrderReleaseReq -> TaskResult<SupplyOrder, ApplicationError>
      Cancel: SupplyOrderCancelReq -> TaskResult<SupplyOrder, ApplicationError>
      Lock: SupplyOrderLockReq -> TaskResult<SupplyOrder, ApplicationError>
      QueryService: QueryService<SupplyOrder, string> }
