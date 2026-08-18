/// CA-D-006 — Prioritize Demand Workflows
/// Traces to: FS-D-013 (Prioritize Planning Entity Workflow)
module Medhavi.Demand.PrioritizeDemand.Workflows

open System
open System.Threading
open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Contracts.Demand
open Medhavi.Demand

/// Dependencies for FS-D-013 Prioritize Planning Entity workflow
type PlanningPriorityWorkflowDependencies =
    { Subscribe: EnvelopeFilter -> (Envelope -> Task<unit>) -> CancellationToken -> Task<IDisposable>
      PriorityApi: PlanningPriorityApi }

/// FS-D-013: Automated Prioritize Planning Entity Workflow
/// Triggered when Planning Classification or Demand Behavior Classification changes (EV-D-017 / EV-D-019)
let createPlanningPriorityWorkflow
    (deps: PlanningPriorityWorkflowDependencies)
    (ct: CancellationToken)
    : Task<IDisposable> =
    task {
        let processEnvelope (envelope: Envelope) : Task<unit> =
            task {
                let entityId = envelope.AggregateId

                if not(String.IsNullOrWhiteSpace entityId) then
                    let req: PrioritizePlanningEntityReq =
                        { EntityType = "Item"
                          EntityId = entityId
                          RevenueContribution = Some 0.85m
                          StrategicImportance = Some 0.90m
                          RiskExposure = Some 0.30m
                          ContractualObligation = Some 0.95m }

                    let! _ = deps.PriorityApi.PrioritizeEntity req
                    return ()
                else
                    return ()
            }

        let filter =
            [ ArsIdentifiers.EnterpriseEvents.planningClassificationChanged.Id
              ArsIdentifiers.EnterpriseEvents.demandBehaviorClassificationChanged.Id ]
            |> EnvelopeFilter.EventTypes

        return! deps.Subscribe filter processEnvelope ct
    }
