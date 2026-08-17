/// PO-C-003: Item Transition Governance Policy.
module Medhavi.Core.ItemTransitionManagement.Policies

/// Governs transition validation criteria and lifecycle governance.
/// No configurable thresholds required — validation is structural and deterministic.
type ItemTransitionPolicy = { PolicyId: string; Version: int }

module ItemTransitionPolicy =
    let defaultPolicy: ItemTransitionPolicy = { PolicyId = "PO-C-003"; Version = 1 }
