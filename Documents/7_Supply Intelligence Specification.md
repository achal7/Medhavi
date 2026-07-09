# Supply Intelligence Specification

**Status:** Draft v1.0  
**Domain Code:** S  
**Governed By:** ARS v1  
**Traceability:** CN‑001  

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Supply Intelligence is the authoritative enterprise domain responsible for developing trusted understanding of supply capabilities, constraints, and plans. Every inventory policy, capacity decision, procurement recommendation, production schedule, distribution allocation, and supplier collaboration activity originates from and is governed by this specification.

Supply Intelligence consumes trusted demand understanding from Demand Intelligence and transforms it into feasible, optimized supply plans that balance service, cost, and risk. It provides the supply‑side foundation upon which order promising, scenario analysis, and enterprise learning depend.

## 1.2 Scope

**Included:** Supply picture management, supply planning across horizons, inventory policy and health, capacity modelling and planning, procurement planning, production scheduling, distribution planning, supplier collaboration, supply change sensing, supply quality evaluation, supply exception detection, supply decision explainability, and continuous supply intelligence learning.

**Excluded:** Demand forecasting, customer order promising, transportation execution, strategic network design, warehouse execution, and manufacturing execution.

## 1.3 Responsibility Boundary

The responsibility of Supply Intelligence begins when supply data enters enterprise evaluation. It continues through supply understanding, supply planning, inventory interpretation, capacity interpretation, procurement planning, production scheduling, distribution planning, supplier collaboration, supply exception detection, supply explainability, and supply learning.

The responsibility of **Understand Supply** ends when the Enterprise Supply Picture has been published and made available for downstream planning.

**Traceability:** Realises CN‑001. Governed by ARS v1.

---

# Chapter 2 — Business Objectives

| ID | Objective | Traceability |
|----|-----------|--------------|
| BO‑S‑001 | Deliver Trusted Supply Understanding | CN‑003, CN‑004, CN‑006 |
| BO‑S‑002 | Optimize Inventory Performance | CN‑002, CN‑012 |
| BO‑S‑003 | Maximize Capacity Utilization | CN‑002, CN‑012 |
| BO‑S‑004 | Ensure Supply Continuity | CN‑002, CN‑006 |
| BO‑S‑005 | Minimize Total Delivered Cost | CN‑002, CN‑012 |
| BO‑S‑006 | Improve Supplier Collaboration | CN‑006, CN‑011 |
| BO‑S‑007 | Increase Planning Automation | CN‑007, CN‑011 |
| BO‑S‑008 | Continuously Improve Supply Intelligence | CN‑012 |

---

# Chapter 3 — Enterprise Measures

| ID | Measure | Produced By |
|----|---------|-------------|
| PI‑S‑002 | Inventory Turnover | Manage Inventory |
| PI‑S‑003 | Days of Supply | Manage Inventory |
| PI‑S‑004 | Fill Rate (Supply) | Evaluate Supply Quality |
| PI‑S‑005 | Capacity Utilization | Manage Capacity |
| PI‑S‑006 | Schedule Adherence | Schedule Production |
| PI‑S‑007 | Supplier On‑Time Delivery | Evaluate Supply Quality |
| PI‑S‑008 | Total Supply Chain Cost | Evaluate Supply Quality |
| PI‑S‑010 | Supply Plan Adherence | Evaluate Supply Quality |
| PI‑S‑011 | Backorder Rate | Evaluate Supply Quality |
| PI‑S‑012 | Stockout Frequency | Evaluate Supply Quality |
| PI‑S‑013 | Excess & Obsolete Inventory | Manage Inventory |
| PI‑S‑015 | Cash‑to‑Cash Cycle Time | Evaluate Supply Quality |

Remaining PIs reserved for future capability realizations.

---

# Chapter 4 — Semantic Model

## 4.1 Enterprise Temporal Semantics

| Temporal Dimension | Business Meaning |
|--------------------|------------------|
| Business Time | When an event occurred in enterprise reality (e.g., goods receipt timestamp). |
| Observation Time | When the enterprise received or recorded the supply data. |
| Transaction Time | When an aggregate was created or revised within Supply Intelligence. |
| Publication Time | When an aggregate became authoritative and visible to consumers. |
| Effective Time | The planning period for which supply information is valid. |

## 4.2 Object Classification

| ARS Classification | Ontology Nature | Ontology Behavior |
|--------------------|-----------------|-------------------|
| Aggregate Root | Record, Scope, State, Projection, Process | Immutable, Versioned, Derived, Authoritative |
| Entity | (inherits from parent Aggregate) | |
| Value Object | — | Immutable |
| Reference Object | — | (owned externally) |

## 4.3 Reference Consistency

Historical supply assertions reference Reference Objects as they existed at Observation Time. Subsequent changes do not retroactively alter historical records.

## 4.4 Enterprise Information Flow

```
Supply Transactions (ERP, WMS, MES, Supplier Portals)
    │
    ▼
Supply Data Record (SE‑S‑010) — evaluated, accepted
    │
    ▼
Enterprise Supply Picture (SE‑S‑001) ─── Consumed by Plan Supply, Manage Inventory,
                                          Manage Capacity, Procure Materials,
                                          Promise Intelligence, Scenario Intelligence
```

## 4.5 Aggregate Roots

### 4.5.1 Enterprise Supply Picture — SE‑S‑001

**Business Intent:** Provide exactly one authoritative supply interpretation for a Planning Scope at any point in time, while preserving the full version history needed for audit, explainability, and downstream planning.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the enterprise's authoritative understanding of current supply — encompassing observed supply facts, committed future supply, and current production capability — for every Planning Scope, based on the best available evidence at the time of publication. |
| Definition | The aggregate root that maintains the authoritative version series of supply understanding for a Planning Scope. Each version represents the enterprise's current understanding of what supply exists, what supply is expected, and what production capability is available, based on accepted supply data. |
| Identity | Planning Scope. Each version within the aggregate receives a monotonically increasing Version Number, starting at 1 and immutable once assigned. |
| Business Owner | Understand Supply (CA‑S‑001) |
| Produced By | AB‑S‑003 (Revise), AB‑S‑005 (Publish) |
| Consumed By | Plan Supply, Manage Inventory, Manage Capacity, Procure Materials, Promise Intelligence, Scenario Intelligence |
| Lifecycle Expectation | Aggregate lifecycle: Active. Version lifecycle: Draft → Published → Superseded. A new version is published when accepted supply data materially changes the enterprise's supply understanding, as defined by PO‑S‑011. Materiality is defined separately for each knowledge category: inventory, commitments, capacity, and orders. |
| Retention Expectation | All versions retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Versioned, Authoritative |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Planning Scope, Version Number | Unique version identity within the aggregate. |
| Observed Supply | Inventory Position | On‑hand, allocated, backorder quantities per product‑location. Current supply that physically exists. |
| Production Capability | Capacity Status | Per‑resource availability and current utilization. Current production capability — influences understanding of what supply can be generated, but is not itself a supply quantity. |
| Supply Commitments | Open Supply Orders | Purchase orders, production orders, transfer orders with expected delivery dates and quantities. |
| Supply Commitments | Supplier Commitments | Committed quantities and expected delivery dates from suppliers. |
| Supporting Assessments | Commitment Reliability | Per‑commitment assessment based on available enterprise evidence, as defined by PO‑S‑012. |
| Supporting Metadata | Supply Data Quality | Per‑source completeness, freshness, accuracy. Supporting publication metadata — describes confidence in the picture, not the picture itself. |
| Traceability | Supply Source Provenance | Per assertion: originating system (ERP, WMS, MES, supplier portal, cycle count). |
| Traceability | Transaction Time, Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Version created, accepted supply data incorporated. | Supply data incorporated. Data quality assessed. Provenance recorded per assertion. |
| Published | Version released as authoritative. | Publication Time recorded. Previous Published version within the same aggregate → Superseded. EV‑S‑004 recorded. BN‑S‑001 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version within the same aggregate. |

**Invariants:**
- Exactly one Published version exists per Planning Scope at any moment.
- A Published version is immutable.
- Only accepted supply data contributes. Rejected or quarantined data does not.
- Every supply assertion records its provenance at the assertion level.

**Business Operations:** Revise, Publish.

**Reconciliation:** Reconciliation Adjustments are transient, deterministic statements of enterprise truth produced when physical count data is compared against system records (e.g., “Physical count found 1,010 units; system record was 1,000; reconciled quantity is 1,010”). They are consumed by the Revise operation (AB‑S‑003) and are not persistent members of the Enterprise Supply Picture. They are modelled as transient capability artefacts, not as an Aggregate Behaviour or Aggregate Root.

**Versioning Trigger:** Material change per knowledge category as defined by PO‑S‑011.

**Traceability:** Business Owner: CA‑S‑001. Produced By: AB‑S‑003, AB‑S‑005. Referenced by: FS‑S‑003, FS‑S‑004. Governed by Chapter 7 rules.

#### Aggregate Behaviours

##### AB‑S‑003 — Revise Enterprise Supply Picture

**Purpose:** Create a new Draft version incorporating accepted supply data and reconciliation adjustments.  
**Business Intent:** Keep the enterprise's supply understanding current by reflecting every accepted supply transaction and reconciled fact.  
**Owned Aggregate:** Enterprise Supply Picture (SE‑S‑001).  
**Required Input State:** None (first version) or Published (subsequent versions).  
**Produced Output State:** Draft.  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑003 (Enterprise Supply Picture Revised).  
**Business Transaction:** Protects Enterprise Supply Picture aggregate. Atomic version creation and data incorporation.  
**Idempotency:** Duplicate version creation prevented by Planning Scope serialization.  
**Concurrency:** Updates to the same Planning Scope serialized.

##### AB‑S‑005 — Publish Enterprise Supply Picture

**Purpose:** Publish the Draft picture, making it authoritative for downstream consumption.  
**Business Intent:** Transfer the latest supply understanding to all consuming capabilities.  
**Owned Aggregate:** Enterprise Supply Picture (SE‑S‑001).  
**Required Input State:** Draft.  
**Produced Output State:** Published. Previous Published version → Superseded.  
**Invoked Decisions:** DE‑S‑011.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑004 (Enterprise Supply Picture Published).  
**Business Transaction:** Protects Enterprise Supply Picture aggregate. Atomic publication and previous version superseding.  
**Idempotency:** Re‑execution on already‑published version terminates immediately.  
**Concurrency:** Publication for a given version occurs exactly once.

### 4.5.2 Supply Data Record — SE‑S‑010

**Business Intent:** Preserve exactly what the enterprise received from a supply source, with enough provenance and evaluation context to support trustworthy downstream supply understanding.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Preserve an immutable enterprise record of a received supply transaction before evaluation. |
| Definition | An enterprise record of supply data received from any source system — inventory update, order status, shipment notification, supplier commitment — captured exactly as received. The received payload and provenance are immutable; evaluation metadata is appended through governed lifecycle transitions. |
| Identity | Supply Data Record Identifier, globally unique, assigned at creation. Immutable. |
| Business Owner | Understand Supply (CA‑S‑001) |
| Produced By | AB‑S‑001 Receive Supply Data |
| Consumed By | Evaluated internally. Accepted records contribute to the Enterprise Supply Picture. |
| Lifecycle Expectation | Received → Accepted / Quarantined / Rejected. Evaluated exactly once from Received. |
| Retention Expectation | Retained permanently for audit. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Record |
| Behavior | Immutable |

**Information Model**

| Category | Information | Mandatory | Immutable |
|----------|-------------|-----------|-----------|
| Identity | Supply Data Record Identifier | Yes | Yes |
| Mandatory | Product, Location | Yes | Yes |
| Mandatory | Supply Type (Inventory, Purchase Order, Production Order, Transfer, Supplier Commitment) | Yes | Yes |
| Mandatory | Quantity, Unit of Measure | Yes | Yes |
| Mandatory | Business Time (when the event occurred in reality) | Yes | Yes |
| Mandatory | Observation Time (when the enterprise received the data) | Yes | Yes |
| Mandatory | Source System Provenance | Yes | Yes |
| Optional | Expected Delivery Date | No | Yes |
| Derived | Evaluation State (Lifecycle State) | No | No |
| Derived | Decision Confidence, Decision Rationale | No | No |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Received | Record established, not yet evaluated. | Identity and mandatory attributes populated. Provenance recorded. |
| Accepted | Passed evaluation, eligible for incorporation. | Decision Identifier, Timestamp, Decision Confidence, Rationale recorded. |
| Quarantined | Failed evaluation; awaiting manual review. | Decision Identifier, Timestamp, Quarantine Reason recorded. |
| Rejected | Permanently excluded. | Decision Identifier, Timestamp, Rejection Reason recorded. |

**Invariants:**
- Evaluated exactly once from Received.
- Only Accepted records contribute to the Enterprise Supply Picture.

**Business Operations:** Receive, Evaluate.

**Decisions Owned:** DE‑S‑010 Accept Supply Data.

**Traceability:** Business Owner: CA‑S‑001. Produced By: AB‑S‑001. Referenced by: FS‑S‑001, FS‑S‑002. Governed by BR‑S‑010–013.

#### Aggregate Behaviours

##### AB‑S‑001 — Receive Supply Data

**Purpose:** Establish a Supply Data Record from a received supply transaction.  
**Business Intent:** Create an immutable enterprise record of exactly what was received, before any evaluation.  
**Owned Aggregate:** Supply Data Record (SE‑S‑010).  
**Required Input State:** None (creation).  
**Produced Output State:** Received.  
**Invoked Decisions:** None.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑001 (Supply Data Record Received).  
**Business Transaction:** Protects Supply Data Record aggregate. Atomic creation of identity, mandatory attributes, and provenance.  
**Idempotency:** Re‑execution with the same business identity produces no duplicate.  
**Concurrency:** Records with different identities processed independently.

##### AB‑S‑002 — Evaluate Supply Data

**Purpose:** Evaluate a Supply Data Record against acceptance criteria.  
**Business Intent:** Determine whether a supply record is trustworthy enough to contribute to the Enterprise Supply Picture.  
**Owned Aggregate:** Supply Data Record (SE‑S‑010).  
**Required Input State:** Received.  
**Produced Output State:** Accepted, Quarantined, or Rejected.  
**Invoked Decisions:** DE‑S‑010.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑002 (Supply Data Record Evaluated).  
**Business Transaction:** Protects Supply Data Record aggregate. Atomic state transition and decision traceability recording.  
**Idempotency:** Re‑execution on already‑evaluated record terminates immediately.  
**Concurrency:** Evaluated exactly once per record.

### 4.5.3 Supply Plan — SE‑S‑020

**Business Intent:** Provide exactly one authoritative supply planning baseline for a Planning Scope and horizon, so downstream supply capabilities work from the same governed interpretation of how demand will be satisfied.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the enterprise's authoritative, feasible, and balanced plan of how it intends to satisfy demand across the supply network, respecting constraints and balancing service, cost, and risk. |
| Definition | The aggregate root that maintains the authoritative version series of supply plans for a defined Planning Scope and horizon. Each plan version represents the enterprise's current planning baseline for supply execution — specifying planned production, procurement, transfers, projected inventory, projected shortfall, constraint status, and planning assumptions. All downstream supply capabilities consume this baseline rather than independently determining supply requirements. |
| Identity | Planning Scope + Plan Horizon. Each plan version receives a globally unique Supply Plan Version Identifier and a monotonically increasing Version Number within the aggregate. |
| Business Owner | Plan Supply (CA‑S‑002) |
| Produced By | AB‑S‑010 (Generate Supply Plan), AB‑S‑011 (Evaluate Supply Plan), AB‑S‑012 (Publish Supply Plan) |
| Consumed By | Manage Inventory (projected inventory positions), Manage Capacity (resource utilization), Procure Materials (planned procurement quantities), Schedule Production (planned production quantities), Manage Distribution (planned transfers), Promise Intelligence (ATP/CTP evaluation), Scenario Intelligence (baseline for scenario comparison) |
| Lifecycle Expectation | Aggregate lifecycle: Active. Version lifecycle: Draft → Published → Superseded. A new version is produced each planning cycle. The previous Published version within the same aggregate is superseded. All versions retained permanently. |
| Retention Expectation | All published plans retained permanently for audit and plan accuracy measurement. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Versioned, Authoritative |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Planning Scope, Plan Horizon, Supply Plan Version Identifier, Version Number | Unique enterprise identity for this plan version within the aggregate. |
| Mandatory | Planning Scope | The set of product‑location‑resource‑supplier combinations covered by this plan. |
| Mandatory | Plan Horizon | Start and end dates defining the planning window. |
| Mandatory | Time Bucket configuration | The time granularity (daily, weekly) of planned quantities. |
| Mandatory | Lifecycle State | Draft, Published, or Superseded. |
| Mandatory | Version number | Monotonically increasing. |
| Planned Supply | Planned Production Quantities | Production the enterprise intends to execute, per product, resource, location, and time bucket. |
| Planned Supply | Planned Procurement Quantities | Purchases the enterprise intends to make, per product, supplier, and time bucket. |
| Planned Supply | Planned Transfer Quantities | Internal movements the enterprise intends to execute, per product, source, destination, and time bucket. |
| Projected State | Projected Inventory | Expected on‑hand inventory after all planned supply and demand consumption. Provides the planning baseline for Inventory Management. |
| Projected State | Projected Shortfall | Demand expected to remain unsatisfied after all feasible supply has been planned. Serves as enterprise input for Promise Intelligence, Scenario Intelligence, and planner intervention. |
| Supply‑Demand Balance | Satisfied Demand, Delayed Demand, Unmet Demand, Excess Supply, Projected Inventory | The complete reconciliation of demand and supply per period. |
| Constraint Status | Resource Utilization vs. Capacity, Constraint Status (Feasible, Near Capacity, Overloaded), Documented Constraint Relaxations | The enterprise's understanding of supply feasibility for each constrained resource. |
| Planning Assumptions | Supplier lead time assumptions, capacity availability assumptions, sourcing assumptions, inventory assumptions, demand assumptions inherited from Demand Intelligence | The key assumptions underpinning the plan. Planning Assumptions do not represent enterprise truth — they represent assumptions accepted during plan generation. They are essential for explainability, scenario comparison, and later learning. |
| Supporting Assessment | Plan Confidence Score | Derived from data quality, supplier reliability, capacity certainty, and constraint satisfaction. |
| Traceability | Transaction Time, Publication Time, Superseded Plan Identifier | Timestamps and version lineage. |
| Traceability | Source Demand Reference (Forecast Publication ID) | The demand forecast this plan was generated against. |
| Traceability | Source Supply Reference (Enterprise Supply Picture ID) | The supply position used as the planning baseline. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Plan created, all quantities and projections populated. | Planned supply, projected inventory, constraint status, supply‑demand balance, and planning assumptions computed. Confidence score computed. |
| Published | Plan version released as the authoritative enterprise planning baseline for all downstream supply capabilities. | Publication Time recorded. Previous Published version within the same aggregate → Superseded. EV‑S‑012 recorded. BN‑S‑010 published. Responsibility for supply execution transfers to downstream capabilities. |
| Superseded | Replaced by a newer Published plan version. | State set on the older version within the same aggregate. |

**Invariants:**
- Exactly one Published Supply Plan exists for a given Planning Scope at any moment.
- A Published plan is immutable. Any change requires a new plan produced by a new planning run.
- All hard constraints must be satisfied or documented and approved as constraint relaxations.
- The plan must cover the full planning horizon for all mandatory product‑location combinations.

**Business Operations:** Balance Supply and Demand (enterprise responsibility), Evaluate, Publish.

**Traceability:** Business Owner: CA‑S‑002. Produced By: AB‑S‑010–012. Referenced by: FS‑S‑010–012. Governed by BR‑S‑020–029.

#### Aggregate Behaviours

##### AB‑S‑010 — Generate Supply Plan

**Purpose:** Execute the supply planning logic to produce a Draft Supply Plan with all planned quantities, projected inventory, constraint status, and supply‑demand balance.  
**Owned Aggregate:** Supply Plan (SE‑S‑020).  
**Required Input State:** None (creation).  
**Produced Output State:** Draft.  
**Invoked Decisions:** None (solver selection governed by policy, not a business decision).  
**Invoked Algorithms:** BA‑S‑001 (Supply Optimization — deferred).  
**Published Events:** EV‑S‑010 (Supply Plan Generated).  
**Business Transaction:** Protects Supply Plan aggregate. Atomic creation of all planned quantities and projections.  
**Idempotency:** Re‑execution with same inputs produces identical plan (deterministic).  
**Concurrency:** Generation for a given Planning Scope serialized.

##### AB‑S‑011 — Evaluate Supply Plan

**Purpose:** Assess the Draft Supply Plan against business quality KPIs and stability rules.  
**Owned Aggregate:** Supply Plan (SE‑S‑020).  
**Required Input State:** Draft.  
**Produced Output State:** Draft (evaluated; acceptance decision recorded).  
**Invoked Decisions:** DE‑S‑020.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑011 (Supply Plan Evaluated).  
**Business Transaction:** Protects Supply Plan aggregate. Evaluation result recorded atomically.  
**Idempotency:** Re‑evaluation produces same outcome.

##### AB‑S‑012 — Publish Supply Plan

**Purpose:** Publish the accepted Supply Plan, making it authoritative for downstream execution.  
**Owned Aggregate:** Supply Plan (SE‑S‑020).  
**Required Input State:** Draft (accepted).  
**Produced Output State:** Published. Previous Published version → Superseded.  
**Invoked Decisions:** DE‑S‑021.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑012 (Supply Plan Published).  
**Business Transaction:** Protects Supply Plan aggregate. Atomic publication and previous version superseding.  
**Idempotency:** Re‑execution on already‑published version terminates immediately.  
**Concurrency:** Publication for a given version occurs exactly once.

### 4.5.4 Inventory Position Assessment — SE‑S‑030

**Business Intent:** Provide a continuously current enterprise interpretation of what each active product‑location inventory position means for service, risk, financial exposure, and policy compliance.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Interpret current and projected inventory for an active product‑location. |
| Definition | The aggregate root that maintains the current inventory position assessment for one active product‑location, including coverage, risk, financial exposure, policy compliance, and immutable assessment change history. |
| Identity | Product + Location. |
| Business Owner | Manage Inventory (CA‑S‑003) |
| Produced By | AB‑S‑020 (Update Inventory Position Assessment) |
| Consumed By | Procure Materials, Evaluate Supply Quality, Learn From Supply |
| Lifecycle Expectation | Active → Archived. An assessment remains Active while the product‑location remains active for planning. |
| Retention Expectation | All assessment states and change events retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Product, Location | Product‑location being assessed. |
| Current Position | On‑hand, allocated, backorder, available quantity | Current inventory position derived from the latest published Enterprise Supply Picture. |
| Projected Position | Projected inventory, projected stockout period, projected excess | Future inventory interpretation derived from the latest published Supply Plan. |
| Interpretation | Days of Supply, coverage status, risk level, health status, financial exposure | Business interpretation used by downstream capabilities. |
| Governance | Policy compliance status, policy reference | Relationship to current Inventory Policy Assignment. |
| Traceability | Source Enterprise Supply Picture ID, Source Supply Plan ID, assessment timestamp | Evidence used to produce the assessment. |
| History | Assessment Change Events | Immutable record of assessment changes. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Product‑location is actively assessed. | Current interpretation and change history maintained. |
| Archived | Product‑location is no longer active for planning. | Final state retained for audit; no new assessment changes appended. |

**Invariants:**
- At any moment, an active product‑location has exactly one current Inventory Position Assessment.
- The assessment is derived from the latest published Enterprise Supply Picture and Supply Plan available at evaluation time.
- Assessment Change Events are immutable.

**Business Operations:** Update Assessment, Archive.

**Traceability:** Business Owner: CA‑S‑003. Produced By: AB‑S‑020. Referenced by: FS‑S‑020. Governed by BR‑S‑040–045.

#### Aggregate Behaviours

##### AB‑S‑020 — Update Inventory Position Assessment

**Purpose:** Re‑evaluate and update the Inventory Position Assessment for a product‑location based on the latest Enterprise Supply Picture and Supply Plan.  
**Owned Aggregate:** Inventory Position Assessment (SE‑S‑030).  
**Required Input State:** Active.  
**Produced Output State:** Active (health status, risk levels may change; Assessment Change Event appended).  
**Invoked Decisions:** DE‑S‑030.  
**Invoked Algorithms:** BA‑S‑004 (Evaluate Inventory Risk — deferred).  
**Published Events:** EV‑S‑020 (Inventory Position Assessment Changed).  
**Business Transaction:** Protects Inventory Position Assessment aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same result.  
**Concurrency:** Assessments for different product‑locations processed independently.

### 4.5.5 Inventory Policy Assignment — SE‑S‑031

**Business Intent:** Ensure every active product‑location has exactly one current, governed inventory policy that downstream replenishment decisions can rely on.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Maintain governed replenishment policy parameters for an active product‑location. |
| Definition | The aggregate root that maintains the current inventory policy assignment for one active product‑location, including policy type, safety stock, reorder point, lot size, review period, service target, and immutable policy change history. |
| Identity | Product + Location. |
| Business Owner | Manage Inventory (CA‑S‑003) |
| Produced By | AB‑S‑021 (Update Inventory Policy) |
| Consumed By | Procure Materials, Evaluate Supply Quality, Learn From Supply |
| Lifecycle Expectation | Active → Archived. An assignment remains Active while the product‑location remains active for planning. |
| Retention Expectation | All policy assignments and policy change events retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Product, Location | Product‑location governed by the policy. |
| Policy Parameters | Policy Type, Safety Stock, Reorder Point, Lot Size, Review Period | Current replenishment policy parameters. |
| Governance | Service Level Target, Cost Parameters, Override Status, Justification | Governed policy context and approved exceptions. |
| Inputs | Demand variability, lead time, supply variability, segmentation class | Inputs used to determine policy. |
| Traceability | Source Demand Forecast ID, Source Supply Plan ID, Source Enterprise Supply Picture ID, policy calculation timestamp | Evidence used to determine the policy. |
| History | Policy Change Events | Immutable record of policy changes. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Product‑location has a current governed policy. | Policy parameters and policy change history maintained. |
| Archived | Product‑location is no longer active for planning. | Final policy retained for audit; no new policy changes appended. |

**Invariants:**
- At any moment, an active product‑location has exactly one current Inventory Policy Assignment.
- Policy Change Events are immutable.
- Planner overrides require justification and periodic review.

**Business Operations:** Update Policy, Archive.

**Traceability:** Business Owner: CA‑S‑003. Produced By: AB‑S‑021. Referenced by: FS‑S‑021. Governed by BR‑S‑046–048 and PO‑S‑033.

#### Aggregate Behaviours

##### AB‑S‑021 — Update Inventory Policy

**Purpose:** Recalculate and update the inventory policy parameters for a product‑location according to the Inventory Policy Governance.  
**Owned Aggregate:** Inventory Policy Assignment (SE‑S‑031).  
**Required Input State:** Active.  
**Produced Output State:** Active (policy parameters may change; Policy Change Event appended).  
**Invoked Decisions:** DE‑S‑031.  
**Invoked Algorithms:** BA‑S‑002 (Compute Safety Stock — deferred), BA‑S‑003 (Compute Reorder Point — deferred).  
**Published Events:** EV‑S‑021 (Inventory Policy Assignment Updated).  
**Business Transaction:** Protects Inventory Policy Assignment aggregate.  
**Idempotency:** Re‑calculation with same inputs produces same result.  
**Concurrency:** Assignments for different product‑locations processed independently.

### 4.5.6 Inventory Health Assessment — SE‑S‑032

**Business Intent:** Provide a periodic, authoritative management view of inventory health for a defined scope and evaluation period.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Publish periodic inventory health understanding for management oversight, trend analysis, and supply quality evaluation. |
| Definition | The aggregate root that maintains the published inventory health assessment version series for an assessment scope and evaluation period. Each version contains health classifications, risk summaries, financial summaries, and evidence references. |
| Identity | Assessment Scope + Evaluation Period. Each assessment version receives a globally unique Assessment Version Identifier and a monotonically increasing Version Number within the aggregate. |
| Business Owner | Manage Inventory (CA‑S‑003) |
| Produced By | AB‑S‑022 (Publish Inventory Health Assessment) |
| Consumed By | Procure Materials, Evaluate Supply Quality, Learn From Supply |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published assessments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Assessment Scope, Evaluation Period, Assessment Version Identifier, Version Number | Unique version identity within the aggregate. |
| Health Classification | Optimal, Under‑Stocked, Over‑Stocked, At Risk, Obsolete | Classification per product‑location. |
| Risk Summary | Stockout risk, excess risk, obsolescence risk, supplier risk | Aggregated risk interpretation for the scope. |
| Financial Summary | Inventory value, excess exposure, shortage exposure | Financial interpretation for management oversight. |
| Evidence | Source Inventory Position Assessment IDs, Source Inventory Policy Assignment IDs | Evidence used to produce the assessment. |
| Traceability | Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Assessment computed for the evaluation period. | Health classification, risk summary, and financial summary populated. |
| Published | Assessment version released as authoritative for the scope and period. | Publication Time recorded. Previous Published version within the same aggregate → Superseded. EV‑S‑022 recorded. BN‑S‑022 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version within the same aggregate. |

**Invariants:**
- Exactly one Published Inventory Health Assessment exists for a given scope and evaluation period.
- A Published Health Assessment is immutable.
- Every classification references the Inventory Position Assessment and Inventory Policy Assignment evidence used.

**Business Operations:** Publish Assessment.

**Traceability:** Business Owner: CA‑S‑003. Produced By: AB‑S‑022. Referenced by: FS‑S‑022. Governed by BR‑S‑049–051 and PO‑S‑034.

#### Aggregate Behaviours

##### AB‑S‑022 — Publish Inventory Health Assessment

**Purpose:** Compute inventory health classifications for all covered product‑locations and publish the periodic, versioned assessment.  
**Owned Aggregate:** Inventory Health Assessment (SE‑S‑032).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑032.  
**Invoked Algorithms:** BA‑S‑005 (Classify Inventory Health — deferred).  
**Published Events:** EV‑S‑022 (Inventory Health Assessment Published).  
**Business Transaction:** Protects Inventory Health Assessment aggregate.  
**Idempotency:** Re‑assessment for same scope/period produces same results.  
**Concurrency:** Independent per scope.

### 4.5.7 Capacity Position Assessment — SE‑S‑040

**Business Intent:** Provide a continuously current enterprise interpretation of resource capacity adequacy, constraint status, bottleneck risk, flexibility, and confidence for every constrained resource.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Interpret current and projected capacity utilization for an active resource, identifying constraints, bottlenecks, risks, flexibility, and the composition of load consuming capacity. |
| Definition | The aggregate root that maintains the current capacity position assessment for one active resource. It interprets Available Capacity (installed capacity less planned downtime and maintenance) against Planned Load (production orders, planned production, maintenance, and reserved capacity) over the time buckets defined by the Planning Calendar. It records how the enterprise arrived at its understanding (explainability) and accommodates both current‑state interpretation and future predictive signals (emerging bottleneck, predicted overload). |
| Identity | Resource (any capacity‑constrained enterprise object: machine, line, cell, work center, production area). One assessment per active resource. |
| Business Owner | Manage Capacity (CA‑S‑004) |
| Produced By | AB‑S‑030 (Update Capacity Position Assessment) |
| Consumed By | Plan Supply (operational constraint feedback), Schedule Production (resource availability and flexibility), Evaluate Supply Quality (capacity effectiveness) |
| Lifecycle Expectation | Active → Archived. An assessment remains Active while the resource remains active for planning. |
| Retention Expectation | All assessment states and change events retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Resource | The constrained resource being assessed. |
| Current Position | Available Capacity (installed capacity less planned downtime/maintenance), Current Utilization, Current Load | Derived from the latest published Enterprise Supply Picture and resource calendars (shift, working, maintenance, holiday). |
| Projected Position | Projected Load, Projected Utilization, Projected Overload Periods, Projected Underload Periods, Constraint Horizon (when constraints exist) | Derived from the latest published Supply Plan. Time‑phased across the planning buckets (daily, weekly, monthly). |
| Load Composition | Production Orders, Planned Production, Maintenance, Reserved Capacity | What is consuming capacity. Enriches explainability. |
| Interpretation | Utilization Status (Feasible, Near Capacity, Overloaded, Underutilized) per time bucket, Constraint Status (per‑period load vs. capacity), Bottleneck Assessment (whether the resource constrains overall system throughput) | Business interpretation. Constraint and bottleneck are distinct concepts. |
| Flexibility | Alternate Resources, Overtime Feasibility, Subcontracting Options, Extra Shift Feasibility, Cross‑Trained Labor, Resource Substitutability | Capacity flexibility interpretation. |
| Risk | Overload Risk, Underload Risk, Bottleneck Risk, Single Point of Failure Risk, Maintenance Dependency Risk, Labor Availability Risk | Structured risk assessment. Risk is a first‑class dimension. |
| Confidence | Assessment Confidence (influenced by missing resource calendars, estimated routing times, uncertain labor availability, incomplete maintenance plans) | Placeholder for future confidence modelling. |
| Traceability | Source Enterprise Supply Picture ID, Source Supply Plan ID, assessment timestamp | Evidence used to produce the assessment. |
| History | Assessment Change Events (each records the reason and evidence for the change, e.g., “Supply Plan v14 added 200 hours of production load to WC‑100, moving it from Feasible to Overloaded”) | Immutable record of assessment changes with explainability. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Resource is actively assessed. | Current interpretation and change history maintained. |
| Archived | Resource is no longer active for planning. | Final state retained for audit; no new assessment changes appended. |

**Invariants:**
- At any moment, an active resource has exactly one current Capacity Position Assessment.
- The assessment is derived from the latest published Enterprise Supply Picture, Supply Plan, and resource calendars available at evaluation time.
- Assessment Change Events are immutable.

**Business Operations:** Update Assessment, Archive.

**Traceability:** Business Owner: CA‑S‑004. Produced By: AB‑S‑030. Referenced by: FS‑S‑030. Governed by BR‑S‑060–065 and PO‑S‑050.

#### Aggregate Behaviours

##### AB‑S‑030 — Update Capacity Position Assessment

**Purpose:** Re‑evaluate and update the Capacity Position Assessment for a resource based on the latest Enterprise Supply Picture, Supply Plan, and resource calendars.  
**Owned Aggregate:** Capacity Position Assessment (SE‑S‑040).  
**Required Input State:** Active.  
**Produced Output State:** Active (utilization status, constraint classification, bottleneck assessment, risk levels, flexibility interpretation may change; Assessment Change Event appended with reason and evidence).  
**Invoked Decisions:** DE‑S‑040.  
**Invoked Algorithms:** BA‑S‑010 (Evaluate Capacity Risk — deferred).  
**Published Events:** EV‑S‑030 (Capacity Position Assessment Changed).  
**Business Transaction:** Protects Capacity Position Assessment aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same result.  
**Concurrency:** Assessments for different resources processed independently.

### 4.5.8 Capacity Health Assessment — SE‑S‑041

**Business Intent:** Provide a periodic, authoritative management view of capacity health, stability, and volatility across a defined scope, enabling investment decisions, operational improvement identification, and trend analysis.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Publish periodic capacity health understanding for management oversight, investment and operational improvement planning, and supply quality evaluation. |
| Definition | The aggregate root that maintains the published capacity health assessment version series for an assessment scope and evaluation period. Each version contains utilization summaries, constraint analyses, bottleneck impacts, stability and volatility metrics, risk summaries, capacity investment and operational improvement signals, and evidence references. |
| Identity | Assessment Scope + Evaluation Period. Each assessment version receives a globally unique Assessment Version Identifier and a monotonically increasing Version Number within the aggregate. |
| Business Owner | Manage Capacity (CA‑S‑004) |
| Produced By | AB‑S‑031 (Publish Capacity Health Assessment) |
| Consumed By | Evaluate Supply Quality (capacity effectiveness trends), Learn From Supply (constraint pattern learning), Management (dashboards) |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published assessments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Assessment Scope, Evaluation Period, Assessment Version Identifier, Version Number | Unique version identity within the aggregate. |
| Utilization Summary | Average utilization, peak utilization, underutilized resource count, overloaded resource count | Aggregated utilization metrics for the scope. |
| Stability & Volatility | Capacity Stability (frequency of state changes), Capacity Volatility (amplitude of utilization swings) | Management metrics for capacity predictability. |
| Constraint Analysis | Binding constraints, bottleneck resources, throughput impact | Enterprise understanding of constraints limiting supply output. |
| Risk Summary | Overload risk exposure, underload cost exposure, bottleneck impact | Aggregated risk interpretation for management oversight. |
| Investment & Improvement Signals | Chronic overload resources, chronic underutilized resources, recurring bottlenecks, unstable utilization patterns, load balancing opportunities, routing optimization opportunities, flexibility improvement opportunities, expansion/contraction candidates | Forward‑looking signals for capacity planning and operational improvement. |
| Evidence | Source Capacity Position Assessment IDs | Evidence used to produce the assessment. |
| Traceability | Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Assessment computed for the evaluation period. | Utilization summary, stability/volatility, constraint analysis, risk summary, and investment/improvement signals populated. |
| Published | Assessment version released as authoritative for the scope and period. | Publication Time recorded. Previous Published version within the same aggregate → Superseded. EV‑S‑031 recorded. BN‑S‑031 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version within the same aggregate. |

**Invariants:**
- Exactly one Published Capacity Health Assessment exists for a given scope and evaluation period.
- A Published Health Assessment is immutable.
- Every assessment references the Capacity Position Assessments used as evidence.

**Business Operations:** Publish Assessment.

**Traceability:** Business Owner: CA‑S‑004. Produced By: AB‑S‑031. Referenced by: FS‑S‑031. Governed by BR‑S‑066–068 and PO‑S‑050.

#### Aggregate Behaviours

##### AB‑S‑031 — Publish Capacity Health Assessment

**Purpose:** Compute capacity health classifications, stability/volatility metrics, and investment/improvement signals for all covered resources and publish the periodic, versioned assessment.  
**Owned Aggregate:** Capacity Health Assessment (SE‑S‑041).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑041.  
**Invoked Algorithms:** BA‑S‑011 (Classify Capacity Health — deferred).  
**Published Events:** EV‑S‑031 (Capacity Health Assessment Published).  
**Business Transaction:** Protects Capacity Health Assessment aggregate.  
**Idempotency:** Re‑assessment for same scope/period produces same results.  
**Concurrency:** Independent per scope.

### 4.5.9 Supplier Commitment — SE‑S‑050

**Business Intent:** Maintain the authoritative enterprise record of a planning collaboration agreement between the enterprise and a specific supplier for a specific supply requirement—capturing both the enterprise’s request and the supplier’s response, together with any alternative proposals, assumptions, and confidence.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Record the shared planning understanding between the enterprise and a supplier for a product, quantity, and required date, including the enterprise’s request, the supplier’s response, and any alternative proposals or assumptions. |
| Definition | The aggregate root that maintains the complete bidirectional collaboration record for one supply requirement. It tracks the enterprise’s request, the supplier’s response (including commitment state, committed quantities/dates, and alternatives), the confidence the enterprise places in that commitment, any supplier‑declared assumptions, and the immutable collaboration history showing the full conversation. |
| Identity | Supplier + Product + Required Date. The identity model is intentionally extensible: future versions may incorporate ship‑to location, planning scope, purchase organization, or delivery window if needed without changing the core design. |
| Business Owner | Collaborate with Suppliers (CA‑S‑005) |
| Produced By | AB‑S‑040 (Record Supplier Commitment) |
| Consumed By | Plan Supply (firm commitments constrain the next planning run), Procure Materials (confirmed commitments generate purchase orders), Supplier Commitment Assessment (SE‑S‑051) |
| Lifecycle Expectation | Active while the collaboration is ongoing. Transitioned to Closed when the requirement is fulfilled or cancelled. |
| Retention Expectation | All commitments and collaboration history retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Record |
| Behavior | Immutable (collaboration history), Authoritative |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Supplier + Product + Required Date | Extensible to include location, planning scope, purchase organization, or delivery window. |
| Enterprise Request | Requested Quantity, Requested Date, Request Timestamp, Source Planning Reference (Supply Plan ID) | The enterprise’s communicated supply requirement. |
| Supplier Response | Commitment State (Proposed, Acknowledged, Committed, Partially Committed, Rejected, Alternative Proposed, Confirmed), Committed Quantity, Committed Date, Response Timestamp | The supplier’s current response to the request. |
| Commitment Confidence | Confidence Level (Firm, Conditional, Tentative, Estimated) or Confidence Score | The enterprise’s assessment of how trustworthy the current commitment is, based on historical reliability, responsiveness, and current signals. |
| Alternative Proposal | Description of any alternative proposed by the supplier (e.g., alternate quantity, date, plant, route, partial shipment schedule, packaging, substitute material) | Broadly defined to accommodate diverse supplier alternatives without enumerating every type. |
| Supplier‑Declared Assumptions | Free‑text or structured list of assumptions the supplier is making (e.g., raw material availability, transportation availability, customer forecast stability) | Supplier‑declared assumptions underpinning the commitment. |
| Collaboration History | Chronological list of Collaboration Events (Enterprise Request, Supplier Response, Enterprise Decision) with timestamps and actors | Full bidirectional conversation record. |
| Traceability | Source Supply Plan ID, Last Updated Timestamp | Evidence lineage. |

**Collaboration Lifecycle**

| State | Meaning |
|-------|---------|
| Proposed | Enterprise has communicated the requirement to the supplier; no response yet. |
| Acknowledged | Supplier has received the requirement; no commitment yet. |
| Committed | Supplier confirms full quantity by requested date. |
| Partially Committed | Supplier commits partial quantity or alternate date. |
| Rejected | Supplier cannot fulfil this requirement. |
| Alternative Proposed | Supplier proposes a different quantity, date, product, or other alternative. |
| Confirmed | Enterprise accepts the supplier’s commitment or alternative; commitment is now firm for planning. |

**Permitted Transitions:** Proposed → Acknowledged → Committed / Partially Committed / Rejected / Alternative Proposed. Alternative Proposed → Confirmed (if enterprise accepts). Confirmed → (terminal for the current commitment; superseded by a new commitment if circumstances change).

**Invariants:**
- A commitment is uniquely identified by Supplier + Product + Required Date within the active collaboration scope.
- Collaboration Events are immutable.
- Only the latest supplier response determines the current commitment state.

**Business Operations:** Propose Requirement, Record Supplier Response, Accept Alternative, Confirm Commitment, Close.

**Traceability:** Business Owner: CA‑S‑005. Produced By: AB‑S‑040. Referenced by: FS‑S‑040. Governed by BR‑S‑080–085 and PO‑S‑060.

#### Aggregate Behaviours

##### AB‑S‑040 — Record Supplier Commitment

**Purpose:** Create or update a Supplier Commitment record following the enterprise’s proposal or the supplier’s response.  
**Owned Aggregate:** Supplier Commitment (SE‑S‑050).  
**Required Input State:** None (creation) or Active.  
**Produced Output State:** Current commitment state updated; Collaboration Event appended.  
**Invoked Decisions:** DE‑S‑050 (Accept Supplier Response).  
**Invoked Algorithms:** BA‑S‑020 (Evaluate Commitment Confidence — deferred).  
**Published Events:** EV‑S‑040 (Supplier Commitment Changed).  
**Business Transaction:** Protects Supplier Commitment aggregate.  
**Idempotency:** Re‑recording the same response with the same timestamp produces no duplicate event.  
**Concurrency:** Commitments for different supplier‑product‑date combinations processed independently.

### 4.5.10 Supplier Commitment Assessment — SE‑S‑051

**Business Intent:** Provide a continuously current enterprise interpretation of a supplier’s overall commitment posture, reliability, responsiveness, and risk—independent of individual commitments.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Interpret the current state of a supplier’s planning collaboration at an aggregate level. |
| Definition | The aggregate root that maintains the current assessment of a supplier’s commitment posture—summarising open commitments, responsiveness, reliability, and current risk signals. The assessment is per supplier; the model is extensible to supplier × product if future enterprise needs require it. |
| Identity | Supplier. One assessment per active supplier. |
| Business Owner | Collaborate with Suppliers (CA‑S‑005) |
| Produced By | AB‑S‑041 (Update Supplier Commitment Assessment) |
| Consumed By | Plan Supply (buffer/safety decisions), Procure Materials (prioritisation), Supplier Collaboration Health Assessment (SE‑S‑052) |
| Lifecycle Expectation | Active → Archived. |
| Retention Expectation | All assessment states and change events retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Supplier | The supplier being assessed. |
| Commitment Summary | Total Open Commitments, Committed vs Proposed, Late Commitments, Commitment Confidence Distribution | Aggregate view of the supplier’s commitment posture. |
| Responsiveness | Average Time to Acknowledge, Average Time to Commit, Response Rate | How quickly and reliably the supplier responds. |
| Reliability | Historical Commitment Accuracy (committed vs actual delivery — consumed from Evaluate Supply Quality), Current Confidence Trend | How reliable the supplier’s commitments have proven to be. |
| Risk Signals | Supply Continuity Risk, Commitment Risk, Dependency Risk (single‑source exposure), Concentration Risk | Current risk interpretation. |
| Traceability | Source Supplier Commitment IDs, Assessment Timestamp | Evidence lineage. |

**Lifecycle**

| State | Description |
|-------|-------------|
| Active | Supplier is actively assessed. |
| Archived | Supplier is no longer active for planning. |

**Invariants:**
- At any moment, an active supplier has exactly one current Supplier Commitment Assessment.
- The assessment is derived from the current set of Supplier Commitments and the latest available reliability data.

**Business Operations:** Update Assessment, Archive.

**Traceability:** Business Owner: CA‑S‑005. Produced By: AB‑S‑041. Referenced by: FS‑S‑041. Governed by BR‑S‑086–089 and PO‑S‑060.

#### Aggregate Behaviours

##### AB‑S‑041 — Update Supplier Commitment Assessment

**Purpose:** Re‑evaluate and update the Supplier Commitment Assessment for a supplier based on all current Supplier Commitments and reliability data.  
**Owned Aggregate:** Supplier Commitment Assessment (SE‑S‑051).  
**Required Input State:** Active.  
**Produced Output State:** Active (commitment summary, responsiveness, reliability, risk signals may change; Assessment Change Event appended).  
**Invoked Decisions:** None (analytical).  
**Invoked Algorithms:** BA‑S‑021 (Assess Supplier Collaboration Risk — deferred).  
**Published Events:** EV‑S‑041 (Supplier Commitment Assessment Changed).  
**Business Transaction:** Protects Supplier Commitment Assessment aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same result.  
**Concurrency:** Assessments for different suppliers processed independently.

### 4.5.11 Supplier Collaboration Health Assessment — SE‑S‑052

**Business Intent:** Provide a periodic, authoritative management view of supplier collaboration quality, trends, and recommended actions.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Publish periodic supplier collaboration health understanding for management oversight, supplier development, and planning policy refinement. |
| Definition | The aggregate root that maintains the published supplier collaboration health assessment version series for an assessment scope and evaluation period. Each version contains health classifications, responsiveness trends, commitment accuracy trends, and recommendation signals. |
| Identity | Assessment Scope + Evaluation Period + Version. |
| Business Owner | Collaborate with Suppliers (CA‑S‑005) |
| Produced By | AB‑S‑042 (Publish Supplier Collaboration Health Assessment) |
| Consumed By | Evaluate Supply Quality, Learn From Supply, Management |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published assessments retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Assessment Scope, Evaluation Period, Assessment Version Identifier, Version Number | Unique version identity. |
| Health Classification | Per supplier: Excellent, Good, Needs Improvement, At Risk | Collaboration health classification. |
| Responsiveness Trends | Direction and magnitude of change in responsiveness metrics | Trend analysis. |
| Commitment Accuracy Trends | Direction and magnitude of change in commitment accuracy | Trend analysis. |
| Recommendation Signals | Consider buffer increase, Escalate for review, Collaboration improving, Seek alternate supplier | Forward‑looking recommendations for planning and supplier management. |
| Evidence | Source Supplier Commitment Assessment IDs | Evidence lineage. |
| Traceability | Publication Time, Superseded Version ID | Timestamps. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Assessment computed for the evaluation period. | Health classification, trends, and recommendations populated. |
| Published | Assessment version released as authoritative. | Publication Time recorded. Previous Published version → Superseded. EV‑S‑042 recorded. BN‑S‑041 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version. |

**Invariants:**
- Exactly one Published Supplier Collaboration Health Assessment exists for a given scope and evaluation period.
- A Published Health Assessment is immutable.

**Business Operations:** Publish Assessment.

**Traceability:** Business Owner: CA‑S‑005. Produced By: AB‑S‑042. Referenced by: FS‑S‑042. Governed by BR‑S‑090–092 and PO‑S‑060.

#### Aggregate Behaviours

##### AB‑S‑042 — Publish Supplier Collaboration Health Assessment

**Purpose:** Compute supplier collaboration health classifications, trends, and recommendation signals for all covered suppliers and publish the periodic, versioned assessment.  
**Owned Aggregate:** Supplier Collaboration Health Assessment (SE‑S‑052).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑051.  
**Invoked Algorithms:** BA‑S‑022 (Classify Collaboration Health — deferred).  
**Published Events:** EV‑S‑042 (Supplier Collaboration Health Assessment Published).  
**Business Transaction:** Protects Supplier Collaboration Health Assessment aggregate.  
**Idempotency:** Re‑assessment for same scope/period produces same results.  
**Concurrency:** Independent per scope.

### 4.5.12 Procurement Recommendation — SE‑S‑060

**Business Intent:** Maintain the enterprise’s continuously current sourcing decision for each procurement requirement—recording which suppliers to use, in what quantities, at what times, with what confidence, and under what rationale, assumptions, and decision evidence.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Convert each procurement requirement from the Supply Plan into a specific, governed, and traceable sourcing decision that explains what to buy, from whom, when, and why. |
| Definition | The aggregate root that maintains the current procurement recommendation for one procurement requirement. It captures the requirement itself, the sourcing decision (supplier allocation, quantities, order timing), procurement feasibility, the decision rationale and evidence, confidence factors, and planning assumptions. The recommendation is continuously maintained; when materially changed, a new version supersedes the previous one. Multiple sourcing strategies (intentional split, contingency) are natively supported. The model is extensible to contract manufacturers, intercompany sourcing, consignment, and spot buying without architectural change. |
| Identity | Supply Plan Procurement Requirement Reference + Version. The identity is anchored to the underlying procurement requirement rather than only Product + Date + Location. Product, Location, Required Date, and Quantity are business attributes carried within the recommendation. One active recommendation per requirement at any moment. |
| Business Owner | Procure Materials (CA‑S‑006) |
| Produced By | AB‑S‑050 (Create Procurement Recommendation) |
| Consumed By | Procurement Plan (SE‑S‑061 — published baseline), Explain Supply Decisions (sourcing rationale), Evaluate Supply Quality (plan adherence), Learn From Supply (sourcing pattern improvements) |
| Lifecycle Expectation | Active → Superseded (when materially revised) → Archived. |
| Retention Expectation | All versions retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived, Versioned |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Supply Plan Procurement Requirement Reference, Version | Anchored to the requirement, not just Product/Date/Location. |
| Requirement | Product, Location, Required Date, Required Quantity | The procurement need, sourced from the Supply Plan. |
| Procurement Feasibility | Feasible / Conditionally Feasible / Infeasible, Feasibility Constraints (no approved supplier, lead time impossible, MOQ impossible, calendar impossible, insufficient committed quantity) | Whether the requirement can actually be procured under current constraints. |
| Sourcing Decision | Supplier Allocations (supplier, quantity, order release date, expected delivery date), Sourcing Strategy (Preferred, Split — Intentional, Split — Contingency, Alternate, Emergency) | The recommended sourcing decision. Multiple allocations per recommendation are supported. |
| Decision Rationale | Why this supplier, why this split, why this timing | Business‑language explanation of the sourcing decision. |
| Decision Evidence | Supplier Commitments used, Inventory Position Assessment used, Capacity Position Assessment used, Policy versions, Planning Assumptions | Structured references to the evidence that informed the decision. |
| Confidence | Overall Recommendation Confidence, Supplier Confidence, Demand Confidence, Capacity Confidence, Inventory Risk Factor, Sourcing Complexity Factor | Multi‑dimensional confidence assessment. |
| Assumptions | Supplier lead time accuracy, demand forecast stability, capacity availability, transit time assumptions | Planning assumptions underpinning the recommendation. |
| Alternative Recommendations | Alternative Recommendation(s) with ranking, rationale, and confidence (placeholder for future AI‑generated options) | Supports future AI‑driven alternative ranking without architectural change. |
| Traceability | Source Supply Plan ID, Last Updated Timestamp | Evidence lineage. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Current, authoritative recommendation for the requirement. | Sourcing decision, confidence, rationale, and evidence maintained. |
| Superseded | Replaced by a materially revised recommendation. | Retained for audit; no longer authoritative. |
| Archived | Requirement is no longer active. | Final state retained permanently. |

**Invariants:**
- At any moment, exactly one Active Procurement Recommendation exists for each procurement requirement.
- The recommendation must be derived from the current Supply Plan, supplier commitments, inventory assessments, and capacity assessments.
- Decision evidence must reference the specific artifacts used.
- Versions are immutable once superseded or archived.

**Business Operations:** Create Recommendation, Revise Recommendation (on material change), Archive.

**Traceability:** Business Owner: CA‑S‑006. Produced By: AB‑S‑050. Referenced by: FS‑S‑060. Governed by BR‑S‑100–110 and PO‑S‑070.

#### Aggregate Behaviours

##### AB‑S‑050 — Create Procurement Recommendation

**Purpose:** For a given procurement requirement, evaluate eligible suppliers, constraints, commitments, and risks to produce a sourcing decision with full rationale, evidence, and confidence.  
**Owned Aggregate:** Procurement Recommendation (SE‑S‑060).  
**Required Input State:** None (creation) or Active (revision).  
**Produced Output State:** Active (previous version → Superseded if revised).  
**Invoked Decisions:** DE‑S‑060.  
**Invoked Algorithms:** BA‑S‑030 (Evaluate Procurement Feasibility — deferred), BA‑S‑031 (Assess Procurement Confidence — deferred).  
**Published Events:** EV‑S‑050 (Procurement Recommendation Created).  
**Business Transaction:** Protects Procurement Recommendation aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same recommendation.  
**Concurrency:** Recommendations for different requirements processed independently.

### 4.5.13 Procurement Plan — SE‑S‑061

**Business Intent:** Publish the authoritative, versioned procurement baseline for a planning cycle—composed of the current Procurement Recommendations, not re‑calculated from scratch.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the authoritative published procurement baseline that downstream execution systems and evaluation capabilities consume. |
| Definition | The aggregate root that maintains the published procurement plan version series for a Planning Scope and Planning Cycle. Each version is a snapshot of the current Active Procurement Recommendations at publication time, together with aggregate metrics, risk signals, and recommendation signals. The Procurement Plan is not another planning calculation; it is the published, authoritative baseline composed of the current Procurement Recommendations. |
| Identity | Planning Scope + Planning Cycle + Version. |
| Business Owner | Procure Materials (CA‑S‑006) |
| Produced By | AB‑S‑051 (Publish Procurement Plan) |
| Consumed By | Procurement Execution (PO creation — external), Explain Supply Decisions, Evaluate Supply Quality, Learn From Supply |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published plans retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Planning Scope, Planning Cycle, Version | Unique version identity. |
| Recommendations | Set of Active Procurement Recommendation references | The current recommendations that compose this plan. |
| Aggregate Metrics | Total Procurement Spend, Supplier Allocation Percentages, Confidence Distribution, Feasibility Summary | Aggregated view of the plan. |
| Risk Signals | Supplier Concentration, Single‑Source Exposure, Geographic Concentration, Procurement Confidence Distribution | Enterprise‑level procurement risk insights. |
| Recommendation Signals | Items Requiring Planner Review, Supplier Consolidation Opportunities, Risk Concentration Alerts, Unresolved Feasibility Issues | Forward‑looking signals for planner attention and procurement strategy. |
| Evidence | Source Procurement Recommendation IDs, Publication Timestamp | Evidence lineage. |
| Traceability | Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Plan composed from current recommendations. | Aggregate metrics and signals computed. |
| Published | Plan version released as authoritative procurement baseline. | Publication Time recorded. Previous Published version → Superseded. EV‑S‑051 recorded. BN‑S‑051 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version. |

**Invariants:**
- Exactly one Published Procurement Plan exists for a given Planning Scope and Planning Cycle.
- The plan is a snapshot of Active Procurement Recommendations at publication time; it does not recompute them.
- A Published plan is immutable.

**Business Operations:** Publish Plan.

**Traceability:** Business Owner: CA‑S‑006. Produced By: AB‑S‑051. Referenced by: FS‑S‑061. Governed by BR‑S‑111–113 and PO‑S‑070.

#### Aggregate Behaviours

##### AB‑S‑051 — Publish Procurement Plan

**Purpose:** Compose the current Active Procurement Recommendations into a versioned, authoritative procurement baseline for the planning cycle.  
**Owned Aggregate:** Procurement Plan (SE‑S‑061).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑061.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑051 (Procurement Plan Published).  
**Business Transaction:** Protects Procurement Plan aggregate.  
**Idempotency:** Re‑publication with the same recommendations produces the same plan.  
**Concurrency:** Publication per planning cycle occurs once.


### 4.5.14 Production Schedule — SE‑S‑070

**Business Intent:** Maintain the enterprise’s continuously current understanding of how production will be sequenced and timed across constrained resources to realize the Supply Plan, respecting finite capacity, changeovers, material availability, and operational constraints.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Transform the Supply Plan’s planned production quantities into an executable, sequenced, time‑phased schedule for each constrained resource, respecting all finite constraints and capturing the rationale, feasibility, confidence, and stability of every scheduling decision. |
| Definition | The aggregate root that maintains the current production schedule for one constrained resource over one planning period. It records the ordered sequence of production activities with their timing, changeovers, resource assignments, and synchronization dependencies. It assesses schedule feasibility against material, capacity, tool, labor, and maintenance constraints. It captures the decision rationale and evidence for every sequencing choice. It tracks schedule stability relative to previous versions. |
| Identity | Resource + Planning Period + Schedule Type (where multiple independent schedules exist for the same resource‑period; default: Primary). The model supports Primary, Recovery, Frozen, and Scenario schedules without architectural change. |
| Business Owner | Schedule Production (CA‑S‑007) |
| Produced By | AB‑S‑060 (Create Production Schedule) |
| Consumed By | Published Production Schedule (SE‑S‑071), Production Execution (external — job dispatch), Explain Supply Decisions (sequencing rationale), Evaluate Supply Quality (schedule adherence) |
| Lifecycle Expectation | Active → Superseded (when materially revised) → Archived. |
| Retention Expectation | All versions retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived, Versioned |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Resource, Planning Period, Version | Unique version identity within the resource‑period. |
| Sequence | Ordered list of Production Activities (activity identifier, product, quantity, start time, end time, duration, preceding activity, succeeding activity) | The ordered production sequence. Order is enterprise knowledge: A→B→C differs from C→A→B. |
| Campaigns | Campaign grouping of consecutive production activities sharing common setup, material, or process characteristics; campaign boundaries defined by changeover triggers (product family change, colour change, allergen clean, grade transition) | Campaign scheduling minimises changeovers and contamination risk in process industries (paint, food, pharma, chemicals, steel). Each campaign groups activities; campaign sequence is enterprise knowledge. |
| Changeovers | Per‑transition changeover type, setup time, cleaning time, tooling change time | Changeover details between consecutive activities. Fundamental scheduling constraint not modelled in the Supply Plan. |
| Resource Assignment | Specific resource assigned, equivalent resources considered (resource families, parallel resources), alternate resource evaluated, overtime applied | Which resource executes each activity, and what flexibility was exercised. The model supports reasoning about equivalent resources and resource families, not merely a single alternate. |
| Synchronization | Cross‑resource dependencies (Activity X on Resource A must complete before Activity Y on Resource B can start), labor synchronization points, tool synchronization points, material synchronization points (materials must be available at the specific time they are needed, not merely exist in inventory — temporal dependency is essential) | Multi‑resource coordination requirements. Material availability is time‑sensitive: a material arriving at 14:00 cannot support an activity scheduled to start at 10:00, even if inventory shows sufficient quantity. |
| Schedule Feasibility | Overall Feasibility (Feasible, Conditionally Feasible, Infeasible), per‑constraint feasibility: Material Availability (time‑sensitive), Capacity Availability (finite), Tool Availability, Labor Availability, Maintenance Conflict, Calendar Conflict, Frozen Horizon Compliance (activities within the frozen horizon are unmodified from the previously published baseline unless an approved exception exists) | Whether the schedule is actually executable. Each constraint is independently assessed and documented. The frozen horizon defined by the Scheduling Policy (PO‑S‑080) is respected; any change within the frozen zone requires explicit approval. |
| Schedule Confidence | Overall Confidence Score, Material Certainty, Resource Reliability, Changeover Accuracy, Demand Stability | Multi‑dimensional confidence in the schedule’s executability. |
| Schedule Criticality | Per‑activity criticality classification: Critical Path Activity (delay directly extends the schedule), Bottleneck Activity (constrained resource with no alternative), Flexible Activity (can be moved with minimal impact) | Enterprise understanding of which schedule elements are most important. Valuable for Explain Supply Decisions, Evaluate Supply Quality, and Detect Supply Exceptions. |
| Decision Evidence | Source Supply Plan ID, Inventory Position Assessments used, Capacity Position Assessments used, Material Availability Confirmations, Scheduling Policy version | Structured references to the evidence that informed scheduling decisions. |
| Decision Rationale | Why this sequence, why this resource, why overtime, why a setup was preferred over due date, why an activity was deferred, why the sequence changed from the previous version (change rationale) | Business‑language explanation of key scheduling choices and the reasons for changes between versions. |
| Schedule Stability | Stability Score, Change Count since previous version, Change Magnitude | How much the schedule has changed relative to the previous version. |
| Traceability | Created Timestamp, Last Updated Timestamp, Source Supply Plan Version | Evidence lineage. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Current, authoritative schedule for the resource‑period. | Sequence, feasibility, confidence, and stability maintained. |
| Superseded | Replaced by a materially revised schedule. | Retained for audit; no longer authoritative. |
| Archived | Planning period has passed. | Final state retained permanently. |

**Invariants:**
- At any moment, exactly one Active Production Schedule exists for each resource‑period combination.
- The schedule must respect all hard constraints (finite capacity, material prerequisites, mandatory maintenance windows).
- Sequencing decisions must be documented with rationale and evidence.
- Versions are immutable once superseded or archived.

**Business Operations:** Sequence Production, Revise Schedule (on material change or disruption), Repair Schedule (minimal disruption repair following an unplanned event such as machine breakdown, rather than full re‑sequencing), Assess Feasibility, Archive. Schedule Repair is an explicit enterprise operation distinct from full revision.

**Traceability:** Business Owner: CA‑S‑007. Produced By: AB‑S‑060. Referenced by: FS‑S‑070. Governed by BR‑S‑120–132 and PO‑S‑080.

**AI Readiness:** The Production Schedule model accommodates future AI‑driven capabilities including autonomous schedule repair (minimal‑disruption recovery after unplanned events), predictive disruption detection, and schedule optimization recommendation, without architectural change. Decision evidence and confidence fields already provide the structured inputs AI agents require.

#### Aggregate Behaviours

##### AB‑S‑060 — Create Production Schedule

**Purpose:** For a given resource and planning period, sequence production activities, assess feasibility, compute confidence and stability, and record the scheduling rationale and evidence.  
**Owned Aggregate:** Production Schedule (SE‑S‑070).  
**Required Input State:** None (creation) or Active (revision).  
**Produced Output State:** Active (previous version → Superseded if revised).  
**Invoked Decisions:** DE‑S‑070 (Determine Production Sequence), DE‑S‑071 (Assess Schedule Feasibility).  
**Invoked Algorithms:** BA‑S‑040 (Optimize Production Sequence — deferred), BA‑S‑041 (Evaluate Schedule Confidence — deferred), BA‑S‑042 (Compute Schedule Stability — deferred).  
**Published Events:** EV‑S‑060 (Production Schedule Created).  
**Business Transaction:** Protects Production Schedule aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same schedule.  
**Concurrency:** Schedules for different resource‑periods processed independently.

### 4.5.15 Published Production Schedule — SE‑S‑071

**Business Intent:** Publish the authoritative, versioned production scheduling baseline for a planning period—composed of the current Production Schedules, not re‑calculated from scratch.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the authoritative published scheduling baseline that production execution systems and evaluation capabilities consume. |
| Definition | The aggregate root that maintains the published production schedule version series for a Planning Scope and Planning Period. Each version is a snapshot of the current Active Production Schedules at publication time, together with aggregate metrics, risk signals, and stability assessments. The Published Production Schedule is not another scheduling calculation; it is the published, authoritative baseline composed of the current Production Schedules. |
| Identity | Planning Scope + Planning Period + Version. |
| Business Owner | Schedule Production (CA‑S‑007) |
| Produced By | AB‑S‑061 (Publish Production Schedule) |
| Consumed By | Production Execution (external — job dispatch), Explain Supply Decisions, Evaluate Supply Quality |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published schedules retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Planning Scope, Planning Period, Version | Unique version identity. |
| Schedules | Set of Active Production Schedule references | The current schedules that compose this baseline. |
| Aggregate Metrics | Total Changeover Time, Resource Utilization, Overtime Required, Schedule Adherence Projection | Aggregated view of the scheduling baseline. |
| Feasibility Summary | Feasible resource count, Conditionally Feasible count, Infeasible count, Top constraints causing infeasibility | Enterprise‑level feasibility overview. |
| Stability Summary | Average Stability Score, Resources with high schedule volatility | Enterprise‑level stability assessment. |
| Risk Signals | Resources at Risk of Delay, Materials at Risk of Unavailability, Bottleneck Resources, Overtime‑Dependent Schedules | Forward‑looking risk signals for production management. |
| Recovery Opportunities | Resources where schedule recovery is possible without affecting customer commitments, estimated recoverable time, recovery strategy (overtime, alternate resource, resequencing) | Forward‑looking recovery assessment for production management. Example: "These schedules could recover 8 hours without affecting customer commitments by using overtime on Resource B and resequencing non‑critical activities." |
| Evidence | Source Production Schedule IDs, Publication Timestamp | Evidence lineage. |
| Traceability | Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Baseline composed from current schedules. | Aggregate metrics, feasibility, stability, and risk signals computed. |
| Published | Baseline version released as authoritative scheduling baseline. | Publication Time recorded. Previous Published version → Superseded. EV‑S‑061 recorded. BN‑S‑060 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version. |

**Invariants:**
- Exactly one Published Production Schedule exists for a given Planning Scope and Planning Period.
- The baseline is a snapshot of Active Production Schedules at publication time; it does not recompute them.
- A Published baseline is immutable.

**Business Operations:** Publish Baseline.

**Traceability:** Business Owner: CA‑S‑007. Produced By: AB‑S‑061. Referenced by: FS‑S‑061. Governed by BR‑S‑133–135 and PO‑S‑080.

#### Aggregate Behaviours

##### AB‑S‑061 — Publish Production Schedule

**Purpose:** Compose the current Active Production Schedules into a versioned, authoritative scheduling baseline for the planning period.  
**Owned Aggregate:** Published Production Schedule (SE‑S‑071).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑072 (Publish Production Schedule).  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑061 (Production Schedule Published).  
**Business Transaction:** Protects Published Production Schedule aggregate.  
**Idempotency:** Re‑publication with the same schedules produces the same baseline.  
**Concurrency:** Publication per planning period occurs once.


### 4.5.16 Distribution Recommendation — SE‑S‑080

**Business Intent:** Maintain the enterprise’s continuously current network positioning decision for each distribution requirement—recording where supply should be sourced, which lanes used, when movement should occur, why, with what confidence, and under what allocation strategy when supply is constrained.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Convert each distribution requirement (planned transfer, replenishment trigger, or rebalancing need) into a specific, governed, and traceable network positioning decision that explains what to move, from where to where, when, and why—or why it was not moved. |
| Definition | The aggregate root that maintains the current distribution recommendation for one distribution requirement. It captures the requirement itself, the movement decision (source locations, lanes, quantities, timing), the distribution strategy (preferred and alternative), allocation strategy when supply is constrained (policy used and allocation outcome), feasibility, confidence, stability, and decision evidence. The model supports any network topology—Supplier→Plant→DC→Regional DC→Warehouse→Customer—without assuming storage at intermediate nodes (cross‑dock flows are natively supported). |
| Identity | Distribution Requirement Reference + Version. Anchored to the underlying requirement (e.g., Supply Plan transfer requirement, Inventory rebalancing trigger). Product, Source, Destination, Required Date are business attributes. |
| Business Owner | Manage Distribution (CA‑S‑008) |
| Produced By | AB‑S‑070 (Create Distribution Recommendation) |
| Consumed By | Distribution Plan (SE‑S‑081 — published baseline), Distribution Execution (external — shipment creation), Explain Supply Decisions (movement rationale), Evaluate Supply Quality (plan adherence) |
| Lifecycle Expectation | Active → Superseded (when materially revised) → Archived. |
| Retention Expectation | All versions retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Authoritative, Derived, Versioned |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Distribution Requirement Reference, Version | Anchored to the requirement. |
| Requirement | Product, Required Quantity, Source (where supply is needed), Destination (where demand exists), Required Date | The network positioning need. |
| Movement Decision | Source Location(s), Quantity per source, Lane(s) used, Departure Timing (considering transit lead time), Destination Receiving Window | The specific movement plan. Inventory is projected to be available at the source at departure time; a unit arriving at the source on Friday cannot be moved on Wednesday. |
| Distribution Strategy | Preferred Distribution Strategy (primary source, primary lane, primary timing), Alternative Distribution Strategy (alternate source, alternate lane, alternate timing) | Enterprise understanding of the optimal and fallback positioning strategies. Strategies may differ in source location, intermediate nodes, lanes, or replenishment patterns—not merely alternative routes. |
| Allocation Strategy | Allocation Policy Used (Fair Share, Strategic Customer Priority, Region Priority, Service Level Priority, Promotion Support), Allocation Outcome (who received less, who received more, who was deferred, and by what quantity) | Applied only when available deployable inventory is insufficient to satisfy all requirements. Captures both the rule applied and the business consequence. |
| Deployment Priority | Service Protection, Promotion Support, New Product Launch, Seasonal Positioning, Inventory Balancing | The business purpose driving the deployment decision. |
| Distribution Feasibility | Source Inventory Availability, Lane Capacity Availability, Transit Time Feasibility, Destination Receiving Capacity (dock capacity, storage availability, handling capability), Time Window Achievability | Whether the movement is actually executable. Each constraint independently assessed. |
| Confidence | Inventory Certainty, Lane Reliability, Transit Time Accuracy, Receiving Capacity Confidence, Demand Volatility, Disruption Exposure | Multi‑dimensional confidence in the recommendation. |
| Decision Rationale | Why this source, why this lane, why this timing, why inventory was intentionally retained (e.g., "Supply retained at DC‑West because downstream demand uncertainty exceeded transfer benefit"), why this allocation outcome | Business‑language explanation of the movement decision and any deliberate non‑movement. |
| Decision Evidence | Source Supply Plan ID, Inventory Position Assessments used, Lane capacity data, Allocation Policy version | Structured references to evidence. |
| Stability | Change Count, Change Magnitude relative to previous version | How much the recommendation has changed. |
| Traceability | Created Timestamp, Last Updated Timestamp | Evidence lineage. |

**Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Active | Current, authoritative recommendation. | Movement decision, strategy, feasibility, confidence, and stability maintained. |
| Superseded | Replaced by a materially revised recommendation. | Retained for audit; no longer authoritative. |
| Archived | Requirement is no longer active. | Final state retained permanently. |

**Invariants:**
- At any moment, exactly one Active Distribution Recommendation exists for each distribution requirement.
- The recommendation must be derived from the current Supply Plan, Inventory Position Assessments, and network data.
- When supply is constrained, the Allocation Strategy must be recorded with both the policy used and the allocation outcome.
- Decision evidence must reference the specific artifacts used.
- Versions are immutable once superseded or archived.

**Business Operations:** Create Recommendation, Revise Recommendation, Assess Feasibility, Archive.

**AI Readiness:** The model accommodates future AI‑driven capabilities including dynamic stock rebalancing, risk‑aware deployment, alternate distribution path suggestion, autonomous network optimisation, and network reconfiguration during disruption, without architectural change. Confidence and evidence fields provide structured inputs for AI agents.

**Traceability:** Business Owner: CA‑S‑008. Produced By: AB‑S‑070. Referenced by: FS‑S‑080. Governed by BR‑S‑140–155 and PO‑S‑090.

#### Aggregate Behaviours

##### AB‑S‑070 — Create Distribution Recommendation

**Purpose:** For a given distribution requirement, evaluate source locations, lanes, constraints, and costs to produce a movement decision with full rationale, evidence, confidence, strategy, and allocation outcome.  
**Owned Aggregate:** Distribution Recommendation (SE‑S‑080).  
**Required Input State:** None (creation) or Active (revision).  
**Produced Output State:** Active (previous version → Superseded if revised).  
**Invoked Decisions:** DE‑S‑080.  
**Invoked Algorithms:** BA‑S‑050 (Evaluate Distribution Feasibility — deferred), BA‑S‑051 (Assess Distribution Confidence — deferred).  
**Published Events:** EV‑S‑070 (Distribution Recommendation Created).  
**Business Transaction:** Protects Distribution Recommendation aggregate.  
**Idempotency:** Re‑evaluation with same inputs produces same recommendation.  
**Concurrency:** Recommendations for different requirements processed independently.

### 4.5.17 Distribution Plan — SE‑S‑081

**Business Intent:** Publish the authoritative, versioned distribution baseline for a planning cycle—composed of the current Distribution Recommendations, not re‑calculated from scratch.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Provide the authoritative published distribution baseline that execution systems and evaluation capabilities consume. |
| Definition | The aggregate root that maintains the published distribution plan version series for a Planning Scope and Planning Cycle. Each version is a snapshot of the current Active Distribution Recommendations at publication time, together with aggregate metrics, risk signals, and recovery opportunities. The Distribution Plan is not another planning calculation; it is the published, authoritative baseline composed of the current Distribution Recommendations. |
| Identity | Planning Scope + Planning Cycle + Version. |
| Business Owner | Manage Distribution (CA‑S‑008) |
| Produced By | AB‑S‑071 (Publish Distribution Plan) |
| Consumed By | Distribution Execution (external — shipment creation), Explain Supply Decisions, Evaluate Supply Quality, Learn From Supply |
| Lifecycle Expectation | Version lifecycle: Draft → Published → Superseded. |
| Retention Expectation | All published plans retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | Projection |
| Behavior | Versioned, Authoritative, Derived |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Planning Scope, Planning Cycle, Version | Unique version identity. |
| Recommendations | Set of Active Distribution Recommendation references | The current recommendations that compose this plan. |
| Aggregate Metrics | Total Movements, Lane Utilisation, Network Balance Score, Cost Summary | Aggregated view of the plan. |
| Risk Signals | Lane Capacity Risks, Receiving Capacity Bottlenecks, Transit Time Exposure, Single‑Lane Dependencies, Constrained Supply Summary | Enterprise‑level distribution risk insights. |
| Recovery Opportunities | Movements that could be rerouted if disruptions occur, estimated recoverable time, alternative strategies available | Forward‑looking recovery assessment. |
| Stability Summary | Average Stability Score, Resources with high movement volatility | Enterprise‑level stability assessment. |
| Evidence | Source Distribution Recommendation IDs, Publication Timestamp | Evidence lineage. |
| Traceability | Publication Time, Superseded Version ID | Timestamps and version lineage. |

**Version Lifecycle**

| State | Description | Transition Effects |
|-------|-------------|-------------------|
| Draft | Plan composed from current recommendations. | Aggregate metrics, risk signals, recovery opportunities, and stability computed. |
| Published | Plan version released as authoritative distribution baseline. | Publication Time recorded. Previous Published version → Superseded. EV‑S‑071 recorded. BN‑S‑071 published. |
| Superseded | Version replaced by a newer Published version. | State set on the older version. |

**Invariants:**
- Exactly one Published Distribution Plan exists for a given Planning Scope and Planning Cycle.
- The plan is a snapshot of Active Distribution Recommendations at publication time; it does not recompute them.
- A Published plan is immutable.

**Business Operations:** Publish Plan.

**Traceability:** Business Owner: CA‑S‑008. Produced By: AB‑S‑071. Referenced by: FS‑S‑081. Governed by BR‑S‑156–158 and PO‑S‑090.

#### Aggregate Behaviours

##### AB‑S‑071 — Publish Distribution Plan

**Purpose:** Compose the current Active Distribution Recommendations into a versioned, authoritative distribution baseline for the planning cycle.  
**Owned Aggregate:** Distribution Plan (SE‑S‑081).  
**Required Input State:** None (creation).  
**Produced Output State:** Published (or Draft if review required).  
**Invoked Decisions:** DE‑S‑081.  
**Invoked Algorithms:** None.  
**Published Events:** EV‑S‑071 (Distribution Plan Published).  
**Business Transaction:** Protects Distribution Plan aggregate.  
**Idempotency:** Re‑publication with the same recommendations produces the same plan.  
**Concurrency:** Publication per planning cycle occurs once.


### 4.5.18 Supply Change Assessment — SE‑S‑090

**Business Intent:** Maintain the enterprise’s continuously current awareness of what has changed in the supply ecosystem—capturing the current change state, evidence, confidence, materiality, and the historical evolution of changes over time.

**Enterprise Information Contract**

| Attribute | Value |
|-----------|-------|
| Purpose | Develop and maintain the enterprise’s authoritative understanding of supply changes by normalising heterogeneous signals into a canonical enterprise change representation. |
| Definition | The aggregate root that maintains the current supply change assessment for one monitored supply entity and change type. It captures what changed (current change state), why the enterprise believes it changed (evidence and confidence), whether the change is material to planning (materiality), whether it was planned or unplanned, its temporal context, its lifecycle stage, and the correlated chain of related changes. It also preserves the immutable history of how the change evolved from initial observation through corroboration, confirmation, resolution, and archival. Multiple concurrent changes on the same entity are supported through distinct change types. |
| Identity | Supply Entity Type + Supply Entity Identifier + Change Type. One assessment per monitored entity per change type. Examples: Supplier S1 + Quantity Reduction, Resource R3 + Capacity Reduction, Lane L4 + Disruption. |
| Business Owner | Sense Supply Changes (CA‑S‑009) |
| Produced By | AB‑S‑080 (Assess Supply Change) |
| Consumed By | Detect Supply Exceptions (CA‑S‑011 — determines whether changes constitute exceptions), Plan Supply (awareness of supply constraints), Evaluate Supply Quality (change frequency analysis), Learn From Supply (change pattern learning) |
| Lifecycle Expectation | Continuously maintained. No periodic published assessment. The assessment is always current and transitions through lifecycle stages as understanding evolves. |
| Retention Expectation | All assessments and change history retained permanently. |

**Ontology Classification**

| Dimension | Value |
|-----------|-------|
| Nature | State |
| Behavior | Continuously Maintained, Authoritative |

**Information Model**

| Category | Information | Description |
|----------|-------------|-------------|
| Identity | Supply Entity Type + Supply Entity Identifier + Change Type | Unique identity per monitored entity and change dimension. |
| Current Change State | Change State (No Change, Change Observed, Change Corroborated, Change Confirmed, Change Resolved, Change Archived), Change Description (business‑language description of what changed), Change Magnitude (deviation from baseline), Change Direction (Increase, Decrease, Disruption, Recovery) | The enterprise’s current understanding of the change. Separated from historical evolution. |
| Change Classification | Planned / Unplanned | Whether the change was expected (maintenance, scheduled downtime, notified delay) or unexpected (machine failure, unplanned disruption). |
| Materiality | Material to Planning / Non‑Material / Unassessed | Planning materiality—whether the change meaningfully affects planning parameters. Not business severity (which belongs to Exception Detection). |
| Temporal Context | Occurrence Time (when the change happened in reality), Detection Time (when the enterprise first became aware), Effective Time (when the change impacts planning), Expected Duration, Actual Resolution Time | Four distinct timestamps providing complete temporal understanding. |
| Evidence | Source Signals (references to Supplier Commitments, Enterprise Supply Picture, Capacity Position Assessments, external data), Corroborating Signals, Signal Summary | Why the enterprise believes the change occurred and what supports that understanding. Heterogeneous signals from multiple sources are normalised into a canonical enterprise change representation. |
| Assessment Confidence | Confirmed, High Confidence, Medium Confidence, Low Confidence, Unconfirmed | The enterprise’s confidence in the change assessment, distinct from the underlying evidence. |
| Correlated Changes | References to other Supply Change Assessments linked to this change (e.g., a port closure linked to a supplier delay linked to a capacity reduction) | Changes rarely happen in isolation. This supports the enterprise's understanding of related changes. Correlation does not imply causation; the enterprise records relationships without presuming causal inference unless explicitly established. |
| Change History | Chronological list of Change History Events (state transition, timestamp, evidence update, confidence update) | Immutable record of the change’s evolution: Observed → Corroborated → Confirmed → Resolved → Archived. |

**Change Type Governance:** Change Type is governed enterprise vocabulary, not arbitrary free text. The Supply Change Policy (PO‑S‑100) defines the recognised change types per supply entity type (e.g., Supplier: Quantity Reduction, Lead Time Change, Commitment Withdrawal; Capacity: Capacity Reduction, Maintenance Change; Inventory: Unexpected Consumption, Inventory Correction). New change types may be added through policy updates without modifying the semantic model.

**Change Lifecycle**

| Stage | Description | Transition Trigger |
|-------|-------------|-------------------|
| No Change | Baseline state; no change detected for this entity and change type. | Initial state or after resolution and archival. |
| Observed | A signal has been received indicating a potential change. Not yet corroborated. | Signal received from a source; confidence Low or Unconfirmed. |
| Corroborated | The change has been corroborated by at least one additional independent source. Confidence upgraded. | Second independent signal confirms the change. |
| Confirmed | The change is confirmed and assessed as material or non‑material to planning. | Sufficient evidence exists; materiality assessed. |
| Resolved | The change has reverted, been resolved, or is no longer relevant to planning. | Underlying condition returns to baseline or becomes irrelevant. |
| Archived | The change record is preserved for audit; no longer actively monitored. | After a retention period following resolution. |

**Lifecycle Transitions:** No Change → Observed → Corroborated → Confirmed → Resolved → Archived. At any stage, if evidence contradicts the change, it may return to No Change or transition to Resolved.

**Invariants:**
- At any moment, for each monitored entity and change type, exactly one current Supply Change Assessment exists.
- The Current Change State is always derived from the latest Change History Event. The two shall never diverge.
- The current Change State reflects the latest understanding based on all available signals.
- Change History Events are immutable.
- Materiality assessment is scoped to planning relevance, not business severity.

**Business Operations:** Observe Change, Corroborate Change, Confirm Change, Resolve Change, Archive.

**AI Readiness:** The model accommodates future AI‑driven capabilities including early disruption detection, emerging change identification (pattern indicates something is likely changing before confirmation), autonomous signal corroboration, and change pattern recognition, without architectural change. Confidence and evidence fields provide structured inputs for AI agents.

**Traceability:** Business Owner: CA‑S‑009. Produced By: AB‑S‑080. Referenced by: FS‑S‑080. Governed by BR‑S‑170–178 and PO‑S‑100.

#### Aggregate Behaviours

##### AB‑S‑080 — Assess Supply Change

**Purpose:** For a given monitored supply entity and change type, evaluate incoming signals against the previously understood state, determine whether a change has occurred, assess its materiality, confidence, and classification, and update the current change state and history.  
**Owned Aggregate:** Supply Change Assessment (SE‑S‑090).  
**Required Input State:** Any lifecycle stage.  
**Produced Output State:** Current change state updated per lifecycle; Change History Event appended if state transitions.  
**Invoked Decisions:** DE‑S‑090 (Assess Supply Change).  
**Invoked Algorithms:** BA‑S‑060 (Normalise Supply Signals — deferred), BA‑S‑061 (Corroborate Supply Change — deferred).  
**Published Events:** EV‑S‑080 (Supply Change Assessment Updated).  
**Business Transaction:** Protects Supply Change Assessment aggregate.  
**Idempotency:** Re‑evaluation with the same signals produces the same outcome.  
**Concurrency:** Assessments for different entity‑change type combinations processed independently.

## 4.6 Entities

*(No entities defined for this version.)*

## 4.7 Value Objects

| Object | Attributes | Business Meaning |
|--------|-----------|------------------|
| Quantity | Numeric value, unit of measure | A measured amount of supply. |
| Planning Period | Start date, end date, time bucket type | A defined interval for planning aggregation. |
| Commitment Reliability | Confidence level, assessment basis | How reliable a supplier commitment is. |

## 4.8 Reference Objects

| ID | Object | Owning Domain | Consistency Expectation |
|----|--------|---------------|--------------------------|
| SE‑C‑005 | Supplier | Core Reference Data | Historical records reference Supplier as at Observation Time. |
| SE‑C‑040 | Product | Core Reference Data | Historical records reference Product as at Observation Time. |
| SE‑C‑041 | Location | Core Reference Data | Historical records reference Location as at Observation Time. |
| SE‑D‑062 | Demand Baseline | Demand Intelligence | Demand context for supply adequacy evaluation. |
| SE‑D‑064 | Segmentation Data (ABC/XYZ) | Demand Intelligence | Consumed by Manage Inventory to provide segmentation context for inventory interpretation and policy determination. |

Governance artifacts are defined as Policies in Chapter 8, not as Semantic Objects.

---

# Chapter 5 — Capability Model

## 5.1 Understand Supply — CA‑S‑001

**Business Intent:** Establish and maintain the enterprise's authoritative understanding of current supply by transforming operational supply data from all source systems into a trusted, published picture of observed supply, committed supply, and production capability.

**Owned Semantic Objects:** SE‑S‑001 (Enterprise Supply Picture), SE‑S‑010 (Supply Data Record).

**Referenced Policies:** PO‑S‑011 (Supply Publication Policy), PO‑S‑012 (Supplier Reliability Policy).

**Business Guarantees:**
- Exactly one Published Enterprise Supply Picture exists for each Planning Scope at any moment.
- Every supply assertion in the picture carries provenance at the assertion level.
- Only accepted supply data contributes. Rejected or quarantined data does not.
- Materiality thresholds prevent over‑versioning while ensuring significant changes are always published.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR‑S‑001 | Receive Supply Data | BW‑S‑001 | FS‑S‑001 |
| CR‑S‑002 | Evaluate Supply Data | BW‑S‑002 | FS‑S‑002 |
| CR‑S‑003 | Revise Enterprise Supply Picture | BW‑S‑003 | FS‑S‑003 |
| CR‑S‑004 | Publish Enterprise Supply Picture | BW‑S‑004 | FS‑S‑004 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑001 | Supply Data Record Received | AB‑S‑001 |
| EV‑S‑002 | Supply Data Record Evaluated | AB‑S‑002 |
| EV‑S‑003 | Enterprise Supply Picture Revised | AB‑S‑003 |
| EV‑S‑004 | Enterprise Supply Picture Published | AB‑S‑005 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑001 | EV‑S‑004 | Enterprise Supply Picture Published: Planning Scope, Version, Publication Time, Superseded Version, Material Change Summary | At‑least‑once | Per Planning Scope | Near‑real‑time |
| BN‑S‑002 | EV‑S‑002 | Supply Data Quarantined: Record Identifier, Reason, Reviewer | At‑least‑once | Per record | Near‑real‑time |
| BN‑S‑003 | EV‑S‑002 | Supply Data Rejected: Record Identifier, Reason | At‑least‑once | Per record | Near‑real‑time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑D‑001 Enterprise Demand Picture Published | Demand Intelligence | Provide demand context for supply adequacy evaluation. | Context refresh |

**Traceability:** Business Owner: CA‑S‑001. Publishes EV‑S‑001–004 and BN‑S‑001–003. Consumes BN‑D‑001. Realises BO‑S‑001.

## 5.2 Plan Supply — CA‑S‑002

**Business Intent:** Produce the enterprise's authoritative, feasible, and balanced supply plan that establishes the planning baseline for all downstream supply capabilities.

Before Plan Supply executes, the enterprise possesses independent understanding of demand (from Demand Intelligence) and current supply (from Understand Supply), but no unified understanding of how future demand is expected to be satisfied. After Plan Supply, the enterprise possesses a single authoritative enterprise understanding — the published Supply Plan — that specifies how demand will be met through production, procurement, and transfers, respecting all constraints and balancing service, cost, and risk. This plan becomes the planning baseline that all downstream supply capabilities consume rather than independently determining supply requirements.

**Owned Semantic Objects:** SE‑S‑020 (Supply Plan).

**Enterprise Dependencies**

Plan Supply transforms multiple sources of enterprise knowledge into a unified supply plan. These dependencies are categorized by their enterprise nature.

**Enterprise Understanding** (consumed from other capabilities — authoritative published knowledge)

| Dependency | Role in Plan Supply |
|------------|---------------------|
| Enterprise Supply Picture (SE‑S‑001) | Represents the enterprise's authoritative understanding of current supply. Plan Supply transforms this current understanding into a future supply plan by projecting inventory forward, consuming open orders, and respecting current commitments. |
| Demand Forecast (from Demand Intelligence) | Represents the enterprise's authoritative projection of future demand. Plan Supply balances this demand against available and planned supply to determine what must be produced, procured, or transferred. |

**Enterprise Master Data** (stable enterprise definitions — define *what* is being planned)

| Dependency | Role in Plan Supply |
|------------|---------------------|
| Product (from Master Data) | Provides the enterprise definition of every planned item and forms the planning identity used throughout supply planning. |
| Resource (from Master Data) | Provides the enterprise definition of every capacity‑constrained production resource used to evaluate production feasibility. |
| Supplier (from Master Data) | Provides the enterprise definition of approved sources of procurement together with contractual planning attributes. |
| Bill of Materials (from Master Data) | Defines material consumption relationships required to translate finished‑product demand into component requirements. |
| Routing (from Master Data) | Defines the sequence of operations and resource requirements required to transform materials into planned supply. |
| Transportation Lane (from Master Data) | Defines permissible material movement between locations together with planning lead times and transportation constraints. |

**Enterprise Governance** (governs *how* planning is performed)

| Dependency | Role in Plan Supply |
|------------|---------------------|
| Planning Calendar (from Master Data / Planning Governance) | Defines the enterprise planning horizon, bucket structure, and working calendar used throughout supply planning. |
| Planning Parameters (from Planning Governance) | Define the enterprise planning strategy for a Planning Scope, including horizon configuration, bucket definitions, frozen periods, sourcing priorities, lot‑sizing behavior, and planning frequencies. Unlike Master Data (which defines *what* is being planned), Planning Parameters govern *how* planning is performed. Unlike Policies (which govern enterprise behavior), Planning Parameters define the operational configuration of the planning process. |
| Supply Planning Policies (PO‑S‑021, PO‑S‑022, PO‑S‑023) | Govern the acceptance, publication, and constraint relaxation decisions made during plan generation and evaluation. |

**Business Guarantees:**
- Exactly one Published Supply Plan exists for a given Planning Scope at any moment.
- Every Published plan satisfies all hard constraints or documents approved constraint relaxations.
- The plan covers the full planning horizon for all mandatory product‑location combinations.
- All published plans are immutable and permanently retained.
- Once published, the Enterprise Supply Plan becomes the authoritative planning baseline for all downstream supply planning capabilities until superseded. Downstream capabilities consume this baseline rather than independently determining supply requirements.

### Capability Responsibilities

Plan Supply executes three responsibilities that together establish the authoritative Supply Plan.

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑005 | Balance Supply and Demand | BW‑S‑010 | Produce a feasible future supply picture by balancing expected demand against available and planned supply, respecting all constraints. This is the enterprise act of reconciling what is needed with what is possible. | FS‑S‑010 |
| CR‑S‑006 | Evaluate Supply Plan | BW‑S‑011 | Assess whether the generated plan satisfies enterprise planning objectives and governance requirements — service levels, cost targets, capacity utilization, inventory policy compliance, and plan stability. | FS‑S‑011 |
| CR‑S‑007 | Publish Supply Plan | BW‑S‑012 | Establish the accepted plan as the enterprise's authoritative supply planning baseline for all downstream capabilities. Publication represents a knowledge handoff: the enterprise transitions from planning to execution, and responsibility transfers to downstream consumers. | FS‑S‑012 |

**Knowledge Handoff**

The Supply Plan represents a critical transition in the enterprise reasoning chain. Plan Supply consumes current supply understanding and demand projections, and produces future supply understanding. Downstream capabilities consume this baseline to develop their own specialized enterprise knowledge — inventory projections, capacity assessments, procurement recommendations, production schedules. Each capability extends the reasoning chain without redefining what has already been established.

```
Enterprise Supply Picture ───→ Supply Plan ───→ Manage Inventory
Demand Forecast               (published,       Manage Capacity
                               authoritative,    Procure Materials
                               versioned)        Schedule Production
                                                 Manage Distribution
                                                 Promise Intelligence
                                                 Scenario Intelligence
```

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑010 | Supply Plan Generated | AB‑S‑010 |
| EV‑S‑011 | Supply Plan Evaluated | AB‑S‑011 |
| EV‑S‑012 | Supply Plan Published | AB‑S‑012 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑010 | EV‑S‑012 | Supply Plan Published: Planning Scope, Horizon, Version, Confidence Score, Planning Assumptions Summary, Constraint Summary, Material Planning Changes relative to previous plan | At‑least‑once | Per plan | Batch (post‑planning run) |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑001 Enterprise Supply Picture Published | Understand Supply | Provides current supply baseline for planning. | FS‑S‑010 (scheduled) |
| BN‑D‑011 Forecast Published | Demand Intelligence | Provides demand to satisfy. | FS‑S‑010 (scheduled) |

### Downstream Consumer Relationships

Each downstream capability consumes the published Supply Plan and develops its own specialized enterprise understanding. The Supply Plan is the single planning baseline — these capabilities do not independently determine supply requirements.

| Consumer | How the Supply Plan Is Used |
|----------|-----------------------------|
| Manage Inventory | Uses projected inventory positions to evaluate future inventory health, assess replenishment adequacy, and identify potential stockout or excess situations. Develops enterprise understanding of inventory performance against policy targets. |
| Manage Capacity | Uses planned production quantities to evaluate future resource loading against available capacity, identify capacity risks (overloads and underloads), and establish enterprise understanding of production capability sufficiency. |
| Procure Materials | Uses planned procurement quantities to develop procurement recommendations, determine order timing, and establish enterprise understanding of future purchasing requirements. |
| Schedule Production | Uses planned production quantities and capacity constraints to develop executable, sequenced production schedules at the resource level. |
| Manage Distribution | Uses planned transfers to develop distribution and replenishment plans, balancing inventory across the network. |
| Promise Intelligence | Uses the authoritative Supply Plan to evaluate Available‑to‑Promise and Capable‑to‑Promise commitments, determining whether customer orders can be fulfilled. |
| Scenario Intelligence | Uses the Supply Plan as the enterprise baseline for scenario comparison and impact analysis, evaluating how changes in assumptions or constraints would alter the supply picture. |

**Traceability:** Business Owner: CA‑S‑002. Publishes EV‑S‑010–012 and BN‑S‑010. Consumes BN‑S‑001 and BN‑D‑011. Realises BO‑S‑001, BO‑S‑002, BO‑S‑003, BO‑S‑005.

*Note:* Consumers marked as stubs (Manage Capacity, Procure Materials, Schedule Production, Manage Distribution, Promise Intelligence, Scenario Intelligence) are planned consumption contracts. Their full specifications will be developed in subsequent versions of the Supply Intelligence specification.

## 5.3 Manage Inventory — CA‑S‑003

**Business Intent:** Develop and maintain the enterprise’s authoritative understanding of inventory — continuously interpreting current inventory positions, maintaining governed replenishment policies, and periodically assessing inventory health for management oversight.

Before Manage Inventory, the enterprise has raw supply positions (from Enterprise Supply Picture) and projected inventory (from Supply Plan), but no interpreted understanding of what those positions mean for planning. After Manage Inventory, the enterprise has continuous operational understanding per product‑location (coverage, risk, financial exposure, policy compliance), continuously maintained replenishment policies, and periodic management assessments for trending and governance.

**Owned Semantic Objects:** SE‑S‑030 (Inventory Position Assessment), SE‑S‑031 (Inventory Policy Assignment), SE‑S‑032 (Inventory Health Assessment).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Manage Inventory |
|------------|--------------------------|
| Enterprise Supply Picture (SE‑S‑001) | Provides current inventory positions, open orders, and supplier commitments — the raw data for position assessment. |
| Supply Plan (SE‑S‑020) | Provides projected inventory positions that inform coverage and risk assessments. |
| Segmentation Data (from Demand Intelligence) | Provides ABC/XYZ classifications that influence policy determination and risk interpretation. |
| Demand Forecast (from Demand Intelligence) | Provides demand projections and variability used in policy calculations. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Inventory Policy Governance (PO‑S‑033) | Defines the calculation methodology, service level targets, cost parameters, and policy type selection rules. |
| Inventory Health Policy (PO‑S‑032, PO‑S‑034) | Defines health classification thresholds, risk assessment criteria, and assessment frequency. |

**Business Guarantees:**
- Every active product‑location has a continuously maintained current interpretation of its inventory position (coverage, risk, financial exposure, compliance).
- Every active product‑location has a continuously maintained, governed inventory policy.
- A periodic Inventory Health Assessment is published for management oversight and trending.
- All state changes and assessments are permanently retained.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑008 | Maintain Inventory Position Understanding | BW‑S‑020 | Continuously interpret current inventory state per product‑location — coverage, risk, financial exposure, policy compliance. | FS‑S‑020 |
| CR‑S‑009 | Maintain Inventory Policies | BW‑S‑021 | Ensure every product‑location has an up‑to‑date, governed inventory policy that reflects current demand and supply conditions. | FS‑S‑021 |
| CR‑S‑010 | Assess Inventory Health | BW‑S‑022 | Publish a periodic, authoritative, versioned assessment of inventory health for management oversight and trend analysis. | FS‑S‑022 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑020 | Inventory Position Assessment Changed | AB‑S‑020 |
| EV‑S‑021 | Inventory Policy Assignment Updated | AB‑S‑021 |
| EV‑S‑022 | Inventory Health Assessment Published | AB‑S‑022 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑020 | EV‑S‑020 | Inventory Position Assessment Changed: Product‑Location, Health Status Change, Risk Level Change | At‑least‑once | Per assessment | Near‑real‑time |
| BN‑S‑021 | EV‑S‑021 | Inventory Policy Updated: Product‑Location, New Policy Parameters | At‑least‑once | Per assignment | Near‑real‑time |
| BN‑S‑022 | EV‑S‑022 | Inventory Health Assessment Published: Assessment ID, Scope, Period, Health Summary, Risk Summary, Financial Summary | At‑least‑once | Per assessment | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑001 Enterprise Supply Picture Published | Understand Supply | Re‑evaluate inventory position assessments. | FS‑S‑020 (conditional) |
| BN‑S‑010 Supply Plan Published | Plan Supply | Re‑evaluate inventory policies and position assessments based on updated projected inventory. | FS‑S‑020, FS‑S‑021 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Inventory Understanding Is Used |
|----------|--------------------------------------|
| Procure Materials | Uses Inventory Position Assessments (risk signals, coverage gaps) to prioritize replenishment actions. Uses Inventory Policy Assignments (reorder point, lot size) to generate replenishment recommendations. |
| Evaluate Supply Quality | Uses Inventory Health Assessments and policy change history to measure inventory management effectiveness. |
| Learn From Supply | Uses health trends, risk patterns, and policy changes to identify improvement opportunities. |

**Traceability:** Business Owner: CA‑S‑003. Publishes EV‑S‑020–022 and BN‑S‑020–022. Consumes BN‑S‑001 and BN‑S‑010. Realises BO‑S‑002, BO‑S‑004, BO‑S‑005, BO‑S‑007.

**Knowledge Handoff**

```
Enterprise Supply Picture ──→ Manage Inventory ──→ Procure Materials
Supply Plan                   (position, policy,    (replenishment orders)
                               health)
```

Manage Inventory transforms raw supply data into interpreted enterprise understanding — what inventory exists, what it means, how it should be managed, and how it is performing over time. Procure Materials consumes this understanding to generate specific replenishment actions, extending the reasoning chain toward execution.

*Note:* Consumers marked as stubs (Manage Capacity, Procure Materials, Schedule Production, Manage Distribution, Promise Intelligence, Scenario Intelligence) are planned consumption contracts. Their full specifications will be developed in subsequent versions of the Supply Intelligence specification.

## 5.4 Manage Capacity — CA‑S‑004

**Business Intent:** Develop and maintain the enterprise's authoritative understanding of capacity — continuously interpreting current resource utilization, identifying constraints and bottlenecks, assessing risks and flexibility, and periodically publishing capacity health for management oversight, investment planning, and operational improvement.

Before Manage Capacity, the enterprise has raw capacity data (from Enterprise Supply Picture) and planned production loads (from Supply Plan), but no interpreted understanding of what those loads mean for resource feasibility, flexibility, or risk. After Manage Capacity, the enterprise has continuous operational understanding per resource and periodic management assessments.

**Owned Semantic Objects:** SE‑S‑040 (Capacity Position Assessment), SE‑S‑041 (Capacity Health Assessment).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Manage Capacity |
|------------|--------------------------|
| Enterprise Supply Picture (SE‑S‑001) | Provides current resource availability and utilization — the raw data for position assessment. |
| Supply Plan (SE‑S‑020) | Provides planned production quantities per resource per time bucket — the projected load used to evaluate future utilization, identify constraints, and build the Load Composition. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Resource (from Master Data) | Defines the capacity‑constrained objects being assessed (machines, lines, cells, work centers, production areas), including installed capacity rates. |
| Resource Calendars (from Master Data) | Shift calendars, working calendars, maintenance calendars, holiday calendars — essential for determining Available Capacity. |
| Routing (from Master Data) | Defines which resources are required for which products, linking planned production to resource load. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Capacity Management Policy (PO‑S‑050) | Governs utilization targets, overtime rules, subcontracting rules, flexibility constraints, risk assessment criteria, stability/volatility thresholds, investment and improvement signal criteria, and assessment frequency. |

**Business Guarantees:**
- Every active resource has a continuously maintained current interpretation of its capacity position (utilization, constraints, bottlenecks, flexibility, risk, load composition).
- A periodic Capacity Health Assessment is published for management oversight, investment planning, and operational improvement.
- All state changes and assessments are permanently retained with explainability.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑011 | Maintain Capacity Position Understanding | BW‑S‑030 | Continuously interpret current and projected capacity status per resource — utilization, constraints, bottlenecks, flexibility, risk, load composition, and confidence. | FS‑S‑030 |
| CR‑S‑012 | Assess Capacity Health | BW‑S‑031 | Publish a periodic, authoritative, versioned assessment of capacity health, stability, and volatility for management oversight, investment planning, and operational improvement. | FS‑S‑031 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑030 | Capacity Position Assessment Changed | AB‑S‑030 |
| EV‑S‑031 | Capacity Health Assessment Published | AB‑S‑031 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑030 | EV‑S‑030 | Capacity Position Assessment Changed: Resource, Utilization Status Change, Constraint Change, Bottleneck Change, Flexibility Change, Risk Change, Confidence | At‑least‑once | Per assessment | Near‑real‑time |
| BN‑S‑031 | EV‑S‑031 | Capacity Health Assessment Published: Assessment ID, Scope, Period, Utilization Summary, Stability & Volatility, Constraint Analysis, Bottleneck Impact, Risk Summary, Investment & Improvement Signals | At‑least‑once | Per assessment | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑001 Enterprise Supply Picture Published | Understand Supply | Re‑evaluate capacity position assessments. | FS‑S‑030 (conditional) |
| BN‑S‑010 Supply Plan Published | Plan Supply | Re‑evaluate capacity position assessments based on updated production loads. | FS‑S‑030 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Capacity Understanding Is Used |
|----------|--------------------------------------|
| Plan Supply | Uses Capacity Position Assessments (constraint feedback, bottleneck information, flexibility) to refine the next supply planning run. |
| Schedule Production | Uses resource availability, flexibility, and constraint horizon to develop feasible production schedules. |
| Evaluate Supply Quality | Uses Capacity Health Assessments (stability, volatility, trends) to measure capacity planning effectiveness. |
| Learn From Supply | Uses constraint patterns, utilization trends, and recurring bottleneck signals to identify capacity improvement opportunities. |

**Traceability:** Business Owner: CA‑S‑004. Publishes EV‑S‑030–031 and BN‑S‑030–031. Consumes BN‑S‑001 and BN‑S‑010. Realises BO‑S‑003, BO‑S‑005.

**Knowledge Handoff**

```
Enterprise Supply Picture ──→ Manage Capacity ──→ Plan Supply (constraint feedback, flexibility)
Supply Plan                   (position, health)   Schedule Production (availability, constraint horizon)
```

Manage Capacity transforms raw resource data, calendars, and planned production loads into interpreted enterprise understanding — which resources are constrained, where bottlenecks exist, what flexibility is available, what risks are present, and what improvements may be warranted. Plan Supply and Schedule Production consume the operational understanding; management and learning capabilities consume the periodic health assessments.


## 5.5 Collaborate with Suppliers — CA‑S‑005

**Business Intent:** Develop and maintain the enterprise’s authoritative understanding of supplier planning commitments and collaborative planning outcomes—enabling the enterprise to communicate supply requirements, receive supplier responses, assess commitment reliability and risk, and periodically evaluate collaboration health.

Before this capability executes, the enterprise has a supply plan identifying what must be procured, but no shared understanding with suppliers about their ability or intent to deliver. After this capability, the enterprise has firm supplier commitments (or documented alternatives), an interpreted understanding of each supplier’s reliability and risk posture, and periodic management assessments of collaboration quality. This serves as the collaborative bridge between enterprise planning and supplier participation—not supplier master data management, not procurement execution, not legal contract management.

**Owned Semantic Objects:** SE‑S‑050 (Supplier Commitment), SE‑S‑051 (Supplier Commitment Assessment), SE‑S‑052 (Supplier Collaboration Health Assessment).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Collaborate with Suppliers |
|------------|--------------------------------------|
| Supply Plan (SE‑S‑020) | Identifies what must be procured from which suppliers, at what quantities, and by when—the enterprise’s supply requirements that become the basis for collaboration. |
| Inventory Position Assessment (SE‑S‑030) | Identifies where shortages or at‑risk positions exist that require supplier action. |
| Capacity Position Assessment (SE‑S‑040) | For suppliers providing capacity‑constrained resources, identifies where supplier capacity may be insufficient. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Supplier (from Core Reference Data) | Defines the suppliers available for collaboration—names, classifications, contact information. The capability does not manage supplier master data. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Supplier Collaboration Policy (PO‑S‑060) | Governs commitment state transitions, confidence thresholds, assessment criteria, risk signal definitions, health classification thresholds, and assessment frequency. |

**Business Guarantees:**
- Every supply requirement that requires supplier collaboration is communicated and tracked as a Supplier Commitment with full bidirectional history.
- Every active supplier has a continuously maintained current interpretation of its commitment posture, responsiveness, reliability, and risk.
- A periodic Supplier Collaboration Health Assessment is published for management oversight and supplier development.
- All commitments, assessments, and collaboration histories are permanently retained.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑013 | Maintain Supplier Commitments | BW‑S‑050 | Communicate enterprise supply requirements to suppliers, capture their responses, and maintain the authoritative collaborative record. | FS‑S‑050 |
| CR‑S‑014 | Assess Supplier Collaboration Health | BW‑S‑051 | Publish a periodic, authoritative, versioned assessment of supplier collaboration quality for management oversight and planning policy refinement. | FS‑S‑051 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑040 | Supplier Commitment Changed | AB‑S‑040 |
| EV‑S‑041 | Supplier Commitment Assessment Changed | AB‑S‑041 |
| EV‑S‑042 | Supplier Collaboration Health Assessment Published | AB‑S‑042 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑040 | EV‑S‑040 | Supplier Commitment Changed: Supplier, Product, Required Date, Previous State, New State, Committed Quantity, Committed Date, Confidence | At‑least‑once | Per commitment | Near‑real‑time |
| BN‑S‑041 | EV‑S‑042 | Supplier Collaboration Health Assessment Published: Assessment ID, Scope, Period, Health Classification Summary, Responsiveness Trends, Commitment Accuracy Trends, Recommendation Signals | At‑least‑once | Per assessment | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑010 Supply Plan Published | Plan Supply | Identify new or changed supply requirements and propose commitments to suppliers. | FS‑S‑050 (conditional) |
| BN‑S‑020 Inventory Position Assessment Changed | Manage Inventory | Identify where shortages require supplier action and propose or expedite commitments. | FS‑S‑050 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Supplier Collaboration Understanding Is Used |
|----------|----------------------------------------------------|
| Plan Supply | Uses firm Supplier Commitments and Supplier Commitment Assessments (reliability, risk) to constrain the next planning run—applying buffers where commitments are uncertain and prioritising reliable suppliers. |
| Procure Materials | Uses Confirmed Supplier Commitments to generate purchase orders and determine order timing. |
| Evaluate Supply Quality | Uses Supplier Collaboration Health Assessments and commitment accuracy trends to measure planning collaboration effectiveness. |
| Learn From Supply | Uses collaboration patterns, reliability trends, and recommendation signals to identify improvement opportunities. |

**Traceability:** Business Owner: CA‑S‑005. Publishes EV‑S‑040–042 and BN‑S‑040–041. Consumes BN‑S‑010 and BN‑S‑020. Realises BO‑S‑004, BO‑S‑006.

**Knowledge Handoff**

```
Supply Plan ──→ Collaborate with Suppliers ──→ Plan Supply (constrained with commitments)
Inventory Assessment      (commitments,              Procure Materials (confirmed POs)
Capacity Assessment        assessments, health)
```

Collaborate with Suppliers transforms internal supply requirements into shared planning agreements with suppliers—capturing what they commit to deliver, with what confidence, under what assumptions. Plan Supply and Procure Materials consume this collaborative understanding to refine plans and execute procurement, extending the reasoning chain toward supply assurance.

## 5.6 Procure Materials — CA‑S‑006

**Business Intent:** Convert the Supply Plan’s procurement requirements into specific, governed, and traceable sourcing decisions—determining what to buy, from whom, when, in what quantities, and with what confidence—and publish the authoritative procurement baseline for each planning cycle.

Before this capability executes, the enterprise knows what must be procured (from the Supply Plan) and has supplier commitments and assessments (from Collaborate with Suppliers), but no specific sourcing decisions. After this capability, the enterprise has a continuously maintained Procurement Recommendation for every requirement—with full sourcing rationale, evidence, confidence, and feasibility assessment—and a periodic published Procurement Plan that serves as the authoritative baseline for procurement execution.

This capability is procurement intelligence, not procurement execution. It does not create purchase orders, transmit them, or handle receipts. Its output is the planning baseline that execution systems consume.

**Owned Semantic Objects:** SE‑S‑060 (Procurement Recommendation), SE‑S‑061 (Procurement Plan).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Procure Materials |
|------------|---------------------------|
| Supply Plan (SE‑S‑020) | Provides planned procurement quantities—the primary input defining what must be bought, by when, for which location. |
| Inventory Position Assessment (SE‑S‑030) | Provides current coverage and stockout risk—used to prioritise, expedite, or defer procurement. Does not own inventory policy. |
| Capacity Position Assessment (SE‑S‑040) | For suppliers providing capacity‑constrained materials, identifies whether supplier capacity is sufficient. |
| Supplier Commitment (SE‑S‑050) | Provides current supplier commitments—what has already been agreed, avoiding duplicate procurement and identifying gaps. |
| Supplier Commitment Assessment (SE‑S‑051) | Provides reliability, responsiveness, and risk signals per supplier—used in sourcing decisions and confidence assessment. |
| Supplier Collaboration Health Assessment (SE‑S‑052) | Provides long‑term collaboration trends—used to identify suppliers whose collaboration is improving or deteriorating. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Supplier (from Core Reference Data) | Approved supplier lists, lead times, MOQs, order multiples, contractual terms. Read by the capability; not managed here. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Procurement Policy (PO‑S‑070) | Governs sourcing rules, supplier allocation preferences, confidence thresholds, feasibility criteria, publication cadence, and risk signal thresholds. |

**Business Guarantees:**
- Every procurement requirement from the Supply Plan has a continuously maintained, current Procurement Recommendation with full sourcing rationale, evidence, confidence, and feasibility assessment.
- A periodic Procurement Plan is published as the authoritative procurement baseline for each planning cycle.
- All recommendations and plans are permanently retained with decision evidence.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑015 | Create Procurement Recommendations | BW‑S‑060 | For each procurement requirement, evaluate suppliers, constraints, and risks to produce a governed sourcing decision. | FS‑S‑060 |
| CR‑S‑016 | Publish Procurement Plan | BW‑S‑061 | Compose the current recommendations into the authoritative procurement baseline for the planning cycle. | FS‑S‑061 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑050 | Procurement Recommendation Created | AB‑S‑050 |
| EV‑S‑051 | Procurement Plan Published | AB‑S‑051 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑050 | EV‑S‑050 | Procurement Recommendation Created: Requirement Reference, Supplier Allocations, Confidence, Feasibility | At‑least‑once | Per recommendation | Near‑real‑time |
| BN‑S‑051 | EV‑S‑051 | Procurement Plan Published: Planning Scope, Planning Cycle, Version, Aggregate Spend, Supplier Concentration, Risk Signals, Recommendation Signals | At‑least‑once | Per plan | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑010 Supply Plan Published | Plan Supply | Create or revise procurement recommendations for new or changed requirements. | FS‑S‑060 (conditional) |
| BN‑S‑040 Supplier Commitment Changed | Collaborate with Suppliers | Re‑evaluate recommendations where supplier commitments have changed. | FS‑S‑060 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Procurement Understanding Is Used |
|----------|----------------------------------------|
| Procurement Execution (external) | Uses the published Procurement Plan to create purchase orders and transmit them to suppliers. |
| Explain Supply Decisions | Uses the sourcing rationale, evidence, and confidence from Procurement Recommendations to generate explanations. |
| Evaluate Supply Quality | Uses plan adherence and supplier allocation accuracy to measure procurement planning effectiveness. |
| Learn From Supply | Uses sourcing patterns, feasibility issues, and risk signals to identify improvement opportunities. |

**Traceability:** Business Owner: CA‑S‑006. Publishes EV‑S‑050–051 and BN‑S‑050–051. Consumes BN‑S‑010 and BN‑S‑040. Realises BO‑S‑004, BO‑S‑005, BO‑S‑006.

**Knowledge Handoff**

```
Supply Plan ──→ Procure Materials ──→ Procurement Execution (PO creation)
Supplier         (recommendations,      Explain Supply Decisions
Commitments       procurement plan)      Evaluate Supply Quality
```

Procure Materials is the final planning capability before procurement execution begins. It converges all upstream supply intelligence—planned requirements, inventory positions, capacity assessments, supplier commitments and reliability—into specific, governed, traceable sourcing decisions and a published procurement baseline.


## 5.7 Schedule Production — CA‑S‑007

**Business Intent:** Transform the Supply Plan’s planned production quantities into executable, sequenced, time‑phased production schedules for every constrained resource—respecting finite capacity, changeovers, material availability, and operational constraints—and publish the authoritative scheduling baseline for each planning period.

Before Schedule Production, the enterprise knows what to produce and roughly when (from the Supply Plan), but not in what sequence, on which specific resource, or at what precise time. After Schedule Production, the enterprise has a continuously maintained Production Schedule per resource per period with full sequencing, timing, changeover, feasibility, confidence, stability, and rationale—and a periodic published baseline that production execution systems consume.

This capability is production scheduling, not production execution. It does not dispatch jobs, record production output, control machines, or manage shop‑floor execution.

**Owned Semantic Objects:** SE‑S‑070 (Production Schedule), SE‑S‑071 (Published Production Schedule).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Schedule Production |
|------------|-----------------------------|
| Supply Plan (SE‑S‑020) | Provides planned production quantities per product, location, and period—the demand that scheduling must satisfy. |
| Inventory Position Assessment (SE‑S‑030) | Confirms material availability for production orders. |
| Capacity Position Assessment (SE‑S‑040) | Provides resource availability, constraints, flexibility, and bottleneck information. |
| Supplier Commitment (SE‑S‑050) | Confirms that inbound materials will arrive in time for scheduled production. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Resource (from Master Data) | Defines production resources (lines, machines, cells) with capacity, calendars, shifts, and capabilities. |
| Routings and BOMs (from Master Data) | Define how products flow through resources, operation sequences, and material requirements. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Scheduling Policy (PO‑S‑080) | Governs sequencing rules, changeover matrices, overtime limits, feasibility criteria, stability thresholds, confidence thresholds, and publication cadence. |

**Business Guarantees:**
- Every constrained production resource has a continuously maintained, current Production Schedule with full sequencing, feasibility, confidence, and stability.
- A periodic Published Production Schedule is released as the authoritative scheduling baseline for each planning period.
- All schedules and baselines are permanently retained with decision evidence.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑017 | Create Production Schedules | BW‑S‑070 | Sequence and time production activities on each constrained resource to satisfy the Supply Plan while respecting all finite constraints. | FS‑S‑070 |
| CR‑S‑018 | Publish Production Schedule Baseline | BW‑S‑071 | Compose the current schedules into the authoritative scheduling baseline for the planning period. | FS‑S‑071 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑060 | Production Schedule Created | AB‑S‑060 |
| EV‑S‑061 | Production Schedule Published | AB‑S‑061 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑060 | EV‑S‑060 | Production Schedule Created: Resource, Planning Period, Version, Feasibility, Confidence, Stability | At‑least‑once | Per schedule | Near‑real‑time |
| BN‑S‑061 | EV‑S‑061 | Production Schedule Published: Planning Scope, Planning Period, Version, Feasibility Summary, Stability Summary, Risk Signals | At‑least‑once | Per baseline | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑010 Supply Plan Published | Plan Supply | Create or revise production schedules for new or changed planned production quantities. | FS‑S‑070 (conditional) |
| BN‑S‑030 Capacity Position Assessment Changed | Manage Capacity | Re‑evaluate schedule feasibility where resource availability or constraints have changed. | FS‑S‑070 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Scheduling Understanding Is Used |
|----------|----------------------------------------|
| Production Execution (external) | Uses the Published Production Schedule to dispatch jobs to the shop floor. |
| Explain Supply Decisions | Uses sequencing rationale, feasibility evidence, and decision rationale to generate scheduling explanations. |
| Evaluate Supply Quality | Uses schedule adherence, stability metrics, and feasibility trends to measure scheduling effectiveness. |

**Traceability:** Business Owner: CA‑S‑007. Publishes EV‑S‑060–061 and BN‑S‑060–061. Consumes BN‑S‑010 and BN‑S‑030. Realises BO‑S‑003, BO‑S‑004, BO‑S‑007.

**Knowledge Handoff**

```
Supply Plan ──→ Schedule Production ──→ Production Execution (job dispatch)
Capacity         (schedules,               Explain Supply Decisions
Assessment        published baseline)       Evaluate Supply Quality
```

Schedule Production is the final Supply Intelligence planning capability before production execution begins. It transforms planned quantities into sequenced, timed, resource‑specific production schedules—the last planning step before the shop floor.


## 5.8 Manage Distribution — CA‑S‑008

**Business Intent:** Develop the enterprise’s authoritative understanding of how supply should be positioned across the network—determining what to move, from where to where, when, and why—and publish the authoritative distribution baseline for each planning cycle.

Before Manage Distribution, the enterprise knows what must be moved (from the Supply Plan), where inventory shortages exist (from Inventory Position Assessments), and what inbound supply is arriving (from Procurement Plan and Production Schedule). After Manage Distribution, the enterprise has a continuously maintained Distribution Recommendation for every requirement—with full movement rationale, strategy, allocation outcomes, feasibility, and confidence—and a periodic published Distribution Plan that serves as the authoritative baseline for distribution execution.

This capability is distribution intelligence, not transportation execution. It does not route trucks, dispatch shipments, select carriers, or manage freight. Its output is the planning baseline that execution systems consume.

**Owned Semantic Objects:** SE‑S‑080 (Distribution Recommendation), SE‑S‑081 (Distribution Plan).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Manage Distribution |
|------------|-----------------------------|
| Supply Plan (SE‑S‑020) | Provides planned transfer quantities—the primary input. |
| Inventory Position Assessment (SE‑S‑030) | Identifies stockout risks (requiring inbound) and excess positions (available for rebalancing). |
| Procurement Plan (SE‑S‑061) | Provides inbound procurement timing—used to synchronise distribution with incoming supply. |
| Production Schedule (SE‑S‑071) | Provides production completion timing—used to synchronise distribution with available output. |
| Supplier Commitment (SE‑S‑050) | Confirms inbound material availability for cross‑docking or direct distribution. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Network Topology, Lanes, Transit Times (from Master Data) | Define permissible movements, capacities, lead times, and costs between network nodes. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Distribution Policy (PO‑S‑090) | Governs sourcing rules, lane preferences, allocation rules, feasibility criteria, stability thresholds, confidence thresholds, and publication cadence. |

**Business Guarantees:**
- Every distribution requirement has a continuously maintained, current Distribution Recommendation with full movement rationale, strategy, feasibility, confidence, and—when supply is constrained—allocation strategy and outcome.
- A periodic Distribution Plan is published as the authoritative distribution baseline for each planning cycle.
- All recommendations and plans are permanently retained with decision evidence.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑019 | Create Distribution Recommendations | BW‑S‑080 | For each distribution requirement, evaluate sources, lanes, constraints, and costs to produce a governed network positioning decision. | FS‑S‑080 |
| CR‑S‑020 | Publish Distribution Plan | BW‑S‑081 | Compose the current recommendations into the authoritative distribution baseline for the planning cycle. | FS‑S‑081 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑070 | Distribution Recommendation Created | AB‑S‑070 |
| EV‑S‑071 | Distribution Plan Published | AB‑S‑071 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑070 | EV‑S‑070 | Distribution Recommendation Created: Requirement Reference, Sources, Lanes, Timing, Allocation (if constrained), Confidence | At‑least‑once | Per recommendation | Near‑real‑time |
| BN‑S‑071 | EV‑S‑071 | Distribution Plan Published: Planning Scope, Planning Cycle, Version, Aggregate Metrics, Risk Signals, Recovery Opportunities | At‑least‑once | Per plan | Batch |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behaviour | Invokes |
|---------------------|-----------|-------------------|---------|
| BN‑S‑010 Supply Plan Published | Plan Supply | Create or revise distribution recommendations for new or changed transfer requirements. | FS‑S‑080 (conditional) |
| BN‑S‑020 Inventory Position Assessment Changed | Manage Inventory | Trigger rebalancing recommendations where shortages or excesses exist. | FS‑S‑080 (conditional) |

### Downstream Consumer Relationships

| Consumer | How Distribution Understanding Is Used |
|----------|----------------------------------------|
| Distribution Execution (external) | Uses the published Distribution Plan to create shipments and dispatch movements. |
| Explain Supply Decisions | Uses movement rationale, allocation strategy, and decision evidence to generate distribution explanations. |
| Evaluate Supply Quality | Uses plan adherence, lane utilisation, and stability metrics to measure distribution effectiveness. |
| Learn From Supply | Uses movement patterns, allocation outcomes, and risk signals to identify improvement opportunities. |

**Traceability:** Business Owner: CA‑S‑008. Publishes EV‑S‑070–071 and BN‑S‑070–071. Consumes BN‑S‑010 and BN‑S‑020. Realises BO‑S‑002, BO‑S‑004, BO‑S‑005.

**Knowledge Handoff**

```
Supply Plan ─────→ Manage Distribution ──────→ Distribution Execution (shipment creation)
Inventory           (recommendations,                Explain Supply Decisions
Assessment          distribution plan)               Evaluate Supply Quality
Procurement Plan
Production Schedule
```

Manage Distribution optimizes network positioning—determining where supply should be deployed to satisfy demand at the right place and time. It is the final planning capability before distribution execution begins.


## 5.9 Sense Supply Changes — CA‑S‑009

**Business Intent:** Develop and maintain the enterprise’s continuous awareness of what has changed in the supply ecosystem—normalising heterogeneous signals into a canonical understanding of supply changes, with evidence, confidence, materiality, temporal context, and change correlation.

Sense Supply Changes answers one enterprise question: *“What has changed in the supply ecosystem that the enterprise now understands to be true?”* It does not determine whether a change is an exception, who should respond, or how to recover. Those responsibilities belong to Detect Supply Exceptions (5.11).

Before Sense Supply Changes, the enterprise receives raw signals from multiple sources—supplier commitments, inventory positions, capacity assessments, procurement recommendations, distribution recommendations, and external data. After Sense Supply Changes, the enterprise has a continuously maintained Supply Change Assessment for every monitored entity and change type, providing a single authoritative awareness of what has changed, why, with what confidence, and whether it matters to planning.

This capability is the bridge from planning to continuous operational awareness. It feeds the intelligence loop: Evaluate Supply Quality (5.10), Detect Supply Exceptions (5.11), Explain Supply Decisions (5.12), and Learn From Supply (5.13).

**Owned Semantic Objects:** SE‑S‑090 (Supply Change Assessment).

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities — heterogeneous signals normalised into canonical changes)

| Dependency | What It Provides |
|------------|-----------------|
| Supplier Commitment (SE‑S‑050) | Changes in supplier responses—delays, quantity changes, rejections, new commitments. |
| Enterprise Supply Picture (SE‑S‑001) | Changes in inventory positions, capacity status, supply orders. |
| Capacity Position Assessment (SE‑S‑040) | Changes in resource availability, constraints, bottlenecks. |
| Procurement Recommendation (SE‑S‑060) | Changes in sourcing decisions, supplier allocations. |
| Distribution Recommendation (SE‑S‑080) | Changes in network positioning, lane usage, allocation outcomes. |
| External Sources (future) | IoT signals, weather events, port closures, transportation delays, ERP updates. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Supply Change Policy (PO‑S‑100) | Governs change detection thresholds, corroboration requirements, materiality criteria, confidence assessment rules, and lifecycle management. |

**Business Guarantees:**
- Every monitored supply entity has a continuously maintained current awareness of changes, per change type.
- Heterogeneous signals are normalised into a canonical enterprise change representation.
- Every change assessment records evidence, confidence, materiality, temporal context, and correlated changes.
- Change history is permanently preserved with immutable lifecycle events.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | Purpose | FS |
|----|----------------|-------------------|---------|-----|
| CR‑S‑021 | Assess Supply Changes | BW‑S‑090 | Continuously monitor supply signals, detect changes from baseline, assess materiality and confidence, and maintain current change awareness. | FS‑S‑080 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV‑S‑080 | Supply Change Assessment Updated | AB‑S‑080 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN‑S‑080 | EV‑S‑080 | Supply Change Assessment Updated: Entity, Change Type, Change State, Materiality, Confidence, Change Description | At‑least‑once | Per assessment | Near‑real‑time |

*Note:* Whether state transitions (Observed, Corroborated, Confirmed) each publish BN‑S‑080, or only certain transitions publish, is governed by the Supply Change Policy (PO‑S‑100). The default is to publish on every state transition.

### Business Notifications Consumed

*(Sense Supply Changes consumes raw signals through its dependencies, not through Business Notifications from other capabilities. It monitors enterprise understanding by reading the current state of upstream aggregates, not by subscribing to their notification streams.)*

### Downstream Consumer Relationships

| Consumer | How Supply Change Awareness Is Used |
|----------|--------------------------------------|
| Detect Supply Exceptions (5.11) | Determines whether changes constitute enterprise exceptions requiring action. |
| Plan Supply | Consumes awareness of supply constraints and changes. Whether a plan revision is triggered is a separate planning decision—Plan Supply may choose to replan, defer, or monitor based on its own governance. |
| Evaluate Supply Quality | Uses change frequency and patterns to evaluate planning stability. |
| Learn From Supply | Uses change patterns and correlations to identify systemic supply risks and improvement opportunities. |
| Explain Supply Decisions | Uses change evidence and temporal context to generate explanations for planning decisions. |

**Traceability:** Business Owner: CA‑S‑009. Publishes EV‑S‑080 and BN‑S‑080. Realises BO‑S‑001, BO‑S‑004.

**Knowledge Handoff**

```
Supplier Commitments ──→ Sense Supply Changes ──→ Detect Supply Exceptions
Enterprise Supply Picture    (change awareness)      Evaluate Supply Quality
Capacity Position Assessments                        Learn From Supply
Procurement Recommendations                          Explain Supply Decisions
Distribution Recommendations
External Signals
```

Sense Supply Changes is the bridge from planning to continuous operational awareness. It transforms heterogeneous signals into a single canonical understanding of what has changed, feeding the downstream intelligence capabilities that evaluate, detect, explain, and learn.

## 5.10 Evaluate Supply Quality — CA‑S‑010 (stub)
## 5.11 Detect Supply Exceptions — CA‑S‑011 (stub)
## 5.12 Explain Supply Decisions — CA‑S‑012 (stub)
## 5.13 Learn From Supply — CA‑S‑013 (stub)

## 5.14 Business Workflows

Each Business Workflow realises exactly one Capability Responsibility and is executable through exactly one Functional Specification.

| ID | Business Intent | Realises | FS | Trigger | Workflow Nodes |
|----|-----------------|----------|----|---------|----------------|
| BW‑S‑001 | Ensure every received supply transaction becomes one traceable enterprise record. | CR‑S‑001 | FS‑S‑001 | Supply data received | Start → AB‑S‑001 → End |
| BW‑S‑002 | Ensure every received supply record receives one governed evaluation outcome. | CR‑S‑002 | FS‑S‑002 | EV‑S‑001 | Start → AB‑S‑002 / DE‑S‑010 → Notification if Quarantined or Rejected → End |
| BW‑S‑003 | Ensure accepted supply data is incorporated into the current draft supply understanding. | CR‑S‑003 | FS‑S‑003 | EV‑S‑002 with Accepted outcome | Start → AB‑S‑003 → End |
| BW‑S‑004 | Ensure only materially changed and complete supply pictures become authoritative. | CR‑S‑004 | FS‑S‑004 | Draft Enterprise Supply Picture available | Start → AB‑S‑005 / DE‑S‑011 → BN‑S‑001 if Published → End |
| BW‑S‑010 | Establish a feasible draft supply plan from governed demand, supply, and planning inputs. | CR‑S‑005 | FS‑S‑010 | Schedule or planning input update | Start → AB‑S‑010 / BA‑S‑001 → End |
| BW‑S‑011 | Ensure the draft supply plan is evaluated before publication. | CR‑S‑006 | FS‑S‑011 | EV‑S‑010 | Start → AB‑S‑011 / DE‑S‑020 → Escalation per PO‑S‑021 if rejected → End |
| BW‑S‑012 | Establish the accepted supply plan as the authoritative planning baseline. | CR‑S‑007 | FS‑S‑012 | EV‑S‑011 with accepted outcome | Start → AB‑S‑012 / DE‑S‑021 → BN‑S‑010 if Published → End |
| BW‑S‑020 | Maintain current interpreted inventory position understanding for each active product‑location. | CR‑S‑008 | FS‑S‑020 | BN‑S‑001, BN‑S‑010, or scheduled re‑evaluation | Start → Fork per product‑location → AB‑S‑020 / DE‑S‑030 → BN‑S‑020 if changed → Join → End |
| BW‑S‑021 | Maintain current governed inventory policy assignments for each active product‑location. | CR‑S‑009 | FS‑S‑021 | BN‑S‑010, BN‑D‑011, or schedule | Start → Fork per product‑location → AB‑S‑021 / DE‑S‑031 → BN‑S‑021 if changed → Join → End |
| BW‑S‑022 | Publish periodic authoritative inventory health understanding. | CR‑S‑010 | FS‑S‑022 | Schedule per PO‑S‑034 | Start → AB‑S‑022 / DE‑S‑032 / BA‑S‑005 → BN‑S‑022 → End |
| BW‑S‑030 | Maintain current interpreted capacity position understanding for each active resource. | CR‑S‑011 | FS‑S‑030 | BN‑S‑001, BN‑S‑010, resource calendar update, or scheduled re‑evaluation | Start → Fork per resource → AB‑S‑030 / DE‑S‑040 → BN‑S‑030 if changed → Join → End |
| BW‑S‑031 | Publish periodic authoritative capacity health understanding. | CR‑S‑012 | FS‑S‑031 | Schedule per PO‑S‑050 | Start → AB‑S‑031 / DE‑S‑041 / BA‑S‑011 → BN‑S‑031 → End |
| BW‑S‑050 | Maintain current supplier commitments and capture bidirectional collaboration history. | CR‑S‑013 | FS‑S‑050 | BN‑S‑010, BN‑S‑020, or supplier response received | Start → Fork per requirement → AB‑S‑040 / DE‑S‑050 → BN‑S‑040 if changed → Join → End |
| BW‑S‑051 | Publish periodic authoritative supplier collaboration health understanding. | CR‑S‑014 | FS‑S‑051 | Schedule per PO‑S‑060 | Start → AB‑S‑042 / DE‑S‑051 / BA‑S‑022 → BN‑S‑041 → End |
| BW‑S‑060 | Create governed procurement recommendations for each supply plan requirement. | CR‑S‑015 | FS‑S‑060 | BN‑S‑010, BN‑S‑040, or scheduled re‑evaluation | Start → Fork per requirement → AB‑S‑050 / DE‑S‑060 / BA‑S‑030 / BA‑S‑031 → BN‑S‑050 if created or revised → Join → End |
| BW‑S‑061 | Publish the authoritative procurement baseline for the planning cycle. | CR‑S‑016 | FS‑S‑061 | Schedule per PO‑S‑070 | Start → AB‑S‑051 / DE‑S‑061 → BN‑S‑051 → End |
| BW‑S‑070 | Create sequenced, feasible production schedules for each constrained resource. | CR‑S‑017 | FS‑S‑070 | BN‑S‑010, BN‑S‑030, or scheduled re‑evaluation | Start → Fork per resource‑period → AB‑S‑060 / DE‑S‑070 / DE‑S‑071 / BA‑S‑040 / BA‑S‑041 / BA‑S‑042 → BN‑S‑060 if created or revised → Join → End |
| BW‑S‑071 | Publish the authoritative production scheduling baseline for the planning period. | CR‑S‑018 | FS‑S‑071 | Schedule per PO‑S‑080 | Start → AB‑S‑061 / DE‑S‑072 → BN‑S‑061 → End |
| BW‑S‑080 | Create governed distribution recommendations for each distribution requirement. | CR‑S‑019 | FS‑S‑080 | BN‑S‑010, BN‑S‑020, or scheduled re‑evaluation | Start → Fork per requirement → AB‑S‑070 / DE‑S‑080 / BA‑S‑050 / BA‑S‑051 → BN‑S‑070 if created or revised → Join → End |
| BW‑S‑081 | Publish the authoritative distribution baseline for the planning cycle. | CR‑S‑020 | FS‑S‑081 | Schedule per PO‑S‑090 | Start → AB‑S‑071 / DE‑S‑081 → BN‑S‑071 → End |
| BW‑S‑090 | Continuously assess supply changes across all monitored entities and change types. | CR‑S‑021 | FS‑S‑080 | Incoming supply signals or scheduled re‑evaluation | Start → Fork per entity‑change type → AB‑S‑080 / DE‑S‑090 / BA‑S‑060 / BA‑S‑061 → BN‑S‑080 if state changed → Join → End |


*Note:* Business Workflow identifiers use a numbering convention: BW‑S‑001–009 are reserved for Understand Supply (5.1), BW‑S‑010–019 for Plan Supply (5.2), BW‑S‑020–029 for Manage Inventory (5.3), and so on for future capabilities. This intentional gap allows each capability to have its own BW number range.

---

# Chapter 6 — Decision Model

## DE‑S‑010 — Accept Supply Data

**Decision Owner:** AB‑S‑002 Evaluate Supply Data  
**Purpose:** Validate incoming supply data and determine whether it is trustworthy enough to incorporate into the Enterprise Supply Picture.  
**Alternatives:** Accept, Accept with Flag, Quarantine, Reject.  
**Criteria Evaluation:** BR‑S‑010 (timeliness), BR‑S‑011 (range validity), BR‑S‑012 (source reliability), BR‑S‑013 (duplicate detection). All mandatory.  
**Conflict Resolution:** Most severe outcome prevails (Reject over Quarantine).  
**Decision Confidence:** Derived from source reliability index, data freshness, and signal consistency.  
**Rationale Template:** "Supply data accepted: source {source}, type {type}, quantity {qty}, timestamp age {age} min, within expected range. Source reliability {rel}%."

**Traceability:** Decision Owner: AB‑S‑002. Invoked by: FS‑S‑002. References: BR‑S‑010–013. Governed By: PO‑S‑010.

## DE‑S‑011 — Publish Enterprise Supply Picture

**Decision Owner:** AB‑S‑005 Publish Enterprise Supply Picture  
**Purpose:** Determine whether the revised picture meets publication criteria.  
**Alternatives:** Publish, Do Not Publish.  
**Criteria Evaluation:** BR‑S‑014 (materiality threshold met for at least one knowledge category), BR‑S‑015 (data completeness).  
**Conflict Resolution:** If no material change → Do Not Publish.  
**Confidence:** Binary.

**Traceability:** Decision Owner: AB‑S‑005. Invoked by: FS‑S‑004. References: BR‑S‑014, BR‑S‑015. Governed By: PO‑S‑011.

## DE‑S‑020 — Accept Supply Plan

**Decision Owner:** AB‑S‑011 Evaluate Supply Plan  
**Purpose:** Assess the generated supply plan against business KPIs (cost, service, capacity utilization, inventory levels) and determine whether it meets quality thresholds for acceptance.  
**Alternatives:** Accept and proceed to publication, Accept with warnings (minor deviations documented), Reject and re‑plan.  
**Criteria:** BR‑S‑025 (quality thresholds: service level ≥ target, capacity utilization ≤ max, projected inventory within policy bounds, total cost within budget variance), BR‑S‑026 (plan stability within frozen period).  
**Conflict Resolution:** If any mandatory quality threshold fails → Reject and either re‑plan or escalate.  
**Rationale Template:** "Supply plan evaluated: service level {x}%, capacity utilization {y}%, cost {z}. All thresholds met. Plan accepted."

**Traceability:** Decision Owner: AB‑S‑011. Invoked by: FS‑S‑011. References: BR‑S‑025, BR‑S‑026. Governed By: PO‑S‑022.

## DE‑S‑021 — Publish Supply Plan

**Decision Owner:** AB‑S‑012 Publish Supply Plan  
**Purpose:** Finalize and release the accepted supply plan as authoritative for downstream consumption.  
**Alternatives:** Publish, Publish with documented exceptions, Hold (unresolved issues).  
**Criteria:** BR‑S‑027 (plan must be accepted by DE‑S‑020 or approved by manual override), BR‑S‑028 (versioning).  
**Rationale Template:** "Supply Plan v{version} published for Planning Scope {scope}, horizon {horizon}."

**Traceability:** Decision Owner: AB‑S‑012. Invoked by: FS‑S‑012. References: BR‑S‑027, BR‑S‑028. Governed By: PO‑S‑023.

## DE‑S‑030 — Determine Inventory Position Status

**Decision Owner:** AB‑S‑020 Update Inventory Position Assessment  
**Purpose:** For a given product‑location, evaluate current and projected inventory against policy targets to determine coverage adequacy, health status, and risk levels.  
**Alternatives:** Normal, Under‑Stocked, Over‑Stocked, At Risk (projected stockout), Obsolete.  
**Criteria:** BR‑S‑040 (coverage thresholds), BR‑S‑041 (risk assessment criteria), BR‑S‑042 (obsolescence criteria).  
**Conflict Resolution:** If multiple conditions apply, the most severe (At Risk > Obsolete > Under‑Stocked > Over‑Stocked > Normal) prevails for health status; risks are assessed independently.  
**Rationale Template:** "{product‑location}: Days of Supply {dos}, Target {target}. Health: {status}. Risks: {risk_summary}."

**Traceability:** Decision Owner: AB‑S‑020. Invoked by: FS‑S‑020. References: BR‑S‑040–042. Governed By: PO‑S‑032.

## DE‑S‑031 — Determine Inventory Policy

**Decision Owner:** AB‑S‑021 Update Inventory Policy  
**Purpose:** For a given product‑location, compute the optimal inventory control parameters according to the Inventory Policy Governance.  
**Alternatives:** Apply statistical formula, Apply simulation‑based optimization, Apply manual override, Maintain current policy.  
**Criteria:** BR‑S‑046 (policy calculation rules), BR‑S‑047 (data sufficiency).  
**Conflict Resolution:** If insufficient data → retain current policy and flag for review.  
**Rationale Template:** "Policy for {product‑location} updated: safety stock {ss}, reorder point {rop}, lot size {ls}. Methodology: {method}."

**Traceability:** Decision Owner: AB‑S‑021. Invoked by: FS‑S‑021. References: BR‑S‑046, BR‑S‑047. Governed By: PO‑S‑033.

## DE‑S‑032 — Classify Inventory Health

**Decision Owner:** AB‑S‑022 Publish Inventory Health Assessment  
**Purpose:** For each product‑location in the assessment scope, classify its health status for the evaluation period according to the Inventory Health Policy.  
**Alternatives:** Optimal, Under‑Stocked, Over‑Stocked, At Risk, Obsolete.  
**Criteria:** BR‑S‑049 (health classification thresholds).  
**Rationale Template:** "{product‑location} classified as {health}: Days of Supply {dos} vs. target {target}."

**Traceability:** Decision Owner: AB‑S‑022. Invoked by: FS‑S‑022. References: BR‑S‑049. Governed By: PO‑S‑034.

## DE‑S‑040 — Determine Capacity Position Status

**Decision Owner:** AB‑S‑030 Update Capacity Position Assessment  
**Purpose:** For a given resource, evaluate current and projected load against Available Capacity to determine utilization status, constraint classification, bottleneck assessment, flexibility interpretation, and risk levels.  
**Alternatives:** Feasible, Near Capacity, Overloaded, Underutilized.  
**Criteria:** BR‑S‑060 (utilization thresholds), BR‑S‑061 (constraint classification), BR‑S‑062 (bottleneck criteria), BR‑S‑063 (flexibility assessment rules), BR‑S‑064 (risk assessment criteria).  
**Conflict Resolution:** If multiple conditions apply across time buckets, the most severe (Overloaded > Near Capacity > Underutilized > Feasible) prevails for status; constraints, bottlenecks, risks, and flexibility are assessed per time bucket.  
**Rationale Template:** "{resource}: Utilization {pct}%, Status: {status}. Constraints: {constraint_summary}. Bottleneck: {bottleneck}. Risks: {risk_summary}."

**Traceability:** Decision Owner: AB‑S‑030. Invoked by: FS‑S‑030. References: BR‑S‑060–064. Governed By: PO‑S‑050.

## DE‑S‑041 — Classify Capacity Health

**Decision Owner:** AB‑S‑031 Publish Capacity Health Assessment  
**Purpose:** For the assessment scope and evaluation period, classify overall capacity health, stability, and volatility; identify binding constraints and bottlenecks; and generate investment and improvement signals.  
**Alternatives:** Healthy, Constrained, Critically Constrained, Underutilized, Unstable.  
**Criteria:** BR‑S‑066 (health classification thresholds), BR‑S‑067 (stability and volatility thresholds), BR‑S‑068 (investment and improvement signal criteria).  
**Rationale Template:** "Scope {scope}: Capacity health {status}, Stability {stability}, Volatility {volatility}. Binding constraints: {bottlenecks}. Signals: {signals}."

**Traceability:** Decision Owner: AB‑S‑031. Invoked by: FS‑S‑031. References: BR‑S‑066–068. Governed By: PO‑S‑050.

---

## DE‑S‑050 — Accept Supplier Response

**Decision Owner:** AB‑S‑040 Record Supplier Commitment  
**Purpose:** Evaluate a supplier’s response (commitment, partial commitment, rejection, or alternative proposal) and determine whether the enterprise can accept it as firm, accept it with buffer, escalate, or reject.  
**Alternatives:** Accept as Firm, Accept with Buffer, Escalate for Review, Reject and Seek Alternative.  
**Criteria:** BR‑S‑080 (responsiveness thresholds), BR‑S‑081 (historical reliability thresholds), BR‑S‑082 (commitment confidence assessment), BR‑S‑083 (criticality of the requirement).  
**Conflict Resolution:** If the supplier commits fully and confidence is Firm or Conditional → Accept as Firm. If confidence is Tentative or Estimated → Accept with Buffer. If the supplier partially commits or proposes an alternative → Escalate for Review. If the supplier rejects → Reject and Seek Alternative.  
**Rationale Template:** "Supplier {supplier} response: {state}. Confidence: {confidence}. Decision: {outcome}. Buffer: {buffer_applied}."

**Traceability:** Decision Owner: AB‑S‑040. Invoked by: FS‑S‑050. References: BR‑S‑080–083. Governed By: PO‑S‑060.

## DE‑S‑051 — Classify Collaboration Health

**Decision Owner:** AB‑S‑042 Publish Supplier Collaboration Health Assessment  
**Purpose:** For the assessment scope and evaluation period, classify each supplier’s collaboration health based on responsiveness, commitment accuracy, and trend data.  
**Alternatives:** Excellent, Good, Needs Improvement, At Risk.  
**Criteria:** BR‑S‑090 (health classification thresholds).  
**Rationale Template:** "Supplier {supplier}: Collaboration health {status}. Responsiveness trend: {trend}. Accuracy trend: {trend}."

**Traceability:** Decision Owner: AB‑S‑042. Invoked by: FS‑S‑051. References: BR‑S‑090. Governed By: PO‑S‑060.

---

## DE‑S‑060 — Determine Sourcing Decision

**Decision Owner:** AB‑S‑050 Create Procurement Recommendation  
**Purpose:** For a given procurement requirement, evaluate eligible suppliers, constraints, commitments, and risks to produce the recommended sourcing allocation.  
**Alternatives:** Single Supplier, Intentional Split, Contingency Split, Alternate Supplier, Emergency Supplier, Infeasible (no viable option).  
**Criteria:** BR‑S‑100 (supplier eligibility), BR‑S‑101 (lead time feasibility), BR‑S‑102 (capacity availability), BR‑S‑103 (commitment confidence), BR‑S‑104 (cost and risk factors), BR‑S‑105 (procurement feasibility).  
**Conflict Resolution:** If no supplier meets all criteria → Infeasible (with documented constraints). If multiple suppliers qualify → allocation per sourcing rules in PO‑S‑070.  
**Rationale Template:** "Requirement {ref}: {allocation_summary}. Supplier(s): {suppliers}. Strategy: {strategy}. Confidence: {confidence}. Feasibility: {feasibility}."

**Traceability:** Decision Owner: AB‑S‑050. Invoked by: FS‑S‑060. References: BR‑S‑100–105. Governed By: PO‑S‑070.

## DE‑S‑061 — Publish Procurement Plan

**Decision Owner:** AB‑S‑051 Publish Procurement Plan  
**Purpose:** Determine whether the current set of Procurement Recommendations meets publication criteria and publish as the authoritative procurement baseline.  
**Alternatives:** Publish, Publish with Flags, Hold.  
**Criteria:** BR‑S‑111 (completeness of recommendations), BR‑S‑112 (confidence threshold), BR‑S‑113 (unresolved feasibility issues).  
**Rationale Template:** "Procurement Plan v{version} published for cycle {cycle}. Recommendations: {count}. Confidence: {confidence_distribution}. Risk signals: {signals}."

**Traceability:** Decision Owner: AB‑S‑051. Invoked by: FS‑S‑061. References: BR‑S‑111–113. Governed By: PO‑S‑070.

## DE‑S‑070 — Determine Production Sequence

**Decision Owner:** AB‑S‑060 Create Production Schedule  
**Purpose:** For a given resource and planning period, determine the optimal sequence of production activities to minimize changeovers, meet due dates, and respect all finite constraints.  
**Alternatives:** Earliest Due Date, Minimize Changeover, Priority‑Based (critical/high/medium), Hybrid (optimize within priority windows).  
**Criteria:** BR‑S‑120 (sequencing rules), BR‑S‑121 (changeover minimization), BR‑S‑122 (due date adherence), BR‑S‑123 (priority adherence).  
**Conflict Resolution:** If multiple strategies are viable, the one with the lowest total cost (changeover cost + lateness penalty) is selected per PO‑S‑080.  
**Rationale Template:** "Resource {resource}, period {period}: Sequence {sequence_summary}. Strategy: {strategy}. Changeover time: {hours}h. Due date adherence: {pct}%."

**Traceability:** Decision Owner: AB‑S‑060. Invoked by: FS‑S‑070. References: BR‑S‑120–123. Governed By: PO‑S‑080.

## DE‑S‑071 — Assess Schedule Feasibility

**Decision Owner:** AB‑S‑060 Create Production Schedule  
**Purpose:** For the sequenced schedule, verify that all prerequisite conditions are met for each production activity.  
**Alternatives:** Feasible, Conditionally Feasible (documented constraints), Infeasible (unresolvable constraints).  
**Criteria:** BR‑S‑124 (material availability), BR‑S‑125 (capacity availability), BR‑S‑126 (tool availability), BR‑S‑127 (labor availability), BR‑S‑128 (maintenance and calendar conflicts).  
**Conflict Resolution:** If any hard constraint fails → Infeasible until resolved. If soft constraints are violated → Conditionally Feasible with documentation.  
**Rationale Template:** "Schedule for {resource}, period {period}: Feasibility {status}. Constraints: {constraint_summary}."

**Traceability:** Decision Owner: AB‑S‑060. Invoked by: FS‑S‑070. References: BR‑S‑124–128. Governed By: PO‑S‑080.

## DE‑S‑072 — Publish Production Schedule

**Decision Owner:** AB‑S‑061 Publish Production Schedule  
**Purpose:** Determine whether the current Production Schedules meet publication criteria and publish as the authoritative scheduling baseline.  
**Alternatives:** Publish, Publish with Flags, Hold.  
**Criteria:** BR‑S‑133 (feasibility threshold), BR‑S‑134 (stability threshold), BR‑S‑135 (completeness of schedules).  
**Rationale Template:** "Production Schedule v{version} published for scope {scope}, period {period}. Feasibility: {feasibility_summary}. Stability: {stability_summary}."

**Traceability:** Decision Owner: AB‑S‑061. Invoked by: FS‑S‑071. References: BR‑S‑133–135. Governed By: PO‑S‑080.

## DE‑S‑080 — Determine Distribution Sourcing

**Decision Owner:** AB‑S‑070 Create Distribution Recommendation  
**Purpose:** For a given distribution requirement, evaluate source locations, lanes, constraints, and costs to produce the recommended network positioning decision.  
**Alternatives:** Single Source, Multi‑Source, Cross‑Dock, Defer (intentionally retain), Infeasible (no viable movement).  
**Criteria:** BR‑S‑140 (source inventory availability), BR‑S‑141 (lane capacity feasibility), BR‑S‑142 (transit time feasibility), BR‑S‑143 (receiving capacity), BR‑S‑144 (cost and deployment priority).  
**Conflict Resolution:** If supply is insufficient → apply Allocation Strategy (BR‑S‑145) and record allocation outcome. If no viable movement → Infeasible with documented constraints.  
**Rationale Template:** "Requirement {ref}: {movement_summary}. Strategy: {strategy}. Allocation: {allocation_summary} (if constrained). Confidence: {confidence}."

**Traceability:** Decision Owner: AB‑S‑070. Invoked by: FS‑S‑080. References: BR‑S‑140–145. Governed By: PO‑S‑090.

## DE‑S‑081 — Publish Distribution Plan

**Decision Owner:** AB‑S‑071 Publish Distribution Plan  
**Purpose:** Determine whether the current Distribution Recommendations meet publication criteria and publish as the authoritative distribution baseline.  
**Alternatives:** Publish, Publish with Flags, Hold.  
**Criteria:** BR‑S‑156 (completeness), BR‑S‑157 (confidence threshold), BR‑S‑158 (unresolved feasibility issues).  
**Rationale Template:** "Distribution Plan v{version} published for cycle {cycle}. Movements: {count}. Lane utilisation: {pct}%. Risk signals: {signals}."

**Traceability:** Decision Owner: AB‑S‑071. Invoked by: FS‑S‑081. References: BR‑S‑156–158. Governed By: PO‑S‑090.


## DE‑S‑090 — Assess Supply Change

**Decision Owner:** AB‑S‑080 Assess Supply Change  
**Purpose:** For a given monitored supply entity and change type, determine whether an incoming signal constitutes an actual change from the previously understood state, whether that change is material to planning, and with what confidence.  
**Alternatives:** Change Observed (awaiting corroboration), Change Corroborated (confirmed by independent sources), Change Confirmed — Material, Change Confirmed — Non‑Material, No Change (within normal variance), Resolved (condition reverted).  
**Criteria:** BR‑S‑170 (deviation threshold), BR‑S‑171 (corroboration requirements), BR‑S‑172 (materiality criteria), BR‑S‑173 (confidence assessment rules).  
**Conflict Resolution:** If deviation is below threshold → No Change. If deviation exceeds threshold but only one source → Observed (awaiting corroboration). If corroborated by independent sources → Corroborated or Confirmed based on evidence strength. If condition reverts → Resolved.  
**Rationale Template:** "Entity {entity}, Change Type {type}: {change_summary}. State: {state}. Materiality: {materiality}. Confidence: {confidence}. Evidence: {evidence_summary}."

**Traceability:** Decision Owner: AB‑S‑080. Invoked by: FS‑S‑080. References: BR‑S‑170–173. Governed By: PO‑S‑100.

# Chapter 7 — Rule Model

### Rule Precedence

| Rule Type | Overridable By |
|-----------|----------------|
| Identity | None |
| Invariant | None |
| Eligibility | Behaviour |
| Behaviour | (none below it) |
| Derivation | (none below it) |

### Identity Rules

| ID | Rule |
|----|------|
| BR‑S‑001 | Each Supply Data Record has a globally unique Supply Data Record Identifier assigned at creation. |
| BR‑S‑002 | Each Enterprise Supply Picture aggregate is uniquely identified by Planning Scope. |
| BR‑S‑003 | Each Supply Plan aggregate is uniquely identified by Planning Scope and Plan Horizon. |
| BR‑S‑004 | Each Inventory Position Assessment is uniquely identified by Product and Location. |
| BR‑S‑005 | Each Inventory Policy Assignment is uniquely identified by Product and Location. |
| BR‑S‑006 | Each Inventory Health Assessment aggregate is uniquely identified by Assessment Scope and Evaluation Period. |
| BR‑S‑007 | Each Capacity Position Assessment is uniquely identified by Resource. |
| BR‑S‑008 | Each Capacity Health Assessment aggregate is uniquely identified by Assessment Scope and Evaluation Period. |
| BR‑S‑009 | Each Supplier Commitment is uniquely identified by Supplier, Product, and Required Date within the active collaboration scope. The identity model is extensible to include location, planning scope, or delivery window. |
| BR‑S‑010 | Each Supplier Commitment Assessment is uniquely identified by Supplier. |
| BR‑S‑011 | Each Supplier Collaboration Health Assessment aggregate is uniquely identified by Assessment Scope and Evaluation Period. |
| BR‑S‑012 | Each Procurement Recommendation is uniquely identified by its Supply Plan Procurement Requirement Reference and Version. |
| BR‑S‑013 | Each Procurement Plan aggregate is uniquely identified by Planning Scope and Planning Cycle. |
| BR‑S‑014 | Each Production Schedule is uniquely identified by Resource, Planning Period, and Version. |
| BR‑S‑015 | Each Published Production Schedule aggregate is uniquely identified by Planning Scope and Planning Period. |
| BR‑S‑016 | Each Distribution Recommendation is uniquely identified by its Distribution Requirement Reference and Version. |
| BR‑S‑017 | Each Distribution Plan aggregate is uniquely identified by Planning Scope and Planning Cycle. |
| BR‑S‑018 | Each Supply Change Assessment is uniquely identified by Supply Entity Type, Supply Entity Identifier, and Change Type. |

### Eligibility Rules

| ID | Rule |
|----|------|
| BR‑S‑010 | Supply data timestamp must be within maximum allowed latency. |
| BR‑S‑011 | Supply quantities must be within valid range. |
| BR‑S‑012 | Source reliability must meet minimum threshold. |
| BR‑S‑013 | Duplicate data within the same window is rejected. |
| BR‑S‑014 | Publication requires material change per PO‑S‑011 for at least one knowledge category. |
| BR‑S‑015 | Data completeness must meet the threshold defined in PO‑S‑011. |

### Invariant Rules

| ID | Rule |
|----|------|
| BR‑S‑016 | Exactly one Published version exists per Planning Scope at any moment. |
| BR‑S‑017 | A Published version is immutable. |
| BR‑S‑018 | Only accepted supply data contributes. |
| BR‑S‑027 | The plan may only be published if it has been accepted by DE‑S‑020 or approved by manual override. |
| BR‑S‑028 | Every published plan receives a unique version identifier and is stored immutably. |
| BR‑S‑029 | All hard constraints must be satisfied or documented and approved as constraint relaxations before publication. |
| BR‑S‑043 | At any moment, an active product‑location has exactly one current Inventory Position Assessment. |
| BR‑S‑044 | The assessment must be derived from the latest published Enterprise Supply Picture and Supply Plan. |
| BR‑S‑045 | Assessment Change Events are immutable. |
| BR‑S‑048 | At any moment, an active product‑location has exactly one current Inventory Policy Assignment. |
| BR‑S‑050 | Exactly one Published Inventory Health Assessment exists for a given scope and evaluation period. |
| BR‑S‑051 | A Published Health Assessment is immutable. |
| BR‑S‑065 | At any moment, an active resource has exactly one current Capacity Position Assessment. |
| BR‑S‑066 | The assessment must be derived from the latest published Enterprise Supply Picture, Supply Plan, and resource calendars. |
| BR‑S‑067 | Assessment Change Events are immutable and must record the reason and evidence for the change. |
| BR‑S‑068 | Capacity health classification for periodic assessments shall use the thresholds governed by PO‑S‑050. |
| BR‑S‑069 | Stability and volatility metrics shall be computed according to the criteria governed by PO‑S‑050. |
| BR‑S‑070 | Investment and improvement signals shall be generated according to the criteria governed by PO‑S‑050. |
| BR‑S‑084 | Collaboration Events within a Supplier Commitment are immutable. |
| BR‑S‑085 | Only the latest supplier response determines the current commitment state. |
| BR‑S‑086 | At any moment, an active supplier has exactly one current Supplier Commitment Assessment. |
| BR‑S‑087 | The assessment must be derived from the current set of Supplier Commitments and the latest available reliability data. |
| BR‑S‑088 | Assessment Change Events are immutable. |
| BR‑S‑089 | Exactly one Published Supplier Collaboration Health Assessment exists for a given scope and evaluation period. |
| BR‑S‑090 | Collaboration health classification for periodic assessments shall use the thresholds governed by PO‑S‑060. |
| BR‑S‑091 | A Published Health Assessment is immutable. |
| BR‑S‑106 | At any moment, exactly one Active Procurement Recommendation exists for each procurement requirement. |
| BR‑S‑107 | The recommendation must be derived from the current Supply Plan, supplier commitments, inventory assessments, and capacity assessments. |
| BR‑S‑108 | Decision evidence must reference the specific artifacts used. |
| BR‑S‑109 | Versions are immutable once superseded or archived. |
| BR‑S‑110 | Exactly one Published Procurement Plan exists for a given Planning Scope and Planning Cycle. |
| BR‑S‑111 | The plan must cover all active procurement requirements for the planning cycle, or document exclusions. |
| BR‑S‑112 | The plan must meet the confidence threshold governed by PO‑S‑070, or be flagged for review. |
| BR‑S‑113 | Unresolved feasibility issues must be documented and flagged in the plan. |
| BR‑S‑129 | At any moment, exactly one Active Production Schedule exists for each resource‑period combination. |
| BR‑S‑130 | The schedule must respect all hard constraints (finite capacity, material prerequisites, mandatory maintenance windows). |
| BR‑S‑131 | Sequencing decisions must be documented with rationale and evidence. |
| BR‑S‑132 | Versions are immutable once superseded or archived. |
| BR‑S‑133 | The scheduling baseline must meet the feasibility threshold governed by PO‑S‑080, or be flagged for review. |
| BR‑S‑134 | The scheduling baseline must meet the stability threshold governed by PO‑S‑080, or be flagged for review. |
| BR‑S‑135 | The baseline must cover all active resources for the planning period, or document exclusions. |
| BR‑S‑146 | At any moment, exactly one Active Distribution Recommendation exists for each distribution requirement. |
| BR‑S‑147 | The recommendation must be derived from the current Supply Plan, Inventory Position Assessments, and network data. |
| BR‑S‑148 | When supply is constrained, both the allocation policy used and the allocation outcome must be recorded. |
| BR‑S‑149 | Decision evidence must reference the specific artifacts used. |
| BR‑S‑150 | Versions are immutable once superseded or archived. |
| BR‑S‑156 | The plan must cover all active distribution requirements for the planning cycle, or document exclusions. |
| BR‑S‑157 | The plan must meet the confidence threshold governed by PO‑S‑090, or be flagged for review. |
| BR‑S‑158 | Unresolved feasibility issues must be documented and flagged in the plan. |
| BR‑S‑174 | At any moment, for each monitored entity and change type, exactly one current Supply Change Assessment exists. |
| BR‑S‑175 | The current change state reflects the latest understanding based on all available signals. |
| BR‑S‑176 | Change History Events are immutable. |
| BR‑S‑177 | Heterogeneous signals from multiple sources shall be normalised into a canonical enterprise change representation. |
| BR‑S‑178 | Correlated changes shall be linked through explicit references to related Supply Change Assessments. |


### Behaviour Rules

| ID | Rule |
|----|------|
| BR‑S‑025 | The plan must meet minimum quality thresholds: service level ≥ target, capacity utilization ≤ max, projected inventory within policy bounds, total cost within budget variance. |
| BR‑S‑026 | The new plan must not deviate from the previously published plan beyond the stability threshold for the frozen period, unless driven by a confirmed demand change or an approved constraint relaxation. |
| BR‑S‑040 | Coverage assessment shall compare Days of Supply against the thresholds governed by PO‑S‑032. |
| BR‑S‑041 | Risk assessment shall consider demand variability, lead time, current coverage, and projected inventory from the Supply Plan. |
| BR‑S‑042 | Obsolescence classification shall use the criteria governed by PO‑S‑032. |
| BR‑S‑047 | If demand history or lead time data is insufficient for statistical calculation, the product‑location retains its current policy and is flagged for manual review. |
| BR‑S‑049 | Health classification for periodic assessments shall use the thresholds governed by PO‑S‑034. |
| BR‑S‑060 | Utilization status classification shall use the thresholds governed by PO‑S‑050. |
| BR‑S‑061 | Constraint classification shall identify resources where projected load exceeds Available Capacity per time bucket, and classify them as binding or non‑binding based on bottleneck criteria. |
| BR‑S‑062 | Bottleneck assessment shall determine whether a constraint limits overall system throughput, using the criteria governed by PO‑S‑050. |
| BR‑S‑063 | Flexibility assessment shall evaluate alternate resources, overtime feasibility, subcontracting options, extra shift feasibility, cross‑trained labor, and resource substitutability according to PO‑S‑050. |
| BR‑S‑064 | Risk assessment shall consider utilization level, single points of failure, maintenance dependencies, labor availability, and resource criticality, as defined by PO‑S‑050. |
| BR‑S‑080 | Supplier responsiveness shall be evaluated against the thresholds governed by PO‑S‑060. |
| BR‑S‑081 | Historical commitment reliability shall be assessed using the criteria governed by PO‑S‑060. |
| BR‑S‑082 | Commitment confidence shall be classified as Firm, Conditional, Tentative, or Estimated based on supplier responsiveness, reliability, and current risk signals, as defined by PO‑S‑060. |
| BR‑S‑083 | The criticality of the supply requirement shall influence the acceptance decision, as governed by PO‑S‑060. |
| BR‑S‑100 | Supplier eligibility shall be determined by approved supplier lists, contractual status, and sourcing rules governed by PO‑S‑070. |
| BR‑S‑101 | Lead time feasibility shall consider supplier lead time, transit time, calendar constraints, and the required delivery date. |
| BR‑S‑102 | Supplier capacity availability shall be assessed using the current Capacity Position Assessment for the supplier’s constrained resources. |
| BR‑S‑103 | Commitment confidence shall be assessed using the current Supplier Commitment and Supplier Commitment Assessment, as governed by PO‑S‑070. |
| BR‑S‑104 | Sourcing decisions shall balance cost, reliability, risk, and policy‑governed allocation preferences. |
| BR‑S‑105 | Procurement feasibility shall be assessed before a sourcing decision is made, identifying infeasibility causes as defined by PO‑S‑070. |
| BR‑S‑120 | Sequencing shall use the strategy defined by PO‑S‑080, balancing changeover minimization, due date adherence, and priority. |
| BR‑S‑121 | Changeover times between consecutive activities shall be respected as hard constraints per the changeover matrix in PO‑S‑080. |
| BR‑S‑122 | Due date adherence shall be prioritized for Critical and High‑priority items as defined by PO‑S‑080. |
| BR‑S‑123 | Priority‑based sequencing shall schedule Critical items first, then High, then Medium, then Low, within the optimization window. |
| BR‑S‑124 | Material availability shall be confirmed for each production activity before the schedule is marked Feasible. |
| BR‑S‑125 | Capacity availability shall be verified against the finite capacity of the assigned resource, including already‑scheduled activities. |
| BR‑S‑126 | Tool availability shall be confirmed where tooling constraints are defined in the routing. |
| BR‑S‑127 | Labor availability shall be confirmed where labor constraints are defined for the resource. |
| BR‑S‑128 | Maintenance windows and calendar closures shall be respected as hard constraints. |
| BR‑S‑140 | Source inventory availability shall be confirmed using projected available inventory at departure time. |
| BR‑S‑141 | Lane capacity shall be verified against the finite capacity of the lane for the required transit period. |
| BR‑S‑142 | Transit time feasibility shall consider the required delivery date, transit lead time, and receiving window. |
| BR‑S‑143 | Destination receiving capacity shall consider dock capacity, storage availability, and handling capability. |
| BR‑S‑144 | Movement decisions shall balance deployment priority (service protection, promotion support, new product launch, seasonal positioning, inventory balancing) against cost. |
| BR‑S‑145 | When available deployable inventory is insufficient, the Allocation Strategy governed by PO‑S‑090 shall be applied, and the allocation outcome (who received less, who was deferred) recorded. |
| BR‑S‑170 | A change shall be assessed when the deviation from the previously understood state exceeds the threshold governed by PO‑S‑100. |
| BR‑S‑171 | A change shall require corroboration from at least one independent source before being classified as Corroborated or Confirmed, as governed by PO‑S‑100. |
| BR‑S‑172 | Materiality assessment shall be scoped to planning relevance—whether the change meaningfully affects planning parameters—not business severity. |
| BR‑S‑173 | Assessment confidence shall be determined using the rules governed by PO‑S‑100, based on evidence strength, source reliability, and corroboration count. |

### Derivation Rules

| ID | Rule |
|----|------|
| BR‑S‑046 | Inventory policy parameters shall be calculated using the methodology governed by PO‑S‑033. |

---

# Chapter 8 — Policy Model

| ID | Policy | Category | Governance Outcome | Governed Rules |
|----|--------|----------|-------------------|----------------|
| PO‑S‑010 | If all supply data eligibility rules pass and source reliability meets threshold, accept automatically. Otherwise, route to Supply Data Steward. | Automation | Accept or quarantine. | BR‑S‑010–013 |
| PO‑S‑011 | Supply publication materiality, cadence, and data completeness thresholds are governed separately for inventory, commitments, capacity, and orders. | Compliance | Publish or hold. | BR‑S‑014, BR‑S‑015 |
| PO‑S‑012 | Supplier commitment reliability assessment is governed by approved evidence types, confidence thresholds, and review cadence. | Compliance | Reliability score accepted or flagged for review. | BR‑S‑012 |
| PO‑S‑021 | If the plan is infeasible, the Supply Planning Manager is notified and a constraint relaxation review is triggered. Constraint relaxations must be approved before the plan can be accepted. | Exception | Escalate; approve relaxation. | BR‑S‑029 |
| PO‑S‑022 | If all quality thresholds are met and plan stability is within limits, the plan is accepted automatically. Otherwise, Supply Planner approval is required. | Automation | Auto‑accept or escalate. | BR‑S‑025, BR‑S‑026 |
| PO‑S‑023 | The supply plan shall be published on the defined cadence for the operational and tactical horizons after acceptance. | Compliance | Scheduled publication. | BR‑S‑027, BR‑S‑028 |
| PO‑S‑032 | Coverage thresholds, risk assessment criteria, and obsolescence rules are governed by the Inventory Health Policy. | Compliance | Governed. | BR‑S‑040–042 |
| PO‑S‑033 | Policy calculation methodology and service level targets are governed by the Inventory Policy Governance. Planner overrides require justification and are reviewed periodically. | Compliance | Governed; overrides tracked. | BR‑S‑046, BR‑S‑047 |
| PO‑S‑034 | Health classification thresholds and assessment frequency are governed by the Inventory Health Policy. | Compliance | Governed. | BR‑S‑049 |
| PO‑S‑050 | Utilization targets, overtime rules, subcontracting rules, flexibility constraints, risk assessment criteria, stability/volatility thresholds, investment and improvement signal criteria, and assessment frequency are governed by the Capacity Management Policy. | Compliance | Governed. | BR‑S‑060–064, BR‑S‑068–070 |
| PO‑S‑060 | Commitment state transitions, confidence thresholds, responsiveness thresholds, reliability criteria, risk signal definitions, health classification thresholds, recommendation signal criteria, and assessment frequency are governed by the Supplier Collaboration Policy. | Compliance | Governed. | BR‑S‑080–090 |
| PO‑S‑070 | Sourcing rules, supplier allocation preferences, confidence thresholds, feasibility criteria, risk signal thresholds, publication cadence, and review requirements are governed by the Procurement Policy. | Compliance | Governed. | BR‑S‑100–105, BR‑S‑111–113 |
| PO‑S‑080 | Sequencing rules, changeover matrices, overtime limits, feasibility criteria, stability thresholds, confidence thresholds, and publication cadence are governed by the Scheduling Policy. | Compliance | Governed. | BR‑S‑120–128, BR‑S‑133–135 |
| PO‑S‑090 | Sourcing rules, lane preferences, allocation rules, feasibility criteria, stability thresholds, confidence thresholds, and publication cadence are governed by the Distribution Policy. | Compliance | Governed. | BR‑S‑140–145, BR‑S‑156–158 |
| PO‑S‑100 | Change detection thresholds, corroboration requirements, materiality criteria, confidence assessment rules, signal normalisation rules, and lifecycle management are governed by the Supply Change Policy. | Compliance | Governed. | BR‑S‑170–173, BR‑S‑177 |

---

# Chapter 9 — Functional Specifications

## Understand Supply Functional Specifications

### FS‑S‑001 — Receive Supply Data

**Realises:** CR‑S‑001  
**Business Contract:**
- **Consumes:** Supply transaction from source systems.
- **Produces:** SE‑S‑010 Supply Data Record (Lifecycle State: Received).
- **Transitions:** SE‑S‑010: (none) → Received.
- **Publishes:** None (evaluation follows immediately).
- **Invokes:** FS‑S‑002.
- **Guarantees:** Exactly one Supply Data Record established with full provenance. Duplicate transactions rejected.

**Trigger:** Supply data received from source systems.

**Preconditions:** Record contains sufficient information for unique identity. Referenced Product and Location exist. Source system is known.

**Semantic Objects:** Read: SE‑C‑040 (Product), SE‑C‑041 (Location). Create: SE‑S‑010.

**Behaviour:**
1. Invoke AB‑S‑001 Receive Supply Data.
2. Immediately invoke FS‑S‑002.

**Business Transaction:** Per AB‑S‑001 contract. Protects Supply Data Record aggregate.

**Postconditions:** SE‑S‑010 exists in Received state. FS‑S‑002 invoked.

**Failure Behaviour:** Record not established. Source may resubmit. (Temporary, retryable.)

**Recovery:** Re‑execution is idempotent.

**Concurrency:** Records with different identities processed independently.

**Traceability:** Realises CR‑S‑001. Invokes AB‑S‑001. Invokes FS‑S‑002.

### FS‑S‑002 — Evaluate Supply Data

**Realises:** CR‑S‑002  
**Business Contract:**
- **Consumes:** SE‑S‑010 in Lifecycle State Received.
- **Produces:** SE‑S‑010 with updated Lifecycle State and decision traceability.
- **Transitions:** Received → Accepted / Quarantined / Rejected.
- **Publishes:** BN‑S‑002 (Quarantined), BN‑S‑003 (Rejected).
- **Invokes:** FS‑S‑003 (if Accepted).
- **Guarantees:** Record evaluated exactly once. Decision traceability recorded.

**Trigger:** Completion of FS‑S‑001.

**Preconditions:** Record in Received state. Record not previously evaluated.

**Semantic Objects:** Read: SE‑S‑010. Update: SE‑S‑010.

**Behaviour:**
1. Invoke AB‑S‑002 Evaluate Supply Data.
2. Publish appropriate notification based on outcome.

**Business Transaction:** Per AB‑S‑002 contract.

**Postconditions:** Record in final evaluation state. If Accepted, eligible for FS‑S‑003.

**Failure Behaviour:** Record remains Received. No notification published. (Permanent for business failure, temporary for operational.)

**Recovery:** Re‑execution permitted while Lifecycle is Received.

**Concurrency:** Record evaluated exactly once.

**Traceability:** Realises CR‑S‑002. Invokes AB‑S‑002. Publishes BN‑S‑002, BN‑S‑003. Invokes FS‑S‑003.

### FS‑S‑003 — Revise Enterprise Supply Picture

**Realises:** CR‑S‑003  
**Business Contract:**
- **Consumes:** Accepted Supply Data Records, Reconciliation Adjustments.
- **Produces:** SE‑S‑001 Enterprise Supply Picture (new Draft version).
- **Transitions:** SE‑S‑001: (none) → Draft, or Published → Draft (previous remains Published until superseded).
- **Publishes:** None.
- **Invokes:** FS‑S‑004.
- **Guarantees:** One current Draft exists for Planning Scope. Previous Published version remains authoritative until publication.

**Trigger:** Accepted supply data available or reconciliation adjustments produced.

**Preconditions:** Accepted records or adjustments exist for the Planning Scope.

**Semantic Objects:** Read: SE‑S‑010 (Accepted), Reconciliation Adjustments. Create: SE‑S‑001 (new version).

**Behaviour:**
1. Invoke AB‑S‑003 Revise Enterprise Supply Picture.

**Business Transaction:** Per AB‑S‑003 contract.

**Postconditions:** Draft version exists. Eligible for FS‑S‑004.

**Failure Behaviour:** Draft not created. Previous version remains authoritative. Retryable.

**Recovery:** Re‑execution creates one revised version.

**Concurrency:** Updates to same Planning Scope serialized.

**Traceability:** Realises CR‑S‑003. Invokes AB‑S‑003. Invokes FS‑S‑004.

### FS‑S‑004 — Publish Enterprise Supply Picture

**Realises:** CR‑S‑004  
**Business Contract:**
- **Consumes:** SE‑S‑001 in Draft.
- **Produces:** SE‑S‑001 Published (authoritative). Previous version → Superseded.
- **Transitions:** Draft → Published. Previous → Superseded.
- **Publishes:** BN‑S‑001 Enterprise Supply Picture Published.
- **Invokes:** None (terminal).
- **Guarantees:** Exactly one Published version per Planning Scope. Responsibility transfers to consumers.

**Trigger:** Material change detected per Supply Publication Policy.

**Preconditions:** Draft exists. Materiality threshold met. Data completeness meets threshold.

**Semantic Objects:** Read: SE‑S‑001 (Draft). Update: SE‑S‑001 (current and previous).

**Behaviour:**
1. Invoke AB‑S‑005 Publish Enterprise Supply Picture.
   - Execute DE‑S‑011 Publish Enterprise Supply Picture.
   - If Publish: transition Draft to Published, supersede previous version, publish BN‑S‑001.
   - If Do Not Publish: Draft retained.

**Business Transaction:** Per AB‑S‑005 contract.

**Postconditions:** Exactly one Published version. BN‑S‑001 published.

**Failure Behaviour:** Draft retained. BN‑S‑001 not published. Retryable.

**Recovery:** Re‑execution publishes same version.

**Concurrency:** Publication for given version occurs exactly once.

**Traceability:** Realises CR‑S‑004. Invokes AB‑S‑005. Publishes BN‑S‑001.

## Plan Supply Functional Specifications

### FS‑S‑010 — Balance Supply and Demand

**Realises:** CR‑S‑005  
**Business Contract:**
- **Consumes:** Demand forecast, Enterprise Supply Picture, capacity data, BOMs, planning parameters.
- **Produces:** SE‑S‑020 Supply Plan (Draft).
- **Transitions:** SE‑S‑020: (none) → Draft.
- **Publishes:** None.
- **Invokes:** FS‑S‑011.
- **Guarantees:** All mandatory product‑location combinations covered. All hard constraints satisfied or documented as infeasibilities requiring relaxation approval.

**Trigger:** Scheduled (weekly operational, monthly tactical) or event‑driven (demand update, major disruption).

**Preconditions:** Demand plan and supply picture available. Capacity and BOM data current.

**Semantic Objects:** Read: Demand forecast, SE‑S‑001, capacity data, BOMs. Create: SE‑S‑020.

**Behaviour:**
1. Invoke AB‑S‑010 Generate Supply Plan.
   - Balance supply and demand: match demand with available supply, respecting constraints.
   - Populate planned supply quantities, projected inventory, projected backorders, constraint status, and supply‑demand balance.
   - Compute plan confidence score.

**Business Transaction:** Per AB‑S‑010 contract.

**Postconditions:** Draft Supply Plan exists. Eligible for FS‑S‑011.

**Failure Behaviour:** Plan not created. Retryable.

**Recovery:** Re‑execution is deterministic.

**Concurrency:** Generation for a given Planning Scope serialized.

**Traceability:** Realises CR‑S‑005. Invokes AB‑S‑010. Invokes FS‑S‑011.

### FS‑S‑011 — Evaluate Supply Plan

**Realises:** CR‑S‑006  
**Business Contract:**
- **Consumes:** SE‑S‑020 Supply Plan (Draft).
- **Produces:** SE‑S‑020 with acceptance decision recorded.
- **Transitions:** SE‑S‑020: Draft (evaluated).
- **Publishes:** None.
- **Invokes:** FS‑S‑012 (if accepted).
- **Guarantees:** Plan evaluated against all quality thresholds.

**Trigger:** Completion of FS‑S‑010.

**Preconditions:** Draft Supply Plan exists.

**Semantic Objects:** Read: SE‑S‑020. Update: SE‑S‑020.

**Behaviour:**
1. Invoke AB‑S‑011 Evaluate Supply Plan.
   - Execute DE‑S‑020 Accept Supply Plan.
   - If Reject: re‑plan or escalate per PO‑S‑021.

**Business Transaction:** Per AB‑S‑011 contract.

**Postconditions:** Plan evaluated. If accepted, eligible for FS‑S‑012.

**Traceability:** Realises CR‑S‑006. Invokes AB‑S‑011. Invokes FS‑S‑012.

### FS‑S‑012 — Publish Supply Plan

**Realises:** CR‑S‑007  
**Business Contract:**
- **Consumes:** SE‑S‑020 Supply Plan (accepted).
- **Produces:** SE‑S‑020 Published (authoritative). Previous → Superseded.
- **Transitions:** Draft → Published. Previous → Superseded.
- **Publishes:** BN‑S‑010 Supply Plan Published.
- **Invokes:** None (terminal).
- **Guarantees:** Exactly one Published Supply Plan for the Planning Scope. Responsibility transfers to consumers.

**Trigger:** Completion of FS‑S‑011 with accepted plan.

**Preconditions:** Plan accepted by DE‑S‑020.

**Semantic Objects:** Read: SE‑S‑020. Update: SE‑S‑020 (current and previous).

**Behaviour:**
1. Invoke AB‑S‑012 Publish Supply Plan.
   - Execute DE‑S‑021 Publish Supply Plan.
   - If Publish: transition to Published, supersede previous, publish BN‑S‑010.

**Business Transaction:** Per AB‑S‑012 contract.

**Postconditions:** Exactly one Published Supply Plan. BN‑S‑010 published.

**Traceability:** Realises CR‑S‑007. Invokes AB‑S‑012. Publishes BN‑S‑010.

## Manage Inventory Functional Specifications

### FS‑S‑020 — Maintain Inventory Position Understanding

**Realises:** CR‑S‑008  
**Business Contract:**
- **Consumes:** Enterprise Supply Picture (SE‑S‑001), Supply Plan (SE‑S‑020 — projected inventory), Segmentation Data, Inventory Health Policy.
- **Produces:** Updated Inventory Position Assessment (SE‑S‑030).
- **Transitions:** SE‑S‑030: Health status, risk levels may change; Assessment Change Event appended.
- **Publishes:** BN‑S‑020 (if health status or risk level changed).
- **Guarantees:** Every active product‑location has a current interpreted understanding of its inventory position.

**Trigger:** Enterprise Supply Picture published, Supply Plan published, or scheduled re‑evaluation.

**Preconditions:** Latest Enterprise Supply Picture and Supply Plan available.

**Behaviour:**
1. For each active product‑location:
   - Invoke AB‑S‑020 Update Inventory Position Assessment.
   - If health status or risk level changed, publish BN‑S‑020.

**Business Transaction:** Per AB‑S‑020 contract.

**Traceability:** Realises CR‑S‑008. Invokes AB‑S‑020. Publishes BN‑S‑020.

### FS‑S‑021 — Maintain Inventory Policies

**Realises:** CR‑S‑009  
**Business Contract:**
- **Consumes:** Demand forecast, Supply Plan, Enterprise Supply Picture, Inventory Policy Governance.
- **Produces:** Updated Inventory Policy Assignment (SE‑S‑031).
- **Transitions:** SE‑S‑031: Policy parameters may change; Policy Change Event appended.
- **Publishes:** BN‑S‑021 (if policy changed).
- **Guarantees:** Every product‑location has a current, governed inventory policy.

**Trigger:** Scheduled (daily/weekly) or event‑driven (Supply Plan published, demand forecast updated).

**Preconditions:** Demand and supply data available. Inventory Policy Governance current.

**Behaviour:**
1. For each active product‑location:
   - Invoke AB‑S‑021 Update Inventory Policy.
   - If policy changed, publish BN‑S‑021.

**Business Transaction:** Per AB‑S‑021 contract.

**Traceability:** Realises CR‑S‑009. Invokes AB‑S‑021. Publishes BN‑S‑021.

### FS‑S‑022 — Assess Inventory Health

**Realises:** CR‑S‑010  
**Business Contract:**
- **Consumes:** Inventory Position Assessments, Inventory Policy Assignments, Inventory Health Policy.
- **Produces:** Inventory Health Assessment (SE‑S‑032) — Published.
- **Publishes:** BN‑S‑022.
- **Guarantees:** Complete health classification, risk summary, and financial summary for the scope and period.

**Trigger:** Scheduled (weekly/monthly) per Inventory Health Policy.

**Behaviour:**
1. Invoke AB‑S‑022 Publish Inventory Health Assessment.
2. Publish BN‑S‑022.

**Business Transaction:** Per AB‑S‑022 contract.

**Traceability:** Realises CR‑S‑010. Invokes AB‑S‑022. Publishes BN‑S‑022.

## Manage Capacity Functional Specifications

### FS‑S‑030 — Maintain Capacity Position Understanding

**Realises:** CR‑S‑011  
**Business Contract:**
- **Consumes:** Enterprise Supply Picture (SE‑S‑001), Supply Plan (SE‑S‑020 — planned production quantities), Resource Calendars, Capacity Management Policy.
- **Produces:** Updated Capacity Position Assessment (SE‑S‑040).
- **Transitions:** SE‑S‑040: Utilization status, constraint classification, bottleneck assessment, flexibility interpretation, risk levels, confidence may change; Assessment Change Event appended with reason and evidence.
- **Publishes:** BN‑S‑030 (if any significant change occurred).
- **Guarantees:** Every active resource has a current interpreted understanding of its capacity position with full explainability.

**Trigger:** Enterprise Supply Picture published, Supply Plan published, resource calendar updated, or scheduled re‑evaluation.

**Preconditions:** Latest Enterprise Supply Picture, Supply Plan, and resource calendars available.

**Behaviour:**
1. For each active resource:
   - Invoke AB‑S‑030 Update Capacity Position Assessment.
   - If any significant change (utilization status, constraint, bottleneck, flexibility, risk) occurred, publish BN‑S‑030.

**Business Transaction:** Per AB‑S‑030 contract.

**Traceability:** Realises CR‑S‑011. Invokes AB‑S‑030. Publishes BN‑S‑030.

### FS‑S‑031 — Assess Capacity Health

**Realises:** CR‑S‑012  
**Business Contract:**
- **Consumes:** Capacity Position Assessments, Capacity Management Policy.
- **Produces:** Capacity Health Assessment (SE‑S‑041) — Published.
- **Publishes:** BN‑S‑031.
- **Guarantees:** Complete utilization summary, stability and volatility metrics, constraint analysis, bottleneck identification, risk summary, and investment/improvement signals for the scope and period.

**Trigger:** Scheduled (weekly/monthly) per Capacity Management Policy.

**Behaviour:**
1. Invoke AB‑S‑031 Publish Capacity Health Assessment.
2. Publish BN‑S‑031.

**Business Transaction:** Per AB‑S‑031 contract.

**Traceability:** Realises CR‑S‑012. Invokes AB‑S‑031. Publishes BN‑S‑031.

## Collaborate with Suppliers Functional Specifications

### FS‑S‑050 — Maintain Supplier Commitments

**Realises:** CR‑S‑013  
**Business Contract:**
- **Consumes:** Supply Plan (SE‑S‑020), Inventory Position Assessment (SE‑S‑030), Capacity Position Assessment (SE‑S‑040), Supplier Collaboration Policy (PO‑S‑060).
- **Produces:** Supplier Commitment (SE‑S‑050) — created or updated.
- **Transitions:** SE‑S‑050: Commitment state may change; Collaboration Event appended.
- **Publishes:** BN‑S‑040 (if commitment state or confidence changed).
- **Guarantees:** Every supply requirement is communicated and tracked as a Supplier Commitment with full bidirectional collaboration history.

**Trigger:** Supply Plan published (new or changed requirements), Inventory Position Assessment changed (shortages requiring action), supplier response received.

**Behaviour:**
1. Identify supply requirements from the Supply Plan, Inventory Position Assessment, and Capacity Position Assessment that require supplier collaboration.
2. For each requirement, propose a Supplier Commitment to the supplier (Proposed state).
3. Upon receiving the supplier’s response, invoke AB‑S‑040 Record Supplier Commitment.
   - Execute DE‑S‑050 Accept Supplier Response to determine acceptance outcome and confidence.
   - Record the response and enterprise decision as Collaboration Events.
4. If commitment state or confidence changed, publish BN‑S‑040.

**Business Transaction:** Per AB‑S‑040 contract.

**Traceability:** Realises CR‑S‑013. Invokes AB‑S‑040. Publishes BN‑S‑040.

### FS‑S‑051 — Assess Supplier Collaboration Health

**Realises:** CR‑S‑014  
**Business Contract:**
- **Consumes:** Supplier Commitment Assessments (SE‑S‑051), Supplier Collaboration Policy (PO‑S‑060).
- **Produces:** Supplier Collaboration Health Assessment (SE‑S‑052) — Published.
- **Publishes:** BN‑S‑041.
- **Guarantees:** Complete health classification, responsiveness trends, commitment accuracy trends, and recommendation signals for the scope and period.

**Trigger:** Scheduled (monthly/quarterly) per Supplier Collaboration Policy.

**Behaviour:**
1. Invoke AB‑S‑042 Publish Supplier Collaboration Health Assessment.
2. Publish BN‑S‑041.

**Business Transaction:** Per AB‑S‑042 contract.

**Traceability:** Realises CR‑S‑014. Invokes AB‑S‑042. Publishes BN‑S‑041.

---


## Procure Materials Functional Specifications

### FS‑S‑060 — Create Procurement Recommendations

**Realises:** CR‑S‑015  
**Business Contract:**
- **Consumes:** Supply Plan (SE‑S‑020), Inventory Position Assessment (SE‑S‑030), Capacity Position Assessment (SE‑S‑040), Supplier Commitment (SE‑S‑050), Supplier Commitment Assessment (SE‑S‑051), Supplier Collaboration Health Assessment (SE‑S‑052), Supplier Master Data, Procurement Policy (PO‑S‑070).
- **Produces:** Procurement Recommendation (SE‑S‑060) — Active.
- **Transitions:** SE‑S‑060: (none) → Active, or Active → Active (previous version → Superseded if materially revised).
- **Publishes:** BN‑S‑050 (on creation or material revision).
- **Guarantees:** Every procurement requirement has a current, governed sourcing decision with full rationale, evidence, confidence, and feasibility assessment.

**Trigger:** Supply Plan published (new or changed requirements), Supplier Commitment changed, or scheduled re‑evaluation.

**Behaviour:**
1. For each procurement requirement in the current Supply Plan:
   - Invoke AB‑S‑050 Create Procurement Recommendation.
   - Execute DE‑S‑060 Determine Sourcing Decision.
   - Assess procurement feasibility (BA‑S‑030).
   - Assess multi‑dimensional confidence (BA‑S‑031).
   - Record decision rationale, evidence, and assumptions.
   - If materially different from the previous Active version, supersede the previous version.
   - Publish BN‑S‑050.

**Business Transaction:** Per AB‑S‑050 contract.

**Traceability:** Realises CR‑S‑015. Invokes AB‑S‑050. Publishes BN‑S‑050.

### FS‑S‑061 — Publish Procurement Plan

**Realises:** CR‑S‑016  
**Business Contract:**
- **Consumes:** Active Procurement Recommendations (SE‑S‑060), Procurement Policy (PO‑S‑070).
- **Produces:** Procurement Plan (SE‑S‑061) — Published.
- **Publishes:** BN‑S‑051.
- **Guarantees:** Complete procurement baseline for the planning cycle, with aggregate metrics, risk signals, and recommendation signals.

**Trigger:** Scheduled per Procurement Policy (weekly/monthly aligned with planning cycle).

**Behaviour:**
1. Invoke AB‑S‑051 Publish Procurement Plan.
   - Compose Active Procurement Recommendations into the plan snapshot.
   - Compute aggregate metrics, risk signals, and recommendation signals.
   - Execute DE‑S‑061 Publish Procurement Plan.
   - Publish BN‑S‑051.

**Business Transaction:** Per AB‑S‑051 contract.

**Traceability:** Realises CR‑S‑016. Invokes AB‑S‑051. Publishes BN‑S‑051.

---

## Schedule Production Functional Specifications

### FS‑S‑070 — Create Production Schedules

**Realises:** CR‑S‑017  
**Business Contract:**
- **Consumes:** Supply Plan (SE‑S‑020), Inventory Position Assessment (SE‑S‑030), Capacity Position Assessment (SE‑S‑040), Supplier Commitment (SE‑S‑050), Resource Master Data, Routings and BOMs, Scheduling Policy (PO‑S‑080).
- **Produces:** Production Schedule (SE‑S‑070) — Active.
- **Transitions:** SE‑S‑070: (none) → Active, or Active → Active (previous version → Superseded if materially revised).
- **Publishes:** BN‑S‑060 (on creation or material revision).
- **Guarantees:** Every constrained resource has a current, feasible, sequenced Production Schedule with full rationale, evidence, confidence, and stability.

**Trigger:** Supply Plan published, Capacity Position Assessment changed, or scheduled re‑evaluation.

**Behaviour:**
1. For each constrained resource and planning period:
   - Invoke AB‑S‑060 Create Production Schedule.
   - Execute DE‑S‑070 Determine Production Sequence.
   - Execute DE‑S‑071 Assess Schedule Feasibility.
   - Compute schedule confidence (BA‑S‑041).
   - Compute schedule stability (BA‑S‑042).
   - Record decision rationale and evidence.
   - If materially different from the previous Active version, supersede the previous version.
   - Publish BN‑S‑060.

**Business Transaction:** Per AB‑S‑060 contract.

**Traceability:** Realises CR‑S‑017. Invokes AB‑S‑060. Publishes BN‑S‑060.

### FS‑S‑071 — Publish Production Schedule Baseline

**Realises:** CR‑S‑018  
**Business Contract:**
- **Consumes:** Active Production Schedules (SE‑S‑070), Scheduling Policy (PO‑S‑080).
- **Produces:** Published Production Schedule (SE‑S‑071) — Published.
- **Publishes:** BN‑S‑061.
- **Guarantees:** Complete scheduling baseline for the planning period, with aggregate metrics, feasibility summary, stability summary, and risk signals.

**Trigger:** Scheduled per Scheduling Policy (aligned with planning cycle).

**Behaviour:**
1. Invoke AB‑S‑061 Publish Production Schedule.
   - Compose Active Production Schedules into the baseline snapshot.
   - Compute aggregate metrics, feasibility summary, stability summary, and risk signals.
   - Execute DE‑S‑072 Publish Production Schedule.
   - Publish BN‑S‑061.

**Business Transaction:** Per AB‑S‑061 contract.

**Traceability:** Realises CR‑S‑018. Invokes AB‑S‑061. Publishes BN‑S‑061.

## Manage Distribution Functional Specifications

### FS‑S‑080 — Create Distribution Recommendations

**Realises:** CR‑S‑019  
**Business Contract:**
- **Consumes:** Supply Plan (SE‑S‑020), Inventory Position Assessment (SE‑S‑030), Procurement Plan (SE‑S‑061), Production Schedule (SE‑S‑071), Supplier Commitment (SE‑S‑050), Network/Lane Master Data, Distribution Policy (PO‑S‑090).
- **Produces:** Distribution Recommendation (SE‑S‑080) — Active.
- **Transitions:** SE‑S‑080: (none) → Active, or Active → Active (previous version → Superseded if materially revised).
- **Publishes:** BN‑S‑070 (on creation or material revision).
- **Guarantees:** Every distribution requirement has a current, governed network positioning decision with full rationale, strategy, evidence, confidence, and—when supply is constrained—allocation strategy and outcome.

**Trigger:** Supply Plan published, Inventory Position Assessment changed, or scheduled re‑evaluation.

**Behaviour:**
1. For each distribution requirement:
   - Invoke AB‑S‑070 Create Distribution Recommendation.
   - Execute DE‑S‑080 Determine Distribution Sourcing.
   - Assess distribution feasibility (BA‑S‑050).
   - Assess multi‑dimensional confidence (BA‑S‑051).
   - If supply is constrained, apply Allocation Strategy and record allocation outcome.
   - Record decision rationale (including why inventory was retained, if applicable), evidence, and strategy.
   - If materially different from the previous Active version, supersede the previous version.
   - Publish BN‑S‑070.

**Business Transaction:** Per AB‑S‑070 contract.

**Traceability:** Realises CR‑S‑019. Invokes AB‑S‑070. Publishes BN‑S‑070.

### FS‑S‑081 — Publish Distribution Plan

**Realises:** CR‑S‑020  
**Business Contract:**
- **Consumes:** Active Distribution Recommendations (SE‑S‑080), Distribution Policy (PO‑S‑090).
- **Produces:** Distribution Plan (SE‑S‑081) — Published.
- **Publishes:** BN‑S‑071.
- **Guarantees:** Complete distribution baseline for the planning cycle, with aggregate metrics, risk signals, recovery opportunities, and stability summary.

**Trigger:** Scheduled per Distribution Policy (aligned with planning cycle).

**Behaviour:**
1. Invoke AB‑S‑071 Publish Distribution Plan.
   - Compose Active Distribution Recommendations into the plan snapshot.
   - Compute aggregate metrics, risk signals, recovery opportunities, and stability summary.
   - Execute DE‑S‑081 Publish Distribution Plan.
   - Publish BN‑S‑071.

**Business Transaction:** Per AB‑S‑071 contract.

**Traceability:** Realises CR‑S‑020. Invokes AB‑S‑071. Publishes BN‑S‑071.


## Sense Supply Changes Functional Specifications

### FS‑S‑080 — Assess Supply Changes

**Realises:** CR‑S‑021  
**Business Contract:**
- **Consumes:** Signals from Supplier Commitments, Enterprise Supply Picture, Capacity Position Assessments, Procurement Recommendations, Distribution Recommendations, and external sources; Supply Change Policy (PO‑S‑100).
- **Produces:** Updated Supply Change Assessment (SE‑S‑090) — current change state updated.
- **Transitions:** SE‑S‑090: Change state transitions per lifecycle (No Change → Observed → Corroborated → Confirmed → Resolved → Archived); Change History Event appended on each state transition.
- **Publishes:** BN‑S‑080 (on state transition).
- **Guarantees:** Every monitored supply entity has continuously current change awareness per change type. Heterogeneous signals are normalised into canonical change representation. All changes record evidence, confidence, materiality, temporal context, and correlations.

**Trigger:** Incoming signal from any monitored source, or scheduled re‑evaluation.

**Preconditions:** Monitored entity is registered for change detection. Supply Change Policy is current.

**Behaviour:**
1. For each incoming signal or scheduled evaluation:
   - Normalise the signal into the canonical change representation (BA‑S‑060).
   - Identify the relevant Supply Change Assessment (or create if first observation for this entity and change type).
   - Invoke AB‑S‑080 Assess Supply Change.
   - Execute DE‑S‑090 Assess Supply Change.
   - If corroboration is pending, attempt corroboration from independent sources (BA‑S‑061).
   - Update current change state, evidence, confidence, materiality, temporal context, and correlated changes.
   - If the change state transitioned (per the publication rules governed by PO‑S‑100), append a Change History Event and publish BN‑S‑080.

**Business Transaction:** Per AB‑S‑080 contract.

**Postconditions:** Current change state reflects the latest understanding based on all available signals.

**Failure Behaviour:** Assessment unchanged. Retryable.

**Recovery:** Re‑evaluation is idempotent.

**Concurrency:** Assessments for different entity‑change type combinations processed independently.

**Example:** Supplier S1, Change Type "Quantity Reduction": signal received indicating 15% reduction. State transitions from No Change → Observed. Second independent source confirms → Corroborated. Materiality assessed as Material to Planning. Confidence: High. BN‑S‑080 published. Correlated to Capacity Reduction on Resource R3 if linked.

**Traceability:** Realises CR‑S‑021. Invokes AB‑S‑080. Publishes BN‑S‑080.

# Chapter 10 — Business Algorithms

### BA‑S‑001 — Supply Optimization (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes (traceable to inputs and constraints) |

**Placeholder Signature:**  
Inputs: Demand forecast, Enterprise Supply Picture, capacity, BOMs, planning parameters.  
Outputs: Planned production, procurement, transfer quantities; projected inventory; constraint status.

---

### BA‑S‑002 — Compute Safety Stock (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Demand variability, lead time, service level target.  
Output: Safety stock quantity.

---

### BA‑S‑003 — Compute Reorder Point (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Average demand, lead time, safety stock.  
Output: Reorder point quantity.

---

### BA‑S‑004 — Evaluate Inventory Risk (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Current coverage, projected inventory, demand variability, lead time.  
Output: Stockout risk, excess risk, obsolescence risk.

---

### BA‑S‑005 — Classify Inventory Health (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Days of Supply, policy targets, risk assessments.  
Output: Health classification per product‑location.

---

### BA‑S‑010 — Evaluate Capacity Risk (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Current utilization, projected load, capacity flexibility, resource criticality, single point of failure indicators, maintenance dependencies, labor availability.  
Output: Overload risk, underload risk, bottleneck risk, single point of failure risk, maintenance risk, labor risk.

---

### BA‑S‑011 — Classify Capacity Health (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Utilization summaries, constraint classifications, stability metrics, volatility metrics, risk assessments.  
Output: Capacity health classification, binding constraints, stability rating, volatility rating, investment signals, improvement signals.

### BA‑S‑020 — Evaluate Commitment Confidence (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Supplier responsiveness metrics, historical commitment accuracy, current risk signals, criticality of requirement.  
Output: Commitment confidence classification (Firm, Conditional, Tentative, Estimated) or confidence score.

---

### BA‑S‑021 — Assess Supplier Collaboration Risk (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Open commitments summary, responsiveness metrics, reliability metrics, dependency exposure, concentration exposure.  
Output: Supply Continuity Risk, Commitment Risk, Dependency Risk, Concentration Risk.

---

### BA‑S‑022 — Classify Collaboration Health (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Responsiveness trends, commitment accuracy trends, risk assessments, recommendation signals.  
Output: Collaboration health classification (Excellent, Good, Needs Improvement, At Risk).

---

### BA‑S‑030 — Evaluate Procurement Feasibility (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Requirement (product, quantity, date, location), supplier lead times, approved supplier list, MOQs, order multiples, calendar constraints.  
Output: Feasibility classification (Feasible, Conditionally Feasible, Infeasible) with documented constraints.

### BA‑S‑031 — Assess Procurement Confidence (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Supplier commitment confidence, demand confidence, capacity confidence, inventory risk factor, sourcing complexity.  
Output: Multi‑dimensional confidence assessment (overall score plus component scores).

---

### BA‑S‑040 — Optimize Production Sequence (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | Yes (sequence order matters) |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Production activities (product, quantity, due date, priority), resource capacity, changeover matrix, scheduling strategy.  
Output: Optimized sequence with start/end times, changeover times, and total cost.

---

### BA‑S‑041 — Evaluate Schedule Confidence (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Material certainty, resource reliability, changeover accuracy, demand stability.  
Output: Multi‑dimensional confidence assessment (overall score plus component scores).


### BA‑S‑042 — Compute Schedule Stability (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Current schedule, previous schedule version.  
Output: Stability score, change count, change magnitude.

---

### BA‑S‑050 — Evaluate Distribution Feasibility (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Source inventory projections, lane capacities, transit times, receiving capacities, time windows.  
Output: Feasibility classification with per‑constraint assessment.


### BA‑S‑051 — Assess Distribution Confidence (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Inventory certainty, lane reliability, transit time accuracy, receiving capacity confidence, demand volatility, disruption exposure.  
Output: Multi‑dimensional confidence assessment.

### BA‑S‑060 — Normalise Supply Signals (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Heterogeneous signals from supplier commitments, enterprise supply picture, capacity assessments, procurement recommendations, distribution recommendations, external sources.  
Output: Normalised canonical change representation (entity type, entity identifier, change type, deviation magnitude, direction).

### BA‑S‑061 — Corroborate Supply Change (Draft)

**Version:** 0.1 (Draft)  
**Algebraic Properties:**

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Yes |

**Placeholder Signature:**  
Inputs: Observed change, independent signal sources.  
Output: Corroboration status, updated confidence.


# Appendix A — Integration Matrix

| Publisher | Notification | Consumer | Behaviour |
|-----------|-------------|----------|-----------|
| Understand Supply | BN‑S‑001 Enterprise Supply Picture Published | Plan Supply | Consume as planning baseline. |
| Understand Supply | BN‑S‑001 | Manage Inventory | Update inventory context. |
| Understand Supply | BN‑S‑001 | Manage Capacity | Update capacity context. |
| Understand Supply | BN‑S‑001 | Promise Intelligence | Consume for ATP/CTP. |
| Understand Supply | BN‑S‑001 | Scenario Intelligence | Consume for scenario baseline. |
| Demand Intelligence | BN‑D‑001 Enterprise Demand Picture Published | Understand Supply | Demand context for supply adequacy. |
| Plan Supply | BN‑S‑010 Supply Plan Published | Manage Inventory | Consume projected inventory for replenishment. |
| Plan Supply | BN‑S‑010 | Manage Capacity | Update capacity load profiles. |
| Plan Supply | BN‑S‑010 | Procure Materials | Generate procurement recommendations. |
| Plan Supply | BN‑S‑010 | Schedule Production | Generate production schedule. |
| Plan Supply | BN‑S‑010 | Manage Distribution | Generate transfer plan. |
| Plan Supply | BN‑S‑010 | Promise Intelligence | Consume for ATP/CTP. |
| Plan Supply | BN‑S‑010 | Scenario Intelligence | Consume for scenario baseline. |
| Manage Inventory | BN‑S‑020 Inventory Position Assessment Changed | Procure Materials | Prioritize replenishment actions based on risk signals. |
| Manage Inventory | BN‑S‑020 | Planners | Operational dashboards. |
| Manage Inventory | BN‑S‑021 Inventory Policy Updated | Procure Materials | Trigger replenishment evaluation. |
| Manage Inventory | BN‑S‑022 Inventory Health Assessment Published | Procure Materials | Use over‑stocked signals for deferral. |
| Manage Inventory | BN‑S‑022 | Evaluate Supply Quality | Track health trends. |
| Manage Inventory | BN‑S‑022 | Learn From Supply | Identify policy improvement opportunities. |
| Manage Capacity | BN‑S‑030 Capacity Position Assessment Changed | Plan Supply | Consume constraint feedback, bottleneck information, and flexibility for next planning run. |
| Manage Capacity | BN‑S‑030 | Schedule Production | Use resource availability, flexibility, and constraint horizon for scheduling. |
| Manage Capacity | BN‑S‑031 Capacity Health Assessment Published | Evaluate Supply Quality | Track capacity effectiveness trends, stability, and volatility. |
| Manage Capacity | BN‑S‑031 | Learn From Supply | Constraint pattern learning, recurring bottleneck identification. |
| Collaborate with Suppliers | BN‑S‑040 Supplier Commitment Changed | Plan Supply | Use firm commitments and confidence to constrain next planning run. |
| Collaborate with Suppliers | BN‑S‑040 | Procure Materials | Use confirmed commitments to generate purchase orders. |
| Collaborate with Suppliers | BN‑S‑041 Supplier Collaboration Health Assessment Published | Evaluate Supply Quality | Track collaboration quality trends. |
| Collaborate with Suppliers | BN‑S‑041 | Learn From Supply | Identify collaboration improvement opportunities. |
| Procure Materials | BN‑S‑050 Procurement Recommendation Created | Procurement Execution (external) | Generate purchase orders from recommendations. |
| Procure Materials | BN‑S‑050 | Explain Supply Decisions | Use sourcing rationale and evidence for explanations. |
| Procure Materials | BN‑S‑051 Procurement Plan Published | Procurement Execution (external) | Consume as authoritative procurement baseline. |
| Procure Materials | BN‑S‑051 | Evaluate Supply Quality | Track plan adherence and supplier allocation accuracy. |
| Procure Materials | BN‑S‑051 | Learn From Supply | Identify sourcing pattern improvements. |
| Schedule Production | BN‑S‑060 Production Schedule Created | Production Execution (external) | Dispatch jobs to shop floor. |
| Schedule Production | BN‑S‑060 | Explain Supply Decisions | Use sequencing rationale and feasibility evidence for explanations. |
| Schedule Production | BN‑S‑061 Production Schedule Published | Production Execution (external) | Consume as authoritative scheduling baseline. |
| Schedule Production | BN‑S‑061 | Evaluate Supply Quality | Track schedule adherence, stability, and feasibility trends. |
| Manage Distribution | BN‑S‑070 Distribution Recommendation Created | Distribution Execution (external) | Create shipments from recommendations. |
| Manage Distribution | BN‑S‑070 | Explain Supply Decisions | Use movement rationale, allocation strategy, and evidence for explanations. |
| Manage Distribution | BN‑S‑071 Distribution Plan Published | Distribution Execution (external) | Consume as authoritative distribution baseline. |
| Manage Distribution | BN‑S‑071 | Evaluate Supply Quality | Track plan adherence, lane utilisation, and stability. |
| Manage Distribution | BN‑S‑071 | Learn From Supply | Identify network positioning improvements. |
| Sense Supply Changes | BN‑S‑080 Supply Change Assessment Updated | Detect Supply Exceptions | Determine whether change constitutes an exception. |
| Sense Supply Changes | BN‑S‑080 | Plan Supply | Trigger plan revision for supply constraints. |
| Sense Supply Changes | BN‑S‑080 | Evaluate Supply Quality | Track change frequency and patterns. |
| Sense Supply Changes | BN‑S‑080 | Learn From Supply | Identify systemic supply risks. |
| Sense Supply Changes | BN‑S‑080 | Explain Supply Decisions | Provide change evidence for explanations. |