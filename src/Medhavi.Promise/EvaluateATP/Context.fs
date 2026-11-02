/// CA-P-004 Evaluate Multi-Option ATP Composition Root
module Medhavi.Promise.EvaluateATP.Context

open System
open System.Threading
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution.AggregateStages
open Medhavi.Foundation.Failure
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Medhavi.Promise.EvaluateATP.Model
open Medhavi.Promise.EvaluateATP.Policies
open Medhavi.Promise.EvaluateATP.Workflows
open Capabilities

type EvaluateATPContext =
    { Commands: EvaluateATPAggregateApi
      Dispose: unit -> unit }

let create
    (deps: EnvelopeStoreDependencies<PromiseEvaluationEvent>)
    (policy: MultiOptionPromisingPolicy)
    (ports: PromisePorts)
    (defaultScopeId: string)
    : TaskResult<EvaluateATPContext, ApplicationError> =
    taskResult {

        let aggApi = Capabilities.create policy ports

        let workflowDeps: EvaluateATPWorkflowDependencies =
            { AssessmentCodec = Medhavi.Foundation.Codec.json
              Subscribe = deps.Subscribe
              DispatchEnvelope = deps.DispatchEnvelope
              DefaultScopeId = defaultScopeId }

        let! workflowSub: IDisposable =
            createATPWorkflow workflowDeps CancellationToken.None

        let dispose () =
            workflowSub.Dispose ()

        return
            { Commands = aggApi
              Dispose = dispose }
    }

