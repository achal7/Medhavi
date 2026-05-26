module Medhavi.MasterData.Domain.NodeAgg

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations
open Medhavi.SharedKernel.Aggregate

type NodeType =
    | Plant
    | DistributionCenter
    | Warehouse
    | StockingPoint
    | Supplier
    | Customer
    | Other of string

type NodeAttributes =
    { LocationCode: string option
      PlanningLevel: int option
      StockingPointRef: StockingPointId option }

type NodeStatus =
    | Active
    | Retired

type Node =
    { Id: NodeId
      Code: string
      Type: NodeType
      Attributes: NodeAttributes
      CreatedAt: Timestamp
      ModifiedAt: Timestamp
      Status: NodeStatus }

type DefineNodeCmd =
    { Id: string
      Code: string
      Name: string
      Type: NodeType
      Attributes: NodeAttributes
      CreatedAt: Timestamp }

type NodeCommand =
    | DefineNode of DefineNodeCmd
    | RetireNode of id: NodeId

type NodeEvent =
    | NodeDefined of Node
    | NodeRetired of NodeId * Timestamp

type DecideNode = Decide<Node, NodeCommand, NodeEvent>
type EvolveNode = Evolve<Node, NodeEvent>

let createNode now nodetype attrs id code =
    { Id = id
      Code = code
      Type = nodetype
      Attributes = attrs
      CreatedAt = now
      ModifiedAt = Timestamp.minValue
      Status = Active }

let validateDefineNode now (cmd: DefineNodeCmd) =
    createNode now cmd.Type cmd.Attributes
    <!> (NodeId.create cmd.Id |> fromResult)
    <*> required "Node code" cmd.Code

let decide: DecideNode =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineNode cmd, None ->
            createAggregate (validateDefineNode Timestamp.now) (fun node -> [ NodeDefined node ]) cmd

        | DefineNode _, Some _ -> Error(DomainError.invariant "Node already exists")

        | RetireNode(id), Some state when state.Id = id ->
            match state.Status with
            | Retired -> Error(DomainError.invariant "Node is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Retired
                        ModifiedAt = Timestamp.now }

                { NewState = updated
                  Events = [ NodeRetired(id, updated.ModifiedAt) ] }
                |> Ok

        | _, Some _ -> Error(DomainError.validation "Invalid command and state combination")
        | _, None -> Error(DomainError.validation "Node not found")

let evolve: EvolveNode =
    fun event stateOpt ->
        match event, stateOpt with
        | NodeDefined state, None -> Some state
        | NodeRetired(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = Retired
                    ModifiedAt = modifiedAt }
        | NodeDefined _, Some state -> Some state
        | _, current -> current
