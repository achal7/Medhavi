namespace Medhavi.Contracts.Demand

open System

type SkuMetadata =
    { SkuId: string
      StockingPointId: string option
      AbcClass: string option
      XyzClass: string option
      StrategicSegment: string option
      BehaviourPattern: string option
      Priority: string option
      PriorityScore: decimal option
      DemandBehaviourState: string option
      Confidence: decimal option
      LastUpdated: DateTimeOffset }
