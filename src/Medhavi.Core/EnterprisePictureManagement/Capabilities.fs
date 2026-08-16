/// CA-C-019 Enterprise Picture Management Capabilities
module Medhavi.Core.EnterprisePictureManagement.Capabilities

open Medhavi.SemanticModel
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Observation
open Medhavi
open Model
open Medhavi.Core

type EnterprisePictureDto = Contracts.Core.EnterprisePicture

/// Creates the public API for Enterprise Picture Management
let create
    (repo: Repository<EnterprisePicture, PlanningScopeId, EnterprisePictureEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: EnterprisePicturePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<EnterprisePictureEvent>)
    : Contracts.Core.EnterprisePictureApi =

    let decider = Behaviors.decide policy

    let getId (cmd: EnterprisePictureCmd) = cmd.PlanningScopeId

    let pipeline = CommandPipeline.create repo getId decider deps

    { Compose =
        let cmd = ACL.toComposeCmd >> Validation.map EnterprisePictureCmd.Compose

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projection.mapToDto
                mapAppErrorToApiError
                req
      Publish =
        let cmd = ACL.toPublishCmd >> Validation.map EnterprisePictureCmd.Publish

        fun req ->
            CommandCapabilities.runCapability
                cmd
                pipeline
                publishKnowledge
                Projection.mapToDto
                mapAppErrorToApiError
                req }
