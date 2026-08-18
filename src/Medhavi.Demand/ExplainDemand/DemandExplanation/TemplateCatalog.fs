/// Governed Explanation Template Catalog
/// Maps (TemplateVersion, ArtifactType) to canonical template structures, required evidence, and NLG templates
module Medhavi.Demand.ExplainDemand.DemandExplanation.TemplateCatalog

/// Governed Explanation Template Specification
type ExplanationTemplate =
    { TemplateId: string
      Version: string
      ArtifactType: string
      RequiredEvidenceTypes: string list
      RequiredDecisionTypes: string list
      RelationshipMapping: Map<string * string, string>
      SummaryTemplate: string }

/// Canonical catalog of enterprise explanation templates
let private catalog: Map<string * string, ExplanationTemplate> =
    [ // 1. SE-D-002 Demand Understanding
      ("v1.0", "DemandUnderstanding"),
      { TemplateId = "TPL-DU-v1.0"
        Version = "v1.0"
        ArtifactType = "DemandUnderstanding"
        RequiredEvidenceTypes = [ "DemandObservation"; "HistoricalDemandSeries"; "EnterprisePicture" ]
        RequiredDecisionTypes = [ "DE-D-002" ]
        RelationshipMapping =
          Map.ofList
              [ ("DemandObservation", "DemandUnderstanding"), "ContributedToRevision"
                ("EnterprisePicture", "DemandUnderstanding"), "DerivedFrom"
                ("HistoricalDemandSeries", "DemandUnderstanding"), "InfluencedBy" ]
        SummaryTemplate =
          "Demand Understanding for {ScopeId}: Health is {HealthIndex}, Volatility is {VolatilityScore}. Continuity state: {ContinuityState} with {PatternClassification} pattern. {EvidenceSummary}" }

      // 2. SE-D-003 Forecast Publication
      ("v1.0", "ForecastPublication"),
      { TemplateId = "TPL-FP-v1.0"
        Version = "v1.0"
        ArtifactType = "ForecastPublication"
        RequiredEvidenceTypes = [ "HistoricalDemandSeries"; "ModelEvaluation"; "ChampionSelection" ]
        RequiredDecisionTypes = [ "DE-D-003"; "DE-D-004"; "DE-D-013" ]
        RelationshipMapping =
          Map.ofList
              [ ("HistoricalDemandSeries", "ForecastPublication"), "DerivedFrom"
                ("ChampionSelection", "ForecastPublication"), "DeterminedBy"
                ("ModelEvaluation", "ForecastPublication"), "EvaluatedBy"
                ("PlannerOverride", "ForecastPublication"), "AdjustedBy" ]
        SummaryTemplate =
          "Forecast Publication for {ScopeId} over {HorizonDays} days ({TotalForecastQuantity} units). Champion model: {ChampionModel} (governed by {PolicyId}). Key drivers: Baseline {BaselineQuantity}, Seasonality {SeasonalityImpact}, Promotional Lift {PromotionLift}, Planner Overrides {OverrideDelta}." }

      // 3. SE-D-004 Demand Behavior Assessment (Demand Sensing)
      ("v1.0", "DemandBehaviorAssessment"),
      { TemplateId = "TPL-DBA-v1.0"
        Version = "v1.0"
        ArtifactType = "DemandBehaviorAssessment"
        RequiredEvidenceTypes = [ "DemandSignal"; "BaselineDemand"; "CorroboratingSignal" ]
        RequiredDecisionTypes = [ "DE-D-006" ]
        RelationshipMapping =
          Map.ofList
              [ ("DemandSignal", "DemandBehaviorAssessment"), "TriggeredBy"
                ("BaselineDemand", "DemandBehaviorAssessment"), "ComparedAgainst"
                ("CorroboratingSignal", "DemandBehaviorAssessment"), "CorroboratedBy" ]
        SummaryTemplate =
          "Demand Behavior for {EntityId} transitioned to {BehaviorState} (Deviation: {DeviationSigma}σ). Triggered by signal from {SourceSystem}, corroborated by {CorroboratingSources}." }

      // 4. SE-D-005 Planning Classification Assignment
      ("v1.0", "PlanningClassificationAssignment"),
      { TemplateId = "TPL-PCA-v1.0"
        Version = "v1.0"
        ArtifactType = "PlanningClassificationAssignment"
        RequiredEvidenceTypes = [ "RevenueData"; "DemandVolatilityData" ]
        RequiredDecisionTypes = [ "DE-D-008" ]
        RelationshipMapping =
          Map.ofList
              [ ("RevenueData", "PlanningClassificationAssignment"), "DerivedFrom"
                ("DemandVolatilityData", "PlanningClassificationAssignment"), "InfluencedBy" ]
        SummaryTemplate =
          "Planning Classification for {EntityId} assigned class {ClassificationValue} under scheme {Scheme}. Revenue contribution is {RevenuePercentile}%; Demand variability CV is {CvValue}." }

      // 5. SE-D-006 Demand Behavior Assignment (Statistical Classification)
      ("v1.0", "DemandBehaviorAssignment"),
      { TemplateId = "TPL-DBA-STAT-v1.0"
        Version = "v1.0"
        ArtifactType = "DemandBehaviorAssignment"
        RequiredEvidenceTypes = [ "IntermittentDemandHistory"; "AutocorrelationData" ]
        RequiredDecisionTypes = [ "DE-D-009" ]
        RelationshipMapping =
          Map.ofList
              [ ("IntermittentDemandHistory", "DemandBehaviorAssignment"), "DerivedFrom"
                ("AutocorrelationData", "DemandBehaviorAssignment"), "EvaluatedBy" ]
        SummaryTemplate =
          "Behavior Classification for {EntityId} ({Dimension}) assigned {ClassificationValue}. ADI = {AdiValue} (cutoff {AdiCutoff}), CV² = {Cv2Value} (cutoff {Cv2Cutoff}), Seasonal lag autocorrelation p = {AutocorrelationPValue}." }

      // 6. SE-D-007 Planning Priority Assignment
      ("v1.0", "PlanningPriorityAssignment"),
      { TemplateId = "TPL-PPA-v1.0"
        Version = "v1.0"
        ArtifactType = "PlanningPriorityAssignment"
        RequiredEvidenceTypes = [ "RevenueContribution"; "StrategicImportance"; "ContractualSla"; "RiskExposure" ]
        RequiredDecisionTypes = [ "DE-D-010" ]
        RelationshipMapping =
          Map.ofList
              [ ("RevenueContribution", "PlanningPriorityAssignment"), "InfluencedBy"
                ("StrategicImportance", "PlanningPriorityAssignment"), "InfluencedBy"
                ("ContractualSla", "PlanningPriorityAssignment"), "ConstrainedBy"
                ("RiskExposure", "PlanningPriorityAssignment"), "InfluencedBy" ]
        SummaryTemplate =
          "Planning Priority for {EntityId} assigned {PriorityLevel} with Score {PriorityScore}/100. Business drivers: Revenue Score {RevenueScore}, Strategic Score {StrategicScore}, Contractual SLA {ContractualScore}, Risk Score {RiskScore}." }

      // 7. SE-D-008 Forecast Quality Assessment
      ("v1.0", "ForecastQualityAssessment"),
      { TemplateId = "TPL-FQA-v1.0"
        Version = "v1.0"
        ArtifactType = "ForecastQualityAssessment"
        RequiredEvidenceTypes = [ "PublishedForecast"; "MaterializedActuals"; "MeasurementPolicy" ]
        RequiredDecisionTypes = [ "DE-D-011" ]
        RelationshipMapping =
          Map.ofList
              [ ("PublishedForecast", "ForecastQualityAssessment"), "ComparedAgainst"
                ("MaterializedActuals", "ForecastQualityAssessment"), "DerivedFrom"
                ("MeasurementPolicy", "ForecastQualityAssessment"), "GovernedBy" ]
        SummaryTemplate =
          "Forecast Quality for {ScopeId} rated {OverallQualityState}. WAPE = {Wape}%, Forecast Bias = {ForecastBias}%, Tracking Signal = {TrackingSignal} (Out of Control: {IsOutOfControl}), Forecast Value Add FVA = {Fva}%." }

      // 8. SE-D-009 Demand Exception Evidence
      ("v1.0", "DemandExceptionEvidence"),
      { TemplateId = "TPL-DEE-v1.0"
        Version = "v1.0"
        ArtifactType = "DemandExceptionEvidence"
        RequiredEvidenceTypes = [ "TriggeringMetric"; "DetectionPolicy"; "HistoricalMetricSeries" ]
        RequiredDecisionTypes = [ "DE-D-012" ]
        RelationshipMapping =
          Map.ofList
              [ ("TriggeringMetric", "DemandExceptionEvidence"), "TriggeredBy"
                ("DetectionPolicy", "DemandExceptionEvidence"), "GovernedBy"
                ("HistoricalMetricSeries", "DemandExceptionEvidence"), "ComparedAgainst" ]
        SummaryTemplate =
          "Demand Exception {ExceptionType} for {EntityId} ({ScopeId}) evaluated with {Severity} severity. Metric {TriggeringMetric} value {MetricValue} breached threshold {ThresholdValue} (Anomaly Z-Score: {ZScore}σ)." } ]
    |> Map.ofList

/// Look up template by version and artifact type, falling back to v1.0
let tryGetTemplate (templateVersion: string) (artifactType: string) : ExplanationTemplate option =
    match catalog |> Map.tryFind (templateVersion, artifactType) with
    | Some tpl -> Some tpl
    | None -> catalog |> Map.tryFind ("v1.0", artifactType)
