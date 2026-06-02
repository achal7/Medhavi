module Medhavi.MasterData.Application.PhysicalResource

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure.Projections
open Medhavi.MasterData.Domain.PhysicalResourceAgg
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.SharedKernel.API

module ACL =
    let toDefineCommand (req: PhysicalResourceDefineReq) : Result<DefinePhysicalResourceCmd, DomainError> =
        Ok
            { Id = req.Id
              StandardResourceId = req.StandardResourceId
              Name = req.Name
              SerialNumber = req.SerialNumber
              Location = req.Location
              EfficiencyOverride = req.EfficiencyOverride
              CostRateOverrideAmount = req.CostRateOverrideAmount
              CostRateOverrideCurrency = req.CostRateOverrideCurrency
              CalendarId = req.CalendarId }

    let toRenameCommand (req: PhysicalResourceRenameReq) : Result<RenamePhysicalResourceCmd, DomainError> =
        PhysicalResourceId.create req.Id
        |> Result.map (fun id -> { Id = id; NewName = req.NewName }: RenamePhysicalResourceCmd)

    let toRetireCommand (req: PhysicalResourceRetireReq) : Result<RetirePhysicalResourceCmd, DomainError> =
        PhysicalResourceId.create req.Id
        |> Result.map (fun id -> { Id = id }: RetirePhysicalResourceCmd)

type Decision = Decision<PhysicalResource, PhysicalResourceEvent>

type PhysicalResourceCapabilities =
    { Define: PhysicalResourceDefineReq -> TaskResult<Decision, ApplicationError>
      Rename: PhysicalResourceRenameReq -> TaskResult<Decision, ApplicationError>
      Retire: PhysicalResourceRetireReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<PhysicalResource, string, PhysicalResourceEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> c.Id) repo DefinePhysicalResource decide
      Rename =
        liftCmdResult ACL.toRenameCommand
        >=> handleCommand (fun c -> PhysicalResourceId.value c.Id) repo RenamePhysicalResource decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun c -> PhysicalResourceId.value c.Id) repo RetirePhysicalResource decide }

let mapPhysicalResourceDto (pr: PhysicalResource) : Contracts.Domain.PhysicalResource =
    { Id = PhysicalResourceId.value pr.Id
      StandardResourceId = StandardResourceId.value pr.StandardResourceId
      Name = pr.Name
      SerialNumber = pr.SerialNumber
      Location = pr.Location
      EfficiencyOverride = pr.EfficiencyOverride |> Option.map Percent.value
      CostRateOverrideAmount =
        pr.CostRateOverride
        |> Option.map (fun c -> c.Amount)
      CostRateOverrideCurrency =
        pr.CostRateOverride
        |> Option.map (fun c -> c.Currency)
      CalendarId = pr.CalendarId |> Option.map CalendarId.value
      IsActive =
        match pr.Status with
        | Active -> true
        | Inactive -> false
      Created = Timestamp.value pr.Created
      Modified = Timestamp.value pr.Modified }

let evolveProjection (state: Map<string, Contracts.Domain.PhysicalResource>) (evt: PhysicalResourceEvent) =
    match evt with
    | PhysicalResourceDefined e ->
        let dto: Contracts.Domain.PhysicalResource =
            { Id = PhysicalResourceId.value e.Id
              StandardResourceId = StandardResourceId.value e.StandardResourceId
              Name = e.Name
              SerialNumber = e.SerialNumber
              Location = e.Location
              EfficiencyOverride = e.EfficiencyOverride |> Option.map Percent.value
              CostRateOverrideAmount =
                e.CostRateOverride
                |> Option.map (fun c -> c.Amount)
              CostRateOverrideCurrency =
                e.CostRateOverride
                |> Option.map (fun c -> c.Currency)
              CalendarId = e.CalendarId |> Option.map CalendarId.value
              IsActive = true
              Created = Timestamp.value e.Created
              Modified = Timestamp.value e.Created }

        Map.add dto.Id dto state
    | PhysicalResourceRenamed e ->
        let key = PhysicalResourceId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            Map.add
                key
                { existing with
                    Name = e.NewName
                    Modified = Timestamp.value e.Modified }
                state
        | None -> state
    | PhysicalResourceRetired e ->
        let key = PhysicalResourceId.value e.Id

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
    ProjectionAgent<Map<string, Contracts.Domain.PhysicalResource>, PhysicalResourceEvent>(
        evolveProjection,
        Map.empty,
        "PhysicalResourceReadModel"
    )

let createPhysicalResourceApi (capabilities: PhysicalResourceCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPhysicalResourceDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapPhysicalResourceDto)
      Rename =
        fun req ->
            capabilities.Rename req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPhysicalResourceDto
      Retire =
        fun req ->
            capabilities.Retire req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapPhysicalResourceDto }
    : PhysicalResourceApi
