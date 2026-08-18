/// CA-D-010 Learn From Demand — Capability Parent API
/// Traces to: CA-D-010, CR-D-017, FS-D-017
module Medhavi.Demand.LearnFromDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.LearnFromDemand.DemandLearning

let create (aggregateApi: Capabilities.AggregateApi) (dispatchEnvelope: Envelope -> Task<unit>) : DemandLearningApi =

    let recordLearning (req: RecordDemandLearningReq) : Task<Result<DemandLearningDto, ApiError>> =
        taskResult {
            let! domainAgg = aggregateApi.Establish req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAgg

            let notif: DemandLearningEstablishedNotification =
                { LearningId = dto.LearningId
                  Scope = domainAgg.Scope
                  LearningType = dto.LearningType
                  LearningStatement = dto.LearningStatement
                  PatternConfidence = dto.PatternConfidence
                  InterventionConfidence = dto.InterventionConfidence
                  SupportingEvidenceCount = dto.SupportingEvidence.Length
                  PolicyVersion = dto.PolicyVersion
                  Timestamp = domainAgg.CreatedAt }

            do! dispatchNotification dispatchEnvelope "BN-D-025" "CA-D-010" "DemandLearning" dto.LearningId notif

            return dto
        }

    let deriveLearnings (req: DeriveDemandLearningsReq) : Task<Result<DemandLearningDto list, ApiError>> =
        taskResult {
            let! domainAggs = aggregateApi.DeriveAndEstablishAll req |> TaskResult.mapError mapAppErrorToApiError
            let dtos = domainAggs |> List.map Projections.mapToDto

            let rec dispatchAll (pairs: (Model.DemandLearning * DemandLearningDto) list) : TaskResult<unit, ApiError> =
                taskResult {
                    match pairs with
                    | [] -> return ()
                    | (domainAgg, dto) :: tail ->
                        let notif: DemandLearningEstablishedNotification =
                            { LearningId = dto.LearningId
                              Scope = domainAgg.Scope
                              LearningType = dto.LearningType
                              LearningStatement = dto.LearningStatement
                              PatternConfidence = dto.PatternConfidence
                              InterventionConfidence = dto.InterventionConfidence
                              SupportingEvidenceCount = dto.SupportingEvidence.Length
                              PolicyVersion = dto.PolicyVersion
                              Timestamp = domainAgg.CreatedAt }

                        do!
                            dispatchNotification
                                dispatchEnvelope
                                "BN-D-025"
                                "CA-D-010"
                                "DemandLearning"
                                dto.LearningId
                                notif

                        return! dispatchAll tail
                }

            do! dispatchAll(List.zip domainAggs dtos)

            return dtos
        }

    { DeriveLearnings = deriveLearnings
      RecordLearning = recordLearning }
