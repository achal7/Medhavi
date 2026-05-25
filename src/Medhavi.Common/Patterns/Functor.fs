namespace Medhavi.Common.Patterns

// ==========================================
// FUNCTOR - Structure-Preserving Mappings
// ==========================================

/// FUNCTOR - Mathematical structure preserving mappings
/// ====================================================
/// A functor is a mapping between categories that preserves structure.
/// It consists of:
/// 1. Object mapping: F: Ob(C) → Ob(D)
/// 2. Morphism mapping: F: Hom_C(A,B) → Hom_D(F(A),F(B))
/// Such that:
/// - F(id_A) = id_F(A) (identity preservation)
/// - F(g ∘ f) = F(g) ∘ F(f) (composition preservation)

module Functor =

    // -------------------------
    // Fundamental helpers / laws
    // -------------------------
    /// Identity law check:
    /// For any map: (('A -> 'A) -> F<'A> -> F<'A>)
    /// map id x = x
    let identityLaw map x = map id x = x

    /// Composition law check:
    /// map (g << f) = map g << map f
    let compositionLaw map f g x = map (g << f) x = (map g << map f) x

// ==========================================
// FUNCTOR UTILITIES (Derived from `map`)
// ==========================================
module FunctorUtils =

    /// Lift a function into the functor context
    let lift map f = map f

    /// fmap: alias for map (common FP name, esp. Haskell style)
    let fmap map = map

    /// void: replace all values inside the functor with unit
    let void_ map fx = map (fun _ -> ()) fx

    /// composeMaps: compose two functorial maps for nested functors
    let composeMaps mapF mapG h gfx = mapF (mapG h) gfx

/// COVARIANT FUNCTOR - Maps in the "forward" direction
/// ====================================================
/// F: C → D where F(f: A→B) becomes F(f): F(A)→F(B)
/// READER FUNCTOR - Handles environment-dependent computations
/// Reader<R, A> represents a computation that depends on environment R and produces value A
type Reader<'R, 'T> = 'R -> 'T

module Reader =
    let map f reader = fun env -> f (reader env)

/// CONTRAVARIANT FUNCTOR - Maps in the "reverse" direction
/// ========================================================
/// F: C → D where F(f: A→B) becomes F(f): F(B)→F(A)
/// Useful for predicates, comparators, and consumers
/// PREDICATE FUNCTOR - For boolean-valued functions
[<Struct>]
type Predicate<'T> =
    | Predicate of ('T -> bool)

    member this.Run x = let (Predicate f) = this in f x
    static member (&&&)(Predicate p1, Predicate p2) = Predicate(fun x -> p1 x && p2 x)
    static member (|||)(Predicate p1, Predicate p2) = Predicate(fun x -> p1 x || p2 x)
    static member (~~~)(Predicate p) = Predicate(fun x -> not (p x))

module Predicate =
    let contramap (f: 'b -> 'T) (Predicate pred: Predicate<'T>) : Predicate<'b> = Predicate(fun x -> pred (f x))

    let and' p1 p2 = p1 &&& p2
    let or' p1 p2 = p1 ||| p2
    let not' p = ~~~p
    let (&&&) = and'
    let (|||) = or'

/// COMPARATOR FUNCTOR - For comparison functions
type Comparator<'T> = 'T -> 'T -> int

module Comparator =
    let contramap f cmp = fun x y -> cmp (f x) (f y)

/// SHOW FUNCTOR - For string conversion functions
type Show<'T> = 'T -> string

module Show =
    let contramap f show = fun x -> show (f x)

/// BIFUNCTOR - Maps over both type parameters
/// ===========================================
/// RESULT BIFUNCTOR - Maps over both success and error types
module ResultBifunctor =
    let bimap f g =
        function
        | Ok value -> Ok(f value)
        | Error error -> Error(g error)

/// PAIR BIFUNCTOR - Maps over both elements of a pair
module PairBifunctor =
    let bimap f g (x, y) = (f x, g y)

// -------------------------
// Natural transformations - - Morphisms between functors
// -------------------------
/// A natural transformation from F to G is a polymorphic function:
///   eta : forall A. F<A> -> G<A>
/// It should satisfy naturality: eta (mapF f x) = mapG f (eta x)
module NaturalTransformation =

    /// Check naturality law:
    /// eta (mapF f x) = mapG f (eta x)
    let naturality eta mapF mapG f x =
        let left = eta (mapF f x)
        let right = mapG f (eta x)
        left = right

    /// Natural transformation: Option → List
    let optionToList =
        function
        | Some x -> [ x ]
        | None -> []

    /// Natural transformation: List → Option (head)
    let listToOption = List.tryHead

/// CONTRAVARIANT FUNCTOR LAWS
/// ===========================

module ContravariantLaws =
    /// Identity law: contramap id = id
    let identityLaw (pred: Predicate<'T>) (x: 'T) =
        let mapped = Predicate.contramap id pred
        mapped.Run x = pred.Run x

    /// Composition law: contramap (g << f) = (contramap f) << (contramap g)
    let compositionLaw (f: 'U -> 'T) (g: 'V -> 'U) (pred: Predicate<'T>) (x: 'V) =
        let left = Predicate.contramap (f << g) pred
        let right = (Predicate.contramap g) (Predicate.contramap f pred)
        left.Run x = right.Run x

module Universal =
    // ==========================================
    // 2. UNIVERSAL CONSTRUCTIONS
    // Limits & Colimits in Category Theory
    // ==========================================

    // UNIVERSAL CONSTRUCTIONS
    // ======================
    // Universal constructions define objects that satisfy universal properties.
    // They are "the most efficient" solutions to certain problems.
    // PRODUCTS (Limits)
    // ================
    // A product A × B is an object equipped with projections π₁: A×B → A, π₂: A×B → B
    // such that for any object C with maps f: C → A, g: C → B, there exists a unique
    // map ⟨f,g⟩: C → A×B making the diagram commute.
    // COPRODUCTS (Colimits)
    // =====================
    // A coproduct A + B is an object equipped with injections ι₁: A → A+B, ι₂: B → A+B
    // such that for any object C with maps f: A → C, g: B → C, there exists a unique
    // map [f,g]: A+B → C making the diagram commute.

    /// EQUALIZERS (Limits)
    /// ==================
    /// An equalizer of morphisms f,g: A → B is an object E with morphism e: E → A
    /// such that f∘e = g∘e, and E is universal with this property.

    type IEqualizer<'A, 'Equalizer> =
        abstract member Equalize: 'A -> 'Equalizer option
        abstract member Embed: 'Equalizer -> 'A

    /// Example: Equalizer for filtering even numbers
    type EvenEqualizer() =
        interface IEqualizer<int, int> with
            member _.Equalize x = if x % 2 = 0 then Some x else None
            member _.Embed x = x

    /// COEQUALIZERS (Colimits)
    /// ======================
    /// A coequalizer of morphisms f,g: A → B is an object Q with morphism q: B → Q
    /// such that q∘f = q∘g, and Q is universal with this property.

    type ICoequalizer<'B, 'Coequalizer> =
        abstract member Coequalize: 'B -> 'Coequalizer
        abstract member Project: 'Coequalizer -> 'B

    /// Example: Coequalizer for modulo operation
    type ModuloCoequalizer(modulus: int) =
        interface ICoequalizer<int, int> with
            member _.Coequalize x = x % modulus
            member _.Project x = x

    /// PULLBACKS (Limits)
    /// =================
    /// A pullback of cospan A → C ← B is an object P with morphisms p₁: P → A, p₂: P → B
    /// such that the diagram commutes, and P is universal with this property.

    type IPullback<'A, 'B, 'C, 'Pullback> =
        abstract member Fst: 'Pullback -> 'A
        abstract member Snd: 'Pullback -> 'B
        abstract member Create: 'A -> 'B -> 'Pullback option

    /// Example: Pullback for intersecting sets
    type SetPullback<'T when 'T: comparison>() =
        interface IPullback<Set<'T>, Set<'T>, Set<'T>, Set<'T>> with
            member _.Fst s = s
            member _.Snd s = s
            member _.Create a b = Some(Set.intersect a b)

    /// PUSHFORWARDS (Colimits)
    /// ======================
    /// A pushforward of span A ← C → B is an object P with morphisms p₁: A → P, p₂: B → P
    /// such that the diagram commutes, and P is universal with this property.

    type IPushforward<'A, 'B, 'C, 'Pushforward> =
        abstract member Fst: 'A -> 'Pushforward
        abstract member Snd: 'B -> 'Pushforward
        abstract member Match: ('A -> 'D) -> ('B -> 'D) -> 'Pushforward -> 'D
