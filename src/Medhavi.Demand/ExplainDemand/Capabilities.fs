/// CA-D-009 — Explain Demand Parent Capability API
module Medhavi.Demand.ExplainDemand.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.SemanticModel
open Medhavi.Contracts
open Medhavi.Contracts.Demand
open Medhavi.SharedKernel.BusinessNotifications
open Medhavi.Demand
open Medhavi.Demand.ExplainDemand.DemandExplanation
open Medhavi.Foundation.Failure

let create (aggregateApi: Capabilities.AggregateApi) (dispatchEnvelope: Envelope -> Task<unit>) : DemandExplanationApi =

    let establishExplanation (req: EstablishDemandExplanationReq) : Task<Result<DemandExplanationDto, ApiError>> =
        taskResult {
            let! domainAgg = aggregateApi.Establish req |> TaskResult.mapError mapAppErrorToApiError
            let dto = Projections.mapToDto domainAgg

            let! ts =
                Timestamp.create(Timestamp.value domainAgg.CreatedAt)
                |> Result.mapError(DomainError.validation >> ApplicationError.fromDomainError >> mapAppErrorToApiError)
                |> TaskResult.ofResult

            let notif: DemandExplanationEstablishedNotification =
                { ExplanationId = dto.ExplanationId
                  ExplainedArtifactType = dto.ExplainedArtifactType
                  ExplainedArtifactId = dto.ExplainedArtifactId
                  Version = dto.Version
                  TemplateVersion = dto.TemplateVersion
                  PlannerSummary = dto.MultiLevelRenderings.PlannerSummary
                  ExplainabilityScore = dto.ExplainabilityScore
                  Timestamp = ts }

            do! dispatchNotification dispatchEnvelope "BN-D-024" "CA-D-009" "DemandExplanation" dto.ExplanationId notif

            return dto
        }

    { EstablishExplanation = establishExplanation }
