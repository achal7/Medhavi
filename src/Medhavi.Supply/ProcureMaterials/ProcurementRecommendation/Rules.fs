/// BR-S-060, BR-S-200, BR-S-202, BR-S-203, BR-S-207, BR-S-208, BR-S-210, BR-S-211, BR-S-212, BR-S-213
/// Traces to: Supply Intelligence Specification §7
module Medhavi.Supply.ProcureMaterials.ProcurementRecommendation.Rules

open System
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// BR-S-060: Procurement Recommendation Identity Invariant
let validateIdentity (requirementId: ProcurementRequirementId) : Result<unit, DomainError> =
    if String.IsNullOrWhiteSpace(ProcurementRequirementId.value requirementId) then
        Error(DomainError.validation "ProcurementRequirementId cannot be empty (BR-S-060)")
    else
        Ok()

/// BR-S-200: Procurement Recommendation Creation Eligibility
let validateCreationEligibility
    (netGapQuantity: decimal)
    (requiredDate: Timestamp)
    (evaluationTimestamp: Timestamp)
    : Result<unit, DomainError> =

    if netGapQuantity <= 0.0m then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.procurementRecommendationCreationEligibility "Net gap quantity must be strictly positive")
    elif Timestamp.value requiredDate = DateTimeOffset.MinValue then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.procurementRecommendationCreationEligibility "Required delivery date must be a valid UTC timestamp")
    elif Timestamp.value evaluationTimestamp = DateTimeOffset.MinValue then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.procurementRecommendationCreationEligibility "Evaluation timestamp must be a valid UTC timestamp")
    else
        Ok()

/// BR-S-202: Single Active Recommendation per Requirement & State Transition Invariant
let validateActiveStateTransition
    (existing: ProcurementRecommendation option)
    : Result<unit, DomainError> =

    match existing with
    | None -> Ok()
    | Some r ->
        match r.LifecycleState with
        | ProcurementRecommendationLifecycleState.Archived ->
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.singleActiveProcurementRecommendation "Procurement requirement is archived; recommendations cannot be created")
        | ProcurementRecommendationLifecycleState.Superseded
            | ProcurementRecommendationLifecycleState.Active ->
                Ok()

/// BR-S-210: Supplier Allocation Consistency Invariant
let validateSupplierAllocations
    (allocations: SupplierAllocation list)
    (recommendedQuantity: Quantity)
    : Result<unit, DomainError> =

    if List.isEmpty allocations then
        Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierAllocationStrategyGovernance "Supplier allocations list cannot be empty for an approved recommendation")
    else
        let totalAllocated = allocations |> List.sumBy (fun a -> Quantity.value a.Quantity)
        let targetQty = Quantity.value recommendedQuantity
        if Math.Abs(totalAllocated - targetQty) > 0.0001m then
            Error(ruleViolationWithDetail ArsIdentifiers.Rules.supplierAllocationStrategyGovernance $"Allocated quantity sum ({totalAllocated}) must equal recommended order quantity ({targetQty})")
        else
            Ok()
