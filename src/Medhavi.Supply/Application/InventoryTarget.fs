module Medhavi.Supply.Application.InventoryTarget

open Medhavi
open Medhavi.Common.Patterns
open Medhavi.Common.Validation
open Medhavi.Contracts.Domain
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.API
open Medhavi.SharedKernel.Aggregate
open Medhavi.Supply.Domain
open Medhavi.Supply.Domain.InventoryTargetAgg

module ACL =
    open Medhavi.SharedKernel.Validations

    let sequenceOpt (opt: Result<'T, 'Err> option) : Result<'T option, 'Err> =
        match opt with
        | None -> Ok None
        | Some(Ok x) -> Ok(Some x)
        | Some(Error e) -> Error e

    let mapPolicy (pOpt: Contracts.Domain.ReplenishmentPolicy option) =
        match pOpt with
        | None -> Valid None
        | Some p ->
            makePolicy p.CoverDays p.Expedite
            <!> (Quantity.create p.Safety |> fromResult)
            <*> (p.MinQty
                 |> Option.map Quantity.create
                 |> sequenceOpt
                 |> fromResult)
            <*> (p.MaxQty
                 |> Option.map Quantity.create
                 |> sequenceOpt
                 |> fromResult)
            <*> (p.LotSize
                 |> Option.map Quantity.create
                 |> sequenceOpt
                 |> fromResult)
            |> map Some

    let mapSeasonal (s: Contracts.Domain.SeasonalAdjustment) : SeasonalAdjustment =
        { PeriodStart = Timestamp.create s.PeriodStart
          PeriodEnd = Timestamp.create s.PeriodEnd
          AdjustmentFactor = s.AdjustmentFactor }

    let toDefineCommand (req: InventoryTargetDefineReq) : Validation<DefineInventoryTargetCmd, DomainError> =
        let make
            (skuId: SkuId)
            (spId: StockingPointId)
            (policy: ReplenishmentPolicy option)
            safety
            min
            max
            : DefineInventoryTargetCmd =
            let id = InventoryTargetId.create skuId spId

            { Id = id
              SkuId = skuId
              StockingPointId = spId
              ReplenishmentPolicy = policy
              SafetyStockQty = safety
              MinQty = min
              MaxQty = max
              TargetServiceLevel = req.TargetServiceLevel
              CoverDays = req.CoverDays
              SeasonalAdjustments = req.SeasonalAdjustments |> List.map mapSeasonal
              EffectiveStart = req.EffectiveStart |> Option.map Timestamp.create
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create
              IsActive = req.IsActive }

        make <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId
             |> fromResult)
        <*> mapPolicy req.ReplenishmentPolicy
        <*> (req.SafetyStockQty
             |> Option.map (nonNegativeDecimal "Safety Stock Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))
        <*> (req.MinQty
             |> Option.map (nonNegativeDecimal "Safety Stock Min Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))
        <*> (req.MaxQty
             |> Option.map (nonNegativeDecimal "Safety Stock Max Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))

    let toUpdateCommand (req: InventoryTargetUpdateReq) : Validation<UpdateInventoryTargetCmd, DomainError> =
        let make
            (skuId: SkuId)
            (spId: StockingPointId)
            (policy: ReplenishmentPolicy option)
            safety
            min
            max
            : UpdateInventoryTargetCmd =

            { Id = InventoryTargetId.create skuId spId
              SkuId = skuId
              StockingPointId = spId
              ReplenishmentPolicy = policy
              SafetyStockQty = safety
              MinQty = min
              MaxQty = max
              TargetServiceLevel = req.TargetServiceLevel
              CoverDays = req.CoverDays
              SeasonalAdjustments =
                req.SeasonalAdjustments
                |> Option.map (List.map mapSeasonal)
              EffectiveStart = req.EffectiveStart |> Option.map Timestamp.create
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create }

        make <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId
             |> fromResult)
        <*> mapPolicy req.ReplenishmentPolicy
        <*> (req.SafetyStockQty
             |> Option.map (nonNegativeDecimal "Safety Stock Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))
        <*> (req.MinQty
             |> Option.map (nonNegativeDecimal "Safety Stock Min Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))
        <*> (req.MaxQty
             |> Option.map (nonNegativeDecimal "Safety Stock Max Quantity")
             |> Option.map (map Some)
             |> Option.defaultValue (Valid None))

    let toActivateCommand (targetIdStr: string) : Result<ActivateInventoryTargetCmd, DomainError> =
        InventoryTargetId.createFromExisting targetIdStr
        |> Result.map (fun id ->
            { Id = id
              SkuId =
                SkuId.create "temp"
                |> function
                    | Ok x -> x
                    | Error _ -> failwith "invalid" // SkuId/StockingPointId not strictly used in activate check
              StockingPointId =
                StockingPointId.create "temp"
                |> function
                    | Ok x -> x
                    | Error _ -> failwith "invalid"
              ModifiedDate = Timestamp.now })

    let toDeactivateCommand (targetIdStr: string) : Result<DeactivateInventoryTargetCmd, DomainError> =
        InventoryTargetId.createFromExisting targetIdStr
        |> Result.map (fun id ->
            { Id = id
              SkuId =
                SkuId.create "temp"
                |> function
                    | Ok x -> x
                    | Error _ -> failwith "invalid"
              StockingPointId =
                StockingPointId.create "temp"
                |> function
                    | Ok x -> x
                    | Error _ -> failwith "invalid"
              ModifiedDate = Timestamp.now })

    let toContractPolicy (p: ReplenishmentPolicy) : Contracts.Domain.ReplenishmentPolicy =
        { Safety = Quantity.value p.Safety
          MinQty = p.MinQty |> Option.map Quantity.value
          MaxQty = p.MaxQty |> Option.map Quantity.value
          CoverDays = p.CoverDays
          LotSize = p.LotSize |> Option.map Quantity.value
          Expedite = p.Expedite }

    let toContractSeasonal (s: SeasonalAdjustment) : Contracts.Domain.SeasonalAdjustment =
        { PeriodStart = Timestamp.value s.PeriodStart
          PeriodEnd = Timestamp.value s.PeriodEnd
          AdjustmentFactor = s.AdjustmentFactor }

    let toContract (t: InventoryTarget) : Contracts.Domain.InventoryTarget =
        { Id = InventoryTargetId.value t.Id
          SkuId = SkuId.value t.SkuId
          StockingPointId = StockingPointId.value t.StockingPointId
          ReplenishmentPolicy =
            t.ReplenishmentPolicy
            |> Option.map toContractPolicy
          SafetyStockQty = t.SafetyStockQty |> Option.map Quantity.value
          MinQty = t.MinQty |> Option.map Quantity.value
          MaxQty = t.MaxQty |> Option.map Quantity.value
          TargetServiceLevel = t.TargetServiceLevel
          CoverDays = t.CoverDays
          SeasonalAdjustments =
            t.SeasonalAdjustments
            |> List.map toContractSeasonal
          EffectiveStart = t.EffectiveStart |> Option.map Timestamp.value
          EffectiveEnd = t.EffectiveEnd |> Option.map Timestamp.value
          IsActive = t.IsActive }

type Decision = Decision<InventoryTarget, InventoryTargetEvent>

type InventoryTargetCapabilities =
    { Define: InventoryTargetDefineReq -> TaskResult<Decision, ApplicationError>
      Update: InventoryTargetUpdateReq -> TaskResult<Decision, ApplicationError>
      Activate: string -> TaskResult<Decision, ApplicationError>
      Deactivate: string -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<InventoryTarget, string, InventoryTargetEvent>) =
    { Define =
        liftCmdValidation ACL.toDefineCommand
        >=> handleCommand (fun cmd -> InventoryTargetId.value cmd.Id) repo DefineInventoryTarget decide

      Update =
        liftCmdValidation ACL.toUpdateCommand
        >=> handleCommand (fun cmd -> InventoryTargetId.value cmd.Id) repo UpdateInventoryTarget decide

      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand (fun cmd -> InventoryTargetId.value cmd.Id) repo ActivateInventoryTarget decide

      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand (fun cmd -> InventoryTargetId.value cmd.Id) repo DeactivateInventoryTarget decide }

let evolveProjection (state: Map<string, Contracts.Domain.InventoryTarget>) (evt: InventoryTargetEvent) =
    match evt with
    | InventoryTargetDefined e -> Map.add (InventoryTargetId.value e.Id) (ACL.toContract (applyDefinedEvent e)) state
    | InventoryTargetUpdated e ->
        let key = InventoryTargetId.value e.Id

        match Map.tryFind key state with
        | Some existing ->
            // Reconstruct the domain target state to update it
            // Or since we already have mapping, let's update it in the projection map
            let updatedPolicy =
                e.ReplenishmentPolicy
                |> Option.map ACL.toContractPolicy

            let updatedSeasonal =
                e.SeasonalAdjustments
                |> Option.map (List.map ACL.toContractSeasonal)

            let newTarget =
                { existing with
                    ReplenishmentPolicy =
                        updatedPolicy
                        |> Option.orElse existing.ReplenishmentPolicy
                    SafetyStockQty =
                        e.SafetyStockQty
                        |> Option.map Quantity.value
                        |> Option.orElse existing.SafetyStockQty
                    MinQty =
                        e.MinQty
                        |> Option.map Quantity.value
                        |> Option.orElse existing.MinQty
                    MaxQty =
                        e.MaxQty
                        |> Option.map Quantity.value
                        |> Option.orElse existing.MaxQty
                    TargetServiceLevel =
                        e.TargetServiceLevel
                        |> Option.orElse existing.TargetServiceLevel
                    CoverDays = e.CoverDays |> Option.orElse existing.CoverDays
                    SeasonalAdjustments =
                        updatedSeasonal
                        |> Option.defaultValue existing.SeasonalAdjustments
                    EffectiveStart =
                        e.EffectiveStart
                        |> Option.map Timestamp.value
                        |> Option.orElse existing.EffectiveStart
                    EffectiveEnd =
                        e.EffectiveEnd
                        |> Option.map Timestamp.value
                        |> Option.orElse existing.EffectiveEnd }

            Map.add key newTarget state
        | None -> state
    | InventoryTargetActivated e ->
        let key = InventoryTargetId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with IsActive = true } state
        | None -> state
    | InventoryTargetDeactivated e ->
        let key = InventoryTargetId.value e.Id

        match Map.tryFind key state with
        | Some existing -> Map.add key { existing with IsActive = false } state
        | None -> state

let createProjectionAgent () =
    ProjectionAgent<Map<string, Contracts.Domain.InventoryTarget>, InventoryTargetEvent>(
        evolveProjection,
        Map.empty,
        "InventoryTargetReadModel"
    )

let createInventoryTargetApi (capabilities: InventoryTargetCapabilities) agent =
    { Define =
        fun req ->
            capabilities.Define req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      DefineBulk =
        fun reqs ->
            reqs
            |> List.map capabilities.Define
            |> TaskResult.sequence
            |> TaskResult.map (fun decisions ->
                decisions
                |> List.map (fun d -> d.NewState)
                |> List.map ACL.toContract)
      Update =
        fun req ->
            capabilities.Update req
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Activate =
        fun reqId ->
            capabilities.Activate reqId
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      Deactivate =
        fun reqId ->
            capabilities.Deactivate reqId
            |> TaskResult.map (fun d -> d.NewState)
            |> TaskResult.map ACL.toContract
      QueryService = QueryServiceBase.getQueryService agent id }
    : InventoryTargetApi
