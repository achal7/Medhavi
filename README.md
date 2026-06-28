# Medhāvī Supply Chain Platform

## Overview

**Medhāvī** is a next-generation, AI-native Advanced Planning and Scheduling (APS) platform built on a fundamentally different architectural philosophy. Instead of organizing enterprise planning around applications, transactions, or data models, Medhavi is organized around authoritative business understanding. It establishes a governed semantic foundation that defines what the enterprise knows before determining what it should do. Planning, optimization, and AI are treated as reasoning capabilities built upon this shared understanding—not as isolated technologies. The result is an explainable, traceable, deterministic, and continuously evolvable decision platform where every capability, decision, rule, policy, and AI recommendation can be traced back to a common business language and governed architectural principles.

## Documents

| Layer (Document)               | Core Concept      | Question it answers                       | Ends With                                      |
| ------------------------------ | ----------------- | ----------------------------------------- | ---------------------------------------------- |
| **Vision**                     | —                 | —                                         | —                                              |
| **Constitution**               | Principles        | What must always be true?                 | Principles                                     |
| **ARS**                        | Requirements      | How is architecture governed?             | Requirements                                   |
| **Semantic Model**             | Meaning           | What do we understand?                    | Enterprise Questions & Intelligence Domains    |
| **Capability Model**           | Abilities         | What can we do?                           | Enterprise Understanding                       |
| **Decision Model**             | Choices           | What choices do we make?                  | Enterprise Decisions                           |
| **Rule Model**                 | Logic             | How do we decide consistently?            | Business Rules                                 |
| **Policy Model**               | Governance        | What is configurable?                     | Policies                                       |
| **Functional Specification**   | Behaviour         | What does the system do?                  | Commands, Events, Queries (within each domain) |
| **Architecture Blueprint**     | Software Design   | How is it architected?                    | Software Components                            |

---

### Architecture Evolution Diagram

```
                 Constitution
                       │
                       ▼
     Architectural Requirement Specification
                       │
                       ▼
                Semantic Model
                       │
                       ▼
               Capability Model
                       │
                       ▼
                Decision Model      
                       │
                       ▼
                  Rule Model
                       │
                       ▼
                 Policy Model
                       │
                       ▼
   Intelligence Domain Realization Specifications
        (containing Functional Behaviour,
            Commands, Events, Queries)
                       │
                       ▼
            Architecture Blueprint
```

---

### “How architecture documents evolve” (linear view)

```
Semantic Model
        │
Meaning │
        ▼
Capability Model
        │
Understanding
        ▼
Decision Model
        │
Recommendations
        ▼
Rule Model
        │
Validated Recommendations
        ▼
Policy Model
        │
Governed Recommendations
        │
        ▼
Intelligence Domain Realization Specifications
        │
System Behaviour (Commands, Events, Queries)
        │
        ▼
Architecture Blueprint
```


## Architecture

The platform consists of three interconnected systems:

### 🏭 Medhavi.Integrator
Data ingestion and anti-corruption layer for multi-source data integration.

### 🧠 Nexus
AI-powered control tower for intelligent orchestration and real-time decision making.

### 📊 ProductionPlanning
Tactical planning engine for APS (Advanced Planning Systems) and optimization.

## Technology Stack

- **Language**: F#
- **Framework**: .NET 10.0
- **Architecture**: Event-Driven Architecture (EDA)
- **Persistence**: Postgres, Merten (For EventStore)
- **Communication**: gRPC, REST APIs, WebSockets

## Key Features

- **Real-time Event Processing**: Sub-200ms latency processing
- **AI/ML Integration**: Autonomous optimization and predictive analytics
- **Digital Twin Management**: Live supply chain representation
- **Advanced Analytics**: Causal AI and predictive modeling
- **Multi-objective Optimization**: Cost, delivery, carbon, and quality balancing

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- EventStoreDB
- Git

### Development Setup

1. Clone the repository
2. Restore dependencies: `dotnet restore`
3. Build the solution: `dotnet build`
4. Run EventStoreDB locally
5. Start the desired service

## Contributing

This is a solo development project following a structured 32-phase implementation plan. See `documents/MASTER-PROJECT-STRUCTURE.md` for detailed planning and roadmap.

## License

GNU General Public License

## Contact

achal7@gmail.com [Achal Shah]