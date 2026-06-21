module Medhavi.Capacity.Application.CapacityResourceApp

open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.Capacity.Domain.CapacityResourceAgg
open Medhavi.Infrastructure.Projections

type Decision = Decision<CapacityResource, CapacityResourceEvent>

type CapacityResourceCapabilities =
    { Register: RegisterCapacityResourceCmd -> TaskResult<Decision, ApplicationError>
      Update: UpdateCapacityResourceCmd -> TaskResult<Decision, ApplicationError> }

let createCapabilities
    (repo: Repository<CapacityResource, string, CapacityResourceEvent>)
    : CapacityResourceCapabilities =
    { Register = handleCommand (fun c -> PhysicalResourceId.value c.Id) repo RegisterCapacityResource decide
      Update = handleCommand (fun c -> PhysicalResourceId.value c.Id) repo UpdateCapacityResource decide }

let evolveProjection (state: Map<string, CapacityResource>) (evt: CapacityResourceEvent) =
    match evt with
    | CapacityResourceRegistered e ->
        let idStr = PhysicalResourceId.value e.Id
        let stateOpt = evolve None (CapacityResourceRegistered e)

        match stateOpt with
        | Some s -> Map.add idStr s state
        | None -> state
    | CapacityResourceUpdated e ->
        let idStr = PhysicalResourceId.value e.Id

        match Map.tryFind idStr state with
        | Some existing ->
            match evolve (Some existing) (CapacityResourceUpdated e) with
            | Some updated -> Map.add idStr updated state
            | None -> state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, CapacityResource>, CapacityResourceEvent>(
        evolveProjection,
        Map.empty,
        "CapacityResourceReadModel"
    )
