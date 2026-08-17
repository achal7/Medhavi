# Demand Intelligence Specification

**Status:** Authorized
**Domain Code:** D
**Governed By:** Specification Meta-Model & Platform Governance (ARS)
**Traceability:** CN-001, CN-002, CN-003, CN-004, CN-005, CN-006, CN-008, CN-009, CN-010, CN-012, ARS §2, §3, §4, §5, §6, §7, §8, §9, §10, §15, §16, §17, §18, §19, §22, Enterprise Semantic Model, Specification Meta-Model & Platform Governance

# Chapter 1 — Purpose & Scope

## 1.1 Purpose

Demand Intelligence is the authoritative enterprise domain responsible for developing trusted understanding of customer demand. It answers the enterprise question:

> **What demand exists, how is it expected to evolve, and what does the enterprise understand that demand to mean?**

Every observation, forecast, segmentation, classification, prioritisation, quality evaluation, exception detection, explanation, and learning activity related to demand originates from and is governed by this specification.

Demand Intelligence consumes the Enterprise Picture (SE-C-021) to interpret current demand facts. It does not duplicate the enterprise’s raw demand data; it provides the authoritative interpretation of what that data means. The output of Understand Demand is a **Demand Understanding** — a published, versioned interpretation of demand health, risk, and confidence, not a second snapshot of demand facts.

Planning, Promise Intelligence, Scenario Intelligence, and Knowledge Intelligence are consumers of Demand Intelligence. They are not the reason Demand Intelligence exists. The domain exists to understand demand; those domains consume that understanding.

## 1.2 Scope

**Included:** Demand observation capture, demand signal ingestion, demand understanding, demand sensing, demand forecasting at all aggregation levels and time horizons, demand segmentation, demand behavior classification, demand prioritisation, demand quality evaluation, demand exception detection, demand explainability, and continuous demand learning.

**Excluded:** Supply planning, inventory planning, capacity planning, production scheduling, procurement planning, distribution planning, order promising, and transportation execution. These belong to their respective Intelligence domains. Customer relationship management, pricing, and promotion management are external inputs to Demand Intelligence, not owned by it.

## 1.3 Responsibility Boundary

The responsibility of Demand Intelligence begins when a business observation capable of influencing enterprise demand is received. It continues through observation evaluation, demand interpretation, demand sensing, demand forecasting, demand segmentation, demand behavior classification, demand prioritisation, demand quality evaluation, demand exception detection, demand explainability, and demand learning.

The responsibility of Demand Intelligence ends when its governed outputs — Demand Understanding, Demand Sensing outputs, Forecast Publications, Demand Segmentation outputs, Demand Behavior Classification outputs, Demand Prioritization outputs, Forecast Quality Assessments, Demand Exception Evidence, Demand Explanations, and Demand Learnings — have been published or explicitly not produced under governance.

## 1.4 Architectural Position

Demand Intelligence is a domain specification. It derives its authority from the Constitution, the Architecture Reference Standard, and the Enterprise Semantic Model. It does not redefine enterprise concepts owned by Core.

Every enterprise concept consumed — Item, Location, Customer, Planning Scope, Enterprise Picture, Planning Period, Calendar — is referenced by its Enterprise Semantic Object identifier (SE-C-xxx). The domain produces interpretations, assessments, forecasts, classifications, and learnings. The single source of truth for point-in-time demand facts is the Enterprise Picture (SE-C-021).

## 1.5 Out of Scope

- Strategic network design
- Customer relationship management
- Pricing and promotion management (inputs to Demand Intelligence, not owned by it)
- Sales order processing and execution
- Warehouse and transportation execution
- Supplier collaboration
- Manufacturing execution

## 1.6 Enterprise Questions

Every capability in the Demand domain will answer one or more of the following enterprise questions. Capabilities with multiple enterprise questions shall define distinct Capability Responsibilities for each question. The questions are ordered by the causal reasoning pipeline established in §1.7.

| ID | Enterprise Question | Capability |
|----|---------------------|------------|
| EQ-D-001 | What demand has the enterprise observed, and is that observation trustworthy? | Understand Demand |
| EQ-D-002 | What does the enterprise currently understand about demand — what patterns exist, what is healthy, what is at risk? | Understand Demand |
| EQ-D-003 | What has changed in demand behavior that the enterprise now understands to be true? | Sense Demand |
| EQ-D-004 | What future demand does the enterprise project, with what confidence, under what assumptions? | Forecast Demand |
| EQ-D-005 | How should the enterprise segment demand entities to enable differentiated planning strategies? | Segment Demand |
| EQ-D-006 | What behavior does this demand exhibit, and what does that behavior mean for forecasting model selection? | Classify Demand |
| EQ-D-007 | Which demand entities are most important to the enterprise’s objectives, and why? | Prioritize Demand |
| EQ-D-008 | How accurate, stable, and valuable is the enterprise’s demand forecasting capability? | Evaluate Demand Quality |
| EQ-D-009 | What situations in the demand picture require enterprise attention because they violate governed policies, given the current Demand Understanding, Forecast Quality Assessment, and Demand Behavior patterns? | Detect Demand Exceptions |
| EQ-D-010 | Why did the enterprise reach, or deliberately not reach, this demand conclusion, and what evidence supports it? | Explain Demand |
| EQ-D-011 | What has the enterprise learned about demand behavior and forecasting performance that should improve future planning? | Learn From Demand |

## 1.7 Demand Intelligence Pipeline

The following pipeline captures the causal reasoning flow of the Demand Intelligence domain. It is a domain-level architectural pattern, not an ARS mandate. Every capability owns one stage of this pipeline. Feedback loops — where assessments and learnings influence future cycles — are described in the Capability Model.

Demand observations flow into the **Enterprise Picture (SE-C-021)** , which is owned by Core. When the Enterprise Picture is published, the Demand Intelligence domain receives **BN-C-001 Enterprise Picture Published** and revises the Demand Understanding. The Demand Understanding does not consume raw demand observations directly; it interprets the authoritative demand snapshot provided by the Enterprise Picture.

```
Demand Observation (evaluated)
        │
        ▼
Enterprise Picture (Core) ─── accumulates demand references
        │
        ▼
BN-C-001 Enterprise Picture Published
        │
        ▼
Demand Understanding
        │
        ▼
Demand Sensing
        │
        ▼
Demand Projection (Forecast)
        │
        ▼
Demand Segmentation
        │
        ▼
Demand Behavior Classification
        │
        ▼
Demand Prioritisation
        │
        ▼
Demand Quality Assessment
        │
        ▼
Demand Exception Assessment
        │
        ▼
Demand Explanation
        │
        ▼
Demand Learning
```

**Feedback Loops:**
- Demand Behavior (Sense) feeds back into Forecast Demand for out-of-cycle refresh.
- Demand Quality Assessment feeds into Learn From Demand for model and policy improvement.
- Demand Exception patterns feed into Learn From Demand for detection policy refinement.
- Demand Learnings feed back into Planning Governance and all upstream capabilities in subsequent planning cycles.

## 1.8 Traceability

| Artifact | Reference |
|----------|-----------|
| Constitution | CN-001 (Constitutional Supremacy), CN-003 (Single Source of Truth), CN-004 (Single Semantic Ownership), CN-010 (Layer Integrity) |
| Architecture Reference Standard | §18 (Document Architecture) |
| Enterprise Semantic Model | SE-C-001 Item, SE-C-002 Location, SE-C-003 Customer, SE-C-010 Planning Scope, SE-C-011 Scenario, SE-C-013 Demand, SE-C-021 Enterprise Picture, SE-C-022 Timestamp, SE-C-023 Quantity, SE-C-033 Calendar, SE-C-034 Planning Period, SE-C-035 Performance Indicator Catalog, SE-C-036 Performance Indicator, SE-C-037 Enterprise Governed Vocabulary |
| Core Domain Specification | CA-C-019 Enterprise Picture Management, CA-C-020 Core Exception Management, BN-C-001, SE-C-019 Exception lifecycle |

---

# Chapter 2 — Business Objectives

| ID | Objective | Traceability |
|----|-----------|--------------|
| BO-D-001 | Deliver Trusted Demand Understanding | CN-003, CN-004, CN-006 |
| BO-D-002 | Improve Planning Effectiveness | CN-002, CN-012 |
| BO-D-003 | Improve Enterprise Responsiveness | CN-002, CN-007 |
| BO-D-004 | Improve Customer Outcomes | CN-005, CN-011 |
| BO-D-005 | Increase Planning Automation | CN-007, CN-011 |
| BO-D-006 | Continuously Improve Enterprise Intelligence | CN-012 |

### BO-D-001 — Deliver Trusted Demand Understanding

**Statement:** The enterprise shall possess a single, governed, explainable interpretation of current and future demand that every downstream capability can rely upon.

**Rationale:** Demand is the foundation of all planning. If different capabilities use different demand interpretations, plans will be inconsistent, promises will be unreliable, and governance will be impossible. A single authoritative Demand Understanding, built from trusted observations and published on a governed cadence, ensures that every planning decision starts from the same demand reality.

**Measures:** PI-D-102, PI-D-205, PI-D-206.

### BO-D-002 — Improve Planning Effectiveness

**Statement:** The enterprise shall continuously improve the accuracy, stability, and value of its demand projections, enabling downstream capabilities to make better planning decisions.

**Rationale:** The quality of every supply plan, inventory policy, and promise commitment depends on the quality of the demand projection. Improving forecast accuracy, reducing bias, and increasing forecast stability directly improve downstream outcomes.

**Measures:** PI-D-002, PI-D-003, PI-D-004, PI-D-005, PI-D-006, PI-D-007.

### BO-D-003 — Improve Enterprise Responsiveness

**Statement:** The enterprise shall detect meaningful changes in demand behavior as they occur, enabling timely replanning before service is affected.

**Rationale:** Demand patterns shift continuously — promotions, competitive actions, supply disruptions, and market changes. The enterprise that detects these shifts earliest can replan before stockouts or excesses occur. Demand sensing bridges the gap between periodic forecasting and real-time awareness.

**Measures:** PI-D-102, PI-D-103.

### BO-D-004 — Improve Customer Outcomes

**Statement:** The enterprise shall understand which demand entities are most critical to customer outcomes, and shall ensure that planning attention, exception handling, and allocation decisions prioritise those entities appropriately.

**Rationale:** Not all demand is equal. A stockout on a critical item for a strategic customer has different consequences than a stockout on a low-priority item. The enterprise must know which demand matters most and act accordingly.

**Measures:** PI-D-004, PI-D-005.

### BO-D-005 — Increase Planning Automation

**Statement:** The enterprise shall automate demand planning activities wherever governance and quality thresholds permit, freeing planners to focus on exceptions, strategy, and improvement.

**Rationale:** Manual forecasting, segmentation, and classification do not scale. Automation increases velocity and consistency. Governance ensures that automation does not compromise accountability or explainability.

**Measures:** PI-D-103, PI-D-202.

### BO-D-006 — Continuously Improve Enterprise Intelligence

**Statement:** The enterprise shall systematically analyse demand outcomes, patterns, and exceptions to discover learnings that improve future demand planning policies, models, and practices.

**Rationale:** Each planning cycle produces data that, if analysed, reveals patterns that can improve the next cycle. Continuous learning transforms operational experience into systemic improvement.

**Measures:** PI-D-107.

---

# Chapter 3 — Enterprise Measurement Model

Every Performance Indicator in this chapter is a governed instance of **SE-C-035 – Performance Indicator**. The measured values computed and published by the Owning Capability are **Knowledge Artifacts** (KA-xxx) following the ARS §3.3 minimum interface. Concrete threshold values do not appear in the PI definitions; they are owned by the referenced policies.

## Measure Taxonomy

| Category | Description | Indicators |
|----------|-------------|------------|
| **Accuracy** | How closely forecasts match actual demand, and how reliable those forecasts are | PI-D-002, PI-D-003, PI-D-004, PI-D-005, PI-D-103 |
| **Stability** | How consistent forecasts are between cycles | PI-D-007 |
| **Value** | How much the forecasting process improves over naive methods | PI-D-006 |
| **Platform Governance** | How well the Medhavi platform produces trustworthy, timely, complete, and explainable demand outputs | PI-D-102, PI-D-107, PI-D-202, PI-D-205, PI-D-206 |

## PI Specification Standard

Each Performance Indicator follows this canonical structure:

1. **Enterprise Measure Definition** – Identifier, Name, Category, Nature, Owning Capability, Governed By, Enterprise Question, Business Objectives Served.
2. **Enterprise Meaning** – What enterprise knowledge this measure represents.
3. **Measurement Model** – Measure type, behavior, context, scope, validity.
4. **Measurement Inputs** – Semantic Dependencies table mapping every variable to its authoritative source.
5. **Calculation Model** – Formula, assumptions, precision.
6. **Composite Methodology** (for composite indices only) – Normalisation, aggregation, weighting, missing value handling, confidence.
7. **Publication Model** – What Knowledge Artifact (KA-D-xxx) is produced, its contents, and the publication contract.
8. **Interpretation Governance** – Reference to the policy that defines thresholds; no concrete thresholds appear here.
9. **Consumers** – Decisions, algorithms, assessments, and dashboards that consume the measured values.
10. **Explainability** – How every published measurement is traceable to its inputs.
11. **Traceability** – Owning capability, governing policy, consumed objects, produced artifact, and consumers.

### PI-D-002 — Forecast Accuracy

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-002 |
| Name | Forecast Accuracy |
| Measure Category | Enterprise Accuracy Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | What percentage of forecasted demand values fall within the acceptable accuracy tolerance of actual demand? |
| Business Objectives Served | BO-D-002, BO-D-004 |

#### 2. Enterprise Meaning

Forecast Accuracy represents the enterprise's understanding of how often its demand projections are close enough to actual demand to be useful for planning. It is the simplest measure of forecast reliability: for each forecasted value, was it right? The measure does not distinguish between small and large errors; those are captured by WAPE and MAPE.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Historical (actuals vs. published forecast) |
| Measure Scope | Planning Scope, Product, Product Family, Location, Business Unit |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Forecast Values | Forecast Publication (SE-D-003). | Resolved – SE-D-003. |
| Actual Demand Values | Enterprise Picture (SE-C-021) or Demand History. | Resolved – SE-C-021. |
| Accuracy Tolerance | Defined by PO-D-041. | Resolved – PO-D-041. |

#### 5. Calculation Model

**Formula:**
```
Forecast Accuracy = (Number of Forecasts within Tolerance ÷ Total Number of Forecasts) × 100
```

**Calculation Assumptions:** The accuracy tolerance (e.g., ±10%) is defined by PO-D-041. A forecast is "accurate" if the absolute percentage error is within the tolerance.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Forecast Accuracy Measurement (KA-D-002) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation of measured values is governed by **PO-D-041 – Forecast Measurement Policy**. The policy defines the accuracy tolerance and the thresholds for Excellent, Good, Warning, and Critical classifications.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Consumed as a core accuracy dimension. |
| Learn From Demand (CA-D-010) | Analysed for accuracy trends and model performance patterns. |
| Dashboards | Displayed for management oversight. |

#### 9. Explainability

Every published Forecast Accuracy Measurement is traceable to the specific forecast values, actual demand values, and the accuracy tolerance used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003, SE-C-021 |
| Produces | KA-D-002 – Forecast Accuracy Measurement |
| Used By | SE-D-008, CA-D-010, Dashboards |

---

### PI-D-003 — Weighted Absolute Percentage Error (WAPE)

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-003 |
| Name | Weighted Absolute Percentage Error |
| Measure Category | Enterprise Accuracy Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | What is the average magnitude of forecast error, weighted by the volume of each demand series? |
| Business Objectives Served | BO-D-002 |

#### 2. Enterprise Meaning

WAPE represents the enterprise's understanding of overall forecast error, weighted so that high-volume items contribute proportionally more to the measure than low-volume items. It answers: "Across everything we forecast, how far off are we, on average?" WAPE is volume-weighted. It reflects the enterprise-level error: high-volume items contribute proportionally more. Use WAPE to evaluate overall forecast quality from an enterprise perspective. Complemented by PI-D-004 (MAPE) for series-level error assessment.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Historical (actuals vs. published forecast) |
| Measure Scope | Planning Scope, Product Family, Location, Business Unit, Enterprise |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Forecast Values | Forecast Publication (SE-D-003). | Resolved – SE-D-003. |
| Actual Demand Values | Enterprise Picture (SE-C-021) or Demand History. | Resolved – SE-C-021. |

#### 5. Calculation Model

**Formula:**
```
WAPE = (Σ |Forecast − Actual| ÷ Σ Actual) × 100
```

**Calculation Assumptions:** Aggregation is across all series in the evaluation scope. Weighting is by actual demand volume.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | WAPE Measurement (KA-D-003) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-041**. The policy defines WAPE thresholds.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Primary accuracy metric for model evaluation. |
| Learn From Demand (CA-D-010) | Analysed for accuracy trends. |
| Dashboards | Displayed for management oversight. |

#### 9. Explainability

Every published WAPE Measurement is traceable to the specific forecast and actual values used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003, SE-C-021 |
| Produces | KA-D-003 – WAPE Measurement |
| Used By | SE-D-008, CA-D-010, Dashboards |

---

### PI-D-004 — Mean Absolute Percentage Error (MAPE)

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-004 |
| Name | Mean Absolute Percentage Error |
| Measure Category | Enterprise Accuracy Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | What is the average percentage error per forecasted series, treating all series equally regardless of volume? |
| Business Objectives Served | BO-D-002, BO-D-004 |

#### 2. Enterprise Meaning

MAPE represents the enterprise's understanding of forecast error at the individual series level. Each series contributes equally, regardless of its volume. This makes MAPE sensitive to errors on low-volume items — which may be critical for customer service even if their volume is small. MAPE is equality-weighted across series. It reflects the typical error per item, regardless of volume. Use MAPE to evaluate forecast quality at the individual series level, especially when low-volume items are critical to customer outcomes. Complemented by PI-D-003 (WAPE) for volume-weighted enterprise error assessment.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Historical (actuals vs. published forecast) |
| Measure Scope | SKU, Product Family, Location |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Forecast Values | Forecast Publication (SE-D-003). | Resolved – SE-D-003. |
| Actual Demand Values | Enterprise Picture (SE-C-021) or Demand History. | Resolved – SE-C-021. |

#### 5. Calculation Model

**Formula:**
```
MAPE = (1 ÷ n) × Σ (|Forecast − Actual| ÷ Actual) × 100
```

**Calculation Assumptions:** Series with zero actual demand are excluded or handled per PO-D-041.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | MAPE Measurement (KA-D-004) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-041**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Consumed alongside WAPE for balanced accuracy assessment. |
| Learn From Demand (CA-D-010) | Analysed for series-level accuracy patterns. |

#### 9. Explainability

Every published MAPE Measurement is traceable to the specific series-level forecast and actual values used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003, SE-C-021 |
| Produces | KA-D-004 – MAPE Measurement |
| Used By | SE-D-008, CA-D-010 |

---

### PI-D-005 — Forecast Bias

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-005 |
| Name | Forecast Bias |
| Measure Category | Enterprise Accuracy Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | Is the enterprise systematically over-forecasting or under-forecasting demand? |
| Business Objectives Served | BO-D-002, BO-D-004 |

#### 2. Enterprise Meaning

Forecast Bias represents the enterprise's understanding of the directionality of its forecast errors. A positive bias means the enterprise is systematically over-forecasting (which leads to excess inventory). A negative bias means it is systematically under-forecasting (which leads to stockouts). Unlike WAPE and MAPE, which measure magnitude, Bias measures direction. A forecast can have low WAPE but high Bias if errors are consistent in one direction.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Historical (actuals vs. published forecast) |
| Measure Scope | Planning Scope, Product, Product Family, Location, Business Unit |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Forecast Values | Forecast Publication (SE-D-003). | Resolved – SE-D-003. |
| Actual Demand Values | Enterprise Picture (SE-C-021) or Demand History. | Resolved – SE-C-021. |

#### 5. Calculation Model

**Formula:**
```
Forecast Bias = (Σ (Forecast − Actual) ÷ Σ Actual) × 100
```

**Calculation Assumptions:** A positive value indicates over-forecasting; a negative value indicates under-forecasting.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Forecast Bias Measurement (KA-D-005) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-041**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Consumed for bias detection and model evaluation. |
| Detect Demand Exceptions (CA-D-008) | Consumed for bias-related exception detection. |
| Learn From Demand (CA-D-010) | Analysed for systematic bias patterns. |

#### 9. Explainability

Every published Forecast Bias Measurement is traceable to the specific forecast and actual values used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003, SE-C-021 |
| Produces | KA-D-005 – Forecast Bias Measurement |
| Used By | SE-D-008, CA-D-008, CA-D-010 |

---

### PI-D-006 — Forecast Value Added (FVA)

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-006 |
| Name | Forecast Value Added |
| Measure Category | Enterprise Value Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | How much does each step in the forecasting process improve or degrade forecast accuracy compared to a naive baseline? |
| Business Objectives Served | BO-D-002 |

#### 2. Enterprise Meaning

FVA represents the enterprise's understanding of whether its forecasting process is adding value. Each step — statistical model, planner override, management adjustment — is evaluated by comparing the accuracy with and without that step. A positive FVA means the step improved accuracy. A negative FVA means the step made the forecast worse. This measure directly identifies where human intervention is helping or hurting.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Historical (step-wise accuracy comparison) |
| Measure Scope | Planning Scope, Product Family, Process Step |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Naive Forecast | Simple baseline (e.g., moving average). | Resolved – computed internally. |
| Intermediate Forecast Values | Forecast Publication (SE-D-003) with step traceability. | Resolved – SE-D-003. |
| Actual Demand Values | Enterprise Picture (SE-C-021). | Resolved – SE-C-021. |

#### 5. Calculation Model

**Formula:**
```
FVA for step X = WAPE(without step X) − WAPE(with step X)
```

**Calculation Assumptions:** A positive value indicates the step added value. The naive baseline is defined by PO-D-041.

**Calculation Precision:** One decimal place.
**Unit:** Percentage points.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | FVA Measurement (KA-D-006) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-041**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Consumed for process value analysis. |
| Learn From Demand (CA-D-010) | Analysed for planner override effectiveness. |
| Demand Planners | Feedback on override impact. |

#### 9. Explainability

Every published FVA Measurement is traceable to the specific step-wise accuracy comparisons used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003, SE-C-021 |
| Produces | KA-D-006 – FVA Measurement |
| Used By | SE-D-008, CA-D-010, Dashboards |

---

### PI-D-007 — Forecast Stability

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-007 |
| Name | Forecast Stability |
| Measure Category | Enterprise Stability Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 – Forecast Measurement Policy |
| Enterprise Question | How much does the forecast change between planning cycles for the same future period? |
| Business Objectives Served | BO-D-002 |

#### 2. Enterprise Meaning

Forecast Stability represents the enterprise's understanding of how consistent its demand projections are between cycles. A highly unstable forecast — where the projection for the same future period changes dramatically from one week to the next — creates planning churn. Downstream capabilities cannot plan effectively if the demand signal keeps shifting. Stability complements accuracy: a forecast can be accurate on average but so unstable that it is unusable for planning.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Cycle-over-cycle comparison of forecasts for the same future period |
| Measure Scope | Planning Scope, Product, Product Family |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Current Forecast | Current Forecast Publication (SE-D-003). | Resolved – SE-D-003. |
| Prior Forecast for Same Period | Previous Forecast Publication (SE-D-003, Superseded version). | Resolved – SE-D-003. |

#### 5. Calculation Model

**Formula:**
```
Forecast Stability = 100 − (|Forecast_current − Forecast_prior| ÷ Forecast_prior) × 100
```
(aggregated across all series for the same target period)

**Calculation Assumptions:** Only periods that appear in both the current and prior forecast are compared.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Forecast Stability Measurement (KA-D-007) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-041**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Quality Assessment (SE-D-008) | Consumed for stability evaluation. |
| Supply Intelligence | Consumed for planning stability context. |
| Learn From Demand (CA-D-010) | Analysed for stability patterns. |

#### 9. Explainability

Every published Forecast Stability Measurement is traceable to the specific forecast versions compared.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Consumes | SE-D-003 (current and prior versions) |
| Produces | KA-D-007 – Forecast Stability Measurement |
| Used By | SE-D-008, Supply Intelligence, CA-D-010 |

---

### PI-D-102 — Demand Signal Quality Index

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-102 |
| Name | Demand Signal Quality Index |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 – Demand Data Acceptance Policy |
| Enterprise Question | What is the overall quality of the demand signals being received by the enterprise? |
| Business Objectives Served | BO-D-001, BO-D-003 |

#### 2. Enterprise Meaning

The Demand Signal Quality Index represents the enterprise's understanding of how trustworthy its incoming demand observations are. It aggregates signal timeliness, completeness, source reliability, and consistency into a single index. A low index indicates that the enterprise's demand understanding is built on unreliable data, regardless of forecast sophistication.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Signal quality over the evaluation period |
| Measure Scope | Source System, Planning Scope, Enterprise |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Signal timeliness data | Demand Observations (SE-D-001). | Resolved – SE-D-001. |
| Source reliability history | Historical acceptance/rejection rates per source. | Resolved – derived from SE-D-001. |

#### 5. Calculation Model

**Formula:**
```
Demand Signal Quality Index = Weighted composite of timeliness, completeness, source reliability, and consistency.
```
The precise weights and methodology are defined in PO-D-001.

**Calculation Precision:** One decimal place.
**Unit:** Index (0–100).

**Composite Methodology**

| Element | Description |
|---------|-------------|
| Normalisation | How each input factor (timeliness, completeness, source reliability, consistency) is scaled to a common range. |
| Aggregation | The method used to combine normalised factors (e.g., weighted sum). |
| Weighting | The weight assigned to each factor, reflecting its relative importance to overall signal quality. |
| Missing Value Handling | The rule applied when a factor cannot be computed (e.g., exclude, impute, flag). |
| Confidence | How the confidence of the index itself is derived from the quality of its inputs. |

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Demand Signal Quality Index Measurement (KA-D-008) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (daily, weekly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-001**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Demand Understanding (SE-D-002) | Consumed for interpretation confidence. |
| Detect Demand Exceptions (CA-D-008) | Consumed for data quality exception detection. |
| Dashboards | Displayed for operational monitoring. |

#### 9. Explainability

Every published Demand Signal Quality Index Measurement is traceable to the specific signal quality data used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 |
| Consumes | SE-D-001 |
| Produces | KA-D-008 – Demand Signal Quality Index Measurement |
| Used By | SE-D-002, CA-D-008, Dashboards |

---

### PI-D-103 — Forecast Confidence Index

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-103 |
| Name | Forecast Confidence Index |
| Measure Category | Enterprise Accuracy Measure |
| Measure Nature | Derived Measure (computed with each forecast publication) |
| Owning Capability | Forecast Demand (CA-D-002) |
| Governed By | PO-D-020 – Forecast Publication Governance |
| Enterprise Question | How reliable is the current forecast publication as a whole, given the quality of inputs, model performance, and data completeness? |
| Business Objectives Served | BO-D-003, BO-D-005 |

#### 2. Enterprise Meaning

The Forecast Confidence Index represents the enterprise's overall assessment of how much trust can be placed in a specific Forecast Publication. It is not a measure of historical accuracy; it is a forward-looking assessment of the current forecast's reliability based on the quality of its inputs, the performance of the champion model, and the completeness of the data used. It is published with every Forecast Publication and provides the evidence that supports the decision regarding automatic publication, as governed by PO-D-020.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Derived Measure |
| Measure Behavior | Per-publication |
| Measure Context | Computed at forecast generation time |
| Measure Scope | Forecast Publication |
| Validity | Valid for the lifespan of the Forecast Publication. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Champion model confidence | Forecast Publication (SE-D-003) metadata. | Resolved – SE-D-003. |
| Data completeness | Percentage of covered series with valid forecasts. | Resolved – SE-D-003. |
| Demand Signal Quality Index | PI-D-102. | Resolved – KA-D-008. |

#### 5. Calculation Model

**Formula:**
```
Forecast Confidence Index = Weighted composite of model confidence, data completeness, and signal quality.
```
The precise weights are defined in PO-D-020.

**Calculation Precision:** One decimal place.
**Unit:** Index (0–100).


**Composite Methodology**

| Element | Description |
|---------|-------------|
| Normalisation | How model confidence, data completeness, and signal quality index are scaled to a common range. |
| Aggregation | The method used to combine normalised factors (e.g., weighted sum). |
| Weighting | The weight assigned to each input factor. |
| Missing Value Handling | The rule applied when an input (e.g., signal quality index) is unavailable. |
| Confidence | How the confidence of the index itself is derived. |

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Forecast Confidence Index Measurement (KA-D-009) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | With each Forecast Publication |
| Publication Pattern | Per-publication |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-020**, which defines the auto-publication threshold.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Publication Governance (DE-D-022) | Determines whether the forecast can be auto-published. |
| Supply Intelligence | Consumed for planning confidence context. |
| Dashboards | Displayed for planner awareness. |

#### 9. Explainability

Every published Forecast Confidence Index Measurement is traceable to the model confidence, data completeness, and signal quality values used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Forecast Demand (CA-D-002) |
| Governed By | PO-D-020 |
| Consumes | SE-D-003, KA-D-008 |
| Produces | KA-D-009 – Forecast Confidence Index Measurement |
| Used By | DE-D-022, Supply Intelligence, Dashboards |

---

### PI-D-107 — Explainability Score

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-107 |
| Name | Explainability Score |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Derived Measure |
| Owning Capability | Explain Demand (CA-D-009) |
| Governed By | PO-D-047 – Explanation Governance |
| Enterprise Question | To what degree are the enterprise's demand conclusions explainable, traceable, and auditable? |
| Business Objectives Served | BO-D-006 |

#### 2. Enterprise Meaning

The Explainability Score represents the enterprise's assessment of how completely and traceably its demand intelligence outputs can be explained. It measures the proportion of demand conclusions — forecasts, classifications, exceptions, priorities — that carry a complete, deterministic explanation with full evidence traceability. A low score indicates governance risk: the enterprise is making planning decisions it cannot fully explain.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Derived Measure |
| Measure Behavior | Periodic |
| Measure Context | Across all explainable demand artifacts |
| Measure Scope | Enterprise, Capability |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Explanation records | Demand Explanations (SE-D-010). | Resolved – SE-D-010. |
| Explained artifact catalogue | All demand artifacts capable of being explained. | Resolved – domain metadata. |

#### 5. Calculation Model

**Formula:**
```
Explainability Score = (Number of Artifacts with Complete Explanations ÷ Total Number of Explainable Artifacts) × 100
```

**Calculation Assumptions:** Completeness is defined by PO-D-047. An explanation is complete if its Structured Reasoning Graph contains all required nodes and edges per the applicable template.

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Explainability Score Measurement (KA-D-010) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (monthly, quarterly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-047**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Learn From Demand (CA-D-010) | Analysed for explanation quality trends. |
| Governance | Audit and compliance oversight. |

#### 9. Explainability

Every published Explainability Score Measurement is traceable to the specific explanation completeness assessments used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Explain Demand (CA-D-009) |
| Governed By | PO-D-047 |
| Consumes | SE-D-010 |
| Produces | KA-D-010 – Explainability Score Measurement |
| Used By | CA-D-010, Governance |

---

### PI-D-202 — Forecast Generation Time

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-202 |
| Name | Forecast Generation Time |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Forecast Demand (CA-D-002) |
| Governed By | PO-D-024 – Forecast Cycle Governance |
| Enterprise Question | How long does it take the enterprise to generate a complete, publication-ready forecast? |
| Business Objectives Served | BO-D-005 |

#### 2. Enterprise Meaning

Forecast Generation Time represents the enterprise's understanding of the operational efficiency of its forecasting process. It measures the elapsed time from forecast cycle initiation to publication readiness. This is a process metric, not a quality metric — it measures speed, not accuracy.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Per-cycle |
| Measure Context | Cycle initiation to publication readiness |
| Measure Scope | Forecast Cycle |
| Validity | Valid for the forecast cycle. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Cycle Initiation Timestamp | Forecast Publication (SE-D-003) – recorded as metadata when the cycle is initiated. | Resolved – SE-D-003. |
| Publication Readiness Timestamp | Forecast Publication (SE-D-003) – Transaction Time of the Draft version when generation completes. | Resolved – SE-D-003. |

#### 5. Calculation Model

**Formula:**
```
Forecast Generation Time = Publication Readiness Timestamp − Cycle Initiation Timestamp
```

**Calculation Precision:** Integer.
**Unit:** Minutes.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Forecast Generation Time Measurement (KA-D-011) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Per forecast cycle |
| Publication Pattern | Per-cycle |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-024**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Forecast Demand (CA-D-002) | Process monitoring and optimisation. |
| Dashboards | Operational monitoring. |

#### 9. Explainability

Every published Forecast Generation Time Measurement is traceable to the specific cycle timestamps used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Forecast Demand (CA-D-002) |
| Governed By | PO-D-024 |
| Consumes | SE-D-003, Workflow metadata |
| Produces | KA-D-011 – Forecast Generation Time Measurement |
| Used By | CA-D-002, Dashboards |

---

### PI-D-205 — Data Completeness

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-205 |
| Name | Data Completeness |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 – Demand Data Acceptance Policy |
| Enterprise Question | What proportion of expected demand observations are actually being received and accepted? |
| Business Objectives Served | BO-D-001 |

#### 2. Enterprise Meaning

Data Completeness represents the enterprise's understanding of whether it is receiving all the demand data it expects. Missing data creates blind spots in the Demand Understanding and degrades forecast quality. This measure compares the volume of accepted observations against the expected volume, by source, by planning scope, and over time.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Received vs. expected observations |
| Measure Scope | Source System, Planning Scope, Enterprise |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Accepted observation count | Demand Observations (SE-D-001). | Resolved – SE-D-001. |
| Expected observation count | Defined by data feed contracts or historical patterns. | Resolved – external metadata. |

#### 5. Calculation Model

**Formula:**
```
Data Completeness = (Number of Accepted Observations ÷ Number of Expected Observations) × 100
```

**Calculation Precision:** One decimal place.
**Unit:** Percentage.

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Data Completeness Measurement (KA-D-012) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (daily, weekly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-001**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Demand Understanding (SE-D-002) | Consumed for interpretation confidence. |
| Detect Demand Exceptions (CA-D-008) | Consumed for data gap exception detection. |
| Dashboards | Displayed for operational monitoring. |

#### 9. Explainability

Every published Data Completeness Measurement is traceable to the specific observation counts used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 |
| Consumes | SE-D-001 |
| Produces | KA-D-012 – Data Completeness Measurement |
| Used By | SE-D-002, CA-D-008, Dashboards |

---

### PI-D-206 — Data Quality Score

#### 1. Enterprise Measure Definition

| Attribute | Value |
|-----------|-------|
| Identifier | PI-D-206 |
| Name | Data Quality Score |
| Measure Category | Enterprise Platform Governance Measure |
| Measure Nature | Historical Derived Measure |
| Owning Capability | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 – Demand Data Acceptance Policy |
| Enterprise Question | What is the overall quality of accepted demand observations, considering accuracy, consistency, and timeliness? |
| Business Objectives Served | BO-D-001 |

#### 2. Enterprise Meaning

The Data Quality Score represents the enterprise's understanding of the fitness of its demand data for planning purposes. It goes beyond completeness to assess the accuracy, consistency, and timeliness of accepted observations. A high score means the Demand Understanding and Forecast Publication are built on trustworthy data.

#### 3. Measurement Model

| Attribute | Value |
|-----------|-------|
| Measure Type | Historical Derived |
| Measure Behavior | Periodic |
| Measure Context | Quality of accepted observations |
| Measure Scope | Source System, Planning Scope, Enterprise |
| Validity | Valid for the evaluation period. |

#### 4. Measurement Inputs

| Variable | Enterprise Source | Resolution Status |
|----------|-------------------|-------------------|
| Observation quality metadata | Demand Observations (SE-D-001) – acceptance confidence, warning codes. | Resolved – SE-D-001. |
| Source reliability history | Historical acceptance/rejection rates per source. | Resolved – derived from SE-D-001. |

#### 5. Calculation Model

**Formula:**
```
Data Quality Score = Weighted composite of acceptance rate, warning frequency, timeliness, and source reliability.
```
The precise weights are defined in PO-D-001.

**Calculation Precision:** One decimal place.
**Unit:** Index (0–100).

**Composite Methodology**

| Element | Description |
|---------|-------------|
| Normalisation | How acceptance rate, warning frequency, timeliness, and source reliability are scaled to a common range. |
| Aggregation | The method used to combine normalised factors (e.g., weighted sum). |
| Weighting | The weight assigned to each factor. |
| Missing Value Handling | The rule applied when a factor is unavailable for a source. |
| Confidence | How the confidence of the score itself is derived. |

#### 6. Publication Model

| Attribute | Value |
|-----------|-------|
| Produces | Data Quality Score Measurement (KA-D-013) |
| Contents | Measure Identifier, Value, Unit, Calculation Context, Validity Period, Evidence References, Calculation Timestamp, Owning Capability, Version, Confidence, Expiry Timestamp |
| Publication Trigger | Periodic (daily, weekly) |
| Publication Pattern | Periodic |

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| Version | Integer | Yes | Monotonically increasing version number for this measurement. |
| Confidence | Decimal | Yes | Confidence in this measurement. |
| Expiry Timestamp | Timestamp (SE-C-022) | Yes | When this measurement expires and requires re-evaluation. |

#### 7. Interpretation Governance

Interpretation is governed by **PO-D-001**.

#### 8. Consumers

| Consumer | How the Measure Is Used |
|----------|-------------------------|
| Demand Understanding (SE-D-002) | Consumed for interpretation confidence. |
| Detect Demand Exceptions (CA-D-008) | Consumed for data quality exception detection. |

#### 9. Explainability

Every published Data Quality Score Measurement is traceable to the specific quality metadata used.

#### 10. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Understand Demand (CA-D-001) |
| Governed By | PO-D-001 |
| Consumes | SE-D-001 |
| Produces | KA-D-013 – Data Quality Score Measurement |
| Used By | SE-D-002, CA-D-008 |

---

# Chapter 4 – Demand Domain Semantic Model

## 4.1 Demand Domain Semantic Principles

### 4.1.1 Enterprise Temporal Semantics

| Temporal Dimension | Business Meaning |
|--------------------|------------------|
| Business Time | When an event occurred in enterprise reality (e.g., the timestamp of a sales order, shipment, or POS transaction). |
| Observation Time | When the enterprise received or recorded the demand observation. |
| Transaction Time | When an aggregate was created or revised within Demand Intelligence. |
| Publication Time | When an aggregate became authoritative and visible to consumers. |
| Effective Time | The planning period for which demand information is valid. |

### 4.1.2 Cross-Cutting Semantic Concepts

| Term | Definition | Governance Reference |
|------|------------|----------------------|
| Confidence | A multi-dimensional assessment of the enterprise’s trust in a demand assertion. Confidence is governed by dimensions including Statistical Confidence, Evidence Strength, Data Completeness, Data Consistency, and Source Reliability. A composite confidence index may be published as a summary measure only when all underlying dimension values are preserved and traceable. | PO-D-001, PO-D-020, PO-D-041 |
| Materiality | A measure of whether a change in demand understanding is significant enough to warrant publication of a new Demand Understanding. Materiality is assessed per interpretation dimension against thresholds defined in the relevant Publication Policy. | PO-D-011 |
| Forecastability | An assessment of whether a demand series possesses sufficient historical data and statistical regularity to support meaningful statistical forecasting. Series that are not forecastable are flagged and assigned a fallback method. | PO-D-019 |
| Recurrence | The observed property that a demand pattern appears across multiple distinct periods or events. Single-occurrence phenomena do not constitute enterprise learnings. | PO-D-048 |
| Stability | A measure of how much a demand projection has changed relative to a previous projection for the same target period. High instability may indicate planning churn or environmental volatility. | PO-D-041 |

### 4.1.3 Semantic Object Families

Semantic Object Families are Demand-domain grouping labels. Where an ARS pattern applies, the object contract shall declare the ARS pattern. This chapter does not create new ARS patterns.

### 4.1.4 Object Classification

All domain-owned semantic objects follow the ARS meta-model classification. The pattern for each object is declared in its specification.

### 4.1.5 Semantic vs. Behavioral Boundary

Chapter 4 defines *what* enterprise objects are – their identity, structure, lifecycle states, and invariants. It does not define *how* behaviors execute. Aggregate Behaviors are documented immediately beneath their owning Aggregate Root. Functional Specifications (Chapter 9) orchestrate behaviors; they do not redefine them.

---

## 4.2 Demand Domain Dependency Declaration

Every Enterprise Semantic Object consumed by the Demand domain is listed below, together with the attributes that Demand capabilities require.

| Enterprise Object | Required Attributes |
|-------------------|---------------------|
| SE-C-001 Item | Identifier, Name, Item Type, Item Roles, Unit of Measure, Lifecycle State |
| SE-C-002 Location | Identifier, Name, Location Type, Time Zone, Lifecycle State |
| SE-C-003 Customer | Identifier, Name, Customer Class, Lifecycle State |
| SE-C-010 Planning Scope | Identifier, Scope Name, Boundary Rules, Boundary Statement (optional), Lifecycle State |
| SE-C-011 Scenario | Identifier, Scenario Name, Scenario Adjustments, Lifecycle State |
| SE-C-013 Demand | Identifier, Item, Quantity, Location, Need Window, Demand Origin, Customer, Lifecycle State |
| SE-C-021 Enterprise Picture | Planning Scope Identifier, Version Number, Lifecycle State, Publication Time, Demand References |
| SE-C-022 Timestamp | (entire value) |
| SE-C-023 Quantity | (entire value) |
| SE-C-024 Duration | (entire value) |
| SE-C-027 Planning Horizon | Start, End |
| SE-C-029 Need Window | Earliest Acceptable, Preferred, Latest Acceptable |
| SE-C-033 Calendar | Calendar Identifier, Calendar Name, Time Zone, Calendar Definition, Version Number, Adoption State |
| SE-C-034 Planning Period | Planning Period Identifier, Adoption State |
| SE-C-035 Performance Indicator Catalog | Catalog Identifier, Version Number, Lifecycle State, Performance Indicator Definitions |
| SE-C-036 Performance Indicator | Performance Indicator Identifier, Name, Measure Category, Measure Nature, Enterprise Question, Business Objectives Served, Enterprise Meaning, Formula, Semantic Dependencies |
| SE-C-037 Enterprise Governed Vocabulary | Catalog Identifier, Version Number, Lifecycle State, Vocabulary Entries |

> No semantic object may be used later in this specification unless declared here.

> All governed identifiers used by Demand Intelligence shall resolve to entries in SE-C-037 Enterprise Governed Vocabulary or to a governed vocabulary explicitly delegated by the Enterprise Semantic Model.

| External Notification | Publisher (Expected) | Required Fields | Purpose |
|-----------------------|----------------------|-----------------|---------|
| BN-C-001 Enterprise Picture Published | Enterprise Picture Management (CA-C-019) | Planning Scope Identifier, Published Version Number, Publication Time | Triggers the revision of the Demand Understanding (FS-D-004) whenever the authoritative demand snapshot changes. |

BN-C-001 is a consumer dependency of Demand Intelligence. The authoritative Business Notification contract, including referenced Enterprise Events, delivery guarantees, ordering guarantees, and timeliness guarantees, is owned by the Core capability responsible for Enterprise Picture publication.

Demand Intelligence requires the following consumer guarantees for BN-C-001:

| Guarantee | Required Value |
|-----------|----------------|
| Delivery | At-least-once |
| Ordering | Per Planning Scope |
| Timeliness | Near-real-time |
| Idempotent Consumption | FS-D-004 shall be idempotent for repeated notifications of the same Published Version Number |

Material Change Summary is not a mandatory dependency of Demand Intelligence. If Core provides it, Demand Intelligence may use it as an optimization, but the Demand Understanding revision shall remain computable from the Published Enterprise Picture version itself.

---

## 4.3 Demand-Owned Semantic Objects

### 4.3.1 Aggregate Roots

#### SE-D-001 – Demand Observation

**Business Intent:** Preserve exactly what the enterprise received from a demand source, with enough provenance and evaluation context to support trustworthy downstream demand understanding.

**Enterprise Meaning:** An enterprise record of demand data received from any source – sales order, shipment confirmation, point-of-sale transaction, return, correction, or forecast-derived signal – captured exactly as received. The received payload and provenance are immutable; evaluation metadata is appended through governed lifecycle transitions.

**Applied Semantic Pattern:** Observation

**Identity:** Demand Observation Identifier, globally unique, assigned at creation. Immutable.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Understand Demand (CA-D-001) |
| Authoritative Representation | The immutable enterprise record of a received demand observation, exactly as received. |
| Business Responsibility | Record the raw demand data and its provenance; evaluate it exactly once to determine trustworthiness. |
| Authority Scope | Per record. The record is authoritative for the fact that the enterprise received this specific data. |
| Intended Consumers | Understand Demand (internal evaluation). Accepted observations contribute to the Enterprise Picture and, through it, to the Demand Understanding. |
| Non-Intended Consumers | Any downstream planning capability must consume the Demand Understanding or Forecast Publication, not raw demand observations. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | The record is an immutable, exact copy of what the enterprise received. Evaluation is performed exactly once. |
| Required Interpretation | Consumers must not use a Received record for planning; only Accepted records are eligible for incorporation. |
| Known Limitations | The record reflects data as received; it may contain errors. Evaluation does not guarantee absolute accuracy. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Received | Record established, not yet evaluated. | AB-D-001 |
| Accepted | Passed evaluation; eligible for incorporation. | DE-D-001 (Accept) |
| Quarantined | Failed evaluation; permanently held. No further processing within this capability. | DE-D-001 (Quarantine) |
| Rejected | Permanently excluded. | DE-D-001 (Reject) |

- Terminal States: Accepted, Quarantined, Rejected.
- History Preservation: All records retained permanently.
- Versioning Rules: Not versioned; the record is immutable once created and evaluated.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Demand Observation Identifier | Yes | Yes | Globally unique. |
| Mandatory | Item (SE-C-001) | Yes | Yes | The item the demand relates to. |
| Mandatory | Location (SE-C-002) | Yes | Yes | The location. |
| Mandatory | Quantity (SE-C-023) | Yes | Yes | The observed quantity. |
| Mandatory | Observation Type (SalesOrder, Shipment, POS, Return, Correction, Signal) | Yes | Yes | The category of demand observation. |
| Mandatory | Business Time | Yes | Yes | When the event occurred in reality. |
| Mandatory | Observation Time | Yes | Yes | When the enterprise received the data. |
| Mandatory | Source System Provenance | Yes | Yes | Originating system. |
| Optional | Customer (SE-C-003) | No | Yes | If the observation is customer-specific. |
| Optional | Promotion, Campaign, Contract Reference | No | Yes | Business context. |
| Derived | Evaluation State (Lifecycle State) | No | No | |
| Derived | Decision Confidence, Decision Rationale | No | No | |
| Derived | Data Quality Flag | No | No | |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| records demand for | Item (SE-C-001) | Many-to-One |
| is observed at | Location (SE-C-002) | Many-to-One |
| may involve | Customer (SE-C-003) | Many-to-One (optional) |

**Dependencies:** SE-C-001, SE-C-002, SE-C-003, SE-C-023, SE-C-022.

**Invariants:** Evaluated exactly once from Received. Only Accepted observations contribute to the Enterprise Picture.

**Traceability:** Business Owner: CA-D-001. Produced By: AB-D-001, AB-D-002. Upward: CN-003, CN-004.

---

#### AB-D-001 – Receive Demand Observation

**Purpose:** Establish a Demand Observation from a received demand signal.

**Business Intent:** Create an immutable enterprise record of exactly what was received, before any evaluation.

**Trigger:** Demand signal received from a source system or internal notification.

**Preconditions:** Record contains sufficient information for unique identity. Referenced Item and Location exist.

**Business Behavior:** Create a new Demand Observation with a globally unique identifier. Populate all mandatory attributes. Record enters Received state.

**State Transitions:** None → Received.

**Business Transaction:** Protects Demand Observation aggregate.

**Decisions Invoked:** None.

**Events Published:** EV-D-001 (Demand Observation Received).

**Idempotency:** Re-execution with the same identity produces no duplicate.

**Traceability:** Owned by SE-D-001. Invoked by FS-D-001.

---

#### AB-D-002 – Evaluate Demand Observation

**Purpose:** Evaluate a Demand Observation against acceptance criteria.

**Business Intent:** Determine whether a demand record is trustworthy enough to contribute to the Enterprise Picture.

**Trigger:** Completion of FS-D-001.

**Preconditions:** Record is in Received state. Not previously evaluated.

**Business Behavior:** Execute DE-D-001 (Accept Demand Observation). Based on outcome, transition to Accepted, Quarantined, or Rejected.

**State Transitions:** Received → Accepted / Quarantined / Rejected.

**Business Transaction:** Protects Demand Observation aggregate.

**Decisions Invoked:** DE-D-001.

**Events Published:** EV-D-002 (Demand Observation Evaluated).

**Notifications Produced:**
- BN-D-006 (Demand Observation Accepted) – published when the decision outcome is Accept.

**Idempotency:** Re-execution on already-evaluated record terminates immediately.

**Traceability:** Owned by SE-D-001. Invoked by FS-D-002.

---

#### SE-D-002 – Demand Understanding

**Business Intent:** Provide exactly one authoritative demand interpretation for a Planning Scope at any point in time, while preserving the full version history needed for audit, explainability, and downstream planning.

**Enterprise Meaning:** The Demand Understanding is the aggregate root that maintains the authoritative version series of demand interpretation for a Planning Scope. Each version represents the enterprise’s current interpretation of what demand exists, what demand patterns are present, and how intrinsically volatile the current demand picture is, based on the demand facts in the latest Enterprise Picture. It does not duplicate demand data; it interprets it.

**Applied Semantic Pattern:** Published Knowledge

**Identity:** Planning Scope (SE-C-010). Each version receives a monotonically increasing Version Number.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Understand Demand (CA-D-001) |
| Authoritative Representation | The governed, versioned interpretation of current demand patterns, health, and intrinsic volatility for a Planning Scope. |
| Business Responsibility | Interpret the demand facts in the Enterprise Picture into a coherent Demand Understanding; publish materially changed versions. |
| Authority Scope | Per Planning Scope. Exactly one Published version per Planning Scope is authoritative. |
| Intended Consumers | Forecast Demand, Sense Demand, Segment Demand, Classify Demand, Prioritize Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Exactly one Published version exists per Planning Scope. A Published version is immutable. All interpretations are traceable to the Enterprise Picture version used. |
| Required Interpretation | Consumers must treat the Published Understanding as the exclusive interpretive baseline for current demand. It does not contain raw demand data. |
| Known Limitations | The Understanding is a point-in-time interpretation. It does not include demand forecasts or projections. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Draft | New version created; interpretations computed from latest Enterprise Picture. | AB-D-003 |
| Published | Version released as authoritative. Previous Published → Superseded. BN-D-001 published. | AB-D-004 |
| Superseded | Version replaced by a newer Published version. | AB-D-005 |

- Terminal States: Superseded.
- History Preservation: All versions retained permanently.

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Planning Scope Identifier | Reference (SE-C-010) | Yes | Aggregate identity. |
| Versions | List of Demand Understanding Version (SE-D-012) | Yes | At least one version. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| interprets demand for | Planning Scope (SE-C-010) | Many-to-One |
| derived from | Enterprise Picture (SE-C-021) | Many-to-One |

**Dependencies:** SE-C-010, SE-C-021.

**Invariants:** Exactly one Published version per Planning Scope. Published version is immutable. Every interpretation dimension references the specific Enterprise Picture version used.

**Traceability:** Business Owner: CA-D-001. Produced By: AB-D-003, AB-D-004. Upward: CN-003, CN-004.

---

#### SE-D-012 — Demand Understanding Version

**Business Intent:** Preserve one immutable version of the Demand Understanding interpretation for a Planning Scope.

**Identity:** Version Number uniquely identifies the Demand Understanding Version within its owning Demand Understanding aggregate.

**Owning Aggregate Root:** SE-D-002 Demand Understanding

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Version Number | Integer | Yes | Monotonically increasing within the Planning Scope. |
| Demand Continuity Interpretation | Governed Identifier Reference (SE-C-037) | Yes | Continuity status. |
| Demand Pattern Interpretation | Governed Identifier Reference (SE-C-037) | Yes | Pattern status. |
| Demand Health Interpretation | Governed Identifier Reference (SE-C-037) | Yes | Health status. |
| Demand Volatility Interpretation | Governed Identifier Reference (SE-C-037) | Yes | Volatility level. |
| Key Demand Drivers | List of Governed Identifier References (SE-C-037) | No | Identified demand drivers. |
| Primary Volatility Drivers | List of Governed Identifier References (SE-C-037) | No | Identified volatility drivers. |
| Data Quality Concerns | List of Governed Identifier References (SE-C-037) | No | Identified data quality concerns. |
| Evidence References | Enterprise Picture Version Reference | Yes | The Enterprise Picture version used. |
| Forecast Publication Reference | Forecast Publication Version Reference | No | The Forecast Publication version used as forward-looking context. |
| Transaction Time | Timestamp (SE-C-022) | Yes | When this version was created. |
| Publication Time | Timestamp (SE-C-022) | No | When this version was published. Absent for Draft versions. |
| Lifecycle State | Enum (Draft, Published, Superseded) | Yes | Current state of this version. |

**Lifecycle Specification Contract**

| State | Description |
| --- | --- |
| Draft | Being prepared; not authoritative. |
| Published | Authoritative; previous Published becomes Superseded. |
| Superseded | Replaced by a newer Published version. |

- Permitted Transitions: Draft → Published; Published → Superseded.
- Terminal State: Superseded.
- History Preservation: All versions are retained permanently.

**Invariants**
- Version Number is monotonically increasing within the owning Demand Understanding.
- Exactly one Demand Understanding Version is Published per Planning Scope at any time.
- A Published Demand Understanding Version is immutable.
- A Superseded Demand Understanding Version is immutable.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-002 Demand Understanding |
| Downward | Forecast Demand, Sense Demand, Segment Demand, Classify Demand, Prioritize Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence. |

---

#### AB-D-003 – Revise Demand Understanding

**Purpose:** Create a new Draft version of the Demand Understanding by interpreting the latest Enterprise Picture.

**Business Intent:** Keep the enterprise’s demand interpretation current.

**Trigger:** Enterprise Picture Published (notification from Core).

**Preconditions:** Published Enterprise Picture exists for the Planning Scope.

**Business Behavior:** Load the latest Enterprise Picture. Interpret demand patterns, health, and volatility. Compose a new Draft version.

**State Transitions:** None → Draft, or Published → Draft.

**Business Transaction:** Protects SE-D-002 aggregate.

**Decisions Invoked:** None.

**Events Published:** EV-D-003 (Demand Understanding Revised).

**Traceability:** Owned by SE-D-002. Invoked by FS-D-003.

---

#### AB-D-004 – Publish Demand Understanding

**Purpose:** Publish the Draft Demand Understanding, making it authoritative.

**Business Intent:** Transfer the latest demand interpretation to all consuming capabilities.

**Trigger:** Material change detected, or Periodic Refresh due.

**Preconditions:** Draft version exists. Materiality assessment performed.

**Business Behavior:** Execute DE-D-002 (Publish Demand Understanding). If Publish, transition to Published, supersede previous, publish BN-D-001.

**State Transitions:** Draft → Published. Previous → Superseded.

**Business Transaction:** Protects SE-D-002 aggregate.

**Decisions Invoked:** DE-D-002.

**Events Published:** EV-D-004 (Demand Understanding Published).

**Notifications Produced:** BN-D-001.

**Traceability:** Owned by SE-D-002. Invoked by FS-D-004.


#### SE-D-003 – Forecast Publication

**Business Intent:** Provide exactly one authoritative demand projection for a Planning Scope and horizon, so downstream capabilities work from the same governed forecast.

**Enterprise Meaning:** The Forecast Publication is the aggregate root that maintains the authoritative version series of demand projections for a Planning Scope and horizon. Each version contains forecast quantities, prediction intervals, confidence scores, assumptions, and model provenance. It is the enterprise’s authoritative statement of expected future demand.

**Applied Semantic Pattern:** Published Knowledge

**Identity:** Planning Scope + Forecast Horizon + Version. Each version receives a globally unique Forecast Publication Identifier and a monotonically increasing Version Number.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Forecast Demand (CA-D-002) |
| Authoritative Representation | The authoritative, versioned demand projection for a Planning Scope and horizon. |
| Business Responsibility | Produce and publish the enterprise’s authoritative demand forecast. |
| Authority Scope | Per Planning Scope and horizon. Exactly one Published version is authoritative at any moment. |
| Intended Consumers | Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Every Published forecast is immutable and permanently retained. All forecast lines carry model provenance. |
| Required Interpretation | Consumers must treat the Published forecast as the single authoritative demand projection. |
| Known Limitations | The forecast is a projection, not a guarantee. It reflects the assumptions and models in effect at generation time. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Draft | Forecast generated, assumptions recorded. | AB-D-007, AB-D-008 |
| Published | Version released as authoritative. | AB-D-009 |
| Superseded | Replaced by a newer Published version. | AB-D-009 |

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Planning Scope Identifier | Reference (SE-C-010) | Yes | Aggregate identity component. |
| Forecast Horizon | Reference (SE-C-027) | Yes | Aggregate identity component. |
| Versions | List of Forecast Publication Version (SE-D-013) | Yes | At least one version. |

**Dependencies:** SE-C-010, SE-C-027, SE-C-001, SE-C-002, SE-C-034, and the Demand Understanding (SE-D-002).

**Invariants:** Exactly one Published version per Planning Scope and horizon. Published version is immutable. Original system forecast preserved on override.


#### SE-D-013 — Forecast Publication Version

**Business Intent:** Preserve one immutable version of the Forecast Publication for a Planning Scope and Forecast Horizon.

**Identity:** Version Number uniquely identifies the Forecast Publication Version within its owning Forecast Publication aggregate.

**Owning Aggregate Root:** SE-D-003 Forecast Publication

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Version Number | Integer | Yes | Monotonically increasing within the Planning Scope and Forecast Horizon. |
| Forecast Lines | List of Forecast Line (SE-D-015) | Yes | At least one forecast line or unforecastable flag. |
| Forecast Assumptions | List of Forecast Assumption (SE-D-016) | No | Assumptions declared during forecasting. |
| Forecast Overrides | List of Forecast Override (SE-D-017) | No | Overrides applied during governance. |
| Forecast Confidence Index | Forecast Confidence Index Measurement (KA-D-009) | Yes | Overall reliability score for the publication. |
| Forecast Completeness Score | Decimal | Yes | Percentage of covered series with valid forecasts. |
| Champion Model Identifier | Governed Identifier Reference (SE-C-037) | Yes | The authorised forecasting strategy used. |
| Generation Context Identifier | ID | Yes | Unique identity of the generation context. |
| Cycle Initiation Timestamp | Timestamp (SE-C-022) | Yes | When the generation context was initiated. |
| Cycle Initiation Reason | Governed Identifier Reference (SE-C-037) | Yes | Scheduled, Critical Demand Change, or Planner Request. |
| Generation Status | Enum (Initialized, Generating, Generated, Overridden, Ready, Published, Superseded) | Yes | Current generation status. |
| Publication Time | Timestamp (SE-C-022) | No | When this version was published. Absent for Draft versions. |
| Lifecycle State | Enum (Draft, Published, Superseded) | Yes | Current state of this version. |

**Lifecycle Specification Contract**

| State | Description |
| --- | --- |
| Draft | Being prepared; not authoritative. |
| Published | Authoritative; previous Published becomes Superseded. |
| Superseded | Replaced by a newer Published version. |

- Permitted Transitions: Draft → Published; Published → Superseded.
- Terminal State: Superseded.
- History Preservation: All versions are retained permanently.

**Invariants**
- Version Number is monotonically increasing within the owning Forecast Publication.
- Exactly one Forecast Publication Version is Published per Planning Scope and Forecast Horizon at any time.
- A Published Forecast Publication Version is immutable.
- A Superseded Forecast Publication Version is immutable.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-003 Forecast Publication |
| Downward | Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality, Understand Demand. |

---

#### SE-D-015 — Forecast Line

**Business Intent:** Represent one forecast line within a Forecast Publication Version for a specific demand series.

**Identity:** Forecast Line Identifier is the immutable identity of the Forecast Line within its owning Forecast Publication Version.

**Owning Aggregate Root:** SE-D-003 Forecast Publication

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Forecast Line Identifier | ID (immutable) | Yes | Unique identity within the Forecast Publication Version. |
| Item | Reference (SE-C-001) | Yes | The item being forecast. |
| Location | Reference (SE-C-002) | Yes | The location being forecast. |
| Time Bucket | Planning Period Reference (SE-C-034) | Yes | The time bucket for this forecast line. |
| Forecast Mean | Quantity (SE-C-023) | Yes | The mean forecast quantity. |
| Prediction Interval Lower | Quantity (SE-C-023) | No | Lower bound of prediction interval. |
| Prediction Interval Upper | Quantity (SE-C-023) | No | Upper bound of prediction interval. |
| Prediction Interval Confidence | Governed Identifier Reference (SE-C-037) | No | Confidence level of prediction interval. |
| Confidence Score | Decimal | Yes | Confidence score for this forecast line. |
| Model Provenance | Governed Identifier Reference (SE-C-037) | Yes | The forecasting strategy that produced this line. |
| Override Indicator | Boolean | Yes | Whether this line has been overridden. |
| Unforecastable Flag | Boolean | Yes | Whether this series is unforecastable. |
| Unforecastable Reason | Governed Identifier Reference (SE-C-037) | No | Reason if unforecastable. |
| Fallback Method | Governed Identifier Reference (SE-C-037) | No | Fallback method applied if unforecastable. |

**Invariants**
- Forecast Line Identifier is immutable within the owning Forecast Publication Version.
- Item and Location must reference active objects.
- Forecast Mean must be non-negative.
- A Forecast Line cannot exist outside its owning Forecast Publication Version.

**Lifecycle Specification Contract**
Forecast Line has no independent lifecycle. Its existence and immutability are governed by the lifecycle of its owning Forecast Publication Version.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-003 Forecast Publication |
| Downward | Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality. |

---

#### SE-D-016 — Forecast Assumption

**Business Intent:** Represent one declared assumption influencing the forecast within a Forecast Publication Version.

**Identity:** Forecast Assumption Identifier is the immutable identity of the Forecast Assumption within its owning Forecast Publication Version.

**Owning Aggregate Root:** SE-D-003 Forecast Publication

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Forecast Assumption Identifier | ID (immutable) | Yes | Unique identity within the Forecast Publication Version. |
| Assumption Category | Governed Identifier Reference (SE-C-037) | Yes | Commercial, Promotional, Macroeconomic. |
| Assumption Statement | String | Yes | Business-language description of the assumption. |
| Scenario Adjustment Reference | Scenario Adjustment Reference (SE-C-039) | No | Reference to the Scenario Adjustment that informed this assumption. |
| Sign-off Status | Enum (Pending, Approved, Rejected) | Yes | Current sign-off status. |
| Sign-off Authority | String | No | Identity of the person who signed off. |
| Sign-off Timestamp | Timestamp (SE-C-022) | No | When sign-off occurred. |

**Invariants**
- Forecast Assumption Identifier is immutable within the owning Forecast Publication Version.
- A Forecast Assumption cannot exist outside its owning Forecast Publication Version.

**Lifecycle Specification Contract**
Forecast Assumption has no independent lifecycle. Its existence and immutability are governed by the lifecycle of its owning Forecast Publication Version.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-003 Forecast Publication |
| Downward | Supply Intelligence, Promise Intelligence, Scenario Intelligence. |

---

#### SE-D-017 — Forecast Override

**Business Intent:** Represent one planner override of a system forecast value within a Forecast Publication Version, preserving the original system forecast for audit and Forecast Value Added computation.

**Identity:** Forecast Override Identifier is the immutable identity of the Forecast Override within its owning Forecast Publication Version.

**Owning Aggregate Root:** SE-D-003 Forecast Publication

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Forecast Override Identifier | ID (immutable) | Yes | Unique identity within the Forecast Publication Version. |
| Forecast Line Reference | Forecast Line Reference (SE-D-015) | Yes | The forecast line being overridden. |
| Original System Forecast Value | Quantity (SE-C-023) | Yes | The original system forecast value. Immutable. |
| Override Value | Quantity (SE-C-023) | Yes | The planner's override value. |
| Planner Identity | String | Yes | Identity of the planner who submitted the override. |
| Justification | String | Yes | Non-empty business justification. |
| Override Timestamp | Timestamp (SE-C-022) | Yes | When the override was applied. |
| Decision Reference | DE-D-005 | Yes | The decision that evaluated this override. |

**Invariants**
- Forecast Override Identifier is immutable within the owning Forecast Publication Version.
- Original System Forecast Value is immutable.
- Justification must be non-empty.
- A Forecast Override cannot exist outside its owning Forecast Publication Version.

**Lifecycle Specification Contract**
Forecast Override has no independent lifecycle. Its existence and immutability are governed by the lifecycle of its owning Forecast Publication Version.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-003 Forecast Publication |
| Downward | Evaluate Demand Quality. |

---

#### AB-D-005 – Initiate Forecast Cycle

**Purpose:** Establish a new forecast cycle identity and initial workflow state for a Planning Scope and horizon.

**Business Intent:** Ensure that every forecast cycle is initiated with a unique enterprise identity, clear business justification, and explicit scope definition.

**Trigger:** Scheduled cadence reached, or BN-D-016 Critical Demand Behavior Requires Action received, or authorized Demand Planner request.

**Preconditions:** No active forecast cycle exists for the target Planning Scope and horizon. Forecast Configuration and calendar parameters are valid.

**Business Behavior:** Validate scope and authorization. Create a new forecast cycle record with a globally unique cycle identifier. Record initiation reason (Scheduled, Critical Demand Change, or Planner Request). Transition cycle state to Initialized.

**State Transitions:** None → Initialized.

**Business Transaction:** Protects forecast cycle workflow state.

**Decisions Invoked:** None.

**Events Published:** EV-D-010 (Forecast Cycle Established).

**Idempotency:** Re-execution with identical parameters generates a distinct cycle identifier.

**Traceability:** Owned by SE-D-003. Invoked by FS-D-005.

---

#### AB-D-006 – Select Champion Model

**Purpose:** Determine the authoritative champion forecasting model for a forecast cycle.

**Business Intent:** Apply governed model selection policy criteria to promote the highest performing forecasting model to champion status.

**Trigger:** Initiation of forecast cycle or model performance re-evaluation.

**Preconditions:** Historical performance metrics for challenger models exist over the governed evaluation window defined in PO-D-017.

**Business Behavior:** Evaluate challenger models against champion per PO-D-017. If a challenger demonstrates statistically significant WAPE improvement without violating bias, stability, or high-priority item thresholds, select it as champion; otherwise retain current champion.

**State Transitions:** Model Selection Evaluated.

**Business Transaction:** Protects Forecast Publication model provenance.

**Decisions Invoked:** DE-D-013 (Select Champion Model)

**Rules Enforced:** BR-D-401 (Authorised Forecasting Strategy)

**Policies Referenced:** PO-D-017 (Forecast Model Governance Policy)

**Events Published:** EV-D-014 (Champion Model Selected).

**Traceability:** Owned by SE-D-003. Invoked by FS-D-005 / FS-D-006.

---

#### AB-D-007 – Produce Forecast Projection

**Purpose:** Generate a complete set of statistical forecast lines and prediction intervals for a Draft Forecast Publication.

**Business Intent:** Synthesize the enterprise's forward-looking demand projection for all covered item-location series using the authorized champion model and governed fallback rules.

**Trigger:** Completion of model selection in a forecast cycle.

**Preconditions:** Draft Forecast Publication initialized. Champion model selected. SE-C-021 Enterprise Picture versions available for training window.

**Business Behavior:** Execute DE-D-003 (Generate Forecast for Series) and BA-D-002 (Produce Forecast Projection). Generate mean forecast, prediction intervals, and confidence scores for forecastable series. Apply PO-D-019 fallback methods for unforecastable series. Compute composite Forecast Confidence Index. Populate Draft publication.

> The outcome of DE-D-003 (Forecastable / Unforecastable per series) is provided as an input to BA-D-002, which applies the corresponding forecasting strategy without re-evaluating data sufficiency.

**State Transitions:** Draft (Unpopulated) → Draft (Populated).

**Business Transaction:** Protects SE-D-003 aggregate root. Atomic population of forecast lines.

**Decisions Invoked:** DE-D-003 (Generate Forecast for Series).

**Events Published:** EV-D-011 (Forecast Projection Produced).

**Traceability:** Owned by SE-D-003. Invoked by FS-D-006.

---

#### AB-D-008 – Govern Forecast Projection

**Purpose:** Process and record planner overrides on system forecast values within a Draft Forecast Publication while permanently preserving original statistical values.

**Business Intent:** Allow human domain experts to incorporate unmodeled business knowledge into the forecast under strict deviation and justification governance.

**Trigger:** Submission of forecast override request by an authorized Demand Planner.

**Preconditions:** Forecast Publication is in Draft state. Non-empty business justification provided.

**Business Behavior:** Execute DE-D-005 (Evaluate Forecast Override). Validate justification and deviation limits against PO-D-022. If accepted, record Forecast Override entity (SE-D-028) preserving original system value, planner identity, justification, and timestamp. Update published forecast line value.

**State Transitions:** Draft (Modified by Override).

**Business Transaction:** Protects SE-D-003 aggregate root.

**Decisions Invoked:** DE-D-005 (Evaluate Forecast Override).

**Events Published:** EV-D-012 (Forecast Override Recorded).

**Notifications Produced:** BN-D-012 (Forecast Override Applied).

**Traceability:** Owned by SE-D-003. Invoked by FS-D-007.

---

#### AB-D-009 – Publish Forecast Publication

**Purpose:** Release a Draft Forecast Publication as the single authoritative enterprise demand projection for a Planning Scope and horizon.

**Business Intent:** Formalize publication authorization, transition Draft to Published, supersede previous publications, and notify downstream consumers.

**Trigger:** Completion of forecast generation and processing of all pending overrides.

**Preconditions:** Draft Forecast Publication populated. Completeness threshold met per PO-D-020. Assumption sign-off complete per PO-D-025.

**Business Behavior:** Execute DE-D-004 (Approve Forecast Publication). If approved, transition Draft to Published, record Publication Time, and atomically transition previous Published version for the scope/horizon to Superseded. Publish BN-D-011.

**State Transitions:** Draft → Published. Previous Published → Superseded.

**Business Transaction:** Protects SE-D-003 aggregate root. Atomic publication and version superseding.

**Decisions Invoked:** DE-D-004 (Approve Forecast Publication).

**Events Published:** EV-D-013 (Forecast Publication Published).

**Notifications Produced:** BN-D-011 (Forecast Published).

**Traceability:** Owned by SE-D-003. Invoked by FS-D-008.

---

#### SE-D-004 – Demand Behavior Assessment

**Business Intent:** Provide a continuously current enterprise interpretation of demand behavior for a monitored planning entity, detecting meaningful changes from expected patterns.

**Enterprise Meaning:** The Demand Behavior Assessment is the aggregate root that maintains the current demand behavior state for one SKU-Location. It records the enterprise's current interpretation of whether demand behavior is Normal, Elevated, Depressed, or Critical. Incoming demand signals are evaluated against baseline parameters by the owning Aggregate Behavior `AB-D-010`; the assessment aggregate holds the authoritative interpreted state and state history.

**Applied Semantic Pattern:** Continuous Assessment

**Identity:** Item (SE-C-001) + Location (SE-C-002). Exactly one assessment exists per monitored item-location.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Sense Demand (CA-D-003) |
| Authoritative Representation | The enterprise's current interpretation of demand behavior state (Normal, Elevated, Depressed, Critical) for a SKU-Location. |
| Business Responsibility | Continuously evaluate incoming demand signals against baselines and maintain the authoritative behavior state. |
| Authority Scope | Per monitored Item-Location. |
| Intended Consumers | Detect Demand Exceptions, Forecast Demand (refresh trigger), Segment Demand, Classify Demand, Supply Intelligence. |
| Non-Intended Consumers | Direct customer order promising (must consume Promise Intelligence). |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | At any moment, exactly one current state exists per monitored item-location. State change events are immutable and traceable to evidence. |
| Required Interpretation | Consumers must treat the Current State as the authoritative operational behavior status. |
| Known Limitations | Reflects operational demand sensing; does not project future tactical horizons. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Normal | Demand signals within expected baseline variability. | AB-D-010 / Initial |
| Elevated | Demand signals exceed Significant threshold positively. | DE-D-006 (Elevated) |
| Depressed | Demand signals exceed Significant threshold negatively. | DE-D-006 (Depressed) |
| Critical | Demand signals exceed Critical threshold with corroboration. | DE-D-006 (Critical) |

- Terminal States: None (monitored continuously while active).
- History Preservation: State Change Events retained permanently.
- Versioning Rules: State changes append immutable State Change Events.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Item (SE-C-001), Location (SE-C-002) | Yes | Yes | SKU-Location identity. |
| State | Current State (Normal, Elevated, Depressed, Critical) | Yes | No | Current operational behavior state. |
| Assessment | Baseline Mean, Baseline StdDev | Yes | No | Expected baseline parameters from Demand Understanding. |
| Assessment | Last Deviation Magnitude, Direction | Yes | No | Assessed deviation from baseline in standard deviations ($\sigma$). |
| Assessment | Corroboration Count | Yes | No | Number of independent signal sources supporting deviation. |
| Assessment | Assessment Confidence | Yes | No | Multi-dimensional confidence level (High, Medium, Low). |
| History | Collection of State Change Events | Yes | No | Immutable audit log of all historical state transitions. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| monitors demand for | Item (`SE-C-001`) | Many-to-One |
| monitored at | Location (`SE-C-002`) | Many-to-One |
| uses baseline from | Demand Understanding (`SE-D-002`) | Many-to-One |

**Dependencies:** `SE-C-001`, `SE-C-002`, `SE-D-002`, `PO-D-031`.

**Invariants:** Exactly one current state per monitored item-location. A Critical state transition requires corroboration by $\ge 2$ independent signal sources within the active operational planning bucket (`PO-D-031`).

**Traceability:** Business Owner: CA-D-003. Produced By: AB-D-010. Upward: CN-002, CN-006.

---

#### AB-D-010 – Maintain Demand Behavior Understanding

**Purpose:** Evaluate streaming demand signals against baseline parameters and update the Demand Behavior Assessment state.

**Business Intent:** Ensure the enterprise maintains a continuously current, evidence-based understanding of demand behavior changes.

**Trigger:** Incoming demand signal received for a monitored Item-Location.

**Preconditions:** Target Demand Behavior Assessment exists or is initialized. Baseline parameters available from `SE-D-002`.

**Business Behavior:** Execute `BA-D-003` (Assess Demand Signal Deviation), `BA-D-004` (Determine Demand Behavior State), and `DE-D-006` (Evaluate Demand Signal for State Change). If state changes, update Current State and append an immutable State Change Event recording deviation, direction, corroboration count, and confidence.

**State Transitions:** Normal $\leftrightarrow$ Elevated / Depressed $\leftrightarrow$ Critical.

**Business Transaction:** Protects `SE-D-004` aggregate root.

**Decisions Invoked:** DE-D-006 (Evaluate Demand Signal for State Change), DE-D-007 (Trigger Forecast Refresh on Critical State) — DE-D-007 is invoked only when the new state is Critical.

**Events Published:** `EV-D-015` (Demand Behavior Changed), `EV-D-016` (Critical Demand Behavior Detected).

**Notifications Produced:** `BN-D-015` (Demand Behavior Changed), `BN-D-016` (Critical Demand Behavior Requires Action).

**Traceability:** Owned by `SE-D-004`. Invoked by `FS-D-009`.

---

#### SE-D-005 – Planning Classification Assignment

**Business Intent:** Maintain the enterprise's continuously current planning classification for a planning entity under a governed classification scheme.

**Enterprise Meaning:** The Planning Classification Assignment is the aggregate root that maintains the authoritative classification (e.g. ABC volume, XYZ variability) for an Item or Customer. It enables differentiated planning strategies across the enterprise.

**Applied Semantic Pattern:** Governed Assignment

**Identity:** Entity Type + Entity Identifier + Classification Type.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Segment Demand (CA-D-004) |
| Authoritative Representation | The enterprise's authoritative classification for an entity under a specific scheme. |
| Business Responsibility | Compute and maintain entity classifications per Segmentation Policy (`PO-D-035`). |
| Authority Scope | Per Entity and Classification Type. |
| Intended Consumers | Forecast Demand, Prioritize Demand, Inventory Planning, Supply Intelligence. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Exactly one current classification exists per entity and classification type. |
| Required Interpretation | Consumers must apply planning policies corresponding to the active classification. |
| Known Limitations | Reflects segmentation scheme; does not replace item priority scoring. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Unclassified | Entity registered, evidence insufficient. | AB-D-011 |
| Classified | Valid class assigned (e.g., Class A, B, or C). | DE-D-008 |

- Terminal States: None.
- History Preservation: Assignment Change Events retained permanently.
- Versioning Rules: Re-evaluations append immutable Assignment Change Events.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Entity Type (Item/Customer), Entity Identifier, Classification Type | Yes | Yes | Target entity and scheme. |
| Content | Current Classification (e.g., A, B, C, Unclassified) | Yes | No | Assigned class label. |
| Content | Analog Item Reference (`SE-C-001`) | No | No | Linked analog product reference for new item fallback (`PO-D-019`). |
| Content | Classification Confidence, Score | Yes | No | Quantitative fit score and confidence. |
| Content | Assignment Rationale, Policy Version | Yes | No | Business explanation and governing policy version. |
| History | Collection of Assignment Change Events | Yes | No | Audit history of classification changes. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| classifies | Item (`SE-C-001`) / Customer (`SE-C-003`) | Many-to-One |
| governed by | Segmentation Policy (`PO-D-035`) | Many-to-One |

**Dependencies:** `SE-C-001`, `SE-C-003`, `SE-D-002`, `PO-D-035`.

**Invariants:** Exactly one current classification per entity and classification type. Re-evaluations must apply `PO-D-035` rules.

**Traceability:** Business Owner: CA-D-004. Produced By: AB-D-011. Upward: CN-004, CN-007.

---

#### AB-D-011 – Classify Planning Entity

**Purpose:** Evaluate entity demand attributes against Segmentation Policy rules and update the Planning Classification Assignment.

**Business Intent:** Ensure planning classifications remain aligned with governed segmentation rules.

**Trigger:** Scheduled re-evaluation, Segmentation Policy change, or planner override.

**Preconditions:** Entity demand attributes available from `SE-C-021` / `SE-D-002`. Policy `PO-D-035` active.

**Business Behavior:** Execute `BA-D-005` (Compute Planning Classification) and `DE-D-008` (Determine Planning Classification). If class changes, update Current Classification and append Assignment Change Event.

**State Transitions:** Unclassified $\rightarrow$ Classified, or Class X $\rightarrow$ Class Y.

**Business Transaction:** Protects `SE-D-005` aggregate root.

**Decisions Invoked:** `DE-D-008` (Determine Planning Classification).

**Events Published:** `EV-D-017` (Planning Classification Changed).

**Notifications Produced:** `BN-D-017` (Planning Classification Changed).

**Traceability:** Owned by `SE-D-005`. Invoked by `FS-D-011`.

---

#### SE-D-006 – Demand Behavior Assignment

**Business Intent:** Maintain the enterprise's authoritative behavioral classification for a planning entity under a governed behavior dimension.

**Enterprise Meaning:** The Demand Behavior Assignment classifies how demand behaves statistically (e.g. Continuous, Intermittent, Seasonal, Lumpy, Trend) for a SKU-Location to govern statistical model selection.

**Applied Semantic Pattern:** Governed Assignment

**Identity:** Entity Type + Entity Identifier + Behavior Dimension.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Classify Demand (CA-D-005) |
| Authoritative Representation | The enterprise's authoritative demand behavior classification for a SKU-Location and dimension. |
| Business Responsibility | Analyze statistical features of demand series and assign behavior classification per `PO-D-037`. |
| Authority Scope | Per SKU-Location and Behavior Dimension. |
| Intended Consumers | Forecast Demand (model selection), Detect Demand Exceptions, Inventory Planning. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Exactly one current behavior classification per entity and dimension. |
| Required Interpretation | Model selection algorithms must consume this classification to filter candidate models. |
| Known Limitations | Statistical classification based on historical series structure. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Unclassified | Insufficient history for statistical pattern analysis. | AB-D-012 |
| Classified | Assigned valid behavior class (Continuous, Intermittent, Seasonal, Lumpy, Trend). | DE-D-009 |

- Terminal States: None.
- History Preservation: Behavior Change Events retained permanently.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Entity Type, Entity Identifier, Behavior Dimension | Yes | Yes | SKU-Location and dimension. |
| Content | Current Classification (Continuous, Intermittent, Seasonal, Lumpy, Trend, Unclassified) | Yes | No | Behavior class label. |
| Content | Statistical Features (CV, Autocorrelation, Trend p-value, Inter-arrival Interval) | Yes | No | Calculated statistical evidence metrics. |
| Content | Classification Confidence, Policy Version | Yes | No | Statistical confidence and policy reference. |
| History | Collection of Behavior Change Events | Yes | No | Audit log of behavior classification changes. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| classifies behavior of | Item (`SE-C-001`) + Location (`SE-C-002`) | Many-to-One |
| governed by | Classification Policy (`PO-D-037`) | Many-to-One |

**Dependencies:** `SE-C-001`, `SE-C-002`, `SE-C-021`, `PO-D-037`.

**Invariants:** Exactly one current classification per entity and dimension. Classification must conform to `PO-D-037`.

**Traceability:** Business Owner: CA-D-005. Produced By: AB-D-012. Upward: CN-004, CN-009.

---

#### AB-D-012 – Classify Demand Behavior

**Purpose:** Calculate statistical features of a demand series and update the Demand Behavior Assignment.

**Business Intent:** Automate statistical demand pattern recognition to ensure optimal forecasting model matching.

**Trigger:** Scheduled cadence, demand history refresh, or policy update.

**Preconditions:** Cleansed demand history available via `SE-C-021` Enterprise Picture series. Policy `PO-D-037` active.

**Business Behavior:** Execute `BA-D-006` (Determine Behavior Classification) and `DE-D-009` (Determine Behavior Classification). Update classification and append Behavior Change Event if modified.

**State Transitions:** Unclassified $\rightarrow$ Classified, or Class A $\rightarrow$ Class B.

**Business Transaction:** Protects `SE-D-006` aggregate root.

**Decisions Invoked:** `DE-D-009` (Determine Behavior Classification).

**Events Published:** `EV-D-019` (Demand Behavior Classification Changed).

**Notifications Produced:** `BN-D-019` (Demand Behavior Classification Changed).

**Traceability:** Owned by `SE-D-006`. Invoked by `FS-D-012`.

---

#### SE-D-007 – Planning Priority Assignment

**Business Intent:** Maintain the enterprise's continuously current assessment of planning importance for every planning entity.

**Enterprise Meaning:** The Planning Priority Assignment calculates and maintains the relative planning priority level (Critical, High, Medium, Low) and composite score for a planning entity, establishing execution precedence across all planning capabilities.

**Applied Semantic Pattern:** Governed Assignment

**Identity:** Entity Type + Entity Identifier.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Prioritize Demand (CA-D-006) |
| Authoritative Representation | The enterprise's authoritative planning priority score and level for an entity. |
| Business Responsibility | Compute priority scores from business dimensions and assign priority levels per `PO-D-039`. |
| Authority Scope | Per Planning Entity. |
| Intended Consumers | Forecast Demand, Detect Demand Exceptions, Inventory Planning, Supply Intelligence, Scenario Intelligence. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Exactly one current priority level and score per entity. Decision rationale and business validity preserved. |
| Required Interpretation | Downstream capabilities must prioritize exception review, allocation, and refresh execution using this priority. |
| Known Limitations | Priority establishes relative ordering, not absolute resource reservation. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Unclassified | Mandatory business evidence unavailable. | AB-D-013 |
| Assigned | Priority score computed and level assigned (Critical, High, Medium, Low). | DE-D-010 |

- Terminal States: None.
- History Preservation: Priority Change Events retained permanently.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Entity Type, Entity Identifier | Yes | Yes | Target entity. |
| Content | Current Priority Level (Critical, High, Medium, Low, Unclassified) | Yes | No | Assigned priority level. |
| Content | Priority Score (0–100) | Yes | No | Composite priority score. |
| Content | Dimension Score Breakdown | Yes | No | Scores for Revenue, Strategy, Risk, Contractual. |
| Content | Decision Rationale, Business Validity | Yes | No | Explanation and validity conditions. |
| History | Collection of Priority Change Events | Yes | No | Audit log of priority level changes. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| prioritizes | Item (`SE-C-001`) / Customer (`SE-C-003`) | Many-to-One |
| governed by | Prioritization Policy (`PO-D-039`) | Many-to-One |

**Dependencies:** `SE-C-001`, `SE-C-003`, `SE-D-002`, `SE-D-005`, `SE-D-006`, `PO-D-039`.

**Invariants:** Exactly one current priority per entity. Priority scoring must conform to `PO-D-039`.

**Traceability:** Business Owner: CA-D-006. Produced By: AB-D-013. Upward: CN-007, CN-009.

---

#### AB-D-013 – Prioritize Planning Entity

**Purpose:** Calculate multi-dimensional priority scores and update the Planning Priority Assignment.

**Business Intent:** Ensure enterprise resources and planner attention are focused on highest-value demand entities.

**Trigger:** Scheduled re-evaluation, segment/behavior change, or policy update.

**Preconditions:** Entity business attributes available from `SE-D-002`, `SE-D-005`, `SE-D-006`. Policy `PO-D-039` active.

**Business Behavior:** Execute `BA-D-007` (Compute Planning Priority Score) and `DE-D-010` (Determine Planning Priority). Update score, level, rationale, and append Priority Change Event.

**State Transitions:** Unclassified $\rightarrow$ Assigned, or Level X $\rightarrow$ Level Y.

**Business Transaction:** Protects `SE-D-007` aggregate root.

**Decisions Invoked:** `DE-D-010` (Determine Planning Priority).

**Events Published:** `EV-D-020` (Planning Priority Changed).

**Notifications Produced:** `BN-D-020` (Planning Priority Changed).

**Traceability:** Owned by `SE-D-007`. Invoked by `FS-D-013`.

---

#### SE-D-008 – Forecast Quality Assessment

**Business Intent:** Publish an authoritative, periodic enterprise assessment of forecast quality.

**Enterprise Meaning:** The Forecast Quality Assessment is the aggregate root that maintains the version series of published forecast quality assessments for a Planning Scope and Evaluation Period. It measures WAPE, Bias, Accuracy, FVA, and Override Effectiveness against governed enterprise objectives.

**Applied Semantic Pattern:** Published Knowledge

**Identity:** Assessment Scope + Evaluation Period + Version Number.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Evaluate Demand Quality (CA-D-007) |
| Authoritative Representation | The enterprise's authoritative evaluation of forecast accuracy, bias, and stability for a scope and period. |
| Business Responsibility | Measure forecast performance against actual outcomes and publish authoritative quality assessments per `PO-D-041`. |
| Authority Scope | Per Assessment Scope and Evaluation Period. |
| Intended Consumers | Learn From Demand, Forecast Demand (model feedback), Demand Planners and Managers. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Published assessments are immutable, permanently retained, and computed per `PO-D-041`. |
| Required Interpretation | Consumers must treat Published assessments as the official enterprise audit of forecast accuracy. |
| Known Limitations | Evaluates historical performance after actual demand has materialized. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Draft | Quality metrics computed from actuals. | AB-D-014 |
| Published | Released as authoritative record. Previous Published $\rightarrow$ Superseded. | DE-D-011 |
| Superseded | Replaced by a revised Published assessment for same scope/period. | DE-D-011 |

- Terminal States: Superseded.
- History Preservation: All versions retained permanently.

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Assessment Scope Identifier | Reference (SE-C-010) | Yes | Aggregate identity component. |
| Evaluation Period | Structured table | Yes | Aggregate identity component. |
| Versions | List of Forecast Quality Assessment Version (SE-D-014) | Yes | At least one version. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| evaluates forecast for | Planning Scope (`SE-C-010`) | Many-to-One |
| evaluates | Forecast Publication (`SE-D-003`) | Many-to-One |
| governed by | Forecast Measurement Policy (`PO-D-041`) | Many-to-One |

**Dependencies:** `SE-C-010`, `SE-D-003`, `SE-C-021`, `PO-D-041`.

**Invariants:** Exactly one Published assessment per scope and period. Published assessments are immutable and permanently retained.

**Traceability:** Business Owner: CA-D-007. Produced By: AB-D-014. Upward: CN-005, CN-006, CN-012.

---

#### SE-D-014 — Forecast Quality Assessment Version

**Business Intent:** Preserve one immutable version of the Forecast Quality Assessment for an Assessment Scope and Evaluation Period.

**Identity:** Version Number uniquely identifies the Forecast Quality Assessment Version within its owning Forecast Quality Assessment aggregate.

**Owning Aggregate Root:** SE-D-008 Forecast Quality Assessment

**Information Model**

| Attribute | Type | Mandatory | Description |
| --- | --- | --- | --- |
| Version Number | Integer | Yes | Monotonically increasing within the Assessment Scope and Evaluation Period. |
| Mandatory Metrics | Structured table | Yes | WAPE, Forecast Bias, Forecast Accuracy. |
| Optional Metrics | Structured table | No | MAPE, Forecast Stability, FVA, Override Effectiveness. |
| Completeness Score | Decimal | Yes | Percentage of covered series with actual demand data. |
| Overall Quality State | Governed Identifier Reference (SE-C-037) | Yes | Excellent, Good, Adequate, Poor, Critical. |
| Metric Computation Evidence | Structured table | Yes | References to forecast versions, actual demand sources, and policy version. |
| Publication Time | Timestamp (SE-C-022) | No | When this version was published. Absent for Draft versions. |
| Lifecycle State | Enum (Draft, Published, Superseded) | Yes | Current state of this version. |

**Lifecycle Specification Contract**

| State | Description |
| --- | --- |
| Draft | Being prepared; not authoritative. |
| Published | Authoritative; previous Published becomes Superseded. |
| Superseded | Replaced by a newer Published version. |

- Permitted Transitions: Draft → Published; Published → Superseded.
- Terminal State: Superseded.
- History Preservation: All versions are retained permanently.

**Invariants**
- Version Number is monotonically increasing within the owning Forecast Quality Assessment.
- Exactly one Forecast Quality Assessment Version is Published per Assessment Scope and Evaluation Period at any time.
- A Published Forecast Quality Assessment Version is immutable.
- A Superseded Forecast Quality Assessment Version is immutable.

**Traceability**

| Direction | Reference |
| --- | --- |
| Upward | Constitution CN-003, CN-004; ARS §3, §4, §16; Enterprise Semantic Model Chapter 4. |
| Owner | SE-D-008 Forecast Quality Assessment |
| Downward | Learn From Demand, Explain Demand, Forecast Demand. |

---

#### AB-D-014 – Evaluate Forecast Quality

**Purpose:** Calculate forecast quality metrics against actual outcomes and publish the Forecast Quality Assessment.

**Business Intent:** Provide transparent, deterministic measurement of forecasting performance.

**Trigger:** Scheduled evaluation cadence per `PO-D-041` or on-demand after actuals materialize.

**Preconditions:** Published forecasts (`SE-D-003`) and actual demand (`SE-C-021`) available for full evaluation period. Completeness meets `PO-D-041` threshold.

**Business Behavior:** Execute `BA-D-008` (Compute Forecast Quality Metrics), `BA-D-009` (Determine Forecast Quality State), and `DE-D-011` (Publish Forecast Quality Assessment). If published, transition Draft to Published and supersede previous version.

**State Transitions:** Draft $\rightarrow$ Published. Previous Published $\rightarrow$ Superseded.

**Business Transaction:** Protects `SE-D-008` aggregate root.

**Decisions Invoked:** `DE-D-011` (Publish Forecast Quality Assessment).

**Events Published:** `EV-D-021` (Forecast Quality Assessment Published).

**Notifications Produced:** `BN-D-021` (Forecast Quality Assessment Published).

**Traceability:** Owned by `SE-D-008`. Invoked by `FS-D-014`.

#### SE-D-009 — Reserved

This identifier is reserved. The Demand Planning Condition concept has been replaced by the centralized Exception model.

Demand Intelligence detects demand exception evidence and publishes it to Core Exception Management. The SE-C-019 Exception aggregate, owned by the Core Domain, is the single authoritative enterprise record of unsatisfied constraints.

Demand Intelligence shall not create, update, or resolve SE-C-019 Exception instances directly.

**EV-D-022 – Demand Exception Evidence Evaluated**

**Purpose:** Signal that the Demand domain has completed its evaluation of exception evidence and is ready to publish to Core Exception Management.
**Business Intent:** Establish a workflow-level event that the Notification Node maps to BN-D-022 and BN-D-023. Because CA-D-008 owns no Aggregate Roots, it cannot publish Aggregate Events; this workflow event fulfills the ARS requirement that every Business Notification references an Enterprise Event.
**Published By:** FS-D-015 (Detect Demand Exception Evidence) Workflow Notification Node.

---

#### SE-D-010 – Demand Explanation

**Business Intent:** Provide an immutable, auditable enterprise record of the reasoning behind any demand intelligence conclusion.

**Enterprise Meaning:** The Demand Explanation is the aggregate root maintaining an immutable explanation for a demand artifact. It composes preserved historical evidence, decisions, policies, and templates into a deterministic Structured Reasoning Graph.

**Applied Semantic Pattern:** Explanation

**Identity:** Demand Explanation Identifier, globally unique.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Explain Demand (CA-D-009) |
| Authoritative Representation | The canonical, deterministic Structured Reasoning Graph explaining a demand conclusion. |
| Business Responsibility | Compose structured explanations from preserved historical evidence and policies. |
| Authority Scope | Per explained artifact and evidence version set. |
| Intended Consumers | Planners, Auditors, AI Copilot, Learn From Demand. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Explanations are immutable, deterministic, and permanently retained. Identical requests return existing explanation. |
| Required Interpretation | Consumers must render the Structured Reasoning Graph into natural language without altering underlying logic. |
| Known Limitations | Explains historical conclusions using evidence preserved at creation time. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Created | Explanation composed and permanently persisted. | AB-D-016 |

- Terminal States: Created (Immutable).
- History Preservation: Retained permanently.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Demand Explanation Identifier | Yes | Yes | Unique explanation identity. |
| Target | Explained Artifact Reference, Artifact Type | Yes | Yes | Target artifact explained. |
| Content | Structured Reasoning Graph (Nodes, Edges, Provenance) | Yes | Yes | Canonical reasoning structure. |
| Content | Natural Language Rendering | Yes | Yes | Text representation of graph. |
| Metadata | Preserved Evidence References (with historical versions), Template Version | Yes | Yes | Historical source versions. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| explains | Demand Artifact (`SE-D-003`/`SE-D-008`/`SE-D-009`) | Many-to-One |

**Dependencies:** Preserved evidence from all Demand Intelligence capabilities, Explanation Template Catalog.

**Invariants:** Immutable once created. Graph construction must be deterministic and consume preserved historical versions.

**Traceability:** Business Owner: CA-D-009. Produced By: AB-D-016. Upward: CN-005, CN-006, CN-008.

---

#### AB-D-016 – Establish Demand Explanation

**Purpose:** Compose a Structured Reasoning Graph for a demand conclusion and record an immutable Demand Explanation.

**Business Intent:** Provide full transparency and explainability for enterprise demand conclusions.

**Trigger:** Planner request, audit request, or automatic governance trigger.

**Preconditions:** Target artifact exists with preserved historical evidence. Explanation template available.

**Business Behavior:** Execute `BA-D-012` (Compose Demand Explanation). Build Structured Reasoning Graph with provenance on every node. Create immutable `SE-D-010` aggregate.

**State Transitions:** None $\rightarrow$ Created.

**Business Transaction:** Protects `SE-D-010` aggregate root.

**Decisions Invoked:** None (deterministic composition).

**Events Published:** `EV-D-024` (Demand Explanation Established).

**Notifications Produced:** `BN-D-024` (Demand Explanation Established).

**Traceability:** Owned by `SE-D-010`. Invoked by `FS-D-016`.

---

#### SE-D-011 – Demand Learning

**Business Intent:** Provide an authoritative, immutable enterprise record of what the enterprise has concluded about a recurring demand phenomenon.

**Enterprise Meaning:** The Demand Learning is the aggregate root maintaining an immutable learning discovery. Each learning captures the observed recurring pattern, enterprise conclusion, supporting evidence, and proposed policy/model improvements.

**Applied Semantic Pattern:** Learning

**Identity:** Enterprise Learning Identifier, globally unique.

**Authority Specification Contract**

| Section | Value |
|---------|-------|
| Business Owner | Learn From Demand (CA-D-010) |
| Authoritative Representation | The enterprise's authoritative discovery regarding recurring demand patterns or performance. |
| Business Responsibility | Derive and record immutable enterprise learnings from multi-period historical evidence. |
| Authority Scope | Enterprise-wide. |
| Intended Consumers | Planning Governance, Forecast Demand, Segment Demand, Classify Demand, Prioritize Demand. |

**Consumer Specification Contract**

| Section | Value |
|---------|-------|
| Business Guarantees | Learnings are immutable, supported by multi-period evidence, and permanently retained. |
| Required Interpretation | Governance must evaluate learnings for policy/parameter refinements in subsequent planning cycles ($N+1$). |
| Known Limitations | Discovers historical patterns; policy adoption requires governance ratification. |

**Lifecycle Specification Contract**

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Created | Learning derived and permanently persisted. | AB-D-017 |

- Terminal States: Created (Immutable).
- History Preservation: Retained permanently.

**Information Model**

| Category | Information | Mandatory | Immutable | Description |
|----------|-------------|-----------|-----------|-------------|
| Identity | Enterprise Learning Identifier | Yes | Yes | Unique learning identity. |
| Content | Learning Statement, Learning Type | Yes | Yes | Enterprise conclusion and category. |
| Content | Supporting Evidence References, Pattern Confidence | Yes | Yes | Multi-period evidence and confidence. |
| Content | Improvement Opportunities, Intervention Confidence | No | Yes | Proposed policy/model refinements. |
| Metadata | Learning Analysis Policy Version, Creation Timestamp | Yes | Yes | Policy version reference. |

**Relationships**

| Relationship | Target Object | Cardinality |
|--------------|---------------|-------------|
| governed by | Learning Analysis Policy (`PO-D-048`) | Many-to-One |

**Dependencies:** Historical evidence across multiple periods (`SE-D-008`, `SE-D-009`, `SE-D-010`, `SE-D-004`), `PO-D-048`.

**Invariants:** Immutable once recorded. Supported by evidence spanning minimum recurrence threshold defined in `PO-D-048`.

**Traceability:** Business Owner: CA-D-010. Produced By: AB-D-017. Upward: CN-007, CN-012.

---

#### AB-D-017 – Establish Demand Learning

**Purpose:** Analyze multi-period historical evidence and record immutable Demand Learnings.

**Business Intent:** Enable continuous enterprise learning and systematic policy/model improvement.

**Trigger:** Scheduled analytical cadence or governance event.

**Preconditions:** Historical evidence spanning minimum periods available. Policy `PO-D-048` active.

**Business Behavior:** Execute `BA-D-013` (Derive Demand Learning). Evaluate recurrence and evidence strength. Create immutable `SE-D-011` aggregate for qualified discoveries.

**State Transitions:** None $\rightarrow$ Created.

**Business Transaction:** Protects `SE-D-011` aggregate root.

**Decisions Invoked:** None (governance discovery).

**Events Published:** `EV-D-025` (Demand Learning Established).

**Notifications Produced:** `BN-D-025` (Demand Learning Established).

**Traceability:** Owned by `SE-D-011`. Invoked by `FS-D-017`.



#### SE-D-018 – Demand Intervention Impact

**Business Intent:** Provide the authoritative enterprise definition of the assessed impact of a planned commercial intervention on demand for a specific item-location combination.

**Enterprise Meaning:** A Demand Intervention Impact is the enterprise-recognised assessment of how a planned commercial action (promotion, price change, marketing event) will affect demand for a specific item at a specific location. It answers "what is the expected demand change from this intervention?" The impact is computed from historical elasticity data, intervention characteristics, and current demand context. It does not modify the baseline forecast; it provides a deterministic adjustment that the forecast capability consumes.

**Identity:** Intervention Impact Identifier is the immutable enterprise identity of the Demand Intervention Impact.

**Applied Semantic Patterns:** Published Knowledge (ARS App. A.3)

**Semantic Ownership**
- **Demand Intervention Impact owns:** identity, intervention reference, item, location, assessed lift, confidence, temporal validity, model provenance, lifecycle.
- **Demand Intervention Impact excludes:** intervention definition (owned by SE-C-039), baseline forecast (owned by SE-D-003), allocation decisions, supply adjustments.

**Authority Specification Contract**

| Section                      | Value                                                                                                        |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Semantic Authority           | Demand Domain                                                                                                 |
| Steward Domain               | Demand                                                                                                        |
| Mutation Authority           | Enterprise-Derived Planning Fact                                                                              |
| Authoritative Representation | The assessed demand impact of a planned commercial intervention.                                              |
| Authority Scope              | Per intervention, per item-location.                                                                          |
| Intended Consumers           | CA-D-002 Forecast Demand, Supply Intelligence, Scenario Intelligence.                                         |
| Non-Intended Consumers       | None.                                                                                                         |
| Business Responsibility      | Preserve the authoritative enterprise meaning, identity, lifecycle integrity, and published contract of this semantic object. |
| Supersedes                   | None.                                                                                                         |
| Superseded By                | None.                                                                                                         |

**Consumer Specification Contract**

| Section                 | Value                                                                                                                                                                                               |
| ----------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Business Guarantees     | Every published Demand Intervention Impact is immutable and traceable to its source intervention. Every impact carries a confidence level and model provenance.                                    |
| Required Interpretation | Consumers shall treat the Published impact as a deterministic demand adjustment. It does not replace the baseline forecast; it supplements it.                                                      |
| Known Limitations       | Does not define the intervention itself. Does not modify the baseline forecast. Does not guarantee actual demand outcomes.                                                                          |
| Declared Consumers      | Governed by Chapter 5.2 Declared Consumer Matrix.                                                                                                                                                   |
| Consumer Responsibility | The consumer shall reference this object solely by its immutable identifier and shall not infer semantics not published in this contract.                                                           |
| Required Attributes     | Immutable identifier; mandatory attributes; attributes required by declared structural relationships. Domain-specific required attributes must be declared in the consuming Domain Semantic Model. |
| Authoritative Source    | SE-D-018 Demand Intervention Impact.                                                                                                                                                                |

**Lifecycle Specification Contract**

| State      | Description                                           |
| ---------- | ----------------------------------------------------- |
| Draft      | Impact being assessed; not authoritative.             |
| Published  | Authoritative; previous Published becomes Superseded. |
| Superseded | Replaced by a newer Published version.                |

- Permitted Transitions: Draft → Published; Published → Superseded.
- Terminal State: Superseded.
- History Preservation: All versions retained permanently.

**Transition Trigger Standard:** Transitions are triggered only by the Business Trigger associated with this object's Mutation Authority archetype, as defined in ESM Section 2.7. Capability specifications shall map Aggregate Behaviors to these triggers.

**Information Model**

| Attribute                     | Type                                    | Mandatory | Description                                                    |
| ----------------------------- | --------------------------------------- | --------- | -------------------------------------------------------------- |
| Intervention Impact Identifier | ID (immutable)                          | Yes       | Unique enterprise identity.                                    |
| Intervention Reference        | Reference (SE-C-039)                    | Yes       | The Scenario Adjustment that defines the intervention.         |
| Item                          | Reference (SE-C-001)                    | Yes       | The item whose demand is affected.                             |
| Location                      | Reference (SE-C-002)                    | Yes       | The location where the demand impact applies.                  |
| Assessed Demand Lift          | Quantity (SE-C-023)                     | Yes       | The expected demand change; non-negative.                      |
| Lift Confidence               | Decimal (0–100)                         | Yes       | Enterprise confidence in the assessed lift.                    |
| Temporal Validity             | Temporal Window (SE-C-028)              | Yes       | The time interval during which the intervention is active.     |
| Model Provenance              | Governed Identifier Reference (SE-C-037)| Yes       | The modeling approach used to compute the lift.                |
| Lifecycle State               | Enum (Draft, Published, Superseded)     | Yes       | Current state.                                                 |

**Relationships**

| Relationship       | Target Object               | Cardinality | Description                                      |
| ------------------ | --------------------------- | ----------- | ------------------------------------------------ |
| assesses impact of | Scenario Adjustment (SE-C-039) | Many-to-One | The intervention being assessed.                 |
| affects            | Item (SE-C-001)             | Many-to-One | The item whose demand is affected.               |
| affects            | Location (SE-C-002)         | Many-to-One | The location where the impact applies.           |
| consumed by        | Forecast Publication (SE-D-003) | One-to-Many | Forecast versions that consume this impact.      |

**Invariants:**
- Intervention Impact Identifier is immutable.
- Assessed Demand Lift must be non-negative.
- Temporal Validity must be a valid interval.
- Published version is immutable.
- Intervention Reference must point to an active Scenario Adjustment.

**Dependencies:** SE-C-039 Scenario Adjustment, SE-C-001 Item, SE-C-002 Location, SE-C-023 Quantity, SE-C-028 Temporal Window, SE-C-037 Enterprise Governed Vocabulary.

**Traceability:** CN-003, CN-004; ARS §3, §4, §16, App. A.3.


#### AB-D-018 – Assess Demand Intervention Impact

**Purpose:** Create a Draft Demand Intervention Impact by computing the assessed demand lift.

**Business Intent:** Produce a deterministic impact assessment for a planned intervention.

**Trigger:** Scenario Adjustment published or updated (BN from Scenario Intelligence), or planner request.

**Preconditions:** SE-C-039 Scenario Adjustment is active. SE-D-002 Demand Understanding is published for the relevant Planning Scope.

**Business Behavior:** Invoke BA-D-016 (Model Intervention Lift) to compute the assessed demand lift. Create a Draft SE-D-018 with the computed lift, confidence, temporal validity, and model provenance.

**State Transitions:** None → Draft.

**Business Transaction:** Protects SE-D-018 aggregate. Atomic creation of Draft.

**Decisions Invoked:** None.

**Events Published:** None (Draft creation does not publish).

**Idempotency:** Re-execution with the same intervention and context updates the existing Draft rather than creating a duplicate.

**Concurrency:** Assessments for different interventions are independent. Same intervention is serialized.

**Traceability:** Owned by SE-D-018. Invoked by FS-D-018.

#### AB-D-019 – Publish Demand Intervention Impact

**Purpose:** Publish the Draft Demand Intervention Impact, making it authoritative.

**Business Intent:** Transfer the impact assessment to consuming capabilities.

**Trigger:** Assessment complete and publication criteria met.

**Preconditions:** Draft SE-D-018 exists. PO-D-050 publication criteria satisfied.

**Business Behavior:** Execute DE-D-014 (Approve Intervention Impact Publication). If Publish: transition Draft to Published, supersede previous Published version. If Do Not Publish: Draft retained.

**State Transitions:** Draft → Published; Previous Published → Superseded.

**Business Transaction:** Protects SE-D-018 aggregate. Atomic publication and superseding.

**Decisions Invoked:** DE-D-014.

**Events Published:** EV-D-023 (Demand Intervention Impact Published).

**Idempotency:** Re-execution on already-published version terminates immediately.

**Concurrency:** Publication for a given intervention is serialized.

**Traceability:** Owned by SE-D-018. Invoked by FS-D-019.

### 4.3.2 Entities

Entities are documented within their owning Aggregate Root specification. Key entities include:

- **State Change Event** (within SE-D-004) – records each demand behavior state transition.
- **Assignment Change Event** (within SE-D-005) – records each classification change.
- **Behavior Change Event** (within SE-D-006) – records each behavior classification change.
- **Priority Change Event** (within SE-D-007) – records each priority change.
- **Condition Change Event** (within SE-D-009) – records each condition lifecycle event.
- **Forecast Line** (within SE-D-003) – the forecast for a single item-location-time bucket.
- **Forecast Assumption** (within SE-D-003) – a declared assumption influencing the forecast.
- **Forecast Override** (within SE-D-003) – a planner’s replacement of a system forecast value, preserved as provenance.

---

### 4.3.3 Value Objects

Demand Intelligence reuses Enterprise Semantic Model value objects: Quantity (SE-C-023), Timestamp (SE-C-022), Duration (SE-C-024), Planning Horizon (SE-C-027), Need Window (SE-C-029), Planning Period (SE-C-034).

Domain-specific value objects (e.g., Prediction Interval, Forecast Configuration) will be defined within their owning Aggregate Root.

---

### 4.3.4 Reference Objects

Demand Intelligence does not define its own Reference Objects. All references to master data entities use the Enterprise Semantic Object identifiers directly (SE-C-001 Item, SE-C-002 Location, SE-C-003 Customer, SE-C-033 Calendar, SE-C-034 Planning Period).

---

### 4.3.5 Knowledge Artifacts

Knowledge Artifacts are defined in the Enterprise Measurement Model (Chapter 3). Each PI publishes its measured values as a KA-D-xxx. The KA minimum interface (Identifier, Owning Capability, Version, Confidence, Evidence Reference, Expiry Timestamp) is satisfied by the Publication Model section of each PI specification.

---

### 4.3.6 Semantic Completeness

All Demand-owned Semantic Objects reference only ratified Enterprise Semantic Objects. No placeholder semantics or unresolved dependencies remain. The domain has passed internal Consumer Verification against the Enterprise Semantic Completion Standard.

---

# Chapter 5 – Capability Model

## 5.1 Understand Demand – CA-D-001

**Business Intent:** Establish and maintain the enterprise's authoritative understanding of current demand by capturing and evaluating demand observations, and interpreting the Enterprise Picture into a published Demand Understanding that downstream capabilities rely upon.

**Enterprise Question:** What demand has the enterprise observed, and what does the enterprise currently understand about demand?

**Owned Semantic Objects:** SE-D-001 (Demand Observation), SE-D-002 (Demand Understanding).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | The Enterprise Picture (SE-C-021), Forecast Publication (SE-D-003). |
| **Produces** | Evaluated Demand Observations (SE-D-001); the Demand Understanding (SE-D-002). |
| **Feeds** | Forecast Demand, Sense Demand, Segment Demand, Classify Demand, Prioritize Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence. All consumption is within the same planning cycle. |

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Understand Demand |
|------------|---------------------------|
| Enterprise Picture (SE-C-021) | Provides the authoritative snapshot of demand facts. Understand Demand interprets the Enterprise Picture to produce the Demand Understanding. |
| Forecast Publication (SE-D-003) | Provides the enterprise's authoritative demand projection, which informs the Demand Understanding's interpretation of current demand patterns and volatility. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Item (SE-C-001) | Defines the products for which demand is observed. |
| Location (SE-C-002) | Defines the locations where demand is observed. |
| Customer (SE-C-003) | Defines the customers associated with demand observations. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Demand Data Acceptance Policy (PO-D-001) | Governs the evaluation of incoming demand observations. |
| Demand Understanding Materiality Policy (PO-D-011) | Governs when a new Demand Understanding version is published. |
| Demand Understanding Publication Cadence Policy (PO-D-012) | Governs the maximum staleness interval. |

**Business Guarantees:**
- Exactly one Published Demand Understanding exists for each Planning Scope at any moment.
- Every Accepted Demand Observation emits BN-D-006 (Demand Observation Accepted) and is made eligible for incorporation into the Enterprise Picture and, through it, into the Demand Understanding.
- No Rejected or Quarantined observation contributes to the Demand Understanding.
- Every interpretation dimension in the Demand Understanding is traceable to the Enterprise Picture version used.
- A Demand Understanding revision may consume a Published Forecast Publication as forward-looking context only for subsequent Demand Understanding versions.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-001 | Receive Demand Observation | BW-D-001 | FS-D-001 |
| CR-D-002 | Evaluate Demand Observation | BW-D-002 | FS-D-002 |
| CR-D-003 | Revise Demand Understanding | BW-D-003 | FS-D-003 |
| CR-D-004 | Publish Demand Understanding | BW-D-004 | FS-D-004 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-001 | Demand Observation Received | AB-D-001 |
| EV-D-002 | Demand Observation Evaluated | AB-D-002 |
| EV-D-003 | Demand Understanding Revised | AB-D-003 |
| EV-D-004 | Demand Understanding Published | AB-D-004 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-D-001 | EV-D-004 | Demand Understanding Published: Planning Scope, Version, Publication Time, Material Change Summary | At-least-once | Per Planning Scope | Near-real-time |
| BN-D-002 | EV-D-002 | Demand Observation Quarantined: Observation Identifier, Reason | At-least-once | Per observation | Near-real-time |
| BN-D-003 | EV-D-002 | Demand Observation Rejected: Observation Identifier, Reason | At-least-once | Per observation | Near-real-time |
| BN-D-005 | EV-D-001 | Demand Observation Received: Observation Identifier, Source | At-least-once | Per observation | Near-real-time |
| BN-D-006 | EV-D-002 | Demand Observation Accepted: Observation Identifier, Item (SE-C-001), Location (SE-C-002), Quantity (SE-C-023), Business Time, Observation Time, Observation Type, Source System Provenance, Customer (SE-C-003, if present), Confidence | At-least-once | Per observation | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| BN-D-011 Forecast Published | Forecast Demand | Revise the Demand Understanding with the latest authoritative demand projection as forward-looking context. | FS-D-003 |
| BN-P-001 Promise Confirmed | Promise Intelligence | Update demand line status with confirmed promise information. | FS-D-001 |
| BN-C-001 Enterprise Picture Published | Enterprise Picture Management (CA-C-019) | Provide the authoritative snapshot of demand facts for interpretation into the Demand Understanding. | FS-D-003 |

**BN-C-001 Enterprise Picture Published**

The Demand Intelligence domain expects the Core domain to publish a notification with the following contract. This contract is binding on the Core domain specification when it is authored.

| Field | Type | Business Meaning | Required |
|-------|------|------------------|----------|
| Planning Scope | Reference (SE-C-010) | The Planning Scope for which the Enterprise Picture was published. | Yes |
| Published Version Number | Integer | The version now Published. | Yes |
| Superseded Version Number | Integer (optional) | The previous Published version, if any. | No |
| Publication Time | Timestamp (SE-C-022) | When the picture became authoritative. | Yes |
| Material Change Summary | Structured (per category: boolean, details) | Which knowledge categories (Demand, Supply, Inventory) changed materially. | Yes |
| Periodic Refresh Flag | Boolean | True if published due to cadence rather than material change. | Yes |

*BN-P-001 Promise Confirmed: Future integration – contract to be ratified by Promise Intelligence domain. Until ratified, the consuming workflow FS-D-001 shall treat this notification as an optional enrichment. The absence of the notification shall not prevent demand observation processing*

- **Delivery Guarantees:**
  - **Delivery:** At-least-once
  - **Ordering:** Per Planning Scope
  - **Timeliness:** Near-real-time

- **Owning Capability:** Enterprise Picture Management (CA-C-019) — to be confirmed when Core domain is authored.


**Knowledge Handoff:** Understand Demand transforms raw demand observations and the Enterprise Picture into the enterprise's authoritative Demand Understanding. This interpretation becomes the baseline that all downstream demand capabilities consume. The Demand Understanding does not duplicate demand data; it provides the interpretive layer that answers "what does demand mean?"

**Feedback Target:** No feedback target — output is consumed forward in the current planning cycle.

**Traceability:** Business Owner: CA-D-001. Publishes EV-D-001–004 and BN-D-001–003, BN-D-005–006. Consumes BN-D-011 and external promise notifications. Realises BO-D-001.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-001 – Demand Observation | §4.3.1 | Frozen |
| Owned Semantic Objects | SE-D-002 – Demand Understanding | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-001 – Receive Demand Observation | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-002 – Evaluate Demand Observation | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-003 – Revise Demand Understanding | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-004 – Publish Demand Understanding | §4.3.1 | Frozen |
| Decisions | DE-D-001 – Accept Demand Observation | §6 | Aligned |
| Decisions | DE-D-002 – Publish Demand Understanding | §6 | Aligned |
| Rules | BR-D-001 – Demand Observation Identity | §7 | Aligned |
| Rules | BR-D-002 – Demand Understanding Aggregate Identity | §7 | Aligned |
| Rules | BR-D-100 – Rejected Observations Exclusion | §7 | Aligned |
| Rules | BR-D-101 – Quarantined Observations Exclusion | §7 | Aligned |
| Rules | BR-D-102 – Single Evaluation | §7 | Aligned |
| Rules | BR-D-103 – Single Published Demand Understanding | §7 | Aligned |
| Rules | BR-D-104 – Published Immutability | §7 | Aligned |
| Rules | BR-D-105 – Superseded Finality | §7 | Aligned |
| Rules | BR-D-106 – Temporal Recording | §7 | Aligned |
| Rules | BR-D-127 – No Raw Data Duplication | §7 | Aligned |
| Rules | BR-D-200 – Demand Signal Timeliness | §7 | Aligned |
| Rules | BR-D-201 – Demand Quantity Range Validity | §7 | Aligned |
| Rules | BR-D-202 – Source Reliability Threshold | §7 | Aligned |
| Rules | BR-D-203 – Duplicate Data Detection | §7 | Aligned |
| Rules | BR-D-204 – Material Change Required | §7 | Aligned |
| Rules | BR-D-205 – Interpretation Completeness | §7 | Aligned |
| Rules | BR-D-210 – Received State Prerequisite | §7 | Aligned |
| Rules | BR-D-211 – Observation Existence Prerequisite | §7 | Aligned |
| Rules | BR-D-400 – Demand Understanding Derivation Source | §7 | Aligned |
| Policies | PO-D-001 – Demand Data Acceptance Policy | §8 | Aligned |
| Policies | PO-D-011 – Demand Understanding Materiality Policy | §8 | Aligned |
| Policies | PO-D-012 – Demand Understanding Publication Cadence Policy | §8 | Aligned |
| Functional Specifications | FS-D-001 – Receive Demand Observation | §9 | Aligned |
| Functional Specifications | FS-D-002 – Evaluate Demand Observation | §9 | Aligned |
| Functional Specifications | FS-D-003 – Revise Demand Understanding | §9 | Aligned |
| Functional Specifications | FS-D-004 – Publish Demand Understanding | §9 | Aligned |
| Business Algorithms | BA-D-001 – Evaluate Demand Understanding Materiality | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-002, SE-C-003, SE-C-010, SE-C-021, SE-C-022, SE-C-023 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-003 – Forecast Publication | §4.3.1 | Aligned |
| Enterprise Question | “What demand has the enterprise observed, and what does the enterprise currently understand about demand?” | §5.1 | Frozen |
| Enterprise Events | EV-D-001 – Demand Observation Received | §5.1 | Aligned |
| Enterprise Events | EV-D-002 – Demand Observation Evaluated | §5.1 | Aligned |
| Enterprise Events | EV-D-003 – Demand Understanding Revised | §5.1 | Aligned |
| Enterprise Events | EV-D-004 – Demand Understanding Published | §5.1 | Aligned |
| Business Notifications | BN-D-001 – Demand Understanding Published | §5.1 | Aligned |
| Business Notifications | BN-D-002 – Demand Observation Quarantined | §5.1 | Aligned |
| Business Notifications | BN-D-003 – Demand Observation Rejected | §5.1 | Aligned |
| Business Notifications | BN-D-005 – Demand Observation Received | §5.1 | Aligned |
| Business Notifications | BN-D-006 – Demand Observation Accepted | §5.1 | Aligned |

---

## 5.2 Forecast Demand – CA-D-002

**Business Intent:** Maintain the enterprise's authoritative projection of future demand by combining statistical prediction, business knowledge, assumptions, and governance into a trusted, published Forecast Publication that all downstream planning capabilities consume.

**Enterprise Question:** What future demand does the enterprise project, with what confidence, under what assumptions?

**Owned Semantic Objects:** SE-D-003 (Forecast Publication).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002), Demand Behavior Assessments (SE-D-004), Demand Behavior Assignments (SE-D-006), Demand Intervention Impacts (SE-D-018). |
| **Produces** | Forecast Publication (SE-D-003). |
| **Feeds** | Understand Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality. |

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Forecast Demand |
|------------|--------------------------|
| Demand Understanding (SE-D-002) | Provides the authoritative interpretation of current demand — the baseline from which projections are generated. |
| Demand Behavior Assessment (SE-D-004) | Provides current demand behavior state; Critical states may trigger out-of-cycle forecasting. |
| Demand Behavior Assignment (SE-D-006) | Provides behavioral classifications used for forecasting model selection. |
| Demand Intervention Impact (SE-D-018)  | Provides deterministic demand adjustments from planned commercial interventions. Consumed as forward-looking context during forecast generation. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Item (SE-C-001) | Defines the products being forecast. |
| Location (SE-C-002) | Defines the locations. |
| Calendar (SE-C-033) | Defines the planning calendar and time buckets. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Forecast Publication Governance (PO-D-020) | Governs auto-publication thresholds. |
| Forecast Publication Generation Governance (PO-D-024) | Governs forecast publication generation context initiation cadence and out-of-cycle rules. |
| Forecast Assumption Sign-off Policy (PO-D-025) | Governs the approval of forecast assumptions before publication. |
| New Product Forecast Method Policy (PO-D-028) | Governs how new products with insufficient history are forecast. |
| Reconciliation Policy (PO-D-029) | Governs forecast hierarchy reconciliation. |

**Business Guarantees:**

- Exactly one Published Forecast Publication exists for a given Planning Scope and horizon at any moment.
- A Published forecast is immutable. Any change requires a new publication produced by a new forecast cycle.
- All forecast lines within a publication carry model provenance.
- The original system forecast is preserved when an override is applied.
- The Forecast Publication is the single authoritative demand projection for the enterprise.
- A Forecast Publication generation cycle shall consume the latest Published Demand Understanding that exists before forecast generation begins.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-005 | Establish Forecast Cycle | BW-D-005 | FS-D-005 |
| CR-D-006 | Produce Forecast Projection | BW-D-006 | FS-D-006 |
| CR-D-007 | Govern Forecast Projection | BW-D-007 | FS-D-007 |
| CR-D-008 | Publish Forecast Publication | BW-D-008 | FS-D-008 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-010 | Forecast Cycle Established | AB-D-005 |
| EV-D-011 | Forecast Projection Produced | AB-D-007 |
| EV-D-012 | Forecast Override Applied | AB-D-008 |
| EV-D-013 | Forecast Publication Published | AB-D-009 |
| EV-D-016 | Critical Demand Behavior Escalated | AB-D-010 (via FS-D-010) |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-D-010 | EV-D-010 | Forecast Cycle Established: Cycle ID, Reason, Horizon | At-least-once | Per cycle | Near-real-time |
| BN-D-011 | EV-D-013 (via AB-D-009) | Forecast Published: Publication ID, Version, Planning Scope, Horizon, Confidence Index, Champion Model | At-least-once | Per publication | Near-real-time |
| BN-D-012 | EV-D-012 | Forecast Override Applied: Publication ID, Item, Location, Bucket, Original, Override, Planner, Justification | At-least-once | Per override | Near-real-time |
| BN-D-014 | EV-D-013 | Forecast Publication Suppressed: Publication ID, Reason | At-least-once | Per publication | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| BN-D-001 Demand Understanding Published | Understand Demand | Update training data with latest demand interpretation. | (data refresh) |
| BN-D-016 Critical Demand Behavior Requires Action | Sense Demand | Initiate out-of-cycle forecast refresh. | FS-D-007 |
| BN-D-017 Planning Classification Changed | Segment Demand | Update model selection for affected entities. | (model selection update) |
| BN-D-019 Demand Behavior Classification Changed | Classify Demand | Update model selection for affected entities. | (model selection update) |

**Knowledge Handoff:** Forecast Demand transforms the enterprise's current demand understanding into a governed projection of future demand. The Forecast Publication is the single authoritative demand projection that all downstream planning capabilities consume.

**Feedback Target:** Consumes Critical behavior states from Sense Demand; triggers out-of-cycle forecast refresh within the same planning cycle.

**Traceability:** Business Owner: CA-D-002. Publishes EV-D-010–012 and BN-D-010–012. Consumes BN-D-001, BN-D-016, BN-D-017, BN-D-019. Realises BO-D-001, BO-D-002, BO-D-005.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-003 – Forecast Publication | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-005 – Initiate Forecast Cycle | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-006 - Select Champion Model | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-007 – Produce Forecast Projection | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-008 – Govern Forecast Projection | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-009 – Publish Forecast Publication | §4.3.1 | Frozen |
| Decisions | DE-D-003 – Generate Forecast for Series | §6 | Aligned |
| Decisions | DE-D-004 – Approve Forecast Publication | §6 | Aligned |
| Decisions | DE-D-005 – Evaluate Forecast Override | §6 | Aligned |
| Decisions | DE-D-013 – Select Champion Model | §6 | Aligned |
| Rules | BR-D-003 – Forecast Publication Aggregate Identity | §7 | Aligned |
| Rules | BR-D-107 – Single Published Forecast Publication | §7 | Aligned |
| Rules | BR-D-108 – Published Immutability | §7 | Aligned |
| Rules | BR-D-109 – Original System Forecast Preservation | §7 | Aligned |
| Rules | BR-D-206 – Forecast Data Sufficiency | §7 | Aligned |
| Rules | BR-D-207 – Completeness Threshold for Publication | §7 | Aligned |
| Rules | BR-D-208 – Override Justification Requirement | §7 | Aligned |
| Rules | BR-D-209 – Override Deviation Limit | §7 | Aligned |
| Rules | BR-D-401 – Authorised Forecasting Strategy | §7 | Aligned |
| Rules | BR-D-410 – Hierarchical Forecast Consistency | §7 | Aligned |
| Policies | PO-D-017 – Forecast Model Governance Policy | §8 | Aligned |
| Policies | PO-D-019 – Unforecastable Series Policy | §8 | Aligned |
| Policies | PO-D-020 – Forecast Publication Governance Policy | §8 | Aligned |
| Policies | PO-D-021 – Demand Manager Override Policy | §8 | Aligned |
| Policies | PO-D-022 – Forecast Override Authorization Policy | §8 | Aligned |
| Policies | PO-D-023 – Override Audit Policy | §8 | Aligned |
| Policies | PO-D-024 – Forecast Cycle Governance Policy | §8 | Aligned |
| Policies | PO-D-025 – Forecast Assumption Sign-off Policy | §8 | Aligned |
| Policies | PO-D-028 – New Product Forecast Policy | §8 | Aligned |
| Policies | PO-D-029 – Forecast Reconciliation Policy | §8 | Aligned |
| Functional Specifications | FS-D-005 – Establish Forecast Cycle | §9 | Aligned |
| Functional Specifications | FS-D-006 – Produce Forecast Projection | §9 | Aligned |
| Functional Specifications | FS-D-007 – Govern Forecast Projection | §9 | Aligned |
| Functional Specifications | FS-D-008 – Publish Forecast Publication | §9 | Aligned |
| Business Algorithms | BA-D-002 – Produce Forecast Projection | §10 | Aligned |
| Business Algorithms | BA-D-015 – Reconcile Forecast Hierarchy | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-002, SE-C-033 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-002 – Demand Understanding | §4.3.1 | Aligned |
| Consumed Domain Objects | SE-D-004 – Demand Behavior Assessment | §4.3.1 | Aligned |
| Consumed Domain Objects | SE-D-006 – Demand Behavior Assignment | §4.3.1 | Aligned |
| Enterprise Question | “What future demand does the enterprise project, with what confidence, under what assumptions?” | §5.2 | Frozen |
| Enterprise Events | EV-D-010 – Forecast Cycle Established | §5.2 | Aligned |
| Enterprise Events | EV-D-011 – Forecast Projection Produced | §5.2 | Aligned |
| Enterprise Events | EV-D-012 – Forecast Override Applied | §5.2 | Aligned |
| Business Notifications | BN-D-010 – Forecast Cycle Established | §5.2 | Aligned |
| Business Notifications | BN-D-011 – Forecast Published | §5.2 | Aligned |
| Business Notifications | BN-D-012 – Forecast Override Applied | §5.2 | Aligned |

---

## 5.3 Sense Demand – CA-D-003

**Business Intent:** Continuously maintain the enterprise's understanding of current demand behavior for every monitored item-location, detecting meaningful changes from expected patterns and providing real-time situational awareness that triggers downstream planning actions.

**Enterprise Question:** What has changed in demand behavior that the enterprise now understands to be true?

**Owned Semantic Objects:** SE-D-004 (Demand Behavior Assessment).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002). |
| **Produces** | Demand Behavior Assessment (SE-D-004). |
| **Feeds** | Forecast Demand (Critical states trigger out-of-cycle forecasting), Segment Demand, Classify Demand, Detect Demand Exceptions. |

**Enterprise Dependencies**

**Enterprise Understanding** (consumed from other capabilities)

| Dependency | Role in Sense Demand |
|------------|----------------------|
| Demand Understanding (SE-D-002) | Provides the current demand interpretation against which incoming signals are evaluated. The expected behavior described in the Demand Understanding serves as the baseline for change detection. |

**Enterprise Master Data**

| Dependency | Role |
|------------|------|
| Item (SE-C-001) | Defines the products monitored. |
| Location (SE-C-002) | Defines the locations monitored. |

**Enterprise Governance**

| Dependency | Role |
|------------|------|
| Demand Sensing Policy (PO-D-031) | Governs deviation thresholds, corroboration requirements, and state transition rules. |
| Forecast Refresh Trigger Policy (PO-D-032) | Governs when Critical states trigger automatic forecast refresh evaluation. |
| Forecast Refresh Execution Policy (PO-D-034) | Governs partial vs full refresh authorization. |

**Business Guarantees:**
- Every monitored item-location has a continuously maintained Current State reflecting the latest demand behavior.
- A state change is published within the enterprise detection latency target.
- Critical state changes automatically trigger evaluation for an out-of-cycle forecast refresh.
- The complete history of state changes is permanently preserved.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-009 | Maintain Demand Behavior Understanding | BW-D-009 | FS-D-009 |
| CR-D-010 | Escalate Critical Demand Behavior | BW-D-010 | FS-D-010 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-015 | Demand Behavior Understanding Maintained | AB-D-010 |
| EV-D-016 | Critical Demand Behavior Escalated | AB-D-010 (via FS-D-015) |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-D-015 | EV-D-015 | Demand Behavior Changed: Item, Location, Previous State, New State, Deviation, Confidence | At-least-once | Per assessment | Near-real-time |
| BN-D-016 | EV-D-016 | Critical Demand Behavior Requires Action: same as BN-D-015 plus Recommended Action | At-least-once | Per assessment | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| BN-D-001 Demand Understanding Published | Understand Demand | Refresh the evaluation baseline with latest demand interpretation. | (baseline refresh) |
| BN-D-011 Forecast Published | Forecast Demand | Optionally update evaluation context with latest forecast. | (baseline refresh) |

**Knowledge Handoff:** Sense Demand maintains the enterprise's continuously current understanding of demand behavior. It is the bridge between periodic forecasting and real-time awareness. Critical state changes flow forward to Forecast Demand for out-of-cycle refresh and to Detect Demand Exceptions for attention.

**Feedback Target:** Critical behavior states feed into Forecast Demand, potentially triggering an immediate forecast refresh within the same planning cycle. Behavior patterns also feed into Classify Demand for model selection refinement.

**Traceability:** Business Owner: CA-D-003. Publishes BN-D-015–016. Consumes BN-D-001 and BN-D-011. Realises BO-D-001, BO-D-003.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-004 – Demand Behavior Assessment | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-010 – Maintain Demand Behavior Understanding | §4.3.1 | Frozen |
| Decisions | DE-D-006 – Evaluate Demand Signal for State Change | §6 | Aligned |
| Decisions | DE-D-007 – Trigger Forecast Refresh on Critical State | §6 | Aligned |
| Rules | BR-D-004 – Demand Behavior Assessment Identity | §7 | Aligned |
| Rules | BR-D-110 – Single Current State | §7 | Aligned |
| Rules | BR-D-111 – Immutable State Change Events | §7 | Aligned |
| Rules | BR-D-300 – Deviation Thresholds | §7 | Aligned |
| Rules | BR-D-301 – Corroboration Requirement | §7 | Aligned |
| Rules | BR-D-302 – High-priority Sensitivity | §7 | Aligned |
| Rules | BR-D-303 – Noise Suppression | §7 | Aligned |
| Rules | BR-D-304 – Forecast Refresh Evaluation on Critical State | §7 | Aligned |
| Policies | PO-D-031 – Demand Sensing Policy | §8 | Aligned |
| Policies | PO-D-032 – Forecast Refresh Trigger Policy | §8 | Aligned |
| Policies | PO-D-034 – Forecast Refresh Execution Policy | §8 | Aligned |
| Functional Specifications | FS-D-009 – Maintain Demand Behavior Understanding | §9 | Aligned |
| Functional Specifications | FS-D-010 – Escalate Critical Demand Behavior | §9 | Aligned |
| Business Algorithms | BA-D-003 – Assess Demand Signal Deviation | §10 | Aligned |
| Business Algorithms | BA-D-004 – Determine Demand Behavior State | §10 | Aligned |
| Business Algorithms | BA-D-014 – Derive Demand Behavior Baseline | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-002 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-002 – Demand Understanding | §4.3.1 | Aligned |
| Enterprise Question | “What has changed in demand behavior that the enterprise now understands to be true?” | §5.3 | Frozen |
| Enterprise Events | EV-D-015 – Demand Behavior Understanding Maintained | §5.3 | Aligned |
| Enterprise Events | EV-D-016 – Critical Demand Behavior Escalated | §5.3 | Aligned |
| Business Notifications | BN-D-015 – Demand Behavior Changed | §5.3 | Aligned |
| Business Notifications | BN-D-016 – Critical Demand Behavior Requires Action | §5.3 | Aligned |

---

## 5.4 Segment Demand – CA-D-004

**Business Intent:** Maintain the enterprise's continuously current planning classifications for every planning entity, enabling all downstream capabilities to vary their behavior based on the characteristics of each entity.

**Enterprise Question:** How should the enterprise segment demand entities to enable differentiated planning strategies?

**Owned Semantic Objects:** SE-D-005 (Planning Classification Assignment).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002), Demand Behavior Assessments (SE-D-004), Segmentation Policy. |
| **Produces** | Planning Classification Assignments (SE-D-005). |
| **Feeds** | Forecast Demand (model selection), Classify Demand (context), Prioritize Demand, Inventory Planning (external), Supply Intelligence. |

**Enterprise Dependencies** *(abbreviated for brevity; follows the same pattern)*

**Business Guarantees:**
- Every active planning entity has a continuously maintained current classification for each active classification type.
- Each classification type is updated independently.
- Classification history is permanently preserved.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-011 | Classify Planning Entity | BW-D-011 | FS-D-011 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-017 | Planning Classification Changed | AB-D-011 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-017 | Planning Classification Changed: Entity Type, Entity Identifier, Classification Type, Previous, New, Reason, Confidence | At-least-once | Per assignment | Near-real-time |

**Feedback Target:** No feedback target — output is consumed forward in the current planning cycle.

**Traceability:** Business Owner: CA-D-004. Publishes BN-D-017. Realises BO-D-001, BO-D-002.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-005 – Planning Classification Assignment | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-011 – Classify Planning Entity | §4.3.1 | Frozen |
| Decisions | DE-D-008 – Determine Planning Classification | §6 | Aligned |
| Rules | BR-D-005 – Planning Classification Assignment Identity | §7 | Aligned |
| Rules | BR-D-112 – Single Current Classification per Type | §7 | Aligned |
| Rules | BR-D-113 – Immutable Assignment Change Events | §7 | Aligned |
| Rules | BR-D-305 – Classification by Policy | §7 | Aligned |
| Rules | BR-D-306 – Unclassified Assignment | §7 | Aligned |
| Policies | PO-D-035 – Segmentation Policy Governance | §8 | Aligned |
| Policies | PO-D-036 – Segmentation Override Review Policy | §8 | Aligned |
| Functional Specifications | FS-D-011 – Classify Planning Entity | §9 | Aligned |
| Business Algorithms | BA-D-005 – Compute Planning Classification | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-003 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-002 – Demand Understanding | §4.3.1 | Aligned |
| Enterprise Question | “How should the enterprise segment demand entities to enable differentiated planning strategies?” | §5.4 | Frozen |
| Business Notifications | BN-D-017 – Planning Classification Changed | §5.4 | Aligned |

---

## 5.5 Classify Demand – CA-D-005

**Business Intent:** Maintain the enterprise's authoritative behavioral classifications for every planning entity, enabling downstream capabilities to select appropriate forecasting models, set detection thresholds, and focus planner attention based on how demand actually behaves.

**Enterprise Question:** What behavior does this demand exhibit, and what does that behavior mean for forecasting model selection?

**Owned Semantic Objects:** SE-D-006 (Demand Behavior Assignment).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002), Demand Behavior Assessments (SE-D-004), Classification Policy. |
| **Produces** | Demand Behavior Assignments (SE-D-006). |
| **Feeds** | Forecast Demand (model selection), Detect Demand Exceptions (threshold setting), Explain Demand, Prioritize Demand, Inventory Planning, Supply Intelligence, Scenario Intelligence. |

**Business Guarantees:**
- Every active planning entity has a continuously maintained current classification for each active behavior dimension.
- Each behavior dimension is updated independently.
- Every classification includes confidence and an evidence summary.
- Classification history is permanently preserved.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-012 | Classify Demand Behavior | BW-D-012 | FS-D-012 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-019 | Demand Behavior Classification Changed | AB-D-012 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-019 | Demand Behavior Classification Changed: Entity Type, Entity Identifier, Behavior Dimension, Previous, New, Confidence, Evidence Summary | At-least-once | Per assignment | Near-real-time |

**Feedback Target:** No feedback target — output is consumed forward in the current planning cycle.

**Traceability:** Business Owner: CA-D-005. Publishes BN-D-019. Realises BO-D-001, BO-D-002.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-006 – Demand Behavior Assignment | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-012 – Classify Demand Behavior | §4.3.1 | Frozen |
| Decisions | DE-D-009 – Determine Behavior Classification | §6 | Aligned |
| Rules | BR-D-006 – Demand Behavior Assignment Identity | §7 | Aligned |
| Rules | BR-D-114 – Single Current Classification per Dimension | §7 | Aligned |
| Rules | BR-D-115 – Immutable Behavior Change Events | §7 | Aligned |
| Rules | BR-D-307 – Classification by Policy | §7 | Aligned |
| Rules | BR-D-308 – Unclassified Assignment | §7 | Aligned |
| Policies | PO-D-037 – Classification Policy Governance | §8 | Aligned |
| Policies | PO-D-038 – Classification Override Review Policy | §8 | Aligned |
| Functional Specifications | FS-D-012 – Classify Demand Behavior | §9 | Aligned |
| Business Algorithms | BA-D-006 – Determine Behavior Classification | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-002 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-002 – Demand Understanding | §4.3.1 | Aligned |
| Enterprise Question | “What behavior does this demand exhibit, and what does that behavior mean for forecasting model selection?” | §5.5 | Frozen |
| Business Notifications | BN-D-019 – Demand Behavior Classification Changed | §5.5 | Aligned |

---

## 5.6 Prioritize Demand – CA-D-006

**Business Intent:** Maintain the enterprise's continuously current assessment of planning importance for every planning entity, directing planner attention, exception handling, and allocation decisions to the most impactful items.

**Enterprise Question:** Which demand entities are most important to the enterprise's objectives, and why?

**Owned Semantic Objects:** SE-D-007 (Planning Priority Assignment).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002), Planning Classification Assignments (SE-D-005), Demand Behavior Assignments (SE-D-006), Prioritization Policy. |
| **Produces** | Planning Priority Assignments (SE-D-007). |
| **Feeds** | Forecast Demand (high-priority protection rules), Detect Demand Exceptions (alert prioritisation), Inventory Planning (allocation priority), Scenario Intelligence (impact assessment). |

**Business Guarantees:**
- Every active planning entity has a continuously maintained current priority.
- Priority is derived from the current Prioritization Policy and available business evidence.
- Every priority assignment includes a business-language decision rationale and business validity.
- Priority history is permanently preserved.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-013 | Prioritize Planning Entity | BW-D-013 | FS-D-013 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-020 | Planning Priority Changed | AB-D-013 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-020 | Planning Priority Changed: Entity Type, Entity Identifier, Previous Priority, New Priority, Decision Rationale, Business Validity | At-least-once | Per assignment | Near-real-time |

**Feedback Target:** No feedback target — output is consumed forward in the current planning cycle.

**Traceability:** Business Owner: CA-D-006. Publishes BN-D-020. Realises BO-D-002, BO-D-004.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-007 – Planning Priority Assignment | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-013 – Prioritize Planning Entity | §4.3.1 | Frozen |
| Decisions | DE-D-010 – Determine Planning Priority | §6 | Aligned |
| Rules | BR-D-007 – Planning Priority Assignment Identity | §7 | Aligned |
| Rules | BR-D-116 – Single Current Priority | §7 | Aligned |
| Rules | BR-D-117 – Immutable Priority Change Events | §7 | Aligned |
| Rules | BR-D-309 – Priority by Policy | §7 | Aligned |
| Rules | BR-D-310 – Unclassified Priority | §7 | Aligned |
| Policies | PO-D-039 – Prioritization Policy Governance | §8 | Aligned |
| Policies | PO-D-040 – Priority Override Review Policy | §8 | Aligned |
| Functional Specifications | FS-D-013 – Prioritize Planning Entity | §9 | Aligned |
| Business Algorithms | BA-D-007 – Compute Planning Priority Score | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-001, SE-C-003 | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-002, SE-D-005, SE-D-006 | §4.3.1 | Aligned |
| Enterprise Question | “Which demand entities are most important to the enterprise’s objectives, and why?” | §5.6 | Frozen |
| Business Notifications | BN-D-020 – Planning Priority Changed | §5.6 | Aligned |

---

## 5.7 Evaluate Demand Quality – CA-D-007

**Business Intent:** Publish authoritative, periodic enterprise assessments of forecast quality, enabling the enterprise to measure and continuously improve forecasting performance.

**Enterprise Question:** How accurate, stable, and valuable is the enterprise's demand forecasting capability?

**Owned Semantic Objects:** SE-D-008 (Forecast Quality Assessment).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Forecast Publications (SE-D-003), actual demand data from the Enterprise Picture (SE-C-021), planner override records, Forecast Measurement Policy (PO-D-041). |
| **Produces** | Forecast Quality Assessment (SE-D-008). |
| **Feeds** | Learn From Demand, Explain Demand, Forecast Demand (model performance feedback), Demand Planners and Managers. Consumption is for the current evaluation period; outputs feed learning in the next cycle. |

**Enterprise Dependencies** *(abbreviated)*

**Business Guarantees:**
- A Forecast Quality Assessment is published for each Planning Scope and Evaluation Period.
- Every published assessment is immutable and permanently retained.
- Source references and policy version are recorded for full traceability.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-014 | Evaluate Forecast Quality | BW-D-014 | FS-D-014 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-021 | Forecast Quality Assessment Published: Assessment Identifier, Planning Scope, Evaluation Period, Version, Key Metrics Summary | At-least-once | Per assessment | Batch |

**Feedback Target:** The Forecast Quality Assessment is consumed by Learn From Demand to improve forecasting models and policies in **subsequent planning cycles**.

**Traceability:** Business Owner: CA-D-007. Publishes BN-D-021. Realises BO-D-001, BO-D-002, BO-D-006.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-008 – Forecast Quality Assessment | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-014 – Evaluate Forecast Quality | §4.3.1 | Frozen |
| Decisions | DE-D-011 – Publish Forecast Quality Assessment | §6 | Aligned |
| Rules | BR-D-008 – Forecast Quality Assessment Identity | §7 | Aligned |
| Rules | BR-D-118 – Single Published Assessment | §7 | Aligned |
| Rules | BR-D-119 – Published Immutability | §7 | Aligned |
| Rules | BR-D-120 – Permanent Retention | §7 | Aligned |
| Rules | BR-D-403 – Quality Metrics Derivation | §7 | Aligned |
| Policies | PO-D-041 – Forecast Measurement Policy | §8 | Aligned |
| Functional Specifications | FS-D-014 – Evaluate Forecast Quality | §9 | Aligned |
| Business Algorithms | BA-D-008 – Compute Forecast Quality Metrics | §10 | Aligned |
| Business Algorithms | BA-D-009 – Determine Forecast Quality State | §10 | Aligned |
| Consumed Enterprise Objects | SE-C-021 – Enterprise Picture | §4.2 | Frozen |
| Consumed Domain Objects | SE-D-003 – Forecast Publication | §4.3.1 | Aligned |
| Enterprise Question | “How accurate, stable, and valuable is the enterprise’s demand forecasting capability?” | §5.7 | Frozen |
| Business Notifications | BN-D-021 – Forecast Quality Assessment Published | §5.7 | Aligned |

---

## 5.8 Detect Demand Exceptions – CA-D-008

**Business Intent:** Continuously monitor the enterprise demand picture to recognize demand exception evidence and publish it to Core Exception Management.

**Enterprise Question:** What situations in the demand picture require enterprise attention because they violate governed policies, given the current Demand Understanding, Forecast Quality Assessment, and Demand Behavior patterns?

**Owned Semantic Objects:** None. This capability produces exception detection evidence published to Core Exception Management. The SE-C-019 Exception aggregate is owned by the Core Domain.

> **Architectural Note:** CA-D-008 owns no Aggregate Roots. Therefore it has no Aggregate Behaviors. The Functional Specification (FS-D-015) orchestrates Decisions and Business Algorithms directly. This is a justified exception to the standard AB-orchestration pattern because no aggregate exists to protect.

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Demand Understanding (SE-D-002), Forecast Publications (SE-D-003), Demand Behavior Assessments (SE-D-004), Forecast Quality Assessments (SE-D-008), Exception Detection Policy (PO-D-044). |
| **Produces** | Demand exception detection and resolution evidence (published as BN-D-022, BN-D-023 to Core Exception Management). |
| **Feeds** | Core Exception Management (CA-C-020), Explain Demand, Learn From Demand, Planners and Managers. |

**Cross-Domain Dependencies**

| Dependency | Role in Detect Demand Exceptions |
|------------|----------------------------------|
| Core Exception Management (CA-C-020) | Consumes demand exception detection and resolution evidence published by this capability. Creates, updates, and resolves SE-C-019 Exception instances. |

**Business Guarantees:**
- Every demand exception evidence that meets detection thresholds is published to Core Exception Management.
- Detection evidence includes the constraint reference, affected scope, severity assessment, and triggering evidence.
- Resolution evidence is published when the underlying data returns to within governed thresholds.
- All detection and resolution evidence is permanently preserved.
- Demand Intelligence never directly creates, updates, or resolves SE-C-019 Exception instances.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-015 | Detect Demand Exception Evidence | BW-D-015 | FS-D-015 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-022 | Demand Exception Evidence Evaluated | FS-D-015 (workflow-level event) |

> **Architectural Note:** EV-D-022 is a workflow-level event emitted by FS-D-015. Since CA-D-008 owns no aggregate, no Aggregate Behavior exists to emit this event. The workflow emits it after DE-D-012 completes evaluation. This is a justified exception to satisfy ARS §14 (BN must reference at least one EV).

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-D-022 | EV-D-022 | Demand Exception Detection Evidence: Constraint Reference, Affected Scope Type, Affected Scope Identifier, Exception Classification, Severity Assessment, Triggering Evidence | At-least-once | Per detection | Near-real-time |
| BN-D-023 | EV-D-022 | Demand Exception Resolution Evidence: Constraint Reference, Affected Scope Type, Affected Scope Identifier, Resolution Evidence | At-least-once | Per resolution | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| BN-D-001 Demand Understanding Published | Understand Demand | Refresh detection context with latest demand interpretation. | FS-D-015 |
| BN-D-021 Forecast Quality Assessment Published | Evaluate Demand Quality | Refresh detection context with latest quality metrics. | FS-D-015 |

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Decision | DE-D-012 – Evaluate Demand Exception Evidence | §6 | Aligned |
| Rule | BR-D-009 – Demand Exception Evidence Business Identity | §7 | Aligned |
| Rule | BR-D-311 – Condition Detection Evidence | §7 | Aligned |
| Policy | PO-D-044 – Exception Detection Policy Governance | §8 | Aligned |
| Functional Specification | FS-D-015 – Detect Demand Exception Evidence | §9 | Aligned |
| Business Algorithm | BA-D-010 – Evaluate Demand Exception Evidence | §10 | Aligned |
| Business Algorithm | BA-D-011 – Assess Demand Exception Lifecycle Evidence | §10 | Aligned |
| Enterprise Event | EV-D-022 – Demand Exception Evidence Evaluated | §5.8 | Aligned |
| Business Notification | BN-D-022 – Demand Exception Detection Evidence | §5.8 | Aligned |
| Business Notification | BN-D-023 – Demand Exception Resolution Evidence | §5.8 | Aligned |

---

## 5.9 Explain Demand – CA-D-009

**Business Intent:** Record immutable, traceable, deterministic enterprise explanations of the reasoning behind demand intelligence outputs, enabling planners, auditors, and AI systems to understand why forecasts, decisions, conditions, classifications, and priorities exist as they do.

**Enterprise Question:** Why did the enterprise reach, or deliberately not reach, this demand conclusion, and what evidence supports it?

**Owned Semantic Objects:** SE-D-010 (Demand Explanation).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Any demand intelligence artifact that carries preserved evidence (Forecast Publications, Demand Planning Conditions, Planning Classifications, Planning Priorities, Forecast Quality Assessments). |
| **Produces** | Demand Explanations (SE-D-010). |
| **Feeds** | Learn From Demand, Planners and Auditors, AI Copilot. Explanations are immutable records; they feed learning and governance asynchronously. |

**Enterprise Dependencies** *(abbreviated)*

**Business Guarantees:**
- Every recorded explanation contains a Structured Reasoning Graph — the canonical, deterministic enterprise representation of the reasoning.
- The reasoning graph carries provenance on every node and references historical versions of all contributing artifacts.
- Explanations are immutable once created and permanently retained.
- Identical reasoning requests return the existing explanation.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-016 | Establish Demand Explanation | BW-D-016 | FS-D-016 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-024 | Demand Explanation Established: Explanation Identifier, Explained Artifact Type, Explained Artifact Identifier | At-least-once | Per explanation | On-demand or near-real-time |

**Feedback Target:** No feedback target — output is consumed asynchronously by learning and governance.

**Traceability:** Business Owner: CA-D-009. Publishes BN-D-024. Realises BO-D-001, BO-D-006.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-010 – Demand Explanation | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-016 – Establish Demand Explanation | §4.3.1 | Frozen |
| Rules | BR-D-010 – Demand Explanation Identity | §7 | Aligned |
| Rules | BR-D-124 – Explanation Immutability | §7 | Aligned |
| Policies | PO-D-047 – Explanation Governance | §8 | Aligned |
| Functional Specifications | FS-D-016 – Establish Demand Explanation | §9 | Aligned |
| Business Algorithms | BA-D-012 – Compose Demand Explanation | §10 | Aligned |
| Consumed Domain Objects | All Demand Intelligence artifacts (preserved evidence) | §4.3.1 | Aligned |
| Enterprise Question | “Why did the enterprise reach, or deliberately not reach, this demand conclusion…?” | §5.9 | Frozen |
| Business Notifications | BN-D-024 – Demand Explanation Established | §5.9 | Aligned |

---

## 5.10 Learn From Demand – CA-D-010

**Business Intent:** Continuously discover and record what the enterprise has concluded about the performance and behavior of its demand intelligence capabilities, by systematically analysing outcomes, patterns, and evidence across the entire domain.

**Enterprise Question:** What has the enterprise learned about demand behavior and forecasting performance that should improve future planning?

**Owned Semantic Objects:** SE-D-011 (Demand Learning).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Historical evidence from all Demand Intelligence capabilities — Forecast Quality Assessments (SE-D-008), Demand Planning Conditions (SE-D-009), Demand Explanations (SE-D-010), Planning Classifications (SE-D-005), Demand Behavior Assignments (SE-D-006), Planning Priorities (SE-D-007), Demand Behavior Assessments (SE-D-004). |
| **Produces** | Demand Learnings (SE-D-011). |
| **Feeds** | Planning Governance, Forecast Demand, Segment Demand, Classify Demand, Prioritize Demand, Future Planning Cycles. All consumption is for **subsequent planning cycles** (cycle N+1 and beyond). |

**Enterprise Dependencies** *(abbreviated)*

**Business Guarantees:**
- Every recorded learning is supported by evidence from at least one completed analysis or evaluation.
- Learnings are immutable once recorded and permanently retained.
- Each learning states what was discovered and how strongly the evidence supports it.
- The Learning Type taxonomy and Evidence Strength criteria are governed by the Learning Analysis Policy.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-017 | Establish Demand Learning | BW-D-017 | FS-D-017 |

### Business Notifications Published

| ID | Business Information | Delivery | Ordering | Timeliness |
|----|---------------------|----------|----------|------------|
| BN-D-025 | Demand Learning Established: Learning Identifier, Learning Type, Learning Statement Summary, Evidence Strength | At-least-once | Per learning | Batch (post-analysis) |

**Feedback Target:** This is the ultimate feedback capability. Its entire output is directed at improving future planning. Learnings flow back to Planning Governance and all upstream planning capabilities via policy and parameter updates that take effect in the **next planning cycle**.

**Traceability:** Business Owner: CA-D-010. Publishes BN-D-025. Realises BO-D-006.

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Objects | SE-D-011 – Demand Learning | §4.3.1 | Frozen |
| Aggregate Behaviors | AB-D-017 – Establish Demand Learning | §4.3.1 | Frozen |
| Rules | BR-D-011 – Demand Learning Identity | §7 | Aligned |
| Rules | BR-D-125 – Learning Immutability | §7 | Aligned |
| Functional Specifications | FS-D-017 – Establish Demand Learning | §9 | Aligned |
| Business Algorithms | BA-D-013 – Derive Demand Learning | §10 | Aligned |
| Consumed Domain Objects | All Demand Intelligence historical evidence | §4.3.1 | Aligned |
| Enterprise Question | “What has the enterprise learned about demand behavior and forecasting performance…?” | §5.10 | Frozen |
| Business Notifications | BN-D-025 – Demand Learning Established | §5.10 | Aligned |

---

## 5.11 Model Demand Interventions – CA-D-011

**Business Intent:** Assess the demand impact of planned commercial interventions and publish deterministic impact assessments for consumption by the forecast capability.

**Enterprise Question:** What is the expected demand change from this planned commercial intervention?

**Owned Semantic Objects:** SE-D-018 (Demand Intervention Impact).

**Position in Enterprise Reasoning**

| Role | Description |
|------|-------------|
| **Consumes** | Scenario Adjustment (SE-C-039), Demand Understanding (SE-D-002), Forecast Publication (SE-D-003) for historical elasticity context. |
| **Produces** | Demand Intervention Impact (SE-D-018). |
| **Feeds** | Forecast Demand (CA-D-002), Supply Intelligence, Scenario Intelligence. |

**Enterprise Dependencies**

| Dependency | Role in Model Demand Interventions |
|------------|--------------------------------------|
| SE-C-039 Scenario Adjustment | Defines the intervention being assessed (type, magnitude, temporal scope). |
| SE-D-002 Demand Understanding | Provides current demand baseline context. |
| SE-D-003 Forecast Publication | Provides historical forecast-actual pairs for elasticity estimation. |
| PO-D-050 Intervention Modeling Governance | Governs modeling approach selection, confidence thresholds, and publication criteria. |

**Business Guarantees:**
1. Every published Demand Intervention Impact is immutable and traceable to its source intervention.
2. Every impact assessment carries a confidence level and model provenance.
3. Impact assessments are computed deterministically from governed inputs.
4. No impact assessment modifies the baseline forecast directly.
5. Exactly one Published version exists per intervention per item-location at any moment.

### Capability Responsibilities

| ID | Responsibility | Business Workflow | FS |
|----|----------------|-------------------|-----|
| CR-D-018 | Assess Demand Intervention Impact | BW-D-018 | FS-D-018 |
| CR-D-019 | Publish Demand Intervention Impact | BW-D-019 | FS-D-019 |

### Enterprise Events Published

| ID | Business Fact | Published By |
|----|---------------|--------------|
| EV-D-023 | Demand Intervention Impact Published | AB-D-019 |

### Business Notifications Published

| ID | References EV | Business Information | Delivery | Ordering | Timeliness |
|----|---------------|---------------------|----------|----------|------------|
| BN-D-026 | EV-D-023 | Demand Intervention Impact Published: Intervention Impact Identifier, Intervention Reference, Item, Location, Assessed Demand Lift, Lift Confidence, Temporal Validity | At-least-once | Per intervention | Near-real-time |

### Business Notifications Consumed

| Source Notification | Publisher | Business Behavior | Invokes |
|---------------------|-----------|-------------------|---------|
| BN-D-001 Demand Understanding Published | Understand Demand | Refresh baseline context for pending impact assessments. | FS-D-018 |

### Capability Artifact Indexes

| Artifact Type | ID | Location | Status |
|---------------|----|----------|--------|
| Owned Semantic Object | SE-D-018 – Demand Intervention Impact | §4.3.1 | Frozen |
| Aggregate Behavior | AB-D-018 – Assess Demand Intervention Impact | §5.11 | Aligned |
| Aggregate Behavior | AB-D-019 – Publish Demand Intervention Impact | §5.11 | Aligned |
| Decision | DE-D-014 – Approve Intervention Impact Publication | §6 | Aligned |
| Rule | BR-D-414 – Intervention Impact Non-Negativity | §7 | Aligned |
| Rule | BR-D-415 – Intervention Reference Validity | §7 | Aligned |
| Policy | PO-D-050 – Intervention Modeling Governance | §8 | Aligned |
| Functional Specification | FS-D-018 – Assess Demand Intervention Impact | §9 | Aligned |
| Functional Specification | FS-D-019 – Publish Demand Intervention Impact | §9 | Aligned |
| Business Algorithm | BA-D-016 – Model Intervention Lift | §10 | Aligned |
| Enterprise Event | EV-D-023 – Demand Intervention Impact Published | §5.11 | Aligned |
| Business Notification | BN-D-026 – Demand Intervention Impact Published | §5.11 | Aligned |

## 5.12 Business Workflows

| ID | Business Intent | Realises | FS | Trigger |
|----|-----------------|----------|----|---------|
| BW-D-001 | Ensure every received demand signal becomes one traceable enterprise record. | CR-D-001 | FS-D-001 | Demand signal received |
| BW-D-002 | Ensure every received demand observation receives one governed evaluation outcome. | CR-D-002 | FS-D-002 | EV-D-001 |
| BW-D-003 | Ensure the latest Enterprise Picture is interpreted into a current draft Demand Understanding. | CR-D-003 | FS-D-003 | Enterprise Picture Published |
| BW-D-004 | Ensure only materially changed and complete Demand Understandings become authoritative. | CR-D-004 | FS-D-004 | Draft Demand Understanding available |
| BW-D-005 | Initiate a new forecast cycle on the governed cadence or in response to critical demand changes. | CR-D-005 | FS-D-005 | Schedule or Critical Behavior notification |
| BW-D-006 | Produce the enterprise's authoritative forecast projection for all covered series. | CR-D-006 | FS-D-006 | Cycle Established |
| BW-D-007 | Govern the forecast projection through planner overrides with full traceability. | CR-D-007 | FS-D-007 | Planner submits override |
| BW-D-008 | Establish the Forecast Publication as the authoritative demand projection. | CR-D-008 | FS-D-008 | Forecast projection produced and governed |
| BW-D-009 | Maintain continuously current demand behavior understanding for every monitored item-location. | CR-D-009 | FS-D-009 | Incoming demand signal |
| BW-D-010 | Escalate Critical demand behavior to trigger out-of-cycle forecast refresh. | CR-D-010 | FS-D-010 | Critical state detected |
| BW-D-011 | Maintain continuously current planning classifications. | CR-D-011 | FS-D-011 | Schedule or policy change |
| BW-D-012 | Maintain continuously current demand behavior classifications. | CR-D-012 | FS-D-012 | Schedule or policy change |
| BW-D-013 | Maintain continuously current planning priorities. | CR-D-013 | FS-D-013 | Schedule or upstream change |
| BW-D-014 | Publish periodic authoritative forecast quality assessment. | CR-D-014 | FS-D-014 | Schedule |
| BW-D-015 | Detect, update, and resolve demand planning conditions. | CR-D-015 | FS-D-015 | Schedule or upstream change |
| BW-D-016 | Establish immutable demand explanation from preserved enterprise knowledge. | CR-D-016 | FS-D-016 | On-demand or automatic |
| BW-D-017 | Discover recurring demand patterns and establish immutable enterprise learnings. | CR-D-017 | FS-D-017 | Schedule or sufficient new evidence |

---

# Chapter 6 — Decision Model

## Domain-Wide Decision Conventions

### Outcome Type Taxonomy

| Outcome Type | Definition |
|--------------|------------|
| Acceptance Decision | The enterprise determines whether an artifact meets governed eligibility criteria. |
| Publication Decision | The enterprise determines whether an artifact should become authoritative. |
| Classification Decision | The enterprise assigns a governed classification to an entity or assessment. |
| Selection Decision | The enterprise chooses a strategy, source, or method from governed alternatives. |
| Authorization Decision | The enterprise approves or rejects a recommendation for further action. |
| Escalation Decision | The enterprise defers the decision to a human authority. |

### Decision Confidence Types

| Confidence Type | Meaning |
|-----------------|---------|
| Evidence Confidence | Confidence derived from the quality, completeness, and reliability of the evidence consumed. |
| Prediction Confidence | Confidence inherited from a predictive model whose output the decision consumes. |
| Classification Confidence | Confidence in a governed classification assignment based on evidence fit. |
| Rule Certainty | The decision outcome is deterministic given the governing rules; no subjective confidence applies. |
| Decision Certainty | Confidence assessed by a human decision-maker when the decision is escalated or overridden. |

### Decision Authority Outcomes (Escalation Model)

| Term | Meaning |
|------|---------|
| Automatic | The decision is made by the system without human intervention. |
| Escalated to [Role] | The decision authority transfers to a named human role. |
| Deferred | The decision is postponed with a reason; it may be re-evaluated later. |
| Inconclusive | The evidence is insufficient; the decision is flagged for re-evaluation. |

---

## DE-D-001 – Accept Demand Observation

**Outcome Type:** Acceptance Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Understand Demand (CA-D-001) |
| **Authoritative Representation** | The enterprise’s determination of whether a received demand observation is trustworthy enough to contribute to the Enterprise Picture. |
| **Business Responsibility** | Evaluate incoming demand observations against governed eligibility criteria and assign an acceptance outcome. |
| **Authority Scope** | Per Demand Observation. |
| **Intended Consumers** | The Enterprise Picture, and through it, the Demand Understanding. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Validate incoming demand observations and determine whether they are trustworthy enough to contribute to the enterprise’s understanding of demand.

### Enterprise Question

Is this demand observation trustworthy, given its timeliness, validity, source reliability, and uniqueness?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Demand Observation is in Received state. It has not been previously evaluated. All mandatory attributes are present. |
| **Business Behavior** | Evaluate the observation against the eligibility rules BR-D-200, BR-D-201, BR-D-202, and BR-D-203. If all rules pass, assign Accept. If one or more rules fail critically, assign Reject. If marginal or borderline, assign Quarantine. Record the decision outcome, confidence, and rationale. |
| **Exceptional Conditions** | If the source reliability is borderline, Quarantine with reason code. If multiple rules conflict, the most severe applicable outcome is selected: Reject overrides Quarantine, Quarantine overrides Accept. |
| **Postconditions** | The observation transitions to Accepted, Quarantined, or Rejected. Decision traceability is recorded. |
| **Outcome When Preconditions Are Not Satisfied** | If the observation is not in Received state, no evaluation is performed. If mandatory attributes are missing, the observation is Quarantined with reason code `MissingMandatoryAttributes`. |

### Decision Alternatives

Accept, Quarantine, Reject.

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---------|---------|----------|
| Accept | Record is trustworthy; eligible for incorporation. | All eligibility rules (BR-D-200–BR-D-203) pass. |
| Quarantine | Record cannot be automatically accepted; permanently held. | One or more rules produce a marginal failure, or source reliability is borderline. |
| Reject | Record is permanently excluded. | One or more rules produce a critical failure (e.g., duplicate, quantity out of range). |

**Conflict Resolution:** Most severe outcome prevails. Reject overrides Quarantine; Quarantine overrides Accept.

**Data Quality Flag:** When the decision is Accept but a quality metric is borderline (e.g., timeliness at 95% of the maximum allowed latency), a Data Quality Flag is recorded on the observation. This is evidence, not a separate decision outcome.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Demand Observation | SE-D-001 | The record being evaluated, containing all mandatory and optional attributes. |
| Source Reliability Index | Derived from historical data, governed by PO-D-001 | A metric representing the historical accuracy and timeliness of the source system. |
| Duplicate Detection Window | Governed by PO-D-001 | The time window within which duplicate records are detected. |

**Decision Confidence:** Confidence Type: Evidence Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: The confidence level reflects the trustworthiness of the source system and the recency of the data.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic if all eligibility rules pass and source reliability meets threshold. Escalated to Demand Data Steward if borderline. |
| **Decision Boundary** | This decision determines whether demand data is trustworthy for incorporation. It does not determine how the observation is used in planning. |
| **Inconclusive Criteria Handling** | If the decision cannot produce a clear Accept or Reject (e.g., borderline source reliability), the data is Quarantined and routed for manual review. The specific criterion causing inconclusiveness is recorded. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-200 | Demand signal timeliness must be within maximum allowed latency. |
| BR-D-201 | Demand signal quantity must be within valid range. |
| BR-D-202 | Source reliability must meet minimum threshold. |
| BR-D-203 | Duplicate data within the same window is rejected. |

### Policies

Governed by PO-D-001.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-002 (Evaluate Demand Observation) |
| Invoked By | FS-D-002 |
| References | BR-D-200, BR-D-201, BR-D-202, BR-D-203 |
| Governed By | PO-D-001 |
| Produces | Decision outcome consumed by AB-D-002 |

### Explainability

**Rationale Template:** “Demand observation accepted: source {source}, type {type}, quantity {qty}, timestamp age {age} min, within expected range. Source reliability {rel}%.”

For Quarantine or Reject outcomes, the rationale is extended with the specific failing rule and reason code.

### Exceptional Conditions

- If the record’s mandatory attributes are incomplete, Quarantine immediately with reason code `MissingMandatoryAttributes`.
- If evaluation is attempted on a record not in Received state, the operation is rejected.

## DE-D-002 – Publish Demand Understanding

**Outcome Type:** Publication Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Understand Demand (CA-D-001) |
| **Authoritative Representation** | The enterprise’s determination that a Draft Demand Understanding meets the criteria for publication and can become the authoritative Published version. |
| **Business Responsibility** | Ensure that only materially changed or periodically refreshed understandings are published, and that published versions are complete and properly supersede previous ones. |
| **Authority Scope** | Per Planning Scope. |
| **Intended Consumers** | All downstream capabilities that consume the Published understanding (Forecast Demand, Sense Demand, Segment Demand, Classify Demand, Prioritize Demand, Supply Intelligence, Promise Intelligence, Scenario Intelligence). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether the revised Demand Understanding meets publication criteria.

### Enterprise Question

Is the draft understanding materially different or due for a periodic refresh, and is it complete enough to become the authoritative Published version?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | A Draft version of the Demand Understanding exists. A materiality assessment has been performed. Interpretation completeness has been evaluated. |
| **Business Behavior** | Evaluate the materiality assessment result. If at least one interpretation dimension is Material, or a Periodic Refresh is due per PO-D-012, and interpretation completeness meets the threshold defined in PO-D-011, the outcome is Publish. Otherwise, Do Not Publish. |
| **Exceptional Conditions** | If a mandatory publication condition from PO-D-011 is triggered but interpretation completeness is below threshold, the decision is Do Not Publish and the Demand Data Steward is notified. |
| **Postconditions** | If Publish: the Draft version becomes Published, the previous Published version is Superseded. If Do Not Publish: the Draft is retained. |
| **Outcome When Preconditions Are Not Satisfied** | If no Draft exists, the decision is not applicable. If the materiality assessment is missing, it is performed synchronously. |

### Decision Alternatives

Publish, Do Not Publish.

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---------|---------|----------|
| Publish | The Draft becomes the authoritative Published version. | Material change detected in at least one interpretation dimension, OR a Periodic Refresh is due per PO-D-012, AND interpretation completeness meets the threshold. |
| Do Not Publish | The Draft is retained; no new Published version is created. | No material change and no Periodic Refresh due, OR interpretation completeness below threshold. |

**Conflict Resolution:** If no material change → Do Not Publish, unless a Periodic Refresh is due. If both material change and Periodic Refresh, Publish. If interpretation completeness is below threshold, Do Not Publish regardless of materiality.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Materiality Assessment | Produced by the materiality evaluation | Per-dimension materiality determination. |
| Interpretation Completeness Score | Derived from Draft version | The proportion of mandatory interpretation dimensions that are complete. |
| Periodic Refresh Status | PO-D-012 | Whether the maximum publication interval has elapsed. |

**Decision Confidence:** Confidence Type: Rule Certainty. Confidence Level: Binary. Confidence Rationale: The decision is Publish or Do Not Publish based on deterministic criteria.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic when all criteria are satisfied. Escalated to Demand Data Steward when a mandatory publication condition is met but interpretation completeness is below threshold. |
| **Decision Boundary** | This decision determines whether a Draft becomes Published. It does not determine what interpretations are in the understanding. |
| **Inconclusive Criteria Handling** | If interpretation completeness is borderline (e.g., at exactly the threshold with some dimensions stale), the decision is Do Not Publish and the Demand Data Steward is notified. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-204 | Publication requires material change per PO-D-011 for at least one interpretation dimension, or a Periodic Refresh due per PO-D-012. |
| BR-D-205 | Interpretation completeness must meet the threshold defined in PO-D-011. |

### Policies

Governed by PO-D-011 and PO-D-012.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-004 (Publish Demand Understanding) |
| Invoked By | FS-D-004 |
| References | BR-D-204, BR-D-205 |
| Governed By | PO-D-011, PO-D-012 |
| Produces | Decision outcome consumed by AB-D-004 |

### Explainability

**Rationale Template:** “Demand Understanding published for Planning Scope {scope}. Material change: {dimensions_material}. Periodic Refresh: {yes/no}. Interpretation completeness: {score}%.”

For Do Not Publish: “Publication deferred. Reason: {reason_code}. Details: {details}.”

### Exceptional Conditions

- If the Periodic Refresh interval has elapsed but a publication cannot be completed (e.g., Enterprise Picture unavailable), the staleness is documented as a data quality flag in the next successful publication.

## DE-D-003 – Generate Forecast for Series

**Outcome Type:** Selection Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s determination of whether a statistical forecast can be produced for a specific demand series, or whether the series must be flagged as unforecastable. |
| **Business Responsibility** | For each series, decide whether sufficient demand history exists to generate a statistical forecast, and if not, declare the series unforecastable. The selection of the specific forecasting model is an algorithmic operation governed by PO-D-017, not an enterprise decision. |
| **Authority Scope** | Per demand series, per forecast cycle. |
| **Intended Consumers** | The Forecast Publication (SE-D-003). |

### Purpose

Determine whether a demand series has sufficient history to support statistical forecasting, and if not, declare it unforecastable so that downstream processes apply the governed fallback method.

### Enterprise Question

Can a meaningful statistical forecast be produced for this series, or must the enterprise declare it unforecastable?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The demand series has been identified for the current forecast cycle. The champion model is selected. |
| **Business Behavior** | Evaluate the series against the data sufficiency rule BR-D-206. If sufficient history exists, the outcome is Forecastable; the statistical forecast will be produced by the champion model. If insufficient history exists, the outcome is Unforecastable; the series will be handled per PO-D-019. |
| **Exceptional Conditions** | If the demand history contains only zero values or is otherwise invalid, the series is Unforecastable. |
| **Postconditions** | The series is classified as Forecastable or Unforecastable. If Forecastable, a forecast is produced. If Unforecastable, the series is flagged with a documented reason. |
| **Outcome When Preconditions Are Not Satisfied** | If the champion model has not been selected, the decision is deferred until selection is complete. |

### Decision Alternatives

Forecastable, Unforecastable.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Forecastable | The series has at least the minimum number of demand history periods required by BR-D-206. |
| Unforecastable | The series does not meet the minimum history requirement, or the history is invalid. |

**Conflict Resolution:** Not applicable; the outcome is determined by a single criterion.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Demand history for the series | Historical demand data (via Enterprise Picture) | Quantity and length of history. |

**Decision Confidence:** Confidence Type: Rule Certainty. Confidence Level: Binary. Confidence Rationale: The decision is based on a deterministic data sufficiency check.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic. |
| **Decision Boundary** | This decision determines whether a series can be statistically forecast. It does not select the forecasting model, nor does it generate the forecast value. |
| **Inconclusive Criteria Handling** | If the history length is exactly at the minimum threshold but the data quality is poor, the series is Forecastable but flagged for review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-206 | A minimum number of periods of demand history is required to generate a statistical forecast. |

### Policies

Governed by PO-D-019.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-007 (Produce Forecast Projection) |
| Invoked By | FS-D-006 |
| References | BR-D-206 |
| Governed By | PO-D-019 |
| Produces | Decision outcome consumed by AB-D-007 |

### Explainability

**Rationale Template:** “Series {id}: Forecastable. History length: {periods} periods (minimum {min}).”
For Unforecastable: “Series {id}: Unforecastable. Reason: insufficient history ({periods} periods, minimum {min}).”

## DE-D-004 – Approve Forecast Publication

**Outcome Type:** Authorization Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s determination that the Forecast Publication meets the criteria to become the authoritative demand projection. |
| **Business Responsibility** | Verify completeness, confidence, and governance compliance before publication. |
| **Authority Scope** | Per Forecast Publication. |
| **Intended Consumers** | All downstream capabilities that consume the Published forecast (Supply Intelligence, Promise Intelligence, Scenario Intelligence, Evaluate Demand Quality, Understand Demand). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether the Forecast Publication is eligible to become the authoritative demand projection.

### Enterprise Question

Does this forecast publication meet the completeness, confidence, and governance thresholds for authoritative release?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Draft Forecast Publication exists with forecasts generated. The Forecast Confidence Index (KA-D-009) has been computed. All forecast assumptions have been signed off per PO-D-025. |
| **Business Behavior** | Evaluate the Forecast Confidence Index against the auto-publication threshold defined in PO-D-020. If the index meets or exceeds the threshold and completeness is met, the outcome is Publish Automatically. If below threshold but above the minimum, the outcome is Require Planner Approval. If completeness is below the minimum threshold, the outcome is Suppress Publication. |
| **Exceptional Conditions** | If the Forecast Confidence Index is borderline (within the borderline tolerance defined by PO-D-020), the outcome is Require Planner Approval. |
| **Postconditions** | If Publish Automatically: the publication is ready for release. If Require Planner Approval: the Demand Planner is notified. If Suppress: the Demand Manager is notified. |
| **Outcome When Preconditions Are Not Satisfied** | If the Forecast Confidence Index has not been computed, the decision is deferred. If assumptions are not signed off, the outcome is Suppress Publication until sign-off is complete. |

### Decision Alternatives

Publish Automatically, Require Planner Approval, Suppress Publication.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Publish Automatically | Forecast Confidence Index ≥ auto-publication threshold, completeness ≥ threshold, all assumptions signed off. |
| Require Planner Approval | Forecast Confidence Index below auto-publication threshold but above the minimum, or a Demand Manager override is pending. |
| Suppress Publication | Completeness below the minimum threshold, or critical governance failure such as unsigned assumptions. |

**Conflict Resolution:** If the Demand Manager overrides per PO-D-021, the override decision takes precedence.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Forecast Confidence Index | KA-D-009 | The computed confidence index for this publication. |
| Completeness score | Derived from SE-D-003 | Percentage of covered series with valid forecasts. |
| Assumption sign-off status | PO-D-025 governance records | Confirmation that all assumptions are approved. |

**Decision Confidence:** Confidence Type: Rule Certainty. Confidence Level: Binary. Confidence Rationale: The decision is based on deterministic thresholds.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic if all thresholds are met. Escalated to Demand Planner if confidence is below threshold. Demand Manager may override per PO-D-021. |
| **Decision Boundary** | This decision authorises publication. It does not alter the forecast values. |
| **Inconclusive Criteria Handling** | If the confidence index is borderline, the decision is Deferred to the Demand Planner. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-207 | Completeness threshold must be met for publication. |

### Policies

Governed by PO-D-020, PO-D-021, PO-D-025.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-009 (Publish Forecast Publication) |
| Invoked By | FS-D-008 |
| References | BR-D-207 |
| Governed By | PO-D-020, PO-D-021, PO-D-025 |
| Produces | Decision outcome consumed by AB-D-009 |

### Explainability

**Rationale Template:** “Forecast Publication {id}: approved for automatic publication. Confidence Index {x}% (threshold {y}%), completeness {z}%.”
For Require Planner Approval: “Publication requires Demand Planner approval. Confidence Index {x}% below auto-publication threshold {y}%.”
For Suppress: “Publication suppressed. Completeness {z}% below minimum threshold {t}%.”

## DE-D-005 – Evaluate Forecast Override

**Outcome Type:** Authorization Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s determination of whether a planner’s override of a system forecast is acceptable. |
| **Business Responsibility** | Govern the acceptance, rejection, or revision of planner overrides. |
| **Authority Scope** | Per override request. |
| **Intended Consumers** | The Forecast Publication (SE-D-003). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Allow a planner to replace a system-generated forecast value when they possess business knowledge not yet reflected in demand signals, while ensuring governance and traceability.

### Enterprise Question

Is this planner override justified, within governed deviation limits, and properly authorised?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | A Draft Forecast Publication exists and is not Published. The override request includes a non-empty business justification. |
| **Business Behavior** | Evaluate the override against three criteria: the justification is non-empty (BR-D-208), the deviation from the system forecast is within the configured limit (BR-D-209), and the planner is authorised under PO-D-022. If all criteria are met, Accept. If justification is empty, Reject. If deviation exceeds the limit and the planner is not a Demand Manager, Request Revision. |
| **Exceptional Conditions** | If the planner is a Demand Manager, the deviation limit may be overridden with documented justification. |
| **Postconditions** | If Accepted: the override is applied and the original system forecast is preserved. If Rejected: the planner is notified of the reason. If Revision Requested: the planner is prompted to resubmit. |
| **Outcome When Preconditions Are Not Satisfied** | If no Draft publication exists, the override is Rejected. |

### Decision Alternatives

Accept override, Reject override, Request revision.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Accept override | Justification is non-empty, deviation is within the configured limit, and the planner is authorised (or Demand Manager approval obtained). |
| Reject override | Justification is empty. |
| Request revision | Deviation exceeds the configured limit and the planner is not a Demand Manager. |

**Conflict Resolution:** If multiple overrides are submitted for the same series, the most recent accepted override prevails.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Override request | Planner submission | The new value, justification, and planner identity. |
| System forecast value | SE-D-003 | The original system forecast being overridden. |
| Deviation limit | PO-D-022 | The configured maximum allowed deviation. |

**Decision Confidence:** Confidence Type: Rule Certainty. Confidence Level: Binary. Confidence Rationale: The decision is based on clear, deterministic criteria.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic if deviation is within limit and justification is non-empty. Escalated to Demand Manager if deviation exceeds limit. |
| **Decision Boundary** | This decision evaluates the override. It does not modify the system forecast directly; that is performed by the owning Aggregate Behavior. |
| **Inconclusive Criteria Handling** | If the justification is present but its sufficiency is questionable, the override is Escalated to the Demand Planner for review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-208 | Every override must contain a non-empty business justification. |
| BR-D-209 | The override value must not deviate from the system forecast beyond the configured limit unless Demand Manager approval is obtained. |

### Policies

Governed by PO-D-022, PO-D-023.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-008 (Govern Forecast Projection) |
| Invoked By | FS-D-007 |
| References | BR-D-208, BR-D-209 |
| Governed By | PO-D-022, PO-D-023 |
| Produces | Decision outcome consumed by AB-D-008 |

### Explainability

**Rationale Template:** “Forecast override for {item} at {location}, bucket {bucket}: accepted. Original {original}, override {override}. Justification: ‘{reason}’. Deviation {deviation}%.”
For Reject: “Override rejected. Reason: Justification empty.”
For Request Revision: “Override requires revision. Deviation {deviation}% exceeds limit {limit}%.”

## DE-D-006 – Evaluate Demand Signal for State Change

**Outcome Type:** Classification Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Sense Demand (CA-D-003) |
| **Authoritative Representation** | The enterprise’s determination of whether an incoming demand signal warrants a change to the current demand behavior state. |
| **Business Responsibility** | Evaluate the signal against the Demand Understanding baseline and governed deviation thresholds, and determine the resulting state transition. |
| **Authority Scope** | Per signal, per monitored item-location. |
| **Intended Consumers** | The Demand Behavior Assessment (SE-D-004). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether an incoming demand signal represents a meaningful deviation from expected demand behavior.

### Enterprise Question

Does this demand signal indicate that demand behavior has changed enough to warrant a state transition?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Demand Understanding (SE-D-002) provides the current expected behavior baseline. The Demand Sensing Policy (PO-D-031) is current. |
| **Business Behavior** | Compare the signal’s deviation from the baseline against the governed thresholds in PO-D-031. If the deviation exceeds the Critical threshold and is corroborated by at least two independent sources, the outcome is Transition to Critical. If it exceeds the Significant threshold but not the Critical threshold, the outcome is Transition to Elevated (positive deviation) or Transition to Depressed (negative deviation). If no threshold is met, the outcome is No Change. |
| **Exceptional Conditions** | If the signal is below the noise threshold defined in PO-D-031, the outcome is No Change and the signal is suppressed. |
| **Postconditions** | A state transition determination is produced. If a state change is warranted, the owning Aggregate Behavior records a State Change Event. |
| **Outcome When Preconditions Are Not Satisfied** | If the Demand Understanding baseline is unavailable, the signal is evaluated against the last known state; confidence is reduced. |

### Decision Alternatives

No Change, Transition to Elevated, Transition to Depressed, Transition to Critical.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| No Change | Deviation is below the Significant threshold. |
| Transition to Elevated | Deviation exceeds the Significant threshold in the positive direction, and Critical criteria are not met. |
| Transition to Depressed | Deviation exceeds the Significant threshold in the negative direction, and Critical criteria are not met. |
| Transition to Critical | Deviation exceeds the Critical threshold, and the signal is corroborated by at least two independent sources. |

**Conflict Resolution:** If the signal exceeds both Significant and Critical thresholds but is not corroborated, the outcome is Transition to Elevated/Depressed.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Demand signal | Incoming data | The observed quantity, timestamp, and source. |
| Baseline expected demand | Demand Understanding (SE-D-002) | The expected demand level for this item-location. |
| Corroborating signals | Other independent sources | Additional signals supporting the deviation. |

**Decision Confidence:** Confidence Type: Evidence Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence increases with corroboration and signal reliability.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-031. Critical state transitions are Escalated to the Demand Manager. |
| **Decision Boundary** | This decision evaluates the signal and determines the state transition. It does not trigger downstream actions; that is the role of DE-D-007. |
| **Inconclusive Criteria Handling** | If corroboration is insufficient for a Critical determination, the outcome is capped at Elevated/Depressed and the assessment is flagged for monitoring. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-300 | Deviation thresholds are governed by PO-D-031. |
| BR-D-301 | Critical state requires corroboration by at least two independent sources. |
| BR-D-302 | For high-priority items, the Significant threshold is lowered per PO-D-031. |
| BR-D-303 | Signals below the noise threshold are suppressed. |

### Policies

Governed by PO-D-031.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-010 (Maintain Demand Behavior Understanding) |
| Invoked By | FS-D-009 |
| References | BR-D-300, BR-D-301, BR-D-302, BR-D-303 |
| Governed By | PO-D-031 |
| Produces | Decision outcome consumed by AB-D-010 |

### Explainability

**Rationale Template:** “Signal for {item} at {location} evaluated. Deviation {d}σ from baseline. Outcome: {state}. Confidence: {c}%.”

## DE-D-007 – Trigger Forecast Refresh on Critical State

**Outcome Type:** Escalation Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Sense Demand (CA-D-003) |
| **Authoritative Representation** | The enterprise’s determination that a Critical demand behavior state warrants an immediate out-of-cycle forecast refresh. |
| **Business Responsibility** | Evaluate whether the Critical state meets the criteria for triggering a forecast refresh. |
| **Authority Scope** | Per Critical state transition. |
| **Intended Consumers** | Forecast Demand (CA-D-002). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether a newly transitioned Critical state warrants an immediate out-of-cycle forecast refresh.

### Enterprise Question

Is the Critical demand behavior significant enough, and is the current forecast stale enough, to justify an immediate refresh?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Demand Behavior Assessment is in Critical state. A current Forecast Publication (SE-D-003) exists. |
| **Business Behavior** | Evaluate the forecast age against the freshness threshold defined in PO-D-032. If the forecast is older than the threshold and the expected accuracy improvement from a refresh exceeds the minimum benefit, the outcome is Trigger Refresh. Otherwise, Defer to Next Scheduled Cycle. |
| **Exceptional Conditions** | If the current forecast is within the freshness threshold defined by PO-D-032, the outcome is Defer regardless of other criteria. |
| **Postconditions** | If Trigger Refresh: a new forecast cycle is initiated in Forecast Demand. If Defer: the Critical state is noted for the next scheduled cycle. |
| **Outcome When Preconditions Are Not Satisfied** | If no current Forecast Publication exists, the decision is not applicable. |

### Decision Alternatives

Trigger Refresh, Defer to Next Scheduled Cycle.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Trigger Refresh | State is Critical, forecast age exceeds the freshness threshold, and expected accuracy improvement exceeds the minimum benefit. |
| Defer to Next Scheduled Cycle | Freshness threshold not exceeded, or expected improvement below minimum benefit. |

**Conflict Resolution:** All criteria must be satisfied for Trigger Refresh; otherwise Defer.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Forecast age | Forecast Publication (SE-D-003) | Time since last publication. |
| Expected accuracy improvement | Derived from historical refresh impact data | Estimated WAPE reduction from a refresh. |

**Decision Confidence:** Confidence Type: Evidence Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence is inherited from the Critical state assessment.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-032. Demand Manager may override a deferral. |
| **Decision Boundary** | This decision triggers the refresh evaluation. It does not execute the forecast; Forecast Demand performs the refresh. |
| **Inconclusive Criteria Handling** | If expected improvement cannot be estimated, the decision defaults to Trigger Refresh for Critical items and Defer for others. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-304 | A Critical state change automatically triggers evaluation for a forecast refresh. |

### Policies

Governed by PO-D-032, PO-D-034.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-010 (Maintain Demand Behavior Understanding) |
| Invoked By | FS-D-010 |
| References | BR-D-304 |
| Governed By | PO-D-032, PO-D-034 |
| Produces | Decision outcome consumed by AB-D-010 |

### Explainability

**Rationale Template:** “Forecast refresh triggered for {item} at {location} due to Critical demand behavior. Forecast age: {hours}h.”
For Defer: “Forecast refresh deferred. Forecast age {hours}h within freshness threshold.”

## DE-D-008 – Determine Planning Classification

**Outcome Type:** Classification Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Segment Demand (CA-D-004) |
| **Authoritative Representation** | The enterprise’s determination of the planning classification for an entity under a governed classification scheme. |
| **Business Responsibility** | Apply the Segmentation Policy rules to the entity’s attributes and assign the correct classification. |
| **Authority Scope** | Per entity, per classification type. |
| **Intended Consumers** | The Planning Classification Assignment (SE-D-005), and downstream capabilities that consume classifications (Forecast Demand, Classify Demand, Prioritize Demand, Inventory Planning, Supply Intelligence). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

For a given entity and classification type, determine the current class label according to the Segmentation Policy.

### Enterprise Question

What is the correct planning classification for this entity under this classification type?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Segmentation Policy (PO-D-035) is current. The required evidence for the classification type is available. |
| **Business Behavior** | Apply the classification rules defined in the Segmentation Policy. If the entity’s attributes satisfy the criteria for a specific class, assign that class. If evidence is insufficient, assign Unclassified. |
| **Exceptional Conditions** | If the Segmentation Policy is missing or defines no rules for the classification type, the outcome is Unclassified. |
| **Postconditions** | A classification is assigned and recorded. |
| **Outcome When Preconditions Are Not Satisfied** | If mandatory evidence is missing, the entity is assigned Unclassified. |

### Decision Alternatives

The set of class labels defined for the classification type in the Segmentation Policy (e.g., A/B/C for ABC; X/Y/Z for XYZ), or Unclassified.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Specific class label | Entity attributes satisfy the criteria for that class per the Segmentation Policy. |
| Unclassified | Insufficient evidence, or no policy rules defined for the type. |

**Conflict Resolution:** If an entity meets criteria for multiple classes (e.g., borderline), the policy precedence rules determine the outcome. If the policy is ambiguous, Unclassified is assigned and the Demand Manager is notified.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Entity attributes | Demand Understanding (SE-D-002), historical demand data | Demand volume, variability, customer importance, etc. |
| Segmentation Policy | PO-D-035 | The governed classification rules and thresholds. |

**Decision Confidence:** Confidence Type: Classification Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence reflects the completeness and recency of the evidence and the fit to the classification rules.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-035. Planner overrides require justification and are reviewed quarterly per PO-D-036. |
| **Decision Boundary** | This decision classifies the entity. It does not determine how downstream capabilities use the classification. |
| **Inconclusive Criteria Handling** | If evidence is borderline, the entity is classified at the lower class and flagged for review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-305 | Classification must be determined by the rules defined in the current Segmentation Policy. |
| BR-D-306 | An entity shall be classified as Unclassified if minimum evidence requirements are not met. |

### Policies

Governed by PO-D-035, PO-D-036.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-011 (Classify Planning Entity) |
| Invoked By | FS-D-011 |
| References | BR-D-305, BR-D-306 |
| Governed By | PO-D-035, PO-D-036 |
| Produces | Decision outcome consumed by AB-D-011 |

### Explainability

**Rationale Template:** “Entity {id}, type {type}: classified as {class}. Evidence: {summary}. Confidence: {c}%.”

## DE-D-009 – Determine Behavior Classification

**Outcome Type:** Classification Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Classify Demand (CA-D-005) |
| **Authoritative Representation** | The enterprise’s determination of the behavioral classification for a demand entity. |
| **Business Responsibility** | Apply the Classification Policy rules to the entity’s demand patterns and assign the correct behavior classification. |
| **Authority Scope** | Per entity, per behavior dimension. |
| **Intended Consumers** | The Demand Behavior Assignment (SE-D-006), and downstream capabilities that consume behavior classifications (Forecast Demand, Detect Demand Exceptions, Explain Demand, Prioritize Demand, Inventory Planning, Supply Intelligence, Scenario Intelligence). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

For a given entity and behavior dimension, determine the current classification according to the Classification Policy.

### Enterprise Question

What behavior does this demand exhibit, and what is the correct classification for this dimension?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Classification Policy (PO-D-037) is current. Required evidence for the dimension is available. |
| **Business Behavior** | Apply the classification rules defined in the Classification Policy. If the entity’s demand pattern meets the criteria for a specific class, assign that class with confidence and an evidence summary. If evidence is insufficient, assign Unclassified. |
| **Exceptional Conditions** | If the Classification Policy is missing, the outcome is Unclassified. |
| **Postconditions** | A classification is assigned and recorded. |
| **Outcome When Preconditions Are Not Satisfied** | If mandatory evidence is missing, the entity is assigned Unclassified. |

### Decision Alternatives

The set of recognised classifications for the dimension as defined in the Classification Policy (e.g., Continuous, Intermittent, Seasonal, Lumpy, Trend for Statistical Pattern), or Unclassified.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Specific class label | Entity demand pattern satisfies the criteria for that class per the Classification Policy. |
| Unclassified | Insufficient evidence, or no policy rules defined for the dimension. |

**Conflict Resolution:** Multiple independent dimensions capture composite behaviors without requiring a single composite label.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Demand pattern data | Demand Understanding (SE-D-002), historical demand data | Statistical features of the demand series. |
| Classification Policy | PO-D-037 | Governed rules per behavior dimension. |

**Decision Confidence:** Confidence Type: Classification Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence reflects the strength of the statistical evidence supporting the classification.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-037. Planner overrides require justification and are reviewed quarterly per PO-D-038. |
| **Decision Boundary** | This decision classifies behavior. It does not select forecasting models directly; model selection is an algorithmic operation governed by the classification output. |
| **Inconclusive Criteria Handling** | If statistical evidence is insufficient, the entity is classified as Unclassified and flagged for review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-307 | Classification must be determined by the rules defined in the current Classification Policy. |
| BR-D-308 | An entity shall be classified as Unclassified if minimum evidence requirements are not met. |

### Policies

Governed by PO-D-037, PO-D-038.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-012 (Classify Demand Behavior) |
| Invoked By | FS-D-012 |
| References | BR-D-307, BR-D-308 |
| Governed By | PO-D-037, PO-D-038 |
| Produces | Decision outcome consumed by AB-D-012 |

### Explainability

**Rationale Template:** “Entity {id}, dimension {dim}: classified as {class}. Evidence: {summary}. Confidence: {c}%.”

## DE-D-010 – Determine Planning Priority

**Outcome Type:** Classification Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Prioritize Demand (CA-D-006) |
| **Authoritative Representation** | The enterprise’s determination of the planning priority for a planning entity. |
| **Business Responsibility** | Apply the Prioritization Policy to compute the priority score and assign the priority level, together with a business-language decision rationale. |
| **Authority Scope** | Per planning entity. |
| **Intended Consumers** | The Planning Priority Assignment (SE-D-007), and downstream capabilities that consume priorities (Forecast Demand, Detect Demand Exceptions, Inventory Planning, Scenario Intelligence). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

For a given planning entity, determine its current planning priority according to the Prioritization Policy.

### Enterprise Question

What is the relative planning importance of this entity, and what is the business rationale for that priority?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Prioritization Policy (PO-D-039) is current. Mandatory business evidence is available. |
| **Business Behavior** | Apply the scoring methodology defined in the Prioritization Policy. Compute the priority score. Map the score to the priority level using the policy-defined thresholds. Generate a business-language decision rationale and a statement of business validity—the conditions under which this priority applies. |
| **Exceptional Conditions** | If mandatory evidence is missing, assign Unclassified. |
| **Postconditions** | A priority level, score, decision rationale, and business validity are recorded. |
| **Outcome When Preconditions Are Not Satisfied** | If mandatory evidence is missing, the entity is assigned Unclassified. |

### Decision Alternatives

Critical, High, Medium, Low, Unclassified.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Critical | Priority score meets the Critical threshold. |
| High | Priority score meets the High threshold. |
| Medium | Priority score meets the Medium threshold. |
| Low | Priority score meets the Low threshold. |
| Unclassified | Mandatory evidence missing. |

**Conflict Resolution:** Priority establishes ordering, not partitioning. If an entity is borderline, it is assigned the higher priority level and flagged for review.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Entity attributes | Demand Understanding (SE-D-002), Planning Classification (SE-D-005), Demand Behavior Assignment (SE-D-006) | Revenue contribution, strategic importance, contractual obligations, demand behavior. |
| Prioritization Policy | PO-D-039 | Scoring methodology and level thresholds. |

**Decision Confidence:** Confidence Type: Classification Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence reflects the completeness and recency of the input data.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-039. Planner overrides require business justification and are reviewed quarterly per PO-D-040. |
| **Decision Boundary** | This decision determines priority. It does not allocate resources or execute actions. |
| **Inconclusive Criteria Handling** | If input data is incomplete, the entity is assigned Unclassified and flagged for review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-309 | Priority must be determined using the scoring methodology and level thresholds defined in the current Prioritization Policy. |
| BR-D-310 | An entity shall be assigned Unclassified priority if mandatory business evidence is not available. |

### Policies

Governed by PO-D-039, PO-D-040.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-013 (Prioritize Planning Entity) |
| Invoked By | FS-D-013 |
| References | BR-D-309, BR-D-310 |
| Governed By | PO-D-039, PO-D-040 |
| Produces | Decision outcome consumed by AB-D-013 |

### Explainability

**Rationale Template:** “Entity {id}: priority {level}, score {score}. Rationale: {business justification}. Validity: {conditions}.”

## DE-D-011 – Publish Forecast Quality Assessment

**Outcome Type:** Publication Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Evaluate Demand Quality (CA-D-007) |
| **Authoritative Representation** | The enterprise’s determination that the Forecast Quality Assessment meets publication criteria and can become the authoritative enterprise quality record. |
| **Business Responsibility** | Verify that the assessment data meets the completeness and evaluation period requirements defined in the Forecast Measurement Policy before publication. |
| **Authority Scope** | Per Planning Scope and Evaluation Period. |
| **Intended Consumers** | Learn From Demand, Explain Demand, Forecast Demand (model performance feedback), Demand Planners and Managers. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether the computed forecast quality metrics meet the publication criteria defined in the Forecast Measurement Policy.

### Enterprise Question

Is this forecast quality assessment complete, reliable, and governance-compliant enough to become the authoritative enterprise record?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The quality metrics have been computed for the full evaluation period. Actual demand data covers the full period. |
| **Business Behavior** | Evaluate data completeness and evaluation period length against the thresholds in PO-D-041. If completeness meets the threshold and the evaluation period meets the minimum length, the outcome is Publish. Otherwise, Do Not Publish. |
| **Exceptional Conditions** | If the evaluation period is shorter than the minimum, the outcome is Do Not Publish regardless of completeness. |
| **Postconditions** | If Publish: the assessment becomes the authoritative Published version for the Planning Scope and Evaluation Period. If Do Not Publish: the assessment is retained as Draft and the Demand Manager is notified. |
| **Outcome When Preconditions Are Not Satisfied** | If completeness is below threshold, the outcome is Do Not Publish. |

### Decision Alternatives

Publish, Do Not Publish.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Publish | Data completeness meets the policy threshold, and the evaluation period meets the minimum length. |
| Do Not Publish | Data completeness below threshold, or evaluation period too short. |

**Conflict Resolution:** Not applicable.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Completeness score | Derived from SE-D-008 | Proportion of covered series with actual demand data. |
| Evaluation period length | Calendar metadata | Whether the period meets the minimum defined in PO-D-041. |

**Decision Confidence:** Confidence Type: Rule Certainty. Confidence Level: Binary. Confidence Rationale: The decision is based on deterministic thresholds.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic if thresholds are met. Demand Manager notified if publication is suppressed per PO-D-041. |
| **Decision Boundary** | This decision determines publication readiness. It does not alter the computed metrics. |
| **Inconclusive Criteria Handling** | If completeness is borderline, the assessment is published with a flag and the Demand Manager is notified. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-212 | A Forecast Quality Assessment shall only be published if actual demand data covers the full evaluation period and meets the completeness threshold. |
| BR-D-213 | The evaluation period shall meet the minimum length defined in the Forecast Measurement Policy. |

### Policies

Governed by PO-D-041.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-014 (Evaluate Forecast Quality) |
| Invoked By | FS-D-014 |
| References | BR-D-212, BR-D-213 |
| Governed By | PO-D-041 |
| Produces | Decision outcome consumed by AB-D-014 |

### Explainability

**Rationale Template:** “Forecast Quality Assessment for Planning Scope {scope}, period {period}: published. Completeness {pct}%.”
For Do Not Publish: “Publication suppressed. Reason: {reason}. Completeness {pct}%.”


## DE-D-012 – Evaluate Demand Exception Evidence

**Outcome Type:** Classification Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Detect Demand Exceptions (CA-D-008) |
| **Authoritative Representation** | The enterprise's determination of whether demand exception evidence exists and, if so, its severity. |
| **Business Responsibility** | Apply the Exception Detection Policy to the current demand evidence and determine whether a governed policy has been breached. |
| **Authority Scope** | Per planning entity and condition type. |
| **Intended Consumers** | Core Exception Management (CA-C-020), Explain Demand, Learn From Demand, Planners and Managers. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

For a given planning entity and condition type, determine whether the current demand information meets the detection thresholds and, if so, at what severity.

### Enterprise Question

Does the current demand situation meet the enterprise’s criteria for a formal demand planning condition, and how severe is it?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The Exception Detection Policy (PO-D-044) is current. The relevant demand evidence (Demand Understanding, Forecast Quality Assessment, Demand Behavior Assessment) is available. |
| **Business Behavior** | Apply the detection rules defined in the Exception Detection Policy for the condition type. If the evidence satisfies the triggering criteria, the outcome is Detection Evidence Exists with the severity level determined by the policy rules. If the evidence no longer meets detection criteria, the outcome is Resolution Evidence Exists. If no criteria are met and no active exception evidence exists, the outcome is No Evidence. |
| **Exceptional Conditions** | If evidence is contradictory (e.g., one source indicates a violation while another indicates compliance), the outcome is Detection Evidence Exists at the lower severity and the evidence is flagged for manual review. |
| **Postconditions** | Exception detection or resolution evidence is produced and published to Core Exception Management. |
| **Outcome When Preconditions Are Not Satisfied** | If mandatory evidence is missing, the condition is not evaluated. |

### Decision Alternatives

Detection Evidence Exists (with severity: Critical, High, Medium, Low), Resolution Evidence Exists, No Evidence.

### Decision Outcome Contract

| Outcome | Meaning | Criteria |
|---------|---------|----------|
| Detection Evidence Exists | Demand exception evidence is published to Core Exception Management. | Detection thresholds met; severity assessment indicates Critical, High, Medium, or Low impact. |
| Resolution Evidence Exists | Demand exception resolution evidence is published to Core Exception Management. | Detection thresholds no longer met; underlying data returned to within governed thresholds. |
| No Evidence | No exception evidence is published. | No detection thresholds are met and no active exception evidence exists. |

**Conflict Resolution:** If multiple condition types could apply, the most specific matching type is selected per policy precedence.

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Forecast quality metrics | SE-D-008 | Accuracy, bias, stability data. |
| Demand Understanding | SE-D-002 | Current demand interpretation. |
| Demand Behavior Assessment | SE-D-004 | Current behavior state. |
| Exception Detection Policy | PO-D-044 | Governed thresholds per condition type. |

**Decision Confidence:** Confidence Type: Evidence Confidence. Confidence Level: High, Medium, or Low. Confidence Rationale: Confidence reflects the reliability and completeness of the evidence used.

### Decision Authority

| Section | Value |
|---------|-------|
| **Governance Authority** | Automatic per PO-D-044. Critical conditions are Escalated to the Demand Manager. |
| **Decision Boundary** | This decision detects and classifies conditions. It does not prescribe responses or manage the condition lifecycle; lifecycle management is the responsibility of the owning Aggregate Behavior. |
| **Inconclusive Criteria Handling** | If evidence is borderline, the condition is created at the lower severity and flagged for manual review. |

### Business Rules

| ID | Rule |
|----|------|
| BR-D-311 | Demand exception evidence shall only be published if the detection evidence meets the thresholds in PO-D-044. |

### Policies

Governed by PO-D-044.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-015 (Detect Demand Planning Conditions) |
| Invoked By | FS-D-015 |
| References | BR-D-311 |
| Governed By | PO-D-044 |
| Produces | Decision outcome consumed by AB-D-015 |

### Explainability

**Rationale Template:** “Condition type {type} for {entity}: {outcome}. Evidence: {summary}. Severity: {severity}. Confidence: {c}%.”


## DE-D-013 – Select Champion Model

**Outcome Type:** Selection Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise's determination of whether the current champion forecasting model should be replaced by a challenger for this forecast cycle. |
| **Business Responsibility** | Apply the Model Governance Policy (PO-D-017) to historical performance evidence and determine whether to retain the champion or promote a challenger. |
| **Authority Scope** | Per forecast cycle. |
| **Intended Consumers** | AB-D-006 (Select Champion Model), AB-D-007 (Produce Forecast Projection). |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Purpose

Determine whether the champion forecasting model should be replaced by a challenger.

### Enterprise Question

Does the best performing challenger demonstrate statistically significant improvement over the current champion without degrading bias, stability, or high-priority item accuracy?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | At least one challenger model has been evaluated over the minimum evaluation period defined in PO-D-017. Historical performance metrics are available. |
| **Business Behavior** | Evaluate the best challenger's WAPE improvement against the champion using the statistical significance threshold governed by PO-D-017. If improvement is significant, verify that bias degradation does not exceed the tolerance, stability degradation does not exceed the tolerance, and WAPE on high-priority items does not degrade beyond the protection threshold. If all criteria are satisfied, outcome is Promote Challenger. Otherwise, Retain Champion. |
| **Exceptional Conditions** | If multiple challengers meet criteria, the one with the highest WAPE improvement is selected. |
| **Postconditions** | A champion model is designated for the cycle. Model provenance is recorded. |
| **Outcome When Preconditions Are Not Satisfied** | If no challenger has been evaluated, Retain Champion. |

### Decision Alternatives

Retain Champion, Promote Challenger.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Retain Champion | No challenger meets the promotion criteria. |
| Promote Challenger | Best challenger shows statistically significant WAPE improvement, with bias, stability, and high-priority WAPE within governed tolerances per PO-D-017. |

### Evidence Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Champion historical performance | SE-D-008 (historical quality assessments) | WAPE, bias, stability, high-priority WAPE. | Yes | Retain Champion. |
| Challenger evaluation metrics | Model evaluation pipeline, governed by PO-D-017 | Comparative WAPE, bias, stability, high-priority WAPE over evaluation period. | Yes | Retain Champion. |

**Decision Confidence:** Rule Certainty. The decision is deterministic given the policy thresholds.

**Decision Authority:** Automatic. Demand Manager may override promotion per PO-D-017.

### Business Rules

| ID | Rule |
|----|------|
| BR-D-401 | The authorised forecasting strategy is selected per PO-D-017. |

### Policies

Governed by PO-D-017.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-006 (Select Champion Model) |
| Invoked By | FS-D-005 (Establish Forecast Cycle) |
| References | BR-D-401 |
| Governed By | PO-D-017 |
| Produces | Decision outcome consumed by AB-D-006 |

### Explainability

**Rationale Template:** "Champion model retained: no challenger met promotion criteria (WAPE improvement not statistically significant)." / "Champion model replaced: Challenger-X promoted due to significant WAPE improvement of 3.2pp with no bias/stability/high-priority degradation."

## DE-D-014 – Approve Intervention Impact Publication

**Outcome Type:** Authorization Decision

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Model Demand Interventions (CA-D-011) |
| **Authoritative Representation** | The enterprise's determination that a Demand Intervention Impact meets publication criteria. |
| **Business Responsibility** | Verify confidence, completeness, and governance compliance before publication. |
| **Authority Scope** | Per intervention impact. |
| **Intended Consumers** | CA-D-002 Forecast Demand, Supply Intelligence, Scenario Intelligence. |

### Purpose

Determine whether the Demand Intervention Impact is eligible to become the authoritative impact assessment.

### Enterprise Question

Does this intervention impact meet the confidence and completeness thresholds for authoritative release?

### Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | Draft SE-D-018 exists. PO-D-050 is current. |
| **Business Behavior** | Evaluate the Lift Confidence against the publication threshold defined in PO-D-050. If confidence meets or exceeds the threshold, the outcome is Publish. If below, the outcome is Do Not Publish. |
| **Exceptional Conditions** | If the intervention reference is no longer active, the outcome is Do Not Publish. |
| **Postconditions** | If Publish: Draft becomes Published, previous Published becomes Superseded. If Do Not Publish: Draft retained. |
| **Outcome When Preconditions Are Not Satisfied** | If no Draft exists, the decision is not applicable. |

### Decision Alternatives

Publish, Do Not Publish.

### Decision Outcome Contract

| Outcome | Criteria |
|---------|----------|
| Publish | Lift Confidence ≥ publication threshold defined in PO-D-050. Intervention Reference is active. |
| Do Not Publish | Lift Confidence below threshold, or Intervention Reference inactive. |

### Evidence Contract

| Input | Source | Description |
|-------|--------|-------------|
| Draft Demand Intervention Impact | SE-D-018 (Draft) | The computed impact assessment. |
| Publication threshold | PO-D-050 | The minimum confidence for publication. |
| Intervention status | SE-C-039 | Whether the intervention is still active. |

**Decision Confidence:** Rule Certainty. Binary.

**Decision Authority:** Automatic.

### Business Rules

| ID | Rule |
|----|------|
| BR-D-414 | Assessed Demand Lift must be non-negative. |
| BR-D-415 | Intervention Reference must point to an active Scenario Adjustment. |

### Policies

Governed by PO-D-050.

### Decision Trace

| Attribute | Value |
|-----------|-------|
| Decision Owner | AB-D-019 (Publish Demand Intervention Impact) |
| Invoked By | FS-D-019 |
| References | BR-D-414, BR-D-415 |
| Governed By | PO-D-050 |
| Produces | Publication determination consumed by AB-D-019 |

### Explainability

**Rationale Template:** "Intervention impact for {item} at {location}: published. Confidence {x}% (threshold {y}%). Intervention: {intervention_ref}."

---

# Chapter 7 — Rule Model

## Rule Precedence

The following evaluation order governs how rules are applied to any enterprise decision. Rules are evaluated in descending precedence. A lower-precedence rule is evaluated only if all higher-precedence rules succeed. If a rule fails, evaluation stops and the failure outcome is applied.

```
Identity
        ↓
Invariant
        ↓
Eligibility
        ↓
Behavior
        ↓
Derivation
```

| Rule Type | Enforcement Point | Override Behavior |
|-----------|-------------------|-------------------|
| Identity | Object creation | Absolute. Cannot be overridden. |
| Invariant | Aggregate commit boundary | Absolute. Cannot be overridden. |
| Eligibility | Functional Specification preconditions | Failure prevents further processing unless governed exception policy applies. |
| Behavior | Decision evaluation | Does not override Eligibility or Invariant rules. |
| Derivation | Business Algorithm execution | Does not override Eligibility, Invariant, or Behavior rules. |

Invariant Rules and Identity Rules cannot be overridden. If a Policy appears to contradict an Invariant Rule, the Invariant takes precedence and the Policy is invalid.

---

## Identity Rules

Identity rules define the unique enterprise business identity of an object. They are enforced at object creation. No other rule type may override an Identity Rule. Every object also receives a permanent, system-assigned identifier for traceability; this identifier is not the business identity.


### BR-D-002 – Demand Understanding Aggregate Identity

**Rule Statement:** The business identity of a Demand Understanding aggregate shall be the Planning Scope. Exactly one aggregate may exist for a given Planning Scope.

**Rule:** The Planning Scope must be a valid, active Planning Scope (SE-C-010). Only one Demand Understanding aggregate may exist for a given Planning Scope. Each version within the aggregate receives a monotonically increasing Version Number. The aggregate identity is independent of version.

**Evaluation Scope:** Per aggregate, before revision.

**Enforcement Point:** AB-D-003 (Revise Demand Understanding).

**Governed Policy:** PO-D-011.

**Outcome When Preconditions Are Not Satisfied:** Revision is not performed. The operation is rejected.

**Traceability:** Owned by CA-D-001. Referenced by AB-D-003, FS-D-004.

### BR-D-009 – Demand Exception Evidence Business Identity

**Rule Statement:** The business identity of demand exception evidence published to Core Exception Management shall map to the Core SE-C-019 Exception deduplication key: the combination of Constraint Reference, Affected Scope Type, and Affected Scope Identifier.

**Rule:** The Demand domain does not assign a permanent identifier to exception conditions. It evaluates evidence and publishes it to Core. Core assigns the Exception Identifier upon creation. The Demand domain must ensure that every detection and resolution evidence payload contains the exact Constraint Reference, Affected Scope Type, and Affected Scope Identifier to allow Core to deduplicate and manage the SE-C-019 lifecycle.

**Evaluation Scope:** Per evidence evaluation, before publication.

**Enforcement Point:** FS-D-015 (Detect Demand Exception Evidence).

**Governed Policy:** PO-D-044.

**Outcome When Preconditions Are Not Satisfied:** If the evidence payload lacks any of the three deduplication key components, publication to Core is rejected.

**Traceability:** Owned by CA-D-008. Referenced by FS-D-015, DE-D-012.

---

## Invariant Rules

Invariant rules define conditions that must always hold true for an aggregate. They are enforced at every aggregate commit boundary. No other rule type may override an Invariant Rule.

### BR-D-128 – Single Active Forecast Publication Generation Context per Scope and Horizon

**Rule Statement:** At any moment, no more than one active Forecast Publication generation context shall exist for a given Planning Scope and Forecast Horizon.

**Rule:** For a given Planning Scope and Forecast Horizon, at most one Forecast Publication generation context may be in an active (non-terminal) state. If a new generation context is initiated, any previously active generation context for the same scope and horizon must have reached a terminal state (Published or Superseded).

**Evaluation Scope:** Per forecast cycle initiation.

**Enforcement Point:** AB-D-005 (Initiate Forecast Cycle).

**Governed Policy:** PO-D-024.

**Outcome When Preconditions Are Not Satisfied:** Cycle initiation is rejected. The requesting event is logged.

**Traceability:** Owned by CA-D-002. Referenced by AB-D-005, FS-D-005.

### BR-D-103 – Single Published Demand Understanding per Planning Scope

**Rule Statement:** Exactly one Published Demand Understanding shall exist per Planning Scope at any moment.

**Rule:** Before publishing a new version, the previous Published version must be transitioned to Superseded atomically within the same business transaction.

**Evaluation Scope:** Per aggregate, at publication.

**Enforcement Point:** AB-D-004 (Publish Demand Understanding).

**Governed Policy:** PO-D-011.

**Outcome When Preconditions Are Not Satisfied:** The transaction fails. No new Published version is created. The previous Published version remains authoritative.

**Traceability:** Owned by CA-D-001. Referenced by AB-D-004, FS-D-004.

### BR-D-104 – Published Demand Understanding Immutability

**Rule Statement:** A Published Demand Understanding is immutable.

**Rule:** Any operation attempting to modify a Published version must be rejected. The version must be in Draft state to accept modifications.

**Evaluation Scope:** Per aggregate, on any modification attempt.

**Enforcement Point:** Aggregate root behavior of SE-D-002.

**Governed Policy:** PO-D-011.

**Outcome When Preconditions Are Not Satisfied:** The operation is rejected with an invariant violation.

**Traceability:** Owned by CA-D-001. Referenced by AB-D-003, AB-D-004.

### BR-D-109 – Original System Forecast Preservation on Override

**Rule Statement:** The original system forecast value shall be preserved when an override is applied.

**Rule:** When a planner override is accepted for a forecast line, the original statistical forecast value must be retained unchanged in the Forecast Override record. The published forecast value becomes the override value, but the original is permanently preserved for audit, accuracy measurement, and Forecast Value Added computation.

**Evaluation Scope:** Per override, at creation.

**Enforcement Point:** AB-D-008 (Govern Forecast Projection).

**Governed Policy:** PO-D-023.

**Outcome When Preconditions Are Not Satisfied:** The override is not recorded. The operation is rejected.

**Traceability:** Owned by CA-D-002. Referenced by AB-D-008, FS-D-007.

---

## Eligibility Rules

Eligibility rules govern whether an artifact meets the criteria to proceed to the next stage of processing. They are enforced at Decision evaluation.

### BR-D-200 – Demand Signal Timeliness

**Rule Statement:** Demand signal timeliness must be within maximum allowed latency.

**Rule:** The difference between Observation Time and Business Time must not exceed the governed latency threshold defined in PO-D-001.

**Evaluation Scope:** Per Demand Observation, at evaluation.

**Enforcement Point:** DE-D-001 (Accept Demand Observation).

**Governed Policy:** PO-D-001.

**Outcome When Preconditions Are Not Satisfied:** Data is Quarantined. Reason code: `DataLatencyExceeded`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-001, FS-D-002.

### BR-D-201 – Demand Quantity Range Validity

**Rule Statement:** Demand signal quantity must be within valid range.

**Rule:** Quantity validity depends on Observation Type and is governed by PO-D-001. Returns and corrections may carry negative quantities where governed by the observation type semantics.

**Evaluation Scope:** Per Demand Observation, at evaluation.

**Enforcement Point:** DE-D-001 (Accept Demand Observation).

**Governed Policy:** PO-D-001.

**Outcome When Preconditions Are Not Satisfied:** Data is Quarantined if borderline, Rejected if clearly erroneous. Reason code: `QuantityOutOfRange`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-001, FS-D-002.

### BR-D-202 – Source Reliability Threshold

**Rule Statement:** Source reliability must meet minimum threshold.

**Rule:** The source system’s historical reliability score must meet or exceed the governed threshold defined in PO-D-001.

**Evaluation Scope:** Per Demand Observation, at evaluation.

**Enforcement Point:** DE-D-001 (Accept Demand Observation).

**Governed Policy:** PO-D-001.

**Outcome When Preconditions Are Not Satisfied:** Data is Quarantined if reliability is marginal, Rejected if below absolute minimum. Reason code: `SourceReliabilityBelowThreshold`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-001, FS-D-002.

### BR-D-203 – Duplicate Data Detection

**Rule Statement:** Duplicate data within the same window shall be rejected.

**Rule:** No existing Demand Observation with the same source, type, item, location, quantity, and Business Time within the duplicate detection window defined in PO-D-001.

**Evaluation Scope:** Per Demand Observation, at evaluation.

**Enforcement Point:** DE-D-001 (Accept Demand Observation).

**Governed Policy:** PO-D-001.

**Outcome When Preconditions Are Not Satisfied:** Data is Rejected. Reason code: `DuplicateRecord`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-001, FS-D-002.

### BR-D-204 – Material Change Required for Publication

**Rule Statement:** Publication of the Demand Understanding requires material change in at least one interpretation dimension, or a Periodic Refresh due per PO-D-012.

**Rule:** The materiality assessment must return Material for at least one interpretation dimension (Demand Continuity, Demand Pattern, Demand Health, or Demand Volatility), OR the staleness interval defined in PO-D-012 has elapsed.

**Evaluation Scope:** Per publication attempt.

**Enforcement Point:** DE-D-002 (Publish Demand Understanding).

**Governed Policy:** PO-D-011, PO-D-012.

**Outcome When Preconditions Are Not Satisfied:** Publication is not performed. The Draft version is retained. Reason code: `NoMaterialChange`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-002, FS-D-006.

### BR-D-205 – Interpretation Completeness for Publication

**Rule Statement:** Interpretation completeness must meet the threshold defined in PO-D-011 before publication.

**Rule:** All mandatory interpretation dimensions (Demand Continuity, Demand Pattern, Demand Health, Demand Volatility) must be present and contain the required evidence references. A dimension may carry a status of "Incomplete" if evidence is missing, but the dimension itself must be present.

**Evaluation Scope:** Per publication attempt.

**Enforcement Point:** DE-D-002 (Publish Demand Understanding).

**Governed Policy:** PO-D-011.

**Outcome When Preconditions Are Not Satisfied:** Publication is deferred. The Demand Data Steward is notified. Reason code: `InterpretationIncompleteness`.

**Traceability:** Owned by CA-D-001. Referenced by DE-D-002, FS-D-006.

### BR-D-206 – Forecast Data Sufficiency

**Rule Statement:** A minimum number of periods of demand history is required to generate a statistical forecast. Series with insufficient history are flagged as unforecastable.

**Rule:** A demand series must have at least the minimum number of historical demand periods defined in PO-D-019. Series that do not meet this requirement shall be classified as Unforecastable.

**Evaluation Scope:** Per demand series, per forecast cycle.

**Enforcement Point:** DE-D-003 (Generate Forecast for Series).

**Governed Policy:** PO-D-019.

**Outcome When Preconditions Are Not Satisfied:** The series is classified as Unforecastable and routed to the fallback method per PO-D-019.

**Traceability:** Owned by CA-D-002. Referenced by DE-D-003, AB-D-007.

### BR-D-208 – Override Justification Requirement

**Rule Statement:** Every override must contain a non-empty business justification.

**Rule:** The justification field in a Forecast Override request must not be empty or consist only of whitespace. Overrides without justification shall be rejected.

**Evaluation Scope:** Per override request.

**Enforcement Point:** DE-D-005 (Evaluate Forecast Override).

**Governed Policy:** PO-D-022.

**Outcome When Preconditions Are Not Satisfied:** The override is Rejected. Reason code: `JustificationEmpty`.

**Traceability:** Owned by CA-D-002. Referenced by DE-D-005, AB-D-008.

### BR-D-209 – Override Deviation Limit

**Rule Statement:** The override value must not deviate from the system forecast beyond the configured deviation limit unless Demand Manager approval is obtained.

**Rule:** The absolute percentage deviation between the override value and the original system forecast value must not exceed the limit defined in PO-D-022. If it does, the override is Rejected or escalated to the Demand Manager per PO-D-022.

**Evaluation Scope:** Per override request.

**Enforcement Point:** DE-D-005 (Evaluate Forecast Override).

**Governed Policy:** PO-D-022.

**Outcome When Preconditions Are Not Satisfied:** The override is Rejected or routed for Demand Manager approval. Reason code: `DeviationExceeded`.

**Traceability:** Owned by CA-D-002. Referenced by DE-D-005, AB-D-008.

### BR-D-212 – Forecast Quality Assessment Publication Completeness

**Rule Statement:** A Forecast Quality Assessment shall only be published if actual demand data covers the full evaluation period and meets the completeness threshold.

**Rule:** The proportion of covered series with actual demand data for the full evaluation period must meet or exceed the completeness threshold defined in PO-D-041.

**Evaluation Scope:** Per Forecast Quality Assessment, at publication.

**Enforcement Point:** DE-D-011 (Publish Forecast Quality Assessment).

**Governed Policy:** PO-D-041.

**Outcome When Preconditions Are Not Satisfied:** The assessment is not published; it is retained as Draft and the Demand Manager is notified.

**Traceability:** Owned by CA-D-007. Referenced by DE-D-011, FS-D-014.

### BR-D-213 – Forecast Quality Assessment Evaluation Period Minimum

**Rule Statement:** The evaluation period shall meet the minimum length defined in the Forecast Measurement Policy.

**Rule:** The evaluation period must be at least the minimum length defined in PO-D-041 for a valid assessment.

**Evaluation Scope:** Per Forecast Quality Assessment, at publication.

**Enforcement Point:** DE-D-011 (Publish Forecast Quality Assessment).

**Governed Policy:** PO-D-041.

**Outcome When Preconditions Are Not Satisfied:** The assessment is not published regardless of completeness.

**Traceability:** Owned by CA-D-007. Referenced by DE-D-011, FS-D-014.

---

## Behavior Rules

Behavior rules govern how the enterprise evaluates situations and makes choices. They are enforced during Decision evaluation. A Behavior Rule expresses an enterprise constraint; it does not prescribe which actor or mechanism fulfils the constraint.

### BR-D-300 – Deviation Thresholds for Demand Behavior State Change

**Rule Statement:** A state transition to Elevated or Depressed requires the signal deviation to exceed the configured Significant threshold. A transition to Critical requires deviation to exceed the configured Critical threshold.

**Rule:** The deviation of an incoming demand signal from the expected baseline, measured in standard deviations, is compared against the thresholds defined in PO-D-031. The default Significant threshold is 2.5σ. The default Critical threshold is 4σ. These values are configurable per PO-D-031.

**Evaluation Scope:** Per signal, per monitored Item-Location.

**Enforcement Point:** DE-D-006 (Evaluate Demand Signal for State Change).

**Governed Policy:** PO-D-031.

**Outcome When Preconditions Are Not Satisfied:** If no threshold is exceeded, the outcome is No Change.

**Traceability:** Owned by CA-D-003. Referenced by DE-D-006, FS-D-009.

### BR-D-301 – Corroboration Requirement for Critical State

**Rule Statement:** A Critical state change shall be corroborated by at least two independent signal sources before the state is updated.

**Rule:** The incoming signal that exceeds the Critical threshold must be supported by at least one additional independent signal source (e.g., a POS signal and a warehouse shipment signal both indicating the same deviation) before the Demand Behavior Assessment transitions to Critical. If corroboration is insufficient, the state is capped at Elevated or Depressed.

**Evaluation Scope:** Per Critical signal evaluation.

**Enforcement Point:** DE-D-006 (Evaluate Demand Signal for State Change).

**Governed Policy:** PO-D-031.

**Outcome When Preconditions Are Not Satisfied:** The outcome is capped at Elevated or Depressed, and the assessment is flagged for monitoring.

**Traceability:** Owned by CA-D-003. Referenced by DE-D-006, FS-D-014.

### BR-D-304 – Forecast Refresh Evaluation on Critical State

**Rule Statement:** Every Critical demand behavior state shall be evaluated for forecast refresh eligibility before the next scheduled forecast cycle.

**Rule:** When a Demand Behavior Assessment transitions to Critical, the enterprise must evaluate whether an out-of-cycle forecast refresh is warranted. The evaluation criteria—forecast age, expected accuracy improvement, and freshness thresholds—are defined in PO-D-032. The rule does not prescribe which mechanism performs the evaluation; it only requires that the evaluation occurs before the next scheduled cycle.

**Evaluation Scope:** Per Critical state transition.

**Enforcement Point:** DE-D-007 (Trigger Forecast Refresh on Critical State).

**Governed Policy:** PO-D-032.

**Outcome When Preconditions Are Not Satisfied:** Not applicable; the rule requires the evaluation to be performed.

**Traceability:** Owned by CA-D-003. Referenced by DE-D-007, FS-D-015.

---

## Derivation Rules

Derivation rules define how enterprise values are derived from other enterprise facts. They are enforced at algorithm execution time. A Derivation Rule constrains the enterprise outcome; it does not prescribe the specific algorithm, model, or technology used to produce it.

### BR-D-400 – Demand Understanding Derivation Source [Derivation Invariant]

**Rule Statement:** The Demand Understanding shall be derived exclusively from the latest Published Enterprise Picture and the most recent Published Forecast Publication, if available.

**Rule:** Every revision to the Demand Understanding must use the most recent Published version of the Enterprise Picture (SE-C-021). If a Published Forecast Publication (SE-D-003) exists for the Planning Scope, it shall be consumed as forward-looking context for the Demand Pattern and Demand Volatility interpretations. No raw demand observations shall be consumed directly for interpretation.

**Evaluation Scope:** Per revision.

**Enforcement Point:** AB-D-003 (Revise Demand Understanding).

**Governed Policy:** PO-D-011.

**Outcome When Preconditions Are Not Satisfied:** The revision is deferred. The previous Published version remains authoritative.

**Traceability:** Owned by CA-D-001. Referenced by AB-D-003, FS-D-004.

### BR-D-401 – Authorised Forecasting Strategy

**Rule Statement:** The forecast for a series shall be produced using the forecasting strategy authorised for the current forecast cycle, as governed by PO-D-017.

**Rule:** Each forecast cycle selects an authorised forecasting strategy according to the model evaluation and governance rules defined in PO-D-017. Every forecast line within the Forecast Publication must be produced by the strategy authorised for that cycle. The authorised strategy may be a single model, an ensemble, or any other forecasting approach permitted by policy. The enterprise does not mandate a specific implementation; it mandates that the authorised strategy is used.

**Evaluation Scope:** Per forecast series, per cycle.

**Enforcement Point:** AB-D-007 (Produce Forecast Projection).

**Governed Policy:** PO-D-017.

**Outcome When Preconditions Are Not Satisfied:** The forecast cannot be produced until an authorised strategy is selected.

**Traceability:** Owned by CA-D-002. Referenced by AB-D-007, FS-D-006.


### BR-D-410 – Hierarchical Forecast Consistency

**Rule Statement:** All published forecasts shall satisfy hierarchical consistency as defined by the reconciliation method governed by PO-D-029.

**Rule:** After reconciliation, the bottom-up forecasts aggregated to any parent level shall equal the reconciled top-down forecast at that level, within the tolerance defined by the reconciliation method. The specific reconciliation algorithm and tolerance are governed by PO-D-029.

**Evaluation Scope:** Per Forecast Publication, before publication approval.

**Enforcement Point:** BA-D-002 (Produce Forecast Projection), at reconciliation step if performed; otherwise, the publication approval decision DE-D-004 may check for reconciliation completeness.

**Governed Policy:** PO-D-029.

**Outcome When Preconditions Are Not Satisfied:** If reconciliation is required but consistency is not met, the forecast publication is flagged and cannot be published automatically. Manual review is required.

**Traceability:** Owned by CA-D-002. Referenced by BA-D-002, DE-D-004, PO-D-029.

### BR-D-411 – Minimum Recurrence for Learning Derivation
**Rule Statement:** A learning shall only be derived when evidence demonstrates recurrence across the minimum number of periods defined by PO-D-048.
**Enforcement Point:** BA-D-013 (Derive Demand Learning).
**Governed Policy:** PO-D-048.

### BR-D-412 – Minimum Horizon Window for Learning Derivation
**Rule Statement:** The historical evidence evaluated for learning derivation must span the minimum horizon window defined by PO-D-048.
**Enforcement Point:** BA-D-013 (Derive Demand Learning).
**Governed Policy:** PO-D-048.

### BR-D-413 – Pattern Confidence Criteria for Learning Derivation
**Rule Statement:** Pattern Confidence and Intervention Confidence shall be assessed independently using the statistical criteria defined by PO-D-048.
**Enforcement Point:** BA-D-013 (Derive Demand Learning).
**Governed Policy:** PO-D-048.

### BR-D-414 – Intervention Impact Non-Negativity

**Rule Statement:** The Assessed Demand Lift in a Demand Intervention Impact must be non-negative.

**Rule:** A negative lift has no enterprise meaning in the current model. If the intervention is expected to reduce demand, the lift is zero and the reduction is handled by the consuming forecast capability through a separate governed mechanism.

**Evaluation Scope:** Per intervention impact, at creation.

**Enforcement Point:** AB-D-018 (Assess Demand Intervention Impact).

**Governed Policy:** PO-D-050.

**Outcome When Preconditions Are Not Satisfied:** The impact assessment is rejected. The algorithm must clamp to zero.

**Traceability:** Owned by CA-D-011. Referenced by AB-D-018, DE-D-014.

### BR-D-415 – Intervention Reference Validity

**Rule Statement:** The Intervention Reference in a Demand Intervention Impact must point to an active Scenario Adjustment.

**Rule:** An impact assessment for an inactive or retired intervention has no enterprise meaning. Publication is blocked if the intervention is no longer active.

**Evaluation Scope:** Per intervention impact, at publication.

**Enforcement Point:** DE-D-014 (Approve Intervention Impact Publication).

**Governed Policy:** PO-D-050.

**Outcome When Preconditions Are Not Satisfied:** Publication is blocked. Outcome is Do Not Publish.

**Traceability:** Owned by CA-D-011. Referenced by DE-D-014, FS-D-019.

---

# Chapter 8 — Policy Model

## Policy Specification Standard

Every Demand Intelligence policy follows the canonical structure established for the Medhavi platform:

| Section | Mandatory | Description |
|---------|-----------|-------------|
| **Purpose** | Yes | Why this policy exists. |
| **Governance Intent** | Yes | The enterprise governance principle being enforced. |
| **Governance Outcome** | Yes | The result of the policy being applied. |
| **Scope** | Yes | What artifacts and decisions the policy governs. |
| **Governed Configuration** | Yes | The parameters whose values realise the policy intent, with their business meaning and constraints. Configurable defaults are enterprise baselines; operational configuration per Planning Scope or entity is owned by the responsible capability. |
| **Authority Specification Contract** | Yes | Who owns the policy and under what authority it is authoritative. |
| **Lifecycle Specification Contract** | Yes | The governed states of the policy artifact itself (Active, Deprecated, Retired). |
| **Governed Rules** | Yes | The specific Business Rules (BR-D-xxx) this policy governs. |
| **Exceptional Conditions** | Yes | What happens when the policy cannot be applied as intended. |
| **Traceability** | Yes | Upward traceability to owning capability, and downward to decisions and algorithms. |

---

## PO-D-001 – Demand Data Acceptance Policy

**Purpose:** Govern the criteria and governance actions for accepting, quarantining, or rejecting incoming demand observations.

**Governance Intent:** Ensure that only trustworthy demand data contributes to the Enterprise Picture and, through it, to the Demand Understanding. Define the automated acceptance boundaries and the escalation path for marginal data.

**Governance Outcome:** Demand observations are either Accepted (eligible for incorporation), Quarantined (held for manual review), or Rejected (permanently excluded).

**Scope:** All Demand Observations (SE-D-001). Applies to the Accept Demand Observation decision (DE-D-001).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Maximum data latency | Configurable (default 60 minutes) | The maximum allowed difference between Observation Time and Business Time. |
| Minimum source reliability | Configurable (default 90%) | The historical reliability score a source must meet for automatic acceptance. |
| Duplicate detection window | Configurable (default 24 hours) | The time window within which identical records are treated as duplicates. |
| Demand Signal Quality Index weights | Completeness 40%, Timeliness 30%, Source Reliability 30% (configurable) | Weights for composite signal quality computation. |
| Consistency weight | Configurable (default 0%) | Weight for consistency dimension in Demand Signal Quality Index computation. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Understand Demand (CA-D-001) |
| **Authoritative Representation** | The enterprise’s definition of trustworthy demand data. |
| **Business Responsibility** | Govern the acceptance boundaries for demand observations. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-001. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification by APS Architecture Governance Board. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to any governed parameter require a new policy version.

### Governed Rules
BR-D-200 (Demand Signal Timeliness), BR-D-201 (Demand Quantity Range Validity), BR-D-202 (Source Reliability Threshold), BR-D-203 (Duplicate Data Detection), BR-D-210 (Received State Prerequisite), BR-D-211 (Observation Existence Prerequisite).

### Exceptional Conditions
- If automatic acceptance is not possible (borderline source reliability, marginal timeliness), the observation is Quarantined and the Demand Data Steward is notified.
- If data is clearly erroneous (negative quantity, duplicate, source below absolute minimum), the observation is Rejected.

### Traceability
- **Owned By:** CA-D-001.
- **Referenced By:** DE-D-001, FS-D-002.

---

## PO-D-011 – Demand Understanding Materiality Policy

**Purpose:** Govern when the enterprise shall publish a new version of the Demand Understanding, ensuring that publication occurs only for changes in demand interpretation that meaningfully affect planning or governance.

**Governance Intent:** Prevent over-versioning of the Demand Understanding by requiring a governed materiality assessment before publication. A new version shall be published only when the enterprise’s interpretation of demand has changed in a way that would reasonably cause a downstream capability to make a different decision.

**Governance Outcome:** A new Demand Understanding version shall be published only when one or more interpretation dimensions have changed materially, or when a Periodic Refresh is required by PO-D-012.

**Scope:** All Planning Scopes. Applies to the Publish Demand Understanding decision (DE-D-002).

### 1. Enterprise Principle
The Demand Understanding is the authoritative interpretation of current demand. Publishing a new version creates downstream planning consequences. To preserve trust and stability, publication shall not occur for trivial or transient changes. Materiality is an enterprise judgment that a change in the interpretation of demand warrants formal publication.

### 2. Materiality Definition
A change in the Demand Understanding is material when, in at least one interpretation dimension, the nature, magnitude, or significance of the change would reasonably cause a downstream capability to make a different decision or reach a different conclusion than it would using the currently published version.

### 3. Interpretation Dimensions
Materiality is assessed independently across the four interpretation dimensions:
- **Demand Continuity Interpretation** — the enterprise’s assessment of current demand patterns and their persistence.
- **Demand Pattern Interpretation** — the enterprise’s assessment of the structure and predictability of current demand.
- **Demand Health Interpretation** — the enterprise’s assessment of whether the current demand understanding is reliable.
- **Demand Volatility Interpretation** — the enterprise’s synthesis of how uncertain the current demand picture is.

> **Architectural Note:** All interpretation dimension statuses (e.g., Stable, Volatile, Normal, Seasonal, Irregular, Step-Change, Healthy, At Risk, Critical, Low, Medium, High) must be formally registered as governed identifier entries in SE-C-037 (Enterprise Governed Vocabulary).

### 4. Mandatory Publication Conditions
The following conditions shall always be considered Material:
- Demand Continuity Interpretation transitions from Stable to Volatile, or vice versa.
- Demand Pattern Interpretation transitions between any of the statuses Normal, Seasonal, Irregular, or Step-Change.
- Demand Health Interpretation transitions to At Risk or Critical.
- Demand Volatility Interpretation changes by more than one level (Low to High, or vice versa).

### 5. Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Mandatory interpretation dimensions | Demand Continuity, Demand Pattern, Demand Health, Demand Volatility | All four dimensions must be present in a Published Demand Understanding. |
| Interpretation completeness threshold | All four mandatory dimensions (Demand Continuity, Demand Pattern, Demand Health, Demand Volatility) must be present and each must include a status. A dimension may have a status of “Incomplete” if the required evidence for that dimension is unavailable. A Published version requires that no dimension is “Incomplete”. A Draft version may carry “Incomplete” dimensions; the publication decision (DE-D-002) will enforce the rule that publication is deferred until all dimensions are complete. | Applies to every interpretation dimension. A dimension may carry an "Incomplete" status if evidence is missing, but the dimension itself must be present. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Understand Demand (CA-D-001) |
| **Authoritative Representation** | The enterprise’s definition of what constitutes a material change in demand interpretation. |
| **Business Responsibility** | Ensure that the Demand Understanding is published only when meaningful interpretation change occurs. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-002. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect and governs materiality decisions. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to thresholds or mandatory conditions require a new policy version.

### Governed Rules
BR-D-204 (Material Change Required), BR-D-205 (Interpretation Completeness).

### Exceptional Conditions
- If materiality assessment cannot be performed, publication is deferred and the Demand Data Steward notified.
- If a mandatory publication condition is triggered but interpretation completeness is below threshold, the decision is escalated to the Demand Data Steward.

### Traceability
- **Owned By:** CA-D-001.
- **Referenced By:** DE-D-002, FS-D-004.

---

## PO-D-012 – Demand Understanding Publication Cadence Policy

**Purpose:** Govern the maximum allowed interval between publications of the Demand Understanding, ensuring that downstream consumers operate from a recent, governed baseline even in the absence of material change.

**Governance Intent:** Prevent staleness of the Demand Understanding by requiring periodic publication when no material change has occurred within a defined interval.

**Governance Outcome:** A new Demand Understanding version shall be published at least once within the maximum cadence interval, even if no material change is detected, with the publication flagged as a Periodic Refresh.

**Scope:** All Planning Scopes. Applies to the Publish Demand Understanding decision (DE-D-002).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Maximum publication interval | 24 hours (configurable per Planning Scope) | The longest allowed gap between Published versions. |
| Staleness warning threshold | 12 hours | If no publication occurs within this window, the Demand Data Steward is notified. |
| Override authority | Demand Data Steward | May force publication at any time. |
| Periodic Refresh flag | Required | Every publication due to cadence alone must carry this flag. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Understand Demand (CA-D-001) |
| **Authoritative Representation** | The enterprise’s definition of maximum acceptable staleness for the Demand Understanding. |
| **Business Responsibility** | Ensure periodic refresh of the Demand Understanding. |
| **Authority Scope** | Per Planning Scope. |
| **Intended Consumers** | DE-D-002. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to the maximum interval or warning threshold require a new policy version.

### Governed Rules
BR-D-204 (Material Change Required), BR-D-205 (Interpretation Completeness).

### Exceptional Conditions
- If unable to publish within the maximum interval, the Demand Data Steward is notified and staleness is documented.
- Force-published understandings are flagged accordingly.

### Traceability
- **Owned By:** CA-D-001.
- **Referenced By:** DE-D-002, FS-D-004.

---

## PO-D-017 – Forecast Model Governance Policy

**Purpose:** Govern the automatic promotion of challenger forecasting models to champion status, ensuring that only models demonstrating statistically significant improvement without harm are promoted.

**Governance Intent:** Ensure that every forecast cycle uses the most accurate available model, with governance to prevent degradation on bias, stability, or high-priority items.

**Governance Outcome:** A champion model is selected for each forecast cycle. Promotion is automatic if all criteria are met; otherwise escalated to the Demand Manager.

**Scope:** All forecast cycles. Applies to the authorised forecasting strategy selection governed by BR-D-401.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Minimum evaluation period | 4 weeks (configurable) | Minimum number of weeks a challenger must be evaluated. |
| Statistical significance threshold | p ≤ 0.05 | WAPE improvement must be statistically significant. |
| Bias degradation tolerance | ±0.5 percentage points (configurable) | Maximum allowed increase in absolute bias. |
| Stability degradation tolerance | −5 percentage points (configurable) | Maximum allowed decrease in forecast stability. |
| High-priority WAPE protection threshold | +1 percentage point (configurable) | Maximum WAPE increase on high-priority items. |
| Rollback window | 4 weeks after promotion (configurable) | Period during which rollback to previous champion is permitted. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s definition of acceptable model promotion criteria. |
| **Business Responsibility** | Govern the promotion of forecasting models. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | The authorised forecasting strategy selection. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to thresholds require a new policy version.

### Governed Rules
BR-D-401 (Authorised Forecasting Strategy).

### Exceptional Conditions
- If no challenger results are available, the current champion is retained.
- If within the rollback window the new champion causes a service-level drop attributable to forecast degradation, the Demand Manager may rollback.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** AB-D-006, FS-D-008.

---

## PO-D-019 – Unforecastable Series Policy

**Purpose:** Govern the handling of demand series that do not have sufficient history for statistical forecasting.

**Governance Intent:** Ensure that every series receives a forecast or a documented reason why it cannot be forecast, and that fallback methods are applied consistently.

**Governance Outcome:** Series with insufficient history are assigned a fallback method based on product lifecycle stage, or flagged for planner attention if no method is viable.

**Scope:** All forecast cycles. Applies to the Generate Forecast for Series decision (DE-D-003).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Minimum history periods for statistical forecast | 12 periods (configurable) | Minimum demand history length. |
| Fallback method preference order | Analog, Attribute-based, Lifecycle model, Launch curve, Expert judgment | Order of preference for fallback method selection. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s definition of how unforecastable series are handled. |
| **Business Responsibility** | Govern the assignment of fallback methods. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-003. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to minimum history or fallback order require a new policy version.

### Governed Rules
BR-D-206 (Forecast Data Sufficiency).

### Exceptional Conditions
- If no fallback method is viable, the series is flagged as unforecastable and the Demand Planner is notified.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** DE-D-003, FS-D-007.

---

## PO-D-020 – Forecast Publication Governance Policy

**Purpose:** Govern the automatic approval, escalation, and suppression of Forecast Publications based on the Forecast Confidence Index and completeness thresholds.

**Governance Intent:** Ensure that forecasts meeting quality thresholds are published automatically, while those that do not receive appropriate governance review.

**Governance Outcome:** A Forecast Publication is published automatically, routed for planner approval, or suppressed.

**Scope:** All Forecast Publications. Applies to the Approve Forecast Publication decision (DE-D-004).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Auto-publication Confidence Index threshold | ≥ 70 (configurable) | Forecast Confidence Index must meet or exceed this value for automatic publication. |
| Minimum Confidence Index for publication | ≥ 50 (configurable) | Below this, publication is suppressed. |
| Completeness threshold | ≥ 95% (configurable) | Percentage of covered series that must have valid forecasts. |
| Forecast Confidence Index weights | Model confidence 50%, Data completeness 30%, Signal quality 20% (configurable) | Weights for composite index computation. |
| Borderline confidence band | Configurable (default 2%) | If the Forecast Confidence Index is within this percentage of the auto-publication threshold, the publication is routed to the Demand Planner. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s definition of acceptable forecast publication quality. |
| **Business Responsibility** | Govern the publication approval of forecast publications. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-004. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to thresholds or weights require a new policy version.

### Governed Rules
BR-D-207 (Completeness Threshold for Publication).

### Exceptional Conditions
- If the Forecast Confidence Index is borderline (within 2% of threshold), the publication is routed to the Demand Planner.
- If assumptions are not signed off, publication is suppressed regardless of confidence.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** DE-D-004, FS-D-008.

---

## PO-D-022 – Forecast Override Authorization Policy

**Purpose:** Govern the authorization of planner overrides to system-generated forecast values.

**Governance Intent:** Ensure that overrides are justified, within governed deviation limits, and properly authorised.

**Governance Outcome:** Overrides are accepted, escalated, or rejected.

**Scope:** All Forecast Publications in Draft state. Applies to the Evaluate Forecast Override decision (DE-D-005).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Maximum deviation limit | ±30% (configurable) | Maximum allowed percentage deviation from the system forecast. |
| Demand Manager deviation authority | Unlimited with justification | Demand Managers may override the deviation limit. |
| Planner role authorization | Demand Planner | Role required to submit overrides. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s definition of acceptable forecast overrides. |
| **Business Responsibility** | Govern the authorization of planner overrides. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-005. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to deviation limits require a new policy version.

### Governed Rules
BR-D-208 (Override Justification Requirement), BR-D-209 (Override Deviation Limit).

### Exceptional Conditions
- If a planner is not authorised, the override is rejected.
- If the justification is present but of questionable sufficiency, the override is escalated to the Demand Planner.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** DE-D-005, FS-D-007.

---

## PO-D-023 – Override Audit Policy

**Purpose:** Govern the audit and review of planner overrides to detect systematic bias and ensure accountability.

**Governance Intent:** Ensure that all overrides are logged, traceable, and subject to periodic review.

**Governance Outcome:** All overrides are permanently recorded. Quarterly reviews assess override effectiveness and identify planner bias patterns.

**Scope:** All Forecast Overrides. Applies to the original value preservation governed by BR-D-109.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Review cadence | Quarterly | Frequency of override audit reviews. |
| Bias detection threshold | Override value-destroying rate > 50% over review period | Triggers planner coaching or policy review. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise’s definition of override audit requirements. |
| **Business Responsibility** | Govern the audit of planner overrides. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | Demand Planners and Managers. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.

### Governed Rules
BR-D-109 (Original System Forecast Preservation on Override).

### Exceptional Conditions
- Override data is permanently retained regardless of review outcomes.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** AB-D-008.

---

## PO-D-031 – Demand Sensing Policy

**Purpose:** Govern the deviation thresholds, corroboration requirements, and state transition rules for demand behavior assessment.

**Governance Intent:** Ensure consistent detection of meaningful demand behavior changes across all monitored entities.

**Governance Outcome:** State changes are determined automatically based on governed thresholds. Critical changes are escalated to the Demand Manager.

**Scope:** All monitored Item-Locations. Applies to the Evaluate Demand Signal for State Change decision (DE-D-006).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Significant threshold | 2.5σ (configurable) | Deviation required for Elevated or Depressed state. |
| Critical threshold | 4σ (configurable) | Deviation required for Critical state. |
| Noise threshold | 1σ (configurable) | Signals below this are suppressed. |
| Corroboration minimum | ≥ 2 independent sources | Required for Critical state. |
| High-priority reduced threshold | 2σ (configurable) | Significant threshold for high-priority items. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Sense Demand (CA-D-003) |
| **Authoritative Representation** | The enterprise’s definition of meaningful demand behavior change. |
| **Business Responsibility** | Govern the detection of demand behavior changes. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-006. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to thresholds require a new policy version.

### Governed Rules
BR-D-300 (Deviation Thresholds), BR-D-301 (Corroboration Requirement), BR-D-302 (High-priority Sensitivity), BR-D-303 (Noise Suppression).

### Exceptional Conditions
- If no baseline exists, the first signal is treated as the new baseline, not a change.
- If signals conflict, confidence is reduced and the state remains at the current level until resolution.

### Traceability
- **Owned By:** CA-D-003.
- **Referenced By:** DE-D-006, FS-D-009.

---

## PO-D-032 – Forecast Refresh Trigger Policy

**Purpose:** Govern when a Critical demand behavior state triggers an automatic evaluation for an out-of-cycle forecast refresh.

**Governance Intent:** Ensure that Critical demand changes are evaluated promptly for their impact on forecast freshness.

**Governance Outcome:** Critical state changes automatically trigger a forecast refresh evaluation.

**Scope:** All Critical state transitions. Applies to the Trigger Forecast Refresh on Critical State decision (DE-D-007).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Forecast freshness threshold | Forecast age > 4 hours (configurable) | Forecast must be older than this to trigger refresh. |
| Minimum expected accuracy improvement | ≥ 2 percentage points WAPE (configurable) | Refresh must be expected to improve accuracy by at least this amount. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Sense Demand (CA-D-003) |
| **Authoritative Representation** | The enterprise’s definition of when a Critical state warrants forecast refresh. |
| **Business Responsibility** | Govern the trigger criteria for out-of-cycle forecast refresh. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-007. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to thresholds require a new policy version.

### Governed Rules
BR-D-304 (Forecast Refresh Evaluation on Critical State).

### Exceptional Conditions
- If expected improvement cannot be estimated, Critical items default to Trigger and others to Defer.

### Traceability
- **Owned By:** CA-D-003.
- **Referenced By:** DE-D-007, FS-D-010.

---

## PO-D-035 – Segmentation Policy Governance

**Purpose:** Govern the classification rules, thresholds, and re-evaluation triggers for all planning classification types.

**Governance Intent:** Ensure that planning classifications are determined by governed rules, not by implementation logic.

**Governance Outcome:** Classification rules for all types are governed by Planning Governance. Changes to the policy trigger automatic reclassification of affected entities.

**Scope:** All active classification types. Applies to the Determine Planning Classification decision (DE-D-008).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Classification rules per type | Defined per type in the Segmentation Policy | Rules that determine class assignment. |
| Re-evaluation triggers | Schedule, demand pattern change, policy update | Events that trigger reclassification. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Segment Demand (CA-D-004) |
| **Authoritative Representation** | The enterprise’s definition of planning classification rules. |
| **Business Responsibility** | Govern the classification of planning entities. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-008. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to classification rules or thresholds require a new policy version.

### Governed Rules
BR-D-305 (Classification by Policy), BR-D-306 (Unclassified Assignment).

### Exceptional Conditions
- If minimum evidence is not met, the entity is classified as Unclassified.

### Traceability
- **Owned By:** CA-D-004.
- **Referenced By:** DE-D-008, FS-D-011.

---

## PO-D-041 – Forecast Measurement Policy

**Purpose:** Govern the mandatory and optional forecast quality metrics, their definitions and formulas, evaluation cadence, publication criteria, and completeness thresholds.

**Governance Intent:** Ensure consistent, governed evaluation of forecast quality across all Planning Scopes.

**Governance Outcome:** Forecast Quality Assessments are produced with standardised metrics and published on the governed cadence.

**Scope:** All Planning Scopes. Applies to the Publish Forecast Quality Assessment decision (DE-D-011).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Mandatory metrics | WAPE, Forecast Bias, Forecast Accuracy | Metrics that must be included in every assessment. |
| Optional metrics | MAPE, FVA, Forecast Stability, Override Effectiveness | Metrics included per policy configuration. |
| Evaluation cadence – operational | Weekly | Cadence for operational horizon assessments. |
| Evaluation cadence – tactical | Monthly | Cadence for tactical horizon assessments. |
| Minimum evaluation period | 4 weeks (configurable) | Shortest period for a valid assessment. |
| Completeness threshold | ≥ 95% of series must have actual demand data | Threshold for publication. |
| WAPE tolerance for accuracy classification | Excellent < 10%, Good 10–20%, Warning 20–30%, Critical > 30% (configurable) | Thresholds for accuracy interpretation. |
| Forecast Accuracy tolerance | Configurable (default ±10%) | The absolute percentage error tolerance for Forecast Accuracy computation. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Evaluate Demand Quality (CA-D-007) |
| **Authoritative Representation** | The enterprise’s definition of forecast quality measurement rules. |
| **Business Responsibility** | Govern the evaluation of forecast quality. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-011. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to metrics, formulas, or thresholds require a new policy version.

### Governed Rules
BR-D-403 (Quality Metrics Derivation).

### Exceptional Conditions
- If completeness is below threshold, the assessment is suppressed and the Demand Manager notified.
- If the evaluation period is shorter than the minimum, publication is suppressed.

### Traceability
- **Owned By:** CA-D-007.
- **Referenced By:** DE-D-011, FS-D-014.

---

## PO-D-044 – Demand Exception Evidence Policy

**Purpose:** Govern the exception types, detection thresholds, severity rules, and resolution criteria for demand exception evidence published to Core Exception Management.

**Governance Intent:** Ensure that only situations meeting enterprise-defined criteria are raised as formal demand planning conditions.

**Governance Outcome:** Demand exception evidence is detected, classified by severity, and published to Core Exception Management according to governed rules. Resolution evidence is published when the underlying data returns to within governed thresholds.

**Scope:** All planning entities. Applies to the Evaluate Demand Planning Condition decision (DE-D-012).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Condition types and triggering criteria | Defined per condition type in the Exception Detection Policy | The set of recognised condition types and their detection rules. |
| Severity dimensions | Business Impact, Urgency, Scope | Dimensions assessed for each condition. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Detect Demand Exceptions (CA-D-008) |
| **Authoritative Representation** | The enterprise’s definition of demand exception detection. |
| **Business Responsibility** | Govern the detection, classification, and resolution of demand planning conditions. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | DE-D-012. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to condition types or thresholds require a new policy version.

### Governed Rules
BR-D-311 (Condition Detection Evidence).

### Exceptional Conditions
- If evidence is contradictory, the condition is created at the lower severity and flagged for manual review.
- If a new condition type is added without historical data, the first occurrence is manually reviewed.

### Traceability
- **Owned By:** CA-D-008.
- **Referenced By:** DE-D-012, FS-D-015.

---

## PO-D-047 – Explanation Governance Policy

**Purpose:** Govern the completeness, determinism, and traceability requirements for demand explanations.

**Governance Intent:** Ensure that every demand intelligence conclusion carries a complete, deterministic, and traceable explanation composed exclusively from preserved historical evidence.

**Governance Outcome:** Demand Explanations (SE-D-010) are complete per the governed template, deterministic, and traceable to preserved evidence at historical versions.

**Scope:** All Demand Explanations (SE-D-010). Applies to BA-D-012 and AB-D-016.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Explanation completeness definition | A Structured Reasoning Graph is complete when it contains all required nodes and edges per the applicable template for the artifact type. | Governs the Explainability Score (PI-D-107) numerator. |
| Required template elements | Evidence nodes, Decision nodes, Policy nodes, Assumption nodes, typed edges (Influenced, Determined, Governed By). | The canonical reasoning structure. |
| Evidence preservation requirement | All contributing evidence, decisions, policies, and assumptions must be referenced at their historical versions in effect when the explained conclusion was reached. | Ensures the explanation reflects the state at conclusion time. |
| Determinism requirement | Identical artifact, evidence, policies, and template produce an identical Structured Reasoning Graph. | Ensures reproducibility (CN-002). |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Explain Demand (CA-D-009) |
| **Authoritative Representation** | The enterprise’s definition of explanation completeness and governance. |
| **Business Responsibility** | Govern the composition and completeness of demand explanations. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | BA-D-012, AB-D-016, PI-D-107. |
| **Non-Intended Consumers** | None. |
| **Supersedes** | None. |
| **Superseded By** | None. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy is in effect. | Ratification. |
| Deprecated | Still valid but planned for replacement. | New policy ratified. |
| Retired | No longer in effect. | Superseding policy active. |

- **Terminal States:** Retired.
- **History Preservation:** All versions retained permanently.
- **Versioning Rules:** Changes to completeness definition or template elements require a new policy version.

### Governed Rules
BR-D-124 (Explanation Immutability).

### Exceptional Conditions
- If preserved evidence is incomplete, the explanation is marked “Unavailable – Incomplete Evidence” and persisted for audit.
- If no template exists for an artifact type, the default template is applied and the explanation is flagged.

### Traceability
- **Owned By:** CA-D-009.
- **Referenced By:** BA-D-012, AB-D-016, PI-D-107, FS-D-016.

---

## PO-D-021 – Demand Manager Override Policy

**Purpose:** Govern the authority and conditions under which a Demand Manager may override publication decisions or deviation limits.

**Governance Intent:** Provide an authoritative escalation mechanism for exceptional business situations while requiring explicit documented justification.

**Governance Outcome:** Demand Managers may authorize publication of forecasts below auto-publication thresholds or accept planner overrides exceeding standard deviation limits.

**Scope:** All Forecast Publications and Forecast Overrides. Applies to `DE-D-004` and `DE-D-005`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Manager Override Deviation Authority | Unlimited with mandatory justification | Deviation limit for Demand Manager approval. |
| Mandatory Justification | Required | Non-empty business reason required for manager override. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | The enterprise's definition of Demand Manager decision authority. |
| **Business Responsibility** | Govern manager override actions. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | `DE-D-004`, `DE-D-005`. |

### Lifecycle Specification Contract

| State | Description | Transition Trigger |
|-------|-------------|-------------------|
| Active | Policy in effect. | Ratification. |
| Deprecated | Superseded by new version. | New policy ratified. |
| Retired | No longer active. | Retirement trigger. |

- Terminal States: Retired.
- History Preservation: All versions retained permanently.

### Governed Rules
`BR-D-207` (Completeness Threshold), `BR-D-209` (Override Deviation Limit).

### Exceptional Conditions
- Overrides without written justification are rejected.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `DE-D-004`, `DE-D-005`, `FS-D-007`, `FS-D-008`.

---

## PO-D-024 – Forecast Publication Generation Governance Policy

**Purpose:** Govern the authorization and initiation rules for regular scheduled and out-of-cycle forecast publication generation contexts.

**Governance Intent:** Ensure forecast cycles run on governed cadences while allowing emergency refreshes for Critical demand changes.

**Governance Outcome:** Forecast publication generation context initiation requests are validated and authorized.

**Scope:** All Forecast Cycles. Applies to `CR-D-005`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Scheduled Cadence | Daily / Weekly (configurable) | Standard cadence for automatic cycle initiation. |
| Out-of-Cycle Initiation Authority | Demand Manager or Critical State Event | Roles / events authorized to initiate out-of-cycle forecast. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | Enterprise definition of forecast cycle initiation governance. |
| **Business Responsibility** | Govern cycle initiation. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | `AB-D-005`, `FS-D-005`. |

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
BR-D-128 (Single Active Forecast Cycle per Scope and Horizon).

### Exceptional Conditions
- Unassigned out-of-cycle requests are rejected.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `AB-D-005`, `FS-D-005`.

---

## PO-D-025 – Forecast Assumption Sign-off Policy

**Purpose:** Govern the mandatory review and sign-off of commercial and operational forecast assumptions before publication.

**Governance Intent:** Ensure all key assumptions influencing the demand forecast are explicitly declared and approved.

**Governance Outcome:** Forecast Publications cannot be published automatically or manually without completed assumption sign-off.

**Scope:** All Draft Forecast Publications. Applies to `DE-D-004`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Mandatory Assumption Categories | Commercial, Promotional, Macroeconomic | Categories requiring explicit sign-off. |
| Sign-off Authority | Demand Planner / Manager | Required role for assumption sign-off. |
| Scenario Adjustment Reference | Required | All forecast assumptions shall be traceable to the Scenario Adjustments that informed them. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Forecast Demand (CA-D-002) |
| **Authoritative Representation** | Enterprise policy for forecast assumption sign-off. |
| **Business Responsibility** | Govern assumption compliance. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | `DE-D-004`, `FS-D-008`. |

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
BR-D-207 (Completeness Threshold for Publication).
Additionally, the assumption sign-off itself is enforced by the workflow – if not complete, DE-D-004 returns Suppress.

### Exceptional Conditions
- Missing assumption sign-off suppresses publication.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `DE-D-004`, `FS-D-008`.

---

## PO-D-028 – New Product Forecast Policy

**Purpose:** Govern the approval and assignment of forecasting methods for new products lacking demand history.

**Governance Intent:** Ensure new products receive governed launch curves, analog projections, or expert judgment forecasts.

**Governance Outcome:** New product forecast methods are authorized by the Demand Manager.

**Scope:** All new product items (`SE-C-001`).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| New Product History Threshold | $< 12$ periods | Definition of new product. |
| Approval Role | Demand Manager | Required sign-off for new product forecast models. |

### Authority Specification Contract
Owner: CA-D-002. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-206` (Forecast Data Sufficiency).

### Exceptional Conditions
- New products without assigned analog or launch model trigger planner alert.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `PO-D-019`, `FS-D-006`.

---

## PO-D-029 – Forecast Reconciliation Policy

**Purpose:** Govern automatic vs. manual reconciliation of bottom-up and top-down forecasts across product and location hierarchies.

**Governance Intent:** Ensure spatial and structural reconciliation follows governed mathematical disaggregation rules.

**Governance Outcome:** Hierarchical forecasts are reconciled consistently across all scopes.

**Scope:** All hierarchical Forecast Publications.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Reconciliation Algorithm | Proportional Disaggregation / Optimal Combination | Governed method for hierarchy alignment. |
| Historical Confidence Threshold | $\ge 80\%$ | Threshold for automatic reconciliation. |

### Authority Specification Contract
Owner: CA-D-002. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
BR-D-410 (Hierarchical Forecast Consistency).

### Exceptional Conditions
- Disaggregation mismatch triggers manual reconciliation queue.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `FS-D-006`.

---

## PO-D-034 – Forecast Refresh Execution Policy

**Purpose:** Govern whether a Critical demand behavior state triggers a partial or full forecast refresh.

**Governance Intent:** Balance forecast freshness against compute overhead and planning instability.

**Governance Outcome:** Critical state triggers partial refresh for affected series automatically; full refresh requires manager sign-off.

**Scope:** All Critical demand sensing triggers.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Partial Refresh Scope | Affected Item-Locations only | Automatic execution scope. |
| Full Refresh Approval | Demand Manager | Required approval for full scope refresh. |

### Authority Specification Contract
Owner: CA-D-002. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-304` (Forecast Refresh Evaluation).

### Exceptional Conditions
- Emergency full refresh authorized during Critical state events.

### Traceability
- **Owned By:** CA-D-002.
- **Referenced By:** `DE-D-007`, `FS-D-010`.

---

## PO-D-036 – Segmentation Override Review Policy

**Purpose:** Govern the review and audit of manual planner overrides to ABC/XYZ planning classifications.

**Governance Intent:** Prevent ungrounded classification overrides and ensure quarterly review of segment stability.

**Governance Outcome:** Planner overrides require non-empty business justification and expire after 90 days unless ratified.

**Scope:** All Planning Classification Assignments (`SE-D-005`).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Override Expiration | 90 days | Maximum duration of unratified classification override. |
| Review Cadence | Quarterly | Cadence for audit review. |

### Authority Specification Contract
Owner: CA-D-004. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-305` (Classification by Policy).

### Exceptional Conditions
- Expired overrides automatically revert to rule-based classification.

### Traceability
- **Owned By:** CA-D-004.
- **Referenced By:** `DE-D-008`, `FS-D-011`.

---

## PO-D-037 – Classification Policy Governance

**Purpose:** Govern the demand behavior classification dimensions, statistical thresholds, and re-evaluation rules.

**Governance Intent:** Ensure demand behavior classification (Continuous, Intermittent, Seasonal, Lumpy, Trend) is determined by ratified enterprise rules.

**Governance Outcome:** Statistical pattern classification rules are governed centrally. Policy updates trigger automatic re-classification.

**Scope:** All Demand Behavior Assignments (`SE-D-006`). Applies to `DE-D-009` and `BA-D-006`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Intermittent Demand Threshold | Inter-arrival interval $> 1.34$ periods | Threshold for intermittent demand pattern. |
| Lumpy Demand Threshold | $CV^2 > 0.49$ and Inter-arrival $> 1.34$ | Threshold for lumpy demand pattern. |
| Seasonal Demand Threshold | Autocorrelation $p < 0.01$ at seasonal lag | Threshold for seasonal demand pattern. |

### Authority Specification Contract
Owner: CA-D-005. Scope: Enterprise-wide. Intended Consumers: `DE-D-009`, `BA-D-006`.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-307` (Behavior Classification by Policy), `BR-D-308` (Unclassified Minimum Evidence).

### Exceptional Conditions
- Insufficient history defaults classification to Unclassified.

### Traceability
- **Owned By:** CA-D-005.
- **Referenced By:** `DE-D-009`, `BA-D-006`, `FS-D-012`.

---

## PO-D-038 – Classification Override Review Policy

**Purpose:** Govern the quarterly audit of planner overrides to demand behavior classifications.

**Governance Intent:** Audit planner overrides to ensure statistical pattern overrides are justified by real-world market intelligence.

**Governance Outcome:** Behavior classification overrides are logged and audited quarterly.

**Scope:** All Demand Behavior Assignments (`SE-D-006`).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Audit Cadence | Quarterly | Review frequency. |
| Reversion Threshold | Override accuracy $< 50\%$ | Triggers automatic override removal. |

### Authority Specification Contract
Owner: CA-D-005. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-307` (Behavior Classification by Policy).

### Exceptional Conditions
- System reverts overrides that consistently destroy forecasting model accuracy.

### Traceability
- **Owned By:** CA-D-005.
- **Referenced By:** `DE-D-009`, `FS-D-012`.

---

## PO-D-039 – Prioritization Policy Governance

**Purpose:** Govern the scoring methodology, dimension weights, and level thresholds for planning priority assignments.

**Governance Intent:** Ensure enterprise planning priority is computed deterministically from governed business dimensions.

**Governance Outcome:** Entities are assigned priority levels (Critical, High, Medium, Low) based on governed weighted scores.

**Scope:** All Planning Priority Assignments (`SE-D-007`). Applies to `DE-D-010` and `BA-D-007`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Dimension Weights | Revenue 30%, Strategy 25%, Risk 20%, Contractual 25% | Weights for priority score computation. |
| Level Thresholds | Critical $\ge 80$, High $\ge 60$, Medium $\ge 40$, Low $< 40$ | Thresholds for priority level mapping. |

### Authority Specification Contract
Owner: CA-D-006. Scope: Enterprise-wide. Intended Consumers: `DE-D-010`, `BA-D-007`.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-309` (Priority Scoring Policy), `BR-D-310` (Unclassified Priority Evidence).

### Exceptional Conditions
- Missing mandatory evidence assigns Unclassified priority.

### Traceability
- **Owned By:** CA-D-006.
- **Referenced By:** `DE-D-010`, `BA-D-007`, `FS-D-013`.

---

## PO-D-040 – Priority Override Review Policy

**Purpose:** Govern planner overrides to planning priority levels.

**Governance Intent:** Require non-empty business justification and quarterly management review for priority overrides.

**Governance Outcome:** Priority overrides are tracked, audited, and expired per policy.

**Scope:** All Planning Priority Assignments (`SE-D-007`).

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Override Approval Role | Demand Manager | Required role for priority escalation to Critical. |
| Review Cadence | Quarterly | Audit cadence. |

### Authority Specification Contract
Owner: CA-D-006. Scope: Enterprise-wide.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
`BR-D-309` (Priority Scoring Policy).

### Exceptional Conditions
- Unjustified overrides are rejected.

### Traceability
- **Owned By:** CA-D-006.
- **Referenced By:** `DE-D-010`, `FS-D-013`.

---

## PO-D-048 – Learning Analysis Policy

**Purpose:** Govern the minimum recurrence thresholds, evidence sufficiency criteria, and confidence evaluation rules for enterprise demand learnings.

**Governance Intent:** Ensure enterprise learnings are derived exclusively from statistically sound, multi-period recurring demand evidence.

**Governance Outcome:** Candidate learnings (`SE-D-011`) are derived deterministically and submitted for governance review.

**Scope:** All Demand Learnings (`SE-D-011`). Applies to `BA-D-013` and `FS-D-017`.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Minimum Recurrence Threshold | $\ge 3$ consecutive planning periods | Minimum occurrence count for learning derivation. |
| Minimum Horizon Window | 3 quarters (configurable) | Window spanning multi-period evidence. |
| Pattern Confidence Criteria | High $\ge 80\%$, Medium $\ge 60\%$, Low $< 60\%$ | Classification threshold for pattern confidence. |

### Authority Specification Contract
Owner: Learn From Demand (CA-D-010). Scope: Enterprise-wide. Intended Consumers: `BA-D-013`, `FS-D-017`.

### Lifecycle Specification Contract
Active $\rightarrow$ Deprecated $\rightarrow$ Retired. Terminal: Retired. History preserved.

### Governed Rules
- BR-D-411 (Minimum Recurrence for Learning Derivation)
- BR-D-412 (Minimum Horizon Window for Learning Derivation)
- BR-D-413 (Pattern Confidence Criteria for Learning Derivation)

### Exceptional Conditions
- Single-period events cannot produce enterprise learnings.

### Traceability
- **Owned By:** CA-D-010.
- **Referenced By:** `BA-D-013`, `FS-D-017`.

## PO-D-050 – Intervention Modeling Governance

**Purpose:** Govern the modeling approach, confidence thresholds, and publication criteria for demand intervention impact assessments.

**Governance Intent:** Ensure that intervention impact assessments are computed using governed approaches, meet minimum confidence standards, and are published only when reliable.

**Governance Outcome:** Demand Intervention Impacts are computed deterministically, meet confidence thresholds, and are published as authoritative adjustments for forecast consumption.

**Scope:** All Demand Intervention Impacts (SE-D-018). Applies to AB-D-018, AB-D-019, DE-D-014, BA-D-016, FS-D-018, FS-D-019.

### Governed Configuration

| Parameter | Value | Description |
|-----------|-------|-------------|
| Publication confidence threshold | Configurable (default 70%) | Minimum Lift Confidence for automatic publication. |
| Minimum historical periods for elasticity | Configurable (default 12 periods) | Minimum forecast-actual pairs required for elasticity estimation. |
| Modeling approach preference order | Governed list | Ordered list of approved modeling approaches (e.g., Historical Elasticity, Analog-Based, Expert Judgment). |
| Maximum temporal validity | Configurable (default 90 days) | Maximum duration for an intervention's temporal validity window. |

### Authority Specification Contract

| Section | Value |
|---------|-------|
| **Business Owner** | Model Demand Interventions (CA-D-011) |
| **Authoritative Representation** | The enterprise's definition of intervention modeling governance. |
| **Business Responsibility** | Govern modeling approach selection, confidence thresholds, and publication criteria. |
| **Authority Scope** | Enterprise-wide. |
| **Intended Consumers** | CA-D-011, CA-D-002. |

### Lifecycle Specification Contract

| State | Description |
|-------|-------------|
| Active | Policy is in effect. |
| Deprecated | Still valid but planned for replacement. |
| Retired | No longer in effect. |

- Terminal State: Retired.
- History Preservation: All versions retained permanently.

### Governed Rules

- BR-D-414 (Intervention Impact Non-Negativity)
- BR-D-415 (Intervention Reference Validity)

### Exceptional Conditions

- If historical data is insufficient for elasticity estimation, the fallback approach from the preference order is applied.
- If no approach is viable, the impact is flagged for planner attention with confidence zero.

### Traceability

- **Owned By:** CA-D-011.
- **Referenced By:** AB-D-018, AB-D-019, DE-D-014, BA-D-016, FS-D-018, FS-D-019.

---

# Chapter 9 — Functional Specifications

## Understand Demand Functional Specifications

### FS-D-001 – Receive Demand Observation

**Realises:** CR-D-001

#### Business Contract
- **Consumes:** Demand signal from a source system or internal notification (e.g., BN-D-011 Forecast Published).
- **Produces:** Demand Observation (SE-D-001) in Received state.
- **Transitions:** SE-D-001: (none) → Received.
- **Publishes:** BN-D-005 Demand Observation Received.
- **Invokes:** AB-D-001.
- **Guarantees:** Exactly one Demand Observation established with full provenance. Duplicate business observations rejected.

#### Trigger
Demand signal received from a source system, or an internal notification indicating demand-relevant information has been published.

#### Preconditions
- The signal contains sufficient information to construct a unique Demand Observation identity.
- Referenced Item (SE-C-001) and Location (SE-C-002) exist and are active.

#### Semantic Objects
- **Read:** SE-C-001, SE-C-002.
- **Create:** SE-D-001.

#### Behavior
1. Receive the incoming demand signal payload.
2. Invoke **AB-D-001 Receive Demand Observation**.
   - AB-D-001 creates a new Demand Observation with a globally unique identifier, populates all mandatory attributes (Item, Location, Quantity, Observation Type, Business Time, Observation Time, Source System Provenance), and populates optional attributes if provided.
   - The record enters **Received** state.
3. The invoked Aggregate Behavior publishes **EV-D-001 Demand Observation Received**.
4. The Business Workflow Notification Node publishes **BN-D-005 Demand Observation Received** after EV-D-001 exists. BW-D-002 / FS-D-002 is triggered by EV-D-001.

#### Business Transaction
Per AB-D-001 contract. Protects the Demand Observation aggregate. Atomic creation of identity, mandatory attributes, and provenance.

#### Postconditions
A Demand Observation exists in Received state with full provenance. BN-D-005 published. BW-D-002 / FS-D-002 is triggered by EV-D-001.

#### Failure Behavior
- **Business Failure (duplicate observation, missing mandatory information, invalid Item or Location):** Observation not established. BN-D-005 not published. Source may resubmit with corrected data. Permanent; no automatic retry.
- **Operational Failure (source system unavailable during receipt):** Observation not established. BN-D-005 not published. Source may resubmit. Temporary; retryable.

#### Recovery Behavior
Re-execution with the same business identity produces no duplicate. AB-D-001 rejects duplicate identifiers.

#### Concurrency Guarantees
Observations with different business identities are processed independently.

#### Example
A source system sends a sales order for Item P-1001 at Location DC-01, quantity 120 units, Business Date 14-Jan-2027. AB-D-001 creates Demand Observation DOR-001 in Received state. BN-D-005 published. BW-D-002 / FS-D-002 triggered.

#### Traceability
- Realises: CR-D-001
- Invokes: AB-D-001
- Publishes: BN-D-005
- Referenced by: CA-D-001

---

### FS-D-002 – Evaluate Demand Observation

**Realises:** CR-D-002

#### Business Contract
- **Consumes:** Demand Observation (SE-D-001) in Received state.
- **Produces:** SE-D-001 with updated Lifecycle State (Accepted, Quarantined, or Rejected) and recorded decision traceability.
- **Transitions:** SE-D-001: Received → Accepted / Quarantined / Rejected.
- **Publishes:** BN-D-006 (Accepted), BN-D-002 (Quarantined), BN-D-003 (Rejected).
- **Invokes:** AB-D-002.
- **Guarantees:** Observation evaluated exactly once. Decision traceability recorded. Appropriate notification published.

#### Trigger
Completion of FS-D-001 (Demand Observation in Received state).

#### Preconditions
- SE-D-001 is in Received state.
- SE-D-001 has not been previously evaluated.
- Referenced Item (SE-C-001) and Location (SE-C-002) exist.

#### Semantic Objects
- **Read:** SE-D-001 (Received), SE-C-001, SE-C-002.
- **Update:** SE-D-001 (lifecycle state, decision traceability).

#### Behavior
1. Load the Demand Observation in Received state.
2. Invoke **AB-D-002 Evaluate Demand Observation**.
   - AB-D-002 internally executes **DE-D-001 Accept Demand Observation**, which evaluates the record against BR-D-200, BR-D-201, BR-D-202, and BR-D-203.
   - Based on the decision outcome, the record transitions to Accepted, Quarantined, or Rejected.
   - Decision confidence and rationale are recorded.
3. Publish the appropriate notification based on the outcome:
   - Accepted → **BN-D-006 Demand Observation Accepted**
   - Quarantined → **BN-D-002 Demand Observation Quarantined**
   - Rejected → **BN-D-003 Demand Observation Rejected**

#### Business Transaction
Per AB-D-002 contract. Protects the Demand Observation aggregate. Atomic state transition and decision traceability recording.

#### Postconditions
Observation is in a final evaluation state. Decision traceability recorded. Appropriate notification published.

#### Failure Behavior
- **Business Failure (DE-D-001 cannot produce a valid outcome):** Observation remains in Received. No notification published. Permanent; requires manual review per PO-D-001.
- **Operational Failure (external reference data unavailable):** Observation remains in Received. No notification published. Temporary; retryable.

#### Recovery Behavior
Re-execution on an already-evaluated observation terminates immediately. AB-D-002 enforces single evaluation.

#### Concurrency Guarantees
Observation is evaluated exactly once. Concurrent evaluation attempts are rejected.

#### Example
Demand Observation DOR-001 (120 units of P-1001) is evaluated. BR-D-200–203 all pass; source reliability is 98%. DE-D-001 returns Accept. Record transitions to Accepted, decision confidence High. BN-D-006 published.

#### Traceability
- Realises: CR-D-002
- Invokes: AB-D-002
- Publishes: BN-D-002, BN-D-003, BN-D-006
- Referenced by: CA-D-001

---

### FS-D-003 – Revise Demand Understanding

**Realises:** CR-D-003

#### Business Contract
- **Consumes:** Enterprise Picture (SE-C-021) — Published version. Forecast Publication (SE-D-003) — Published version, if available.
- **Produces:** Demand Understanding (SE-D-002) — new Draft version.
- **Transitions:** SE-D-002: (none) → Draft (first version), or Published → Draft (subsequent versions). Previous Published version remains authoritative.
- **Publishes:** None. AB-D-003 publishes EV-D-003.
- **Invokes:** AB-D-003.
- **Guarantees:** One current Draft per Planning Scope. All interpretations traceable to the Enterprise Picture version and Forecast Publication version used.

#### Trigger
Enterprise Picture Published (notification from Core), or Forecast Publication Published (BN-D-011).

#### Preconditions
- A Published Enterprise Picture (SE-C-021) exists for the Planning Scope.
- For first revision, no prior Demand Understanding exists. For subsequent revisions, a Published version exists.

#### Semantic Objects
- **Read:** SE-C-021 (Published), SE-D-003 (Published, if available).
- **Create/Update:** SE-D-002.

#### Behavior
1. Load the latest Published Enterprise Picture for the Planning Scope.
2. If a Published Forecast Publication exists for the Planning Scope, load it as forward-looking context.
3. Invoke **AB-D-003 Revise Demand Understanding**.
   - AB-D-003 interprets the demand facts in the Enterprise Picture into the four interpretation dimensions (Demand Continuity, Demand Pattern, Demand Health, Demand Volatility).
   - Evidence references are recorded for every dimension.
   - If a Draft already exists for the Planning Scope, it is updated in place rather than creating a duplicate.
4. The invoked Aggregate Behavior publishes **EV-D-003 Demand Understanding Revised**. BW-D-004 / FS-D-004 is triggered by EV-D-003.

#### Business Transaction
Per AB-D-003 contract. Protects the Demand Understanding aggregate. Atomic version creation and interpretation recording.

#### Postconditions
A Draft version of the Demand Understanding exists for the Planning Scope. The previous Published version remains authoritative. BW-D-004 / FS-D-004 is triggered by EV-D-003.

#### Failure Behavior
- **Business Failure (no Enterprise Picture available):** No Draft created. Retryable when the Enterprise Picture is published.
- **Operational Failure (unable to compute interpretations):** No Draft created. Previous version retained. Retryable.

#### Recovery Behavior
Re-execution with the same Enterprise Picture version produces the same Draft. Idempotent.

#### Concurrency Guarantees
Updates to the same Planning Scope are serialized.

#### Example
Enterprise Picture v27 is published for Planning Scope PS-001. AB-D-003 creates Draft v12 of the Demand Understanding, interpreting demand as Stable (Continuity), Normal (Pattern), Healthy (Health), and Low Volatility. EV-D-003 published.

#### Traceability
- Realises: CR-D-003
- Invokes: AB-D-003
- Referenced by: CA-D-001

---

### FS-D-004 – Publish Demand Understanding

**Realises:** CR-D-004

#### Business Contract
- **Consumes:** Demand Understanding (SE-D-002) — Draft version. Materiality assessment. Interpretation completeness score.
- **Produces:** SE-D-002 — Published version (authoritative). Previous Published version → Superseded.
- **Transitions:** Draft → Published. Previous Published → Superseded.
- **Publishes:** BN-D-001 Demand Understanding Published.
- **Invokes:** AB-D-004. Per AB-D-004 contract. Protects the Demand Understanding aggregate. Atomic publication and previous version superseding.
- **Guarantees:** Exactly one Published version per Planning Scope. Published version is immutable.

#### Trigger
Material change detected in at least one interpretation dimension per PO-D-011, or Periodic Refresh due per PO-D-012.

#### Preconditions
- Draft version exists.
- Materiality assessment has been performed and at least one interpretation dimension is Material, OR Periodic Refresh is due.
- Interpretation completeness meets the threshold defined in PO-D-011.

#### Semantic Objects
- **Read:** SE-D-002 (Draft), materiality assessment.
- **Update:** SE-D-002 (current and previous).

#### Behavior
1. Load the Draft Demand Understanding and the materiality assessment.
2. Invoke **AB-D-004 Publish Demand Understanding**.
   - AB-D-004 internally executes **DE-D-002 Publish Demand Understanding**.
   - If the decision is Publish: transition Draft to Published, record Publication Time, supersede previous Published version.
   - If Do Not Publish: Draft retained.
3. If Published, publish **BN-D-001 Demand Understanding Published** with Planning Scope, Version, Publication Time, Superseded Version, and Material Change Summary.

#### Business Transaction
Per AB-D-005 contract. Protects the Demand Understanding aggregate. Atomic publication and previous version superseding.

#### Postconditions
If Publish: exactly one Published version per Planning Scope. BN-D-001 published. If Do Not Publish: Draft retained.

#### Failure Behavior
- **Business Failure (DE-D-002 returns Do Not Publish):** Draft retained. BN-D-001 not published. Permanent until conditions met.
- **Operational Failure (publication cannot be completed):** Draft retained. BN-D-001 not published. Retryable.

#### Recovery Behavior
Re-execution on an already-published version terminates immediately. Idempotent.

#### Concurrency Guarantees
Publication for a given version occurs exactly once.

#### Example
Draft v12 of Demand Understanding for PS-001 has a material change in Demand Volatility (Low → Medium). Interpretation completeness is 100%. DE-D-002 returns Publish. v12 transitions to Published, v11 superseded. BN-D-001 published.

#### Traceability
- Realises: CR-D-004
- Invokes: AB-D-004
- Publishes: BN-D-001
- Referenced by: CA-D-001

---

## Forecast Demand Functional Specifications

### FS-D-005 – Initialize Forecast Publication Generation Context

**Realises:** CR-D-005

#### Business Contract
- **Consumes:** Scheduled time signal, or Critical demand behavior notification (BN-D-016), or authorised planner request.
- **Produces:** Forecast Publication generation context identity and initial workflow state.
- **Transitions:** None (workflow initiation).
- **Publishes:** BN-D-010 Forecast Publication Generation Established.
- **Invokes:** AB-D-005, AB-D-006.
- **Guarantees:** Exactly one Forecast Publication generation context initiated with unique identity. No concurrent active contexts for the same Planning Scope and Forecast Horizon.

#### Trigger
Scheduled forecast generation time reached, or BN-D-016 Critical Demand Behavior Requires Action received, or authorised Demand Planner request. Out-of-cycle initiation is governed by PO-D-024.

#### Preconditions
- No active generation context exists for the same Planning Scope and Forecast Horizon.
- Forecast Horizon and time bucket configuration are defined.

#### Semantic Objects
- **Read:** PO-D-024 Governed Parameters, Calendar (SE-C-033).
- **Create:** None (workflow state only).

#### Behavior
1. Validate that no active generation context exists for the same Planning Scope and Forecast Horizon.
2. Invoke **AB-D-005 Initialize Forecast Publication Generation Context**.
   - AB-D-005 creates a new Forecast Publication generation context with a unique identifier, records the initiation reason (Scheduled, Critical Demand Change, or Planner Request), and sets the generation status to Initialized.
3. Invoke **AB-D-006 Select Champion Model**.
   - AB-D-006 executes **DE-D-013 Select Champion Model** to determine the authorised forecasting strategy for this generation context.
4. The Business Workflow Notification Node publishes **BN-D-010 Forecast Publication Generation Established** with Generation Context ID, Reason, Horizon, and Timestamp after EV-D-010 exists. BW-D-006 / FS-D-006 is triggered by EV-D-010.

#### Business Transaction
Per AB-D-005 contract. Protects workflow state. Atomic creation of generation context identity and metadata.

#### Postconditions
Forecast Publication generation context established with unique identity and champion model. BN-D-010 published. BW-D-006 / FS-D-006 is triggered by EV-D-010.

#### Failure Behavior
- **Business Failure (active generation context already in progress, unauthorised out-of-cycle request):** Generation context not initiated. BN-D-010 not published. Permanent for this trigger.
- **Operational Failure (calendar configuration unavailable):** Generation context not initiated. BN-D-010 not published. Temporary; retryable.

#### Recovery Behavior
Each trigger creates a distinct generation context with a new unique identifier.

#### Concurrency Guarantees
Only one active generation context permitted per Planning Scope and Forecast Horizon. Initiation checks prevent concurrent creation.

#### Example
Scheduled nightly run at 02:00 UTC. AB-D-005 creates generation context GC-2027-042. AB-D-006 selects Champion Model X. BN-D-010 published. BW-D-006 / FS-D-006 triggered by EV-D-010.

#### Traceability
- Realises: CR-D-005
- Invokes: AB-D-005, AB-D-006
- Publishes: BN-D-010
- Referenced by: CA-D-002

---

### FS-D-006 – Produce Forecast Projection

**Realises:** CR-D-006

#### Business Contract
- **Consumes:** Demand Understanding (SE-D-002), Demand Behavior Assessments (SE-D-004), Demand Behavior Assignments (SE-D-006), historical demand data, Forecast Configuration.
- **Produces:** Forecast Publication (SE-D-003) — Draft populated with forecast lines.
- **Transitions:** SE-D-003: (none) → Draft (populated).
- **Publishes:** None. AB-D-007 publishes EV-D-011.
- **Invokes:** AB-D-007, BA-D-015.
- **Guarantees:** Every covered series receives a forecast or is flagged as unforecastable. All forecast lines carry model provenance and confidence scores.

#### Trigger
Completion of FS-D-005 (Forecast Cycle Established).

#### Preconditions
- Draft Forecast Publication exists for the cycle.
- The authorised forecasting strategy has been selected per PO-D-017.
- Demand history is available for the training window.

#### Semantic Objects
- **Read:** SE-D-002, SE-D-004, SE-D-006, historical demand data, Forecast Configuration, Calendar (SE-C-033).
- **Create:** SE-D-003 (Draft with forecast lines).

#### Behavior
1. Load the Draft Forecast Publication for the cycle.
2. For each covered demand series:
   - Invoke **AB-D-007 Produce Forecast Projection**.
   - AB-D-007 executes **DE-D-003 Generate Forecast for Series** to determine whether the series is Forecastable or Unforecastable.
   - If Forecastable: the authorised forecasting strategy produces the statistical forecast, including mean, prediction interval, and confidence score.
   - If Unforecastable: the series is flagged with a documented reason. The fallback method is selected per PO-D-019 by the algorithm.
3. The Forecast Confidence Index (KA-D-009) is computed from model confidence, data completeness, and signal quality.
4. The invoked Aggregate Behavior publishes **EV-D-011 Forecast Projection Produced**.
5. Invoke **BA-D-015 Reconcile Forecast Hierarchy** to apply the reconciliation method governed by PO-D-029 to produce hierarchically consistent forecast lines.

#### Business Transaction
Per AB-D-007 contract. Protects the Forecast Publication aggregate. Atomic creation of forecast lines and metadata.

#### Postconditions
Draft Forecast Publication populated with forecast lines for all covered series. Unforecastable series are flagged. FS-D-010 and FS-D-011 are eligible.

#### Failure Behavior
- **Business Failure (no demand history for any series):** All series flagged unforecastable. Publication produced with completeness warning.
- **Operational Failure (forecasting strategy execution error):** Affected series flagged unforecastable. Publication proceeds with available forecasts.

#### Recovery Behavior
Re-execution regenerates all forecasts within the same Draft publication. Idempotent.

#### Concurrency Guarantees
Generation for a given publication occurs exactly once.

#### Example
5,000 series processed. 4,900 forecast successfully with the authorised strategy. 100 series flagged unforecastable (insufficient history). Overall Forecast Confidence Index 87%. EV-D-011 published.

#### Traceability
- Realises: CR-D-006
- Invokes: AB-D-007, BA-D-015
- Referenced by: CA-D-002

---

### FS-D-007 – Govern Forecast Projection

**Realises:** CR-D-007

#### Business Contract
- **Consumes:** Draft Forecast Publication (SE-D-003). Planner-submitted override request.
- **Produces:** Forecast Override recorded within the Draft publication. Original system forecast preserved unchanged.
- **Transitions:** None (publication remains Draft).
- **Publishes:** BN-D-012 Forecast Override Applied.
- **Invokes:** None (may be invoked multiple times before publication).
- **Guarantees:** Override recorded with full traceability. Original system forecast preserved permanently.

#### Trigger
Authorised planner submits an override for a specific forecast within a Draft Forecast Publication.

#### Preconditions
- Draft Forecast Publication exists and is not Published.
- Override request includes a non-empty business justification (BR-D-208).
- Override value is within the configured deviation limit, or Demand Manager approval has been obtained (BR-D-209, PO-D-022).

#### Semantic Objects
- **Read:** SE-D-003 (Draft), SE-D-015 (target forecast line).
- **Create:** SE-D-017 (Forecast Override entity).
- **Update:** None (original forecast line preserved unchanged).

#### Behavior
1. Validate planner authorisation per PO-D-022.
2. Invoke **AB-D-008 Govern Forecast Projection**.
   - AB-D-008 executes **DE-D-005 Evaluate Forecast Override**.
   - If Accept: create a Forecast Override entity (SE-D-028) recording the original system value, the override value, the planner identity, the justification, and the timestamp. The published forecast value becomes the override value.
   - If Reject: notify the planner of the rejection reason.
   - If Request Revision: return to the planner with required changes.
3. If accepted, publish **BN-D-012 Forecast Override Applied**.

#### Business Transaction
Per AB-D-008 contract. Protects the Forecast Publication aggregate. Atomic creation of override and preservation of original.

#### Postconditions
If accepted: override recorded with full traceability. Original system forecast preserved. BN-D-012 published. If rejected: planner notified. If revision requested: planner prompted.

#### Failure Behavior
- **Business Failure (justification empty, deviation exceeded without authorisation):** Override rejected. BN-D-012 not published. Planner may resubmit.
- **Operational Failure (unable to record override):** Override not recorded. BN-D-012 not published. Temporary; retryable.

#### Recovery Behavior
Planner may resubmit a corrected override. Re-execution with the same parameters updates the existing override.

#### Concurrency Guarantees
Overrides for different series are processed independently.

#### Example
System forecast 250 units. Planner override 500 units, justification "Confirmed large one-time order from Customer X." Within deviation limit. Accepted. BN-D-012 published.

#### Traceability
- Realises: CR-D-007
- Invokes: AB-D-008
- Publishes: BN-D-012
- Referenced by: CA-D-002

---

### FS-D-008 – Publish Forecast Publication

**Realises:** CR-D-008

#### Business Contract
- **Consumes:** Draft Forecast Publication (SE-D-003) with forecasts generated and overrides applied. Forecast Confidence Index (KA-D-009). Assumption sign-off status.
- **Produces:** SE-D-003 — Published (authoritative). Previous publication for the same scope → Superseded.
- **Transitions:** SE-D-003: Draft → Published. Previous: Published → Superseded.
- **Publishes:** BN-D-011 Forecast Published.
- **Invokes:** None (terminal).
- **Guarantees:** Exactly one Published Forecast Publication for the scope and horizon. Responsibility transfers to consumers.

#### Trigger
Completion of FS-D-006 (Produce Forecast Projection) and after all expected overrides (FS-D-007) have been processed.

#### Preconditions
- Draft Forecast Publication exists with forecasts generated.
- Forecast Confidence Index (KA-D-009) computed.
- All forecast assumptions signed off per PO-D-025.
- Completeness threshold met per BR-D-207.

#### Semantic Objects
- **Read:** SE-D-003 (Draft), KA-D-009, assumption sign-off records.
- **Update:** SE-D-003 (current and previous).

#### Behavior
1. Load the Draft Forecast Publication, Forecast Confidence Index, and sign-off status.
2. Invoke **AB-D-009 Publish Forecast Publication**.
   - AB-D-009 executes **DE-D-004 Approve Forecast Publication**.
   - If Publish Automatically: transition Draft to Published, supersede previous Published version for the same scope and horizon, record Publication Time.
   - If Require Planner Approval: pause and notify the Demand Planner.
   - If Suppress: publish BN-D-014 Forecast Publication Suppressed.
3. If published, publish **BN-D-011 Forecast Published** with Publication ID, Version, Planning Scope, Horizon, Confidence Index, and Champion Model.

#### Business Transaction
Per AB-D-009 contract. Protects the Forecast Publication aggregate. Atomic publication and previous version superseding.

#### Postconditions
Exactly one Published Forecast Publication for the scope and horizon. BN-D-011 published. Responsibility transfers to downstream consumers.

#### Failure Behavior
- **Business Failure (confidence below threshold, completeness not met):** Publication remains Draft. BN-D-013 published if suppressed; otherwise routed for approval.
- **Operational Failure (notification delivery unavailable):** Publication succeeds. BN-D-011 delivery retried per its delivery guarantee.

#### Recovery Behavior
Re-execution publishes the same publication. Idempotent.

#### Concurrency Guarantees
Publication for a given publication occurs exactly once.

#### Example
Forecast Publication PUB-2027-003, Confidence Index 87%, Completeness 98%. DE-D-004 returns Publish Automatically. Publication Published. Previous PUB-2027-002 Superseded. BN-D-011 published.

#### Traceability
- Realises: CR-D-008
- Invokes: AB-D-009
- Publishes: BN-D-011
- Referenced by: CA-D-002

---

## Sense Demand Functional Specifications

### FS-D-009 – Maintain Demand Behavior Understanding

**Realises:** CR-D-009

#### Business Contract
- **Consumes:** Streaming demand signal. Demand Understanding (SE-D-002) as the evaluation baseline.
- **Produces:** Updated Demand Behavior Assessment (SE-D-004) with possible new State Change Event.
- **Transitions:** SE-D-004: Current State may change per evaluation.
- **Publishes:** BN-D-015 (if state changed), BN-D-016 (if new state is Critical).
- **Invokes:** BA-D-014, AB-D-010.
- **Guarantees:** Every signal is evaluated against the current baseline. If a state change is warranted, the assessment is updated atomically and notifications published.

#### Trigger
Streaming demand signal received for a monitored Item-Location.

#### Preconditions
- Signal contains Item, Location, quantity, and timestamp.
- Demand Understanding (SE-D-002) provides the current expected behavior baseline for the Item-Location.
- Demand Sensing Policy (PO-D-031) is current.

#### Semantic Objects
- **Read:** SE-D-002, incoming signal.
- **Update:** SE-D-004.

#### Behavior
0. Invoke **BA-D-014 Derive Demand Behavior Baseline** to compute the expected demand level and normal variation for the monitored Item-Location from Demand Understanding, historical observations, and governed policy.
1. Load the current Demand Behavior Assessment for the Item-Location, or create a new one if not yet monitored (initial state Normal).
2. Invoke **AB-D-010 Maintain Demand Behavior Understanding**.
   - AB-D-010 executes **DE-D-006 Evaluate Demand Signal for State Change**.
   - If outcome is No Change: terminate.
   - If outcome is a state transition: update Current State, append a State Change Event recording the deviation magnitude, direction, confidence, corroborating signals, and baseline reference.
3. If a state change occurred, the invoked Aggregate Behavior publishes **EV-D-015 Demand Behavior Changed**. The Business Workflow Notification Node publishes **BN-D-015 Demand Behavior Changed**.
4. If the new state is Critical, the invoked Aggregate Behavior publishes **EV-D-016 Critical Demand Behavior Detected**. The Business Workflow Notification Node publishes **BN-D-016 Critical Demand Behavior Requires Action**. BW-D-010 / FS-D-010 is triggered by EV-D-016.

#### Business Transaction
Per AB-D-010 contract. Protects the Demand Behavior Assessment aggregate. Atomic state update and history recording.

#### Postconditions
Assessment reflects the latest signal evaluation. If state changed, history recorded and notifications published.

#### Failure Behavior
- **Business Failure (signal invalid, baseline missing):** Assessment unchanged. No notification.
- **Operational Failure (baseline service unavailable):** Assessment unchanged. Retryable.

#### Recovery Behavior
Re-evaluation of the same signal is idempotent.

#### Concurrency Guarantees
Signals for different Item-Locations are processed independently. Signals for the same Item-Location are serialized.

#### Example
SKU P-1001, DC-01: signal shows +4.2σ deviation, corroborated by POS and web sources. DE-D-006 returns Transition to Critical. Current State set to Critical, State Change Event recorded. BN-D-015 and BN-D-016 published. FS-D-010 invoked.

#### Traceability
- Realises: CR-D-009
- Invokes: BA-D-014, AB-D-010
- Publishes: BN-D-015, BN-D-016
- Referenced by: CA-D-003

---

### FS-D-010 – Escalate Critical Demand Behavior

**Realises:** CR-D-010

#### Business Contract
- **Consumes:** Demand Behavior Assessment (SE-D-004) in Critical state.
- **Produces:** Forecast refresh trigger in Forecast Demand.
- **Transitions:** None (cross-capability invocation).
- **Publishes:** None additional.
- **Invokes:** AB-D-010, DE-D-007.
- **Guarantees:** Critical state changes are evaluated for forecast refresh; if criteria met, refresh is triggered.

#### Trigger
FS-D-009 detects a Critical state transition.

#### Preconditions
- SE-D-004 Current State is Critical.
- A current Forecast Publication (SE-D-003) exists for the Item-Location.

#### Semantic Objects
- **Read:** SE-D-004, SE-D-003.

#### Behavior
1. Invoke **AB-D-010 Maintain Demand Behavior Understanding**.
   - AB-D-010 internally executes **DE-D-007 Trigger Forecast Refresh on Critical State**.
   - Evaluate forecast age against the freshness threshold in PO-D-032.
   - Evaluate expected accuracy improvement against the minimum benefit threshold.
2. If outcome is Trigger Refresh: the Business Workflow Notification Node publishes **BN-D-016 Critical Demand Behavior Requires Action**. Forecast Demand consumes BN-D-016 and triggers its own workflow.
3. If outcome is Defer: log the deferral for the next scheduled cycle.

#### Business Transaction
None. This FS orchestrates cross-capability invocation.

#### Postconditions
If triggered, a new forecast cycle is initiated in Forecast Demand.

#### Failure Behavior
If Forecast Demand is unavailable, an alert is logged and manual follow-up is required.

#### Recovery Behavior
Retry the invocation of FS-D-005.

#### Concurrency Guarantees
Independent per assessment.

#### Traceability
- Realises: CR-D-010
- Invokes: AB-D-010, DE-D-007
- Referenced by: CA-D-003

---

## Segment Demand Functional Specifications

### FS-D-011 – Classify Planning Entity

**Realises:** CR-D-011

#### Business Contract
- **Consumes:** Entity identifier (Item or Customer), classification type, Segmentation Policy (PO-D-035), Demand Understanding (SE-D-002), historical demand data.
- **Produces:** Updated Planning Classification Assignment (SE-D-005) with possible new classification.
- **Transitions:** SE-D-005: Current Classification may change; Assignment Change Event appended.
- **Publishes:** BN-D-017 Planning Classification Changed (if changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Classification is current per the Segmentation Policy. History preserved.

#### Trigger
Scheduled re-evaluation, Segmentation Policy change, demand behavior change, new entity registration, or planner override.

#### Preconditions
- Segmentation Policy (PO-D-035) is current.
- Required evidence for the classification type is available.

#### Semantic Objects
- **Read:** SE-D-002, PO-D-035, historical demand data, SE-C-001 (if Item) or SE-C-003 (if Customer).
- **Update:** SE-D-005.

#### Behavior
1. Load the current Planning Classification Assignment for the entity and type, or create a new one if not yet classified (initial state Unclassified).
2. Invoke **AB-D-011 Classify Planning Entity**.
   - AB-D-011 executes **DE-D-008 Determine Planning Classification**.
   - If classification has changed: update Current Classification, append an Assignment Change Event with the reason, confidence, and policy version reference.
3. If classification changed, publish **BN-D-017 Planning Classification Changed**.

#### Business Transaction
Per AB-D-011 contract. Protects the Planning Classification Assignment aggregate.

#### Postconditions
Entity has a current classification for the type. History updated if changed.

#### Failure Behavior
Assignment unchanged. Retryable.

#### Recovery Behavior
Re-classification is idempotent.

#### Concurrency Guarantees
Assignments for different entities or types are independent; same entity and type updates are serialized.

#### Example
SKU P-1001, type ABC: volume contribution 15% of total → Class A. Previously Class B → change. BN-D-017 published.

#### Traceability
- Realises: CR-D-011
- Invokes: AB-D-011
- Publishes: BN-D-017
- Referenced by: CA-D-004

---

## Classify Demand Functional Specifications

### FS-D-012 – Classify Demand Behavior

**Realises:** CR-D-012

#### Business Contract
- **Consumes:** Entity identifier (Item-Location), behavior dimension, Classification Policy (PO-D-037), Demand Understanding (SE-D-002), historical demand data.
- **Produces:** Updated Demand Behavior Assignment (SE-D-006) with possible new classification.
- **Transitions:** SE-D-006: Current Classification may change; Behavior Change Event appended.
- **Publishes:** BN-D-019 (if classification changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Classification is current per the Classification Policy. Evidence is recorded. History preserved.

#### Trigger
Scheduled re-evaluation, Classification Policy change, demand behavior change, new entity registration, or planner override.

#### Preconditions
- Classification Policy (PO-D-037) is current.
- Required evidence for the behavior dimension is available.

#### Semantic Objects
- **Read:** SE-D-002, PO-D-037, historical demand data, SE-C-001, SE-C-002.
- **Update:** SE-D-006.

#### Behavior
1. Load the current Demand Behavior Assignment for the entity and dimension, or create a new one if not yet classified (initial state Unclassified).
2. Invoke **AB-D-012 Classify Demand Behavior**.
   - AB-D-012 executes **DE-D-009 Determine Behavior Classification**.
   - If classification has changed: update Current Classification, append a Behavior Change Event with evidence summary, confidence, and policy version reference.
3. If classification changed, publish **BN-D-019 Demand Behavior Classification Changed**.

#### Business Transaction
Per AB-D-012 contract. Protects the Demand Behavior Assignment aggregate.

#### Postconditions
Entity has a current classification for the dimension. Evidence recorded.

#### Failure Behavior
Assignment unchanged. Retryable.

#### Recovery Behavior
Re-classification is idempotent.

#### Concurrency Guarantees
Assignments for different entities or dimensions are independent; same entity and dimension updates are serialized.

#### Example
SKU P-1001, DC-01, dimension Statistical Pattern: autocorrelation at seasonal lag significant (p<0.01), CV=0.8 → Seasonal. Previously Continuous → change. BN-D-019 published.

#### Traceability
- Realises: CR-D-012
- Invokes: AB-D-012
- Publishes: BN-D-019
- Referenced by: CA-D-005

---

## Prioritize Demand Functional Specifications

### FS-D-013 – Prioritize Planning Entity

**Realises:** CR-D-013

#### Business Contract
- **Consumes:** Entity identifier, Prioritization Policy (PO-D-039), Planning Classification (SE-D-005), Demand Behavior Assignment (SE-D-006), Demand Understanding (SE-D-002).
- **Produces:** Updated Planning Priority Assignment (SE-D-007) with possible new priority, score, and decision rationale.
- **Transitions:** SE-D-007: Current Priority may change; Priority Change Event appended.
- **Publishes:** BN-D-020 Planning Priority Changed (if changed).
- **Invokes:** None (may be invoked in batch).
- **Guarantees:** Priority is current per the Prioritization Policy. Decision rationale and business validity are preserved. History is preserved.

#### Trigger
Scheduled re-evaluation, Prioritization Policy change, segment change, behavior change, or planner override.

#### Preconditions
- Prioritization Policy (PO-D-039) is current.
- Mandatory business evidence is available.

#### Semantic Objects
- **Read:** SE-D-002, SE-D-005, SE-D-006, PO-D-039, SE-C-001 or SE-C-003.
- **Update:** SE-D-007.

#### Behavior
1. Load the current Planning Priority Assignment for the entity, or create a new one if not yet assigned (initial state Unclassified).
2. Invoke **AB-D-013 Prioritize Planning Entity**.
   - AB-D-013 executes **DE-D-010 Determine Planning Priority**.
   - If priority has changed: update Current Priority, Priority Score, Decision Rationale, and Business Validity; append a Priority Change Event.
3. If priority changed, publish **BN-D-020 Planning Priority Changed**.

#### Business Transaction
Per AB-D-013 contract. Protects the Planning Priority Assignment aggregate.

#### Postconditions
Entity has a current priority with decision rationale and business validity.

#### Failure Behavior
Assignment unchanged. Retryable.

#### Recovery Behavior
Re-evaluation is idempotent.

#### Concurrency Guarantees
Assignments for different entities are independent; same entity updates are serialized.

#### Example
SKU P-1001: segment A, strategic customer, contractual SLA → Critical. Rationale: "Top-5 customer, strategic launch product, contractual SLA requires 98% service level." Previously High → change. BN-D-020 published.

#### Traceability
- Realises: CR-D-013
- Invokes: AB-D-013
- Publishes: BN-D-020
- Referenced by: CA-D-006

---

## Evaluate Demand Quality Functional Specifications

### FS-D-014 – Evaluate Forecast Quality

**Realises:** CR-D-014

#### Business Contract
- **Consumes:** Published forecast data (SE-D-003), actual demand data from Enterprise Picture (SE-C-021), planner override records, Forecast Measurement Policy (PO-D-041).
- **Produces:** Forecast Quality Assessment (SE-D-008) — Published.
- **Transitions:** SE-D-008: (none) → Draft → Published. Previous version for the same scope and period → Superseded.
- **Publishes:** BN-D-021 Forecast Quality Assessment Published.
- **Invokes:** None (terminal).
- **Guarantees:** Quality metrics computed according to PO-D-041. If published, the assessment is the authoritative enterprise quality record.

#### Trigger
Scheduled per PO-D-041 cadence, or on-demand after sufficient actuals are available.

#### Preconditions
- Forecast and actual demand data available for the full evaluation period.
- Data completeness meets the policy threshold (BR-D-212).
- Evaluation period meets the minimum length (BR-D-213).

#### Semantic Objects
- **Read:** SE-D-003, SE-C-021, override records, PO-D-041.
- **Create:** SE-D-008.

#### Behavior
1. Create a Draft Forecast Quality Assessment.
2. Invoke **AB-D-014 Evaluate Forecast Quality**.
   - Compute all mandatory metrics (WAPE, Forecast Bias, Forecast Accuracy) and optional metrics (MAPE, FVA, Forecast Stability, Override Effectiveness) per PO-D-041.
   - Execute **DE-D-011 Publish Forecast Quality Assessment**.
   - If Publish: transition to Published, supersede previous version for the same Assessment Scope and Evaluation Period.
   - If Do Not Publish: assessment remains Draft; Demand Manager notified.
3. If published, publish **BN-D-021 Forecast Quality Assessment Published**.

#### Business Transaction
Per AB-D-014 contract. Protects the Forecast Quality Assessment aggregate.

#### Postconditions
If published, exactly one Published assessment exists for the scope and period. BN-D-021 published.

#### Failure Behavior
- **Business Failure (completeness below threshold):** Assessment suppressed. Demand Manager notified.
- **Operational Failure:** Assessment not published. Retryable.

#### Recovery Behavior
Re-evaluation for the same period is idempotent.

#### Concurrency Guarantees
Assessments for different scopes or periods are independent.

#### Example
Q1 2027, enterprise scope: WAPE 8.5%, Bias +1.2%, Forecast Accuracy 91.5%. Completeness 98%. Published. BN-D-021 published.

#### Traceability
- Realises: CR-D-014
- Invokes: AB-D-014
- Publishes: BN-D-021
- Referenced by: CA-D-007

---

## Detect Demand Exceptions Functional Specifications

### FS-D-015 – Detect Demand Exception Evidence

**Realises:** CR-D-015

#### Business Contract
- **Consumes:** Forecast Publications (SE-D-003), actual demand data (SE-C-021), Demand Understanding (SE-D-002), Demand Behavior Assessments (SE-D-004), Forecast Quality Assessments (SE-D-008), Exception Detection Policy (PO-D-044).
- **Produces:** Demand Exception Detection Evidence or Demand Exception Resolution Evidence published to Core Exception Management.
- **Transitions:** None.
- **Publishes:** BN-D-022 (if detection evidence exists), BN-D-023 (if resolution evidence exists).
- **Invokes:** DE-D-012, BA-D-010, BA-D-011.
- **Guarantees:** Exception detection and resolution evidence is published to Core Exception Management according to PO-D-044 thresholds.

#### Trigger
Scheduled evaluation, or event-driven when new forecasts, actuals, quality assessments, or behavior assessments are published.

#### Preconditions
- Exception Detection Policy (PO-D-044) is current.
- Required demand evidence is available for the evaluation scope.

#### Semantic Objects
- **Read:** SE-D-002, SE-D-003, SE-D-004, SE-D-008, SE-C-021, PO-D-044.
- **Create/Update:** None.

#### Behavior
1. For each planning entity and exception type defined in PO-D-044:
   - Invoke **BA-D-010 Evaluate Demand Exception Evidence** to assess whether detection criteria are met.
   - Invoke **BA-D-011 Assess Demand Exception Lifecycle Evidence** to determine the lifecycle evidence (detection, resolution, or no evidence).
   - Invoke **DE-D-012 Evaluate Demand Exception Evidence** to produce the final determination.
   - If Detection Evidence Exists: emit **EV-D-022** and the Workflow Notification Node publishes **BN-D-022 Demand Exception Detection Evidence** to Core Exception Management.
   - If Resolution Evidence Exists: emit **EV-D-022** and the Workflow Notification Node publishes **BN-D-023 Demand Exception Resolution Evidence** to Core Exception Management.
   - If No Evidence: no event emitted, no notification published.

#### Business Transaction
No Business Transaction. CA-D-008 owns no aggregate.

#### Postconditions
Demand exception evidence is produced and published to Core Exception Management.

#### Failure Behavior
Evidence publishing fails cleanly. Retryable.

#### Recovery Behavior
Re-evaluation with the same evidence is idempotent.

#### Concurrency Guarantees
Evaluations for different entities or types are independent.

#### Example
Forecast Bias for Segment A evaluated at 18% (threshold 10%). DE-D-012 returns Detection Evidence Exists. BN-D-022 published to Core Exception Management with High severity. Next cycle: Bias = 8% → DE-D-012 returns Resolution Evidence Exists. BN-D-023 published to Core Exception Management.

#### Traceability
- Realises: CR-D-015
- Invokes: AB-D-015
- Publishes: BN-D-022, BN-D-023
- Referenced by: CA-D-008

---

## Explain Demand Functional Specifications

### FS-D-016 – Establish Demand Explanation

**Realises:** CR-D-016

#### Business Contract
- **Consumes:** Artifact to be explained (any demand intelligence artifact carrying preserved evidence), Explanation Template Catalog, source evidence with historical versions.
- **Produces:** Demand Explanation (SE-D-010) — immutable record, or existing explanation returned if reasoning is unchanged.
- **Transitions:** SE-D-010: (none) → Created (or existing returned).
- **Publishes:** BN-D-024 Demand Explanation Established.
- **Invokes:** None (terminal).
- **Guarantees:** Structured Reasoning Graph is canonical, deterministic, and carries provenance on every node. All source artifacts referenced by their historical versions.

#### Trigger
Planner or auditor request for a specific artifact, or automatic trigger per governance policy (Critical condition detected, forecast published).

#### Preconditions
- Explained artifact exists and has preserved evidence.
- Explanation template is available for the artifact type.
- Source evidence is accessible with historical versions.

#### Semantic Objects
- **Read:** The explained artifact and its referenced evidence (with version history), Explanation Template Catalog.
- **Create:** SE-D-010 (or return existing).

#### Behavior
1. Invoke **AB-D-016 Establish Demand Explanation**.
   - If an existing explanation exists for the same artifact with identical reasoning, policy versions, and template version: return the existing explanation without creating a new one.
   - Otherwise: select the appropriate template; gather source evidence with historical versions; build the Structured Reasoning Graph with provenance on every node; generate the natural language rendering from the graph; create the immutable Demand Explanation.
2. Publish **BN-D-024 Demand Explanation Established**.

#### Business Transaction
Per AB-D-016 contract. Protects the Demand Explanation aggregate.

#### Postconditions
Immutable explanation record exists with Structured Reasoning Graph and historical version references. BN-D-024 published.

#### Failure Behavior
- **Business Failure (insufficient preserved evidence):** Explanation marked "Unavailable – Incomplete Evidence" and persisted for audit.
- **Operational Failure:** Explanation not created. Retryable.

#### Recovery Behavior
Re-requesting the same explanation is idempotent.

#### Concurrency Guarantees
Explanations for different artifacts are independent.

#### Example
Planner requests explanation for Forecast FC-2027-042, SKU P-1001. Structured Reasoning Graph built with seasonal factor, promotion factor, and trend factor, each with provenance. Natural language rendering generated. BN-D-024 published.

#### Traceability
- Realises: CR-D-016
- Invokes: AB-D-016
- Publishes: BN-D-024
- Referenced by: CA-D-009

---

## Learn From Demand Functional Specifications

### FS-D-017 – Establish Demand Learning

**Realises:** CR-D-017

#### Business Contract
- **Consumes:** All Demand Intelligence semantic objects — Forecast Quality Assessments (SE-D-008), Demand Planning Conditions (SE-D-009), Demand Explanations (SE-D-010), Planning Classifications (SE-D-005), Demand Behavior Assignments (SE-D-006), Planning Priorities (SE-D-007), Demand Behavior Assessments (SE-D-004), and the Learning Analysis Policy (PO-D-048).
- **Produces:** Demand Learning (SE-D-011) — immutable record.
- **Transitions:** SE-D-011: (none) → Created.
- **Publishes:** BN-D-025 Demand Learning Established.
- **Invokes:** None (terminal).
- **Guarantees:** Every learning is supported by evidence from at least one completed analysis. Learnings are immutable and permanently retained.

#### Trigger
Scheduled per the Learning Analysis Policy, or event-driven when new quality assessments, resolved conditions, or explanations are available.

#### Preconditions
- Learning Analysis Policy (PO-D-048) is current.
- Sufficient historical evidence is available per the policy's evidence sufficiency thresholds.

#### Semantic Objects
- **Read:** SE-D-008, SE-C-019 (via Core Exception Management BNs), SE-D-010, SE-D-005, SE-D-006, SE-D-007, SE-D-004, PO-D-048.
- **Create:** SE-D-011.

#### Behavior
1. Invoke **AB-D-017 Establish Demand Learning**.
   - Analyse historical evidence across the governed learning scope defined in PO-D-048.
   - If the evidence supports a discovery: create an immutable Demand Learning with Learning Type (from the policy taxonomy), Learning Statement, Supporting Evidence references, and Evidence Strength (as defined by the policy).
   - If the evidence does not meet the threshold: no learning is created.
2. For each learning created, publish **BN-D-025 Demand Learning Established**.

#### Business Transaction
Per AB-D-017 contract. Protects the Demand Learning aggregate.

#### Postconditions
If created, immutable learning records exist with supporting evidence and evidence strength. BN-D-025 published per learning.

#### Failure Behavior
- **Business Failure (recurrence threshold not met):** No learning created.
- **Operational Failure:** Learning not created. Retryable.

#### Recovery Behavior
Re-analysis with the same evidence is idempotent. Duplicate learnings are not created.

#### Concurrency Guarantees
Analyses for different learning scopes are independent.

#### Example
Q1–Q3 2027 analysis: planner overrides in Segment B value-destroying 68% of the time for 3 consecutive quarters. Learning created: "Planner overrides in Segment B are systematically degrading forecast accuracy." Pattern Confidence High. BN-D-025 published.

#### Traceability
- Realises: CR-D-017
- Invokes: AB-D-017
- Publishes: BN-D-025
- Referenced by: CA-D-010

---

## Model Demand Interventions Functional Specifications

### FS-D-018 – Assess Demand Intervention Impact

**Realises:** CR-D-018

#### Business Contract
- **Consumes:** SE-C-039 Scenario Adjustment, SE-D-002 Demand Understanding, SE-D-003 Forecast Publication (historical elasticity context), PO-D-050.
- **Produces:** SE-D-018 Demand Intervention Impact in Draft state.
- **Transitions:** SE-D-018: (none) → Draft.
- **Publishes:** None (Draft creation does not publish).
- **Invokes:** AB-D-018, BA-D-016.
- **Guarantees:** Draft impact created with computed lift, confidence, and provenance.

#### Trigger
Scenario Adjustment published or updated, or authorised planner request.

#### Preconditions
- SE-C-039 Scenario Adjustment is active.
- SE-D-002 Demand Understanding is published for the relevant Planning Scope.
- PO-D-050 is current.

#### Semantic Objects
- **Read:** SE-C-039, SE-D-002, SE-D-003, PO-D-050.
- **Create:** SE-D-018 (Draft).

#### Behavior
1. Load the Scenario Adjustment and validate it is active.
2. Load the Demand Understanding for baseline context.
3. Load historical forecast-actual pairs from Forecast Publication for elasticity estimation.
4. Invoke **BA-D-016 Model Intervention Lift** to compute the assessed demand lift.
5. Invoke **AB-D-018 Assess Demand Intervention Impact** to create the Draft SE-D-018.

#### Business Transaction
Per AB-D-018 contract. Protects SE-D-018 aggregate.

#### Postconditions
Draft SE-D-018 exists with computed lift, confidence, temporal validity, and model provenance.

#### Failure Behavior
- **Business Failure (intervention inactive, insufficient data):** Draft not created. Planner notified.
- **Operational Failure (computation error):** Draft not created. Retryable.

#### Recovery Behavior
Re-execution with the same intervention updates the existing Draft.

#### Concurrency Guarantees
Assessments for different interventions are independent. Same intervention is serialized.

#### Traceability
- Realises: CR-D-018
- Invokes: AB-D-018, BA-D-016
- Referenced by: CA-D-011

---

### FS-D-019 – Publish Demand Intervention Impact

**Realises:** CR-D-019

#### Business Contract
- **Consumes:** SE-D-018 (Draft), PO-D-050.
- **Produces:** SE-D-018 Published version. Previous Published → Superseded.
- **Transitions:** SE-D-018: Draft → Published; Previous Published → Superseded.
- **Publishes:** BN-D-026 Demand Intervention Impact Published.
- **Invokes:** AB-D-019, DE-D-014.
- **Guarantees:** Exactly one Published version per intervention per item-location.

#### Trigger
Assessment complete (Draft SE-D-018 exists).

#### Preconditions
- Draft SE-D-018 exists.
- PO-D-050 is current.

#### Semantic Objects
- **Read:** SE-D-018 (Draft), PO-D-050.
- **Update:** SE-D-018 (current and previous).

#### Behavior
1. Load the Draft SE-D-018.
2. Invoke **AB-D-019 Publish Demand Intervention Impact**.
   - AB-D-019 executes **DE-D-014 Approve Intervention Impact Publication**.
   - If Publish: transition Draft to Published, supersede previous.
   - If Do Not Publish: Draft retained.
3. If Published, publish **BN-D-026 Demand Intervention Impact Published**.

#### Business Transaction
Per AB-D-019 contract. Protects SE-D-018 aggregate.

#### Postconditions
If Published: exactly one Published version exists. BN-D-026 published. If Do Not Publish: Draft retained.

#### Failure Behavior
- **Business Failure (confidence below threshold, intervention inactive):** Draft retained. BN-D-026 not published.
- **Operational Failure (notification delivery unavailable):** Publication succeeds. BN-D-026 delivery retried.

#### Recovery Behavior
Re-execution on already-published version terminates immediately.

#### Concurrency Guarantees
Publication for a given intervention occurs exactly once.

#### Traceability
- Realises: CR-D-019
- Invokes: AB-D-019, DE-D-014
- Publishes: BN-D-026
- Referenced by: CA-D-011

---

# Chapter 10 — Business Algorithms

## BA-D-001 — Evaluate Demand Understanding Materiality

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Deterministic Assessment |
| Domain | Understand Demand |
| Knowledge Category | Materiality Assessment |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Is the difference between the draft Demand Understanding and the currently published version materially significant such that publication is warranted?"**

### 3. Business Intent

This algorithm operationalizes PO-D-011. It compares the draft and published versions of the Demand Understanding across the four governed interpretation dimensions, applies the defined evaluation criteria, and produces a structured materiality assessment. It does not decide whether to publish; it provides the evidence on which DE-D-002 makes that decision.

### 4. Architectural Principle

This algorithm consumes the governed thresholds defined in PO-D-011. It does not define them; it applies them. Changing a threshold is a policy change, not an algorithm change. The algorithm owns the comparison procedure, the deterministic computation, and the production of the materiality assessment.

### 5. Business Explanation

When the Demand Understanding is revised—because a new Enterprise Picture has been published or a new Forecast Publication provides updated forward-looking context—the enterprise must determine whether the changes in interpretation are significant enough to warrant formal publication. This algorithm performs that check. It examines each interpretation dimension, determines whether the change crosses a governed materiality threshold, and marks each dimension as Material or NotMaterial. The result feeds directly into the publication decision.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Draft Demand Understanding | SE-D-002 (Draft) | Proposed new version. | Yes | Algorithm not applicable. |
| Published Demand Understanding | SE-D-002 (Published) | Currently authoritative version. For first publication, this is absent. | Yes | For first publication, all dimensions return NotApplicable and HasMaterialChange is true. |
| Demand Understanding Materiality Policy | PO-D-011 | The governed enterprise materiality thresholds and mandatory publication conditions. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Output | Meaning |
|--------|---------|
| MaterialityAssessment | Per-dimension results with HasMaterialChange flag. Each dimension is marked Material, NotMaterial, or NotApplicable. |

### 9. Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | A Draft version of the Demand Understanding exists. PO-D-011 is current. |
| **Business Behavior** | Compare the Draft and Published versions across the four interpretation dimensions using the thresholds defined in PO-D-011. For first publication, all dimensions return NotApplicable and HasMaterialChange is true. For subsequent publications, each dimension is evaluated independently. |
| **Exceptional Conditions** | If a dimension cannot be evaluated because evidence is missing in either version, it is marked NotApplicable with a reason code. |
| **Postconditions** | A MaterialityAssessment is produced indicating, for each dimension, whether the change is Material, NotMaterial, or NotApplicable, and an overall HasMaterialChange flag. |
| **Outcome When Preconditions Are Not Satisfied** | If no Draft version exists, the algorithm is not applicable. If PO-D-011 is missing, the algorithm is not applicable. |

### 10. Evaluation Methodology

**First publication (Published is absent):** All four dimensions return NotApplicable. HasMaterialChange is true, because the initial publication must proceed.

**Subsequent publications:** For each interpretation dimension, compare the Draft and Published versions using the thresholds governed by PO-D-011. If any threshold is crossed, the dimension is Material. If multiple dimensions are material, each is independently reported.

#### 10.1 Demand Continuity Interpretation

The Demand Continuity Interpretation captures the enterprise's assessment of current demand patterns and their persistence. The status is one of: Stable, Increasing, Declining, or Volatile.

- The Continuity Status transitions between any of the four statuses. A transition from Stable to Volatile or Volatile to Stable is always Material. A transition between Stable and Increasing, or Stable and Declining, is Material if the magnitude of the underlying demand change exceeds the threshold defined in PO-D-011.
- The set of Key Demand Drivers changes: a new driver appears, or an existing driver is removed.

#### 10.2 Demand Pattern Interpretation

The Demand Pattern Interpretation captures the enterprise's assessment of the structure and predictability of current demand. The status is one of: Normal, Seasonal, Irregular, or Step-Change.

- The Pattern Status transitions between any of the four statuses. Any transition involving Step-Change is always Material. A transition between Normal and Seasonal, or Normal and Irregular, is Material.
- Pattern Confidence changes by more than one level (High to Low, or vice versa).

#### 10.3 Demand Health Interpretation

The Demand Health Interpretation captures the enterprise's assessment of whether the current demand understanding is reliable. The status is one of: Healthy, At Risk, or Critical.

- The Health Status transitions to At Risk or Critical from any other status.
- The Health Status transitions from At Risk or Critical back to Healthy.
- A Data Quality Concern is newly identified or resolved.

#### 10.4 Demand Volatility Interpretation

The Demand Volatility Interpretation synthesizes how uncertain the current demand picture is. The level is one of: Low, Medium, or High.

- The Volatility Level changes by more than one level (Low to High, or High to Low).
- The set of Primary Volatility Drivers changes: a new driver appears, or an existing driver is removed.

### 11. Business Rules

| ID | Rule |
|----|------|
| (algorithm rule) | For first publication, HasMaterialChange is always true. |
| (algorithm rule) | For subsequent publications, a dimension is Material if any of its defined thresholds are crossed. |
| (algorithm rule) | If multiple dimensions are Material, each is independently reported. |
| PO-D-011 | Governs the thresholds used in the evaluation. |

### 12. Assumptions

- The draft and published versions contain complete interpretation data per the interpretation completeness rules.
- Interpretation status values and confidence levels are accurate and governed.

### 13. Explainability

Every materiality determination traces back to the specific dimension status change, the threshold it crossed, and the evidence from the version comparison. The rationale template for DE-D-002 directly references the output of this algorithm.

### 14. Postconditions / Guarantees

- A MaterialityAssessment is produced for all four interpretation dimensions.
- Each dimension is independently assessed.
- No publication decision is made by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| Draft version missing | Algorithm not applicable. |
| PO-D-011 missing | Algorithm not applicable. |
| First publication | All dimensions NotApplicable; HasMaterialChange = true. |
| Dimension evidence missing | Dimension marked NotApplicable with reason code. |

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Understand Demand (CA-D-001) |
| Governed By | PO-D-011 |
| Invoked By | AB-D-004 (Publish Demand Understanding) |
| Referenced By | FS-D-004 |
| Produces | MaterialityAssessment consumed by DE-D-002 |
| Depends On | PO-D-011 |

---

## BA-D-002 — Produce Forecast Projection

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Constructive Reasoning |
| Domain | Forecast Demand |
| Knowledge Category | Forecast Generation |
| Stage | Constructive |
| Output Type | Plan |
| Explainable | Full |

> **Architectural Note (ARS §15.7.1):** This algorithm is a Constructive algorithm. It synthesises a complete set of forecast lines for a Forecast Publication by applying the authorised forecasting strategy to historical demand data and forward-looking context. It simultaneously assesses the confidence and completeness of the generated forecasts. It does not select the forecasting strategy; that selection is governed by PO-D-017 and occurs before this algorithm is invoked.

> **Architectural Note:** This algorithm is a Constructive algorithm per ARS §15.7.1. It synthesises a complete set of forecast lines for a Forecast Publication, and simultaneously assesses the confidence and completeness of those forecasts. The algorithm owns the synthesis of the artifact; internal decomposition into separate forecasting, fallback, and confidence computation steps is an implementation concern. The enterprise contract—its inputs, outputs, and purpose—remains stable regardless of how those steps are internally organised.

### 2. Purpose

Answer the enterprise question: **"Given the authorised forecasting strategy, the historical demand evidence, and the current Demand Understanding, what future demand quantities should the enterprise project, with what confidence, for every covered series?"**

### 3. Business Intent

The enterprise does not manually compute demand projections across thousands of item-location combinations. This algorithm embodies the enterprise's governed logic for generating forecasts. It applies the authorised forecasting strategy to historical demand data, produces statistical forecasts with prediction intervals and confidence scores, identifies series that cannot be forecast, and computes the overall Forecast Confidence Index for the publication. It does not decide which strategy to use; it applies the strategy that governance has authorised for this cycle.

### 4. Architectural Principle

This algorithm applies the authorised forecasting strategy to produce the enterprise's demand projections. It does not select the strategy, does not decide whether the forecast is acceptable, and does not publish the result. It produces a complete Draft forecast; governance determines whether that forecast becomes authoritative.

### 5. Business Explanation

Every forecast cycle, the enterprise must answer: "What do we expect demand to be?" This algorithm performs that computation. It takes the historical demand data, the current Demand Understanding as forward-looking context, and the authorised forecasting strategy, and produces a forecast for every covered series. For series with sufficient history, it generates a statistical forecast with a mean value, a prediction interval, and a confidence score. For series without sufficient history, it applies the fallback method governed by PO-D-019. It also computes the Forecast Confidence Index, which measures the overall reliability of the publication. The output is a complete set of forecast lines ready for override governance and publication approval.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and authorised strategy) | Same inputs and strategy produce identical forecasts. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects; computes solely from inputs. |
| Order Sensitive | No | The order in which series are processed does not affect individual forecasts. |
| Explainable | Full | Every forecast is traceable to its input data and the strategy that produced it. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Historical demand data | Enterprise Picture (SE-C-021), historical records | Cleansed demand history for each series over the training window. | Yes | Series without sufficient history are flagged as Unforecastable. |
| Demand Understanding | SE-D-002 (Published) | Current demand interpretation providing forward-looking context. | Yes | Forecasts generated without forward-looking context; confidence may be reduced. |
| Authorised forecasting strategy | Selected per PO-D-017 | The forecasting model or ensemble authorised for this cycle. | Yes | Algorithm not applicable. |
| Forecast Configuration | Governed parameters | Horizon, time buckets, coverage scope, confidence thresholds. | Yes | Algorithm not applicable. |
| Unforecastable Series Policy | PO-D-019 | Governs the fallback method selection for series with insufficient history. | Yes | All unforecastable series are flagged for planner attention. |
| Per-series Forecastability Classification | DE-D-003 outcome (produced in FS-D-006 step 2) | For each covered series, whether it is Forecastable or Unforecastable. | Yes | If missing, the algorithm cannot distinguish series; series are treated as Unforecastable with reason code `ClassificationUnavailable`. |
| Forecast Reconciliation Policy | PO-D-029 | Governed reconciliation method. | Yes | Algorithm not applicable. |

### 8. Output Contract

The algorithm produces a complete set of forecast lines for the Draft Forecast Publication:

| Component | Business Meaning |
|-----------|------------------|
| Forecast Lines | For each covered series: mean forecast quantity, prediction interval (lower, upper, confidence level), confidence score, model provenance. |
| Unforecastable Series | Series flagged as unable to be forecast, with documented reasons and assigned fallback methods. |
| Forecast Confidence Index | The overall reliability score for the publication, computed as a weighted composite per PO-D-020. |
| Forecast Completeness Score | The percentage of covered series that received a valid forecast. |
| Generation Metadata | Timestamps, strategy identifier, data window references. |

### 9. Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | The authorised forecasting strategy has been selected. Historical demand data is available for the training window. PO-D-019 and PO-D-020 are current. |
| **Business Behavior** | For each covered demand series, determine whether sufficient history exists. If yes, apply the authorised strategy to generate the statistical forecast. If no, apply the fallback method per PO-D-019. Compute the Forecast Confidence Index from model confidence, data completeness, and signal quality. |
| **Exceptional Conditions** | If the authorised strategy fails to produce a valid forecast for a series, the series is flagged as Unforecastable with the reason code `StrategyExecutionError`. |
| **Postconditions** | A complete set of forecast lines is produced. Unforecastable series are flagged. The Forecast Confidence Index is computed. |
| **Outcome When Preconditions Are Not Satisfied** | If no authorised strategy has been selected, the algorithm is not applicable. If no historical data is available, all series are flagged as Unforecastable. |

### 10. Evaluation Methodology

The algorithm executes the following steps:

1. Load the authorised forecasting strategy and the Forecast Configuration.
2. Load the historical demand data and the current Demand Understanding.
3. For each covered demand series:
   - If the Per-series Forecastability Classification is **Forecastable**: apply the authorised forecasting strategy to produce the mean forecast, prediction interval, and confidence score. Record the strategy provenance.
   - If **Unforecastable**: select the fallback method according to the preference order in PO-D-019. If a fallback method is viable, apply it. If no method is viable, flag the series as Unforecastable with the reason code `NoViableFallbackMethod`.

4. After all series are processed, compute the Forecast Confidence Index as a weighted composite of:
   - Model confidence: the average confidence score across all statistically forecast series.
   - Data completeness: the percentage of covered series that received a valid forecast.
   - Signal quality: the Demand Signal Quality Index (KA-D-008) for the evaluation period.
   The specific weights are defined in PO-D-020.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-206 | A minimum number of periods of demand history is required to generate a statistical forecast. |
| BR-D-401 | The forecast must be produced using the authorised forecasting strategy. |
| BR-D-410 | All published forecasts shall satisfy hierarchical consistency as defined by the reconciliation method governed by PO-D-029. |
| PO-D-019 | Governs fallback method selection for unforecastable series. |
| PO-D-020 | Governs the Forecast Confidence Index computation. |
| PO-D-029 | Governs the reconciliation method. |

### 12. Assumptions

- Historical demand data is accurate and has been cleansed of known anomalies.
- The authorised forecasting strategy is capable of producing prediction intervals and confidence scores.
- The Forecast Configuration correctly defines the coverage scope and time buckets.

### 13. Explainability

Every forecast line is traceable to the specific historical data window used, the authorised strategy that produced it, and the confidence score. The unforecastable series carry documented reasons. The Forecast Confidence Index is traceable to its component factors and their weights. Explain Supply Decisions can answer: "Why is the forecast for P-1001 250 units?" by tracing to the historical data and the strategy.

### 14. Postconditions / Guarantees

- Every covered series has either a forecast line or a documented reason why it cannot be forecast.
- All forecast lines carry model provenance.
- The Forecast Confidence Index is computed and recorded.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| No authorised strategy selected | Algorithm not applicable. |
| No historical data for any series | All series flagged Unforecastable. |
| Strategy execution error for a series | Series flagged Unforecastable; other series unaffected. |

### 16. Example

**Inputs:** 5,000 covered series. Authorised strategy: Model-B. Historical data window: 104 weeks.
**Processing:** 4,900 series have sufficient history and receive statistical forecasts with mean, 90% prediction interval, and confidence scores. 100 series have insufficient history; 95 receive analog-based fallback forecasts, 5 receive lifecycle model forecasts. Forecast Confidence Index computed as weighted composite: model confidence 89%, completeness 98%, signal quality 92% → Index 87%.
**Output:** Complete forecast line set. 100 series flagged with fallback method provenance. Forecast Confidence Index 87%.

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Forecast Demand (CA-D-002) |
| Governed By | PO-D-017, PO-D-019, PO-D-020 |
| Invoked By | AB-D-007 (Produce Forecast Projection) |
| Referenced By | FS-D-006 |
| Produces | Forecast lines and Forecast Confidence Index for SE-D-003 |
| Consumed By | DE-D-003, DE-D-004 |
| Depends On | SE-C-021, SE-D-002, PO-D-017, PO-D-019, PO-D-020 |

---

## BA-D-003 — Assess Demand Signal Deviation

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Sense Demand |
| Knowledge Category | Signal Assessment |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the incoming demand signal and the current expected behavior baseline, has a meaningful deviation occurred, and what is the assessed magnitude, direction, and confidence of that deviation?"**

### 3. Business Intent

Demand signals arrive continuously from multiple sources. Most represent normal variation. A few indicate that something has genuinely changed. This algorithm evaluates each incoming signal against the expected behavior baseline from the Demand Understanding, computes the deviation in standard deviations, assesses corroboration from independent sources, and determines whether the deviation is noise, significant, or critical. It does not manage the lifecycle state of the Demand Behavior Assessment; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm assesses whether an incoming demand signal constitutes a meaningful deviation from expected behavior. It produces an evidence-based deviation assessment with magnitude, direction, and confidence. It does not normalise data, manage state transitions, or decide on responses.

### 5. Business Explanation

Every day the enterprise receives thousands of demand signals—POS transactions, warehouse shipments, order entries. A planner cannot review them all. This algorithm acts as the enterprise's filter. It takes each signal, compares it against the expected demand level for that item-location, and computes the deviation in standard deviations. It determines whether the deviation is below the noise threshold (suppress), above the significant threshold (elevated or depressed), or above the critical threshold (critical). It also checks whether corroborating signals from other independent sources support the deviation. The result is a concise, evidence-based deviation assessment.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical signal and baseline) | Same signal and baseline produce identical assessment. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects; assesses solely from inputs. |
| Order Sensitive | No | The order in which signals are evaluated does not affect individual assessments. |
| Explainable | Full | Every assessment is traceable to the signal, baseline, and policy thresholds. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Incoming demand signal | External systems, internal events | The observed quantity, timestamp, source, and item-location. | Yes | Algorithm not applicable. |
| Expected behavior baseline | SE-D-004 baseline parameters, derived by BA-D-014 | The expected demand level and standard deviation of normal variation for this item-location. | Yes | Assessment uses signal-only evidence; confidence reduced. |
| Corroborating signals | Other independent sources | Additional signals for the same item-location and time period. | No | Corroboration count is zero; confidence may be reduced. |
| Demand Sensing Policy | PO-D-031 | Governed deviation thresholds (Significant, Critical, Noise). | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Deviation Magnitude | The deviation expressed in standard deviations (σ) from the baseline. |
| Deviation Direction | Increase or Decrease relative to baseline. |
| Deviation Classification | Noise (below noise threshold), Significant (above Significant threshold), or Critical (above Critical threshold). |
| Corroboration Count | Number of independent sources supporting the deviation. |
| Assessment Confidence | Enterprise confidence in the deviation assessment (High, Medium, Low). |
| Reason Codes | Governed codes identifying contributing factors (e.g., `SingleSourceEvidence`, `HighCorroboration`). |
| Assessment Timestamp | When the assessment was produced. |

### 9. Preconditions

- At least one incoming demand signal has been received for a monitored item-location.
- The Demand Understanding provides a current expected behavior baseline.
- PO-D-031 is current and defines the deviation thresholds.

### 10. Evaluation Methodology

1. Load the expected behavior baseline from the Demand Understanding for the item-location. The baseline includes the expected demand level and the standard deviation of normal variation.
2. Compute the deviation of the incoming signal from the expected level: `deviation = (signal_quantity − expected_level) / standard_deviation`.
3. Determine the direction: Increase (signal exceeds expected) or Decrease (signal is below expected).
4. Classify the deviation magnitude against the thresholds defined in PO-D-031:
   - Below the Noise threshold: classification is Noise.
   - Between Noise and Significant: classification is Noise. Only deviations above Significant are considered meaningful.
   - Between Significant and Critical: classification is Significant.
   - Above Critical: classification is Critical.
5. Count corroborating signals from other independent sources for the same item-location and time period.
6. Assess confidence based on:
   - Signal quality (source reliability, timeliness).
   - Corroboration count (higher corroboration increases confidence).
   - Baseline recency (stale baselines reduce confidence).
7. Assign reason codes from the governed taxonomy in PO-D-031.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-300 | Deviation thresholds are governed by PO-D-031. |
| BR-D-301 | Critical state requires corroboration by at least two independent sources. |
| BR-D-302 | For high-priority items, the Significant threshold is lowered per PO-D-031. |
| BR-D-303 | Signals below the noise threshold are suppressed. |

### 12. Assumptions

- The expected behavior baseline accurately reflects normal demand variation.
- Incoming signals are timely and contain sufficient information for evaluation.
- PO-D-031 reflects the enterprise's tolerance for demand volatility.

### 13. Explainability

Every deviation assessment is traceable to the specific signal quantity, the expected baseline level and standard deviation, and the policy thresholds applied. Explain Supply Decisions can answer: "Why was this signal assessed as Critical?" by referencing the deviation magnitude, the threshold it crossed, and the corroboration count.

### 14. Postconditions / Guarantees

- A deviation assessment is produced for the signal.
- The assessment contains magnitude, direction, classification, corroboration, and confidence.
- No state transition is performed by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| No incoming signal | Algorithm not applicable. |
| Baseline unavailable | Assessment uses signal-only evidence; confidence reduced. |
| Signal below noise threshold | Classification is Noise; signal suppressed. |
| Conflicting corroborating signals | All signals recorded; confidence reduced due to inconsistency. |

### 16. Example

**Inputs:** Signal for Item P-1001, Location DC-01: 850 units. Baseline expected: 500 units, σ = 80 units. PO-D-031 thresholds: Noise 1σ, Significant 2.5σ, Critical 4σ.
**Computation:** deviation = (850 − 500) / 80 = 4.375σ. Direction: Increase. Classification: Critical (exceeds 4σ). Corroboration: POS signal shows +4.1σ, web order signal shows +4.5σ → count = 2. Confidence: High.
**Output:** Deviation 4.4σ, Increase, Critical, Corroboration 2, Confidence High. Reason codes: `CriticalDeviation`, `MultiSourceCorroboration`.

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Sense Demand (CA-D-003) |
| Governed By | PO-D-031 |
| Invoked By | AB-D-010 (Maintain Demand Behavior Understanding) |
| Referenced By | FS-D-009 |
| Produces | Deviation assessment consumed by DE-D-006 |
| Depends On | SE-D-002, PO-D-031 |

---

## BA-D-004 — Determine Demand Behavior State

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Sense Demand |
| Knowledge Category | Business State Determination |
| Stage | State |
| Output Type | State |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the assessed deviation of the current demand signal, what is the authoritative demand behavior state for this item-location?"**

### 3. Business Intent

The deviation assessment from BA-D-003 tells the enterprise *how much* the signal deviates from baseline. This algorithm determines *what that means* for the enterprise's understanding of demand behavior. It applies the governed state transition rules from PO-D-031 to the deviation assessment and the current state, and determines whether a state transition is warranted. The result is the authoritative demand behavior state. It does not apply the state transition to the assessment; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm determines the authoritative demand behavior state exclusively from the deviation assessment and the current state. It applies a governed state transition logic defined by policy. It does not re-evaluate the signal, does not manage the lifecycle of the assessment, and does not trigger downstream actions.

### 5. Business Explanation

A planner does not ask "what was the deviation in standard deviations?" They ask "is demand normal, elevated, depressed, or critical?" This algorithm answers that question. It takes the deviation assessment and the current state, and applies the enterprise's own rules for state transitions. For example, a single Critical deviation does not automatically transition to Critical if corroboration is insufficient. A sustained Elevated pattern may transition to Critical even if no single signal exceeds the Critical threshold. The algorithm applies these rules consistently and transparently.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and policy) | Same assessment, current state, and policy produce identical result. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects; determines solely from inputs. |
| Order Sensitive | No | The order of evaluation does not affect the result. |
| Explainable | Full | Every state determination is traceable to the deviation and policy rule applied. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Deviation Assessment | BA-D-003 output | The assessed deviation magnitude, direction, classification, corroboration, and confidence. | Yes | Algorithm not applicable; state remains unchanged. |
| Current Demand Behavior State | SE-D-004 | The current state of the Demand Behavior Assessment (Normal, Elevated, Depressed, Critical). | Yes | For new assessments, initial state is Normal. |
| Demand Sensing Policy | PO-D-031 | Governed state transition rules, including high-priority sensitivity. | Yes | Algorithm not applicable. |
| Planning Priority Assignment | SE-D-007 | The priority level for the monitored Item-Location. | No | High-priority sensitivity is not applied. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Determined State | The authoritative demand behavior state: Normal, Elevated, Depressed, or Critical. |
| State Transition Occurred | Boolean indicating whether the state changed from the current state. |
| Determination Rationale | Business-language explanation linking the determined state to the deviation evidence and policy rule. |
| Determination Confidence | Enterprise confidence in the state determination (High, Medium, Low). |

### 9. Preconditions

- BA-D-003 has produced a deviation assessment for the signal.
- The current Demand Behavior Assessment state is available.
- PO-D-031 is current and defines state transition rules.

### 10. Evaluation Methodology

1. Load the deviation assessment from BA-D-003, the current state from SE-D-004, and the state transition rules from PO-D-031.
2. Apply the transition rules in the following order. The specific threshold values (Noise, Significant, Critical) referenced in these rules are defined in PO-D-031, not in this algorithm:

   **Rule 1 — Noise Suppression:** If the deviation classification is Noise, the determined state is the current state (no change).

   **Rule 2 — Critical Transition:** If the deviation classification is Critical AND the corroboration count meets the minimum defined in PO-D-031, the determined state is Critical, regardless of the current state.

   **Rule 3 — Significant Transition:** If the deviation classification is Significant and the current state is Normal:
   - If direction is Increase: determined state is Elevated.
   - If direction is Decrease: determined state is Depressed.

   **Rule 4 — Return to Normal:** If the deviation classification is Significant or below, and the current state is Elevated, Depressed, or Critical, and the deviation does not meet the criteria for those states, the determined state is Normal. The policy may require multiple consecutive non-deviating signals before returning to Normal; the specific requirement is defined in PO-D-031.

   **Rule 5 — High-Priority Sensitivity:** For items classified as high-priority by the Planning Priority Assignment (SE-D-007), the Significant threshold is lowered to the value defined in PO-D-031.

   **Rule 6 — Additional Policy Rules:** Any additional transition rules defined in PO-D-031 (such as sustained-pattern escalation, manual override handling, or state-specific corroboration exceptions) shall be applied in the order specified by the policy.

3. This algorithm applies the above rules in sequence. The first rule whose conditions are fully satisfied determines the outcome. The specific numerical thresholds for Noise, Significant, and Critical classifications, the corroboration minimum, and the high-priority adjusted threshold are all defined in PO-D-031. This algorithm does not define them; it applies them.
4. If the determined state differs from the current state, State Transition Occurred is true.
5. Generate the determination rationale identifying the specific rule that was applied and the evidence that triggered it.
6. Assess confidence from the deviation assessment confidence and the consistency of the evidence.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-300 | Deviation thresholds are governed by PO-D-031. |
| BR-D-301 | Critical state requires corroboration by at least two independent sources. |
| BR-D-302 | High-priority items use a lowered Significant threshold. |
| BR-D-303 | Signals below noise threshold do not trigger state changes. |

### 12. Assumptions

- The deviation assessment from BA-D-003 is accurate and complete.
- The state transition rules in PO-D-031 are complete and non-contradictory.
- The current state in SE-D-004 accurately reflects the last determined state.

### 13. Explainability

Every state determination is traceable to the specific deviation assessment, the current state, and the policy rule that was applied. The determination rationale explains which rule triggered the transition or why no transition occurred.

### 14. Postconditions / Guarantees

- A single authoritative state is determined.
- The determination rationale references the specific policy rule and evidence.
- No state transition is applied to the aggregate by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| Deviation assessment missing | Algorithm not applicable; state unchanged. |
| PO-D-031 missing | Algorithm not applicable. |
| First signal for a new item-location | Current state is Normal. |
| Deviation is Critical but not corroborated | State capped at Elevated or Depressed per BR-D-301. |

### 16. Example

**Inputs:** Deviation assessment: 4.4σ, Increase, Critical, Corroboration 2, Confidence High. Current state: Normal. PO-D-031: Critical threshold 4σ, corroboration minimum 2.
**Processing:** Rule 2 applies: deviation is Critical AND corroboration ≥ 2. Determined state is Critical. State Transition Occurred: true.
**Output:** Determined State: Critical. Transition: true. Rationale: "Deviation 4.4σ exceeds Critical threshold (4σ) with corroboration from 2 independent sources. State transitions from Normal to Critical per PO-D-031 Rule 2." Confidence: High.

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Sense Demand (CA-D-003) |
| Governed By | PO-D-031 |
| Invoked By | AB-D-010 (Maintain Demand Behavior Understanding) |
| Referenced By | FS-D-009 |
| Produces | State determination consumed by AB-D-010 to update SE-D-004 |
| Depends On | BA-D-003, PO-D-031 |

---

## BA-D-005 — Compute Planning Classification

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Segment Demand |
| Knowledge Category | Classification |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the current Segmentation Policy and the entity's demand attributes, what classification should be assigned, with what confidence?"**

### 3. Business Intent

The enterprise classifies planning entities—items and customers—into segments that determine differentiated planning strategies. This algorithm evaluates an entity against the classification rules defined in the Segmentation Policy, computes the appropriate class label, and assesses the confidence of that classification. It does not assign the classification to the aggregate; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm applies the Segmentation Policy rules to entity attributes to produce a classification recommendation. It does not define the rules; those belong to policy. It does not manage the classification lifecycle.

### 5. Business Explanation

A demand planner needs to know whether an item is an "A" item (high volume, strategically important) or a "C" item (low volume, routine). The Segmentation Policy defines the rules for this classification. This algorithm applies those rules: it gathers the entity's demand attributes—volume, variability, revenue contribution, customer importance—evaluates them against the policy thresholds, and produces a recommended class label with a confidence score.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and policy) | Same attributes and policy produce identical classification. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects; classifies solely from inputs. |
| Order Sensitive | No | The order of classification types does not affect individual results. |
| Explainable | Full | Every classification is traceable to the policy rule and evidence. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Entity attributes | SE-D-002 (Demand Understanding), historical demand data | Demand volume, variability, revenue contribution, customer importance, etc. | Yes | Entity cannot be classified; classification is Unclassified. |
| Segmentation Policy | PO-D-035 | Governed classification rules and thresholds per classification type. | Yes | Algorithm not applicable. |
| Classification type | Workflow context | The specific classification type being evaluated (e.g., ABC, XYZ). | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Recommended Classification | The class label determined by the policy rules (e.g., A, B, C for ABC). |
| Classification Confidence | Enterprise confidence in the classification (High, Medium, Low). |
| Classification Rationale | Business-language explanation of why this class was assigned. |
| Evidence References | The specific entity attributes and policy version used. |

### 9. Preconditions

- The Segmentation Policy is current and defines rules for the classification type.
- Entity attributes are available and sufficient for the classification type.

### 10. Evaluation Methodology

1. Load the classification rules for the specified type from PO-D-035.
2. Gather the required entity attributes. The specific attributes depend on the classification type:
   - For ABC classification: demand volume (or revenue contribution) over the evaluation period, cumulative percentage.
   - For XYZ classification: demand variability (coefficient of variation) over the evaluation period.
3. Apply the policy rules. For each class in the policy-defined order, evaluate whether the entity's attributes satisfy the criteria. The first matching class is the recommended classification.
4. If no class criteria are satisfied, the recommended classification is Unclassified.
5. Assess classification confidence based on:
   - Completeness of the required attributes.
   - Recency of the demand data.
   - Closeness of the attribute values to the class boundaries (borderline values reduce confidence).
6. Generate the classification rationale.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-305 | Classification must be determined by the rules in the current Segmentation Policy. |
| BR-D-306 | Entity is Unclassified if minimum evidence is not met. |

### 12. Assumptions

- Entity attributes accurately reflect the planning entity's demand characteristics.
- The Segmentation Policy rules are mutually exclusive and exhaustive for the classification type.

### 13. Explainability

Every classification is traceable to the specific entity attributes, the policy rule that was applied, and the class boundaries. The classification rationale provides a business-language explanation.

### 14. Postconditions / Guarantees

- A classification recommendation is produced with confidence and rationale.
- No classification is assigned to the aggregate by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| Insufficient entity attributes | Classification is Unclassified. |
| Policy missing for the classification type | Algorithm not applicable. |
| Entity attribute values at class boundary | Classification is assigned to the lower class; confidence reduced. |

### 16. Example

**Inputs:** Entity P-1001, type ABC. Demand volume: 15,000 units (15% of total). PO-D-035 ABC rules: A ≥ 10% of total, B ≥ 5%, C < 5%.
**Processing:** Volume 15% ≥ 10% → Class A. Confidence: High (volume clearly above threshold).
**Output:** Recommended Classification: A. Confidence: High. Rationale: "Volume contribution 15% meets Class A threshold (≥10%)."

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Segment Demand (CA-D-004) |
| Governed By | PO-D-035 |
| Invoked By | AB-D-011 (Classify Planning Entity) |
| Referenced By | FS-D-011 |
| Produces | Classification recommendation consumed by DE-D-008 |
| Depends On | SE-D-002, PO-D-035 |

---

## BA-D-006 — Determine Behavior Classification

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Classify Demand |
| Knowledge Category | Business State Determination |
| Stage | State |
| Output Type | State |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the statistical evidence of demand behavior for this entity and dimension, what is the authoritative behavior classification?"**

### 3. Business Intent

The enterprise classifies demand behavior—intermittent, seasonal, lumpy, trending—to enable appropriate forecasting model selection and exception threshold setting. This algorithm applies the Classification Policy rules to the statistical features of the demand series and determines the authoritative behavior classification. It does not assign the classification to the aggregate; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm determines the authoritative behavior classification by applying governed policy rules to statistical evidence. It does not define the rules; those belong to policy.

### 5. Business Explanation

Forecasting models perform differently depending on demand behavior. A model that works well for smooth, continuous demand may fail for intermittent demand. This algorithm analyses the statistical features of each demand series—autocorrelation, coefficient of variation, trend significance, seasonality—and classifies the behavior according to the enterprise's Classification Policy. The result guides model selection and detection threshold setting.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and policy) | Same evidence and policy produce identical classification. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects. |
| Order Sensitive | No | |
| Explainable | Full | Every classification is traceable to the statistical evidence. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Demand history | Enterprise Picture (SE-C-021), historical records | The cleansed demand series for the entity. | Yes | Entity cannot be classified; classification is Unclassified. |
| Classification Policy | PO-D-037 | Governed behavior dimension definitions and classification rules. | Yes | Algorithm not applicable. |
| Behavior dimension | Workflow context | The specific dimension being evaluated (e.g., Statistical Pattern). | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Determined Classification | The authoritative behavior class (e.g., Continuous, Intermittent, Seasonal, Lumpy, Trend). |
| Classification Confidence | Enterprise confidence in the classification (High, Medium, Low). |
| Evidence Summary | Business-level statistical findings supporting the classification. |
| Determination Rationale | Business-language explanation of why this class was assigned. |

### 9. Preconditions

- Sufficient demand history exists for statistical analysis.
- The Classification Policy is current and defines rules for the behavior dimension.

### 10. Evaluation Methodology

1. Load the classification rules for the specified behavior dimension from PO-D-037.
2. Compute the required statistical features from the demand history:
   - Coefficient of Variation (CV): standard deviation / mean. High CV indicates intermittent or lumpy demand.
   - Autocorrelation at seasonal lag: significant autocorrelation indicates seasonality.
   - Trend significance: p-value of trend coefficient. Significant trend indicates trending behavior.
   - Average demand interval: average number of periods between non-zero demands. High interval indicates intermittent demand.
3. Apply the policy rules. For each recognised class in the policy-defined order, evaluate whether the statistical features satisfy the criteria. The first matching class is the determined classification.
4. If no class criteria are satisfied, the classification is Unclassified.
5. Assess confidence based on the length and quality of the demand history and the statistical significance of the detected features.
6. Generate the evidence summary and determination rationale.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-307 | Behavior classification must be determined by the rules in the current Classification Policy. |
| BR-D-308 | Entity is Unclassified if minimum evidence is not met. |

### 12. Assumptions

- The demand history is sufficient in length and quality for statistical analysis.
- The Classification Policy rules are mutually exclusive for the behavior dimension.

### 13. Explainability

Every behavior classification is traceable to the specific statistical features, the policy rule applied, and the evidence. The evidence summary provides business-language findings.

### 14. Postconditions / Guarantees

- A single authoritative classification is determined with confidence and evidence summary.
- No classification is assigned to the aggregate by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| Insufficient demand history | Classification is Unclassified. |
| Policy missing for the dimension | Algorithm not applicable. |
| Statistical features at boundary | Lower class assigned; confidence reduced. |

### 16. Example

**Inputs:** P-1001, DC-01, dimension Statistical Pattern. Demand history: 104 weeks. CV = 0.8, autocorrelation at lag 52 significant (p<0.01), no significant trend. PO-D-037: Seasonal requires CV < 1.0 AND significant seasonal autocorrelation.
**Processing:** CV 0.8 < 1.0, seasonal autocorrelation significant → Seasonal. Confidence: High (long history, strong signal).
**Output:** Determined Classification: Seasonal. Confidence: High. Evidence: "CV 0.8, seasonal autocorrelation at 52 weeks significant (p<0.01)."

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Classify Demand (CA-D-005) |
| Governed By | PO-D-037 |
| Invoked By | AB-D-012 (Classify Demand Behavior) |
| Referenced By | FS-D-012 |
| Produces | Behavior classification consumed by DE-D-009 |
| Depends On | SE-C-021, PO-D-037 |

---

## BA-D-007 — Compute Planning Priority Score

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Prioritize Demand |
| Knowledge Category | Priority Computation |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

> **Architectural Note:** This algorithm performs two distinct reasoning steps: the computation of a weighted priority score from governed dimensions and weights, and the mapping of that score to a priority level using the thresholds defined in PO-D-039. These two steps may evolve independently—the scoring methodology can change without affecting the level thresholds, and vice versa. For the initial release, both steps are combined in a single algorithm. Future versions may separate them into distinct algorithms (e.g., a Parameter-stage algorithm for score computation and a State-stage algorithm for level determination) if the enterprise's governance complexity requires independent evolution of scoring methodology and threshold interpretation.

### 2. Purpose

Answer the enterprise question: **"Given the entity's attributes, classifications, and the Prioritization Policy, what is the computed priority score, and what priority level does it correspond to?"**

### 3. Business Intent

Not all demand entities are equally important. This algorithm applies the Prioritization Policy to the entity's attributes—revenue contribution, strategic importance, contractual obligations, demand behavior—and computes a priority score. The score is then mapped to a priority level (Critical, High, Medium, Low) using policy-defined thresholds. The algorithm does not assign the priority to the aggregate; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm applies the Prioritization Policy scoring methodology to entity attributes. It does not define the scoring rules; those belong to policy. It does not make business judgments about the entity's importance.

### 5. Business Explanation

A planner needs to know which items to focus on. This algorithm computes a priority score from multiple business dimensions—revenue, strategy, risk, contractual obligations—and translates that score into a clear priority level. The business-language decision rationale explains why the entity received its priority, and the business validity states the conditions under which the priority applies.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and policy) | Same attributes and policy produce identical score. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects. |
| Order Sensitive | No | |
| Explainable | Full | Every score component is traceable to its source. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Entity attributes | SE-D-002, SE-D-005, SE-D-006, SE-C-001 or SE-C-003 | Revenue contribution, strategic classification, demand behavior, contractual obligations. | Yes | Entity cannot be scored; priority is Unclassified. |
| Prioritization Policy | PO-D-039 | Scoring methodology, dimension weights, and level thresholds. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Priority Score | Numeric score (0–100) computed per the policy methodology. |
| Recommended Priority Level | The priority level corresponding to the score (Critical, High, Medium, Low). |
| Decision Rationale | Business-language explanation of why this priority was assigned. |
| Business Validity | The business conditions under which this priority applies. |
| Scoring Breakdown | Per-dimension contribution to the total score. |

### 9. Preconditions

- The Prioritization Policy is current and defines the scoring methodology and thresholds.
- Mandatory entity attributes are available.

### 10. Evaluation Methodology

1. Load the scoring methodology from PO-D-039. The methodology defines:
   - The dimensions that contribute to the priority score (e.g., Revenue Impact, Strategic Importance, Demand Criticality, Contractual Obligation).
   - The weight assigned to each dimension.
   - The scoring function for each dimension (how raw attributes are converted to a 0–100 dimension score).
2. For each dimension, gather the required entity attributes and compute the dimension score.
3. Compute the weighted aggregate priority score: `Score = Σ (dimension_score_i × weight_i)`.
4. Map the score to a priority level using the thresholds in PO-D-039.
5. Generate the decision rationale describing the key factors that influenced the priority.
6. Generate the business validity stating the conditions under which this priority applies (e.g., "Effective during the current planning cycle", "Valid while the promotional campaign is active").

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-309 | Priority must be determined using the scoring methodology and thresholds in the current Prioritization Policy. |
| BR-D-310 | Entity is Unclassified if mandatory evidence is not available. |

### 12. Assumptions

- Entity attributes are accurate and current.
- The Prioritization Policy scoring methodology produces meaningful differentiation between entities.

### 13. Explainability

Every priority score is traceable to the dimension scores, their weights, and the source attributes. The decision rationale explains the business reasoning. The scoring breakdown shows each dimension's contribution.

### 14. Postconditions / Guarantees

- A priority score and recommended level are produced with rationale and validity.
- No priority is assigned to the aggregate by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| Mandatory attributes missing | Priority is Unclassified. |
| Policy missing | Algorithm not applicable. |
| Score exactly at a level boundary | Higher level assigned; flagged for review. |

### 16. Example

**Inputs:** Entity P-1001. Revenue Impact: 85 (top 5% of products). Strategic Importance: 90 (strategic launch product). Demand Criticality: 75 (seasonal, moderate variability). Contractual Obligation: 95 (SLA requires 98% service). PO-D-039 weights: Revenue 30%, Strategic 25%, Demand 20%, Contractual 25%. Thresholds: Critical ≥ 80, High ≥ 60, Medium ≥ 40, Low < 40.
**Processing:** Score = 85×0.30 + 90×0.25 + 75×0.20 + 95×0.25 = 25.5 + 22.5 + 15.0 + 23.75 = 86.75. Score 86.75 ≥ 80 → Critical.
**Output:** Priority Score: 87. Recommended Level: Critical. Rationale: "Top-5 revenue product, strategic launch item, contractual SLA requires 98% service level." Validity: "Effective during current planning cycle."

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Prioritize Demand (CA-D-006) |
| Governed By | PO-D-039 |
| Invoked By | AB-D-013 (Prioritize Planning Entity) |
| Referenced By | FS-D-013 |
| Produces | Priority score and recommendation consumed by DE-D-010 |
| Depends On | SE-D-002, SE-D-005, SE-D-006, PO-D-039 |

---

## BA-D-008 — Compute Forecast Quality Metrics

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Deterministic Computation |
| Domain | Evaluate Demand Quality |
| Knowledge Category | Quality Measurement |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the published forecasts and the actual demand outcomes for the evaluation period, what are the measured values of all governed forecast quality metrics?"**

### 3. Business Intent

The enterprise must know how accurate, biased, stable, and valuable its forecasts are. This algorithm computes every metric defined in the Forecast Measurement Policy—WAPE, MAPE, Forecast Bias, Forecast Accuracy, Forecast Stability, Forecast Value Added, and Override Effectiveness—by comparing published forecasts against actual demand outcomes. It produces a complete set of measured metric values, but does not evaluate whether those values are good or bad; that evaluation is performed by the decision DE-D-011 and the policy PO-D-041.

### 4. Architectural Principle

This algorithm measures forecast quality by applying the formulas defined in policy to observed outcomes. It does not interpret the results, does not classify quality, and does not make judgments. It computes; the decision evaluates.

### 5. Business Explanation

At the end of every evaluation period, the enterprise asks: "How good were our forecasts?" This algorithm provides the raw answer in the form of measured metrics. It compares every forecast line against the actual demand that materialized, computes error measures, bias measures, stability measures, and value-added measures. The output is a complete, evidence-based set of metrics that the Forecast Quality Assessment and the Demand Planner can use to evaluate performance.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Published forecasts | SE-D-003 (Published versions for the evaluation period) | The forecast values, prediction intervals, and confidence scores. | Yes | Metrics cannot be computed. |
| Actual demand outcomes | Enterprise Picture (SE-C-021) | The actual demand quantities for the evaluation period. | Yes | Completeness score reduced; missing series excluded from metric computation. |
| Planner override records | SE-D-003 (Override entities) | Historical overrides for FVA and Override Effectiveness computation. | No | FVA and Override Effectiveness are not computed. |
| Forecast Measurement Policy | PO-D-041 | Governed metric definitions, formulas, and evaluation period configuration. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Mandatory Metrics | WAPE, Forecast Bias, Forecast Accuracy — computed per PO-D-041. |
| Optional Metrics | MAPE, Forecast Stability, FVA, Override Effectiveness — computed if data available. |
| Completeness Score | Percentage of covered series with actual demand data available. |
| Metric Computation Evidence | References to the forecast versions, actual demand sources, and policy version used. |

### 9. Preconditions

- Published forecasts and actual demand data are available for the full evaluation period.
- PO-D-041 is current and defines the mandatory metric formulas.

### 10. Evaluation Methodology

For each metric defined in PO-D-041, apply the specified formula:

- **WAPE:** Σ|Forecast − Actual| / Σ Actual × 100. Aggregated across all series in the evaluation scope. Weighted by actual demand volume.
- **Forecast Bias:** Σ(Forecast − Actual) / Σ Actual × 100. Positive indicates over-forecasting; negative indicates under-forecasting.
- **Forecast Accuracy:** Percentage of forecast values within the acceptable accuracy tolerance (defined in PO-D-041).
- **MAPE (if configured):** (1/n) × Σ(|Forecast − Actual| / Actual) × 100. Series with zero actual demand are excluded per PO-D-041.
- **Forecast Stability (if configured):** 100 − (|Forecast_current − Forecast_prior| / Forecast_prior) × 100, comparing forecasts for the same target period from successive publications.
- **FVA (if configured):** WAPE(without step X) − WAPE(with step X), measuring the value added by each forecasting process step.
- **Override Effectiveness (if configured):** Percentage of overrides that improved forecast accuracy compared to the original system forecast.

Compute the completeness score as the percentage of covered series that have actual demand data for the full evaluation period.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-403 | Forecast quality metrics shall be computed according to the definitions and formulas in PO-D-041. |

### 12. Assumptions

- Published forecast data is accurate and complete.
- Actual demand data is accurate and complete.
- The evaluation period is consistent between forecasts and actuals.

### 13. Explainability

Every metric value is traceable to the specific forecast and actual demand values used in the computation. The metric formulas are governed and versioned in PO-D-041.

### 14. Postconditions / Guarantees

- All mandatory metrics are computed. Optional metrics are computed where data is available.
- Completeness score is computed.
- No quality judgments or classifications are made.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|-----------|
| No actual demand data | Metrics cannot be computed. |
| Policy missing | Algorithm not applicable. |
| Insufficient data for optional metric | Metric is omitted with a flag. |

### 16. Example

**Inputs:** Q1 2027, enterprise scope. 5,000 forecast series with actuals. PO-D-041 thresholds: WAPE tolerance for accuracy classification defined.
**Processing:** WAPE = 8.5%. Forecast Bias = +1.2%. Forecast Accuracy = 91.5%. MAPE = 12.3%. Forecast Stability = 88%. FVA (model step) = +7.3pp. Override Effectiveness = 52%. Completeness = 98%.
**Output:** Complete metric set with computation evidence.

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Invoked By | AB-D-014 (Evaluate Forecast Quality) |
| Referenced By | FS-D-014 |
| Produces | Forecast quality metrics consumed by DE-D-011 |
| Depends On | SE-D-003, SE-C-021, PO-D-041 |

---

## BA-D-009 – Determine Forecast Quality State

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Evaluate Demand Quality |
| Knowledge Category | Business State Determination |
| Stage | State |
| Output Type | State |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the measured forecast quality metrics, what is the enterprise's authoritative assessment of forecast quality for the evaluation period?"**

### 3. Business Intent

The metrics from BA-D-008 tell the enterprise *what the numbers are*. This algorithm determines *what those numbers mean*. It applies the quality thresholds from PO-D-041 to the measured metrics and produces an authoritative quality classification for each dimension and an overall quality assessment. It does not publish the assessment; that is the responsibility of the owning Aggregate Behavior.

### 4. Architectural Principle

This algorithm determines the authoritative forecast quality state by applying governed thresholds to measured metrics. It does not redefine the thresholds; those belong to policy.

### 5. Business Explanation

A board reviewing forecast performance does not want a table of numbers. They want a clear judgment: was forecast quality Excellent, Good, Adequate, Poor, or Critical? This algorithm provides that judgment. It applies the enterprise's own quality thresholds to each metric, classifies each dimension, and derives an overall quality state using the composition rules in PO-D-041.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes (for identical metrics and policy) |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Forecast quality metrics | BA-D-008 output | WAPE, Forecast Bias, Forecast Accuracy, and optional metrics. | Yes | Algorithm not applicable. |
| Forecast Measurement Policy | PO-D-041 | Governed quality thresholds per metric and overall composition rules. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Per-Metric Classification | Quality classification (Excellent, Good, Adequate, Poor, Critical) for each mandatory metric. |
| Overall Quality State | The authoritative overall forecast quality assessment. |
| Determination Rationale | Business-language explanation of the overall quality state. |
| Determination Confidence | Enterprise confidence in the quality state (High, Medium, Low). |

### 9. Preconditions

- BA-D-008 has produced a complete set of mandatory metrics.
- PO-D-041 is current and defines quality thresholds and composition rules.

### 10. Evaluation Methodology


1. For each mandatory metric, classify the measured value against the quality thresholds defined in PO-D-041. The policy defines the threshold ranges for Excellent, Good, Adequate, Poor, and Critical for each metric (WAPE, Forecast Bias, Forecast Accuracy, and any optional metrics configured for the evaluation). This algorithm applies those thresholds to produce per-metric classifications. It does not define the threshold values; those belong solely to PO-D-041.
2. For optional metrics, classify if data is available.
3. Derive the overall quality state using the composition rules in PO-D-041:
   - If any mandatory metric is Critical, the overall is capped at Poor.
   - If any mandatory metric is Poor, the overall is capped at Adequate.
   - If all mandatory metrics are Good or Excellent, the overall is the lowest among them.
4. Assess confidence from the completeness score and data quality.
5. Generate the determination rationale.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-403 | Forecast quality metrics shall be computed according to PO-D-041. |

### 12. Assumptions

- The metric values are accurate and complete.
- The quality thresholds in PO-D-041 reflect the enterprise's performance expectations.

### 13. Explainability

Every quality classification is traceable to the specific metric value, the threshold it was compared against, and the policy version.

### 14. Postconditions / Guarantees

- An overall quality state is determined with rationale and confidence.
- No quality assessment is published by this algorithm.

### 15. Example

**Inputs:** WAPE 8.5% (Excellent), Bias +1.2% (Excellent), Accuracy 91.5% (Good). PO-D-041: overall is the lowest mandatory metric = Good.
**Output:** Overall Quality State: Good. Rationale: "WAPE Excellent (8.5%), Bias Excellent (+1.2%), Accuracy Good (91.5%). Overall Good per PO-D-041 composition rules." Confidence: High (completeness 98%).

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Evaluate Demand Quality (CA-D-007) |
| Governed By | PO-D-041 |
| Invoked By | AB-D-014 (Evaluate Forecast Quality) |
| Referenced By | FS-D-014 |
| Produces | Quality state consumed by DE-D-011 |
| Depends On | BA-D-008, PO-D-041 |

---

## BA-D-010 – Evaluate Demand Exception Evidence

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Detect Demand Exceptions |
| Knowledge Category | Condition Assessment |
| Stage | Assessment |
| Output Type | Assessment |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the current demand evidence, does the situation meet the enterprise's criteria for demand exception evidence, and if so, what is the assessed severity?"**

### 3. Business Intent

Not every forecast bias, data gap, or model degradation warrants formal attention. This algorithm evaluates the current demand evidence—forecast quality metrics, demand behavior assessments, data completeness scores—against the detection thresholds defined in the Exception Detection Policy, and produces a structured assessment of whether a condition exists and how severe it is. It does not manage the condition lifecycle.

### 4. Architectural Principle

This algorithm evaluates demand evidence against governed detection criteria. It produces an evidence-based condition assessment. It does not create, update, or resolve conditions; those actions are the responsibility of the owning Aggregate Behavior.

### 5. Business Explanation

A demand manager staring at multiple dashboards cannot determine which situations require formal attention. This algorithm applies the enterprise's own definition of what constitutes a demand planning condition. It evaluates the evidence against the thresholds for each condition type, determines whether any thresholds are breached, and assesses the severity of each breach. The result tells the enterprise: "Yes, this situation meets our criteria for a formal condition, and here is how severe it is."

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes (for identical evidence and policy) |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Forecast quality metrics | SE-D-008, BA-D-008 | Recent accuracy, bias, and stability data. | No | Quality-based conditions cannot be detected. |
| Demand Understanding | SE-D-002 | Current demand interpretation. | No | Health-based conditions cannot be detected. |
| Demand Behavior Assessment | SE-D-004 | Current behavior state for relevant entities. | No | Behavior-based conditions cannot be detected. |
| Exception Detection Policy | PO-D-044 | Governed condition types, triggering criteria, and severity rules. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Exception Evidence Determination | Whether exception evidence exists: Detection Evidence Exists, Resolution Evidence Exists, or No Evidence. |
| Exception Type | The governed category of exception (e.g., Forecast Bias Elevated, Data Completeness Gap, Model Performance Degradation). |
| Severity Assessment | Multi-dimensional severity: Business Impact, Urgency, Scope. |
| Triggering Evidence | The specific evidence that met the detection thresholds. |
| Assessment Confidence | Enterprise confidence in the determination (High, Medium, Low). |
| Reason Codes | Governed codes from PO-D-044. |

### 9. Preconditions

- PO-D-044 is current and defines at least one condition type with triggering criteria.
- At least one source of evidence is available.

### 10. Evaluation Methodology

1. Load the condition types and triggering criteria from PO-D-044.
2. For each condition type, gather the relevant evidence and evaluate against the criteria.
3. If criteria are met, the determination is Condition Exists. Assess severity across the dimensions defined in PO-D-044.
4. If no criteria are met, the determination is No Condition.
5. Assess confidence based on evidence quality and completeness.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-311 | A Demand Planning Condition shall only be recognized if detection evidence meets the thresholds in PO-D-044. |

### 12. Assumptions

- Available evidence is current and accurately reflects the demand situation.
- The detection criteria in PO-D-044 are comprehensive and aligned with enterprise risk tolerance.

### 13. Explainability

Every condition determination is traceable to the specific evidence that triggered it and the policy criteria that were satisfied.

### 14. Postconditions / Guarantees

- A condition assessment is produced with determination, type, severity, and evidence.
- No condition lifecycle management is performed.

### 15. Example

**Inputs:** Forecast Bias for Segment A = 18% (PO-D-044 threshold: >10%).
**Processing:** 18% > 10% → Condition Exists. Type: Forecast Bias Elevated. Severity: Business Impact Moderate, Urgency Short-term, Scope Segment A.
**Output:** Condition Exists, Forecast Bias Elevated, Severity Moderate.

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Detect Demand Exceptions (CA-D-008) |
| Governed By | PO-D-044 |
| Invoked By | AB-D-015 (Detect Demand Planning Conditions) |
| Referenced By | FS-D-015 |
| Produces | Condition assessment consumed by DE-D-012 |
| Depends On | SE-D-002, SE-D-004, SE-D-008, PO-D-044 |

---

## BA-D-011 – Assess Demand Exception Lifecycle Evidence

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Structured Reasoning |
| Domain | Detect Demand Exceptions |
| Knowledge Category | Business State Determination |
| Stage | State |
| Output Type | State |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the exception evidence assessment, what is the appropriate exception lifecycle evidence to publish to Core Exception Management?"**

### 3. Business Intent

The condition assessment from BA-D-010 determines whether evidence supports a condition. This algorithm determines the lifecycle state—whether a new condition should be created (Active), whether an existing condition's severity should be updated, or whether a condition should be resolved. It applies the lifecycle transition rules from PO-D-044.

### 4. Architectural Principle

This algorithm determines the authoritative lifecycle state of a demand planning condition by applying governed transition rules to the condition assessment and the current state.

### 5. Business Explanation

A condition is not simply "exists" or "does not exist." It follows a lifecycle: it is detected, its severity may change as evidence evolves, and it is eventually resolved when the underlying situation returns to normal. This algorithm determines where in that lifecycle the condition should be.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Condition Assessment | BA-D-010 output | Determination, type, severity, and evidence. | Yes | Algorithm not applicable. |
| Current Condition State |  | Current lifecycle state (Active, Resolved) and severity. | No | Treated as no existing condition. |
| Exception Detection Policy | PO-D-044 | Governed lifecycle transition rules. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Determined Lifecycle State | Active (new or updated) or Resolved. |
| State Transition Occurred | Boolean. |
| Severity Level | Critical, High, Medium, or Low (if Active). |
| Determination Rationale | Business-language explanation. |

### 9. Preconditions

- BA-D-010 has produced a condition assessment.
- PO-D-044 is current.

### 10. Evaluation Methodology

1. Load the condition assessment from BA-D-010, the current condition state from SE-D-009 (if an active condition exists for the same business identity), and the lifecycle transition rules from PO-D-044.
2. Determine the lifecycle state by applying the transition rules defined in PO-D-044 to the condition assessment and the current state:
   - If the condition assessment indicates a condition exists and the transition rules determine the state should be Active, the determined lifecycle state is Active, with the severity level from the condition assessment.
   - If the condition assessment indicates a condition exists and the transition rules determine the severity has changed, the determined lifecycle state is Active, with the updated severity level.
   - If the condition assessment indicates no condition exists and the transition rules determine the condition should be resolved, the determined lifecycle state is Resolved.
3. State Transition Occurred is true if the determined lifecycle state or severity differs from the current state.
4. The owning Functional Specification (FS-D-015) is responsible for translating this state determination into the appropriate publication action—emitting EV-D-022 and publishing BN-D-022 or BN-D-023. This algorithm determines only the authoritative lifecycle evidence.
5. Generate the determination rationale identifying the specific transition rule applied and the evidence that triggered it.


### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-311 | Demand exception evidence shall only be published if the detection evidence meets the thresholds in PO-D-044. |
| BR-D-121 | Demand exception evidence shall reference SE-C-019 Exception as the authoritative enterprise record. |
| BR-D-122 | Demand exception detection evidence shall include constraint reference, affected scope, severity assessment, and triggering evidence. |
| BR-D-123 | Demand exception resolution evidence shall be published when the underlying data returns to within governed thresholds. |

### 12. Assumptions

- The condition assessment is accurate.
- The current condition state accurately reflects the last determined state.

### 13. Explainability

Every lifecycle state determination is traceable to the assessment evidence and the transition rule applied.

### 14. Postconditions / Guarantees

- A lifecycle state is determined.
- No state transition is applied to the aggregate by this algorithm.

### 15. Example

**Inputs:** Condition Assessment: Condition Exists, Forecast Bias Elevated, Severity High. Current state: Active, Severity Medium.
**Processing:** Condition Exists with different severity → Active (updated).
**Output:** Determined State: Active, Severity High. Transition: true.

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Detect Demand Exceptions (CA-D-008) |
| Governed By | PO-D-044 |
| Invoked By | AB-D-015 (Detect Demand Planning Conditions) |
| Referenced By | FS-D-015 |
| Produces | Lifecycle state determination consumed by AB-D-015 |
| Depends On | BA-D-010, PO-D-044 |

---

## BA-D-012 – Compose Demand Explanation

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Deterministic Composition |
| Domain | Explain Demand |
| Knowledge Category | Explanation Composition |
| Stage | Explanation |
| Output Type | Explanation |
| Explainable | Full |

> **Architectural Note (ARS §15.7.3):** This algorithm is an Explanation algorithm. It composes the deterministic, evidence-based reasoning behind an enterprise conclusion from pre-existing governed knowledge. It does not generate new reasoning and does not fit into the evaluation semantic progression.

### 2. Purpose

Answer the enterprise question: **"Why did the enterprise reach, or deliberately not reach, this demand conclusion, and what evidence, decisions, and policies support it?"**

### 3. Business Intent

Every demand intelligence conclusion carries within it the evidence, policies, and assumptions that were preserved when it was made. This algorithm composes that preserved knowledge into a deterministic Structured Reasoning Graph—the canonical, immutable explanation of why the conclusion was reached. It does not write natural language; it produces the reasoning structure from which natural language can be derived.

### 4. Architectural Principle

This algorithm composes existing enterprise knowledge into a structured explanation. It does not invent reasoning, fill gaps with assumptions, or generate prose. The canonical explanation is representation-independent.

### 5. Business Explanation

An auditor asks: "Why was this forecast approved when the bias was elevated?" A planner asks: "Why was this item classified as Critical?" An AI agent needs to explain a recommendation. This algorithm provides the structured, traceable answer by assembling the evidence, decisions, policies, and assumptions that were in effect when the conclusion was made.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes (identical artifact, evidence, policies, and template produce identical explanation) |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Explained artifact | Any demand intelligence artifact | The conclusion to be explained. | Yes | Algorithm not applicable. |
| Preserved evidence set | Historical versions of evidence, decisions, policies, assumptions | The knowledge that contributed to the conclusion. | Yes | Explanation marked "Unavailable – Incomplete Evidence." |
| Explanation template | Explanation Template Catalog | Required reasoning elements for the artifact type. | Yes | Default template applied; flagged. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Canonical Reasoning Structure | The complete, deterministic graph of what influenced what, with typed relationships and provenance on every node. |
| Explanation Confidence | Inherited evidence confidence, evidence completeness, and evidence consistency. |
| Template Version Reference | The version of the template used. |

### 9. Preconditions

- The explained artifact exists and has preserved evidence.
- An explanation template is available.

### 10. Evaluation Methodology

1. Load the explained artifact and its type.
2. Load the explanation template for the artifact type.
3. Gather all preserved evidence, decisions, policies, and assumptions at their historical versions.
4. Assemble the Structured Reasoning Graph by linking evidence to decisions, decisions to policies, and assumptions to outcomes, using the template's relationship vocabulary.
5. Compute explanation confidence from inherited evidence confidence, evidence completeness, and evidence consistency.

### 11. Business Rules

| ID | Rule |
|----|------|
| (algorithm rule) | The explanation shall be composed exclusively from preserved evidence and policies at their historical versions. |
| (algorithm rule) | The Structured Reasoning Graph must be deterministic. |
| (algorithm rule) | The algorithm shall not invent reasoning or generate prose. |

### 12. Assumptions

- The explained artifact was produced with sufficient preserved evidence.
- Historical versions of evidence, policies, and templates are accessible.

### 13. Explainability

The explanation is itself the artifact of explainability. Every node in the Structured Reasoning Graph is traceable to its source.

### 14. Postconditions / Guarantees

- A Demand Explanation content is produced.
- The Structured Reasoning Graph is deterministic and complete per the template.

### 15. Example

**Inputs:** Explained artifact: DE-D-004 (Approve Forecast Publication) for PUB-2027-003. Preserved evidence: Forecast Confidence Index 87%, completeness 98%, assumption sign-off complete. Template: Decision Explanation.
**Processing:** Graph built with nodes for Confidence Index, completeness, sign-off, and policy threshold, linked by "Influenced" and "Determined" edges.
**Output:** Canonical Reasoning Structure. Confidence: High (all evidence complete and consistent).

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Explain Demand (CA-D-009) |
| Governed By | PO-D-047 |
| Invoked By | AB-D-016 (Establish Demand Explanation) |
| Referenced By | FS-D-016 |
| Produces | Explanation content for SE-D-010 |
| Depends On | Preserved evidence from all Demand Intelligence capabilities |

---

## BA-D-013 – Derive Demand Learning

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Analytical Discovery |
| Domain | Learn From Demand |
| Knowledge Category | Learning Derivation |
| Stage | Discovery |
| Output Type | Learning |
| Explainable | Full |

> **Architectural Note (ARS §15.7.2):** This algorithm is a Discovery algorithm. It analyses historical evidence across multiple periods and capability boundaries, identifies recurring patterns, and produces candidate learnings. It does not determine whether the learning is adopted; that is a governance decision.

### 2. Purpose

Answer the enterprise question: **"What recurring patterns, supported by historical evidence, suggest that the enterprise should change its demand planning policies, models, or practices?"**

### 3. Business Intent

The enterprise accumulates a rich history of forecasts, quality assessments, overrides, exceptions, and explanations. This algorithm systematically analyses that history to discover patterns that recur across multiple periods—planner overrides that consistently degrade accuracy, seasonal bias patterns, classification changes that improve model performance—and produces candidate learnings with Pattern Confidence and Intervention Confidence. It does not decide whether to adopt the learning; governance makes that determination.

### 4. Architectural Principle

This algorithm derives enterprise learning by systematically analysing historical outcomes and patterns. It produces candidate knowledge—what the enterprise has concluded, with what confidence—but does not determine whether to act on that knowledge.

### 5. Business Explanation

After several quarters of operations, the enterprise has a wealth of data. Somewhere in that data are signals: overrides that consistently make forecasts worse, suppliers whose delays follow predictable patterns, classification rules that could be improved. This algorithm finds those signals by analysing quality assessments, exception histories, explanation records, and operational outcomes across multiple periods.

### 6. Algebraic Properties

| Property | Value |
|----------|-------|
| Deterministic | Yes (for identical evidence and policy) |
| Idempotent | Yes |
| Pure | Yes |
| Order Sensitive | No |
| Explainable | Full |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Historical demand evidence | SE-D-008, SE-C-019 (via Core Exception Management), SE-D-010, SE-D-005, SE-D-006, SE-D-007, SE-D-004 | The preserved history of demand outcomes across multiple periods. | Yes | Algorithm not applicable. |
| Learning Analysis Policy | PO-D-048 | Minimum recurrence threshold, evidence sufficiency, confidence criteria. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Observed Pattern | Description of the recurring phenomenon with recurrence evidence. |
| Enterprise Learning Statement | The enterprise's conclusion about what the pattern means. |
| Pattern Confidence | Confidence that the pattern genuinely exists (High, Medium, Low). |
| Improvement Opportunities | Proposed improvements derived from the learning. |
| Intervention Confidence | For each opportunity, confidence that acting would produce benefit. |
| Supporting Evidence | References to specific historical records and periods. |

### 9. Preconditions

- Sufficient historical evidence exists spanning the minimum periods defined by PO-D-048.
- PO-D-048 is current.

### 10. Evaluation Methodology

1. Define the analysis scope from PO-D-048.
2. Systematically examine historical evidence across the eligible scope.
3. Identify candidate patterns meeting the recurrence threshold defined in PO-D-048. The policy governs the minimum number of occurrences, the time horizon over which recurrence is assessed, the evidence diversity requirements, and any additional statistical significance criteria. This algorithm applies those criteria to the historical evidence without defining the specific thresholds.
4. For each candidate, formulate the Enterprise Learning Statement.
5. Assess Pattern Confidence: recurrence frequency, evidence diversity, statistical significance.
6. Identify Improvement Opportunities.
7. Assess Intervention Confidence: causal evidence, historical analogues, expected impact.

### 11. Business Rules

| ID | Rule |
|----|------|
| (algorithm rule) | A learning shall only be derived when evidence demonstrates recurrence across multiple periods. |
| (algorithm rule) | Pattern Confidence and Intervention Confidence shall be assessed independently. |

### 12. Assumptions

- Historical evidence is accurate and reflects true enterprise outcomes.
- Patterns recurring across multiple periods are likely genuine.

### 13. Explainability

Every learning is traceable to the specific historical evidence and analysis criteria.

### 14. Postconditions / Guarantees

- Candidate learnings are produced (may be empty if no patterns meet thresholds).

### 15. Example

**Inputs:** Q1–Q3 2027 quality assessments, override records. PO-D-048: recurrence threshold 3 periods.
**Processing:** Overrides in Segment B value-destroying 68% of time for 3 consecutive quarters. Pattern Confidence: High. Learning: "Planner overrides in Segment B systematically degrade forecast accuracy." Improvement: "Reduce override deviation limit from ±50% to ±30%." Intervention Confidence: Medium.
**Output:** Candidate learning with Pattern Confidence High, Intervention Confidence Medium.

### 16. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Learn From Demand (CA-D-010) |
| Governed By | PO-D-048 |
| Invoked By | AB-D-017 (Establish Demand Learning) |
| Referenced By | FS-D-017 |
| Produces | Candidate learnings for SE-D-011 |
| Depends On | Historical evidence from all Demand Intelligence capabilities, PO-D-048 |

---

## BA-D-014 — Derive Demand Behavior Baseline

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Deterministic Computation |
| Domain | Sense Demand |
| Knowledge Category | Baseline Derivation |
| Stage | Parameter |
| Output Type | Parameter |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the Demand Understanding, historical observations, and governed policy, what is the expected demand level and normal variation for this monitored Item-Location?"**

### 3. Business Intent

Demand sensing requires a baseline against which incoming signals are evaluated. This algorithm derives the expected demand level and standard deviation of normal variation for a monitored Item-Location from the Demand Understanding, historical observations, and governed policy. It does not evaluate signals; it provides the baseline parameters that BA-D-003 consumes.

### 4. Architectural Principle

This algorithm computes baseline parameters from governed inputs. It does not evaluate signals, manage state transitions, or decide on responses.

### 5. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes | Same inputs produce identical baseline. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects. |
| Order Sensitive | No | |
| Explainable | Full | Every baseline parameter is traceable to its source. |

### 6. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Demand Understanding | SE-D-002 (Published) | Current demand interpretation providing scope-level context. | Yes | Algorithm not applicable. |
| Historical observations | SE-D-001 (Accepted) | Historical demand observations for the monitored Item-Location. | Yes | Algorithm not applicable. |
| Demand Sensing Policy | PO-D-031 | Governed baseline derivation parameters. | Yes | Algorithm not applicable. |

### 7. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Expected Demand Level | The expected demand quantity for the monitored Item-Location. |
| Standard Deviation | The standard deviation of normal variation. |
| Baseline Timestamp | When this baseline was derived. |
| Baseline Confidence | Enterprise confidence in the baseline (High, Medium, Low). |

### 8. Preconditions

- Demand Understanding is available for the Planning Scope.
- Historical observations are available for the monitored Item-Location.
- PO-D-031 is current.

### 9. Evaluation Methodology

1. Load the Demand Understanding for the Planning Scope.
2. Load historical observations for the monitored Item-Location.
3. Compute the expected demand level from historical observations using the method governed by PO-D-031.
4. Compute the standard deviation of normal variation from historical observations using the method governed by PO-D-031.
5. Assess baseline confidence from the length and quality of historical observations.

### 10. Business Rules

| ID | Rule |
|----|------|
| BR-D-300 | Baseline parameters are governed by PO-D-031. |

### 11. Assumptions

- Historical observations are accurate and complete.
- PO-D-031 reflects the enterprise's baseline derivation methodology.

### 12. Explainability

Every baseline parameter is traceable to the specific historical observations and the policy method applied.

### 13. Postconditions / Guarantees

- Baseline parameters are produced for the monitored Item-Location.
- No signal evaluation is performed by this algorithm.

### 14. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Sense Demand (CA-D-003) |
| Governed By | PO-D-031 |
| Invoked By | AB-D-010 (Maintain Demand Behavior Understanding) |
| Referenced By | FS-D-009 |
| Produces | Baseline parameters consumed by BA-D-003 |
| Depends On | SE-D-002, SE-D-001, PO-D-031 |

---

## BA-D-015 — Reconcile Forecast Hierarchy

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Deterministic Computation |
| Domain | Forecast Demand |
| Knowledge Category | Forecast Reconciliation |
| Stage | Constructive |
| Output Type | Forecast Publication |
| Explainable | Full |

### 2. Purpose

Answer the enterprise question: **"Given the unreconciled forecast lines and the reconciliation method governed by PO-D-029, what are the hierarchically consistent forecast lines?"**

### 3. Business Intent

Forecast reconciliation ensures that bottom-up forecasts aggregated to any parent level equal the reconciled top-down forecast at that level. This algorithm applies the reconciliation method governed by PO-D-029 to produce hierarchically consistent forecast lines.

### 4. Architectural Principle

This algorithm applies the reconciliation method governed by PO-D-029. It does not define the reconciliation method; that belongs to policy. It does not decide whether the forecast is acceptable; governance determines that.

### 5. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes | Same inputs produce identical reconciled forecasts. |
| Idempotent | Yes | Repeated execution yields the same result. |
| Pure | Yes | No side effects. |
| Order Sensitive | No | |
| Explainable | Full | Every reconciled forecast line is traceable to its unreconciled source. |

### 6. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Unreconciled forecast lines | BA-D-002 output | The forecast lines before reconciliation. | Yes | Algorithm not applicable. |
| Forecast Reconciliation Policy | PO-D-029 | Governed reconciliation method. | Yes | Algorithm not applicable. |

### 7. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Reconciled forecast lines | The hierarchically consistent forecast lines. |
| Reconciliation evidence | References to the reconciliation method applied. |

### 8. Preconditions

- BA-D-002 has produced unreconciled forecast lines.
- PO-D-029 is current.

### 9. Evaluation Methodology

1. Load the unreconciled forecast lines from BA-D-002.
2. Load the reconciliation method from PO-D-029.
3. Apply the reconciliation method to produce hierarchically consistent forecast lines.
4. Record reconciliation evidence.

### 10. Business Rules

| ID | Rule |
|----|------|
| BR-D-410 | All published forecasts shall satisfy hierarchical consistency as defined by the reconciliation method governed by PO-D-029. |

### 11. Assumptions

- Unreconciled forecast lines are accurate and complete.
- PO-D-029 reflects the enterprise's reconciliation methodology.

### 12. Explainability

Every reconciled forecast line is traceable to its unreconciled source and the reconciliation method applied.

### 13. Postconditions / Guarantees

- Reconciled forecast lines are produced.
- Hierarchical consistency is satisfied.

### 14. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Forecast Demand (CA-D-002) |
| Governed By | PO-D-029 |
| Invoked By | AB-D-007 (Produce Forecast Projection) |
| Referenced By | FS-D-006 |
| Produces | Reconciled forecast lines for SE-D-003 |
| Depends On | BA-D-002, PO-D-029 |

---

## BA-D-016 – Model Intervention Lift

### 1. Business Classification

| Attribute | Value |
|-----------|-------|
| Type | Business Algorithm |
| Nature | Constructive Reasoning |
| Domain | Model Demand Interventions |
| Knowledge Category | Intervention Impact Modeling |
| Stage | Constructive |
| Output Type | Plan |
| Explainable | Full |

> **Architectural Note (ARS §15.7.1):** This algorithm is a Constructive algorithm. It synthesises a demand impact assessment from intervention characteristics, historical elasticity data, and current demand context. It does not select the modeling approach; that selection is governed by PO-D-050.

### 2. Purpose

Answer the enterprise question: **"Given the planned commercial intervention and historical demand data, what is the expected demand lift for this item-location?"**

### 3. Business Intent

The enterprise plans commercial interventions (promotions, price changes, marketing events) that affect demand. This algorithm computes the expected demand impact from historical elasticity patterns, intervention characteristics, and current demand context. It produces a deterministic lift estimate with confidence, enabling the forecast capability to incorporate the intervention effect.

### 4. Architectural Principle

This algorithm applies the modeling approach governed by PO-D-050. It does not select the approach, does not modify the baseline forecast, and does not make publication decisions. It computes; governance decides.

### 5. Business Explanation

When the enterprise plans a promotion or price change, planners need to know: "How much additional demand will this generate?" This algorithm answers that question by examining how similar interventions affected demand historically, adjusting for current demand conditions, and producing a lift estimate with confidence. The output feeds into the forecast publication as a forward-looking adjustment.

### 6. Algebraic Properties

| Property | Value | Meaning |
|----------|-------|---------|
| Deterministic | Yes (for identical inputs and approach) | Same inputs produce identical lift. |
| Idempotent | Yes | Repeated execution yields same result. |
| Pure | Yes | No side effects. |
| Order Sensitive | No | |
| Explainable | Full | Every lift is traceable to historical evidence and approach. |

### 7. Input Contract

| Input | Source | Business Meaning | Required | Handling of Missing |
|-------|--------|------------------|----------|---------------------|
| Intervention definition | SE-C-039 (Scenario Adjustment) | Type, magnitude, temporal scope of the intervention. | Yes | Algorithm not applicable. |
| Item and Location | SE-C-001, SE-C-002 | The affected item-location. | Yes | Algorithm not applicable. |
| Historical forecast-actual pairs | SE-D-003 (Forecast Publication) | Past forecast vs actual for elasticity estimation. | Yes | Fallback approach per PO-D-050. |
| Demand Understanding | SE-D-002 | Current demand baseline context. | No | Confidence reduced. |
| Intervention Modeling Governance | PO-D-050 | Modeling approach preference, confidence thresholds. | Yes | Algorithm not applicable. |

### 8. Output Contract

| Component | Business Meaning |
|-----------|------------------|
| Assessed Demand Lift | The expected demand change (non-negative Quantity). |
| Lift Confidence | Enterprise confidence in the estimate (0–100). |
| Temporal Validity | The time interval during which the intervention is active. |
| Model Provenance | The modeling approach used. |
| Computation Evidence | Historical data points and elasticity factors used. |

### 9. Behavioral Specification Contract

| Section | Value |
|---------|-------|
| **Preconditions** | Intervention definition is active. PO-D-050 is current. |
| **Business Behavior** | Select the modeling approach from PO-D-050 preference order. Compute the assessed demand lift. Assess confidence. Determine temporal validity from the intervention definition. |
| **Exceptional Conditions** | If insufficient historical data, apply fallback approach. If no approach viable, produce zero lift with zero confidence. |
| **Postconditions** | A complete impact assessment is produced with lift, confidence, validity, and provenance. |
| **Outcome When Preconditions Are Not Satisfied** | Algorithm not applicable. |

### 10. Evaluation Methodology

1. Load the intervention definition from SE-C-039.
2. Select the modeling approach from PO-D-050 preference order.
3. Gather historical forecast-actual pairs for the item-location.
4. Compute elasticity factor from historical data.
5. Apply intervention characteristics (type, magnitude, duration) to the elasticity factor.
6. Adjust for current demand context from SE-D-002.
7. Compute the assessed demand lift (clamp to non-negative per BR-D-414).
8. Assess confidence based on data sufficiency and approach reliability.
9. Determine temporal validity from the intervention definition.
10. Record model provenance and computation evidence.

### 11. Business Rules

| ID | Rule |
|----|------|
| BR-D-414 | Assessed Demand Lift must be non-negative. |
| PO-D-050 | Governs modeling approach selection and confidence thresholds. |

### 12. Assumptions

- Historical forecast-actual pairs accurately reflect past intervention effects.
- The intervention definition in SE-C-039 is complete and active.
- PO-D-050 provides a valid modeling approach preference order.

### 13. Explainability

Every lift estimate is traceable to the specific historical data points, elasticity factors, intervention characteristics, and modeling approach used. The computation evidence enables full audit.

### 14. Postconditions / Guarantees

- A complete impact assessment is produced.
- The lift is non-negative.
- The confidence reflects data sufficiency.
- No publication decision is made by this algorithm.

### 15. Applicability & Exceptional Conditions

| Condition | Behavior |
|-----------|----------|
| Intervention inactive | Algorithm not applicable. |
| Insufficient historical data | Fallback approach per PO-D-050. |
| No viable approach | Zero lift, zero confidence, flagged for planner. |

### 16. Example

**Inputs:** Intervention: "20% price reduction on Item P-1001 at DC-01 for 14 days." Historical data: 52 weeks of forecast-actual pairs. PO-D-050: Historical Elasticity approach preferred.
**Processing:** Elasticity factor computed from historical promotions: 1.8x. Current demand baseline: 500 units/week. Intervention duration: 14 days. Assessed lift: 500 × 0.8 × 1.8 = 720 additional units over 14 days. Confidence: 82% (sufficient history, reliable approach).
**Output:** Assessed Demand Lift: 720 units. Confidence: 82%. Temporal Validity: 14-day window. Model Provenance: Historical Elasticity.

### 17. Traceability

| Attribute | Value |
|-----------|-------|
| Owned By | Model Demand Interventions (CA-D-011) |
| Governed By | PO-D-050 |
| Invoked By | AB-D-018 (Assess Demand Intervention Impact) |
| Referenced By | FS-D-018 |
| Produces | Impact assessment for SE-D-018 |
| Depends On | SE-C-039, SE-D-002, SE-D-003, PO-D-050 |

---

## Dependency Rule Compliance Verification

| Algorithm | Stage | Depends On | Stage of Dependency | Rule |
|-----------|-------|------------|---------------------|------|
| BA-D-001 | Assessment | (none — SE objects, PO) | — | ✓ |
| BA-D-002 | Constructive | (SE objects, PO) | — | ✓ |
| BA-D-003 | Assessment | SE-D-002, PO-D-031 | — | ✓ |
| BA-D-004 | State | BA-D-003 | Assessment (N-1) | ✓ |
| BA-D-005 | Assessment | SE-D-002, PO-D-035 | — | ✓ |
| BA-D-006 | State | SE-C-021, PO-D-037 | — | ✓ |
| BA-D-007 | Assessment | SE objects, PO-D-039 | — | ✓ |
| BA-D-008 | Assessment | SE-D-003, SE-C-021, PO-D-041 | — | ✓ |
| BA-D-009 | State | BA-D-008 | Assessment (N-1) | ✓ |
| BA-D-010 | Assessment | SE objects, PO-D-044 | — | ✓ |
| BA-D-011 | State | BA-D-010 | Assessment (N-1) | ✓ |
| BA-D-012 | Explanation | (artifacts) | — | ✓ |
| BA-D-013 | Discovery | (historical evidence) | — | ✓ |
| BA-D-014 | Parameter | SE-D-002, SE-D-001, PO-D-031 | — | ✓ |
| BA-D-015 | Constructive | BA-D-002, PO-D-029 | Constructive (same stage) | ✓ |
| BA-D-016 | Constructive | SE-C-039, SE-D-002, SE-D-003, PO-D-050 | Constructive (same stage) | ✓ |

No dependency rule violations. All State algorithms depend on Assessment algorithms (N-1). No same-stage dependencies. Constructive, Explanation, and Discovery algorithms may consume any artifact type per ARS §15.7.

---

# Appendix A — Integration Matrix

| Publisher | Notification | Consumer | Behavior |
|-----------|-------------|----------|-----------|
| Core (Enterprise Picture Management) | BN-C-001 Enterprise Picture Published | Understand Demand | Revise the Demand Understanding with the latest demand facts from the Enterprise Picture. |
| Detect Demand Exceptions | BN-D-022 Demand Exception Detection Evidence | Core Exception Management (CA-C-020) | Create or update SE-C-019 Exception. |
| Detect Demand Exceptions | BN-D-023 Demand Exception Resolution Evidence | Core Exception Management (CA-C-020) | Resolve SE-C-019 Exception. |
| Understand Demand | BN-D-001 Demand Understanding Published | Forecast Demand | Update training data with latest demand interpretation. |
| Understand Demand | BN-D-001 | Sense Demand | Refresh evaluation baseline. |
| Understand Demand | BN-D-001 | Segment Demand | Trigger re-evaluation for volume/variability types. |
| Understand Demand | BN-D-001 | Classify Demand | Trigger re-evaluation for statistical dimensions. |
| Understand Demand | BN-D-001 | Prioritize Demand | Trigger re-evaluation if volume contribution changed. |
| Understand Demand | BN-D-001 | Supply Intelligence | Consume for supply planning baseline. |
| Understand Demand | BN-D-001 | Promise Intelligence | Consume for order promising context. |
| Understand Demand | BN-D-001 | Scenario Intelligence | Consume for scenario baseline. |
| Core (Enterprise Picture Management) | BN-C-001 Enterprise Picture Published | Understand Demand | Revise the Demand Understanding with the latest demand facts from the Enterprise Picture. |
| Understand Demand | BN-D-002 Demand Observation Quarantined | Demand Data Steward | Manual review of quarantined observation. |
| Understand Demand | BN-D-003 Demand Observation Rejected | Demand Data Steward | Notification of permanently excluded observation. |
| Understand Demand | BN-D-005 Demand Observation Received | Operational Monitoring | Record of observation receipt. |
| Understand Demand | BN-D-006 Demand Observation Accepted | Operational Monitoring | Record of observation acceptance. |
| Forecast Demand | BN-D-010 Forecast Publication Generation Established | Enterprise Monitoring | Track forecast publication generation execution. |
| Forecast Demand | BN-D-011 Forecast Published | Understand Demand | Revise Demand Understanding with forward-looking context. |
| Forecast Demand | BN-D-011 | Sense Demand | Optionally update evaluation context. |
| Forecast Demand | BN-D-011 | Evaluate Demand Quality | Begin accuracy measurement when actuals arrive. |
| Forecast Demand | BN-D-011 | Supply Intelligence | Consume for supply planning. |
| Forecast Demand | BN-D-011 | Promise Intelligence | Consume for order promising. |
| Forecast Demand | BN-D-011 | Scenario Intelligence | Consume for scenario planning. |
| Forecast Demand | BN-D-012 Forecast Override Applied | Evaluate Demand Quality | Track override impact on accuracy. |
| Sense Demand | BN-D-015 Demand Behavior Changed | Demand Planners | Review demand behavior change. |
| Sense Demand | BN-D-015 | Segment Demand | Trigger re-evaluation for affected classification types. |
| Sense Demand | BN-D-015 | Classify Demand | Trigger re-evaluation for affected behavior dimensions. |
| Sense Demand | BN-D-015 | Detect Demand Exceptions | Evaluate behavior change against condition thresholds. |
| Sense Demand | BN-D-016 Critical Demand Behavior Requires Action | Forecast Demand | Initiate out-of-cycle forecast refresh. |
| Sense Demand | BN-D-016 | Demand Manager | Escalation of critical demand behavior. |
| Segment Demand | BN-D-017 Planning Classification Changed | Forecast Demand | Update model selection for affected entity. |
| Segment Demand | BN-D-017 | Prioritize Demand | Re-evaluate priority for affected entity. |
| Segment Demand | BN-D-017 | Inventory Planning (external) | Update inventory policy. |
| Classify Demand | BN-D-019 Demand Behavior Classification Changed | Forecast Demand | Update model selection for affected entity. |
| Classify Demand | BN-D-019 | Detect Demand Exceptions | Adjust detection thresholds. |
| Classify Demand | BN-D-019 | Explain Demand | Provide evidence for explanation generation. |
| Classify Demand | BN-D-019 | Prioritize Demand | Adjust planner attention. |
| Classify Demand | BN-D-019 | Inventory Planning (external) | Update safety stock policy. |
| Classify Demand | BN-D-019 | Supply Intelligence | Adjust supply parameters. |
| Classify Demand | BN-D-019 | Scenario Intelligence | Inform scenario assumptions. |
| Prioritize Demand | BN-D-020 Planning Priority Changed | Demand Planners | Reorder worklist by priority. |
| Prioritize Demand | BN-D-020 | Detect Demand Exceptions | Prioritize exception alerts. |
| Prioritize Demand | BN-D-020 | Forecast Demand | Apply high-priority protection rules. |
| Prioritize Demand | BN-D-020 | Inventory Planning (external) | Prioritize allocation decisions. |
| Prioritize Demand | BN-D-020 | Scenario Intelligence | Assess impact on high-priority items. |
| Evaluate Demand Quality | BN-D-021 Forecast Quality Assessment Published | Learn From Demand | Trigger model improvement analysis. |
| Evaluate Demand Quality | BN-D-021 | Explain Demand | Provide performance context for explanations. |
| Evaluate Demand Quality | BN-D-021 | Forecast Demand | Champion model performance feedback. |
| Evaluate Demand Quality | BN-D-021 | Demand Planners and Managers | Performance dashboards. |
| Detect Demand Exceptions | BN-D-022 Demand Exception Detection Evidence | Core Exception Management | Create SE-C-019 Exception instance. |
| Detect Demand Exceptions | BN-D-022 | Explain Demand | Provide context for explanations. |
| Detect Demand Exceptions | BN-D-022 | Learn From Demand | Pattern learning for proactive detection. |
| Detect Demand Exceptions | BN-D-023 Demand Exception Resolution Evidence | Core Exception Management | Resolve SE-C-019 Exception instance. |
| Explain Demand | BN-D-024 Demand Explanation Established | Learn From Demand | Analyze explanation quality and completeness. |
| Explain Demand | BN-D-024 | Planners and Auditors | Understand why conclusions were reached. |
| Explain Demand | BN-D-024 | AI Copilot | Render Structured Reasoning into natural language. |
| Learn From Demand | BN-D-025 Demand Learning Established | Planning Governance | Refine policies, parameters, decision rules, and planning models. |
| Learn From Demand | BN-D-025 | Forecast Demand | Adjust forecasting strategy and model configurations. |
| Learn From Demand | BN-D-025 | Segment Demand | Refine segmentation rules. |
| Learn From Demand | BN-D-025 | Classify Demand | Refine behavior classification policies. |
| Learn From Demand | BN-D-025 | Prioritize Demand | Adjust priority scoring parameters. |
| Learn From Demand | BN-D-025 | Future Planning Cycles | Continuous improvement of demand planning behavior. |
| Model Demand Interventions | BN-D-026 Demand Intervention Impact Published | Forecast Demand | Consume intervention impact as forward-looking context for forecast generation. |
| Model Demand Interventions | BN-D-026 Demand Intervention Impact Published | Supply Intelligence | Consume intervention impact for supply planning context. |
| Model Demand Interventions | BN-D-026 Demand Intervention Impact Published | Scenario Intelligence | Consume intervention impact for scenario evaluation. |
| Understand Demand | BN-D-001 Demand Understanding Published | Model Demand Interventions | Refresh baseline context for pending impact assessments. |

*Note: BN-P-001 (Promise Confirmed) is a future integration point with Promise Intelligence. It is listed as consumed in the Understand Demand capability until Promise Intelligence is specified.*

---

# Appendix B — Enterprise Capability Matrix

| # | Capability | Business Algorithms | Decisions | Enterprise Question |
|---|---|---|---|---|
| 1 | **Understand Demand** | BA-D-001 | DE-D-001, DE-D-002 | What demand has the enterprise observed, and what does the enterprise currently understand about demand? |
| 2 | **Forecast Demand** | BA-D-002, BA-D-015 | DE-D-003, DE-D-004, DE-D-005, DE-D-013 | What future demand does the enterprise project, with what confidence, under what assumptions? |
| 3 | **Sense Demand** | BA-D-003, BA-D-004, BA-D-014 | DE-D-006, DE-D-007 | What has changed in demand behavior that the enterprise now understands to be true? |
| 4 | **Segment Demand** | BA-D-005 | DE-D-008 | How should the enterprise segment demand entities to enable differentiated planning strategies? |
| 5 | **Classify Demand** | BA-D-006 | DE-D-009 | What behavior does this demand exhibit, and what does that behavior mean for forecasting model selection? |
| 6 | **Prioritize Demand** | BA-D-007 | DE-D-010 | Which demand entities are most important to the enterprise’s objectives, and why? |
| 7 | **Evaluate Demand Quality** | BA-D-008, BA-D-009 | DE-D-011 | How accurate, stable, and valuable is the enterprise’s demand forecasting capability? |
| 8 | **Detect Demand Exceptions** | BA-D-010, BA-D-011 | DE-D-012 | What demand exception evidence requires publication to Core Exception Management, given the current Demand Understanding, Forecast Quality Assessment, and Demand Behavior patterns? |
| 9 | **Explain Demand** | BA-D-012 | *(none – deterministic)* | Why did the enterprise reach, or deliberately not reach, this demand conclusion, and what evidence supports it? |
| 10 | **Learn From Demand** | BA-D-013 | *(none – governance-driven)* | What has the enterprise learned about demand behavior and forecasting performance that should improve future planning? |
| 11 | **Model Demand Interventions** | BA-D-016 | DE-D-014 | What is the expected demand change from this planned commercial intervention? |

---

# Appendix C — Demand Intelligence Pipeline

The following pipeline captures the causal reasoning flow of the Demand Intelligence domain. It is a domain-level architectural pattern, not an ARS mandate. Feedback loops—where assessments and learnings influence future cycles—are described in the Capability Model.

```
Demand Observation
        │
        ▼
Accepted Demand Observation
        │
        ▼
Enterprise Picture (Core)
        │
        ▼
Demand Understanding
        │
        ▼
Demand Sensing
        │
        ▼
Demand Projection (Forecast)
        │
        ▼
Demand Segmentation
        │
        ▼
Demand Behavior Classification
        │
        ▼
Demand Prioritisation
        │
        ▼
Demand Quality Assessment
        │
        ▼
Demand Exception Assessment
        │
        ▼
Demand Explanation
        │
        ▼
Demand Learning
```

**Feedback Loops:**
- Demand Behavior (Sense) feeds back into Forecast Demand for out-of-cycle refresh.
- Demand Quality Assessment feeds into Learn From Demand for model and policy improvement.
- Demand Exception patterns feed into Learn From Demand for detection policy refinement.
- Demand Learnings feed back into Planning Governance and all upstream capabilities in subsequent planning cycles.

---

# Appendix D — Canonical Assessment Structure

Every Assessment-stage Business Algorithm in the Demand domain produces a structured, multi-dimensional assessment. The structure is consistent across BA-D-003 (Assess Demand Signal Deviation), BA-D-005 (Compute Planning Classification), BA-D-007 (Compute Planning Priority Score), BA-D-008 (Compute Forecast Quality Metrics), and BA-D-010 (Evaluate Demand Planning Condition Evidence).

**Assessment-Level Metadata:**
- Assessment Completeness (Complete, Partial, Unavailable)
- Overall Assessment Confidence (High, Medium, Low)

**Per-Dimension Structure:**

| Component | Description |
|-----------|-------------|
| **Evidence** | The specific enterprise facts used for the evaluation. |
| **Interpretation** | What the evidence means for enterprise objectives, in business language. |
| **Exposure / Opportunity** | The nature and magnitude of the threat or opportunity. Quantified where possible. |
| **Confidence** | The enterprise’s confidence in this dimension’s assessment (High, Medium, Low). |
| **Reason Codes** | Governed, stable identifiers (e.g., `CriticalDeviation`, `HighCorroboration`, `VolumeThresholdMet`). Defined by the relevant policy (PO-D-031, PO-D-035, PO-D-037, PO-D-039, PO-D-041, PO-D-044). |
| **Affected Enterprise Objectives** | The business objectives impacted by this assessment (e.g., Delivery Reliability, Forecast Accuracy, Planning Stability). |

This structure is the Demand domain’s proven shape of an enterprise assessment. It is provided as a reference for future domain authors.

All reason codes are governed entries in SE-C-037 Enterprise Governed Vocabulary.

**Cross-Domain Contract Verification**

| Contract | Demand Intelligence Reference | Core Intelligence Reference | Status |
|----------|-------------------------------|----------------------------|--------|
| BN-C-001 Enterprise Picture Published | Consumed by CA-D-001 (Understand Demand) via FS-D-003 | Published by CA-C-019 (Enterprise Picture Management) via FS-C-002 | ✓ Aligned |
| BN-D-022 Demand Exception Detection Evidence | Published by CA-D-008 (Detect Demand Exceptions) via FS-D-015 | Consumed by CA-C-020 (Core Exception Management) via FS-C-003 | ✓ Aligned |
| BN-D-023 Demand Exception Resolution Evidence | Published by CA-D-008 (Detect Demand Exceptions) via FS-D-015 | Consumed by CA-C-020 (Core Exception Management) via FS-C-004 | ✓ Aligned |
| SE-C-019 Exception lifecycle | Not mutated by Demand Intelligence | Owned by CA-C-020 (Core Exception Management) | ✓ Aligned |
| SE-C-021 Enterprise Picture lifecycle | Consumed by Demand Intelligence (read-only) | Owned by CA-C-019 (Enterprise Picture Management) | ✓ Aligned |
| SE-D-018 Demand Intervention Impact lifecycle | Owned by CA-D-011 (Model Demand Interventions) | Consumed by CA-D-002 (Forecast Demand) via FS-D-006 | ✓ Aligned |

---

# Appendix E — Capability Classification Archetypes

The ten Demand Intelligence capabilities fall into four distinct archetypes. These archetypes are domain observations, not an ARS taxonomy.

| Archetype | Capabilities | Distinguishing Behavior |
|-----------|-------------|--------------------------|
| **Intelligence** | Understand Demand, Sense Demand, Segment Demand, Classify Demand, Prioritize Demand, Evaluate Demand Quality, Detect Demand Exceptions, Explain Demand, Learn From Demand | Create enterprise knowledge. They interpret facts, produce assessments, detect conditions, classify behavior, and derive learnings. |
| **Planning** | Forecast Demand | Creates enterprise intent. It synthesises a demand projection from current understanding and governed strategies. |
| **Collaboration** | *(none in Demand)* | Demand Intelligence does not directly collaborate with external parties. Collaboration with suppliers is owned by Supply Intelligence. |
| **Feedback** | Evaluate Demand Quality, Learn From Demand | Produce assessments consumed by upstream capabilities in **subsequent** planning cycles. They explicitly declare their Feedback Target. Sense Demand also provides operational feedback within the same cycle for out-of-cycle forecasting. |
