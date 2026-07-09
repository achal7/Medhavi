module Medhavi.Demand.DemandExplanation.Capabilities

open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure
open Medhavi.Contracts.Demand.DemandLearning
open Medhavi.Demand
open Medhavi.Demand.DemandExplanation.Model
open Medhavi.Demand.DemandExplanation.ACL
open Medhavi.Demand.ReasoningGraphAlgorithms
open Medhavi.SharedKernel.Execution
open Medhavi.Common.Validation
open Medhavi.SharedKernel.Contracts.DecisionTrace

let private publishSuccess (exp: DemandExplanation) =
    let notification: DemandExplanationRecordedNotification =
        { ExplanationId = DemandExplanationId.value exp.Id
          ExplainedArtifactType = exp.ExplainedArtifactType
          ExplainedArtifactId = exp.ExplainedArtifactId
          ExplanationGenerationTimestamp = Timestamp.value exp.ExplanationGenerationTimestamp }

    DomainEventBus.Publish notification

// Traceability: Implements CA‑D‑009 (Explain Demand) Capabilities API for SE‑D‑041 (Demand Explanation)
// Exposes the workflow layer: validates raw requests, fetches decision traces, builds the reasoning graph,
// constructs the domain command, and publishes BN‑D‑024.

let createCapabilities
    (execute: DemandExplanationCommand -> Task<ExecutionOutcome<DemandExplanation, ApplicationError>>)
    (getDecisionTraces: string -> string -> Task<DecisionTrace list>) // (artifactType, artifactId) -> traces
    (getSourceArtifactRefs: string -> string -> Task<ExplanationSourceArtifactRef list>)
    (templateVersionRef: string)
    (generateNaturalLanguage: NaturalLanguageGenerator)
    : DemandExplanationApi =

    let record (req: RecordDemandExplanationReq) =
        task {
            // 1. EARLY VALIDATION
            match validateRequest req with
            | Invalid errors ->
                return
                    Error(
                        ApplicationError.Domain(DomainError.combineValidationErrors errors)
                        |> ApplicationError.mapToApiError
                    )
            | Valid validatedReq ->
                // 2. FETCH TRACES & SOURCE REFS
                let! traces = getDecisionTraces validatedReq.ExplainedArtifactType validatedReq.ExplainedArtifactId

                let! sourceRefs =
                    getSourceArtifactRefs validatedReq.ExplainedArtifactType validatedReq.ExplainedArtifactId

                // 3. BUILD REASONING GRAPH (BA‑D‑009)
                let graph = buildReasoningGraph traces sourceRefs

                // Try template renderer first, fallback to LLM
                let businessContextOpt = graph.Nodes |> List.tryFind(fun n -> n.NodeType = "BusinessContext")

                let templateExplanationOpt =
                    businessContextOpt
                    |> Option.bind(TemplateRenderer.resolve templateVersionRef validatedReq.ExplainedArtifactType)

                let! naturalLangResult =
                    match templateExplanationOpt with
                    | Some text -> Task.FromResult(Ok text)
                    | None ->
                        task {
                            let! llmText =
                                generateNaturalLanguage
                                    graph
                                    validatedReq.ExplainedArtifactType
                                    validatedReq.ExplainedArtifactId
                                    validatedReq.Question

                            return Ok llmText
                        }

                match naturalLangResult with
                | Error err -> return Error(ApplicationError.mapToApiError err)
                | Ok naturalLang ->

                    // 4. CONSTRUCT DOMAIN COMMAND
                    let cmd =
                        { ExplanationId = validatedReq.ExplanationId
                          ExplainedArtifactType = validatedReq.ExplainedArtifactType
                          ExplainedArtifactId = validatedReq.ExplainedArtifactId
                          StructuredReasoningGraph = graph
                          NaturalLanguageExplanation = naturalLang
                          SourceArtifactRefs = sourceRefs
                          TemplateVersionRef = templateVersionRef
                          BusinessTime = validatedReq.BusinessTime
                          TransactionTime = validatedReq.TransactionTime }

                    // 5. EXECUTE
                    let! outcome = execute(RecordDemandExplanation cmd)

                    match Helpers.toApiResult outcome with
                    | Ok exp ->
                        publishSuccess exp
                        return Ok(DemandExplanationId.value exp.Id)
                    | Error err -> return Error err
        }

    { RecordExplanation = record }
