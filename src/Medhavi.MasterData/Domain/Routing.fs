module Medhavi.MasterData.Domain.RoutingAgg

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations

type RoutingPreference = { Priority: int; IsPreferred: bool }

type RoutingQuantityRule =
    { MinQuantity: PositiveDecimal option
      MaxQuantity: PositiveDecimal option
      LotSize: PositiveDecimal option
      OrderMultiple: PositiveDecimal option }

type RoutingCostPolicy =
    | NoRoutingCost
    | FixedCost of PositiveDecimal
    | CostPerUnit of PositiveDecimal

type RoutingApplicability =
    { StockingPointId: StockingPointId option
      EffectivePeriod: DateRange }

// Work routing

type RoutingStepKind =
    | Standard
    | Alternate
    | Parallel
    | Rework
    | External

type StepOverlapPolicy =
    | NoOverlap
    | OverlapAfterQuantity of Quantity
    | OverlapAfterDuration of DurationMinutes

type StepInputTiming =
    | AtStepStart
    | AtStepEnd
    | OffsetBeforeStepStart of DurationMinutes
    | OffsetAfterStepStart of DurationMinutes
    | OffsetBeforeStepEnd of DurationMinutes
    | OffsetAfterStepEnd of DurationMinutes

type StepOutputTiming =
    | AtStepStart
    | AtStepEnd
    | OffsetAfterStepStart of DurationMinutes
    | OffsetAfterStepEnd of DurationMinutes

type RoutingOutputRole =
    | PrimaryOutput
    | CoProduct
    | ByProduct
    | Scrap
    | Waste

type RoutingStepInput =
    { SkuId: SkuId
      FromNodeId: NodeId option
      QuantityPerBaseOutput: PositiveDecimal option
      Timing: StepInputTiming
      IsConsumed: bool
      IsOptional: bool }

type RoutingStepOutput =
    { SkuId: SkuId
      ToNodeId: NodeId option
      QuantityRatioToPrimaryOutput: PositiveDecimal option
      Role: RoutingOutputRole
      Timing: StepOutputTiming }

// Work routing: Resource Requirements

type RoutingResourceKind =
    | Machine
    | WorkCenter
    | LaborPool
    | Tool
    | Utility
    | Berth
    | Conveyor
    | RailTrack
    | TruckFleet
    | VesselClass

type ResourceLoadBasis =
    | PerOrder
    | PerUnit
    | PerBatch
    | PerTonne
    | PerPallet
    | PerContainer

type ResourceUsage =
    | Primary
    | Alternate
    | Optional
    | Parallel
    | Rework

type ResourceSelectionRule =
    | AnyAllowed
    | PreferPrimary
    | PreferLowestCost
    | PreferFastest
    | PreferPriorityOrder

type SetupPolicy =
    | NoSetup
    | FixedSetup of DurationMinutes

type CoolingPolicy =
    | NoCooling
    | FixedCooling of DurationMinutes

type ResourceEfficiencyPolicy =
    | StandardEfficiency
    | EfficiencyFactor of PositiveDecimal

type ResourceTimingProfile =
    { SetupTime: DurationMinutes option
      RunTimePerBaseQuantity: DurationMinutes option
      TeardownTime: DurationMinutes option
      CoolingTime: DurationMinutes option
      MinLeadTime: DurationMinutes option }

type RoutingResourceOptionId = RoutingResourceOptionId of string

module RoutingResourceOptionId =
    let value (RoutingResourceOptionId v) = v

type RoutingResourceRequirementId = RoutingResourceRequirementId of string

module RoutingResourceRequirementId =
    let value (RoutingResourceRequirementId v) = v

type RoutingResourceOption =
    { OptionId: RoutingResourceOptionId

      ResourceGroupId: ResourceGroupId
      WorkCenterId: WorkCenterId option

      Usage: ResourceUsage
      Priority: int option

      TimingProfile: ResourceTimingProfile

      SetupPolicy: SetupPolicy
      CoolingPolicy: CoolingPolicy

      CostPerMinute: PositiveDecimal option
      EfficiencyPolicy: ResourceEfficiencyPolicy

      EffectivePeriod: DateRange option }

type RoutingStepResourceRequirement =
    { RequirementId: RoutingResourceRequirementId

      ResourceKind: RoutingResourceKind
      LoadBasis: ResourceLoadBasis

      RequiredUnits: PositiveDecimal

      SelectionRule: ResourceSelectionRule

      Options: RoutingResourceOption list }

// ======================================================
// Work Routing: Quality / Rework / Timing
// ======================================================

type StepYieldPolicy =
    | NoYieldLoss
    | ExpectedYield of Percent

type ReworkPolicy =
    | NoRework
    | ReworkToStep of RoutingStepId * reworkRate: Percent

type StepTimingProfile =
    { FixedLeadTime: DurationMinutes option
      QueueTime: DurationMinutes option
      WaitTime: DurationMinutes option
      MoveTime: DurationMinutes option }

type RoutingStep =
    { StepId: RoutingStepId
      Sequence: int

      OperationCode: string

      Name: string
      Description: string option

      Kind: RoutingStepKind

      Inputs: RoutingStepInput list
      Outputs: RoutingStepOutput list

      ResourceRequirements: RoutingStepResourceRequirement list

      TimingProfile: StepTimingProfile

      YieldPolicy: StepYieldPolicy
      ReworkPolicy: ReworkPolicy
      OverlapPolicy: StepOverlapPolicy

      EffectivePeriod: DateRange option }

type WorkRoutingDetails =
    { ProductId: SkuId

      PrimaryOutputSkuId: SkuId
      BaseOutputQuantity: Quantity

      Steps: RoutingStep list }

// Transport Routing

type TransportMode =
    | Road
    | Rail
    | Sea
    | Air
    | Pipeline
    | Conveyor
    | InternalTransfer

type CarrierId = CarrierId of string

module CarrierId =
    let value (CarrierId v) = v

type TransportResourceOption =
    { OptionId: RoutingResourceOptionId

      ResourceGroupId: ResourceGroupId option
      CarrierId: CarrierId option

      Usage: ResourceUsage
      Priority: int option

      TransitTime: DurationMinutes
      LoadingTime: DurationMinutes option
      UnloadingTime: DurationMinutes option

      CostPerUnit: PositiveDecimal option
      CostPerTrip: PositiveDecimal option

      EffectivePeriod: DateRange option }

type TransportRoutingDetails =
    { SkuId: SkuId

      FromNodeId: NodeId
      ToNodeId: NodeId

      Mode: TransportMode

      TransitLeadTime: DurationMinutes

      LossFactor: Percent option

      ResourceSelectionRule: ResourceSelectionRule
      TransportResourceOptions: TransportResourceOption list }

// Purchase Routing

type SupplierPreference = { Priority: int; IsPreferred: bool }

type PurchasePricingPolicy =
    | NoPurchaseCost
    | PurchaseCostPerUnit of PositiveDecimal
    | ContractPriceReference of contractId: string

type PurchaseRoutingDetails =
    { SkuId: SkuId

      SupplierId: SupplierId

      ReceivingNodeId: NodeId
      SupplierShipFromNodeId: NodeId option

      SupplierLeadTime: DurationMinutes
      InspectionLeadTime: DurationMinutes option
      PutawayLeadTime: DurationMinutes option

      SupplierSkuCode: string option

      SupplierPreference: SupplierPreference

      PurchasePricingPolicy: PurchasePricingPolicy }

type RoutingDetails =
    | Work of WorkRoutingDetails
    | Transport of TransportRoutingDetails
    | Purchase of PurchaseRoutingDetails

type RoutingStatus =
    | Active
    | Inactive

    member this.ToBool() =
        match this with
        | Active -> true
        | Inactive -> false

type Routing =
    { Id: RoutingId

      Name: string
      Description: string option

      Applicability: RoutingApplicability

      Preference: RoutingPreference

      QuantityRule: RoutingQuantityRule

      CostPolicy: RoutingCostPolicy

      Details: RoutingDetails

      Revision: Revision

      Status: RoutingStatus

      CreatedAt: Timestamp
      ModifiedAt: Timestamp }

module Commands =
    type DefineRoutingApplicability =
        { StockingPointId: string option
          EffectiveStart: Timestamp
          EffectiveEnd: Timestamp option }

    type DefineRoutingQuantityRule =
        { MinQuantity: decimal option
          MaxQuantity: decimal option
          LotSize: decimal option
          OrderMultiple: decimal option }

    type DefineRoutingCostPolicy =
        | NoRoutingCost
        | FixedCost of decimal
        | CostPerUnit of decimal

    type DefineRoutingStepInput =
        { SkuId: string
          FromNodeId: string option
          QuantityPerBaseOutput: decimal option
          Timing: StepInputTiming
          IsConsumed: bool
          IsOptional: bool }

    type DefineRoutingStepOutput =
        { SkuId: string
          ToNodeId: string option
          QuantityRatioToPrimaryOutput: decimal option
          Role: RoutingOutputRole
          Timing: StepOutputTiming }

    type DefineRoutingResourceOption =
        { OptionId: string
          ResourceGroupId: string
          ResourceId: string option
          WorkCenterId: string option
          Usage: ResourceUsage
          Priority: int option
          SetupTime: decimal option
          RunTimePerBaseQuantity: decimal option
          TeardownTime: decimal option
          CoolingTime: decimal option
          MinLeadTime: decimal option
          CostPerMinute: decimal option
          EfficiencyPolicy: decimal option
          SetupPolicy: SetupPolicy
          CoolingPolicy: CoolingPolicy
          EffectiveStart: Timestamp option
          EffectiveEnd: Timestamp option }

    type DefineRoutingStepResourceRequirement =
        { RequirementId: string
          ResourceKind: RoutingResourceKind
          LoadBasis: ResourceLoadBasis
          RequiredUnits: decimal
          SelectionRule: ResourceSelectionRule
          Options: DefineRoutingResourceOption list }

    type DefineStepTimingProfile =
        { FixedLeadTime: decimal option
          QueueTime: decimal option
          WaitTime: decimal option
          MoveTime: decimal option }

    type DefineRoutingStep =
        { StepId: string
          Sequence: int
          OperationCode: string
          Name: string
          Description: string option
          Kind: RoutingStepKind
          Inputs: DefineRoutingStepInput list
          Outputs: DefineRoutingStepOutput list
          ResourceRequirements: DefineRoutingStepResourceRequirement list
          TimingProfile: DefineStepTimingProfile
          YieldPolicy: StepYieldPolicy
          ReworkPolicy: ReworkPolicy
          OverlapPolicy: StepOverlapPolicy
          EffectiveStart: Timestamp option
          EffectiveEnd: Timestamp option }

    type DefineWorkRoutingDetails =
        { ProductId: string
          PrimaryOutputSkuId: string
          BaseOutputQuantity: decimal
          Steps: DefineRoutingStep list }

    type DefineTransportResourceOption =
        { OptionId: string
          ResourceGroupId: string option
          ResourceId: string option
          CarrierId: string option
          Usage: ResourceUsage
          Priority: int option
          TransitTime: decimal
          LoadingTime: decimal option
          UnloadingTime: decimal option
          CostPerUnit: decimal option
          CostPerTrip: decimal option
          EffectiveStart: Timestamp option
          EffectiveEnd: Timestamp option }

    type DefineTransportRoutingDetails =
        { SkuId: string
          FromNodeId: string
          ToNodeId: string
          Mode: TransportMode
          TransitLeadTime: decimal
          LossFactor: decimal option
          ResourceSelectionRule: ResourceSelectionRule
          TransportResourceOptions: DefineTransportResourceOption list }

    type DefinePurchasePricingPolicy =
        | NoPurchaseCost
        | PurchaseCostPerUnit of decimal
        | ContractPriceReference of contractId: string

    type DefinePurchaseRoutingDetails =
        { SkuId: string
          SupplierId: string
          ReceivingNodeId: string
          SupplierShipFromNodeId: string option
          SupplierLeadTime: decimal
          InspectionLeadTime: decimal option
          PutawayLeadTime: decimal option
          SupplierSkuCode: string option
          SupplierPreference: SupplierPreference
          PurchasePricingPolicy: DefinePurchasePricingPolicy }

    type DefineRoutingDetails =
        | Work of DefineWorkRoutingDetails
        | Transport of DefineTransportRoutingDetails
        | Purchase of DefinePurchaseRoutingDetails

    type DefineRoutingCmd =
        { Id: RoutingId
          Name: string
          Description: string option
          Applicability: DefineRoutingApplicability
          Priority: int
          IsPreffered: bool
          QuantityRule: DefineRoutingQuantityRule
          CostPolicy: DefineRoutingCostPolicy
          Details: DefineRoutingDetails
          CreatedAt: Timestamp
          ModifiedAt: Timestamp }

    type RoutingCommand =
        | DefineRouting of DefineRoutingCmd
        | ActivateRouting of RoutingId
        | DeactivateRouting of RoutingId

type RoutingEvent =
    | RoutingDefined of Routing
    | RoutingActivated of RoutingId * Timestamp
    | RoutingDeactivated of RoutingId * Timestamp

type DecideRouting = Decide<Routing, Commands.RoutingCommand, RoutingEvent>
type EvolveRouting = Evolve<Routing, RoutingEvent>

module private RoutingValidationHelpers =
    open Commands

    let requiredText fieldName value = required fieldName value

    let optionalStringId create idOpt =
        match idOpt with
        | None -> Valid None
        | Some value -> create value |> fromResult |> map Some

    let validatePositive fieldName value = PositiveDecimal.create value |> fromResult

    let validatePercentage fieldName value = Percent.create value |> fromResult

    let validateDuration fieldName value =
        DurationMinutes.create value
        |> Result.mapError (fun msg -> DomainError.validation $"{fieldName}: {msg}")
        |> fromResult

    let validatePositiveOpt fieldName valueOpt =
        match valueOpt with
        | None -> Valid None
        | Some value -> validatePositive fieldName value |> map Some

    let validatePercentageOpt fieldName valueOpt =
        match valueOpt with
        | None -> Valid None
        | Some value -> validatePercentage fieldName value |> map Some

    let validateDurationOpt fieldName valueOpt =
        match valueOpt with
        | None -> Valid None
        | Some value -> validateDuration fieldName value |> map Some

    let validateDateRange (startDate: Timestamp) (endDate: Timestamp option) =
        match endDate with
        | Some e when Timestamp.value e < Timestamp.value startDate ->
            Invalid [ DomainError.validation "Effective end cannot be before effective start" ]
        | _ -> Valid { Start = startDate; End = endDate }

    let validateOptionalDateRange startDateOpt endDateOpt =
        match startDateOpt, endDateOpt with
        | None, None -> Valid None
        | Some startDate, endDate -> validateDateRange startDate endDate |> map Some
        | None, Some _ ->
            Invalid [ DomainError.validation "Effective start is required when effective end is provided" ]

    let validateQuantity fieldName (quantity: decimal) = Quantity.create quantity |> fromResult

    let validateQuantityRule (rule: DefineRoutingQuantityRule) =
        let makeRule minQty maxQty lotSize orderMultiple =
            { MinQuantity = minQty
              MaxQuantity = maxQty
              LotSize = lotSize
              OrderMultiple = orderMultiple }
            : RoutingQuantityRule

        let minMaxValidation =
            match rule.MinQuantity, rule.MaxQuantity with
            | Some minQ, Some maxQ when minQ > maxQ ->
                Invalid [ DomainError.validation "Minimum quantity cannot be greater than maximum quantity" ]
            | _ -> Valid()

        makeRule
        <!> validatePositiveOpt "Minimum quantity" rule.MinQuantity
        <*> validatePositiveOpt "Maximum quantity" rule.MaxQuantity
        <*> validatePositiveOpt "Lot size" rule.LotSize
        <*> validatePositiveOpt "Order multiple" rule.OrderMultiple
        <* minMaxValidation

    let validatePreference (priority, isPreferred) : Validation<RoutingPreference, DomainError> =
        Valid
            { Priority = priority
              IsPreferred = isPreferred }

    let validateCostPolicy (policy: DefineRoutingCostPolicy) =
        match policy with
        | DefineRoutingCostPolicy.NoRoutingCost -> Valid RoutingCostPolicy.NoRoutingCost
        | DefineRoutingCostPolicy.FixedCost v ->
            PositiveDecimal.create v
            |> Result.map RoutingCostPolicy.FixedCost
            |> fromResult
        | DefineRoutingCostPolicy.CostPerUnit v ->
            PositiveDecimal.create v
            |> Result.map RoutingCostPolicy.CostPerUnit
            |> fromResult

    let validateStepInput (input: DefineRoutingStepInput) =
        let makeInput skuId fromNode qty =
            { SkuId = skuId
              FromNodeId = fromNode
              QuantityPerBaseOutput = qty
              Timing = input.Timing
              IsConsumed = input.IsConsumed
              IsOptional = input.IsOptional }
            : RoutingStepInput

        makeInput
        <!> (SkuId.create input.SkuId |> fromResult)
        <*> optionalStringId NodeId.create input.FromNodeId
        <*> validatePositiveOpt "Quantity per base output" input.QuantityPerBaseOutput

    let validateStepOutput (output: DefineRoutingStepOutput) =
        let makeOutput skuId toNode ratio =
            { SkuId = skuId
              ToNodeId = toNode
              QuantityRatioToPrimaryOutput = ratio
              Role = output.Role
              Timing = output.Timing }
            : RoutingStepOutput

        makeOutput
        <!> (SkuId.create output.SkuId |> fromResult)
        <*> optionalStringId NodeId.create output.ToNodeId
        <*> validatePositiveOpt "Quantity ratio to primary output" output.QuantityRatioToPrimaryOutput

    let validateResourceOption (opt: DefineRoutingResourceOption) =
        let makeOption optId rgId wcId priority setup run teardown cooling minLead cost eff period =
            { OptionId = RoutingResourceOptionId optId
              ResourceGroupId = rgId
              WorkCenterId = wcId
              Usage = opt.Usage
              Priority = priority
              TimingProfile =
                { SetupTime = setup
                  RunTimePerBaseQuantity = run
                  TeardownTime = teardown
                  CoolingTime = cooling
                  MinLeadTime = minLead }
              SetupPolicy = opt.SetupPolicy
              CoolingPolicy = opt.CoolingPolicy
              CostPerMinute = cost
              EfficiencyPolicy = eff
              EffectivePeriod = period }

        makeOption opt.OptionId
        <!> (ResourceGroupId.create opt.ResourceGroupId
             |> fromResult)
        <*> (match opt.ResourceId |> Option.orElse opt.WorkCenterId with
             | None -> Valid None
             | Some wc -> WorkCenterId.create wc |> fromResult |> Medhavi.Common.Validation.map Some)
        <*> Valid opt.Priority
        <*> validateDurationOpt "Setup time" opt.SetupTime
        <*> validateDurationOpt "Run time per base quantity" opt.RunTimePerBaseQuantity
        <*> validateDurationOpt "Teardown time" opt.TeardownTime
        <*> validateDurationOpt "Cooling time" opt.CoolingTime
        <*> validateDurationOpt "Minimum lead time" opt.MinLeadTime
        <*> validatePositiveOpt "Cost per minute" opt.CostPerMinute
        <*> (match opt.EfficiencyPolicy with
             | None -> Valid StandardEfficiency
             | Some factor ->
                 PositiveDecimal.create factor
                 |> Result.map EfficiencyFactor
                 |> fromResult)
        <*> validateOptionalDateRange opt.EffectiveStart opt.EffectiveEnd

    let validateResourceRequirement (req: DefineRoutingStepResourceRequirement) =
        let makeRequirement reqId units options =
            { RequirementId = RoutingResourceRequirementId reqId
              ResourceKind = req.ResourceKind
              LoadBasis = req.LoadBasis
              RequiredUnits = units
              SelectionRule = req.SelectionRule
              Options = options }
            : RoutingStepResourceRequirement

        makeRequirement req.RequirementId
        <!> validatePositive "Required units" req.RequiredUnits
        <*> traverse validateResourceOption req.Options

    let validateStepTimingProfile (profile: DefineStepTimingProfile) =
        let makeProfile fixedLead queue wait move =
            { FixedLeadTime = fixedLead
              QueueTime = queue
              WaitTime = wait
              MoveTime = move }
            : StepTimingProfile

        makeProfile
        <!> validateDurationOpt "Fixed lead time" profile.FixedLeadTime
        <*> validateDurationOpt "Queue time" profile.QueueTime
        <*> validateDurationOpt "Wait time" profile.WaitTime
        <*> validateDurationOpt "Move time" profile.MoveTime

    let validateStep (step: DefineRoutingStep) =
        let makeStep stepId name desc inputs outputs reqs timing period =
            { StepId = stepId
              Sequence = step.Sequence
              OperationCode = step.OperationCode
              Name = name
              Description = desc
              Kind = step.Kind
              Inputs = inputs
              Outputs = outputs
              ResourceRequirements = reqs
              TimingProfile = timing
              YieldPolicy = step.YieldPolicy
              ReworkPolicy = step.ReworkPolicy
              OverlapPolicy = step.OverlapPolicy
              EffectivePeriod = period }

        makeStep
        <!> (RoutingStepId.create step.StepId |> fromResult)
        <*> requiredText "Step name" step.Name
        <*> Valid step.Description
        <*> traverse validateStepInput step.Inputs
        <*> traverse validateStepOutput step.Outputs
        <*> traverse validateResourceRequirement step.ResourceRequirements
        <*> validateStepTimingProfile step.TimingProfile
        <*> validateOptionalDateRange step.EffectiveStart step.EffectiveEnd

    let validateWorkDetails (work: DefineWorkRoutingDetails) =
        let validateStepsList (steps: DefineRoutingStep list) =
            if List.isEmpty steps then
                Invalid [ DomainError.validation "Work routing must contain at least one step" ]
            else
                let stepIds = steps |> List.map (fun s -> s.StepId)

                let duplicateIds =
                    stepIds
                    |> List.groupBy id
                    |> List.exists (fun (_, g) -> g.Length > 1)

                let sequences = steps |> List.map (fun s -> s.Sequence)

                let duplicateSeqs =
                    sequences
                    |> List.groupBy id
                    |> List.exists (fun (_, g) -> g.Length > 1)

                let nonPositiveSeqs = sequences |> List.exists (fun s -> s <= 0)

                let invalidReworkRefs =
                    steps
                    |> List.choose (fun s ->
                        match s.ReworkPolicy with
                        | ReworkToStep(refId, rate) -> Some(s.StepId, RoutingStepId.value refId, rate)
                        | NoRework -> None)
                    |> List.filter (fun (_, refId, _) -> not (List.contains refId stepIds))

                let idVal =
                    if duplicateIds then
                        Invalid [ DomainError.validation "Step IDs must be unique" ]
                    else
                        Valid()

                let seqDupVal =
                    if duplicateSeqs then
                        Invalid [ DomainError.validation "Step sequence numbers must be unique" ]
                    else
                        Valid()

                let seqPosVal =
                    if nonPositiveSeqs then
                        Invalid [ DomainError.validation "Step sequence numbers must be positive" ]
                    else
                        Valid()

                let reworkVal =
                    if not (List.isEmpty invalidReworkRefs) then
                        let errors =
                            invalidReworkRefs
                            |> List.map (fun (stepId, refId, _) ->
                                DomainError.validation
                                    $"Step '{stepId}' has an invalid ReworkPolicy referencing non-existent step '{refId}'")

                        Invalid errors
                    else
                        Valid()

                traverse validateStep steps
                <* idVal
                <* seqDupVal
                <* seqPosVal
                <* reworkVal

        let makeDetails pid primaryQty baseQty steps : WorkRoutingDetails =
            { ProductId = pid
              PrimaryOutputSkuId = primaryQty
              BaseOutputQuantity = baseQty
              Steps = steps }

        makeDetails
        <!> (SkuId.create work.ProductId |> fromResult)
        <*> (SkuId.create work.PrimaryOutputSkuId |> fromResult)
        <*> validateQuantity "Base output" work.BaseOutputQuantity
        <*> validateStepsList work.Steps

    let validateTransportResourceOption (opt: DefineTransportResourceOption) =
        let makeOption optId rgId carrierId priority transit loading unloading costUnit costTrip period =
            { OptionId = RoutingResourceOptionId optId
              ResourceGroupId = rgId
              CarrierId = carrierId
              Usage = opt.Usage
              Priority = priority
              TransitTime = transit
              LoadingTime = loading
              UnloadingTime = unloading
              CostPerUnit = costUnit
              CostPerTrip = costTrip
              EffectivePeriod = period }

        makeOption opt.OptionId
        <!> optionalStringId ResourceGroupId.create opt.ResourceGroupId
        <*> (match opt.CarrierId with
             | None -> Valid None
             | Some c -> Valid(Some(CarrierId c)))
        <*> Valid opt.Priority
        <*> validateDuration "Transit time" opt.TransitTime
        <*> validateDurationOpt "Loading time" opt.LoadingTime
        <*> validateDurationOpt "Unloading time" opt.UnloadingTime
        <*> validatePositiveOpt "Cost per unit" opt.CostPerUnit
        <*> validatePositiveOpt "Cost per trip" opt.CostPerTrip
        <*> validateOptionalDateRange opt.EffectiveStart opt.EffectiveEnd

    let validateTransportDetails (trans: DefineTransportRoutingDetails) =
        let makeDetails skuId fromNode toNode transit loss options : TransportRoutingDetails =
            { SkuId = skuId
              FromNodeId = fromNode
              ToNodeId = toNode
              Mode = trans.Mode
              TransitLeadTime = transit
              LossFactor = loss
              ResourceSelectionRule = trans.ResourceSelectionRule
              TransportResourceOptions = options }

        makeDetails
        <!> (SkuId.create trans.SkuId |> fromResult)
        <*> (NodeId.create trans.FromNodeId |> fromResult)
        <*> (NodeId.create trans.ToNodeId |> fromResult)
        <*> validateDuration "Transit lead time" trans.TransitLeadTime
        <*> validatePercentageOpt "Loss factor" trans.LossFactor
        <*> traverse validateTransportResourceOption trans.TransportResourceOptions

    let validatePurchasePricingPolicy (policy: DefinePurchasePricingPolicy) =
        match policy with
        | DefinePurchasePricingPolicy.NoPurchaseCost -> Valid PurchasePricingPolicy.NoPurchaseCost
        | DefinePurchasePricingPolicy.PurchaseCostPerUnit v ->
            PositiveDecimal.create v
            |> Result.map PurchasePricingPolicy.PurchaseCostPerUnit
            |> fromResult
        | DefinePurchasePricingPolicy.ContractPriceReference ref ->
            Valid(PurchasePricingPolicy.ContractPriceReference ref)

    let validatePurchaseDetails (pur: DefinePurchaseRoutingDetails) =
        let makeDetails
            skuId
            supplierId
            receivingNode
            shipFrom
            supplierLead
            inspection
            putaway
            skuCode
            policy
            : PurchaseRoutingDetails =
            { SkuId = skuId
              SupplierId = supplierId
              ReceivingNodeId = receivingNode
              SupplierShipFromNodeId = shipFrom
              SupplierLeadTime = supplierLead
              InspectionLeadTime = inspection
              PutawayLeadTime = putaway
              SupplierSkuCode = skuCode
              SupplierPreference = pur.SupplierPreference
              PurchasePricingPolicy = policy }

        makeDetails
        <!> (SkuId.create pur.SkuId |> fromResult)
        <*> (SupplierId.create pur.SupplierId |> fromResult)
        <*> (NodeId.create pur.ReceivingNodeId |> fromResult)
        <*> optionalStringId NodeId.create pur.SupplierShipFromNodeId
        <*> validateDuration "Supplier lead time" pur.SupplierLeadTime
        <*> validateDurationOpt "Inspection lead time" pur.InspectionLeadTime
        <*> validateDurationOpt "Putaway lead time" pur.PutawayLeadTime
        <*> Valid pur.SupplierSkuCode
        <*> validatePurchasePricingPolicy pur.PurchasePricingPolicy

    let validateRoutingDetails (details: DefineRoutingDetails) =
        match details with
        | DefineRoutingDetails.Work work ->
            validateWorkDetails work
            |> map RoutingDetails.Work
        | DefineRoutingDetails.Transport trans ->
            validateTransportDetails trans
            |> map RoutingDetails.Transport
        | DefineRoutingDetails.Purchase pur ->
            validatePurchaseDetails pur
            |> map RoutingDetails.Purchase

open RoutingValidationHelpers
open Commands

let private validateAndCreateRouting (routing: DefineRoutingCmd) =

    let combine id name effectiveDate stockingPoint pref qtyRule cost details status =
        { Id = id
          Name = name
          Description = routing.Description
          Applicability =
            { EffectivePeriod = effectiveDate
              StockingPointId = stockingPoint }
          Preference = pref
          QuantityRule = qtyRule
          CostPolicy = cost
          Details = details
          Revision = Revision.initial
          Status = status
          CreatedAt = routing.CreatedAt
          ModifiedAt = routing.ModifiedAt }

    let validateAndCreateStockingPoint spOpt =
        match spOpt with
        | None -> Valid None
        | Some sp ->
            StockingPointId.create sp
            |> Result.map Some
            |> fromResult

    combine routing.Id
    <!> required "Routing name" routing.Name
    <*> validateDateRange routing.Applicability.EffectiveStart routing.Applicability.EffectiveEnd
    <*> validateAndCreateStockingPoint routing.Applicability.StockingPointId
    <*> validatePreference (routing.Priority, routing.IsPreffered)
    <*> validateQuantityRule routing.QuantityRule
    <*> validateCostPolicy routing.CostPolicy
    <*> validateRoutingDetails routing.Details
    <*> Valid RoutingStatus.Inactive

let decide: DecideRouting =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineRouting cmd, None ->
            validateAndCreateRouting cmd
            |> toResult
            |> Result.mapError DomainError.combineValidationErrors
            |> Result.map (fun routing ->
                { NewState = routing
                  Events = [ RoutingDefined routing ] })
        | DefineRouting _, Some _ -> Error(DomainError.invariant "Routing already exists")

        | ActivateRouting(id), Some state when state.Id = id ->
            match state.Status with
            | RoutingStatus.Active -> Error(DomainError.invariant "Routing is already active")
            | RoutingStatus.Inactive ->
                let updated =
                    { state with
                        Status = RoutingStatus.Active
                        ModifiedAt = Timestamp.now }

                { NewState = updated
                  Events = [ RoutingActivated(id, updated.ModifiedAt) ] }
                |> Ok
        | ActivateRouting _, Some _ -> Error(DomainError.validation "Routing not found")

        | DeactivateRouting(id), Some state when state.Id = id ->
            match state.Status with
            | RoutingStatus.Inactive -> Error(DomainError.invariant "Routing is already inactive")
            | RoutingStatus.Active ->
                let updated =
                    { state with
                        Status = RoutingStatus.Inactive
                        ModifiedAt = Timestamp.now }

                { NewState = updated
                  Events = [ RoutingDeactivated(id, updated.ModifiedAt) ] }
                |> Ok
        | DeactivateRouting _, Some _ -> Error(DomainError.validation "Routing not found")

        | _, None -> Error(DomainError.validation "Routing not found")

let evolve: EvolveRouting =
    fun event stateOpt ->
        match event, stateOpt with
        | RoutingDefined routing, None -> Some routing
        | RoutingActivated(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = RoutingStatus.Active
                    ModifiedAt = modifiedAt }
        | RoutingDeactivated(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = RoutingStatus.Inactive
                    ModifiedAt = modifiedAt }
        | RoutingDefined _, Some state -> Some state
        | _, current -> current

let isEffective (asOf: Timestamp) (routing: Routing) =
    DateRange.contains asOf routing.Applicability.EffectivePeriod
    && routing.Status = RoutingStatus.Active
