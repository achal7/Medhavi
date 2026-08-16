namespace Medhavi.Foundation.Contracts

open Medhavi.Foundation.Failure

type RuleId = string

type Rule<'Input> =
    { Id: RuleId
      Description: string
      Evaluate: 'Input -> Result<RuleEvaluation, DomainError> }

module Rule =
    /// Create a simple rule from a predicate and evidence.
    let create
        (id: RuleId)
        (description: string)
        (predicate: 'Input -> bool)
        (evidence: 'Input -> string)
        : Rule<'Input> =
        { Id = id
          Description = description
          Evaluate =
            fun input ->
                let passed = predicate input

                Ok
                    { RuleId = id
                      Passed = passed
                      Evidence = [ evidence input ]
                      ReasonCode = if passed then None else Some id } }

    /// Evaluate a list of rules, stopping on the first Error or collecting all evaluations.
    let evaluateAll (rules: Rule<'Input> list) (input: 'Input) : Result<RuleEvaluation list, DomainError> =
        (Ok [], rules)
        ||> List.fold(fun acc rule ->
            acc |> Result.bind(fun evals -> rule.Evaluate input |> Result.map(fun eval -> eval :: evals)))
        |> Result.map List.rev
