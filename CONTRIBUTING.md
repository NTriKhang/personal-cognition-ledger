# Contributing to Personal Cognition Ledger

Thank you for contributing. This project favors small, behavior-focused changes that preserve module ownership and keep documentation aligned with code.

## Before starting

1. Read the [documentation index](docs/README.md).
2. Read [Architecture](docs/architecture.md) and [Module boundaries](docs/module-boundaries.md) for cross-module work.
3. Check the [Roadmap](docs/roadmap.md) and active plans under `docs/plans/` for scope.
4. For API work, read the [API reference](docs/api/reference.md) and the integration-test project README.

If a proposed change expands product scope or changes a module boundary, discuss and document the decision before implementing it.

## Development setup

See the [Development guide](docs/development.md) for prerequisites, commands, project layout, migrations, and testing conventions.

The standard verification command, run from `be/PCL_API`, is:

```powershell
dotnet test PCL_API.sln
```

Docker must be available because the integration suite uses Testcontainers.

## Change expectations

- Keep domain behavior inside the owning module.
- Do not read or write another module's database tables directly.
- Use provider-owned application contracts for immediate cross-module validation.
- Use integration events for asynchronous reactions and eventual consistency.
- Add or update tests for changed observable behavior.
- Update the authoritative document in the same change; do not create a second description of the same fact.
- Do not include credentials, persistent developer paths, generated build output, or presigned URLs.

## Pull requests

Keep pull requests narrow enough to review. In the description, state:

- the behavior or problem being addressed
- the modules and public contracts affected
- the checks that were run
- documentation updated, or why no documentation change was required
- known limitations or deliberately deferred work

Before requesting review:

1. Build and run the relevant tests.
2. Run the complete suite when the change affects shared infrastructure, persistence, module contracts, or API behavior.
3. Check Markdown links when documentation paths change.
4. Verify that current behavior and future plans remain clearly distinguished.

## Reporting security issues

The project does not yet publish a private security-reporting channel. Do not open a public issue containing credentials, personal data, or an exploitable vulnerability. Contact the repository owner privately until a security policy is added.
