module Medhavi.Common.Result

let sequenceOfResultList<'a, 'b> (xs: Result<'a, 'b> list) : Result<'a list, 'b> =
    let rec loop acc =
        function
        | [] -> Ok(List.rev acc)
        | Result.Ok v :: tail -> loop (v :: acc) tail
        | Result.Error e :: _ -> Error e

    loop [] xs

/// Return success with the specified value
let succeed x = Ok x

/// Apply either a success function or failure function
let either successFunc failureFunc result =
    match result with
    | Error err -> failureFunc err
    | Ok x -> successFunc x

/// Convert a Result to an Option
let toOption result =
    match result with
    | Ok v -> Some v
    | Error _ -> None

let validate predicate error x = if predicate x then Ok x else Error error

/// Combine two results using applicative style (short-circuits on first error)
[<CompiledName("Apply")>]
let apply fResult xResult =
    match fResult, xResult with
    | Ok f, Ok x -> Ok(f x)
    | Error err, Ok _ -> Error err
    | Ok _, Error err -> Error err
    | Error err1, Error _ -> Error err1

/// Combine two results using applicative style (accumulates errors)
let applyAcc (fR: Result<_, 'e list>) (xR: Result<_, 'e list>) =
    match fR, xR with
    | Ok f, Ok x -> Ok(f x)
    | Error errs1, Error errs2 -> Error(errs1 @ errs2)
    | Error errs, _ -> Error errs
    | _, Error errs -> Error errs

/// Convert an option to a Result
[<CompiledName("OfOption")>]
let ofOption errorValue opt =
    match opt with
    | Some v -> Ok v
    | None -> Error errorValue

/// Try to execute a function that might throw exceptions
let tryCatch f exnHandler x =
    try
        Ok(f x)
    with ex ->
        Error(exnHandler ex)

/// Sequence a list of results (short-circuits on first error)
let sequence results =
    let (<*>) = apply
    let (<!>) = Result.map
    let cons head tail = head :: tail
    let consR headR tailR = cons <!> headR <*> tailR
    List.foldBack consR results (Ok [])

/// Sequence a list of results (accumulates errors)
let sequenceAcc aListOfResults =
    let (<*>) = applyAcc
    let (<!>) = Result.map
    let cons head tail = head :: tail
    let consR headR tailR = cons <!> headR <*> tailR
    let initialValue = Ok []
    List.foldBack consR aListOfResults initialValue

/// Partition results into successes and failures
let partitionResults results =
    let successes, failures =
        results
        |> List.fold
            (fun (oks, errs) result ->
                match result with
                | Ok ok -> (ok :: oks, errs)
                | Error err -> (oks, err @ errs))
            ([], [])

    (List.rev successes, List.rev failures)

/// Map over both cases of Result (bifunctor)
let bimap f g =
    function
    | Ok x -> Ok(f x)
    | Error e -> Error(g e)

/// Execute side effect if Ok and return original result
let tee f result =
    match result with
    | Ok x ->
        f x
        Ok x
    | Error _ -> result

/// Execute side effect if Error and return original result
let teeError f result =
    match result with
    | Ok _ -> result
    | Error e ->
        f e
        Error e

/// Map over the error case
let mapError f =
    function
    | Ok x -> Ok x
    | Error e -> Error(f e)

/// Kleisli composition for result-returning functions
let (>=>) f g x =
    match f x with
    | Ok y -> g y
    | Error e -> Error e

/// Traverse a list with a result-returning function
let traverse (f: 'a -> Result<'b, 'e>) (list: 'a list) : Result<'b list, 'e> = sequence(List.map f list)

/// Traverse with error accumulation
let traverseAcc (f: 'a -> Result<'b, 'e list>) (list: 'a list) : Result<'b list, 'e list> = sequenceAcc(List.map f list)

/// Lift a function to work on Results
let lift2 f xR yR = apply (apply (succeed f) xR) yR

/// Safe parallel execution - partitions results without unsafe pattern matching
/// This replaces the unsafe `failwith "Impossible"` patterns
let partitionResultsSafe (results: Result<'T, 'E> array) : Result<'T list, 'E list> =
    let oks, errors =
        results
        |> Array.fold
            (fun (okAcc, errAcc) result ->
                match result with
                | Ok value -> (value :: okAcc, errAcc)
                | Error err -> (okAcc, err :: errAcc))
            ([], [])

    if List.isEmpty errors then Ok(List.rev oks) else Error(List.rev errors)

/// Extract Ok values from array (safe - uses Array.choose with exhaustive matching)
let extractOkValues (results: Result<'T, 'E> array) : 'T list =
    results
    |> Array.choose (function
        | Ok x -> Some x
        | Error _ -> None)
    |> Array.toList

/// Extract Error values from array (safe - uses Array.choose with exhaustive matching)
let extractErrorValues (results: Result<'T, 'E> array) : 'E list =
    results
    |> Array.choose (function
        | Ok _ -> None
        | Error e -> Some e)
    |> Array.toList

/// Computation expression builder for Result
type ResultBuilder() =
    member __.Return(x) = Ok x
    member __.ReturnFrom(x) = x
    member __.Bind(x, f) = Result.bind f x
    member __.Zero() = Ok()
    member __.Delay(f) = f
    member __.Run(f) = f()

    // member _.Combine(x, f) =
    //     match x with
    //     | Ok _ -> f ()
    //     | Error e -> Error e
    member this.Combine(a, b) = this.Bind(a, (fun () -> b()))

    member this.IfThenElse(condition, ifBody, elseBody) = if condition then ifBody() else elseBody()

    member __.For(xs: seq<'a>, body: 'a -> Result<unit, 'b>) =
        let folder state x =
            match state with
            | Error _ -> state
            | Ok _ -> body x

        Seq.fold folder (Ok()) xs

    member __.While(guard, body) =
        if not(guard()) then
            Ok()
        else
            Result.bind (fun () -> __.While(guard, body)) (body())

    member __.TryWith(body, handler) =
        try
            body()
        with e ->
            handler e

    member __.TryFinally(body, compensation) =
        try
            body()
        finally
            compensation()

    member _.Using(resource: #System.IDisposable, body) =
        try
            body resource
        finally
            if not(isNull(box resource)) then
                resource.Dispose()

// Additional utilities for common patterns
[<AutoOpen>]
module ResultOperators =
    let (<*>) = apply
    let (<*!>) = applyAcc
    let (>=>) = (>=>)
    let (>>=) x f = Result.bind f x
