namespace Medhavi.Common.Patterns

type Monoid<'W> =
    { Empty : 'W
      Append : 'W -> 'W -> 'W }

// Theory: M w a = (a × w) with monoid w
// η a = (a, ε) where ε is monoid identity
// μ f = λ(a, w) → let (b, w') = f a in (b, w ⊕ w')

// Functor:T A = A * W
// Unit (η): η a = (a, ε)
// Join (μ): μ ((a, w₁), w₂) = (a, w₁ ⊕ w₂)
// Bind: bind m f = μ (fmap f m)
// Where (W, ⊕, ε) is a monoid.

//T(A)=A×W
type Writer<'Log, 'A> = Writer of ('A * 'Log)

module Writer =
    let run (Writer (value, log)) = (value, log)
    
    // return/η
    // η(a)=(a,ε)
    let return' m x = Writer (x, m.Empty)
    
    // fmap f(a,w)=(f(a),w)
    // fmap : (A → B) → Writer W A → Writer W B
    let map (f: 'A -> 'B) (Writer (a, w)) =
        Writer (f a, w)
    
    // μ : Writer W (Writer W A) → Writer W A
    let join m (Writer (Writer (a, w1), w2)) =
        Writer (a, m.Append w1 w2)

    // bind = μ ∘ fmap
    // bind mf=μ(fmap fm)
    let bind m (Writer (a, w1)) (f: 'A -> Writer<'W,'B>) =
        let (b, w2) = run (f a)
        Writer (b, m.Append w1 w2)
    
    // Writer-specific: tell/add to log
    let tell msg = Writer ((), msg)

    let (>>=) m f = bind m f
    let (<!>) f m = map f m

    
    // Listen to log
    let listen (Writer (a, log)) = Writer ((a, log), log)

type WriterBuilder<'W>(mo: Monoid<'W>) =
    member _.Return(x) = Writer.return' mo x
    member _.Bind(m, f) = Writer.bind mo m f
    member _.ReturnFrom(mw) = mw
    //member _.Zero() = Writer.return' ()


// let logMonoid : Monoid<string list> =
//     { Empty = []
//       Append = (@) }

// let writer = WriterBuilder<string list>(logMonoid)
// let logMonoid : Monoid<string list> =
//     { Empty = []
//       Append = (@) }

// let writer = WriterBuilder<string list>(logMonoid)

// let tell msg = Writer ((), [msg])

// let computation =
//     writer {
//         do! tell "start"
//         let! x = writer { return 10 }
//         do! tell "end"
//         return x * 2
//     }

// let result, log = Writer.run computation
