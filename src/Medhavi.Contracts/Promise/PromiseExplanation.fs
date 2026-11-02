namespace Medhavi.Contracts.Promise

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// Causal lineage step in the explanation graph
type LineageNodeDto =
    { StepIndex: int
      StageName: string
      EntityReference: string
      ActionTaken: string
      EvidenceSummary: string }

/// SE-P-090 Promise Explanation Data Transfer Object
type PromiseExplanationDto =
    { ExplanationId: string
      CommitmentId: string
      OrderLineId: string
      SummaryRationale: string
      EvaluatedRulesCount: int
      BindingConstraints: string list
      ConsideredAlternativesCount: int
      CausalLineage: LineageNodeDto list
      LineageDigestHash: string
      GeneratedAt: DateTimeOffset }

/// External request to generate an immutable explainability tree for a promise decision
type GeneratePromiseExplanationReq =
    { ExplanationId: string
      CommitmentId: string
      OrderLineId: string
      GenerationTime: DateTimeOffset }

// ---------- Business Notifications ----------

/// BN-P-019: Promise Decision Lineage Evidence Notification (Delivered to Compliance & Audit)
type PromiseDecisionLineageEvidenceNotification =
    { ExplanationId: string
      CommitmentId: string
      OrderLineId: string
      LineageDigestHash: string
      NotificationTime: DateTimeOffset }

/// Public API for Promise Explainability & Audit (CA-P-006)
type PromiseExplanationApi =
    { GenerateExplanation: GeneratePromiseExplanationReq -> Task<Result<PromiseExplanationDto, ApiError>> }

/// Query service alias for Promise Explanations
type PromiseExplanationQueries = QueryService<PromiseExplanationDto, string>

