using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PCL.Modules.Session.Application.Abstractions.Data;
using PCL.Modules.Session.Application.Repositories;
using PCL.Modules.Session.Infrastructure.Database;
using PCL.Modules.Session.Infrastructure.LSessions;

namespace PCL.Modules.Session.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            string? conn = configuration.GetConnectionString("LSession") ?? configuration["ConnectionStrings:LSession"];

            services.AddDbContext<LSessionDbContext>((sp, opt) =>
                opt.UseNpgsql(conn));

            services.AddScoped<ILSessionRepository, LSessionRepository>();
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<LSessionDbContext>());
        }
    }
}

