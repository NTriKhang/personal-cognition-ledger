using Common.Application.Messaging;
using Common.Infrastructure;
using Common.Infrastructure.Outbox;
using Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Application.Repositories;
using PCL.Modules.Evidence.Application.Storage;
using PCL.Modules.Evidence.Infrastructure.Database;
using PCL.Modules.Evidence.Infrastructure.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Storage;
using PCL.Modules.Evidence.Infrastructure.Storage.AmazonS3;
using PCL.Modules.Evidence.Infrastructure.Storage.Local;

namespace PCL.Modules.Evidence.Infrastructure;

public static class EvidenceModule
{
    internal const string ModuleName = "Evidence";

    public static IServiceCollection AddEvidenceModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDomainEventHandlers<EvidenceModuleMarker>();
        services.AddInfrastructure(configuration);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        return services;
    }

    private static void AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EvidenceDbContext>((sp, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("Database"),
                npgsqlOptions => npgsqlOptions
                    .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Evidence))
            .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<EvidenceDbContext>());
        services.AddScoped<IEvidenceItemRepository, EvidenceItemRepository>();
        services.AddScoped<IStorageProfileRepository, StorageProfileRepository>();
        services.AddScoped<
            IEvidenceStorageSettingsRepository,
            EvidenceStorageSettingsRepository>();

        services.AddScoped<
            IEvidenceStorageProfileVerifier,
            LocalStorageProfileVerifier>();
        services.AddScoped<
            IEvidenceStorageProfileVerifier,
            S3StorageProfileVerifier>();
        services.AddScoped<
            IEvidenceStorageProfileVerifierResolver,
            EvidenceStorageProfileVerifierResolver>();

        services.AddScoped<IEvidenceFileStorage, LocalEvidenceFileStorage>();
        services.AddScoped<IEvidenceFileStorage, S3EvidenceFileStorage>();
        services.AddScoped<IEvidenceFileStorageResolver, EvidenceFileStorageResolver>();

        services.AddOptions<LocalEvidenceStorageOptions>()
            .Bind(configuration.GetSection(LocalEvidenceStorageOptions.SectionName))
            .Validate(
                options => options.AllowedRootDirectories.All(
                    Path.IsPathFullyQualified),
                "All allowed Local storage roots must be absolute paths.")
            .ValidateOnStart();

        services.AddOutboxProcessor<EvidenceModuleMarker>(
            configuration.GetSection("Outbox:Evidence"));
    }

    private static void AddDomainEventHandlers<TModule>(this IServiceCollection services)
        where TModule : IModuleMarker
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(type =>
                    type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<,>)
                .MakeGenericType(domainEvent, typeof(TModule));

            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }
}

public sealed class EvidenceModuleMarker : IModuleMarker
{
    public static string ModuleName => EvidenceModule.ModuleName;
}
