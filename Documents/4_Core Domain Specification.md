# Core Domain Specification

Status: Governance Review Draft — Revised  
Domain Code: C  
Governed By: Specification Meta-Model & Platform Governance (ARS)  
Traceability: CN-001, CN-002, CN-003, CN-004, CN-005, CN-006, CN-008, CN-009, CN-010, CN-012, ARS §2, §3, §4, §5, §6, §7, §8, §9, §10, §15, §16, §17, §18, §19, §22, Enterprise Semantic Model, Specification Meta-Model & Platform Governance

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Core Intelligence is the authoritative enterprise domain responsible for maintaining the foundational enterprise infrastructure that all Intelligence domains depend upon.

It answers the enterprise questions:

1. What is the current, authoritative state of enterprise reality for a given Planning Scope?
2. Which enterprise constraints are currently breached, what is their severity, and what is their resolution state?

Core Intelligence does not perform domain-specific planning, forecasting, promising, or scenario evaluation. It provides the enterprise infrastructure — authoritative snapshots and centralized exception lifecycle management — that all Intelligence domains consume.

## 1.2 Scope

**Included:**

- Enterprise Picture composition.
- Enterprise Picture publication.
- Centralized Exception lifecycle management.

**Excluded:**

- Domain-specific planning activities.
- Domain-specific forecasting activities.
- Domain-specific promise activities.
- Domain-specific scenario activities.
- Domain-specific exception detection logic.
- Operational execution.
- Financial accounting.
- Master data creation.
- Enterprise reference data governance, unless a separate ratified Capability specification defines that responsibility.

## 1.3 Responsibility Boundary

The responsibility of Core Intelligence begins when:

- a governed schedule triggers Enterprise Picture composition; or
- an authorized Intelligence domain publishes exception detection evidence; or
- an authorized Intelligence domain publishes exception resolution evidence.

The responsibility of Core Intelligence ends when:

- an Enterprise Picture Version is published; or
- a no-material-change publication evaluation completes without publication; or
- an Exception lifecycle transition is completed; or
- exception evidence is rejected with a governed business failure reason.

Core Intelligence does not evaluate whether an exception should be detected. That responsibility belongs to the Intelligence domain that owns the underlying data. Core Intelligence only manages the lifecycle of exceptions based on domain-published evidence.

## 1.4 Architectural Position

Core Intelligence is a domain specification.

It derives its authority from:

- the Constitution;
- the Architecture Reference Standard;
- the Enterprise Semantic Model.

Core Intelligence does not redefine enterprise semantic objects owned by the Enterprise Semantic Model.

Every enterprise concept consumed is referenced by its Enterprise Semantic Object identifier.

## 1.5 Out of Scope

The following are out of scope for this specification:

- Domain-specific planning.
- Domain-specific forecasting.
- Domain-specific promising.
- Domain-specific scenario evaluation.
- Domain-specific exception detection logic.
- Operational execution.
- Financial accounting.
- Master data creation.
- Enterprise reference data governance, unless separately ratified.

## 1.6 Enterprise Questions

| ID | Enterprise Question | Capability |
|---|---|---|
| `EQ-C-001` | What is the current, authoritative state of enterprise reality for a given Planning Scope? | `CA-C-019 Enterprise Picture Management` |
| `EQ-C-002` | Which enterprise constraints are currently breached, what is their severity, and what is their resolution state? | `CA-C-020 Core Exception Management` |

## 1.7 Core Intelligence Pipeline

```text
Enterprise State
(Demand, Supply, Inventory)
        |
        v
Scheduled Enterprise Picture Composition
        |
        v
EV-C-001 Enterprise Picture Version Composed
        |
        v
Publication Materiality Evaluation
        |
        +--> No Material Change: no publication
        |
        +--> Material Change
                 |
                 v
        Enterprise Picture Publication
                 |
                 v
        EV-C-002 Enterprise Picture Version Published
                 |
                 v
        BN-C-001 Enterprise Picture Published
                 |
                 v
        Declared consuming domains

Authorized Exception Detection Evidence
(BN-D-022)
        |
        v
Core Exception Management
        |
        v
SE-C-019 Exception Active or Updated
        |
        v
EV-C-003 / EV-C-004
        |
        v
BN-C-002 Enterprise Exception Active

Authorized Exception Resolution Evidence
(BN-D-023)
        |
        v
Core Exception Management
        |
        v
SE-C-019 Exception Resolved
        |
        v
EV-C-005 Enterprise Exception Resolved
        |
        v
BN-C-003 Enterprise Exception Resolved
```

## 1.8 Traceability

| Artifact | Reference |
|---|---|
| Constitution | `CN-001`, `CN-002`, `CN-003`, `CN-004`, `CN-005`, `CN-006`, `CN-008`, `CN-009`, `CN-010`, `CN-012` |
| Architecture Reference Standard | §2, §3, §4, §5, §6, §7, §8, §9, §10, §15, §16, §17, §18, §19, §22 |
| Enterprise Semantic Model | `SE-C-010`, `SE-C-013`, `SE-C-014`, `SE-C-015`, `SE-C-019`, `SE-C-021`, `SE-C-022`, `SE-C-036`, `SE-C-037`, `SE-C-038` |

---

# Chapter 2 — Business Objectives

| ID | Objective | Traceability |
|---|---|---|
| `BO-C-001` | Maintain Authoritative Enterprise Reality | `CN-003`, `CN-004`, `CN-006` |
| `BO-C-002` | Maintain Centralized Exception Registry | `CN-003`, `CN-004`, `CN-009` |

## BO-C-001 — Maintain Authoritative Enterprise Reality

**Statement:**

The enterprise shall possess a single, governed, immutable snapshot of enterprise reality for every active Planning Scope, ensuring all downstream planning capabilities reason over a consistent, synchronized baseline.

**Rationale:**

Without a single authoritative picture of enterprise reality, downstream planning capabilities would reason over inconsistent, stale, or conflicting data. The Enterprise Picture provides the single source of truth for demand, supply, and inventory within a governed Planning Scope.

**Measures:**

- `PI-C-001 Picture Publication Latency`.

**Suspended Measure:**

- `PI-C-002 Picture Completeness` is not implementable until an authoritative Expected Reference Set definition is ratified.

## BO-C-002 — Maintain Centralized Exception Registry

**Statement:**

The enterprise shall possess a single, centralized registry of all unsatisfied enterprise constraints, providing unified lifecycle management, severity assessment, and deduplication across authorized Intelligence domains.

**Rationale:**

Without a centralized exception registry, each Intelligence domain would maintain its own exception tracking, leading to fragmented exception awareness, inconsistent severity assessment, and duplicate exception records.

**Measures:**

- `PI-C-003 Exception Deduplication Rate`.
- `PI-C-004 Exception Resolution Latency`.

---

# Chapter 3 — Enterprise Measurement Model

Every Performance Indicator in this chapter is a governed instance of `SE-C-036 Performance Indicator`.

Measured values are published as Knowledge Artifacts owned by the relevant Core Capability.

Concrete threshold values do not appear in Performance Indicator definitions. They are owned by the referenced Policies.

## PI-C-001 — Picture Publication Latency

| Attribute | Value |
|---|---|
| Identifier | `PI-C-001` |
| Name | Picture Publication Latency |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | `CA-C-019 Enterprise Picture Management` |
| Governed By | `PO-C-001` |
| Enterprise Question | How long does it take to publish an Enterprise Picture after the governed composition trigger? |
| Business Objectives Served | `BO-C-001` |
| Formula | `PicturePublicationLatency = PublicationTime - CompositionTriggerTime` |
| Semantic Dependencies | `SE-C-021 PictureVersion.Publication Time`; `EV-C-001 Composition Trigger Time` |
| Measurement Artifact | `KA-C-001` |
| Applicability | Applies only when a Published `PictureVersion` exists. |
| Missing Evidence Handling | If Publication Time or Composition Trigger Time is absent, the measured value is not produced. |

## PI-C-002 — Picture Completeness

| Attribute | Value |
|---|---|
| Identifier | `PI-C-002` |
| Name | Picture Completeness |
| Status | Not implementable. |
| Reason | The enterprise has not defined an authoritative Expected Reference Set. |
| Required Governance Artifact | An authoritative Expected Reference Set definition for each Planning Scope. |
| Prohibition | No implementation shall compute, estimate, infer, or enforce Picture Completeness until the Required Governance Artifact is ratified. |
| Policy Effect | The completeness requirement in `PO-C-001` is not enforceable. |

## PI-C-003 — Exception Deduplication Rate

| Attribute | Value |
|---|---|
| Identifier | `PI-C-003` |
| Name | Exception Deduplication Rate |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | `CA-C-020 Core Exception Management` |
| Governed By | `PO-C-002` |
| Enterprise Question | What proportion of valid exception detection evidence results in deduplication rather than duplicate exception creation? |
| Business Objectives Served | `BO-C-002` |
| Formula | `ExceptionDeduplicationRate = count(EV-C-004) / (count(EV-C-003) + count(EV-C-004))` |
| Measurement Window | Defined by the measurement Knowledge Artifact version. |
| Semantic Dependencies | `EV-C-003 Enterprise Exception Activated`; `EV-C-004 Enterprise Exception Updated` |
| Measurement Artifact | `KA-C-003` |
| Missing Evidence Handling | If the denominator is zero, the indicator value is Not Applicable. |

## PI-C-004 — Exception Resolution Latency

| Attribute | Value |
|---|---|
| Identifier | `PI-C-004` |
| Name | Exception Resolution Latency |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | `CA-C-020 Core Exception Management` |
| Governed By | `PO-C-002` |
| Enterprise Question | How long does it take for an active exception to transition to Resolved after resolution evidence is received? |
| Business Objectives Served | `BO-C-002` |
| Formula | `ExceptionResolutionLatency = ResolutionTime - ActivationTime` |
| ActivationTime | Occurrence time of `EV-C-003` for the resolved Exception. |
| Semantic Dependencies | `SE-C-019 Exception` resolution evidence; `EV-C-003 Enterprise Exception Activated`; `EV-C-005 Enterprise Exception Resolved` |
| Measurement Artifact | `KA-C-004` |
| Missing Evidence Handling | If no activation event exists, the measured value is not produced. |


# Chapter 4 — Core Domain Semantic Model

## 4.1 Domain Semantic Principles

### 4.1.1 Enterprise Temporal Semantics

| Temporal Dimension | Business Meaning |
|---|---|
| Business Time | When an enterprise fact occurred in reality. |
| Observation Time | When the enterprise received or recorded the fact. |
| Transaction Time | When an aggregate was created or revised within Core Intelligence. |
| Publication Time | When an aggregate became authoritative and visible to consumers. |

### 4.1.2 Semantic Object Families

| Family | Pattern | Examples |
|---|---|---|
| Snapshot | Point-in-time capture of enterprise facts | Enterprise Picture |
| Centralized Registry | Unified lifecycle management for enterprise conditions | Exception |

### 4.1.3 Object Classification

All Core domain semantic objects are defined in the Enterprise Semantic Model.

Core Intelligence does not define new enterprise semantic objects.

Core Intelligence defines governed behaviors over Enterprise Semantic Objects.

### 4.1.4 Behavior Contract Principles

Core Aggregate Behaviors:

- modify exactly one Aggregate Root;
- publish Enterprise Events only;
- never publish Business Notifications;
- never mutate Semantic Objects owned by another Capability;
- enforce declared Business Rules;
- consume governed Policy configuration;
- invoke Decisions and Business Algorithms by identifier.

## 4.2 Domain Dependency Declaration

Every Enterprise Semantic Object consumed by the Core domain is listed below.

| Enterprise Object | Required Attributes |
|---|---|
| `SE-C-010 Planning Scope` | Identifier, Scope Name, Boundary Rules, Lifecycle State |
| `SE-C-013 Demand` | Identifier, Item, Quantity, Location, Need Window, Demand Origin, Lifecycle State |
| `SE-C-014 Supply` | Identifier, Item, Quantity, Location, Availability Window, Supply Provenance Classification, Lifecycle State |
| `SE-C-015 Inventory` | Item, Location, Batch Identifier, On-Hand Quantity, Observation Timestamp |
| `SE-C-019 Exception` | Identifier, Constraint Reference, Exception Classification, Affected Scope Type, Affected Scope Identifier, Evidence Reference, Lifecycle State |
| `SE-C-021 Enterprise Picture` | Planning Scope Identifier, Versions |
| `SE-C-022 Timestamp` | Entire value |
| `SE-C-037 Enterprise Governed Vocabulary` | Catalog Identifier, Version Number, Vocabulary Entries |
| `SE-C-038 Scope Boundary Rule` | Rule Identifier, Target Semantic Type, Inclusion Indicator, Target Instance Identifiers, Target Category Identifiers |

No semantic object may be used later in this specification unless declared here.

### Inventory Identity Rule

`SE-C-015 Inventory` is identified by:

> `Item + Location + Batch Identifier`.

Core Intelligence shall never treat Inventory identity as `Item + Location` alone.

## 4.3 Aggregate Roots

Aggregate Behaviors defined in this section do not redefine Enterprise Semantic Objects.

They define the governed verbs that mutate Enterprise Semantic Objects stewarded by the Core domain.

All Aggregate Behaviors publish Enterprise Events only.

All Business Notifications are published exclusively by Business Workflow Notification Nodes.


### AB-C-001 — Compose Enterprise Picture Version

| Section | Contract |
|---|---|
| Purpose | Compose a Draft `PictureVersion` for a Planning Scope. |
| Business Intent | Ensure every governed composition attempt produces a traceable candidate snapshot of enterprise reality. |
| Owned Aggregate | `SE-C-021 Enterprise Picture`. |
| Trigger | Scheduled composition cadence governed by `PO-C-001`. |
| Required Input State | `SE-C-010 Planning Scope` is Active. |
| Produced Output State | A new `PictureVersion` in Draft state exists within `SE-C-021`. |
| Invoked Decisions | None. |
| Invoked Algorithms | None. |
| Published Events | `EV-C-001 Enterprise Picture Version Composed`. |
| Business Transaction | Atomic creation of one Draft `PictureVersion` within the `SE-C-021` aggregate. |
| Idempotency Guarantee | Re-execution of the same scheduled composition occurrence with the same enterprise facts produces the same Draft content and does not create a duplicate Draft. |
| Concurrency Guarantee | Composition for different Planning Scopes is independent. Composition for the same Planning Scope is serialized. |

**Preconditions**

- `SE-C-010 Planning Scope` exists.
- `SE-C-010 Planning Scope` is Active.
- `PO-C-001` is current.
- Current `SE-C-013 Demand`, `SE-C-014 Supply`, and `SE-C-015 Inventory` facts are readable.
- `SE-C-038 Scope Boundary Rule` entries for the Planning Scope are readable.

**Semantic Preconditions**

- `SE-C-010 Planning Scope` identity is valid.
- `SE-C-038 Scope Boundary Rule` entries are structurally valid.
- `SE-C-013 Demand`, `SE-C-014 Supply`, and `SE-C-015 Inventory` identities are valid.
- `SE-C-015 Inventory` identity is interpreted as `Item + Location + Batch Identifier`.

**Business Behavior**

1. Validate the Planning Scope.
2. Load the Planning Scope Boundary Rules.
3. Evaluate the Boundary Rules against current Demand, Supply, and Inventory.
4. Create a new Draft `PictureVersion` with the next monotonic Version Number.
5. Include only Demand, Supply, and Inventory references that satisfy the Boundary Rules.
6. Record the Composition Trigger Time.
7. Publish `EV-C-001 Enterprise Picture Version Composed`.

**State Transitions**

| From | To |
|---|---|
| Enterprise Picture aggregate with no Draft for this composition | Enterprise Picture aggregate containing a new Draft `PictureVersion` |
| Enterprise Picture aggregate with existing versions | Enterprise Picture aggregate containing an additional Draft `PictureVersion` |

**Rules Enforced**

- `BR-C-001`
- `BR-C-007`
- `BR-C-011`

**Policies Referenced**

- `PO-C-001`

**Exceptional Conditions**

- If the Planning Scope is not Active, composition is rejected.
- If Boundary Rules cannot be evaluated, composition fails.
- If Demand, Supply, or Inventory facts are unavailable, composition fails as a retryable operational failure.

**Postconditions**

- A Draft `PictureVersion` exists.
- `EV-C-001` has been published.
- No Published `PictureVersion` is changed by this behavior.

**Traceability**

- Owned by `CA-C-019`.
- Invoked by `FS-C-001`.
- Modifies `SE-C-021`.
- Publishes `EV-C-001`.


### AB-C-002 — Publish Enterprise Picture Version

| Section | Contract |
|---|---|
| Purpose | Publish the latest Draft `PictureVersion` only when publication is materially warranted. |
| Business Intent | Preserve exactly one authoritative Published `PictureVersion` per Planning Scope. |
| Owned Aggregate | `SE-C-021 Enterprise Picture`. |
| Trigger | `EV-C-001 Enterprise Picture Version Composed`. |
| Required Input State | A Draft `PictureVersion` exists. |
| Produced Output State | If material change is determined: Draft becomes Published and previous Published becomes Superseded. If no material change is determined: no lifecycle transition occurs. |
| Invoked Decisions | `DE-C-001 Assess Picture Materiality`. |
| Invoked Algorithms | `BA-C-001 Evaluate Picture Materiality`. |
| Published Events | `EV-C-002 Enterprise Picture Version Published` when publication occurs. No event is published when no material change exists. |
| Business Transaction | Atomic transition of the Draft `PictureVersion` to Published and the previous Published `PictureVersion` to Superseded. If no material change exists, the transaction completes with no state change. |
| Idempotency Guarantee | If the Draft `PictureVersion` is already Published, re-execution has no effect. |
| Concurrency Guarantee | Publication for a given Planning Scope is serialized. |

**Preconditions**

- `EV-C-001` has been received.
- A Draft `PictureVersion` exists.
- `PO-C-001` is current.

**Semantic Preconditions**

- The latest Draft `PictureVersion` belongs to the Planning Scope identified by `EV-C-001`.
- The current Published `PictureVersion`, if present, is readable.
- `PO-C-001` is readable.

**Business Behavior**

1. Identify the latest Draft `PictureVersion`.
2. Invoke `BA-C-001` to compare the latest Draft with the current Published version.
3. Invoke `DE-C-001` to determine whether the change is material.
4. If the outcome is `Material Change`:
   - transition the Draft `PictureVersion` to Published;
   - transition the previous Published `PictureVersion` to Superseded;
   - record Publication Time;
   - publish `EV-C-002`.
5. If the outcome is `No Material Change`:
   - make no lifecycle transition;
   - publish no event.

**State Transitions**

| Condition | From | To |
|---|---|---|
| Material Change | Draft | Published |
| Material Change | Previous Published | Superseded |
| No Material Change | Draft | Draft |
| No Material Change | Published | Published |

Rules Enforced

- `BR-C-003`
- `BR-C-004`
- `BR-C-015`

**Policies Referenced**

- `PO-C-001`

**Exceptional Conditions**

- If no Draft exists, publication is rejected.
- If `PO-C-001` is missing or not current, publication is rejected.
- If this is the first publication, the result is always `Material Change`.
- If the previous Published version is unavailable and this is not the first publication, the result is `Material Change` with a governance warning.

**Postconditions**

If publication occurs:

- Exactly one Published `PictureVersion` exists for the Planning Scope.
- The newly Published version is immutable.
- The previous Published version is Superseded and immutable.
- `EV-C-002` has been published.

If publication does not occur:

- The previous Published version remains authoritative.
- The Draft remains retained.
- No `EV-C-002` is published.

**Traceability**

- Owned by `CA-C-019`.
- Invoked by `FS-C-002`.
- Modifies `SE-C-021`.
- Publishes `EV-C-002`.

### AB-C-003 — Process Exception Detection Evidence

| Section | Contract |
|---|---|
| Purpose | Create or update an Exception from authorized detection evidence. |
| Business Intent | Maintain a single deduplicated Active Exception per exception business identity. |
| Owned Aggregate | `SE-C-019 Exception`. |
| Trigger | Authorized exception detection evidence notification. Currently declared: `BN-D-022`. |
| Required Input State | None for creation. Active Exception for update. |
| Produced Output State | New Active Exception, or updated Active Exception. |
| Invoked Decisions | `DE-C-002 Evaluate Exception Evidence`. |
| Invoked Algorithms | None. |
| Published Events | `EV-C-003 Enterprise Exception Activated` for creation. `EV-C-004 Enterprise Exception Updated` for update. |
| Business Transaction | Atomic creation or update of one `SE-C-019` aggregate. If evidence is rejected, the transaction completes with no state change. |
| Idempotency Guarantee | Re-processing the same detection evidence does not create a duplicate Exception. |
| Concurrency Guarantee | Exceptions for different business identities are independent. Exceptions for the same business identity are serialized. |

**Preconditions**

- Authorized detection evidence has been received.
- `PO-C-002` is current.
- Evidence contains:
  - Constraint Reference;
  - Exception Classification;
  - Affected Scope Type;
  - Affected Scope Identifier;
  - Severity;
  - Evidence Reference.
- Constraint Reference is valid.
- Exception Classification is an Active entry in `SE-C-037`.
- Affected Scope Type is an Active entry in `SE-C-037`.
- Severity is recognized by `PO-C-002`.

**Semantic Preconditions**

- The exception business identity is structurally complete.
- The affected scope identifier is non-empty.
- The evidence reference is non-empty.

**Business Behavior**

1. Invoke `DE-C-002`.
2. If the outcome is `Create New Exception`:
   - create a new `SE-C-019 Exception` in Active state;
   - publish `EV-C-003`.
3. If the outcome is `Update Existing Exception`:
   - update the existing Active Exception with new evidence;
   - update severity only if the governed severity update rule determines a higher severity;
   - publish `EV-C-004`.
4. If the outcome is `Reject Evidence`:
   - create no Exception;
   - update no Exception;
   - publish no event.

**State Transitions**

| Outcome | From | To |
|---|---|---|
| Create New Exception | No Exception for business identity | Active Exception |
| Update Existing Exception | Active Exception | Active Exception |
| Reject Evidence | No state change | No state change |

**Rules Enforced**

- `BR-C-002`
- `BR-C-005`
- `BR-C-008`
- `BR-C-010`

**Policies Referenced**

- `PO-C-002`

**Exceptional Conditions**

- Invalid Constraint Reference: reject with reason `InvalidConstraintReference`.
- Unrecognized Exception Classification: reject with reason `UnrecognizedExceptionClassification`.
- Unrecognized Affected Scope Type: reject with reason `UnrecognizedScopeType`.
- Unrecognized Severity: reject with reason `UnrecognizedSeverity`.
- Missing `PO-C-002`: reject with reason `MissingExceptionGovernancePolicy`.

**Postconditions**

If creation occurs:

- Exactly one Active Exception exists for the business identity.
- `EV-C-003` has been published.

If update occurs:

- The existing Active Exception is updated.
- No duplicate Exception is created.
- `EV-C-004` has been published.

If rejection occurs:

- No Exception state changes.
- No event is published.

**Traceability**

- Owned by `CA-C-020`.
- Invoked by `FS-C-003`.
- Modifies `SE-C-019`.
- Publishes `EV-C-003` or `EV-C-004`.


### AB-C-004 — Process Exception Resolution Evidence

| Section | Contract |
|---|---|
| Purpose | Resolve an Active Exception from authorized resolution evidence. |
| Business Intent | Preserve immutable resolution history for enterprise exceptions. |
| Owned Aggregate | `SE-C-019 Exception`. |
| Trigger | Authorized exception resolution evidence notification. Currently declared: `BN-D-023`. |
| Required Input State | The referenced Exception is Active. |
| Produced Output State | The referenced Exception becomes Resolved. |
| Invoked Decisions | `DE-C-003 Evaluate Exception Resolution`. |
| Invoked Algorithms | None. |
| Published Events | `EV-C-005 Enterprise Exception Resolved`. |
| Business Transaction | Atomic transition of one `SE-C-019` aggregate from Active to Resolved. If evidence is rejected, the transaction completes with no state change. |
| Idempotency Guarantee | Re-processing resolution evidence for an already Resolved Exception has no effect. |
| Concurrency Guarantee | Resolutions for different Exceptions are independent. Resolutions for the same Exception are serialized. |

**Preconditions**

- Authorized resolution evidence has been received.
- `PO-C-002` is current.
- Evidence contains:
  - Constraint Reference;
  - Affected Scope Type;
  - Affected Scope Identifier;
  - Resolution Evidence.
- Constraint Reference is valid.

**Semantic Preconditions**

- The exception business identity is structurally complete.
- The resolution evidence reference is non-empty.

**Business Behavior**

1. Invoke `DE-C-003`.
2. If the outcome is `Resolve Exception`:
   - transition the Active Exception to Resolved;
   - record Resolution Evidence;
   - record Resolution Time;
   - publish `EV-C-005`.
3. If the outcome is `Reject Resolution Evidence`:
   - make no lifecycle transition;
   - publish no event.

**State Transitions**

| Outcome | From | To |
|---|---|---|
| Resolve Exception | Active | Resolved |
| Reject Resolution Evidence | No state change | No state change |

**Rules Enforced**

- `BR-C-002`
- `BR-C-006`
- `BR-C-008`

**Policies Referenced**

- `PO-C-002`

**Exceptional Conditions**

- No Active Exception exists: reject with reason `NoActiveException`.
- Exception already Resolved: reject with reason `AlreadyResolved`.
- Invalid Constraint Reference: reject with reason `InvalidConstraintReference`.
- Missing `PO-C-002`: reject with reason `MissingExceptionGovernancePolicy`.

**Postconditions**

If resolution occurs:

- The Exception is Resolved.
- The Resolved Exception is immutable.
- `EV-C-005` has been published.

If rejection occurs:

- The Exception state does not change.
- No event is published.

**Traceability**

- Owned by `CA-C-020`.
- Invoked by `FS-C-004`.
- Modifies `SE-C-019`.
- Publishes `EV-C-005`.


### SE-C-040 — Item Transition

**Business Intent:** Provide the authoritative enterprise definition of a governed succession relationship between two enterprise items, establishing how planning capabilities shall handle the transition of demand, supply, and inventory from a superseded item to a superseding item.

**Enterprise Meaning:** An Item Transition is an enterprise-recognised succession relationship between two items. It answers "which item replaces which item, and how shall planning handle the transition?" The transition is a governed enterprise fact that multiple Intelligence domains consume to adjust their behavior: Demand Intelligence transfers historical patterns, Supply Intelligence manages phase-out/phase-in procurement, Inventory Intelligence manages obsolescence, and Promise Intelligence manages substitution eligibility. The transition does not own the items themselves; it defines the relationship between them.

**Identity:** Transition Identifier is the immutable enterprise identity of the Item Transition.

**Applied Semantic Patterns:** Aggregate Root

**Semantic Ownership**
- **Item Transition owns:** identity, superseded/superseding item references, transition type, effective/end dates, history mapping rule, phase-in/phase-out parameters, substitution eligibility, lifecycle.
- **Item Transition excludes:** item identities (owned by SE-C-001), demand/supply/inventory records, allocation decisions, execution actions.

**Authority Specification Contract**

| Section                      | Value                                                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Core Domain                                                                                                  |
| Steward Domain               | Core                                                                                                         |
| Mutation Authority           | Enterprise-Governed Master Data                                                                              |
| Authoritative Representation | The enterprise definition of item succession relationships for planning.                                     |
| Authority Scope              | Enterprise-wide                                                                                              |
| Intended Consumers           | Demand Intelligence, Supply Intelligence, Promise Intelligence, Scenario Intelligence, Inventory Planning.   |
| Non-Intended Consumers       | None                                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None                                                                                                         |
| Superseded By                | None                                                                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Business Guarantees     | Every Active Item Transition accurately identifies the superseded item, the superseding item, the transition type, and the governing parameters. At most one Active transition exists per Superseded Item at any moment.         |
| Required Interpretation | Consumers shall treat the Active transition as the authoritative succession relationship. The transition does not prescribe actions; it defines the relationship and parameters that domain capabilities use to adjust behavior. |
| Known Limitations       | Does not define the execution of the transition (e.g., when to actually stop ordering the old item). Does not modify the items themselves. Does not guarantee that the superseding item has equivalent capability.              |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                                              |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                                                     |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model.                            |
| Authoritative Source    | SE-C-040 Item Transition.                                                                                                                                                                                                      |

**Lifecycle Specification Contract**

| State    | Description                                                                 |
| -------- | --------------------------------------------------------------------------- |
| Draft    | Transition is being prepared; not yet authoritative for planning.           |
| Active   | Transition is approved and governs planning behavior.                       |
| Retired  | Transition is no longer relevant; retained for historical traceability.     |

- Permitted Transitions: Draft → Active; Active → Retired.
- Terminal State: Retired.
- History Preservation: State changes recorded for audit.
- Versioning Rules: Not applicable.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object's Mutation Authority archetype (Governed Stewardship Change Approved), as defined in ESM Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                  | Type                                        | Mandatory | Description                                                                                                                                    |
| -------------------------- | ------------------------------------------- | --------- | ---------------------------------------------------------------------------------------------------------------------------------------------- |
| Transition Identifier      | ID (immutable)                              | Yes       | Unique enterprise identity.                                                                                                                    |
| Superseded Item            | Reference (SE-C-001)                        | Yes       | The item being replaced.                                                                                                                       |
| Superseding Item           | Reference (SE-C-001)                        | Yes       | The item that replaces it.                                                                                                                     |
| Transition Type            | Governed Identifier Reference (SE-C-037)    | Yes       | The category of transition (e.g., "DirectReplacement", "PhaseInPhaseOut", "Merge"). Governed by SE-C-037.                                      |
| Effective Date             | Timestamp (SE-C-022)                        | Yes       | When the transition becomes applicable for planning.                                                                                           |
| End Date                   | Timestamp (SE-C-022)                        | No        | When the transition ceases to be applicable. Absent means open-ended.                                                                          |
| History Mapping Rule       | Governed Identifier Reference (SE-C-037)    | Yes       | How demand history transfers to the superseding item (e.g., "FullTransfer", "WeightedTransfer", "NoTransfer"). Governed by SE-C-037.           |
| Substitution Eligibility   | Boolean                                     | Yes       | Whether the superseding item can substitute for the superseded item in order promising.                                                        |
| Lifecycle State            | Enum (Draft, Active, Retired)               | Yes       | Current state.                                                                                                                                 |

**Relationships**

| Relationship     | Target Object       | Cardinality | Description                              |
| ---------------- | ------------------- | ----------- | ---------------------------------------- |
| supersedes       | Item (SE-C-001)     | Many-to-One | The item being replaced.                 |
| is superseded by | Item (SE-C-001)     | Many-to-One | The item that replaces it.               |
| referenced by    | Plan (SE-C-012)     | One-to-Many | Plans may reference active transitions.  |

**Invariants:**
- Transition Identifier is immutable.
- Superseded Item and Superseding Item must reference distinct items.
- Superseded Item must be in Active or Inactive state (cannot supersede a Retired item).
- Superseding Item must be in Active state.
- At most one Active transition exists per Superseded Item at any moment.
- Effective Date must be valid.
- End Date, if present, must be after Effective Date.
- A Retired transition cannot be referenced by new planning activities.

**Dependencies:**

| Dependency Type       | Description                                                    |
| --------------------- | -------------------------------------------------------------- |
| Semantic Dependency   | SE-C-001 Item, SE-C-022 Timestamp, SE-C-037 Enterprise Governed Vocabulary. |
| Conceptual Dependency  | None.                                                          |

**Traceability:**

| Direction | Reference                                                                              |
| --------- | -------------------------------------------------------------------------------------- |
| Upward    | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4.   |
| Admission | Enterprise Vocabulary admission record.                                                |
| Downward  | Demand Intelligence, Supply Intelligence, Promise Intelligence, Scenario Intelligence. |

#### AB-C-005 – Register Item Transition

**Purpose:** Create a Draft Item Transition from a governed stewardship request.

**Business Intent:** Establish a traceable record of a proposed item succession relationship.

**Trigger:** Governed Stewardship Change Approved (per ESM §2.7 Mutation Authority archetype).

**Preconditions:** Superseded Item exists in SE-C-001. Superseding Item exists in SE-C-001. Superseded Item is not Retired. Superseding Item is Active. Superseded Item ≠ Superseding Item.

**Business Behavior:** Validate item references. Create a new SE-C-040 Item Transition in Draft state with all mandatory attributes. Publish EV-C-006.

**State Transitions:** None → Draft.

**Business Transaction:** Protects SE-C-040 aggregate. Atomic creation.

**Decisions Invoked:** None.

**Events Published:** EV-C-006 (Item Transition Registered).

**Idempotency:** Re-execution with the same Transition Identifier produces no duplicate.

**Concurrency:** Registrations for different Superseded Items are independent. Same Superseded Item is serialized.

**Traceability:** Owned by SE-C-040. Invoked by FS-C-005.

#### AB-C-006 – Activate Item Transition

**Purpose:** Transition a Draft Item Transition to Active, making it authoritative for planning.

**Business Intent:** Ensure that only validated transitions govern planning behavior.

**Trigger:** Stewardship approval for activation.

**Preconditions:** SE-C-040 is in Draft state. No other Active transition exists for the same Superseded Item. Superseding Item is still Active. PO-C-003 activation criteria are satisfied.

**Business Behavior:** Execute DE-C-005 (Approve Item Transition Activation). If approved: transition Draft → Active, publish EV-C-007. If rejected: Draft retained.

**State Transitions:** Draft → Active.

**Business Transaction:** Protects SE-C-040 aggregate. Atomic activation.

**Decisions Invoked:** DE-C-005.

**Events Published:** EV-C-007 (Item Transition Activated).

**Idempotency:** Re-execution on already-Active transition terminates immediately.

**Concurrency:** Activation for a given Superseded Item is serialized.

**Traceability:** Owned by SE-C-040. Invoked by FS-C-006.

#### AB-C-007 – Retire Item Transition

**Purpose:** Transition an Active Item Transition to Retired when it is no longer relevant.

**Business Intent:** Ensure that retired transitions no longer govern planning behavior while retaining historical traceability.

**Trigger:** Stewardship approval for retirement.

**Preconditions:** SE-C-040 is in Active state.

**Business Behavior:** Transition Active → Retired. Publish EV-C-008.

**State Transitions:** Active → Retired.

**Business Transaction:** Protects SE-C-040 aggregate. Atomic retirement.

**Decisions Invoked:** None.

**Events Published:** EV-C-008 (Item Transition Retired).

**Idempotency:** Re-execution on already-Retired transition terminates immediately.

**Concurrency:** Retirement for a given transition is serialized.

**Traceability:** Owned by SE-C-040. Invoked by FS-C-007.

---

## 4.4 Core Knowledge Artifact Contracts

The following Knowledge Artifacts publish measured Performance Indicator values.

No Knowledge Artifact is defined for `PI-C-002` because `PI-C-002` is not implementable.

### KA-C-001 — Picture Publication Latency Measurement

| Attribute | Value |
|---|---|
| Identifier | `KA-C-001` |
| Name | Picture Publication Latency Measurement |
| Owning Capability | `CA-C-019 Enterprise Picture Management` |
| Knowledge Produced | The publication latency of a Published Enterprise Picture Version. |
| Versioning Rule | Each measurement is versioned by Planning Scope Identifier, Published Version Number, and Measurement Occurrence Number. |
| Confidence | Deterministic. |
| Evidence Reference | `EV-C-001` identity and `EV-C-002` identity. |
| Expiry | Superseded when a replacement measurement for the same Planning Scope and Published Version is published. |
| Governed By | `PO-C-001` |
| Traceability | Implements `PI-C-001`. Owned by `CA-C-019`. |

### KA-C-003 — Exception Deduplication Rate Measurement

| Attribute | Value |
|---|---|
| Identifier | `KA-C-003` |
| Name | Exception Deduplication Rate Measurement |
| Owning Capability | `CA-C-020 Core Exception Management` |
| Knowledge Produced | The deduplication rate for valid exception detection evidence over a governed measurement window. |
| Versioning Rule | Each measurement is versioned by Measurement Window Identifier and Measurement Occurrence Number. |
| Confidence | Deterministic. |
| Evidence Reference | `EV-C-003` occurrences and `EV-C-004` occurrences within the measurement window. |
| Expiry | Superseded when a newer measurement for the same measurement window is published. |
| Governed By | `PO-C-002` |
| Traceability | Implements `PI-C-003`. Owned by `CA-C-020`. |

### KA-C-004 — Exception Resolution Latency Measurement

| Attribute | Value |
|---|---|
| Identifier | `KA-C-004` |
| Name | Exception Resolution Latency Measurement |
| Owning Capability | `CA-C-020 Core Exception Management` |
| Knowledge Produced | The latency between exception activation and exception resolution for a resolved Exception. |
| Versioning Rule | Each measurement is versioned by Exception Identifier and Measurement Occurrence Number. |
| Confidence | Deterministic. |
| Evidence Reference | `EV-C-003` identity and `EV-C-005` identity. |
| Expiry | Superseded when a replacement measurement for the same Exception is published. |
| Governed By | `PO-C-002` |
| Traceability | Implements `PI-C-004`. Owned by `CA-C-020`. |

---

# Chapter 5 — Capability Model

## CA-C-019 — Enterprise Picture Management

**Business Intent:**

Maintain the single, authoritative, point-in-time snapshot of enterprise reality for every active Planning Scope.

**Enterprise Question:**

What is the current, authoritative state of enterprise reality for a given Planning Scope?

**Owned Semantic Objects:**

- `SE-C-021 Enterprise Picture`

**Position in Enterprise Reasoning**

| Role | Description |
|---|---|
| Consumes | `SE-C-013 Demand`, `SE-C-014 Supply`, `SE-C-015 Inventory`, `SE-C-010 Planning Scope`, `SE-C-038 Scope Boundary Rule`. |
| Produces | `SE-C-021 Enterprise Picture` Published Versions. |
| Feeds | Declared consuming Intelligence domains, as verified in their domain specifications. |

**Enterprise Master Data**

| Dependency | Role in Enterprise Picture Management |
|---|---|
| `SE-C-010 Planning Scope` | Defines the business boundary within which the picture is composed. |
| `SE-C-013 Demand` | Provides the current demand facts that fall within the Planning Scope. |
| `SE-C-014 Supply` | Provides the current supply facts that fall within the Planning Scope. |
| `SE-C-015 Inventory` | Provides the current inventory facts that fall within the Planning Scope. |
| `SE-C-038 Scope Boundary Rule` | Provides the deterministic inclusion and exclusion rules for the Planning Scope. |

**Enterprise Governance**

| Dependency | Role |
|---|---|
| `PO-C-001` | Governs materiality thresholds, publication cadence, and publication governance. |

**Business Guarantees**

1. Exactly one Published `PictureVersion` exists per Planning Scope at any moment.
2. A Published `PictureVersion` is immutable and is never modified.
3. A Published `PictureVersion` is only superseded by another Published `PictureVersion`.
4. All references within a Published `PictureVersion` satisfied the Planning Scope boundary at the exact moment of publication.
5. Enterprise Picture composition occurs only on a governed schedule.
6. Enterprise Picture publication occurs only when governed materiality evaluation determines publication is warranted.
7. No ad-hoc composition or publication is authorized.

**Capability Responsibilities**

| ID | Responsibility | Business Workflow | Functional Specification |
|---|---|---|---|
| `CR-C-001` | Compose Enterprise Picture Version | `BW-C-001` | `FS-C-001` |
| `CR-C-002` | Evaluate and Publish Enterprise Picture Version | `BW-C-002` | `FS-C-002` |

**Enterprise Events Published**

All Enterprise Event identities include:

- Aggregate Identifier;
- Event Type;
- Occurrence Number.

| ID | Business Fact | Published By | Business Information |
|---|---|---|---|
| `EV-C-001` | Enterprise Picture Version Composed | `AB-C-001` | Planning Scope Identifier, Draft Version Number, Composition Trigger Time, Composition Timestamp |
| `EV-C-002` | Enterprise Picture Version Published | `AB-C-002` | Planning Scope Identifier, Published Version Number, Publication Time, Material Change Summary |

**Business Notifications Published**

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|---|---|---|---|---|---|
| `BN-C-001` | `EV-C-002` | Enterprise Picture Published: Planning Scope Identifier, Published Version Number, Publication Time, Material Change Summary | At-least-once | Per Planning Scope | Near-real-time |

**Business Notifications Consumed**

| Source Notification | Publisher | Business Behavior | Invokes |
|---|---|---|---|
| None | Not applicable | Enterprise Picture composition is authorized only by governed schedule. | Not applicable |

On-demand Enterprise Picture composition is not authorized until a triggering Intelligence domain publishes a ratified Business Notification contract with stable identifier, referenced Enterprise Events, and explicit delivery, ordering, and timeliness guarantees.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---|---|---|---|
| Owned Semantic Object | `SE-C-021 Enterprise Picture` | ESM §4.1 | Authorized |
| Aggregate Behavior | `AB-C-001 Compose Enterprise Picture Version` | §4.3 | Authorized |
| Aggregate Behavior | `AB-C-002 Publish Enterprise Picture Version` | §4.3 | Authorized |
| Decision | `DE-C-001 Assess Picture Materiality` | §6 | Authorized |
| Rule | `BR-C-001` | §7 | Authorized |
| Rule | `BR-C-003` | §7 | Authorized |
| Rule | `BR-C-004` | §7 | Authorized |
| Rule | `BR-C-007` | §7 | Authorized |
| Rule | `BR-C-009` | §7 | Authorized |
| Rule | `BR-C-011` | §7 | Authorized |
| Rule | `BR-C-012` | §7 | Authorized |
| Rule | `BR-C-013` | §7 | Authorized |
| Rule | `BR-C-014` | §7 | Authorized |
| Rule | `BR-C-015` | §7 | Authorized |
| Policy | `PO-C-001` | §8 | Authorized |
| Functional Specification | `FS-C-001` | §9 | Authorized |
| Functional Specification | `FS-C-002` | §9 | Authorized |
| Business Algorithm | `BA-C-001` | §10 | Authorized |
| Enterprise Event | `EV-C-001` | §5 | Authorized |
| Enterprise Event | `EV-C-002` | §5 | Authorized |
| Business Notification | `BN-C-001` | §5 | Authorized |
| Knowledge Artifact | `KA-C-001` | §4.4 | Authorized |
| Performance Indicator | `PI-C-001` | §3 | Authorized |
| Performance Indicator | `PI-C-002` | §3 | Not implementable |

## CA-C-020 — Core Exception Management

**Business Intent:**

Maintain the single, centralized enterprise registry of all unsatisfied enterprise constraints published by authorized Intelligence domains.

**Enterprise Question:**

Which enterprise constraints are currently breached, what is their severity, and what is their resolution state?

**Owned Semantic Objects:**

- `SE-C-019 Exception`

**Position in Enterprise Reasoning**

| Role | Description |
|---|---|
| Consumes | Authorized exception detection evidence and authorized exception resolution evidence. |
| Produces | `SE-C-019 Exception` lifecycle transitions. |
| Feeds | Declared consuming Intelligence domains and governance consumers, as verified in their domain specifications. |

**Cross-Domain Dependencies**

| Dependency | Role in Core Exception Management | Verification Status |
|---|---|---|
| Demand Intelligence `CA-D-008 Detect Demand Exceptions` | Publishes demand exception detection and resolution evidence: `BN-D-022`, `BN-D-023`. | Declared. Must be verified against the Demand Intelligence Domain Specification before implementation. |

No Supply, Promise, Scenario, or Knowledge exception evidence notification is authorized in this specification.

Additional evidence notifications may be added only after the owning domain publishes ratified Business Notification contracts and this Core specification is revised through governance.

**Enterprise Governance**

| Dependency | Role |
|---|---|
| `PO-C-002` | Governs exception classification taxonomy, severity assessment, deduplication criteria, and resolution criteria. |

**Business Guarantees**

1. Intelligence domains do not mutate `SE-C-019 Exception` directly.
2. Exception identity is strictly governed by `Constraint Reference + Affected Scope Type + Affected Scope Identifier`.
3. At most one Active Exception exists per exception business identity.
4. Duplicate detection evidence updates an existing Active Exception rather than creating a duplicate Exception.
5. A Resolved Exception is immutable.
6. Resolution history is permanently retained.
7. Exception evidence that cannot be validated is rejected with a governed reason code.

**Capability Responsibilities**

| ID | Responsibility | Business Workflow | Functional Specification |
|---|---|---|---|
| `CR-C-003` | Process Exception Detection Evidence | `BW-C-003` | `FS-C-003` |
| `CR-C-004` | Process Exception Resolution Evidence | `BW-C-004` | `FS-C-004` |

**Enterprise Events Published**

| ID | Business Fact | Published By | Business Information |
|---|---|---|---|
| `EV-C-003` | Enterprise Exception Activated | `AB-C-003` | Exception Identifier, Constraint Reference, Affected Scope Type, Affected Scope Identifier, Exception Classification, Severity, Evidence Reference, Activation Time |
| `EV-C-004` | Enterprise Exception Updated | `AB-C-003` | Exception Identifier, Updated Evidence Reference, Severity, Update Time |
| `EV-C-005` | Enterprise Exception Resolved | `AB-C-004` | Exception Identifier, Resolution Evidence, Resolution Time |

**Business Notifications Published**

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|---|---|---|---|---|---|
| `BN-C-002` | `EV-C-003` or `EV-C-004` | Enterprise Exception Active: Exception Identifier, Constraint Reference, Affected Scope Type, Affected Scope Identifier, Exception Classification, Severity, Detection Evidence | At-least-once | Per Exception | Near-real-time |
| `BN-C-003` | `EV-C-005` | Enterprise Exception Resolved: Exception Identifier, Resolution Evidence, Resolution Time | At-least-once | Per Exception | Near-real-time |

**Business Notifications Consumed**

| Source Notification | Publisher | Business Behavior | Invokes | Verification Status |
|---|---|---|---|---|
| `BN-D-022 Demand Exception Detection Evidence` | Demand Intelligence `CA-D-008` | Processes demand exception detection evidence. Creates or updates `SE-C-019 Exception`. | `FS-C-003` | Must be verified against the Demand Intelligence Domain Specification before implementation. |
| `BN-D-023 Demand Exception Resolution Evidence` | Demand Intelligence `CA-D-008` | Processes demand exception resolution evidence. Resolves `SE-C-019 Exception`. | `FS-C-004` | Must be verified against the Demand Intelligence Domain Specification before implementation. |

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---|---|---|---|
| Owned Semantic Object | `SE-C-019 Exception` | ESM §4.1 | Authorized |
| Aggregate Behavior | `AB-C-003 Process Exception Detection Evidence` | §4.3 | Authorized |
| Aggregate Behavior | `AB-C-004 Process Exception Resolution Evidence` | §4.3 | Authorized |
| Decision | `DE-C-002 Evaluate Exception Evidence` | §6 | Authorized |
| Decision | `DE-C-003 Evaluate Exception Resolution` | §6 | Authorized |
| Rule | `BR-C-002` | §7 | Authorized |
| Rule | `BR-C-005` | §7 | Authorized |
| Rule | `BR-C-006` | §7 | Authorized |
| Rule | `BR-C-008` | §7 | Authorized |
| Rule | `BR-C-010` | §7 | Authorized |
| Policy | `PO-C-002` | §8 | Authorized |
| Functional Specification | `FS-C-003` | §9 | Authorized |
| Functional Specification | `FS-C-004` | §9 | Authorized |
| Enterprise Event | `EV-C-003` | §5 | Authorized |
| Enterprise Event | `EV-C-004` | §5 | Authorized |
| Enterprise Event | `EV-C-005` | §5 | Authorized |
| Business Notification | `BN-C-002` | §5 | Authorized |
| Business Notification | `BN-C-003` | §5 | Authorized |
| Knowledge Artifact | `KA-C-003` | §4.4 | Authorized |
| Knowledge Artifact | `KA-C-004` | §4.4 | Authorized |
| Performance Indicator | `PI-C-003` | §3 | Authorized |
| Performance Indicator | `PI-C-004` | §3 | Authorized |

### Business Workflows

| ID | Business Intent | Realises | Functional Specification | Trigger |
|---|---|---|---|---|
| `BW-C-001` | Compose a point-in-time snapshot of enterprise reality for a governed Planning Scope. | `CR-C-001` | `FS-C-001` | Scheduled cadence governed by `PO-C-001`. |
| `BW-C-002` | Evaluate publication materiality and publish the composed Enterprise Picture Version when warranted. | `CR-C-002` | `FS-C-002` | `EV-C-001 Enterprise Picture Version Composed`. |
| `BW-C-003` | Process exception detection evidence and create or update the centralized exception registry. | `CR-C-003` | `FS-C-003` | `BN-D-022 Demand Exception Detection Evidence`. |
| `BW-C-004` | Process exception resolution evidence and resolve the centralized exception. | `CR-C-004` | `FS-C-004` | `BN-D-023 Demand Exception Resolution Evidence`. |


## 5.3 Manage Item Transitions – CA-C-021

**Business Intent:** Govern the lifecycle of item succession relationships, ensuring that all planning capabilities operate from a single, authoritative transition definition.

**Enterprise Question:** Which item replaces which item, and how shall planning handle the transition?

**Owned Semantic Objects:** SE-C-040 (Item Transition).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | SE-C-001 Item (to validate superseded/superseding items). |
| **Produces** | SE-C-040 Item Transition (Draft → Active → Retired). |
| **Feeds** | Demand Intelligence (history transfer), Supply Intelligence (phase-out/phase-in), Promise Intelligence (substitution), Inventory Planning (obsolescence), Scenario Intelligence (transition scenarios). |

**Enterprise Dependencies**

| Dependency | Role in Manage Item Transitions |
|------------|----------------------------------|
| SE-C-001 Item | Validates that superseded and superseding items exist and are in valid lifecycle states. |
| SE-C-037 Enterprise Governed Vocabulary | Provides governed Transition Type and History Mapping Rule classifications. |
| PO-C-003 Item Transition Governance | Governs transition validation rules and activation criteria. |

**Business Guarantees:**
1. At most one Active Item Transition exists per Superseded Item at any moment.
2. Every Active transition is immutable. Changes require a new transition.
3. All transitions are traceable to their governing approval.
4. Superseded and Superseding items are validated against SE-C-001 lifecycle states before activation.
5. Demand Intelligence never directly creates, modifies, or retires SE-C-040 Item Transition instances.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-C-005 | Register Item Transition | BW-C-005 | FS-C-005 |
| CR-C-006 | Activate Item Transition | BW-C-006 | FS-C-006 |
| CR-C-007 | Retire Item Transition | BW-C-007 | FS-C-007 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-C-006 | Item Transition Registered | AB-C-005 |
| EV-C-007 | Item Transition Activated | AB-C-006 |
| EV-C-008 | Item Transition Retired | AB-C-007 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-C-004 | EV-C-007 | Item Transition Activated: Transition Identifier, Superseded Item, Superseding Item, Transition Type, Effective Date, History Mapping Rule, Substitution Eligibility | At-least-once | Per transition | Near-real-time |
| BN-C-005 | EV-C-008 | Item Transition Retired: Transition Identifier, Superseded Item, Superseding Item | At-least-once | Per transition | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| None | Not applicable | Item Transition lifecycle is governed by stewardship approval, not by external notifications. | Not applicable |

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Object | SE-C-040 – Item Transition | ESM §4.1 | Aligned |
| Aggregate Behavior | AB-C-005 – Register Item Transition | §5.3 | Aligned |
| Aggregate Behavior | AB-C-006 – Activate Item Transition | §5.3 | Aligned |
| Aggregate Behavior | AB-C-007 – Retire Item Transition | §5.3 | Aligned |
| Decision | DE-C-005 – Approve Item Transition Activation | §6 | Aligned |
| Rule | BR-C-013 – Item Transition Identity | §7 | Aligned |
| Rule | BR-C-014 – Superseded Item Validity | §7 | Aligned |
| Rule | BR-C-015 – Superseding Item Validity | §7 | Aligned |
| Rule | BR-C-016 – Single Active Transition per Superseded Item | §7 | Aligned |
| Rule | BR-C-017 – No Self-Supersession | §7 | Aligned |
| Policy | PO-C-003 – Item Transition Governance | §8 | Aligned |
| Functional Specification | FS-C-005 – Register Item Transition | §9 | Aligned |
| Functional Specification | FS-C-006 – Activate Item Transition | §9 | Aligned |
| Functional Specification | FS-C-007 – Retire Item Transition | §9 | Aligned |
| Enterprise Event | EV-C-006 – Item Transition Registered | §5.3 | Aligned |
| Enterprise Event | EV-C-007 – Item Transition Activated | §5.3 | Aligned |
| Enterprise Event | EV-C-008 – Item Transition Retired | §5.3 | Aligned |
| Business Notification | BN-C-004 – Item Transition Activated | §5.3 | Aligned |
| Business Notification | BN-C-005 – Item Transition Retired | §5.3 | Aligned |

---

# Chapter 6 — Decision Model

## DE-C-001 — Assess Picture Materiality

**Outcome Type:** Assessment Decision

### Authority Specification Contract

| Section | Value |
|---|---|
| Business Owner | `CA-C-019 Enterprise Picture Management` |
| Authoritative Representation | The enterprise determination of whether a composed picture differs materially from the last published version. |
| Business Responsibility | Compare the composed picture with the last published version and determine whether a new publication is warranted. |
| Authority Scope | Per Planning Scope. |
| Intended Consumers | `AB-C-002 Publish Enterprise Picture Version`. |
| Non-Intended Consumers | None. |
| Supersedes | None. |
| Superseded By | None. |

### Purpose

Determine whether the composed Enterprise Picture differs materially from the last published version for a given Planning Scope.

### Enterprise Question

Is the composed picture materially different from the last published version, warranting a new publication?

### Behavioral Specification Contract

| Section | Value |
|---|---|
| Preconditions | A composed Draft `PictureVersion` exists. The last published `PictureVersion` is available for comparison, or this is the first publication. `PO-C-001` is current. |
| Business Behavior | Consume the Materiality Assessment produced by `BA-C-001`. Determine whether the change is material according to `PO-C-001`. If this is the first publication, the result is always `Material Change`. |
| Exceptional Conditions | If the last published version is unavailable and this is not the first publication, the result is `Material Change` with a governance warning. |
| Postconditions | A materiality determination is produced: `Material Change` or `No Material Change`. |
| Outcome When Preconditions Are Not Satisfied | If no Draft `PictureVersion` exists, the decision is not applicable. |

### Decision Alternatives

- `Material Change`
- `No Material Change`

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---|---|---|
| `Material Change` | A new publication is warranted. | At least one reference set has changed beyond the materiality threshold governed by `PO-C-001`, or this is the first publication. |
| `No Material Change` | No new publication is warranted. | No reference set has changed beyond the materiality threshold governed by `PO-C-001`. |

**Conflict Resolution:**

If any reference set is material, the overall result is `Material Change`.

### Evidence Contract

| Input | Source | Description |
|---|---|---|
| Draft `PictureVersion` | `SE-C-021` Draft | The composed snapshot to evaluate. |
| Last Published `PictureVersion` | `SE-C-021` Published | The current authoritative snapshot for comparison. |
| Materiality Assessment | `BA-C-001` | Structured comparison result for Demand, Supply, and Inventory reference sets. |
| Materiality thresholds | `PO-C-001` | Governed thresholds for material change detection. |

### Decision Confidence

| Attribute | Value |
|---|---|
| Confidence Type | Rule Certainty |
| Confidence Level | Deterministic |

### Decision Authority

Automatic.

### Business Rules

| ID | Rule |
|---|---|
| `BR-C-009` | Materiality assessment is governed by `PO-C-001`. |
| `BR-C-012` | For first publication, the materiality result is always `Material Change`. |
| `BR-C-013` | A reference set is Material when any governed threshold in `PO-C-001` is crossed. |
| `BR-C-014` | Materiality assessment reports each reference set independently. |

### Policies

Governed by `PO-C-001`.

### Decision Trace

| Attribute | Value |
|---|---|
| Decision Owner | `AB-C-002 Publish Enterprise Picture Version` |
| Invoked By | `AB-C-002` |
| References | `BR-C-009`, `BR-C-012`, `BR-C-013`, `BR-C-014` |
| Governed By | `PO-C-001` |
| Produces | Publication determination consumed by `AB-C-002`. |

### Explainability

The explanation shall identify:

- Planning Scope;
- Draft Version Number;
- Published Version Number, if present;
- result;
- Demand reference set result;
- Supply reference set result;
- Inventory reference set result;
- applied thresholds from `PO-C-001`.

## DE-C-002 — Evaluate Exception Evidence

**Outcome Type:** Acceptance Decision

### Authority Specification Contract

| Section | Value |
|---|---|
| Business Owner | `CA-C-020 Core Exception Management` |
| Authoritative Representation | The enterprise determination of whether exception detection evidence warrants creating a new exception or updating an existing one. |
| Business Responsibility | Evaluate exception detection evidence, enforce deduplication, and determine the appropriate lifecycle action. |
| Authority Scope | Per exception business identity. |
| Intended Consumers | `AB-C-003 Process Exception Detection Evidence`. |
| Non-Intended Consumers | None. |
| Supersedes | None. |
| Superseded By | None. |

### Purpose

Determine whether exception detection evidence warrants creating a new exception, updating an existing exception, or rejecting the evidence.

### Enterprise Question

Does this exception detection evidence warrant creating a new exception or updating an existing one?

### Behavioral Specification Contract

| Section | Value |
|---|---|
| Preconditions | Exception detection evidence has been received from an authorized notification. `PO-C-002` is current. |
| Business Behavior | Validate the constraint reference, affected scope, classification, severity, and evidence reference. Check for an existing Active exception with the same business identity. If no existing exception exists, the outcome is `Create New Exception`. If an existing exception exists, the outcome is `Update Existing Exception`. If the evidence is invalid, the outcome is `Reject Evidence`. |
| Exceptional Conditions | If the constraint reference is invalid, classification is unrecognized, scope type is unrecognized, severity is unrecognized, or policy is missing, the outcome is `Reject Evidence` with a governed reason code. |
| Postconditions | A lifecycle action determination is produced: `Create New Exception`, `Update Existing Exception`, or `Reject Evidence`. |
| Outcome When Preconditions Are Not Satisfied | If the evidence is malformed, the outcome is `Reject Evidence`. |

### Decision Alternatives

- `Create New Exception`
- `Update Existing Exception`
- `Reject Evidence`

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---|---|---|
| `Create New Exception` | A new `SE-C-019 Exception` is created in Active state. | No existing Active exception matches the business identity. Evidence is valid. |
| `Update Existing Exception` | The existing `SE-C-019 Exception` is updated with new evidence and, where governed, higher severity. | An existing Active exception matches the business identity. |
| `Reject Evidence` | The evidence is not processed. | The evidence is invalid. |

**Conflict Resolution:**

If multiple detection evidence items arrive for the same business identity, they are processed sequentially with idempotent deduplication.

### Evidence Contract

| Input | Source | Description |
|---|---|---|
| Exception detection evidence | `BN-D-022` | Constraint Reference, Affected Scope Type, Affected Scope Identifier, Exception Classification, Severity, Evidence Reference. |
| Existing Active exceptions | `SE-C-019 Exception` | Current Active exceptions for deduplication. |
| Exception governance rules | `PO-C-002` | Deduplication criteria and severity assessment rules. |

### Decision Confidence

| Attribute | Value |
|---|---|
| Confidence Type | Rule Certainty |
| Confidence Level | Deterministic |

### Decision Authority

Automatic.

### Business Rules

| ID | Rule |
|---|---|
| `BR-C-002` | Exception business identity is `Constraint Reference + Affected Scope Type + Affected Scope Identifier`. |
| `BR-C-005` | At most one Active exception exists per business identity. |
| `BR-C-008` | Exception evidence requires a valid Constraint Reference. |
| `BR-C-010` | Exception severity assessment is governed by `PO-C-002`. |

### Policies

Governed by `PO-C-002`.

### Decision Trace

| Attribute | Value |
|---|---|
| Decision Owner | `AB-C-003 Process Exception Detection Evidence` |
| Invoked By | `AB-C-003` |
| References | `BR-C-002`, `BR-C-005`, `BR-C-008`, `BR-C-010` |
| Governed By | `PO-C-002` |
| Produces | Lifecycle action determination consumed by `AB-C-003`. |

### Explainability

The explanation shall identify:

- Constraint Reference;
- Affected Scope Type;
- Affected Scope Identifier;
- outcome;
- deduplication result;
- severity;
- reason code when evidence is rejected.

## DE-C-003 — Evaluate Exception Resolution

**Outcome Type:** Acceptance Decision

### Authority Specification Contract

| Section | Value |
|---|---|
| Business Owner | `CA-C-020 Core Exception Management` |
| Authoritative Representation | The enterprise determination of whether exception resolution evidence warrants resolving an existing exception. |
| Business Responsibility | Evaluate exception resolution evidence and determine whether the exception should transition to Resolved. |
| Authority Scope | Per exception business identity. |
| Intended Consumers | `AB-C-004 Process Exception Resolution Evidence`. |
| Non-Intended Consumers | None. |
| Supersedes | None. |
| Superseded By | None. |

### Purpose

Determine whether exception resolution evidence warrants resolving an existing exception.

### Enterprise Question

Does this resolution evidence warrant resolving the existing exception?

### Behavioral Specification Contract

| Section | Value |
|---|---|
| Preconditions | Exception resolution evidence has been received from an authorized notification. `PO-C-002` is current. |
| Business Behavior | Validate the resolution evidence. Check that an Active exception exists for the referenced business identity. If valid and the exception is Active, the outcome is `Resolve Exception`. If no Active exception exists, the outcome is `Reject Resolution Evidence`. |
| Exceptional Conditions | If resolution evidence is received for an exception that is already Resolved, the outcome is `Reject Resolution Evidence` with reason code `AlreadyResolved`. |
| Postconditions | A lifecycle action determination is produced: `Resolve Exception` or `Reject Resolution Evidence`. |
| Outcome When Preconditions Are Not Satisfied | If no Active exception exists, the outcome is `Reject Resolution Evidence`. |

### Decision Alternatives

- `Resolve Exception`
- `Reject Resolution Evidence`

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---|---|---|
| `Resolve Exception` | The `SE-C-019 Exception` transitions from Active to Resolved. | An Active exception exists for the business identity. Resolution evidence is valid. |
| `Reject Resolution Evidence` | The resolution evidence is not processed. | No Active exception exists, the exception is already resolved, or the evidence is invalid. |

**Conflict Resolution:**

Not applicable. Resolution is a single deterministic action.

### Evidence Contract

| Input | Source | Description |
|---|---|---|
| Exception resolution evidence | `BN-D-023` | Constraint Reference, Affected Scope Type, Affected Scope Identifier, Resolution Evidence. |
| Existing Active exception | `SE-C-019 Exception` | The exception to resolve. |
| Exception governance rules | `PO-C-002` | Resolution criteria. |

### Decision Confidence

| Attribute | Value |
|---|---|
| Confidence Type | Rule Certainty |
| Confidence Level | Deterministic |

### Decision Authority

Automatic.

### Business Rules

| ID | Rule |
|---|---|
| `BR-C-002` | Exception business identity is `Constraint Reference + Affected Scope Type + Affected Scope Identifier`. |
| `BR-C-006` | Resolved Exception is immutable. |
| `BR-C-008` | Exception evidence requires a valid Constraint Reference. |

### Policies

Governed by `PO-C-002`.

### Decision Trace

| Attribute | Value |
|---|---|
| Decision Owner | `AB-C-004 Process Exception Resolution Evidence` |
| Invoked By | `AB-C-004` |
| References | `BR-C-002`, `BR-C-006`, `BR-C-008` |
| Governed By | `PO-C-002` |
| Produces | Lifecycle action determination consumed by `AB-C-004`. |

### Explainability

The explanation shall identify:

- Constraint Reference;
- Affected Scope Type;
- Affected Scope Identifier;
- outcome;
- resolution evidence;
- reason code when resolution evidence is rejected.

---


## DE-C-005 – Approve Item Transition Activation

**Outcome Type:** Authorization Decision

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| **Business Owner** | Manage Item Transitions (CA-C-021) |
| **Authoritative Representation** | The enterprise's determination that an Item Transition meets activation criteria. |
| **Business Responsibility** | Validate that the transition is eligible to become authoritative for planning. |
| **Authority Scope** | Per Item Transition. |
| **Intended Consumers** | Demand Intelligence, Supply Intelligence, Promise Intelligence. |

**Purpose:** Determine whether a Draft Item Transition is eligible for activation.

**Enterprise Question:** Does this item transition meet all validation criteria to become the authoritative succession relationship?

**Behavioral Specification Contract**

| Section | Value |
|---------|-------|
| **Preconditions** | SE-C-040 is in Draft state. PO-C-003 is current. |
| **Business Behavior** | Validate: (1) No other Active transition exists for the same Superseded Item. (2) Superseding Item is still Active. (3) Effective Date is valid. (4) History Mapping Rule is a recognized governed entry. If all pass, outcome is Activate. Otherwise, Do Not Activate. |
| **Exceptional Conditions** | If Superseding Item has been retired since registration, outcome is Do Not Activate. |
| **Postconditions** | If Activate: Draft becomes Active. If Do Not Activate: Draft retained. |
| **Outcome When Preconditions Are Not Satisfied** | If no Draft exists, the decision is not applicable. |

**Decision Alternatives:** Activate, Do Not Activate.

**Decision Outcome Contract**

| Outcome | Criteria |
|---------|----------|
| Activate | No conflicting Active transition. Superseding Item Active. All validation rules pass. |
| Do Not Activate | Conflicting transition exists, or Superseding Item not Active, or validation rule fails. |

**Evidence Contract**

| Input | Source | Description |
|-------|--------|-------------|
| Draft Item Transition | SE-C-040 (Draft) | The transition being evaluated. |
| Existing Active transitions | SE-C-040 | Check for conflicts. |
| Superseding Item state | SE-C-001 | Validate item is Active. |
| Activation criteria | PO-C-003 | Governed validation rules. |

**Decision Confidence:** Rule Certainty. Binary.

**Decision Authority:** Automatic.

**Business Rules**

| ID | Rule |
|----|------|
| BR-C-013 | Transition Identifier is unique. |
| BR-C-014 | Superseded Item must be Active or Inactive. |
| BR-C-015 | Superseding Item must be Active. |
| BR-C-016 | At most one Active transition per Superseded Item. |
| BR-C-017 | Superseded Item ≠ Superseding Item. |

**Policies:** Governed by PO-C-003.

**Decision Trace**

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-C-006 (Activate Item Transition) |
| Invoked By | FS-C-006 |
| References | BR-C-013, BR-C-014, BR-C-015, BR-C-016, BR-C-017 |
| Governed By | PO-C-003 |
| Produces | Activation determination consumed by AB-C-006 |

---

# Chapter 7 — Rule Model

## Rule Precedence

| Rule Type | Enforcement Point | Override Behavior |
|---|---|---|
| Identity | Object creation | Absolute. Cannot be overridden. |
| Invariant | Aggregate commit boundary | Absolute. Cannot be overridden. |
| Eligibility | Functional Specification preconditions | Failure prevents further processing unless governed exception policy applies. |
| Behavior | Decision evaluation | Does not override Eligibility or Invariant rules. |
| Derivation | Business Algorithm execution | Does not override Eligibility, Invariant, or Behavior rules. |

## Identity Rules

### BR-C-001 — Enterprise Picture Aggregate Identity

**Rule Statement:**

The business identity of an Enterprise Picture is the Planning Scope Identifier. Exactly one Enterprise Picture exists per Planning Scope.

**Rule:**

Each Planning Scope has exactly one Enterprise Picture aggregate. The aggregate identity is the Planning Scope Identifier. Multiple PictureVersions exist within the aggregate, but the aggregate itself is singular per scope.

**Evaluation Scope:**

Per aggregate, at creation.

**Enforcement Point:**

`AB-C-001 Compose Enterprise Picture Version`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If an Enterprise Picture already exists for the Planning Scope, a new PictureVersion is created within the existing aggregate.

**Traceability:**

Owned by `CA-C-019`. Referenced by `AB-C-001`, `FS-C-001`.

### BR-C-002 — Exception Business Identity

**Rule Statement:**

The business identity of an Exception is the combination of Constraint Reference, Affected Scope Type, and Affected Scope Identifier.

**Rule:**

Each Exception is uniquely identified by the combination of the constraint that is breached, the type of enterprise entity affected, and the specific identifier of that entity. This composite key is used for deduplication.

**Evaluation Scope:**

Per exception, at creation.

**Enforcement Point:**

`AB-C-003 Process Exception Detection Evidence`.

**Governed Policy:**

`PO-C-002`.

**Outcome When Preconditions Are Not Satisfied:**

If the composite key is incomplete, the evidence is rejected.

**Traceability:**

Owned by `CA-C-020`. Referenced by `AB-C-003`, `AB-C-004`, `FS-C-003`, `FS-C-004`.

### BR-C-013 – Item Transition Identity

**Rule Statement:** Each Item Transition shall have a globally unique Transition Identifier assigned at registration.

**Evaluation Scope:** Per transition, at creation.

**Enforcement Point:** AB-C-005 (Register Item Transition).

**Governed Policy:** PO-C-003.

**Outcome When Preconditions Are Not Satisfied:** Registration is rejected.

**Traceability:** Owned by CA-C-021. Referenced by AB-C-005, FS-C-005.

---

## Invariant Rules

### BR-C-003 — Single Published PictureVersion per Planning Scope

**Rule Statement:**

Exactly one Published PictureVersion exists per Planning Scope at any moment.

**Rule:**

Before publishing a new PictureVersion, the previous Published version must be transitioned to Superseded atomically within the same business transaction.

**Evaluation Scope:**

Per aggregate, at publication.

**Enforcement Point:**

`AB-C-002 Publish Enterprise Picture Version`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

The transaction fails. No new Published version is created. The previous Published version remains authoritative.

**Traceability:**

Owned by `CA-C-019`. Referenced by `AB-C-002`, `FS-C-002`.

### BR-C-004 — Published PictureVersion Immutability

**Rule Statement:**

A Published PictureVersion is immutable.

**Rule:**

Any operation attempting to modify a Published PictureVersion must be rejected. The version must be in Draft state to accept modifications.

**Evaluation Scope:**

Per aggregate, on any modification attempt.

**Enforcement Point:**

Aggregate root behavior of `SE-C-021`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

The operation is rejected with an invariant violation.

**Traceability:**

Owned by `CA-C-019`. Referenced by `AB-C-001`, `AB-C-002`.

### BR-C-005 — Single Active Exception per Business Identity

**Rule Statement:**

At most one Active Exception exists per business identity.

**Rule:**

For a given composite business identity, at most one Exception may be in Active state. If detection evidence arrives for an identity that already has an Active exception, the existing exception is updated rather than a duplicate being created.

**Evaluation Scope:**

Per exception, at creation or update.

**Enforcement Point:**

`AB-C-003 Process Exception Detection Evidence`.

**Governed Policy:**

`PO-C-002`.

**Outcome When Preconditions Are Not Satisfied:**

If an Active exception already exists, the evidence updates the existing exception.

**Traceability:**

Owned by `CA-C-020`. Referenced by `AB-C-003`, `FS-C-003`.

### BR-C-006 — Resolved Exception Immutability

**Rule Statement:**

A Resolved Exception is immutable.

**Rule:**

Any operation attempting to modify a Resolved Exception must be rejected. If a new constraint breach occurs for the same business identity after resolution, a new Exception instance is created.

**Evaluation Scope:**

Per exception, on any modification attempt.

**Enforcement Point:**

Aggregate root behavior of `SE-C-019`.

**Governed Policy:**

`PO-C-002`.

**Outcome When Preconditions Are Not Satisfied:**

The operation is rejected with an invariant violation.

**Traceability:**

Owned by `CA-C-020`. Referenced by `AB-C-003`, `AB-C-004`.


### BR-C-014 – Superseded Item Validity

**Rule Statement:** The Superseded Item must be in Active or Inactive state. A Retired item cannot be superseded.

**Evaluation Scope:** Per transition, at registration and activation.

**Enforcement Point:** AB-C-005, AB-C-006.

**Governed Policy:** PO-C-003.

**Outcome When Preconditions Are Not Satisfied:** Registration or activation is rejected.

**Traceability:** Owned by CA-C-021. Referenced by AB-C-005, AB-C-006, DE-C-005.

### BR-C-015 – Superseding Item Validity

**Rule Statement:** The Superseding Item must be in Active state at registration and at activation.

**Evaluation Scope:** Per transition, at registration and activation.

**Enforcement Point:** AB-C-005, AB-C-006.

**Governed Policy:** PO-C-003.

**Outcome When Preconditions Are Not Satisfied:** Registration or activation is rejected.

**Traceability:** Owned by CA-C-021. Referenced by AB-C-005, AB-C-006, DE-C-005.

### BR-C-016 – Single Active Transition per Superseded Item

**Rule Statement:** At most one Active Item Transition shall exist per Superseded Item at any moment.

**Evaluation Scope:** Per transition, at activation.

**Enforcement Point:** AB-C-006.

**Governed Policy:** PO-C-003.

**Outcome When Preconditions Are Not Satisfied:** Activation is rejected. The conflicting transition must be retired first.

**Traceability:** Owned by CA-C-021. Referenced by AB-C-006, DE-C-005.

### BR-C-017 – No Self-Supersession

**Rule Statement:** The Superseded Item and Superseding Item must reference distinct items.

**Evaluation Scope:** Per transition, at registration.

**Enforcement Point:** AB-C-005.

**Governed Policy:** PO-C-003.

**Outcome When Preconditions Are Not Satisfied:** Registration is rejected.

**Traceability:** Owned by CA-C-021. Referenced by AB-C-005, DE-C-005.

---

## Eligibility Rules

| ID | Rule |
|---|---|
| `BR-C-007` | Picture composition requires a valid, Active Planning Scope. |
| `BR-C-008` | Exception evidence requires a valid, non-empty Constraint Reference. |

### BR-C-007 — Picture Composition Requires Valid Planning Scope

**Rule Statement:**

Picture composition requires a valid, Active Planning Scope.

**Rule:**

The Planning Scope referenced by the composition request must exist and be in Active state. If the scope is Inactive or Retired, composition is not permitted.

**Evaluation Scope:**

Per composition request.

**Enforcement Point:**

`FS-C-001 Compose Enterprise Picture` preconditions.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

Composition is rejected. The Planning Scope must be Active.

**Traceability:**

Owned by `CA-C-019`. Referenced by `FS-C-001`, `AB-C-001`.

### BR-C-008 — Exception Evidence Requires Valid Constraint Reference

**Rule Statement:**

Exception evidence requires a valid, non-empty Constraint Reference.

**Rule:**

The Constraint Reference in exception detection or resolution evidence must be non-empty and resolve to an authoritative governance artifact identifier governed by the Constitution, ARS, Enterprise Semantic Model, a ratified domain specification, a Business Rule, or a Policy.

**Evaluation Scope:**

Per evidence item.

**Enforcement Point:**

`FS-C-003`, `FS-C-004`, `AB-C-003`, `AB-C-004`.

**Governed Policy:**

`PO-C-002`.

**Outcome When Preconditions Are Not Satisfied:**

The evidence is rejected with reason code `InvalidConstraintReference`.

**Traceability:**

Owned by `CA-C-020`. Referenced by `FS-C-003`, `FS-C-004`, `AB-C-003`, `AB-C-004`.

## Behavior Rules

| ID | Rule |
|---|---|
| `BR-C-009` | Materiality assessment is governed by `PO-C-001`. |
| `BR-C-010` | Exception severity assessment is governed by `PO-C-002`. |
| `BR-C-015` | Publication evaluation applies to the highest Version Number Draft PictureVersion for the Planning Scope. |

### BR-C-009 — Materiality Assessment Governed by Policy

**Rule Statement:**

Materiality assessment is governed by `PO-C-001`.

**Rule:**

The thresholds and criteria for determining whether a picture change is material are defined exclusively in `PO-C-001`. The algorithm `BA-C-001` applies these thresholds; it does not define them.

**Evaluation Scope:**

Per materiality assessment.

**Enforcement Point:**

`DE-C-001 Assess Picture Materiality`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If `PO-C-001` is missing, the materiality assessment is not applicable.

**Traceability:**

Owned by `CA-C-019`. Referenced by `DE-C-001`, `BA-C-001`.

### BR-C-010 — Exception Severity Assessment Governed by Policy

**Rule Statement:**

Exception severity assessment is governed by `PO-C-002`.

**Rule:**

The severity assessment dimensions and criteria are defined exclusively in `PO-C-002`. Core Exception Management applies these dimensions; it does not define them.

**Evaluation Scope:**

Per exception detection evidence.

**Enforcement Point:**

`DE-C-002 Evaluate Exception Evidence`.

**Governed Policy:**

`PO-C-002`.

**Outcome When Preconditions Are Not Satisfied:**

If `PO-C-002` is missing or not current, exception evidence processing is rejected with reason code `MissingExceptionGovernancePolicy`.

**Traceability:**

Owned by `CA-C-020`. Referenced by `DE-C-002`, `AB-C-003`.

### BR-C-015 — Latest Draft Publication Evaluation

**Rule Statement:**

Publication evaluation applies to the highest Version Number Draft PictureVersion for the Planning Scope.

**Rule:**

When multiple Draft PictureVersions exist, publication evaluation and publication action apply only to the Draft PictureVersion with the highest Version Number.

**Evaluation Scope:**

Per publication evaluation.

**Enforcement Point:**

`AB-C-002 Publish Enterprise Picture Version`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If no Draft PictureVersion exists, publication is rejected.

**Traceability:**

Owned by `CA-C-019`. Referenced by `AB-C-002`, `FS-C-002`.

## Derivation Rules

| ID | Rule |
|---|---|
| `BR-C-011` | Picture snapshot references are derived by evaluating the Planning Scope Boundary Rules against current enterprise reality. |
| `BR-C-012` | For first publication, the materiality result is always `Material Change`. |
| `BR-C-013` | A reference set is Material when any governed threshold in `PO-C-001` is crossed. |
| `BR-C-014` | Materiality assessment reports each reference set independently. |

### BR-C-011 — Picture Snapshot References Derived from Scope Boundary

**Rule Statement:**

Picture snapshot references are derived by evaluating the Planning Scope Boundary Rules against current enterprise reality.

**Rule:**

The demand, supply, and inventory references included in a PictureVersion are determined by evaluating the Planning Scope's Boundary Rules against the current state of Demand, Supply, and Inventory. Only references that satisfy inclusion rules and do not satisfy exclusion rules are included.

**Evaluation Scope:**

Per picture composition.

**Enforcement Point:**

`AB-C-001 Compose Enterprise Picture Version`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If Boundary Rules cannot be evaluated, the composition fails.

**Traceability:**

Owned by `CA-C-019`. Referenced by `AB-C-001`, `FS-C-001`.

### BR-C-012 — First Publication Materiality

**Rule Statement:**

For first publication, the materiality result is always `Material Change`.

**Rule:**

If no Published PictureVersion exists for the Planning Scope, the materiality assessment must produce `HasMaterialChange = true`.

**Evaluation Scope:**

Per materiality assessment.

**Enforcement Point:**

`BA-C-001 Evaluate Picture Materiality`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If no Draft PictureVersion exists, the algorithm is not applicable.

**Traceability:**

Owned by `CA-C-019`. Referenced by `BA-C-001`, `DE-C-001`, `AB-C-002`.

### BR-C-013 — Reference Set Materiality

**Rule Statement:**

A reference set is Material when any governed threshold in `PO-C-001` is crossed.

**Rule:**

For each reference set — Demand, Supply, Inventory — the materiality result is Material when any governed threshold for that reference set is crossed.

**Evaluation Scope:**

Per materiality assessment.

**Enforcement Point:**

`BA-C-001 Evaluate Picture Materiality`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If a reference set cannot be evaluated, it is marked Not Applicable with a reason code.

**Traceability:**

Owned by `CA-C-019`. Referenced by `BA-C-001`, `DE-C-001`, `AB-C-002`.

---

### BR-C-014 — Independent Reference Set Reporting

**Rule Statement:**

Materiality assessment reports each reference set independently.

**Rule:**

Demand, Supply, and Inventory reference sets are evaluated independently. If multiple reference sets are Material, each is independently reported.

**Evaluation Scope:**

Per materiality assessment.

**Enforcement Point:**

`BA-C-001 Evaluate Picture Materiality`.

**Governed Policy:**

`PO-C-001`.

**Outcome When Preconditions Are Not Satisfied:**

If a reference set cannot be evaluated, it is marked Not Applicable with a reason code.

**Traceability:**

Owned by `CA-C-019`. Referenced by `BA-C-001`, `DE-C-001`, `AB-C-002`.

---

# Chapter 8 — Policy Model

## PO-C-001 — Enterprise Picture Publication Governance

**Purpose:**

Govern materiality thresholds, publication cadence, and publication requirements for Enterprise Picture publication.

**Governance Intent:**

Ensure that Enterprise Pictures are published at appropriate frequency and only when material changes warrant publication, while guaranteeing that no Planning Scope remains without a published picture for longer than the maximum staleness interval.

**Governance Outcome:**

Enterprise Pictures are published on a governed cadence with materiality-based triggering, ensuring downstream capabilities always have a current authoritative baseline.

**Scope:**

All Planning Scopes. Applies to `DE-C-001`, `AB-C-001`, `AB-C-002`, `FS-C-001`, `FS-C-002`, and `BA-C-001`.

### Governed Configuration

| Parameter | Value | Description |
|---|---|---|
| Materiality threshold — Demand | Configurable, initial governed value: 5% change in demand reference count | Minimum change in demand references to trigger material change. |
| Materiality threshold — Supply | Configurable, initial governed value: 5% change in supply reference count | Minimum change in supply references to trigger material change. |
| Materiality threshold — Inventory | Configurable, initial governed value: 5% change in inventory reference count | Minimum change in inventory references to trigger material change. |
| Maximum publication interval | 24 hours, configurable per Planning Scope | Maximum allowed gap between Published versions. |
| Staleness warning threshold | 12 hours | If no publication occurs within this window, a governance warning is issued. |
| Completeness requirement | Not enforceable | No completeness threshold shall be enforced until an authoritative Expected Reference Set definition is ratified. |

### Authority Specification Contract

| Section | Value |
|---|---|
| Business Owner | `CA-C-019 Enterprise Picture Management` |
| Authoritative Representation | The enterprise definition of picture publication governance. |
| Business Responsibility | Govern publication frequency and materiality criteria. |
| Authority Scope | Enterprise-wide. |
| Intended Consumers | `DE-C-001`, `AB-C-002`, `FS-C-001`, `FS-C-002`, `BA-C-001`. |
| Non-Intended Consumers | None. |
| Supersedes | None. |
| Superseded By | None. |

### Lifecycle Specification Contract

| State | Description |
|---|---|
| Active | Policy is in effect. |
| Deprecated | Still valid but planned for replacement. |
| Retired | No longer in effect. |

**Terminal State:** Retired.

**History Preservation:** All versions retained permanently.

**Versioning Rules:** Changes to thresholds or cadence require a new policy version.

### Governed Rules

- `BR-C-009`
- `BR-C-011`
- `BR-C-012`
- `BR-C-013`
- `BR-C-014`
- `BR-C-015`

**Exceptional Conditions**

If the Enterprise Picture cannot be published within the maximum publication interval, the scheduled composition failure must be recorded as a governance violation of `PO-C-001`.

No force-published picture is authorized unless a separately ratified override policy exists.

The completeness requirement is not enforceable until an authoritative Expected Reference Set definition is ratified.

**Traceability**

**Owned By:** `CA-C-019`.

**Referenced By:** `DE-C-001`, `AB-C-001`, `AB-C-002`, `FS-C-001`, `FS-C-002`, `BA-C-001`.

## PO-C-002 — Enterprise Exception Governance

**Purpose:**

Govern the exception classification taxonomy, severity assessment rules, deduplication criteria, and resolution criteria for the centralized exception registry.

**Governance Intent:**

Ensure that all enterprise constraint breaches are recorded consistently, deduplicated correctly, assessed for severity uniformly, and resolved through governed criteria.

**Governance Outcome:**

The centralized exception registry maintains a single, consistent, deduplicated record of all enterprise constraint breaches with uniform severity assessment.

**Scope:**

All authorized Intelligence domains publishing exception evidence to Core Intelligence. Applies to `DE-C-002`, `DE-C-003`, `AB-C-003`, `AB-C-004`, `FS-C-003`, and `FS-C-004`.

### Governed Configuration

| Parameter | Value | Description |
|---|---|---|
| Exception classification taxonomy | Governed by `SE-C-037 Enterprise Governed Vocabulary` | The set of recognized exception classifications. |
| Severity dimensions | Business Impact, Urgency, Scope | Dimensions assessed for each exception. |
| Severity levels | Critical, High, Medium, Low | Recognized severity levels. These levels must exist as Active Vocabulary Entries in `SE-C-037`. |
| Deduplication criteria | `Constraint Reference + Affected Scope Type + Affected Scope Identifier` | Composite key for deduplication. |
| Resolution criteria | Domain-published resolution evidence | Criteria for transitioning an exception to Resolved. |
| Severity update rule | Higher severity prevails | If new evidence has higher severity, the exception severity is updated. |

### Authority Specification Contract

| Section | Value |
|---|---|
| Business Owner | `CA-C-020 Core Exception Management` |
| Authoritative Representation | The enterprise definition of exception governance. |
| Business Responsibility | Govern exception lifecycle, deduplication, and severity assessment. |
| Authority Scope | Enterprise-wide. |
| Intended Consumers | `DE-C-002`, `DE-C-003`, `AB-C-003`, `AB-C-004`, authorized Intelligence domains. |
| Non-Intended Consumers | None. |
| Supersedes | None. |
| Superseded By | None. |

### Lifecycle Specification Contract

| State | Description |
|---|---|
| Active | Policy is in effect. |
| Deprecated | Still valid but planned for replacement. |
| Retired | No longer in effect. |

**Terminal State:** Retired.

**History Preservation:** All versions retained permanently.

**Versioning Rules:** Changes to taxonomy, severity rules, or deduplication criteria require a new policy version.

### Governed Rules

- `BR-C-002`
- `BR-C-005`
- `BR-C-006`
- `BR-C-008`
- `BR-C-010`

**Exceptional Conditions**

If exception detection evidence contains an unrecognized Exception Classification, the evidence is rejected with reason code `UnrecognizedExceptionClassification`.

If exception detection evidence contains an unrecognized Severity, the evidence is rejected with reason code `UnrecognizedSeverity`.

If exception resolution evidence is received for an already-resolved Exception, the evidence is rejected with reason code `AlreadyResolved`.

If exception evidence is received while `PO-C-002` is missing or not current, the evidence is rejected with reason code `MissingExceptionGovernancePolicy`.

**Traceability**

**Owned By:** `CA-C-020`.

**Referenced By:** `DE-C-002`, `DE-C-003`, `AB-C-003`, `AB-C-004`, `FS-C-003`, `FS-C-004`.

---


## PO-C-003 – Item Transition Governance

**Purpose:** Govern the validation criteria, activation rules, and transition type taxonomy for item succession relationships.

**Governance Intent:** Ensure that item transitions are validated consistently, activated only when all criteria are met, and classified using a governed taxonomy.

**Governance Outcome:** Item Transitions are registered, validated, activated, and retired under consistent governance. All transition types and history mapping rules are governed classifications.

**Scope:** All Item Transitions (SE-C-040). Applies to AB-C-005, AB-C-006, AB-C-007, DE-C-005, FS-C-005, FS-C-006, FS-C-007.

**Governed Configuration**

| Parameter | Value | Description |
|-----------|-------|-------------|
| Transition Type taxonomy | Governed by SE-C-037 | Recognized transition types (e.g., DirectReplacement, PhaseInPhaseOut, Merge). |
| History Mapping Rule taxonomy | Governed by SE-C-037 | Recognized history mapping rules (e.g., FullTransfer, WeightedTransfer, NoTransfer). |
| Activation validation | All BR-C-014 through BR-C-017 must pass | Criteria for transition activation. |
| Substitution eligibility default | Configurable (default: true) | Default substitution eligibility when not explicitly specified. |

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| **Business Owner** | Manage Item Transitions (CA-C-021) |
| **Authoritative Representation** | The enterprise's definition of item transition governance. |
| **Business Responsibility** | Govern transition validation, activation, and retirement criteria. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | CA-C-021, Demand Intelligence, Supply Intelligence, Promise Intelligence. |

**Lifecycle Specification Contract**

| State | Description |
|-------|-------------|
| Active | Policy is in effect. |
| Deprecated | Still valid but planned for replacement. |
| Retired | No longer in effect. |

- Terminal State: Retired.
- History Preservation: All versions retained permanently.

**Governed Rules:** BR-C-013, BR-C-014, BR-C-015, BR-C-016, BR-C-017.

**Exceptional Conditions:**
- If the Superseding Item is retired after transition activation, the transition remains Active but is flagged for stewardship review.
- If a conflicting transition is discovered after activation, the later-activated transition is flagged for immediate stewardship review.

**Traceability:**
- **Owned By:** CA-C-021.
- **Referenced By:** AB-C-005, AB-C-006, AB-C-007, DE-C-005, FS-C-005, FS-C-006, FS-C-007.

---

# Chapter 9 — Functional Specifications

## FS-C-001 — Compose Enterprise Picture

**Realises:** `CR-C-001`

### Business Contract

| Section | Value |
|---|---|
| Consumes | `SE-C-010 Planning Scope`, `SE-C-038 Scope Boundary Rule`, `SE-C-013 Demand`, `SE-C-014 Supply`, `SE-C-015 Inventory`. |
| Produces | `SE-C-021 Enterprise Picture` — new PictureVersion in Draft state. |
| Transitions | `SE-C-021`: existing aggregate → new Draft version. |
| Publishes | `EV-C-001 Enterprise Picture Version Composed`. |
| Invokes | `AB-C-001`. |
| Guarantees | A Draft PictureVersion exists with all references that satisfy the Planning Scope boundary. |

### Trigger

Scheduled cadence governed by `PO-C-001`.

**Preconditions**

| Rule | Label |
|---|---|
| `BR-C-007` | Planning Scope is Active. |
| `PO-C-001` | Publication governance policy is current. |

### Semantic Objects

**Read:**

- `SE-C-010`
- `SE-C-038`
- `SE-C-013`
- `SE-C-014`
- `SE-C-015`

**Create/Update:**

- `SE-C-021` — new PictureVersion in Draft state.

### Behavior

1. Validate that the Planning Scope is Active.
2. Load the Boundary Rules for the Planning Scope.
3. Invoke `AB-C-001 Compose Enterprise Picture Version`.
4. `AB-C-001` creates a Draft PictureVersion.
5. `AB-C-001` publishes `EV-C-001`.

### Business Transaction

Per `AB-C-001` contract. Protects the Enterprise Picture aggregate.

**Postconditions**

- A Draft PictureVersion exists.
- `EV-C-001` has been published.
- No Published PictureVersion is changed.

### Failure Behavior

- Business Failure: Planning Scope not Active. Composition rejected.
- Operational Failure: Demand, Supply, or Inventory data unavailable. Composition fails and is retryable.

### Recovery Behavior

Re-execution with the same scheduled occurrence and same enterprise facts produces the same Draft content and does not create duplicate effects.

### Concurrency Guarantees

Composition for different Planning Scopes is independent. Composition for the same Planning Scope is serialized.

### Example

A governed schedule triggers composition for Planning Scope `PS-001`. Boundary Rules are loaded. Demand, Supply, and Inventory facts satisfying the boundary are included. `AB-C-001` creates Draft PictureVersion `28`. `EV-C-001` is published with Composition Trigger Time and Composition Timestamp.

**Traceability**

- Realises: `CR-C-001`
- Invokes: `AB-C-001`
- Publishes: `EV-C-001`
- Referenced by: `CA-C-019`

## FS-C-002 — Evaluate and Publish Enterprise Picture

**Realises:** `CR-C-002`

### Business Contract

| Section | Value |
|---|---|
| Consumes | Latest Draft PictureVersion of `SE-C-021`. |
| Produces | Published PictureVersion when material change exists. |
| Transitions | `SE-C-021`: Draft → Published; previous Published → Superseded. |
| Publishes | `EV-C-002 Enterprise Picture Version Published` when publication occurs. |
| Business Notification | `BN-C-001 Enterprise Picture Published` after `EV-C-002` exists. |
| Invokes | `AB-C-002`. |
| Guarantees | Exactly one Published PictureVersion per Planning Scope. Published version is immutable. |

### Trigger

`EV-C-001 Enterprise Picture Version Composed`.

**Preconditions**

| Rule | Label |
|---|---|
| `BR-C-015` | A Draft PictureVersion exists for the Planning Scope identified by `EV-C-001`. |
| `PO-C-001` | Publication governance policy is current. |

### Semantic Objects

**Read:**

- `SE-C-021` — latest Draft PictureVersion; current Published PictureVersion, if present.

**Update:**

- `SE-C-021` — Draft → Published; previous Published → Superseded, when material change exists.

### Behavior

1. Invoke `AB-C-002 Publish Enterprise Picture Version`.
2. `AB-C-002` invokes `BA-C-001`.
3. `AB-C-002` invokes `DE-C-001`.
4. If the decision is `Material Change`:
   - `AB-C-002` publishes the Draft;
   - `AB-C-002` publishes `EV-C-002`.
5. If `EV-C-002` exists, the Business Workflow Notification Node publishes `BN-C-001`.
6. If the decision is `No Material Change`:
   - no publication occurs;
   - no `EV-C-002` is published;
   - no `BN-C-001` is published.

### Business Transaction

Per `AB-C-002` contract. Protects the Enterprise Picture aggregate.

**Postconditions**

If publication occurs:

- Exactly one Published PictureVersion exists.
- The previous Published version is Superseded.
- `EV-C-002` exists.
- `BN-C-001` is published after `EV-C-002`.

If publication does not occur:

- The previous Published version remains authoritative.
- No new Published version exists.
- No `BN-C-001` is published.

### Failure Behavior

- Business Failure: no Draft PictureVersion exists. Publication rejected.
- Operational Failure: notification delivery unavailable. Publication remains committed. `BN-C-001` delivery retried per its delivery guarantee.

### Recovery Behavior

Re-execution for an already Published Draft has no effect.

### Concurrency Guarantees

Publication for a given Planning Scope occurs at most once per Draft version.

### Example

Draft PictureVersion `28` for `PS-001` is material. `AB-C-002` transitions version `28` to Published. Previous version `27` transitions to Superseded. `EV-C-002` is published. The Notification Node publishes `BN-C-001`.

**Traceability**

- Realises: `CR-C-002`
- Invokes: `AB-C-002`
- Publishes: `EV-C-002`, `BN-C-001`
- Referenced by: `CA-C-019`

## FS-C-003 — Process Exception Detection Evidence

**Realises:** `CR-C-003`

### Business Contract

| Section | Value |
|---|---|
| Consumes | Authorized exception detection evidence. Currently declared: `BN-D-022`. |
| Produces | New Active Exception or updated Active Exception. |
| Transitions | `SE-C-019`: none → Active; Active → Active. |
| Publishes | `EV-C-003` for creation; `EV-C-004` for update. |
| Business Notification | `BN-C-002 Enterprise Exception Active` after `EV-C-003` or `EV-C-004` exists. |
| Invokes | `AB-C-003`. |
| Guarantees | Idempotent deduplication. At most one Active Exception per business identity. |

### Trigger

`BN-D-022 Demand Exception Detection Evidence`.

No other notification is authorized.

**Preconditions**

| Rule | Label |
|---|---|
| `BR-C-008` | Constraint Reference is valid and non-empty. |
| `PO-C-002` | Exception governance policy is current. |

### Semantic Objects

**Read:**

- `SE-C-019 Exception` — existing Active exceptions for deduplication.

**Create/Update:**

- `SE-C-019 Exception`.

### Behavior

1. Invoke `AB-C-003 Process Exception Detection Evidence`.
2. `AB-C-003` invokes `DE-C-002`.
3. If the outcome is `Create New Exception`:
   - `AB-C-003` creates a new Active Exception;
   - `AB-C-003` publishes `EV-C-003`.
4. If the outcome is `Update Existing Exception`:
   - `AB-C-003` updates the existing Active Exception;
   - `AB-C-003` publishes `EV-C-004`.
5. If `EV-C-003` or `EV-C-004` exists, the Business Workflow Notification Node publishes `BN-C-002`.
6. If the outcome is `Reject Evidence`:
   - no Exception is created;
   - no Exception is updated;
   - no event is published;
   - the workflow completes with business failure state and governed reason code.

### Business Transaction

Per `AB-C-003` contract. Protects the Exception aggregate.

**Postconditions**

If creation occurs:

- A new Active Exception exists.
- `EV-C-003` exists.
- `BN-C-002` is published.

If update occurs:

- The existing Active Exception is updated.
- No duplicate Exception is created.
- `EV-C-004` exists.
- `BN-C-002` is published.

If rejection occurs:

- No Exception state changes.
- No `BN-C-002` is published.

### Failure Behavior

- Invalid Constraint Reference: reject with `InvalidConstraintReference`.
- Unrecognized Exception Classification: reject with `UnrecognizedExceptionClassification`.
- Unrecognized Affected Scope Type: reject with `UnrecognizedScopeType`.
- Unrecognized Severity: reject with `UnrecognizedSeverity`.
- Missing `PO-C-002`: reject with `MissingExceptionGovernancePolicy`.
- Exception registry unavailable: retryable operational failure.

### Recovery Behavior

Re-execution with the same evidence produces the same result and does not create duplicate Exceptions.

### Concurrency Guarantees

Exceptions for different business identities are independent. Exceptions for the same business identity are serialized.

### Example

Demand Intelligence publishes `BN-D-022` with Constraint Reference `PO-D-041`, Scope Type `PlanningScope`, Scope Identifier `PS-001`, Severity `High`. No Active Exception exists for this business identity. `AB-C-003` creates a new Active Exception. `EV-C-003` is published. `BN-C-002` is published.

**Traceability**

- Realises: `CR-C-003`
- Invokes: `AB-C-003`
- Publishes: `EV-C-003`, `EV-C-004`, `BN-C-002`
- Referenced by: `CA-C-020`

## FS-C-004 — Process Exception Resolution Evidence

**Realises:** `CR-C-004`

### Business Contract

| Section | Value |
|---|---|
| Consumes | Authorized exception resolution evidence. Currently declared: `BN-D-023`. |
| Produces | Resolved Exception. |
| Transitions | `SE-C-019`: Active → Resolved. |
| Publishes | `EV-C-005 Enterprise Exception Resolved`. |
| Business Notification | `BN-C-003 Enterprise Exception Resolved` after `EV-C-005` exists. |
| Invokes | `AB-C-004`. |
| Guarantees | Resolved Exception is immutable. Resolution evidence is recorded. |

### Trigger

`BN-D-023 Demand Exception Resolution Evidence`.

No other notification is authorized.

**Preconditions**

| Rule | Label |
|---|---|
| `BR-C-008` | Constraint Reference is valid and non-empty. |
| `PO-C-002` | Exception governance policy is current. |

### Semantic Objects

**Read:**

- `SE-C-019 Exception`.

**Update:**

- `SE-C-019 Exception` — Active → Resolved.

### Behavior

1. Invoke `AB-C-004 Process Exception Resolution Evidence`.
2. `AB-C-004` invokes `DE-C-003`.
3. If the outcome is `Resolve Exception`:
   - `AB-C-004` transitions the Exception from Active to Resolved;
   - `AB-C-004` records Resolution Evidence and Resolution Time;
   - `AB-C-004` publishes `EV-C-005`.
4. If `EV-C-005` exists, the Business Workflow Notification Node publishes `BN-C-003`.
5. If the outcome is `Reject Resolution Evidence`:
   - no Exception is resolved;
   - no event is published;
   - the workflow completes with business failure state and governed reason code.

### Business Transaction

Per `AB-C-004` contract. Protects the Exception aggregate.

**Postconditions**

If resolution occurs:

- The Exception is Resolved.
- The Resolved Exception is immutable.
- `EV-C-005` exists.
- `BN-C-003` is published.

If rejection occurs:

- The Exception state does not change.
- No `BN-C-003` is published.

### Failure Behavior

- No Active Exception: reject with `NoActiveException`.
- Already Resolved: reject with `AlreadyResolved`.
- Invalid Constraint Reference: reject with `InvalidConstraintReference`.
- Missing `PO-C-002`: reject with `MissingExceptionGovernancePolicy`.
- Exception registry unavailable: retryable operational failure.

### Recovery Behavior

Re-execution for an already Resolved Exception has no effect.

### Concurrency Guarantees

Resolutions for different Exceptions are independent. Resolutions for the same Exception are serialized.

### Example

Demand Intelligence publishes `BN-D-023` for Constraint Reference `PO-D-041`, Scope Type `PlanningScope`, Scope Identifier `PS-001`. An Active Exception exists for this business identity. `AB-C-004` transitions the Exception to Resolved. `EV-C-005` is published. `BN-C-003` is published.

**Traceability**

- Realises: `CR-C-004`
- Invokes: `AB-C-004`
- Publishes: `EV-C-005`, `BN-C-003`
- Referenced by: `CA-C-020`

---


## FS-C-005 – Register Item Transition

**Realises:** CR-C-005

**Business Contract:**
- **Consumes:** SE-C-001 Item (superseded and superseding), SE-C-037 (transition type, history mapping rule).
- **Produces:** SE-C-040 Item Transition in Draft state.
- **Transitions:** SE-C-040: (none) → Draft.
- **Publishes:** EV-C-006 Item Transition Registered.
- **Invokes:** AB-C-005.
- **Guarantees:** Draft transition created with all mandatory attributes.

**Trigger:** Governed Stewardship Change Approved.

**Preconditions:** Superseded Item exists and is not Retired. Superseding Item exists and is Active. Superseded ≠ Superseding.

**Semantic Objects:**
- **Read:** SE-C-001, SE-C-037.
- **Create:** SE-C-040.

**Behavior:**
1. Validate Superseded Item and Superseding Item against SE-C-001.
2. Validate Transition Type and History Mapping Rule against SE-C-037.
3. Invoke **AB-C-005 Register Item Transition**.
4. AB-C-005 creates Draft SE-C-040 and publishes EV-C-006.

**Business Transaction:** Per AB-C-005 contract.

**Postconditions:** Draft SE-C-040 exists. EV-C-006 published.

**Failure Behavior:**
- **Business Failure (item not found, item retired, self-supersession):** Registration rejected. EV-C-006 not published.
- **Operational Failure (storage unavailable):** Registration not completed. Retryable.

**Recovery Behavior:** Re-execution with same Transition Identifier produces no duplicate.

**Concurrency Guarantees:** Registrations for different Superseded Items are independent.

**Traceability:** Realises CR-C-005. Invokes AB-C-005. Publishes EV-C-006. Referenced by CA-C-021.

---

## FS-C-006 – Activate Item Transition

**Realises:** CR-C-006

**Business Contract:**
- **Consumes:** SE-C-040 (Draft), SE-C-001 (Superseding Item validation), PO-C-003.
- **Produces:** SE-C-040 Active.
- **Transitions:** SE-C-040: Draft → Active.
- **Publishes:** EV-C-007 Item Transition Activated. BN-C-004 Item Transition Activated.
- **Invokes:** AB-C-006, DE-C-005.
- **Guarantees:** At most one Active transition per Superseded Item.

**Trigger:** Stewardship approval for activation.

**Preconditions:** SE-C-040 is in Draft state. PO-C-003 is current.

**Semantic Objects:**
- **Read:** SE-C-040, SE-C-001, PO-C-003.
- **Update:** SE-C-040.

**Behavior:**
1. Load Draft SE-C-040.
2. Invoke **AB-C-006 Activate Item Transition**.
   - AB-C-006 executes **DE-C-005 Approve Item Transition Activation**.
   - If Activate: transition Draft → Active, publish EV-C-007.
   - If Do Not Activate: Draft retained.
3. If Activated, Workflow Notification Node publishes **BN-C-004 Item Transition Activated**.

**Business Transaction:** Per AB-C-006 contract.

**Postconditions:** If Activated: SE-C-040 is Active. EV-C-007 and BN-C-004 published. If not: Draft retained.

**Failure Behavior:**
- **Business Failure (conflicting transition, superseding item retired):** Activation rejected. Draft retained.
- **Operational Failure (storage unavailable):** Activation not completed. Retryable.

**Recovery Behavior:** Re-execution on already-Active transition terminates immediately.

**Concurrency Guarantees:** Activation for a given Superseded Item is serialized.

**Traceability:** Realises CR-C-006. Invokes AB-C-006, DE-C-005. Publishes EV-C-007, BN-C-004. Referenced by CA-C-021.

---

## FS-C-007 – Retire Item Transition

**Realises:** CR-C-007

**Business Contract:**
- **Consumes:** SE-C-040 (Active).
- **Produces:** SE-C-040 Retired.
- **Transitions:** SE-C-040: Active → Retired.
- **Publishes:** EV-C-008 Item Transition Retired. BN-C-005 Item Transition Retired.
- **Invokes:** AB-C-007.
- **Guarantees:** Retired transition no longer governs planning behavior.

**Trigger:** Stewardship approval for retirement.

**Preconditions:** SE-C-040 is in Active state.

**Semantic Objects:**
- **Read:** SE-C-040.
- **Update:** SE-C-040.

**Behavior:**
1. Load Active SE-C-040.
2. Invoke **AB-C-007 Retire Item Transition**.
   - AB-C-007 transitions Active → Retired, publishes EV-C-008.
3. Workflow Notification Node publishes **BN-C-005 Item Transition Retired**.

**Business Transaction:** Per AB-C-007 contract.

**Postconditions:** SE-C-040 is Retired. EV-C-008 and BN-C-005 published.

**Failure Behavior:**
- **Business Failure (transition not Active):** Retirement rejected.
- **Operational Failure (storage unavailable):** Retirement not completed. Retryable.

**Recovery Behavior:** Re-execution on already-Retired transition terminates immediately.

**Concurrency Guarantees:** Retirement for a given transition is serialized.

**Traceability:** Realises CR-C-007. Invokes AB-C-007. Publishes EV-C-008, BN-C-005. Referenced by CA-C-021.

---

# Chapter 10 — Business Algorithms

## BA-C-001 — Evaluate Picture Materiality

### 1. Business Classification

| Attribute | Value |
|---|---|
| Type | Business Algorithm |
| Nature | Deterministic Assessment |
| Domain | Enterprise Picture Management |
| Knowledge Category | Materiality Assessment |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question:

> Is the composed picture materially different from the last published version, warranting a new publication?

### 3. Business Intent

This algorithm operationalizes `PO-C-001`.

It compares the composed Draft PictureVersion with the last published PictureVersion across the three reference sets — Demand, Supply, Inventory — applies the governed materiality thresholds, and produces a structured materiality assessment.

It does not decide whether to publish. It provides the evidence on which `DE-C-001` makes that decision.

### 4. Architectural Principle

This algorithm consumes the governed thresholds defined in `PO-C-001`.

It does not define thresholds.

Changing a threshold is a policy change, not an algorithm change.

### 5. Algebraic Properties

| Property | Value |
|---|---|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 6. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|---|---|---|---|---|
| Draft PictureVersion | `SE-C-021` Draft | The composed snapshot to evaluate. | Yes | Algorithm not applicable. |
| Last Published PictureVersion | `SE-C-021` Published | The current authoritative snapshot for comparison. | No | If absent and this is the first publication, result is Material Change. |
| Materiality thresholds | `PO-C-001` | Governed thresholds for material change detection. | Yes | Algorithm not applicable. |

### 7. Output Contract

| Component | Business Meaning |
|---|---|
| MaterialityAssessment | Per-reference-set results with `HasMaterialChange` flag. Each set — Demand, Supply, Inventory — is marked Material, NotMaterial, or NotApplicable. |

### 8. Behavioral Specification Contract

| Section | Value |
|---|---|
| Preconditions | A Draft PictureVersion exists. `PO-C-001` is current. |
| Business Behavior | Compare the Draft and Published PictureVersions across the three reference sets using the thresholds governed by `PO-C-001`. For first publication, all sets return NotApplicable and `HasMaterialChange` is true. For subsequent publications, each set is evaluated independently. |
| Exceptional Conditions | If a reference set cannot be evaluated because data is missing, it is marked NotApplicable with a reason code. |
| Postconditions | A MaterialityAssessment is produced indicating, for each reference set, whether the change is Material, NotMaterial, or NotApplicable, and an overall `HasMaterialChange` flag. |
| Outcome When Preconditions Are Not Satisfied | If no Draft PictureVersion exists, the algorithm is not applicable. |

### 9. Evaluation Methodology

**First publication:**

Published version is absent.

All three reference sets return NotApplicable.

`HasMaterialChange` is true.

**Subsequent publications:**

For each reference set — Demand, Supply, Inventory — compare the reference counts and specific references between the Draft and Published versions using the thresholds governed by `PO-C-001`.

If any governed threshold is crossed, the set is Material.

If multiple sets are Material, each is independently reported.

### 10. Business Rules

| ID | Rule |
|---|---|
| `BR-C-012` | For first publication, `HasMaterialChange` is always true. |
| `BR-C-013` | A reference set is Material if any governed threshold in `PO-C-001` is crossed. |
| `BR-C-014` | If multiple reference sets are Material, each is independently reported. |
| `PO-C-001` | Governs the thresholds used in the evaluation. |

### 11. Assumptions

This algorithm assumes only that:

- the Draft PictureVersion contains complete reference data produced by `AB-C-001`;
- the Published PictureVersion, when present, is immutable and readable;
- `PO-C-001` provides current materiality thresholds.

### 12. Explainability

Every materiality determination traces back to:

- the specific reference set comparison;
- the governed threshold applied;
- the evidence from the version comparison.

### 13. Postconditions / Guarantees

- A MaterialityAssessment is produced for all three reference sets.
- Each reference set is independently assessed.
- No publication decision is made by this algorithm.

### 14. Traceability

| Attribute | Value |
|---|---|
| Owned By | `CA-C-019 Enterprise Picture Management` |
| Governed By | `PO-C-001` |
| Invoked By | `AB-C-002 Publish Enterprise Picture Version` |
| Referenced By | `FS-C-002` through `AB-C-002` |
| Produces | MaterialityAssessment consumed by `DE-C-001` |
| Depends On | `PO-C-001`, `SE-C-021` |

### Dependency Rule Compliance Verification

| Algorithm | Stage | Depends On | Stage of Dependency | Rule |
|---|---|---|---|---|
| `BA-C-001` | Assessment | `SE-C-021`, `PO-C-001` | Not applicable | Compliant |

No dependency rule violations exist.

---

# Appendix A — Integration Matrix

This matrix is a derived view. The owning contracts in Chapters 4, 5, 6, 7, 8, and 9 are authoritative.

| Publisher | Notification | Consumer | Behavior | Status |
|---|---|---|---|---|
| `CA-C-019 Enterprise Picture Management` | `BN-C-001 Enterprise Picture Published` | Declared consuming domains, as verified in their domain specifications | Consume the authoritative published Enterprise Picture. | Core contract authorized. Consumer contracts unresolved until consuming domain specifications are provided. |
| `CA-C-020 Core Exception Management` | `BN-C-002 Enterprise Exception Active` | Declared consuming domains, as verified in their domain specifications | Cross-domain exception awareness. | Core contract authorized. Consumer declarations required. |
| `CA-C-020 Core Exception Management` | `BN-C-003 Enterprise Exception Resolved` | Declared consuming domains, as verified in their domain specifications | Cross-domain exception resolution awareness. | Core contract authorized. Consumer declarations required. |
| Manage Item Transitions | BN-C-004 Item Transition Activated | Demand Intelligence | Consume transition for demand history transfer and forecasting adjustment. |
| Manage Item Transitions | BN-C-004 Item Transition Activated | Supply Intelligence | Consume transition for procurement phase-out/phase-in planning. |
| Manage Item Transitions | BN-C-004 Item Transition Activated | Promise Intelligence | Consume transition for substitution eligibility in order promising. |
| Manage Item Transitions | BN-C-004 Item Transition Activated | Scenario Intelligence | Consume transition for transition scenario evaluation. |
| Manage Item Transitions | BN-C-005 Item Transition Retired | Demand Intelligence | Update forecasting to remove transition context. |
| Manage Item Transitions | BN-C-005 Item Transition Retired | Supply Intelligence | Update procurement planning to remove transition context. |


# Appendix B — Enterprise Capability Matrix

| # | Capability | Business Algorithms | Decisions | Enterprise Question |
|---|---|---|---|---|
| 1 | `CA-C-019 Enterprise Picture Management` | `BA-C-001` | `DE-C-001` | What is the current, authoritative state of enterprise reality for a given Planning Scope? |
| 2 | `CA-C-020 Core Exception Management` | None | `DE-C-002`, `DE-C-003` | Which enterprise constraints are currently breached, what is their severity, and what is their resolution state? |

# Appendix C — Core Intelligence Pipeline

```text
Enterprise State
(Demand, Supply, Inventory)
        |
        v
Scheduled Enterprise Picture Composition
(AB-C-001)
        |
        v
EV-C-001 Enterprise Picture Version Composed
        |
        v
Publication Materiality Evaluation
(BA-C-001, DE-C-001, AB-C-002)
        |
        +--> No Material Change: no publication
        |
        +--> Material Change
                 |
                 v
        Enterprise Picture Publication
        (AB-C-002)
                 |
                 v
        EV-C-002 Enterprise Picture Version Published
                 |
                 v
        BN-C-001 Enterprise Picture Published
                 |
                 v
        Declared consuming domains

Authorized Exception Detection Evidence
(BN-D-022)
        |
        v
Exception Evidence Evaluation
(DE-C-002, AB-C-003)
        |
        +--> Reject Evidence: business failure with governed reason code
        |
        +--> Create or Update Exception
                 |
                 v
        EV-C-003 / EV-C-004
                 |
                 v
        BN-C-002 Enterprise Exception Active
                 |
                 v
        Declared consuming domains

Authorized Exception Resolution Evidence
(BN-D-023)
        |
        v
Exception Resolution Evaluation
(DE-C-003, AB-C-004)
        |
        +--> Reject Resolution Evidence: business failure with governed reason code
        |
        +--> Resolve Exception
                 |
                 v
        EV-C-005 Enterprise Exception Resolved
                 |
                 v
        BN-C-003 Enterprise Exception Resolved
                 |
                 v
        Declared consuming domains
```

# Appendix D — Cross-Domain Contract Verification

| Contract | Core Intelligence Reference | Status |
|---|---|---|
| `BN-C-001 Enterprise Picture Published` | Published by `CA-C-019` via `FS-C-002`. | Core contract authorized. Consumer contracts unresolved until consuming domain specifications are provided. |
| `BN-D-022 Demand Exception Detection Evidence` | Consumed by `CA-C-020` via `FS-C-003`. | Declared. Publisher contract unresolved until the Demand Intelligence Domain Specification is provided. |
| `BN-D-023 Demand Exception Resolution Evidence` | Consumed by `CA-C-020` via `FS-C-004`. | Declared. Publisher contract unresolved until the Demand Intelligence Domain Specification is provided. |
| `SE-C-019 Exception` lifecycle | Owned by `CA-C-020`. Intelligence domains do not mutate `SE-C-019` directly. | Authorized. |
| `SE-C-021 Enterprise Picture` lifecycle | Owned by `CA-C-019`. Consumers read published versions only. | Authorized. |