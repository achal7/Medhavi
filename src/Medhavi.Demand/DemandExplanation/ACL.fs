module Medhavi.Demand.DemandExplanation.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Demand

type ValidatedRequest =
    { ExplanationId: DemandExplanationId
      ExplainedArtifactType: string
      ExplainedArtifactId: string
      Question: string
      BusinessTime: Timestamp
      TransactionTime: Timestamp }

let private notEmpty field =
    validate
        (fun s -> not(System.String.IsNullOrWhiteSpace s))
        (DomainError.validation(field, $"{field} cannot be empty"))

let validateRequest (req: RecordDemandExplanationReq) : Validation<ValidatedRequest, DomainError> =
    let make explId artifactType artifactId question =
        { ExplanationId = explId
          ExplainedArtifactType = artifactType
          ExplainedArtifactId = artifactId
          Question = question
          BusinessTime = Timestamp.create req.BusinessTime
          TransactionTime = Timestamp.now }

    make <!> (DemandExplanationId.create req.ExplanationId |> fromResult)
    <*> notEmpty "ExplainedArtifactType" req.ExplainedArtifactType
    <*> notEmpty "ExplainedArtifactId" req.ExplainedArtifactId
    <*> notEmpty "Question" req.Question
