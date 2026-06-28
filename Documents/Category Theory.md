# Medhāvī Production Planning: Category-Theoretic Foundations & Functional Architecture

This document serves as the unified architectural reference, concept dictionary, and coding standard for the **Medhāvī Production Planning** engine. It establishes a pure, mathematical foundation using Category Theory and Domain-Driven Design (DDD) in F# (.NET 10), strictly adhering to the **Functional Programming Only** constraint (no OOP classes, no interfaces).

---

## 1. Core Philosophy & F# Guidelines

### A. The "Lawful but Concrete" Principle
F# does not support native **Higher-Kinded Types (HKTs)** at the type level. Trying to model abstract interfaces like `Functor<'F>` or `Monad<'M>` leads to complex, slow, and unidiomatic F# code relying on reflection or boilerplate interfaces.
* **The Rule**: Write **concrete type-specific** implementations of maps, binds, and composition functions (e.g., `Result.map`, `Option.bind`) inside specific modules.
* **The Constraint**: All functions must strictly satisfy mathematical category-theoretic laws (e.g., identity, associativity, functor and monad laws), even if the compiler cannot enforce them generically.

### B. Decoupling Without Interfaces
Decoupling and dependency injection are handled strictly through functional techniques:
1. **Parameter Records (Capabilities)**: Group dependency functions into simple F# records.
2. **Partial Application**: Inject dependencies at startup by partially applying capability functions.
3. **First-Class Functions**: Pass helper or lookup functions directly as parameters.

---

## 2. Concept-by-Concept Reference Guide (The 44 Topics)

Here is the complete dictionary mapping all 42 category-theoretic topics from the original planning notes, plus our 2 newly researched extensions, into SCM planning concepts and F# coding standards.

### 1. Functions & Composition
* **SCM Role**: A business rule or transformation (e.g., `applySafetyStockCover`).
* **F# Rule**: Modeled as pure functions compiled with the piping (`|>`) or composition (`>>`) operators.

### 2. Categories
* **SCM Role**: The mathematical universe of SCM types (objects) and transformations (arrows).
* **F# Rule**: Represented by the overall module structure, ensuring type inputs and outputs align.

### 3. Morphisms
* **SCM Role**: Structure-preserving arrows (e.g., `Order -> PlannedOrder`).
* **F# Rule**: Explicitly named functions translating one domain type to another.

### 4. Identity
* **SCM Role**: The "no-op" rule (e.g., no allocation rules matching leaves the supply plan unchanged).
* **F# Rule**: Built-in F# `id` function, used as a default or fallback step in pipelines.

### 5. Associativity
* **SCM Role**: Grouping of pipeline steps. Chaining `(validate >> explode) >> net` must yield the same result as `validate >> (explode >> net)`.
* **F# Rule**: Enforced by ensuring functions do not depend on external mutable state.

### 6. Product & Coproduct
* **SCM Role**: "And" vs. "Or" data structures. Product combines fields; Coproduct models choices or states.
* **F# Rule**: Product is modeled as a record (e.g., `{ Sku: SkuId; Qty: int }`); Coproduct is modeled as a Discriminated Union (e.g., `type SupplyDecision = Accept of int | Reject of string`).

### 7. Initial & Terminal Objects
* **SCM Role**: Initial is the "impossible" type (`absurd` / empty set). Terminal represents a unique trivial result (`unit`).
* **F# Rule**: `unit` (written as `()`) signals command completions or no-op side effects.

### 8. Functors
* **SCM Role**: Applying a morphism inside a container context (e.g., modifying quantities inside an `Option` or `Result` without unpacking it).
* **F# Rule**: Concrete `map` functions (e.g., `Option.map`, `Result.map`).

### 9. Natural Transformations
* **SCM Role**: Converting structural contexts without touching the payload (e.g., translating an optional demand `Option<Demand>` to a list of demands `Demand list`).
* **F# Rule**: Safe structure-converting functions: `Option.toList`, `Result.toOption`.

### 10. Applicatives
* **SCM Role**: Combining independent contexts (e.g., validating Sku, Plant, and Qty independently and accumulating all errors in parallel).
* **F# Rule**: Standard F# `<*>` and `<!>` custom operators over `Result<'T, 'Error list>`.

### 11. Monads
* **SCM Role**: Sequencing dependent operations where step B requires the result of step A (e.g., query inventory only after checking demand validity).
* **F# Rule**: Concrete `bind` operations and computation expressions (`async`, `task`, `result`).

### 12. Kleisli Categories
* **SCM Role**: The category of monadic functions of shape `A -> M<B>`.
* **F# Rule**: Composition of functions returning `Result`, `Option`, or `Async` using monadic bind operators.

### 13. Monoids
* **SCM Role**: Accumulating values with a safe starting point (e.g., summing total demand over a timeline starting from 0).
* **F# Rule**: A record containing a `Zero` element and a associative `Combine` function.

### 14. Semigroups
* **SCM Role**: Combining values where no natural zero exists (e.g., combining multiple validation error lists).
* **F# Rule**: An associative `Combine` function (like list concatenation `@`).

### 15. Foldable / Traversable
* **SCM Role**: Foldable collapses a collection into a summary (e.g., folding events into state). Traversable sequences effects across a collection (e.g., executing validation across a list of orders to return a single `Result<List, Errors>`).
* **F# Rule**: `List.fold` (for folds) and custom `traverse` functions over results.

### 16. Contravariant Functors
* **SCM Role**: Pre-processing inputs to a consumer or predicate.
* **F# Rule**: Mapping inputs prior to validation checks or comparers (e.g., sorting orders by dueDate before running allocation).

### 17. Profunctors
* **SCM Role**: Input-output adapters. Consuming inputs (contravariant) and producing outputs (covariant).
* **F# Rule**: Represented by pipeline adapters mapping external API inputs to internal commands and outputs back.

### 18. Bifunctors
* **SCM Role**: Mapping over two-sided structures independently (e.g., mapping success data or error data).
* **F# Rule**: `Result.map` (covariant success) and `Result.mapError` (covariant error).

### 19. Monoidal Categories
* **SCM Role**: Parallel execution capabilities in SCM workflows (e.g., executing MRP netting on Plant A and Plant B in parallel).
* **F# Rule**: Represented by using F#'s `Async.Parallel` to tensor concurrent computations.

### 20. Cartesian Closed Categories
* **SCM Role**: High-order functions are treated as first-class domain values.
* **F# Rule**: Functions can be passed as parameters, returned from functions, or stored in record properties (Capabilities).

### 21. Adjunctions
* **SCM Role**: The relationship between syntax generators (Free) and evaluation solvers (Forgetful).
* **F# Rule**: Translating a declarative demand tree into a simplified linear list of solver constraints.

### 22. Free Monads
* **SCM Role**: Describing SCM workflows as pure data structures (ASTs) to be interpreted later.
* **F# Rule**: Model workflows as recursive Discriminated Unions and process them via pattern-matching interpreters.

### 23. Comonads
* **SCM Role**: Neighborhood-based calculations (e.g., demand forecasts that depend on the surrounding sliding window of periods).
* **F# Rule**: Context-focused sliding window traversals over timelines.

### 24. Yoneda Lemma
* **SCM Role**: Defining a SCM entity by how it interacts with the rest of the system rather than its internal database representation.
* **F# Rule**: Querying objects via observation functions rather than exposing raw record fields.

### 25. Limits & Colimits
* **SCM Role**: Data synchronization. Pullbacks (limits) model constrained joins (matching demand to supply on matching SKU/Plant). Pushouts (colimits) model data gluing (unioning stock updates from multiple warehouse feeds).
* **F# Rule**: Enforced in domain service functions using key matching and list union joins.

### 26. Kan Extensions
* **SCM Role**: The best possible extension of a calculation across a new structure (e.g., rolling up SKU-level planning parameters to Product Family aggregates).
* **F# Rule**: Generalizing projection logic from fine-grained data to coarse-grained abstractions.

### 27. Algebraic Structures
* **SCM Role**: Mathematical frameworks governing business numbers.
* **F# Rule**: Enforcing properties of semirings, rings, or groups in value objects.

### 28. Optics (Lens, Prism, Traversal)
* **SCM Role**: Accessing and updating fields deep within nested data structures without boilerplate.
* **F# Rule**: Define record lenses (getter/setter records) rather than complex HKT lens libraries to maintain fast compilation and performance.

### 29. Recursive Schemes
* **SCM Role**: Handling hierarchical trees like multi-level Bills of Materials (BOM).
* **F# Rule**: Using tail-recursive Catamorphisms (folds) to avoid stack overflows.

### 30. State Machines (FSM)
* **SCM Role**: Lifecycle tracking (e.g., Order: Draft -> Approved -> Allocated -> Shipped).
* **F# Rule**: FSM states modeled as Discriminated Unions where transition functions output `Result<NextState, Error>`.

### 31. Event Sourcing as Category Theory (Algebras)
* **SCM Role**: Event Sourcing. Deriving state by folding historical facts (Algebras: $F(S) \to S$).
* **F# Rule**: Rebuilding aggregate state via `List.fold evolve initialState events`.

### 32. DDD Aggregates Categorically
* **SCM Role**: Boundaries protecting invariants.
* **F# Rule**: Modeled as a tuple of: Initial State, Command Handler (`State -> Command -> Result<Event list, Error>`), and Evolver fold function.

### 33. Functional Architecture Patterns
* **SCM Role**: Pure Core / Impure Shell. Isolating business decisions from DB/IO.
* **F# Rule**: Domain modules are 100% pure; IO is handled at the entry points via Application workflows.

### 34. Effect Systems
* **SCM Role**: Explicit representation of side effects.
* **F# Rule**: Effects are made visible in the function return types (e.g., returning `Async<Result<'T, 'E>>`).

### 35. Tagless Final
* **SCM Role**: Abstracting operations over multiple runtimes.
* **F# Rule**: Passing records of capability functions representing the required effects.

### 36. Arrows
* **SCM Role**: Composable pipeline processors with multiple inputs and outputs.
* **F# Rule**: Writing staged pipelines where computations pass tuples containing both data and processing metadata.

### 37. Monoids in Endofunctor Categories
* **SCM Role**: The mathematical definition of a Monad (mapping $T \times T \to T$ and $I \to T$).
* **F# Rule**: The law of `bind` and `return` composing cleanly within our concrete contexts.

### 38. Higher-Kinded Structure Intuition
* **SCM Role**: Thinking in type constructors (`M<_>`) rather than concrete types.
* **F# Rule**: Designing reusable module patterns for map/bind operations even when HKT syntax is unavailable.

### 39. Type-Level Reasoning
* **SCM Role**: Separating validated data from raw inputs (e.g., preventing a raw demand from being planned until validated).
* **F# Rule**: Defining private constructors inside records and exposing smart validation functions returning `Result<ValidatedDemand, Error>`.

### 40. Algebraic Effects Intuition
* **SCM Role**: Describing operations as syntax trees and handling them via modular interpreters.
* **F# Rule**: Handled by passing Capability records to pure domain workflows.

### 41. Coalgebras (Simulation & Projections)
* **SCM Role**: Simulating inventory horizons and projections day-by-day (unfolding state: $S \to F(S)$).
* **F# Rule**: Time-phased netting simulation functions yielding observations and the next period's state.

### 42. Fixed Points
* **SCM Role**: Iterative planning convergence (e.g., repeating allocation logic until supply constraints stabilize).
* **F# Rule**: Running recursive calculation loops that terminate when the delta between runs is zero.

### 43. Tropical (Min-Plus) Semirings
* **SCM Role**: Finding minimum-cost or minimum-lead-time paths across supply chain routing graphs.
* **F# Rule**: Implementing addition as `min` and multiplication as `+`.

### 44. Markov Categories
* **SCM Role**: Modeling lead-time variability and yield losses.
* **F# Rule**: Represented using stochastic sampler functions of type `unit -> 'T`.

---

## 3. Layers

A few of the concepts are **cross-cutting** rather than living in just one place. In particular, **algebraic structures**, **higher-kinded intuition**, **type-level reasoning**, **limits/colimits**, and **Kan extensions** sit mostly in the architecture/interpretation layer, but they also influence the lower layers. That is normal; category theory concepts often overlap.

Here is the process to apply category theory in SCM:

### 3.1 Layer A — Data shape

Answer: **What are the things?**
Here you use:
* Product & coproduct
* Initial & terminal objects
* Algebraic data types as the domain shape foundation

**Safety note:** this layer should define only the business nouns and state shapes. No orchestration, no IO, no planning logic.

### 3.2 Layer B — Pure transformations

Answer: **How do things change?**

Here you use:
* Functions & composition
* Categories
* Morphisms
* Identity
* Associativity
* Monoids
* Semigroups

**Safety note:** this layer should stay pure and deterministic. If a function depends on DB, time, or external systems, it does not belong here.

### 3.3 Layer C — Context and effects

Answer: **What happens when the logic is inside Option, Result, Async, Seq, List, or State?**

Here you use:

* Functors
* Natural transformations
* Applicatives
* Monads
* Kleisli categories
* Foldable / Traversable
* Contravariant functors
* Bifunctors
* Profunctors

**Safety note:** in F#, some of these are not surfaced as typeclasses, but the design intuition still applies through Option, Result, List, Seq, Async, and computation expressions.

### 3.4 Layer D — Architecture and interpretation

Answer: **How do we separate business syntax from runtime interpretation?**

Here you use:

* Monoidal categories
* Cartesian closed categories
* Adjunctions
* Free monads
* Comonads
* Yoneda lemma
* Limits & colimits
* Kan extensions
* Algebraic structures
* Optics (Lens, Prism, Traversal)
* Functional architecture patterns
* Effect systems
* Tagless final
* Arrows
* Monoids in endofunctor categories
* Higher-kinded structure intuition
* Type-level reasoning
* Algebraic effects intuition

### 3.5 Layer E — Evolution over time

Answer: **How does the system move from one state to another?**

Here you use:

* Recursive schemes
* State machines
* Event sourcing through category theory
* DDD aggregates interpreted categorically
* Coalgebras
* Fixed points

**Safety note:** this layer is where state evolution and replay live. It should be explicit and testable.

### The thinking pattern you should use

When designing any SCM rule, ask these questions in order:

1. What is the domain noun?

Example: InventoryPosition

2. Is it a product, coproduct, or scalar?
Product: many fields together
Coproduct: one of several cases
Scalar: single value with meaning
3. Is this a transformation?

If yes, make it a function.

4. Does the next step depend on the previous result?
No → use functor/applicative style
Yes → use monad/Kleisli style
5. Is this accumulation?

Use a monoid and fold.

6. Is this a projection over a structure?

Use optics.

7. Is this state evolution over time?

Use a state machine, coalgebra, or event-sourced fold.

8. Is this business logic or interpretation?

Keep the logic pure; push interpretation to the edge.

### The core principles to keep repeating

#### Principle 1: Composition over mutation

Build everything so it can be chained.

#### Principle 2: Types first

Let the type encode the domain shape.

#### Principle 3: Separate decision from execution

Pure logic should not know about persistence or messaging.

#### Principle 4: Make invariants explicit

Use smart constructors and state-specific types.

#### Principle 5: Prefer lawful abstractions

If something behaves like a monoid, functor, or monad, model it that way.

#### Principle 6: Start concrete, generalize later

Do not begin with the most advanced abstraction. Earn it from the domain.


## 4. Layer A: Core Domain Types

This layer is about answering:

**What are the things in the domain, and what shapes can they take?**

In SCM, this is where you define the nouns:

* demand
* inventory
* shipment
* order
* plant
* SKU
* period
* BOM node
* allocation
* exception

This is the layer where you want the model to feel **obvious, safe, and explicit**.

### 4.1) Product types

A product type means “these things together.”

In F# that is usually:

* a record
* a tuple

Example:

```fsharp
type SkuId = SkuId of string
type PlantId = PlantId of string
type OrderId = OrderId of string
type ShipmentId = ShipmentId of string

type DemandSource =
    | SalesOrder of OrderId * customerId: string
    | Forecast of forecastId: string
    | SafetyStock
    | StockTransfer of sourcePlant: PlantId

type RawDemand =
    { Sku: SkuId
      Plant: PlantId
      Qty: int
      DueDate: DateTime
      Source: DemandSource }

type ValidatedDemand =
    private { VRaw: RawDemand; VPriority: int }
    with
        member this.Sku = this.VRaw.Sku
        member this.Plant = this.VRaw.Plant
        member this.Qty = this.VRaw.Qty
        member this.DueDate = this.VRaw.DueDate
        member this.Source = this.VRaw.Source
        member this.Priority = this.VPriority

type DemandLine =
    { SkuId: SkuId
      Period: int
      Qty: int }

type InventoryRecord =
    { SkuId: SkuId
      Period: int
      OnHand: int
      Reserved: int }

type InventoryEvent =
    | StockReceived of qty: int
    | StockIssued of qty: int
    | StockReserved of qty: int
    | ReservationReleased of qty: int

type ShipmentStatus =
    | Draft
    | Planned of scheduledShipDate: DateTime
    | Dispatched of actualShipDate: DateTime
    | Delivered of deliveryDate: DateTime
    | Cancelled of reason: string

type Shipment =
    { ShipmentId: ShipmentId
      Sku: SkuId
      Source: PlantId
      Destination: PlantId
      Qty: int
      Status: ShipmentStatus }

type BomComponent =
    { ComponentSku: SkuId
      QtyPerParent: decimal
      LeadTimeOffsetDays: int }

type BomSpecification =
    { ParentSku: SkuId
      Components: BomComponent list }

type BomGraph = Map<SkuId, BomComponent list>
```

#### Thinking

Use a product when the concept is naturally “and also.”

#### SCM example

A shipment request may need both a `PlantId` and a `SkuId` and a `Qty`.

---

### 4.2) Coproduct types

A coproduct means “one of these cases.”

In F# that is usually a discriminated union.

Example:

```fsharp
type PlanResult =
    | Planned of shipmentCount: int
    | Rejected of reasons: string list
    | NeedsReview of message: string
```

#### Thinking

Use a coproduct when the domain says “either this case or that case.” It prevents illegal ambiguity. You do not force every state into one record shape.

#### SCM example

An order is either:

* draft
* approved
* allocated
* shipped
* cancelled

---

### 4.3) Initial object

This is the “empty starting point” idea.

In practice, for F# domain design, you use this intuition when there is no meaningful value to carry yet.

Examples:

* empty list
* a workflow step that returns `unit`
* an impossible branch in a domain model

Example:

```fsharp
let markComplete () : unit = ()
```

#### Thinking

Use it when the domain has “nothing more to say.”

#### SCM example

A notification step that exists only to trigger a side effect may return `unit`.

---

#### 4.4) Terminal object

This is the “unique trivial result” idea.

In F#, `unit` is the closest everyday example.

#### Thinking

Use it when the result is intentionally irrelevant and you only care that something happened.

#### SCM example

A projection update that applies a change and returns no business payload.

---

### 4.5) Why Layer A comes first

Because if the shape is wrong, everything built on top becomes harder.

If you model:

* a state as a record when it is actually one of many cases,
* a sum as a record when it should be a union,
* optional information as a required field,

then later layers become awkward and error-prone.

So Layer A is where you protect the domain from bad structure.

---

### 4.6) Design principles for Layer A

#### Principle 1: Prefer explicit domain shapes

Do not let generic primitive types hide meaning.

Bad:

```fsharp
type Order = string * string * int
```

Better:

```fsharp
type Order =
    { OrderId: string
      CustomerId: string
      Qty: int }
```

#### Principle 2: Use unions for states and alternatives

If something can be different valid cases, do not flatten it.

#### Principle 3: Model absence honestly

If something may be missing, do not force fake values.

#### Principle 4: Keep invalid states unrepresentable

This is one of the strongest DDD benefits of F#.

---

### 4.7) SCM examples for Layer A

#### Order lifecycle

```fsharp
type OrderStatus =
    | Draft
    | Submitted
    | Allocated
    | PartiallyShipped
    | Shipped
    | Cancelled
```

#### Supply response

```fsharp
type SupplyDecision =
    | Accept of plannedQty: int
    | Reject of reason: string
```

#### Nested planning shape

```fsharp
type PlanningInput =
    { Demands: DemandLine list
      Inventory: InventoryRecord list
      Horizon: int }
```

These are all Layer A decisions. They define the world before behavior starts.

---

### 4.8) What Layer A is not

Layer A is **not**:

* validation logic
* orchestration
* event replay
* database access
* optimization algorithm

Those come later.

Layer A only defines the **shape of meaning**.

---

### 4.9) How Layer A connects to the rest

* Layer B takes these shapes and transforms them.
* Layer C wraps these shapes in context like `Option`, `Result`, `Async`.
* Layer E evolves these shapes through time.

So Layer A is the foundation.

---

## 5. Layer B: Pure Transformations & Workflows


This layer is about **pure transformations**: how one domain value becomes another, and how those transformations compose.

The core question here is:

**Given a domain value, what is the next valid domain value, and how do we chain those steps safely?**

---

### 5.1) Functions

A function is the basic unit of business logic.

In SCM, a function answers things like:

* Is this demand valid?
* What is the projected inventory after this movement?
* What shipment quantity should be proposed?
* How should a BOM explode?

Example:

```fsharp
let addSafetyStock qty safety = qty + safety
let isPositive qty = qty > 0
```

#### 5.1.1) How to think

A function is not just code. It is a **domain rule**.

If you can describe a business step as:
“take this input, produce that output,”
then it should probably be a function.

#### 5.1.2) SCM design rule

Prefer pure functions for:

* validation
* calculation
* normalization
* scoring
* derivation

Avoid mixing database, logging, and network calls into this layer.

---

### 5.2) Composition

Composition means combining smaller functions into larger ones.

Example:

```fsharp
let normalizeQty qty = max 0 qty
let applySafetyStock qty = qty + 5
let formatQty qty = $"Qty = {qty}"

let pipeline =
    normalizeQty >> applySafetyStock >> formatQty
```

#### 5.2.1) How to think

If one step produces the input for the next step, compose them instead of writing one large function.

#### 5.2.2) Why it matters in SCM

SCM logic is naturally staged:

* clean input
* validate
* enrich
* calculate
* allocate
* summarize

Composition lets each stage stay small and testable.

---

### 5.3) Categories

A category is the abstract world where:

* objects are types,
* arrows are functions,
* arrows compose,
* identity exists.

In F#, this is not a separate syntax feature. It is the deeper structure behind pure functions.

#### 5.3.1) How to think

Your domain is a network of types, and your business rules are arrows connecting them.

Example:

```fsharp
type RawDemand = string
type ValidDemand = int
type AllocatedDemand = int

let parseDemand (s: string) : int = int s
let validateDemand (n: int) : int = max 0 n
let allocateDemand (n: int) : int = n
```

These are arrows between domain stages.

#### 5.3.2) SCM design rule

Treat each stage of your pipeline as a named transformation between meaningful types.

That gives you:

* readability
* testability
* easier refactoring
* better separation of concerns

---

### 5.4) Morphisms

A morphism is simply a structure-preserving arrow.

In software terms, it is a function that changes a value while respecting the meaning of the domain.

Example:

```fsharp
type Demand = { Qty: int }
type ValidDemand = { Qty: int }

let validate (d: Demand) : ValidDemand =
    { Qty = max 0 d.Qty }
```

#### 5.4.1) How to think

A morphism is not “just any function.” It is a function that respects the shape you care about.

#### 5.4.2) SCM meaning

A good morphism preserves domain correctness:

* a valid demand stays meaningful,
* a shipment request stays a shipment request,
* an allocation result stays a valid allocation result.

#### 5.4.3) Design rule

Do not use a transformation that destroys important domain meaning unless that is explicitly the purpose.

---

### 5.5) Identity

Identity is the function that returns its input unchanged.

```fsharp
let idDemand x = x
```

#### 5.5.1) How to think

Identity is the “do nothing” step.

#### 5.5.2) Why it matters

Identity gives you a neutral element for composition. It means you can insert or remove a no-op without changing behavior.

#### 5.5.3) SCM meaning

Sometimes a pipeline stage is optional:

* no adjustment
* no filtering
* no mapping
* no transformation needed

Identity is the safest default.

#### 5.5.4) Design rule

If a step is sometimes unnecessary, make the no-op explicit rather than inventing fake logic.

---

### 5.6) Associativity

Associativity means that grouping does not change the result.

If you have:

* `f`
* `g`
* `h`

then:

```fsharp
(f >> g) >> h = f >> (g >> h)
```

#### 5.6.1) How to think

Composition should be stable regardless of how you parenthesize it.

#### 5.6.2) Why it matters

This is what makes large pipelines manageable.

#### 5.6.3) SCM meaning

If you can regroup your planning steps without changing the result, your design is robust.

Example:

* validate then enrich then allocate
* or validate then (enrich then allocate)

If the result differs, there is probably hidden state or hidden side effects leaking into what should be a pure layer.

#### 5.6.4) Design rule

Pure Layer B functions should be associative under composition.

---

### 5.7) Semigroups

A semigroup is a type with a combine operation that is associative.

In practice:

* you can combine values,
* but you may not have a true zero/empty value.

Example:

```fsharp
let combineErrors a b = a @ b
```

List concatenation is associative.

#### 5.7.1) How to think

A semigroup is what you have when combining makes sense, but there is no obvious neutral element.

#### 5.7.2) SCM examples

* merge error lists
* combine notes
* merge warning sets
* combine planning explanations

#### 5.7.3) Design rule

Use semigroup thinking when your domain naturally merges values but does not have a meaningful empty case.

---

### 5.8) Monoids

A monoid is a semigroup plus an identity element.

You have:

* an associative combine operation,
* a neutral element.

Example:

* integer addition with `0`
* string concatenation with `""`
* list concatenation with `[]`

```fsharp
type Monoid<'T> =
    { Zero: 'T
      Combine: 'T -> 'T -> 'T }

module InventoryMonoid =
    let sumMonoid : Monoid<int> =
        { Zero = 0; Combine = (+) }

let totalQty = List.fold InventoryMonoid.Combine InventoryMonoid.Zero [10; 20; 15]
```

#### 5.8.1) How to think

A monoid is perfect whenever your domain wants repeated accumulation.

#### 5.8.2) SCM examples

* total demand across periods
* total cost across routes
* total inventory movements
* merged event logs
* combined score from multiple heuristics

#### 5.8.3) Design rule

If you keep asking “how do I sum these things?” you are probably dealing with a monoid.

---

#### 5.8.4) Why monoids matter so much in SCM

SCM is full of aggregation:

* totals by plant
* quantities by period
* costs by lane
* risk by supplier
* exceptions by order

Whenever you see repeated accumulation, ask:

1. What is the combine operation?
2. Is it associative?
3. What is the identity?

If yes, you have a monoid-like structure and can use `fold` cleanly.

---

### Layer B SCM example

Suppose you want to turn raw demand into a plan score.

```fsharp
type RawDemand = { Qty: int }
type ValidDemand = { Qty: int }
type EnrichedDemand = { Qty: int; Priority: int }
type PlanScore = int

let validateDemand d =
    { Qty = max 0 d.Qty }

let enrichDemand d =
    { Qty = d.Qty
      Priority = if d.Qty > 100 then 2 else 1 }

let scorePlan d =
    d.Qty * d.Priority
```

Now compose:

```fsharp
let planScore =
    validateDemand
    >> enrichDemand
    >> scorePlan
```

---

### Another example: aggregation as a monoid

```fsharp
let totalDemand demands =
    demands
    |> List.map (fun d -> d.Qty)
    |> List.fold (+) 0
```

Here:

* the values are quantities,
* `+` is the combine operation,
* `0` is the identity.

That is a clean monoid pattern.

---

### 5.9) Tropical (Min-Plus) Semirings for SCM Pathfinding

SCM networks require finding optimal paths (e.g., shortest lead time, cheapest cost). We formalize this using the **Min-Plus Semiring**:
* **Addition ($\oplus$)** is modeled as `min` (choosing the best option).
* **Multiplication ($\otimes$)** is modeled as `+` (accumulating costs or lead times).

```fsharp
module TropicalSemiring =
    type TropicalValue =
        | Finite of float
        | Infinity

    let add x y =
        match x, y with
        | Finite a, Finite b -> Finite (min a b)
        | Infinity, v | v, Infinity -> v

    let multiply x y =
        match x, y with
        | Finite a, Finite b -> Finite (a + b)
        | _ -> Infinity
```

---

### Layer B design principles

#### Principle 1: Make every rule a named function

Do not hide domain logic inside generic utility code.

#### Principle 2: Keep functions small

One function, one responsibility.

#### Principle 3: Prefer composition over branching

If steps naturally follow one another, compose them.

#### Principle 4: Use identity deliberately

No-op is a valid design choice.

#### Principle 5: Look for associative combine operations

If values accumulate, seek monoids or semigroups.

#### Principle 6: Avoid hidden mutation

Layer B should be pure and deterministic.

---

### What Layer B is not

Layer B is not:

* database access
* HTTP calls
* logging
* asynchronous orchestration
* event publishing

Those belong to later layers or outer layers.

Layer B is only about the pure domain transformations themselves.

---

### The mental model to keep repeating

Think of Layer B as:

**Type A → Type B → Type C → Type D**

Each arrow is a business rule.

Then ask:

* Can I name each arrow clearly?
* Can I compose them?
* Do they obey identity?
* Are they associative?
* Do my accumulations form monoids or semigroups?

If yes, your design is getting mathematically clean and practically maintainable.

---


## 6. Layer C: Context and effects

The question in this layer is:

**How do I work with values that are wrapped in a context, without constantly unpacking and repacking them?**

Layer C is the right place for **contexts**: when a value is not just a plain value, but lives inside `Option`, `Result`, `List`, `Seq`, `Async`, or a custom computation model. In F#, computation expressions give you the syntax for sequencing and combining these kinds of computations, and the built-in language/library types make this style very natural. 

In SCM, this comes up everywhere:

* a value may be missing,
* a rule may fail,
* a calculation may produce many alternatives,
* a workflow may be asynchronous,
* a validation may accumulate errors,
* a function may consume a structure rather than produce one.

---

### 6.1) Functors

A functor is a context that supports mapping a function over the inside while preserving the outer structure. In category theory, a functor preserves identity and composition; in F#, this is the intuitive behavior you already use with `Option.map`, `List.map`, and similar mapping functions.

#### When to use

Use a functor when you want to transform the value **inside** a context, but you do not want to remove the context itself.

#### SCM example

You have an optional inventory forecast and want to increase it by a buffer only if it exists.

```fsharp id="c1_functor"
let addBuffer x = x + 10
let adjusted = Option.map addBuffer (Some 100)
```

#### Thinking pattern

If the structure stays the same and only the inside changes, you are in functor territory. That is the safest “shape-preserving” kind of transformation. 

---

### 6.2) Natural transformations

A natural transformation is a uniform transformation from one functor to another. The key idea is that it works the same way for every element type, not in a value-specific way. 

#### When to use

Use it when you want to translate one contextual representation into another without depending on the specific business value.

#### SCM example

Convert an optional demand into a list of zero-or-one demands for batch processing.

```fsharp id="c2_nat"
let optionToList = function
    | Some x -> [x]
    | None -> []
```

#### Thinking pattern

This is a structural translation. The value does not matter as much as the shape change from one context to another.

---

### 6.3) Applicative Functors

Applicative functors are the programming form of lax monoidal functors with tensorial strength. In practical terms, they let you combine independent computations inside a context.

#### When to use

Use applicative style when:

* computations are independent,
* you want to combine multiple validations,
* you want to collect all errors rather than stop at the first one.

#### SCM example

Validate `Plant`, `Sku`, and `Period` independently before creating a planning command.

```fsharp id="c3_applicative"
type Validation<'T, 'Error> = Result<'T, 'Error list>

module Validation =
    let ret x : Validation<'T, 'Error> = Ok x

    let apply (fV: Validation<'T -> 'U, 'Error>) (xV: Validation<'T, 'Error>) : Validation<'U, 'Error> =
        match fV, xV with
        | Ok f, Ok x -> Ok (f x)
        | Error e1, Error e2 -> Error (e1 @ e2)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    let (<*>) = apply
    let (<!>) f xV = Ok f <*> xV

module DemandValidation =
    open Validation

    let validateQty qty =
        if qty <= 0 then Error [ "Quantity must be positive" ] else Ok qty

    let validateDueDate (date: DateTime) =
        if date < DateTime.UtcNow.Date then Error [ "Due date cannot be in the past" ] else Ok date

    let assignPriority rawDemand =
        match rawDemand.Source with
        | SalesOrder _ -> 1
        | Forecast _ -> 2
        | StockTransfer _ -> 3
        | SafetyStock -> 4

    let validate (raw: RawDemand) : Validation<ValidatedDemand, string> =
        let create validatedQty validatedDate =
            { VRaw = raw; VPriority = assignPriority raw }
            |> fun vd -> 
                if vd.Qty <> validatedQty || vd.DueDate <> validatedDate then 
                    failwith "Invariant check failed during creation"
                vd

        create 
        <!> validateQty raw.Qty 
        <*> validateDueDate raw.DueDate
```

#### Thinking pattern

If the next step does not depend on the previous one, applicative style is often better than monadic style because it expresses parallel or independent checks more clearly. This is a very natural fit for input validation in SCM.

---

### 6.4) Monads

A monad is an endofunctor equipped with a unital associative binary operation under composition. In programming terms, monads describe computations with context, especially when the next step depends on the previous one. F# `Result` is explicitly documented as being used for monadic error handling, often called railway-oriented programming in the F# community.

#### When to use

Use monads when:

* each step depends on the previous value,
* failure should stop the pipeline,
* you want clean sequencing of context-aware operations.

#### SCM example

Validate demand, check inventory, then allocate supply.

```fsharp id="c4_monad"
let validate demand = Ok demand
let checkInventory demand = Ok demand
let allocate demand = Ok demand

let plan demand =
    validate demand
    |> Result.bind checkInventory
    |> Result.bind allocate
```

#### Thinking pattern

If your workflow reads like “do this, then depending on the result do that,” monadic composition is the right abstraction. F# computation expressions are designed for exactly this kind of sequencing and binding.

---

### 6.5) Kleisli categories

The Kleisli category is the category where monadic functions compose. In typed functional programming, it is used to model call-by-value functions with side effects and multiple sources, which makes it the right mental model for pipelines of the form `A -> M<B>`.

#### When to use

Use Kleisli thinking when your domain arrows return a contextual result:

* `Demand -> Result<ValidatedDemand, Error>`
* `Order -> Async<ShipmentPlan>`
* `Input -> Option<Output>`

#### SCM example

A planning pipeline that can fail at each stage.

```fsharp id="c5_kleisli"
let validate d = Ok d
let enrich d = Ok d
let score d = Ok d

let pipeline d =
    validate d
    |> Result.bind enrich
    |> Result.bind score
```

#### Thinking pattern

Instead of pretending these are plain `A -> B` functions, treat them as arrows in the Kleisli world. That keeps the context honest.

---

### 6.6) Foldable / Traversable intuition

F# does not expose the Haskell names `Foldable` and `Traversable` as core typeclass abstractions, but the same ideas show up clearly in `List.fold`, sequence processing, and computation expressions. F# lists are immutable ordered collections, and sequence expressions are a standard way to evaluate and combine sequences. 

#### When to use

Use the folding intuition when you are reducing many items to one summary:

* total demand
* total cost
* final inventory
* event replay into state

Use the traversing intuition when you want to apply a context-aware operation across many items while preserving their structure.

#### SCM example

Fold a stream of inventory events into current state.

```fsharp id="c6_fold"
type Event =
    | Added of int
    | Reserved of int

let apply state event =
    match event with
    | Added x -> state + x
    | Reserved x -> state - x

let replay events = List.fold apply 0 events
```

#### Thinking pattern

Folding is “many to one.” Traversing is “many through a context.” In SCM, both show up constantly in projections, reports, and batch validation.

---

### 6.7) Contravariant functors

A contravariant functor is like a functor that reverses morphism direction. In practice, this appears in consumer-like abstractions, such as predicates or validators.

#### When to use

Use contravariant thinking when your abstraction **consumes** values rather than producing them.

#### SCM example

A rule that validates a shipment request can often be thought of as something you can precompose with a function that converts richer inputs into the required input.

```fsharp id="c7_contra"
let isPositive x = x > 0
```

#### Thinking pattern

If a structure receives values and you want to adapt it from a broader input type to a narrower one, contravariance is the right intuition.

---

### 6.8) Bifunctors

A bifunctor is a functor of two variables. In category theory it is a functor whose domain is a product category, and in F# the most familiar practical example is `Result<'T,'E>`, where you conceptually have two independent type parameters on either side.

#### When to use

Use bifunctor thinking when both sides matter:

* success and error,
* left and right values,
* input and output annotations.

### SCM example

A planning result may carry either a plan or a list of reasons it failed.

```fsharp id="c8_bifunctor"
type PlanResult<'ok,'err> =
    | Good of 'ok
    | Bad of 'err
```

#### Thinking pattern

This is useful when the “good path” and “error path” both carry meaningful information.

---

### 6.9) Profunctors

A profunctor is a generalization of a functor, and in modern categorical language it is closely tied to representable profunctors. In practical programming, profunctor intuition fits adapters, transformers, and pipelines that both consume and produce values.

#### When to use

Use profunctor intuition when you are designing something like:

* an input/output adapter,
* a middleware pipeline,
* a transformation stage with an input side and an output side.

#### SCM example

A planner adapter may consume raw demand and produce a plan summary.

```fsharp id="c9_prof"
let adapt raw = raw
```

#### Thinking pattern

If a component is best understood as “something that takes one shape in and another shape out,” profunctor intuition is useful.

---

### 6.10) Option and Result as the everyday Layer C pair

In F#, `Option` is used when a value might not exist, and `Result` is used for monadic error handling. Those two types are the most important practical entry points for Layer C in domain code.

#### SCM example

* `Option` for “inventory record may or may not exist”
* `Result` for “allocation may succeed or fail with a reason”

```fsharp id="c10_option_result"
let findInventory sku = None
let allocate qty = Ok qty
```

#### Thinking pattern

Use `Option` when absence is normal and expected. Use `Result` when failure is meaningful and you want an explanation.

---

### 6.11) Async as a context

F# asynchronous computations are independent of the main program flow, and F# provides `async { ... }` along with computation expressions for structuring them. This is important in SCM because many real-world operations are delayed, remote, or IO-bound.

#### When to use

Use async when:

* reading from remote services,
* calling inventory APIs,
* publishing events,
* waiting on external planning engines.

#### SCM example

```fsharp id="c11_async"
let fetchInventory sku = async { return sku }
```

#### Thinking pattern

Async is still a context. The important thing is to keep the domain logic pure and let the async boundary sit outside it when possible.

---

### 6.11.1) Markov Categories for SCM Uncertainty

Supply chains are highly uncertain (lead-time variance, yield loss). Category theory models this using **Markov Categories**—categories where morphisms represent probability kernels (randomized processes). 
In F#, we represent this probabilistic context using a stochastic distribution or sampler function:

```fsharp
type Distribution<'T> = unit -> 'T  // Sampler representation of probability kernel

module MarkovKernel =
    let ret x : Distribution<'T> = fun () -> x
    
    let bind (f: 'T -> Distribution<'U>) (dist: Distribution<'T>) : Distribution<'U> =
        fun () -> f (dist ()) ()
        
    // Example: Modeling randomized lead-time offset
    let stochasticLeadTime baseLeadTime : Distribution<int> =
        let rand = System.Random()
        fun () ->
            let deviation = rand.Next(-1, 2) // -1, 0, or +1 day deviation
            max 1 (baseLeadTime + deviation)
```

---

### 6.12) Layer C design principles

#### Principle 1: Do not unwrap too early

Keep values inside their context until the last responsible moment.

#### Principle 2: Choose the weakest context that fits

* `Option` for absence
* `Result` for failure
* `List` for alternatives
* `Async` for delayed effects

#### Principle 3: Use functor before monad

If you only need to map, do not reach for binding.

#### Principle 4: Use applicative when checks are independent

This is especially good for validation.

#### Principle 5: Use monadic/Kleisli style when steps depend on previous results

That is the natural shape of many SCM workflows.

#### Principle 6: Use folds for replay and summaries

This is the backbone of event-sourced projections and reporting.

---

### 6.13) SCM layer example: a full Layer C pipeline

```fsharp id="c_full"
type RawDemand = { SkuId: string; Qty: int }

type ValidDemand = { SkuId: string; Qty: int }
type AllocatedDemand = { SkuId: string; Qty: int }

let validate d =
    if d.Qty > 0 then Ok { SkuId = d.SkuId; Qty = d.Qty }
    else Error ["Quantity must be positive"]

let checkInventory d =
    Ok d

let allocate d =
    Ok { SkuId = d.SkuId; Qty = d.Qty }

let plan raw =
    validate raw
    |> Result.bind checkInventory
    |> Result.bind allocate
```

This pipeline shows the whole layer in action:

* `Result` for context,
* `bind` for sequencing,
* plain functions for each business step,
* clear separation between pure logic and effectful orchestration.

---

### 6.14) How Layer C fits into your SCM architecture

Layer C is the bridge between pure domain shapes and full system behavior. It is where you decide:

* whether a value may be missing,
* whether a rule may fail,
* whether computations are independent or dependent,
* whether you need batch processing or streaming,
* whether a workflow is synchronous or asynchronous.

The big design lesson is this: **Layer C is not about business nouns; it is about business nouns inside context**. Once that becomes natural, the next layer — architecture and interpretation — will make much more sense.


## 7. Layer D: Architecture and interpretation

Layer D is the part where category theory starts telling you **how to architect the whole SCM solution**, not just how to write individual rules. It is about interpretation, abstraction, modularity, and the “same core logic, many runtimes” idea that shows up in pure F# design. F# computation expressions are specifically intended to help library authors and application authors sequence and combine computations in a clean domain-oriented way, and that makes them a natural fit for the ideas in this layer.


The key question in this layer is:

**How do I describe the business once, then interpret it many ways?**

For SCM, this means:

* one core domain model,
* one abstract planning language,
* many interpreters,
* clean boundaries between logic and effect,
* reusable transformations over nested structures,
* explicit handling of composition, context, and state.

That is why this layer is so important. It is where the pure core becomes a real system.

### 7.1) Monoidal categories

A monoidal category gives you a way to combine objects and arrows with a tensor-like product and a unit object, while keeping composition coherent. In other words A monoidal category is the categorical way to talk about combining independent things with a coherent notion of “togetherness.” In practice, this is the categorical language for “parallel composition” or “combining independent parts,” which is why it shows up everywhere in functional architecture and semantics.

In SCM, this matters when two subproblems can be planned independently and then merged: for example, demand from two plants, or inventory from two warehouses, or two heuristic scores that must be combined into one decision. The idea is not “use multiplication” but “use the right lawful combine structure.”

Examples:

* combine two independent demand forecasts,
* combine cost and risk scores,
* combine supply plans from two plants,
* merge two partial exception reports.

Example:

```fsharp
type RouteScore = { Cost: decimal; Risk: decimal }

let combine a b =
    { Cost = a.Cost + b.Cost
      Risk = a.Risk + b.Risk }
```

The deeper point is that monoidal structure is what later explains why applicatives, monads, and arrows can all be treated as monoids in suitable categories. That is not just elegant theory; it is the reason these abstractions share a common compositional core. ([arXiv][3])

---

#### 7.1.1) Symmetric Monoidal Categories (SMCs) for Parallelism

SCM workflows execute processes both sequentially (serial composition) and concurrently (parallel composition). We formalize this using **Symmetric Monoidal Categories (SMCs)**:
* **Serial Composition ($\circ$)**: Represented in F# by the pipe/composition operators (`>>`).
* **Parallel Composition ($\otimes$)**: Represented by running independent computations concurrently (using F#'s `Async.Parallel`).

To ensure this works with differing return types without compilation errors, we upcast to `Async<obj>` inside the tensor product:

```fsharp
module ParallelComposition =
    /// Runs two independent planning calculations (morphisms) in parallel
    let tensor (m1: 'A -> Async<'B>) (m2: 'C -> Async<'D>) : ('A * 'C) -> Async<'B * 'D> =
        fun (a, c) ->
            async {
                let! jobs = Async.Parallel [
                    async { let! r = m1 a in return box r }
                    async { let! r = m2 c in return box r }
                ]
                let b = box jobs.[0] :?> 'B
                let d = box jobs.[1] :?> 'D
                return (b, d)
            }
```

---

### 7.2) Cartesian closed categories

A cartesian closed category is the categorical setting where products and function spaces coexist coherently. This is one of the reasons typed functional programming feels so natural: you can combine data with product types and treat functions as first-class values. ([Microsoft Learn][1])

In SCM, this shows up whenever you want policies to be first-class.

Example:

```fsharp
type PlanningInput =
    { Demand: int
      Capacity: int }

let decide input =
    if input.Demand <= input.Capacity then "Accept"
    else "Reject"
```

Here:

* `PlanningInput` is a product of facts,
* `decide` is a function from one domain value to another,
* the function itself can be passed around, stored, or composed.

#### Why this matters

A lot of SCM architecture is about functions:

* policy functions,
* scoring functions,
* allocation functions,
* feasibility functions,
* projection functions.

CCC intuition tells you that these are not second-class helpers. They are core domain objects.

#### Design rule

When you hear “policy,” “rule,” “classifier,” or “decision,” model it as a function first.

---

### 7.3) Adjunctions

An adjunction is a pair of functors that express a best-fit translation between two structures. In category theory, adjunctions are one of the main ways to formalize “free vs. forgetful” and “syntax vs. semantics” relationships. The standard notes also show that many limit/colimit constructions fit adjoint patterns. ([Ncat Lab][3])

For SCM, this is the idea behind:

* build a planning DSL,
* then interpret it into simulation, execution, or testing.

Example shape:

```fsharp
type PlanningOp =
    | ReadInventory of string
    | ReserveStock of string * int
    | EmitEvent of string
```

This is not yet execution. It is a description.

Then you can interpret it in many ways:

* a pure simulator,
* a test interpreter,
* a production interpreter,
* a replay interpreter.

#### Why this matters

Adjunction thinking prevents you from hard-wiring the meaning of a plan too early. The logic stays reusable because it is written as structure, not as immediate effect.

#### Design rule

When a business process should be:

* describable,
* inspectable,
* replayable,
* interpretable in multiple ways,

think adjunction.

---

### 7.4) Free monads

Free monads are the standard way to represent a program as data before interpreting it. In the algebraic-effects literature, the free monad is exactly what arises from the signature of effectful operations, and the interpreter is then given by a fold-like handler. ([arXiv][2])

For SCM, this is extremely useful when you want the same plan logic to run in multiple environments.

Example:

```fsharp
type PlanningProgram<'a> =
    | Return of 'a
    | ReadInventory of sku:string * (int -> PlanningProgram<'a>)
    | ReserveStock of sku:string * qty:int * (unit -> PlanningProgram<'a>)
```

This says:

* the program can return a value,
* or request inventory,
* or request a reservation,
* and continue afterward.

Then you write interpreters:

* in-memory interpreter for testing,
* live interpreter for production,
* simulation interpreter for what-if analysis.

#### Why this matters

Free monads give you:

* testability,
* replayability,
* separation of description from execution,
* explicit effect boundaries.

#### Design rule

Use free-monad-style modeling when the workflow itself is a domain artifact.

---

### 7.5) Comonads

Comonads are the dual of monads. The practical intuition is that comonadic structure models a value together with the surrounding context from which you can observe or derive information. Research on comonadic and effect-related semantics often uses this duality to reason about context-sensitive computation. ([arXiv][4])

In SCM, comonadic intuition is helpful for:

* rolling forecast windows,
* neighborhood-based demand scoring,
* context-aware heuristics,
* calculations that depend on adjacent periods.

Example intuition:

```fsharp
type Window<'a> =
    { Left: 'a list
      Focus: 'a
      Right: 'a list }
```

This is the shape of “a value plus its neighborhood.”

#### Why this matters

Some SCM rules are not about the current item alone. They depend on the surrounding horizon:

* demand at period `t` depends on `t-1`, `t`, `t+1`,
* allocation may depend on nearby capacity,
* smoothing uses neighboring points.

#### Design rule

Use comonadic thinking when the computation reads a focused value in a context, not when it constructs a result step by step.

---

### 7.6) Yoneda lemma

Yoneda is one of the deepest pieces of Layer D. In category theory, it says an object is determined by the way it interacts with all other objects through morphisms. nLab states this is a central and deep result, and the formal statement identifies representable behavior as the essential information. ([Ncat Lab][5])

For SCM and DDD, the practical lesson is:

**Understand a type by how it behaves, not only by its fields.**

For example, an `InventoryState` is more meaningfully understood by:

* what queries are allowed,
* what transformations are allowed,
* what invariants it preserves,
* what projections it supports.

That is exactly why good domain APIs matter. The behavior exposes the meaning.

Example intuition:

```fsharp
type InventoryState =
    { OnHand: int
      Reserved: int }

let available inv = inv.OnHand - inv.Reserved
```

The real meaning of the type is not just “two integers.” It is the set of lawful observations and transformations available on that structure.

#### Why this matters

Yoneda is the foundation for thinking in terms of:

* capabilities,
* interfaces,
* representable behavior,
* domain meaning through usage.

#### Design rule

Ask “What can this type do?” before asking “What fields does it store?”

---

### 7.7) Limits and colimits

Limits are universal constructions for combining structures by constraints; colimits are the dual constructions for assembling or gluing structures together. The standard notes describe products, pullbacks, coproducts, and pushouts as special cases of these universal ideas. ([Microsoft Learn][1])

In SCM:

* a **limit-like** question is “What is the common solution that satisfies these constraints?”
* a **colimit-like** question is “How do I glue together these compatible parts into one result?”

Examples:

* limit-like: match order, inventory, and shipment data on the same key,
* colimit-like: merge multiple demand streams into one planning view.

#### Why this matters

You constantly do both in SCM:

* constraints for feasibility,
* merging for consolidation.

#### Design rule

Use limit intuition when the system must agree on shared structure.
Use colimit intuition when the system assembles from alternatives or fragments.

---

### 7.8) Kan extensions

Kan extensions are a principled way to extend a functor along another functor. The category theory literature uses them as one of the most general forms of “best possible extension,” and Yoneda itself is connected to Kan extension through the Yoneda extension viewpoint. ([Ncat Lab][3])

In SCM, the intuition is this:

You know how to compute something at one level of detail.
Now you need the “correct” way to lift that computation to another level.

Example:

* SKU-level cost logic
* family-level cost logic
* plant-level cost logic

A Kan-extension mindset asks:
“Can I extend this logic canonically rather than rewriting it by hand?”

#### Why this matters

SCM often changes granularity:

* line level,
* SKU level,
* order level,
* plant level,
* network level.

Kan extension intuition is useful anytime you are lifting a rule across a different indexing scheme.

#### Design rule

When a computation must move cleanly across a changed structure, think Kan extension.

---

### 7.9) Algebraic structures

Algebraic structures are the laws behind combine behavior. The categorical-programming literature makes a strong point that monads, applicatives, and arrows can all be studied as monoids in suitable categories. That is why the same structural ideas recur so often in effectful programming. ([arXiv][2])

In SCM, algebraic structures show up everywhere:

* addition for quantities,
* concatenation for logs,
* merge for errors,
* max/min for priorities,
* weighted combination for scores.

Example:

```fsharp
let totalQty = List.fold (+) 0 [10; 20; 15]
```

#### Why this matters

Whenever a business field gets combined many times, ask:

* what is the operation?
* is it associative?
* what is the identity?
* is there a lawful algebraic structure underneath?

#### Design rule

If accumulation is central, stop thinking ad hoc and start thinking algebraically.

---

### 7.10) Optics: Lens, Prism, Traversal

Optics provide a modular way to access and update nested data. The optics literature explicitly shows that lenses, prisms, and traversals fit into a unified profunctor optic framework, and traversals are important because they model zero-or-more focused updates in a principled way. ([Oxford Computer Science][6])

In SCM, optics are gold because your domain objects become deeply nested:

* planning input contains inventories,
* inventories contain warehouses,
* warehouses contain locations,
* locations contain quantities and policies.

#### Lens

Use a lens when you focus on one guaranteed field.

Example intuition:

* update `SafetyStock`
* update `CapacityLimit`
* update `Priority`

#### Prism

Use a prism when you focus on one case of a discriminated union.

Example intuition:

* inspect only `Rejected`
* inspect only `Approved`
* inspect only `Backordered`

#### Traversal

Use a traversal when you want to visit and possibly update many parts.

Example intuition:

* increase every demand line by 5%
* mark every period in a horizon as reviewed
* adjust every route score

Example:

```fsharp
type Lens<'S, 'A> = { Get: 'S -> 'A; Set: 'A -> 'S -> 'S }

module Lenses =
    let shipmentQtyLens =
        { Get = fun (s: Shipment) -> s.Qty
          Set = fun qty (s: Shipment) -> { s with Qty = qty } }
```

#### Why this matters

Without optics, nested domain updates turn into repetitive pattern matching and fragile plumbing.

#### Design rule

Use optics whenever a nested update appears in more than one place.

---

### 7.11) Functional architecture patterns

This layer is where you decide how the whole system is shaped:

* pure core / impure shell,
* interpreter pattern,
* DSL plus runtime,
* pipelines,
* projection builders,
* simulation versus execution.

F# computation expressions are explicitly designed to let you define convenient syntax for domain-specific computations, and Microsoft notes that they are especially useful for library and framework authors building expressive components. ([Microsoft Learn][1])

In SCM, a strong functional architecture usually looks like this:

* domain core = pure functions and immutable types,
* planning language = description of the workflow,
* interpreter = production, test, simulation, or replay,
* effect boundary = IO, DB, messaging, time,
* projections = folds from events to read models.

#### Why this matters

This structure makes the system:

* easier to test,
* easier to reason about,
* easier to replay,
* easier to evolve.

#### Design rule

Keep “what the business means” separate from “how it runs.”

---

### 7.12) Effect systems

An effect system is a way to track which effects a computation may perform. The algebraic-effects literature treats effects as operations plus handlers, and effect systems as a way to reason about them. ([arXiv][7])

In SCM, your code often performs:

* IO,
* persistence,
* messaging,
* logging,
* time-based operations,
* remote API calls.

#### Why this matters

Even if your language does not expose a full research-grade effect system, the architectural principle still helps:

* separate effectful code from pure code,
* make effect boundaries explicit,
* interpret the same business program in multiple ways.

#### Design rule

Treat effectful operations as a boundary concern, not as the center of your domain model.

---

### 7.13) Tagless final

Tagless final is a style for embedding DSLs where you describe programs abstractly in terms of capabilities rather than by building one concrete syntax tree. The literature describes it as a way to build typed, reusable DSL embeddings with interpreters for evaluation, compilation, pretty-printing, and optimization. ([Nicolas Rinaudo][8])

In SCM, this is powerful when the same business logic must run in several modes:

* simulation,
* testing,
* live execution,
* replay,
* optimization.

Example intuition:

```fsharp
type PlanningCapabilities =
    { FetchInventory: SkuId -> PlantId -> Async<InventoryRecord>
      FetchBomGraph: unit -> Async<BomGraph>
      SaveEvents: OrderId -> InventoryEvent list -> Async<Result<unit, string>> }

module ApplicationWorkflows =
    /// Orchestrates validation, BOM explosion, and inventory netting/reservation.
    /// This workflow is entirely pure apart from the capability executions.
    let processOrderAcceptance (caps: PlanningCapabilities) (orderId: OrderId) (rawDemand: RawDemand) =
        async {
            // Step 1: Validate Demand (Layer C: Applicative Context)
            match DemandValidation.validate rawDemand with
            | Error errors -> 
                return Error (String.concat "; " errors)
            | Ok validatedDemand ->
                // Step 2: Fetch current dependencies (Layer D: Capabilities Injection)
                let! inventory = caps.FetchInventory validatedDemand.Sku validatedDemand.Plant
                let! bomGraph = caps.FetchBomGraph ()

                // Step 3: Explode BOM (Layer E: Recursive Scheme)
                match BomExplosion.explode bomGraph validatedDemand.Sku (decimal validatedDemand.Qty) with
                | Error err -> 
                    return Error $"BOM explosion failed: {err}"
                | Ok requirements ->
                    // Step 4: Run command handler on aggregate to reserve inventory (Layer E: Aggregate Fold)
                    match InventoryAggregate.handleCommand inventory (InventoryAggregate.ReserveStock validatedDemand.Qty) with
                    | Error err -> 
                        return Error $"Inventory allocation failed: {err}"
                    | Ok events ->
                        // Step 5: Persist changes
                        let! saveResult = caps.SaveEvents orderId events
                        match saveResult with
                        | Error err -> return Error $"Persistence failed: {err}"
                        | Ok () -> return Ok "Order accepted and inventory reserved"
        }
```

The important point is not the exact syntax but the shape:
you define a capability interface, and each interpreter gives that capability a meaning.

#### Why this matters

Tagless final keeps the logic reusable without committing too early to a concrete runtime.

#### Design rule

Use tagless final when you want one abstract business language and many interpreters.

---

### 7.14) Arrows

Arrows generalize functions and are useful for structured pipelines, especially when there is static composition and branching. The literature connects arrows with metaprogramming and composition of structured computations. ([Hacker News][9])

In SCM, arrows are a good fit for:

* staged planning pipelines,
* input normalization,
* enrichment,
* scoring,
* branching by feasibility,
* recombining results.

#### Why this matters

Not all pipelines are simple linear functions. Some have branching structure that is easier to model as an arrow-like pipeline.

#### Design rule

Use arrow intuition when composition is staged and structured, not just linear.

---

### 7.15) Monoids in endofunctor categories

This is one of the deeper unifying ideas behind Layer D. The categorical-programming literature describes monads, applicatives, and arrows as monoids in suitable monoidal categories. That means the computational structures you use every day are all instances of a deeper algebraic pattern. ([arXiv][2])

In SCM, this tells you that:

* validation,
* sequencing,
* parallel combination,
* and pipeline composition

are not unrelated tricks. They are expressions of a shared algebraic shape.

#### Why this matters

Once you see the same pattern repeatedly, you stop building one-off abstractions and start building lawful ones.

#### Design rule

When an abstraction feels “computation-like,” look for the monoidal structure behind it.

---

### 7.16) Higher-kinded structure intuition

Higher-kinded intuition is the habit of thinking in terms of **type constructors** rather than only concrete types. In practice, F# computation expressions, `Option`, `Result`, `Async`, and collections all encourage this way of thinking because they are contexts you map, bind, or sequence rather than plain values. ([Microsoft Learn][1])

For SCM, this means you constantly ask:

* Is this a plain value?
* Is it a value in a context?
* Does this context compose?
* Can I abstract over the context?

#### Why this matters

It lets you write reusable code for:

* optional data,
* error-aware data,
* asynchronous work,
* batch processing,
* planning contexts.

#### Design rule

Think in terms of “shape constructors” as well as concrete business types.

---

### 7.17) Type-level reasoning

Type-level reasoning means using the type system to encode domain constraints. F# discriminated unions, records, options, results, and pattern matching make this style very natural. Microsoft’s docs emphasize pattern matching and discriminated unions as core tools for decomposing and transforming data. ([Microsoft Learn][10])

In SCM, this often becomes phase separation:

* `RawDemand`
* `ValidatedDemand`
* `AllocatedDemand`
* `ShippedDemand`

Example:

```fsharp
type RawDemand = { Qty: int }
type ValidDemand = { Qty: int }

let validate d =
    if d.Qty > 0 then Some { Qty = d.Qty }
    else None
```

#### Why this matters

The compiler becomes part of your business-rule enforcement.

#### Design rule

Use distinct types for distinct business stages whenever confusion is possible.

---

### 7.18) Algebraic effects intuition

Algebraic effects describe effects as operations with handlers. The foundational papers say algebraic effects are computational effects represented by an equational theory, with the free model inducing the corresponding computational monad, and handlers interpreting the operations. ([arXiv][7])

In SCM, this is exactly what you want when a plan says:

* read inventory,
* reserve stock,
* emit event,
* ask for review,
* fail gracefully,
* retry later.

Instead of hardcoding each action directly in your domain logic, you describe the effect and interpret it separately.

#### Why this matters

It gives you modular side effects without sacrificing purity in the core.

#### Design rule

Think of the core as describing “what should happen,” and handlers as deciding “how it happens.”

---

### A concrete Layer D SCM blueprint

Here is the architecture I would recommend for your SCM application:

### Core domain

Immutable types and pure functions.

* `Demand`
* `InventoryRecord`
* `Shipment`
* `Plan`
* `Allocation`
* `Exception`

### Planning DSL

A description of planning operations.

* read inventory
* reserve stock
* score plan
* emit event
* request review

### Interpreters

Different meanings for the same DSL.

* production interpreter
* simulation interpreter
* test interpreter
* replay interpreter

### Projection/read model

Fold events into current state.

* inventory snapshots
* shipment status views
* demand coverage views
* exception dashboards

### Optics

Use lenses/prisms/traversals to manipulate nested structures cleanly.

### Effect boundary

Keep IO, async, persistence, messaging, and external calls outside the pure core.

This is the architecture where Layer D concepts become genuinely useful instead of merely beautiful.

---

### A concrete end-to-end example

Imagine a planning request:

```fsharp
type PlanningInput =
    { Sku: string
      Qty: int
      Plant: string }
```

The pure logic might:

1. validate input,
2. create a planning description,
3. interpret that description into simulation or execution,
4. fold resulting events into a projection.

That means:

* **Layer B** handles validation and scoring,
* **Layer C** handles `Result` or `Async`,
* **Layer D** defines the DSL, interpreter, optics, and architecture boundaries,
* **Layer E** later handles state evolution and event replay.

---

### What to internalize from Layer D

The deep lesson is this:

**Do not confuse the description of a plan with its execution.**
**Do not confuse data access with data meaning.**
**Do not confuse nested updates with business logic.**
**Do not confuse a workflow with the side effects it eventually triggers.**

Layer D gives you the structures to keep those apart.

---

### How Layer D fits your SCM system

Layer D is where you decide how the whole application hangs together:

* **Monoidal categories** help when combining independent subsystems.
* **CCC** helps when your core is a world of products and functions.
* **Adjunctions** help when you move between syntax and semantics.
* **Free monads** help when you need a describable workflow.
* **Comonads** help when context around a focus matters.
* **Yoneda** helps you think in terms of observable behavior.
* **Limits and colimits** help you reason about constrained merging and structured assembly.
* **Kan extensions** help you extend logic across new dimensions.
* **Optics** help you edit nested domain models.
* **Effect systems and algebraic effects** help you keep side effects modular.
* **Tagless final** helps you keep one logic with many interpreters.
* **Arrows** help with structured pipelines.
* **Type-level reasoning** keeps bad states out.
* **Higher-kinded intuition** keeps abstractions reusable. ([Math UCR][2])


### Layer D requires Layer C to stay effect-shaped and contract-driven:

* provider APIs should remain Async<Result<...>>
* reservations must be idempotent
* provider failures must map to limiters/reasons
* telemetry should be emitted at boundaries
* concurrency wrappers should serialize or gate promise requests where needed
* ML-based estimates should carry basis/version and degrade gracefully on failure.

### Layer D in your system is the architecture and interpretation layer:

* shared contracts,
* reusable policies,
* provider-based orchestration,
* event-sourced boundaries,
* runtime interpretation,
* optimization reuse,
* telemetry and degradation,
* and clear separation between knowledge, decision, and execution.

If you are building an SCM platform in F#, the practical rule is this: keep the core domain as pure transformations and lawful data shapes, then use Layer D ideas to design the interpreters, effect boundaries, and reusable abstractions around that core. That is the point where category theory stops being “math about math” and becomes a design language for the whole system. ([Microsoft Learn][1])


## 8. Layer E: State evolution over time

Layer E is where your SCM system becomes a story over time: not just “what is the data?” or “how do we transform it?”, but “how does the system evolve safely from one valid state to the next?” In F#, discriminated unions are a natural fit for this because they model named cases, including valid/error cases and recursive tree structures, and pattern matching is the language feature that lets you decompose those cases explicitly and safely. Lists are ordered, immutable series of elements, and `List.fold` is the standard tool for carrying an accumulator through a sequence of values. ([Microsoft Learn][1])

The main question here is:

**Given a current state and a sequence of events or inputs, what is the next valid state, and how do we prove we never leave the domain’s rules?**

Layer E is where the SCM system becomes a **living system** rather than just a set of computations.
This is the layer for:

* state machines (inventory movement, shipment progression, order lifecycles)
* event sourcing (forecast updates)
* recursive schemes (BOM expansion, routing expansion)
* coalgebras
* fixed points
* aggregate replay
* replanning after disruption
* traceability through pegs and reservations


Layer E is where the earlier layers become a living system:

* Layer A gives you the state shapes.
* Layer B gives you the pure transitions.
* Layer C gives you contextual computations like failure and async.
* Layer D gives you the architecture for interpretation and reuse.
* Layer E turns all of that into a time-aware system that can evolve, replay, simulate, and stabilize. ([Microsoft Learn][1])
---

### 8.1) State machines

A state machine is the cleanest way to represent legal transitions over time. In categorical terms, the coalgebraic view of automata models a state-based system as a map from states into their observable next-step behavior, and nLab explicitly treats deterministic automata as coalgebraic state-based systems. ([NCAT Lab][2])

In F#, the state machine usually becomes a discriminated union for states plus a transition function that pattern matches on the current state and input. That is a very natural fit because discriminated unions are intended for alternative cases, and pattern matching is the standard way to branch on them. ([Microsoft Learn][1])

#### Example:

A state machine prevents illegal transitions.
For SCM, that is essential because the system must not allow:

* a released reservation to become tentative again,
* a completed work order to be re-scheduled as if it were new,
* a superseded peg to act as active,
* a rejected promise to be treated like an accepted one.


```fsharp id="e1_state"
type ReservationStatus =
    | Tentative
    | Confirmed
    | Released
    | Expired
    | Reduced of decimal

type ReservationEvent =
    | ReservationCreated
    | ReservationConfirmed
    | ReservationReleased
    | ReservationExpired
    | ReservationReduced of decimal

let evolveReservation state event =
    match state, event with
    | Tentative, ReservationConfirmed -> Confirmed
    | Tentative, ReservationReleased -> Released
    | Tentative, ReservationExpired -> Expired
    | Confirmed, ReservationReleased -> Released
    | Confirmed, ReservationExpired -> Expired
    | Confirmed, ReservationReduced q -> Reduced q
    | s, _ -> s
```

#### Why this matters in SCM

Orders, shipments, production orders, and approval flows all have **legal paths**. State machines make those paths explicit, so illegal transitions are rejected by the transition function instead of being discovered later in production.

#### Design rule

Use state machines whenever the business says, “this thing may move through only these allowed stages.”

---

### 8.2) Event sourcing through category theory

Event sourcing is best understood as **state reconstruction by folding a sequence of immutable events**. F# lists are immutable ordered series of values, and `List.fold` carries an accumulator through the list; the recursion-scheme literature identifies catamorphisms as folds over inductive data types. That is the categorical backbone of event replay. ([Microsoft Learn][3])

In practice, the model is:

1. append events,
2. replay them,
3. derive current state,
4. build projections from the replayed state.

Example:

```fsharp
type InventoryEvent =
    | StockAdded of sku:string * qty:int
    | StockReserved of sku:string * qty:int
    | StockReleased of sku:string * qty:int

type InventoryState =
    { OnHand: int
      Reserved: int }

let apply state event =
    match event with
    | StockAdded (_, qty) ->
        { state with OnHand = state.OnHand + qty }
    | StockReserved (_, qty) ->
        { state with OnHand = state.OnHand - qty
          Reserved = state.Reserved + qty }
    | StockReleased (_, qty) ->
        { state with OnHand = state.OnHand + qty
          Reserved = state.Reserved - qty }

let replay events =
    List.fold apply { OnHand = 0; Reserved = 0 } events
```

#### Why this matters in SCM

SCM is naturally historical:

* what was ordered,
* what was received,
* what was reserved,
* what was shipped,
* what changed and when.

Event sourcing gives you auditability, replayability, traceability and the ability to derive read models for inventory, shipments, exceptions, and capacity. Because the state is a fold over events, you can rebuild it at any time from the event stream. ([Microsoft Learn][4])

#### Design rule

Use events as immutable facts. Use a fold to derive the current truth from them.

---

### 8.3) Recursive schemes

Recursive schemes are principled ways to consume or produce recursive data. nLab’s recursion-scheme page states that a catamorphism is a fold over an inductive data type, and the dual anamorphism is an unfold; hylomorphisms compose the two. ([NCAT Lab][5])

This is hugely useful in SCM because many domain structures are recursive:

* BOM trees,
* route trees,
* supply networks,
* nested exception trees,
* hierarchical allocations,
* multi-level product structures.

Example BOM tree:

```fsharp
type Bom =
    | Leaf of sku:string * qty:int
    | Node of sku:string * qty:int * children: Bom list
```

A catamorphism-like fold over this tree can compute total leaf demand, total cost, or exploded requirements.

Example fold:

```fsharp
let rec foldBom leafCase nodeCase bom =
    match bom with
    | Leaf (sku, qty) -> leafCase sku qty
    | Node (sku, qty, children) ->
        nodeCase sku qty (children |> List.map (foldBom leafCase nodeCase))
```

#### Why this matters in SCM

BOM explosion is a textbook recursion problem. Once you model it as a recursive structure, a recursive scheme gives you a clean way to:

* aggregate requirements,
* derive lower-level demands,
* compute totals,
* preserve invariants.

#### Design rule

If the domain is recursive, do not hand-roll ad hoc recursion everywhere. Put the recursion shape in one place and reuse it.

---

### 8.4) Coalgebras

Coalgebras are the dual of algebras: the structure maps are “turned around.” nLab describes coalgebras as the dual notion and emphasizes their role in state-based systems and automata. It also notes that coalgebras for endofunctors are a standard notion in computer science. ([NCAT Lab][6])

The practical intuition is:

* algebra / catamorphism = build or fold up a result,
* coalgebra = describe how a system unfolds over time.

Example coalgebraic state-step intuition:

```fsharp
type MachineState = { Qty:int }

type Observation =
    | NeedMore
    | Enough

let step state =
    if state.Qty < 100 then
        NeedMore, { state with Qty = state.Qty + 10 }
    else
        Enough, state
```

#### Why this matters in SCM

Simulation, replenishment, planning iteration, and rolling-horizon forecasting are all coalgebraic in spirit because they ask:

* what is the next state?
* what is observable now?
* what happens if we advance one step?

#### Design rule

Use coalgebraic thinking when the system is about **unfolding**, **advancing**, or **observing** rather than collapsing data into a summary.

---

### 8.5) Fixed points

Fixed points are the mathematical language of recursive equations. nLab notes that fixed points of endofunctors arise naturally as solutions to recursive equations, especially in the form of initial algebras and terminal coalgebras. ([NCAT Lab][7])

In SCM, fixed-point thinking appears when:

* a planning rule is applied repeatedly until stable,
* a projection is rebuilt until the final shape is reached,
* a recursive structure is defined in terms of itself.

Example intuition:

* “Keep propagating supply until no more changes occur.”
* “Recompute inventory projections until the result stabilizes.”
* “Keep expanding the BOM until the leaf requirements are reached.”

You do not always need a formal fixed-point combinator in code. What matters is the architectural idea: some SCM processes are naturally **iterative until stable**, not single-pass transformations.

#### Why this matters

A large part of planning is convergence:

* allocation updates,
* demand balancing,
* capacity checks,
* cascading recalculations.

#### Design rule

Use fixed-point thinking when repeated application of a rule should converge to a stable result.

---

### 8.6) DDD aggregates interpreted categorically

A DDD aggregate is best understood in Layer E as a state machine plus a fold over events or transitions, with the aggregate boundary protecting invariants. F# discriminated unions and pattern matching are especially useful here because they let you encode allowable states and transitions directly in types and matches. ([Microsoft Learn][1])

Example:

```fsharp
type ShipmentAggregate =
    { State: OrderState
      Events: OrderEvent list }

let evolve agg evt =
    match transition agg.State evt with
    | Some nextState -> Some { agg with State = nextState; Events = evt :: agg.Events }
    | None -> None
```

#### Why this matters in SCM

An aggregate should:

* protect invariants,
* accept only valid transitions,
* emit events for state changes,
* be replayable from history.

This fits SCM extremely well because orders, inventory buckets, and production requests all have clear boundaries and legal state progressions.

#### Design rule

Treat aggregates as the place where domain legality is enforced, not as a dumping ground for mutable data.

---

### 8.7) Event stream, projection, and read model

Once events exist, the read model is usually another fold over those facts, often optimized for reporting or UI queries. The key idea is still the same: immutable history in, derived state out. F# lists and folds are the everyday tools for this, while sequences are useful when the event stream is large or lazy. ([Microsoft Learn][3])

Example projection:

```fsharp
type InventoryView =
    { Available:int
      Reserved:int }

let project view event =
    match event with
    | StockAdded (_, qty) -> { view with Available = view.Available + qty }
    | StockReserved (_, qty) ->
        { view with Available = view.Available - qty
          Reserved = view.Reserved + qty }
    | StockReleased (_, qty) ->
        { view with Available = view.Available + qty
          Reserved = view.Reserved - qty }
```

#### Why this matters

In SCM, operational state and reporting state are often not the same thing. Projections let you keep the source of truth compact while building fast query views separately.

#### Design rule

Do not force the write model and read model to be identical just because they both come from the same events.

---

### 8.8) Example: A full Layer E SCM example

Suppose you are building an order-to-ship flow.

#### State

```fsharp
type OrderState =
    | Draft
    | Submitted
    | Allocated
    | Packed
    | Shipped
    | Cancelled
```

#### Events

```fsharp
type OrderEvent =
    | Submit
    | Allocate
    | Pack
    | Ship
    | Cancel
```

#### Transition

```fsharp
let transition state event =
    match state, event with
    | Draft, Submit -> Some Submitted
    | Submitted, Allocate -> Some Allocated
    | Allocated, Pack -> Some Packed
    | Packed, Ship -> Some Shipped
    | Draft, Cancel
    | Submitted, Cancel
    | Allocated, Cancel
    | Packed, Cancel -> Some Cancelled
    | _ -> None
```

#### Replay

```fsharp
let replay events =
    List.fold
        (fun state evt ->
            match transition state evt with
            | Some next -> next
            | None -> state)
        Draft
        events
```

This example combines the whole layer:

* explicit states via discriminated unions,
* explicit transitions via pattern matching,
* historical evolution via fold,
* replayable state from immutable events. ([Microsoft Learn][1])

---

### 8.9) Example: Recursive BOM Explosion
BOM explosion is inherently recursive. To avoid stack overflow on deep trees (10,000+ levels) and safely handle cycle errors, we use tail-recursive accumulators and path-based cycle detection.

```fsharp
module BomExplosion =

    type MaterialRequirement =
        { Sku: SkuId
          RequiredQty: decimal
          OffsetDays: int }

    /// Explodes BOM recursively, maintaining a path set of visited nodes to detect cycles.
    /// Supports diamond dependencies (same component reached via multiple paths).
    let explode (bomGraph: BomGraph) (rootSku: SkuId) (rootQty: decimal) : Result<MaterialRequirement list, string> =
        let rec explodeNode (sku: SkuId) (qty: decimal) (offset: int) (visited: Set<SkuId>) : Result<MaterialRequirement list, string> =
            if Set.contains sku visited then
                Error $"Circular BOM dependency detected at SKU: {sku} in path: {visited}"
            else
                match Map.tryFind sku bomGraph with
                | None -> 
                    // Leaf component: return requirements
                    Ok [ { Sku = sku; RequiredQty = qty; OffsetDays = offset } ]
                | Some components ->
                    let nextVisited = Set.add sku visited
                    components
                    |> List.fold (fun accResult comp ->
                        accResult |> Result.bind (fun acc ->
                            explodeNode comp.ComponentSku (qty * comp.QtyPerParent) (offset + comp.LeadTimeOffsetDays) nextVisited
                            |> Result.map (fun childReqs -> acc @ childReqs)
                        )
                    ) (Ok [])

        explodeNode rootSku rootQty 0 Set.empty
```
---


### 8.10) Example: Event Sourcing and Aggregate State
An aggregate is represented by:
1. An **Initial State**.
2. A **Command Handler**: `State -> Command -> Result<Event list, Error>`.
3. A **State Evolver (Fold)**: `State -> Event -> State`.

```fsharp
module InventoryAggregate =

    type InventoryCommand =
        | ReceiveStock of qty: int
        | IssueStock of qty: int
        | ReserveStock of qty: int
        | ReleaseReservation of qty: int

    let handleCommand (state: InventoryRecord) (command: InventoryCommand) : Result<InventoryEvent list, string> =
        match command with
        | ReceiveStock qty ->
            if qty <= 0 then Error "Receive quantity must be positive"
            else Ok [ StockReceived qty ]
            
        | IssueStock qty ->
            let available = state.OnHand - state.Reserved
            if qty <= 0 then Error "Issue quantity must be positive"
            elif available < qty then Error $"Insufficient stock. Available: {available}, Requested: {qty}"
            else Ok [ StockIssued qty ]
            
        | ReserveStock qty ->
            let available = state.OnHand - state.Reserved
            if qty <= 0 then Error "Reservation quantity must be positive"
            elif available < qty then Error $"Insufficient stock to reserve. Available: {available}, Requested: {qty}"
            else Ok [ StockReserved qty ]
            
        | ReleaseReservation qty ->
            if qty <= 0 then Error "Release quantity must be positive"
            elif state.Reserved < qty then Error $"Cannot release more reservations than are currently held. Held: {state.Reserved}, Requested: {qty}"
            else Ok [ ReservationReleased qty ]

    // Pure state fold (Layer E: Evolution)
    let evolve (state: InventoryRecord) (event: InventoryEvent) : InventoryRecord =
        match event with
        | StockReceived qty -> { state with OnHand = state.OnHand + qty }
        | StockIssued qty -> { state with OnHand = state.OnHand - qty }
        | StockReserved qty -> { state with Reserved = state.Reserved + qty }
        | ReservationReleased qty -> { state with Reserved = state.Reserved - qty }

    // Reconstruct state from history (Catamorphism)
    let rebuildState (initialState: InventoryRecord) (history: InventoryEvent list) : InventoryRecord =
        List.fold evolve initialState history
```

### 8.11) Example: Coalgebraic Netting Simulation
Planning systems project future states (unfolding a starting state over a planning horizon). This is modeled mathematically as a **Coalgebra**:
$$\text{step} : \text{State} \to (\text{Observations} \times \text{State})$$

```fsharp
module NettingSimulation =
    type NettingState =
        { Period: int
          CurrentStock: int }

    type NettingObservation =
        { Period: int
          DemandQty: int
          SupplyQty: int
          StockoutQty: int }

    /// A Coalgebraic transition function that models one step of the timeline.
    let step (state: NettingState) (demandQty: int) (supplyQty: int) : NettingObservation * NettingState =
        let nettingBalance = state.CurrentStock + supplyQty - demandQty
        let stockout = if nettingBalance < 0 then abs nettingBalance else 0
        let obs = { Period = state.Period; DemandQty = demandQty; SupplyQty = supplyQty; StockoutQty = stockout }
        let nextState = { Period = state.Period + 1; CurrentStock = max 0 nettingBalance }
        (obs, nextState)
```

---


### 8.12) Layer E design principles

#### Principle 1: Make time explicit

Do not hide evolution inside mutable state if the domain wants history.

#### Principle 2: Keep transitions lawful

Only allow valid state changes.

#### Principle 3: Store facts, not guesses

Use events as immutable historical records.

#### Principle 4: Rebuild from first principles

Use folds, not ad hoc hidden mutation, to reconstruct state.

#### Principle 5: Treat recursion as a first-class shape

BOMs, trees, and nested structures should have a recursion strategy designed in, not patched on later.

#### Principle 6: Separate aggregate state from projections

The write-side truth and read-side shape should not be forced to be the same.


The big idea is that SCM is not just a static model. It is a system of lawful evolutions. Once you see state machines, event sourcing, coalgebras, and recursion schemes as different views of that same evolution, the design becomes much more coherent.

---


### The clearest Layer E signals in the PDD are:

* **EventStoreDB for event persistence**
* **CQRS**
* **domain aggregates for production orders, inventory, resources**
* **real-time replanning based on disruptions**
* **work order status and progress from MES**
* **reservations and pegs with lifecycle**
* **read models in PostgreSQL**
* **incremental updates and cache invalidation on calendar/alloc/reservation changes**

That means Layer E is not an optional add-on. It is one of the central design commitments of the document.

---

## 9. Strategic Developer Coding Standards

To ensure that the engineering team implements this plan cleanly:
1. **Never use interfaces (`interface ... with`)** for application service decoupling. Use `Capabilities` records instead.
2. **Never use mutable variables (`mutable`, `Ref`, `ResizeArray`)** within the domain core. If aggregation is required, use list folds.
3. **Use single-case Discriminated Unions** (e.g. `SkuId` and `PlantId`) to enforce domain safety.
4. **Tail-recursion is mandatory** for all BOM tree and network traversals to prevent stack overflows on large datasets.
5. **Always test using pure values**: Since capabilities are parameters, mocking is done by passing lambda functions directly in tests, avoiding mocking frameworks.
