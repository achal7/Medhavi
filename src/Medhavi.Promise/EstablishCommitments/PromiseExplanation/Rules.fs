/// CA-P-006 Establish Commitments — Promise Explanation Rules
module Medhavi.Promise.EstablishCommitments.PromiseExplanation.Rules

open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Promise.ArsIdentifiers
open Model

type GenerateExplanationInput =
    { Cmd: GeneratePromiseExplanationCmd
      CommitmentExists: bool }

let explanationPrerequisitesRule: Rule<GenerateExplanationInput> =
    Rule.create
        Rules.orderCommitmentIdentity.Id
        Rules.orderCommitmentIdentity.Explanation
        (fun input -> input.CommitmentExists)
        (fun input -> $"CommitmentExists: %b{input.CommitmentExists}")

let explanationRules: Rule<GenerateExplanationInput> list =
    [ explanationPrerequisitesRule ]

