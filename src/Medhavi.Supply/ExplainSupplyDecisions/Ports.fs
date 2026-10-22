// =============================================================================
// ExplainSupplyDecisions — Ports (Decoupled Cross-Capability Queries)
// =============================================================================
namespace Medhavi.Supply.ExplainSupplyDecisions

open System.Threading.Tasks
open Medhavi.Foundation.Contracts
open Medhavi.Supply.ExplainSupplyDecisions.SupplyExplanation.Model

type GetDecisionTracesPort = string -> string -> Task<DecisionTrace list>
type GetSourceArtifactRefsPort = string -> string -> Task<ExplanationSourceArtifactRef list>
type GetCausalChainPort = string list -> Task<DecisionTrace list>

type ExplainSupplyPorts =
    { GetDecisionTraces: GetDecisionTracesPort
      GetSourceArtifactRefs: GetSourceArtifactRefsPort
      GetCausalChain: GetCausalChainPort }
