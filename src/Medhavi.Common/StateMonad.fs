namespace Medhavi.Common

/// S -> (A × S) or S -> (A, S)
type State<'S, 'A> = State of ('S -> 'A * 'S)

module StateMonad =

    let return_ a = State(fun s -> (a, s))

    let bind (State sa) f =
        State(fun s ->
            let a, s' = sa s
            let (State sb) = f a
            sb s')

    let (>>=) m f = bind m f

    let map f (State sa) =
        State(fun s ->
            let a, s' = sa s
            f a, s')

    let apply (State sf) (State sa) =
        State(fun s ->
            let f', s1 = sf s
            let a, s2 = sa s1
            f' a, s2)

    let (<*>) = apply

    let (>=>) (f: 'A -> State<'S, 'B>) (g: 'B -> State<'S, 'C>) : 'A -> State<'S, 'C> = fun a -> f a >>= g

    let (<=<) (g: 'B -> State<'S, 'C>) (f: 'A -> State<'S, 'B>) : 'A -> State<'S, 'C> = f >=> g

    let idKleisli: 'A -> State<'S, 'A> = return_

    let runState (State sa) initialState = sa initialState

    let getState = State(fun s -> (s, s))

    let putState s = State(fun _ -> ((), s))

    let getsState (f: 'S -> 'A) : State<'S, 'A> = State(fun s -> (f s, s))

    let evalState (State sa) initialState = sa initialState |> fst

    let execState (State sa) initialState = sa initialState |> snd

type StateBuilder() =

    member _.Return(x) = StateMonad.return_ x

    member _.Bind(m, f) = StateMonad.bind m f

    member _.ReturnFrom(m) = m

    member _.Zero() = StateMonad.return_()

    member _.Combine(m1, m2) = StateMonad.bind m1 (fun () -> m2)

    member _.Delay(f) = State(fun s -> StateMonad.runState (f()) s)

    member _.Run(m) = m

    member _.For(sequence: seq<'T>, body: 'T -> State<'S, unit>) : State<'S, unit> =
        State(fun s ->
            let finalState =
                sequence
                |> Seq.fold
                    (fun currentState item ->
                        let _, nextState = StateMonad.runState (body item) currentState
                        nextState)
                    s

            (), finalState)

    member _.While(guard: unit -> bool, body: State<'S, unit>) : State<'S, unit> =
        let rec loop currentState =
            if guard() then
                let _, nextState = StateMonad.runState body currentState
                loop nextState
            else
                (), currentState

        State loop
