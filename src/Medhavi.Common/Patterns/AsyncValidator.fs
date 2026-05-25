module Medhavi.Common.Patterns.AsyncValidator

open Medhavi.Common
open Medhavi.Common.Validator

// ---------- Core type ----------
type AsyncValidation<'a, 'e> = Async<Validation<'a, 'e>>

// ---------- Constructors / Functor ----------
let return' (x: 'a) : AsyncValidation<'a, 'e> = async { return Valid x }

let map (f: 'a -> 'b) (av: AsyncValidation<'a, 'e>) : AsyncValidation<'b, 'e> =
    async {
        let! v = av
        return Validator.map f v
    }

let (<!>) = map

let bind (f: 'a -> AsyncValidation<'b, 'e>) (av: AsyncValidation<'a, 'e>) : AsyncValidation<'b, 'e> =
    async {
        let! v = av

        match v with
        | Valid x -> return! f x
        | Invalid errs -> return Invalid errs
    }
// ---------- Apply (two flavours) ----------
// Sequential apply (keeps original sequential semantics)
let applySequential (af: AsyncValidation<'a -> 'b, 'e>) (ax: AsyncValidation<'a, 'e>) : AsyncValidation<'b, 'e> =
    async {
        let! vf = af

        match vf with
        | Invalid errsF ->
            // short-circuit returning Invalid errsF; but we still need to check ax's errors if we want accumulation
            // We'll prefer Validator.apply to accumulate below; but keep this if explicit sequential desired
            let! _ = ax
            return Invalid errsF
        | Valid f ->
            let! vx = ax
            return Validator.apply (Valid f) vx
    }

// Parallel apply (recommended for independent validations / repository checks)
// This starts both child async computations concurrently and then combines their Validation results using Validator.apply
let applyParallel (af: AsyncValidation<'a -> 'b, 'e>) (ax: AsyncValidation<'a, 'e>) : AsyncValidation<'b, 'e> =
    async {
        // Start both child computations concurrently
        let! afChild = Async.StartChild af
        let! axChild = Async.StartChild ax

        // Await results
        let! vf = afChild
        let! vx = axChild

        // combine using Validation.apply (accumulates errors)
        return Validator.apply vf vx
    }

// Default operator: parallel apply (makes independent validations concurrent by default).
let (<*>) = applyParallel

// Optional operator for explicit sequential apply, if needed by caller
let (<**>) = applySequential

// ---------- Sequence / traverse helpers ----------

let sequence (avs: AsyncValidation<'a, 'e> list) : AsyncValidation<'a list, 'e> =
    async {
        let! arr = avs |> Async.Parallel

        let cons head tail = head :: tail
        let consV head tail = Validator.apply (Validator.map cons head) tail

        return Array.foldBack consV arr (Valid [])
    }

let traverse (f: 'x -> AsyncValidation<'a, 'e>) (xs: 'x list) : AsyncValidation<'a list, 'e> =
    xs |> List.map f |> sequence

let ofValidation (v: Validation<'a, 'e>) : AsyncValidation<'a, 'e> = async { return v }

let flatten (av: AsyncValidation<Validation<'a, 'e>, 'e>) : AsyncValidation<'a, 'e> =
    async {
        let! v = av

        match v with
        | Valid(Valid x) -> return Valid x
        | Valid(Invalid e) -> return Invalid e
        | Invalid e -> return Invalid e
    }

let flattenAsync (av: AsyncValidation<AsyncValidation<'a, 'e>, 'e>) : AsyncValidation<'a, 'e> =
    async {
        let! outer = av

        match outer with
        | Valid inner -> return! inner
        | Invalid errs -> return Invalid errs
    }

let liftValidation f = fun x -> f x |> ofValidation

let returnFromValidation f = return' (f >> ofValidation)
