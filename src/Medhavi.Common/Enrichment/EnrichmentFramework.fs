namespace Medhavi.Common.Enrichment

open System
open System.Threading.Tasks

// ==========================================
// ENRICHMENT FRAMEWORK - Main Composition Layer
// ==========================================

// Generic enrichment pipeline
type EnrichmentPipeline<'input, 'output> =
    { Validate: 'input -> EnrichmentResult<'input>
      Enrich: EnrichmentContext -> 'input -> Async<Result<'output, string>>
      Serialize: 'output -> string }

// Framework composition functions
module EnrichmentFramework =

    // Create pipeline from functions
    let createPipeline validate enrich serialize =
        { Validate = validate
          Enrich = enrich
          Serialize = serialize }

    // Compose pipelines using Kleisli composition
    let composePipelines p1 p2 =
        { Validate =
            fun input ->
                match p1.Validate input with
                | Success valid -> p2.Validate valid
                | ValidationError err -> ValidationError err
                | EnrichmentError err -> EnrichmentError err
                | NotFound msg -> NotFound msg

          Enrich =
            fun ctx input ->
                async {
                    match p1.Validate input with
                    | Success validInput ->
                        let! result1 = p1.Enrich ctx validInput

                        match result1 with
                        | Ok output1 ->
                            let! result2 = p2.Enrich ctx output1
                            return result2
                        | Error e -> return Error e
                    | ValidationError err -> return Error err
                    | EnrichmentError err -> return Error err
                    | NotFound msg -> return Error msg
                }

          Serialize = fun output -> output |> p2.Serialize }

    // Functor instance for pipelines
    let mapPipeline f pipeline =
        { pipeline with
            Enrich =
                fun ctx input ->
                    async {
                        let! result = pipeline.Enrich ctx input
                        return Result.map f result
                    } }

    // Execute pipeline with context
    let executePipeline (pipeline: EnrichmentPipeline<'input, 'output>) ctx input =
        async {
            match pipeline.Validate input with
            | Success validInput ->
                let! result = pipeline.Enrich ctx validInput

                match result with
                | Ok output -> return Success(pipeline.Serialize output)
                | Error err -> return EnrichmentError err
            | ValidationError err -> return ValidationError err
            | EnrichmentError err -> return EnrichmentError err
            | NotFound msg -> return NotFound msg
        }

    // Utility functions
    let createEnrichmentContext source correlationId enrichmentLevel =
        { Source = source
          Timestamp = DateTimeOffset.UtcNow
          CorrelationId = correlationId
          EnrichmentLevel = enrichmentLevel
          RelatedEntities = Map.empty }

    let combineMetadata (maps: Map<string, obj> seq) =
        maps
        |> Seq.fold (fun acc map -> Map.fold (fun acc k v -> Map.add k v acc) acc map) Map.empty

// ==========================================
// ENRICHMENT RESULT UTILITIES
// ==========================================

module EnrichmentResult =

    // Functor operations on EnrichmentResult
    let map f =
        function
        | Success value -> Success(f value)
        | ValidationError err -> ValidationError err
        | EnrichmentError err -> EnrichmentError err
        | NotFound msg -> NotFound msg

    let bind f =
        function
        | Success value -> f value
        | ValidationError err -> ValidationError err
        | EnrichmentError err -> EnrichmentError err
        | NotFound msg -> NotFound msg

    // Convert to Result type
    let toResult =
        function
        | Success value -> Ok value
        | ValidationError err -> Error err
        | EnrichmentError err -> Error err
        | NotFound msg -> Error msg

    // Convert from Result type
    let fromResult =
        function
        | Ok value -> Success value
        | Error err -> EnrichmentError err

    // Get value or default
    let defaultValue defaultValue =
        function
        | Success value -> value
        | _ -> defaultValue

    // Check if successful
    let isSuccess =
        function
        | Success _ -> true
        | _ -> false

    // Get error message if any
    let getError =
        function
        | Success _ -> None
        | ValidationError err -> Some err
        | EnrichmentError err -> Some err
        | NotFound msg -> Some msg
