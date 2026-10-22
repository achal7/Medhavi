module Medhavi.Supply.PlanSupply.SupplyPlan.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Policies
open Algorithms

type SupplyPlanAggregateApi =
    { Generate: GenerateSupplyPlanReq -> Task<Result<SupplyPlan, ApplicationError>>
      Evaluate: EvaluateSupplyPlanReq -> Task<Result<SupplyPlan, ApplicationError>>
      Publish: PublishSupplyPlanReq -> Task<Result<SupplyPlan, ApplicationError>> }

/// CA-S-002 — Supply Plan command capabilities (FS-S-010 Generate, FS-S-011 Evaluate, FS-S-012 Publish).
let create
    (repo: Repository<SupplyPlan, SupplyPlanId, SupplyPlanEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (relaxationPolicy: ConstraintRelaxationPolicy)
    (acceptancePolicy: SupplyPlanAcceptancePolicy)
    (cadencePolicy: SupplyPlanPublicationCadencePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<SupplyPlanEvent>)
    (ports: SupplyPorts)
    : SupplyPlanAggregateApi =

    let buildPlanId (scope: PlanningScopeId) (horizon: ForecastHorizonId) =
        SupplyPlanId.ofScopeAndHorizon scope horizon

    /// FS-S-010 — Balance Supply and Demand (Generate Supply Plan)
    let generate (req: GenerateSupplyPlanReq) : Task<Result<SupplyPlan, ApplicationError>> =
        taskResult {
            let! cmd: GeneratePlanCmd = liftValidation (ACL.toGenerateCmd req)

            do! requireEntityExists ports.PlanningScopeExists cmd.PlanningScopeId "PlanningScope" PlanningScopeId.value
            do! requireEntityExists ports.ForecastHorizonExists cmd.ForecastHorizonId "ForecastHorizon" ForecastHorizonId.value

            let! (forecastFacts: DemandForecastFact list) = protect (ports.GetDemandForecast cmd.PlanningScopeId cmd.ForecastHorizonId)
            let! (pictureFacts: PictureSupplyFacts) = protect (ports.GetPictureSupplyFacts cmd.PlanningScopeId)

            // Collate BOMs and Sourcing vectors for all demand items
            let demandItems = forecastFacts |> List.map (fun f -> f.Item) |> List.distinct

            let! (bomArray: BomComponentFact list array) =
                demandItems
                |> List.map ports.GetBomComponents
                |> Task.WhenAll
                |> TaskResult.ofTaskValue

            let! (sourcingArray: SourcingAllocationFact list array) =
                demandItems
                |> List.map ports.GetSourcingAllocations
                |> Task.WhenAll
                |> TaskResult.ofTaskValue

            let allBoms = bomArray |> List.ofArray |> List.concat
            let allSourcing = sourcingArray |> List.ofArray |> List.concat

            let initialOnHand =
                pictureFacts.SupplyFacts
                |> List.filter (fun f -> f.SupplyType = SupplyType.Inventory)
                |> List.map (fun f -> ((f.Item, f.Location), Quantity.value f.Quantity))
                |> Map.ofList

            let defaultUom = UnitOfMeasureId.defaultUoM ()

            let balancingInputProvider _ _ : BalancingInput =
                { DemandForecast = forecastFacts
                  InitialInventory = initialOnHand
                  SafetyStockBuffers = Map.empty
                  BillOfMaterials = allBoms
                  SourcingAllocations = allSourcing
                  ResourceCapacities = Map.empty
                  DefaultUnitOfMeasure = defaultUom
                  CurrentTimestamp = cmd.Timestamp }

            let decide = Behaviors.generate relaxationPolicy balancingInputProvider

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: GeneratePlanCmd) -> buildPlanId c.PlanningScopeId c.ForecastHorizonId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    /// FS-S-011 — Evaluate Supply Plan
    let evaluate (req: EvaluateSupplyPlanReq) : Task<Result<SupplyPlan, ApplicationError>> =
        taskResult {
            let! cmd: EvaluatePlanCmd = liftValidation (ACL.toEvaluateCmd req)

            let decide = Behaviors.evaluate acceptancePolicy

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: EvaluatePlanCmd) -> buildPlanId c.PlanningScopeId c.ForecastHorizonId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    /// FS-S-012 — Publish Supply Plan
    let publish (req: PublishSupplyPlanReq) : Task<Result<SupplyPlan, ApplicationError>> =
        taskResult {
            let! cmd: PublishPlanCmd = liftValidation (ACL.toPublishCmd req)

            let decide = Behaviors.publish cadencePolicy

            let pipeline =
                CommandPipeline.create
                    repo
                    (fun (c: PublishPlanCmd) -> buildPlanId c.PlanningScopeId c.ForecastHorizonId)
                    decide
                    deps

            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Generate = generate
      Evaluate = evaluate
      Publish = publish }
