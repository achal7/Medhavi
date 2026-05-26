module Medhavi.MasterData.Application.Routing

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.RoutingAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate

module ACL =
    let parseRoutingType (t: string) : Result<RoutingType, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "work" -> Ok RoutingType.Work
        | "transport" -> Ok RoutingType.Transport
        | "purchase" -> Ok RoutingType.Purchase
        | _ -> Error (DomainError.validation $"Unknown RoutingType: {t}")

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
              Status = RoutingStatus.Active }

        make
        <!> (RoutingId.create req.Id |> fromResult)
        <*> (parseRoutingType req.Type |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toActivateCommand (req: RoutingActivateReq) : Result<RoutingId, DomainError> =
        RoutingId.create req.Id

    let toDeactivateCommand (req: RoutingDeactivateReq) : Result<RoutingId, DomainError> =
        RoutingId.create req.Id

type RoutingCapabilities =
    { Define: RoutingDefineReq -> TaskResult<RoutingEvent list, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<RoutingEvent list, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<RoutingEvent list, ApplicationError> }

let createCapabilities (repo: Repository<Routing, string, RoutingEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefineRoutingCmd) -> RoutingId.value c.Id) repo DefineRouting decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand (fun id -> RoutingId.value id) repo ActivateRouting decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand (fun id -> RoutingId.value id) repo DeactivateRouting decide }
