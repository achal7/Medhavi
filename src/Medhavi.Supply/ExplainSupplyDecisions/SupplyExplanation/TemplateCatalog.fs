/// Governed Explanation Template Catalog (PO-S-130)
/// Maps (TemplateVersion, ArtifactType) to canonical template structures, required evidence, and NLG templates
module Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.TemplateCatalog

/// Governed Explanation Template Specification
type ExplanationTemplate =
    {
        TemplateId: string
        Version: string
        ArtifactType: string
        RequiredEvidenceTypes: string list
        RequiredDecisionTypes: string list
        RelationshipMapping: Map<string * string, string>
        SummaryTemplate: string
    }

/// Canonical catalog of enterprise supply explanation templates
let private catalog: Map<string * string, ExplanationTemplate> =
    [ // 1. SE-S-001 Supply Understanding
        ("v1.0", "SupplyUnderstanding"),
        {
            TemplateId = "TPL-SU-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplyUnderstanding"
            RequiredEvidenceTypes = [ "SupplyDataRecord"; "EnterprisePicture"; "SupplyChangeAssessment" ]
            RequiredDecisionTypes = [ "DE-S-001"; "DE-S-002" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("SupplyDataRecord", "SupplyUnderstanding"), "ContributedToRevision"
                        ("EnterprisePicture", "SupplyUnderstanding"), "DerivedFrom"
                        ("SupplyChangeAssessment", "SupplyUnderstanding"), "InfluencedBy"
                    ]
            SummaryTemplate =
                "Supply Understanding for Scope '{ScopeId}' (v{Version}): Continuity posture is {ContinuityStatus}, Capacity Adequacy is {CapacityAdequacyStatus}, and Inventory Position is {InventoryPositionStatus}. Confidence is evaluated as {ConfidenceLevel} based on {EvidenceSummary}."
        }

        // 2. SE-S-020 Supply Plan
        ("v1.0", "SupplyPlan"),
        {
            TemplateId = "TPL-SP-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplyPlan"
            RequiredEvidenceTypes = [ "SupplyUnderstanding"; "DemandForecast"; "BomComponent"; "ResourceCapacity" ]
            RequiredDecisionTypes = [ "DE-S-011"; "DE-S-012" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("DemandForecast", "SupplyPlan"), "DeterminedBy"
                        ("SupplyUnderstanding", "SupplyPlan"), "ConstrainedBy"
                        ("ResourceCapacity", "SupplyPlan"), "ConstrainedBy"
                        ("BomComponent", "SupplyPlan"), "DerivedFrom"
                    ]
            SummaryTemplate =
                "Supply Plan for Scope '{ScopeId}' across Horizon '{HorizonDays}' days: Balanced total requirement of {TotalRequiredUnits} units against available capacity {AvailableCapacityHours} hrs. Bottlenecks identified: {Bottlenecks}. Publication authorized under stability score {StabilityScore}%."
        }

        // 3. SE-S-030 Inventory Position Assessment
        ("v1.0", "InventoryPosition"),
        {
            TemplateId = "TPL-INV-v1.0"
            Version = "v1.0"
            ArtifactType = "InventoryPosition"
            RequiredEvidenceTypes = [ "StockSnapshot"; "InventoryPolicyAssignment"; "DemandForecast" ]
            RequiredDecisionTypes = [ "DE-S-020"; "DE-S-021" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("StockSnapshot", "InventoryPosition"), "DerivedFrom"
                        ("InventoryPolicyAssignment", "InventoryPosition"), "GovernedBy"
                        ("DemandForecast", "InventoryPosition"), "ComparedAgainst"
                    ]
            SummaryTemplate =
                "Inventory Position for Item '{ItemId}' at Location '{LocationId}': Current on-hand is {OnHandQuantity} units against buffer target {TargetBuffer} units (Safety Stock: {SafetyStock}). Health State: {HealthState}. Stockout exposure: {StockoutExposure}."
        }

        // 4. SE-S-040 Capacity Position Assessment
        ("v1.0", "CapacityPosition"),
        {
            TemplateId = "TPL-CAP-v1.0"
            Version = "v1.0"
            ArtifactType = "CapacityPosition"
            RequiredEvidenceTypes = [ "ResourceShiftCalendar"; "PlannedLoad"; "MaintenanceWindow" ]
            RequiredDecisionTypes = [ "DE-S-030"; "DE-S-031" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("ResourceShiftCalendar", "CapacityPosition"), "DerivedFrom"
                        ("PlannedLoad", "CapacityPosition"), "ComparedAgainst"
                        ("MaintenanceWindow", "CapacityPosition"), "ConstrainedBy"
                    ]
            SummaryTemplate =
                "Capacity Position for Resource '{ResourceId}' over Period '{PeriodBucket}': Utilization is {UtilizationPercentage}% (Max Rated: {MaxRatedHours} hrs, Planned: {PlannedLoadHours} hrs). Health State: {CapacityHealthState} with bottleneck status '{BottleneckStatus}'."
        }

        // 5. SE-S-050 Supplier Commitment Assessment
        ("v1.0", "SupplierCommitment"),
        {
            TemplateId = "TPL-SUP-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplierCommitment"
            RequiredEvidenceTypes = [ "PurchaseOrderAcknowledgment"; "SupplierTelemetry"; "ContractualSla" ]
            RequiredDecisionTypes = [ "DE-S-040"; "DE-S-041" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("PurchaseOrderAcknowledgment", "SupplierCommitment"), "DeterminedBy"
                        ("SupplierTelemetry", "SupplierCommitment"), "InfluencedBy"
                        ("ContractualSla", "SupplierCommitment"), "GovernedBy"
                    ]
            SummaryTemplate =
                "Supplier Commitment for Supplier '{SupplierId}' on Item '{ItemId}': Confirmed commitment ratio is {CommitmentRatio}% against requested volume {RequestedVolume}. Sourcing risk: {SourcingRiskLevel}; Historical SLA compliance: {SlaComplianceRate}%."
        }

        // 6. SE-S-060 Procurement Recommendation
        ("v1.0", "ProcurementRecommendation"),
        {
            TemplateId = "TPL-PROC-v1.0"
            Version = "v1.0"
            ArtifactType = "ProcurementRecommendation"
            RequiredEvidenceTypes = [ "NetProcurementGap"; "SourcingAllocation"; "SupplierLeadTime" ]
            RequiredDecisionTypes = [ "DE-S-050"; "DE-S-051" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("NetProcurementGap", "ProcurementRecommendation"), "TriggeredBy"
                        ("SourcingAllocation", "ProcurementRecommendation"), "DeterminedBy"
                        ("SupplierLeadTime", "ProcurementRecommendation"), "ConstrainedBy"
                    ]
            SummaryTemplate =
                "Procurement Recommendation for Item '{ItemId}': Recommended order quantity of {OrderQuantity} units to Supplier '{SelectedSupplierId}' with required delivery by {NeedDate}. Supplier selected based on {SourcingRationale}; Lead-time constraint of {LeadTimeDays} days satisfied."
        }

        // 7. SE-S-070 Production Schedule
        ("v1.0", "ProductionSchedule"),
        {
            TemplateId = "TPL-SCHED-v1.0"
            Version = "v1.0"
            ArtifactType = "ProductionSchedule"
            RequiredEvidenceTypes = [ "SupplyPlan"; "MachineAvailability"; "ChangeoverMatrix" ]
            RequiredDecisionTypes = [ "DE-S-060"; "DE-S-061" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("SupplyPlan", "ProductionSchedule"), "DerivedFrom"
                        ("MachineAvailability", "ProductionSchedule"), "ConstrainedBy"
                        ("ChangeoverMatrix", "ProductionSchedule"), "AdjustedBy"
                    ]
            SummaryTemplate =
                "Production Schedule for Line '{ResourceId}' spanning {ScheduleStart} to {ScheduleEnd}: Sequenced {JobCount} production jobs with total changeover downtime of {SetupDowntimeHours} hrs. Schedule stability score: {StabilityScore}%."
        }

        // 8. SE-S-080 Distribution Plan
        ("v1.0", "DistributionPlan"),
        {
            TemplateId = "TPL-DIST-v1.0"
            Version = "v1.0"
            ArtifactType = "DistributionPlan"
            RequiredEvidenceTypes = [ "TransferRequirement"; "TransportationLane"; "DockCapacity" ]
            RequiredDecisionTypes = [ "DE-S-070"; "DE-S-071" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("TransferRequirement", "DistributionPlan"), "TriggeredBy"
                        ("TransportationLane", "DistributionPlan"), "DeterminedBy"
                        ("DockCapacity", "DistributionPlan"), "ConstrainedBy"
                    ]
            SummaryTemplate =
                "Distribution Plan for Lane '{OriginLocationId}' -> '{DestinationLocationId}': Replenishment transfer of {TransferQuantity} units dispatched. Transit SLA: {TransitDays} days; Dock utilization impact: {DockUtilization}%."
        }

        // 9. SE-S-002 Supply Change Assessment
        ("v1.0", "SupplyChangeAssessment"),
        {
            TemplateId = "TPL-SCA-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplyChangeAssessment"
            RequiredEvidenceTypes = [ "OperationalSignal"; "BaselineSupplyFact"; "CorroboratingSignal" ]
            RequiredDecisionTypes = [ "DE-S-080"; "DE-S-081" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("OperationalSignal", "SupplyChangeAssessment"), "TriggeredBy"
                        ("BaselineSupplyFact", "SupplyChangeAssessment"), "ComparedAgainst"
                        ("CorroboratingSignal", "SupplyChangeAssessment"), "CorroboratedBy"
                    ]
            SummaryTemplate =
                "Supply Change Assessment for Entity '{EntityId}' ({ChangeType}): Detected variance magnitude of {VarianceMagnitude} (Deviation: {DeviationSigma}σ). Corroborated by {CorroboratingCount} distinct operational sources; Materiality: {MaterialityStatus}."
        }

        // 10. SE-S-100 Supply Quality Assessment
        ("v1.0", "SupplyQualityAssessment"),
        {
            TemplateId = "TPL-SQA-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplyQualityAssessment"
            RequiredEvidenceTypes = [ "DeliveryPerformanceRecord"; "AdherenceScore"; "SupplierTelemetry" ]
            RequiredDecisionTypes = [ "DE-S-091"; "DE-S-092" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("DeliveryPerformanceRecord", "SupplyQualityAssessment"), "DerivedFrom"
                        ("AdherenceScore", "SupplyQualityAssessment"), "EvaluatedBy"
                        ("SupplierTelemetry", "SupplyQualityAssessment"), "InfluencedBy"
                    ]
            SummaryTemplate =
                "Supply Quality Assessment for Scope '{ScopeId}' (Evaluation Period '{PeriodId}'): Overall synthesis is {OverallQualityScore}% (Rating: {QualityRating}). Delivery Timeliness: {TimelinessScore}%, Supplier Adherence: {AdherenceScore}%, Plan Adherence: {PlanAdherenceScore}%. Publication authorized under PO-S-110."
        }

        // 11. SE-S-110 Supply Exception Assessment
        ("v1.0", "SupplyExceptionAssessment"),
        {
            TemplateId = "TPL-EXC-v1.0"
            Version = "v1.0"
            ArtifactType = "SupplyExceptionAssessment"
            RequiredEvidenceTypes = [ "ExceptionTriggerSignal"; "CorrelatedDisruption"; "PolicyThreshold" ]
            RequiredDecisionTypes = [ "DE-S-100"; "DE-S-101" ]
            RelationshipMapping =
                Map.ofList
                    [
                        ("ExceptionTriggerSignal", "SupplyExceptionAssessment"), "TriggeredBy"
                        ("PolicyThreshold", "SupplyExceptionAssessment"), "GovernedBy"
                        ("CorrelatedDisruption", "SupplyExceptionAssessment"), "InfluencedBy"
                    ]
            SummaryTemplate =
                "Supply Exception Assessment '{ExceptionId}' ({ExceptionType}): Severity classified as {SeverityLevel} across Horizon '{TemporalHorizon}'. Policy violation confirmed against threshold {ThresholdViolated}. Enterprise escalation status: {EscalationStatus}."
        }
    ]
    |> Map.ofList

/// Resolves an explanation template by version and artifact type
let tryGetTemplate (version: string) (artifactType: string) : ExplanationTemplate option =
    catalog |> Map.tryFind(version, artifactType)
