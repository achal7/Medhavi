module Medhavi.Foundation.Observation.Performance

open System.Diagnostics
open System.Threading.Tasks
open Medhavi.Foundation
open Medhavi.Foundation.Failure

let measure (operationName: string) (operation: unit -> 'T) : 'T * PerformanceMeasurement =
    let sw = Stopwatch.StartNew()

    try
        let result = operation()
        sw.Stop()

        let measurement =
            { OperationName = operationName
              Duration = sw.Elapsed
              Success = true
              Timestamp = SystemTimestamp.now
              Properties = Map.empty }

        (result, measurement)
    with ex ->
        sw.Stop()

        let _ =
            { OperationName = operationName
              Duration = sw.Elapsed
              Success = false
              Timestamp = SystemTimestamp.now
              Properties = Map.ofList [ "Error", box ex.Message ] }

        reraise()

let measureAsync
    (operationName: string)
    (operation: Task<'T>)
    : Task<Result<'T * PerformanceMeasurement, ApplicationError>> =
    task {
        let sw = Stopwatch.StartNew()

        try
            let! result = operation
            sw.Stop()

            let measurement =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = true
                  Timestamp = SystemTimestamp.now
                  Properties = Map.empty }

            return Ok(result, measurement)
        with ex ->
            sw.Stop()

            let _ =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = false
                  Timestamp = SystemTimestamp.now
                  Properties = Map.ofList [ "Error", box ex.Message ] }

            return Error(ApplicationError.fromException ex)
    }
