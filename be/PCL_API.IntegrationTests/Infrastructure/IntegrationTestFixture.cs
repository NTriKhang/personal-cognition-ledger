using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PCL.Modules.Evidence.Infrastructure.Database;
using PCL.Modules.Session.Infrastructure.Database;
using PCL.Modules.TaskPlanning.Infrastructure.Database;
using PCL_API.IntegrationTests.Infrastructure.Messaging;
using Respawn;
using Testcontainers.PostgreSql;
using Xunit;

namespace PCL_API.IntegrationTests.Infrastructure;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database =
        new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("pcl_integration_tests")
            .WithUsername("pcl")
            .WithPassword("pcl")
            .Build();

    private Respawner _respawner = null!;

    public PclApiFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public IntegrationMessageProcessor Messages { get; private set; } = null!;
    public string LocalStorageRoot { get; private set; } = string.Empty;

    public async ValueTask InitializeAsync()
    {
        LocalStorageRoot = Path.Combine(
            Path.GetTempPath(),
            "pcl-integration-tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(LocalStorageRoot);

        await _database.StartAsync();

        Factory = new PclApiFactory(
            _database.GetConnectionString(),
            LocalStorageRoot);

        Client = Factory.CreateClient();
        Messages = new IntegrationMessageProcessor(
            _database.GetConnectionString(),
            Factory.Services.GetRequiredService<Quartz.ISchedulerFactory>());

        await ApplyMigrationsAsync();

        await using var connection = new NpgsqlConnection(
            _database.GetConnectionString());
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude =
                [
                    "evidence",
                    "session",
                    "task_planning"
                ],
                TablesToIgnore =
                [
                    new Respawn.Graph.Table("evidence", "__EFMigrationsHistory"),
                    new Respawn.Graph.Table("session", "__EFMigrationsHistory"),
                    new Respawn.Graph.Table("task_planning", "__EFMigrationsHistory")
                ],
                WithReseed = true
            });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(
            _database.GetConnectionString());
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);

        Factory.Services
            .GetRequiredService<FakeS3StorageProfileVerifier>()
            .Reset();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _database.DisposeAsync();

        if (Directory.Exists(LocalStorageRoot))
        {
            Directory.Delete(LocalStorageRoot, recursive: true);
        }
    }

    private async Task ApplyMigrationsAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();

        await scope.ServiceProvider
            .GetRequiredService<EvidenceDbContext>()
            .Database
            .MigrateAsync();

        await scope.ServiceProvider
            .GetRequiredService<LSessionDbContext>()
            .Database
            .MigrateAsync();

        await scope.ServiceProvider
            .GetRequiredService<TaskPlanningDbContext>()
            .Database
            .MigrateAsync();
    }

}
