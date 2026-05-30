module Medhavi.Capacity.Application.CapacityResourceApp

open System
open Medhavi.Common.Patterns
open Medhavi.Infrastructure
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

// Translator State
type MasterResourceState =
    { Groups: Map<ResourceGroupId, Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroup>
      Standards: Map<StandardResourceId, Medhavi.MasterData.Domain.StandardResourceAgg.StandardResource>
      Physicals: Map<PhysicalResourceId, Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResource> }

let emptyMasterState =
    { Groups = Map.empty
      Standards = Map.empty
      Physicals = Map.empty }

// Resolves default values with hierarchical fallback chains
let resolveResource (state: MasterResourceState) (pr: Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResource) =
    let srOpt = Map.tryFind pr.StandardResourceId state.Standards

    let rgOpt =
        srOpt
        |> Option.bind (fun sr -> Map.tryFind sr.ResourceGroupId state.Groups)

    let eff =
        match pr.EfficiencyOverride with
        | Some eff -> eff
        | None ->
            match srOpt with
            | Some sr -> sr.DefaultEfficiency
            | None -> Percent.Hundred

    let cost =
        match pr.CostRateOverride with
        | Some cost -> Some cost
        | None ->
            srOpt
            |> Option.bind (fun sr -> sr.DefaultCostRate)

    let cal =
        match pr.CalendarId with
        | Some cal -> Some cal
        | None ->
            rgOpt
            |> Option.bind (fun rg -> rg.DefaultCalendarId)

    let groupId =
        match srOpt with
        | Some sr -> sr.ResourceGroupId
        | None ->
            ResourceGroupId.create "UNKNOWN"
            |> Result.defaultWith (fun _ -> failwith "Invalid Group ID")

    let isActive =
        pr.Status = Medhavi.SharedKernel.Status.Active
        && (match srOpt with
            | Some (sr: Medhavi.MasterData.Domain.StandardResourceAgg.StandardResource) -> sr.Status = Medhavi.SharedKernel.Status.Active
            | None -> true)
        && (match rgOpt with
            | Some (rg: Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroup) -> rg.Status = Medhavi.SharedKernel.Status.Active
            | None -> true)

    { Id = pr.Id
      StandardResourceId = pr.StandardResourceId
      ResourceGroupId = groupId
      Name = pr.Name
      IsActive = isActive
      EffectiveEfficiency = eff
      EffectiveCostRate = cost
      EffectiveCalendarId = cal }

// Process a MasterData event, returning next state and commands to dispatch
let processMasterEvent (state: MasterResourceState) (evt: obj) : MasterResourceState * CapacityResourceCommand list =
    match evt with
    | :? Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupEvent as rgEvt ->
        let evolvedGroups =
            match rgEvt with
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupDefined e ->
                let rg = Medhavi.MasterData.Domain.ResourceGroupAgg.applyDefined e
                Map.add rg.Id rg state.Groups
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupRenamed e ->
                match Map.tryFind e.Id state.Groups with
                | Some rg -> Map.add rg.Id (Medhavi.MasterData.Domain.ResourceGroupAgg.applyRenamed e rg) state.Groups
                | None -> state.Groups
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupRetired e ->
                match Map.tryFind e.Id state.Groups with
                | Some rg -> Map.add rg.Id (Medhavi.MasterData.Domain.ResourceGroupAgg.applyRetired e rg) state.Groups
                | None -> state.Groups

        let nextState = { state with Groups = evolvedGroups }

        let rgId =
            match rgEvt with
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupDefined e -> e.Id
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupRenamed e -> e.Id
            | Medhavi.MasterData.Domain.ResourceGroupAgg.ResourceGroupRetired e -> e.Id

        let cmds =
            state.Physicals.Values
            |> Seq.filter (fun pr ->
                match Map.tryFind pr.StandardResourceId state.Standards with
                | Some sr -> sr.ResourceGroupId = rgId
                | None -> false)
            |> Seq.map (fun pr ->
                let resolved = resolveResource nextState pr

                UpdateCapacityResource
                    { Id = resolved.Id
                      Name = resolved.Name
                      IsActive = resolved.IsActive
                      EffectiveEfficiency = resolved.EffectiveEfficiency
                      EffectiveCostRate = resolved.EffectiveCostRate
                      EffectiveCalendarId = resolved.EffectiveCalendarId })
            |> Seq.toList

        nextState, cmds

    | :? Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceEvent as srEvt ->
        let evolvedStandards =
            match srEvt with
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceDefined e ->
                let sr = Medhavi.MasterData.Domain.StandardResourceAgg.applyDefined e
                Map.add sr.Id sr state.Standards
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceRenamed e ->
                match Map.tryFind e.Id state.Standards with
                | Some sr ->
                    Map.add sr.Id (Medhavi.MasterData.Domain.StandardResourceAgg.applyRenamed e sr) state.Standards
                | None -> state.Standards
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceRetired e ->
                match Map.tryFind e.Id state.Standards with
                | Some sr ->
                    Map.add sr.Id (Medhavi.MasterData.Domain.StandardResourceAgg.applyRetired e sr) state.Standards
                | None -> state.Standards

        let nextState =
            { state with
                Standards = evolvedStandards }

        let srId =
            match srEvt with
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceDefined e -> e.Id
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceRenamed e -> e.Id
            | Medhavi.MasterData.Domain.StandardResourceAgg.StandardResourceRetired e -> e.Id

        let cmds =
            state.Physicals.Values
            |> Seq.filter (fun pr -> pr.StandardResourceId = srId)
            |> Seq.map (fun pr ->
                let resolved = resolveResource nextState pr

                UpdateCapacityResource
                    { Id = resolved.Id
                      Name = resolved.Name
                      IsActive = resolved.IsActive
                      EffectiveEfficiency = resolved.EffectiveEfficiency
                      EffectiveCostRate = resolved.EffectiveCostRate
                      EffectiveCalendarId = resolved.EffectiveCalendarId })
            |> Seq.toList

        nextState, cmds

    | :? Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceEvent as prEvt ->
        let evolvedPhysicals =
            match prEvt with
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceDefined e ->
                let pr = Medhavi.MasterData.Domain.PhysicalResourceAgg.applyDefined e
                Map.add pr.Id pr state.Physicals
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceRenamed e ->
                match Map.tryFind e.Id state.Physicals with
                | Some pr ->
                    Map.add pr.Id (Medhavi.MasterData.Domain.PhysicalResourceAgg.applyRenamed e pr) state.Physicals
                | None -> state.Physicals
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceRetired e ->
                match Map.tryFind e.Id state.Physicals with
                | Some pr ->
                    Map.add pr.Id (Medhavi.MasterData.Domain.PhysicalResourceAgg.applyRetired e pr) state.Physicals
                | None -> state.Physicals

        let nextState =
            { state with
                Physicals = evolvedPhysicals }

        let prId =
            match prEvt with
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceDefined e -> e.Id
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceRenamed e -> e.Id
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceRetired e -> e.Id

        match Map.tryFind prId evolvedPhysicals with
        | Some pr ->
            let resolved = resolveResource nextState pr

            match prEvt with
            | Medhavi.MasterData.Domain.PhysicalResourceAgg.PhysicalResourceDefined _ ->
                let cmd =
                    RegisterCapacityResource
                        { Id = resolved.Id
                          StandardResourceId = resolved.StandardResourceId
                          ResourceGroupId = resolved.ResourceGroupId
                          Name = resolved.Name
                          IsActive = resolved.IsActive
                          EffectiveEfficiency = resolved.EffectiveEfficiency
                          EffectiveCostRate = resolved.EffectiveCostRate
                          EffectiveCalendarId = resolved.EffectiveCalendarId }

                nextState, [ cmd ]
            | _ ->
                let cmd =
                    UpdateCapacityResource
                        { Id = resolved.Id
                          Name = resolved.Name
                          IsActive = resolved.IsActive
                          EffectiveEfficiency = resolved.EffectiveEfficiency
                          EffectiveCostRate = resolved.EffectiveCostRate
                          EffectiveCalendarId = resolved.EffectiveCalendarId }

                nextState, [ cmd ]
        | None -> nextState, []

    | _ -> state, []
