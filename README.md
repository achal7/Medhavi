# Medhāvī Supply Chain Platform

## Overview

**Medhāvī** is an AI-first supply chain orchestration platform that leverages event-driven architecture, artificial intelligence, digital twins, and real-time analytics to provide end-to-end supply chain management for manufacturing companies.

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