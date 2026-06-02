namespace Medhavi.SharedKernel.PromisePolicy

open System

/// Time vs cost preference for planning and ATP decisions.
type TimeVsCost =
    | Fastest
    | Cheapest
    | Balanced

/// Risk basis for lead times/confidence.
type RiskBasis =
    | P50
    | P95
    | P50Plus of float

/// Policy applied to a promise evaluation.
/// This encodes the SLA/behavior for a promise run (per tenant/customer/SKU).
type PromisePolicy =
    {
        /// Time vs cost trade-off (fastest, cheapest, balanced).
        TimePreference: TimeVsCost
        /// Lead-time risk basis / confidence level used when selecting dates.
        RiskPreference: RiskBasis
        /// Optional absolute cap on total cost (material + production + transport + holding + penalties).
        CostCap: decimal option
        /// Optional minimum confidence requirement (0.0–1.0) for accepted plans.
        RiskCap: float option
        /// Require a single promise for the whole order (no partial acceptance).
        FullOrder: bool
        /// Require full delivery in a single promise/window (no split deliveries).
        FullDelivery: bool
        /// Allow consulting supplier ATP on shortfall according to policy.
        CallSupplierOnShortfall: bool
        /// Milliseconds budget for search/heuristics/solver before degrading.
        SearchBudgetMs: int
        /// Enable reservation subtraction in material availability calculations (light vs full mode).
        /// When true (full mode): subtract active reservations from availability.
        /// When false (light mode): ignore reservations, return full availability.
        EnableReservationSubtraction: bool
    }

/// Default policy presets
module PolicyPresets =

    /// Default policy (balanced, no caps, no supplier-on-shortfall).
    let defaultPolicy: PromisePolicy =
        { TimePreference = Balanced
          RiskPreference = P50
          CostCap = None
          RiskCap = None
          FullOrder = false
          FullDelivery = false
          CallSupplierOnShortfall = false
          SearchBudgetMs = 500
          EnableReservationSubtraction = true }

    /// Gold SLA preset:
    /// - Fastest delivery (time-first)
    /// - High confidence (P95)
    /// - Strict FullOrder + FullDelivery
    /// - Always try supplier on shortfall
    /// - Higher search budget and degraded-promise fallback
    let goldPreset =
        { defaultPolicy with
            TimePreference = Fastest
            RiskPreference = P95
            CostCap = None
            RiskCap = Some 0.95
            FullOrder = true
            FullDelivery = true
            CallSupplierOnShortfall = true
            SearchBudgetMs = 1_000
            EnableReservationSubtraction = true }

    /// Silver SLA preset:
    /// - Balanced time/cost
    /// - Slightly buffered risk (P50Plus)
    /// - FullOrder only, but partial deliveries allowed
    /// - Call supplier on shortfall
    /// - Medium search budget
    let silverPreset =
        { defaultPolicy with
            TimePreference = Balanced
            RiskPreference = P50Plus 0.15
            CostCap = None
            RiskCap = Some 0.85
            FullOrder = true
            FullDelivery = false
            CallSupplierOnShortfall = true
            SearchBudgetMs = 750 }

    /// Bronze SLA preset:
    /// - Cheapest-first behavior
    /// - P50 risk basis (no extra buffer)
    /// - No FullOrder/FullDelivery guarantees
    /// - Does not call supplier on shortfall by default
    /// - Lower search budget and quote-only fallback
    let bronzePreset =
        { defaultPolicy with
            TimePreference = Cheapest
            RiskPreference = P50
            CostCap = None
            RiskCap = None
            FullOrder = false
            FullDelivery = false
            CallSupplierOnShortfall = false
            SearchBudgetMs = 400
            EnableReservationSubtraction = false }

/// Helper functions for policy management
module PolicyHelpers =

    /// Catalog of presets keyed by tier name (case-insensitive keys preferred at call sites).
    let presetCatalog: Map<string, PromisePolicy> =
        Map
            [ "gold", PolicyPresets.goldPreset
              "silver", PolicyPresets.silverPreset
              "bronze", PolicyPresets.bronzePreset
              "default", PolicyPresets.defaultPolicy ]

    /// Apply a risk basis to pick a lead-time estimate (p50/p95/p50+buffer).
    let applyRiskBasis (basis: RiskBasis) (p50: TimeSpan) (p95: TimeSpan option) =
        match basis, p95 with
        | P95, Some v -> v
        | P50Plus b, _ ->
            let factor = 1.0 + b
            TimeSpan.FromTicks(int64 (float p50.Ticks * factor))
        | _ -> p50

    /// Merge a preset (e.g., SLA tier) onto an existing policy.
    /// Preset fields override or extend the base policy according to sensible rules.
    let mergePolicy (preset: PromisePolicy) (basePol: PromisePolicy) : PromisePolicy =
        { basePol with
            TimePreference = preset.TimePreference
            RiskPreference = preset.RiskPreference
            CostCap = preset.CostCap |> Option.orElse basePol.CostCap
            RiskCap = preset.RiskCap |> Option.orElse basePol.RiskCap
            FullOrder = preset.FullOrder || basePol.FullOrder
            FullDelivery = preset.FullDelivery || basePol.FullDelivery
            CallSupplierOnShortfall =
                preset.CallSupplierOnShortfall
                || basePol.CallSupplierOnShortfall
            SearchBudgetMs = max basePol.SearchBudgetMs preset.SearchBudgetMs }

    /// Resolve a policy by tier identifiers.
    /// Precedence: skuTier > customerTier > tenantTier > "default".
    let resolvePolicy
        (presets: Map<string, PromisePolicy>)
        (tenantTier: string option)
        (customerTier: string option)
        (skuTier: string option)
        (basePol: PromisePolicy)
        =
        let pick (tierKey: string option) =
            tierKey
            |> Option.bind (fun k -> presets |> Map.tryFind (k.ToLowerInvariant()))

        let resolved =
            pick skuTier
            |> Option.orElseWith (fun () -> pick customerTier)
            |> Option.orElseWith (fun () -> pick tenantTier)
            |> Option.defaultValue PolicyPresets.defaultPolicy

        mergePolicy resolved basePol

    /// Convenience helper: resolve policy from tiers using the default preset catalog.
    let resolveFromTiers tenantTier customerTier skuTier basePol =
        resolvePolicy presetCatalog tenantTier customerTier skuTier basePol

    /// Validate a policy and return a list of problems (empty list = valid).
    let validatePolicy (pol: PromisePolicy) : string list =
        [ if pol.SearchBudgetMs <= 0 then
              yield "SearchBudgetMs must be positive."

          match pol.RiskCap with
          | Some rc when rc < 0.0 || rc > 1.0 -> yield "RiskCap must be between 0.0 and 1.0."
          | _ -> () ]