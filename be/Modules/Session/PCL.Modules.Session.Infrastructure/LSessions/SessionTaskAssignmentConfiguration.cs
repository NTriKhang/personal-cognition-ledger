using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Infrastructure.LSessions;

internal sealed class SessionTaskAssignmentConfiguration : IEntityTypeConfiguration<SessionTaskAssignment>
{
    public void Configure(EntityTypeBuilder<SessionTaskAssignment> builder)
    {
        builder.ToTable("session_task_assignments");

        builder.HasKey(e => new { e.SessionId, e.TaskId });

        builder.Property(e => e.SessionId).IsRequired();
        builder.Property(e => e.TaskId).IsRequired();
        builder.Property(e => e.AssignedAt).IsRequired();

        builder.HasIndex(e => e.TaskId);
    }
}
