namespace Medhavi.SemanticModel

type ScenarioId = private ScenarioId of string

module ScenarioId =
    let create (id: string) = Invariants.createStringId ScenarioId "ScenarioId" id
    let value (ScenarioId id) = id

/// Lifecycle states for Scenario
type ScenarioLifecycleState =
    | Draft
    | Active
    | Archived

module ScenarioLifecycleState =
    let validateTransition
        (fromState: ScenarioLifecycleState)
        (toState: ScenarioLifecycleState)
        : Result<unit, SemanticValidationError> =
        match fromState, toState with
        | ScenarioLifecycleState.Draft, ScenarioLifecycleState.Active
        | ScenarioLifecycleState.Draft, ScenarioLifecycleState.Archived
        | ScenarioLifecycleState.Active, ScenarioLifecycleState.Archived -> Ok()
        | _ -> Error(InvalidLifecycleTransition(sprintf "%A -> %A is not a permitted lifecycle transition." fromState toState))

/// SE-C-011 Scenario
type Scenario =
    { ScenarioIdentifier: ScenarioId
      ScenarioName: string
      AssumptionStatement: string option
      Adjustments: ScenarioAdjustment list
      LifecycleState: ScenarioLifecycleState }

module Scenario =
    let validate (scenario: Scenario) : Result<unit, SemanticValidationError> =
        let adjustmentChecks = scenario.Adjustments |> List.map ScenarioAdjustment.validateScenarioAdjustment

        Invariants.firstError(
            [ Invariants.nonEmptyIdentifier "ScenarioId" (ScenarioId.value scenario.ScenarioIdentifier)
              Invariants.nonEmptyField "Scenario" "ScenarioName" scenario.ScenarioName ]
            @ adjustmentChecks
        )
