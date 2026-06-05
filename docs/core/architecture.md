# PCL API — Architecture Summary

## 1. Overview

PCL API is a .NET 9 backend application designed using a Modular Monolith architecture combined with:

- Clean Architecture
- Domain-Driven Design (DDD)
- CQRS
- Event-Driven Internal Communication

The system is organized around business modules, where each module encapsulates its own:

- Application layer
- Domain layer
- Infrastructure layer
- Presentation layer
- Integration events

The architecture is intentionally designed to:

- maximize modularity
- reduce coupling between domains
- support maintainability and scalability
- prepare for future microservice extraction

---

# 2. High-Level Architecture

```text
Clients
   |
   v
PCL_API (Host / Composition Root)
   |
   +------------------------------------------------+
   |                                                |
   v                                                v
Business Modules                             Common Core
(Session, Events, Ticketing, ...)            Shared abstractions
   |                                          Infrastructure support
   |                                          Cross-cutting concerns
   |
   v
PostgreSQL
```

---

# 3. Architectural Style

## Modular Monolith

Each business capability is isolated into an independent module.

A module owns:

- its domain model
- its application logic
- its infrastructure implementation
- its integration contracts

Modules communicate primarily through integration events rather than direct database access.

When immediate cross-module validation is required inside the monolith, use narrow synchronous application contracts owned by the provider module. See `docs/core/module-rules.md`.

---

## Clean Architecture

Each module follows layered separation:

```text
Presentation
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
```

### Presentation Layer

Responsibilities:

- Controllers / endpoints
- Request mapping
- Authentication boundary
- API exposure

### Application Layer

Responsibilities:

- CQRS handlers
- Use cases
- Orchestration
- Transaction coordination
- Integration event triggering

### Domain Layer

Responsibilities:

- Aggregates
- Entities
- Value objects
- Domain events
- Business rules

### Infrastructure Layer

Responsibilities:

- EF Core
- Dapper
- PostgreSQL access
- Messaging
- External service integrations
- Persistence implementations

---

# 4. CQRS + MediatR

The system uses CQRS with MediatR.

## Commands

Commands represent state-changing operations.

Examples:

- CreateSessionCommand
- PublishEventCommand
- PurchaseTicketCommand

Commands are processed by Command Handlers.

---

## Queries

Queries are optimized for data retrieval.

Dapper is primarily used for read operations requiring:

- better query performance
- projection optimization
- lightweight data access

---

# 5. Persistence Strategy

The application uses PostgreSQL as the primary database.

## EF Core

Used mainly for:

- transactional write operations
- aggregate persistence
- change tracking
- domain consistency

## Dapper

Used mainly for:

- read models
- optimized queries
- reporting scenarios
- lightweight projections

---

# 6. Event-Driven Architecture

The system implements asynchronous module communication using integration events.

This enables:

- low coupling
- eventual consistency
- module isolation
- future microservice migration readiness

---

# 7. Outbox / Inbox Pattern

The system implements reliable event delivery using:

- Outbox Pattern
- Inbox Pattern

## Outbox Flow

```text
1. Business data is persisted
2. Integration event is stored in Outbox table
3. Transaction commits
4. Background job scans Outbox
5. MassTransit publishes event
```

EF Core interceptors are used to automatically capture and persist integration events during SaveChanges.

---

## Inbox Flow

```text
1. Consumer receives event
2. Event stored in Inbox table
3. Background job scans Inbox
4. Matching IntegrationEventHandler is resolved
5. Event processing executed
```

This ensures:

- reliable processing
- retry capability
- idempotency support
- decoupled asynchronous handling

---

# 8. Messaging Infrastructure

MassTransit is used for:

- event publishing
- event consumption
- asynchronous communication

The messaging layer abstracts inter-module event propagation.

Current implementation behaves as internal event-driven communication within the monolith.

---

# 9. Authentication

The application uses JWT-based authentication.

JWT tokens are used to:

- authenticate API requests
- identify users
- secure protected endpoints

Authentication is configured at the API host layer.

---

# 10. Common Layer Responsibilities

The Common layer acts as the architectural foundation of the system.

Typical responsibilities include:

- shared abstractions
- base domain types
- middleware
- exception handling
- background processing
- messaging infrastructure
- cross-cutting concerns
- common utilities
- shared contracts

---

# 11. Example End-to-End Flow

```text
Client Request
    ↓
Controller
    ↓
Command
    ↓
MediatR
    ↓
Command Handler
    ↓
Repository + Domain Logic
    ↓
EF Core Transaction
    ↓
Business Data Saved
    ↓
Outbox Event Saved
    ↓
Background Job
    ↓
MassTransit Publish
    ↓
Consumer Receives Event
    ↓
Inbox Saved
    ↓
Background Job
    ↓
IntegrationEventHandler
    ↓
Module Reaction
```

---

# 12. Architectural Goals

The architecture prioritizes:

- maintainability
- modularity
- domain isolation
- scalability
- future service extraction
- clean dependency boundaries
- long-term evolvability
