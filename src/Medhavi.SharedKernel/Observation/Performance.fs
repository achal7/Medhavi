namespace Medhavi.SharedKernel.Observation

open System.Diagnostics
open System.Threading.Tasks
open Medhavi.SharedKernel
open Medhavi.SharedKernel.Failure

module Performance =
    let measure (operationName: string) (operation: unit -> 'T) : 'T * PerformanceMeasurement =
        let sw = Stopwatch.StartNew()

        try
            let result = operation()
            sw.Stop()

            let measurement =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = true
                  Timestamp = Timestamp.now
                  Properties = Map.empty }

            (result, measurement)
        with ex ->
            sw.Stop()

            let _ =
                { OperationName = operationName
                  Duration = sw.Elapsed
                  Success = false
                  Timestamp = Timestamp.now
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
                      Timestamp = Timestamp.now
                      Properties = Map.empty }

                return Ok(result, measurement)
            with ex ->
                sw.Stop()

                let _ =
                    { OperationName = operationName
                      Duration = sw.Elapsed
                      Success = false
                      Timestamp = Timestamp.now
                      Properties = Map.ofList [ "Error", box ex.Message ] }

                return Error(ApplicationError.fromException ex)
        }
