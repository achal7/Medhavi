module Medhavi.Demand.DemandExplanation.ExplanationModel

open Medhavi.Demand.DemandExplanation.Model
open Medhavi.SharedKernel.Contracts.DecisionTrace

// Controlled vocabulary for edge relationships
type ExplanationEdgeType =
    | ContributedToRevision
    | IncludedInCalculation
    | PublishedAs
    | ApprovedBy
    | ConstrainedBy

// A single business‑meaningful path element
type ExplanationPath = {
    SourceArtifact      : ExplanationSourceArtifactRef
    Relationship        : ExplanationEdgeType
    TargetDescription   : string
}

// The complete deterministic explanation
type Explanation = {
    Target : ExplanationSourceArtifactRef
    WhatHappened : string
    ContributingArtifacts : ExplanationPath list
    Decisions : DecisionTrace list
    RulesAndPolicies : string list   // plain‑language descriptions
    DerivedFacts : string list   // e.g., "Total quantity contributed: 150 units (100 + 50)"
    Unknowns : string list
}
