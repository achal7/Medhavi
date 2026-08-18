// =============================================================================
// ExplainDemand — Ports
// =============================================================================
module Medhavi.Demand.ExplainDemand.Ports

open System.Threading.Tasks
open Medhavi.Demand.ExplainDemand.DemandExplanation.Model
open Medhavi.Foundation.Contracts.DecisionTrace

type GetDecisionTracesPort = string -> string -> Task<DecisionTrace list>
type GetSourceArtifactRefsPort = string -> string -> Task<ExplanationSourceArtifactRef list>
type GetCausalChainPort = string list -> Task<DecisionTrace list>

type ExplainDemandPorts =
    { GetDecisionTraces: GetDecisionTracesPort
      GetSourceArtifactRefs: GetSourceArtifactRefsPort
      GetCausalChain: GetCausalChainPort }
