module Medhavi.MasterData.Domain.RoutingAgg

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Validations

type RoutingType =
    | Work
    | Transport
    | Purchase

type RoutingStep =
    { StepId: string
      Sequence: int
      ResourceGroupId: ResourceGroupId option
      Yield: decimal option }

type RoutingInput =
    { StepId: string
      SkuId: SkuId
      NodeId: NodeId
      ConversionRate: decimal option }

type RoutingOutput =
    { StepId: string
      SkuId: SkuId
      NodeId: NodeId
      ConversionRate: decimal option
      IsCoSku: bool }

type StepResourceMap =
    { StepId: string
      ResourceId: ResourceId
      IsAllowed: bool
      Sequence: int
      DurationPerUnitMinutes: decimal option }

type Routing =
    { Id: RoutingId
      Name: string
      Type: RoutingType
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Steps: RoutingStep list
      Inputs: RoutingInput list
      Outputs: RoutingOutput list
      StepResources: StepResourceMap list
      CreatedAt: Timestamp
      ModifiedAt: Timestamp
      Status: Status }

type DefineRoutingCmd =
    { Id: RoutingId
      Name: string
      Type: RoutingType
      EffectiveStart: Timestamp
      EffectiveEnd: Timestamp option
      Steps: DefineRoutingStep list
      Inputs: DefineRoutingInput list
      Outputs: DefineRoutingOutput list
      StepResources: DefineStepResourceMap list
      CreatedAt: Timestamp
      ModifiedAt: Timestamp
      Status: Status }

and DefineRoutingStep =
    { StepId: string
      Sequence: int
      ResourceGroupId: string option
      Yield: decimal option }

and DefineRoutingInput =
    { StepId: string
      SkuId: string
      NodeId: string
      ConversionRate: decimal option }

and DefineRoutingOutput =
    { StepId: string
      SkuId: string
      NodeId: string
      ConversionRate: decimal option
      IsCoSku: bool }

and DefineStepResourceMap =
    { StepId: string
      ResourceId: string
      IsAllowed: bool
      Sequence: int32
      DurationPerUnitMinutes: decimal option }

type RoutingCommand =
    | DefineRouting of DefineRoutingCmd
    | ActivateRouting of RoutingId
    | DeactivateRouting of RoutingId

type RoutingEvent =
    | RoutingDefined of Routing
    | RoutingActivated of RoutingId * Timestamp
    | RoutingDeactivated of RoutingId * Timestamp

type DecideRouting = Decide<Routing, RoutingCommand, RoutingEvent>
type EvolveRouting = Evolve<Routing, RoutingEvent>

let createRoutingStep id nr rgId yieldnr =
    { StepId = id
      Sequence = nr
      ResourceGroupId = rgId
      Yield = yieldnr }
    : RoutingStep

let private validateAndTranslateSteps (steps: DefineRoutingStep list) =

    let makeStep (step: DefineRoutingStep) rgid = createRoutingStep step.StepId step.Sequence rgid step.Yield

    let notEmpty =
        if List.isEmpty steps then
            Invalid [ DomainError.validation "Routing must contain at least one step" ]
        else
            Valid steps

    let uniqueIds =
        let duplicated =
            steps
            |> List.groupBy (fun step -> step.StepId)
            |> List.exists (fun (_, grouped) -> List.length grouped > 1)

        if duplicated then
            Invalid [ DomainError.validation "Routing step ids must be unique" ]
        else
            Valid steps

    let validateStep step =
        match step.ResourceGroupId with
        | Some rg -> ResourceGroupId.create rg |> Result.map (Some)
        | None -> Ok None
        |> Result.map (fun rgId -> makeStep step rgId)

    notEmpty
    *> uniqueIds
    *> traverse (fun steps -> steps |> (validateStep >> fromResult)) steps

let validateInputs inputs =

    let makeInput stepId rate pid nid =
        let res: RoutingInput =
            { StepId = stepId
              SkuId = pid
              NodeId = nid
              ConversionRate = rate }

        res

    if List.isEmpty inputs then
        Invalid [ DomainError.validation "Routing must contain at least one input" ]
    else
        inputs
        |> List.map (fun (input: DefineRoutingInput) ->
            makeInput input.StepId input.ConversionRate
            <!> (SkuId.create input.SkuId |> fromResult)
            <*> (NodeId.create input.NodeId |> fromResult))
        |> sequence

let validateOutputs outputs =
    let makeOutput stepId rate isCoSku pid nid =
        let res: RoutingOutput =
            { StepId = stepId
              SkuId = pid
              NodeId = nid
              ConversionRate = rate
              IsCoSku = isCoSku }

        res

    if List.isEmpty outputs then
        Invalid [ DomainError.validation "Routing must contain at least one output" ]
    else
        outputs
        |> List.map (fun (output: DefineRoutingOutput) ->
            makeOutput output.StepId output.ConversionRate output.IsCoSku
            <!> (SkuId.create output.SkuId |> fromResult)
            <*> (NodeId.create output.NodeId |> fromResult))
        |> sequence

let validateAndDefineStepResources stepResources =
    let makeStepResource stepId seqNr isAllowed durationPerUnitMinutes resourceId =
        let res: StepResourceMap =
            { StepId = stepId
              ResourceId = resourceId
              IsAllowed = isAllowed
              Sequence = seqNr
              DurationPerUnitMinutes = durationPerUnitMinutes }

        res

    traverse
        (fun step ->
            makeStepResource step.StepId step.Sequence step.IsAllowed step.DurationPerUnitMinutes
            <!> (ResourceId.create step.ResourceId |> fromResult))
        stepResources

let private validateAndCreateRouting (routing: DefineRoutingCmd) =
    let nameValidation = required "Routing name" routing.Name

    let stepsValidation = validateAndTranslateSteps routing.Steps
    let inputsValidation = validateInputs routing.Inputs
    let outputsValidation = validateOutputs routing.Outputs
    let stepResources = validateAndDefineStepResources routing.StepResources

    let combine id name validSteps validInputs validOutputs resources =
        let routing: Routing =
            { Id = id
              Name = name
              Type = routing.Type
              EffectiveStart = routing.EffectiveStart
              EffectiveEnd = routing.EffectiveEnd
              Steps = validSteps
              Inputs = validInputs
              Outputs = validOutputs
              StepResources = resources
              CreatedAt = routing.CreatedAt
              ModifiedAt = routing.ModifiedAt
              Status = routing.Status }

        routing

    combine routing.Id <!> nameValidation
    <*> stepsValidation
    <*> inputsValidation
    <*> outputsValidation
    <*> stepResources

let decide: DecideRouting =
    fun command stateOpt ->
        match command, stateOpt with
        | DefineRouting routing, None ->
            validateAndCreateRouting routing
            |> toResult
            |> Result.mapError DomainError.combineValidationErrors
            |> Result.map (fun routing ->
                { NewState = routing
                  Events = [ RoutingDefined routing ] })
        | DefineRouting _, Some _ -> Error(DomainError.invariant "Routing already exists")

        | ActivateRouting(id), Some state when state.Id = id ->
            match state.Status with
            | Active -> Error(DomainError.invariant "Routing is already active")
            | Inactive ->
                let updated =
                    { state with
                        Status = Active
                        ModifiedAt = Timestamp.now }

                { NewState = updated
                  Events = [ RoutingActivated(id, updated.ModifiedAt) ] }
                |> Ok
        | ActivateRouting _, Some _ -> Error(DomainError.validation "Routing not found")

        | DeactivateRouting(id), Some state when state.Id = id ->
            match state.Status with
            | Inactive -> Error(DomainError.invariant "Routing is already inactive")
            | Active ->
                let updated =
                    { state with
                        Status = Inactive
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
                    Status = Active
                    ModifiedAt = modifiedAt }
        | RoutingDeactivated(id, modifiedAt), Some state when state.Id = id ->
            Some
                { state with
                    Status = Inactive
                    ModifiedAt = modifiedAt }
        | RoutingDefined _, Some state -> Some state
        | _, current -> current

let isEffective (asOf: Timestamp) (routing: Routing) =
    let inStart = asOf >= routing.EffectiveStart

    let inEnd =
        routing.EffectiveEnd
        |> Option.map (fun value -> asOf <= value)
        |> Option.defaultValue true

    routing.Status = Active && inStart && inEnd
