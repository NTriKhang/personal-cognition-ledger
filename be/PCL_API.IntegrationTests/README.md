# PCL API Integration Tests

This project hosts HTTP-level integration tests for the complete API.

## Prerequisites

- .NET 9 SDK
- Docker Desktop or another Docker-compatible engine

No manually configured PostgreSQL database is required. Testcontainers starts a disposable PostgreSQL container and removes it when the test run finishes.

## Run

From `be/PCL_API`:

```powershell
dotnet test ..\PCL_API.IntegrationTests\PCL_API.IntegrationTests.csproj
```

Or run the complete solution:

```powershell
dotnet test PCL_API.sln
```

If the test fixture reports `DockerUnavailableException`, start Docker Desktop and run the command again.

## Test structure

- `Infrastructure/IntegrationTestFixture.cs` owns PostgreSQL, migrations, database cleanup, and the HTTP client.
- `Infrastructure/PclApiFactory.cs` starts the real API in the `Testing` environment and overrides external configuration.
- `Infrastructure/IntegrationTestBase.cs` resets all module data before each test.
- `Infrastructure/ProblemDetailsAssertions.cs` provides shared API error assertions.
- `TestData/TestDataBuilder.cs` provides reusable request data.
- `Foundation/ApiFoundationTests.cs` proves that the API starts against migrated PostgreSQL and that cleanup works.
- `TaskPlanning/TaskPlanningQueryTests.cs` covers draft, get, list, filters, and assignable tasks.
- `TaskPlanning/TaskPlanningOrganizationTests.cs` covers refine, categorize, prioritize, and their validation rules.
- `TaskPlanning/TaskPlanningLifecycleTests.cs` covers plan, activate, defer, complete, cancel, and lifecycle conflicts.

注意: The shared database is reset before every test. Tests must still create their own prerequisites and must not depend on execution order.

Tests are intentionally non-parallel while they share one database container.
