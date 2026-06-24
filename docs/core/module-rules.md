# Module Rules

## Purpose

This document defines practical rules for communication between modules in the modular monolith.

Each module owns its own:

- domain model
- application use cases
- database schema
- infrastructure
- integration events

Modules must not reference each other's domain models directly.

---

## Communication Rules

Use different communication styles for different consistency needs.

### 1. Domain Logic

Domain models must not communicate across module boundaries.

Allowed:

```text
Session.Application -> Session.Domain
TaskPlanning.Application -> TaskPlanning.Domain
```

Not allowed:

```text
Session.Domain -> TaskPlanning.Domain
Session.Domain -> TaskPlanning.Application
TaskPlanning.Domain -> Session.Domain
```

Aggregates should only enforce rules that belong to their own bounded context.

---

### 2. Synchronous Application Contracts

Use synchronous application contracts when a command requires immediate cross-module validation.

Example:

```text
Assign Task To Session
```

The Session module owns the assignment command because assignment affects an active execution container.

Before saving the assignment, Session may need to ask Task Planning:

```text
Can this Task be assigned right now?
```

This should be done through a narrow contract such as:

```csharp
public interface ITaskAssignmentEligibilityChecker
{
    Task<TaskAssignmentEligibilityResult> CheckAsync(
        Guid taskId,
        Guid ownerId,
        CancellationToken cancellationToken);
}
```

This is a synchronous application-layer check.

It is not a domain dependency.
It is not an integration event.
It is not an HTTP call between modules inside the monolith.

---

### 3. Integration Events

Use integration events after a business fact has happened.

Examples:

- `TaskAssignedToSessionIntegrationEvent`
- `SessionStoppedIntegrationEvent`
- `TaskCompletedIntegrationEvent`
- `EvidenceItemAddedIntegrationEvent`

Integration events are suitable for:

- read model updates
- replay projections
- analytics
- notifications
- downstream processing
- eventual consistency between modules

Integration events should not be used for immediate command validation.

Bad:

```text
Session waits for TaskPlanning integration event to know if Task is assignable.
```

Good:

```text
Session calls TaskPlanning eligibility contract before assignment.
Session publishes TaskAssignedToSessionIntegrationEvent after assignment.
```

---

## Contract Project Organization

Prefer one application contract project per provider module.

Recommended:

```text
be/Modules/TaskPlanning/
  PCL.Modules.TaskPlanning.Contracts/
    Tasks/
      ITaskAssignmentEligibilityChecker.cs
      TaskAssignmentEligibilityResult.cs

be/Modules/Session/
  PCL.Modules.Session.Contracts/
```

Avoid putting all module contracts into one global project by default:

```text
PCL.Shared.Contracts
```

A global shared contracts project can easily become a dumping ground.

Use `Common` only for stable cross-cutting abstractions such as:

- IDs
- timestamps
- base result types
- messaging abstractions
- domain event abstractions

Do not put module-specific business contracts in `Common`.

---

## Dependency Direction

The module that owns the business fact owns the contract.

Example:

Task Planning owns task eligibility.

Therefore:

```text
TaskPlanning.Contracts
  contains ITaskAssignmentEligibilityChecker

TaskPlanning.Application
  implements ITaskAssignmentEligibilityChecker

Session.Application
  references TaskPlanning.Contracts
```

Allowed:

```text
Session.Application -> TaskPlanning.Contracts
TaskPlanning.Application -> TaskPlanning.Contracts
Host -> wires implementation with dependency injection
```

Not allowed:

```text
Session.Application -> TaskPlanning.Application
Session.Application -> TaskPlanning.Domain
Session.Domain -> TaskPlanning.*
```

---

## Naming Conventions

Project names:

```text
PCL.Modules.{ModuleName}.Contracts
```

Contract interface names should describe a narrow use case:

```text
ITaskAssignmentEligibilityChecker
ISessionMutabilityChecker
IEvidenceAttachmentEligibilityChecker
```

Avoid broad service names:

```text
ITaskService
ISessionClient
ICommonDataProvider
```

Contract DTOs should be specific to the use case:

```text
TaskAssignmentEligibilityResult
TaskAssignmentEligibilityFailureReason
TaskSnapshot
```

Integration events should use past-tense business facts:

```text
TaskAssignedToSessionIntegrationEvent
SessionStoppedIntegrationEvent
EvidenceItemAddedIntegrationEvent
```

---

## Contract Design Rules

Contracts must be small and stable.

Do:

- expose only data needed by the consuming use case
- return primitive values or contract-specific DTOs
- document consistency expectations
- keep methods narrow
- keep provider module ownership clear

Do not:

- return domain entities
- expose EF entities
- expose database models
- expose broad CRUD services
- let another module mutate provider-owned state through generic methods

Bad:

```csharp
public interface ITaskService
{
    Task<TaskDto> GetTaskAsync(Guid taskId);
    Task UpdateTaskAsync(TaskDto task);
    Task DeleteTaskAsync(Guid taskId);
}
```

Good:

```csharp
public interface ITaskAssignmentEligibilityChecker
{
    Task<TaskAssignmentEligibilityResult> CheckAsync(
        Guid taskId,
        Guid ownerId,
        CancellationToken cancellationToken);
}
```

---

## Rule For Adding A New Contract

Before adding a synchronous contract, answer:

1. Is immediate consistency required?
2. Can this be handled by an integration event instead?
3. Which module owns the business fact?
4. Is the interface narrow and use-case-specific?
5. Does it avoid returning domain models?
6. Does it avoid cross-module writes?
7. What should happen when the provider rejects the request?
8. Can the contract remain stable if the provider domain evolves?

If immediate consistency is not required, prefer an integration event and a local projection.

---

## Session And Task Planning Example

Use case:

```text
AssignTaskToSession
```

Ownership:

- Session owns the public command endpoint.
- Session owns `session_task_assignments`.
- Task Planning owns Task existence, owner, status, and assignability.

Command flow:

```text
Client
  -> Session endpoint
  -> AssignTaskToSessionCommandHandler
  -> load LSession
  -> call ITaskAssignmentEligibilityChecker
  -> call LSession.AssignTask
  -> save Session transaction
  -> publish TaskAssignedToSessionIntegrationEvent
```

Session-side validations:

- Session exists.
- Session is Active.
- Task is not already assigned to this Session.
- Task is not assigned to another Active Session if V1 disallows that.

TaskPlanning-side validations:

- Task exists.
- Task belongs to the same owner.
- Task is `Planned` or `Active`.
- Task is not `Draft`, `Completed`, or `Cancelled`.

Integration event usage after assignment:

- Task Planning may update a task assignment read model.
- Replay may update a session timeline projection.
- Analytics may update derived metrics.

The integration event must not be used to decide whether the assignment is valid.

---

## Local Projections

Local projections are useful for fast reads and display.

Examples:

- Session may keep a small task snapshot projection for display.
- Task Planning may keep a task-to-session assignment read model.
- Replay may keep a timeline projection.

Local projections should not be used for strict command validation unless stale data is acceptable.

If strict validation is required, use a synchronous application contract.

---

## Versioning And Evolution

Treat contract projects as public APIs inside the monolith.

Safe changes:

- add optional DTO fields
- add new result reason values when consumers tolerate unknown values
- add a new interface for a new use case

Risky changes:

- rename methods
- remove DTO fields
- change field meaning
- expose richer domain-shaped DTOs
- reuse a contract for a different business meaning

When semantics change materially, prefer a new contract:

```text
ITaskAssignmentEligibilityChecker
ITaskConcurrentAssignmentPolicyChecker
```

For integration events, create a new version only when payload meaning changes:

```text
TaskAssignedToSessionIntegrationEventV2
```

---

## Common Scaling Problems

### Contract Layer Becomes Shared Domain

Problem:

```text
Contracts contain large TaskDto or SessionDto objects.
```

Avoid by using use-case-specific DTOs.

### Too Many Synchronous Dependencies

Problem:

```text
One command calls five other modules before it can save.
```

Avoid by reserving synchronous contracts for hard consistency rules.

### Circular Module Pressure

Problem:

```text
Session needs TaskPlanning contracts.
TaskPlanning needs Session contracts.
Both modules start coordinating too many decisions.
```

Avoid by clarifying which module owns the command and which module only supplies facts.

### Stale Projections Used For Hard Rules

Problem:

```text
Session assigns a Task based on an outdated TaskStatus projection.
```

Avoid by using synchronous contracts for hard validation.

### Cross-Module Writes Through Contracts

Problem:

```text
Session calls TaskPlanning.MarkTaskActive().
```

Avoid by keeping writes inside the owning module. Use explicit commands only when ownership is clear, or publish integration events for downstream reactions.

---

## Final Rule

Use this default decision model:

```text
Need immediate validation?
  -> synchronous application contract

Need downstream reaction after a fact happened?
  -> integration event

Need fast display/query?
  -> local projection/read model

Need core business rule?
  -> owning aggregate inside owning module
```
