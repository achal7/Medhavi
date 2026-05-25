namespace Medhavi.SharedKernel

open System.Collections.Concurrent
open Medhavi.SharedKernel

/// Error types for repository operations
type RepositoryError =
    | ConcurrencyConflict of string
    | NotFound of string
    | StorageError of string

/// Pluggable Repository port defined as a record-of-functions.
type Repository<'Aggregate, 'Id, 'Event> =
    { Get: 'Id -> Async<Result<'Aggregate option, RepositoryError>>
      Save: 'Id * 'Aggregate * 'Event list -> Async<Result<unit, RepositoryError>>
      Delete: 'Id -> Async<Result<unit, RepositoryError>>
      GetEvents: 'Id -> Async<Result<'Event list, RepositoryError>>
      GetEventsByType: ('Event -> bool) -> Async<Result<'Event list, RepositoryError>>
      GetAll: unit -> Async<Result<'Aggregate list, RepositoryError>> }
