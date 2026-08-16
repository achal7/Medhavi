namespace Medhavi.Foundation.Contracts

open Medhavi.Common
open Medhavi.Foundation.Failure

/// Error types for repository operations
type RepositoryError =
    | ConcurrencyConflict of string
    | NotFound of string
    | StorageError of string

/// Pluggable Repository port defined as a record-of-functions.
type Repository<'Aggregate, 'Id, 'Event> =
    { Get: 'Id -> TaskResult<'Aggregate option, RepositoryError>
      GetWithVersion: 'Id -> TaskResult<('Aggregate option * int option), RepositoryError>
      Save: 'Id * int option * 'Aggregate * 'Event list -> TaskResult<unit, RepositoryError>
      SaveBatch: ('Id * 'Aggregate * 'Event list) list -> TaskResult<unit, RepositoryError>
      Delete: 'Id -> TaskResult<unit, RepositoryError>
      GetEvents: 'Id -> TaskResult<'Event list, RepositoryError>
      GetEventsByType: ('Event -> bool) -> TaskResult<'Event list, RepositoryError>
      GetAll: unit -> TaskResult<'Aggregate list, RepositoryError> }

module Repository =
    let mapRepositoryErrorToApplicationError (e: RepositoryError) : ApplicationError =
        match e with
        | ConcurrencyConflict msg
        | NotFound msg
        | StorageError msg -> Infrastructure(Database msg)
