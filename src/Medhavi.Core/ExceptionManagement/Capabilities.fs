/// CA-C-020 Exception Management Capabilities
module Medhavi.Core.ExceptionManagement.Capabilities

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
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
    : Exception.ExceptionApi =

    let decider = Behaviors.decide policy

    let getId (cmd: ExceptionCmd) =
        match cmd with
        | Register c -> c.ExceptionId
        | Resolve c -> c.ExceptionId

    let pipeline = CommandPipeline.create repo getId decider deps

    { Register =
        let cmd = ACL.toRegisterCmd >> Validation.map ExceptionCmd.Register

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projections.mapToDto
                mapAppErrorToApiError
                req
      Resolve =
        let cmd = ACL.toResolveCmd >> Validation.map ExceptionCmd.Resolve

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projections.mapToDto
                mapAppErrorToApiError
                req }
