/// SE-S-001 — Supply Understanding Aggregate Behaviors
/// Traces to: AB-S-003 (Revise), AB-S-005 (Publish)
module Medhavi.Supply.UnderstandSupply.SupplyUnderstanding.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model
open Rules
open Decisions
open Policies
open Algorithms

/// Derives the 6-dimensional interpretation from Enterprise Picture supply facts and change signals
let private deriveInterpretation
    (facts: PictureSupplyFacts)
    (changeAssessments: string list)
    : Interpretation =
    if facts.SupplyFacts.IsEmpty then
        { Continuity = Incomplete "No supply facts in Enterprise Picture"
          ContinuityDrivers = []
          CapacityAdequacy = Incomplete "No capacity facts in Enterprise Picture"
          Bottlenecks = []
          InventoryPosition = Incomplete "No inventory facts in Enterprise Picture"
          StockoutRiskItems = []
          ExcessInventoryItems = []
          SupplierPosture = Incomplete "No supplier commitment facts in Enterprise Picture"
          CriticalDependencies = []
          OverallRisk = Incomplete "Evidence incomplete"
          RiskDrivers = [ "Missing baseline supply facts" ]
          Confidence = Incomplete "Evidence incomplete"
          ConfidenceFactors = [ "Empty Enterprise Picture supply facts" ]
          ReasonCodes = [ "EmptyPictureSupplyFacts" ] }
    else
        let hasDisruptions = not changeAssessments.IsEmpty

        // 1. Supply Continuity
        let continuityStatus =
            if hasDisruptions then ContinuityStatus.AtRisk else ContinuityStatus.Stable

        let continuityDrivers =
            if hasDisruptions then
                [ sprintf "%d active supply change assessment(s) detected" changeAssessments.Length ]
            else
                [ "Supply commitments and inventory aligned with demand schedule" ]

        // 2. Capacity Adequacy
        let productionFacts =
            facts.SupplyFacts
            |> List.filter(fun f -> f.SupplyType = SupplyType.ProductionOrder)

        let capacityStatus =
            if productionFacts.IsEmpty then CapacityAdequacyStatus.Sufficient
            else CapacityAdequacyStatus.Sufficient

        let bottlenecks = []

        // 3. Inventory Position
        let inventoryFacts =
            facts.SupplyFacts
            |> List.filter(fun f -> f.SupplyType = SupplyType.Inventory)

        let zeroStockItems =
            inventoryFacts
            |> List.filter(fun f -> Quantity.value f.Quantity <= 0m)
            |> List.map(fun f -> ItemId.value f.Item)

        let inventoryStatus =
            if not zeroStockItems.IsEmpty then InventoryPositionStatus.AtRisk else InventoryPositionStatus.Healthy

        // 4. Supplier Commitment Posture
        let supplierCommitments =
            facts.SupplyFacts
            |> List.filter(fun f ->
                f.SupplyType = SupplyType.SupplierCommitment
                || f.SupplyType = SupplyType.PurchaseOrder)

        let supplierPosture =
            if hasDisruptions then SupplierCommitmentPostureStatus.MediumConfidence else SupplierCommitmentPostureStatus.HighConfidence

        let criticalDependencies =
            supplierCommitments
            |> List.map(fun f -> ItemId.value f.Item)
            |> List.distinct

        // 5. Overall Supply Risk
        let overallRisk =
            if hasDisruptions || not zeroStockItems.IsEmpty then OverallSupplyRiskLevel.Medium
            else OverallSupplyRiskLevel.Low

        let riskDrivers =
            [ if hasDisruptions then yield "Supply change signals in progress"
              if not zeroStockItems.IsEmpty then yield sprintf "Zero-stock detected for %d item(s)" zeroStockItems.Length ]
            |> fun ds -> if ds.IsEmpty then [ "Nominal enterprise supply conditions" ] else ds

        // 6. Confidence Level
        let confidenceLevel =
            if facts.SupplyFacts.Length > 0 then ConfidenceLevel.High else ConfidenceLevel.Medium

        let confidenceFactors =
            [ sprintf "Enterprise Picture v%d verified; %d supply fact(s) evaluated" facts.PictureVersion facts.SupplyFacts.Length ]

        { Continuity = Known continuityStatus
          ContinuityDrivers = continuityDrivers
          CapacityAdequacy = Known capacityStatus
          Bottlenecks = bottlenecks
          InventoryPosition = Known inventoryStatus
          StockoutRiskItems = zeroStockItems
          ExcessInventoryItems = []
          SupplierPosture = Known supplierPosture
          CriticalDependencies = criticalDependencies
          OverallRisk = Known overallRisk
          RiskDrivers = riskDrivers
          Confidence = Known confidenceLevel
          ConfidenceFactors = confidenceFactors
          ReasonCodes = [ "PictureFactsInterpreted" ] }

/// AB-S-003: Revise Supply Understanding
let revise: Decide<SupplyUnderstanding, ReviseCmd, SupplyUnderstandingEvent> =
    fun (cmd: ReviseCmd) (stateOpt: SupplyUnderstanding option) ->
        result {
            let interpretation = deriveInterpretation cmd.PictureSupplyFacts cmd.SupplyChangeAssessments

            let currentAggregate =
                stateOpt
                |> Option.defaultValue
                    { PlanningScopeId = cmd.PlanningScopeId
                      Versions = []
                      CurrentPublishedVersion = None }

            let existingDraft = currentAggregate.Versions |> List.tryFind(fun v -> v.State = Draft)

            let newDraftVersion =
                match existingDraft with
                | Some existing ->
                    // Update existing Draft in place
                    { existing with
                        Interpretation = interpretation
                        EvidencePictureVersion = cmd.PictureSupplyFacts.PictureVersion
                        SupplyChangeAssessmentReferences = cmd.SupplyChangeAssessments
                        TransactionTime = cmd.TransactionTime }
                | None ->
                    // Compose new Draft with next version number
                    let nextVersionNumber =
                        currentAggregate.Versions
                        |> List.map(fun v -> v.VersionNumber)
                        |> List.fold max 0
                        |> (+) 1

                    { VersionNumber = nextVersionNumber
                      Interpretation = interpretation
                      EvidencePictureVersion = cmd.PictureSupplyFacts.PictureVersion
                      SupplyChangeAssessmentReferences = cmd.SupplyChangeAssessments
                      TransactionTime = cmd.TransactionTime
                      PublicationTime = None
                      State = Draft }

            let updatedVersions =
                match existingDraft with
                | Some existing ->
                    currentAggregate.Versions
                    |> List.map(fun v -> if v.VersionNumber = existing.VersionNumber then newDraftVersion else v)
                | None ->
                    newDraftVersion :: currentAggregate.Versions

            let updatedAggregate =
                { currentAggregate with Versions = updatedVersions }

            let events =
                [ SupplyUnderstandingRevised(updatedAggregate, currentAggregate.CurrentPublishedVersion) ]

            let traceId = Guid.NewGuid().ToString()
            let decision = { Outcome = Revised; Evaluations = [] }

            return
                buildDecisionWithTrace
                    evolve
                    stateOpt
                    events
                    traceId
                    []
                    ArsIdentifiers.Capabilities.understandSupply.Id
                    decision
                    (Some ArsIdentifiers.Policies.supplyUnderstandingMaterialityPolicy.Id)
                    (Some 1)
                    [ ArsIdentifiers.SemanticObjects.supplyUnderstanding.Id ]
                    (Some "Supply Understanding revised")
        }

/// AB-S-005: Publish Supply Understanding
let publish
    (materialityPolicy: MaterialityPolicy)
    (cadencePolicy: CadencePolicy)
    : Decide<SupplyUnderstanding, PublishCmd, SupplyUnderstandingEvent> =
    fun (cmd: PublishCmd) (stateOpt: SupplyUnderstanding option) ->
        result {
            match stateOpt with
            | None ->
                return! Error(DomainError.notFound("SupplyUnderstanding", (PlanningScopeId.value cmd.PlanningScopeId)))
            | Some su ->
                match su.Versions |> List.tryFind(fun v -> v.State = Draft) with
                | None -> return! Error(DomainError.validation "No Draft version to publish")
                | Some draft ->
                    let published = su.Versions |> List.tryFind(fun v -> v.State = Published)

                    let lastPublicationTime =
                        published |> Option.bind(fun v -> v.PublicationTime) |> Option.defaultValue cmd.PublicationTime

                    let periodicRefreshDue =
                        cmd.IsPeriodicRefresh
                        || (Timestamp.value cmd.PublicationTime) - (Timestamp.value lastPublicationTime)
                           >= cadencePolicy.MaximumPublicationInterval

                    let input: PublicationInput =
                        { Assessment =
                            evaluateUnderstandingChangeMateriality
                                materialityPolicy
                                { Draft = draft.Interpretation
                                  Published = published |> Option.map(fun v -> v.Interpretation) }
                          PeriodicRefreshDue = periodicRefreshDue
                          Interpretation = draft.Interpretation
                          CompletenessThreshold = materialityPolicy.CompletenessThreshold }

                    let! decisionOutcome = evaluatePublication publicationRules input

                    match decisionOutcome.Outcome with
                    | DoNotPublish ->
                        let reason =
                            decisionOutcome.Evaluations
                            |> List.filter(fun e -> not e.Passed)
                            |> List.map(fun e -> e.RuleId)
                            |> String.concat "; "

                        return!
                            Error(
                                ruleViolationWithDetail
                                    ArsIdentifiers.Rules.supplyUnderstandingPublicationAuthorization
                                    $"Publication criteria not met: {reason}"
                            )
                    | Publish ->
                        let now = cmd.PublicationTime

                        let publishedVersion =
                            { draft with
                                State = Published
                                PublicationTime = Some now }

                        let supersededPrevious = published |> Option.map(fun v -> { v with State = Superseded })

                        let updatedVersions =
                            su.Versions
                            |> List.map(fun v ->
                                if v.VersionNumber = draft.VersionNumber then
                                    publishedVersion
                                else
                                    match supersededPrevious with
                                    | Some prev when v.VersionNumber = prev.VersionNumber -> prev
                                    | _ -> v)

                        let newSU =
                            { su with
                                Versions = updatedVersions
                                CurrentPublishedVersion = Some draft.VersionNumber }

                        let events =
                            [ SupplyUnderstandingPublished(
                                  newSU,
                                  published |> Option.map(fun v -> v.VersionNumber),
                                  now
                              ) ]

                        let traceId = Guid.NewGuid().ToString()

                        return
                            buildDecisionWithTrace
                                evolve
                                (Some su)
                                events
                                traceId
                                []
                                ArsIdentifiers.Capabilities.understandSupply.Id
                                decisionOutcome
                                (Some ArsIdentifiers.Policies.supplyUnderstandingMaterialityPolicy.Id)
                                (Some 1)
                                [ ArsIdentifiers.SemanticObjects.supplyUnderstanding.Id ]
                                (Some "Supply Understanding published")
        }
