using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PCL.Modules.Session.Application.Abstractions.Data;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Infrastructure.Database;
using PCL.Modules.Session.Infrastructure.LSessions;
using Common.Presentation.Endpoints;
using Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PCL.Modules.Session.Infrastructure
{
    public static class LSessionModule
    {
        public static IServiceCollection AddLSessionModule(
        this IServiceCollection services,
        IConfiguration configuration)
        {
            //services.AddDomainEventHandlers();

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
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Session)));
                //.AddInterceptors(sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LSessionDbContext>());

            services.AddScoped<ILSessionRepository, LSessionRepository>();
            services.AddAutoMapper((sp, cfg) => { }, Session.Application.AssemblyReference.Assembly);
        }
    }
}

