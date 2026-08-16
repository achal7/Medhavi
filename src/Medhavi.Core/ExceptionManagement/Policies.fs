/// PO-C-020: Core Exception Management Policy. SLA fields REMOVED (unauthorized capability).
module Medhavi.Core.ExceptionManagement.Policies

open Medhavi.SemanticModel

/// Governs deduplication, evidence requirements, and severity ranking (for DE-C-002).
type ExceptionManagementPolicy =
    { PolicyId: string
      Version: int
      RequireEvidenceReference: bool
      AllowDuplicateRegistration: bool
      /// Ordered severity levels, lowest -> highest. Governed by PO-C-002.
      /// VocabularyEntryIds ranked by position; higher index = higher severity.
      SeverityRanking: VocabularyEntryId list }

module ExceptionManagementPolicy =
    let defaultPolicy : ExceptionManagementPolicy =
        { PolicyId = "PO-C-002"
          Version = 1
          RequireEvidenceReference = true
          AllowDuplicateRegistration = false
          SeverityRanking = [] }

/// Returns the rank of a severity (higher = more severe). Unknown severity ranks -1.
let severityRank (policy: ExceptionManagementPolicy) (severity: VocabularyEntryId option) : int =
    match severity with
    | None -> -1
    | Some s -> policy.SeverityRanking |> List.tryFindIndex (fun v -> v = s) |> Option.defaultValue -1
