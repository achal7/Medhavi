namespace Medhavi.Contracts

open System
open System.Text.Json.Serialization

type UIEventLogItem =
    { EventId: string
      EventType: string
      Stream: string
      Timestamp: DateTimeOffset }

[<JsonFSharpConverter>]
type Role =
    | Planner
    | Supervisor
    | Manager
    | Administrator

type User = { Name: string; Role: Role }

[<JsonFSharpConverter>]
type ApiError ={
    Code: string
    Category: string
    Message: string
}