/// CA-C-020 Exception Management Capabilities
module Medhavi.Core.ExceptionManagement.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Core
open Medhavi.Core.ArsIdentifiers
open Model
open Policies
open Medhavi.Core

/// Creates the public API for Exception Management
let create
    (repo: Repository<CoreException, ExceptionId, ExceptionEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: ExceptionManagementPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<ExceptionEvent>)
    (getCurrentSeverity: ExceptionId -> Task<VocabularyEntryId option>)
    : Exception.ExceptionApi =

    let getId (cmd: ExceptionCmd) =
        match cmd with
        | ProcessEvidence c -> c.ExceptionId
        | Resolve c -> c.ExceptionId

    { ProcessEvidence =
        fun req ->
            task {
                let validatedCmd = ACL.toProcessEvidenceCmd req

                match validatedCmd with
                | Valid cmd ->
                    let! currentSeverity = getCurrentSeverity cmd.ExceptionId
                    let decider = Behaviors.decide policy currentSeverity
                    let pipeline = CommandPipeline.create repo getId decider deps

                    return!
                        CommandCapabilities.runCapability
                            (fun c -> Valid(ExceptionCmd.ProcessEvidence c))
                            pipeline
                            publishKnowledge
                            (fun agg -> Projections.mapToDto agg currentSeverity)
                            mapAppErrorToApiError
                            cmd
                | Invalid errs ->
                    return
                        Error(
                            DomainError.combineValidationErrors errs
                            |> ApplicationError.fromDomainError
                            |> mapAppErrorToApiError
                        )
            }
      Resolve =
        let cmd = ACL.toResolveCmd >> Validation.map ExceptionCmd.Resolve
        let decider = Behaviors.decide policy None
        let pipeline = CommandPipeline.create repo getId decider deps

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                (fun agg -> Projections.mapToDto agg None)
                mapAppErrorToApiError
                req }
