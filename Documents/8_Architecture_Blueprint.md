# Medhavi APS — Architecture Blueprint  

# Chapter 1 — Introduction  

## 1.1 Purpose  

This document defines the software architecture that realises the Medhavi APS business specifications. It translates the five Intelligence Specifications—Demand, Supply, Promise, Scenario, and Knowledge—into a concrete technical design that governs how the system is built, deployed, and evolved.  

The Architecture Blueprint is the bridge between business intent and working software. It answers:  

- How do the Intelligence Specifications become bounded contexts, services, and modules?  
- What are the component contracts, integration patterns, and data flows?  
- How are cross‑cutting concerns—traceability, observability, resilience, AI governance—implemented consistently?  
- How does the system evolve from the current monolithic MVP to a distributed production architecture?  

This document is the single source of truth for the technical design. No implementation decision should contradict it without a documented architectural review.  

## 1.2 Relationship to Other Documents  

This Blueprint does not stand alone. It depends on and extends:  

| Document | Role |
|----------|------|
| **Medhavi APS Constitution** | Unchanging enterprise principles that every component must respect. |
| **Architecture Reference Standard (ARS)** | Identifier standards, traceability rules, lifecycle governance. |
| **Semantic Model** | Enterprise meaning and intelligence domain definitions. |
| **Capability / Decision / Rule‑Policy Models** | How the enterprise reasons, chooses, and governs. |
| **Demand Intelligence Specification** | Authoritative business specification for demand. |
| **Supply Intelligence Specification** | Authoritative business specification for supply. |
| **Promise Intelligence Specification** | Authoritative business specification for order promising. |
| **Scenario Intelligence Specification** | Authoritative business specification for scenario planning. |
| **Knowledge Intelligence Specification** | Authoritative business specification for cross‑domain learning. |

The Intelligence Specifications define **what** the system must do. This Blueprint defines **how** it will be built.  

## 1.3 Architectural Principles  

Every design decision in this Blueprint is governed by the following principles. They are derived from the Constitution, the ARS, and the practical experience of building the MVP.  

**P1 — Event‑Driven by Default**  
All state changes are represented as immutable events. Events are the source of truth for both domain state and cross‑context integration. No component directly mutates another component’s data.  

**P2 — Domain‑Driven Design**  
Bounded contexts own their invariants. Aggregates enforce transaction boundaries. The ubiquitous language of the Intelligence Specifications is directly reflected in the code.  

**P3 — Clean Architecture**  
Dependencies point inward. Domain logic has no external dependencies. Application services orchestrate; infrastructure implements abstractions. The functional core is pure; the imperative shell handles I/O.  

**P4 — AI‑Ready by Design**  
Every decision, event, and policy evaluation is traceable and explainable. AI agents use the same interfaces and follow the same rules as human users. Autonomy is governed by explicit contracts and policy gates.  

**P5 — Evolutionary Architecture**  
The system is designed to evolve. The MVP monolith can be extracted into independent services without rewriting domain logic. Database, event bus, and deployment strategies can change without touching business rules.  

**P6 — Production‑Ready from Day One**  
Resilience, observability, traceability, and security are not afterthoughts. They are built into the architecture from the first line of code.  

## 1.4 Document Structure  

- **Chapter 2** provides a high‑level system overview and component map.  
- **Chapters 3–6** define the core architecture: bounded contexts, events, command handling, and projections.  
- **Chapters 7–9** cover shared libraries, integration patterns, and the Planning Engine.  
- **Chapters 10–13** address AI enablement, traceability, observability, and resilience.  
- **Chapters 14–17** cover the presentation layer, data management, configuration, and security.  
- **Chapters 18–19** describe deployment evolution and testing strategy.  
- **Appendices** provide reference material for implementers.  

---

# Chapter 2 — System Overview  

## 2.1 High‑Level Component Map  

The Medhavi APS is composed of the following major elements:  

```
┌─────────────────────────────────────────────────────────────────┐
│                         Medhavi.Hub                             │
│                   (ASP.NET Core Web Host)                       │
│  Serves REST APIs, SignalR, health checks, static files         │
└──────────────────────────────┬──────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Medhavi.Nexus                            │
│              (Control Tower + Composition Root)                 │
│                                                                 │
│  ┌───────────┐ ┌───────────┐ ┌───────────┐ ┌───────────────┐    │
│  │  Demand   │ │  Supply   │ │  Promise  │ │   Scenario    │    │
│  │           │ │           │ │           │ │               │    │
│  │ Domain    │ │ Domain    │ │ Domain    │ │ Domain        │    │
│  │ App       │ │ App       │ │ App       │ │ App           │    │
│  │ Proj      │ │ Proj      │ │ Proj      │ │ Proj          │    │
│  └───────────┘ └───────────┘ └───────────┘ └───────────────┘    │
│                                                                 │
│  ┌───────────────┐ ┌──────────────┐ ┌───────────────────────┐   │
│  │   Knowledge   │ │  MasterData  │ │    Integration        │   │
│  │   Domain      │ │  Domain      │ │    ACL / Ingest       │   │
│  │   App         │ │  App         │ │                       │   │
│  │   Proj        │ │  Proj        │ │                       │   │
│  └───────────────┘ └──────────────┘ └───────────────────────┘   │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                  Planning Engine                          │  │
│  │  (MRP, Replenishment, Optimization, What‑If Simulation)   │  │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────┬──────────────────────────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          ▼                    ▼                    ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────────────┐
│   PostgreSQL    │ │   DecisionCore  │ │    SharedKernel         │
│                 │ │   (Pure F#)     │ │    Contracts            │
│  • events       │ │                 │ │    Infrastructure       │
│  • checkpoints  │ │  • Scoring      │ │                         │
│  • snapshots    │ │  • Feasibility  │ │  • ExecutionContext     │
│                 │ │  • Reservations │ │  • Logging / Telemetry  │
│                 │ │  • PolicyGate   │ │  • CircuitBreaker       │
│                 │ │  • Autonomy     │ │  • ExceptionHandling    │
└─────────────────┘ └─────────────────┘ └─────────────────────────┘
```

**Component responsibilities:**

- **Medhavi.Hub** — The ASP.NET Core web host. Exposes REST APIs, SignalR hubs, health check endpoints, and serves static files for the UI. It is the single entry point for all external traffic.
- **Medhavi.Nexus** — The composition root and control tower. In the MVP, it hosts all bounded contexts in‑process. It wires together the application services, domain logic, projections, and the event bus. It also serves as the control tower, aggregating telemetry and health information.
- **Bounded Contexts** — Each Intelligence Domain is realised as a bounded context with its own domain logic, application services, and projections. They communicate through events via the `DomainEventBus`.
- **Planning Engine** — A service within Nexus that executes planning modes (FastInsert, IncrementalRepair, FullReplan, Optimization, WhatIf). It uses DecisionCore for shared feasibility and scoring logic.
- **PostgreSQL** — The single database for event storage, checkpointing, and optional projection snapshots. In the MVP, an in‑memory store may be used; the repository abstraction makes the swap seamless.
- **DecisionCore** — A pure F# library shared by all bounded contexts. It contains scoring, feasibility, reservations, fingerprints, policy validation, and autonomy contracts. It has no dependencies and no side effects.
- **SharedKernel, Contracts, Infrastructure** — Supporting libraries providing base types, DTOs, and persistence adapters.

## 2.2 The Five Intelligence Domains  

Each Intelligence Specification maps to one bounded context:

| Intelligence Domain | Bounded Context | Primary Responsibility |
|---------------------|-----------------|------------------------|
| Demand Intelligence | `Medhavi.Demand` | Forecasts, segmentation, prioritisation, quality, exceptions, learning |
| Supply Intelligence | `Medhavi.Supply` | Supply plans, inventory, capacity, procurement, production, distribution, supplier collaboration, quality, exceptions, learning |
| Promise Intelligence | `Medhavi.Promise` | Order promising (ATP/CTP), allocation, prioritisation, order changes, customer collaboration, risk sensing, quality, exceptions, learning |
| Scenario Intelligence | `Medhavi.Scenario` | Scenario definition, simulation, comparison, risk assessment, recommendation, collaboration, trigger sensing, quality, exceptions, learning |
| Knowledge Intelligence | `Medhavi.Knowledge` | Cross‑domain pattern discovery, root‑cause analysis, knowledge graph governance, improvement portfolio, best practices, feedback loops, enterprise memory, AI knowledge serving, quality, learning |

Supporting contexts provide foundational capabilities:

| Context | Role |
|---------|------|
| `Medhavi.MasterData` | Products, BOMs, routings, resources, calendars, suppliers, customers, locations |
| `Medhavi.Integration` | Anti‑corruption layer, external event ingestion, data normalisation |

## 2.3 MVP Monolith Structure  

The MVP is a **modular monolith**. All bounded contexts are deployed as a single process within `Medhavi.Nexus`. They communicate through an in‑process `DomainEventBus` (an F# `Event<T>`). The in‑memory repository allows rapid development and testing without external dependencies.

```
Medhavi.Web (Blazor / ASP.NET)
        │
        ▼ (direct project reference, not HTTP)
Medhavi.Nexus
        │
        ├── Medhavi.Demand
        ├── Medhavi.Supply
        ├── Medhavi.Promise
        ├── Medhavi.Scenario
        ├── Medhavi.Knowledge
        ├── Medhavi.MasterData
        ├── Medhavi.Integration
        └── Medhavi.PlanningEngine
                │
                ▼
        Medhavi.Contracts (shared DTOs)
        Medhavi.SharedKernel (ExecutionContext, Logging, Telemetry)
        Medhavi.DecisionCore (pure shared library)
        Medhavi.Infrastructure (repository, event store abstractions)
```

This structure preserves clean domain boundaries while avoiding the operational complexity of a distributed system during the early stages of development.

### 2.4 Evolution Path  

The architecture is designed to evolve without rewriting domain logic:

1. **MVP** — Modular monolith with in‑memory event store. All contexts in one process.
2. **Persistent Event Store** — Swap the in‑memory repository for a PostgreSQL implementation. The `Repository` and `EnvelopeStoreOps` interfaces make this a configuration change.
3. **External Event Bus** — Replace the in‑process `DomainEventBus` with PostgreSQL `LISTEN`/`NOTIFY`, and later with RabbitMQ or Kafka if needed. The `Subscribe<T>` / `Publish` interface remains the same.
4. **Extracted Services** — Extract bounded contexts into independently deployable services. Because they already communicate only through events, the extraction is primarily a deployment change, not a code change.
5. **Production Cluster** — Containerised services running on Kubernetes with auto‑scaling, health checks, and distributed tracing.

Each stage is optional and can be deferred until the operational need arises. The business logic remains unchanged throughout.

Yes, let's continue with Chapter 3 — one of the most critical chapters, as it defines exactly how the Intelligence Specifications become working software.

---

# Chapter 3 — Bounded Context Realisation  

## 3.1 Domain‑to‑Module Mapping  

The five Intelligence Specifications map directly to five bounded contexts within the Medhavi codebase. Supporting contexts provide master data and integration capabilities.

| Intelligence Domain | Bounded Context (F# Project) | Primary Modules |
|---------------------|------------------------------|-----------------|
| Demand Intelligence | `Medhavi.Demand` | Forecast, Segmentation, Prioritisation, Quality, Exceptions, Explain, Learn |
| Supply Intelligence | `Medhavi.Supply` | Plan, Inventory, Capacity, Procurement, Production, Distribution, Collaboration, Quality, Exceptions, Explain, Learn |
| Promise Intelligence | `Medhavi.Promise` | Orders, Promise (ATP/CTP), Allocation, Prioritisation, Changes, Collaboration, Risk, Quality, Exceptions, Explain, Learn |
| Scenario Intelligence | `Medhavi.Scenario` | Define, Simulate, Compare, Risk, Recommend, Collaborate, Trigger, Quality, Exceptions, Explain, Learn |
| Knowledge Intelligence | `Medhavi.Knowledge` | Govern, Discover, Root Cause, Portfolio, Best Practice, Feedback, Memory, Serve, Quality, Explain, Learn |
| Master Data | `Medhavi.MasterData` | Product, BOM, Routing, Resource, Calendar, Supplier, Customer, Location |
| External Integration | `Medhavi.Integration` | ACL, Ingest, Normalisation, External Event Adapters |

Each Intelligence Capability (from Chapter 5 of the Domain Specifications) becomes a module within its bounded context. The mapping is one‑to‑one: the capability specification is the source of truth; the module is its technical realisation.

## 3.2 Capability‑to‑Module Translation Rules  

Every Intelligence Capability is specified with the same anatomy. The translation from specification to code follows consistent rules:

| Specification Element | Code Translation |
|-----------------------|------------------|
| **Semantic Objects** | F# types in the `Domain` module (records, discriminated unions). Each object gets a type with the same name and fields. |
| **Enterprise Inputs** | Function parameters in the `Application` layer. Each input becomes a typed argument or a record of arguments. |
| **Enterprise Understanding Produced** | Return types of domain functions. The output of a capability is a typed result. |
| **Enterprise Outputs** | Event types appended to the event stream. Each output becomes an event case in a discriminated union. |
| **Business Decisions** | Pure functions in the `Domain` module. Each decision is a function that takes current state and command, and returns new events or errors. |
| **Rules** | Validation functions within the decision function. Each rule is a pure boolean or Result‑returning function. |
| **Policies** | Policy check functions called by the application service before or after the decision. Policies may query external state (e.g., user authorisation) and are injected as dependencies. |
| **Commands** | API endpoint DTOs in `Medhavi.Contracts` and command handler functions in the `Application` module. |
| **Events** | Cases in the aggregate’s event discriminated union, stored in the event store. |
| **Queries** | Pure functions over the read model, exposed as query service methods. |
| **Functional Behaviour** | The orchestration logic in the application service that wires commands → decisions → events → policies. |

The translation is deterministic. Given a capability specification, a developer can produce the code without ambiguity.

## 3.3 Internal Structure of a Bounded Context  

Every bounded context follows the same folder and module structure. This consistency ensures that any developer can navigate any context.

```
Medhavi.Demand/
├── Domain/
│   ├── DemandLineAgg.fs         # Aggregate state, events, decision functions
│   ├── DemandLine.fs            # Domain types (DemandLine, DemandStatus, etc.)
│   ├── ForecastAgg.fs           # Forecast aggregate (if separate)
│   ├── DemandCategory.fs        # Enumerations and value objects
│   └── DemandErrors.fs          # Domain error types
│
├── Application/
│   ├── ACL.fs                   # Anti‑Corruption Layer (external → domain translation)
│   ├── DemandCapabilities.fs    # Command handlers, decision orchestration
│   └── DemandApi.fs             # Public API composition for Nexus / Hub
│
├── Projections/
│   ├── DemandProjection.fs      # Read model state and evolution
│   └── DemandQueryService.fs    # Query functions over the read model
│
└── BoundedContext.fs            # Composition root for this context (registers repos, agents, APIs)
```

**Domain layer** — Pure F#. No external dependencies. No I/O. Contains:

- **Aggregate types**: the shape of the state (e.g., `DemandLine` record).
- **Event types**: a discriminated union of all events this aggregate can emit.
- **Decision functions**: `decide : State -> Command -> Result<Event list, DomainError>`.
- **Evolution functions**: `evolve : State -> Event -> State` (fold).

**Application layer** — May depend on Domain and SharedKernel. Contains:

- **ACL**: translates external DTOs (`DemandDefineReq`) into domain commands (`IngestDemandLineCmd`), with validation.
- **Capabilities**: functions that load an aggregate, call `decide`, append events, and handle policies.
- **API composition**: the public interface exposed to Nexus.

**Projections layer** — Subscribes to events and maintains a read model. Contains:

- **Projection state**: a type representing the read model (often a `Map<string, Dto>`).
- **Evolution function**: `evolve : ReadModel -> Event -> ReadModel`.
- **Query service**: pure functions over the read model.

**BoundedContext module** — The composition root. Creates the repository, projection agent, capabilities, and API. Registers event subscriptions.

## 3.4 Example: Translating “Forecast Demand” into Software  

This section demonstrates the full translation of a single capability—**Forecast Demand (CA‑DI‑002)** from the Demand Intelligence Specification—into working code.

### 3.4.1 Specification as Source of Truth  

The Forecast Demand capability specification defines:

- **Semantic Objects**: `Forecast`, `Forecast Model`, `Prediction Interval`, `Forecast Confidence`, `Forecast Override`, `Forecast Cycle`.
- **Commands**: `GenerateForecast`, `SelectChampionModel`, `OverrideForecast`, `PublishForecast`, `ApproveForecastPublication`.
- **Decisions**: `DE‑DI‑020` (Select Champion), `DE‑DI‑021` (Generate Baseline), `DE‑DI‑022` (Publish), `DE‑DI‑023` (Override).
- **Rules**: `BR‑DI‑020` (Champion Selection Significance), `BR‑DI‑023` (Forecast Validity), `BR‑DI‑026` (Publication Completeness), etc.
- **Policies**: `PO‑DI‑020` (Champion Promotion Approval), `PO‑DI‑023` (Auto‑Publication), etc.
- **Events**: `ForecastGenerated`, `ForecastPublished`, `ForecastOverridden`, etc.

### 3.4.2 Derived Domain Types  

```fsharp
// Domain/ForecastTypes.fs
type PredictionInterval = {
    LowerBound: decimal
    UpperBound: decimal
    ConfidenceLevel: decimal  // e.g., 0.90
}

type ForecastConfidence = ForecastConfidence of decimal  // 0.0 – 100.0

type ForecastModel = {
    ModelId: string
    ModelType: string        // "ExponentialSmoothing", "ARIMA", "NeuralNetwork"
    Hyperparameters: Map<string, string>
    TrainedAt: DateTimeOffset
}

type Forecast = {
    ForecastId: string
    ProductId: string
    LocationId: string
    TimeBucket: DateTimeOffset
    Mean: decimal
    PredictionInterval: PredictionInterval
    Confidence: ForecastConfidence
    ModelId: string
    GeneratedAt: DateTimeOffset
    OverrideReason: string option
}
```

### 3.4.3 Derived Event Types  

```fsharp
// Domain/ForecastEvents.fs
type ForecastEvent =
    | ForecastGenerated of Forecast
    | ForecastOverridden of ForecastId * decimal * string  // id, newValue, reason
    | ForecastPublished of ForecastId * DateTimeOffset
    | ModelChampionSelected of newModelId: string * oldModelId: string * metrics: Map<string, decimal>
    | ForecastApprovalRequired of ForecastId * reason: string
```

#### 3.4.4 Derived Commands  

```fsharp
// Application/ForecastCommands.fs  (or in Medhavi.Contracts)
type GenerateForecastCmd = {
    HorizonStart: DateTimeOffset
    HorizonEnd: DateTimeOffset
    ProductIds: string list option
}

type SelectChampionModelCmd = {
    CandidateModelId: string
    EvaluationWindowStart: DateTimeOffset
    EvaluationWindowEnd: DateTimeOffset
}

type OverrideForecastCmd = {
    ForecastId: string
    NewValue: decimal
    Justification: string
}

type PublishForecastCmd = {
    ForecastCycleId: string
}
```

### 3.4.5 Derived Decision Functions  

Each decision from the specification becomes a pure function:

```fsharp
// Domain/ForecastDecisions.fs

// DE‑DI‑020: Select Champion Forecast Model
let selectChampion (currentChampion: ForecastModel) 
                   (candidate: ForecastModel) 
                   (evaluationMetrics: Map<string, decimal>)
                   : Result<ForecastEvent list, DomainError> =
    
    // BR‑DI‑020: Statistical significance check
    let candidateWAPE = evaluationMetrics.["candidate_wape"]
    let championWAPE = evaluationMetrics.["champion_wape"]
    let significance = evaluationMetrics.["p_value"]
    
    if significance > 0.05m then
        Error (DomainError.validation "Candidate improvement is not statistically significant")
    elif candidateWAPE >= championWAPE then
        Error (DomainError.validation "Candidate does not improve WAPE")
    else
        Ok [ ModelChampionSelected(candidate.ModelId, currentChampion.ModelId, evaluationMetrics) ]

// DE‑DI‑023: Override Forecast
let overrideForecast (forecast: Forecast) 
                     (newValue: decimal) 
                     (justification: string)
                     : Result<ForecastEvent list, DomainError> =
    
    // BR‑DI‑027: Justification required
    if String.IsNullOrWhiteSpace justification then
        Error (DomainError.validation "Override justification is required")
    // BR‑DI‑028: Deviation limit
    elif abs (newValue - forecast.Mean) / forecast.Mean > 0.50m then
        Error (DomainError.validation "Override exceeds 50% deviation limit")
    else
        Ok [ ForecastOverridden(forecast.ForecastId, newValue, justification) ]
```

### 3.4.6 Derived Application Service  

The application service wires commands to the aggregate, decisions, and policies:

```fsharp
// Application/ForecastCapabilities.fs
type ForecastCapabilities = {
    GenerateForecast: GenerateForecastCmd -> TaskResult<unit, ApplicationError>
    OverrideForecast: OverrideForecastCmd -> TaskResult<unit, ApplicationError>
    PublishForecast: PublishForecastCmd -> TaskResult<unit, ApplicationError>
}

let createForecastCapabilities 
    (repo: Repository<ForecastAggregate, string, ForecastEvent>)
    (policyChecker: PolicyChecker)
    : ForecastCapabilities =
    
    let handleCommand cmdHandler =
        // Standard pattern: load aggregate, decide, append events, check policies
        fun cmd ->
            task {
                let! agg = repo.Load(cmd.AggregateId)
                match cmdHandler agg.State cmd with
                | Error err -> return Error (ApplicationError.Domain err)
                | Ok events ->
                    let! result = repo.Append(cmd.AggregateId, agg.Version, events)
                    match result with
                    | Error _ -> return Error (ApplicationError.Infrastructure ...)
                    | Ok _ ->
                        // Apply policies (non‑blocking, may raise additional events)
                        do! policyChecker.Evaluate(events)
                        return Ok ()
            }
    
    { GenerateForecast = handleCommand generateForecastDecision
      OverrideForecast = handleCommand overrideForecastDecision
      PublishForecast = handleCommand publishForecastDecision }
```

### 3.4.7 Derived Projection  

```fsharp
// Projections/ForecastProjection.fs
type ForecastReadModel = Map<string, ForecastDto>  // ForecastId → DTO

let evolveReadModel (state: ForecastReadModel) (event: ForecastEvent) : ForecastReadModel =
    match event with
    | ForecastGenerated f ->
        state |> Map.add f.ForecastId (ForecastDto.fromDomain f)
    | ForecastOverridden (fid, newVal, reason) ->
        state |> Map.change fid (Option.map (fun dto -> { dto with Mean = newVal; OverrideReason = Some reason }))
    | ForecastPublished (fid, ts) ->
        state |> Map.change fid (Option.map (fun dto -> { dto with PublishedAt = Some ts }))
    | _ -> state

let createForecastQueryService (agent: ProjectionAgent<ForecastReadModel, ForecastEvent>) =
    { GetForecast = fun fid -> agent.QueryAsync(fun state -> Map.tryFind fid state)
      GetAllForecasts = fun () -> agent.QueryAsync(Map.values >> Seq.toList) }
```

### 3.4.8 Wiring in the Bounded Context  

```fsharp
// BoundedContext.fs
let create () =
    let repo = InMemRepository.create<ForecastAggregate, string, ForecastEvent>()
    let forecastAgent = ProjectionAgent(evolveReadModel, Map.empty, "ForecastReadModel")
    let policyChecker = PolicyChecker.create()  // injects DecisionCore.PolicyGate
    
    // Subscribe projection to events
    DomainEventBus.Subscribe<ForecastEvent>(fun ev -> forecastAgent.Post(ev, Guid.NewGuid(), None))
    
    let capabilities = createForecastCapabilities repo policyChecker
    
    { ForecastApi = capabilities
      ForecastQueries = createForecastQueryService forecastAgent
      ForecastAgent = forecastAgent }
```


This example demonstrates the complete translation from specification to code. Every capability in every domain follows this same pattern. The specification is the source of truth; the code is its faithful realisation. No ambiguity. No interpretation required.

---

# Chapter 4 — Event Architecture  

## 4.1 Events as the Source of Truth  

Every state change in Medhavi is captured as an **immutable event**. Events are the single source of truth for both domain state and cross‑context integration. No component directly mutates another component’s data—they communicate exclusively through events.

An event is a fact. It records something that has already happened:

- `DemandLineIngested` — a demand line has been accepted into the system.  
- `ForecastGenerated` — a forecast has been produced.  
- `SupplyPlanPublished` — a supply plan has been released for execution.  
- `PromiseConfirmed` — a customer order has been promised.  
- `CrossDomainPatternDiscovered` — a systemic pattern spanning multiple domains has been identified.

Each event is stored immutably in a PostgreSQL `events` table. The event stream for an aggregate can be replayed at any time to reconstruct its state. This is the foundation of auditability, traceability, and resilience.

## 4.2 The `Envelope` Type  

Every event is wrapped in an `Envelope` before it is stored or published. The `Envelope` carries metadata that enables distributed tracing, idempotency, schema evolution, and debugging—without cluttering the domain event itself.

```fsharp
type Envelope = {
    EventId: Guid              // Unique identifier for this event
    EventType: string          // Fully qualified event type (e.g., "Demand.DemandLineIngested")
    DataJson: string           // Serialised domain event payload
    SchemaVersion: int         // Schema version for evolution and upcasting
    StreamName: string         // The aggregate stream this event belongs to
    CreatedUtc: DateTimeOffset // Infrastructure timestamp (when persisted)
    CorrelationId: Guid option // Links events across aggregates and services
    CausationId: Guid option   // The event or command that directly caused this event
    TenantId: string option    // Multi‑tenancy partition key
    Metadata: Map<string, string> // Extensible metadata for tracing and context
}
```

**Metadata conventions** (populated from `ExecutionContext`):

| Key | Value | Purpose |
|-----|-------|---------|
| `correlationId` | Guid | Groups all events belonging to the same business flow |
| `causationId` | Guid | The immediate parent event or command |
| `principal` | string | The user or system that initiated the action |
| `aggregateId` | string | The aggregate root identifier |
| `aggregateType` | string | The aggregate type name |
| `messageId` | string | Client‑supplied idempotency key |

Every event appended to the store **must** carry at minimum `correlationId` and `causationId`. The `Envelope` module provides helper functions (`withExecutionContext`, `withAggregateContext`) to enrich the envelope from the current `ExecutionContext`.

## 4.3 PostgreSQL `events` Table  

The single source of truth for all events is a PostgreSQL table. This schema is designed for simplicity, performance, and compatibility with the existing `Envelope` type.

```sql
CREATE TABLE events (
    stream_name      TEXT        NOT NULL,
    stream_position  BIGINT      NOT NULL,
    event_id         UUID        NOT NULL,
    event_type       TEXT        NOT NULL,
    data_json        JSONB       NOT NULL,
    metadata_json    JSONB       NOT NULL,
    created_utc      TIMESTAMPTZ NOT NULL DEFAULT now(),
    tenant_id        TEXT        NULL,          -- nullable for MVP, indexed for multi-tenant

    PRIMARY KEY (stream_name, stream_position)
);

CREATE INDEX idx_events_created   ON events (created_utc);
CREATE INDEX idx_events_type     ON events (event_type, created_utc);
CREATE INDEX idx_events_tenant   ON events (tenant_id, created_utc) WHERE tenant_id IS NOT NULL;
CREATE INDEX idx_events_correlation ON events ((metadata_json->>'correlationId'), created_utc);
```

**Stream naming conventions:**

- Aggregate streams: `{domain}-{aggregateType}-{aggregateId}` (e.g., `demand-DemandLine-DL-001`, `supply-SupplyPlan-SP-2026-W27`).
- Category streams: `$ce-{domain}` (projections that consume all events from a domain).
- Integration streams: `$integration` (events explicitly published for other bounded contexts).
- System streams: `$checkpoint`, `$snapshot`.

**Append rules:**

- Writes to a stream must specify an **expected stream position** (`ExpectedRevision`). The database enforces optimistic concurrency: if another writer has appended since the read, the write is rejected and the caller must retry.
- Writes are atomic per stream. A batch of events appended to a single stream either all succeed or all fail.

## 4.4 Event Bus  

The event bus is the mechanism by which events are delivered from publishers to subscribers across bounded contexts.

### 4.4.1 MVP: In‑Process `DomainEventBus`  

In the MVP, all bounded contexts run in the same process within `Medhavi.Nexus`. The event bus is a lightweight in‑process pub/sub:

```fsharp
type DomainEventBus =
    static let eventObj = Event<obj>()

    static member Publish(evt: obj) = eventObj.Trigger(evt)

    static member Subscribe<'T>(handler: 'T -> unit) : IDisposable =
        eventObj.Publish
        |> Observable.choose (fun o ->
            match o with
            | :? 'T as e -> Some e
            | _ -> None)
        |> Observable.subscribe handler
```

Publishers call `DomainEventBus.Publish(envelope)`. Subscribers register typed handlers. This is fast, simple, and sufficient for a single‑process deployment.

### 4.4.2 Production: PostgreSQL `LISTEN`/`NOTIFY`  

When the system scales to multiple processes, the in‑process bus is replaced by a PostgreSQL‑backed implementation that uses the same `events` table as the transport:

1. **Append** — Events are written to the `events` table as before.
2. **Notify** — A PostgreSQL trigger or application‑level call issues `NOTIFY event_channel, '<event_id>'` after each successful append.
3. **Subscribe** — Subscribers open a persistent connection and `LISTEN event_channel`. On notification, they read the new event from the `events` table at their last checkpoint position.

The subscriber interface remains unchanged:

```fsharp
type EventBusPort = {
    Publish: Envelope -> Task<unit>
    Subscribe: SubscriptionMode -> Position option -> (EnvelopedEvent -> Task<unit>) -> Task<SubscriptionHandle>
}
```

The same interface works for in‑process, PostgreSQL `LISTEN`/`NOTIFY`, and later RabbitMQ or Kafka. The bounded contexts never know which implementation is active.

## 4.5 Checkpointing and Idempotency  

Projections and subscribers must be able to resume processing from where they left off after a restart, and must never process the same event twice.

### 4.5.1 Checkpoints  

A checkpoint records the last successfully processed position in the event stream.

```fsharp
type Checkpoint = {
    StreamName: string
    LastPosition: int64
    LastMessageId: Guid
    UpdatedUtc: DateTimeOffset
}
```

Checkpoints are stored in a dedicated `checkpoints` table:

```sql
CREATE TABLE checkpoints (
    projection_name  TEXT PRIMARY KEY,
    last_position    BIGINT NOT NULL,
    last_message_id  UUID   NOT NULL,
    updated_utc      TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

On startup, a projection reads its checkpoint and resumes from `last_position + 1`. After processing a batch of events, it updates the checkpoint atomically.

### 4.5.2 Idempotency  

Idempotency ensures that an event is never processed twice, even if it is delivered more than once. The system uses a two‑tier approach:

| Tier | Implementation | Purpose |
|------|----------------|---------|
| **In‑memory cache** | `ConcurrentDictionary<Guid, bool>` | Fast duplicate detection within the same process lifetime |
| **Persistent store** | PostgreSQL `idempotency` table | Survives restarts; enables cluster‑wide deduplication |

```sql
CREATE TABLE idempotency (
    message_id UUID PRIMARY KEY,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

Before processing any event, the handler checks the in‑memory cache, then the persistent store. After successful processing, it records the event’s `messageId` in both.

## 4.6 Schema Evolution  

Event schemas evolve over time. The `SchemaVersion` field in the `Envelope` enables backward‑compatible changes without rewriting history.

**Upcasting strategy:**

```fsharp
type Upcaster = {
    FromVersion: int
    ToVersion: int
    Upcast: JObject -> JObject  // transforms old JSON to new JSON
}
```

Upcasters are registered by event type at application startup. When a projection or subscriber reads events at an older version, the upcaster is applied before the event reaches the handler.

**Rules:**

- **Additive changes** (new fields with defaults): increment minor version. Upcaster fills in defaults.
- **Breaking changes** (renamed fields, changed types): increment major version. Upcaster transforms the old shape to the new shape.
- **Never mutate** the original stored event. Upcasters are applied at read time.

## 4.7 Cross‑Context Event Routing  

Events produced by one bounded context are consumed by others according to the dependency chains defined in the Intelligence Specifications. The following table is the authoritative routing map.

| Publisher Domain | Event Type | Consumer Domain(s) | Purpose |
|------------------|------------|-------------------|---------|
| Demand | `ForecastGenerated` | Supply, Scenario | Drives supply planning and scenario simulations |
| Demand | `ForecastPublished` | Supply, Promise, Scenario | Authoritative forecast for downstream planning |
| Demand | `DemandChangeDetected` | Supply, Scenario | Triggers replanning and what‑if analysis |
| Supply | `SupplyPlanGenerated` | Promise, Scenario | Enables ATP/CTP evaluation and scenario comparison |
| Supply | `SupplyPlanPublished` | Promise, Scenario, Knowledge | Authoritative supply plan for all downstream consumers |
| Supply | `InventoryPositionUpdated` | Promise, Knowledge | Real‑time ATP and cross‑domain analytics |
| Supply | `CapacityFeasibilityAssessed` | Scenario | Feeds capacity constraints into simulations |
| Supply | `SupplierCommitmentEvaluated` | Promise | Updates supply availability for promising |
| Promise | `PromiseConfirmed` | Supply, Scenario, Knowledge | Consumes supply; triggers replanning; feeds enterprise memory |
| Promise | `PromiseBreached` | Knowledge | Cross‑domain pattern discovery and root‑cause analysis |
| Scenario | `ScenarioRecommendationAdopted` | Demand, Supply, Promise | Updates operational plans in all affected domains |
| Scenario | `ScenarioComparisonCompleted` | Knowledge | Cross‑domain analytics and learning |
| Knowledge | `CrossDomainPatternDiscovered` | All domains | Alerts domains to systemic patterns |
| Knowledge | `BestPracticePublished` | All domains | Propagates proven strategies across domains |

Each consumer maintains its own projection of the events it needs, built from the event store using its own checkpoint. There is no direct database access across bounded contexts—only events.

---

# Chapter 5 — Command Handling & Concurrency  

## 5.1 Stateless Command Execution  

The default command execution path is **stateless**. Any application node can handle any command for any aggregate. The node loads the aggregate’s event stream from the event store, reconstructs the current state, runs the domain decision function, and appends new events.

This is the core loop:

```
Command arrives (from API, event handler, or scheduler)
        │
        ▼
Application Service receives command with ExecutionContext
        │
        ▼
Repository.Load(aggregateId)
        │
        ▼
Aggregate state reconstructed from event stream (fold)
        │
        ▼
Decision function: decide(state, command) → events | error
        │
        ▼
Repository.Append(aggregateId, expectedVersion, events)
        │
        ▼
Domain events published to DomainEventBus
        │
        ▼
Telemetry recorded (decision trace, performance metrics)
```

The application service code for a generic command handler:

```fsharp
let handleCommand
    (repo: Repository<'Aggregate, 'Id, 'Event>)
    (decide: State -> Command -> Result<'Event list, DomainError>)
    (command: Command)
    : Task<Result<unit, ApplicationError>> =
    task {
        let! aggregate = repo.Load(command.AggregateId)
        match aggregate with
        | Error e -> return Error (ApplicationError.Infrastructure e)
        | Ok agg ->
            match decide agg.State command with
            | Error e -> return Error (ApplicationError.Domain e)
            | Ok events ->
                let! result = repo.Append(command.AggregateId, agg.Version, events)
                match result with
                | Error e -> return Error (ApplicationError.Infrastructure e)
                | Ok _ -> return Ok ()
    }
```

The application service is thin. It orchestrates but does not contain business logic. The `decide` function is pure and testable in isolation.

## 5.2 Optimistic Concurrency Control  

Multiple nodes may handle commands for the same aggregate concurrently. Medhavi uses **optimistic concurrency** to prevent conflicting writes without pessimistic locks.

When the repository appends events, it passes the expected stream version:

```fsharp
type ExpectedRevision =
    | Any
    | NoStream
    | StreamRevision of int64

type AppendResult = {
    NewVersion: int64
    Success: bool
}
```

The PostgreSQL implementation uses the `stream_position` column as the concurrency token:

```sql
-- Append function checks expected version atomically
INSERT INTO events (stream_name, stream_position, ...)
SELECT @stream, COALESCE(MAX(stream_position), -1) + 1, ...
FROM events WHERE stream_name = @stream
HAVING COALESCE(MAX(stream_position), -1) = @expectedVersion;
```

If the `HAVING` clause fails (another writer appended since the read), the insert returns zero rows. The repository raises a `ConcurrencyException`.

The application service handles the concurrency exception with a **retry loop**:

```fsharp
let rec handleWithRetry (maxRetries: int) (cmd: Command) =
    task {
        let mutable retries = 0
        let mutable result = None
        while retries <= maxRetries && result.IsNone do
            let! attempt = handleCommand repo decide cmd
            match attempt with
            | Ok _ -> result <- Some (Ok ())
            | Error (ApplicationError.Infrastructure (ConcurrencyError _)) when retries < maxRetries ->
                retries <- retries + 1
                do! Task.Delay (100 * (1 <<< retries))  // exponential backoff
            | Error e -> result <- Some (Error e)
        return result |> Option.defaultValue (Error (ApplicationError.Infrastructure (ConcurrencyError "Max retries exceeded")))
    }
```

For the vast majority of aggregates, the default retry count of 3 with exponential backoff is sufficient to resolve contention without noticeable latency.

## 5.3 Hotspot Concurrency — `MailboxProcessor`  

For high‑contention aggregates—particularly the **ATP/CTP engine** in Promise Intelligence and **capacity reservation** in Supply Intelligence—stateless optimistic concurrency can cause unacceptable retry rates and latency under load.

These hotspots are handled by **F# MailboxProcessor agents**. An agent serialises all commands for its assigned partition, eliminating contention entirely.

### 5.3.1 When to Use an Agent  

An aggregate or partition qualifies for a `MailboxProcessor` agent if:

- It experiences **high command volume** (hundreds of commands per second).
- Contention is **a business risk** (e.g., double‑booking inventory).
- **Latency must be predictable** and retries are unacceptable.
- The aggregate state can be **cached in memory** for rapid access.

The ATP/CTP engine is the primary example. Promise evaluations are high‑frequency, low‑latency, and require absolute correctness in supply consumption.

### 5.3.2 Agent Lifecycle  

Each agent is assigned a **partition key**—typically `SkuId` or `NodeId`—using consistent hashing. Only one agent owns a given partition across the entire cluster.

```
API Gateway receives PromiseRequest
        │
        ▼
Consistent Hash Router maps (SkuId, NodeId) → partition
        │
        ▼
Request routed to the node owning that partition
        │
        ▼
MailboxProcessor agent for that partition
        │
        ▼
Sequential processing:
  1. Validate command against in‑memory balance cache
  2. Execute decision function
  3. Append events to event store (single writer, no contention)
  4. Update in‑memory cache
  5. Return result to caller
```

The agent’s internal state:

```fsharp
type AgentState = {
    BalanceCache: Map<SkuId, InventoryBalance>
    LastStreamVersion: int64
    Stats: AgentStats
}
```

### 5.3.3 Crash Recovery  

If the node hosting the agent crashes, a new agent is initialised on another node:

1. The new agent reads the partition’s event stream from PostgreSQL from the last known checkpoint.
2. It replays all events into its in‑memory `BalanceCache`.
3. Once replay is complete, it begins accepting commands.

This is the same replay mechanism used by projections. The warm‑up time depends on the event volume for that partition, typically seconds.

## 5.4 Aggregate State Reconstruction  

Aggregates are not persisted as current state. They are **reconstructed on demand** by folding their event streams.

```fsharp
type Repository<'Aggregate, 'Id, 'Event> = {
    Load: 'Id -> Task<Result<AggregateInstance * int64, StoreError>>
    Append: 'Id -> int64 -> 'Event list -> Task<Result<unit, StoreError>>
}
```

The `Load` function:

1. Reads all events for the stream `{domain}-{aggregateType}-{id}` from the `events` table, ordered by `stream_position`.
2. Folds the events using the aggregate’s `evolve` function: `fold evolve initialState events`.
3. Returns the reconstructed state and the current stream version.

For aggregates with long event histories, a **snapshot** can be used to speed up reconstruction. The snapshot stores a point‑in‑time state at a known `stream_position`. On `Load`, the most recent snapshot is loaded first, then only events after that position are replayed.

Snapshots are optional. The system is correct without them; they exist purely for performance.

## 5.5 Repository Pattern  

The repository abstraction hides the event store implementation behind a pure interface. This enables the MVP to use an in‑memory store and production to use PostgreSQL—without changing any domain or application code.

### 5.5.1 In‑Memory Repository (MVP)  

```fsharp
type InMemRepository<'Aggregate, 'Id, 'Event>() =
    let streams = ConcurrentDictionary<string, ('Event list * int64)>()

    member this.Load(id: string) =
        match streams.TryGetValue(id) with
        | true, (events, version) -> Ok ({ State = fold evolve initialState events }, version)
        | false, _ -> Ok ({ State = initialState }, -1L)

    member this.Append(id: string, expectedVersion: int64, events: 'Event list) =
        // Check concurrency, append, increment version
        ...
```

The in‑memory repository is single‑process only and does not survive restarts. It is intended for development and MVP testing. The interface is identical to the PostgreSQL implementation.

### 5.5.2 PostgreSQL Repository (Production)  

The production repository implements the same interface using the `events` table:

```fsharp
type PostgresRepository<'Aggregate, 'Id, 'Event>(conn: NpgsqlConnection, evolve: State -> 'Event -> State, initialState: State) =
    
    member this.Load(id: string) =
        task {
            let! events = conn.QueryAsync<Envelope>(
                "SELECT * FROM events WHERE stream_name = @stream ORDER BY stream_position", 
                {| stream = streamName id |})
            let domainEvents = events |> List.map deserialize<'Event>
            let version = events |> List.tryLast |> Option.map (fun e -> e.StreamPosition) |> Option.defaultValue -1L
            let state = domainEvents |> List.fold evolve initialState
            return Ok ({ State = state }, version)
        }

    member this.Append(id: string, expectedVersion: int64, domainEvents: 'Event list) =
        task {
            let envelopes = domainEvents |> List.map (toEnvelope id)
            // Atomic append with version check (as described in 5.2)
            ...
        }
```

## 5.6 Command Validation Flow  

Before a command reaches the aggregate decision function, it passes through the **Anti‑Corruption Layer (ACL)**. The ACL:

1. Deserialises the incoming DTO.
2. Validates primitive types (non‑negative quantities, valid dates, required fields present).
3. Maps external identifiers to domain identifiers (e.g., string `SkuId` → `SkuId` type with validation).
4. Produces a domain command or a validation error.

This is a pure function with no side effects:

```fsharp
module ACL =
    let toIngestCommand (req: DemandDefineReq) : Validation<IngestDemandLineCmd, DomainError> =
        make
        <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)
```

The `Validation` applicative ensures all fields are validated independently and errors are accumulated. A valid `Command` is guaranteed to be well‑formed before it reaches the domain.

## 5.7 Command → Event → Policy Pipeline  

The complete pipeline for a command is:

1. **ACL** — External DTO → Domain Command (pure validation).  
2. **Load** — Reconstruct aggregate state from event stream.  
3. **Decide** — Pure domain function: `State → Command → Result<Event list, DomainError>`.  
4. **Append** — Write events to the event store with optimistic concurrency.  
5. **Publish** — Publish events to the `DomainEventBus` for projections and other contexts.  
6. **Policy Check** — Evaluate any policies that govern the decision (non‑blocking, may raise additional events like `ApprovalRequired`).  
7. **Telemetry** — Record the decision trace with ARS identifiers, correlation, and performance metrics.

Policies are evaluated **after** events are appended. They do not block the command from succeeding—they may trigger an approval workflow or an escalation, but they never roll back the decision. If a policy requires blocking approval (e.g., `PO‑DI‑020` requires manager approval for champion model promotion), the decision itself emits a `PendingApproval` event rather than the final outcome event. The approval workflow is a separate process that later emits the final event.

---

# Chapter 6 — Query Side & Projections  

## 6.1 CQRS Separation  

Medhavi separates command processing from query processing. The command side writes events to the event store. The query side reads from **projections**—optimised read models built by subscribing to those events.

This separation means:

- **Queries are fast.** Read models are in‑memory data structures, purpose‑built for the queries they serve. No joins, no complex SQL.
- **Queries are simple.** They are pure functions over immutable state. No business logic, no side effects.
- **Queries are real‑time.** When an event is appended, the projection updates immediately (in‑process) or within milliseconds (distributed).
- **Queries are isolated.** The read model for one bounded context is never directly accessed by another context.

## 6.2 The Projection Agent  

The core of the query side is the **projection agent**—an F# `MailboxProcessor` that holds the read model state and processes events sequentially.

```fsharp
type ProjectionAgent<'State, 'Event>(applyFn: 'State -> 'Event -> 'State, initial: 'State, name: string) =
    let agent =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (state: 'State, lastMessageId: Guid option, stats: ProjectionStats) =
                async {
                    let! msg = inbox.Receive()
                    match msg with
                    | Apply(event, messageId) ->
                        // Idempotency check
                        match lastMessageId with
                        | Some lm when lm = messageId -> return! loop (state, lastMessageId, stats)
                        | _ ->
                            let newState = applyFn state event
                            let newStats = { stats with EventsProcessed = stats.EventsProcessed + 1L }
                            return! loop (newState, Some messageId, newStats)
                    | GetState(reply) ->
                        reply.Reply state
                        return! loop (state, lastMessageId, stats)
                    | Query(query, reply) ->
                        let result = query state
                        reply.Reply result
                        return! loop (state, lastMessageId, stats)
                }
            loop (initial, None, ProjectionStats.Default))
```

A projection agent:

- **Serialises all updates.** Events are applied one at a time by a single logical thread. No locks, no race conditions.
- **Is idempotent.** The `messageId` check prevents double‑processing of the same event.
- **Exposes queries.** Callers post a `Query` message with a pure function `'State -> 'Result` and receive the result asynchronously.
- **Is isolated.** Each projection agent owns its read model. No shared mutable state.

Every bounded context creates one or more projection agents for its read models. For example, Demand Intelligence creates a `DemandProjectionAgent` for the demand line read model and a `ForecastProjectionAgent` for the forecast read model.

## 6.3 Event Evolution Functions  

The core of a projection is its **evolution function**: a pure function that takes the current read model state and an event, and returns the new state.

```fsharp
type DemandReadModel = Map<string, DemandLineDto>

let evolveDemandReadModel (state: DemandReadModel) (event: DemandLineEvent) : DemandReadModel =
    match event with
    | DemandLineIngested dl ->
        state |> Map.add dl.DemandLineId (mapToDto dl)
    | DemandLineRevised evt ->
        state |> Map.change evt.DemandLineId (Option.map (updateDtoWithRevision evt))
    | DemandLinePromised evt ->
        state |> Map.change evt.DemandLineId (Option.map (updateDtoWithPromise evt))
    | DemandLineFrozen evt ->
        state |> Map.change evt.DemandLineId (Option.map (fun dto -> { dto with IsFrozen = true }))
    | DemandLineFulfilled evt ->
        state |> Map.change evt.DemandLineId (Option.map (fun dto -> 
            { dto with Status = "Fulfilled"; FulfilledQty = dto.FulfilledQty + evt.Quantity }))
```

Key design rules:

- **One evolution function per read model.** If a projection consumes events from multiple aggregate types, the function pattern‑matches on all of them.
- **The function is pure.** No I/O, no randomness, no external state. Given the same state and event, it always produces the same new state.
- **The function is total.** Every event case is handled, even if the handler is a no‑op (`state`).
- **DTOs are pre‑computed.** The evolution function computes everything needed by queries. Queries never do calculation; they only return pre‑computed values.

## 6.4 Startup: Rebuilding Projections from Events  

When the application starts, every projection agent must rebuild its read model from the event store. The sequence is:

1. **Read checkpoint.** The agent queries the `checkpoints` table for its last known position.
2. **Replay events.** It reads all events from the event store starting from that position (or from the beginning if no checkpoint exists).
3. **Apply events.** Each event is folded through the evolution function to rebuild the in‑memory state.
4. **Subscribe to live events.** Once the replay is complete, the agent subscribes to the `DomainEventBus` (or PostgreSQL `LISTEN`/`NOTIFY`) for new events.
5. **Update checkpoint.** After processing each batch of live events, the checkpoint is updated.

```fsharp
let rebuildProjection (agent: ProjectionAgent<'State, 'Event>) 
                      (store: EnvelopeStoreOps) 
                      (checkpointStore: CheckpointStore)
                      (projectionName: string) =
    task {
        // 1. Load checkpoint
        let! checkpoint = checkpointStore.ReadCheckpoint projectionName
        let startPosition = checkpoint |> Option.map (fun c -> c.LastPosition) |> Option.defaultValue 0L

        // 2. Replay events from store
        let! events = store.ReadAll (Some startPosition) None CancellationToken.None
        match events with
        | Ok envelopedEvents ->
            for e in envelopedEvents do
                let domainEvent = Envelope.deserialize<'Event> e.Envelope
                match domainEvent with
                | Ok ev -> agent.Post(ev, e.Envelope.EventId)
                | Error _ -> () // skip events that don't match this projection's type
        | Error _ -> () // log and continue; projection can catch up later

        // 3. Subscribe to live events
        let! sub = store.Subscribe All (Some startPosition) (fun envelopedEvent ->
            task {
                let domainEvent = Envelope.deserialize<'Event> envelopedEvent.Envelope
                match domainEvent with
                | Ok ev -> agent.Post(ev, envelopedEvent.Envelope.EventId)
                | Error _ -> ()
            }) CancellationToken.None

        return sub
    }
```

The replay is performed **once at startup per agent**. For the MVP with an in‑memory store, the repository can be seeded with initial data, and the replay is instantaneous. For production, the replay time depends on the event volume, but the system is designed so that a projection can catch up from any position without blocking queries—queries simply see a progressively more current view.

## 6.5 Optional Projection Snapshots  

For projections with very large event histories (hundreds of thousands of events), replaying from the beginning on every restart can be slow. A **snapshot** captures the entire read model state at a specific stream position.

A snapshot is:

- Stored in the `snapshots` table (or as a serialised blob in PostgreSQL).
- Associated with a `stream_position` and a `projection_name`.
- Created periodically (e.g., every 10,000 events) or on demand.

On startup, instead of replaying from the beginning:

1. Load the most recent snapshot for the projection.
2. Replay only events after the snapshot’s `stream_position`.
3. Continue with live subscription.

```sql
CREATE TABLE snapshots (
    projection_name  TEXT   NOT NULL,
    stream_position  BIGINT NOT NULL,
    state_json       JSONB  NOT NULL,
    created_utc      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (projection_name, stream_position)
);
```

Snapshots are an optimisation, not a correctness requirement. The system is fully correct without them; they exist purely to reduce warm‑up time.

## 6.6 Query Service Design  

Queries are exposed through **query services**—records of pure functions that the application and API layers call to retrieve data.

```fsharp
type DemandQueries = {
    GetById: DemandLineId -> Task<DemandLineDto option>
    GetAll: unit -> Task<DemandLineDto list>
    GetByCustomer: CustomerId -> Task<DemandLineDto list>
    GetByStatus: DemandLineStatus -> Task<DemandLineDto list>
    GetActiveCount: unit -> Task<int>
}
```

Each function is implemented by posting a `Query` message to the projection agent:

```fsharp
let createDemandQueryService (agent: ProjectionAgent<DemandReadModel, DemandLineEvent>) : DemandQueries =
    { GetById = fun id -> agent.QueryAsync(fun state -> state |> Map.tryFind (id.ToString()))
      GetAll = fun () -> agent.QueryAsync(fun state -> state.Values |> Seq.toList)
      GetByCustomer = fun custId -> 
          agent.QueryAsync(fun state -> 
              state.Values |> Seq.filter (fun d -> d.CustomerId = custId) |> Seq.toList)
      GetByStatus = fun status ->
          agent.QueryAsync(fun state -> 
              state.Values |> Seq.filter (fun d -> d.Status = status) |> Seq.toList)
      GetActiveCount = fun () -> agent.QueryAsync(fun state -> state.Count)
    }
```

Key design rules:

- **Query functions are pure.** They read from the in‑memory read model and return. No I/O, no mutation.
- **Queries run inside the agent.** The `QueryAsync` method posts a message to the agent’s mailbox. The agent executes the query function and returns the result. This ensures thread safety without locks.
- **Queries are non‑blocking for writes.** Because the agent processes messages sequentially, a long‑running query could theoretically delay event application. In practice, projection queries are simple map/filter operations that complete in microseconds. If a query is genuinely expensive (e.g., a complex aggregation), it can be run against a periodic snapshot of the state instead of the live agent.
- **Queries are versioned with the read model.** When the read model DTO changes, the query service changes with it. There is no separate API versioning layer between the query service and the read model.

## 6.7 Query‑Side Integration with the Store Pattern  

The query services are consumed by the **Store pattern** (Chapter 14). A Store wraps a query service and adds:

- **Freshness tracking** (Fresh, Stale, Loading, Failed).
- **Subscription management** (notifies the UI when the underlying read model changes).
- **Planning context awareness** (re‑queries when the scenario or planning horizon changes).

The query service remains a pure data access layer. The Store adds orchestration and caching semantics appropriate for the UI. Neither layer contains business logic—that remains in the domain.

## 6.8 Multiple Projections per Bounded Context  

A single bounded context may maintain several independent read models, each served by its own projection agent. For example, Demand Intelligence may have:

| Read Model | Agent | Underlying Events | Typical Queries |
|------------|-------|-------------------|-----------------|
| Demand Line Read Model | `DemandAgent` | `DemandLineEvent` | Get by ID, customer, status, date range |
| Forecast Read Model | `ForecastAgent` | `ForecastEvent` | Get forecast by product, location, horizon |
| Demand Quality Read Model | `QualityAgent` | `DemandLineEvent` + `ForecastEvent` | Accuracy metrics, bias trends, override analysis |

Each agent is independent. They can be rebuilt, snapshotted, and queried independently. They subscribe to the same event streams but maintain different slices of the data, optimised for their specific queries.

## 6.9 Projection Agent Lifecycle  

```
Application Startup
        │
        ├──► Create ProjectionAgent with evolve function and initial state
        │
        ├──► Load checkpoint from checkpoints table
        │
        ├──► Replay events from checkpoint position to current
        │        │
        │        └──► For each event: agent.Post(event, eventId)
        │
        ├──► Subscribe to live events via DomainEventBus
        │        │
        │        └──► On each new event: agent.Post(event, eventId)
        │
        └──► Projection agent processes messages sequentially:
                 │
                 ├── Apply(event) → evolve state, update stats
                 ├── Query(fn)   → return fn(state)
                 └── GetState    → return state
```

---

# Chapter 7 — Shared Libraries  

## 7.1 Overview  

Three shared libraries provide the foundation for every bounded context in Medhavi. They contain no business logic—only the cross‑cutting types, contracts, and pure functions that every component depends on.

| Library | Purpose | Depends On |
|---------|---------|------------|
| `Medhavi.SharedKernel` | Base types, execution context, logging, telemetry, metrics, health checks, resilience primitives | `Medhavi.Common` |
| `Medhavi.DecisionCore` | Pure decision semantics: scoring, feasibility, reservations, fingerprints, policy gate, autonomy contracts, planning graph | None |
| `Medhavi.Contracts` | DTOs, request/response types, integration event schemas, API contracts | None (plain F# records) |

These libraries are referenced by every bounded context. They ensure consistency across the entire system without introducing coupling between domains.

## 7.2 Medhavi.SharedKernel  

`SharedKernel` provides the operational backbone for all components. It is not a dumping ground for utility functions—every type it contains is used across multiple bounded contexts and represents a genuine cross‑cutting concern.

> **Configuration submodule.** `SharedKernel` also contains the `Configuration` submodule, which holds compile‑time ARS identifier constants, the `FeatureFlags` record type, and the `AppSettings` types with validation. These are used by every bounded context and have no additional dependencies, so they reside in `SharedKernel` rather than a separate project for MVP simplicity.

### 7.2.1 Execution Context  

`ExecutionContext` is the carrier of distributed tracing information. It flows from the initial API request through every command, aggregate decision, event, and telemetry record.

```fsharp
type ExecutionContext = {
    CorrelationId: Guid
    CausationId: Guid option
    Principal: string option
    Timestamp: DateTimeOffset
    TenantId: string option
    MessageId: string option
}
```

Key functions:

- `create()` — generates a fresh `CorrelationId`.
- `asCausation(ctx)` — creates a child context with `CausationId` set to the parent’s `CorrelationId`. Used by sagas and process managers.
- `fromEnvelope(env)` — reconstructs an `ExecutionContext` from event metadata, enabling traceability across service boundaries.
- `toMetadataMap(ctx)` — serialises the context into a `Map<string, string>` for inclusion in event envelope metadata.

The context is propagated implicitly via `AsyncLocal<ExecutionContext>` so that every function in the call chain can access it without explicit parameter passing, or explicitly for pure functions that require testability.

### 7.2.2 Logging  

The logging module provides structured, correlation‑aware logging built on `Microsoft.Extensions.Logging.ILogger`.

```fsharp
type LogContext = {
    CorrelationId: Guid option
    Operation: string option
    Component: string
    EntityId: string option
    EntityType: string option
    StreamName: string option
    EventId: Guid option
    EventType: string option
    Duration: TimeSpan option
    AdditionalData: Map<string, obj> option
}
```

`Logger` wraps an `ILogger` with convenience methods (`Info`, `Debug`, `Warning`, `Error`, `Critical`) that automatically merge the ambient `LogContext`. `MailboxLogger` provides asynchronous batching for high‑throughput scenarios, using the same `MailboxProcessor` pattern as the rest of the system.

The `ComponentNaming` module enforces a hierarchical naming convention:

```fsharp
// Produces: "Actor.Aggregate.DemandLine"
ComponentNaming.Actor.aggregate "DemandLine"

// Produces: "Integration.Publisher.OrderEvents"
ComponentNaming.Integration.publisher "OrderEvents"
```

Every log entry carries a structured component name, enabling precise filtering in observability tools.

### 7.2.3 Telemetry & Metrics  

The telemetry module provides structured event emission for observability:

```fsharp
type TelemetryEvent = {
    EventId: Guid
    Timestamp: DateTimeOffset
    Severity: TelemetrySeverity
    Message: string
    Properties: Map<string, obj>
    CorrelationId: Guid option
    CausationId: Guid option
    TraceId: string option
    SpanId: string option
}
```

The `Metrics` module provides counters, gauges, and histograms:

```fsharp
type MetricPoint = {
    MetricName: string
    MetricType: MetricType  // Counter | Gauge | Histogram | Summary
    Value: float
    Timestamp: DateTimeOffset
    Tags: Map<string, string>
    Unit: string option
}
```

Domain‑specific telemetry types (`PlanningKpis`, `LimiterFrequency`, `LatencyTelemetry`, `TelemetryErrorMetric`) are defined in `SharedKernel` so they can be produced by any bounded context and consumed by Knowledge Intelligence. They carry `CorrelationId` and `TenantId` for traceability.

### 7.2.4 Health Checks  

The `HealthCheck` module provides a standard pattern for component health reporting:

```fsharp
type HealthStatus = Healthy | Degraded of string | Unhealthy of string

type ComponentHealth = {
    ComponentName: string
    Status: HealthStatus
    LastChecked: DateTimeOffset
    ResponseTime: TimeSpan option
    Details: Map<string, obj>
}
```

Every bounded context registers health checks that are aggregated by Nexus and exposed via the ASP.NET Core health check endpoint.

### 7.2.5 Distributed Tracing  

`ActivityTracking` wraps `System.Diagnostics.Activity` for OpenTelemetry‑compatible distributed tracing:

```fsharp
let withActivity (logger: LogTelemetryEvent) (activityName: string) (tags: (string * string) list) (operation: unit -> 'T) : 'T =
    let activity = new Activity(activityName)
    for (key, value) in tags do activity.SetTag(key, value) |> ignore
    activity.Start()
    try
        let result = operation ()
        activity.Stop()
        // Emit telemetry event with TraceId and SpanId
        result
    with ex ->
        activity.SetTag("error", "true") |> ignore
        activity.Stop()
        reraise()
```

### 7.2.6 Exception Handling & Error Taxonomy  

The error handling module defines the canonical error types used across the entire system:

```fsharp
type DomainError =
    | ValidationError of code: string * message: string * data: Map<string, obj>
    | DomainError of code: string * message: string * data: Map<string, obj>

type InfrastructureError =
    | Network of string | Timeout of string | EventStore of string 
    | Database of string | Http of string | CircuitOpen of string | OtherInfra of string

type ApplicationError =
    | Domain of DomainError
    | NotFound of code: string * message: string * data: Map<string, obj>
    | Mismatch of code: string * expected: Version * actual: Version
    | Infrastructure of InfraError
    | External of code: string * message: string * data: Map<string, obj>
    | Unknown of string
```

All bounded contexts use these types. Domain logic returns `Result<'T, DomainError>`. Application services catch infrastructure exceptions and map them to `ApplicationError`. The API layer maps `ApplicationError` to HTTP status codes.

The `ApplicationError.fromException` function maps standard .NET exceptions to their appropriate `ApplicationError` variants, ensuring consistent error handling across all contexts.

### 7.2.7 Domain Event Bus (In‑Process)  

For the MVP monolith, an in‑process event bus using F# `Event<T>` provides lightweight publish/subscribe:

```fsharp
type DomainEventBus =
    static member Publish(evt: obj) = eventObj.Trigger(evt)
    static member Subscribe<'T>(handler: 'T -> unit) : IDisposable = ...
```

This will be replaced by a PostgreSQL‑backed bus in production (Chapter 4), but the interface remains the same.

## 7.3 Medhavi.DecisionCore  

`DecisionCore` is a **pure F# library** with no dependencies. It contains the shared decision semantics that must be identical across all bounded contexts. It is the technical realisation of the Decision Model and Rule & Policy Model from the Medhavi architecture.

Every function in DecisionCore is deterministic, side‑effect‑free, and independently testable.

### 7.3.1 Scoring  

Shared score model for plan evaluation:

```fsharp
type PlanScore = {
    TotalCost: decimal
    ServiceLevel: float
    CapacityUtilization: float
    LatenessPenalty: decimal
    RiskScore: float
}

type ScoreWeights = {
    CostWeight: float
    ServiceWeight: float
    CapacityWeight: float
    RiskWeight: float
}

type PlanScoreCard = {
    VariantId: string
    Score: PlanScore
    WeightedTotal: float
    Rank: int
}
```

Functions: `emptyScore`, `combineScores`, `weightedObjectiveScore`, `candidateRanking`, `cardComparison`.

### 7.3.2 Feasibility  

Pure feasibility contracts used by Promise (ATP/CTP) and Supply (plan validation):

```fsharp
type FeasibilityInput = {
    DemandQty: decimal
    AvailableSupply: SupplySnapshot
    ActiveReservations: Reservation list
    TimeWindow: TimeWindow
}

type FeasibilityResult =
    | Feasible of earliestDate: DateTimeOffset * confidence: float
    | PartiallyFeasible of quantity: decimal * date: DateTimeOffset
    | Infeasible of reason: Limiter list
```

Functions: `checkATP`, `checkCTP`, `composeFeasibility`, `determineAcceptability`.

### 7.3.3 Reservations  

Shared reservation semantics for Promise and PlanningEngine:

```fsharp
type ReservationScope = | Atp | Ctp | Allocation | Planned

type ReservationStatus = | Tentative | Confirmed | Released | Expired

type Reservation = {
    ReservationId: Guid
    Scope: ReservationScope
    Status: ReservationStatus
    SkuId: string
    Quantity: decimal
    Source: string
    CreatedAt: DateTimeOffset
    ExpiresAt: DateTimeOffset option
}
```

Functions: `createTentative`, `confirm`, `release`, `expire`, `reduce`, `validateLifecycle`.

### 7.3.4 Fingerprints  

Content‑addressed identifiers for planning artifacts:

```fsharp
type SnapshotFingerprint = SnapshotFingerprint of string
type PolicyFingerprint = PolicyFingerprint of string
type PlanFingerprint = PlanFingerprint of string
type GraphFingerprint = GraphFingerprint of string
```

Fingerprints are deterministically generated from the content they identify. They are used by Knowledge Intelligence to correlate artifacts across domains and by the event store for deduplication.

### 7.3.5 Policy Gate  

The `PolicyGate` is a pure validation function that ensures no policy change violates safety boundaries:

```fsharp
type PolicyGateResult =
    | Valid
    | ValidWithWarnings of string list
    | Rejected of string list

let validatePolicy (current: PlanningPolicySet) (proposed: PlanningPolicySet) : PolicyGateResult =
    // Checks: max solver time, min/max safety stock, frozen horizon, 
    // firm order protection, hard constraint preservation, max weight shift, ...
```

Every policy change—whether from a human planner or an AI recommendation—must pass through the `PolicyGate` before taking effect.

### 7.3.6 Autonomy Contracts  

Formal contracts that define what an AI agent is permitted to do:

```fsharp
type AutonomyLevel = | Advisory | Guardrailed | Autonomous

type AutonomyContract = {
    Level: AutonomyLevel
    AllowedActions: string list
    MaxPolicyDelta: float
    RollbackRules: string
    ApprovalRequired: bool
}
```

Functions: `validateAction(contract, action) -> Result<unit, string>`, `isWithinBoundary(contract, proposedChange) -> bool`.

### 7.3.7 AI Contracts  

AI‑facing contract shapes (not ML implementations):

```fsharp
type FeatureVector = Map<string, float>

type PolicyRecommendation = {
    PolicySet: PlanningPolicySet
    Confidence: float
    Reasoning: string
}

type ModeRecommendation = {
    RecommendedMode: PlanningMode
    Confidence: float
    Rationale: string
}
```

These contracts define the shape of data exchanged between AI models and the Planning Engine. The AI models themselves are external; DecisionCore defines only the contracts.

### 7.3.8 Integration Example  

Promise Orders calls DecisionCore for ATP feasibility:

```fsharp
// In Medhavi.Promise, DE‑PI‑020 Evaluate ATP:
let evaluateATP (cmd: EvaluateATPCmd) (supplySnapshot: SupplySnapshot) : Result<ATPResult, DomainError> =
    let input = {
        FeasibilityInput.DemandQty = cmd.Quantity
        AvailableSupply = supplySnapshot
        ActiveReservations = currentReservations
        TimeWindow = cmd.RequestedWindow
    }
    DecisionCore.Feasibility.checkATP input
    |> Result.map toATPResult
    |> Result.mapError (fun f -> DomainError.validation f.Message)
```

This is the only place where feasibility logic lives. Both Promise and Supply use the same function, guaranteeing consistent behaviour.

## 7.4 Medhavi.Contracts  

`Medhavi.Contracts` is a lightweight library of **plain F# records and discriminated unions** used for communication between bounded contexts and between the backend and the UI.

It contains:

- **DTOs**: `DemandLineDto`, `ForecastDto`, `InventorySnapshot`, `SupplyPlanDto`, `PromiseDto`, `ScenarioDto`, `KnowledgeInsightDto`, etc.
- **Request/Response types**: `DemandDefineReq`, `ForecastOverrideReq`, `PromiseRequest`, `ApiError`, etc.
- **Integration event schemas**: `DemandCreatedNotification`, `SupplyPlanPublishedNotification`, etc.
- **Enumerations**: `DemandLineStatus`, `ForecastConfidence`, `PromiseType`, `ScenarioType`, etc.

`Contracts` has **no domain dependencies**. It references only .NET base types. This ensures it can be shared freely without creating coupling between bounded contexts.

The mapping between domain types and contract types is performed by the ACL in each bounded context’s Application layer. The domain never references `Contracts`; `Contracts` never references the domain.

---

# Chapter 8 — Integration Patterns  

## 8.1 Internal Integration  

Bounded contexts within Medhavi communicate exclusively through events. One context publishes an event; zero or more other contexts subscribe to that event and react accordingly. There is no direct database access across bounded contexts, no shared mutable state, and no synchronous RPC between contexts for business operations.

### 8.1.1 DomainEventBus (MVP)  

In the MVP, all contexts run in the same process within `Medhavi.Nexus`. Integration is handled by the in‑process `DomainEventBus`:

```fsharp
// Publisher (e.g., Demand context)
DomainEventBus.Publish(envelope)

// Subscriber (e.g., Supply context, in its composition root)
let subscription = DomainEventBus.Subscribe<ForecastPublished>(fun env ->
    // Handle the event: update projection, trigger planning cycle, etc.
)
```

Because everything runs in one process, event delivery is synchronous and instantaneous. There is no network overhead, no serialisation cost beyond what is already stored in the event store, and no risk of message loss.

### 8.1.2 Evolution to PostgreSQL‑Backed Bus  

When contexts are extracted into separate services, the in‑process bus is replaced by a PostgreSQL `LISTEN`/`NOTIFY` implementation. The subscription interface remains identical; only the implementation changes:

```
┌──────────────┐     append event      ┌──────────────┐
│  Publisher   │ ────────────────────► │  PostgreSQL   │
│  Service     │                       │  events table │
└──────────────┘                       └───────┬───────┘
                                               │ NOTIFY
                                               ▼
┌──────────────┐     read + checkpoint ┌──────────────┐
│  Subscriber  │ ◄──────────────────── │  PostgreSQL   │
│  Service     │                       │  events table │
└──────────────┘                       └──────────────┘
```

The same `EnvelopeStoreOps.Subscribe` function handles both modes. Bounded contexts never know which implementation is active.

### 8.1.3 Cross‑Context Event Routing  

The authoritative routing map is defined in Chapter 4, Section 4.7. Every event published by one domain and consumed by another is explicitly listed. No implicit dependencies are allowed.

A consumer that needs data from another domain **subscribes to the appropriate events** and builds its own local projection. For example, Promise Intelligence needs supply availability for ATP evaluation. It does not query Supply’s database—it subscribes to `SupplyPlanPublished` and `InventoryPositionUpdated` and maintains its own `SupplyAvailabilityReadModel`.

This ensures:

- **No runtime coupling.** The publisher has no knowledge of its consumers.
- **Independent deployability.** A consumer can be down without affecting the publisher.
- **Resilience.** If a consumer misses events, it replays from its checkpoint.

## 8.2 External Integration  

External systems—ERP, WMS, MES, IoT platforms, supplier portals—do not speak Medhavi’s domain language. The `Medhavi.Integration` bounded context is the gatekeeper between the outside world and the Medhavi domain.

### 8.2.1 Anti‑Corruption Layer (ACL)  

The ACL is a set of pure translation functions that convert external data formats into well‑formed domain commands. It enforces the architectural principle that **external data must be validated before it enters the domain**.

```fsharp
module ACL =
    let toIngestCommand (req: DemandDefineReq) : Validation<IngestDemandLineCmd, DomainError> =
        make
        <!> (SkuId.create req.SkuId |> fromResult)
        <*> (StockingPointId.create req.StockingPointId |> fromResult)
        <*> (Quantity.create req.Quantity |> fromResult)
```

The ACL performs three responsibilities:

1. **Type safety.** External strings and decimals become domain value objects (`SkuId`, `Quantity`). Invalid inputs produce validation errors, not runtime exceptions.
2. **Semantic mapping.** External status codes and categories are mapped to domain enumerations.
3. **Data quality.** Missing fields, out‑of‑range values, and malformed dates are rejected before they reach the domain.

The ACL is a pure function. It can be tested exhaustively without any infrastructure.

### 8.2.2 Ingestion Pipeline  

External events flow through a standard pipeline:

```
External System (ERP, WMS, MES)
        │
        ▼
Integration Adapter (HTTP endpoint, file watcher, message queue listener)
        │
        ▼
ACL: External DTO → Domain Command (with validation)
        │
        ├── Validation Error → Rejection event published, logged, returned to caller
        │
        └── Valid Command → Published to DomainEventBus → Domain aggregate handles it
```

Integration adapters are **thin**. They deserialise the incoming payload, call the ACL, and route the result. They contain no business logic.

### 8.2.3 Integration Context Structure  

```
Medhavi.Integration/
├── Adapters/
│   ├── ErpAdapter.fs           # SAP / Oracle integration
│   ├── WmsAdapter.fs           # Warehouse system integration
│   ├── MesAdapter.fs           # Shop‑floor feedback
│   └── SupplierPortalAdapter.fs
├── ACL/
│   ├── DemandAcl.fs            # Maps external order formats to IngestDemandLineCmd
│   ├── SupplyAcl.fs            # Maps external inventory updates
│   └── MasterDataAcl.fs        # Maps external product, BOM, supplier data
└── IntegrationContext.fs       # Composition root, registers adapters and ACLs
```

## 8.3 Saga / Process Manager for Long‑Running Workflows  

Some business processes span multiple bounded contexts and cannot be completed in a single command. For example, the **Order Promising** workflow involves:

1. **Demand** — Order is accepted and validated.  
2. **Supply** — Inventory is checked and temporarily reserved.  
3. **Promise** — A promise is made (or rejected).  
4. **Supply** — The reservation is confirmed or released.  

This is a **long‑running business transaction**. It cannot be a single ACID database transaction because it spans aggregates, and eventually spans services. Instead, it is managed by a **Process Manager** (a saga).

### 8.3.1 Process Manager Pattern  

A Process Manager:

- **Listens to events** from multiple domains.
- **Maintains state** representing where the workflow is.
- **Dispatches commands** to advance the workflow.
- **Handles failures** with compensating actions.

In Medhavi, a Process Manager is implemented as a state machine driven by events, using the same event‑sourcing and projection patterns as aggregates.

```fsharp
type OrderPromisingState =
    | Initial
    | DemandValidated
    | SupplyReserved of reservationId: Guid
    | PromiseConfirmed of promiseId: string
    | PromiseRejected of reason: string
    | Completed

type OrderPromisingEvent =
    | SagaStarted of orderId: string
    | DemandValidationRequested of orderId: string
    | SupplyReservationRequested of orderId: string * quantity: decimal
    | PromiseRequested of orderId: string
    | SagaCompleted of orderId: string * promiseId: string option
    | SagaFailed of orderId: string * reason: string
```

The Process Manager subscribes to domain events and, based on its current state, decides what command to send next.

### 8.3.2 Causation and Traceability  

Every command dispatched by a Process Manager carries the `CausationId` of the event that triggered it. This preserves the full causal chain:

```
OrderReceived (event)
  └─► SagaStarted (event, CausationId = OrderReceived.MessageId)
        └─► DemandValidationRequested (command, CausationId = SagaStarted.MessageId)
              └─► DemandValidated (event)
                    └─► SupplyReservationRequested (command, CausationId = DemandValidated.MessageId)
                          ...
```

This chain is visible in every event’s envelope metadata and is queryable for audit and debugging.

### 8.3.3 Compensation  

If a step in the saga fails, the Process Manager must **compensate** for any steps that already succeeded. Compensation actions are themselves commands dispatched by the saga:

| Step | Compensating Action |
|------|---------------------|
| Demand Validated | No action needed (validation is idempotent) |
| Supply Reserved | `ReleaseReservation` command |
| Promise Confirmed | `RevokePromise` command |

Compensation logic is part of the saga’s state machine. It is tested independently.

### 8.3.4 Where Sagas Live  

Sagas that coordinate a single dominant workflow (e.g., Order Promising) live **inside the bounded context that owns the workflow**. The Order Promising saga belongs to `Medhavi.Promise`, because Promise is the domain responsible for the promise outcome.

Sagas that span domains without a clear single owner (e.g., a cross‑domain improvement rollout orchestrated by Knowledge Intelligence) live in a small `Medhavi.Sagas` project or within `Medhavi.Nexus`.

### 8.3.5 Saga Implementation  

A saga is implemented as an **event‑sourced aggregate** with its own event stream. This means:

- The saga’s state is reconstructed from its events on startup.
- The saga can be snapshotted for performance.
- The saga’s decisions are pure functions: `State → Event → Command list`.
- The saga emits its own events (`SagaStepCompleted`, `SagaFailed`) for observability.

Alternatively, a lightweight saga can use a **projection agent** if its state is simple and does not need to survive restarts independently of the events it processes. The choice depends on the complexity of the workflow.

## 8.4 External Event Publishing  

Not all events are consumed only within Medhavi. Some must be sent to external systems:

- **Production schedules** → MES.  
- **Purchase orders** → Supplier portals.  
- **Promise confirmations** → Customer‑facing systems.  
- **Quality reports** → Data warehouse or analytics platform.  

External publishing is handled by **integration adapters** within `Medhavi.Integration`. An adapter subscribes to the relevant domain events, transforms them into the external system’s format, and transmits them via the appropriate protocol (REST, file, message queue).

```fsharp
// Example: Publish production schedule to MES
DomainEventBus.Subscribe<ProductionSchedulePublished>(fun env ->
    let mesFormat = MesAdapter.toMesFormat env
    MesClient.Publish(mesFormat)
)
```

The adapter is responsible for retry logic, circuit breaking (using the shared `CircuitBreaker`), and dead‑letter queuing. If the external system is unavailable, the adapter retries with exponential backoff and eventually places the message in a dead‑letter queue for manual intervention.

## 8.5 Integration Testing  

Integration between bounded contexts is verified through:

- **Contract tests** — Event schemas in `Medhavi.Contracts` are validated against both publisher and subscriber.
- **Scenario tests** — A command is sent to the entry point; the resulting events across all affected contexts are verified.
- **Saga tests** — The saga state machine is tested with sequences of events and the resulting commands are asserted.

All integration tests use the in‑memory event store and event bus, so they run in milliseconds without external dependencies.

---

# Chapter 9 — Planning Engine  

## 9.1 Position and Purpose  

The Planning Engine is a **computational service**, not a bounded context. It does not own business invariants, master data, or the source of truth for any plan. It is the engine that generates plans—material plans, production schedules, replenishment orders, optimised scenarios—on behalf of the Intelligence Capabilities that own the planning process.

The Planning Engine lives inside `Medhavi.Nexus` (in the MVP) and is called by:

- **Supply Intelligence — Plan Supply** to generate constrained supply plans.  
- **Supply Intelligence — Manage Inventory** to calculate replenishment quantities.  
- **Supply Intelligence — Manage Capacity** to evaluate capacity feasibility.  
- **Supply Intelligence — Schedule Production** to sequence production orders.  
- **Scenario Intelligence — Simulate Scenarios** to run what‑if plan variants.  

Every planning run is initiated by an Intelligence Capability. The Planning Engine executes the requested computation and returns results. It never decides *whether* to plan—that decision belongs to the domain.

## 9.2 Planning Modes  

The Planning Engine supports five planning modes, each optimised for a specific situation.

| Mode | Trigger | Scope | Typical Latency |
|------|---------|-------|-----------------|
| **FastInsert** | Single new order or small change | One product, one location | Milliseconds |
| **IncrementalRepair** | Local disruption (supplier delay, machine breakdown) | Affected products and their dependencies | Seconds |
| **FullReplan** | Major baseline drift, periodic cycle | All products and locations in scope | Minutes to hours |
| **Optimization** | Strategic objective improvement, cost reduction | Full plan with relaxed constraints | Minutes to hours |
| **WhatIf** | Scenario evaluation, plan variant comparison | Defined by scenario overlay | Minutes |

The mode is selected by the calling capability, not by the Planning Engine itself. The `PlanningModeDispatcher` in Supply’s application layer determines the appropriate mode based on trigger type, impact classification, and policy.

### 9.2.1 FastInsert  

FastInsert evaluates whether a single new demand can be satisfied without disrupting the existing plan. It:

1. Loads the current supply‑demand balance for the requested product‑location.
2. Checks ATP (Available‑to‑Promise) against uncommitted inventory and planned supply.
3. If feasible, reserves the supply and returns a promise date.
4. If infeasible, returns the earliest feasible date.

FastInsert does not run a full MRP cycle. It operates on the existing plan’s indexed state. It is used by the Promise domain for real‑time order promising.

### 9.2.2 IncrementalRepair  

IncrementalRepair handles a local disruption—a supplier delay, a machine breakdown, a quality hold—by:

1. Identifying the affected products and their dependent demand (using the BOM and pegging data).
2. Re‑planning only those products, freezing the rest of the plan.
3. Minimising churn by preferring replan options that deviate least from the published plan.
4. Generating new supply orders and adjusting promise dates only where necessary.

IncrementalRepair uses the `PlanIndex` to efficiently identify impacted orders without scanning the entire plan.

### 9.2.3 FullReplan  

FullReplan is the standard planning cycle. It runs the complete MRP pipeline for all products and locations in the planning scope. It is scheduled periodically (daily or weekly) and on detection of major plan drift.

#### 9.2.4 Optimization  

Optimization is FullReplan with an **optimisation objective**—cost minimisation, service level maximisation, capacity utilisation balancing, or a weighted combination. It uses a mathematical solver (e.g., Google OR‑Tools CP‑SAT, or an LP solver) to find the best plan subject to constraints.

Optimization runs are typically longer than FullReplan. The solver can be configured with time limits and quality bounds.

### 9.2.5 WhatIf  

WhatIf mode is identical to FullReplan or Optimization, but operates on a **scenario overlay** rather than the baseline plan. The overlay is provided by Scenario Intelligence (`ScenarioOverlay`, equivalent to `SE‑SN‑014 Scenario Assumption`). WhatIf runs do not publish their results as the authoritative plan; they are used for comparison and recommendation.

## 9.3 MRP Pipeline  

The core of the Planning Engine is the **Material Requirements Planning (MRP) pipeline**. It is used by FullReplan, Optimization, and WhatIf modes.

```
preprocess → forecast consumption → BOM explosion → netting → supply generation → capacity check → pegging → postprocess
```

Each step is a pure function followed by a thin application adapter. The pipeline is identical whether it operates on the baseline plan or a scenario overlay.

### 9.3.1 Preprocess  

Load and validate all inputs:

- Demand forecast (from Demand Intelligence)  
- Current inventory position and open supply orders (from Understand Supply)  
- BOM and routings (from MasterData)  
- Resource calendars and capacity (from Manage Capacity)  
- Planning parameters (frozen horizon, safety stock targets, lot sizes, lead times)  
- If a scenario overlay is provided, apply it to the baseline data.

Validation checks: data freshness, completeness, horizon alignment. Failures are reported to the caller; the plan run is not started if critical inputs are missing.

### 9.3.2 Forecast Consumption  

Combine the demand forecast with actual orders. Firm customer orders consume the forecast within the demand time fence. Beyond the fence, the forecast is used directly. The result is a single, time‑phased demand quantity per product‑location.

### 9.3.3 BOM Explosion  

For each finished product, explode the Bill of Materials to determine gross requirements for all components and raw materials. Multi‑level BOMs are expanded recursively. The output is a set of gross requirements at every level, time‑phased by the lead‑time offset from the parent order.

### 9.3.4 Netting  

For each product at each location and time bucket:

```
Projected Inventory = Prior Inventory + Scheduled Receipts − Gross Requirements
```

If projected inventory falls below the safety stock level, a **net requirement** is generated. The net requirement quantity is the amount needed to bring projected inventory back to the target stock level.

Netting is performed level‑by‑level, from finished goods down to raw materials. Lower‑level net requirements feed into the gross requirements of their components.

### 9.3.5 Supply Generation  

For each net requirement, generate a **planned supply order** (planned production order, planned purchase order, or planned transfer). The order quantity respects lot‑sizing rules (lot‑for‑lot, EOQ, minimum order quantity, period‑order‑quantity). The order release date is calculated by offsetting the due date by the item’s lead time.

Supply generation may combine multiple net requirements into a single order if they fall within the same lot‑sizing window.

### 9.3.6 Capacity Check  

After supply orders are generated, the Planning Engine evaluates whether the required capacity is available. For each resource and time bucket, the total load from planned production orders is compared against available capacity. Overloads are flagged as constraint violations.

In FullReplan mode, capacity violations are reported but do not block plan publication. In Optimization mode, the solver is instructed to resolve capacity violations by shifting orders or adding capacity (overtime, outsourcing).

### 9.3.7 Pegging  

Pegging traces the source of each supply order back to the demand that created it. It creates a graph of dependencies:

```
Customer Order → Finished Good Supply Order → Component Supply Order → Raw Material Purchase Order
```

Pegging is stored as part of the plan result. It is used by IncrementalRepair to identify what must be replanned when a disruption occurs, and by the Explainability builder to generate decision narratives.

### 9.3.8 Postprocess  

Generate the final plan outputs:

- Time‑phased supply plan (planned production, procurement, transfers).  
- Projected inventory positions.  
- Capacity load profile.  
- Constraint and bottleneck report.  
- Pegging graph.  
- Plan scorecard (using DecisionCore `Scoring`).

The outputs are returned to the calling capability, which owns the decision to publish, revise, or reject the plan.

## 9.4 Replenishment  

Replenishment is a specialised planning mode that operates on **inventory thresholds** rather than a full MRP cycle. It is used by Supply Intelligence — Manage Inventory for continuous‑review items.

```fsharp
type ReplenishmentTrigger = {
    SkuId: string
    StockingPointId: string
    CurrentPosition: decimal
    ReorderPoint: decimal
    TargetStock: decimal
    LotSize: decimal
}
```

When the inventory position drops to or below the reorder point, a replenishment trigger is generated. The Planning Engine:

1. Validates the trigger (cooldown suppression—no duplicate triggers within a configurable window).  
2. Calculates the order quantity (up to target stock, respecting lot size).  
3. Generates a planned purchase order or production order.  
4. Publishes a `ReplenishmentRecommended` event.  

The trigger is converted into a command by the Manage Inventory capability. The Planning Engine does not decide to replenish; it executes the replenishment calculation when asked.

## 9.5 Solver Integration (Optimization Mode)  

Optimization mode uses a mathematical solver to find the best plan. The Planning Engine is **solver‑agnostic**. It defines a contract that any solver can implement:

```fsharp
type OptimizationModel = {
    Variables: Variable list
    Constraints: Constraint list
    Objective: Objective
}

type SolverPort = {
    Solve: OptimizationModel -> TimeSpan option -> Task<Result<SolverSolution, SolverError>>
}
```

The default implementation uses Google OR‑Tools (CP‑SAT for scheduling, Glop for linear programming). The solver is a **stateless, pure computational service**. It receives a model, returns a solution. It has no domain knowledge, no persistence, no side effects.

The model is built by the Planning Engine from the MRP data and the optimisation objectives. The solver runs within a configurable time limit. If no optimal solution is found within the limit, the best feasible solution found so far is returned.

## 9.6 Scenario What‑If Execution  

Scenario Intelligence calls the Planning Engine in **WhatIf mode** to evaluate plan variants. The flow:

1. **Scenario Intelligence — Simulate Scenarios** prepares a `ScenarioOverlay`—a set of assumption overrides (demand ±X%, capacity changes, inventory adjustments, policy parameter changes).  
2. The overlay is applied to the baseline plan data during the Preprocess step.  
3. The Planning Engine runs the MRP pipeline (or Optimization) against the overlaid data.  
4. The resulting plan variant is returned to Scenario Intelligence for comparison, scoring, and recommendation.  

WhatIf runs are **isolated**. They do not publish their results as the authoritative plan. They do not consume real supply reservations. The `ScenarioVariantRunner` in the Planning Engine ensures that what‑if runs operate on a copy of the plan state, not the live plan.

## 9.7 Integration with DecisionCore  

The Planning Engine delegates to `DecisionCore` for all shared decision semantics. It does not implement its own scoring, feasibility, or reservation logic.

| Planning Engine Function | DecisionCore Module | Purpose |
|--------------------------|---------------------|---------|
| Evaluating plan alternatives | `Scoring` | Compute `PlanScoreCard`, rank variants |
| Checking ATP/CTP feasibility | `Feasibility` | Determine if demand can be satisfied |
| Reserving supply during planning | `Reservations` | Create tentative reservations, confirm/release |
| Validating plan stability | `Fingerprints` | Content‑address plan versions for comparison |
| Validating policy changes | `PolicyGate` | Ensure AI or planner policy changes are safe |

## 9.8 Time‑Window Projections  

The Planning Engine produces time‑phased plans over configurable horizons:

| Horizon | Typical Duration | Bucket Size | Used By |
|---------|------------------|-------------|---------|
| Operational | 1–12 weeks | Day / Week | Promise (ATP), Production Scheduling |
| Tactical | 3–18 months | Week / Month | Supply Planning, Inventory Management |
| Strategic | 1–5 years | Month / Quarter | Scenario Intelligence, S&OP |

Time‑window math (overlap, containment, shifting, bucket alignment, lead‑time offsets) is provided by `DecisionCore.TimeWindows` and used consistently across all planning modes.

## 9.9 Explainability and Telemetry  

Every planning run produces structured explainability data and telemetry.

### 9.9.1 Decision Traces  

The Planning Engine builds `DecisionTrace` records for every significant choice made during the run:

- Why a particular supply order was generated (which demand triggered it, which rule applied).  
- Why a capacity violation exists (which orders overload which resource).  
- Why one plan variant was ranked higher than another (which criteria contributed to the score).  

These traces are produced by `DecisionCore.Explainability` and stored with the plan result. They are consumed by each domain’s Explain capability to generate human‑ and AI‑readable explanations.

### 9.9.2 Telemetry  

Every planning run emits `PlanningKpis` telemetry:

```fsharp
type PlanningKpis = {
    ScenarioId: string
    Timestamp: DateTimeOffset
    TotalCost: decimal
    LatenessPenalty: decimal
    CapacityUtilization: Map<string, float>
    ServiceLevel: float
    TenantId: string option
}
```

This is consumed by Knowledge Intelligence for cross‑domain pattern discovery and enterprise‑wide learning.

## 9.10 Software Realisation  

```
Planning Engine
├── Domain/
│   ├── PlanningMode.fs           # Mode discriminated union
│   ├── PlanningResult.fs         # Plan result, scorecard, decision traces
│   └── ReplenishmentTrigger.fs   # Replenishment trigger type
│
├── Pipeline/
│   ├── Preprocess.fs             # Data loading, validation, overlay application
│   ├── ForecastConsumption.fs    # Forecast vs. actual order consumption
│   ├── BomExplosion.fs           # Multi‑level BOM explosion
│   ├── Netting.fs                # Inventory netting and net requirements
│   ├── SupplyGeneration.fs       # Planned order creation
│   ├── CapacityCheck.fs          # Capacity load evaluation
│   ├── Pegging.fs                # Dependency graph generation
│   └── Postprocess.fs            # Output assembly
│
├── Modes/
│   ├── FastInsertRunner.fs
│   ├── IncrementalRepairRunner.fs
│   ├── FullReplanRunner.fs
│   ├── OptimizationRunner.fs     # Includes solver model builder
│   └── WhatIfRunner.fs
│
├── Replenishment/
│   └── ReplenishmentService.fs
│
├── Solvers/
│   └── OrToolsSolver.fs          # Google OR‑Tools integration
│
├── Index/
│   └── PlanIndex.fs              # Fast lookup for IncrementalRepair and FastInsert
│
└── PlanningEngine.fs             # Public API surface for callers
```

The Planning Engine is instantiated within `Medhavi.Nexus` and its public API is injected into the Application layers of Supply and Scenario. It has no direct dependency on any bounded context’s domain logic—only on the shared `DecisionCore` and `SharedKernel` libraries.

---

# Chapter 10 — AI Enablement & Copilot Architecture  

## 10.1 AI‑Ready by Design  

Medhavi is built from the ground up to support AI agents as first‑class participants in the planning process. This is not a future aspiration—it is embedded in the Constitution (C‑AI‑001), the Intelligence Specifications, and every layer of the architecture described in this Blueprint.

An AI‑ready system must satisfy four conditions:

| Condition | How Medhavi Satisfies It |
|-----------|---------------------------|
| **Explainability** | Every decision, event, and policy evaluation carries a traceable chain of ARS identifiers. AI agents can generate and consume explanations using the same `Explainability Template` as human planners. |
| **Governed Autonomy** | AI agents operate within explicit autonomy contracts validated by the `PolicyGate`. They can recommend, but cannot bypass hard constraints or locked orders without policy‑approved escalation. |
| **Shared Interfaces** | AI agents use the same commands, events, queries, and Stores as human users. There is no separate “AI path.” This ensures consistency and auditability. |
| **Knowledge Foundation** | The Knowledge Intelligence domain provides enterprise memory, validated patterns, root‑cause analyses, and best practices that AI agents can query at runtime to make context‑aware decisions. |

The architecture does not distinguish between “AI” and “human” as system actors. Both issue commands, consume events, read from Stores, and are subject to the same rules and policies. The only difference is that AI agents may operate at higher speed and scale, which is why autonomy contracts and guardrails exist.

## 10.2 Knowledge Intelligence as the AI Foundation  

The Knowledge Intelligence domain is the primary source of context for AI agents. Before making a recommendation, an AI agent can query Knowledge Intelligence to answer:

- **Has this situation occurred before?** → Enterprise memory, queried via `Memory Query` (SE‑KN‑083).  
- **What patterns are relevant to my current decision?** → Cross‑domain patterns (SE‑KN‑003), served via `Serve Knowledge to AI Agents`.  
- **What is the best practice for this scenario?** → Best‑practice catalogue (SE‑KN‑060), filtered by applicability conditions.  
- **What was the outcome of similar past decisions?** → Outcome records (SE‑KN‑081), with full provenance.  

This means an AI agent does not operate on raw data alone. It operates with the full accumulated wisdom of the enterprise, delivered in a structured, machine‑readable format with confidence scores and traceability.

```
AI Agent (Planning Copilot)
        │
        ▼
POST /api/v1/knowledge/agent/query
        │
        ▼
Serve Knowledge to AI Agents (Knowledge Intelligence 5.8)
        │
        ├── Query enterprise memory for similar past situations
        ├── Retrieve applicable best practices
        ├── Check for relevant cross‑domain patterns
        └── Assemble response with provenance and confidence
        │
        ▼
AI agent uses knowledge context to inform its recommendation
        │
        ▼
AI agent issues a Workspace Action (same as a human planner would)
```

## 10.3 AI Agent Interaction Model  

AI agents interact with the system through exactly the same interfaces as human users. There is no separate AI API for business operations.

### 10.3.1 Commands and Workspace Actions  

An AI agent that wants to recommend a forecast override does not call a special “AI Override” endpoint. It issues the same `OverrideForecastCmd` that a human planner would use. The command goes through the same ACL validation, the same domain decision function, the same rule and policy evaluation.

The AI agent is a client of the `Medhavi.Hub` API, just like the UI. In the Store pattern (Chapter 14), the AI agent can also emit Workspace Actions that the Elmish Workspace processes identically to user actions.

```fsharp
// AI agent emits the same Workspace Action as a human user
let aiRecommendation = 
    WorkspaceAction.OverrideForecast {
        ForecastId = "FC-2026-001"
        NewValue = 500m
        Justification = "AI recommendation: confirmed large one‑time order from Customer X (source: CRM integration, confidence 94%)"
    }
```

### 10.3.2 Reading State  

AI agents read current state from the same Stores and query services that the UI uses. They do not have direct database access. They do not bypass projections. They see exactly what a human planner would see, plus any additional knowledge context from Knowledge Intelligence.

### 10.3.3 Event Subscriptions  

AI agents can subscribe to domain events to react in real time, just as projections and other bounded contexts do. For example, an AI agent monitoring supply disruptions subscribes to `SupplyDisruptionDetected` and `PromiseBreached` events. When it detects a pattern, it can issue a command or escalate to a human planner.

## 10.4 Autonomy Contracts  

Not all AI actions are created equal. Some are safe to execute automatically; others require human approval. The **autonomy contract** defines the boundary.

An autonomy contract is defined in `DecisionCore.Autonomy` and is associated with an AI agent, a domain, and a set of permitted actions.

```fsharp
type AutonomyLevel = 
    | Advisory       // AI can only suggest; all actions require human approval
    | Guardrailed    // AI can act within defined boundaries; outside requires approval
    | Autonomous     // AI can act freely within its permitted actions

type AutonomyContract = {
    ContractId: string
    AgentId: string
    Level: AutonomyLevel
    Domain: string                          // e.g., "Demand", "Supply", "Promise"
    AllowedActions: string list             // e.g., ["OverrideForecast", "PublishForecast"]
    MaxPolicyDelta: float                   // Maximum allowed change to policy parameters
    MaxValueThreshold: decimal option       // Maximum financial impact per action
    RollbackRules: string                   // How to reverse an action if needed
    ApprovalRequiredAbove: decimal option   // Actions above this value require approval
    ExpiresAt: DateTimeOffset
}
```

Before an AI agent executes any command, the system validates:

1. Is the agent’s contract active and not expired?  
2. Is the command within the `AllowedActions` list?  
3. Does the financial impact stay within `MaxValueThreshold`?  
4. Does any policy change stay within `MaxPolicyDelta`?  
5. If the action exceeds `ApprovalRequiredAbove`, has human approval been granted?

This validation is performed by `DecisionCore.Autonomy.validateAction` and is called by the application service before the command reaches the domain.

```fsharp
let executeAiAction (contract: AutonomyContract) (command: Command) : Result<unit, string> =
    match contract.Level with
    | Advisory -> Error "Advisory agents cannot execute actions directly"
    | Guardrailed ->
        if not (List.contains command.Action contract.AllowedActions) then
            Error $"Action '{command.Action}' not permitted by contract {contract.ContractId}"
        elif command.EstimatedImpact > contract.MaxValueThreshold then
            Error "Action exceeds maximum value threshold"
        else
            Ok ()
    | Autonomous ->
        if List.contains command.Action contract.AllowedActions then
            Ok ()
        else
            Error $"Action '{command.Action}' not permitted by contract {contract.ContractId}"
```

## 10.5 PolicyGate for AI Recommendations  

When an AI agent recommends a **policy change**—adjusting a threshold, modifying safety stock parameters, changing planning mode selection rules—that recommendation must pass through the `PolicyGate`.

The `PolicyGate` (DecisionCore, Section 7.3.5) validates:

- **Hard constraint preservation**: the proposed policy must not violate absolute safety boundaries (minimum safety stock, frozen horizon protection, firm order protection).  
- **Maximum policy delta**: the proposed change must not exceed the allowable deviation from the current policy.  
- **Approval requirements**: risky changes are flagged for human approval.

AI agents cannot bypass the `PolicyGate`. Even an `Autonomous` agent with policy‑changing permissions must pass this gate. If the gate rejects the change, the AI’s recommendation is recorded but not applied. A human planner can review and override.

```fsharp
// AI agent proposes a policy change
let proposedPolicy = aiAgent.GeneratePolicyRecommendation(context)

// PolicyGate validates
match DecisionCore.PolicyGate.validatePolicy currentPolicy proposedPolicy with
| Valid -> applyPolicy proposedPolicy
| ValidWithWarnings warnings -> applyPolicy proposedPolicy; logWarnings warnings
| Rejected reasons -> 
    // Policy change is blocked; AI recommendation is recorded for review
    recordAiRecommendation (PolicyRecommendation proposedPolicy reasons)
    escalateToPlanner (PolicyRecommendation proposedPolicy reasons)
```

## 10.6 AI Explainability  

Every AI recommendation must be explainable. The AI agent populates the same `Explainability Template` that human decision traces use (defined in the Decision Model, Section 3.6).

An AI‑generated forecast override carries:

- **The recommendation itself**: “Override forecast for SKU123 to 500 units.”  
- **The context**: “CRM integration indicates confirmed large order from Customer X.”  
- **The confidence**: “94% confidence (source reliability: 98%, historical accuracy of CRM signals: 91%).”  
- **The traceability chain**: `DE‑DI‑023` → `BR‑DI‑027` (Justification Rule) → `BR‑DI‑028` (Deviation Rule) → `PO‑DI‑025` (Override Authorization Policy) → `CA‑DI‑002` (Forecast Demand).  
- **The ARS identifiers**: Every artifact referenced is traceable back to the Constitution.

This is the same structure used for human decisions. The AI agent simply populates it from its own reasoning context, which includes the knowledge retrieved from Knowledge Intelligence.

## 10.7 AI Agent Implementation Model  

AI agents are implemented as **external services** that interact with Medhavi through standard APIs and event subscriptions. They are not embedded in the Medhavi process.

```
┌─────────────────────────────────┐
│          AI Agent Service        │
│  (Python / F# / external)       │
│                                 │
│  ┌───────────────────────────┐  │
│  │   Model / Reasoning       │  │
│  │   (ML, rules, LLM)        │  │
│  └───────────┬───────────────┘  │
│              │                   │
│  ┌───────────▼───────────────┐  │
│  │   Medhavi Client Library  │  │
│  │   (API calls, event       │  │
│  │    subscriptions,          │  │
│  │    Store integration)      │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────┐
│         Medhavi.Hub             │
│  (REST API, event bus)          │
└─────────────────────────────────┘
```

The **Medhavi Client Library** is a thin wrapper (available in F# and potentially Python) that provides:

- Typed API clients for all Medhavi endpoints.  
- Event subscription helpers.  
- Knowledge Intelligence query builders.  
- Autonomy contract validation (so the agent can check its own permissions before making a call).

The client library ensures that AI agents use the same contracts, the same identifier formats, and the same error handling as any other client.

## 10.8 AI Agent Categories  

The architecture anticipates multiple categories of AI agent:

| Agent Category | Primary Domain | Typical Autonomy Level | Examples |
|----------------|----------------|------------------------|----------|
| **Planner Assistant** | Demand, Supply | Advisory → Guardrailed | Suggests forecast adjustments, flags exceptions, recommends replenishment actions |
| **Promise Optimizer** | Promise | Guardrailed | Evaluates ATP/CTP, proposes substitution options, auto‑promises within confidence thresholds |
| **Scenario Explorer** | Scenario | Advisory → Guardrailed | Generates scenario variants, runs comparisons, ranks alternatives |
| **Knowledge Curator** | Knowledge | Guardrailed → Autonomous | Discovers cross‑domain patterns, proposes best practices, maintains knowledge graph |
| **Conversational Copilot** | Cross‑domain | Advisory | Answers “what‑if” questions, explains plan changes, summarises performance via LLM |

Each category has different autonomy contracts, different permitted actions, and different value thresholds. The contracts are versioned and can be evolved as the AI’s reliability is demonstrated.

## 10.9 The Continuous Learning Loop  

AI agents participate in the enterprise learning loop orchestrated by Knowledge Intelligence:

1. **Telemetry collection** — Every AI action, whether executed or only recommended, is recorded as a `DecisionRecord` in the enterprise memory. The outcome (did the recommendation improve the plan?) is captured as an `OutcomeRecord`.  
2. **Pattern discovery** — Knowledge Intelligence analyses AI recommendations over time to identify patterns: which agents are most accurate, which recommendation types add the most value, where AI consistently outperforms or underperforms human planners.  
3. **Feedback** — Planners can accept or reject AI recommendations. Rejections carry a reason. This feedback is ingested by the AI agent’s training pipeline.  
4. **Autonomy evolution** — As an AI agent demonstrates consistently high accuracy, its autonomy contract can be evolved from Advisory to Guardrailed, and potentially to Autonomous for low‑risk actions. This evolution is governed by the `PolicyGate` and requires human approval.  

The loop is the same whether the actor is human or AI. The system learns from every decision, regardless of source.

## 10.10 LLM Integration  

Large Language Models (LLMs) are treated as a specialised category of AI agent. They do not mutate plan state. They operate at the Advisory autonomy level, providing:

- **Natural language explanations** — Translating structured `DecisionTrace` data into human‑readable summaries.  
- **Conversational what‑if** — Allowing planners to ask “What would happen if we increased safety stock by 10%?” and receiving a scenario summary.  
- **Plan summarisation** — Generating executive summaries of plan changes, key risks, and recommendations.  

LLMs interface through the same `Serve Knowledge to AI Agents` API. They receive structured knowledge context (patterns, best practices, precedents) and produce natural language output. The structured traceability chain remains the source of truth; the LLM output is an interpretation, not an authority.

## 10.11 Security and Governance for AI  

- **Authentication** — AI agents authenticate with the same OAuth 2.0 mechanism as human users. Each agent has its own identity and scoped permissions.  
- **Audit** — Every AI action is recorded in the immutable event store with the agent’s identity, its autonomy contract reference, and the full decision trace.  
- **Rate limiting** — AI agents are subject to rate limits to prevent runaway automation from overwhelming the system.  
- **Rollback** — Any AI action that causes a statistically significant degradation in plan quality triggers an automatic rollback recommendation (via `DecisionCore.Autonomy.rollbackRules`). The rollback itself is a command that must be approved or executed by a human planner, depending on the autonomy contract.  

## 10.12 Testing AI Behaviour  

AI behaviour is tested at multiple levels:

| Test Level | What is Verified |
|------------|------------------|
| **Unit tests** | `DecisionCore.Autonomy.validateAction` correctly accepts and rejects actions based on contract rules. `PolicyGate` correctly validates or rejects policy changes. |
| **Integration tests** | An AI agent with a Guardrailed contract can execute permitted actions and is blocked from disallowed actions. AI recommendations appear in the enterprise memory with correct provenance. |
| **Simulation tests** | An AI agent’s recommendations over a historical period are replayed and compared to actual outcomes to measure accuracy and value. |
| **Acceptance tests** | A conversational copilot correctly answers “what‑if” questions and generates accurate explanations from structured trace data. |

---

# Chapter 11 — End‑to‑End Traceability

## 11.1 Traceability as a First‑Class Architectural Concern

Traceability is not a logging add‑on or a debugging convenience. It is a Constitutional principle (C‑TR‑001) and an architectural requirement (ARS‑TR‑001) that every component must satisfy. The Architecture Reference Standard mandates:

- Every artifact has a permanent, unique, human‑readable identifier.
- Every decision traces back to the capability, semantic concept, and constitutional principle that authorised it.
- Every runtime violation must be traceable through the full chain: violation → functional specification → business rule → decision → capability → semantic model → constitution.

This chapter defines exactly how these requirements are realised in the running system—from compile‑time identifier constants to immutable event envelopes to AI‑readable explanation chains.

## 11.2 ARS Identifier Propagation in Code

Every artifact identifier defined in the Intelligence Specifications—capabilities (`CA‑DI‑002`), decisions (`DE‑DI‑020`), rules (`BR‑DI‑020`), policies (`PO‑DI‑023`), semantic objects (`SE‑DI‑003`), and performance indicators (`PI‑DI‑002`)—must be available at runtime for traceability.

### 11.2.1 Compile‑Time Identifier Registry

Identifiers are defined as F# string constants in a dedicated `Medhavi.Configuration` project. This ensures they are safe from typos, searchable, and can be validated at build time.

```fsharp
// Medhavi.Configuration/ArsIdentifiers.fs
module Medhavi.Configuration.ArsIdentifiers

module Demand =
    let domain = "DI"
    
    module Capabilities =
        let forecastDemand = "CA-DI-002"
        let senseDemand = "CA-DI-003"
        let evaluateDemandQuality = "CA-DI-007"
    
    module Decisions =
        let selectChampionModel = "DE-DI-020"
        let generateBaselineForecast = "DE-DI-021"
        let publishForecast = "DE-DI-022"
        let overrideForecast = "DE-DI-023"
    
    module Rules =
        let championSelectionSignificance = "BR-DI-020"
        let noHarmRule = "BR-DI-021"
        let forecastValidity = "BR-DI-023"
        let overrideJustification = "BR-DI-027"
        let overrideDeviationLimit = "BR-DI-028"
    
    module Policies =
        let autoPublication = "PO-DI-023"
        let overrideAuthorization = "PO-DI-025"
```

### 11.2.2 Runtime Attachment

When a domain decision function executes, it attaches the relevant ARS identifiers to the events it produces. Every event carries a `DecisionTrace` record that links the event back to its governing identifiers.

```fsharp
type DecisionTrace = {
    DecisionId: string            // e.g., "DE-DI-023"
    CapabilityId: string          // e.g., "CA-DI-002"
    RulesEvaluated: string list   // e.g., ["BR-DI-027"; "BR-DI-028"]
    PolicyId: string option       // e.g., "PO-DI-025"
    SemanticObjectIds: string list // e.g., ["SE-DI-003"; "SE-DI-023"]
}
```

This record is embedded in every event envelope's metadata under the key `decisionTrace`. It is the foundation for both human audit trails and AI‑generated explanations.

## 11.3 Execution Context and Correlation

The `ExecutionContext` is the carrier of distributed tracing information. It is created at the entry point (API request, event handler, or scheduler trigger) and propagated through every subsequent operation.

```fsharp
type ExecutionContext = {
    CorrelationId: Guid          // Groups all events in the same business flow
    CausationId: Guid option     // The immediate parent event or command
    Principal: string option     // User or system that initiated the operation
    Timestamp: DateTimeOffset    // When the context was created
    TenantId: string option      // Multi‑tenancy partition key
    MessageId: string option     // Client‑supplied idempotency key
}
```

### 11.3.1 Propagation Through the Command Pipeline

```
HTTP Request (CorrelationId from header, or newly generated)
        │
        ▼
ExecutionContext stored in AsyncLocal<ExecutionContext>
        │
        ▼
Application Service reads ExecutionContext
        │
        ├──► Enriches Envelope with correlationId and causationId
        ├──► Passes context to PolicyChecker
        └──► Passes context to Telemetry emitter
```

Every log entry, every telemetry event, and every event envelope carries the `CorrelationId` from the current `ExecutionContext`. This means a single business flow—say, an order promise—can be traced across every domain that participates: Demand validation → Supply reservation → Promise confirmation → Customer notification.

### 11.3.2 Causation Chains

`CausationId` links each event to its immediate cause. When a Process Manager dispatches a command in response to an event, the command's `ExecutionContext` carries the event's ID as `CausationId`. This creates a directed acyclic graph of causality.

```
OrderReceived (Event A, CorrelationId = X, CausationId = None)
  └─► ValidateDemand (Command, CausationId = A.MessageId)
        └─► DemandValidated (Event B, CorrelationId = X, CausationId = A.MessageId)
              └─► ReserveSupply (Command, CausationId = B.MessageId)
                    └─► SupplyReserved (Event C, CorrelationId = X, CausationId = B.MessageId)
```

This chain is queryable: given any event, you can walk backward to the originating request and forward to every consequence.

## 11.4 Event Envelope Metadata

Every event persisted to the event store carries the full traceability context in its envelope metadata.

```fsharp
type Envelope = {
    EventId: Guid
    EventType: string
    DataJson: string
    SchemaVersion: int
    StreamName: string
    CreatedUtc: DateTimeOffset
    CorrelationId: Guid option
    CausationId: Guid option
    TenantId: string option
    Metadata: Map<string, string>
}
```

The `Metadata` map is enriched by the application service before each append:

| Key | Value | Source |
|-----|-------|--------|
| `correlationId` | Guid | `ExecutionContext.CorrelationId` |
| `causationId` | Guid | `ExecutionContext.CausationId` |
| `principal` | string | `ExecutionContext.Principal` |
| `aggregateId` | string | The aggregate's identifier |
| `aggregateType` | string | The aggregate type name |
| `decisionTrace` | JSON | Serialised `DecisionTrace` record |
| `messageId` | string | `ExecutionContext.MessageId` |

Envelope enrichment is performed by a single function, `Envelope.withExecutionContext`, called consistently by every application service.

## 11.5 Decision Traceability Chain

Every domain decision produces a traceability chain that links the specific command to the ARS identifiers of the rules and policies that governed it.

### 11.5.1 Building the Trace

The application service is responsible for building the `DecisionTrace` and attaching it to events:

```fsharp
let overrideForecast (cmd: OverrideForecastCmd) (ctx: ExecutionContext) =
    task {
        let! aggregate = repo.Load(cmd.ForecastId)
        match aggregate with
        | Error e -> return Error e
        | Ok agg ->
            // Domain decision
            match decideOverride agg.State cmd with
            | Error e -> return Error e
            | Ok events ->
                // Build the decision trace
                let trace = {
                    DecisionId = ArsIdentifiers.Demand.Decisions.overrideForecast
                    CapabilityId = ArsIdentifiers.Demand.Capabilities.forecastDemand
                    RulesEvaluated = [
                        ArsIdentifiers.Demand.Rules.overrideJustification
                        ArsIdentifiers.Demand.Rules.overrideDeviationLimit
                    ]
                    PolicyId = Some ArsIdentifiers.Demand.Policies.overrideAuthorization
                    SemanticObjectIds = ["SE-DI-003"; "SE-DI-023"]
                }
                
                // Enrich events with trace before appending
                let enrichedEvents = events |> List.map (fun ev -> 
                    ev |> withDecisionTrace trace |> withExecutionContext ctx)
                
                let! result = repo.Append(cmd.ForecastId, agg.Version, enrichedEvents)
                // ...
    }
```

### 11.5.2 The Complete Traceability Chain

The ARS mandates a chain from violation back to constitution. In the running system, this chain is:

```
Runtime Event (e.g., ForecastOverridden)
        │ (carries DecisionTrace)
        ▼
Decision ID (DE‑DI‑023 — Override Forecast)
        │ (carries RulesEvaluated and PolicyId)
        ├──► Rule IDs (BR‑DI‑027, BR‑DI‑028)
        └──► Policy ID (PO‑DI‑025)
                │
                ▼
Capability ID (CA‑DI‑002 — Forecast Demand)
        │
        ▼
Semantic Object ID (SE‑DI‑003 — Forecast, SE‑DI‑023 — Forecast Override)
        │
        ▼
Constitution Principle (C‑EP‑001, C‑EX‑001, C‑TR‑001)
```

This chain can be traversed at runtime by the Explain capability to generate a human‑ or AI‑readable explanation of any decision.

## 11.6 AI Explainability Chains

AI agents produce explanations using the same `DecisionTrace` structure. The traceability chain is populated automatically from the agent's context.

An AI‑generated forecast override carries:

```
DecisionTrace {
    DecisionId = "DE-DI-023"
    CapabilityId = "CA-DI-002"
    RulesEvaluated = ["BR-DI-027"; "BR-DI-028"]
    PolicyId = "PO-DI-025"
    SemanticObjectIds = ["SE-DI-003"; "SE-DI-023"]
}
```

The `Explain Knowledge Insights` capability (Knowledge Intelligence 5.10) consumes this trace and the underlying event data to generate a structured explanation:

```json
{
  "artifactId": "FC-2026-001",
  "type": "ForecastOverride",
  "naturalLanguage": "Forecast for SKU123 overridden to 500 units because CRM data indicates a confirmed large order from Customer X. Rule BR-DI-027 required a justification, which was provided. Rule BR-DI-028 confirmed the deviation is within the 50% limit. Policy PO-DI-025 authorised the planner to make this override.",
  "traceabilityChain": {
    "decision": "DE-DI-023",
    "capability": "CA-DI-002",
    "rules": ["BR-DI-027", "BR-DI-028"],
    "policy": "PO-DI-025",
    "constitutionalPrinciples": ["C-EP-001", "C-EX-001", "C-TR-001"]
  },
  "confidence": 94,
  "explainabilityScore": 92
}
```

This explanation is served to both human users (via the Store pattern and UI) and AI agents (via `Serve Knowledge to AI Agents`).

## 11.7 Audit Logging and the Immutable Event Store

The PostgreSQL `events` table is the immutable audit log. Every event, once appended, cannot be modified or deleted. The `events` table plus the envelope metadata form a complete, tamper‑evident record of every state change in the system.

### 11.7.1 What is Recorded

- **Business events**: `DemandLineIngested`, `ForecastGenerated`, `SupplyPlanPublished`, `PromiseConfirmed`, etc.
- **Decision traces**: Which rules were evaluated, which policies applied.
- **AI recommendations**: Every recommendation, whether accepted, rejected, or modified.
- **Human overrides**: Every manual change with the planner's identity and justification.
- **Policy changes**: Every adjustment to thresholds, weights, or planning parameters, with the `PolicyGate` validation result.

### 11.7.2 Audit Queries

The event store supports the following audit queries without additional infrastructure:

| Question | How to Answer |
|----------|---------------|
| What happened to order ORD‑890? | Query all events where `aggregateId = "ORD-890"`, ordered by `created_utc`. |
| Who overrode the forecast for SKU123? | Query `ForecastOverridden` events for that SKU; extract `principal` from metadata. |
| What rules were evaluated for this promise? | Extract `decisionTrace` from the `PromiseConfirmed` event; read `RulesEvaluated`. |
| What was the full causal chain for this disruption? | Walk `causationId` links backward from the disruption event to the originating trigger. |
| How many AI recommendations were rejected this quarter? | Query events where `event_type = "AIRecommendationRejected"`, count by agent. |

These queries use standard SQL over the `events` table and its JSONB indexes. No separate audit database is required.

### 11.7.3 Replay Safety

Events can be replayed for debugging, testing, or recovery. Every replay is recorded with:

- `replayOrigin`: who initiated the replay, under what ticket, and why.
- `replayTimestamp`: when the replay was executed.

Replayed events are clearly marked so that projections and subscribers can distinguish live events from replayed events and avoid double‑processing.

## 11.8 Runtime Traceability Example

The ARS defines a canonical runtime traceability example:

```
VI‑R‑008 → FS‑R‑018 → BR‑S‑011 → DE‑R‑003 → CA‑R‑002 → SE‑R‑001 → CN‑004
```

In the running system, this chain is traversable:

1. **Violation `VI‑R‑008`**: A telemetry event reports that a promise was breached (late delivery). The event carries a `CorrelationId` and references the affected `PromiseId`.
2. **Functional Specification `FS‑R‑018`**: The violation is traced to the functional behaviour of `Promise Orders`, which specified that ATP evaluation must use a temporary reservation to prevent over‑booking. The reservation was not confirmed before the supply was consumed.
3. **Business Rule `BR‑S‑011`**: The root cause is traced to a rule violation in Supply—a temporary reservation expired because the evaluation exceeded the configurable timeout, and the supply was consumed by another order.
4. **Decision `DE‑R‑003`**: The decision that failed was `Confirm Promise` in Promise Intelligence, which attempted to confirm an expired reservation.
5. **Capability `CA‑R‑002`**: The owning capability is `Promise Orders`.
6. **Semantic Object `SE‑R‑001`**: The semantic concept involved is `Promise` (the commitment that was breached).
7. **Constitutional Principle `CN‑004`**: The violation is ultimately a failure of the principle of `Architectural Consistency`, because the timeout configuration was not aligned between the Promise and Supply bounded contexts.

This entire chain is reconstructable from the event store and the ARS identifier registry. No manual investigation is required—the Explain capability can generate this trace automatically for any violation.

## 11.9 Implementation in Code

Traceability is not a separate service or a bolt‑on. It is embedded in the fabric of every application service.

```fsharp
// Standard traceability wrapper for every command handler
let withTraceability
    (cmd: 'Cmd)
    (ctx: ExecutionContext)
    (decisionId: string)
    (capabilityId: string)
    (rulesEvaluated: string list)
    (policyId: string option)
    (handler: 'Cmd -> Task<Result<'Events, ApplicationError>>)
    : Task<Result<'Events, ApplicationError>> =
    
    task {
        let! result = handler cmd
        match result with
        | Error e -> return Error e
        | Ok events ->
            let trace = {
                DecisionId = decisionId
                CapabilityId = capabilityId
                RulesEvaluated = rulesEvaluated
                PolicyId = policyId
                SemanticObjectIds = []  // populated by caller if needed
            }
            let enrichedEvents = events |> List.map (fun ev ->
                ev
                |> withDecisionTrace trace
                |> withExecutionContext ctx)
            
            // Emit telemetry with the trace
            Telemetry.logWithCorrelation logger ctx.CorrelationId Information 
                $"Decision {decisionId} executed" 
                (Map.ofList ["decisionTrace", box trace])
            
            return Ok enrichedEvents
    }
```

This wrapper ensures that every command handler consistently records the decision trace, enriches events with traceability metadata, and emits telemetry—without each handler having to implement the logic independently.

---

# Chapter 12 — Observability

## 12.1 The Observability Stack

Medhavi’s observability is built on three pillars—**structured logging**, **metrics**, and **distributed tracing**—supplemented by **health checks** and **alerting**. These are not separate concerns bolted on after development; they are integrated into the `SharedKernel` and used uniformly by every bounded context.

The observability modules are:

| Module | Purpose |
|--------|---------|
| `Logging` | Structured, correlation‑aware logging via `ILogger` with async batching |
| `Telemetry` | Structured telemetry events with severity, properties, and trace context |
| `Metrics` | Counters, gauges, histograms for quantitative measurement |
| `Performance` | Automatic performance measurement with telemetry conversion |
| `ActivityTracking` | OpenTelemetry‑compatible distributed tracing via `System.Diagnostics.Activity` |
| `HealthCheck` | Component health status reporting with telemetry integration |

Together they provide complete visibility into the system’s behaviour, from a single function call to a cross‑domain business flow.

## 12.2 Structured Logging

Every log entry in Medhavi carries structured context, not just a text message. This enables precise filtering, aggregation, and correlation in log aggregation tools.

### 12.2.1 LogContext

```fsharp
type LogContext = {
    CorrelationId: Guid option
    Operation: string option
    Component: string
    EntityId: string option
    EntityType: string option
    StreamName: string option
    EventId: Guid option
    EventType: string option
    Duration: TimeSpan option
    AdditionalData: Map<string, obj> option
}
```

The `LogContext` is built from the current `ExecutionContext` (for traceability) and enriched with domain‑specific information (the aggregate being modified, the event being processed, the operation being performed).

### 12.2.2 Logger

The `Logger` type wraps `Microsoft.Extensions.Logging.ILogger` with convenience methods that automatically merge the ambient `LogContext`:

```fsharp
type Logger = {
    InnerLogger: ILogger
    Context: LogContext
    MailboxLogger: MailboxLogger option
}

// Usage:
logger.Info("Forecast generated successfully")
logger.Error(ex, "Failed to publish forecast")
logger.LogPerformance("GenerateForecast", "PlanningEngine", duration)
```

The `MailboxLogger` provides asynchronous batching for high‑throughput scenarios, using the same `MailboxProcessor` pattern as projections and hotspot agents. In the MVP, logging is direct; in production, the mailbox logger can be enabled for specific high‑volume components.

### 12.2.3 Component Naming

All log entries carry a hierarchical component name, enforced by the `ComponentNaming` module:

```fsharp
// Produces: "Actor.Aggregate.DemandLine"
ComponentNaming.Actor.aggregate "DemandLine"

// Produces: "Service.ForecastPublisher"
ComponentNaming.Service.service "ForecastPublisher"
```

This naming convention ensures that log entries are immediately traceable to their source, without guessing which service or module produced them.

## 12.3 Metrics

Quantitative measurement is provided by the `Metrics` module. Three metric types are supported:

| Metric Type | Purpose | Example |
|-------------|---------|---------|
| **Counter** | Cumulative count of events | `event_ingest_rate` — events ingested per second |
| **Gauge** | Point‑in‑time value | `projection_lag` — seconds behind the event stream |
| **Histogram** | Distribution of values | `command_latency_ms` — distribution of command execution times |

```fsharp
// Recording a counter
let eventIngested = Metrics.recordCounter "event_ingest_rate" 1.0 (Map.ofList ["domain", "Demand"])

// Recording a gauge
let lag = Metrics.recordGauge "projection_lag" 2.5 (Map.ofList ["projection", "DemandLineReadModel"])

// Recording a histogram
let latency = Metrics.recordHistogram "command_latency_ms" 45.0 (Map.ofList ["command", "OverrideForecast"])
```

Every metric point carries tags for domain, component, and operation. These tags are used by Prometheus/Grafana for filtering and aggregation.

## 12.3.1 Domain‑Specific Telemetry

In addition to generic metrics, Medhavi defines domain‑specific telemetry types in `SharedKernel`. These are produced by bounded contexts and consumed by Knowledge Intelligence:

```fsharp
type PlanningKpis = {
    ScenarioId: string
    TotalCost: decimal
    LatenessPenalty: decimal
    CapacityUtilization: Map<string, float>
    ServiceLevel: float
    TenantId: string option
}

type LatencyTelemetry = {
    OperationName: string
    Component: string
    DurationMs: float
    IsSuccess: bool
    CorrelationId: Guid
}

type LimiterFrequencyTelemetry = {
    LimiterName: string
    Utilization: float option
    ThrottledCount: int64
    IsActive: bool
}
```

These types are not abstract—they are concrete records emitted at specific points in the planning pipeline. Knowledge Intelligence subscribes to them via the event bus and uses them for cross‑domain pattern discovery.

## 12.4 Distributed Tracing

Distributed tracing follows a request across service boundaries. Medhavi uses `System.Diagnostics.Activity` (the .NET OpenTelemetry‑compatible API) wrapped by the `ActivityTracking` module.

```fsharp
let withActivity (logger: LogTelemetryEvent) (activityName: string) (tags: (string * string) list) (operation: unit -> 'T) : 'T =
    let activity = new Activity(activityName)
    for (key, value) in tags do activity.SetTag(key, value) |> ignore
    activity.Start()
    try
        let result = operation ()
        activity.Stop()
        // Emit telemetry with TraceId and SpanId
        result
    with ex ->
        activity.SetTag("error", "true") |> ignore
        activity.Stop()
        reraise()
```

Every significant operation—command handling, event processing, saga step, external API call—is wrapped in an activity. This produces a trace that can be visualised in Jaeger, Zipkin, or any OpenTelemetry‑compatible backend.

### 12.4.1 Trace Context Propagation

Trace context (`traceId`, `spanId`) is propagated through:

- **Event envelopes**: The `Metadata` map carries `traceId` and `spanId` from the publisher to all subscribers.
- **HTTP headers**: The standard `traceparent` header propagates context to external services.
- **ExecutionContext**: The `ActivityTracking` module sets the current `Activity` as the ambient context, and `Telemetry` events carry `TraceId` and `SpanId`.

This means a single trace can span an HTTP request → command handler → aggregate decision → event publication → projection update → cross‑context event handling—all visualised as a single trace.

## 12.5 Performance Measurement

Performance measurement is automated through the `PerformanceTracker` and `Performance` module.

### 12.5.1 PerformanceTracker (IDisposable)

```fsharp
use tracker = new PerformanceTracker(logger, "GenerateForecast", "PlanningEngine")
// ... operation ...
// tracker automatically logs duration on Dispose
```

The `PerformanceTracker` records the elapsed time on disposal. If the operation exceeds warning thresholds (1 second for information, 5 seconds for warning), the log level is automatically adjusted.

### 12.5.2 Performance Measurement Functions

```fsharp
let result, measurement = Performance.measure "ValidateDemand" (fun () -> validateDemand cmd)
// measurement.Duration, measurement.Success are available for telemetry
```

These functions return both the operation result and the performance measurement, allowing the caller to decide how to handle the measurement (log it, emit it as telemetry, or both).

## 12.6 Health Checks

Health checks provide a standardised way for infrastructure (Kubernetes, load balancers, monitoring tools) to assess whether a component is functioning correctly.

```fsharp
type ComponentHealth = {
    ComponentName: string
    Status: HealthStatus       // Healthy | Degraded of string | Unhealthy of string
    LastChecked: DateTimeOffset
    ResponseTime: TimeSpan option
    Details: Map<string, obj>
}
```

Every bounded context registers health checks for:

- **Event store connectivity**: Can the service read and write to the `events` table?
- **Projection lag**: Is the projection agent within acceptable lag of the event stream?
- **Circuit breaker state**: Are any circuit breakers open, indicating downstream failures?
- **External service connectivity**: Can the service reach required external APIs?

Health checks are aggregated by `Medhavi.Nexus` and exposed via the ASP.NET Core health check endpoint (`/health`). Kubernetes uses this endpoint for liveness and readiness probes.

## 12.7 Alerting and Notification

Alerting is driven by the metrics and health check data already being collected.

### 12.7.1 Alert Rules

Alert rules are defined as conditions on metrics or health status:

| Alert | Condition | Severity | Channel |
|-------|-----------|----------|---------|
| `projection_lag_high` | Projection lag > 30 seconds for > 2 minutes | Critical | PagerDuty, Slack |
| `circuit_open` | Any circuit breaker in Open state | High | Slack, Email |
| `event_ingest_drop` | Event ingest rate drops > 50% from baseline | High | Slack |
| `command_failure_rate` | Command failure rate > 5% over 5 minutes | Critical | PagerDuty |
| `health_degraded` | Any component in Degraded state for > 5 minutes | Warning | Slack |

### 12.7.2 Alert Pipeline

```
Metric emitted (e.g., projection_lag = 35s)
        │
        ▼
Alert Evaluator (checks against configured rules)
        │
        ├── No rule triggered → nothing
        │
        └── Rule triggered → Alert Event published
                │
                ├──► Telemetry event emitted (for audit and dashboards)
                ├──► Notification dispatched to configured channel(s)
                └──► Alert recorded in enterprise memory (for Knowledge Intelligence)
```

Alert rules are configuration‑driven (JSON or environment variables), not hard‑coded. They can be adjusted without code changes.

## 12.8 Export and Visualisation

In the MVP, telemetry and metrics are logged to the console and to the `ILogger` output. In production, they are exported to industry‑standard observability platforms.

### 12.8.1 Prometheus Metrics Export

The `Metrics` module produces metric points that are exported to Prometheus via the ASP.NET Core `/metrics` endpoint. Each bounded context registers its own metrics, and Prometheus scrapes them on a configurable interval.

### 12.8.2 Grafana Dashboards

Pre‑built Grafana dashboards visualise:

- **System health**: CPU, memory, event store latency, projection lag per context.
- **Business KPIs**: Forecast accuracy, supply plan adherence, promise fill rate, scenario recommendation adoption rate.
- **AI performance**: AI recommendation acceptance rate, autonomy contract violations, policy gate rejection reasons.
- **Trace explorer**: Drill‑down into individual traces for debugging.

### 12.8.3 Structured Log Export

Logs are emitted as structured JSON and can be ingested by any log aggregation platform (Elasticsearch, Splunk, Azure Monitor). The `LogContext` fields (`CorrelationId`, `Component`, `Operation`) enable precise filtering.

## 12.9 Observability in the MVP

In the MVP monolith, the full observability stack is active but simplified:

- **Logging**: Console output via `ILogger`, with `LogContext` serialised as JSON.
- **Metrics**: `MetricPoint` records are logged as telemetry events and can be viewed in the console.
- **Tracing**: `Activity` spans are created but may not be exported to a tracing backend in early development.
- **Health checks**: Available at `/health` endpoint, used by the development environment.

No external dependencies (Prometheus, Grafana, Elasticsearch) are required for the MVP. The instrumentation is present from day one; the export to external platforms is enabled when the operational need arises.

---

# Chapter 13 — Resilience & Error Handling

## 13.1 Error Taxonomy

Medhavi defines a three‑tier error taxonomy that separates domain failures, infrastructure failures, and application‑level errors. Every component uses the same types, defined in `SharedKernel`.

| Tier | Type | Purpose | Example |
|------|------|---------|---------|
| **Domain** | `DomainError` | Business rule violations, validation failures, invariants broken | `"Safety stock cannot be negative"` |
| **Infrastructure** | `InfrastructureError` | External system failures, timeouts, network errors, event store unavailability | `"Event store connection refused"` |
| **Application** | `ApplicationError` | Wraps Domain and Infrastructure errors; adds NotFound, Mismatch, External, Unknown | `NotFound("SKU123")`, `Mismatch(expectedV, actualV)` |

```fsharp
type DomainError =
    | ValidationError of code: string * message: string * data: Map<string, obj>
    | DomainError of code: string * message: string * data: Map<string, obj>

type InfrastructureError =
    | Network of string | Timeout of string | EventStore of string
    | Database of string | Http of string | CircuitOpen of string | OtherInfra of string

type ApplicationError =
    | Domain of DomainError
    | NotFound of code: string * message: string * data: Map<string, obj>
    | Mismatch of code: string * expected: Version * actual: Version
    | Infrastructure of InfraError
    | External of code: string * message: string * data: Map<string, obj>
    | Unknown of string
```

### 13.1.1 Error Flow Through Layers

```
External System / User
        │
        ▼
API Layer (Medhavi.Hub)
        │
        ├──► Maps ApplicationError → HTTP status codes
        │      • Domain → 422 Unprocessable Entity
        │      • NotFound → 404
        │      • Infrastructure → 503 Service Unavailable
        │      • Mismatch → 409 Conflict
        │
        ▼
Application Layer
        │
        ├──► Catches .NET exceptions → maps to ApplicationError via fromException
        ├──► Domain errors pass through unchanged
        └──► Infrastructure errors wrapped in ApplicationError.Infrastructure
        │
        ▼
Domain Layer
        │
        └──► Returns Result<'T, DomainError>
               • Pure, no exceptions for business logic
               • Validation errors accumulated via applicative validation
```

Domain logic **never throws exceptions** for business rule violations. It returns `Error` results. Infrastructure code (database access, HTTP calls) may throw .NET exceptions, which are caught at the application layer and mapped to `ApplicationError`.

## 13.2 Exception Handling Orchestration

The `ExceptionHandling` module provides a unified mechanism for executing operations with configurable recovery strategies.

```fsharp
type RecoveryStrategy =
    | Retry of maxRetries: int * delayMs: int
    | Fallback of fallbackValue: obj
    | CircuitBreak of serviceName: string
    | LogAndContinue
    | FailFast
```

### 13.2.1 The Core Execution Function

```fsharp
let executeWithErrorHandling (ctx: ExceptionContext) (operation: unit -> Task<'T>) (contextData: Map<string, obj>) =
    task {
        try
            let! result = operation ()
            return Ok result
        with ex ->
            let errorInfo = createErrorInfo ctx ex contextData
            logError ctx errorInfo (determineSeverity ctx.RecoveryStrategy)
            
            match ctx.RecoveryStrategy with
            | Retry(maxRetries, delayMs) ->
                return! retryWithBackoff ctx operation maxRetries delayMs
            | Fallback(fallbackValue) ->
                return Ok (fallbackValue :?> 'T)
            | CircuitBreak(serviceName) ->
                return Error (ApplicationError.Infrastructure (CircuitOpen serviceName))
            | LogAndContinue ->
                return Error (ApplicationError.fromException ex)
            | FailFast ->
                return Error (ApplicationError.fromException ex)
    }
```

### 13.2.2 ExceptionContext

```fsharp
type ExceptionContext = {
    CorrelationId: CorrelationId
    ServiceName: string
    OperationName: string
    Logger: (string -> unit)
    RecoveryStrategy: RecoveryStrategy
}
```

The `ExceptionContext` is populated from the ambient `ExecutionContext` at the application layer. This ensures every error record carries the `CorrelationId` of the originating request.

## 13.3 Circuit Breaker

The circuit breaker protects the system from cascading failures when an external dependency is unavailable. It is implemented as a `MailboxProcessor` agent, consistent with the rest of the architecture.

### 13.3.1 States

```
Closed ──(failures reach threshold)──► Open
  ▲                                      │
  │                              (timeout expires)
  │                                      ▼
  └───────────────────────────── HalfOpen
               (success)          │
                          (failure)
                                  │
                                  ▼
                                Open
```

| State | Behaviour |
|-------|-----------|
| **Closed** | Requests pass through normally. Failures are counted within a monitoring window. |
| **Open** | Requests are immediately rejected with `CircuitOpen` error. No calls are made to the failing dependency. |
| **HalfOpen** | A limited number of trial requests are allowed. If they succeed, the circuit closes. If they fail, it re‑opens. |

### 13.3.2 Configuration

```fsharp
type CircuitBreakerConfig = {
    FailureThreshold: int          // Number of failures before opening (default: 5)
    RecoveryTimeout: TimeSpan      // Time before attempting HalfOpen (default: 30s)
    MaxRecoveryTimeout: TimeSpan   // Maximum backoff (default: 60s)
    BackoffFactor: float           // Exponential backoff multiplier (default: 2.0)
    MonitoringPeriod: TimeSpan     // Window for counting failures (default: 30s)
    SuccessThreshold: int          // Successes needed in HalfOpen to close (default: 3)
    OnEvent: (CircuitBreakerEvent -> unit) option  // Observability callback
}
```

Recovery timeout uses exponential backoff: each time the circuit opens, the timeout doubles (up to `MaxRecoveryTimeout`). This prevents hammering a recovering dependency.

### 13.3.3 Usage

```fsharp
let breaker = CircuitBreaker.create config (Some logger) None

let! result = breaker.ExecuteAsync(fun () -> 
    externalService.CallAsync())
    
match result with
| CircuitBreakerResult.Success data -> // use data
| CircuitOpen reason -> // circuit is open, use fallback
| ExecutionFailed error -> // operation failed within the circuit
```

Circuit breakers are created per external dependency (e.g., one for the ERP adapter, one for the MES adapter). They are registered in the bounded context's composition root and injected into application services.

### 13.3.4 Observability

Every state transition emits a `CircuitBreakerEvent`:

```fsharp
type CircuitBreakerEvent =
    | Opened of timestamp * reason * consecutiveOpens
    | HalfOpened of timestamp
    | Closed of timestamp
    | RequestSucceeded of timestamp
    | RequestFailed of timestamp * error
```

These events are published to the `DomainEventBus` and consumed by the observability layer. Dashboards display real‑time circuit breaker status. Alerts are triggered when a circuit stays open beyond a threshold.

## 13.4 Retry Policies

Retry policies are used for transient failures—temporary network issues, brief database unavailability, rate limiting.

```fsharp
let rec retryWithBackoff (ctx: ExceptionContext) (operation: unit -> Task<'T>) (maxRetries: int) (delayMs: int) =
    task {
        let mutable attempt = 0
        let mutable result = None
        while attempt <= maxRetries && result.IsNone do
            try
                let! value = operation ()
                result <- Some (Ok value)
            with ex when attempt < maxRetries ->
                attempt <- attempt + 1
                let backoffMs = delayMs * (1 <<< attempt)  // exponential: 100, 200, 400, 800ms
                ctx.Logger $"Retry {attempt}/{maxRetries} after {backoffMs}ms for {ctx.OperationName}: {ex.Message}"
                do! Task.Delay backoffMs
        return result |> Option.defaultValue (Error (ApplicationError.fromException (Exception("Max retries exceeded"))))
    }
```

Retry policies are applied selectively:

| Scenario | Strategy | Reason |
|----------|----------|--------|
| Database connection failure | Retry × 3, 100ms backoff | Transient network issues |
| HTTP 429 (rate limited) | Retry × 5, 500ms backoff | Respect server's rate limit window |
| HTTP 503 (service unavailable) | Retry × 3 + circuit breaker | Allow service to recover |
| Domain validation error | No retry | Validation errors won't resolve with retries |
| Optimistic concurrency conflict | Retry × 3, 100ms backoff | Other writer may have completed |

## 13.5 Dead Letter Queues

When an event cannot be processed after all retries are exhausted, it is moved to a **dead letter queue (DLQ)** for manual investigation.

The DLQ is implemented as a dedicated event stream, `$dlq-{domain}`. Failed events are appended to this stream with:

- The original envelope.
- The error that caused the failure.
- The retry history.
- A timestamp.

```fsharp
let moveToDlq (envelope: Envelope) (error: exn) (retryCount: int) =
    let dlqEnvelope = Envelope.createEnvelope "DeadLetter" (serialize envelope) 1
        |> Envelope.withMetadata "error" error.Message
        |> Envelope.withMetadata "retryCount" (retryCount.ToString())
        |> Envelope.withMetadata "originalStream" envelope.StreamName
    
    eventStore.Publish $"$dlq-{domain}" [dlqEnvelope] ExpectedRevision.Any
```

A dedicated dashboard displays DLQ contents, and alerts are triggered when new messages appear in the DLQ. Operators can inspect the failed event, correct the underlying issue, and replay the event if appropriate.

## 13.6 Error → Telemetry Bridge

Every error is routed to the telemetry system for observability and learning.

```fsharp
let reportErrorToTelemetry (ctx: ExceptionContext) (error: ApplicationError) =
    let telemetryError = {
        Component = ctx.ServiceName
        ErrorCode = error.Code
        ErrorMessage = error.Message
        CorrelationId = ctx.CorrelationId
        TenantId = None  // populated from ExecutionContext if available
    }
    // Emit as a telemetry metric
    let metric = TelemetryMetric.ErrorEvent telemetryError
    DomainEventBus.Publish(metric)
```

The bridge ensures that:

1. Every error is recorded with its `CorrelationId`, enabling traceability back to the originating request.
2. Error rates by component, error code, and time period are available in dashboards.
3. Knowledge Intelligence can analyse error patterns across domains (e.g., a supplier failure in Supply causing promise breaches in Promise).
4. Alerts are triggered when error rates exceed thresholds.

## 13.7 Resilience in the MVP

In the MVP monolith:

- **Circuit breakers** are optional. External dependencies may not exist yet, so circuit breakers are instantiated but not actively protecting live calls.
- **Retry policies** are active for database access and any external HTTP calls made during development.
- **Dead letter queues** are implemented using the in‑memory event store for development and testing.
- **Error → Telemetry bridge** is active; all errors are logged and available in the console.

As external dependencies are added (ERP integration, MES integration), circuit breakers are activated for those specific dependencies. The resilience infrastructure is in place from day one.

## 13.8 Graceful Degradation

When a downstream capability is unavailable, Medhavi degrades gracefully rather than failing completely.

| Scenario | Degradation Behaviour |
|----------|----------------------|
| **Event store unavailable** | Commands return 503; queries serve stale data from in‑memory projections with a `Stale` freshness flag. |
| **External ERP unavailable** | Integration adapter circuit opens; events are buffered and replayed when the ERP recovers. |
| **AI agent unavailable** | Planning continues without AI recommendations; autonomy contracts default to human approval. |
| **Knowledge Intelligence unavailable** | Other domains operate normally; AI agents query only their local context without enterprise memory enrichment. |
| **Projection lag exceeds threshold** | Health check reports `Degraded`; queries serve data with a `StaleSince` timestamp; writes continue normally. |

The system never blocks a write because a read model is behind. The event store is the source of truth; projections are an optimisation.

---


# Chapter 14 — Presentation Architecture & Store Pattern

## 14.1 Technology Stack

The Medhavi front end is built entirely in F# using **Bolero** (F# on Blazor), with **Radzen Blazor** for UI components. State management follows the **Elmish MVU** pattern, extended with a **Store layer** for shared, reactive, read‑only state. The same F# types that define backend commands and queries are used in the front end, giving end‑to‑end type safety.

| Layer | Technology | Rationale |
|-------|------------|-----------|
| **Runtime** | .NET 10, Blazor Server | Single language stack; server‑side rendering for fast initial load and direct backend access |
| **Framework** | Bolero | Elmish MVU, type‑safe routing, F# components |
| **UI Components** | Radzen Blazor | Rich component library with built‑in theming and accessibility |
| **State** | Elmish + WorkspaceStore | Predictable one‑way data flow; shared state with freshness and reactive updates |
| **AI Copilot** | Elmish Actions + Command Palette | AI uses identical Workspace Actions; no separate UI path |

## 14.2 Shell Architecture

The UI is composed of two nested shells, a composition root, and multiple workspaces. Dependency injection flows top‑down; actions flow bottom‑up via `MsgOut`.

```
SystemShell
  │  Creates DI container, injects services, Stores, authentication
  │  Passes dependencies to ApplicationShell
  │
  └── ApplicationShell
        │  Owns global UI state: navigation, session, command palette
        │  Hosts the active workspace
        │  Dispatches cross‑cutting WorkspaceActions (NavigateTo, Refresh, OpenCopilot)
        │  Receives MsgOut from workspaces and coordinates
        │
        └── Workspaces (independent Elmish components)
              │  Own local UI state (selected rows, form inputs)
              │  Subscribe to Stores for shared data
              │  Emit MsgOut upward for cross‑cutting actions
```

**Key rules:**
- **SystemShell** does not render UI; it is a startup container.
- **ApplicationShell** is the top‑level `ElmishComponent`, receiving `AppShellEnv` (services, Store registry, tooltip).
- **Workspaces** are initialised lazily. Their state is held as `option` fields in `AppShellModel`, so navigating away preserves state without re‑render of siblings.
- **MsgOut** is the only mechanism for a workspace to communicate upward. The parent uses `updateChildWithOutput` to process child messages, preventing unnecessary re‑renders of the whole shell.

## 14.3 The Store Layer — Reactive Read Models

Stores are the bridge between the backend event‑driven architecture and the UI. They cache projection data, track freshness, and push updates to subscribers. A Store is **never mutated directly** by the UI or by AI agents. All changes flow through the backend; Stores refresh from backend query services or are updated by projection subscriptions.

### 14.3.1 WorkspaceStore<'TState>

The generic `WorkspaceStore` provides:

- A cached `WorkspaceSnapshot<TState>` with freshness, version, and optional error.
- A `Refresh` method that calls a backend query and updates the snapshot.
- A `Subscribe` mechanism for reactive UI updates.
- An internal `updateStore` function for applying optimistic updates from projection events.

```fsharp
type WorkspaceStore<'TState> =
    { Get: unit -> WorkspaceSnapshot<'TState>
      Refresh: PlanningContext -> TaskResult<WorkspaceSnapshot<'TState>, string>
      MarkStale: unit -> unit
      Subscribe: (StoreEvent<WorkspaceSnapshot<'TState>> -> unit) -> SubscriptionId
      Unsubscribe: SubscriptionId -> unit
      Clear: unit -> unit }
```

**Freshness lifecycle:**  
`Fresh → Stale → Loading → Fresh` (on success) or `Failed` (on error). A context change marks all stores stale; the next refresh loads current data.

**Internal mutation:**  
The `create` function returns a second value, `updateStore : (('TState option -> 'TState option) -> unit)`, which is **not exposed to the UI**. It is used exclusively by projection subscription handlers to apply incremental updates without a full backend query.

**Concurrency:**  
Refresh uses a `SemaphoreSlim` to avoid overlapping requests. Snapshot state is protected by a lock, and listeners are held in a `ConcurrentDictionary`.

#### 14.3.2 PlanningContextStore

A specialised store that holds the shared planning context (scenario, horizon, plant, product families). When the context changes, it automatically marks **all** registered WorkspaceStores as stale via the `WorkspaceStoreRegistry`.

```fsharp
type PlanningContextStore =
    { Get: unit -> PlanningContext
      Set: PlanningContext -> unit
      Update: (PlanningContext -> PlanningContext) -> unit
      Subscribe: (PlanningContext -> unit) -> SubscriptionId
      Unsubscribe: SubscriptionId -> unit }
```

### 14.3.3 WorkspaceStoreRegistry

The registry holds all WorkspaceStores, keyed by `WorkspaceKind`. It provides `Register`, `TryGet`, `MarkAllStale`, and `ClearAll`. During creation, it subscribes to the `PlanningContextStore` so that any context change automatically marks every store stale. Workspaces then refresh on next activation or in response to the `StoreUpdated` event.

```fsharp
type WorkspaceStoreRegistry =
    { ContextStore: PlanningContextStore
      Register: WorkspaceKind * obj -> unit
      TryGet: WorkspaceKind -> obj option
      MarkAllStale: unit -> unit
      ClearAll: unit -> unit }
```

## 14.4 Projection Subscription Layer — Event‑Driven Store Updates

Instead of polling, the UI receives near‑real‑time updates from the backend via the existing `DomainEventBus`. The `ProjectionSubscription` module subscribes to domain event notifications (e.g., `DemandCreatedNotification`) and calls the corresponding handler on the appropriate store.

```fsharp
let create (demandHandlers: StoreNotificationHandlers) =
    let subscriptions =
        [ DomainEventBus.Subscribe<DemandCreatedNotification>(fun n ->
              runHandler demandHandlers.OnCreated n.DemandLineId)
          DomainEventBus.Subscribe<DemandUpdatedNotification>(fun n ->
              runHandler demandHandlers.OnUpdated n.DemandLineId)
          DomainEventBus.Subscribe<DemandDeletedNotification>(fun n ->
              runHandler demandHandlers.OnDeleted n.DemandLineId) ]
    ...
```

Each handler fetches the latest state from the backend query service and calls the store’s internal `updateStore` to patch the cached snapshot. This keeps the UI eventually consistent without any polling.

## 14.5 Example: DemandStore

`DemandStore` wires a query service and a command API into a `WorkspaceStore<DemandData>` and exposes `StoreNotificationHandlers` for the projection subscription.

```fsharp
let create (commandService: DemandLineApi) (queryService: DemandLineQueries) (initialContext: PlanningContext) =
    let loadFromBackend (context: PlanningContext) =
        taskResult {
            let! demands = queryService.GetAll()
            return DemandData.ofList demands
        }
    let store, updateStore = WorkspaceStore.create loadFromBackend initialContext None

    let onDemandCreated demandLineId =
        taskResult {
            let! demandOpt = queryService.GetById(demandLineId)
            match demandOpt with
            | Some demand -> updateStore(fun currentData -> ...)
            | None -> ()
        }
    ...
    let handlers = { OnCreated = onDemandCreated; OnUpdated = ...; OnDeleted = ... }
    store, handlers
```

During `StoreComposition`, all stores are created and registered. The `ProjectionSubscription` is started with the handlers, closing the loop from backend events to UI state.

## 14.6 Workspace Actions and AI Integration

`WorkspaceAction` defines cross‑cutting actions that can be initiated by the user (via navigation or command palette) or by AI agents.

```fsharp
type WorkspaceAction =
    | NavigateTo of Workspace
    | RefreshActiveWorkspace
    | RefreshAllWorkspaces
    | ApplyContext of PlanningContext
    | OpenCopilot
    | ShowWorkspaceEvents
    | Help
```

AI‑generated actions follow the exact same path. The `CommandTrace` records `Origin = Ai` to distinguish AI from human commands in the session history.

## 14.7 One‑Way Data Flow

All data flows in one direction:

1. **User/AI action** → `WorkspaceAction` or workspace `Msg`.
2. **Service call** to backend API (command or query).
3. **Backend** processes, persists, publishes events.
4. **Projection** updates (in backend) and notification events fire.
5. **Projection subscription** receives notification, calls store handler.
6. **Store** applies update via internal `updateStore`, sets freshness, notifies subscribers.
7. **Workspace** receives `StoreUpdated` message, updates its model, re‑renders.

The UI never directly mutates business data. It always goes through the backend.

## 14.8 Admin Center

The Admin Center is a workspace within the Bolero UI that provides administrators with full visibility into traceability, audit trails, AI decisions, system configuration, health, and the event store. It consumes data already produced by the existing capabilities and requires no new backend services. Access is restricted to users with the `Administrator` role.

**Panels:**

| Panel | Purpose | Data Source |
|-------|---------|-------------|
| **Traceability Explorer** | Search by `CorrelationId` to view the complete causal chain of commands, events, and decisions. Reconstructs the full ARS traceability chain from any event. Drills into `DecisionTrace` metadata. | Enterprise Memory (CA‑KN‑007), event store |
| **Audit Log Viewer** | Filterable audit trail by user, AI agent, domain, action type, and date range. Distinguishes human from AI actions. Shows policy change history with `PolicyGate` results. | Enterprise Memory, audit streams |
| **AI Decision Review** | AI recommendation log with confidence, autonomy level, and acceptance status. AI effectiveness metrics per agent and domain. Autonomy contract viewer. Escalated decisions with outcomes. | Enterprise Memory, `CommandHistory` |
| **Configuration Manager** | Feature flag dashboard (view/toggle). Read‑only configuration viewer. ARS identifier registry (searchable). | `SharedKernel.Configuration` |
| **System Health** | Component health grid, circuit breaker status, projection lag monitor, event ingest rate. | Health check endpoints, telemetry |
| **Event Store Browser** | Browse events by stream with position and timestamp. Full envelope detail. DLQ browser. Replay initiation (admin only). | Event store (read‑only) |

## 14.9 Summary

The presentation architecture fully aligns with the Blueprint’s principles:

- **Read‑only Stores** with freshness tracking and reactive updates.
- **Projection subscriptions** that eliminate polling and keep the UI eventually consistent.
- **Shell isolation** via `MsgOut` and lazy workspace initialisation.
- **AI integration** through identical action types and command palette.

This design keeps the UI predictable, testable, and ready for both human planners and AI copilots.

# Chapter 15 — Data & Persistence Management  

### 15.1 Persistence Architecture Overview  

Medhavi separates persistence into two distinct concerns, managed through a single PostgreSQL database:

- **Event Store** — the immutable source of truth for all state changes. Every aggregate decision appends events; no state is ever updated in place.  
- **Checkpoint & Snapshot Store** — operational metadata that enables projections and subscribers to resume processing efficiently.  

The MVP uses an in‑memory event store for speed and simplicity. The production architecture swaps that for PostgreSQL without changing any domain or application code—only the repository implementation changes.

```
┌────────────────────────────────────────────────────────────┐
│                     PostgreSQL (single instance)            │
│                                                            │
│  ┌──────────────┐  ┌───────────────┐  ┌──────────────────┐ │
│  │ events       │  │ checkpoints   │  │ snapshots        │ │
│  │ (append‑only)│  │ (projection   │  │ (optional,       │ │
│  │              │  │  progress)    │  │  per‑projection) │ │
│  └──────────────┘  └───────────────┘  └──────────────────┘ │
└────────────────────────────────────────────────────────────┘
```

### 15.2 Event Store Schema  

All domain events are stored in a single `events` table. The schema is designed for append‑only writes, fast positional reads, and efficient JSON queries.

```sql
CREATE TABLE events (
    stream_name      TEXT        NOT NULL,
    stream_position  BIGINT      NOT NULL,
    event_id         UUID        NOT NULL,
    event_type       TEXT        NOT NULL,
    data_json        JSONB       NOT NULL,
    metadata_json    JSONB       NOT NULL,
    created_utc      TIMESTAMPTZ NOT NULL DEFAULT now(),
    tenant_id        TEXT        NULL,

    PRIMARY KEY (stream_name, stream_position)
);

CREATE INDEX idx_events_created   ON events (created_utc);
CREATE INDEX idx_events_type     ON events (event_type, created_utc);
CREATE INDEX idx_events_tenant   ON events (tenant_id, created_utc) WHERE tenant_id IS NOT NULL;
CREATE INDEX idx_events_correlation ON events ((metadata_json->>'correlationId'), created_utc);
```

**Stream naming:**  
- Aggregate streams: `{domain}-{aggregateType}-{aggregateId}` (e.g., `demand-DemandLine-DL‑001`).  
- Category streams: `$ce-{domain}` (projections consuming all events from a domain).  
- Integration streams: `$integration` (events explicitly published across contexts).  
- System streams: `$checkpoint`, `$snapshot`.  

**Append semantics:**  
Events are appended atomically to a stream with optimistic concurrency. The application provides an `ExpectedRevision`. If the actual stream version does not match, the append is rejected, and the caller retries. This is enforced at the database level:

```sql
INSERT INTO events (stream_name, stream_position, …)
SELECT @stream, COALESCE(MAX(stream_position), -1) + 1, …
FROM events WHERE stream_name = @stream
HAVING COALESCE(MAX(stream_position), -1) = @expectedVersion;
```

### 15.3 Checkpoints  

Checkpoints record the last successfully processed position for each projection. On restart, a projection resumes from its checkpoint rather than replaying the entire event history.

```sql
CREATE TABLE checkpoints (
    projection_name  TEXT PRIMARY KEY,
    last_position    BIGINT NOT NULL,
    last_message_id  UUID   NOT NULL,
    updated_utc      TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

**Checkpoint lifecycle:**  
1. Projection agent starts → reads checkpoint.  
2. Reads events from `last_position + 1` to current.  
3. Processes events in order, applying the evolution function.  
4. After each batch, updates the checkpoint with the latest processed position and message ID.  

If a projection fails or restarts, it replays only events after its last checkpoint. The `last_message_id` enables idempotency: if an event with the same ID is replayed, it is skipped.

### 15.4 Idempotency  

Idempotency prevents duplicate event processing. The system uses a two‑tier approach:

| Tier | Implementation | Purpose |
|------|----------------|---------|
| **In‑memory** | `ConcurrentDictionary<Guid, bool>` in the projection agent | Fast duplicate detection within the same process lifetime |
| **Persistent** | PostgreSQL `idempotency` table | Survives restarts; enables cluster‑wide deduplication |

```sql
CREATE TABLE idempotency (
    message_id UUID PRIMARY KEY,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

Before processing any event, the handler checks the in‑memory cache, then the persistent store. After successful processing, it records the event’s `event_id` in both.

### 15.5 Projection Snapshots (Optional)  

For projections with very large event histories, replaying from the beginning on every restart can be slow. Snapshots capture the entire projection state at a known stream position.

```sql
CREATE TABLE snapshots (
    projection_name  TEXT   NOT NULL,
    stream_position  BIGINT NOT NULL,
    state_json       JSONB  NOT NULL,
    created_utc      TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (projection_name, stream_position)
);
```

**Snapshot lifecycle:**  
1. Periodically (every 10,000 events, or on demand), the projection agent serialises its full state and writes it to the `snapshots` table with the current `stream_position`.  
2. On restart, the agent loads the most recent snapshot, then replays only events after that position.  
3. Old snapshots are retained for audit but can be pruned by a background job.  

Snapshots are optional. The system is fully correct without them; they are purely a performance optimisation.

### 15.6 Data Migration — In‑Memory to PostgreSQL  

The MVP uses an in‑memory event store behind the same `Repository` and `EnvelopeStoreOps` interfaces that the production PostgreSQL implementation will use. The migration path is:

**Step 1 — Implement PostgreSQL Adapter**  
Build `PostgresRepository` and `PostgresEnvelopeStore` classes implementing the existing interfaces. These are drop‑in replacements.

**Step 2 — Dual‑Write Validation (Optional)**  
For a transition period, run both stores side‑by‑side: write to PostgreSQL while reading from in‑memory, or vice versa. Compare results to validate correctness.

**Step 3 — Cutover**  
Change the composition root in `Medhavi.Nexus` to inject the PostgreSQL implementations instead of in‑memory. No domain or application code changes.

**Step 4 — Historical Migration**  
If the MVP has accumulated in‑memory events that need to be preserved, run a one‑time migration script:

1. Read all events from the in‑memory store.  
2. Append them to PostgreSQL in the correct stream order.  
3. Set initial checkpoints for all projections to the latest stream positions.  

This script uses the same `EnvelopeStoreOps.Publish` interface, ensuring the same validation and enrichment are applied.

### 15.7 Multi‑Tenancy Isolation  

Multi‑tenancy is implemented through a `tenant_id` column on the `events` table and tenant‑aware projection agents.

**Event isolation:**  
Every event envelope carries `tenant_id` from the `ExecutionContext`. The `events` table includes a `tenant_id` column, indexed for efficient querying. Queries for a specific tenant filter by `tenant_id`.

**Projection isolation:**  
A projection agent can operate in two modes:

- **Dedicated mode** — one agent per tenant, each with its own in‑memory state and checkpoint. This is suitable for tenants with high data volumes or strict isolation requirements.  
- **Scoped mode** — a single agent maintains a `Map<TenantId, ProjectionState>`, filtering events by tenant. This is suitable for tenants with lower volumes.  

The mode is configured per projection, not hard‑coded.

**Checkpoint partitioning:**  
Checkpoints are keyed by `projection_name`. In dedicated mode, the projection name includes the tenant ID (e.g., `demand‑readmodel‑tenant‑abc`). In scoped mode, a single checkpoint tracks the global position, and the in‑memory state is partitioned.

**API and Store isolation:**  
The `ExecutionContext` carries `TenantId` through every command and query. The application layer ensures that data from one tenant is never returned to another. Stores in the UI are tenant‑scoped; switching tenants clears all Stores and reloads from the new tenant’s data.

### 15.8 Backup and Disaster Recovery  

The event store is the single source of truth. All other state (projections, snapshots) is derivable from events. The recovery strategy leverages this.

**Backup strategy:**  
- **Continuous archiving**: PostgreSQL WAL (Write‑Ahead Log) archiving to cloud object storage (S3, Azure Blob) provides point‑in‑time recovery.  
- **Daily logical backups**: `pg_dump` of the `events` table (without indexes) for an additional portable backup.  
- **Snapshot backups**: `snapshots` table can be backed up to speed up recovery, but it is not required.  

**Recovery process:**  
1. Restore the latest PostgreSQL backup (base backup + WAL to the desired point in time).  
2. Start the application. Projection agents will replay from their checkpoints.  
3. If a projection’s checkpoint is lost or corrupted, the projection replays from the beginning (or from the latest available snapshot).  
4. The system becomes fully consistent once all projections have caught up.  

**Recovery Time Objective (RTO):**  
- Database restore: < 1 hour (dependent on backup size and infrastructure).  
- Projection warm‑up: < 5 minutes for most projections (faster with snapshots).  

**Recovery Point Objective (RPO):**  
- Near‑zero with continuous WAL archiving (seconds to minutes of potential data loss).  

**DR testing:**  
Quarterly DR drills verify the recovery process. A script automates the restore, projection rebuild, and validation checks.

### 15.9 Data Retention  

The event store is append‑only and grows indefinitely. Retention policies are applied at the stream level:

| Stream Type | Retention | Rationale |
|-------------|-----------|-----------|
| Aggregate streams | Indefinite | Required for audit and full state reconstruction |
| Projection checkpoints | Latest only | Old checkpoint entries are superseded; only the current position is needed |
| Snapshots | Latest 3 per projection | Older snapshots are superseded by newer ones |
| Idempotency records | 30 days | After this window, duplicate events are extremely unlikely; entries can be pruned |

A background job runs weekly to prune expired idempotency records and old snapshots. Aggregate events are never pruned in the operational database; they may be archived to cold storage after a configurable period (default 7 years) for compliance.

### 15.10 Data Integrity and Validation  

**Immutability guarantee:**  
The `events` table is append‑only at the application level. No `UPDATE` or `DELETE` statements are issued against the `events` table by application code. Database permissions enforce this: the application’s database user has `INSERT` and `SELECT` privileges on `events`, but not `UPDATE` or `DELETE`.

**Hash‑chain integrity (future):**  
For tamper‑evident audit, an optional hash chain can be implemented: each event includes a SHA‑256 hash of the previous event in the stream. This is a future enhancement, not required for MVP.

**Validation on read:**  
When events are replayed, the application validates:  
- Event schema version is supported.  
- Required metadata fields (`correlationId`, `causationId`) are present.  
- Stream ordering is correct (no gaps in `stream_position`).  

### 15.11 In‑Memory Store for MVP  

The MVP uses an in‑memory implementation of the same `Repository` and `EnvelopeStoreOps` interfaces:

```fsharp
type InMemRepository<'Aggregate, 'Id, 'Event>() =
    let streams = ConcurrentDictionary<string, ('Event list * int64)>()

    member this.Load(id: string) = …
    member this.Append(id: string, expectedVersion: int64, events: 'Event list) = …
```

This enables rapid development and testing with zero external dependencies. The interface contracts are identical to the PostgreSQL implementations, guaranteeing a seamless transition.

The in‑memory store does not persist across restarts. For the MVP, this is acceptable; data is seeded at startup or via integration adapters.

---

# Chapter 16 — Configuration & Feature Management  

## 16.1 Configuration Philosophy  

Medhavi configuration is **typed, validated, and versioned**. There are no magic strings, no untyped dictionaries, and no runtime surprises from missing or malformed settings. Configuration is loaded at startup, validated against a schema, and exposed to the application as immutable F# records.

Configuration is divided into three categories:

| Category | Source | Examples |
|----------|--------|----------|
| **ARS Identifiers** | Compile‑time constants in F# | `CA‑DI‑002`, `BR‑DI‑027` |
| **Feature Flags** | Environment variables or JSON | AI autonomy level, planning mode availability |
| **Environment Settings** | Environment variables or JSON | Connection strings, timeouts, thresholds, circuit breaker parameters |

## 16.2 ARS Identifier Registry  

Every ARS identifier defined in the Intelligence Specifications is a compile‑time constant in the `Medhavi.SharedKernel.Configuration` project. This ensures they are safe from typos, searchable across the codebase, and validated at build time.

```fsharp
// Medhavi.SharedKernel.Configuration/ArsIdentifiers.fs
module Medhavi.SharedKernel.Configuration.ArsIdentifiers

module Demand =
    let domain = "DI"
    
    module Capabilities =
        let understandDemand = "CA‑DI‑001"
        let forecastDemand = "CA‑DI‑002"
        let senseDemand = "CA‑DI‑003"
        let segmentDemand = "CA‑DI‑004"
        let classifyDemand = "CA‑DI‑005"
        let prioritizeDemand = "CA‑DI‑006"
        let evaluateDemandQuality = "CA‑DI‑007"
        let detectDemandExceptions = "CA‑DI‑008"
        let explainDemand = "CA‑DI‑009"
        let learnFromDemand = "CA‑DI‑010"
    
    module Decisions =
        let acceptDemandSignal = "DE‑DI‑010"
        let adjustDemandHistory = "DE‑DI‑011"
        let selectChampionModel = "DE‑DI‑020"
        let generateBaselineForecast = "DE‑DI‑021"
        let publishForecast = "DE‑DI‑022"
        let overrideForecast = "DE‑DI‑023"
    
    module Rules =
        let signalTimeliness = "BR‑DI‑010"
        let signalRange = "BR‑DI‑011"
        let championSelectionSignificance = "BR‑DI‑020"
        let noHarmRule = "BR‑DI‑021"
        let highPriorityProtection = "BR‑DI‑022"
        let forecastValidity = "BR‑DI‑023"
        let dataSufficiency = "BR‑DI‑024"
        let overrideJustification = "BR‑DI‑027"
        let overrideDeviationLimit = "BR‑DI‑028"
    
    module Policies =
        let signalAcceptanceAutomation = "PO‑DI‑010"
        let championPromotionApproval = "PO‑DI‑020"
        let modelRollback = "PO‑DI‑021"
        let forecastAutoPublication = "PO‑DI‑023"
        let overrideAuthorization = "PO‑DI‑025"

// Similar modules exist for Supply (SI), Promise (PI), Scenario (SN), Knowledge (KN)
```

These constants are defined in the `Medhavi.SharedKernel.Configuration.ArsIdentifiers` module. They are used in `DecisionTrace` records, event envelope metadata, telemetry events, and AI explanations. They are the single source of truth for all ARS identifiers in the running system.

## 16.3 Feature Flags  

Feature flags enable safe, gradual rollout of new capabilities and control over AI autonomy levels. Environment settings are loaded into typed records at startup. Each bounded context and infrastructure component defines its own settings type, with validation on load.

```fsharp
// Medhavi.SharedKernel.Configuration/FeatureFlags.fs
type FeatureFlags = {
    // AI autonomy controls
    AiAutonomyEnabled: bool
    AiDefaultAutonomyLevel: string       // "Advisory" | "Guardrailed" | "Autonomous"
    AiPolicySuggestionEnabled: bool
    AiConversationalCopilotEnabled: bool
    
    // Planning mode controls
    FastInsertEnabled: bool
    IncrementalRepairEnabled: bool
    OptimizationEnabled: bool
    WhatIfSimulationEnabled: bool
    
    // Domain feature toggles
    SupplyCollaborationEnabled: bool
    KnowledgeIntelligenceEnabled: bool
    ScenarioComparisonEnabled: bool
    
    // Experimental features
    LlmIntegrationEnabled: bool
    AdvancedAnalyticsEnabled: bool
}
```

**Loading and defaults:**  
Flags are loaded from environment variables prefixed with `MEDHAVI_` (e.g., `MEDHAVI_AI_AUTONOMY_ENABLED=true`). Missing flags default to `false` for experimental features and `true` for core planning modes. A validation function checks that incompatible combinations are not enabled (e.g., `AiAutonomyEnabled = true` but `KnowledgeIntelligenceEnabled = false`).

**Usage in code:**  
```fsharp
if featureFlags.AiAutonomyEnabled then
    let contract = AutonomyContract.create agentId level allowedActions
    PolicyGate.registerAutonomyContract contract
```

## 16.4 Environment‑Specific Configuration  

Environment settings are loaded into typed records at startup. Each bounded context and infrastructure component defines its own settings type, with validation on load.

```fsharp
// Medhavi.SharedKernel/Configuration/EnvironmentSettings.fs
type EventStoreSettings = {
    ConnectionString: string
    MaxRetryCount: int
    RetryDelayMs: int
    CommandTimeoutSeconds: int
}

type CircuitBreakerSettings = {
    FailureThreshold: int
    RecoveryTimeoutSeconds: int
    MaxRecoveryTimeoutSeconds: int
    BackoffFactor: float
    SuccessThreshold: int
}

type PlanningEngineSettings = {
    SolverTimeLimitSeconds: int
    MaxIterations: int
    OptimalityGap: float
    ParallelWorkers: int
}

type ObservabilitySettings = {
    LogLevel: string
    MetricsEnabled: bool
    TracingEnabled: bool
    PrometheusPort: int option
}

type AppSettings = {
    EventStore: EventStoreSettings
    CircuitBreaker: CircuitBreakerSettings
    PlanningEngine: PlanningEngineSettings
    Observability: ObservabilitySettings
    FeatureFlags: FeatureFlags
}
```

**Loading:**  
Settings are loaded from `appsettings.json` (for development) and overridden by environment variables (for production). The ASP.NET Core configuration system is used, but settings are immediately bound to F# records and validated:

```fsharp
let loadSettings (config: IConfiguration) : Result<AppSettings, string> =
    validation {
        let! eventStore = config.GetSection("EventStore").Get<EventStoreSettings>() |> validateEventStore
        let! circuitBreaker = config.GetSection("CircuitBreaker").Get<CircuitBreakerSettings>() |> validateCircuitBreaker
        let! planning = config.GetSection("PlanningEngine").Get<PlanningEngineSettings>() |> validatePlanning
        let! observability = config.GetSection("Observability").Get<ObservabilitySettings>() |> validateObservability
        let! flags = loadFeatureFlags config
        return {
            EventStore = eventStore
            CircuitBreaker = circuitBreaker
            PlanningEngine = planning
            Observability = observability
            FeatureFlags = flags
        }
    }
```

Validation ensures that numeric values are within safe ranges (e.g., `FailureThreshold >= 1`, `RecoveryTimeoutSeconds >= 5`), connection strings are non‑empty, and ports are valid.

## 16.5 Configuration in the Composition Root  

All settings are loaded once at startup in `Medhavi.Nexus` (or `Medhavi.Hub`) and injected into bounded contexts as part of their environment. Bounded contexts never read configuration directly from files or environment variables. They receive their settings through typed records in their composition root.

```fsharp
// Medhavi.Nexus / Startup.fs
let settings = loadSettings configuration |> Result.defaultWith (fun err -> failwithf "Invalid configuration: %s" err)
// Settings are loaded from SharedKernel.Configuration module
```

let eventStore = PostgresEnvelopeStore.create settings.EventStore
let circuitBreakerConfig = settings.CircuitBreaker
let planningConfig = settings.PlanningEngine

let demandContext = Demand.BoundedContext.create demandEnv
let supplyContext = Supply.BoundedContext.create supplyEnv
// ...
```

### 16.6 Feature Flag Integration with PolicyGate  

Feature flags that control AI autonomy are integrated with `DecisionCore.PolicyGate`. When `AiAutonomyEnabled` is `false`, all `AutonomyContract` registrations are rejected at startup, and any AI agent attempting an action receives an `Advisory only` error. When enabled, the specific `AiDefaultAutonomyLevel` determines the maximum level any agent can operate at, regardless of its individual contract.

### 16.7 Configuration Versioning and Audit  

Configuration changes are versioned. The current configuration fingerprint (a hash of all settings) is recorded with every planning run and every AI decision. This enables traceability: if a decision was made under a specific configuration, that configuration can be precisely reproduced for audit.

```fsharp
let configFingerprint = DecisionCore.Fingerprints.hash settings
// Recorded in plan provenance and AI decision traces
```

---

# Chapter 17 — Security & Governance  

## 17.1 Security Architecture Overview  

Medhavi’s security model is built on three principles:

- **Least privilege** — every component, service, and user has only the permissions it needs.  
- **Defence in depth** — security is enforced at multiple layers: network, application, domain, and data.  
- **Auditability** — every action is traceable to an authenticated principal, recorded immutably, and available for review.  

Security is not a standalone module. It is embedded in the command pipeline, the event envelope, the API gateway, and the database access patterns.

## 17.2 Authentication  

Authentication is the responsibility of the API gateway (`Medhavi.Hub`) and is enforced before any request reaches a bounded context.

### 17.2.1 Human Authentication  

Human users authenticate via **OpenID Connect / OAuth 2.0**. The identity provider (IdP) can be Azure AD, Auth0, or any OpenID‑compatible provider. On successful authentication, the gateway receives an ID token and an access token.

The access token contains:
- `sub` — user identifier.  
- `preferred_username` — human‑readable name.  
- `roles` or `permissions` — Medhavi‑specific role claims.  
- `tenant_id` — for multi‑tenant deployments.  

The gateway validates the token signature, expiry, and issuer. It then populates the `ExecutionContext.Principal` with the authenticated user’s identifier and injects `TenantId` from the token.

### 17.2.2 AI Agent Authentication  

AI agents authenticate using **OAuth 2.0 Client Credentials** flow. Each agent has its own client ID and secret (or certificate). The token contains:

- `sub` — agent identifier (e.g., `demand‑forecast‑agent‑v2`).  
- `roles` — agent‑specific permissions (e.g., `ai:recommend`, `ai:execute‑guardrailed`).  

The gateway validates the token and populates `ExecutionContext.Principal` with the agent’s identifier and sets a flag indicating this is an AI agent. This flag is used by the `AutonomyContract` validation logic.

### 17.2.3 Service‑to‑Service Authentication  

In the current MVP monolith, bounded contexts communicate in‑process and do not require authentication. When contexts are extracted into separate services, service‑to‑service communication will use **mutual TLS (mTLS)** or OAuth 2.0 Client Credentials, depending on the deployment environment.

## 17.3 Authorisation  

Authorisation is the responsibility of the application layer in each bounded context. It is performed **after** authentication and **before** the domain decision function is called.

Authorisation is policy‑based. Every command that requires authorisation is associated with a `PolicyId` from the ARS registry. The application service evaluates the policy against the current `ExecutionContext`.

```fsharp
let authorize (ctx: ExecutionContext) (policyId: string) (command: Command) : Result<unit, AuthorizationError> =
    match ctx.Principal with
    | None -> Error (AuthorizationError "Unauthenticated")
    | Some principal ->
        match policyId with
        | ArsIdentifiers.Demand.Policies.overrideAuthorization ->
            // PO‑DI‑025: Only users in Demand Planner role may override forecasts
            if principal.HasRole("DemandPlanner") || principal.HasRole("DemandManager") then
                Ok ()
            else
                Error (AuthorizationError "Forecast override requires Demand Planner role")
        | _ -> Ok () // Default: allow authenticated users
```

The authorisation function is pure. It takes the `ExecutionContext` and a `PolicyId`, and returns a result. It can be tested exhaustively.

### 17.3.1 Role‑Based Access Control (RBAC)  

Medhavi uses a simple RBAC model:

| Role | Permissions |
|------|-------------|
| **Demand Planner** | Create and modify demand plans, override forecasts, run demand quality reports |
| **Supply Planner** | Create and modify supply plans, manage inventory policies, schedule production |
| **Promise Manager** | Configure promising rules, manage allocations, approve promise overrides |
| **Scenario Manager** | Create and run scenarios, compare plans, recommend strategies |
| **Knowledge Manager** | Govern the knowledge graph, approve best practices, manage improvement portfolio |
| **Administrator** | Full access, including configuration changes and autonomy contract management |
| **Viewer** | Read‑only access to all data |

Roles are carried in the access token claims. They are mapped to specific ARS policy IDs in the authorisation function.

### 17.3.2 AI Agent Authorisation  

AI agents are authorised based on their `AutonomyContract`. The `DecisionCore.Autonomy.validateAction` function determines whether the agent is permitted to execute a specific action. This is separate from human role‑based authorisation and is described fully in Chapter 10.

## 17.4 API Versioning  

API versioning ensures backward compatibility as bounded contexts evolve. Medhavi uses **URL‑based versioning**:

```
/api/v1/demand/forecasts
/api/v2/demand/forecasts
```

**Versioning policy:**  
- **Major version** (v1, v2): breaking changes to request or response schemas.  
- **Minor version**: non‑breaking additions (new optional fields, new endpoints). Minor versions are not reflected in the URL; they are indicated in response headers (`X‑API‑Version: 1.2`).  

**Backward compatibility rules:**  
- A new major version must coexist with the previous major version for at least one release cycle.  
- Deprecated versions are announced in the API response headers (`Deprecation: true`, `Sunset: <date>`).  
- Clients (including the UI) should specify the API version they expect via the `Accept‑Version` header; if not specified, the latest stable version is used.  

**Contract testing** ensures that a new version does not accidentally break existing consumers. The `Medhavi.Contracts` project serves as the schema source of truth. Contract tests compare generated OpenAPI specs against known client expectations.

## 17.5 Audit Logging  

Audit logging is a first‑class architectural requirement derived from the ARS traceability mandate. Every significant action is recorded immutably in the event store.

### 17.5.1 What is Audited  

| Event | Recorded Data |
|-------|---------------|
| **Command execution** | `CorrelationId`, `Principal`, command type, ARS decision ID, timestamp, result (success/failure) |
| **AI recommendation** | `CorrelationId`, agent ID, autonomy contract ID, proposed action, `PolicyGate` result, acceptance/rejection by human |
| **Configuration change** | `Principal`, previous configuration fingerprint, new configuration fingerprint, timestamp |
| **Policy change** | `Principal`, policy ID, old parameters, new parameters, `PolicyGate` result |
| **Data access** | `Principal`, query type, filters applied, timestamp (for sensitive queries) |
| **Authentication event** | `Principal`, login timestamp, IP address, success/failure |

### 17.5.2 Audit Trail Implementation  

The audit trail is the `events` table itself. Every audited action is appended as an event to a dedicated audit stream (`$audit-{domain}`). The `Envelope` metadata carries the `Principal`, `CorrelationId`, and the action details.

For query‑only actions (which do not produce domain events), an explicit `AuditEvent` is created:

```fsharp
type AuditEvent =
    | CommandExecuted of commandType: string * decisionId: string * result: string
    | AiRecommendationMade of agentId: string * action: string * accepted: bool
    | ConfigurationChanged of previousFingerprint: string * newFingerprint: string
    | PolicyChanged of policyId: string * oldParams: string * newParams: string
```

These events are appended to the audit stream and are queryable by administrators.

### 17.5.3 Replay Safety  

Events can be replayed for debugging, testing, or recovery. Every replay is itself audited:

- `ReplayInitiated` event records who initiated the replay, under what ticket, and why.  
- Replayed events carry a `replayOrigin` metadata field so that projections can distinguish live events from replayed events.  
- RBAC controls who can initiate a replay (Administrator role only).  

## 17.6 Data Protection  

Data protection is applied at multiple layers:

| Layer | Protection |
|-------|------------|
| **Transit** | TLS 1.3 for all external communication (HTTPS, gRPC). mTLS for service‑to‑service communication in production. |
| **At rest** | PostgreSQL encryption at rest (enabled at the database or disk level, depending on the deployment environment). |
| **Application** | PII masking at the integration boundary. Sensitive fields (customer names, email addresses) are masked or tokenised in event data if required by policy. |
| **Secrets** | Connection strings, API keys, and certificates are stored in a secrets manager (Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault). They are never stored in configuration files or committed to source control. |

## 17.7 API Security  

All external APIs are protected by the following controls:

- **HTTPS only** — HTTP requests are redirected to HTTPS.  
- **OAuth 2.0 bearer tokens** — all endpoints require a valid access token.  
- **CORS policy** — the gateway restricts cross‑origin requests to known UI origins.  
- **Rate limiting** — per‑user and per‑agent rate limits prevent abuse. Rate limit headers are returned in responses.  
- **Input validation** — all inputs are validated by the ACL before reaching the domain.  

## 17.8 Governance  

Governance is the set of processes and controls that ensure the system remains compliant, auditable, and aligned with enterprise policies.

### 17.8.1 PolicyGate Governance  

The `PolicyGate` (DecisionCore, Section 7.3.5) is the primary governance mechanism for both human and AI actions. It validates:

- **Safety constraints** — minimum safety stock, frozen horizon protection, firm order protection.  
- **Policy delta limits** — maximum allowed change to any planning parameter.  
- **Approval requirements** — risky changes flagged for human approval.  

No policy change—whether from a human planner, an administrator, or an AI agent—can take effect without passing through the `PolicyGate`.

### 17.8.2 Change Management  

Configuration changes that affect system behaviour (feature flags, environment settings, autonomy contracts) follow a standard change management process:

1. Change proposed in a staging environment.  
2. Impact analysis (automated where possible: what planning runs would be affected?).  
3. Approval by the appropriate role (Administrator for configuration, Knowledge Manager for best practices, Domain Manager for domain‑specific policies).  
4. Deployment with rollback plan.  

All changes are recorded in the audit trail with the previous and new configuration fingerprints.

### 17.8.3 Compliance Reporting  

The audit trail supports automated compliance reporting:

- **Access reviews** — who accessed what data, when, and from where.  
- **Change logs** — all configuration and policy changes with approver details.  
- **AI decision logs** — every AI recommendation, its validation result, and whether it was accepted or rejected.  

These reports are generated from the event store using standard SQL queries. They can be scheduled and delivered to compliance officers.

---



# Chapter 18 — Deployment & Evolution Path  

## 18.1 The Evolution Strategy  

Medhavi is designed to evolve gracefully from a single‑process monolith to a distributed, independently deployable service mesh. The key principle is that **business logic never changes during this evolution**—only deployment topology, infrastructure, and configuration change.

The evolution is staged so that each stage delivers value and can be sustained indefinitely. There is no forced migration; each stage is optional until its benefits outweigh its costs.

## 18.2 Stage 1 — Modular Monolith (Current MVP)  

All bounded contexts run in a single process within `Medhavi.Nexus`. Communication is in‑process via `DomainEventBus`. Persistence is in‑memory (backed by event sourcing abstractions). The UI is served from the same process.

```
┌──────────────────────────────────────────────┐
│              Medhavi.Hub (ASP.NET Core)       │
│                    Port 5000                   │
│                                               │
│  ┌─────────────────────────────────────────┐  │
│  │          Medhavi.Nexus                   │  │
│  │                                          │  │
│  │  Demand  Supply  Promise  Scenario       │  │
│  │  Knowledge  MasterData  Integration      │  │
│  │  PlanningEngine                          │  │
│  │                                          │  │
│  │  DomainEventBus (in‑process)             │  │
│  │  In‑memory Repository                    │  │
│  └─────────────────────────────────────────┘  │
│                                               │
│  PostgreSQL (optional, for persistence)       │
└──────────────────────────────────────────────┘
```

**Characteristics:**  
- Single deployable unit.  
- Zero network latency between bounded contexts.  
- Simplest operational model: one process to start, monitor, and debug.  
- In‑memory store enables fast iteration without database setup.  

**When to stay here:**  
- Development and early testing.  
- Small‑scale deployments with low event volume.  
- Demonstrations and proofs of concept.  

## 18.3 Stage 2 — Persistent Event Store  

The in‑memory repository is replaced with the PostgreSQL `events` table. The same `Repository` and `EnvelopeStoreOps` interfaces are used; only the implementation changes.

```
┌──────────────────────────────────────────────┐
│              Medhavi.Hub                      │
│                                               │
│  ┌─────────────────────────────────────────┐  │
│  │          Medhavi.Nexus                   │  │
│  │  (same bounded contexts, same logic)     │  │
│  │                                          │  │
│  │  DomainEventBus (in‑process)             │  │
│  │  PostgresRepository                      │  │
│  └─────────────────────────────────────────┘  │
│                                               │
│  ┌─────────────────────────────────────────┐  │
│  │         PostgreSQL                       │  │
│  │  events │ checkpoints │ snapshots        │  │
│  └─────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘
```

**Changes from Stage 1:**  
- `InMemRepository` → `PostgresRepository` in the composition root.  
- PostgreSQL instance added (single instance, no clustering required yet).  
- Event durability and restart survival achieved.  

**When to move here:**  
- Before any production deployment.  
- When event history needs to survive process restarts.  
- When audit trail is required.  

## 18.4 Stage 3 — External Event Bus  

The in‑process `DomainEventBus` is replaced with a PostgreSQL `LISTEN`/`NOTIFY` implementation. Bounded contexts still run in the same process but communicate through the database, enabling future extraction.

```
┌──────────────────────────────────────────────┐
│              Medhavi.Hub                      │
│                                               │
│  ┌─────────────────────────────────────────┐  │
│  │          Medhavi.Nexus                   │  │
│  │  (same bounded contexts)                 │  │
│  │                                          │  │
│  │  Event Bus Adapter (Pg LISTEN/NOTIFY)    │  │
│  │  PostgresRepository                      │  │
│  └─────────────────────────────────────────┘  │
│                                               │
│  ┌─────────────────────────────────────────┐  │
│  │         PostgreSQL                       │  │
│  │  events │ checkpoints │ NOTIFY channel   │  │
│  └─────────────────────────────────────────┘  │
└──────────────────────────────────────────────┘
```

**Changes from Stage 2:**  
- `DomainEventBus` implementation changed to PostgreSQL‑backed.  
- No code changes in bounded contexts.  
- Bounded contexts are technically ready for extraction (they already communicate via the database, not shared memory).  

**When to move here:**  
- When multiple instances of the monolith are needed for scaling or high availability.  
- Before extracting any bounded context into its own service.  

## 18.5 Stage 4 — Extracted Services  

Individual bounded contexts are extracted into independently deployable services. Each service has its own process, its own database connection, and its own API surface. Communication between services uses the same PostgreSQL‑backed event bus (or RabbitMQ/Kafka if higher throughput is needed).

```
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────┐
│  Demand  │ │  Supply  │ │ Promise  │ │   Scenario   │
│  Service │ │  Service │ │ Service  │ │   Service    │
│  Port 1  │ │  Port 2  │ │  Port 3  │ │    Port 4    │
└────┬─────┘ └────┬─────┘ └────┬─────┘ └──────┬───────┘
     │            │            │               │
     └────────────┼────────────┴───────────────┘
                  │
        ┌─────────▼─────────┐
        │    Event Bus      │
        │ (PostgreSQL /     │
        │  RabbitMQ / Kafka)│
        └─────────┬─────────┘
                  │
        ┌─────────▼─────────┐
        │   Knowledge       │
        │   Service         │
        │   Port 5          │
        └───────────────────┘
                  │
        ┌─────────▼─────────┐
        │   Medhavi.Hub     │
        │   (API Gateway)   │
        │   Port 5000       │
        └───────────────────┘
```

**Changes from Stage 3:**  
- Each bounded context gets its own project, its own `Program.fs`, and its own deployment unit.  
- `Medhavi.Nexus` is retired or reduced to a thin composition helper for local development.  
- An API gateway (e.g., YARP, NGINX, or Azure API Management) routes external requests to the appropriate service.  
- Service discovery and health checks are managed by the container orchestrator.  

**When to move here:**  
- When independent scaling is required (e.g., Promise needs more instances than Demand).  
- When independent deployment cycles are needed (different teams own different contexts).  
- When fault isolation is critical (a failure in one context must not affect others).  

## 18.6 Stage 5 — Containerised Cluster  

All services are containerised and deployed on Kubernetes. Auto‑scaling, rolling updates, and infrastructure‑as‑code are fully adopted.

```
┌──────────────────────────────────────────────────────────────┐
│                    Kubernetes Cluster                         │
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │
│  │ Demand Service │  │ Supply Service │  │ Promise Service│  │
│  │  (3 replicas)  │  │  (3 replicas)  │  │  (5 replicas)  │  │
│  └────────────────┘  └────────────────┘  └────────────────┘  │
│                                                              │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  │
│  │Scenario Service│  │Knowledge Svc   │  │ Medhavi.Hub    │  │
│  │  (2 replicas)  │  │  (2 replicas)  │  │  (2 replicas)  │  │
│  └────────────────┘  └────────────────┘  └────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │              PostgreSQL (HA Cluster)                    │  │
│  │              RabbitMQ / Kafka                          │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

**Characteristics:**  
- Auto‑scaling based on CPU, memory, and custom metrics (event processing lag, command queue depth).  
- Rolling updates with zero downtime.  
- Infrastructure defined as code (Terraform, Helm, or similar).  
- Centralised logging (Elasticsearch / Splunk), metrics (Prometheus), and tracing (Jaeger / Zipkin).  

## 18.7 Evolution Decision Matrix  

Not every bounded context needs to be extracted. The decision to move from Stage 3 to Stage 4 for a specific context depends on:

| Factor | Stay Monolith | Extract Service |
|--------|---------------|-----------------|
| **Scaling needs** | All contexts scale together | Context needs independent scaling |
| **Deployment frequency** | Same release cycle for all | Independent release cycles |
| **Team ownership** | Single team | Multiple teams |
| **Fault isolation** | Failure in one context is acceptable for all | Strict fault isolation required |
| **Data volume** | Low to moderate | High event volume in specific context |

The architecture supports a **mixed mode**: some contexts extracted as services, others remaining in a shared process. The event bus abstraction makes the physical deployment transparent to the business logic.

## 18.8 Deployment Environments  

| Environment | Purpose | Database | Event Bus | Scaling |
|-------------|---------|----------|-----------|---------|
| **Development** | Local development | In‑memory (Stage 1) | In‑process | Single instance |
| **CI/Test** | Automated testing | PostgreSQL (Testcontainers) | In‑process | Single instance |
| **Staging** | Pre‑production validation | PostgreSQL (Stage 2/3) | LISTEN/NOTIFY | Single instance, production‑like config |
| **Production** | Live system | PostgreSQL HA (Stage 4/5) | RabbitMQ or Kafka | Auto‑scaled |

## 18.9 Rollback Strategy  

Every deployment must be reversible. The evolution path supports rollback at every stage:

- **Stage 1 → 2**: If PostgreSQL is unavailable, revert to in‑memory store by changing one configuration flag. Events accumulated in PostgreSQL are retained but not lost (they can be replayed later).  
- **Stage 2 → 3**: If the PostgreSQL `LISTEN`/`NOTIFY` bus is problematic, revert to in‑process bus. No events are lost; they remain in the `events` table.  
- **Stage 3 → 4**: If an extracted service is unstable, route its traffic back to the monolith. The monolith still has the full event bus subscription and can process all events.  

Rollback is a configuration change, not a code change.

---

# Chapter 19 — Testing Strategy  

## 19.1 Testing Philosophy  

Medhavi’s testing strategy is built on three principles:  

- **Test the right thing at the right level.** Business logic is tested with fast, isolated unit tests. Integration between components is tested with realistic infrastructure. End‑to‑end flows are tested through the public API.  
- **Pure functions are testable by construction.** Domain logic, decision functions, rules, projection evolution functions, and ACL translations are all pure. They require no mocking, no setup, and no teardown.  
- **The event store is the ultimate integration point.** Any test that writes events and reads them back validates the full command‑to‑query cycle.  

Every bounded context follows the same testing patterns. The test suite is written in F# using **Expecto** as the test framework, **FsCheck** for property‑based testing, and **Testcontainers** for PostgreSQL integration tests.

## 19.2 Unit Tests — Domain Logic  

Domain unit tests verify that decision functions, validation rules, and evolution functions behave correctly in isolation. These tests are fast, deterministic, and run in memory with no external dependencies.

### 19.2.1 Decision Function Tests  

Every decision function is tested with valid inputs, invalid inputs, and edge cases.  

```fsharp
testCase "Override forecast succeeds with valid justification" <| fun () ->
    let forecast = createTestForecast (mean = 100m)
    let cmd = { ForecastId = forecast.Id; NewValue = 120m; Justification = "Customer confirmed increase" }
    let result = ForecastDecisions.overrideForecast forecast cmd.NewValue cmd.Justification
    Expect.isOk result "Override should succeed with valid justification"

testCase "Override forecast fails without justification" <| fun () ->
    let forecast = createTestForecast (mean = 100m)
    let cmd = { ForecastId = forecast.Id; NewValue = 120m; Justification = "" }
    let result = ForecastDecisions.overrideForecast forecast cmd.NewValue cmd.Justification
    Expect.isError result "Override should fail without justification"
    match result with
    | Error (DomainError.ValidationError(_, msg, _)) ->
        Expect.stringContains msg "justification" "Error should mention justification"
    | _ -> failwith "Expected validation error"
```

### 19.2.2 Rule Validation Tests  

Each rule from the Intelligence Specifications is tested independently. The ARS rule identifier is included in the test name for traceability.  

```fsharp
testCase "BR-DI-028: Override exceeds 50% deviation limit" <| fun () ->
    let forecast = createTestForecast (mean = 100m)
    let result = ForecastDecisions.overrideForecast forecast 160m "Valid reason"
    Expect.isError result "Override exceeding 50% should be rejected"
```

### 19.2.3 Projection Evolution Tests  

Projection evolution functions are tested by applying sequences of events and verifying the resulting read model.  

```fsharp
testCase "Demand projection: ingest then promise updates correctly" <| fun () ->
    let state = Map.empty
    let state = evolveDemandReadModel state (DemandLineIngested testDemand)
    let state = evolveDemandReadModel state (DemandLinePromised { DemandLineId = testDemand.Id; PromisedDate = testDate; ConfirmedQty = 50m })
    let dto = state |> Map.find testDemand.Id
    Expect.equal dto.ConfirmedQty 50m "Confirmed quantity should be updated"
```

### 19.2.4 Property‑Based Testing with FsCheck  

For functions with complex input spaces, property‑based tests verify invariants across randomly generated inputs.  

```fsharp
testProperty "Safety stock calculation is never negative" <| fun (demand: NormalFloat) (leadTime: PositiveInt) ->
    let z = 1.65  // 95% service level
    let sigma = max 0.01 (abs (float demand))
    let lt = float leadTime.Get
    let ss = DecisionCore.Inventory.safetyStock z sigma lt
    Expect.isGreaterThanOrEqual ss 0.0m "Safety stock must be non-negative"
```

## 19.3 Integration Tests — Command to Event to Projection  

Integration tests verify that the full command‑to‑query cycle works correctly. They use the real repository, real application services, and real projection agents, but with an in‑memory event store for speed.

```fsharp
testCase "Full demand ingest → query cycle" <| fun () ->
    task {
        // Arrange
        let repo = InMemRepository.create<DemandLine, string, DemandEvent>()
        let agent = ProjectionAgent(evolveDemandReadModel, Map.empty, "Test")
        let capabilities = createDemandCapabilities repo
        DomainEventBus.Subscribe<DemandEvent>(fun ev -> agent.Post(ev, Guid.NewGuid()))
        
        // Act
        let cmd = { DemandLineId = "DL-001"; SkuId = "SKU-123"; Quantity = 100m; ... }
        let! result = capabilities.Ingest cmd
        
        // Assert
        Expect.isOk result "Ingest should succeed"
        let! dto = agent.QueryAsync(fun state -> state |> Map.tryFind "DL-001")
        Expect.isSome dto "Projection should contain the demand line"
    }
```

### 19.3.1 PostgreSQL Integration Tests  

Tests that require the real PostgreSQL event store use **Testcontainers**. A fresh PostgreSQL container is spun up for each test run, ensuring isolation and repeatability.

```fsharp
let postgresContainer = PostgreSqlBuilder()
    .WithImage("postgres:16")
    .WithDatabase("medhavi_test")
    .Build()

testCase "PostgreSQL event store append and read" <| fun () ->
    task {
        do! postgresContainer.StartAsync()
        let store = PostgresEnvelopeStore.create postgresContainer.ConnectionString
        let envelope = Envelope.createEnvelope "TestEvent" "{\"value\": 42}" 1
        
        let! appendResult = store.Publish "test-stream" [envelope] ExpectedRevision.NoStream CancellationToken.None
        Expect.isOk appendResult "Append should succeed"
        
        let! readResult = store.ReadStream "test-stream" None (Some 10) CancellationToken.None
        Expect.isOk readResult "Read should succeed"
        match readResult with
        | Ok events -> Expect.hasLength events 1 "Should read exactly one event"
        | Error e -> failwithf "Unexpected error: %A" e
    }
```

## 19.4 End‑to‑End Tests  

End‑to‑end tests verify complete business scenarios through the public API. They run against a fully wired‑up test host that includes all bounded contexts, the event bus, and (optionally) PostgreSQL.

```fsharp
testCase "Order promising scenario: create demand → promise → verify" <| fun () ->
    task {
        // Arrange
        use host = TestHost.createWithInMemoryStore()
        let client = host.GetClient()
        
        // Act — create demand
        let demandReq = { DemandLineId = "DL-E2E-001"; SkuId = "SKU-001"; Quantity = 100m; ... }
        let! demandResponse = client.PostAsync("/api/v1/demand", serialize demandReq)
        Expect.equal demandResponse.StatusCode HttpStatusCode.OK "Demand creation should succeed"
        
        // Act — promise the demand
        let promiseReq = { DemandLineId = "DL-E2E-001"; PromisedDate = DateTime.UtcNow.AddDays(3); ConfirmedQty = 100m }
        let! promiseResponse = client.PostAsync("/api/v1/promise", serialize promiseReq)
        Expect.equal promiseResponse.StatusCode HttpStatusCode.OK "Promise should succeed"
        
        // Assert — verify promise is visible
        let! getResponse = client.GetAsync("/api/v1/promises/DL-E2E-001")
        let promise = deserialize<PromiseDto> getResponse.Content
        Expect.equal promise.Status "Promised" "Promise status should be Promised"
    }
```

## 19.5 Contract Testing  

Contract tests ensure that event schemas and API contracts remain backward‑compatible. They verify that producers and consumers agree on the shape of data.

### 19.5.1 Event Schema Contract Tests  

```fsharp
testCase "DemandLineIngested event is backward compatible with v1 consumers" <| fun () ->
    let v1Event = """{"DemandLineId":"DL-001","SkuId":"SKU-123","Quantity":100}"""
    let result = Envelope.deserialize<DemandLineEvent> (Envelope.createEnvelope "DemandLineIngested" v1Event 1)
    Expect.isOk result "V1 event should deserialize successfully"
```

### 19.5.2 API Contract Tests  

OpenAPI specs are generated from the `Medhavi.Contracts` types and validated against known client expectations. The test fails if a breaking change is introduced.

## 19.6 AI Behaviour Testing  

AI‑specific tests verify that autonomy contracts, PolicyGate validation, and AI recommendation flows work correctly.

### 19.6.1 Autonomy Contract Validation Tests  

```fsharp
testCase "Guardrailed agent can execute permitted action" <| fun () ->
    let contract = AutonomyContract.create "agent-1" AutonomyLevel.Guardrailed ["OverrideForecast"] 0.5m
    let result = DecisionCore.Autonomy.validateAction contract "OverrideForecast" 100m
    Expect.isOk result "Permitted action should be allowed"

testCase "Guardrailed agent cannot execute disallowed action" <| fun () ->
    let contract = AutonomyContract.create "agent-1" AutonomyLevel.Guardrailed ["OverrideForecast"] 0.5m
    let result = DecisionCore.Autonomy.validateAction contract "PublishForecast" 100m
    Expect.isError result "Disallowed action should be rejected"

testCase "Advisory agent cannot execute any action" <| fun () ->
    let contract = AutonomyContract.create "agent-2" AutonomyLevel.Advisory ["OverrideForecast"] 0.5m
    let result = DecisionCore.Autonomy.validateAction contract "OverrideForecast" 100m
    Expect.isError result "Advisory agent should not execute actions"
```

### 19.6.2 PolicyGate Tests  

```fsharp
testCase "PolicyGate rejects safety stock below minimum" <| fun () ->
    let current = createDefaultPolicy()
    let proposed = { current with MinSafetyStock = -10m }
    let result = DecisionCore.PolicyGate.validatePolicy current proposed
    match result with
    | PolicyGateResult.Rejected reasons ->
        Expect.stringContains (String.concat "; " reasons) "safety stock" "Should mention safety stock violation"
    | _ -> failwith "Expected rejection"
```

### 19.6.3 AI Agent Simulation Tests  

Simulation tests replay historical scenarios through AI agents and verify that their recommendations are consistent, safe, and explainable.

```fsharp
testCase "AI forecast agent produces explainable override recommendations" <| fun () ->
    task {
        let agent = createTestAiAgent "forecast-agent" AutonomyLevel.Guardrailed
        let context = createTestContextWithHistory()
        
        let! recommendation = agent.GenerateForecastOverride context
        Expect.isSome recommendation "Agent should produce a recommendation"
        
        let explanation = recommendation.Value.Explanation
        Expect.stringContains explanation "BR-DI-027" "Explanation should reference the justification rule"
        Expect.stringContains explanation "CorrelationId" "Explanation should be traceable"
    }
```

## 19.7 Performance Tests  

Performance tests verify that critical paths meet their latency and throughput targets.

| Scenario | Target | Test Method |
|----------|--------|-------------|
| ATP check (single item) | < 50ms p99 | BenchmarkDotNet |
| Forecast generation (100 SKUs) | < 5 seconds | Integration test with timer |
| Event append (batch of 10) | < 20ms | Integration test with timer |
| Projection catch‑up (1,000 events) | < 1 second | Integration test with timer |

Performance tests are run in CI on every commit that modifies the critical path. Regressions are flagged before merge.

## 19.8 Test Data Management  

Tests use a combination of:

- **Hard‑coded test data** for simple unit tests.  
- **FsCheck generators** for property‑based tests.  
- **Test fixture builders** for integration tests that need realistic aggregate states.

```fsharp
type DemandLineBuilder() =
    member _.WithSku(sku: string) = ...
    member _.WithQuantity(qty: decimal) = ...
    member _.Build() = ...

let aDemandLine = DemandLineBuilder().WithSku("SKU-123").WithQuantity(100m).Build()
```

All test data is self‑contained within each test. Tests do not depend on shared state or a pre‑seeded database.

## 19.9 Continuous Integration  

Tests are organised into tiers for efficient CI execution:

| Tier | Tests | Execution Time | Runs On |
|------|-------|---------------|---------|
| **Tier 0** | Unit tests (domain, projections, ACL) | < 10 seconds | Every commit |
| **Tier 1** | Integration tests (in‑memory, full command‑query cycle) | < 2 minutes | Every push |
| **Tier 2** | Integration tests (PostgreSQL, Testcontainers) | < 5 minutes | Every push to main, PR validation |
| **Tier 3** | End‑to‑end tests, contract tests, AI simulation tests | < 15 minutes | Nightly, pre‑release |
| **Tier 4** | Performance tests | < 30 minutes | Weekly, pre‑release |

Tiers 0 and 1 must pass before a PR can be merged. Tiers 2‑4 are advisory but failures block releases.

---

# Appendices  

## Appendix A — DecisionCore Library Reference  

### A.1 Overview  

`DecisionCore` is a pure F# library with no dependencies. It contains the shared decision semantics that must be identical across all bounded contexts. Every function is deterministic, side‑effect‑free, and independently testable.

### A.2 Module: Scoring  

Provides shared plan score computation and candidate ranking.

| Function | Signature | Purpose |
|----------|-----------|---------|
| `emptyScore` | `unit -> PlanScore` | Creates a zero‑value score |
| `combineScores` | `PlanScore -> PlanScore -> PlanScore` | Sums two scores component‑wise |
| `weightedObjectiveScore` | `PlanScore -> ScoreWeights -> float` | Computes weighted single‑objective value |
| `candidateRanking` | `PlanScore list -> ScoreWeights -> PlanScoreCard list` | Ranks variants by weighted score |
| `cardComparison` | `PlanScoreCard -> PlanScoreCard -> int` | Compares two cards (‑1, 0, 1) |

**Types:**

```fsharp
type PlanScore = {
    TotalCost: decimal
    ServiceLevel: float
    CapacityUtilization: float
    LatenessPenalty: decimal
    RiskScore: float
}

type ScoreWeights = {
    CostWeight: float
    ServiceWeight: float
    CapacityWeight: float
    RiskWeight: float
}

type PlanScoreCard = {
    VariantId: string
    Score: PlanScore
    WeightedTotal: float
    Rank: int
}
```

### A.3 Module: Feasibility  

Pure feasibility contracts used by Promise (ATP/CTP) and Supply (plan validation).

| Function | Purpose |
|----------|---------|
| `checkATP` | Evaluates Available‑to‑Promise against uncommitted supply |
| `checkCTP` | Evaluates Capable‑to‑Promise against capacity and materials |
| `composeFeasibility` | Combines multiple feasibility results into one |
| `determineAcceptability` | Returns true if the feasibility result meets thresholds |

**Types:**

```fsharp
type FeasibilityResult =
    | Feasible of earliestDate: DateTimeOffset * confidence: float
    | PartiallyFeasible of quantity: decimal * date: DateTimeOffset
    | Infeasible of reason: Limiter list

type Limiter = {
    Domain: LimiterDomain
    Severity: LimiterSeverity
    Code: string
    Message: string
}
```

### A.4 Module: Reservations  

Shared reservation semantics for Promise and PlanningEngine.

| Function | Purpose |
|----------|---------|
| `createTentative` | Creates a new tentative reservation |
| `confirm` | Confirms a tentative reservation (consumes supply) |
| `release` | Releases a tentative reservation (returns supply) |
| `expire` | Expires a reservation that exceeded its TTL |
| `reduce` | Reduces the quantity of a reservation |
| `validateLifecycle` | Checks that a state transition is valid |

**Types:**

```fsharp
type Reservation = {
    ReservationId: Guid
    Scope: ReservationScope           // Atp | Ctp | Allocation | Planned
    Status: ReservationStatus         // Tentative | Confirmed | Released | Expired
    SkuId: string
    Quantity: decimal
    Source: string
    CreatedAt: DateTimeOffset
    ExpiresAt: DateTimeOffset option
}
```

### A.5 Module: Fingerprints  

Content‑addressed identifiers for planning artifacts, used by Knowledge Intelligence for cross‑domain correlation.

| Function | Purpose |
|----------|---------|
| `ofSnapshot` | Generates fingerprint from a planning snapshot |
| `ofPolicy` | Generates fingerprint from a policy set |
| `ofPlan` | Generates fingerprint from a plan version |
| `ofGraph` | Generates fingerprint from a planning graph |
| `hash` | Generic content hash for any serialisable object |

**Types:**

```fsharp
type SnapshotFingerprint = SnapshotFingerprint of string
type PolicyFingerprint = PolicyFingerprint of string
type PlanFingerprint = PlanFingerprint of string
type GraphFingerprint = GraphFingerprint of string
```

### A.6 Module: PolicyGate  

Pure validation gate that ensures no policy change violates safety boundaries.

| Function | Purpose |
|----------|---------|
| `validatePolicy` | Validates a proposed policy set against the current one |

**Types:**

```fsharp
type PolicyGateResult =
    | Valid
    | ValidWithWarnings of string list
    | Rejected of string list
```

**Checks performed:**
- Maximum solver time within bounds
- Minimum safety stock not negative
- Maximum safety stock not excessive
- Frozen horizon protected
- Firm orders protected
- Hard constraints preserved
- Maximum objective weight shift not exceeded
- Approval requirements for risky changes

### A.7 Module: Autonomy  

Formal contracts defining what AI agents are permitted to do.

| Function | Purpose |
|----------|---------|
| `createContract` | Creates a new autonomy contract |
| `validateAction` | Checks if an action is permitted by the contract |
| `isWithinBoundary` | Checks if a proposed change is within delta limits |
| `expireContract` | Marks a contract as expired |

**Types:**

```fsharp
type AutonomyLevel = Advisory | Guardrailed | Autonomous

type AutonomyContract = {
    ContractId: string
    AgentId: string
    Level: AutonomyLevel
    Domain: string
    AllowedActions: string list
    MaxPolicyDelta: float
    MaxValueThreshold: decimal option
    RollbackRules: string
    ApprovalRequiredAbove: decimal option
    ExpiresAt: DateTimeOffset
}
```

### A.8 Module: TimeWindows  

Pure UTC time‑window math used across all planning modes.

| Function | Purpose |
|----------|---------|
| `overlap` | Checks if two time windows overlap |
| `contains` | Checks if one window fully contains another |
| `intersection` | Computes the intersection of two windows |
| `expand` | Expands a window by a duration |
| `shift` | Shifts a window forward or backward |
| `slack` | Computes the gap between two windows |
| `bucketAlign` | Aligns a timestamp to a bucket boundary |
| `leadTimeOffset` | Offsets a window by a lead time |

### A.9 Module: PlanningGraph  

Immutable planning graph model used for indexing and traversal.

| Function | Purpose |
|----------|---------|
| `empty` | Creates an empty graph |
| `addNode` | Adds a node to the graph |
| `addEdge` | Adds an edge between nodes |
| `applyDelta` | Applies a graph delta to the current graph |
| `indexByNode` | Creates a lookup index by node ID |
| `indexByEdge` | Creates a lookup index by edge type |

**Types:**

```fsharp
type PlanningNode =
    | MaterialNode of id: string * skuId: string * quantity: decimal
    | InventoryNode of id: string * locationId: string * onHand: decimal
    | DemandNode of id: string * demandId: string * quantity: decimal
    | SupplyNode of id: string * orderId: string * quantity: decimal
    | CapacityNode of id: string * resourceId: string * hours: decimal
    | TransportNode of id: string * laneId: string * cost: decimal
    | OperationNode of id: string * routingStep: int * duration: TimeSpan

type PlanningEdge =
    | Consumes of sourceId: string * targetId: string * quantity: decimal
    | Produces of sourceId: string * targetId: string * quantity: decimal
    | Requires of sourceId: string * targetId: string
    | Constrains of sourceId: string * targetId: string * limit: decimal
```

---

## Appendix B — Event Type Catalogue  

### B.1 Demand Events  

| Event Type | Payload | Publisher | Consumers |
|------------|---------|-----------|-----------|
| `DemandLineIngested` | Demand line details, quantity, dates | Understand Demand | Demand projections, Supply, Knowledge |
| `DemandLineRevised` | Updated fields | Understand Demand | Demand projections |
| `DemandLinePromised` | Promise date, confirmed quantity | Promise Orders | Demand projections, Supply |
| `DemandLineFrozen` | Frozen until timestamp | Understand Demand | Demand projections |
| `DemandLineFulfilled` | Fulfilled quantity, delivery date | Understand Demand | Demand projections, Knowledge |
| `ForecastGenerated` | Forecast with prediction intervals, confidence | Forecast Demand | Supply, Scenario, Knowledge |
| `ForecastPublished` | Forecast ID, cycle ID, timestamp | Forecast Demand | Supply, Promise, Scenario |
| `ForecastOverridden` | Old value, new value, justification, planner | Forecast Demand | Evaluate Quality, Explain, Knowledge |
| `ModelChampionSelected` | New model, old model, evaluation metrics | Forecast Demand | Evaluate Quality, Knowledge |
| `DemandChangeDetected` | Product, severity, magnitude | Sense Demand | Forecast, Detect Exceptions |
| `SegmentMasterPublished` | Segmentation version, coverage | Segment Demand | Prioritize, Classify, Forecast |
| `DemandPatternClassified` | Product, pattern, confidence | Classify Demand | Forecast |
| `PriorityListPublished` | Priority scores per item | Prioritize Demand | Detect Exceptions, Planners |
| `QualityReportPublished` | Period, WAPE, flags | Evaluate Quality | Learn, Management |
| `DemandExceptionDetected` | Exception type, item, severity | Detect Exceptions | Explain, Learn |

### B.2 Supply Events  

| Event Type | Payload | Publisher | Consumers |
|------------|---------|-----------|-----------|
| `InventoryPositionUpdated` | Product, location, on‑hand, on‑order | Understand Supply | Plan Supply, Promise, Knowledge |
| `SupplyPlanGenerated` | Plan version, horizon, metrics | Plan Supply | Promise, Scenario, Knowledge |
| `SupplyPlanPublished` | Plan version, published timestamp | Plan Supply | Procure, Schedule, Distribute, Promise |
| `SupplyPlanInfeasible` | Violated constraints | Plan Supply | Sense Exceptions |
| `ReplenishmentRecommended` | Product, location, quantity, due date | Manage Inventory | Procure |
| `InventoryPolicyUpdated` | Safety stock, reorder point per item | Manage Inventory | Plan Supply, Knowledge |
| `CapacityFeasibilityAssessed` | Resource, load vs. capacity | Manage Capacity | Plan Supply, Schedule |
| `SupplierCommitmentEvaluated` | Supplier, confidence factor | Collaborate Suppliers | Plan Supply, Procure |
| `PurchaseOrderReleased` | PO ID, supplier, items | Procure | Supplier systems, ERP |
| `ProductionSchedulePublished` | Schedule version, resource assignments | Schedule Production | MES, Manage Inventory |
| `TransferOrderReleased` | Transfer ID, source, destination | Manage Distribution | WMS |
| `SupplyDisruptionDetected` | Event ID, severity, affected orders | Sense Supply Changes | Detect Exceptions, Plan Supply |
| `SupplyQualityReportPublished` | Report ID, period, metrics | Evaluate Quality | Learn, Management |

### B.3 Promise Events  

| Event Type | Payload | Publisher | Consumers |
|------------|---------|-----------|-----------|
| `OrderAccepted` | Order ID, customer, lines, requested date | Understand Orders | Promise Orders, Prioritize |
| `ATPResultCalculated` | Line ID, ATP quantity, earliest date, sources | Promise Orders | Explain |
| `CTPResultCalculated` | Line ID, CTP feasible, production date | Promise Orders | Explain |
| `SubstitutionOffered` | Line ID, substitute product/location | Promise Orders | Customer Communication |
| `PromiseConfirmed` | Line ID, promise date, type, commitment ID | Promise Orders | Understand Orders, Supply, Knowledge |
| `PromiseRejected` | Line ID, reason | Promise Orders | Customer Communication |
| `SupplyConsumed` | Supply source, quantity, commitment ref | Promise Orders | Supply, Manage Allocations |
| `AllocationRuleDefined` | Rule ID, item, channel, quantity, period | Manage Allocations | Promise Orders |
| `AllocationConsumed` | Promise ref, pool ID, quantity | Manage Allocations | Supply |
| `AllocationExhausted` | Pool ID, exhaustion time | Manage Allocations | Promise Orders, Sense Risks |
| `PromiseRiskAssessed` | Promise ID, risk score, cause | Sense Risks | Detect Exceptions |
| `PromiseExceptionDetected` | Exception ID, type, affected orders | Detect Exceptions | Explain, Learn |
| `PromiseMetricsComputed` | Scope, period, metrics | Evaluate Quality | Learn |

### B.4 Scenario Events  

| Event Type | Payload | Publisher | Consumers |
|------------|---------|-----------|-----------|
| `ScenarioDefined` | Scenario ID, type, assumptions | Define Scenarios | Simulate |
| `SimulationStarted` | Run ID, scenario, plan, method | Simulate | Monitor |
| `SimulationCompleted` | Run ID, results summary, confidence | Simulate | Compare, Assess Risks, Recommend |
| `ComparisonCompleted` | Comparison ID, ranking summary | Compare | Recommend, Collaborate |
| `RiskAssessmentCompleted` | Risk ID, score, level | Assess Risks | Recommend |
| `ScenarioRecommendationMade` | Recommendation ID, variant, rationale | Recommend | All operational domains |
| `ScenarioRecommendationAdopted` | Recommendation ID, adopted plan version | Recommend | Demand, Supply, Promise |
| `ScenarioQualityReportPublished` | Report ID, period, metrics | Evaluate Quality | Learn, Management |

### B.5 Knowledge Events  

| Event Type | Payload | Publisher | Consumers |
|------------|---------|-----------|-----------|
| `KnowledgeGraphUpdated` | Object ID, type, change type | Govern Graph | All Knowledge capabilities |
| `CandidatePatternDetected` | Pattern ID, type, domains, metrics | Discover Patterns | Analyze Root Causes |
| `PatternValidated` | Pattern ID, validation method, confidence | Discover Patterns | Analyze Root Causes, Explain |
| `RootCauseIdentified` | Analysis ID, root cause, confidence | Analyze Root Causes | Manage Portfolio, Explain |
| `CorrectiveActionRecommended` | Analysis ID, action details, impact | Analyze Root Causes | Manage Portfolio |
| `ImprovementProposed` | Initiative ID, origin, expected benefit | Manage Portfolio | Domain Managers |
| `BestPracticePublished` | Practice ID, version, applicable domains | Institutionalise | All domains, AI Agents |
| `FeedbackLoopOpened` | Loop ID, triggering event, owner | Orchestrate Loops | All Knowledge capabilities |
| `FeedbackLoopClosed` | Loop ID, closure status, lessons learned | Orchestrate Loops | Enterprise Memory, Learn |
| `EventRecorded` | Event ID, domain, type, artifacts | Maintain Memory | Discover Patterns, AI Agents |
| `DecisionRecorded` | Decision ID, context, alternatives, rationale | Maintain Memory | AI Agents |
| `KnowledgeQualityComputed` | Period, metrics | Evaluate Quality | Learn |

---

## Appendix C — API Endpoint Reference  

### C.1 Demand API (`/api/v1/demand`)  

| Method | Path | Purpose | Owner Capability |
|--------|------|---------|------------------|
| `POST` | `/signals` | Ingest demand signals | Understand Demand |
| `GET` | `/history` | Query cleansed demand history | Understand Demand |
| `POST` | `/forecasts/generate` | Generate forecast for a horizon | Forecast Demand |
| `GET` | `/forecasts` | Retrieve published forecasts | Forecast Demand |
| `POST` | `/forecasts/override` | Submit manual forecast override | Forecast Demand |
| `GET` | `/segments` | Retrieve segmentation assignments | Segment Demand |
| `GET` | `/patterns` | Retrieve demand pattern classifications | Classify Demand |
| `GET` | `/priorities` | Retrieve priority scores | Prioritize Demand |
| `GET` | `/exceptions` | Retrieve active demand exceptions | Detect Demand Exceptions |
| `GET` | `/quality/report` | Retrieve demand quality report | Evaluate Demand Quality |

### C.2 Supply API (`/api/v1/supply`)  

| Method | Path | Purpose | Owner Capability |
|--------|------|---------|------------------|
| `POST` | `/data` | Ingest supply transactions | Understand Supply |
| `GET` | `/position` | Retrieve inventory position | Understand Supply |
| `GET` | `/plan` | Retrieve current supply plan | Plan Supply |
| `POST` | `/plan/generate` | Generate a new supply plan | Plan Supply |
| `GET` | `/inventory/policy` | Retrieve inventory policies | Manage Inventory |
| `PUT` | `/inventory/policy/{id}` | Update inventory policy | Manage Inventory |
| `GET` | `/inventory/replenishment` | Retrieve replenishment recommendations | Manage Inventory |
| `GET` | `/capacity` | Retrieve capacity status | Manage Capacity |
| `POST` | `/procurement/requisitions` | Create purchase requisition | Procure Materials |
| `POST` | `/procurement/purchase-orders` | Release purchase order | Procure Materials |
| `GET` | `/production/schedule` | Retrieve production schedule | Schedule Production |
| `GET` | `/distribution/transfers` | Retrieve transfer recommendations | Manage Distribution |
| `GET` | `/suppliers/scorecard` | Retrieve supplier scorecards | Collaborate Suppliers |
| `GET` | `/exceptions` | Retrieve active supply exceptions | Detect Supply Exceptions |
| `GET` | `/quality/report` | Retrieve supply quality report | Evaluate Supply Quality |

### C.3 Promise API (`/api/v1/promises`)  

| Method | Path | Purpose | Owner Capability |
|--------|------|---------|------------------|
| `POST` | `/orders` | Accept and validate order | Understand Orders |
| `POST` | `/evaluate` | Execute ATP/CTP evaluation | Promise Orders |
| `GET` | `/{orderId}` | Retrieve promise status | Promise Orders |
| `POST` | `/orders/{orderId}/changes` | Submit order change request | Manage Order Changes |
| `GET` | `/allocations` | Retrieve allocation rules and pools | Manage Allocations |
| `POST` | `/allocations/rules` | Define new allocation rule | Manage Allocations |
| `GET` | `/exceptions` | Retrieve active promise exceptions | Detect Promise Exceptions |
| `GET` | `/quality/report` | Retrieve promise quality report | Evaluate Promise Quality |

### C.4 Scenario API (`/api/v1/scenarios`)  

| Method | Path | Purpose | Owner Capability |
|--------|------|---------|------------------|
| `POST` | `/` | Create scenario definition | Define Scenarios |
| `GET` | `/catalogue` | Retrieve scenario catalogue | Define Scenarios |
| `POST` | `/simulations/run` | Start a simulation | Simulate Scenarios |
| `GET` | `/simulations/{runId}` | Retrieve simulation status | Simulate Scenarios |
| `POST` | `/comparisons` | Execute scenario comparison | Compare Scenarios |
| `POST` | `/risks/assess` | Run risk assessment | Assess Risks |
| `POST` | `/risks/stress-test` | Execute stress test | Assess Risks |
| `POST` | `/recommendations` | Generate recommendation | Recommend Scenario |
| `POST` | `/recommendations/{id}/adopt` | Adopt recommended plan | Recommend Scenario |
| `GET` | `/quality/report` | Retrieve scenario quality report | Evaluate Scenario Quality |

### C.5 Knowledge API (`/api/v1/knowledge`)  

| Method | Path | Purpose | Owner Capability |
|--------|------|---------|------------------|
| `GET` | `/graph` | Query knowledge graph | Govern Graph |
| `POST` | `/graph/validate` | Validate proposed object | Govern Graph |
| `GET` | `/patterns` | Retrieve discovered patterns | Discover Patterns |
| `POST` | `/patterns/detect` | Trigger pattern detection | Discover Patterns |
| `POST` | `/root-cause/analyze` | Start root‑cause analysis | Analyze Root Causes |
| `GET` | `/improvements` | Retrieve improvement portfolio | Manage Portfolio |
| `POST` | `/improvements` | Propose improvement | Manage Portfolio |
| `GET` | `/practices` | Retrieve best practices | Institutionalise |
| `POST` | `/practices` | Nominate best practice | Institutionalise |
| `GET` | `/feedback-loops` | Retrieve feedback loops | Orchestrate Loops |
| `POST` | `/memory/query` | Query enterprise memory | Maintain Memory |
| `POST` | `/agent/query` | AI agent knowledge request | Serve Knowledge |
| `GET` | `/quality/report` | Retrieve knowledge quality report | Evaluate Quality |
| `GET` | `/explanations/{artifactId}` | Retrieve explanation | Explain Insights |

---

## Appendix D — Configuration and Feature Flags  

### D.1 Feature Flags Reference  

| Flag | Default | Description |
|------|---------|-------------|
| `AiAutonomyEnabled` | `false` | Master switch for AI autonomy features |
| `AiDefaultAutonomyLevel` | `"Advisory"` | Default level for new AI agents |
| `AiPolicySuggestionEnabled` | `false` | Allows AI to suggest policy changes |
| `AiConversationalCopilotEnabled` | `false` | Enables conversational LLM copilot |
| `FastInsertEnabled` | `true` | Enables FastInsert planning mode |
| `IncrementalRepairEnabled` | `true` | Enables IncrementalRepair mode |
| `OptimizationEnabled` | `true` | Enables Optimization mode |
| `WhatIfSimulationEnabled` | `true` | Enables WhatIf scenario simulation |
| `SupplyCollaborationEnabled` | `false` | Enables supplier collaboration features |
| `KnowledgeIntelligenceEnabled` | `true` | Enables Knowledge Intelligence domain |
| `ScenarioComparisonEnabled` | `true` | Enables multi‑scenario comparison |
| `LlmIntegrationEnabled` | `false` | Enables LLM integration for explanations |
| `AdvancedAnalyticsEnabled` | `false` | Enables advanced analytics features |

### D.2 Environment Variables  

| Variable | Purpose | Default |
|----------|---------|---------|
| `MEDHAVI_EVENTSTORE_CONNECTION` | PostgreSQL connection string | `Host=localhost;Database=medhavi` |
| `MEDHAVI_EVENTSTORE_MAX_RETRY` | Max retries for event store operations | `3` |
| `MEDHAVI_CIRCUIT_FAILURE_THRESHOLD` | Failures before circuit opens | `5` |
| `MEDHAVI_CIRCUIT_RECOVERY_SECONDS` | Seconds before half‑open attempt | `30` |
| `MEDHAVI_PLANNING_SOLVER_TIMEOUT` | Solver time limit in seconds | `300` |
| `MEDHAVI_OBSERVABILITY_LOG_LEVEL` | Minimum log level | `Information` |
| `MEDHAVI_OBSERVABILITY_METRICS_ENABLED` | Enable Prometheus metrics | `true` |
| `MEDHAVI_AI_AUTONOMY_ENABLED` | Enable AI autonomy | `false` |

---

## Appendix E — Store Catalogue and Schemas  

### E.1 WorkspaceStore Types  

| Store | Kind | Data Type | Load Function |
|-------|------|-----------|---------------|
| DemandStore | `DemandWorkspace` | `DemandData` | `DemandLineQueries.GetAll()` |
| SupplyStore | `SupplyWorkspace` | `SupplyData` | `SupplyQueries.GetAll()` |
| MaterialReservationStore | `MaterialReservationWorkspace` | `DemandData` | `DemandLineQueries.GetAll()` |
| CapacityStore | `CapacityWorkspace` | `CapacityData` | `CapacityQueries.GetAll()` |
| ScenarioStore | `ScenarioWorkspace` | `ScenarioData` | `ScenarioQueries.GetAll()` |
| PromiseStore | `PromiseWorkspace` | `PromiseData` | `PromiseQueries.GetAll()` |
| KnowledgeStore | `KnowledgeWorkspace` | `KnowledgeData` | `KnowledgeQueries.GetAll()` |

### E.2 WorkspaceSnapshot Schema  

```fsharp
type WorkspaceSnapshot<'data> = {
    Data: 'data option
    Freshness: Freshness       // Fresh | Stale | Loading | Failed of string
    Version: int64
    LastRefreshUtc: DateTime option
    Error: string option
}
```

### E.3 PlanningContext Schema  

```fsharp
type PlanningContext = {
    ScenarioId: string option
    PlantId: string option
    HorizonStart: DateTimeOffset
    HorizonEnd: DateTimeOffset
    SelectedProductFamilies: string list
}
```

---

## Appendix F — Resilience Policies Reference  

### F.1 Circuit Breaker Defaults  

| Parameter | Default | Description |
|-----------|---------|-------------|
| `FailureThreshold` | 5 | Consecutive failures before opening |
| `RecoveryTimeout` | 30 seconds | Time before attempting half‑open |
| `MaxRecoveryTimeout` | 60 seconds | Maximum backoff time |
| `BackoffFactor` | 2.0 | Exponential backoff multiplier |
| `MonitoringPeriod` | 30 seconds | Window for counting failures |
| `SuccessThreshold` | 3 | Successes in half‑open to close |

### F.2 Retry Defaults  

| Scenario | Max Retries | Base Delay | Backoff Strategy |
|----------|-------------|------------|------------------|
| Database connection failure | 3 | 100ms | Exponential (×2) |
| HTTP 429 (rate limited) | 5 | 500ms | Exponential (×2) |
| HTTP 503 (service unavailable) | 3 | 200ms | Exponential (×2) |
| Optimistic concurrency conflict | 3 | 100ms | Exponential (×2) |
| Domain validation error | 0 | — | No retry |

### F.3 Graceful Degradation Defaults  

| Scenario | Behaviour |
|----------|-----------|
| Event store unavailable | Return 503; serve stale projections |
| External ERP unavailable | Circuit open; buffer events |
| AI agent unavailable | Fall back to human approval |
| Knowledge Intelligence unavailable | Operate without enterprise memory |
| Projection lag > 30s | Report Degraded; serve with StaleSince timestamp |

---