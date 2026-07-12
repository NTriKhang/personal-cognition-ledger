using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

internal sealed class EvidenceFileUploadAttemptConfiguration : IEntityTypeConfiguration<EvidenceFileUploadAttempt>
{
    public void Configure(EntityTypeBuilder<EvidenceFileUploadAttempt> builder)
    {
        builder.ToTable("evidence_file_upload_attempt");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasConversion(x => x.Value, x => EvidenceFileUploadAttemptId.From(x)).ValueGeneratedNever();
        builder.Property(x => x.EvidenceItemId).HasConversion(x => x.Value, x => EvidenceItemId.From(x)).ValueGeneratedNever();
        builder.Property(x => x.StorageProfileId).HasConversion(x => x.Value, x => StorageProfileId.From(x)).ValueGeneratedNever();
        builder.Property(x => x.ObjectKey).HasMaxLength(EvidenceFile.MaximumObjectKeyLength).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.CleanupAttempts).IsRequired();
        builder.Property(x => x.CleanupNextAttemptAt);
        builder.HasOne<EvidenceItem>().WithMany().HasForeignKey(x => x.EvidenceItemId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<StorageProfile>().WithMany().HasForeignKey(x => x.StorageProfileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.ObjectKey).IsUnique();
        builder.HasIndex(x => new { x.StatusChangedAt, x.PhysicalDeletedAt });
    }
}
