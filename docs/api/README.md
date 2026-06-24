# API Manual Testing Guide

This directory is the code-aligned catalog for manually testing every HTTP endpoint in Personal Cognition Ledger.

## Current API surface

| Module | Purpose | Endpoints | Documentation | Requests |
| :--- | :--- | ---: | :--- | :--- |
| Task Planning | Capture and manage planned intent | 12 | [task-planning.md](modules/task-planning.md) | [task-planning.http](requests/task-planning.http) |
| Session | Record bounded execution and task assignment | 6 | [session.md](modules/session.md) | [session.http](requests/session.http) |
| Evidence | Attach proof and configure evidence storage | 9 | [evidence.md](modules/evidence.md) | [evidence.http](requests/evidence.http) |

Total: **27 endpoints**.

Cross-module scenarios are documented in [system-flows.md](flows/system-flows.md). The repeatable test order is in [manual-regression-checklist.md](manual-regression-checklist.md).

## Runtime conventions

- Development HTTP base URL: `http://localhost:5266`
- Development HTTPS base URL: `https://localhost:7082`
- Routes are currently mounted at the root. Use `/tasks` and `/lsessions`, not `/api/tasks` or `/api/lsessions`.
- JSON enum values can be sent as their numeric values. The request files use numbers because no string-enum JSON converter is configured.
- Optional timestamps are usually replaced with `DateTimeOffset.UtcNow` when omitted. Session start and end timestamps are required.
- Failures use RFC 7807-style `ProblemDetails`.

Typical failure body:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Task.CannotActivate",
  "status": 409,
  "detail": "Only planned tasks can be activated."
}
```

Status mapping:

| Domain error type | HTTP status |
| :--- | ---: |
| Validation / Problem | 400 |
| Not found | 404 |
| Conflict | 409 |
| Unexpected failure | 500 |

## Authentication status

The current host does not enforce authentication or authorization. `UseAuthorization()` is commented out, and endpoints do not call `RequireAuthorization()`. Owner identity is supplied through request bodies or query parameters.

This is current behavior, not a security recommendation. Update this documentation and the request files when authentication becomes active.

## How to run the requests

1. Start PostgreSQL using the connection configured in `be/PCL_API/appsettings.Development.json`.
2. Start the API:

   ```powershell
   dotnet run --project be/PCL_API/PCL_API.csproj --launch-profile http
   ```

3. Open a file under `docs/api/requests` in an HTTP client that supports `.http` files.
4. Run requests from top to bottom. Copy IDs returned by create requests into the variables at the top of the files.
5. Follow [manual-regression-checklist.md](manual-regression-checklist.md) for a complete pass.

## Keeping this current

For every added or changed endpoint, the same change should update:

1. The module endpoint table and detailed contract.
2. Any affected cross-module flow.
3. The module `.http` request file.
4. The regression checklist if a new business capability or failure path was introduced.
5. The endpoint count in this index.

Future modules should receive `docs/api/modules/<module>.md` and `docs/api/requests/<module>.http`. No separate template directory is required; existing module documents are the working examples.

## Known contract mismatches

- Created responses currently set `Location` headers under `/api/...`, while actual routes are mounted without the `/api` prefix.
- The old `be/PCL_API/PCL_API.http` weather request did not correspond to a mapped endpoint; it now points readers to this collection.
- Evidence file lifecycle domain behavior exists, but no HTTP endpoint currently initializes or completes a file upload. `FileReference` therefore cannot be created through the exposed Evidence Item endpoint.

