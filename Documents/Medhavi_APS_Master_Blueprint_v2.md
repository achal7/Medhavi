# Medhavi APS Master Blueprint v2

This is the canonical architecture and implementation blueprint for Medhavi APS.

It is strict by design:
- no abstract placeholders,
- no temporary hacks,
- no duplicated business rules,
- no shared orchestration between Promise and PlanningEngine,
- no direct AI mutation of core plan state,
- no reliance on future features to justify current ambiguity.

This blueprint is written for a production APS platform, not an ERP system.

> Note: domain identifiers such as `SkuId`, `StockingPointId`, `PlantId`, `ResourceId`, and related SharedKernel types are already implemented. This document focuses on the APS decision architecture, not on re-defining those shared primitive IDs.

---

# 1. Architectural decision

The platform has two permanent decision centers:

## 1.1 DecisionCore
A shared pure library that contains APS semantics that must be identical everywhere.

It is the source of truth for:
- scoring
- limiters
- reservations
- feasibility
- time-window math
- normalization
- planning graph abstractions
- explainability contracts
- policy definitions
- policy validation contracts
- telemetry contracts
- policy suggestions
- autonomy contracts
- AI contract shapes
- decision context
- fingerprints
- provenance
- impact classification

## 1.2 PlanningEngine
The tactical planning bounded context that owns:
- planning snapshots
- planning runs
- plan versions
- material planning / MRP
- replanning
- replenishment
- optimization
- scenario execution
- explainability generation
- telemetry emission
- future continuous planning orchestration
- AI policy recommendation intake

Promise remains separate.
Scenario remains separate.
Capacity, Supply, Demand, MasterData, and Transport remain separate.

AI is not the owner of state; it is an advisory, ranking, and suggestion layer.

---

# 2. Final module architecture

## 2.1 PlanningEngine

PlanningEngine should be organized as:

- Shared
- MaterialPlanning
- Replan
- Replenishment
- Optimization
- ScenarioPlanning
- ContinuousPlanning
- Explainability
- Telemetry
- AI

## 2.2 DecisionCore

DecisionCore should be organized as:

- Identities
- Fingerprints
- DecisionContext
- Scoring
- Limiters
- Reservations
- Feasibility
- TimeWindows
- Normalization
- PlanningGraph
- Explainability
- Policies
- PolicyGate
- PolicySuggestions
- Autonomy
- AIContracts
- Provenance
- ImpactClassification
- TelemetryContracts

## 2.3 External bounded contexts
- Promise
- Scenario
- Capacity
- Supply
- Demand
- MasterData
- Transport
- Future AI bounded context

---

# 3. Design principles

1. Deterministic core first.
2. Shared decision semantics everywhere.
3. Mode-driven planning.
4. Separation of orchestration boundaries.
5. Pure core / imperative shell.
6. Explainability as a first-class product feature.
7. Event-first concurrency.
8. AI advisory first, guardrailed autonomy later.
9. Production-ready from day one.
10. No duplicated decision logic across modules.
11. Policies must be explicitly validated before use.
12. Every plan run must be traceable to snapshot, policy, mode, graph state, and provenance.
13. Promise and PlanningEngine may share semantics, never orchestration.
14. Scenario planning must reuse PlanningEngine, not reimplement it.
15. Optimization must reuse the same scoring and feasibility semantics as all other modes.
16. Planning context must be explicit and propagated everywhere.
17. Domain cross-cutting contracts must exist before AI is introduced.
18. Planning graph is an internal decision model first, concurrency substrate later.

---

# 4. Phase 0 — Architecture foundation

Before the core implementation begins, the architecture foundation must exist.

## 4.1 Why Phase 0 exists
The roadmap becomes much safer if the following are treated as first-class citizens before DecisionCore and PlanningEngine are expanded:
- fingerprints
- planning decision context
- provenance
- policy suggestions
- autonomy contracts
- AI contract shapes
- impact classification
- expanded explainability
- graph versioning

This layer prevents future retrofits.

## 4.2 Phase 0 deliverables
- `DecisionCore/Identities.fs`
- `DecisionCore/Fingerprints.fs`
- `DecisionCore/DecisionContext.fs`
- `DecisionCore/Provenance.fs`
- `DecisionCore/PolicySuggestions.fs`
- `DecisionCore/Autonomy.fs`
- `DecisionCore/AIContracts.fs`
- `DecisionCore/ImpactClassification.fs`
- `DecisionCore/Explainability.fs` expansion
- `DecisionCore/PlanningGraph.fs` versioning support

---

# 5. DecisionCore — file-by-file build order

Build DecisionCore before any PlanningEngine execution flow.

## 5.1 `DecisionCore/Identities.fs`
Common planning identity types distinct from SharedKernel domain IDs.

Owns:
- `SnapshotId`
- `PlanRunId`
- `PlanVersionId`
- `GraphStateId`
- `GraphVersionId`
- `DecisionTraceId`

These are planning-system identity concepts, not business-domain IDs.

## 5.2 `DecisionCore/Fingerprints.fs`
Canonical content-addressed identifiers for planning artifacts.

Owns:
- `SnapshotFingerprint`
- `PolicyFingerprint`
- `PlanFingerprint`
- `GraphFingerprint`

Requirements:
- deterministic generation
- stable equality
- version-safe comparison
- hash algorithm policy documented and testable

## 5.3 `DecisionCore/DecisionContext.fs`
Explicit context propagated into all decisions.

Owns:
- `PlanningDecisionContext`

Contains:
- snapshot id
- graph state id
- policy set id
- planning mode
- scenario id option
- planning horizon
- provenance reference
- trigger information

Every decision-facing function should receive this context or a derived context.

## 5.4 `DecisionCore/TimeWindows.fs`
Pure UTC time-window math.

Implement:
- overlap
- containment
- intersection
- expansion
- shifting
- slack
- bucket alignment
- lead-time offsets

## 5.5 `DecisionCore/Normalization.fs`
Normalization for cost, time, risk, and CO2.

Implement:
- clamping
- scaling
- unit interval mapping
- comparable score normalization

## 5.6 `DecisionCore/Reservations.fs`
Reservation semantics shared by Promise and PlanningEngine.

Add:
- `ReservationScope`
- `ReservationStatus`
- `ReservationRequest`
- `ReservationRecord`
- `ReservationPolicy`

Implement:
- create tentative
- confirm
- release
- expire
- reduce
- validate lifecycle transitions

## 5.7 `DecisionCore/Limiters.fs`
Standard limiter model.

Add:
- `LimiterDomain`
- `LimiterSeverity`
- `LimiterCode`
- `Limiter`
- `LimiterCatalog`

Implement mapping from:
- material shortage
- capacity overload
- transport failure
- supplier failure
- policy violation

## 5.8 `DecisionCore/Scoring.fs`
Shared score model.

Add:
- `PlanScore`
- `ScoreWeights`
- `ScoreContribution`
- `PlanScoreCard`

Implement:
- empty score
- combining scores
- weighted objective score
- candidate ranking
- card comparison

## 5.9 `DecisionCore/Feasibility.fs`
Pure feasibility contracts.

Add results for:
- material feasibility
- capacity feasibility
- transport feasibility
- supplier feasibility
- combined feasibility

Implement:
- compose feasibility outputs
- determine acceptability
- surface limiter explanations

## 5.10 `DecisionCore/Explainability.fs`
Structured decision trace contracts.

Add:
- `DecisionRationale`
- `DecisionTrace`
- `DecisionNarrative`
- `DecisionEvidence`
- `AlternativeCandidate`
- `BindingReason`

Implement:
- build trace
- add alternatives
- add binding reasons
- summarize trace
- produce AI-readable summary
- produce human-readable narrative

## 5.11 `DecisionCore/Policies.fs`
Shared policy envelope and mode profiles.

Add:
- `SharedPolicy`
- `FastInsertPolicy`
- `IncrementalRepairPolicy`
- `FullReplanPolicy`
- `OptimizationPolicy`
- `WhatIfPolicy`
- `ReplenishmentPolicy`
- `PlanningPolicySet`

Implement:
- defaults for each profile
- validation of shape and ranges
- policy resolution per mode

## 5.12 `DecisionCore/PolicyGate.fs`
Pure safety gate for policy changes.

This is mandatory.

The PolicyGate validates:
- max solver time
- max memory / search budget where applicable
- min safety stock
- max safety stock
- frozen horizon protection
- firm order protection
- hard constraint preservation
- maximum policy delta
- maximum objective weight shift
- approval requirements for risky updates

PolicyGate returns:
- valid
- rejected with reasons
- valid with warnings if the policy is within advisory boundaries

## 5.13 `DecisionCore/PolicySuggestions.fs`
Formal policy suggestion contract.

Owns:
- `PolicySuggestion`
- suggestion source
- confidence
- reasoning
- approval state
- expiration state

Policy suggestions exist before AI automation is introduced.

## 5.14 `DecisionCore/Autonomy.fs`
Formal autonomy contract model.

Owns:
- `AutonomyLevel`
- `AutonomyContract`
- approval rules
- rollback policy
- permitted actions
- allowed delta boundaries

No autonomous action is allowed unless the contract validates.

## 5.15 `DecisionCore/AIContracts.fs`
AI-facing contract shapes only.

Owns:
- `FeatureVector`
- `PolicyRecommendation`
- `ModeRecommendation`
- `ScenarioRanking`

These are contracts only, not ML implementations.

## 5.16 `DecisionCore/ImpactClassification.fs`
Impact classification for mode selection.

Owns:
- `ImpactLevel`
- trigger classification
- local/regional/global disruption semantics

This becomes the input to mode dispatch.

## 5.17 `DecisionCore/Provenance.fs`
Plan provenance is mandatory.

Owns:
- `PlanProvenance`

Contains:
- snapshot fingerprint
- policy fingerprint
- graph state id
- run id
- scenario id
- trigger source
- mode
- created utc

## 5.18 `DecisionCore/PlanningGraph.fs`
Immutable planning graph model.

Add:
- `PlanningNode`
- `PlanningEdge`
- `PlanningGraph`
- `GraphDelta`

Include, at minimum:
- material node
- inventory node
- demand node
- supply node
- capacity node
- transport node
- operation node

Implement:
- empty graph
- add node
- add edge
- apply delta
- index by node
- index by edge

Phase 1 should keep the graph as a decision model and indexing substrate.
Phase 2 expands traversal and propagation APIs for continuous planning.

## 5.19 `DecisionCore/CandidateRanking.fs`
Rank alternative candidates consistently.

Implement:
- rank by score
- rank by risk
- rank by service level
- rank by balanced objective

## 5.20 `DecisionCore/TelemetryContracts.fs`
Common telemetry shapes.

Add:
- `DecisionTelemetry`
- `PlanTelemetry`
- `PromiseTelemetry`
- `ReplanTelemetry`
- `OptimizationTelemetry`
- `PolicyTelemetry`
- `AIRecommendationTelemetry`

Telemetry contracts belong here.
Telemetry transport adapters do not.

---

# 6. PlanningEngine — file-by-file build order

Build PlanningEngine only after DecisionCore is in place.

## 6.1 `PlanningEngine/Domain/PlanningMode.fs`
Canonical planning modes:
- FastInsert
- IncrementalRepair
- FullReplan
- Optimization
- WhatIf
- Replenishment
- CapacityOnly
- MaterialOnly

## 6.2 `PlanningEngine/Domain/PlanningSnapshot.fs`
Snapshot aggregate with lock semantics.

Keep and extend with:
- policy set id
- graph version
- snapshot fingerprint
- source version vector
- lock reason
- scenario id
- current planning mode if useful for audit
- provenance reference

Commands:
- Create
- Lock
- Unlock
- Expire

## 6.3 `PlanningEngine/Domain/PlanningRun.fs`
Planning run lifecycle.

Keep and extend with:
- mode
- policy set id
- graph state id
- current phase
- phase timings
- failed phase
- result storage key
- run trigger type
- provenance reference

Commands:
- CreateRun
- StartRun
- AdvancePhase
- CompletePhase
- MarkRunCompleted
- FailRun
- FailInPhase
- CancelRun

## 6.4 `PlanningEngine/Domain/PlanVersion.fs`
Immutable committed planning version.

Extend with:
- fingerprint
- policy set id
- mode
- scenario variant id
- scorecard
- objective value
- explanation key
- graph state id
- provenance reference

## 6.5 `PlanningEngine/Domain/PlanIndex.fs`
Index for fast replan and explainability.

Include indexes for:
- proposal by id
- proposal by sku
- proposal by resource
- proposal by demand
- proposal by supplier
- proposal by lane
- proposal by inventory
- pegging by demand
- pegging by proposal
- demand by sku and stocking point
- resource by time bucket
- limiter by affected proposal

## 6.6 `PlanningEngine/Domain/PlanningResult.fs`
Output of a planning run.

Extend with:
- mode
- policy set id
- scenario variant id
- scorecard
- decision traces
- decision evidence
- decision narratives
- constraint bindings
- alternative candidates
- graph delta summary
- warnings and advisory notes
- provenance reference

## 6.7 `PlanningEngine/Domain/ScenarioDiff.fs`
Baseline vs variant comparison.

Implement:
- added
- removed
- changed
- unchanged
- KPI delta
- objective delta
- limiter delta
- churn delta
- lateness delta

## 6.8 `PlanningEngine/Shared/ImpactClassification.fs`
Trigger impact classification.

Implement:
- demand impact classifier
- supply impact classifier
- capacity impact classifier
- scenario impact classifier

Outputs `ImpactLevel` used by mode dispatch.

## 6.9 `PlanningEngine/Application/PlanningSnapshotBuilder.fs`
Single composition point for planning inputs.

Load from:
- Demand
- Supply
- Capacity
- MasterData
- Scenario overlays
- Transport data

This file must not contain planning rules.

## 6.10 `PlanningEngine/Application/ScenarioReadinessValidator.fs`
Hard gate before execution.

Validate:
- invalid horizons
- missing inputs
- stale version vectors
- locked conflict
- invalid policy
- invalid scenario state
- graph consistency preconditions
- no unsupported mode-policy combinations

## 6.11 `PlanningEngine/Application/PlanningPolicyResolver.fs`
Resolve the correct policy profile.

Inputs:
- trigger
- impact classification
- scenario state
- user preference
- service level requirement
- workload pressure
- AI suggestion, if available

Output:
- `PlanningPolicySet`

## 6.12 `PlanningEngine/Application/PlanningModeDispatcher.fs`
Choose the planning mode.

Rules:
- small insert -> FastInsert
- local disruption -> IncrementalRepair
- material shortfall -> Replenishment
- major baseline drift -> FullReplan
- objective improvement -> Optimization
- variant comparison -> WhatIf

## 6.13 `PlanningEngine/Application/PlanningOrchestrator.fs`
Top-level orchestration only.

Responsibilities:
- build snapshot
- resolve policy
- validate policy using PolicyGate
- lock snapshot
- create run
- dispatch mode
- persist version
- unlock snapshot
- emit telemetry

Do not put business decision logic here.

## 6.14 `PlanningEngine/Application/ExecutionPipeline.fs`
Common execution pipeline.

Steps:
1. validate snapshot
2. build graph or indexed representation
3. feasibility pre-check
4. run mode strategy
5. calculate scorecard
6. generate explainability
7. persist version
8. emit telemetry

## 6.15 `PlanningEngine/Application/MaterialPlanningPipeline.fs`
Refactor the current MRP logic here.

Must contain:
- preprocess
- forecast consumption
- grouping
- BOM explosion
- netting
- supply generation
- pegging
- postprocess

Each step should be a pure function plus a thin application adapter.

The conceptual logic remains; the internal data structures may evolve toward graph-backed traversal over time.

## 6.16 `PlanningEngine/Application/ReplanService.fs`
Disruption analysis and repair planning.

Use:
- `PlanIndex`
- `PlanningGraph`
- `ImpactLevel`

Implement:
- blast radius evaluation
- impacted demand selection
- impacted proposal selection
- local repair generation
- fallback to full replan
- churn and lateness KPI computation

## 6.17 `PlanningEngine/Application/ReplenishmentService.fs`
Stock-threshold based planning trigger.

Implement:
- target evaluation
- cooldown suppression
- duplicate trigger detection
- severity scoring
- trigger payload generation

Do not create supply orders here.
Replenishment should trigger PlanningEngine planning, not execution document creation.

## 6.18 `PlanningEngine/Application/OptimizationRunner.fs`
Solver integration.

Implement:
- model generation
- constraint assembly
- objective assembly
- warm start
- solver execution
- solution translation

This should be built using a functional solver contract, not a heavyweight interface-first design.

## 6.19 `PlanningEngine/Application/ScenarioVariantRunner.fs`
Scenario execution and comparison.

Implement:
- overlay application
- what-if run
- comparison
- ranking
- recommendation cards

## 6.20 `PlanningEngine/Application/PlanningExplainabilityBuilder.fs`
Decision trace generation for runs.

Implement:
- rationale assembly
- limiter binding summary
- alternative listing
- decision evidence
- decision narrative
- human-readable summary support
- AI-readable summary support

## 6.21 `PlanningEngine/Application/PlanningTelemetryPublisher.fs`
Structured APS telemetry.

Emit:
- plan run telemetry
- mode telemetry
- KPI telemetry
- explainability telemetry
- policy telemetry
- AI recommendation telemetry

## 6.22 `PlanningEngine/Infrastructure/EventStore/...`
Persistence adapters.

Implement:
- snapshot store
- run store
- version store
- plan result store
- telemetry store

---

# 7. MRP implementation structure

The current MRP pipeline should be refactored, not replaced.

## 7.1 Keep
- preprocess step
- forecast consumption step
- netting step
- supply generation step
- pegging step
- postprocess step

## 7.2 Refactor
Each step should be rewritten so that:
- its decision rules come from DecisionCore policies and scoring
- its trace output is standardized
- its inputs come from a PlanIndex in Phase 1 and, later, from the planning graph
- repeated global scans are eliminated

## 7.3 Required step order
1. preprocess
2. forecast consumption
3. BOM explosion
4. netting
5. supply generation
6. capacity check
7. pegging
8. postprocess

---

# 8. Promise integration

Promise remains separate.

## 8.1 Keep in Promise
- customer-facing request orchestration
- temporary reservations
- promise response generation
- promise-specific SLAs

## 8.2 Move to DecisionCore
- scoring
- limiter mapping
- feasibility math
- time-window math
- reservation semantics
- explainability contracts

## 8.3 Promise responsibilities
Promise should:
- call shared feasibility functions
- call shared scoring logic
- create tentative reservations
- return a promise decision with reason, confidence, and alternatives

Promise should not own planning run lifecycle or solver orchestration.

In the Promise application layer, create a thin feasibility service that wraps the pure DecisionCore calls. Promise orchestrators should not directly reference graph types; they call this service, which internally uses current snapshot data or, later, the continuous planner.

---

# 9. Scenario planning integration

Scenario remains a separate context.

## 9.1 Keep in Scenario
- scenario aggregate
- overlay set
- scenario configuration
- approval workflow

## 9.2 Add
- variant runner
- scenario diff generation
- scenario ranking
- baseline comparison

## 9.3 Rule
Scenario planning must call PlanningEngine in WhatIf or Optimization mode.
It must not duplicate planning logic.

---

# 10. AI and continuous planning

## 10.1 AI module structure
The `AI` module in PlanningEngine is an orchestration boundary for:
- policy suggestion intake
- ranking assistance
- explanation generation
- autonomy gating
- feedback loop ingestion

## 10.2 ContinuousPlanning module structure
The `ContinuousPlanning` module in PlanningEngine is a future concurrency layer for:
- live graph projection
- delta propagation
- fast feasibility checks
- near-real-time impact analysis

On startup, the live plan graph loads the most recent committed PlanningGraph from PlanningEngine’s version store, then subscribes to domain events from that point onward. This guarantees immediate continuity.

## 10.3 Future AI bounded context
Long term, AI should mature into a separate bounded context:

```text
AI
├── TelemetryIngestion
├── FeatureStore
├── ModelTraining
├── ModelRegistry
├── PolicySuggestion
├── ModeRecommendation
├── ReplenishmentLearning
├── ScenarioRanking
└── LLM
```

## 10.4 AI governance
AI may:
- suggest modes
- suggest policies
- rank variants
- summarize plans
- propose replenishment actions

AI may not:
- mutate plan state directly
- bypass hard constraints
- override locked orders without policy approval

## 10.5 Policy suggestions
Policy suggestions must carry:
- policy source
- confidence
- reasoning
- proposed policy set
- approval state

Policy sources include:
- manual
- organization default
- scenario override
- AI recommendation
- ML recommendation
- RL recommendation

## 10.6 Learning loop
The learning loop is:
- telemetry collected from plan runs
- analytics platform stores events
- models train on plan outcome data
- trained models publish suggestions
- PolicyGate validates suggestions
- planners approve or reject
- accepted suggestions become new defaults or recommended variants

---

# 11. PolicyGate and governance

PolicyGate is mandatory and separate from Policies.

It validates proposed policy adjustments against absolute safety boundaries.

## 11.1 PolicyGate checks
- max solver time
- max search budget
- max memory budget where applicable
- min safety stock
- max safety stock
- frozen horizon protection
- firm order protection
- hard constraint preservation
- maximum objective weight shift
- maximum policy delta
- approval requirements

## 11.2 PolicyGate outputs
- valid
- valid with warnings
- rejected with reasons

## 11.3 Rule
No AI-generated policy or human override can bypass PolicyGate.

---

# 12. AI/ML and LLM integration architecture

## 12.1 Telemetry collection
Each plan execution publishes `PlanTelemetry` containing:
- snapshot fingerprint
- policy set id and parameters
- plan scorecard
- planner overrides
- actual outcomes when joined later

## 12.2 Data platform
Telemetry is sent to an analytical data platform.
The concrete storage technology is an infrastructure choice, not an architectural dependency.

## 12.3 Model training
Python-based services train models periodically:
- policy tuner
- mode selector
- replenishment learner
- scenario ranking model

## 12.4 Suggestion publishing
Models publish `PlanningPolicySet` proposals and recommendation payloads.
These carry provenance and confidence.

## 12.5 AI policy adapter
PlanningEngine consumes recommendations through an adapter that:
- validates through PolicyGate
- stores audit trail
- exposes suggestion to orchestrator
- optionally auto-applies only within safe guardrails

## 12.6 LLM conversational planner
A separate LLM service provides:
- natural language what-if
- plan explanation
- recommendation cards

The LLM never mutates core plan state.
It only produces overlays or summaries.

## 12.7 Guardrailed autonomy
Autonomy levels:
- Advisory
- Guardrailed
- Autonomous

Any autonomous action requires an autonomy contract with:
- allowed actions
- max policy delta
- rollback rules
- approval requirements
- customer scope

---

# 13. Re-evaluation and validation rules

Every design choice must pass these questions:

1. Does it duplicate a shared semantic that should live in DecisionCore?
2. Does it force Promise and PlanningEngine to share orchestration?
3. Does it weaken deterministic planning?
4. Does it blur Phase 1 and Phase 2?
5. Does it weaken explainability?
6. Does it make optimization inconsistent with replanning?
7. Does it prevent AI governance and rollback?
8. Does it make future continuous planning harder?
9. Does it violate bounded-context separation?
10. Does it add hidden side effects to a pure domain concern?

If the answer to any is yes, the design must be adjusted.

---

# 14. Integration with existing code

## 14.1 Keep and extend
- `PlanningSnapshot` — add graph version, policy set id, fingerprint, scenario references, provenance reference
- `PlanningRun` — add mode, policy set id, graph state id, phase state, provenance reference
- `PlanVersion` — keep and enrich with metadata
- `ScenarioReadinessValidator` — adapt to graph and policy validation
- `ScenarioDiffService` — keep and base on result/graph deltas

## 14.2 Refactor
- `PlanningOrchestrator` — split into `SnapshotBuilder`, `ModeDispatcher`, `ExecutionPipeline`
- `MRP Pipeline` — preserve logic, refactor to use shared core and indexed/graph-backed traversal
- `ReplanService` — use `PlanIndex` and graph traversal, not repeated scans
- `ReplenishmentService` — trigger planning runs, not direct order creation
- `Promise` — remove duplicated scoring/limiter/feasibility logic and use DecisionCore

## 14.3 No structural change
- `Capacity`, `Supply`, `Demand`, `MasterData`, `Transport` remain data-providing bounded contexts

---

# 15. Implementation sequence

1. DecisionCore/Identities
2. DecisionCore/Fingerprints
3. DecisionCore/DecisionContext
4. DecisionCore/Provenance
5. DecisionCore/TimeWindows
6. DecisionCore/Normalization
7. DecisionCore/Reservations
8. DecisionCore/Limiters
9. DecisionCore/Scoring
10. DecisionCore/Feasibility
11. DecisionCore/Explainability
12. DecisionCore/Policies
13. DecisionCore/PolicyGate
14. DecisionCore/PolicySuggestions
15. DecisionCore/Autonomy
16. DecisionCore/AIContracts
17. DecisionCore/ImpactClassification
18. DecisionCore/PlanningGraph
19. DecisionCore/CandidateRanking
20. DecisionCore/TelemetryContracts
21. PlanningEngine/PlanningMode
22. PlanningEngine/PlanningSnapshot
23. PlanningEngine/PlanningRun
24. PlanningEngine/PlanVersion
25. PlanningEngine/PlanIndex
26. PlanningEngine/PlanningPolicyResolver
27. PlanningEngine/PlanningModeDispatcher
28. PlanningEngine/PlanningOrchestrator
29. PlanningEngine/ExecutionPipeline
30. PlanningEngine/MaterialPlanningPipeline
31. PlanningEngine/ReplanService
32. PlanningEngine/ReplenishmentService
33. PlanningEngine/OptimizationRunner
34. PlanningEngine/ScenarioVariantRunner
35. PlanningEngine/PlanningExplainabilityBuilder
36. PlanningEngine/PlanningTelemetryPublisher
37. Promise refactor to shared core
38. Scenario refactor to reuse PlanningEngine
39. ContinuousPlanning module
40. AI bounded context and learning loop
41. LLM service and scenario overlays

---

# 16. What must not be built as a hack

- no direct AI-to-plan mutation path
- no duplicated scoring logic in Promise or PlanningEngine
- no orphan policy objects without PolicyGate
- no replan logic that scans the world repeatedly when an index exists
- no optimization implementation that bypasses shared feasibility and scoring
- no scenario evaluator that duplicates PlanningEngine logic
- no planner state that lacks snapshot and policy provenance
- no interface-first architecture in the shared pure core unless there is a strong and explicit reason

---

# 17. Acceptance rules

A module is only acceptable if it:
- uses DecisionCore semantics,
- is deterministic where expected,
- has clear input/output contracts,
- is traceable,
- is testable,
- is rollback-safe,
- does not weaken the existing architecture,
- does not create a new source of truth for existing APS semantics.

---

# 18. Closing statement

This blueprint is the master document.
It is the source of truth for implementation sequencing, module boundaries, APS semantics, AI governance, and future extensibility.
