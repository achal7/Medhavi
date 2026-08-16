namespace Medhavi.Foundation.Observation

open System
open Medhavi.Foundation.Contracts

/// A pure operational knowledge event. The Name field follows a convention that allows
/// infrastructure adapters to derive severity and event type:
///   "Error.*"          → Error
///   "PI‑D‑*", "PI‑S‑*" → Metric
///   "Performance"      → Performance
///   everything else    → BusinessEvent / Information
type ArchitecturalKnowledge =
    { Name: string
      Timestamp: DateTimeOffset
      Attributes: Map<string, obj> }

module ArchitecturalKnowledge =

    let ofMetric (piId: string) (value: decimal) (additionalTags: (string * obj) list) =
        { Name = piId
          Timestamp = DateTimeOffset.UtcNow
          Attributes = ("Value", box value) :: ("MetricType", box "PI") :: additionalTags |> Map.ofList }

    let ofPerformance (operation: string) (duration: TimeSpan) (success: bool) =
        { Name = "Performance"
          Timestamp = DateTimeOffset.UtcNow
          Attributes =
            Map.ofList
                [ "Operation", box operation
                  "DurationMs", box duration.TotalMilliseconds
                  "Success", box success ] }

    let ofBusinessEvent (name: string) (attributes: (string * obj) list) =
        { Name = name
          Timestamp = DateTimeOffset.UtcNow
          Attributes = attributes |> Map.ofList }

    let ofError (name: string) (attributes: (string * obj) list) =
        { Name = $"Error.%s{name}"
          Timestamp = DateTimeOffset.UtcNow
          Attributes = attributes |> Map.ofList }

    let ofDecisionTrace (decisionId: string, trace: DecisionTrace) =
        { Name = $"DecisionTrace.%s{decisionId}"
          Timestamp = DateTimeOffset.UtcNow
          Attributes =
            Map.ofList
                [ "DecisionId", box decisionId
                  "Outcome", box trace.Outcome
                  "RulesEvaluated", box trace.RulesEvaluated
                  "CapabilityId", box trace.CapabilityId ] }
