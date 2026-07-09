module Medhavi.SharedKernel.Contracts.DecisionTrace
type DecisionRationale = {
    Summary: string
    Evidence: string list
    Alternatives: (string * string) list
}

type DecisionTrace = {
    DecisionId: string
    CapabilityId: string
    RulesEvaluated: (string * int) list
    PolicyId: string option
    PolicyVersion: int option
    SemanticObjectIds: string list
    Rationale: DecisionRationale
}