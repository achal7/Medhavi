namespace Medhavi.DecisionCore

type DecisionRationale =
    { Summary: string
      Evidence: string list
      Alternatives: (string * string) list } // (name, reason)

type DecisionTrace =
    { DecisionId: string
      CapabilityId: string
      RulesEvaluated: string list
      PolicyId: string option
      SemanticObjectIds: string list
      Rationale: DecisionRationale }

module Explainability =

    let buildTrace
        (decisionId: string)
        (capabilityId: string)
        (rules: string list)
        (policy: string option)
        (semantics: string list)
        (rationale: DecisionRationale)
        : DecisionTrace =
        { DecisionId = decisionId
          CapabilityId = capabilityId
          RulesEvaluated = rules
          PolicyId = policy
          SemanticObjectIds = semantics
          Rationale = rationale }

    let addAlternative (trace: DecisionTrace) (name: string) (reason: string) =
        { trace with
            Rationale =
                { trace.Rationale with
                    Alternatives = (name, reason) :: trace.Rationale.Alternatives } }

    let summarizeTrace (trace: DecisionTrace) =
        let rules = trace.RulesEvaluated |> String.concat ", "
        let policy = trace.PolicyId |> Option.defaultValue "none"
        let evidence = trace.Rationale.Evidence |> String.concat "; "
        let alternatives = trace.Rationale.Alternatives |> List.map(fun (n, r) -> $"{n}: {r}") |> String.concat "; "

        $"Decision {trace.DecisionId} (capability {trace.CapabilityId}) evaluated rules [{rules}], "
        + $"policy {policy}, evidence [{evidence}]. Alternatives: {alternatives}. {trace.Rationale.Summary}"
