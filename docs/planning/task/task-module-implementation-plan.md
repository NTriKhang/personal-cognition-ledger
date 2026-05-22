# Task Module Implementation Plan - Personal Cognition Ledger

## Purpose

This document defines a practical backend implementation roadmap for the Task module.

The goal is to implement Task Planning as a small, stable module inside the Personal Cognition Ledger modular monolith.

This is not a feature wishlist.

It is an incremental engineering plan aligned with:

- Modular Monolith architecture
- Clean Architecture
- Domain-Driven Design
- CQRS
- event-driven internal communication
- PostgreSQL
- EF Core for writes
- Dapper for reads

The implementation should remain suitable for a solo developer.

Optimize for:

- low cognitive load
- small deliverable increments
- stable domain foundations
- clear boundaries with Session
- future compatibility without speculative complexity

---

# 1. Module Goals

## Business Capability

The Task module provides the Task Planning capability.

It allows the user to declare, organize, prioritize, and resolve intended work.

A Task represents intention.

It helps the system answer:

- what the user planned to do
- what work is still relevant
- what work was completed
- what work was cancelled
- which intentions were associated with a Session

## Why The Module Exists

Personal Cognition Ledger is not only a time tracker.

It captures:

```text
Task = intent
Session = execution
Evidence = proof
Reflection = conclusion
```

Without Task Planning, a Session can show that work happened, but it cannot clearly show what the user intended to accomplish.

The Task module exists to preserve that intention as a first-class business concept.

## Problems It Solves

The Task module solves:

- capturing work before execution starts
- organizing planned work by category and priority
- linking intentions to Sessions
- distinguishing completed intent from stopped execution
- preserving cancelled or deferred intent for later reflection

## Relationship With Session

Task supplies intent to Session.

Session records actual execution.

A Task may exist without a Session.  
A Session may contain multiple Tasks.

Assigning a Task to a Session does not prove execution happened.

Stopping a Session does not automatically complete assigned Tasks.

---

# 2. Module Boundaries

## Task Owns

The Task module owns:

- Task identity
- Task title and description
- Task lifecycle state
- Task category
- Task priority
- Task completion decision
- Task cancellation decision
- Task planning domain events
- Task read models
- Task planning APIs

## Task Must Not Own

The Task module must not own:

- Session lifecycle
- Session start or stop times
- active Session consistency
- Evidence storage
- Activity execution tracking
- Replay projections
- Reflection interpretation
- AI-generated insights
- notification scheduling

## Upstream And Downstream Relationships

Task Planning is upstream to Session for declared intention.

Session is the transactional execution core.

Replay, Reflection, Analytics, and future AI pipelines are downstream consumers of Task and Session facts.

## Integration Points With Session

The Task module integrates with Session through:

- assigning a Task to a Session
- removing a Task assignment while allowed
- exposing Task facts to Session use cases
- emitting task assignment events
- consuming Session facts only when needed for validation

The initial implementation should keep integration synchronous and explicit.

Do not introduce distributed-service patterns.

---

# 3. Recommended Initial Scope (V1)

## Supported Task States

V1 should support the smallest useful lifecycle:

- Draft
- Planned
- Active
- Completed
- Cancelled

Archived can be documented but does not need behavior in V1.

## Supported Operations

V1 should support:

- draft task
- plan task
- refine task title and description
- categorize task
- prioritize task
- mark task active
- defer task back to planned
- complete task
- cancel task
- get task detail
- list tasks
- assign task to Session
- remove task assignment if Session is still mutable

## Excluded Future Features

Do not implement in V1:

- recurring tasks
- task templates
- AI-assisted planning
- complex scheduling
- calendar integration
- notifications
- collaboration
- task dependencies
- subtasks
- planning boards
- productivity scoring
- automatic task completion from Session stop

## Assumptions And Simplifications

Use these simplifications for V1:

- one owner per Task
- no shared Tasks
- no task hierarchy
- no recurrence
- no due-date workflow beyond optional metadata
- category and priority are simple value objects or constrained values
- assignment to Session is explicit
- completion is a user decision
- cancellation preserves history

---

# 4. Step-By-Step Implementation Phases

## Phase 1 - Domain Foundation

### Objective

Create the Task domain model and enforce lifecycle rules in the domain layer.

### Why This Phase Exists

Task lifecycle behavior is the source of truth for planning rules.

Starting with the domain prevents the module from becoming CRUD endpoints over a table.

### Implementation Details

Add:

- `Task` aggregate
- `TaskId` value object
- `TaskStatus` enum or value object
- `TaskTitle` value object if validation is non-trivial
- `TaskCategory` value object
- `TaskPriority` value object
- lifecycle behavior methods
- domain events

Recommended behavior methods:

- `Draft(...)`
- `Plan(...)`
- `Refine(...)`
- `Categorize(...)`
- `Prioritize(...)`
- `Activate(...)`
- `Defer(...)`
- `Complete(...)`
- `Cancel(...)`

Recommended invariants:

- Task has exactly one state
- Draft cannot be assigned to Session
- Completed and Cancelled are terminal
- CompletedAt is set exactly once
- CancelledAt is set exactly once
- completion does not affect Session lifecycle
- cancellation does not delete Session history

Recommended domain events:

- `TaskDraftedDomainEvent`
- `TaskPlannedDomainEvent`
- `TaskActivatedDomainEvent`
- `TaskDeferredDomainEvent`
- `TaskCompletedDomainEvent`
- `TaskCancelledDomainEvent`
- `TaskCategoryChangedDomainEvent`
- `TaskPriorityChangedDomainEvent`

### Expected Outcome

The Task aggregate can be tested without database or API concerns.

### Risks Or Anti-Patterns To Avoid

- anemic domain model
- public setters for lifecycle state
- infrastructure attributes inside domain classes
- using Session state as Task state
- completing Tasks automatically from Session stop

---

## Phase 2 - Persistence

### Objective

Persist Task aggregate state reliably with EF Core and PostgreSQL.

### Why This Phase Exists

The write model needs stable persistence before application commands can be useful.

### Implementation Details

Add:

- EF Core entity configuration for Task
- task table migration
- repository contract in Application or Domain boundary
- repository implementation in Infrastructure
- unit of work integration if already used
- optimistic concurrency column if the project already has that pattern

Suggested persisted fields:

- Id
- OwnerId
- Title
- Description
- Status
- Category
- Priority
- PlannedAt
- ActivatedAt
- CompletedAt
- CancelledAt
- CancellationReason
- CompletionNote
- CreatedAt
- UpdatedAt
- Version or RowVersion

Avoid persisting domain events as part of the Task table.

Let the existing outbox mechanism handle event persistence if available.

### Expected Outcome

Task aggregate state can be saved and restored through a repository.

### Risks Or Anti-Patterns To Avoid

- leaking EF Core types into domain
- designing schema for future recurrence now
- storing Session execution facts inside Task rows
- skipping migrations and relying on manual database drift

---

## Phase 3 - Application Layer

### Objective

Add command use cases that orchestrate Task behavior.

### Why This Phase Exists

Application handlers coordinate validation, authorization context, transactions, and persistence without moving business rules out of the aggregate.

### Implementation Details

Add commands:

- `DraftTaskCommand`
- `PlanTaskCommand`
- `RefineTaskCommand`
- `CategorizeTaskCommand`
- `PrioritizeTaskCommand`
- `ActivateTaskCommand`
- `DeferTaskCommand`
- `CompleteTaskCommand`
- `CancelTaskCommand`

Command handlers should:

- load the Task when required
- call aggregate behavior methods
- persist changes through repository
- commit one transaction per command
- rely on domain events for downstream reactions

Validation should cover:

- required title
- title length
- valid category
- valid priority
- required cancellation reason when cancelling from Active
- valid owner access

Validation should not duplicate aggregate lifecycle rules.

### Expected Outcome

Task behavior is available through command handlers with clear transaction boundaries.

### Risks Or Anti-Patterns To Avoid

- command handlers directly setting status
- putting business rules only in validators
- combining unrelated operations into large commands
- letting Task commands modify Session lifecycle

---

## Phase 4 - Query Layer

### Objective

Add read-optimized Task queries using Dapper.

### Why This Phase Exists

Task lists and detail screens are read concerns.

Dapper keeps query models lightweight and avoids loading aggregates for simple reads.

### Implementation Details

Add read models:

- `TaskSummaryReadModel`
- `TaskDetailReadModel`
- `TaskSessionAssignmentReadModel`

Add queries:

- `GetTaskByIdQuery`
- `ListTasksQuery`
- `ListTasksByStatusQuery`
- `ListTasksByCategoryQuery`
- `ListAssignableTasksQuery`

Initial filters:

- status
- category
- priority
- text search on title
- created date range

Keep read models separate from domain entities.

### Expected Outcome

The API can list and display Tasks without loading write aggregates.

### Risks Or Anti-Patterns To Avoid

- using Dapper results as domain objects
- implementing advanced search before basic filters
- adding analytics queries before real usage exists
- coupling query models to API request shapes too tightly

---

## Phase 5 - API Layer

### Objective

Expose Task Planning use cases through stable HTTP endpoints.

### Why This Phase Exists

The module needs a usable boundary for clients while preserving application and domain layering.

### Implementation Details

Add endpoints such as:

- `POST /tasks/draft`
- `POST /tasks/{taskId}/plan`
- `PUT /tasks/{taskId}/details`
- `PUT /tasks/{taskId}/category`
- `PUT /tasks/{taskId}/priority`
- `POST /tasks/{taskId}/activate`
- `POST /tasks/{taskId}/defer`
- `POST /tasks/{taskId}/complete`
- `POST /tasks/{taskId}/cancel`
- `GET /tasks/{taskId}`
- `GET /tasks`

Use request and response contracts that reflect business behavior.

Examples:

- `CompleteTaskRequest`
- `CancelTaskRequest`
- `RefineTaskRequest`
- `TaskResponse`
- `TaskSummaryResponse`

Recommended status codes:

- `201 Created` for newly drafted Tasks
- `200 OK` for successful reads and state changes returning data
- `204 No Content` for successful commands returning no body
- `400 Bad Request` for invalid input
- `401 Unauthorized` for missing authentication
- `403 Forbidden` for invalid ownership
- `404 Not Found` for missing Task
- `409 Conflict` for invalid lifecycle transitions or concurrency conflicts

### Expected Outcome

The Task module can be exercised end-to-end through APIs.

### Risks Or Anti-Patterns To Avoid

- exposing database models
- using generic `UpdateTaskStatus` endpoints
- returning infrastructure exceptions directly
- adding endpoints for future features not implemented yet

---

## Phase 6 - Session Integration

### Objective

Implement Task-to-Session assignment without mixing Task and Session lifecycles.

### Why This Phase Exists

V1 requires the system to answer what the user intended to do in a Session.

This requires a relationship between Task and Session, not shared ownership.

### Implementation Details

Add `SessionTaskAssignment`.

Possible ownership options:

- inside Session module if assignment is treated as part of Session preparation
- inside Task Planning module if assignment is treated as planning relationship
- separate lightweight relationship model if rules grow

Recommended V1 approach:

```text
Session owns assignment consistency.
Task owns planning lifecycle.
```

Assignment use cases:

- assign Planned or Active Task to Active Session
- remove assigned Task before Session is Stopped
- list Tasks assigned to Session
- list Sessions associated with Task

Validation:

- Task must exist
- Task must not be Completed or Cancelled
- Session must exist
- Session must be Active for V1 assignment
- assignment must not duplicate the same Task in the same Session
- stopped Sessions should not accept new Task assignments

Events:

- `TaskAssignedToSessionDomainEvent`
- `TaskRemovedFromSessionDomainEvent`
- `TaskAssignedToSessionIntegrationEvent`

### Expected Outcome

Session can show declared intent without Task owning execution facts.

### Risks Or Anti-Patterns To Avoid

- making Task aggregate load Session aggregate directly
- making Session stop complete Tasks
- duplicating Task state inside Session
- allowing assignment rules to become a workflow engine

---

## Phase 7 - Timeline / Replay Preparation

### Objective

Make Task events usable by future Timeline and Replay read models.

### Why This Phase Exists

Replay is downstream and derived.

Task events should provide enough facts for replay later without implementing replay now.

### Implementation Details

Publish integration events for:

- Task drafted
- Task planned
- Task assigned to Session
- Task completed
- Task cancelled

Event payloads should include:

- event id
- occurred at
- task id
- owner id
- task title snapshot when useful
- session id when relevant
- status transition when relevant

Keep payloads stable and business-oriented.

Do not expose internal EF Core or database concepts.

### Expected Outcome

Future Replay and Timeline projections can consume Task facts without changing Task domain behavior.

### Risks Or Anti-Patterns To Avoid

- building Replay in this phase
- making Replay a source of truth
- publishing overly large event payloads
- leaking private implementation details into integration contracts

---

## Phase 8 - Hardening

### Objective

Improve reliability, safety, and operability after the core workflow exists.

### Why This Phase Exists

Hardening should happen after the domain flow is proven, not before every possible edge case is imagined.

### Implementation Details

Add:

- authorization checks by owner
- optimistic concurrency for Task updates
- idempotency for commands that may be retried
- audit fields
- structured logging around command handlers
- consistent error mapping
- integration event outbox coverage
- test coverage for important lifecycle rules

Recommended tests:

- Draft can become Planned
- Planned can become Active
- Active can return to Planned
- Planned or Active can complete
- Draft, Planned, or Active can cancel
- Completed cannot be changed
- Cancelled cannot be changed
- Task completion does not stop Session
- Session stop does not complete Task
- stopped Session rejects new assignment

### Expected Outcome

The Task module is stable enough to build Evidence and Replay work on top of it.

### Risks Or Anti-Patterns To Avoid

- adding generic audit frameworks too early
- adding distributed idempotency patterns before retries exist
- over-instrumenting every method
- adding permissions beyond owner-based access

---

# 5. Suggested Folder Structure

Use the existing modular monolith style.

Recommended structure:

```text
src/
  Modules/
    TaskPlanning/
      Domain/
        Tasks/
          Task.cs
          TaskId.cs
          TaskStatus.cs
          TaskCategory.cs
          TaskPriority.cs
          TaskDomainErrors.cs
          Events/
            TaskDraftedDomainEvent.cs
            TaskPlannedDomainEvent.cs
            TaskActivatedDomainEvent.cs
            TaskDeferredDomainEvent.cs
            TaskCompletedDomainEvent.cs
            TaskCancelledDomainEvent.cs
      Application/
        Abstractions/
          ITaskRepository.cs
          ITaskReadRepository.cs
        Tasks/
          Commands/
            DraftTask/
              DraftTaskCommand.cs
              DraftTaskCommandHandler.cs
              DraftTaskRequestValidator.cs
            PlanTask/
              PlanTaskCommand.cs
              PlanTaskCommandHandler.cs
            CompleteTask/
              CompleteTaskCommand.cs
              CompleteTaskCommandHandler.cs
            CancelTask/
              CancelTaskCommand.cs
              CancelTaskCommandHandler.cs
          Queries/
            GetTaskById/
              GetTaskByIdQuery.cs
              TaskDetailReadModel.cs
              GetTaskByIdQueryHandler.cs
            ListTasks/
              ListTasksQuery.cs
              TaskSummaryReadModel.cs
              ListTasksQueryHandler.cs
      Infrastructure/
        Persistence/
          TaskPlanningDbContext.cs
          Configurations/
            TaskConfiguration.cs
          Repositories/
            TaskRepository.cs
            TaskReadRepository.cs
          Migrations/
        Integrations/
          TaskIntegrationEventMapper.cs
      Presentation/
        TasksController.cs
        Contracts/
          DraftTaskRequest.cs
          RefineTaskRequest.cs
          CompleteTaskRequest.cs
          CancelTaskRequest.cs
          TaskResponse.cs
          TaskSummaryResponse.cs
      Integrations/
        Events/
          TaskPlannedIntegrationEvent.cs
          TaskCompletedIntegrationEvent.cs
          TaskCancelledIntegrationEvent.cs
          TaskAssignedToSessionIntegrationEvent.cs
```

If the existing solution uses a different folder convention, follow it.

The important rule is that each layer has a clear responsibility.

---

# 6. Aggregate Design Recommendations

## Inside Task Aggregate

The Task aggregate should contain:

- TaskId
- owner reference
- title
- description
- status
- category
- priority
- planning timestamps
- completion note
- cancellation reason
- domain events
- lifecycle behavior methods

## Outside Task Aggregate

Keep these outside:

- Session lifecycle checks
- Session start and stop behavior
- Evidence records
- Activity records
- Replay read models
- Reflection summaries
- authorization infrastructure
- persistence mapping details

## Consistency Boundary

Task aggregate guarantees Task planning consistency.

Examples:

- invalid state transitions are rejected
- terminal Tasks cannot be changed
- completion timestamp is set once
- cancellation timestamp is set once

Session assignment requires coordination across Task and Session.

That coordination belongs in the application layer or a domain service because it may require loading both aggregates.

## Transaction Boundary

Use one transaction per command.

Examples:

- `CompleteTaskCommand` updates one Task aggregate and commits.
- `AssignTaskToSessionCommand` validates Task and Session, creates assignment, emits event, and commits.

Do not create long-running transactions across unrelated modules.

---

# 7. Recommended Domain Events

## Core Lifecycle Events

- `TaskDraftedDomainEvent`
- `TaskPlannedDomainEvent`
- `TaskActivatedDomainEvent`
- `TaskDeferredDomainEvent`
- `TaskCompletedDomainEvent`
- `TaskCancelledDomainEvent`

Emit these from the Task aggregate when lifecycle behavior succeeds.

## Planning Events

- `TaskRefinedDomainEvent`
- `TaskCategoryChangedDomainEvent`
- `TaskPriorityChangedDomainEvent`

Emit these when planning facts change.

## Relationship Events

- `TaskAssignedToSessionDomainEvent`
- `TaskRemovedFromSessionDomainEvent`

Emit these when the Task-to-Session relationship changes.

Depending on ownership, these may be emitted by Session or an assignment aggregate instead of Task.

## Integration Events

Publish integration events for cross-module reactions:

- `TaskPlannedIntegrationEvent`
- `TaskCompletedIntegrationEvent`
- `TaskCancelledIntegrationEvent`
- `TaskAssignedToSessionIntegrationEvent`
- `TaskRemovedFromSessionIntegrationEvent`

Consumers may include:

- Session
- Replay / Timeline
- Reflection / Learning Insight
- Analytics
- future AI pipelines

## Future Replay-Related Events

Future replay projections may consume:

- Task assigned to Session
- Task removed from Session
- Task completed
- Task cancelled
- Task priority changed

Replay must remain derived and read-only.

---

# 8. Persistence Recommendations

## EF Core

Use EF Core for:

- Task aggregate persistence
- transactional writes
- change tracking
- domain event capture
- outbox integration
- migrations

EF Core should persist the write model.

It should not be used to build complex read screens if Dapper is already the read strategy.

## Dapper

Use Dapper for:

- Task list queries
- Task detail queries
- task assignment read models
- filtering by status, category, priority
- future read models for planning views

Dapper read models should be explicit and query-focused.

## Schema Direction

Suggested tables:

- `tasks`
- `session_task_assignments`

Keep V1 schema boring.

Do not add recurrence, templates, dependency graphs, or scheduling tables yet.

## Indexing Considerations

Recommended indexes:

- owner id
- status
- category
- priority
- created at
- owner id + status
- session id for assignments
- task id for assignments
- unique session id + task id assignment constraint

Add text search only after basic filtering is used.

## Audit Fields

Recommended fields:

- CreatedAt
- CreatedBy
- UpdatedAt
- UpdatedBy
- CompletedAt
- CancelledAt
- Version or RowVersion

Do not overbuild audit history in V1.

If full history becomes necessary, build it later from domain events or an explicit audit log.

---

# 9. Testing Recommendations

## Domain Tests

Start with domain tests.

They are the cheapest way to validate lifecycle rules.

Cover:

- valid state transitions
- invalid state transitions
- terminal state protection
- completion timestamp rules
- cancellation timestamp rules
- category and priority changes

## Application Tests

Add application tests for:

- command handlers
- repository interaction
- transaction boundaries
- validation behavior
- ownership checks

## Integration Tests

Add integration tests after persistence exists.

Cover:

- migrations apply cleanly
- Task can be saved and loaded
- list query returns expected read model
- assignment uniqueness is enforced
- outbox events are persisted when expected

Keep early tests focused.

Do not build a large test framework before the module behavior stabilizes.

---

# 10. Anti-Patterns To Avoid

## 1. Anemic Domain Model

Avoid storing Task state with public setters and pushing all rules into handlers.

Task lifecycle behavior belongs in the Task aggregate.

---

## 2. Task Becoming Execution Tracking

Task records intention.

Session records execution.

Do not add start time, stop time, duration, or evidence capture rules to Task as execution facts.

---

## 3. Leaking Session Rules Into Task

Task should not know how Session starts or stops.

Use application coordination for rules involving both Task and Session.

---

## 4. CRUD-Oriented Design

Avoid generic operations like:

```text
UpdateTask
PatchTaskStatus
DeleteTask
```

Prefer business operations:

```text
PlanTask
CompleteTask
CancelTask
AssignTaskToSession
```

---

## 5. Premature Generic Abstractions

Avoid generic lifecycle engines, generic status transition frameworks, and generic repository layers unless the existing codebase already requires them.

Task rules are small enough to model directly.

---

## 6. Infrastructure Leaking Into Domain

Do not place EF Core attributes, Dapper concerns, API DTOs, or message bus contracts inside the domain model.

---

## 7. Building Future Features Early

Do not implement:

- recurring tasks
- templates
- scheduling
- reminders
- collaboration
- task dependencies
- planning boards

until the basic Task lifecycle is stable and useful.

---

# 11. Suggested Implementation Order

Implement in this order:

1. Add Task lifecycle domain tests.
2. Implement `TaskId`, `TaskStatus`, and basic Task aggregate.
3. Add Task lifecycle behavior methods.
4. Add Task domain events.
5. Add EF Core configuration for Task.
6. Add initial migration for `tasks`.
7. Add Task repository contract and implementation.
8. Add `DraftTaskCommand` and handler.
9. Add `PlanTaskCommand` and handler.
10. Add `CompleteTaskCommand` and handler.
11. Add `CancelTaskCommand` and handler.
12. Add Dapper `GetTaskByIdQuery`.
13. Add Dapper `ListTasksQuery`.
14. Add Task API endpoints for core commands and reads.
15. Add validation and error mapping.
16. Add owner-based authorization.
17. Add `session_task_assignments` persistence.
18. Add `AssignTaskToSessionCommand`.
19. Add assignment read queries.
20. Add integration events and outbox mapping.
21. Add focused integration tests.
22. Document final Task module behavior and known exclusions.

This order provides early validation and keeps refactor cost low.

The first useful milestone is:

```text
Draft Task -> Plan Task -> Complete Task -> List Tasks
```

The second useful milestone is:

```text
Assign Planned Task to Active Session
```

---

# 12. Future Extensions

Future extensions should be added only after V1 Task Planning is stable.

## Task Templates

Reusable task templates can support repeated planning.

Rules to preserve:

- Template is not a Task.
- Creating from template creates a new Task.
- Template changes do not rewrite historical Tasks.

## Recurring Tasks

Recurring tasks can create planned Task instances from a schedule.

Keep recurrence separate from Task lifecycle.

## AI-Assisted Planning

AI may suggest Tasks, categories, or priorities.

AI suggestions must not become source of truth until the user accepts them.

## Analytics

Future analytics may compare:

- planned Tasks vs completed Tasks
- cancelled Task patterns
- Tasks assigned across Sessions
- category-level focus patterns

Analytics must remain downstream.

## Prioritization Engines

Future prioritization can recommend planning order.

Priority recommendations should not override explicit user intent.

## Calendar Integrations

Calendar integrations may schedule planning reminders or future Sessions.

They should not redefine Task lifecycle.

## Collaboration

Collaboration is out of scope for the solo-user V1 model.

If introduced later, ownership and permissions should be revisited explicitly.

---

# Final Recommendation

Implement Task Planning as a small, intention-focused module first.

Keep it boring and durable:

- domain rules in the aggregate
- EF Core for writes
- Dapper for reads
- commands for business behavior
- events for downstream consumers
- Session integration through explicit assignment

Do not let Task become execution tracking.

Session remains the source of truth for what actually happened.

