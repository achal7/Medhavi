// =============================================================================
// LearnFromDemand — Ports
// =============================================================================
module Medhavi.Demand.LearnFromDemand.Ports

open System.Threading.Tasks
open Medhavi.Core
open Medhavi.Demand.LearnFromDemand.DemandLearning.Model

type GetForecastQualityHistoryPort = string -> Timestamp -> Timestamp -> Task<ForecastQualityAssessmentSummary list>

type GetDemandPlanningConditionHistoryPort =
    string -> Timestamp -> Timestamp -> Task<DemandPlanningConditionSummary list>

type GetClassificationChangesHistoryPort = string -> Timestamp -> Timestamp -> Task<ClassificationChangeSummary list>
type GetBehaviourChangesHistoryPort = string -> Timestamp -> Timestamp -> Task<BehaviourChangeSummary list>
type GetPriorityChangesHistoryPort = string -> Timestamp -> Timestamp -> Task<PriorityChangeSummary list>
type GetOverrideHistoryPort = string -> Timestamp -> Timestamp -> Task<OverrideSummary list>

type LearnFromDemandPorts =
    { GetForecastQualityHistory: GetForecastQualityHistoryPort
      GetDemandPlanningConditionHistory: GetDemandPlanningConditionHistoryPort
      GetClassificationChangesHistory: GetClassificationChangesHistoryPort
      GetBehaviourChangesHistory: GetBehaviourChangesHistoryPort
      GetPriorityChangesHistory: GetPriorityChangesHistoryPort
      GetOverrideHistory: GetOverrideHistoryPort }
