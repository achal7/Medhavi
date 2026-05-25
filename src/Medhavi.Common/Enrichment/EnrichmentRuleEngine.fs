module Medhavi.Common.Enrichment.EnrichmentRuleEngine

open System
open System.Threading.Tasks

// ==========================================
// ENRICHMENT RULE ENGINE - Configurable Rules
// ==========================================

/// Generic enrichment rule that can transform any input type to any output type
type EnrichmentRule<'TInput, 'TOutput> =
    {
        Id: Guid
        Name: string
        Description: string
        Condition: 'TInput -> bool
        Transform: EnrichmentContext -> 'TInput -> Async<Result<'TOutput, string>>
        Priority: int
        Enabled: bool
        Metadata: Map<string, obj>
    }

/// Rule execution result
type RuleExecutionResult<'TOutput> =
    | Executed of 'TOutput
    | Skipped of reason: string
    | Failed of error: string

/// Rule engine statistics
type EnrichmentRuleStats =
    {
        TotalRules: int
        EnabledRules: int
        ExecutedRules: int
        SkippedRules: int
        FailedRules: int
        AverageExecutionTime: TimeSpan
    }

/// Enrichment rule engine
type EnrichmentRuleEngine<'TInput, 'TOutput> =
    {
        Rules: EnrichmentRule<'TInput, 'TOutput> list
        Execute: EnrichmentContext -> 'TInput -> Async<Result<'TOutput, string>>
        ExecuteWithStats: EnrichmentContext -> 'TInput -> Async<Result<'TOutput * EnrichmentRuleStats, string>>
        AddRule:
            EnrichmentRule<'TInput, 'TOutput>
                -> EnrichmentRuleEngine<'TInput, 'TOutput>
                -> EnrichmentRuleEngine<'TInput, 'TOutput>
        RemoveRule: Guid -> EnrichmentRuleEngine<'TInput, 'TOutput> -> EnrichmentRuleEngine<'TInput, 'TOutput>
        EnableRule: Guid -> EnrichmentRuleEngine<'TInput, 'TOutput> -> EnrichmentRuleEngine<'TInput, 'TOutput>
        DisableRule: Guid -> EnrichmentRuleEngine<'TInput, 'TOutput> -> EnrichmentRuleEngine<'TInput, 'TOutput>
        GetStats: unit -> EnrichmentRuleStats
    }

// ==========================================
// RULE ENGINE IMPLEMENTATION
// ==========================================

module private Impl =

    /// Execute a single rule
    let executeRule (rule: EnrichmentRule<'TInput, 'TOutput>) ctx input =
        async {
            try
                if not rule.Enabled then
                    return Skipped "Rule is disabled"
                elif not (rule.Condition input) then
                    return Skipped "Condition not met"
                else
                    let startTime = DateTimeOffset.UtcNow
                    let! result = rule.Transform ctx input
                    let executionTime = DateTimeOffset.UtcNow - startTime

                    match result with
                    | Ok output -> return Executed output
                    | Error err -> return Failed err
            with ex ->
                return Failed $"Exception during rule execution: {ex.Message}"
        }

    /// Execute all applicable rules in priority order
    let executeRules (rules: EnrichmentRule<'TInput, 'TOutput> list) ctx input =
        async {
            let sortedRules = rules |> List.sortBy (fun r -> r.Priority)

            let mutable executedRules = 0
            let mutable skippedRules = 0
            let mutable failedRules = 0
            let mutable totalExecutionTime = TimeSpan.Zero
            let mutable finalResult = Error("No rules applicable")

            for rule in sortedRules do
                let! result = executeRule rule ctx input

                match result with
                | Executed output ->
                    executedRules <- executedRules + 1
                    finalResult <- Ok output
                // Continue to next rule for potential overrides
                | Skipped reason -> skippedRules <- skippedRules + 1
                | Failed error ->
                    failedRules <- failedRules + 1
                    finalResult <- Error error
                    // Stop on first failure
                    ()

            let stats =
                {
                    TotalRules = rules.Length
                    EnabledRules =
                        rules
                        |> List.filter (fun r -> r.Enabled)
                        |> List.length
                    ExecutedRules = executedRules
                    SkippedRules = skippedRules
                    FailedRules = failedRules
                    AverageExecutionTime = TimeSpan.Zero // Would need individual timing
                }

            match finalResult with
            | Ok result -> return Ok(result, stats)
            | Error err -> return Error "No rules were applicable"
        }

    /// Create rule engine instance
    let createRuleEngine<'TInput, 'TOutput> rules =
        let execute ctx input =
            async {
                let! result = executeRules rules ctx input

                return
                    result
                    |> Result.bind (fun (res: 'TOutput, _) -> Ok(res))
            }

        let executeWithStats ctx input = executeRules rules ctx input

        let addRule newRule (engine: EnrichmentRuleEngine<'TInput, 'TOutput>) =
            let updatedRules = engine.Rules @ [ newRule ]
            { engine with Rules = updatedRules }

        let removeRule ruleId (engine: EnrichmentRuleEngine<'TInput, 'TOutput>) =
            let updatedRules =
                engine.Rules
                |> List.filter (fun r -> r.Id <> ruleId)

            { engine with Rules = updatedRules }

        let enableRule ruleId (engine: EnrichmentRuleEngine<'TInput, 'TOutput>) =
            let updatedRules =
                engine.Rules
                |> List.map (fun r ->
                    if r.Id = ruleId then
                        { r with Enabled = true }
                    else
                        r)

            { engine with Rules = updatedRules }

        let disableRule ruleId (engine: EnrichmentRuleEngine<'TInput, 'TOutput>) =
            let updatedRules =
                engine.Rules
                |> List.map (fun r ->
                    if r.Id = ruleId then
                        { r with Enabled = false }
                    else
                        r)

            { engine with Rules = updatedRules }

        let getStats () =
            {
                TotalRules = rules.Length
                EnabledRules =
                    rules
                    |> List.filter (fun r -> r.Enabled)
                    |> List.length
                ExecutedRules = 0 // Runtime stats
                SkippedRules = 0
                FailedRules = 0
                AverageExecutionTime = TimeSpan.Zero
            }

        {
            Rules = rules
            Execute = execute
            ExecuteWithStats = executeWithStats
            AddRule = addRule
            RemoveRule = removeRule
            EnableRule = enableRule
            DisableRule = disableRule
            GetStats = getStats
        }

// ==========================================
// PUBLIC API
// ==========================================

/// Create a new enrichment rule
let createRule name description condition transform priority enabled metadata =
    {
        Id = Guid.NewGuid()
        Name = name
        Description = description
        Condition = condition
        Transform = transform
        Priority = priority
        Enabled = enabled
        Metadata = metadata
    }

/// Create an enrichment rule engine with initial rules
let create rules = Impl.createRuleEngine rules

/// Create an empty rule engine
let empty () = create []

/// Rule builder pattern
type RuleBuilder<'TInput, 'TOutput>() =
    let mutable rule =
        {
            Id = Guid.NewGuid()
            Name = ""
            Description = ""
            Condition = fun _ -> true
            Transform = fun _ _ -> async { return Error "Not implemented" }
            Priority = 0
            Enabled = true
            Metadata = Map.empty
        }

    member this.Named(name) =
        rule <- { rule with Name = name }
        this

    member this.Described(description) =
        rule <- { rule with Description = description }
        this

    member this.When(condition: 'TInput -> bool) =
        rule <- { rule with Condition = condition }
        this

    member this.TransformWith(transform: EnrichmentContext -> 'TInput -> Async<Result<'TOutput, string>>) =
        rule <- { rule with Transform = transform }
        this

    member this.WithPriority(priority: int) =
        rule <- { rule with Priority = priority }
        this

    member this.Enabled(enabled: bool) =
        rule <- { rule with Enabled = enabled }
        this

    member this.WithMetadata(key: string, value: obj) =
        rule <-
            { rule with
                Metadata = rule.Metadata |> Map.add key value
            }

        this

    member this.Build() = rule

/// Create a rule builder
let ruleBuilder<'TInput, 'TOutput> () = RuleBuilder<'TInput, 'TOutput>()

/// Utility functions for common rule patterns
module RuleUtils =

    /// Create a rule that always executes
    let alwaysExecuteRule name description transform priority =
        createRule name description (fun _ -> true) transform priority true Map.empty

    /// Create a conditional rule
    let conditionalRule name description condition transform priority =
        createRule name description condition transform priority true Map.empty

    /// Create a rule that matches a specific type
    let typeMatchRule name description typeMatcher transform priority =
        createRule name description typeMatcher transform priority true Map.empty

    /// Combine multiple rules into a single rule engine
    let combineRules rules = create rules

    /// Filter rules by predicate
    let filterRules predicate (rules: EnrichmentRule<'TInput, 'TOutput> list) = rules |> List.filter predicate

    /// Sort rules by priority
    let sortByPriority rules = rules |> List.sortBy (fun r -> r.Priority)

    /// Get enabled rules only
    let enabledRules rules = rules |> List.filter (fun r -> r.Enabled)

    /// Validate rule configuration
    let validateRule (rule: EnrichmentRule<'TInput, 'TOutput>) =
        if String.IsNullOrWhiteSpace(rule.Name) then
            Error "Rule name cannot be empty"
        elif rule.Priority < 0 then
            Error "Rule priority must be non-negative"
        else
            Ok rule
