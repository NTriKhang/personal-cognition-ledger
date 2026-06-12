# Recommended Next Work

## Current Baseline

The current implementation has two stable core modules:

- Session: execution lifecycle and task assignment relationship.
- TaskPlanning: task intention, lifecycle, and assignment eligibility.

The current cross-module assignment flow is:

```text
Session assigns Task
-> Session stores SessionTaskAssignment
-> Session publishes TaskAssignedToSessionIntegrationEvent
-> TaskPlanning consumes the event
-> Planned Task becomes Active
```

In V1, an Active Task means the intention is in progress. It does not require a currently Active Session.

Stopping a Session must not automatically complete, cancel, or defer Tasks.

Removing a Task from a Session must not automatically change Task status.

---

## Already Identified

- `TaskRemovedFromSessionIntegrationEvent`
- `SessionStoppedIntegrationEvent`

---

## Recommended Next Work

### 1. Task Removed From Session Event Flow

Priority: High

Module: Session

Business value:

Completes the assignment lifecycle. Assignment already produces a business fact, so removal should also produce a business fact.

Dependencies:

- `RemoveTaskFromSessionCommand`
- `LSession.RemoveAssignedTask`
- Outbox processing
- Session integration event contracts

Estimated implementation complexity:

Medium

Why now:

The command and domain behavior already exist, but the event story is incomplete. Implementing this now keeps assignment and removal symmetrical.

Suggested scope:

```text
LSession.RemoveAssignedTask
-> TaskRemovedFromSessionDomainEvent
-> TaskRemovedFromSessionIntegrationEvent
-> publish through Session outbox
```

TaskPlanning does not need to change Task status when this event occurs.

---

### 2. Session Stopped Integration Event

Priority: High

Module: Session

Business value:

Makes Session completion visible to downstream modules such as Evidence, Replay, Reflection, Analytics, and future AI pipelines.

Dependencies:

- `LSessionStoppedDomainEvent`
- Outbox processing
- Session integration event contracts

Estimated implementation complexity:

Low to Medium

Why now:

Session stopped is a core lifecycle fact. It is repeatedly referenced in architecture and domain documentation as a downstream trigger.

Suggested scope:

```text
LSessionStoppedDomainEvent
-> SessionStoppedIntegrationEvent
-> publish through Session outbox
```

No TaskPlanning handler is needed in V1.

---

### 3. EvidenceItem Basic Module

Priority: High

Module: Evidence

Business value:

Introduces proof of what happened during a Session. This moves the system beyond planning and time tracking.

Dependencies:

- Stable Session lifecycle
- Active Session validation
- Session ownership and mutability rules

Estimated implementation complexity:

Medium to High

Why now:

V1 scope includes `EvidenceItem`, and the core product model is:

```text
Task = intent
Session = execution
Evidence = proof
Reflection = conclusion
```

Suggested first slice:

```text
EvidenceItem aggregate
Evidence type: Note, Link, FileReference
Attach EvidenceItem to Active Session
List EvidenceItems by SessionId
EvidenceItemAddedDomainEvent
EvidenceItemAddedIntegrationEvent
```

Do not implement file processing, OCR, annotations, replay UI, or AI enrichment yet.

---

### 4. Session Mutability Contract For Evidence

Priority: Medium

Module: Session contracts and Evidence

Business value:

Lets Evidence validate whether evidence can be attached to a Session without depending on Session domain or database internals.

Dependencies:

- Evidence module start
- Session application contract pattern

Estimated implementation complexity:

Medium

Why now:

Only implement this when Evidence starts. It preserves module boundaries and prevents Evidence from reading Session tables directly.

Possible contract names:

```text
ISessionEvidenceAttachmentEligibilityChecker
ISessionMutabilityChecker
```

Prefer the narrower name if the use case remains evidence-specific.

---

### 5. Document Final Session / Task Assignment Behavior

Priority: Medium

Module: Documentation

Business value:

Records the intentional V1 behavior so the same lifecycle questions do not need to be rediscovered later.

Dependencies:

- Task removal event decision
- Session stopped event decision

Estimated implementation complexity:

Low

Why now:

The important decision has already been made:

```text
Stopping a Session does not change Task status.
Removing a Task from a Session does not change Task status.
Active Task means the intention is in progress.
```

This should be documented after the event flow is implemented.

---

### 6. Focused Tests

Priority: Low for now, Medium before larger modules

Module: Future test project

Business value:

Protects the cross-module event flow from regressions.

Dependencies:

- Stable task assignment flow
- Task removal event flow
- Session stopped event flow

Estimated implementation complexity:

Medium

Why later:

Tests are valuable, but the current priority is still stabilizing the core domain and event facts. Add tests when the test project structure is ready.

Suggested future coverage:

- `RecordTaskAssignedToSessionCommandHandler`
- outbox/inbox DateTimeOffset serialization
- task assignment integration event processing
- task removal integration event publishing
- session stopped integration event publishing

---

## Suggested Implementation Order

```text
1. Implement TaskRemovedFromSessionDomainEvent
2. Implement TaskRemovedFromSessionIntegrationEvent
3. Add or verify RemoveTaskFromSession API endpoint
4. Implement SessionStoppedIntegrationEvent
5. Document V1 task/session lifecycle decisions
6. Start EvidenceItem module with the smallest useful slice
7. Add Session mutability or evidence attachment eligibility contract
8. Add EvidenceItemAddedIntegrationEvent
9. Consider Replay / Timeline read model only after Evidence exists
```

## Explicitly Skipped For Now

- Assignment read projections for UI.
- Completion-with-session-context feature.
- Replay UI.
- Annotation system.
- AI features.
- Analytics.
