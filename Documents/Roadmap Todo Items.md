# Medhavi APS — Todo Items Roadmap v1.0

## How This Document Is Organised

Each phase is broken into categories that guarantee full coverage:

| Category | Source | What It Covers |
|----------|--------|-----------------|
| **Architecture & Infrastructure** | Architecture Blueprint (all chapters) | Shared libraries, event store, event bus, execution context, logging, telemetry, circuit breaker, configuration, etc. |
| **Semantic Foundation** | Each Intelligence Spec, Chapter 4 | Every `SE‑xxx` concept as an F# type, plus all enumerations as discriminated unions |
| **Measurement Model** | Each Intelligence Spec, Chapter 3 | Every `PI‑xxx` formula, business interpretation, worked‑example test, and report integration |
| **Capability: [name]** | Each Intelligence Spec, Chapter 5 | Every capability with its full anatomy, broken into sub‑categories below |
| ├─ **Enterprise Outputs** | Capability’s “Enterprise Outputs” table | Every `OUT‑xxx` record as a distinct type |
| ├─ **Decisions** | Capability’s “Business Decisions” section | Every `DE‑xxx` decision function |
| ├─ **Rules** | Each decision’s “Rules” table | Every `BR‑xxx` rule as a validation/derivation/calculation function |
| ├─ **Policies** | Each decision’s “Policies” table | Every `PO‑xxx` policy as a policy checker |
| ├─ **Commands** | Capability’s “Commands” table | Every command handler |
| ├─ **Events** | Capability’s “Events” table | Every event type |
| ├─ **Queries** | Capability’s “Queries” table | Every query service method |
| ├─ **Functional Behaviour** | Capability’s “Functional Behaviour” section | End‑to‑end orchestration logic |
| ├─ **Business Objectives** | Capability’s “Business Objectives Served” | Verification tasks linking implementation to `BO‑xxx` |
| ├─ **Reports** | Capability’s “Reports” sub‑section | Every `RPT‑xxx` report |
| └─ **Dashboards** | Capability’s “Dashboards” sub‑section | Every `DASH‑xxx` dashboard |
| **External Interfaces** | Intelligence Spec, Chapter 6 | Every API endpoint and integration event publisher/consumer |
| **Appendix** | Intelligence Spec, Chapter 8 | Exception Priority Matrix, Enterprise Glossary, Formula Reference |

---

## Phase 0 — Foundation: Todo Items

### 0.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P0‑001 | Create `Medhavi.Common` project (functional patterns, serialisation) | Arch. Blueprint §7.1 |
| P0‑002 | Create `Medhavi.SharedKernel` project — base domain types (`SkuId`, `Quantity`, `Timestamp`, `Version`, `Money`) | Arch. Blueprint §7.2 |
| P0‑003 | `DomainError` discriminated union (ValidationError, DomainError) | Arch. Blueprint §13.1, your existing `DomainError` |
| P0‑004 | `InfrastructureError` discriminated union (Network, Timeout, EventStore, Database, Http, CircuitOpen, OtherInfra) | Arch. Blueprint §13.1 |
| P0‑005 | `ApplicationError` discriminated union (Domain, NotFound, Mismatch, Infrastructure, External, Unknown) | Arch. Blueprint §13.1 |
| P0‑006 | `Envelope` type with all metadata fields | Arch. Blueprint §4.2 |
| P0‑007 | `ExecutionContext` type (`CorrelationId`, `CausationId`, `Principal`, `TenantId`, `MessageId`, `Timestamp`) | Arch. Blueprint §7.2.1, §11.3 |
| P0‑008 | `ExecutionContext` factory functions (`create`, `asCausation`, `fromEnvelope`, `toMetadataMap`) | Arch. Blueprint §7.2.1 |
| P0‑009 | `ExecutionContextHolder` with `AsyncLocal` propagation | Arch. Blueprint §7.2.1 |
| P0‑010 | Create `Medhavi.DecisionCore` pure F# library (zero dependencies) | Arch. Blueprint §7.3 |
| P0‑011 | `DecisionCore.Scoring` — `PlanScore`, `ScoreWeights`, `PlanScoreCard` types + `emptyScore`, `combineScores`, `weightedObjectiveScore`, `candidateRanking`, `cardComparison` | Arch. Blueprint §7.3.1 |
| P0‑012 | `DecisionCore.Feasibility` — `FeasibilityInput`, `FeasibilityResult` types + `checkATP`, `checkCTP`, `composeFeasibility`, `determineAcceptability` | Arch. Blueprint §7.3.2 |
| P0‑013 | `DecisionCore.Reservations` — `Reservation`, `ReservationScope`, `ReservationStatus` types + `createTentative`, `confirm`, `release`, `expire`, `reduce`, `validateLifecycle` | Arch. Blueprint §7.3.3 |
| P0‑014 | `DecisionCore.Fingerprints` — `SnapshotFingerprint`, `PolicyFingerprint`, `PlanFingerprint`, `GraphFingerprint` types + factory functions | Arch. Blueprint §7.3.4 |
| P0‑015 | `DecisionCore.PolicyGate` — `PolicyGateResult` type + `validatePolicy` with all checks (safety stock, frozen horizon, firm order, hard constraints, weight shifts, approval requirements) | Arch. Blueprint §7.3.5 |
| P0‑016 | `DecisionCore.Autonomy` — `AutonomyLevel`, `AutonomyContract` types + `createContract`, `validateAction`, `isWithinBoundary`, `expireContract` | Arch. Blueprint §7.3.6 |
| P0‑017 | `DecisionCore.TimeWindows` — `TimeWindow` type + `overlap`, `contains`, `intersection`, `expand`, `shift`, `slack`, `bucketAlign`, `leadTimeOffset` | Arch. Blueprint §7.3 |
| P0‑018 | `DecisionCore.PlanningGraph` — `PlanningNode`, `PlanningEdge` DUs + `empty`, `addNode`, `addEdge`, `applyDelta`, `indexByNode`, `indexByEdge` | Arch. Blueprint §7.3 |
| P0‑019 | `DecisionCore.Explainability` — `DecisionTrace`, `DecisionRationale`, `DecisionEvidence` types + trace builder | Arch. Blueprint §7.3 |
| P0‑020 | Create `Medhavi.Contracts` library — all DTOs, request/response types, integration event schemas for all five domains | Arch. Blueprint §7.4 |
| P0‑ARC‑021a | Create `Configuration` submodule in `Medhavi.SharedKernel` — `ArsIdentifiers.fs` with compile‑time constants for all ARS identifiers | Arch. Blueprint §16.2 |
| P0‑ARC‑021b | Implement `FeatureFlags` record type and loader in `SharedKernel.Configuration.FeatureFlags` | Arch. Blueprint §16.3 |
| P0‑ARC‑021c | Implement `AppSettings` types with validation in `SharedKernel.Configuration.EnvironmentSettings` | Arch. Blueprint §16.4 |
| P0‑022 | Create `Medhavi.Infrastructure` library — `Repository<'Aggregate, 'Id, 'Event>` interface | Arch. Blueprint §5.5 |
| P0‑023 | `EnvelopeStoreOps` interface with `Publish`, `ReadStream`, `ReadAll`, `Subscribe`, `GetLastRevision` | Arch. Blueprint §4.3 |
| P0‑024 | `ExpectedRevision` type (`Any`, `NoStream`, `StreamRevision`) | Arch. Blueprint §5.2 |
| P0‑025 | `InMemRepository` implementation | Arch. Blueprint §5.5.1 |
| P0‑026 | `InMemEnvelopeStore` implementation | Arch. Blueprint §5.5.1 |
| P0‑027 | `ProjectionAgent<'State, 'Event>` — MailboxProcessor with `Post`, `QueryAsync`, `SetState`, `GetStateAsync`, idempotency via `lastMessageId` | Arch. Blueprint §6.2 |
| P0‑028 | `Checkpoint` type and in‑memory `CheckpointStore` | Arch. Blueprint §4.5.1 |
| P0‑029 | `IdempotencyStore` with in‑memory implementation | Arch. Blueprint §4.5.2 |
| P0‑030 | `DomainEventBus` — in‑process publish/subscribe using F# `Event<T>` | Arch. Blueprint §4.4.1 |
| P0‑031 | `LogContext` type, `Logger` wrapper, `MailboxLogger`, `ComponentNaming` module | Arch. Blueprint §12.2 |
| P0‑032 | `Telemetry` module — `TelemetryEvent`, `TelemetrySeverity`, `createEvent`, `withCorrelation`, `withTracing`, `logEvent` | Arch. Blueprint §12.3 |
| P0‑033 | `Telemetry` domain‑specific types — `PlanningKpis`, `LimiterFrequency`, `LatencyTelemetry`, `TelemetryErrorMetric` | Arch. Blueprint §12.3.1 |
| P0‑034 | `Metrics` module — `recordCounter`, `recordGauge`, `recordHistogram` | Arch. Blueprint §12.3 |
| P0‑035 | `ActivityTracking` module — `startActivity`, `stopActivity`, `withActivity` (OpenTelemetry‑compatible) | Arch. Blueprint §12.4 |
| P0‑036 | `HealthCheck` module — `HealthStatus`, `ComponentHealth`, `createHealth`, `withResponseTime`, `addDetail` | Arch. Blueprint §12.6 |
| P0‑037 | `Performance` module — `PerformanceTracker`, `measure`, `measureAsync` | Arch. Blueprint §12.5 |
| P0‑038 | `ExceptionHandling` module — `ExceptionContext`, `RecoveryStrategy`, `executeWithErrorHandling`, unified with `ExecutionContext` | Arch. Blueprint §13.2 |
| P0‑039 | `CircuitBreaker` — MailboxProcessor agent with Closed/Open/HalfOpen, exponential backoff, `OnEvent` callback | Arch. Blueprint §13.3 |
| P0‑040 | Error‑to‑telemetry bridge — `reportErrorToTelemetry` function | Arch. Blueprint §13.6 |
| P0‑041 | `FeatureFlags` record type and environment‑variable loader | Arch. Blueprint §16.3 |
| P0‑042 | `AppSettings` typed config loader with validation | Arch. Blueprint §16.4 |

### 0.2 DecisionCore Unit Tests

| ID | Todo Item | Source |
|----|-----------|--------|
| P0‑TST‑001 | Scoring: empty score, combine, weighted objective with known inputs | Arch. Blueprint §7.3.1 |
| P0‑TST‑002 | Scoring: candidate ranking produces correct order | Arch. Blueprint §7.3.1 |
| P0‑TST‑003 | Feasibility: ATP feasible/infeasible, limiter generation | Arch. Blueprint §7.3.2 |
| P0‑TST‑004 | Feasibility: CTP feasible/infeasible, partial feasibility | Arch. Blueprint §7.3.2 |
| P0‑TST‑005 | Reservations: full lifecycle (create→confirm→release; create→expire) | Arch. Blueprint §7.3.3 |
| P0‑TST‑006 | Reservations: invalid transitions rejected | Arch. Blueprint §7.3.3 |
| P0‑TST‑007 | Fingerprints: determinism, collision resistance | Arch. Blueprint §7.3.4 |
| P0‑TST‑008 | PolicyGate: valid accepted, safety stock below min rejected, frozen horizon violation rejected, max weight shift exceeded | Arch. Blueprint §7.3.5 |
| P0‑TST‑009 | Autonomy: advisory cannot execute, guardrailed can execute permitted, guardrailed cannot execute disallowed, value threshold | Arch. Blueprint §7.3.6 |
| P0‑TST‑010 | TimeWindows: overlap, contains, slack, lead‑time offset | Arch. Blueprint §7.3 |
| P0‑TST‑011 | PlanningGraph: add/remove nodes, edges, immutability | Arch. Blueprint §7.3 |

---

## Phase 1 — Demand Intelligence: Todo Items

### 1.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P1‑ARC‑001 | Create `Medhavi.Demand` project with standard folder structure (Domain, Application, Projections) | Arch. Blueprint §3.3 |
| P1‑ARC‑002 | Register Demand bounded context in `Medhavi.Nexus` composition root | Arch. Blueprint §3.3 |
| P1‑ARC‑003 | Wire Demand event subscriptions to `DomainEventBus` for all Demand events | Arch. Blueprint §4.4.1 |

### 1.2 Semantic Foundation — Demand

**All SE‑DI‑xxx concepts implemented as F# types.**

| ID | Semantic Object | Type |
|----|-----------------|------|
| P1‑SEM‑001 | SE‑DI‑001 Demand | Record |
| P1‑SEM‑002 | SE‑DI‑002 Demand Signal | Record |
| P1‑SEM‑003 | SE‑DI‑003 Forecast | Record |
| P1‑SEM‑004 | SE‑DI‑004 Demand History | Record |
| P1‑SEM‑005 | SE‑DI‑005 Demand Plan | Record |
| P1‑SEM‑010 | SE‑DI‑010 Demand Quantity | Value object (decimal wrapper) |
| P1‑SEM‑011 | SE‑DI‑011 Demand Pattern | DU: Continuous, Intermittent, Lumpy, Seasonal, Trend, Stationary |
| P1‑SEM‑012 | SE‑DI‑012 Demand Variability | Value object (CV) |
| P1‑SEM‑013 | SE‑DI‑013 Demand Segmentation | Record (ABC, XYZ, Strategic) |
| P1‑SEM‑014 | SE‑DI‑014 Demand Priority | Record (score, level) |
| P1‑SEM‑015 | SE‑DI‑015 Demand Exception | Record (type, severity, affected items) |
| P1‑SEM‑020 | SE‑DI‑020 Forecast Model | Record (id, type, hyperparameters, trainedAt) |
| P1‑SEM‑021 | SE‑DI‑021 Prediction Interval | Record (lower, upper, confidence) |
| P1‑SEM‑022 | SE‑DI‑022 Forecast Confidence | Value object (0–100) |
| P1‑SEM‑023 | SE‑DI‑023 Forecast Override | Record (id, newValue, justification) |
| P1‑SEM‑024 | SE‑DI‑024 Forecast Cycle | Record (id, timestamp) |
| P1‑SEM‑025 | SE‑DI‑025 Forecast Horizon | Value object |
| P1‑SEM‑030 | SE‑DI‑030 Customer | Record (id, name, tier, channel, location) |
| P1‑SEM‑031 | SE‑DI‑031 Customer Tier | DU: Platinum, Gold, Silver, Bronze |
| P1‑SEM‑032 | SE‑DI‑032 Customer Channel | DU: Direct, Retail, ECommerce, Wholesale, Distributor |
| P1‑SEM‑033 | SE‑DI‑033 Ship‑To Location | Value object |
| P1‑SEM‑040 | SE‑DI‑040 Product | Record |
| P1‑SEM‑041 | SE‑DI‑041 Product Family | Value object |
| P1‑SEM‑042 | SE‑DI‑042 Product Life‑Cycle Stage | DU: Introduction, Growth, Maturity, Decline, EndOfLife |
| P1‑SEM‑043 | SE‑DI‑043 Substitutability | Record |
| P1‑SEM‑050 | SE‑DI‑050 Time Bucket | Value object |
| P1‑SEM‑051 | SE‑DI‑051 Planning Horizon | Value object |
| P1‑SEM‑052 | SE‑DI‑052 Lead Time | Value object |
| P1‑SEM‑053 | SE‑DI‑053 Frozen Period | Value object |
| P1‑SEM‑060 | SE‑DI‑060 Demand Aggregation | Relationship (function) |
| P1‑SEM‑061 | SE‑DI‑061 Demand Disaggregation | Relationship (function) |
| P1‑SEM‑062 | SE‑DI‑062 Demand Correlation | Relationship (function) |
| P1‑SEM‑063 | SE‑DI‑063 Demand Dependency | Relationship (function) |
| P1‑SEM‑064 | SE‑DI‑064 Demand Cannibalisation | Relationship (function) |

**Enumerations (as Discriminated Unions):**

| ID | Enumeration | Values |
|----|-------------|--------|
| P1‑ENUM‑001 | DemandPattern | Continuous, Intermittent, Lumpy, Seasonal, Trend, Stationary |
| P1‑ENUM‑002 | SignalType | FirmOrder, Quotation, PointOfSale, PromotionalCalendar, MarketIntelligence, WeatherForecast, SocialSentiment |
| P1‑ENUM‑003 | ForecastModelType | Statistical, MachineLearning, Judgemental, Hybrid, Naive |

### 1.3 Measurement Model — Demand

**Every Business Outcome Measure (PI‑DI‑002 through PI‑DI‑015) implemented as a pure computation function with worked‑example verification.**

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P1‑PI‑001 | PI‑DI‑001 | Reserved — define placeholder type. |
| P1‑PI‑002 | PI‑DI‑002 Forecast Accuracy | `computeForecastAccuracy(forecast, actuals) → decimal` + test against spec worked example |
| P1‑PI‑003 | PI‑DI‑003 WAPE | `computeWAPE(forecast, actuals) → decimal` + test against spec worked example |
| P1‑PI‑004 | PI‑DI‑004 MAPE | `computeMAPE(forecast, actuals) → decimal` + test against spec worked example |
| P1‑PI‑005 | PI‑DI‑005 Forecast Bias | `computeForecastBias(forecast, actuals) → decimal` + test against spec worked examples (zero‑bias and biased) |
| P1‑PI‑006 | PI‑DI‑006 FVA | `computeFVA(naiveWAPE, processWAPE) → decimal` + test against spec worked example (positive and negative FVA) |
| P1‑PI‑007 | PI‑DI‑007 Forecast Stability | `computeForecastStability(cycles) → decimal` + test against spec worked example |
| P1‑PI‑008 | PI‑DI‑008 Forecast Value Realization | `computeFVR(actualValue, maxPotential) → decimal` + test against spec worked example |
| P1‑PI‑009 | PI‑DI‑009 Demand Plan Adherence | `computeDemandPlanAdherence(plan, actuals, tolerance) → decimal` + test against spec worked example |
| P1‑PI‑010 | PI‑DI‑010 Service Level | `computeServiceLevel(fulfilledWithinWindow, totalDemanded) → decimal` + test against spec worked example |
| P1‑PI‑011 | PI‑DI‑011 Order Fill Rate | `computeOrderFillRate(orders) → decimal` + test against spec worked example |
| P1‑PI‑012 | PI‑DI‑012 OTIF | `computeOTIF(orderLines) → decimal` + test against spec worked example |
| P1‑PI‑013 | PI‑DI‑013 Perfect Order Rate | `computePerfectOrderRate(orders) → decimal` + test against spec worked example |
| P1‑PI‑014 | PI‑DI‑014 Customer Request Fulfilment Rate | `computeCRFR(requests) → decimal` + test against spec worked example |
| P1‑PI‑015 | PI‑DI‑015 Demand Satisfaction Rate | `computeDSR(satisfied, demanded) → decimal` + test against spec worked example |

### 1.4 Capability: CA‑DI‑001 — Understand Demand

**Specification:** Demand Intelligence Specification, Section 5.1

#### Enterprise Outputs

| ID | Output | Description |
|----|--------|-------------|
| P1‑OUT‑001 | OUT‑DI‑001 Inventory Position Snapshot | Implement type and publisher |
| P1‑OUT‑002 | OUT‑DI‑002 Open Supply Orders | Implement type and publisher |
| P1‑OUT‑003 | OUT‑DI‑003 Capacity Status | Implement type and publisher |
| P1‑OUT‑004 | OUT‑DI‑004 Supply Data Quality Score | Implement type and publisher |
| P1‑OUT‑005 | OUT‑DI‑005 Supplier Commitment Tracker | Implement type and publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P1‑DEC‑001 | DE‑DI‑010 Accept Demand Signal | `decideAcceptDemandSignal(signal, context) → Result<Event list, DomainError>` |
| P1‑DEC‑002 | DE‑DI‑011 Adjust Demand History | `decideAdjustDemandHistory(anomaly, context) → Result<Event list, DomainError>` |

#### Rules (for DE‑DI‑010)

| ID | Rule | Implementation |
|----|------|---------------|
| P1‑RUL‑001 | BR‑DI‑010 Signal Timeliness Rule | Validate signal timestamp within max latency |
| P1‑RUL‑002 | BR‑DI‑011 Signal Range Rule | Validate signal value within statistical bounds |
| P1‑RUL‑003 | BR‑DI‑012 Signal Source Reliability Rule | Quarantine signals from low‑reliability sources |

#### Rules (for DE‑DI‑011)

| ID | Rule | Implementation |
|----|------|---------------|
| P1‑RUL‑004 | BR‑DI‑013 Anomaly Adjustment Justification Rule | Every adjustment must have documented root cause |
| P1‑RUL‑005 | BR‑DI‑014 Adjustment Method Rule | Data error → median replacement; known event → retain with tag |
| P1‑RUL‑006 | BR‑DI‑016 Reconciliation Documentation Rule | (if present in spec) |

#### Policies (for DE‑DI‑010)

| ID | Policy | Implementation |
|----|--------|---------------|
| P1‑POL‑001 | PO‑DI‑010 Signal Acceptance Automation Policy | Auto‑accept if all rules pass and source reliability ≥90% |
| P1‑POL‑002 | PO‑DI‑011 Anomaly Adjustment Authorization Policy | Adjustments >50% of original value require Demand Manager approval |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P1‑CMD‑001 | `IngestDemandSignals` | Command handler |
| P1‑CMD‑002 | `AdjustDemandHistory` | Command handler |
| P1‑CMD‑003 | `RefreshDemandPicture` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P1‑EVT‑001 | `DemandSignalAccepted` | Event type + publisher |
| P1‑EVT‑002 | `DemandSignalQuarantined` | Event type + publisher |
| P1‑EVT‑003 | `DemandHistoryAdjusted` | Event type + publisher |
| P1‑EVT‑004 | `DemandPictureUpdated` | Event type + publisher |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P1‑QRY‑001 | `GetDemandHistory(product, location, start, end)` | Query service method |
| P1‑QRY‑002 | `GetCurrentDemandSnapshot(filters)` | Query service method |
| P1‑QRY‑003 | `GetSignalQualityReport(period)` | Query service method |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P1‑BEH‑001 | CA‑DI‑001 5.1.10 | Implement end‑to‑end flow: Ingest → Validate (DE‑DI‑010) → Aggregate → Detect outliers → Adjust (DE‑DI‑011) → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P1‑VFY‑001 | BO‑DI‑001 Deliver Trusted Demand Understanding | Verify demand picture is complete and trustworthy |
| P1‑VFY‑002 | BO‑DI‑003 Improve Enterprise Responsiveness | Verify demand changes are detected |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P1‑RPT‑001 | RPT‑DI‑001 Demand Data Quality Report | Report generation + data source |
| P1‑RPT‑002 | RPT‑DI‑002 Signal Source Performance Report | Report generation + data source |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P1‑DASH‑001 | DASH‑DI‑001 Demand Health Dashboard | UI dashboard + data binding |

### 1.5 Capability: CA‑DI‑002 — Forecast Demand

**Specification:** Demand Intelligence Specification, Section 5.2

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P1‑OUT‑010 | OUT‑DI‑010 Constrained Supply Plan (Note: this is Supply, not Demand — check spec; for Forecast Demand the outputs are forecast data) | If present, implement |
| P1‑OUT‑011 | OUT‑DI‑011 Supply‑Demand Balance Report | If present, implement |
| P1‑OUT‑012 | OUT‑DI‑012 Constraint Analysis | If present, implement |
| P1‑OUT‑013 | OUT‑DI‑013 Plan Confidence Score | If present, implement |

*(Note: I must verify the exact OUT‑xxx for Forecast Demand from the spec. In our earlier writing, Forecast Demand had outputs like forecast result sets. I'll check.)*

Actually, the Forecast Demand capability in Section 5.2 of our spec had these Enterprise Outputs:

| ID | Output | Description |
|----|--------|-------------|
| OUT‑DI‑010 | Forecast Result Set | Time‑series forecasts with intervals |
| OUT‑DI‑011 | Forecast Confidence Score | Per‑forecast confidence |
| OUT‑DI‑012 | Model Performance Summary | WAPE, bias per model |
| OUT‑DI‑013 | Forecast Cycle Metadata | Run ID, timestamp, model used |

So the tasks are:

| ID | Output | Implementation |
|----|--------|---------------|
| P1‑OUT‑010 | OUT‑DI‑010 Forecast Result Set | Type + publisher |
| P1‑OUT‑011 | OUT‑DI‑011 Forecast Confidence Score | Type + publisher |
| P1‑OUT‑012 | OUT‑DI‑012 Model Performance Summary | Type + publisher |
| P1‑OUT‑013 | OUT‑DI‑013 Forecast Cycle Metadata | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P1‑DEC‑010 | DE‑DI‑020 Select Champion Forecast Model | `decideSelectChampion(candidate, metrics) → Result<Event list, DomainError>` |
| P1‑DEC‑011 | DE‑DI‑021 Generate Baseline Forecast | `decideGenerateBaseline(plan, horizon) → Result<Event list, DomainError>` |
| P1‑DEC‑012 | DE‑DI‑022 Publish Forecast | `decidePublishForecast(cycleId, confidence) → Result<Event list, DomainError>` |
| P1‑DEC‑013 | DE‑DI‑023 Override Forecast | `decideOverrideForecast(forecast, newValue, justification) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P1‑RUL‑010 | BR‑DI‑020 Champion Selection Significance Rule | p ≤ 0.05 check |
| P1‑RUL‑011 | BR‑DI‑021 No Harm Rule | Bias increase ≤1pp, stability ≤5pp |
| P1‑RUL‑012 | BR‑DI‑022 High‑Priority Protection Rule | No WAPE increase >2pp on high‑priority items |
| P1‑RUL‑013 | BR‑DI‑023 Forecast Validity Rule | All forecasts ≥0 |
| P1‑RUL‑014 | BR‑DI‑024 Data Sufficiency Rule | Minimum 8 periods history |
| P1‑RUL‑015 | BR‑DI‑025 Prediction Interval Completeness Rule | 90% interval present |
| P1‑RUL‑016 | BR‑DI‑026 Forecast Completeness for Publication Rule | ≥95% mandatory items have forecasts |
| P1‑RUL‑017 | BR‑DI‑027 Override Justification Rule | Non‑empty justification |
| P1‑RUL‑018 | BR‑DI‑028 Override Deviation Limit Rule | Within ±50% of system forecast |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P1‑POL‑010 | PO‑DI‑020 Champion Promotion Approval Policy | Auto‑promote if all model evaluation rules pass |
| P1‑POL‑011 | PO‑DI‑021 Model Rollback Policy | Rollback permitted if service‑level drop >2% within 2 weeks |
| P1‑POL‑012 | PO‑DI‑022 Unforecastable Series Handling Policy | Assign naive forecast or planner placeholder |
| P1‑POL‑013 | PO‑DI‑023 Forecast Auto‑Publication Policy | Auto‑publish if confidence ≥90% and completeness rule passes |
| P1‑POL‑014 | PO‑DI‑024 Publication Override Policy | Demand Manager may force publish/suppress |
| P1‑POL‑015 | PO‑DI‑025 Forecast Override Authorization Policy | Only Demand Planner role may override |
| P1‑POL‑016 | PO‑DI‑026 Override Audit Policy | All overrides logged for quarterly review |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P1‑CMD‑010 | `StartForecastCycle` | Command handler |
| P1‑CMD‑011 | `SelectChampionModel` | Command handler |
| P1‑CMD‑012 | `OverrideForecast` | Command handler |
| P1‑CMD‑013 | `ApproveForecastPublication` | Command handler |
| P1‑CMD‑014 | `PublishForecast` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P1‑EVT‑010 | `ForecastCycleStarted` | Event type |
| P1‑EVT‑011 | `ForecastGenerated` | Event type |
| P1‑EVT‑012 | `ForecastOverridden` | Event type |
| P1‑EVT‑013 | `ForecastPublished` | Event type |
| P1‑EVT‑014 | `ForecastApprovalRequired` | Event type |
| P1‑EVT‑015 | `ModelChampionSelected` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P1‑QRY‑010 | `GetForecast(product, location, startDate, endDate)` | Query service |
| P1‑QRY‑011 | `GetModelPerformance(modelId, period)` | Query service |
| P1‑QRY‑012 | `GetForecastOverrides(cycleId)` | Query service |
| P1‑QRY‑013 | `GetPublicationStatus(cycleId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P1‑BEH‑010 | CA‑DI‑002 5.2.10 | Implement full flow: Trigger → Retrieve → Evaluate challengers (DE‑DI‑020) → Generate (DE‑DI‑021) → Overrides (DE‑DI‑023) → Publish (DE‑DI‑022) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P1‑VFY‑010 | BO‑DI‑001 Deliver Trusted Demand Understanding | Verify forecast is accurate and explainable |
| P1‑VFY‑011 | BO‑DI‑002 Improve Planning Effectiveness | Verify forecast supports downstream planning |
| P1‑VFY‑012 | BO‑DI‑005 Increase Planning Automation | Verify auto‑publication and auto‑champion selection work |
| P1‑VFY‑013 | BO‑DI‑006 Continuously Improve Enterprise Intelligence | Verify model performance is tracked |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P1‑RPT‑010 | RPT‑DI‑003 Forecast Accuracy Report | Report generation |
| P1‑RPT‑011 | RPT‑DI‑004 FVA Report | Report generation |
| P1‑RPT‑012 | RPT‑DI‑005 Model Champion Report | Report generation |
| P1‑RPT‑013 | RPT‑DI‑006 Override Analysis Report | Report generation |
| P1‑RPT‑014 | RPT‑DI‑007 Planner Performance Scorecard | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P1‑DASH‑010 | DASH‑DI‑002 Forecast Performance Dashboard | UI dashboard |
| P1‑DASH‑011 | DASH‑DI‑003 Forecast Confidence Dashboard | UI dashboard |
| P1‑DASH‑012 | DASH‑DI‑004 Planner Override Monitor | UI dashboard |

### 1.6 Remaining Demand Capabilities (Summary)

The same exhaustive pattern applies to every remaining Demand capability:

- **CA‑DI‑004 Segment Demand** — 4 decisions (DE‑DI‑040–043), 8 rules (BR‑DI‑040–047), 4 policies (PO‑DI‑040–043), 3 commands, 3 events, 3 queries, 2 reports, 2 dashboards, functional behaviour, business objectives verification
- **CA‑DI‑005 Classify Demand** — 2 decisions (DE‑DI‑050–051), 5 rules (BR‑DI‑050–054), 2 policies (PO‑DI‑050–051), 3 commands, 3 events, 3 queries, 2 reports, 2 dashboards, functional behaviour, business objectives verification
- **CA‑DI‑006 Prioritize Demand** — 2 decisions (DE‑DI‑060–061), 4 rules (BR‑DI‑060–063), 3 policies (PO‑DI‑060–062), 3 commands, 3 events, 3 queries, 2 reports, 2 dashboards, functional behaviour, business objectives verification
- **CA‑DI‑007 Evaluate Demand Quality** — 4 decisions (DE‑DI‑070–073), 8 rules (BR‑DI‑070–077), 4 policies (PO‑DI‑070–073), 4 commands, 4 events, 4 queries, 3 reports, 3 dashboards, functional behaviour, business objectives verification
- **CA‑DI‑009 Explain Demand Decisions** — 3 decisions (DE‑DI‑090–092), 6 rules (BR‑DI‑090–095), 4 policies (PO‑DI‑090–093), 4 commands, 3 events, 4 queries, 2 reports, 2 dashboards, functional behaviour, business objectives verification

### 1.7 External Interfaces — Demand

| ID | Endpoint | Method | Path | Owner Capability |
|----|----------|--------|------|------------------|
| P1‑API‑001 | Demand Signal Ingestion | POST | `/api/v1/demand/signals` | CA‑DI‑001 |
| P1‑API‑002 | Demand History Query | GET | `/api/v1/demand/history` | CA‑DI‑001 |
| P1‑API‑003 | Forecast Query | GET | `/api/v1/forecasts` | CA‑DI‑002 |
| P1‑API‑004 | Forecast Override | POST | `/api/v1/forecasts/overrides` | CA‑DI‑002 |
| P1‑API‑005 | Segmentation & Classification | GET | `/api/v1/demand/metadata/{productId}` | CA‑DI‑004, CA‑DI‑005 |
| P1‑API‑006 | Demand Quality Report | GET | `/api/v1/demand/quality/report` | CA‑DI‑007 |
| P1‑API‑007 | Demand Explanation | GET | `/api/v1/demand/explanations/{artifactId}` | CA‑DI‑009 |

**Integration Events — Published:**

| ID | Event | Publisher |
|----|-------|-----------|
| P1‑INT‑001 | `DemandSignalAccepted` | CA‑DI‑001 |
| P1‑INT‑002 | `DemandPictureUpdated` | CA‑DI‑001 |
| P1‑INT‑003 | `ForecastGenerated` | CA‑DI‑002 |
| P1‑INT‑004 | `ForecastPublished` | CA‑DI‑002 |
| P1‑INT‑005 | `SegmentMasterPublished` | CA‑DI‑004 |
| P1‑INT‑006 | `DemandPatternClassified` | CA‑DI‑005 |
| P1‑INT‑007 | `PriorityListPublished` | CA‑DI‑006 |
| P1‑INT‑008 | `QualityReportPublished` | CA‑DI‑007 |
| P1‑INT‑009 | `DecisionExplanationGenerated` | CA‑DI‑009 |

### 1.8 Appendix — Demand

| ID | Item | Implementation |
|----|------|---------------|
| P1‑APP‑001 | Demand Exception Priority Matrix | Configuration document / runtime config |
| P1‑APP‑002 | Demand Enterprise Glossary | Documentation |
| P1‑APP‑003 | Demand Formula Reference | Documentation |
| P1‑APP‑004 | Demand References | Documentation |

---

We continue with the exhaustive Phase 2 Todo Items, following exactly the same pattern established in Phase 1. Every capability from the Supply Intelligence Specification is broken down into its complete anatomy—Enterprise Outputs, Decisions, Rules, Policies, Commands, Events, Queries, Functional Behaviour, Business Objectives verification, Reports, and Dashboards—plus Semantic Foundation, Measurement Model, External Interfaces, and Appendix.

---

## Phase 2 — Supply Intelligence + Planning Engine: Todo Items

### 2.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P2‑ARC‑001 | Create `Medhavi.Supply` project with standard folder structure (Domain, Application, Projections) | Arch. Blueprint §3.3 |
| P2‑ARC‑002 | Create `Medhavi.PlanningEngine` module within Supply (or as shared service) | Arch. Blueprint §9.1 |
| P2‑ARC‑003 | Register Supply bounded context and Planning Engine in `Medhavi.Nexus` composition root | Arch. Blueprint §3.3 |
| P2‑ARC‑004 | Wire Supply event subscriptions to `DomainEventBus` (consume Demand events, publish Supply events) | Arch. Blueprint §4.4.1, §4.7 |
| P2‑ARC‑005 | Implement `PlanIndex` for fast lookups (by SKU, resource, demand, supplier) | Arch. Blueprint §9.3 |

### 2.2 Semantic Foundation — Supply

Every `SE‑SI‑xxx` concept from Chapter 4 of the Supply Intelligence Specification implemented as an F# type.

| ID | Semantic Object | Type |
|----|-----------------|------|
| P2‑SEM‑001 | SE‑SI‑001 Supply | Record |
| P2‑SEM‑002 | SE‑SI‑002 Supply Plan | Record |
| P2‑SEM‑003 | SE‑SI‑003 Inventory | Record |
| P2‑SEM‑004 | SE‑SI‑004 Capacity | Record |
| P2‑SEM‑005 | SE‑SI‑005 Supplier | Record |
| P2‑SEM‑010 | SE‑SI‑010 Planned Supply Quantity | Value object |
| P2‑SEM‑011 | SE‑SI‑011 Supply Plan Horizon | Value object |
| P2‑SEM‑012 | SE‑SI‑012 Supply Constraint | Record |
| P2‑SEM‑013 | SE‑SI‑013 Supply Variability | Value object |
| P2‑SEM‑014 | SE‑SI‑014 Supply Lead Time | Value object |
| P2‑SEM‑020 | SE‑SI‑020 Inventory Position | Record |
| P2‑SEM‑021 | SE‑SI‑021 Safety Stock | Value object |
| P2‑SEM‑022 | SE‑SI‑022 Reorder Point | Value object |
| P2‑SEM‑023 | SE‑SI‑023 Economic Order Quantity | Value object |
| P2‑SEM‑024 | SE‑SI‑024 Inventory Policy | Record |
| P2‑SEM‑025 | SE‑SI‑025 Excess Inventory | Record |
| P2‑SEM‑026 | SE‑SI‑026 Obsolete Inventory | Record |
| P2‑SEM‑030 | SE‑SI‑030 Capacity Bucket | Record |
| P2‑SEM‑031 | SE‑SI‑031 Resource | Record |
| P2‑SEM‑032 | SE‑SI‑032 Bottleneck | Record |
| P2‑SEM‑033 | SE‑SI‑033 Throughput | Value object |
| P2‑SEM‑034 | SE‑SI‑034 Capacity Utilization | Value object |
| P2‑SEM‑035 | SE‑SI‑035 Capacity Strategy | DU: Level, Chase, Hybrid |
| P2‑SEM‑040 | SE‑SI‑040 Supplier Performance | Record |
| P2‑SEM‑041 | SE‑SI‑041 Supplier Commitment | Record |
| P2‑SEM‑042 | SE‑SI‑042 Supplier Lead Time | Value object |
| P2‑SEM‑043 | SE‑SI‑043 Supplier Capacity | Record |
| P2‑SEM‑044 | SE‑SI‑044 Supplier Contract | Record |
| P2‑SEM‑050 | SE‑SI‑050 Purchase Requisition | Record |
| P2‑SEM‑051 | SE‑SI‑051 Purchase Order | Record |
| P2‑SEM‑052 | SE‑SI‑052 Procurement Policy | Record |
| P2‑SEM‑053 | SE‑SI‑053 Procurement Lead Time | Value object |
| P2‑SEM‑060 | SE‑SI‑060 Bill of Materials | Record |
| P2‑SEM‑061 | SE‑SI‑061 Routing | Record |
| P2‑SEM‑062 | SE‑SI‑062 Production Order | Record |
| P2‑SEM‑063 | SE‑SI‑063 Production Schedule | Record |
| P2‑SEM‑064 | SE‑SI‑064 Work Center | Record |
| P2‑SEM‑065 | SE‑SI‑065 Changeover | Record |
| P2‑SEM‑070 | SE‑SI‑070 Distribution Network | Record |
| P2‑SEM‑071 | SE‑SI‑071 Transfer Order | Record |
| P2‑SEM‑072 | SE‑SI‑072 Allocation Rule (Supply) | Record |
| P2‑SEM‑073 | SE‑SI‑073 Distribution Lead Time | Value object |
| P2‑SEM‑080 | SE‑SI‑080 Supply‑Demand Balancing | Relationship (function) |
| P2‑SEM‑081 | SE‑SI‑081 Substitutability (Supply) | Relationship (function) |
| P2‑SEM‑082 | SE‑SI‑082 Co‑Products | Relationship (function) |
| P2‑SEM‑083 | SE‑SI‑083 Supply Network | Relationship (function) |
| P2‑SEM‑084 | SE‑SI‑084 Dependency (Supply) | Relationship (function) |

**Enumerations (as Discriminated Unions):**

| ID | Enumeration | Values |
|----|-------------|--------|
| P2‑ENUM‑001 | SupplyPlanType | Strategic, Tactical, Operational |
| P2‑ENUM‑002 | InventoryPolicyType | PeriodicReview, ContinuousReview(s,Q), ContinuousReview(s,S), MinMax, LotForLot |
| P2‑ENUM‑003 | CapacityStrategy | Level, Chase, Hybrid |
| P2‑ENUM‑004 | SupplyExceptionType | Shortage, Excess, LateDelivery, CapacityViolation, QualityFailure, DataGap |

### 2.3 Measurement Model — Supply (MVP Scope)

Every Business Outcome Measure implemented as a pure function with worked‑example verification.

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P2‑PI‑001 | PI‑SI‑001 Supply Intelligence Effectiveness | Reserved — placeholder type |
| P2‑PI‑002 | PI‑SI‑002 Inventory Turnover | `computeInventoryTurnover(cogs, avgInventory) → decimal` + test against spec worked example |
| P2‑PI‑003 | PI‑SI‑003 Days of Supply | `computeDaysOfSupply(onHand, avgDailyDemand) → decimal` + test against spec worked example (both adequate and very‑lean cases) |
| P2‑PI‑004 | PI‑SI‑004 Fill Rate (Supply) | `computeFillRate(fulfilled, requested) → decimal` + test against spec worked example |
| P2‑PI‑005 | PI‑SI‑005 Capacity Utilization | `computeCapacityUtilization(actualOutput, maxCapacity) → decimal` + test against spec worked example (optimal and over‑utilised cases) |
| P2‑PI‑006 | PI‑SI‑006 Schedule Adherence | `computeScheduleAdherence(ordersCompletedOTIF, totalScheduled) → decimal` + test against spec worked example |
| P2‑PI‑010 | PI‑SI‑010 Supply Plan Adherence | `computeSupplyPlanAdherence(executedPerPlan, totalPlanned) → decimal` + test against spec worked example |
| P2‑PI‑011 | PI‑SI‑011 Backorder Rate | `computeBackorderRate(backordered, totalRequested) → decimal` + test against spec worked example |
| P2‑PI‑012 | PI‑SI‑012 Stockout Frequency | `computeStockoutFrequency(events) → int` + test against spec worked example |
| P2‑PI‑013 | PI‑SI‑013 Excess & Obsolete Inventory | `computeEandO(excessObsoleteValue, totalInventoryValue) → decimal` + test against spec worked example |
| P2‑PI‑015 | PI‑SI‑015 Cash‑to‑Cash Cycle Time | `computeCashToCash(daysInventory, daysSales, daysPayable) → decimal` + test against spec worked example |

(PI‑SI‑007, PI‑SI‑008, PI‑SI‑009, PI‑SI‑014 are deferred to Phase 9.)

### 2.4 Capability: CA‑SI‑001 — Understand Supply

**Specification:** Supply Intelligence Specification, Section 5.1

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑001 | OUT‑SI‑001 Inventory Position Snapshot | Type + publisher |
| P2‑OUT‑002 | OUT‑SI‑002 Open Supply Orders | Type + publisher |
| P2‑OUT‑003 | OUT‑SI‑003 Capacity Status | Type + publisher |
| P2‑OUT‑004 | OUT‑SI‑004 Supply Data Quality Score | Type + publisher |
| P2‑OUT‑005 | OUT‑SI‑005 Supplier Commitment Tracker | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑001 | DE‑SI‑010 Accept Supply Data | `decideAcceptSupplyData(supplyData, context) → Result<Event list, DomainError>` |
| P2‑DEC‑002 | DE‑SI‑011 Reconcile Inventory Position | `decideReconcileInventory(position, physicalCount) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑010)

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑001 | BR‑SI‑010 Supply Data Timeliness Rule | Validate inventory data not older than max latency |
| P2‑RUL‑002 | BR‑SI‑011 Supply Data Range Rule | Non‑negative inventory, positive order quantities |
| P2‑RUL‑003 | BR‑SI‑012 Supply Data Source Reliability Rule | Quarantine data from sources with reliability <70% |
| P2‑RUL‑004 | BR‑SI‑013 Duplicate Detection Rule (Supply) | Reject duplicate transactions within 24‑hour window |

#### Rules (for DE‑SI‑011)

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑005 | BR‑SI‑014 Inventory Reconciliation Threshold Rule | Adjust if discrepancy >2% AND >10 units |
| P2‑RUL‑006 | BR‑SI‑015 Recurring Discrepancy Rule | Flag item as “suspect” if discrepancy recurs in consecutive cycles |
| P2‑RUL‑007 | BR‑SI‑016 Reconciliation Documentation Rule | Every adjustment must record reason, authority, and date |

#### Policies (for DE‑SI‑010)

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑001 | PO‑SI‑010 Supply Data Acceptance Automation Policy | Auto‑accept if all validation rules pass and source reliability ≥90% |

#### Policies (for DE‑SI‑011)

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑002 | PO‑SI‑011 Reconciliation Approval Policy | Adjustments >5% of inventory value require Supply Manager approval |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑001 | `IngestSupplyData` | Command handler |
| P2‑CMD‑002 | `ReconcileInventory` | Command handler |
| P2‑CMD‑003 | `RefreshSupplyPicture` | Command handler |
| P2‑CMD‑004 | `FlagSupplyDataIssue` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑001 | `SupplyDataAccepted` | Event type |
| P2‑EVT‑002 | `SupplyDataQuarantined` | Event type |
| P2‑EVT‑003 | `InventoryPositionUpdated` | Event type |
| P2‑EVT‑004 | `InventoryReconciled` | Event type |
| P2‑EVT‑005 | `SupplyPictureUpdated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑001 | `GetInventoryPosition(product, location)` | Query service |
| P2‑QRY‑002 | `GetOpenOrders(filter)` | Query service |
| P2‑QRY‑003 | `GetCapacityStatus(resource)` | Query service |
| P2‑QRY‑004 | `GetSupplyDataQuality(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑001 | CA‑SI‑001 5.1.13 | Implement full flow: Ingest → Validate (DE‑SI‑010) → Aggregate → Reconcile (DE‑SI‑011) → Compute quality → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑001 | BO‑SI‑001 Deliver Trusted Supply Understanding | Verify supply picture is complete and trustworthy |
| P2‑VFY‑002 | BO‑SI‑003 Maximize Capacity Utilization | Verify capacity status is accurate |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑001 | RPT‑SI‑001 Supply Data Quality Report | Report generation |
| P2‑RPT‑002 | RPT‑SI‑002 Inventory Reconciliation Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑001 | DASH‑SI‑001 Supply Health Dashboard | UI dashboard |
| P2‑DASH‑002 | DASH‑SI‑002 Supply Data Quality Monitor | UI dashboard |

### 2.5 Capability: CA‑SI‑002 — Plan Supply

**Specification:** Supply Intelligence Specification, Section 5.2

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑010 | OUT‑SI‑010 Constrained Supply Plan | Type + publisher |
| P2‑OUT‑011 | OUT‑SI‑011 Supply‑Demand Balance Report | Type + publisher |
| P2‑OUT‑012 | OUT‑SI‑012 Constraint Analysis | Type + publisher |
| P2‑OUT‑013 | OUT‑SI‑013 Plan Confidence Score | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑010 | DE‑SI‑020 Select Supply Planning Model | `decideSelectPlanningModel(horizon, constraints, timeBudget) → Result<Event list, DomainError>` |
| P2‑DEC‑011 | DE‑SI‑021 Generate Supply Plan | `decideGenerateSupplyPlan(planInputs) → Result<Event list, DomainError>` |
| P2‑DEC‑012 | DE‑SI‑022 Evaluate Supply Plan Quality | `decideEvaluatePlanQuality(plan, targets) → Result<Event list, DomainError>` |
| P2‑DEC‑013 | DE‑SI‑023 Publish Supply Plan | `decidePublishSupplyPlan(planVersion) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑010 | BR‑SI‑020 Planning Model Selection Rule | Select model by horizon (Operational→Heuristic, Tactical→LP, Strategic→Aggregate LP) |
| P2‑RUL‑011 | BR‑SI‑021 Model Performance Monitoring Rule | Trigger review if plan cost deviates >5% from optimal benchmark |
| P2‑RUL‑012 | BR‑SI‑022 Hard Constraint Rule | Plan must not violate hard constraints (capacity, BOM, lead times) |
| P2‑RUL‑013 | BR‑SI‑023 Soft Constraint Documentation Rule | Soft constraint violations must be documented |
| P2‑RUL‑014 | BR‑SI‑024 Plan Completeness Rule | Plan must cover ≥95% of active product‑locations |
| P2‑RUL‑015 | BR‑SI‑025 Plan Quality Threshold Rule | Service level ≥95%, capacity utilization ≤98%, cost within 105% of budget |
| P2‑RUL‑016 | BR‑SI‑026 Plan Stability Rule | New plan must not deviate >20% from prior for first 4 weeks unless confirmed demand change |
| P2‑RUL‑017 | BR‑SI‑027 Publication Authorization Rule | Plan may only be published if accepted or approved |
| P2‑RUL‑018 | BR‑SI‑028 Versioning Rule | Every published plan receives unique version identifier, stored immutably |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑010 | PO‑SI‑020 Model Selection Override Policy | Supply Planning Manager may override with documented justification |
| P2‑POL‑011 | PO‑SI‑021 Infeasibility Escalation Policy | Immediate notification if hard constraints cannot be satisfied |
| P2‑POL‑012 | PO‑SI‑022 Plan Acceptance Automation Policy | Auto‑accept if all quality thresholds met |
| P2‑POL‑013 | PO‑SI‑023 Plan Publication Policy | Published by 10:00 AM Monday (operational) / 5th business day (tactical) |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑010 | `StartSupplyPlanningCycle` | Command handler |
| P2‑CMD‑011 | `SelectPlanningModel` | Command handler |
| P2‑CMD‑012 | `GenerateSupplyPlan` | Command handler |
| P2‑CMD‑013 | `EvaluateSupplyPlan` | Command handler |
| P2‑CMD‑014 | `PublishSupplyPlan` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑010 | `SupplyPlanGenerated` | Event type |
| P2‑EVT‑011 | `SupplyPlanEvaluated` | Event type |
| P2‑EVT‑012 | `SupplyPlanPublished` | Event type |
| P2‑EVT‑013 | `SupplyPlanInfeasible` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑010 | `GetSupplyPlan(cycleId, product, location)` | Query service |
| P2‑QRY‑011 | `GetSupplyDemandBalance(cycleId)` | Query service |
| P2‑QRY‑012 | `GetConstraintAnalysis(cycleId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑010 | CA‑SI‑002 5.2.13 | Implement full flow: Trigger → Retrieve → Select model (DE‑SI‑020) → Generate (DE‑SI‑021) → Evaluate (DE‑SI‑022) → Publish (DE‑SI‑023) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑010 | BO‑SI‑001 Deliver Trusted Supply Understanding | Verify supply plan is feasible and trustworthy |
| P2‑VFY‑011 | BO‑SI‑002 Optimize Inventory Performance | Verify supply plan balances demand and supply |
| P2‑VFY‑012 | BO‑SI‑003 Maximize Capacity Utilization | Verify capacity constraints are respected |
| P2‑VFY‑013 | BO‑SI‑005 Minimize Total Delivered Cost | Verify plan cost is within targets |
| P2‑VFY‑014 | BO‑SI‑007 Increase Planning Automation | Verify auto‑acceptance and publication work |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑010 | RPT‑SI‑003 Supply Plan Accuracy Report | Report generation |
| P2‑RPT‑011 | RPT‑SI‑004 Constraint Utilization Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑010 | DASH‑SI‑003 Supply Plan Dashboard | UI dashboard |
| P2‑DASH‑011 | DASH‑SI‑004 Constraint Monitor | UI dashboard |

### 2.6 Capability: CA‑SI‑003 — Manage Inventory

**Specification:** Supply Intelligence Specification, Section 5.3

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑020 | OUT‑SI‑020 Inventory Policy Set | Type + publisher |
| P2‑OUT‑021 | OUT‑SI‑021 Inventory Projection | Type + publisher |
| P2‑OUT‑022 | OUT‑SI‑022 Replenishment Recommendation | Type + publisher |
| P2‑OUT‑023 | OUT‑SI‑023 Inventory Health Status | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑020 | DE‑SI‑030 Set Inventory Policy | `decideSetInventoryPolicy(item, parameters) → Result<Event list, DomainError>` |
| P2‑DEC‑021 | DE‑SI‑031 Generate Replenishment Recommendation | `decideGenerateReplenishment(position, policy) → Result<Event list, DomainError>` |
| P2‑DEC‑022 | DE‑SI‑032 Assess Inventory Health | `decideAssessInventoryHealth(position, coverage) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑020 | BR‑SI‑030 Safety Stock Calculation Rule | SS = Z × σ × √L; simulation‑based for CV > 1.0 |
| P2‑RUL‑021 | BR‑SI‑031 Lot Size Calculation Rule | EOQ for CV < 0.5; periodic review otherwise |
| P2‑RUL‑022 | BR‑SI‑032 Policy Consistency Rule | Parameters must not change >±20% without structural demand/supply change |
| P2‑RUL‑023 | BR‑SI‑033 Reorder Point Rule | Order if projected position ≤ reorder point at end of lead time |
| P2‑RUL‑024 | BR‑SI‑034 Lot Sizing Rule | Order quantity = defined lot size; expediting may increase |
| P2‑RUL‑025 | BR‑SI‑035 Excess Inventory Action Rule | If projected exceeds max target, recommend defer/cancel |
| P2‑RUL‑026 | BR‑SI‑036 Inventory Health Classification Rule | Under‑stocked: <50% target; Over‑stocked: >200% target; Obsolete: no demand in 12 months |
| P2‑RUL‑027 | BR‑SI‑037 Obsolete Inventory Review Rule | Quarterly review of obsolete items |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑020 | PO‑SI‑030 Inventory Policy Override Policy | Supply Planner may override with justification; tracked monthly |
| P2‑POL‑021 | PO‑SI‑031 Replenishment Automation Policy | Auto‑convert for low‑variability (XYZ‑X) with high confidence; others require planner approval |
| P2‑POL‑022 | PO‑SI‑032 Expediting Authorization Policy | Expediting requests above cost threshold require Supply Manager approval |
| P2‑POL‑023 | PO‑SI‑033 Overstock Disposition Policy | Over‑stocked items above value threshold require disposition plan within 30 days |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑020 | `SetInventoryPolicy` | Command handler |
| P2‑CMD‑021 | `GenerateReplenishment` | Command handler |
| P2‑CMD‑022 | `AssessInventoryHealth` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑020 | `InventoryPolicyUpdated` | Event type |
| P2‑EVT‑021 | `ReplenishmentRecommended` | Event type |
| P2‑EVT‑022 | `InventoryHealthAlert` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑020 | `GetInventoryPolicy(product, location)` | Query service |
| P2‑QRY‑021 | `GetReplenishmentRecommendations(filter)` | Query service |
| P2‑QRY‑022 | `GetInventoryHealth(scope)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑020 | CA‑SI‑003 5.3.13 | Implement: Scheduled/daily → Retrieve → Set policy (DE‑SI‑030) → Generate replenishment (DE‑SI‑031) → Assess health (DE‑SI‑032) → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑020 | BO‑SI‑002 Optimize Inventory Performance | Verify inventory policies balance service and cost |
| P2‑VFY‑021 | BO‑SI‑004 Ensure Supply Continuity | Verify replenishment prevents stockouts |
| P2‑VFY‑022 | BO‑SI‑007 Increase Planning Automation | Verify auto‑replenishment works |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑020 | RPT‑SI‑005 Inventory Policy Compliance Report | Report generation |
| P2‑RPT‑021 | RPT‑SI‑006 Replenishment Action Report | Report generation |
| P2‑RPT‑022 | RPT‑SI‑007 Inventory Health Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑020 | DASH‑SI‑005 Inventory Optimization Dashboard | UI dashboard |
| P2‑DASH‑021 | DASH‑SI‑006 Replenishment Workbench | UI dashboard |

### 2.7 Capability: CA‑SI‑004 — Manage Capacity

**Specification:** Supply Intelligence Specification, Section 5.4

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑030 | OUT‑SI‑030 Capacity Load Profile | Type + publisher |
| P2‑OUT‑031 | OUT‑SI‑031 Bottleneck Report | Type + publisher |
| P2‑OUT‑032 | OUT‑SI‑032 Capacity Adjustment Recommendations | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑030 | DE‑SI‑040 Assess Capacity Feasibility | `decideAssessCapacityFeasibility(load, availability) → Result<Event list, DomainError>` |
| P2‑DEC‑031 | DE‑SI‑041 Publish Capacity Plan | `decidePublishCapacityPlan(plan) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑030 | BR‑SI‑040 Capacity Overload Rule | Resolution required if utilization >100% in any bucket |
| P2‑RUL‑031 | BR‑SI‑041 Underload Alert Rule | Alert if utilization <50% for 4 consecutive weeks |
| P2‑RUL‑032 | BR‑SI‑042 Bottleneck Impact Rule | Identify bottleneck resource and quantify throughput impact |
| P2‑RUL‑033 | BR‑SI‑043 Capacity Plan Publication Rule | Must not publish with unresolved overload |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑030 | PO‑SI‑040 Overtime Authorization Policy | Overtime ≤20% allowed without approval; >20% requires Plant Manager |
| P2‑POL‑031 | PO‑SI‑041 Outsourcing Policy | Outsourcing requires Supply Chain Director approval |
| P2‑POL‑032 | PO‑SI‑042 Capacity Plan Publication Policy | Auto‑publish with supply plan if feasible |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑030 | `AssessCapacityFeasibility` | Command handler |
| P2‑CMD‑031 | `PublishCapacityPlan` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑030 | `CapacityFeasibilityAssessed` | Event type |
| P2‑EVT‑031 | `CapacityPlanPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑030 | `GetCapacityLoad(resource, period)` | Query service |
| P2‑QRY‑031 | `GetBottlenecks()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑030 | CA‑SI‑004 5.4.13 | After supply plan → Load → Assess feasibility (DE‑SI‑040) → Generate adjustments → Publish (DE‑SI‑041) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑030 | BO‑SI‑003 Maximize Capacity Utilization | Verify capacity is utilised efficiently |
| P2‑VFY‑031 | BO‑SI‑005 Minimize Total Delivered Cost | Verify capacity constraints are respected at minimum cost |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑030 | RPT‑SI‑008 Capacity Utilization Report | Report generation |
| P2‑RPT‑031 | RPT‑SI‑009 Bottleneck Analysis Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑030 | DASH‑SI‑007 Capacity Control Tower | UI dashboard |
| P2‑DASH‑031 | DASH‑SI‑008 Resource Utilization Heatmap | UI dashboard |

### 2.8 Capability: CA‑SI‑010 — Evaluate Supply Quality (MVP Scope)

**Specification:** Supply Intelligence Specification, Section 5.10; simplified for MVP.

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑100 | OUT‑SI‑100 Supply Quality Report | Type + publisher |
| P2‑OUT‑101 | OUT‑SI‑101 Supplier Performance Scorecard | Type + publisher |
| P2‑OUT‑102 | OUT‑SI‑102 Inventory Health Trends | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑100 | DE‑SI‑100 Compute Supply Metrics | `computeSupplyMetrics(plans, actuals) → Result<Event list, DomainError>` |
| P2‑DEC‑101 | DE‑SI‑101 Publish Supply Quality Report | `decidePublishQualityReport(metrics) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑100 | BR‑SI‑100 Metric Calculation Standard Rule | All metrics per Chapter 3 formulas |
| P2‑RUL‑101 | BR‑SI‑101 Data Completeness for Metrics Rule | Flag metrics with <90% data availability as low confidence |
| P2‑RUL‑102 | BR‑SI‑102 Report Completeness Rule | Report must include all mandatory metrics |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑100 | PO‑SI‑100 Metric Calculation Frequency Policy | Weekly (rolling 4 weeks), monthly (rolling 13 weeks) |
| P2‑POL‑101 | PO‑SI‑101 Report Distribution Policy | Published by 10:00 Monday to Supply Chain leadership |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑100 | `ComputeSupplyMetrics` | Command handler |
| P2‑CMD‑101 | `PublishSupplyQualityReport` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑100 | `SupplyMetricsComputed` | Event type |
| P2‑EVT‑101 | `SupplyQualityReportPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑100 | `GetSupplyMetrics(scope, period)` | Query service |
| P2‑QRY‑101 | `GetQualityReport(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑100 | CA‑SI‑010 5.10.13 | Scheduled → Retrieve plans/actuals → Compute metrics (DE‑SI‑100) → Publish report (DE‑SI‑101) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑100 | BO‑SI‑001 Deliver Trusted Supply Understanding | Verify supply quality is measured accurately |
| P2‑VFY‑101 | BO‑SI‑008 Continuously Improve Supply Intelligence | Verify metrics show improvement over time |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑100 | RPT‑SI‑019 Supply Quality Report (MVP) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑100 | DASH‑SI‑019 Supply Performance Dashboard | UI dashboard |

### 2.9 Capability: CA‑SI‑012 — Explain Supply Decisions

**Specification:** Supply Intelligence Specification, Section 5.12

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P2‑OUT‑120 | OUT‑SI‑090 Supply Explanation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P2‑DEC‑120 | DE‑SI‑120 Generate Supply Plan Explanation | `generateSupplyExplanation(planId, trace) → Explanation` |
| P2‑DEC‑121 | DE‑SI‑121 Generate Exception Explanation | `generateExceptionExplanation(exceptionId, trace) → Explanation` |
| P2‑DEC‑122 | DE‑SI‑122 Generate Decision Explanation | `generateDecisionExplanation(decisionId, trace) → Explanation` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P2‑RUL‑120 | BR‑SI‑120 Explanation Completeness Rule | Must include: artifact description, evidence summary, causal chain, confidence, traceability chain, limitations |
| P2‑RUL‑121 | BR‑SI‑121 Traceability Chain Rule | Full ARS traceability chain required |
| P2‑RUL‑122 | BR‑SI‑122 Natural Language Rule | Follow standard explainability template |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P2‑POL‑120 | PO‑SI‑120 Explanation Quality Policy | Below 60% flagged; below 40% held for enhancement |
| P2‑POL‑121 | PO‑SI‑121 Explanation Accessibility Policy | Available to human stakeholders and AI agents |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P2‑CMD‑120 | `GenerateSupplyExplanation` | Command handler |
| P2‑CMD‑121 | `RegenerateSupplyExplanation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P2‑EVT‑120 | `SupplyDecisionExplanationGenerated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P2‑QRY‑120 | `GetSupplyExplanation(artifactId)` | Query service |
| P2‑QRY‑121 | `GetExplainabilityScore(scope, period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P2‑BEH‑120 | CA‑SI‑012 5.12.13 | Event‑driven: on plan publication, exception classification → generate explanation → publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P2‑VFY‑120 | BO‑SI‑001 Deliver Trusted Supply Understanding | Verify every supply decision is explainable |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P2‑RPT‑120 | RPT‑SI‑017 Explainability Score Report (Supply) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P2‑DASH‑120 | DASH‑SI‑017 Explainability Overview (Supply) | UI dashboard |

### 2.10 Planning Engine

**Specification:** Architecture Blueprint, Chapter 9

#### Planning Modes

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑001 | `PlanningMode` DU: FastInsert, IncrementalRepair, FullReplan, Optimization, WhatIf | Domain type |
| P2‑ENG‑002 | `PlanningModeDispatcher` | Select mode based on trigger, impact, policy |

#### MRP Pipeline

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑010 | `preprocess` step | Pure function: load and validate inputs |
| P2‑ENG‑011 | `forecastConsumption` step | Pure function: consume forecast with actuals |
| P2‑ENG‑012 | `bomExplosion` step | Pure function: multi‑level BOM explosion |
| P2‑ENG‑013 | `netting` step | Pure function: calculate net requirements |
| P2‑ENG‑014 | `supplyGeneration` step | Pure function: create planned supply orders |
| P2‑ENG‑015 | `capacityCheck` step | Pure function: evaluate capacity load |
| P2‑ENG‑016 | `pegging` step | Pure function: trace demand to supply |
| P2‑ENG‑017 | `postprocess` step | Pure function: assemble plan outputs |

#### Planning Runners

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑020 | `FastInsertRunner` | Evaluate single order ATP against plan index |
| P2‑ENG‑021 | `IncrementalRepairRunner` | Re‑plan affected scope only |
| P2‑ENG‑022 | `FullReplanRunner` | Full MRP pipeline execution |
| P2‑ENG‑023 | `WhatIfRunner` | Apply scenario overlay, run pipeline, return variant result (no publish) |

#### Replenishment

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑030 | `ReplenishmentService` | Evaluate triggers, calculate order quantities, emit events |

#### Plan Index

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑040 | `PlanIndex` | Fast lookup structures for IncrementalRepair and FastInsert |

#### Integration with DecisionCore

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑050 | Use `DecisionCore.Scoring` for plan ranking | Integration |
| P2‑ENG‑051 | Use `DecisionCore.Feasibility` for ATP/CTP | Integration |
| P2‑ENG‑052 | Use `DecisionCore.Reservations` for tentative reservations during what‑if | Integration |

#### Explainability & Telemetry

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑060 | `PlanningExplainabilityBuilder` | Build `DecisionTrace` records for every planning run |
| P2‑ENG‑061 | `PlanningTelemetryPublisher` | Emit `PlanningKpis`, `LatencyTelemetry`, `LimiterFrequency` after every run |

#### Aggregate Types

| ID | Item | Implementation |
|----|------|---------------|
| P2‑ENG‑070 | `PlanningSnapshot` aggregate | Create, lock, unlock, expire; carries plan version, policy set, graph state, provenance |
| P2‑ENG‑071 | `PlanningRun` aggregate | Lifecycle: Create → Start → AdvancePhase → Complete → Fail |
| P2‑ENG‑072 | `PlanVersion` aggregate | Immutable committed plan; carries fingerprint, scorecard, objective value, explanation key |

### 2.11 External Interfaces — Supply

#### API Endpoints

| ID | Endpoint | Method | Path | Owner Capability |
|----|----------|--------|------|------------------|
| P2‑API‑001 | Supply Data Ingestion | POST | `/api/v1/supply/data` | CA‑SI‑001 |
| P2‑API‑002 | Supply Position Query | GET | `/api/v1/supply/position` | CA‑SI‑001 |
| P2‑API‑003 | Supply Plan Query | GET | `/api/v1/supply/plan` | CA‑SI‑002 |
| P2‑API‑004 | Inventory Policy | GET, PUT | `/api/v1/inventory/policy/{productId}/{locationId}` | CA‑SI‑003 |
| P2‑API‑005 | Replenishment Recommendations | GET | `/api/v1/inventory/replenishment` | CA‑SI‑003 |
| P2‑API‑006 | Supply Quality Report | GET | `/api/v1/supply/quality/report` | CA‑SI‑010 |
| P2‑API‑007 | Supply Explanation | GET | `/api/v1/supply/explanations/{artifactId}` | CA‑SI‑012 |

#### Integration Events — Published

| ID | Event | Publisher | Consumers |
|----|-------|-----------|-----------|
| P2‑INT‑001 | `InventoryPositionUpdated` | CA‑SI‑001 | CA‑SI‑002, CA‑SI‑003, CA‑PI‑002 |
| P2‑INT‑002 | `SupplyPictureUpdated` | CA‑SI‑001 | All supply capabilities |
| P2‑INT‑003 | `SupplyPlanGenerated` | CA‑SI‑002 | CA‑SI‑003, CA‑SI‑004, CA‑PI‑002, CA‑SN‑002 |
| P2‑INT‑004 | `SupplyPlanPublished` | CA‑SI‑002 | CA‑SI‑006, CA‑SI‑007, CA‑SI‑008, CA‑PI‑002, CA‑SN‑002 |
| P2‑INT‑005 | `ReplenishmentRecommended` | CA‑SI‑003 | CA‑SI‑006 |
| P2‑INT‑006 | `InventoryPolicyUpdated` | CA‑SI‑003 | CA‑SI‑002 |
| P2‑INT‑007 | `CapacityFeasibilityAssessed` | CA‑SI‑004 | CA‑SI‑002, CA‑SI‑007 |
| P2‑INT‑008 | `CapacityPlanPublished` | CA‑SI‑004 | CA‑SI‑002 |
| P2‑INT‑009 | `SupplyQualityReportPublished` | CA‑SI‑010 | CA‑KN‑002, CA‑KN‑007 |
| P2‑INT‑010 | `SupplyDecisionExplanationGenerated` | CA‑SI‑012 | CA‑KN‑007, Audit |

### 2.12 Appendix — Supply

| ID | Item | Implementation |
|----|------|---------------|
| P2‑APP‑001 | Supply Exception Priority Matrix | Configuration document / runtime config |
| P2‑APP‑002 | Supply Enterprise Glossary | Documentation |
| P2‑APP‑003 | Supply Formula Reference | Documentation |
| P2‑APP‑004 | Supply References | Documentation |

---



We’ll now build the exhaustive Todo Items for **Phase 3 — Promise Intelligence**, following exactly the same comprehensive pattern. All seven MVP capabilities are full scope; no simplifications. Every identifier from the Promise Intelligence Specification is accounted for.

---

## Phase 3 — Promise Intelligence: Todo Items

### 3.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P3‑ARC‑001 | Create `Medhavi.Promise` project with standard folder structure (Domain, Application, Projections) | Arch. Blueprint §3.3 |
| P3‑ARC‑002 | Register Promise bounded context in `Medhavi.Nexus` composition root | Arch. Blueprint §3.3 |
| P3‑ARC‑003 | Wire Promise event subscriptions to `DomainEventBus` (consume Demand and Supply events, publish Promise events) | Arch. Blueprint §4.4.1, §4.7 |

### 3.2 Semantic Foundation — Promise

Every `SE‑PI‑xxx` concept from Chapter 4 of the Promise Intelligence Specification implemented as an F# type.

| ID | Semantic Object | Type |
|----|-----------------|------|
| P3‑SEM‑001 | SE‑PI‑001 Order | Record |
| P3‑SEM‑002 | SE‑PI‑002 Promise | Record |
| P3‑SEM‑003 | SE‑PI‑003 Allocation | Record |
| P3‑SEM‑004 | SE‑PI‑004 Commitment | Record |
| P3‑SEM‑005 | SE‑PI‑005 Promise Status | DU: Requested, Evaluating, Promised, PartiallyPromised, Rejected, Fulfilled, Breached, Cancelled |
| P3‑SEM‑010 | SE‑PI‑010 Order Line | Record |
| P3‑SEM‑011 | SE‑PI‑011 Order Request | Record |
| P3‑SEM‑012 | SE‑PI‑012 Order Status | DU: Received, Validated, UnderEvaluation, Promised, PartiallyPromised, Rejected, Fulfilled, Cancelled |
| P3‑SEM‑013 | SE‑PI‑013 Order Type | DU: Standard, Rush, Contract, Consignment, Intercompany, Sample |
| P3‑SEM‑014 | SE‑PI‑014 Order Priority | Record (score, level) |
| P3‑SEM‑015 | SE‑PI‑015 Backorder Line | Record |
| P3‑SEM‑016 | SE‑PI‑016 Order Split | Record |
| P3‑SEM‑020 | SE‑PI‑020 Promise Date | Value object |
| P3‑SEM‑021 | SE‑PI‑021 Promise Type | DU: ATP, CTP, Allocation, Substitution |
| P3‑SEM‑022 | SE‑PI‑022 Promise Confidence | Value object (0–100) |
| P3‑SEM‑023 | SE‑PI‑023 Promise Expiry | Value object |
| P3‑SEM‑024 | SE‑PI‑024 Promise Revision | Record |
| P3‑SEM‑025 | SE‑PI‑025 Promise Breach | Record |
| P3‑SEM‑030 | SE‑PI‑030 Allocation Rule | Record |
| P3‑SEM‑031 | SE‑PI‑031 Allocation Pool | Record |
| P3‑SEM‑032 | SE‑PI‑032 Allocation Consumption | Record |
| P3‑SEM‑033 | SE‑PI‑033 Allocation Period | Value object |
| P3‑SEM‑034 | SE‑PI‑034 Allocation Exhaustion | Record |
| P3‑SEM‑040 | SE‑PI‑040 Commitment Level | DU: Firm, Tentative, Contingent |
| P3‑SEM‑041 | SE‑PI‑041 Commitment Expiry | Value object |
| P3‑SEM‑042 | SE‑PI‑042 Commitment Revision | Record |
| P3‑SEM‑046 | SE‑PI‑046 Temporary Reservation | Record (from our recent patch) |
| P3‑SEM‑047 | SE‑PI‑047 Reservation Lifecycle | DU: Created, Confirmed, Released, Expired |
| P3‑SEM‑050 | SE‑PI‑050 Customer Order Profile | Record |
| P3‑SEM‑051 | SE‑PI‑051 Customer Tier (Promise) | DU: Platinum, Gold, Silver, Bronze |
| P3‑SEM‑052 | SE‑PI‑052 Communication Preference | Record |
| P3‑SEM‑053 | SE‑PI‑053 Customer Communication Template | Record |
| P3‑SEM‑060 | SE‑PI‑060 ATP Check | Record |
| P3‑SEM‑061 | SE‑PI‑061 CTP Check | Record |
| P3‑SEM‑062 | SE‑PI‑062 ATP Check Result | Record |
| P3‑SEM‑063 | SE‑PI‑063 CTP Check Result | Record |
| P3‑SEM‑064 | SE‑PI‑064 Supply Search | Record |
| P3‑SEM‑065 | SE‑PI‑065 Substitution Option | Record |
| P3‑SEM‑066 | SE‑PI‑066 Substitution Rule | Record |
| P3‑SEM‑070 | SE‑PI‑070 Promise Breach (Exception) | Record |
| P3‑SEM‑071 | SE‑PI‑071 Allocation Exhaustion (Exception) | Record |
| P3‑SEM‑072 | SE‑PI‑072 Order Change Exception | Record |
| P3‑SEM‑073 | SE‑PI‑073 ATP/CTP Failure | Record |
| P3‑SEM‑080 | SE‑PI‑080 Order Dependency | Relationship |
| P3‑SEM‑081 | SE‑PI‑081 Order Consolidation | Relationship |
| P3‑SEM‑082 | SE‑PI‑082 Order‑Supply Link | Relationship |

**Enumerations (as Discriminated Unions):**

| ID | Enumeration | Values |
|----|-------------|--------|
| P3‑ENUM‑001 | PromiseStatus | Requested, Evaluating, Promised, PartiallyPromised, Rejected, Fulfilled, Breached, Cancelled |
| P3‑ENUM‑002 | PromiseType | ATP, CTP, Allocation, Substitution |
| P3‑ENUM‑003 | OrderStatus | Received, Validated, UnderEvaluation, Promised, PartiallyPromised, Rejected, Fulfilled, Cancelled |
| P3‑ENUM‑004 | OrderType | Standard, Rush, Contract, Consignment, Intercompany, Sample |
| P3‑ENUM‑005 | CommitmentLevel | Firm, Tentative, Contingent |
| P3‑ENUM‑006 | SubstitutionType | ProductSubstitution, LocationSubstitution, GradeSubstitution, NoSubstitution |

### 3.3 Measurement Model — Promise

Every Business Outcome Measure from Chapter 3 implemented as a pure function with worked‑example verification.

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P3‑PI‑001 | PI‑PI‑001 Promise Intelligence Effectiveness | Reserved — placeholder type |
| P3‑PI‑002 | PI‑PI‑002 Order Fill Rate (Promise) | `computeOrderFillRatePromise(orders) → decimal` + test against spec worked example |
| P3‑PI‑003 | PI‑PI‑003 On‑Time Delivery (to Promise Date) | `computeOnTimeDelivery(promisedLines) → decimal` + test against spec worked example |
| P3‑PI‑004 | PI‑PI‑004 Order Cycle Time (Order to Promise) | `computeOrderCycleTime(orders) → TimeSpan` + test against spec worked example |
| P3‑PI‑005 | PI‑PI‑005 Promise Adherence | `computePromiseAdherence(promisedLines) → decimal` + test against spec worked example |
| P3‑PI‑006 | PI‑PI‑006 Order Rejection Rate | `computeOrderRejectionRate(orders) → decimal` + test against spec worked example |
| P3‑PI‑007 | PI‑PI‑007 Average Promise Lead Time | `computeAveragePromiseLeadTime(orders) → decimal` + test against spec worked example |
| P3‑PI‑008 | PI‑PI‑008 Allocation Compliance | `computeAllocationCompliance(orders, rules) → decimal` + test against spec worked example |
| P3‑PI‑009 | PI‑PI‑009 Backorder Conversion Rate | `computeBackorderConversionRate(backorders) → decimal` + test against spec worked example |
| P3‑PI‑010 | PI‑PI‑010 Perfect Order Rate (Promise) | `computePerfectOrderRatePromise(orders) → decimal` + test against spec worked example |
| P3‑PI‑011 | PI‑PI‑011 Customer Communication Accuracy | `computeCommunicationAccuracy(communications) → decimal` + test against spec worked example |
| P3‑PI‑012 | PI‑PI‑012 Order Change Cycle Time | `computeOrderChangeCycleTime(changes) → TimeSpan` + test against spec worked example |
| P3‑PI‑013 | PI‑PI‑013 Revenue Impact of Promising | `computeRevenueImpact(orders) → decimal` + test against spec worked example |
| P3‑PI‑014 | PI‑PI‑014 Planning Cycle Time (Promise) | `computePlanningCycleTimePromise(cycles) → TimeSpan` + test against spec worked example |
| P3‑PI‑015 | PI‑PI‑015 Cash Impact (Lost Sales vs. Expedite Cost) | `computeCashImpact(orders, expedites) → decimal` + test against spec worked example |

### 3.4 Capability: CA‑PI‑001 — Understand Orders

**Specification:** Promise Intelligence Specification, Section 5.1

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑001 | OUT‑PI‑001 Order Book Snapshot | Type + publisher |
| P3‑OUT‑002 | OUT‑PI‑002 Promise Register | Type + publisher |
| P3‑OUT‑003 | OUT‑PI‑003 Backorder Queue | Type + publisher |
| P3‑OUT‑004 | OUT‑PI‑004 Customer Order Profile | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑001 | DE‑PI‑010 Accept Order Request | `decideAcceptOrderRequest(order, context) → Result<Event list, DomainError>` |
| P3‑DEC‑002 | DE‑PI‑011 Update Order Status | `decideUpdateOrderStatus(orderId, newStatus, context) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑010)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑001 | BR‑PI‑010 Order Validation Rule | Required fields present, quantity >0 |
| P3‑RUL‑002 | BR‑PI‑011 Customer Eligibility Rule | Customer active, not on credit hold |
| P3‑RUL‑003 | BR‑PI‑012 Duplicate Detection Rule | Same customer/product/quantity/date within 1‑hour window flagged |

#### Rules (for DE‑PI‑011)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑004 | BR‑PI‑013 Status Transition Rule | Enforce allowed status transition paths |

#### Policies (for DE‑PI‑010)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑001 | PO‑PI‑010 Order Acceptance Automation Policy | Auto‑accept if all validation rules pass |

#### Policies (for DE‑PI‑011)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑002 | PO‑PI‑011 Order Cancellation Policy | Customer‑requested cancellation allowed only if not yet Fulfilled; internal cancellation requires Order Manager approval |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑001 | `AcceptOrder` | Command handler |
| P3‑CMD‑002 | `UpdateOrderStatus` | Command handler |
| P3‑CMD‑003 | `CancelOrder` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑001 | `OrderAccepted` | Event type |
| P3‑EVT‑002 | `OrderRejected` | Event type |
| P3‑EVT‑003 | `OrderStatusChanged` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑001 | `GetOrder(orderId)` | Query service |
| P3‑QRY‑002 | `GetOrderBook(filter)` | Query service |
| P3‑QRY‑003 | `GetPromiseRegister(customerId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑001 | CA‑PI‑001 5.1.13 | Implement full flow: Ingest → Validate (DE‑PI‑010) → Assign status → Update statuses (DE‑PI‑011) → Maintain register → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑001 | BO‑PI‑001 Deliver Trusted Order Commitments | Verify order book is complete and accurate |
| P3‑VFY‑002 | BO‑PI‑005 Improve Order Visibility and Transparency | Verify promise register is queryable in real‑time |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑001 | RPT‑PI‑001 Order Book Summary | Report generation |
| P3‑RPT‑002 | RPT‑PI‑002 Promise Register Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑001 | DASH‑PI‑001 Order Book Monitor | UI dashboard |
| P3‑DASH‑002 | DASH‑PI‑002 Customer Promise View | UI dashboard |

### 3.5 Capability: CA‑PI‑002 — Promise Orders (ATP/CTP)

**Specification:** Promise Intelligence Specification, Section 5.2

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑010 | OUT‑PI‑010 Promise Decision | Type + publisher |
| P3‑OUT‑011 | OUT‑PI‑011 Promise Commitment | Type + publisher |
| P3‑OUT‑012 | OUT‑PI‑012 Substitution Offer | Type + publisher |
| P3‑OUT‑013 | OUT‑PI‑013 ATP/CTP Evaluation Log | Type + publisher |
| P3‑OUT‑014 | OUT‑PI‑014 Supply Consumption Update | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑010 | DE‑PI‑020 Evaluate ATP | `evaluateATP(cmd, supplySnapshot) → Result<Event list, DomainError>` |
| P3‑DEC‑011 | DE‑PI‑021 Evaluate CTP | `evaluateCTP(cmd, capacitySnapshot) → Result<Event list, DomainError>` |
| P3‑DEC‑012 | DE‑PI‑022 Determine Substitution Option | `determineSubstitution(order, rules) → Result<Event list, DomainError>` |
| P3‑DEC‑013 | DE‑PI‑023 Confirm Promise and Create Commitment | `confirmPromise(decision) → Result<Event list, DomainError>` |
| P3‑DEC‑014 | DE‑PI‑024 Simulate Promise (What‑If) | `simulatePromise(params) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑020)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑010 | BR‑PI‑020 ATP Calculation Rule | ATP = OnHand + InboundSupply(confirmed) − AlreadyPromised − AllocationReserve; search priority order |
| P3‑RUL‑011 | BR‑PI‑021 ATP Horizon Rule | Search limited to configurable horizon (default 12 weeks); beyond → CTP or reject |
| P3‑RUL‑012 | BR‑PI‑022 ATP Confidence Rule | Weighted average of source reliability |
| P3‑RUL‑013 | BR‑PI‑020‑R1 Temporary Reservation Requirement Rule | ATP evaluation must place temporary reservation; if not possible, re‑compute excluding that source |

#### Rules (for DE‑PI‑021)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑014 | BR‑PI‑023 CTP Feasibility Rule | Materials available, capacity available, promise date ≤ requested + acceptance window |
| P3‑RUL‑015 | BR‑PI‑024 CTP Confidence Rule | Weighted average: capacity confidence × material confidence × production adherence |

#### Rules (for DE‑PI‑022)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑016 | BR‑PI‑025 Substitution Eligibility Rule | Rule exists, substitute available (ATP), price within ±10% or tier allows |
| P3‑RUL‑017 | BR‑PI‑026 Substitution Consent Rule | If consent required, must be obtained before confirmation |

#### Rules (for DE‑PI‑023)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑018 | BR‑PI‑027 Commitment Creation Rule | Firm if confidence ≥90%, otherwise Tentative; link to specific supply source |
| P3‑RUL‑019 | BR‑PI‑028 Supply Consumption Rule | Temporary reservation confirmed; if expired, re‑evaluate |

#### Rules (for DE‑PI‑024)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑020 | BR‑PI‑029 Simulation Isolation Rule | Simulations must not consume real supply or affect order book |

#### Policies (for DE‑PI‑020)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑010 | PO‑PI‑020 ATP Auto‑Promise Policy | Auto‑promise if confidence ≥95% and date ≤ requested + 1 day |
| P3‑POL‑011 | PO‑PI‑021 ATP Partial Promise Policy | Partial promises require customer consent |

#### Policies (for DE‑PI‑021)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑012 | PO‑PI‑022 CTP Approval Policy | CTP promises with confidence <85% require Promise Manager approval |

#### Policies (for DE‑PI‑022)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑013 | PO‑PI‑023 Auto‑Substitution Policy | Auto‑apply if customer opted in and within price tolerance |

#### Policies (for DE‑PI‑023)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑014 | PO‑PI‑024 Promise Confirmation Automation Policy | Confirm if confidence ≥90% and no policy violations |
| P3‑POL‑015 | PO‑PI‑025 Customer Communication Policy | Promise confirmations, substitutions, rejections communicated within 15 minutes |

#### Policies (for DE‑PI‑024)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑016 | PO‑PI‑026 Simulation Access Policy | Available to Sales Managers and Supply Chain Managers for orders above value threshold |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑010 | `EvaluatePromise` | Command handler |
| P3‑CMD‑011 | `ConfirmPromise` | Command handler |
| P3‑CMD‑012 | `SimulatePromise` | Command handler |
| P3‑CMD‑013 | `RePromiseBacklog` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑010 | `ATPResultCalculated` | Event type |
| P3‑EVT‑011 | `CTPResultCalculated` | Event type |
| P3‑EVT‑012 | `SubstitutionOffered` | Event type |
| P3‑EVT‑013 | `PromiseConfirmed` | Event type |
| P3‑EVT‑014 | `PromiseRejected` | Event type |
| P3‑EVT‑015 | `SupplyConsumed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑010 | `GetPromiseStatus(orderId)` | Query service |
| P3‑QRY‑011 | `GetATPResult(orderLineId)` | Query service |
| P3‑QRY‑012 | `GetCommitment(commitmentId)` | Query service |
| P3‑QRY‑013 | `SimulatePromise(params)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑010 | CA‑PI‑002 5.2.13 | Implement full flow: Trigger → Retrieve → ATP (DE‑PI‑020) → CTP (DE‑PI‑021) if needed → Substitution (DE‑PI‑022) if needed → Create temporary reservation → Confirm (DE‑PI‑023) → Simulate (DE‑PI‑024) for what‑if → Update supply consumption → Communicate → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑010 | BO‑PI‑001 Deliver Trusted Order Commitments | Verify promises are feasible and reliable |
| P3‑VFY‑011 | BO‑PI‑002 Maximize Customer Service Reliability | Verify fill rate and on‑time delivery targets |
| P3‑VFY‑012 | BO‑PI‑003 Optimize Order Promising Profitability | Verify substitution and allocation maximize margin |
| P3‑VFY‑013 | BO‑PI‑006 Increase Promising Automation | Verify auto‑promise works within confidence thresholds |
| P3‑VFY‑014 | BO‑PI‑007 Ensure Commitment Feasibility | Verify commitments do not exceed supply |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑010 | RPT‑PI‑003 Promise Decision Report | Report generation |
| P3‑RPT‑011 | RPT‑PI‑004 ATP Accuracy Report | Report generation |
| P3‑RPT‑012 | RPT‑PI‑005 CTP Accuracy Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑010 | DASH‑PI‑003 Promise Control Tower | UI dashboard |
| P3‑DASH‑011 | DASH‑PI‑004 ATP/CTP Performance Dashboard | UI dashboard |

### 3.6 Capability: CA‑PI‑003 — Manage Allocations

**Specification:** Promise Intelligence Specification, Section 5.3

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑020 | OUT‑PI‑020 Allocation Rule Set | Type + publisher |
| P3‑OUT‑021 | OUT‑PI‑021 Allocation Pool Status | Type + publisher |
| P3‑OUT‑022 | OUT‑PI‑022 Allocation Consumption Forecast | Type + publisher |
| P3‑OUT‑023 | OUT‑PI‑023 Allocation Compliance Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑020 | DE‑PI‑030 Define Allocation Rule | `decideDefineAllocationRule(rule, context) → Result<Event list, DomainError>` |
| P3‑DEC‑021 | DE‑PI‑031 Monitor Allocation Consumption | `monitorAllocationConsumption(pools) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑030)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑030 | BR‑PI‑030 Allocation Quantity Rule | Total allocated ≤100% of available supply |
| P3‑RUL‑031 | BR‑PI‑031 Allocation Priority Rule | Priority by strategic score: CustomerTier ×0.4 + Margin ×0.35 + Volume ×0.25 |
| P3‑RUL‑032 | BR‑PI‑032 Allocation Review Rule | Quarterly review; unadjusted in 2 quarters → sunset review |

#### Rules (for DE‑PI‑031)

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑033 | BR‑PI‑033 Consumption Warning Rule | Warning if exhaustion projected >2 weeks before period end or >90% consumed |
| P3‑RUL‑034 | BR‑PI‑034 Auto‑Rebalance Rule | If one pool exhausted and another has unconsumed >10% with 1 week remaining, auto‑rebalance up to 50% |

#### Policies (for DE‑PI‑030)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑030 | PO‑PI‑030 Allocation Approval Policy | Allocations >20% of constrained supply for single channel require Supply Chain Director approval |

#### Policies (for DE‑PI‑031)

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑031 | PO‑PI‑031 Exhaustion Escalation Policy | Pool exhaustion triggers immediate notification to Supply Chain Manager and affected channel managers |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑020 | `DefineAllocation` | Command handler |
| P3‑CMD‑021 | `MonitorConsumption` | Command handler |
| P3‑CMD‑022 | `RebalanceAllocation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑020 | `AllocationRuleDefined` | Event type |
| P3‑EVT‑021 | `AllocationPoolUpdated` | Event type |
| P3‑EVT‑022 | `AllocationConsumed` | Event type |
| P3‑EVT‑023 | `AllocationExhausted` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑020 | `GetAllocationRules(filter)` | Query service |
| P3‑QRY‑021 | `GetAllocationPoolStatus(poolId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑020 | CA‑PI‑003 5.3.13 | Implement: Scheduled/weekly → Define rules (DE‑PI‑030) → Monitor consumption (DE‑PI‑031) → Update pools → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑020 | BO‑PI‑003 Optimize Order Promising Profitability | Verify allocations direct scarce supply to highest value |
| P3‑VFY‑021 | BO‑PI‑006 Increase Promising Automation | Verify allocation consumption is automatic |
| P3‑VFY‑022 | BO‑PI‑007 Ensure Commitment Feasibility | Verify allocations do not exceed available supply |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑020 | RPT‑PI‑006 Allocation Compliance Report | Report generation |
| P3‑RPT‑021 | RPT‑PI‑007 Allocation Pool Health Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑020 | DASH‑PI‑005 Allocation Monitor | UI dashboard |

### 3.7 Capability: CA‑PI‑004 — Prioritize Orders

**Specification:** Promise Intelligence Specification, Section 5.4

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑030 | OUT‑PI‑030 Prioritized Order Queue | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑030 | DE‑PI‑040 Compute Order Priority | `computeOrderPriority(order, attributes) → PriorityScore` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑040 | BR‑PI‑040 Priority Scoring Rule | Priority = Σ (Weight × NormalizedFactor); thresholds: ≥85 Critical, 70–84 High, 50–69 Medium, <50 Low |
| P3‑RUL‑041 | BR‑PI‑041 Backorder Aging Rule | Backorders gain +5 points per week beyond first week |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑040 | PO‑PI‑040 Priority Override Policy | Only Promise Manager may manually override with documented justification |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑030 | `PrioritizeOrders` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑030 | `OrderPriorityAssigned` | Event type |
| P3‑EVT‑031 | `PrioritizedQueueUpdated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑030 | `GetPrioritizedQueue(filter)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑030 | CA‑PI‑004 5.4.13 | Implement: Trigger on new order/daily re‑rank → Retrieve → Compute priority (DE‑PI‑040) → Publish queue → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑030 | BO‑PI‑003 Optimize Order Promising Profitability | Verify highest‑value orders get priority |
| P3‑VFY‑031 | BO‑PI‑002 Maximize Customer Service Reliability | Verify Platinum/Gold customers get appropriate priority |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑030 | RPT‑PI‑008 Order Priority Distribution Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑030 | DASH‑PI‑006 Order Priority Dashboard | UI dashboard |

### 3.8 Capability: CA‑PI‑005 — Manage Order Changes

**Specification:** Promise Intelligence Specification, Section 5.5

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑040 | OUT‑PI‑040 Order Change Decision | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑040 | DE‑PI‑050 Evaluate Order Change | `evaluateOrderChange(order, changeRequest) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑050 | BR‑PI‑050 Change Feasibility Rule | Change feasible if new quantity/date can be satisfied via ATP/CTP within acceptance window |
| P3‑RUL‑051 | BR‑PI‑051 Cancellation Window Rule | Cancel without penalty only if not shipped and not within frozen period |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑050 | PO‑PI‑050 Change Authorization Policy | Changes causing promise date delay >3 days require customer acknowledgement |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑040 | `RequestOrderChange` | Command handler |
| P3‑CMD‑041 | `EvaluateOrderChange` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑040 | `OrderChangeRequested` | Event type |
| P3‑EVT‑041 | `OrderChangeApproved` | Event type |
| P3‑EVT‑042 | `OrderChangeRejected` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑040 | `GetOrderChangeHistory(orderId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑040 | CA‑PI‑005 5.5.13 | Implement: Trigger on change request → Validate → Evaluate (DE‑PI‑050) → If approved, re‑promise → Update commitment → Communicate → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑040 | BO‑PI‑002 Maximize Customer Service Reliability | Verify order changes are handled without breaking promises |
| P3‑VFY‑041 | BO‑PI‑005 Improve Order Visibility and Transparency | Verify change status is communicated clearly |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑040 | RPT‑PI‑009 Order Change Analysis Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑040 | DASH‑PI‑007 Order Change Monitor | UI dashboard |

### 3.9 Capability: CA‑PI‑008 — Evaluate Promise Quality

**Specification:** Promise Intelligence Specification, Section 5.8

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑080 | OUT‑PI‑070 Promise Quality Report | Type + publisher |
| P3‑OUT‑081 | OUT‑PI‑071 ATP/CTP Accuracy Report | Type + publisher |
| P3‑OUT‑082 | OUT‑PI‑072 Cycle Time Analysis | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑080 | DE‑PI‑080 Compute Promise Metrics | `computePromiseMetrics(promises, actuals) → Result<Event list, DomainError>` |
| P3‑DEC‑081 | DE‑PI‑081 Publish Promise Quality Report | `publishPromiseQualityReport(metrics) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑080 | BR‑PI‑080 Metric Calculation Standard Rule | All metrics per Chapter 3 formulas |
| P3‑RUL‑081 | BR‑PI‑081 Data Completeness Rule | Metrics with <90% data availability flagged as low confidence |
| P3‑RUL‑082 | BR‑PI‑082 Report Completeness Rule | Report must include all mandatory metrics |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑080 | PO‑PI‑080 Metric Calculation Frequency Policy | Daily, weekly, monthly |
| P3‑POL‑081 | PO‑PI‑081 Report Distribution Policy | Published by 10:00 Monday to Order Management and Supply Chain leadership |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑080 | `ComputePromiseMetrics` | Command handler |
| P3‑CMD‑081 | `PublishPromiseQualityReport` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑080 | `PromiseMetricsComputed` | Event type |
| P3‑EVT‑081 | `PromiseQualityReportPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑080 | `GetPromiseMetrics(scope, period)` | Query service |
| P3‑QRY‑081 | `GetATPAccuracy(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑080 | CA‑PI‑008 5.8.13 | Implement: Scheduled → Retrieve → Compute metrics (DE‑PI‑080) → Publish report (DE‑PI‑081) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑080 | BO‑PI‑001 Deliver Trusted Order Commitments | Verify promise quality is measured accurately |
| P3‑VFY‑081 | BO‑PI‑002 Maximize Customer Service Reliability | Verify metrics drive improvement |
| P3‑VFY‑082 | BO‑PI‑008 Continuously Improve Promise Intelligence | Verify quality trends show improvement |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑080 | RPT‑PI‑012 Promise Quality Report | Report generation |
| P3‑RPT‑081 | RPT‑PI‑004 ATP Accuracy Report | Report generation |
| P3‑RPT‑082 | RPT‑PI‑005 CTP Accuracy Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑080 | DASH‑PI‑011 Promise Performance Dashboard | UI dashboard |
| P3‑DASH‑081 | DASH‑PI‑012 ATP/CTP Accuracy Dashboard | UI dashboard |

### 3.10 Capability: CA‑PI‑010 — Explain Promise Decisions

**Specification:** Promise Intelligence Specification, Section 5.10

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P3‑OUT‑090 | OUT‑PI‑090 Promise Explanation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P3‑DEC‑090 | DE‑PI‑100 Generate Promise Explanation | `generatePromiseExplanation(artifactId) → Explanation` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P3‑RUL‑090 | BR‑PI‑100 Explanation Completeness Rule | Must include: request summary, supply source trace, rule evaluations, policy applications, outcome |
| P3‑RUL‑091 | BR‑PI‑101 Traceability Chain Rule | Full ARS traceability chain required |
| P3‑RUL‑092 | BR‑PI‑102 Natural Language Rule | Follow standard explainability template |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P3‑POL‑090 | PO‑PI‑100 Explanation Quality Policy | Below 60% flagged; below 40% held for enhancement |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P3‑CMD‑090 | `GeneratePromiseExplanation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P3‑EVT‑090 | `PromiseExplanationGenerated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P3‑QRY‑090 | `GetPromiseExplanation(promiseId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P3‑BEH‑090 | CA‑PI‑010 5.10.13 | Event‑driven: on promise confirmation, rejection, exception resolution → Generate explanation → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P3‑VFY‑090 | BO‑PI‑001 Deliver Trusted Order Commitments | Verify every promise decision is explainable |
| P3‑VFY‑091 | BO‑PI‑005 Improve Order Visibility and Transparency | Verify explanations are available to customers and planners |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P3‑RPT‑090 | RPT‑PI‑014 Explainability Score Report (Promise) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P3‑DASH‑090 | DASH‑PI‑014 Explainability Overview (Promise) | UI dashboard |

### 3.11 External Interfaces — Promise

#### API Endpoints

| ID | Endpoint | Method | Path | Owner Capability |
|----|----------|--------|------|------------------|
| P3‑API‑001 | Order Ingestion | POST | `/api/v1/orders` | CA‑PI‑001 |
| P3‑API‑002 | Promise Status Query | GET | `/api/v1/promises/{orderId}` | CA‑PI‑002 |
| P3‑API‑003 | ATP/CTP Evaluation | POST | `/api/v1/promises/evaluate` | CA‑PI‑002 |
| P3‑API‑004 | Allocation Management | GET, POST | `/api/v1/allocations`, `/api/v1/allocations/rules` | CA‑PI‑003 |
| P3‑API‑005 | Order Change Management | POST | `/api/v1/orders/{orderId}/changes` | CA‑PI‑005 |
| P3‑API‑006 | Promise Quality Report | GET | `/api/v1/promises/quality/report` | CA‑PI‑008 |
| P3‑API‑007 | Promise Exception Query | GET | `/api/v1/promises/exceptions` | CA‑PI‑009 (post‑MVP) |
| P3‑API‑008 | Promise Explanation | GET | `/api/v1/promises/explanations/{artifactId}` | CA‑PI‑010 |

#### Integration Events — Published

| ID | Event | Publisher | Consumers |
|----|-------|-----------|-----------|
| P3‑INT‑001 | `OrderAccepted` | CA‑PI‑001 | CA‑PI‑002, CA‑PI‑004 |
| P3‑INT‑002 | `OrderRejected` | CA‑PI‑001 | Customer Communication |
| P3‑INT‑003 | `OrderStatusChanged` | CA‑PI‑001 | Customer Communication, CA‑PI‑008 |
| P3‑INT‑004 | `ATPResultCalculated` | CA‑PI‑002 | CA‑PI‑010 |
| P3‑INT‑005 | `CTPResultCalculated` | CA‑PI‑002 | CA‑PI‑010 |
| P3‑INT‑006 | `SubstitutionOffered` | CA‑PI‑002 | Customer Communication |
| P3‑INT‑007 | `PromiseConfirmed` | CA‑PI‑002 | CA‑PI‑001, Customer Communication, CA‑PI‑003, Supply |
| P3‑INT‑008 | `PromiseRejected` | CA‑PI‑002 | Customer Communication |
| P3‑INT‑009 | `SupplyConsumed` | CA‑PI‑002 | Supply, CA‑PI‑003 |
| P3‑INT‑010 | `AllocationRuleDefined` | CA‑PI‑003 | CA‑PI‑002 |
| P3‑INT‑011 | `AllocationConsumed` | CA‑PI‑003 | Supply |
| P3‑INT‑012 | `AllocationExhausted` | CA‑PI‑003 | CA‑PI‑002, CA‑PI‑007 |
| P3‑INT‑013 | `OrderPriorityAssigned` | CA‑PI‑004 | CA‑PI‑002 |
| P3‑INT‑014 | `OrderChangeApproved` | CA‑PI‑005 | Customer Communication |
| P3‑INT‑015 | `PromiseMetricsComputed` | CA‑PI‑008 | CA‑PI‑011 |
| P3‑INT‑016 | `PromiseExplanationGenerated` | CA‑PI‑010 | Customer Communication, Audit |

### 3.12 Appendix — Promise

| ID | Item | Implementation |
|----|------|---------------|
| P3‑APP‑001 | Promise Exception Priority Matrix | Configuration document / runtime config |
| P3‑APP‑002 | Promise Enterprise Glossary | Documentation |
| P3‑APP‑003 | Promise Formula Reference | Documentation |
| P3‑APP‑004 | Promise References | Documentation |

---




We’ll now build the exhaustive Todo Items for **Phase 4 — Scenario Intelligence**. Seven MVP capabilities are covered: CA‑SN‑001, CA‑SN‑002, CA‑SN‑003, CA‑SN‑004 (MVP scope), CA‑SN‑005, CA‑SN‑008, and CA‑SN‑010. As per our roadmap, **Assess Risks** is simplified in the MVP; its full probabilistic and resilience assessment scope is deferred to Phase 9.

---

## Phase 4 — Scenario Intelligence: Todo Items

### 4.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P4‑ARC‑001 | Create `Medhavi.Scenario` project with standard folder structure (Domain, Application, Projections) | Arch. Blueprint §3.3 |
| P4‑ARC‑002 | Register Scenario bounded context in `Medhavi.Nexus` composition root | Arch. Blueprint §3.3 |
| P4‑ARC‑003 | Wire Scenario event subscriptions to `DomainEventBus` (consume Demand, Supply, Promise events; publish Scenario events) | Arch. Blueprint §4.4.1, §4.7 |

### 4.2 Semantic Foundation — Scenario

Every `SE‑SN‑xxx` concept from Chapter 4 of the Scenario Intelligence Specification implemented as an F# type.

| ID | Semantic Object | Type |
|----|-----------------|------|
| P4‑SEM‑001 | SE‑SN‑001 Scenario | Record |
| P4‑SEM‑002 | SE‑SN‑002 Simulation | Record |
| P4‑SEM‑003 | SE‑SN‑003 Plan Variant | Record |
| P4‑SEM‑004 | SE‑SN‑004 Scenario Outcome | Record |
| P4‑SEM‑005 | SE‑SN‑005 Scenario Trigger | Record |
| P4‑SEM‑010 | SE‑SN‑010 Scenario Definition | Record |
| P4‑SEM‑011 | SE‑SN‑011 Scenario Type | DU: Baseline, Upside, Downside, StressTest, Strategic, EventDriven, Sensitivity |
| P4‑SEM‑012 | SE‑SN‑012 Scenario Horizon | Value object |
| P4‑SEM‑013 | SE‑SN‑013 Scenario Status | DU: Draft, Defined, Simulating, Simulated, Compared, Recommended, Adopted, Archived |
| P4‑SEM‑014 | SE‑SN‑014 Scenario Assumption | Record |
| P4‑SEM‑020 | SE‑SN‑020 Simulation Engine | Record |
| P4‑SEM‑021 | SE‑SN‑021 Simulation Type | DU: Deterministic, Sensitivity, Probabilistic, OptimizationUnderUncertainty |
| P4‑SEM‑022 | SE‑SN‑022 Simulation Result | Record |
| P4‑SEM‑023 | SE‑SN‑023 Simulation Confidence | Value object |
| P4‑SEM‑024 | SE‑SN‑024 Probabilistic Outcome | Record |
| P4‑SEM‑025 | SE‑SN‑025 Simulation Run | Record |
| P4‑SEM‑030 | SE‑SN‑030 Baseline Plan | Record |
| P4‑SEM‑031 | SE‑SN‑031 Alternative Plan | Record |
| P4‑SEM‑032 | SE‑SN‑032 Recommended Plan | Record |
| P4‑SEM‑033 | SE‑SN‑033 Adopted Plan | Record |
| P4‑SEM‑034 | SE‑SN‑034 Plan Variant Lineage | Record |
| P4‑SEM‑040 | SE‑SN‑040 Risk Factor | Record |
| P4‑SEM‑041 | SE‑SN‑041 Risk Event | Record |
| P4‑SEM‑042 | SE‑SN‑042 Risk Score | Record |
| P4‑SEM‑043 | SE‑SN‑043 Risk Mitigation | Record |
| P4‑SEM‑044 | SE‑SN‑044 Stress Test | Record |
| P4‑SEM‑045 | SE‑SN‑045 Risk Appetite | Record |
| P4‑SEM‑050 | SE‑SN‑050 Scenario Comparison | Record |
| P4‑SEM‑051 | SE‑SN‑051 Comparison Criteria | Record |
| P4‑SEM‑052 | SE‑SN‑052 Trade‑Off Analysis | Record |
| P4‑SEM‑053 | SE‑SN‑053 Pareto Frontier | Record |
| P4‑SEM‑060 | SE‑SN‑060 Sensitivity Variable | Record |
| P4‑SEM‑061 | SE‑SN‑061 Sensitivity Range | Value object |
| P4‑SEM‑062 | SE‑SN‑062 Sensitivity Impact | Record |
| P4‑SEM‑063 | SE‑SN‑063 Tornado Chart | Record |
| P4‑SEM‑070 | SE‑SN‑070 Scenario Stakeholder | Record |
| P4‑SEM‑071 | SE‑SN‑071 Scenario Workshop | Record |
| P4‑SEM‑072 | SE‑SN‑072 Collaborative Scenario | Record |
| P4‑SEM‑073 | SE‑SN‑073 Consensus Scenario | Record |
| P4‑SEM‑080 | SE‑SN‑080 Scenario Dependency | Relationship |
| P4‑SEM‑081 | SE‑SN‑081 Scenario Hierarchy | Relationship |
| P4‑SEM‑082 | SE‑SN‑082 Scenario Version | Value object |
| P4‑SEM‑083 | SE‑SN‑083 Scenario Lineage | Relationship |

**Enumerations (as Discriminated Unions):**

| ID | Enumeration | Values |
|----|-------------|--------|
| P4‑ENUM‑001 | ScenarioType | Baseline, Upside, Downside, StressTest, Strategic, EventDriven, Sensitivity |
| P4‑ENUM‑002 | ScenarioStatus | Draft, Defined, Simulating, Simulated, Compared, Recommended, Adopted, Archived |
| P4‑ENUM‑003 | SimulationType | Deterministic, Sensitivity, Probabilistic, OptimizationUnderUncertainty |
| P4‑ENUM‑004 | RiskLevel | Critical, High, Medium, Low |
| P4‑ENUM‑005 | ComparisonMethod | WeightedScore, ParetoFrontier, RobustnessFirst, Minimax, Consensus |

### 4.3 Measurement Model — Scenario

Every Business Outcome Measure from Chapter 3 implemented as a pure function with worked‑example verification.

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P4‑PI‑001 | PI‑SN‑001 Scenario Intelligence Effectiveness | Reserved — placeholder type |
| P4‑PI‑002 | PI‑SN‑002 Plan Robustness Score | `computePlanRobustnessScore(plan, scenarios) → decimal` + test against spec worked example |
| P4‑PI‑003 | PI‑SN‑003 Risk Reduction Impact | `computeRiskReductionImpact(before, after) → decimal` + test against spec worked example |
| P4‑PI‑004 | PI‑SN‑004 Scenario Analysis Cycle Time | `computeScenarioCycleTime(start, end) → TimeSpan` + test against spec worked example |
| P4‑PI‑005 | PI‑SN‑005 Scenario Recommendation Adoption Rate | `computeAdoptionRate(adopted, total) → decimal` + test against spec worked example |
| P4‑PI‑006 | PI‑SN‑006 Forecast Value of Scenario Analysis | `computeValueOfScenarioAnalysis(withScenario, withoutScenario) → decimal` + test against spec worked example |
| P4‑PI‑007 | PI‑SN‑007 Stress Test Coverage | `computeStressTestCoverage(tested, total) → decimal` + test against spec worked example |
| P4‑PI‑008 | PI‑SN‑008 Scenario Accuracy | `computeScenarioAccuracy(predicted, actual) → decimal` + test against spec worked example |
| P4‑PI‑009 | PI‑SN‑009 Decision Confidence Improvement | `computeConfidenceImprovement(after, before) → decimal` |
| P4‑PI‑010 | PI‑SN‑010 Cost of Delay Avoided | `computeCostOfDelayAvoided(daysSaved, costPerDay) → decimal` + test against spec worked example |
| P4‑PI‑011 | PI‑SN‑011 Resilience Index | `computeResilienceIndex(performanceLoss, recoveryTime) → decimal` + test against spec worked example |
| P4‑PI‑012 | PI‑SN‑012 Scenario Comparison Completeness | `computeComparisonCompleteness(evaluated, total) → decimal` |
| P4‑PI‑013 | PI‑SN‑013 Collaborative Scenario Participation Rate | `computeParticipationRate(participated, invited) → decimal` |
| P4‑PI‑014 | PI‑SN‑014 Planning Cycle Time (Scenario) | `computePlanningCycleTime(start, end) → TimeSpan` |
| P4‑PI‑015 | PI‑SN‑015 Strategic Alignment Score | `computeStrategicAlignmentScore(scores) → decimal` |

### 4.4 Capability: CA‑SN‑001 — Define Scenarios

**Specification:** Scenario Intelligence Specification, Section 5.1

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑001 | OUT‑SN‑001 Scenario Definition | Type + publisher |
| P4‑OUT‑002 | OUT‑SN‑002 Scenario Catalogue | Type + publisher |
| P4‑OUT‑003 | OUT‑SN‑003 Coverage Assessment | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑001 | DE‑SN‑010 Create Scenario Definition | `createScenarioDefinition(input) → Result<Event list, DomainError>` |
| P4‑DEC‑002 | DE‑SN‑011 Approve Scenario for Simulation | `approveScenario(scenarioId) → Result<Event list, DomainError>` |
| P4‑DEC‑003 | DE‑SN‑012 Publish Scenario to Catalogue | `publishScenario(scenarioId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑001 | BR‑SN‑010 Scenario Completeness Rule | Must include type, horizon, at least one distinguishing assumption, stated purpose |
| P4‑RUL‑002 | BR‑SN‑011 Assumption Consistency Rule | Assumptions must not contradict each other |
| P4‑RUL‑003 | BR‑SN‑012 Duplicate Detection Rule (Scenario) | Identical type/horizon/assumptions (±5%) flagged as potential duplicate |
| P4‑RUL‑004 | BR‑SN‑013 Approval Gate Rule | Must pass all validation rules and have required approvals |
| P4‑RUL‑005 | BR‑SN‑014 Strategic Alignment Rule | Strategic scenarios must reference a current strategic objective |
| P4‑RUL‑006 | BR‑SN‑015 Catalogue Versioning Rule | Every publication increments catalogue version; previous retained |
| P4‑RUL‑007 | BR‑SN‑016 Coverage Update Rule | Coverage assessment recalculated on publication |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑001 | PO‑SN‑010 Scenario Creation Authorization Policy | Strategic scenarios require VP/Director approval; operational scenarios by authorised planners |
| P4‑POL‑002 | PO‑SN‑011 Scenario Approval Workflow Policy | Operational: 1 approver; Strategic: 2 approvers |
| P4‑POL‑003 | PO‑SN‑012 Catalogue Review Policy | Quarterly review; scenarios unused in 12 months are archiving candidates |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑001 | `CreateScenario` | Command handler |
| P4‑CMD‑002 | `ReviseScenario` | Command handler |
| P4‑CMD‑003 | `ApproveScenario` | Command handler |
| P4‑CMD‑004 | `PublishScenario` | Command handler |
| P4‑CMD‑005 | `ArchiveScenario` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑001 | `ScenarioDefined` | Event type |
| P4‑EVT‑002 | `ScenarioApproved` | Event type |
| P4‑EVT‑003 | `ScenarioPublished` | Event type |
| P4‑EVT‑004 | `ScenarioArchived` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑001 | `GetScenario(scenarioId)` | Query service |
| P4‑QRY‑002 | `GetScenarioCatalogue(filter)` | Query service |
| P4‑QRY‑003 | `GetCoverageAssessment()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑001 | CA‑SN‑001 5.1.13 | Implement: Trigger → Retrieve → Create/Revise (DE‑SN‑010) → Validate & Approve (DE‑SN‑011) → Publish (DE‑SN‑012) → Update coverage → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑001 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify scenario catalogue is complete and governed |
| P4‑VFY‑002 | BO‑SN‑002 Improve Plan Robustness and Resilience | Verify scenarios cover key risks |
| P4‑VFY‑003 | BO‑SN‑003 Minimize Enterprise Risk Exposure | Verify risk scenarios are comprehensive |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑001 | RPT‑SN‑001 Scenario Catalogue Report | Report generation |
| P4‑RPT‑002 | RPT‑SN‑002 Coverage Gap Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑001 | DASH‑SN‑001 Scenario Catalogue Dashboard | UI dashboard |
| P4‑DASH‑002 | DASH‑SN‑002 Scenario Lineage Viewer | UI dashboard |

### 4.5 Capability: CA‑SN‑002 — Simulate Scenarios

**Specification:** Scenario Intelligence Specification, Section 5.2

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑010 | OUT‑SN‑010 Simulation Result Set | Type + publisher |
| P4‑OUT‑011 | OUT‑SN‑011 Probabilistic Distribution | Type + publisher |
| P4‑OUT‑012 | OUT‑SN‑012 Simulation Confidence Score | Type + publisher |
| P4‑OUT‑013 | OUT‑SN‑013 Simulation Run Record | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑010 | DE‑SN‑020 Select Simulation Method | `selectSimulationMethod(scenario, context) → Method` |
| P4‑DEC‑011 | DE‑SN‑021 Execute Simulation Run | `executeSimulationRun(config) → Result<Event list, DomainError>` |
| P4‑DEC‑012 | DE‑SN‑022 Generate Probabilistic Summary | `generateProbabilisticSummary(results) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑010 | BR‑SN‑020 Simulation Method Selection Rule | StressTest/Strategic + impact >$1M → Probabilistic; EventDriven + response <1hr → Deterministic |
| P4‑RUL‑011 | BR‑SN‑021 Resource Budget Rule | Probabilistic must complete within time budget; otherwise downgrade with warning |
| P4‑RUL‑012 | BR‑SN‑022 Output Plausibility Rule | No negative inventory, service level ≤100%, capacity utilization ≤200% |
| P4‑RUL‑013 | BR‑SN‑023 Confidence Threshold Rule | <60% flagged as low confidence; <40% suppressed |
| P4‑RUL‑014 | BR‑SN‑024 Reproducibility Rule | All inputs, engine version, random seed, configuration recorded |
| P4‑RUL‑015 | BR‑SN‑025 Convergence Rule | Standard error of mean for primary KPI <1% of mean or max iterations reached |
| P4‑RUL‑016 | BR‑SN‑026 Summary Completeness Rule | Must include mean, median, P10, P90, VaR at configured confidence, convergence indicator |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑010 | PO‑SN‑020 Method Override Policy | Senior planner may override with documented justification |
| P4‑POL‑011 | PO‑SN‑021 Simulation Retry Policy | Failed/low‑confidence runs auto‑retried once; second failure escalated |
| P4‑POL‑012 | PO‑SN‑022 VaR Confidence Level Policy | 95% default; strategic decisions >$10M require 99% VaR |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑010 | `StartSimulation` | Command handler |
| P4‑CMD‑011 | `RetrySimulation` | Command handler |
| P4‑CMD‑012 | `CancelSimulation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑010 | `SimulationStarted` | Event type |
| P4‑EVT‑011 | `SimulationCompleted` | Event type |
| P4‑EVT‑012 | `SimulationFailed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑010 | `GetSimulationResult(runId)` | Query service |
| P4‑QRY‑011 | `GetProbabilisticSummary(runId)` | Query service |
| P4‑QRY‑012 | `GetSimulationStatus(runId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑010 | CA‑SN‑002 5.2.13 | Implement: Trigger → Retrieve scenario + plan → Select method (DE‑SN‑020) → Execute (DE‑SN‑021) → Summarize (DE‑SN‑022) → Store → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑010 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify simulation outputs are accurate and reproducible |
| P4‑VFY‑011 | BO‑SN‑002 Improve Plan Robustness and Resilience | Verify simulations stress‑test plans adequately |
| P4‑VFY‑012 | BO‑SN‑005 Increase Scenario Planning Automation | Verify simulation method selection and retry are automated |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑010 | RPT‑SN‑002 Simulation Execution Report | Report generation |
| P4‑RPT‑011 | RPT‑SN‑003 Probabilistic Analysis Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑010 | DASH‑SN‑003 Simulation Monitor | UI dashboard |
| P4‑DASH‑011 | DASH‑SN‑004 Scenario Outcome Explorer | UI dashboard |

### 4.6 Capability: CA‑SN‑003 — Compare Scenarios

**Specification:** Scenario Intelligence Specification, Section 5.3

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑020 | OUT‑SN‑020 Comparison Matrix | Type + publisher |
| P4‑OUT‑021 | OUT‑SN‑021 Plan Ranking | Type + publisher |
| P4‑OUT‑022 | OUT‑SN‑022 Pareto Frontier | Type + publisher |
| P4‑OUT‑023 | OUT‑SN‑023 Trade‑Off Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑020 | DE‑SN‑030 Select Comparison Method and Criteria | `selectComparisonMethod(context) → Method` |
| P4‑DEC‑021 | DE‑SN‑031 Execute Comparison | `executeComparison(results, criteria) → Result<Event list, DomainError>` |
| P4‑DEC‑022 | DE‑SN‑032 Publish Comparison Report | `publishComparisonReport(comparisonId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑020 | BR‑SN‑030 Comparison Method Selection Rule | ≥2 stakeholder groups + impact >$1M → Pareto; single stakeholder operational → WeightedScore; worst‑case primary → Minimax |
| P4‑RUL‑021 | BR‑SN‑031 Criteria Completeness Rule | At least 3 criteria covering financial, service, and risk |
| P4‑RUL‑022 | BR‑SN‑032 Normalization Rule | Criteria normalized to 0–1 before weighting |
| P4‑RUL‑023 | BR‑SN‑033 Data Completeness Rule | ≥95% of plan‑scenario combinations must have completed simulations |
| P4‑RUL‑024 | BR‑SN‑034 Report Completeness Rule | Must include ranking, Pareto frontier, trade‑off analysis for top 3 criteria pairs, sensitivity to ±10% weight changes |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑020 | PO‑SN‑030 Criteria Weight Approval Policy | Strategic comparisons require Finance and Supply Chain Director approval |
| P4‑POL‑021 | PO‑SN‑031 Comparison Publication Policy | Auto‑publish if completeness and confidence thresholds met |
| P4‑POL‑022 | PO‑SN‑032 Report Distribution Policy | Strategic reports distributed ≥5 business days before decision meetings |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑020 | `StartComparison` | Command handler |
| P4‑CMD‑021 | `PublishComparisonReport` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑020 | `ComparisonCompleted` | Event type |
| P4‑EVT‑021 | `ComparisonReportPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑020 | `GetComparison(comparisonId)` | Query service |
| P4‑QRY‑021 | `GetParetoFrontier(comparisonId)` | Query service |
| P4‑QRY‑022 | `GetTradeOffAnalysis(comparisonId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑020 | CA‑SN‑003 5.3.13 | Implement: After simulation completion → Retrieve results → Select method (DE‑SN‑030) → Execute comparison (DE‑SN‑031) → Publish report (DE‑SN‑032) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑020 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify comparisons are complete and unbiased |
| P4‑VFY‑021 | BO‑SN‑002 Improve Plan Robustness and Resilience | Verify Pareto frontier identifies robust options |
| P4‑VFY‑022 | BO‑SN‑004 Optimize Strategic Decision Making | Verify comparison supports decision‑making |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑020 | RPT‑SN‑004 Scenario Comparison Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑020 | DASH‑SN‑005 Comparison Workbench | UI dashboard |
| P4‑DASH‑021 | DASH‑SN‑006 Scenario Scorecard | UI dashboard |

### 4.7 Capability: CA‑SN‑004 — Assess Risks (MVP Scope)

**Specification:** Scenario Intelligence Specification, Section 5.4; simplified per roadmap §3.2.

**MVP scope:** Risk scoring via `Probability × Impact`, basic deterministic stress tests with pass/fail against risk appetite thresholds, and prioritisation by ROI.

**Full scope (Phase 9):** Probabilistic risk assessment using Monte Carlo simulation outputs, full stress test suite with recovery metrics, and automated mitigation portfolio optimisation.

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑030 | OUT‑SN‑030 Risk Assessment Report | Type + publisher |
| P4‑OUT‑031 | OUT‑SN‑031 Stress Test Results | Type + publisher |
| P4‑OUT‑032 | OUT‑SN‑032 Mitigation Prioritization | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑030 | DE‑SN‑040 Compute Risk Scores | `computeRiskScores(risks) → Result<Event list, DomainError>` |
| P4‑DEC‑031 | DE‑SN‑041 Execute Stress Test | `executeStressTest(scenario) → Result<Event list, DomainError>` |
| P4‑DEC‑032 | DE‑SN‑042 Prioritize Risk Mitigations | `prioritizeMitigations(risks) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑030 | BR‑SN‑040 Risk Score Calculation Rule | Risk Score = Probability (%) × Impact ($); Probability from deterministic assessment |
| P4‑RUL‑031 | BR‑SN‑041 Risk Level Classification Rule | Critical: ≥80 or top 10%; High: 50–79; Medium: 20–49; Low: <20 |
| P4‑RUL‑032 | BR‑SN‑042 Stress Test Pass/Fail Rule | Fail if any KPI breaches risk appetite threshold |
| P4‑RUL‑033 | BR‑SN‑043 Stress Test Recovery Rule | Recovery time > max acceptable → mandatory mitigation review |
| P4‑RUL‑034 | BR‑SN‑044 Mitigation ROI Rule | Mitigation ROI = Risk Reduction (score points) ÷ Mitigation Cost ($100K units) |
| P4‑RUL‑035 | BR‑SN‑045 Critical Risk Mitigation Rule | All Critical risks must have at least one proposed mitigation within 30 days |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑030 | PO‑SN‑040 Risk Quantification Policy | Risks with estimated impact >$500K must be quantified via structured analysis (MVP: deterministic; Phase 9: probabilistic) |
| P4‑POL‑031 | PO‑SN‑041 Stress Test Escalation Policy | Failed stress tests escalated to Risk Committee within 24 hours |
| P4‑POL‑032 | PO‑SN‑042 Mitigation Approval Policy | Mitigations >$1M require CFO approval; others require Risk Manager |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑030 | `AssessRisk` | Command handler |
| P4‑CMD‑031 | `ExecuteStressTest` | Command handler |
| P4‑CMD‑032 | `PrioritizeMitigations` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑030 | `RiskAssessmentCompleted` | Event type |
| P4‑EVT‑031 | `StressTestCompleted` | Event type |
| P4‑EVT‑032 | `MitigationPrioritized` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑030 | `GetRiskAssessment(period)` | Query service |
| P4‑QRY‑031 | `GetStressTestResults(scenarioId)` | Query service |
| P4‑QRY‑032 | `GetMitigationPlan()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑030 | CA‑SN‑004 5.4.13 | Implement: After simulation → Retrieve → Compute risk scores (DE‑SN‑040) → Execute stress tests (DE‑SN‑041) → Prioritize mitigations (DE‑SN‑042) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑030 | BO‑SN‑003 Minimize Enterprise Risk Exposure | Verify risk scores are quantified and prioritised |
| P4‑VFY‑031 | BO‑SN‑002 Improve Plan Robustness and Resilience | Verify stress tests identify breaking points |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑030 | RPT‑SN‑005 Risk Assessment Report | Report generation |
| P4‑RPT‑031 | RPT‑SN‑006 Stress Test Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑030 | DASH‑SN‑007 Risk Heatmap Dashboard | UI dashboard |
| P4‑DASH‑031 | DASH‑SN‑008 Stress Test Monitor | UI dashboard |

### 4.8 Capability: CA‑SN‑005 — Recommend Scenario

**Specification:** Scenario Intelligence Specification, Section 5.5

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑040 | OUT‑SN‑040 Scenario Recommendation | Type + publisher |
| P4‑OUT‑041 | OUT‑SN‑041 Decision Brief | Type + publisher |
| P4‑OUT‑042 | OUT‑SN‑042 Implementation Pathway | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑040 | DE‑SN‑050 Generate Recommendation | `generateRecommendation(comparison, risk) → Result<Event list, DomainError>` |
| P4‑DEC‑041 | DE‑SN‑051 Adopt Recommended Plan | `adoptRecommendedPlan(recommendationId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑040 | BR‑SN‑050 Recommendation Selection Rule | Must be on Pareto frontier, satisfy risk appetite, highest robustness; tie‑break by expected value |
| P4‑RUL‑041 | BR‑SN‑051 Risk Appetite Compliance Rule | No variant breaching risk appetite may be recommended without explicit override |
| P4‑RUL‑042 | BR‑SN‑052 Baseline Comparison Rule | Must compare to baseline; if improvement <2% on all criteria, “No Change” may be recommended |
| P4‑RUL‑043 | BR‑SN‑053 Adoption Authorization Rule | Adoption only if all required approvals obtained |
| P4‑RUL‑044 | BR‑SN‑054 Plan Lineage Rule | Adoption creates new plan version with full lineage |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑040 | PO‑SN‑050 Recommendation Approval Policy | Strategic (>$5M impact) requires VP + CFO; operational requires Director |
| P4‑POL‑041 | PO‑SN‑051 Override Policy | Decision‑makers may override with documented rationale; overrides tracked for learning |
| P4‑POL‑042 | PO‑SN‑052 Adoption Transmission Policy | Adopted plan changes transmitted to operational domains within 4 business hours |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑040 | `GenerateRecommendation` | Command handler |
| P4‑CMD‑041 | `AdoptRecommendation` | Command handler |
| P4‑CMD‑042 | `RejectRecommendation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑040 | `ScenarioRecommendationMade` | Event type |
| P4‑EVT‑041 | `ScenarioRecommendationAdopted` | Event type |
| P4‑EVT‑042 | `ScenarioRecommendationRejected` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑040 | `GetRecommendation(recommendationId)` | Query service |
| P4‑QRY‑041 | `GetAdoptedPlans(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑040 | CA‑SN‑005 5.5.13 | Implement: After comparison + risk assessment → Retrieve → Generate recommendation (DE‑SN‑050) → Present → If approved, adopt (DE‑SN‑051) → Transmit → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑040 | BO‑SN‑004 Optimize Strategic Decision Making | Verify recommendations are evidence‑based and traceable |
| P4‑VFY‑041 | BO‑SN‑003 Minimize Enterprise Risk Exposure | Verify recommended plan satisfies risk appetite |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑040 | RPT‑SN‑007 Recommendation Summary Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑040 | DASH‑SN‑009 Recommendation Dashboard | UI dashboard |

### 4.9 Capability: CA‑SN‑008 — Evaluate Scenario Quality

**Specification:** Scenario Intelligence Specification, Section 5.8

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑080 | OUT‑SN‑070 Scenario Quality Report | Type + publisher |
| P4‑OUT‑081 | OUT‑SN‑071 Calibration Analysis | Type + publisher |
| P4‑OUT‑082 | OUT‑SN‑072 Improvement Opportunities | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑080 | DE‑SN‑080 Compute Scenario Accuracy | `computeScenarioAccuracy(predictions, actuals) → Result<Event list, DomainError>` |
| P4‑DEC‑081 | DE‑SN‑081 Evaluate Probability Calibration | `evaluateCalibration(predictions, actuals) → Result<Event list, DomainError>` |
| P4‑DEC‑082 | DE‑SN‑082 Publish Scenario Quality Report | `publishScenarioQualityReport(metrics) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑080 | BR‑SN‑080 Accuracy Calculation Rule | (1 − |Predicted − Actual| ÷ Actual) × 100; weighted average across KPIs |
| P4‑RUL‑081 | BR‑SN‑081 Materialization Identification Rule | Scenario considered materialized if key variables within ±20% tolerance |
| P4‑RUL‑082 | BR‑SN‑082 Minimum Data Rule | Accuracy only computed if materialized period covers ≥80% of horizon |
| P4‑RUL‑083 | BR‑SN‑083 Calibration Assessment Rule | Brier Score or calibration curve; <0.8 triggers recalibration review |
| P4‑RUL‑084 | BR‑SN‑084 Minimum Sample Rule | At least 20 probabilistic scenarios with materialized outcomes required |
| P4‑RUL‑085 | BR‑SN‑085 Report Completeness Rule | Must include accuracy, calibration, adoption rate |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑080 | PO‑SN‑080 Accuracy Review Frequency Policy | Quarterly, presented at S&OP review |
| P4‑POL‑081 | PO‑SN‑081 Recalibration Policy | If calibration score <0.8, engine recalibrated within 30 days |
| P4‑POL‑082 | PO‑SN‑082 Report Distribution Policy | Published within 15 business days of quarter end |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑080 | `ComputeScenarioAccuracy` | Command handler |
| P4‑CMD‑081 | `AssessCalibration` | Command handler |
| P4‑CMD‑082 | `PublishQualityReport` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑080 | `ScenarioAccuracyComputed` | Event type |
| P4‑EVT‑081 | `CalibrationAssessed` | Event type |
| P4‑EVT‑082 | `ScenarioQualityReportPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑080 | `GetScenarioAccuracy(scenarioId)` | Query service |
| P4‑QRY‑081 | `GetCalibrationScore(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑080 | CA‑SN‑008 5.8.13 | Implement: Quarterly → Retrieve predictions/actuals → Identify materialized scenarios → Compute accuracy (DE‑SN‑080) → Evaluate calibration (DE‑SN‑081) → Publish report (DE‑SN‑082) → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑080 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify scenario accuracy is measured |
| P4‑VFY‑081 | BO‑SN‑008 Continuously Improve Scenario Intelligence | Verify quality trends improve |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑080 | RPT‑SN‑010 Scenario Quality Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑080 | DASH‑SN‑013 Scenario Quality Dashboard | UI dashboard |
| P4‑DASH‑081 | DASH‑SN‑014 Scenario Performance Scorecard | UI dashboard |

### 4.10 Capability: CA‑SN‑010 — Explain Scenario Decisions

**Specification:** Scenario Intelligence Specification, Section 5.10

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P4‑OUT‑090 | OUT‑SN‑090 Scenario Explanation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P4‑DEC‑090 | DE‑SN‑100 Generate Scenario Explanation | `generateScenarioExplanation(artifactId) → Explanation` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P4‑RUL‑090 | BR‑SN‑100 Explanation Completeness Rule (Scenario) | Must include: artifact type, assumptions, rules evaluated, policies applied, outcome with rationale |
| P4‑RUL‑091 | BR‑SN‑101 Traceability Chain Rule (Scenario) | Full ARS traceability chain |
| P4‑RUL‑092 | BR‑SN‑102 Natural Language Rule (Scenario) | Follow standard explainability template |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P4‑POL‑090 | PO‑SN‑100 Explanation Quality Policy (Scenario) | Below 60% flagged; below 40% held for enhancement |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P4‑CMD‑090 | `GenerateScenarioExplanation` | Command handler |
| P4‑CMD‑091 | `RegenerateScenarioExplanation` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P4‑EVT‑090 | `ScenarioExplanationGenerated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P4‑QRY‑090 | `GetScenarioExplanation(artifactId)` | Query service |
| P4‑QRY‑091 | `GetExplainabilityScore(scope, period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P4‑BEH‑090 | CA‑SN‑010 5.10.13 | Event‑driven: on scenario publication, simulation completion, comparison completion, recommendation made → Generate explanation → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P4‑VFY‑090 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify every scenario decision is explainable |
| P4‑VFY‑091 | BO‑SN‑008 Continuously Improve Scenario Intelligence | Verify explanations enable learning |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P4‑RPT‑090 | RPT‑SN‑017 Explainability Score Report (Scenario) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P4‑DASH‑090 | DASH‑SN‑017 Explainability Overview (Scenario) | UI dashboard |

### 4.11 External Interfaces — Scenario

#### API Endpoints

| ID | Endpoint | Method | Path | Owner Capability |
|----|----------|--------|------|------------------|
| P4‑API‑001 | Scenario Management | POST, GET, PUT, DELETE | `/api/v1/scenarios`, `/api/v1/scenarios/{id}`, `/api/v1/scenarios/catalogue` | CA‑SN‑001 |
| P4‑API‑002 | Simulation Execution | POST, GET | `/api/v1/simulations/run`, `/api/v1/simulations/{runId}`, `/api/v1/simulations/{runId}/results` | CA‑SN‑002 |
| P4‑API‑003 | Comparison | POST, GET | `/api/v1/comparisons`, `/api/v1/comparisons/{comparisonId}` | CA‑SN‑003 |
| P4‑API‑004 | Risk Assessment | POST | `/api/v1/risks/assess`, `/api/v1/risks/stress-test` | CA‑SN‑004 |
| P4‑API‑005 | Recommendation | POST, GET | `/api/v1/recommendations`, `/api/v1/recommendations/{id}`, `/api/v1/recommendations/{id}/adopt` | CA‑SN‑005 |
| P4‑API‑006 | Scenario Quality Report | GET | `/api/v1/scenarios/quality/report` | CA‑SN‑008 |
| P4‑API‑007 | Scenario Explanation | GET | `/api/v1/scenarios/explanations/{artifactId}` | CA‑SN‑010 |

#### Integration Events — Published

| ID | Event | Publisher | Consumers |
|----|-------|-----------|-----------|
| P4‑INT‑001 | `ScenarioDefined` | CA‑SN‑001 | CA‑SN‑002, CA‑SN‑003 |
| P4‑INT‑002 | `ScenarioPublished` | CA‑SN‑001 | All scenario capabilities |
| P4‑INT‑003 | `SimulationStarted` | CA‑SN‑002 | Monitor, CA‑SN‑009 |
| P4‑INT‑004 | `SimulationCompleted` | CA‑SN‑002 | CA‑SN‑003, CA‑SN‑004, CA‑SN‑005 |
| P4‑INT‑005 | `SimulationFailed` | CA‑SN‑002 | CA‑SN‑009 |
| P4‑INT‑006 | `ComparisonCompleted` | CA‑SN‑003 | CA‑SN‑005, CA‑SN‑006 |
| P4‑INT‑007 | `RiskAssessmentCompleted` | CA‑SN‑004 | CA‑SN‑005 |
| P4‑INT‑008 | `StressTestCompleted` | CA‑SN‑004 | CA‑SN‑005 |
| P4‑INT‑009 | `ScenarioRecommendationMade` | CA‑SN‑005 | All operational domains (Demand, Supply, Promise) |
| P4‑INT‑010 | `ScenarioRecommendationAdopted` | CA‑SN‑005 | Demand, Supply, Promise (plan updates) |
| P4‑INT‑011 | `ScenarioRecommendationRejected` | CA‑SN‑005 | CA‑SN‑011 |
| P4‑INT‑012 | `ScenarioQualityReportPublished` | CA‑SN‑008 | CA‑SN‑011, Management |
| P4‑INT‑013 | `ScenarioExplanationGenerated` | CA‑SN‑010 | Audit, AI Agents |

### 4.12 Appendix — Scenario

| ID | Item | Implementation |
|----|------|---------------|
| P4‑APP‑001 | Scenario Exception Priority Matrix | Configuration document / runtime config |
| P4‑APP‑002 | Scenario Enterprise Glossary | Documentation |
| P4‑APP‑003 | Scenario Formula Reference | Documentation |
| P4‑APP‑004 | Scenario References | Documentation |

---

We’ll now build the exhaustive Todo Items for **Phase 5 — Knowledge Intelligence & AI Copilot**. This is the most critical domain—it ties all other domains together and enables the AI‑native APS vision. All eleven Knowledge capabilities are full MVP scope, plus the three AI Copilot items.

---

## Phase 5 — Knowledge Intelligence & AI Copilot: Todo Items

### 5.1 Architecture & Infrastructure

| ID | Todo Item | Source |
|----|-----------|--------|
| P5‑ARC‑001 | Create `Medhavi.Knowledge` project with standard folder structure (Domain, Application, Projections) | Arch. Blueprint §3.3 |
| P5‑ARC‑002 | Register Knowledge bounded context in `Medhavi.Nexus` composition root | Arch. Blueprint §3.3 |
| P5‑ARC‑003 | Wire Knowledge event subscriptions to `DomainEventBus` (consume quality reports, exception logs, learning events from all domains) | Arch. Blueprint §4.4.1, §4.7 |
| P5‑ARC‑004 | Implement Knowledge Graph store (in‑memory graph structure with nodes and edges) | Arch. Blueprint §5.1 |
| P5‑ARC‑005 | Implement Enterprise Memory store (append‑only event log with indexing) | Arch. Blueprint §5.7 |

### 5.2 Semantic Foundation — Knowledge

Every `SE‑KN‑xxx` concept from Chapter 4 of the Knowledge Intelligence Specification implemented as an F# type.

| ID | Semantic Object | Type |
|----|-----------------|------|
| P5‑SEM‑001 | SE‑KN‑001 Knowledge | Record |
| P5‑SEM‑002 | SE‑KN‑002 Learning Event | Record |
| P5‑SEM‑003 | SE‑KN‑003 Cross‑Domain Pattern | Record |
| P5‑SEM‑004 | SE‑KN‑004 Improvement Portfolio | Record |
| P5‑SEM‑005 | SE‑KN‑005 Enterprise Memory | Record |
| P5‑SEM‑010 | SE‑KN‑010 Knowledge Artifact | Record |
| P5‑SEM‑011 | SE‑KN‑011 Knowledge Confidence | Value object (0–100) |
| P5‑SEM‑012 | SE‑KN‑012 Knowledge Lifecycle | DU: Proposed, UnderReview, Validated, Published, Superseded, Retired |
| P5‑SEM‑013 | SE‑KN‑013 Knowledge Domain | Value object |
| P5‑SEM‑014 | SE‑KN‑014 Knowledge Evidence | Record |
| P5‑SEM‑015 | SE‑KN‑015 Knowledge Provenance | Record |
| P5‑SEM‑020 | SE‑KN‑020 Pattern Type | DU: Correlation, Causation, Sequence, Anomaly, Structural |
| P5‑SEM‑021 | SE‑KN‑021 Pattern Significance | Record |
| P5‑SEM‑022 | SE‑KN‑022 Pattern Trigger | Record |
| P5‑SEM‑023 | SE‑KN‑023 Cross‑Domain Correlation | Record |
| P5‑SEM‑024 | SE‑KN‑024 Causal Chain | Record |
| P5‑SEM‑030 | SE‑KN‑030 Knowledge Node | Record |
| P5‑SEM‑031 | SE‑KN‑031 Knowledge Edge | Record (typed relationship) |
| P5‑SEM‑032 | SE‑KN‑032 Semantic Consistency Rule | Record |
| P5‑SEM‑033 | SE‑KN‑033 Ontology Version | Record |
| P5‑SEM‑034 | SE‑KN‑034 Knowledge Graph Coverage | Record |
| P5‑SEM‑040 | SE‑KN‑040 Improvement Initiative | Record |
| P5‑SEM‑041 | SE‑KN‑041 Improvement Status | DU: Proposed, Approved, InProgress, Implemented, Verified, Rejected, RolledBack |
| P5‑SEM‑042 | SE‑KN‑042 Improvement ROI | Value object |
| P5‑SEM‑043 | SE‑KN‑043 Improvement Dependency | Relationship |
| P5‑SEM‑050 | SE‑KN‑050 Root Cause | Record |
| P5‑SEM‑051 | SE‑KN‑051 Contributing Factor | Record |
| P5‑SEM‑052 | SE‑KN‑052 Root‑Cause Confidence | Value object (0–100) |
| P5‑SEM‑053 | SE‑KN‑053 Root‑Cause Analysis | Record |
| P5‑SEM‑060 | SE‑KN‑060 Best Practice | Record |
| P5‑SEM‑061 | SE‑KN‑061 Practice Provenance | Record |
| P5‑SEM‑062 | SE‑KN‑062 Practice Applicability | Record |
| P5‑SEM‑063 | SE‑KN‑063 Practice Institutionalisation | Record |
| P5‑SEM‑070 | SE‑KN‑070 Feedback Signal | Record |
| P5‑SEM‑071 | SE‑KN‑071 Feedback Target | Record |
| P5‑SEM‑072 | SE‑KN‑072 Feedback Loop | Record |
| P5‑SEM‑073 | SE‑KN‑073 Loop Closure | Record |
| P5‑SEM‑080 | SE‑KN‑080 Enterprise Event | Record |
| P5‑SEM‑081 | SE‑KN‑081 Outcome Record | Record |
| P5‑SEM‑082 | SE‑KN‑082 Decision Record | Record |
| P5‑SEM‑083 | SE‑KN‑083 Memory Query | Record |
| P5‑SEM‑090 | SE‑KN‑090 Knowledge Dependency | Relationship |
| P5‑SEM‑091 | SE‑KN‑091 Knowledge Hierarchy | Relationship |
| P5‑SEM‑092 | SE‑KN‑092 Knowledge Version | Value object |
| P5‑SEM‑093 | SE‑KN‑093 Knowledge Lineage | Relationship |

**Enumerations (as Discriminated Unions):**

| ID | Enumeration | Values |
|----|-------------|--------|
| P5‑ENUM‑001 | PatternType | Correlation, Causation, Sequence, Anomaly, Structural |
| P5‑ENUM‑002 | KnowledgeLifecycleState | Proposed, UnderReview, Validated, Published, Superseded, Retired |
| P5‑ENUM‑003 | ImprovementStatus | Proposed, Approved, InProgress, Implemented, Verified, Rejected, RolledBack |
| P5‑ENUM‑004 | KnowledgeEdgeType | DependsOn, Produces, Consumes, Governs, Constrains, Validates, CorrelatesWith, Causes, Supersedes, References |
| P5‑ENUM‑005 | FeedbackLoopState | Opened, Analyzing, Recommending, Implementing, Verifying, Closed, Abandoned |

### 5.3 Measurement Model — Knowledge

Every Business Outcome Measure from Chapter 3 implemented as a pure function with worked‑example verification.

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P5‑PI‑001 | PI‑KN‑001 Knowledge Intelligence Effectiveness | Reserved — placeholder type |
| P5‑PI‑002 | PI‑KN‑002 Cross‑Domain Pattern Discovery Rate | `computePatternDiscoveryRate(patterns, period) → int` |
| P5‑PI‑003 | PI‑KN‑003 Root‑Cause Identification Accuracy | `computeRootCauseAccuracy(analyses) → decimal` + test against spec worked example |
| P5‑PI‑004 | PI‑KN‑004 Improvement Portfolio ROI | `computePortfolioROI(initiatives) → decimal` + test against spec worked example |
| P5‑PI‑005 | PI‑KN‑005 Best‑Practice Institutionalisation Rate | `computeInstitutionalisationRate(practices) → decimal` + test against spec worked example |
| P5‑PI‑006 | PI‑KN‑006 Knowledge Graph Consistency Score | `computeConsistencyScore(graph) → decimal` + test against spec worked example |
| P5‑PI‑007 | PI‑KN‑007 Cross‑Domain Learning Cycle Time | `computeLearningCycleTime(loops) → TimeSpan` |
| P5‑PI‑008 | PI‑KN‑008 Enterprise Memory Completeness | `computeMemoryCompleteness(events) → decimal` |
| P5‑PI‑009 | PI‑KN‑009 Systemic Risk Reduction | `computeSystemicRiskReduction(before, after) → decimal` |
| P5‑PI‑010 | PI‑KN‑010 Decision Confidence Improvement (Enterprise) | `computeConfidenceImprovement(after, before) → decimal` |
| P5‑PI‑011 | PI‑KN‑011 Cross‑Domain Plan Consistency Score | `computePlanConsistency(plans) → decimal` |
| P5‑PI‑012 | PI‑KN‑012 Feedback Loop Closure Rate | `computeLoopClosureRate(loops) → decimal` |
| P5‑PI‑013 | PI‑KN‑013 Knowledge Serving Latency | `computeServingLatency(queries) → TimeSpan` |
| P5‑PI‑014 | PI‑KN‑014 Planning Cycle Time (Knowledge) | `computePlanningCycleTime(start, end) → TimeSpan` |
| P5‑PI‑015 | PI‑KN‑015 Strategic Insight Generation Rate | `computeInsightRate(insights, period) → int` |

### 5.4 Capability: CA‑KN‑001 — Govern Knowledge Graph

**Specification:** Knowledge Intelligence Specification, Section 5.1

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑001 | OUT‑KN‑001 Enterprise Knowledge Graph | Type + publisher |
| P5‑OUT‑002 | OUT‑KN‑002 Consistency Violation Report | Type + publisher |
| P5‑OUT‑003 | OUT‑KN‑003 Coverage Gap Report | Type + publisher |
| P5‑OUT‑004 | OUT‑KN‑004 Ontology Version | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑001 | DE‑KN‑010 Validate Semantic Consistency | `validateSemanticConsistency(object, graph) → Result<Event list, DomainError>` |
| P5‑DEC‑002 | DE‑KN‑011 Publish Ontology Version | `publishOntologyVersion(version) → Result<Event list, DomainError>` |
| P5‑DEC‑003 | DE‑KN‑012 Remediate Knowledge Graph Gaps | `remediateGaps(gaps) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑001 | BR‑KN‑010 Identifier Uniqueness Rule | Identifier must be unique across all domains |
| P5‑RUL‑002 | BR‑KN‑011 Definition Consistency Rule | No logical contradiction with existing objects |
| P5‑RUL‑003 | BR‑KN‑012 Mandatory Relationship Rule | Decision→Capability, Rule→Decision, PI→Objective links required |
| P5‑RUL‑004 | BR‑KN‑013 Circular Dependency Rule | Graph must be acyclic for DependsOn and Constrains edges |
| P5‑RUL‑005 | BR‑KN‑014 Versioning Rule | Major for breaking changes, minor for additions |
| P5‑RUL‑006 | BR‑KN‑015 Domain Notification Rule | Affected domains notified 5 business days before change |
| P5‑RUL‑007 | BR‑KN‑016 Gap Assignment Rule | Gaps assigned to owning domain by prefix |
| P5‑RUL‑008 | BR‑KN‑017 Gap Acceptance Rule | Intentional gaps must be documented |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑001 | PO‑KN‑010 Object Acceptance Policy | Auto‑accept if all consistency rules pass |
| P5‑POL‑002 | PO‑KN‑011 Conflict Resolution Policy | Conflicting definitions escalated to Knowledge Manager, resolved within 10 business days |
| P5‑POL‑003 | PO‑KN‑012 Ontology Approval Policy | Major versions require Knowledge Manager + affected Domain Managers |
| P5‑POL‑004 | PO‑KN‑013 Remediation Deadline Policy | Critical gaps 30 days, non‑critical 90 days |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑001 | `ValidateObject` | Command handler |
| P5‑CMD‑002 | `IncorporateObject` | Command handler |
| P5‑CMD‑003 | `PublishOntologyVersion` | Command handler |
| P5‑CMD‑004 | `AssignGapRemediation` | Command handler |
| P5‑CMD‑005 | `RunConsistencyScan` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑001 | `KnowledgeGraphUpdated` | Event type |
| P5‑EVT‑002 | `ConsistencyViolationDetected` | Event type |
| P5‑EVT‑003 | `OntologyVersionPublished` | Event type |
| P5‑EVT‑004 | `KnowledgeGapAssigned` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑001 | `GetKnowledgeGraph(filter)` | Query service |
| P5‑QRY‑002 | `GetConsistencyReport()` | Query service |
| P5‑QRY‑003 | `GetCoverageAssessment()` | Query service |
| P5‑QRY‑004 | `GetOntologyVersion()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑001 | CA‑KN‑001 5.1.13 | Implement: On object publication → Validate (DE‑KN‑010) → Incorporate → Publish ontology (DE‑KN‑011) on schema change → Remediate gaps (DE‑KN‑012) monthly |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑001 | BO‑KN‑003 Govern the Enterprise Knowledge Graph | Verify graph is consistent and complete |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑001 | RPT‑KN‑001 Knowledge Graph Health Report | Report generation |
| P5‑RPT‑002 | RPT‑KN‑002 Ontology Change Log | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑001 | DASH‑KN‑001 Knowledge Graph Explorer | UI dashboard |
| P5‑DASH‑002 | DASH‑KN‑002 Knowledge Health Dashboard | UI dashboard |

### 5.5 Capability: CA‑KN‑002 — Discover Cross‑Domain Patterns

**Specification:** Knowledge Intelligence Specification, Section 5.2

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑010 | OUT‑KN‑010 Discovered Pattern | Type + publisher |
| P5‑OUT‑011 | OUT‑KN‑011 Cross‑Domain Correlation Matrix | Type + publisher |
| P5‑OUT‑012 | OUT‑KN‑012 Pattern Significance Report | Type + publisher |
| P5‑OUT‑013 | OUT‑KN‑013 Causal Chain Hypothesis | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑010 | DE‑KN‑020 Detect Candidate Patterns | `detectCandidatePatterns(outcomeData) → Result<Event list, DomainError>` |
| P5‑DEC‑011 | DE‑KN‑021 Validate Discovered Pattern | `validatePattern(pattern) → Result<Event list, DomainError>` |
| P5‑DEC‑012 | DE‑KN‑022 Propose Causal Chain | `proposeCausalChain(pattern) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑010 | BR‑KN‑020 Correlation Threshold Rule | |r| > 0.6, p < 0.05 |
| P5‑RUL‑011 | BR‑KN‑021 Minimum Data Rule | ≥8 aligned data points across involved domains |
| P5‑RUL‑012 | BR‑KN‑022 Domain Diversity Rule | Must involve ≥2 different Intelligence Domains |
| P5‑RUL‑013 | BR‑KN‑023 False Discovery Rate Rule | Benjamini‑Hochberg at α = 0.05 |
| P5‑RUL‑014 | BR‑KN‑024 Hold‑Out Validation Rule | ≥25% hold‑out data, strength must remain above threshold |
| P5‑RUL‑015 | BR‑KN‑025 Stakeholder Review Rule | At least one stakeholder from each involved domain |
| P5‑RUL‑016 | BR‑KN‑026 Confounding Check Rule | Potential confounds evaluated before publication |
| P5‑RUL‑017 | BR‑KN‑027 Causal Link Evidence Rule | Each link supported by documented relationship, mediation analysis, or experiment |
| P5‑RUL‑018 | BR‑KN‑028 Causal Chain Completeness Rule | Complete chain with no logical gaps |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑010 | PO‑KN‑020 Detection Frequency Policy | Weekly operational, monthly strategic; event‑driven within 24 hours |
| P5‑POL‑011 | PO‑KN‑021 Detection Method Policy | Methods reviewed annually by Data Science |
| P5‑POL‑012 | PO‑KN‑022 Pattern Publication Policy | Confidence ≥80% auto‑publish; 60–80% provisional; <60% rejected |
| P5‑POL‑013 | PO‑KN‑023 Stakeholder Review Deadline Policy | Review within 10 business days; unreviewed escalated |
| P5‑POL‑014 | PO‑KN‑024 Causal Chain Prioritisation Policy | Impact >$1M prioritised for immediate root‑cause analysis |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑010 | `RunPatternDetection` | Command handler |
| P5‑CMD‑011 | `ValidatePattern` | Command handler |
| P5‑CMD‑012 | `ProposeCausalChain` | Command handler |
| P5‑CMD‑013 | `RejectPattern` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑010 | `CandidatePatternDetected` | Event type |
| P5‑EVT‑011 | `PatternValidated` | Event type |
| P5‑EVT‑012 | `PatternRejected` | Event type |
| P5‑EVT‑013 | `CausalChainProposed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑010 | `GetDiscoveredPatterns(filter)` | Query service |
| P5‑QRY‑011 | `GetCrossDomainCorrelations(domain1, domain2)` | Query service |
| P5‑QRY‑012 | `GetCausalChain(chainId)` | Query service |
| P5‑QRY‑013 | `GetPatternEvidence(patternId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑010 | CA‑KN‑002 5.2.13 | Scheduled/event‑driven → Retrieve outcome data → Detect (DE‑KN‑020) → Validate (DE‑KN‑021) → Propose causal chain (DE‑KN‑022) → Publish → Raise events |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑010 | BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence | Verify patterns are statistically valid |
| P5‑VFY‑011 | BO‑KN‑002 Discover Systemic Patterns and Root Causes | Verify patterns span multiple domains |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑010 | RPT‑KN‑003 Cross‑Domain Pattern Report | Report generation |
| P5‑RPT‑011 | RPT‑KN‑004 Correlation Matrix Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑010 | DASH‑KN‑003 Cross‑Domain Insight Dashboard | UI dashboard |
| P5‑DASH‑011 | DASH‑KN‑004 Pattern Significance Heatmap | UI dashboard |

### 5.6 Capability: CA‑KN‑003 — Analyze Root Causes

**Specification:** Knowledge Intelligence Specification, Section 5.3

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑020 | OUT‑KN‑020 Root‑Cause Analysis Report | Type + publisher |
| P5‑OUT‑021 | OUT‑KN‑021 Corrective Action Recommendation | Type + publisher |
| P5‑OUT‑022 | OUT‑KN‑022 Root‑Cause Confidence Assessment | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑020 | DE‑KN‑030 Identify Root Cause | `identifyRootCause(pattern, evidence) → Result<Event list, DomainError>` |
| P5‑DEC‑021 | DE‑KN‑031 Validate Root Cause | `validateRootCause(analysis, outcome) → Result<Event list, DomainError>` |
| P5‑DEC‑022 | DE‑KN‑032 Recommend Corrective Action | `recommendCorrectiveAction(rootCause) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑020 | BR‑KN‑030 Root Cause Depth Rule | Iterative “why” until no further enterprise‑controllable cause |
| P5‑RUL‑021 | BR‑KN‑031 Multi‑Source Evidence Rule | ≥2 independent sources |
| P5‑RUL‑022 | BR‑KN‑032 Distinction Rule | Root cause vs. proximate cause vs. contributing factor |
| P5‑RUL‑023 | BR‑KN‑033 Outcome Verification Rule | ≥2 planning cycles observation, p<0.05 improvement |
| P5‑RUL‑024 | BR‑KN‑034 Partial Validation Rule | 30–50% reduction → partially validated, search for additional causes |
| P5‑RUL‑025 | BR‑KN‑035 Action‑Cause Alignment Rule | Corrective action must directly address root cause |
| P5‑RUL‑026 | BR‑KN‑036 Feasibility Check Rule | Reviewed by implementing domain |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑020 | PO‑KN‑030 Stakeholder Review Policy (Root Cause) | Reviewed by ≥1 stakeholder from each affected domain |
| P5‑POL‑021 | PO‑KN‑031 Inconclusive Analysis Policy | Published as inconclusive, revisited quarterly |
| P5‑POL‑022 | PO‑KN‑032 Validation Tracking Policy | Unvalidated analyses escalated after 6 months |
| P5‑POL‑023 | PO‑KN‑033 Corrective Action Prioritisation Policy | >$500K annual benefit prioritised for executive review |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑020 | `StartRootCauseAnalysis` | Command handler |
| P5‑CMD‑021 | `IdentifyRootCause` | Command handler |
| P5‑CMD‑022 | `RecommendCorrectiveAction` | Command handler |
| P5‑CMD‑023 | `ValidateRootCause` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑020 | `RootCauseIdentified` | Event type |
| P5‑EVT‑021 | `CorrectiveActionRecommended` | Event type |
| P5‑EVT‑022 | `RootCauseValidated` | Event type |
| P5‑EVT‑023 | `RootCauseValidationFailed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑020 | `GetRootCauseAnalysis(analysisId)` | Query service |
| P5‑QRY‑021 | `GetCorrectiveActions(filter)` | Query service |
| P5‑QRY‑022 | `GetValidationStatus(analysisId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑020 | CA‑KN‑003 5.3.13 | Trigger on validated pattern → Conduct investigation → Identify root cause (DE‑KN‑030) → Recommend action (DE‑KN‑032) → After implementation, validate (DE‑KN‑031) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑020 | BO‑KN‑002 Discover Systemic Patterns and Root Causes | Verify root causes are correctly identified |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑020 | RPT‑KN‑005 Root‑Cause Analysis Report | Report generation |
| P5‑RPT‑021 | RPT‑KN‑006 Corrective Action Tracking Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑020 | DASH‑KN‑005 Root‑Cause Investigation Workbench | UI dashboard |
| P5‑DASH‑021 | DASH‑KN‑006 Corrective Action Dashboard | UI dashboard |

### 5.7 Capability: CA‑KN‑004 — Manage Improvement Portfolio

**Specification:** Knowledge Intelligence Specification, Section 5.4

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑030 | OUT‑KN‑030 Prioritised Improvement Portfolio | Type + publisher |
| P5‑OUT‑031 | OUT‑KN‑031 Initiative Status Report | Type + publisher |
| P5‑OUT‑032 | OUT‑KN‑032 Portfolio ROI Report | Type + publisher |
| P5‑OUT‑033 | OUT‑KN‑033 Portfolio Health Dashboard Data | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑030 | DE‑KN‑040 Propose Improvement Initiative | `proposeImprovement(input) → Result<Event list, DomainError>` |
| P5‑DEC‑031 | DE‑KN‑041 Prioritise Improvement Portfolio | `prioritisePortfolio(initiatives) → Result<Event list, DomainError>` |
| P5‑DEC‑032 | DE‑KN‑042 Approve Implementation | `approveImplementation(initiativeId) → Result<Event list, DomainError>` |
| P5‑DEC‑033 | DE‑KN‑043 Verify Improvement Outcome | `verifyOutcome(initiativeId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑030 | BR‑KN‑040 Business Case Completeness Rule | Benefit, cost, domains, owner, timeline, success criteria |
| P5‑RUL‑031 | BR‑KN‑041 Duplicate Detection Rule | Merge or reject duplicates |
| P5‑RUL‑032 | BR‑KN‑042 Prioritisation Formula Rule | Composite score = Σ(Weight × NormalizedFactor) |
| P5‑RUL‑033 | BR‑KN‑043 Dependency Rule | Flag dependent initiatives |
| P5‑RUL‑034 | BR‑KN‑044 Resource Constraint Rule | Must respect domain resource capacity |
| P5‑RUL‑035 | BR‑KN‑045 Approval Gate Rule | Complete business case, resource assignment, success criteria, funding |
| P5‑RUL‑036 | BR‑KN‑046 Verification Window Rule | 2 cycles operational, 4 cycles strategic |
| P5‑RUL‑037 | BR‑KN‑047 Rollback Trigger Rule | Statistically significant degradation → auto‑rollback |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑030 | PO‑KN‑040 Proposal Submission Policy | Any domain manager or Knowledge Intelligence may submit |
| P5‑POL‑031 | PO‑KN‑041 Portfolio Review Frequency Policy | Monthly reprioritisation, quarterly strategic |
| P5‑POL‑032 | PO‑KN‑042 Resource Allocation Policy | >$100K requires executive S&OP approval |
| P5‑POL‑033 | PO‑KN‑043 Funding Approval Policy | ≤$50K Domain Manager, $50–250K Director, >$250K VP/CFO |
| P5‑POL‑034 | PO‑KN‑044 Verification Reporting Policy | Every implemented improvement verified within one cycle of observation window |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑030 | `ProposeImprovement` | Command handler |
| P5‑CMD‑031 | `PrioritisePortfolio` | Command handler |
| P5‑CMD‑032 | `ApproveImplementation` | Command handler |
| P5‑CMD‑033 | `VerifyOutcome` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑030 | `ImprovementProposed` | Event type |
| P5‑EVT‑031 | `PortfolioPrioritised` | Event type |
| P5‑EVT‑032 | `ImplementationApproved` | Event type |
| P5‑EVT‑033 | `ImprovementVerified` | Event type |
| P5‑EVT‑034 | `ImprovementRollbackRecommended` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑030 | `GetPortfolio(filter)` | Query service |
| P5‑QRY‑031 | `GetInitiative(initiativeId)` | Query service |
| P5‑QRY‑032 | `GetPortfolioROI(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑030 | CA‑KN‑004 5.4.13 | On proposal → Propose (DE‑KN‑040) → Prioritise monthly (DE‑KN‑041) → Approve top (DE‑KN‑042) → Track → Verify (DE‑KN‑043) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑030 | BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement | Verify improvements are managed and tracked |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑030 | RPT‑KN‑007 Improvement Portfolio Report | Report generation |
| P5‑RPT‑031 | RPT‑KN‑008 Portfolio Health Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑030 | DASH‑KN‑007 Improvement Portfolio Dashboard | UI dashboard |
| P5‑DASH‑031 | DASH‑KN‑008 Portfolio Health Dashboard | UI dashboard |

### 5.8 Capability: CA‑KN‑005 — Institutionalise Best Practices

**Specification:** Knowledge Intelligence Specification, Section 5.5

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑040 | OUT‑KN‑040 Validated Best Practice | Type + publisher |
| P5‑OUT‑041 | OUT‑KN‑041 Institutionalisation Plan | Type + publisher |
| P5‑OUT‑042 | OUT‑KN‑042 Adoption Compliance Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑040 | DE‑KN‑050 Nominate Best Practice | `nominateBestPractice(practice) → Result<Event list, DomainError>` |
| P5‑DEC‑041 | DE‑KN‑051 Validate Best Practice | `validateBestPractice(practice) → Result<Event list, DomainError>` |
| P5‑DEC‑042 | DE‑KN‑052 Publish as Enterprise Standard | `publishBestPractice(practiceId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑040 | BR‑KN‑050 Evidence Threshold Rule | ≥2 independent instances with measurable improvement |
| P5‑RUL‑041 | BR‑KN‑051 Generalisability Rule | Enterprise‑wide must apply to ≥2 domains |
| P5‑RUL‑042 | BR‑KN‑052 Validation Completeness Rule | Effectiveness, applicability, limitations, prerequisites, monitoring plan |
| P5‑RUL‑043 | BR‑KN‑053 Stakeholder Endorsement Rule | ≥1 domain manager from each applicable domain |
| P5‑RUL‑044 | BR‑KN‑054 Publication Rule | Added to knowledge graph with edges to applicable capabilities/decisions/rules |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑040 | PO‑KN‑050 Nomination Policy | Any domain manager or Knowledge Manager may nominate; reviewed within 15 days |
| P5‑POL‑041 | PO‑KN‑051 Validation Review Policy | Validation completed within 30 days of nomination |
| P5‑POL‑042 | PO‑KN‑052 Adoption Mandate Policy | Applicable domains must adopt within 90 days or document justified exception |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑040 | `NominateBestPractice` | Command handler |
| P5‑CMD‑041 | `ValidateBestPractice` | Command handler |
| P5‑CMD‑042 | `PublishBestPractice` | Command handler |
| P5‑CMD‑043 | `ReviseBestPractice` | Command handler |
| P5‑CMD‑044 | `RetireBestPractice` | Command handler |
| P5‑CMD‑045 | `MonitorAdoption` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑040 | `BestPracticeNominated` | Event type |
| P5‑EVT‑041 | `BestPracticeValidated` | Event type |
| P5‑EVT‑042 | `BestPracticePublished` | Event type |
| P5‑EVT‑043 | `BestPracticeRevised` | Event type |
| P5‑EVT‑044 | `BestPracticeRetired` | Event type |
| P5‑EVT‑045 | `BestPracticeAdoptionAssessed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑040 | `GetBestPractice(practiceId)` | Query service |
| P5‑QRY‑041 | `GetBestPracticeCatalogue(filter)` | Query service |
| P5‑QRY‑042 | `GetPracticeProvenance(practiceId)` | Query service |
| P5‑QRY‑043 | `GetAdoptionStatus(practiceId)` | Query service |
| P5‑QRY‑044 | `GetBestPracticeHistory(practiceId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑040 | CA‑KN‑005 5.5.13 | On verified improvement → Nominate (DE‑KN‑050) → Validate (DE‑KN‑051) → Publish (DE‑KN‑052) → Monitor adoption |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑040 | BO‑KN‑005 Institutionalise Best Practices | Verify practices are adopted across domains |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑040 | RPT‑KN‑009 Best‑Practice Catalogue Report | Report generation |
| P5‑RPT‑041 | RPT‑KN‑010 Adoption Compliance Report | Report generation |
| P5‑RPT‑042 | RPT‑KN‑011 Practice Effectiveness Review | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑040 | DASH‑KN‑009 Best‑Practice Catalogue Dashboard | UI dashboard |
| P5‑DASH‑041 | DASH‑KN‑010 Adoption Compliance Monitor | UI dashboard |

### 5.9 Capability: CA‑KN‑006 — Orchestrate Feedback Loops

**Specification:** Knowledge Intelligence Specification, Section 5.6

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑050 | OUT‑KN‑050 Feedback Loop Map | Type + publisher |
| P5‑OUT‑051 | OUT‑KN‑051 Loop Cycle Time Analysis | Type + publisher |
| P5‑OUT‑052 | OUT‑KN‑052 Stalled Loop Report | Type + publisher |
| P5‑OUT‑053 | OUT‑KN‑053 Loop Closure Record | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑050 | DE‑KN‑060 Open Feedback Loop | `openFeedbackLoop(event) → Result<Event list, DomainError>` |
| P5‑DEC‑051 | DE‑KN‑061 Monitor Loop Progress | `monitorLoopProgress(loops) → Result<Event list, DomainError>` |
| P5‑DEC‑052 | DE‑KN‑062 Close Feedback Loop | `closeFeedbackLoop(loopId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑050 | BR‑KN‑060 Loop Opening Rule | Enterprise loop if ≥2 domains or impact >$100K |
| P5‑RUL‑051 | BR‑KN‑061 Duplicate Loop Rule | Merge if same underlying pattern |
| P5‑RUL‑052 | BR‑KN‑062 Event Classification Rule | Positive (replicate), Negative (correct), Neutral (monitor) |
| P5‑RUL‑053 | BR‑KN‑063 Stage Duration Rule | Expected durations per stage |
| P5‑RUL‑054 | BR‑KN‑064 Stall Detection Rule | Exceeded duration +50% → flagged stalled |
| P5‑RUL‑055 | BR‑KN‑065 Stage Transition Rule | Exit criteria per stage |
| P5‑RUL‑056 | BR‑KN‑066 Closure Criteria Rule | All stages complete, verification data, lessons learned, owner sign‑off |
| P5‑RUL‑057 | BR‑KN‑067 Lessons Learned Rule | What worked, what didn't, what would be done differently |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑050 | PO‑KN‑060 Loop Opening SLA Policy | Enterprise loops within 24 hours, domain‑level within 48 hours |
| P5‑POL‑051 | PO‑KN‑061 Loop Ownership Policy | Every loop assigned an owner |
| P5‑POL‑052 | PO‑KN‑062 Stalled Loop Escalation Policy | >30 days stalled → escalated to executive S&OP |
| P5‑POL‑053 | PO‑KN‑063 Loop Reprioritisation Policy | Monthly review; Critical/High impact prioritised |
| P5‑POL‑054 | PO‑KN‑064 Closure Sign‑Off Policy | Successful/partial: loop owner + domain stakeholder; unsuccessful/abandoned: Knowledge Manager |
| P5‑POL‑055 | PO‑KN‑065 Loop Archival Policy | Closed loops archived with full traceability, queryable indefinitely |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑050 | `OpenFeedbackLoop` | Command handler |
| P5‑CMD‑051 | `UpdateLoopStage` | Command handler |
| P5‑CMD‑052 | `FlagStalledLoop` | Command handler |
| P5‑CMD‑053 | `CloseFeedbackLoop` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑050 | `FeedbackLoopOpened` | Event type |
| P5‑EVT‑051 | `FeedbackLoopStageCompleted` | Event type |
| P5‑EVT‑052 | `FeedbackLoopStalled` | Event type |
| P5‑EVT‑053 | `FeedbackLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑050 | `GetActiveLoops(filter)` | Query service |
| P5‑QRY‑051 | `GetLoop(loopId)` | Query service |
| P5‑QRY‑052 | `GetLoopCycleTimeAnalysis(period)` | Query service |
| P5‑QRY‑053 | `GetStalledLoops()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑050 | CA‑KN‑006 5.6.13 | Continuous monitoring → Open loop (DE‑KN‑060) → Route through capabilities → Monitor (DE‑KN‑061) → Close (DE‑KN‑062) → Record |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑050 | BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement | Verify loops are tracked to closure |
| P5‑VFY‑051 | BO‑KN‑006 Accelerate Cross‑Domain Learning | Verify cycle times are measured and improving |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑050 | RPT‑KN‑012 Feedback Loop Status Report | Report generation |
| P5‑RPT‑051 | RPT‑KN‑013 Loop Effectiveness Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑050 | DASH‑KN‑011 Feedback Loop Control Tower | UI dashboard |
| P5‑DASH‑051 | DASH‑KN‑012 Learning Cycle Dashboard | UI dashboard |

### 5.10 Capability: CA‑KN‑007 — Maintain Enterprise Memory

**Specification:** Knowledge Intelligence Specification, Section 5.7

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑060 | OUT‑KN‑060 Enterprise Memory Record | Type + publisher |
| P5‑OUT‑061 | OUT‑KN‑061 Memory Query Response | Type + publisher |
| P5‑OUT‑062 | OUT‑KN‑062 Memory Completeness Report | Type + publisher |
| P5‑OUT‑063 | OUT‑KN‑063 Memory Index | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑060 | DE‑KN‑070 Record Significant Event | `recordSignificantEvent(event) → Result<Event list, DomainError>` |
| P5‑DEC‑061 | DE‑KN‑071 Record Decision with Context | `recordDecision(decision) → Result<Event list, DomainError>` |
| P5‑DEC‑062 | DE‑KN‑072 Respond to Memory Query | `respondToMemoryQuery(query) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑060 | BR‑KN‑070 Significance Threshold Rule | Plan adoptions, promise breaches (Gold+), forecast errors >20%, disruptions >$100K, etc. |
| P5‑RUL‑061 | BR‑KN‑071 Context Capture Rule | Timestamp, domain, type, artifacts, impact, related events |
| P5‑RUL‑062 | BR‑KN‑072 Deduplication Rule | Same domain/artifacts/time window → merge |
| P5‑RUL‑063 | BR‑KN‑073 Decision Recording Rule | All strategic decisions + operational >$100K |
| P5‑RUL‑064 | BR‑KN‑074 Context Completeness Rule | Decision context, alternatives, rationale, expected outcome, decision‑maker |
| P5‑RUL‑065 | BR‑KN‑075 Relevance Ranking Rule | Domain match, artifact similarity, impact similarity, recency, outcome availability |
| P5‑RUL‑066 | BR‑KN‑076 Response Completeness Rule | Matched records, outcome summary, confidence score |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑060 | PO‑KN‑070 Recording Timeliness Policy | Operational within 1 hour, strategic within 24 hours |
| P5‑POL‑061 | PO‑KN‑071 Retention Policy | Indefinite; cold storage after 7 years |
| P5‑POL‑062 | PO‑KN‑072 Decision Recording Policy | Within 24 hours; >7 days escalated |
| P5‑POL‑063 | PO‑KN‑073 Query Response SLA Policy | AI agents <500ms; humans <5s |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑060 | `RecordEvent` | Command handler |
| P5‑CMD‑061 | `RecordDecision` | Command handler |
| P5‑CMD‑062 | `QueryMemory` | Command handler |
| P5‑CMD‑063 | `AssessMemoryCompleteness` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑060 | `EventRecorded` | Event type |
| P5‑EVT‑061 | `DecisionRecorded` | Event type |
| P5‑EVT‑062 | `MemoryQueryResponded` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑060 | `QueryMemory(params)` | Query service |
| P5‑QRY‑061 | `GetEvent(eventId)` | Query service |
| P5‑QRY‑062 | `GetDecision(decisionId)` | Query service |
| P5‑QRY‑063 | `GetMemoryCompleteness()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑060 | CA‑KN‑007 5.7.13 | Continuous ingestion → Record events (DE‑KN‑070) → Record decisions (DE‑KN‑071) → Index → Respond to queries (DE‑KN‑072) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑060 | BO‑KN‑007 Maintain Enterprise Memory | Verify memory is complete and queryable |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑060 | RPT‑KN‑014 Enterprise Memory Completeness Report | Report generation |
| P5‑RPT‑061 | RPT‑KN‑015 Memory Query Analytics Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑060 | DASH‑KN‑013 Enterprise Memory Explorer | UI dashboard |
| P5‑DASH‑061 | DASH‑KN‑014 Memory Health Dashboard | UI dashboard |

### 5.11 Capability: CA‑KN‑008 — Serve Knowledge to AI Agents

**Specification:** Knowledge Intelligence Specification, Section 5.8

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑070 | OUT‑KN‑070 Knowledge Response | Type + publisher |
| P5‑OUT‑071 | OUT‑KN‑071 Precedent Summary | Type + publisher |
| P5‑OUT‑072 | OUT‑KN‑072 Best‑Practice Recommendation | Type + publisher |
| P5‑OUT‑073 | OUT‑KN‑073 Pattern Alert | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑070 | DE‑KN‑080 Process Knowledge Request | `processKnowledgeRequest(request) → Result<Event list, DomainError>` |
| P5‑DEC‑071 | DE‑KN‑081 Assemble Knowledge Response | `assembleKnowledgeResponse(results) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑070 | BR‑KN‑080 Query Classification Rule | Precedent, Guidance, Relationship, Risk, or Composite |
| P5‑RUL‑071 | BR‑KN‑081 Minimum Confidence Rule | Served knowledge must meet agent's required confidence |
| P5‑RUL‑072 | BR‑KN‑082 Provenance Attachment Rule | Every served piece must include provenance |
| P5‑RUL‑073 | BR‑KN‑083 Relevance Threshold Rule | Only artifacts with relevance ≥70% included |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑070 | PO‑KN‑080 Agent Authentication Policy | Agents must authenticate and be authorised for queried domains |
| P5‑POL‑071 | PO‑KN‑081 Rate Limiting Policy | Per‑agent rate limits, configurable |
| P5‑POL‑072 | PO‑KN‑082 Response Format Policy | JSON/JSON‑LD structured schema |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑070 | `RequestKnowledge` | Command handler |
| P5‑CMD‑071 | `AssembleResponse` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑070 | `KnowledgeRequestReceived` | Event type |
| P5‑EVT‑071 | `KnowledgeResponseServed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑070 | `GetKnowledgeResponse(requestId)` | Query service |
| P5‑QRY‑071 | `GetAgentKnowledgeUsage(agentId, period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑070 | CA‑KN‑008 5.8.13 | On request → Authenticate → Process (DE‑KN‑080) → Query sources → Assemble (DE‑KN‑081) → Return → Log |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑070 | BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence | Verify AI agents receive accurate knowledge |
| P5‑VFY‑071 | BO‑KN‑007 Maintain Enterprise Memory | Verify memory is accessible to AI agents |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑070 | RPT‑KN‑016 AI Agent Knowledge Usage Report | Report generation |
| P5‑RPT‑071 | RPT‑KN‑017 Knowledge Serving Performance Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑070 | DASH‑KN‑015 AI Knowledge Serving Monitor | UI dashboard |
| P5‑DASH‑071 | DASH‑KN‑016 Knowledge Usage Analytics | UI dashboard |

### 5.12 Capability: CA‑KN‑009 — Evaluate Knowledge Quality

**Specification:** Knowledge Intelligence Specification, Section 5.9

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑080 | OUT‑KN‑080 Knowledge Quality Report | Type + publisher |
| P5‑OUT‑081 | OUT‑KN‑081 Quality Trend Analysis | Type + publisher |
| P5‑OUT‑082 | OUT‑KN‑082 Quality Gap Report | Type + publisher |
| P5‑OUT‑083 | OUT‑KN‑083 Knowledge Improvement Recommendations | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑080 | DE‑KN‑090 Compute Knowledge Quality Metrics | `computeKnowledgeQualityMetrics() → Result<Event list, DomainError>` |
| P5‑DEC‑081 | DE‑KN‑091 Assess Knowledge Quality Trends | `assessKnowledgeQualityTrends() → Result<Event list, DomainError>` |
| P5‑DEC‑082 | DE‑KN‑092 Publish Knowledge Quality Report | `publishKnowledgeQualityReport(metrics) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑080 | BR‑KN‑090 Metric Calculation Standard Rule | All metrics per Chapter 3 formulas |
| P5‑RUL‑081 | BR‑KN‑091 Data Completeness Rule | <90% → flagged low confidence |
| P5‑RUL‑082 | BR‑KN‑092 Trend Detection Rule | Mann‑Kendall at p<0.05, ≥4 data points |
| P5‑RUL‑083 | BR‑KN‑093 Degradation Alert Rule | Two consecutive negative periods → escalated |
| P5‑RUL‑084 | BR‑KN‑094 Report Completeness Rule | All metrics, trends, gaps, recommendations |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑080 | PO‑KN‑090 Evaluation Frequency Policy | Monthly operational, quarterly strategic |
| P5‑POL‑081 | PO‑KN‑091 Trend Review Policy | Degrading trends addressed within 30 days |
| P5‑POL‑082 | PO‑KN‑092 Report Distribution Policy | Within 10 business days of period end |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑080 | `ComputeKnowledgeQuality` | Command handler |
| P5‑CMD‑081 | `AssessQualityTrends` | Command handler |
| P5‑CMD‑082 | `PublishQualityReport` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑080 | `KnowledgeQualityComputed` | Event type |
| P5‑EVT‑081 | `KnowledgeQualityTrendAssessed` | Event type |
| P5‑EVT‑082 | `KnowledgeQualityReportPublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑080 | `GetKnowledgeQualityMetrics(period)` | Query service |
| P5‑QRY‑081 | `GetQualityTrends(metric, periods)` | Query service |
| P5‑QRY‑082 | `GetQualityReport(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑080 | CA‑KN‑009 5.9.13 | Scheduled → Retrieve → Compute (DE‑KN‑090) → Assess trends (DE‑KN‑091) → Publish (DE‑KN‑092) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑080 | BO‑KN‑008 Continuously Improve Knowledge Intelligence | Verify meta‑domain quality is measured |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑080 | RPT‑KN‑018 Knowledge Quality Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑080 | DASH‑KN‑017 Knowledge Quality Dashboard | UI dashboard |
| P5‑DASH‑081 | DASH‑KN‑018 Knowledge Health Scorecard | UI dashboard |

### 5.13 Capability: CA‑KN‑010 — Explain Knowledge Insights

**Specification:** Knowledge Intelligence Specification, Section 5.10

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑090 | OUT‑KN‑090 Knowledge Explanation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑090 | DE‑KN‑100 Generate Knowledge Explanation | `generateKnowledgeExplanation(artifactId) → Explanation` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑090 | BR‑KN‑100 Explanation Completeness Rule (Knowledge) | Artifact, evidence, causal chain, confidence, traceability, limitations |
| P5‑RUL‑091 | BR‑KN‑101 Evidence Citation Rule | Every claim supported by a citation |
| P5‑RUL‑092 | BR‑KN‑102 Traceability Chain Rule (Knowledge) | Full ARS traceability chain |
| P5‑RUL‑093 | BR‑KN‑103 Natural Language Rule (Knowledge) | Standard explainability template |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑090 | PO‑KN‑100 Explanation Quality Policy (Knowledge) | <60% flagged; <40% not published |
| P5‑POL‑091 | PO‑KN‑101 Explanation Accessibility Policy | Available to humans and AI agents |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑090 | `GenerateKnowledgeExplanation` | Command handler |
| P5‑CMD‑091 | `RegenerateKnowledgeExplanation` | Command handler |
| P5‑CMD‑092 | `EvaluateExplanationQuality` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑090 | `KnowledgeExplanationGenerated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑090 | `GetKnowledgeExplanation(artifactId)` | Query service |
| P5‑QRY‑091 | `GetExplainabilityScores(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑090 | CA‑KN‑010 5.10.13 | Event‑driven on knowledge artifact publication → Generate explanation → Attach → Publish |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑090 | BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence | Verify every knowledge insight is explainable |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑090 | RPT‑KN‑019 Explainability Score Report (Knowledge) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑090 | DASH‑KN‑019 Explainability Overview (Knowledge) | UI dashboard |

### 5.14 Capability: CA‑KN‑011 — Learn From Knowledge

**Specification:** Knowledge Intelligence Specification, Section 5.11

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P5‑OUT‑100 | OUT‑KN‑100 Knowledge Improvement Recommendation | Type + publisher |
| P5‑OUT‑101 | OUT‑KN‑101 Meta‑Domain Calibration Report | Type + publisher |
| P5‑OUT‑102 | OUT‑KN‑102 Meta‑Domain Learning Loop Closure | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P5‑DEC‑100 | DE‑KN‑110 Recommend Knowledge Method Improvement | `recommendMethodImprovement() → Result<Event list, DomainError>` |
| P5‑DEC‑101 | DE‑KN‑111 Recommend Process Improvement | `recommendProcessImprovement() → Result<Event list, DomainError>` |
| P5‑DEC‑102 | DE‑KN‑112 Close the Meta‑Domain Learning Loop | `closeMetaDomainLoop(improvementId) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P5‑RUL‑100 | BR‑KN‑110 Method Improvement Trigger Rule | False discovery >20%, validation <60%, cycle time >25% increase |
| P5‑RUL‑101 | BR‑KN‑111 Calibration Review Rule | Confidence calibration error >0.1 → recalibration |
| P5‑RUL‑102 | BR‑KN‑112 Method Stability Rule | No change >1 per quarter without significant degradation |
| P5‑RUL‑103 | BR‑KN‑113 Process Bottleneck Rule | Highest average cycle time exceeding SLA identified |
| P5‑RUL‑104 | BR‑KN‑114 Process Change Validation Rule | Piloted for one quarter before full adoption |
| P5‑RUL‑105 | BR‑KN‑115 Meta‑Loop Verification Rule | 2 months operational, 2 quarters strategic |
| P5‑RUL‑106 | BR‑KN‑116 Meta‑Loop Rollback Rule | Statistically significant degradation → auto‑rollback |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P5‑POL‑100 | PO‑KN‑110 Method Change Approval Policy | Knowledge Manager approval required |
| P5‑POL‑101 | PO‑KN‑111 Process Change Approval Policy | Affected Domain Managers must approve |
| P5‑POL‑102 | PO‑KN‑112 Meta‑Loop Closure Policy | Before‑after evaluation documented and reported quarterly |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P5‑CMD‑100 | `AnalyzeKnowledgePerformance` | Command handler |
| P5‑CMD‑101 | `RecommendMethodImprovement` | Command handler |
| P5‑CMD‑102 | `RecommendProcessImprovement` | Command handler |
| P5‑CMD‑103 | `EvaluateMetaImprovement` | Command handler |
| P5‑CMD‑104 | `RollbackMetaImprovement` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P5‑EVT‑100 | `KnowledgeImprovementRecommended` | Event type |
| P5‑EVT‑101 | `KnowledgeProcessImprovementRecommended` | Event type |
| P5‑EVT‑102 | `KnowledgeMetaLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P5‑QRY‑100 | `GetKnowledgeImprovementHistory(period)` | Query service |
| P5‑QRY‑101 | `GetActiveKnowledgeImprovements()` | Query service |
| P5‑QRY‑102 | `GetKnowledgeLearningEffectiveness()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P5‑BEH‑100 | CA‑KN‑011 5.11.13 | Scheduled/event‑driven → Retrieve quality data → Recommend method (DE‑KN‑110) → Recommend process (DE‑KN‑111) → Implement → Close loop (DE‑KN‑112) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P5‑VFY‑100 | BO‑KN‑008 Continuously Improve Knowledge Intelligence | Verify meta‑domain learning loop is active |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P5‑RPT‑100 | RPT‑KN‑020 Knowledge Improvement Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P5‑DASH‑100 | DASH‑KN‑020 Meta‑Domain Learning Dashboard | UI dashboard |

### 5.15 AI Copilot

| ID | Item | Implementation |
|----|------|---------------|
| P5‑AI‑001 | Command Palette — UI component that opens via keyboard shortcut/toolbar button, accepts search text, invokes AI service to generate suggestions | Arch. Blueprint §14.5 |
| P5‑AI‑002 | Command Palette — AI suggestion generation: debounced search text → call Knowledge Intelligence (or local heuristic) → return list of `CopilotSuggestion` (title, description, action) | Arch. Blueprint §14.5 |
| P5‑AI‑003 | Command Palette — action dispatch: on user selection, translate suggestion into `WorkspaceAction` and dispatch via `ExecuteWorkspaceAction` | Arch. Blueprint §14.5 |
| P5‑AI‑004 | Workspace Actions for AI — define `WorkspaceAction` DU cases for all known cross‑cutting actions (NavigateTo, RefreshActiveWorkspace, etc.) plus extensible AI‑specific actions | Arch. Blueprint §14.6 |
| P5‑AI‑005 | Workspace Actions for AI — implement `WorkspaceEngine.executeWorkspaceAction` that processes both human and AI actions identically | Arch. Blueprint §14.6 |
| P5‑AI‑006 | Workspace Actions for AI — log `CommandTrace` with `Origin = Ai` in Session command history | Arch. Blueprint §14.8 |
| P5‑AI‑007 | Autonomy Contracts enforcement — register `AutonomyContract` for each AI agent at startup; validate every AI‑initiated `WorkspaceAction` against contract via `DecisionCore.Autonomy.validateAction` | Arch. Blueprint §10.4 |
| P5‑AI‑008 | Autonomy Contracts enforcement — block disallowed actions; allow Guardrailed actions within value thresholds; require approval for actions exceeding thresholds | Arch. Blueprint §10.4 |
| P5‑AI‑009 | Autonomy Contracts enforcement — display AI recommendations with confidence and provenance in the UI; user can accept or reject | Arch. Blueprint §10.3 |

### 5.16 External Interfaces — Knowledge

#### API Endpoints

| ID | Endpoint | Method | Path | Owner Capability |
|----|----------|--------|------|------------------|
| P5‑API‑001 | Knowledge Graph Query | GET, POST | `/api/v1/knowledge/graph`, `/api/v1/knowledge/graph/validate` | CA‑KN‑001 |
| P5‑API‑002 | Pattern Detection & Query | POST, GET | `/api/v1/knowledge/patterns/detect`, `/api/v1/knowledge/patterns` | CA‑KN‑002 |
| P5‑API‑003 | Root‑Cause Analysis | POST, GET | `/api/v1/knowledge/root-cause/analyze`, `/api/v1/knowledge/root-cause/{id}` | CA‑KN‑003 |
| P5‑API‑004 | Improvement Portfolio | POST, GET | `/api/v1/knowledge/improvements`, `/api/v1/knowledge/improvements/portfolio` | CA‑KN‑004 |
| P5‑API‑005 | Best Practices | POST, GET | `/api/v1/knowledge/practices`, `/api/v1/knowledge/practices/{id}` | CA‑KN‑005 |
| P5‑API‑006 | Feedback Loops | GET | `/api/v1/knowledge/feedback-loops`, `/api/v1/knowledge/feedback-loops/{id}` | CA‑KN‑006 |
| P5‑API‑007 | Enterprise Memory Query | POST, GET | `/api/v1/knowledge/memory/query`, `/api/v1/knowledge/memory/events/{id}` | CA‑KN‑007 |
| P5‑API‑008 | AI Agent Knowledge Query | POST | `/api/v1/knowledge/agent/query` | CA‑KN‑008 |
| P5‑API‑009 | Knowledge Quality | GET | `/api/v1/knowledge/quality`, `/api/v1/knowledge/quality/trends` | CA‑KN‑009 |
| P5‑API‑010 | Knowledge Explanation | GET | `/api/v1/knowledge/explanations/{artifactId}` | CA‑KN‑010 |

#### Integration Events — Published

| ID | Event | Publisher | Consumers |
|----|-------|-----------|-----------|
| P5‑INT‑001 | `KnowledgeGraphUpdated` | CA‑KN‑001 | All Knowledge capabilities, AI Agents |
| P5‑INT‑002 | `PatternValidated` | CA‑KN‑002 | CA‑KN‑003, CA‑KN‑005, CA‑KN‑010 |
| P5‑INT‑003 | `RootCauseIdentified` | CA‑KN‑003 | CA‑KN‑004, CA‑KN‑010 |
| P5‑INT‑004 | `ImprovementProposed` | CA‑KN‑004 | Domain Managers |
| P5‑INT‑005 | `BestPracticePublished` | CA‑KN‑005 | All domains, AI Agents |
| P5‑INT‑006 | `FeedbackLoopClosed` | CA‑KN‑006 | CA‑KN‑007, CA‑KN‑011 |
| P5‑INT‑007 | `EventRecorded` | CA‑KN‑007 | CA‑KN‑002, CA‑KN‑008 |
| P5‑INT‑008 | `KnowledgeResponseServed` | CA‑KN‑008 | CA‑KN‑009 |
| P5‑INT‑009 | `KnowledgeQualityReportPublished` | CA‑KN‑009 | CA‑KN‑011, Executive |
| P5‑INT‑010 | `KnowledgeExplanationGenerated` | CA‑KN‑010 | CA‑KN‑008, CA‑KN‑011 |

### 5.17 Appendix — Knowledge

| ID | Item | Implementation |
|----|------|---------------|
| P5‑APP‑001 | Knowledge Exception Priority Matrix | Configuration document / runtime config |
| P5‑APP‑002 | Knowledge Enterprise Glossary | Documentation |
| P5‑APP‑003 | Knowledge Formula Reference | Documentation |
| P5‑APP‑004 | Knowledge References | Documentation |

---

We’ll now rebuild Phases 6 through 11 with exactly the same exhaustive granularity used for Phases 0–5. Every capability is fully expanded; every ARS identifier is listed individually.

---

## Phase 6 — Integration, Host & UI: Todo Items

This phase composes all bounded contexts into the deployable application, builds the API gateway, external adapters, and the Bolero UI.

### 6.1 Architecture & Infrastructure — Composition Root

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑ARC‑001 | Create `Medhavi.Nexus` project as the composition root | Arch. Blueprint §2.3 |
| P6‑ARC‑002 | Wire Demand bounded context: register repository, projection agents, capabilities, API, event subscriptions | Arch. Blueprint §3.3 |
| P6‑ARC‑003 | Wire Supply bounded context: register repository, projection agents, capabilities, API, event subscriptions, Planning Engine | Arch. Blueprint §3.3 |
| P6‑ARC‑004 | Wire Promise bounded context: register repository, projection agents, capabilities, API, event subscriptions | Arch. Blueprint §3.3 |
| P6‑ARC‑005 | Wire Scenario bounded context: register repository, projection agents, capabilities, API, event subscriptions | Arch. Blueprint §3.3 |
| P6‑ARC‑006 | Wire Knowledge bounded context: register repository, projection agents, capabilities, API, event subscriptions | Arch. Blueprint §3.3 |
| P6‑ARC‑007 | Wire MasterData and Integration supporting contexts | Arch. Blueprint §3.1 |
| P6‑ARC‑008 | Configure `DomainEventBus` in‑process pub/sub with all domain event subscriptions | Arch. Blueprint §4.4.1 |
| P6‑ARC‑009 | Register all cross‑context event subscriptions (Demand→Supply, Supply→Promise, etc.) per event routing map | Arch. Blueprint §4.7 |
| P6‑ARC‑010 | Register all `ProjectionAgent` instances and wire to event bus | Arch. Blueprint §6.2 |
| P6‑ARC‑011 | Wire `CircuitBreaker` agents for all external dependencies | Arch. Blueprint §13.3 |
| P6‑ARC‑012 | Wire `HealthCheck` aggregation across all bounded contexts | Arch. Blueprint §12.6 |
| P6‑ARC‑013 | Wire `FeatureFlags` and `AppSettings` into all contexts | Arch. Blueprint §16.3–16.4 |

### 6.2 Architecture & Infrastructure — Integration Layer

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑INT‑001 | Create `Medhavi.Integration` project structure | Arch. Blueprint §8.2 |
| P6‑INT‑002 | Implement `ErpAdapter` — ingest demand orders, supply orders, inventory updates from ERP | Arch. Blueprint §8.2.2 |
| P6‑INT‑003 | Implement `WmsAdapter` — ingest shipment confirmations, inventory adjustments | Arch. Blueprint §8.2.2 |
| P6‑INT‑004 | Implement `MesAdapter` — ingest production order completions, quality results | Arch. Blueprint §8.2.2 |
| P6‑INT‑005 | Implement `DemandAcl` — translate external order formats to `IngestDemandLineCmd` | Arch. Blueprint §8.2.1 |
| P6‑INT‑006 | Implement `SupplyAcl` — translate external inventory updates to `IngestSupplyData` | Arch. Blueprint §8.2.1 |
| P6‑INT‑007 | Implement `MasterDataAcl` — translate external product, BOM, supplier data | Arch. Blueprint §8.2.1 |
| P6‑INT‑008 | Register integration adapters in `Medhavi.Nexus` | Arch. Blueprint §3.3 |

### 6.3 Architecture & Infrastructure — API Gateway

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑HUB‑001 | Create `Medhavi.Hub` ASP.NET Core project | Arch. Blueprint §2.1 |
| P6‑HUB‑002 | Configure program startup: load `AppSettings`, `FeatureFlags`, initialise all bounded contexts | Arch. Blueprint §16.5 |
| P6‑HUB‑003 | Register all Demand API endpoints (listed in Phase 1, §1.7) | Demand Spec §6.2 |
| P6‑HUB‑004 | Register all Supply API endpoints (listed in Phase 2, §2.11) | Supply Spec §6.2 |
| P6‑HUB‑005 | Register all Promise API endpoints (listed in Phase 3, §3.11) | Promise Spec §6.2 |
| P6‑HUB‑006 | Register all Scenario API endpoints (listed in Phase 4, §4.11) | Scenario Spec §6.2 |
| P6‑HUB‑007 | Register all Knowledge API endpoints (listed in Phase 5, §5.16) | Knowledge Spec §6.2 |
| P6‑HUB‑008 | Configure authentication middleware (OAuth 2.0 / OpenID Connect) | Arch. Blueprint §17.2 |
| P6‑HUB‑009 | Configure RBAC authorisation middleware with role‑to‑policy mapping | Arch. Blueprint §17.3 |
| P6‑HUB‑010 | Configure CORS policy for known UI origins | Arch. Blueprint §17.7 |
| P6‑HUB‑011 | Configure rate limiting per user and per AI agent | Arch. Blueprint §17.7 |
| P6‑HUB‑012 | Configure HTTPS redirection and HSTS | Arch. Blueprint §17.7 |
| P6‑HUB‑013 | Configure health check endpoint (`/health`) aggregating all `HealthCheck` components | Arch. Blueprint §12.6 |
| P6‑HUB‑014 | Configure Swagger / OpenAPI documentation generation from `Medhavi.Contracts` | Arch. Blueprint §17.4 |
| P6‑HUB‑015 | Configure API versioning (URL‑based: `/api/v1/…`) | Arch. Blueprint §17.4 |
| P6‑HUB‑016 | Implement global exception handler mapping `ApplicationError` → HTTP status codes | Arch. Blueprint §13.1.1 |
| P6‑HUB‑017 | Implement request/response logging middleware with `CorrelationId` propagation | Arch. Blueprint §11.3 |

### 6.4 Architecture & Infrastructure — Bolero UI Shell

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑UI‑001 | Create `Medhavi.Web` Bolero project targeting Blazor Server | Arch. Blueprint §14.1 |
| P6‑UI‑002 | Implement `SystemShell` — dependency injection container, loads services, Stores, authentication | Arch. Blueprint §14.2 |
| P6‑UI‑003 | Implement `AppShellModel` — top‑level Elmish model: `Session`, `ActiveWorkspace`, `NavigationbarExpanded`, `RightSidebarExpanded`, `CommandPaletteOpen`, component states | Arch. Blueprint §14.2 |
| P6‑UI‑004 | Implement `AppShellEnv` — injects `DemandLineQueries`, `StoreRegistry`, `TooltipService`, `MasterDataService` | Arch. Blueprint §14.2 |
| P6‑UI‑005 | Implement `AppShell` Elmish update with full message routing: `ToggleCommandPalette`, `ExecuteWorkspaceAction`, `ReservationWorkspaceMsg`, `MasterDataMsg`, `AppbarMsg`, `NavigationMsg`, `SessionMsg`, etc. | Arch. Blueprint §14.2 |
| P6‑UI‑006 | Implement `AppShellView` — renders `Appbar`, `Navigation`, active workspace, right sidebar, command palette dialog, settings dialog | Arch. Blueprint §14.2 |
| P6‑UI‑007 | Implement `Session` module — `Model` (User, Theme, ConnectionStatus, Notifications, Operations, Activities, PlanningContext, CommandHistory), `Msg`, `init`, `update` | Arch. Blueprint §14.8 |
| P6‑UI‑008 | Implement `Workspace` DU and `WorkspaceAction` DU | Arch. Blueprint §14.6 |
| P6‑UI‑009 | Implement `WorkspaceEngine.executeWorkspaceAction` — navigates to workspace, lazy‑initialises state, dispatches `Initialize` | Arch. Blueprint §14.6 |
| P6‑UI‑010 | Implement `updateChildWithOutput` utility for parent‑child Elmish composition | Arch. Blueprint §14.4 |
| P6‑UI‑011 | Implement `Appbar` component — model, update, view, `Output` type (scenario selection, command palette toggle, theme, notifications, user role cycling, logout) | Arch. Blueprint §14.2 |
| P6‑UI‑012 | Implement `Navigation` component — `WorkspaceNavigation` DU, routing, model, update, view, `Output` | Arch. Blueprint §14.2 |
| P6‑UI‑013 | Implement `CommandTrace` type with `CommandOrigin` (Human, Ai, System) and `CommandStatus` (Queued, Succeeded, Failed) | Arch. Blueprint §14.8 |
| P6‑UI‑014 | Implement `Notification` type and notification handling in `Session` | Arch. Blueprint §14.8 |
| P6‑UI‑015 | Implement `Operation` type and operation tracking in `Session` | Arch. Blueprint §14.8 |

### 6.5 Stores Implementation

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑STR‑001 | Implement `WorkspaceStore<'TState>` factory with `Get`, `Refresh`, `MarkStale`, `Subscribe`, `Unsubscribe`, `Clear` | Arch. Blueprint §14.3.1, your existing `WorkspaceStore` module |
| P6‑STR‑002 | Implement `WorkspaceSnapshot<'data>` type with `Data`, `Freshness`, `Version`, `LastRefreshUtc`, `Error` | Arch. Blueprint §14.3.1 |
| P6‑STR‑003 | Implement `Freshness` DU: `Fresh`, `Stale`, `Loading`, `Failed of string` | Arch. Blueprint §14.3.1 |
| P6‑STR‑004 | Implement `StoreEvent<'TState>` DU: `StateChanged`, `ContextChanged`, `ErrorOccurred` | Arch. Blueprint §14.3.1 |
| P6‑STR‑005 | Implement `PlanningContextStore` with `Get`, `Set`, `Update`, `Subscribe`, `Unsubscribe` | Arch. Blueprint §14.3.2, your existing `PlanningContextStore` |
| P6‑STR‑006 | Implement `WorkspaceStoreRegistry` with `Register`, `TryGet`, `MarkAllStale`, `ClearAll`, auto‑subscribe to context changes | Arch. Blueprint §14.3.3, your existing `WorkspaceStoreRegistry` |
| P6‑STR‑007 | Implement `SubscriptionId` type | Arch. Blueprint §14.3.1 |
| P6‑STR‑008 | Implement `StoreComposition.createRegistry` — creates all domain stores, registers them, starts projection subscriptions | Arch. Blueprint §14.5, your existing `StoreComposition` |
| P6‑STR‑009 | Implement `DemandStore` with `loadFromBackend`, `updateStore`, `StoreNotificationHandlers` (`OnCreated`, `OnUpdated`, `OnDeleted`) | Arch. Blueprint §14.5, your existing `DemandStore` |
| P6‑STR‑010 | Implement `SupplyStore` with `loadFromBackend`, `updateStore`, notification handlers | Arch. Blueprint §14.5 |
| P6‑STR‑011 | Implement `MaterialReservationStore` | Arch. Blueprint §14.5, your existing `MaterialReservationStore` |
| P6‑STR‑012 | Implement `CapacityStore` | Arch. Blueprint §14.5, your existing `CapacityStore` |
| P6‑STR‑013 | Implement `ScenarioStore` | Arch. Blueprint §14.5 |
| P6‑STR‑014 | Implement `PromiseStore` | Arch. Blueprint §14.5 |
| P6‑STR‑015 | Implement `KnowledgeStore` | Arch. Blueprint §14.5 |
| P6‑STR‑016 | Implement `ProjectionSubscription` layer — subscribes to `DomainEventBus` notifications and routes to store handlers | Arch. Blueprint §14.4, your existing `ProjectionSubscription` |
| P6‑STR‑017 | Implement `DemandData` type and `DemandData.ofList`, `addOrUpdate`, `remove`, `toList` helpers | Arch. Blueprint §14.5, your existing `DemandData` |

### 6.6 Workspace Implementations

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑WS‑001 | Implement `MaterialReservation` workspace — `Model`, `Msg`, `Action`, `ReservationEnv`, `Output`, `init`, `update`, `view` | Arch. Blueprint §14.6, your existing `MaterialReservation` |
| P6‑WS‑002 | Implement `ResourceScheduling` workspace — full Elmish component | Arch. Blueprint §14.6 |
| P6‑WS‑003 | Implement `ScenarioManagement` workspace — full Elmish component | Arch. Blueprint §14.6 |
| P6‑WS‑004 | Implement `MasterData` workspace — full Elmish component | Arch. Blueprint §14.6 |
| P6‑WS‑005 | Implement `DemandPanels` — reusable panel components (DemandPanel, ForecastPanel, etc.) with upward intents | Arch. Blueprint §14.2 |
| P6‑WS‑006 | Implement `SupplyPanels` — reusable panel components | Arch. Blueprint §14.2 |
| P6‑WS‑007 | Implement `PromisePanels` — reusable panel components | Arch. Blueprint §14.2 |
| P6‑WS‑008 | Implement `ScenarioPanels` — reusable panel components | Arch. Blueprint §14.2 |
| P6‑WS‑009 | Implement `KnowledgePanels` — reusable panel components | Arch. Blueprint §14.2 |


### 6.7A Admin Center Workspace

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑ADM‑001 | Implement `AdminCenter` workspace in `Medhavi.Web` — shell with navigation between admin panels (Traceability, Audit, AI, Config, Health, Event Store) | Arch. Blueprint §14.9 |
| P6‑ADM‑002 | Implement `TraceabilityExplorer` panel — correlation ID search input, causation chain tree view, ARS chain reconstruction, decision trace drill‑down | Arch. Blueprint §14.9 |
| P6‑ADM‑003 | Implement `AuditLogViewer` panel — filterable grid by user/agent/domain/action/date, human vs. AI indicator, policy change history | Arch. Blueprint §14.9 |
| P6‑ADM‑004 | Implement `AIDecisionReview` panel — AI recommendation log, agent effectiveness metrics (acceptance rate, improvement rate), autonomy contract viewer, escalated decisions list | Arch. Blueprint §14.9 |
| P6‑ADM‑005 | Implement `ConfigurationManager` panel — feature flag toggle switches (with audit), read‑only config viewer (connection strings masked), ARS identifier searchable list | Arch. Blueprint §14.9 |
| P6‑ADM‑006 | Implement `SystemHealth` panel — component health grid with status icons, circuit breaker state indicators, projection lag gauges with alert thresholds, event ingest rate chart | Arch. Blueprint §14.9 |
| P6‑ADM‑007 | Implement `EventStoreBrowser` panel — stream list, event table with position/timestamp, envelope detail modal, DLQ stream browser, replay trigger button | Arch. Blueprint §14.9 |
| P6‑ADM‑008 | Add `Administrator` role gate — all admin panels return 403 / show "Access Denied" for non‑Administrator users | Arch. Blueprint §17.3 |
| P6‑ADM‑009 | Add `AdminCenter` entry to Navigation component — visible only when `Session.User.Role = Administrator` | Arch. Blueprint §14.2 |
| P6‑ADM‑010 | Implement admin API endpoints for admin‑only data: `/api/v1/admin/traces/{correlationId}`, `/api/v1/admin/audit`, `/api/v1/admin/ai-decisions`, `/api/v1/admin/health`, `/api/v1/admin/events/{stream}` | Arch. Blueprint §6.2 |

### 6.7B AI Copilot UI Integration

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑AI‑001 | Implement Command Palette UI — `RadzenDialog` with search input bound to `CommandPaletteSearchText` | Arch. Blueprint §14.5, your existing `AppShell` |
| P6‑AI‑002 | Implement debounced AI suggestion generation — on text change, call Knowledge Intelligence `POST /api/v1/knowledge/agent/query` or local heuristic | Arch. Blueprint §14.5 |
| P6‑AI‑003 | Implement suggestion list display — render `CopilotSuggestion` items (title, description) in dropdown | Arch. Blueprint §14.5 |
| P6‑AI‑004 | Implement suggestion selection — translate to `WorkspaceAction` and dispatch via `ExecuteWorkspaceAction` | Arch. Blueprint §14.5 |
| P6‑AI‑005 | Implement `CopilotSuggestion` type (title, description, action) | Arch. Blueprint §14.5 |
| P6‑AI‑006 | Wire keyboard shortcut (Ctrl+K / Cmd+K) to `ToggleCommandPalette` | Arch. Blueprint §14.5 |
| P6‑AI‑007 | Log all AI‑initiated actions with `CommandOrigin.Ai` in `CommandHistory` | Arch. Blueprint §14.8 |

### 6.8 End‑to‑End Tests

| ID | Todo Item | Source |
|----|-----------|--------|
| P6‑E2E‑001 | Order‑to‑Promise E2E test: create demand → generate forecast → generate supply plan → promise order → verify promise status and events | Arch. Blueprint §19.4 |
| P6‑E2E‑002 | Scenario What‑If E2E test: define scenario → simulate → compare → recommend → adopt → verify plan updates in operational domains | Arch. Blueprint §19.4 |
| P6‑E2E‑003 | Knowledge Discovery E2E test: inject quality report events → verify cross‑domain pattern discovered → verify root‑cause analysis completed → verify improvement proposed | Arch. Blueprint §19.4 |
| P6‑E2E‑004 | AI Copilot E2E test: open command palette → type query → receive AI suggestions → select suggestion → verify action executed → verify CommandHistory recorded | Arch. Blueprint §19.6 |
| P6‑E2E‑005 | Full API contract test: verify all endpoints respond with correct schemas per `Medhavi.Contracts` | Arch. Blueprint §19.4 |
| P6‑E2E‑006 | Cross‑context event flow test: publish `ForecastPublished` → verify `SupplyPlanGenerated` → verify `PromiseConfirmed` → verify `SupplyConsumed` | Arch. Blueprint §19.4 |
| P6‑E2E‑007 | Resilience test: simulate event store unavailable → verify 503 returned → verify stale projections served → restore → verify catch‑up | Arch. Blueprint §19.3 |

---

We’ll now build the exhaustive Todo Items for **Phase 7 — Sensing & Exception Detection**, with every capability fully expanded to match the density of Phases 0–5.

---

## Phase 7 — Sensing & Exception Detection: Todo Items

This phase activates all Sense and Detect capabilities across Demand, Supply, Promise, and Scenario.

### 7.1 Capability: CA‑DI‑003 — Sense Demand

**Specification:** Demand Intelligence Specification, Section 5.3

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑DI‑OUT‑001 | OUT‑DI‑030 Demand Change Alert | Type + publisher |
| P7‑DI‑OUT‑002 | OUT‑DI‑031 Disruption Impact Estimate | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑DI‑DEC‑001 | DE‑DI‑030 Detect Demand Change | `detectDemandChange(signals, baseline) → Result<Event list, DomainError>` |
| P7‑DI‑DEC‑002 | DE‑DI‑031 Trigger Forecast Refresh | `triggerForecastRefresh(change) → Result<Event list, DomainError>` |
| P7‑DI‑DEC‑003 | DE‑DI‑032 Accept Streaming Signal | `acceptStreamingSignal(signal) → Result<Event list, DomainError>` |

#### Rules (for DE‑DI‑030)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑RUL‑001 | BR‑DI‑030 Deviation Threshold Rule | Significant if >2.5σ for ≥3 consecutive periods; Critical if ≥4σ sustained for 2 periods |
| P7‑DI‑RUL‑002 | BR‑DI‑031 Signal Corroboration Rule | Critical alerts must be corroborated by ≥2 independent signal sources |
| P7‑DI‑RUL‑003 | BR‑DI‑032 High‑Priority Product Sensitivity Rule | Threshold lowered to 2.0σ for high‑priority products |
| P7‑DI‑RUL‑004 | BR‑DI‑033 Noise Filter Rule | Deviation <1.5σ and Lumpy pattern → classified as Noise and suppressed |

#### Rules (for DE‑DI‑031)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑RUL‑005 | BR‑DI‑034 Refresh Benefit Rule | Refresh only if expected WAPE improvement >2% |

#### Rules (for DE‑DI‑032)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑RUL‑006 | BR‑DI‑035 Streaming Signal Latency Rule | POS ≤15 min, social sentiment ≤1 hour |
| P7‑DI‑RUL‑007 | BR‑DI‑036 Streaming Signal Range Rule | Outside 3σ → accept with low‑confidence flag unless duplicate |
| P7‑DI‑RUL‑008 | BR‑DI‑037 Duplicate Detection Rule | Reject if same source/product/location/time‑bucket/value within same hour |

#### Policies (for DE‑DI‑030)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑POL‑001 | PO‑DI‑030 Alert Escalation Policy | Significant → Demand Planner; Critical → Demand Manager + mandatory forecast refresh |
| P7‑DI‑POL‑002 | PO‑DI‑031 Automatic Forecast Refresh Policy | Critical change auto‑triggers new forecast cycle |

#### Policies (for DE‑DI‑031)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑POL‑003 | PO‑DI‑032 Refresh Authorization Policy | Partial refreshes for Significant changes → auto; Full refreshes → Demand Manager approval unless Critical |

#### Policies (for DE‑DI‑032)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑POL‑004 | PO‑DI‑033 Streaming Signal Acceptance Policy | Auto‑accept if all rules pass; low‑confidence flagged signals batched hourly to Data Steward |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑DI‑CMD‑001 | `IngestSignalStream` | Command handler |
| P7‑DI‑CMD‑002 | `EvaluateDemandDeviation` | Command handler |
| P7‑DI‑CMD‑003 | `TriggerForecastRefresh` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑DI‑EVT‑001 | `DemandChangeDetected` | Event type |
| P7‑DI‑EVT‑002 | `ForecastRefreshTriggered` | Event type |
| P7‑DI‑EVT‑003 | `SignalAccepted` | Event type |
| P7‑DI‑EVT‑004 | `SignalDiscarded` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑DI‑QRY‑001 | `GetCurrentDemandDeviation(product, location)` | Query service |
| P7‑DI‑QRY‑002 | `GetActiveAlerts()` | Query service |
| P7‑DI‑QRY‑003 | `GetSignalHistory(product, location, period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑DI‑BEH‑001 | CA‑DI‑003 5.3.10 | Implement: Continuous ingestion → Pre‑process signals → Aggregate → Compare to baseline → Detect change (DE‑DI‑030) → Trigger refresh (DE‑DI‑031) → Publish alerts |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑DI‑VFY‑001 | BO‑DI‑003 Improve Enterprise Responsiveness | Verify changes detected and alerts raised within time targets |
| P7‑DI‑VFY‑002 | BO‑DI‑001 Deliver Trusted Demand Understanding | Verify signal quality assessment |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑DI‑RPT‑001 | RPT‑DI‑008 Change Detection Report | Report generation |
| P7‑DI‑RPT‑002 | RPT‑DI‑002 Signal Quality Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑DI‑DASH‑001 | DASH‑DI‑006 Real‑Time Demand Dashboard | UI dashboard |
| P7‑DI‑DASH‑002 | DASH‑DI‑007 Signal Health Monitor | UI dashboard |

### 7.2 Capability: CA‑DI‑008 — Detect Demand Exceptions

**Specification:** Demand Intelligence Specification, Section 5.8

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑DI‑EX‑OUT‑001 | OUT‑DI‑040 Demand Exception Record | Type + publisher |
| P7‑DI‑EX‑OUT‑002 | OUT‑DI‑041 Exception Resolution Recommendation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑DI‑EX‑DEC‑001 | DE‑DI‑080 Classify Exception Type | `classifyExceptionType(anomaly) → Result<Event list, DomainError>` |
| P7‑DI‑EX‑DEC‑002 | DE‑DI‑081 Prioritize Exception | `prioritizeException(exception) → Result<Event list, DomainError>` |
| P7‑DI‑EX‑DEC‑003 | DE‑DI‑082 Resolve Exception | `resolveException(exception) → Result<Event list, DomainError>` |

#### Rules (for DE‑DI‑080)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑EX‑RUL‑001 | BR‑DI‑080 Exception Classification Rule | >3σ single period → Outlier; >2.5σ sustained ≥5 periods → Level Shift; forecast errors >PI in ≥80% periods for 4 weeks → Model Failure; missing actuals → Data Gap; <2σ → False Positive |
| P7‑DI‑EX‑RUL‑002 | BR‑DI‑081 Signal Corroboration Rule | Level Shift/Trend Break must be corroborated by independent signal source |

#### Rules (for DE‑DI‑081)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑EX‑RUL‑003 | BR‑DI‑082 Exception Priority Matrix Rule | Matrix maps exception type × business priority → severity |
| P7‑DI‑EX‑RUL‑004 | BR‑DI‑083 Escalation Rule | Critical exceptions escalated to Demand Manager with push notification |

#### Rules (for DE‑DI‑082)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑DI‑EX‑RUL‑005 | BR‑DI‑084 Auto‑Resolution Eligibility Rule | Auto‑resolve if Outlier/DataGap/LevelShift AND confidence ≥90% AND not Critical |
| P7‑DI‑EX‑RUL‑006 | BR‑DI‑085 Resolution Documentation Rule | Every resolution logged with action, timestamp, actor |

#### Policies (for DE‑DI‑080)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑EX‑POL‑001 | PO‑DI‑080 False Positive Filtering Policy | False Positives logged but not presented unless recur 3× in 7‑day window |

#### Policies (for DE‑DI‑081)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑EX‑POL‑002 | PO‑DI‑081 Exception Escalation Policy | Critical → auto‑escalated; High → assigned planner, escalated if not acknowledged within 4 hours |

#### Policies (for DE‑DI‑082)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑DI‑EX‑POL‑003 | PO‑DI‑082 Auto‑Resolution Policy | Eligible exceptions auto‑resolved; others require planner intervention |
| P7‑DI‑EX‑POL‑004 | PO‑DI‑083 Resolution Timeliness SLA Policy | Critical: 2 hours, High: 8 hours, Medium: 48 hours |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑DI‑EX‑CMD‑001 | `ScanForExceptions` | Command handler |
| P7‑DI‑EX‑CMD‑002 | `ClassifyException` | Command handler |
| P7‑DI‑EX‑CMD‑003 | `ResolveException` | Command handler |
| P7‑DI‑EX‑CMD‑004 | `EscalateException` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑DI‑EX‑EVT‑001 | `ExceptionDetected` | Event type |
| P7‑DI‑EX‑EVT‑002 | `ExceptionClassified` | Event type |
| P7‑DI‑EX‑EVT‑003 | `ExceptionPrioritized` | Event type |
| P7‑DI‑EX‑EVT‑004 | `ExceptionResolved` | Event type |
| P7‑DI‑EX‑EVT‑005 | `ExceptionEscalated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑DI‑EX‑QRY‑001 | `GetActiveExceptions(filter)` | Query service |
| P7‑DI‑EX‑QRY‑002 | `GetExceptionHistory(item, period)` | Query service |
| P7‑DI‑EX‑QRY‑003 | `GetExceptionSLAReport(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑DI‑EX‑BEH‑001 | CA‑DI‑008 5.8.10 | Implement: Continuous monitoring → Anomaly detection → Classify (DE‑DI‑080) → Prioritize (DE‑DI‑081) → Resolve (DE‑DI‑082) → Track SLAs |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑DI‑EX‑VFY‑001 | BO‑DI‑003 Improve Enterprise Responsiveness | Verify exceptions resolved within SLAs |
| P7‑DI‑EX‑VFY‑002 | BO‑DI‑005 Increase Planning Automation | Verify auto‑resolution rates |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑DI‑EX‑RPT‑001 | RPT‑DI‑015 Exception Summary Report | Report generation |
| P7‑DI‑EX‑RPT‑002 | RPT‑DI‑016 SLA Compliance Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑DI‑EX‑DASH‑001 | DASH‑DI‑014 Exception Monitor | UI dashboard |
| P7‑DI‑EX‑DASH‑002 | DASH‑DI‑015 Exception Analytics Dashboard | UI dashboard |

### 7.3 Capability: CA‑SI‑009 — Sense Supply Changes

**Specification:** Supply Intelligence Specification, Section 5.9

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑SI‑OUT‑001 | OUT‑SI‑090 Supply Change Alert | Type + publisher |
| P7‑SI‑OUT‑002 | OUT‑SI‑091 Disruption Impact Estimate | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑SI‑DEC‑001 | DE‑SI‑090 Detect Supply Disruption | `detectSupplyDisruption(event, baseline) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SI‑RUL‑001 | BR‑SI‑090 Disruption Classification Rule | Delay >1 day or shortfall >10% for critical items → Critical; non‑critical: >3 days or >20% → Significant |
| P7‑SI‑RUL‑002 | BR‑SI‑091 Corroboration Rule (Supply) | Critical disruptions must be corroborated by supplier confirmation or independent tracking |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SI‑POL‑001 | PO‑SI‑090 Disruption Escalation Policy | Critical → escalated immediately to Supply Manager and affected planners |
| P7‑SI‑POL‑002 | PO‑SI‑091 Automatic Plan Refresh Policy | Critical disruptions auto‑trigger supply plan re‑generation |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑SI‑CMD‑001 | `EvaluateSupplyEvent` | Command handler |
| P7‑SI‑CMD‑002 | `AcknowledgeAlert` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑SI‑EVT‑001 | `SupplyDisruptionDetected` | Event type |
| P7‑SI‑EVT‑002 | `SupplyAlertEscalated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑SI‑QRY‑001 | `GetActiveSupplyAlerts()` | Query service |
| P7‑SI‑QRY‑002 | `GetDisruptionHistory(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑SI‑BEH‑001 | CA‑SI‑009 5.9.13 | Implement: Ingest real‑time events → Correlate to planned orders → Detect disruption (DE‑SI‑090) → Publish alerts → Trigger downstream actions |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑SI‑VFY‑001 | BO‑SI‑004 Ensure Supply Continuity | Verify disruptions detected and alerted |
| P7‑SI‑VFY‑002 | BO‑SI‑003 Maximize Capacity Utilization | Verify capacity disruptions detected |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑SI‑RPT‑001 | RPT‑SI‑018 Supply Disruption Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑SI‑DASH‑001 | DASH‑SI‑017 Supply Disruption Monitor | UI dashboard |

### 7.4 Capability: CA‑SI‑011 — Detect Supply Exceptions

**Specification:** Supply Intelligence Specification, Section 5.11

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑SI‑EX‑OUT‑001 | OUT‑SI‑110 Supply Exception Record | Type + publisher |
| P7‑SI‑EX‑OUT‑002 | OUT‑SI‑111 Exception Resolution Recommendation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑SI‑EX‑DEC‑001 | DE‑SI‑110 Classify Supply Exception | `classifySupplyException(anomaly) → Result<Event list, DomainError>` |
| P7‑SI‑EX‑DEC‑002 | DE‑SI‑111 Prioritize Supply Exception | `prioritizeSupplyException(exception) → Result<Event list, DomainError>` |
| P7‑SI‑EX‑DEC‑003 | DE‑SI‑112 Resolve Supply Exception | `resolveSupplyException(exception) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑110)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SI‑EX‑RUL‑001 | BR‑SI‑110 Exception Classification Rule (Supply) | Shortage: projected inventory < safety stock ≥2 periods; Late Delivery: supplier confirmation > required +1 day; etc. |

#### Rules (for DE‑SI‑111)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SI‑EX‑RUL‑002 | BR‑SI‑111 Exception Priority Matrix Rule (Supply) | Matrix maps exception type × item priority → severity |
| P7‑SI‑EX‑RUL‑003 | BR‑SI‑112 Escalation Rule (Supply) | Critical → Supply Manager |

#### Rules (for DE‑SI‑112)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SI‑EX‑RUL‑004 | BR‑SI‑113 Auto‑Resolution Eligibility Rule (Supply) | Auto‑resolve if confidence ≥90% and severity not Critical |
| P7‑SI‑EX‑RUL‑005 | BR‑SI‑114 Resolution Documentation Rule (Supply) | Every resolution logged with timestamp, actor, outcome |

#### Policies (for DE‑SI‑110)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SI‑EX‑POL‑001 | PO‑SI‑110 False Positive Filtering Policy (Supply) | Self‑resolving within 24 hours → logged, not presented |

#### Policies (for DE‑SI‑111)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SI‑EX‑POL‑002 | PO‑SI‑111 Escalation Policy (Supply) | Critical → Supply Manager; High → assigned planner, escalated if unacknowledged |

#### Policies (for DE‑SI‑112)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SI‑EX‑POL‑003 | PO‑SI‑112 Auto‑Resolution Policy (Supply) | Eligible exceptions auto‑resolved; others require planner |
| P7‑SI‑EX‑POL‑004 | PO‑SI‑113 Resolution SLA Policy (Supply) | Critical: 2 hours, High: 8 hours, Medium: 48 hours |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑SI‑EX‑CMD‑001 | `ClassifySupplyException` | Command handler |
| P7‑SI‑EX‑CMD‑002 | `ResolveSupplyException` | Command handler |
| P7‑SI‑EX‑CMD‑003 | `EscalateSupplyException` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑SI‑EX‑EVT‑001 | `SupplyExceptionDetected` | Event type |
| P7‑SI‑EX‑EVT‑002 | `SupplyExceptionPrioritized` | Event type |
| P7‑SI‑EX‑EVT‑003 | `SupplyExceptionResolved` | Event type |
| P7‑SI‑EX‑EVT‑004 | `SupplyExceptionEscalated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑SI‑EX‑QRY‑001 | `GetActiveSupplyExceptions(filter)` | Query service |
| P7‑SI‑EX‑QRY‑002 | `GetSupplyExceptionHistory(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑SI‑EX‑BEH‑001 | CA‑SI‑011 5.11.13 | Implement: Event‑driven → Classify (DE‑SI‑110) → Prioritize (DE‑SI‑111) → Resolve (DE‑SI‑112) → Track SLAs |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑SI‑EX‑VFY‑001 | BO‑SI‑004 Ensure Supply Continuity | Verify exceptions resolved within SLAs |
| P7‑SI‑EX‑VFY‑002 | BO‑SI‑007 Increase Planning Automation | Verify auto‑resolution rates |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑SI‑EX‑RPT‑001 | RPT‑SI‑015 Exception Summary Report (Supply) | Report generation |
| P7‑SI‑EX‑RPT‑002 | RPT‑SI‑016 SLA Compliance Report (Supply) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑SI‑EX‑DASH‑001 | DASH‑SI‑018 Supply Exception Monitor | UI dashboard |

### 7.5 Capability: CA‑PI‑007 — Sense Promise Risks

**Specification:** Promise Intelligence Specification, Section 5.7

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑PI‑OUT‑001 | OUT‑PI‑060 Promise Risk Alert | Type + publisher |
| P7‑PI‑OUT‑002 | OUT‑PI‑061 Risk Heatmap | Type + publisher |
| P7‑PI‑OUT‑003 | OUT‑PI‑062 Risk Mitigation Recommendation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑PI‑DEC‑001 | DE‑PI‑070 Assess Promise Risk | `assessPromiseRisk(disruption, promises) → Result<Event list, DomainError>` |

#### Rules

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑PI‑RUL‑001 | BR‑PI‑070 Promise‑Supply Linkage Rule | Promise at risk if linked supply source affected by disruption |
| P7‑PI‑RUL‑002 | BR‑PI‑071 Risk Scoring Rule | Risk Score = (1 − BufferTime/LeadTime) × DisruptionSeverity × CustomerTierWeight |
| P7‑PI‑RUL‑003 | BR‑PI‑072 Risk Aggregation Rule | Multiple promises linked to same disrupted source all flagged at same base risk |

#### Policies

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑PI‑POL‑001 | PO‑PI‑070 High‑Risk Escalation Policy | High‑risk promises escalated to Promise Manager and account manager |
| P7‑PI‑POL‑002 | PO‑PI‑071 Auto‑Mitigation Policy | Medium‑risk with confidence >80% → auto‑re‑promised against alternate supply |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑PI‑CMD‑001 | `AssessPromiseRisk` | Command handler |
| P7‑PI‑CMD‑002 | `AcknowledgeRiskAlert` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑PI‑EVT‑001 | `PromiseRiskAssessed` | Event type |
| P7‑PI‑EVT‑002 | `PromiseAtRiskAlert` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑PI‑QRY‑001 | `GetAtRiskPromises(filter)` | Query service |
| P7‑PI‑QRY‑002 | `GetRiskHeatmap(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑PI‑BEH‑001 | CA‑PI‑007 5.7.13 | Implement: Event‑driven → Map disruptions to promises → Assess risk (DE‑PI‑070) → Publish alerts → Update heatmap |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑PI‑VFY‑001 | BO‑PI‑007 Ensure Commitment Feasibility | Verify risks identified before breaches occur |
| P7‑PI‑VFY‑002 | BO‑PI‑002 Maximize Customer Service Reliability | Verify mitigation actions triggered |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑PI‑RPT‑001 | RPT‑PI‑011 Promise Risk Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑PI‑DASH‑001 | DASH‑PI‑010 Promise Risk Monitor | UI dashboard |

### 7.6 Capability: CA‑PI‑009 — Detect Promise Exceptions

**Specification:** Promise Intelligence Specification, Section 5.9

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑PI‑EX‑OUT‑001 | OUT‑PI‑080 Promise Exception Record | Type + publisher |
| P7‑PI‑EX‑OUT‑002 | OUT‑PI‑081 Exception Resolution Recommendation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑PI‑EX‑DEC‑001 | DE‑PI‑090 Classify Promise Exception | `classifyPromiseException(anomaly) → Result<Event list, DomainError>` |
| P7‑PI‑EX‑DEC‑002 | DE‑PI‑091 Prioritize Promise Exception | `prioritizePromiseException(exception) → Result<Event list, DomainError>` |
| P7‑PI‑EX‑DEC‑003 | DE‑PI‑092 Resolve Promise Exception | `resolvePromiseException(exception) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑090)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑PI‑EX‑RUL‑001 | BR‑PI‑090 Exception Classification Rule (Promise) | Actual delivery > promised → Breach; pool exhausted with active demand → Allocation Exhaustion; ATP/CTP error → ATP/CTP Failure; null fields → Data Gap |
| P7‑PI‑EX‑RUL‑002 | BR‑PI‑091 Breach Attribution Rule | Root cause: Supplier Delay, Production Delay, ATP Inaccuracy, Capacity Shortfall, Data Error |

#### Rules (for DE‑PI‑091)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑PI‑EX‑RUL‑003 | BR‑PI‑092 Exception Priority Matrix Rule (Promise) | Matrix maps exception type × customer tier → severity |
| P7‑PI‑EX‑RUL‑004 | BR‑PI‑093 Escalation Rule (Promise) | Critical → escalated immediately to Promise Manager and Account Manager |

#### Rules (for DE‑PI‑092)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑PI‑EX‑RUL‑005 | BR‑PI‑094 Auto‑Resolution Eligibility Rule (Promise) | Auto‑resolve if Breach or ATP Failure, alternate supply available, confidence ≥90%, not Critical |
| P7‑PI‑EX‑RUL‑006 | BR‑PI‑095 Resolution Documentation Rule | Every resolution logged with timestamp, actor, outcome |

#### Policies (for DE‑PI‑090)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑PI‑EX‑POL‑001 | PO‑PI‑090 False Positive Filtering Policy (Promise) | Delivery within 1 day and qty ≥95% → Minor Deviation, logged without alerting |

#### Policies (for DE‑PI‑091)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑PI‑EX‑POL‑002 | PO‑PI‑091 Exception Escalation Policy (Promise) | Critical acknowledged within 30 min; High within 2 hours |

#### Policies (for DE‑PI‑092)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑PI‑EX‑POL‑003 | PO‑PI‑092 Auto‑Resolution Policy (Promise) | Eligible exceptions auto‑resolved; non‑eligible require planner |
| P7‑PI‑EX‑POL‑004 | PO‑PI‑093 Resolution SLA Policy (Promise) | Critical: 30 min, High: 2 hrs, Medium: 8 hrs, Low: 24 hrs |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑PI‑EX‑CMD‑001 | `ClassifyPromiseException` | Command handler |
| P7‑PI‑EX‑CMD‑002 | `ResolvePromiseException` | Command handler |
| P7‑PI‑EX‑CMD‑003 | `EscalatePromiseException` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑PI‑EX‑EVT‑001 | `PromiseExceptionDetected` | Event type |
| P7‑PI‑EX‑EVT‑002 | `PromiseExceptionPrioritized` | Event type |
| P7‑PI‑EX‑EVT‑003 | `PromiseExceptionResolved` | Event type |
| P7‑PI‑EX‑EVT‑004 | `PromiseExceptionEscalated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑PI‑EX‑QRY‑001 | `GetActivePromiseExceptions(filter)` | Query service |
| P7‑PI‑EX‑QRY‑002 | `GetPromiseExceptionHistory(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑PI‑EX‑BEH‑001 | CA‑PI‑009 5.9.13 | Implement: Event‑driven → Classify (DE‑PI‑090) → Prioritize (DE‑PI‑091) → Resolve (DE‑PI‑092) → Track SLAs |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑PI‑EX‑VFY‑001 | BO‑PI‑001 Deliver Trusted Order Commitments | Verify exceptions detected and resolved |
| P7‑PI‑EX‑VFY‑002 | BO‑PI‑002 Maximize Customer Service Reliability | Verify SLA compliance |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑PI‑EX‑RPT‑001 | RPT‑PI‑013 Promise Exception Summary Report | Report generation |
| P7‑PI‑EX‑RPT‑002 | RPT‑PI‑016 SLA Compliance Report (Promise) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑PI‑EX‑DASH‑001 | DASH‑PI‑013 Promise Exception Monitor | UI dashboard |

### 7.7 Capability: CA‑SN‑007 — Sense Scenario Triggers

**Specification:** Scenario Intelligence Specification, Section 5.7

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑SN‑OUT‑001 | OUT‑SN‑060 Scenario Trigger Alert | Type + publisher |
| P7‑SN‑OUT‑002 | OUT‑SN‑061 Trigger‑to‑Scenario Mapping | Type + publisher |
| P7‑SN‑OUT‑003 | OUT‑SN‑062 Trigger Trend Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑SN‑DEC‑001 | DE‑SN‑070 Detect Scenario Trigger | `detectScenarioTrigger(event) → Result<Event list, DomainError>` |
| P7‑SN‑DEC‑002 | DE‑SN‑071 Determine Trigger Scope | `determineTriggerScope(trigger) → Result<Event list, DomainError>` |

#### Rules (for DE‑SN‑070)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SN‑RUL‑001 | BR‑SN‑070 Trigger Detection Rule | Monitored metric breaches threshold sustained for required periods or corroborated |
| P7‑SN‑RUL‑002 | BR‑SN‑071 Trigger‑to‑Scenario Mapping Rule | Each trigger type mapped to pre‑defined scenarios; if none, request new definition |
| P7‑SN‑RUL‑003 | BR‑SN‑072 Duplicate Suppression Rule | Same trigger within 24‑hour window suppressed |

#### Rules (for DE‑SN‑071)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SN‑RUL‑004 | BR‑SN‑073 Scope Determination Rule | Must include directly affected domain, baseline plan, worst‑case scenario |
| P7‑SN‑RUL‑005 | BR‑SN‑074 Scope Adequacy Rule | Warning if scope covers <80% of trigger’s estimated impact area |

#### Policies (for DE‑SN‑070)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SN‑POL‑001 | PO‑SN‑070 Auto‑Trigger Policy | Urgent triggers auto‑initiate mapped scenario simulation; non‑urgent queued |
| P7‑SN‑POL‑002 | PO‑SN‑071 Trigger Notification Policy | Urgent triggers notify Scenario Manager and domain managers within 5 minutes |

#### Policies (for DE‑SN‑071)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SN‑POL‑003 | PO‑SN‑072 Scope Override Policy | Scenario Manager may expand or reduce scope with documented justification |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑SN‑CMD‑001 | `EvaluateTrigger` | Command handler |
| P7‑SN‑CMD‑002 | `SetTriggerScope` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑SN‑EVT‑001 | `ScenarioTriggerDetected` | Event type |
| P7‑SN‑EVT‑002 | `ScenarioTriggerScopeDetermined` | Event type |
| P7‑SN‑EVT‑003 | `ScenarioTriggerActioned` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑SN‑QRY‑001 | `GetActiveTriggers()` | Query service |
| P7‑SN‑QRY‑002 | `GetTriggerHistory(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑SN‑BEH‑001 | CA‑SN‑007 5.7.13 | Implement: Continuous monitoring → Detect trigger (DE‑SN‑070) → Determine scope (DE‑SN‑071) → Initiate simulation → Track response |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑SN‑VFY‑001 | BO‑SN‑007 Accelerate Response to Change | Verify triggers detected and actioned within time targets |
| P7‑SN‑VFY‑002 | BO‑SN‑005 Increase Scenario Planning Automation | Verify auto‑trigger rate |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑SN‑RPT‑001 | RPT‑SN‑009 Trigger Response Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑SN‑DASH‑001 | DASH‑SN‑012 Trigger Monitor | UI dashboard |

### 7.8 Capability: CA‑SN‑009 — Detect Scenario Exceptions

**Specification:** Scenario Intelligence Specification, Section 5.9

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P7‑SN‑EX‑OUT‑001 | OUT‑SN‑080 Scenario Exception Record | Type + publisher |
| P7‑SN‑EX‑OUT‑002 | OUT‑SN‑081 Exception Resolution Recommendation | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P7‑SN‑EX‑DEC‑001 | DE‑SN‑090 Classify Scenario Exception | `classifyScenarioException(anomaly) → Result<Event list, DomainError>` |
| P7‑SN‑EX‑DEC‑002 | DE‑SN‑091 Prioritize Scenario Exception | `prioritizeScenarioException(exception) → Result<Event list, DomainError>` |
| P7‑SN‑EX‑DEC‑003 | DE‑SN‑092 Resolve Scenario Exception | `resolveScenarioException(exception) → Result<Event list, DomainError>` |

#### Rules (for DE‑SN‑090)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SN‑EX‑RUL‑001 | BR‑SN‑090 Exception Classification Rule (Scenario) | Simulation run error/timeout → Simulation Failure; calibration degrades >0.1 → Calibration Drift; recommendation rejection >50% → Recommendation Rejection; trigger not actioned → Trigger Failure; missing inputs → Data Gap |
| P7‑SN‑EX‑RUL‑002 | BR‑SN‑091 False Positive Filter Rule | Known temporary infrastructure issues → Transient, not raised unless recur >3× in 24 hours |

#### Rules (for DE‑SN‑091)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SN‑EX‑RUL‑003 | BR‑SN‑092 Exception Priority Rule (Scenario) | Critical if blocks active strategic recommendation with deadline <3 days; High if blocks any active scenario |
| P7‑SN‑EX‑RUL‑004 | BR‑SN‑093 Escalation Rule (Scenario) | Critical → escalated immediately to Scenario Manager and affected capability owners |

#### Rules (for DE‑SN‑092)

| ID | Rule | Implementation |
|----|------|---------------|
| P7‑SN‑EX‑RUL‑005 | BR‑SN‑094 Auto‑Resolution Rule (Scenario) | Simulation Failures and Data Gaps may be auto‑retried once; Calibration Drift and Recommendation Rejection require manual |
| P7‑SN‑EX‑RUL‑006 | BR‑SN‑095 Resolution Documentation Rule | Every resolution logged with timestamp, actor, outcome |

#### Policies (for DE‑SN‑090)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SN‑EX‑POL‑001 | PO‑SN‑090 Exception Logging Policy | All classified exceptions logged immutably |

#### Policies (for DE‑SN‑091)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SN‑EX‑POL‑002 | PO‑SN‑091 Exception Escalation Policy (Scenario) | Critical acknowledged within 15 minutes, High within 1 hour |

#### Policies (for DE‑SN‑092)

| ID | Policy | Implementation |
|----|--------|---------------|
| P7‑SN‑EX‑POL‑003 | PO‑SN‑092 Resolution SLA Policy (Scenario) | Critical: 1 hour, High: 4 hours, Medium: 24 hours, Low: 5 business days |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P7‑SN‑EX‑CMD‑001 | `ClassifyScenarioException` | Command handler |
| P7‑SN‑EX‑CMD‑002 | `ResolveScenarioException` | Command handler |
| P7‑SN‑EX‑CMD‑003 | `EscalateScenarioException` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P7‑SN‑EX‑EVT‑001 | `ScenarioExceptionDetected` | Event type |
| P7‑SN‑EX‑EVT‑002 | `ScenarioExceptionPrioritized` | Event type |
| P7‑SN‑EX‑EVT‑003 | `ScenarioExceptionResolved` | Event type |
| P7‑SN‑EX‑EVT‑004 | `ScenarioExceptionEscalated` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P7‑SN‑EX‑QRY‑001 | `GetActiveScenarioExceptions(filter)` | Query service |
| P7‑SN‑EX‑QRY‑002 | `GetScenarioExceptionHistory(period)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P7‑SN‑EX‑BEH‑001 | CA‑SN‑009 5.9.13 | Implement: Event‑driven → Classify (DE‑SN‑090) → Prioritize (DE‑SN‑091) → Resolve (DE‑SN‑092) → Track SLAs |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P7‑SN‑EX‑VFY‑001 | BO‑SN‑001 Deliver Trusted Scenario Analysis | Verify exceptions resolved without blocking decisions |
| P7‑SN‑EX‑VFY‑002 | BO‑SN‑007 Accelerate Response to Change | Verify exception resolution timeliness |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P7‑SN‑EX‑RPT‑001 | RPT‑SN‑011 Scenario Exception Summary Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P7‑SN‑EX‑DASH‑001 | DASH‑SN‑015 Scenario Exception Monitor | UI dashboard |

---

We’ll now build the exhaustive Todo Items for **Phase 8 — Collaboration & Execution**, with every capability fully expanded.

---

## Phase 8 — Collaboration & Execution: Todo Items

This phase activates supplier collaboration, customer collaboration, procurement, production scheduling, distribution, and cross‑domain scenario workshops.

### 8.1 Capability: CA‑SI‑005 — Collaborate with Suppliers

**Specification:** Supply Intelligence Specification, Section 5.5

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑SI‑COL‑OUT‑001 | OUT‑SI‑040 Supplier Scorecard | Type + publisher |
| P8‑SI‑COL‑OUT‑002 | OUT‑SI‑041 Supplier Commitment Schedule | Type + publisher |
| P8‑SI‑COL‑OUT‑003 | OUT‑SI‑042 Supplier Risk Report | Type + publisher |
| P8‑SI‑COL‑OUT‑004 | OUT‑SI‑043 Supplier Collaboration Plan | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑SI‑COL‑DEC‑001 | DE‑SI‑050 Evaluate Supplier Commitment | `evaluateSupplierCommitment(commitment) → Result<Event list, DomainError>` |
| P8‑SI‑COL‑DEC‑002 | DE‑SI‑051 Share Demand Forecast with Supplier | `shareDemandForecast(supplier, forecast) → Result<Event list, DomainError>` |
| P8‑SI‑COL‑DEC‑003 | DE‑SI‑052 Assess Supplier Risk | `assessSupplierRisk(supplier) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑050)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑COL‑RUL‑001 | BR‑SI‑050 Commitment Reliability Rule | If 12‑month OTD <80%, commitments auto‑flagged as unreliable |
| P8‑SI‑COL‑RUL‑002 | BR‑SI‑051 Buffer Calculation Rule | For OTD 80–95%, buffer of (1 − OTD) × lead time added to expected delivery |

#### Rules (for DE‑SI‑051)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑COL‑RUL‑003 | BR‑SI‑052 Forecast Sharing Authorization Rule | Share only with valid confidentiality agreement and strategic/preferred status |
| P8‑SI‑COL‑RUL‑004 | BR‑SI‑053 Forecast Aggregation Rule | Non‑strategic suppliers receive product‑family‑level aggregation |

#### Rules (for DE‑SI‑052)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑COL‑RUL‑005 | BR‑SI‑054 Supplier Risk Scoring Rule | Risk Score = weighted sum of performance (30%), financial health (25%), single‑source dependency (30%), geographic risk (15%) |

#### Policies (for DE‑SI‑050)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑COL‑POL‑001 | PO‑SI‑050 Supplier Escalation Policy | OTD <80% for two consecutive months → escalated to Supplier Management |

#### Policies (for DE‑SI‑051)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑COL‑POL‑002 | PO‑SI‑051 Forecast Sharing Policy | Cadence and level reviewed annually and upon contract renewal |

#### Policies (for DE‑SI‑052)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑COL‑POL‑003 | PO‑SI‑052 Risk Mitigation Policy | High‑risk suppliers require documented mitigation plan within 60 days |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑SI‑COL‑CMD‑001 | `EvaluateSupplierCommitments` | Command handler |
| P8‑SI‑COL‑CMD‑002 | `ShareForecastWithSupplier` | Command handler |
| P8‑SI‑COL‑CMD‑003 | `AssessSupplierRisk` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑SI‑COL‑EVT‑001 | `SupplierCommitmentEvaluated` | Event type |
| P8‑SI‑COL‑EVT‑002 | `ForecastSharedWithSupplier` | Event type |
| P8‑SI‑COL‑EVT‑003 | `SupplierRiskAssessed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑SI‑COL‑QRY‑001 | `GetSupplierScorecard(supplierId)` | Query service |
| P8‑SI‑COL‑QRY‑002 | `GetSupplierCommitments(supplierId)` | Query service |
| P8‑SI‑COL‑QRY‑003 | `GetSupplierRisk(supplierId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑SI‑COL‑BEH‑001 | CA‑SI‑005 5.5.13 | Implement: Scheduled → Retrieve performance data → Evaluate commitments (DE‑SI‑050) → Share forecasts (DE‑SI‑051) → Assess risks (DE‑SI‑052) → Update supply picture |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑SI‑COL‑VFY‑001 | BO‑SI‑006 Improve Supplier Collaboration | Verify supplier commitments are evaluated and forecasts shared |
| P8‑SI‑COL‑VFY‑002 | BO‑SI‑004 Ensure Supply Continuity | Verify supplier risks are quantified and mitigated |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑SI‑COL‑RPT‑001 | RPT‑SI‑010 Supplier Scorecard Report | Report generation |
| P8‑SI‑COL‑RPT‑002 | RPT‑SI‑011 Supplier Risk Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑SI‑COL‑DASH‑001 | DASH‑SI‑009 Supplier Collaboration Hub | UI dashboard |
| P8‑SI‑COL‑DASH‑002 | DASH‑SI‑010 Supplier Risk Dashboard | UI dashboard |

### 8.2 Capability: CA‑SI‑006 — Procure Materials

**Specification:** Supply Intelligence Specification, Section 5.6

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑SI‑PRC‑OUT‑001 | OUT‑SI‑050 Purchase Requisition | Type + publisher |
| P8‑SI‑PRC‑OUT‑002 | OUT‑SI‑051 Order Consolidation Plan | Type + publisher |
| P8‑SI‑PRC‑OUT‑003 | OUT‑SI‑052 Procurement Compliance Flags | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑SI‑PRC‑DEC‑001 | DE‑SI‑060 Select Supplier for Order | `selectSupplierForOrder(order) → Result<Event list, DomainError>` |
| P8‑SI‑PRC‑DEC‑002 | DE‑SI‑061 Generate Purchase Requisition | `generatePurchaseRequisition(plan) → Result<Event list, DomainError>` |
| P8‑SI‑PRC‑DEC‑003 | DE‑SI‑062 Release Purchase Order | `releasePurchaseOrder(requisition) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑060)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑PRC‑RUL‑001 | BR‑SI‑060 Primary Supplier Assignment Rule | Assign to primary if OTD ≥95%, risk ≤medium, within allocation quota |
| P8‑SI‑PRC‑RUL‑002 | BR‑SI‑061 Supplier Split Rule | Split proportionally if exceeds any supplier’s capacity or allocation limit |
| P8‑SI‑PRC‑RUL‑003 | BR‑SI‑062 Supplier Exclusion Rule | Exclude suppliers with OTD <80% or risk score “High” from auto‑assignment |

#### Rules (for DE‑SI‑061)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑PRC‑RUL‑004 | BR‑SI‑063 Requisition Release Date Rule | Release date = Required date − Supplier lead time − 2 days internal processing |
| P8‑SI‑PRC‑RUL‑005 | BR‑SI‑064 Order Consolidation Rule | Same supplier with release dates within 3 business days → consolidate |
| P8‑SI‑PRC‑RUL‑006 | BR‑SI‑065 MOQ Compliance Rule | Round up to MOQ; if excess >50%, flag for review |

#### Rules (for DE‑SI‑062)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑PRC‑RUL‑007 | BR‑SI‑066 PO Release Validation Rule | Requisition approved, supplier contract valid, item active |

#### Policies (for DE‑SI‑060)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑PRC‑POL‑001 | PO‑SI‑060 Supplier Assignment Override Policy | Procurement Manager may override with documented justification |
| P8‑SI‑PRC‑POL‑002 | PO‑SI‑061 Allocation Quota Policy | Quotas reviewed quarterly |

#### Policies (for DE‑SI‑061)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑PRC‑POL‑003 | PO‑SI‑062 Requisition Automation Policy | Below value threshold + auto‑selected supplier → auto‑release; above → Procurement approval |
| P8‑SI‑PRC‑POL‑004 | PO‑SI‑063 MOQ Exception Policy | MOQ rounding causing inventory excess beyond threshold → Supply Planner approval |

#### Policies (for DE‑SI‑062)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑PRC‑POL‑005 | PO‑SI‑064 PO Release Approval Policy | POs above value threshold require Procurement Manager approval |
| P8‑SI‑PRC‑POL‑006 | PO‑SI‑065 PO Transmission Policy | Transmitted electronically within 4 hours of release |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑SI‑PRC‑CMD‑001 | `SelectSupplierForOrder` | Command handler |
| P8‑SI‑PRC‑CMD‑002 | `GenerateRequisition` | Command handler |
| P8‑SI‑PRC‑CMD‑003 | `ReleasePurchaseOrder` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑SI‑PRC‑EVT‑001 | `SupplierSelected` | Event type |
| P8‑SI‑PRC‑EVT‑002 | `RequisitionCreated` | Event type |
| P8‑SI‑PRC‑EVT‑003 | `PurchaseOrderReleased` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑SI‑PRC‑QRY‑001 | `GetProcurementRecommendations(filter)` | Query service |
| P8‑SI‑PRC‑QRY‑002 | `GetOpenRequisitions()` | Query service |
| P8‑SI‑PRC‑QRY‑003 | `GetSupplierOrderHistory(supplierId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑SI‑PRC‑BEH‑001 | CA‑SI‑006 5.6.13 | Implement: After supply plan publication → Select supplier (DE‑SI‑060) → Generate requisition (DE‑SI‑061) → Release PO (DE‑SI‑062) → Transmit → Update supply picture |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑SI‑PRC‑VFY‑001 | BO‑SI‑004 Ensure Supply Continuity | Verify procurement recommendations are timely |
| P8‑SI‑PRC‑VFY‑002 | BO‑SI‑005 Minimize Total Delivered Cost | Verify supplier selection minimises cost |
| P8‑SI‑PRC‑VFY‑003 | BO‑SI‑007 Increase Planning Automation | Verify auto‑requisition and auto‑PO rates |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑SI‑PRC‑RPT‑001 | RPT‑SI‑012 Procurement Action Report | Report generation |
| P8‑SI‑PRC‑RPT‑002 | RPT‑SI‑013 Supplier Allocation Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑SI‑PRC‑DASH‑001 | DASH‑SI‑011 Procurement Workbench | UI dashboard |
| P8‑SI‑PRC‑DASH‑002 | DASH‑SI‑012 Spend Dashboard | UI dashboard |

### 8.3 Capability: CA‑SI‑007 — Schedule Production

**Specification:** Supply Intelligence Specification, Section 5.7

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑SI‑SCH‑OUT‑001 | OUT‑SI‑060 Production Schedule | Type + publisher |
| P8‑SI‑SCH‑OUT‑002 | OUT‑SI‑061 Schedule Risk Alerts | Type + publisher |
| P8‑SI‑SCH‑OUT‑003 | OUT‑SI‑062 Material Requirement Dates | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑SI‑SCH‑DEC‑001 | DE‑SI‑070 Sequence Production Orders | `sequenceProductionOrders(orders, resources) → Result<Event list, DomainError>` |
| P8‑SI‑SCH‑DEC‑002 | DE‑SI‑071 Release Production Orders | `releaseProductionOrders(schedule) → Result<Event list, DomainError>` |
| P8‑SI‑SCH‑DEC‑003 | DE‑SI‑072 Publish Production Schedule | `publishProductionSchedule(schedule) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑070)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑SCH‑RUL‑001 | BR‑SI‑070 Sequencing Rule | Critical priority first, then minimise changeover within priority group |
| P8‑SI‑SCH‑RUL‑002 | BR‑SI‑071 Due Date Constraint Rule | No order scheduled to complete after required date unless capacity infeasible; late orders flagged |
| P8‑SI‑SCH‑RUL‑003 | BR‑SI‑072 Minimum Run Length Rule | Runs must meet minimum run quantity/time; below minimum → consolidate or defer |

#### Rules (for DE‑SI‑071)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑SCH‑RUL‑004 | BR‑SI‑073 Material Availability Check Rule | All BOM materials must be available or confirmed for delivery before production start |
| P8‑SI‑SCH‑RUL‑005 | BR‑SI‑074 Order Release Timing Rule | Released at scheduled release date (start date minus staging time) |

#### Rules (for DE‑SI‑072)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑SCH‑RUL‑006 | BR‑SI‑075 Schedule Publication Rule | Must not publish if any critical priority orders unscheduled or late without mitigation |

#### Policies (for DE‑SI‑070)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑SCH‑POL‑001 | PO‑SI‑070 Sequencing Override Policy | Production Scheduler may manually adjust sequence with justification; tracked |
| P8‑SI‑SCH‑POL‑002 | PO‑SI‑071 Minimum Run Exception Policy | Below‑minimum runs allowed with documented Production Manager exception |

#### Policies (for DE‑SI‑071)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑SCH‑POL‑003 | PO‑SI‑072 Partial Release Policy | Partial release allowed only with Production Manager approval and risk assessment |

#### Policies (for DE‑SI‑072)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑SCH‑POL‑004 | PO‑SI‑073 Schedule Publication Cadence Policy | Published daily by 08:00 for the next 2 weeks |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑SI‑SCH‑CMD‑001 | `SequenceOrders` | Command handler |
| P8‑SI‑SCH‑CMD‑002 | `ReleaseProductionOrder` | Command handler |
| P8‑SI‑SCH‑CMD‑003 | `PublishSchedule` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑SI‑SCH‑EVT‑001 | `ProductionScheduleGenerated` | Event type |
| P8‑SI‑SCH‑EVT‑002 | `ProductionOrderReleased` | Event type |
| P8‑SI‑SCH‑EVT‑003 | `ProductionSchedulePublished` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑SI‑SCH‑QRY‑001 | `GetSchedule(resource, period)` | Query service |
| P8‑SI‑SCH‑QRY‑002 | `GetOrderStatus(orderId)` | Query service |
| P8‑SI‑SCH‑QRY‑003 | `GetScheduleRisk()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑SI‑SCH‑BEH‑001 | CA‑SI‑007 5.7.13 | Implement: Daily/after plan update → Retrieve planned production → Sequence (DE‑SI‑070) → Release (DE‑SI‑071) → Publish (DE‑SI‑072) → Transmit to MES |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑SI‑SCH‑VFY‑001 | BO‑SI‑003 Maximize Capacity Utilization | Verify schedules optimise resource utilisation |
| P8‑SI‑SCH‑VFY‑002 | BO‑SI‑004 Ensure Supply Continuity | Verify schedules meet due dates |
| P8‑SI‑SCH‑VFY‑003 | BO‑SI‑007 Increase Planning Automation | Verify auto‑release and auto‑publication rates |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑SI‑SCH‑RPT‑001 | RPT‑SI‑014 Schedule Adherence Report | Report generation |
| P8‑SI‑SCH‑RPT‑002 | RPT‑SI‑015 Changeover Analysis Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑SI‑SCH‑DASH‑001 | DASH‑SI‑013 Production Schedule Board | UI dashboard |
| P8‑SI‑SCH‑DASH‑002 | DASH‑SI‑014 Schedule Risk Dashboard | UI dashboard |

### 8.4 Capability: CA‑SI‑008 — Manage Distribution

**Specification:** Supply Intelligence Specification, Section 5.8

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑SI‑DST‑OUT‑001 | OUT‑SI‑070 Transfer Plan | Type + publisher |
| P8‑SI‑DST‑OUT‑002 | OUT‑SI‑071 Allocation Plan | Type + publisher |
| P8‑SI‑DST‑OUT‑003 | OUT‑SI‑072 Network Balance Projection | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑SI‑DST‑DEC‑001 | DE‑SI‑080 Determine Rebalancing Transfers | `determineRebalancingTransfers(network) → Result<Event list, DomainError>` |
| P8‑SI‑DST‑DEC‑002 | DE‑SI‑081 Allocate Constrained Supply | `allocateConstrainedSupply(supply, demand) → Result<Event list, DomainError>` |
| P8‑SI‑DST‑DEC‑003 | DE‑SI‑082 Release Transfer Orders | `releaseTransferOrders(plan) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑080)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑DST‑RUL‑001 | BR‑SI‑080 Rebalancing Trigger Rule | Transfer recommended when node’s projected inventory drops below safety stock and another node has surplus above max |
| P8‑SI‑DST‑RUL‑002 | BR‑SI‑081 Cost‑Benefit Rule | Transport cost must not exceed external purchase cost + holding cost of surplus |

#### Rules (for DE‑SI‑081)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑DST‑RUL‑003 | BR‑SI‑082 Allocation Rule | Priority customers up to 100% of forecast; remainder distributed proportionally |
| P8‑SI‑DST‑RUL‑004 | BR‑SI‑083 Allocation Documentation Rule | Every constrained allocation must record method, quantities, affected customers |

#### Rules (for DE‑SI‑082)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SI‑DST‑RUL‑005 | BR‑SI‑084 Transfer Release Rule | Transfer only if source inventory confirmed and source not projected below safety stock after transfer |

#### Policies (for DE‑SI‑080)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑DST‑POL‑001 | PO‑SI‑080 Transfer Authorization Policy | Transfers below value threshold → auto‑approved; above → Supply Manager approval |

#### Policies (for DE‑SI‑081)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑DST‑POL‑002 | PO‑SI‑081 Allocation Method Policy | Default allocation method reviewed and approved annually by S&OP Council |

#### Policies (for DE‑SI‑082)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SI‑DST‑POL‑003 | PO‑SI‑082 Transfer Automation Policy | Transfers meeting all rules and below value threshold → auto‑released |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑SI‑DST‑CMD‑001 | `DetermineTransfers` | Command handler |
| P8‑SI‑DST‑CMD‑002 | `AllocateSupply` | Command handler |
| P8‑SI‑DST‑CMD‑003 | `ReleaseTransfer` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑SI‑DST‑EVT‑001 | `TransferRecommended` | Event type |
| P8‑SI‑DST‑EVT‑002 | `SupplyAllocated` | Event type |
| P8‑SI‑DST‑EVT‑003 | `TransferOrderReleased` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑SI‑DST‑QRY‑001 | `GetNetworkBalance()` | Query service |
| P8‑SI‑DST‑QRY‑002 | `GetTransferPlan()` | Query service |
| P8‑SI‑DST‑QRY‑003 | `GetAllocationHistory(product)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑SI‑DST‑BEH‑001 | CA‑SI‑008 5.8.13 | Implement: Daily/after supply plan → Retrieve inventory → Determine transfers (DE‑SI‑080) → Allocate if constrained (DE‑SI‑081) → Release (DE‑SI‑082) → Transmit to WMS |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑SI‑DST‑VFY‑001 | BO‑SI‑002 Optimize Inventory Performance | Verify network is balanced |
| P8‑SI‑DST‑VFY‑002 | BO‑SI‑005 Minimize Total Delivered Cost | Verify transfer costs are minimised |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑SI‑DST‑RPT‑001 | RPT‑SI‑016 Network Balance Report | Report generation |
| P8‑SI‑DST‑RPT‑002 | RPT‑SI‑017 Transfer Cost Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑SI‑DST‑DASH‑001 | DASH‑SI‑015 Distribution Network View | UI dashboard |
| P8‑SI‑DST‑DASH‑002 | DASH‑SI‑016 Allocation Dashboard | UI dashboard |

### 8.5 Capability: CA‑PI‑006 — Collaborate with Customers

**Specification:** Promise Intelligence Specification, Section 5.6

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑PI‑COL‑OUT‑001 | OUT‑PI‑050 Promise Confirmation Message | Type + publisher |
| P8‑PI‑COL‑OUT‑002 | OUT‑PI‑051 Status Update | Type + publisher |
| P8‑PI‑COL‑OUT‑003 | OUT‑PI‑052 Substitution Consent Request | Type + publisher |
| P8‑PI‑COL‑OUT‑004 | OUT‑PI‑053 Communication Log | Type + publisher |
| P8‑PI‑COL‑OUT‑005 | OUT‑PI‑054 Customer Collaboration Score | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑PI‑COL‑DEC‑001 | DE‑PI‑060 Determine Communication Channel and Content | `determineCommunication(event, preferences) → Result<Event list, DomainError>` |
| P8‑PI‑COL‑DEC‑002 | DE‑PI‑061 Obtain Customer Consent | `obtainCustomerConsent(offer, preferences) → Result<Event list, DomainError>` |
| P8‑PI‑COL‑DEC‑003 | DE‑PI‑062 Share Promise Options | `sharePromiseOptions(order, options) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑060)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑PI‑COL‑RUL‑001 | BR‑PI‑060 Channel Selection Rule | Match customer preference; fallback if primary unavailable; Platinum/Gold → personalised |
| P8‑PI‑COL‑RUL‑002 | BR‑PI‑061 Urgency‑Based Communication Rule | Breach → always immediate; routine Bronze → daily digest |
| P8‑PI‑COL‑RUL‑003 | BR‑PI‑062 Template Matching Rule | Template by event type and customer language; must include order reference, date, qty, reason |

#### Rules (for DE‑PI‑061)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑PI‑COL‑RUL‑004 | BR‑PI‑063 Auto‑Consent Rule | Pre‑authorised substitution within parameters → auto‑approved |
| P8‑PI‑COL‑RUL‑005 | BR‑PI‑064 Consent Timeout Rule | No response within window → default action per policy |

#### Rules (for DE‑PI‑062)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑PI‑COL‑RUL‑006 | BR‑PI‑065 Collaborative Promising Eligibility Rule | Tier ≥ Gold, or order value >$50K, or customer enabled collaborative promising |
| P8‑PI‑COL‑RUL‑007 | BR‑PI‑066 Option Filtering Rule | Feasible, within acceptance window, compliant with allocation, profitable |

#### Policies (for DE‑PI‑060)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑PI‑COL‑POL‑001 | PO‑PI‑060 Communication Timing Policy | Confirmations within 15 min; breaches within 30 min; batch summaries by 08:00 |
| P8‑PI‑COL‑POL‑002 | PO‑PI‑061 Manual Escalation Policy | Platinum breaches or any event >$100K → manual review before automated communication |

#### Policies (for DE‑PI‑061)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑PI‑COL‑POL‑003 | PO‑PI‑062 Consent Default Policy | Timeout default → accept best available fulfilment unless customer opted out |

#### Policies (for DE‑PI‑062)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑PI‑COL‑POL‑004 | PO‑PI‑063 Collaborative Promising Policy | Default for Platinum customers; customer selections treated as firm promises once confirmed |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑PI‑COL‑CMD‑001 | `SendCommunication` | Command handler |
| P8‑PI‑COL‑CMD‑002 | `RequestCustomerConsent` | Command handler |
| P8‑PI‑COL‑CMD‑003 | `SharePromiseOptions` | Command handler |
| P8‑PI‑COL‑CMD‑004 | `RecordCustomerResponse` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑PI‑COL‑EVT‑001 | `CommunicationSent` | Event type |
| P8‑PI‑COL‑EVT‑002 | `CustomerResponseReceived` | Event type |
| P8‑PI‑COL‑EVT‑003 | `ConsentObtained` | Event type |
| P8‑PI‑COL‑EVT‑004 | `PromiseOptionsShared` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑PI‑COL‑QRY‑001 | `GetCommunicationHistory(customerId, period)` | Query service |
| P8‑PI‑COL‑QRY‑002 | `GetPendingConsents()` | Query service |
| P8‑PI‑COL‑QRY‑003 | `GetCustomerCollaborationScore(customerId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑PI‑COL‑BEH‑001 | CA‑PI‑006 5.6.13 | Implement: Event‑driven → Retrieve preferences → Determine channel (DE‑PI‑060) → Obtain consent if needed (DE‑PI‑061) → Share options if eligible (DE‑PI‑062) → Transmit → Log |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑PI‑COL‑VFY‑001 | BO‑PI‑005 Improve Order Visibility and Transparency | Verify communications are timely and accurate |
| P8‑PI‑COL‑VFY‑002 | BO‑PI‑002 Maximize Customer Service Reliability | Verify consent and collaborative promising work |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑PI‑COL‑RPT‑001 | RPT‑PI‑010 Communication Effectiveness Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑PI‑COL‑DASH‑001 | DASH‑PI‑008 Customer Communication Monitor | UI dashboard |
| P8‑PI‑COL‑DASH‑002 | DASH‑PI‑009 Collaborative Promising Workbench | UI dashboard |

### 8.6 Capability: CA‑SN‑006 — Collaborate on Scenarios

**Specification:** Scenario Intelligence Specification, Section 5.6

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P8‑SN‑COL‑OUT‑001 | OUT‑SN‑050 Workshop Record | Type + publisher |
| P8‑SN‑COL‑OUT‑002 | OUT‑SN‑051 Stakeholder Feedback Log | Type + publisher |
| P8‑SN‑COL‑OUT‑003 | OUT‑SN‑052 Consensus Statement | Type + publisher |
| P8‑SN‑COL‑OUT‑004 | OUT‑SN‑053 Workshop Action Items | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P8‑SN‑COL‑DEC‑001 | DE‑SN‑060 Convene Scenario Workshop | `conveneWorkshop(context) → Result<Event list, DomainError>` |
| P8‑SN‑COL‑DEC‑002 | DE‑SN‑061 Facilitate Scenario Review | `facilitateReview(workshop) → Result<Event list, DomainError>` |
| P8‑SN‑COL‑DEC‑003 | DE‑SN‑062 Reach Consensus | `reachConsensus(workshop) → Result<Event list, DomainError>` |

#### Rules (for DE‑SN‑060)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SN‑COL‑RUL‑001 | BR‑SN‑060 Workshop Requirement Rule | Full workshop required if impact >$5M or affects >2 business units |
| P8‑SN‑COL‑RUL‑002 | BR‑SN‑061 Quorum Rule | At least one representative from each affected business unit |

#### Rules (for DE‑SN‑061)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SN‑COL‑RUL‑003 | BR‑SN‑062 Challenge Documentation Rule | Every assumption challenged must be documented with challenger’s rationale and alternative |
| P8‑SN‑COL‑RUL‑004 | BR‑SN‑063 Dissent Recording Rule | Dissenting views recorded with name, position, rationale; visible to final decision‑maker |

#### Rules (for DE‑SN‑062)

| ID | Rule | Implementation |
|----|------|---------------|
| P8‑SN‑COL‑RUL‑005 | BR‑SN‑064 Consensus Threshold Rule | Full (100%), Majority (>67%), No Consensus (≤67%); no consensus → escalated |
| P8‑SN‑COL‑RUL‑006 | BR‑SN‑065 Consensus Documentation Rule | Must list all endorsing stakeholders, dissenting views, final agreed position |

#### Policies (for DE‑SN‑060)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SN‑COL‑POL‑001 | PO‑SN‑060 Workshop Scheduling Policy | Strategic workshops scheduled ≥10 business days ahead; materials distributed ≥5 days prior |

#### Policies (for DE‑SN‑061)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SN‑COL‑POL‑002 | PO‑SN‑061 Challenge Resolution Policy | Assumption challenged by domain authority → new scenario variant created and simulated |

#### Policies (for DE‑SN‑062)

| ID | Policy | Implementation |
|----|--------|---------------|
| P8‑SN‑COL‑POL‑003 | PO‑SN‑062 Escalation Policy (Consensus) | No consensus after one workshop + one follow‑up → escalated to Executive Committee |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P8‑SN‑COL‑CMD‑001 | `ConveneWorkshop` | Command handler |
| P8‑SN‑COL‑CMD‑002 | `RecordChallenge` | Command handler |
| P8‑SN‑COL‑CMD‑003 | `RecordConsensus` | Command handler |
| P8‑SN‑COL‑CMD‑004 | `EscalateConsensus` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P8‑SN‑COL‑EVT‑001 | `WorkshopConvened` | Event type |
| P8‑SN‑COL‑EVT‑002 | `ScenarioAssumptionChallenged` | Event type |
| P8‑SN‑COL‑EVT‑003 | `ConsensusReached` | Event type |
| P8‑SN‑COL‑EVT‑004 | `ConsensusFailed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P8‑SN‑COL‑QRY‑001 | `GetWorkshop(workshopId)` | Query service |
| P8‑SN‑COL‑QRY‑002 | `GetConsensusStatus(scenarioId)` | Query service |
| P8‑SN‑COL‑QRY‑003 | `GetStakeholderFeedback(scenarioId)` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P8‑SN‑COL‑BEH‑001 | CA‑SN‑006 5.6.13 | Implement: Scheduled/comparison complete → Convene (DE‑SN‑060) → Facilitate review (DE‑SN‑061) iteratively → Reach consensus (DE‑SN‑062) → Publish record |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P8‑SN‑COL‑VFY‑001 | BO‑SN‑006 Enable Collaborative What‑If Exploration | Verify workshops are recorded and consensus tracked |
| P8‑SN‑COL‑VFY‑002 | BO‑SN‑004 Optimize Strategic Decision Making | Verify stakeholder input is captured and used |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P8‑SN‑COL‑RPT‑001 | RPT‑SN‑008 Workshop Summary Report | Report generation |
| P8‑SN‑COL‑RPT‑002 | RPT‑SN‑008 Stakeholder Participation Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P8‑SN‑COL‑DASH‑001 | DASH‑SN‑010 Collaboration Hub | UI dashboard |
| P8‑SN‑COL‑DASH‑002 | DASH‑SN‑011 Stakeholder Engagement Dashboard | UI dashboard |

---

We’ll now build the exhaustive Todo Items for **Phase 9 — Domain Learning & Full Quality**, with every capability fully expanded.

---

## Phase 9 — Domain Learning & Full Quality: Todo Items

This phase activates all domain‑level Learn capabilities and upgrades the simplified capabilities to full scope.

### 9.1 Capability: CA‑DI‑010 — Learn From Demand

**Specification:** Demand Intelligence Specification, Section 5.10

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑OUT‑001 | OUT‑DI‑050 Improvement Recommendation | Type + publisher |
| P9‑DI‑LRN‑OUT‑002 | OUT‑DI‑051 Learning Loop Closure Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P9‑DI‑LRN‑DEC‑001 | DE‑DI‑100 Recommend Model Improvement | `recommendDemandModelImprovement(metrics) → Result<Event list, DomainError>` |
| P9‑DI‑LRN‑DEC‑002 | DE‑DI‑101 Recommend Threshold Adjustment | `recommendDemandThresholdAdjustment(metrics) → Result<Event list, DomainError>` |
| P9‑DI‑LRN‑DEC‑003 | DE‑DI‑102 Propose New Pattern or Segment | `proposeNewDemandPattern(data) → Result<Event list, DomainError>` |
| P9‑DI‑LRN‑DEC‑004 | DE‑DI‑103 Close the Learning Loop | `closeDemandLearningLoop(improvementId) → Result<Event list, DomainError>` |

#### Rules (for DE‑DI‑100)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑DI‑LRN‑RUL‑001 | BR‑DI‑100 Model Performance Degradation Rule | WAPE increase >2pp over rolling 6‑month period → retrain/tune recommendation |
| P9‑DI‑LRN‑RUL‑002 | BR‑DI‑101 Data Drift Detection Rule | Population Stability Index >0.25 → recommend retraining before tuning |
| P9‑DI‑LRN‑RUL‑003 | BR‑DI‑102 Model Switch Rule | Champion WAPE exceeds benchmark naive for 3 consecutive months → recommend switch |
| P9‑DI‑LRN‑RUL‑004 | BR‑DI‑109 New Model Validation Rule | Discovered model must achieve WAPE ≥5% lower than champion on holdout and be reproducible |

#### Rules (for DE‑DI‑101)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑DI‑LRN‑RUL‑005 | BR‑DI‑103 Threshold Optimization Rule | Quarterly review with cost‑benefit framework; recommend if net benefit exceeds minimum |
| P9‑DI‑LRN‑RUL‑006 | BR‑DI‑104 Threshold Stability Rule | No threshold change more than once per quarter without documented structural shift |

#### Rules (for DE‑DI‑102)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑DI‑LRN‑RUL‑007 | BR‑DI‑105 New Pattern Validation Rule | ≥10 independent product‑location series, cluster separation index >0.7, identifiable business cause |
| P9‑DI‑LRN‑RUL‑008 | BR‑DI‑106 Segment Proposal Rule | Within‑segment variance reduction ≥20% compared to existing segmentation |

#### Rules (for DE‑DI‑103)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑DI‑LRN‑RUL‑009 | BR‑DI‑107 Improvement Verification Rule | Minimum 4‑week observation for model changes, 8‑weeks for threshold changes |
| P9‑DI‑LRN‑RUL‑010 | BR‑DI‑108 Automatic Rollback Rule | Statistically significant degradation (p≤0.05) in WAPE or service level → auto‑rollback |

#### Policies (for DE‑DI‑100)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑POL‑001 | PO‑DI‑100 Automatic Retraining Policy | Data drift + retraining recommended → auto‑execute next forecast cycle; all others require Demand Manager approval |
| P9‑DI‑LRN‑POL‑002 | PO‑DI‑101 New Model Proposal Policy | Discovered model architecture must be reviewed by Data Science before registering as challenger |

#### Policies (for DE‑DI‑101)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑POL‑003 | PO‑DI‑102 Threshold Adjustment Approval Policy | Adjustments affecting Critical alerting or automation require Demand Manager + Supply Chain Director approval |

#### Policies (for DE‑DI‑102)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑POL‑004 | PO‑DI‑103 Catalogue Evolution Policy | New patterns, exception types, segments reviewed by Demand Planning Council before catalogue addition |

#### Policies (for DE‑DI‑103)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑POL‑005 | PO‑DI‑104 Learning Loop Closure Policy | Every improvement documented with before‑after evaluation; reported monthly |
| P9‑DI‑LRN‑POL‑006 | PO‑DI‑105 Rollback Authorization Policy | Rollbacks due to degradation executed immediately by Demand Manager |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P9‑DI‑LRN‑CMD‑001 | `AnalyzePerformanceTrends` | Command handler |
| P9‑DI‑LRN‑CMD‑002 | `ProposeImprovement` | Command handler |
| P9‑DI‑LRN‑CMD‑003 | `EvaluateImprovement` | Command handler |
| P9‑DI‑LRN‑CMD‑004 | `RollbackImprovement` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P9‑DI‑LRN‑EVT‑001 | `DemandImprovementRecommended` | Event type |
| P9‑DI‑LRN‑EVT‑002 | `DemandLearningLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P9‑DI‑LRN‑QRY‑001 | `GetImprovementHistory(period)` | Query service |
| P9‑DI‑LRN‑QRY‑002 | `GetActiveImprovements()` | Query service |
| P9‑DI‑LRN‑QRY‑003 | `GetLearningEffectivenessIndex()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P9‑DI‑LRN‑BEH‑001 | CA‑DI‑010 5.10.10 | Implement: Scheduled/weekly → Retrieve quality data → Recommend model improvement (DE‑DI‑100) → Recommend threshold adjustment (DE‑DI‑101) → Propose new pattern (DE‑DI‑102) → Close loop (DE‑DI‑103) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P9‑DI‑LRN‑VFY‑001 | BO‑DI‑006 Continuously Improve Enterprise Intelligence | Verify learning loop closure rate and improvement adoption |
| P9‑DI‑LRN‑VFY‑002 | BO‑DI‑001 Deliver Trusted Demand Understanding | Verify forecast accuracy improves over time |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P9‑DI‑LRN‑RPT‑001 | RPT‑DI‑019 Continuous Improvement Report | Report generation |
| P9‑DI‑LRN‑RPT‑002 | RPT‑DI‑020 Model Health Report | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P9‑DI‑LRN‑DASH‑001 | DASH‑DI‑018 Learning Dashboard | UI dashboard |
| P9‑DI‑LRN‑DASH‑002 | DASH‑DI‑019 Model Performance Trend Dashboard | UI dashboard |

### 9.2 Capability: CA‑SI‑013 — Learn From Supply

**Specification:** Supply Intelligence Specification, Section 5.13

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑OUT‑001 | OUT‑SI‑100 Improvement Recommendation | Type + publisher |
| P9‑SI‑LRN‑OUT‑002 | OUT‑SI‑101 Learning Loop Closure Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P9‑SI‑LRN‑DEC‑001 | DE‑SI‑130 Recommend Supply Model Improvement | `recommendSupplyModelImprovement(metrics) → Result<Event list, DomainError>` |
| P9‑SI‑LRN‑DEC‑002 | DE‑SI‑131 Recommend Supply Threshold Adjustment | `recommendSupplyThresholdAdjustment(metrics) → Result<Event list, DomainError>` |
| P9‑SI‑LRN‑DEC‑003 | DE‑SI‑132 Propose New Supply Pattern or Exception Type | `proposeNewSupplyPattern(data) → Result<Event list, DomainError>` |
| P9‑SI‑LRN‑DEC‑004 | DE‑SI‑133 Close the Supply Learning Loop | `closeSupplyLearningLoop(improvementId) → Result<Event list, DomainError>` |

#### Rules (for DE‑SI‑130)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SI‑LRN‑RUL‑001 | BR‑SI‑120 Model Performance Degradation Rule (Supply) | Plan adherence or inventory health degradation triggers review |
| P9‑SI‑LRN‑RUL‑002 | BR‑SI‑121 Data Drift Detection Rule (Supply) | Feature distribution shift → recommend retraining |
| P9‑SI‑LRN‑RUL‑003 | BR‑SI‑122 Model Switch Rule (Supply) | Systematic underperformance → recommend model type switch |

#### Rules (for DE‑SI‑131)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SI‑LRN‑RUL‑004 | BR‑SI‑123 Threshold Optimization Rule (Supply) | Quarterly review with cost‑benefit framework |
| P9‑SI‑LRN‑RUL‑005 | BR‑SI‑124 Threshold Stability Rule (Supply) | Max one change per quarter without documented event |

#### Rules (for DE‑SI‑132)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SI‑LRN‑RUL‑006 | BR‑SI‑125 New Supply Pattern Validation Rule | ≥10 independent series, statistical distinctiveness, identifiable cause |
| P9‑SI‑LRN‑RUL‑007 | BR‑SI‑126 Supply Segment Proposal Rule | Within‑segment variance reduction ≥20% |

#### Rules (for DE‑SI‑133)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SI‑LRN‑RUL‑008 | BR‑SI‑127 Improvement Verification Rule (Supply) | Minimum observation window per improvement type |
| P9‑SI‑LRN‑RUL‑009 | BR‑SI‑128 Automatic Rollback Rule (Supply) | Significant degradation → auto‑rollback |

#### Policies (for DE‑SI‑130)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑POL‑001 | PO‑SI‑110 Automatic Retraining Policy (Supply) | Data drift → auto‑retrain; other improvements require Supply Manager approval |
| P9‑SI‑LRN‑POL‑002 | PO‑SI‑111 New Model Proposal Policy (Supply) | Data Science review required |

#### Policies (for DE‑SI‑131)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑POL‑003 | PO‑SI‑112 Threshold Adjustment Approval Policy (Supply) | Critical thresholds require Supply Chain Director approval |

#### Policies (for DE‑SI‑132)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑POL‑004 | PO‑SI‑113 Catalogue Evolution Policy (Supply) | Supply Planning Council review required |

#### Policies (for DE‑SI‑133)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑POL‑005 | PO‑SI‑114 Learning Loop Closure Policy (Supply) | Documented before‑after evaluation; reported monthly |
| P9‑SI‑LRN‑POL‑006 | PO‑SI‑115 Rollback Authorization Policy (Supply) | Supply Manager may execute immediately |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P9‑SI‑LRN‑CMD‑001 | `AnalyzeSupplyPerformance` | Command handler |
| P9‑SI‑LRN‑CMD‑002 | `ProposeSupplyImprovement` | Command handler |
| P9‑SI‑LRN‑CMD‑003 | `EvaluateSupplyImprovement` | Command handler |
| P9‑SI‑LRN‑CMD‑004 | `RollbackSupplyImprovement` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P9‑SI‑LRN‑EVT‑001 | `SupplyImprovementRecommended` | Event type |
| P9‑SI‑LRN‑EVT‑002 | `SupplyLearningLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P9‑SI‑LRN‑QRY‑001 | `GetSupplyImprovementHistory(period)` | Query service |
| P9‑SI‑LRN‑QRY‑002 | `GetActiveSupplyImprovements()` | Query service |
| P9‑SI‑LRN‑QRY‑003 | `GetSupplyLearningEffectiveness()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P9‑SI‑LRN‑BEH‑001 | CA‑SI‑013 5.13.13 | Implement: Scheduled → Analyze quality data → Recommend model (DE‑SI‑130) → Recommend threshold (DE‑SI‑131) → Propose new pattern (DE‑SI‑132) → Close loop (DE‑SI‑133) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P9‑SI‑LRN‑VFY‑001 | BO‑SI‑008 Continuously Improve Supply Intelligence | Verify learning loop closure rate and improvement adoption |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P9‑SI‑LRN‑RPT‑001 | RPT‑SI‑020 Continuous Improvement Report (Supply) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P9‑SI‑LRN‑DASH‑001 | DASH‑SI‑020 Learning Dashboard (Supply) | UI dashboard |

### 9.3 Capability: CA‑PI‑011 — Learn From Promise

**Specification:** Promise Intelligence Specification, Section 5.11

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P9‑PI‑LRN‑OUT‑001 | OUT‑PI‑100 Improvement Recommendation | Type + publisher |
| P9‑PI‑LRN‑OUT‑002 | OUT‑PI‑101 Learning Loop Closure Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P9‑PI‑LRN‑DEC‑001 | DE‑PI‑110 Recommend Promise Parameter Adjustment | `recommendPromiseParameterAdjustment(metrics) → Result<Event list, DomainError>` |
| P9‑PI‑LRN‑DEC‑002 | DE‑PI‑111 Propose New Substitution Rule | `proposeNewSubstitutionRule(data) → Result<Event list, DomainError>` |
| P9‑PI‑LRN‑DEC‑003 | DE‑PI‑112 Close the Promise Learning Loop | `closePromiseLearningLoop(improvementId) → Result<Event list, DomainError>` |

#### Rules (for DE‑PI‑110)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑PI‑LRN‑RUL‑001 | BR‑PI‑110 Parameter Adjustment Rule | Expected net benefit (automation gain − breach risk) exceeds threshold |
| P9‑PI‑LRN‑RUL‑002 | BR‑PI‑111 Parameter Stability Rule | Max one change per quarter without significant event |

#### Rules (for DE‑PI‑111)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑PI‑LRN‑RUL‑003 | BR‑PI‑112 New Substitution Validation Rule | ≥85% customer acceptance in controlled trial; no increase in breach rate |

#### Rules (for DE‑PI‑112)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑PI‑LRN‑RUL‑004 | BR‑PI‑113 Improvement Verification Rule (Promise) | Minimum 4‑week observation window |
| P9‑PI‑LRN‑RUL‑005 | BR‑PI‑114 Auto‑Rollback Rule (Promise) | Significant degradation in promise adherence → auto‑rollback |

#### Policies (for DE‑PI‑110)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑PI‑LRN‑POL‑001 | PO‑PI‑110 Parameter Adjustment Approval Policy | Confidence threshold and allocation rule changes require Promise Manager approval |

#### Policies (for DE‑PI‑111)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑PI‑LRN‑POL‑002 | PO‑PI‑111 Substitution Rule Evolution Policy | New rules require Product Management + Supply Chain approval |

#### Policies (for DE‑PI‑112)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑PI‑LRN‑POL‑003 | PO‑PI‑112 Learning Loop Closure Policy (Promise) | Documented before‑after; reported monthly |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P9‑PI‑LRN‑CMD‑001 | `AnalyzePromisePerformance` | Command handler |
| P9‑PI‑LRN‑CMD‑002 | `ProposePromiseImprovement` | Command handler |
| P9‑PI‑LRN‑CMD‑003 | `EvaluatePromiseImprovement` | Command handler |
| P9‑PI‑LRN‑CMD‑004 | `RollbackPromiseImprovement` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P9‑PI‑LRN‑EVT‑001 | `PromiseImprovementRecommended` | Event type |
| P9‑PI‑LRN‑EVT‑002 | `PromiseLearningLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P9‑PI‑LRN‑QRY‑001 | `GetPromiseImprovementHistory(period)` | Query service |
| P9‑PI‑LRN‑QRY‑002 | `GetActivePromiseImprovements()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P9‑PI‑LRN‑BEH‑001 | CA‑PI‑011 5.11.13 | Implement: Scheduled → Analyze quality data → Recommend parameter adjustment (DE‑PI‑110) → Propose substitution rule (DE‑PI‑111) → Close loop (DE‑PI‑112) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P9‑PI‑LRN‑VFY‑001 | BO‑PI‑008 Continuously Improve Promise Intelligence | Verify learning loop closure rate |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P9‑PI‑LRN‑RPT‑001 | RPT‑PI‑015 Continuous Improvement Report (Promise) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P9‑PI‑LRN‑DASH‑001 | DASH‑PI‑015 Learning Dashboard (Promise) | UI dashboard |

### 9.4 Capability: CA‑SN‑011 — Learn From Scenarios

**Specification:** Scenario Intelligence Specification, Section 5.11

#### Enterprise Outputs

| ID | Output | Implementation |
|----|--------|---------------|
| P9‑SN‑LRN‑OUT‑001 | OUT‑SN‑100 Improvement Recommendation | Type + publisher |
| P9‑SN‑LRN‑OUT‑002 | OUT‑SN‑101 Learning Loop Closure Report | Type + publisher |

#### Decisions

| ID | Decision | Implementation |
|----|----------|---------------|
| P9‑SN‑LRN‑DEC‑001 | DE‑SN‑110 Recommend Simulation Improvement | `recommendSimulationImprovement(metrics) → Result<Event list, DomainError>` |
| P9‑SN‑LRN‑DEC‑002 | DE‑SN‑111 Recommend Threshold Adjustment (Scenario) | `recommendScenarioThresholdAdjustment(metrics) → Result<Event list, DomainError>` |
| P9‑SN‑LRN‑DEC‑003 | DE‑SN‑112 Close the Scenario Learning Loop | `closeScenarioLearningLoop(improvementId) → Result<Event list, DomainError>` |

#### Rules (for DE‑SN‑110)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SN‑LRN‑RUL‑001 | BR‑SN‑110 Recalibration Trigger Rule | Calibration score <0.8 or degrades >0.1 in one quarter → recalibration |
| P9‑SN‑LRN‑RUL‑002 | BR‑SN‑111 Performance Degradation Rule | Failure rate >5% or run time increase >20% → performance review |

#### Rules (for DE‑SN‑111)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SN‑LRN‑RUL‑003 | BR‑SN‑112 Threshold Optimization Rule (Scenario) | Quarterly review; adjust if net benefit exceeds minimum |
| P9‑SN‑LRN‑RUL‑004 | BR‑SN‑113 Threshold Stability Rule (Scenario) | Max one change per quarter without significant event |

#### Rules (for DE‑SN‑112)

| ID | Rule | Implementation |
|----|------|---------------|
| P9‑SN‑LRN‑RUL‑005 | BR‑SN‑114 Improvement Verification Rule (Scenario) | Minimum one quarter for calibration, one month for thresholds |
| P9‑SN‑LRN‑RUL‑006 | BR‑SN‑115 Auto‑Rollback Rule (Scenario) | Significant degradation → auto‑rollback |

#### Policies (for DE‑SN‑110)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SN‑LRN‑POL‑001 | PO‑SN‑110 Recalibration Approval Policy | Data Science team approval; scheduled during maintenance window |

#### Policies (for DE‑SN‑111)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SN‑LRN‑POL‑002 | PO‑SN‑111 Threshold Adjustment Approval Policy (Scenario) | Trigger thresholds → Scenario Manager; risk appetite → Risk Committee |

#### Policies (for DE‑SN‑112)

| ID | Policy | Implementation |
|----|--------|---------------|
| P9‑SN‑LRN‑POL‑003 | PO‑SN‑112 Learning Loop Closure Policy (Scenario) | Documented before‑after; reported quarterly at S&OP review |

#### Commands

| ID | Command | Implementation |
|----|---------|---------------|
| P9‑SN‑LRN‑CMD‑001 | `AnalyzeScenarioPerformance` | Command handler |
| P9‑SN‑LRN‑CMD‑002 | `ProposeScenarioImprovement` | Command handler |
| P9‑SN‑LRN‑CMD‑003 | `EvaluateScenarioImprovement` | Command handler |
| P9‑SN‑LRN‑CMD‑004 | `RollbackScenarioImprovement` | Command handler |

#### Events

| ID | Event | Implementation |
|----|-------|---------------|
| P9‑SN‑LRN‑EVT‑001 | `ScenarioImprovementRecommended` | Event type |
| P9‑SN‑LRN‑EVT‑002 | `ScenarioLearningLoopClosed` | Event type |

#### Queries

| ID | Query | Implementation |
|----|-------|---------------|
| P9‑SN‑LRN‑QRY‑001 | `GetScenarioImprovementHistory(period)` | Query service |
| P9‑SN‑LRN‑QRY‑002 | `GetActiveScenarioImprovements()` | Query service |

#### Functional Behaviour

| ID | Step | Implementation |
|----|------|---------------|
| P9‑SN‑LRN‑BEH‑001 | CA‑SN‑011 5.11.13 | Implement: Scheduled → Analyze quality data → Recommend simulation improvement (DE‑SN‑110) → Recommend threshold (DE‑SN‑111) → Close loop (DE‑SN‑112) |

#### Business Objectives Verification

| ID | Objective | Verification |
|----|-----------|-------------|
| P9‑SN‑LRN‑VFY‑001 | BO‑SN‑008 Continuously Improve Scenario Intelligence | Verify learning loop closure rate |

#### Reports

| ID | Report | Implementation |
|----|--------|---------------|
| P9‑SN‑LRN‑RPT‑001 | RPT‑SN‑012 Continuous Improvement Report (Scenario) | Report generation |

#### Dashboards

| ID | Dashboard | Implementation |
|----|-----------|---------------|
| P9‑SN‑LRN‑DASH‑001 | DASH‑SN‑016 Learning Dashboard (Scenario) | UI dashboard |

### 9.5 Full Scope Upgrade: CA‑SI‑010 — Evaluate Supply Quality

**From:** MVP scope (PI‑SI‑002–006, 010–013, 015)  
**To:** Full scope (all Business Outcome Measures + all Intelligence Measures PI‑SI‑100–199)

#### Additional Business Outcome Measures

| ID | PI | Implementation + Test |
|----|-----|----------------------|
| P9‑SI‑QFY‑PI‑001 | PI‑SI‑007 Supplier On‑Time Delivery | `computeSupplierOTD(deliveries) → decimal` + test against spec worked example |
| P9‑SI‑QFY‑PI‑002 | PI‑SI‑008 Total Supply Chain Cost | `computeTotalSupplyChainCost(costs) → decimal` + test against spec worked example |
| P9‑SI‑QFY‑PI‑003 | PI‑SI‑009 Perfect Order Fulfillment (Supply) | `computePerfectOrderFulfillmentSupply(orders) → decimal` + test against spec worked example |
| P9‑SI‑QFY‑PI‑004 | PI‑SI‑014 Planning Cycle Time (Supply) | `computePlanningCycleTimeSupply(cycles) → TimeSpan` + test against spec worked example |

#### Intelligence Measures (PI‑SI‑100–199)

| ID | PI | Implementation |
|----|-----|---------------|
| P9‑SI‑QFY‑IM‑001 | PI‑SI‑101 Supply Understanding Index | Composite measure — implement computation |
| P9‑SI‑QFY‑IM‑002 | PI‑SI‑102 Inventory Optimization Effectiveness | Implement computation |
| P9‑SI‑QFY‑IM‑003 | PI‑SI‑103 Capacity Forecast Accuracy | Implement computation |
| P9‑SI‑QFY‑IM‑004 | PI‑SI‑104 Supplier Risk Score | Implement computation |
| P9‑SI‑QFY‑IM‑005 | PI‑SI‑105 Recommendation Quality Index (Supply) | Implement computation |
| P9‑SI‑QFY‑IM‑006 | PI‑SI‑106 Decision Confidence Index (Supply) | Implement computation |
| P9‑SI‑QFY‑IM‑007 | PI‑SI‑107 Explainability Score (Supply) | Implement computation |
| P9‑SI‑QFY‑IM‑008 | PI‑SI‑108 Learning Effectiveness Index (Supply) | Implement computation |
| P9‑SI‑QFY‑IM‑009 | PI‑SI‑109 Supplier Collaboration Index | Implement computation |
| P9‑SI‑QFY‑IM‑010 | PI‑SI‑110 Supply Exception Detection Accuracy | Implement computation |
| P9‑SI‑QFY‑IM‑011 | PI‑SI‑111 Supply Plan Confidence Index | Implement computation |
| P9‑SI‑QFY‑IM‑012 | PI‑SI‑112 Supply Intelligence Coverage Index | Implement computation |

#### Updates to Quality Report

| ID | Update | Implementation |
|----|--------|---------------|
| P9‑SI‑QFY‑UPD‑001 | Extend `SupplyQualityReport` to include all new PI‑SI‑xxx metrics | Update report type and generation |
| P9‑SI‑QFY‑UPD‑002 | Update `DASH‑SI‑019 Supply Performance Dashboard` with new metric panels | UI dashboard update |
| P9‑SI‑QFY‑UPD‑003 | Add `Supplier Performance Scorecard` report | New report generation |

### 9.6 Full Scope Upgrade: CA‑SN‑004 — Assess Risks

**From:** MVP scope (deterministic Probability × Impact, basic stress tests)  
**To:** Full scope (probabilistic risk assessment, full stress test suite, automated mitigation optimisation)

#### Probabilistic Risk Assessment

| ID | Item | Implementation |
|----|------|---------------|
| P9‑SN‑RSK‑UP‑001 | Implement Monte Carlo‑based risk scoring | Use probabilistic simulation outputs (CA‑SN‑002) for probability estimation |
| P9‑SN‑RSK‑UP‑002 | Implement PI‑SN‑103 Risk Prediction Accuracy | Compare predicted vs actual risk outcomes |
| P9‑SN‑RSK‑UP‑003 | Implement PI‑SN‑111 Probability Calibration Score | Brier score and calibration curve analysis |

#### Full Stress Test Suite

| ID | Item | Implementation |
|----|------|---------------|
| P9‑SN‑RSK‑UP‑004 | Implement PI‑SN‑011 Resilience Index | Performance loss + recovery speed computation |
| P9‑SN‑RSK‑UP‑005 | Implement multi‑scenario stress testing | Run multiple stress scenarios simultaneously |
| P9‑SN‑RSK‑UP‑006 | Implement recovery time projection | Model recovery trajectories post‑disruption |

#### Automated Mitigation Portfolio Optimisation

| ID | Item | Implementation |
|----|------|---------------|
| P9‑SN‑RSK‑UP‑007 | Implement automated mitigation portfolio builder | Propose optimal mitigation mix based on ROI |
| P9‑SN‑RSK‑UP‑008 | Implement mitigation dependency resolution | Handle dependencies between mitigations |
| P9‑SN‑RSK‑UP‑009 | Implement mitigation effectiveness tracking | Before‑after risk score comparison |

#### Updates

| ID | Update | Implementation |
|----|--------|---------------|
| P9‑SN‑RSK‑UP‑010 | Extend `RiskAssessmentReport` with probabilistic metrics | Update report type |
| P9‑SN‑RSK‑UP‑011 | Update `DASH‑SN‑007 Risk Heatmap Dashboard` with probabilistic risk scores | UI dashboard update |
| P9‑SN‑RSK‑UP‑012 | Update `DASH‑SN‑008 Stress Test Monitor` with recovery projections | UI dashboard update |

---

We’ll now build the exhaustive Todo Items for **Phase 10 — Advanced AI & Simulation**, with every workstream fully expanded to match the density of earlier phases.

---

## Phase 10 — Advanced AI & Simulation: Todo Items

This phase introduces probabilistic simulation, solver‑based optimisation, digital twin capabilities, and completes all Intelligence and Operational Measures across all domains.

### 10.1 Probabilistic Simulation

**Source:** Architecture Blueprint §9.5; Scenario Intelligence Specification §5.2; Promise Intelligence Specification §5.2

#### 10.1.1 Monte Carlo Simulation Engine

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑SIM‑001 | Implement `MonteCarloEngine` in `Medhavi.PlanningEngine` — accepts probability distributions for input variables, runs N iterations, collects output KPI distributions | Arch. Blueprint §9.5 |
| P10‑SIM‑002 | Implement distribution types: `NormalDistribution`, `LogNormalDistribution`, `TriangularDistribution`, `UniformDistribution`, `BetaDistribution`, `EmpiricalDistribution` | Arch. Blueprint §9.5 |
| P10‑SIM‑003 | Implement distribution sampling functions for each type | Arch. Blueprint §9.5 |
| P10‑SIM‑004 | Implement configurable iteration control: fixed count, convergence‑based (standard error of mean < threshold), time‑bounded | Scenario Spec §5.2 |
| P10‑SIM‑005 | Implement random seed management for reproducibility (per BR‑SN‑024) | Scenario Spec §5.2 |
| P10‑SIM‑006 | Implement parallel iteration execution using `Task` parallelism with configurable degree of parallelism | Arch. Blueprint §9.5 |

#### 10.1.2 Probabilistic Output Types

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑SIM‑010 | Implement `ProbabilisticOutcome` type: `Mean`, `Median`, `StdDev`, `P10`, `P25`, `P75`, `P90`, `P95`, `P99`, `ValueAtRisk`, `ConditionalValueAtRisk` | Scenario Spec §5.2, SE‑SN‑024 |
| P10‑SIM‑011 | Implement `DistributionSummary` type with histogram bins and cumulative distribution function | Scenario Spec §5.2 |
| P10‑SIM‑012 | Implement convergence monitoring: track standard error of mean across iterations; signal convergence when SE < threshold | Scenario Spec §5.2 |
| P10‑SIM‑013 | Implement `ProbabilisticResult` envelope containing raw iteration results + summary statistics + convergence metadata | Scenario Spec §5.2 |

#### 10.1.3 Integration with Simulate Scenarios (CA‑SN‑002)

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑SIM‑020 | Extend `DE‑SN‑020 Select Simulation Method` to route to probabilistic engine when method = `Probabilistic` | Scenario Spec §5.2 |
| P10‑SIM‑021 | Extend `DE‑SN‑021 Execute Simulation Run` to accept probability distributions as input assumptions | Scenario Spec §5.2 |
| P10‑SIM‑022 | Extend `DE‑SN‑022 Generate Probabilistic Summary` to produce full `ProbabilisticOutcome` with all statistics | Scenario Spec §5.2 |
| P10‑SIM‑023 | Update `OUT‑SN‑011 Probabilistic Distribution` to carry `ProbabilisticOutcome` type | Scenario Spec §5.2 |
| P10‑SIM‑024 | Update `DASH‑SN‑004 Scenario Outcome Explorer` to render probability distribution charts (histograms, CDF, fan charts) | Scenario Spec §5.2 |

#### 10.1.4 Probabilistic Promising (CA‑PI‑002)

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑SIM‑030 | Extend `DE‑PI‑020 Evaluate ATP` to accept probability distributions for lead times and supply variability | Promise Spec §5.2 |
| P10‑SIM‑031 | Implement `ProbabilisticPromiseResult` type: promise date with confidence intervals (P50, P85, P95) | Promise Spec §5.2 |
| P10‑SIM‑032 | Extend `OUT‑PI‑010 Promise Decision` to carry probabilistic promise information | Promise Spec §5.2 |
| P10‑SIM‑033 | Update `BR‑PI‑022 ATP Confidence Rule` to use probabilistic confidence derived from simulation | Promise Spec §5.2 |
| P10‑SIM‑034 | Update `DASH‑PI‑004 ATP/CTP Performance Dashboard` to show confidence interval visualisations | Promise Spec §5.2 |

#### 10.1.5 Integration with Assess Risks — Full Scope (CA‑SN‑004)

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑SIM‑040 | Extend `DE‑SN‑040 Compute Risk Scores` to use probabilistic simulation outputs for probability estimation | Scenario Spec §5.4 |
| P10‑SIM‑041 | Implement PI‑SN‑103 Risk Prediction Accuracy — compare predicted risk distributions to actual outcomes | Scenario Spec §3.3 |
| P10‑SIM‑042 | Implement PI‑SN‑111 Probability Calibration Score — Brier score and calibration curve analysis | Scenario Spec §3.3 |

### 10.2 Advanced Optimisation Solvers

**Source:** Architecture Blueprint §9.5; Supply Intelligence Specification §5.2 (Plan Supply); Supply Intelligence Specification §5.7 (Schedule Production)

#### 10.2.1 Solver Abstraction Layer

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑OPT‑001 | Implement `SolverPort` interface: `Solve(OptimizationModel, TimeSpan option) → Task<Result<SolverSolution, SolverError>>` | Arch. Blueprint §9.5 |
| P10‑OPT‑002 | Implement `OptimizationModel` type: `Variables list`, `Constraints list`, `Objective` | Arch. Blueprint §9.5 |
| P10‑OPT‑003 | Implement `Variable` type: name, lower bound, upper bound, type (continuous, integer, binary) | Arch. Blueprint §9.5 |
| P10‑OPT‑004 | Implement `Constraint` type: linear expression, bound, type (≤, ≥, =) | Arch. Blueprint §9.5 |
| P10‑OPT‑005 | Implement `Objective` type: expression, direction (minimise, maximise) | Arch. Blueprint §9.5 |
| P10‑OPT‑006 | Implement `SolverSolution` type: variable assignments, objective value, optimality gap, solve time, status (Optimal, Feasible, Infeasible, Timeout) | Arch. Blueprint §9.5 |
| P10‑OPT‑007 | Implement `SolverConfig`: time limit, optimality gap tolerance, thread count, solver‑specific parameters | Arch. Blueprint §9.5 |

#### 10.2.2 MILP Solver Integration (Plan Supply — Optimization Mode)

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑OPT‑010 | Implement `OrToolsMILPSolver` implementing `SolverPort` using Google OR‑Tools MIP solver | Arch. Blueprint §9.5 |
| P10‑OPT‑011 | Implement `SupplyPlanModelBuilder` — translates MRP data (demand, inventory, capacity, BOM, costs) into `OptimizationModel` | Arch. Blueprint §9.5 |
| P10‑OPT‑012 | Implement objective function builder: weighted sum of total cost, lateness penalty, capacity utilisation, inventory holding cost | Supply Spec §5.2 |
| P10‑OPT‑013 | Implement constraint builder: material balance, capacity limits, BOM dependencies, lead time offsets, minimum order quantities | Supply Spec §5.2 |
| P10‑OPT‑014 | Implement warm start: seed solver with heuristic plan as initial solution | Arch. Blueprint §9.5 |
| P10‑OPT‑015 | Implement solution translator: `SolverSolution` → `SupplyPlan` with all planned orders, inventory projections, capacity load | Arch. Blueprint §9.5 |
| P10‑OPT‑016 | Integrate with `DE‑SI‑021 Generate Supply Plan` — when mode = `Optimization`, route through MILP solver | Supply Spec §5.2 |
| P10‑OPT‑017 | Implement fallback to heuristic on solver timeout or infeasibility | Arch. Blueprint §9.5 |

#### 10.2.3 CP‑SAT Solver Integration (Schedule Production — Optimization Mode)

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑OPT‑020 | Implement `OrToolsCPSATSolver` implementing `SolverPort` using Google OR‑Tools CP‑SAT solver | Arch. Blueprint §9.5 |
| P10‑OPT‑021 | Implement `ProductionScheduleModelBuilder` — translates production orders, resources, changeover matrices into `OptimizationModel` | Arch. Blueprint §9.5 |
| P10‑OPT‑022 | Implement sequencing constraints: no overlap on same resource, precedence constraints, changeover time between different products | Supply Spec §5.7 |
| P10‑OPT‑023 | Implement objective function: minimise total changeover time, minimise lateness, maximise throughput | Supply Spec §5.7 |
| P10‑OPT‑024 | Implement solution translator: `SolverSolution` → `ProductionSchedule` with Gantt chart data | Arch. Blueprint §9.5 |
| P10‑OPT‑025 | Integrate with `DE‑SI‑070 Sequence Production Orders` — when optimisation enabled, route through CP‑SAT solver | Supply Spec §5.7 |

### 10.3 Digital Twin

**Source:** Architecture Blueprint §9.5; Scenario Intelligence Specification §5.2 (extended concept)

#### 10.3.1 High‑Fidelity Shop Floor Simulator

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑DT‑001 | Implement `DiscreteEventSimulator` — event‑driven simulation of production operations (machines, labour, material flow) | Arch. Blueprint §9.5 |
| P10‑DT‑002 | Implement simulation entities: `Machine` (with failure/repair models), `Operator`, `WorkOrder`, `MaterialLot`, `Buffer` | Arch. Blueprint §9.5 |
| P10‑DT‑003 | Implement stochastic event models: machine breakdown (MTBF/MTTR distributions), quality inspection (yield distributions), operator availability | Arch. Blueprint §9.5 |
| P10‑DT‑004 | Implement simulation output: `SimulationTrace` (event log), `SimulationKPIs` (throughput, utilisation, WIP, cycle time, on‑time delivery) | Arch. Blueprint §9.5 |
| P10‑DT‑005 | Implement `SimulationScenario` — configuration for simulation runs (duration, warm‑up period, replication count) | Arch. Blueprint §9.5 |

#### 10.3.2 Telemetry‑Driven Calibration

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑DT‑010 | Implement `CalibrationDataCollector` — collects MES execution actuals (order completion times, downtime events, yield results) | Arch. Blueprint §9.5 |
| P10‑DT‑011 | Implement `DistributionFitter` — fits probability distributions to actual data (uses statistical methods: MLE, method of moments) | Arch. Blueprint §9.5 |
| P10‑DT‑012 | Implement `SimulationCalibrator` — updates simulator parameters (MTBF, MTTR, processing times) from fitted distributions | Arch. Blueprint §9.5 |
| P10‑DT‑013 | Implement calibration validation: compare simulated KPIs to actual KPIs; flag significant deviations | Arch. Blueprint §9.5 |

#### 10.3.3 Sim‑to‑Real Domain Randomisation

| ID | Todo Item | Source |
|----|-----------|--------|
| P10‑DT‑020 | Implement `DomainRandomizer` — creates randomised simulation environments for RL agent training (vary demand patterns, breakdown rates, processing times within realistic bounds) | Arch. Blueprint §9.5 |
| P10‑DT‑021 | Implement `RandomizationConfig` — parameter ranges for domain randomisation | Arch. Blueprint §9.5 |
| P10‑DT‑022 | Implement `RLEnvironment` interface — standard RL environment contract (state, action, reward, next‑state) for integration with RL agent training pipelines | Arch. Blueprint §9.5 |

### 10.4 Full Intelligence & Operational Measures

**Source:** Each Intelligence Specification, Chapter 3 (Enterprise Measurement Model)

#### 10.4.1 Demand Intelligence Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑DI‑001 | PI‑DI‑101 Demand Understanding Index | Composite measure — implement computation |
| P10‑PI‑DI‑002 | PI‑DI‑102 Demand Signal Quality Index | Implement computation |
| P10‑PI‑DI‑003 | PI‑DI‑103 Forecast Confidence Index | Implement computation |
| P10‑PI‑DI‑004 | PI‑DI‑104 Decision Confidence Index | Implement computation |
| P10‑PI‑DI‑005 | PI‑DI‑105 Recommendation Quality Index | Implement computation |
| P10‑PI‑DI‑006 | PI‑DI‑106 Recommendation Acceptance Rate | Implement computation |
| P10‑PI‑DI‑007 | PI‑DI‑107 Explainability Score | Implement computation |
| P10‑PI‑DI‑008 | PI‑DI‑108 Learning Effectiveness Index | Implement computation |
| P10‑PI‑DI‑009 | PI‑DI‑109 Demand Intelligence Coverage Index | Implement computation |
| P10‑PI‑DI‑010 | PI‑DI‑110 Exception Detection Accuracy | Implement computation |
| P10‑PI‑DI‑011 | PI‑DI‑111 Exception Prediction Accuracy | Implement computation |
| P10‑PI‑DI‑012 | PI‑DI‑112 Demand Segmentation Quality | Implement computation |
| P10‑PI‑DI‑013 | PI‑DI‑113 Demand Classification Accuracy | Implement computation |
| P10‑PI‑DI‑014 | PI‑DI‑114 Demand Prioritization Effectiveness | Implement computation |
| P10‑PI‑DI‑015 | PI‑DI‑115 AI Recommendation Utilization | Implement computation |

#### 10.4.2 Demand Operational Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑DI‑OP‑001 | PI‑DI‑201 Planning Cycle Time | Implement computation |
| P10‑PI‑DI‑OP‑002 | PI‑DI‑202 Forecast Generation Time | Implement computation |
| P10‑PI‑DI‑OP‑003 | PI‑DI‑203 Demand Refresh Latency | Implement computation |
| P10‑PI‑DI‑OP‑004 | PI‑DI‑204 Data Freshness | Implement computation |
| P10‑PI‑DI‑OP‑005 | PI‑DI‑205 Data Completeness | Implement computation |
| P10‑PI‑DI‑OP‑006 | PI‑DI‑206 Data Quality Score | Implement computation |
| P10‑PI‑DI‑OP‑007 | PI‑DI‑207 Integration Success Rate | Implement computation |
| P10‑PI‑DI‑OP‑008 | PI‑DI‑208 Event Processing Latency | Implement computation |
| P10‑PI‑DI‑OP‑009 | PI‑DI‑209 Projection Processing Latency | Implement computation |
| P10‑PI‑DI‑OP‑010 | PI‑DI‑210 API Response Time | Implement computation |
| P10‑PI‑DI‑OP‑011 | PI‑DI‑211 Dashboard Refresh Time | Implement computation |
| P10‑PI‑DI‑OP‑012 | PI‑DI‑212 Report Generation Time | Implement computation |
| P10‑PI‑DI‑OP‑013 | PI‑DI‑213 System Availability | Implement computation |
| P10‑PI‑DI‑OP‑014 | PI‑DI‑214 Planning Throughput | Implement computation |
| P10‑PI‑DI‑OP‑015 | PI‑DI‑215 Exception Processing Time | Implement computation |

#### 10.4.3 Supply Intelligence Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑SI‑001 | PI‑SI‑101 Supply Understanding Index | Implement computation |
| P10‑PI‑SI‑002 | PI‑SI‑102 Inventory Optimization Effectiveness | Implement computation |
| P10‑PI‑SI‑003 | PI‑SI‑103 Capacity Forecast Accuracy | Implement computation |
| P10‑PI‑SI‑004 | PI‑SI‑104 Supplier Risk Score | Implement computation |
| P10‑PI‑SI‑005 | PI‑SI‑105 Recommendation Quality Index (Supply) | Implement computation |
| P10‑PI‑SI‑006 | PI‑SI‑106 Decision Confidence Index (Supply) | Implement computation |
| P10‑PI‑SI‑007 | PI‑SI‑107 Explainability Score (Supply) | Implement computation |
| P10‑PI‑SI‑008 | PI‑SI‑108 Learning Effectiveness Index (Supply) | Implement computation |
| P10‑PI‑SI‑009 | PI‑SI‑109 Supplier Collaboration Index | Implement computation |
| P10‑PI‑SI‑010 | PI‑SI‑110 Supply Exception Detection Accuracy | Implement computation |
| P10‑PI‑SI‑011 | PI‑SI‑111 Supply Plan Confidence Index | Implement computation |
| P10‑PI‑SI‑012 | PI‑SI‑112 Supply Intelligence Coverage Index | Implement computation |

#### 10.4.4 Supply Operational Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑SI‑OP‑001 | PI‑SI‑201 Supply Planning Cycle Time | Implement computation |
| P10‑PI‑SI‑OP‑002 | PI‑SI‑202 Plan Generation Time | Implement computation |
| P10‑PI‑SI‑OP‑003 | PI‑SI‑203 Inventory Refresh Latency | Implement computation |
| P10‑PI‑SI‑OP‑004 | PI‑SI‑204 Supply Data Freshness | Implement computation |
| P10‑PI‑SI‑OP‑005 | PI‑SI‑205 Supply Data Completeness | Implement computation |
| P10‑PI‑SI‑OP‑006 | PI‑SI‑206 Supply Data Quality Score | Implement computation |
| P10‑PI‑SI‑OP‑007 | PI‑SI‑207 Integration Success Rate (Supply) | Implement computation |
| P10‑PI‑SI‑OP‑008 | PI‑SI‑208 Event Processing Latency (Supply) | Implement computation |
| P10‑PI‑SI‑OP‑009 | PI‑SI‑209 API Response Time (Supply) | Implement computation |
| P10‑PI‑SI‑OP‑010 | PI‑SI‑210 System Availability (Supply) | Implement computation |
| P10‑PI‑SI‑OP‑011 | PI‑SI‑211 Planning Throughput (Supply) | Implement computation |
| P10‑PI‑SI‑OP‑012 | PI‑SI‑212 Exception Processing Time (Supply) | Implement computation |

#### 10.4.5 Promise Intelligence Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑PI‑001 | PI‑PI‑101 Promise Understanding Index | Implement computation |
| P10‑PI‑PI‑002 | PI‑PI‑102 ATP Accuracy | Implement computation |
| P10‑PI‑PI‑003 | PI‑PI‑103 CTP Accuracy | Implement computation |
| P10‑PI‑PI‑004 | PI‑PI‑104 Decision Confidence Index (Promise) | Implement computation |
| P10‑PI‑PI‑005 | PI‑PI‑105 Recommendation Quality Index (Promise) | Implement computation |
| P10‑PI‑PI‑006 | PI‑PI‑106 Explainability Score (Promise) | Implement computation |
| P10‑PI‑PI‑007 | PI‑PI‑107 Order Prioritization Effectiveness | Implement computation |
| P10‑PI‑PI‑008 | PI‑PI‑108 Allocation Optimization Effectiveness | Implement computation |
| P10‑PI‑PI‑009 | PI‑PI‑109 Commitment Risk Score | Implement computation |
| P10‑PI‑PI‑010 | PI‑PI‑110 Customer Collaboration Index | Implement computation |

#### 10.4.6 Promise Operational Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑PI‑OP‑001 | PI‑PI‑201 Promise Response Time | Implement computation |
| P10‑PI‑PI‑OP‑002 | PI‑PI‑202 ATP Check Latency | Implement computation |
| P10‑PI‑PI‑OP‑003 | PI‑PI‑203 CTP Check Latency | Implement computation |
| P10‑PI‑PI‑OP‑004 | PI‑PI‑204 Allocation Refresh Time | Implement computation |
| P10‑PI‑PI‑OP‑005 | PI‑PI‑205 System Availability (Promise) | Implement computation |
| P10‑PI‑PI‑OP‑006 | PI‑PI‑206 Event Processing Latency (Promise) | Implement computation |

#### 10.4.7 Scenario Intelligence Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑SN‑001 | PI‑SN‑101 Scenario Understanding Index | Implement computation |
| P10‑PI‑SN‑002 | PI‑SN‑102 Simulation Accuracy | Implement computation |
| P10‑PI‑SN‑003 | PI‑SN‑103 Risk Prediction Accuracy | Implement computation |
| P10‑PI‑SN‑004 | PI‑SN‑104 Decision Confidence Index (Scenario) | Implement computation |
| P10‑PI‑SN‑005 | PI‑SN‑105 Recommendation Quality Index (Scenario) | Implement computation |
| P10‑PI‑SN‑006 | PI‑SN‑106 Explainability Score (Scenario) | Implement computation |
| P10‑PI‑SN‑007 | PI‑SN‑107 Sensitivity Analysis Coverage | Implement computation |
| P10‑PI‑SN‑008 | PI‑SN‑108 What‑If Completeness | Implement computation |
| P10‑PI‑SN‑009 | PI‑SN‑109 Scenario Diversity Index | Implement computation |
| P10‑PI‑SN‑010 | PI‑SN‑110 Learning Effectiveness Index (Scenario) | Implement computation |
| P10‑PI‑SN‑011 | PI‑SN‑111 Probability Calibration Score | Implement computation |

#### 10.4.8 Scenario Operational Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑SN‑OP‑001 | PI‑SN‑201 Simulation Execution Time | Implement computation |
| P10‑PI‑SN‑OP‑002 | PI‑SN‑202 Scenario Data Refresh Latency | Implement computation |
| P10‑PI‑SN‑OP‑003 | PI‑SN‑203 API Response Time (Scenario) | Implement computation |
| P10‑PI‑SN‑OP‑004 | PI‑SN‑204 System Availability (Scenario) | Implement computation |
| P10‑PI‑SN‑OP‑005 | PI‑SN‑205 Event Processing Latency (Scenario) | Implement computation |

#### 10.4.9 Knowledge Intelligence Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑KN‑001 | PI‑KN‑101 Enterprise Understanding Index (Cross‑Domain) | Implement computation |
| P10‑PI‑KN‑002 | PI‑KN‑102 Pattern Significance Score | Implement computation |
| P10‑PI‑KN‑003 | PI‑KN‑103 Knowledge Graph Coverage | Implement computation |
| P10‑PI‑KN‑004 | PI‑KN‑104 Recommendation Quality Index (Knowledge) | Implement computation |
| P10‑PI‑KN‑005 | PI‑KN‑105 Explainability Score (Knowledge) | Implement computation |
| P10‑PI‑KN‑006 | PI‑KN‑106 Learning Effectiveness Index (Enterprise) | Implement computation |
| P10‑PI‑KN‑007 | PI‑KN‑107 Cross‑Domain Correlation Strength | Implement computation |
| P10‑PI‑KN‑008 | PI‑KN‑108 Causal Chain Confidence | Implement computation |
| P10‑PI‑KN‑009 | PI‑KN‑109 Improvement Adoption Rate | Implement computation |
| P10‑PI‑KN‑010 | PI‑KN‑110 Knowledge Freshness Index | Implement computation |

#### 10.4.10 Knowledge Operational Measures

| ID | PI | Implementation |
|----|-----|---------------|
| P10‑PI‑KN‑OP‑001 | PI‑KN‑201 Knowledge Query Response Time | Implement computation |
| P10‑PI‑KN‑OP‑002 | PI‑KN‑202 Pattern Discovery Computation Time | Implement computation |
| P10‑PI‑KN‑OP‑003 | PI‑KN‑203 Knowledge Graph Update Latency | Implement computation |
| P10‑PI‑KN‑OP‑004 | PI‑KN‑204 System Availability (Knowledge) | Implement computation |
| P10‑PI‑KN‑OP‑005 | PI‑KN‑205 Event Processing Latency (Knowledge) | Implement computation |

#### 10.4.11 Measurement Integration Updates

| ID | Update | Implementation |
|----|--------|---------------|
| P10‑PI‑UPD‑001 | Update all Evaluate Quality capabilities to include Intelligence Measures in quality reports | Each domain, Chapter 5 |
| P10‑PI‑UPD‑002 | Update all `RPT‑xxx` quality reports to include Intelligence Measures sections | Each domain, Chapter 7 |
| P10‑PI‑UPD‑003 | Update all `DASH‑xxx` quality dashboards with Intelligence Measure panels | Each domain, Chapter 7 |
| P10‑PI‑UPD‑004 | Implement operational measure collection infrastructure (timers, counters) in `SharedKernel.Telemetry` | Arch. Blueprint §12.3 |
| P10‑PI‑UPD‑005 | Export all measures to Prometheus/Grafana via `/metrics` endpoint | Arch. Blueprint §12.8 |

---

We’ll now build the exhaustive Todo Items for **Phase 11 — Autonomous & Agentic APS**, the final phase that delivers the fully autonomous, production‑hardened Medhavi APS.

---

## Phase 11 — Autonomous & Agentic APS: Todo Items

This phase activates autonomous AI operations, continuous event‑driven planning, GNN‑based supply chain intelligence, full AI Autonomy Level 3, and production hardening (PostgreSQL migration, external event bus, security, multi‑tenancy, disaster recovery, and performance optimisation).

### 11.1 Agentic Exception Resolution

**Source:** Architecture Blueprint §10.4–10.8; Knowledge Intelligence Specification §5.8 (Serve Knowledge to AI Agents)

#### 11.1.1 Autonomous Resolution Engine

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AG‑001 | Implement `AutonomousResolutionEngine` in Knowledge Intelligence — subscribes to exception events from all domains, analyses root causes via CA‑KN‑003, generates resolution actions within autonomy contracts | Arch. Blueprint §10.4 |
| P11‑AG‑002 | Implement resolution action taxonomy: `RePromise`, `ExpediteOrder`, `SwitchSupplier`, `AdjustSafetyStock`, `RerunOptimization`, `EscalateToHuman` | Arch. Blueprint §10.4 |
| P11‑AG‑003 | Implement confidence‑based action selection — only execute actions with confidence ≥ threshold defined in autonomy contract | Arch. Blueprint §10.4 |
| P11‑AG‑004 | Implement resolution verification — track whether autonomous resolution improved the target metric; feed back into enterprise memory | Arch. Blueprint §10.9 |
| P11‑AG‑005 | Implement `ResolutionAuditLog` — every autonomous action recorded with full decision trace, autonomy contract reference, and outcome | Arch. Blueprint §10.11 |

#### 11.1.2 Integration with Exception Detection Capabilities

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AG‑010 | Wire autonomous resolution to CA‑DI‑008 (Detect Demand Exceptions) — auto‑resolve Outlier, Data Gap, Level Shift exceptions within contract bounds | Demand Spec §5.8 |
| P11‑AG‑011 | Wire autonomous resolution to CA‑SI‑011 (Detect Supply Exceptions) — auto‑resolve Shortage, Late Delivery, Capacity Violation exceptions within contract bounds | Supply Spec §5.11 |
| P11‑AG‑012 | Wire autonomous resolution to CA‑PI‑009 (Detect Promise Exceptions) — auto‑resolve Breach, ATP Failure exceptions within contract bounds | Promise Spec §5.9 |
| P11‑AG‑013 | Wire autonomous resolution to CA‑SN‑009 (Detect Scenario Exceptions) — auto‑resolve Simulation Failure, Data Gap exceptions within contract bounds | Scenario Spec §5.9 |

#### 11.1.3 Human‑AI Handoff

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AG‑020 | Implement `EscalationWorkflow` — when autonomous resolution is not permitted (confidence below threshold, action outside contract, value exceeds limit), escalate to human with full context | Arch. Blueprint §10.4 |
| P11‑AG‑021 | Implement `EscalationNotification` — structured notification with exception details, proposed resolution, reason for escalation, decision deadline | Arch. Blueprint §10.4 |
| P11‑AG‑022 | Implement `HumanDecisionCapture` — record human decision on escalated items; feed into enterprise memory for AI learning | Arch. Blueprint §10.9 |

### 11.2 Continuous Planning

**Source:** Architecture Blueprint §9.2 (Planning Modes), §9.8 (Time‑Window Projections); Supply Intelligence Specification §5.2 (Plan Supply)

#### 11.2.1 Event‑Driven Micro‑Replanning Engine

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑CP‑001 | Implement `ContinuousPlanningEngine` — subscribes to disruption events (SupplyDisruptionDetected, DemandChangeDetected, PromiseBreached) and triggers IncrementalRepair planning mode | Arch. Blueprint §9.2 |
| P11‑CP‑002 | Implement `ImpactAssessor` — on disruption event, identifies affected orders, supply commitments, and promises via PlanIndex and pegging graph | Arch. Blueprint §9.2 |
| P11‑CP‑003 | Implement `DeltaPlanner` — generates minimal plan changes to restore feasibility; preserves locked/firmed orders | Arch. Blueprint §9.2 |
| P11‑CP‑004 | Implement `ChurnMinimizer` — selects repair strategy with smallest deviation from published plan (minimise quantity changes, date shifts, order count changes) | Arch. Blueprint §9.2 |
| P11‑CP‑005 | Implement `ContinuousPlanningMode` in PlanningEngine — always‑on mode that processes disruption events as they arrive, with configurable batching window | Arch. Blueprint §9.2 |

#### 11.2.2 Live Plan Graph

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑CP‑010 | Implement `LivePlanGraph` — on startup, loads most recent committed PlanningGraph from version store, then subscribes to domain events to apply deltas | Arch. Blueprint §9.2 |
| P11‑CP‑011 | Implement `GraphDelta` application — incremental updates to the live graph without full replan (add/remove nodes, update quantities, change edges) | Arch. Blueprint §9.2 |
| P11‑CP‑012 | Implement `FastFeasibilityCheck` — on live graph, quickly evaluate whether a proposed change would violate any hard constraint without running full MRP | Arch. Blueprint §9.2 |
| P11‑CP‑013 | Implement `GraphVersioning` — every delta creates a new graph version; previous versions retained for rollback and audit | Arch. Blueprint §9.2 |

#### 11.2.3 Near‑Real‑Time Impact Analysis

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑CP‑020 | Implement `ImpactPropagator` — given a change (e.g., supplier delay of 3 days on PO‑890), propagates impact through BOM dependencies, capacity constraints, and promise commitments | Arch. Blueprint §9.2 |
| P11‑CP‑021 | Implement `ImpactReport` — structured output: affected orders (with severity), estimated financial impact, earliest resolution time | Arch. Blueprint §9.2 |
| P11‑CP‑022 | Implement `ProactiveAlert` — before a disruption causes a breach, alert planners with estimated impact and recommended actions | Arch. Blueprint §9.2 |

### 11.3 Supply Chain Graph GNN

**Source:** Architecture Blueprint §9.5 (PlanningGraph); Knowledge Intelligence Specification §5.2 (Discover Cross‑Domain Patterns)

#### 11.3.1 Supply Chain Multi‑Graph Representation

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑GNN‑001 | Implement `SupplyChainGraph` — directed multi‑graph with node types: `SupplierNode`, `PlantNode`, `DistributionCenterNode`, `CustomerNode`, `ProductNode`, `ResourceNode` | Arch. Blueprint §9.5 |
| P11‑GNN‑002 | Implement edge types: `SuppliesEdge` (supplier→plant, with lead time, reliability), `ProducesEdge` (plant→product, with BOM, capacity), `ShipsToEdge` (plant→DC, with transport lane, cost), `DemandsEdge` (customer→product, with forecast, priority) | Arch. Blueprint §9.5 |
| P11‑GNN‑003 | Implement graph construction from enterprise data — build graph from MasterData (suppliers, plants, BOMs), Supply plans, and Demand forecasts | Arch. Blueprint §9.5 |
| P11‑GNN‑004 | Implement graph indexing — fast lookup by node type, by product, by location, by supplier; shortest‑path queries for impact propagation | Arch. Blueprint §9.5 |

#### 11.3.2 GNN Model Integration

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑GNN‑010 | Implement `GNNInferenceService` — Python‑based service (or ONNX runtime in F#) that loads trained GNN model and runs inference on the supply chain graph | Arch. Blueprint §9.5 |
| P11‑GNN‑011 | Implement `RiskPropagationModel` — GNN predicts how a disruption at one node propagates to downstream nodes (probability of impact, expected delay, severity) | Arch. Blueprint §9.5 |
| P11‑GNN‑012 | Implement `CascadePredictionModel` — GNN identifies nodes most vulnerable to cascading failures given a disruption scenario | Arch. Blueprint §9.5 |
| P11‑GNN‑013 | Implement `NodeEmbeddingModel` — GNN produces vector embeddings for each node, enabling similarity search (“which suppliers are most similar to Supplier X in terms of risk profile?”) | Arch. Blueprint §9.5 |

#### 11.3.3 Integration with Knowledge Intelligence

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑GNN‑020 | Integrate GNN risk propagation results with CA‑KN‑002 (Discover Cross‑Domain Patterns) — feed cascade predictions as candidate patterns | Knowledge Spec §5.2 |
| P11‑GNN‑021 | Integrate GNN node embeddings with CA‑KN‑008 (Serve Knowledge to AI Agents) — AI agents can query “similar nodes” for context | Knowledge Spec §5.8 |
| P11‑GNN‑022 | Integrate GNN predictions with CA‑KN‑007 (Maintain Enterprise Memory) — record predictions and actual outcomes for model improvement | Knowledge Spec §5.7 |

### 11.4 Full AI Autonomy Level 3

**Source:** Architecture Blueprint §10.4 (Autonomy Contracts), §10.5 (PolicyGate), §10.8 (AI Agent Categories)

#### 11.4.1 Autonomous Autonomy Level

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AUT‑001 | Extend `DecisionCore.Autonomy.AutonomyLevel` to support full `Autonomous` level with configurable action whitelist | Arch. Blueprint §10.4 |
| P11‑AUT‑002 | Implement `AutonomousActionValidator` — for Autonomous agents, validate that action is within permitted actions, within value threshold, within policy delta, and does not violate hard constraints | Arch. Blueprint §10.4 |
| P11‑AUT‑003 | Implement `AutonomousPolicyAdjustment` — Autonomous agents may propose policy changes (e.g., safety stock adjustments, threshold changes) that pass through PolicyGate; if Valid, auto‑apply; if ValidWithWarnings, auto‑apply with log; if Rejected, escalate | Arch. Blueprint §10.5 |
| P11‑AUT‑004 | Implement `AutonomousRollback` — any autonomous action that causes statistically significant degradation in the next evaluation period is auto‑rolled back; rollback itself is an autonomous action | Arch. Blueprint §10.4 |

#### 11.4.2 Autonomy Contract Evolution

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AUT‑010 | Implement `ContractEvolutionEngine` — tracks agent performance over time (acceptance rate, improvement rate, error rate); recommends contract level upgrades (Advisory→Guardrailed→Autonomous) when performance exceeds thresholds | Arch. Blueprint §10.9 |
| P11‑AUT‑011 | Implement `ContractEvolutionApproval` — level upgrades require human approval (Knowledge Manager + Domain Manager); downgrades are automatic on performance degradation | Arch. Blueprint §10.9 |
| P11‑AUT‑012 | Implement `ContractAuditTrail` — all contract changes recorded immutably with rationale, approver, and performance evidence | Arch. Blueprint §10.11 |

#### 11.4.3 AI Agent Fleet Management

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑AUT‑020 | Implement `AgentRegistry` — register all AI agents with their identity, domain, autonomy contract, and performance metrics | Arch. Blueprint §10.8 |
| P11‑AUT‑021 | Implement `AgentHealthMonitor` — track agent uptime, response latency, error rate, recommendation quality; alert on degradation | Arch. Blueprint §10.8 |
| P11‑AUT‑022 | Implement `AgentRateLimiter` — per‑agent rate limiting to prevent runaway automation; configurable burst and sustained limits | Arch. Blueprint §10.11 |

### 11.5 Production Hardening

**Source:** Architecture Blueprint §15.2–15.8, §17.2–17.8, §18.4–18.6

#### 11.5.1 PostgreSQL Event Store Migration

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑001 | Create `events` table in PostgreSQL with schema: `stream_name TEXT`, `stream_position BIGINT`, `event_id UUID`, `event_type TEXT`, `data_json JSONB`, `metadata_json JSONB`, `created_utc TIMESTAMPTZ`, `tenant_id TEXT` | Arch. Blueprint §15.2 |
| P11‑PRD‑002 | Create `checkpoints` table: `projection_name TEXT PRIMARY KEY`, `last_position BIGINT`, `last_message_id UUID`, `updated_utc TIMESTAMPTZ` | Arch. Blueprint §15.3 |
| P11‑PRD‑003 | Create `snapshots` table: `projection_name TEXT`, `stream_position BIGINT`, `state_json JSONB`, `created_utc TIMESTAMPTZ`, `PRIMARY KEY (projection_name, stream_position)` | Arch. Blueprint §15.5 |
| P11‑PRD‑004 | Create `idempotency` table: `message_id UUID PRIMARY KEY`, `processed_at TIMESTAMPTZ` | Arch. Blueprint §15.4 |
| P11‑PRD‑005 | Create indexes: `idx_events_created`, `idx_events_type`, `idx_events_tenant`, `idx_events_correlation` | Arch. Blueprint §15.2 |
| P11‑PRD‑006 | Implement `PostgresEnvelopeStore` implementing `EnvelopeStoreOps` — `Publish` with optimistic concurrency, `ReadStream`, `ReadAll`, `Subscribe` via PostgreSQL `LISTEN`/`NOTIFY` | Arch. Blueprint §15.2 |
| P11‑PRD‑007 | Implement `PostgresRepository<'Aggregate, 'Id, 'Event>` implementing `Repository` — `Load` (replay events from stream), `Append` (atomic insert with version check) | Arch. Blueprint §15.2 |
| P11‑PRD‑008 | Implement `PostgresCheckpointStore` — `ReadCheckpoint`, `WriteCheckpoint` | Arch. Blueprint §15.3 |
| P11‑PRD‑009 | Implement `PostgresIdempotencyStore` — `IsProcessed`, `MarkProcessed` | Arch. Blueprint §15.4 |
| P11‑PRD‑010 | Implement `PostgresSnapshotStore` — `SaveSnapshot`, `LoadLatestSnapshot` | Arch. Blueprint §15.5 |
| P11‑PRD‑011 | Implement data migration script — replay in‑memory events to PostgreSQL; verify event count and content integrity | Arch. Blueprint §15.6 |
| P11‑PRD‑012 | Implement `PostgresHealthCheck` — verify connectivity, read/write capability, replication lag | Arch. Blueprint §15.8 |

#### 11.5.2 External Event Bus

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑020 | Implement `EventBusPort` abstraction over the event bus | Arch. Blueprint §4.4.2 |
| P11‑PRD‑021 | Implement `PostgresEventBus` — uses PostgreSQL `LISTEN`/`NOTIFY` for near‑real‑time event delivery; subscribers receive notifications and read from `events` table | Arch. Blueprint §4.4.2 |
| P11‑PRD‑022 | (Optional) Implement `RabbitMQEventBus` or `KafkaEventBus` if higher throughput needed — implements same `EventBusPort` interface | Arch. Blueprint §4.4.2 |
| P11‑PRD‑023 | Implement `EventBusHealthCheck` — verify connectivity, message delivery latency, consumer lag | Arch. Blueprint §4.4.2 |
| P11‑PRD‑024 | Implement dead‑letter queue: failed events moved to `$dlq-{domain}` streams after all retries exhausted; DLQ dashboard for operations | Arch. Blueprint §13.5 |

#### 11.5.3 Security Hardening

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑030 | Implement full RBAC with OAuth 2.0 / OpenID Connect — role‑to‑policy mapping for all API endpoints | Arch. Blueprint §17.2–17.3 |
| P11‑PRD‑031 | Implement AI agent authentication via OAuth 2.0 Client Credentials with agent‑specific scopes | Arch. Blueprint §17.2.2 |
| P11‑PRD‑032 | Implement mutual TLS (mTLS) for service‑to‑service communication | Arch. Blueprint §17.2.3 |
| P11‑PRD‑033 | Implement TLS 1.3 for all external communication (HTTPS, gRPC) | Arch. Blueprint §17.6 |
| P11‑PRD‑034 | Implement secrets management integration (Azure Key Vault / AWS Secrets Manager / HashiCorp Vault) — no secrets in configuration files | Arch. Blueprint §17.6 |
| P11‑PRD‑035 | Implement PII masking at integration boundary — sensitive fields tokenised or masked in event data per policy | Arch. Blueprint §17.6 |
| P11‑PRD‑036 | Implement WAF (Web Application Firewall) rules for API gateway | Arch. Blueprint §17.7 |
| P11‑PRD‑037 | Implement DDoS protection at infrastructure level | Arch. Blueprint §17.7 |
| P11‑PRD‑038 | Security penetration test — engage external security testing; remediate findings | Arch. Blueprint §17.8 |

#### 11.5.4 Multi‑Tenancy

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑040 | Implement tenant‑scoped event streams — `tenant_id` column on `events` table; all queries filtered by tenant | Arch. Blueprint §15.7 |
| P11‑PRD‑041 | Implement tenant‑scoped projections — projection agents operate in dedicated mode (one per tenant) or scoped mode (single agent with `Map<TenantId, ProjectionState>`) | Arch. Blueprint §15.7 |
| P11‑PRD‑042 | Implement tenant‑scoped checkpoints — checkpoint key includes tenant ID in dedicated mode | Arch. Blueprint §15.7 |
| P11‑PRD‑043 | Implement tenant isolation in API — `ExecutionContext.TenantId` propagated through all layers; cross‑tenant data access prevented at application layer | Arch. Blueprint §15.7 |
| P11‑PRD‑044 | Implement tenant‑scoped Stores — UI Stores scoped to current tenant; switching tenant clears and reloads all Stores | Arch. Blueprint §15.7 |

#### 11.5.5 Backup & Disaster Recovery

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑050 | Configure PostgreSQL continuous archiving (WAL archiving to cloud object storage) for point‑in‑time recovery | Arch. Blueprint §15.8 |
| P11‑PRD‑051 | Implement daily logical backups (`pg_dump` of `events` table) to cloud object storage | Arch. Blueprint §15.8 |
| P11‑PRD‑052 | Implement projection rebuild from event store — documented and tested procedure for rebuilding all projections from scratch | Arch. Blueprint §15.8 |
| P11‑PRD‑053 | Implement automated DR drill script — restore backup, rebuild projections, validate data integrity; run quarterly | Arch. Blueprint §15.8 |
| P11‑PRD‑054 | Document RTO (Recovery Time Objective) and RPO (Recovery Point Objective) with measured results | Arch. Blueprint §15.8 |

#### 11.5.6 Performance Optimisation

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑060 | Implement projection snapshots — periodic serialisation of projection state to `snapshots` table; on restart, load latest snapshot and replay only newer events | Arch. Blueprint §15.5 |
| P11‑PRD‑061 | Implement aggregate snapshotting — for aggregates with long event histories, store periodic snapshots to speed up `Load` operations | Arch. Blueprint §15.5 |
| P11‑PRD‑062 | Implement `PlanIndex` caching — cache frequently‑accessed index lookups (by SKU, by resource, by demand) for IncrementalRepair and FastInsert | Arch. Blueprint §9.3 |
| P11‑PRD‑063 | Implement API response caching — cache frequently‑queried read model data with TTL‑based invalidation | Arch. Blueprint §6.2 |
| P11‑PRD‑064 | Implement connection pooling for PostgreSQL — configure optimal pool size per service | Arch. Blueprint §15.2 |
| P11‑PRD‑065 | Implement load testing suite — benchmark critical paths: ATP check (target <50ms p99), forecast generation (target <5s for 100 SKUs), event append (target <20ms for batch of 10), projection catch‑up (target <1s for 1,000 events) | Arch. Blueprint §19.7 |
| P11‑PRD‑066 | Implement SLO monitoring — track latency, throughput, error rate against defined SLOs; alert on violations | Arch. Blueprint §12.7 |

#### 11.5.7 Observability Export

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑070 | Configure Prometheus metrics export — expose `/metrics` endpoint with all counters, gauges, histograms from `Metrics` module | Arch. Blueprint §12.8.1 |
| P11‑PRD‑071 | Deploy Grafana dashboards — System Health, Business KPIs, AI Performance, Trace Explorer | Arch. Blueprint §12.8.2 |
| P11‑PRD‑072 | Configure structured log export to Elasticsearch / Splunk / Azure Monitor — JSON‑formatted logs with `LogContext` fields | Arch. Blueprint §12.8.3 |
| P11‑PRD‑073 | Configure distributed tracing export to Jaeger / Zipkin — `Activity` spans exported via OpenTelemetry | Arch. Blueprint §12.4 |
| P11‑PRD‑074 | Implement alerting rules in Prometheus / Grafana — projection lag, circuit breaker state, event ingest drop, command failure rate, health degradation | Arch. Blueprint §12.7 |

#### 11.5.8 Containerisation & Kubernetes

| ID | Todo Item | Source |
|----|-----------|--------|
| P11‑PRD‑080 | Create `Dockerfile` for `Medhavi.Hub` — multi‑stage build, optimised for size and startup time | Arch. Blueprint §18.6 |
| P11‑PRD‑081 | Create Kubernetes manifests: `Deployment`, `Service`, `ConfigMap`, `Secret`, `HorizontalPodAutoscaler`, `PodDisruptionBudget` | Arch. Blueprint §18.6 |
| P11‑PRD‑082 | Implement health check probes — liveness (`/health/live`) and readiness (`/health/ready`) endpoints | Arch. Blueprint §12.6 |
| P11‑PRD‑083 | Implement graceful shutdown — drain in‑flight requests, flush logs, close database connections on `SIGTERM` | Arch. Blueprint §18.6 |
| P11‑PRD‑084 | Implement CI/CD pipeline — build, test, containerise, deploy to staging, run smoke tests, promote to production | Arch. Blueprint §18.6 |

---

This completes Phase 11 Todo Items. The phase covers agentic exception resolution (autonomous engine, integration with all Detect capabilities, human‑AI handoff), continuous planning (event‑driven micro‑replanning, live plan graph, impact analysis), supply chain graph GNN (multi‑graph, risk propagation, integration with Knowledge Intelligence), full AI Autonomy Level 3 (autonomous actions, contract evolution, agent fleet management), and production hardening (PostgreSQL migration, external event bus, security, multi‑tenancy, backup/DR, performance optimisation, observability export, containerisation).

---