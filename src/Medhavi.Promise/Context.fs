namespace Medhavi.Promise

/// Promise Intelligence Bounded Context Root Composition
/// Architectural Lineage: ARS §3, §4, §5, §14, §16, §17, §18; Table A.1

open System

type PromiseContext =
    { UnderstandOrders: UnderstandOrders.Context.UnderstandOrdersContext
      AllocateSupply: AllocateSupply.Context.AllocateSupplyContext
      ReserveInventory: ReserveInventory.Context.ReserveInventoryContext
      EvaluateATP: EvaluateATP.Context.EvaluateATPContext
      EvaluateCTP: EvaluateCTP.Context.EvaluateCTPContext
      EstablishCommitments: EstablishCommitments.Context.EstablishCommitmentsContext
      AssessPromiseRisk: AssessPromiseRisk.Context.AssessPromiseRiskContext
      ManageBackorders: ManageBackorders.Context.ManageBackordersContext
      ResolveExceptions: ResolveExceptions.Context.ResolveExceptionsContext
      MeasurePerformance: MeasurePerformance.Context.MeasurePerformanceContext
      LearnFromPromises: LearnFromPromises.Context.LearnFromPromisesContext
      Dispose: unit -> unit }

