module Medhavi.Demand.ForecastPublication.Decisions

open Medhavi.Common
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.ForecastPublication.Model

let initiate (cmd: InitiateForecastCycleCmd) : Result<ForecastPublicationEvent list, DomainError> =
    let pub: ForecastPublication =
        { Id = cmd.PublicationId
          PlanningScopeIds = cmd.PlanningScopeIds
          ForecastHorizon = cmd.ForecastHorizon
          TimeBucketConfig = cmd.TimeBucketConfig
          Status = Draft
          Version = 1
          ChampionModelId = None
          OverallConfidenceIndex = None
          Forecasts = Map.empty
          Coverage = []
          Assumptions = Map.empty
          Overrides = Map.empty
          TransactionTime = Timestamp.now
          PublicationTime = None
          SupersededPublicationId = None }

    Ok [ ForecastCycleInitiated(pub, [], []) ]

let prepareContext
    (cmd: PrepareForecastContextCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    Rules.publishedImmutable state.Status
    |> Result.map(fun _ ->
        let newAssumptions = cmd.Assumptions |> List.map(fun a -> a.AssumptionId, a) |> Map.ofList

        let updated =
            { state with
                Assumptions = Map.fold (fun acc k v -> Map.add k v acc) state.Assumptions newAssumptions
                Coverage = cmd.Coverage
                TransactionTime = Timestamp.now }

        [ ForecastContextPrepared updated ])

let selectChampion
    (cmd: SelectChampionModelCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    Rules.publishedImmutable state.Status
    |> Result.map(fun _ ->
        // In real implementation, we'd evaluate metrics; here we just record
        let updated =
            { state with
                ChampionModelId = Some cmd.CandidateModelId
                TransactionTime = Timestamp.now }

        [ ChampionModelSelected(
              updated,
              state.ChampionModelId |> Option.defaultValue "",
              cmd.CandidateModelId,
              Map.empty
          ) ])

let generateBaselineForecasts
    (cmd: GenerateBaselineForecastsCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    result {
        // BR‑D‑029: cannot modify published
        do! Rules.publishedImmutable state.Status
        do! Rules.validateForecastLines cmd.Forecasts

        let updated =
            { state with
                Forecasts = cmd.Forecasts |> List.map(fun f -> ForecastId.value f.ForecastId, f) |> Map.ofList
                TransactionTime = Timestamp.now }

        return [ BaselineForecastsGenerated(updated, cmd.Forecasts) ]
    }

let recordOverride
    (cmd: RecordForecastOverrideCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    result {
        do! Rules.publishedImmutable state.Status
        do! Rules.overrideJustification cmd.Justification
        // Check deviation if forecast exists
        match state.Forecasts.TryFind(ForecastId.value cmd.ForecastId) with
        | Some f -> do! Rules.overrideDeviation f.Mean (PositiveDecimal.value cmd.NewValue) 50m
        | None -> ()

        let ovr: ForecastOverride =
            { ForecastId = cmd.ForecastId
              OriginalValue =
                state.Forecasts.TryFind(ForecastId.value cmd.ForecastId)
                |> Option.map(fun f -> f.Mean)
                |> Option.defaultValue 0m
              OverrideValue = PositiveDecimal.value cmd.NewValue
              Justification = cmd.Justification
              PlannerIdentity = cmd.PlannerIdentity
              DecisionId = ArsIdentifiers.Demand.Decisions.evaluateForecastOverride
              OverrideTimestamp = Timestamp.now }

        let updated =
            { state with
                Overrides = state.Overrides |> Map.add (ForecastId.value cmd.ForecastId) ovr
                TransactionTime = Timestamp.now }

        return [ ForecastOverrideRecorded(updated, ovr) ]
    }

let reconcileHierarchy
    (cmd: ReconcileForecastHierarchyCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    Rules.publishedImmutable state.Status
    |> Result.map(fun _ ->
        let updatedForecasts =
            match cmd.TargetTotal with
            | None -> state.Forecasts
            | Some targetTotal ->
                let forecastsList = state.Forecasts |> Map.values |> Seq.toList
                if forecastsList.IsEmpty then
                    state.Forecasts
                else
                    let sum = forecastsList |> List.map (fun f -> f.Mean) |> List.sum
                    if sum = 0m then
                        let equalShare = targetTotal / decimal forecastsList.Length
                        forecastsList
                        |> List.map (fun f ->
                            let lower =
                                match PositiveDecimal.create (max 0.0m (equalShare - 20.0m)) with
                                | Ok q -> q
                                | Error err -> failwith err.Message
                            let upper =
                                match PositiveDecimal.create (equalShare + 20.0m) with
                                | Ok q -> q
                                | Error err -> failwith err.Message
                            { f with
                                Mean = equalShare
                                PredictionInterval = { f.PredictionInterval with LowerBound = lower; UpperBound = upper } })
                        |> List.map (fun f -> ForecastId.value f.ForecastId, f)
                        |> Map.ofList
                    else
                        forecastsList
                        |> List.map (fun f ->
                            let ratio = f.Mean / sum
                            let newMean = targetTotal * ratio
                            let oldSd = (PositiveDecimal.value f.PredictionInterval.UpperBound - PositiveDecimal.value f.PredictionInterval.LowerBound) / (2.0m * 1.96m)
                            let newLower =
                                match PositiveDecimal.create (max 0.0m (newMean - 1.96m * oldSd)) with
                                | Ok q -> q
                                | Error err -> failwith err.Message
                            let newUpper =
                                match PositiveDecimal.create (newMean + 1.96m * oldSd) with
                                | Ok q -> q
                                | Error err -> failwith err.Message
                            { f with
                                Mean = newMean
                                PredictionInterval = { f.PredictionInterval with LowerBound = newLower; UpperBound = newUpper } })
                        |> List.map (fun f -> ForecastId.value f.ForecastId, f)
                        |> Map.ofList

        let updated =
            { state with
                Forecasts = updatedForecasts
                TransactionTime = Timestamp.now }

        [ ForecastHierarchyReconciled updated ])

let publishPublication
    (_: PublishForecastPublicationCmd)
    (state: ForecastPublication)
    : Result<ForecastPublicationEvent list, DomainError> =
    result {
        do! Rules.publishedImmutable state.Status

        if state.Status <> Draft then
            return! Error(DomainError.validation "Forecast Publication must be in Draft state to publish")
        else
            let updated =
                { state with
                    Status = Published
                    PublicationTime = Some Timestamp.now
                    SupersededPublicationId = None }

            return [ ForecastPublicationPublished(updated, None) ]
    }

let decide: Decide<ForecastPublication, ForecastPublicationCommand, ForecastPublicationEvent> =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | InitiateForecastCycle cmd, None ->
            initiate cmd
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    None
                    events
                    (Some
                        { DecisionId = ""
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated = []
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Cycle initiated"
                              Evidence = []
                              Alternatives = [] } }))
        | PrepareForecastContext cmd, Some state ->
            prepareContext cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ""
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated = []
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Context prepared"
                              Evidence = []
                              Alternatives = [] } }))
        | SelectChampionModel cmd, Some state ->
            selectChampion cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.selectChampionModel
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated =
                            [ (ArsIdentifiers.Demand.Rules.championSelectionSignificance, 1)
                              (ArsIdentifiers.Demand.Rules.noHarmBias, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.automaticChampionPromotion
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Champion selected"
                              Evidence = []
                              Alternatives = [] } }))
        | GenerateBaselineForecasts cmd, Some state ->
            generateBaselineForecasts cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.generateForecastForSeries
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated =
                            [ (ArsIdentifiers.Demand.Rules.forecastNonNegative, 1)
                              (ArsIdentifiers.Demand.Rules.dataSufficiency, 1)
                              (ArsIdentifiers.Demand.Rules.predictionIntervalCompleteness, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.unforecastableSeriesHandling
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Baseline generated"
                              Evidence = []
                              Alternatives = [] } }))
        | RecordForecastOverride cmd, Some state ->
            recordOverride cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateForecastOverride
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated =
                            [ (ArsIdentifiers.Demand.Rules.overrideJustification, 1)
                              (ArsIdentifiers.Demand.Rules.overrideDeviationLimit, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.overrideAuthorization
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Override recorded"
                              Evidence = []
                              Alternatives = [] } }))
        | ReconcileForecastHierarchy cmd, Some state ->
            reconcileHierarchy cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.reconcileHierarchyNode
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.reconciliationSumEqualsParent, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.reconciliationAutoSelect
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Hierarchy reconciled"
                              Evidence = []
                              Alternatives = [] } }))
        | PublishForecastPublication cmd, Some state ->
            publishPublication cmd state
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some state)
                    events
                    (Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.approveForecastPublication
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                          RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.forecastPublicationCompleteness, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.autoPublicationConfidence
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.forecastPublication ]
                          Rationale =
                            { Summary = "Published"
                              Evidence = []
                              Alternatives = [] } }))
        | _ -> Error(DomainError.validation "Command invalid for current state")
