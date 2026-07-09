
# Specification Meta-Model & Platform Governance

**Status:** Authoritative  
**Scope:** Entire Medhavi APS ecosystem   
**Traceability:** CN‑001, CN‑TR‑001, CN‑EX‑001  

---

# Part 1 — Specification Meta-Model

## 1. Purpose

This document defines the formal language for describing every business capability, semantic object, decision, rule, policy, workflow, functional specification, event, notification, and algorithm within Medhavi APS. It also establishes enterprise-wide identifier standards, traceability rules, artifact lifecycles, and governance policies.

The meta-model is the single contract that all domain specifications must honour.

## 2. Fundamental Concepts

| Concept | Definition | Prefix | Cardinality Relationships |
|---------|------------|--------|---------------------------|
| Semantic Object | A named business thing with identity, attributes, relationships, and lifecycle. | SE | Owned by exactly one Capability. |
| Aggregate Root | The smallest business concept whose invariants must always hold together; defines a consistency boundary. | SE (subset) | Owns 0..* Entities and Value Objects. Modified by 0..* Aggregate Behaviours. |
| Entity | A Semantic Object with local identity, existing only within an Aggregate Root. | SE | Owned by exactly one Aggregate Root. |
| Value Object | Immutable, no identity, defined solely by attributes. | SE (or inline) | Used by any object; owned by none. |
| Reference Object | A Semantic Object owned by a different domain, referenced by identity only. | SE | Read by any object; never modified locally. |
| Knowledge Artifact | A named, versioned, traceable piece of business knowledge with confidence, evidence, and expiry. | KA | Owned by exactly one Capability. Referenced by Decisions and Algorithms. |
| Business Intent | The enterprise promise an object, capability, or workflow exists to uphold. | (none) | Declared by every Aggregate Root and Capability. |
| Aggregate Behaviour | A reusable unit of business logic that changes exactly one Aggregate Root, following a formal contract. | AB | Owned by exactly one Aggregate Root. Invokes 0..* Decisions, 0..* Algorithms. Publishes 0..* Events. |
| Business Workflow | A directed acyclic graph of nodes (Aggregate Behaviours, Decisions, Notifications) achieving a business outcome. | BW | Realises exactly one Capability Responsibility. Contains 1..* nodes. |
| Capability | A named business responsibility owning Semantic Objects, Decisions, Rules, Policies, Workflows, and Guarantees. | CA | Owns 1..* Capability Responsibilities. |
| Capability Responsibility | A single, cohesive business outcome within a capability, realised by exactly one Workflow. | CR | Realised by exactly one Business Workflow (1:1:1). |
| Functional Specification | Executable specification of a Business Workflow. Orchestrates ABs, Decisions, and Notifications. | FS | Realises exactly one Capability Responsibility. |
| Business Transaction | The unit of atomic commitment protecting exactly one Aggregate Root during an Aggregate Behaviour. | (part of AB) | Owned by exactly one Aggregate Behaviour. |
| Business Guarantee | Enterprise-level invariant a capability promises across all its workflows. | (in CA) | Declared by exactly one Capability. |
| Decision | A business choice producing exactly one outcome from a defined set of alternatives. | DE | Owned by exactly one Aggregate Behaviour (Decision Owner). References 1..* Rules. |
| Rule | A reusable constraint categorised as Identity, Eligibility, Invariant, Behaviour, or Derivation. | BR | Owned by exactly one Capability. Referenced by Decisions, Preconditions, and Algorithms. |
| Policy | Governance action when a Rule cannot be satisfied or an exceptional situation occurs. | PO | Owned by exactly one Capability. Governs 0..* Rules. |
| Enterprise Event | Immutable record of a business fact. Identity = (Aggregate ID, Event Type, Occurrence Number). | EV | Published by exactly one Aggregate Behaviour. Consumed by 0..* Workflows and Notifications. |
| Business Notification | Directed communication to known consumers, with defined delivery guarantees. | BN | Published by exactly one Capability. References 1..* Enterprise Events. |
| Business Algorithm | A named, versioned, traceable computation with declared algebraic properties. | BA | Owned by exactly one Capability. Invoked by 0..* Aggregate Behaviours. |

## 3. Semantic Model Architecture

### 3.1 Object Classification
Every Semantic Object belongs to exactly one classification:
- **Aggregate Root:** The smallest business concept whose invariants must always hold together. Protected by a Business Transaction.
- **Entity:** Has local identity within an Aggregate Root. Cannot exist independently.
- **Value Object:** No identity. Immutable, defined entirely by attributes. Two Value Objects are equal if all attributes are equal.
- **Reference Object:** Owned by a different domain. Referenced by identity only; never created, modified, or lifecycle-defined locally.

### 3.2 Lifecycle Specification
Every Aggregate Root and Entity with meaningful states must define its lifecycle in the Semantic Model (Chapter 4). The lifecycle includes:
- All possible states
- Permitted transitions with triggers
- Invariants that hold in each state
- Initial and terminal states

**Workflow Lifecycles** define execution states for Business Workflows: **Triggered**, **Running**, **Completed**, **Failed**, **Compensating**. These are defined alongside object lifecycles.

Functional Specifications reference object lifecycle transitions by name only (e.g., "Transition SE‑D‑001 from Received to Accepted"). They must not redefine states or transition rules.

### 3.3 Knowledge Artifact (Provisional)
A Knowledge Artifact is a named, versioned, traceable piece of business knowledge. It carries:
- Confidence (quantitative reliability measure)
- Evidence trail (derivation trace)
- Expiry (when re‑evaluation is needed)

Owned by a Capability. Full definition deferred to Knowledge Intelligence; this placeholder ensures forward compatibility.

## 4. Business Intent

Every Aggregate Root, Capability, and Business Workflow must declare its Business Intent — the enterprise promise it upholds. Intent answers *why* the enterprise needs this thing, not what it contains.

**Constraints:**
- Must never describe implementation, process, ownership, or lifecycle.
- Must only state what the enterprise can rely on because this thing exists.
- Must be testable (verifiable by observing enterprise state or events).

Example for Aggregate Root **Enterprise Demand Picture**: "Provide exactly one authoritative planning interpretation for a Planning Scope at any point in time."

## 5. Aggregate Behaviour Contract

Every Aggregate Behaviour must satisfy a formal contract:

| Section | Description |
|---------|-------------|
| Purpose | Business action performed. |
| Business Intent | Enterprise promise upheld. |
| Owned Aggregate | Exactly one Aggregate Root whose state is changed. |
| Required Input State | Lifecycle state(s) the aggregate must be in before execution. |
| Produced Output State | Lifecycle state(s) the aggregate transitions to upon success. |
| Invoked Decisions | 0..* Decisions consulted. |
| Invoked Algorithms | 0..* Business Algorithms executed. |
| Published Events | 0..* Enterprise Events published upon success. |
| Business Transaction | Exactly one, protecting the Owned Aggregate (see §6). |
| Idempotency Guarantee | Whether re‑execution with same inputs produces same outcome without duplicate effects. |
| Concurrency Guarantee | Business promise about ordering and isolation for concurrent invocations on the same aggregate. |

Aggregate Behaviour is owned by the Aggregate Root (Lifecycle Owner) and documented in the Semantic Model or a dedicated Aggregate Behaviour catalogue. Functional Specifications invoke Aggregate Behaviours by name; they must not inline behaviour logic.

## 6. Business Transaction

A Business Transaction is the unit of atomic commitment protecting exactly one Aggregate Root during an Aggregate Behaviour.

- **Scope:** Exactly one aggregate. Cross‑aggregate workflows use multiple transactions; consistency across aggregates is eventual.
- **Semantics:**
  - **Atomicity:** All state changes to the aggregate are applied together or not at all.
  - **Consistency:** Aggregate invariants are enforced at transaction boundaries.
  - **Isolation:** Concurrent transactions on the same aggregate are serialized based on business order.
  - **Durability:** Once committed, the aggregate state change is permanent.
- **Location:** The Business Transaction definition belongs to the Aggregate Behaviour contract. A Functional Specification references it; it never redefines it.

## 7. Business Workflow

A Business Workflow is a directed acyclic graph (DAG) of nodes achieving a business outcome.

**Node Types:**
| Node | Description |
|------|-------------|
| Behaviour Node | Invokes an Aggregate Behaviour. |
| Decision Node | Evaluates a Decision and branches on its outcome. |
| Notification Node | Publishes a Business Notification. |
| Fork Node | Initiates parallel execution of multiple branches. |
| Join Node | Waits for all incoming parallel branches before continuing. |
| Start Node | Receives the triggering Enterprise Event or scheduled time. |
| End Node | Marks workflow completion (success or failure). |

**Transition Semantics:** An edge A → B means:
- If A is a Behaviour Node: B executes after A commits successfully.
- If A is a Decision Node: B executes after A's outcome is determined, following the branch for that outcome.
- If A is a Notification Node: B executes after A publishes.

- **Trigger:** An Enterprise Event or a scheduled time, received at the Start Node.
- **Scope:** May span multiple aggregates, but each aggregate is modified only by its own Aggregate Behaviours.
- **Lifecycle:** Triggered → Running → Completed / Failed / Compensating.
- **Mapping:** 1:1:1 — Capability Responsibility → Business Workflow → Functional Specification.

## 8. Capability and Capability Responsibility

A **Capability** is a named business responsibility that owns Semantic Objects, Decisions, Rules, Policies, Workflows, and Business Guarantees.

- **Business Intent:** Declared; the enterprise promise the capability upholds.
- **Business Guarantees:** Declared in the Capability chapter; cross‑workflow invariants.
- **Capability Responsibility:** A single, cohesive business outcome.
  - **Granularity Principle:** One responsibility protects one aggregate consistency outcome. If it modifies two aggregates independently, split it.
  - **Typing (Guidance):** Common patterns include Capture, Evaluate, Transform, Publish, Govern. Formal taxonomy deferred until sufficient domains exist.

## 9. Functional Specification

The executable specification of a Business Workflow. Structure:

1. **Business Contract** — Consumes, Produces, Transitions, Publishes, Invokes, Guarantees (compact summary).
2. **Trigger** — Event or schedule.
3. **Preconditions** — Table of Eligibility Rule IDs with brief labels.
4. **Semantic Objects** — Read, Create, Update, Archive (reference IDs).
5. **Behaviour** — Sequence of steps, each invoking an AB, Decision, Notification, or workflow control node. Uses declarative business verbs (Establish, Determine, Assign, Calculate, Supersede, Publish). No validation logic (in Preconditions/Decisions), no data manipulation (in ABs).
6. **Business Transaction** — Reference to the Aggregate Behaviour contract(s) governing this workflow.
7. **Postconditions** — Guarantees upon success.
8. **Failure Behaviour** — Resulting business state (not software exceptions).
9. **Recovery Behaviour** — Idempotency and safe re‑execution.
10. **Concurrency Guarantees** — Business promises, not technical locks.
11. **Example** — Worked example.

## 10. Business Guarantee

Enterprise‑level invariant upheld by a capability across all its workflows. Testable, verifiable by observing aggregate states and events. Declared in the Capability chapter.

## 11. Decision

A business choice producing exactly one outcome from a predefined set of alternatives. Owned by the Aggregate Behaviour that invokes it (Decision Owner).

Structure: Purpose, Trigger, Inputs, Alternatives, Criteria (referencing Rules), Confidence, Rationale Template.

Defined in the Decision Model chapter; invoked by name in Functional Specifications.

## 12. Rules

Five categories with distinct enforcement points:

| Category | Enforcement Point |
|----------|-------------------|
| Identity Rule | At object creation. |
| Eligibility Rule | Preconditions of Functional Specifications. |
| Invariant Rule | At every aggregate commit boundary. |
| Behaviour Rule | During Decision evaluation. |
| Derivation Rule | At Algorithm execution time. |

Rules are defined once in the Rule Model chapter with unique IDs (BR‑xxx). Referenced by ID everywhere else; never restated.

## 13. Policy

Governance action when a Rule cannot be satisfied or an exceptional situation occurs. Categories: Authorization, Compliance, Automation, Exception, Audit.

Defined in Policy Model chapter; referenced by Functional Specifications.

## 14. Enterprise Events and Business Notifications

**Enterprise Event (EV):** Immutable business fact. Identity = (Aggregate ID, Event Type, Occurrence Number). Occurrence Number is monotonically increasing per aggregate. Published by an Aggregate Behaviour.

**Business Notification (BN):** Directed communication to known consumers. Mandatory delivery guarantees:
- **Delivery:** at‑least‑once, at‑most‑once, or exactly‑once.
- **Ordering:** per aggregate, per type, or none.
- **Timeliness:** real‑time, near‑real‑time, batch.

A Notification references the underlying Enterprise Event(s) but defines the contract with consumers. Notifications are defined in the Capability chapter.

## 15. Business Algorithm

Named, versioned, traceable computation. Declares algebraic properties:

| Property | Definition |
|----------|------------|
| Deterministic | Same inputs always produce same outputs. |
| Idempotent | Re‑execution produces no additional effects. |
| Pure | No side effects; computes only from inputs. |
| Order Sensitive | Whether input order affects output. |
| Explainable | Can produce human‑readable computation trace. |

Defined in Business Algorithm catalogue. Invoked by name from Aggregate Behaviours or Functional Specifications.

## 16. Ownership

Three ownership dimensions:
- **Business Owner:** The Capability responsible for meaning and quality.
- **Lifecycle Owner:** The Aggregate Root (or Capability) controlling state transitions and enforcing invariants.
- **Decision Owner:** The Aggregate Behaviour that invokes a Decision.

Additionally, **Information Consumers** (capabilities that read an object) are documented to show cross‑domain dependencies.

## 17. Document Architecture

Every domain specification follows this fixed structure:

| Chapter | Content |
|---------|---------|
| 1 | Purpose & Scope |
| 2 | Business Objectives |
| 3 | Enterprise Measures |
| 4 | Semantic Model (Aggregate Roots, Entities, Value Objects, Reference Objects, Knowledge Artifacts; lifecycles, invariants, intents) |
| 5 | Capability Model (Capabilities, Responsibilities, Guarantees, Notifications) |
| 6 | Decision Model |
| 7 | Rule Model (categorised) |
| 8 | Policy Model |
| 9 | Functional Specifications (one per Capability Responsibility) |
| 10 | Business Algorithms |

**Cross‑referencing rules:**
- Never duplicate a definition. Chapters 4–8 and 10 are the single source of truth.
- Reference by ID only (SE‑xxx, BR‑xxx, PO‑xxx, DE‑xxx, BN‑xxx, EV‑xxx, BA‑xxx, AB‑xxx).
- Lifecycles are defined in Chapter 4; FSs reference transition names only.

---

# Part 2 — Platform Governance

## 18. Architecture Layer Model

```
CN → SE → CA → DE → BR → PO → AB → BW → FS → BA → BP → CODE → TE / VI / AI
```

| Layer | Description |
|-------|-------------|
| CN | Constitution — enterprise principles |
| SE | Semantic Model — business objects |
| CA | Capability — business responsibilities |
| DE | Decision — business choices |
| BR | Business Rule — constraints |
| PO | Policy — governance |
| AB | Aggregate Behaviour — atomic business logic |
| BW | Business Workflow — orchestration |
| FS | Functional Specification — executable spec |
| BA | Business Algorithm — computation |
| BP | Blueprint — technology design |
| CODE | Implementation |
| TE | Telemetry — observability |
| VI | Violation — exception detection |
| AI | AI Recommendation / Explanation |

## 19. Artifact Catalog

| Prefix | Artifact Type |
|--------|---------------|
| CN | Constitution |
| SE | Semantic Object |
| CA | Capability |
| CR | Capability Responsibility |
| DE | Decision |
| BR | Business Rule |
| PO | Policy |
| AB | Aggregate Behaviour |
| BW | Business Workflow |
| FS | Functional Specification |
| BA | Business Algorithm |
| EV | Enterprise Event |
| BN | Business Notification |
| KA | Knowledge Artifact |
| BO | Business Objective |
| PI | Performance Indicator |
| TE | Telemetry |
| VI | Violation |
| AI | AI Recommendation / Explanation |

## 20. Domain Catalog

| Code | Domain |
|------|--------|
| C | Core (enterprise‑wide, meta‑model) |
| D | Demand Intelligence |
| S | Supply Intelligence |
| R | Promise Intelligence |
| N | Scenario Intelligence |
| K | Knowledge Intelligence |
| A | AI (cross‑cutting AI agents, copilot) |
| O | Operations (monitoring, infrastructure) |

**Note:** "A" is reserved for AI‑specific runtime artifacts (copilot suggestions, agent actions). Knowledge Intelligence domain uses "K". All domain specifications (Demand, Supply, Promise, Scenario, Knowledge) follow this meta‑model.

## 21. Identifier Standard

**Format:** `<PREFIX>-<DOMAIN>-<NNN>`

Examples:
- SE‑D‑001 (Demand Observation)
- SE‑D‑002 (Planning Scope)
- SE‑D‑003 (Enterprise Demand Picture)
- CA‑D‑001 (Understand Demand capability)
- CR‑D‑003 (Determine Planning Scope responsibility)
- DE‑D‑010 (Accept Demand Observation decision)
- BR‑D‑025 (Planning Scope identity rule)
- PO‑D‑006 (Quarantine policy)
- AB‑D‑001 (Establish Demand Observation behaviour)
- BW‑D‑003 (Planning Scope Determination workflow)
- FS‑D‑003 (Determine Planning Scope functional spec)
- BA‑D‑001 (Calculate Planning Demand algorithm)
- EV‑D‑001 (EnterpriseDemandPicturePublished event)
- BN‑D‑001 (Enterprise Demand Picture Published notification)
- KA‑D‑001 (Forecast Confidence Index knowledge artifact)
- VI‑R‑008 (Promise violation)
- AI‑D‑001 (Demand AI recommendation)

**Identity Rules:**
- IDs are permanent and never reused.
- IDs remain human‑readable.
- No two artifacts share the same ID.

## 22. Traceability Rules

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
| TE | BP / CODE |
| VI | FS + BR |
| AI | DE + BR + FS |

**Forbidden:** Upward references, layer skipping (e.g., a Decision may not reference a Policy directly; it references Rules that are governed by Policies).

## 23. Runtime Traceability

Every runtime decision or exception must produce a traceability chain:

```
VI‑R‑008 → FS‑R‑018 → BR‑S‑011 → DE‑R‑003 → CA‑R‑002 → SE‑R‑001 → CN‑004
```

This chain is embedded in `DecisionTraced` events and consumed by AI explainability.

## 24. AI Explainability

All AI‑generated explanations must derive from ARS traceability chains. The `DecisionTraced` event carries the full chain. AI agents consume these to produce human‑readable explanations.

## 25. Lifecycle States for Architecture Artifacts

Every architecture artifact (Capability, Decision, Rule, etc.) has a lifecycle:

| State | Meaning |
|-------|---------|
| Draft | Under development; not yet authoritative. |
| Active | Authoritative; in use. |
| Deprecated | Still valid but planned for removal; no new dependencies. |
| Retired | No longer in use; retained for audit. |
| Replaced | Superseded by a new artifact (with traceability link to replacement). |

Transitions: Draft → Active → Deprecated → Retired (or Replaced). Replaced artifacts must reference the replacement ID.

## 26. Architecture Evolution

New or changed artifacts must satisfy all traceability and dependency requirements before activation. Changes to Active artifacts require a new version or a replacement artifact (Replaced state). Replacement must preserve backward traceability.

## 27. Knowledge Representation

All enterprise artifacts shall be expressed in a structured format suitable for machine reasoning. The textual specification remains authoritative; a derived machine‑readable representation (e.g., JSON‑LD, OWL) shall be auto‑generated and kept in sync.

## 28. Anti-Patterns

The following are prohibited in any domain specification or implementation:

- Code creates business rules (rules must exist in the Rule Model).
- Missing traceability (every artifact must trace back to CN).
- Upward dependencies.
- Reusing identifiers.
- Violations without architecture references.
- Duplicating definitions across chapters.
- Aggregate Behaviour modifying more than one Aggregate Root.
- Functional Specification redefining Rule, Decision, or Lifecycle semantics.
- Decision publishing Notifications.
- Business Algorithm changing lifecycle state.

## 29. Meta-Model Invariants (Well-Formedness Rules)

Every domain specification must satisfy these invariants:

| ID | Invariant |
|----|-----------|
| WF‑001 | Every Semantic Object has exactly one Business Owner (Capability). |
| WF‑002 | Every Entity has exactly one Lifecycle Owner (Aggregate Root). |
| WF‑003 | Every Aggregate Behaviour modifies exactly one Aggregate Root. |
| WF‑004 | Every Business Transaction protects exactly one Aggregate Root. |
| WF‑005 | Every FS realises exactly one Capability Responsibility. |
| WF‑006 | Every Capability Responsibility is realised by exactly one BW and one FS. |
| WF‑007 | Every BW contains at least one Behaviour Node. |
| WF‑008 | Every Decision references at least one Rule. |
| WF‑009 | Every BA has a version. |
| WF‑010 | Every BN references at least one EV. |
| WF‑011 | Every Rule belongs to exactly one category. |
| WF‑012 | Every Aggregate Root declares at least one Invariant Rule. |
| WF‑013 | Every Capability declares at least one Business Guarantee. |
| WF‑014 | Every Semantic Object lifecycle is defined in exactly one place (Chapter 4). |
| WF‑015 | Every EV identity includes (Aggregate ID, Event Type, Occurrence Number). |
| WF‑016 | Every BN declares delivery, ordering, and timeliness guarantees. |
| WF‑017 | Every AB declares idempotency and concurrency guarantees. |
| WF‑018 | Every BA declares its algebraic properties. |
| WF‑019 | Every Business Intent is testable. |
| WF‑020 | All identifiers follow format `<PREFIX>-<DOMAIN>-<NNN>`. |
| WF‑021 | No two artifacts share the same identifier. |
| WF‑022 | Traceability chains never contain upward references. |

## 30. Compliance Rules

All domain specifications must:
1. Classify Semantic Objects per §3.
2. Structure FS per §9, including mandatory Business Contract.
3. Define one CR per BW, one FS per CR (1:1:1).
4. Never duplicate lifecycle definitions, rule text, or decision logic outside owning chapters.
5. Declare Business Intent for every Aggregate Root and Capability.
6. Separate EV from BN, with full delivery guarantees on BN.
7. Define Business Transactions within AB contracts.
8. State Business Guarantees in the Capability chapter.
9. Satisfy all Well‑Formedness Rules (§29) and violate no Anti‑Patterns (§28).
10. Declare algebraic properties on every BA.
11. Use approved identifiers (§21), no reuse.
12. Maintain full downward traceability (§22).



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
| Aggregate Behaviour | What state change occurs? |
| Business Workflow | In what order are behaviours orchestrated? |
| Functional Specification | How is the business orchestrated? |
| Business Algorithm | How is knowledge computed? |

When an artifact starts answering two questions, split it.

### Principle 2 — Model the enterprise, not the software.

Use business verbs: Determine, Revise, Publish — not Load, Update, Persist. The enterprise has no databases, repositories, or CRUD. Everything should read like a business operating model.

### Principle 3 — Prefer semantic precision over implementation detail.

Define what the enterprise means. Do not define how software realizes that meaning. Technologies change; enterprise meaning endures.

### Principle 4 — Separate information, behaviour, governance, and computation.

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
| LEP‑005 | Cross‑Domain Ownership Principle | 1 | Demand | Proposed |