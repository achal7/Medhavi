module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Policies

open System

/// PO-S-011 Supply Understanding Materiality Policy
type MaterialityPolicy =
    { CompletenessThreshold: decimal
      StockoutExposureDeltaThresholdPercent: decimal option
      ExcessExposureDeltaThresholdPercent: decimal option }

module MaterialityPolicy =
    let defaultPolicy =
        { CompletenessThreshold = 1.0m // All 6 dimensions must be populated per BR-S-015a
          StockoutExposureDeltaThresholdPercent = Some 10.0m
          ExcessExposureDeltaThresholdPercent = Some 15.0m }

/// PO-S-012 Supply Understanding Publication Cadence Policy
type CadencePolicy =
    { MaximumPublicationInterval: TimeSpan
      StalenessWarningThreshold: TimeSpan }

module CadencePolicy =
    let defaultPolicy =
        { MaximumPublicationInterval = TimeSpan.FromHours 24.0
          StalenessWarningThreshold = TimeSpan.FromHours 12.0 }
