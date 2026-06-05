using Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Infrastructure.Tasks;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Infrastructure.Database
{
    public class TaskPlanningDbContext(DbContextOptions<TaskPlanningDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<PlanningTask> Tasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.TaskPlanning);

            TaskConfiguration.ConfigureSequence(modelBuilder);

            modelBuilder.ApplyConfiguration(new TaskConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        }
    }
}
