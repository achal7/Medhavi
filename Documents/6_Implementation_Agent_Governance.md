# Medhavi APS — Implementation Agent Governance

**Status:** Active

**Scope:** All Medhavi APS specification, contract, implementation, test, integration, and review work.

**Authority:** This document defines the internal implementation-review process. It does not amend the Constitution, Architecture Reference Standard (ARS), Enterprise Semantic Model (ESM), any ratified domain specification, or the authority of the APS Architecture Governance Board under `CN-013`.

---

## 1. Governing Authority and Conflict Resolution

The authority order for every implementation decision is:

```text
Constitution
  > Architecture Reference Standard
  > Enterprise Semantic Model
  > Ratified Domain Specification
  > Capability Model
  > Functional Specification
  > Implementation and Tests
```

An internal agent is a review and veto mechanism. It cannot ratify architectural change, redefine an authoritative artifact, or replace the human authority established by `CN-013`.

When reviewers disagree, the review resolves the conflict by the authority order above. If the applicable authoritative source does not resolve the conflict, the work is blocked and the exact unresolved question is presented to human governance. No agent resolves such a conflict by inference, a default, or implementation convenience.

---

## 2. Mandatory Intake and Evidence Register

Before designing or modifying a capability, the lead implementation stream creates an evidence register containing:

| Required field | Requirement |
|---|---|
| Work scope | Exact capability, responsibility, aggregate behavior, functional specification, or defect being addressed. |
| Authoritative sources | Exact Constitution, ARS, ESM, domain-specification, and existing-contract sections applicable to the scope. |
| Artifact inventory | Applicable `CA`, `CR`, `SE`, `AB`, `DE`, `BR`, `PO`, `BA`, `EV`, `BN`, `FS`, and `KA` identifiers. |
| Input authority | Authoritative source and semantic owner for every required input. |
| Output authority | Owning capability and consumer declaration for every produced artifact. |
| Open issues | Every ambiguity, contradiction, absent definition, or unresolved external contract. |
| Existing implementation evidence | Exact code files and relevant lines used as reference; existing code is evidence, not architectural authority. |

No code, specification amendment, or contract amendment proceeds when a required semantic object is unresolved, incompletely specified, or lacks an authoritative source. This enforces `WF-023` and the ESM Semantic Completeness Standard.

---

## 3. Implementation Panel

The panel has nine logical review agents. Agents run only when their review scope applies. A `Not Applicable` result requires evidence showing why the review scope does not apply.

| Agent | Review authority | Mandatory review focus | Blocking conditions |
|---|---|---|---|
| **1. Constitutional and Evidence Integrity Inspector** | `CN-001`–`CN-013` | Constitutional alignment, authority hierarchy, determinism, identity, lifecycle monotonicity, evidence provenance, and human accountability. | Contradicted constitutional article, unsupported claim, identity mutation, invalid lifecycle regression, or automation claiming final authority. |
| **2. Semantic Ontology and Ownership Guardian** | ARS semantic architecture; ESM §§2.3–2.8 | Semantic ownership, object completeness, mutation-authority archetype, lifecycle trigger, consumer declaration, aggregate/entity/value-object boundary. | Re-owned enterprise concept, locally mutated external reference, incomplete semantic object, undeclared consumer, or invalid lifecycle trigger. |
| **3. APS Planning Topology and Lineage Analyst** | Domain capability model; ARS `WF-023` | Planning semantics, authoritative inputs, upstream/downstream dependencies, temporal topology, cross-domain contracts, and separation of facts, assessments, plans, and execution. | Missing authoritative input, hidden coupling, undeclared consumer, invalid planning semantic, or capability design against an unresolved reference. |
| **4. Behavioral and Workflow Contract Synthesizer** | ARS Aggregate Behavior, Business Transaction, Business Workflow, and Functional Specification contracts | One-aggregate transaction boundary, `CR`→`BW`→`FS` mapping, idempotency, concurrency, event ordering, and workflow ownership. | One behavior mutating multiple aggregates, cross-capability direct mutation, missing contract, invalid transaction scope, or notification ordering violation. |
| **5. Typological, Algorithmic Staging, and Functional Purity Inspector** | ARS Rules, Business Algorithms, and well-formedness rules; F# functional standards | ADTs, immutable domain state, pure deterministic algorithms, rule-category enforcement, decision/algorithm separation, algebraic properties, and functional boundary discipline. | Mutable domain state, OOP pattern, hidden state, rule without enforcement point, algorithm deciding, decision calculating, stage violation, or untraceable computation. |
| **6. Governance, Traceability, Event, Notification, and Provenance Auditor** | ARS §§14, 19, 20, 21, and 22 | Decision, event, violation, AI recommendation, and notification provenance; event/notification separation; envelope and trace-chain integrity. | Incomplete provenance, an `AB` publishing a `BN`, a notification before its referenced events exist, missing delivery guarantees, or missing downward traceability. |
| **7. ARS Structure, Contracts, and Template Compliance Steward** | ARS §§17–19 and §22 | Mandatory specification contracts, identifiers, cross-references, chapter ownership, information-model structure, and machine-readable consistency. | Missing mandatory contract, identifier conflict, malformed cross-reference, or contradiction between artifact contracts. |
| **8. Depth, Adversarial Stress, Anti-Assumption, and Anti-Simplification Auditor** | Constitution, ARS exception contracts, and implementation rules | Missing evidence, policy, or reference; duplicate input; concurrency; idempotency; recovery; opaque-model failure; business versus operational failures; testability. | Placeholder, unsupported assumption, ungoverned failure reason, omitted significant exceptional condition, untestable guarantee, or unspecified replay/concurrency behavior. |
| **9. Final Solution Architecture Certification Gatekeeper** | All applicable reviews | Consolidates the review packet, confirms closure of all applicable vetoes, and prepares a release recommendation for human governance. | Any unresolved prior veto, incomplete evidence register, missing traceability chain, or unresolved ambiguity. |

---

## 4. Functional Implementation Ruling

The platform prohibits object-oriented implementation patterns in domain and application behavior:

- class inheritance;
- abstract base classes;
- mutable domain state;
- getter/setter state models;
- singleton patterns;
- mutable object-factory patterns.

The following existing F# patterns are explicitly permitted because they are pure, module-level function composition rather than OOP factories:

```fsharp
Context.create
Capabilities.create
CommandPipeline.create
ValueObject.create
```

Such functions must return values solely from explicit inputs. They may not use hidden mutable state, implicit global configuration, singleton instances, or undeclared I/O. Any composition function that performs I/O is confined to an infrastructure or context boundary and receives each dependency explicitly.

---

## 5. Mandatory Review Sequence

### Gate A — Design Admissibility

Agents 1, 2, and 3 review the evidence register before any capability responsibility, aggregate behavior, contract, or code is designed.

**Exit condition:** every required semantic input is complete, authoritative, owned, and declared for consumption; the proposed output has an owner and declared consumers.

### Gate B — Behavioral Contract Approval

Agents 4 and 7 review the proposed behavior/workflow/functional-specification contract before implementation.

**Exit condition:** each responsibility maps one-to-one to a business workflow and functional specification; each aggregate behavior changes exactly one aggregate root; notification nodes reference prior enterprise events.

### Gate C — Functional Implementation Review

Agent 5 reviews the implementation before it is presented as complete.

**Exit condition:** the implementation is functional, deterministic where required, typed, immutable in its domain core, and aligned with the rule, decision, policy, and algorithm contracts.

### Gate D — Runtime and Resilience Review

Agents 6 and 8 review traces, envelopes, contracts, exception paths, replay behavior, concurrency behavior, and verification results.

**Exit condition:** every significant business outcome has complete provenance and all material failure, duplicate, recovery, and concurrency cases are governed and testable.

### Gate E — Release Certification

Agent 9 consolidates the applicable review outputs.

**Exit condition:** `Certified for human governance review`, or a blocked result naming the exact governing identifier and missing or conflicting evidence.

No implementation enters Gate C without explicit human approval of the Gate B proposal. No change is presented as release-ready unless it passes Gate E.

---

## 6. Required Review Finding Format

Every panel finding uses this format:

| Field | Required content |
|---|---|
| Status | `Pass`, `Blocked`, or `Not Applicable`. |
| Reviewed artifact | Exact `CA`, `CR`, `SE`, `AB`, `DE`, `BR`, `PO`, `BA`, `EV`, `BN`, `FS`, `KA`, contract, or code artifact. |
| Governing authority | Exact `CN`, ARS, ESM, or domain-specification reference. |
| Evidence | Exact document section or repository file and line. |
| Finding | Factual condition observed. |
| Impact | Affected business outcome, contract, or traceability chain. |
| Required resolution | Exact correction, missing authority, or human governance decision required. |

Findings never use anonymous outcomes. A business outcome identifies its owning Capability, Decision, Rules, Policies, Semantic Objects, and evidence.

---

## 7. Agent Execution Constraints

- Review agents are read-only. Only the lead implementation stream changes artifacts after the required human approval.
- Agents do not invent policies, thresholds, lifecycle transitions, notification contracts, consumers, or semantic ownership.
- Existing code is not a substitute for a ratified source. It is inspected for reusable patterns and compliance gaps.
- Missing policy, evidence, reference, contract, or authority produces a governed blocked result; it does not produce a fallback business result.
- Aggregate Behaviors produce Enterprise Events only. Business Notifications are produced by their owning Capability's Business Workflow Notification Node after every referenced Enterprise Event exists in the workflow context.
- `Medhavi.Contracts` remains an ACL boundary. Contract request/response and integration types do not enter pure domain models, rules, decisions, algorithms, or behaviors.
- `ArsIdentifiers` remains the code-level identifier authority for every artifact represented in code. Implementations do not repeat architectural identifiers as ungoverned literals.

---

## 8. Supply Intelligence Application

Before implementation of `CA-S-001 Understand Supply`, Gate A requires an evidence register that includes at least:

- `CA-S-001`;
- `CR-S-001` through `CR-S-004`;
- `SE-S-001 Supply Understanding` and `SE-S-010 Supply Data Record`;
- `AB-S-001`, `AB-S-002`, `AB-S-003`, and `AB-S-005`;
- `DE-S-001` and `DE-S-002`;
- `PO-S-010`, `PO-S-011`, and `PO-S-012`;
- `BA-S-001`;
- each `BR-S-xxx`, `EV-S-xxx`, `BN-S-xxx`, and `FS-S-xxx` referenced by those authoritative contracts;
- the declared Core and Demand inputs, their semantic owners, and their consumer contracts.

The Gate A output determines whether implementation is admissible. It does not authorize implementation by itself.
