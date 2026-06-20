namespace Medhavi.Web

open System

type EntityRef = EntityRef of string * string // type and id

type UITheme =
    | Standard
    | Dark
    | StandardDark

type ConnectionStatus =
    | Connected
    | Reconnecting
    | Disconnected

type Notification =
    { Id: Guid
      Title: string
      Message: string
      Timestamp: DateTime
      IsRead: bool }

type OperationState<'TSuccess, 'TFailure> =
    | Pending
    | Running of progressPercentage: int * currentStage: string
    | Completed of 'TSuccess
    | Failed of 'TFailure
    | Cancelled

type Operation =
    { Id: Guid
      Name: string
      State: OperationState<unit, string> }

type CommandOrigin =
    | Human
    | Ai
    | System

type CommandStatus =
    | Queued
    | Succeeded
    | Failed

type CommandTrace =
    { TimestampUtc: DateTime
      Origin: CommandOrigin
      RawText: string
      ActionText: string
      Status: CommandStatus
      Notes: string option }

type ScenarioStatus =
    | Draft
    | Published
    | Archived

type BucketView =
    | Day
    | Week
    | Month

type StoreNotificationHandlers =
    { OnCreated: string -> Medhavi.Common.Patterns.TaskResult<unit, string>
      OnUpdated: string -> Medhavi.Common.Patterns.TaskResult<unit, string>
      OnDeleted: string -> Medhavi.Common.Patterns.TaskResult<unit, string> }
