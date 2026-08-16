namespace Medhavi.Foundation.Contracts

type DecisionRationale =
    { Summary: string
      Evidence: string list
      Alternatives: (string * string) list }

type RuleEvaluation =
    { RuleId: string
      Passed: bool
      Evidence: string list
      ReasonCode: string option }

type DecisionOutcome<'Outcome> =
    { Outcome: 'Outcome
      Evaluations: RuleEvaluation list }

type DecisionTrace =
    { DecisionId: string
      CausalDecisionIds: string list
      CapabilityId: string
      Outcome: string
      RulesEvaluated: RuleEvaluation list
      PolicyId: string option
      PolicyVersion: int option
      SemanticObjectIds: string list
      Rationale: DecisionRationale }
