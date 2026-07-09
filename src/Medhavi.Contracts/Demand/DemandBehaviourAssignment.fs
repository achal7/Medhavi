module Medhavi.Contracts.Demand.DemandBehaviourAssignment

open System
open System.Threading.Tasks
open Medhavi.Contracts

type UpdateDemandBehaviourClassificationReq =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string }

type OverrideDemandBehaviourClassificationReq =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string
      NewClassification: string
      Justification: string }

type DemandBehaviourAssignment =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string
      CurrentClassification: string
      ClassificationConfidence: decimal
      EvidenceSummary: string
      LastClassified: DateTimeOffset
      BusinessTime: DateTimeOffset
      TransactionTime: DateTimeOffset }

type DemandBehaviourAssignmentApi =
    { UpdateBehaviour: UpdateDemandBehaviourClassificationReq -> Task<Result<string, ApiError>>
      OverrideBehaviour: OverrideDemandBehaviourClassificationReq -> Task<Result<string, ApiError>> }

type DemandBehaviourAssignmentQueries = QueryService<DemandBehaviourAssignment, string>

type DemandBehaviourClassificationChangedNotification =
    { EntityType: string
      EntityId: string
      BehaviourDimension: string
      PreviousClassification: string
      NewClassification: string
      Confidence: decimal }
