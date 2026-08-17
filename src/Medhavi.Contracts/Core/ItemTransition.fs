namespace Medhavi.Contracts.Core.ItemTransition

open System
open System.Threading.Tasks
open Medhavi.Contracts

/// SE-C-040 Item Transition Data Transfer Object
type ItemTransitionDto =
    { TransitionId: string
      SupersededItem: string
      SupersedingItem: string
      TransitionType: string
      EffectiveDate: DateTimeOffset
      EndDate: DateTimeOffset option
      LifecycleState: string }

/// External request to recognize a new item transition
type RecognizeItemTransitionReq =
    { SupersededItem: string
      SupersedingItem: string
      TransitionType: string
      EffectiveDate: DateTimeOffset
      EndDate: DateTimeOffset option }

/// External request to suspend an active item transition
type SuspendItemTransitionReq =
    { SupersededItem: string
      SupersedingItem: string
      SuspensionTime: DateTimeOffset }

/// External request to reinstate a suspended item transition
type ReinstateItemTransitionReq =
    { SupersededItem: string
      SupersedingItem: string
      ReinstatementTime: DateTimeOffset }

/// External request to retire an item transition
type RetireItemTransitionReq =
    { SupersededItem: string
      SupersedingItem: string
      RetirementTime: DateTimeOffset }

/// Public API for Item Transition Management (CA-C-021)
type ItemTransitionApi =
    { Recognize: RecognizeItemTransitionReq -> Task<Result<ItemTransitionDto, ApiError>>
      Suspend: SuspendItemTransitionReq -> Task<Result<ItemTransitionDto, ApiError>>
      Reinstate: ReinstateItemTransitionReq -> Task<Result<ItemTransitionDto, ApiError>>
      Retire: RetireItemTransitionReq -> Task<Result<ItemTransitionDto, ApiError>> }

/// Query service alias for Item Transitions
type ItemTransitionQueries = QueryService<ItemTransitionDto, string>
