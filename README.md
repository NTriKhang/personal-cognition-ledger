# Personal Cognition Ledger

Personal Cognition Ledger (PCL) is a backend-first system for recording intended work, bounded execution sessions, and evidence of what happened. It is currently a .NET 9 modular monolith with Task Planning, Session, and Evidence modules.

The project is under active development. Authentication is not yet enforced, and the Evidence file-upload workflow is not exposed over HTTP. See the [current roadmap](docs/roadmap.md) before relying on the API in a production setting.

## What PCL models

```text
Task     = intended work
Session  = a bounded period of execution
Evidence = proof or a record of what happened
```

The modules keep those responsibilities separate. Completing a Task does not stop a Session, and stopping a Session does not complete its Tasks.

## Technology

- .NET 9 and ASP.NET Core
- Modular monolith with Clean Architecture boundaries
- CQRS with MediatR
- EF Core for writes and Dapper for reads
- PostgreSQL
- MassTransit with transactional outbox/inbox processing
- xUnit integration tests using Testcontainers and Respawn

## Repository layout

```text
be/
  Common/                    shared application and infrastructure building blocks
  Modules/                   Task Planning, Session, and Evidence modules
  PCL_API/                   API host and solution
  PCL_API.IntegrationTests/  HTTP integration test suite
docs/                        maintained product and engineering documentation
fe/                          reserved frontend workspace
```

## Quick start

Prerequisites:

- .NET 9 SDK
- PostgreSQL for running the API locally
- Docker Desktop or another Docker-compatible engine for integration tests

The development configuration expects PostgreSQL at `localhost:5432`, database `PCL`, with username and password `pcl`. Override the `ConnectionStrings__Database` environment variable if needed.

From `be/PCL_API`:

```powershell
dotnet restore PCL_API.sln
dotnet build PCL_API.sln
dotnet run --project PCL_API.csproj --launch-profile http
```

Development migrations are applied when the API starts. Swagger is available at `/swagger`.

Run the complete test suite from the same directory:

```powershell
dotnet test PCL_API.sln
```

Integration tests create a disposable PostgreSQL container; they do not use the development database.

## Documentation

Start with the [documentation index](docs/README.md). The main references are:

- [Product and scope](docs/product.md)
- [Architecture](docs/architecture.md)
- [Domain model](docs/domain-model.md)
- [Development guide](docs/development.md)
- [API reference](docs/api/reference.md)
- [Roadmap](docs/roadmap.md)

Executable request examples live under [`docs/api/requests`](docs/api/requests/).

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) before making a change. Coding agents should also follow [AGENTS.md](AGENTS.md).

## License

No license file is currently present. Until a license is added, the repository is source-available but does not grant the standard permissions expected of an open-source project.
