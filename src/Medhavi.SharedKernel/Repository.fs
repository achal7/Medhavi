namespace Medhavi.SharedKernel

open Medhavi.Common.Patterns
open System.Threading.Tasks

/// Versioned aggregate for optimistic concurrency
type VersionedAggregate<'Aggregate> = { Aggregate: 'Aggregate; Version: int }

/// Error types for repository operations
type RepositoryError =
    | ConcurrencyConflict of string
    | NotFound of string
    | StorageError of string

/// Pluggable Repository port defined as a record-of-functions.
type Repository<'Aggregate, 'Id, 'Event> =
    { Get: 'Id -> TaskResult<'Aggregate option, RepositoryError>
      Save: 'Id * 'Aggregate * 'Event list -> TaskResult<unit, RepositoryError>
      Delete: 'Id -> TaskResult<unit, RepositoryError>
      GetEvents: 'Id -> TaskResult<'Event list, RepositoryError>
      GetEventsByType: ('Event -> bool) -> TaskResult<'Event list, RepositoryError>
      GetAll: unit -> TaskResult<'Aggregate list, RepositoryError> }
