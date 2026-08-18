/// Anti-Corruption Layer (ACL) for Demand Explanation
/// Uses Applicative validation combinators and builds EstablishExplanationCmd
module Medhavi.Demand.ExplainDemand.DemandExplanation.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Policies
open TemplateCatalog
open ReasoningGraphAlgorithms
open Algorithms

/// Translates EstablishDemandExplanationReq into EstablishExplanationCmd
let toEstablishCmd
    (policy: ExplanationGovernancePolicy)
    (req: EstablishDemandExplanationReq)
    : Validation<EstablishExplanationCmd, DomainError> =

    let create artifactType artifactId =
        let templateVersion = req.TemplateVersion |> Option.defaultValue policy.DefaultTemplateVersion

        let template =
            TemplateCatalog.tryGetTemplate templateVersion artifactType
            |> Option.defaultWith(fun () ->
                { TemplateId = "TPL-GENERIC-v1.0"
                  Version = templateVersion
                  ArtifactType = artifactType
                  RequiredEvidenceTypes = []
                  RequiredDecisionTypes = []
                  RelationshipMapping = Map.empty
                  SummaryTemplate = "Explanation for {ArtifactType} '{ArtifactId}' v{Version}." })

        let sourceRefs: ExplanationSourceArtifactRef list =
            req.EvidenceRefs
            |> List.map(fun r ->
                { ArtifactType = r.ArtifactType
                  ArtifactId = r.ArtifactId
                  Version = r.Version
                  Properties = r.Properties })

        let factors: FactorContribution list = []

        let completenessScore, missingEvidence = Algorithms.evaluateCompleteness template sourceRefs

        let graph =
            ReasoningGraphAlgorithms.buildReasoningGraph
                artifactType
                artifactId
                req.Version
                [] // decision traces populated from domain context
                sourceRefs
                factors
                template

        let flatProps =
            [ "ArtifactType", artifactType
              "ArtifactId", artifactId
              "Version", string req.Version ]
            |> Map.ofList

        let mergedProps =
            sourceRefs |> List.fold (fun acc r -> r.Properties |> Map.fold (fun a k v -> Map.add k v a) acc) flatProps

        let renderings =
            Algorithms.composeMultiLevelRenderings
                graph
                template
                mergedProps
                factors
                0.0m
                completenessScore
                missingEvidence
                req.WhatIfAssumption
                []

        let id = DemandExplanationId.ofComponents artifactType artifactId req.Version

        { ExplanationId = id
          ExplainedArtifactType = artifactType
          ExplainedArtifactId = artifactId
          Version = req.Version
          StructuredReasoningGraph = graph
          MultiLevelRenderings = renderings
          FactorContributions = factors
          PreservedEvidenceRefs = sourceRefs
          TemplateVersion = templateVersion
          ExplainabilityScore = completenessScore
          WhatIfAssumption = req.WhatIfAssumption
          CreationTime = Timestamp.now() }

    create <!> required "ExplainedArtifactType" req.ExplainedArtifactType
    <*> required "ExplainedArtifactId" req.ExplainedArtifactId
