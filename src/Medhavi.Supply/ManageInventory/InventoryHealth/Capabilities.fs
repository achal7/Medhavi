/// SE-S-032 — Inventory Health Command Capabilities
/// Traces to: FS-S-022 (Assess Inventory Health)
module Medhavi.Supply.ManageInventory.InventoryHealth.Capabilities

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
open Algorithms
open Medhavi.Supply.ManageInventory

type InventoryHealthAggregateApi =
    {
        Publish: PublishInventoryHealthReq -> Task<Result<InventoryHealthAssessment, ApplicationError>>
    }

let create
    (repo: Repository<InventoryHealthAssessment, InventoryHealthAssessmentId, InventoryHealthEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: InventoryHealthAssessmentPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<InventoryHealthEvent>)
    (ports: SupplyPorts)
    : InventoryHealthAggregateApi =

    let publish (req: PublishInventoryHealthReq) : Task<Result<InventoryHealthAssessment, ApplicationError>> =
        taskResult {
            let! cmd: PublishInventoryHealthCmd = liftValidation(ACL.toPublishCmd req)

            do!
                requireEntityExists
                    ports.PlanningScopeExists
                    cmd.AssessmentScopeId
                    "PlanningScope"
                    PlanningScopeId.value

            do!
                requireEntityExists
                    ports.EvaluationPeriodExists
                    cmd.EvaluationPeriodId
                    "EvaluationPeriod"
                    EvaluationPeriodId.value

            let! itemLocations = protect(ports.GetScopeItemLocations cmd.AssessmentScopeId)

            let! (summariesArray: ItemHealthSummary array) =
                itemLocations
                |> List.map(fun (item, loc) ->
                    task {
                        let! inv = ports.GetItemLocationInventory item loc
                        let! demand = ports.GetDemandStatistics item loc
                        let! leadTime = ports.GetLeadTimeStatistics item loc
                        let! valuation = ports.GetItemValuation item
                        let! ss = ports.GetSafetyStockBuffer item loc

                        let riskInput: RiskEvaluationInput =
                            {
                                OnHand = inv.OnHand
                                Allocated = inv.Allocated
                                InTransit = inv.InTransit
                                SafetyStock = ss
                                ReorderPoint = ss + (demand.AverageDemand * decimal leadTime.LeadTimeDays)
                                TargetStockLevel = ss + (demand.AverageDemand * decimal leadTime.LeadTimeDays * 1.5m)
                                LeadTimeDays = leadTime.LeadTimeDays
                                UnitCost = valuation.UnitCost
                                RemainingForecastDemand = valuation.RemainingLifetimeDemand
                                ShelfLifeRemainingDays = valuation.ShelfLifeDays
                                SupplierReliability = leadTime.SupplierReliability
                                IsSingleSource = leadTime.IsSingleSource
                            }

                        let risk = evaluateInventoryRisk riskInput
                        let healthStatus, _ = determineInventoryHealth policy risk

                        let available = inv.OnHand + inv.InTransit - inv.Allocated

                        let dos =
                            if demand.AverageDemand > 0m then
                                max 0m (available / demand.AverageDemand)
                            else
                                999m

                        let compliance =
                            if available < ss then
                                PolicyComplianceStatus.BelowSafetyStock
                            elif available > (riskInput.TargetStockLevel * 1.20m) then
                                PolicyComplianceStatus.PolicyBandBreach
                            else
                                PolicyComplianceStatus.Conformant

                        let summary: ItemHealthSummary =
                            {
                                Item = item
                                Location = loc
                                HealthClassification = healthStatus
                                DaysOfSupply = dos
                                PolicyComplianceStatus = compliance
                                StockoutRiskExposure = risk.StockoutExposure.ValueCurrency
                                ExcessValue = risk.ExcessExposure.ValueCurrency
                            }

                        return summary
                    })
                |> Task.WhenAll
                |> TaskResult.ofTaskValue

            let summaries = summariesArray |> List.ofArray
            let overallConfidence = if summaries.IsEmpty then 0.50m else 0.95m

            let summariesProvider _ _ : ItemHealthSummary list * decimal = (summaries, overallConfidence)

            let decide = Behaviors.publish policy summariesProvider

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishInventoryHealthCmd) ->
                        InventoryHealthAssessmentId.ofScopeAndPeriod c.AssessmentScopeId c.EvaluationPeriodId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Publish = publish }
