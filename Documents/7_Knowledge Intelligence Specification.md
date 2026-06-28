# Knowledge Intelligence Specification  

# Chapter 1 — Purpose & Scope  

## 1.1 Purpose  

Knowledge Intelligence is the authoritative enterprise meta‑domain responsible for discovering, governing, and institutionalising learning that spans the entire Medhavi APS. It answers the question: *“What does the enterprise know, across all its planning domains, and how can that knowledge make every domain smarter?”*  

Where Demand Intelligence understands customers, Supply Intelligence plans fulfillment, Promise Intelligence commits to orders, and Scenario Intelligence explores futures — Knowledge Intelligence sits above them all. It observes the outcomes of every domain, discovers patterns that no single domain can see alone, traces systemic problems to their root causes, and curates the enterprise memory that turns isolated experiences into reusable wisdom.  

It is the feedback loop that closes the loop. Without it, each domain learns in isolation. With it, a forecast error in Demand teaches Supply how to buffer better, a promise breach in Promise teaches Scenario what to stress‑test next, and a successful mitigation in Supply becomes an institutionalised best practice that Promise and Scenario can both adopt.  

This specification defines every business objective, performance indicator, semantic concept, capability, decision, rule, policy, functional behaviour, interface, report, and dashboard that constitutes the Knowledge Intelligence domain. It is the single source of enterprise truth for cross‑domain learning.  

## 1.2 Scope  

**Knowledge Intelligence includes:**  

- Cross‑domain pattern discovery: detecting correlations between outcomes in different domains (e.g., forecast bias in Demand causing promise breaches in Promise, supply plan instability causing inventory excess in Supply)  
- Systemic root‑cause analysis: tracing problems that span multiple domains to their originating source  
- Enterprise knowledge graph governance: ensuring semantic concepts, identifiers, and relationships remain consistent, current, and complete across all Intelligence Specifications  
- Improvement portfolio management: tracking, prioritising, and evaluating enterprise‑wide improvement initiatives that affect multiple domains  
- Best‑practice institutionalisation: capturing successful strategies from one part of the enterprise and making them available to all others  
- Feedback‑loop orchestration: ensuring that the Learn capabilities in each domain are connected to each other and to the enterprise‑level learning cycle  
- Enterprise memory: maintaining a queryable, traceable record of what was decided, what happened, and what was learned, for both human decision‑makers and AI agents  
- Knowledge serving: providing the knowledge graph and enterprise memory as a runtime resource for AI agents conducting planning, promising, or scenario analysis  
- Knowledge quality evaluation: measuring the accuracy, completeness, and value of cross‑domain insights  
- Knowledge explainability: generating traceable explanations for every cross‑domain insight and improvement recommendation  

**Knowledge Intelligence excludes:**  

- Single‑domain learning (belongs to the Learn capability within each operational domain)  
- Operational plan execution (belongs to Demand, Supply, Promise)  
- Scenario simulation execution (belongs to Scenario Intelligence)  
- Real‑time order promising (belongs to Promise Intelligence)  

## 1.3 Traceability  

| Reference | Principle |  
|-----------|-----------|  
| C‑EP‑001 | Enterprise First |  
| C‑AI‑001 | AI Ready by Design |  
| C‑TR‑001 | End‑to‑End Traceability |  
| C‑EX‑001 | Explainability |  
| C‑CO‑001 | Architectural Consistency |  
| ARS‑SM‑001 | Semantic Consistency |  
| ARS‑CP‑001 | Capability Consistency |  
| ARS‑DM‑001 | Decision Consistency |  
| ARS‑RP‑001 | Rule & Policy Consistency |  

---

# Chapter 2 — Business Objectives  

## BO‑KN‑001 — Deliver Trusted Cross‑Domain Intelligence  

**Business Motivation**  

Single‑domain intelligence answers “What do we know about demand?” or “What do we know about supply?” But the most valuable enterprise insights come from connecting the domains: “Why does demand forecast bias cause promise breaches?” or “Where does supply plan instability originate?” Knowledge Intelligence shall deliver trusted, evidence‑based answers to these cross‑domain questions, becoming the authoritative source of enterprise‑wide insight.  

**Business Questions**  

- What patterns exist across our planning domains that no single domain can see?  
- How confident are we in the cross‑domain insights we produce?  
- Are our insights traceable to the underlying evidence?  
- Which insights are being acted upon, and which are being ignored?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑001 | Knowledge Intelligence Effectiveness (Reserved) |  
| PI‑KN‑002 | Cross‑Domain Pattern Discovery Rate |  
| PI‑KN‑008 | Enterprise Memory Completeness |  
| PI‑KN‑105 | Explainability Score (Knowledge) |  

---

## BO‑KN‑002 — Discover Systemic Patterns and Root Causes  

**Business Motivation**  

Problems rarely stay in one domain. A supplier delay in Supply becomes a promise breach in Promise. A forecast error in Demand creates excess inventory in Supply. Knowledge Intelligence shall proactively discover these systemic patterns and trace them to their root causes, so the enterprise fixes the source, not just the symptoms.  

**Business Questions**  

- What recurring patterns of failure or success span multiple domains?  
- What are the root causes of systemic problems, not just their local manifestations?  
- How long does it take to identify a systemic pattern after it first appears?  
- How often do our root‑cause analyses lead to effective corrective action?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑003 | Root‑Cause Identification Accuracy |  
| PI‑KN‑107 | Cross‑Domain Correlation Strength |  
| PI‑KN‑108 | Causal Chain Confidence |  

---

## BO‑KN‑003 — Govern the Enterprise Knowledge Graph  

**Business Motivation**  

Every Intelligence Specification defines semantic concepts, but concepts must be consistent across domains. A “promise” in Promise must mean the same thing that “promise adherence” measures in Supply. A “forecast” in Demand must be the same entity that Scenario uses for stress testing. Knowledge Intelligence shall govern the enterprise knowledge graph, ensuring semantic consistency, identifier uniqueness, and relationship integrity across all domains.  

**Business Questions**  

- Are all semantic concepts consistently defined and used across domains?  
- Are there duplicate or conflicting definitions of the same concept?  
- Are all identifiers unique and following the ARS standard?  
- Is the knowledge graph complete enough to support AI reasoning?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑006 | Knowledge Graph Consistency Score |  
| PI‑KN‑103 | Knowledge Graph Coverage |  
| PI‑KN‑110 | Knowledge Freshness Index |  

---

## BO‑KN‑004 — Orchestrate Enterprise‑Wide Improvement  

**Business Motivation**  

When one domain learns something valuable—a better forecasting method, a more robust inventory policy, a faster promising algorithm—that knowledge should benefit every domain that can use it. Knowledge Intelligence shall manage the enterprise improvement portfolio, prioritising initiatives that deliver the greatest cross‑domain impact, tracking their implementation, and verifying their outcomes.  

**Business Questions**  

- What improvements are currently in progress across all domains?  
- Which improvements are expected to deliver cross‑domain benefits?  
- Are improvements delivering their expected outcomes?  
- Where are the greatest opportunities for enterprise‑wide improvement?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑004 | Improvement Portfolio ROI |  
| PI‑KN‑109 | Improvement Adoption Rate |  
| PI‑KN‑012 | Feedback Loop Closure Rate |  

---

## BO‑KN‑005 — Institutionalise Best Practices  

**Business Motivation**  

A successful strategy in one part of the enterprise should not remain tribal knowledge. If a particular inventory policy configuration consistently outperforms in volatile demand environments, that should become an institutionalised best practice that every planner and every AI agent can access. Knowledge Intelligence shall capture, validate, and disseminate best practices across the enterprise.  

**Business Questions**  

- What proven strategies exist that are not yet widely adopted?  
- How quickly can a successful local practice become an enterprise standard?  
- Are institutionalised practices actually being followed?  
- Which best practices need revision based on recent evidence?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑005 | Best‑Practice Institutionalisation Rate |  
| PI‑KN‑109 | Improvement Adoption Rate |  

---

## BO‑KN‑006 — Accelerate Cross‑Domain Learning  

**Business Motivation**  

The speed at which the enterprise learns determines its competitive advantage. Knowledge Intelligence shall reduce the time from “something happened” to “we understand why” to “we’ve improved because of it.” It shall ensure that learning cycles in individual domains are connected, so a lesson learned in Supply propagates to Promise and Scenario without delay.  

**Business Questions**  

- How long does it take from an event occurring to a cross‑domain insight being generated?  
- How long from insight to action?  
- How long from action to verified improvement?  
- Is the overall learning cycle accelerating over time?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑007 | Cross‑Domain Learning Cycle Time |  
| PI‑KN‑106 | Learning Effectiveness Index (Enterprise) |  
| PI‑KN‑012 | Feedback Loop Closure Rate |  

---

## BO‑KN‑007 — Maintain Enterprise Memory  

**Business Motivation**  

Enterprises forget. Planners move on. Decisions made under pressure are not recorded. The rationale behind a successful strategy fades. Knowledge Intelligence shall maintain the enterprise memory: an immutable, queryable record of what was decided, what happened, what was learned, and why. This memory serves both human decision‑makers seeking context and AI agents seeking precedent.  

**Business Questions**  

- What was the state of the enterprise when a particular decision was made?  
- What were the outcomes of similar decisions in the past?  
- What was the rationale behind a now‑standard practice?  
- Is the enterprise memory complete enough to prevent repeated mistakes?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑008 | Enterprise Memory Completeness |  
| PI‑KN‑013 | Knowledge Serving Latency |  
| PI‑KN‑110 | Knowledge Freshness Index |  

---

## BO‑KN‑008 — Continuously Improve Knowledge Intelligence  

**Business Motivation**  

Knowledge Intelligence itself must learn. It must evaluate the accuracy of its pattern discoveries, the effectiveness of its improvement recommendations, the completeness of its knowledge graph, and the value of its enterprise memory. This meta‑learning ensures that the enterprise’s capacity for cross‑domain intelligence continuously improves.  

**Business Questions**  

- Are our cross‑domain insights becoming more accurate over time?  
- Are our improvement recommendations delivering expected ROI?  
- Is the knowledge graph becoming more complete and consistent?  
- How can Knowledge Intelligence itself improve?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑KN‑106 | Learning Effectiveness Index (Enterprise) |  
| PI‑KN‑104 | Recommendation Quality Index (Knowledge) |  
| PI‑KN‑105 | Explainability Score (Knowledge) |  

---

# Chapter 3 — Enterprise Measurement Model  

## 3.1 Measurement Architecture  

The Enterprise Measurement Model defines every performance indicator used to evaluate Knowledge Intelligence. Each indicator is a first‑class enterprise object with a unique identifier, complete definition, formula, interpretation, worked example, limitations, and relationships.  

**Three measurement tiers:**  

| Range | Tier | Purpose |
|-------|------|---------|
| PI‑KN‑001 – PI‑KN‑049 | Business Outcome Measures | Measure business value delivered |
| PI‑KN‑050 – PI‑KN‑099 | Reserved | Future expansion |
| PI‑KN‑100 – PI‑KN‑199 | Intelligence Measures | Measure intelligence quality |
| PI‑KN‑200 – PI‑KN‑299 | Operational Measures | Measure system performance |

**PI‑KN‑001** is reserved for a future composite index—Knowledge Intelligence Effectiveness—to be derived after all underlying measures are defined.  

---

## 3.2 Business Outcome Measures  

### PI‑KN‑001 — Knowledge Intelligence Effectiveness [RESERVED]  

This identifier is reserved for a future composite indicator that will aggregate Business Outcome Measures, Intelligence Measures, and Operational Measures into a single executive health score for the Knowledge Intelligence domain. It cannot be defined until all underlying measures exist and their interactions are understood.  

---

### PI‑KN‑002 — Cross‑Domain Pattern Discovery Rate  

**Definition**  

Cross‑Domain Pattern Discovery Rate measures the number of statistically significant cross‑domain patterns discovered by Knowledge Intelligence over a defined period. A pattern is considered “discovered” when it is formally identified, validated against evidence, and published to the enterprise knowledge graph with sufficient confidence.  

This metric reflects the productivity of the enterprise’s systemic learning capability. Higher rates indicate that Knowledge Intelligence is actively finding connections across domains that would otherwise remain hidden.  

**Business Objectives**  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  
- BO‑KN‑002 Discover Systemic Patterns and Root Causes  

**Business Interpretation**  

| Value (patterns per quarter) | Interpretation |
|------------------------------|----------------|
| > 10 | Excellent — highly productive cross‑domain learning |
| 5 – 10 | Good — active discovery of systemic patterns |
| 2 – 5 | Acceptable — steady but limited discovery |
| < 2 | Investigation required — enterprise learning may be stagnant |

Thresholds are enterprise‑specific and configurable. Quality matters more than quantity: patterns with low confidence or limited business impact should not inflate this metric.  

**Formula**  

Cross‑Domain Pattern Discovery Rate = Count of Validated Patterns Published in Period  

Where a Validated Pattern satisfies all of:  
- Involves artifacts from at least two different Intelligence Domains (Demand, Supply, Promise, Scenario)  
- Has a confidence score ≥ threshold (default 80%)  
- Has been reviewed and accepted by at least one domain stakeholder  
- Is recorded in the enterprise knowledge graph with full traceability  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Validated Pattern | Entity | A cross‑domain pattern meeting all validation criteria |
| Confidence Score | Percentage (0–100%) | Statistical or evidential confidence in the pattern |
| Period | Duration | Evaluation period (typically quarterly) |

**Preconditions**  

- Knowledge Intelligence is actively ingesting outcomes from at least two operational domains  
- Pattern validation criteria and confidence thresholds are defined  
- A review process for pattern acceptance is operational  

**Assumptions**  

- The metric counts only patterns that have been formally validated; raw correlations without review are not counted  
- Patterns that are later invalidated are subtracted from the period in which they were invalidated, not the original discovery period  
- A pattern spanning three domains counts as one pattern, not multiple  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Pattern discovery log, Validation records, Knowledge graph entries |
| Unit | Count per period |
| Precision | Integer |
| Aggregation Levels | Enterprise |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target >10, Warning 5–10, Critical <5 (configurable) |
| Business Owner | Knowledge Manager / Chief Data Officer |
| Business Consumers | Supply Chain Director, Executive Management, Domain Managers |
| System Consumers | Dashboards, Reports |
| Derived From | Pattern discovery and validation records |
| Related PIs | PI‑KN‑003 Root‑Cause Identification Accuracy, PI‑KN‑107 Cross‑Domain Correlation Strength |

**Worked Example**  

**Q3 2026 Pattern Discovery Log:**  

| Pattern ID | Domains Involved | Confidence | Validated? |
|------------|------------------|------------|-------------|
| P‑001 | Demand + Supply | 92% | Yes |
| P‑002 | Supply + Promise | 88% | Yes |
| P‑003 | Demand + Promise + Scenario | 95% | Yes |
| P‑004 | Demand + Supply | 65% | No (below confidence threshold) |
| P‑005 | Supply + Promise | 91% | Pending review |
| P‑006 | Demand + Supply | 85% | Yes |

Validated Patterns Published = 4 (P‑001, P‑002, P‑003, P‑006)  

Cross‑Domain Pattern Discovery Rate = **4 patterns per quarter**  

Business Interpretation: **Acceptable** — steady but limited discovery.  

**Limitations**  

- Count does not reflect business impact; a single high‑impact pattern may be worth more than dozens of minor correlations  
- Metric is sensitive to the confidence threshold setting  
- Patterns discovered near the end of a period may not be validated until the next period, creating timing effects  

**Relationships**  

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑KN‑001, BO‑KN‑002 |
| Complemented By | PI‑KN‑003 Root‑Cause Identification Accuracy |
| Displayed In | Knowledge Intelligence Dashboard |
| Used By | Enterprise Learning Review, S&OP |

---

### PI‑KN‑003 — Root‑Cause Identification Accuracy  

**Definition**  

Root‑Cause Identification Accuracy measures the percentage of systemic problems for which Knowledge Intelligence correctly identifies the true root cause. A root cause is considered correctly identified if, when the recommended corrective action is applied to that cause, the systemic problem is resolved or significantly reduced.  

This metric reflects the diagnostic precision of the enterprise’s cross‑domain analysis.  

**Business Objectives**  

- BO‑KN‑002 Discover Systemic Patterns and Root Causes  

**Business Interpretation**  

| Value | Interpretation |
|-------|----------------|
| 90% – 100% | Excellent diagnostic accuracy |
| 75% – 90% | Good accuracy |
| 50% – 75% | Acceptable — some misdiagnosis |
| Below 50% | Investigation required — systemic analysis needs improvement |

**Formula**  

Root‑Cause Identification Accuracy (%) = ( Number of Root Causes Confirmed Correct ÷ Total Number of Root‑Cause Analyses Completed with Outcome Data ) × 100  

Where Confirmed Correct means the corrective action applied to the identified root cause resulted in a measurable reduction of the systemic problem (≥50% reduction in the target metric).  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Root Cause Confirmed Correct | Integer | Count of analyses where the identified cause was validated by successful correction |
| Total Root‑Cause Analyses with Outcome Data | Integer | All completed analyses where sufficient time has passed to observe outcomes |

**Preconditions**  

- Root‑cause analyses must be documented with the identified cause and recommended corrective action  
- Outcome data must be available for a sufficient observation period after corrective action is applied (minimum one planning cycle)  

**Assumptions**  

- Successful correction is defined as ≥50% reduction in the problem metric within two planning cycles  
- If multiple root causes are identified for a single problem, the analysis is counted as correct if at least the primary root cause is validated  
- Analyses where the corrective action was not implemented are excluded (cannot evaluate accuracy of unimplemented recommendations)  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Root‑cause analysis records, Outcome data post‑correction |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Enterprise |
| Frequency | Quarterly, Annually |
| Performance Targets | Target ≥90%, Warning 75–90%, Critical <75% (configurable) |
| Business Owner | Knowledge Manager |
| Business Consumers | Domain Managers, Supply Chain Director |
| System Consumers | Dashboards, Continuous Improvement Reports |
| Derived From | Root‑cause analysis and outcome records |
| Related PIs | PI‑KN‑002 Cross‑Domain Pattern Discovery Rate, PI‑KN‑108 Causal Chain Confidence |

**Worked Example**  

**Q3 2026 Root‑Cause Analyses:**  

| Analysis ID | Problem | Identified Root Cause | Corrective Action Applied | Problem Resolved? |
|-------------|---------|----------------------|---------------------------|-------------------|
| RCA‑001 | Promise breaches increasing | Demand forecast bias in top 10 SKUs | Forecast model retuned | Yes (breaches dropped 65%) |
| RCA‑002 | Excess inventory in DC‑A | Supply plan not consuming actual demand signals | Plan input refresh shortened | Yes (excess reduced 55%) |
| RCA‑003 | Scenario accuracy degrading | Calibration drift in simulation engine | Engine recalibrated | Partially (accuracy improved 40%, below threshold) |
| RCA‑004 | Late deliveries from Supplier S3 | Incorrect lead time in master data | Lead time corrected | Yes (OTD improved from 72% to 94%) |

Confirmed Correct = 3 (RCA‑001, RCA‑002, RCA‑004). RCA‑003 did not achieve ≥50% reduction.  

Total Analyses with Outcome Data = 4  

Root‑Cause Identification Accuracy = (3 ÷ 4) × 100 = **75.0%**  

Business Interpretation: **Good accuracy** — at the boundary between Good and Acceptable.  

**Limitations**  

- Requires sufficient time to observe outcomes; fast‑cycle analyses may not yet have outcome data  
- The 50% reduction threshold may not be appropriate for all problem types; configurable per problem class  
- Some problems have multiple interacting causes; partial attribution is difficult  

**Relationships**  

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑KN‑002 |
| Complemented By | PI‑KN‑108 Causal Chain Confidence |
| Displayed In | Knowledge Quality Dashboard |
| Used By | Continuous Improvement, Domain Reviews |

---

### PI‑KN‑004 — Improvement Portfolio ROI  

**Definition**  

Improvement Portfolio ROI measures the total estimated financial return generated by the enterprise‑wide improvement portfolio managed by Knowledge Intelligence, relative to the cost of implementing those improvements. It reflects the economic value of systemic learning.  

**Business Objectives**  

- BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement  

**Business Interpretation**  

| Value (ROI ratio) | Interpretation |
|-------------------|----------------|
| > 3.0 | Exceptional return on improvement investment |
| 2.0 – 3.0 | Excellent return |
| 1.0 – 2.0 | Positive return |
| < 1.0 | Negative return — improvement costs exceed benefits |

**Formula**  

Improvement Portfolio ROI = ( Σ Estimated Annual Benefit of All Implemented Improvements − Σ Total Cost of Implementation ) ÷ Σ Total Cost of Implementation  

Where:  
- Estimated Annual Benefit is the projected financial benefit (cost savings, revenue increase, risk reduction) annualised  
- Total Cost of Implementation includes all costs (personnel, technology, process change) incurred to implement the improvement  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Estimated Annual Benefit | Currency | Annualised financial value of the improvement outcome |
| Total Cost of Implementation | Currency | All costs to implement the improvement |

**Preconditions**  

- Each improvement in the portfolio must have an approved business case with estimated benefits and costs  
- Actual costs must be tracked  
- Benefits must be estimated at the time of implementation and validated after a sufficient observation period  

**Assumptions**  

- Benefits are annualised for comparability across improvements with different time horizons  
- Benefits that are difficult to quantify financially (e.g., improved customer satisfaction) are estimated using agreed‑upon proxies  
- Costs are fully loaded (direct and indirect)  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Improvement portfolio records, Financial tracking data |
| Unit | Ratio (dimensionless) |
| Precision | One decimal place |
| Aggregation Levels | Enterprise |
| Frequency | Quarterly, Annually |
| Performance Targets | Target >3.0, Warning 1.5–3.0, Critical <1.0 (configurable) |
| Business Owner | Knowledge Manager / Finance |
| Business Consumers | CFO, Supply Chain Director, Executive Management |
| System Consumers | Executive Dashboards |
| Derived From | Improvement portfolio and financial records |
| Related PIs | PI‑KN‑109 Improvement Adoption Rate, PI‑KN‑012 Feedback Loop Closure Rate |

**Worked Example**  

**Q3 2026 Improvement Portfolio:**  

| Improvement ID | Annual Benefit ($) | Implementation Cost ($) |
|----------------|-------------------|-------------------------|
| IMP‑001 | 450,000 | 120,000 |
| IMP‑002 | 280,000 | 95,000 |
| IMP‑003 | 620,000 | 310,000 |
| IMP‑004 | 150,000 | 60,000 |

Σ Annual Benefit = 450,000 + 280,000 + 620,000 + 150,000 = 1,500,000  

Σ Cost = 120,000 + 95,000 + 310,000 + 60,000 = 585,000  

Net Benefit = 1,500,000 − 585,000 = 915,000  

Improvement Portfolio ROI = 915,000 ÷ 585,000 = **1.56**  

Business Interpretation: **Positive return** — improvements are generating value above their cost.  

**Limitations**  

- Early‑stage improvements may not yet have validated benefit data; estimated benefits carry uncertainty  
- ROI does not capture strategic, non‑financial benefits  
- Long‑term improvements may show low short‑term ROI despite high lifetime value  

**Relationships**  

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑KN‑004 |
| Complemented By | PI‑KN‑109 Improvement Adoption Rate |
| Displayed In | Executive Dashboards, Improvement Portfolio Dashboard |
| Used By | Investment Prioritisation, Budgeting |

---

### PI‑KN‑005 — Best‑Practice Institutionalisation Rate  

**Definition**  

Best‑Practice Institutionalisation Rate measures the percentage of validated best practices that have been formally adopted as enterprise standards across all applicable domains. It reflects how effectively Knowledge Intelligence turns isolated successes into widespread improvements.  

**Business Objectives**  

- BO‑KN‑005 Institutionalise Best Practices  

**Business Interpretation**  

| Value | Interpretation |
|-------|----------------|
| 80% – 100% | Excellent — best practices spread rapidly |
| 60% – 80% | Good — most practices adopted |
| 40% – 60% | Acceptable — significant practices remain local |
| Below 40% | Investigation required — institutionalisation process is failing |

**Formula**  

Best‑Practice Institutionalisation Rate (%) = ( Number of Best Practices Adopted as Enterprise Standards ÷ Total Number of Validated Best Practices ) × 100  

Where Adopted as Enterprise Standards means the practice is documented in the enterprise knowledge graph, published to all applicable domains, and referenced in domain‑level rules or policies.  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Validated Best Practice | Entity | A proven strategy, method, or configuration validated by outcomes |
| Adopted as Enterprise Standard | Boolean | True if the practice is formally adopted across all applicable domains |

**Preconditions**  

- A best‑practice catalogue exists with validation criteria  
- Domain governance processes allow for adoption of enterprise standards  

**Assumptions**  

- A practice is only counted as institutionalised when all applicable domains have adopted it  
- Practices that are superseded by newer practices are excluded from the count  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Best‑practice catalogue, Adoption records per domain |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Frequency | Quarterly |
| Performance Targets | Target ≥80%, Warning 60–80%, Critical <60% (configurable) |
| Business Owner | Knowledge Manager |
| Related PIs | PI‑KN‑109 Improvement Adoption Rate |

**Worked Example**  

| Best Practice ID | Validated | Applicable Domains | Adopted In All? |
|------------------|-----------|-------------------|-----------------|
| BP‑001 | Yes | Demand, Supply | Yes |
| BP‑002 | Yes | Supply, Promise, Scenario | No (Scenario pending) |
| BP‑003 | Yes | Demand | Yes |
| BP‑004 | Yes | Supply, Promise | Yes |

Validated Best Practices = 4  

Adopted as Enterprise Standards = 3 (BP‑001, BP‑003, BP‑004)  

Institutionalisation Rate = (3 ÷ 4) × 100 = **75.0%**  

Business Interpretation: **Good** — most practices adopted, one pending.  

---

### PI‑KN‑006 — Knowledge Graph Consistency Score  

**Definition**  

Knowledge Graph Consistency Score measures the degree to which the enterprise knowledge graph is internally consistent—free of contradictory definitions, duplicate identifiers, circular references, and semantic conflicts across domains.  

**Business Objectives**  

- BO‑KN‑003 Govern the Enterprise Knowledge Graph  

**Business Interpretation**  

| Value | Interpretation |
|-------|----------------|
| 95% – 100% | Excellent consistency |
| 85% – 95% | Good consistency — minor issues |
| 70% – 85% | Acceptable — some conflicts present |
| Below 70% | Investigation required — significant governance failures |

**Formula**  

Consistency Score (%) = ( 1 − ( Number of Consistency Violations ÷ Total Number of Knowledge Graph Nodes and Edges ) ) × 100  

Where Consistency Violations include: duplicate identifiers, conflicting definitions, missing mandatory relationships, circular dependencies, and broken references.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Knowledge graph, Automated consistency checks, Manual review findings |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target ≥95%, Warning 85–95%, Critical <85% (configurable) |
| Business Owner | Knowledge Manager |

**Worked Example**  

Total Nodes and Edges in Knowledge Graph = 2,500  

Consistency Violations Found: 15 (duplicate IDs) + 8 (broken references) + 5 (conflicting definitions) = 28  

Consistency Score = (1 − (28 ÷ 2,500)) × 100 = **98.9%**  

Business Interpretation: **Excellent consistency**.  

---

### PI‑KN‑007 — Cross‑Domain Learning Cycle Time  

**Definition**  

Cross‑Domain Learning Cycle Time measures the average elapsed time from the occurrence of a significant cross‑domain event to the completion of the learning cycle: pattern discovery, root‑cause analysis, improvement recommendation, and feedback loop closure.  

**Business Objectives**  

- BO‑KN‑006 Accelerate Cross‑Domain Learning  

**Formula**  

Learning Cycle Time = Average ( Time(Learning Loop Closed) − Time(Triggering Event Occurred) ) across all closed learning loops in the period.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Event timestamps, Learning loop closure records |
| Unit | Days |
| Precision | One decimal place |
| Frequency | Quarterly |
| Performance Targets | Target <30 days, Warning 30–60 days, Critical >60 days (configurable) |
| Business Owner | Knowledge Manager |

---

### PI‑KN‑008 — Enterprise Memory Completeness  

**Definition**  

Enterprise Memory Completeness measures the percentage of significant enterprise decisions and outcomes that are recorded in the enterprise memory with full traceability.  

**Business Objectives**  

- BO‑KN‑007 Maintain Enterprise Memory  

**Formula**  

Enterprise Memory Completeness (%) = ( Number of Significant Events Recorded with Full Traceability ÷ Total Number of Significant Events ) × 100  

Where Significant Events are defined as: strategic plan adoptions, major promise breaches (affecting top‑tier customers), significant forecast errors (>20% WAPE in a cycle), supply disruptions with impact >$100K, and cross‑domain improvement implementations.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Event logs from all domains, Enterprise memory records |
| Unit | Percentage (%) |
| Frequency | Monthly |
| Performance Targets | Target ≥95% |
| Business Owner | Knowledge Manager |

---

### PI‑KN‑009 — Systemic Risk Reduction  

**Definition**  

Systemic Risk Reduction measures the decrease in enterprise‑level risk exposure attributable to improvements identified and orchestrated by Knowledge Intelligence.  

**Business Objectives**  

- BO‑KN‑002 Discover Systemic Patterns and Root Causes  

**Formula**  

Systemic Risk Reduction (%) = ( (Systemic Risk Score Before − Systemic Risk Score After) ÷ Systemic Risk Score Before ) × 100  

Where Systemic Risk Score is an aggregate of cross‑domain risk factors (e.g., probability of cascading supply‑demand failure).  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Risk assessments from all domains, Improvement outcome data |
| Unit | Percentage (%) |
| Frequency | Annually |
| Business Owner | Risk Committee / Knowledge Manager |

---

### PI‑KN‑010 — Decision Confidence Improvement (Enterprise)  

**Definition**  

Decision Confidence Improvement measures the increase in decision‑maker confidence across all domains attributable to insights and recommendations from Knowledge Intelligence.  

**Business Objectives**  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  

**Formula**  

Confidence Improvement (pp) = Average Confidence (post‑Knowledge insight) − Average Confidence (pre‑Knowledge insight) across all decisions supported by Knowledge Intelligence in the period.  

Measured via decision‑maker survey on a 1–10 scale, normalised to percentage points.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Decision‑maker surveys |
| Unit | Percentage points |
| Frequency | Quarterly |
| Business Owner | Knowledge Manager |

---

### PI‑KN‑011 — Cross‑Domain Plan Consistency Score  

**Definition**  

Cross‑Domain Plan Consistency Score measures the degree of alignment between plans in different domains. A high score indicates that the demand plan, supply plan, and promise strategies are mutually consistent; a low score indicates misalignment that may cause execution failures.  

**Business Objectives**  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  

**Formula**  

Consistency Score (%) = ( 1 − ( Σ |Domain Plan Deviation| ÷ Number of Checkpoints) ) × 100  

Where Domain Plan Deviation is measured at integration points (e.g., demand forecast vs. supply plan quantities for the same product‑location‑period, or supply plan availability vs. promise capacity).  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Plans from Demand, Supply, and Promise |
| Unit | Percentage (%) |
| Frequency | Weekly |
| Performance Targets | Target ≥95% |
| Business Owner | Knowledge Manager / S&OP |

---

### PI‑KN‑012 — Feedback Loop Closure Rate  

**Definition**  

Feedback Loop Closure Rate measures the percentage of learning feedback loops that are completed—from event detection through pattern discovery, root‑cause analysis, improvement recommendation, implementation, and outcome verification.  

**Business Objectives**  

- BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement  
- BO‑KN‑006 Accelerate Cross‑Domain Learning  

**Formula**  

Feedback Loop Closure Rate (%) = ( Number of Closed Learning Loops ÷ Total Number of Opened Learning Loops ) × 100  

Where a Closed Learning Loop has completed all stages including outcome verification.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Learning loop records |
| Unit | Percentage (%) |
| Frequency | Quarterly |
| Performance Targets | Target ≥90% |
| Business Owner | Knowledge Manager |

---

### PI‑KN‑013 — Knowledge Serving Latency  

**Definition**  

Knowledge Serving Latency measures the average response time for a knowledge query from an AI agent or human user to the Knowledge Intelligence system.  

**Business Objectives**  

- BO‑KN‑007 Maintain Enterprise Memory  

**Formula**  

Knowledge Serving Latency = Average ( Time(Response Returned) − Time(Query Received) ) for all queries in the period.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Query logs with timestamps |
| Unit | Milliseconds |
| Frequency | Daily |
| Performance Targets | Target <500 ms for simple queries, <5 seconds for complex pattern queries |
| Business Owner | Knowledge Manager / IT Operations |

---

### PI‑KN‑014 — Planning Cycle Time (Knowledge)  

**Definition**  

Planning Cycle Time measures the total elapsed time for Knowledge Intelligence to complete its own planning cycle: evaluating all domain Learn outputs, discovering patterns, and publishing insights.  

**Formula**  

Planning Cycle Time = Time(Knowledge Cycle Completed) − Time(Knowledge Cycle Initiated)  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Cycle timestamps |
| Unit | Days |
| Frequency | Per cycle |
| Business Owner | Knowledge Manager |

---

### PI‑KN‑015 — Strategic Insight Generation Rate  

**Definition**  

Strategic Insight Generation Rate measures the number of strategic‑level insights (those influencing executive decisions or multi‑year planning) generated by Knowledge Intelligence per year.  

**Business Objectives**  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  

**Formula**  

Strategic Insight Generation Rate = Count of Strategic Insights Published in Period  

Where a Strategic Insight is defined as an insight that influences a decision with enterprise‑wide impact, typically reviewed at the executive level.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Insight publication log |
| Unit | Count per year |
| Frequency | Annually |
| Business Owner | Knowledge Manager |

---

## 3.3 Intelligence Measures (Stubs)  

| PI | Name | Description |
|----|------|-------------|
| PI‑KN‑101 | Enterprise Understanding Index (Cross‑Domain) | Composite of how completely the enterprise understands systemic patterns and risks. Reserved. |
| PI‑KN‑102 | Pattern Significance Score | Average statistical or evidential significance of discovered patterns. Reserved. |
| PI‑KN‑103 | Knowledge Graph Coverage | Percentage of enterprise concepts and relationships represented in the knowledge graph. Reserved. |
| PI‑KN‑104 | Recommendation Quality Index (Knowledge) | Quality of cross‑domain improvement recommendations. Reserved. |
| PI‑KN‑105 | Explainability Score (Knowledge) | Completeness and quality of knowledge insight explanations. Reserved. |
| PI‑KN‑106 | Learning Effectiveness Index (Enterprise) | Aggregate rate of enterprise‑wide learning improvement. Reserved. |
| PI‑KN‑107 | Cross‑Domain Correlation Strength | Average strength of discovered cross‑domain correlations. Reserved. |
| PI‑KN‑108 | Causal Chain Confidence | Average confidence in identified causal chains. Reserved. |
| PI‑KN‑109 | Improvement Adoption Rate | Percentage of recommended cross‑domain improvements adopted. Reserved. |
| PI‑KN‑110 | Knowledge Freshness Index | Currency and timeliness of the enterprise knowledge graph. Reserved. |

---

## 3.4 Operational Measures (Stubs)  

| PI | Name | Description |
|----|------|-------------|
| PI‑KN‑201 | Knowledge Query Response Time | 95th percentile query latency. Reserved. |
| PI‑KN‑202 | Pattern Discovery Computation Time | Time to run cross‑domain pattern detection. Reserved. |
| PI‑KN‑203 | Knowledge Graph Update Latency | Time from domain event to knowledge graph update. Reserved. |
| PI‑KN‑204 | System Availability (Knowledge) | Uptime of knowledge services. Reserved. |
| PI‑KN‑205 | Event Processing Latency (Knowledge) | Time to process learning events from domains. Reserved. |

---

# Chapter 4 — Semantic Foundation  

The following concepts establish the enterprise meaning upon which all Knowledge Intelligence capabilities operate. Each concept is a first‑class enterprise object with a unique identifier and a complete definition. This chapter mirrors the structure of the Demand, Supply, Promise, and Scenario Semantic Foundations, specialized for cross‑domain learning and knowledge governance.  

## 4.1 Core Enterprise Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑001 | Knowledge | A validated, reusable piece of enterprise understanding that emerges from the analysis of outcomes across one or more planning domains. Knowledge may take the form of a discovered pattern, an identified root cause, a best practice, or an improvement recommendation. Knowledge is the fundamental unit of Knowledge Intelligence. |
| SE‑KN‑002 | Learning Event | A significant occurrence in any domain that triggers or contributes to enterprise learning. A learning event may be positive (a successful strategy) or negative (a failure, breach, or error). Every learning event is recorded with its domain of origin, timestamp, and a reference to the artifacts involved. |
| SE‑KN‑003 | Cross‑Domain Pattern | A statistically or evidentially significant correlation, causal relationship, or recurring sequence that spans at least two Intelligence Domains. A pattern is discovered, not created; it exists latently in the enterprise data and is surfaced by Knowledge Intelligence. |
| SE‑KN‑004 | Improvement Portfolio | The managed collection of all active, proposed, completed, and rejected enterprise‑wide improvement initiatives. The portfolio is prioritised by expected cross‑domain impact and tracked from proposal through implementation to verified outcome. |
| SE‑KN‑005 | Enterprise Memory | The immutable, queryable record of significant enterprise decisions, their contexts, their outcomes, and the lessons learned. Enterprise memory serves as the institutional memory that persists beyond individual planners, systems, or organisational changes. |

## 4.2 Knowledge Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑010 | Knowledge Artifact | A discrete, identifiable piece of enterprise knowledge. Every knowledge artifact has a unique identifier, a type (pattern, root cause, best practice, insight), a confidence score, and full traceability to the evidence that supports it. |
| SE‑KN‑011 | Knowledge Confidence | A score (0–100%) expressing the statistical or evidential reliability of a knowledge artifact. High confidence indicates strong evidence; low confidence indicates the artifact is provisional and requires further validation. |
| SE‑KN‑012 | Knowledge Lifecycle | The states through which a knowledge artifact passes: Proposed (submitted for validation), Under Review (being evaluated against evidence), Validated (confirmed by evidence and stakeholder review), Published (available in the enterprise knowledge graph), Superseded (replaced by a more current artifact), Retired (no longer applicable). |
| SE‑KN‑013 | Knowledge Domain | The scope of applicability of a knowledge artifact. A knowledge domain may be a single Intelligence Domain (e.g., Supply), multiple specific domains (Demand + Supply), or Enterprise (all domains). |
| SE‑KN‑014 | Knowledge Evidence | The collection of data points, event records, outcome measurements, and causal analyses that support a knowledge artifact. Every knowledge artifact must have traceable evidence; artifacts without evidence are classified as hypotheses, not knowledge. |
| SE‑KN‑015 | Knowledge Provenance | The complete lineage of a knowledge artifact: which learning events triggered its discovery, which analyses produced it, who validated it, and which evidence supports it. Provenance ensures every piece of enterprise knowledge is auditable and traceable to its source. |

## 4.3 Cross‑Domain Pattern Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑020 | Pattern Type | A classification of cross‑domain patterns by their nature: Correlation (two or more metrics move together across domains), Causation (a change in one domain demonstrably causes a change in another), Sequence (a recurring order of events spanning domains), Anomaly (a rare but significant cross‑domain event cluster). |
| SE‑KN‑021 | Pattern Significance | A measure of the statistical or practical importance of a discovered pattern. Significance considers the strength of the relationship, the volume of evidence, the business impact of the pattern, and whether the pattern is likely to be actionable. |
| SE‑KN‑022 | Pattern Trigger | The condition or threshold that causes Knowledge Intelligence to search for patterns. Triggers may be scheduled (periodic scan), event‑driven (after a significant failure or success in any domain), or user‑initiated (a strategic inquiry). |
| SE‑KN‑023 | Cross‑Domain Correlation | A quantified relationship between metrics in different domains (e.g., correlation coefficient between forecast bias in Demand and promise breach rate in Promise). Correlations are the most common type of cross‑domain pattern and the starting point for causal investigation. |
| SE‑KN‑024 | Causal Chain | A sequence of cause‑and‑effect relationships that links an originating event or condition in one domain to an observed outcome in another domain. A causal chain may span multiple domains and have intermediate effects. Each link in the chain has an assigned confidence. |

## 4.4 Knowledge Graph Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑030 | Knowledge Node | A vertex in the enterprise knowledge graph representing any enterprise object: a semantic concept, a capability, a decision, a rule, a policy, a performance indicator, a learning event, or a knowledge artifact. Every node has a unique ARS‑compliant identifier. |
| SE‑KN‑031 | Knowledge Edge | A directed, typed relationship between two knowledge nodes. Edge types include: DependsOn, Produces, Consumes, Governs, Constrains, Validates, CorrelatesWith, Causes, Supersedes, and References. |
| SE‑KN‑032 | Semantic Consistency Rule | A rule that governs the integrity of the knowledge graph. Consistency rules detect duplicate identifiers, conflicting definitions, missing mandatory relationships, circular dependencies, and broken references. |
| SE‑KN‑033 | Ontology Version | A versioned snapshot of the enterprise knowledge graph schema—the set of node types, edge types, and consistency rules that define the valid structure of enterprise knowledge. Ontology versions enable safe evolution of the knowledge model. |
| SE‑KN‑034 | Knowledge Graph Coverage | The degree to which the knowledge graph represents all defined enterprise concepts, capabilities, decisions, rules, policies, and their relationships. Coverage gaps indicate areas where the enterprise knowledge base is incomplete. |

## 4.5 Improvement Portfolio Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑040 | Improvement Initiative | A proposed or active project to enhance enterprise performance based on knowledge discoveries. An initiative has a defined scope, expected benefits, estimated cost, affected domains, and an owner. |
| SE‑KN‑041 | Improvement Status | The lifecycle state of an improvement initiative: Proposed (submitted for evaluation), Approved (authorized for implementation), In Progress (being executed), Implemented (changes deployed), Verified (outcomes confirmed), Rejected (not pursued), Rolled Back (reversed due to negative impact). |
| SE‑KN‑042 | Improvement ROI | The estimated or actual return on investment for an improvement initiative: (Estimated Annual Benefit − Total Cost) ÷ Total Cost. ROI is estimated at proposal time and validated after implementation. |
| SE‑KN‑043 | Improvement Dependency | A relationship between improvement initiatives where one initiative must be completed before another can begin, or where the success of one initiative depends on the outcome of another. |

## 4.6 Root‑Cause Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑050 | Root Cause | The fundamental originating factor of a systemic problem. A root cause is distinguished from proximate causes (immediate triggers) and contributing factors (conditions that amplify the problem). Addressing the root cause prevents recurrence; addressing only proximate causes does not. |
| SE‑KN‑051 | Contributing Factor | A condition that amplifies or enables a systemic problem but is not its fundamental origin. Multiple contributing factors may interact with a root cause to produce the observed outcome. |
| SE‑KN‑052 | Root‑Cause Confidence | A score (0–100%) expressing the certainty that an identified cause is the true root cause of a systemic problem. Confidence is derived from the strength of the causal evidence, the success of corrective actions, and expert review. |
| SE‑KN‑053 | Root‑Cause Analysis | A structured investigation that traces a systemic problem from its observed symptoms back through causal chains to the fundamental root cause(s). The analysis produces a documented causal chain, identified root causes, contributing factors, and recommended corrective actions. |

## 4.7 Best‑Practice Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑060 | Best Practice | A proven strategy, method, configuration, or policy that has been demonstrated to produce superior outcomes in specific enterprise conditions. A best practice is derived from empirical evidence (outcome data) and validated across multiple instances or domains. |
| SE‑KN‑061 | Practice Provenance | The origin and validation history of a best practice: which domain first demonstrated it, under what conditions, with what outcomes, and how it was subsequently validated in other domains. |
| SE‑KN‑062 | Practice Applicability | The set of conditions under which a best practice is expected to be effective. Applicability may be defined by demand pattern, supply network characteristics, customer segment, product type, or other enterprise context. |
| SE‑KN‑063 | Practice Institutionalisation | The formal adoption of a best practice as an enterprise standard. Institutionalisation includes publishing the practice in the knowledge graph, updating domain‑level rules or policies to reference it, and training planners and AI agents to apply it. |

## 4.8 Feedback Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑070 | Feedback Signal | An observable indicator from any domain that something has changed—an outcome exceeded or fell short of expectations, a pattern emerged, a decision succeeded or failed. Feedback signals are the raw inputs to enterprise learning. |
| SE‑KN‑071 | Feedback Target | The specific domain, capability, decision, rule, or policy that a feedback signal is directed toward. The target determines who should act on the feedback. |
| SE‑KN‑072 | Feedback Loop | The complete cycle from event occurrence through pattern discovery, root‑cause analysis, improvement recommendation, implementation, and outcome verification. A feedback loop is the mechanism by which enterprise learning becomes institutionalised improvement. |
| SE‑KN‑073 | Loop Closure | The formal completion of a feedback loop, marked by the verification that an implemented improvement produced the expected outcome. Loop closure confirms that learning has been successfully converted into action and benefit. |

## 4.9 Memory Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑080 | Enterprise Event | A significant occurrence in any domain that is recorded in the enterprise memory. Events include plan adoptions, major promise breaches, significant forecast errors, supply disruptions, scenario recommendations, and improvement implementations. |
| SE‑KN‑081 | Outcome Record | The documented result of an enterprise decision or event. An outcome record captures what was expected, what actually occurred, and the deviation between them. Outcome records are the raw material for pattern discovery. |
| SE‑KN‑082 | Decision Record | An immutable record of a significant enterprise decision, capturing the decision context (what was known at the time), the alternatives considered, the rationale, the decision made, and the expected outcome. |
| SE‑KN‑083 | Memory Query | A request to the enterprise memory for relevant past events, decisions, or outcomes, typically framed as: “Has this situation occurred before, and what was the outcome?” Memory queries may be issued by human decision‑makers or AI agents. |

## 4.10 Knowledge Relationships  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑KN‑090 | Knowledge Dependency | A relationship where one knowledge artifact depends on another for its validity. For example, an improvement recommendation depends on the root‑cause analysis that identified the problem, and the root‑cause analysis depends on the cross‑domain pattern that triggered it. |
| SE‑KN‑091 | Knowledge Hierarchy | The organisation of knowledge artifacts into parent‑child relationships. A high‑level strategic insight may have child artifacts that provide detailed evidence, specific recommendations, and domain‑level implementation guidance. |
| SE‑KN‑092 | Knowledge Version | A specific iteration of a knowledge artifact, tracked over time. As evidence accumulates or conditions change, knowledge artifacts are versioned to maintain traceability. Previous versions remain accessible for audit. |
| SE‑KN‑093 | Knowledge Lineage | The complete history of a knowledge artifact: when it was created, what learning events triggered it, which versions exist, how its confidence evolved, and whether it was ultimately validated, superseded, or retired. |

## 4.11 Common Enumerations  

**Pattern Type**  

| Value | Description |
|-------|-------------|
| Correlation | Two or more metrics across domains move together (positive or negative correlation) |
| Causation | A demonstrated cause‑and‑effect relationship spanning domains |
| Sequence | A recurring temporal order of events across domains |
| Anomaly | A rare but significant cross‑domain event cluster |
| Structural | A persistent relationship driven by enterprise structure (e.g., BOM, network design) |

**Knowledge Lifecycle State**  

| Value | Description |
|-------|-------------|
| Proposed | Submitted for validation |
| Under Review | Being evaluated against evidence |
| Validated | Confirmed by evidence and stakeholder review |
| Published | Available in the enterprise knowledge graph |
| Superseded | Replaced by a more current artifact |
| Retired | No longer applicable |

**Improvement Status**  

| Value | Description |
|-------|-------------|
| Proposed | Submitted for evaluation |
| Approved | Authorized for implementation |
| In Progress | Being executed |
| Implemented | Changes deployed |
| Verified | Outcomes confirmed |
| Rejected | Not pursued |
| Rolled Back | Reversed due to negative impact |

**Knowledge Edge Type**  

| Value | Description |
|-------|-------------|
| DependsOn | Source depends on target |
| Produces | Source produces target |
| Consumes | Source consumes target |
| Governs | Source governs target |
| Constrains | Source constrains target |
| Validates | Source validates target |
| CorrelatesWith | Source correlates with target |
| Causes | Source causes target |
| Supersedes | Source supersedes target |
| References | Source references target |

**Feedback Loop State**  

| Value | Description |
|-------|-------------|
| Opened | Triggering event detected |
| Analyzing | Pattern discovery or root‑cause analysis in progress |
| Recommending | Improvement recommendation generated |
| Implementing | Improvement being executed |
| Verifying | Outcome evaluation in progress |
| Closed | Outcome verified, loop complete |
| Abandoned | No action taken, loop closed without improvement |

---

# Chapter 5 — Enterprise Capability Specifications  

## 5.1 Govern Knowledge Graph  

### 5.1.1 Purpose  

Maintain the enterprise knowledge graph as the authoritative, consistent, and complete representation of all enterprise concepts, capabilities, decisions, rules, policies, performance indicators, and their relationships across every Intelligence Domain. Answers: *“Is our enterprise knowledge complete, consistent, and trustworthy?”* The capability ensures that every semantic object has a unique ARS‑compliant identifier, that no contradictory definitions exist, that all mandatory relationships are present, and that the knowledge graph evolves in a governed, versioned manner.  

### 5.1.2 Business Objectives Served  

- BO‑KN‑003 Govern the Enterprise Knowledge Graph  

### 5.1.3 Enterprise Measures  

- PI‑KN‑006 Knowledge Graph Consistency Score  
- PI‑KN‑103 Knowledge Graph Coverage  
- PI‑KN‑110 Knowledge Freshness Index  

### 5.1.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑030 | Knowledge Node | Core element |
| SE‑KN‑031 | Knowledge Edge | Relationship |
| SE‑KN‑032 | Semantic Consistency Rule | Governance rule |
| SE‑KN‑033 | Ontology Version | Version control |
| SE‑KN‑034 | Knowledge Graph Coverage | Completeness metric |
| SE‑KN‑010 | Knowledge Artifact | Nodes representing knowledge |
| SE‑KN‑015 | Knowledge Provenance | Traceability |

### 5.1.5 Primitive Capabilities Composed  

- **Observe** – monitors the knowledge graph for changes, new definitions, and updates from all domains  
- **Understand** – interprets semantic definitions and relationship structures  
- **Assess** – evaluates consistency, completeness, and correctness of the graph  

### 5.1.6 Enterprise Inputs  

- Semantic objects and relationships from all Intelligence Specifications (Demand, Supply, Promise, Scenario)  
- New and updated capability specifications, rules, policies, and performance indicators from all domains  
- Change notifications when any domain publishes a new or revised enterprise object  
- The current ontology version and consistency rules  

### 5.1.7 Enterprise Understanding Produced  

- The current state of the enterprise knowledge graph with all nodes and edges validated for consistency  
- Consistency violation reports: duplicates, conflicts, missing relationships, broken references  
- Coverage assessment: which concepts and domains are fully represented, which have gaps  
- Ontology version history and pending changes  

### 5.1.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑001 | Enterprise Knowledge Graph | The governed, versioned graph of all enterprise objects and relationships |
| OUT‑KN‑002 | Consistency Violation Report | All detected inconsistencies with severity and recommended remediation |
| OUT‑KN‑003 | Coverage Gap Report | Concepts, capabilities, or domains not yet represented in the graph |
| OUT‑KN‑004 | Ontology Version | The current ontology schema version and change log |

### 5.1.9 Preconditions  

- All domains publish their semantic objects, capabilities, decisions, rules, and policies in a structured format conforming to the ARS  
- The ontology schema (node types, edge types, consistency rules) is defined and approved  
- A change notification mechanism exists across domains  

### 5.1.10 Capability Dependencies  

None within Knowledge Intelligence. Externally depends on all domains to publish their enterprise objects.

### 5.1.11 Collaborating Capabilities  

- **All domain capabilities** – publish their objects to the knowledge graph  
- **Discover Cross‑Domain Patterns** – consumes the knowledge graph for pattern detection  
- **Serve Knowledge to AI Agents** – consumes the knowledge graph for query responses  

### 5.1.12 Business Decisions  

---

#### DE‑KN‑010 — Validate Semantic Consistency  

**Purpose:** Evaluate a newly proposed or updated enterprise object (semantic concept, capability, decision, rule, policy, performance indicator) for consistency with the existing knowledge graph before it is incorporated.  

**Required Understanding:** The proposed object, the existing knowledge graph, the current ontology version, the ARS identifier and traceability standards.  

**Decision Alternatives:**  
- Accept (object is consistent and can be incorporated)  
- Accept with warnings (minor issues that do not block incorporation but should be addressed)  
- Reject (violates mandatory consistency rules — duplicate identifier, conflicting definition, broken required relationship)  

**Decision Criteria:**  
- Identifier is unique across all domains and follows the ARS format  
- Definition does not conflict with any existing definition for the same or related concept  
- All mandatory relationships (traceability to parent concepts, domain ownership) are present  
- No circular dependencies are introduced  

**Decision Confidence:** Based on completeness of the proposed object and coverage of the existing graph.  

**Decision Rationale:** *Explainability Template:* “Semantic concept SE‑KN‑046 ‘Temporary Reservation’ accepted: identifier unique, definition consistent with existing Commitment concepts, mandatory relationships to SE‑PI‑002 Promise and SE‑PI‑004 Commitment established. Rule BR‑KN‑010 passed.”  

---

##### Rules (for DE‑KN‑010)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑010 | Identifier Uniqueness Rule | Validation Rule | Every enterprise object must have an identifier that is unique across all Intelligence Domains. If a duplicate is detected, the proposed object is rejected with a reference to the existing object. |
| BR‑KN‑011 | Definition Consistency Rule | Consistency Rule | A proposed definition must not logically contradict the definition of any existing object with which it shares a relationship. Contradictions are flagged and must be resolved by the proposing domain. |
| BR‑KN‑012 | Mandatory Relationship Rule | Validation Rule | Certain object types must have defined relationships: every Decision must trace to a Capability; every Rule must trace to a Decision; every Performance Indicator must trace to a Business Objective. Missing mandatory relationships cause rejection. |
| BR‑KN‑013 | Circular Dependency Rule | Consistency Rule | The knowledge graph must remain acyclic with respect to DependsOn and Constrains relationships. Introducing a circular dependency causes rejection and requires redesign of the affected objects. |

##### Policies (for DE‑KN‑010)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑010 | Object Acceptance Policy | Automation Policy | Objects passing all consistency rules are automatically accepted and incorporated into the knowledge graph. Objects with warnings are accepted but the proposing domain is notified. Rejected objects are returned with a detailed violation report. |
| PO‑KN‑011 | Conflict Resolution Policy | Authorization Policy | If two domains propose conflicting definitions for the same concept, the Knowledge Manager convenes a review with both domain owners. The resolution must be documented and published within 10 business days. |

---

#### DE‑KN‑011 — Publish Ontology Version  

**Purpose:** Create a new versioned snapshot of the enterprise knowledge graph schema when the set of node types, edge types, or consistency rules changes.  

**Required Understanding:** Proposed schema changes, impact assessment on existing graph content, domain stakeholder approvals.  

**Decision Alternatives:**  
- Publish new major version (breaking changes to schema)  
- Publish new minor version (additions or non‑breaking changes)  
- Defer (further review required)  

**Decision Criteria:** Schema changes are reviewed for backward compatibility. Breaking changes require a major version increment and notification to all domains. Non‑breaking additions increment the minor version.  

**Decision Confidence:** High if all affected domains have reviewed and approved.  

**Decision Rationale:** “Ontology version 2.1 published: added ‘CorrelatesWith’ edge type for cross‑domain pattern representation. Non‑breaking addition. All domains notified.”  

---

##### Rules (for DE‑KN‑011)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑014 | Versioning Rule | Compliance Rule | Every ontology change must increment the version number. Breaking changes increment the major version; non‑breaking changes increment the minor version. The previous version is retained for audit. |
| BR‑KN‑015 | Domain Notification Rule | Compliance Rule | All affected domains must be notified of an ontology version change at least 5 business days before it takes effect, with a summary of changes and any required actions. |

##### Policies (for DE‑KN‑011)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑012 | Ontology Approval Policy | Approval Policy | Major version changes require approval from the Knowledge Manager and at least one Domain Manager from each affected domain. Minor version changes require Knowledge Manager approval only. |

---

#### DE‑KN‑012 — Remediate Knowledge Graph Gaps  

**Purpose:** Identify gaps in the knowledge graph—missing concepts, capabilities not linked to decisions, rules without policies—and assign remediation actions to the responsible domains.  

**Required Understanding:** Coverage assessment, gap analysis, domain ownership mapping.  

**Decision Alternatives:**  
- Assign remediation to owning domain with deadline  
- Accept gap as intentional (e.g., future capability, not yet specified)  
- Escalate gap to Knowledge Manager for prioritisation  

**Decision Criteria:** Gaps in mandatory relationships or missing objects in active domains are assigned for remediation. Gaps in future domains or intentionally deferred capabilities are documented and accepted.  

**Decision Rationale:** “Gap identified: Capability ‘Manage Inventory’ in Supply Intelligence lacks a published semantic object for ‘Safety Stock Calculation Method’. Assigned to Supply Domain Manager for remediation by end of Q3. Rule BR‑KN‑016 applied.”  

---

##### Rules (for DE‑KN‑012)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑016 | Gap Assignment Rule | Derivation Rule | Gaps in the knowledge graph are assigned to the owning domain based on the domain prefix of the affected object. If no owning domain can be determined, the Knowledge Manager is the default assignee. |
| BR‑KN‑017 | Gap Acceptance Rule | Validation Rule | A gap may be accepted as intentional only if the domain owner documents the reason and an expected resolution date. Accepted gaps are reviewed quarterly. |

##### Policies (for DE‑KN‑012)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑013 | Remediation Deadline Policy | Compliance Policy | Assigned remediations must be completed within 30 days for critical gaps (missing mandatory relationships) and 90 days for non‑critical gaps. Overdue remediations are escalated to the Knowledge Manager. |

---

### 5.1.13 Functional Behaviour  

1. **Trigger:** On publication of any new or updated enterprise object from any domain, on scheduled consistency scan (weekly), on ontology change request.  
2. **Retrieve** the proposed object(s) and current knowledge graph state.  
3. **Execute DE‑KN‑010** (Validate Semantic Consistency) for each proposed object — rules BR‑KN‑010/011/012/013, policies PO‑KN‑010/011.  
4. **Incorporate** accepted objects into the knowledge graph; return rejected objects with violation reports.  
5. **Execute DE‑KN‑011** (Publish Ontology Version) when schema changes occur — rules BR‑KN‑014/015, policy PO‑KN‑012.  
6. **Execute DE‑KN‑012** (Remediate Knowledge Graph Gaps) on a scheduled basis (monthly) — rules BR‑KN‑016/017, policy PO‑KN‑013.  
7. **Publish** updated knowledge graph and notify domains of changes.  
8. **Raise events:** `KnowledgeGraphUpdated`, `ConsistencyViolationDetected`, `OntologyVersionPublished`, `KnowledgeGapAssigned`.  

### 5.1.14 Commands  

| Command | Purpose |
|---------|---------|
| `ValidateObject` | Validate a proposed enterprise object against consistency rules |
| `IncorporateObject` | Add a validated object to the knowledge graph |
| `PublishOntologyVersion` | Create and publish a new ontology version |
| `AssignGapRemediation` | Assign a knowledge graph gap to a domain for remediation |
| `RunConsistencyScan` | Trigger a full consistency scan of the knowledge graph |

### 5.1.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `KnowledgeGraphUpdated` | Object ID, type, change type (added/updated/removed), timestamp |
| `ConsistencyViolationDetected` | Violation ID, type, affected objects, severity |
| `OntologyVersionPublished` | Version number, change summary, affected domains |
| `KnowledgeGapAssigned` | Gap ID, description, assigned domain, deadline |

### 5.1.16 Queries  

| Query | Description |
|-------|-------------|
| `GetKnowledgeGraph(filter)` | Query the graph by node type, domain, relationship |
| `GetConsistencyReport()` | Current consistency violations and status |
| `GetCoverageAssessment()` | Knowledge graph coverage by domain and object type |
| `GetOntologyVersion()` | Current ontology version and change history |

### 5.1.17 Reports  

- **Knowledge Graph Health Report** – consistency score, coverage percentage, violation trends  
- **Ontology Change Log** – version history with change descriptions  

### 5.1.18 Dashboards  

- **Knowledge Graph Explorer** – interactive graph visualisation, node and edge inspection  
- **Knowledge Health Dashboard** – consistency gauges, coverage charts, gap tracker  

### 5.1.19 Software Realization  

```
API → Application Service → Domain Model (KnowledgeGraph, OntologyVersion)  
→ Rule Engine (consistency rules, identifier validation)  
→ Event Store → Projections (GraphView, ConsistencyView) → Read Model  
→ Integration Adapters (receive publications from all domains)
```  
The knowledge graph is stored as a versioned, queryable structure. Consistency rules are configurable and hot‑reloadable. The graph supports traversal queries for pattern discovery and AI agent knowledge serving.

---

## 5.2 Discover Cross‑Domain Patterns  

### 5.2.1 Purpose  

Proactively detect statistically significant correlations, causal relationships, recurring sequences, and anomalies that span two or more Intelligence Domains. Answers: *“What patterns exist across our domains that no single domain can see, and what do they mean for the enterprise?”* The capability transforms raw outcome data from all domains into validated, actionable cross‑domain insights.  

### 5.2.2 Business Objectives Served  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  
- BO‑KN‑002 Discover Systemic Patterns and Root Causes  

### 5.2.3 Enterprise Measures  

- PI‑KN‑002 Cross‑Domain Pattern Discovery Rate  
- PI‑KN‑107 Cross‑Domain Correlation Strength  
- PI‑KN‑108 Causal Chain Confidence  
- PI‑KN‑102 Pattern Significance Score  

### 5.2.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑003 | Cross‑Domain Pattern | Primary output |
| SE‑KN‑020 | Pattern Type | Classification |
| SE‑KN‑021 | Pattern Significance | Importance measure |
| SE‑KN‑023 | Cross‑Domain Correlation | Statistical relationship |
| SE‑KN‑024 | Causal Chain | Causal explanation |
| SE‑KN‑011 | Knowledge Confidence | Confidence score |
| SE‑KN‑014 | Knowledge Evidence | Supporting data |
| SE‑KN‑015 | Knowledge Provenance | Traceability |
| SE‑KN‑002 | Learning Event | Triggering events |
| SE‑KN‑030 | Knowledge Node | Graph representation |
| SE‑KN‑031 | Knowledge Edge | Relationship representation |

### 5.2.5 Primitive Capabilities Composed  

- **Observe** – ingests outcome data, learning events, and quality metrics from all domains  
- **Understand** – computes statistical relationships and identifies candidate patterns  
- **Assess** – evaluates pattern significance, confidence, and actionability  
- **Learn** – improves pattern detection models over time based on feedback and validation outcomes  

### 5.2.6 Enterprise Inputs  

- Quality reports and outcome records from all operational domains (Demand, Supply, Promise, Scenario)  
- Learning loop closure reports from each domain’s Learn capability  
- Enterprise memory: past decisions, outcomes, and patterns  
- The current knowledge graph for context and relationship data  
- Pattern detection triggers: scheduled, event‑driven, or user‑initiated  

### 5.2.7 Enterprise Understanding Produced  

- Discovered cross‑domain patterns with type, strength, confidence, and evidence summary  
- Cross‑domain correlation matrices showing relationships between metrics in different domains  
- Pattern significance assessments and business impact estimates  
- Candidate causal chains for high‑significance correlations, ready for root‑cause analysis  

### 5.2.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑010 | Discovered Pattern | A validated cross‑domain pattern with full metadata and evidence |
| OUT‑KN‑011 | Cross‑Domain Correlation Matrix | Quantitative relationships between key metrics across domains |
| OUT‑KN‑012 | Pattern Significance Report | Ranked patterns by significance, confidence, and estimated business impact |
| OUT‑KN‑013 | Causal Chain Hypothesis | Proposed causal chain for a pattern, queued for root‑cause analysis |

### 5.2.9 Preconditions  

- At least two operational domains are actively publishing outcome data and learning events  
- The knowledge graph contains sufficient relationship data to contextualise patterns  
- Statistical thresholds for pattern detection are configured  
- A minimum volume of historical data is available (typically 4+ planning cycles)  

### 5.2.10 Capability Dependencies  

- `CA‑KN‑001 Govern Knowledge Graph` – for knowledge graph context and relationship data  
- External: Learn capabilities in Demand, Supply, Promise, and Scenario for outcome data and learning events  

### 5.2.11 Collaborating Capabilities  

- **Analyze Root Causes** – consumes causal chain hypotheses for deep investigation  
- **Manage Improvement Portfolio** – consumes significant patterns for improvement recommendations  
- **Explain Knowledge Insights** – consumes patterns for explanation generation  

### 5.2.12 Business Decisions  

---

#### DE‑KN‑020 — Detect Candidate Patterns  

**Purpose:** Apply statistical and machine learning methods to outcome data from multiple domains to identify candidate cross‑domain patterns—correlations, sequences, and anomalies that exceed defined thresholds.  

**Required Understanding:** Outcome data from all domains (forecast accuracy, plan adherence, promise adherence, scenario accuracy, etc.), time alignment of data across domains, detection thresholds.  

**Decision Alternatives:**  
- Candidate pattern detected (correlation, sequence, or anomaly exceeding threshold)  
- No pattern detected (below threshold or insufficient data)  
- Inconclusive (further data or different method required)  

**Decision Criteria:**  
- Correlation strength exceeds configured threshold (default: Pearson |r| > 0.6 or Spearman ρ > 0.5)  
- Sequence frequency exceeds expected by chance (statistical significance p < 0.05)  
- Anomaly cluster density exceeds baseline by >3 standard deviations  

**Decision Confidence:** Derived from statistical significance, sample size, and data quality.  

**Decision Rationale:** *Explainability Template:* “Candidate pattern P‑007 detected: strong negative correlation (r = −0.78, p < 0.01) between Demand forecast bias for Product Family PF3 and Promise On‑Time Delivery for the same family over 8 planning cycles. Suggests forecast overestimation causes promise breaches. Queued for validation. Rule BR‑KN‑020 applied.”  

---

##### Rules (for DE‑KN‑020)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑020 | Correlation Threshold Rule | Derivation Rule | A candidate cross‑domain correlation pattern is triggered when the absolute correlation coefficient between two metrics from different domains exceeds 0.6 and is statistically significant (p < 0.05). |
| BR‑KN‑021 | Minimum Data Rule | Validation Rule | Pattern detection requires at least 8 aligned data points (planning cycles or time periods) across all involved domains. Fewer data points produce an “Insufficient Data” flag. |
| BR‑KN‑022 | Domain Diversity Rule | Validation Rule | A candidate pattern must involve artifacts from at least two different Intelligence Domains. Single‑domain patterns are the responsibility of that domain’s Learn capability. |
| BR‑KN‑023 | False Discovery Rate Rule | Validation Rule | When testing multiple hypotheses simultaneously, the false discovery rate must be controlled (Benjamini‑Hochberg procedure at α = 0.05). Patterns that only appear significant due to multiple comparisons are suppressed. |

##### Policies (for DE‑KN‑020)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑020 | Detection Frequency Policy | Compliance Policy | Scheduled pattern detection runs weekly for operational patterns (short‑cycle) and monthly for strategic patterns (long‑cycle). Event‑driven detection is triggered within 24 hours of a significant learning event in any domain. |
| PO‑KN‑021 | Detection Method Policy | Compliance Policy | The statistical methods used for pattern detection are reviewed annually by the Data Science team. Any change to methods must be validated on historical data before production use. |

---

#### DE‑KN‑021 — Validate Discovered Pattern  

**Purpose:** Subject a candidate pattern to rigorous validation: confirm statistical significance with additional data, verify domain relevance with stakeholders, and assess business impact.  

**Required Understanding:** Candidate pattern with evidence, additional data for hold‑out validation, domain stakeholder input, business context.  

**Decision Alternatives:**  
- Validate and publish (pattern confirmed, confidence high)  
- Validate as provisional (pattern likely but requires ongoing monitoring)  
- Reject (pattern fails validation — statistical artifact, spurious correlation, or no business relevance)  
- Defer (insufficient data for validation, continue monitoring)  

**Decision Criteria:** Pattern holds on hold‑out data; domain stakeholders confirm business plausibility; estimated business impact exceeds minimum threshold; no confounding variable identified that explains the pattern.  

**Decision Confidence:** Updated based on validation results.  

**Decision Rationale:** “Pattern P‑007 validated and published: confirmed on hold‑out data (r = −0.74), Demand and Promise domain managers confirm business plausibility (forecast overestimation → excess supply commitment → late deliveries), estimated impact $1.2M annually in avoidable promise breaches. Confidence 91%.”  

---

##### Rules (for DE‑KN‑021)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑024 | Hold‑Out Validation Rule | Validation Rule | A candidate pattern must be validated on a hold‑out dataset (at least 25% of the available data, not used in detection). Pattern strength must remain above threshold on the hold‑out data. |
| BR‑KN‑025 | Stakeholder Review Rule | Validation Rule | Every cross‑domain pattern must be reviewed by at least one domain stakeholder from each involved domain. Stakeholders confirm business plausibility and relevance. |
| BR‑KN‑026 | Confounding Check Rule | Consistency Rule | Before publication, potential confounding variables (external factors that could explain the pattern independently) must be evaluated. If a plausible confound is identified, the pattern is held until the confound can be controlled. |

##### Policies (for DE‑KN‑021)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑022 | Pattern Publication Policy | Automation Policy | Patterns that pass all validation rules with confidence ≥80% are automatically published to the knowledge graph. Patterns with confidence 60–80% are published as provisional. Patterns below 60% are rejected or deferred. |
| PO‑KN‑023 | Stakeholder Review Deadline Policy | Compliance Policy | Domain stakeholders must complete their review within 10 business days of a validation request. Unreviewed patterns are escalated to the Knowledge Manager. |

---

#### DE‑KN‑022 — Propose Causal Chain  

**Purpose:** For validated correlation or sequence patterns, propose a causal chain hypothesis that explains the observed relationship in terms of enterprise cause‑and‑effect.  

**Required Understanding:** Validated pattern, knowledge graph relationships, domain expertise, historical decision records.  

**Decision Alternatives:**  
- Causal chain proposed (complete hypothesis with all links identified)  
- Partial causal chain (some links identified, gaps remain)  
- No causal chain possible (pattern is purely correlational with no identifiable mechanism)  

**Decision Criteria:** Each link in the causal chain must be supported by a known enterprise mechanism (e.g., a BOM dependency, a lead time relationship, a policy rule) or by statistical mediation analysis.  

**Decision Confidence:** Derived from the weakest link in the chain.  

**Decision Rationale:** “Causal chain proposed for Pattern P‑007: (1) Demand forecast overestimates demand for PF3 by average +12% → (2) Supply plan generates excess supply commitments for PF3 components → (3) Production capacity is consumed by these excess commitments → (4) Genuine customer orders for PF3 cannot be fulfilled on time → (5) Promise breaches occur. All links verified via BOM dependencies and capacity analysis. Chain confidence 87%.”  

---

##### Rules (for DE‑KN‑022)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑027 | Causal Link Evidence Rule | Validation Rule | Each link in a proposed causal chain must be supported by at least one of: (a) a documented enterprise relationship (BOM, routing, policy), (b) statistical mediation analysis showing the link is a mediator, or (c) a controlled experiment or natural experiment. |
| BR‑KN‑028 | Causal Chain Completeness Rule | Validation Rule | A complete causal chain must connect the originating event in one domain to the observed outcome in another domain without logical gaps. Partial chains are published as hypotheses and queued for further investigation. |

##### Policies (for DE‑KN‑022)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑024 | Causal Chain Prioritisation Policy | Authorization Policy | Causal chains for patterns with estimated annual impact >$1M are prioritised for immediate root‑cause analysis. Lower‑impact chains are queued for scheduled analysis. |

---

### 5.2.13 Functional Behaviour  

1. **Trigger:** Scheduled (weekly for operational patterns, monthly for strategic patterns), event‑driven (within 24 hours of a significant learning event), on‑demand (user‑initiated inquiry).  
2. **Retrieve** outcome data, quality metrics, and learning events from all domains via their published event streams and quality reports.  
3. **Align** data temporally and by enterprise dimensions (product, location, customer, time bucket).  
4. **Execute DE‑KN‑020** (Detect Candidate Patterns) using statistical and ML methods — rules BR‑KN‑020/021/022/023, policies PO‑KN‑020/021.  
5. **For each candidate pattern**, execute DE‑KN‑021 (Validate Discovered Pattern) — rules BR‑KN‑024/025/026, policies PO‑KN‑022/023.  
6. **For validated correlation or sequence patterns**, execute DE‑KN‑022 (Propose Causal Chain) — rules BR‑KN‑027/028, policy PO‑KN‑024.  
7. **Publish** validated patterns and causal chain hypotheses to the knowledge graph.  
8. **Raise events:** `CandidatePatternDetected`, `PatternValidated`, `PatternRejected`, `CausalChainProposed`.  

### 5.2.14 Commands  

| Command | Purpose |
|---------|---------|
| `RunPatternDetection` | Initiate a scheduled or on‑demand pattern detection scan |
| `ValidatePattern` | Submit a candidate pattern for validation |
| `ProposeCausalChain` | Generate a causal chain hypothesis for a validated pattern |
| `RejectPattern` | Manually reject a candidate or validated pattern with reason |

### 5.2.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `CandidatePatternDetected` | Pattern ID, type, involved domains, metrics, strength, confidence |
| `PatternValidated` | Pattern ID, validation method, hold‑out results, stakeholder confirmation |
| `PatternRejected` | Pattern ID, reason |
| `CausalChainProposed` | Pattern ID, chain ID, links, confidence per link |

### 5.2.16 Queries  

| Query | Description |
|-------|-------------|
| `GetDiscoveredPatterns(filter)` | Patterns by type, domains, confidence, period |
| `GetCrossDomainCorrelations(domain1, domain2)` | Correlation matrix between two domains |
| `GetCausalChain(chainId)` | Full causal chain with evidence per link |
| `GetPatternEvidence(patternId)` | All evidence supporting a pattern |

### 5.2.17 Reports  

- **Cross‑Domain Pattern Report** – all validated patterns with significance, confidence, and impact estimates  
- **Correlation Matrix Report** – quantitative relationships between key metrics across all domains  

### 5.2.18 Dashboards  

- **Cross‑Domain Insight Dashboard** – pattern discovery feed, significance rankings, causal chain visualiser  
- **Pattern Significance Heatmap** – visual representation of pattern strength across domain pairs  

### 5.2.19 Software Realization  

```
Scheduled/Event Triggers → Analytics Engine (statistical correlation, sequence mining, anomaly detection)  
→ Domain Service (Pattern aggregate, CausalChain)  
→ Rule Engine (validation rules, stakeholder workflow)  
→ Event Store → Projections (PatternCatalogue, CorrelationMatrix) → Read Model
```  
The analytics engine queries outcome data from all domains via their published APIs and event streams. Statistical methods are configurable and versioned. Validation workflows route patterns to domain stakeholders for business plausibility review. Causal chain proposals use the knowledge graph to identify potential enterprise mechanisms. All patterns and chains are immutably recorded with provenance.  

---

## 5.3 Analyze Root Causes  

### 5.3.1 Purpose  

Trace systemic enterprise problems that span multiple domains to their fundamental originating causes. Answers: *“Why did this happen, and what is the real source of the problem?”* The capability transforms cross‑domain patterns and significant failure events into rigorous root‑cause analyses, distinguishing root causes from proximate triggers and contributing factors, and recommending corrective actions that prevent recurrence rather than merely treating symptoms.  

### 5.3.2 Business Objectives Served  

- BO‑KN‑002 Discover Systemic Patterns and Root Causes  

### 5.3.3 Enterprise Measures  

- PI‑KN‑003 Root‑Cause Identification Accuracy  
- PI‑KN‑108 Causal Chain Confidence  

### 5.3.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑050 | Root Cause | Primary output |
| SE‑KN‑051 | Contributing Factor | Secondary factor |
| SE‑KN‑052 | Root‑Cause Confidence | Confidence score |
| SE‑KN‑053 | Root‑Cause Analysis | Structured investigation |
| SE‑KN‑024 | Causal Chain | Input hypothesis |
| SE‑KN‑014 | Knowledge Evidence | Supporting data |
| SE‑KN‑015 | Knowledge Provenance | Traceability |
| SE‑KN‑010 | Knowledge Artifact | Output classification |

### 5.3.5 Primitive Capabilities Composed  

- **Understand** – interprets causal chain hypotheses, evidence, and domain context  
- **Assess** – evaluates causal strength and distinguishes root cause from contributing factors  
- **Learn** – improves root‑cause identification accuracy over time from validation outcomes  

### 5.3.6 Enterprise Inputs  

- Causal chain hypotheses from Discover Cross‑Domain Patterns  
- Significant failure events and outcome records from all domains  
- Enterprise memory: past root‑cause analyses, decisions, and outcomes  
- Knowledge graph for enterprise relationships, dependencies, and constraints  
- Domain expert input and stakeholder interviews  

### 5.3.7 Enterprise Understanding Produced  

- Documented root‑cause analyses with identified root cause(s), contributing factors, and evidence  
- Confidence scores for each identified root cause  
- Corrective action recommendations with expected impact and implementation guidance  
- Traceability from the systemic problem back through causal chains to the originating cause  

### 5.3.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑020 | Root‑Cause Analysis Report | Complete analysis with causal chain, root cause, contributing factors, evidence |
| OUT‑KN‑021 | Corrective Action Recommendation | Specific actions to address the root cause, with expected impact |
| OUT‑KN‑022 | Root‑Cause Confidence Assessment | Confidence score with basis and limitations |

### 5.3.9 Preconditions  

- A validated cross‑domain pattern or significant failure event exists as a trigger  
- Sufficient outcome data and domain context are available for analysis  
- Domain stakeholders are available for review and input  

### 5.3.10 Capability Dependencies  

- `CA‑KN‑002 Discover Cross‑Domain Patterns` – for causal chain hypotheses and patterns  
- `CA‑KN‑001 Govern Knowledge Graph` – for enterprise relationship data  

### 5.3.11 Collaborating Capabilities  

- **Manage Improvement Portfolio** – consumes corrective action recommendations  
- **Institutionalise Best Practices** – consumes validated root causes for best‑practice nomination  
- **Explain Knowledge Insights** – consumes analyses for explanation generation  

### 5.3.12 Business Decisions  

---

#### DE‑KN‑030 — Identify Root Cause  

**Purpose:** Conduct a structured investigation to identify the fundamental root cause(s) of a systemic problem, distinguishing them from proximate causes and contributing factors.  

**Required Understanding:** Causal chain hypothesis or failure event, supporting evidence, knowledge graph relationships, domain expert input.  

**Decision Alternatives:**  
- Root cause identified (single or primary cause determined)  
- Multiple root causes identified (several independent fundamental causes)  
- Inconclusive (insufficient evidence, further investigation required)  
- No systemic root cause (problem is a one‑time event or purely external)  

**Decision Criteria:** A root cause must satisfy: (1) it is the deepest identifiable cause in the causal chain—removing it would prevent the problem; (2) it is within the enterprise’s ability to influence or control; (3) it is supported by evidence from at least two independent sources (data analysis, domain expert confirmation, documented relationships).  

**Decision Confidence:** Derived from evidence strength, expert consensus, and historical accuracy of similar analyses.  

**Decision Rationale:** *Explainability Template:* “Root cause identified for systemic promise breaches in Q3: Demand forecast model for Product Family PF3 had developed a systematic +12% bias after a parameter drift went undetected for 3 cycles. This is the root cause because: (1) correcting the forecast bias would eliminate the excess supply commitments that consumed capacity; (2) the parameter drift is detectable and correctable; (3) confirmed by both statistical analysis of forecast errors and Supply domain capacity records. Contributing factors: lack of automated forecast bias monitoring (amplified the problem). Rule BR‑KN‑030 applied.”  

---

##### Rules (for DE‑KN‑030)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑030 | Root Cause Depth Rule | Derivation Rule | A root cause must be the deepest identifiable cause in the causal chain. The investigator must ask “why” iteratively until no further enterprise‑controllable cause can be identified. A cause that merely restates the symptom is rejected. |
| BR‑KN‑031 | Multi‑Source Evidence Rule | Validation Rule | A root cause identification must be supported by evidence from at least two independent sources (data analysis, domain expert, documented relationship, historical precedent). Single‑source identifications are classified as hypotheses, not root causes. |
| BR‑KN‑032 | Distinction Rule | Consistency Rule | The analysis must explicitly distinguish between Root Causes (fundamental origins), Proximate Causes (immediate triggers), and Contributing Factors (conditions that amplified the problem). Conflating these categories is a methodological error. |

##### Policies (for DE‑KN‑030)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑030 | Stakeholder Review Policy (Root Cause) | Compliance Policy | Every root‑cause analysis must be reviewed by at least one domain stakeholder from each affected domain before publication. Stakeholders confirm the analysis is complete and the identified root cause is plausible. |
| PO‑KN‑031 | Inconclusive Analysis Policy | Exception Policy | If a root cause cannot be confidently identified after two rounds of investigation, the analysis is published as “Inconclusive” with documented gaps. It is revisited quarterly or when new evidence becomes available. |

---

#### DE‑KN‑031 — Validate Root Cause  

**Purpose:** Verify that the identified root cause is correct by testing the recommended corrective action and observing whether the systemic problem is resolved.  

**Required Understanding:** Identified root cause, recommended corrective action, pre‑intervention baseline metrics, post‑intervention outcome data.  

**Decision Alternatives:**  
- Validated (corrective action resolved the problem — root cause confirmed)  
- Partially validated (problem reduced but not eliminated — additional causes may exist)  
- Not validated (corrective action did not resolve the problem — root cause likely incorrect)  

**Decision Criteria:** The systemic problem metric must show a statistically significant improvement (p < 0.05) and a reduction of ≥50% from baseline within two planning cycles after corrective action.  

**Decision Confidence:** Updated based on outcome evidence.  

**Decision Rationale:** “Root cause RCA‑001 validated: after forecast bias correction for PF3, promise breaches decreased 65% (from 12% to 4.2%) within 2 cycles, exceeding the 50% threshold. Root cause confirmed.”  

---

##### Rules (for DE‑KN‑031)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑033 | Outcome Verification Rule | Validation Rule | Validation requires a minimum observation period of two full planning cycles after corrective action implementation, with statistically significant improvement (p < 0.05) on the primary problem metric. |
| BR‑KN‑034 | Partial Validation Rule | Derivation Rule | If the problem is reduced by ≥30% but <50%, the root cause is classified as “Partially Validated” and a search for additional root causes is triggered. |

##### Policies (for DE‑KN‑031)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑032 | Validation Tracking Policy | Compliance Policy | Every root‑cause analysis with an implemented corrective action is tracked for validation. Analyses that remain unvalidated after 6 months are escalated to the Knowledge Manager. |

---

#### DE‑KN‑032 — Recommend Corrective Action  

**Purpose:** Generate specific, actionable recommendations to address the identified root cause, including implementation guidance, expected impact, and monitoring plan.  

**Required Understanding:** Identified root cause, contributing factors, domain capabilities and constraints, implementation feasibility.  

**Decision Alternatives:**  
- Recommend specific corrective action(s) with implementation plan  
- Recommend further investigation (if root cause is uncertain)  
- Recommend no action (if cost of correction exceeds benefit)  

**Decision Criteria:** Corrective action must directly address the root cause; expected benefit must exceed implementation cost (positive ROI); action must be feasible within the affected domain’s capabilities.  

**Decision Confidence:** Based on evidence strength and implementation feasibility.  

**Decision Rationale:** “Corrective action recommended for RCA‑001: (1) Retune forecast model for PF3 using recent data (immediate, cost $5K). (2) Implement automated forecast bias monitoring rule across all product families (within 30 days, cost $15K). Expected benefit: $1.2M annually in avoided promise breaches. ROI: 60:1.”  

---

##### Rules (for DE‑KN‑032)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑035 | Action‑Cause Alignment Rule | Validation Rule | Every recommended corrective action must directly address the identified root cause. Actions that address only proximate causes or contributing factors must be explicitly labelled as such. |
| BR‑KN‑036 | Feasibility Check Rule | Validation Rule | Corrective actions must be reviewed for feasibility by the domain that will implement them. Actions deemed infeasible by the domain owner are returned for revision with documented reasons. |

##### Policies (for DE‑KN‑032)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑033 | Corrective Action Prioritisation Policy | Authorization Policy | Corrective actions with expected annual benefit >$500K are prioritised and presented to the executive S&OP review within one cycle. |

---

### 5.3.13 Functional Behaviour  

1. **Trigger:** On publication of a validated cross‑domain pattern with a causal chain hypothesis, on detection of a significant failure event spanning multiple domains, on scheduled quarterly systemic review.  
2. **Retrieve** causal chain, evidence, knowledge graph relationships, domain context.  
3. **Conduct** structured root‑cause investigation using iterative “why” analysis.  
4. **Execute DE‑KN‑030** (Identify Root Cause) — rules BR‑KN‑030/031/032, policies PO‑KN‑030/031.  
5. **Execute DE‑KN‑032** (Recommend Corrective Action) — rules BR‑KN‑035/036, policy PO‑KN‑033.  
6. **Publish** root‑cause analysis with corrective action recommendations.  
7. **After corrective action implementation**, execute DE‑KN‑031 (Validate Root Cause) — rules BR‑KN‑033/034, policy PO‑KN‑032.  
8. **Raise events:** `RootCauseIdentified`, `CorrectiveActionRecommended`, `RootCauseValidated`, `RootCauseValidationFailed`.  

### 5.3.14 Commands  

| Command | Purpose |
|---------|---------|
| `StartRootCauseAnalysis` | Initiate analysis for a given pattern or event |
| `IdentifyRootCause` | Submit identified root cause for review |
| `RecommendCorrectiveAction` | Generate and submit corrective action recommendation |
| `ValidateRootCause` | Evaluate root cause after corrective action implementation |

### 5.3.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `RootCauseIdentified` | Analysis ID, root cause, contributing factors, confidence |
| `CorrectiveActionRecommended` | Analysis ID, action details, expected impact, implementing domain |
| `RootCauseValidated` | Analysis ID, outcome metrics, confidence update |
| `RootCauseValidationFailed` | Analysis ID, reason |

### 5.3.16 Queries  

| Query | Description |
|-------|-------------|
| `GetRootCauseAnalysis(analysisId)` | Full analysis with causal chain and evidence |
| `GetCorrectiveActions(filter)` | Actions by status, domain, impact |
| `GetValidationStatus(analysisId)` | Validation outcome and metrics |

### 5.3.17 Reports  

- **Root‑Cause Analysis Report** – completed analyses with findings and recommendations  
- **Corrective Action Tracking Report** – implementation status and validation outcomes  

### 5.3.18 Dashboards  

- **Root‑Cause Investigation Workbench** – causal chain visualiser, evidence explorer  
- **Corrective Action Dashboard** – action status, validation progress, ROI tracking  

### 5.3.19 Software Realization  

```
API → Application Service → Domain Model (RootCauseAnalysis, CorrectiveAction)  
→ Investigation Engine (causal chain traversal, evidence correlation)  
→ Knowledge Graph Adapter (relationship queries)  
→ Event Store → Projections (AnalysisCatalogue) → Read Model
```  
The investigation engine supports structured root‑cause methods (5 Whys, Fishbone, causal graph traversal). It correlates evidence from multiple domains automatically and flags gaps. Stakeholder review workflows route analyses to domain experts.  

---

## 5.4 Manage Improvement Portfolio  

### 5.4.1 Purpose  

Prioritise, track, govern, and verify the enterprise‑wide portfolio of improvement initiatives that emerge from cross‑domain pattern discoveries, root‑cause analyses, and best‑practice nominations. Answers: *“Which improvements should we invest in, are they on track, and are they delivering the expected benefits?”* The capability ensures that enterprise learning translates into managed, measurable, and governed action.  

### 5.4.2 Business Objectives Served  

- BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement  

### 5.4.3 Enterprise Measures  

- PI‑KN‑004 Improvement Portfolio ROI  
- PI‑KN‑109 Improvement Adoption Rate  
- PI‑KN‑012 Feedback Loop Closure Rate  

### 5.4.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑040 | Improvement Initiative | Core managed entity |
| SE‑KN‑041 | Improvement Status | Lifecycle state |
| SE‑KN‑042 | Improvement ROI | Financial measure |
| SE‑KN‑043 | Improvement Dependency | Relationship between initiatives |
| SE‑KN‑004 | Improvement Portfolio | Aggregate collection |

### 5.4.5 Primitive Capabilities Composed  

- **Observe** – monitors initiative progress and outcomes  
- **Assess** – evaluates ROI, prioritisation, and portfolio balance  
- **Evaluate** – compares expected vs. actual benefits  
- **Learn** – improves portfolio prioritisation accuracy  

### 5.4.6 Enterprise Inputs  

- Improvement proposals from root‑cause analyses, pattern discoveries, and best‑practice nominations  
- Domain‑level improvement initiatives from Learn capabilities in Demand, Supply, Promise, Scenario  
- Financial data: cost estimates, benefit projections, actual costs and outcomes  
- Strategic objectives and risk appetite  
- Resource availability and domain capacity  

### 5.4.7 Enterprise Understanding Produced  

- Prioritised improvement portfolio with ranked initiatives  
- Status tracking for all active initiatives  
- ROI projections and validated actuals  
- Portfolio health: balance across domains, risk diversification, resource allocation  

### 5.4.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑030 | Prioritised Improvement Portfolio | Ranked initiatives with scores, ROI, and status |
| OUT‑KN‑031 | Initiative Status Report | Current status, milestones, risks for each initiative |
| OUT‑KN‑032 | Portfolio ROI Report | Aggregate and per‑initiative ROI |
| OUT‑KN‑033 | Portfolio Health Dashboard Data | Balance, risk, and resource metrics |

### 5.4.9 Preconditions  

- Improvement proposals include estimated costs, benefits, and affected domains  
- Domain resource availability and capacity are known  
- Strategic priorities are current  

### 5.4.10 Capability Dependencies  

- `CA‑KN‑003 Analyze Root Causes` – for corrective action proposals  
- `CA‑KN‑005 Institutionalise Best Practices` – for best‑practice nominations  
- External: Learn capabilities in all domains for domain‑level initiatives  

### 5.4.11 Collaborating Capabilities  

- **Institutionalise Best Practices** – consumes verified improvements for institutionalisation  
- **Orchestrate Feedback Loops** – consumes initiative outcomes for loop closure  
- **Explain Knowledge Insights** – consumes portfolio data for explanation  

### 5.4.12 Business Decisions  

---

#### DE‑KN‑040 — Propose Improvement Initiative  

**Purpose:** Formalise an improvement recommendation into a proposal suitable for portfolio evaluation, with complete business case.  

**Required Understanding:** Improvement origin (root cause, pattern, best practice), expected benefit, estimated cost, affected domains, implementation timeline, dependencies.  

**Decision Alternatives:**  
- Accept proposal for portfolio evaluation  
- Return for refinement (incomplete business case)  
- Reject (duplicate, out of scope, or cost exceeds benefit)  

**Decision Criteria:** Business case must include quantified expected benefit, cost estimate, affected domains, implementation owner, and success criteria.  

**Decision Rationale:** “Improvement initiative IMP‑007 proposed: ‘Automated Forecast Bias Monitoring’ based on RCA‑001. Expected benefit $1.2M/year, cost $20K, implementing domain Demand. Business case complete. Rule BR‑KN‑040 passed.”  

---

##### Rules (for DE‑KN‑040)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑040 | Business Case Completeness Rule | Validation Rule | Every improvement proposal must include: quantified expected benefit, estimated cost, affected domains, proposed implementation owner, timeline, and success criteria. Incomplete proposals are returned. |
| BR‑KN‑041 | Duplicate Detection Rule | Validation Rule | A proposal that substantially duplicates an existing active initiative is flagged and merged or rejected. |

##### Policies (for DE‑KN‑040)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑040 | Proposal Submission Policy | Compliance Policy | Improvement proposals may be submitted by any domain manager or by Knowledge Intelligence. All proposals follow the same evaluation process. |

---

#### DE‑KN‑041 — Prioritise Improvement Portfolio  

**Purpose:** Rank all active and proposed improvement initiatives based on expected ROI, strategic alignment, risk reduction, and implementation feasibility.  

**Required Understanding:** All active and proposed initiatives, strategic priorities, resource capacity, initiative dependencies.  

**Decision Alternatives:**  
- Ranked portfolio with top‑priority initiatives identified  
- Recommend deferral for low‑priority initiatives  
- Recommend acceleration for initiatives addressing critical risks  

**Decision Criteria:** Composite score = (ROI weight × 0.4) + (Strategic Alignment × 0.3) + (Risk Reduction × 0.2) + (Implementation Readiness × 0.1). Weights configurable.  

**Decision Rationale:** “Portfolio prioritised for Q4: IMP‑007 (Automated Bias Monitoring) ranked #1 with composite score 92, expected ROI 60:1, addresses systemic risk identified in RCA‑001.”  

---

##### Rules (for DE‑KN‑041)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑042 | Prioritisation Formula Rule | Calculation Rule | Composite score = Σ (Weight × Normalised Factor). Default weights: ROI 0.4, Strategic Alignment 0.3, Risk Reduction 0.2, Implementation Readiness 0.1. Weights configurable by Knowledge Manager. |
| BR‑KN‑043 | Dependency Rule | Consistency Rule | Initiatives that depend on other incomplete initiatives are flagged and their priority adjusted. An initiative cannot be started until its dependencies are complete. |
| BR‑KN‑044 | Resource Constraint Rule | Validation Rule | The prioritised portfolio must respect domain resource capacity. Initiatives that exceed available capacity are deferred or re‑scoped. |

##### Policies (for DE‑KN‑041)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑041 | Portfolio Review Frequency Policy | Compliance Policy | The improvement portfolio is reviewed and reprioritised monthly. Strategic reprioritisation occurs quarterly. |
| PO‑KN‑042 | Resource Allocation Policy | Authorization Policy | Resource allocation across domains for improvement initiatives is approved by the executive S&OP review for initiatives with total cost >$100K. |

---

#### DE‑KN‑042 — Approve Implementation  

**Purpose:** Authorise a prioritised improvement initiative to proceed to implementation, with defined scope, budget, timeline, and success criteria.  

**Required Understanding:** Prioritised initiative, resource availability, funding approval.  

**Decision Alternatives:** Approve, Approve with conditions, Defer, Reject.  

**Decision Criteria:** Initiative is in the top priority tier, resources are allocated, funding is approved per policy, success criteria are measurable.  

**Decision Rationale:** “IMP‑007 approved for implementation: budget $20K, timeline 30 days, success criteria: forecast bias monitoring deployed across all product families with alerting active.”  

---

##### Rules (for DE‑KN‑042)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑045 | Approval Gate Rule | Validation Rule | An initiative may be approved only if it has a complete business case, a resource assignment, measurable success criteria, and required funding approvals. |

##### Policies (for DE‑KN‑042)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑043 | Funding Approval Policy | Authorization Policy | Initiatives with total cost ≤$50K require Domain Manager approval. $50K–$250K require Director approval. >$250K require VP/CFO approval. |

---

#### DE‑KN‑043 — Verify Improvement Outcome  

**Purpose:** After implementation, evaluate whether the improvement initiative delivered its expected benefits.  

**Required Understanding:** Initiative success criteria, pre‑implementation baseline, post‑implementation outcome data.  

**Decision Alternatives:** Verified (benefits achieved), Partially verified, Not verified, Rollback recommended.  

**Decision Criteria:** Actual benefit ≥80% of expected → Verified. 50–80% → Partially verified. <50% → Not verified. Negative impact → Rollback.  

**Decision Rationale:** “IMP‑007 verified: forecast bias monitoring active across all product families. Promise breaches attributable to forecast bias reduced 65%, exceeding the expected 50% reduction. Benefit $1.1M vs. expected $1.2M (92%). Verified.”  

---

##### Rules (for DE‑KN‑043)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑046 | Verification Window Rule | Validation Rule | Outcome verification requires a minimum observation period: 2 planning cycles for operational improvements, 4 cycles for strategic improvements. |
| BR‑KN‑047 | Rollback Trigger Rule | Model Evaluation Rule | If an improvement causes a statistically significant degradation in any primary KPI, an automatic rollback recommendation is generated. |

##### Policies (for DE‑KN‑043)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑044 | Verification Reporting Policy | Compliance Policy | Every implemented improvement must have a verification report within one cycle after the observation window closes. Unverified improvements are escalated quarterly. |

---

### 5.4.13 Functional Behaviour  

1. **Trigger:** On new improvement proposal, on scheduled monthly portfolio review, on implementation milestone completion.  
2. **Execute DE‑KN‑040** (Propose Improvement Initiative) for new proposals — rules BR‑KN‑040/041, policy PO‑KN‑040.  
3. **Execute DE‑KN‑041** (Prioritise Improvement Portfolio) monthly — rules BR‑KN‑042/043/044, policies PO‑KN‑041/042.  
4. **Execute DE‑KN‑042** (Approve Implementation) for top‑priority initiatives — rule BR‑KN‑045, policy PO‑KN‑043.  
5. **Track** implementation progress and collect outcome data.  
6. **Execute DE‑KN‑043** (Verify Improvement Outcome) after observation window — rules BR‑KN‑046/047, policy PO‑KN‑044.  
7. **Raise events:** `ImprovementProposed`, `PortfolioPrioritised`, `ImplementationApproved`, `ImprovementVerified`, `ImprovementRollbackRecommended`.  

### 5.4.14 Commands  

| Command | Purpose |
|---------|---------|
| `ProposeImprovement` | Submit a new improvement initiative |
| `PrioritisePortfolio` | Run portfolio prioritisation |
| `ApproveImplementation` | Authorise an initiative to proceed |
| `VerifyOutcome` | Evaluate implemented initiative outcomes |

### 5.4.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ImprovementProposed` | Initiative ID, origin, expected benefit, cost |
| `PortfolioPrioritised` | Ranked list, scores, timestamp |
| `ImplementationApproved` | Initiative ID, budget, timeline, owner |
| `ImprovementVerified` | Initiative ID, actual benefit, verification status |

### 5.4.16 Queries  

| Query | Description |
|-------|-------------|
| `GetPortfolio(filter)` | Initiatives by status, domain, priority |
| `GetInitiative(initiativeId)` | Full initiative details |
| `GetPortfolioROI(period)` | Aggregate and per‑initiative ROI |

### 5.4.17 Reports  

- **Improvement Portfolio Report** – ranked initiatives, status, ROI  
- **Portfolio Health Report** – balance, resource utilisation, risk diversification  

### 5.4.18 Dashboards  

- **Improvement Portfolio Dashboard** – prioritised list, status tracking, ROI trends  
- **Portfolio Health Dashboard** – domain balance, resource load, risk heatmap  

### 5.4.19 Software Realization  

```
API → Application Service → Domain Model (ImprovementInitiative, Portfolio)  
→ Prioritisation Engine (weighted scoring, dependency resolution)  
→ Event Store → Projections (PortfolioView) → Read Model
```  

---

## 5.5 Institutionalise Best Practices  

### 5.5.1 Purpose  

Capture proven strategies, methods, and configurations that have demonstrated superior outcomes, validate them as enterprise best practices, and propagate them across all applicable domains. Answers: *“What works best, and how do we make sure everyone does it?”* The capability transforms isolated successes into enterprise standards.  

### 5.5.2 Business Objectives Served  

- BO‑KN‑005 Institutionalise Best Practices  

### 5.5.3 Enterprise Measures  

- PI‑KN‑005 Best‑Practice Institutionalisation Rate  
- PI‑KN‑109 Improvement Adoption Rate  

### 5.5.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑060 | Best Practice | Core entity |
| SE‑KN‑061 | Practice Provenance | Origin and validation |
| SE‑KN‑062 | Practice Applicability | Conditions for use |
| SE‑KN‑063 | Practice Institutionalisation | Adoption process |
| SE‑KN‑010 | Knowledge Artifact | Output |

### 5.5.5 Primitive Capabilities Composed  

- **Understand** – interprets outcome evidence and practice context  
- **Assess** – evaluates generalisability and applicability  
- **Evaluate** – verifies practice effectiveness across domains  
- **Learn** – improves institutionalisation strategies  

### 5.5.6 Enterprise Inputs  

- Verified improvement outcomes from Manage Improvement Portfolio  
- Successful practices identified by domain Learn capabilities  
- Domain context: demand patterns, supply network, customer segments  
- Adoption status across domains  

### 5.5.7 Enterprise Understanding Produced  

- Validated best practices with provenance, applicability conditions, and adoption status  
- Institutionalisation plans for each practice  
- Adoption compliance reports  

### 5.5.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑040 | Validated Best Practice | Practice with evidence, applicability, and adoption guidance |
| OUT‑KN‑041 | Institutionalisation Plan | Steps to propagate and embed the practice |
| OUT‑KN‑042 | Adoption Compliance Report | Which domains have adopted, which have not, and why |

### 5.5.9 Preconditions  

- A verified improvement or successful practice exists as a candidate  
- Evidence of effectiveness is documented with outcome data  
- Applicable domains are identified  

### 5.5.10 Capability Dependencies  

- `CA‑KN‑004 Manage Improvement Portfolio` – for verified outcomes  
- External: Learn capabilities in all domains for practice nominations  

### 5.5.11 Collaborating Capabilities  

- **Govern Knowledge Graph** – publishes best practices as knowledge artifacts  
- **Orchestrate Feedback Loops** – monitors adoption effectiveness  
- **Serve Knowledge to AI Agents** – exposes practices for AI consumption  

### 5.5.12 Business Decisions  

---

#### DE‑KN‑050 — Nominate Best Practice  

**Purpose:** Evaluate a successful outcome or verified improvement for nomination as an enterprise best practice.  

**Required Understanding:** The practice, its evidence, its origin domain, its applicability conditions.  

**Decision Alternatives:** Nominate for validation, Reject (insufficient evidence, too narrow), Defer (more evidence needed).  

**Decision Criteria:** Practice must have demonstrated superior outcomes in at least two independent instances or planning cycles, with measurable improvement over the baseline.  

**Decision Rationale:** “Practice ‘Automated Forecast Bias Monitoring’ nominated as best practice: demonstrated 65% reduction in promise breaches across multiple product families over 3 cycles. Evidence from Demand domain, applicable to all forecast‑driven domains.”  

---

##### Rules (for DE‑KN‑050)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑050 | Evidence Threshold Rule | Validation Rule | A practice must have demonstrated effectiveness in at least two independent instances (planning cycles, product families, locations) with measurable improvement. |
| BR‑KN‑051 | Generalisability Rule | Validation Rule | A practice nominated as enterprise‑wide must be applicable to at least two domains. Domain‑specific practices are published within that domain only. |

##### Policies (for DE‑KN‑050)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑050 | Nomination Policy | Compliance Policy | Any domain manager or Knowledge Manager may nominate a practice. Nominations are reviewed within 15 business days. |

---

#### DE‑KN‑051 — Validate Best Practice  

**Purpose:** Rigorously assess the nominated practice for validity, generalisability, and readiness for enterprise adoption.  

**Required Understanding:** Nomination evidence, domain applicability, potential risks and limitations.  

**Decision Criteria:** Practice is effective across its claimed applicability range; risks of adoption are understood and acceptable; implementation guidance is complete.  

**Decision Rationale:** “Best practice BP‑003 validated: effective across all product families, applicable to Demand and Supply domains, implementation guidance complete.”  

---

##### Rules (for DE‑KN‑051)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑052 | Validation Completeness Rule | Validation Rule | Validation must include: effectiveness evidence, applicability boundaries, known limitations, implementation prerequisites, and a monitoring plan. |
| BR‑KN‑053 | Stakeholder Endorsement Rule | Validation Rule | At least one domain manager from each applicable domain must endorse the practice before enterprise publication. |

##### Policies (for DE‑KN‑051)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑051 | Validation Review Policy | Compliance Policy | Practice validation is completed within 30 days of nomination. |

---

#### DE‑KN‑052 — Publish as Enterprise Standard  

**Purpose:** Formally publish a validated best practice as an enterprise standard, making it available in the knowledge graph and notifying all applicable domains.  

**Decision Alternatives:** Publish, Publish with phased rollout, Defer.  

**Decision Rationale:** “BP‑003 published as enterprise standard: all Demand and Supply planning capabilities to reference this practice in their rules.”  

---

##### Rules (for DE‑KN‑052)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑054 | Publication Rule | Derivation Rule | A published best practice is added to the knowledge graph with edges to applicable capabilities, decisions, and rules. |

##### Policies (for DE‑KN‑052)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑052 | Adoption Mandate Policy | Authorization Policy | Once published, applicable domains must adopt the practice within 90 days or document a justified exception. |

---

### 5.5.13 Functional Behaviour  

1. **Trigger:** On verified improvement, on domain success nomination, on scheduled review.  
2. **Execute DE‑KN‑050** (Nominate) — rules BR‑KN‑050/051, policy PO‑KN‑050.  
3. **Execute DE‑KN‑051** (Validate) — rules BR‑KN‑052/053, policy PO‑KN‑051.  
4. **Execute DE‑KN‑052** (Publish) — rule BR‑KN‑054, policy PO‑KN‑052.  
5. **Monitor** adoption across domains.  
6. **Raise events:** `BestPracticeNominated`, `BestPracticeValidated`, `BestPracticePublished`.  

---

### 5.5.14 Commands  

| Command | Purpose |
|---------|---------|
| `NominateBestPractice` | Submit a practice for best‑practice evaluation, with evidence and provenance. |
| `ValidateBestPractice` | Run the full validation process (evidence review, applicability assessment, stakeholder endorsement). |
| `PublishBestPractice` | Publish a validated practice as an enterprise standard and propagate to applicable domains. |
| `ReviseBestPractice` | Update an existing practice based on new evidence or changed conditions (creates a new version). |
| `RetireBestPractice` | Mark a practice as no longer applicable and remove from active recommendations. |
| `MonitorAdoption` | Evaluate adoption compliance across domains and flag non‑adoption. |

### 5.5.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `BestPracticeNominated` | Practice ID, nominator, origin domain, evidence summary, timestamp |
| `BestPracticeValidated` | Practice ID, validation outcome, confidence, applicable domains, endorsing stakeholders |
| `BestPracticePublished` | Practice ID, version, applicable domains, implementation guidance, knowledge graph references |
| `BestPracticeRevised` | Practice ID, new version, change summary, reason for revision |
| `BestPracticeRetired` | Practice ID, retirement reason, superseding practice (if any) |
| `BestPracticeAdoptionAssessed` | Practice ID, domain adoption statuses, non‑adoption reasons |

### 5.5.16 Queries  

| Query | Description |
|-------|-------------|
| `GetBestPractice(practiceId)` | Full practice definition, evidence, applicability, and version history. |
| `GetBestPracticeCatalogue(filter)` | Active practices by domain, applicability, confidence, or keyword. |
| `GetPracticeProvenance(practiceId)` | Complete provenance chain: origin, validations, revisions, and evidence links. |
| `GetAdoptionStatus(practiceId)` | Per‑domain adoption status with compliance details. |
| `GetBestPracticeHistory(practiceId)` | Version history with change summaries. |

### 5.5.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑KN‑008 | Best‑Practice Catalogue Report | Institutionalise Best Practices | Knowledge Manager, Domain Managers | Quarterly | All active practices with applicability, adoption status, and evidence strength. |
| RPT‑KN‑009 | Adoption Compliance Report | Institutionalise Best Practices | Knowledge Manager, Domain Managers | Monthly | Per‑domain adoption rates, non‑adoption justifications, overdue adoptions. |
| RPT‑KN‑010 | Practice Effectiveness Review | Institutionalise Best Practices | Knowledge Manager | Annually | Review of each practice’s continued effectiveness, relevance, and any recommended revisions. |

### 5.5.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑KN‑007 | Best‑Practice Catalogue Dashboard | Institutionalise Best Practices | Knowledge Manager, Domain Managers | Daily | Practice inventory, confidence distribution, applicability map, adoption progress gauges. |
| DASH‑KN‑008 | Adoption Compliance Monitor | Institutionalise Best Practices | Knowledge Manager | Weekly | Per‑domain adoption status, overdue items, compliance trends. |

### 5.5.19 Software Realization  

```
API → Application Service → Domain Model (BestPractice, PracticeVersion, AdoptionRecord)  
→ Rule Engine (validation rules, generalisability checks, adoption compliance)  
→ Knowledge Graph Adapter (publish practice nodes and edges, link to capabilities/decisions/rules)  
→ Event Store → Projections (PracticeCatalogue, AdoptionView) → Read Model
```

The practice catalogue is version‑controlled; each revision creates a new immutable version while retaining history. Validation workflows include automated evidence strength scoring and stakeholder endorsement routing. Adoption monitoring compares expected vs. actual domain incorporation and flags non‑compliance per PO‑KN‑052. The knowledge graph adapter creates edges linking the practice to all applicable capabilities, decisions, and rules, making it discoverable by AI agents via the Serve Knowledge capability.

---

## 5.6 Orchestrate Feedback Loops  

### 5.6.1 Purpose  

Connect, monitor, and govern the complete feedback cycles that turn isolated learning events in individual domains into closed, verified enterprise‑wide improvements. Answers: *“Are we learning from what happened, and is that learning turning into action?”* The capability ensures that no significant learning event falls through the cracks—that every pattern discovery leads to root‑cause analysis, every root cause leads to corrective action, every corrective action leads to verified improvement, and every verified improvement feeds back into the enterprise memory and knowledge graph.  

### 5.6.2 Business Objectives Served  

- BO‑KN‑004 Orchestrate Enterprise‑Wide Improvement  
- BO‑KN‑006 Accelerate Cross‑Domain Learning  

### 5.6.3 Enterprise Measures  

- PI‑KN‑012 Feedback Loop Closure Rate  
- PI‑KN‑007 Cross‑Domain Learning Cycle Time  
- PI‑KN‑014 Planning Cycle Time (Knowledge)  

### 5.6.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑070 | Feedback Signal | Input trigger |
| SE‑KN‑071 | Feedback Target | Direction |
| SE‑KN‑072 | Feedback Loop | Managed cycle |
| SE‑KN‑073 | Loop Closure | Completion state |
| SE‑KN‑002 | Learning Event | Originating event |
| SE‑KN‑010 | Knowledge Artifact | Output knowledge |
| SE‑KN‑040 | Improvement Initiative | Action output |

### 5.6.5 Primitive Capabilities Composed  

- **Observe** – monitors feedback signals and loop states across all domains  
- **Understand** – interprets feedback signals and routes them to the correct capabilities  
- **Assess** – evaluates loop progress, bottlenecks, and closure readiness  
- **Learn** – improves feedback orchestration based on loop performance data  

### 5.6.6 Enterprise Inputs  

- Learning events from all operational domains (Demand, Supply, Promise, Scenario)  
- Pattern discoveries from Discover Cross‑Domain Patterns  
- Root‑cause analyses from Analyze Root Causes  
- Improvement initiative status from Manage Improvement Portfolio  
- Best‑practice publications from Institutionalise Best Practices  
- Loop state data: opened, in progress, stalled, closed  

### 5.6.7 Enterprise Understanding Produced  

- Complete map of all active, stalled, and closed feedback loops across the enterprise  
- Loop cycle time metrics: time from event to pattern, pattern to root cause, root cause to action, action to verification  
- Stalled loop identification with root cause of the stall (e.g., awaiting stakeholder review, insufficient data, resource contention)  
- Loop effectiveness: which types of loops produce the highest‑value improvements  

### 5.6.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑050 | Feedback Loop Map | All active, stalled, and closed loops with status and cycle times |
| OUT‑KN‑051 | Loop Cycle Time Analysis | Stage‑by‑stage timing for each loop |
| OUT‑KN‑052 | Stalled Loop Report | Loops that have not progressed, with blocking causes |
| OUT‑KN‑053 | Loop Closure Record | Completed loop with full traceability and outcome summary |

### 5.6.9 Preconditions  

- All domains publish learning events and outcome data  
- Feedback loop stages and transitions are defined  
- Loop ownership and SLAs are established  

### 5.6.10 Capability Dependencies  

- `CA‑KN‑002 Discover Cross‑Domain Patterns` – for pattern discovery stage  
- `CA‑KN‑003 Analyze Root Causes` – for root‑cause stage  
- `CA‑KN‑004 Manage Improvement Portfolio` – for action and verification stages  
- `CA‑KN‑005 Institutionalise Best Practices` – for institutionalisation stage  
- External: Learn capabilities in all domains for learning event intake  

### 5.6.11 Collaborating Capabilities  

- **All Knowledge Intelligence capabilities** – each handles a stage of the feedback loop  
- **Maintain Enterprise Memory** – records loop outcomes  
- **Evaluate Knowledge Quality** – consumes loop performance data for quality assessment  

### 5.6.12 Business Decisions  

---

#### DE‑KN‑060 — Open Feedback Loop  

**Purpose:** When a significant learning event occurs in any domain, determine whether it warrants opening a formal enterprise feedback loop and assign the appropriate scope.  

**Required Understanding:** The learning event (type, severity, domain, affected artifacts), historical patterns, existing open loops.  

**Decision Alternatives:**  
- Open new enterprise feedback loop (cross‑domain significance)  
- Route to domain‑level loop (single‑domain significance, handled by that domain’s Learn capability)  
- Log as informational (no loop required, below significance threshold)  
- Merge with existing open loop (related to an already‑active investigation)  

**Decision Criteria:** Enterprise loop if the event involves or affects ≥2 domains, or the estimated business impact exceeds a configurable threshold. Domain‑level loop if contained within one domain. Informational if impact is below threshold.  

**Decision Confidence:** Based on event severity classification and impact estimate.  

**Decision Rationale:** *Explainability Template:* “Enterprise feedback loop FL‑042 opened for learning event ‘Q3 Promise Breach Spike affecting Gold customers.’ Involves Demand (forecast bias) and Promise (breach). Estimated impact $1.2M. Routed to Discover Cross‑Domain Patterns for pattern detection. Rule BR‑KN‑060 applied.”  

---

##### Rules (for DE‑KN‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑060 | Loop Opening Rule | Derivation Rule | An enterprise feedback loop is opened when a learning event involves ≥2 domains or has an estimated business impact >$100K. Single‑domain events below the threshold are routed to that domain’s Learn capability. |
| BR‑KN‑061 | Duplicate Loop Rule | Validation Rule | If an open loop already exists for the same underlying pattern or root cause, the new event is merged into the existing loop rather than opening a duplicate. |
| BR‑KN‑062 | Event Classification Rule | Derivation Rule | Each learning event is classified at opening: Positive (success to replicate), Negative (failure to correct), or Neutral (observation to monitor). Classification determines loop priority. |

##### Policies (for DE‑KN‑060)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑060 | Loop Opening SLA Policy | Compliance Policy | Enterprise feedback loops must be opened within 24 hours of a qualifying learning event. Domain‑level loops within 48 hours. |
| PO‑KN‑061 | Loop Ownership Policy | Authorization Policy | Every opened loop is assigned an owner from the Knowledge Intelligence domain. The owner is responsible for tracking the loop through all stages to closure. |

---

#### DE‑KN‑061 — Monitor Loop Progress  

**Purpose:** Track the progress of every open feedback loop through its lifecycle stages, detect stalls, and trigger interventions when loops are not progressing.  

**Required Understanding:** Current loop state, stage transition timestamps, expected stage durations, blocking conditions.  

**Decision Alternatives:**  
- On track (progressing within expected timelines)  
- At risk (approaching stage deadline without completion)  
- Stalled (exceeded stage deadline, intervention required)  
- Ready for closure (all stages complete, outcome verified)  

**Decision Criteria:** Each loop stage has a defined expected duration. A loop is at risk when 80% of the expected duration has elapsed without stage completion. It is stalled when the expected duration is exceeded.  

**Decision Confidence:** Based on stage completion data and domain responsiveness history.  

**Decision Rationale:** “Loop FL‑042 at risk: Root‑cause analysis stage expected within 10 days, 8 days elapsed, analysis not yet submitted. Automated reminder sent to assigned analyst. Escalation to Knowledge Manager if not completed within 2 days. Rule BR‑KN‑063 applied.”  

---

##### Rules (for DE‑KN‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑063 | Stage Duration Rule | Derivation Rule | Expected stage durations: Pattern Detection 5 days, Root‑Cause Analysis 10 days, Action Recommendation 5 days, Implementation (domain‑dependent), Verification 2 planning cycles. |
| BR‑KN‑064 | Stall Detection Rule | Validation Rule | A loop is flagged as stalled when any stage exceeds its expected duration by 50%. Stalled loops are escalated to the loop owner and Knowledge Manager. |
| BR‑KN‑065 | Stage Transition Rule | Consistency Rule | A loop stage may only be marked complete when its defined exit criteria are met. For root‑cause analysis: published analysis with identified root cause. For verification: outcome data confirming ≥50% problem reduction. |

##### Policies (for DE‑KN‑064)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑062 | Stalled Loop Escalation Policy | Authorization Policy | Loops stalled for >30 days beyond their expected duration are escalated to the executive S&OP review with a recommendation to either resource the loop or close it with documented reasons. |
| PO‑KN‑063 | Loop Reprioritisation Policy | Compliance Policy | Loop priorities are reviewed monthly. Loops addressing Critical risks or with expected benefit >$1M are prioritised over lower‑impact loops. |

---

#### DE‑KN‑062 — Close Feedback Loop  

**Purpose:** Formally close a feedback loop when all stages are complete and the outcome has been verified, recording the complete loop in the enterprise memory.  

**Required Understanding:** Completed loop with all stage outputs, verification results, outcome metrics.  

**Decision Alternatives:**  
- Close as successful (problem resolved, improvement verified)  
- Close as partially successful (some benefit achieved, further monitoring)  
- Close as unsuccessful (no significant improvement, lessons documented)  
- Close as abandoned (loop no longer relevant, reason documented)  

**Decision Criteria:** All stages completed with documented outputs; verification data confirms outcome; lessons learned are captured.  

**Decision Rationale:** “Feedback loop FL‑042 closed as successful: Pattern P‑007 discovered (forecast bias → promise breaches), root cause RCA‑001 identified (forecast model parameter drift), corrective action IMP‑007 implemented (automated bias monitoring), outcome verified (65% reduction in attributable promise breaches). Full traceability recorded in enterprise memory. Rule BR‑KN‑066 applied.”  

---

##### Rules (for DE‑KN‑062)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑066 | Closure Criteria Rule | Validation Rule | A loop may be closed only when: (1) all stages have documented outputs, (2) verification data is available (for corrective loops), (3) lessons learned are captured, (4) the loop owner signs off. |
| BR‑KN‑067 | Lessons Learned Rule | Compliance Rule | Every closed loop must include a “lessons learned” summary: what worked, what didn’t, what would be done differently, and any residual risk. |

##### Policies (for DE‑KN‑062)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑064 | Closure Sign‑Off Policy | Authorization Policy | Successful and partially successful loops require sign‑off from the loop owner and at least one domain stakeholder from each affected domain. Unsuccessful and abandoned loops require Knowledge Manager sign‑off. |
| PO‑KN‑065 | Loop Archival Policy | Compliance Policy | Closed loops are archived in the enterprise memory with full traceability. They remain queryable indefinitely for future reference and pattern detection. |

---

### 5.6.13 Functional Behaviour  

1. **Continuous monitoring** of learning events from all domains.  
2. **Execute DE‑KN‑060** (Open Feedback Loop) for qualifying events — rules BR‑KN‑060/061/062, policies PO‑KN‑060/061.  
3. **Route** the loop through the appropriate Knowledge Intelligence capabilities: Discover Cross‑Domain Patterns → Analyze Root Causes → Manage Improvement Portfolio → Institutionalise Best Practices.  
4. **Execute DE‑KN‑061** (Monitor Loop Progress) continuously — rules BR‑KN‑063/064/065, policies PO‑KN‑062/063.  
5. **Execute DE‑KN‑062** (Close Feedback Loop) when all stages complete — rules BR‑KN‑066/067, policies PO‑KN‑064/065.  
6. **Record** closed loop in enterprise memory.  
7. **Raise events:** `FeedbackLoopOpened`, `FeedbackLoopStageCompleted`, `FeedbackLoopStalled`, `FeedbackLoopClosed`.  

### 5.6.14 Commands  

| Command | Purpose |
|---------|---------|
| `OpenFeedbackLoop` | Create a new enterprise feedback loop |
| `UpdateLoopStage` | Mark a stage as complete with outputs |
| `FlagStalledLoop` | Manually flag a loop as stalled |
| `CloseFeedbackLoop` | Finalise and archive a completed loop |

### 5.6.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `FeedbackLoopOpened` | Loop ID, triggering event, classification, owner |
| `FeedbackLoopStageCompleted` | Loop ID, stage, outputs, timestamp |
| `FeedbackLoopStalled` | Loop ID, stalled stage, reason, escalation target |
| `FeedbackLoopClosed` | Loop ID, closure status, lessons learned |

### 5.6.16 Queries  

| Query | Description |
|-------|-------------|
| `GetActiveLoops(filter)` | All open loops by status, priority, domain |
| `GetLoop(loopId)` | Full loop details with stage history |
| `GetLoopCycleTimeAnalysis(period)` | Stage‑by‑stage timing across all loops |
| `GetStalledLoops()` | Loops currently stalled with blocking causes |

### 5.6.17 Reports  

- **Feedback Loop Status Report** – all active loops with progress, bottlenecks, and cycle times  
- **Loop Effectiveness Report** – closure rates, success rates, average cycle times by type  

### 5.6.18 Dashboards  

- **Feedback Loop Control Tower** – real‑time loop map, stage progress, stalled loops highlighted  
- **Learning Cycle Dashboard** – cycle time trends, closure rates, bottleneck analysis  

### 5.6.19 Software Realization  

```
Event Bus (Learning Events from all domains) → Stream Processor (event classification)  
→ Domain Service (FeedbackLoop aggregate)  
→ Workflow Engine (stage routing, SLA monitoring, escalation)  
→ Event Store → Projections (LoopMap, CycleTimeAnalysis) → Read Model
```  
The workflow engine routes loops through Knowledge Intelligence capabilities based on event classification. SLA timers trigger automated reminders and escalations. All loop stages and transitions are immutably recorded.  

---

## 5.7 Maintain Enterprise Memory  

### 5.7.1 Purpose  

Capture, index, and serve the immutable, queryable record of significant enterprise events, decisions, outcomes, patterns, root causes, improvements, and lessons learned. Answers: *“What happened, what did we decide, what was the outcome, and what did we learn?”* The enterprise memory is the institutional knowledge that persists beyond individual planners, systems, or organisational changes—accessible to human decision‑makers and AI agents alike.  

### 5.7.2 Business Objectives Served  

- BO‑KN‑007 Maintain Enterprise Memory  

### 5.7.3 Enterprise Measures  

- PI‑KN‑008 Enterprise Memory Completeness  
- PI‑KN‑013 Knowledge Serving Latency  
- PI‑KN‑110 Knowledge Freshness Index  

### 5.7.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑080 | Enterprise Event | Recorded occurrence |
| SE‑KN‑081 | Outcome Record | Result data |
| SE‑KN‑082 | Decision Record | Decision context and choice |
| SE‑KN‑083 | Memory Query | Retrieval request |
| SE‑KN‑005 | Enterprise Memory | The system itself |
| SE‑KN‑015 | Knowledge Provenance | Traceability metadata |

### 5.7.5 Primitive Capabilities Composed  

- **Observe** – ingests events, decisions, and outcomes from all domains  
- **Understand** – indexes and organises records for efficient retrieval  
- **Assess** – evaluates memory completeness and freshness  

### 5.7.6 Enterprise Inputs  

- Significant events from all domains: plan adoptions, major promise breaches, forecast errors exceeding thresholds, supply disruptions, scenario recommendations, improvement implementations  
- Decision records with full context from all domains  
- Outcome records: actual vs. expected for all significant decisions  
- Closed feedback loops and lessons learned from Orchestrate Feedback Loops  
- Knowledge artifacts from all Knowledge Intelligence capabilities  

### 5.7.7 Enterprise Understanding Produced  

- A complete, indexed, queryable enterprise memory  
- Memory completeness metrics: percentage of significant events recorded  
- Memory freshness: time from event occurrence to memory recording  
- Query response: ability to answer “Has this situation occurred before, and what was the outcome?”  

### 5.7.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑060 | Enterprise Memory Record | An immutable record of an event, decision, or outcome |
| OUT‑KN‑061 | Memory Query Response | Structured response to a memory query with relevant historical records |
| OUT‑KN‑062 | Memory Completeness Report | Assessment of coverage and gaps |
| OUT‑KN‑063 | Memory Index | Searchable index of all records by domain, type, time, and artifact |

### 5.7.9 Preconditions  

- All domains publish significant events, decisions, and outcomes in a structured format  
- Memory recording criteria define which events are “significant”  
- Indexing schema is defined  

### 5.7.10 Capability Dependencies  

- `CA‑KN‑006 Orchestrate Feedback Loops` – for closed loop records  
- External: All domains for events, decisions, and outcomes  

### 5.7.11 Collaborating Capabilities  

- **Serve Knowledge to AI Agents** – provides the memory for AI queries  
- **Discover Cross‑Domain Patterns** – uses memory for historical pattern detection  
- **Evaluate Knowledge Quality** – assesses memory completeness  

### 5.7.12 Business Decisions  

---

#### DE‑KN‑070 — Record Significant Event  

**Purpose:** Determine whether an event from any domain is significant enough to warrant recording in the enterprise memory, and capture it with full context.  

**Required Understanding:** The event (type, domain, artifacts involved, impact), recording criteria, existing memory.  

**Decision Alternatives:**  
- Record with full context (significant event meeting criteria)  
- Record as summary (minor event, brief entry)  
- Do not record (below significance threshold)  
- Merge with related record (part of an already‑recorded sequence)  

**Decision Criteria:** Significance thresholds: plan adoptions (all), promise breaches affecting Gold/Platinum customers, forecast errors >20% WAPE, supply disruptions with impact >$100K, scenario recommendations adopted, improvement implementations, cross‑domain patterns validated, root‑cause analyses published, feedback loops closed.  

**Decision Rationale:** “Event EV‑2026‑07845 recorded: Supply disruption—Supplier S5 OTD dropped to 72%, impact $180K. Linked to promise breach event EV‑2026‑07850. Full context captured including snapshot references, affected orders, and initial response. Rule BR‑KN‑070 applied.”  

---

##### Rules (for DE‑KN‑070)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑070 | Significance Threshold Rule | Derivation Rule | Events meeting any significance criterion are recorded with full context. Criteria are defined per domain and reviewed annually. |
| BR‑KN‑071 | Context Capture Rule | Validation Rule | Every recorded event must include: timestamp, domain, event type, affected artifacts (with identifiers), estimated or actual impact, and links to related events. |
| BR‑KN‑072 | Deduplication Rule | Validation Rule | An event that is already recorded (same domain, same artifacts, same time window, same nature) is merged rather than duplicated. |

##### Policies (for DE‑KN‑070)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑070 | Recording Timeliness Policy | Compliance Policy | Significant events must be recorded in the enterprise memory within 1 hour of detection for operational events and 24 hours for strategic events. |
| PO‑KN‑071 | Retention Policy | Compliance Policy | Enterprise memory records are retained indefinitely. Records may be archived to cold storage after 7 years but remain queryable. |

---

#### DE‑KN‑071 — Record Decision with Context  

**Purpose:** Capture an enterprise decision in the memory with its full decision context—what was known at the time, the alternatives considered, the rationale, and the expected outcome.  

**Required Understanding:** The decision, decision context (plans, policies, constraints in effect), alternatives, rationale, expected outcome.  

**Decision Criteria:** All strategic decisions and operational decisions with impact >$100K are recorded.  

**Decision Rationale:** “Decision DEC‑2026‑0312 recorded: ‘Adopt Flex Capacity plan variant for Q3.’ Context: demand forecast v4.2, supply plan v7.1, policy set PS‑Q3‑001. Alternatives considered: Max Service (rejected—failed stress test), Baseline (rejected—lower robustness). Rationale: highest robustness score (82%) while satisfying risk appetite. Expected outcome: service level 95%, cost $5.3M.”  

---

##### Rules (for DE‑KN‑071)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑073 | Decision Recording Rule | Derivation Rule | All strategic decisions (adoptions, major investments) and operational decisions with impact >$100K must be recorded. |
| BR‑KN‑074 | Context Completeness Rule | Validation Rule | Decision records must include: the decision context (what was known), alternatives considered, rationale, the decision made, expected outcome, and the decision‑maker. |

##### Policies (for DE‑KN‑071)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑072 | Decision Recording Policy | Compliance Policy | Decisions are recorded within 24 hours of being made. Decisions not recorded within 7 days are escalated to the Knowledge Manager. |

---

#### DE‑KN‑072 — Respond to Memory Query  

**Purpose:** Process a query to the enterprise memory—typically “Has this situation occurred before?”—and return relevant historical records with context and outcomes.  

**Required Understanding:** Query parameters (domain, artifacts, time range, situation description), memory index, historical records.  

**Decision Alternatives:** Return matched records ranked by relevance, Return “no match found”, Request clarification (ambiguous query).  

**Decision Criteria:** Match on domain, artifact type, event type, impact similarity, and temporal proximity. Rank by relevance score.  

**Decision Confidence:** Based on match quality and completeness of memory.  

**Decision Rationale:** “Memory query MQ‑0892: ‘Similar situations to current: forecast bias >10% in Demand for top 20 SKUs.’ 3 matching events found: EV‑2025‑0345 (Q2 2025, similar bias, led to promise breaches), EV‑2025‑0678 (Q4 2025, resolved by model retuning), EV‑2026‑0123 (Q1 2026, minor, self‑corrected). Top match: EV‑2025‑0345 (relevance 94%). Recommended action: review corrective action from EV‑2025‑0678.”  

---

##### Rules (for DE‑KN‑072)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑075 | Relevance Ranking Rule | Derivation Rule | Memory query results are ranked by: domain match (exact = highest), artifact similarity, impact magnitude similarity, temporal recency, and outcome availability. |
| BR‑KN‑076 | Response Completeness Rule | Validation Rule | Every query response must include: matched records with relevance scores, a summary of outcomes for the top matches, and a confidence score for the match. |

##### Policies (for DE‑KN‑072)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑073 | Query Response SLA Policy | Compliance Policy | Memory queries from AI agents must be responded to within 500 ms. Queries from human users within 5 seconds. Complex queries with notification. |

---

### 5.7.13 Functional Behaviour  

1. **Continuous ingestion** of events, decisions, and outcomes from all domains via event streams.  
2. **Execute DE‑KN‑070** (Record Significant Event) for each incoming event — rules BR‑KN‑070/071/072, policies PO‑KN‑070/071.  
3. **Execute DE‑KN‑071** (Record Decision with Context) for each significant decision — rules BR‑KN‑073/074, policy PO‑KN‑072.  
4. **Index** all records for efficient querying.  
5. **Execute DE‑KN‑072** (Respond to Memory Query) on demand — rules BR‑KN‑075/076, policy PO‑KN‑073.  
6. **Raise events:** `EventRecorded`, `DecisionRecorded`, `MemoryQueryResponded`.  

### 5.7.14 Commands  

| Command | Purpose |
|---------|---------|
| `RecordEvent` | Capture a significant event in memory |
| `RecordDecision` | Capture a decision with full context |
| `QueryMemory` | Submit a memory query |
| `AssessMemoryCompleteness` | Evaluate coverage and identify gaps |

### 5.7.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `EventRecorded` | Event ID, domain, type, artifacts, impact, timestamp |
| `DecisionRecorded` | Decision ID, context, alternatives, rationale, expected outcome |
| `MemoryQueryResponded` | Query ID, matched records, relevance scores, confidence |

### 5.7.16 Queries  

| Query | Description |
|-------|-------------|
| `QueryMemory(params)` | General memory query with filters |
| `GetEvent(eventId)` | Full event record |
| `GetDecision(decisionId)` | Full decision record with context |
| `GetMemoryCompleteness()` | Coverage assessment |

### 5.7.17 Reports  

- **Enterprise Memory Completeness Report** – coverage by domain, event type, and period  
- **Memory Query Analytics** – query volume, response times, match rates  

### 5.7.18 Dashboards  

- **Enterprise Memory Explorer** – interactive search and browse of recorded events and decisions  
- **Memory Health Dashboard** – completeness gauges, freshness indicators, query performance  

### 5.7.19 Software Realization  

```
Event Bus (all domain events) → Ingestion Service (significance filter)  
→ Domain Service (EnterpriseMemory aggregate)  
→ Indexing Engine (full‑text and graph‑based indexing)  
→ Query Engine (relevance ranking, similarity matching)  
→ Event Store (immutable record store) → Read Model (MemoryIndex)
```  
The memory store is append‑only and immutable. Indexing supports full‑text search, graph traversal, and similarity matching. The query engine is optimised for sub‑500ms responses for AI agent queries.

---

## 5.8 Serve Knowledge to AI Agents  

### 5.8.1 Purpose  

Provide a governed, real‑time interface for AI agents—planning agents, promising agents, scenario agents, and conversational assistants—to query the enterprise knowledge graph, enterprise memory, best‑practice catalogue, and validated patterns. Answers the AI agent’s implicit question: *“What does the enterprise know that is relevant to my current decision?”* The capability ensures that AI agents operate with the full context of enterprise knowledge while respecting governance boundaries.  

### 5.8.2 Business Objectives Served  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  
- BO‑KN‑007 Maintain Enterprise Memory  

### 5.8.3 Enterprise Measures  

- PI‑KN‑013 Knowledge Serving Latency  
- PI‑KN‑110 Knowledge Freshness Index  

### 5.8.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑030 | Knowledge Node | Served knowledge element |
| SE‑KN‑031 | Knowledge Edge | Served relationship |
| SE‑KN‑083 | Memory Query | Input request |
| SE‑KN‑060 | Best Practice | Served practice |
| SE‑KN‑003 | Cross‑Domain Pattern | Served pattern |
| SE‑KN‑010 | Knowledge Artifact | Served artifact |
| SE‑KN‑015 | Knowledge Provenance | Traceability for served knowledge |

### 5.8.5 Primitive Capabilities Composed  

- **Observe** – listens for knowledge requests from AI agents  
- **Understand** – interprets the agent’s context and information need  
- **Assess** – evaluates relevance and confidence of served knowledge  

### 5.8.6 Enterprise Inputs  

- Knowledge requests from AI agents: context (current decision, domain, artifacts), query type, required confidence level  
- The enterprise knowledge graph (from Govern Knowledge Graph)  
- Enterprise memory (from Maintain Enterprise Memory)  
- Best‑practice catalogue (from Institutionalise Best Practices)  
- Validated patterns and root‑cause analyses (from Discover Cross‑Domain Patterns, Analyze Root Causes)  

### 5.8.7 Enterprise Understanding Produced  

- Structured knowledge responses tailored to the AI agent’s context  
- Relevance‑ranked historical precedents, best practices, and patterns  
- Confidence scores and provenance for every piece of served knowledge  
- Traceability enabling the AI agent to explain its reasoning  

### 5.8.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑070 | Knowledge Response | Structured response with relevant knowledge artifacts |
| OUT‑KN‑071 | Precedent Summary | Historical similar situations with outcomes |
| OUT‑KN‑072 | Best‑Practice Recommendation | Applicable best practices for the current context |
| OUT‑KN‑073 | Pattern Alert | Relevant cross‑domain patterns that may affect the decision |

### 5.8.9 Preconditions  

- The knowledge graph, enterprise memory, and best‑practice catalogue are populated and current  
- AI agents are authenticated and authorised for knowledge access  
- Query interface contracts are defined  

### 5.8.10 Capability Dependencies  

- `CA‑KN‑001 Govern Knowledge Graph` – for knowledge graph data  
- `CA‑KN‑007 Maintain Enterprise Memory` – for historical records  
- `CA‑KN‑005 Institutionalise Best Practices` – for best‑practice catalogue  
- `CA‑KN‑002 Discover Cross‑Domain Patterns` – for validated patterns  

### 5.8.11 Collaborating Capabilities  

- **All AI agent capabilities** across Demand, Supply, Promise, and Scenario domains – consume knowledge responses  

### 5.8.12 Business Decisions  

---

#### DE‑KN‑080 — Process Knowledge Request  

**Purpose:** Interpret an AI agent’s knowledge request, identify the type of knowledge needed, and route to the appropriate knowledge source.  

**Required Understanding:** Agent context (domain, decision type, artifacts involved), query type, required confidence level, urgency.  

**Decision Alternatives:**  
- Route to enterprise memory (precedent query: “Has this happened before?”)  
- Route to best‑practice catalogue (guidance query: “What is the best way to do this?”)  
- Route to knowledge graph (relationship query: “How are these concepts related?”)  
- Route to pattern catalogue (risk query: “What patterns might affect this?”)  
- Composite response (multi‑faceted query)  

**Decision Criteria:** Query type classification based on agent’s expressed intent and decision context.  

**Decision Confidence:** Based on query clarity and knowledge source completeness.  

**Decision Rationale:** “Knowledge request KR‑0892 from Supply Planning Agent: ‘Evaluating capacity allocation for Product Family PF3. Are there any known patterns or historical issues with this family?’ Routed to pattern catalogue (cross‑domain patterns involving PF3) and enterprise memory (past PF3 capacity decisions and outcomes). Composite response generated.”  

---

##### Rules (for DE‑KN‑080)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑080 | Query Classification Rule | Derivation Rule | Knowledge requests are classified by intent: Precedent, Guidance, Relationship, Risk, or Composite. Classification determines routing. |
| BR‑KN‑081 | Minimum Confidence Rule | Validation Rule | Served knowledge must meet the agent’s required confidence level. If no knowledge meets the threshold, the response explicitly states “No knowledge meeting confidence threshold available.” |

##### Policies (for DE‑KN‑080)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑080 | Agent Authentication Policy | Authorization Policy | AI agents must authenticate and be authorised for the knowledge domains they query. Agents may only access knowledge within their authorised domains. |
| PO‑KN‑081 | Rate Limiting Policy | Compliance Policy | AI agent knowledge requests are rate‑limited per agent to ensure system stability. Burst limits and sustained throughput limits are configurable. |

---

#### DE‑KN‑081 — Assemble Knowledge Response  

**Purpose:** Compile the results from the relevant knowledge sources into a structured, provenance‑tagged response suitable for AI agent consumption.  

**Required Understanding:** Raw results from knowledge sources, agent context, required response format.  

**Decision Alternatives:** Deterministic assembly with relevance ranking.  

**Decision Criteria:** Response must include: the knowledge artifacts, their confidence scores, their provenance (traceability to evidence), and a relevance ranking.  

**Decision Confidence:** Aggregate of individual artifact confidences.  

**Decision Rationale:** “Knowledge response KR‑0892 assembled: (1) Pattern P‑007 (relevance 94%): forecast bias in PF3 correlates with promise breaches. (2) Memory EV‑2025‑0345 (relevance 89%): similar capacity allocation in Q2 2025 led to 12% under‑delivery. (3) Best Practice BP‑003 (relevance 78%): automated forecast bias monitoring reduces downstream impact. All artifacts have confidence ≥85%, provenance attached.”  

---

##### Rules (for DE‑KN‑081)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑082 | Provenance Attachment Rule | Validation Rule | Every piece of knowledge served to an AI agent must include its provenance: origin, evidence, validation status, and confidence score. |
| BR‑KN‑083 | Relevance Threshold Rule | Derivation Rule | Only knowledge artifacts with relevance score ≥70% are included in the response. Lower‑relevance artifacts are available on explicit request. |

##### Policies (for DE‑KN‑081)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑082 | Response Format Policy | Compliance Policy | Knowledge responses are structured in a machine‑readable format (JSON/JSON‑LD) with a standard schema enabling AI agents to parse and integrate knowledge directly into their reasoning. |

---

### 5.8.13 Functional Behaviour  

1. **Event‑driven:** On knowledge request from an AI agent.  
2. **Authenticate** the agent and validate authorisation.  
3. **Execute DE‑KN‑080** (Process Knowledge Request) — rules BR‑KN‑080/081, policies PO‑KN‑080/081.  
4. **Query** the relevant knowledge sources.  
5. **Execute DE‑KN‑081** (Assemble Knowledge Response) — rules BR‑KN‑082/083, policy PO‑KN‑082.  
6. **Return** structured response to the agent.  
7. **Log** the request and response for audit and quality evaluation.  
8. **Raise events:** `KnowledgeRequestReceived`, `KnowledgeResponseServed`.  

### 5.8.14 Commands  

| Command | Purpose |
|---------|---------|
| `RequestKnowledge` | Submit a knowledge request from an AI agent |
| `AssembleResponse` | Compile results into a structured response |

### 5.8.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `KnowledgeRequestReceived` | Request ID, agent ID, domain, query type, timestamp |
| `KnowledgeResponseServed` | Request ID, artifact count, confidence range, response time |

### 5.8.16 Queries  

| Query | Description |
|-------|-------------|
| `GetKnowledgeResponse(requestId)` | Retrieve the response for a past request |
| `GetAgentKnowledgeUsage(agentId, period)` | Knowledge consumption analytics per agent |

### 5.8.17 Reports  

- **AI Agent Knowledge Usage Report** – request volume, response times, knowledge types consumed  
- **Knowledge Serving Performance Report** – latency, throughput, confidence distributions  

### 5.8.18 Dashboards  

- **AI Knowledge Serving Monitor** – real‑time request volume, response times, error rates  
- **Knowledge Usage Analytics Dashboard** – most‑queried patterns, practices, and precedents  

### 5.8.19 Software Realization  

```
API Gateway (authenticated AI agent endpoint)  
→ Knowledge Request Handler (classification, routing)  
→ Knowledge Source Adapters (graph query, memory query, pattern query, practice query)  
→ Response Assembler (relevance ranking, provenance attachment, formatting)  
→ Event Store (request/response log) → Read Model (usage analytics)
```  
The service is optimised for low‑latency responses (target <500ms). It caches frequently‑accessed knowledge artifacts. All responses carry full provenance enabling the AI agent to cite its sources in explanations.  

---

## 5.9 Evaluate Knowledge Quality  

### 5.9.1 Purpose  

Continuously measure and assess the quality, accuracy, completeness, and value of all Knowledge Intelligence outputs—patterns, root‑cause analyses, improvement recommendations, best practices, enterprise memory, and knowledge graph governance. Answers: *“How good is our enterprise knowledge, and is it getting better?”* The capability is the analytical engine for Knowledge Intelligence’s own performance, ensuring that the meta‑domain holds itself to the same rigorous standards it demands of all other domains.  

### 5.9.2 Business Objectives Served  

- BO‑KN‑008 Continuously Improve Knowledge Intelligence  
- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  

### 5.9.3 Enterprise Measures  

- PI‑KN‑002 Cross‑Domain Pattern Discovery Rate (computed)  
- PI‑KN‑003 Root‑Cause Identification Accuracy (computed)  
- PI‑KN‑004 Improvement Portfolio ROI (computed)  
- PI‑KN‑005 Best‑Practice Institutionalisation Rate (computed)  
- PI‑KN‑006 Knowledge Graph Consistency Score (computed)  
- PI‑KN‑007 Cross‑Domain Learning Cycle Time (computed)  
- PI‑KN‑008 Enterprise Memory Completeness (computed)  
- PI‑KN‑012 Feedback Loop Closure Rate (computed)  
- PI‑KN‑101 Enterprise Understanding Index (Cross‑Domain)  
- PI‑KN‑104 Recommendation Quality Index (Knowledge)  
- PI‑KN‑105 Explainability Score (Knowledge)  

### 5.9.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑003 | Cross‑Domain Pattern | Evaluated pattern |
| SE‑KN‑053 | Root‑Cause Analysis | Evaluated analysis |
| SE‑KN‑040 | Improvement Initiative | Evaluated improvement |
| SE‑KN‑060 | Best Practice | Evaluated practice |
| SE‑KN‑010 | Knowledge Artifact | Evaluated artifact |
| SE‑KN‑030 | Knowledge Node | Evaluated graph element |
| SE‑KN‑072 | Feedback Loop | Evaluated loop |
| SE‑KN‑005 | Enterprise Memory | Evaluated memory |
| SE‑KN‑011 | Knowledge Confidence | Evaluated confidence |
| SE‑KN‑015 | Knowledge Provenance | Traceability for quality data |

### 5.9.5 Primitive Capabilities Composed  

- **Observe** – collects quality metrics from all Knowledge Intelligence capabilities  
- **Understand** – interprets quality trends and identifies root causes of quality issues  
- **Assess** – computes quality scores and compares against targets  
- **Evaluate** – evaluates overall knowledge health and improvement trajectories  

### 5.9.6 Enterprise Inputs  

- Pattern discovery records with validation outcomes (from Discover Cross‑Domain Patterns)  
- Root‑cause analysis records with verification outcomes (from Analyze Root Causes)  
- Improvement portfolio data with ROI and verification (from Manage Improvement Portfolio)  
- Best‑practice catalogue with adoption status (from Institutionalise Best Practices)  
- Knowledge graph consistency reports (from Govern Knowledge Graph)  
- Feedback loop records with cycle times and closure status (from Orchestrate Feedback Loops)  
- Enterprise memory completeness assessments (from Maintain Enterprise Memory)  
- Knowledge serving logs (from Serve Knowledge to AI Agents)  

### 5.9.7 Enterprise Understanding Produced  

- Aggregate knowledge quality scores across all dimensions  
- Trend analysis: which aspects of knowledge are improving or degrading  
- Quality gap identification: where Knowledge Intelligence is falling short of its targets  
- Improvement recommendations for Knowledge Intelligence itself  

### 5.9.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑080 | Knowledge Quality Report | Consolidated quality metrics across all Knowledge Intelligence capabilities |
| OUT‑KN‑081 | Quality Trend Analysis | Direction and significance of quality changes over time |
| OUT‑KN‑082 | Quality Gap Report | Areas where quality metrics fall below target, with root causes |
| OUT‑KN‑083 | Knowledge Improvement Recommendations | Specific recommendations to improve Knowledge Intelligence quality |

### 5.9.9 Preconditions  

- All Knowledge Intelligence capabilities publish quality‑relevant data and outcomes  
- Quality targets and thresholds are defined for each metric  
- Sufficient historical data exists for trend analysis (minimum 3 evaluation periods)  

### 5.9.10 Capability Dependencies  

- All Knowledge Intelligence capabilities (5.1–5.8) for quality data  

### 5.9.11 Collaborating Capabilities  

- **Learn From Knowledge** – consumes quality gaps for improvement  
- **Explain Knowledge Insights** – consumes quality assessments for explanation  

### 5.9.12 Business Decisions  

---

#### DE‑KN‑090 — Compute Knowledge Quality Metrics  

**Purpose:** Calculate the standard set of quality metrics for all Knowledge Intelligence outputs across a defined evaluation period.  

**Required Understanding:** Input data from all Knowledge capabilities, metric formulas, quality targets.  

**Decision Alternatives:** Deterministic computation.  

**Decision Criteria:** Apply formulas defined in Chapter 3. Flag any metric where data completeness is below threshold.  

**Decision Confidence:** Based on data completeness and source reliability.  

**Decision Rationale:** “Q3 Knowledge Quality metrics computed: Pattern Discovery Rate 4 (Acceptable), Root‑Cause Accuracy 75% (Good), Improvement Portfolio ROI 1.56 (Positive), Knowledge Graph Consistency 98.9% (Excellent), Feedback Loop Closure Rate 82% (Good). All metrics within acceptable bounds. Data completeness 94%.”  

---

##### Rules (for DE‑KN‑090)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑090 | Metric Calculation Standard Rule | Calculation Rule | All knowledge quality metrics shall be calculated per Chapter 3 formulas. Any deviation requires documented justification. |
| BR‑KN‑091 | Data Completeness Rule | Validation Rule | If input data for any metric is less than 90% complete for the evaluation period, that metric is flagged as “low confidence” and the data gap is reported. |

##### Policies (for DE‑KN‑090)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑090 | Evaluation Frequency Policy | Compliance Policy | Knowledge quality metrics are computed monthly (operational) and quarterly (strategic). Quarterly evaluations are presented to the Knowledge Manager and executive S&OP review. |

---

#### DE‑KN‑091 — Assess Knowledge Quality Trends  

**Purpose:** Analyse quality metric trends over time to detect improvement, degradation, or stability in Knowledge Intelligence performance.  

**Required Understanding:** Historical quality metrics (minimum 3 periods), statistical trend tests.  

**Decision Alternatives:**  
- Improving (positive trend, statistically significant)  
- Stable (no significant trend)  
- Degrading (negative trend, statistically significant)  
- Insufficient data for trend analysis  

**Decision Criteria:** Trend is assessed using a Mann‑Kendall trend test at p < 0.05 with a minimum of 4 data points.  

**Decision Confidence:** Based on data volume and trend significance.  

**Decision Rationale:** “Knowledge quality trend analysis for Q3: Root‑Cause Accuracy improving (τ = +0.67, p = 0.03, over 4 quarters from 62% to 75%). Feedback Loop Closure Rate stable. Pattern Discovery Rate stable. No significant degradation detected.”  

---

##### Rules (for DE‑KN‑091)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑092 | Trend Detection Rule | Derivation Rule | A trend is identified using the Mann‑Kendall test at p < 0.05 with a minimum of 4 data points. Trend direction and magnitude are reported. |
| BR‑KN‑093 | Degradation Alert Rule | Validation Rule | Any metric showing a statistically significant negative trend for two consecutive periods triggers a quality degradation alert and is escalated to the Knowledge Manager. |

##### Policies (for DE‑KN‑091)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑091 | Trend Review Policy | Compliance Policy | Quality trends are reviewed quarterly. Degrading trends must be addressed with an improvement plan within 30 days of identification. |

---

#### DE‑KN‑092 — Publish Knowledge Quality Report  

**Purpose:** Compile the knowledge quality metrics, trend analysis, and gap assessment into a formal report for stakeholders.  

**Required Understanding:** Quality metrics, trends, gaps, improvement recommendations.  

**Decision Alternatives:** Publish, Publish with flags, Hold.  

**Decision Criteria:** Report must include all mandatory metrics, trend analysis, and identified gaps. Data completeness must be ≥85%.  

**Decision Rationale:** “Q3 Knowledge Quality Report published: overall knowledge health Good. 2 metrics improving, 4 stable, 0 degrading. 1 gap identified: Feedback Loop Closure Rate below 90% target. Recommendation: investigate stalled loops in Orchestrate Feedback Loops.”  

---

##### Rules (for DE‑KN‑092)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑094 | Report Completeness Rule | Validation Rule | The quality report must include: all metrics per Chapter 3, trend analysis for each metric, comparison against targets, identified gaps, and improvement recommendations. |

##### Policies (for DE‑KN‑092)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑092 | Report Distribution Policy | Compliance Policy | The Knowledge Quality Report is published within 10 business days of period end and distributed to the Knowledge Manager, Domain Managers, and executive S&OP review. |

---

### 5.9.13 Functional Behaviour  

1. **Scheduled:** Monthly and quarterly, aligned with evaluation periods.  
2. **Retrieve** quality data from all Knowledge Intelligence capabilities.  
3. **Execute DE‑KN‑090** (Compute Knowledge Quality Metrics) — rules BR‑KN‑090/091, policy PO‑KN‑090.  
4. **Execute DE‑KN‑091** (Assess Knowledge Quality Trends) — rules BR‑KN‑092/093, policy PO‑KN‑091.  
5. **Execute DE‑KN‑092** (Publish Knowledge Quality Report) — rule BR‑KN‑094, policy PO‑KN‑092.  
6. **Raise events:** `KnowledgeQualityComputed`, `KnowledgeQualityTrendAssessed`, `KnowledgeQualityReportPublished`.  

### 5.9.14 Commands  

| Command | Purpose |
|---------|---------|
| `ComputeKnowledgeQuality` | Calculate all quality metrics for a given period |
| `AssessQualityTrends` | Run trend analysis on historical metrics |
| `PublishQualityReport` | Compile and release the Knowledge Quality Report |

### 5.9.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `KnowledgeQualityComputed` | Period, metrics with values and confidence |
| `KnowledgeQualityTrendAssessed` | Period, trend direction per metric, significance |
| `KnowledgeQualityReportPublished` | Report ID, period, overall health score |

### 5.9.16 Queries  

| Query | Description |
|-------|-------------|
| `GetKnowledgeQualityMetrics(period)` | All quality metrics for a period |
| `GetQualityTrends(metric, periods)` | Trend data for a specific metric |
| `GetQualityReport(period)` | Full quality report |

### 5.9.17 Reports  

- **Knowledge Quality Report** – all metrics, trends, gaps, and recommendations  

### 5.9.18 Dashboards  

- **Knowledge Quality Dashboard** – metric gauges, trend charts, gap indicators  
- **Knowledge Health Scorecard** – balanced scorecard of all Knowledge Intelligence KPIs  

### 5.9.19 Software Realization  

```
Scheduled Trigger → Analytics Engine (metric computation, trend analysis)  
→ Domain Service (KnowledgeQuality aggregate)  
→ Event Store → Projections (QualityView, TrendView) → Read Model
```  
All metric formulas are implemented as pure functions, verifiable against Chapter 3. Trend analysis uses standard statistical libraries. The quality report is auto‑generated with configurable thresholds for alerts.  

---

## 5.10 Explain Knowledge Insights  

### 5.10.1 Purpose  

Generate clear, traceable, and auditable explanations for every cross‑domain pattern, root‑cause analysis, improvement recommendation, best‑practice publication, feedback loop outcome, and knowledge graph governance decision. Answers: *“Why did Knowledge Intelligence reach this conclusion, and what is the evidence?”* The capability ensures that the meta‑domain’s own reasoning is as explainable as it demands every other domain to be.  

### 5.10.2 Business Objectives Served  

- BO‑KN‑001 Deliver Trusted Cross‑Domain Intelligence  
- BO‑KN‑008 Continuously Improve Knowledge Intelligence  

### 5.10.3 Enterprise Measures  

- PI‑KN‑105 Explainability Score (Knowledge)  

### 5.10.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑003 | Cross‑Domain Pattern | Subject of explanation |
| SE‑KN‑053 | Root‑Cause Analysis | Subject of explanation |
| SE‑KN‑040 | Improvement Initiative | Subject of explanation |
| SE‑KN‑060 | Best Practice | Subject of explanation |
| SE‑KN‑010 | Knowledge Artifact | Subject of explanation |
| SE‑KN‑014 | Knowledge Evidence | Supporting evidence |
| SE‑KN‑015 | Knowledge Provenance | Traceability chain |
| SE‑KN‑024 | Causal Chain | Causal explanation |
| SE‑KN‑082 | Decision Record | Decision context |

### 5.10.5 Primitive Capabilities Composed  

- **Understand** – interprets knowledge artifacts, evidence chains, and decision contexts  

### 5.10.6 Enterprise Inputs  

- Knowledge artifacts from all Knowledge Intelligence capabilities (patterns, analyses, recommendations, practices, loop closures, governance decisions)  
- Evidence records and provenance data associated with each artifact  
- The enterprise knowledge graph for relationship context  
- Historical decision records and outcomes  

### 5.10.7 Enterprise Understanding Produced  

- Structured explanation objects containing natural language and machine‑readable traceability  
- Explanation quality scores  
- Traceability chains linking every insight back to its underlying evidence and decisions  

### 5.10.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑090 | Knowledge Explanation | Structured explanation with traceability chain and evidence summary |

### 5.10.9 Preconditions  

- Knowledge artifacts are published with complete provenance and evidence  
- Traceability links are populated in the knowledge graph  

### 5.10.10 Capability Dependencies  

- All Knowledge Intelligence capabilities for their decision logs and artifacts  
- `CA‑KN‑001 Govern Knowledge Graph` – for traceability relationships  

### 5.10.11 Collaborating Capabilities  

- **Serve Knowledge to AI Agents** – embeds explanations in knowledge responses  
- **Learn From Knowledge** – consumes explanations for improvement analysis  

### 5.10.12 Business Decisions  

---

#### DE‑KN‑100 — Generate Knowledge Explanation  

**Purpose:** Produce a human‑ and machine‑readable explanation for any knowledge artifact produced by Knowledge Intelligence.  

**Required Understanding:** The artifact (pattern, root cause, recommendation, practice, loop outcome, governance decision), its provenance, the evidence supporting it, the causal chain or reasoning path, and the decisions that led to it.  

**Decision Alternatives:** Deterministic generation based on available evidence and provenance. If critical evidence is missing, the explanation notes this limitation.  

**Decision Criteria:** Every explanation must include: (1) what the artifact is and what it concludes, (2) the evidence supporting it, (3) the causal chain or reasoning path, (4) the confidence level and its basis, (5) the traceability chain to underlying data and decisions, (6) any limitations or caveats.  

**Decision Confidence:** Based on the completeness and quality of the underlying evidence and provenance.  

**Decision Rationale:** *Explainability Template:* “Pattern P‑007 was discovered by correlating Demand forecast bias for Product Family PF3 with Promise On‑Time Delivery across 8 planning cycles (Q1–Q4 2026). A strong negative correlation (r = −0.78, p < 0.01) was detected, indicating that higher forecast overestimation is associated with lower on‑time delivery. The pattern was validated on hold‑out data (r = −0.74) and confirmed by both Demand and Promise domain managers. Root‑cause analysis RCA‑001 traced the pattern to a parameter drift in the PF3 forecast model that went undetected for 3 cycles. Evidence sources: forecast accuracy reports from Evaluate Demand Quality, promise breach records from Evaluate Promise Quality, and BOM/capacity data from Supply Intelligence. Confidence: 91%. Limitations: pattern may not generalise to product families with stable demand patterns. Full traceability: Learning Event EV‑2026‑0123 → Pattern P‑007 → Root‑Cause Analysis RCA‑001 → Improvement IMP‑007 → Loop FL‑042.”  

---

##### Rules (for DE‑KN‑100)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑100 | Explanation Completeness Rule (Knowledge) | Validation Rule | Every knowledge explanation must include: artifact description, evidence summary, causal chain or reasoning path, confidence and its basis, traceability chain, and known limitations. |
| BR‑KN‑101 | Evidence Citation Rule | Validation Rule | Every claim in an explanation must be supported by a citation to a specific piece of evidence (event record, outcome record, statistical result, stakeholder confirmation). Unsupported claims are flagged. |
| BR‑KN‑102 | Traceability Chain Rule (Knowledge) | Validation Rule | The explanation must include the full ARS traceability chain from the triggering event through all intermediate artifacts to the final insight. |
| BR‑KN‑103 | Natural Language Rule (Knowledge) | Derivation Rule | Explanations follow the standard template: “{{Artifact type}} {{ID}} was produced by {{method/process}}. It concludes that {{finding}}. Evidence: {{evidence summary}}. Causal chain: {{chain}}. Confidence: {{score}}%. Limitations: {{limitations}}. Traceability: {{chain}}.” |

##### Policies (for DE‑KN‑100)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑100 | Explanation Quality Policy (Knowledge) | Compliance Policy | Knowledge explanations with quality score <60% are flagged for improvement. Below 40%, the artifact is not published until the explanation is enhanced. |
| PO‑KN‑101 | Explanation Accessibility Policy | Compliance Policy | All knowledge explanations are accessible to human stakeholders via the knowledge graph explorer and to AI agents via the Serve Knowledge capability. |

---

### 5.10.13 Functional Behaviour  

1. **Event‑driven:** On publication of any knowledge artifact (pattern validated, root cause published, improvement recommended, practice published, loop closed, governance decision made).  
2. **Retrieve** the artifact, its evidence chain, provenance data, and related knowledge graph nodes and edges.  
3. **Execute DE‑KN‑100** (Generate Knowledge Explanation) — rules BR‑KN‑100/101/102/103, policies PO‑KN‑100/101.  
4. **Attach** the explanation to the artifact in the knowledge graph and enterprise memory.  
5. **Publish** explanation via the Serve Knowledge capability for AI agent consumption.  
6. **Raise events:** `KnowledgeExplanationGenerated`.  

### 5.10.14 Commands  

| Command | Purpose |
|---------|---------|
| `GenerateKnowledgeExplanation` | Create a structured explanation for a given knowledge artifact |
| `RegenerateKnowledgeExplanation` | Rebuild explanation after evidence or provenance updates |
| `EvaluateExplanationQuality` | Score an explanation against completeness and clarity criteria |

### 5.10.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `KnowledgeExplanationGenerated` | Artifact ID, artifact type, explanation text, traceability chain, explainability score, timestamp |

### 5.10.16 Queries  

| Query | Description |
|-------|-------------|
| `GetKnowledgeExplanation(artifactId)` | Retrieve the full structured explanation for a knowledge artifact |
| `GetExplainabilityScores(period)` | Aggregate explainability scores by artifact type and capability |

### 5.10.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑KN‑011 | Explainability Score Report (Knowledge) | Explain Knowledge Insights | Knowledge Manager, Data Science | Monthly | Average explainability score by artifact type, trend, low‑score items flagged. |

### 5.10.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑KN‑009 | Explainability Overview (Knowledge) | Explain Knowledge Insights | Knowledge Manager, Data Science | Weekly | Score trends, explanation completeness by capability, traceability chain visualiser. |

### 5.10.19 Software Realization  

```
Event Bus (PatternValidated, RootCausePublished, ImprovementRecommended, etc.)  
→ Explanation Service (template engine, evidence resolver, traceability assembler)  
→ Knowledge Graph Adapter (relationship and provenance queries)  
→ Domain Model (KnowledgeExplanation)  
→ Event Store → Read Model
```  
The explanation service subscribes to all knowledge artifact publication events. It resolves the full evidence chain and traceability path via the knowledge graph. Templates are versioned and stored in a content repository. Explanations are structured in a machine‑readable format (JSON‑LD) for AI agent consumption and rendered as natural language for human stakeholders.  

---

## 5.11 Learn From Knowledge  

### 5.11.1 Purpose  

Close the meta‑loop: continuously improve Knowledge Intelligence itself by analysing the quality, accuracy, timeliness, and value of its own outputs, and recommending enhancements to its methods, thresholds, models, and processes. Answers: *“How can the enterprise’s capacity for cross‑domain learning become better over time?”* The capability ensures that the meta‑domain holds itself accountable to the same continuous improvement standard it orchestrates for all other domains.  

### 5.11.2 Business Objectives Served  

- BO‑KN‑008 Continuously Improve Knowledge Intelligence  

### 5.11.3 Enterprise Measures  

- PI‑KN‑106 Learning Effectiveness Index (Enterprise)  
- PI‑KN‑104 Recommendation Quality Index (Knowledge)  

### 5.11.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑KN‑010 | Knowledge Artifact | Evaluated artifact |
| SE‑KN‑011 | Knowledge Confidence | Subject of calibration |
| SE‑KN‑021 | Pattern Significance | Subject of threshold tuning |
| SE‑KN‑052 | Root‑Cause Confidence | Subject of calibration |
| SE‑KN‑042 | Improvement ROI | Subject of accuracy assessment |
| SE‑KN‑012 | Knowledge Lifecycle | Subject of process improvement |

### 5.11.5 Primitive Capabilities Composed  

- **Observe** – monitors Knowledge Intelligence’s own performance trends  
- **Understand** – identifies patterns in knowledge quality and accuracy  
- **Assess** – evaluates improvement opportunities for the meta‑domain  
- **Predict** – forecasts the impact of proposed improvements to knowledge methods  
- **Evaluate** – compares before/after metrics for meta‑domain improvements  
- **Learn** – institutionalises improvements to Knowledge Intelligence itself  

### 5.11.6 Enterprise Inputs  

- Knowledge quality metrics and trends (from Evaluate Knowledge Quality)  
- Explanation quality scores (from Explain Knowledge Insights)  
- Feedback loop performance data (from Orchestrate Feedback Loops)  
- Pattern validation outcomes: confirmed vs. rejected patterns over time  
- Root‑cause validation outcomes: verified vs. unverified root causes  
- Improvement portfolio ROI actuals vs. estimates  
- Knowledge serving latency and usage data (from Serve Knowledge to AI Agents)  
- Enterprise memory completeness and freshness metrics  

### 5.11.7 Enterprise Understanding Produced  

- Improvement recommendations for Knowledge Intelligence methods, thresholds, and processes  
- Meta‑domain calibration assessments (e.g., pattern significance thresholds, confidence calibration)  
- Learning loop closure reports for meta‑domain improvements  
- Meta‑domain trend analysis: is Knowledge Intelligence getting better at its job?  

### 5.11.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑KN‑100 | Knowledge Improvement Recommendation | Specific change to Knowledge Intelligence methods or parameters with expected benefit |
| OUT‑KN‑101 | Meta‑Domain Calibration Report | Assessment of Knowledge Intelligence’s own confidence and significance calibration |
| OUT‑KN‑102 | Meta‑Domain Learning Loop Closure | Before‑after evaluation of a Knowledge Intelligence improvement |

### 5.11.9 Preconditions  

- Sufficient historical data on Knowledge Intelligence performance exists (minimum 3 evaluation periods)  
- Improvement tracking mechanisms are in place  

### 5.11.10 Capability Dependencies  

- `CA‑KN‑009 Evaluate Knowledge Quality` – for quality metrics  
- `CA‑KN‑010 Explain Knowledge Insights` – for explanation quality data  
- `CA‑KN‑006 Orchestrate Feedback Loops` – for loop performance data  

### 5.11.11 Collaborating Capabilities  

- **All Knowledge Intelligence capabilities** – receive improvement recommendations  

### 5.11.12 Business Decisions  

---

#### DE‑KN‑110 — Recommend Knowledge Method Improvement  

**Purpose:** Analyse Knowledge Intelligence’s own performance to recommend improvements to its detection methods, validation thresholds, confidence calibration, or processes.  

**Required Understanding:** Quality metrics, calibration data, false positive/negative rates for pattern detection, root‑cause accuracy trends, feedback loop cycle times.  

**Decision Alternatives:**  
- No change (performance is adequate and stable)  
- Adjust pattern detection threshold (e.g., correlation strength, significance level)  
- Adjust confidence calibration (e.g., recalibrate confidence scoring model)  
- Adjust validation process (e.g., stakeholder review timelines, evidence requirements)  
- Recommend new method (e.g., new statistical technique for pattern detection)  

**Decision Criteria:** If pattern false discovery rate >20% → adjust detection thresholds. If root‑cause validation rate <60% → adjust confidence calibration. If feedback loop cycle time is increasing → process improvement.  

**Decision Confidence:** Based on data volume and stability.  

**Decision Rationale:** “Recommend adjusting pattern detection correlation threshold from |r| > 0.6 to |r| > 0.65: current false discovery rate is 22% (above 20% threshold), with 3 of 14 candidate patterns in Q3 failing hold‑out validation. Estimated reduction in false discoveries: 2 per quarter, saving 40 stakeholder review hours. Rule BR‑KN‑110 applied.”  

---

##### Rules (for DE‑KN‑110)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑110 | Method Improvement Trigger Rule | Model Evaluation Rule | A method improvement recommendation is triggered if: pattern false discovery rate >20%, root‑cause validation rate <60%, or feedback loop cycle time increases >25% year‑over‑year. |
| BR‑KN‑111 | Calibration Review Rule | Model Evaluation Rule | Knowledge confidence calibration is reviewed quarterly. If confidence scores show systematic over‑ or under‑confidence (calibration error >0.1), recalibration is recommended. |
| BR‑KN‑112 | Method Stability Rule | Consistency Rule | Detection methods and thresholds shall not be changed more than once per quarter unless a significant degradation is documented. |

##### Policies (for DE‑KN‑110)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑110 | Method Change Approval Policy | Approval Policy | Changes to detection methods, validation thresholds, or confidence calibration require Knowledge Manager approval and must be documented with a rationale and expected impact. |

---

#### DE‑KN‑111 — Recommend Process Improvement  

**Purpose:** Analyse Knowledge Intelligence’s operational processes—loop orchestration, stakeholder review, adoption enforcement—and recommend changes to improve efficiency and effectiveness.  

**Required Understanding:** Process cycle times, bottleneck analysis, stakeholder responsiveness, adoption compliance rates.  

**Decision Criteria:** Identify the most significant process bottleneck each quarter and propose a targeted improvement.  

**Decision Rationale:** “Recommend process improvement: stakeholder review stage for pattern validation currently takes average 12 days (target 10 days). Bottleneck identified: Domain Managers from Supply are consistently the last to respond. Recommendation: designate a Supply knowledge liaison with 5‑day SLA. Estimated cycle time reduction: 3 days.”  

---

##### Rules (for DE‑KN‑111)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑113 | Process Bottleneck Rule | Derivation Rule | The process stage with the highest average cycle time exceeding its SLA is identified as the primary bottleneck and targeted for improvement. |
| BR‑KN‑114 | Process Change Validation Rule | Validation Rule | Process changes must be piloted for one quarter before full adoption. |

##### Policies (for DE‑KN‑111)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑111 | Process Change Approval Policy | Approval Policy | Process changes that affect cross‑domain stakeholder responsibilities require approval from affected Domain Managers. |

---

#### DE‑KN‑112 — Close the Meta‑Domain Learning Loop  

**Purpose:** After an improvement to Knowledge Intelligence itself is implemented, evaluate whether the expected benefit was realized.  

**Required Understanding:** Implemented improvement, pre‑implementation baseline metrics, post‑implementation outcome data, observation window.  

**Decision Alternatives:** Improvement confirmed, Partially realized, No improvement, Negative impact (rollback).  

**Decision Criteria:** Improvement must demonstrate the expected benefit within two evaluation periods.  

**Decision Rationale:** “Method improvement IMP‑KN‑003 (correlation threshold adjustment from 0.6 to 0.65) confirmed: false discovery rate reduced from 22% to 14% in Q4. Pattern discovery rate stable at 4 per quarter. No increase in missed patterns (sensitivity maintained). Improvement confirmed.”  

---

##### Rules (for DE‑KN‑112)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑KN‑115 | Meta‑Loop Verification Rule | Validation Rule | Every Knowledge Intelligence improvement must be evaluated after a minimum observation window of two evaluation periods (2 months for operational, 2 quarters for strategic). |
| BR‑KN‑116 | Meta‑Loop Rollback Rule | Model Evaluation Rule | If an implemented change causes statistically significant degradation in any primary Knowledge Intelligence metric, automatic rollback is triggered. |

##### Policies (for DE‑KN‑112)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑KN‑112 | Meta‑Loop Closure Policy | Compliance Policy | Every Knowledge Intelligence improvement is documented with before‑after evaluation. Results are reported in the Knowledge Quality Report and presented at the quarterly executive S&OP review. |

---

### 5.11.13 Functional Behaviour  

1. **Scheduled:** Quarterly deep analysis of Knowledge Intelligence performance, monthly threshold review.  
2. **Event‑driven:** After Knowledge Quality Report publication, after detection of significant method degradation.  
3. **Retrieve** quality metrics, calibration data, process cycle times, loop performance data.  
4. **Execute DE‑KN‑110** (Recommend Knowledge Method Improvement) — rules BR‑KN‑110/111/112, policy PO‑KN‑110.  
5. **Execute DE‑KN‑111** (Recommend Process Improvement) — rules BR‑KN‑113/114, policy PO‑KN‑111.  
6. **For prior meta‑domain improvements**, execute DE‑KN‑112 (Close Meta‑Domain Learning Loop) — rules BR‑KN‑115/116, policy PO‑KN‑112.  
7. **Publish** recommendations and loop closure reports.  
8. **Feed** approved improvements back to the relevant Knowledge Intelligence capabilities.  
9. **Raise events:** `KnowledgeImprovementRecommended`, `KnowledgeProcessImprovementRecommended`, `KnowledgeMetaLoopClosed`.  

### 5.11.14 Commands  

| Command | Purpose |
|---------|---------|
| `AnalyzeKnowledgePerformance` | Run trend analysis on Knowledge Intelligence’s own quality and process metrics |
| `RecommendMethodImprovement` | Generate a method or threshold improvement recommendation |
| `RecommendProcessImprovement` | Generate a process improvement recommendation |
| `EvaluateMetaImprovement` | Assess the impact of an implemented Knowledge Intelligence improvement |
| `RollbackMetaImprovement` | Revert a meta‑domain change that caused degradation |

### 5.11.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `KnowledgeImprovementRecommended` | Recommendation ID, type (method/threshold/calibration), target capability, expected benefit, confidence |
| `KnowledgeProcessImprovementRecommended` | Recommendation ID, process stage, bottleneck, expected cycle time reduction |
| `KnowledgeMetaLoopClosed` | Improvement ID, before/after metrics, verdict (confirmed/partial/rejected), rollback status |

### 5.11.16 Queries  

| Query | Description |
|-------|-------------|
| `GetKnowledgeImprovementHistory(period)` | All recommended and implemented meta‑domain improvements with outcomes |
| `GetActiveKnowledgeImprovements()` | Meta‑domain improvements awaiting evaluation or in progress |
| `GetKnowledgeLearningEffectiveness()` | Composite metric of Knowledge Intelligence’s own improvement success rate |

### 5.11.17 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑KN‑012 | Knowledge Improvement Report | Learn From Knowledge | Knowledge Manager, Executive | Quarterly | Meta‑domain improvements proposed, implemented, verified; before/after metrics. |

### 5.11.18 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑KN‑010 | Meta‑Domain Learning Dashboard | Learn From Knowledge | Knowledge Manager, Data Science | Monthly | Improvement funnel, meta‑domain metrics trends, calibration health, rollback events. |

### 5.11.19 Software Realization  

```
Scheduled/Event Triggers → Analytics Engine (meta‑domain performance analysis)  
→ Domain Service (KnowledgeImprovement aggregate, MetaLearningLoop)  
→ Rule Engine (method improvement triggers, calibration rules, rollback rules)  
→ Event Store → Projections → Read Model
```  
The analytics engine queries the quality, explanation, and process data from Knowledge Intelligence’s own capabilities. Method changes (e.g., threshold adjustments) are applied via a configuration service. Process changes are communicated to affected stakeholders and tracked for compliance. All meta‑domain improvements are version‑tracked for audit and reproducibility.  

---

# Chapter 6 — External Interfaces  

## 6.1 Purpose  

This chapter defines every external interface that the Knowledge Intelligence domain exposes to other domains, AI agents, and users. Each interface is specified with its purpose, contract, authentication, and the capability that owns it.  

## 6.2 Enterprise APIs  

### 6.2.1 Knowledge Graph API  

| Attribute | Value |
|-----------|-------|
| Owner | Govern Knowledge Graph (5.1) |
| Purpose | Query, validate, and update the enterprise knowledge graph. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 (Client Credentials / User) |
| Endpoints | `GET /api/v1/knowledge/graph`, `POST /api/v1/knowledge/graph/validate`, `GET /api/v1/knowledge/graph/ontology`, `GET /api/v1/knowledge/graph/consistency` |

**Example Response (Graph Query):**  
```json
{
  "nodes": [
    {"id": "SE-PI-002", "type": "SemanticConcept", "domain": "Promise", "label": "Promise"},
    {"id": "DE-PI-020", "type": "Decision", "domain": "Promise", "label": "Evaluate ATP"}
  ],
  "edges": [
    {"source": "DE-PI-020", "target": "SE-PI-002", "type": "Produces"}
  ]
}
```  

---

### 6.2.2 Pattern Discovery API  

| Attribute | Value |
|-----------|-------|
| Owner | Discover Cross‑Domain Patterns (5.2) |
| Purpose | Trigger pattern detection, query discovered patterns, retrieve causal chains. |
| Endpoints | `POST /api/v1/knowledge/patterns/detect`, `GET /api/v1/knowledge/patterns`, `GET /api/v1/knowledge/patterns/{patternId}`, `GET /api/v1/knowledge/patterns/{patternId}/causal-chain` |

---

### 6.2.3 Root‑Cause Analysis API  

| Attribute | Value |
|-----------|-------|
| Owner | Analyze Root Causes (5.3) |
| Purpose | Start and retrieve root‑cause analyses and corrective action recommendations. |
| Endpoints | `POST /api/v1/knowledge/root-cause/analyze`, `GET /api/v1/knowledge/root-cause/{analysisId}`, `GET /api/v1/knowledge/root-cause/{analysisId}/corrective-actions` |

---

### 6.2.4 Improvement Portfolio API  

| Attribute | Value |
|-----------|-------|
| Owner | Manage Improvement Portfolio (5.4) |
| Purpose | Submit, query, prioritise, approve, and verify improvement initiatives. |
| Endpoints | `POST /api/v1/knowledge/improvements`, `GET /api/v1/knowledge/improvements`, `GET /api/v1/knowledge/improvements/portfolio`, `POST /api/v1/knowledge/improvements/{id}/approve`, `POST /api/v1/knowledge/improvements/{id}/verify` |

---

### 6.2.5 Best‑Practice API  

| Attribute | Value |
|-----------|-------|
| Owner | Institutionalise Best Practices (5.5) |
| Purpose | Nominate, validate, publish, and query best practices. |
| Endpoints | `POST /api/v1/knowledge/practices`, `GET /api/v1/knowledge/practices`, `GET /api/v1/knowledge/practices/{practiceId}`, `POST /api/v1/knowledge/practices/{id}/validate`, `POST /api/v1/knowledge/practices/{id}/publish` |

---

### 6.2.6 Feedback Loop API  

| Attribute | Value |
|-----------|-------|
| Owner | Orchestrate Feedback Loops (5.6) |
| Purpose | Query active, stalled, and closed feedback loops; retrieve cycle time analysis. |
| Endpoints | `GET /api/v1/knowledge/feedback-loops`, `GET /api/v1/knowledge/feedback-loops/{loopId}`, `GET /api/v1/knowledge/feedback-loops/analytics/cycle-time` |

---

### 6.2.7 Enterprise Memory API  

| Attribute | Value |
|-----------|-------|
| Owner | Maintain Enterprise Memory (5.7) |
| Purpose | Query enterprise memory for past events, decisions, and outcomes. |
| Endpoints | `POST /api/v1/knowledge/memory/query`, `GET /api/v1/knowledge/memory/events/{eventId}`, `GET /api/v1/knowledge/memory/decisions/{decisionId}` |

**Example Request (Memory Query):**  
```json
{
  "query": "Similar situations to current: forecast bias >10% in Demand for top 20 SKUs",
  "domain": "Demand",
  "artifactTypes": ["Forecast"],
  "timeRange": "2024-01-01 to 2026-06-30",
  "maxResults": 5
}
```  

---

### 6.2.8 AI Agent Knowledge API  

| Attribute | Value |
|-----------|-------|
| Owner | Serve Knowledge to AI Agents (5.8) |
| Purpose | Authenticated endpoint for AI agents to request enterprise knowledge in context. |
| Protocol | REST (HTTPS) / gRPC |
| Authentication | OAuth 2.0 (Client Credentials) with agent‑specific scopes |
| Rate Limit | 1,000 requests/minute per agent |
| Endpoint | `POST /api/v1/knowledge/agent/query` |

**Example Request:**  
```json
{
  "agentId": "supply-planning-agent-v2",
  "context": {
    "domain": "Supply",
    "decision": "CapacityAllocation",
    "artifacts": ["ProductFamily:PF3", "Resource:WC-100"],
    "horizon": "Q3-2026"
  },
  "queryType": "Composite",
  "requiredConfidence": 80
}
```  

---

### 6.2.9 Knowledge Quality API  

| Attribute | Value |
|-----------|-------|
| Owner | Evaluate Knowledge Quality (5.9) |
| Purpose | Retrieve knowledge quality metrics, trends, and reports. |
| Endpoints | `GET /api/v1/knowledge/quality`, `GET /api/v1/knowledge/quality/trends`, `GET /api/v1/knowledge/quality/reports/{period}` |

---

### 6.2.10 Knowledge Explanation API  

| Attribute | Value |
|-----------|-------|
| Owner | Explain Knowledge Insights (5.10) |
| Purpose | Retrieve structured explanations for any knowledge artifact. |
| Endpoint | `GET /api/v1/knowledge/explanations/{artifactId}` |

---

## 6.3 Integration Events  

Knowledge Intelligence publishes events to the enterprise event bus (Kafka topic: `knowledge-intelligence-events`). All events use the CloudEvents v1.0 envelope.  

| Event Type | Payload Summary | Publisher Capability | Consumers |
|------------|-----------------|---------------------|-----------|
| `KnowledgeGraphUpdated` | Object ID, type, change type, timestamp | Govern Knowledge Graph | All Knowledge capabilities, AI Agents |
| `ConsistencyViolationDetected` | Violation ID, type, affected objects, severity | Govern Knowledge Graph | Knowledge Manager |
| `OntologyVersionPublished` | Version number, change summary | Govern Knowledge Graph | All domains |
| `KnowledgeGapAssigned` | Gap ID, description, assigned domain, deadline | Govern Knowledge Graph | Affected domain |
| `CandidatePatternDetected` | Pattern ID, type, involved domains, metrics, confidence | Discover Cross‑Domain Patterns | Analyze Root Causes |
| `PatternValidated` | Pattern ID, validation method, confidence | Discover Cross‑Domain Patterns | Analyze Root Causes, Institutionalise Best Practices, Explain Knowledge Insights |
| `PatternRejected` | Pattern ID, reason | Discover Cross‑Domain Patterns | Learn From Knowledge |
| `CausalChainProposed` | Pattern ID, chain ID, links, confidence | Discover Cross‑Domain Patterns | Analyze Root Causes |
| `RootCauseIdentified` | Analysis ID, root cause, contributing factors, confidence | Analyze Root Causes | Manage Improvement Portfolio, Explain Knowledge Insights |
| `CorrectiveActionRecommended` | Analysis ID, action details, expected impact, implementing domain | Analyze Root Causes | Manage Improvement Portfolio |
| `RootCauseValidated` | Analysis ID, outcome metrics, confidence update | Analyze Root Causes | Manage Improvement Portfolio, Learn From Knowledge |
| `RootCauseValidationFailed` | Analysis ID, reason | Analyze Root Causes | Learn From Knowledge |
| `ImprovementProposed` | Initiative ID, origin, expected benefit, cost | Manage Improvement Portfolio | Domain Managers |
| `PortfolioPrioritised` | Ranked list, scores, timestamp | Manage Improvement Portfolio | Executive, Domain Managers |
| `ImplementationApproved` | Initiative ID, budget, timeline, owner | Manage Improvement Portfolio | Implementing domain |
| `ImprovementVerified` | Initiative ID, actual benefit, verification status | Manage Improvement Portfolio | Learn From Knowledge, Institutionalise Best Practices |
| `ImprovementRollbackRecommended` | Initiative ID, reason | Manage Improvement Portfolio | Implementing domain |
| `BestPracticeNominated` | Practice ID, nominator, origin domain, evidence summary | Institutionalise Best Practices | Knowledge Manager |
| `BestPracticeValidated` | Practice ID, confidence, applicable domains | Institutionalise Best Practices | Govern Knowledge Graph, Serve Knowledge to AI Agents |
| `BestPracticePublished` | Practice ID, version, applicable domains | Institutionalise Best Practices | All domains, AI Agents |
| `BestPracticeAdoptionAssessed` | Practice ID, domain adoption statuses | Institutionalise Best Practices | Domain Managers |
| `FeedbackLoopOpened` | Loop ID, triggering event, classification, owner | Orchestrate Feedback Loops | All Knowledge capabilities |
| `FeedbackLoopStageCompleted` | Loop ID, stage, outputs, timestamp | Orchestrate Feedback Loops | Orchestrate Feedback Loops (monitoring) |
| `FeedbackLoopStalled` | Loop ID, stalled stage, reason, escalation target | Orchestrate Feedback Loops | Knowledge Manager |
| `FeedbackLoopClosed` | Loop ID, closure status, lessons learned | Orchestrate Feedback Loops | Maintain Enterprise Memory, Learn From Knowledge |
| `EventRecorded` | Event ID, domain, type, artifacts, impact | Maintain Enterprise Memory | Discover Cross‑Domain Patterns, Serve Knowledge to AI Agents |
| `DecisionRecorded` | Decision ID, context, alternatives, rationale | Maintain Enterprise Memory | Serve Knowledge to AI Agents |
| `MemoryQueryResponded` | Query ID, matched records, relevance scores | Maintain Enterprise Memory | AI Agents |
| `KnowledgeRequestReceived` | Request ID, agent ID, domain, query type | Serve Knowledge to AI Agents | Evaluate Knowledge Quality |
| `KnowledgeResponseServed` | Request ID, artifact count, confidence, response time | Serve Knowledge to AI Agents | Evaluate Knowledge Quality |
| `KnowledgeQualityComputed` | Period, metrics with values and confidence | Evaluate Knowledge Quality | Learn From Knowledge |
| `KnowledgeQualityTrendAssessed` | Period, trend direction per metric | Evaluate Knowledge Quality | Learn From Knowledge |
| `KnowledgeQualityReportPublished` | Report ID, period, overall health score | Evaluate Knowledge Quality | Knowledge Manager, Executive |
| `KnowledgeExplanationGenerated` | Artifact ID, artifact type, explanation, traceability, score | Explain Knowledge Insights | Serve Knowledge to AI Agents, Learn From Knowledge |
| `KnowledgeImprovementRecommended` | Recommendation ID, type, target capability, expected benefit | Learn From Knowledge | Knowledge Manager |
| `KnowledgeProcessImprovementRecommended` | Recommendation ID, process stage, bottleneck | Learn From Knowledge | Knowledge Manager |
| `KnowledgeMetaLoopClosed` | Improvement ID, before/after metrics, verdict | Learn From Knowledge | Evaluate Knowledge Quality |

---

## 6.4 Import Interfaces  

| Interface | Format | Frequency | Target Capability |
|-----------|--------|-----------|-------------------|
| Domain Semantic Object Publication | JSON (ARS‑compliant schema) | On change | Govern Knowledge Graph |
| Learning Event Intake | JSON (standard event schema) | Real‑time | Orchestrate Feedback Loops |
| Domain Quality Report Intake | JSON | Weekly/Monthly | Evaluate Knowledge Quality |
| External Risk Factor Data | CSV / API | Daily | Discover Cross‑Domain Patterns |

---

## 6.5 Export Interfaces  

| Interface | Format | Frequency | Source Capability |
|-----------|--------|-----------|-------------------|
| Knowledge Graph Export | JSON‑LD | On demand / Weekly snapshot | Govern Knowledge Graph |
| Best‑Practice Catalogue Distribution | PDF / API | Quarterly | Institutionalise Best Practices |
| Knowledge Quality Report Distribution | PDF / Email | Monthly / Quarterly | Evaluate Knowledge Quality |
| Improvement Portfolio Summary | PDF / API | Monthly | Manage Improvement Portfolio |

---

# Chapter 7 — Reports & Dashboards  

## 7.1 Purpose  

This chapter consolidates every report and dashboard defined across the eleven Knowledge Intelligence capabilities.  

## 7.2 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑KN‑001 | Knowledge Graph Health Report | Govern Knowledge Graph | Knowledge Manager | Monthly | Consistency score, coverage percentage, violation trends, gap status |
| RPT‑KN‑002 | Ontology Change Log | Govern Knowledge Graph | Knowledge Manager, Domain Managers | On change | Version history with change descriptions and impact |
| RPT‑KN‑003 | Cross‑Domain Pattern Report | Discover Cross‑Domain Patterns | Knowledge Manager, Domain Managers, Executive | Monthly | All validated patterns with significance, confidence, domains, and impact estimates |
| RPT‑KN‑004 | Correlation Matrix Report | Discover Cross‑Domain Patterns | Knowledge Manager, Data Science | Quarterly | Quantitative relationships between key metrics across all domain pairs |
| RPT‑KN‑005 | Root‑Cause Analysis Report | Analyze Root Causes | Knowledge Manager, Domain Managers | Quarterly | Completed analyses with findings, causal chains, confidence, and corrective actions |
| RPT‑KN‑006 | Corrective Action Tracking Report | Analyze Root Causes | Knowledge Manager, Domain Managers | Monthly | Implementation status and validation outcomes for all corrective actions |
| RPT‑KN‑007 | Improvement Portfolio Report | Manage Improvement Portfolio | Knowledge Manager, Executive | Monthly | Ranked initiatives with status, ROI, and resource allocation |
| RPT‑KN‑008 | Portfolio Health Report | Manage Improvement Portfolio | Knowledge Manager | Monthly | Balance across domains, resource utilisation, risk diversification |
| RPT‑KN‑009 | Best‑Practice Catalogue Report | Institutionalise Best Practices | Knowledge Manager, Domain Managers | Quarterly | All active practices with applicability, adoption status, and evidence strength |
| RPT‑KN‑010 | Adoption Compliance Report | Institutionalise Best Practices | Knowledge Manager | Monthly | Per‑domain adoption rates, non‑adoption justifications, overdue adoptions |
| RPT‑KN‑011 | Practice Effectiveness Review | Institutionalise Best Practices | Knowledge Manager | Annually | Review of each practice’s continued effectiveness and recommended revisions |
| RPT‑KN‑012 | Feedback Loop Status Report | Orchestrate Feedback Loops | Knowledge Manager | Weekly | All active loops with progress, bottlenecks, and cycle times |
| RPT‑KN‑013 | Loop Effectiveness Report | Orchestrate Feedback Loops | Knowledge Manager, Executive | Quarterly | Closure rates, success rates, average cycle times by loop type |
| RPT‑KN‑014 | Enterprise Memory Completeness Report | Maintain Enterprise Memory | Knowledge Manager | Monthly | Coverage by domain, event type, and period; identified gaps |
| RPT‑KN‑015 | Memory Query Analytics Report | Maintain Enterprise Memory | Knowledge Manager, IT Operations | Monthly | Query volume, response times, match rates, top queries |
| RPT‑KN‑016 | AI Agent Knowledge Usage Report | Serve Knowledge to AI Agents | Knowledge Manager, Data Science | Monthly | Request volume, response times, knowledge types consumed, top agents |
| RPT‑KN‑017 | Knowledge Serving Performance Report | Serve Knowledge to AI Agents | IT Operations | Weekly | Latency, throughput, error rates, cache hit rates |
| RPT‑KN‑018 | Knowledge Quality Report | Evaluate Knowledge Quality | Knowledge Manager, Executive | Monthly, Quarterly | All quality metrics with trends, gaps, and recommendations |
| RPT‑KN‑019 | Explainability Score Report (Knowledge) | Explain Knowledge Insights | Knowledge Manager, Data Science | Monthly | Average explainability score by artifact type, low‑score items flagged |
| RPT‑KN‑020 | Knowledge Improvement Report | Learn From Knowledge | Knowledge Manager, Executive | Quarterly | Meta‑domain improvements proposed, implemented, verified; before/after metrics |

---

## 7.3 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑KN‑001 | Knowledge Graph Explorer | Govern Knowledge Graph | Knowledge Manager, Domain Managers | Real‑time | Interactive graph visualisation, node and edge inspection, search |
| DASH‑KN‑002 | Knowledge Health Dashboard | Govern Knowledge Graph | Knowledge Manager | Daily | Consistency gauges, coverage charts, gap tracker, violation trends |
| DASH‑KN‑003 | Cross‑Domain Insight Dashboard | Discover Cross‑Domain Patterns | Knowledge Manager, Executive | Weekly | Pattern discovery feed, significance rankings, causal chain visualiser |
| DASH‑KN‑004 | Pattern Significance Heatmap | Discover Cross‑Domain Patterns | Knowledge Manager, Data Science | Monthly | Visual representation of pattern strength across domain pairs |
| DASH‑KN‑005 | Root‑Cause Investigation Workbench | Analyze Root Causes | Knowledge Manager | On‑demand | Causal chain visualiser, evidence explorer, 5 Whys tracker |
| DASH‑KN‑006 | Corrective Action Dashboard | Analyze Root Causes | Knowledge Manager, Domain Managers | Weekly | Action status, validation progress, ROI tracking |
| DASH‑KN‑007 | Improvement Portfolio Dashboard | Manage Improvement Portfolio | Knowledge Manager, Executive | Weekly | Prioritised list, status tracking, ROI trends, resource load |
| DASH‑KN‑008 | Portfolio Health Dashboard | Manage Improvement Portfolio | Knowledge Manager | Monthly | Domain balance, risk heatmap, resource allocation pie charts |
| DASH‑KN‑009 | Best‑Practice Catalogue Dashboard | Institutionalise Best Practices | Knowledge Manager, Domain Managers | Daily | Practice inventory, confidence distribution, applicability map |
| DASH‑KN‑010 | Adoption Compliance Monitor | Institutionalise Best Practices | Knowledge Manager | Weekly | Per‑domain adoption status, overdue items, compliance trends |
| DASH‑KN‑011 | Feedback Loop Control Tower | Orchestrate Feedback Loops | Knowledge Manager | Real‑time | Loop map, stage progress, stalled loops highlighted, SLA gauges |
| DASH‑KN‑012 | Learning Cycle Dashboard | Orchestrate Feedback Loops | Knowledge Manager, Executive | Monthly | Cycle time trends, closure rates, bottleneck analysis |
| DASH‑KN‑013 | Enterprise Memory Explorer | Maintain Enterprise Memory | Knowledge Manager, Domain Managers | Real‑time | Interactive search and browse of recorded events and decisions |
| DASH‑KN‑014 | Memory Health Dashboard | Maintain Enterprise Memory | Knowledge Manager | Daily | Completeness gauges, freshness indicators, query performance |
| DASH‑KN‑015 | AI Knowledge Serving Monitor | Serve Knowledge to AI Agents | Knowledge Manager, IT Operations | Real‑time | Request volume, response times, error rates, active agents |
| DASH‑KN‑016 | Knowledge Usage Analytics | Serve Knowledge to AI Agents | Knowledge Manager, Data Science | Weekly | Most‑queried patterns, practices, and precedents |
| DASH‑KN‑017 | Knowledge Quality Dashboard | Evaluate Knowledge Quality | Knowledge Manager, Executive | Monthly | Metric gauges, trend charts, gap indicators, target comparisons |
| DASH‑KN‑018 | Knowledge Health Scorecard | Evaluate Knowledge Quality | Executive | Quarterly | Balanced scorecard of all Knowledge Intelligence KPIs |
| DASH‑KN‑019 | Explainability Overview (Knowledge) | Explain Knowledge Insights | Knowledge Manager, Data Science | Weekly | Score trends, explanation completeness by capability, traceability visualiser |
| DASH‑KN‑020 | Meta‑Domain Learning Dashboard | Learn From Knowledge | Knowledge Manager | Monthly | Improvement funnel, meta‑domain metrics trends, calibration health |

---

# Chapter 8 — Appendix  

## 8.1 Knowledge Exception Priority Matrix  

The following matrix defines the default mapping from Knowledge Exception Type and Decision Impact to Exception Severity. It is referenced by DE‑KN‑091 and similar prioritisation decisions across Knowledge Intelligence capabilities.  

| Exception Type | Blocks Strategic Decision | Blocks Tactical Decision | Degrades Quality | Informational |
|----------------|--------------------------|--------------------------|------------------|---------------|
| Knowledge Graph Inconsistency | Critical | High | Medium | Low |
| Pattern False Discovery Surge | High | Medium | Medium | Low |
| Root‑Cause Validation Failure | Critical | High | High | Medium |
| Improvement Rollback | High | High | Medium | Low |
| Best‑Practice Adoption Non‑Compliance | High | Medium | Medium | Low |
| Feedback Loop Stall (strategic) | Critical | High | Medium | Low |
| Enterprise Memory Gap (critical event) | High | Medium | Medium | Low |
| AI Knowledge Serving Degradation | High | High | Medium | Low |
| Knowledge Quality Metric Degradation | High | Medium | Medium | Low |
| Explainability Score Drop | Medium | Medium | Medium | Low |
| Method Calibration Drift | High | Medium | Medium | Low |

**Notes:**  
- The matrix is configurable via the meta‑domain learning loop (DE‑KN‑110) subject to policy PO‑KN‑110.  
- Exceptions that are self‑resolving within one evaluation period are classified as Transient and not raised.  

---

## 8.2 Enterprise Glossary  

A consolidated glossary of all enterprise terms defined across the Knowledge Intelligence Specification.  

| Term | ID (if any) | Definition |
|------|-------------|------------|
| Best Practice | SE‑KN‑060 | A proven strategy, method, or configuration demonstrated to produce superior outcomes. |
| Causal Chain | SE‑KN‑024 | A sequence of cause‑and‑effect relationships linking an event in one domain to an outcome in another. |
| Contributing Factor | SE‑KN‑051 | A condition that amplifies a systemic problem but is not its fundamental origin. |
| Cross‑Domain Pattern | SE‑KN‑003 | A statistically significant correlation, causal relationship, or recurring sequence spanning at least two Intelligence Domains. |
| Enterprise Memory | SE‑KN‑005 | The immutable, queryable record of significant enterprise events, decisions, outcomes, and lessons learned. |
| Feedback Loop | SE‑KN‑072 | The complete cycle from event occurrence through pattern discovery, root‑cause analysis, improvement, and verification. |
| Feedback Signal | SE‑KN‑070 | An observable indicator from any domain that something has changed or a decision succeeded or failed. |
| Improvement Initiative | SE‑KN‑040 | A proposed or active project to enhance enterprise performance based on knowledge discoveries. |
| Improvement Portfolio | SE‑KN‑004 | The managed collection of all improvement initiatives across the enterprise. |
| Knowledge | SE‑KN‑001 | A validated, reusable piece of enterprise understanding from analysis of outcomes across domains. |
| Knowledge Artifact | SE‑KN‑010 | A discrete, identifiable piece of enterprise knowledge with unique ID, type, confidence, and traceability. |
| Knowledge Confidence | SE‑KN‑011 | A score (0–100%) expressing the statistical or evidential reliability of a knowledge artifact. |
| Knowledge Edge | SE‑KN‑031 | A directed, typed relationship between two knowledge nodes. |
| Knowledge Graph | — | The authoritative, versioned graph of all enterprise objects and their relationships. |
| Knowledge Node | SE‑KN‑030 | A vertex in the knowledge graph representing any enterprise object. |
| Knowledge Provenance | SE‑KN‑015 | The complete lineage of a knowledge artifact: origin, evidence, validations, and decisions. |
| Learning Event | SE‑KN‑002 | A significant occurrence in any domain that triggers or contributes to enterprise learning. |
| Loop Closure | SE‑KN‑073 | The formal completion of a feedback loop with verified outcome. |
| Memory Query | SE‑KN‑083 | A request to the enterprise memory for relevant past events, decisions, or outcomes. |
| Ontology Version | SE‑KN‑033 | A versioned snapshot of the knowledge graph schema (node types, edge types, consistency rules). |
| Pattern Significance | SE‑KN‑021 | A measure of the statistical or practical importance of a discovered pattern. |
| Practice Applicability | SE‑KN‑062 | The conditions under which a best practice is expected to be effective. |
| Practice Institutionalisation | SE‑KN‑063 | The formal adoption of a best practice as an enterprise standard across all applicable domains. |
| Practice Provenance | SE‑KN‑061 | The origin and validation history of a best practice. |
| Root Cause | SE‑KN‑050 | The fundamental originating factor of a systemic problem. |
| Root‑Cause Analysis | SE‑KN‑053 | A structured investigation tracing a systemic problem from symptoms back to root cause(s). |
| Root‑Cause Confidence | SE‑KN‑052 | A score (0–100%) expressing the certainty that an identified cause is the true root cause. |
| Semantic Consistency Rule | SE‑KN‑032 | A rule governing the integrity of the knowledge graph (duplicates, conflicts, missing relationships). |

---

## 8.3 Formula Reference  

Complete set of formulas used in Chapter 3 (Enterprise Measurement Model).  

**PI‑KN‑002 — Cross‑Domain Pattern Discovery Rate**  
```
Pattern Discovery Rate = Count of Validated Patterns Published in Period
```
Where Validated Pattern involves ≥2 domains, confidence ≥80%, stakeholder‑reviewed, and recorded in the knowledge graph.  

**PI‑KN‑003 — Root‑Cause Identification Accuracy**  
```
Root‑Cause Accuracy (%) = (Number of Root Causes Confirmed Correct ÷ Total Root‑Cause Analyses with Outcome Data) × 100
```
Where Confirmed Correct means corrective action produced ≥50% reduction in the problem metric.  

**PI‑KN‑004 — Improvement Portfolio ROI**  
```
Portfolio ROI = (Σ Estimated Annual Benefit − Σ Total Cost) ÷ Σ Total Cost
```

**PI‑KN‑005 — Best‑Practice Institutionalisation Rate**  
```
Institutionalisation Rate (%) = (Number of Best Practices Adopted as Enterprise Standards ÷ Total Validated Best Practices) × 100
```

**PI‑KN‑006 — Knowledge Graph Consistency Score**  
```
Consistency Score (%) = (1 − (Number of Consistency Violations ÷ Total Nodes and Edges)) × 100
```

**PI‑KN‑007 — Cross‑Domain Learning Cycle Time**  
```
Learning Cycle Time = Average (Time(Loop Closed) − Time(Triggering Event Occurred))
```

**PI‑KN‑008 — Enterprise Memory Completeness**  
```
Memory Completeness (%) = (Number of Significant Events Recorded with Full Traceability ÷ Total Significant Events) × 100
```

**PI‑KN‑009 — Systemic Risk Reduction**  
```
Systemic Risk Reduction (%) = ((Risk Score Before − Risk Score After) ÷ Risk Score Before) × 100
```

**PI‑KN‑010 — Decision Confidence Improvement (Enterprise)**  
```
Confidence Improvement (pp) = Average Confidence (post‑Knowledge insight) − Average Confidence (pre‑Knowledge insight)
```

**PI‑KN‑011 — Cross‑Domain Plan Consistency Score**  
```
Plan Consistency (%) = (1 − (Σ |Domain Plan Deviation| ÷ Number of Checkpoints)) × 100
```

**PI‑KN‑012 — Feedback Loop Closure Rate**  
```
Loop Closure Rate (%) = (Number of Closed Loops ÷ Total Opened Loops) × 100
```

**PI‑KN‑013 — Knowledge Serving Latency**  
```
Serving Latency = Average (Time(Response Returned) − Time(Query Received))
```

**PI‑KN‑014 — Planning Cycle Time (Knowledge)**  
```
Cycle Time = Time(Knowledge Cycle Completed) − Time(Knowledge Cycle Initiated)
```

**PI‑KN‑015 — Strategic Insight Generation Rate**  
```
Strategic Insight Generation Rate = Count of Strategic Insights Published in Period
```

---

## 8.4 References  

### Architecture Documents  
- Medhavi APS Constitution  
- Architecture Reference Standard (ARS) v1  
- Semantic Model  
- Capability Model  
- Decision Model  
- Rule & Policy Model  

### Intelligence Specifications  
- Demand Intelligence Specification  
- Supply Intelligence Specification  
- Promise Intelligence Specification  
- Scenario Intelligence Specification  
- Knowledge Intelligence Specification (this document)  

---
