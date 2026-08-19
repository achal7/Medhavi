module Medhavi.Core.DemandManagement.Policies

/// PO-C-004: Demand Management Policy
type DemandManagementPolicy =
    { PolicyId: string
      Version: int
      AllowDuplicateRecording: bool }

module DemandManagementPolicy =
    let defaultPolicy: DemandManagementPolicy =
        { PolicyId = "PO-C-004"
          Version = 1
          AllowDuplicateRecording = false }
