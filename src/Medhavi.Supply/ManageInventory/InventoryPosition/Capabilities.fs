/// SE-S-030 — Inventory Position Command Capabilities
/// Traces to: FS-S-020 (Maintain Inventory Position Understanding)
module Medhavi.Supply.ManageInventory.InventoryPosition.Capabilities

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
open Decisions

type InventoryPositionAggregateApi =
    { Update: UpdateInventoryPositionReq -> Task<Result<InventoryPositionAssessment, ApplicationError>> }

let create
    (repo: Repository<InventoryPositionAssessment, ItemLocationId, InventoryPositionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (deps: AggregateStages.EnvelopeStoreDependencies<InventoryPositionEvent>)
    (ports: SupplyPorts)
    : InventoryPositionAggregateApi =

    let update (req: UpdateInventoryPositionReq) : Task<Result<InventoryPositionAssessment, ApplicationError>> =
        taskResult {
            let! cmd: UpdateInventoryPositionCmd = liftValidation (ACL.toUpdateCmd req)

            do! requireEntityExists ports.ItemExists cmd.Item "Item" ItemId.value
            do! requireEntityExists ports.LocationExists cmd.Location "Location" LocationId.value

            let! invFacts:ItemLocationInventoryFacts = protect (ports.GetItemLocationInventory cmd.Item cmd.Location)
            let! demandFacts: DemandStatisticsFact = protect (ports.GetDemandStatistics cmd.Item cmd.Location)
            let! leadTimeFacts: LeadTimeStatisticsFact = protect (ports.GetLeadTimeStatistics cmd.Item cmd.Location)
            let! valuation: ItemValuationFact = protect (ports.GetItemValuation cmd.Item)
            let! safetyStock = protect (ports.GetSafetyStockBuffer cmd.Item cmd.Location)

            let inputProvider _ _ : PositionDeterminationInput =
                { OnHand = invFacts.OnHand
                  Allocated = invFacts.Allocated
                  InTransit = invFacts.InTransit
                  SafetyStock = safetyStock
                  ReorderPoint = safetyStock + (demandFacts.AverageDemand * decimal leadTimeFacts.LeadTimeDays)
                  TargetStockLevel = safetyStock + (demandFacts.AverageDemand * decimal leadTimeFacts.LeadTimeDays * 1.5m)
                  AverageDemand = demandFacts.AverageDemand
                  LeadTimeDays = leadTimeFacts.LeadTimeDays
                  UnitCost = valuation.UnitCost
                  ShelfLifeDays = valuation.ShelfLifeDays
                  RemainingLifetimeDemand = valuation.RemainingLifetimeDemand
                  IsSingleSource = leadTimeFacts.IsSingleSource
                  SupplierReliability = leadTimeFacts.SupplierReliability }

            let evidenceProvider _ _ : int option * int option = (Some 1, Some 1)

            let decide = Behaviors.update inputProvider evidenceProvider

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: UpdateInventoryPositionCmd) -> ItemLocationId.ofItemAndLocation c.Item c.Location)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Update = update }
