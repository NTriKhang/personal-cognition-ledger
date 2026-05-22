using Microsoft.EntityFrameworkCore;
using PCL.Modules.Session.Infrastructure.Database;
using PCL.Modules.TaskPlanning.Infrastructure.Database;

namespace PCL_API.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            ApplyMigration<LSessionDbContext>(scope);
            ApplyMigration<TaskPlanningDbContext>(scope);
        }

        private static void ApplyMigration<TDbContext>(IServiceScope scope)
            where TDbContext : DbContext
        {
            using TDbContext context = scope.ServiceProvider.GetRequiredService<TDbContext>();

            context.Database.Migrate();
        }
    }
}
