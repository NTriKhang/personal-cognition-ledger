using Common.Application.EventBus;
using Common.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL_API.IntegrationTests.Infrastructure;

public sealed class PclApiFactory(
    string databaseConnectionString,
    string localStorageRoot)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] = databaseConnectionString,
                    ["EvidenceStorage:Local:AllowedRootDirectories:0"] = localStorageRoot,
                    ["Outbox:Evidence:ModuleName"] = "Evidence",
                    ["Outbox:Evidence:HandlerAssemblyName"] = "PCL.Modules.Evidence.Application",
                    ["Outbox:Evidence:SchemaName"] = "evidence",
                    ["Outbox:Evidence:IntervalInSeconds"] = "3600",
                    ["Outbox:Evidence:BatchSize"] = "40",
                    ["Outbox:Session:ModuleName"] = "Session",
                    ["Outbox:Session:HandlerAssemblyName"] = "PCL.Modules.Session.Application",
                    ["Outbox:Session:SchemaName"] = "session",
                    ["Outbox:Session:IntervalInSeconds"] = "3600",
                    ["Outbox:Session:BatchSize"] = "40",
                    ["Outbox:TaskPlanning:ModuleName"] = "TaskPlanning",
                    ["Outbox:TaskPlanning:HandlerAssemblyName"] = "PCL.Modules.TaskPlanning.Application",
                    ["Outbox:TaskPlanning:SchemaName"] = "task_planning",
                    ["Outbox:TaskPlanning:IntervalInSeconds"] = "3600",
                    ["Outbox:TaskPlanning:BatchSize"] = "40",
                    ["Inbox:TaskPlanning:ModuleName"] = "TaskPlanning",
                    ["Inbox:TaskPlanning:HandlerAssemblyName"] = "PCL.Modules.TaskPlanning.Presentation",
                    ["Inbox:TaskPlanning:SchemaName"] = "task_planning",
                    ["Inbox:TaskPlanning:IntervalInSeconds"] = "3600",
                    ["Inbox:TaskPlanning:BatchSize"] = "40"
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<NpgsqlDataSource>();
            services.AddSingleton(
                new NpgsqlDataSourceBuilder(databaseConnectionString).Build());

            services.RemoveAll<IEventBus>();
            services.AddSingleton<TestEventBus>();
            services.AddSingleton<IEventBus>(
                provider => provider.GetRequiredService<TestEventBus>());

            ServiceDescriptor? s3Verifier = services.SingleOrDefault(
                descriptor =>
                    descriptor.ServiceType == typeof(IEvidenceStorageProfileVerifier) &&
                    descriptor.ImplementationType?.Name == "S3StorageProfileVerifier");

            if (s3Verifier is not null)
            {
                services.Remove(s3Verifier);
            }

            services.AddSingleton<FakeS3StorageProfileVerifier>();
            services.AddSingleton<IEvidenceStorageProfileVerifier>(
                provider => provider.GetRequiredService<FakeS3StorageProfileVerifier>());
        });
    }
}

public sealed class TestEventBus : IEventBus
{
    private readonly List<IIntegrationEvent> _publishedEvents = [];

    public IReadOnlyCollection<IIntegrationEvent> PublishedEvents =>
        _publishedEvents.AsReadOnly();

    public Task PublishAsync<T>(
        T integrationEvent,
        CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        _publishedEvents.Add(integrationEvent);

        return Task.CompletedTask;
    }

    public void Clear() => _publishedEvents.Clear();
}

public sealed class FakeS3StorageProfileVerifier
    : IEvidenceStorageProfileVerifier
{
    public EvidenceStorageProviderType ProviderType =>
        EvidenceStorageProviderType.AmazonS3;

    public bool ShouldSucceed { get; set; } = true;

    public Task<Result> VerifyAsync(
        StorageProfile storageProfile,
        CancellationToken cancellationToken = default)
    {
        Result result = ShouldSucceed
            ? Result.Success()
            : Result.Failure(StorageProfileErrors.AmazonS3Unavailable);

        return Task.FromResult(result);
    }
}
