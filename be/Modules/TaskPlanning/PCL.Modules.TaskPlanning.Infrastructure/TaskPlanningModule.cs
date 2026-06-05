using Common.Infrastructure.Outbox;
using Common.Presentation.Endpoints;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Application.Repositories;
using PCL.Modules.TaskPlanning.Application.Tasks;
using PCL.Modules.TaskPlanning.Application.Tasks.AssignmentEligibility;
using PCL.Modules.TaskPlanning.Contracts.Tasks;
using PCL.Modules.TaskPlanning.Infrastructure.Database;
using PCL.Modules.TaskPlanning.Infrastructure.Tasks;

namespace PCL.Modules.TaskPlanning.Infrastructure
{
    public static class TaskPlanningModule
    {
        public static IServiceCollection AddTaskPlanningModule(
            this IServiceCollection services,
            IConfiguration configuration)
        {
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
            services.AddScoped<ITaskAssignmentEligibilityChecker, TaskAssignmentEligibilityChecker>();
            services.AddScoped<TaskProjectionAffectingDomainEventHandler>();

            services.AddOutboxProcessor(
                moduleName: "TaskPlanning",
                configuration.GetSection("Outbox:TaskPlanning"));
        }

        public static void ConfigureConsumers(IRegistrationConfigurator registrationConfigurator)
        {

            //registrationConfigurator.ADdconsumer;
        }
    }
}
