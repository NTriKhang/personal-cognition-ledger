# Development guide

## Prerequisites

- .NET 9 SDK
- PostgreSQL for running the API locally
- Docker Desktop or another Docker-compatible engine for integration tests
- An HTTP client with `.http` support for executable API examples

Amazon S3 credentials are needed only when manually verifying a real S3 storage profile. Use the AWS SDK credential chain; never add access keys to repository configuration.

## Repository layout

```text
be/
  Common/
    Common.Domain
    Common.Application
    Common.Infrastructure
    Common.Presentation
  Modules/
    TaskPlanning/
    Session/
    Evidence/
  PCL_API/
  PCL_API.IntegrationTests/
docs/
  api/requests/
  plans/
fe/
```

Each business module normally contains Domain, Application, Infrastructure, Presentation, and Contracts projects. Read [Module boundaries](module-boundaries.md) before creating a dependency between modules.

## Build

Run commands from `be/PCL_API`:

```powershell
dotnet restore PCL_API.sln
dotnet build PCL_API.sln
```

The solution targets .NET 9.

## Run the API

The checked-in Development connection expects:

```text
Host=localhost;Port=5432;Database=PCL;Username=pcl;Password=pcl
```

Override it without editing tracked configuration:

```powershell
$env:ConnectionStrings__Database = "Host=localhost;Port=5432;Database=PCL;Username=pcl;Password=your-password"
```

Start the HTTP launch profile:

```powershell
dotnet run --project PCL_API.csproj --launch-profile http
```

In Development, the host applies module migrations during startup and exposes Swagger at `/swagger`. The default HTTP base URL is `http://localhost:5266`; the default HTTPS URL is `https://localhost:7082`.

Local Evidence storage is restricted by `EvidenceStorage:Local:AllowedRootDirectories`. Do not configure or test paths outside an intentional developer-owned root.

## Test

Run the complete solution:

```powershell
dotnet test PCL_API.sln
```

The current automated suite is the HTTP integration project at `be/PCL_API.IntegrationTests`. It uses:

- an in-process ASP.NET Core test host
- disposable PostgreSQL through Testcontainers
- real EF Core migrations
- Respawn database reset before every test
- in-memory messaging with explicit outbox/inbox processing
- controlled fakes for nondeterministic external providers

Docker must be running. See the [integration-test README](../be/PCL_API.IntegrationTests/README.md) for architecture and test-authoring conventions.

### Adding an integration test

1. Put the test under the capability folder: `TaskPlanning`, `Session`, `Evidence`, or `Flows`.
2. Derive from `IntegrationTestBase`.
3. Arrange independent data, preferring public HTTP endpoints for prerequisites.
4. Act through the shared `HttpClient`.
5. Assert status plus meaningful response state or externally visible behavior.
6. Use `ProblemDetailsAssertions` for error contracts.
7. Put reusable valid request data in `TestDataBuilder`; keep deliberately invalid values in the test that explains them.
8. Pass `TestContext.Current.CancellationToken` to asynchronous test operations.

Do not manually clean the shared database, rely on test order, contact real Amazon S3, or write to persistent developer directories. Flow tests should explicitly process only the module outbox/inbox routes relevant to the scenario.

### Manual API verification

The executable requests under [`api/requests`](api/requests/) are the maintained manual surface. Start the API, select a fresh owner UUID, and run the relevant request file from top to bottom.

Manual checks are useful for:

- Swagger startup and request ergonomics
- real Local filesystem permissions
- real AWS identity and bucket access
- observing scheduled eventual consistency at the configured interval

Behavior already covered by integration tests should not be duplicated in a permanent checked checklist.

## Persistence and migrations

- Each module `DbContext` is the central registration point for its schema.
- Put entity/table mappings in dedicated `IEntityTypeConfiguration<T>` classes.
- Register shared Outbox/Inbox mappings from the module context.
- Keep migrations in the owning module Infrastructure project.
- Do not hand-edit EF model snapshots except when intentionally resolving migration metadata.
- Never query or mutate another module's schema from a module migration or repository.

When changing persistence, test both migration startup and the affected HTTP behavior against PostgreSQL. Do not rely only on an in-memory provider.

## Application and domain conventions

- Put invariants and state transitions in aggregate behavior, not handlers or endpoints.
- Put use-case orchestration in Application handlers.
- Keep endpoint mapping, serialization, and HTTP status decisions in Presentation.
- Use EF Core for transactional aggregate writes and Dapper for read projections.
- Return explicit domain/application errors and map them consistently to `ProblemDetails`.
- Treat owner IDs as current business inputs, not proof of authentication.
- Keep provider SDK types in Infrastructure.

## Cross-module changes

For a new dependency:

1. Identify which module owns the fact.
2. Decide whether the consumer truly needs an immediate answer.
3. Use a provider-owned application contract for immediate hard validation.
4. Use an integration event for eventual reaction.
5. Add cross-module integration coverage.
6. Update the architecture or domain reference if the relationship changes.

Do not use direct table access, shared aggregates, or host-level orchestration as shortcuts.

## API changes

For every added or changed endpoint, update in the same change:

1. `api/reference.md`
2. the matching executable `.http` file
3. affected cross-module flow documentation
4. integration tests and endpoint counts
5. authentication/security notes if the exposure changes

Actual routes currently use `/tasks`, `/lsessions`, and `/admin/evidence-storage`; do not document a hypothetical `/api` prefix.

## Documentation maintenance

Use [the documentation index](README.md) to find the authoritative home for a fact. Prefer updating an existing section over creating a new file.

A separate document is justified when it has:

- a distinct reader task
- enough durable content to navigate independently
- a clear owner or update trigger
- minimal overlap with existing authority

Active implementation plans are temporary. Every plan must declare status, review date, scope, and completion condition. When work completes, migrate durable facts into reference documentation and remove the plan.

Before merging a documentation change:

- check relative links and referenced paths
- confirm current and future behavior are visibly distinguished
- compare API claims with mapped endpoints and integration tests
- remove machine-specific absolute paths
- render Mermaid diagrams where their relationships changed
- search for old filenames after moves
- update repository entry points if the reading path changed

Run the local link check from the repository root:

```powershell
./.github/scripts/Test-MarkdownLinks.ps1
```

The same check runs in GitHub Actions for Markdown changes.

Documentation-only changes do not require the application test suite unless they reveal or accompany a behavior change. Link and structure checks still apply.
