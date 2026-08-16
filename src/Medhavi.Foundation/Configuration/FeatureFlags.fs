namespace Medhavi.Foundation.Configuration

open System

type FeatureFlags =
    { AiAutonomyEnabled: bool
      AiDefaultAutonomyLevel: string
      AiPolicySuggestionEnabled: bool
      AiConversationalCopilotEnabled: bool
      FastInsertEnabled: bool
      IncrementalRepairEnabled: bool
      OptimizationEnabled: bool
      WhatIfSimulationEnabled: bool
      SupplyCollaborationEnabled: bool
      KnowledgeIntelligenceEnabled: bool
      ScenarioComparisonEnabled: bool
      LlmIntegrationEnabled: bool
      AdvancedAnalyticsEnabled: bool }

module FeatureFlags =
    let private parseBool (value: string) =
        match value.ToLowerInvariant() with
        | "true"
        | "1"
        | "yes" -> true
        | _ -> false

    /// Pure loading function. Accepts an environment reader to remain side-effect free.
    let load (getEnv: string -> string option) : FeatureFlags =
        let get key = getEnv $"MEDHAVI_{key}"

        { AiAutonomyEnabled = get "AI_AUTONOMY_ENABLED" |> Option.map parseBool |> Option.defaultValue false
          AiDefaultAutonomyLevel = get "AI_DEFAULT_AUTONOMY_LEVEL" |> Option.defaultValue "Advisory"
          AiPolicySuggestionEnabled =
            get "AI_POLICY_SUGGESTION_ENABLED" |> Option.map parseBool |> Option.defaultValue false
          AiConversationalCopilotEnabled =
            get "AI_CONVERSATIONAL_COPILOT_ENABLED" |> Option.map parseBool |> Option.defaultValue false
          FastInsertEnabled = get "FAST_INSERT_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          IncrementalRepairEnabled =
            get "INCREMENTAL_REPAIR_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          OptimizationEnabled = get "OPTIMIZATION_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          WhatIfSimulationEnabled = get "WHAT_IF_SIMULATION_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          SupplyCollaborationEnabled =
            get "SUPPLY_COLLABORATION_ENABLED" |> Option.map parseBool |> Option.defaultValue false
          KnowledgeIntelligenceEnabled =
            get "KNOWLEDGE_INTELLIGENCE_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          ScenarioComparisonEnabled =
            get "SCENARIO_COMPARISON_ENABLED" |> Option.map parseBool |> Option.defaultValue true
          LlmIntegrationEnabled = get "LLM_INTEGRATION_ENABLED" |> Option.map parseBool |> Option.defaultValue false
          AdvancedAnalyticsEnabled =
            get "ADVANCED_ANALYTICS_ENABLED" |> Option.map parseBool |> Option.defaultValue false }

    let loadFromEnvironment () = load(fun key -> Environment.GetEnvironmentVariable(key) |> Option.ofObj)

    let validate (flags: FeatureFlags) : string list =
        let errors = ResizeArray<string>()

        if flags.AiAutonomyEnabled && not flags.KnowledgeIntelligenceEnabled then
            errors.Add "AI autonomy requires KnowledgeIntelligenceEnabled"

        if flags.AiConversationalCopilotEnabled && not flags.KnowledgeIntelligenceEnabled then
            errors.Add "Conversational copilot requires KnowledgeIntelligenceEnabled"

        List.ofSeq errors
