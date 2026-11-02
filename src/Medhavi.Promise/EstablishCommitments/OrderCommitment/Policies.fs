/// CA-P-006 Establish Commitments Policies
/// Traces to: PO-P-011
module Medhavi.Promise.EstablishCommitments.OrderCommitment.Policies

type CommitmentGovernancePolicy =
    { AllowScheduleRevision: bool
      MaxAllowedRevisions: int
      FreezeWindowHours: int
      RequireRevisionJustification: bool }

let defaultCommitmentPolicy: CommitmentGovernancePolicy =
    { AllowScheduleRevision = true
      MaxAllowedRevisions = 5
      FreezeWindowHours = 24
      RequireRevisionJustification = true }

