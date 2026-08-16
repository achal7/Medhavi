/// PO-C-020: Core Exception Management Policy.
/// CA-C-020 Exception Management Policies
module Medhavi.Core.ExceptionManagement.Policies

/// PO-C-020: Exception Management Policy
type ExceptionManagementPolicy =
    {
        PolicyId: string
        Version: int
        RequireEvidenceReference: bool
        AllowDuplicateRegistration: bool
        /// SLA thresholds per severity level (in hours)
        SlaThresholds: SlaThresholds
        /// Percentage of SLA elapsed before issuing a warning (e.g., 80 means warn at 80% of deadline)
        WarningThresholdPercent: int
    }

/// SLA thresholds per severity level
and SlaThresholds =
    { CriticalHours: int
      HighHours: int
      MediumHours: int
      LowHours: int }

/// Default policy configuration
let defaultPolicy: ExceptionManagementPolicy =
    { PolicyId = "PO-C-020"
      Version = 1
      RequireEvidenceReference = true
      AllowDuplicateRegistration = false
      SlaThresholds =
        { CriticalHours = 1
          HighHours = 4
          MediumHours = 24
          LowHours = 72 }
      WarningThresholdPercent = 80 }

/// Resolves the SLA deadline hours based on severity
let resolveSlaHours (severity: string option) (thresholds: SlaThresholds) : int =
    match severity with
    | Some s ->
        match s.ToLowerInvariant() with
        | "critical" -> thresholds.CriticalHours
        | "high" -> thresholds.HighHours
        | "medium" -> thresholds.MediumHours
        | "low" -> thresholds.LowHours
        | _ -> thresholds.MediumHours // Default to medium for unknown severities
    | None -> thresholds.MediumHours // Default to medium when no severity specified
