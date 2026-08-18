/// Segment Demand Business Rules
/// Traces to: BR-D-305, BR-D-306, PO-D-035, PO-D-036 (Specification Chapter 7)
module Medhavi.Demand.SegmentDemand.PlanningClassificationAssignment.Rules

open System
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Demand.ArsIdentifiers
open Model
open Policies

/// Input context for planning classification rules (DE-D-008)
type ClassificationRuleInput =
    { EntityId: string
      ClassificationType: ClassificationType
      VolumeOrRevenuePercentage: decimal option
      HistoricalDemandValues: decimal list option
      AnalogItemId: ItemId option
      Policy: SegmentationPolicy }

/// Input context for planner override rules
type OverrideRuleInput =
    { PlannerId: string
      Justification: string
      Policy: SegmentationOverridePolicy }

/// BR-D-305: Classification must be determined by the rules defined in the current Segmentation Policy
let policyComplianceRule: Rule<ClassificationRuleInput> =
    Rule.create
        Rules.classificationDeterminedBySegmentationPolicy.Id
        Rules.classificationDeterminedBySegmentationPolicy.Explanation
        (fun input ->
            match input.ClassificationType with
            | ABC ->
                input.AnalogItemId.IsSome
                || (input.VolumeOrRevenuePercentage |> Option.exists(fun p -> p >= 0.0m && p <= 100.0m))
            | XYZ ->
                input.AnalogItemId.IsSome
                || (input.HistoricalDemandValues
                    |> Option.exists(fun v -> v.Length >= input.Policy.MinimumHistoryPeriods)))
        (fun input ->
            match input.ClassificationType with
            | ABC ->
                match input.VolumeOrRevenuePercentage with
                | Some pct -> sprintf "Cumulative volume/revenue percentage: %.2f%%" pct
                | None when input.AnalogItemId.IsSome ->
                    sprintf "Analog item reference provided: %s" (ItemId.value input.AnalogItemId.Value)
                | None -> "No volume/revenue percentage or analog reference provided"
            | XYZ ->
                match input.HistoricalDemandValues with
                | Some values ->
                    sprintf
                        "History length: %d periods (minimum required: %d)"
                        values.Length
                        input.Policy.MinimumHistoryPeriods
                | None when input.AnalogItemId.IsSome ->
                    sprintf "Analog item reference provided: %s" (ItemId.value input.AnalogItemId.Value)
                | None -> "No historical demand series or analog reference provided")

/// BR-D-306: An entity shall be classified as Unclassified if minimum evidence requirements are not met
let minimumEvidenceRule: Rule<ClassificationRuleInput> =
    Rule.create
        Rules.minimumEvidenceForClassification.Id
        Rules.minimumEvidenceForClassification.Explanation
        (fun input ->
            not(String.IsNullOrWhiteSpace input.EntityId)
            && (input.VolumeOrRevenuePercentage.IsSome
                || input.HistoricalDemandValues.IsSome
                || input.AnalogItemId.IsSome))
        (fun input ->
            sprintf
                "EntityId: '%s', VolumeEvidence: %b, HistoryEvidence: %b, AnalogEvidence: %b"
                input.EntityId
                input.VolumeOrRevenuePercentage.IsSome
                input.HistoricalDemandValues.IsSome
                input.AnalogItemId.IsSome)

/// PO-D-036: Manual Override Justification Rule
let overrideJustificationRule: Rule<OverrideRuleInput> =
    Rule.create
        "BR-D-306-OVR"
        "Planner override requires a valid business justification meeting length criteria"
        (fun input ->
            if not input.Policy.RequireJustification then
                true
            else
                not(String.IsNullOrWhiteSpace input.PlannerId)
                && not(String.IsNullOrWhiteSpace input.Justification)
                && input.Justification.Trim().Length >= input.Policy.MinimumJustificationLength)
        (fun input ->
            sprintf
                "PlannerId: '%s', JustificationLength: %d, MinimumRequired: %d"
                input.PlannerId
                (if String.IsNullOrWhiteSpace input.Justification then
                     0
                 else
                     input.Justification.Trim().Length)
                input.Policy.MinimumJustificationLength)

let classificationRules: Rule<ClassificationRuleInput> list = [ minimumEvidenceRule; policyComplianceRule ]

let overrideRules: Rule<OverrideRuleInput> list = [ overrideJustificationRule ]
