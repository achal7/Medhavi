namespace Medhavi.SharedKernel.Projections

open System.Threading.Tasks
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Supply

type QueryService<'Entity, 'Id> =
    { GetAll: unit -> Task<'Entity list>
      GetById: 'Id -> Task<'Entity option>
      Exists: 'Id -> Task<bool>
      Filter: ('Entity -> bool) -> Task<'Entity list> }

// Master data
type UomQueryService = QueryService<UnitOfMeasure, string>
type UnitConversionQueryService = QueryService<UnitConversion, string>
type PlantQueryService = QueryService<Plant, string>
type StockingPointQueryService = QueryService<StockingPoint, string>
type SkuQueryService = QueryService<Sku, string>
type BomQueryService = QueryService<Bom, string>
type InventoryQueryService = QueryService<Inventory, string>
type InventoryTargetQueryService = QueryService<InventoryTarget, string>
type SupplyOrderQueryService = QueryService<SupplyOrder, string>
type SupplierOfferQueryService = QueryService<SupplierOffer, string>
type ResourceGroupQueryService = QueryService<ResourceGroup, string>
type StandardResourceQueryService = QueryService<StandardResource, string>
type PhysicalResourceQueryService = QueryService<PhysicalResource, string>
type RoutingQueryService = QueryService<Routing, string>
type TransportLegQueryService = QueryService<TransportLeg, string>

// Supply bounded context
type MaterialReservationQueryService = QueryService<MaterialReservation, string>
type MaterialSnapshotQueryService = QueryService<MaterialSnapshot, string>
type SupplyProposalQueryService = QueryService<Medhavi.Contracts.Domain.SupplyProposal, string>
