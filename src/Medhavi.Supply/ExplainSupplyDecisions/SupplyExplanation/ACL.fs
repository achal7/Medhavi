/// Anti-Corruption Layer (ACL) for Supply Explanation
/// Translates external RecordSupplyExplanationReq into ComposeSupplyExplanationCmd using applicative validation
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Validations
open Medhavi.SemanticModel
open Medhavi.Contracts.Supply
open Medhavi.Supply
open Model
open Policies
open TemplateCatalog

/// Translates RecordSupplyExplanationReq into domain ComposeSupplyExplanationCmd
let toComposeCmd
    (policy: ExplanationPolicy)
    (decisionTraces: DecisionTrace list)
    (req: RecordSupplyExplanationReq)
    : Validation<ComposeSupplyExplanationCmd, DomainError> =

    let create artifactType artifactId =
        let templateVersion = req.TemplateVersion |> Option.defaultValue policy.DefaultTemplateVersion

        let template =
            TemplateCatalog.tryGetTemplate templateVersion artifactType
            |> Option.defaultWith(fun () ->
                {
                    TemplateId = "TPL-GENERIC-SUPPLY-v1.0"
                    Version = templateVersion
                    ArtifactType = artifactType
                    RequiredEvidenceTypes = []
                    RequiredDecisionTypes = []
                    RelationshipMapping = Map.empty
                    SummaryTemplate =
                        "Authoritative explanation for {ArtifactType} '{ArtifactId}' (v{Version}) composed from preserved enterprise facts."
                })

        let sourceRefs: ExplanationSourceArtifactRef list =
            req.EvidenceRefs
            |> List.map(fun r ->
                {
                    ArtifactType = r.ArtifactType
                    ArtifactId = r.ArtifactId
                    Version = r.Version
                    Properties = r.Properties
                })

        let factors: FactorContribution list = []

        let negativePayloadOpt =
            if req.IsNegativeExplanation then
                let reasonType =
                    match req.NegativeReasonCategory with
                    | Some "PreconditionViolated" -> NegativeReasonType.PreconditionViolated
                    | Some "GateThresholdUnmet" -> NegativeReasonType.GateThresholdUnmet
                    | Some "DominantAlternativeSelected" -> NegativeReasonType.DominantAlternativeSelected
                    | Some "PolicyConstraintVeto" -> NegativeReasonType.PolicyConstraintVeto
                    | _ -> NegativeReasonType.DominantAlternativeSelected

                let altId = req.NegativeAlternativeId |> Option.defaultValue "UnknownAlternative"
                let altType = req.NegativeAlternativeType |> Option.defaultValue artifactType

                Some
                    {
                        DiscardedAlternativeId = altId
                        DiscardedAlternativeType = altType
                        ReasonCategory = reasonType
                        FormalReason =
                            $"Alternative {altType} '{altId}' was rejected under governed rule {reasonType.AsString}."
                        EvaluationContext = Map.empty
                    }
            else
                None

        let kappaComp, missingEvidence = Algorithms.evaluateCompleteness template sourceRefs

        let gammaInh = Algorithms.computeInheritedConfidence sourceRefs

        let graph =
            ReasoningGraphAlgorithms.buildReasoningGraph
                artifactType
                artifactId
                req.ExplainedArtifactVersion
                decisionTraces
                sourceRefs
                factors
                negativePayloadOpt
                None
                template

        let sigmaCons = Algorithms.computeEvidenceConsistency graph

        let confidence = Algorithms.evaluateConfidenceBreakdown policy gammaInh kappaComp sigmaCons

        let rationaleBlocks = Algorithms.generateRationaleBlocks artifactType artifactId decisionTraces sourceRefs

        let flatProps =
            [
                "ArtifactType", artifactType
                "ArtifactId", artifactId
                "Version", string req.ExplainedArtifactVersion
            ]
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
                confidence
                missingEvidence
                req.WhatIfAssumption
                decisionTraces

        let id = SupplyExplanationId.ofComponents artifactType artifactId req.ExplainedArtifactVersion

        {
            ExplanationId = id
            ExplainedArtifactType = artifactType
            ExplainedArtifactId = artifactId
            ExplainedArtifactVersion = req.ExplainedArtifactVersion
            StructuredReasoningGraph = graph
            MultiLevelRenderings = renderings
            FactorContributions = factors
            PreservedEvidenceRefs = sourceRefs
            RationaleBlocks = rationaleBlocks
            IsNegativeExplanation = req.IsNegativeExplanation
            NegativeExplanation = negativePayloadOpt
            OpaqueModelProvenance = None
            Confidence = confidence
            TemplateVersionReference = templateVersion
            CompositionTimestamp = Timestamp.now()
        }

    create <!> required "ExplainedArtifactType" req.ExplainedArtifactType
    <*> required "ExplainedArtifactId" req.ExplainedArtifactId

/// Maps SupplyExplanationEvent to ARS enterprise event identifier
let toArsEventId: SupplyExplanationEvent -> string =
    function
    | SupplyExplanationRecorded _ -> ArsIdentifiers.EnterpriseEvents.supplyExplanationRecorded.Id
