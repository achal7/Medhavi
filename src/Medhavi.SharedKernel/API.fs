namespace Medhavi.SharedKernel.API

open Medhavi.Common.Patterns
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Projections

type UomApi =
    { Define: UomDefineReq -> TaskResult<UnitOfMeasure, ApplicationError>
      ChangeConversionFactor: UomChangeConversionFactorReq -> TaskResult<UnitOfMeasure, ApplicationError>
      Retire: string -> TaskResult<UnitOfMeasure, ApplicationError>
      Activate: string -> TaskResult<UnitOfMeasure, ApplicationError>
      QueryService: QueryService<UnitOfMeasure, string> }

type PlantApi =
    { Define: PlantDefineReq -> TaskResult<Plant, ApplicationError>
      Rename: PlantRenameReq -> TaskResult<Plant, ApplicationError>
      Retire: PlantRetireReq -> TaskResult<Plant, ApplicationError>
      QueryService: QueryService<Plant, string> }

type UnitConversionApi =
    { Define: UnitConversionDefineReq -> TaskResult<UnitConversion, ApplicationError>
      UpdateRatio: UnitConversionUpdateReq -> TaskResult<UnitConversion, ApplicationError>
      Retire: UnitConversionRetireReq -> TaskResult<UnitConversion, ApplicationError>
      QueryService: QueryService<UnitConversion, string> }

type SkuApi =
    { Define: SkuDefineReq -> TaskResult<Sku, ApplicationError>
      Rename: SkuRenameReq -> TaskResult<Sku, ApplicationError>
      Retire: SkuRetireReq -> TaskResult<Sku, ApplicationError>
      QueryService: QueryService<Sku, string> }

type StockingPointApi =
    { Define: StockingPointDefineReq -> TaskResult<StockingPoint, ApplicationError>
      Rename: StockingPointRenameReq -> TaskResult<StockingPoint, ApplicationError>
      Retire: StockingPointRetireReq -> TaskResult<StockingPoint, ApplicationError>
      QueryService: QueryService<StockingPoint, string> }

type BomApi =
    { Define: BomDefineReq -> TaskResult<Bom, ApplicationError>
      Activate: BomActivateReq -> TaskResult<Bom, ApplicationError>
      Deactivate: BomDeactivateReq -> TaskResult<Bom, ApplicationError>
      QueryService: QueryService<Bom, string> }

type RoutingApi =
    { Define: RoutingDefineReq -> TaskResult<Routing, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<Routing, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<Routing, ApplicationError>
      QueryService: QueryService<Routing, string> }

type TransportLegApi =
    { Define: TransportLegDefineReq -> TaskResult<TransportLeg, ApplicationError>
      Update: TransportLegUpdateReq -> TaskResult<TransportLeg, ApplicationError>
      Deactivate: TransportLegDeactivateReq -> TaskResult<TransportLeg, ApplicationError>
      QueryService: QueryService<TransportLeg, string> }
