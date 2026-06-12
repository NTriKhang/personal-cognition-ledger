# Task Assignment Integration Test Notes

## Purpose

This note records future test coverage for the Task-to-Session assignment integration flow.

No tests are required immediately. Add these when the project has a test structure or when this flow becomes risky to change.

## Behavior To Preserve

Assigning a Task to a Session is owned by the Session module.

After assignment succeeds, Session publishes `TaskAssignedToSessionIntegrationEvent`.

TaskPlanning consumes that event and records that the Task is now in progress by moving a Planned Task to Active.

Ending a Session does not automatically complete, cancel, or defer assigned Tasks.

## Suggested Unit Tests

- Planned Task receives assignment event command -> Task becomes Active.
- Already Active Task receives assignment event command -> command succeeds as a no-op.
- Completed Task receives assignment event command -> Task remains Completed.
- Cancelled Task receives assignment event command -> Task remains Cancelled.
- Assignment command with mismatched owner -> failure.
- Assignment command with missing Task -> failure.

Primary target:

```text
RecordTaskAssignedToSessionCommandHandler
```

## Suggested Integration Tests

- Session assigns a Task -> Session outbox stores assignment domain event.
- Session outbox processing publishes `TaskAssignedToSessionIntegrationEvent`.
- TaskPlanning inbox stores the integration event.
- TaskPlanning inbox processing invokes the Presentation integration event handler.
- TaskPlanning Application updates the Task status from Planned to Active.

## Serialization Coverage

Add a regression test for DateTimeOffset preservation across outbox/inbox JSON serialization.

Example case:

```text
AssignedAt = 2026-06-12T10:00:00+07:00
```

Expected behavior:

- serialization should preserve the DateTimeOffset value correctly;
- deserialization should produce a DateTimeOffset, not an unintended DateTime conversion;
- processing should not fail because of timestamp offset handling.

## Architectural Boundary To Preserve

TaskPlanning Application should not reference Session integration event contracts.

The expected dependency shape is:

```text
TaskPlanning.Presentation -> Session.Contracts
TaskPlanning.Presentation -> TaskPlanning.Application
TaskPlanning.Application -> TaskPlanning.Domain
```

The Presentation integration event handler should remain an adapter that translates external integration events into TaskPlanning Application commands.
