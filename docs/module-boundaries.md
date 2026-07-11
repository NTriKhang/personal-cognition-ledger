# Module boundaries

## Purpose

These rules preserve independent business ownership inside the modular monolith. They apply to Task Planning, Session, Evidence, and future modules.

## Ownership rule

A module exclusively owns:

- its domain model and business invariants
- its database schema and persistence mappings
- commands and queries that change or expose its state
- its public application and integration contracts
- the meaning of facts it publishes

Another module may reference an identifier or maintain a local projection, but it does not acquire ownership of the source entity.

## Allowed communication

Choose communication based on the consistency required by the use case.

| Need | Mechanism | Example |
| :--- | :--- | :--- |
| Immediate answer required to enforce a hard invariant | Provider-owned synchronous application contract | Session checks whether a Task is eligible for assignment |
| Reaction may occur after the source transaction commits | Integration event | Task Planning activates a Planned Task after Session assignment |
| Data needed repeatedly for local reads or decisions tolerant of staleness | Consumer-owned local projection | Future Timeline projection of Session facts |

### Domain logic

Domain objects never call another module. A Domain project must not reference another module's projects, application services, DbContext, or messaging infrastructure.

Cross-module orchestration belongs in the Application layer. The consumer asks a provider contract for facts, then invokes its own domain behavior.

### Synchronous application contracts

Use a synchronous contract only when the current request cannot safely proceed without an immediate answer.

The provider module owns the contract because it owns the meaning of the answer. A contract should:

- express a business capability, not generic data access
- return the minimum immutable facts needed by the consumer
- avoid returning provider aggregates, EF entities, or queryables
- avoid accepting consumer-specific infrastructure types
- define explicit missing, ineligible, or unavailable outcomes
- remain read-only with respect to another module's state unless the named capability intentionally represents a provider-owned command

Prefer:

```text
CheckTaskAssignmentEligibility(taskId, sessionOwnerId)
    -> Eligible | NotFound | OwnerMismatch | NotAssignable
```

Avoid:

```text
GetTaskEntity(taskId)
GetTableRows(tableName, predicate)
ExecuteSqlInTaskSchema(sql)
```

### Integration events

Publish an integration event for a committed fact that other modules may react to eventually.

An integration event should:

- use past-tense business language
- contain stable identifiers and the facts consumers need
- avoid domain entities and infrastructure types
- be safe for duplicate delivery
- evolve compatibly or receive an explicit version when compatibility cannot be preserved

The publishing transaction must store the event in its outbox. Consumers use inbox/idempotency infrastructure and cannot assume ordering unless the contract explicitly provides it.

Domain events are internal to the owning module. Map them to integration events at the module boundary; do not expose domain-event instances as public contracts.

### Local projections

A consumer may build a local projection from integration events when it needs fast local reads or can tolerate eventual consistency.

A projection:

- is owned by the consumer
- is rebuildable from authoritative facts or a defined reconciliation process
- must not be used to enforce a hard rule when staleness could violate that rule
- must not be written by the provider module

## Contract project organization

Each provider module may expose a small `Contracts` project. It may contain:

- synchronous application contract interfaces and result DTOs
- integration-event contracts when those are intentionally shared at compile time
- stable identifiers or enums only when they are genuinely part of the public contract

It must not contain:

- aggregates, entities, value objects, or repositories
- DbContext or persistence configuration
- application handlers
- generic shared-domain helpers
- consumer-specific DTOs

Contracts projects may depend on minimal common primitives. They must not depend on provider Infrastructure or Presentation.

## Dependency direction

Within a module:

```text
Presentation -> Application -> Domain
Infrastructure -> Application + Domain
```

Across modules:

```text
Consumer Application/Infrastructure -> Provider Contracts
```

Forbidden cross-module references include:

- Domain -> any other module
- Consumer -> provider Domain
- Consumer -> provider Infrastructure
- Consumer -> provider Presentation
- one module's DbContext -> another module's entities or tables

`PCL_API` is the composition root and may reference module Infrastructure projects to register them. That permission does not make the host a place for business orchestration.

## Naming

Use names that state business meaning and direction:

- `ITaskAssignmentEligibilityChecker`
- `TaskAssignmentEligibilityResult`
- `TaskAssignedToSessionIntegrationEvent`

Avoid generic names such as `IModuleService`, `IDataProvider`, `SharedTask`, or `CommonEntity`.

Commands request an action, queries request data, domain events describe an internal occurrence, and integration events describe a fact exposed beyond the module.

## Adding or changing a contract

Before adding a cross-module contract:

1. Identify the module that owns the fact or action.
2. Decide whether the consumer needs immediate consistency.
3. Prefer an integration event or projection when eventual consistency is acceptable.
4. Define the smallest provider-owned business contract.
5. Document failure and availability behavior.
6. Add provider contract tests and consumer integration coverage.
7. Update [Architecture](architecture.md), [Domain model](domain-model.md), or the [API reference](api/reference.md) if observable behavior changes.

## Scaling warnings

Review the design when:

- a Contracts project begins accumulating business behavior
- a module has many synchronous dependencies
- two modules need contracts from each other
- consumers request increasingly broad provider DTOs
- a stale projection is proposed for a hard invariant
- a consumer wants to write through a provider's tables

These are signals to revisit ownership or orchestration. They are not reasons to create a global shared domain.

## Final rule

Communicate business facts across explicit boundaries. Never share mutable domain state or database ownership across modules.
