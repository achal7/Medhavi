namespace Medhavi.Web

open System
open Medhavi.Nexus

open System.Text.Json.Serialization

[<JsonConverter(typeof<JsonStringEnumConverter>)>]
type Role =
    | Planner = 0
    | Supervisor = 1
    | Manager = 2
    | Administrator = 3

type User =
    { Username: string
      Email: string
      Role: Role }

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

type ActiveOperation =
    { Id: Guid
      Name: string
      State: OperationState<unit, string> }
