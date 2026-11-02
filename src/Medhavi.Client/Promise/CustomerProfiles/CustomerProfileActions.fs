module Medhavi.Client.Promise.CustomerProfiles.CustomerProfileActions

open System
open System.Threading.Tasks
open Medhavi.Contracts.Promise
open Medhavi.Client.ViewComposition.Identity
open Medhavi.Client.ActionExecution
open Medhavi.Client

let configureProfile: ActionHandler =
    fun ctx ->
        task {
            let api = ctx.Domain.Promise.UnderstandOrders.CustomerProfiles
            let req: ConfigureCustomerPromiseProfileReq =
                { CustomerId = $"CUST-NEW-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1000L}"
                  CustomerTier = "Gold"
                  CreditLimit = 250000.0m
                  CreditHoldActive = false
                  DefaultSlaDays = 5
                  AllowSplitShipments = true
                  MaxSplitShipmentPackages = 3
                  AllowProductSubstitution = true
                  SubstitutionMarginFloorPct = 0.20m
                  DailyPenaltyRatePct = 0.005m
                  MaxPenaltyCapPct = 0.15m
                  EffectiveFrom = DateTimeOffset.UtcNow
                  EffectiveTo = None }

            match! api.Configure req with
            | Ok dto ->
                return
                    Ok(
                        ActionExecutionOutcome.Completed(
                            $"Customer profile '{dto.CustomerId}' (Tier {dto.CustomerTier}) configured successfully.",
                            [ CustomerProfileGrid.storeId ]
                        )
                    )
            | Error err -> return Error err.Message
        }

let updateTerms: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some custId ->
                let api = ctx.Domain.Promise.UnderstandOrders.CustomerProfiles
                let req: UpdateCustomerServiceTermsReq =
                    { CustomerId = custId
                      DefaultSlaDays = Some 3
                      AllowSplitShipments = Some true
                      AllowProductSubstitution = Some true
                      DailyPenaltyRatePct = Some 0.004m
                      CreditHoldActive = None
                      UpdateTime = DateTimeOffset.UtcNow }

                match! api.UpdateTerms req with
                | Ok dto ->
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Customer terms updated for '{dto.CustomerId}'. SLA: {dto.DefaultSlaDays} days.",
                                [ CustomerProfileGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No customer profile selected."
        }

let toggleHold: ActionHandler =
    fun ctx ->
        task {
            match ctx.TargetKey with
            | Some custId ->
                let api = ctx.Domain.Promise.UnderstandOrders.CustomerProfiles
                let req: UpdateCustomerServiceTermsReq =
                    { CustomerId = custId
                      DefaultSlaDays = None
                      AllowSplitShipments = None
                      AllowProductSubstitution = None
                      DailyPenaltyRatePct = None
                      CreditHoldActive = Some true
                      UpdateTime = DateTimeOffset.UtcNow }

                match! api.UpdateTerms req with
                | Ok dto ->
                    let state = if dto.CreditHoldActive then "ACTIVE" else "RELEASED"
                    return
                        Ok(
                            ActionExecutionOutcome.Completed(
                                $"Credit hold for customer '{dto.CustomerId}' set to {state}.",
                                [ CustomerProfileGrid.storeId ]
                            )
                        )
                | Error err -> return Error err.Message
            | None -> return Error "No customer profile selected."
        }

let all: (ActionDefId * ActionHandler) list =
    [ CustomerProfileGrid.CustomerProfileActions.configure.Id, configureProfile
      CustomerProfileGrid.CustomerProfileActions.updateTerms.Id, updateTerms
      CustomerProfileGrid.CustomerProfileActions.toggleHold.Id, toggleHold ]

