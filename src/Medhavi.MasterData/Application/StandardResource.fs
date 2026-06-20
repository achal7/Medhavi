module Medhavi.MasterData.Application.StandardResource

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts
open Medhavi.Contracts.API
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.StandardResourceAgg
open Medhavi.Infrastructure.Projections

module ACL =
    let toDefineCommand (req: StandardResourceDefineReq) : Result<DefineStandardResourceCmd, DomainError> =
        Ok
            { Id = req.Id
              ResourceGroupId = req.ResourceGroupId
              Name = req.Name
              Description = req.Description
              DefaultEfficiency = req.DefaultEfficiency
              DefaultCostRateAmount = req.DefaultCostRateAmount
              DefaultCostRateCurrency = req.DefaultCostRateCurrency }

    let toRenameCommand (req: StandardResourceRenameReq) : Result<RenameStandardResourceCmd, DomainError> =
        StandardResourceId.create req.Id
        |> Result.map(fun id -> { Id = id; NewName = req.NewName }: RenameStandardResourceCmd)

    let toRetireCommand (req: StandardResourceRetireReq) : Result<RetireStandardResourceCmd, DomainError> =
        StandardResourceId.create req.Id |> Result.map(fun id -> { Id = id }: RetireStandardResourceCmd)

type Decision = Decision<StandardResource, StandardResourceEvent>

type StandardResourceCapabilities =
    { Define: StandardResourceDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: StandardResourceRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: StandardResourceRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<StandardResource, string, StandardResourceEvent>) =
    { Define = liftCmdResult ACL.toDefineCommand >=> handleCommand (fun c -> c.Id) repo DefineStandardResource decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun c -> StandardResourceId.value c.Id) repo RenameStandardResource decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun c -> StandardResourceId.value c.Id) repo RetireStandardResource decide }

let mapStandardResourceDto (sr: StandardResource) : MasterData.StandardResource =
    { Id = StandardResourceId.value sr.Id
      ResourceGroupId = ResourceGroupId.value sr.ResourceGroupId
      Name = sr.Name
      Description = sr.Description
      DefaultEfficiency = Percent.value sr.DefaultEfficiency
      DefaultCostRateAmount = sr.DefaultCostRate |> Option.map(fun c -> c.Amount)
      DefaultCostRateCurrency = sr.DefaultCostRate |> Option.map(fun c -> c.Currency)
      IsActive = sr.Status.ToBool()
      Created = Timestamp.value sr.Created
      Modified = Timestamp.value sr.Modified }

let evolveProjection (state: Map<string, MasterData.StandardResource>) (evt: StandardResourceEvent) =
    match evt with
    | StandardResourceDefined e ->
        let dto: MasterData.StandardResource =
            { Id = StandardResourceId.value e.Id
              ResourceGroupId = ResourceGroupId.value e.ResourceGroupId
              Name = e.Name
              Description = e.Description
              DefaultEfficiency = Percent.value e.DefaultEfficiency
              DefaultCostRateAmount = e.DefaultCostRate |> Option.map(fun c -> c.Amount)
              DefaultCostRateCurrency = e.DefaultCostRate |> Option.map(fun c -> c.Currency)
              IsActive = true
              Created = Timestamp.value e.Created
              Modified = Timestamp.value e.Created }

        Map.add dto.Id dto state
    | StandardResourceRenamed e ->
        let key = StandardResourceId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    Name = e.NewName
                    Modified = Timestamp.value e.Modified }
                state
        | None -> state
    | StandardResourceRetired e ->
        let key = StandardResourceId.value e.Id

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
    ProjectionAgent<Map<string, MasterData.StandardResource>, StandardResourceEvent>(
        evolveProjection,
        Map.empty,
        "StandardResourceReadModel"
    )

let createStandardResourceApi (capabilities: StandardResourceCapabilities) =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map(fun d -> d.NewState)
            |> TaskResult.map mapStandardResourceDto
            |> TaskResult.mapError ApplicationError.mapToApiError
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map(fun decisions ->
                decisions |> List.map(fun d -> d.NewState) |> List.map mapStandardResourceDto)
            |> TaskResult.mapError ApplicationError.mapToApiError
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map(fun d -> d.NewState)
            |> TaskResult.map mapStandardResourceDto
            |> TaskResult.mapError ApplicationError.mapToApiError
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map(fun d -> d.NewState)
            |> TaskResult.map mapStandardResourceDto
            |> TaskResult.mapError ApplicationError.mapToApiError }
    : StandardResourceApi
