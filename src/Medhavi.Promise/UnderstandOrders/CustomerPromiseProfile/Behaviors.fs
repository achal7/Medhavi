/// CA-P-001 Customer Promise Profile Aggregate Behaviors
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Behaviors

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Contracts.Decision
open Medhavi.Foundation.Failure
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Rules
open Decisions
open Policies

let buildTrace policy decisionId decisionOutcome state events summary =
    let policyId = policy |> Option.map (fun _ -> Policies.orderValidationPolicy.Id)
    let policyVersion = policy |> Option.map (fun _ -> 1)

    buildDecisionWithTrace
        evolve
        state
        events
        decisionId
        []
        Capabilities.captureAndValidateOrderBook.Id
        decisionOutcome
        policyId
        policyVersion
        [ SemanticObjects.customerPromiseProfile.Id ]
        (Some summary)

/// AB-P-017: Configure Customer Promise Profile
let configure
    (policy: CustomerProfilePolicy)
    (customerExists: bool)
    : Decide<CustomerPromiseProfile, ConfigureCustomerPromiseProfileCmd, CustomerProfileEvent> =
    fun (cmd: ConfigureCustomerPromiseProfileCmd) (state: CustomerPromiseProfile option) ->
        result {
            let input: ConfigureProfileInput =
                { Cmd = cmd
                  CustomerExists = customerExists
                  Policy = policy }

            let! decision = evaluateConfigureProfile configureRules input
            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | Validated ->
                let profile: CustomerPromiseProfile =
                    { CustomerId = cmd.CustomerId
                      CustomerTier = cmd.CustomerTier
                      CreditLimit = cmd.CreditLimit
                      CreditHoldActive = cmd.CreditHoldActive
                      DefaultSlaDays = cmd.DefaultSlaDays
                      AllowSplitShipments = cmd.AllowSplitShipments
                      MaxSplitShipmentPackages = cmd.MaxSplitShipmentPackages
                      AllowProductSubstitution = cmd.AllowProductSubstitution
                      SubstitutionMarginFloorPct = cmd.SubstitutionMarginFloorPct
                      DailyPenaltyRatePct = cmd.DailyPenaltyRatePct
                      MaxPenaltyCapPct = cmd.MaxPenaltyCapPct
                      EffectiveFrom = cmd.EffectiveFrom
                      EffectiveTo = cmd.EffectiveTo
                      LifecycleState = Active
                      DecisionTraceability = Some traceId }

                let events = [ CustomerPromiseProfileConfigured profile ]
                return buildTrace (Some policy) traceId decision state events "Customer promise profile configured"

            | ValidationFailed reasons ->
                return! Error(DomainError.validation (String.concat "; " reasons))
        }

/// AB-P-018: Update Customer Service Terms
let updateTerms
    (policy: CustomerProfilePolicy)
    : Decide<CustomerPromiseProfile, UpdateCustomerServiceTermsCmd, CustomerProfileEvent> =
    fun (cmd: UpdateCustomerServiceTermsCmd) (state: CustomerPromiseProfile option) ->
        result {
            let input: UpdateTermsInput =
                { Cmd = cmd
                  CurrentProfile = state
                  Policy = policy }

            let! decision = evaluateUpdateTerms updateRules input
            let traceId = Guid.NewGuid().ToString()

            match decision.Outcome with
            | UpdateAuthorized ->
                match state with
                | Some p ->
                    let updated =
                        { p with
                            DefaultSlaDays = cmd.DefaultSlaDays |> Option.defaultValue p.DefaultSlaDays
                            AllowSplitShipments = cmd.AllowSplitShipments |> Option.defaultValue p.AllowSplitShipments
                            AllowProductSubstitution = cmd.AllowProductSubstitution |> Option.defaultValue p.AllowProductSubstitution
                            DailyPenaltyRatePct = cmd.DailyPenaltyRatePct |> Option.defaultValue p.DailyPenaltyRatePct
                            CreditHoldActive = cmd.CreditHoldActive |> Option.defaultValue p.CreditHoldActive
                            DecisionTraceability = Some traceId }

                    let events = [ CustomerServiceTermsUpdated(cmd.CustomerId, "Terms updated", cmd.UpdateTime, traceId) ]
                    return buildTrace (Some policy) traceId decision (Some updated) events "Customer service terms updated"
                | None ->
                    return! Error(DomainError.notFound (SemanticObjects.customerPromiseProfile.Id, CustomerId.value cmd.CustomerId))

            | UpdateRejected reason ->
                return! Error(DomainError.rule (reason, rule = Rules.customerPromiseProfileIdentity.Id))
        }
