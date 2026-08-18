/// CA-D-011 Model Demand Interventions — Capability Parent API
/// Traces to: CA-D-011, CR-D-018, CR-D-019, FS-D-018, FS-D-019
module Medhavi.Demand.ModelDemandInterventions.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ModelDemandInterventions.DemandInterventionImpact

let create
    (aggregateApi: Capabilities.AggregateApi)
    (dispatchEnvelope: Envelope -> Task<unit>)
    : DemandInterventionApi =

    let assessImpact (req: AssessInterventionImpactReq) : Task<Result<DemandInterventionImpactDto, ApiError>> =
        aggregateApi.AssessImpact req
        |> TaskResult.map Projections.mapToDto
        |> TaskResult.mapError mapAppErrorToApiError

    let publishImpact (req: PublishInterventionImpactReq) : Task<Result<DemandInterventionImpactDto, ApiError>> =
        taskResult {
            let! domainAgg = aggregateApi.PublishImpact req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAgg

            let notif: DemandInterventionImpactPublishedNotification =
                { ImpactId = dto.ImpactId
                  InterventionReference = dto.InterventionReference
                  Item = domainAgg.Item
                  Location = domainAgg.Location
                  AssessedDemandLift = domainAgg.AssessedDemandLift
                  LiftConfidence = dto.LiftConfidence
                  TemporalValidityStart = domainAgg.TemporalValidity.Start
                  TemporalValidityEnd = domainAgg.TemporalValidity.End
                  ModelProvenance = dto.ModelProvenance
                  Timestamp = domainAgg.CreatedAt }

            do!
                dispatchNotification
                    dispatchEnvelope
                    "BN-D-026"
                    "CA-D-011"
                    "DemandInterventionImpact"
                    dto.ImpactId
                    notif

            return dto
        }

    { AssessImpact = assessImpact
      PublishImpact = publishImpact }
