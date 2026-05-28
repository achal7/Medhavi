module Medhavi.MasterData.Application.Routing

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.RoutingAgg

module ACL =
    let parseRoutingType (t: string) : Result<RoutingType, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "work" -> Ok RoutingType.Work
        | "transport" -> Ok RoutingType.Transport
        | "purchase" -> Ok RoutingType.Purchase
        | _ -> Error(DomainError.validation $"Unknown RoutingType: {t}")

    let toStep (req: RoutingStepReq) : DefineRoutingStep =
        { StepId = req.StepId
          Sequence = req.Sequence
          ResourceGroupId = req.ResourceGroupId
          Yield = req.Yield }

    let toInput (req: RoutingInputReq) : DefineRoutingInput =
        { StepId = req.StepId
          SkuId = req.SkuId
          NodeId = req.NodeId
          ConversionRate = req.ConversionRate }

    let toOutput (req: RoutingOutputReq) : DefineRoutingOutput =
        { StepId = req.StepId
          SkuId = req.SkuId
          NodeId = req.NodeId
          ConversionRate = req.ConversionRate
          IsCoSku = req.IsCoSku }

    let toStepResource (req: StepResourceReq) : DefineStepResourceMap =
        { StepId = req.StepId
          ResourceId = req.ResourceId
          IsAllowed = req.IsAllowed
          Sequence = req.Sequence
          DurationPerUnitMinutes = req.DurationPerUnitMinutes }

    let toDefineCommand (req: RoutingDefineReq) : Result<DefineRoutingCmd, DomainError> =
        let make (rId: RoutingId) (rType: RoutingType) : DefineRoutingCmd =
            { Id = rId
              Name = req.Name
              Type = rType
              EffectiveStart = Timestamp.create req.EffectiveStart
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create
              Steps = req.Steps |> List.map toStep
              Inputs = req.Inputs |> List.map toInput
              Outputs = req.Outputs |> List.map toOutput
              StepResources = req.StepResources |> List.map toStepResource
              CreatedAt = Timestamp.create req.Created
              ModifiedAt = Timestamp.create req.Created
              Status = Status.Active }

        make <!> (RoutingId.create req.Id |> fromResult)
        <*> (parseRoutingType req.Type |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toActivateCommand (req: RoutingActivateReq) : Result<RoutingId, DomainError> = RoutingId.create req.Id

    let toDeactivateCommand (req: RoutingDeactivateReq) : Result<RoutingId, DomainError> = RoutingId.create req.Id

type Decision = Decision<Routing, RoutingEvent>

type RoutingCapabilities =
    { Define: RoutingDefineReq -> TaskResult<Decision, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<Decision, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Routing, string, RoutingEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun c -> RoutingId.value c.Id) repo DefineRouting decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand RoutingId.value repo ActivateRouting decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand RoutingId.value repo DeactivateRouting decide }

let mapRoutingDto (r: Routing) : Medhavi.Contracts.Domain.Routing =
    let tStr =
        match r.Type with
        | RoutingType.Work -> "Work"
        | RoutingType.Transport -> "Transport"
        | RoutingType.Purchase -> "Purchase"

    let steps =
        r.Steps
        |> List.map (fun s ->
            let duration =
                r.StepResources
                |> List.tryFind (fun sr -> sr.StepId = s.StepId)
                |> Option.bind (fun sr -> sr.DurationPerUnitMinutes)

            let step: Medhavi.Contracts.Domain.RoutingStep =
                { StepId = s.StepId
                  Sequence = s.Sequence
                  ResourceGroupId =
                    s.ResourceGroupId
                    |> Option.map ResourceGroupId.value
                  Yield = s.Yield
                  DurationPerUnitMinutes = duration }

            step)

    { Id = RoutingId.value r.Id
      Name = r.Name
      Type = tStr
      Steps = steps
      Status = r.Status.ToBool() }

let evolveProjection (state: Map<string, Medhavi.Contracts.Domain.Routing>) (evt: RoutingEvent) =
    match evt with
    | RoutingDefined r -> Map.add (RoutingId.value r.Id) (mapRoutingDto r) state
    | RoutingActivated(id, _) ->
        let key = RoutingId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = true } state
        | None -> state
    | RoutingDeactivated(id, _) ->
        let key = RoutingId.value id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with Status = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Medhavi.Contracts.Domain.Routing>, RoutingEvent>(
        evolveProjection,
        Map.empty,
        "RoutingReadModel"
    )

let createQueryService agent = QueryServiceBase.getQueryService agent id

open Medhavi.SharedKernel.API

let createRoutingApi (capabilities: RoutingCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapRoutingDto
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map mapRoutingDto)
      Activate =
        fun req ->
            capabilities.Activate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapRoutingDto
      Deactivate =
        fun req ->
            capabilities.Deactivate req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map mapRoutingDto
      QueryService = QueryServiceBase.getQueryService agent id }
    : RoutingApi
