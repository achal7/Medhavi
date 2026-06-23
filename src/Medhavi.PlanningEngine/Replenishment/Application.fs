namespace Medhavi.Scheduler.Replenishment

open System.Threading.Tasks
open Medhavi.Contracts.Supply
open Medhavi.Scheduler.Mrp.Domain.Types
open Medhavi.SharedKernel

type GetMaterialSnapshot = SkuId -> StockingPointId -> Timestamp -> Task<Result<MaterialSnapshot, ApplicationError>>
type GetInventoryTargets = unit -> Task<InventoryTarget list>
type GetForecasts = SkuId -> StockingPointId -> Task<MrpDemand list>
type TriggerPlanningRun = SkuId -> StockingPointId -> Quantity -> Timestamp -> Task<Result<unit, string>>
type PublishShortfallAlert = ShortfallAlert -> Task<unit>

type ReplenishmentDependencies =
    { GetSnapshot: GetMaterialSnapshot
      GetTargets: GetInventoryTargets
      GetForecasts: GetForecasts
      TriggerPlanning: TriggerPlanningRun
      PublishAlert: PublishShortfallAlert }

module ReplenishmentService =

    /// Evaluates stock levels for a single target and yields an alert if shortfall detected
    let evaluateTarget
        (deps: ReplenishmentDependencies)
        (target: InventoryTarget)
        (trigger: ReplenishmentTrigger)
        (asOf: Timestamp)
        : Task<Result<ShortfallAlert option, ApplicationError>> =
        task {
            match SkuId.create target.SkuId, StockingPointId.create target.StockingPointId with
            | Error err, _
            | _, Error err -> return Error(ApplicationError.Domain err)
            | Ok skuId, Ok spId ->
                // 1. Fetch material snapshot
                let! snapshotResult = deps.GetSnapshot skuId spId asOf

                match snapshotResult with
                | Error err -> return Error err
                | Ok snapshot ->
                    // 2. Fetch forecasts
                    let! forecasts = deps.GetForecasts skuId spId

                    // 3. Calculate targets (incorporating seasonal factors and forecasts)
                    let domainTarget =
                        ReplenishmentDomain.calculateTargets skuId spId target forecasts None asOf

                    // 4. Detect shortfall using the configured trigger policy (reactive vs proactive)
                    let alertOpt =
                        ReplenishmentDomain.detectShortfallWithForecast snapshot domainTarget forecasts trigger asOf

                    return Ok alertOpt
        }

    /// Run replenishment evaluation for all active inventory targets
    let evaluateAll
        (deps: ReplenishmentDependencies)
        (trigger: ReplenishmentTrigger)
        (asOf: Timestamp)
        : Task<Result<ShortfallAlert list, ApplicationError>> =
        task {
            let! targets = deps.GetTargets()
            let activeTargets = targets |> List.filter (fun t -> t.IsActive)

            let mutable alerts = []
            let mutable finalError = None

            for target in activeTargets do
                let! result = evaluateTarget deps target trigger asOf

                match result with
                | Error err -> finalError <- Some err
                | Ok(Some alert) -> alerts <- alert :: alerts
                | Ok None -> ()

            match finalError with
            | Some err -> return Error err
            | None -> return Ok(List.rev alerts)
        }

    /// Evaluates stock and triggers MRP on shortfall detection
    let runReplenishmentEvaluation
        (deps: ReplenishmentDependencies)
        (trigger: ReplenishmentTrigger)
        (asOf: Timestamp)
        : Task<Result<ShortfallAlert list, ApplicationError>> =
        task {
            let! evalResult = evaluateAll deps trigger asOf

            match evalResult with
            | Error err -> return Error err
            | Ok alerts ->
                for alert in alerts do
                    // Publish the shortfall alert event
                    do! deps.PublishAlert alert

                    // Trigger the MRP planning run for this specific SKU & Stocking Point
                    let! triggerResult =
                        deps.TriggerPlanning alert.SkuId alert.StockingPointId alert.ShortfallQuantity asOf

                    match triggerResult with
                    | Error _msg -> ()
                    | Ok _ -> ()

                return Ok alerts
        }
