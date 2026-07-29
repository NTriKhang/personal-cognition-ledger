using Common.Application.Messaging;
using Common.Infrastructure;
using Common.Infrastructure.Outbox;
using Common.Presentation.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PCL.Modules.Session.Application.Abstractions.Data;
using PCL.Modules.Session.Application.LSessions.AssignTaskToSession;
using PCL.Modules.Session.Application.LSessions.EvidenceAttachmentEligibility;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.Session.Infrastructure.Database;
using PCL.Modules.Session.Infrastructure.LSessions;

namespace PCL.Modules.Session.Infrastructure
{
    public static class LSessionModule
    {
        internal const string ModuleName = "Session";

        public static IServiceCollection AddLSessionModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            services.AddDomainEventHandlers<SessionModuleMarker>();

            //services.AddIntegrationEventHandlers();

            services.AddInfrastructure(configuration);

            services.AddEndpoints(Presentation.AssemblyReference.Assembly);

            return services;
        }

        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<LSessionDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Session))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LSessionDbContext>());

            services.AddScoped<ILSessionRepository, LSessionRepository>();
            services.AddScoped<IAssignTaskToSessionPolicy, AssignTaskToSessionPolicy>();
            services.AddScoped<ISessionEvidenceAttachmentEligibilityChecker, SessionEvidenceAttachmentEligibilityChecker>();
            services.AddAutoMapper((sp, cfg) => { }, Session.Application.AssemblyReference.Assembly);

            services.AddOutboxProcessor<SessionModuleMarker>(
                configuration.GetSection("Outbox:Session"));

        }

        private static void AddDomainEventHandlers<TModule>(this IServiceCollection services)
            where TModule : IModuleMarker
        {
            Type[] domainEventHandlers = Application.AssemblyReference.Assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
                .ToArray();

            foreach (Type domainEventHandler in domainEventHandlers)
            {
                services.TryAddScoped(domainEventHandler);

                Type domainEvent = domainEventHandler
                    .GetInterfaces()
                    .Single(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                    .GetGenericArguments()
                    .Single();

                Type closedIdempotentHandler = typeof(IdempotentDomainEventHandler<,>)
                    .MakeGenericType(domainEvent, typeof(TModule));

                services.Decorate(domainEventHandler, closedIdempotentHandler);
            }
        }
    }

    public sealed class SessionModuleMarker : IModuleMarker
    {
        public static string ModuleName => LSessionModule.ModuleName;
    }
}

