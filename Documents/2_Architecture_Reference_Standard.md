
# Specification Meta-Model & Platform Governance

**Status:** Authoritative
**Scope:** Entire Medhavi APS ecosystem
**Traceability:** CN‑001

---

# Part 1 — Specification Meta-Model

# 1. Purpose

This document defines the formal language for describing every business capability, semantic object, decision, rule, policy, workflow, functional specification, event, notification, and algorithm within Medhavi APS. It also establishes enterprise-wide identifier standards, traceability rules, artifact lifecycles, and governance policies.

The meta-model is the single contract that all domain specifications must honour.

# 2. Fundamental Concepts

| Concept | Definition | Prefix | Cardinality Relationships |
|---------|------------|--------|---------------------------|
| Semantic Object | A named business thing with identity, attributes, relationships, and lifecycle. | SE | Owned by exactly one Capability. |
| Aggregate Root | The smallest business concept whose invariants must always hold together; defines a consistency boundary. | SE (subset) | Owns 0..* Entities and Value Objects. Modified by 0..* Aggregate Behaviors. |
| Entity | A Semantic Object with local identity, existing only within an Aggregate Root. | SE | Owned by exactly one Aggregate Root. |
| Value Object | Immutable, no identity, defined solely by attributes. | SE (or inline) | Used by any object; owned by none. |
| Reference Object | A Semantic Object owned by a different domain, referenced by identity only. | SE | Read by any object; never modified locally. |
| Knowledge Artifact | A named, versioned, traceable piece of business knowledge with confidence, evidence, and expiry. | KA | Owned by exactly one Capability. Referenced by Decisions and Algorithms. |
| Business Intent | The enterprise promise an object, capability, or workflow exists to uphold. | (none) | Declared by every Aggregate Root and Capability. |
| Aggregate Behavior | A reusable unit of business logic that changes exactly one Aggregate Root, following a formal contract. | AB | Owned by exactly one Aggregate Root. Invokes 0..* Decisions, 0..* Algorithms. Publishes 0..* Enterprise Events. Does not publish Business Notifications. |
| Business Workflow | A directed acyclic graph of nodes (Aggregate Behaviors, Decisions, Notifications) achieving a business outcome. | BW | Realises exactly one Capability Responsibility. Contains 1..* nodes. |
| Capability | A named business responsibility owning Semantic Objects, Decisions, Rules, Policies, Workflows, and Guarantees. | CA | Owns 1..* Capability Responsibilities. |
| Capability Responsibility | A single, cohesive business outcome within a capability, realised by exactly one Workflow. | CR | Realised by exactly one Business Workflow (1:1:1). |
| Functional Specification | Executable specification of a Business Workflow. Orchestrates ABs, Decisions, and Notifications. | FS | Realises exactly one Capability Responsibility. |
| Business Transaction | The unit of atomic commitment protecting exactly one Aggregate Root during an Aggregate Behavior. | (part of AB) | Owned by exactly one Aggregate Behavior. |
| Business Guarantee | Enterprise-level invariant a capability promises across all its workflows. | (in CA) | Declared by exactly one Capability. |
| Decision | A business choice producing exactly one outcome from a defined set of alternatives. | DE | Owned by exactly one Aggregate Behavior (Decision Owner). References 1..* Rules. |
| Rule | A reusable constraint categorised as Identity, Eligibility, Invariant, Behavior, or Derivation. | BR | Owned by exactly one Capability. Referenced by Decisions, Preconditions, and Algorithms. |
| Policy | Governance action when a Rule cannot be satisfied or an exceptional situation occurs. | PO | Owned by exactly one Capability. Governs 0..* Rules. |
| Enterprise Event | Immutable record of a business fact. Identity = (Aggregate ID, Event Type, Occurrence Number). | EV | Published by exactly one Aggregate Behavior. Consumed by 0..* Workflows and Notifications. |
| Business Notification | Directed communication to known consumers, with defined delivery guarantees. | BN | Owned by exactly one Capability. Publication is realised by a Business Workflow Notification Node after all referenced Enterprise Events exist. References 1..* Enterprise Events. |
| Business Algorithm | A named, versioned, traceable computation with declared algebraic properties. | BA | Owned by exactly one Capability. Invoked by 0..* Aggregate Behaviors. |

# 3. Semantic Model Architecture

## 3.1 Object Classification
Every Semantic Object belongs to exactly one classification:
- **Aggregate Root:** The smallest business concept whose invariants must always hold together. Protected by a Business Transaction.
- **Entity:** Has local identity within an Aggregate Root. Cannot exist independently.
- **Value Object:** No identity. Immutable, defined entirely by attributes. Two Value Objects are equal if all attributes are equal.
- **Reference Object:** Owned by a different domain. Referenced by identity only; never created, modified, or lifecycle-defined locally.

## 3.2 Lifecycle Specification
Every Aggregate Root and Entity with meaningful states must define its lifecycle in the Semantic Model (Chapter 4). The lifecycle includes:
- All possible states
- Permitted transitions with triggers
- Invariants that hold in each state
- Initial and terminal states

**Workflow Lifecycles** define execution states for Business Workflows: **Triggered**, **Running**, **Completed**, **Failed**, **Compensating**. These are defined alongside object lifecycles.

Functional Specifications reference object lifecycle transitions by name only (e.g., "Transition SE‑D‑001 from Received to Accepted"). They must not redefine states or transition rules.

## 3.3 Knowledge Artifact

A Knowledge Artifact is a named, versioned, traceable piece of business knowledge. It carries:
- Confidence (quantitative reliability measure)
- Evidence trail (derivation trace)
- Expiry (when re‑evaluation is needed)

**Minimum Interface:** Regardless of future detailed design, every Knowledge Artifact shall expose:
- **Identifier** (KA‑xxx)
- **Owning Capability** (CA‑xxx)
- **Version**
- **Confidence** (structured, e.g., percentage, classification, or statistical measure)
- **Evidence Reference** (trace to the producing BA‑xxx or external source)
- **Expiry Timestamp**

Owned by a Capability. Full definition deferred to Knowledge Intelligence; this placeholder ensures forward compatibility.

## 3.4 Snapshot Semantic Objects

A Snapshot Semantic Object captures a point‑in‑time representation of enterprise facts that are authoritatively owned by other Semantic Objects. The snapshot owns:
- The observation itself (what was observed and when).
- The publication lifecycle (Draft → Published → Superseded).
- Versioning and traceability.
The snapshot does **not** own:
- The lifecycle of the underlying concept.
- The business behavior that mutates the underlying concept.
- The governance rules that define the underlying concept.
- The authoritative meaning of the underlying concept.


**Attribute Selection Rule:** A snapshot shall contain only those attributes required by its declared consumers to perform their business responsibilities. Attributes that exist solely for the lifecycle or internal behavior of the authoritative source shall not be replicated. If a declared consumer cannot fulfil its responsibility using the published snapshot, the snapshot contract shall be revised.

# 4. Business Intent

Every Aggregate Root, Capability, and Business Workflow must declare its Business Intent — the enterprise promise it upholds. Intent answers *why* the enterprise needs this thing, not what it contains.

**Constraints:**
- Must never describe implementation, process, ownership, or lifecycle.
- Must only state what the enterprise can rely on because this thing exists.
- Must be testable (verifiable by observing enterprise state or events).

Example for Aggregate Root **Enterprise Demand Picture**: "Provide exactly one authoritative planning interpretation for a Planning Scope at any point in time."

# 5. Aggregate Behavior Contract

Every Aggregate Behavior must satisfy a formal contract:

| Section | Description |
|---------|-------------|
| Purpose | Business action performed. |
| Business Intent | Enterprise promise upheld. |
| Owned Aggregate | Exactly one Aggregate Root whose state is changed. |
| Required Input State | Lifecycle state(s) the aggregate must be in before execution. |
| Produced Output State | Lifecycle state(s) the aggregate transitions to upon success. |
| Invoked Decisions | 0..* Decisions consulted. |
| Invoked Algorithms | 0..* Business Algorithms executed. |
| Published Events | 0..* Enterprise Events published upon success. Aggregate Behaviors do not publish Business Notifications. Business Notifications are published by Business Workflow Notification Nodes. |
| Business Transaction | Exactly one, protecting the Owned Aggregate (see §6). |
| Idempotency Guarantee | Whether re‑execution with same inputs produces same outcome without duplicate effects. |
| Concurrency Guarantee | Business promise about ordering and isolation for concurrent invocations on the same aggregate. |

Aggregate Behavior is owned by the Aggregate Root (Lifecycle Owner) and documented in the Semantic Model or a dedicated Aggregate Behavior catalogue. Functional Specifications invoke Aggregate Behaviors by name; they must not inline behavior logic.

# 6. Business Transaction

A Business Transaction is the unit of atomic commitment protecting exactly one Aggregate Root during an Aggregate Behavior.

- **Scope:** Exactly one aggregate. Cross‑aggregate workflows use multiple transactions; consistency across aggregates is eventual.
- **Semantics:**
  - **Atomicity:** All state changes to the aggregate are applied together or not at all.
  - **Consistency:** Aggregate invariants are enforced at transaction boundaries.
  - **Isolation:** Concurrent transactions on the same aggregate are serialized based on business order.
  - **Durability:** Once committed, the aggregate state change is permanent.
- **Location:** The Business Transaction definition belongs to the Aggregate Behavior contract. A Functional Specification references it; it never redefines it.

# 7. Business Workflow

A Business Workflow is a directed acyclic graph (DAG) of nodes achieving a business outcome.

**Node Types:**
| Node | Description |
|------|-------------|
| Behavior Node | Invokes an Aggregate Behavior. |
| Decision Node | Evaluates a Decision and branches on its outcome. |
| Notification Node | Publishes a Business Notification owned by a Capability. It may execute only after every Enterprise Event referenced by that Business Notification exists in the workflow execution context. |
| Fork Node | Initiates parallel execution of multiple branches. |
| Join Node | Waits for all incoming parallel branches before continuing. |
| Start Node | Receives the triggering Enterprise Event or scheduled time. |
| End Node | Marks workflow completion (success or failure). |

**Transition Semantics:** An edge A → B means:
- If A is a Behavior Node: B executes after A commits successfully.
- If A is a Decision Node: B executes after A's outcome is determined, following the branch for that outcome.
- If A is a Notification Node: B executes after A publishes.
- A Notification Node shall not publish a Business Notification unless every Enterprise Event referenced by that Business Notification has already been published by a preceding Behaviour Node or exists as the workflow-triggering event.
- **Trigger:** An Enterprise Event or a scheduled time, received at the Start Node.
- **Scope:** May span multiple aggregates, but each aggregate is modified only by its own Aggregate Behaviors.
- **Lifecycle:** Triggered → Running → Completed / Failed / Compensating.
- **Mapping:** 1:1:1 — Capability Responsibility → Business Workflow → Functional Specification.

*Workflow Reuse: A Business Workflow is the exclusive realisation of exactly one Capability Responsibility. Business Workflows shall not be referenced as sub‑workflows by other Workflows. Reuse of orchestration logic across multiple Capability Responsibilities is achieved by composing the same Aggregate Behaviors, Decisions, and Notification patterns within each Workflow, not by nesting BWs. This preserves the single‑ownership principle and avoids hidden coupling between capabilities.*

# 8. Capability and Capability Responsibility

A **Capability** is a named business responsibility that owns Semantic Objects, Decisions, Rules, Policies, Workflows, and Business Guarantees.

- **Business Intent:** Declared; the enterprise promise the capability upholds.
- **Business Guarantees:** Declared in the Capability chapter; cross‑workflow invariants.
- **Capability Responsibility:** A single, cohesive business outcome.
  - **Granularity Principle:** One responsibility protects one aggregate consistency outcome. If it modifies two aggregates independently, split it.
  - **Typing (Guidance):** Common patterns include Capture, Evaluate, Transform, Publish, Govern. Formal taxonomy deferred until sufficient domains exist.

# 9. Functional Specification

The executable specification of a Business Workflow. Structure:

1. **Business Contract** — Consumes, Produces, Transitions, Publishes, Invokes, Guarantees (compact summary).
2. **Trigger** — Event or schedule.
3. **Preconditions** — Table of Eligibility Rule IDs with brief labels.
4. **Semantic Objects** — Read, Create, Update, Archive (reference IDs).
5. **Behavior** — Sequence of steps, each invoking an AB, Decision, Notification, or workflow control node. Uses declarative business verbs (Establish, Determine, Assign, Calculate, Supersede, Publish). No validation logic (in Preconditions/Decisions), no data manipulation (in ABs).
6. **Business Transaction** — Reference to the Aggregate Behavior contract(s) governing this workflow.
7. **Postconditions** — Guarantees upon success.
8. **Failure Behavior** — Resulting business state (not software exceptions).
9. **Recovery Behavior** — Idempotency and safe re‑execution.
10. **Concurrency Guarantees** — Business promises, not technical locks.
11. **Example** — Worked example.

>Functional Specifications invoke Business Notification publication by BN identifier only. They do not define Business Notification contracts, delivery guarantees, ordering guarantees, or timeliness guarantees.

# 10. Business Guarantee

Enterprise‑level invariant upheld by a capability across all its workflows. Testable, verifiable by observing aggregate states and events. Declared in the Capability chapter.

# 11. Decision

A business choice producing exactly one outcome from a predefined set of alternatives. Owned by the Aggregate Behavior that invokes it (Decision Owner).

Structure: Purpose, Trigger, Inputs, Alternatives, Criteria (referencing Rules), Confidence, Rationale Template.

Defined in the Decision Model chapter; invoked by name in Functional Specifications.

# 12. Rules

Five categories with distinct enforcement points:

| Category | Enforcement Point |
|----------|-------------------|
| Identity Rule | At object creation. |
| Eligibility Rule | Preconditions of Functional Specifications. |
| Invariant Rule | At every aggregate commit boundary. |
| Behavior Rule | During Decision evaluation. |
| Derivation Rule | At Algorithm execution time. |

Rules are defined once in the Rule Model chapter with unique IDs (BR‑xxx). Referenced by ID everywhere else; never restated.

A Business Guarantee shall be expressed as a condition over the states of one or more Semantic Objects or over the occurrence/non‑occurrence of specific Enterprise Events. Violation of the guarantee must be detectable by monitoring those states or events. This ensures guarantees are verifiable and not merely aspirational.

# 13. Policy

Governance action when a Rule cannot be satisfied or an exceptional situation occurs. Categories: Authorization, Compliance, Automation, Exception, Audit.

Defined in Policy Model chapter; referenced by Functional Specifications.

# 14. Enterprise Events and Business Notifications

**Enterprise Event (EV):** Immutable business fact. Identity = (Aggregate ID, Event Type, Occurrence Number). Occurrence Number is monotonically increasing per aggregate. Published by an Aggregate Behavior.

**Business Notification (BN):** Directed communication to known consumers.

A Business Notification is owned by exactly one Capability.

A Business Notification is published by a Business Workflow Notification Node acting on behalf of that Capability.

A Business Notification shall not be published by an Aggregate Behaviour, Decision, Business Algorithm, or Functional Specification step except through a Business Workflow Notification Node.

A Business Notification may be published only after every Enterprise Event referenced by that Business Notification exists.

Notification publication occurs after the relevant Aggregate Behaviour has committed and is not part of the Aggregate Business Transaction.

Mandatory delivery guarantees:
- **Delivery:** at‑least‑once, at‑most‑once, or exactly‑once.
- **Ordering:** per aggregate, per type, or none.
- **Timeliness:** real‑time, near‑real‑time, batch.

A Notification references the underlying Enterprise Event(s) but defines the contract with consumers. Notifications are defined in the Capability chapter.

# 15. Business Algorithm

The Business Algorithm Architecture Standard establishes the semantic progression, output types, and dependency rules that govern all Business Algorithms across every Intelligence Domain. It ensures that algorithms are composable, auditable, and aligned with the enterprise reasoning model, regardless of the specific domain or capability they serve.

## 15.1 Declares algebraic properties:

| Property | Definition |
|----------|------------|
| Deterministic | Same inputs always produce same outputs. |
| Idempotent | Re‑execution produces no additional effects. |
| Pure | No side effects; computes only from inputs. |
| Order Sensitive | Whether input order affects output. |
| Explainable | Declares the algorithm’s ability to produce a human‑readable computation trace. Acceptable values: Full (complete trace), Partial (traceable inputs/outputs/provenance but internal logic is not fully decomposable), or None. Partial is only permitted when the algorithm’s primary output is a Knowledge Artifact consumed through transparent Decisions. |

Defined in Business Algorithm catalogue. Invoked by name from Aggregate Behaviors or Functional Specifications.

  - Opaque Model Exception: A Business Algorithm that employs machine learning or other non‑decomposable techniques may declare Explainable = Partial provided that:

  - Its primary output is a Knowledge Artifact (KA);

  - The algorithm’s version, training data provenance, confidence metrics, and known limitations are fully documented and traceable;

The final business outcome is determined by transparent Decisions and Rules that consume the Knowledge Artifact, preserving full explainability of the business outcome.

## 15.2 Semantic Progression

Every Business Algorithm belongs to exactly one stage in the enterprise reasoning pipeline. The progression from raw computation to authoritative business state is fixed:

| Stage | Verb | Purpose | Produces | Example |
|-------|------|---------|----------|---------|
| **Parameter** | Compute | Derive a governed numeric value from a formula and governed inputs. | Parameter | Safety Stock Quantity |
| **Threshold** | Compute | Derive a governed trigger or limit value from parameters and policy. | Threshold | Reorder Point |
| **Assessment** | Interpret | Evaluate evidence and produce a structured, multi‑dimensional exposure assessment. | Assessment | Stockout Exposure Assessment |
| **State** | Determine | Apply a governed enterprise taxonomy to assessments to produce an authoritative, mutually exclusive business state. | State | Enterprise Inventory Health State |

- A **Parameter** is a single, governed numeric value (e.g., 83 units).
- A **Threshold** is a governed value that triggers a business action (e.g., reorder when inventory reaches 583 units).
- An **Assessment** is a structured interpretation of evidence. It must include, for each evaluated dimension: evidence, interpretation, exposure, likelihood, impact, confidence, and reason codes. Assessments do not produce a single composite score or a business classification.
- A **State** is the authoritative enterprise business classification determined by applying a governed taxonomy (e.g., Healthy, At Risk). States are mutually exclusive for a given entity at a given point in time.

This progression is not merely a documentation convention; it is the architectural law for all Business Algorithms in the Medhavi ecosystem.

## 15.3 Standardized Output Types

Every Business Algorithm must produce exactly one of the four output types defined in §15.2. Hybrid algorithms that both compute a parameter and produce a classification (or that combine multiple stages) are prohibited. This constraint ensures that each algorithm has a single responsibility and that the enterprise reasoning chain remains transparent and auditable.

## 15.4 Dependency Rule

Business Algorithms must respect a strict dependency hierarchy:

```
Parameter
    ↓
Threshold
    ↓
Assessment
    ↓
State
    ↓
Decision
    ↓
Publication
```

**Rule:**
- A Business Algorithm at stage N may depend only on algorithms at stage N‑1 or below.
- A State algorithm may depend on an Assessment algorithm, but may never depend directly on a Threshold or Parameter.
- A Threshold algorithm may depend on a Parameter, but may not depend on an Assessment or State.
- No stage may be skipped.
- Decisions (owned by Aggregate Behaviors) may consume a State, but a State may never depend on a Decision.
- Publication (via Functional Specifications) may consume a Decision, but may not directly consume a State or Assessment without passing through a Decision.

This rule prevents backward dependencies and ensures that each stage adds a distinct layer of enterprise reasoning. It applies uniformly across all Intelligence Domains.

## 15.5 Reason Codes in Assessments

Every Assessment‑stage algorithm must include Reason Codes for each evaluated dimension. Reason codes are governed, stable identifiers (e.g., `BelowSafetyStock`, `HighDemandVariability`) that describe the primary causes contributing to the assessment. They are defined by the relevant domain policy and provide a structured, machine‑consumable basis for explanations, dashboards, and analytics.

## 15.6 Traceability and Compliance

- Every Business Algorithm must declare its stage, output type, and dependencies in its specification (see the Algorithm Traceability table).
- Domain specifications must demonstrate compliance with this standard for every Business Algorithm they define.
- Architecture reviews must verify that no algorithm violates the dependency rule or combines multiple output types.

### 15.7 Constructive, Discovery, and Explanation Algorithms

The semantic progression defined in § 15.2 (Parameter → Threshold → Assessment → State) governs evaluation algorithms — those that interpret enterprise facts to produce assessments and business states.
Enterprise reasoning also includes algorithms that synthesise plans, discover patterns, and compose explanations. These algorithms do not fit the evaluation progression and shall be classified as follows.

#### 15.7.1 Constructive Algorithms

A **Constructive Algorithm** synthesises a new enterprise artifact (a plan, schedule, recommendation, or similar) from enterprise facts, policies, and governing rules. It simultaneously assesses the feasibility, confidence, and quality of the constructed artifact.

**Characteristics:**
- Consumes enterprise facts and governed parameters.
- Produces a complete artifact that did not previously exist.
- Includes assessment metadata (feasibility, confidence, stability) within the output.
- Does not select among alternatives; that remains the role of a Decision.

**Stage classification:** Constructive algorithms are not required to declare a stage from the evaluation progression. Instead, they declare `Stage: Constructive` and `Output Type: Plan`, `Output Type: Schedule`, `Output Type: Recommendation`, or a domain‑specific artifact type.

**Dependency rule:** A Constructive algorithm may depend on algorithms at any stage of the evaluation progression (Parameter, Threshold, Assessment, State) and on other Constructive algorithms. Dependencies among Constructive algorithms shall form a directed acyclic graph: a Constructive algorithm shall not depend, directly or transitively, on itself. Where one Constructive algorithm consumes the output of another, the consuming algorithm shall reference the producing algorithm by its Business Algorithm identifier (BA-xxx), and the producing algorithm's output shall be fully determined before consumption. Dependency cycles are prohibited and constitute an architectural defect.

**Examples:** BA‑S‑015 (Balance Supply and Demand), BA‑S‑040 (Construct Production Schedule), BA‑S‑050 (Construct Distribution Recommendation).

#### 15.7.2 Discovery Algorithms

A **Discovery Algorithm** analyses historical evidence across multiple periods to identify recurring patterns, derive learnings, and propose improvement opportunities.

**Characteristics:**
- Consumes historical enterprise evidence from many sources.
- Produces candidate learnings with confidence assessments.
- Does not determine whether the learning is adopted; that is a governance decision.

**Stage classification:** `Stage: Discovery`, `Output Type: Learning`.

**Dependency rule:** A Discovery algorithm may depend on any evaluation algorithm and on Constructive algorithms whose outputs are part of the historical record.

**Example:** BA‑S‑100 (Derive Supply Learning).

#### 15.7.3 Explanation Algorithms

An **Explanation Algorithm** composes the deterministic, evidence‑based reasoning behind an enterprise conclusion from pre‑existing governed knowledge. It does not generate new reasoning or natural language.

**Characteristics:**
- Consumes the historical versions of evidence, decisions, policies, and assumptions that were in effect when the explained artifact was produced.
- Produces a canonical, structured explanation.
- Is not part of the planning reasoning chain.

**Stage classification:** `Stage: Explanation`, `Output Type: Explanation`.

**Dependency rule:** An Explanation algorithm may depend on any artifact that carries preserved evidence, regardless of stage.

**Example:** BA‑S‑090 (Compose Supply Explanation).

#### 15.7.4 Relationship to the Evaluation Progression

Constructive, Discovery, and Explanation algorithms exist alongside the evaluation progression. They are not required to fit into Parameter / Threshold / Assessment / State. Their stage classification shall be one of `Constructive`, `Discovery`, or `Explanation` as defined above. The dependency rule in § 15.4 is amended to allow these algorithm types to depend on algorithms of any evaluation stage.

---

# 16. Ownership

Three ownership dimensions:

- **Semantic Authority:** The enterprise authority responsible for the meaning, identity, and invariants of a concept. Every Semantic Object shall have a Semantic Authority.
- **Operational Capability:** The Capability responsible for the business behaviors that create, change, or govern the object. An Operational Capability is mandatory only when the enterprise has defined operational behavior for that object.
- **Decision Owner:** The Aggregate Behavior that invokes a Decision.

- **Semantic Object Ownership Dimensions**

For every Semantic Object, the following four dimensions shall be explicitly declared:

| Ownership Dimension | Question Answered |
|---------------------|-------------------|
| Semantic Authority | Who defines what this concept means? |
| Steward Domain | Which domain is responsible for evolving the semantic definition? |
| Operational Capability | Which capability creates or mutates instances of this concept? (if any) |
| Primary Consumers | Which domains and capabilities depend on this concept? |

These dimensions extend the Authority Specification Contract. The Authority Specification Contract template is updated accordingly.

---

# 17. Common Specification Contracts

**Status:** Authoritative
**Traceability:** CN‑001, CN‑003, CN‑004, CN‑005, CN‑006, CN‑008, CN‑010, ARS §3–§10, §16, §17

## 17.1. Purpose

This addendum defines four reusable **Common Specification Contracts** — shared specification primitives that capture recurring architectural concerns across all artifact types. They ensure that every domain artifact explicitly declares its authority, lifecycle, behavior, and consumer guarantees, without duplicating these concerns in every template.

They are **specification contracts**, distinct from the artifact‑specific contracts already defined in the ARS (such as the Aggregate Behavior Contract, Input Contract, or Output Contract). They provide the vocabulary and structure that those artifact contracts draw upon.


## 17.2. The Four Common Specification Contracts

The contracts are organised in a natural dependency order. Each downstream contract assumes the integrity of the contracts above it.

```
Authority Specification Contract
             ↓
  Lifecycle Specification Contract
             ↓
  Behavioral Specification Contract
             ↓
   Consumer Specification Contract
```

An artifact’s Consumer contract shall not contradict its Behavioral contract; its Behavioral contract shall not contradict its Lifecycle contract; its Lifecycle contract shall not contradict its Authority contract.


## 17.3 Authority Specification Contract

**Purpose:** Declare exactly who owns an artifact and under what constraints it is authoritative.

| Section | Question Answered |
|---------|-------------------|
| **Business Owner** | Which Capability is responsible for the meaning and quality of this artifact? |
| **Authoritative Representation** | What enterprise fact does this artifact establish as truth? |
| **Business Responsibility** | What specific business obligation does the owner undertake? |
| **Authority Scope** | Within what scope (enterprise, business unit, planning horizon) is this artifact authoritative? |
| **Intended Consumers** | Which capabilities or external systems may consume this artifact as governed truth? |
| **Non‑Intended Consumers** | Which capabilities should not consume this artifact, even if technically possible? |
| **Supersedes** | Does this artifact replace a previous authoritative artifact? Which one? |
| **Superseded By** | Which artifact, if any, has replaced this one as authoritative? |


## 17.4 Lifecycle Specification Contract

**Purpose:** Define the governed states, transitions, and rules that a stateful artifact obeys throughout its existence.

| Section | Question Answered |
|---------|-------------------|
| **States** | What are the possible business states of this artifact? |
| **Entry Criteria** | What must be true for the artifact to enter each state? |
| **Exit Criteria** | What must be true for the artifact to leave each state? |
| **State Transition Trigger** | What business event or condition causes a state transition? |
| **Responsible Behavior** | Which Aggregate Behavior is responsible for executing the transition? |
| **Terminal States** | Which states are terminal, and what do they mean for consumers? |
| **History Preservation** | Are previous states retained? For how long? |
| **Versioning Rules** | How are versions created, identified, and superseded? |


## 17.5 Behavioral Specification Contract

**Purpose:** Define *how the enterprise behaves*, not how software behaves. Applicable to any artifact that produces a business outcome or state change.

| Section | Question Answered |
|---------|-------------------|
| **Trigger** | What business condition causes this artifact to execute or change? |
| **Preconditions** | What business conditions must already be true before this artifact can execute? |
| **Business Behavior** | What is the expected business behavior? Describe in business verbs. |
| **Exceptional Conditions** | How should the business behave if normal conditions cannot be satisfied? |
| **Postconditions** | What business facts are guaranteed after successful completion? |
| **Outcome When Preconditions Are Not Satisfied** | What business outcome occurs if the preconditions are not met? |


## 17.6 Consumer Specification Contract

**Purpose:** Define what downstream capabilities may rely upon when consuming an authoritative artifact.

| Section | Question Answered |
|---------|-------------------|
| **Business Guarantees** | What enterprise promises does this artifact uphold for its consumers? |
| **Required Interpretation** | What must a consumer understand about this artifact to use it correctly? |
| **Known Limitations** | What does this artifact *not* guarantee? What should consumers not assume? |
| **Version Expectations** | How should consumers handle version changes? |
| **Freshness Expectations** | How current is this artifact expected to be? How stale is too stale? |
| **Intended Consumers** | Which capabilities are explicitly permitted to consume this artifact? |
| **Non‑Intended Consumers** | Which capabilities should not consume this artifact, even if technically possible? |
| **Declared Consumers** | Capabilities explicitly consuming this object. |
| **Consumer Responsibility** | Why each consumer requires the object. |
| **Required Attributes** | Which published attributes each consumer depends upon (reference only, not a copy). |
| **Authoritative Source** | The owning Semantic Object. |

## 17.7. Applicability by Artifact Category

Rather than enumerating every artifact type, contracts are mapped to four **artifact categories**. Each artifact type belongs to one or more categories, and the applicable contracts follow deterministically.

### 17.7.1 Artifact Categories

| Category | Definition | Example Artifacts |
|----------|-----------|-------------------|
| **Authoritative Artifact** | An artifact that establishes governed enterprise truth. | Capability, Semantic Object, Decision, Policy, Knowledge Artifact |
| **Stateful Artifact** | An artifact with a governed lifecycle. | Aggregate Root, Business Workflow, Policy, Rule |
| **Behavioral Artifact** | An artifact that describes or orchestrates enterprise behavior. | Aggregate Behavior, Business Workflow, Functional Specification, Decision, Business Algorithm |
| **Governance Artifact** | An artifact that constrains or governs other artifacts. | Policy, Rule |

### 17.7.2 Mandatory Applicability

Every artifact shall declare the contracts that correspond to its categories.

| Category | Authority Contract | Lifecycle Contract | Behavioral Contract | Consumer Contract |
|----------|:-----------------:|:------------------:|:--------------------:|:-----------------:|
| Authoritative | ✓ | | | ✓ |
| Stateful | | ✓ | | |
| Behavioral | | | ✓ | |
| Governance | ✓ | ✓ | ✓ | |

If an artifact belongs to multiple categories (e.g., a Policy is Authoritative, Stateful, Behavioral, and Governance), it shall satisfy all corresponding contracts.


## 17.8 Relationship to Existing ARS Structures

These Common Specification Contracts are not new artifact types. They are shared vocabulary. Where the ARS already defines a complete concrete contract for a specific artifact type, that concrete contract remains authoritative. The common contracts serve as the underlying semantic model.

| Existing ARS Element | Expresses |
|----------------------|-----------|
| Business Intent (§4) | Authority Contract (partially) |
| Semantic Object Lifecycle (§3.2) | Lifecycle Specification Contract |
| Aggregate Behavior Contract (§5) | Behavioral Specification Contract (concrete realisation) |
| Business Notification delivery guarantees (§14) | Consumer Specification Contract (runtime expression) |
| Capability Business Guarantees (§8) | Consumer Specification Contract (static expression) |


## 17.9 Integration into Domain Specification Templates

The domain specification structure defined in ARS §17 shall reference these contracts. For example, the Semantic Object chapter template becomes:

```
Semantic Object

Business Intent (ARS §4)
Authority Specification Contract
Identity
Information Model
Invariants
Lifecycle Specification Contract
Consumer Specification Contract
Traceability
```

Not every artifact will use all four contracts. The domain author follows the mandatory applicability rules in §3.2 of this addendum.


## 17.10. Compliance

All new domain specifications shall satisfy the mandatory applicability rules. Existing specifications shall be updated during their next revision cycle to declare the required contracts. Compliance is verifiable: an artifact that belongs to a category but omits its mandatory contract is non‑conformant.


# 18. Document Architecture

## 18.1 Purpose

Every Intelligence Domain Specification shall follow a common document architecture to ensure:

- consistent organisation across all Intelligence domains
- single semantic ownership
- clear architectural boundaries
- deterministic traceability
- progressive development from enterprise semantics to implementation
- elimination of duplicated business meaning

Each chapter owns one architectural concern.

A lower chapter shall never redefine the responsibility of a higher chapter.

## 18.2 Architectural Dependency

Every chapter depends only on chapters above it.

```
Purpose & Scope
        ↓
Business Objectives
        ↓
Enterprise Measures
        ↓
Domain Semantic Model
        ↓
Capability Model
        ↓
Decision Model
        ↓
Rule Model
        ↓
Policy Model
        ↓
Functional Specification Model
        ↓
Business Algorithm Model
```

No chapter may introduce enterprise semantics that belong to an earlier chapter.

## 18.3 Chapter Structure

### Chapter 1 — Purpose & Scope

#### Purpose

Defines why this Intelligence domain exists.

#### Contains

- Business Purpose
- Scope
- Enterprise Questions
- Architectural Position
- Out of Scope

#### Shall Not Contain

- semantic definitions
- behaviors
- rules
- policies
- workflows

### Chapter 2 — Business Objectives

#### Purpose

Defines what business outcomes the domain is responsible for achieving.

#### Contains

- Business Objectives
- Success Criteria
- Enterprise Outcomes
- Business Responsibilities

#### Shall Not Contain

- implementation
- algorithms
- workflows

### Chapter 3 — Enterprise Measures

#### Purpose

Defines how enterprise success is measured.

#### Contains

- KPIs
- Enterprise Metrics
- Measurement Definitions
- Performance Indicators

#### Shall Not Contain

- decision logic
- policies
- behaviors

### Chapter 4 — Domain Semantic Model

#### Purpose

Defines the authoritative business meaning of every semantic object owned by this domain.

This chapter is the semantic foundation of the domain.

No later chapter may redefine anything contained here.

#### 4.1 Domain Semantic Principles

Contains

- semantic principles
- ownership principles
- extension principles
- semantic constraints

#### 4.2 Domain Dependency Declaration

Contains

- Enterprise Semantic Objects consumed
- Required Attributes
- Dependency Purpose
- Consumer Responsibilities

No semantic object may be used later unless declared here.

#### 4.3 Domain-Owned Semantic Objects

Contains all semantic objects owned by this domain.

#### 4.3.1 Aggregate Roots

Each Aggregate Root contains the following sections in this order:

1. **Business Intent** – *Why does this aggregate exist?* A single sentence stating the enterprise promise it upholds.
2. **Enterprise Meaning** – *What enterprise concept does this aggregate represent?* A concise definition of the business reality it models.
3. **Identity** – *How is the aggregate uniquely identified?* The business key that distinguishes one instance from another.
4. **Authority Specification Contract** – *Who governs the meaning and content of this aggregate?* (Semantic Authority, Steward Domain, Operational Capability, etc.)
5. **Consumer Specification Contract** – *What may downstream consumers rely upon?* (Guarantees, required interpretation, known limitations, version expectations, freshness expectations.)
6. **Lifecycle Specification Contract** – *What are the governed states and transitions?* (States, entry/exit criteria, transition triggers, responsible behaviors, terminal states, history preservation, versioning rules.)
7. **Information Model** – *What enterprise facts does the aggregate hold?* Structured representation of its attributes and owned entities.
8. **Relationships** – *How is this aggregate structurally related to other enterprise concepts?* Describes only structural semantic relationships (contains, references, composes, specialises, associates). Answers: “How is this concept related to other concepts?”
9. **Dependencies** – *What semantic contracts must already exist for this aggregate to be valid?* Lists the required enterprise concepts and the specific attributes the aggregate depends on. Answers: “What must already be understood?”
10. **Invariants** – *What business rules must always hold true?* Conditions that the aggregate guarantees at every commit boundary.
11. **Traceability** – *What is the architectural lineage of this aggregate?* (Upward to Constitution/ARS, admission history, downward to capabilities and domain specifications.)

**Previous sections no longer used:** Enterprise Information Contract, Ontology Classification, Business Operations, Versioning Trigger. Their content has been redistributed into the above sections or removed where redundant.


#### Aggregate Behaviors

Aggregate Behaviors are documented immediately beneath their owning Aggregate Root. Each Aggregate Behavior contains the following sections in this order:

1. **Purpose** – *What business action does this behavior perform?*
2. **Business Intent** – *What enterprise promise does this behavior uphold?*
3. **Trigger** – *What business condition or event causes this behavior to execute?*
4. **Preconditions** – *What business conditions must already be true before execution?*
5. **Semantic Preconditions** – *Which enterprise semantic contracts must already be satisfied?* (Explicitly declares the concepts this behavior assumes are complete and available.)
6. **Business Behavior** – *What is the expected business behavior?* Described in business verbs; no implementation details.
7. **State Transitions** – *What lifecycle state changes does this behavior produce?* (Required input state → produced output state.)
8. **Business Transaction** – *What is the atomic unit of work protecting the aggregate?* (Scope, semantics.)
9. **Decisions Invoked** – *Which business decisions does this behavior evaluate?* (List of DE‑xxx identifiers.)
10. **Rules Enforced** – *Which business rules does this behavior enforce?* (List of BR‑xxx identifiers.)
11. **Policies Referenced** – *Which policies govern this behavior?* (List of PO‑xxx identifiers.)
12. **Algorithms Invoked** – *Which business algorithms does this behavior execute?* (List of BA‑xxx identifiers.)
13. **Events Published** – *Which enterprise events are published upon success?* (List of EV‑xxx identifiers.)
14. **Idempotency** – *Does re‑execution with the same inputs produce the same outcome without duplicate effects?*
15. **Concurrency** – *What are the business promises about ordering and isolation for concurrent invocations?*
16. **Exceptional Conditions** – *How should the business behave if normal conditions cannot be satisfied?*
17. **Postconditions** – *What business facts are guaranteed after successful completion?*
18. **Traceability** – *Architectural lineage of the behavior (owning aggregate, invoking FS, etc.).*

#### 4.3.2 Entities

Each Entity contains

- Business Intent
- Identity
- Information Model
- Relationships
- Invariants
- Lifecycle
- Traceability

#### 4.3.3 Value Objects

Each Value Object contains

- Business Intent
- Structure
- Validation Rules
- Invariants
- Traceability

#### 4.3.4 Reference Objects

Each Reference Object contains

- Business Intent
- Identity
- Information Model
- Relationships
- Lifecycle
- Traceability

#### 4.3.5 Knowledge Artifacts

Each Knowledge Artifact contains

- Business Intent
- Knowledge Produced
- Publication Pattern
- Lifecycle
- Consumers
- Traceability

#### 4.3.6 Enumerations

Contains

- Enterprise Enumerations
- Domain Enumerations
- Classification Tables

#### 4.3.7 Domain Relationships

Defines relationships owned only by this domain.

Contains

- Relationship Purpose
- Participating Objects
- Cardinality
- Ownership
- Constraints

#### 4.3.8 Domain Semantic Completeness

Defines

- Semantic Completeness
- Consumer Completeness
- Ownership Completeness
- Traceability Completeness

### Chapter 5 — Capability Model

Defines what the domain is responsible for doing.

Each Capability contains

- Business Intent
- Enterprise Question
- Responsibilities
- Business Guarantees
- Capability Boundaries
- Produced Knowledge
- Consumed Knowledge
- Notifications — BN identifiers owned by the Capability. Each Business Notification defines business purpose, intended consumers, referenced Enterprise Events, and delivery guarantees. Publication is realised by Business Workflow Notification Nodes.
- Dependencies
- Traceability

Capabilities shall not define business rules or algorithms.

### Chapter 6 — Decision Model

Each Decision contains

- Decision Question
- Alternatives
- Evidence
- Governing Rules
- Explainability
- Decision Outcome
- Traceability

Decisions determine.

Decisions never calculate.

### Chapter 7 — Rule Model

Rules are categorised according to the ARS meta-model:

- **Identity Rules** — enforced at object creation.
- **Eligibility Rules** — enforced at Functional Specification preconditions.
- **Invariant Rules** — enforced at every aggregate commit boundary.
- **Behavior Rules** — enforced during Decision evaluation.
- **Derivation Rules** — enforced at Algorithm execution time.

Each Rule contains

- Rule Statement
- Purpose
- Evaluation Scope
- Enforcement Point
- Governing Policy
- Traceability

Rules never own business policy.

### Chapter 8 — Policy Model

Each Policy contains

- Governance Intent
- Governed Configuration
- Authority Specification Contract
- Consumer Specification Contract
- Lifecycle Specification Contract
- Governed Rules
- Policy Dependencies
- Exceptional Conditions
- Traceability

Policies govern interpretation.

Policies never own enterprise facts.

### Chapter 9 — Functional Specification Model

Each Functional Specification contains

- Business Intent
- Workflow
- Orchestration Steps
- Aggregate Behaviors Invoked
- External Interactions
- Preconditions
- Postconditions
- Exceptional Flows
- Notifications — BN identifiers owned by the Capability. Each Business Notification defines business purpose, intended consumers, referenced Enterprise Events, and delivery guarantees. Publication is realised by Business Workflow Notification Nodes.
- Traceability

Functional Specifications orchestrate.

They never contain business rules, decision logic, or algorithms.

### Chapter 10 — Business Algorithm Model

Each Algorithm contains

- Business Intent
- Enterprise Question
- Mathematical Foundation
- Authoritative Inputs
- Input Authority
- Governing Policies
- Computation Stages
- Output Contract
- Dependencies
- Explainability
- Determinism
- Traceability

Algorithms compute.

Algorithms never make business decisions.

## 18.4 Cross-Referencing Rules

The following architectural rules apply throughout the specification:

- Every enterprise concept has exactly one authoritative definition.
- No chapter may redefine content owned by an earlier chapter.
- All cross-references shall use architectural identifiers only (SE-xxx, AB-xxx, DE-xxx, BR-xxx, PO-xxx, FS-xxx, BA-xxx, EV-xxx, BN-xxx).
- Semantic Objects define lifecycles; later chapters reference lifecycle states but do not redefine them.
- Rules, Decisions, Policies, Functional Specifications, and Algorithms consume Semantic Objects; they do not redefine them.
- Functional Specifications orchestrate Aggregate Behaviors rather than directly executing Decisions, Rules, or Algorithms.
- Every dependency shall resolve to an authoritative artifact before implementation begins.

## 18.5 Architectural Completion

Chapter readiness gates and the Architectural Completion Principle are defined in the Medhavi Architecture Review Guidelines and must be satisfied before a Domain Specification is considered complete. A domain shall not proceed to implementation until every gate has been passed.


> **Each domain specification shall use the Common Specification Contracts (Chapter 17) as appropriate.**

# Part 2 — Platform Governance

# 19. Architecture Layers and Artifacts

The Medhavi architecture is organised into the following layers. Each layer defines a specific architectural concern and a set of artifacts with their assigned identifier prefixes.

**Layer Model**

```
CN → SE → CA → DE → BR → PO → AB → BW → FS → BA → CODE → TE / VI / AI
```

| Layer | Prefix | Artifact Type | Description |
|-------|--------|---------------|-------------|
| Constitution | CN | Constitution | Enterprise principles – the supreme governing authority. |
| Semantic Model | SE | Semantic Entity | Business objects (Aggregate Roots, Entities, Value Objects, Reference Objects, Knowledge Artifacts). |
| Capability Model | CA | Capability | Business responsibilities that own Semantic Objects, Decisions, Rules, and Policies. |
| Capability Responsibility | CR | Capability Responsibility | A single cohesive business outcome within a Capability. |
| Decision Model | DE | Decision | A business choice with defined alternatives. |
| Rule Model | BR | Business Rule | Reusable constraints categorised as Identity, Eligibility, Invariant, Behavior, or Derivation. |
| Policy Model | PO | Policy | Governance action when a Rule is violated or an exceptional situation occurs. |
| Aggregate Behavior | AB | Aggregate Behavior | A reusable unit of business logic that changes exactly one Aggregate Root. |
| Business Workflow | BW | Business Workflow | A directed acyclic graph of ABs, Decisions, and Notifications. |
| Functional Specification | FS | Functional Specification | Executable specification of a Business Workflow. |
| Business Algorithm | BA | Business Algorithm | A named, versioned, traceable computation with declared algebraic properties. |
| Implementation | (none) | (none) | Realisation of architecture in software. |
| Telemetry | TE | Telemetry | Observability data. |
| Violation | VI | Violation | Exception detection result. |
| AI Recommendation | AI | AI Recommendation / Explanation | AI‑generated output. |

**Additional Artifacts**

| Prefix | Artifact Type | Description |
|--------|---------------|-------------|
| EV | Enterprise Event | Immutable record of a business fact. |
| BN | Business Notification | Directed communication to known consumers. |
| KA | Knowledge Artifact | A named, versioned, traceable piece of business knowledge. |
| BO | Business Objective | A measurable business outcome a domain is responsible for. |
| PI | Performance Indicator | A metric that measures achievement of a Business Objective. |

## Domain Catalog

| Code | Domain |
|------|--------|
| C | Core (enterprise‑wide) |
| D | Demand Intelligence |
| S | Supply Intelligence |
| R | Promise Intelligence |
| N | Scenario Intelligence |
| K | Knowledge Intelligence |
| A | AI (cross‑cutting) |
| O | Operations |

## Identifier Standard

**Format:** `<PREFIX>-<DOMAIN>-<NNN>`
IDs are permanent, never reused, and human‑readable.

Examples:
- SE‑D‑001 (Demand Observation)
- SE‑D‑002 (Planning Scope)
- SE‑D‑003 (Enterprise Demand Picture)
- CA‑D‑001 (Understand Demand capability)
- CR‑D‑003 (Determine Planning Scope responsibility)
- VI‑R‑008 (Promise violation)
- AI‑D‑001 (Demand AI recommendation)

**Identity Rules:**
- IDs are permanent and never reused.
- IDs remain human‑readable.
- No two artifacts share the same ID.

## Traceability Rules

Every artifact must trace back to the Constitution (CN). Allowed downward dependencies:

| Artifact | Must Trace To |
|----------|---------------|
| SE | CN |
| CA | SE + CN |
| CR | CA |
| DE | CA + SE + CN |
| BR | CA (Capability that owns the Rule) |
| PO | BR (Rule it governs) |
| AB | SE (Aggregate Root) + CA |
| BW | AB + DE + BN + CA |
| FS | BW |
| BA | CA (or SE if tightly coupled) |
| EV | AB |
| BN | EV + CA |
| KA | CA |
| TE | BP | FS / AB / CODE |
| VI | FS + BR |
| AI | DE + BR + FS |

**Forbidden:** Upward references, layer skipping (e.g., a Decision may not reference a Policy directly; it references Rules that are governed by Policies).

>BP is a retired architectural concept and shall not appear in any artifact identifier, traceability chain, runtime provenance, or implementation reference.

> Telemetry shall trace to governed execution artifacts, Functional Specifications, Aggregate Behaviors, and implementation artifacts.

## Runtime Traceability

Every runtime decision or exception must produce a traceability chain:

```
VI‑R‑008 → FS‑R‑018 → BR‑S‑011 → DE‑R‑003 → CA‑R‑002 → SE‑R‑001 → CN‑004
```

This chain is embedded in `DecisionTraced` events and consumed by AI explainability.


## Lifecycle States for Architecture Artifacts

Every architecture artifact (Capability, Decision, Rule, etc.) has a lifecycle:

| State | Meaning |
|-------|---------|
| Draft | Under development; not yet authoritative. |
| Active | Authoritative; in use. |
| Deprecated | Still valid but planned for removal; no new dependencies. |
| Retired | No longer in use; retained for audit. |
| Replaced | Superseded by a new artifact (with traceability link to replacement). |

Transitions: Draft → Active → Deprecated → Retired (or Replaced). Replaced artifacts must reference the replacement ID.

## Architecture Evolution

New or changed artifacts must satisfy all traceability and dependency requirements before activation. Changes to Active artifacts require a new version or a replacement artifact (Replaced state). Replacement must preserve backward traceability.

## Knowledge Representation

All enterprise artifacts shall be expressed in a structured format suitable for machine reasoning. The textual specification remains authoritative; a derived machine‑readable representation (e.g., JSON‑LD, OWL) shall be auto‑generated and kept in sync.


# 20. AI Explainability

All AI‑generated explanations must derive from ARS traceability chains. The `DecisionTraced` event carries the full chain. AI agents consume these to produce human‑readable explanations.

# 21. Minimum Provenance Schemas

To satisfy constitutional “sufficiency” requirements, every runtime artifact that contributes to a significant business outcome shall carry at least the following provenance information. Domain specifications and implementations shall ensure these fields are populated.

### Decision Provenance
| Field | Description |
|-------|-------------|
| Decision ID | DE‑xxx identifier |
| Timestamp | When the decision was evaluated |
| Material Input IDs | SE‑xxx or KA‑xxx identifiers of inputs that influenced the outcome |
| Applied Rule IDs | BR‑xxx identifiers of Rules used |
| Outcome | The selected alternative |
| Accountable Capability | CA‑xxx identifier of the Capability that owns the decision |
| Traceability Chain | Full chain to CN (as recorded in DecisionTraced event) |
### Enterprise Event Provenance
| Field | Description |
|-------|-------------|
| Event Identity | Aggregate ID, Event Type, Occurrence Number |
| Publishing AB | AB‑xxx identifier |
| Timestamp | When the event was published |
| Causal Event IDs | Events that directly caused this event (if any) |
| Material Aggregate State Snapshot | Optional but recommended for audit |
### Violation Provenance
| Field | Description |
|-------|-------------|
| Violation ID | VI‑xxx identifier |
| Violated Rule ID | BR‑xxx identifier |
| Affected Semantic Object IDs | SE‑xxx identifiers |
| Detecting FS ID | FS‑xxx identifier |
| Timestamp | When the violation was detected |
| Traceability Chain | Full chain to CN |
### AI Recommendation Provenance
| Field | Description |
|-------|-------------|
| Recommendation ID | AI‑xxx identifier |
| Model/Business Algorithm ID | BA‑xxx identifier (or model reference) |
| Input Semantic Object / KA IDs | SE‑xxx, KA‑xxx |
| Confidence | Confidence metric provided by the model |
| Applied Guardrail Rule IDs | BR‑xxx identifiers that constrain the recommendation |
| Outcome | The recommended action |
| Traceability Chain | Full chain to CN |

### Business Notification Provenance
| Field | Description |
|-------|-------------|
| Notification ID | BN‑xxx identifier |
| Timestamp | When the notification publication occurred |
| Referenced Event Identities | Identity of each referenced Enterprise Event: Aggregate ID, Event Type, Occurrence Number |
| Publishing Functional Specification | FS‑xxx identifier of the Functional Specification that orchestrated publication |
| Owning Capability | CA‑xxx identifier of the Capability that owns the Business Notification |
| Declared Delivery Guarantee | Delivery, ordering, and timeliness guarantees declared for the notification |
| Traceability Chain | Full chain to CN |

# 22. Meta-Model Invariants (Well-Formedness Rules)

Every domain specification must satisfy these invariants:

| ID | Invariant |
|----|-----------|
| WF‑001 | Every Semantic Object has exactly one Business Owner (Capability). |
| WF‑002 | Every Entity has exactly one Lifecycle Owner (Aggregate Root). |
| WF‑003 | Every Aggregate Behavior modifies exactly one Aggregate Root. |
| WF‑004 | Every Business Transaction protects exactly one Aggregate Root. |
| WF‑005 | Every FS realises exactly one Capability Responsibility. |
| WF‑006 | Every Capability Responsibility is realised by exactly one BW and one FS. |
| WF‑007 | Every BW contains at least one Behavior Node. |
| WF‑008 | Every Decision references at least one Rule. |
| WF‑009 | Every BA has a version. |
| WF‑010 | Every BN references at least one EV. |
| WF‑011 | Every Rule belongs to exactly one category. |
| WF‑012 | Every Aggregate Root declares at least one Invariant Rule. |
| WF‑013 | Every Capability declares at least one Business Guarantee expressed as a verifiable condition over Semantic Object states or Enterprise Events. |
| WF‑014 | Every Semantic Object lifecycle is defined in exactly one place (Chapter 4). |
| WF‑015 | Every EV identity includes (Aggregate ID, Event Type, Occurrence Number). |
| WF‑016 | Every BN declares delivery, ordering, and timeliness guarantees. |
| WF‑017 | Every AB declares idempotency and concurrency guarantees. |
| WF‑018 | Every BA declares its algebraic properties. |
| WF‑019 | Every Business Intent is testable. |
| WF‑020 | All identifiers follow format `<PREFIX>-<DOMAIN>-<NNN>`. |
| WF‑021 | No two artifacts share the same identifier. |
| WF‑022 | Traceability chains never contain upward references. |
| WF‑023 | Before any Capability Responsibility may be designed, every Semantic Object referenced by its declared inputs shall exist in the Enterprise Semantic Model or the owning Domain Semantic Model and satisfy the Semantic Object Completeness Standard defined in that model. No capability design shall proceed with unresolved semantic references. |
| WF-024 | Every Business Notification is owned by exactly one Capability and published only through Business Workflow Notification Nodes. |
| WF-025 | A Notification Node may publish a Business Notification only after every Enterprise Event referenced by that Business Notification exists in the workflow execution context. |

## Anti-Patterns

The following are prohibited in any domain specification or implementation:

- Code creates business rules (rules must exist in the Rule Model).
- Missing traceability (every artifact must trace back to CN).
- Upward dependencies.
- Reusing identifiers.
- Violations without architecture references.
- Duplicating definitions across chapters.
- Aggregate Behavior modifying more than one Aggregate Root.
- Functional Specification redefining Rule, Decision, or Lifecycle semantics.
- Decision publishing Notifications.
- Business Algorithm changing lifecycle state.
- Aggregate Behaviour publishing or returning Business Notifications.
- Capability or Functional Specification defining Business Notification delivery guarantees outside the owning Capability contract.
- Notification Node publishing a Business Notification before all referenced Enterprise Events exist.
- Business Workflow publishing a Business Notification without referencing at least one Enterprise Event.

# 23. Compliance Rules

All domain specifications must:
1. Classify Semantic Objects per §3.
2. Structure FS per §9, including mandatory Business Contract.
3. Define one CR per BW, one FS per CR (1:1:1).
4. Never duplicate lifecycle definitions, rule text, or decision logic outside owning chapters.
5. Declare Business Intent for every Aggregate Root and Capability.
6. Separate EV from BN, with full delivery guarantees on BN.
7. Define Business Transactions within AB contracts.
8. State Business Guarantees in the Capability chapter.
9. Satisfy all Well‑Formedness Rules (§22) and violate no Anti‑Patterns (§22).
10. Declare algebraic properties on every BA.
11. Use approved identifiers (§19), no reuse.
12. Maintain full downward traceability (§19).
13. Business Notifications shall be owned by Capabilities and published only through Business Workflow Notification Nodes.
14. Aggregate Behaviours shall publish Enterprise Events only and shall not publish Business Notifications.

# Part 3 - ARS Design Principles & Language Governance

## Part 1 — Philosophy

ARS exists to solve a single problem: **enterprise specifications that describe software instead of specifying the enterprise.**

Most architecture frameworks eventually collapse business meaning, behavior, data, process, and technology into one document. Over time, these documents become inconsistent, implementation‑biased, and impossible to maintain as the system grows.

ARS separates these concerns permanently.

- **Business knowledge** (what exists, what is true, what choices exist, what governance applies) lives in Chapters 4–8 and 10.
- **Business execution** (how the enterprise orchestrates that knowledge) lives in Chapter 9.

This separation means a change to technology does not require rewriting the business specification. A change to business rules does not require redesigning the software architecture. Each evolves at its own pace, connected only through traceability.

ARS is not a documentation standard. It is a **specification language** — a formal grammar for describing enterprise behavior. Every domain specification is a valid program written in that language.


## Part 2 — Core Principles

### Principle 1 — Every artifact answers exactly one question.

| Artifact | Question |
|----------|----------|
| Semantic Object | What exists? |
| Capability | Who is responsible? |
| Decision | Which business choice exists? |
| Rule | What constraint exists? |
| Policy | What governance applies? |
| Aggregate Behavior | What state change occurs? |
| Business Workflow | In what order are behaviors orchestrated? |
| Functional Specification | How is the business orchestrated? |
| Business Algorithm | How is knowledge computed? |

When an artifact starts answering two questions, split it.

### Principle 2 — Model the enterprise, not the software.

Use business verbs: Determine, Revise, Publish — not Load, Update, Persist. The enterprise has no databases, repositories, or CRUD. Everything should read like a business operating model.

### Principle 3 — Prefer semantic precision over implementation detail.

Define what the enterprise means. Do not define how software realizes that meaning. Technologies change; enterprise meaning endures.

### Principle 4 — Separate information, behavior, governance, and computation.

These are orthogonal dimensions. Mixing them creates documents that break when any one dimension changes.

### Principle 5 — Traceability is mandatory.

Every artifact must trace back to the Constitution. Every artifact shall participate in an explicit traceability chain. Traceability enables governance, auditability, explainability, and automated validation.

### Principle 6 — Every enterprise concept has exactly one semantic owner.

Every other artifact references that owner rather than redefining the concept. This is the single most important law in ARS.

### Principle 7 — A Capability shall never directly modify Semantic Objects whose Lifecycle Owner is another Capability.

Cross‑capability collaboration occurs exclusively through published and consumed Business Notifications. The producer publishes information; the consumer owns the resulting state change.

### Principle 8 — Generated views are non‑normative.

Integration matrices, traceability maps, and dependency graphs are derived from the single sources of truth. They are never manually maintained.

### Principle 9 — Reference, never duplicate.

If a definition exists in one chapter, every other chapter references it by ID. Lifecycles are defined once. Rules are defined once. Decisions are defined once.

### Principle 10 — ARS evolves only through evidence from multiple domains.

A single domain's needs do not justify a language change. Three domains independently requiring the same concept provide sufficient evidence.


## Part 3 — Language Laws

### Law 1 — Single Semantic Owner

Every enterprise fact (a Semantic Object, a Rule, a Decision, a Business Guarantee) has exactly one owning artifact. Every other artifact references it by identifier. There is no second source of truth.

### Law 2 — Single Concern Per Artifact

No artifact may answer more than one of the canonical questions defined in Principle 1. Violations are corrected by splitting the artifact.

### Law 3 — Non‑Normative Derived Views

Any view that combines information from multiple artifacts (integration matrices, dependency graphs) is non‑normative. It is generated from traceability data, not manually authored. If it contradicts an owning artifact, the owning artifact wins.

### Law 4 — Reference By Identifier

Cross‑references use artifact identifiers exclusively (SE‑xxx, BR‑xxx, DE‑xxx, BN‑xxx, FS‑xxx, AB‑xxx, BA‑xxx). Natural‑language references are not sufficient.

### Law 5 — Lifecycle Definition Ownership

Every lifecycle is defined exactly once by its owning Semantic Object. Functional Specifications reference transitions by name only. State definitions are never duplicated.

### Law 6 — Cross‑Domain Ownership

A Capability shall never directly create, modify, or delete a Semantic Object owned by another Capability. All cross‑domain state changes occur when a Capability consumes a Business Notification and creates or modifies its own Semantic Objects in response.

### Law 7 — Well‑Formedness

Every domain specification must satisfy all WF‑xxx invariants defined in ARS v2 §29. A specification that violates any well‑formedness rule is invalid.


## Part 4 — Evolution Process

### Language Evolution Proposals (LEPs)

ARS changes follow a formal process modeled on PEPs and RFCs.

1. **Proposal:** A Language Evolution Proposal is created with Motivation, Problem, Current Limitation, Proposed Change, and Impact.
2. **Evidence Gathering:** The proposal is tested against the current domain. Evidence is recorded.
3. **Multi‑Domain Validation:** At least two additional domains independently demonstrate need for the same concept.
4. **Decision:** With evidence from three domains, the proposal is either Accepted (merged into ARS), Deferred (needs more evidence), or Rejected (does not generalize).

### Candidate Improvement Backlog

Proposals that have not yet met the multi‑domain threshold live in the Candidate Improvement Backlog. They are **not** part of ARS. Domain specifications may adopt them experimentally, but they remain provisional until validated.

| LEP | Concept | Evidence Count | Domains | Status |
|-----|---------|---------------|---------|--------|
| LEP‑001 | Enterprise Information Contract | 1 | Demand | Proposed |
| LEP‑002 | Ontology Classification | 1 | Demand | Proposed |
| LEP‑003 | Enterprise Information Flow | 1 | Demand | Proposed |
| LEP‑004 | Ownership Vocabulary Standardization | 1 | Demand | Proposed |


No changes are required to the Constitution. The ratified architecture remains fully compliant with all Constitutional articles.

The ARS requires the following patches to incorporate the new governance rules and patterns that underpin the Enterprise Semantic Model.

---

# Appendix A – Enterprise Semantic Patterns

The following architectural patterns are compositions of the Common Specification Contracts. They are owned by the ARS and referenced by the Enterprise Semantic Model.

**A.1 Snapshot Pattern**

A Snapshot Semantic Object captures a point‑in‑time representation of enterprise facts authoritatively owned by other Semantic Objects. It applies the Snapshot Ownership Principle (§3.4), the Versioned Lifecycle pattern (Draft → Published → Superseded), the Authority Specification Contract, and the Consumer Specification Contract. Its Information Model shall reference the authoritative source of every captured fact.

**A.2 Continuous Assessment Pattern**

A Continuous Assessment is an Aggregate Root that maintains a continuously current interpretation of enterprise state. It applies the Authority Specification Contract, the Consumer Specification Contract, and a Lifecycle of Active → Archived. It is updated by a single Aggregate Behavior triggered by changes to its source facts. History is preserved as immutable change events.

**A.3 Published Knowledge Pattern**

A Published Knowledge object is a versioned, periodically published assessment for management oversight. It applies the Versioned Lifecycle, the Authority Specification Contract, and the Consumer Specification Contract. Each version is immutable; the previous version is superseded on publication.

**A.4 Observation Pattern**

An Observation is an immutable record of an enterprise fact as it was received. It applies the Authority Specification Contract and a Lifecycle of Received → Evaluated (Accepted / Quarantined / Rejected). It captures provenance, business time, and observation time.

**A.5 Pattern Applicability Table**

| Pattern | Example Objects |
|---------|-----------------|
| Snapshot | Enterprise Supply Picture, Enterprise Demand Picture, Supply Plan |
| Continuous Assessment | Inventory Position Assessment, Capacity Position Assessment, Supplier Commitment Assessment |
| Published Knowledge | Inventory Health Assessment, Capacity Health Assessment, Supply Quality Assessment |
| Observation | Supply Data Record, Demand Observation |
