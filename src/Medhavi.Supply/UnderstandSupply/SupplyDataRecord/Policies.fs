module Medhavi.Supply.UnderstandSupply.SupplyDataRecord.Policies

open System

/// PO-S-010 Supply Data Acceptance Policy
type SupplyDataAcceptancePolicy =
    { MaximumDataLatency: TimeSpan
      MinimumSourceReliability: decimal
      BorderlineSourceReliability: decimal
      DuplicateDetectionWindow: TimeSpan
      MaxAllowedQuantity: decimal }

module SupplyDataAcceptancePolicy =
    let defaultPolicy =
        { MaximumDataLatency = TimeSpan.FromMinutes 60.0
          MinimumSourceReliability = 0.90m
          BorderlineSourceReliability = 0.70m
          DuplicateDetectionWindow = TimeSpan.FromHours 24.0
          MaxAllowedQuantity = 10000000.0m }
