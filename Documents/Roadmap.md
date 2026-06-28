# Medhavi APS — Implementation Roadmap v1.0

**Document Version:** 1.0  
**Last Updated:** July 2026  
**Status:** Planning Complete — Execution Not Started  

This document is the single, authoritative implementation plan for the Medhavi APS. It defines every phase of work, every capability to be built, the MVP scope, the wave structure, and the complete post‑MVP journey. Every item is directly traceable to the five Intelligence Specifications and the Architecture Blueprint.

---

## 1. Progress Dashboard

| Wave | Name | Phases | Status | Completion |
|------|------|--------|--------|------------|
| **1** | Foundation & Core Intelligence | 0–5 | ⬜ Not Started | 0% |
| **2** | Integration, UI & Sensing | 6–7 | ⬜ Not Started | 0% |
| **3** | Collaboration & Execution | 8 | ⬜ Not Started | 0% |
| **4** | Learning, Quality & Advanced AI | 9–10 | ⬜ Not Started | 0% |
| **5** | Autonomous & Agentic APS | 11 | ⬜ Not Started | 0% |

### Wave Progress Bars

```
Wave 1: Foundation & Core Intelligence      ░░░░░░░░░░░░░░░░░░░░ 0%
Wave 2: Integration, UI & Sensing           ░░░░░░░░░░░░░░░░░░░░ 0%
Wave 3: Collaboration & Execution           ░░░░░░░░░░░░░░░░░░░░ 0%
Wave 4: Learning, Quality & Advanced AI     ░░░░░░░░░░░░░░░░░░░░ 0%
Wave 5: Autonomous & Agentic APS            ░░░░░░░░░░░░░░░░░░░░ 0%
```

---

## 2. Guiding Principles

| # | Principle | Impact |
|---|-----------|--------|
| **P1** | Intelligence Specifications are the sole source of truth for business requirements | Every feature traces to a capability, decision, rule, or policy |
| **P2** | Architecture Blueprint defines technical realisation | Bounded contexts, event architecture, AI enablement, deployment model |
| **P3** | MVP is a modular monolith: single process, in‑memory event store, in‑process DomainEventBus | Fast iteration, simple operations, no infrastructure complexity |
| **P4** | DecisionCore is a pure F# library with zero dependencies, built first | Every domain shares scoring, feasibility, reservations, PolicyGate |
| **P5** | Every phase ends with a demonstrable, testable, specification‑traceable increment | No phase ends with “partially done” work |
| **P6** | AI readiness is built in from Phase 0 — not added later | Explainability, traceability, and autonomy contracts are foundational |

---

## 3. MVP Definition

### 3.1 MVP Scope by Domain

| Domain | Total Capabilities | MVP | Post‑MVP |
|--------|-------------------|-----|----------|
| Foundation & Cross‑Cutting | 14 components | 14 | 0 |
| Demand Intelligence | 10 | 7 | 3 |
| Supply Intelligence + PlanningEngine | 14 | 7 | 7 |
| Promise Intelligence | 11 | 7 | 4 |
| Scenario Intelligence | 11 | 7 | 4 |
| Knowledge Intelligence | 11 | 11 | 0 |
| AI Copilot | 3 | 3 | 0 |
| Integration & Host | 4 | 4 | 0 |
| **Total** | **78** | **60** | **18** |

### 3.2 MVP Selection Criteria

A capability is included in the MVP if it satisfies at least one of:

- **C1 — Essential for end‑to‑end flow:** Required to demonstrate demand → supply → promise → scenario → insight.
- **C2 — Required by an included capability:** Hard dependency of a capability already selected for MVP.
- **C3 — Demonstrates AI differentiation:** Showcases explainability, traceability, learning, or the Knowledge Intelligence foundation.
- **C4 — Required for operational credibility:** Without it, the system cannot be meaningfully tested or demonstrated.

A capability is excluded from MVP if:

- **X1 — Optional enhancement:** Adds depth but is not essential for the core value proposition.
- **X2 — Depends on excluded capabilities:** Its dependencies are not in MVP.
- **X3 — High complexity, low MVP impact:** Disproportionate effort relative to its contribution to the MVP demonstration.

### 3.3 Simplified Capabilities in MVP

Two capabilities are implemented with reduced scope in the MVP. Full scope is delivered in Phase 9.

**CA‑SI‑010 Evaluate Supply Quality (MVP scope)**
- PI‑SI‑002 Inventory Turnover, PI‑SI‑003 Days of Supply, PI‑SI‑004 Fill Rate, PI‑SI‑005 Capacity Utilization, PI‑SI‑006 Schedule Adherence, PI‑SI‑010 Supply Plan Adherence, PI‑SI‑011 Backorder Rate, PI‑SI‑012 Stockout Frequency, PI‑SI‑013 Excess & Obsolete Inventory, PI‑SI‑015 Cash‑to‑Cash Cycle Time.

**CA‑SN‑004 Assess Risks (MVP scope)**
- Risk scoring: `Risk Score = Probability (%) × Impact ($)`. Basic deterministic stress tests with pass/fail against risk appetite thresholds. Prioritisation by ROI.

---

## 4. Complete Capability Coverage Matrix

| Capability ID | Capability Name | Phase | Status |
|---------------|-----------------|-------|--------|
| **Foundation** | | | |
| F01–F14 | All shared libraries, DecisionCore, traceability, observability, error handling | 0 | MVP |
| **Demand Intelligence** | | | |
| CA‑DI‑001 | Understand Demand | 1 | MVP |
| CA‑DI‑002 | Forecast Demand | 1 | MVP |
| CA‑DI‑003 | Sense Demand | 7 | Post‑MVP |
| CA‑DI‑004 | Segment Demand | 1 | MVP |
| CA‑DI‑005 | Classify Demand | 1 | MVP |
| CA‑DI‑006 | Prioritize Demand | 1 | MVP |
| CA‑DI‑007 | Evaluate Demand Quality | 1 | MVP (full) |
| CA‑DI‑008 | Detect Demand Exceptions | 7 | Post‑MVP |
| CA‑DI‑009 | Explain Demand Decisions | 1 | MVP |
| CA‑DI‑010 | Learn From Demand | 9 | Post‑MVP |
| **Supply Intelligence** | | | |
| CA‑SI‑001 | Understand Supply | 2 | MVP |
| CA‑SI‑002 | Plan Supply | 2 | MVP |
| CA‑SI‑003 | Manage Inventory | 2 | MVP |
| CA‑SI‑004 | Manage Capacity | 2 | MVP |
| CA‑SI‑005 | Collaborate with Suppliers | 8 | Post‑MVP |
| CA‑SI‑006 | Procure Materials | 8 | Post‑MVP |
| CA‑SI‑007 | Schedule Production | 8 | Post‑MVP |
| CA‑SI‑008 | Manage Distribution | 8 | Post‑MVP |
| CA‑SI‑009 | Sense Supply Changes | 7 | Post‑MVP |
| CA‑SI‑010 | Evaluate Supply Quality | 2 (MVP) + 9 (full) | MVP simplified |
| CA‑SI‑011 | Detect Supply Exceptions | 7 | Post‑MVP |
| CA‑SI‑012 | Explain Supply Decisions | 2 | MVP |
| CA‑SI‑013 | Learn From Supply | 9 | Post‑MVP |
| — | Planning Engine | 2 | MVP |
| **Promise Intelligence** | | | |
| CA‑PI‑001 | Understand Orders | 3 | MVP |
| CA‑PI‑002 | Promise Orders (ATP/CTP) | 3 | MVP |
| CA‑PI‑003 | Manage Allocations | 3 | MVP |
| CA‑PI‑004 | Prioritize Orders | 3 | MVP |
| CA‑PI‑005 | Manage Order Changes | 3 | MVP |
| CA‑PI‑006 | Collaborate with Customers | 8 | Post‑MVP |
| CA‑PI‑007 | Sense Promise Risks | 7 | Post‑MVP |
| CA‑PI‑008 | Evaluate Promise Quality | 3 | MVP (full) |
| CA‑PI‑009 | Detect Promise Exceptions | 7 | Post‑MVP |
| CA‑PI‑010 | Explain Promise Decisions | 3 | MVP |
| CA‑PI‑011 | Learn From Promise | 9 | Post‑MVP |
| **Scenario Intelligence** | | | |
| CA‑SN‑001 | Define Scenarios | 4 | MVP |
| CA‑SN‑002 | Simulate Scenarios | 4 | MVP |
| CA‑SN‑003 | Compare Scenarios | 4 | MVP |
| CA‑SN‑004 | Assess Risks | 4 (MVP) + 9 (full) | MVP simplified |
| CA‑SN‑005 | Recommend Scenario | 4 | MVP |
| CA‑SN‑006 | Collaborate on Scenarios | 8 | Post‑MVP |
| CA‑SN‑007 | Sense Scenario Triggers | 7 | Post‑MVP |
| CA‑SN‑008 | Evaluate Scenario Quality | 4 | MVP (full) |
| CA‑SN‑009 | Detect Scenario Exceptions | 7 | Post‑MVP |
| CA‑SN‑010 | Explain Scenario Decisions | 4 | MVP |
| CA‑SN‑011 | Learn From Scenarios | 9 | Post‑MVP |
| **Knowledge Intelligence** | | | |
| CA‑KN‑001 | Govern Knowledge Graph | 5 | MVP |
| CA‑KN‑002 | Discover Cross‑Domain Patterns | 5 | MVP |
| CA‑KN‑003 | Analyze Root Causes | 5 | MVP |
| CA‑KN‑004 | Manage Improvement Portfolio | 5 | MVP |
| CA‑KN‑005 | Institutionalise Best Practices | 5 | MVP |
| CA‑KN‑006 | Orchestrate Feedback Loops | 5 | MVP |
| CA‑KN‑007 | Maintain Enterprise Memory | 5 | MVP |
| CA‑KN‑008 | Serve Knowledge to AI Agents | 5 | MVP |
| CA‑KN‑009 | Evaluate Knowledge Quality | 5 | MVP |
| CA‑KN‑010 | Explain Knowledge Insights | 5 | MVP |
| CA‑KN‑011 | Learn From Knowledge | 5 | MVP |
| **AI Copilot** | | | |
| A01–A03 | Command Palette, Workspace Actions, Autonomy Contracts | 5 | MVP |
| **Integration & Host** | | | |
| H01–H04 | Nexus, Integration, Hub, Web | 6 | MVP |

---

## 5. Phase Details

### Wave 1 — Foundation & Core Intelligence [MVP]

#### Phase 0 — Foundation (Weeks 1–2)

| ID | Deliverable |
|----|-------------|
| F01 | `Medhavi.Common` |
| F02 | `Medhavi.SharedKernel` (base types, `DomainError`, `Envelope`, `ExecutionContext`) |
| F03 | `Medhavi.DecisionCore` (Scoring, Feasibility, Reservations, Fingerprints, PolicyGate, Autonomy, TimeWindows, PlanningGraph) |
| F04 | `Medhavi.Contracts` (all DTOs, request/response types) |
| F05 | `Medhavi.Infrastructure` (`Repository`, `EnvelopeStoreOps` + in‑memory implementations) |
| F06 | `Medhavi.Configuration` (ARS identifier constants) |
| F07 | ExecutionContext propagation |
| F08 | Logging (`Logger`, `LogContext`, `MailboxLogger`) |
| F09 | Telemetry & Metrics |
| F10 | ActivityTracking (distributed tracing) |
| F11 | HealthCheck module |
| F12 | ExceptionHandling unified with ExecutionContext |
| F13 | CircuitBreaker agent |
| F14 | Error‑to‑telemetry bridge |

#### Phase 1 — Demand Intelligence (Weeks 3–6)

| Capability ID | Capability Name |
|---------------|-----------------|
| CA‑DI‑001 | Understand Demand |
| CA‑DI‑002 | Forecast Demand |
| CA‑DI‑004 | Segment Demand |
| CA‑DI‑005 | Classify Demand |
| CA‑DI‑006 | Prioritize Demand |
| CA‑DI‑007 | Evaluate Demand Quality |
| CA‑DI‑009 | Explain Demand Decisions |

Includes: Demand Measurement Model (PI‑DI‑001–015), Demand Semantic Foundation (all SE‑DI‑xxx).

#### Phase 2 — Supply Intelligence + Planning Engine (Weeks 7–12)

| Capability ID | Capability Name |
|---------------|-----------------|
| CA‑SI‑001 | Understand Supply |
| CA‑SI‑002 | Plan Supply |
| CA‑SI‑003 | Manage Inventory |
| CA‑SI‑004 | Manage Capacity |
| CA‑SI‑010 | Evaluate Supply Quality (MVP scope) |
| CA‑SI‑012 | Explain Supply Decisions |
| — | Planning Engine |

Includes: Supply Measurement Model (MVP PI‑SI‑xxx), Supply Semantic Foundation (all SE‑SI‑xxx).

#### Phase 3 — Promise Intelligence (Weeks 13–17)

| Capability ID | Capability Name |
|---------------|-----------------|
| CA‑PI‑001 | Understand Orders |
| CA‑PI‑002 | Promise Orders (ATP/CTP) |
| CA‑PI‑003 | Manage Allocations |
| CA‑PI‑004 | Prioritize Orders |
| CA‑PI‑005 | Manage Order Changes |
| CA‑PI‑008 | Evaluate Promise Quality |
| CA‑PI‑010 | Explain Promise Decisions |

Includes: Promise Measurement Model (PI‑PI‑001–015), Promise Semantic Foundation (all SE‑PI‑xxx).

#### Phase 4 — Scenario Intelligence (Weeks 18–22)

| Capability ID | Capability Name |
|---------------|-----------------|
| CA‑SN‑001 | Define Scenarios |
| CA‑SN‑002 | Simulate Scenarios |
| CA‑SN‑003 | Compare Scenarios |
| CA‑SN‑004 | Assess Risks (MVP scope) |
| CA‑SN‑005 | Recommend Scenario |
| CA‑SN‑008 | Evaluate Scenario Quality |
| CA‑SN‑010 | Explain Scenario Decisions |

Includes: Scenario Measurement Model (PI‑SN‑001–015), Scenario Semantic Foundation (all SE‑SN‑xxx).

#### Phase 5 — Knowledge Intelligence & AI Copilot (Weeks 23–28)

| Capability ID | Capability Name |
|---------------|-----------------|
| CA‑KN‑001 | Govern Knowledge Graph |
| CA‑KN‑002 | Discover Cross‑Domain Patterns |
| CA‑KN‑003 | Analyze Root Causes |
| CA‑KN‑004 | Manage Improvement Portfolio |
| CA‑KN‑005 | Institutionalise Best Practices |
| CA‑KN‑006 | Orchestrate Feedback Loops |
| CA‑KN‑007 | Maintain Enterprise Memory |
| CA‑KN‑008 | Serve Knowledge to AI Agents |
| CA‑KN‑009 | Evaluate Knowledge Quality |
| CA‑KN‑010 | Explain Knowledge Insights |
| CA‑KN‑011 | Learn From Knowledge |

**AI Copilot:** Command Palette (A01), Workspace Actions (A02), Autonomy Contracts (A03).  
Includes: Knowledge Measurement Model (PI‑KN‑001–015), Knowledge Semantic Foundation (all SE‑KN‑xxx).

### Wave 2 — Integration, UI & Sensing [MVP]

#### Phase 6 — Integration, Host & UI (Weeks 29–32)

| ID | Deliverable |
|----|-------------|
| H01 | `Medhavi.Nexus` — composition root |
| H02 | `Medhavi.Integration` — external adapters, ACL |
| H03 | `Medhavi.Hub` — ASP.NET Core host, REST APIs |
| H04 | `Medhavi.Web` — Bolero UI with Stores and Workspaces |

#### Phase 7 — Sensing & Exception Detection (Weeks 33–36)

| Capability ID | Capability Name | Domain |
|---------------|-----------------|--------|
| CA‑DI‑003 | Sense Demand | Demand |
| CA‑DI‑008 | Detect Demand Exceptions | Demand |
| CA‑SI‑009 | Sense Supply Changes | Supply |
| CA‑SI‑011 | Detect Supply Exceptions | Supply |
| CA‑PI‑007 | Sense Promise Risks | Promise |
| CA‑PI‑009 | Detect Promise Exceptions | Promise |
| CA‑SN‑007 | Sense Scenario Triggers | Scenario |
| CA‑SN‑009 | Detect Scenario Exceptions | Scenario |

### Wave 3 — Collaboration & Execution

#### Phase 8 — Collaboration & Execution (Weeks 37–44)

| Capability ID | Capability Name | Domain |
|---------------|-----------------|--------|
| CA‑SI‑005 | Collaborate with Suppliers | Supply |
| CA‑SI‑006 | Procure Materials | Supply |
| CA‑SI‑007 | Schedule Production | Supply |
| CA‑SI‑008 | Manage Distribution | Supply |
| CA‑PI‑006 | Collaborate with Customers | Promise |
| CA‑SN‑006 | Collaborate on Scenarios | Scenario |

### Wave 4 — Learning, Quality & Advanced AI

#### Phase 9 — Domain Learning & Full Quality (Weeks 45–48)

| Capability ID | Capability Name | Domain |
|---------------|-----------------|--------|
| CA‑DI‑010 | Learn From Demand | Demand |
| CA‑SI‑013 | Learn From Supply | Supply |
| CA‑PI‑011 | Learn From Promise | Promise |
| CA‑SN‑011 | Learn From Scenarios | Scenario |
| CA‑SI‑010 | Evaluate Supply Quality (full scope) | Supply |
| CA‑SN‑004 | Assess Risks (full scope) | Scenario |

#### Phase 10 — Advanced AI & Simulation (Weeks 49–56)

| Deliverable |
|-------------|
| Probabilistic simulation (Monte Carlo) in CA‑SN‑002 |
| Advanced optimisation (MILP/CP‑SAT solvers) |
| Digital twin simulator |
| Full Intelligence Measures (PI‑xxx‑100–199) across all domains |
| Full Operational Measures (PI‑xxx‑200–299) across all domains |

### Wave 5 — Autonomous & Agentic APS

#### Phase 11 — Autonomous & Agentic APS (Weeks 57–64)

| Deliverable |
|-------------|
| Agentic exception resolution within autonomy contracts |
| Continuous planning (event‑driven micro‑replanning) |
| Supply chain graph GNN for risk propagation |
| Full AI Autonomy Level 3 |
| PostgreSQL event store migration |
| External event bus (RabbitMQ/Kafka) |
| RBAC, TLS, secrets management, WAF |
| Full multi‑tenancy isolation |
| Disaster recovery automation |
| Performance optimisation and SLO validation |

---

## 6. Measurement Model Coverage

Every Performance Indicator from all five Intelligence Specifications is allocated to a phase.

| Domain | Business Outcome Measures | Intelligence Measures | Operational Measures | Phase(s) |
|--------|--------------------------|----------------------|---------------------|----------|
| Demand | PI‑DI‑001–015 | PI‑DI‑100–115 | PI‑DI‑200–215 | 1 + 9 + 10 |
| Supply | PI‑SI‑001–015 | PI‑SI‑100–112 | PI‑SI‑200–212 | 2 + 9 + 10 |
| Promise | PI‑PI‑001–015 | PI‑PI‑100–110 | PI‑PI‑200–206 | 3 + 9 + 10 |
| Scenario | PI‑SN‑001–015 | PI‑SN‑100–111 | PI‑SN‑200–205 | 4 + 9 + 10 |
| Knowledge | PI‑KN‑001–015 | PI‑KN‑100–110 | PI‑KN‑200–205 | 5 + 10 |

Business Outcome Measures are fully implemented in the domain’s MVP phase. Intelligence and Operational Measures are defined as stubs in the MVP phase and fully implemented in Phase 10.

---

## 7. Post‑MVP Backlog (Complete)

| Capability ID | Capability Name | Phase |
|---------------|-----------------|-------|
| CA‑DI‑003 | Sense Demand | 7 |
| CA‑DI‑008 | Detect Demand Exceptions | 7 |
| CA‑DI‑010 | Learn From Demand | 9 |
| CA‑SI‑005 | Collaborate with Suppliers | 8 |
| CA‑SI‑006 | Procure Materials | 8 |
| CA‑SI‑007 | Schedule Production | 8 |
| CA‑SI‑008 | Manage Distribution | 8 |
| CA‑SI‑009 | Sense Supply Changes | 7 |
| CA‑SI‑011 | Detect Supply Exceptions | 7 |
| CA‑SI‑013 | Learn From Supply | 9 |
| CA‑PI‑006 | Collaborate with Customers | 8 |
| CA‑PI‑007 | Sense Promise Risks | 7 |
| CA‑PI‑009 | Detect Promise Exceptions | 7 |
| CA‑PI‑011 | Learn From Promise | 9 |
| CA‑SN‑006 | Collaborate on Scenarios | 8 |
| CA‑SN‑007 | Sense Scenario Triggers | 7 |
| CA‑SN‑009 | Detect Scenario Exceptions | 7 |
| CA‑SN‑011 | Learn From Scenarios | 9 |

Plus full scope upgrades for CA‑SI‑010 (Phase 9) and CA‑SN‑004 (Phase 9).

---

## 8. References

- **Medhavi APS Constitution** — Governing principles  
- **Architecture Reference Standard (ARS) v1** — Identifier standards, traceability rules, lifecycle governance  
- **Semantic Model** — Enterprise meaning and intelligence domain definitions  
- **Capability Model** — How the enterprise reasons: primitives, composition, anatomy  
- **Decision Model** — How enterprise choices are made and governed  
- **Rule & Policy Model** — How decisions are validated and governed  
- **Demand Intelligence Specification** — Authoritative business specification for demand  
- **Supply Intelligence Specification** — Authoritative business specification for supply  
- **Promise Intelligence Specification** — Authoritative business specification for order promising  
- **Scenario Intelligence Specification** — Authoritative business specification for scenario planning  
- **Knowledge Intelligence Specification** — Authoritative business specification for cross‑domain learning  
- **Architecture Blueprint** — Technical realisation of all Intelligence Specifications  

---