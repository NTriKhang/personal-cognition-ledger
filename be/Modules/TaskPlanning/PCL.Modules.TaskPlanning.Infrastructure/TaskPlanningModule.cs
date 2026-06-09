using Common.Application.Messaging;
using Common.Infrastructure;
using Common.Infrastructure.Outbox;
using Common.Infrastructure.Inbox;
using Common.Presentation.Endpoints;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PCL.Modules.Session.Contracts.LSessions;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Application.Tasks.AssignmentEligibility;
using PCL.Modules.TaskPlanning.Contracts.Tasks;
using PCL.Modules.TaskPlanning.Infrastructure.Database;
using PCL.Modules.TaskPlanning.Infrastructure.Tasks;

namespace PCL.Modules.TaskPlanning.Infrastructure
{
    public static class TaskPlanningModule
    {
        internal const string ModuleName = "TaskPlanning";

        public static IServiceCollection AddTaskPlanningModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDomainEventHandlers<TaskPlanningModuleMarker>();
            services.AddInfrastructure(configuration);
            services.AddEndpoints(Presentation.AssemblyReference.Assembly);

            return services;
        }

        private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TaskPlanningDbContext>((sp, options) =>
                options.UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.TaskPlanning))
                .AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TaskPlanningDbContext>());
            services.AddScoped<ITaskRepository, TaskRepository>();

            services.AddOutboxProcessor<TaskPlanningModuleMarker>(
                configuration.GetSection("Outbox:TaskPlanning"));

            services.AddInboxProcessor<TaskPlanningModuleMarker>(
                configuration.GetSection("Inbox:TaskPlanning"));
        }

        private static void AddDomainEventHandlers<TModule>(this IServiceCollection services)
            where TModule : IModuleMarker
        {
            services.AddScoped<ITaskAssignmentEligibilityChecker, TaskAssignmentEligibilityChecker>();

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

        public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
        {
            registrationConfigurator.AddInboxConsumer<TaskAssignedToSessionIntegrationEvent, TaskPlanningModuleMarker>();
        }
    }

    public sealed class TaskPlanningModuleMarker : IModuleMarker
    {
        public static string ModuleName => TaskPlanningModule.ModuleName;
    }
}
