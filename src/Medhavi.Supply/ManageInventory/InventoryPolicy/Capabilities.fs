/// SE-S-031 — Inventory Policy Command Capabilities
/// Traces to: FS-S-021 (Maintain Inventory Policies)
module Medhavi.Supply.ManageInventory.InventoryPolicy.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies
open Decisions

type InventoryPolicyAggregateApi =
    { Update: UpdateInventoryPolicyReq -> Task<Result<InventoryPolicyAssignment, ApplicationError>> }

let create
    (repo: Repository<InventoryPolicyAssignment, ItemLocationId, InventoryPolicyEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (governancePolicy: InventoryPolicyGovernancePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<InventoryPolicyEvent>)
    (ports: SupplyPorts)
    : InventoryPolicyAggregateApi =

    let update (req: UpdateInventoryPolicyReq) : Task<Result<InventoryPolicyAssignment, ApplicationError>> =
        taskResult {
            let! cmd: UpdateInventoryPolicyCmd = liftValidation (ACL.toUpdateCmd req)

            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let! demandFacts: DemandStatisticsFact = protect (ports.GetDemandStatistics cmd.Item cmd.Location)
            let! leadTimeFacts: LeadTimeStatisticsFact = protect (ports.GetLeadTimeStatistics cmd.Item cmd.Location)

            let inputProvider _ _ (slOpt: decimal option) (overrideOpt: InventoryPolicyParameters option) : PolicyDeterminationInput =
                { DemandHistoryPeriods = demandFacts.ObservationCount
                  AverageDemand = demandFacts.AverageDemand
                  DemandStdDev = demandFacts.DemandStdDev
                  LeadTimeDays = leadTimeFacts.LeadTimeDays
                  LeadTimeStdDevDays = leadTimeFacts.LeadTimeStdDevDays
                  TargetServiceLevel = slOpt |> Option.defaultValue governancePolicy.DefaultServiceLevelTarget
                  ManualOverride = overrideOpt }

            let evidenceProvider _ _ : string list = [ "DemandStatistics"; "LeadTimeStatistics" ]

            let decide = Behaviors.update governancePolicy inputProvider evidenceProvider

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: UpdateInventoryPolicyCmd) -> ItemLocationId.ofItemAndLocation c.Item c.Location)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Update = update }
