using Microsoft.EntityFrameworkCore;
using PCL.Modules.TaskPlanning.Application.Abstractions.Data;
using PCL.Modules.TaskPlanning.Domain.Tasks;
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

            modelBuilder.HasSequence<int>(Sequences.TaskSq, schema: Schemas.TaskPlanning)
                .StartsAt(1)
                .IncrementsBy(2);

            modelBuilder.Entity<PlanningTask>(b =>
            {
                b.ToTable("task");

                b.HasKey(e => e.Id);

                b.Property(e => e.Id)
                    .HasConversion(
                        taskId => taskId.Value,
                        value => TaskId.From(value))
                    .ValueGeneratedNever();

                b.Property(e => e.Code)
                    .HasDefaultValueSql($"nextval('\"{Schemas.TaskPlanning}\".\"{Sequences.TaskSq}\"')")
                    .ValueGeneratedOnAdd();

                b.Property(e => e.OwnerId).IsRequired();

                b.Property(e => e.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                b.Property(e => e.Description)
                    .HasMaxLength(4000);

                b.Property(e => e.Status)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                b.Property(e => e.Category)
                    .HasConversion<string>()
                    .HasMaxLength(64);

                b.Property(e => e.Priority)
                    .HasConversion<string>()
                    .HasMaxLength(32)
                    .IsRequired();

                b.Property(e => e.CreatedAt).IsRequired();
                b.Property(e => e.UpdatedAt);
                b.Property(e => e.PlannedAt);
                b.Property(e => e.ActivatedAt);
                b.Property(e => e.CompletedAt);
                b.Property(e => e.CancelledAt);

                b.Property(e => e.CompletionNote)
                    .HasMaxLength(2000);

                b.Property(e => e.CancellationReason)
                    .HasMaxLength(2000);

                b.HasIndex(e => e.OwnerId);
                b.HasIndex(e => e.Status);
                b.HasIndex(e => e.Category);
                b.HasIndex(e => e.Priority);
                b.HasIndex(e => e.CreatedAt);
                b.HasIndex(e => new { e.OwnerId, e.Status });
            });
        }
    }
}
