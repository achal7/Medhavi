/// CA-P-011 Learn & Calibrate Promising Models — Rules
/// Architectural Lineage: ARS §3, §14; Table A.5 BR-P-012, BR-P-028, BR-P-029, BR-P-068
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Rules

open System
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.SemanticModel
open Medhavi.Promise
open Medhavi.Promise.ArsIdentifiers
open Model
open Policies

type ProposeInput =
    { Cmd: ProposeModelCalibrationCmd
      HistoricalSampleSize: int
      Policy: PromisingModelCalibrationPolicy }

type ApplyInput =
    { Cmd: ApplyModelCalibrationCmd
      CurrentModel: PromiseCalibrationModel option
      Policy: PromisingModelCalibrationPolicy }

/// BR-P-012: Promise Calibration Model must be uniquely identified per scope and epoch
let modelIdentityRule: Rule<ProposeInput> =
    Rule.create
        Rules.promiseModelCalibrationRecordIdentity.Id
        Rules.promiseModelCalibrationRecordIdentity.Explanation
        (fun input -> not (String.IsNullOrWhiteSpace(PromiseLearningId.value input.Cmd.ModelId)))
        (fun input -> $"ModelId: %s{PromiseLearningId.value input.Cmd.ModelId}")

/// BR-P-068: Parameter calibration requires a minimum sample size of audited deliveries
let sampleSufficiencyRule: Rule<ProposeInput> =
    Rule.create
        Rules.calibrationSampleSufficiency.Id
        Rules.calibrationSampleSufficiency.Explanation
        (fun input -> input.HistoricalSampleSize >= input.Policy.MinimumObservationSampleSize)
        (fun input ->
            $"SampleSize: %d{input.HistoricalSampleSize}, MinimumRequired: %d{input.Policy.MinimumObservationSampleSize}")

let proposeRules: Rule<ProposeInput> list =
    [ modelIdentityRule; sampleSufficiencyRule ]

/// BR-P-028: Calibrated parameter adjustment must not exceed maximum step change percentage
let stepChangeRule: Rule<ApplyInput> =
    Rule.create
        Rules.calibrationDampeningGuardrails.Id
        Rules.calibrationDampeningGuardrails.Explanation
        (fun input ->
            match input.CurrentModel with
            | Some m ->
                m.Parameters
                |> List.forall (fun p ->
                    if p.BaselineValue = 0.0m then true
                    else abs (p.DampenedStepDelta / p.BaselineValue) <= input.Policy.MaxStepChangePercentage)
            | None -> false)
        (fun input ->
            match input.CurrentModel with
            | Some m -> $"ParametersCount: %d{m.Parameters.Length}"
            | None -> "ModelNotFound")

/// BR-P-029: Buffer parameter shifts exceeding threshold mandate human steward sign-off
let stewardSignOffRule: Rule<ApplyInput> =
    Rule.create
        Rules.stewardAuthorizationInvariant.Id
        Rules.stewardAuthorizationInvariant.Explanation
        (fun input ->
            match input.CurrentModel with
            | Some m ->
                let requiresSignOff =
                    m.Parameters
                    |> List.exists (fun p -> PromisingModelCalibrationPolicy.requiresStewardSignOff input.Policy (p.DampenedStepDelta / p.BaselineValue))
                if requiresSignOff then not (String.IsNullOrWhiteSpace input.Cmd.ApprovedBySteward)
                else true
            | None -> false)
        (fun input ->
            match input.CurrentModel with
            | Some _ -> $"ApprovedBy: %s{input.Cmd.ApprovedBySteward}"
            | None -> "ModelNotFound")

let applyRules: Rule<ApplyInput> list =
    [ stepChangeRule; stewardSignOffRule ]
