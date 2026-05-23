using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Infrastructure.Database;

namespace PCL.Modules.Session.Infrastructure.LSessions;

internal sealed class LSessionConfiguration : IEntityTypeConfiguration<LSession>
{
    internal static void ConfigureSequence(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>(Sequences.LSessionSq, schema: Schemas.Session)
            .StartsAt(1)
            .IncrementsBy(2);
    }

    public void Configure(EntityTypeBuilder<LSession> builder)
    {
        builder.ToTable("lsession");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.OwnerId).IsRequired();
        builder.Property(e => e.StartedAt).IsRequired();
        builder.Property(e => e.EndedAt);
        builder.Property(e => e.Title).IsRequired();

        builder.Property(e => e.Code)
            .HasDefaultValueSql($"nextval('\"{Schemas.Session}\".\"{Sequences.LSessionSq}\"')")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => new { e.OwnerId, e.Status });

        builder.Ignore(e => e.AssignedTaskIds);

        builder.HasMany<SessionTaskAssignment>("_taskAssignments")
            .WithOne()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation("_taskAssignments")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
