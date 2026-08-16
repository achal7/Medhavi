# Scenario Intelligence Specification  

# Chapter 1 — Purpose & Scope  

## 1.1 Purpose  

Scenario Intelligence is the authoritative enterprise domain responsible for exploring, simulating, comparing, and recommending alternative enterprise futures. It answers the question: *“What if things change, and what should we do about it?”*  

Where Demand Intelligence tells us what customers want, Supply Intelligence tells us how we can satisfy them, and Promise Intelligence tells us what we commit to — Scenario Intelligence asks what happens if those assumptions are wrong, or if we choose a different path. It stress‑tests plans against demand shocks, supply disruptions, capacity changes, strategic shifts, and competitive moves. It evaluates trade‑offs between cost, service, risk, and resilience. It recommends robust strategies that perform well across multiple possible futures rather than optimizing for a single forecast.  

This specification defines every business objective, performance indicator, semantic concept, capability, decision, rule, policy, functional behaviour, interface, report, and dashboard that constitutes the Scenario Intelligence domain. It is the single source of enterprise truth for scenario planning.  

## 1.2 Scope  

**Scenario Intelligence includes:**  

- Scenario definition and lifecycle management: creating, versioning, and archiving scenario definitions  
- What‑if simulation: executing simulations against demand, supply, capacity, and financial models under varying assumptions  
- Multi‑scenario comparison: evaluating multiple scenarios side‑by‑side against defined criteria  
- Risk assessment: identifying, quantifying, and stress‑testing enterprise risks  
- Sensitivity analysis: understanding which variables most influence outcomes  
- Probabilistic and deterministic simulation methods (Monte Carlo, discrete event, linear programming)  
- Scenario‑based recommendations: proposing which plan variant to adopt  
- Collaborative scenario planning: enabling cross‑functional stakeholders to explore scenarios together  
- Scenario trigger sensing: detecting events that warrant a new scenario analysis  
- Scenario quality evaluation: measuring simulation accuracy and recommendation effectiveness  
- Scenario decision explainability and traceability  
- Continuous learning from scenario outcomes  

**Scenario Intelligence excludes:**  

- Operational plan execution (belongs to Demand, Supply, Promise)  
- Real‑time order promising (Promise Intelligence)  
- Tactical supply planning (Supply Intelligence)  
- Demand forecasting (Demand Intelligence)  
- Financial planning and budgeting (Finance systems — though Scenario Intelligence may interface with them)  

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

## BO‑SN‑001 — Deliver Trusted Scenario Analysis  

**Business Motivation**  

Enterprise planning cannot rely on a single forecast. Scenario Intelligence shall produce trusted, rigorous, and explainable analyses of multiple possible futures, giving decision‑makers confidence that they understand the range of outcomes and the drivers behind them.  

**Business Questions**  

- What are the plausible futures the enterprise could face?  
- How robust is the current plan across these futures?  
- What are the key uncertainties that most affect outcomes?  
- How trustworthy are our simulations and their outputs?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑001 | Scenario Intelligence Effectiveness (Reserved) |  
| PI‑SN‑002 | Plan Robustness Score |  
| PI‑SN‑101 | Scenario Understanding Index |  
| PI‑SN‑106 | Explainability Score (Scenario) |  

---

## BO‑SN‑002 — Improve Plan Robustness and Resilience  

**Business Motivation**  

A plan that is optimal under one forecast may be fragile under another. Scenario Intelligence shall evaluate and improve the robustness of enterprise plans — demand plans, supply plans, and promise strategies — so that they perform acceptably across a wide range of conditions, not just the most likely one.  

**Business Questions**  

- How does the current plan perform under adverse scenarios?  
- Where are the single points of failure or fragility?  
- What plan adjustments would most improve robustness?  
- How resilient is the enterprise to demand and supply shocks?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑002 | Plan Robustness Score |  
| PI‑SN‑011 | Resilience Index |  
| PI‑SN‑007 | Stress Test Coverage |  

---

## BO‑SN‑003 — Minimize Enterprise Risk Exposure  

**Business Motivation**  

Every enterprise faces risks — demand volatility, supplier failures, capacity constraints, geopolitical events, regulatory changes. Scenario Intelligence shall identify, quantify, and stress‑test these risks, recommending mitigation strategies that reduce the enterprise’s exposure to unacceptable outcomes.  

**Business Questions**  

- What are the top risks facing the enterprise?  
- What is the financial and operational impact of each risk if it materializes?  
- Which mitigation actions offer the best return on investment?  
- How has the risk profile changed over time?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑003 | Risk Reduction Impact |  
| PI‑SN‑103 | Risk Prediction Accuracy |  
| PI‑SN‑104 | Decision Confidence Index (Scenario) |  

---

## BO‑SN‑004 — Optimize Strategic Decision Making  

**Business Motivation**  

Strategic decisions — capacity investments, network redesign, major contract commitments, inventory strategy shifts — involve irreversible commitments of resources. Scenario Intelligence shall provide the analytical foundation for these decisions by evaluating alternatives across multiple futures and recommending strategies that maximize value while controlling risk.  

**Business Questions**  

- What is the expected value of each strategic alternative?  
- Which alternative performs best in the worst‑case scenario?  
- What is the value of waiting for more information versus deciding now?  
- How sensitive is the recommendation to key assumptions?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑005 | Scenario Recommendation Adoption Rate |  
| PI‑SN‑006 | Forecast Value of Scenario Analysis |  
| PI‑SN‑010 | Cost of Delay Avoided |  
| PI‑SN‑015 | Strategic Alignment Score |  

---

## BO‑SN‑005 — Increase Scenario Planning Automation  

**Business Motivation**  

Routine scenario analysis — periodic plan stress‑testing, risk monitoring, sensitivity scans — shall be automated wherever possible. This frees strategic planners and decision‑makers to focus on novel scenarios, deep analysis, and collaborative exploration rather than mechanical simulation execution.  

**Business Questions**  

- Which scenarios can be defined, executed, and evaluated automatically?  
- What triggers should automatically initiate a scenario analysis?  
- How can we reduce the cycle time from trigger to recommendation?  
- What is the touchless scenario planning rate?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑004 | Scenario Analysis Cycle Time |  
| PI‑SN‑014 | Planning Cycle Time (Scenario) |  
| PI‑SN‑108 | What‑If Completeness |  

---

## BO‑SN‑006 — Enable Collaborative What‑If Exploration  

**Business Motivation**  

Scenario planning is inherently cross‑functional. Demand planners, supply planners, finance teams, and executives must explore scenarios together, challenge assumptions, and build consensus. Scenario Intelligence shall provide collaborative tools that allow multiple stakeholders to define, run, and interpret scenarios in a shared environment.  

**Business Questions**  

- Who is participating in scenario planning?  
- How quickly can a collaborative scenario workshop reach consensus?  
- What assumptions are being challenged by different stakeholders?  
- How effective is the collaborative process at surfacing blind spots?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑013 | Collaborative Scenario Participation Rate |  
| PI‑SN‑015 | Strategic Alignment Score |  
| PI‑SN‑109 | Scenario Diversity Index |  

---

## BO‑SN‑007 — Accelerate Response to Change  

**Business Motivation**  

When the unexpected occurs — a demand spike, a supply disruption, a competitor move — the enterprise must rapidly assess the impact and decide on a response. Scenario Intelligence shall sense these triggers, automatically run relevant scenarios, and provide decision‑ready analysis in hours, not days.  

**Business Questions**  

- How quickly can we assess the impact of an unexpected event?  
- What pre‑defined scenarios exist for common disruption types?  
- How fast can a recommendation reach the decision‑maker?  
- What is the lag between event detection and scenario completion?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑004 | Scenario Analysis Cycle Time |  
| PI‑SN‑010 | Cost of Delay Avoided |  
| PI‑SN‑014 | Planning Cycle Time (Scenario) |  

---

## BO‑SN‑008 — Continuously Improve Scenario Intelligence  

**Business Motivation**  

Scenario Intelligence shall continuously evolve by learning from the accuracy of past simulations, the outcomes of adopted recommendations, and the changing nature of enterprise risks. This objective ensures that scenario analysis becomes progressively more accurate, more timely, and more valuable.  

**Business Questions**  

- How accurate were our past scenario predictions?  
- Are our simulation models improving over time?  
- Which scenario types add the most value?  
- Where should we invest to improve scenario capability?  

**Success Measures**  

| PI | Name |  
|----|------|  
| PI‑SN‑008 | Scenario Accuracy |  
| PI‑SN‑102 | Simulation Accuracy |  
| PI‑SN‑105 | Recommendation Quality Index (Scenario) |  
| PI‑SN‑110 | Learning Effectiveness Index (Scenario) |  

---

# Chapter 3 — Enterprise Measurement Model  

## 3.1 Measurement Architecture  

The Enterprise Measurement Model defines every performance indicator used to evaluate Scenario Intelligence. Each indicator is a first‑class enterprise object with a unique identifier, complete definition, formula, interpretation, worked example, limitations, and relationships.  

**Three measurement tiers:**  

| Range | Tier | Purpose |
|-------|------|---------|
| PI‑SN‑001 – PI‑SN‑049 | Business Outcome Measures | Measure business value delivered |
| PI‑SN‑050 – PI‑SN‑099 | Reserved | Future expansion |
| PI‑SN‑100 – PI‑SN‑199 | Intelligence Measures | Measure intelligence quality |
| PI‑SN‑200 – PI‑SN‑299 | Operational Measures | Measure system performance |

**PI‑SN‑001** is reserved for a future composite index—Scenario Intelligence Effectiveness—to be derived after all underlying measures are defined.  

---

## 3.2 Business Outcome Measures  

### PI‑SN‑001 — Scenario Intelligence Effectiveness [RESERVED]  

This identifier is reserved for a future composite indicator that will aggregate Business Outcome Measures, Intelligence Measures, and Operational Measures into a single executive health score for the Scenario Intelligence domain. It cannot be defined until all underlying measures exist and their interactions are understood.  

---

### PI‑SN‑002 — Plan Robustness Score  

**Definition**  

Plan Robustness Score measures the degree to which a plan (demand plan, supply plan, or integrated business plan) performs acceptably across a defined set of scenarios. A robust plan delivers acceptable outcomes under a wide range of conditions, not just the most likely forecast.  

The score is calculated as the weighted percentage of scenarios in which the plan meets all defined performance thresholds (e.g., service level ≥ 95%, cost within budget, capacity utilization ≤ 98%). Higher scores indicate greater robustness.  

**Business Objectives**  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑002 Improve Plan Robustness and Resilience  

**Business Interpretation**  

| Value | Interpretation |
|-------|----------------|
| 90% – 100% | Highly robust — plan performs well under nearly all scenarios |
| 75% – 90% | Robust — plan performs well under most scenarios |
| 50% – 75% | Moderately robust — plan is sensitive to scenario conditions |
| Below 50% | Fragile — plan fails thresholds under many scenarios, requires redesign |

Thresholds are configurable by plan type and enterprise risk appetite.  

**Formula**  

Plan Robustness Score (%) = ( Σ (Scenario Weight × Pass Factor) ÷ Σ Scenario Weight ) × 100  

Where:  
- Scenario Weight = the relative likelihood or importance of the scenario (sum of weights typically = 1.0 or 100%). If all scenarios are equally weighted, each weight = 1 ÷ N.  
- Pass Factor = 1 if the plan meets all defined performance thresholds in that scenario; 0 otherwise. A partial pass factor (e.g., 0.5) may be used if the plan meets some but not all thresholds, per policy.  

Performance thresholds are defined per plan type and may include:  
- Service Level ≥ target (e.g., 95%)  
- Total Cost ≤ budget + tolerance (e.g., +5%)  
- Capacity Utilization ≤ maximum (e.g., 98%)  
- Promise Adherence ≥ target (e.g., 95%)  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Scenario Weight | Decimal | Relative weight of the scenario (probability or importance) |
| Pass Factor | Decimal (0 or 1) | 1 if plan meets all thresholds; 0 otherwise (or partial) |
| Number of Scenarios | Integer | Total scenarios included in the evaluation |

**Preconditions**  

- A set of defined scenarios with weights must exist  
- The plan must be simulated against each scenario  
- Performance thresholds must be defined for the plan type  

**Assumptions**  

- Scenario weights represent the best available probability or importance estimates  
- Thresholds are set at levels that truly distinguish acceptable from unacceptable performance  
- The set of scenarios adequately spans the range of plausible futures  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Scenario definitions with weights, Simulation results, Performance thresholds |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Rounding | Round Half Up |
| Aggregation Levels | Plan type, Business Unit, Enterprise |
| Frequency | Per scenario analysis cycle, Monthly |
| Performance Targets | Target ≥90%, Warning 75–90%, Critical <75% (configurable) |
| Business Owner | Strategic Planning / Supply Chain |
| Business Consumers | Supply Chain Director, CFO, Executive Management |
| System Consumers | Dashboards, Reports |
| Derived From | Scenario simulation results |
| Related PIs | PI‑SN‑011 Resilience Index, PI‑SN‑007 Stress Test Coverage |

**Worked Example**  

**Plan: 2026 Q3 Supply Plan**  
**Performance Thresholds:** Service Level ≥ 95%, Total Cost ≤ $5.5M, Capacity Utilization ≤ 98%  

| Scenario | Weight | Service Level | Cost ($M) | Capacity Util. | All Thresholds Met? | Pass Factor |
|----------|--------|---------------|-----------|----------------|---------------------|-------------|
| Base Case | 50% | 97% | 5.1 | 92% | Yes | 1 |
| Upside Demand | 25% | 94% | 5.6 | 98% | No (Service + Cost) | 0 |
| Supplier Disruption | 15% | 91% | 5.4 | 85% | No (Service) | 0 |
| Recession | 10% | 99% | 4.8 | 78% | Yes | 1 |

Weighted Pass = (0.50 × 1) + (0.25 × 0) + (0.15 × 0) + (0.10 × 1) = 0.60  

Plan Robustness Score = 0.60 × 100 = **60.0%**  

Business Interpretation: **Moderately robust** — the plan fails under upside demand and supplier disruption scenarios.  

**Limitations**  

- The score depends entirely on the set of scenarios defined; missing an important risk scenario overstates robustness  
- Binary pass/fail may not capture the degree of failure (a plan that barely misses a threshold is treated the same as one that misses catastrophically). Partial pass factors can address this but add complexity  
- Scenario weights are subjective; sensitivity analysis on weights is recommended  

**Relationships**  

| Relationship | Reference |
|--------------|-----------|
| Supports | BO‑SN‑001, BO‑SN‑002 |
| Complemented By | PI‑SN‑011 Resilience Index |
| Displayed In | Scenario Analysis Dashboard |
| Used By | Strategic Planning, S&OP, Risk Management |

---

### PI‑SN‑003 — Risk Reduction Impact  

**Definition**  

Risk Reduction Impact measures the degree to which a recommended mitigation action reduces the enterprise’s risk exposure. It compares the risk score (probability × impact) of a given risk before and after the mitigation is applied.  

**Business Objectives**  

- BO‑SN‑003 Minimize Enterprise Risk Exposure  

**Business Interpretation**  

| Value (Reduction %) | Interpretation |
|---------------------|----------------|
| > 50% | Highly effective mitigation |
| 25% – 50% | Effective mitigation |
| 10% – 25% | Moderate mitigation |
| < 10% | Marginal mitigation — consider alternative |

**Formula**  

Risk Reduction Impact (%) = ( (Risk Score Before − Risk Score After) ÷ Risk Score Before ) × 100  

Where Risk Score = Probability (%) × Impact ($ or other impact unit).  
Probability and Impact are estimated for each risk before and after the proposed mitigation.  

**Formula Variables**  

| Variable | Type | Definition |
|----------|------|-------------|
| Risk Score Before | Decimal | Probability × Impact before mitigation |
| Risk Score After | Decimal | Probability × Impact after mitigation (estimated) |
| Probability | Percentage | Likelihood of risk occurrence (0–100%) |
| Impact | Currency or Score | Financial or operational impact if risk occurs |

**Preconditions**  

- Risk must be identified and quantified (probability and impact)  
- Mitigation action must be defined and its effect on probability and/or impact estimated  

**Assumptions**  

- Probability and impact estimates are credible (based on historical data or expert judgment)  
- Mitigation effects can be isolated from other changes  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Risk register, Mitigation proposals |
| Unit | Percentage (%) |
| Precision | One decimal place |
| Frequency | Per mitigation evaluation |
| Business Owner | Risk Management |
| Related PIs | PI‑SN‑103 Risk Prediction Accuracy |

**Worked Example**  

**Risk: Supplier S5 bankruptcy**  
- Before Mitigation: Probability = 20%, Impact = $2,000,000 → Risk Score = $400,000  
- Mitigation: Dual‑source with Supplier S6, reducing probability to 5%  
- After Mitigation: Probability = 5%, Impact = $2,000,000 → Risk Score = $100,000  

Risk Reduction Impact = ( ($400,000 − $100,000) ÷ $400,000 ) × 100 = **75.0%**  

Business Interpretation: **Highly effective mitigation**.  

---

### PI‑SN‑004 — Scenario Analysis Cycle Time  

**Definition**  

Scenario Analysis Cycle Time measures the total elapsed time from the initiation of a scenario analysis (triggered by an event or scheduled) to the delivery of the final recommendation or report to decision‑makers. It reflects the speed of the scenario planning process.  

**Business Objectives**  

- BO‑SN‑005 Increase Scenario Planning Automation  
- BO‑SN‑007 Accelerate Response to Change  

**Formula**  

Scenario Analysis Cycle Time = Time(Recommendation Delivered) − Time(Scenario Triggered)  

Measured in hours for routine scenarios, minutes for automated scenarios triggered by real‑time events.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Timestamps: trigger event, recommendation delivery |
| Unit | Hours (or minutes for automated) |
| Precision | Minutes |
| Frequency | Per scenario cycle |
| Performance Targets | Automated: < 5 min; Manual workshop: < 8 business hours |
| Business Owner | Strategic Planning |

**Worked Example**  

Trigger: Supply disruption alert received at 09:00.  
Recommendation delivered to Supply Chain Director at 09:45.  

Cycle Time = 45 minutes.  

---

### PI‑SN‑005 — Scenario Recommendation Adoption Rate  

**Definition**  

Scenario Recommendation Adoption Rate measures the percentage of scenario‑based recommendations that are accepted and adopted by decision‑makers. High adoption indicates trust in the scenario analysis; low adoption may indicate credibility gaps or misalignment with business judgment.  

**Business Objectives**  

- BO‑SN‑004 Optimize Strategic Decision Making  

**Formula**  

Adoption Rate (%) = ( Number of Recommendations Adopted ÷ Total Number of Recommendations Made ) × 100  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Recommendation log, Adoption decisions |
| Unit | Percentage (%) |
| Frequency | Monthly, Quarterly |
| Performance Targets | Target ≥80% |
| Business Owner | Strategic Planning |

**Worked Example**  

Q3: 15 scenario recommendations made; 12 adopted, 2 rejected, 1 pending.  

Adoption Rate = (12 ÷ 15) × 100 = **80.0%** (pending excluded).  

---

### PI‑SN‑006 — Forecast Value of Scenario Analysis  

**Definition**  

Forecast Value of Scenario Analysis quantifies the estimated financial benefit derived from scenario planning activities. It compares the expected outcome of decisions made with scenario analysis against the expected outcome of decisions that would have been made using only the base case forecast.  

**Business Objectives**  

- BO‑SN‑004 Optimize Strategic Decision Making  

**Formula**  

Value of Scenario Analysis = Expected Value (with scenario analysis) − Expected Value (without scenario analysis)  

Where Expected Value = Σ (Scenario Probability × Outcome Value) for each decision path.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Scenario probabilities, Outcome values, Decision records |
| Unit | Currency |
| Frequency | Quarterly, Annually |
| Business Owner | Finance / Strategic Planning |

---

### PI‑SN‑007 — Stress Test Coverage  

**Definition**  

Stress Test Coverage measures the percentage of critical plan elements (key products, suppliers, resources, customers) that are included in at least one stress test scenario during the evaluation period. It reflects the breadth of scenario analysis.  

**Business Objectives**  

- BO‑SN‑002 Improve Plan Robustness and Resilience  

**Formula**  

Stress Test Coverage (%) = ( Number of Critical Elements Stress‑Tested ÷ Total Number of Critical Elements ) × 100  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Critical element register, Scenario definitions |
| Unit | Percentage (%) |
| Frequency | Quarterly |
| Performance Targets | Target ≥90% |
| Business Owner | Risk Management |

---

### PI‑SN‑008 — Scenario Accuracy  

**Definition**  

Scenario Accuracy measures how closely the outcomes predicted by a scenario simulation matched the actual outcomes when (and if) that scenario materialized. It is the scenario equivalent of forecast accuracy.  

**Business Objectives**  

- BO‑SN‑008 Continuously Improve Scenario Intelligence  

**Formula**  

Scenario Accuracy (%) = ( 1 − |Predicted Outcome − Actual Outcome| ÷ Actual Outcome ) × 100  

Measured for key outcome variables (revenue, cost, service level, etc.) and aggregated.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Scenario predictions, Actual outcomes |
| Unit | Percentage (%) |
| Frequency | Per scenario that materializes, Quarterly review |
| Performance Targets | Target ≥85% |
| Business Owner | Strategic Planning |

---

### PI‑SN‑009 — Decision Confidence Improvement  

**Definition**  

Decision Confidence Improvement measures the increase in decision‑maker confidence attributable to scenario analysis. Confidence is self‑reported by decision‑makers before and after reviewing scenario analysis.  

**Business Objectives**  

- BO‑SN‑004 Optimize Strategic Decision Making  

**Formula**  

Confidence Improvement (pp) = Average Confidence (after scenario review) − Average Confidence (before scenario review)  

Measured on a scale (e.g., 1–10) and reported as percentage points.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Decision‑maker surveys |
| Unit | Percentage points |
| Frequency | Per major decision |
| Business Owner | Strategic Planning |

---

### PI‑SN‑010 — Cost of Delay Avoided  

**Definition**  

Cost of Delay Avoided estimates the financial loss prevented by accelerating a decision through scenario analysis compared to a counterfactual where analysis took longer or was not performed.  

**Business Objectives**  

- BO‑SN‑004 Optimize Strategic Decision Making  
- BO‑SN‑007 Accelerate Response to Change  

**Formula**  

Cost of Delay Avoided = (Loss per Day of Delay) × (Days Saved by Scenario Analysis)  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Decision timeline, Financial impact estimates |
| Unit | Currency |
| Frequency | Per event |
| Business Owner | Finance |

---

### PI‑SN‑011 — Resilience Index  

**Definition**  

Resilience Index measures the enterprise’s ability to maintain acceptable performance levels during and after a disruption. It is derived from stress test simulations and measures the speed and completeness of recovery.  

**Business Objectives**  

- BO‑SN‑002 Improve Plan Robustness and Resilience  

**Formula**  

Resilience Index = (1 − (Performance Loss During Disruption ÷ Normal Performance)) × Recovery Speed Factor  

Recovery Speed Factor = 1 − (Time to Recover ÷ Maximum Acceptable Recovery Time).  

Values close to 1.0 indicate high resilience.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Stress test simulation results |
| Unit | Index (0–1) |
| Frequency | Per stress test |
| Business Owner | Risk Management |

---

### PI‑SN‑012 — Scenario Comparison Completeness  

**Definition**  

Scenario Comparison Completeness measures whether all defined comparison criteria were evaluated for every scenario in a comparison set. Incomplete comparisons may miss important trade‑offs.  

**Business Objectives**  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  

**Formula**  

Comparison Completeness (%) = ( Number of Criteria Actually Evaluated ÷ Total Number of Defined Criteria ) × 100  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Comparison templates, Evaluation records |
| Unit | Percentage (%) |
| Frequency | Per comparison |
| Performance Targets | Target = 100% |
| Business Owner | Strategic Planning |

---

### PI‑SN‑013 — Collaborative Scenario Participation Rate  

**Definition**  

Collaborative Scenario Participation Rate measures the percentage of identified stakeholders who actively participated in a collaborative scenario planning process.  

**Business Objectives**  

- BO‑SN‑006 Enable Collaborative What‑If Exploration  

**Formula**  

Participation Rate (%) = ( Number of Stakeholders Who Participated ÷ Total Number of Invited Stakeholders ) × 100  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Stakeholder list, Participation records |
| Unit | Percentage (%) |
| Frequency | Per scenario workshop |
| Business Owner | Strategic Planning |

---

### PI‑SN‑014 — Planning Cycle Time (Scenario)  

**Definition**  

Planning Cycle Time measures the total elapsed time for a complete scenario planning cycle, from initial scenario definition through simulation, comparison, recommendation, and stakeholder review.  

**Formula**  

Planning Cycle Time = Time(Cycle Closed) − Time(Cycle Initiated)  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Cycle timestamps |
| Unit | Days |
| Frequency | Per cycle |
| Business Owner | Strategic Planning |

---

### PI‑SN‑015 — Strategic Alignment Score  

**Definition**  

Strategic Alignment Score measures the degree to which scenario‑based recommendations align with the enterprise’s stated strategic objectives. Each recommendation is scored against strategic criteria by decision‑makers or an AI‑assisted alignment check.  

**Business Objectives**  

- BO‑SN‑004 Optimize Strategic Decision Making  
- BO‑SN‑006 Enable Collaborative What‑If Exploration  

**Formula**  

Strategic Alignment Score (%) = ( Σ Alignment Score per Recommendation ÷ Number of Recommendations ) × 100  

Where each recommendation is scored 1–5 on strategic alignment, normalized to 0–100%.  

**Specification**  

| Attribute | Value |
|-----------|-------|
| Inputs | Recommendation records, Strategic objectives |
| Unit | Percentage (%) |
| Frequency | Quarterly |
| Business Owner | Strategic Planning |

---

## 3.3 Intelligence Measures (Stubs)  

| PI | Name | Description |
|----|------|-------------|
| PI‑SN‑101 | Scenario Understanding Index | Composite of scenario data quality and completeness. Reserved. |
| PI‑SN‑102 | Simulation Accuracy | Accuracy of simulation outputs vs. actual outcomes. Reserved. |
| PI‑SN‑103 | Risk Prediction Accuracy | Accuracy of probability and impact estimates. Reserved. |
| PI‑SN‑104 | Decision Confidence Index (Scenario) | Average confidence across scenario decisions. Reserved. |
| PI‑SN‑105 | Recommendation Quality Index (Scenario) | Quality of scenario recommendations. Reserved. |
| PI‑SN‑106 | Explainability Score (Scenario) | Completeness of scenario explanations. Reserved. |
| PI‑SN‑107 | Sensitivity Analysis Coverage | Percentage of key variables analyzed. Reserved. |
| PI‑SN‑108 | What‑If Completeness | Percentage of required what‑if dimensions explored. Reserved. |
| PI‑SN‑109 | Scenario Diversity Index | Variety of scenarios in the catalogue. Reserved. |
| PI‑SN‑110 | Learning Effectiveness Index (Scenario) | Rate of improvement in scenario accuracy. Reserved. |
| PI‑SN‑111 | Probability Calibration Score | How well predicted probabilities match observed frequencies. Reserved. |

---

## 3.4 Operational Measures (Stubs)  

| PI | Name | Description |
|----|------|-------------|
| PI‑SN‑201 | Simulation Execution Time | Time to complete a simulation run. Reserved. |
| PI‑SN‑202 | Scenario Data Refresh Latency | Time to update scenario inputs. Reserved. |
| PI‑SN‑203 | API Response Time (Scenario) | 95th percentile API latency. Reserved. |
| PI‑SN‑204 | System Availability (Scenario) | Uptime of scenario services. Reserved. |
| PI‑SN‑205 | Event Processing Latency (Scenario) | Time to process scenario events. Reserved. |

---

# Chapter 4 — Semantic Foundation  

The following concepts establish the enterprise meaning upon which all Scenario Intelligence capabilities operate. Each concept is a first‑class enterprise object with a unique identifier and a complete definition. This chapter mirrors the structure of the Demand, Supply, and Promise Semantic Foundations, specialized for scenario planning.

## 4.1 Core Enterprise Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑001 | Scenario | A coherent, internally consistent description of a possible future state of the enterprise and its environment. A scenario is defined by a set of assumptions about key variables—demand, supply, capacity, external factors—and produces a projected set of enterprise outcomes when simulated against a plan. |
| SE‑SN‑002 | Simulation | The computational process of projecting enterprise outcomes (service levels, costs, inventory, capacity utilization, financials) given a defined plan and a set of scenario assumptions. A simulation may be deterministic (single outcome) or probabilistic (distribution of outcomes). |
| SE‑SN‑003 | Plan Variant | A specific version of an enterprise plan—demand plan, supply plan, inventory policy, allocation rule, or integrated business plan—that is the subject of scenario evaluation. The baseline plan is the current adopted plan; alternative plans are what‑if variants. |
| SE‑SN‑004 | Scenario Outcome | The projected enterprise performance metrics resulting from simulating a plan variant against a scenario. Outcomes include KPIs such as service level, total cost, revenue, inventory turns, and risk exposure. |
| SE‑SN‑005 | Scenario Trigger | An event or condition that initiates a scenario analysis. Triggers may be scheduled (periodic plan review), event‑driven (supply disruption, demand shock), or user‑initiated (strategic what‑if inquiry). |

## 4.2 Scenario Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑010 | Scenario Definition | The formal specification of a scenario, including its name, description, type, horizon, the set of assumptions (variable overrides), and the plan variants to evaluate. The scenario definition is the input to the simulation engine. |
| SE‑SN‑011 | Scenario Type | A classification of scenarios by purpose: Baseline (most likely), Upside (optimistic), Downside (pessimistic), Stress Test (extreme but plausible), Strategic (major change in business model or network), Event‑Driven (response to a specific trigger). |
| SE‑SN‑012 | Scenario Horizon | The future time span covered by the scenario, aligned with the planning horizon of the plan being evaluated. Horizons may be short‑term (1–12 weeks, operational), medium‑term (3–18 months, tactical), or long‑term (1–5 years, strategic). |
| SE‑SN‑013 | Scenario Status | The lifecycle state of a scenario: Draft, Defined, Simulating, Simulated, Compared, Recommended, Adopted, Archived. |
| SE‑SN‑014 | Scenario Assumption | A specific parameter override applied in a scenario. Assumptions modify baseline plan inputs: demand quantities (±X%), supply lead times, capacity availability, supplier reliability, cost rates, exchange rates, regulatory changes. |
| SE‑SN‑015 | Scenario Trigger | As defined in 4.1. Additional detail: Triggers carry metadata including trigger type (Scheduled, Event, Manual), priority (Routine, Urgent, Critical), and the scope of scenario analysis to initiate. |

## 4.3 Simulation Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑020 | Simulation Engine | The computational component that executes scenario simulations. It consumes a plan variant and scenario assumptions, applies the enterprise model (constraints, BOM, routings, lead times, costs), and produces scenario outcomes. |
| SE‑SN‑021 | Simulation Type | The method used for simulation: Deterministic (single‑point assumptions produce a single outcome), Sensitivity (one variable varied across a range), Probabilistic (distributions on multiple variables, e.g., Monte Carlo, produce a distribution of outcomes), Optimization‑Under‑Uncertainty (search for best plan variant across scenarios). |
| SE‑SN‑022 | Simulation Result | The output of a single simulation run: the projected values of all defined KPIs for a specific plan variant under a specific scenario assumption set. |
| SE‑SN‑023 | Simulation Confidence | A score (0–100%) reflecting the reliability of the simulation output, derived from input data quality, model accuracy, and historical simulation accuracy for similar scenarios. |
| SE‑SN‑024 | Probabilistic Outcome | The output of a probabilistic simulation: a probability distribution of each KPI (e.g., service level distribution, cost distribution) rather than a single point estimate. Includes summary statistics: mean, median, P10, P90, and value‑at‑risk. |
| SE‑SN‑025 | Simulation Run | A single execution of the simulation engine for a specific plan variant, scenario, and set of assumptions. Each run has a unique identifier and is reproducible. |

## 4.4 Plan Variant Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑030 | Baseline Plan | The current adopted enterprise plan against which scenarios are compared. It represents the "do nothing different" or "business as usual" reference point. |
| SE‑SN‑031 | Alternative Plan | A proposed modification to the baseline plan being evaluated through scenarios. Alternatives may involve capacity changes, inventory policy adjustments, sourcing strategy shifts, network reconfiguration, or pricing changes. |
| SE‑SN‑032 | Recommended Plan | The plan variant that emerges from scenario analysis as the preferred choice based on defined comparison criteria (robustness, value, risk). A recommendation is the primary output of the Recommend Scenario capability. |
| SE‑SN‑033 | Adopted Plan | A recommended plan that has been formally approved by decision‑makers and transmitted to the operational domains (Demand, Supply, Promise) for execution. Adoption closes the scenario‑to‑action loop. |
| SE‑SN‑034 | Plan Variant Lineage | The traceability chain showing the origin and evolution of a plan variant: which baseline it derived from, which scenario assumptions were applied, which recommendations were made, and who approved adoption. |

## 4.5 Risk Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑040 | Risk Factor | A variable or event that can cause enterprise outcomes to deviate from the baseline plan. Risk factors include demand volatility, supplier reliability, capacity availability, commodity price fluctuations, geopolitical events, and regulatory changes. |
| SE‑SN‑041 | Risk Event | A specific occurrence of a risk factor, defined by its timing, magnitude, and duration. Scenarios may include one or more risk events (e.g., "Supplier S5 ceases operations in Q3 for 8 weeks"). |
| SE‑SN‑042 | Risk Score | A quantified measure of risk: Risk Score = Probability (%) × Impact (financial, service, or operational). Used to rank risks and prioritize mitigation. |
| SE‑SN‑043 | Risk Mitigation | An action or strategy that reduces the probability or impact of a risk. Mitigations may be preventive (reducing likelihood) or contingent (reducing impact if the risk occurs). Scenarios evaluate the effectiveness of mitigations. |
| SE‑SN‑044 | Stress Test | A scenario specifically designed to evaluate the enterprise's resilience under extreme but plausible conditions. Stress tests push variables beyond normal ranges (e.g., demand drops 40%, all sole‑source suppliers fail simultaneously) to identify breaking points. |
| SE‑SN‑045 | Risk Appetite | The level of risk the enterprise is willing to accept in pursuit of its objectives. Expressed as thresholds on KPIs (e.g., "service level must not drop below 85% in any scenario"). Risk appetite constrains scenario recommendations. |

## 4.6 Comparison Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑050 | Scenario Comparison | The side‑by‑side evaluation of multiple plan variants across multiple scenarios against a defined set of comparison criteria. Comparison enables trade‑off analysis and decision support. |
| SE‑SN‑051 | Comparison Criteria | The dimensions on which plan variants are evaluated: Service Level, Total Cost, Revenue, Inventory Investment, Capacity Utilization, Robustness Score, Risk Exposure, Sustainability, Strategic Alignment. Criteria may be weighted. |
| SE‑SN‑052 | Trade‑Off Analysis | The evaluation of how improving performance on one criterion degrades performance on another (e.g., higher service level vs. higher inventory cost). Trade‑off analysis identifies the efficient frontier of plan variants. |
| SE‑SN‑053 | Pareto Frontier | The set of plan variants for which no other variant is better on all criteria. The frontier represents the efficient choices; the decision‑maker selects among frontier variants based on strategic preference. |

## 4.7 Sensitivity Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑060 | Sensitivity Variable | A key input variable whose impact on outcomes is systematically tested by varying it across a defined range while holding other variables constant. Common sensitivity variables: demand growth rate, material cost, lead time, exchange rate. |
| SE‑SN‑061 | Sensitivity Range | The low‑to‑high range over which a sensitivity variable is tested. Ranges may be expressed as percentages (e.g., ±20%) or absolute values. |
| SE‑SN‑062 | Sensitivity Impact | The degree to which changes in a sensitivity variable affect key outcomes. Typically visualized as a tornado chart, ranking variables by impact magnitude. |
| SE‑SN‑063 | Tornado Chart | A visualization that ranks sensitivity variables by their impact on a selected KPI, showing the range of outcomes as horizontal bars sorted from largest to smallest impact. The chart resembles a tornado shape. |

## 4.8 Collaboration Concepts  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑070 | Scenario Stakeholder | A person or role with an interest in scenario outcomes and the authority to contribute assumptions, evaluate results, or make decisions. Stakeholders include demand planners, supply planners, finance managers, and executives. |
| SE‑SN‑071 | Scenario Workshop | A structured collaborative session where stakeholders define scenarios, review simulation results, discuss trade‑offs, and build consensus on recommendations. Workshops may be physical, virtual, or AI‑facilitated. |
| SE‑SN‑072 | Collaborative Scenario | A scenario that has been jointly defined or reviewed by multiple stakeholders, with documented contributions, challenges, and consensus decisions. |
| SE‑SN‑073 | Consensus Scenario | A scenario that has achieved formal agreement among stakeholders as the basis for a recommendation or decision. Consensus is documented with any dissenting views recorded. |

## 4.9 Scenario Relationships  

| ID | Concept | Definition |
|----|---------|-------------|
| SE‑SN‑080 | Scenario Dependency | A relationship where the definition or outcome of one scenario depends on another. For example, a "Mitigated Supplier Failure" scenario depends on the "Unmitigated Supplier Failure" scenario as its baseline for comparison. |
| SE‑SN‑081 | Scenario Hierarchy | The organization of scenarios into parent‑child relationships. A parent scenario (e.g., "Economic Downturn") may have child scenarios varying specific assumptions (e.g., "Mild Recession", "Deep Recession"). |
| SE‑SN‑082 | Scenario Version | A specific iteration of a scenario definition, tracked over time. As assumptions, models, or plans evolve, scenarios are versioned to maintain traceability and reproducibility. |
| SE‑SN‑083 | Scenario Lineage | The complete history of a scenario: when it was created, what versions exist, which plan variants were evaluated, what recommendations were made, and whether those recommendations were adopted. Lineage supports audit and learning. |

## 4.10 Common Enumerations  

**Scenario Type**  

| Value | Description |
|-------|-------------|
| Baseline | The most likely future; current plan assumptions maintained |
| Upside | Optimistic assumptions (higher demand, lower costs, better supply) |
| Downside | Pessimistic assumptions (lower demand, higher costs, worse supply) |
| Stress Test | Extreme but plausible conditions to test plan limits |
| Strategic | Major structural change (new market, M&A, network redesign) |
| Event‑Driven | Response to a specific trigger (disruption, opportunity) |
| Sensitivity | Systematic variation of one or more variables |

**Scenario Status**  

| Value | Description |
|-------|-------------|
| Draft | Scenario definition in progress |
| Defined | Scenario fully specified, ready for simulation |
| Simulating | Simulation execution in progress |
| Simulated | Simulation complete, results available |
| Compared | Multi‑scenario comparison complete |
| Recommended | Recommendation made to decision‑makers |
| Adopted | Recommendation approved and transmitted for execution |
| Archived | Scenario retained for audit and learning |

**Simulation Type**  

| Value | Description |
|-------|-------------|
| Deterministic | Single‑point assumptions, single outcome |
| Sensitivity | One variable varied across a range |
| Probabilistic | Multiple variables with probability distributions (Monte Carlo) |
| Optimization‑Under‑Uncertainty | Search for best plan variant across scenarios |

**Risk Level**  

| Value | Description |
|-------|-------------|
| Critical | Risk score in top 10% of all assessed risks, or impact exceeds critical threshold |
| High | Significant probability and impact; active mitigation required |
| Medium | Moderate probability or impact; monitor and plan mitigation |
| Low | Low probability and impact; accept or monitor |

**Comparison Method**  

| Value | Description |
|-------|-------------|
| Weighted Score | Criteria weighted and summed; highest score wins |
| Pareto Frontier | Identify efficient variants; decision‑maker selects |
| Robustness First | Rank by robustness score, then by expected value |
| Minimax | Select variant with best worst‑case outcome |
| Consensus | Stakeholder discussion leads to agreed preference |

---

# Chapter 5 — Enterprise Capability Specifications  

## 5.1 Define Scenarios  

### 5.1.1 Purpose  

Create, manage, and govern the catalogue of scenarios that represent the range of plausible futures the enterprise may face. Answers: *“What possible futures should we evaluate, and what assumptions define them?”* The capability ensures that every scenario is well‑formed, internally consistent, and aligned with enterprise risk appetite and strategic objectives. It maintains the scenario catalogue as an authoritative, versioned knowledge asset.

### 5.1.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑002 Improve Plan Robustness and Resilience  
- BO‑SN‑003 Minimize Enterprise Risk Exposure  

### 5.1.3 Enterprise Measures  

- PI‑SN‑007 Stress Test Coverage  
- PI‑SN‑109 Scenario Diversity Index  
- PI‑SN‑012 Scenario Comparison Completeness (indirectly, by providing complete scenario definitions)  

### 5.1.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Core output |
| SE‑SN‑010 | Scenario Definition | Specification |
| SE‑SN‑011 | Scenario Type | Classification |
| SE‑SN‑012 | Scenario Horizon | Time span |
| SE‑SN‑013 | Scenario Status | Lifecycle |
| SE‑SN‑014 | Scenario Assumption | Parameter override |
| SE‑SN‑015 | Scenario Trigger | Initiation condition |
| SE‑SN‑044 | Stress Test | Extreme scenario |
| SE‑SN‑081 | Scenario Hierarchy | Parent‑child |
| SE‑SN‑082 | Scenario Version | Version tracking |
| SE‑SN‑083 | Scenario Lineage | History |

### 5.1.5 Primitive Capabilities Composed  

- **Understand** – interprets risk factors, strategic objectives, and historical scenario performance  
- **Assess** – evaluates scenario completeness and consistency  
- **Evaluate** – selects the most relevant scenarios for the catalogue  

### 5.1.6 Enterprise Inputs  

- Enterprise strategic objectives and risk appetite  
- Risk register with identified risk factors  
- Historical scenario catalogue and performance  
- External environmental data (market forecasts, geopolitical risk indices, regulatory outlook)  
- Stakeholder input and scenario requests  
- Planning calendars and horizon definitions  

### 5.1.7 Enterprise Understanding Produced  

- A structured, versioned catalogue of active and archived scenarios  
- Each scenario fully specified: type, horizon, assumptions, triggers, and lineage  
- Coverage assessment: which risks and plan elements are addressed by the catalogue  
- Scenario consistency checks: no contradictory assumptions within a scenario  

### 5.1.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑001 | Scenario Definition | Complete, validated scenario specification |
| OUT‑SN‑002 | Scenario Catalogue | Active and archived scenarios with metadata |
| OUT‑SN‑003 | Coverage Assessment | Gaps in risk or plan element coverage |

### 5.1.9 Preconditions  

- Enterprise risk register and strategic objectives are available  
- Scenario taxonomy (types, horizons) is defined  
- Stakeholder roles and authorities are assigned  

### 5.1.10 Capability Dependencies  

None. This is the foundational scenario capability.

### 5.1.11 Collaborating Capabilities  

- **Simulate Scenarios** – consumes scenario definitions for execution  
- **Compare Scenarios** – consumes catalogue for comparison setup  
- **Learn From Scenarios** – consumes lineage for accuracy tracking  

### 5.1.12 Business Decisions  

---

#### DE‑SN‑010 — Create Scenario Definition  

**Purpose:** Define a new scenario or a new version of an existing scenario, specifying its type, horizon, assumptions, and triggers.  

**Required Understanding:** Strategic context, risk register, existing scenario catalogue, stakeholder requirements.  

**Decision Alternatives:**  
- Create new standalone scenario  
- Create child scenario under an existing parent  
- Create new version of an existing scenario (revise assumptions)  
- Reject (duplicate or out‑of‑scope)  

**Decision Criteria:** Scenario addresses at least one identified risk or strategic question; assumptions are internally consistent; horizon matches the plan being tested; scenario type is appropriate for the purpose.  

**Decision Confidence:** Based on completeness of input data and consensus among stakeholders.  

**Decision Rationale:** *Explainability Template:* “Scenario ‘Q3 Upside Demand’ defined: Type = Upside, Horizon = Q3 2026. Assumptions: demand +15%, supplier lead times unchanged, capacity at 110% via overtime. Addresses risk of demand exceeding supply capacity. Rule BR‑SN‑010 passed.”  

---

##### Rules (for DE‑SN‑010)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑010 | Scenario Completeness Rule | Validation Rule | A scenario definition must include: type, horizon, at least one assumption that distinguishes it from the baseline, and a stated purpose (risk or question addressed). Incomplete definitions are saved as Draft. |
| BR‑SN‑011 | Assumption Consistency Rule | Consistency Rule | Assumptions within a scenario must not be contradictory (e.g., demand increase cannot simultaneously assume a recession). The system checks predefined consistency rules. |
| BR‑SN‑012 | Duplicate Detection Rule (Scenario) | Validation Rule | A scenario with identical type, horizon, and assumptions (±5% tolerance) to an existing active scenario is flagged as a potential duplicate and held for review. |

##### Policies (for DE‑SN‑010)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑010 | Scenario Creation Authorization Policy | Authorization Policy | Strategic scenarios (type = Strategic) require VP or Director approval before activation. Operational scenarios may be created by authorized planners. |

---

#### DE‑SN‑011 — Approve Scenario for Simulation  

**Purpose:** Validate that a scenario definition is complete, consistent, and appropriate for simulation, and transition its status from Draft or Defined to ready for execution.  

**Required Understanding:** Scenario definition, validation results, stakeholder sign‑offs where required.  

**Decision Alternatives:**  
- Approve (ready for simulation)  
- Return for revision (issues identified)  
- Reject (not appropriate for catalogue)  

**Decision Criteria:** All validation rules passed; required approvals obtained; scenario aligns with current strategic objectives and risk appetite.  

**Decision Confidence:** High if all validations passed and approvals are complete.  

**Decision Rationale:** “Scenario ‘Q3 Upside Demand’ approved for simulation: all rules passed, Planner and Demand Manager approvals obtained. Status updated to Defined.”  

---

##### Rules (for DE‑SN‑011)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑013 | Approval Gate Rule | Validation Rule | A scenario must pass all validation rules (BR‑SN‑010, BR‑SN‑011, BR‑SN‑012) and have required approvals per policy PO‑SN‑010 before it can be approved for simulation. |
| BR‑SN‑014 | Strategic Alignment Rule | Consistency Rule | Strategic scenarios must explicitly reference the strategic objective they address, and that objective must be current (not superseded). |

##### Policies (for DE‑SN‑011)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑011 | Scenario Approval Workflow Policy | Approval Policy | Operational scenarios require one approver (Planner or Manager). Strategic scenarios require two approvers (Director + Finance or VP). |

---

#### DE‑SN‑012 — Publish Scenario to Catalogue  

**Purpose:** Finalize an approved scenario definition and add it (or its new version) to the active scenario catalogue, making it available for simulation and comparison.  

**Required Understanding:** Approved scenario definition, catalogue version, impact on coverage metrics.  

**Decision Alternatives:**  
- Publish as active (add to catalogue)  
- Publish as replacement (supersede a prior version)  
- Archive (move to inactive)  

**Decision Criteria:** Scenario is approved; catalogue coverage is improved; no duplication of active scenarios.  

**Decision Confidence:** High.  

**Decision Rationale:** “Scenario ‘Q3 Upside Demand v2’ published to catalogue, superseding v1. Catalogue now covers 92% of identified risks.”  

---

##### Rules (for DE‑SN‑012)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑015 | Catalogue Versioning Rule | Compliance Rule | Every publication increments the catalogue version. Previous versions are retained for audit. |
| BR‑SN‑016 | Coverage Update Rule | Derivation Rule | Upon publication, the coverage assessment (PI‑SN‑007 relevant metrics) is recalculated. |

##### Policies (for DE‑SN‑012)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑012 | Catalogue Review Policy | Compliance Policy | The scenario catalogue shall be reviewed quarterly. Scenarios not used in the last 12 months are candidates for archiving. |

---

### 5.1.13 Functional Behaviour  

1. **Trigger:** Scheduled quarterly review, on risk register update, on stakeholder request, on strategic planning cycle initiation.  
2. **Retrieve** risk register, strategic objectives, current catalogue, external data.  
3. **Create or revise** scenario definitions via DE‑SN‑010 — rules BR‑SN‑010/011/012, policy PO‑SN‑010.  
4. **Validate and approve** via DE‑SN‑011 — rules BR‑SN‑013/014, policy PO‑SN‑011.  
5. **Publish** to catalogue via DE‑SN‑012 — rules BR‑SN‑015/016, policy PO‑SN‑012.  
6. **Update** coverage metrics.  
7. **Raise events:** `ScenarioDefined`, `ScenarioApproved`, `ScenarioPublished`.  

### 5.1.14 Commands  

| Command | Purpose |
|---------|---------|
| `CreateScenario` | Create a new scenario definition |
| `ReviseScenario` | Create a new version of an existing scenario |
| `ApproveScenario` | Validate and approve a scenario for simulation |
| `PublishScenario` | Add approved scenario to the active catalogue |
| `ArchiveScenario` | Move a scenario to archived status |

### 5.1.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioDefined` | Scenario ID, type, horizon, assumptions |
| `ScenarioApproved` | Scenario ID, approver, timestamp |
| `ScenarioPublished` | Scenario ID, catalogue version |
| `ScenarioArchived` | Scenario ID, reason |

### 5.1.16 Queries  

| Query | Description |
|-------|-------------|
| `GetScenario(scenarioId)` | Full scenario definition |
| `GetScenarioCatalogue(filter)` | Active scenarios by type, horizon, status |
| `GetCoverageAssessment()` | Risk and plan element coverage |

### 5.1.17 Reports  

- **Scenario Catalogue Report** – all active scenarios with metadata  
- **Coverage Gap Report** – risks and plan elements not yet covered by scenarios  

### 5.1.18 Dashboards  

- **Scenario Catalogue Dashboard** – scenario inventory, status, coverage gauges  
- **Scenario Lineage Viewer** – version history and evolution  

### 5.1.19 Software Realization  

```
API → Application Service → Domain Model (ScenarioDefinition, ScenarioCatalogue)  
→ Rule Engine (consistency, duplication checks)  
→ Event Store → Projections (CatalogueView) → Read Model
```  
Scenario definitions are stored with full version history. Consistency rules are configurable and hot‑reloadable.

---

## 5.2 Simulate Scenarios  

### 5.2.1 Purpose  

Execute scenario simulations against defined plan variants to project enterprise outcomes under varying assumptions. Answers: *“What would happen if this scenario occurred?”* The capability runs deterministic, sensitivity, and probabilistic simulations, producing quantitative projections of KPIs—service levels, costs, inventory, capacity utilization, and financials—for each plan variant under each scenario.  

### 5.2.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑002 Improve Plan Robustness and Resilience  
- BO‑SN‑005 Increase Scenario Planning Automation  
- BO‑SN‑007 Accelerate Response to Change  

### 5.2.3 Enterprise Measures  

- PI‑SN‑002 Plan Robustness Score (computed by Compare Scenarios using simulation outputs)  
- PI‑SN‑004 Scenario Analysis Cycle Time  
- PI‑SN‑102 Simulation Accuracy  
- PI‑SN‑201 Simulation Execution Time  

### 5.2.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑002 | Simulation | Process |
| SE‑SN‑020 | Simulation Engine | Engine |
| SE‑SN‑021 | Simulation Type | Method |
| SE‑SN‑022 | Simulation Result | Output per run |
| SE‑SN‑023 | Simulation Confidence | Confidence |
| SE‑SN‑024 | Probabilistic Outcome | Distribution output |
| SE‑SN‑025 | Simulation Run | Execution record |
| SE‑SN‑003 | Plan Variant | Input plan |
| SE‑SN‑004 | Scenario Outcome | Output KPIs |
| SE‑SN‑014 | Scenario Assumption | Input overrides |

### 5.2.5 Primitive Capabilities Composed  

- **Understand** – interprets plan variant and scenario assumptions  
- **Predict** – projects outcomes using the simulation engine  
- **Evaluate** – assesses simulation quality and confidence  

### 5.2.6 Enterprise Inputs  

- Scenario definitions with assumptions (from Define Scenarios)  
- Plan variants: baseline plan and alternative plans (from Demand, Supply, Promise, or defined directly)  
- Enterprise model: supply network, BOMs, routings, capacities, lead times, costs (from Supply Intelligence master data)  
- Simulation configuration: type, number of iterations (for probabilistic), random seed  

### 5.2.7 Enterprise Understanding Produced  

- Simulation results: projected KPI values for each plan variant under each scenario  
- Probabilistic outcome distributions (for Monte Carlo simulations): P10, P50, P90, value‑at‑risk  
- Simulation confidence scores  
- Execution metadata: run ID, timestamp, engine version, input data freshness  

### 5.2.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑010 | Simulation Result Set | All KPI projections for a plan‑scenario combination |
| OUT‑SN‑011 | Probabilistic Distribution | Distribution of outcomes per KPI |
| OUT‑SN‑012 | Simulation Confidence Score | Confidence in the simulation output |
| OUT‑SN‑013 | Simulation Run Record | Execution metadata for reproducibility |

### 5.2.9 Preconditions  

- Scenario definitions are approved and published  
- Plan variants are specified and accessible  
- Enterprise model data is current and complete  
- Simulation engine is operational and calibrated  

### 5.2.10 Capability Dependencies  

- `CA‑SN‑001 Define Scenarios` – for scenario definitions  
- `CA‑DI‑002 Forecast Demand` – for demand plan variants  
- `CA‑SI‑002 Plan Supply` – for supply plan variants  
- `CA‑PI‑002 Promise Orders` – for promise plan variants  

### 5.2.11 Collaborating Capabilities  

- **Compare Scenarios** – consumes simulation results for comparison  
- **Assess Risks** – consumes results for risk scoring  
- **Recommend Scenario** – consumes results for recommendation  
- **Evaluate Scenario Quality** – consumes results and actuals for accuracy assessment  

### 5.2.12 Business Decisions  

---

#### DE‑SN‑020 — Select Simulation Method  

**Purpose:** Choose the appropriate simulation type (deterministic, sensitivity, probabilistic, optimization‑under‑uncertainty) based on the scenario purpose, available data, time budget, and required output detail.  

**Required Understanding:** Scenario type, horizon, criticality, number of variables with uncertainty, available computational resources, decision timeline.  

**Decision Alternatives:**  
- Deterministic (fast, single‑point)  
- Sensitivity (vary one variable, identify key drivers)  
- Probabilistic (Monte Carlo with distributions, full risk profile)  
- Optimization‑Under‑Uncertainty (search for best plan across scenarios)  

**Decision Criteria:**  
- Routine operational scenario → Deterministic or Sensitivity.  
- Strategic decision with significant investment → Probabilistic or Optimization.  
- Time available: Deterministic < 1 min; Probabilistic may take hours.  
- Number of uncertain variables: >3 with significant impact → Probabilistic.  

**Decision Confidence:** Based on appropriateness of method for the scenario and historical simulation accuracy.  

**Decision Rationale:** “Probabilistic simulation selected for ‘Supplier Disruption’ scenario: multiple uncertain variables (supplier recovery time, demand during disruption, spot market price), strategic decision impact $5M+, acceptable run time 2 hours. Rule BR‑SN‑020 applied.”  

---

##### Rules (for DE‑SN‑020)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑020 | Simulation Method Selection Rule | Derivation Rule | If scenario type = Stress Test or Strategic AND expected impact > $1M → Probabilistic. If scenario is Event‑Driven with response time < 1 hour → Deterministic. Otherwise, Sensitivity. Configurable matrix. |
| BR‑SN‑021 | Resource Budget Rule | Constraint Rule | Probabilistic simulations must complete within the defined time budget (default 2 hours for strategic, 30 minutes for tactical). If the model size exceeds capability, the simulation is downgraded to Sensitivity with a warning. |

##### Policies (for DE‑SN‑020)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑020 | Method Override Policy | Authorization Policy | A senior planner or strategist may override the selected simulation method with documented justification. |

---

#### DE‑SN‑021 — Execute Simulation Run  

**Purpose:** Run the simulation engine for a specific plan variant and scenario, producing a set of projected outcomes.  

**Required Understanding:** Plan variant data, scenario assumptions, selected simulation method, engine configuration.  

**Decision Alternatives:** The execution itself is deterministic once configured. The decision is whether to accept the run results or flag for re‑execution due to errors or low confidence.  

**Decision Criteria:** Simulation completes without errors; output data is within plausible bounds (no negative inventory without explanation, no impossible lead times); confidence score meets minimum threshold (≥60%).  

**Decision Confidence:** Computed by the engine based on input data quality and model fit.  

**Decision Rationale:** “Simulation run SR‑4401 completed: Plan ‘Q3 Base’ vs. Scenario ‘Upside Demand’. 10,000 iterations, 22 minutes. Confidence 91%. All outputs within plausible bounds. Rule BR‑SN‑022 passed.”  

---

##### Rules (for DE‑SN‑021)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑022 | Output Plausibility Rule | Validation Rule | Simulation outputs must pass plausibility checks: no negative inventory (unless explicitly allowed for backorders), no service level >100%, no capacity utilization >200%. Violations cause automatic re‑run with adjusted parameters or flagging. |
| BR‑SN‑023 | Confidence Threshold Rule | Validation Rule | If simulation confidence < 60%, the run is flagged as “Low Confidence” and results carry a warning. If < 40%, results are suppressed and the run is queued for investigation. |
| BR‑SN‑024 | Reproducibility Rule | Compliance Rule | Every simulation run must be reproducible: all inputs, engine version, random seed, and configuration are recorded. |

##### Policies (for DE‑SN‑021)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑021 | Simulation Retry Policy | Automation Policy | Failed or low‑confidence runs are automatically retried once with a different random seed (probabilistic) or after data refresh (deterministic). If the retry also fails, the run is escalated to a simulation specialist. |

---

#### DE‑SN‑022 — Generate Probabilistic Summary  

**Purpose:** For probabilistic simulations, compute summary statistics and risk metrics from the distribution of outcomes.  

**Required Understanding:** Raw simulation output (iteration results), required summary metrics.  

**Decision Alternatives:** Deterministic output generation — no choice.  

**Decision Criteria:** Compute mean, median, standard deviation, P10, P90, value‑at‑risk (VaR), conditional value‑at‑risk (CVaR) for each KPI.  

**Decision Confidence:** Based on number of iterations (convergence check).  

**Decision Rationale:** “Probabilistic summary for SR‑4401: Service Level mean 94.2%, P10 91.1%, VaR(95) 89.5%. Convergence achieved at 10,000 iterations (standard error <0.1%).”  

---

##### Rules (for DE‑SN‑022)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑025 | Convergence Rule | Validation Rule | Probabilistic simulation must achieve convergence: the standard error of the mean for the primary KPI must be <1% of the mean, or the maximum configured iterations must be reached. |
| BR‑SN‑026 | Summary Completeness Rule | Validation Rule | Probabilistic summary must include: mean, median, P10, P90, VaR at configured confidence level (default 95%), and a convergence indicator. |

##### Policies (for DE‑SN‑022)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑022 | VaR Confidence Level Policy | Compliance Policy | Value‑at‑Risk is computed at 95% confidence by default. For strategic decisions involving >$10M, 99% VaR is also required. |

---

### 5.2.13 Functional Behaviour  

1. **Trigger:** Scheduled (after plan publication), on scenario publication, on disruption event (event‑driven), on manual request.  
2. **Retrieve** scenario definition, plan variant(s), enterprise model data.  
3. **Execute DE‑SN‑020** (Select Simulation Method) — rules BR‑SN‑020/021, policy PO‑SN‑020.  
4. **Execute DE‑SN‑021** (Execute Simulation Run) for each plan‑scenario combination — rules BR‑SN‑022/023/024, policy PO‑SN‑021.  
5. **If probabilistic**, execute DE‑SN‑022 (Generate Probabilistic Summary) — rules BR‑SN‑025/026, policy PO‑SN‑022.  
6. **Store** simulation results with full metadata for traceability.  
7. **Raise events:** `SimulationStarted`, `SimulationCompleted`, `SimulationFailed`.  

### 5.2.14 Commands  

| Command | Purpose |
|---------|---------|
| `StartSimulation` | Initiate a simulation run for a given scenario and plan |
| `RetrySimulation` | Re‑run a failed or low‑confidence simulation |
| `CancelSimulation` | Abort a running simulation |

### 5.2.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `SimulationStarted` | Run ID, scenario ID, plan variant, method |
| `SimulationCompleted` | Run ID, results summary, confidence |
| `SimulationFailed` | Run ID, reason, error details |

### 5.2.16 Queries  

| Query | Description |
|-------|-------------|
| `GetSimulationResult(runId)` | Full results for a run |
| `GetProbabilisticSummary(runId)` | Distribution summary |
| `GetSimulationStatus(runId)` | Current status of a run |

### 5.2.17 Reports  

- **Simulation Execution Report** – run times, success rates, confidence distributions  
- **Probabilistic Analysis Report** – VaR, CVaR, outcome distributions  

### 5.2.18 Dashboards  

- **Simulation Monitor** – real‑time run status, queue depth, resource utilization  
- **Scenario Outcome Explorer** – interactive distribution charts for probabilistic results  

### 5.2.19 Software Realization  

```
API → Application Service → Domain Model (SimulationRun, SimulationResult)  
→ Simulation Engine (pluggable: LP solver, Monte Carlo, discrete event)  
→ Enterprise Model Adapter (queries Supply, Demand, Promise for plan data)  
→ Event Store → Projections (ResultStore) → Read Model
```  
The simulation engine is pluggable. Deterministic runs use the same optimization engines as Supply Intelligence. Monte Carlo runs distribute iterations across a compute cluster. All runs are reproducible via stored inputs and seeds.

---

## 5.3 Compare Scenarios  

### 5.3.1 Purpose  

Evaluate multiple plan variants across multiple scenarios side‑by‑side against a defined set of weighted comparison criteria. Answers: *“Which plan performs best across the range of possible futures, and what are the trade‑offs?”* The capability transforms raw simulation results into structured comparisons, identifies the Pareto‑efficient frontier of plan variants, and highlights where improving one objective degrades another.  

### 5.3.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑002 Improve Plan Robustness and Resilience  
- BO‑SN‑004 Optimize Strategic Decision Making  

### 5.3.3 Enterprise Measures  

- PI‑SN‑002 Plan Robustness Score  
- PI‑SN‑012 Scenario Comparison Completeness  
- PI‑SN‑015 Strategic Alignment Score  

### 5.3.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑050 | Scenario Comparison | Core output |
| SE‑SN‑051 | Comparison Criteria | Evaluation dimensions |
| SE‑SN‑052 | Trade‑Off Analysis | Conflict between criteria |
| SE‑SN‑053 | Pareto Frontier | Efficient choices |
| SE‑SN‑004 | Scenario Outcome | Input results |
| SE‑SN‑022 | Simulation Result | Input data |
| SE‑SN‑024 | Probabilistic Outcome | Distribution input |

### 5.3.5 Primitive Capabilities Composed  

- **Understand** – interprets simulation results and criteria definitions  
- **Evaluate** – ranks plan variants against criteria  
- **Assess** – determines robustness and trade‑offs  

### 5.3.6 Enterprise Inputs  

- Simulation results for all plan variants and scenarios (from Simulate Scenarios)  
- Comparison criteria definitions with weights  
- Risk appetite thresholds (from enterprise policy)  
- Strategic objectives for alignment scoring  

### 5.3.7 Enterprise Understanding Produced  

- Ranked list of plan variants by composite score, robustness, and expected value  
- Pareto frontier of non‑dominated variants  
- Trade‑off matrices: cost vs. service, inventory vs. risk, etc.  
- Sensitivity of rankings to criteria weights  

### 5.3.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑020 | Comparison Matrix | Plan variants × scenarios × criteria scores |
| OUT‑SN‑021 | Plan Ranking | Ranked list with composite, robustness, and expected value scores |
| OUT‑SN‑022 | Pareto Frontier | Set of efficient plan variants |
| OUT‑SN‑023 | Trade‑Off Report | Key trade‑offs with visualization data |

### 5.3.9 Preconditions  

- Simulation results are available for all scenarios and plan variants to be compared  
- Comparison criteria and weights are defined and approved  
- Risk appetite thresholds are current  

### 5.3.10 Capability Dependencies  

- `CA‑SN‑002 Simulate Scenarios` – for simulation results  

### 5.3.11 Collaborating Capabilities  

- **Recommend Scenario** – consumes comparison results for recommendation  
- **Collaborate on Scenarios** – shares comparison for workshop discussions  

### 5.3.12 Business Decisions  

---

#### DE‑SN‑030 — Select Comparison Method and Criteria  

**Purpose:** Choose the comparison methodology and the set of weighted criteria against which plan variants will be evaluated.  

**Required Understanding:** Decision context (strategic vs. operational), stakeholder preferences, simulation outputs available.  

**Decision Alternatives:**  
- Weighted Score (criteria weighted and summed; highest score wins)  
- Pareto Frontier (identify efficient variants; decision‑maker selects)  
- Robustness First (rank by robustness score, then by expected value)  
- Minimax (select variant with best worst‑case outcome)  
- Custom (combination of methods)  

**Decision Criteria:**  
- Strategic decisions with multiple stakeholders → Pareto Frontier with weighted score as secondary.  
- Operational decisions with clear KPI priority → Weighted Score.  
- High uncertainty, risk‑averse → Minimax or Robustness First.  

**Decision Confidence:** Based on appropriateness for the decision context.  

**Decision Rationale:** “Pareto Frontier method selected for Q3 supply plan comparison: multiple stakeholders (Supply Chain, Finance, Sales) with different KPI priorities. Weighted Score will be provided as secondary input. Rule BR‑SN‑030 applied.”  

---

##### Rules (for DE‑SN‑030)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑030 | Comparison Method Selection Rule | Derivation Rule | If decision involves >2 stakeholder groups and impact >$1M → Pareto Frontier + Weighted Score. If single stakeholder operational → Weighted Score. If worst‑case risk is primary concern → Minimax. |
| BR‑SN‑031 | Criteria Completeness Rule | Validation Rule | At least three comparison criteria must be defined, covering financial, service, and risk dimensions. |

##### Policies (for DE‑SN‑030)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑030 | Criteria Weight Approval Policy | Authorization Policy | Criteria weights for strategic comparisons require Finance and Supply Chain Director approval. |

---

#### DE‑SN‑031 — Execute Comparison  

**Purpose:** Compute the comparison matrix, rank plan variants, and identify the Pareto frontier.  

**Required Understanding:** Simulation results, selected method, weighted criteria.  

**Decision Alternatives:** Deterministic computation.  

**Decision Criteria:** All plan‑scenario combinations evaluated against all criteria.  

**Decision Confidence:** Based on simulation confidence scores of underlying data.  

**Decision Rationale:** “Comparison complete: 5 plan variants × 6 scenarios evaluated. Plan Variant ‘Flex Capacity’ ranks #1 on robustness (82%), #2 on expected value ($5.3M). Pareto frontier contains 3 variants.”  

---

##### Rules (for DE‑SN‑031)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑032 | Normalization Rule | Calculation Rule | Criteria values are normalized to 0–1 scale before weighted combination. Financial criteria normalized by range; service criteria by target. |
| BR‑SN‑033 | Data Completeness Rule | Validation Rule | Comparison is valid only if ≥95% of all plan‑scenario combinations have completed simulations. Missing data is flagged. |

##### Policies (for DE‑SN‑031)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑031 | Comparison Publication Policy | Automation Policy | Comparisons that meet completeness and confidence thresholds are automatically published to the comparison dashboard. |

---

#### DE‑SN‑032 — Publish Comparison Report  

**Purpose:** Compile the comparison results into a structured report for decision‑makers.  

**Required Understanding:** Comparison results, trade‑off analysis, sensitivity to weights.  

**Decision Alternatives:** Publish as final, Publish with caveats, Hold.  

**Decision Rationale:** “Comparison Report CR‑2026‑Q3 published: Plan ‘Flex Capacity’ recommended for robustness; trade‑off with cost documented.”  

---

##### Rules (for DE‑SN‑032)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑034 | Report Completeness Rule | Validation Rule | Comparison report must include: ranking, Pareto frontier, trade‑off analysis for top 3 criteria pairs, and sensitivity of ranking to ±10% weight changes. |

##### Policies (for DE‑SN‑032)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑032 | Report Distribution Policy | Compliance Policy | Strategic comparison reports are distributed to all stakeholders at least 5 business days before decision meetings. |

---

### 5.3.13 Functional Behaviour  

1. **Trigger:** After simulation completion for all required plan‑scenario combinations.  
2. **Retrieve** simulation results, criteria definitions, risk appetite thresholds.  
3. **Execute DE‑SN‑030** (Select Method and Criteria) — rules BR‑SN‑030/031, policy PO‑SN‑030.  
4. **Execute DE‑SN‑031** (Execute Comparison) — rules BR‑SN‑032/033, policy PO‑SN‑031.  
5. **Execute DE‑SN‑032** (Publish Comparison Report) — rule BR‑SN‑034, policy PO‑SN‑032.  
6. **Raise events:** `ComparisonCompleted`, `ComparisonReportPublished`.  

### 5.3.14 Commands  

| Command | Purpose |
|---------|---------|
| `StartComparison` | Initiate comparison for a set of plan variants and scenarios |
| `PublishComparisonReport` | Finalize and distribute report |

### 5.3.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ComparisonCompleted` | Comparison ID, method, ranking summary |
| `ComparisonReportPublished` | Report ID, timestamp |

### 5.3.16 Queries  

| Query | Description |
|-------|-------------|
| `GetComparison(comparisonId)` | Full comparison results |
| `GetParetoFrontier(comparisonId)` | Efficient variants |
| `GetTradeOffAnalysis(comparisonId)` | Trade‑off data |

### 5.3.17 Reports  

- **Scenario Comparison Report** – ranking, Pareto frontier, trade‑offs  

### 5.3.18 Dashboards  

- **Comparison Workbench** – interactive ranking, Pareto chart, trade‑off visualizations  
- **Scenario Scorecard** – KPI comparison across variants and scenarios  

### 5.3.19 Software Realization  

```
API → Application Service → Domain Model (Comparison, ComparisonResult)  
→ Computation Engine (multi‑criteria decision analysis, Pareto solver)  
→ Event Store → Projections (ComparisonView) → Read Model
```  

---

## 5.4 Assess Risks  

### 5.4.1 Purpose  

Identify, quantify, and evaluate enterprise risks using scenario simulation outputs. Answers: *“What are our top risks, how severe are they, and what happens under extreme conditions?”* The capability transforms simulation distributions into risk scores, stress‑tests critical vulnerabilities, and recommends risk mitigation priorities.  

### 5.4.2 Business Objectives Served  

- BO‑SN‑003 Minimize Enterprise Risk Exposure  
- BO‑SN‑002 Improve Plan Robustness and Resilience  

### 5.4.3 Enterprise Measures  

- PI‑SN‑003 Risk Reduction Impact  
- PI‑SN‑103 Risk Prediction Accuracy  

### 5.4.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑040 | Risk Factor | Input variable |
| SE‑SN‑041 | Risk Event | Specific occurrence |
| SE‑SN‑042 | Risk Score | Output metric |
| SE‑SN‑043 | Risk Mitigation | Response strategy |
| SE‑SN‑044 | Stress Test | Extreme scenario |
| SE‑SN‑045 | Risk Appetite | Constraint |
| SE‑SN‑024 | Probabilistic Outcome | Distribution input |

### 5.4.5 Primitive Capabilities Composed  

- **Observe** – monitors risk indicators  
- **Understand** – interprets simulation outcomes as risk exposures  
- **Assess** – quantifies probability and impact  
- **Predict** – projects risk evolution  
- **Evaluate** – prioritizes risks and mitigations  

### 5.4.6 Enterprise Inputs  

- Simulation results (especially probabilistic distributions) from Simulate Scenarios  
- Risk register with identified risk factors  
- Stress test definitions (from Define Scenarios)  
- Risk appetite thresholds  
- Historical risk event data  

### 5.4.7 Enterprise Understanding Produced  

- Quantified risk scores (Probability × Impact) for each risk factor  
- Risk heatmap by category, business unit, and severity  
- Stress test results: enterprise performance under extreme scenarios  
- Mitigation prioritization: which risks to address first, ranked by ROI of mitigation  

### 5.4.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑030 | Risk Assessment Report | Risk scores, heatmap, top risks |
| OUT‑SN‑031 | Stress Test Results | Performance under extreme scenarios |
| OUT‑SN‑032 | Mitigation Prioritization | Ranked mitigation actions with estimated risk reduction |

### 5.4.9 Preconditions  

- Probabilistic simulation results available for key risks  
- Risk appetite thresholds defined  
- Stress test scenarios defined  

### 5.4.10 Capability Dependencies  

- `CA‑SN‑002 Simulate Scenarios` – for simulation outputs  
- `CA‑SN‑001 Define Scenarios` – for stress test definitions  

### 5.4.11 Collaborating Capabilities  

- **Recommend Scenario** – consumes risk assessments for recommendation  
- **Sense Scenario Triggers** – risk assessments may trigger new scenarios  

### 5.4.12 Business Decisions  

---

#### DE‑SN‑040 — Compute Risk Scores  

**Purpose:** Calculate quantified risk scores for each identified risk factor using simulation outcome distributions.  

**Required Understanding:** Probabilistic simulation results, risk factor definitions, impact measurement methodology.  

**Decision Alternatives:** Deterministic computation.  

**Decision Criteria:** Risk Score = Probability × Impact. Probability derived from simulation outcome distribution (e.g., % of iterations where KPI exceeds risk threshold). Impact = financial loss, service degradation, or other defined metric.  

**Decision Confidence:** Based on simulation convergence and data quality.  

**Decision Rationale:** “Risk ‘Supplier S5 Failure’ scored 85 (Critical): Probability 17% × Impact $5M loss. Derived from Monte Carlo simulation of supplier disruption scenario.”  

---

##### Rules (for DE‑SN‑040)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑040 | Risk Score Calculation Rule | Calculation Rule | Risk Score = Probability (%) × Impact (financial or scaled 1–10). Probability derived from simulation: percentage of iterations where the risk event causes KPI to exceed threshold. |
| BR‑SN‑041 | Risk Level Classification Rule | Derivation Rule | Critical: score ≥ 80 or top 10%. High: 50–79. Medium: 20–49. Low: <20. Configurable thresholds. |

##### Policies (for DE‑SN‑040)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑040 | Risk Quantification Policy | Compliance Policy | All risks with estimated impact >$500K must be quantified via probabilistic simulation, not expert judgment alone. |

---

#### DE‑SN‑041 — Execute Stress Test  

**Purpose:** Run and evaluate a stress test scenario to determine the enterprise’s breaking point and recovery capability.  

**Required Understanding:** Stress test scenario definition, simulation results, risk appetite thresholds.  

**Decision Alternatives:**  
- Pass (enterprise maintains acceptable performance)  
- Marginal (performance degraded but recoverable)  
- Fail (performance drops below acceptable threshold; action required)  

**Decision Criteria:** All KPIs remain above risk appetite thresholds → Pass. Any KPI drops below → Fail. Recovery time within acceptable window → Marginal if thresholds breached but recovered quickly.  

**Decision Rationale:** “Stress test ‘All Sole‑Source Suppliers Fail’: Fail. Service level drops to 62% (threshold 85%), recovery time 8 weeks (threshold 4 weeks). Rule BR‑SN‑042 triggered.”  

---

##### Rules (for DE‑SN‑041)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑042 | Stress Test Pass/Fail Rule | Validation Rule | A stress test fails if any KPI breaches the risk appetite threshold defined for that scenario type. |
| BR‑SN‑043 | Stress Test Recovery Rule | Derivation Rule | Recovery is measured as time for the KPI to return to within 10% of pre‑stress baseline. Recovery > maximum acceptable time triggers a mandatory mitigation review. |

##### Policies (for DE‑SN‑041)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑041 | Stress Test Escalation Policy | Exception Policy | Failed stress tests are escalated to the Risk Committee within 24 hours with recommended mitigations. |

---

#### DE‑SN‑042 — Prioritize Risk Mitigations  

**Purpose:** Rank proposed risk mitigations by their expected risk reduction per unit cost, and recommend the highest‑value mitigation portfolio.  

**Required Understanding:** Risk scores before and after mitigation, mitigation cost, implementation feasibility.  

**Decision Alternatives:** Ranked list of mitigations.  

**Decision Criteria:** Mitigation ROI = (Risk Score Before − Risk Score After) ÷ Mitigation Cost. Rank descending.  

**Decision Rationale:** “Mitigation ‘Dual‑source Supplier S5’ ranked #1: reduces risk score from 85 to 21 (64 points), cost $300K, ROI 21.3 points/$100K.”  

---

##### Rules (for DE‑SN‑042)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑044 | Mitigation ROI Rule | Calculation Rule | Mitigation ROI = Risk Reduction (score points) ÷ Mitigation Cost ($100K units). |
| BR‑SN‑045 | Critical Risk Mitigation Rule | Validation Rule | All Critical risks must have at least one proposed mitigation within 30 days of identification. |

##### Policies (for DE‑SN‑042)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑042 | Mitigation Approval Policy | Authorization Policy | Mitigations costing >$1M require CFO approval. All others require Risk Manager approval. |

---

### 5.4.13 Functional Behaviour  

1. **Trigger:** After probabilistic simulations complete, after stress test execution, quarterly risk review.  
2. **Retrieve** simulation distributions, risk register, stress test results.  
3. **Execute DE‑SN‑040** (Compute Risk Scores) — rules BR‑SN‑040/041, policy PO‑SN‑040.  
4. **Execute DE‑SN‑041** (Execute Stress Test) for each stress test scenario — rules BR‑SN‑042/043, policy PO‑SN‑041.  
5. **Execute DE‑SN‑042** (Prioritize Risk Mitigations) — rules BR‑SN‑044/045, policy PO‑SN‑042.  
6. **Raise events:** `RiskAssessmentCompleted`, `StressTestCompleted`, `MitigationPrioritized`.  

### 5.4.14 Commands  

| Command | Purpose |
|---------|---------|
| `AssessRisk` | Run risk assessment for a set of risk factors |
| `ExecuteStressTest` | Execute and evaluate a stress test |
| `PrioritizeMitigations` | Rank mitigations |

### 5.4.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `RiskAssessmentCompleted` | Risk ID, score, level |
| `StressTestCompleted` | Scenario ID, pass/fail, KPI values |
| `MitigationPrioritized` | Mitigation ID, ROI, rank |

### 5.4.16 Queries  

| Query | Description |
|-------|-------------|
| `GetRiskAssessment(period)` | Current risk scores and levels |
| `GetStressTestResults(scenarioId)` | Stress test pass/fail and KPIs |
| `GetMitigationPlan()` | Prioritized mitigations |

### 5.4.17 Reports  

- **Risk Assessment Report** – risk heatmap, top risks, trends  
- **Stress Test Report** – scenario results, breaking points  

### 5.4.18 Dashboards  

- **Risk Heatmap Dashboard** – visual risk scores by category  
- **Stress Test Monitor** – pass/fail status, recovery projections  

### 5.4.19 Software Realization  

```
API → Application Service → Domain Model (RiskAssessment, StressTest)  
→ Computation Engine (probability extraction, risk scoring)  
→ Event Store → Projections → Read Model
```  

---

## 5.5 Recommend Scenario  

### 5.5.1 Purpose  

Synthesize comparison results, risk assessments, and strategic objectives into a clear, actionable recommendation for decision‑makers. Answers: *“Which plan should we adopt, and why?”* The capability evaluates the trade‑offs, applies the enterprise’s decision criteria and risk appetite, and produces a recommendation with supporting rationale, confidence, and traceability.  

### 5.5.2 Business Objectives Served  

- BO‑SN‑004 Optimize Strategic Decision Making  
- BO‑SN‑003 Minimize Enterprise Risk Exposure  

### 5.5.3 Enterprise Measures  

- PI‑SN‑005 Scenario Recommendation Adoption Rate  
- PI‑SN‑009 Decision Confidence Improvement  
- PI‑SN‑015 Strategic Alignment Score  

### 5.5.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑032 | Recommended Plan | Output |
| SE‑SN‑033 | Adopted Plan | Adopted variant |
| SE‑SN‑053 | Pareto Frontier | Input |
| SE‑SN‑051 | Comparison Criteria | Input |
| SE‑SN‑045 | Risk Appetite | Constraint |
| SE‑SN‑042 | Risk Score | Input |

### 5.5.5 Primitive Capabilities Composed  

- **Understand** – interprets comparison, risk, and strategic context  
- **Evaluate** – applies decision rules to select preferred variant  
- **Assess** – verifies recommendation feasibility  

### 5.5.6 Enterprise Inputs  

- Comparison results, ranking, Pareto frontier (from Compare Scenarios)  
- Risk assessment and stress test results (from Assess Risks)  
- Strategic objectives and risk appetite  
- Stakeholder input and preferences (from Collaborate on Scenarios)  

### 5.5.7 Enterprise Understanding Produced  

- Recommended plan variant with rationale and confidence  
- Alternative options considered and rejected with reasons  
- Implementation pathway (which operational domains must update their plans)  
- Expected outcomes and residual risks of the recommended plan  

### 5.5.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑040 | Scenario Recommendation | Recommended plan variant with full rationale |
| OUT‑SN‑041 | Decision Brief | Executive summary for decision‑makers |
| OUT‑SN‑042 | Implementation Pathway | Steps to operationalize the recommendation |

### 5.5.9 Preconditions  

- Comparison and risk assessment are complete  
- Strategic objectives and risk appetite are current  
- Stakeholder review (if required) is complete  

### 5.5.10 Capability Dependencies  

- `CA‑SN‑003 Compare Scenarios` – for comparison results  
- `CA‑SN‑004 Assess Risks` – for risk assessments  
- `CA‑SN‑006 Collaborate on Scenarios` – for stakeholder input  

### 5.5.11 Collaborating Capabilities  

- **Operational domains (Demand, Supply, Promise)** – receive adopted plans  
- **Learn From Scenarios** – receives recommendation outcomes for learning  

### 5.5.12 Business Decisions  

---

#### DE‑SN‑050 — Generate Recommendation  

**Purpose:** Apply the enterprise’s decision logic to select the recommended plan variant from the efficient set.  

**Required Understanding:** Pareto frontier, robustness scores, expected values, risk scores, strategic alignment, risk appetite constraints.  

**Decision Alternatives:**  
- Recommend the top‑ranked variant from the comparison  
- Recommend a different variant (based on risk appetite override or strategic alignment)  
- Recommend no change (baseline remains best)  
- Defer recommendation (insufficient information)  

**Decision Criteria:** The recommended variant must satisfy all risk appetite constraints, be on the Pareto frontier, and have the highest composite score (or strategic alignment score if strategy is prioritized).  

**Decision Confidence:** Based on simulation confidence, comparison completeness, and stakeholder consensus.  

**Decision Rationale:** *Explainability Template:* “We recommend adopting Plan Variant ‘Flex Capacity’ because: (1) It ranks #1 on robustness (82%), performing acceptably in 5 of 6 scenarios. (2) Expected value is $5.3M, within 3% of the highest‑value option. (3) It satisfies all risk appetite constraints (service level ≥85% in worst‑case). (4) The alternative ‘Max Service’ has higher expected value but fails the stress test (service level 62% under supplier disruption). Rule BR‑SN‑050 applied.”  

---

##### Rules (for DE‑SN‑050)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑050 | Recommendation Selection Rule | Derivation Rule | The recommended variant must be on the Pareto frontier, satisfy all risk appetite constraints, and have the highest robustness score. If multiple variants tie, select the one with higher expected value. |
| BR‑SN‑051 | Risk Appetite Compliance Rule | Validation Rule | No variant that breaches any risk appetite threshold (in any scenario) may be recommended unless the decision‑maker explicitly overrides. |
| BR‑SN‑052 | Baseline Comparison Rule | Validation Rule | The recommendation must include a comparison to the baseline plan, quantifying the expected improvement. If improvement <2% on all primary criteria, “No Change” may be recommended. |

##### Policies (for DE‑SN‑050)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑050 | Recommendation Approval Policy | Approval Policy | Strategic recommendations (>$5M impact) require VP and CFO approval. Operational recommendations require Director approval. |
| PO‑SN‑051 | Override Policy | Authorization Policy | Decision‑makers may override the recommendation with a documented rationale. Overrides are tracked and analyzed for learning. |

---

#### DE‑SN‑051 — Adopt Recommended Plan  

**Purpose:** Formalize the adoption of a recommended plan variant and transmit the plan changes to the operational domains for execution.  

**Required Understanding:** Approved recommendation, plan variant details, implementation pathway.  

**Decision Alternatives:**  
- Adopt and transmit to operational domains  
- Adopt with phased implementation  
- Reject (decision‑maker declines)  

**Decision Criteria:** Recommendation is approved per policy PO‑SN‑050.  

**Decision Rationale:** “Plan Variant ‘Flex Capacity’ adopted. Implementation: Demand Intelligence to update demand plan assumptions; Supply Intelligence to adjust capacity parameters and safety stock targets. Transmitted via events.”  

---

##### Rules (for DE‑SN‑051)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑053 | Adoption Authorization Rule | Validation Rule | Adoption is permitted only if the recommendation has all required approvals per PO‑SN‑050. |
| BR‑SN‑054 | Plan Lineage Rule | Compliance Rule | Adoption creates a new plan version with full lineage: baseline → scenario assumptions → comparison → recommendation → approval. |

##### Policies (for DE‑SN‑051)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑052 | Adoption Transmission Policy | Compliance Policy | Adopted plan changes are transmitted to operational domains within 4 business hours of adoption. |

---

### 5.5.13 Functional Behaviour  

1. **Trigger:** After comparison and risk assessment completion, after stakeholder review.  
2. **Retrieve** comparison results, risk assessments, strategic objectives, risk appetite.  
3. **Execute DE‑SN‑050** (Generate Recommendation) — rules BR‑SN‑050/051/052, policies PO‑SN‑050/051.  
4. **Present** recommendation to decision‑makers with full rationale and supporting evidence.  
5. **If approved**, execute DE‑SN‑051 (Adopt Recommended Plan) — rules BR‑SN‑053/054, policy PO‑SN‑052.  
6. **Transmit** plan changes to Demand, Supply, and/or Promise domains via events.  
7. **Raise events:** `ScenarioRecommendationMade`, `ScenarioRecommendationAdopted`, `ScenarioRecommendationRejected`.  

### 5.5.14 Commands  

| Command | Purpose |
|---------|---------|
| `GenerateRecommendation` | Produce recommendation from comparison and risk data |
| `AdoptRecommendation` | Formalize adoption and transmit to operational domains |
| `RejectRecommendation` | Decline with reason |

### 5.5.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioRecommendationMade` | Recommendation ID, plan variant, rationale, confidence |
| `ScenarioRecommendationAdopted` | Recommendation ID, adopted plan version, transmission details |
| `ScenarioRecommendationRejected` | Recommendation ID, reason |

### 5.5.16 Queries  

| Query | Description |
|-------|-------------|
| `GetRecommendation(recommendationId)` | Full recommendation with rationale |
| `GetAdoptedPlans(period)` | Plans adopted via scenario analysis |

### 5.5.17 Reports  

- **Recommendation Summary Report** – all recommendations with adoption status  

### 5.5.18 Dashboards  

- **Recommendation Dashboard** – active recommendations, adoption rates, decision confidence  

### 5.5.19 Software Realization  

```
API → Application Service → Domain Model (Recommendation, PlanAdoption)  
→ Rule Engine (selection logic, risk appetite checks)  
→ Integration Adapter (transmits adopted plans to Demand, Supply, Promise APIs)  
→ Event Store → Projections → Read Model
```  

---

## 5.6 Collaborate on Scenarios  

### 5.6.1 Purpose  

Enable cross‑functional stakeholders to jointly define, explore, challenge, and reach consensus on scenarios and their implications. Answers: *“How do we bring the right people together to explore futures and build shared understanding?”* The capability provides the collaborative environment for scenario workshops—virtual or physical—where assumptions are debated, simulation results are reviewed, trade‑offs are discussed, and consensus recommendations emerge. It treats scenario planning as a social process, not just a computational one.  

### 5.6.2 Business Objectives Served  

- BO‑SN‑006 Enable Collaborative What‑If Exploration  
- BO‑SN‑004 Optimize Strategic Decision Making  

### 5.6.3 Enterprise Measures  

- PI‑SN‑013 Collaborative Scenario Participation Rate  
- PI‑SN‑015 Strategic Alignment Score  
- PI‑SN‑109 Scenario Diversity Index  

### 5.6.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑070 | Scenario Stakeholder | Participant |
| SE‑SN‑071 | Scenario Workshop | Structured session |
| SE‑SN‑072 | Collaborative Scenario | Jointly defined |
| SE‑SN‑073 | Consensus Scenario | Agreed outcome |
| SE‑SN‑010 | Scenario Definition | Subject of collaboration |
| SE‑SN‑050 | Scenario Comparison | Reviewed output |

### 5.6.5 Primitive Capabilities Composed  

- **Understand** – interprets stakeholder inputs and perspectives  
- **Assess** – evaluates consensus levels and dissent  

### 5.6.6 Enterprise Inputs  

- Scenario definitions and catalogue (from Define Scenarios)  
- Simulation and comparison results (from Simulate, Compare)  
- Stakeholder roster with roles and authorities  
- Previous workshop records and consensus decisions  

### 5.6.7 Enterprise Understanding Produced  

- Workshop records: participants, scenarios reviewed, assumptions challenged, consensus reached  
- Stakeholder feedback: assumptions questioned, alternatives proposed, concerns raised  
- Consensus metrics: degree of agreement, dissenting views documented  
- Action items and decisions from workshops  

### 5.6.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑050 | Workshop Record | Participants, agenda, scenarios reviewed, decisions |
| OUT‑SN‑051 | Stakeholder Feedback Log | Assumptions challenged, alternatives proposed |
| OUT‑SN‑052 | Consensus Statement | Agreed position with any dissenting views |
| OUT‑SN‑053 | Workshop Action Items | Tasks assigned with owners and deadlines |

### 5.6.9 Preconditions  

- Scenario definitions and simulation results are available for review  
- Stakeholder roster is current with correct roles and authorities  
- Workshop is scheduled with defined agenda  

### 5.6.10 Capability Dependencies  

- `CA‑SN‑001 Define Scenarios` – for scenario definitions  
- `CA‑SN‑003 Compare Scenarios` – for comparison results  
- `CA‑SN‑004 Assess Risks` – for risk assessments  

### 5.6.11 Collaborating Capabilities  

- **Recommend Scenario** – consumes consensus for recommendation  
- **Define Scenarios** – receives new scenario requests from workshops  

### 5.6.12 Business Decisions  

---

#### DE‑SN‑060 — Convene Scenario Workshop  

**Purpose:** Determine when a collaborative workshop is required, who must participate, and what scenarios will be reviewed.  

**Required Understanding:** Scenario impact, decision criticality, stakeholder availability, urgency.  

**Decision Alternatives:**  
- Convene full workshop (all stakeholders, facilitated session)  
- Convene targeted review (subset of stakeholders, focused agenda)  
- Defer workshop (use asynchronous review)  
- Skip workshop (decision can be made without collaboration)  

**Decision Criteria:** Strategic decisions with impact >$5M or affecting >2 business units require a full workshop. Operational decisions may use targeted review. Urgent decisions (<24 hours) may use asynchronous collaboration.  

**Decision Confidence:** Based on completeness of stakeholder representation.  

**Decision Rationale:** “Full workshop convened for Q3 supply plan scenario review: impact $8M, affects Supply Chain, Sales, and Finance. 8 stakeholders confirmed. Workshop scheduled for 15‑Jul, 4‑hour session. Rule BR‑SN‑060 applied.”  

---

##### Rules (for DE‑SN‑060)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑060 | Workshop Requirement Rule | Derivation Rule | A full workshop is required if the scenario recommendation impact exceeds $5M or the decision affects >2 business units. Documented consensus from all required stakeholders is mandatory before recommendation adoption. |
| BR‑SN‑061 | Quorum Rule | Validation Rule | A workshop is valid only if at least one representative from each affected business unit is present. If quorum is not met, the workshop is rescheduled or conducted asynchronously with documented sign‑offs. |

##### Policies (for DE‑SN‑060)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑060 | Workshop Scheduling Policy | Compliance Policy | Workshops for strategic decisions shall be scheduled at least 10 business days in advance. Materials shall be distributed 5 business days prior. |

---

#### DE‑SN‑061 — Facilitate Scenario Review  

**Purpose:** Guide stakeholders through structured review of scenario assumptions, simulation results, comparisons, and trade‑offs, capturing challenges, alternatives, and preferences.  

**Required Understanding:** Scenario catalogue, simulation results, comparison outputs, stakeholder perspectives.  

**Decision Alternatives:**  
- Endorse scenario/assumption as presented  
- Challenge assumption with alternative proposed  
- Request new scenario or simulation  
- Record dissent  

**Decision Criteria:** Based on stakeholder judgment, evidence from simulation, and alignment with strategic objectives.  

**Decision Confidence:** Reflects degree of stakeholder agreement.  

**Decision Rationale:** “Stakeholder from Sales challenged the downside demand assumption (−15%) as too pessimistic; proposed −10% based on recent pipeline data. New scenario variant requested. Dissent recorded.”  

---

##### Rules (for DE‑SN‑061)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑062 | Challenge Documentation Rule | Compliance Rule | Every assumption challenged must be documented with the challenger’s rationale and any proposed alternative. Challenges cannot be dismissed without recorded justification. |
| BR‑SN‑063 | Dissent Recording Rule | Compliance Rule | Dissenting views on a consensus decision must be recorded with the dissenter’s name, position, and rationale. Dissent does not block the decision but must be visible to the final decision‑maker. |

##### Policies (for DE‑SN‑061)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑061 | Challenge Resolution Policy | Authorization Policy | If an assumption is challenged by a stakeholder with domain authority (e.g., Sales VP challenging demand assumption), a new scenario variant shall be created and simulated before final recommendation. |

---

#### DE‑SN‑062 — Reach Consensus  

**Purpose:** Formalize the level of agreement among stakeholders on the scenario analysis and preferred course of action.  

**Required Understanding:** Workshop discussion outcomes, challenges and dissents, comparison rankings, risk appetite.  

**Decision Alternatives:**  
- Full consensus (all stakeholders agree)  
- Majority consensus (majority agrees, dissents recorded)  
- No consensus (decision escalated to higher authority)  

**Decision Criteria:** Full consensus = all required stakeholders endorse. Majority = >67% endorse. Below 67% = no consensus.  

**Decision Confidence:** Based on level of agreement.  

**Decision Rationale:** “Full consensus reached on recommending Plan Variant ‘Flex Capacity’. 8 of 8 stakeholders endorse. 1 dissent on assumption (recorded) but does not affect final recommendation choice.”  

---

##### Rules (for DE‑SN‑062)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑064 | Consensus Threshold Rule | Derivation Rule | Consensus levels: Full (100% endorse), Majority (>67%), No Consensus (≤67%). Full or Majority consensus is required to proceed to recommendation. No Consensus triggers escalation to the next management level. |
| BR‑SN‑065 | Consensus Documentation Rule | Compliance Rule | The consensus statement must list all endorsing stakeholders, any dissenting views with rationale, and the final agreed position. |

##### Policies (for DE‑SN‑062)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑062 | Escalation Policy (Consensus) | Authorization Policy | If no consensus is reached after one workshop and one follow‑up session, the decision is escalated to the Executive Committee with all positions documented. |

---

### 5.6.13 Functional Behaviour  

1. **Trigger:** Scheduled per planning calendar, or on completion of strategic scenario comparison.  
2. **Retrieve** scenario catalogue, simulation results, stakeholder roster.  
3. **Execute DE‑SN‑060** (Convene Workshop) — rules BR‑SN‑060/061, policy PO‑SN‑060.  
4. **Conduct** workshop (physical, virtual, or AI‑facilitated) — execute DE‑SN‑061 (Facilitate Review) iteratively as scenarios are discussed — rules BR‑SN‑062/063, policy PO‑SN‑061.  
5. **Execute DE‑SN‑062** (Reach Consensus) — rules BR‑SN‑064/065, policy PO‑SN‑062.  
6. **Publish** workshop record, consensus statement, and action items.  
7. **Raise events:** `WorkshopConvened`, `ScenarioAssumptionChallenged`, `ConsensusReached`, `ConsensusFailed`.  

### 5.6.14 Commands  

| Command | Purpose |
|---------|---------|
| `ConveneWorkshop` | Schedule and invite stakeholders |
| `RecordChallenge` | Document an assumption challenge |
| `RecordConsensus` | Formalize stakeholder agreement |
| `EscalateConsensus` | Escalate a no‑consensus decision |

### 5.6.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `WorkshopConvened` | Workshop ID, agenda, participants |
| `ScenarioAssumptionChallenged` | Scenario ID, assumption, challenger, alternative |
| `ConsensusReached` | Workshop ID, consensus level, endorsers, dissents |
| `ConsensusFailed` | Workshop ID, reason, escalation target |

### 5.6.16 Queries  

| Query | Description |
|-------|-------------|
| `GetWorkshop(workshopId)` | Full workshop record |
| `GetConsensusStatus(scenarioId)` | Current consensus state |
| `GetStakeholderFeedback(scenarioId)` | All challenges and inputs |

### 5.6.17 Reports  

- **Workshop Summary Report** – attendance, decisions, action items  
- **Stakeholder Participation Report** – engagement metrics  

### 5.6.18 Dashboards  

- **Collaboration Hub** – active workshops, consensus status, pending actions  
- **Stakeholder Engagement Dashboard** – participation rates, challenge activity  

### 5.6.19 Software Realization  

```
API → Application Service → Domain Model (Workshop, Consensus)  
→ Collaboration Adapters (calendar integration, virtual meeting APIs, shared whiteboards)  
→ Event Store → Projections (WorkshopRecord) → Read Model
```  
The collaboration platform integrates with enterprise calendar and communication tools. AI‑facilitated workshops may use natural language summarization of discussions. All challenges and decisions are recorded for traceability.

---

## 5.7 Sense Scenario Triggers  

### 5.7.1 Purpose  

Continuously monitor the enterprise environment—plans, actuals, external signals—for conditions that warrant initiating a new scenario analysis. Answers: *“Has something changed that requires us to re‑evaluate our plans?”* The capability detects plan deviations, supply disruptions, demand shocks, external events, and risk indicator breaches that should trigger scenario analysis, transforming the enterprise from reactive to anticipatory.  

### 5.7.2 Business Objectives Served  

- BO‑SN‑007 Accelerate Response to Change  
- BO‑SN‑005 Increase Scenario Planning Automation  

### 5.7.3 Enterprise Measures  

- PI‑SN‑004 Scenario Analysis Cycle Time (by triggering early)  
- PI‑SN‑010 Cost of Delay Avoided  

### 5.7.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑005 | Scenario Trigger | Core concept |
| SE‑SN‑015 | Scenario Trigger | Detailed metadata |
| SE‑SN‑041 | Risk Event | Trigger source |
| SE‑SN‑040 | Risk Factor | Trigger indicator |

### 5.7.5 Primitive Capabilities Composed  

- **Observe** – monitors plan vs. actuals, external signals, risk indicators  
- **Understand** – interprets deviations and events as triggers  
- **Assess** – determines trigger urgency and scope  

### 5.7.6 Enterprise Inputs  

- Plan vs. actual data from Demand, Supply, Promise (forecast accuracy, plan adherence, promise adherence)  
- Supply disruption events (from Supply Intelligence — Sense Supply Changes)  
- Demand change events (from Demand Intelligence — Sense Demand)  
- Promise breach events (from Promise Intelligence — Detect Promise Exceptions)  
- External data: market indices, commodity prices, weather, geopolitical alerts  
- Risk indicator thresholds  

### 5.7.7 Enterprise Understanding Produced  

- Trigger events with type, urgency, and recommended scenario scope  
- Trigger‑to‑scenario mapping: which pre‑defined scenarios should be executed for this trigger  
- Trigger trend analysis: are certain triggers becoming more frequent?  

### 5.7.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑060 | Scenario Trigger Alert | Trigger with type, urgency, recommended actions |
| OUT‑SN‑061 | Trigger‑to‑Scenario Mapping | Recommended scenarios to execute |
| OUT‑SN‑062 | Trigger Trend Report | Frequency and pattern analysis |

### 5.7.9 Preconditions  

- Plan vs. actual monitoring is operational  
- Trigger thresholds and mappings are configured  
- External data feeds are available  

### 5.7.10 Capability Dependencies  

- `CA‑SN‑001 Define Scenarios` – for scenario catalogue (trigger mappings)  
- `CA‑DI‑003 Sense Demand` – for demand change events  
- `CA‑SI‑009 Sense Supply Changes` – for supply disruption events  
- `CA‑PI‑007 Sense Promise Risks` – for promise risk events  

### 5.7.11 Collaborating Capabilities  

- **Simulate Scenarios** – receives trigger to auto‑initiate simulation  
- **Define Scenarios** – may request new scenario definition for novel triggers  

### 5.7.12 Business Decisions  

---

#### DE‑SN‑070 — Detect Scenario Trigger  

**Purpose:** Evaluate whether an observed event or deviation constitutes a trigger that warrants scenario analysis.  

**Required Understanding:** Event data, current plan baselines, trigger thresholds, historical trigger patterns.  

**Decision Alternatives:**  
- Trigger scenario analysis (urgent, immediate)  
- Queue for scheduled scenario review (non‑urgent)  
- Log for information only (below threshold)  
- Suppress (false positive, duplicate)  

**Decision Criteria:** Deviation exceeds trigger threshold (e.g., demand forecast error >15% for 2 consecutive weeks, supplier OTD drops below 80%, a major geopolitical event is detected in a key sourcing region).  

**Decision Confidence:** Based on signal quality and corroboration.  

**Decision Rationale:** “Scenario trigger detected: Supplier S5 OTD dropped to 72% (threshold 80%), confirmed by 3 consecutive late deliveries. Recommended scenario: ‘Supplier S5 Disruption’ stress test. Urgency: High. Rule BR‑SN‑070 applied.”  

---

##### Rules (for DE‑SN‑070)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑070 | Trigger Detection Rule | Derivation Rule | A trigger is activated if any monitored metric breaches its defined threshold and the breach is sustained for the required number of periods or confirmed by independent sources. |
| BR‑SN‑071 | Trigger‑to‑Scenario Mapping Rule | Derivation Rule | Each trigger type is mapped to one or more pre‑defined scenarios. If no scenario exists for a trigger, a request to Define Scenarios is generated. |
| BR‑SN‑072 | Duplicate Suppression Rule | Validation Rule | A trigger for the same underlying event within a configurable window (default 24 hours) is suppressed as duplicate. |

##### Policies (for DE‑SN‑070)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑070 | Auto‑Trigger Policy | Automation Policy | Triggers classified as Urgent automatically initiate the mapped scenario simulation without human intervention. Non‑urgent triggers are queued for the next scheduled review. |
| PO‑SN‑071 | Trigger Notification Policy | Compliance Policy | Urgent triggers notify the Scenario Manager and affected domain managers within 5 minutes of detection. |

---

#### DE‑SN‑071 — Determine Trigger Scope  

**Purpose:** Define the scope of scenario analysis to be initiated: which plan variants, which scenarios, and at what level of detail.  

**Required Understanding:** Trigger type, affected business areas, available scenarios, urgency.  

**Decision Alternatives:**  
- Execute pre‑defined scenario set (full scope)  
- Execute subset of scenarios (targeted analysis)  
- Request new scenario definition (novel trigger)  
- No action (monitor only)  

**Decision Criteria:** Match trigger to catalogue. If an exact match exists, execute the full pre‑defined scenario. If partial match, execute the closest scenarios. If no match, request new definition.  

**Decision Confidence:** Based on match quality.  

**Decision Rationale:** “Trigger scope: execute ‘Supplier S5 Disruption’ stress test and ‘Alternate Sourcing’ comparison. Affected area: Supply, Promise. Horizon: 8 weeks. Rule BR‑SN‑073 applied.”  

---

##### Rules (for DE‑SN‑071)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑073 | Scope Determination Rule | Derivation Rule | The scope must include at least: the most directly affected domain, the baseline plan, and the worst‑case scenario from the trigger mapping. |
| BR‑SN‑074 | Scope Adequacy Rule | Validation Rule | If the proposed scope covers <80% of the trigger’s estimated impact area, a warning is raised and scope expansion is recommended. |

##### Policies (for DE‑SN‑071)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑072 | Scope Override Policy | Authorization Policy | The Scenario Manager may expand or reduce the trigger scope with documented justification. |

---

### 5.7.13 Functional Behaviour  

1. **Continuous monitoring** of plan vs. actuals, disruption events, external signals.  
2. **For each detected deviation**, execute DE‑SN‑070 (Detect Scenario Trigger) — rules BR‑SN‑070/071/072, policies PO‑SN‑070/071.  
3. **For each confirmed trigger**, execute DE‑SN‑071 (Determine Trigger Scope) — rules BR‑SN‑073/074, policy PO‑SN‑072.  
4. **Initiate** scenario simulation via Simulate Scenarios.  
5. **Track** trigger response: time from trigger to simulation start, to recommendation.  
6. **Raise events:** `ScenarioTriggerDetected`, `ScenarioTriggerScopeDetermined`, `ScenarioTriggerActioned`.  

### 5.7.14 Commands  

| Command | Purpose |
|---------|---------|
| `EvaluateTrigger` | Manually evaluate a potential trigger |
| `SetTriggerScope` | Define or override the scope |

### 5.7.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioTriggerDetected` | Trigger ID, type, urgency, source event |
| `ScenarioTriggerScopeDetermined` | Trigger ID, scope, scenarios to execute |
| `ScenarioTriggerActioned` | Trigger ID, simulation run IDs initiated |

### 5.7.16 Queries  

| Query | Description |
|-------|-------------|
| `GetActiveTriggers()` | Current active triggers |
| `GetTriggerHistory(period)` | Past triggers and response times |

### 5.7.17 Reports  

- **Trigger Response Report** – detection to action time, automation rate  

### 5.7.18 Dashboards  

- **Trigger Monitor** – real‑time trigger feed with response status  

### 5.7.19 Software Realization  

```
Event Bus → Stream Processor (threshold monitoring) → Domain Service (TriggerDetector)  
→ Rule Engine (trigger mapping)  
→ Integration (calls Simulate Scenarios API)  
→ Event Store → Read Model (TriggerLog)
```  
Stream processing enables sub‑second trigger detection. Mappings are stored in a configurable rules repository.

---

## 5.8 Evaluate Scenario Quality  

### 5.8.1 Purpose  

Continuously measure and assess the quality, accuracy, timeliness, and value of scenario analysis activities. Answers: *“How good are our scenarios, and are they improving?”* The capability compares past scenario predictions to actual outcomes, tracks recommendation adoption and effectiveness, and identifies improvement opportunities for the entire Scenario Intelligence domain.  

### 5.8.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑008 Continuously Improve Scenario Intelligence  

### 5.8.3 Enterprise Measures  

- PI‑SN‑008 Scenario Accuracy  
- PI‑SN‑005 Scenario Recommendation Adoption Rate  
- PI‑SN‑009 Decision Confidence Improvement  
- PI‑SN‑102 Simulation Accuracy  
- PI‑SN‑111 Probability Calibration Score  
- All Business Outcome Measures (computed by this capability)  

### 5.8.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Evaluated scenario |
| SE‑SN‑004 | Scenario Outcome | Predicted outcome |
| SE‑SN‑022 | Simulation Result | Evaluated result |
| SE‑SN‑032 | Recommended Plan | Evaluated recommendation |
| SE‑SN‑033 | Adopted Plan | Outcome of adoption |

### 5.8.5 Primitive Capabilities Composed  

- **Observe** – collects actual outcomes  
- **Understand** – aligns predictions with actuals  
- **Assess** – computes accuracy metrics  
- **Evaluate** – compares against targets and trends  

### 5.8.6 Enterprise Inputs  

- Past scenario predictions and simulation results  
- Actual enterprise outcomes (KPIs realized vs. scenario projections)  
- Recommendation and adoption records  
- Decision‑maker feedback and confidence surveys  
- Trigger response logs  

### 5.8.7 Enterprise Understanding Produced  

- Scenario accuracy metrics: predicted vs. actual for each scenario that materialized  
- Simulation accuracy: calibration of probabilistic outputs vs. observed frequencies  
- Recommendation adoption and effectiveness trends  
- Process metrics: cycle times, automation rates, participation rates  
- Quality trends: improvement or degradation signals  

### 5.8.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑070 | Scenario Quality Report | Consolidated accuracy and process metrics |
| OUT‑SN‑071 | Calibration Analysis | Probability calibration assessment |
| OUT‑SN‑072 | Improvement Opportunities | Identified gaps with recommendations |

### 5.8.9 Preconditions  

- Actual outcome data is available for the evaluation period  
- Scenario predictions are stored and accessible  
- Evaluation periods align with scenario horizons  

### 5.8.10 Capability Dependencies  

- `CA‑SN‑001 Define Scenarios` – for scenario catalogue  
- `CA‑SN‑002 Simulate Scenarios` – for simulation results  
- `CA‑SN‑005 Recommend Scenario` – for adoption records  

### 5.8.11 Collaborating Capabilities  

- **Learn From Scenarios** – consumes quality reports for improvement  
- **Sense Scenario Triggers** – consumes accuracy data for trigger threshold tuning  

### 5.8.12 Business Decisions  

---

#### DE‑SN‑080 — Compute Scenario Accuracy  

**Purpose:** Compare past scenario predictions against actual realized outcomes and compute accuracy metrics.  

**Required Understanding:** Scenario predictions (KPI projections), actual outcomes for the corresponding periods, materialized scenario identification.  

**Decision Alternatives:** Deterministic computation.  

**Decision Criteria:** For each materialized scenario, compute accuracy = (1 − |Predicted − Actual| ÷ Actual) × 100 for each KPI. Aggregate across KPIs and scenarios.  

**Decision Confidence:** Based on data completeness.  

**Decision Rationale:** “Scenario ‘Q3 Upside Demand’ accuracy: predicted service level 94%, actual 93% (accuracy 98.9%). Predicted cost $5.1M, actual $5.3M (accuracy 96.1%). Overall scenario accuracy 94.2%.”  

---

##### Rules (for DE‑SN‑080)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑080 | Accuracy Calculation Rule | Calculation Rule | Scenario accuracy is computed as (1 − |Predicted − Actual| ÷ Actual) × 100 for each KPI. Overall accuracy = weighted average (weights per KPI importance). |
| BR‑SN‑081 | Materialization Identification Rule | Derivation Rule | A scenario is considered materialized if the actual values of key variables (demand, supply, cost) fall within the scenario’s defined assumption ranges (±20% tolerance) for the scenario horizon. |
| BR‑SN‑082 | Minimum Data Rule | Validation Rule | Accuracy is only computed if the materialized period covers at least 80% of the scenario horizon. |

##### Policies (for DE‑SN‑080)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑080 | Accuracy Review Frequency Policy | Compliance Policy | Scenario accuracy is assessed quarterly for all scenarios with horizons that have elapsed. Results are presented at the quarterly S&OP review. |

---

#### DE‑SN‑081 — Evaluate Probability Calibration  

**Purpose:** Assess whether the probability distributions produced by probabilistic simulations are well‑calibrated—do events predicted with X% probability actually occur X% of the time?  

**Required Understanding:** Probabilistic simulation outputs (predicted probabilities for events), actual event occurrences.  

**Decision Alternatives:**  
- Well‑calibrated (predicted probabilities match observed frequencies)  
- Over‑confident (predicted probabilities too extreme)  
- Under‑confident (predicted probabilities too conservative)  

**Decision Criteria:** Calibration curve analysis. For events predicted with P% probability, across many predictions, the event should occur ~P% of the time. Deviations indicate miscalibration.  

**Decision Confidence:** Depends on sample size (number of probabilistic scenarios with materialized outcomes).  

**Decision Rationale:** “Probability calibration assessed: 25 probabilistic scenarios evaluated. Calibration score 0.87 (1.0 = perfect). Slight over‑confidence detected in high‑probability range (>90%): events predicted at 95% occurred 88% of the time. Rule BR‑SN‑083 recommends recalibration.”  

---

##### Rules (for DE‑SN‑081)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑083 | Calibration Assessment Rule | Derivation Rule | Calibration is measured using Brier Score or calibration curve. A calibration score <0.8 triggers a model recalibration review. |
| BR‑SN‑084 | Minimum Sample Rule | Validation Rule | Calibration assessment requires at least 20 probabilistic scenarios with materialized outcomes. Below that threshold, assessment is flagged as “provisional”. |

##### Policies (for DE‑SN‑081)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑081 | Recalibration Policy | Compliance Policy | If calibration score <0.8, the simulation engine must be recalibrated within 30 days. |

---

#### DE‑SN‑082 — Publish Scenario Quality Report  

**Purpose:** Compile and distribute the periodic scenario quality report to stakeholders.  

**Required Understanding:** Accuracy metrics, calibration results, process metrics, trends.  

**Decision Alternatives:** Publish, Publish with flags, Hold.  

**Decision Rationale:** “Q3 Scenario Quality Report published: overall accuracy 94%, calibration 0.87, 15 scenarios evaluated.”  

---

##### Rules (for DE‑SN‑082)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑085 | Report Completeness Rule | Validation Rule | Quality report must include accuracy metrics, calibration assessment, and adoption rate for the period. |

##### Policies (for DE‑SN‑082)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑082 | Report Distribution Policy | Compliance Policy | Scenario Quality Report is published quarterly, within 15 business days of quarter end, to the Executive Committee and all scenario stakeholders. |

---

### 5.8.13 Functional Behaviour  

1. **Scheduled:** Quarterly, aligned with financial reporting and S&OP calendar.  
2. **Retrieve** scenario predictions, actual outcomes, recommendation records.  
3. **Identify** materialized scenarios via BR‑SN‑081.  
4. **Execute DE‑SN‑080** (Compute Scenario Accuracy) — rules BR‑SN‑080/081/082, policy PO‑SN‑080.  
5. **Execute DE‑SN‑081** (Evaluate Probability Calibration) — rules BR‑SN‑083/084, policy PO‑SN‑081.  
6. **Execute DE‑SN‑082** (Publish Quality Report) — rule BR‑SN‑085, policy PO‑SN‑082.  
7. **Raise events:** `ScenarioAccuracyComputed`, `CalibrationAssessed`, `ScenarioQualityReportPublished`.  

### 5.8.14 Commands  

| Command | Purpose |
|---------|---------|
| `ComputeScenarioAccuracy` | Run accuracy assessment |
| `AssessCalibration` | Run calibration evaluation |
| `PublishQualityReport` | Compile and release |

### 5.8.15 Events  

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioAccuracyComputed` | Scenario ID, accuracy per KPI, overall |
| `CalibrationAssessed` | Calibration score, over/under‑confidence flag |
| `ScenarioQualityReportPublished` | Report ID, period |

### 5.8.16 Queries  

| Query | Description |
|-------|-------------|
| `GetScenarioAccuracy(scenarioId)` | Accuracy metrics |
| `GetCalibrationScore(period)` | Calibration assessment |

### 5.8.17 Reports  

- **Scenario Quality Report** – accuracy, calibration, adoption, process metrics  

### 5.8.18 Dashboards  

- **Scenario Quality Dashboard** – accuracy trends, calibration charts  
- **Scenario Performance Scorecard** – KPI‑by‑KPI comparison  

### 5.8.19 Software Realization  

```
API → Application Service → Domain Model (ScenarioQuality)  
→ Computation Engine (accuracy, calibration algorithms)  
→ Event Store → Projections (QualityView) → Read Model
```  

---

## 5.9 Detect Scenario Exceptions  

### 5.9.1 Purpose  

Identify, classify, prioritize, and resolve exceptions that arise within the Scenario Intelligence domain—simulation failures, calibration drift, recommendation rejection patterns, trigger failures, data gaps, and systemic quality degradation. Answers: *“What is going wrong in our scenario process, and what needs immediate attention?”*  

### 5.9.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑007 Accelerate Response to Change  

### 5.9.3 Enterprise Measures  

- PI‑SN‑004 Scenario Analysis Cycle Time (indirectly, by reducing exception resolution time)  
- PI‑SN‑205 Event Processing Latency (Scenario)  

### 5.9.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑002 | Simulation | Source of failures |
| SE‑SN‑022 | Simulation Result | Source of anomalies |
| SE‑SN‑025 | Simulation Run | Failed run |
| SE‑SN‑015 | Scenario Trigger | Failed trigger |
| SE‑SN‑013 | Scenario Status | Stuck or error state |

### 5.9.5 Primitive Capabilities Composed  

- **Observe** – monitors simulation execution, quality metrics, trigger pipeline  
- **Understand** – interprets error signals and patterns  
- **Assess** – determines exception severity and impact  

### 5.9.6 Enterprise Inputs  

- Simulation run logs, failure records (from Simulate Scenarios)  
- Scenario quality metrics (from Evaluate Scenario Quality)  
- Trigger processing logs (from Sense Scenario Triggers)  
- Recommendation rejection records (from Recommend Scenario)  
- Data freshness and completeness indicators  

### 5.9.7 Enterprise Understanding Produced  

- Exception instances with type, severity, affected scenarios or capabilities, root cause, and timestamp  
- Exception queue prioritized by impact on decision‑making  
- Recommended resolution actions  

### 5.9.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑080 | Scenario Exception Record | Exception with type, severity, status, assigned owner |
| OUT‑SN‑081 | Exception Resolution Recommendation | Suggested action with rationale |

### 5.9.9 Preconditions  

- Monitoring of simulation execution and quality metrics is operational  
- Exception thresholds are configured  

### 5.9.10 Capability Dependencies  

- `CA‑SN‑002 Simulate Scenarios` – for simulation failures  
- `CA‑SN‑007 Sense Scenario Triggers` – for trigger failures  
- `CA‑SN‑008 Evaluate Scenario Quality` – for quality degradation signals  

### 5.9.11 Collaborating Capabilities  

- **Explain Scenario Decisions** – generates explanations for exceptions  
- **Learn From Scenarios** – consumes resolved exceptions for learning  

### 5.9.12 Business Decisions  

---

#### DE‑SN‑090 — Classify Scenario Exception  

**Purpose:** Determine the nature and type of a detected anomaly in the scenario process.  

**Required Understanding:** Anomaly data, context, affected capabilities, historical patterns.  

**Decision Alternatives:**  
- Simulation Failure (run error, time‑out, implausible output)  
- Calibration Drift (probability calibration degrading)  
- Recommendation Rejection (systematic rejection of recommendations)  
- Trigger Failure (trigger not detected or not actioned)  
- Data Gap (missing inputs for simulation)  
- Quality Degradation (accuracy or calibration below threshold)  
- False Positive  

**Decision Criteria:** Rules‑based classification.  

**Decision Confidence:** Based on data quality and corroboration.  

**Decision Rationale:** *Explainability Template:* “Exception EX‑SN‑5001 classified as Simulation Failure: Run SR‑4450 failed after 45 minutes with memory overflow. Data volume within normal range. Rule BR‑SN‑090 applied.”  

---

##### Rules (for DE‑SN‑090)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑090 | Exception Classification Rule (Scenario) | Derivation Rule | If simulation run status = Error or Timeout → Simulation Failure. If calibration score degrades >0.1 in one quarter → Calibration Drift. If recommendation rejection rate >50% in a quarter → Recommendation Rejection. If trigger not actioned within SLA → Trigger Failure. If required data not available at simulation start → Data Gap. |
| BR‑SN‑091 | False Positive Filter Rule | Validation Rule | Simulation failures caused by known, temporary infrastructure issues (documented outage) are classified as Transient and not raised as exceptions unless they recur >3 times in 24 hours. |

##### Policies (for DE‑SN‑090)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑090 | Exception Logging Policy | Compliance Policy | All classified exceptions are logged immutably with traceability to the originating capability. |

---

#### DE‑SN‑091 — Prioritize Scenario Exception  

**Purpose:** Assign severity and urgency to the exception based on impact on decision‑making.  

**Required Understanding:** Exception type, affected scenarios (strategic vs. operational), decision deadlines.  

**Decision Alternatives:** Critical, High, Medium, Low.  

**Decision Criteria:** If exception blocks an active strategic decision → Critical. If it degrades quality but does not block → High or Medium. If it is informational → Low.  

**Decision Rationale:** “Exception EX‑SN‑5001 prioritized as High: Simulation failure blocks comparison for Q3 strategic supply plan, but deadline is 5 days away.”  

---

##### Rules (for DE‑SN‑091)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑092 | Exception Priority Rule (Scenario) | Derivation Rule | Critical if exception blocks an active strategic recommendation with decision deadline <3 days. High if blocks any active scenario. Medium if degrades quality but does not block. Low if informational. |
| BR‑SN‑093 | Escalation Rule (Scenario) | Validation Rule | Critical exceptions are escalated immediately to Scenario Manager and affected capability owners. |

##### Policies (for DE‑SN‑091)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑091 | Exception Escalation Policy (Scenario) | Authorization Policy | Critical exceptions must be acknowledged within 15 minutes, High within 1 hour. |

---

#### DE‑SN‑092 — Resolve Scenario Exception  

**Purpose:** Determine and execute the appropriate resolution action.  

**Required Understanding:** Exception details, available resources, automation rules.  

**Decision Alternatives:**  
- Auto‑resolve (retry simulation, refresh data, clear transient error)  
- Suggest resolution (generate recommendation for specialist)  
- Manual only (requires investigation)  

**Decision Criteria:** Based on exception type and automation policy.  

**Decision Rationale:** “Exception EX‑SN‑5001 auto‑resolved: Simulation retried with reduced parallel load, completed successfully in 52 minutes. Results validated.”  

---

##### Rules (for DE‑SN‑092)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑094 | Auto‑Resolution Rule (Scenario) | Validation Rule | Simulation Failures and Data Gaps may be auto‑retried once. Calibration Drift and Recommendation Rejection require manual investigation. |
| BR‑SN‑095 | Resolution Documentation Rule | Compliance Rule | Every resolution action must be logged with timestamp, actor, and outcome. |

##### Policies (for DE‑SN‑092)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑092 | Resolution SLA Policy (Scenario) | Compliance Policy | Critical: 1 hour, High: 4 hours, Medium: 24 hours, Low: 5 business days. |

---

### 5.9.13 Functional Behaviour  

1. **Event‑driven:** Listens to simulation failures, quality alerts, trigger gaps, recommendation rejections.  
2. **For each anomaly**, execute DE‑SN‑090 (Classify Exception) — rules BR‑SN‑090/091, policy PO‑SN‑090.  
3. **For each confirmed exception**, execute DE‑SN‑091 (Prioritize Exception) — rules BR‑SN‑092/093, policy PO‑SN‑091.  
4. **For each prioritized exception**, execute DE‑SN‑092 (Resolve Exception) — rules BR‑SN‑094/095, policy PO‑SN‑092.  
5. **Track** resolution SLAs.  
6. **Raise events:** `ScenarioExceptionDetected`, `ScenarioExceptionPrioritized`, `ScenarioExceptionResolved`.  


### 5.9.15 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioExceptionDetected` | Exception ID, type, source capability, affected run/scenario, timestamp |
| `ScenarioExceptionPrioritized` | Exception ID, severity (Critical/High/Medium/Low), justification |
| `ScenarioExceptionResolved` | Exception ID, resolution type, actor, outcome, timestamp |
| `ScenarioExceptionEscalated` | Exception ID, escalated to, reason |

### 5.9.16 Queries

| Query | Description |
|-------|-------------|
| `GetActiveScenarioExceptions(filter)` | List all unresolved exceptions, optionally filtered by type or severity. |
| `GetScenarioExceptionHistory(period)` | Retrieve the log of past exceptions and their resolutions. |
| `GetScenarioException(exceptionId)` | Full details of a single exception. |

### 5.9.17 Reports

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑011 | Scenario Exception Summary Report | Detect Scenario Exceptions | Scenario Manager, Simulation Specialist | Weekly | Counts by type, severity, resolution time, SLA compliance, recurring issues. |

### 5.9.18 Dashboards

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑015 | Scenario Exception Monitor | Detect Scenario Exceptions | Scenario Manager | Real‑time | Live exception feed, aging, resolution status, SLA gauges, trend charts. |

### 5.9.19 Software Realization

```
Event Bus (SimulationFailed, CalibrationDrift, etc.)  
  → Stream Processor (windowed deduplication, anomaly pattern detection)  
  → Domain Service (ScenarioException aggregate)  
      → Rule Engine (classification, prioritization, auto‑resolution)  
  → Event Store → Projections (ExceptionQueue, ExceptionHistory)  
  → Read Model (for dashboards and queries)
```

The exception detection logic runs as a stateful stream processor that correlates failure events, quality metric anomalies, and trigger gaps. Auto‑resolution rules are configurable and can trigger retries, data refreshes, or escalation. All exceptions are immutably logged for audit and learning.

---

## 5.10 Explain Scenario Decisions  

### 5.10.1 Purpose  

Generate clear, traceable explanations for every scenario definition, simulation result, comparison, risk assessment, recommendation, and trigger action. Answers: *“Why was this scenario defined this way? Why did the simulation produce these results? Why was this plan recommended over others?”* Explanations are derived automatically from the causal traceability chain—assumptions, model logic, rule evaluations, and stakeholder input.  

### 5.10.2 Business Objectives Served  

- BO‑SN‑001 Deliver Trusted Scenario Analysis  
- BO‑SN‑008 Continuously Improve Scenario Intelligence  

### 5.10.3 Enterprise Measures  

- PI‑SN‑106 Explainability Score (Scenario)  

### 5.10.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Subject |
| SE‑SN‑004 | Scenario Outcome | Subject |
| SE‑SN‑032 | Recommended Plan | Subject |
| SE‑SN‑083 | Scenario Lineage | Traceability chain |

### 5.10.5 Primitive Capabilities Composed  

- **Understand** – interprets decision logs, rule evaluations, stakeholder inputs  

### 5.10.6 Enterprise Inputs  

- Scenario definitions with assumptions and lineage  
- Simulation results and confidence scores  
- Comparison outputs and ranking rationale  
- Risk assessments  
- Recommendation decisions and stakeholder consensus records  
- Trigger events and scope decisions  

### 5.10.7 Enterprise Understanding Produced  

- Structured explanation objects with natural language and machine‑readable traceability  
- Explanation quality scores  

### 5.10.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑090 | Scenario Explanation | Structured explanation with traceability |

### 5.10.9 Preconditions  

- Decision logs and lineage data are complete  
- Traceability links are populated  

### 5.10.10 Capability Dependencies  

- All scenario capabilities for their decision logs  

### 5.10.11 Collaborating Capabilities  

- **Collaborate on Scenarios** – consumes explanations for workshop materials  
- **Learn From Scenarios** – consumes explanations for learning  

### 5.10.12 Business Decisions  

---

#### DE‑SN‑100 — Generate Scenario Explanation  

**Purpose:** Produce a human‑ and machine‑readable explanation for any scenario artifact.  

**Required Understanding:** The artifact (scenario, simulation, comparison, recommendation), its lineage, the rules and policies applied, the data used.  

**Decision Alternatives:** Deterministic.  

**Decision Criteria:** Explanation must include: (1) what was evaluated, (2) what assumptions were made, (3) what rules/policies applied, (4) what the outcome was and why.  

**Decision Rationale:** “Scenario ‘Q3 Upside Demand’ explanation: This scenario was defined to test plan robustness against demand exceeding forecast by 15%. Assumption based on Sales pipeline data showing 12% upside risk, rounded to 15% for conservatism. Rule BR‑SN‑010 required at least one distinguishing assumption. Simulation used probabilistic method with 10,000 iterations (rule BR‑SN‑020). The recommended plan ‘Flex Capacity’ was selected because it ranked #1 on robustness (82%) and satisfied all risk appetite constraints (rule BR‑SN‑050). Full traceability: Scenario SN‑003 → Simulation SR‑4401 → Comparison CMP‑012 → Recommendation REC‑005.” (Explainability template.)  

---

##### Rules (for DE‑SN‑100)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑100 | Explanation Completeness Rule (Scenario) | Validation Rule | Every scenario explanation must include: artifact type, assumptions, rules evaluated, policies applied, and outcome with rationale. |
| BR‑SN‑101 | Traceability Chain Rule (Scenario) | Validation Rule | Explanation must include the full ARS traceability chain: Artifact ID → Decision ID(s) → Rule ID(s) → Policy ID(s) → Capability ID(s). |
| BR‑SN‑102 | Natural Language Rule (Scenario) | Derivation Rule | Explanations follow the standard template: “Scenario {{ID}} was defined to {{purpose}}. Assumptions: {{list}}. Simulation used {{method}} with {{iterations}} iterations. Comparison method: {{method}}. Recommendation: {{variant}} selected because {{rationale}}.” |

##### Policies (for DE‑SN‑100)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑100 | Explanation Quality Policy (Scenario) | Compliance Policy | Explanations with quality score <60% are flagged. Below 40%, the artifact is held until explanation is enhanced. |

---

### 5.10.13 Functional Behaviour  

1. **Event‑driven:** On scenario publication, simulation completion, comparison completion, recommendation made, trigger actioned.  
2. **Retrieve** decision logs, lineage, rule evaluations.  
3. **Execute DE‑SN‑100** (Generate Explanation) — rules BR‑SN‑100/101/102, policy PO‑SN‑100.  
4. **Publish** explanation with traceability chain.  
5. **Raise events:** `ScenarioExplanationGenerated`.  

### 5.10.14 Commands

| Command | Purpose |
|---------|---------|
| `GenerateScenarioExplanation` | Create a structured explanation for a given artifact (scenario, simulation, comparison, recommendation). |
| `RegenerateScenarioExplanation` | Rebuild explanation after template or rule changes. |

### 5.10.15 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioExplanationGenerated` | Artifact ID, artifact type, natural language text, causal trace, explainability score, timestamp |

### 5.10.16 Queries

| Query | Description |
|-------|-------------|
| `GetScenarioExplanation(artifactId)` | Retrieve the full structured explanation for a specific artifact. |
| `GetExplainabilityScore(scope, period)` | Aggregate explainability score by capability, scenario, or time period. |

### 5.10.17 Reports

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑017 | Explainability Score Report (Scenario) | Explain Scenario Decisions | Scenario Manager, Data Science | Monthly | Average explainability score, breakdown by capability, low‑score items flagged. |

### 5.10.18 Dashboards

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑017 | Explainability Overview (Scenario) | Explain Scenario Decisions | Scenario Manager, Data Science | Weekly | Score trends, explanation completeness, traceability chain visualizer. |

### 5.10.19 Software Realization

```
Event Bus (ScenarioPublished, SimulationCompleted, ComparisonCompleted, etc.)  
  → Explanation Service (template engine, traceability resolver)  
  → Domain Model (Explanation)  
  → Event Store → Read Model
```

The explanation service subscribes to all scenario‑related events. It retrieves decision logs, rule evaluations, and lineage data from the respective capabilities’ read models. Templates are versioned and stored in a content repository. The service assembles natural‑language explanations and a machine‑readable traceability chain conforming to the ARS standard.


---

## 5.11 Learn From Scenarios  

### 5.11.1 Purpose  

Continuously improve the Scenario Intelligence domain by analyzing scenario accuracy, simulation calibration, recommendation effectiveness, trigger responsiveness, and process efficiency. Answers: *“How can our scenario analysis become more accurate, faster, and more valuable?”* The capability closes the feedback loop, recommending enhancements to simulation models, assumption frameworks, comparison criteria, risk thresholds, and automation policies.  

### 5.11.2 Business Objectives Served  

- BO‑SN‑008 Continuously Improve Scenario Intelligence  
- BO‑SN‑005 Increase Scenario Planning Automation  

### 5.11.3 Enterprise Measures  

- PI‑SN‑110 Learning Effectiveness Index (Scenario)  
- PI‑SN‑105 Recommendation Quality Index (Scenario)  

### 5.11.4 Semantic Objects  

| ID | Object | Role |
|----|--------|------|
| SE‑SN‑001 | Scenario | Evaluated scenario |
| SE‑SN‑020 | Simulation Engine | Subject of improvement |
| SE‑SN‑051 | Comparison Criteria | Subject of tuning |
| SE‑SN‑043 | Risk Mitigation | Subject of effectiveness tracking |

### 5.11.5 Primitive Capabilities Composed  

- **Observe** – monitors performance trends  
- **Understand** – identifies root causes  
- **Assess** – evaluates improvement opportunities  
- **Predict** – forecasts impact of proposed changes  
- **Evaluate** – compares before/after  
- **Learn** – institutionalizes improvements  

### 5.11.6 Enterprise Inputs  

- Scenario quality metrics (from Evaluate Scenario Quality)  
- Exception logs (from Detect Scenario Exceptions)  
- Simulation performance data (run times, failure rates)  
- Recommendation adoption and outcomes  
- Trigger response metrics  

### 5.11.7 Enterprise Understanding Produced  

- Improvement recommendations with estimated impact  
- Simulation model recalibration recommendations  
- Threshold adjustment recommendations (trigger, risk, automation)  
- Assumption framework enhancements  
- Learning loop closure reports  

### 5.11.8 Enterprise Outputs  

| ID | Output | Description |
|----|--------|-------------|
| OUT‑SN‑100 | Improvement Recommendation | Specific change with rationale and expected benefit |
| OUT‑SN‑101 | Learning Loop Closure Report | Before‑after evaluation |

### 5.11.9 Preconditions  

- Historical performance data available for multiple quarters  

### 5.11.10 Capability Dependencies  

- `CA‑SN‑008 Evaluate Scenario Quality` – for quality metrics  
- `CA‑SN‑009 Detect Scenario Exceptions` – for exception data  

### 5.11.11 Collaborating Capabilities  

- **Simulate Scenarios** – receives model recalibration updates  
- **Define Scenarios** – receives assumption framework enhancements  
- **Recommend Scenario** – receives decision criteria tuning  

### 5.11.12 Business Decisions  

---

#### DE‑SN‑110 — Recommend Simulation Improvement  

**Purpose:** Analyze simulation accuracy and performance to recommend model recalibration, engine upgrades, or parameter tuning.  

**Required Understanding:** Simulation accuracy trends, calibration scores, failure rates, run times, resource utilization.  

**Decision Criteria:** If calibration score <0.8 → recalibration. If failure rate >5% → engine tuning. If average run time growth >20% year‑over‑year → performance optimization.  

**Decision Rationale:** “Simulation engine recalibration recommended: calibration score dropped from 0.87 to 0.76 over 2 quarters. Estimated improvement: +0.10 calibration score. Rule BR‑SN‑110 applied.”  

---

##### Rules (for DE‑SN‑110)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑110 | Recalibration Trigger Rule | Model Evaluation Rule | If calibration score <0.8 or degrades >0.1 in one quarter, a recalibration recommendation is generated. |
| BR‑SN‑111 | Performance Degradation Rule | Model Evaluation Rule | If simulation failure rate >5% or average run time increases >20% without a corresponding increase in model complexity, a performance review is triggered. |

##### Policies (for DE‑SN‑110)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑110 | Recalibration Approval Policy | Approval Policy | Recalibration of the simulation engine requires Data Science team approval and is scheduled during the next maintenance window. |

---

#### DE‑SN‑111 — Recommend Threshold Adjustment (Scenario)  

**Purpose:** Analyze trigger sensitivity, risk thresholds, and automation parameters to recommend adjustments that improve responsiveness and reduce noise.  

**Decision Criteria:** Cost‑benefit analysis of threshold changes.  

**Decision Rationale:** “Recommend lowering trigger threshold for demand deviation from 15% to 12%: expected to detect 3 additional actionable triggers per quarter with 1 additional false positive. Net benefit positive.”  

---

##### Rules (for DE‑SN‑111)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑112 | Threshold Optimization Rule (Scenario) | Model Evaluation Rule | Thresholds are reviewed quarterly. A recommendation to adjust is generated if expected net benefit exceeds a configurable minimum. |
| BR‑SN‑113 | Threshold Stability Rule (Scenario) | Consistency Rule | A threshold shall not be changed more than once per quarter unless a significant event is documented. |

##### Policies (for DE‑SN‑111)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑111 | Threshold Adjustment Approval Policy (Scenario) | Approval Policy | Adjustments to trigger thresholds require Scenario Manager approval. Adjustments to risk appetite thresholds require Risk Committee approval. |

---

#### DE‑SN‑112 — Close the Learning Loop (Scenario)  

**Purpose:** After an improvement is implemented, evaluate whether the expected benefit was realized.  

**Decision Alternatives:** Improvement confirmed, Partially realized, No improvement, Negative impact (rollback).  

**Decision Rationale:** “Recalibration completed in Q3. Post‑recalibration calibration score 0.85 (target 0.83), improvement confirmed. Learning loop closed.”  

---

##### Rules (for DE‑SN‑112)  

| ID | Rule | Category | Description |
|----|------|----------|-------------|
| BR‑SN‑114 | Improvement Verification Rule (Scenario) | Validation Rule | Every implemented improvement must be evaluated after a minimum observation window (one quarter for calibration, one month for thresholds). |
| BR‑SN‑115 | Auto‑Rollback Rule (Scenario) | Model Evaluation Rule | If an implemented change causes statistically significant degradation in scenario accuracy or calibration, automatic rollback is triggered. |

##### Policies (for DE‑SN‑112)  

| ID | Policy | Category | Description |
|----|--------|----------|-------------|
| PO‑SN‑112 | Learning Loop Closure Policy (Scenario) | Compliance Policy | Every improvement is documented with before‑after evaluation. Results reported at the quarterly S&OP review. |

---

### 5.11.13 Functional Behaviour  

1. **Scheduled:** Quarterly deep analysis, monthly threshold review.  
2. **Event‑driven:** After quality report publication, after significant exception clusters.  
3. **Retrieve** quality metrics, exception logs, performance data.  
4. **Execute DE‑SN‑110** (Recommend Simulation Improvement) — rules BR‑SN‑110/111, policy PO‑SN‑110.  
5. **Execute DE‑SN‑111** (Recommend Threshold Adjustment) — rules BR‑SN‑112/113, policy PO‑SN‑111.  
6. **For prior improvements**, execute DE‑SN‑112 (Close Learning Loop) — rules BR‑SN‑114/115, policy PO‑SN‑112.  
7. **Publish** recommendations and loop closure reports.  
8. **Feed** approved improvements back to relevant capabilities.  
9. **Raise events:** `ScenarioImprovementRecommended`, `ScenarioLearningLoopClosed`.  

### 5.11.14 Commands

| Command | Purpose |
|---------|---------|
| `AnalyzeScenarioPerformance` | Run trend analysis on quality, accuracy, calibration, and trigger metrics. |
| `ProposeImprovement` | Generate a specific improvement recommendation (recalibration, threshold adjustment, new scenario type). |
| `EvaluateImprovement` | Assess the impact of an implemented improvement (before/after comparison). |
| `RollbackImprovement` | Revert a change that caused degradation. |

### 5.11.15 Events

| Event | Payload Highlights |
|-------|-------------------|
| `ScenarioImprovementRecommended` | Improvement ID, type (calibration, threshold, engine), target, expected benefit, confidence. |
| `ScenarioLearningLoopClosed` | Improvement ID, before/after metrics, verdict (confirmed/partial/rejected), rollback status. |

### 5.11.16 Queries

| Query | Description |
|-------|-------------|
| `GetImprovementHistory(period)` | All recommended and implemented improvements, including outcomes. |
| `GetActiveImprovements()` | Improvements that are awaiting evaluation or in‑progress. |
| `GetLearningEffectivenessIndex()` | Composite metric of improvement success rate over time. |

### 5.11.17 Reports

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑012 | Continuous Improvement Report (Scenario) | Learn From Scenarios | Scenario Manager, Supply Chain Director | Quarterly | Improvements proposed, implemented, verified; before/after metrics; learning loop status. |

### 5.11.18 Dashboards

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑016 | Learning Dashboard (Scenario) | Learn From Scenarios | Scenario Manager, Data Science | Monthly | Improvement funnel (proposed → approved → implemented → verified), learning effectiveness trend, rollback events, recalibration schedule. |

### 5.11.19 Software Realization

```
Scheduled/Event Triggers → Analytics Engine (trend analysis, optimization)  
  → Domain Service (Improvement aggregate, LearningLoop)  
  → Rule Engine (improvement recommendation rules, rollback rules)  
  → Event Store → Projections → Read Model
```
The analytics engine queries the quality, exception, and execution data from the respective read models. Improvement recommendations that require recalibration are forwarded to the Simulation Engine’s model management pipeline. Threshold changes are applied via a configuration service consumed by the appropriate capabilities. All improvement actions are version‑tracked for audit and reproducibility.

---

# Chapter 6 — External Interfaces  

## 6.1 Purpose  

This chapter defines every external interface that the Scenario Intelligence domain exposes to other domains, external systems, and users. Each interface is specified with its purpose, contract, authentication, and the capability that owns it.  

## 6.2 Enterprise APIs  

### 6.2.1 Scenario Management API  

| Attribute | Value |
|-----------|-------|
| Owner | Define Scenarios (5.1) |
| Purpose | Create, update, query, and archive scenario definitions and the scenario catalogue. |
| Protocol | REST (HTTPS) |
| Authentication | OAuth 2.0 |
| Endpoints | `POST /api/v1/scenarios`, `GET /api/v1/scenarios/{id}`, `GET /api/v1/scenarios/catalogue`, `PUT /api/v1/scenarios/{id}`, `DELETE /api/v1/scenarios/{id}` |

**Example Request (Create Scenario):**  
```json
{
  "type": "Upside",
  "horizon": "Q3-2026",
  "assumptions": [
    {"variable": "demand", "override": "+15%"},
    {"variable": "leadTime", "override": "unchanged"}
  ],
  "purpose": "Test plan robustness to demand upside"
}
```  

---

### 6.2.2 Simulation Execution API  

| Attribute | Value |
|-----------|-------|
| Owner | Simulate Scenarios (5.2) |
| Purpose | Start a simulation run, query status and results. |
| Endpoints | `POST /api/v1/simulations/run`, `GET /api/v1/simulations/{runId}`, `GET /api/v1/simulations/{runId}/results` |

**Example Request (Start Simulation):**  
```json
{
  "scenarioId": "SN-003",
  "planVariantId": "PLAN-Q3-Base",
  "method": "Probabilistic",
  "iterations": 10000
}
```  

---

### 6.2.3 Comparison API  

| Attribute | Value |
|-----------|-------|
| Owner | Compare Scenarios (5.3) |
| Purpose | Execute comparison, retrieve ranking, Pareto frontier, trade‑off analysis. |
| Endpoints | `POST /api/v1/comparisons`, `GET /api/v1/comparisons/{comparisonId}` |

---

### 6.2.4 Risk Assessment API  

| Attribute | Value |
|-----------|-------|
| Owner | Assess Risks (5.4) |
| Purpose | Run risk assessment, execute stress test, prioritize mitigations. |
| Endpoints | `POST /api/v1/risks/assess`, `POST /api/v1/risks/stress-test`, `GET /api/v1/risks/mitigations` |

---

### 6.2.5 Recommendation API  

| Attribute | Value |
|-----------|-------|
| Owner | Recommend Scenario (5.5) |
| Purpose | Generate recommendation, adopt or reject, view recommendation history. |
| Endpoints | `POST /api/v1/recommendations`, `POST /api/v1/recommendations/{id}/adopt`, `GET /api/v1/recommendations/{id}` |

---

### 6.2.6 Collaboration API  

| Attribute | Value |
|-----------|-------|
| Owner | Collaborate on Scenarios (5.6) |
| Purpose | Manage workshops, record challenges, reach consensus. |
| Endpoints | `POST /api/v1/workshops`, `GET /api/v1/workshops/{id}`, `POST /api/v1/workshops/{id}/consensus` |

---

### 6.2.7 Trigger API  

| Attribute | Value |
|-----------|-------|
| Owner | Sense Scenario Triggers (5.7) |
| Purpose | Query active triggers, manually evaluate triggers. |
| Endpoints | `GET /api/v1/triggers`, `POST /api/v1/triggers/evaluate` |

---

### 6.2.8 Quality API  

| Attribute | Value |
|-----------|-------|
| Owner | Evaluate Scenario Quality (5.8) |
| Purpose | Query scenario accuracy, calibration scores, quality reports. |
| Endpoints | `GET /api/v1/quality/accuracy`, `GET /api/v1/quality/calibration`, `GET /api/v1/quality/reports` |

---

### 6.2.9 Explanation API  

| Attribute | Value |
|-----------|-------|
| Owner | Explain Scenario Decisions (5.10) |
| Purpose | Retrieve structured explanation for any scenario artifact. |
| Endpoint | `GET /api/v1/explanations/{artifactId}` |

---

## 6.3 Integration Events  

Scenario Intelligence publishes events to the enterprise event bus (Kafka topic: `scenario-intelligence-events`).  

| Event Type | Payload Summary | Publisher Capability | Consumers |
|------------|-----------------|---------------------|-----------|
| `ScenarioDefined` | Scenario ID, type, assumptions | Define Scenarios | Simulate Scenarios, Compare Scenarios |
| `ScenarioPublished` | Scenario ID, catalogue version | Define Scenarios | All scenario capabilities |
| `SimulationStarted` | Run ID, scenario, plan, method | Simulate Scenarios | Monitor, Detect Exceptions |
| `SimulationCompleted` | Run ID, results summary, confidence | Simulate Scenarios | Compare Scenarios, Assess Risks, Recommend Scenario |
| `SimulationFailed` | Run ID, reason | Simulate Scenarios | Detect Scenario Exceptions |
| `ComparisonCompleted` | Comparison ID, ranking summary | Compare Scenarios | Recommend Scenario, Collaborate on Scenarios |
| `RiskAssessmentCompleted` | Risk ID, score, level | Assess Risks | Recommend Scenario |
| `StressTestCompleted` | Scenario ID, pass/fail | Assess Risks | Recommend Scenario |
| `ScenarioRecommendationMade` | Recommendation ID, plan variant, rationale | Recommend Scenario | All operational domains (Demand, Supply, Promise) |
| `ScenarioRecommendationAdopted` | Recommendation ID, adopted plan version | Recommend Scenario | Demand, Supply, Promise domains (plan updates) |
| `ScenarioRecommendationRejected` | Recommendation ID, reason | Recommend Scenario | Learn From Scenarios |
| `WorkshopConvened` | Workshop ID, participants | Collaborate on Scenarios | All stakeholders |
| `ConsensusReached` | Workshop ID, consensus level | Collaborate on Scenarios | Recommend Scenario |
| `ScenarioTriggerDetected` | Trigger ID, type, urgency | Sense Scenario Triggers | Simulate Scenarios |
| `ScenarioTriggerActioned` | Trigger ID, simulation IDs | Sense Scenario Triggers | Monitor |
| `ScenarioQualityReportPublished` | Report ID, period, metrics | Evaluate Scenario Quality | Learn From Scenarios, Management |
| `ScenarioExceptionDetected` | Exception ID, type | Detect Scenario Exceptions | Explain Scenario Decisions, Learn From Scenarios |
| `ScenarioExceptionResolved` | Exception ID, resolution | Detect Scenario Exceptions | Learn From Scenarios |
| `ScenarioExplanationGenerated` | Artifact ID, explanation | Explain Scenario Decisions | Audit, AI Agents |
| `ScenarioImprovementRecommended` | Type, target, benefit | Learn From Scenarios | All scenario capabilities |
| `ScenarioLearningLoopClosed` | Improvement ID, verdict | Learn From Scenarios | Evaluate Scenario Quality |

---

## 6.4 Import Interfaces  

| Interface | Format | Frequency | Target Capability |
|-----------|--------|-----------|-------------------|
| External Market Data Import | CSV / API | Daily | Sense Scenario Triggers |
| Risk Factor Update | CSV | Weekly | Assess Risks |
| Strategic Objective Import | JSON | Quarterly | Define Scenarios |
| Plan Variant Import | API (from Demand, Supply, Promise) | On demand | Simulate Scenarios |

---

## 6.5 Export Interfaces  

| Interface | Format | Frequency | Source Capability |
|-----------|--------|-----------|-------------------|
| Adopted Plan Export to Demand | API | On adoption | Recommend Scenario |
| Adopted Plan Export to Supply | API | On adoption | Recommend Scenario |
| Adopted Plan Export to Promise | API | On adoption | Recommend Scenario |
| Scenario Quality Report Distribution | PDF / Email | Quarterly | Evaluate Scenario Quality |
| Risk Assessment Report | PDF | On completion | Assess Risks |

---

# Chapter 7 — Reports & Dashboards  

## 7.1 Purpose  

This chapter consolidates every report and dashboard defined across the eleven Scenario Intelligence capabilities.  

## 7.2 Reports  

| Report ID | Name | Source Capability | Audience | Frequency | Content Summary |
|-----------|------|-------------------|----------|-----------|-----------------|
| RPT‑SN‑001 | Scenario Catalogue Report | Define Scenarios | Scenario Manager | Quarterly | All active scenarios, coverage gaps |
| RPT‑SN‑002 | Simulation Execution Report | Simulate Scenarios | Simulation Specialist | Weekly | Run times, success rates, confidence distributions |
| RPT‑SN‑003 | Probabilistic Analysis Report | Simulate Scenarios | Risk Manager | Per simulation | VaR, CVaR, outcome distributions |
| RPT‑SN‑004 | Scenario Comparison Report | Compare Scenarios | Decision‑makers | Per comparison | Ranking, Pareto frontier, trade‑offs |
| RPT‑SN‑005 | Risk Assessment Report | Assess Risks | Risk Committee | Quarterly | Risk heatmap, top risks, trends |
| RPT‑SN‑006 | Stress Test Report | Assess Risks | Risk Committee | Per test | Stress test results, breaking points |
| RPT‑SN‑007 | Recommendation Summary Report | Recommend Scenario | Executive | Monthly | Recommendations made, adopted, rejected |
| RPT‑SN‑008 | Workshop Summary Report | Collaborate on Scenarios | Scenario Manager | Per workshop | Attendance, decisions, action items |
| RPT‑SN‑009 | Trigger Response Report | Sense Scenario Triggers | Scenario Manager | Monthly | Detection to action time, automation rate |
| RPT‑SN‑010 | Scenario Quality Report | Evaluate Scenario Quality | Executive | Quarterly | Accuracy, calibration, process metrics |
| RPT‑SN‑011 | Scenario Exception Summary Report | Detect Scenario Exceptions | Scenario Manager | Monthly | Exceptions by type, severity, resolution time |
| RPT‑SN‑012 | Continuous Improvement Report (Scenario) | Learn From Scenarios | Scenario Manager | Quarterly | Improvements proposed, implemented, verified |

---

## 7.3 Dashboards  

| Dashboard ID | Name | Source Capabilities | Audience | Refresh | Panels Summary |
|--------------|------|---------------------|----------|---------|----------------|
| DASH‑SN‑001 | Scenario Catalogue Dashboard | Define Scenarios | Scenario Manager | Daily | Scenario inventory, status, coverage gauges |
| DASH‑SN‑002 | Scenario Lineage Viewer | Define Scenarios | Audit | On‑demand | Version history, evolution |
| DASH‑SN‑003 | Simulation Monitor | Simulate Scenarios | Simulation Specialist | Real‑time | Run status, queue depth, resource utilization |
| DASH‑SN‑004 | Scenario Outcome Explorer | Simulate Scenarios | Decision‑makers | On‑demand | Interactive distribution charts |
| DASH‑SN‑005 | Comparison Workbench | Compare Scenarios | Decision‑makers | On‑demand | Ranking, Pareto chart, trade‑off visualizations |
| DASH‑SN‑006 | Scenario Scorecard | Compare Scenarios | Decision‑makers | On‑demand | KPI comparison across variants and scenarios |
| DASH‑SN‑007 | Risk Heatmap Dashboard | Assess Risks | Risk Committee | Daily | Visual risk scores by category |
| DASH‑SN‑008 | Stress Test Monitor | Assess Risks | Risk Committee | On‑demand | Pass/fail status, recovery projections |
| DASH‑SN‑009 | Recommendation Dashboard | Recommend Scenario | Executive | Daily | Active recommendations, adoption rates, decision confidence |
| DASH‑SN‑010 | Collaboration Hub | Collaborate on Scenarios | All stakeholders | Daily | Active workshops, consensus status, pending actions |
| DASH‑SN‑011 | Stakeholder Engagement Dashboard | Collaborate on Scenarios | Scenario Manager | Monthly | Participation rates, challenge activity |
| DASH‑SN‑012 | Trigger Monitor | Sense Scenario Triggers | Scenario Manager | Real‑time | Trigger feed, response status |
| DASH‑SN‑013 | Scenario Quality Dashboard | Evaluate Scenario Quality | Executive | Quarterly | Accuracy trends, calibration charts |
| DASH‑SN‑014 | Scenario Performance Scorecard | Evaluate Scenario Quality | Executive | Quarterly | KPI‑by‑KPI predicted vs. actual |
| DASH‑SN‑015 | Scenario Exception Monitor | Detect Scenario Exceptions | Scenario Manager | Real‑time | Live exception feed, SLA status |
| DASH‑SN‑016 | Learning Dashboard (Scenario) | Learn From Scenarios | Scenario Manager | Monthly | Improvement funnel, effectiveness index |

---

# Chapter 8 — Appendix  

## 8.1 Scenario Exception Priority Matrix  

The following matrix defines the default mapping from Scenario Exception Type and Decision Impact to Exception Severity. It is referenced by DE‑SN‑091 (Prioritize Scenario Exception) in Section 5.9.  

| Exception Type | Blocks Strategic Decision | Blocks Tactical Decision | Degrades Quality | Informational |
|----------------|--------------------------|--------------------------|------------------|---------------|
| Simulation Failure | Critical | High | Medium | Low |
| Calibration Drift | High | Medium | Medium | Low |
| Recommendation Rejection | Critical | High | High | Medium |
| Trigger Failure | High | High | Medium | Low |
| Data Gap | High | Medium | Medium | Low |
| Quality Degradation | High | Medium | Medium | Low |
| Transient Error | — | — | — | Low |

**Notes:**  
- Transient Errors (infrastructure‑related, self‑resolving) are classified but not prioritized beyond Low unless they recur.  
- The matrix is configurable via the learning feedback loop (DE‑SN‑111) subject to policy PO‑SN‑111.  

---

## 8.2 Enterprise Glossary  

A consolidated glossary of all enterprise terms defined across the Scenario Intelligence Specification.  

| Term | ID (if any) | Definition |
|------|-------------|------------|
| Adopted Plan | SE‑SN‑033 | Recommended plan formally approved and transmitted for execution. |
| Alternative Plan | SE‑SN‑031 | Proposed modification to the baseline plan being evaluated. |
| Baseline Plan | SE‑SN‑030 | Current adopted plan used as the reference point for scenario comparison. |
| Calibration Drift | — | Degradation of probability calibration over time. |
| Collaborative Scenario | SE‑SN‑072 | Scenario jointly defined or reviewed by multiple stakeholders. |
| Comparison Criteria | SE‑SN‑051 | Dimensions on which plan variants are evaluated (cost, service, risk, etc.). |
| Consensus Scenario | SE‑SN‑073 | Scenario with formal stakeholder agreement; dissenting views recorded. |
| Deterministic Simulation | — | Single‑point assumptions produce a single outcome. |
| Monte Carlo Simulation | — | Probabilistic simulation using random sampling from input distributions. |
| Pareto Frontier | SE‑SN‑053 | Set of plan variants not dominated by any other variant on all criteria. |
| Plan Robustness Score | PI‑SN‑002 | Weighted percentage of scenarios in which a plan meets all performance thresholds. |
| Plan Variant | SE‑SN‑003 | Specific version of an enterprise plan evaluated under scenarios. |
| Probabilistic Outcome | SE‑SN‑024 | Probability distribution of a KPI from a probabilistic simulation. |
| Recommended Plan | SE‑SN‑032 | Plan variant emerging from scenario analysis as the preferred choice. |
| Resilience Index | PI‑SN‑011 | Measure of enterprise ability to maintain performance during and after disruption. |
| Risk Appetite | SE‑SN‑045 | Level of risk the enterprise is willing to accept. |
| Risk Factor | SE‑SN‑040 | Variable or event that can cause outcomes to deviate from plan. |
| Risk Mitigation | SE‑SN‑043 | Action to reduce probability or impact of a risk. |
| Risk Score | SE‑SN‑042 | Probability × Impact; quantifies risk magnitude. |
| Scenario | SE‑SN‑001 | Coherent description of a possible future state with defined assumptions. |
| Scenario Assumption | SE‑SN‑014 | Specific parameter override applied in a scenario. |
| Scenario Catalogue | — | Authoritative collection of all active and archived scenario definitions. |
| Scenario Horizon | SE‑SN‑012 | Future time span covered by a scenario. |
| Scenario Lineage | SE‑SN‑083 | Complete history of a scenario from creation through adoption. |
| Scenario Outcome | SE‑SN‑004 | Projected KPI values from simulating a plan variant against a scenario. |
| Scenario Trigger | SE‑SN‑005 | Event or condition that initiates a scenario analysis. |
| Scenario Type | SE‑SN‑011 | Baseline, Upside, Downside, Stress Test, Strategic, Event‑Driven. |
| Sensitivity Variable | SE‑SN‑060 | Key input systematically varied to test impact on outcomes. |
| Simulation | SE‑SN‑002 | Computational process of projecting outcomes given a plan and assumptions. |
| Simulation Confidence | SE‑SN‑023 | Score (0–100%) reflecting reliability of simulation output. |
| Simulation Engine | SE‑SN‑020 | Computational component that executes scenario simulations. |
| Simulation Run | SE‑SN‑025 | Single execution of the simulation engine; unique and reproducible. |
| Stakeholder | SE‑SN‑070 | Person or role with interest in scenario outcomes and decision authority. |
| Stress Test | SE‑SN‑044 | Extreme but plausible scenario to test plan limits. |
| Tornado Chart | SE‑SN‑063 | Visualization ranking sensitivity variables by impact. |
| Trade‑Off Analysis | SE‑SN‑052 | Evaluation of how improving one criterion degrades another. |
| Value‑at‑Risk (VaR) | — | Maximum expected loss at a given confidence level over a defined horizon. |
| Workshop | SE‑SN‑071 | Structured collaborative session for scenario review and consensus building. |

---

## 8.3 Formula Reference  

Complete set of formulas used in Chapter 3 (Enterprise Measurement Model).  

**PI‑SN‑002 — Plan Robustness Score**  
```
Plan Robustness Score (%) = ( Σ (Scenario Weight × Pass Factor) ÷ Σ Scenario Weight ) × 100
```
Where Pass Factor = 1 if the plan meets all defined performance thresholds in that scenario; 0 otherwise.  

**PI‑SN‑003 — Risk Reduction Impact**  
```
Risk Reduction Impact (%) = ( (Risk Score Before − Risk Score After) ÷ Risk Score Before ) × 100
Risk Score = Probability (%) × Impact (currency or score)
```

**PI‑SN‑004 — Scenario Analysis Cycle Time**  
```
Cycle Time = Time(Recommendation Delivered) − Time(Scenario Triggered)
```

**PI‑SN‑005 — Scenario Recommendation Adoption Rate**  
```
Adoption Rate (%) = ( Number of Recommendations Adopted ÷ Total Number of Recommendations Made ) × 100
```

**PI‑SN‑006 — Forecast Value of Scenario Analysis**  
```
Value of Scenario Analysis = Expected Value (with scenario analysis) − Expected Value (without scenario analysis)
Expected Value = Σ (Scenario Probability × Outcome Value)
```

**PI‑SN‑007 — Stress Test Coverage**  
```
Stress Test Coverage (%) = ( Number of Critical Elements Stress‑Tested ÷ Total Number of Critical Elements ) × 100
```

**PI‑SN‑008 — Scenario Accuracy**  
```
Scenario Accuracy (%) = ( 1 − |Predicted Outcome − Actual Outcome| ÷ Actual Outcome ) × 100
```
Aggregated across KPIs using weighted average.  

**PI‑SN‑009 — Decision Confidence Improvement**  
```
Confidence Improvement (pp) = Average Confidence (after scenario review) − Average Confidence (before scenario review)
```

**PI‑SN‑010 — Cost of Delay Avoided**  
```
Cost of Delay Avoided = (Loss per Day of Delay) × (Days Saved by Scenario Analysis)
```

**PI‑SN‑011 — Resilience Index**  
```
Resilience Index = (1 − (Performance Loss During Disruption ÷ Normal Performance)) × Recovery Speed Factor
Recovery Speed Factor = 1 − (Time to Recover ÷ Maximum Acceptable Recovery Time)
```

**PI‑SN‑012 — Scenario Comparison Completeness**  
```
Comparison Completeness (%) = ( Number of Criteria Actually Evaluated ÷ Total Number of Defined Criteria ) × 100
```

**PI‑SN‑013 — Collaborative Scenario Participation Rate**  
```
Participation Rate (%) = ( Number of Stakeholders Who Participated ÷ Total Number of Invited Stakeholders ) × 100
```

**PI‑SN‑014 — Planning Cycle Time (Scenario)**  
```
Planning Cycle Time = Time(Cycle Closed) − Time(Cycle Initiated)
```

**PI‑SN‑015 — Strategic Alignment Score**  
```
Strategic Alignment Score (%) = ( Σ Alignment Score per Recommendation ÷ Number of Recommendations ) × 100
```
Where each recommendation is scored 1–5 on alignment, normalized to 0–100%.  

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
- Scenario Intelligence Specification (this document)  

### Dependency Specifications  
- Knowledge Intelligence Specification (future)  

---