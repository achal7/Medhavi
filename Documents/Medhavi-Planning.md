# Medhāvī ProductionPlanning - Project Description Document (PDD)

**Product**: Medhāvī ProductionPlanning  
**Version**: 1.0  
**Date**: May 2026  
**Author**: Medhāvī  
**Status**: Draft - Architecture Definition Phase

---

## Table of Contents

1. [Introduction](#1-introduction)
    1.1 [Objectives](#11-objectives)
    1.2 [PDD Structure](#12-pdd-structure)
    1.3 [What to Avoid](#13-what-to-avoid)
    1.4 [Keep it Simple](#14-keep-it-simple)

2. [Expectations](#2-expectations)
    2.1 [Results](#21-results)
    2.2 [In Scope](#22-in-scope)
    2.3 [Out of Scope](#23-out-of-scope)
    2.4 [Performance/Usability](#24-performanceusability)
    2.5 [Technology](#25-technology)
    2.6 [Miscellaneous](#26-miscellaneous)

3. [Business Goals](#3-business-goals)
    3.1 [Goal 1: Optimized Resource Utilization](#31-goal-1-optimized-resource-utilization)
    3.2 [Goal 2: Just-in-Time Material Management](#32-goal-2-just-in-time-material-management)
    3.3 [Goal 3: Advanced Scheduling Optimization](#33-goal-3-advanced-scheduling-optimization)
    3.4 [Goal 4: Total Value and Churn Minimization](#34-goal-4-total-value-and-churn-minimization)

4. [Core Decision Framework (Shared Building Blocks)](#4-core-decision-framework-shared-building-blocks)
    4.1 [What Belongs Here](#41-what-belongs-here)
    4.2 [Shared Types (Code Structure)](#42-shared-types-code-structure)
    4.3 [How to Use](#43-how-to-use)
    4.4 [Concurrency / Reliability](#44-concurrency--reliability)
    4.5 [Telemetry / KPIs](#45-telemetry--kpis)
    4.6 [ML Integration & Closed-Loop Parameter Feedback](#46-ml-integration--closed-loop-parameter-feedback)
    4.7 [Examples](#47-examples)

5. [Scope and Planning Decisions Overview](#5-scope-and-planning-decisions-overview)
    5.1 [Application Architecture: Module Overview](#51-application-architecture-module-overview)
    5.2 [Core Modules Detailed](#52-core-modules-detailed)
    5.3 [Complete Planning Flow: Promise → MRP → Optimization](#53-complete-planning-flow-promise--mrp--optimization)
    5.4 [Event Processing Flow](#54-event-processing-flow)
    5.5 [Planning Decisions Overview (detailed)](#55-planning-decisions-overview-detailed)
    5.6 [KPI Matrix (with meaning)](#56-kpi-matrix-with-meaning)

6. [Planning Decision Details](#6-planning-decision-details)
    6.1 [Material Availability Module](#61-material-availability-module)
    6.2 [Order Acceptance (Promise/ATP)](#62-order-acceptance-promiseatp---heuristic-atpctp-lite)
    6.3 [Material Requirements Planning (MRP)](#63-material-requirements-planning-mrp)
    6.4 [Capacity Assignment](#64-capacity-assignment)
    6.5 [Work Order Planning](#65-work-order-planning)
    6.6 [Material Reservations](#66-material-reservations)
    6.7 [Pegging & Traceability](#67-pegging--traceability)
    6.8 [Routing & Process Management](#68-routing--process-management)
    6.9 [Transport Management](#69-transport-management)
    6.10 [Supplier Management](#610-supplier-management)
    6.11 [Multi-Objective Optimization Module](#611-multi-objective-optimization-module)
    6.12 [Replanning & What-if](#612-replanning--what-if)
    6.13 [Campaign Management](#613-campaign-management)

7. [Other Functionality](#7-other-functionality)
    7.1 [System Health Monitoring](#71-system-health-monitoring)
    7.2 [Configuration Management](#72-configuration-management)
    7.3 [Audit Logging](#73-audit-logging)
    7.4 [Validation Approach](#74-validation-approach)
    7.5 [AI Governance & Autonomous Guardrails](#75-ai-governance--autonomous-guardrails)

8. [Application/IT Environment](#8-applicationit-environment)
    8.1 [Scale and Performance](#81-scale-and-performance)
    8.2 [Security Provisioning](#82-security-provisioning)
    8.3 [Disaster Recovery & Business Continuity](#83-disaster-recovery--business-continuity)
    8.4 [Data Retention & Lifecycle](#84-data-retention--lifecycle)
    8.5 [API Versioning](#85-api-versioning)
    8.6 [Observability](#86-observability)

9. [Integration](#9-integration)
    9.1 [General](#91-general)
    9.2 [Nexus Integration](#92-nexus-integration)
    9.3 [ERP Integration](#93-erp-integration)
    9.4 [MES Integration](#94-mes-integration)

10. [Appendix A: Technical Architecture](#10-appendix-a-technical-architecture)
    10.1 [System Components](#101-system-components)
    10.2 [Data Flow Architecture](#102-data-flow-architecture)
    10.3 [Scalability Analysis](#103-scalability-analysis)
    10.4 [Supply Chain Network Graph](#104-supply-chain-network-graph)
    10.5 [Core Domain Schema Placeholders](#105-core-domain-schema-placeholders)

11. [Appendix B: Terminology](#11-appendix-b-terminology)

---

## 1. Introduction

### 1.1 Objectives

The main aim of this Project Description Document (PDD) is to describe the Medhāvī ProductionPlanning system that serves as the tactical planning engine for Advanced Planning & Scheduling (APS). This document translates our architectural vision into a technical implementation plan for finite capacity scheduling, material requirements planning, and work order optimization.

> **Note**: This PDD focuses on the _what_ and _why_ of the system. For phased implementation planning, wave structure, and progress tracking, see the companion document `Phase-Management.md`. The guiding principles defined there (snapshot-based persistence, strict aggregate API, functional core / imperative shell, planning modes, etc.) are normative and take precedence on architectural decisions.

#### Stakeholder Perspective
Medhāvī ProductionPlanning empowers manufacturers to increase customer satisfaction, delivery performance, and profitability by streamlining complex tactical planning decisions. It balances demand and supply throughout the supply chain, ensuring finite planning of production orders and capacities.

#### Developer Perspective
ProductionPlanning is implemented using a **Functional Core / Imperative Shell** architecture in F#/.NET with specialized optimization engines. Aggregate state is persisted as **snapshots via Marten (PostgreSQL document store)**, with **integration events** published for cross-system communication. ProductionPlanning subscribes to Nexus event streams and maintains domain aggregates for production orders, inventory, and resources.

### 1.2 PDD Structure

This PDD is organized around the core planning decisions and architectural components:

**Expectations**: Essential properties and requirements for the final solution
**Business Goals**: Measurable objectives that the system should achieve
**Scope Overview**: Functional architecture and planning decision flows
**Planning Decisions**: Detailed specifications for each decision point
**Technical Architecture**: System design and scalability considerations

### 1.3 What to Avoid

- Over-engineering for future requirements not yet validated
- Complex integrations before proving core planning capabilities
- Premature optimization without performance baselines
- Feature creep that delays the MVP delivery

### 1.4 Keep it Simple

Following the KIS principle:
- Start with core order acceptance and material planning
- Implement essential optimization algorithms
- Focus on reliable planning results before advanced features
- Validate each planning decision before adding complexity

---

## 2. Expectations

### 2.1 Results

The Medhāvī ProductionPlanning should deliver:
- Sub-500ms planning decision response times for 99th percentile
- 99.5% system availability with <2 hours annual downtime
- Zero data loss with guaranteed event persistence
- Optimal resource utilization across all production assets
- Seamless integration with existing ERP and MES systems

### 2.2 In Scope

#### Phase 1: Core Planning Engine (MVP)
- **Material Availability Query Service**: Projection-backed stock availability.
- **Material Reservations**: Tentative/Confirmed lifecycle to prevent double-booking.
- **Heuristic Order Promising (Promise/ATP)**: Fast, tiered CTP-lite capacity checks (Simple ATP <1s, Standard CTP <5s, Full CTP <30s).
- **Heuristic MRP**: Fast batch netting, BOM explosion, and planned supply proposals.
- **Capacity Assignment**: Finite allocation to resource groups and time periods.
- **Marten/PostgreSQL**: Snapshot-based persistence, CQRS, and read-model projections.
- **REST & WebSockets**: System integration and real-time frontend updates.

#### Phase 2: Advanced AI/ML & Optimization (Post-MVP)
- **Multi-Objective Optimizer**: Nightly CPLEX/OR-Tools MILP global solver.
- **Operational Scheduler**: Reinforcement Learning or Metaheuristic sequencing for sequence-dependent setups, campaigns, and CIP cleaning.
- **Agentic Planner (Orchestrator)**: Automated exception handling and what-if simulation evaluation.
- **Generative AI Co-pilot**: Natural language interface for querying and replanning.
- **Closed-Loop ML Feedback**: Actual vs. planned execution data feeding ML parameter updates.

### 2.3 Out of Scope

- Strategic planning (1-5 year horizons).
- Complete master data management (MDM) governance.
- **Demand Forecasting Engine**: Statistical / ML demand forecasting model training and generation is out of scope. Medhāvī consumes forecast data as an input (via Nexus integration events). However, the system should define:
  - Forecast consumption logic (how customer orders consume/reduce forecast quantities)
  - Forecast accuracy KPIs (MAPE, WAPE, Bias) tracked as telemetry
  - Demand sensing hooks (short-term forecast adjustments from real-time signals) as a Phase 2 integration point
- **Advanced AI/ML Model Training Pipeline**: The actual training of ML models is a separate pipeline (outside this engine). Medhāvī only consumes predictions and updates planning parameters at runtime.
- Multi-tenant isolation (single tenant for MVP).
- Mobile application development.

### 2.4 Performance/Usability

**Computational Performance**
- Planning run execution: <30 minutes for full optimization
- Order acceptance (tiered SLA):
  - Simple ATP (stock check only): <1 second
  - Standard CTP (material + capacity): <5 seconds
  - Full CTP (material + capacity + transport + supplier): <30 seconds
- Schedule updates: <2 seconds for real-time adjustments
- What-if analysis: <10 seconds for scenario evaluation
- System startup: <60 seconds

**Scalability Requirements**
- Operations: Support for 50,000+ operations per planning run
- Resources: Handle 1,000+ resources with capacity constraints
- Time horizon: 3-6 month planning horizon
- Concurrent users: Support 50+ concurrent planning users
- BOM processing: Handle 10,000+ BOM levels efficiently

**Data Processing Requirements**
- Event throughput: Process 1,000+ planning events per minute
- Optimization: Solve complex scheduling problems in real-time
- Memory usage: <4GB under normal load

**User Experience**
- Intuitive web-based planning interface with drag-and-drop capabilities
- Real-time planning updates via WebSocket/SignalR

### 2.5 Technology

- **Runtime**: .NET 10.0, F# primary language
- **Optimization**: Custom algorithms with OR-Tools (CP-SAT / MILP) integration; CPLEX as optional commercial upgrade
- **Persistence**: Marten (PostgreSQL document store) for aggregate snapshots and read-model projections
- **Web Framework**: ASP.NET Core with SignalR
- **Database**: PostgreSQL (via Marten) for domain state and read models
- **Deployment**: Docker containers with Kubernetes orchestration

### 2.6 Miscellaneous

- Open-source licensing (MIT)
- Comprehensive logging and monitoring
- Automated testing with >95% coverage
- CI/CD pipeline with automated deployment
- Documentation and API specifications

---

## 3. Business Goals

### 3.1 Goal 1: Optimized Resource Utilization

**Definition**: Maximize the utilization of production resources (machines, labor, tooling) while respecting capacity constraints and minimizing bottlenecks.

**Motivation**: Efficient resource utilization directly impacts profitability and delivery performance. Under-utilized resources represent wasted investment, while over-utilization leads to quality issues and maintenance problems.

**Current State**: Manual scheduling with 70-80% average utilization
**Target State**: AI-optimized scheduling with 85-95% utilization
**Success Metric**: 90% average resource utilization with <5% bottleneck occurrences

**Technical Success Metrics**
- Schedule quality: 95%+ constraint satisfaction, 90%+ resource utilization
- Optimization accuracy: 85%+ improvement over manual planning
- System reliability: 99.5% uptime for planning operations
- Replanning speed: <2 seconds for schedule adjustments

### 3.2 Goal 2: Just-in-Time Material Management

**Definition**: Maintain optimal inventory levels to support production while minimizing carrying costs and stockouts.

**Motivation**: Proper inventory management ensures production continuity while reducing working capital requirements. Just-in-time principles eliminate waste and improve cash flow.

**Current State**: Manual inventory planning with 20-30% excess inventory
**Target State**: Automated MRP with 95% on-time material availability
**Success Metric**: 98% material availability with <10% excess inventory

**Business Success Metrics**
- Inventory optimization: 98% material availability with <10% excess inventory
- Planning efficiency: 20% reduction in planning effort and time
- Cost reduction: 25% reduction in production costs through optimization

### 3.3 Goal 3: Advanced Scheduling Optimization

**Definition**: Generate optimal production schedules that balance multiple competing objectives including delivery dates, costs, and resource efficiency.

**Motivation**: Complex scheduling decisions require sophisticated algorithms to consider all constraints and objectives simultaneously. Manual scheduling cannot achieve the same level of optimization.

**Current State**: Manual scheduling with frequent replanning.
**Target State**: Automated optimization with real-time adjustments.
**Success Metric**: 95% on-time delivery with 20% reduction in planning effort.

**Business Success Metrics**
- On-time delivery: 95% on-time delivery achievement
- Resource utilization: 90% average utilization across production resources
- Planning efficiency: 20% reduction in planning effort and time

### 3.4 Goal 4: Total Value and Churn Minimization [Phase 2]

**Definition**: Optimize the schedule to maximize overall enterprise value (incorporating ESG metrics such as carbon footprints) while minimizing schedule nervousness (churn).

**Motivation**: Modern manufacturers must balance carbon limits alongside cost and customer service. Additionally, constantly changing scheduled operations (nervousness) causes friction on the shop floor; stabilizing schedules via penalties on previous plan deviation is essential.

**Current State**: No tracking of carbon emissions; manual scheduling suffers from high nervousness/churn (>40% daily shifts).
**Target State**: Optimization model balances cost, CO2, and churn, keeping churn under 15% and carbon footprints minimized.
**Success Metric**: Churn rate <15% per replan; CO2 footprint reduced by 10% vs. unoptimized schedules.

**Success Metrics**
- Schedule Stability: 85%+ operations remain unchanged during minor replanning runs.
- Carbon Efficiency: 10% reduction in transport/production-related carbon emissions.
- Total Value Score: Pareto frontier trade-offs explicitly quantified.

---

## 4. Core Decision Framework (Shared Building Blocks)

This section captures shared logic across ATP/CTP, material/capacity/transport/supplier services, and optimizer reuse. Keep core policies, scoring, limiters, reservations, and cross-cutting concerns here to avoid duplicating business rules.

### 4.1 What Belongs Here
- Policies & SLA presets (time vs cost vs risk caps, buffers, FullOrder/FullDelivery)
- Scoring primitives for routings, itineraries, supplier options (time/cost/risk/CO2)
- Limiters/reason catalog + ProviderError → Limiter mapping
- Reservations lifecycle contracts (create/confirm/release/expire), deterministic IDs
- Time/FX/cost normalization helpers (UTC, as-of rates, stale-rate guard)
- Routing/alternate selection primitives (policy-driven)
- Transport/supplier feasibility & scoring primitives (cutoff/capacity/MOQ/reliability)
- Telemetry hooks (latency, at-risk, cache hit/miss, provider errors)
- Concurrency/degradation patterns (agent wrapper, error-to-limiter)
- ML hooks (lead-time/reliability predictors with basis/version)
- Tenant scoping and UTC normalization (all timestamps UTC; apply tenant policy presets)
- Cache invalidation events (calendars, legs, reservations, allocations)
- Idempotency guidance (if upstream broker provides idempotency/checkpoints, note boundaries)

### 4.2 Shared Types (Code Structure)
- `Common/PromiseTypes.fs` (exists): limiter domains/codes, policies, provider contracts, request/response, reservations, routing choice, itineraries
- `Common/PromisePolicies.fs` (added): default policy, risk-basis helper, policy merge
- `Common/PromiseScoring.fs` (added): scoring helpers for itineraries, suppliers, routings
- `Common/PromiseLimiter.fs` (added): ProviderError → Limiter mapping
- `Common/CostFx.fs` (added): cost breakdown, FX-as-of with stale guard
- `Common/TimeWindow.fs` (added): window overlap/containment, slack, cutoff helper
- `Common/ValidationHelpers.fs` (added): transport feasibility, supplier MOQ/lead, safety check, apply risk
- `Common/TelemetryContracts.fs` (added): standard telemetry events/contracts

### 4.3 How to Use
- Services (material, capacity, transport, supplier) and the Promise Orchestrator consume the shared types and helpers; inject provider implementations
- Optimizer reuses the same policies, scoring, limiters, and reservations contracts—no duplicated rules in solver code
- Light vs full: inject trivial providers (zero reservations/transport) for heuristic mode; richer providers for full ATP/CTP

### 4.4 Concurrency / Reliability
- Optional Mailbox/agent wrapper to serialize promise requests; reservations are idempotent to avoid race issues
- ProviderError maps to Limiter with retry/alternate suggestions; degrade gracefully, never leave dangling reservations

### 4.5 Telemetry / KPIs
- Use TelemetryProvider (or TelemetryContracts) for latency per provider, errors, cache hit/miss, timeouts, promise accuracy/at-risk

### 4.6 ML Integration & Closed-Loop Parameter Feedback [Phase 2]

**1. Predictor Consumption Pattern**
- Providers call external ML predictors (e.g., predicting transport leg lead times or supplier reliability) via REST endpoints.
- ML predictions include a basis and version (e.g., model ID, version, and confidence interval basis: p50/p95).
- The Promise Orchestrator enforces the requested policy preference (e.g., RiskPreference=P95 for gold customers, P50 for standard).
- On ML service failure, the system falls back gracefully to default master data values, maps a ProviderError to a Limiter, and logs the incident.

**2. Closed-Loop Feedback Architecture (Self-Healing Master Data)**
To prevent planning model drift, the system implements a closed-loop feedback loop:
- **Telemetry Capture**: During execution, the system captures actual vs. planned data by subscribing to completion events from MES (`WorkOrderCompleted`) and ERP (`MaterialReceived`).
- **Deviation Emission**: The system calculates the variance: `Variance = ActualDuration - PlannedDuration`. If `Variance` exceeds a policy-defined threshold (e.g., 2 standard deviations), it emits a `PlanningDeviationDetected` event containing:
  - ProductId, ResourceId/SupplierId, PlannedDuration, ActualDuration, ExecutionDate, and Context.
- **Model Re-training Pipeline**: A separate ML training service consumes these deviation events to re-train the models.
- **Parameter Dynamic Update**: Once re-trained, the ML service publishes updated parameters (e.g., dynamic safety stocks, supplier lead times) back to Medhāvī via the API.
- **Cache Invalidation**: The update triggers a cache invalidation event (e.g., `SupplierLeadTimeUpdated`), forcing read models and optimization buckets to rebuild with the new, accurate parameters.

### 4.7 Examples
- Policy preset: Gold customer → TimePreference=Fastest, RiskPreference=P95, CostCap=None, CallSupplierOnShortfall=true
- Cache bust: ResourceCalendar updated → emit CalendarChanged event → capacity cache invalidated and rebuilt
- Limiter mapping: Transport provider timeout → Limiter.Domain=Transport, Code=SearchTimeout, Suggestions=[retry,fallbackHeuristic]

---

## 5. Scope and Planning Decisions Overview

### 5.1 Application Architecture: Module Overview

Medhāvī ProductionPlanning consists of **8 core execution modules** layered under a **Cognitive AI & Orchestration Layer**. This architecture ensures a separation of concerns: execution modules manage hard constraints and transactions, while the cognitive layer coordinates scenarios, exception handling, and user interaction.

#### 5.1.1 Architecture & Layering Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                    COGNITIVE AI & ORCHESTRATION LAYER (Phase 2)         │
│                                                                         │
│   ┌──────────────────────────┐             ┌────────────────────────┐   │
│   │ Generative AI Co-pilot   │             │ Agentic Resolution     │   │
│   │ (Natural Language / NLP) │ <─────────> │ Orchestrator (What-If) │   │
│   └─────────────┬────────────┘             └───────────┬────────────┘   │
└─────────────────┼──────────────────────────────────────┼────────────────┘
                  │ Queries & Scenarios                  │ Coordinates Delta Planning
                  ▼                                      ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                    APS CORE EXECUTION ENGINE                            │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  1. MATERIAL AVAILABILITY (Query Service) [MVP]                  │   │
│  │     Purpose: Provide net stock position information              │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Netting & Holds                          │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  2. MATERIAL RESERVATION (Hold/Commit) [MVP]                     │   │
│  │     Purpose: Prevent double-booking (Tentative/Confirmed)        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Capacity Checks                          │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  3. CAPACITY CTP (Capacity Checking) [MVP]                       │   │
│  │     Purpose: Check bucket availability and routing constraints   │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Logistics Checks                         │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  4. TRANSPORT ATP (Logistics Checking) [MVP]                     │   │
│  │     Purpose: Check leg capacity, schedules, and transit lead time│   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ ATP/CTP Inputs                           │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  5. PROMISE/ATP (Order Promising) [MVP]                          │   │
│  │     Purpose: Tiered heuristic order acceptance (1s/5s/30s)       │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Demands & Netting                        │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  6. MRP (Material Requirements Planning) [MVP]                   │   │
│  │     Purpose: Batch netting, BOM explosion, and supply generation │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Safety/Min Stock Targets                 │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  7. MATERIAL REPLENISHMENT (Stock Management) [MVP]              │   │
│  │     Purpose: Maintain stock targets and trigger MRP on shortfall │   │
│  └──────────────────────────────────────────────────────────────────┘   │
│                              ▲                                          │
│                              │ Global Optimization Run                  │
│                              ▼                                          │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │  8. OPTIMIZATION (Multi-Objective Solver) [Phase 2]              │   │
│  │     Purpose: Global MILP (CPLEX) + Local Sequencing (RL/Heuristic)│   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
```

#### 5.1.2 Module Roadmap: MVP (Phase 1) vs. Advanced (Phase 2)

To reduce risk and accelerate time-to-market, the system is designed in two sequential phases:

##### Phase 1: Core Execution Engine (MVP)
The MVP establishes the snapshot-based transactional core and fast heuristic planning rules. It includes:
1. **Material Availability**: Live query service of on-hand inventory, firm inbound supplies, active reservations, and safety floors.
2. **Material Reservation**: State machine for booking material space. Essential for preventing double-booking on concurrently processed orders.
3. **Capacity CTP**: Standard work routing step durations checked against bucketed resource calendars.
4. **Transport ATP**: Multi-hop transit routing leg schedules, lead times, and capacity checks.
5. **Promise/ATP**: Real-time heuristic orchestrator. When an order is requested, it queries the four modules above, computes the promise date under tiered SLAs (Simple ATP <1s, Standard CTP <5s, Full CTP <30s), and creates tentative reservations. No heavy solver runs on the online path.
6. **Heuristic MRP**: Nightly or on-demand batch run executing BOM explosion, inventory netting, and proposing planned supply orders (PO/WO/TO) pegged to demands.
7. **Material Replenishment**: Triggers MRP proposals when projected inventory drops below safety limits.

##### Phase 2: Cognitive AI & Optimization
Phase 2 overlays AI-based optimization and planner co-pilots:
8. **Multi-Objective Optimization**: Nightly optimization solver using CPLEX/OR-Tools to globally re-allocate material and capacity, minimizing lateness, CO2, and churn.
9. **Operational Scheduler (Reinforcement Learning / Metaheuristic)**: Sub-module of Optimization. Resolves sequence-dependent setups, changeover matrices, and clean-in-place (CIP) constraints at the line-sequencing level.
10. **Agentic Resolution Orchestrator**: Subscribes to execution disruption events (resource outage, supplier delay) and generates alternative resolution scenarios, scoring them on a multi-objective matrix.
11. **Generative AI Co-pilot**: NLP interface allowing planners to query system states and trigger scenarios.
12. **Closed-Loop ML Feedback**: Live telemetry pipeline that updates master data parameters (such as safety stock and lead times) based on actual MES/ERP execution deviations.

#### 5.1.3 Implementation Order

The modules should be implemented in the following order to respect dependencies:

```
1. Material Availability [MVP]
   └─> No dependencies (foundation)

2. Material Reservation [MVP]
   └─> Depends on: Material Availability

3. Capacity CTP [MVP] (can be parallel with Transport ATP)
   └─> Depends on: Work Routing (master data), Resource Calendars

4. Transport ATP [MVP] (can be parallel with Capacity CTP)
   └─> Depends on: Transport Routing (master data), Transport Legs

5. Promise [MVP]
   └─> Depends on: Material Availability, Material Reservation,
       Capacity CTP, Transport ATP

6. MRP [MVP]
   └─> Depends on: Material Availability, Material Reservation,
       Capacity CTP, BOM

7. Material Replenishment [MVP]
   └─> Depends on: MRP (triggers MRP on shortfall)

8. Optimization [Phase 2]
   └─> Depends on: MRP functions, Capacity CTP, Transport ATP

9. Cognitive & Agentic Layers [Phase 2]
   └─> Layered on top of core planning and optimization modules
```

#### 5.1.4 Module Responsibilities Summary

| Module                     | Purpose                    | Speed     | Scope           | Creates                  |
| -------------------------- | -------------------------- | --------- | --------------- | ------------------------ |
| **Material Availability**  | Knowledge/Query Service    | Instant   | Per query       | Information only         |
| **Material Reservation**   | Hold/Commit Material       | Instant   | Per reservation | Reservations             |
| **Capacity CTP**           | Capacity Checking Service  | Seconds   | Per work order  | Capacity availability    |
| **Transport ATP**          | Transport Checking Service | Seconds   | Per route       | Transport availability   |
| **Promise**                | Order Promising            | Sub-30s   | Per order       | Tentative reservations   |
| **MRP**                    | Material Planning          | 30min-2hr | All orders      | Supply orders (PO/WO/TO) |
| **Material Replenishment** | Inventory Management       | Minutes   | Per product/SP  | Triggers MRP             |
| **Optimization**           | Plan Optimization          | Hours     | All orders      | Optimized plan           |

---

### 5.2 Core Modules Detailed

This section provides detailed descriptions of each of the 8 core modules, their responsibilities, interfaces, and how they interact.

#### 5.2.1 Material Availability Module

**Purpose**: Knowledge/Query Service - Provides material position information without making decisions.

**Responsibilities**:
- ✅ Query Inventory projection (on-hand quantities)
- ✅ Query SupplyOrder projection (inbound supply orders)
- ✅ Query MaterialReservation projection (active reservations)
- ✅ Query Safety Policy (safety stock requirements)
- ✅ Calculate net available quantity
- ✅ Return material snapshot

**What It Does NOT Do**:
- ❌ Does NOT decide whether to import or produce
- ❌ Does NOT generate supply orders
- ❌ Does NOT make planning decisions

**Interface**:
```fsharp
type MaterialProvider = {
    GetSnapshot: ProductId * StockingPointId * DateTimeOffset 
        -> Async<Result<MaterialSnapshot, ProviderError>>
}

type MaterialSnapshot = {
    OnHand: decimal
    Inbound: (DateTimeOffset * decimal) list
    Reservations: decimal
    Safety: decimal
}
```

**Usage**:
- Used by: Promise, MRP, Material Replenishment
- Called when: Need to know material availability
- Returns: Snapshot of material position (information only)

**Example**:
```fsharp
let snapshot = MaterialProvider.GetSnapshot("ProductX", "SP-1", Monday)
// Returns:
// {
//   OnHand = 200.0m
//   Inbound = [{ Date = Monday, Qty = 50 }]
//   Reservations = 150.0m
//   Safety = 20.0m
// }
// Net Available = 200 + 50 - 150 - 20 = 80 units
```

---

#### 5.2.2 Material Reservation Module

**Purpose**: Hold/Commit Material - Prevents double-booking and tracks committed material.

**Responsibilities**:
- ✅ Create tentative reservations (on promise)
- ✅ Confirm reservations (on order acceptance)
- ✅ Release reservations (on cancellation/rejection)
- ✅ Expire reservations (automatic cleanup)
- ✅ Reduce reservations (on quantity decrease)

**Lifecycle**:
```
Tentative → Confirmed → Released/Expired
         ↘ Reduced ↗
```

**Key Properties**:
- Simple contract: Product + Quantity + Time Window
- Does NOT store full inventory details (MaterialProvider knows this)
- Does NOT specify which order it's for (Pegging does this)
- Subtracted in Material Availability calculations
- Subtracted in MRP netting calculations

**Interface**:
```fsharp
type ReservationProvider = {
    CreateTentative: ReservationRequest -> Async<Result<ReservationId, ProviderError>>
    Confirm: ReservationId -> Async<Result<unit, ProviderError>>
    Release: ReservationId -> Async<Result<unit, ProviderError>>
    Expire: ReservationId -> Async<Result<unit, ProviderError>>
    Reduce: ReservationId * decimal -> Async<Result<unit, ProviderError>>
}
```

**Usage**:
- Used by: Promise (creates tentative), MRP (subtracts in netting)
- Input for: Material Availability, MRP
- Purpose: Prevent double-booking

---

#### 5.2.3 Capacity CTP Module

**Purpose**: Capacity Checking Service - Answers "When can I produce this product?"

**Responsibilities**:
- ✅ Own Work Routing knowledge (for capacity checking)
- ✅ Check capacity availability for routing steps
- ✅ Find earliest feasible capacity windows
- ✅ Handle alternate routings/resources
- ✅ Return capacity availability results

**Key Design Decision**:
- **Owns Routing Knowledge**: Capacity CTP looks up routing internally
- **MRP Interface**: MRP just asks "can I produce Product X?" (no routing knowledge needed)
- **Better Separation**: MRP doesn't need routing details

**Interface**:
```fsharp
type CapacityCTP = {
    CheckCapacity: ProductId * decimal * DateTimeOffset 
        -> Async<Result<CapacityCheckResult, CapacityError>>
}

type CapacityCheckResult = {
    EarliestAvailable: DateTimeOffset
    CapacityAvailable: bool
    ResourceAssignments: ResourceAssignment list
    Limiter: PromiseLimiter option
}
```

**Internal Process**:
```
1. Receive: ProductId, Quantity, Need Date
2. Look up Work Routing from master data (internal)
3. Get routing steps
4. For each routing step:
   - Get resource group from step
   - Check capacity for that resource
   - Find earliest feasible window
5. Calculate earliest capacity-ready date
6. Return result
```

**Usage**:
- Used by: Promise (real-time), MRP (batch), Optimizer
- Shared service: Same Capacity CTP used by all modules
- Purpose: Check if production capacity is available

---

#### 5.2.4 Transport ATP Module

**Purpose**: Transport Availability Service - Answers "Can I ship this product on time?"

**Responsibilities**:
- ✅ Own Transport Routing knowledge (for transport checking)
- ✅ Check transport leg availability
- ✅ Find earliest feasible transport itineraries
- ✅ Check capacity, cutoffs, constraints
- ✅ Return transport availability results

**Key Design Decision**:
- **Owns Transport Routing Knowledge**: Transport ATP looks up transport routing internally
- **Used by**: Promise (for transport ATP), Transport Planning (for logistics)

**Interface**:
```fsharp
type TransportATP = {
    GetOptions: StockingPointId * StockingPointId * DateTimeOffset 
        -> Async<Result<TransportItinerary list, TransportError>>
}

type TransportItinerary = {
    Legs: TransportLeg list
    Arrival: DateTimeOffset
    Cost: decimal
    Reliability: float
    CO2: decimal option
}
```

**Usage**:
- Used by: Promise (for transport ATP), Transport Planning
- Purpose: Check if transport/logistics is available

---

#### 5.2.5 Promise/ATP Module

**Purpose**: Order Promising - Real-time, per-order, customer-facing (tiered SLA: <1s/<5s/<30s)

**Responsibilities**:
- ✅ Answer "When can I deliver THIS order?"
- ✅ Check material, capacity, transport availability
- ✅ Create tentative reservations
- ✅ Return probabilistic promise dates with confidence intervals (e.g., p50/p85/p95 dates) and cost

**Key Characteristics**:
- **Real-Time**: Tiered SLA (Simple ATP <1s, Standard CTP <5s, Full CTP <30s)
- **Per-Order**: One order at a time
- **Does NOT**: Generate supply orders (that's MRP's job)
- **Does NOT**: Call MRP (too slow, different purpose)

**Process Flow** (Detailed):
```
┌──────────────────────────────────────────────────────────────┐
│              PROMISE ORCHESTRATOR                            │
│  (Real-Time, Per-Order, Sub-30s)                             │
│                                                              │
│  Input: Customer Order (Product, Quantity, Due Date)         │
│                                                              │
│  Step 1: ROUTING SELECTION ← FIRST!                          │
│    └─> Call RoutingProvider.Select(ProductId, SP, Policy)    │
│    └─> Returns: Primary Routing + Alternates                 │
│    └─> Routing contains:                                     │
│        - Work Routing Steps (for capacity checking)          │
│        - Transport Routing (for shipping)                    │
│        - Qty-dependent durations                             │
│                                                              │
│  Step 2: MATERIAL ATP                                        │
│    └─> Call MaterialProvider.GetSnapshot()                   │
│    └─> Calculate: netAvailable = onHand + inbound -          │
│        reservations - safety                                 │
│    └─> If shortfall and policy allows:                       │
│        - Call SupplierProvider.GetSupplierOptions()          │
│    └─> Result: Earliest material-ready date                  │
│                                                              │
│  Step 3: CAPACITY ATP (Uses Work Routing from Step 1)        │
│    └─> For each routing step:                                │
│        - Get resource group from routing step                │
│        - Call CapacityCTP.CheckCapacity()                    │
│        - Find earliest feasible window                       │
│    └─> Result: Earliest capacity-ready date                  │
│                                                              │
│  Step 4: TRANSPORT ATP (Uses Transport Routing from Step 1)  │
│    └─> Get origin/destination from routing                   │
│    └─> Call TransportATP.GetOptions()                        │
│    └─> Check capacity, cutoffs, constraints                  │
│    └─> Result: Earliest transport arrival date               │
│                                                              │
│  Step 5: CALCULATE PROMISE DATE                              │
│    └─> PromiseDate = MAX(                                    │
│          materialReady,                                      │
│          capacityReady,                                      │
│          transportArrival                                    │
│        )                                                     │
│    └─> Bottleneck determines promise date                    │
│                                                              │
│  Step 6: CREATE TENTATIVE RESERVATIONS                       │
│    └─> Material Reservation (if material available)          │
│    └─> Capacity Reservation (if capacity available)          │
│    └─> Transport Reservation (if transport available)        │
│                                                              │
│  Step 7: RETURN PROMISE RESPONSE                             │
│    └─> Decision: Accepted/Rejected                           │
│    └─> Promise Date (if accepted)                            │
│    └─> Limiter (if rejected, explains why)                   │
│    └─> Cost, Confidence, Reservations                        │
└──────────────────────────────────────────────────────────────┘
```

**Why Routing Selection First?**
- Capacity ATP needs routing steps to know which machines to check
- Transport ATP needs routing to know which transport route to check
- Routing provides context for all subsequent checks

**Integration Points**:
- Uses: Material Availability, Material Reservation, Capacity CTP, Transport ATP
- Creates: Tentative Reservations
- Does NOT: Call MRP (different purpose, too slow)

---

#### 5.2.6 MRP Module

**Purpose**: Material Requirements Planning - Batch planning for all orders (30min-2hr)

**Responsibilities**:
- ✅ BOM Explosion (multi-level material requirements)
- ✅ Material Netting (gross - available = net requirements)
- ✅ Finite Capacity Checking (via Capacity CTP)
- ✅ Generate Supply Orders (PO/WO/TO)
- ✅ Create Pegging Links (demand → supply traceability)

**Key Characteristics**:
- **Batch**: Plans all orders together (30 minutes to 2 hours)
- **Finite Capacity**: Checks capacity via Capacity CTP (doesn't assume infinite)
- **No Routing Knowledge**: Just asks Capacity CTP "can I produce?" (Capacity CTP owns routing)
- **Planning-Facing**: Internal planning process (not customer-facing)

**Process Flow**: BOM Explosion → Material Netting → WO Proposals → Capacity Checking (via Capacity CTP) → Date Adjustment → Supply Order Generation → Pegging. See [§6.3 Material Requirements Planning](#63-material-requirements-planning-mrp) for the detailed process flow, inputs, outputs, and algorithms.

**Sub-Modules**:
- **Heuristic MRP**: Fast path (minutes per order, daytime)
- **Material Replenishment**: Triggers MRP when stock falls below minimum

**Integration Points**:
- Uses: Material Availability, Material Reservation, Capacity CTP, BOM
- Generates: Supply Orders (PO/WO/TO)
- Creates: Pegging Links
- Does NOT: Create reservations (that's Promise's job)


---

#### 5.2.7 Material Replenishment Module

**Purpose**: Inventory Management - Maintain stock levels (supply-driven planning)

**Responsibilities**:
- ✅ Monitor stock levels (current vs. targets)
- ✅ Detect shortfalls (below min/safety)
- ✅ Trigger MRP when shortfall detected
- ✅ Maintain stock levels independently of customer orders

**Key Characteristics**:
- **Supply-Driven**: Works on stock levels (not customer orders)
- **Triggers MRP**: When shortfall detected, calls MRP to arrange supply
- **Independent**: Can run alongside MRP (different planning approach)

**Process Flow**:
```
┌──────────────────────────────────────────────────────────────┐
│              MATERIAL REPLENISHMENT                          │
│  (Inventory Management - Supply-Driven)                      │
│                                                              │
│  Step 1: MONITOR STOCK LEVELS                                │
│    └─> Check current stock: 50 units Product X               │
│                                                              │
│  Step 2: COMPARE TO TARGETS                                  │
│    └─> Min Level: 100 units                                  │
│    └─> Safety Stock: 20 units                                │
│    └─> Target: 100 units                                     │
│                                                              │
│  Step 3: CALCULATE SHORTFALL                                 │
│    └─> Shortfall = 100 - 50 = 50 units                       │
│    └─> Need Date: Jan 15 (based on cover days)               │
│                                                              │
│  Step 4: TRIGGER MRP ← KEY ACTION!                           │
│    └─> Call MRP with:                                        │
│        - Product: Product X                                  │
│        - Quantity: 50 units                                  │
│        - Need Date: Jan 15                                   │
│        - Source: Replenishment (not customer order)          │
│                                                              │
│  Step 5: MRP TAKES OVER                                      │
│    └─> MRP does BOM explosion (if needed)                    │
│    └─> MRP checks Material Availability                      │
│    └─> MRP calculates net requirements                       │
│    └─> MRP checks Capacity (via Capacity CTP)                │
│    └─> MRP generates Supply Orders                           │
│                                                              │
│  Result: Supply orders generated to maintain stock levels    │
└──────────────────────────────────────────────────────────────┘
```

**Integration Points**:
- Triggers: MRP (when shortfall detected)
- Uses: Material Availability (to check current stock)
- Purpose: Maintain stock levels (supply-driven)

---

#### 5.2.8 Optimization Module

**Purpose**: Multi-Objective Optimization - Improve planning results (better than heuristics)

**Responsibilities**:
- ✅ Optimize across material + capacity + transport together
- ✅ Multi-objective optimization (cost, time, CO2, utilization)
- ✅ Replace heuristic results with optimized plan
- ✅ Reuse MRP functions (no duplication)

**Key Characteristics**:
- **Cross-Domain**: Optimizes material + capacity + transport
- **Separate Module**: Not sub-module of MRP (optimizes more than just material)
- **Reuses Functions**: Calls MRP functions, Capacity CTP, Transport ATP
- **Nightly Batch**: Runs overnight to optimize all orders

**When to Run**:
- **Daytime**: Heuristic MRP (fast, per-order)
- **Nighttime**: Full Optimizer (better results, all orders)

**Process Flow**:
```
┌──────────────────────────────────────────────────────────────┐
│              OPTIMIZATION MODULE                             │
│  (Cross-Domain, Multi-Objective)                             │
│                                                              │
│  Input: All Demands (from all orders)                        │
│                                                              │
│  Step 1: REUSE MRP FUNCTIONS                                 │
│    └─> Call MRP.bomExplosion() (reuse, no duplication)       │
│    └─> Call MRP.materialNetting() (reuse)                    │
│                                                              │
│  Step 2: REUSE CAPACITY CTP                                  │
│    └─> Call CapacityCTP.CheckCapacity() (reuse)              │
│                                                              │
│  Step 3: REUSE TRANSPORT ATP                                 │
│    └─> Call TransportATP.GetOptions() (reuse)                │
│                                                              │
│  Step 4: BUILD OPTIMIZATION MODEL                            │
│    └─> Decision Variables:                                   │
│        - Production quantities per routing/step/time         │
│        - Purchase quantities per supplier/time               │
│        - Transport quantities per leg/time                   │
│    └─> Constraints:                                          │
│        - Material balance                                    │
│        - Capacity limits                                     │
│        - Transport capacity                                  │
│        - BOM relationships                                   │
│    └─> Objectives:                                           │
│        - Minimize lateness                                   │
│        - Minimize cost                                       │
│        - Maximize utilization                                │
│        - Minimize CO2                                        │
│                                                              │
│  Step 5: SOLVE (MILP Solver)                                 │
│    └─> Run CPLEX/OR-Tools solver                             │
│    └─> Find optimal solution                                 │
│    └─> Fallback to heuristic if timeout                      │
│                                                              │
│  Step 6: REPLACE HEURISTIC RESULTS                           │
│    └─> Replace heuristic supply orders with optimized        │
│    └─> Update work order dates                               │
│    └─> Ready for next day execution                          │
│                                                              │
│  Output: Optimized plan (better than heuristic)              │
└──────────────────────────────────────────────────────────────┘
```

**Integration Points**:
- Reuses: MRP functions, Capacity CTP, Transport ATP
- Replaces: Heuristic MRP results (with optimized plan)
- Purpose: Improve overall plan quality

---

### 5.3 Complete Planning Flow: Promise → MRP → Optimization

This section shows how all modules work together in a complete planning cycle.

#### 5.3.1 Daytime Flow: Real-Time Promise

```
┌─────────────────────────────────────────────────────────────┐
│              DAYTIME: REAL-TIME PROMISE                     │
│  (Customer-Facing, Per-Order, Sub-30s)                      │
│                                                             │
│  Customer Order Arrives:                                    │
│    "I need 100 units Product A by Jan 15"                   │
│                                                             │
│  Step 1: Promise Orchestrator                               │
│    └─> Routing Selection (Work + Transport)                 │
│    └─> Material ATP (via Material Availability)             │
│    └─> Capacity ATP (via Capacity CTP)                      │
│    └─> Transport ATP (via Transport ATP)                    │
│    └─> Calculate Promise Date                               │
│    └─> Create Tentative Reservations                        │
│                                                             │
│  Result: "Yes, Jan 15 is feasible" (30 seconds)             │
│    → Customer accepts                                       │
│    → Reservations confirmed                                 │
└─────────────────────────────────────────────────────────────┘
```

#### 5.3.2 Nighttime Flow: Batch MRP

```
┌─────────────────────────────────────────────────────────────┐
│              NIGHTTIME: BATCH MRP                           │
│  (Planning-Facing, All Orders, 30min-2hr)                   │
│                                                             │
│  MRP Runs (e.g., at midnight):                              │
│    └─> Takes ALL customer orders (from Promise)             │
│    └─> Takes ALL forecasts                                  │
│                                                             │
│  Step 1: BOM Explosion (all orders)                         │
│  Step 2: Material Netting (all orders)                      │
│  Step 3: Generate Work Order Proposals (all orders)         │
│  Step 4: Capacity Checking (via Capacity CTP, all orders)   │
│  Step 5: Generate Supply Orders (PO/WO/TO)                  │
│  Step 6: Create Pegging Links                               │
│                                                             │
│  Result: Complete supply plan for all orders                │
│    → Supply orders ready for execution                      │
└─────────────────────────────────────────────────────────────┘
```

#### 5.3.3 Nighttime Flow: Full Optimization (Optional)

```
┌─────────────────────────────────────────────────────────────┐
│              NIGHTTIME: FULL OPTIMIZATION                   │
│  (Better Results, All Orders, Hours)                        │
│                                                             │
│  Optimizer Runs (after MRP, e.g., 2am):                     │
│    └─> Takes ALL demands                                    │
│    └─> Reuses MRP functions (BOM, netting)                  │
│    └─> Reuses Capacity CTP                                  │
│    └─> Reuses Transport ATP                                 │
│    └─> Optimizes across all domains together                │
│    └─> Replaces heuristic results with optimized plan       │
│                                                             │
│  Result: Optimized plan (better than heuristic)             │
│    → Ready for next day execution                           │
└─────────────────────────────────────────────────────────────┘
```

#### 5.3.4 Material Replenishment Flow (Independent)

```
┌─────────────────────────────────────────────────────────────┐
│              MATERIAL REPLENISHMENT (Independent)           │
│  (Inventory Management, Supply-Driven)                      │
│                                                             │
│  Replenishment Runs (e.g., hourly):                         │
│    └─> Monitors stock levels                                │
│    └─> Detects shortfall (below min/safety)                 │
│    └─> Triggers MRP (when shortfall detected)               │
│                                                             │
│  Result: Stock levels maintained                            │
│    → MRP generates supply orders to maintain stock          │
└─────────────────────────────────────────────────────────────┘
```

#### 5.3.5 Key Distinctions

**Promise vs MRP**:
- **Promise**: Real-time (30s), per-order, customer-facing, creates reservations
- **MRP**: Batch (30min-2hr), all orders, planning-facing, generates supply orders
- **Both use**: Material Availability, Material Reservation, Capacity CTP
- **Promise does NOT call MRP** (too slow, different purpose)

**Heuristic MRP vs Optimizer**:
- **Heuristic MRP**: Fast (minutes), good enough, daytime
- **Optimizer**: Slower (hours), better results, nighttime
- **Optimizer replaces** heuristic results (doesn't run after MRP)

**Material Replenishment vs MRP**:
- **Material Replenishment**: Supply-driven, maintains stock levels, triggers MRP
- **MRP**: Demand-driven, fulfills specific orders, generates supply orders

---

### 5.4 Event Processing Flow

1. **Demand Ingestion**: Receive demand signals from Nexus (forecasts, orders)
2. **Real-Time Promise**: Answer "When can I deliver?" (Promise module, sub-30s)
3. **Material Planning**: Calculate material requirements and generate purchase orders (MRP module, batch)
4. **Capacity Planning**: Assign production operations to resources with finite constraints (Capacity CTP, used by Promise and MRP)
5. **Schedule Optimization**: Balance competing objectives using advanced algorithms (Optimization module, nightly)
6. **Work Order Generation**: Create executable work orders for shop floor (MRP module)
7. **Real-time Adjustments**: Handle disruptions with automated replanning (Replanning module)

### 5.4 Planning Decisions Overview (detailed)

| Decision               | Description                                                         | Automation Level      | Frequency    |
| ---------------------- | ------------------------------------------------------------------- | --------------------- | ------------ |
| Order Acceptance       | Heuristic ATP/CTP-lite returning probabilistic promise windows      | Semi-automated (fast) | Per order    |
| Material Replenishment | Netting, pegging, safety/min-max/cover days, PO recommendations     | Fully automated       | Daily        |
| Capacity Assignment    | Finite allocation to periods/resources (no detailed sequencing)     | Fully automated       | Daily        |
| Work Order Planning    | Generate/release WOs, track progress, reconcile MES feedback        | Semi-automated        | Per order/WO |
| Campaign Management    | Group similar ops to reduce setups (no batching)                    | Semi-automated        | Weekly       |
| Replanning / Continuous Planning | Incremental updates & event-driven micro-replans; sandbox scenarios | Fully automated / Semi-automated | On disruption event or MES update |
| Optimization           | Multi-objective solver run with heuristic fallback                  | Automated/opt-in      | On demand    |

### 5.5 KPI Matrix (with meaning)

| Business Goal           | Order Acceptance | Material Replenishment | Capacity Assignment | Work Order Planning | Campaign Management |
| ----------------------- | ---------------- | ---------------------- | ------------------- | ------------------- | ------------------- |
| Resource Utilization    | ✅ Medium         | ✅ Low                  | ✅ High              | ✅ High              | ✅ High              |
| Material Management     | ✅ High           | ✅ High                 | ✅ Medium            | ✅ Medium            | ✅ Low               |
| Scheduling Optimization | ✅ High           | ✅ Medium               | ✅ High              | ✅ High              | ✅ Medium            |

**KPI Definitions, equations, and decisions they drive**
- On-Time Delivery = delivered_on_or_before_promise ÷ delivered. If low, improve capacity/material/lead times.
- Promise Accuracy = accepted_on_time ÷ accepted. If low, tighten acceptance heuristics or add buffers.
- Acceptance Rate = accepted ÷ requested. If low, expand capacity/material or relax rules.
- Response Time (P95/P99) = latency of acceptance decision. Over SLA → optimize heuristics/data access.
- Resource Utilization = used_capacity ÷ available_capacity. High+lateness → add capacity; low → rebalance.
- Inventory Turnover = COGS ÷ average_inventory. Low → reduce buffers/lot sizes; high with stockouts → raise safety.
- Planning Cycle Time = order_to_schedule_release duration. Long → streamline data/algorithms.
- Schedule Stability = unchanged_ops_after_replan ÷ total_ops. Low → reduce churn and increase locks/fixed.

---

### 5.6 Module Architecture Summary

#### 5.6.1 Module Responsibilities Matrix

| Module                     | Purpose                      | Speed     | Scope           | Creates                | Owns Knowledge             |
| -------------------------- | ---------------------------- | --------- | --------------- | ---------------------- | -------------------------- |
| **Material Availability**  | Query material position      | Instant   | Per query       | Information            | None (queries projections) |
| **Material Reservation**   | Hold/commit material         | Instant   | Per reservation | Reservations           | None (simple contract)     |
| **Capacity CTP**           | Check capacity availability  | Seconds   | Per work order  | Capacity availability  | **Work Routing**           |
| **Transport ATP**          | Check transport availability | Seconds   | Per route       | Transport availability | **Transport Routing**      |
| **Promise**                | Order promising              | Sub-30s   | Per order       | Tentative reservations | None (orchestrates)        |
| **MRP**                    | Material planning            | 30min-2hr | All orders      | Supply orders          | BOM (for explosion)        |
| **Material Replenishment** | Inventory management         | Minutes   | Per product/SP  | Triggers MRP           | None (monitors stock)      |
| **Optimization**           | Plan optimization            | Hours     | All orders      | Optimized plan         | None (reuses functions)    |

#### 5.6.2 Key Architectural Decisions

**1. Routing Knowledge Ownership**
- ✅ **Capacity CTP owns Work Routing knowledge** (for capacity checking)
- ✅ **Transport ATP owns Transport Routing knowledge** (for transport checking)
- ✅ **MRP does NOT need routing knowledge** (just asks "can I produce?")
- ✅ **Promise does NOT need routing knowledge** (uses Capacity CTP and Transport ATP)

**2. Finite Capacity MRP**
- ✅ **MRP checks capacity** via Capacity CTP (doesn't assume infinite capacity)
- ✅ **Realistic plans**: MRP creates feasible plans (not over-committed)
- ✅ **Early detection**: Catch capacity issues during planning (not execution)

**3. Promise vs MRP Separation**
- ✅ **Promise**: Real-time (30s), per-order, customer-facing, creates reservations
- ✅ **MRP**: Batch (30min-2hr), all orders, planning-facing, generates supply orders
- ✅ **Promise does NOT call MRP** (too slow, different purpose)
- ✅ **Both use**: Material Availability, Material Reservation, Capacity CTP

**4. Material Replenishment → MRP**
- ✅ **Material Replenishment triggers MRP** (when shortfall detected)
- ✅ **MRP generates supply orders** (single source of truth)
- ✅ **No duplication**: Replenishment doesn't generate orders directly

**5. Optimization as Separate Module**
- ✅ **Optimization is separate** (not sub-module of MRP)
- ✅ **Cross-domain**: Optimizes material + capacity + transport
- ✅ **Reuses functions**: Calls MRP functions, Capacity CTP, Transport ATP
- ✅ **No duplication**: Same functions used by heuristic and optimizer

#### 5.6.3 Data Flow Summary

```
Material Availability
    ↓ (queries)
Material Reservation ← (subtracts from availability)
    ↓ (used by)
┌─────────────┐  ┌─────────────┐
│   Promise   │  │     MRP     │
│ (Real-Time) │  │   (Batch)   │
└──────┬──────┘  └──────┬──────┘
       │                │
       │ Uses           │ Uses
       ▼                ▼
┌─────────────┐  ┌─────────────┐
│ Capacity CTP│  │ Capacity CTP│
│  (Shared)   │  │  (Shared)   │
└─────────────┘  └─────────────┘
       │                │
       │ Uses           │
       ▼                │
┌─────────────┐         │
│Transport ATP│         │
└─────────────┘         │
                        │
                        │ Can trigger
                        ▼
            ┌───────────────────────┐
            │ Material Replenishment │
            │  (Triggers MRP)        │
            └───────────────────────┘
                        │
                        │ Can use
                        ▼
            ┌───────────────────────┐
            │   Optimization         │
            │  (Uses all functions)  │
            └───────────────────────┘
```

---

## 6. Planning Decision Details

### 6.1 Material Availability Module

**Description**  
Material Availability is a **knowledge/query service** that provides material position information without making planning decisions. It serves as the foundation for all material-related planning operations.

**Purpose**  
Answer the question: "What material is available?" (Information only, no decisions)

**Responsibilities**:
- ✅ Query Inventory projection (on-hand quantities per product/stocking point)
- ✅ Query SupplyOrder projection (inbound supply orders with dates/quantities)
- ✅ Query MaterialReservation projection (active reservations to subtract)
- ✅ Query Safety Policy (safety stock requirements to subtract)
- ✅ Calculate net available quantity
- ✅ Return material snapshot (aggregated information)

**What It Does NOT Do**:
- ❌ Does NOT decide whether to import or produce
- ❌ Does NOT generate supply orders
- ❌ Does NOT make planning decisions
- ❌ Does NOT store full inventory details (just queries and calculates)

**Interface**:
```fsharp
type MaterialProvider = {
    GetSnapshot: ProductId * StockingPointId * DateTimeOffset 
        -> Async<Result<MaterialSnapshot, ProviderError>>
}

type MaterialSnapshot = {
    OnHand: decimal                    // Current inventory
    Inbound: (DateTimeOffset * decimal) list  // Inbound supply orders
    Reservations: decimal               // Active reservations (subtract)
    Safety: decimal                     // Safety stock (subtract)
}
```

**Calculation**:
```
Net Available = OnHand + Inbound - Reservations - Safety
```

**Usage**:
- **Used by**: Promise, MRP, Material Replenishment
- **Called when**: Need to know material availability
- **Returns**: Snapshot of material position (information only)

**Example**:
```fsharp
let snapshot = MaterialProvider.GetSnapshot("ProductX", "SP-1", Monday)

// MaterialProvider queries:
// 1. Inventory Projection → 200 units on-hand
// 2. SupplyOrder Projection → 50 units arriving Monday
// 3. MaterialReservation Projection → 150 units reserved
// 4. Safety Policy → 20 units safety stock

// Returns:
{
    OnHand = 200.0m
    Inbound = [{ Date = Monday, Qty = 50 }]
    Reservations = 150.0m
    Safety = 20.0m
}

// Net Available = 200 + 50 - 150 - 20 = 80 units
// (Just information, no decision about what to do)
```

**Integration Points**:
- **Promise**: Queries Material Availability to check if material is available
- **MRP**: Queries Material Availability for netting calculations
- **Material Replenishment**: Queries Material Availability to check stock levels

**Key Principle**:
Material Availability = "What do I have?" (Information)  
MRP = "What should I do?" (Decision)

**Architectural Note**:
Material Availability is a pure query/knowledge service. It does NOT make decisions about importing or producing. It simply provides information about material position. Decision-making (what to import, what to produce) is the responsibility of MRP.

---


### 6.2 Order Acceptance (Promise/ATP - Heuristic ATP/CTP-lite) [MVP]

**Description**  
Commit orders by checking material, capacity, transport, and supplier feasibility with a fast heuristic (tiered SLA: Simple ATP <1s, Standard CTP <5s, Full CTP <30s); no detailed sequencing or solver execution on the online path. The order promising framework provides reliable order promising across all availability domains with snapshot-based persistence, projection-backed queries, deterministic IDs, and pluggable policies (time-first, cost-aware, risk-aware).

**Architectural Decision: Heuristic vs. Optimizer Promising**
- **Decision**: Promising is strictly **Heuristic-based (ATP/CTP-lite)** on the real-time path (sub-5 seconds). **Mathematical Optimization** (MILP CPLEX/OR-Tools) is deferred to the nightly batch run.
- **Rationale**: 
  1. *Latency*: Running a multi-variable MILP solver for a single order takes too long, violating the sub-5-second SLA needed for online order promising or customer-facing APIs.
  2. *Myopic Allocation*: Real-time optimization of single orders in isolation leads to "first-come, first-served" resource grabbing, which is globally sub-optimal.
  3. *Conflict Resolution*: Heuristics place a **tentative reservation** immediately to lock resources. The nightly global optimizer then runs across all orders to resolve resource contention, re-sequence tasks, and minimize setup changeovers globally.
- **Process**:
  - The Promise Orchestrator performs bucket-based capacity and material availability subtraction using cached read models and reservations.
  - If a shortfall occurs, it queries supplier lead times and transport options.
  - On acceptance, it creates tentative reservations (material, capacity, transport) with a Time-To-Live (TTL).

**Goals**  
- Reliable order promising across material, capacity, transport, and supplier options
- Snapshot-based persistence (Marten), projection-backed queries, deterministic IDs, idempotency
- Pluggable policies (time-first, cost-aware, risk-aware), alternates/fallbacks
- Reservations lifecycle (tentative → confirmed → release/expire) across domains
- Provide probabilistic promises (confidence intervals) based on lead-time distributions and capacity queueing models
- Caching with clear invalidation for performance; optional background rebuild

**High-Level Flow (Data & Decisions)**  
```
┌─────────────────────────────────────────────────────────────┐
│              PROMISE WORKFLOW (Complete Flow)               │
│                                                             │
│  Input: Customer Order (Product, Quantity, Due Date)        │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 1: ROUTING SELECTION ← FIRST!                  │   │
│  │    └─> RoutingProvider.Select(ProductId, SP, Policy) │   │
│  │    └─> Returns: Primary + Alternates                 │   │
│  │    └─> Contains: Work Routing + Transport Routing    │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 2: MATERIAL ATP                                │   │
│  │    └─> MaterialProvider.GetSnapshot()                │   │
│  │    └─> Calculate: onHand + inbound - reservations    │   │
│  │    └─> If shortfall: SupplierProvider (optional)     │   │
│  │    └─> Result: Material ready date                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 3: CAPACITY ATP (Uses Work Routing)            │   │
│  │    └─> For each routing step:                        │   │
│  │        - Get resource group from step                │   │
│  │        - CapacityCTP.CheckCapacity()                 │   │
│  │        - Find earliest feasible window               │   │
│  │    └─> Result: Capacity ready date                   │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 4: TRANSPORT ATP (Uses Transport Routing)      │   │
│  │    └─> Get origin/destination from routing           │   │
│  │    └─> TransportATP.GetOptions()                     │   │
│  │    └─> Check capacity, cutoffs, constraints          │   │
│  │    └─> Result: Transport arrival date                │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 5: CALCULATE PROMISE DATE                      │   │
│  │    └─> PromiseDate = MAX(                            │   │
│  │          materialReady,                              │   │
│  │          capacityReady,                              │   │
│  │          transportArrival                            │   │
│  │        )                                             │   │
│  │    └─> Bottleneck determines promise date            │   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Step 6: CREATE TENTATIVE RESERVATIONS               │   │
│  │    └─> Material Reservation (if material available)  │   │
│  │    └─> Capacity Reservation (if capacity available)  │   │
│  │    └─> Transport Reservation (if transport available)│   │
│  └──────────────────────────────────────────────────────┘   │
│                          │                                  │
│                          ▼                                  │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  Output: Promise Response                            │   │
│  │    - Decision: Accepted/Rejected                     │   │
│  │    - Promise Date (if accepted)                      │   │
│  │    - Limiter (if rejected)                           │   │
│  │    - Cost, Confidence, Reservations                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘

Key Dependencies:
  Material Availability ← Queries projections
  Material Reservation ← Subtracts from availability
  Capacity CTP ← Owns Work Routing knowledge
  Transport ATP ← Owns Transport Routing knowledge
  Routing Master Data ← Used by Capacity CTP and Transport ATP
```

**Core Domain Pieces (Aggregates / Projections)**  
- CustomerOrder / Promise: decision, limiter, rationale
- Routing: primary + alternates, qty-duration, validity, resource groups, cost/risk prefs
- ResourceCalendar (proj): capacity windows per resource ref
- Operation: allocations/locks against capacity (scheduled/started/completed)
- Inventory (proj): on-hand per product/SP
- SupplyOrder (proj): firm inbound with required dates/state
- MaterialRequirement / MaterialReservation: pegged demand + holds; safety policies
- TransportLeg aggregate + TransportCalendar proj: schedules, capacity, lead time, cost, constraints, reliability
- SupplierOffer / Supplier ATP: lead times (p50/p95), MOQs, capacities, price, incoterms, reliability
- Reservation aggregate: scope (material/capacity/transport), qty/time window, state (Tentative/Confirmed/Released/Expired/Reduced), TTL

**Inputs**  
- **Order**: product, qty, due date, priority, full-order/full-delivery flags, stocking point, expedite flag, customer
- **Supply**: on-hand, WIP, in-transit, firm inbound POs/WOs with dates, reservations, safety/targets
- **Routing/capacity**: routing steps, qty-dependent durations, resource calendars, fixed/locked operations, allowed slack, routing alternates
- **Transport**: transport legs, calendars, capacities, cutoffs, lead times, constraints, reliability, CO2
- **Supplier**: supplier offers, MOQs, lead times (p50/p95), price tiers, reliability, incoterms
- **Policies**: priority ordering, full-order/full-delivery enforcement, expedite limits, time vs cost vs risk preferences, cost/risk caps, search budget

**Process (online fast path <30s heuristic; no detailed sequencing)**  
1) **Routing Selection**: Call RoutingProvider.Select with policy; get primary/alternate set. Try primary first, then alternates on failure or better score per policy (fastest/cheapest/balanced)
2) **Material ATP-lite**: 
   - Call MaterialProvider.GetSnapshot (on-hand + inbound - reservations - safety)
   - Compute earliest material-ready date: net_available(t) = on_hand + inbound_by(t) – reservations_by(t) – safety; earliest_material_date = min t where net_available ≥ qty
   - If shortfall and policy allows, call SupplierProvider.GetSupplierOptions; consider supplier option if earlier/cheaper per policy
3) **Capacity window**: 
   - For each routing step, call CapacityProvider.GetBuckets (calendars − allocations/locks − reservations − safety)
   - Apply qty-duration from CapacityProvider.GetQtyDuration (qty-dependent duration calculation)
   - Find earliest feasible window per routing step; apply slack/buffer per policy
   - Earliest_capacity_date = end of last step
4) **Transport ATP**: 
   - Call TransportProvider.GetOptions for origin→destination (multi-hop pathfinding with k-shortest paths)
   - Check capacity, cutoffs, constraints (regulatory/hazmat), reliability
   - Pick earliest/score by policy (time/cost/risk/CO2/green preference)
   - Earliest transport arrival date
5) **Promise date calculation**: PromiseDate = max(material_ready, capacity_ready, transport_arrival)
6) **Limiter selection**: Limiter = argmax contributor (material/capacity/transport/supplier); attach rationale with reason code from catalog
7) **Risk/confidence calculation**: Use lead-time probability distributions (e.g., log-normal for transport legs, beta for supplier deliveries) and G/G/1 queueing models for capacity queues; compute confidence intervals (e.g., 85% and 95% delivery dates) rather than a single deterministic date. Enforce gold customer policies (e.g., promising at p95) vs standard customers (promising at p50).
8) **Cost calculation**: Compute time + cost score (material + production + transport + holding + lateness penalties); policy chooses fastest/cheapest/balanced
9) **Constraint application**: Apply FullOrder/FullDelivery (requires single promise per order/line), priority (may preempt within policy), expedite (only if policy allows)
10) **Decision emission**: 
    - If feasible: Create tentative reservations (material/capacity/transport) via ReservationProvider.CreateTentative; emit Promise/Acceptance event with limiter, rationale, routing/itinerary, cost, confidence, reservations
    - If infeasible: Emit Rejection with limiter (domain, code, message, suggestions) and rationale

**Outputs**  
- **Decision**: accepted/rejected, PromiseDate, confidence intervals (e.g., p50, p85, and p95 dates), limiter, rationale
- **Routing**: selected routing (primary/alternate), steps used
- **Itinerary**: transport itinerary (legs, arrival, cost, reliability, CO2) if transport used
- **Material**: material snapshot (earliest available, net available qty)
- **Capacity**: capacity availability per routing step (earliest end, is sufficient)
- **Cost**: total cost breakdown (material, production, transport, holding, lateness penalties), currency
- **Risk**: confidence intervals, probability distribution basis, reliability notes
- **Reservations**: tentative reservation IDs (material/capacity/transport) created
- **Meta**: generated timestamp, search budget used, search mode, provider latencies
- **Events**: PromiseCreated/AcceptanceCreated events to read models/audit

**Rules**  
- Single UoM; no batching; no detailed sequencing
- Online SLA <5s heuristic; day-over-day full run can refine
- Honor locks/fixed operations; enforce full-order/full-delivery; priority strictly ordered
- Reservations are idempotent (deterministic IDs, idempotency keys); retry-safe
- Provider-based extensibility: light mode (no-op providers) vs full mode (real providers) without code changes
- Degradation: ProviderError maps to Limiter with retry/alternate suggestions; never leave dangling reservations on failure
- Cache invalidation: subscribe to calendar/leg/reservation/allocation change events; rebuild caches on invalidation

**Provider-Based Architecture**  
The promise orchestrator uses injected providers to enable light vs full modes:
- **MaterialProvider**: GetSnapshot (on-hand + inbound - reservations - safety), optional GetSupplierOptions
- **CapacityProvider**: GetBuckets (calendars - allocations - reservations - safety), GetQtyDuration (qty-dependent duration)
- **TransportProvider**: GetOptions (k-shortest paths, capacity/cutoff/constraint checks, scoring)
- **SupplierProvider**: GetSupplierOptions (MOQ/lead-time validation, scoring by time/cost/reliability)
- **RoutingProvider**: Select (primary/alternates, policy-driven selection)
- **ReservationProvider**: CreateTentative, Confirm, Release (idempotent, deterministic IDs)
- **PolicyProvider**: GetPolicy (customer/SKU-specific policy presets: Gold/Silver/Bronze)
- **TelemetryProvider**: RecordKpi, RecordError, RecordLatency (per provider, limiter frequency, cache hit/miss)
- **FxProvider**: GetRate (currency conversion with as-of timestamp, stale guard)
- **TenantProvider**: GetTenant (tenant ID + timezone, UTC normalization)

**Light vs Full Mode**  
- **Light mode**: Inject defaults (reservations=0, safety=0, GetOptions returns empty transport, GetSupplierOptions empty, allocations not subtracted, qty-duration None). Orchestrator still runs and returns heuristic results
- **Full mode**: Inject real providers (reservations/safety filled, allocations/locks subtracted, transport itineraries, supplier options, qty-duration present). No code duplication, just richer data and policies

**Reservations Lifecycle (Cross-Domain)**  
- States: Tentative → Confirmed → Released/Expired → Reduced
- Events: ReservationCreated, Confirmed, Released, Reduced, Expired
- Created on promise (tentative), confirmed on release/firming; released on cancel/qty decrease/expiry
- TTL/expiry sweeper frees resources; projections feed availability subtraction
- Deterministic IDs: hash(orderId, lineId, scope, step/leg, window)
- Idempotency keys: prevent duplicates on retry

**Multi-Hop Material Flow**  
- Graph: stocking points + transport legs
- Material readiness can be sourced from multiple origins; k-shortest paths optional
- Stage chaining: material ready → production finish → transport to next node → … → delivery
- Promise driven by slowest stage; limiter names the bottleneck

**Transport Modeling**  
- Leg: origin, destination, mode, schedule (departures), capacity per departure, cutoff, lead time, cost, constraints, reliability, CO2
- Availability: earliest feasible itinerary with capacity/cutoff/constraints; add safety slack or p95 lead times for high-SLA
- Pathfinding: k-shortest path algorithm (Yen's algorithm) over transport graph with capacity/cutoff/constraint checks
- Scoring: time/cost/risk/reliability/CO2 per policy preference

**Supplier ATP**  
- SupplierOffer events: capacity offered/consumed, lead time updates, price tiers, MOQs, reliability
- Query when local short or policy says compare; score by date/cost/reliability
- Fallback policy configurable (block vs accept-with-risk vs counteroffer)
- MOQ validation, lead-time validation (reject if exceeds SLA)

**Cost & Risk**  
- Cost model: material (buy), production (resource), transport (lane/mode), holding, lateness penalties; currency normalized via FX snapshot
- Risk: lead-time distributions, reliability, buffers (p95 or p50+buffer%); RiskAssessment returns confidence + sensitivity
- Promise payload: date, limiter, cost, confidence

**Quality & Rework**  
- Yield per step; effective required material/capacity = demand / yield
- Rework/inspection steps; add capacity/transport hops where needed

**Regulatory & Compliance**  
- Constraints on legs/routings (export controls, hazmat, country pairs)
- Customs/clearance as extra lead time/cost nodes
- Sustainability: CO2 per leg; "green" preference in scoring if required

**Real-Time Updates & Alerting**  
- On projection deltas (resource down, transport outage, supplier delay): re-eval impacted promises, emit AtRisk, notify customers; optional reroute/replan per policy
- SLA monitoring: promised vs predicted; alert on breach risk

**Caching & Invalidation**  
- Caches: Capacity buckets (post allocations/reservations/safety) per resource key; Material snapshots (on-hand + inbound − reservations − safety) per product/SP; Transport next departures/itineraries per lane
- Invalidation triggers: calendar/leg changes, reservations changes, allocations changes, safety policy changes
- Background agents: rebuild caches on events/interval; keep last-good on failure. Reservation expiry sweeper
- Timeouts: per-request budget; fallback to heuristic if optimal search exceeds budget
- Scale: stateless services over shared projections/caches; shardable caches if high QPS

**Reason-Code Catalog**  
- **Material**: MaterialShortfall, MaterialReservationConflict, SafetyViolation, SupplierMOQ, SupplierLeadtimeExceeded
- **Capacity**: CapacityShortfall, CapacityLocked, CapacitySafetyBuffer, QtyDurationUnsupported
- **Transport**: NoTransportLeg, NoTransportCapacity, CutoffMissed, RegulatoryBlocked
- **Routing/Alternates**: RoutingInvalid, RoutingCapacityFail, AlternateExhausted
- **Policy/Constraints**: FullOrderViolation, FullDeliveryViolation, CostCapExceeded, RiskCapExceeded
- **System**: SearchTimeout, DataStale

**Concurrency / Race Handling**  
- For app-driven ATP, wrap the orchestrator in a MailboxProcessor (PromiseAgent) to serialize promise computations if needed and avoid concurrent state mutations
- Providers themselves should be side-effect free or concurrency-safe; reservations remain the critical shared mutation and should be idempotent

**ML Integration Workflow**  
- Where ML fits: Providers can call ML models to predict lead times, reliability, or risk for material inbound, capacity availability (e.g., outage risk), transport itineraries, and supplier performance
- Pattern: Provider returns Result<'a, ProviderError> with fields carrying both predicted values and the basis (model/version, p50/p95)
- On ML failure, return ProviderError and let limiter surface with retry/heuristic suggestion

**UI**  
- Promise date, limiter reason, priority/flags, response timer; hot-spot alerts; override with explanation
- Show routing/itinerary chosen, cost breakdown, confidence level, reservation status
- Display suggestions from limiter (later date, alternate routing/mode, reduce qty, supplier option, override buffer, retry)

**Use Cases**  
- Standard acceptance; rejection with reason; priority preemption; expedite attempt; full-order enforcement; bulk check
- Multi-hop material flow; supplier ATP on shortfall; transport pathfinding with constraints; alternate routing selection
- Real-time promise re-evaluation on disruption; cache invalidation and rebuild

**KPIs (see 4.4 definitions)**  
- Promise accuracy, acceptance rate, response time (P95/P99), lateness split (material vs capacity vs transport vs supplier)
- Provider latency per domain (material/capacity/transport/supplier); limiter frequency distribution; cache hit/miss rates
- Promise at-risk tracking (promises that may breach due to projection deltas)

### 6.3 Material Requirements Planning (MRP)

**Description**  
Multi-level netting, pegging, and PO recommendations with safety/min-max/cover-day policies. The material planning system computes net requirements per product/stocking point to drive supply orders and reservations, supporting pegging/traceability, safety/min/max/cover policies, and integration with ATP/Promise. **Includes finite capacity checking** to create realistic, feasible plans.

**Goals**  
- Compute net requirements per product/stocking point; drive supply orders and reservations
- Support pegging/traceability, safety/min/max/cover policies, and integration with ATP/Promise
- **Check capacity constraints** (finite capacity MRP) to ensure realistic plans
- Remain deterministic/idempotent; reuse shared core (policies, scoring, limiters, reservations)

**Scope**  
- Netting logic (gross → net) using on-hand, inbound (firm/planned), reservations, safety targets
- **Finite capacity checking** via Capacity CTP (ensures realistic work order dates)
- Supply order proposals (PO/WO/TO) generation and updates
- Pegging map (demand → supply/reservations)
- Time-phased visibility (optional buckets)
- **Sub-Modules**:
  - **Heuristic MRP**: Fast path (minutes per order, daytime)
  - **Material Replenishment**: Triggers MRP when stock falls below minimum

**Inputs**  
- **Demands**: Customer Orders, Forecasts (time-phased), safety stock, inventory targets
- **Supply**: Inventory (on-hand), SupplyOrders (firm & planned), inbound lead times, WIP, in-transit
- **Reservations**: Material reservations (tentative/confirmed/reduced) - active totals per product/SP
- **Policies**: safety/min/max/cover days per product/SP, lot sizes (fixed lot, min lot, EOQ), priority/expedite flags, violation handling (allow vs reject vs flag)
- **Master data**: Products, BOMs (for multi-level expansion with alternates), Stocking Points, UoM, lead times

**Core Logic**  
1) **BOM Explosion**: Multi-level BOM explosion with cycle detection; choose alternates per policy (preferred, availability)
2) **Gross Requirements**: Sum demand per period (customer orders + forecasts + safety stock)
3) **Netting**: 
   - NetReq = max(0, GrossReq – (OnHand + InTransit + FirmSupply + PlannedSupply – Reservations – Safety))
   - Subtract active material reservations (Tentative + Confirmed + Reduced) from availability
   - Honor safety/min/max/cover days; never violate safety unless policy permits
4) **Lot Sizing**: Apply lot size/EOQ/min/max to net requirement with rounding rules (up, nearest lot)
5) **Pegging**: Create/update pegging map linking demand to supply/reservations (optional but recommended)
6) **Planned Supply**: Emit SupplyOrder recommendations (PO/WO/TO) with dates/qty, priority/expedite, pegging references

**Process (Detailed Steps with Capacity Checking)**  

```
┌─────────────────────────────────────────────────────────────┐
│              MRP PROCESS FLOW                               │
│  (Batch, All Orders, Finite Capacity)                       │
│                                                             │
│  Step 1: BOM EXPLOSION                                      │
│    └─> For each order:                                      │
│        - Explode BOM recursively (multi-level)              │
│        - Detect circular BOM references                     │
│        - Calculate gross requirements per component         │
│        - Example: Product A (100 units) requires:           │
│          * 200 units Component X                            │
│          * 100 units Component Y                            │
│                                                             │
│  Step 2: MATERIAL NETTING                                   │
│    └─> For each component:                                  │
│        - Query Material Availability:                       │
│          * OnHand + Inbound - Reservations - Safety         │
│        - Calculate: NetReq = GrossReq - Available           │
│        - Example: Component X                               │
│          * Gross: 200 units                                 │
│          * Available: 50 units                              │
│          * Net: 150 units (need to order/produce)           │
│                                                             │
│  Step 3: GENERATE WORK ORDER PROPOSALS                      │
│    └─> For each net requirement:                            │
│        - Create Work Order Proposal                         │
│        - Product: Product A                                 │
│        - Quantity: 100 units                                │
│        - Need Date: Jan 15                                  │
│        - NO routing knowledge needed! ✅                    │
│          (Capacity CTP will handle routing lookup)          │
│                                                             │
│  Step 4: CAPACITY CHECKING (Via Capacity CTP) ← KEY STEP!   │
│    └─> For each work order proposal:                        │
│        - Call CapacityCTP.CheckCapacity(                    │
│            productId: "ProductA",                           │
│            quantity: 100,                                   │
│            needDate: Jan 15                                 │
│          )                                                  │
│        - Capacity CTP internally:                           │
│          * Looks up Work Routing (its responsibility)       │
│          * Gets routing steps                               │
│          * Checks capacity for each step                    │
│          * Returns: Earliest available date                 │
│        - Example Result:                                    │
│          * Need Date: Jan 15                                │
│          * Capacity Available: Jan 16 (1 day delay)         │
│                                                             │
│  Step 5: ADJUST WORK ORDER DATES                            │
│    └─> If capacity not available on need date:              │
│        - Option A: Delay to earliest available              │
│          * Work Order: Jan 16 (delayed by 1 day)            │
│        - Option B: Try alternate routing (if policy allows) │
│        - Option C: Reject if too late (per policy)          │
│                                                             │
│  Step 6: GENERATE SUPPLY ORDERS                             │
│    └─> Purchase Orders (for materials from BOM)             │
│        - Component X: 150 units, need by Jan 10             │
│    └─> Work Orders (with capacity-checked dates)            │
│        - Product A: 100 units, start Jan 16                 │
│    └─> Transport Orders (if needed)                         │
│                                                             │
│  Step 7: CREATE PEGGING LINKS                               │
│    └─> Link demand to supply (traceability)                 │
│        - Customer Order → Work Order                        │
│        - Work Order → Purchase Order                        │
│                                                             │
│  Output: Supply Orders (PO/WO/TO) with realistic dates      │
│          (capacity-checked, not assuming infinite capacity) │
└─────────────────────────────────────────────────────────────┘
```

**Key Architectural Points**:
- ✅ **MRP does NOT own routing knowledge** (Capacity CTP owns it)
- ✅ **MRP queries Capacity CTP** (just asks "can I produce?")
- ✅ **Finite Capacity MRP** (checks capacity, creates realistic plans)
- ✅ **Capacity CTP handles routing lookup** (better separation of concerns)

**Example: Capacity Constraint Detection**:
```
Scenario: 100 customer orders, all due Jan 15

MRP Process:
  Step 1-3: BOM Explosion, Netting, Generate 100 Work Orders
  
  Step 4: Capacity Checking
    └─> For each of 100 work orders:
        - CapacityCTP.CheckCapacity()
        - Capacity CTP finds: Only 50 can be produced on Jan 15
        - Returns: Jan 16 for remaining 50 orders
  
  Step 5: Adjust Dates
    └─> Work Orders 1-50: Jan 15 ✅
    └─> Work Orders 51-100: Jan 16-20 (spread across capacity) ⚠️
  
  Result: Realistic plan (not over-committed) ✅
```

**States & Events**  
- MaterialRequirement aggregate: exists with net requirement, pegging, safety context
- SupplyOrder aggregate: planning creates/updates planned orders (Planned → Confirmed/Firm → Released)
- MaterialReservation aggregate: provides holds; subtracted in availability/netting
- Events: MaterialRequirementCreated/Updated, SupplyOrderRecommended, PeggingLinkCreated/Updated

**Projections / Queries**  
- Inventory projection (on-hand per product/SP)
- SupplyOrder projection (firm/planned with dates/states)
- MaterialReservation projection (active totals per product/SP)
- Optional time-phased netting buckets (gross/net/on-hand/inbound/reservations per period)

**Integration Points**  
- **Promise/ATP**: Uses reservations and inbound; may create tentative reservations on accept
- **Capacity CTP**: MRP queries Capacity CTP to check capacity (finite capacity MRP)
- **Netting loop**: After net calc, propose/update planned supply orders; produce pegging map
- **MRP output**: SupplyOrder recommendations + pegging events

**Key Architectural Decision: Finite Capacity MRP**
- ✅ **MRP checks capacity** via Capacity CTP (doesn't assume infinite capacity)
- ✅ **MRP does NOT own routing knowledge** (Capacity CTP owns it)
- ✅ **MRP interface**: Just asks "can I produce Product X?" (Capacity CTP handles routing lookup)
- ✅ **Realistic plans**: MRP creates feasible plans (not over-committed to capacity)

**Policies**  
- Safety/min/max/cover days per product/SP
- Lot sizing (fixed lot, min lot, EOQ) with rounding rules
- Priority/expedite influence on dates/selection
- Violation handling: allow vs reject vs flag (ConstraintPolicy)
- Forecast consumption: finished strong, intermediate limited to preferred routing

**Telemetry / KPIs**  
- Netting run latency; number of planned orders; safety violations; pegging completeness
- Promise accuracy impact (optional)
- Material availability, netting accuracy, pegging completeness, plan stability

**ML Hooks (Optional)**  
- Predict lead times for inbound; predict demand variability for safety; adjust netting buffers
- Include model basis/version in predictions; fallback to defaults on ML failure

**Outputs**  
- Material requirements (gross/net per product/SP/period)
- Pegged reservations (demand ↔ supply/reservations links)
- PO recommendations (PurchaseOrderRecommendation with qty, dates, priority, expedite, pegging)
- Projected inventory (on-hand + inbound – reservations – safety over time)

**Rules**  
- Single UoM; detect circular/invalid BOM; honor safety/min/max/cover days; alternates allowed per policy
- Forecast consumption rules: finished strong, intermediate limited to preferred routing
- Reservations subtraction: subtract active material reservations (Tentative + Confirmed + Reduced) in netting
- Idempotent proposal generation: key proposals by (demandId, period, type) to avoid duplicates on rerun

**UI**  
- Pegging graph (demand ↔ supply visualization)
- Inventory projections (on-hand + inbound – reservations – safety over time)
- PO suggestion list (recommendations with qty, dates, priority, expedite)
- Shortfall alerts (when net requirement cannot be met)
- Policy tuning interface (safety/min/max/cover days per product/SP)

**Use Cases**  
- Automatic daily netting run; forecast consumption; alternate BOM selection; shortfall detection; expedite PO
- Multi-level BOM explosion with cycle detection; time-phased netting visibility; pegging traceability

**KPIs**  
- **Material Availability** = demands met on/before need date ÷ total demands. Drives procurement/target tuning
- **Netting Accuracy** = 1 – |planned_net – actual_net| ÷ planned_net (per period). Signals BOM/lead-time/forecast issues
- **Pegging Completeness** = pegged demand lines ÷ total demand lines. Low → improve pegging/traceability
- **Plan Stability** = unchanged requirements/reservations after replan ÷ total. Low → tune policies to reduce churn

### 6.4 Capacity Assignment (Finite, no detailed sequencing)

**Description**  
Allocate operations to periods/resources with finite capacity, honoring locks/fixed ops; no detailed shop-floor sequencing. The capacity planning system provides configurable finite and infinite capacity planning modes (switchable by policy/config without code changes), plans operations against resource calendars/allocations/locks with buffers/safety, and supports routing steps, qty-dependent durations, alternates, and bottleneck handling.

**Goals**  
- Provide configurable finite and infinite capacity planning modes (switchable by policy/config without code changes)
- Plan operations against resource calendars/allocations/locks with buffers/safety
- Support routing steps, qty-dependent durations, alternates, and bottleneck handling
- Integrate with reservations (capacity) and respect material/transport constraints via upstream services

**Modes (Configurable)**  
- **Infinite planning**: Ignore capacity limits; schedule to requested/target dates; useful for what-if or rough-cut
- **Finite planning**: Honor capacity calendars, allocations/locks, reservations, safety buffers; find earliest feasible windows
- **Switch via configuration/policy**: CapacityPlanningParameters.EarliestDesiredInfiniteStrategy, bottleneck thresholds, overload thresholds

**Scope**  
- Operations planning (Operation aggregate) over resource groups/resources
- Capacity buckets from ResourceCalendars minus allocations/locks/reservations/safety
- Scheduling heuristics: earliest feasible (no sequencing detail here), bottleneck identification, slack/buffer application
- Alternates: routing alternates or resource alternates if available
- Optional: time-phased capacity visibility

**Inputs**  
- **Operations**: From SupplyOrder routing steps (Operation aggregate with routing step references)
- **ResourceCalendars**: Availability per resource/resource group (from ResourceCalendar projection)
- **Allocations/locks**: From scheduled ops (Operation allocations projection: scheduled/started operations)
- **Capacity reservations**: Future extension; aligned to material reservations pattern (CapacityReservation projection)
- **Policies**: Overload thresholds, bottleneck %, buffers, early/late thresholds (CapacityPlanningParameters)
- **Routings**: Precedence, validity windows, qty-dependent durations, resource groups, alternates

**Core Logic (Heuristic Scheduler)**  
1) **Build Capacity Buckets**: 
   - Per resource group/resource: calendar × factor − allocations/locks − reservations − safety
   - Buckets = available capacity windows (start time, end time, available duration)
   - Cache buckets; invalidate on calendar/alloc/reservation change events
2) **For Each Operation Step**:
   - Determine required duration (qty-dependent from routing step qty-duration function)
   - **Finite mode**: Find earliest bucket(s) meeting duration; apply buffers; if none, flag overload/limiter
   - **Infinite mode**: Place on target date (e.g., requested/confirmed) ignoring limits; optionally set overload flag if exceeding thresholds
3) **Alternates**: Try alternate routings or resource groups if primary fails (policy-driven)
4) **Buffers**: Apply safety/withheld capacity per resource; slack for reliability if configured

**Process (Detailed Steps)**  
1) **Build Capacity Buckets**:
   - For each resource/resource group:
     - Base capacity = calendar availability (duration × factor) – downtime
     - Subtract allocations (from scheduled/started operations)
     - Subtract locks (from fixed operations)
     - Subtract reservations (from CapacityReservation projection, if available)
     - Subtract safety buffers (per resource policy)
     - Clamp to minimum 0
     - Create capacity buckets (start time, end time, available duration)
   - Cache buckets; invalidate on calendar/alloc/reservation change events
2) **Interpret Routing**:
   - For each routing step:
     - Get qty-duration function from routing (qty → duration mapping)
     - Calculate required duration = qty-duration(operation_qty) + slack per policy
     - Enforce precedence (step k+1 after step k completion)
     - Identify resource groups required for step
3) **Allocate Operations**:
   - **Finite mode**:
     - For each operation step:
       - Find earliest feasible bucket(s) meeting duration requirement
       - Apply buffers (safety, reliability, bottleneck protection)
       - If no feasible bucket, flag CapacityShortfall limiter
       - Assign operation to bucket (start time, end time, resource)
   - **Infinite mode**:
     - Place operation on target date (requested/confirmed) ignoring capacity limits
     - Optionally set overload flag if exceeding thresholds (for visibility)
   - **Alternates**: If primary routing/resource fails, try alternates per policy
4) **Produce Schedule Snapshot**:
   - Create schedule snapshot with period/resource assignments
   - Calculate utilization metrics (used ÷ available capacity per resource/period)
   - Identify bottlenecks (resources with high utilization or overload)

**Projections / Queries**  
- ResourceCalendar projection (existing): availability per resource/resource group
- Operation allocations projection: scheduled ops (scheduled/started operations consuming capacity)
- Capacity reservation projection: future extension, similar to material reservations
- Capacity bucket read model: optional cache for performance

**Integration Points**  
- **Promise/ATP (capacity side)**: Can reuse buckets/reservations
- **Optimizer**: Can reuse policies and buckets; switch finite/infinite by policy
- **Material/transport constraints**: Handled upstream; this module focuses on capacity

**Key Configurations (CapacityPlanningParameters)**  
- BottleneckPercentage: threshold for bottleneck identification (e.g., 85%)
- OverloadCapacityThreshold: threshold for overload flagging (e.g., 0.5%)
- EarliestDesiredInfiniteStrategy: infinite mode behavior (place on target date, flag overload)
- MaxDistanceToBottleneck: maximum distance from bottleneck for alternate selection
- PlanEarlyThreshold, PlanLateThreshold: early/late scheduling thresholds
- OnTimeDeliveryBuffer: buffer for on-time delivery protection

**Outputs**  
- **Planned operation windows**: Start/End times per operation step, resource assignment
- **Overload/bottleneck flags**: If finite mode cannot satisfy within target
- **Optional planned capacity reservations**: Future extension for capacity holds
- **Schedule snapshot**: Period/resource assignments, utilization view
- **Utilization metrics**: Used ÷ available capacity per resource/period

**Rules**  
- No detailed sequencing; no setup optimization (handled via campaigns later)
- Honor locks/fixed; support quantity-dependent lead times
- Finite/infinite toggle via policy; no code changes required
- Alternates: try alternate routings or resource groups if primary fails
- Buffers: apply safety/withheld capacity per resource; slack for reliability

**Telemetry / KPIs**  
- Latency per planning run; overloads flagged; bottleneck detection counts
- Finite vs infinite usage; fallback rates
- Optional KPI weighting (if optimizer consumes)

**ML Hooks (Optional)**  
- Predict downtime/reliability; adjust buffers or bucket availability
- Include model basis/version in predictions

**UI**  
- Gantt-like capacity view (operations on timeline per resource)
- Utilization dashboard (used ÷ available per resource/period)
- Constraint violation alerts (overload, bottleneck, capacity shortfall)
- Finite/infinite mode indicator

**Use Cases**  
- Initial schedule; reschedule with locks; expedite placement; view bottlenecks
- What-if analysis (finite vs infinite mode comparison)
- Bottleneck identification and resolution

**KPIs**  
- **Utilization** = used capacity ÷ available capacity (per resource/period). High with lateness → need more capacity; low → balance loading
- **Lateness** = avg/max (scheduled end – required date if >0). High → improve capacity or adjust targets
- **Lock Adherence** = unchanged locked ops ÷ locked ops. Low → scheduler violating constraints
- **Churn per Replan** = ops moved ÷ total ops. High churn erodes shop-floor stability
- **Overload Count** = number of operations flagged as overload in finite mode
- **Bottleneck Detection** = number of resources identified as bottlenecks (utilization > threshold)

### 6.5 Work Order Planning

**Description**  
Generate and release executable WOs from planned supply orders; track execution and reconcile MES feedback. The work order planning system turns net requirements into supply proposals (PO/WO/TO) deterministically and idempotently, supports planned vs firm states, priority/expedite, lead times, and confirmation workflows, and keeps proposals policy-driven (lot sizing, buffers) so behavior can be tuned without code changes.

**Goals**  
- Turn net requirements into supply proposals (PO/WO/TO) deterministically and idempotently
- Support planned vs firm states, priority/expedite, lead times, and confirmation workflows
- Keep proposals policy-driven (lot sizing, buffers) so behavior can be tuned without code changes

**Scope**  
- Proposal generation for SupplyOrders: PurchaseOrder, WorkOrder, TransportOrder
- States: Planned → Confirmed/Firm → Released → InProgress → Completed/Cancelled (align with existing SupplyOrder aggregate)
- Priority/expedite and dates (required/earliest/latest) derived from netting output and policies
- Pegging (optional): maintain linkage from demand to supply proposals for traceability
- Out of scope: capacity scheduling (separate), transport pathfinding (separate), optimization solver specifics

**Inputs**  
- **Netting results**: From Material Planning - net qty, need date, pegging context
- **Policies**: Lot sizing (fixed/min/max/EOQ), lead time buffers, priority/expedite rules, min/cover days, supplier vs internal preference
- **Master data**: Products, StockingPoints, Routing (for WO), Suppliers (for PO), Transport legs (for TO)
- **Reservations**: Material reservations (subtracting active reservations already handled in netting)

**Process (Detailed Steps)**  
1) **Proposal Record Creation**:
   - Define proposal record shape: type (PO/WO/TO), qty (lot-sized), requested/need date, planned/available date, priority/expedite, routingId (WO), supplierId (PO), origin/destination (TO), state=Planned, pegging refs, idempotency key
2) **Lot Sizing**:
   - Apply lot-sizing policy: fixed lot, min/max, EOQ
   - Rounding rules (up, nearest lot)
   - Example: net=120, lot=50 → propose 3 lots (150) or policy to cap at 2 lots (100) with residual rule
3) **Lead-Time Buffers**:
   - Derive planned date = need date – (lead time p50/p95 or buffer%)
   - Lead-time source selection: internal (routing) vs supplier (supplier offer)
   - Example: need T0, lead p50=5d, buffer=20% → planned date T0-6d
4) **Priority/Expedite Mapping**:
   - Map from demand priority or SLA tier
   - Promote when late vs need date
   - Example: Critical demand sets expedite=true and higher priority
5) **Firming Workflow**:
   - Define rules to auto-confirm/firm proposals (e.g., when inside firming window) vs manual
   - Specify emitting Confirm/Plan commands to SupplyOrder aggregate
   - Example: firm POs 5 days before need if supplier lead time is short/locked
6) **Pegging Storage**:
   - Store demand→proposal links (optional) for traceability
   - Update on replan/churn

**Outputs**  
- **Planned SupplyOrder recommendations** with:
  - Type: PO/WO/TO
  - Qty (lot-sized), RequiredDeliveryDate/PlannedDeliveryDate
  - Priority/Expedite flag
  - RoutingId (for WO), SupplierId (for PO), Origin/Destination (for TO)
  - State: Planned (until confirmed)
  - Pegging references (optional)
- Optionally emit events or populate a planning read model (recommendations)

**Policies (Configurable)**  
- Lot sizing: fixed lot, min/max, EOQ; rounding rules
- Lead time buffers: add p50/p95 or buffer% to planned dates
- Priority/expedite: map from demand priority or SLA tier; promote when late vs need date
- Supplier vs internal: prefer internal WO unless policy says compare supplier ATP; fall back to supplier on shortfall
- Firming rules: when to auto-confirm/firm; when to require approval
- Date choice: end-of-period vs start-of-period suggestions

**Integration Points**  
- Material Planning: consumes netting results to propose supply; feeds back into SupplyOrder aggregate or a recommendations projection
- Promise/ATP: firm SupplyOrders appear as inbound; planned orders may be excluded or marked as non-firm unless policy allows
- Transport: for TO proposals, defer to transport planning/pathfinding for dates/routes
- Reservations: no direct creation here; material reservations are upstream; capacity reservations are downstream (capacity module)

**Telemetry / KPIs**  
- Proposal counts, lot-size adjustments, lead-time buffer usage, firming rate, replan churn (changes to planned orders), lateness risk

**ML Hooks (Optional)**  
- Predict lead times (supplier/internal) to set planned dates; predict expedite need; predict churn to minimize replan

**Rules**  
- Partial completions allowed; guarded state transitions; record variance/scrap; respect locks from schedule
- Idempotent proposal generation: key by (demandId, period, type) to avoid duplicates on rerun
- Replan/churn policy: define how to replace/update planned proposals vs leave as-is; versioning or supersede rules

**UI**  
- WO list/detail, progress tracking, variance/rework alerts
- Supply order proposal list (recommendations with qty, dates, priority, expedite, supplier/routing)
- Firming workflow interface (approve/reject proposals)

**Use Cases**  
- Release WO; record partial completion; close WO; register scrap/rework; reconcile MES update
- Generate supply proposals from netting; firm proposals; update proposals on replan

**KPIs**  
- WO cycle time, throughput, redo/partial rate, variance vs plan
- Proposal counts, lot-size adjustments, lead-time buffer usage, firming rate, replan churn

### 6.6 Material Reservations

**Description**  
Hold material for demand/promises in an idempotent, deterministic way. Material reservations support lifecycle: Tentative → Confirmed → Released/Expired/Reduced, subtract reservations in material availability/ATP/netting, and provide clear reason codes on conflicts.

**Goals**  
- Hold material for demand/promises in an idempotent, deterministic way
- Support lifecycle: Tentative → Confirmed → Released/Expired/Reduced
- Subtract reservations in material availability/ATP/netting
- Provide clear reason codes on conflicts; avoid race conditions; support TTL sweeps

**Scope**  
- Material reservations per ProductId + StockingPointId
- Cross-cutting: used by Promise/ATP, planning/netting, and operations that consume material
- Out of scope (for now): capacity/transport reservations (handled separately)

**States & Events**  
- States: Tentative | Confirmed | Released | Expired | Reduced
- Commands: CreateTentative (idempotent key, ttl optional), Confirm, Release, Reduce (lower qty/window), Expire (sweeper)
- Events: ReservationCreated, ReservationConfirmed, ReservationReleased, ReservationReduced, ReservationExpired

**Fields (Aggregate)**  
- Id (deterministic; e.g., hash of orderId/line/scope/window)
- IdempotencyKey (for retry-safe create/confirm)
- ProductId, StockingPointId
- Quantity (decimal, non-negative)
- WindowStart, WindowEnd (DateTimeOffset)
- Origin (e.g., PromiseId/OrderId)
- State, TTL (optional), Created, Modified

**Projection / Query**  
- Projection keyed by (ProductId, StockingPointId), holding totals by state and list of active reservations
- Query service: getAll(), getByProductSp(product, sp), getActiveTotal(product, sp) → decimal
- MaterialAvailabilityService subtracts active (Tentative+Confirmed+Reduced) quantities from on-hand+inbound

**Integration Points**  
- Promise/ATP: on Accept → CreateTentative; on cancel/timeout → Release; on order release/firming → Confirm
- Netting/planning: subtract active reservations when computing net requirements
- Expiry: background sweeper posts Expire for reservations past TTL or window end

**Validation**  
- Non-negative qty; end >= start
- Reduce cannot increase qty; Confirm only from Tentative; Release from Tentative/Confirmed/Reduced
- Expire only if past TTL window

**Concurrency / Races**  
- Idempotent Create with IdempotencyKey
- Deterministic IDs to avoid duplicates
- Sweeper should be idempotent; Release safe to call multiple times

**Telemetry**  
- Emit events/KPIs for created/confirmed/released/expired; track failures and idempotent replays

---


### 6.7 Pegging & Traceability

**Description**  
Maintain end-to-end traceability between demand (customer orders/forecasts) and supply (planned/firm POs/WOs/TOs) plus reservations. Enable clear visibility for ATP/CTP, replanning, and audit (what demand is covered by which supply/reservation).

**Goals**  
- Maintain end-to-end traceability between demand (customer orders/forecasts) and supply (planned/firm POs/WOs/TOs) plus reservations
- Enable clear visibility for ATP/CTP, replanning, and audit (what demand is covered by which supply/reservation)
- Keep peg updates deterministic and idempotent across replans

**Scope**  
- Pegs between demand lines and supply artifacts (SupplyOrders) and/or reservations (material/capacity/transport)
- Supports partial pegging (multiple supplies cover one demand; one supply covers multiple demands)
- Applies to both planned and firm supply; preserves links across replans/churn
- Out of scope: sequencing/scheduling specifics (covered in capacity/transport designs)

**Pegging Model**  
- Entities:
  - DemandRef: { DemandId, LineId, ProductId, StockingPointId, NeedDate, Quantity }
  - SupplyRef: { SupplyOrderId, Type (PO/WO/TO), ProductId, StockingPointId, DeliveryDate, Quantity, State (Planned/Firm/…)}
  - ReservationRef: { MaterialReservationId | CapacityReservationId | TransportReservationId }
- PeggingLink:
  - Id (deterministic from DemandRef + SupplyRef + quantity slice)
  - DemandRef
  - SupplyRef or ReservationRef
  - PeggedQty (decimal)
  - Status: Active | Superseded | Released
  - Created, Modified
- Supports partials: multiple PeggingLinks can exist for one demand; a supply can have multiple links

**Events**  
- PegCreated (demand↔supply/reservation, qty)
- PegUpdated (qty/date changes)
- PegSuperseded (old peg replaced)
- PegReleased (on cancel/replan)

**Lifecycle / Replan**  
- Deterministic generation: key off demand+period and supply proposal identity to avoid duplicates on rerun
- Replan rules:
  - If supply proposal changes, supersede old pegs and create new ones
  - If demand decreases, reduce/release pegs accordingly
  - If demand cancels, release pegs
- Firm supply: pegs persist; replans should not break firm pegs unless explicitly allowed by policy

**Integration Points**  
- Material Planning: when proposing SupplyOrders, create/update pegs to those proposals; subtract reservations separately
- ATP/Promise: can surface pegged supply to show coverage; reservations are already tracked separately
- Capacity/Transport: pegs can reference reservations rather than supply if holding capacity/transport slots (future)
- Replanning: pegging rebuild runs with netting/replan; ensure idempotent keys to avoid duplicates

**Policies**  
- Firm peg protection: disallow changing pegs to firm supply unless policy permits
- Partial allocation rules: fill firm/earliest supplies first, then planned; policy-driven (FIFO/priority)
- Peg visibility: include peg info in promise/plan responses (optional)
- Over-peg handling: cap vs allow, with limiter

**Data / Storage**  
- Pegging projection/read model:
  - Keyed by DemandRef and by SupplyRef for fast lookup
  - Stores active pegs and history (Superseded/Released) if audit needed
- Idempotent key: hash of DemandId+LineId+SupplyId(+slice) to prevent duplicate pegs on rerun

**Telemetry / KPIs**  
- Pegging completeness (% demand pegged), peg churn (changes per replan), peg age, firm peg breakage count

**Examples**  
- One demand, multiple supplies: Demand CO#1 line1 qty 100; Supply WO#A qty 60 (firm), WO#B qty 50 (planned) → pegs: CO1→WOA 60 (Active), CO1→WOB 40 (Active), net pegged 100 (10 over, policy decides to cap at 100)
- Partial reduction: demand drops to 80 → reduce/release CO1→WOB from 40 to 20 or release per policy
- Cancellation: demand cancels → release CO1→WOA and CO1→WOB pegs; supply stays available for other demand if policy allows

---


### 6.8 Routing & Process Management

**Description**  
Model routings and steps (process and transport) with alternates and validity windows. Capture qty-dependent durations, resource requirements, and preferences. Provide selection logic (primary/alternate) driven by policy (time/cost/risk).

**Goals**  
- Model routings and steps (process and transport) with alternates and validity windows
- Capture qty-dependent durations, resource requirements, and preferences
- Provide selection logic (primary/alternate) driven by policy (time/cost/risk)

**Scope**  
- Routing aggregate: steps, precedence, validity (effectivity), preferences/weights
- Routing steps: resource group/skill, setup/processing/yield, optional transport step
- Alternates: whole-routing alternates or per-step alternates
- Qty-duration functions; yields and rework/inspection steps
- **Note**: Routing is master data (not owned by Capacity Planning or MRP)
- **Note**: Capacity CTP owns Work Routing knowledge (for capacity checking)
- **Note**: Transport ATP owns Transport Routing knowledge (for transport checking)

**Inputs**  
- Master data: products, stocking points, resource groups, calendars
- Policies: alternate selection (fastest/cheapest/balanced), validity enforcement, yield handling

**Outputs**  
- Selected routing/steps for a demand (or alternates set)
- Duration estimates per step (qty-adjusted) and aggregate lead estimate

**Integration Points**  
- Promise/ATP: routing selection feeds capacity/transport checks
- Capacity Planning: uses routing steps for resource buckets and qty-duration
- Material Planning: for BOM explosion alignment (not covered here)

**Telemetry / KPIs**  
- Alternate usage rate; invalid/expired routing incidence; selection latency

**ML Hooks (Optional)**  
- Predict step duration/yield; choose alternates based on predicted reliability/cost

---

### 6.9 Transport Management

**Description**  
Model transport legs, calendars, capacities, cutoffs, constraints; provide transport ATP (earliest feasible arrival). Support alternates/modes and k-shortest path search; policy-driven scoring (time/cost/risk/CO2).

**Goals**  
- Model transport legs, calendars, capacities, cutoffs, constraints; provide transport ATP (earliest feasible arrival)
- Support alternates/modes and k-shortest path search; policy-driven scoring (time/cost/risk/CO2)

**Scope**  
- TransportLeg aggregate: origin/destination SP, mode, schedule, capacity, cutoff, constraints (regulatory/hazmat), reliability, CO2
- Transport calendars/itineraries: departures/arrivals
- Transport ATP service: feasibility and earliest arrival; limiter on failures
- Alternates: multiple legs/modes; pathfinding

**Inputs**  
- Legs/calendars, capacity per departure, cutoff rules
- Constraints: regulatory/hazmat/country, blackout periods
- Policies: time vs cost vs risk/CO2, supplier/green preference (if any)

**Outputs**  
- Itineraries with arrival, cost, reliability, CO2; limiter code if infeasible

**Integration Points**  
- **Promise/ATP**: Calls Transport ATP for transport availability checking
- **Transport Planning**: Uses Transport ATP for logistics planning
- **Optimizer**: Calls Transport ATP for optimization transport constraints
- **Reservations (future)**: Seat/slot holds per departure

**Key Architectural Decision: Transport Routing Knowledge Ownership**
- ✅ **Transport ATP owns Transport Routing knowledge** (for transport checking)
- ✅ **Promise uses Transport ATP** (doesn't need transport routing details)
- ✅ **Better separation**: Promise = order promising, Transport ATP = transport checking

**Telemetry / KPIs**  
- Pathfinding latency, success/fail rate, cutoff misses, capacity shortfalls, limiter distribution

**ML Hooks (Optional)**  
- Predict lead times/delays/reliability per leg; adjust itineraries; include model basis/version

---

### 6.10 Supplier Management

**Description**  
Model suppliers and offers; provide supplier ATP (earliest feasible supply) with MOQs/lead times/reliability/cost. Allow policy-driven use (always compare vs only on shortfall) without code changes.

**Goals**  
- Model suppliers and offers; provide supplier ATP (earliest feasible supply) with MOQs/lead times/reliability/cost
- Allow policy-driven use (always compare vs only on shortfall) without code changes

**Scope**  
- Supplier aggregate/offer: lead times (p50/p95), MOQs, lot sizes, capacities/windows, price tiers, incoterms, reliability
- Supplier ATP service: feasibility and scoring (time/cost/reliability)
- Policies: when to query supplier (shortfall vs always), fallback behavior (block vs accept-with-risk vs counteroffer)

**Inputs**  
- Supplier master/offers, capacities, MOQs, lead times, price tiers, reliability scores
- Policies from PromisePolicy/presets

**Outputs**  
- Supplier options with earliest date, qty, cost, reliability; limiter if infeasible (SupplierMOQ/LeadtimeExceeded)

**Integration Points**  
- Promise/ATP: invoked on shortfall or per policy
- Material Planning: can generate PurchaseOrder recommendations from supplier options
- Optimizer: reuses scoring/policies to choose supplier vs internal

**Telemetry / KPIs**  
- Supplier ATP latency; hit rate; limiter distribution; cost deltas vs internal

**ML Hooks (Optional)**  
- Predict lead times/reliability from historical performance; include model basis/version

---

### 6.11 Multi-Objective Optimization Module [Phase 2]

**Description**  
Optimization is a **separate, cross-domain module** that improves planning results beyond heuristics. The optimizer reuses functions from MRP, Capacity CTP, and Transport ATP (no duplication) but remains a separate module because it optimizes across material + capacity + transport together.

**Architectural Decision: Hybrid Multi-Stage Solver Architecture**
To prevent solver timeouts and scaling bottlenecks (solving a large MILP with sequence-dependent setup binaries at a minute-by-minute level is NP-hard and computationally infeasible for 50,000+ operations), the optimization module is decomposed into a **two-stage hybrid architecture**:

```
┌────────────────────────────────────────────────────────────────┐
│   STAGE 1: GLOBAL TACTICAL SOLVER (MILP - CPLEX / OR-Tools)    │
│   - Horizon: 3-6 months. Buckets: Daily/Weekly.                │
│   - Decides: Material flow, POs, TOs, bucketed resource loading│
└──────────────────────────────┬─────────────────────────────────┘
                               │ Bucketed allocations & bounds
                               ▼
┌────────────────────────────────────────────────────────────────┐
│   STAGE 2: LOCAL OPERATIONAL SCHEDULER (RL / Metaheuristics)   │
│   - Horizon: 1-2 weeks. Buckets: Continuous/Hourly.            │
│   - Decides: Sequence-dependent changeover, campaign lengths,  │
│              CIP (cleaning) windows, minute-by-minute order    │
└────────────────────────────────────────────────────────────────┘
```

1. **Stage 1: Global Tactical Solver (MILP)**:
   - *Purpose*: Optimizes the global network. It balances multi-level BOM sourcing, supplier selections, transport leg allocations, and resource group capacity loading.
   - *Time Buckets*: Bucketed into daily or weekly periods.
   - *Solvers*: Uses CPLEX or Google OR-Tools (SCIP/CBC) to solve a Mixed-Integer Linear Programming model.
   - *Output*: Feasible material flows, purchase order suggestions, transport requirements, and bucketed capacity allocations for resources.
2. **Stage 2: Local Operational Scheduler (Reinforcement Learning or Metaheuristic)**:
   - *Purpose*: Solves the detailed line sequencing and campaign setup problem.
   - *Time Horizon*: 1-2 weeks of high-fidelity planning.
   - *Solvers*: A Reinforcement Learning (RL) agent trained on a simulator (Digital Twin) of the factory lines, or a Metaheuristic (Genetic Algorithm/Tabu Search).
   - *Constraints Handled*: Sequence-dependent setup matrix ($SetupTime_{i,j,l}$), family campaign lengths ($MinCamp_{p,l}$), and cleaning windows ($CIP_{l,t}$).
   - *Output*: Executable, sequenced work orders for the shop floor.

**Stage 2 RL Agent Design Specification** (to be detailed before Phase 2 implementation):
- **State Space**: Current line status, pending order queue, remaining capacity per resource, setup matrix position, campaign state, CIP schedule
- **Action Space**: Next product to sequence on each line (discrete, combinatorial)
- **Reward Signal**: Composite of lateness penalty, setup time cost, CIP waste, utilization bonus (weights from policy)
- **Episode Length**: 1-2 week scheduling horizon
- **Training Environment**: Digital Twin simulator of factory lines (fidelity, data pipeline, and sim-to-real transfer strategy TBD)
- **Safety Constraints**: Feasibility enforcement during training; fallback to metaheuristic if RL agent produces infeasible solutions
- **Deployment**: A/B comparison against metaheuristic baseline before full deployment; human-in-the-loop override capability
- **Explainability**: RL agent decisions must produce structured explanations (action rationale, constraint satisfaction status, objective contribution breakdown) for planner review

**When to Run What**  
- **Daytime Heuristic MRP [MVP]**: Fast path (minutes per run) utilizing cached availability and reservations; provides immediate execution plans during daytime operations.
- **Nighttime Full Optimizer [Phase 2]**: Multi-objective global optimization solving network flow and local sequencing. Replaces heuristic MRP results with optimized plans overnight.
- **Triggered Re-run**: Executed during significant disruptions (e.g., calendar outage, large supply cancellation) or when the Agentic Orchestrator detects a critical deviation.

**Goals**  
- Improve plan quality across delivery, utilization, inventory costs, emissions, and stability [Phase 2]
- Decompose optimization into Tactical MILP (Stage 1) and Local Sequencing (Stage 2) [Phase 2]
- Provide mathematical optimization (MILP) for global network flow constraints [Phase 2]
- Support sequence-dependent setups, campaigns, and CIP cleaning windows via metaheuristics/RL [Phase 2]

**Modeling Principles**  
- Policy-driven toggles: finite vs infinite capacity, emissions/cost caps, acceptance policy, churn weight
- Idempotent, deterministic keys outside the solver (reservations/pegs) to avoid double-allocation
- Tight bounds and small big-M; prefer caps/slacks with penalties over infeasibility
- Piecewise-linear where needed; bucketed time (hours/days) aligned to operations cutoffs

**Sets and Indices**  
- \(p \in P\): products/SKUs
- \(s \in S\): stocking points
- \(d \in D\): demand lines
- \(t \in T\): time buckets (ordered)
- \(r \in R_p\): routings/alternates for product \(p\)
- \(k \in K_{p,r}\): steps of routing \(r\)
- \(g \in G\): resource groups
- \(m \in M_{s \to s'}\): transport legs/modes
- \(v \in V\): suppliers
- \(c \in C\): components
- \(b \in B_p\): BOM entries for product \(p\)
- \(h \in H_d\): candidate itineraries (precomputed K-shortest)
- \(l \in L\): production lines (subset/specialization of resource groups) for sequencing/changeovers

**Parameters (Nonnegative Unless Noted)**  
- Demand: \(D_d\), due bucket \(\tau^{due}_d\), priority weight \(w^{prio}_d\), earliness/lateness weights \(w^{early}_d, w^{late}_d\)
- Inventory/supply: on-hand \(Inv_{p,s}\); firm inbound \(Inb_{p,s,t}\); reservations to subtract \(Resv^{mat}_{p,s,t}\); safety floor \(Safety_{p,s}\)
- BOM: \(BOM_{c,p}\) (units of component \(c\) per unit of \(p\))
- Production: lead \(LT^{prod}_{p,r,k}\); duration rate \(Dur_{p,r,k}(\cdot)\); step yield \(Yield_{p,r,k}\); cost \(Cost^{prod}_{p,r,k}\); feasible resources \(AltRes_{p,r,k}\)
- Capacity: available \(Cap_{g,t}\); reserved \(Resv^{cap}_{g,t}\); mode toggle \(Mode^{cap} \in \{\text{finite}, \text{infinite}\}\)
- Transport: capacity \(Cap^{trans}_{m,t}\); reserved \(Resv^{trans}_{m,t}\); lead \(LT^{trans}_m\); cutoff mask \(Cutoff_{m,t} \in \{0,1\}\); cost \(Cost^{trans}_{m,t}\); emissions \(CO2^{trans}_m\); reliability \(Rel^{trans}_m\)
- Supplier: \(MOQ_{v,p,s}\), \(Lot_{v,p,s}\), lead \(LT^{sup}_{v,p,s}\), cost \(Cost^{sup}_{v,p,s}\), reliability \(Rel^{sup}_{v,p,s}\)
- Sequencing & changeover: setup time \(SetupTime_{i,j,l}\) and cost \(SetupCost_{i,j,l}\) for switching from item \(i\) to \(j\) on line \(l\); family map \(Family_{p}\); min run \(MinRun_{p,l}\); min campaign length \(MinCamp_{p,l}\); max campaign length \(MaxCamp_{p,l}\); cleaning windows \(CIP_{l,t} \in \{0,1\}\) (unavailable if 0)
- Transport preference & cost detail: fixed depart cost \(FixCost^{trans}_{m,t}\); variable cost \(VarCost^{trans}_{m,t}\); lane preference penalty \(PrefPen_{m}\); min fill \(MinFill_{m}\)
- Shelf-life / freshness (CPG): shelf-life \(Life_{p}\) (buckets), minimum remaining life at ship \(MinLife_{p}\)
- Replenishment policies: min/max \(MinInv_{p,s}, MaxInv_{p,s}\); cover-days target \(Cover_{p,s}\)
- Policy weights: \(w^{cost}, w^{late}, w^{early}, w^{util}, w^{co2}, w^{churn}, w^{risk}\)
- Churn reference: \(PlanPrev^{prod}_{p,r,k,t}\), \(PlanPrev^{trans}_{m,t}\)
- Caps (optional): emissions \(CO2^{max}\), cost \(Budget^{cost}\), reliability floor \(Rel^{min}\)
- Big-M: tight upper bounds from demand/throughput; avoid loose values

**Decision Variables**  
- \(x^{prod}_{p,r,k,t} \ge 0\): qty completing step \(k\) of routing \(r\) at time \(t\)
- \(x^{sup}_{v,p,s,t} \ge 0\): qty from supplier \(v\) arriving at \(s,t\)
- \(x^{trans}_{m,t} \ge 0\): qty shipped on leg \(m\) departing \(t\)
- \(Inv^{end}_{p,s,t} \ge 0\): end-of-bucket inventory
- \(y_{d,t} \ge 0\): demand \(d\) fulfilled at \(t\)
- \(Back_d, Early_d \ge 0\): lateness / earliness slack
- \(z^{route}_{p,r} \in \{0,1\}\): routing selection (optional exclusivity)
- \(z^{res}_{p,r,k,g} \in \{0,1\}\): resource-group selection (optional)
- \(u^{sup}_{v,p,s,t} \in \mathbb{Z}_{\ge 0},\; b^{sup}_{v,p,s,t} \in \{0,1\}\): MOQ/lot binaries
- \(OverUtil_{g,t} \ge 0\): capacity overload slack (finite mode)
- Reservations (if created by optimizer): \(resv^{mat}_{p,s,t}, resv^{cap}_{g,t}, resv^{trans}_{m,t} \ge 0\)
- Churn: \(dev^{prod}_{p,r,k,t} \ge 0\), \(dev^{trans}_{m,t} \ge 0\)
- Acceptance (optional): \(acc_d \in \{0,1\}\)
- Sequencing/changeover: \(start_{p,l,t} \in \{0,1\}\) (line \(l\) starts product \(p\) at \(t\)); \(seq_{i,j,l,t} \in \{0,1\}\) (changeover from \(i\) to \(j\)); \(run_{p,l,t} \ge 0\) time used by \(p\) on line \(l\) at \(t\)
- Campaign/family: \(camp_{p,l} \in \{0,1\}\) (campaign active); \(fam_{f,l,t} \in \{0,1\}\) for family \(f\) if batching by family
- Shelf-life: \(Inv^{age}_{p,s,t,a}\) (inventory of age \(a\)); \(ship^{age}_{p,s,t,a}\) shipped of age \(a\) (age-bucketed freshness)

**Objective**  
Weighted composite (can be turned into lexicographic by staged solves):
\[
\min \;
w^{late}\!\sum_d w^{prio}_d Back_d
+ w^{early}\!\sum_d w^{prio}_d Early_d
+ w^{cost}(\text{Prod}+\text{Sup}+\text{Trans}+\text{Hold}+\text{Setup})
+ w^{util}\!\sum_{g,t} OverUtil_{g,t}
+ w^{co2}\!\sum_{m,t} CO2^{trans}_m x^{trans}_{m,t}
+ w^{churn}(\sum dev^{prod}+\sum dev^{trans})
- w^{risk}(\sum Rel^{trans}_m x^{trans}_{m,t} + \sum Rel^{sup}_{v,p,s} x^{sup}_{v,p,s,t})
+ \text{LanePrefPenalty} + \text{MinFillPenalty}
\]
Caps (optional hard):
\[
\sum_{m,t} CO2^{trans}_m x^{trans}_{m,t} \le CO2^{max}, \quad
\text{TotalCost} \le Budget^{cost}
\]

**Constraints (Key Categories)**  
1. **Demand fulfillment & tardiness**: \(\sum_t y_{d,t} = D_d\); \(Back_d \ge \sum_{t > \tau^{due}_d} y_{d,t}\); \(Early_d \ge \sum_{t < \tau^{due}_d} y_{d,t}\)
2. **Inventory balance + safety**: \(Inv^{end}_{p,s,t} = Inv^{end}_{p,s,t-1} + Inflow_{p,s,t} - Outflow_{p,s,t}\); \(Inv^{end}_{p,s,t} \ge Safety_{p,s}\)
3. **BOM consumption**: \(\sum_{r,k} x^{prod}_{p,r,k,t'} \cdot BOM_{c,p} \le \text{AvailComponent}_{c,t'}\)
4. **Material reservations subtraction**: \(Outflow_{p,s,t} \le Inv^{end}_{p,s,t-1} + Inb_{p,s,t} - Resv^{mat}_{p,s,t}\)
5. **Routing validity & precedence with yield**: \(x^{prod}_{p,r,k,t} \le M \cdot Valid_{p,r,t} \cdot z^{route}_{p,r}\); \(x^{prod}_{p,r,k+1,t} \le \sum_{t' \le t - LT^{prod}_{p,r,k+1}} x^{prod}_{p,r,k,t'} \cdot Yield_{p,r,k}\)
6. **Capacity (finite/infinite toggle)**: \(\sum_{p,r,k:g\in AltRes} timeReq_{p,r,k,g} \cdot x^{prod}_{p,r,k,t} \le Cap_{g,t} - Resv^{cap}_{g,t} + OverUtil_{g,t}\)
7. **Transport capacity, cutoff, lead**: \(x^{trans}_{m,t} \le Cap^{trans}_{m,t} - Resv^{trans}_{m,t}\); \(x^{trans}_{m,t} \le M \cdot Cutoff_{m,t}\)
8. **Supplier MOQ / lot sizing**: \(x^{sup}_{v,p,s,t} = Lot_{v,p,s} \cdot u^{sup}_{v,p,s,t}\); \(x^{sup}_{v,p,s,t} \ge MOQ_{v,p,s} \cdot b^{sup}_{v,p,s,t}\)
9. **Churn (plan stability)**: \(dev^{prod}_{p,r,k,t} \ge |x^{prod}_{p,r,k,t} - PlanPrev^{prod}_{p,r,k,t}|\)
10. **Sequence-dependent setup / changeover**: \(\sum_{p} run_{p,l,t} + \sum_{i,j} SetupTime_{i,j,l} \cdot seq_{i,j,l,t} \le Cap_{l,t}\)
11. **Minimum run and campaign length**: \(run_{p,l,t} \ge MinRun_{p,l} \cdot start_{p,l,t}\); \(MinCamp_{p,l} \cdot camp_{p,l} \le \sum_t run_{p,l,t} \le MaxCamp_{p,l} \cdot camp_{p,l}\)
12. **Family batching / cleaning (CIP) windows**: \(run_{p,l,t} \le M \cdot fam_{Family_p,l,t}\); \(run_{p,l,t} \le M \cdot CIP_{l,t}\)
13. **Shelf-life / freshness (CPG)**: \(Inv^{age}_{p,s,t+1,a+1} = Inv^{age}_{p,s,t,a} - ship^{age}_{p,s,t,a}\); \(ship^{age}_{p,s,t,a} = 0\) if \(a + Transit > Life_p - MinLife_p\)
14. **Replenishment policy (min/max, cover days)**: \(Inv^{end}_{p,s,t} \ge MinInv_{p,s}\); \(Inv^{end}_{p,s,t} \le MaxInv_{p,s}\); \(Inv^{end}_{p,s,t} \ge Cover_{p,s} \cdot \text{AvgDemand}_{p,s}\)

**Inputs**  
- Problem: operations, resources, constraints, precedence, calendars, locks
- Objectives: lateness/tardiness, utilization, inventory/expedite cost, carbon, churn
- Parameters: weights/epsilon, time limits, gap targets

**Process**  
1) Build model via abstraction (solver abstraction layer)
2) Run solver (CPLEX/OR-Tools or custom heuristic)
3) Fallback to heuristic on timeout/gap
4) Generate solutions/Pareto frontier; compute objective scores
5) Explain trade-offs vs baseline

**Outputs**  
- Optimized schedules, Pareto set, objective scorecards, explanations
- Plan delta (changes from baseline to optimized)
- **Decision Explanations** (XAI): For each optimization decision, provide structured rationale including: which constraints were binding, which trade-offs were made, sensitivity analysis (how much would the objective change if a constraint were relaxed), and a natural-language summary suitable for planner consumption

**Rules**  
- Same constraints as core scheduler; respect locks; fallback required
- Optimizer reuses PromiseOrchestrator/providers with different policy/search budget
- No duplicated business rules; optimizer calls same orchestrator

**UI**  
- Run summary (solver runtime, gap, objective scores)
- Pareto chart (trade-offs between objectives)
- Solution picker (choose from Pareto set)
- Objective breakdown (lateness, cost, utilization, CO2, churn)

**Use Cases**  
- Full optimization run; compare heuristic vs optimized; select Pareto candidate
- What-if analysis: optimize with different policy weights

**KPIs**  
- **Objective Attainment** = achieved objective value ÷ target. High → optimization effective
- **Solver Runtime** = time to solve. Over limit → fallback to heuristic
- **Gap** = (best bound – solution) ÷ best bound. Low → solution quality high
- **Pareto Diversity** = spread of solutions in objective space. High → good trade-off exploration

**Stochastic Planning & Uncertainty Modeling (Phase 2)**
To account for real-world volatility, the Stage 1 Global Solver can be configured for stochastic and robust optimization rather than relying solely on deterministic inputs:
1. **Two-Stage Stochastic Programming**:
   - *First-Stage Decisions*: Sourcing commitments, supplier reservations, and resource group capacity allocations made under uncertainty before specific demands are finalized.
   - *Second-Stage Decisions*: Recourse actions (re-routing transport, pre-empting lower priority orders, expediting material) taken after demand and lead-time realizations.
   - *Scenarios*: Model uncertainty using a scenario tree based on probability distributions of supplier lead times and historical demand variations.
2. **Robust Optimization & CVaR**:
   - *Robust Formulations*: Protect the schedule against worst-case variations in transport leg durations and machine breakdowns within defined uncertainty sets.
   - *Conditional Value-at-Risk (CVaR)*: Minimize tail-risk lateness by optimizing the expected lateness in the worst $\alpha\%$ of scenarios (typically $\alpha = 5\%$), protecting high-priority customer SLAs.

**Digital Twin Simulator & Sim-to-Real Strategy (Phase 2)**
The Stage 2 RL agent requires a high-fidelity simulator of the production floor to learn optimal sequencing policies:
1. **Simulator Fidelity**:
   - Model line setup times ($SetupTime_{i,j,l}$), family campaigns, and Cleaning-in-Place (CIP) constraints.
   - Inject stochastic breakdowns, operator unavailability, and micro-stoppages.
2. **Continuous Telemetry Calibration**:
   - Calibrate simulation parameters (distributions of setup times, cleaning durations, yield factors) continuously using execution telemetry from MES (`WorkOrderCompleted`) and ERP (`MaterialReceived`) streams.
3. **Domain Randomization**:
   - To bridge the "sim-to-real" gap, train the RL agent across randomized variations of lead times, resource efficiencies, and demand distributions. This ensures the learned policy remains robust when deployed to the actual shop floor.

---



### 6.12 Replanning & What-if [MVP & Phase 2]

**Description**  
React to disruptions and evaluate scenarios while preserving locks and minimizing churn. The MVP provides incremental replanning rules, scenario sandboxing, and manual comparisons. Phase 2 overlays the **Agentic Resolution Orchestrator**, which automates exception handling by running transient simulation scenarios, scoring them on the "Total Value" objective function, and presenting natural language recommendations.

**Agentic Exception Resolution Workflow (Phase 2)**
1) **Disruption Ingestion**:
   - The system listens for execution events: `ResourceDowntime`, `MaterialDelay`, `MESVariance` (rework/scrap), or `DemandChange`.
2) **Agentic Trigger**:
   - Instead of merely raising an exception, the Agentic Orchestrator is triggered. It acts as an autonomous planner agent.
3) **Transient Sandbox Generation**:
   - The agent creates transient sandbox projections (Marten document snapshots) representing alternative scenario states.
   - It runs 3 default resolution strategies in parallel:
     - *Strategy A (Expedite & Pre-empt)*: Expedites raw materials and pre-empts lower-priority planned orders.
     - *Strategy B (Alternative Resource)*: Reroutes production to alternate resources/lines, applying the setup matrix penalties.
     - *Strategy C (Delay & Consolidate)*: Delays the affected order to fit into the next scheduled family campaign.
4) **Multi-Objective Scenario Scoring**:
   - Each scenario is evaluated against the multi-objective score:
     $$\text{TotalValue} = w^{cost}(\text{CostDelta}) + w^{late}(\text{LatenessDelta}) + w^{churn}(\text{ChurnDelta}) + w^{co2}(\text{CO2Delta})$$
5) **Natural Language Synthesis**:
   - The agent interfaces with the Generative AI layer to generate a recommendation card:
     > **Disruption**: Machine Line A is down for 8 hours. Affected: Order CO-102 (Due Jan 15).
     >
     > **Recommendation**: Reroute Order CO-102 to Line B.
     > - **Impact**: 0 days delay, Cost delta +$450 (due to setup changeover), CO2 delta negligible.
     > - **Alternative**: Delay Order CO-102 by 24 hours. No cost delta, but 1 day delay.
6) **Human-in-the-loop Execution**:
   - The planner selects the preferred option. The system applies the chosen scenario delta to the active schedule state (Marten snapshot), publishing integration events for downstream consumers.
7) **Agent Memory & Continuous Learning**:
   - The Agentic Orchestrator maintains three memory tiers to optimize future resolutions:
     - *Episodic Memory*: Persists concrete histories of past disruptions, the generated sandbox resolution options, their scores, and the final human planner choice.
     - *Semantic Memory*: Stores global domain facts, constraints, and relationships (e.g., supplier capacities, line capability rules).
     - *Procedural Memory*: Holds planning rules and heuristic weights that adapt based on planner feedback (e.g., if a planner repeatedly selects "Strategy B" despite higher cost, the agent adjusts the weights of $w^{cost}$ and $w^{churn}$ for that user/context).
   - *Model Re-training Hook*: The saved episodes are fed into an offline training pipeline to improve the scenario generation policy of the RL sequencing agent.

**Continuous Planning Mode (Phase 2+)**
Unlike batch nighttime MRP runs, Continuous Planning Mode provides an event-driven execution path that triggers micro-replanning runs dynamically when critical telemetry events are received (e.g., a major transport delay, material shortage, or line breakdown). It evaluates the delta impact in a transient sandbox, calculates the new Pareto frontier, and applies the localized schedule updates in real-time, bridging the gap between real-time Promise (which is per-order) and batch optimization (which is global).

**Goals**  
- React to disruptions and evaluate scenarios while preserving locks and minimizing churn [MVP]
- Detect disruption; classify impact (material vs capacity) [MVP]
- Compute affected ops/orders; propose alternatives respecting locks [MVP]
- Apply incremental replan or generate scenarios; score them; apply chosen plan [MVP]
- Automate scenario execution and structured recommendations via the Agentic Planner [Phase 2]

**Inputs**  
- **Disruption events**: Resource down, material delay, quality flags, MES variance
- **Current plan**: Existing schedule with operations, assignments, reservations
- **Locks/fixed ops**: Fixed operations that should not be moved
- **Priorities/SLAs**: Order priorities, SLA tiers, full-order/full-delivery constraints

**Process (Detailed Steps)**  
1) **Disruption Listeners**:
   - Create disruption event listeners (resource down, material delay, quality issues, MES variance)
   - Detect disruption; classify impact (material vs capacity vs transport)
   - Example: resource down → capacity impact; material delay → material impact
2) **Impact Assessment**:
   - Implement impact assessment logic (assess impact of disruptions on plan)
   - Add impact calculation (affected operations, promises, supply orders)
   - Example: resource down → 5 operations affected, 3 promises at risk
3) **Delta Planner (Preserving Locks/Fixed)**:
   - Create delta planner (minimal changes to existing plan)
   - Implement lock preservation (preserve fixed operations, firm supply orders)
   - Add fixed protection (do not change fixed/firm items unless policy permits)
   - Example: change only affected operations, preserve fixed items
4) **Minimal-Move Strategy**:
   - Implement minimal-move strategy (minimize changes to existing plan)
   - Add churn minimization (minimize operation/supply order changes)
   - Example: move only 2 operations instead of 10
5) **Rollback/Fallback**:
   - Implement rollback logic (revert to previous plan if new plan worse)
   - Add fallback strategy (fallback to heuristic if solver fails)
   - Example: new plan has more lateness → rollback to previous
6) **Scenario Runner (Snapshot/Apply/Compare)**:
   - Create scenario snapshot logic (snapshot current plan state)
   - Implement scenario application (apply what-if changes to snapshot)
   - Add scenario comparison (compare scenarios: finite vs infinite, buffer variations)
   - Example: snapshot → apply 10% buffer increase → compare outcomes
7) **What-If Config Structure**:
   - Define what-if config structure (policy overrides: buffers, finite/infinite, supplier usage, green preference)
   - Implement policy override application (apply overrides to base policy)
   - Example: what-if config = { buffer: +10%, finite: true, supplierUsage: always }
8) **Scenario Diffing/Reporting**:
   - Implement scenario diffing (compare scenario outcomes: promise dates, overloads, cost)
   - Add diff reporting (report deltas between scenarios)
   - Example: scenario A vs B → 3 promises later, 2 overloads reduced, cost +$100
9) **Plan Delta Emission**:
   - Implement plan delta emission (emit changes from old plan to new plan)
   - Add delta structure (operations added/removed/changed, supply orders added/removed/changed)
   - Example: Operation-1 moved from T+5 to T+7, SupplyOrder-2 qty increased 100→120

**Outputs**  
- Replan proposals (alternative plans with minimal changes)
- Applied plan updates (operations moved, supply orders adjusted)
- Scenario scores/diffs (comparison of scenarios with metrics)
- Plan delta (changes from old plan to new plan)

**Rules**  
- Preserve locks/fixed; minimize churn; respect priorities and full-order/full-delivery
- Deterministic replan: same disruption → same replan result (idempotent)
- Firm peg protection: do not break firm pegs unless policy permits

**UI**  
- Disruption alerts (resource down, material delay, quality issues)
- Proposed moves (operations to move, new assignments)
- Scenario comparison with scores (finite vs infinite, buffer variations)
- Plan delta view (changes from old plan to new plan)

**Use Cases**  
- Machine down; material delay; due-date move what-if; choose lowest-lateness scenario
- What-if analysis: compare scenarios with different policies (buffers, finite/infinite, supplier usage)

**KPIs**  
- **Replan Latency** = time from disruption to plan update. Low → faster response to disruptions
- **Stability** = unchanged ops after replan ÷ total ops. Low → tune policies to reduce churn. Target: 80%+ for stable schedules
- **Service Preservation** = orders still on-time after replan ÷ total accepted. High → replan maintains service levels


### 6.13 Campaign Management (No batching)

**Description**  
Group similar operations to reduce setups; no true batching; sequencing still out of scope. Campaign management groups similar operations to reduce setups, supports sequence-dependent changeover modeling, min run and campaign length constraints, family batching and cleaning windows, and campaign reduction factor modeling.

**Goals**  
- Group similar operations to reduce setups; no true batching
- Support sequence-dependent changeover modeling, min run and campaign length constraints
- Family batching and cleaning windows; campaign reduction factor modeling

**Scope**  
- Operations with similarity attributes, campaign types, setup matrices, resources/calendars, priorities
- Sequence-dependent changeover: setup time depends on previous product
- Min run and campaign length constraints
- Family batching and cleaning windows (CIP - Cleaning In Place)
- Campaign reduction factor (efficiency loss for longer campaigns)
- Out of scope: true batching; detailed sequencing (handled separately)

**Inputs**  
- Operations with similarity attributes (product family, setup matrix entries)
- Campaign types: product family, setup matrix, min run, campaign length
- Setup matrices: from product → to product: setup time, cleaning required
- Resources/calendars: resource groups, availability, cleaning windows (CIP)
- Priorities: operation priorities for campaign assignment

**Process (Detailed Steps)**  
1) **Campaign Types & Setup Matrices**:
   - Define campaign types (product family, setup matrix, min run, campaign length)
   - Create setup matrix structure (from product → to product: setup time, cleaning required)
   - Example: Product A → Product B: 2h setup, cleaning=yes
   - Example: Family=SKU-100-*, MinRun=100, CampaignLength=500
2) **Sequence-Dependent Changeover Modeling**:
   - Implement sequence-dependent changeover logic (setup time depends on previous product)
   - Add changeover time calculation (from previous product to next product)
   - Example: A→B: 2h, A→C: 3h, B→C: 1h
   - Implement changeover optimization (minimize total changeover time)
3) **Min Run & Campaign Length Constraints**:
   - Implement min run constraint (campaign must produce at least min run qty)
   - Add campaign length constraint (campaign can produce up to campaign length qty)
   - Example: min run=100, campaign length=500 → campaign must be 100-500 qty
   - Add constraint enforcement (reject campaigns that violate min run or exceed campaign length)
4) **Family Batching & Cleaning Windows**:
   - Implement family batching (group products by family for campaigns)
   - Add cleaning window logic (require cleaning between families)
   - Example: Family A: SKU-100-*, Family B: SKU-200-*
   - Implement cleaning window enforcement (block family B after family A without cleaning)
5) **Campaign Reduction Factor Modeling**:
   - Implement campaign reduction factor (efficiency loss for longer campaigns)
   - Add reduction factor calculation (apply reduction factor to capacity)
   - Example: campaign length >1000 → 5% efficiency loss
   - Add reduction factor to capacity calculations
6) **Campaign Assignment**:
   - Score similarity and setup savings; propose campaign groups
   - Assign campaigns to resources/time buckets; enforce capacity/locks
   - Example: assign 3 operations to 1 campaign (same family)
   - Implement campaign sequencing (optimize campaign order to minimize changeover)
7) **Adherence Tracking**:
   - Track campaign adherence (actual vs planned campaign execution)
   - Add adherence metrics (campaign start/end variance, qty variance, setup time reduction)
   - Example: planned start T+5, actual start T+6 (1 day late)

**Outputs**  
- Campaign definitions/assignments, setup-saving estimates, adherence metrics
- Campaign assignments to resources/time buckets
- Setup time reduction estimates (baseline setup – campaign setup)
- Adherence reports (campaign performance dashboard)

**Rules**  
- No true batching; campaigns approximate grouping; sequencing remains out of scope; respect capacity/locks
- Sequence-dependent changeover: setup time depends on previous product
- Min run and campaign length: enforce constraints per campaign type
- Family batching: group by family; require cleaning between families
- Campaign reduction factor: apply efficiency loss for longer campaigns

**UI**  
- Campaign planner (create/adjust campaigns, assign to resources)
- Setup-saving indicators (time saved by campaigns vs individual setups)
- Compliance view (campaign adherence, lateness impact)
- Setup matrix editor (from product → to product: setup time, cleaning)

**Use Cases**  
- Create/adjust campaign; assign to resource; assess lateness impact; monitor adherence
- Sequence-dependent changeover optimization; family batching with cleaning windows

**KPIs**  
- **Setup Reduction** = (baseline setup – campaign setup) ÷ baseline setup. Measures time saved by campaigns
- **Campaign Adherence** = ops executed within campaign ÷ ops assigned to campaign. Measures compliance
- **Lateness Impact** = lateness delta with/without campaign (simulated). Evaluates whether campaigns help without hurting lateness


## 7. Other Functionality

### 7.1 System Health Monitoring


#### Description and Relevancy
System health monitoring ensures the reliability and availability of the ProductionPlanning platform.

**Monitoring Requirements:**
- Real-time monitoring of planning performance
- Algorithm performance tracking and benchmarking
- System reliability monitoring (99.5% uptime target)
- Planning result quality validation
- Disruption detection and alerting

**Health Checks:**
- Planning engine availability
- Optimization solver health
- Event processing latency
- Database connectivity
- Integration endpoint health

#### Inputs, Process and Outputs
**Inputs:** system metrics (CPU/mem), service liveness/readiness, ingest lag, queue depth, error rates.
**Process:** automated health checks; SLO/SLA alerting; synthetic probes for critical APIs; circuit breakers/back-pressure on ingest.
**Outputs:** health dashboards, alerts (pager/email), maintenance notifications, SLO burn-rate reports.

#### Knowledge
Health check rules and thresholds for API, scheduler, MRP, ingestion, PostgreSQL (Marten), cache, and message queues.

#### Graphical User Interface
Health dashboard with component status, SLO gauges, ingest lag charts, and recent incidents.

#### Use Cases
1. Startup health verification
2. Continuous health monitoring with alerts
3. Automated recovery (restart, failover) when checks fail
4. Operator drill-down into failing component metrics/logs

#### Automation
Fully automated checks and alerting; automated remediation where safe (restart, scale-up), manual approval for higher risk actions.

#### Assumptions
Monitoring/alerting stack deployed; runbooks defined for critical alerts; SLOs defined per flow (ingest, acceptance, run, replan).

### 7.2 Configuration Management

#### Description and Relevancy
Configuration management enables dynamic system configuration without code changes.

#### Inputs, Process and Outputs
**Inputs:** configuration files/values (alg parameters, SLOs, feature flags), environment overrides, secrets.
**Process:** schema validation; safe reload; audit trail of changes; drift detection across environments.
**Outputs:** applied configuration state, change history, validation errors.

#### Knowledge
Configuration schemas, validation rules, defaults, and precedence (env > file > default).

#### Graphical User Interface
Configuration editor with validation feedback, diff to previous, and approval workflow for risky changes.

#### Use Cases
1. Tune planning parameters (e.g., slack, priorities) without redeploy.
2. Environment-specific overrides (dev/test/prod).
3. Enable/disable features (heuristic acceptance path, solver runs).
4. Secrets rotation (handled via secure store).

#### Automation
Semi-automated with validation and role-based approval; automatic propagation after approval.

#### Assumptions
Configs are versioned; rollbacks possible; changes validated in lower envs before prod.

### 7.3 Audit Logging

#### Description and Relevancy
Audit logging provides complete traceability of planning decisions and system activities.

#### Inputs, Process and Outputs
**Inputs:** domain events (decisions, assignments, replans), user actions (overrides, approvals), config changes.
**Process:** write-once structured audit logs (Marten audit stream), retention and access controls.
**Outputs:** audit read model (for UI later), compliance exports, change history per entity.

#### Knowledge
Audit requirements, retention policies, access controls, PII stance.

#### Graphical User Interface
Deferred UI: planned audit viewer (filter/search by order, resource, user, time, action).

#### Use Cases
1. Trace why/when an order was promised/replanned.  
2. Track who changed config/overrode a decision.  
3. Export audit for compliance.

#### Automation
Fully automated event capture; retention configurable; access logged.

#### Assumptions
PostgreSQL (Marten) provides durable snapshot storage; retention storage sized; audit UI deferred to later phase.

---




### 7.4 Validation Approach

**Feature-Level Validation:**
1. **Algorithm Testing**: Mathematical validation of optimization algorithms
2. **Integration Testing**: Cross-system data flow and communication validation
3. **Performance Testing**: Computational performance and scalability validation
4. **Business Validation**: Real-world scenario testing with domain experts

**System-Level Validation:**
1. **End-to-End Testing**: Complete planning workflow validation
2. **Load Testing**: High-volume planning scenario validation
3. **Stress Testing**: Peak load and failure scenario validation
4. **User Acceptance Testing**: Business user validation of planning results

**Quality Assurance Standards:**
- **Algorithm Validation**: Mathematical proof of algorithm correctness where possible
- **Performance Testing**: Comprehensive performance testing for all planning operations
- **Integration Testing**: End-to-end testing of planning workflows
- **Business Validation**: Domain expert validation of planning results
- **Code Coverage**: Automated testing with >95% coverage

### 7.5 AI Governance & Autonomous Guardrails [Phase 2]

**Description**  
AI Governance defines the rules, thresholds, and guardrails under which the agentic planning engine operates. It balances planning agility with human oversight, ensuring autonomous decisions cannot destabilize the system or exceed commercial risk boundaries.

**Autonomy Levels**:
- **Level 1: Pure Advisory (Human-in-the-Loop)**: The AI engine recommends plans (e.g., what-if scenario recommendation cards). Planners must review and manually click "Apply" to execute.
- **Level 2: Guardrailed Autonomy (Human-on-the-Loop)**: The AI engine automatically applies adjustments for minor disruptions that fall within pre-approved boundaries. Planners are notified and can rollback decisions within a configurable window (e.g., 10 minutes).
- **Level 3: Full Autonomy (Human-out-of-the-Loop)**: Reserved for micro-allocations (e.g., shifting resource tasks by less than 15 minutes to account for minor delays) without planner intervention.

**Action Boundaries (Guardrails)**:
The agent is authorized to auto-apply adjustments (Level 2) only if *all* of the following conditions are met:
1. **Cost Threshold**: The cost delta of the new plan vs. the old plan is less than $1,000.
2. **Delivery Impact**: No customer order promise date is delayed by more than 2 hours.
3. **Priority Safeguards**: No high-priority (Gold/Silver) orders are pre-empted or rescheduled.
4. **Setup Churn**: The setup time changeover penalty does not increase by more than 10%.
If any boundary is breached, the agent escalates the decision to Level 1, requiring explicit planner approval.

**Audit & Transparency**:
- **Immutable Log**: Every autonomous action, corresponding sandbox ID, scoring sheet, and evaluation delta is recorded in the Marten structured audit stream.
- **Explainability Payload**: Explanations generated via Explainable AI (XAI) are stored alongside the audit trail to describe why the autonomous action was taken.
- **Rollback Contract**: An API endpoint `POST /api/v1/planning/rollback` allows the user to immediately revert any autonomous or approved plan update to its previous Marten snapshot.

---

## 8. Application/IT Environment

### 8.1 Scale and Performance

**Scale**
- Number of products: < 10,000
- Number of plants: < 20
- Number of resources: < 1,000
- Number of concurrent users: < 50
- Planning horizon: 3-6 months
- Operations per full run: target 50,000+

**Performance**
- Planning run: < 30 minutes (full MRP + capacity)
- Order acceptance (tiered SLA):
  - Simple ATP (stock check only): <1 second
  - Standard CTP (material + capacity): <5 seconds
  - Full CTP (material + capacity + transport + supplier): <30 seconds
- Replan application: < 2 seconds for schedule updates
- Real-time updates: < 2 seconds
- What-if analysis: < 10 seconds for scenario evaluation
- Startup: < 60 seconds
- Memory usage: < 4GB under load (subject to profiling; may increase for 50K+ operation scale)

**Data Processing Requirements**
- Event throughput: Process 1,000+ planning events per minute
- BOM processing: Handle 10,000+ BOM levels efficiently
- Optimization: Solve complex scheduling problems in real-time

**Success Metrics**
- **Technical Success Metrics**:
  - Planning performance: <30 minute planning cycles, tiered order acceptance SLA (Simple ATP <1s, Standard CTP <5s, Full CTP <30s)
  - Schedule quality: 95%+ constraint satisfaction, 90%+ resource utilization
  - Optimization accuracy: 85%+ improvement over manual planning
  - System reliability: 99.5% uptime for planning operations
  - Replanning speed: <2 seconds for schedule adjustments
- **Business Success Metrics**:
  - Resource utilization: 90% average utilization across production resources
  - On-time delivery: 95% on-time delivery achievement
  - Inventory optimization: 98% material availability with <10% excess inventory
  - Planning efficiency: 20% reduction in planning effort and time
  - Cost reduction: 25% reduction in production costs through optimization

### 8.2 Security Provisioning

**Authentication**
- Integration with enterprise identity providers
- Role-based access control
- API key management for external systems

**Authorization**
- Planner roles with least privilege
- Data-level security for multi-plant operations
- Audit trails for all planning changes

**Communication Security**
- TLS 1.3 encryption for all communications
- Secure API gateways (rate limits, WAF)
- Database encryption at rest and in transit
- Secrets managed via secure store

### 8.3 Disaster Recovery & Business Continuity

**Recovery Objectives:**
- **RPO (Recovery Point Objective)**: <5 minutes (PostgreSQL WAL shipping / streaming replication)
- **RTO (Recovery Time Objective)**: <30 minutes (container restart + cache warm-up)

**Backup Strategy:**
- PostgreSQL: Continuous WAL archiving + daily base backups
- Configuration: Version-controlled in Git
- Planning state: Marten snapshots are the source of truth; reconstructable from latest snapshot + pending integration events

**Failover:**
- Primary/standby PostgreSQL with automatic failover (Patroni or cloud-managed)
- Stateless planning engine containers: Kubernetes handles restarts and scaling
- Redis cache: volatile by design; rebuilt on startup from Marten projections

### 8.4 Data Retention & Lifecycle

- **Planning snapshots**: Retained for 12 months (configurable per tenant)
- **Audit logs**: Retained for 7 years (compliance-driven, configurable)
- **Integration events**: Retained for 90 days in hot storage, archived to cold storage thereafter
- **Telemetry/metrics**: 30 days hot, 12 months cold
- **Old planning runs**: Summarized and archived; raw data purged after retention window

### 8.5 API Versioning

- REST APIs use **URL-based versioning** (e.g., `/api/v1/promise`, `/api/v2/promise`)
- GraphQL schema evolution follows **additive-only** changes; deprecated fields marked with `@deprecated` directive
- Breaking changes require a new major version; old version supported for 6 months minimum
- API contracts published as OpenAPI 3.x specifications

### 8.6 Observability

- **Distributed Tracing**: OpenTelemetry (OTLP) with correlation IDs across Promise → Capacity CTP → Transport ATP call chains
- **Structured Logging**: JSON-formatted logs with correlation IDs, aggregate IDs, and operation context
- **Metrics**: Prometheus-compatible metrics for planning run duration, solver gap, cache hit rates, provider latencies
- **Planning Health Dashboard**: Aggregated "planning health score" combining KPIs (promise accuracy, utilization, lateness)
- **Alerting**: SLO burn-rate alerts for critical paths (promise latency, planning run duration, data staleness)

---

## 9. Integration

### 9.1 General

ProductionPlanning natively supports integration using standard data structures including:

- Production orders and operations
- Inventory levels and movements
- Resource availability and utilization
- Material requirements and allocations

**Integration Architecture:**
- Event-driven integration with Nexus for demand signals and master data enrichment
- REST and GraphQL APIs for external system communication
- WebSocket/SignalR for real-time updates
- Message queues for async communication
- API gateway for external access with rate limiting and security

**Critical Integration Points:**
- **Nexus**: Demand signals and master data enrichment
- **ERP Systems**: Product, BOM, and supplier master data
- **MES Systems**: Shop floor execution and progress data
- **Resource Management**: Capacity and availability data
- **Quality Systems**: Quality specifications and results
- Work order status and progress
- Calendars, routings (with alternates), BOMs, reservations, forecasts

### 9.2 Nexus Integration

**Event-Driven Communication:**
- Subscribes to Nexus event streams for demand signals
- Publishes planning results as events to Nexus
- Real-time synchronization of production schedules

**API Integration:**
- REST APIs for demand signal consumption
- WebSocket connections for real-time updates
- GraphQL endpoints for complex queries
- Contracts: idempotent ingest (by message ID), schema validation, lag and error metrics, back-pressure handling.

### 9.3 ERP Integration

**Data Exchange:**
- Standard integration interfaces for order data
- Material master data synchronization
- Production order status updates
- Inventory level reconciliation
- BOM/routing/alternate updates; supplier/lead-time updates; reservations and POs back to ERP.
- Idempotent upsert to Marten document store; validation and missing-reference reporting; retries with DLQ.

**Supported Systems:**
- SAP ERP
- Oracle ERP
- Microsoft Dynamics
- Custom ERP systems via APIs

### 9.4 MES Integration

**Shop Floor Integration:**
- Work order release and tracking
- Operation status updates
- Resource utilization feedback
- Quality control data exchange
- Partial completions, scrap, rework triggers.

**Real-Time Synchronization:**
- Event-driven updates from shop floor
- Automated replanning based on actual progress
- Performance metric collection
- Contracts: at-least-once event ingestion with idempotency; reconciliation rules for quantity/time variance; latency/lag metrics.

---

## 10. Appendix A: Technical Architecture [MVP & Phase 2]

### 10.1 System Components

#### Planning Engine
- **Heuristic Engine [MVP]**: Fast, rule-based ATP/CTP-lite calculations and MRP netting logic.
- **Stage 1 Tactical Solver (MILP) [Phase 2]**: CPLEX or Google OR-Tools for global network balancing.
- **Stage 2 Operational Scheduler [Phase 2]**: Google OR-Tools CP-SAT (primary), Reinforcement Learning (RL) agent, and Metaheuristic (Genetic Algorithm) engines for setup-dependent sequencing.
- **Agentic Orchestrator [Phase 2]**: Sandbox manager that handles exception routing, triggers simulations, and scores outcomes.
- **Generative AI Integration Layer [Phase 2]**: Natural Language Interface executing intent parsers and translating user prompts to planning queries.
- **Closed-Loop Feedback pipeline [Phase 2]**: Event stream subscribers capturing actual MES/ERP execution deviations and publishing updated planning variables.

#### Optimization Algorithm Architecture
- **CPLEX/OR-Tools Integration [Phase 2]**: Industry-standard optimization solvers for Stage 1 global capacity/material balancing.
- **Custom Algorithms [MVP]**: 
  - Finite capacity bucket-based scheduling.
  - Material optimization (lot sizing, inventory netting algorithms).
- **CP-SAT, Reinforcement Learning & Metaheuristics [Phase 2]**: OR-Tools CP-SAT scheduling model, localized line scheduling simulator, and reward optimization for Stage 2 sequencing.
- **Real-Time Planning Pipeline**: Event-driven planning updates triggered by business events.
- **Incremental Planning**: Support for partial schedule updates without full replan.
- **Constraint Management**: Robust constraint satisfaction and validation.
- **Result Explanation**: Clear explanation of planning decisions and trade-offs.

#### Data Management
- **Marten (PostgreSQL Document Store)**: Snapshot-based aggregate persistence, CQRS read-model projections, and audit streams.
- **PostgreSQL**: Relational database for read models, calendars, and routing projections.
- **Redis Cache**: In-memory cache for fast capacity bucket checks.
- **Integration Events**: Event-driven updates to read models and downstream systems.

#### Integration Layer
- **REST and GraphQL APIs**: Standard integration endpoints.
- **WebSocket/SignalR**: Live planning updates to the web dashboard.
- **Message queues**: Async communication with Nexus.
- **API gateway**: Enterprise access with rate limiting and security.

#### User Interface
- **Web-based planning dashboard**: Gantt, inventory projections, and constraint views.
- **Generative AI Chat panel [Phase 2]**: NLP panel for co-pilot conversations.
- **What-if Scenario editor**: Sandbox comparison interface.
- **Mobile-responsive design**: Web UI optimized for shop floor tablets.

### 10.2 Data Flow Architecture

```
Nexus (Demand Events)
        ↓
ProductionPlanning Engine
        ↓ (Planning Decisions)
Marten/PostgreSQL (Aggregate Snapshots + Read Models)
        ↓ (Integration Events)
MES/Shop Floor Systems
```

### 10.3 Scalability Analysis

**Current Load:** 1000 operations/day
**Target Load:** 50,000 operations/day
**Scaling Strategy:** Horizontal scaling with Kubernetes
**Performance Baseline:** Sub-30 minute planning cycles

**Scalability Requirements:**
- **Operations**: Support for 50,000+ operations per planning run
- **Resources**: Handle 1,000+ resources with capacity constraints
- **Time Horizon**: 3-6 month planning horizon
- **Concurrent Users**: Support 50+ concurrent planning users
- **BOM Processing**: Handle 10,000+ BOM levels efficiently
- **Event Throughput**: Process 1,000+ planning events per minute

**Performance Targets:**
- **Planning Run Time**: <30 minutes for full optimization
- **Order Acceptance (tiered SLA)**:
  - Simple ATP (stock check only): <1 second
  - Standard CTP (material + capacity): <5 seconds
  - Full CTP (material + capacity + transport + supplier): <30 seconds
- **Schedule Updates**: <2 seconds for real-time adjustments
- **What-If Analysis**: <10 seconds for scenario evaluation
- **Memory Usage**: <4GB under normal load (subject to profiling; may increase for 50K+ operation scale)

### 10.4 Supply Chain Network Graph [Phase 2]

**Description**  
To capture the dependencies and risk propagation throughout the supply chain, the system models the network as a directed multi-graph:
$$\mathcal{G} = (\mathcal{V}, \mathcal{E})$$
where:
- **Nodes ($\mathcal{V}$)**: Represent stocking points, production plants, supplier nodes, and specific resource work centers.
- **Edges ($\mathcal{E}$)**: Represent BOM consumption relationships, process routing steps, and transportation legs. Edges carry metadata such as capacity, schedules, lead-time probability distributions, cost, and reliability metrics.

**Graph Neural Network (GNN) Applications**:
During Phase 2, a Graph Neural Network (GNN) can be trained over this network structure to perform:
1. **Disruption Cascade Prediction**: Estimating the downstream impact of localized delays (e.g., how a supplier delay at Node A propagates to customer orders at Node Z).
2. **Network-Aware Lead Time Estimation**: Dynamically predicting edge traversal times under variable congestion and capacity loading.
3. **Graph-Enhanced Optimization Bounds**: Generating search bounds and hot-start solutions for the Stage 1 MILP flow solver by identifying bottleneck cut-sets.

### 10.5 Core Domain Schema Placeholders [MVP & Phase 2]

**Description**  
This section provides structural type schemas representing the core aggregates and models across Medhāvī modules.

```fsharp
namespace Medhavi.Common.Domain

open System

/// Represents a customer order line in the system.
type CustomerOrder = {
    OrderId: string
    LineId: string
    ProductId: string
    StockingPointId: string
    Quantity: decimal
    DueDate: DateTimeOffset
    Priority: int
    IsExpedited: bool
}

/// Tracks material holds to prevent double-booking.
type MaterialReservation = {
    ReservationId: string
    IdempotencyKey: string
    ProductId: string
    StockingPointId: string
    Quantity: decimal
    WindowStart: DateTimeOffset
    WindowEnd: DateTimeOffset
    State: string // Tentative, Confirmed, Released, Expired, Reduced
    TTL: DateTimeOffset option
}

/// Projections of capacity availability over resource groups.
type ResourceCalendar = {
    ResourceId: string
    ResourceGroupId: string
    CapacityFactor: decimal
    Downtimes: (DateTimeOffset * DateTimeOffset) list
    AllocatedHours: Map<DateTimeOffset, decimal>
}

/// Transportation lane specifications.
type TransportLeg = {
    LegId: string
    OriginStockingPointId: string
    DestinationStockingPointId: string
    Mode: string
    Schedules: DateTimeOffset list
    CapacityPerDeparture: decimal
    CutoffDuration: TimeSpan
    LeadTime: TimeSpan
    Cost: decimal
    CO2Emissions: decimal
    Reliability: float
}

/// Tracks execution variance for closed-loop ML feedback.
type PlanningDeviation = {
    DeviationId: string
    ProductId: string
    ResourceId: string option
    SupplierId: string option
    PlannedDuration: TimeSpan
    ActualDuration: TimeSpan
    Variance: TimeSpan
    ExecutionDate: DateTimeOffset
}
```

---

## 11. Appendix B: Terminology

| Term            | Description                                         |
| --------------- | --------------------------------------------------- |
| APS             | Advanced Planning & Scheduling system               |
| MRP             | Material Requirements Planning                      |
| Finite Capacity | Resource scheduling with capacity constraints       |
| Campaign        | Grouped production operations for efficiency        |
| Work Order      | Executable production order for shop floor          |
| Marten          | .NET document store library for PostgreSQL          |
| CQRS            | Command Query Responsibility Segregation pattern    |
| BOM             | Bill of Materials defining product structure        |
| CP-SAT          | Constraint Programming - Boolean Satisfiability solver (Google OR-Tools) |
| MILP            | Mixed-Integer Linear Programming                    |
| RL              | Reinforcement Learning                              |
| CTP             | Capable-to-Promise                                  |
| ATP             | Available-to-Promise                                |

---

**Document Control:**
- **Version**: 1.2
- **Status**: Draft
- **Review Date**: Monthly
- **Owner**: Medhāvī Development Team
- **Architecture**: Nexus → ProductionPlanning → MES
- **Persistence**: Marten (PostgreSQL) snapshots + integration events
- **Integration**: Event-driven with REST APIs

---
