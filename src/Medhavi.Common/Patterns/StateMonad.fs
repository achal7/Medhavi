namespace Medhavi.Common.Patterns

// S -> (A × S) or S -> (A, S)
type State<'S, 'A> = State of ('S -> 'A * 'S)

module StateMonad =
    let return_ a = State(fun s -> (a, s))
    
    let bind (State sa) f =
        State(fun s ->
            let (a, s') = sa s
            let (State sb) = f a
            sb s')

    // Monad
    let (>>=) m f = bind m f
    
    // Functor
    let map f (State sa) =
        State(fun s ->
            let (a, s') = sa s
            (f a, s'))
    
    // Applicative
    let apply (State sf) (State sa) =
        State(fun s ->
            let (f', s1) = sf s
            let (a, s2) = sa s1
            (f' a, s2))

    let (<*>) = apply

    // Kleisli composition (fish operator >=>)
    let (>=>) (f: 'A -> State<'S, 'B>) (g: 'B -> State<'S, 'C>) : 'A -> State<'S, 'C> =
        fun a ->
            f a >>= g
    
    // Right-to-left Kleisli composition (<=<)
    let (<=<) (g: 'B -> State<'S, 'C>) (f: 'A -> State<'S, 'B>) : 'A -> State<'S, 'C> =
        f >=> g

    // Kleisli identity arrow
    let idKleisli : 'A -> State<'S, 'A> = return_

    let runState (State sa) initialState = sa initialState
    let getState = State(fun s -> (s, s))
    let putState s = State(fun _ -> ((), s))
    
     // Gets a function of the state
    let getsState (f: 'S -> 'A) : State<'S, 'A> =
        State(fun s -> (f s, s))
    
    // Evaluate a state computation, discarding the final state
    let evalState (State sa) initialState = 
        sa initialState |> fst
    
    // Execute a state computation, discarding the final value
    let execState (State sa) initialState = 
        sa initialState |> snd

    type StateBuilder() =
        member _.Return(x) = return_ x
        member _.Bind(m, f) = bind m f
        member _.ReturnFrom(m) = m
        member _.Zero() = return_ ()
        member _.Combine(m1, m2) = m1 >>= (fun () -> m2)
        //member _.Delay(f) = f()
        member _.Delay(f) = State(fun s -> runState (f()) s)

        member _.Run(m) = m
        member _.For(sequence: seq<'T>, body: 'T -> State<'S, unit>) : State<'S, unit> =
            State(fun s ->
                let mutable currentState = s
                for item in sequence do
                    let (_, newState) = runState (body item) currentState
                    currentState <- newState
                ((), currentState))
        member _.While(guard: unit -> bool, body: State<'S, unit>) : State<'S, unit> =
            State(fun s ->
                let mutable currentState = s
                while guard() do
                    let (_, newState) = runState body currentState
                    currentState <- newState
                ((), currentState))

// module Examples =
//     open StateMonad

//     let state = StateBuilder()
    
//     // Example 1: Simple counter operations
//     let increment : State<int, int> =
//         state {
//             let! current = getState
//             do! putState (current + 1)
//             return current + 1
//         }
    
//     let decrement : State<int, int> =
//         state {
//             let! current = getState
//             do! putState (current - 1)
//             return current - 1
//         }
    
//     let double : State<int, int> =
//         state {
//             let! current = getState
//             do! putState (current * 2)
//             return current * 2
//         }
    
//     // Convert to Kleisli arrows
//     let incrementK : unit -> State<int, int> = fun () -> increment
//     let decrementK : unit -> State<int, int> = fun () -> decrement
//     let doubleK : unit -> State<int, int> = fun () -> double
    
//     // Compose them
//     let complexCounterOp =
//         incrementK
//         >=> doubleK
//         >=> (fun x -> state { 
//             printfn "Intermediate value: %d" x
//             return! decrementK () 
//         })
    
//     // Run it
//     let counterResult = runState (complexCounterOp ()) 5
    
//     // Example 2: Stack operations
//     type Stack = int list
    
//     let push x : State<Stack, unit> =
//         State(fun stack -> ((), x :: stack))
    
//     let pop : State<Stack, int> =
//         State(function
//             | [] -> failwith "Empty stack"
//             | x :: xs -> (x, xs))
    
//     let peek : State<Stack, int> =
//         State(function
//             | [] -> failwith "Empty stack"
//             | x :: xs -> (x, x :: xs))
    
//     // Kleisli arrows for stack
//     let pushK x : unit -> State<Stack, int> =
//         fun () -> state {
//             do! push x
//             return x
//         }
    
//     let popK : unit -> State<Stack, int> = fun () -> pop
//     let peekK : unit -> State<Stack, int> = fun () -> peek
    
//     // Compose stack operations
//     let stackComputation =
//         pushK 10
//         >=> pushK 20
//         >=> popK
//         >=> (fun x -> pushK (x * 2))
//         >=> popK
//         >=> (fun result -> state {
//             let! stack = getState
//             return (result, stack)
//         })
    
//     let stackResult = runState (stackComputation ()) []

(*
type State<'S, 'A> = State of ('S -> 'A * 'S)

module StateMonad =
    // Run a state computation
    let runState (State sa) initialState = sa initialState

    // Core monad operations
    let returnS a = State(fun s -> (a, s))
    
    let bind (State sa) f =
        State(fun s ->
            let (a, s') = sa s
            let (State sb) = f a
            sb s')

    // Functor
    let map f (State sa) =
        State(fun s ->
            let (a, s') = sa s
            (f a, s'))

    // Applicative
    let apply (State sf) (State sa) =
        State(fun s ->
            let (f', s1) = sf s
            let (a, s2) = sa s1
            (f' a, s2))
    
    let (<*>) = apply
    
    // Monad
    let (>>=) m f = bind m f
    
    // Kleisli composition (fish operator >=>)
    let (>=>) (f: 'A -> State<'S, 'B>) (g: 'B -> State<'S, 'C>) : 'A -> State<'S, 'C> =
        fun a ->
            f a >>= g
    
    // Right-to-left Kleisli composition (<=<)
    let (<=<) (g: 'B -> State<'S, 'C>) (f: 'A -> State<'S, 'B>) : 'A -> State<'S, 'C> =
        f >=> g
    
    // Kleisli identity arrow
    let idKleisli : 'A -> State<'S, 'A> = returnS
    
    // Standard state operations
    let getState : State<'S, 'S> = 
        State(fun s -> (s, s))
    
    let putState newState : State<'S, unit> = 
        State(fun _ -> ((), newState))
    
    let modifyState (f: 'S -> 'S) : State<'S, unit> =
        State(fun s -> ((), f s))
    
    // Gets a function of the state
    let getsState (f: 'S -> 'A) : State<'S, 'A> =
        State(fun s -> (f s, s))
    
    // Evaluate a state computation, discarding the final state
    let evalState (State sa) initialState = 
        sa initialState |> fst
    
    // Execute a state computation, discarding the final value
    let execState (State sa) initialState = 
        sa initialState |> snd
    
    // **CORRECTED Computation Expression Builder**
    // The builder operates on State<'S, 'A> directly, not functions
    type StateBuilder() =
        member this.Return(x: 'A) : State<'S, 'A> = returnS x
        member this.Bind(m: State<'S, 'A>, f: 'A -> State<'S, 'B>) : State<'S, 'B> = 
            bind m f
        member this.ReturnFrom(m: State<'S, 'A>) = m
        member this.Zero() : State<'S, unit> = returnS ()
        member this.Combine(m1: State<'S, unit>, m2: State<'S, 'A>) : State<'S, 'A> = 
            bind m1 (fun () -> m2)
        member this.Delay(f: unit -> State<'S, 'A>) = f()
        member this.Run(f: unit -> State<'S, 'A>) = f()
        member this.For(sequence: seq<'T>, body: 'T -> State<'S, unit>) : State<'S, unit> =
            State(fun s ->
                let mutable currentState = s
                for item in sequence do
                    let (_, newState) = runState (body item) currentState
                    currentState <- newState
                ((), currentState))
        member this.While(guard: unit -> bool, body: State<'S, unit>) : State<'S, unit> =
            State(fun s ->
                let mutable currentState = s
                while guard() do
                    let (_, newState) = runState body currentState
                    currentState <- newState
                ((), currentState))

    let state = StateBuilder()

    // Examples of using the corrected builder
    module CorrectedExamples =
        // CORRECT: Direct State values
        let increment : State<int, int> =
            state {
                let! current = getState
                do! putState (current + 1)
                return current + 1
            }
        
        let decrement : State<int, int> =
            state {
                let! current = getState
                do! putState (current - 1)
                return current - 1
            }
        
        let double : State<int, int> =
            state {
                let! current = getState
                do! putState (current * 2)
                return current * 2
            }
        
        // Compose using Kleisli composition
        let incrementK : unit -> State<int, int> = fun () -> increment
        let decrementK : unit -> State<int, int> = fun () -> decrement
        let doubleK : unit -> State<int, int> = fun () -> double
        
        // Complex operation using Kleisli composition
        let complexOp =
            incrementK
            >=> doubleK
            >=> incrementK
            >=> (fun x -> 
                state {
                    printfn "Current value: %d" x
                    return x
                })
        
        // Test it
        let testComplexOp () =
            let result = runState (complexOp ()) 5
            printfn "Result: %A" result
        
        // Using For and While in computation expressions
        let repeatNTimes n (operation: State<int, unit>) : State<int, unit> =
            state {
                for i in 1..n do
                    do! operation
            }
        
        let countTo limit : State<int, unit> =
            state {
                let! current = getState
                while current < limit do
                    do! increment
            }
        
        // Complex example with all features
        let complexExample : State<int, string> =
            state {
                // Start with increment
                let! afterIncrement = increment
                
                // Use a loop
                for i in 1..3 do
                    do! modifyState (fun s -> s + i)
                
                // Use while
                do! countTo 20
                
                // Multiple operations
                do! repeatNTimes 2 (state {
                    do! modifyState (fun s -> s * 2)
                })
                
                // Final result
                let! final = getState
                return sprintf "Final state: %d" final
            }

    // Kleisli utilities module
    module Kleisli =
        // Lift a regular function to a Kleisli arrow
        let lift (f: 'A -> 'B) : 'A -> State<'S, 'B> =
            fun a -> returnS (f a)
        
        // Kleisli if-then-else
        let kleisliIf (cond: State<'S, bool>)
                      (thenBranch: State<'S, 'A>)
                      (elseBranch: State<'S, 'A>) : State<'S, 'A> =
            state {
                let! c = cond
                if c then return! thenBranch
                else return! elseBranch
            }
        
        // Create a Kleisli arrow from a state computation
        let fromState (comp: State<'S, 'A>) : unit -> State<'S, 'A> =
            fun () -> comp
        
        // Pipe Kleisli composition
        let (|>>) (value: 'A) (kleisli: 'A -> State<'S, 'B>) = kleisli value

    // More practical examples
    module PracticalExamples =
        // Stack operations
        type Stack = int list
        
        let push x : State<Stack, unit> =
            State(fun stack -> ((), x :: stack))
        
        let pop : State<Stack, int> =
            State(function
                | [] -> failwith "Empty stack"
                | x :: xs -> (x, xs))
        
        // Using computation expression
        let swap : State<Stack, unit> =
            state {
                let! a = pop
                let! b = pop
                do! push a
                do! push b
            }
        
        let duplicate : State<Stack, unit> =
            state {
                let! top = pop
                do! push top
                do! push top
            }
        
        // Calculator example
        let add : State<Stack, unit> =
            state {
                let! a = pop
                let! b = pop
                do! push (a + b)
            }
        
        let multiply : State<Stack, unit> =
            state {
                let! a = pop
                let! b = pop
                do! push (a * b)
            }
        
        // Compose calculator operations using Kleisli
        let calculateExpression =
            let pushK x = Kleisli.fromState (push x)
            let popK = Kleisli.fromState pop
            
            // Push 2, push 3, multiply, push 5, add
            pushK 2
            >=> pushK 3
            >=> (fun () -> multiply)
            >=> pushK 5
            >=> (fun () -> add)
            >=> (fun () -> 
                state {
                    let! result = pop
                    return result
                })

    // Verification tests
    module Tests =
        let testIncrement () =
            let (result, state') = runState CorrectedExamples.increment 5
            printfn "Increment from 5: Result=%d, NewState=%d" result state'
            // Should be: Result=6, NewState=6
        
        let testComplexOp () =
            let (result, state') = runState (CorrectedExamples.complexOp ()) 10
            printfn "Complex op from 10: Result=%d, NewState=%d" result state'
        
        let testForLoop () =
            let comp = CorrectedExamples.repeatNTimes 3 CorrectedExamples.increment
            let (_, finalState) = runState comp 0
            printfn "Repeat increment 3 times from 0: FinalState=%d" finalState
            // Should be: 3
        
        let testWhileLoop () =
            let comp = CorrectedExamples.countTo 5
            let (_, finalState) = runState comp 0
            printfn "Count to 5 from 0: FinalState=%d" finalState
            // Should be: 5
        
        let testStackCalculator () =
            let (result, finalStack) = runState PracticalExamples.calculateExpression []
            printfn "Calculator result: %d, Final stack: %A" result finalStack
            // Should be: 11 (2*3 + 5), stack: []

// Run all tests
let runTests () =
    printfn "=== Testing Corrected State Monad ===\n"
    
    printfn "1. Basic increment:"
    StateMonad.Tests.testIncrement()
    
    printfn "\n2. Complex operation:"
    StateMonad.Tests.testComplexOp()
    
    printfn "\n3. For loop:"
    StateMonad.Tests.testForLoop()
    
    printfn "\n4. While loop:"
    StateMonad.Tests.testWhileLoop()
    
    printfn "\n5. Stack calculator:"
    StateMonad.Tests.testStackCalculator()
    
    printfn "\n6. Testing Kleisli composition directly:"
    
    // Direct Kleisli composition test
    let f x = StateMonad.state { return x * 2 }
    let g x = StateMonad.state { return x + 1 }
    
    let composed = f >=> g
    let (result, state) = StateMonad.runState (composed 5) 0
    printfn "f(5)=10, then g(10)=11: Result=%d, State=%d" result state

// Alternative approach: If you want the original error-free version
// Here's a simpler implementation without the computation expression issues:

module SimpleStateMonad =
    type State<'S, 'A> = State of ('S -> 'A * 'S)
    
    let run (State f) = f
    let result = run >> fst
    let nextState = run >> snd
    
    let ret a = State(fun s -> (a, s))
    
    let bind m f =
        State(fun s ->
            let (a, s') = run m s
            run (f a) s')
    
    let (>>=) = bind
    
    // Kleisli composition
    let (>=>) f g = fun x -> f x >>= g
    
    // Basic operations
    let get = State(fun s -> (s, s))
    let put s = State(fun _ -> ((), s))
    let modify f = State(fun s -> ((), f s))
    
    // Usage examples without computation expressions
    let increment =
        get >>= fun current ->
        put (current + 1) >>= fun () ->
        ret (current + 1)
    
    // Using Kleisli composition
    let incrementK = fun () -> increment
    let doubleK = fun () -> 
        get >>= fun current ->
        put (current * 2) >>= fun () ->
        ret (current * 2)
    
    let composed = incrementK >=> doubleK >=> incrementK
    
    let test () =
        let (value, state) = run (composed ()) 5
        printfn "Value: %d, State: %d" value state
        // Should be: Value: 13, State: 13
*)