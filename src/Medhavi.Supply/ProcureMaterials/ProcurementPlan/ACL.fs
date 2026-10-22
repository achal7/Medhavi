/// Anti-Corruption Layer (ACL) for Procurement Plan Commands
/// Translates Medhavi.Contracts.Supply DTOs into pure domain commands using applicative validation
module Medhavi.Supply.ProcureMaterials.ProcurementPlan.ACL

open System
open Medhavi.Common.Validation
open Medhavi.Contracts.Supply
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Supply
open Model

let toPublishPlanCmd (req: PublishProcurementPlanReq) : Validation<PublishProcurementPlanCmd, DomainError> =
        let scopeV = validatePlanningScopeId req.PlanningScopeId
        let timestampV = validateTimestamp req.PublicationTimestamp

        let cycleV =
            if String.IsNullOrWhiteSpace req.PlanningCycleId then
                Invalid [DomainError.validation "PlanningCycleId cannot be empty"]
            else
                Valid (req.PlanningCycleId.Trim())

        let authV =
            if String.IsNullOrWhiteSpace req.PublicationAuthority then
                Invalid [DomainError.validation "PublicationAuthority cannot be empty"]
            else
                Valid (req.PublicationAuthority.Trim())

        (fun s c a t ->
            { PlanningScopeId = s
              PlanningCycleId = c
              PublicationAuthority = a
              PublicationTimestamp = t })
        <!> scopeV
        <*> cycleV
        <*> authV
        <*> timestampV
