module Medhavi.Demand.PlanningScope.Decisions

open Medhavi.SharedKernel
open Medhavi.SharedKernel.Contracts.Aggregate
open Medhavi.SharedKernel.Contracts.DecisionTrace
open Medhavi.SharedKernel.Failure
open Medhavi.Demand
open Medhavi.Demand.PlanningScope.Model

let determineScope
    (cmd: DeterminePlanningScopeCmd)
    (state: PlanningScope option)
    : Result<PlanningScopeEvent list, DomainError> =
    match state with
    | Some _ ->
        // Scope already exists; no change needed (idempotent)
        Ok []
    | None ->
        let id =
            PlanningScopeId.create(
                SkuId.value cmd.SkuId,
                StockingPointId.value cmd.StockingPointId,
                cmd.CustomerId |> Option.map CustomerId.value,
                cmd.PlanningPeriod
            )

        match id with
        | Error err -> Error err
        | Ok scopeId ->
            let scope: PlanningScope =
                { Id = scopeId
                  SkuId = cmd.SkuId
                  StockingPointId = cmd.StockingPointId
                  CustomerId = cmd.CustomerId
                  PlanningPeriod = cmd.PlanningPeriod
                  Status = Active
                  TransactionTime = Timestamp.now }

            Ok [ ScopeDetermined scope ]

let archiveScope (scope: PlanningScope) : Result<PlanningScopeEvent list, DomainError> =
    Rules.neverDeleted scope.Status |> Result.map(fun () -> [ ScopeArchived scope.Id ])

let decide: Decide<PlanningScope, PlanningScopeCommand, PlanningScopeEvent> =
    fun cmd stateOpt ->
        match cmd, stateOpt with
        | PlanningScopeCommand.Determine cmd, _ ->
            determineScope cmd stateOpt
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    stateOpt
                    events
                    (Some
                        { DecisionId = "" // no specific DE-ID for this; it's an internal AB
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                          RulesEvaluated =
                            [ (ArsIdentifiers.Demand.Rules.planningScopeIdentityUnique, 1)
                              (ArsIdentifiers.Demand.Rules.atMostOneActiveScope, 1) ]
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.planningScope ]
                          Rationale =
                            { Summary = "Planning scope determined"
                              Evidence = []
                              Alternatives = [] } }))
        | PlanningScopeCommand.Archive _, Some scope ->
            archiveScope scope
            |> Result.map(fun events ->
                buildDecision
                    evolve
                    (Some scope)
                    events
                    (Some
                        { DecisionId = ""
                          CapabilityId = ArsIdentifiers.Demand.Capabilities.understandDemand
                          RulesEvaluated = [ (ArsIdentifiers.Demand.Rules.historicalEdpsPermanently, 1) ]
                          PolicyId = None
                          PolicyVersion = None
                          SemanticObjectIds = [ ArsIdentifiers.Demand.SemanticObjects.planningScope ]
                          Rationale =
                            { Summary = "Planning scope archived"
                              Evidence = []
                              Alternatives = [] } }))
        | _ -> Error(DomainError.validation "Command invalid for current scope state")
