namespace Medhavi.Foundation.Contracts

open System
open System.Threading.Tasks
open Medhavi.Foundation.Failure

/// Declarative event filter for projections
type EnvelopeFilter =
    /// Subscribe to specific event types
    | EventTypes of string list
    /// Subscribe to all events from a capability
    | Capability of string
    /// Subscribe to all events from an aggregate
    | Aggregate of string
    /// Subscribe to all events (use with caution)
    | All

/// Projection health status
type ProjectionHealth =
    | Healthy of eventsProcessed: int64
    | Degraded of reason: string
    | Failed of error: string

/// Projection statistics
type ProjectionStats =
    { EventsProcessed: int64
      ItemCount: int64
      ProcessedMessageIds: Set<Guid>
      LastUpdated: DateTimeOffset
      LastMessageId: Guid option
      LastCausationId: Guid option
      LastCorrelationId: Guid option
      LastError: string option
      MailboxSize: int }

    static member Default =
        { EventsProcessed = 0L
          ItemCount = 0L
          ProcessedMessageIds = Set.empty
          LastUpdated = DateTimeOffset.MinValue
          LastMessageId = None
          LastCausationId = None
          LastCorrelationId = None
          LastError = None
          MailboxSize = 0 }

type QueryService<'Entity, 'Id> =
    { GetAll: unit -> Task<'Entity list>
      GetById: 'Id -> Task<'Entity option>
      Exists: 'Id -> Task<bool>
      Filter: ('Entity -> bool) -> Task<'Entity list>
      SubscribeApiEvents: (obj -> unit) -> IDisposable }

type ProjectionContext<'Entity, 'Id> =
    { QueryService: QueryService<'Entity, 'Id>
      Dispose: unit -> unit }

/// Enhanced port that accepts pure domain functions and returns a wired query service.
type CreateQueryService<'Event, 'Entity, 'Id when 'Id: comparison> =
    (Map<'Id, 'Entity> -> 'Event -> Map<'Id, 'Entity>) // Apply function
        -> EnvelopeFilter // Envelope filter list
        -> Map<'Id, 'Entity> // Initial state
        -> string // Projection name
        -> Task<Result<ProjectionContext<'Entity, 'Id>, ApplicationError>>

type ProjectionAgentPort<'Event, 'Entity, 'Id when 'Id: comparison> =
    {
        /// Create a projection with query service
        /// Domain provides: apply function, filter, initial state, projection name
        /// Infrastructure returns: QueryService (subscription managed externally)
        Create: CreateQueryService<'Event, 'Entity, 'Id>

        /// Get current projection statistics
        GetStats: string -> Task<ProjectionStats>

        /// Get current projection health
        GetHealth: string -> Task<ProjectionHealth>
    }
