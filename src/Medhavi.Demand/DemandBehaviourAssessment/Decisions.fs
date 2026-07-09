module Medhavi.Demand.DemandBehaviourAssessment.Decisions

open System
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand.DemandBehaviourAssessment.Model
open Medhavi.Demand.DemandBehaviourAssessment.Rules

let decide
    (cmd: DemandBehaviourAssessmentCommand)
    (stateOpt: DemandBehaviourAssessment option)
    : Result<Decision<DemandBehaviourAssessment, DemandBehaviourAssessmentEvent>, DomainError> =
    match cmd with
    | EvaluateSignal cmd ->
        let state =
            stateOpt
            |> Option.defaultValue
                { SkuId = cmd.SkuId
                  StockingPointId = cmd.StockingPointId
                  CurrentState = Normal
                  LastUpdated = Timestamp.now
                  CurrentDeviation = None
                  Confidence = PositiveDecimal.Zero
                  CorroboratingSignalCount = 0
                  BaselineReference = "Initial"
                  ActiveSources = []
                  LastSignalTime = None
                  LastStateChange = None
                  BusinessTime = Timestamp.now
                  TransactionTime = Timestamp.now }

        let dev = calculateDeviation cmd.Signal.Value cmd.Signal.RecentBaseline cmd.Signal.StatisticalBound
        let direction = if dev >= 0.0M then Increase else Decrease

        if isNoise dev then
            Ok
                { NewState = state
                  Events = []
                  Trace =
                    Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandSignalStateChange
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.senseDemand
                          RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.noiseFilter, 1) ]
                          PolicyId = Some ArsIdentifiers.Demand.Policies.noiseSuppression
                          PolicyVersion = Some 1
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssessment ]
                          Rationale =
                            { Summary = "Signal below noise threshold"
                              Evidence = []
                              Alternatives = [] } } }
        else
            let signalTime = Timestamp.create cmd.Signal.Timestamp

            let recentSources =
                match state.LastSignalTime with
                | None -> [ cmd.Signal.Source ]
                | Some lastTime ->
                    if (Timestamp.value signalTime - Timestamp.value lastTime).TotalHours <= 24.0 then
                        if List.contains cmd.Signal.Source state.ActiveSources then
                            state.ActiveSources
                        else
                            cmd.Signal.Source :: state.ActiveSources
                    else
                        [ cmd.Signal.Source ]

            let isCorroborated = recentSources.Length >= 2
            let significantThreshold = getSignificantThreshold cmd.IsHighPriority
            let isSignificant = abs dev >= significantThreshold
            let isCritical = abs dev >= defaultCriticalThreshold

            let targetState =
                if isCritical && isCorroborated then
                    Critical
                elif isSignificant then
                    if dev >= 0.0M then Elevated else Depressed
                else
                    Normal

            if targetState = state.CurrentState then
                Ok
                    { NewState = state
                      Events = []
                      Trace =
                        Some
                            { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandSignalStateChange
                              CapabilityId = ArsIdentifiers.Demand.Capabilities.senseDemand
                              RulesEvaluated =
                                [ (ArsIdentifiers.Demand.Rules.stateChangeElevatedThreshold, 1)
                                  (ArsIdentifiers.Demand.Rules.stateChangeCriticalCorrobor, 1) ]
                              PolicyId = Some ArsIdentifiers.Demand.Policies.stateChangeRouting
                              PolicyVersion = Some 1
                              SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssessment ]
                              Rationale =
                                { Summary = "No state change warranted"
                                  Evidence = []
                                  Alternatives = [] } } }
            else
                let corroborationFactor = if isCorroborated then 1.0M else 0.8M
                let confidenceRaw = (cmd.Signal.SourceReliability / 100.0M) * corroborationFactor
                let confidence = PositiveDecimal.createSafe(min 1.0M (max 0.0M confidenceRaw))
                let deviationMagnitude = PositiveDecimal.createSafe(abs dev)
                let eventId = Guid.NewGuid().ToString()

                let changeEvent: StateChangeEvent =
                    { EventId = eventId
                      Timestamp = signalTime
                      PreviousState = state.CurrentState
                      NewState = targetState
                      DeviationMagnitude = deviationMagnitude
                      DeviationDirection = direction
                      ConfidenceScore = confidence
                      CorroboratingSignalCount = recentSources.Length
                      BaselineReference = "Baseline-1"
                      TriggeringSignalId = Some cmd.Signal.SignalId }

                let updatedAssessment =
                    { state with
                        CurrentState = targetState
                        LastUpdated = signalTime
                        CurrentDeviation = Some deviationMagnitude
                        Confidence = confidence
                        CorroboratingSignalCount = recentSources.Length
                        ActiveSources = recentSources
                        LastSignalTime = Some signalTime
                        LastStateChange = Some changeEvent
                        BusinessTime = signalTime
                        TransactionTime = Timestamp.now }

                Ok
                    { NewState = updatedAssessment
                      Events = [ BehaviourStateChanged(updatedAssessment, changeEvent) ]
                      Trace =
                        Some
                            { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandSignalStateChange
                              CapabilityId = ArsIdentifiers.Demand.Capabilities.senseDemand
                              RulesEvaluated =
                                [ (ArsIdentifiers.Demand.Rules.stateChangeElevatedThreshold, 1)
                                  (ArsIdentifiers.Demand.Rules.stateChangeCriticalCorrobor, 1)
                                  (ArsIdentifiers.Demand.Rules.highPrioritySensitivity, 1) ]
                              PolicyId = Some ArsIdentifiers.Demand.Policies.stateChangeRouting
                              PolicyVersion = Some 1
                              SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssessment ]
                              Rationale =
                                { Summary = $"State changed to {targetState.AsString()}"
                                  Evidence = []
                                  Alternatives = [] } } }

    | Acknowledge cmd ->
        match stateOpt with
        | None ->
            Error(DomainError.notFound("DemandBehaviourAssessment", cmd.AssignmentId))
        | Some state ->
            let evt =
                BehaviourAssessmentAcknowledged(
                    cmd.SkuId,
                    cmd.StockingPointId,
                    cmd.PlannerIdentity,
                    cmd.Justification,
                    Timestamp.now
                )

            Ok
                { NewState = state
                  Events = [ evt ]
                  Trace =
                    Some
                        { DecisionId = ArsIdentifiers.Demand.Decisions.evaluateDemandSignalStateChange
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.senseDemand
                          RulesEvaluated = []
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.demandBehaviourAssessment ]
                          Rationale =
                            { Summary = $"Demand behaviour assessment acknowledged by planner {cmd.PlannerIdentity}."
                              Evidence = [ $"Justification: {cmd.Justification}" ]
                              Alternatives = [] } } }

/// DE‑D‑031 Trigger Forecast Refresh on Critical State
let triggerForecastRefresh
    (state: DemandBehaviourAssessment)
    (forecastAgeHours: float)
    (expectedImprovement: decimal)
    : Result<bool, DomainError> =
    if state.CurrentState <> Critical then
        Ok false
    else
        Ok(forecastAgeHours >= 24.0 && expectedImprovement >= 0.05M)
