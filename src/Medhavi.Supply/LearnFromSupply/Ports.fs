// Medhavi.Supply.LearnFromSupply.Ports
// Traceability: Decoupled Multi-Period Fact Query Ports for CA-S-013 Learn From Supply
namespace Medhavi.Supply.LearnFromSupply

open System.Threading.Tasks
open Medhavi.SemanticModel
open Medhavi.Supply
open Medhavi.Supply.LearnFromSupply.SupplyLearning.Algorithms

type GetSupplyQualityHistoryPort = PlanningScopeId -> TemporalWindow -> Task<SupplyQualitySummary list>

type GetSupplyExceptionHistoryPort = PlanningScopeId -> TemporalWindow -> Task<SupplyExceptionSummary list>

type GetSupplyPlanStabilityHistoryPort = PlanningScopeId -> TemporalWindow -> Task<SupplyPlanStabilitySummary list>

type GetCapacityUtilizationHistoryPort = PlanningScopeId -> TemporalWindow -> Task<CapacityUtilizationSummary list>

type GetSupplierCommitmentHistoryPort = PlanningScopeId -> TemporalWindow -> Task<SupplierCommitmentSummary list>

type GetInventoryPositionHistoryPort = PlanningScopeId -> TemporalWindow -> Task<InventoryPositionSummary list>

/// Port bundle for fetching historical factual evidence across Supply capabilities
type LearnFromSupplyPorts =
    { GetSupplyQualityHistory: GetSupplyQualityHistoryPort
      GetSupplyExceptionHistory: GetSupplyExceptionHistoryPort
      GetSupplyPlanStabilityHistory: GetSupplyPlanStabilityHistoryPort
      GetCapacityUtilizationHistory: GetCapacityUtilizationHistoryPort
      GetSupplierCommitmentHistory: GetSupplierCommitmentHistoryPort
      GetInventoryPositionHistory: GetInventoryPositionHistoryPort }
