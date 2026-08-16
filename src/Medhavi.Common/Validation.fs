module Medhavi.Common.Validation

type Validation<'a, 'b> =
    | Valid of 'a
    | Invalid of 'b list

// APPLICATIVE
let return' x = Valid x

let apply vf vx =
    match vf, vx with
    | Valid f, Valid x -> Valid(f x)

    | Invalid e1, Invalid e2 -> Invalid(e1 @ e2)

    | Invalid e, _
    | _, Invalid e -> Invalid e

let (<*>) = apply

/// Functor
let map f o =
    match o with
    | Valid x -> f x |> return'
    | Invalid errs -> Invalid errs

let mapError f o =
    match o with
    | Valid x -> Valid x
    | Invalid errs -> Invalid(errs |> List.map f)

let guard predicate error x = if predicate x then Valid x else Invalid [ error ]
let (<!>) = map

// Convenience
let lift2 f a b = f <!> a <*> b
let lift3 f a b c = f <!> a <*> b <*> c
let lift4 f a b c d = f <!> a <*> b <*> c <*> d
let lift5 f a b c d e = f <!> a <*> b <*> c <*> d <*> e

let inline ( *> ) a b = lift2 (fun _ z -> z) a b
let inline (<*) a b = lift2 (fun z _ -> z) a b

let validate predicate error x = if predicate x then Valid x else Invalid [ error ]
// let notEmptyString field = validate (System.String.IsNullOrWhiteSpace >> not) (MissingValue field)
// let positive field = validate (fun (x: int) -> x > 0) (InvalidValue(field, "Must be positive"))
// let notNull error = validator ((<>) null) error
// let inRange field min max = validator (fun x -> x >= min && x <= max) (OutOfRange(field, min, max))

/// Natural Transformation from Validation to List
let sequence xs =
    let cons head tail = head :: tail
    let consR headR tailR = cons <!> headR <*> tailR
    List.foldBack consR xs (return' [])

/// Natural Transformation from Validation to Sequence
let traverse f list = sequence(List.map f list)

/// Natural Transformation from Validation to Result
let toResult (validation: Validation<'a, 'b>) : Result<'a, 'b list> =
    match validation with
    | Valid x -> Ok x
    | Invalid e -> Error e

let toOption (validation: Validation<'a, 'b>) : 'a option =
    match validation with
    | Valid x -> Some x
    | Invalid _ -> None

let getErrors (validation: Validation<'a, 'b>) : 'b list =
    match validation with
    | Valid _ -> []
    | Invalid e -> e

/// Converts a Result<'a, 'e> into Validation<'a, 'e list>
/// Where Success = Valid 'a
/// Where Failure = Invalid ['e]
let fromResult result =
    match result with
    | Ok x -> Valid x
    | Error e -> Invalid [ e ]

let bindResult f validation =
    match validation with
    | Valid x -> fromResult(f x)
    | Invalid errs -> Invalid errs

// /// Composes a choice type with a non choice type.
// let inline (<?>) a b = lift2 (fun _ z -> z) a (Valid b)

// /// Composes a non-choice type with a choice type.
// let inline (|?>) a b = lift2 (fun z _ -> z) (Valid a) b
