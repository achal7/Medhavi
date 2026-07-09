namespace Medhavi.Contracts.Demand.SenseDemand

open System
open System.Threading.Tasks
open Medhavi.Contracts

// ---------- Notifications ----------
type DemandBehaviourChangedNotification =
    { SkuId: string
      StockingPointId: string
      PreviousState: string
      NewState: string
      Deviation: decimal
      Direction: string // "Increase" | "Decrease"
      Confidence: decimal
      Timestamp: DateTimeOffset }

type CriticalBehaviourNotification =
    { SkuId: string
      StockingPointId: string
      Deviation: decimal
      RecommendedAction: string
      Timestamp: DateTimeOffset }

type CriticalDemandBehaviourRequiresActionNotification =
    { SkuId: string
      StockingPointId: string
      PreviousState: string
      NewState: string
      Deviation: decimal
      Direction: string
      Confidence: decimal
      Timestamp: DateTimeOffset
      RecommendedAction: string }

// ---------- Requests ----------
type EvaluateDemandSignalReq =
    { SkuId: string
      StockingPointId: string
      SignalId: string
      Source: string
      SourceReliability: decimal
      Timestamp: DateTimeOffset
      Value: decimal
      StatisticalBound: decimal
      RecentBaseline: decimal }

type AcknowledgeAssessmentReq =
    { SkuId: string
      StockingPointId: string
      PlannerIdentity: string
      Justification: string }

// ---------- Read model (mirrors domain name) ----------
type DemandBehaviourAssessment =
    { SkuId: string
      StockingPointId: string
      CurrentState: string
      LastDeviation: decimal
      Confidence: decimal
      BaselineReference: string
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

// ---------- API ----------
type SenseDemandApi =
    { EvaluateSignal: EvaluateDemandSignalReq -> Task<Result<string, ApiError>>
      Acknowledge: AcknowledgeAssessmentReq -> Task<Result<unit, ApiError>> }

type SenseDemandQueries = QueryService<DemandBehaviourAssessment, string>
