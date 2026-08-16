/// CA-C-019 Aggregate Behaviors
/// Pure decide functions: Command → State → Result<Decision, DomainError>
/// These are the functions passed to the Execution Engine's CommandPipeline.
module Medhavi.Core.EnterprisePictureManagement.Behaviors

open Medhavi.Common
open Medhavi.SemanticModel
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel.Identities
open Model
open Rules
open Decisions
open Medhavi.Core.ArsIdentifiers

/// Generates the next version number for a picture.
/// Returns 1 for a new picture, or max existing + 1.
let private nextVersionNumber (state: EnterprisePicture option) =
    state
    |> Option.map(fun picture ->
        picture.Versions |> List.map(fun v -> pictureVersionIdValue v.VersionNumber) |> List.fold max 0 |> (+) 1)
    |> Option.defaultValue 1
    |> pictureVersionIdCreate

/// Finds the currently Published version, if any.
let private findPublishedVersion (state: EnterprisePicture option) : PictureVersion option =
    state
    |> Option.bind(fun picture ->
        picture.Versions |> List.tryFind(fun v -> v.LifecycleState = PictureVersionLifecycleState.Published))

/// AB-C-019a: Compose Enterprise Picture Version.
/// Creates a new PictureVersion in Draft state and appends it to the picture.
let compose
    (policy: EnterprisePicturePolicy)
    (cmd: ComposePictureVersionCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =
    result {
        let input: ComposeInput = { Cmd = cmd; CurrentState = state }
        let! decision = decideComposition Rules.compositionRules input

        let! versionNumber =
            nextVersionNumber state
            |> Result.mapError(fun _ -> DomainError.rule("Failed to generate next version number", "AB-C-019"))

        match decision.Outcome with
        | CompositionRejected reasons -> return! Error(DomainError.rule((String.concat "; " reasons), "DE-C-019a"))

        | ComposedSuccessfully ->
            let newVersion: PictureVersion =
                { VersionNumber = versionNumber
                  DemandReferences = cmd.DemandReferences
                  SupplyReferences = cmd.SupplyReferences
                  InventoryReferences = cmd.InventoryReferences
                  CompositionTime = cmd.CompositionTime
                  PublicationTime = None
                  LifecycleState = Draft }

            let events = [ PictureVersionComposed(cmd.PlanningScopeId, newVersion) ]
            let evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Capabilities.enterprisePictureManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Succeeded"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Composed Enterprise Picture Version %A" versionNumber
                      Evidence = evidence
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ SemanticObjects.enterprisePicture.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.validation("EnterprisePicture state must exist after applying events"))

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// AB-C-019b: Publish Enterprise Picture Version.
/// Transitions a Draft version to Published, superseding any existing Published version.
let publish
    (policy: EnterprisePicturePolicy)
    (cmd: PublishPictureVersionCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =
    result {

        let input: PublishInput = { Cmd = cmd; CurrentState = state }

        let! decision = decidePublication Rules.publicationRules input

        match decision.Outcome with
        | PublicationRejected reasons -> return! Error(DomainError.rule((String.concat "; " reasons), "DE-C-019b"))

        | PublishedSuccessfully ->
            let events =
                let supersedeEvents =
                    match findPublishedVersion state with
                    | Some existingPublished ->
                        [ PictureVersionSuperseded(cmd.PlanningScopeId, existingPublished.VersionNumber) ]
                    | None -> []

                supersedeEvents
                @ [ PictureVersionPublished(cmd.PlanningScopeId, cmd.VersionNumber, cmd.PublicationTime) ]

            let evidence = decision.Evaluations |> List.collect(fun e -> e.Evidence)

            let trace: DecisionTrace =
                { DecisionId = System.Guid.NewGuid().ToString()
                  CapabilityId = Capabilities.enterprisePictureManagement.Id
                  CausalDecisionIds = []
                  Outcome = "Succeeded"
                  PolicyId = Some policy.PolicyId
                  PolicyVersion = Some policy.Version
                  Rationale =
                    { Summary = sprintf "Published Enterprise Picture Version %A" cmd.VersionNumber
                      Evidence = evidence
                      Alternatives = [] }
                  RulesEvaluated = decision.Evaluations
                  SemanticObjectIds = [ SemanticObjects.enterprisePicture.Id ] }

            let! newState =
                events
                |> List.fold evolve state
                |> Result.ofOption(DomainError.validation("EnterprisePicture state must exist after applying events"))

            return
                { NewState = newState
                  Events = events
                  Trace = Some trace }
    }

/// Unified decide function for the Execution Engine.
/// Routes commands to the appropriate behavior.
let decide
    (policy: EnterprisePicturePolicy)
    (cmd: EnterprisePictureCmd)
    (state: EnterprisePicture option)
    : Result<Decision<EnterprisePicture, EnterprisePictureEvent>, DomainError> =

    match cmd with
    | Compose composeCmd -> compose policy composeCmd state
    | Publish publishCmd -> publish policy publishCmd state
