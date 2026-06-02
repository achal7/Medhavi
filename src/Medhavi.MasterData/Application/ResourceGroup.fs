module Medhavi.MasterData.Application.ResourceGroup

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.ResourceGroupAgg
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel.API

module ACL =
    let toDefineCommand (req: ResourceGroupDefineReq) : Result<DefineResourceGroupCmd, DomainError> =
        Ok
            { Id = req.Id
              PlantId = req.PlantId
              Name = req.Name
              Description = req.Description
              DefaultCalendarId = req.DefaultCalendarId }

    let toRenameCommand (req: ResourceGroupRenameReq) : Result<RenameResourceGroupCmd, DomainError> =
        ResourceGroupId.create req.Id
        |> Result.map (fun id -> { Id = id; NewName = req.NewName }: RenameResourceGroupCmd)

    let toRetireCommand (req: ResourceGroupRetireReq) : Result<RetireResourceGroupCmd, DomainError> =
        ResourceGroupId.create req.Id
        |> Result.map (fun id -> { Id = id }: RetireResourceGroupCmd)

type Decision = Decision<ResourceGroup, ResourceGroupEvent>

type ResourceGroupCapabilities =
    { Define: ResourceGroupDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: ResourceGroupRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: ResourceGroupRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<ResourceGroup, string, ResourceGroupEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefineResourceGroup decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun c -> ResourceGroupId.value c.Id) repo RenameResourceGroup decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun c -> ResourceGroupId.value c.Id) repo RetireResourceGroup decide }

let mapResourceGroupDto (rg: ResourceGroup) : Contracts.Domain.ResourceGroup =
    { Id = ResourceGroupId.value rg.Id
      PlantId = rg.PlantId |> Option.map PlantId.value
      Name = rg.Name
      Description = rg.Description
      DefaultCalendarId =
        rg.DefaultCalendarId
        |> Option.map CalendarId.value
      IsActive = rg.Status.ToBool()
      Created = Timestamp.value rg.Created
      Modified = Timestamp.value rg.Modified }

let evolveProjection (state: Map<string, Contracts.Domain.ResourceGroup>) (evt: ResourceGroupEvent) =
    match evt with
    | ResourceGroupDefined e ->
        let dto: Contracts.Domain.ResourceGroup =
            { Id = ResourceGroupId.value e.Id
              PlantId = e.PlantId |> Option.map PlantId.value
              Name = e.Name
              Description = e.Description
              DefaultCalendarId = e.DefaultCalendarId |> Option.map CalendarId.value
              IsActive = true
              Created = Timestamp.value e.Created
              Modified = Timestamp.value e.Created }

        Map.add dto.Id dto state
    | ResourceGroupRenamed e ->
        let key = ResourceGroupId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    Name = e.NewName
                    Modified = Timestamp.value e.Modified }
                state
        | None -> state
    | ResourceGroupRetired e ->
        let key = ResourceGroupId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    IsActive = false
                    Modified = Timestamp.value e.RetiredAt }
                state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.ResourceGroup>, ResourceGroupEvent>(
        evolveProjection,
        Map.empty,
        "ResourceGroupReadModel"
    )

let createResourceGroupApi (capabilities: ResourceGroupCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapResourceGroupDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapResourceGroupDto)
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapResourceGroupDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapResourceGroupDto }
    : ResourceGroupApi
