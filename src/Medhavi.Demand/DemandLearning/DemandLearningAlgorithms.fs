module Medhavi.Demand.DemandLearningAlgorithms

open System

/// A lightweight snapshot of a quality assessment used for trend analysis.
type QualityAssessmentSnapshot = {
    ScopeId    : string
    PeriodEnd  : DateTimeOffset
    Wape       : decimal
}

/// BA‑D‑010 — Analyse a sequence of quality assessments to detect declining forecast accuracy trends.
/// Returns None if no significant trend is found, otherwise a learning statement, evidence, and evidence strength.
let analyzeQualityTrend (assessments: QualityAssessmentSnapshot list) : (string * string list * string) option =
    if assessments.Length < 3 then
        None
    else
        let sorted = assessments |> List.sortBy (fun a -> a.PeriodEnd)
        let wapes = sorted |> List.map (fun a -> a.Wape)
        let pairs = wapes |> List.pairwise
        let increasingCount = pairs |> List.filter (fun (prev, next) -> next > prev) |> List.length
        let totalPairs = pairs.Length
        if decimal increasingCount / decimal totalPairs > 0.6m then
            let evidence = sorted |> List.map (fun a -> "WAPE " + a.Wape.ToString("P2") + " at " + a.PeriodEnd.ToString("yyyy-MM-dd"))
            let statement = $"Forecast accuracy has shown a consistent decline over the last {assessments.Length} periods for scope {sorted.Head.ScopeId}."
            let strength = if increasingCount = totalPairs then "High" else "Medium"
            Some (statement, evidence, strength)
        else
            None
