/// CA-D-003 Sense Demand — Capability Parent API
/// Traces to: CA-D-003, FS-D-009, FS-D-010
module Medhavi.Demand.SenseDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.SenseDemand.DemandBehaviorAssessment

let create
    (aggregateApi: Capabilities.AggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : DemandBehaviorAssessmentApi =

    let initializeBaseline (req: InitializeBaselineReq) : Task<Result<DemandBehaviorAssessmentDto, ApiError>> =
        aggregateApi.InitializeBaseline req
        |> TaskResult.map Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let evaluateSignal (req: EvaluateDemandSignalReq) : Task<Result<DemandBehaviorAssessmentDto, ApiError>> =
        taskResult {
            let! agg = aggregateApi.EvaluateSignal req |> TaskResult.mapError mapAppErrorToApiError

            let dto = Projections.mapToDto agg

            do!
                match dto.StateChangeEvents with
                | latestEvent :: _ when latestEvent.FromState <> latestEvent.ToState ->
                    taskResult {
                        let changeNotif: DemandBehaviorChangedNotification =
                            { Item = agg.Item
                              Location = agg.Location
                              PreviousState = latestEvent.FromState
                              NewState = latestEvent.ToState
                              DeviationMagnitude = latestEvent.DeviationMagnitude
                              Direction = latestEvent.Direction
                              Confidence = latestEvent.Confidence
                              Timestamp = latestEvent.Timestamp.ToString("O") }

                        do!
                            dispatchNotification
                                dispatchEnvelope
                                "BN-D-015"
                                "CA-D-003"
                                "DemandBehaviorAssessment"
                                dto.AssessmentId
                                changeNotif

                        if dto.CurrentState = "Critical" then
                            let critNotif: CriticalDemandBehaviorNotification =
                                { Item = agg.Item
                                  Location = agg.Location
                                  DeviationMagnitude = latestEvent.DeviationMagnitude
                                  Direction = latestEvent.Direction
                                  CorroborationCount = dto.CorroborationCount
                                  Timestamp = latestEvent.Timestamp.ToString("O")
                                  ActionRequired =
                                    "Critical demand behavior detected; evaluate out-of-cycle forecast refresh (FS-D-010)." }

                            do!
                                dispatchNotification
                                    dispatchEnvelope
                                    "BN-D-016"
                                    "CA-D-003"
                                    "DemandBehaviorAssessment"
                                    dto.AssessmentId
                                    critNotif
                    }
                | _ -> TaskResult.return'()

            return dto
        }

    let evaluateForecastRefresh (req: EvaluateForecastRefreshReq) : Task<Result<ForecastRefreshDecisionDto, ApiError>> =
        aggregateApi.EvaluateForecastRefresh req |> TaskResult.mapError mapAppErrorToApiError

    { InitializeBaseline = initializeBaseline
      EvaluateSignal = evaluateSignal
      EvaluateForecastRefresh = evaluateForecastRefresh }
