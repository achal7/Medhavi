# Medhavi Platform — Implementation Blueprint for AI Agents

**Lead Enterprise Software Architect — Medhavi APS Platform**

Below is the complete, consolidated implementation guideline for your AI agent. It incorporates all previous structural and coding rules, plus your additional production‑quality demands.

---

# Medhavi Platform — AI Agent Implementation Guidelines (v1.0)

## 0. Governing Principles

- **No fabrication, no assumption, no workaround.** Every line of code must trace back to a ratified artifact in the specification documents. If something is not defined, **stop and ask**; never invent.
- **Production‑grade code only.** No placeholder comments (`// TODO`), no stub methods, no simplified logic. The code must be correct, complete, and ready for deployment.
- **Functional programming only.** Use pure functions, immutability, algebraic data types (discriminated unions), monadic error handling (`Result`, `AsyncResult`), and function composition. No classes, no inheritance, no mutable state outside controlled boundaries (like the `ProjectionAgent`) in case if we really need mutable state.
- **Category theory concepts where relevant.** Use functors (`map`, `<!>`), applicatives (`<*>`), monads (`bind`, `>>=`, computation expressions). Prefer `Result`, `Task` and `TaskResult` for error propagation. 
Refer to Medhavi.Common project
- **Comments for business rules, policies, decisions, algorithms.** Every rule function must be annotated with its `BR‑xxx` ID, every decision with its `DE‑xxx` ID, etc. The code itself should be self‑documenting, but the traceability comment is mandatory.
- **Challenge every line for perfection.** Before outputting code, the agent must internally verify:
  - Correctness against the spec’s Information Model, invariants, lifecycle transitions.
  - Compliance with the Medhavi architectural constitution.
  - Handling of all edge cases (missing inputs, invalid state transitions, concurrency).
  - That no OOP patterns are used.
- **If a requirement cannot be met with production‑grade code, stop and report the obstacle.** Do not produce incomplete or temporary code.

---

## Before writing any code, ensure the agent has access to:

- `1_Constitution_v1.md`
- `2_Architecture_Reference_Standard_v1.md`
- `3_Semantic_Model_v1.md`
- `4_Demand_Intelligence_Specification.md`

All code must trace back to a ratified artifact ID (`SE‑C‑xxx`, `SE‑D‑xxx`, `AB‑D‑xxx`, etc.).

---

## 1. Solution & Project Structure

The solution is already defined with the following projects:

```
Medhavi.Common   (Computation expressions for Result, Task,Async, Validations library, Retry, Serialization etc)
Medhavi.Foundation   (technical infrastructure – no business meaning)
Medhavi.Core         (enterprise semantic model + internal lifecycle capabilities)
Medhavi.Contracts    (shared DTOs, commands, notifications, API types)
Medhavi.Demand       (demand bounded context – vertical slices)
Medhavi.Infrastructure (persistence, event bus, projections)
Medhavi.Nexus        (composition root, API host)
Medhavi.Web          (UI)
```

Reference rules:
- `Medhavi.Core` → `Medhavi.Foundation`
- `Medhavi.Contracts` → `Medhavi.Core`, `Medhavi.Foundation`
- `Medhavi.Demand` → `Medhavi.Core`, `Medhavi.Contracts`, `Medhavi.Foundation`
- `Medhavi.Infrastructure` → `Medhavi.Foundation`
- `Medhavi.Nexus` → everything except Web
- `Medhavi.Web` → `Medhavi.Contracts`

No circular references. Do not add new projects unless explicitly approved.

---

## 2. Core Types & File Layout

### 2.1 `Medhavi.Foundation` (DO NOT MODIFY without approval)

Contains only technical primitives. Do **not** put any `SE‑C‑xxx` or domain concepts here. The following base types are available:

- `ArsIdentifier` (base type for all architectural IDs)
- `DomainError` and `Result`/`AsyncResult` helpers
- `Aggregate` module with `Repository<'a, 'key, 'event>`, `CommandPipeline`, `ExecutionOutcome`
- `DomainEventBus`, `ProjectionAgent`
- `DecisionTrace` base types
- Execution pipeline modules (`CommandPipeline`, `CommandCapabilities`, etc.)
- `Ids` (ID generation), `Validations` (common validators)
- `InMemRepository` (generic in‑memory repository)

Use these as they are. Never add business logic here.

### 2.2 `Medhavi.Core`

Flat value objects live directly at the root of the project. Aggregates with lifecycles get a component folder.

**Flat files (no folder, pure immutable types):**
- `Timestamp.fs` (SE‑C‑022)
- `PositiveDecimal.fs`
- `Quantity.fs` (SE‑C‑023)
- `Duration.fs` (SE‑C‑024)
- `TemporalWindow.fs` (SE‑C‑028)
- `NeedWindow.fs` (SE‑C‑029)
- `Money.fs` (SE‑C‑025) – if needed
- `TimeZone.fs` (SE‑C‑031) – if needed

**Aggregate component folders (7 files each):** `UnitOfMeasure/`, `Item/`, `Location/`, `Customer/`, `Supplier/`, `PlanningScope/`, `Calendar/`, `PlanningPeriod/`, `EnterprisePicture/`.

Inside each aggregate folder, the files are ordered exactly as:
1. `Model.fs`
2. `Rules.fs`
3. `Behaviours.fs`
4. `Projections.fs`
5. `ACL.fs`
6. `CommandHandler.fs`
7. `Capabilities.fs`

**ArsIdentifiers** file at project root: `ArsIdentifiers.fs` (contains `Core` module with all `SE‑C‑xxx`, `BR‑C‑xxx`, `BN‑C‑xxx` etc.).

### 2.3 `Medhavi.Demand`

Vertical capability slices are top‑level folders: `UnderstandDemand/`, `ForecastDemand/`, `SenseDemand/`, `SegmentDemand/`, `ClassifyDemand/`, `PrioritizeDemand/`, `EvaluateDemandQuality/`, `DetectDemandExceptions/`, `ExplainDemand/`, `LearnFromDemand/`.

Inside each capability folder:

- **One sub‑folder per owned aggregate** (e.g., `DemandObservation/`, `DemandUnderstanding/`), containing:
  - `Model.fs`
  - `Rules.fs`
  - `Decisions.fs`
  - `Algorithms.fs` (if needed)
  - `Policies.fs`
  - `Projections.fs`
- **Capability‑level files:**
  - `ACL.fs`
  - `FunctionalSpecifications.fs`
  - `Capabilities.fs`

**ArsIdentifiers** file at project root: `ArsIdentifiers.fs` (contains `Demand` module with all `CA‑D‑xxx`, `DE‑D‑xxx`, `BR‑D‑xxx`, etc.).

---

## 3. File‑by‑File Implementation Rules

### 3.1 `Model.fs`

Defines the aggregate’s identity, state, lifecycle states, commands, events, and the `evolve` function. Follow the exact pattern from your old `EnterpriseDemandPicture/Model.fs`:

- **Identity type**: Single‑case discriminated union wrapping `string` or `Guid`. Use the `ArsIdentifier` base if desired.
- **Lifecycle state DU**: Exactly the states from the specification. Never a generic `Active`/`Inactive`. Example: `type ItemState = Active | Inactive | Retired`.
- **Aggregate record**: All mandatory and optional attributes as per the spec’s Information Model. Use `Core` types (`Timestamp`, `Quantity`, etc.).
- **Commands**: A discriminated union containing all commands that target this aggregate. Each command carries the identity of the aggregate and the required data.
- **Events**: A discriminated union of past‑tense events. Events are granular (e.g., `ItemCreated`, `ItemActivated`, `ItemRetired`). Each event carries the full snapshot of the aggregate state after the event (as in your old code: `EdpRevised of EnterpriseDemandPicture`). This simplifies projections.
- **`evolve` function**: `Event -> state option -> state option`. Never mutates state; always returns a new option.
- **No external dependencies** (no I/O, no database calls inside the model).

### 3.2 `Rules.fs`

Pure functions that enforce invariants and eligibility rules. Signature: `input -> Result<unit, DomainError>`. Each rule maps to a `BR‑C‑xxx` or `BR‑D‑xxx` identifier. Use the `Result` type from Foundation.

Example:

```fsharp
let itemMustNotBeRetired (state: ItemState) =
    if state = Retired then Error(DomainError.validation "Item is retired")
    else Ok()
```

Rules are combined in the decision or behaviour using `result { }` computation expressions. Those are defined in Medhavi.Common project

### 3.3 `Behaviours.fs` (Core aggregates) / `Decisions.fs` (Demand aggregates)

**For Core:** `Behaviours.fs` contains functions that take `command + current state` and return `Result<Event list, DomainError>`. They call `Rules` functions, apply the command, and return new events. Example:

```fsharp
let activateItem (cmd: ActivateItemCmd) (state: Item) =
    result {
        do! Rules.itemMustNotBeRetired state.State
        return [ ItemActivated { state with State = Active } ]
    }
```

**For Demand:** `Decisions.fs` contains the decision functions (`DE‑D‑xxx`) that evaluate evidence against rules and produce a `DecisionOutcome` (a DU like `Accept | Quarantine of reasonCode | Reject of reasonCode`). The `decide` function signature follows your old pattern: `Command -> state option -> Result<Event list, DomainError>`. Inside, it may call algorithms, evaluate rules, and return events.

### 3.4 `Algorithms.fs` (Demand only)

Pure functions that perform computations specified in `BA‑D‑xxx`. No side effects. They consume domain types and return results (assessments, scores, confidence levels). Example:

```fsharp
let evaluateMateriality (draft: DemandUnderstanding) (published: DemandUnderstanding option) =
    // returns MaterialityAssessment
```

Algorithms are invoked by decisions or functional specifications.

### 3.5 `Policies.fs` (Demand only)

Defines configuration records for the governed policy parameters. No behaviour. Example:

```fsharp
type DemandDataAcceptancePolicy = {
    MaxDataLatency: Duration
    MinSourceReliability: decimal
    DuplicateDetectionWindow: Duration
}
```

Default values are provided as static members or module values. Policy loading is done by `FunctionalSpecifications.fs` or the composition root.

### 3.6 `ACL.fs`

Anti‑corruption layer. Translates external DTOs (from `Medhavi.Contracts`) into domain commands. Uses applicative validation with the `<*>` pattern (exactly as in your old `EnterpriseDemandPicture.ACL.fs`). Example:
Refer to Medhavi.Common project which contains Validation framework 
Refer to Medhavi.Foundation.Validation which contains common validation checks.

```fsharp
let toCreateItemCmd (req: CreateItemReq) =
    let make name uom =
        { ItemId = ItemId.newId(); Name = name; UnitOfMeasureId = uom }
    make <!> (nonEmptyString req.Name |> fromResult)
         <*> (UnitOfMeasureId.fromString req.UnitOfMeasureId |> fromResult)
```

ACL functions may also validate existence of referenced Core objects by calling injected lookup functions (`IItemLookup`, etc.). The ACL is capability‑scoped (e.g., `UnderstandDemand/ACL.fs` contains validators for both DemandObservation and DemandUnderstanding commands).

### 3.7 `CommandHandler.fs` (Core) / `FunctionalSpecifications.fs` (Demand)

This is the orchestrator. It wires the repository, ACL, policies, decisions, and event bus.

**For Core:** `CommandHandler.fs` directly uses `CommandPipeline` from Foundation:

```fsharp
let execute (repo: Repository<Item, ItemId, ItemEvent>) (cmd: ItemCommand) =
    let pipeline = CommandPipeline.create repo (fun cmd -> cmd.AssignmentId) Behaviours.decide
    CommandCapabilities.execute pipeline publishKnowledge cmd
```

**For Demand:** `FunctionalSpecifications.fs` contains the implementation of `FS‑D‑xxx`. Each function corresponds to one workflow step. It performs the orchestration (load aggregate, call decisions, persist events, publish notifications) exactly as described in the FS contract. Example pattern:

```fsharp
let receiveObservation (repo, acl, publish) (cmd: ReceiveDemandObservationCmd) =
    asyncResult {
        let! domainCmd = acl.toReceiveCmd cmd
        let! events = Behaviours.receive domainCmd None  // or use decide
        do! repo.Save(domainCmd.ObservationId, None, events)
        publish (DemandObservationReceivedNotification { ... })
    }
```

### 3.8 `Projections.fs`

Defines a read‑model projection using `ProjectionAgent` (exactly as your old `EdpProjectionAgent`). The projection agent subscribes to the domain events and maintains a map or list that can be queried. Exposes a `QueryService`. Must map the domain aggregate to the contract DTO (from `Medhavi.Contracts`).

Example pattern:

```fsharp
let evolveProjection (state: Map<ItemId, ItemDto>) (evt: ItemEvent) =
    match evt with
    | ItemCreated item -> state |> Map.add item.Id (ItemDto.fromDomain item)
    | ItemActivated item -> state |> Map.add item.Id (ItemDto.fromDomain item)
    | ...

let createProjectionAgent () = ProjectionAgent(evolveProjection, Map.empty, "ItemReadModel")
```

### 3.9 `Capabilities.fs`

Exposes the public API of the capability (or aggregate for Core) to Nexus. This is a thin record of functions:

```fsharp
type ItemApi = {
    Create: CreateItemCmd -> Task<Result<ItemId, ApiError>>
    Activate: ActivateItemCmd -> Task<Result<unit, ApiError>>
    // ...
}

let createCapabilities (execute: ItemCommand -> Task<ExecutionOutcome<Item, _>>) : ItemApi = {
    Create = fun cmd -> execute (ItemCommand.Create cmd) |> Task.map (outcomeToResult)
    // ...
}
```

Use `CommandPipeline` from Foundation to handle the common command execution pattern.


**Lead Enterprise Software Architect — Medhavi APS Platform**

The following is the missing `3.10` section that must be inserted into the AI agent guideline immediately after the `Medhavi.Demand` implementation rules, before the step‑by‑step order.

---

## 3.10 `Medhavi.Contracts` — Detailed Implementation Rules

`Medhavi.Contracts` is the **single shared contracts assembly for V1**. It contains **only data types** – no domain logic, no functions with business meaning, no algorithms, no behaviour. Its purpose is to serve as the public API surface between bounded contexts and from `Medhavi.Nexus` to the outside world.

### 3.10.1 Project Structure

```
Medhavi.Contracts/
├── Common/
│   ├── ApiError.fs
├── Core/
│   ├── Item.fs
│   ├── Location.fs
│   ├── Customer.fs
│   ├── Supplier.fs
│   ├── PlanningScope.fs
│   ├── Calendar.fs
│   ├── PlanningPeriod.fs
│   └── EnterprisePicture.fs
├── Demand/
│   ├── DemandObservation.fs
│   └── DemandUnderstanding.fs
└── Web/
    ├── Demand/
    │   └── (API request/response DTOs for demand endpoints)
    └── Core/
        └── (API request/response DTOs for core endpoints)
```

### 3.10.2 Naming and File Organisation

- **Per‑domain subfolder** (`Core/`, `Demand/`). Each domain’s contracts live strictly inside its own namespace. No subfolders, just one folder per domain and all related types will be in that folder.
- **Within each domain subfolder, there will be only one file per DTO/contract type.** This is a strict convention to avoid monolithic contract files. Each file will be created using <AggregateName>.fs
For example if our aggregate name is Item, then our file name will be Item.fs and it will contain all the types related to Item. In future if we add new aggregate in Core domain we will create a new file for it.

**Types inside aggregate file:**

| File | Purpose |
|------|---------|
| `*.fs` | Read‑side data transfer object — flat, immutable record with no behaviour. For example, `Item.fs` contains a single `Item` record. |
| `*Cmd` | Command payloads — records that carry the data needed to execute a command. One file per command, or one file with a discriminated union if multiple commands are closely related and share an API endpoint. Use separate files when commands are complex. |
| `*Notification` | Published business notifications (`BN‑xxx`). These are immutable records with all fields defined by the specification. |
| `*Api` | The public API record for the aggregate/capability. A record of functions returning `Task<Result<_, ApiError>>`. For Core aggregates, this is the per‑aggregate API; for Demand, this is the capability API (wrapping multiple aggregates if needed, but placed in the most relevant aggregate folder for simplicity). |
| `*Queries` | Type alias for the query service (e.g., `type ItemQueries = QueryService<Item, string>`). Create dedicate type in case if you need customized querires which are not available in QueryService base type. |

### 3.10.3 Type Design Rules

1. **DTOs are flat, immutable records.** All fields are exposed as primitive types (strings, decimals, DateTimeOffset, lists, etc.) or other contract DTOs.  
   ```fsharp
   type Item = {
       ItemId: string
       Name: string
       UnitOfMeasureId: string
       State: string
   }
   ```
2. **No domain types in contracts.** Never use internal domain types like `ItemId` (the single‑case DU) or `Quantity`. Convert to `string`, `decimal`, etc. The mapping from domain to contract is done in the `Projections.fs` file of the owning aggregate.
3. **Commands are immutable records.** They contain exactly the data the command needs, with fields as primitive types where possible.
4. **Notifications mirror the specification.** A `BN‑D‑006 DemandObservationAcceptedNotification` must contain `ObservationId` and `Confidence` – every field listed in the spec’s Business Notification table. No extra fields.
5. **API records** define the function signatures that Nexus calls. They always return `Task<Result<'a, ApiError>>`. For commands that produce a new identifier, return the identifier; for state changes, return `unit`. The API record is the **only** entry point from the outside.
6. **Query types** are simple aliases for `QueryService<'dto, 'key>`. The actual implementation lives in the infrastructure layer or the projection agent.

### 3.10.4 Traceability Comments

Every file must have a header comment documenting the artifact IDs it implements. Example for `DemandObservationDto.fs`:

```fsharp
// Medhavi.Contracts.Demand.DemandObservation.DemandObservationDto
// Traceability: SE‑D‑001 Demand Observation read‑side DTO
```

For a notification:

```fsharp
// Medhavi.Contracts.Demand.DemandObservation.DemandObservationAcceptedNotification
// Traceability: BN‑D‑006
```

For an API:

```fsharp
// Medhavi.Contracts.Demand.DemandObservation.DemandObservationApi
// Traceability: Capability CA‑D‑001 UnderstandDemand, public command API
```

### 3.10.5 `Web/` Folder Rules

- Contains request/response DTOs specific to the Web API. These may differ slightly from the internal DTOs (e.g., pagination wrappers, HTTP‑friendly error formats).
- They are used exclusively by `Medhavi.Nexus` to serialize/deserialize HTTP requests and by `Medhavi.Web` for its HTTP client.
- They must not leak into domain projects.

### 3.10.6 Strict Ownership

- Only the owning bounded context (e.g., Demand) is allowed to define types in its `Medhavi.Contracts/Demand/` folder. No other project may place files there.
- No domain project may reference `Medhavi.Contracts` types that belong to a different domain (e.g., `Medhavi.Demand` must not reference `Medhavi.Contracts.Supply`). This is enforced by namespace and solution folder conventions.

### 3.10.7 No Logic

Contracts files contain **only type definitions**. No `match` expressions, no validation functions, no mapping functions. The one exception: a `static member` to create a default or example value is acceptable for testing convenience, but must be clearly marked.

---

## 4. Implementation Rules for Value Objects

- Each value object is a flat file in `Medhavi.Core`.
- Contains a type definition, a `create` function that returns `Result`, and any helper functions.
- Value objects are immutable and use structural equality.
- Never add lifecycle states to a value object; that belongs to aggregates.

---

## 5. Command Pipeline Execution

Use the existing `CommandPipeline` from `Medhavi.Foundation`:

- Create pipeline: `CommandPipeline.create repo keySelector decide`
- Execute: `CommandCapabilities.execute pipeline publishKnowledge cmd`
- `publishKnowledge` is a function that records architectural metadata (decision traces).
- The pipeline handles versioning, concurrency, and event persistence automatically.

---

## 6. Event Bus and Notifications

- Use `DomainEventBus` from Foundation to publish events in‑process.
- Projections subscribe to events via `DomainEventBus.Subscribe`.
- Business notifications (`BN‑xxx`) are simple records defined in `Medhavi.Contracts`. They are published manually in `FunctionalSpecifications.fs` after successful persistence (or via a separate dispatcher). They are not part of the event sourcing stream.

---

## 7. ArsIdentifiers Usage

Every time a decision trace is recorded, use the constants from the appropriate `ArsIdentifiers` module:

```fsharp
open Medhavi.Core.ArsIdentifiers
open Medhavi.Demand.ArsIdentifiers

// Example trace record
{ DecisionId = Demand.Decisions.acceptDemandObservation
  CapabilityId = Demand.Capabilities.understandDemand
  RulesEvaluated = [ (Demand.Rules.demandSignalTimeliness, 1) ]
  ...
}
```

Always use the module constants, never hardcoded strings.

---

## 8. Testing Principles

For each aggregate/capability:

- Unit tests for all `Rules`, `Behaviours`/`Decisions`, and `Algorithms`.
- Use the examples from the specification as acceptance criteria.
- Integration tests for `FunctionalSpecifications.fs` using `InMemRepository`.
- Property‑based tests for invariants where possible.

---

## 9. Step‑by‑Step Implementation Order

1. **`Medhavi.Core` flat files**: `Timestamp`, `PositiveDecimal`, `Quantity`, `Duration`, `TemporalWindow`, `NeedWindow`.
2. **`Medhavi.Core` aggregates** in the order needed by Demand: `UnitOfMeasure` (needed by `Quantity`), `Item`, `Location`, `Customer`, `Supplier`, `PlanningScope`, `Calendar`, `PlanningPeriod`, `EnterprisePicture`.
3. **`Medhavi.Contracts/Core/`** DTOs, commands, notifications for each Core aggregate.
4. **`Medhavi.Demand` Slice 1**: `DemandObservation` aggregate and `UnderstandDemand` capability (ACL, FunctionalSpecifications, Capabilities).
5. **`Medhavi.Demand` Slice 2**: `DemandUnderstanding` aggregate and extend `UnderstandDemand`.
6. Continue with subsequent slices.

---

## 10. Final Constraint

Do **not** invent concepts not present in the specification. Do **not** add generic types like `Status` that violate the governed lifecycles. Every file must correspond to a ratified artifact.

This blueprint ensures the agent produces code consistent with the Medhavi architecture, your established patterns, and the frozen specifications.