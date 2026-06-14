using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

internal sealed class EvidenceItemConfiguration : IEntityTypeConfiguration<EvidenceItem>
{
    public void Configure(EntityTypeBuilder<EvidenceItem> builder)
    {
        builder.ToTable("evidence_item");

        builder.HasKey(evidenceItem => evidenceItem.Id);

        builder.Property(evidenceItem => evidenceItem.Id)
            .HasConversion(
                evidenceItemId => evidenceItemId.Value,
                value => EvidenceItemId.From(value))
            .ValueGeneratedNever();

        builder.Property(evidenceItem => evidenceItem.SessionId).IsRequired();
        builder.Property(evidenceItem => evidenceItem.OwnerId).IsRequired();

        builder.Property(evidenceItem => evidenceItem.Type)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(evidenceItem => evidenceItem.Content)
            .HasMaxLength(EvidenceItem.MaximumContentLength)
            .IsRequired();

        builder.Property(evidenceItem => evidenceItem.AddedAt).IsRequired();

        builder.Property(evidenceItem => evidenceItem.RemovedAt);
        builder.Property(evidenceItem => evidenceItem.RemovedBy);
        builder.Property(evidenceItem => evidenceItem.RemovalReason)
            .HasMaxLength(EvidenceItemErrors.MaximumRemovalReasonLength);

        builder.Ignore(evidenceItem => evidenceItem.IsRemoved);

        builder.HasIndex(evidenceItem => evidenceItem.SessionId);
        builder.HasIndex(evidenceItem => evidenceItem.OwnerId);
        builder.HasIndex(evidenceItem => new { evidenceItem.SessionId, evidenceItem.OwnerId });
        builder.HasIndex(evidenceItem => evidenceItem.AddedAt);
        builder.HasIndex(evidenceItem => evidenceItem.RemovedAt);
    }
}
