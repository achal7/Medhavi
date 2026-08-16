namespace Medhavi.Common

/// STORE COMONAD - Context-dependent values
/// The Store comonad represents a value and its context (environment)
/// It captures computations that depend on their surrounding context
type Store<'S, 'A> =
    {
        /// Current state/context
        State: 'S

        /// Function to get value from any state
        Get: 'S -> 'A
    }

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
          Get = fun s -> f(store.Get s) }

    /// Create a new store with initial state and value function
    let create (initialState: 'S) (getValue: 'S -> 'A) = { State = initialState; Get = getValue }

    /// Update the current state of a store
    let setState (newState: 'S) (store: Store<'S, 'A>) = { store with State = newState }

/// Env comonad: fixed context + value
type Env<'E, 'A> = { Env: 'E; Value: 'A }

module Env =
    let map f w = { w with Value = f w.Value }

    let extract w = w.Value

    let duplicate w = { Env = w.Env; Value = w }

    let extend f w = { Env = w.Env; Value = f w }

/// List zipper
type Zipper<'A> =
    { Left: 'A list
      Focus: 'A
      Right: 'A list }

module Zipper =
    let map f z =
        { Left = List.map f z.Left
          Focus = f z.Focus
          Right = List.map f z.Right }

    let extract z = z.Focus

    let moveLeft z =
        match z.Left with
        | h :: t ->
            Some
                { Left = t
                  Focus = h
                  Right = z.Focus :: z.Right }
        | [] -> None

    let moveRight z =
        match z.Right with
        | h :: t ->
            Some
                { Left = z.Focus :: z.Left
                  Focus = h
                  Right = t }
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

        { Left = lefts [] z
          Focus = z
          Right = rights [] z }

    let extend f z = map f (duplicate z)

/// Infinite stream
type Stream<'A> = Cons of 'A * (unit -> Stream<'A>)

module Stream =
    let head (Cons(h, _)) = h
    let tail (Cons(_, t)) = t()

    let rec map f (Cons(h, t)) = Cons(f h, (fun () -> map f (t())))

    let extract = head

    let rec duplicate s = Cons(s, (fun () -> duplicate(tail s)))

    let extend f s = map f (duplicate s)
