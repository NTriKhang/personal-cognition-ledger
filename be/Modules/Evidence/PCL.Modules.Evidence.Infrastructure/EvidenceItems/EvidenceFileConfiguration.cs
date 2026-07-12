using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

internal sealed class EvidenceFileConfiguration : IEntityTypeConfiguration<EvidenceFile>
{
    public void Configure(EntityTypeBuilder<EvidenceFile> builder)
    {
        builder.ToTable(
            "evidence_file",
            table => table.HasCheckConstraint(
                "CK_evidence_file_FileSizeBytes",
                $"\"FileSizeBytes\" > 0 AND \"FileSizeBytes\" <= {EvidenceFilePolicy.MaximumFileSizeBytes}"));

        builder.HasKey(evidenceFile => evidenceFile.EvidenceItemId);

        builder.Property(evidenceFile => evidenceFile.EvidenceItemId)
            .HasConversion(
                evidenceItemId => evidenceItemId.Value,
                value => EvidenceItemId.From(value))
            .ValueGeneratedNever();

        builder.Property(evidenceFile => evidenceFile.UploadAttemptId)
            .HasConversion(
                uploadAttemptId => uploadAttemptId.Value,
                value => EvidenceFileUploadAttemptId.From(value))
            .ValueGeneratedNever();

        builder.Property(evidenceFile => evidenceFile.StorageProfileId)
            .HasConversion(id => id.Value, value => StorageProfileId.From(value))
            .ValueGeneratedNever();

        builder.HasOne<StorageProfile>()
            .WithMany()
            .HasForeignKey(evidenceFile => evidenceFile.StorageProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(evidenceFile => evidenceFile.ObjectKey)
            .HasMaxLength(EvidenceFile.MaximumObjectKeyLength)
            .IsRequired();

        builder.Property(evidenceFile => evidenceFile.OriginalFileName)
            .HasMaxLength(EvidenceFile.MaximumOriginalFileNameLength)
            .IsRequired();

        builder.Property(evidenceFile => evidenceFile.ContentType)
            .HasMaxLength(EvidenceFile.MaximumContentTypeLength)
            .IsRequired();

        builder.Property(evidenceFile => evidenceFile.FileSizeBytes).IsRequired();

        builder.Property(evidenceFile => evidenceFile.UploadStatus)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(evidenceFile => evidenceFile.ChecksumAlgorithm)
            .HasMaxLength(EvidenceFile.MaximumChecksumAlgorithmLength);

        builder.Property(evidenceFile => evidenceFile.ChecksumValue)
            .HasMaxLength(EvidenceFile.MaximumChecksumValueLength);

        builder.Property(evidenceFile => evidenceFile.VersionId)
            .HasMaxLength(EvidenceFile.MaximumVersionIdLength);

        builder.Property(evidenceFile => evidenceFile.CreatedAt).IsRequired();
        builder.Property(evidenceFile => evidenceFile.UploadExpiresAt).IsRequired();
        builder.Property(evidenceFile => evidenceFile.StatusChangedAt).IsRequired();
        builder.Property(evidenceFile => evidenceFile.UploadedAt);

        builder.Property(evidenceFile => evidenceFile.FailureReason)
            .HasMaxLength(EvidenceFile.MaximumFailureReasonLength);
        builder.Property(evidenceFile => evidenceFile.PhysicalDeletedAt);
        builder.Property(evidenceFile => evidenceFile.CleanupAttempts).IsRequired();
        builder.Property(evidenceFile => evidenceFile.CleanupNextAttemptAt);
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasOne<EvidenceItem>()
            .WithOne(evidenceItem => evidenceItem.File)
            .HasForeignKey<EvidenceFile>(evidenceFile => evidenceFile.EvidenceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(evidenceFile => evidenceFile.ObjectKey).IsUnique();
        builder.HasIndex(evidenceFile => evidenceFile.UploadAttemptId).IsUnique();
        builder.HasIndex(evidenceFile => new
        {
            evidenceFile.UploadStatus,
            evidenceFile.UploadExpiresAt
        });
    }
}
