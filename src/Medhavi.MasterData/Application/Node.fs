module Medhavi.MasterData.Application.Node

open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.SharedKernel
open Medhavi.MasterData.Domain.NodeAgg
open Medhavi.Contracts.Integration
open Medhavi.SharedKernel.Aggregate
open Medhavi.Infrastructure

module ACL =
    let parseNodeType (t: string) : Result<NodeType, DomainError> =
        match t.Trim().ToLowerInvariant() with
        | "plant" -> Ok NodeType.Plant
        | "distributioncenter"
        | "distribution_center"
        | "dc" -> Ok DistributionCenter
        | "warehouse"
        | "wh" -> Ok Warehouse
        | "stockingpoint"
        | "sp" -> Ok StockingPoint
        | "supplier"
        | "vendor" -> Ok Supplier
        | "customer" -> Ok Customer
        | s -> Ok(Other s)

    let toAttributes (req: NodeAttributesReq) : Result<NodeAttributes, DomainError> =
        let make (spRef: StockingPointId option) : NodeAttributes =
            { LocationCode = req.LocationCode
              PlanningLevel = req.PlanningLevel
              StockingPointRef = spRef }

        match req.StockingPointRef with
        | None -> Ok(make None)
        | Some refVal ->
            StockingPointId.create refVal
            |> Result.map (Some >> make)

    let toDefineCommand (req: NodeDefineReq) : Result<DefineNodeCmd, DomainError> =
        let make (nodeType: NodeType) (attrs: NodeAttributes) : DefineNodeCmd =
            { Id = req.Id
              Code = req.Code
              Name = req.Name
              Type = nodeType
              Attributes = attrs
              CreatedAt = Timestamp.create req.Created }

        make <!> (parseNodeType req.Type |> fromResult)
        <*> (toAttributes req.Attributes |> fromResult)
        |> toResult
        |> Result.mapError DomainError.combineValidationErrors

    let toRetireCommand (req: NodeRetireReq) : Result<NodeId, DomainError> = NodeId.create req.Id

type NodeCapabilities =
    { Define: NodeDefineReq -> TaskResult<Decision<Node, NodeEvent>, ApplicationError>
      Retire: NodeRetireReq -> TaskResult<Decision<Node, NodeEvent>, ApplicationError> }

let createCapabilities (repo: Repository<Node, string, NodeEvent>) =
    { Define =
        liftCmdResult ACL.toDefineCommand
        >=> handleCommand (fun (c: DefineNodeCmd) -> c.Id) repo DefineNode decide
      Retire =
        liftCmdResult ACL.toRetireCommand
        >=> handleCommand (fun (id: NodeId) -> NodeId.value id) repo RetireNode decide }
