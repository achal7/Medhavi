/// CA-P-008 Manage Backorders — Policies
/// Traces to: PO-P-013 Backlog Governance Policy
module Medhavi.Promise.ManageBackorders.BackorderRecord.Policies

open Medhavi.Promise.ArsIdentifiers

type BacklogGovernancePolicy =
    { PolicyId: string
      Version: int
      MaxBackorderAgingDays: int
      DampeningWindowHours: int
      AllowPartialMatches: bool }

let defaultPolicy: BacklogGovernancePolicy =
    { PolicyId = Policies.backlogGovernancePolicy.Id
      Version = 1
      MaxBackorderAgingDays = 90
      DampeningWindowHours = 24
      AllowPartialMatches = true }

