namespace Medhavi.Scheduler.Mrp.Domain

open System
open Medhavi.SharedKernel
open Medhavi.Scheduler.Mrp.Domain.Types

type PegStatus =
    | Active
    | Superseded
    | Released

type ReservationRef =
    | Material of string
    | Capacity of CapacityReservationId
    | Transport of string

type DemandRef =
    { DemandId: string
      SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      NeedDate: Timestamp
      Quantity: Quantity }

type SupplyRef =
    { SupplyId: string
      ProposalType: ProposalType
      SkuId: SkuId
      NodeId: NodeId
      StockingPointId: StockingPointId
      DeliveryDate: Timestamp
      Quantity: Quantity }

type PegTarget =
    | Supply of SupplyRef
    | Reservation of ReservationRef

type PeggingLink =
    { Id: PeggingId
      Demand: DemandRef
      Target: PegTarget
      PeggedQty: Quantity
      Status: PegStatus
      IsLocked: bool
      Created: DateTimeOffset
      Modified: DateTimeOffset }

type AllocationOrderPolicy =
    | FIFO
    | Priority

type PeggingPolicy =
    { AllocationOrder: AllocationOrderPolicy
      FirmPegProtection: bool
      LockedPegProtection: bool }

module PeggingPolicy =
    let defaultPolicy =
        { AllocationOrder = FIFO
          FirmPegProtection = true
          LockedPegProtection = true }

    let goldPolicy =
        { AllocationOrder = Priority
          FirmPegProtection = true
          LockedPegProtection = true }

    let silverPolicy =
        { AllocationOrder = FIFO
          FirmPegProtection = true
          LockedPegProtection = true }

    let bronzePolicy =
        { AllocationOrder = FIFO
          FirmPegProtection = false
          LockedPegProtection = true }

    let presetCatalog =
        Map [
            "gold", goldPolicy
            "silver", silverPolicy
            "bronze", bronzePolicy
            "default", defaultPolicy
        ]

    let resolvePolicy (tier: string option) : PeggingPolicy =
        tier
        |> Option.bind (fun t -> Map.tryFind (t.ToLowerInvariant()) presetCatalog)
        |> Option.defaultValue defaultPolicy

module PeggingId =
    let createDeterministic (demandId: string) (targetId: string) : PeggingId =
        let idStr = Medhavi.SharedKernel.IdsFactory.DeterministicIds.pegId demandId targetId
        PeggingId.create idStr
        |> Result.defaultWith (fun _ -> failwith "Invalid Pegging ID")

module Traceability =
    let getUpstreamSupplies (demandId: string) (peggings: PeggingLink list) : PegTarget list =
        peggings
        |> List.filter (fun p -> p.Demand.DemandId = demandId && p.Status = Active)
        |> List.map (fun p -> p.Target)

    let getDownstreamDemands (supplyId: string) (peggings: PeggingLink list) : DemandRef list =
        peggings
        |> List.filter (fun p ->
            match p.Target with
            | Supply s -> s.SupplyId = supplyId
            | Reservation _ -> false
            && p.Status = Active)
        |> List.map (fun p -> p.Demand)

module PeggingEngine =
    let demandRefFromMrpDemand (d: MrpDemand) : DemandRef =
        { DemandId = d.DemandId
          SkuId = d.SkuId
          NodeId = d.NodeId
          StockingPointId = d.StockingPointId
          NeedDate = d.RequiredDate
          Quantity = d.Quantity }

    let supplyRefFromProposal (p: SupplyProposal) : SupplyRef =
        { SupplyId = SupplyProposalId.value p.Id
          ProposalType = p.ProposalType
          SkuId = p.SkuId
          NodeId = p.NodeId
          StockingPointId = p.StockingPointId
          DeliveryDate = p.DueDate
          Quantity = p.Quantity }

    let pegSuppliesToDemands
        (policy: PeggingPolicy)
        (demands: MrpDemand list)
        (proposals: SupplyProposal list)
        : PeggingLink list =
        
        let demandGroups = demands |> List.groupBy (fun d -> (d.SkuId, d.StockingPointId)) |> Map.ofList
        let proposalGroups = proposals |> List.groupBy (fun p -> (p.SkuId, p.StockingPointId)) |> Map.ofList
        
        let allKeys = Seq.append (demandGroups.Keys) (proposalGroups.Keys) |> Seq.distinct |> Seq.toList
        
        allKeys
        |> List.collect (fun key ->
            let groupDemands = demandGroups |> Map.tryFind key |> Option.defaultValue []
            let groupProposals = proposalGroups |> Map.tryFind key |> Option.defaultValue []
            
            let sortedDemands =
                match policy.AllocationOrder with
                | FIFO -> groupDemands |> List.sortBy (fun d -> d.RequiredDate)
                | Priority -> 
                    groupDemands 
                    |> List.sortBy (fun d -> -(Option.defaultValue 0 d.Priority), d.RequiredDate)
                    
            let sortedProposals = groupProposals |> List.sortBy (fun p -> p.DueDate)
            
            let rec allocate
                (dLeft: (MrpDemand * decimal) list)
                (pLeft: (SupplyProposal * decimal) list)
                (acc: PeggingLink list) =
                match dLeft, pLeft with
                | [], _
                | _, [] -> acc
                | (d, dQty) :: dRest, (p, pQty) :: pRest ->
                    if dQty <= 0m then
                        allocate dRest pLeft acc
                    elif pQty <= 0m then
                        allocate dLeft pRest acc
                    else
                        let allocatedVal = min dQty pQty
                        let allocatedQty = Quantity.clampToZero allocatedVal
                        
                        let targetSupply = Supply (supplyRefFromProposal p)
                        let link =
                            { Id = PeggingId.createDeterministic d.DemandId (SupplyProposalId.value p.Id)
                              Demand = demandRefFromMrpDemand d
                              Target = targetSupply
                              PeggedQty = allocatedQty
                              Status = PegStatus.Active
                              IsLocked = false
                              Created = DateTimeOffset.UtcNow
                              Modified = DateTimeOffset.UtcNow }
                              
                        let nextDQty = dQty - allocatedVal
                        let nextPQty = pQty - allocatedVal
                        
                        let nextDList = if nextDQty > 0m then (d, nextDQty) :: dRest else dRest
                        let nextPList = if nextPQty > 0m then (p, nextPQty) :: pRest else pRest
                        
                        allocate nextDList nextPList (link :: acc)
                        
            let dAlloc = sortedDemands |> List.map (fun d -> d, Quantity.value d.Quantity)
            let pAlloc = sortedProposals |> List.map (fun p -> p, Quantity.value p.Quantity)
            
            allocate dAlloc pAlloc []
        )
