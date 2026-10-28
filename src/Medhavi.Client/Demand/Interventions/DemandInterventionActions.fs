module Medhavi.Client.Demand.Interventions.DemandInterventionActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Demand
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ViewComposition.Component
open Medhavi.Client.ActionExecution
open Medhavi.Client

let private resolveImpact (ctx: ActionExecutionContext) : Task<DemandInterventionImpactDto option> =
    task {
        match ctx.TargetKey with
        | Some key ->
            let! dtoOpt = ctx.Domain.Demand.ModelDemandInterventions.Queries.GetById key
            return dtoOpt
        | None -> return None
    }

let assessImpact: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ModelDemandInterventions.Commands
            let! existingOpt = resolveImpact ctx

            let impactId, refId, item, loc, pStart, pEnd =
                match existingOpt with
                | Some dto ->
                    dto.ImpactId,
                    dto.InterventionReference,
                    dto.Item,
                    dto.Location,
                    dto.TemporalValidityStart,
                    dto.TemporalValidityEnd
                | None ->
                    let id = ctx.TargetKey |> Option.defaultValue ($"IMP-{Guid.NewGuid().ToString().Substring(0, 8)}")
                    let now = DateTimeOffset.UtcNow
                    id,
                    "SCENARIO-Q4-PROMO",
                    "SKU-DRIVE-500GB",
                    "LOC-CENTRAL-WH",
                    now,
                    now.AddDays(30.0)

            let magnitude =
                match ctx.Payload with
                | CustomParameters p ->
                    match Map.tryFind "InterventionMagnitude" p with
                    | Some (DecimalValue d) -> d
                    | _ -> 0.15m
                | _ -> 0.15m

            let req: AssessInterventionImpactReq =
                { ImpactId = impactId
                  InterventionReference = refId
                  Item = item
                  Location = loc
                  InterventionType = "PromotionPriceDiscount"
                  InterventionMagnitude = magnitude
                  TemporalValidityStart = pStart
                  TemporalValidityEnd = pEnd
                  HistoricalPairs = [ (100.0m, 115.0m); (120.0m, 138.0m); (90.0m, 105.0m) ]
                  BaselineDemand = Some 1000.0m }

            match! api.AssessImpact req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Intervention impact assessed for '{dto.ImpactId}': Assessed Lift is +{dto.AssessedDemandLift:F1} ({dto.LiftConfidence:P1} confidence).",
                            [ DemandInterventionGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let publishImpact: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Demand.ModelDemandInterventions.Commands
            let! existingOpt = resolveImpact ctx

            let impactId =
                match existingOpt with
                | Some dto -> dto.ImpactId
                | None -> ctx.TargetKey |> Option.defaultValue "IMP-DEMO-001"

            let req: PublishInterventionImpactReq = { ImpactId = impactId }

            match! api.PublishImpact req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Demand intervention impact '{dto.ImpactId}' published as authoritative (Version {dto.Version}).",
                            [ DemandInterventionGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let all: (ActionDefId * ActionHandler) list =
    [ DemandInterventionGrid.DemandInterventionActions.assessImpact.Id, assessImpact
      DemandInterventionGrid.DemandInterventionActions.publishImpact.Id, publishImpact ]
