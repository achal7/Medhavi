/// SE-S-080 — Distribution Recommendation ACL
/// Traces to: Supply Intelligence Specification §4.3.1 (SE-S-080)
module Medhavi.Supply.ManageDistribution.DistributionRecommendation.ACL

open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

/// Translates external API request to domain command
let toCreateCmd (req: CreateDistributionRecommendationReq) : Validation<CreateRecommendationCmd, DomainError> =
    (fun reqId timestamp ->
        { RequirementId = reqId
          Timestamp = timestamp })
    <!> validateDistributionRequirementId req.RequirementReference
    <*> validateTimestamp req.Timestamp

/// Outbound event mapping to canonical ARS Enterprise Event (EV-S-070)
let toArsEventId (event: DistributionRecommendationEvent) : string =
    match event with
    | DistributionRecommendationCreated _ -> ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id
    | DistributionRecommendationWithdrawn _ -> ArsIdentifiers.EnterpriseEvents.distributionRecommendationCreated.Id
