namespace Medhavi.DecisionCore

open System

type AutonomyLevel =
    | Advisory
    | Guardrailed
    | Autonomous

type AutonomyContract =
    { ContractId: string
      AgentId: string
      Level: AutonomyLevel
      Domain: string
      AllowedActions: string list
      MaxPolicyDelta: float
      MaxValueThreshold: decimal option
      RollbackRules: string
      ApprovalRequiredAbove: decimal option
      ExpiresAt: DateTimeOffset }

module Autonomy =

    let createContract (agentId: string) (level: AutonomyLevel) (domain: string) (allowedActions: string list) =
        { ContractId = Guid.NewGuid().ToString()
          AgentId = agentId
          Level = level
          Domain = domain
          AllowedActions = allowedActions
          MaxPolicyDelta = 0.1
          MaxValueThreshold = None
          RollbackRules = "manual"
          ApprovalRequiredAbove = None
          ExpiresAt = DateTimeOffset.MaxValue }

    let validateAction (contract: AutonomyContract) (action: string) (estimatedImpact: decimal) =
        match contract.Level with
        | Advisory -> Error "Advisory agents cannot execute actions directly"
        | Guardrailed
        | Autonomous ->
            if not(List.contains action contract.AllowedActions) then
                Error $"Action '{action}' not permitted by contract {contract.ContractId}"
            elif contract.MaxValueThreshold |> Option.exists(fun maxVal -> estimatedImpact > maxVal) then
                Error $"Action '{action}' exceeds maximum value threshold"
            else
                Ok()

    let isWithinBoundary (contract: AutonomyContract) (proposedDelta: float) = proposedDelta <= contract.MaxPolicyDelta

    let expireContract (contract: AutonomyContract) =
        { contract with
            ExpiresAt = DateTimeOffset.UtcNow }
