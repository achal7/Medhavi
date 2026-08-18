/// CA-C-019 Aggregate Behaviors
module Medhavi.Core.EnterprisePictureManagement.Behaviors

open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Model
open Rules
open Decisions
open Algorithms

let private nextVersionNumber (state: EnterprisePicture option) : Result<PictureVersionId, DomainError> =
    state
    |> Option.map(fun p ->
        p.Versions |> List.map(fun v -> PictureVersionId.value v.VersionNumber) |> List.fold max 0 |> (+) 1)
    |> Option.defaultValue 1
    |> PictureVersionId.create
    |> Result.mapError(fun e -> DomainError.validation(sprintf "Version number creation failed: %A" e))

let private findPublishedVersion (state: EnterprisePicture option) : PictureVersion option =
    state
    |> Option.bind(fun p ->
        p.Versions |> List.tryFind(fun v -> v.LifecycleState = PictureVersionLifecycleState.Published))

/// AB-C-001: Compose Enterprise Picture Version (creates Draft, emits EV-C-001).
let compose
    (policy: EnterprisePicturePolicy)
    (cmd: ComposePictureVersionCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =
    result {
        let input: ComposeInput = { Cmd = cmd; CurrentState = state }
        let! (decision: DecisionOutcome<CompositionOutcome>) = decideComposition Rules.compositionRules input

        match decision.Outcome with
        | CompositionRejected reasons ->
            return!
                Error(
                    DomainError.rule(
                        (String.concat "; " reasons),
                        Medhavi.Core.ArsIdentifiers.Decisions.assessPictureMateriality.Id
                    )
                )
        | ComposedSuccessfully ->
            let! versionNumber = nextVersionNumber state

            let newVersion: PictureVersion =
                { VersionNumber = versionNumber
                  DemandReferences = cmd.DemandReferences
                  SupplyReferences = cmd.SupplyReferences
                  InventoryReferences = cmd.InventoryReferences
                  PublicationTime = None
                  LifecycleState = PictureVersionLifecycleState.Draft }

            let events = [ PictureVersionComposed(cmd.PlanningScopeId, newVersion, cmd.CompositionTriggerTime) ]

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.enterprisePictureManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Composed"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Composed Draft %A" versionNumber
                      Evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.enterprisePicture.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.invariant "EnterprisePicture state must exist after compose")

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-002: Publish Enterprise Picture Version.
/// Internally runs BA-C-001 + DE-C-001. Returns EMPTY events when not material (no-op).
let publish
    (policy: EnterprisePicturePolicy)
    (cmd: PublishPictureVersionCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =
    result {
        let input: PublishInput = { Cmd = cmd; CurrentState = state }

        // Locate draft and current published version
        let draftOpt =
            state |> Option.bind(fun p -> p.Versions |> List.tryFind(fun v -> v.VersionNumber = cmd.VersionNumber))

        let publishedOpt = findPublishedVersion state

        match draftOpt with
        | None ->
            return!
                Error(
                    DomainError.rule(
                        $"Version {cmd.VersionNumber} not found",
                        Medhavi.Core.ArsIdentifiers.Rules.versionMustExist.Id
                    )
                )
        | Some draft ->
            let! currentState =
                state
                |> Result.ofOption(DomainError.invariant "EnterprisePicture state must exist when evaluating draft")

            // BA-C-001: assess materiality
            let assessment = Algorithms.evaluatePictureMateriality policy draft publishedOpt
            // DE-C-001: decide publication
            let! (decision: DecisionOutcome<PublicationOutcome>) =
                Decisions.assessMateriality Rules.publicationRules input assessment

            match decision.Outcome with
            | RetainDraft reason ->
                // Not material: no state change, no event (EV-C-002 suppressed)
                let trace: DecisionTrace =
                    { DecisionId = System.Guid.NewGuid().ToString()
                      CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.enterprisePictureManagement.Id
                      CausalDecisionIds = []
                      Outcome = "RetainDraft"
                      PolicyId = Some policy.PolicyId
                      PolicyVersion = Some policy.Version
                      Rationale =
                        { Summary = reason
                          Evidence = [ assessment.Reason ]
                          Alternatives = [ ("PublishVersion", "Materiality threshold not met") ] }
                      RulesEvaluated = decision.Evaluations
                      SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.enterprisePicture.Id ] }

                return
                    { NewState = currentState
                      Events = []
                      Trace = Some trace }
            | PublishVersion ->
                let events = [ PictureVersionPublished(cmd.PlanningScopeId, cmd.VersionNumber, cmd.PublicationTime) ]

                let trace: DecisionTrace =
                    { DecisionId = System.Guid.NewGuid().ToString()
                      CapabilityId = Medhavi.Core.ArsIdentifiers.Capabilities.enterprisePictureManagement.Id
                      CausalDecisionIds = []
                      Outcome = "Published"
                      PolicyId = Some policy.PolicyId
                      PolicyVersion = Some policy.Version
                      Rationale =
                        { Summary = sprintf "Published version %A" cmd.VersionNumber
                          Evidence = assessment.Reason :: (decision.Evaluations |> List.collect(fun e -> e.Evidence))
                          Alternatives = [ ("RetainDraft", "Materiality threshold met") ] }
                      RulesEvaluated = decision.Evaluations
                      SemanticObjectIds = [ Medhavi.Core.ArsIdentifiers.SemanticObjects.enterprisePicture.Id ] }

                let! newState =
                    events
                    |> List.fold evolve state
                    |> Result.ofOption(DomainError.invariant "EnterprisePicture state must exist after publish")

                return
                    { NewState = newState
                      Events = events
                      Trace = Some trace }
    }

/// Unified decide function for the Execution Engine.
let decide
    (policy: EnterprisePicturePolicy)
    (cmd: EnterprisePictureCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =
    match cmd with
    | Compose c -> compose policy c state
    | Publish c -> publish policy c state
