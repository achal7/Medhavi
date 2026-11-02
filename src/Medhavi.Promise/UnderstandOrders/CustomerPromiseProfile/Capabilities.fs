/// CA-P-001 Customer Promise Profile Aggregate Capabilities
module Medhavi.Promise.UnderstandOrders.CustomerPromiseProfile.Capabilities

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

type CustomerPromiseProfileAggregateApi =
    { Configure: ConfigureCustomerPromiseProfileReq -> Task<Result<CustomerPromiseProfile, ApplicationError>>
      UpdateTerms: UpdateCustomerServiceTermsReq -> Task<Result<CustomerPromiseProfile, ApplicationError>> }

let create
    (repo: Repository<CustomerPromiseProfile, CustomerId, CustomerProfileEvent>)
    (publishKnowledge: KnowledgeRepresentation)
    (policy: CustomerProfilePolicy)
    (deps: AggregateStages.EnvelopeStoreDependencies<CustomerProfileEvent>)
    (ports: PromisePorts)
    : CustomerPromiseProfileAggregateApi =

    let configure (req: ConfigureCustomerPromiseProfileReq) : Task<Result<CustomerPromiseProfile, ApplicationError>> =
        taskResult {
            let! cmd: ConfigureCustomerPromiseProfileCmd = liftValidation (ACL.toConfigureCmd req)

            do! requireEntityExists ports.CustomerExists cmd.CustomerId "Customer" CustomerId.value

            let decideFn = Behaviors.configure policy true
            let pipeline = CommandPipeline.create repo (fun (c: ConfigureCustomerPromiseProfileCmd) -> c.CustomerId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    let updateTerms (req: UpdateCustomerServiceTermsReq) : Task<Result<CustomerPromiseProfile, ApplicationError>> =
        taskResult {
            let! cmd: UpdateCustomerServiceTermsCmd = liftValidation (ACL.toUpdateTermsCmd req)
            let decideFn = Behaviors.updateTerms policy
            let pipeline = CommandPipeline.create repo (fun (c: UpdateCustomerServiceTermsCmd) -> c.CustomerId) decideFn deps
            return! runPipeline pipeline publishKnowledge id cmd
        }

    { Configure = configure
      UpdateTerms = updateTerms }
