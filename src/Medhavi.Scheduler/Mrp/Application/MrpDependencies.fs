namespace Medhavi.Scheduler.Mrp.Application

open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.Scheduler.Mrp.Domain.MrpRunAggregate
open Medhavi.Scheduler.Mrp.Domain.Algorithms
open Medhavi.Scheduler.Mrp.Steps

// ============================================================================
// MRP DEPENDENCY RECORD (FOR CLEAN BOUNDED CONTEXT INJECTION)
// ============================================================================

/// Injected command to create Supply Orders in the Supply Bounded Context
type CreateSupplyOrders = MrpRunId -> SupplyProposal list -> Async<Result<unit, string>>

/// Holds all query and command signatures needed by the MRP Pipeline.
/// This prevents direct project/class reference coupling across Bounded Contexts.
type MrpDependencies =
    {
        /// Master Data BOM Lookup
        BomLookup: BomExplosion.BomLookup

        /// Inventory On-Hand balance lookup
        OnHandQuery: NettingStep.OnHandQuery

        /// Inbound supply orders query
        InboundQuery: NettingStep.InboundQuery

        /// Material reservations query
        ReservationsQuery: NettingStep.ReservationsQuery

        /// Safety Stock target query
        SafetyStockQuery: NettingStep.SafetyStockQuery

        /// SKU procurement type (manufactured vs purchased) query
        ProductTypeQuery: SupplyGenerationStep.ProductTypeQuery

        /// Preferred supplier lookup
        SupplierQuery: SupplyGenerationStep.SupplierQuery

        /// Work routing lookup
        RoutingQuery: SupplyGenerationStep.RoutingQuery

        /// Transfer source lookup
        TransferSourceQuery: SupplyGenerationStep.TransferSourceQuery

        /// Capacity Promise check query
        CapacityPromiseQuery: CapacityPromiseQuery

        /// Capacity Routing details query (gets ResourceGroupId and calculated Duration)
        CapacityRoutingQuery: CapacityRoutingQuery

        /// Alternate routings lookup
        AlternateRoutingsQuery: SkuId -> StockingPointId -> System.Threading.Tasks.Task<RoutingId list>

        /// Pegging link creator (Pegging Bounded Context helper)
        PeggingCreator: PeggingStep.PeggingCreator option

        /// Reservation creator (Supply Bounded Context helper)
        ReservationCreator: PostprocessStep.ReservationCreator option

        /// Command to create Supply Orders in the Supply Bounded Context
        CreateSupplyOrders: CreateSupplyOrders
    }
