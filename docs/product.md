# Product and scope

## Purpose

Personal Cognition Ledger (PCL) records the relationship between what a person intended to do, the bounded period in which they worked, and the evidence produced during that work.

Traditional task trackers primarily record intention and completion. PCL is intended to preserve a richer account:

```text
intention -> execution -> evidence -> later reflection
```

The product is backend-first. It favors trustworthy domain data and explicit boundaries over early dashboards, analytics, or AI-generated interpretation.

## Core idea

PCL separates concepts that are often collapsed:

- A **Task** records intended work.
- A **Session** records a bounded period of execution.
- **Evidence** records proof or a meaningful trace of what happened.
- A future **Reflection** may record what the user concluded.
- A future **Replay** may present an ordered, read-only account derived from recorded facts.

Completing a Task and stopping a Session are independent actions. Evidence can show that work occurred without proving that the intended outcome was completed.

## Intended users

The initial product is designed for an individual who wants to retain trustworthy records of focused work or learning. Ownership remains explicit even while authentication and multi-user product behavior are incomplete.

Potential examples include:

- implementing or debugging software
- reading and researching
- practicing a skill
- reviewing material
- recording notes and links produced during a focused period

## Current scope

The implemented backend currently supports:

- drafting, organizing, planning, activating, deferring, completing, cancelling, listing, and retrieving Tasks
- starting, ending, listing, and retrieving Sessions
- assigning eligible Tasks to an active Session and removing assignments
- recording Note and Link Evidence against an active Session
- listing and soft-removing Evidence
- configuring and verifying Local or Amazon S3 Evidence storage profiles
- reliable asynchronous Task activation after Session assignment through outbox/inbox processing
- HTTP integration coverage for all 32 mapped endpoints and principal cross-module flows

The current API does **not** enforce authentication or authorization. Owner IDs in requests are business ownership inputs, not a secure identity boundary.

## Explicitly not implemented

- Evidence file-upload and download endpoints
- frontend or administration UI
- Session pause and resume
- granular Activity tracking inside a Session
- Replay or Timeline projections
- Reflection and Annotation modules
- analytics, productivity scoring, or gamification
- AI-generated summaries or insights
- recurring Tasks, templates, calendar integrations, or collaboration

These are possibilities, not promises. The [Roadmap](roadmap.md) distinguishes approved next work from deferred ideas.

## Design principles

### Capture reality, not ideal behavior

The ledger should record what happened without forcing every Session to end in a completed Task or every piece of work to fit a rigid workflow.

### Evidence over inference

Core transactional modules record facts. Future replay, reflection, analytics, and AI features must remain downstream and must not rewrite transactional truth.

### Structured but flexible

Domain boundaries and invariants should be explicit, while Evidence content and later interpretation remain extensible.

### Slow, testable evolution

Add a capability when its domain responsibility and observable behavior are clear. Prefer a small end-to-end increment that can be tested over speculative infrastructure or detailed models for unimplemented features.

### Backend truth before presentation

The backend should establish reliable lifecycles, ownership, persistence, and integration behavior before a feature-rich user interface is built.

## V1 success condition

The first coherent product increment is complete when a user can:

1. declare intended work as a Task
2. start a Session and associate eligible Tasks
3. capture meaningful Evidence during that Session
4. stop the Session independently of Task completion
5. retrieve the resulting records reliably

Storage profile administration is implemented in preparation for file Evidence. The file lifecycle itself remains the principal gap in that initial experience.
