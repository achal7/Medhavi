namespace Medhavi.Web.Stores

open System
open System.Threading.Tasks
open Medhavi.Nexus
open Medhavi.Contracts.Promise

type PromiseStore = {
    EvaluatePromise : PromiseRequest -> Task<Result<PromiseEvaluationResponse, string>>
}

module PromiseStore =
    let create (engine: MedhaviEngine) : PromiseStore =
        let evaluatePromise (req: PromiseRequest) =
            engine.EvaluatePromise(req)

        { EvaluatePromise = evaluatePromise }
