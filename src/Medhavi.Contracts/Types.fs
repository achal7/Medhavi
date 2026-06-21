namespace Medhavi.Contracts

open System
open System.Text.Json.Serialization
open System.Threading.Tasks

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
type ApiError =
    { Code: string
      Category: string
      Message: string }

type QueryService<'Entity, 'Id> =
    { GetAll: unit -> Task<'Entity list>
      GetById: 'Id -> Task<'Entity option>
      Exists: 'Id -> Task<bool>
      Filter: ('Entity -> bool) -> Task<'Entity list>
      SubscribeApiEvents: (obj -> unit) -> IDisposable }
