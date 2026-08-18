/// CA-D-008 — Detect Demand Exceptions Parent Capability API
module Medhavi.Demand.DetectDemandExceptions.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence
open Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.Model

let create (aggregateApi: Capabilities.AggregateApi) (dispatchEnvelope: Envelope -> Task<unit>) : DemandExceptionApi =

    let evaluateException
        (req: EvaluateDemandExceptionReq)
        : Task<Result<DemandExceptionEvidenceDto option, ApiError>> =
        taskResult {
            let! domainAgg = aggregateApi.Evaluate req |> TaskResult.mapError mapAppErrorToApiError
            let dtoOpt = Projections.mapToDto domainAgg

            do!
                match domainAgg.History |> List.tryHead with
                | Some latestRecord when not latestRecord.IsResolution ->
                    taskResult {

                        let notif: DemandExceptionDetectionEvidenceNotification =
                            { EvidenceId = DemandExceptionEvidenceId.value latestRecord.EvidenceId
                              ExceptionType = latestRecord.ExceptionType.AsString
                              EntityType = latestRecord.PlanningEntityType
                              EntityId = latestRecord.PlanningEntityId
                              Scope = latestRecord.Scope
                              Severity =
                                latestRecord.Severity |> Option.map(fun s -> s.AsString) |> Option.defaultValue "Low"
                              TriggeringMetric = latestRecord.TriggeringMetric
                              MetricValue = latestRecord.MetricValue
                              ThresholdValue = latestRecord.ThresholdValue
                              Rationale = latestRecord.Rationale
                              Timestamp = latestRecord.Timestamp }

                        do!
                            dispatchNotification
                                dispatchEnvelope
                                "BN-D-022"
                                "CA-D-008"
                                "DemandExceptionEvidence"
                                (DemandExceptionEvidenceId.value latestRecord.EvidenceId)
                                notif
                    }
                | Some latestRecord when latestRecord.IsResolution ->
                    taskResult {
                        let notif: DemandExceptionResolutionEvidenceNotification =
                            { EvidenceId = DemandExceptionEvidenceId.value latestRecord.EvidenceId
                              ExceptionType = latestRecord.ExceptionType.AsString
                              EntityType = latestRecord.PlanningEntityType
                              EntityId = latestRecord.PlanningEntityId
                              Scope = latestRecord.Scope
                              ResolutionMetric = latestRecord.TriggeringMetric
                              MetricValue = latestRecord.MetricValue
                              ThresholdValue = latestRecord.ThresholdValue
                              Rationale = latestRecord.Rationale
                              Timestamp = latestRecord.Timestamp }

                        do!
                            dispatchNotification
                                dispatchEnvelope
                                "BN-D-023"
                                "CA-D-008"
                                "DemandExceptionEvidence"
                                (DemandExceptionEvidenceId.value latestRecord.EvidenceId)
                                notif
                    }
                | _ -> TaskResult.return'()

            return dtoOpt
        }

    { EvaluateException = evaluateException }
