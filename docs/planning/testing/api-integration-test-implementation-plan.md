# API Integration Test Implementation Plan

## Summary

Create a complete HTTP integration test suite for all 27 endpoints.

Use:

- xUnit
- `WebApplicationFactory<Program>`
- Testcontainers PostgreSQL
- Respawn database cleanup
- Real temporary-directory Local storage
- Fake Amazon S3 verifier
- Deterministic outbox/inbox processing
- GitHub Actions running `dotnet test`

Browser automation remains out of scope.

## Architecture and Environment

- Add one API-level integration test project to `PCL_API.sln`.
- Reference the API host and use its actual endpoint, MediatR, validation, EF Core, Dapper, serialization, and error pipelines.
- Expose `Program` to `WebApplicationFactory` using `public partial class Program`.
- Start one PostgreSQL container per test collection and apply all module migrations once.
- Reset `evidence`, `session`, `task_planning`, inbox, and outbox tables with Respawn before each test.
- Run tests sequentially initially because the application uses shared database state and background processors.
- Override development configuration with the container connection string, test storage roots, and deterministic job settings.
- Disable normal scheduled outbox/inbox execution in tests and provide a test fixture method that explicitly processes pending messages, followed by bounded polling for the expected state.

## Test Data and External Dependencies

- Generate unique owner, task, session, evidence, and profile values through test-data builders.
- Create prerequisites through public HTTP endpoints whenever they are part of the tested flow.
- Seed directly through module DbContexts only for states impossible or unnecessarily expensive to create through public APIs.
- Use a fresh temporary directory under the test runtime directory for Local storage; delete it during fixture disposal.
- Verify real Local behavior: allowed roots, directory creation, write access, and rejected paths.
- Replace only the Amazon S3 profile verifier with a configurable fake supporting success and unavailable-provider outcomes.
- Do not introduce test-only HTTP endpoints.
- Keep reusable response deserialization, ProblemDetails assertions, and endpoint request builders in shared test infrastructure.

## Coverage and Execution Order

### Milestone 1 — Test foundation

- Create the integration test project and shared API factory.
- Add PostgreSQL Testcontainers, migrations, Respawn reset, HTTP client creation, and fixture lifecycle.
- Add ProblemDetails assertion helpers and builders for owners, tasks, sessions, evidence, and storage profiles.
- Prove the foundation with API availability and database-isolation smoke tests.

### Milestone 2 — Task Planning: 12 endpoints

Cover:

- Draft, get, list, filtered list, and assignable list.
- Refine, categorize, and prioritize.
- Plan, activate, defer, complete, and cancel.
- Lifecycle conflicts, invalid enums, length validation, invalid timestamps, missing resources, and owner mismatch.
- Active-task cancellation requiring a reason.
- Terminal tasks rejecting further mutation.

### Milestone 3 — Session: 6 endpoints

Cover:

- Start, list, get, assign task, remove task, and end.
- One Active Session per owner.
- Missing Session and invalid end time.
- Planned/Active task eligibility.
- Task/session owner mismatch.
- Duplicate assignment and assignment to another Active Session.
- Idempotent removal.
- Mutations rejected after Session stop.

### Milestone 4 — Evidence items: 3 endpoints

Cover:

- Add Note and Link evidence.
- List with and without removed items.
- Soft removal and idempotent removal.
- Session existence, ownership, and Active-state eligibility.
- Invalid content, invalid links, unsupported enum values, and oversized content.
- Generic `FileReference` creation returning the expected conflict because upload endpoints do not yet exist.
- Wrong Session route ID, wrong owner, invalid removal time, and oversized removal reason.

### Milestone 5 — Evidence storage: 6 endpoints

Cover:

- Create Local and Amazon S3 profiles.
- List and test profiles.
- Select and retrieve active settings.
- Duplicate names, missing profiles, invalid fields, rejected Local roots, and unavailable directories.
- Real Local directory verification.
- Configurable S3 success and failure.
- Successful selection and verification failure preventing selection.

### Milestone 6 — Cross-module flows

- Draft and plan a task, start a Session, assign the task, explicitly process outbox/inbox messages, and verify eventual Task activation.
- Remove the assignment and verify it does not automatically defer the Task.
- Add Evidence during an Active Session and reject new Evidence after stopping it.
- Complete a Task and stop its Session independently.
- Verify all flow outcomes through HTTP reads rather than database assertions where public reads exist.

## Authorization and Error Coverage

- Record that production authentication and authorization are currently disabled.
- Do not add fake authentication or assert `401`/`403`, because that would test a pipeline production does not run.
- Treat current owner checks as business-ownership coverage:
  - Task operations hide owner mismatch as `404`.
  - Session/task assignment rejects owner mismatch.
  - Evidence attachment/removal enforces matching owners.
- Add real authentication tests later when production enables authentication and endpoint authorization.
- Assert status, ProblemDetails code/title, detail, and validation extension where relevant.

## CI/CD Integration

- Add a GitHub Actions workflow triggered by pull requests and pushes to the main branch.
- Install the repository’s required .NET 9 SDK.
- Restore, build in Release mode, and run integration tests with `--no-build`.
- Rely on Testcontainers through the GitHub-hosted runner’s Docker support; do not configure a separate PostgreSQL service.
- Upload TRX test results when the test step fails.
- Set a suite timeout and ensure container/filesystem disposal runs on failure.
- Make the integration test job required before merge once it is stable.
- Do not add code coverage gating initially; collect coverage later after behavioral coverage is established.

## Acceptance Criteria

- All 27 mapped endpoints have at least one successful HTTP-level test.
- Every checklist item in `docs/api/manual-regression-checklist.md` is linked to an automated test or explicitly marked deferred.
- Tests run locally with one `dotnet test` command and require only Docker.
- Tests pass repeatedly without relying on execution order or persistent developer data.
- PostgreSQL behavior, migrations, serialization, validation, and ProblemDetails are exercised through the real host.
- No test contacts Amazon S3 or writes outside its temporary Local storage root.
- Cross-module assignment completes deterministically without a 30-second scheduler wait.
- CI runs the same suite and reports actionable failures.

## Assumptions

- Initial coverage includes all current modules, not only Evidence.
- Respawn resets data before each test while preserving migrated schemas.
- Tests are initially non-parallel.
- Local storage uses real filesystem operations; S3 verification is replaced.
- File-upload lifecycle endpoints remain deferred because they are not currently exposed.
- HTTP authentication testing remains deferred until authorization is enabled in the production host.
