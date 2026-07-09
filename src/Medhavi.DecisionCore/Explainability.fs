namespace Medhavi.DecisionCore

open Medhavi.SharedKernel.Contracts.DecisionTrace

module Explainability =

    let buildTrace
        (decisionId: string)
        (capabilityId: string)
        (rules: (string * int) list)
        (policy: string option)
        (policyVersion: int option)
        (semantics: string list)
        (rationale: DecisionRationale)
        : DecisionTrace =
        { DecisionId = decisionId
          CapabilityId = capabilityId
          RulesEvaluated = rules
          PolicyId = policy
          PolicyVersion = policyVersion
          SemanticObjectIds = semantics
          Rationale = rationale }

    let addAlternative (trace: DecisionTrace) (name: string) (reason: string) =
        { trace with
            Rationale =
                { trace.Rationale with
                    Alternatives = (name, reason) :: trace.Rationale.Alternatives } }

    let summarizeTrace (trace: DecisionTrace) =
        let rules = trace.RulesEvaluated |> List.map (fun (r, v) -> $"{r} v{v}") |> String.concat ", "
        let policy = trace.PolicyId |> Option.defaultValue "none"
        let evidence = trace.Rationale.Evidence |> String.concat "; "
        let alternatives = trace.Rationale.Alternatives |> List.map(fun (n, r) -> $"{n}: {r}") |> String.concat "; "

        $"Decision {trace.DecisionId} (capability {trace.CapabilityId}) evaluated rules [{rules}], "
        + $"policy {policy}, evidence [{evidence}]. Alternatives: {alternatives}. {trace.Rationale.Summary}"
