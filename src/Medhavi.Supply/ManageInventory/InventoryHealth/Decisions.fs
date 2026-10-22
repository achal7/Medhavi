/// SE-S-032 — Inventory Health Decisions
/// Traces to: DE-S-023 (Classify Inventory Health)
module Medhavi.Supply.ManageInventory.InventoryHealth.Decisions

open Medhavi.Foundation.Failure
open Medhavi.Supply
open Model
open Policies

type HealthClassificationDecisionOutcome =
    | Published of version: int * superseded: int option
    | Rejected of reason: string

/// DE-S-023: Classify Inventory Health (Authorizes publication of periodic health assessment)
let classifyInventoryHealth
    (policy: InventoryHealthAssessmentPolicy)
    (currentPublishedOpt: InventoryHealthAssessmentVersion option)
    (versionNumber: int)
    (summaries: ItemHealthSummary list)
    (confidence: decimal)
    : Result<InventoryHealthAssessmentVersion * int option, DomainError> =

    if summaries.IsEmpty then
        Error(
            ruleViolation ArsIdentifiers.Rules.inventoryHealthPublicationCompleteness
        )
    elif confidence < policy.MinConfidenceThreshold then
        Error(
            ruleViolationWithDetail
                ArsIdentifiers.Rules.inventoryHealthAssessmentEligibility
                $"Assessment confidence {confidence} is below minimum governed threshold {policy.MinConfidenceThreshold}"
        )
    else
        let totalExcess = summaries |> List.sumBy(fun s -> s.ExcessValue)
        let totalStockoutRisk = summaries |> List.sumBy(fun s -> s.StockoutRiskExposure)

        let supersededOpt = currentPublishedOpt |> Option.map(fun v -> v.VersionNumber)

        let version: InventoryHealthAssessmentVersion =
            {
                VersionNumber = versionNumber
                State = HealthVersionLifecycleState.Published
                ItemHealthSummaries = summaries
                TotalExcessValue = totalExcess
                TotalStockoutRiskExposure = totalStockoutRisk
                AssessmentConfidence = confidence
                AssessmentRationale =
                    sprintf
                        "Classified across %d item-locations; Total Excess: %.2f; Total Stockout Risk: %.2f"
                        summaries.Length
                        totalExcess
                        totalStockoutRisk
                PublicationTimestamp = None
            }

        Ok(version, supersededOpt)
