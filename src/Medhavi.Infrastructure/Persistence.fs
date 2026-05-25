namespace Medhavi.Infrastructure

open System
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.Scenario

/// Generic Repository Interface for Aggregate Root snapshots
type IRepository<'Id, 'Aggregate> =
    abstract member LoadAsync : 'Id -> Task<Result<'Aggregate option, DomainError>>
    abstract member SaveAsync : 'Id * 'Aggregate * expectedVersion: int -> Task<Result<unit, DomainError>>

/// Concrete Scenario Repository Implementation (Placeholder for Marten Session integration)
type ScenarioRepository () =
    interface IRepository<string, Scenario> with
        member _.LoadAsync (id: string) =
            // Simulated Marten session load
            Task.FromResult(Ok None)
            
        member _.SaveAsync (id: string, scenario: Scenario, expectedVersion: int) =
            // Simulated Marten document persist and optimistic concurrency checking
            Task.FromResult(Ok ())
