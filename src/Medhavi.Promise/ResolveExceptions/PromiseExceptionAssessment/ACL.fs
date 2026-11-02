/// CA-P-009 Resolve Exceptions — ACL
module Medhavi.Promise.ResolveExceptions.PromiseExceptionAssessment.ACL

open Medhavi.Common
open Medhavi.Common.Validation
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model

let parseDisruptionType (s: string) : PromiseDisruptionType =
    match (if isNull s then "" else s.Trim().ToLowerInvariant()) with
    | "capacity" | "workcenter" | "workcentercapacitybreakdown" -> PromiseDisruptionType.WorkCenterCapacityBreakdown
    | "transit" | "logistics" | "logisticstransitbottleneck" -> PromiseDisruptionType.LogisticsTransitBottleneck
    | "credit" | "customercommercialhold" -> PromiseDisruptionType.CustomerCommercialHold
    | "allocation" | "allocationpoolexhausted" -> PromiseDisruptionType.AllocationPoolExhausted
    | _ -> PromiseDisruptionType.InboundSupplyShortfall

let toRaiseCmd (req: RaisePromiseExceptionReq) : Validation<RaisePromiseExceptionCmd, DomainError> =
    let create excId commId lineId time =
        { ExceptionId = excId
          CommitmentId = commId
          OrderLineId = lineId
          DisruptionType = parseDisruptionType req.DisruptionType
          Description = req.Description
          IncidentTime = time }

    create <!> validatePromiseExceptionId req.ExceptionId
           <*> validateOrderCommitmentId req.CommitmentId
           <*> validateOrderLineId req.OrderLineId
           <*> validateTimestamp req.IncidentTime

let toResolveCmd (req: ResolvePromiseExceptionReq) : Validation<ResolvePromiseExceptionCmd, DomainError> =
    let create excId time =
        { ExceptionId = excId
          SelectedPlanId = req.SelectedPlanId
          AuthorizationTime = time }

    create <!> validatePromiseExceptionId req.ExceptionId
           <*> validateTimestamp req.AuthorizationTime

let toArsEventId = function
    | DisruptionCaseOpened _ -> ArsIdentifiers.EnterpriseEvents.disruptionCaseOpened.Id
    | DisruptionRecoveryExecuted _ -> ArsIdentifiers.EnterpriseEvents.disruptionRecoveryExecuted.Id

