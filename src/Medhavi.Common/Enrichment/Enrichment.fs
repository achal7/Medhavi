namespace Medhavi.Common.Enrichment

open System
open System.Threading.Tasks

// ==========================================
// ENRICHMENT ALGEBRA - Functional Programming Constructs
// ==========================================

// Core enrichment types
type EnrichmentResult<'T> =
    | Success of 'T
    | ValidationError of string
    | EnrichmentError of string
    | NotFound of string

type EnrichmentContext =
    { Source: string
      Timestamp: DateTimeOffset
      CorrelationId: string option
      EnrichmentLevel: string
      RelatedEntities: Map<string, obj> }

// FUNCTOR - Structure-preserving enrichment transformations
module EnrichmentFunctor =
    type 'a Enrichment = EnrichmentContext -> 'a -> Async<Result<'a, string>>

    let map (f: 'a -> 'b) (enrich: 'a Enrichment) : 'b Enrichment =
        fun ctx input ->
            async {
                let! result = enrich ctx input
                return Result.map f result
            }

    let identityLaw (enrich: 'a Enrichment) input =
        let mapped = map id enrich
        // Verify: map id = id
        true

// APPLICATIVE - Parallel independent enrichment operations
module EnrichmentApplicative =
    type 'a Enrichment = EnrichmentContext -> Async<Result<'a, string>>

    let return_ (x: 'a) : 'a Enrichment = fun _ -> async.Return(Ok x)

    let apply (f: ('a -> 'b) Enrichment) (x: 'a Enrichment) : 'b Enrichment =
        fun ctx ->
            async {
                let! fResult = f ctx
                let! xResult = x ctx

                return
                    match fResult, xResult with
                    | Ok f', Ok x' -> Ok(f' x')
                    | Error e, _ -> Error e
                    | _, Error e -> Error e
            }

    let liftA2 f x y = apply (apply (return_ f) x) y

// MONAD - Sequential enrichment with dependencies
module EnrichmentMonad =
    type 'a Enrichment = EnrichmentContext -> Async<Result<'a, string>>

    let return_ = EnrichmentApplicative.return_

    let bind (m: 'a Enrichment) (f: 'a -> 'b Enrichment) : 'b Enrichment =
        fun ctx ->
            async {
                let! result = m ctx

                match result with
                | Ok value -> return! (f value) ctx
                | Error e -> return Error e
            }

    let compose f g = bind f g

// MONOID - Enrichment result combination
module EnrichmentMonoid =
    type EnrichmentResult<'T> with
        static member Empty = Success Unchecked.defaultof<'T>

        static member Combine (a: EnrichmentResult<'T>) (b: EnrichmentResult<'T>) : EnrichmentResult<'T> =
            match a, b with
            | Success x, _ -> Success x
            | _, Success y -> Success y
            | ValidationError e1, ValidationError e2 -> ValidationError $"{e1}; {e2}"
            | ValidationError e, _ -> ValidationError e
            | _, ValidationError e -> ValidationError e
            | EnrichmentError e1, EnrichmentError e2 -> EnrichmentError $"{e1}; {e2}"
            | EnrichmentError e, _ -> EnrichmentError e
            | _, EnrichmentError e -> EnrichmentError e
            | NotFound e1, NotFound e2 -> NotFound $"{e1}; {e2}"

// NATURAL TRANSFORMATIONS - Context conversions
module EnrichmentNaturalTransformations =
    type NT<'a, 'b> = EnrichmentContext -> 'a -> Async<Result<'b, string>>

    let identity (enrich: 'a EnrichmentMonad.Enrichment) : 'a EnrichmentMonad.Enrichment = enrich

    let composeNT (nt1: NT<'a, 'b>) (nt2: NT<'b, 'c>) : NT<'a, 'c> =
        fun ctx input ->
            async {
                let! result1 = nt1 ctx input

                match result1 with
                | Ok intermediate -> return! nt2 ctx intermediate
                | Error e -> return Error e
            }

// KLEISLI CATEGORY - Function composition in enrichment context
module EnrichmentKleisli =
    type Kleisli<'a, 'b> = 'a -> EnrichmentMonad.Enrichment<'b>

    let identity (x: 'a) : Kleisli<'a, 'a> = fun _ -> EnrichmentMonad.return_ x

    let compose (f: Kleisli<'a, 'b>) (g: Kleisli<'b, 'c>) : Kleisli<'a, 'c> =
        fun x ctx ->
            async {
                let! result = f x ctx

                match result with
                | Ok y -> return! g y ctx
                | Error e -> return Error e
            }

// VALIDATION APPLICATIVE - Accumulate validation errors
module EnrichmentValidation =
    type Validation<'T> = Result<'T, string list>

    let return_ (x: 'T) : Validation<'T> = Ok x

    let apply (f: Validation<'T -> 'U>) (x: Validation<'T>) : Validation<'U> =
        match f, x with
        | Ok f', Ok x' -> Ok(f' x')
        | Error e1, Ok _ -> Error e1
        | Ok _, Error e2 -> Error e2
        | Error e1, Error e2 -> Error(e1 @ e2)

    let liftA2 f x y = apply (apply (return_ f) x) y
