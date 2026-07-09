module Medhavi.Demand.Projections.UnifiedPlanningMetadataProjection

open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.Contracts.Demand
open Medhavi.Demand.PlanningClassificationAssignment.Model
open Medhavi.Demand.DemandBehaviourAssignment.Model
open Medhavi.Demand.PlanningPriorityAssignment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Model
open System

// ---------- Projection state ----------

type UnifiedMetadataState = Map<string, SkuMetadata>

let private emptyMetadata skuId (spId: string option) =
    { SkuId = skuId
      StockingPointId = spId
      AbcClass = None
      XyzClass = None
      StrategicSegment = None
      BehaviourPattern = None
      Priority = None
      PriorityScore = None
      DemandBehaviourState = None
      Confidence = None
      LastUpdated = DateTimeOffset.MinValue }

// ---------- Helpers ----------

let private upsert (key: string) (updater: SkuMetadata -> SkuMetadata) (state: UnifiedMetadataState) =
    let current = state |> Map.tryFind key |> Option.defaultValue(emptyMetadata key None)
    let updated = updater current
    Map.add key updated state

let private skuOnlyKey (entityType: string) (entityId: string) = if entityType = "Product" then Some entityId else None

let private skuSpKey (skuId: string) (spId: string) = $"{skuId}-{spId}"

// ---------- Event handler ----------

let evolveProjection (state: UnifiedMetadataState) (evt: obj) : UnifiedMetadataState =
    match evt with
    // --- Planning Classification Assignment (SE‑D‑036) ---
    | :? PlanningClassificationEvent as pca ->
        match pca with
        | PlanningClassificationUpdated(ass, _) ->
            match skuOnlyKey ass.EntityType ass.EntityId with
            | None -> state
            | Some sku ->
                let updater (m: SkuMetadata) =
                    match ass.ClassificationType with
                    | ABC ->
                        { m with
                            AbcClass = Some ass.CurrentClassification
                            LastUpdated = DateTimeOffset.UtcNow }
                    | XYZ ->
                        { m with
                            XyzClass = Some ass.CurrentClassification
                            LastUpdated = DateTimeOffset.UtcNow }
                    | Strategic ->
                        { m with
                            StrategicSegment = Some ass.CurrentClassification
                            LastUpdated = DateTimeOffset.UtcNow }

                upsert sku updater state

    // --- Demand Behaviour Assignment (SE‑D‑037) ---
    | :? DemandBehaviourAssignmentEvent as dba ->
        match dba with
        | DemandBehaviourClassificationUpdated(ass, _) ->
            match skuOnlyKey ass.EntityType ass.EntityId with
            | None -> state
            | Some sku ->
                upsert
                    sku
                    (fun m ->
                        { m with
                            BehaviourPattern = Some ass.CurrentClassification
                            LastUpdated = DateTimeOffset.UtcNow })
                    state

    // --- Planning Priority Assignment (SE‑D‑038) ---
    | :? PlanningPriorityEvent as pp ->
        match pp with
        | PlanningPriorityUpdated(ass, _) ->
            match skuOnlyKey ass.EntityType ass.EntityId with
            | None -> state
            | Some sku ->
                upsert
                    sku
                    (fun m ->
                        { m with
                            Priority = Some <| ass.CurrentPriority.AsString()
                            PriorityScore = Some(PositiveDecimal.value ass.PriorityScore)
                            LastUpdated = DateTimeOffset.UtcNow })
                    state

    // --- Demand Behaviour Assessment (SE‑D‑035) ---
    | :? DemandBehaviourAssessmentEvent as dbaEvent ->
        match dbaEvent with
        | BehaviourStateChanged(ass, _) ->
            let key = skuSpKey (SkuId.value ass.SkuId) (StockingPointId.value ass.StockingPointId)

            upsert
                key
                (fun m ->
                    { m with
                        DemandBehaviourState = Some <| ass.CurrentState.AsString()
                        Confidence = ass.CurrentDeviation |> Option.map PositiveDecimal.value
                        LastUpdated = DateTimeOffset.UtcNow })
                state

        | BehaviourAssessmentAcknowledged _ -> state

    | _ -> state

// ---------- Agent & queries ----------

type UnifiedMetadataAgent = ProjectionAgent<UnifiedMetadataState, obj>

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "UnifiedPlanningMetadata")

type UnifiedMetadataQueries() =
    interface IDisposable with
        member _.Dispose() = ()

    member _.GetSkuMetadata (skuId: string, spId: string) (agent: UnifiedMetadataAgent) =
        agent.QueryAsync(fun state ->
            let skuSpKey = skuSpKey skuId spId
            let skuSpData = state |> Map.tryFind skuSpKey
            let skuData = state |> Map.tryFind skuId

            match skuSpData, skuData with
            | None, None -> None
            | Some sp, None -> Some sp
            | None, Some sku -> Some sku
            | Some sp, Some sku ->
                Some
                    { sp with
                        AbcClass = sp.AbcClass |> Option.orElse sku.AbcClass
                        XyzClass = sp.XyzClass |> Option.orElse sku.XyzClass
                        StrategicSegment = sp.StrategicSegment |> Option.orElse sku.StrategicSegment
                        BehaviourPattern = sp.BehaviourPattern |> Option.orElse sku.BehaviourPattern
                        Priority = sp.Priority |> Option.orElse sku.Priority
                        PriorityScore = sp.PriorityScore |> Option.orElse sku.PriorityScore
                        DemandBehaviourState = sp.DemandBehaviourState |> Option.orElse sku.DemandBehaviourState
                        Confidence = sp.Confidence |> Option.orElse sku.Confidence
                        LastUpdated = max sp.LastUpdated sku.LastUpdated })

let seedProjections (agent: UnifiedMetadataAgent) =
    // No pre-seeding needed – projections are empty at startup and populated by events.
    ()
