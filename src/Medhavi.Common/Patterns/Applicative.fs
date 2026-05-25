namespace Medhavi.Common.Patterns

/// APPLICATIVE FUNCTOR - Structure for independent computations
/// ===========================================================
/// An applicative functor sits between Functor and Monad in the type class hierarchy.
/// It allows applying functions wrapped in a context to values wrapped in the same context,
/// but unlike monads, the effects are independent (not sequential).
/// ======================

/// OPTION APPLICATIVE - Handles optional computations
/// Short-circuits on None (first failure stops computation)
module OptionApplicative =
    let return_ (x: 'T) = Some x

    let apply (f: Option<'T -> 'U>) (x: Option<'T>) =
        match f, x with
        | Some f', Some x' -> Some(f' x')
        | _ -> None

/// RESULT APPLICATIVE - Handles success/failure computations
module ResultApplicative =
    let return_ (x: 'T) = Ok x

    let apply (f: Result<'T -> 'U, 'E>) (x: Result<'T, 'E>) =
        match f, x with
        | Ok f', Ok x' -> Ok(f' x')
        | Error e1, Ok _ -> Error e1
        | Ok _, Error e2 -> Error e2
        | Error e1, Error _ -> Error e1

/// LIST APPLICATIVE - Handles non-deterministic computations
/// Creates all possible combinations of function applications
module ListApplicative =
    let return_ (x: 'T) = [ x ]

    let apply (fs: List<'T -> 'U>) (xs: List<'T>) =
        [ for f in fs do
              for x in xs do
                  yield f x ]

/// VALIDATION APPLICATIVE - Accumulates multiple validation errors
module ValidationApplicative =
    let return_ (x: 'T) = Ok x

    let apply (f: Result<'T -> 'U, 'E list>) (x: Result<'T, 'E list>) =
        match f, x with
        | Ok f', Ok x' -> Ok(f' x')
        | Error e1, Ok _ -> Error e1
        | Ok _, Error e2 -> Error e2
        | Error e1, Error e2 -> Error(e1 @ e2)

/// ASYNC APPLICATIVE - Handles parallel asynchronous computations
module AsyncApplicative =
    let return_ (x: 'T) = async.Return x

    let apply (fAsync: Async<'T -> 'U>) (xAsync: Async<'T>) =
        async {
            let! f = fAsync
            let! x = xAsync
            return f x
        }

/// ASYNCRESULT APPLICATIVE - Combines async and result applicatives
module AsyncResultApplicative =
    let return_ (x: 'T) = async.Return(Ok x)

    let apply (fAsync: Async<Result<'T -> 'U, 'E>>) (xAsync: Async<Result<'T, 'E>>) =
        async {
            let! fResult = fAsync
            let! xResult = xAsync

            return
                match fResult, xResult with
                | Ok f', Ok x' -> Ok(f' x')
                | Error e1, Ok _ -> Error e1
                | Ok _, Error e2 -> Error e2
                | Error e1, Error _ -> Error e1
        }

/// APPLICATIVE UTILITY FUNCTIONS
/// =============================
module ApplicativeUtils =
    /// Lift a two-parameter function to any applicative given its map and apply implementations
    let liftA2 map apply f x y =
        apply (map f x) y

    /// Lift a three-parameter function to any applicative given its map and apply implementations
    let liftA3 map apply f x y z =
        apply (apply (map f x) y) z

    /// Sequence a list of applicative values given its return_, map, and apply implementations
    let sequenceA return_ map apply xs =
        let cons head tail = head :: tail
        let pureEmpty = return_ []

        List.foldBack
            (fun x acc ->
                apply (map cons x) acc)
            xs
            pureEmpty

/// APPLICATIVE LAWS VERIFICATION
/// =============================

module ApplicativeLaws =
    /// Identity: return_ id <*> v ≡ v
    let identityLaw (v: Option<'T>) =
        let pureId = OptionApplicative.return_ id
        let left = OptionApplicative.apply pureId v
        left = v

    /// Homomorphism: return_ f <*> return_ x ≡ return_ (f x)
    let homomorphismLaw (f: 'T -> 'U) (x: 'T) =
        let pureF = OptionApplicative.return_ f
        let pureX = OptionApplicative.return_ x
        let left = OptionApplicative.apply pureF pureX
        let right = OptionApplicative.return_ (f x)
        left = right

    /// Interchange: u <*> return_ y ≡ return_ ($ y) <*> u
    let interchangeLaw (u: Option<'T -> 'U>) (y: 'T) =
        let pureY = OptionApplicative.return_ y
        let left = OptionApplicative.apply u pureY
        let dollarY = fun f -> f y
        let pureDollarY = OptionApplicative.return_ dollarY
        let right = OptionApplicative.apply pureDollarY u
        left = right
