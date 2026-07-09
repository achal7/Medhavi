namespace Medhavi.SharedKernel.Traceability

type DecisionRationale = {
    Summary: string
    Evidence: string list
    Alternatives: (string * string) list
}

type DecisionTrace = {
    DecisionId: string
    CapabilityId: string
    RulesEvaluated: string list
    PolicyId: string option
    SemanticObjectIds: string list
    Rationale: DecisionRationale
}
