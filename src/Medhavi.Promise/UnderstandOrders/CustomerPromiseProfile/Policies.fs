/// CA-P-001 Customer Promise Profile Policies
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Policies

open System

/// PO-P-010 Customer Service Terms Policy
type CustomerProfilePolicy =
    { MaxDefaultSlaDays: int
      MinDefaultSlaDays: int
      MaxDailyPenaltyRatePct: decimal
      MaxPenaltyCapPct: decimal
      MinSubstitutionMarginFloorPct: decimal }

module CustomerProfilePolicy =
    let defaultPolicy =
        { MaxDefaultSlaDays = 60
          MinDefaultSlaDays = 1
          MaxDailyPenaltyRatePct = 5.0m
          MaxPenaltyCapPct = 25.0m
          MinSubstitutionMarginFloorPct = 15.0m }

