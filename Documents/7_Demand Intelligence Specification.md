# Demand Intelligence Specification

**Status:** Active v1.1
**Domain Code:** D
**Governed By:** ARS v2
**Traceability:** CN-001

---

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Demand Intelligence is the authoritative enterprise domain responsible for developing trusted understanding of customer demand. It answers the Enterprise Question: **What is needed?**

Every forecast, segmentation, classification, prioritisation, quality evaluation, exception detection, explanation, and learning activity related to demand originates from and is governed by this specification. Demand Intelligence provides the foundation upon which all downstream planning activities depend.

## 1.2 Scope

**Included:** Demand observations, demand history, demand signals, demand forecasting at all aggregation levels and time horizons, demand segmentation, demand classification, demand prioritisation, demand quality evaluation, demand exception detection, demand explainability, and continuous learning.

**Excluded:** Supply planning, inventory planning, production planning, procurement planning, transportation planning, and order promising. These belong to their respective Intelligence Domains.

## 1.3 Responsibility Boundary

The responsibility of Demand Intelligence begins when a business observation capable of influencing enterprise demand is received. It ends when demand understanding has been published and made available for downstream planning.

**Traceability:** Realises CN-001. Governed by ARS v2.

---

# Chapter 2 — Business Objectives

| ID | Objective | Traceability |
|----|-----------|--------------|
| BO-DI-001 | Deliver Trusted Demand Understanding | CN-EP-001 |
| BO-DI-002 | Improve Planning Effectiveness | CN-EP-001 |
| BO-DI-003 | Improve Enterprise Responsiveness | CN-EP-001 |
| BO-DI-004 | Improve Customer Outcomes | CN-EP-001 |
| BO-DI-005 | Increase Planning Automation | CN-EP-001 |
| BO-DI-006 | Continuously Improve Enterprise Intelligence | CN-EP-001 |

---

# Chapter 3 — Enterprise Measures

| ID | Measure | Produced By |
|----|---------|-------------|
| PI-DI-002 | Forecast Accuracy | Evaluate Demand Quality |
| PI-DI-003 | Weighted Absolute Percentage Error | Evaluate Demand Quality |
| PI-DI-004 | Mean Absolute Percentage Error | Evaluate Demand Quality |
| PI-DI-005 | Forecast Bias | Evaluate Demand Quality |
| PI-DI-006 | Forecast Value Added | Evaluate Demand Quality |
| PI-DI-007 | Forecast Stability | Evaluate Demand Quality |
| PI-DI-102 | Demand Signal Quality Index | Understand Demand |
| PI-DI-103 | Forecast Confidence Index | Forecast Demand |
| PI-DI-107 | Explainability Score | Explain Demand |
| PI-DI-205 | Data Completeness | Understand Demand |
| PI-DI-206 | Data Quality Score | Understand Demand |
| PI-DI-202 | Forecast Generation Time | Forecast Demand |

Remaining PIs reserved for future capability realizations.

---

# Chapter 4 — Semantic Model

## 4.1 Enterprise Temporal Semantics

| Temporal Dimension | Business Meaning |
|--------------------|------------------|
| Business Time | The point in time when an event occurred in enterprise reality. |
| Observation Time | The point in time when the enterprise received the business observation. |
| Transaction Time | The point in time when an aggregate was created or revised within Demand Intelligence. |
| Publication Time | The point in time when an aggregate became authoritative and visible to downstream consumers. |
| Effective Time | The planning period for which information is valid. |

Every Aggregate Root that undergoes state transitions shall record at minimum Business Time and Transaction Time. Published aggregates shall additionally record Publication Time.

## 4.2 Object Classification

| ARS Classification | Ontology Nature | Ontology Behavior |
|--------------------|-----------------|-------------------|
| Aggregate Root | Record, Scope, State, Projection, Process | Immutable, Versioned, Derived, Authoritative |
| Entity | (inherits Nature from parent Aggregate) | (inherits Behavior from parent Aggregate) |
| Value Object | — | Immutable |
| Reference Object | — | (owned externally) |

**Ontology Dimensions:**

- **Nature:** Record (immutable capture of reality), Scope (planning independence boundary), State (authoritative representation at a point in time), Projection (derived estimate of future reality), Knowledge (learned understanding), Decision (recorded business choice), Process (ongoing activity).
- **Behavior:** Immutable (never changes after creation), Versioned (new versions supersede old), Derived (computed from other objects), Authoritative (the single source of truth for consumers).
- **Structure:** Represents a governed enterprise relationship model, hierarchy, network, or classification structure whose purpose is to organize other business objects rather than record operational state.

## 4.3 Reference Consistency

Historical Demand Observations reference Reference Objects as they existed at Observation Time. Subsequent changes do not retroactively alter historical records (BR-D-058).

## 4.4 Enterprise Information Flow

```text
Business Observation
    │
    ▼
Demand Observation (SE-D-001)
    │
    ▼
Operational Demand (SE-D-011)
    │
    ▼
Planning Demand (SE-D-010) ─── Consumed by Supply Planning, Inventory Planning
    │
    ▼
Enterprise Demand Picture (SE-D-003) ─── Consumed by Forecast Demand, Segment Demand,
                                          Classify Demand, Evaluate Demand Quality,
                                          Supply Intelligence, Promise Intelligence,
                                          Scenario Intelligence
    │
    ▼
Forecast Publication (SE-D-029) ─── Consumed by Understand Demand (forecast-derived
                                      demand), Supply Intelligence, Promise Intelligence,
                                      Scenario Intelligence
```

---

## 4.5 Aggregate Roots

### 4.5.1 Demand Observation — SE-D-001

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Preserve the exact operational truth of a received business observation until it is evaluated. |
| Definition | An immutable enterprise record of a business observation received from any source. |
| Identity | Demand Observation Identifier — composite of Source System, Business Document ID, and Line ID (if applicable). Globally unique, immutable. |
| Business Owner | Understand Demand (CA-D-001) |
| Produced By | AB-D-001 Establish Demand Observation |
| Consumed By | Evaluated internally; after publication, resulting Planning Demand is consumed by downstream domains. |
| Lifecycle Expectation | Progresses through Received → Accepted / Quarantined / Rejected. Evaluated exactly once from Received. |
| Retention Expectation | Retained permanently for audit and explainability. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Record |
| Behavior | Immutable |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Observation Identifier | Yes | Yes |
| Mandatory | SKU (Reference to SE-DI-040) | Yes | Yes |
| Mandatory | StockingPoint (Reference to SE-DI-041) | Yes | Yes |
| Mandatory | Quantity (Value Object: numeric value, unit of measure) | Yes | Yes |
| Mandatory | Observation Type (enum: Sales Order, Shipment, POS, Return, Correction, Signal) | Yes | Yes |
| Mandatory | Business Time | Yes | Yes |
| Optional | Customer (Reference to SE-DI-030) | No | Yes |
| Optional | Promotion, Campaign, Contract Reference | No | Yes |
| Derived | Planning Scope assignment | No | No |
| Derived | Lifecycle State | No | No |
| Derived | Evaluation Decision, Confidence, Rationale | No | No |
| Traceability | Source System, Observation Time, Business Document ID | Yes | Yes |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Received | Observation established, not yet evaluated. | Identity and mandatory attributes populated. Provenance recorded. |
| Accepted | Passed evaluation, eligible for planning. | Decision Identifier, Timestamp, Confidence, Warning Code (if any), Rationale recorded. |
| Quarantined | Failed evaluation; awaiting manual review. | Decision Identifier, Timestamp, Quarantine Reason recorded. |
| Rejected | Permanently excluded from Enterprise Demand. | Decision Identifier, Timestamp, Rejection Reason recorded. |

**Permitted Transitions:** Received → Accepted, Received → Quarantined, Received → Rejected, Quarantined → Accepted, Quarantined → Rejected.

**Invariants:**
- Evaluated only once from Received (BR-D-014).
- Accepted observations assigned to exactly one Planning Scope (BR-D-004).
- Rejected observations never contribute to Enterprise Demand (BR-D-002).
- Quarantined observations never contribute until accepted (BR-D-003).

**Business Operations:** Receive, Evaluate, Assign Planning Scope.

**Decisions Owned:** DE-D-010 Accept Demand Observation.

**Traceability:** Business Owner: CA-D-001. Produced By: AB-D-001. Referenced by: FS-D-001, FS-D-002, FS-D-003. Governed By: BR-D-001 through BR-D-004, BR-D-013, BR-D-014, BR-D-016 through BR-D-020.

### 4.5.2 Planning Scope — SE-D-002

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Define the boundary of planning independence for enterprise demand. |
| Definition | A unique combination of SKU, StockingPoint, Customer (optional), and Planning Time Bucket that partitions demand into independent planning units. |
| Identity | Composite of SKU, StockingPoint, Customer (optional), Planning Time Bucket. Immutable once established. |
| Business Owner | Understand Demand (CA-D-001) |
| Produced By | AB-PS-001 Determine Planning Scope |
| Consumed By | Enterprise Demand Picture (organizes demand); downstream planning domains (as routing key). |
| Lifecycle Expectation | Active or Archived. Never deleted. |
| Retention Expectation | Retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Scope |
| Behavior | Immutable |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | SKU, StockingPoint, Customer (optional), Planning Time Bucket | Yes | Yes |
| Mandatory | Lifecycle State (Active, Archived) | Yes | No |
| Traceability | Created Timestamp (Transaction Time) | Yes | Yes |

**Lifecycle:** Active → Archived.

**Invariants:**
- Identity uniquely identifies one Planning Scope (BR-D-025).
- At most one Active Planning Scope per identity (BR-D-027).
- Never deleted, only archived (BR-D-048).

**Business Operations:** Determine, Archive.

**Traceability:** Business Owner: CA-D-001. Produced By: AB-PS-001. Referenced by: FS-D-003, FS-D-004. Governed By: BR-D-024, BR-D-025, BR-D-027, BR-D-048.

### 4.5.3 Enterprise Demand Picture — SE-D-003

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide exactly one authoritative planning interpretation for a Planning Scope at any point in time. |
| Definition | A versioned enterprise state representing the accumulated Operational Demand and calculated Planning Demand for a Planning Scope. |
| Identity | Composite of Planning Scope identity and Version number (monotonically increasing integer starting at 1). |
| Business Owner | Understand Demand (CA-D-001) |
| Produced By | AB-EDP-001 (revise), AB-EDP-002 (calculate Planning Demand), AB-EDP-003 (publish) |
| Consumed By | Forecast Demand, Segment Demand, Classify Demand, Evaluate Demand Quality, Supply Intelligence, Inventory Intelligence, Promise Intelligence, Scenario Intelligence. |
| Lifecycle Expectation | Draft → Awaiting Calculation → Ready For Publication → Published → Superseded. Each version immutable once created. |
| Retention Expectation | All versions retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Versioned, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Planning Scope (Reference) + Version | Yes | Yes |
| Mandatory | Lifecycle State | Yes | No |
| Derived | Operational Demand (SE-D-011) | No | No |
| Derived | Planning Demand (SE-D-010) | No | No |
| Traceability | Transaction Time, Publication Time, Superseded Version ID | Conditional | Yes |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Version created, Operational Demand populated. | Identity assigned. Operational Demand entity created or revised. |
| Awaiting Planning Demand Calculation | Ready for Planning Demand calculation. | Status set. |
| Ready For Publication | Planning Demand calculated. | Planning Demand entity created. Calculation traceability recorded. |
| Published | Authoritative, visible to consumers. | Publication Time recorded. Previous version → Superseded. |
| Superseded | Replaced by a newer Published version. | Set on previous version when new version publishes. |

**Permitted Transitions:** Draft → Awaiting Planning Demand Calculation → Ready For Publication → Published → Superseded. A Superseded version shall never return to Published (BR-D-056).

**Versioning Semantics:** A new version is created whenever an Accepted Demand Observation, approved Planning Adjustment, or approved Planner Override affects the Planning Scope. Each version is immutable once created. The Version number is assigned at creation and never changes. Versions represent distinct enterprise states at different points in Transaction Time.

**Invariants:**
- Exactly one Published version per Planning Scope at any moment (BR-D-005).
- Published version never modified; change creates a new version (BR-D-006).
- Superseded version never returns to Published (BR-D-056).
- Records Business Time, Transaction Time, and Publication Time (BR-D-057).

**Business Operations:** Revise, Calculate Planning Demand, Publish.

**Decisions Owned:** DE-D-012 Publish Enterprise Demand Picture.

**Traceability:** Business Owner: CA-D-001. Contains: SE-D-010, SE-D-011. Referenced by: FS-D-004, FS-D-005, FS-D-006. Governed By: BR-D-005, BR-D-006, BR-D-008, BR-D-027, BR-D-028, BR-D-044 through BR-D-048, BR-D-056, BR-D-057.


### 4.5.4 Forecast Publication — SE-D-029

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the enterprise's single authoritative projection of future demand for a defined Planning Scope, representing the enterprise planning baseline consumed by downstream planning capabilities. |
| Definition | A versioned, published, immutable snapshot of future demand projections for a defined set of SKU-location combinations over a fixed horizon, produced by a forecast cycle and approved for enterprise consumption. |
| Identity | Business Identity: Planning Scope + Publication Version. Technical Identifier: Forecast Publication Identifier (globally unique, immutable) |
| Business Owner | Forecast Demand (CA-D-002) |
| Produced By | AB-F-005 Publish Forecast Publication |
| Consumed By | Understand Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality |
| Lifecycle Expectation | Draft → Published → Superseded. Each publication immutable once Published. |
| Retention Expectation | All publications retained permanently for audit and accuracy measurement. |

**Architectural Note:** Forecast Publication is the authoritative enterprise business object for future demand. Its aggregate realization (single aggregate, partitioned aggregates, or collaborating aggregates) will be validated during aggregate design, considering business invariants, transactional boundaries, and expected planning scale.

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Versioned, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Business Identity Planning Scope + Publication Version | Yes | Yes |
| Mandatory | Planning Scope (Reference to one or more SE-D-002 Planning Scope identifiers) | Yes | Yes |
| Mandatory | Forecast Horizon | Yes | Yes |
| Mandatory | Time Bucket configuration | Yes | Yes |
| Mandatory | Lifecycle State (Draft, Published, Superseded) | Yes | No |
| Mandatory | Version number (monotonically increasing) | Yes | Yes |
| Derived | Champion Model Identifier | No | No |
| Derived | Overall Confidence Index | No | No |
| Derived | Forecasts (collection of SE-D-025) | No | No |
| Derived | Assumptions (collection of SE-D-027) | No | No |
| Derived | Overrides (collection of SE-D-028) | No | No |
| Traceability | Transaction Time (when version created) | Yes | Yes |
| Traceability | Publication Time (when version became authoritative) | No (only if Published) | Yes |
| Traceability | Superseded Publication Identifier | No | Yes |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Publication created by a forecast cycle. Forecasts generated, assumptions recorded, overrides applied, hierarchy reconciled. | All forecast lines created. Assumptions captured. Overrides recorded. Reconciliation applied. |
| Published | Publication released as the authoritative planning baseline. | Publication Time recorded. Previous publication for the same scope → Superseded. BN-D-011 published. |
| Superseded | Replaced by a newer Published publication. | State set on the older publication. |

**Permitted Transitions:** Draft → Published → Superseded. A Superseded publication shall never return to Published (BR-D-029).

**Invariants**
- Exactly one Published Forecast Publication exists for a given Planning Scope at any moment (BR-D-005-F).
- A Published publication is immutable. Any change requires a new publication produced by a new forecast cycle (BR-D-029).
- All forecast lines within a publication are generated using the same champion model unless explicitly flagged otherwise.
- All forecast lines are hierarchically reconciled before publication (BR-D-034).
- The original system forecast is preserved when an override is applied (BR-D-045).

**Business Operations:** Create Draft, Generate Forecasts, Record Assumption, Record Override, Reconcile, Publish.

**Decisions Owned:** DE-D-020, DE-D-021, DE-D-022, DE-D-023, DE-D-024, DE-D-025

**Traceability:** Business Owner: CA-D-002. Contains: SE-D-025, SE-D-027, SE-D-028. Referenced by: FS-D-007–FS-D-011. Governed by rules in Chapter 7.



### 4.5.5 Demand Behaviour Assessment — SE-D-035

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain the enterprise’s continuously current understanding of demand behaviour for a monitored planning entity, detecting meaningful changes from expected behaviour. |
| Definition | The enterprise’s authoritative, continuously maintained assessment of how demand is behaving for a specific SKU-location combination, relative to its statistical baseline. |
| Identity | Composite of SKU (Reference to SE-DI-040) and StockingPoint (Reference to SE-DI-041). Immutable once the entity is placed under monitoring. |
| Business Owner | Sense Demand (CA-D-003) |
| Produced By | AB-S-001 Update Demand Behaviour Assessment |
| Consumed By | Planners (via dashboards), Forecast Demand (for out-of-cycle refresh), Detect Demand Exceptions (future), Explain Demand (future), Learn From Demand (future) |
| Lifecycle Expectation | The assessment exists as long as the SKU-StockingPoint is monitored. Its state changes in response to incoming signals, but the assessment itself persists continuously. |
| Retention Expectation | Current state retained permanently while monitoring is active; historical state changes retained permanently for audit and detection accuracy analysis. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | SKU (Reference) + StockingPoint (Reference) | Yes | Yes |
| Mandatory | Current State (Normal, Elevated, Depressed, Critical) | Yes | No |
| Mandatory | Last Updated Timestamp (Transaction Time) | Yes | No |
| Derived | Current Deviation (magnitude and direction from baseline; null when Normal) | No | No |
| Derived | Confidence Score | No | No |
| Derived | Corroborating Signal Count (for current state) | No | No |
| Traceability | Baseline Reference (identifier of the Demand Baseline used for evaluation) | Yes | No |
| Traceability | State Change History (collection of StateChangeEvent entities) | Yes | No |

**Lifecycle of the Assessment**

The assessment itself does not have a traditional lifecycle with start and end states tied to anomalies. It is **Active** for the duration of monitoring. Its internal state changes are captured as **State Change Events** (see below).

**Lifecycle of State Changes**

Each state change is recorded as a **State Change Event** (Entity within SE-D-035):

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| (any state) | A new signal is evaluated. | If the evaluated signal does not warrant a state change, no transition occurs. If it does, a new State Change Event is appended. |
| Normal → Elevated / Depressed / Critical | Demand has deviated beyond the configured threshold. | New State Change Event created. Current State updated. BN-D-015 published. |
| Elevated / Depressed / Critical → Normal | Demand has returned to within expected bounds. | New State Change Event created. Current State updated. BN-D-015 published. |
| Elevated → Critical | Severity escalated. | New State Change Event created. Current State updated. BN-D-015 published. |
| Critical → Elevated | Severity de-escalated. | New State Change Event created. Current State updated. BN-D-015 published. |

**Permitted Transitions:** Any state to any other state, based on signal evaluation. The assessment’s Current State is always the most recent State Change Event’s resulting state.

**Invariants:**
- At any moment, a monitored SKU-StockingPoint has exactly one Current State.
- The Current State is always the result of the most recent State Change Event.
- A State Change Event shall not be created unless the deviation meets the configured threshold for the target state (Elevated or Critical thresholds per BR-D-050).
- State Change Events are immutable once recorded.

**Business Operations:** Evaluate Signal (updates state and evidence), Acknowledge (planner marks as seen — does not change state).

**Traceability:** Business Owner: CA-D-003. Produced By: AB-S-001. Referenced by: FS-D-014, FS-D-015. Governed by rules in Chapter 7.



### 4.5.6 Planning Classification Assignment — SE-D-036

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain the enterprise’s continuously current planning classification for a planning entity under a governed classification scheme. |
| Definition | The authoritative, continuously maintained classification of a planning entity (SKU or Customer) within a single classification scheme (e.g., ABC, XYZ, Strategic), used by downstream capabilities to vary planning behaviour. |
| Identity | Composite of Entity Type (SKU/Customer), Entity Identifier, and Classification Type (e.g., 'ABC', 'XYZ', 'Strategic'). Immutable while the entity is active and the type is in use. |
| Business Owner | Segment Demand (CA-D-004) |
| Produced By | AB-SG-001 Update Planning Classification |
| Consumed By | Forecast Demand, Prioritize Demand, Inventory Planning, Replenishment, Safety Stock, and all downstream capabilities that vary behaviour by classification. |
| Lifecycle Expectation | Active while the entity is active and the classification type is in use. Updated when re-evaluation is triggered. History permanently preserved. |
| Retention Expectation | Current assignment retained while entity and type are active. Historical assignments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Entity Type + Entity Identifier + Classification Type | Yes | Yes |
| Mandatory | Current Classification (the class label) | Yes | No |
| Mandatory | Classification Confidence | Yes | No |
| Mandatory | Last Classified Timestamp | Yes | No |
| Traceability | Classification History (collection of Assignment Change Events) | Yes | No |

**Lifecycle of Changes**

Each classification update is recorded as an **Assignment Change Event** (Entity within SE-D-036):

| Event | Description |
|-------|-------------|
| Classification Updated | The classification has changed. A new Assignment Change Event is appended. BN-D-017 published. |

**Invariants:**
- At any moment, an active entity has exactly one current classification per active classification type (BR-D-063).
- Classification must be based on the current Segmentation Policy and sufficient evidence (BR-D-061).
- Assignment Change Events are immutable (BR-D-064).

**Business Operations:** Classify (update assignment), Override (planner override with justification).

**Traceability:** Business Owner: CA-D-004. Produced By: AB-SG-001. Referenced by: FS-D-016. Governed by BR-D-061, BR-D-062, BR-D-063, BR-D-064.



### 4.5.7 Demand Behaviour Assignment — SE-D-037

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain the enterprise’s authoritative behavioural classification for a planning entity under a governed classification scheme. |
| Definition | The authoritative, continuously maintained classification of a planning entity (SKU-StockingPoint) within a single behavioural dimension (e.g., Statistical Pattern, Lifecycle Behaviour, Promotion Sensitivity), used by downstream capabilities to select forecasting models, set exception thresholds, and focus planner attention. |
| Identity | Composite of Entity Type (SKU), Entity Identifier, and Behaviour Dimension (e.g., 'Statistical Pattern', 'Lifecycle Behaviour'). Immutable while the entity is active and the dimension is in use. |
| Business Owner | Classify Demand (CA-D-005) |
| Produced By | AB-CL-001 Update Behaviour Classification |
| Consumed By | Forecast Demand (model selection), Detect Demand Exceptions (threshold setting), Explain Demand (evidence for explanations), Prioritize Demand (planner attention), Inventory Planning (safety stock policy), Supply Intelligence, Scenario Intelligence. |
| Lifecycle Expectation | Active while the entity is active and the behaviour dimension is in use. Updated when re-evaluation is triggered. History permanently preserved. |
| Retention Expectation | Current assignment retained while entity and dimension are active. Historical assignments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Entity Type + Entity Identifier + Behaviour Dimension | Yes | Yes |
| Mandatory | Current Classification (the behaviour label) | Yes | No |
| Mandatory | Classification Confidence | Yes | No |
| Mandatory | Evidence Summary (business-level description of why this classification was assigned) | Yes | No |
| Mandatory | Last Classified Timestamp | Yes | No |
| Traceability | Classification History (collection of Behaviour Change Events) | Yes | No |

**Lifecycle of Changes**

| Event | Description |
|-------|-------------|
| Classification Updated | The classification has changed. A new Behaviour Change Event is appended. BN-D-019 published. |

**Invariants:**
- At any moment, an active entity has exactly one current classification per active behaviour dimension (BR-D-073).
- Classification must be based on the current Classification Policy and sufficient evidence (BR-D-066).
- Behaviour Change Events are immutable (BR-D-068).

**Business Operations:** Classify (update assignment), Override (planner override with justification).

**Traceability:** Business Owner: CA-D-005. Produced By: AB-CL-001. Referenced by: FS-D-017. Governed by BR-D-066–074.



### 4.5.8 Planning Priority Assignment — SE-D-038

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain the enterprise’s continuously current assessment of planning importance for every planning entity, directing planner attention, exception handling, and allocation decisions to the most impactful items. |
| Definition | The authoritative, continuously maintained priority of a planning entity (SKU, Customer, or SKU-Customer combination), representing its relative planning importance derived from multiple business considerations and used to order planner worklists, prioritise exception alerts, and guide allocation decisions. |
| Identity | Composite of Entity Type (SKU, Customer, or SKU-Customer) and Entity Identifier(s). One assignment per planning entity. Immutable while the entity is active. |
| Business Owner | Prioritize Demand (CA-D-006) |
| Produced By | AB-PR-001 Update Planning Priority |
| Consumed By | Planners (worklist ordering), Detect Demand Exceptions (alert prioritisation), Forecast Demand (high-priority protection rules), Inventory Planning (allocation priority), Scenario Intelligence (impact assessment). |
| Lifecycle Expectation | Active while the entity is active. Updated when re-evaluation is triggered. History permanently preserved. |
| Retention Expectation | Current assignment retained while entity is active. Historical assignments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Entity Type + Entity Identifier(s) | Yes | Yes |
| Mandatory | Current Priority (Critical, High, Medium, Low, Unclassified) | Yes | No |
| Mandatory | Priority Score (0–100, derived) | Yes | No |
| Mandatory | Decision Rationale (business-language explanation of why this priority was assigned) | Yes | No |
| Mandatory | Business Validity (the business conditions under which this priority applies, e.g., 'during campaign', 'until backlog cleared', 'effective Q3') | Yes | No |
| Mandatory | Last Evaluated Timestamp | Yes | No |
| Traceability | Priority History (collection of Priority Change Events) | Yes | No |

**Lifecycle of Changes**

| Event | Description |
|-------|-------------|
| Priority Changed | The priority level or score has changed. A new Priority Change Event is appended. BN-D-020 published. |

**Invariants:**
- At any moment, an active entity has exactly one current priority (BR-D-078).
- Priority must be derived from the current Prioritization Policy and available business evidence (BR-D-075).
- Priority Change Events are immutable (BR-D-079).

**Business Operations:** Evaluate (compute priority and decision rationale), Override (planner override with business justification).

**Traceability:** Business Owner: CA-D-006. Produced By: AB-PR-001. Referenced by: FS-D-018. Governed by BR-D-075–080.



### 4.5.9 Forecast Quality Assessment — SE-D-039

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Publish an authoritative, periodic enterprise assessment of forecast quality, enabling the enterprise to measure and continuously improve forecasting performance. |
| Definition | A versioned, published, immutable enterprise assessment of forecast quality for a defined Planning Scope and Evaluation Period, comparing published forecasts against actual demand according to the enterprise Forecast Measurement Policy. |
| Identity | Forecast Quality Assessment Identifier, globally unique, assigned at creation. Immutable. Business uniqueness: Planning Scope + Evaluation Period + Version. |
| Business Owner | Evaluate Demand Quality (CA-D-007) |
| Produced By | AB-EQ-001 Publish Forecast Quality Assessment |
| Consumed By | Learn From Demand (model improvement), Explain Demand (performance context), Forecast Demand (model performance feedback), Demand Planners and Managers (performance dashboards). |
| Lifecycle Expectation | Draft → Published → Superseded. Supersession occurs only when a new version of the assessment for the **same** Planning Scope and Evaluation Period is published. Assessments for different periods are independent. |
| Retention Expectation | All published assessments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Versioned, Authoritative (when Published) |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Forecast Quality Assessment Identifier | Yes | Yes |
| Mandatory | Planning Scope Reference | Yes | Yes |
| Mandatory | Evaluation Period (start and end dates) | Yes | Yes |
| Mandatory | Lifecycle State (Draft, Published, Superseded) | Yes | No |
| Mandatory | Version number | Yes | Yes |
| Derived | Core Metrics (WAPE, MAPE, Forecast Bias, Forecast Accuracy) per the Forecast Measurement Policy | Yes | Yes |
| Derived | Optional Metrics (FVA, Forecast Stability, Planner Override Effectiveness) per the Forecast Measurement Policy | No | Yes |
| Derived | Overall Quality Score — derived per the Forecast Measurement Policy | No | Yes |
| Traceability | Transaction Time | Yes | Yes |
| Traceability | Publication Time | No (only if Published) | Yes |
| Traceability | Superseded Assessment Identifier | No | Yes |
| Traceability | Source Forecast Publication Reference(s) | Yes | Yes |
| Traceability | Source Demand History Reference(s) | Yes | Yes |
| Traceability | Forecast Measurement Policy Version Reference | Yes | Yes |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Assessment created, metrics computed. The assessment exists as enterprise state because the enterprise may review and validate it before publication. | All metrics populated. Source references and policy version recorded. |
| Published | Assessment released as the authoritative enterprise evaluation for the Planning Scope and Evaluation Period. | Publication Time recorded. Previous version for the **same** Planning Scope and Evaluation Period → Superseded. BN-D-021 published. |
| Superseded | Replaced by a newer Published version for the **same** Planning Scope and Evaluation Period. | State set on the older assessment. |

**Permitted Transitions:** Draft → Published → Superseded.

**Invariants:**
- Exactly one Published assessment exists for a given Planning Scope and Evaluation Period (BR-D-083).
- A Published assessment is immutable (BR-D-084).
- Source data must meet the completeness threshold defined in the Forecast Measurement Policy (BR-D-080).

**Business Operations:** Publish (the enterprise operation; computation is a necessary internal step, not a separate business operation).

**Traceability:** Business Owner: CA-D-007. Produced By: AB-EQ-001. Referenced by: FS-D-019. Governed by BR-D-080–085.



### 4.5.10 Demand Planning Condition — SE-D-040

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain the enterprise’s continuously current record of demand planning conditions that require attention, enabling the enterprise to recognize and respond to situations where demand information is insufficient, inaccurate, or degrading. |
| Definition | An authoritative, continuously maintained enterprise record that a specific abnormal condition exists in the demand picture, recognized by comparing current demand information against governed thresholds and persisting until the underlying data returns to acceptable bounds. |
| Identity | Demand Planning Condition Identifier, globally unique, assigned at creation. Immutable. |
| Business Owner | Detect Demand Exceptions (CA-D-008) |
| Produced By | AB-DE-001 Recognize Demand Planning Condition |
| Consumed By | Planners (via a separate workflow capability for review and action), Forecast Demand (model conditions may trigger champion re-evaluation), Explain Demand (context for explanations), Learn From Demand (pattern learning for proactive detection). |
| Lifecycle Expectation | Active while the underlying data supports the condition. Resolved when the underlying data returns to within governed thresholds. Once Resolved, the condition is terminal. Recurrence creates a new condition instance with a new identifier. History permanently preserved. |
| Retention Expectation | Current conditions retained while active. Historical conditions retained permanently for audit and detection effectiveness analysis. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Demand Planning Condition Identifier | Yes | Yes |
| Mandatory | Planning Entity (SKU, SKU-StockingPoint, Segment, or Enterprise-wide) | Yes | Yes |
| Mandatory | Condition Type (e.g., Forecast Bias Elevated, Data Completeness Gap, Model Performance Degradation) | Yes | Yes |
| Mandatory | Current State (Active, Resolved) | Yes | No |
| Mandatory | Severity (Critical, High, Medium, Low) | Yes | No |
| Mandatory | Detection Evidence (statistical or data evidence that triggered the condition) | Yes | Yes |
| Mandatory | Detection Timestamp | Yes | Yes |
| Optional | Resolution Timestamp | No | Yes |
| Optional | Resolution Evidence (evidence that the underlying data returned to acceptable bounds) | No | Yes |
| Traceability | Condition History (collection of Condition Change Events) | Yes | No |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | The condition currently exists in the demand picture. | Detection Evidence and Detection Timestamp recorded. BN-D-022 published. |
| Resolved | The underlying data has returned to within governed thresholds. The condition is terminal. | Resolution Timestamp and Resolution Evidence recorded. BN-D-023 published. |

**Permitted Transitions:** Active → Resolved. Once Resolved, the condition is terminal.

**Invariants:**
- A Demand Planning Condition shall not be created unless the detection evidence meets the thresholds defined in the Exception Detection Policy (BR-D-086).
- At any moment, a Demand Planning Condition is either Active or Resolved, never both (BR-D-089).
- A Resolved condition is terminal. Recurrence of the same condition type for the same planning entity shall create a new condition instance with a new identifier (BR-D-091).
- Condition History shall be permanently preserved (BR-D-090).

**Business Operations:** Recognize (create or update condition), Resolve (when underlying data returns to normal).

**Traceability:** Business Owner: CA-D-008. Produced By: AB-DE-001. Referenced by: FS-D-020. Governed by BR-D-086–091.



### 4.5.11 Demand Explanation — SE-D-041

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide an immutable, traceable, deterministic enterprise record of the reasoning behind a specific demand artifact, enabling planners, auditors, and AI systems to understand why forecasts, decisions, conditions, classifications, and priorities exist as they do. |
| Definition | An authoritative, immutable enterprise record documenting the structured reasoning — the rules, decisions, policies, evidence, and their relationships — that produced a specific demand intelligence output at a specific point in time. The reasoning structure is deterministic and reproducible. Natural language is one derived rendering of this canonical reasoning. |
| Identity | Demand Explanation Identifier, globally unique, assigned at creation. Immutable. When an explanation is requested for the same artifact under identical reasoning, policy versions, and template version, the existing explanation is returned rather than creating a duplicate. A new explanation is created only when the reasoning, policy versions, or template version materially changes. |
| Business Owner | Explain Demand (CA-D-009) |
| Produced By | AB-EX-001 Record Demand Explanation |
| Consumed By | Planners (understanding why a forecast or decision was made), AI Copilot (providing natural-language explanations), Audit (traceability and reproducibility), Learn From Demand (explanation quality feedback). |
| Lifecycle Expectation | Created as an immutable record. No state transitions. Retained permanently. |
| Retention Expectation | All explanations retained permanently for audit and continuous improvement. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Record |
| Behavior | Immutable |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Demand Explanation Identifier | Yes | Yes |
| Mandatory | Explained Artifact Type (Forecast, Decision, Condition, Classification, Priority, Assessment) | Yes | Yes |
| Mandatory | Explained Artifact Identifier (reference to the artifact being explained) | Yes | Yes |
| Mandatory | Structured Reasoning Graph (the canonical enterprise representation of the reasoning) | Yes | Yes |
| Mandatory | Natural Language Explanation (human-readable rendering derived from the Structured Reasoning Graph) | Yes | Yes |
| Mandatory | Source Artifact References (identifiers with versions of every contributing rule, decision, policy, model, data source, and template used) | Yes | Yes |
| Mandatory | Explanation Generation Timestamp | Yes | Yes |
| Traceability | Template Version Reference (identifier of the template used to structure the explanation) | Yes | Yes |

**Structured Reasoning Graph**

The Structured Reasoning Graph is the canonical enterprise representation of the reasoning behind an artifact. It consists of:

- **Nodes:** Each node represents a reasoning element — a Rule evaluated, a Decision made, a Policy applied, a Data input, a Planner action, a Statistical contribution, or a Model output.
- **Edges:** Typed relationships between nodes — Influenced, Determined, Overrode, Supported, Contradicted, Triggered.
- **Node Provenance (mandatory on each node):** The kind of reasoning the node represents — Rule-Based, Statistical, Optimization, Machine Learning, Hybrid, or Planner Judgment. This is governance metadata identifying the nature of each reasoning contribution.
- **Historical Version References (mandatory on each node):** Every contributing rule, policy, model, and template is identified by the version that was in effect when the explained artifact was produced, not the current version.

**Lifecycle**

Demand Explanation has no state transitions. It is created as an immutable record and retained permanently.

**Architectural Note:** The current model explains individual artifacts. Future extensions may support hierarchical explanations (e.g., explaining a regional forecast composed of many SKU-level forecasts) through an Explanation Context concept. This is not implemented in the current version.

**Invariants:**
- The Structured Reasoning Graph shall reference the versions of every contributing rule, decision, policy, model, and template that were in effect when the explained artifact was produced (BR-D-092).
- The Structured Reasoning Graph shall be deterministic: identical inputs (explained artifact, evidence, policy versions, template version) shall produce an identical reasoning graph (BR-D-093).
- An explanation, once created, shall never be modified (BR-D-094).

**Business Operations:** Record (create explanation on demand or on event; return existing explanation if reasoning is unchanged).

**Traceability:** Business Owner: CA-D-009. Produced By: AB-EX-001. Referenced by: FS-D-021. Governed by BR-D-092–094.



### 4.5.12 Demand Learning — SE-D-042

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide an authoritative, immutable enterprise record of what the enterprise has concluded about the performance and behaviour of its demand intelligence capabilities, based on systematic analysis of evidence available at the time. |
| Definition | An immutable enterprise record capturing a conclusion the enterprise has reached about its demand intelligence — a performance pattern, causal relationship, behavioural insight, or improvement opportunity — supported by evidence from completed analyses and evaluations. The learning records what the enterprise concluded from the evidence available at the time. Subsequent evidence may produce new learnings without modifying historical records. |
| Identity | Demand Learning Identifier, globally unique, assigned at creation. Immutable. |
| Business Owner | Learn From Demand (CA-D-010) |
| Produced By | AB-LR-001 Record Demand Learning |
| Consumed By | Forecast Demand, Detect Demand Exceptions, Segment Demand, Explain Demand, Planning Governance. Learnings may be consumed by any capability seeking to improve its performance based on discovered enterprise knowledge. |
| Lifecycle Expectation | Created as an immutable record. No state transitions. Retained permanently. |
| Retention Expectation | All learnings retained permanently for audit and learning effectiveness measurement. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Record |
| Behavior | Immutable |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Demand Learning Identifier | Yes | Yes |
| Mandatory | Learning Type (as defined by the Learning Analysis Policy — e.g., Performance Pattern, Causal Relationship, Anomaly Cluster, Behavioural Insight, Improvement Opportunity) | Yes | Yes |
| Mandatory | Learning Statement (precise, business-language description of what the enterprise has concluded) | Yes | Yes |
| Mandatory | Supporting Evidence (references to quality assessments, condition histories, explanations, classifications, and other artifacts across all demand capabilities that support the learning) | Yes | Yes |
| Mandatory | Evidence Strength (as defined by the Learning Analysis Policy — the enterprise’s assessment of how strongly the body of evidence supports the learning) | Yes | Yes |
| Mandatory | Learning Timestamp | Yes | Yes |
| Traceability | Source Analysis Reference (identifier of the specific analysis run that produced this learning) | Yes | Yes |

**Historical Validity**

A Demand Learning records what the enterprise concluded from the evidence available at the time. It is a permanent historical record. If subsequent evidence strengthens, weakens, or contradicts the learning, a new Demand Learning is created with a new identifier. The original learning is never modified. This preserves the enterprise’s ability to audit what was known and when.

**Cross-Capability Learning**

Learnings may arise from relationships across any Demand Intelligence semantic objects — not only within a single capability. Examples include relationships between Priority and Forecast Accuracy, between Segment and Override Behaviour, between Classification and Forecast Stability, or between Planning Conditions and downstream inventory outcomes. The Learning Analysis Policy governs which domains and relationships are eligible for analysis.

**Lifecycle**

Demand Learning has no state transitions. It is created as an immutable record and retained permanently. Whether the learning leads to recommendations, actions, or policy changes is external to this capability.

**Invariants:**
- A learning shall be supported by evidence from at least one completed analysis or evaluation (BR-D-096).
- A learning, once created, shall never be modified (BR-D-097).

**Business Operations:** Record (create learning based on analysis of evidence).

**Traceability:** Business Owner: CA-D-010. Produced By: AB-LR-001. Referenced by: FS-D-022. Governed by BR-D-096–097.

---

## 4.6 Entities

### 4.6.1 Operational Demand — SE-D-011

**Lifecycle Owner:** Enterprise Demand Picture (SE-D-003).  
**Ontology Classification:** Nature: State, Behavior: Versioned.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | (Local to the EDP version) | Yes | Yes |
| Mandatory | Planning Period (Value Object) | Yes | Yes |
| Mandatory | Accumulated Quantity (Value Object) | Yes | Yes |
| Traceability | Last Updated Transaction Time | Yes | Yes |

Operational Demand is updated only when a new Accepted Demand Observation is incorporated. Never modified by planning adjustments.

**Traceability:** Lifecycle Owner: SE-D-003. Referenced by: FS-D-004, FS-D-005.



### 4.6.2 Planning Demand — SE-D-010

**Lifecycle Owner:** Enterprise Demand Picture (SE-D-003).  
**Ontology Classification:** Nature: State, Behavior: Versioned, Derived, Authoritative.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Definition | The enterprise-approved quantity representing future demand after incorporating all approved planning adjustments and planner overrides. |
| Business Owner | Understand Demand |
| Produced By | BA-D-001 Calculate Planning Demand |
| Consumed By | Supply Intelligence, Production Planning, Inventory Planning. |
| Lifecycle Expectation | Immutable within an Enterprise Demand Picture version. |
| Retention Expectation | Retained permanently within the EDP version. |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | (Local to the EDP version) | Yes | Yes |
| Mandatory | Planning Period (Value Object) | Yes | Yes |
| Mandatory | Final Planning Demand Quantity (Value Object) | Yes | Yes |
| Derived | Operational Demand Quantity | Yes | Yes |
| Derived | Adjustment Quantity | No | Yes |
| Derived | Override Quantity | No | Yes |
| Traceability | Calculation Method (Algorithm version) | Yes | Yes |
| Traceability | Calculation Timestamp (Transaction Time) | Yes | Yes |

**Traceability:** Lifecycle Owner: SE-D-003. Produced By: BA-D-001. Referenced by: FS-D-005, FS-D-006.



### 4.6.3 Forecast — SE-D-025

**Lifecycle Owner:** Forecast Publication (SE-D-029).  
**Ontology Classification:** Nature: Projection, Behavior: Versioned, Derived.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | (Local to the Publication) | Yes | Yes |
| Mandatory | SKU, StockingPoint | Yes | Yes |
| Mandatory | Time Bucket | Yes | Yes |
| Mandatory | Mean Quantity | Yes | Yes |
| Mandatory | Prediction Interval (lower, upper, confidence level) | Yes | Yes |
| Mandatory | Confidence Score | Yes | Yes |
| Derived | Override Value (if overridden) | No | No |
| Traceability | Model ID | Yes | Yes |
| Traceability | Generation Timestamp | Yes | Yes |

**Traceability:** Lifecycle Owner: SE-D-029. Referenced by: FS-D-009, FS-D-010, FS-D-011.



### 4.6.4 Forecast Assumption — SE-D-027

**Lifecycle Owner:** Forecast Publication (SE-D-029).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

**Lifecycle:** Declared → Validated → Approved → Withdrawn.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Assumption Identifier | Yes | Yes |
| Mandatory | Statement (e.g., "Promotion runs Weeks 30-32", "Price decreases 8%") | Yes | Yes |
| Mandatory | Declared By (role) | Yes | Yes |
| Mandatory | Lifecycle State | Yes | No |
| Optional | Linked Driver Reference (Reference to SE-DI-060) | No | Yes |
| Traceability | Timestamp | Yes | Yes |

Assumptions are Forecast-owned interpretations of business context. The underlying events (promotions, pricing changes) are Reference Objects from other capabilities; Forecast Demand creates Assumptions to record how those events affect the forecast.

**Traceability:** Lifecycle Owner: SE-D-029. Referenced by: FS-D-012.



### 4.6.5 Forecast Override — SE-D-028

**Lifecycle Owner:** Forecast Publication (SE-D-029).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | (Local — SKU, StockingPoint, time bucket) | Yes | Yes |
| Mandatory | Original System Value | Yes | Yes |
| Mandatory | Override Value | Yes | Yes |
| Mandatory | Justification | Yes | Yes |
| Mandatory | Planner Identity | Yes | Yes |
| Traceability | Decision Identifier (DE-D-023) | Yes | Yes |
| Traceability | Override Timestamp | Yes | Yes |

Original system forecast value preserved unchanged. Override value becomes the planning forecast for the specified SKU-StockingPoint-time bucket.

**Traceability:** Lifecycle Owner: SE-D-029. Referenced by: FS-D-010.


### 4.6.6 Forecast Hierarchy — SE-D-031

**Lifecycle Owner:** Forecast Demand (CA-D-002) — Reference Object consumed during reconciliation.  
**Ontology Classification:** Nature: Structure, Behavior: Versioned.

**Definition:** Defines the aggregation levels (SKU, SKU Family, Category, Brand, Region, Channel, Company) and their parent-child relationships used for forecast reconciliation.

**Traceability:** Business Owner: CA-D-002. Referenced by: FS-D-013.



### 4.6.7 State Change Event — Entity within SE-D-035

**Lifecycle Owner:** Demand Behaviour Assessment (SE-D-035).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Event Identifier (unique within the assessment) | Yes | Yes |
| Mandatory | Timestamp (Transaction Time) | Yes | Yes |
| Mandatory | Previous State | Yes | Yes |
| Mandatory | New State | Yes | Yes |
| Mandatory | Deviation Magnitude (σ) | Yes | Yes |
| Mandatory | Deviation Direction (Increase, Decrease) | Yes | Yes |
| Mandatory | Confidence Score | Yes | Yes |
| Mandatory | Corroborating Signal Count | Yes | Yes |
| Mandatory | Baseline Reference (identifier of the baseline used) | Yes | Yes |
| Traceability | Triggering Signal Reference (reference to the Demand Signal that triggered the change, if retained) | No | Yes |



### 4.6.8 Assignment Change Event — Entity within SE-D-036

**Lifecycle Owner:** Planning Classification Assignment (SE-D-036).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Event Identifier (unique within the assignment) | Yes | Yes |
| Mandatory | Timestamp (Transaction Time) | Yes | Yes |
| Mandatory | Previous Classification | Yes | Yes |
| Mandatory | New Classification | Yes | Yes |
| Mandatory | Reason (Scheduled Reclassification, Demand Pattern Change, Policy Change, Planner Override) | Yes | Yes |
| Optional | Override Justification (if Reason = Planner Override) | No | Yes |
| Mandatory | Classification Confidence | Yes | Yes |
| Traceability | Policy Version Reference (identifier of the Segmentation Policy used) | Yes | Yes |



### 4.6.9 Behaviour Change Event — Entity within SE-D-037

**Lifecycle Owner:** Demand Behaviour Assignment (SE-D-037).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Event Identifier (unique within the assignment) | Yes | Yes |
| Mandatory | Timestamp (Transaction Time) | Yes | Yes |
| Mandatory | Previous Classification | Yes | Yes |
| Mandatory | New Classification | Yes | Yes |
| Mandatory | Reason (Scheduled Re-evaluation, Policy Change, Demand Pattern Shift, Planner Override) | Yes | Yes |
| Mandatory | Classification Confidence | Yes | Yes |
| Mandatory | Evidence Summary (business-level statistical findings supporting the classification) | Yes | Yes |
| Optional | Override Justification (if Reason = Planner Override) | No | Yes |
| Traceability | Policy Version Reference (identifier of the Classification Policy used) | Yes | Yes |



### 4.6.10 Priority Change Event — Entity within SE-D-038

**Lifecycle Owner:** Planning Priority Assignment (SE-D-038).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Event Identifier (unique within the assignment) | Yes | Yes |
| Mandatory | Timestamp (Transaction Time) | Yes | Yes |
| Mandatory | Previous Priority | Yes | Yes |
| Mandatory | New Priority | Yes | Yes |
| Mandatory | Previous Score | Yes | Yes |
| Mandatory | New Score | Yes | Yes |
| Mandatory | Decision Rationale (business-language explanation of why the priority was assigned) | Yes | Yes |
| Mandatory | Business Validity (the business conditions under which this priority applies) | Yes | Yes |
| Mandatory | Reason (Scheduled Re-evaluation, Policy Change, Segment Change, Behaviour Change, Planner Override) | Yes | Yes |
| Optional | Override Justification (if Reason = Planner Override) | No | Yes |
| Traceability | Policy Version Reference (identifier of the Prioritization Policy used) | Yes | Yes |



### 4.6.11 Condition Change Event — Entity within SE-D-040

**Lifecycle Owner:** Demand Planning Condition (SE-D-040).  
**Ontology Classification:** Nature: Record, Behavior: Immutable.

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Event Identifier (unique within the condition) | Yes | Yes |
| Mandatory | Timestamp (Transaction Time) | Yes | Yes |
| Mandatory | Event Type (Detected, Severity Changed, Resolved) | Yes | Yes |
| Mandatory | Current State After Event | Yes | Yes |
| Mandatory | Evidence (statistical or data evidence supporting the event) | Yes | Yes |
| Optional | Previous Severity (if Event Type = Severity Changed) | No | Yes |
| Optional | New Severity (if Event Type = Severity Changed) | No | Yes |
| Traceability | Policy Version Reference (identifier of the Exception Detection Policy used) | Yes | Yes |

---

## 4.7 Value Objects

| Object | Attributes | Business Meaning |
|--------|-----------|------------------|
| Quantity | Numeric value, unit of measure | A measured amount of demand, supply, or material. |
| Planning Period | Start date, end date, time bucket type | A defined interval for planning aggregation. |
| Prediction Interval | Lower bound, upper bound, confidence level | Quantified uncertainty around a forecast. |
| Customer Identity | Customer ID (Reference wrapper) | Uniquely identifies a customer. |
| SKU Identity | SKU ID (Reference wrapper) | Uniquely identifies a product. |
| StockingPoint Identity | StockingPoint ID (Reference wrapper) | Uniquely identifies a location. |
| Forecast Configuration | Set of parameters: forecast horizon, freeze horizon, override deviation limit, confidence thresholds, reconciliation method, review cadences. Owned by Forecast Demand (CA-D-002). | Configuration that governs how a forecast cycle behaves. |
| Forecast Coverage | List of mandatory SKU-StockingPoint combinations (and hierarchy levels) that must be forecast. May vary per cycle. Owned by Forecast Demand (CA-D-002). | The scope of the forecasting obligation. |

---

## 4.8 Reference Objects

| ID | Object | Owning Domain | Consistency Expectation |
|----|--------|---------------|--------------------------|
| SE-DI-030 | Customer | Master Data Management | Historical observations reference Customer as at Observation Time. |
| SE-DI-040 | SKU | Master Data Management | Historical observations reference SKU as at Observation Time. |
| SE-DI-041 | StockingPoint | Master Data Management | Historical observations reference StockingPoint as at Observation Time. |
| SE-DI-050 | Time Bucket | Master Data Management | Planning calendar configuration. |
| SE-DI-051 | Planning Calendar | Master Data Management | Enterprise calendar definition. |
| SE-DI-060 | Demand Driver | Various (Marketing, Commercial, Master Data, external) | An enterprise event or condition that influences future demand. Forecast Demand consumes Drivers but does not own them. |
| SE-DI-061 | SKU Lifecycle Stage | SKU Master / Classify Demand | The phase of a product's market life (Introduction, Growth, Maturity, Decline, End-of-Life). Forecast Demand consumes this to vary forecasting policies. |
| SE-DI-062 | Demand Baseline | Understand Demand | A rolling statistical model of expected demand for a SKU-StockingPoint, derived from the published Enterprise Demand Picture and historical data. Sense Demand consumes this for signal evaluation. |
| SE-DI-063 | Demand History | Understand Demand | Authoritative cleansed historical demand observations for a SKU-StockingPoint or planning scope, used as input for forecasting and segmentation. |
| SE-DI-064 | Segmentation Policy | Planning Governance | The enterprise’s governed rules for all active classification types, their thresholds, triggers, and evidence requirements. Segment Demand consumes this policy. |
| SE-DI-065 | Classification Policy | Planning Governance | The enterprise’s governed rules for all active behaviour dimensions, their recognised classifications, evidence requirements, confidence thresholds, and re-evaluation triggers. Classify Demand consumes this policy. |
| SE-DI-066 | Prioritization Policy | Planning Governance | The enterprise’s governed rules for computing planning priority, including scoring methodology, priority level thresholds, and business validity rules. Prioritize Demand consumes this policy. |
| SE-DI-067 | Forecast Measurement Policy | Planning Governance | The enterprise’s governed rules for forecast quality measurement: mandatory and optional metrics with definitions and formulas, evaluation cadence, publication criteria, completeness thresholds, minimum sample size, Overall Quality Score derivation (if used), and policy versioning. |
| SE-DI-068 | Forecast Publication (for evaluation) | Forecast Demand | The published forecast being evaluated. |
| SE-DI-069 | Planner Override Records | Forecast Demand | Historical planner overrides for override effectiveness computation. |
| SE-DI-070 | Exception Detection Policy | Planning Governance | The enterprise’s governed rules for all active exception detection types, their thresholds, triggers, and evidence requirements. Detect Demand Exceptions consumes this policy. |
| SE-DI-071 | Explanation Template Catalog | Planning Governance | The enterprise’s governed catalog of required reasoning content for each explained artifact type. Defines what reasoning elements must be included and their completeness criteria, without governing language, wording, or formatting. |
| SE-DI-072 | Learning Analysis Policy | Planning Governance | The enterprise’s governed rules for learning discovery: analysis cadence, evidence sufficiency thresholds, Evidence Strength criteria, governed learning scope (which domains and cross-capability relationships are eligible for analysis), and the taxonomy of recognized Learning Types. New Learning Types may be added to the taxonomy by policy change without modifying the semantic model. |

Demand Intelligence never creates, modifies, or defines the lifecycle of Reference Objects (BR-D-058).

---

## 4.9 Knowledge Artifacts (Provisional)

| ID | Artifact | Definition |
|----|----------|------------|
| KA-D-001 | Forecast Confidence Index | Quantitative measure of forecast reliability. |
| KA-D-002 | Explainability Score | Degree to which a forecast or decision is traceable. |
| KA-D-003 | Demand Pattern Evidence | Statistical evidence supporting pattern classification. |

Full definition deferred to Knowledge Intelligence domain.

---

# Chapter 5 — Capability Model

## 5.1 Understand Demand — CA-D-001

**Business Intent:** Establish and maintain the enterprise's current understanding of actual demand by transforming operational business observations into trusted planning information.

**Owned Semantic Objects:** SE-D-001, SE-D-002, SE-D-003, SE-D-010, SE-D-011.

**Business Guarantees:**
- Exactly one Published Enterprise Demand Picture exists for each Planning Scope at any moment.
- Every Accepted Demand Observation is reflected in the Published Enterprise Demand Picture for its Planning Scope.
- No Rejected or Quarantined observation contributes to Planning Demand.
- Every Planning Demand quantity is explainable back to its source observations, adjustments, and overrides.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-001 | Receive Business Observation | BW-D-001 | FS-D-001 |
| CR-D-002 | Evaluate Business Observation | BW-D-002 | FS-D-002 |
| CR-D-003 | Determine Planning Scope | BW-D-003 | FS-D-003 |
| CR-D-004 | Maintain Enterprise Demand Picture | BW-D-004 | FS-D-004 |
| CR-D-005 | Calculate Planning Demand | BW-D-005 | FS-D-005 |
| CR-D-006 | Publish Enterprise Demand Picture | BW-D-006 | FS-D-006 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-001 | Enterprise Demand Picture Published: Planning Scope Identifier, Version, Publication Time, Superseded Version Identifier (if any), Affected Planning Time Buckets | At-least-once | Per Planning Scope | Near-real-time |
| BN-D-002 | Demand Observation Quarantined: Observation Identifier, Quarantine Reason, Decision Identifier, Decision Timestamp | At-least-once | Per observation | Near-real-time |
| BN-D-003 | Demand Observation Rejected: Observation Identifier, Rejection Reason, Decision Identifier, Decision Timestamp | At-least-once | Per observation | Near-real-time |
| BN-D-004 | Enterprise Demand Recalculation Failed: Planning Scope Identifier, Failure Reason | At-least-once | Per Planning Scope | Near-real-time |
| BN-D-005 | Demand Observation Received: Observation Identifier, Observation Type, Source System, Observation Time | At-least-once | Per observation | Near-real-time |
| BN-D-006 | Demand Observation Accepted: Observation Identifier, Decision Identifier, Decision Confidence | At-least-once | Per observation | Near-real-time |
| BN-D-007 | Demand Observation Accepted With Warning: Observation Identifier, Warning Code, Warning Description | At-least-once | Per observation | Near-real-time |

### Business Notifications Consumed

| ID | Consumed Notification | Business Behaviour | Guarantee | Invokes |
|----|----------------------|-------------------|-----------|---------|
| BC-D-001 | Planning Adjustment Approved | Recalculate Enterprise Demand for affected Planning Scopes. | Only approved, effective adjustments applied. | FS-D-004 → FS-D-005 → FS-D-006 |
| BC-D-002 | Planner Override Approved | Recalculate Enterprise Demand for affected Planning Scopes. | Only approved, effective overrides applied. | FS-D-004 → FS-D-005 → FS-D-006 |
| BC-D-003 | Enterprise Reference Information Changed | Determine if existing EDPs are invalidated. | Only affected Planning Scopes recalculated. | FS-D-004 (conditional) |
| BC-D-004 | BN-D-011 Forecast Published | Create forecast-derived demand observations for each SKU-StockingPoint-time bucket. | Exactly one observation per forecast record. | FS-D-001 |

**Traceability:** Business Owner: CA-D-001. Publishes: BN-D-001 through BN-D-007. Consumes: BC-D-001 through BC-D-004. Realises: BO-DI-001, BO-DI-003.

---

## 5.2 Forecast Demand — CA-D-002

**Business Intent:** Maintain the enterprise's authoritative understanding of future demand by combining statistical prediction, business knowledge, assumptions, hierarchy reconciliation, and governance into a trusted planning forecast, published as a Forecast Publication.

**Owned Semantic Objects:** SE-D-029 (Forecast Publication), SE-D-025 (Forecast), SE-D-027 (Forecast Assumption), SE-D-028 (Forecast Override), SE-D-031 (Forecast Hierarchy).

**Referenced Value Objects:** Forecast Configuration, Forecast Coverage.

**Referenced Reference Objects:** SE-DI-060 (Demand Driver), SE-DI-061 (SKU Lifecycle Stage).

**Business Guarantees:**
- Exactly one Published Forecast Publication exists for a given planning scope at any moment.
- Every SKU-StockingPoint combination within forecast coverage receives a forecast or a documented reason why it cannot be forecast.
- Every forecast is accompanied by the assumptions and drivers that influenced it.
- All forecast lines within a publication are hierarchically reconciled before publication.
- All planner overrides are fully traceable to the individual and the business reason.
- The historical record of every publication is permanently preserved.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-007 | Initiate Forecast Cycle | BW-D-007 | FS-D-007 |
| CR-D-012 | Prepare Forecast Context | BW-D-012 | FS-D-012 |
| CR-D-008 | Select Forecasting Model | BW-D-008 | FS-D-008 |
| CR-D-009 | Generate Baseline Forecasts | BW-D-009 | FS-D-009 |
| CR-D-013 | Reconcile Forecast Hierarchy | BW-D-013 | FS-D-013 |
| CR-D-010 | Record Forecast Override | BW-D-010 | FS-D-010 |
| CR-D-011 | Publish Forecast Publication | BW-D-011 | FS-D-011 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-010 | Forecast Cycle Initialised: Cycle ID, Reason, Horizon, Timestamp | At-least-once | Per cycle | Near-real-time |
| BN-D-011 | Forecast Published: Publication ID, Version, Planning Scope Identifier(s), Horizon, Confidence Index, Champion Model, Publication Time | At-least-once | Per publication | Near-real-time |
| BN-D-012 | Forecast Override Recorded: Publication ID, SKU, StockingPoint, Bucket, Original, Override, Planner, Justification | At-least-once | Per override | Near-real-time |
| BN-D-013 | Forecast Publication Failed: Cycle ID, Reason | At-least-once | Per cycle | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-005 | BN-D-001 Enterprise Demand Picture Published | Update training data with latest cleansed history. | (data refresh only) |
| BC-D-006 | BN-K-xxx Calendar Exception Raised | Adjust forecast calendar; may trigger out-of-cycle forecast. | FS-D-007 (conditional) |
| BC-D-007 | Various external driver notifications (Promotion, Pricing, etc.) | Capture relevant drivers as Reference Objects; create Assumptions during context preparation. | FS-D-012 |

**Traceability:** Business Owner: CA-D-002. Publishes BN-D-010–013. Consumes BC-D-005–007. Realises BO-DI-001, BO-DI-002, BO-DI-005, BO-DI-006.

---

## 5.3 Sense Demand — CA-D-003

**Business Intent:** Continuously maintain the enterprise’s understanding of current demand behaviour for every monitored SKU-StockingPoint, detecting meaningful changes from expected patterns and providing real-time situational awareness that triggers downstream planning actions.

**Owned Semantic Objects:** SE-D-035 (Demand Behaviour Assessment), State Change Event (Entity within SE-D-035).

**Referenced Reference Objects:** SE-DI-040 (SKU), SE-DI-041 (StockingPoint), SE-DI-062 (Demand Baseline).

**Business Guarantees:**
- Every monitored SKU-StockingPoint has a continuously maintained Current State reflecting the latest demand behaviour.
- A state change is published within the enterprise detection latency target after the corroborating signals are evaluated.
- Critical state changes automatically trigger evaluation for an out-of-cycle forecast refresh in Forecast Demand.
- The complete history of state changes is permanently preserved for audit and for evaluating detection accuracy.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-014 | Update Demand Behaviour Assessment | BW-D-014 | FS-D-014 |
| CR-D-015 | Trigger Downstream Actions | BW-D-015 | FS-D-015 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-015 | Demand Behaviour Changed: Assessment Identity (SKU, StockingPoint), Previous State, New State, Deviation (σ), Direction, Confidence, Timestamp | At-least-once | Per assessment | Near-real-time |
| BN-D-016 | Critical Demand Behaviour Requires Action: same as BN-D-015 plus Recommended Action (Forecast Refresh) | At-least-once | Per assessment | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-008 | BN-D-001 Enterprise Demand Picture Published | Refresh the Demand Baseline with the latest cleansed demand history. | (baseline refresh) |
| BC-D-009 | BN-D-011 Forecast Published | Optionally update baseline to incorporate the latest forecast (if configured). | (baseline refresh) |

**Traceability:** Business Owner: CA-D-003. Publishes BN-D-015–016. Consumes BC-D-008–009. Realises BO-DI-001, BO-DI-003.
## 5.4 Segment Demand — CA-D-004

**Business Intent:** Maintain the enterprise’s continuously current planning classifications for every planning entity, enabling all downstream capabilities to vary their behaviour based on the characteristics of each entity.

**Owned Semantic Objects:** SE-D-036 (Planning Classification Assignment), Assignment Change Event (Entity within SE-D-036).

**Referenced Reference Objects:** SE-DI-040 (SKU), SE-DI-030 (Customer), SE-DI-063 (Demand History), SE-DI-064 (Segmentation Policy).

**Business Guarantees:**
- Every active planning entity has a continuously maintained current classification for each active classification type.
- Each classification type is updated independently when its triggering conditions are met.
- Classification history is permanently preserved per entity per type.
- The set of active classification types is governed by the Segmentation Policy, not hard-coded.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-016 | Classify Planning Entity | BW-D-016 | FS-D-016 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-017 | Planning Classification Changed: Entity Type, Entity Identifier, Classification Type, Previous Classification, New Classification, Reason, Confidence | At-least-once | Per assignment | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-010 | BN-D-001 Enterprise Demand Picture Published | Update demand history; may trigger re-evaluation for volume/variability types. | FS-D-016 (conditional) |
| BC-D-011 | BN-D-015 Demand Behaviour Changed | If a sustained pattern change is detected, trigger re-evaluation for the affected classification types. | FS-D-016 (conditional) |
| BC-D-012 | Segmentation Policy Updated (from Planning Governance) | Reclassify all affected entities for the changed classification types. | FS-D-016 (conditional) |

**Traceability:** Business Owner: CA-D-004. Publishes BN-D-017. Consumes BC-D-010–012. Realises BO-DI-001, BO-DI-002, BO-DI-005.
## 5.5 Classify Demand — CA-D-005

**Business Intent:** Maintain the enterprise’s authoritative behavioural classifications for every planning entity, enabling downstream capabilities to select appropriate forecasting models, set detection thresholds, and focus planner attention based on how demand actually behaves.

**Owned Semantic Objects:** SE-D-037 (Demand Behaviour Assignment), Behaviour Change Event (Entity within SE-D-037).

**Referenced Reference Objects:** SE-DI-040 (SKU), SE-DI-041 (StockingPoint), SE-DI-063 (Demand History), SE-DI-065 (Classification Policy).

**Business Guarantees:**
- Every active planning entity has a continuously maintained current classification for each active behaviour dimension.
- Each behaviour dimension is updated independently when its triggering conditions are met.
- Every classification includes confidence and a business-level evidence summary.
- Classification history is permanently preserved per entity per dimension.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-017 | Classify Demand Behaviour | BW-D-017 | FS-D-017 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-019 | Demand Behaviour Classification Changed: Entity Type, Entity Identifier, Behaviour Dimension, Previous Classification, New Classification, Confidence, Evidence Summary | At-least-once | Per assignment | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-013 | BN-D-001 Enterprise Demand Picture Published | Update demand history; may trigger re-evaluation for statistical dimensions. | FS-D-017 (conditional) |
| BC-D-014 | BN-D-015 Demand Behaviour Changed | If a sustained behavioural shift is detected, trigger re-evaluation for the affected dimensions. | FS-D-017 (conditional) |
| BC-D-015 | Classification Policy Updated (from Planning Governance) | Reclassify all affected entities for the changed behaviour dimensions. | FS-D-017 (conditional) |

**Traceability:** Business Owner: CA-D-005. Publishes BN-D-019. Consumes BC-D-013–015. Realises BO-DI-001, BO-DI-002, BO-DI-005.
## 5.6 Prioritize Demand — CA-D-006

**Business Intent:** Maintain the enterprise’s continuously current assessment of planning importance for every planning entity, directing planner attention, exception handling, and allocation decisions to the most impactful items.

**Owned Semantic Objects:** SE-D-038 (Planning Priority Assignment), Priority Change Event (Entity within SE-D-038).

**Referenced Reference Objects:** SE-DI-040 (SKU), SE-DI-030 (Customer), SE-DI-066 (Prioritization Policy). Also reads segment data from SE-DI-064 and behaviour data from SE-D-037.

**Business Guarantees:**
- Every active planning entity has a continuously maintained current priority.
- Priority is derived from the current Prioritization Policy and available business evidence.
- Every priority assignment includes a business-language decision rationale explaining why the priority was assigned, and the business conditions under which it applies.
- Priority history is permanently preserved per entity.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-018 | Prioritize Planning Entity | BW-D-018 | FS-D-018 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-020 | Planning Priority Changed: Entity Type, Entity Identifier(s), Previous Priority, New Priority, Decision Rationale, Business Validity | At-least-once | Per assignment | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-016 | BN-D-001 Enterprise Demand Picture Published | May trigger re-evaluation if volume contribution has changed significantly. | FS-D-018 (conditional) |
| BC-D-017 | BN-D-017 Planning Classification Changed | If segment classification has changed, re-evaluate priority for the affected entity. | FS-D-018 (conditional) |
| BC-D-018 | BN-D-019 Demand Behaviour Classification Changed | If behaviour classification affects risk assessment, re-evaluate priority. | FS-D-018 (conditional) |
| BC-D-019 | Prioritization Policy Updated (from Planning Governance) | Re-evaluate priority for all affected entities. | FS-D-018 (conditional) |

**Traceability:** Business Owner: CA-D-006. Publishes BN-D-020. Consumes BC-D-016–019. Realises BO-DI-002, BO-DI-004, BO-DI-005.
## 5.7 Evaluate Demand Quality — CA-D-007

**Business Intent:** Publish authoritative, periodic enterprise assessments of forecast quality, enabling the enterprise to measure and continuously improve forecasting performance.

**Owned Semantic Objects:** SE-D-039 (Forecast Quality Assessment).

**Referenced Reference Objects:** SE-DI-067 (Forecast Measurement Policy), SE-DI-068 (Forecast Publication), SE-DI-069 (Planner Override Records), SE-DI-063 (Demand History).

**Business Guarantees:**
- A Forecast Quality Assessment is published for each Planning Scope and Evaluation Period, containing the complete set of metrics defined by the Forecast Measurement Policy.
- Every published assessment is immutable and permanently retained.
- Supersession occurs only for a new version of the same Planning Scope and Evaluation Period.
- Source references and policy version are recorded for full traceability.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-019 | Evaluate Forecast Quality | BW-D-019 | FS-D-019 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-021 | Forecast Quality Assessment Published: Assessment Identifier, Planning Scope, Evaluation Period, Version, Key Metrics Summary, Overall Quality Score (if policy-defined) | At-least-once | Per assessment | Batch |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-020 | BN-D-011 Forecast Published | Initiates quality evaluation when actuals become available. | FS-D-019 (scheduled) |
| BC-D-021 | BN-D-001 Enterprise Demand Picture Published | Provides actual demand data for accuracy computation. | FS-D-019 (data input) |

**Traceability:** Business Owner: CA-D-007. Publishes BN-D-021. Consumes BC-D-020–021. Realises BO-DI-001, BO-DI-002, BO-DI-006.
## 5.8 Detect Demand Exceptions — CA-D-008

**Business Intent:** Continuously monitor the enterprise demand picture to recognize demand planning conditions that require attention, maintaining an authoritative record of their existence until the underlying data returns to acceptable bounds.

**Owned Semantic Objects:** SE-D-040 (Demand Planning Condition), Condition Change Event (Entity within SE-D-040).

**Referenced Reference Objects:** SE-DI-070 (Exception Detection Policy), SE-DI-068 (Forecast Publication), SE-DI-063 (Demand History), SE-DI-069 (Planner Override Records), SE-D-039 (Forecast Quality Assessment), SE-D-037 (Demand Behaviour Assignment).

**Business Guarantees:**
- Every demand planning condition that meets the detection thresholds defined in the Exception Detection Policy is recognized and recorded.
- A condition persists as Active until the underlying data returns to within governed thresholds, at which point it is Resolved and becomes terminal.
- Recurrence of the same condition type for the same planning entity creates a new condition instance with a new identifier.
- Condition history is permanently preserved for audit and detection effectiveness analysis.
- The boundary with Sense Demand is explicit: Sense Demand owns changes in demand behaviour; Detect Demand Exceptions owns conditions where planning requires intervention.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-020 | Detect Demand Planning Conditions | BW-D-020 | FS-D-020 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-022 | Demand Planning Condition Detected: Condition Identifier, Planning Entity, Condition Type, Severity, Detection Evidence, Detection Timestamp | At-least-once | Per condition | Near-real-time |
| BN-D-023 | Demand Planning Condition Resolved: Condition Identifier, Planning Entity, Condition Type, Resolution Evidence, Resolution Timestamp | At-least-once | Per condition | Near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-022 | BN-D-011 Forecast Published | Evaluate forecast quality metrics against detection thresholds. | FS-D-020 (conditional) |
| BC-D-023 | BN-D-001 Enterprise Demand Picture Published | Evaluate data completeness and quality against detection thresholds. | FS-D-020 (conditional) |
| BC-D-024 | BN-D-015 Demand Behaviour Changed | If a sustained anomalous behaviour is detected, evaluate against planning condition thresholds. | FS-D-020 (conditional) |
| BC-D-025 | BN-D-021 Forecast Quality Assessment Published | Evaluate quality metrics against degradation thresholds. | FS-D-020 (conditional) |

**Traceability:** Business Owner: CA-D-008. Publishes BN-D-022–023. Consumes BC-D-022–025. Realises BO-DI-001, BO-DI-003, BO-DI-004.
## 5.9 Explain Demand — CA-D-009

**Business Intent:** Record immutable, traceable, deterministic enterprise explanations of the reasoning behind demand intelligence outputs, enabling planners, auditors, and AI systems to understand why forecasts, decisions, conditions, classifications, and priorities exist as they do.

**Owned Semantic Objects:** SE-D-041 (Demand Explanation).

**Referenced Reference Objects:** SE-DI-071 (Explanation Template Catalog).

**Business Guarantees:**
- Every recorded explanation contains a Structured Reasoning Graph — the canonical, deterministic enterprise representation of the reasoning.
- The reasoning graph carries provenance on every node and references historical versions of every contributing rule, decision, policy, model, and template.
- Explanations are immutable once created and permanently retained.
- Identical reasoning requests return the existing explanation rather than creating duplicates.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-021 | Record Demand Explanation | BW-D-021 | FS-D-021 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-024 | Demand Explanation Recorded: Explanation Identifier, Explained Artifact Type, Explained Artifact Identifier | At-least-once | Per explanation | On-demand or near-real-time |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-026 | BN-D-011 Forecast Published | Optionally record an automatic explanation for the published forecast. | FS-D-021 (conditional) |
| BC-D-027 | BN-D-022 Demand Planning Condition Detected | Optionally record an automatic explanation for the detected condition. | FS-D-021 (conditional) |

**Traceability:** Business Owner: CA-D-009. Publishes BN-D-024. Consumes BC-D-026–027. Realises BO-DI-001, BO-DI-006.
## 5.10 Learn From Demand — CA-D-010

**Business Intent:** Continuously discover and record what the enterprise has concluded about the performance and behaviour of its demand intelligence capabilities, by systematically analyzing outcomes, patterns, and evidence across the entire domain.

**Owned Semantic Objects:** SE-D-042 (Demand Learning).

**Referenced Reference Objects:** SE-DI-072 (Learning Analysis Policy). Analyzes all Demand Intelligence semantic objects: quality assessments (SE-D-039), planning conditions (SE-D-040), explanations (SE-D-041), classifications (SE-D-036, SE-D-037), priorities (SE-D-038), behaviour assessments (SE-D-035), and performance data from all demand capabilities.

**Business Guarantees:**
- Every recorded learning is supported by evidence from at least one completed analysis or evaluation.
- Learnings are immutable once recorded and permanently retained.
- Each learning states what was discovered and how strongly the evidence supports it, without prescribing actions.
- The Learning Type taxonomy and Evidence Strength criteria are governed by the Learning Analysis Policy, enabling new types of learning to be recognized without modifying the semantic model.
- The capability analyzes the complete enterprise knowledge produced by Demand Intelligence, and may discover patterns that span multiple capabilities.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-022 | Record Demand Learning | BW-D-022 | FS-D-022 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-025 | Demand Learning Recorded: Learning Identifier, Learning Type, Learning Statement Summary, Evidence Strength | At-least-once | Per learning | Batch (post-analysis) |

### Business Notifications Consumed

| ID | Notification | Business Behaviour | Invokes |
|----|-------------|-------------------|---------|
| BC-D-028 | BN-D-021 Forecast Quality Assessment Published | Analyze accuracy trends, bias patterns, and FVA results for performance learnings. | FS-D-022 (conditional) |
| BC-D-029 | BN-D-023 Demand Planning Condition Resolved | Analyze condition histories for recurrence patterns, resolution timing, and systemic issues. | FS-D-022 (conditional) |
| BC-D-030 | BN-D-024 Demand Explanation Recorded | Analyze explanation quality and completeness for learnings about explainability. | FS-D-022 (conditional) |
| BC-D-031 | All Demand Intelligence notifications | Learn From Demand may analyze any published enterprise knowledge from Demand Intelligence to discover learnings. | FS-D-022 (conditional) |

**Traceability:** Business Owner: CA-D-010. Publishes BN-D-025. Consumes BC-D-028–031. Realises BO-DI-006.

---

# Chapter 6 — Decision Model

## DE-D-010 — Accept Demand Observation

**Decision Owner:** AB-D-002 Evaluate Demand Observation  
**Purpose:** Determine whether a received Business Observation is suitable for incorporation into Enterprise Demand.  
**Alternatives:** Accepted, Accepted With Warning, Quarantined, Rejected.  
**Criteria Evaluation:** All criteria are mandatory. Rules BR-D-010, BR-D-011, BR-D-012 are evaluated. Failure of BR-D-012 results in Quarantine. Failure of BR-D-010 or BR-D-011 results in Quarantine or Rejection based on severity.  
**Conflict Resolution:** If multiple rules fail, the most severe outcome prevails (Rejection over Quarantine).  
**Confidence:** Derived from source reliability index and signal consistency score.  
**Rationale Template:** "Observation accepted because source reliability is {x}%, timestamp is {y} min old, value within {z}σ of recent demand."

**Traceability:** Decision Owner: AB-D-002. Invoked by: FS-D-002. References: BR-D-010, BR-D-011, BR-D-012, BR-D-016. Governed By: PO-D-001, PO-D-006, PO-D-007, PO-D-008.

---

## DE-D-011 — Apply Planning Adjustment

**Decision Owner:** AB-EDP-002 Calculate Planning Demand  
**Purpose:** Determine whether an approved Planning Adjustment shall modify the current Planning Demand.  
**Alternatives:** Apply, Do Not Apply.  
**Criteria Evaluation:** Adjustment must be Approved, Effective, and Not Expired (BR-D-039). All mandatory.  
**Confidence:** High if adjustment state is unambiguous.  

**Traceability:** Decision Owner: AB-EDP-002. Invoked by: FS-D-005. References: BR-D-039.

---

## DE-D-012 — Publish Enterprise Demand Picture

**Decision Owner:** AB-EDP-003 Publish Enterprise Demand Picture  
**Purpose:** Determine whether the Enterprise Demand Picture is eligible for publication.  
**Alternatives:** Publish, Do Not Publish.  
**Criteria Evaluation:** All criteria mandatory (BR-D-044, BR-D-046, BR-D-047).  
**Confidence:** Binary.  

**Traceability:** Decision Owner: AB-EDP-003. Invoked by: FS-D-006. References: BR-D-044, BR-D-046, BR-D-047. Governed By: PO-D-013, PO-D-014, PO-D-015.

---

## DE-D-020 — Select Champion Model

**Decision Owner:** AB-F-002 Select Champion Model  
**Purpose:** Evaluate challenger models against the current champion and decide which model shall generate the official forecast for this cycle.  
**Alternatives:** Retain current champion, Promote specific challenger to champion, Require manual review.  
**Criteria Evaluation:** All criteria mandatory. Rules evaluated: BR-D-036 (significance), BR-D-037 (no harm), BR-D-038 (high-priority protection). If any rule fails, manual review is required per PO-D-017. If no challenger results are available, the current champion is retained without evaluation.  
**Conflict Resolution:** If multiple challengers satisfy all rules, the one with the lowest WAPE is selected.  
**Confidence:** Based on statistical significance (p-value) of the WAPE improvement and evaluation window length.  
**Rationale Template:** "Model {challenger} promoted to champion. WAPE improved from {old}% to {new}% over {weeks}-week evaluation window (p={pvalue}). No degradation on bias, stability, or high-priority items."

**Traceability:** Decision Owner: AB-F-002. Invoked by: FS-D-008. References: BR-D-036, BR-D-037, BR-D-038. Governed By: PO-D-017, PO-D-018.

---

## DE-D-021 — Generate Forecast for Series

**Decision Owner:** AB-F-003 Generate Baseline Forecasts  
**Purpose:** Produce the initial statistical forecast for a single SKU-StockingPoint series before any judgmental overrides.  
**Alternatives:** Generate using champion model, Flag as unforecastable, Apply fallback method (naive forecast or planner placeholder per PO-D-019).  
**Criteria Evaluation:** Rules evaluated per series: BR-D-039 (non-negative values), BR-D-040 (data sufficiency), BR-D-041 (prediction interval completeness). All mandatory.  
**Conflict Resolution:** A series that fails any rule is flagged as unforecastable and handled per PO-D-019.  
**Confidence:** Per-series: the model's inherent confidence score. Aggregate: overall Forecast Confidence Index across all series.  
**Rationale Template:** "Series {id}: forecast generated with confidence {c}%. {n} series flagged as unforecastable."

**Traceability:** Decision Owner: AB-F-003. Invoked by: FS-D-009. References: BR-D-039, BR-D-040, BR-D-041. Governed By: PO-D-019.

---

## DE-D-022 — Approve Forecast Publication

**Decision Owner:** AB-F-005 Publish Forecast Publication  
**Purpose:** Determine whether the Forecast Publication is eligible for release to downstream domains.  
**Alternatives:** Publish automatically, Require planner approval, Suppress publication.  
**Criteria Evaluation:** Rules evaluated: BR-D-026 (completeness threshold). If Forecast Confidence Index meets auto-publication threshold and completeness rule passes → Publish automatically (PO-D-020). Otherwise → Require planner approval.  
**Conflict Resolution:** A Demand Manager may override per PO-D-021.  
**Confidence:** Binary.  
**Rationale Template:** "Forecast published automatically. Confidence Index {x}% (threshold {y}%), completeness {z}% (threshold 95%)."

**Traceability:** Decision Owner: AB-F-005. Invoked by: FS-D-011. References: BR-D-026. Governed By: PO-D-020, PO-D-021.

---

## DE-D-023 — Evaluate Forecast Override

**Decision Owner:** AB-F-004 Record Forecast Override  
**Purpose:** Allow a planner to replace a system-generated forecast value when they possess business knowledge not yet reflected in the demand signals.  
**Alternatives:** Accept override, Reject override, Request revision.  
**Criteria Evaluation:** Rules evaluated: BR-D-042 (justification non-empty), BR-D-043 (deviation within configured limit). All mandatory. Planner authorization checked per PO-D-022.  
**Conflict Resolution:** If justification is empty → Reject. If deviation exceeds limit and planner is not Demand Manager → Request revision or escalate per PO-D-022.  
**Confidence:** Marked lower than system forecast confidence when override is applied.  
**Rationale Template:** "Forecast for {product} overridden from {original} to {override} units. Justification: '{reason}'. Deviation {deviation}%."

**Traceability:** Decision Owner: AB-F-004. Invoked by: FS-D-010. References: BR-D-042, BR-D-043. Governed By: PO-D-022, PO-D-023.

---

## DE-D-024 — Determine New SKU Forecast Method

**Decision Owner:** AB-F-003 Generate Baseline Forecasts  
**Purpose:** Select the appropriate forecasting approach for a product with insufficient history for statistical forecasting.  
**Alternatives:** Use analog product, Attribute-based similarity, Lifecycle model, Launch curve, Expert judgment, Defer to planner.  
**Criteria:** SKU attributes, lifecycle stage (SE-DI-061), availability of analog data, planner input.  
**Conflict Resolution:** If multiple methods are viable, prefer the one with the lowest expected error based on analog performance.  
**Confidence:** Lower than statistical forecast; explicitly flagged.  
**Rationale Template:** "New product {id}: forecast method {method} selected based on {criteria}. Confidence: {c}%."

**Traceability:** Decision Owner: AB-F-003. Invoked by: FS-D-009. References: BR-D-033. Governed By: PO-D-028.

---

## DE-D-025 — Reconcile Hierarchy Node

**Decision Owner:** AB-F-007 Reconcile Forecast Hierarchy  
**Purpose:** For each node in the forecast hierarchy, determine the reconciliation method to align bottom-up and top-down forecasts.  
**Alternatives:** Bottom-up, Top-down, Middle-out, Proportional distribution, Statistical reconciliation (MinT, OLS).  
**Criteria:** Historical accuracy of each method for this node, data quality, planner preference, configured default.  
**Conflict Resolution:** If historical accuracy data is insufficient, use the configured default method.  
**Confidence:** Based on reconciliation error history.  
**Rationale Template:** "Node {node}: reconciliation method {method} applied. Historical reconciliation error: {e}%."

**Traceability:** Decision Owner: AB-F-007. Invoked by: FS-D-013. References: BR-D-034. Governed By: PO-D-029.

---

## DE-D-030 — Evaluate Demand Signal for State Change

**Decision Owner:** AB-S-001 Update Demand Behaviour Assessment  
**Purpose:** Determine whether the incoming demand signal, when evaluated against the Demand Baseline, warrants a change to the current state of the Demand Behaviour Assessment.  
**Alternatives:** No Change, Transition to Elevated/Depressed (Significant), Transition to Critical.  
**Criteria Evaluation:** Rules evaluated: BR-D-050 (deviation thresholds), BR-D-051 (signal corroboration for Critical), BR-D-052 (high-priority sensitivity). All criteria are mandatory.  
**Conflict Resolution:** If the signal exceeds the Critical threshold and is corroborated, the outcome is Critical. If it exceeds the Significant threshold but does not meet Critical criteria, it is Elevated/Depressed. If no threshold is met, No Change.  
**Confidence:** Derived from signal quality, deviation magnitude, and corroboration strength.  
**Rationale Template:** "Signal for {product} at {location} evaluated. Deviation {d}σ from baseline. Outcome: {state}. Confidence: {c}%."

**Traceability:** Decision Owner: AB-S-001. Invoked by: FS-D-014. References: BR-D-050, BR-D-051, BR-D-052. Governed By: PO-D-031.

---

## DE-D-031 — Trigger Forecast Refresh on Critical State

**Decision Owner:** FS-D-015 Trigger Downstream Actions  
**Purpose:** Determine whether a newly transitioned Critical state warrants an immediate out-of-cycle forecast refresh.  
**Alternatives:** Trigger refresh, Defer to next scheduled cycle.  
**Criteria:** State must be Critical, forecast age exceeds configured freshness threshold, expected forecast accuracy improvement exceeds minimum benefit (BR-D-054).  
**Conflict Resolution:** All criteria must be satisfied; otherwise defer.  
**Confidence:** Inherited from the state change confidence.  
**Rationale Template:** "Forecast refresh triggered for {product} at {location} due to Critical demand behaviour. Forecast age: {hours}h."

**Traceability:** Decision Owner: FS-D-015. Invoked by: FS-D-015. References: BR-D-054. Governed By: PO-D-032, PO-D-034.

---

## DE-D-032 — Determine Classification

**Decision Owner:** AB-SG-001 Update Planning Classification  
**Purpose:** For a given entity and classification type, determine the current class label according to the Segmentation Policy.  
**Alternatives:** The set of class labels for the type (e.g., A/B/C for ABC; X/Y/Z for XYZ; Gold/Silver/Bronze for Strategic), or Unclassified if evidence is insufficient.  
**Criteria Evaluation:** Rules are defined per classification type in the Segmentation Policy. All criteria are mandatory.  
**Conflict Resolution:** If insufficient evidence → Unclassified. Otherwise, the rule matching the entity’s attributes is applied.  
**Confidence:** Based on evidence quality and classification rule fit.  
**Rationale Template:** "Entity {id} classified as {class} for type {type} based on {rule}. Confidence: {c}%."

**Traceability:** Decision Owner: AB-SG-001. Invoked by: FS-D-016. References: BR-D-061, BR-D-062. Governed By: PO-D-035.

---

## DE-D-033 — Determine Behaviour Classification

**Decision Owner:** AB-CL-001 Update Behaviour Classification  
**Purpose:** For a given entity and behaviour dimension, determine the current classification according to the Classification Policy.  
**Alternatives:** The set of recognised classifications for the dimension as defined in the Classification Policy (e.g., Continuous, Intermittent, Seasonal, Lumpy, Trend for Statistical Pattern), or Unclassified if evidence is insufficient.  
**Criteria Evaluation:** Rules are defined per behaviour dimension in the Classification Policy. All criteria are mandatory.  
**Conflict Resolution:** If insufficient evidence → Unclassified. Multiple independent dimensions capture composite behaviours without requiring a single composite label.  
**Confidence:** Based on evidence quality and statistical significance of detected features.  
**Rationale Template:** "Entity {id}, dimension {dim}: classified as {class}. Evidence: {summary}. Confidence: {c}%."

**Traceability:** Decision Owner: AB-CL-001. Invoked by: FS-D-017. References: BR-D-066, BR-D-067. Governed By: PO-D-037.

---

## DE-D-034 — Determine Planning Priority

**Decision Owner:** AB-PR-001 Update Planning Priority  
**Purpose:** For a given planning entity, determine its current planning priority according to the Prioritization Policy.  
**Alternatives:** Critical, High, Medium, Low, or Unclassified (insufficient business evidence).  
**Criteria Evaluation:** The Prioritization Policy defines the scoring methodology and priority level thresholds. All mandatory evidence must be available.  
**Conflict Resolution:** If mandatory evidence is missing → Unclassified. Priority represents planning importance, not a classification; it establishes ordering, not partitioning.  
**Confidence:** Derived from input data quality and policy parameters.  
**Rationale Template:** "Entity {id}: priority {level}. Rationale: {business justification}."

**Traceability:** Decision Owner: AB-PR-001. Invoked by: FS-D-018. References: BR-D-075, BR-D-076. Governed By: PO-D-039.

---

## DE-D-035 — Publish Forecast Quality Assessment

**Decision Owner:** AB-EQ-001 Publish Forecast Quality Assessment  
**Purpose:** Determine whether the computed forecast quality metrics meet the publication criteria defined in the Forecast Measurement Policy, enabling a business decision about whether the assessment is fit for publication as an authoritative enterprise record.  
**Alternatives:** Publish, Do Not Publish.  
**Criteria Evaluation:** BR-D-080 (data completeness), BR-D-081 (minimum evaluation period length). The decision may involve human review if the policy requires it.  
**Conflict Resolution:** If any mandatory criterion fails → Do Not Publish. Demand Manager notified per PO-D-041.  
**Confidence:** Derived from source data quality.  
**Rationale Template:** "Assessment for Planning Scope {scope}, period {period}: completeness {pct}%. Metrics meet publication criteria. Published as version {version}."

**Traceability:** Decision Owner: AB-EQ-001. Invoked by: FS-D-019. References: BR-D-080, BR-D-081. Governed By: PO-D-041.

---

## DE-D-036 — Evaluate Demand Planning Condition

**Decision Owner:** AB-DE-001 Recognize Demand Planning Condition  
**Purpose:** For a given planning entity and condition type, determine whether the current demand information meets the detection thresholds defined in the Exception Detection Policy and, if so, at what severity.  
**Alternatives:** Condition Exists (with severity: Critical, High, Medium, Low), No Condition.  
**Criteria Evaluation:** Rules and thresholds are defined per condition type in the Exception Detection Policy. All criteria are mandatory.  
**Conflict Resolution:** If detection thresholds are met → Condition Exists at the determined severity. If thresholds are not met → No Condition.  
**Confidence:** Derived from source data quality.  
**Rationale Template:** "Condition type {type} for {entity}: {outcome}. Evidence: {summary}. Severity: {severity}."

**Traceability:** Decision Owner: AB-DE-001. Invoked by: FS-D-020. References: BR-D-086. Governed By: PO-D-044.

---

# Chapter 7 — Rule Model

### Rule Precedence

| Rule Type | Overridable By |
|-----------|----------------|
| Identity | None |
| Invariant | None |
| Eligibility | Behaviour |
| Behaviour | (none below it) |
| Derivation | (none below it) |

Invariant Rules and Identity Rules cannot be overridden. If a Policy appears to contradict an Invariant Rule, the Invariant takes precedence and the Policy is invalid.

---

### Identity Rules

| ID | Rule |
|----|------|
| BR-D-001 | A Demand Observation identity shall be unique and immutable once assigned. |
| BR-D-025 | A Planning Scope identity shall uniquely identify one Planning Scope within the enterprise. |

### Eligibility Rules

| ID | Rule |
|----|------|
| BR-D-010 | Demand Signal timeliness shall be within maximum allowed latency. |
| BR-D-011 | Demand Signal quantity shall not deviate beyond configured statistical bounds. |
| BR-D-012 | Signal source reliability shall meet minimum threshold. |
| BR-D-013 | Demand Observation Lifecycle State shall equal Received before evaluation. |
| BR-D-015 | Demand Observation shall exist before evaluation. |
| BR-D-022 | Demand Observation Lifecycle State shall equal Accepted for Enterprise Demand Picture creation. |
| BR-D-024 | Planning Scope shall be established before Enterprise Demand Picture creation. |
| BR-D-032 | Enterprise Demand Picture Status shall equal Awaiting Planning Demand Calculation before calculation. |
| BR-D-044 | Enterprise Demand Picture Status shall equal Ready For Publication before publication. |
| BR-D-046 | Planning Demand shall exist before publication. |
| BR-D-047 | Planning Demand shall be calculated before publication. |
| BR-D-033 | A product with insufficient history for statistical forecasting must have an explicitly assigned new product forecast method. |
| BR-D-036 | A challenger model may replace the champion only if it demonstrates a statistically significant reduction in WAPE (p ≤ 0.05) over a minimum evaluation period. |
| BR-D-039 | All forecast mean values must be non-negative. |
| BR-D-040 | A minimum number of periods of demand history is required to generate a statistical forecast. Series with insufficient history are flagged as unforecastable. |
| BR-D-041 | Every forecast must include a prediction interval. Missing intervals cause the forecast to be marked incomplete. |
| BR-D-042 | Every override must contain a non-empty business justification. Overrides without justification are rejected. |
| BR-D-043 | The override value must not deviate from the system forecast mean by more than the configured deviation limit unless an exception policy is invoked. |
| BR-D-050 | A state transition to Elevated or Depressed requires the signal deviation to exceed the configured Significant threshold (default 2.5σ). A transition to Critical requires deviation to exceed the configured Critical threshold (default 4σ). |
| BR-D-051 | A Critical state change shall be corroborated by at least two independent signal sources before the state is updated and BN-D-016 is published. |
| BR-D-052 | For products classified as high-priority, the Significant threshold is lowered to the configured reduced value. |
| BR-D-053 | Signals with deviation below the configured noise threshold shall not trigger a state change. |
| BR-D-061 | Classification shall be determined by the rules defined for the classification type in the current Segmentation Policy. |
| BR-D-062 | An entity shall be classified as Unclassified for a type if the minimum evidence requirements defined in the policy are not met. |
| BR-D-066 | Classification shall be determined by the rules defined for the behaviour dimension in the current Classification Policy. |
| BR-D-067 | An entity shall be classified as Unclassified for a dimension if the minimum evidence requirements defined in the policy are not met. |
| BR-D-075 | Priority shall be determined using the scoring methodology and level thresholds defined in the current Prioritization Policy. |
| BR-D-076 | An entity shall be assigned Unclassified priority if the mandatory business evidence defined in the policy is not available. |
| BR-D-080 | A Forecast Quality Assessment shall only be published if actual demand data covers the full evaluation period and meets the completeness threshold defined in the Forecast Measurement Policy. |
| BR-D-081 | The evaluation period shall meet the minimum length defined in the Forecast Measurement Policy. |
| BR-D-086 | A Demand Planning Condition shall only be recognized if the detection evidence meets the thresholds defined for that condition type in the current Exception Detection Policy. |

### Invariant Rules

| ID | Rule |
|----|------|
| BR-D-002 | Rejected Demand Observations shall never contribute to Enterprise Demand. |
| BR-D-003 | Quarantined Demand Observations shall never contribute until accepted. |
| BR-D-004 | Every Accepted Demand Observation shall belong to exactly one Planning Scope. |
| BR-D-005 | Exactly one Published Enterprise Demand Picture shall exist per Planning Scope at any moment. |
| BR-D-005-F | Exactly one Published Forecast Publication shall exist for a given planning scope at any moment. |
| BR-D-006 | Published Enterprise Demand Picture shall never be modified directly. |
| BR-D-014 | A Demand Observation shall be evaluated only once from Received state. |
| BR-D-027 | At most one Active Planning Scope shall exist for a given identity. |
| BR-D-028 | Planning Demand shall not be calculated until Operational Demand is updated. |
| BR-D-029 | A Published Forecast Publication shall never be modified. |
| BR-D-034 | After reconciliation, the sum of child node forecasts must equal the parent node forecast (or a documented reconciliation difference must be recorded). |
| BR-D-037 | A challenger must not increase absolute bias beyond the configured tolerance and must not degrade forecast stability beyond the configured tolerance. |
| BR-D-038 | On products classified as high-priority, the challenger must not show a WAPE increase exceeding the configured protection threshold. |
| BR-D-038-I | Operational Demand shall never be modified by Planning Adjustments. |
| BR-D-041-I | Planning Demand shall never overwrite Operational Demand. |
| BR-D-042-I | Every Planning Demand quantity shall be fully traceable. |
| BR-D-045 | The original system forecast shall be preserved when an override is applied. |
| BR-D-048 | Historical Enterprise Demand Pictures shall remain permanently available. |
| BR-D-056 | A Superseded Enterprise Demand Picture shall never return to Published. |
| BR-D-057 | Every version shall record Business Time, Transaction Time, and Publication Time. |
| BR-D-058 | Historical Demand Observations shall not be retroactively altered when Reference Objects change. |
| BR-D-055 | Every monitored SKU-StockingPoint shall have exactly one Current State at any moment. |
| BR-D-059 | State Change Events shall be preserved permanently for audit and detection accuracy measurement. |
| BR-D-063 | At any moment, an active entity shall have exactly one current classification per active classification type. |
| BR-D-064 | Classification history shall be permanently preserved. |
| BR-D-073 | At any moment, an active entity shall have exactly one current classification per active behaviour dimension. |
| BR-D-068 | Classification history shall be permanently preserved. |
| BR-D-077 | At any moment, an active entity shall have exactly one current priority. |
| BR-D-078 | Priority history shall be permanently preserved. |
| BR-D-082 | Metrics shall be computed according to the definitions and formulas in the current Forecast Measurement Policy. |
| BR-D-083 | Exactly one Published Forecast Quality Assessment shall exist for a given Planning Scope and Evaluation Period. |
| BR-D-084 | A Published assessment is immutable. |
| BR-D-085 | All published assessments shall be permanently retained. |
| BR-D-088 | Detection and resolution shall be performed according to the condition types and criteria defined in the current Exception Detection Policy. |
| BR-D-089 | At any moment, a Demand Planning Condition is either Active or Resolved, never both. |
| BR-D-090 | Condition History shall be permanently preserved. |
| BR-D-091 | A Resolved condition is terminal. Recurrence of the same condition type for the same planning entity shall create a new condition instance with a new identifier. |
| BR-D-092 | The Structured Reasoning Graph shall reference the versions of every contributing rule, decision, policy, model, and template that were in effect when the explained artifact was produced. |
| BR-D-093 | The Structured Reasoning Graph shall be deterministic: identical explained artifact, evidence, policy versions, and template version shall produce an identical reasoning graph. |
| BR-D-094 | An explanation, once created, shall never be modified. |
| BR-D-096 | A learning shall be supported by evidence from at least one completed analysis or evaluation. |
| BR-D-097 | A learning, once created, shall never be modified. |

### Behaviour Rules

| ID | Rule |
|----|------|
| BR-D-016 | DE-D-010 shall produce exactly one outcome. |
| BR-D-017 | Accepted observations shall record Decision Identifier, Timestamp, Confidence, and Rationale. |
| BR-D-018 | Warnings shall not prevent participation in Enterprise Demand. |
| BR-D-019 | Quarantined observations shall not participate until quarantine is released. |
| BR-D-020 | Rejected observations shall never participate. |
| BR-D-035 | Every Demand Driver used in a forecast must be linked to at least one Forecast Assumption that interprets its impact. |
| BR-D-046 | Overrides are permanently recorded with planner identity, justification, and timestamp. |
| BR-D-047 | Every Forecast Publication shall record the champion model identity used for generation. |
| BR-D-057 | A transition to Critical shall automatically trigger evaluation for a forecast refresh (DE-D-031). |
| BR-D-060 | A state change to Elevated or Depressed shall be routed to the Demand Planner for review within the configured response time. |
| BR-D-065 | Planner overrides to classification shall be recorded with justification and are subject to periodic review. |
| BR-D-074 | Planner overrides to classification shall be recorded with justification and are subject to periodic review. |
| BR-D-079 | Planner overrides to priority shall be recorded with business justification and are subject to periodic review. |

### Derivation Rules

| ID | Rule |
|----|------|
| BR-D-008 | Planning Demand = Operational Demand + Effective Adjustments + Effective Overrides. |
| BR-D-030 | Operational Demand = accumulation of all Accepted Demand Observations for the Planning Scope and Period. |
| BR-D-048-D | Forecast = champion model output applied to cleansed demand history and demand signals for the SKU-StockingPoint-time bucket. |

**Traceability:** All rules owned by their respective Capability. Referenced by Decisions, Preconditions, and Algorithms. Governed by Policies.

---

# Chapter 8 — Policy Model

| ID | Policy | Category | Governance Outcome | Governed Rules |
|----|--------|----------|-------------------|----------------|
| PO-D-001 | Only authorised Business Observations may participate in Enterprise Demand. | Authorization | Terminate processing; observation not established. | BR-D-001 |
| PO-D-002 | Planning Adjustments require business approval before becoming effective. | Authorization | Adjustment held pending approval. | BR-D-008 |
| PO-D-003 | Planner Overrides require business approval before becoming effective. | Authorization | Override held pending approval. | BR-D-008 |
| PO-D-004 | Historical Enterprise Demand Pictures shall be retained per enterprise policy. | Compliance | Retain; do not delete. | BR-D-048 |
| PO-D-005 | Enterprise Demand shall not be published while mandatory governance policies remain unsatisfied. | Compliance | Suspend publication; notify Demand Manager. | BR-D-044 |
| PO-D-006 | Quarantined observations shall not be processed until quarantine is released. | Exception | Suspend processing; route to Demand Data Steward. | BR-D-019 |
| PO-D-007 | Rejected observations shall never be reconsidered. | Exception | Terminate permanently. | BR-D-020 |
| PO-D-008 | If a Decision cannot produce a valid outcome, processing terminates and state remains unchanged. | Exception | Terminate; retain previous state. | BR-D-016 |
| PO-D-009 | If preconditions for EDP creation fail, existing EDP remains current. | Exception | Terminate; retain current EDP as authoritative. | BR-D-024 |
| PO-D-010 | If EDP creation fails, previous published version remains authoritative. | Exception | Retain previous published version; no new version created. | BR-D-006 |
| PO-D-011 | If Planning Demand calculation fails, status remains Awaiting Calculation. | Exception | Suspend; allow retry. | BR-D-032 |
| PO-D-012 | Failed Planning Demand shall not be published. | Exception | Suppress publication. | BR-D-046 |
| PO-D-013 | EDP shall not be published if preconditions fail. | Exception | Suppress publication; notify Demand Planner. | BR-D-044 |
| PO-D-014 | Publication transfers responsibility to downstream capabilities. | Automation | Publish BN-D-001; downstream consumers react independently. | BR-D-005 |
| PO-D-015 | If publication fails, status remains Ready For Publication. | Exception | Retain Ready For Publication status; allow retry. | BR-D-044 |
| PO-D-016 | If Planning Scope cannot be determined, observation remains Accepted but unassigned. | Exception | Suspend; route to Demand Data Steward. | BR-D-004 |
| PO-D-017 | Automatic champion promotion is permitted if all Model Evaluation Rules pass. If any rule fails, route to Demand Manager for manual approval. | Automation | Promote automatically or escalate. | BR-D-036, BR-D-037, BR-D-038 |
| PO-D-018 | If within a defined period after promotion the new champion causes a service-level drop attributable to forecast degradation, the Demand Manager may rollback to the previous champion without further approval. | Exception | Rollback permitted; previous champion reinstated. | BR-D-036 |
| PO-D-019 | Series flagged as unforecastable are automatically assigned a fallback forecast method according to product life-cycle stage. | Exception | Apply fallback; flag for review. | BR-D-040 |
| PO-D-020 | If Forecast Confidence Index meets or exceeds the auto-publication threshold and the completeness rule passes, publish automatically. Otherwise, route to Demand Planner for approval. | Automation | Auto-publish or escalate. | BR-D-026 |
| PO-D-021 | A Demand Manager may override the auto-publication decision and force publication or suppression with a documented business reason. | Authorization | Override applied; reason recorded. | BR-D-026 |
| PO-D-022 | Only users in the Demand Planner role may submit an override. Overrides exceeding the deviation limit require Demand Manager approval. | Authorization | Accept, escalate, or reject. | BR-D-042, BR-D-043 |
| PO-D-023 | All overrides, including justifications, are logged and subject to quarterly review to detect planner bias. | Compliance | Logged; reviewed quarterly. | BR-D-046 |
| PO-D-024 | Forecast cycles shall be initiated on the defined enterprise cadence. Out-of-cycle initiation requires Demand Manager approval unless triggered by a Critical demand change. | Authorization | Scheduled or exception-based initiation. | — |
| PO-D-025 | Forecast Assumptions must be reviewed and signed off by the relevant department head before the forecast can be published. | Compliance | Publication blocked until sign-off. | BR-D-035 |
| PO-D-028 | New product forecast method requires Demand Manager approval before publication. | Authorization | Method approved or escalated. | BR-D-033 |
| PO-D-029 | Reconciliation method may be selected automatically if historical confidence exceeds threshold; otherwise manual selection required. | Automation | Auto-select or escalate. | BR-D-034 |
| PO-D-030 | Forecast Assumptions must be reviewed and signed off by the relevant department head before the forecast can be published. | Compliance | Publication blocked until sign-off. | BR-D-035 |
| PO-D-031 | State changes to Elevated or Depressed are routed to the Demand Planner. Critical changes are routed to the Demand Manager and automatically trigger forecast refresh evaluation. | Automation | Route or escalate. | BR-D-057, BR-D-060 |
| PO-D-032 | A Critical state change automatically initiates an out-of-cycle forecast refresh evaluation. | Automation | Evaluation triggered. | BR-D-057 |
| PO-D-033 | Signals evaluated below the noise threshold are suppressed and not presented to planners. | Exception | Suppress. | BR-D-053 |
| PO-D-034 | Partial forecast refreshes for Significant changes are executed automatically. Full refreshes require Demand Manager approval unless the change is Critical. | Authorization | Auto or escalate. | BR-D-054 |
| PO-D-035 | Classification rules for all types are governed by the Segmentation Policy owned by Planning Governance. Changes to the policy trigger automatic reclassification of affected entities. | Compliance | Reclassification triggered. | BR-D-061 |
| PO-D-036 | Planner overrides to classification require justification and are reviewed quarterly. | Compliance | Override recorded; reviewed. | BR-D-065 |
| PO-D-037 | Classification rules for all behaviour dimensions are governed by the Classification Policy owned by Planning Governance. Changes to the policy trigger automatic reclassification. | Compliance | Reclassification triggered. | BR-D-066 |
| PO-D-038 | Planner overrides to behaviour classification require justification and are reviewed quarterly. | Compliance | Override recorded; reviewed. | BR-D-074 |
| PO-D-039 | Priority scoring methodology and level thresholds are governed by the Prioritization Policy owned by Planning Governance. Changes to the policy trigger automatic re-evaluation. | Compliance | Re-evaluation triggered. | BR-D-075 |
| PO-D-040 | Planner overrides to priority require business justification and are reviewed quarterly. | Compliance | Override recorded; reviewed. | BR-D-079 |
| PO-D-041 | If data completeness is below the publication threshold, the assessment is suppressed and the Demand Manager is notified. | Exception | Suppress; notify. | BR-D-080 |
| PO-D-042 | Quality assessments shall be published on the cadence defined in the Forecast Measurement Policy. | Compliance | Scheduled publication. | — |
| PO-D-043 | The Overall Quality Score, if used, is a policy-governed derived metric, not an independent enterprise fact. Its derivation methodology is defined in the Forecast Measurement Policy. | Compliance | Derived per policy. | BR-D-082 |
| PO-D-044 | Condition detection thresholds, severity rules, and resolution criteria are defined in the Exception Detection Policy owned by Planning Governance. | Compliance | Governed by policy. | BR-D-086 |
| PO-D-045 | Planner acknowledgement and workflow management are external to this capability. The Demand Planning Condition records only existence and resolution, not human workflow states. | Compliance | Separation of concerns. | — |
| PO-D-046 | The reasoning graph structure and determinism of explanations are governed by the Explanation Template Catalog. | Compliance | Governed by policy. | BR-D-093 |
| PO-D-047 | Automatic explanations are triggered for Critical planning conditions and Published forecasts. Other explanations are generated on-demand by planner request. | Automation | Automatic explanation generation. | — |
| PO-D-048 | Analysis cadence, evidence sufficiency thresholds, Evidence Strength criteria, governed learning scope (which domains and cross-capability relationships are eligible for learning), and the taxonomy of recognized Learning Types are defined in the Learning Analysis Policy owned by Planning Governance. New Learning Types may be added to the taxonomy by policy change without modifying the semantic model. | Compliance | Governed by policy. | BR-D-096 |
| PO-D-049 | Learnings are recorded as immutable enterprise records. Recommendations, actions, and policy changes derived from learnings are the responsibility of Planning Governance or consuming capabilities, not Learn From Demand. | Compliance | Separation of discovery from action. | BR-D-097 |

**Traceability:** All policies owned by their respective Capability. Govern the indicated Rules. Referenced by Functional Specifications.

---

# Chapter 9 — Aggregate Behaviour Catalogue

## Understand Demand Behaviours

### AB-D-001 — Establish Demand Observation

**Purpose:** Establish a Demand Observation from a received business observation.  
**Business Intent:** Create an immutable enterprise record of exactly what was received, before any evaluation.  
**Owned Aggregate:** Demand Observation (SE-D-001).  
**Required Input State:** None (creation).  
**Produced Output State:** Received.  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** None (notification published by invoking FS).  
**Business Transaction:** Protects Demand Observation aggregate. Atomic creation of identity, mandatory attributes, and provenance.  
**Idempotency:** Re-execution with the same business identity produces no duplicate (BR-D-001).  
**Concurrency:** Observations with different identities processed independently.

**Traceability:** Owned by SE-D-001. Invoked by FS-D-001.

### AB-D-002 — Evaluate Demand Observation

**Purpose:** Evaluate a Demand Observation against acceptance criteria.  
**Business Intent:** Determine whether an observation is trustworthy enough to contribute to Enterprise Demand.  
**Owned Aggregate:** Demand Observation (SE-D-001).  
**Required Input State:** Received.  
**Produced Output State:** Accepted, Quarantined, or Rejected.  
**Invoked Decisions:** DE-D-010.  
**Invoked Algorithms:** None.  
**Published Events:** None.  
**Business Transaction:** Protects Demand Observation aggregate. Atomic state transition and decision traceability recording.  
**Idempotency:** Re-execution on already-evaluated observation terminates immediately (BR-D-014).  
**Concurrency:** Evaluated exactly once per observation.

**Traceability:** Owned by SE-D-001. Invoked by FS-D-002. Invokes DE-D-010.

### AB-PS-001 — Determine Planning Scope

**Purpose:** Assign a Demand Observation to the correct Planning Scope, creating the scope if it does not exist.  
**Business Intent:** Ensure every Accepted observation is routed to its correct planning independence boundary.  
**Owned Aggregate:** Planning Scope (SE-D-002).  
**Required Input State:** Demand Observation in Accepted, unassigned.  
**Produced Output State:** Planning Scope Active (or remains Active). Demand Observation assigned.  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** None.  
**Business Transaction:** Protects Planning Scope aggregate. Atomic lookup-or-create and assignment.  
**Idempotency:** Re-execution on already-assigned observation terminates immediately (BR-D-053).  
**Concurrency:** Observations for the same Planning Scope serialized by arrival order.

**Traceability:** Owned by SE-D-002. Invoked by FS-D-003.

### AB-EDP-001 — Revise Enterprise Demand Picture

**Purpose:** Create a new version of the Enterprise Demand Picture incorporating an Accepted Demand Observation.  
**Business Intent:** Keep the enterprise's demand understanding current by reflecting every accepted observation.  
**Owned Aggregate:** Enterprise Demand Picture (SE-D-003).  
**Required Input State:** None for first version; Published for subsequent versions.  
**Produced Output State:** Awaiting Planning Demand Calculation (new version). Previous version Superseded (if applicable).  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** None.  
**Business Transaction:** Protects Enterprise Demand Picture aggregate. Atomic version creation, Operational Demand update, and previous version superseding.  
**Idempotency:** Duplicate version creation prevented by Planning Scope serialization.  
**Concurrency:** Updates to the same Planning Scope serialized.

**Traceability:** Owned by SE-D-003. Invoked by FS-D-004.

### AB-EDP-002 — Calculate Planning Demand

**Purpose:** Calculate Planning Demand from Operational Demand plus adjustments and overrides.  
**Business Intent:** Produce the planning-approved demand quantity for downstream consumption.  
**Owned Aggregate:** Enterprise Demand Picture (SE-D-003).  
**Required Input State:** Awaiting Planning Demand Calculation.  
**Produced Output State:** Ready For Publication.  
**Invoked Decisions:** DE-D-011.  
**Invoked Algorithms:** BA-D-001.  
**Published Events:** None.  
**Business Transaction:** Protects Enterprise Demand Picture aggregate. Atomic Planning Demand creation and status transition.  
**Idempotency:** Already-calculated versions not recalculated.  
**Concurrency:** Calculation for a given version occurs exactly once.

**Traceability:** Owned by SE-D-003. Invoked by FS-D-005. Invokes DE-D-011, BA-D-001.

### AB-EDP-003 — Publish Enterprise Demand Picture

**Purpose:** Publish the Enterprise Demand Picture, making it authoritative for downstream planning.  
**Business Intent:** Transfer the latest planning interpretation to all consuming capabilities.  
**Owned Aggregate:** Enterprise Demand Picture (SE-D-003).  
**Required Input State:** Ready For Publication.  
**Produced Output State:** Published. Previous version Superseded (if exists).  
**Invoked Decisions:** DE-D-012.  
**Invoked Algorithms:** None.  
**Published Events:** None (notification published by invoking FS).  
**Business Transaction:** Protects Enterprise Demand Picture aggregate. Atomic publication, previous version superseding, and timestamp recording.  
**Idempotency:** Re-execution on already-published version terminates immediately (BR-D-045).  
**Concurrency:** Publication for a given version occurs exactly once.

**Traceability:** Owned by SE-D-003. Invoked by FS-D-006. Invokes DE-D-012.

---

## Forecast Demand Behaviours

### AB-F-001 — Initiate Forecast Cycle

**Purpose:** Initiate a new forecast cycle that will produce a Forecast Publication.  
**Business Intent:** Begin the forecasting process on the defined enterprise cadence, ensuring downstream planning always has current projections.  
**Owned Aggregate:** None (creation of workflow state; the cycle is a Business Workflow, not an Aggregate Root).  
**Required Input State:** None (workflow initiation).  
**Produced Output State:** Cycle Initialised (workflow state).  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** None (notification published by invoking FS).  
**Business Transaction:** Protects workflow state. Atomic creation of cycle identity and metadata.  
**Idempotency:** Each trigger creates a distinct cycle with a new unique identifier.  
**Concurrency:** Only one active cycle permitted at a time.

**Traceability:** Invoked by FS-D-007.

### AB-F-006 — Prepare Forecast Context

**Purpose:** Load business context for the forecast cycle: consume Demand Drivers, capture Forecast Assumptions, apply Forecast Coverage.  
**Business Intent:** Ensure every forecast publication is grounded in the current business reality before prediction begins.  
**Owned Aggregate:** Forecast Publication (SE-D-029) — creates Draft publication with assumptions and coverage.  
**Required Input State:** None (creation of Draft publication).  
**Produced Output State:** Draft publication created with assumptions and coverage applied.  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Business Transaction:** Protects Forecast Publication aggregate. Atomic creation of Draft version, assumption entities, and coverage snapshot.  
**Idempotency:** Re-execution updates assumptions and coverage for the same publication.

**Traceability:** Owned by SE-D-029. Invoked by FS-D-012.


### AB-F-002 — Select Champion Model

**Purpose:** Evaluate challenger models and select the best model for this cycle.  
**Business Intent:** Ensure every forecast publication uses the most accurate available model, with governance to prevent degradation.  
**Owned Aggregate:** None (workflow decision; champion identity recorded in the Forecast Publication later).  
**Required Input State:** Cycle in progress.  
**Produced Output State:** Champion model selected for the cycle.  
**Invoked Decisions:** DE-D-020.  
**Invoked Algorithms:** None.  
**Published Events:** None.  
**Business Transaction:** Protects workflow state. Atomic recording of champion identity.  
**Idempotency:** Re-execution on a cycle that already has a champion selected terminates immediately.  
**Concurrency:** Selection for a given cycle occurs exactly once.

**Traceability:** Invoked by FS-D-008. Invokes DE-D-020.

### AB-F-003 — Generate Baseline Forecasts

**Purpose:** Generate statistical forecasts for every covered SKU-StockingPoint combination and populate the Draft Forecast Publication.  
**Business Intent:** Produce the quantitative demand projections that downstream planning depends upon, with explicit handling of new products and unforecastable series.  
**Owned Aggregate:** Forecast Publication (SE-D-029).  
**Required Input State:** Draft publication exists.  
**Produced Output State:** Draft publication populated with forecast lines (SE-D-025).  
**Invoked Decisions:** DE-D-021 (per series), DE-D-024 (for new products).  
**Invoked Algorithms:** BA-D-002 (deferred).  
**Business Transaction:** Protects Forecast Publication aggregate. Atomic creation of Forecast entities and computation of Overall Confidence Index.  
**Idempotency:** Re-execution regenerates forecasts within the same Draft publication.  
**Concurrency:** Generation for a given publication occurs exactly once.

**Traceability:** Owned by SE-D-029. Invoked by FS-D-009. Invokes DE-D-021, DE-D-024.

### AB-F-007 — Reconcile Forecast Hierarchy

**Purpose:** Align forecasts within the Draft publication across hierarchy levels.  
**Business Intent:** Guarantee that forecasts at different aggregation levels do not contradict each other.  
**Owned Aggregate:** Forecast Publication (SE-D-029).  
**Required Input State:** Draft publication with forecast lines generated.  
**Produced Output State:** Draft publication with reconciled values.  
**Invoked Decisions:** DE-D-025.  
**Invoked Algorithms:** BA-D-003 (when defined).  
**Business Transaction:** Protects Forecast Publication aggregate. Atomic update of reconciled forecast values.  
**Idempotency:** Re-execution reapplies reconciliation; result is deterministic.  
**Concurrency:** Reconciliation for a given publication occurs exactly once.

**Traceability:** Owned by SE-D-029. Invoked by FS-D-013. Invokes DE-D-025.

### AB-F-004 — Record Forecast Override

**Purpose:** Record a planner's replacement of a specific system-generated forecast value within the Draft publication.  
**Business Intent:** Allow human business knowledge to adjust statistical output while preserving the original for audit and accuracy analysis.  
**Owned Aggregate:** Forecast Publication (SE-D-029).  
**Required Input State:** Draft publication with forecasts generated.  
**Produced Output State:** (state unchanged; Override entity added).  
**Invoked Decisions:** DE-D-023.  
**Invoked Algorithms:** None.  
**Published Events:** None (notification published by invoking FS).  
**Business Transaction:** Protects Forecast Publication aggregate. Atomic creation of Forecast Override entity (SE-D-028). Original forecast value preserved unchanged.  
**Idempotency:** Re-execution with the same parameters updates the existing override.  
**Concurrency:** Overrides for different SKU-StockingPoint-time buckets may be processed concurrently.

**Traceability:** Owned by SE-D-029. Invoked by FS-D-010. Invokes DE-D-023.

### AB-F-005 — Publish Forecast Publication

**Purpose:** Publish the Forecast Publication, making it authoritative for downstream planning.  
**Business Intent:** Transfer the latest demand projections to all consuming capabilities.  
**Owned Aggregate:** Forecast Publication (SE-D-029).  
**Required Input State:** Draft publication with forecasts generated and reconciled.  
**Produced Output State:** Published. Previous publication for the same scope Superseded.  
**Invoked Decisions:** DE-D-022.  
**Invoked Algorithms:** None.  
**Published Events:** None (notification published by invoking FS).  
**Business Transaction:** Protects Forecast Publication aggregate. Atomic publication, previous publication superseding, and timestamp recording.  
**Idempotency:** Re-execution on already-published publication terminates immediately.  
**Concurrency:** Publication for a given publication occurs exactly once.

**Traceability:** Owned by SE-D-029. Invoked by FS-D-011. Invokes DE-D-022.

### AB-S-001 — Update Demand Behaviour Assessment

**Purpose:** Evaluate an incoming demand signal against the Demand Baseline and, if warranted, update the Demand Behaviour Assessment’s current state and record a State Change Event.  
**Business Intent:** Maintain the enterprise’s continuously current understanding of demand behaviour, detecting and recording all meaningful changes.  
**Owned Aggregate:** Demand Behaviour Assessment (SE-D-035).  
**Required Input State:** None (the assessment always exists; this behaviour updates it).  
**Produced Output State:** Current State may change; a new State Change Event is appended if a transition occurs.  
**Invoked Decisions:** DE-D-030.  
**Invoked Algorithms:** None.  
**Published Events:** None (notifications published by invoking FS).  
**Business Transaction:** Protects the Demand Behaviour Assessment aggregate. Atomic evaluation of the signal, potential state update, and recording of the State Change Event.  
**Idempotency:** Re-evaluation of the same signal with the same baseline produces the same outcome. Duplicate processing of the same signal is prevented by signal identity.  
**Concurrency:** Signals for different assessments are processed independently. Signals for the same assessment are serialized to maintain a consistent current state.

**Traceability:** Owned by SE-D-035. Invoked by FS-D-014. Invokes DE-D-030.

### AB-SG-001 — Update Planning Classification

**Purpose:** For a given entity and classification type, evaluate the entity against the Segmentation Policy and update its classification if it has changed.  
**Owned Aggregate:** Planning Classification Assignment (SE-D-036).  
**Required Input State:** None (assignment always exists or is created if new).  
**Produced Output State:** Current classification may change; Assignment Change Event appended.  
**Invoked Decisions:** DE-D-032.  
**Invoked Algorithms:** None (classification logic is in the policy rules evaluated by the decision).  
**Business Transaction:** Protects the Planning Classification Assignment aggregate.  
**Idempotency:** Re-evaluation with the same policy and evidence produces the same outcome.  
**Concurrency:** Assignments for different entities or types are independent; same entity+type is serialized.

**Traceability:** Owned by SE-D-036. Invoked by FS-D-016. Invokes DE-D-032.

### AB-CL-001 — Update Behaviour Classification

**Purpose:** For a given entity and behaviour dimension, evaluate the entity against the Classification Policy and update its classification if it has changed.  
**Owned Aggregate:** Demand Behaviour Assignment (SE-D-037).  
**Required Input State:** None (assignment always exists or is created if new).  
**Produced Output State:** Current classification may change; Behaviour Change Event appended.  
**Invoked Decisions:** DE-D-033.  
**Invoked Algorithms:** None (classification logic is in the policy rules evaluated by the decision).  
**Business Transaction:** Protects the Demand Behaviour Assignment aggregate.  
**Idempotency:** Re-evaluation with the same policy and evidence produces the same outcome.  
**Concurrency:** Assignments for different entities or dimensions are independent; same entity+dimension is serialized.

**Traceability:** Owned by SE-D-037. Invoked by FS-D-017. Invokes DE-D-033.

### AB-PR-001 — Update Planning Priority

**Purpose:** For a given planning entity, evaluate the entity against the Prioritization Policy and update its priority if it has changed.  
**Owned Aggregate:** Planning Priority Assignment (SE-D-038).  
**Required Input State:** None (assignment always exists or is created if new).  
**Produced Output State:** Current priority, score, and decision rationale may change; Priority Change Event appended.  
**Invoked Decisions:** DE-D-034.  
**Invoked Algorithms:** None (scoring logic is in the policy rules evaluated by the decision).  
**Business Transaction:** Protects the Planning Priority Assignment aggregate.  
**Idempotency:** Re-evaluation with the same policy and evidence produces the same outcome.  
**Concurrency:** Assignments for different entities are independent; same entity updates are serialized.

**Traceability:** Owned by SE-D-038. Invoked by FS-D-018. Invokes DE-D-034.

### AB-EQ-001 — Publish Forecast Quality Assessment

**Purpose:** Compute forecast quality metrics according to the Forecast Measurement Policy and publish the enterprise assessment if publication criteria are met.  
**Owned Aggregate:** Forecast Quality Assessment (SE-D-039).  
**Required Input State:** None (creation).  
**Produced Output State:** Draft (metrics computed) → Published (if DE-D-035 approves).  
**Invoked Decisions:** DE-D-035.  
**Invoked Algorithms:** BA-D-005–008 (as defined by the Forecast Measurement Policy).  
**Business Transaction:** Protects the Forecast Quality Assessment aggregate.  
**Idempotency:** Re-computation for the same Planning Scope and Evaluation Period produces the same metrics.  
**Concurrency:** Assessments for different Planning Scopes or Evaluation Periods are independent.

**Traceability:** Owned by SE-D-039. Invoked by FS-D-019. Invokes DE-D-035, BA-D-005–008.

### AB-DE-001 — Recognize Demand Planning Condition

**Purpose:** For a given planning entity and condition type, evaluate the current demand information against the Exception Detection Policy. If a condition exists, create or update the Demand Planning Condition. If the condition no longer exists, resolve it.  
**Owned Aggregate:** Demand Planning Condition (SE-D-040).  
**Required Input State:** None (creation if condition detected), Active (update or resolution), or Resolved (no action — terminal).  
**Produced Output State:** Active (new or updated) or Resolved. Condition Change Event appended.  
**Invoked Decisions:** DE-D-036.  
**Invoked Algorithms:** None.  
**Business Transaction:** Protects the Demand Planning Condition aggregate. The behaviour maps the decision outcome to the aggregate action:
- Decision = Condition Exists + no Active condition → Create new Active condition.
- Decision = Condition Exists + Active condition with different severity → Update severity.
- Decision = Condition Exists + Active condition with same severity → No change.
- Decision = No Condition + Active condition → Resolve.
- Decision = No Condition + no Active condition → No action.
**Idempotency:** Re-evaluation with the same evidence and policy produces the same outcome.  
**Concurrency:** Conditions for different entities or types are independent; same entity+type is serialized.

**Traceability:** Owned by SE-D-040. Invoked by FS-D-020. Invokes DE-D-036.

### AB-EX-001 — Record Demand Explanation

**Purpose:** Record an immutable Demand Explanation for a specified artifact. If an explanation already exists for the same artifact under identical reasoning, policy versions, and template version, return the existing explanation rather than creating a duplicate.  
**Owned Aggregate:** Demand Explanation (SE-D-041).  
**Required Input State:** None (creation) or existing explanation (reuse).  
**Produced Output State:** Created (immutable) — or existing explanation returned unchanged.  
**Invoked Decisions:** None (template-driven recording).  
**Invoked Algorithms:** BA-D-009 (Build Reasoning Graph — deferred).  

### BA-D-010 — Discover Demand Learnings (Deferred)

**Note:** The algorithm that analyzes evidence and identifies potential learnings will be specified when the analytical methodology is formalized. Currently inlined within AB-LR-001.

**Business Transaction:** Protects the Demand Explanation aggregate. Atomic creation of the explanation with Structured Reasoning Graph, source artifact references with historical versions, and natural language rendering.  
**Idempotency:** Re-requesting an explanation for the same artifact under identical conditions returns the existing explanation. A new explanation is created only when reasoning, policy versions, or template version materially changes.  
**Concurrency:** Explanations for different artifacts are independent.

**Traceability:** Owned by SE-D-041. Invoked by FS-D-021. Invokes BA-D-009.

### BA-D-010 — Discover Demand Learnings (Deferred)

**Note:** The algorithm that analyzes evidence and identifies potential learnings will be specified when the analytical methodology is formalized. Currently inlined within AB-LR-001.

---

### AB-LR-001 — Record Demand Learning

**Purpose:** Analyze historical performance data, detected patterns, and outcome evidence across all demand capabilities according to the Learning Analysis Policy, and record an immutable Demand Learning when the evidence supports a discovery.  
**Owned Aggregate:** Demand Learning (SE-D-042).  
**Required Input State:** None (creation).  
**Produced Output State:** Created (immutable).  
**Invoked Decisions:** None (policy-driven analysis).  
**Invoked Algorithms:** BA-D-010 (Discover Demand Learnings — deferred).  
**Business Transaction:** Protects the Demand Learning aggregate. Atomic creation of the learning with type (as defined by policy), statement, supporting evidence, and evidence strength (as defined by policy).  
**Idempotency:** Re-analysis with the same evidence and policy produces the same learning. Duplicate learnings for the same discovery and evidence are not created.  
**Concurrency:** Analyses for different learning domains are independent.

**Traceability:** Owned by SE-D-042. Invoked by FS-D-022. Invokes BA-D-010.

---

# Chapter 10 — Functional Specifications

## Understand Demand Functional Specifications

### FS-D-001 — Receive Demand Observation

**Realises:** CR-D-001  
**Business Contract:**
- **Consumes:** Business observation from source systems, or Business Notification indicating demand-relevant information (e.g., BN-D-011 Forecast Published, BN-R-xxx Promise Confirmed, BN-N-xxx Scenario Recommendation Adopted).
- **Produces:** SE-D-001 Demand Observation (Lifecycle State: Received).
- **Transitions:** SE-D-001: (none) → Received.
- **Publishes:** BN-D-005 Demand Observation Received.
- **Invokes:** FS-D-002.
- **Guarantees:** Exactly one Demand Observation established with full provenance. Duplicate business observations rejected.

**Trigger:** Business Observation Available — any situation where the enterprise receives information that may represent demand. This includes external source systems and internal notifications from Forecast Demand, Promise Intelligence, or Scenario Intelligence.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-001 | Observation contains sufficient information for unique identity. |
| (analogue) | Observation does not already exist as Demand Observation. |
| BR-D-003 (analogue) | Referenced SKU exists. |
| BR-D-004 (analogue) | Referenced StockingPoint exists. |
| BR-D-005 (analogue) | Referenced Planning Calendar exists. |

**Semantic Objects:** Read: SE-DI-040, SE-DI-041, SE-DI-050, SE-DI-051. Create: SE-D-001.

**Behaviour:**
1. Invoke AB-D-001 Establish Demand Observation.
2. Publish BN-D-005 Demand Observation Received.

**Business Transaction:** Per AB-D-001 contract. Protects Demand Observation aggregate.

**Postconditions:** SE-D-001 exists in Received state with full provenance. FS-D-002 may execute.

**Failure Behaviour:**
- Business Failure (duplicate observation, missing mandatory information, invalid SKU/StockingPoint): Observation not established. BN-D-005 not published. Source observation rejected. (Permanent, no retry.)
- Operational Failure (source system unavailable during receipt): Observation not established. BN-D-005 not published. Source may resubmit. (Temporary, retryable.)

**Recovery:** Re-execution is idempotent. Duplicate observations detected and rejected.

**Concurrency:** Observations with different business identities processed independently.

**Example:** Input: SKU P-1001, StockingPoint DC-01, Customer CUST-245, Quantity 120 EA, Business Date 14-Jan-2027. Output: SE-D-001 established, Lifecycle Received. Published: BN-D-005.

**Traceability:** Realises CR-D-001. Invokes AB-D-001. Publishes BN-D-005. Invokes FS-D-002.

---

### FS-D-002 — Evaluate Demand Observation

**Realises:** CR-D-002  
**Business Contract:**
- **Consumes:** SE-D-001 in Lifecycle State Received.
- **Produces:** SE-D-001 with updated Lifecycle State and decision traceability.
- **Transitions:** Received → Accepted / Quarantined / Rejected.
- **Publishes:** BN-D-006 (Accepted), BN-D-007 (Accepted With Warning), BN-D-002 (Quarantined), BN-D-003 (Rejected).
- **Invokes:** FS-D-003 (if Accepted).
- **Guarantees:** Observation evaluated exactly once. Decision traceability recorded.

**Trigger:** Completion of FS-D-001.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-013 | Lifecycle State equals Received. |
| BR-D-015 | Demand Observation exists. |
| BR-D-014 | Observation not previously evaluated. |

**Semantic Objects:** Read: SE-D-001, SE-DI-040, SE-DI-041. Update: SE-D-001.

**Behaviour:**
1. Invoke AB-D-002 Evaluate Demand Observation.
2. Publish appropriate notification based on outcome.

**Business Transaction:** Per AB-D-002 contract. Protects Demand Observation aggregate.

**Postconditions:** Observation in final evaluation state. If Accepted, eligible for FS-D-003.

**Failure Behaviour:**
- Business Failure (DE-D-010 cannot produce valid outcome): Observation remains in Received. No notification published. (Permanent, requires manual review per PO-D-008.)
- Operational Failure (external reference data unavailable): Observation remains in Received. No notification published. (Temporary, retryable.)

**Recovery:** Re-execution permitted while Lifecycle is Received (BR-D-021).

**Concurrency:** Observation evaluated exactly once.

**Traceability:** Realises CR-D-002. Invokes AB-D-002. Publishes BN-D-002, BN-D-003, BN-D-006, BN-D-007. Invokes FS-D-003.

---

### FS-D-003 — Determine Planning Scope

**Realises:** CR-D-003  
**Business Contract:**
- **Consumes:** SE-D-001 in Lifecycle State Accepted (unassigned).
- **Produces:** SE-D-002 Planning Scope (if new). SE-D-001 with Planning Scope assigned.
- **Transitions:** SE-D-001: Accepted (unassigned) → Accepted (assigned). SE-D-002: (none) → Active.
- **Publishes:** None.
- **Invokes:** FS-D-004.
- **Guarantees:** Observation assigned to exactly one Planning Scope. Planning Scope identity is unique.

**Trigger:** Completion of FS-D-002 with Accepted outcome.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-013 | Lifecycle State equals Accepted. |
| BR-D-053 | Observation not already assigned a Planning Scope. |

**Semantic Objects:** Read: SE-D-001, SE-DI-040, SE-DI-041. Create: SE-D-002 (if new). Update: SE-D-001.

**Behaviour:**
1. Invoke AB-PS-001 Determine Planning Scope.

**Business Transaction:** Per AB-PS-001 contract. Protects Planning Scope aggregate.

**Postconditions:** Observation assigned to exactly one Planning Scope (BR-D-025). Eligible for FS-D-004.

**Failure Behaviour:**
- Business Failure (Planning Scope identity cannot be constructed): Observation remains Accepted unassigned. (Permanent, requires manual review per PO-D-016.)
- Operational Failure (Reference data unavailable): Observation remains Accepted unassigned. (Temporary, retryable.)

**Recovery:** Re-execution permitted while unassigned (BR-D-054). Idempotent: if already assigned, terminates.

**Concurrency:** Observations for same Planning Scope serialized by arrival order.

**Traceability:** Realises CR-D-003. Invokes AB-PS-001. Invokes FS-D-004.

---

### FS-D-004 — Create or Update Enterprise Demand Picture

**Realises:** CR-D-004  
**Business Contract:**
- **Consumes:** SE-D-001 Accepted and assigned to Planning Scope.
- **Produces:** SE-D-003 Enterprise Demand Picture (new or new version). SE-D-011 Operational Demand (new or revised).
- **Transitions:** SE-D-003: (none) → Draft, or Published → Draft (previous Superseded).
- **Publishes:** None.
- **Invokes:** FS-D-005.
- **Guarantees:** One current EDP exists for Planning Scope. Previous versions preserved.

**Trigger:** Completion of FS-D-003.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-022 | Observation Lifecycle equals Accepted. |
| BR-D-024 | Planning Scope established. |

**Semantic Objects:** Read: SE-D-001, SE-D-002, SE-D-003 (current). Create: SE-D-003 (new version), SE-D-011. Update: SE-D-011 (revised). Archive: Previous EDP version → Superseded.

**Behaviour:**
1. Invoke AB-EDP-001 Revise Enterprise Demand Picture.

**Business Transaction:** Per AB-EDP-001 contract. Protects Enterprise Demand Picture aggregate.

**Postconditions:** EDP exists in Awaiting Planning Demand Calculation. Previous versions preserved.

**Failure Behaviour:**
- Business Failure (observation already incorporated): EDP unchanged. (Permanent, no retry.)
- Operational Failure (unable to create new version): EDP unchanged. (Temporary, retryable.)

**Recovery:** Re-execution creates exactly one revised version.

**Concurrency:** Updates to same Planning Scope serialized.

**Traceability:** Realises CR-D-004. Invokes AB-EDP-001. Invokes FS-D-005.

---

### FS-D-005 — Calculate Planning Demand

**Realises:** CR-D-005  
**Business Contract:**
- **Consumes:** SE-D-003 EDP in Awaiting Planning Demand Calculation. SE-D-012 Historical Planning Adjustments. SE-D-013 Planner Overrides.
- **Produces:** SE-D-010 Planning Demand with full traceability.
- **Transitions:** SE-D-003: Awaiting Planning Demand Calculation → Ready For Publication.
- **Publishes:** None.
- **Invokes:** FS-D-006.
- **Guarantees:** Planning Demand calculated with traceable derivation.

**Trigger:** Completion of FS-D-004.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-032 | EDP Status equals Awaiting Planning Demand Calculation. |
| BR-D-035 | Operational Demand exists. |

**Semantic Objects:** Read: SE-D-003, SE-D-011, SE-D-012, SE-D-013. Create: SE-D-010. Update: SE-D-003.

**Behaviour:**
1. Invoke AB-EDP-002 Calculate Planning Demand.

**Business Transaction:** Per AB-EDP-002 contract. Protects Enterprise Demand Picture aggregate.

**Postconditions:** Planning Demand exists. EDP Status Ready For Publication.

**Failure Behaviour:**
- Business Failure (no effective adjustments/overrides available when required): EDP remains Awaiting Calculation. (Temporary; retry after data available per PO-D-011.)
- Operational Failure (adjustment/override data unavailable): EDP remains Awaiting Calculation. (Temporary, retryable.)

**Recovery:** Re-execution recalculates from same version. Previous partial results discarded.

**Concurrency:** Calculation for given version occurs exactly once.

**Traceability:** Realises CR-D-005. Invokes AB-EDP-002. Invokes FS-D-006.

---

### FS-D-006 — Publish Enterprise Demand Picture

**Realises:** CR-D-006  
**Business Contract:**
- **Consumes:** SE-D-003 EDP in Ready For Publication.
- **Produces:** SE-D-003 EDP Published (authoritative).
- **Transitions:** SE-D-003: Ready For Publication → Published. Previous: Published → Superseded.
- **Publishes:** BN-D-001 Enterprise Demand Picture Published.
- **Invokes:** None (terminal).
- **Guarantees:** Exactly one Published EDP per Planning Scope. Responsibility transfers to consumers.

**Trigger:** Completion of FS-D-005.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-044 | EDP Status equals Ready For Publication. |
| BR-D-046 | Planning Demand exists. |
| BR-D-047 | Planning Demand calculated. |

**Semantic Objects:** Read: SE-D-003, SE-D-010. Update: SE-D-003 (current and previous).

**Behaviour:**
1. Invoke AB-EDP-003 Publish Enterprise Demand Picture.
2. Publish BN-D-001 Enterprise Demand Picture Published.

**Business Transaction:** Per AB-EDP-003 contract. Protects Enterprise Demand Picture aggregate. New version publication, previous version superseding, and notification publication are one atomic business outcome.

**Postconditions:** Exactly one Published EDP for Planning Scope. BN-D-001 published. Responsibility transferred.

**Failure Behaviour:**
- Business Failure (DE-D-012 returns Do Not Publish): EDP remains Ready For Publication. BN-D-001 not published. (Permanent until conditions met per PO-D-015.)
- Operational Failure (notification delivery unavailable): EDP publication succeeds. BN-D-001 delivery retried per its delivery guarantee. (Temporary, retryable for notification only.)

**Recovery:** Re-execution publishes same version. New version not created solely due to prior failure (BR-D-051).

**Concurrency:** Publication for given version occurs exactly once.

**Traceability:** Realises CR-D-006. Invokes AB-EDP-003. Publishes BN-D-001.

---

## Forecast Demand Functional Specifications

### FS-D-007 — Initiate Forecast Cycle

**Realises:** CR-D-007  
**Business Contract:**
- **Consumes:** Scheduled time signal, or Critical demand change notification, or authorised planner request.
- **Produces:** Forecast cycle identity and initial workflow state.
- **Transitions:** None (workflow initiation).
- **Publishes:** BN-D-010 Forecast Cycle Initialised.
- **Invokes:** FS-D-012.
- **Guarantees:** Exactly one forecast cycle initiated with unique identity. No concurrent active cycles for the same scope.

**Trigger:** Scheduled forecast cycle time reached, or Critical demand change notification received, or authorised planner request. Out-of-cycle initiation governed by PO-D-024.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| (internal) | No Forecast Cycle currently in progress for the same scope. |
| (internal) | Forecast Horizon and Time Bucket configuration defined. |

**Semantic Objects:** Read: SE-DI-050, SE-DI-051, Forecast Configuration. Create: None (workflow state only).

**Behaviour:**
1. Invoke AB-F-001 Initiate Forecast Cycle.
2. Publish BN-D-010 Forecast Cycle Initialised.

**Business Transaction:** Per AB-F-001 contract. Protects workflow state.

**Postconditions:** Forecast cycle initiated. Eligible for FS-D-012.

**Failure Behaviour:**
- Business Failure (active cycle already in progress, unauthorised out-of-cycle request): Cycle not initiated. BN-D-010 not published. (Permanent for this trigger; next scheduled trigger will retry.)
- Operational Failure (calendar configuration unavailable): Cycle not initiated. BN-D-010 not published. (Temporary, retryable.)

**Recovery:** Each trigger creates a distinct cycle with a new unique identifier.

**Concurrency:** Only one active cycle permitted. Initiation checks prevent concurrent creation.

**Example:** Scheduled nightly cycle at 02:00 UTC → Cycle FC-2027-042 initiated. BN-D-010 published.

**Traceability:** Realises CR-D-007. Invokes AB-F-001. Publishes BN-D-010. Invokes FS-D-012.

---

### FS-D-012 — Prepare Forecast Context

**Realises:** CR-D-012  
**Business Contract:**
- **Consumes:** External Driver notifications, Forecast Coverage definition, Forecast Configuration.
- **Produces:** SE-D-029 Forecast Publication (Draft) with assumptions and coverage applied.
- **Transitions:** SE-D-029: (none) → Draft.
- **Publishes:** None.
- **Invokes:** FS-D-008.
- **Guarantees:** All relevant business drivers captured as Assumptions. Coverage scope recorded. Draft publication created.

**Trigger:** Completion of FS-D-007.

**Preconditions:** Forecast Configuration and Coverage available. External driver data accessible.

**Semantic Objects:** Read: Forecast Configuration, Forecast Coverage, SE-DI-060 (Demand Drivers). Create: SE-D-029 (Draft), SE-D-027 (multiple).

**Behaviour:**
1. Invoke AB-F-006 Prepare Forecast Context.
   - Load Forecast Configuration and Coverage.
   - Create a Draft Forecast Publication for the defined scope and horizon.
   - Ingest external Demand Drivers; for each, create a Forecast Assumption (Declared state) interpreting its expected impact.
   - Record which SKU-StockingPoint combinations are covered this cycle.

**Business Transaction:** Per AB-F-006 contract. Protects Forecast Publication aggregate.

**Postconditions:** Draft Forecast Publication exists with assumptions and coverage. Eligible for FS-D-008.

**Failure Behaviour:**
- Business Failure (Coverage undefined, no drivers available): Context preparation cannot complete. (Permanent until data available.)
- Operational Failure (external driver system unavailable): Publication remains unchanged. (Temporary, retryable.)

**Recovery:** Re-execution updates assumptions and coverage for the same Draft publication.

**Concurrency:** Context preparation for a given cycle occurs once.

**Traceability:** Realises CR-D-012. Invokes AB-F-006. Invokes FS-D-008.

---

### FS-D-008 — Select Champion Model

**Realises:** CR-D-008  
**Business Contract:**
- **Consumes:** Draft Forecast Publication. Champion and challenger model performance data.
- **Produces:** Champion model selected for the cycle (recorded for later use in publication).
- **Transitions:** None (workflow state update).
- **Publishes:** None.
- **Invokes:** FS-D-009.
- **Guarantees:** Exactly one champion model selected. Selection rationale recorded.

**Trigger:** Completion of FS-D-012.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| (internal) | Draft publication exists. |
| (internal) | At least one champion model registered in the model registry. |

**Semantic Objects:** Read: SE-D-029 (Draft), model registry, model performance metrics. Update: workflow state.

**Behaviour:**
1. Invoke AB-F-002 Select Champion Model.
   - Retrieve current champion identity and performance metrics.
   - Retrieve challenger evaluation results (if available).
   - Execute DE-D-020. If no challengers available, retain current champion.
   - Record selected model identity and selection rationale for this cycle.

**Business Transaction:** Per AB-F-002 contract. Protects workflow state.

**Postconditions:** Champion model selected. Eligible for FS-D-009.

**Failure Behaviour:**
- Business Failure (no champion model registered): Selection cannot proceed. (Permanent; requires model registry remediation.)
- Operational Failure (model performance data unavailable): Selection cannot proceed. (Temporary, retryable.)

**Recovery:** Re-execution on cycle with champion already selected terminates immediately.

**Concurrency:** Champion selection for given cycle occurs exactly once.

**Example:** Current champion Model-A (WAPE 9.1%), challenger Model-B (WAPE 8.2%, p=0.02) → Model-B promoted.

**Traceability:** Realises CR-D-008. Invokes AB-F-002. Invokes FS-D-009.

---

### FS-D-009 — Generate Baseline Forecasts

**Realises:** CR-D-009  
**Business Contract:**
- **Consumes:** Draft Forecast Publication. Cleansed demand history. SKU-StockingPoint master data. Forecast Coverage. SKU Lifecycle Stage (SE-DI-061).
- **Produces:** SE-D-025 Forecast entities within the Draft publication.
- **Transitions:** SE-D-029: Draft populated with forecasts.
- **Publishes:** None.
- **Invokes:** FS-D-013 (if hierarchy defined), FS-D-010.
- **Guarantees:** Every covered series has a forecast or is flagged as unforecastable. Every forecast includes mean, prediction interval, and confidence score. New products receive an explicitly assigned forecast method.

**Trigger:** Completion of FS-D-008.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| (internal) | Draft publication exists. Champion model selected. |
| (internal) | Demand history available for the training window. |

**Semantic Objects:** Read: SE-D-029 (Draft), SE-DI-020 (champion model), demand history data, Forecast Coverage, SE-DI-061 (lifecycle stage). Create: SE-D-025 (multiple). Update: SE-D-029.

**Behaviour:**
1. Invoke AB-F-003 Generate Baseline Forecasts.
   - For each covered series: if insufficient history, execute DE-D-024 to determine new product method; otherwise execute DE-D-021 to generate statistical forecast.
   - Apply lifecycle-aware policies (vary confidence thresholds by product lifecycle stage from SE-DI-061).
   - Flag unforecastable series per PO-D-019.
   - Record generation metadata and overall Forecast Confidence Index.

**Business Transaction:** Per AB-F-003 contract. Protects Forecast Publication aggregate.

**Postconditions:** Draft publication populated with forecast lines. Eligible for FS-D-013 and FS-D-010.

**Failure Behaviour:**
- Business Failure (insufficient demand history for all series): Generation proceeds; series flagged unforecastable per PO-D-019.
- Operational Failure (model execution error): Publication remains unchanged. (Temporary, retryable.)

**Recovery:** Re-execution regenerates all forecasts within the same Draft publication.

**Concurrency:** Generation for given publication occurs exactly once.

**Example:** 5,000 series, Model-B → 4,900 forecast successfully, 100 flagged unforecastable (including 5 new products assigned analog methods). Overall Confidence Index 87%.

**Traceability:** Realises CR-D-009. Invokes AB-F-003. Invokes FS-D-013, FS-D-010.

---

### FS-D-013 — Reconcile Forecast Hierarchy

**Realises:** CR-D-013  
**Business Contract:**
- **Consumes:** Draft Forecast Publication with generated forecasts. Forecast Hierarchy (SE-D-031).
- **Produces:** Reconciled forecast values within the Draft publication.
- **Transitions:** None (publication remains Draft).
- **Publishes:** None.
- **Invokes:** FS-D-010, FS-D-011.
- **Guarantees:** Forecast lines are internally consistent across all hierarchy levels.

**Trigger:** Completion of FS-D-009 (if hierarchy is defined for the enterprise).

**Preconditions:** Draft publication exists with forecast lines. Hierarchy definition available.

**Semantic Objects:** Read: SE-D-029, SE-D-025 (all forecasts), SE-D-031. Update: SE-D-025 (reconciled values).

**Behaviour:**
1. Invoke AB-F-007 Reconcile Forecast Hierarchy.
   - For each hierarchy node, execute DE-D-025 to choose reconciliation method.
   - Apply reconciliation algorithm (BA-D-003) to adjust forecasts.
   - Record reconciliation differences where perfect alignment is not achieved (BR-D-034).

**Business Transaction:** Per AB-F-007 contract. Protects Forecast Publication aggregate.

**Postconditions:** Forecast lines reconciled. Publication remains Draft.

**Failure Behaviour:**
- Business Failure (hierarchy definition missing or incomplete): Reconciliation skipped. Publication proceeds with unreconciled forecasts.
- Operational Failure (algorithm execution error): Reconciliation not applied. (Temporary, retryable.)

**Recovery:** Re-execution reapplies reconciliation. Idempotent.

**Concurrency:** Reconciliation for given publication occurs exactly once.

**Traceability:** Realises CR-D-013. Invokes AB-F-007. Invokes FS-D-010, FS-D-011.

---

### FS-D-010 — Record Forecast Override

**Realises:** CR-D-010  
**Business Contract:**
- **Consumes:** Draft Forecast Publication. Planner-submitted override request for a specific SKU-StockingPoint-time bucket.
- **Produces:** SE-D-028 Forecast Override entity within the Draft publication.
- **Transitions:** None (publication remains Draft).
- **Publishes:** BN-D-012 Forecast Override Recorded.
- **Invokes:** None (may be invoked multiple times before publication).
- **Guarantees:** Override recorded with full traceability. Original system forecast preserved unchanged.

**Trigger:** Authorised planner submits override for a specific forecast within a Draft publication.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| (internal) | Draft publication exists and is not Published. |
| BR-D-042 | Override includes non-empty business justification. |
| BR-D-043 | Override value within configured deviation limit (or Demand Manager approval obtained per PO-D-022). |

**Semantic Objects:** Read: SE-D-029, SE-D-025 (target forecast). Create: SE-D-028. Update: None (original SE-D-025 preserved unchanged).

**Behaviour:**
1. Invoke AB-F-004 Record Forecast Override.
   - Validate planner authorization per PO-D-022.
   - Execute DE-D-023 Evaluate Forecast Override.
   - If Accepted: create SE-D-028. Publish BN-D-012.
   - If Rejected: notify planner of rejection reason.
   - If Revision Requested: return to planner with required changes.

**Business Transaction:** Per AB-F-004 contract. Protects Forecast Publication aggregate.

**Postconditions:** Override recorded. Original system forecast preserved. Override value will be used in the published forecast.

**Failure Behaviour:**
- Business Failure (justification empty, deviation exceeded without authorisation): Override rejected. BN-D-012 not published. (Planner may resubmit corrected override.)
- Operational Failure (unable to record override): Override not recorded. BN-D-012 not published. (Temporary, retryable.)

**Recovery:** Planner may resubmit corrected override. Re-execution with same parameters updates existing override.

**Concurrency:** Overrides for different SKU-StockingPoint-time buckets processed independently.

**Example:** System forecast 250 units (90% PI: 200–300). Planner override 500 units, justification "Confirmed large one-time order from Customer X." Within deviation limit → Accepted. BN-D-012 published.

**Traceability:** Realises CR-D-010. Invokes AB-F-004. Publishes BN-D-012.

---

### FS-D-011 — Publish Forecast Publication

**Realises:** CR-D-011  
**Business Contract:**
- **Consumes:** Draft Forecast Publication with forecasts generated, reconciled, and overrides applied.
- **Produces:** SE-D-029 Published (authoritative). Previous publication for the same scope Superseded.
- **Transitions:** SE-D-029: Draft → Published. Previous publication: Published → Superseded.
- **Publishes:** BN-D-011 Forecast Published.
- **Invokes:** None (terminal).
- **Guarantees:** Exactly one Published Forecast Publication for the scope. Responsibility transfers to consumers. Understand Demand creates forecast-derived demand observations.

**Trigger:** Completion of FS-D-009 (and FS-D-013 if applicable), after override window closes or all expected overrides processed.

**Preconditions:**

| Rule | Requirement |
|------|-------------|
| BR-D-026 | Completeness threshold met (≥ configured % of covered series have valid forecasts). |
| (internal) | Draft publication exists with forecasts generated. |
| (internal) | All submitted overrides processed. |
| PO-D-025 | Forecast Assumptions signed off by department heads. |

**Semantic Objects:** Read: SE-D-029, SE-D-025 (all forecasts), SE-D-027 (assumptions). Update: SE-D-029 (current and previous).

**Behaviour:**
1. Invoke AB-F-005 Publish Forecast Publication.
   - Execute DE-D-022 Approve Forecast Publication.
   - If Publish: transition previous Published publication for the same scope to Superseded. Transition current Draft to Published. Record Publication Time.
   - Publish BN-D-011 Forecast Published.
   - If Require Approval: route to Demand Planner. Pause until approval.
   - If Suppress: mark publication accordingly. Publish BN-D-013.

**Business Transaction:** Per AB-F-005 contract. Protects Forecast Publication aggregate. Publication, previous publication superseding, and notification are one atomic business outcome.

**Postconditions:** Exactly one Published Forecast Publication for the scope. BN-D-011 published. Understand Demand creates forecast-derived demand observations (triggered by BN-D-011 → FS-D-001).

**Failure Behaviour:**
- Business Failure (completeness below threshold, confidence below auto-publication threshold): Publication remains Draft. BN-D-013 published if suppressed; otherwise routed for manual approval per PO-D-020.
- Operational Failure (notification delivery unavailable): Publication succeeds. BN-D-011 delivery retried per its delivery guarantee.

**Recovery:** Re-execution publishes same publication. New publication not created solely due to prior failure.

**Concurrency:** Publication for given publication occurs exactly once.

**Example:** Publication PUB-2027-003, Confidence 87%, Completeness 98% → DE-D-022 returns Publish Automatically. Publication Published. Previous PUB-2027-002 Superseded. BN-D-011 published. Understand Demand initiates FS-D-001 for forecast-derived demand observations.

**Traceability:** Realises CR-D-011. Invokes AB-F-005. Publishes BN-D-011.

---

### FS-D-014 — Update Demand Behaviour Assessment

**Realises:** CR-D-014  
**Business Contract:**
- **Consumes:** Streaming demand signal, Demand Baseline (SE-DI-062).
- **Produces:** Updated Demand Behaviour Assessment (SE-D-035) with possible new State Change Event.
- **Transitions:** SE-D-035: Current State may change per evaluation.
- **Publishes:** BN-D-015 (if state changed) and BN-D-016 (if new state is Critical).
- **Invokes:** FS-D-015 (if state changed to Critical).
- **Guarantees:** Every signal is evaluated against the current baseline. If a state change is warranted, the assessment is updated atomically and notifications are published.

**Trigger:** Streaming demand signal received.

**Preconditions:**
- Signal contains SKU, StockingPoint, quantity, timestamp.
- Demand Baseline exists for the SKU-StockingPoint.

**Semantic Objects:** Read: SE-DI-062 (Demand Baseline). Update: SE-D-035.

**Behaviour:**
1. Invoke AB-S-001 Update Demand Behaviour Assessment.
   - Load current assessment for the SKU-StockingPoint (or create if new to monitoring, with initial state Normal).
   - Execute DE-D-030 to evaluate the signal.
   - If outcome is No Change, terminate.
   - If outcome is a state transition: update Current State, append State Change Event, record deviation, confidence, corroboration.
2. Publish BN-D-015 Demand Behaviour Changed.
3. If new state is Critical, publish BN-D-016 Critical Demand Behaviour Requires Action and invoke FS-D-015.

**Business Transaction:** Per AB-S-001 contract. Protects Demand Behaviour Assessment aggregate.

**Postconditions:** Assessment reflects latest signal evaluation. If state changed, history recorded and notifications published.

**Failure Behaviour:**
- Business Failure (signal invalid, baseline missing): Assessment unchanged. No notification.
- Operational Failure (baseline service unavailable): Assessment unchanged. Retryable.

**Recovery:** Re-evaluation of the same signal is idempotent.

**Concurrency:** Signals for different assessments processed independently.

**Example:** SKU P-1001, StockingPoint DC-01, signal shows +4.2σ deviation, corroborated by POS and web → DE-D-030 returns Critical. Current State set to Critical, State Change Event recorded. BN-D-015 and BN-D-016 published.

**Traceability:** Realises CR-D-014. Invokes AB-S-001. Publishes BN-D-015, BN-D-016. Invokes FS-D-015 (conditional).

---

### FS-D-015 — Trigger Downstream Actions on Critical State

**Realises:** CR-D-015  
**Business Contract:**
- **Consumes:** Demand Behaviour Assessment in Critical state (from FS-D-014).
- **Produces:** Forecast refresh trigger in Forecast Demand.
- **Transitions:** None (cross-capability invocation).
- **Publishes:** None additional.
- **Invokes:** FS-D-007 (Initiate Forecast Cycle) in Forecast Demand.
- **Guarantees:** Critical state changes evaluated for forecast refresh; if criteria met, refresh triggered.

**Trigger:** FS-D-014 detects Critical state.

**Preconditions:** Assessment Current State is Critical. Forecast age exceeds freshness threshold.

**Semantic Objects:** Read: SE-D-035.

**Behaviour:**
1. Execute DE-D-031 Trigger Forecast Refresh on Critical State.
2. If outcome is Trigger refresh, invoke FS-D-007 (Initiate Forecast Cycle) in Forecast Demand with initiation reason = Critical Demand Change.

**Business Transaction:** None (orchestration only).

**Postconditions:** If triggered, a new forecast cycle is initiated in Forecast Demand.

**Failure Behaviour:** If Forecast Demand unavailable, alert is logged; manual follow-up required.

**Recovery:** Retry invocation.

**Concurrency:** Independent per assessment.

**Traceability:** Realises CR-D-015. Invokes FS-D-007. Invokes DE-D-031.

---

### FS-D-016 — Classify Planning Entity

**Realises:** CR-D-016  
**Business Contract:**
- **Consumes:** Entity identifier, classification type, Segmentation Policy (SE-DI-064), demand history (SE-DI-063), master data.
- **Produces:** Updated Planning Classification Assignment (SE-D-036) with possible new classification.
- **Transitions:** SE-D-036: Current Classification may change; Assignment Change Event appended.
- **Publishes:** BN-D-017 Planning Classification Changed (if changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Classification is current per the policy. History preserved.

**Trigger:** Scheduled re-evaluation, policy change, demand behaviour change, new entity registration, or planner override.

**Preconditions:** Segmentation Policy current. Required evidence available per type.

**Semantic Objects:** Read: SE-DI-064, SE-DI-063, SE-DI-040/SE-DI-030. Update: SE-D-036.

**Behaviour:**
1. Invoke AB-SG-001 Update Planning Classification.
   - Load current assignment for the entity and type (or create if new, initial state Unclassified).
   - Execute DE-D-032 Determine Classification.
   - If classification has changed: update Current Classification, append Assignment Change Event with reason and confidence.
2. If classification changed, publish BN-D-017.

**Business Transaction:** Per AB-SG-001 contract.

**Postconditions:** Entity has current classification for the type. History updated if changed.

**Failure Behaviour:** Assignment unchanged. Retryable.

**Recovery:** Re-classification is idempotent.

**Concurrency:** Assignments for different entities or types processed independently; same entity+type updates serialized.

**Example:** SKU P-1001, type ABC, volume 15% of total → A. Previously B → change, BN-D-017 published.

**Traceability:** Realises CR-D-016. Invokes AB-SG-001. Publishes BN-D-017.

---

### FS-D-017 — Classify Demand Behaviour

**Realises:** CR-D-017  
**Business Contract:**
- **Consumes:** Entity identifier, behaviour dimension, Classification Policy (SE-DI-065), demand history (SE-DI-063), master data.
- **Produces:** Updated Demand Behaviour Assignment (SE-D-037) with possible new classification.
- **Transitions:** SE-D-037: Current Classification may change; Behaviour Change Event appended.
- **Publishes:** BN-D-019 (if classification changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Classification is current per the policy. Evidence is recorded. History preserved.

**Trigger:** Scheduled re-evaluation, policy change, demand behaviour change, new entity registration, or planner override.

**Preconditions:** Classification Policy current. Required evidence available per dimension.

**Semantic Objects:** Read: SE-DI-065, SE-DI-063, SE-DI-040, SE-DI-041. Update: SE-D-037.

**Behaviour:**
1. Invoke AB-CL-001 Update Behaviour Classification.
   - Load current assignment for the entity and dimension (or create if new, initial state Unclassified).
   - Execute DE-D-033 Determine Behaviour Classification.
   - If classification has changed: update Current Classification, append Behaviour Change Event with evidence and confidence.
2. If classification changed, publish BN-D-019.

**Business Transaction:** Per AB-CL-001 contract.

**Postconditions:** Entity has current classification for the dimension. Evidence recorded.

**Failure Behaviour:** Assignment unchanged. Retryable.

**Recovery:** Re-classification is idempotent.

**Concurrency:** Assignments for different entities or dimensions processed independently; same entity+dimension updates serialized.

Example: SKU P-1001, StockingPoint DC-01, dimension 'Statistical Pattern': autocorrelation at seasonal lag significant (p<0.01), CV = 0.8 → Seasonal. Previously Continuous → change, BN-D-019 published.

**Traceability:** Realises CR-D-017. Invokes AB-CL-001. Publishes BN-D-019.

---

### FS-D-018 — Prioritize Planning Entity

**Realises:** CR-D-018  
**Business Contract:**
- **Consumes:** Entity identifier, Prioritization Policy (SE-DI-066), segment data, demand history, master data.
- **Produces:** Updated Planning Priority Assignment (SE-D-038) with possible new priority, score, and decision rationale.
- **Transitions:** SE-D-038: Current Priority may change; Priority Change Event appended.
- **Publishes:** BN-D-020 (if priority changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Priority is current per the policy. Decision rationale is preserved. History is preserved.

**Trigger:** Scheduled re-evaluation, policy change, segment change, behaviour change, or planner override.

**Preconditions:** Prioritization Policy current. Mandatory business evidence available.

**Semantic Objects:** Read: SE-DI-066, SE-DI-040/SE-DI-030. Update: SE-D-038.

**Behaviour:**
1. Invoke AB-PR-001 Update Planning Priority.
   - Load current assignment for the entity (or create if new, initial state Unclassified).
   - Execute DE-D-034 Determine Planning Priority.
   - If priority has changed: update Current Priority, Priority Score, Decision Rationale, and Business Validity; append Priority Change Event.
2. If priority changed, publish BN-D-020.

**Business Transaction:** Per AB-PR-001 contract.

**Postconditions:** Entity has current priority with decision rationale and business validity.

**Failure Behaviour:** Assignment unchanged. Retryable.

**Recovery:** Re-evaluation is idempotent.

**Concurrency:** Assignments for different entities processed independently; same entity updates serialized.

Example: SKU P-1001, segment A-X-Gold, significant revenue contribution, contractual SLA → Critical. Rationale: 'Top-5 customer, strategic launch product, contractual SLA requires 98% service level.' Validity: 'Effective during current planning cycle.' Previously High → change, BN-D-020 published.

**Traceability:** Realises CR-D-018. Invokes AB-PR-001. Publishes BN-D-020.

---

### FS-D-019 — Evaluate Forecast Quality

**Realises:** CR-D-019  
**Business Contract:**
- **Consumes:** Published forecast data (SE-DI-068), actual demand data (SE-DI-063), planner override records (SE-DI-069), Forecast Measurement Policy (SE-DI-067).
- **Produces:** Forecast Quality Assessment (SE-D-039) with metrics computed per the policy.
- **Transitions:** SE-D-039: (none) → Draft → Published (if approved).
- **Publishes:** BN-D-021 (if published).
- **Invokes:** None (terminal).
- **Guarantees:** Quality metrics computed according to the Forecast Measurement Policy. If published, the assessment is the authoritative enterprise quality record.

**Trigger:** Scheduled per the Forecast Measurement Policy cadence, or on-demand after sufficient actuals are available.

**Preconditions:** Forecast and actual demand data available for the full evaluation period. Data completeness meets policy threshold.

**Semantic Objects:** Read: SE-DI-068, SE-DI-063, SE-DI-069, SE-DI-067. Create: SE-D-039.

**Behaviour:**
1. Invoke AB-EQ-001 Publish Forecast Quality Assessment.
   - Create Draft assessment.
   - Compute metrics per the Forecast Measurement Policy.
   - Execute DE-D-035.
   - If Publish: transition to Published, record Publication Time, supersede previous version for the **same** Planning Scope and Evaluation Period, publish BN-D-021.
   - If Do Not Publish: assessment remains Draft; Demand Manager notified per PO-D-041.

**Business Transaction:** Per AB-EQ-001 contract.

**Postconditions:** If published, exactly one Published assessment exists for the Planning Scope and Evaluation Period.

Example: Q1 2027, enterprise scope: WAPE 8.5%, Bias +1.2%, Forecast Accuracy 91.5%. Optional: FVA +7.3pp, stability 92%, override effectiveness 52%. Overall Quality Score 78/100. Published. BN-D-021 published.

**Traceability:** Realises CR-D-019. Invokes AB-EQ-001. Publishes BN-D-021.

---

### FS-D-020 — Detect Demand Planning Conditions

**Realises:** CR-D-020  
**Business Contract:**
- **Consumes:** Forecast data, actual demand data, quality assessments, behaviour assessments, Exception Detection Policy (SE-DI-070).
- **Produces:** Demand Planning Condition (SE-D-040) — created, updated, or resolved.
- **Transitions:** SE-D-040: (none) → Active, Active → Active (severity change), Active → Resolved.
- **Publishes:** BN-D-022 (if condition detected or severity changed), BN-D-023 (if condition resolved).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Every condition that meets policy thresholds is recognized. Conditions persist until the underlying data returns to acceptable bounds. Resolved conditions are terminal; recurrence creates a new instance.

**Trigger:** Scheduled evaluation, or event-driven when new forecasts, actuals, quality assessments, or behaviour assessments are published.

**Preconditions:** Exception Detection Policy current. Required demand information available for the evaluation scope.

**Semantic Objects:** Read: SE-DI-070, SE-DI-068, SE-DI-063, SE-D-039, SE-D-037. Create/Update: SE-D-040.

**Behaviour:**
1. For each planning entity and condition type defined in the Exception Detection Policy:
   - Invoke AB-DE-001 Recognize Demand Planning Condition.
   - If condition created: publish BN-D-022.
   - If condition updated (severity changed): publish BN-D-022.
   - If condition resolved: publish BN-D-023.

**Business Transaction:** Per AB-DE-001 contract.

**Postconditions:** All detected conditions are recorded with current state and evidence.

Example: Forecast Bias for SKU Segment A evaluated at 18% (threshold 10%) → Condition Exists, Severity High. No Active condition exists → New Demand Planning Condition created with identifier DPC-0042. BN-D-022 published. Next cycle: Bias = 8% → No Condition. Active condition DPC-0042 → Resolved. BN-D-023 published.

**Traceability:** Realises CR-D-020. Invokes AB-DE-001. Publishes BN-D-022, BN-D-023.

---

### FS-D-021 — Record Demand Explanation

**Realises:** CR-D-021  
**Business Contract:**
- **Consumes:** Artifact to be explained, Explanation Template Catalog (SE-DI-071), source evidence from referenced artifacts with their historical versions.
- **Produces:** Demand Explanation (SE-D-041) — immutable record, or existing explanation returned if reasoning is unchanged.
- **Transitions:** SE-D-041: (none) → Created (or existing returned).
- **Publishes:** BN-D-024 Demand Explanation Recorded.
- **Invokes:** None (terminal).
- **Guarantees:** Structured Reasoning Graph is canonical, deterministic, and carries provenance on every node. All source artifacts are referenced by their historical versions.

**Trigger:** Planner request for a specific artifact, or automatic trigger per PO-D-047 (Critical condition detected, forecast published).

**Preconditions:** Explained artifact exists. Explanation template available for the artifact type. Source evidence accessible with historical versions.

**Semantic Objects:** Read: SE-DI-071, the explained artifact and its referenced evidence (with version history). Create: SE-D-041 (or return existing).

**Behaviour:**
1. Invoke AB-EX-001 Record Demand Explanation.
   - If an existing explanation exists for the same artifact with identical reasoning, policy versions, and template version: return the existing explanation without creating a new one.
   - Otherwise: select the appropriate template; gather source evidence with historical versions; build the Structured Reasoning Graph with provenance on every node; generate the natural language rendering; create the immutable Demand Explanation.
2. Publish BN-D-024.

**Business Transaction:** Per AB-EX-001 contract.

**Postconditions:** Immutable explanation record exists (newly created or previously existing) with Structured Reasoning Graph and historical version references.

Example: Planner requests explanation for Forecast FC-2027-042, SKU P-1001. Template "Forecast Explanation" applied. Structured Reasoning Graph built: nodes for seasonal factor (Statistical provenance), promotion factor (Planner Judgment provenance via override), trend factor (Statistical provenance), with edges showing their contributions. All referenced rules, policies, and models recorded at their January versions. Natural language rendering generated from the graph. BN-D-024 published.

**Traceability:** Realises CR-D-021. Invokes AB-EX-001. Publishes BN-D-024.

---

### FS-D-022 — Record Demand Learning

**Realises:** CR-D-022  
**Business Contract:**
- **Consumes:** All Demand Intelligence semantic objects — quality assessments (SE-D-039), planning condition histories (SE-D-040), explanation records (SE-D-041), classifications, priorities, behaviour assessments, performance data — and the Learning Analysis Policy (SE-DI-072).
- **Produces:** Demand Learning (SE-D-042) — immutable record.
- **Transitions:** SE-D-042: (none) → Created.
- **Publishes:** BN-D-025 Demand Learning Recorded.
- **Invokes:** None (terminal).
- **Guarantees:** Every learning is supported by evidence from at least one completed analysis. Learnings are immutable, permanently retained, and state what was discovered without prescribing actions. The Learning Type and Evidence Strength are defined by the Learning Analysis Policy.

**Trigger:** Scheduled per the Learning Analysis Policy, or event-driven when new quality assessments, resolved conditions, or explanations are available.

**Preconditions:** Learning Analysis Policy current. Sufficient evidence available per the policy’s evidence sufficiency thresholds.

**Semantic Objects:** Read: SE-DI-072, all Demand Intelligence semantic objects. Create: SE-D-042.

**Behaviour:**
1. Invoke AB-LR-001 Record Demand Learning.
   - Analyze evidence across the governed learning scope defined in the Learning Analysis Policy.
   - If the evidence supports a discovery: create the immutable Demand Learning with type (from the policy-governed taxonomy), statement, supporting evidence references, and evidence strength (as defined by the policy).
   - If the evidence does not meet the threshold: no learning created.
2. If a learning was created, publish BN-D-025.

**Business Transaction:** Per AB-LR-001 contract.

**Postconditions:** If created, an immutable learning record exists with supporting evidence and evidence strength.

Example: Analysis of Q1 2027 quality assessments reveals Forecast Bias trending upward for SKU Segment B over 3 consecutive months (Bias: +3.2%, +4.1%, +5.8%). Override analysis shows planner overrides in this segment are value-destroying 68% of the time. Learning: "Planner overrides in Segment B are systematically degrading forecast accuracy. Bias has increased for 3 consecutive months. Override value-destroying rate is 68%." Learning Type: Performance Pattern (as defined by policy). Evidence Strength: High (as defined by policy). BN-D-025 published.

**Traceability:** Realises CR-D-022. Invokes AB-LR-001. Publishes BN-D-025.

---

# Chapter 11 — Business Algorithms

### BA-D-001 — Calculate Planning Demand

**Version:** 1.0  
**Owned By:** Understand Demand (CA-D-001)  

**Algebraic Properties:**

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes | Same inputs always produce same output. |
| Idempotent | Yes | Repeated execution with same inputs produces identical result. |
| Pure | Yes | No side effects; computes only from inputs. |
| Order Sensitive | No | Input order does not affect result. |
| Explainable | Yes | Every calculation can be traced to its inputs. |

**Input Contract:**

| Input | Source | Required | Handling of Missing | Handling of Negative | Unit |
|-------|--------|----------|---------------------|----------------------|------|
| Operational Demand Quantity | SE-D-011 | Yes | Cannot execute; precondition BR-D-035 must be satisfied | Treated as valid (returns/cancellations) | As specified on SE-D-011 |
| Effective Adjustment Quantity | SE-D-012 | No | Treated as zero | Treated as valid | As specified on SE-D-012 |
| Effective Override Quantity | SE-D-013 | No | Treated as zero | Treated as valid | As specified on SE-D-013 |

**Output Contract:**

| Output | Business Meaning | Precision | Rounding |
|--------|------------------|-----------|----------|
| Planning Demand Quantity | Final planning-approved demand for the Planning Period | Same as Operational Demand | Round half up to unit precision |

**Derivation Rule:** BR-D-008.  
**Formula:** Planning Demand = Operational Demand + Σ(Effective Adjustments) + Σ(Effective Overrides)

**Traceability:** Each invocation records algorithm version, all inputs, and computed output for full explainability (BR-D-042-I).

**Traceability:** Owned by CA-D-001. Invoked by AB-EDP-002. Referenced by FS-D-005.

---

### BA-D-003 — Forecast Reconciliation Algorithm (Deferred placeholder)

**Version:** 1.0 (deferred)  
**Algebraic Properties:** Deterministic, Idempotent, Pure, Explainable.  
**Input Contract:** Hierarchy structure, bottom-level forecasts, top-level forecast (if available).  
**Output Contract:** Reconciled forecasts at all levels.  
**Method:** Configurable; default is bottom-up proportional.

---

**BA-D-004 — Maintain Demand Baseline (Deferred)** – Will be specified when the baseline computation is formalised. Currently inlined within the capability.
### BA-D-005 — Compute Accuracy Metrics (Deferred)
### BA-D-006 — Compute Forecast Value Added (Deferred)
### BA-D-007 — Compute Forecast Stability (Deferred)
### BA-D-008 — Compute Override Effectiveness (Deferred)
### BA-D-009 — Build Reasoning Graph (Deferred)
### BA-D-010 — Discover Demand Learnings (Deferred)

**Note:** The algorithm that analyzes evidence and identifies potential learnings will be specified when the analytical methodology is formalized. Currently inlined within AB-LR-001.

---

### Additional Algorithms

BA-D-002 through BA-D-0xx reserved for forecasting, segmentation, classification, and quality evaluation algorithms. Full definitions deferred.

---

## Appendix A — Integration Matrix (Non-Normative)

⚠️ Derived View — Generated from traceability data. Not an independent source of truth.

| Publisher | Notification | Consumer | Behaviour | FS |
|-----------|-------------|----------|-----------|-----|
| Understand Demand | BN-D-001 Enterprise Demand Picture Published | Forecast Demand | Update training data with latest cleansed history. | (data refresh) |
| Understand Demand | BN-D-001 | Segment Demand | Re-evaluate segmentation. | (future) |
| Understand Demand | BN-D-001 | Classify Demand | Re-classify patterns. | (future) |
| Understand Demand | BN-D-001 | Evaluate Demand Quality | Compare against actuals. | (future) |
| Understand Demand | BN-D-001 | Supply Intelligence | Consume for supply planning. | (external) |
| Understand Demand | BN-D-001 | Promise Intelligence | Consume for order promising. | (external) |
| Understand Demand | BN-D-001 | Scenario Intelligence | Consume for scenario planning. | (external) |
| Understand Demand | BN-D-001 | Sense Demand | Refresh Demand Baseline. | (baseline refresh) |
| Forecast Demand | BN-D-010 Forecast Cycle Initialised | Enterprise Monitoring | Track cycle execution. | — |
| Forecast Demand | BN-D-011 Forecast Published | Understand Demand | Create forecast-derived demand observations. | FS-D-001 |
| Forecast Demand | BN-D-011 | Supply Intelligence | Consume for supply planning. | (external) |
| Forecast Demand | BN-D-011 | Promise Intelligence | Consume for order promising. | (external) |
| Forecast Demand | BN-D-011 | Scenario Intelligence | Consume for scenario planning. | (external) |
| Forecast Demand | BN-D-011 | Evaluate Demand Quality | Begin accuracy measurement when actuals arrive. | (future) |
| Forecast Demand | BN-D-011 | Sense Demand | Optionally refresh baseline with latest forecast. | (baseline refresh) |
| Forecast Demand | BN-D-012 Forecast Override Recorded | Evaluate Demand Quality | Track override impact on accuracy. | (future) |
| Sense Demand | BN-D-015 Demand Behaviour Changed | Demand Planners, Detect Demand Exceptions | Review change; initiate investigation if needed. | — |
| Sense Demand | BN-D-016 Critical Demand Behaviour Requires Action | Forecast Demand | Initiate out-of-cycle forecast refresh. | FS-D-007 |
| Segment Demand | BN-D-017 Planning Classification Changed | Forecast Demand | Update model selection for affected entity. | — |
| Segment Demand | BN-D-017 | Prioritize Demand | Recompute priority score. | (future) |
| Segment Demand | BN-D-017 | Inventory Planning | Update inventory policy. | (external) |
| Classify Demand | BN-D-019 Demand Behaviour Classification Changed | Forecast Demand | Update model selection for affected entity. | — |
| Classify Demand | BN-D-019 | Detect Demand Exceptions | Adjust detection thresholds. | (future) |
| Classify Demand | BN-D-019 | Explain Demand | Use evidence for explanation generation. | (future) |
| Classify Demand | BN-D-019 | Prioritize Demand | Adjust planner attention. | (future) |
| Classify Demand | BN-D-019 | Inventory Planning | Update safety stock policy. | (external) |
| Classify Demand | BN-D-019 | Supply Intelligence | Adjust supply parameters. | (external) |
| Classify Demand | BN-D-019 | Scenario Intelligence | Inform scenario assumptions. | (external) |
| Prioritize Demand | BN-D-020 Planning Priority Changed | Demand Planners | Reorder worklist by priority. | — |
| Prioritize Demand | BN-D-020 | Detect Demand Exceptions | Prioritize exception alerts. | (future) |
| Prioritize Demand | BN-D-020 | Forecast Demand | Apply high-priority protection rules. | — |
| Prioritize Demand | BN-D-020 | Inventory Planning | Prioritize allocation decisions. | (external) |
| Prioritize Demand | BN-D-020 | Scenario Intelligence | Assess impact of changes on high-priority items. | (external) |
| Evaluate Demand Quality | BN-D-021 Forecast Quality Assessment Published | Learn From Demand | Trigger model improvement. | (future) |
| Evaluate Demand Quality | BN-D-021 | Explain Demand | Performance context for explanations. | (future) |
| Evaluate Demand Quality | BN-D-021 | Forecast Demand | Champion model performance feedback. | — |
| Evaluate Demand Quality | BN-D-021 | Demand Planners and Managers | Performance dashboards. | — |
| Detect Demand Exceptions | BN-D-022 Demand Planning Condition Detected | Planners (via workflow capability) | Review and take action. | (external) |
| Detect Demand Exceptions | BN-D-022 | Forecast Demand | Model conditions may trigger champion re-evaluation. | — |
| Detect Demand Exceptions | BN-D-022 | Explain Demand | Provide context for explanations. | (future) |
| Detect Demand Exceptions | BN-D-022 | Learn From Demand | Pattern learning for proactive detection. | (future) |
| Detect Demand Exceptions | BN-D-023 Demand Planning Condition Resolved | Planners | Condition cleared; no further action required. | (external) |
| Explain Demand | BN-D-024 Demand Explanation Recorded | Learn From Demand | Analyze explanation quality and completeness. | — |
| Learn From Demand | BN-D-025 Demand Learning Recorded | Forecast Demand | Review performance pattern learnings for model improvement. | — |
| Learn From Demand | BN-D-025 | Detect Demand Exceptions | Review condition pattern learnings for threshold adjustments. | — |
| Learn From Demand | BN-D-025 | Segment Demand | Review classification learnings for policy refinements. | — |
| Learn From Demand | BN-D-025 | Explain Demand | Review explanation quality learnings. | — |
| Learn From Demand | BN-D-025 | Planning Governance | Derive recommendations, actions, or policy changes from learnings. | (external) |
| Promise Intelligence | BN-R-xxx Promise Confirmed | Understand Demand | Update demand line status. | FS-D-001 |
| Scenario Intelligence | BN-N-xxx Scenario Recommendation Adopted | Understand Demand | Update demand assumptions. | FS-D-001 |
| Knowledge Intelligence | BN-K-xxx Calendar Exception Raised | Forecast Demand | Adjust forecast calendar. | FS-D-007 (conditional) |
| Planning Governance | Segmentation Policy Updated | Segment Demand | Trigger reclassification for affected types. | FS-D-016 |
| Planning Governance | Classification Policy Updated | Classify Demand | Trigger reclassification for affected dimensions. | FS-D-017 |
| Planning Governance | Prioritization Policy Updated | Prioritize Demand | Trigger re-evaluation for all entities. | FS-D-018 |
| Various external | Driver notifications | Forecast Demand | Ingest drivers for context preparation. | FS-D-012 |
