# Roadmap

Last reviewed: 2026-07-12

This roadmap states current project status and priority. It is not a promise of release dates. Detailed design belongs in an active plan only after the next increment is approved.

## Current baseline

Implemented and covered by HTTP integration tests:

- modular-monolith foundation with PostgreSQL, CQRS, migrations, outbox/inbox processing, and `ProblemDetails`
- complete current Task Planning HTTP surface and lifecycle
- Session lifecycle and Task assignment eligibility
- asynchronous activation of assigned Planned Tasks
- Note and Link Evidence, listing, and soft removal
- Local and Amazon S3 storage-profile creation, verification, selection, and settings
- all 34 mapped endpoints and principal cross-module flows

Known baseline limitations:

- authentication and authorization are disabled
- some created-response `Location` headers incorrectly include `/api`
- Session reads are not owner-filtered
- no frontend exists

## Current priority

### Production identity and authorization

Status: required before production exposure

The FileReference workflow is implemented: idempotent reservation, Local API streaming or S3 direct upload, provider verification, status, download, renewal/cancellation, interrupted-upload reconciliation, and seven-day retention-based physical cleanup.

Authentication-derived ownership and administrative authorization must be added before file and storage-administration endpoints are considered production-ready. Until then, caller-supplied owner IDs are domain inputs rather than a secure identity boundary.

## Near-term hardening

These are concrete known gaps, but are not separate detailed plans yet:

1. Enable authentication and derive ownership from claims rather than public owner inputs.
2. Protect `/admin/evidence-storage` with an administrative policy.
3. Correct created-response `Location` headers to match mapped routes.
4. Add owner-aware Session queries.
5. Add CI checks for build, integration tests, and documentation links.
6. Define the project license and security-reporting policy before presenting the repository as fully open source.

## Candidate product increments

These are ordered directions, not approved implementation specifications.

### Timeline and Replay

Build a read-only chronological projection from Session, Task-assignment, and Evidence facts. It must remain downstream and must not own business rules or become a second transactional source of truth.

### Reflection

Allow a user to record a summary, learned material, difficulties, and next steps associated with a completed or stopped Session. Exact timing and edit rules require product validation.

### Lightweight Annotation

Allow notes or highlights on Evidence without building a collaborative document editor.

### Minimal user interface

Provide only the flows needed to plan a Task, start/stop a Session, attach Evidence, and inspect the resulting record. Avoid dashboard-heavy work before the file and security foundations are reliable.

## Deferred exploration

The following ideas are intentionally not designed in detail:

- granular Activity tracking within a Session
- Session pause/resume or continuation relationships
- recurring Tasks and Task templates
- OCR, preview generation, and indexing
- provider-managed lifecycle rules for multipart remnants or storage versions
- analytics, scoring, and behavioral metrics
- AI summaries, friction detection, or learning insights
- calendar integrations and collaboration

When one of these becomes a current priority, validate the user need first. Do not revive old speculative state machines as though they were accepted domain behavior.

## Roadmap maintenance

- Keep only one current priority unless work is truly proceeding in parallel.
- Add a detailed plan only for approved, near-term work with unresolved implementation decisions.
- Move durable completed behavior into Product, Architecture, Domain model, Development, or API reference.
- Remove completed plans from the maintained tree after migration; use Git history for archaeology.
- Update the baseline and known gaps in the same pull request that changes them.
