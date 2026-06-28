namespace Medhavi.DecisionCore

type PlanningPolicySet =
    { MaxSolverTimeSeconds: int
      MinSafetyStock: decimal
      MaxSafetyStock: decimal
      FrozenHorizonDays: int
      FirmOrderProtection: bool
      HardConstraintPreservation: bool
      MaxObjectiveWeightShift: float
      ApprovalRequiredForRiskyChanges: bool }

type PolicyGateResult =
    | Valid
    | ValidWithWarnings of string list
    | Rejected of string list

module PolicyGate =

    let validatePolicy (current: PlanningPolicySet) (proposed: PlanningPolicySet) : PolicyGateResult =
        let warnings = ResizeArray<string>()
        let errors = ResizeArray<string>()

        // 1. Max solver time must be positive
        if proposed.MaxSolverTimeSeconds <= 0 then
            errors.Add "Max solver time must be > 0"

        // 2. Safety stock bounds
        if proposed.MinSafetyStock < 0m then
            errors.Add "Minimum safety stock must be >= 0"

        if proposed.MaxSafetyStock < proposed.MinSafetyStock then
            errors.Add "Max safety stock must be >= min safety stock"

        // 3. Frozen horizon must be non‑negative
        if proposed.FrozenHorizonDays < 0 then
            errors.Add "Frozen horizon days must be >= 0"

        // 4. Hard constraint preservation cannot be disabled if firm orders protected
        if proposed.FirmOrderProtection && not proposed.HardConstraintPreservation then
            errors.Add "Cannot protect firm orders without preserving hard constraints"

        // 5. Max objective weight shift
        let currentWeights = 1.0 // placeholder – in reality you'd compute actual shift
        let proposedWeights = 1.0
        let shift = abs(proposedWeights - currentWeights)

        if shift > proposed.MaxObjectiveWeightShift then
            warnings.Add $"Objective weight shift of {shift} exceeds allowed max of {proposed.MaxObjectiveWeightShift}"

        // 6. Risky changes require approval (just a flag check)
        if proposed.ApprovalRequiredForRiskyChanges && not current.ApprovalRequiredForRiskyChanges then
            warnings.Add "Enabling approval for risky changes may slow down operations"

        if errors.Count > 0 then Rejected(List.ofSeq errors)
        elif warnings.Count > 0 then ValidWithWarnings(List.ofSeq warnings)
        else Valid
