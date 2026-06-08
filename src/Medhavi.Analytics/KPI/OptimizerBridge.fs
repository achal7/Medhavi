namespace Medhavi.Analytics.KPI

open System

type ObjectiveComponents =
    { TotalLatenessDays: decimal
      TotalShortfallUnits: decimal
      TotalEarlinessDays: decimal
      TotalOverUtilHours: decimal
      TotalChangedOps: decimal
      TotalProductionCost: decimal
      TotalHoldingCost: decimal
      TotalTransportCost: decimal
      TotalSetupCost: decimal
      TotalCO2: decimal }

type OptimizerWeights =
    { Lateness: decimal
      Shortfall: decimal
      Earliness: decimal
      OverUtil: decimal
      UnderUtil: decimal
      Churn: decimal
      Production: decimal
      Holding: decimal
      Transport: decimal
      Setup: decimal
      CO2: decimal }

module OptimizerBridge =

    /// THE shared objective function -- same formula used by optimizer AND post-hoc KPI reporting
    let calculateObjectiveScore (w: OptimizerWeights) (c: ObjectiveComponents) : decimal =
        w.Lateness   * c.TotalLatenessDays   + w.Shortfall * c.TotalShortfallUnits +
        w.Earliness  * c.TotalEarlinessDays  + w.OverUtil   * c.TotalOverUtilHours  +
        w.Churn      * c.TotalChangedOps     + w.Production * c.TotalProductionCost +
        w.Holding    * c.TotalHoldingCost    + w.Transport  * c.TotalTransportCost  +
        w.Setup      * c.TotalSetupCost      + w.CO2        * c.TotalCO2

    let latenessPenaltyCoeff (w: OptimizerWeights) (dueBucket: int) (fillBucket: int) : decimal =
        w.Lateness * (max 0 (fillBucket - dueBucket) |> decimal)
