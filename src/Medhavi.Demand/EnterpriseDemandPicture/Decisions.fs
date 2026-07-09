module Medhavi.Demand.EnterpriseDemandPicture.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.EnterpriseDemandPicture.Model

let revise (cmd: ReviseEdpCmd) (state: EnterpriseDemandPicture option) : Result<EdpEvent list, DomainError> =
    match state with
    | Some edp ->
        result {
            // Existing EDP – check status
            do! Rules.supersededImmutable edp.Status
            // Create new version
            let newVersion = edp.Version + 1

            let newOperationalDemand =
                edp.OperationalDemand
                |> Map.change cmd.Period (fun existing ->
                    let current = existing |> Option.defaultValue Quantity.Zero
                    Some(current + cmd.Quantity))

            let newEdp =
                { edp with
                    Version = newVersion
                    Status = AwaitingPlanningDemandCalculation
                    OperationalDemand = newOperationalDemand
                    PlanningDemand = Map.empty
                    TransactionTime = Timestamp.now
                    SupersededVersionId = Some edp.Version }

            return [ EdpRevised newEdp ]
        }
    | None ->
        // First version
        let newOperationalDemand = Map.ofList [ cmd.Period, cmd.Quantity ]

        let newEdp =
            { PlanningScopeId = cmd.PlanningScopeId
              Version = 1
              Status = AwaitingPlanningDemandCalculation
              OperationalDemand = newOperationalDemand
              PlanningDemand = Map.empty
              TransactionTime = Timestamp.now
              PublicationTime = None
              SupersededVersionId = None }

        Ok [ EdpRevised(newEdp) ]

let calculate (cmd: CalculateEdpCmd) (state: EnterpriseDemandPicture) : Result<EdpEvent list, DomainError> =
    result {
        do! Rules.publishedImmutable state.Status
        do! Rules.supersededImmutable state.Status

        match state.Status with
        | AwaitingPlanningDemandCalculation ->
            let allPeriods =
                Set.unionMany [
                    state.OperationalDemand |> Map.keys |> Set.ofSeq
                    cmd.Adjustments |> Map.keys |> Set.ofSeq
                    cmd.Overrides |> Map.keys |> Set.ofSeq
                ]

            let planningDemandLines =
                allPeriods
                |> Seq.map(fun period ->
                    let op = state.OperationalDemand |> Map.tryFind period |> Option.defaultValue Quantity.Zero
                    let adj = cmd.Adjustments |> Map.tryFind period |> Option.defaultValue Quantity.Zero
                    let ovr = cmd.Overrides |> Map.tryFind period |> Option.defaultValue Quantity.Zero
                    let final =
                        match Quantity.create (Quantity.value op + Quantity.value adj + Quantity.value ovr) with
                        | Ok q -> q
                        | Error err -> failwith err.Message
                    period,
                    { OperationalDemand = op
                      Adjustment = adj
                      Override = ovr
                      FinalQuantity = final })
                |> Map.ofSeq

            let newEdp =
                { state with
                    PlanningDemand = planningDemandLines
                    Status = ReadyForPublication
                    TransactionTime = Timestamp.now }

            return [ EdpCalculated newEdp ]
        | _ -> return! Error(DomainError.validation "EDP must be in AwaitingPlanningDemandCalculation state")
    }

let publish (cmd: PublishEdpCmd) (state: EnterpriseDemandPicture) : Result<EdpEvent list, DomainError> =
    result {
        do! Rules.publishedImmutable state.Status
        do! Rules.supersededImmutable state.Status

        match state.Status with
        | ReadyForPublication ->
            // Find previous Published version ID for superseding – done at context/repository level
            // We'll just publish with a placeholder SupersededVersionId = None for now; the repository will handle the actual superseding.
            let newEdp =
                { state with
                    Status = Published
                    PublicationTime = Some Timestamp.now }

            return [ EdpPublished(newEdp, state.SupersededVersionId) ]
        | _ -> return! Error(DomainError.validation "EDP must be in ReadyForPublication state")
    }

let decide: Decide<EnterpriseDemandPicture, EdpCommand, EdpEvent> =
    fun cmd stateOpt ->
        match cmd with
        | Revise cmd ->
            revise cmd stateOpt
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    stateOpt
                    events
                    (Some
                        { DecisionId = "" // AB‑EDP‑001
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                          RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.publishedEdpImmutable, 1) ]
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.enterpriseDemandPicture ]
                          Rationale =
                            { Summary = "EDP revised with new observation"
                              Evidence = []
                              Alternatives = [] } }))
        | Calculate cmd ->
            match stateOpt with
            | None -> Error(DomainError.validation "Cannot calculate on non‑existent EDP")
            | Some state ->
                calculate cmd state
                |> Result.map(fun events ->
                    buildDecision
                        evolve
                        (Some state)
                        events
                        (Some
                            { DecisionId = ArsIdentifiers.Demand.Decisions.applyPlanningAdjustment
                              CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                              RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.operationalDemandNotAdjusted, 1) ]
                              PolicyId = Some ArsIdentifiers.Demand.Policies.planningAdjustmentApproval
                              PolicyVersion = Some 1
                              SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.enterpriseDemandPicture ]
                              Rationale =
                                { Summary = "Planning demand calculated"
                                  Evidence = []
                                  Alternatives = [] } }))
        | Publish cmd ->
            match stateOpt with
            | None -> Error(DomainError.validation "Cannot publish non‑existent EDP")
            | Some state ->
                publish cmd state
                |> Result.map(fun events ->
                    buildDecision
                        evolve
                        (Some state)
                        events
                        (Some
                            { DecisionId = ArsIdentifiers.Demand.Decisions.publishEnterpriseDemandPicture
                              CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                              RulesEvaluated =
                                [ (ArsIdentifiers.Demand.Rules.exactlyOnePublishedEDP, 1)
                                  (ArsIdentifiers.Demand.Rules.publishedEdpImmutable, 1) ]
                              PolicyId = Some ArsIdentifiers.Demand.Policies.publicationTransfersResponsibility
                              PolicyVersion = Some 1
                              SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.enterpriseDemandPicture ]
                              Rationale =
                                { Summary = "EDP published"
                                  Evidence = []
                                  Alternatives = [] } }))
