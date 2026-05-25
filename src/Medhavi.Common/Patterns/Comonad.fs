namespace Medhavi.Common.Patterns

(*
// ==========================================
// COMONAD
// Definition: Dual of monad, with extract, duplicate, and extend operations
// ==========================================

/// Comonad type class
/// ------------------
/// A comonad is the dual of a monad in category theory. It consists of:
/// - A functor F
/// - Extract operation: F A → A (get value from context)
/// - Duplicate operation: F A → F (F A) (create context containing context)
/// - Extend operation: (F A → B) → F A → F B (apply context-dependent function)

type Comonad<'F, 'T> =
    {
        /// Extract a value from its context
        Extract: 'F<'T> -> 'T
        
        /// Create a context that contains the original context
        Duplicate: 'F<'T> -> 'F<'F<'T>>
        
        /// Apply a context-dependent function
        Extend: ('F<'T> -> 'U) -> 'F<'T> -> 'F<'U>
        
        /// Map a function over the comonad
        Map: ('T -> 'U) -> 'F<'T> -> 'F<'U>
    }

/// Comonad laws
module ComonadLaws =
    /// Law 1: extract (duplicate w) = w
    let extractDuplicate (comonad: Comonad<'F, 'T>) (w: 'F<'T>) =
        comonad.Extract (comonad.Duplicate w) = w

    /// Law 2: map extract (duplicate w) = w
    let mapExtractDuplicate (comonad: Comonad<'F, 'T>) (w: 'F<'T>) =
        comonad.Map comonad.Extract (comonad.Duplicate w) = w

    /// Law 3: duplicate (duplicate w) = map duplicate (duplicate w)
    let duplicateAssociativity (comonad: Comonad<'F, 'T>) (w: 'F<'T>) =
        comonad.Duplicate (comonad.Duplicate w) =  comonad.Map comonad.Duplicate (comonad.Duplicate w)

*)
// ==========================================
// STORE COMONAD - Context-dependent values
// ==========================================

/// The Store comonad represents a value and its context (environment)
/// It captures computations that depend on their surrounding context
type Store<'S, 'A> = 
    {
        /// Current state/context
        State: 'S
        
        /// Function to get value from any state
        Get: 'S -> 'A
    }

/// Store comonad implementation
module Store =
    /// Extract value from current state
    let extract (store: Store<'S, 'A>) = store.Get store.State

    /// Duplicate a store - creates a store that contains the original store
    let duplicate (store: Store<'S, 'A>) =
        { State = store.State
          Get = fun s -> { State = s; Get = store.Get } }

    /// Extend a store with a context-dependent function
    let extend (f: Store<'S, 'A> -> 'B) (store: Store<'S, 'A>) =
        { State = store.State
          Get = fun s -> f { State = s; Get = store.Get } }

    /// Map a function over the store
    let map (f: 'A -> 'B) (store: Store<'S, 'A>) =
        { State = store.State
          Get = fun s -> f (store.Get s) }

    /// Create a new store with initial state and value function
    let create (initialState: 'S) (getValue: 'S -> 'A) =
        { State = initialState
          Get = getValue }

    /// Update the current state of a store
    let setState (newState: 'S) (store: Store<'S, 'A>) =
        { store with State = newState }

// let timeline p =
//     match p with
//     | 0 -> 80
//     | 1 -> 100
//     | 2 -> 70
//     | _ -> 90

// let feasible (ctx : Store<int,int>) =
//     let now  = ctx.Get ctx.State
//     let next = ctx.Get (ctx.State + 1)
//     now + next <= 180
// let store =
//     {
        
//         Get = timeline
//         State = 1
//     }
// let result = Store.extend feasible store


/// Env comonad: fixed context + value
type Env<'E,'A> =
    { Env : 'E
      Value : 'A }

module Env =
    let map f w =
        { w with Value = f w.Value }

    let extract w =
        w.Value

    let duplicate w =
        { Env = w.Env
          Value = w }

    let extend f w =
        { Env = w.Env
          Value = f w }

/// List zipper
type Zipper<'A> =
    { Left  : 'A list
      Focus : 'A
      Right : 'A list }

module Zipper =
    let map f z =
        { Left  = List.map f z.Left
          Focus = f z.Focus
          Right = List.map f z.Right }

    let extract z =
        z.Focus

    let moveLeft z =
        match z.Left with
        | h :: t -> Some { Left = t; Focus = h; Right = z.Focus :: z.Right }
        | [] -> None

    let moveRight z =
        match z.Right with
        | h :: t -> Some { Left = z.Focus :: z.Left; Focus = h; Right = t }
        | [] -> None

    let rec duplicate z =
        let rec lefts acc current =
            match moveLeft current with
            | Some l -> lefts (l :: acc) l
            | None -> acc

        let rec rights acc current =
            match moveRight current with
            | Some r -> rights (r :: acc) r
            | None -> acc

        { Left  = lefts [] z
          Focus = z
          Right = rights [] z }

    let extend f z =
        map f (duplicate z)

/// Infinite stream
type Stream<'A> =
    Cons of 'A * (unit -> Stream<'A>)

module Stream =
    let head (Cons (h,_)) = h
    let tail (Cons (_,t)) = t()

    let rec map f (Cons (h,t)) =
        Cons (f h, fun () -> map f (t()))

    let extract =
        head

    let rec duplicate s =
        Cons (s, fun () -> duplicate (tail s))

    let extend f s =
        map f (duplicate s)


// // ==========================================
// // RESOURCE CONTEXT COMONAD - Supply chain specific
// // ==========================================

// /// Resource context for capacity planning
// type ResourceContext<'R, 'A> = 
//     {
//         /// Resource identifier
//         Resource: 'R
        
//         /// Available capacity
//         Capacity: decimal
        
//         /// Value associated with the resource context
//         Value: 'A
//     }

// /// Resource context comonad implementation
// module ResourceContext =
//     /// Extract value from resource context
//     let extract (ctx: ResourceContext<'R, 'A>) = ctx.Value

//     /// Duplicate a resource context
//     let duplicate (ctx: ResourceContext<'R, 'A>) =
//         { Resource = ctx.Resource
//           Capacity = ctx.Capacity
//           Value = ctx }

//     /// Extend a resource context with a context-dependent function
//     let extend (f: ResourceContext<'R, 'A> -> 'B) (ctx: ResourceContext<'R, 'A>) =
//         { Resource = ctx.Resource
//           Capacity = ctx.Capacity
//           Value = f ctx }

//     /// Map a function over the resource context
//     let map (f: 'A -> 'B) (ctx: ResourceContext<'R, 'A>) =
//         { ctx with Value = f ctx.Value }

//     /// Create a new resource context
//     let create (resource: 'R) (capacity: decimal) (value: 'A) =
//         { Resource = resource
//           Capacity = capacity
//           Value = value }

//     /// Check if required quantity fits in available capacity
//     let canFit (required: decimal) (ctx: ResourceContext<'R, 'A>) =
//         ctx.Capacity >= required