module Medhavi.MasterData.Application.Routing

open System
open Medhavi.Common.Validation
open Medhavi.Common.Patterns
open Medhavi.Contracts.Integration
open Medhavi.Infrastructure
open Medhavi.Infrastructure.Projections
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Aggregate
open Medhavi.MasterData.Domain.RoutingAgg
open Medhavi.MasterData.Domain.RoutingAgg.Commands

module ACL =
    let mapErr r =
        r
        |> Result.mapError (fun err -> [ DomainError.validation err ])

    let toApplicability (req: RoutingDefineReq) : DefineRoutingApplicability =
        { StockingPointId = req.StockingPointId
          EffectiveStart = Timestamp.create req.EffectiveStart
          EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create }

    let toPreference (req: RoutingDefineReq) : RoutingPreference =
        { Priority = req.PreferencePriority
          IsPreferred = req.IsPreferred }

    let toQuantityRule (req: RoutingDefineReq) : DefineRoutingQuantityRule =
        { MinQuantity = req.MinQuantity
          MaxQuantity = req.MaxQuantity
          LotSize = req.LotSize
          OrderMultiple = req.OrderMultiple }

    let toCostPolicy (req: RoutingDefineReq) : Validation<DefineRoutingCostPolicy, DomainError> =
        match req.CostPolicyType.Trim().ToLowerInvariant() with
        | "noroutingcost" -> Valid DefineRoutingCostPolicy.NoRoutingCost
        | "fixedcost" ->
            match req.CostPolicyValue with
            | Some v -> Valid(DefineRoutingCostPolicy.FixedCost v)
            | None -> Invalid [ DomainError.validation "FixedCost requires a cost value" ]
        | "costperunit" ->
            match req.CostPolicyValue with
            | Some v -> Valid(DefineRoutingCostPolicy.CostPerUnit v)
            | None -> Invalid [ DomainError.validation "CostPerUnit requires a cost value" ]
        | _ -> Invalid [ DomainError.validation $"Unknown CostPolicyType: {req.CostPolicyType}" ]

    let toStepInputTiming (t: StepInputTimingReq) : Result<StepInputTiming, string> =
        match t with
        | StepInputTimingReq.AtStepStart -> Ok StepInputTiming.AtStepStart
        | StepInputTimingReq.AtStepEnd -> Ok StepInputTiming.AtStepEnd
        | StepInputTimingReq.OffsetBeforeStepStart v ->
            DurationMinutes.create v
            |> Result.map StepInputTiming.OffsetBeforeStepStart
        | StepInputTimingReq.OffsetAfterStepStart v ->
            DurationMinutes.create v
            |> Result.map StepInputTiming.OffsetAfterStepStart
        | StepInputTimingReq.OffsetBeforeStepEnd v ->
            DurationMinutes.create v
            |> Result.map StepInputTiming.OffsetBeforeStepEnd
        | StepInputTimingReq.OffsetAfterStepEnd v ->
            DurationMinutes.create v
            |> Result.map StepInputTiming.OffsetAfterStepEnd

    let toStepOutputTiming (t: StepOutputTimingReq) =
        match t with
        | StepOutputTimingReq.AtStepStart -> Ok StepOutputTiming.AtStepStart
        | StepOutputTimingReq.AtStepEnd -> Ok StepOutputTiming.AtStepEnd
        | StepOutputTimingReq.OffsetAfterStepStart v ->
            DurationMinutes.create v
            |> Result.map StepOutputTiming.OffsetAfterStepStart
        | StepOutputTimingReq.OffsetAfterStepEnd v ->
            DurationMinutes.create v
            |> Result.map StepOutputTiming.OffsetAfterStepEnd

    let toRoutingOutputRole (r: RoutingOutputRoleReq) : RoutingOutputRole =
        match r with
        | RoutingOutputRoleReq.PrimaryOutput -> RoutingOutputRole.PrimaryOutput
        | RoutingOutputRoleReq.CoProduct -> RoutingOutputRole.CoProduct
        | RoutingOutputRoleReq.ByProduct -> RoutingOutputRole.ByProduct
        | RoutingOutputRoleReq.Scrap -> RoutingOutputRole.Scrap
        | RoutingOutputRoleReq.Waste -> RoutingOutputRole.Waste

    let toStepInput (req: RoutingStepInputReq) : Validation<DefineRoutingStepInput, DomainError> =
        toStepInputTiming req.Timing
        |> Result.map (fun timing ->
            { SkuId = req.SkuId
              FromNodeId = req.FromNodeId
              QuantityPerBaseOutput = req.QuantityPerBaseOutput
              Timing = timing
              IsConsumed = req.IsConsumed
              IsOptional = req.IsOptional })
        |> Result.mapError DomainError.validation
        |> fromResult

    let toStepOutput (req: RoutingStepOutputReq) : Validation<DefineRoutingStepOutput, DomainError> =
        toStepOutputTiming req.Timing
        |> Result.map (fun timing ->
            { SkuId = req.SkuId
              ToNodeId = req.ToNodeId
              QuantityRatioToPrimaryOutput = req.QuantityRatioToPrimaryOutput
              Role = toRoutingOutputRole req.Role
              Timing = timing })
        |> Result.mapError DomainError.validation
        |> fromResult

    let toResourceKind (k: string) : RoutingResourceKind =
        match k.Trim().ToLowerInvariant() with
        | "machine" -> RoutingResourceKind.Machine
        | "workcenter" -> RoutingResourceKind.WorkCenter
        | "laborpool" -> RoutingResourceKind.LaborPool
        | "tool" -> RoutingResourceKind.Tool
        | "utility" -> RoutingResourceKind.Utility
        | "berth" -> RoutingResourceKind.Berth
        | "conveyor" -> RoutingResourceKind.Conveyor
        | "railtrack" -> RoutingResourceKind.RailTrack
        | "truckfleet" -> RoutingResourceKind.TruckFleet
        | "vesselclass" -> RoutingResourceKind.VesselClass
        | _ -> RoutingResourceKind.Machine

    let toLoadBasis (b: string) : ResourceLoadBasis =
        match b.Trim().ToLowerInvariant() with
        | "perorder" -> ResourceLoadBasis.PerOrder
        | "perunit" -> ResourceLoadBasis.PerUnit
        | "perbatch" -> ResourceLoadBasis.PerBatch
        | "pertonne" -> ResourceLoadBasis.PerTonne
        | "perpallet" -> ResourceLoadBasis.PerPallet
        | "percontainer" -> ResourceLoadBasis.PerContainer
        | _ -> ResourceLoadBasis.PerUnit

    let toSelectionRule (r: string) : ResourceSelectionRule =
        match r.Trim().ToLowerInvariant() with
        | "anyallowed" -> ResourceSelectionRule.AnyAllowed
        | "preferprimary" -> ResourceSelectionRule.PreferPrimary
        | "preferlowestcost" -> ResourceSelectionRule.PreferLowestCost
        | "preferfastest" -> ResourceSelectionRule.PreferFastest
        | "preferpriorityorder" -> ResourceSelectionRule.PreferPriorityOrder
        | _ -> ResourceSelectionRule.AnyAllowed

    let toResourceUsage (u: string) : ResourceUsage =
        match u.Trim().ToLowerInvariant() with
        | "primary" -> ResourceUsage.Primary
        | "alternate" -> ResourceUsage.Alternate
        | "optional" -> ResourceUsage.Optional
        | "parallel" -> ResourceUsage.Parallel
        | "rework" -> ResourceUsage.Rework
        | _ -> ResourceUsage.Primary

    let toResourceOption (req: RoutingResourceOptionReq) =
        let toSetup (sOpt: decimal option) =
            sOpt
            |> Option.map (
                DurationMinutes.create
                >> (Result.mapError (fun e -> [ DomainError.validation e ]))
                >> (Result.map SetupPolicy.FixedSetup)
            )
            |> Option.defaultValue (Ok SetupPolicy.NoSetup)

        let toCooling (cOpt: decimal option) =
            cOpt
            |> Option.map (
                DurationMinutes.create
                >> (Result.mapError (fun e -> [ DomainError.validation e ]))
                >> (Result.map CoolingPolicy.FixedCooling)
            )
            |> Option.defaultValue (Ok CoolingPolicy.NoCooling)

        let make eff setupPolicy coolingPolicy : DefineRoutingResourceOption =
            { OptionId = req.OptionId
              ResourceGroupId = req.ResourceGroupId
              ResourceId = req.ResourceId
              WorkCenterId = req.WorkCenterId
              Usage = toResourceUsage req.Usage
              Priority = req.Priority
              SetupTime = req.SetupTimeMinutes
              RunTimePerBaseQuantity = req.RunTimePerBaseQuantityMinutes
              TeardownTime = req.TeardownTimeMinutes
              CoolingTime = req.CoolingTimeMinutes
              MinLeadTime = req.MinLeadTimeMinutes
              CostPerMinute = req.CostPerMinute
              EfficiencyPolicy = eff
              SetupPolicy = setupPolicy
              CoolingPolicy = coolingPolicy
              EffectiveStart = req.EffectiveStart |> Option.map Timestamp.create
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create }

        make req.EfficiencyFactor
        <!> (toSetup req.SetupTimeFixed |> fromResult)
        <*> (toCooling req.CoolingTimeFixed |> fromResult)

    let toResourceRequirement
        (req: RoutingStepResourceRequirementReq)
        : Validation<DefineRoutingStepResourceRequirement, DomainError> =
        let make options : DefineRoutingStepResourceRequirement =
            { RequirementId = req.RequirementId
              ResourceKind = toResourceKind req.ResourceKind
              LoadBasis = toLoadBasis req.LoadBasis
              RequiredUnits = req.RequiredUnits
              SelectionRule = toSelectionRule req.SelectionRule
              Options = options }

        let optionsResult =
            req.Options
            |> List.map toResourceOption
            |> sequence
            |> mapError DomainError.combineValidationErrors

        make <!> optionsResult

    let toStepTimingProfile (t: StepTimingProfileReq) : DefineStepTimingProfile =
        { FixedLeadTime = t.FixedLeadTime
          QueueTime = t.QueueTime
          WaitTime = t.WaitTime
          MoveTime = t.MoveTime }

    let toYieldPolicy (yieldPct: decimal option) : Result<StepYieldPolicy, DomainError> =
        match yieldPct with
        | None -> Ok StepYieldPolicy.NoYieldLoss
        | Some v ->
            Percent.create v
            |> Result.map StepYieldPolicy.ExpectedYield

    let toReworkPolicy (reworkStepId: string option) (reworkRate: decimal option) =
        match reworkStepId, reworkRate with
        | Some stepId, Some rate ->
            RoutingStepId.create stepId
            |> Result.bind (fun stepId ->
                Percent.create rate
                |> Result.map (fun rate -> ReworkPolicy.ReworkToStep(stepId, rate)))
            |> Result.mapError (fun err -> DomainError.validation (sprintf "Rework step id: %A" err))
        | _ -> Ok ReworkPolicy.NoRework

    let toOverlapPolicy (policyType: string) (policyValue: decimal option) =
        match policyType.Trim().ToLowerInvariant() with
        | "nooverlap" -> Ok StepOverlapPolicy.NoOverlap
        | "overlapafterquantity" ->
            policyValue
            |> Option.map (
                Quantity.create
                >> (Result.map StepOverlapPolicy.OverlapAfterQuantity)
            )
            |> Option.defaultValue (Ok StepOverlapPolicy.NoOverlap)
            |> Result.mapError (fun err -> DomainError.validation (sprintf "Overlap after quantity: %A" err))
        | "overlapafterduration" ->
            policyValue
            |> Option.map (
                DurationMinutes.create
                >> (Result.map StepOverlapPolicy.OverlapAfterDuration)
            )
            |> Option.defaultValue (Ok StepOverlapPolicy.NoOverlap)
            |> Result.mapError (fun err -> DomainError.validation (sprintf "Overlap after duration: %A" err))
        | _ -> Ok StepOverlapPolicy.NoOverlap

    let toStepKind (k: string) : RoutingStepKind =
        match k.Trim().ToLowerInvariant() with
        | "standard" -> RoutingStepKind.Standard
        | "alternate" -> RoutingStepKind.Alternate
        | "parallel" -> RoutingStepKind.Parallel
        | "rework" -> RoutingStepKind.Rework
        | "external" -> RoutingStepKind.External
        | _ -> RoutingStepKind.Standard

    let toStep (req: RoutingStepReq) : Validation<DefineRoutingStep, DomainError> =
        let make stepId kind inputs outputs reqs timing yieldPolicy rework overlap : DefineRoutingStep =
            { StepId = stepId
              Sequence = req.Sequence
              OperationCode = req.OperationCode
              Name = req.Name
              Description = req.Description
              Kind = kind
              Inputs = inputs
              Outputs = outputs
              ResourceRequirements = reqs
              TimingProfile = timing
              YieldPolicy = yieldPolicy
              ReworkPolicy = rework
              OverlapPolicy = overlap
              EffectiveStart = req.EffectiveStart |> Option.map Timestamp.create
              EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create }

        let inputsResult = req.Inputs |> List.map toStepInput |> sequence
        let outputsResult = req.Outputs |> List.map toStepOutput |> sequence

        let reqsResult =
            req.ResourceRequirements
            |> List.map toResourceRequirement
            |> sequence

        make <!> (Ok req.StepId |> fromResult)
        <*> (Ok(toStepKind req.Kind) |> fromResult)
        <*> inputsResult
        <*> outputsResult
        <*> reqsResult
        <*> (Ok(toStepTimingProfile req.TimingProfile)
             |> fromResult)
        <*> (toYieldPolicy req.YieldPercentage |> fromResult)
        <*> (toReworkPolicy req.ReworkStepId req.ReworkRate
             |> fromResult)
        <*> (toOverlapPolicy req.OverlapPolicyType req.OverlapPolicyValue
             |> fromResult)

    let toTransportMode (m: string) : TransportMode =
        match m.Trim().ToLowerInvariant() with
        | "road" -> TransportMode.Road
        | "rail" -> TransportMode.Rail
        | "sea" -> TransportMode.Sea
        | "air" -> TransportMode.Air
        | "pipeline" -> TransportMode.Pipeline
        | "conveyor" -> TransportMode.Conveyor
        | "internaltransfer" -> TransportMode.InternalTransfer
        | _ -> TransportMode.Road

    let toTransportResourceOption (req: TransportResourceOptionReq) =
        if req.TransitTime < 0.0m then
            Error [ DomainError.validation "Transit time cannot be negative" ]
        else
            Ok
                { OptionId = req.OptionId
                  ResourceGroupId = req.ResourceGroupId
                  ResourceId = req.ResourceId
                  CarrierId = req.CarrierId
                  Usage = toResourceUsage req.Usage
                  Priority = req.Priority
                  TransitTime = req.TransitTime
                  LoadingTime = req.LoadingTime
                  UnloadingTime = req.UnloadingTime
                  CostPerUnit = req.CostPerUnit
                  CostPerTrip = req.CostPerTrip
                  EffectiveStart = req.EffectiveStart |> Option.map Timestamp.create
                  EffectiveEnd = req.EffectiveEnd |> Option.map Timestamp.create }

    let toTransportDetails (req: TransportRoutingDetailsReq) =

        req.TransportResourceOptions
        |> List.map toTransportResourceOption
        |> Medhavi.Common.Result.sequence
        |> Result.map (fun opts ->
            { SkuId = req.SkuId
              FromNodeId = req.FromNodeId
              ToNodeId = req.ToNodeId
              Mode = toTransportMode req.Mode
              TransitLeadTime = req.TransitLeadTime
              LossFactor = req.LossFactor
              ResourceSelectionRule = toSelectionRule req.ResourceSelectionRule
              TransportResourceOptions = opts })

    let toPurchasePricingPolicy (policyType: string) (valueOpt: string option) : DefinePurchasePricingPolicy =
        match policyType.Trim().ToLowerInvariant() with
        | "nopurchasecost" -> DefinePurchasePricingPolicy.NoPurchaseCost
        | "purchasecostperunit" ->
            match valueOpt with
            | Some s ->
                match Decimal.TryParse(s) with
                | true, v -> DefinePurchasePricingPolicy.PurchaseCostPerUnit v
                | false, _ -> DefinePurchasePricingPolicy.NoPurchaseCost
            | None -> DefinePurchasePricingPolicy.NoPurchaseCost
        | "contractpricereference" ->
            match valueOpt with
            | Some ref -> DefinePurchasePricingPolicy.ContractPriceReference ref
            | None -> DefinePurchasePricingPolicy.NoPurchaseCost
        | _ -> DefinePurchasePricingPolicy.NoPurchaseCost

    let toPurchaseDetails (req: PurchaseRoutingDetailsReq) =
        { SkuId = req.SkuId
          SupplierId = req.SupplierId
          ReceivingNodeId = req.ReceivingNodeId
          SupplierShipFromNodeId = req.SupplierShipFromNodeId
          SupplierLeadTime = req.SupplierLeadTime
          InspectionLeadTime = req.InspectionLeadTime
          PutawayLeadTime = req.PutawayLeadTime
          SupplierSkuCode = req.SupplierSkuCode
          SupplierPreference =
            { Priority = req.SupplierPriority
              IsPreferred = req.SupplierIsPreferred }
          PurchasePricingPolicy = toPurchasePricingPolicy req.PricingPolicyType req.PricingPolicyValue }

    let toDefineCommand (req: RoutingDefineReq) : Validation<DefineRoutingCmd, DomainError> =
        let make
            (rId: RoutingId)
            (costPolicy: DefineRoutingCostPolicy)
            (details: DefineRoutingDetails)
            : DefineRoutingCmd =
            { Id = rId
              Name = req.Name
              Description = req.Description
              Applicability = toApplicability req
              Priority = req.PreferencePriority
              IsPreffered = req.IsPreferred
              QuantityRule = toQuantityRule req
              CostPolicy = costPolicy
              Details = details
              CreatedAt = Timestamp.create req.Created
              ModifiedAt = Timestamp.now }

        let detailsResult =
            match req.Details with
            | WorkDetails w ->
                let stepsResult = w.Steps |> List.map toStep |> sequence

                stepsResult
                |> map (fun steps ->
                    DefineRoutingDetails.Work
                        { ProductId = w.ProductId
                          PrimaryOutputSkuId = w.PrimaryOutputSkuId
                          BaseOutputQuantity = w.BaseOutputQuantity
                          Steps = steps })
            | TransportDetails t ->
                toTransportDetails t
                |> Result.map DefineRoutingDetails.Transport
                |> Result.mapError DomainError.combineValidationErrors
                |> fromResult
            | PurchaseDetails p ->
                toPurchaseDetails p
                |> DefineRoutingDetails.Purchase
                |> Valid

        make <!> (RoutingId.create req.Id |> fromResult)
        <*> (toCostPolicy req)
        <*> detailsResult

    let toActivateCommand (req: RoutingActivateReq) : Result<RoutingId, DomainError> = RoutingId.create req.Id

    let toDeactivateCommand (req: RoutingDeactivateReq) : Result<RoutingId, DomainError> = RoutingId.create req.Id

type Decision = Decision<Routing, RoutingEvent>

type RoutingCapabilities =
    { Define: RoutingDefineReq -> TaskResult<Decision, ApplicationError>
      Activate: RoutingActivateReq -> TaskResult<Decision, ApplicationError>
      Deactivate: RoutingDeactivateReq -> TaskResult<Decision, ApplicationError> }

let createCapabilities (repo: Repository<Routing, string, RoutingEvent>) =
    { Define =
        liftCmdValidation ACL.toDefineCommand
        >=> handleCommand (fun c -> RoutingId.value c.Id) repo DefineRouting decide
      Activate =
        liftCmdResult ACL.toActivateCommand
        >=> handleCommand RoutingId.value repo ActivateRouting decide
      Deactivate =
        liftCmdResult ACL.toDeactivateCommand
        >=> handleCommand RoutingId.value repo DeactivateRouting decide }

module Mappers =
    let mapApplicability (app: RoutingApplicability) : Medhavi.Contracts.Domain.RoutingApplicability =
        { StockingPointId =
            app.StockingPointId
            |> Option.map StockingPointId.value
          EffectiveStart = Timestamp.value app.EffectivePeriod.Start
          EffectiveEnd =
            app.EffectivePeriod.End
            |> Option.map (fun t -> Timestamp.value t) }

    let mapPreference (pref: RoutingPreference) : Medhavi.Contracts.Domain.RoutingPreference =
        { Priority = pref.Priority
          IsPreferred = pref.IsPreferred }

    let mapQuantityRule (rule: RoutingQuantityRule) : Medhavi.Contracts.Domain.RoutingQuantityRule =
        { MinQuantity =
            rule.MinQuantity
            |> Option.map PositiveDecimal.value
          MaxQuantity =
            rule.MaxQuantity
            |> Option.map PositiveDecimal.value
          LotSize = rule.LotSize |> Option.map PositiveDecimal.value
          OrderMultiple =
            rule.OrderMultiple
            |> Option.map PositiveDecimal.value }

    let mapCostPolicy (policy: RoutingCostPolicy) : Medhavi.Contracts.Domain.RoutingCostPolicy =
        match policy with
        | RoutingCostPolicy.NoRoutingCost -> Medhavi.Contracts.Domain.RoutingCostPolicy.NoRoutingCost
        | RoutingCostPolicy.FixedCost v -> Medhavi.Contracts.Domain.RoutingCostPolicy.FixedCost(PositiveDecimal.value v)
        | RoutingCostPolicy.CostPerUnit v ->
            Medhavi.Contracts.Domain.RoutingCostPolicy.CostPerUnit(PositiveDecimal.value v)

    let mapStepInputTiming (t: StepInputTiming) : Medhavi.Contracts.Domain.StepInputTiming =
        match t with
        | StepInputTiming.AtStepStart -> Medhavi.Contracts.Domain.StepInputTiming.AtStepStart
        | StepInputTiming.AtStepEnd -> Medhavi.Contracts.Domain.StepInputTiming.AtStepEnd
        | StepInputTiming.OffsetBeforeStepStart v ->
            Medhavi.Contracts.Domain.StepInputTiming.OffsetBeforeStepStart(DurationMinutes.value v)
        | StepInputTiming.OffsetAfterStepStart v ->
            Medhavi.Contracts.Domain.StepInputTiming.OffsetAfterStepStart(DurationMinutes.value v)
        | StepInputTiming.OffsetBeforeStepEnd v ->
            Medhavi.Contracts.Domain.StepInputTiming.OffsetBeforeStepEnd(DurationMinutes.value v)
        | StepInputTiming.OffsetAfterStepEnd v ->
            Medhavi.Contracts.Domain.StepInputTiming.OffsetAfterStepEnd(DurationMinutes.value v)

    let mapStepOutputTiming (t: StepOutputTiming) : Medhavi.Contracts.Domain.StepOutputTiming =
        match t with
        | StepOutputTiming.AtStepStart -> Medhavi.Contracts.Domain.StepOutputTiming.AtStepStart
        | StepOutputTiming.AtStepEnd -> Medhavi.Contracts.Domain.StepOutputTiming.AtStepEnd
        | StepOutputTiming.OffsetAfterStepStart v ->
            Medhavi.Contracts.Domain.StepOutputTiming.OffsetAfterStepStart(DurationMinutes.value v)
        | StepOutputTiming.OffsetAfterStepEnd v ->
            Medhavi.Contracts.Domain.StepOutputTiming.OffsetAfterStepEnd(DurationMinutes.value v)

    let mapRoutingOutputRole (r: RoutingOutputRole) : Medhavi.Contracts.Domain.RoutingOutputRole =
        match r with
        | RoutingOutputRole.PrimaryOutput -> Medhavi.Contracts.Domain.RoutingOutputRole.PrimaryOutput
        | RoutingOutputRole.CoProduct -> Medhavi.Contracts.Domain.RoutingOutputRole.CoProduct
        | RoutingOutputRole.ByProduct -> Medhavi.Contracts.Domain.RoutingOutputRole.ByProduct
        | RoutingOutputRole.Scrap -> Medhavi.Contracts.Domain.RoutingOutputRole.Scrap
        | RoutingOutputRole.Waste -> Medhavi.Contracts.Domain.RoutingOutputRole.Waste

    let mapStepInput (i: RoutingStepInput) : Medhavi.Contracts.Domain.RoutingStepInput =
        { SkuId = SkuId.value i.SkuId
          FromNodeId = i.FromNodeId |> Option.map NodeId.value
          QuantityPerBaseOutput =
            i.QuantityPerBaseOutput
            |> Option.map PositiveDecimal.value
          Timing = mapStepInputTiming i.Timing
          IsConsumed = i.IsConsumed
          IsOptional = i.IsOptional }

    let mapStepOutput (o: RoutingStepOutput) : Medhavi.Contracts.Domain.RoutingStepOutput =
        { SkuId = SkuId.value o.SkuId
          ToNodeId = o.ToNodeId |> Option.map NodeId.value
          QuantityRatioToPrimaryOutput =
            o.QuantityRatioToPrimaryOutput
            |> Option.map PositiveDecimal.value
          Role = mapRoutingOutputRole o.Role
          Timing = mapStepOutputTiming o.Timing }

    let mapResourceKind (k: RoutingResourceKind) : Medhavi.Contracts.Domain.RoutingResourceKind =
        match k with
        | RoutingResourceKind.Machine -> Medhavi.Contracts.Domain.RoutingResourceKind.Machine
        | RoutingResourceKind.WorkCenter -> Medhavi.Contracts.Domain.RoutingResourceKind.WorkCenter
        | RoutingResourceKind.LaborPool -> Medhavi.Contracts.Domain.RoutingResourceKind.LaborPool
        | RoutingResourceKind.Tool -> Medhavi.Contracts.Domain.RoutingResourceKind.Tool
        | RoutingResourceKind.Utility -> Medhavi.Contracts.Domain.RoutingResourceKind.Utility
        | RoutingResourceKind.Berth -> Medhavi.Contracts.Domain.RoutingResourceKind.Berth
        | RoutingResourceKind.Conveyor -> Medhavi.Contracts.Domain.RoutingResourceKind.Conveyor
        | RoutingResourceKind.RailTrack -> Medhavi.Contracts.Domain.RoutingResourceKind.RailTrack
        | RoutingResourceKind.TruckFleet -> Medhavi.Contracts.Domain.RoutingResourceKind.TruckFleet
        | RoutingResourceKind.VesselClass -> Medhavi.Contracts.Domain.RoutingResourceKind.VesselClass

    let mapLoadBasis (b: ResourceLoadBasis) : Medhavi.Contracts.Domain.ResourceLoadBasis =
        match b with
        | ResourceLoadBasis.PerOrder -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerOrder
        | ResourceLoadBasis.PerUnit -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerUnit
        | ResourceLoadBasis.PerBatch -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerBatch
        | ResourceLoadBasis.PerTonne -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerTonne
        | ResourceLoadBasis.PerPallet -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerPallet
        | ResourceLoadBasis.PerContainer -> Medhavi.Contracts.Domain.ResourceLoadBasis.PerContainer

    let mapSelectionRule (r: ResourceSelectionRule) : Medhavi.Contracts.Domain.ResourceSelectionRule =
        match r with
        | ResourceSelectionRule.AnyAllowed -> Medhavi.Contracts.Domain.ResourceSelectionRule.AnyAllowed
        | ResourceSelectionRule.PreferPrimary -> Medhavi.Contracts.Domain.ResourceSelectionRule.PreferPrimary
        | ResourceSelectionRule.PreferLowestCost -> Medhavi.Contracts.Domain.ResourceSelectionRule.PreferLowestCost
        | ResourceSelectionRule.PreferFastest -> Medhavi.Contracts.Domain.ResourceSelectionRule.PreferFastest
        | ResourceSelectionRule.PreferPriorityOrder ->
            Medhavi.Contracts.Domain.ResourceSelectionRule.PreferPriorityOrder

    let mapResourceUsage (u: ResourceUsage) : Medhavi.Contracts.Domain.ResourceUsage =
        match u with
        | ResourceUsage.Primary -> Medhavi.Contracts.Domain.ResourceUsage.Primary
        | ResourceUsage.Alternate -> Medhavi.Contracts.Domain.ResourceUsage.Alternate
        | ResourceUsage.Optional -> Medhavi.Contracts.Domain.ResourceUsage.Optional
        | ResourceUsage.Parallel -> Medhavi.Contracts.Domain.ResourceUsage.Parallel
        | ResourceUsage.Rework -> Medhavi.Contracts.Domain.ResourceUsage.Rework

    let mapSetupPolicy (p: SetupPolicy) : Medhavi.Contracts.Domain.SetupPolicy =
        match p with
        | SetupPolicy.NoSetup -> Medhavi.Contracts.Domain.SetupPolicy.NoSetup
        | SetupPolicy.FixedSetup v -> Medhavi.Contracts.Domain.SetupPolicy.FixedSetup(DurationMinutes.value v)

    let mapCoolingPolicy (p: CoolingPolicy) : Medhavi.Contracts.Domain.CoolingPolicy =
        match p with
        | CoolingPolicy.NoCooling -> Medhavi.Contracts.Domain.CoolingPolicy.NoCooling
        | CoolingPolicy.FixedCooling v -> Medhavi.Contracts.Domain.CoolingPolicy.FixedCooling(DurationMinutes.value v)

    let mapResourceEfficiencyPolicy (p: ResourceEfficiencyPolicy) : Medhavi.Contracts.Domain.ResourceEfficiencyPolicy =
        match p with
        | ResourceEfficiencyPolicy.StandardEfficiency ->
            Medhavi.Contracts.Domain.ResourceEfficiencyPolicy.StandardEfficiency
        | ResourceEfficiencyPolicy.EfficiencyFactor v ->
            Medhavi.Contracts.Domain.ResourceEfficiencyPolicy.EfficiencyFactor(PositiveDecimal.value v)

    let mapResourceTimingProfile
        (setup: DurationMinutes option)
        (run: DurationMinutes option)
        (teardown: DurationMinutes option)
        (cooling: DurationMinutes option)
        (minLead: DurationMinutes option)
        : Medhavi.Contracts.Domain.ResourceTimingProfile =
        { SetupTime = setup |> Option.map DurationMinutes.value
          RunTimePerBaseQuantity = run |> Option.map DurationMinutes.value
          TeardownTime = teardown |> Option.map DurationMinutes.value
          CoolingTime = cooling |> Option.map DurationMinutes.value
          MinLeadTime = minLead |> Option.map DurationMinutes.value }

    let mapResourceOption (opt: TransportResourceOption) : Medhavi.Contracts.Domain.TransportResourceOption =
        { OptionId = RoutingResourceOptionId.value opt.OptionId
          ResourceGroupId =
            opt.ResourceGroupId
            |> Option.map ResourceGroupId.value
          CarrierId = opt.CarrierId |> Option.map CarrierId.value
          Usage = mapResourceUsage opt.Usage
          Priority = opt.Priority
          TransitTime = DurationMinutes.value opt.TransitTime
          LoadingTime =
            opt.LoadingTime
            |> Option.map DurationMinutes.value
          UnloadingTime =
            opt.UnloadingTime
            |> Option.map DurationMinutes.value
          CostPerUnit =
            opt.CostPerUnit
            |> Option.map PositiveDecimal.value
          CostPerTrip =
            opt.CostPerTrip
            |> Option.map PositiveDecimal.value
          EffectivePeriodStart =
            opt.EffectivePeriod
            |> Option.bind (fun p -> Some(Timestamp.value p.Start))
          EffectivePeriodEnd =
            opt.EffectivePeriod
            |> Option.bind (fun p -> p.End |> Option.map Timestamp.value) }

    let mapWorkResourceOption (opt: RoutingResourceOption) : Medhavi.Contracts.Domain.RoutingResourceOption =
        { OptionId = RoutingResourceOptionId.value opt.OptionId
          ResourceGroupId = ResourceGroupId.value opt.ResourceGroupId
          WorkCenterId = opt.WorkCenterId |> Option.map WorkCenterId.value
          Usage = mapResourceUsage opt.Usage
          Priority = opt.Priority
          TimingProfile =
            { SetupTime =
                opt.TimingProfile.SetupTime
                |> Option.map DurationMinutes.value
              RunTimePerBaseQuantity =
                opt.TimingProfile.RunTimePerBaseQuantity
                |> Option.map DurationMinutes.value
              TeardownTime =
                opt.TimingProfile.TeardownTime
                |> Option.map DurationMinutes.value
              CoolingTime =
                opt.TimingProfile.CoolingTime
                |> Option.map DurationMinutes.value
              MinLeadTime =
                opt.TimingProfile.MinLeadTime
                |> Option.map DurationMinutes.value }
          SetupPolicy = mapSetupPolicy opt.SetupPolicy
          CoolingPolicy = mapCoolingPolicy opt.CoolingPolicy
          CostPerMinute =
            opt.CostPerMinute
            |> Option.map PositiveDecimal.value
          EfficiencyPolicy = mapResourceEfficiencyPolicy opt.EfficiencyPolicy
          EffectivePeriodStart =
            opt.EffectivePeriod
            |> Option.bind (fun p -> Some(Timestamp.value p.Start))
          EffectivePeriodEnd =
            opt.EffectivePeriod
            |> Option.bind (fun p -> p.End |> Option.map Timestamp.value) }

    let mapResourceRequirement
        (req: RoutingStepResourceRequirement)
        : Medhavi.Contracts.Domain.RoutingStepResourceRequirement =
        { RequirementId = RoutingResourceRequirementId.value req.RequirementId
          ResourceKind = mapResourceKind req.ResourceKind
          LoadBasis = mapLoadBasis req.LoadBasis
          RequiredUnits = PositiveDecimal.value req.RequiredUnits
          SelectionRule = mapSelectionRule req.SelectionRule
          Options = req.Options |> List.map mapWorkResourceOption }

    let mapStepYieldPolicy (p: StepYieldPolicy) : Medhavi.Contracts.Domain.StepYieldPolicy =
        match p with
        | StepYieldPolicy.NoYieldLoss -> Medhavi.Contracts.Domain.StepYieldPolicy.NoYieldLoss
        | StepYieldPolicy.ExpectedYield v -> Medhavi.Contracts.Domain.StepYieldPolicy.ExpectedYield(Percent.value v)

    let mapReworkPolicy (p: ReworkPolicy) : Medhavi.Contracts.Domain.ReworkPolicy =
        match p with
        | ReworkPolicy.NoRework -> Medhavi.Contracts.Domain.ReworkPolicy.NoRework
        | ReworkPolicy.ReworkToStep(stepId, rate) ->
            Medhavi.Contracts.Domain.ReworkPolicy.ReworkToStep(RoutingStepId.value stepId, Percent.value rate)

    let mapStepTimingProfile (t: StepTimingProfile) : Medhavi.Contracts.Domain.StepTimingProfile =
        { FixedLeadTime =
            t.FixedLeadTime
            |> Option.map DurationMinutes.value
          QueueTime = t.QueueTime |> Option.map DurationMinutes.value
          WaitTime = t.WaitTime |> Option.map DurationMinutes.value
          MoveTime = t.MoveTime |> Option.map DurationMinutes.value }

    let mapStepOverlapPolicy (p: StepOverlapPolicy) : Medhavi.Contracts.Domain.StepOverlapPolicy =
        match p with
        | StepOverlapPolicy.NoOverlap -> Medhavi.Contracts.Domain.StepOverlapPolicy.NoOverlap
        | StepOverlapPolicy.OverlapAfterQuantity v ->
            Medhavi.Contracts.Domain.StepOverlapPolicy.OverlapAfterQuantity(Quantity.value v)
        | StepOverlapPolicy.OverlapAfterDuration v ->
            Medhavi.Contracts.Domain.StepOverlapPolicy.OverlapAfterDuration(DurationMinutes.value v)

    let mapStepKind (k: RoutingStepKind) : Medhavi.Contracts.Domain.RoutingStepKind =
        match k with
        | RoutingStepKind.Standard -> Medhavi.Contracts.Domain.RoutingStepKind.Standard
        | RoutingStepKind.Alternate -> Medhavi.Contracts.Domain.RoutingStepKind.Alternate
        | RoutingStepKind.Parallel -> Medhavi.Contracts.Domain.RoutingStepKind.Parallel
        | RoutingStepKind.Rework -> Medhavi.Contracts.Domain.RoutingStepKind.Rework
        | RoutingStepKind.External -> Medhavi.Contracts.Domain.RoutingStepKind.External

    let mapStep (s: RoutingStep) : Medhavi.Contracts.Domain.RoutingStep =
        { StepId = RoutingStepId.value s.StepId
          Sequence = s.Sequence
          OperationCode = s.OperationCode
          Name = s.Name
          Description = s.Description
          Kind = mapStepKind s.Kind
          Inputs = s.Inputs |> List.map mapStepInput
          Outputs = s.Outputs |> List.map mapStepOutput
          ResourceRequirements =
            s.ResourceRequirements
            |> List.map mapResourceRequirement
          TimingProfile = mapStepTimingProfile s.TimingProfile
          YieldPolicy = mapStepYieldPolicy s.YieldPolicy
          ReworkPolicy = mapReworkPolicy s.ReworkPolicy
          OverlapPolicy = mapStepOverlapPolicy s.OverlapPolicy
          EffectivePeriodStart =
            s.EffectivePeriod
            |> Option.bind (fun p -> Some(Timestamp.value p.Start))
          EffectivePeriodEnd =
            s.EffectivePeriod
            |> Option.bind (fun p -> p.End |> Option.map Timestamp.value) }

    let mapWorkDetails (work: WorkRoutingDetails) : Medhavi.Contracts.Domain.WorkRoutingDetails =
        { ProductId = SkuId.value work.ProductId
          PrimaryOutputSkuId = SkuId.value work.PrimaryOutputSkuId
          BaseOutputQuantity = Quantity.value work.BaseOutputQuantity
          Steps = work.Steps |> List.map mapStep }

    let mapTransportMode (m: TransportMode) : Medhavi.Contracts.Domain.TransportMode =
        match m with
        | TransportMode.Road -> Medhavi.Contracts.Domain.TransportMode.Road
        | TransportMode.Rail -> Medhavi.Contracts.Domain.TransportMode.Rail
        | TransportMode.Sea -> Medhavi.Contracts.Domain.TransportMode.Sea
        | TransportMode.Air -> Medhavi.Contracts.Domain.TransportMode.Air
        | TransportMode.Pipeline -> Medhavi.Contracts.Domain.TransportMode.Pipeline
        | TransportMode.Conveyor -> Medhavi.Contracts.Domain.TransportMode.Conveyor
        | TransportMode.InternalTransfer -> Medhavi.Contracts.Domain.TransportMode.InternalTransfer

    let mapTransportDetails (t: TransportRoutingDetails) : Medhavi.Contracts.Domain.TransportRoutingDetails =
        { SkuId = SkuId.value t.SkuId
          FromNodeId = NodeId.value t.FromNodeId
          ToNodeId = NodeId.value t.ToNodeId
          Mode = mapTransportMode t.Mode
          TransitLeadTime = DurationMinutes.value t.TransitLeadTime
          LossFactor = t.LossFactor |> Option.map Percent.value
          ResourceSelectionRule = mapSelectionRule t.ResourceSelectionRule
          TransportResourceOptions =
            t.TransportResourceOptions
            |> List.map mapResourceOption }

    let mapPurchasePricingPolicy (p: PurchasePricingPolicy) : Medhavi.Contracts.Domain.PurchasePricingPolicy =
        match p with
        | PurchasePricingPolicy.NoPurchaseCost -> Medhavi.Contracts.Domain.PurchasePricingPolicy.NoPurchaseCost
        | PurchasePricingPolicy.PurchaseCostPerUnit v ->
            Medhavi.Contracts.Domain.PurchasePricingPolicy.PurchaseCostPerUnit(PositiveDecimal.value v)
        | PurchasePricingPolicy.ContractPriceReference ref ->
            Medhavi.Contracts.Domain.PurchasePricingPolicy.ContractPriceReference ref

    let mapPurchaseDetails (p: PurchaseRoutingDetails) : Medhavi.Contracts.Domain.PurchaseRoutingDetails =
        { SkuId = SkuId.value p.SkuId
          SupplierId = SupplierId.value p.SupplierId
          ReceivingNodeId = NodeId.value p.ReceivingNodeId
          SupplierShipFromNodeId =
            p.SupplierShipFromNodeId
            |> Option.map NodeId.value
          SupplierLeadTime = DurationMinutes.value p.SupplierLeadTime
          InspectionLeadTime =
            p.InspectionLeadTime
            |> Option.map DurationMinutes.value
          PutawayLeadTime =
            p.PutawayLeadTime
            |> Option.map DurationMinutes.value
          SupplierSkuCode = p.SupplierSkuCode
          SupplierPreference =
            { Priority = p.SupplierPreference.Priority
              IsPreferred = p.SupplierPreference.IsPreferred }
          PurchasePricingPolicy = mapPurchasePricingPolicy p.PurchasePricingPolicy }

    let mapDetails (d: RoutingDetails) : Medhavi.Contracts.Domain.RoutingDetails =
        match d with
        | RoutingDetails.Work w -> Medhavi.Contracts.Domain.RoutingDetails.Work(mapWorkDetails w)
        | RoutingDetails.Transport t -> Medhavi.Contracts.Domain.RoutingDetails.Transport(mapTransportDetails t)
        | RoutingDetails.Purchase p -> Medhavi.Contracts.Domain.RoutingDetails.Purchase(mapPurchaseDetails p)

open Mappers

let mapRoutingDto (r: Routing) : Medhavi.Contracts.Domain.Routing =
    { Id = RoutingId.value r.Id
      Name = r.Name
      Description = r.Description
      Applicability = mapApplicability r.Applicability
      Preference = mapPreference r.Preference
      QuantityRule = mapQuantityRule r.QuantityRule
      CostPolicy = mapCostPolicy r.CostPolicy
      Details = mapDetails r.Details
      Status = r.Status.ToBool()
      CreatedAt = Timestamp.value r.CreatedAt
      ModifiedAt = Timestamp.value r.ModifiedAt }

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
