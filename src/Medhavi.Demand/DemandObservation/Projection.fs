module Medhavi.Demand.DemandObservation.Projection

open Medhavi.SharedKernel
open Medhavi.Infrastructure.Projections
open Medhavi.Demand
open Medhavi.Demand.DemandObservation.Model
open Medhavi.Demand.DemandObservation.ACL
open Medhavi.Contracts.Demand.DemandObservation

type ObservationProjectionState = Map<string, DemandObservation>

type ObservationAgent = ProjectionAgent<ObservationProjectionState, ObservationEvent>

// ---------- Mappers ----------

let mapToDTO (obs: Model.DemandObservation) : DemandObservation =
    { Id = DemandObservationId.value obs.Id
      SkuId = SkuId.value obs.SkuId
      StockingPointId = StockingPointId.value obs.StockingPointId
      Quantity = Quantity.value obs.Quantity
      UnitOfMeasure = "" // not stored yet; add if needed
      ObservationType = mapToObservationTypeContract obs.ObservationType
      BusinessTime = Timestamp.value obs.BusinessTime
      CustomerId = obs.CustomerId |> Option.map CustomerId.value
      PromotionRef = obs.PromotionRef
      CampaignRef = obs.CampaignRef
      ContractRef = obs.ContractRef
      PlanningScopeId = obs.PlanningScopeId |> Option.map PlanningScopeId.value
      Status = mapToStatusContract obs.Status
      DecisionRationale = obs.Decision |> Option.map(fun d -> d.Rationale)
      Confidence = obs.Decision |> Option.map(fun d -> d.Confidence)
      WarningCode = obs.Decision |> Option.bind(fun d -> d.WarningCode)
      SourceSystem = obs.Provenance.SourceSystem
      ExternalRef = obs.Provenance.ExternalRef }

// ---------- Evolve projection ----------

let evolveProjection (state: ObservationProjectionState) (evt: ObservationEvent) : ObservationProjectionState =
    match evt with
    | ObservationEstablished obs -> state |> Map.add (DemandObservationId.value obs.Id) (mapToDTO obs)

    | ObservationAccepted(obsId, decision) ->
        let id = DemandObservationId.value obsId

        state
        |> Map.tryFind id
        |> Option.map(fun dto ->
            { dto with
                Status = ObservationStatus.Accepted
                DecisionRationale = Some decision.Rationale
                Confidence = Some decision.Confidence
                WarningCode = decision.WarningCode })
        |> Option.fold (fun m dto -> Map.add id dto m) state

    | ObservationQuarantined(obsId, decision) ->
        let id = DemandObservationId.value obsId

        state
        |> Map.change
            id
            (Option.map(fun dto ->
                { dto with
                    Status = ObservationStatus.Quarantined
                    DecisionRationale = Some decision.Rationale }))

    | ObservationRejected(obsId, decision) ->
        let id = DemandObservationId.value obsId

        state
        |> Map.change
            id
            (Option.map(fun dto ->
                { dto with
                    Status = ObservationStatus.Rejected
                    DecisionRationale = Some decision.Rationale }))

    | ObservationWarningRecorded(obsId, code, decision) ->
        let id = DemandObservationId.value obsId

        state
        |> Map.change
            id
            (Option.map(fun dto ->
                { dto with
                    Status = ObservationStatus.Accepted
                    WarningCode = Some code
                    DecisionRationale = Some decision.Rationale }))
    | ObservationScopeAssigned(obsId, scopeId) ->
        let id = DemandObservationId.value obsId

        state
        |> Map.tryFind id
        |> Option.map(fun dto ->
            { dto with
                PlanningScopeId = Some(PlanningScopeId.value scopeId) })
        |> Option.fold (fun m dto -> Map.add id dto m) state

// ---------- Agent & Queries ----------

let createProjectionAgent () : ObservationAgent =
    ProjectionAgent(evolveProjection, Map.empty, "DemandObservationReadModel")

let createQueryService (agent: ObservationAgent) =
    let queryService = QueryServiceBase.getQueryService agent id

    // Subscribe to events for UI notifications
    // let _ =
    //     agent.EventApplied
    //     |> Observable.subscribe(fun ev ->
    //         match ev with
    //         | ObservationEstablished obs ->
    //             DomainEventBus.Publish(
    //                 { ObservationId = DemandObservationId.value obs.Id }: ObservationCreatedNotification
    //             )
    //         | ObservationAccepted(obsId, _) ->
    //             DomainEventBus.Publish(
    //                 { ObservationId = DemandObservationId.value obsId }: ObservationUpdatedNotification
    //             )
    //         | ObservationQuarantined(obsId, _) ->
    //             DomainEventBus.Publish(
    //                 { ObservationId = DemandObservationId.value obsId }: ObservationUpdatedNotification
    //             )
    //         | ObservationRejected(obsId, _) ->
    //             DomainEventBus.Publish(
    //                 { ObservationId = DemandObservationId.value obsId }: ObservationUpdatedNotification
    //             )
    //         | ObservationWarningRecorded(obsId, _, _) ->
    //             DomainEventBus.Publish(
    //                 { ObservationId = DemandObservationId.value obsId }: ObservationUpdatedNotification
    //             ))

    queryService

let seedProjections (agent: ObservationAgent) (list: Model.DemandObservation list) =
    let m = list |> List.map(fun o -> DemandObservationId.value o.Id, mapToDTO o) |> Map.ofList
    agent.SetState m
