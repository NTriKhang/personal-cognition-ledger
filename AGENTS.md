# Repository instructions for coding agents

## Read first

- Use `docs/README.md` to locate the authoritative document for a fact.
- Read `docs/architecture.md` and `docs/module-boundaries.md` before changing dependencies, messaging, persistence, or cross-module behavior.
- Read `docs/domain-model.md` before changing domain states or invariants.
- Read `docs/api/reference.md` and `be/PCL_API.IntegrationTests/README.md` before changing HTTP behavior or integration tests.
- Treat `docs/plans/` as approved future design, not implemented behavior.

## Repository map

- `be/Common`: shared abstractions and cross-cutting infrastructure
- `be/Modules/<Module>`: module-owned Domain, Application, Infrastructure, Presentation, and Contracts projects
- `be/PCL_API`: API host, configuration, migrations at development startup, and solution file
- `be/PCL_API.IntegrationTests`: HTTP integration suite and controlled test infrastructure
- `docs`: maintained documentation with one authority per subject

## Required rules

- Preserve module ownership. Never access another module's tables or domain types directly.
- Put business rules in the owning Domain project and orchestration in Application.
- Use narrow provider-owned application contracts for synchronous cross-module validation.
- Use integration events for asynchronous reactions; account for eventual consistency and idempotency.
- Keep AWS types and provider-specific details out of Domain and Application contracts.
- Do not hand-edit EF Core model snapshots except to intentionally resolve migration metadata.
- Do not invent authentication guarantees: production authorization is currently disabled.
- Do not present planned behavior as implemented behavior.
- Preserve unrelated user changes in the working tree.

## Verification

From `be/PCL_API`:

```powershell
dotnet build PCL_API.sln
dotnet test PCL_API.sln
```

Integration tests require Docker. Prefer focused tests while iterating, then run the complete relevant suite. Update the authoritative documentation and executable `.http` requests when public behavior changes.
