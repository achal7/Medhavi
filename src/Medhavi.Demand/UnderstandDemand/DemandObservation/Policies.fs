module Medhavi.Demand.UnderstandDemand.DemandObservation.Policies

/// PO-D-001: Demand Data Acceptance Policy
type DemandDataAcceptancePolicy =
    { PolicyId: string
      Version: int
      MaxDataLatencyMinutes: int
      MinSourceReliability: decimal
      DuplicateDetectionWindowHours: int
      CompletenessWeight: decimal
      TimelinessWeight: decimal
      SourceReliabilityWeight: decimal
      ConsistencyWeight: decimal }

module DemandDataAcceptancePolicy =
    let defaultPolicy: DemandDataAcceptancePolicy =
        { PolicyId = "PO-D-001"
          Version = 1
          MaxDataLatencyMinutes = 60
          MinSourceReliability = 0.90m
          DuplicateDetectionWindowHours = 24
          CompletenessWeight = 0.40m
          TimelinessWeight = 0.30m
          SourceReliabilityWeight = 0.30m
          ConsistencyWeight = 0.00m }
