/// CA-P-011 Learn & Calibrate Promising Models — Aggregate Capabilities
module Medhavi.Promise.LearnFromPromises.PromiseCalibrationModel.Capabilities

open System.Threading.Tasks
open Medhavi.Common
open Medhavi.Foundation.Contracts
open Medhavi.Foundation.Execution
open Medhavi.Foundation.Execution.CommandCapabilities
open Medhavi.Foundation.Failure
open Medhavi.Foundation.Observation
open Medhavi.SemanticModel
open Medhavi.Contracts.Promise
open Medhavi.Promise
open Model
open Policies

type PromiseCalibrationAggregateApi =
    { ProposeCalibration: ProposeCalibrationReq -> Task<Result<PromiseCalibrationModel, ApplicationError>>
      ApplyCalibration: ApplyCalibrationReq -> Task<Result<PromiseCalibrationModel, ApplicationError>> }

let create
    (repo: Repository<PromiseCalibrationModel, PromiseLearningId, PromiseCalibrationEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: PromisingModelCalibrationPolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<PromiseCalibrationEvent>)
    : PromiseCalibrationAggregateApi =

    let proposeCalibration (req: ProposeCalibrationReq) : Task<Result<PromiseCalibrationModel, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toProposeCmd req)
            let decideFn = Behaviors.propose policy 50
            let pipeline = CommandPipeline.create repo (fun (c: ProposeModelCalibrationCmd) -> c.ModelId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let applyCalibration (req: ApplyCalibrationReq) : Task<Result<PromiseCalibrationModel, ApplicationError>> =
        taskResult {
            let! cmd = liftValidation (ACL.toApplyCmd req)
            let decideFn = Behaviors.apply policy
            let pipeline = CommandPipeline.create repo (fun (c: ApplyModelCalibrationCmd) -> c.ModelId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { ProposeCalibration = proposeCalibration
      ApplyCalibration = applyCalibration }
