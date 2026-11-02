/// CA-P-006 Establish Commitments — Promise Explanation ACL
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let toGenerateCmd (req: GeneratePromiseExplanationReq) : Validation<GeneratePromiseExplanationCmd, DomainError> =
    let create expId commId lineId time =
        { ExplanationId = expId
          CommitmentId = commId
          OrderLineId = lineId
          GenerationTime = time }

    create <!> validatePromiseExplanationId req.ExplanationId
           <*> validateOrderCommitmentId req.CommitmentId
           <*> validateOrderLineId req.OrderLineId
           <*> validateTimestamp req.GenerationTime

let toArsEventId = function
    | PromiseExplanationGenerated _ -> ArsIdentifiers.EnterpriseEvents.promiseExplanationGenerated.Id

