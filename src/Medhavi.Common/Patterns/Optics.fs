module Medhavi.Common.Patterns.Optics

/// Simple Lens: get and set
type Lens<'S, 'A> = { Get: 'S -> 'A; Set: 'A -> 'S -> 'S }

let lens get set = { Get = get; Set = set }

let get (l: Lens<'S, 'A>) (s: 'S) : 'A = l.Get s
let set (l: Lens<'S, 'A>) (a: 'A) (s: 'S) : 'S = l.Set a s
let over (l: Lens<'S, 'A>) (f: 'A -> 'A) (s: 'S) : 'S = l.Set (f (l.Get s)) s

/// Compose two lenses: S -> A and A -> B -> S -> B
let composeLens (outer: Lens<'S, 'A>) (inner: Lens<'A, 'B>) : Lens<'S, 'B> =
    {
        Get = fun s -> inner.Get(outer.Get s)
        Set =
            fun b s ->
                let a = outer.Get s
                let a' = inner.Set b a
                outer.Set a' s
    }

/// Identity lens
let idLens<'T> : Lens<'T, 'T> = { Get = id; Set = fun v _ -> v }

// ---------------------------------------------------------------------
/// Prism for discriminated unions: tryGet + inject (build)
type Prism<'S, 'A> =
    {
        TryGet: 'S -> 'A option
        Inject: 'A -> 'S
    }

let prism tryGet inject = { TryGet = tryGet; Inject = inject }

/// Try to modify via prism: if present, apply f, else leave S unchanged
let overPrism (p: Prism<'S, 'A>) (f: 'A -> 'A) (s: 'S) : 'S =
    match p.TryGet s with
    | Some a -> p.Inject(f a)
    | None -> s

(*
// Example
type RoutingSpecific =
    | Purchase of int
    | Transport of int * int
let example = Purchase 10

let purchasePrism =
    { TryGet =
        function
        | Purchase x -> Some x
        | _ -> None

      Inject =
        fun x -> Purchase x }

let updated =
    overPrism purchasePrism (fun x -> x + 5) example

*)

// Compose Prism after Lens (Lens S A) then Prism (A B) => Prism S B
// let composeLensPrism (ln: Lens<'S, 'A>) (pr: Prism<'A, 'B>) : Prism<'S, 'B> =
//     {
//         TryGet = fun s -> ln.Get s |> pr.TryGet
//         Inject = fun b -> ln.Set(pr.Inject b)
//     } // inject b into A then set A into S

// ---------------------------------------------------------------------
/// Traversal (simple): focuses zero..many parts in S (we implement for lists here)
/// Traversal is represented by `modifyMany : ('A -> 'A) -> 'S -> 'S`
type Traversal<'S, 'A> = { ModifyMany: ('A -> 'A) -> 'S -> 'S }

let traversal modifyMany = { ModifyMany = modifyMany }
let overMany (t: Traversal<'S, 'A>) f s = t.ModifyMany f s

/// Basic traversal for list<'A> (the whole structure is list<A>)
let listTraversal<'A> : Traversal<'A list, 'A> = traversal (fun f -> List.map f)

/// Example: traversal that modifies every element inside a particular field (requires lens)
let traversalViaLens (ln: Lens<'S, 'A list>) : Traversal<'S, 'A> = traversal (fun f s -> over ln (List.map f) s)

// ---------------------------------------------------------------------
/// Simple Iso: bijection between types A <-> B
type Iso<'A, 'B> =
    {
        Forward: 'A -> 'B
        Backward: 'B -> 'A
    }

let iso f g = { Forward = f; Backward = g }
let view (i: Iso<'A, 'B>) = i.Forward
let rev (i: Iso<'A, 'B>) = i.Backward

// Compose isos
let composeIso (i1: Iso<'A, 'B>) (i2: Iso<'B, 'C>) : Iso<'A, 'C> =
    iso (i2.Forward << i1.Forward) (i1.Backward << i2.Backward)

// ---------------------------------------------------------------------
/// Optional: lens-like for Option<'A> inside S
type Optional<'S, 'A> =
    {
        TryGet: 'S -> 'A option
        Set: 'A option -> 'S -> 'S
    }

let optional tryGet set = { TryGet = tryGet; Set = set }

let overOptional (o: Optional<'S, 'A>) (f: 'A -> 'A) (s: 'S) =
    match o.TryGet s with
    | None -> s
    | Some a -> o.Set (Some(f a)) s

// Van Laarhoven Encoding
type Identity<'a> = | Identity of 'a

module Identity =
    let return' x = Identity x
    let map f (Identity x) = Identity(f x)
    let apply (Identity f) (Identity x) = Identity(f x)
    let run (Identity x) = x

let traverseList return' map apply f xs =
    let cons head tail = head :: tail

    let rec loop =
        function
        | [] -> return' []
        | x :: rest -> apply (map cons (f x)) (loop rest)

    loop xs

module ListA =
    let return' x = [ x ]

    let map f xs = List.map f xs

    let apply fs xs =
        [
            for f in fs do
                for x in xs do
                    yield f x
        ]

let overList f xs =
    traverseList Identity.return' Identity.map Identity.apply (fun x -> Identity(f x)) xs
    |> Identity.run

// Example
let branch xs = traverseList ListA.return' ListA.map ListA.apply (fun x -> [ x; x + 10 ]) xs

let traverseViaLens return' map apply lens f s =
    let inner = lens.Get s

    let updated = traverseList return' map apply f inner

    map (fun newInner -> lens.Set newInner s) updated

// type Const<'c, 'a> = | Const of 'c

// module Const =
//     let return' _ = failwith "Const.pure not meaningful"
//     let map _ (Const c) = Const c
//     let apply (Const c1) (Const c2) = Const c1

// type VLens<'S, 'A> = ('A -> Identity<'A>) -> 'S -> Identity<'S>

// let vLens get set : VLens<'S,'A> =
//     fun f s ->
//         let a = get s
//         let Identity a' = f a
//         Identity (set a' s)

// let view (lens:VLens<'S,'A>) (s:'S) =
//     let toConst a = Const(a)
//     lens (fun a -> Identity a) s // simplified for demo

// type VLens<'S,'A> =
//     abstract Apply<'F> :
//         (module Functor<'F>) ->
//         ('A -> 'F) ->
//         'S ->
//         'F
// // Van Laarhoven Traversal
// type Applicative<'a, 'b,'F> =
//     { Pure : 'a -> 'F
//       Map  : ('a -> 'b) -> 'F -> 'F
//       Apply: 'F -> 'F -> 'F }

// type VTraversal<'S, 'A, 'F> = { Traverse: ('A -> 'F) -> 'S -> 'F }

// type VTraversal<'S, 'A> =
//     {
//         Traverse: ('A -> Identity<'A>) -> 'S -> Identity<'S>
//     }

// let listTraversal =
//     fun (return', map, apply) f xs ->
//         let cons head tail = head :: tail
//         let liftedCons = map cons (f (List.head xs))
//         List.foldBack (fun x acc -> apply (map cons (f x)) acc) xs (return' [])

// let overTraversal t f s =
//     t.Traverse (fun a -> Identity (f a)) s

// let overList f xs =
//     listTraversal (Identity.return', Identity.map, Identity.apply) (fun x -> Identity(f x)) xs

// // Example
// let branchingTraversal =
//     fun f xs ->
//         let rec combine acc = function
//             | [] -> [List.rev acc]
//             | x::rest ->
//                 [ for x' in f x do
//                     yield! combine (x'::acc) rest ]
//         combine [] xs
