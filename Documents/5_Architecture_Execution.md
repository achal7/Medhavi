# Execution Architecture

# Chapter 1 — Purpose

## 1.1 Purpose

The purpose of the Execution Architecture is to define the architectural semantics through which the Business Architecture is faithfully realized.

The Business Architecture defines **what the enterprise means**.

The Execution Architecture defines **how that meaning is realized**.

The Execution Architecture introduces no business meaning of its own. Instead, it establishes the invariant execution semantics that every implementation shall preserve regardless of technology, runtime, deployment model or operational environment.

The Execution Architecture therefore serves as the architectural contract between Business Architecture and software implementation.

## 1.2 Scope

The Execution Architecture is concerned exclusively with the realization of business behaviour.

It defines:

* execution semantics
* observation semantics
* abnormal execution semantics
* execution composition
* execution collaboration

It intentionally does **not** define:

* business meaning
* implementation technology
* runtime frameworks
* persistence mechanisms
* communication protocols
* deployment models
* operational tooling

These belong to other architectural layers.

## 1.3 Objectives

The Execution Architecture exists to ensure that every realization of business behaviour is:

* **Faithful** — business meaning is preserved.
* **Deterministic** — equivalent business decisions produce equivalent execution semantics.
* **Explicit** — execution behaviour is always visible and never implicit.
* **Composable** — independent executions cooperate without changing individual semantics.
* **Observable** — execution knowledge can be acquired without influencing execution.
* **Recoverable** — abnormal execution can be handled while preserving business meaning.
* **Technology Independent** — implementation choices never redefine architectural semantics.

## 1.4 Architectural Position

The Execution Architecture occupies the architectural layer between Business Architecture and software implementation.

```text
Platform Architecture
        │
        ▼
Business Architecture
        │
        ▼
Execution Architecture
        │
        ▼
Component Architecture
        │
        ▼
Implementation
```

Each architectural layer refines the previous layer without changing its meaning.

Business Architecture defines meaning.

Execution Architecture defines realization semantics.

Component Architecture defines architectural components.

Implementation realizes those components using specific technologies.


## 1.5 Architectural Responsibility

The Execution Architecture is responsible for defining **how** business behaviour is realized.

It is not responsible for defining **what** business behaviour means.

Business meaning remains the exclusive responsibility of the Business Architecture.

Similarly, implementation concerns remain the responsibility of the Component Architecture and Implementation layers.

This separation ensures that business evolution, architectural evolution and technological evolution remain independently possible.


## 1.6 Guiding Principle

The Execution Architecture is organized around **architectural responsibilities** rather than implementation mechanisms.

Its purpose is not to describe software components.

Its purpose is to define the execution semantics from which software components are derived.

Perfect. And thank you for pointing that out.

I completely agree.

I think we should write this like an **engineering specification**, not like a blog or a book.

That means:

* No unnecessary `---`
* Minimal whitespace
* Compact but precise
* Every sentence should have architectural value

Also, after re-reading Chapter 1, I realised something.

The **Execution Laws** should not be "nice architectural statements."

They should be **axioms**.

An axiom has a very high bar:

> If an implementation violates it, the implementation is **not** an implementation of this architecture.

That is much stronger than a design guideline.

With that mindset, I would rewrite Chapter 2.

---

# 2. Execution Laws

Execution Laws define the invariant truths governing the Execution Architecture. Every implementation claiming conformance with this architecture shall preserve these laws.

## EL-001 Business Meaning is Immutable

Business meaning is defined exclusively by the Business Architecture.

The Execution Architecture shall realize business meaning but shall never define, modify or reinterpret it.

## EL-002 Realization Never Creates Meaning

Execution realizes business decisions.

Execution shall never introduce business decisions, business rules or business policies.

Business defines meaning.

Execution realizes meaning.

## EL-003 One Concept, One Authority

Every architectural concept has exactly one authoritative owner.

Other concepts may reference, transport, observe or persist that concept without acquiring semantic authority over it.

Authority shall never be duplicated.

## EL-004 Transport Does Not Transfer Ownership

Transporting information shall never transfer ownership, responsibility or meaning.

Messages transport.

Envelopes transport.

Execution Context transports.

Transport mechanisms never become authorities for the information they carry.

## EL-005 Responsibilities are Independent

Architectural responsibilities collaborate without absorbing one another.

Execution realizes.

Observation observes.

Failure represents abnormal execution.

No responsibility shall redefine another responsibility.

## EL-006 Observation is Non-Participating

Observation acquires architectural knowledge.

Observation shall never participate in execution, influence execution semantics or determine execution outcomes.

Execution shall remain semantically correct regardless of whether observation exists.

## EL-007 Failure Belongs to Execution

Failure is part of execution semantics.

Failure is neither a business concern nor an observational concern.

Execution determines how failures affect realization.

Observation may describe failures.

Recovery may respond to failures.

Neither owns failure.

## EL-008 Architectural Truth is Singular

Architectural truth originates exactly once.

Representations such as logs, traces, metrics, diagnostics or audit records are projections of architectural truth.

Representations shall never become the authoritative source of truth.

## EL-009 Behaviour Shall Be Explicit

Architectural behaviour shall always be explicit.

Execution shall never rely upon implicit transitions, hidden side effects or undocumented semantics.

Every behavioural transition shall have explicit architectural meaning.

## EL-010 Execution Semantics are Deterministic

Equivalent business decisions executed under equivalent conditions shall produce equivalent execution semantics.

Operational concerns shall never alter execution semantics.

## EL-011 Collaboration Preserves Semantics

Multiple executions may collaborate to realize larger business capabilities.

Collaboration shall coordinate executions without changing the semantics of any participating execution.

## EL-012 Technology Never Defines Architecture

Programming languages, frameworks, databases, messaging systems, actor systems, telemetry frameworks and deployment technologies are implementation choices.

Implementation technology realizes architecture.

It never defines architecture.

## EL-013 Architecture is Responsibility-Centric

The architecture is organized around architectural responsibilities rather than implementation mechanisms.

Architectural responsibilities define behaviour.

Components realize responsibilities.

Technologies implement components.

This direction shall never be reversed.

## EL-014 Architectural Layers Preserve Meaning

Each architectural layer refines the layer above it without changing its meaning.

```text
Platform Architecture
        ↓
Business Architecture
        ↓
Execution Architecture
        ↓
Component Architecture
        ↓
Implementation
```

Every layer may introduce additional detail.

No layer may redefine the meaning established by a higher layer.

---

# 3. Semantic Model

The Execution Architecture defines the semantic concepts required to realize the Business Architecture.

Unlike the Business Architecture, which models business meaning, the Execution Architecture models runtime realization.

Execution is the primary semantic concept describing the realization of business meaning. The internal mechanics through which an execution progresses are intentionally outside the scope of the Semantic Model and belong to the architectural realization defined by subsequent chapters.

Observation and Failure are semantic domains that describe specific aspects of execution.

Every remaining architectural concept derives its meaning from these semantic concepts.

## 3.1 Execution

### Definition

Execution is the realization of business meaning.

Execution represents the complete semantic scope within which a business decision is realized while preserving the meaning defined by the Business Architecture.

Execution introduces no business meaning.

It exists solely to realize it.

### Purpose

Execution establishes the architectural foundation for runtime realization.

Every realization of business behaviour belongs to exactly one execution.

### Semantic Characteristics

Execution is:

* bounded
* deterministic
* explicit
* composable
* observable
* recoverable

These characteristics define execution semantics rather than implementation behaviour.

### Relationships

Execution:

* realizes Business Architecture.
* may be observed.
* may experience abnormal realization.

Execution remains the semantic root of the Execution Architecture.

## 3.2 Observation

### Definition

Observation is the semantic domain concerned with acquiring knowledge about execution.

Observation exists because execution exists.

Without execution there is nothing to observe.

Observation never participates in realization.

It only acquires knowledge about realization.

### Purpose

Observation establishes the architectural foundation for understanding execution without influencing execution semantics.

### Relationships

Observation:

* derives knowledge from Execution.
* communicates execution knowledge.
* may describe abnormal realization.

Observation never changes execution.

## 3.3 Failure

### Definition

Failure is the semantic domain concerned with abnormal realization.

Failure exists because execution may be unable to faithfully realize business meaning.

Failure is therefore an execution concern rather than a business concern.

### Purpose

Failure establishes the architectural foundation for representing abnormal realization explicitly.

### Relationships

Failure:

* originates within Execution.
* may be observed.
* influences execution realization.

Failure never changes business meaning.

## 3.4 Derived Concepts

The Semantic Model intentionally defines only the minimum number of primary semantic concepts.

All remaining concepts derive their meaning from the concepts defined above.

Examples include:

From Execution

* Execution Boundary
* Execution State
* Execution Lifecycle
* Execution Outcome
* Execution Collaboration
* Execution Context

From Observation

* Execution Knowledge
* Log
* Metric
* Trace
* Diagnostic
* Health Information
* Audit

From Failure

* Recovery
* Retry
* Compensation
* Escalation
* Dead Letter
* Circuit Breaker

These are supporting semantic concepts rather than independent architectural concepts.

---

# 4. Execution Semantics

This chapter defines the behavioural semantics governing Execution.

Execution Semantics describe how business decisions are realized while preserving the architectural laws defined by this architecture.

Execution Semantics intentionally define behaviour rather than implementation.

## 4.1 Execution Initiation

Execution begins when a business decision is accepted for realization.

Execution establishes a bounded semantic scope within which that decision is realized.

Every execution has:

* a defined beginning
* a defined purpose
* a bounded scope

Execution shall not exist outside its defined scope.

## 4.2 Execution Progression

Execution progresses through an ordered sequence of explicit execution transformations.

Each transformation preserves execution semantics while advancing the realization of business meaning.

The internal realization of these transformations is an implementation concern.

The architectural requirement is that every transformation remains explicit, deterministic and semantically valid.

Progression represents the evolution of execution from initiation to completion.

Execution progression shall:

* be deterministic
* be explicit
* preserve business meaning
* preserve execution integrity

Implicit progression is prohibited.

## 4.3 Execution Collaboration

Business capabilities frequently require multiple executions to cooperate.

Execution collaboration coordinates independent executions while preserving the semantics of each execution.

Execution collaboration shall:

* preserve execution independence
* preserve execution boundaries
* communicate explicitly
* avoid semantic coupling

Collaboration coordinates execution.

It never changes execution semantics.

## 4.4 Execution Completion

Every execution terminates with exactly one architectural outcome.

Execution outcomes represent the semantic completion of realization.

An execution may conclude as:

* Completed
* Failed
* Cancelled

Additional specialized outcomes may refine these semantics without changing them.

Execution outcomes are final.

## 4.5 Execution Guarantees

Every execution guarantees:

### Semantic Preservation

Execution faithfully realizes business meaning.

Execution shall never redefine business meaning.

### Determinism

Equivalent business decisions under equivalent execution conditions produce equivalent execution semantics.

### Explicit Behaviour

Execution behaviour and progression are explicit.

Hidden execution semantics are prohibited.

### Isolation

Execution semantics remain isolated from other executions except through explicit collaboration.

### Composability

Independent executions may cooperate without changing their individual semantics.

### Observability

Execution exposes sufficient architectural knowledge to permit independent observation.

### Recoverability

Execution preserves sufficient semantic information to support architectural recovery while preserving business meaning.

---

# 5. Observation Semantics

This chapter defines the behavioural semantics governing architectural observation.

Observation exists to acquire knowledge about execution without participating in execution.

Observation is independent of execution realization and independent of implementation technology.

Its purpose is to make execution understandable without changing execution.

## 5.1 Observation Acquisition

Observation acquires knowledge produced during execution.

Observation is passive.

It neither participates in execution nor influences execution semantics.

Observation shall:

* acquire architectural knowledge
* preserve execution independence
* remain non-intrusive
* tolerate independent implementation

Execution shall remain semantically correct regardless of whether observation exists.

## 5.2 Architectural Knowledge

Execution continuously exposes architectural knowledge throughout its progression.

Architectural knowledge describes execution.

It does not control execution.

Architectural knowledge shall be:

* explicit
* consistent
* immutable once produced
* implementation independent

Architectural knowledge represents architectural truth.

## 5.3 Knowledge Representation

Observation communicates architectural knowledge through representations appropriate for different consumers.

Typical representations include:

* logs
* metrics
* traces
* diagnostics
* audit records
* health information

Representations are projections of architectural knowledge.

Representations are never the architectural truth itself.

Multiple representations may coexist simultaneously.

## 5.4 Observation Consumers

Architectural knowledge may be consumed by different stakeholders and systems.

Examples include:

* operators
* monitoring systems
* diagnostics
* compliance
* auditing
* analytics
* artificial intelligence

Consumers observe architectural knowledge.

Consumers never redefine it.

## 5.5 Observation Guarantees

Observation guarantees:

### Independence

Observation never influences execution semantics.

### Fidelity

Observation faithfully represents architectural knowledge.

### Replaceability

Observation mechanisms may be replaced without changing execution semantics.

### Completeness

Observation acquires sufficient architectural knowledge to explain execution behaviour.

### Consistency

Equivalent executions produce equivalent architectural knowledge.

Representations may differ.

Architectural knowledge shall not.

---

Excellent.

I think Chapter 6 is actually where our architecture becomes the most different from typical software architectures.

I spent quite a while thinking about this after Chapter 5.

And I think we made the same mistake here that we made with Observation.

We gave **Failure** too much responsibility.

Let's start from first principles.

---

# The Question

> **What is Failure?**

Not

"What happens after failure?"

Not

"How do we recover?"

Not

"How do we retry?"

Just

**What is Failure?**

---

My answer today is much simpler than yesterday.

> **Failure is the inability of an execution to continue realizing business meaning under its current execution conditions.**

Notice something.

Failure doesn't do anything.

Failure doesn't retry.

Failure doesn't compensate.

Failure doesn't escalate.

Failure simply **exists**.

Everything else is an execution decision.

That completely changes the chapter.

---

# Another realization

Yesterday I wanted to move Recovery into Chapter 4.

Today I'm even more convinced.

Recovery is not about Failure.

Recovery is about **Execution continuing**.

Retry?

Execution strategy.

Compensation?

Execution strategy.

Fallback?

Execution strategy.

Escalation?

Execution strategy.

Failure is passive.

Execution is active.

---

# 6. Abnormal Execution Semantics

This chapter defines the semantics governing abnormal execution.

Abnormal execution represents situations in which execution cannot continue realizing business meaning under its current execution conditions.

Abnormal execution is an inherent part of execution semantics.

It is neither an implementation concern nor an observational concern.

## 6.1 Failure

### Definition

Failure represents an abnormal execution condition.

A failure exists whenever execution cannot continue realizing business meaning without an explicit change in execution strategy.

Failure introduces no business meaning.

Failure introduces no execution behaviour.

It simply represents an architectural condition.

### Purpose

Failure exists to ensure abnormal execution remains:

* explicit
* deterministic
* observable
* architecturally meaningful

Failure shall never be hidden within implementation.

## 6.2 Failure Classification

Failures may originate from different semantic causes.

Typical categories include:

* business constraints
* execution coordination
* resource availability
* infrastructure capability
* external collaboration

These classifications describe the origin of abnormal execution.

They do not prescribe implementation behaviour.

## 6.3 Failure Propagation

Failures belong to the execution in which they originate.

Propagation beyond an execution boundary shall always be explicit.

Execution boundaries isolate abnormal execution in the same way they isolate normal execution.

Propagation never changes business meaning.

## 6.4 Failure Visibility

Failures shall always produce architectural knowledge.

Failure visibility enables independent observation without coupling observation to execution.

Representations of failure remain implementation concerns.

Failure itself remains architectural.

## 6.5 Semantic Guarantees

Abnormal execution guarantees:

### Explicitness

Failure is always represented explicitly.

Hidden failure is prohibited.

### Isolation

Failure belongs to the execution in which it originates.

### Determinism

Equivalent failures under equivalent execution semantics produce equivalent abnormal execution semantics.

### Architectural Integrity

Failure never changes business meaning.

Only execution strategy may change.

### Observability

Failure exposes sufficient architectural knowledge for independent observation.

---

# Chapter 7 – Architectural Building Blocks

## 7.1 Purpose

The Execution Architecture defines the semantics of execution.

Architectural Building Blocks define how those semantics are realized.

Each Building Block owns exactly one architectural responsibility.

Collectively, the Building Blocks realize the complete Execution Architecture while preserving the architectural laws defined by this specification.

Architectural Building Blocks define logical responsibilities rather than implementation technologies.

An implementation may realize a Building Block using modules, functions, actors, services, workflows, middleware or any other suitable technology, provided the architectural responsibilities remain unchanged.

---

## 7.2 Design Principles

Every Architectural Building Block shall satisfy the following principles.

* Own exactly one architectural responsibility.
* Collaborate through explicit architectural contracts.
* Preserve the Execution Architecture semantics.
* Remain independently replaceable.
* Hide implementation details.
* Never redefine business meaning.
* Never absorb the responsibilities of another Building Block.

The preferred architectural direction is to increase composition while reducing responsibility.

---

## 7.3 Core Architectural Building Blocks

The following Building Blocks collectively realize the Execution Architecture.

### Execution Coordinator

Owns execution initiation.

Receives execution requests, normalizes different execution sources into a common execution model and initiates the appropriate Execution Pipeline.

It never performs execution progression.

---

### Execution Pipeline

Owns execution composition.

Composes an ordered sequence of Execution Stages into a complete execution flow.

It never performs the behaviour implemented by individual stages.

---

### Execution Stage

Owns one architectural transformation.

Each stage performs exactly one responsibility and transforms the current Execution Model before delegating control back to the Execution Pipeline.

Execution Stages are the fundamental compositional unit of the Execution Architecture.

---

### Execution Model

Owns architectural execution state.

The Execution Model flows through the Execution Pipeline and is transformed by Execution Stages.

It represents the architectural state of an execution rather than business behaviour.

---

### Architectural Knowledge Provider

Owns publication of Architectural Knowledge.

Execution Stages produce Architectural Knowledge.

The provider publishes that knowledge to one or more independent representations without influencing execution semantics.

---

### Execution Strategy

Owns post-execution decision making.

Evaluates the Execution Outcome and determines the appropriate architectural response.

The Execution Strategy selects execution actions.

It never realizes those actions.

---

## 7.4 Core Building Block Collaboration

The Core Building Blocks collaborate as follows.

```text
                    Execution Request
                            │
                            ▼
                Execution Coordinator
                            │
                            ▼
                  Execution Pipeline
                            │
                 composed of Stages
                            │
                            ▼
                  Execution Stage(s)
                            │
                            ▼
                    Execution Model
                  ┌─────────┴──────────┐
                  ▼                    ▼
      Architectural Knowledge   Execution Outcome
                  │                    │
                  ▼                    ▼
Architectural Knowledge      Execution Strategy
       Provider
```

Execution produces two architectural outputs.

* Architectural Knowledge
* Execution Outcome

Each output is consumed by a dedicated Building Block.

This separation preserves a clear distinction between execution, observation and execution decision making.

---

## 7.5 Supporting Architectural Building Blocks

Supporting Building Blocks extend the Execution Architecture without changing its semantics.

Unlike the Core Building Blocks, Supporting Building Blocks are optional.

Their necessity depends upon implementation requirements.

Typical Supporting Building Blocks include:

### Execution Store

Preserves and restores Execution Models to support durable execution.

---

### Messaging Provider

Transfers Execution Models across architectural boundaries.

Examples include messaging systems, event streams and integration mechanisms.

---

### Scheduler

Initiates execution based on time or schedules.

Examples include delayed execution, periodic execution and workflow continuation.

---

### Security Provider

Provides authentication, authorization and security policies required during execution.

Security behaviour is typically realized through dedicated Execution Stages.

---

### Policy Provider

Provides configurable execution policies.

Examples include retry policies, authorization policies and business policy evaluation.

Policy evaluation is typically realized through dedicated Execution Stages.

---

### Configuration Provider

Supplies execution configuration required by Building Blocks without becoming part of execution semantics.

---

## 7.6 Building Block Dependency Rules

Building Block dependencies shall preserve architectural responsibility boundaries.

The following dependency rules apply.

* The Execution Coordinator depends only upon the Execution Pipeline.
* The Execution Pipeline depends only upon Execution Stages.
* Execution Stages transform the Execution Model.
* Execution Stages produce Architectural Knowledge.
* Execution Stages produce Execution Outcomes.
* Architectural Knowledge is published exclusively by the Architectural Knowledge Provider.
* Execution Outcomes are evaluated exclusively by the Execution Strategy.
* Supporting Building Blocks may support any Core Building Block.
* Supporting Building Blocks shall never redefine execution semantics.

Architectural dependencies should remain acyclic whenever possible.

---

## 7.7 Technology Independence

Architectural Building Blocks define responsibilities rather than implementations.

The same Building Block may be realized using different implementation techniques.

| Building Block                   | Possible Realizations                                                      |
| -------------------------------- | -------------------------------------------------------------------------- |
| Execution Coordinator            | Function, Actor, Service, Workflow Entry Point                             |
| Execution Pipeline               | Function Composition, Middleware Pipeline, Workflow Engine                 |
| Execution Stage                  | Function, Middleware, Behaviour, Workflow Activity                         |
| Execution Model                  | Immutable Record, Document, Aggregate State                                |
| Architectural Knowledge Provider | Logging, Tracing, Metrics, Audit, Analytics                                |
| Execution Strategy               | Retry Policy, Compensation Policy, Escalation Policy                       |
| Execution Store                  | Event Store, Snapshot Store, SQL Database, Document Store, In-Memory Store |

Implementation technology shall never redefine architectural responsibility.

---

## 7.8 Evolution Principles

The preferred architectural direction is to evolve the architecture through composition rather than modification.

Execution behaviour should evolve by introducing new Execution Stages rather than modifying the Execution Pipeline.

Execution observation should evolve by introducing new Architectural Knowledge representations rather than modifying execution behaviour.

Execution durability should evolve by introducing Supporting Building Blocks rather than extending the Core Building Blocks.

A growing Core Building Block is an architectural indicator that responsibilities have not been decomposed correctly.

---

# Building Block 1 – Execution Coordinator

## 1. Responsibility

The Execution Coordinator is the architectural entry point into the Execution Architecture.

Its sole responsibility is to accept an execution request, select the appropriate Execution Pipeline and initiate its execution.

The Execution Coordinator owns execution initiation.

It does not own execution progression.

## 2. Architectural Responsibilities

The Execution Coordinator shall:

* accept execution requests
* normalize different execution sources into a common execution model
* select the appropriate execution pipeline
* initiate execution
* return the execution outcome

The Execution Coordinator shall not:

* perform execution progression
* execute business behaviour
* own business state
* perform observation
* classify failures
* execute recovery
* persist execution
* communicate with infrastructure directly

## 3. Collaborations

The Execution Coordinator collaborates exclusively with the Execution Pipeline.

```text
Execution Request
        │
        ▼
Execution Coordinator
        │
        ▼
Execution Pipeline
```

The coordinator owns execution initiation.

The pipeline owns execution progression.

This separation shall always be preserved.

## 4. Architectural Contract

The Execution Coordinator exposes a single architectural capability.

```text
CoordinateExecution
```

Every execution source is transformed into the same architectural execution model.

Typical execution sources include:

* HTTP requests
* Messages
* Scheduled execution
* Workflow continuation
* Manual user actions
* Recovery requests

The origin of an execution shall not influence its execution semantics.

## 5. Reference Realization

The reference realization intentionally remains minimal.

```fsharp
type ExecutionCoordinator<'Execution,'Outcome> =
    ExecutionPipeline<'Execution,'Outcome>
        -> 'Execution
        -> 'Outcome

module ExecutionCoordinator =

    let coordinate
        pipeline
        execution =

        ExecutionPipeline.run pipeline execution
```

The coordinator owns no execution logic.

Its responsibility is limited to initiating the selected execution pipeline.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns execution initiation.

✓ Delegates execution progression.

✓ Contains no business logic.

✓ Contains no execution stages.

✓ Contains no observation.

✓ Contains no persistence.

✓ Contains no recovery.

✓ Contains no infrastructure concerns.

The preferred implementation should remain intentionally small.

Growth of the coordinator indicates that execution responsibilities are leaking from the Execution Pipeline.

## 7. Typical Realizations

The same architectural building block may be realized using different implementation technologies.

| Execution Source      | Typical Realization  |
| --------------------- | -------------------- |
| HTTP API              | Endpoint Handler     |
| Message Bus           | Message Consumer     |
| Background Processing | Scheduled Job        |
| Workflow Engine       | Workflow Entry Point |
| Actor System          | Message Handler      |
| CLI                   | Command Handler      |

Regardless of the execution source, the architectural responsibility remains identical.

## 8. Examples

### HTTP Request

```text
HTTP Request
      │
      ▼
Execution Coordinator
      │
      ▼
Execution Pipeline
```

---

### Message Processing

```text
Message
    │
    ▼
Execution Coordinator
    │
    ▼
Execution Pipeline
```

---

### Scheduled Execution

```text
Scheduler
     │
     ▼
Execution Coordinator
     │
     ▼
ExecutionPipeline
```

Each execution source enters the architecture through the same building block.

Execution semantics remain independent of the triggering mechanism.

## 9. Design Notes

The Execution Coordinator should remain the smallest core building block within the Execution Architecture.

Its responsibility is limited to execution initiation.

It should never evolve into an orchestration component or absorb responsibilities belonging to the Execution Pipeline.

A coordinator that grows over time is an architectural smell indicating that execution progression or business behaviour has been misplaced.

The preferred architectural direction is to keep the coordinator stable while allowing the Execution Pipeline and Execution Stages to evolve independently.

---

# Building Block 2 – Execution Pipeline

## 1. Responsibility

The Execution Pipeline is responsible for composing and executing an ordered sequence of Execution Stages.

It defines the progression of an execution by coordinating the execution of individual stages while preserving the execution semantics defined by this architecture.

The Execution Pipeline owns stage composition.

It does not own the behaviour of individual stages.

## 2. Architectural Responsibilities

The Execution Pipeline shall:

* compose execution stages into a complete execution flow
* execute stages in a deterministic order
* preserve execution semantics throughout execution
* terminate execution when the pipeline completes
* return the final execution outcome

The Execution Pipeline shall not:

* initiate execution
* implement business behaviour
* own business state
* perform observation
* classify failures
* implement recovery
* communicate with infrastructure directly

## 3. Collaborations

The Execution Pipeline collaborates exclusively with Execution Stages.

```text
Execution Coordinator
        │
        ▼
Execution Pipeline
        │
        ▼
Execution Stage
        │
        ▼
Execution Stage
        │
        ▼
Execution Stage
```

The pipeline owns composition.

Each stage owns a single unit of execution behaviour.

## 4. Architectural Contract

The Execution Pipeline exposes a single architectural capability.

```text
ExecutePipeline
```

The pipeline accepts an execution together with an ordered collection of execution stages.

It executes those stages sequentially.

The pipeline itself remains unaware of the responsibilities implemented by individual stages.

## 5. Reference Realization

The reference realization models the architectural composition of execution stages.

```fsharp
type ExecutionStage<'Execution> =
    'Execution -> 'Execution

type ExecutionPipeline<'Execution> =
    ExecutionStage<'Execution> list

module ExecutionPipeline =

    let run
        (pipeline : ExecutionPipeline<'Execution>)
        (execution : 'Execution) =

        (execution, pipeline)
        ||> List.fold (fun state stage ->
            stage state)
```

The reference realization intentionally contains no business behaviour.

Its responsibility is limited to executing an ordered composition of execution stages.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns execution stage composition.

✓ Executes stages deterministically.

✓ Contains no business behaviour.

✓ Contains no observation.

✓ Contains no persistence.

✓ Contains no recovery.

✓ Contains no infrastructure.

✓ Remains implementation independent.

The preferred implementation should remain focused exclusively on stage composition.

## 7. Typical Realizations

The architectural pipeline may be realized using different implementation technologies.

| Architecture           | Typical Realization         |
| ---------------------- | --------------------------- |
| Functional Programming | Function Composition        |
| ASP.NET Core           | Middleware Pipeline         |
| MediatR                | Pipeline Behaviors          |
| Workflow Engine        | Activity Pipeline           |
| Actor System           | Behaviour Composition       |
| State Machine          | Ordered Transition Pipeline |

The realization mechanism may change.

Pipeline semantics shall not.

## 8. Examples

### Aggregate Execution

```text
Validation
      │
      ▼
Load Aggregate
      ▼
Business Decision
      ▼
Persist Changes
      ▼
Publish Events
```

---

### Long Running Workflow

```text
Validate
      │
      ▼
Reserve Resources
      ▼
Human Approval
      ▼
Finalize
```

---

### Optimization

```text
Validate Input
      │
      ▼
Build Model
      ▼
Solve
      ▼
Validate Solution
      ▼
Publish Result
```

---

### HTTP Request

```text
Authentication
      │
      ▼
Authorization
      ▼
Validation
      ▼
Business Capability
      ▼
Response Mapping
```

Each example represents a different realization while preserving identical execution semantics.

## 9. Pipeline Extensibility

Execution Pipelines are inherently extensible.

New execution stages may be introduced without modifying the pipeline itself.

Typical extension stages include:

* Authentication
* Authorization
* Validation
* Idempotency
* Policy Evaluation
* Business Capability
* Persistence
* Event Publication
* Observation
* Response Transformation

The pipeline remains unchanged.

Only its stage composition evolves.

This preserves architectural stability while allowing execution behaviour to evolve over time.

## 10. Design Notes

The Execution Pipeline is the compositional core of the Execution Architecture.

Its responsibility is intentionally narrow.

It composes execution stages.

It does not understand the responsibilities implemented by those stages.

This separation preserves the Open/Closed Principle at the architectural level.

The preferred architectural direction is to extend execution by introducing new Execution Stages rather than modifying the Execution Pipeline itself.

A growing pipeline implementation is an architectural indicator that responsibilities belonging to Execution Stages are leaking into the pipeline.

---

# Building Block 3 – Execution Stage

## 1. Responsibility

The Execution Stage is the smallest architectural realization unit within the Execution Architecture. Each Execution Stage realizes exactly one execution transformation while preserving the execution semantics defined by this architecture.

Its responsibility is to perform exactly one architectural transformation of an execution before delegating control to the next stage in the execution pipeline.

An Execution Stage owns one responsibility.

It realizes one execution transformation.

It produces one result.

## 2. Architectural Responsibilities

An Execution Stage shall:

* perform one architectural responsibility
* transform the execution
* preserve execution semantics
* delegate execution to the next stage
* terminate execution when continuation is not possible

An Execution Stage shall not:

* coordinate execution
* compose pipelines
* own business state
* perform multiple independent responsibilities
* know about unrelated stages

## 3. Collaborations

```text
Execution Pipeline
        │
        ▼
Execution Stage
        │
        ▼
Execution Stage
        │
        ▼
Execution Stage
```

Each stage collaborates only with its immediate successor through the Execution Pipeline.

Stages remain independent of one another.

## 4. Architectural Contract

Every Execution Stage exposes a single architectural capability.

```text
ExecuteStage
```

A stage receives an execution.

It performs one architectural transformation.

It either:

* returns the transformed execution for the next stage, or
* terminates execution by producing an execution outcome.

Execution Stages never directly invoke other stages.

The Execution Pipeline owns stage sequencing.

## 5. Reference Realization

The reference realization models an Execution Stage as a transformation that may either continue or terminate execution.

```fsharp
type StageResult<'Execution,'Outcome> =
    | Continue of 'Execution
    | Complete of 'Outcome

type ExecutionStage<'Execution,'Outcome> =
    'Execution -> StageResult<'Execution,'Outcome>
```

The Execution Pipeline interprets the result.

```fsharp
module ExecutionPipeline =

    let run
        (stages : ExecutionStage<'Execution,'Outcome> list)
        (execution : 'Execution) =

        let rec execute current remaining =

            match remaining with

            | [] ->
                Continue current

            | stage :: tail ->

                match stage current with

                | Continue next ->
                    execute next tail

                | Complete outcome ->
                    Complete outcome

        execute execution stages
```

The pipeline owns sequencing.

The stage owns behaviour.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ One responsibility per stage.

✓ One transformation per stage.

✓ Independent stage execution.

✓ Explicit continuation.

✓ Explicit completion.

✓ No pipeline knowledge.

✓ No execution coordination.

✓ No infrastructure concerns.

The preferred implementation should keep stages small, deterministic and independently testable.

## 7. Typical Realizations

Different architectural responsibilities may be realized as Execution Stages.

Examples include:

| Responsibility | Example Stage                   |
| -------------- | ------------------------------- |
| Validation     | Validate Request                |
| Authorization  | Verify Permissions              |
| Policy         | Evaluate Policy                 |
| Business       | Execute Capability              |
| Persistence    | Persist Changes                 |
| Publication    | Publish Events                  |
| Observation    | Publish Architectural Knowledge |
| Transformation | Build Response                  |

Each stage performs one architectural responsibility.

The pipeline composes them into a complete execution.

## 8. Examples

### Validation Stage

```fsharp
let validate execution =

    if isValid execution then
        Continue execution
    else
        Complete ValidationFailed
```

---

### Business Capability Stage

```fsharp
let executeCapability execution =

    let updatedExecution =
        decide execution

    Continue updatedExecution
```

---

### Persistence Stage

```fsharp
let persist execution =

    save execution

    Continue execution
```

---

### Observation Stage

```fsharp
let observe execution =

    publishKnowledge execution

    Continue execution
```

Each stage owns exactly one responsibility.

The pipeline remains unchanged.

## 9. Stage Composition

Execution behaviour evolves by composing additional stages rather than modifying existing stages.

Typical pipeline compositions include:

### Aggregate Execution

```text
Validate
      │
      ▼
Load Aggregate
      ▼
Execute Capability
      ▼
Persist
      ▼
Publish Events
```

### Workflow Execution

```text
Validate
      │
      ▼
Reserve Resources
      ▼
Human Approval
      ▼
Finalize
```

### HTTP Execution

```text
Authenticate
      │
      ▼
Authorize
      ▼
Validate
      ▼
Execute Capability
      ▼
Build Response
```

The execution semantics remain unchanged.

Only the stage composition differs.

## 10. Design Notes

The Execution Stage is the fundamental compositional unit of the Execution Architecture.

Execution Pipelines should evolve by introducing new stages rather than modifying existing ones.

Stages should remain highly cohesive, independently testable and reusable across multiple execution pipelines.

A stage that owns multiple unrelated responsibilities or requires knowledge of neighbouring stages is an architectural indicator that the execution responsibilities have not been decomposed correctly.

---

# Building Block 4 – Architectural Knowledge Provider

## 1. Responsibility

The Architectural Knowledge Provider is responsible for publishing Architectural Knowledge produced during execution.

It transforms Architectural Knowledge into one or more observable representations without influencing execution semantics.

The Architectural Knowledge Provider owns knowledge publication.

It does not own execution.

It does not own business behaviour.

It does not own observation semantics.

## 2. Architectural Responsibilities

The Architectural Knowledge Provider shall:

* publish Architectural Knowledge
* support multiple knowledge representations
* preserve knowledge fidelity
* remain independent of execution semantics
* allow multiple consumers to observe the same Architectural Knowledge

The Architectural Knowledge Provider shall not:

* participate in execution
* modify execution
* generate business behaviour
* become the architectural source of truth
* own persistence responsibilities

## 3. Collaborations

```text
Execution Pipeline
        │
        ▼
Execution Stage
        │
produces
        ▼
Architectural Knowledge
        │
published by
        ▼
Architectural Knowledge Provider
        │
     ┌──┴────────┬────────┬────────┬─────────┐
     ▼           ▼        ▼        ▼         ▼
   Logs       Traces   Metrics   Audit   AI Analysis
```

Execution Stages produce Architectural Knowledge.

The Architectural Knowledge Provider publishes that knowledge.

Representations consume the published knowledge.

Execution remains completely independent of how knowledge is represented.

## 4. Architectural Contract

The Architectural Knowledge Provider exposes a single architectural capability.

```text
PublishKnowledge
```

The provider accepts Architectural Knowledge.

It distributes that knowledge to one or more representation providers.

The provider never modifies the Architectural Knowledge it receives.

## 5. Reference Realization

Architectural Knowledge represents immutable knowledge produced during execution.

```fsharp
type ArchitecturalKnowledge =
    { Name : string
      Timestamp : Instant
      Attributes : Map<string,obj> }

type KnowledgeRepresentation =
    ArchitecturalKnowledge -> unit

type ArchitecturalKnowledgeProvider =
    KnowledgeRepresentation list

module ArchitecturalKnowledgeProvider =

    let publish
        (provider : ArchitecturalKnowledgeProvider)
        knowledge =

        provider
        |> List.iter (fun representation ->
            representation knowledge)
```

The provider owns publication.

Individual representations own presentation.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns knowledge publication.

✓ Supports multiple representations.

✓ Contains no execution behaviour.

✓ Contains no business logic.

✓ Contains no persistence.

✓ Preserves Architectural Knowledge.

✓ Independent of implementation technology.

The provider remains replaceable without changing execution semantics.

## 7. Typical Realizations

The same Architectural Knowledge may be represented in multiple ways.

| Representation    | Example               |
| ----------------- | --------------------- |
| Structured Log    | Serilog               |
| Distributed Trace | OpenTelemetry         |
| Metrics           | Prometheus            |
| Audit Record      | Audit Store           |
| Diagnostics       | Debug Output          |
| AI Reasoning      | Explainability Engine |

Representations are projections.

Architectural Knowledge remains the architectural truth.

## 8. Examples

### Example – Business Decision

```text
Business Capability
        │
        ▼
Decision Completed
        │
produces
        ▼
Architectural Knowledge
        │
published by
        ▼
Architectural Knowledge Provider
        │
     ┌──┴────────┬────────┬────────┐
     ▼           ▼        ▼        ▼
   Log        Trace    Audit    Dashboard
```

---

### Example – Long Running Workflow

```text
Workflow Stage
        │
        ▼
Checkpoint Reached
        │
produces
        ▼
Architectural Knowledge
        │
published by
        ▼
Architectural Knowledge Provider
        │
     ┌──┴────────┬────────┬─────────┐
     ▼           ▼        ▼         ▼
 Metrics     Trace     Audit    AI Analysis
```

Every consumer observes the same Architectural Knowledge.

No consumer changes execution behaviour.

### Example – Structured Log Representation

```text
Business Capability
        │
        ▼
Order Accepted
        │
produces
        ▼
Architectural Knowledge
        │
──────────────────────────────────────────
Name:
    "OrderAccepted"

Attributes:
    OrderId = O-10245
    CustomerId = C-1001
    ExecutionId = EX-5421
    Capability = Order Management
    Decision = Accept Order
    Timestamp = 2026-06-29T10:15:42Z
──────────────────────────────────────────
        │
published by
        ▼
Architectural Knowledge Provider
        │
represented as
        ▼
Structured Log Entry
```

The provider does not generate the log.

It publishes Architectural Knowledge.

The log is one possible representation of that knowledge.

---

### Example – Metrics Representation

```text
Business Capability
        │
        ▼
Order Accepted
        │
produces
        ▼
Architectural Knowledge
        │
──────────────────────────────────────────
Name:
    "OrderAccepted"

Attributes:
    ExecutionDuration = 124 ms
    Outcome = Completed
    Capability = Order Management
    Priority = High
──────────────────────────────────────────
        │
published by
        ▼
Architectural Knowledge Provider
        │
represented as
        ▼
Metrics

OrdersProcessed += 1

AverageExecutionTime = 124 ms

SuccessfulExecutions += 1
```

The metrics are derived from the Architectural Knowledge.

They are not produced directly by the Business Capability or the Execution Stage.

Multiple representations may consume the same Architectural Knowledge simultaneously while preserving a single architectural source of truth.


## 9. Design Notes

Architectural Knowledge is the semantic source of truth for architectural observation.

Logs, traces, metrics, audit records and AI reasoning are independent representations of that knowledge.

New representations may be introduced without modifying execution, execution stages or the Architectural Knowledge Provider.

The preferred architectural direction is to preserve a clear separation between:

* execution,
* Architectural Knowledge,
* publication,
* representation.

This separation allows the execution architecture to evolve independently from observability technologies while preserving a single architectural truth.

---

# Building Block 5 – Execution Strategy

## 1. Responsibility

The Execution Strategy is responsible for determining the appropriate architectural response to an Execution Outcome.

It evaluates the outcome of an execution and selects the strategy that preserves the execution semantics defined by this architecture.

The Execution Strategy owns decision making after execution.

It does not execute the chosen strategy.

## 2. Architectural Responsibilities

The Execution Strategy shall:

* evaluate execution outcomes
* determine the appropriate execution strategy
* preserve execution semantics
* support multiple strategy implementations
* remain independent of implementation technology

The Execution Strategy shall not:

* execute business behaviour
* perform execution progression
* publish architectural knowledge
* perform retries
* perform persistence
* communicate with infrastructure directly

## 3. Collaborations

```text
Execution Pipeline
        │
produces
        ▼
Execution Outcome
        │
evaluated by
        ▼
Execution Strategy
        │
selects
        ▼
Execution Action
```

The pipeline determines the outcome.

The strategy determines the architectural response.

The execution of that response belongs to the selected implementation.

## 4. Architectural Contract

The Execution Strategy exposes a single architectural capability.

```text
DetermineStrategy
```

It accepts an Execution Outcome.

It returns the Execution Action that should follow.

The strategy itself performs no work.

It only determines the appropriate response.

## 5. Reference Realization

```fsharp
type ExecutionOutcome =
    | Completed
    | Failed
    | Cancelled

type ExecutionAction =
    | Finish
    | Retry
    | Delay
    | Escalate
    | Compensate
    | DeadLetter
    | AwaitExternalDecision

type ExecutionStrategy =
    ExecutionOutcome -> ExecutionAction
```

The strategy determines the architectural response.

The realization of that response remains outside the Execution Strategy.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns post-execution decision making.

✓ Contains no execution logic.

✓ Contains no retry implementation.

✓ Contains no compensation implementation.

✓ Contains no persistence.

✓ Contains no infrastructure concerns.

✓ Independent of execution progression.

The preferred implementation should remain focused exclusively on determining the architectural response.

## 7. Typical Realizations

Different execution environments may realize different strategies.

| Execution Outcome                | Typical Execution Action |
| -------------------------------- | ------------------------ |
| Completed                        | Finish                   |
| Validation Failure               | Finish                   |
| Business Rejection               | Finish                   |
| Temporary Infrastructure Failure | Retry                    |
| External Dependency Unavailable  | Delay                    |
| Manual Approval Required         | Await External Decision  |
| Unrecoverable Failure            | Escalate                 |
| Poison Message                   | Dead Letter              |
| Partial Completion               | Compensate               |

Different implementations may choose different actions while preserving the same architectural semantics.

## 8. Examples

### Successful Execution

```text
Execution Outcome
        │
        ▼
Completed
        │
evaluated by
        ▼
Execution Strategy
        │
selects
        ▼
Finish
```

---

### Temporary Failure

```text
Execution Outcome
        │
        ▼
Infrastructure Failure
        │
evaluated by
        ▼
Execution Strategy
        │
selects
        ▼
Retry
```

---

### Human Approval

```text
Execution Outcome
        │
        ▼
Approval Required
        │
evaluated by
        ▼
Execution Strategy
        │
selects
        ▼
Await External Decision
```

The Execution Strategy determines the response.

It does not perform the response.

## 9. Design Notes

Execution Strategy separates architectural decision making from execution realization.

This allows execution behaviour to evolve independently from the mechanisms used to realize retries, delays, compensations or escalations.

New execution actions may be introduced without modifying the Execution Pipeline or Execution Stages.

The preferred architectural direction is to keep the Execution Strategy focused exclusively on determining the architectural response while delegating the realization of that response to independent execution mechanisms.

---

# Building Block 6 – Execution Model

## 1. Responsibility

The Execution Model represents the architectural state that flows through the Execution Architecture.

It is the subject transformed by the Execution Pipeline and its Execution Stages.

The Execution Model represents the architectural state of an execution at a specific point in its realization. It provides the common architectural object exchanged between Execution Stages while preserving execution identity and semantic integrity.

It owns execution state.

It owns nothing else.

## 2. Architectural Responsibilities

The Execution Model shall:

* represent the current execution
* evolve as execution progresses
* preserve execution identity
* carry architectural information between stages
* remain immutable from an architectural perspective

The Execution Model shall not:

* execute business behaviour
* coordinate execution
* determine execution strategy
* publish architectural knowledge
* communicate with infrastructure

## 3. Collaborations

```text
Execution Coordinator
        │
creates
        ▼
Execution Model
        │
transformed by
        ▼
Execution Pipeline
        │
composed of
        ▼
Execution Stage(s)
        │
produce
        ▼
Execution Outcome
```

Every stage receives the current Execution Model.

Every stage produces a new Execution Model.

## 4. Architectural Contract

The Execution Model exposes no behaviour.

It is a purely architectural representation of execution.

Execution behaviour belongs to Execution Stages.

## 5. Reference Realization

```fsharp
type Execution<'State,'Metadata> =
    { State : 'State
      Metadata : 'Metadata }

type ExecutionStage<'State,'Metadata,'Outcome> =
    Execution<'State,'Metadata>
        -> StageResult<Execution<'State,'Metadata>,'Outcome>
```

The Execution Model evolves through immutable transformations.

Execution Stages never modify an existing Execution.

They produce a new one.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns execution state.

✓ Immutable progression.

✓ Independent of business behaviour.

✓ Independent of observation.

✓ Independent of execution strategy.

✓ Technology independent.

The Execution Model remains the single architectural object flowing through the entire Execution Architecture.

## 7. Typical Realizations

The Execution Model may carry different kinds of architectural information.

Examples include:

* execution identity
* correlation information
* execution metadata
* execution state
* execution timing
* execution attributes

Business information may also form part of the Execution Model when required by the execution.

## 8. Examples

### Aggregate Command

```text
Execution
    State = Aggregate
    Metadata = Correlation
```

↓

Validation Stage

↓

Load Aggregate Stage

↓

Decision Stage

↓

Persist Stage

↓

Execution Completed

---

### Long Running Workflow

```text
Execution
    State = Workflow
    Metadata = Checkpoint
```

↓

Approval Stage

↓

Resume Stage

↓

Completion Stage

Each stage transforms the same Execution Model.

The pipeline remains unchanged.

## 9. Design Notes

The Execution Model is the architectural object that unifies the Execution Architecture.

Execution Stages transform it.

The Execution Pipeline composes those transformations.

The Execution Coordinator initiates it.

Architectural Knowledge is produced from it.

Execution Outcomes are derived from it.

Execution Strategies determine the appropriate response to those outcomes.

The Execution Model therefore forms the common architectural language shared by every building block.

# Building Block 7 – Execution Store

## 1. Responsibility

The Execution Store is responsible for preserving and restoring Execution Models whenever durable execution is required.

It provides durable execution without influencing execution semantics.

The Execution Store owns execution persistence.

It owns nothing else.

## 2. Architectural Responsibilities

The Execution Store shall:

* preserve Execution Models
* restore previously preserved Execution Models
* support durable execution
* preserve execution identity
* preserve execution integrity
* remain independent of execution progression

The Execution Store shall not:

* coordinate execution
* execute business behaviour
* publish Architectural Knowledge
* determine execution strategy
* communicate with external consumers

## 3. Collaborations

```text
Execution Pipeline
        │
produces
        ▼
Execution Model
        │
preserved by
        ▼
Execution Store
        │
restores
        ▼
Execution Model
        │
returned to
        ▼
Execution Coordinator
```

The Execution Store preserves the Execution Model.

It does not understand its meaning.

## 4. Architectural Contract

The Execution Store exposes two architectural capabilities.

```text
PreserveExecution

RestoreExecution
```

Preservation shall not modify the Execution Model.

Restoration shall faithfully reconstruct the previously preserved Execution Model.

The preservation mechanism remains an implementation concern.

## 5. Reference Realization

```fsharp
type ExecutionId = string

type ExecutionStore<'Execution> =
    { Preserve :
        ExecutionId
            -> 'Execution
            -> unit

      Restore :
        ExecutionId
            -> 'Execution option }
```

The Execution Store owns durability.

It owns no execution behaviour.

## 6. Architectural Validation

The reference realization validates the architectural responsibilities.

✓ Owns execution persistence.

✓ Preserves execution identity.

✓ Independent of execution progression.

✓ Independent of business behaviour.

✓ Independent of observation.

✓ Independent of execution strategy.

✓ Independent of implementation technology.

The preferred implementation should remain focused exclusively on durable execution.

## 7. Typical Realizations

Different implementations may preserve the Execution Model using different storage technologies.

| Architecture           | Typical Realization |
| ---------------------- | ------------------- |
| Event Sourcing         | Event Store         |
| Snapshot Persistence   | Snapshot Store      |
| Relational Persistence | SQL Database        |
| Document Persistence   | Document Database   |
| In-Memory Execution    | Memory Store        |

The storage mechanism may change.

Execution semantics shall not.

## 8. Examples

### Event Sourced Execution

```text
Execution Model
        │
PreserveExecution
        │
        ▼
Event Store

...

RestoreExecution
        │
        ▼
Execution Model
```

---

### Long Running Workflow

```text
Execution Model
        │
Checkpoint
        │
        ▼
Execution Store

...

Resume
        │
        ▼
Execution Model
```

---

### In-Memory Execution

```text
Execution Model
        │
PreserveExecution
        │
        ▼
Memory Store
```

The same architectural building block supports both durable and transient execution.

## 9. Design Notes

The Execution Store preserves architectural state rather than business meaning.

It is responsible only for preserving and restoring Execution Models.

The internal persistence mechanism is intentionally outside the scope of this architecture.

Implementations may use event sourcing, snapshots, relational databases, document stores or in-memory storage while preserving identical architectural semantics.

Durability is therefore an implementation choice rather than an architectural requirement.

The preferred architectural direction is to keep the Execution Store independent of execution progression, execution strategy and architectural observation.
