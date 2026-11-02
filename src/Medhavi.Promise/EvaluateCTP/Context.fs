/// CA-P-005 Evaluate Capable-to-Promise Composition Root
module Medhavi.Promise.EvaluateCTP.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.EvaluateCTP.Model
open Medhavi.Promise.EvaluateCTP.Policies
open Medhavi.Promise.EvaluateCTP.Workflows
open Capabilities

type EvaluateCTPContext =
    { Commands: EvaluateCTPAggregateApi
      Dispose: unit -> unit }

let create
    (deps: EnvelopeStoreDependencies<CTPEvent>)
    (policy: CapableToPromisePolicy)
    (atpPolicy: EvaluateATP.Policies.MultiOptionPromisingPolicy)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<EvaluateCTPContext, ApplicationError> =
    taskResult {

        let aggApi = Capabilities.create policy atpPolicy ports

        let workflowDeps: EvaluateCTPWorkflowDependencies =
            { AssessmentCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              DefaultScopeId = defaultScopeId }

        let! workflowSub: IDisposable =
            createCTPWorkflow workflowDeps CancellationToken.None

        let dispose () =
            workflowSub.Dispose ()

        return
            { Commands = aggApi
              Dispose = dispose }
    }

