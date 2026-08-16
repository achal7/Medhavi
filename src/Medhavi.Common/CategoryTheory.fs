namespace Medhavi.Common

/// CATEGORY - Mathematical structure with objects and morphisms
/// ============================================================
/// A category consists of:
/// - Objects: Abstract entities
/// - Morphisms: Structure-preserving mappings between objects
/// - Identity: Each object has an identity morphism
/// - Composition: Morphisms compose associatively
type Category<'Mor> =
    {
        /// Identity morphism for any object
        Id: 'Mor
        /// Compose two morphisms (g ∘ f)
        Compose: 'Mor -> 'Mor -> 'Mor
    }

/// FUNCTION CATEGORY - Functions as morphisms
/// ==========================================
/// The fundamental category where types are objects and functions are morphisms
module Category =
    /// Identity function - satisfies category identity law
    let identity (x: 'T) = x

    // /// Function composition - satisfies category composition law
    // let compose (f: 'A -> 'B) (g: 'B -> 'C) : ('A -> 'C) = f >> g

    /// Left-to-right composition (F#-friendly)
    let composeLR f g = f >> g

    /// Right-to-left composition (math-style)
    let composeRL f g = f << g

    // For endomorphisms (morphisms are 'A -> 'A)
    let endoFunctionCategory<'A> : Category<'A -> 'A> =
        { Id = identity // for any object return the identity morphism
          Compose = composeLR }

/// ISOMORPHISM - Bijective morphism with inverse
/// ==============================================
/// A morphism that has a two-way inverse satisfying:
/// to ∘ from = id ∧ from ∘ to = id
type Isomorphism<'A, 'B> =
    {
        /// Forward transformation
        To: 'A -> 'B
        /// Inverse transformation
        From: 'B -> 'A
    }

/// Verify isomorphism laws
module Isomorphism =
    let verifyLaws (iso: Isomorphism<'A, 'B>) (a: 'A) (b: 'B) =
        let toFrom = iso.To a |> iso.From = a // to ∘ from = id
        let fromTo = iso.From b |> iso.To = b // from ∘ to = id
        toFrom, fromTo

/// MONOMORPHISM - Injective morphism. Preserves distinctness of elements (left-cancellable)
type Monomorphism<'A, 'B> = { Morphism: 'A -> 'B }

/// EPIMORPHISM - Surjective morphism. Covers entire codomain (right-cancellable)
type Epimorphism<'A, 'B> = { Morphism: 'A -> 'B }

/// INITIAL OBJECT - Universal source
/// ================================
/// Object with unique morphism to every other object
type Initial<'Obj> =
    {
        /// The initial object
        Object: 'Obj
        /// Unique morphism to any object
        UniqueMorphism: 'Obj -> 'Obj
    }

/// TERMINAL OBJECT - Universal sink
/// ================================
/// Object with unique morphism from every other object
type Terminal<'Obj> =
    {
        /// The terminal object
        Object: 'Obj
        /// Unique morphism from any object
        UniqueMorphism: 'Obj -> 'Obj
    }

/// PRODUCT - Universal construction for pairs
/// ==========================================
type Product<'A, 'B, 'P> =
    {
        /// Extract first component
        Fst: 'P -> 'A
        /// Extract second component
        Snd: 'P -> 'B
        /// Create product from components
        Create: 'A -> 'B -> 'P
    }

/// COPRODUCT - Universal construction for sums
/// ===========================================
type Coproduct<'A, 'B, 'S, 'C> =
    {
        /// Inject left value
        Left: 'A -> 'S
        /// Inject right value
        Right: 'B -> 'S
        /// Eliminate coproduct with case analysis
        Match: ('A -> 'C) -> ('B -> 'C) -> 'S -> 'C
    }

/// A semigroup is a type equipped with an associative combination operation.
type Semigroup<'a> = { Append: 'a -> 'a -> 'a }

module Semigroup =

    let append (semigroup: Semigroup<'a>) (x: 'a) (y: 'a) : 'a = semigroup.Append x y

    let list: Semigroup<'a list> = { Append = fun left right -> left @ right }

    let string: Semigroup<string> = { Append = fun left right -> left + right }

    let pair (first: Semigroup<'a>) (second: Semigroup<'b>) : Semigroup<'a * 'b> =
        { Append = fun (a1, b1) (a2, b2) -> first.Append a1 a2, second.Append b1 b2 }

// ==========================================
// MONOID
// Definition: (M, ⊕, e) with associativity and identity
// ==========================================

// Monoid type class
// -----------------
// A monoid is an algebraic structure consisting of:
// - A set M
// - A binary operation ⊕: M × M → M (associative)
// - An identity element e ∈ M
//
// Laws:
// - Associativity: (a ⊕ b) ⊕ c = a ⊕ (b ⊕ c)
// - Left Identity: e ⊕ a = a
// - Right Identity: a ⊕ e = a
type Monoid<'T> = { Empty: 'T; Combine: 'T -> 'T -> 'T }

// /// Monoid laws
// module MonoidLaws =
//     let leftIdentity (monoid: Monoid<'T>) (x: 'T) = monoid.Combine monoid.Empty x = x

//     let rightIdentity (monoid: Monoid<'T>) (x: 'T) = monoid.Combine x monoid.Empty = x

//     let associativity (monoid: Monoid<'T>) (x: 'T) (y: 'T) (z: 'T) =
//         monoid.Combine (monoid.Combine x y) z = monoid.Combine x (monoid.Combine y z)

module Monoid =

    /// Fold a sequence using a monoid
    let fold (monoid: Monoid<'T>) (xs: 'T seq) = Seq.fold monoid.Combine monoid.Empty xs

    /// Fold a sequence after mapping each element
    let foldMap (monoid: Monoid<'U>) (f: 'T -> 'U) (xs: 'T seq) = xs |> Seq.map f |> fold monoid

    let empty (monoid: Monoid<'a>) : 'a = monoid.Empty

    let append (monoid: Monoid<'a>) (x: 'a) (y: 'a) : 'a = monoid.Combine x y

    let combineAll (monoid: Monoid<'a>) (values: 'a seq) : 'a = values |> Seq.fold monoid.Combine monoid.Empty

    let fromSemigroup (emptyValue: 'a) (semigroup: Semigroup<'a>) : Monoid<'a> =
        { Empty = emptyValue
          Combine = semigroup.Append }

    let list: Monoid<'a list> = { Empty = []; Combine = (@) }

    let string: Monoid<string> = { Empty = ""; Combine = (+) }

    let sumInt: Monoid<int> = { Empty = 0; Combine = (+) }

    let productInt: Monoid<int> =
        { Empty = 1
          Combine = fun left right -> left * right }
