namespace Medhavi.Scenario.Domain

open System
open System.Security.Cryptography
open System.Text
open Medhavi.SharedKernel
open Medhavi.Contracts.Scenario

type ScenarioObjective =
    | Lexicographic of priority: (ObjectiveTerm * decimal) list
    | WeightedSum of weights: (ObjectiveTerm * decimal) list
    | ParetoFront of objectives: ObjectiveTerm list

type ScenarioConstraint =
    | HardCapacityLimit of resourceId: PhysicalResourceId * maxLoad: decimal
    | HardLeadTimeConstraint of skuId: SkuId * maxLeadDays: int
    | SoftServiceLevelTarget of skuId: SkuId * targetLevel: decimal
    | SoftInventoryTarget of skuId: SkuId * minDays: int * maxDays: int

module ScenarioConstraint =
    let contentHash (c: ScenarioConstraint) : string =
        let input = sprintf "%A" c
        let bytes = Encoding.UTF8.GetBytes(input)
        use sha = SHA256.Create()
        Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant()

type PlanningPolicy =
    { AllowBacklogging: bool
      AllowLateDelivery: bool
      SafetyStockMultiplier: decimal
      MinimumOrderQuantityOverride: Map<SkuId, decimal>
      ChurnPenaltyCoefficient: decimal
      FrozenOrderBehavior: FrozenOrderBehavior }

and FrozenOrderBehavior =
    | Lock
    | Adjust
    | Cancel

type ScenarioConfiguration =
    { Id: ScenarioConfigurationId
      ScenarioId: ScenarioId
      Version: Version
      Horizon: PlanRunHorizon option
      Objective: ScenarioObjective
      Constraints: ScenarioConstraint list
      Policy: PlanningPolicy
      CreatedAt: DateTimeOffset
      LastModifiedAt: DateTimeOffset }

type ScenarioConfigurationCommand =
    | Create of
        id: ScenarioConfigurationId *
        scenarioId: ScenarioId *
        horizon: PlanRunHorizon option *
        objective: ScenarioObjective *
        constraints: ScenarioConstraint list *
        policy: PlanningPolicy
    | SetHorizon of horizon: PlanRunHorizon
    | SetObjective of objective: ScenarioObjective
    | AddConstraint of constraint_: ScenarioConstraint
    | RemoveConstraint of constraintHash: string
    | UpdatePolicy of policy: PlanningPolicy

type ScenarioConfigurationEvent =
    | ScenarioConfigurationCreated of ScenarioConfigurationId * ScenarioId
    | HorizonSet of ScenarioConfigurationId * PlanRunHorizon
    | ObjectiveSet of ScenarioConfigurationId * ScenarioObjective
    | ConstraintAdded of ScenarioConfigurationId * ScenarioConstraint
    | ConstraintRemoved of ScenarioConfigurationId * constraintHash: string
    | PolicyUpdated of ScenarioConfigurationId * PlanningPolicy

module ScenarioConfigurationAgg =
    let private errConflict msg = Error (DomainError.conflict msg)
    let private errNotFound msg = Error (DomainError.notFound msg)

    let private defaultPolicy =
        { AllowBacklogging = false
          AllowLateDelivery = true
          SafetyStockMultiplier = 1.0m
          MinimumOrderQuantityOverride = Map.empty
          ChurnPenaltyCoefficient = 0.1m
          FrozenOrderBehavior = Lock }

    let handle: Decide<ScenarioConfiguration, ScenarioConfigurationCommand, ScenarioConfigurationEvent> =
        fun command stateOpt ->
            match command, stateOpt with
            | Create(id, scenarioId, horizon, objective, constraints, policy), None ->
                let config =
                    { Id = id
                      ScenarioId = scenarioId
                      Version = Version.initial
                      Horizon = horizon
                      Objective = objective
                      Constraints = constraints
                      Policy = policy
                      CreatedAt = DateTimeOffset.UtcNow
                      LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = config; Events = [ ScenarioConfigurationCreated(id, scenarioId) ] }

            | Create _, Some _ -> errConflict "ScenarioConfiguration already exists"

            | SetHorizon horizon, Some state ->
                let updated =
                    { state with
                        Horizon = Some horizon
                        Version = Version.increment state.Version
                        LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = updated; Events = [ HorizonSet(state.Id, horizon) ] }

            | SetObjective objective, Some state ->
                let updated =
                    { state with
                        Objective = objective
                        Version = Version.increment state.Version
                        LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = updated; Events = [ ObjectiveSet(state.Id, objective) ] }

            | AddConstraint constraint_, Some state ->
                let updated =
                    { state with
                        Constraints = state.Constraints @ [ constraint_ ]
                        Version = Version.increment state.Version
                        LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = updated; Events = [ ConstraintAdded(state.Id, constraint_) ] }

            | RemoveConstraint hash, Some state ->
                let remaining =
                    state.Constraints
                    |> List.filter (fun c -> ScenarioConstraint.contentHash c <> hash)
                if List.length remaining = List.length state.Constraints then
                    errNotFound (sprintf "No constraint with hash '%s' found" hash)
                else
                    let updated =
                        { state with
                            Constraints = remaining
                            Version = Version.increment state.Version
                            LastModifiedAt = DateTimeOffset.UtcNow }
                    Ok { NewState = updated; Events = [ ConstraintRemoved(state.Id, hash) ] }

            | UpdatePolicy policy, Some state ->
                let updated =
                    { state with
                        Policy = policy
                        Version = Version.increment state.Version
                        LastModifiedAt = DateTimeOffset.UtcNow }
                Ok { NewState = updated; Events = [ PolicyUpdated(state.Id, policy) ] }

            | _, None -> errNotFound "ScenarioConfiguration not found"

    let evolve (event: ScenarioConfigurationEvent) (stateOpt: ScenarioConfiguration option) : ScenarioConfiguration option =
        match event, stateOpt with
        | ScenarioConfigurationCreated(id, scenarioId), None ->
            Some
                { Id = id
                  ScenarioId = scenarioId
                  Version = Version.initial
                  Horizon = None
                  Objective = Lexicographic []
                  Constraints = []
                  Policy = defaultPolicy
                  CreatedAt = DateTimeOffset.UtcNow
                  LastModifiedAt = DateTimeOffset.UtcNow }

        | HorizonSet(_, horizon), Some s ->
            Some { s with Horizon = Some horizon; Version = Version.increment s.Version }

        | ObjectiveSet(_, objective), Some s ->
            Some { s with Objective = objective; Version = Version.increment s.Version }

        | ConstraintAdded(_, c), Some s ->
            Some { s with Constraints = s.Constraints @ [ c ]; Version = Version.increment s.Version }

        | ConstraintRemoved(_, hash), Some s ->
            let remaining = s.Constraints |> List.filter (fun c -> ScenarioConstraint.contentHash c <> hash)
            Some { s with Constraints = remaining; Version = Version.increment s.Version }

        | PolicyUpdated(_, policy), Some s ->
            Some { s with Policy = policy; Version = Version.increment s.Version }

        | _, _ -> stateOpt
