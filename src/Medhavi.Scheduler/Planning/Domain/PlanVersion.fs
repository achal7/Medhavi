namespace Medhavi.Scheduler.Planning.Domain

open System
open Medhavi.Contracts.Scenario
open Medhavi.SharedKernel

type PlanVersion =
    {
        Id: PlanVersionId
        RunId: Guid
        ScenarioId: ScenarioId
        InputFingerprint: InputFingerprint
        KpiSummary: PlanKpiSummary
        /// Key pointing to PlanVersionData bulk output.
        StorageKey: string
        /// True while this version is the active plan for its scenario.
        IsActive: bool
        GeneratedAt: DateTimeOffset
    }

type PlanVersionCommand =
    | Publish of
        runId: Guid *
        scenarioId: ScenarioId *
        fingerprint: InputFingerprint *
        kpiSummary: PlanKpiSummary *
        storageKey: string *
        generatedAt: DateTimeOffset
    | MarkActive

type PlanVersionEvent =
    | PlanVersionPublished of
        id: PlanVersionId *
        runId: Guid *
        scenarioId: ScenarioId *
        fingerprint: InputFingerprint *
        kpiSummary: PlanKpiSummary *
        storageKey: string *
        generatedAt: DateTimeOffset
    | PlanVersionActivated of PlanVersionId

module PlanVersionAgg =
    let private errConflict msg = Error (DomainError.conflict msg)
    let private errInvariant msg = Error (DomainError.invariant msg)

    let handle: Decide<PlanVersion, PlanVersionCommand, PlanVersionEvent> =
        fun command stateOpt ->
            match command, stateOpt with
            | Publish(runId, scenarioId, fingerprint, kpiSummary, storageKey, generatedAt), None ->
                if String.IsNullOrWhiteSpace storageKey then
                    errInvariant "StorageKey cannot be empty — bulk plan data must be persisted first"
                else
                    let id = InputFingerprint.toPlanVersionId fingerprint

                    let version =
                        { Id = id
                          RunId = runId
                          ScenarioId = scenarioId
                          InputFingerprint = fingerprint
                          KpiSummary = kpiSummary
                          StorageKey = storageKey
                          IsActive = false
                          GeneratedAt = generatedAt }

                    Ok { NewState = version; Events = [ PlanVersionPublished(id, runId, scenarioId, fingerprint, kpiSummary, storageKey, generatedAt) ] }

            | Publish _, Some _ ->
                errConflict "PlanVersion already published — PlanVersionId must be unique per InputFingerprint"

            | MarkActive, Some state ->
                if state.IsActive then
                    Ok { NewState = state; Events = [] } // idempotent
                else
                    Ok { NewState = { state with IsActive = true }; Events = [ PlanVersionActivated state.Id ] }

            | MarkActive, None -> errInvariant "PlanVersion not found"

    let evolve (event: PlanVersionEvent) (stateOpt: PlanVersion option) : PlanVersion option =
        match event, stateOpt with
        | PlanVersionPublished(id, runId, scenarioId, fingerprint, kpiSummary, storageKey, generatedAt), None ->
            Some
                { Id = id
                  RunId = runId
                  ScenarioId = scenarioId
                  InputFingerprint = fingerprint
                  KpiSummary = kpiSummary
                  StorageKey = storageKey
                  IsActive = false
                  GeneratedAt = generatedAt }

        | PlanVersionActivated _, Some s -> Some { s with IsActive = true }
        | _, _ -> stateOpt
