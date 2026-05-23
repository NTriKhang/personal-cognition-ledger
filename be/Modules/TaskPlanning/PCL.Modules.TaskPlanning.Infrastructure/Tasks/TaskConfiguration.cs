using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.TaskPlanning.Domain.Tasks;
using PCL.Modules.TaskPlanning.Infrastructure.Database;
using PlanningTask = PCL.Modules.TaskPlanning.Domain.Tasks.Task;

namespace PCL.Modules.TaskPlanning.Infrastructure.Tasks;

internal sealed class TaskConfiguration : IEntityTypeConfiguration<PlanningTask>
{
    internal static void ConfigureSequence(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>(Sequences.TaskSq, schema: Schemas.TaskPlanning)
            .StartsAt(1)
            .IncrementsBy(2);
    }

    public void Configure(EntityTypeBuilder<PlanningTask> builder)
    {
        builder.ToTable("task");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                taskId => taskId.Value,
                value => TaskId.From(value))
            .ValueGeneratedNever();

        builder.Property(e => e.Code)
            .HasDefaultValueSql($"nextval('\"{Schemas.TaskPlanning}\".\"{Sequences.TaskSq}\"')")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.OwnerId).IsRequired();

        builder.Property(e => e.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(e => e.Priority)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.PlannedAt);
        builder.Property(e => e.ActivatedAt);
        builder.Property(e => e.CompletedAt);
        builder.Property(e => e.CancelledAt);

        builder.Property(e => e.CompletionNote)
            .HasMaxLength(2000);

        builder.Property(e => e.CancellationReason)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.Priority);
        builder.HasIndex(e => e.CreatedAt);
        builder.HasIndex(e => new { e.OwnerId, e.Status });
    }
}
