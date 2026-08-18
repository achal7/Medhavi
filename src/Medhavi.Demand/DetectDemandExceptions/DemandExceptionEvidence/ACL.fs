/// Anti-Corruption Layer (ACL) for Demand Exception Evidence
/// Uses Applicative validation combinators and shared Helpers
module Medhavi.Demand.DetectDemandExceptions.DemandExceptionEvidence.ACL

open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Demand
open Medhavi.Demand
open Model
open Medhavi.Foundation.Validations

/// Translates EvaluateDemandExceptionReq into EvaluateDemandExceptionCmd
let toEvaluateCmd (req: EvaluateDemandExceptionReq) : Validation<EvaluateDemandExceptionCmd, DomainError> =
    let create (scope: PlanningScopeId) (entityType: string) (entityId: string) (excType: DemandExceptionType) =
        let id = DemandExceptionEvidenceId.ofComponents excType.AsString entityType entityId scope

        { EvidenceId = id
          Scope = scope
          PlanningEntityType = entityType
          PlanningEntityId = entityId
          ExceptionType = excType
          TriggeringMetric = req.TriggeringMetric
          MetricValue = req.MetricValue
          HistoricalValues = req.HistoricalValues |> Option.defaultValue []
          EvaluationTime = Timestamp.now() }

    create <!> validatePlanningScopeId req.ScopeId
    <*> required "EntityType" req.EntityType
    <*> required "EntityId" req.EntityId
    <*> (DemandExceptionType.FromString req.ExceptionType |> fromResult)
