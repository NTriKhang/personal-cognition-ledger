using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class EvidenceStorageSettingsConfiguration
    : IEntityTypeConfiguration<EvidenceStorageSettings>
{
    public void Configure(EntityTypeBuilder<EvidenceStorageSettings> builder)
    {
        builder.ToTable(
            "evidence_storage_settings",
            table => table.HasCheckConstraint(
                "CK_evidence_storage_settings_Singleton",
                $"\"Id\" = {EvidenceStorageSettings.SingletonId}"));

        builder.HasKey(settings => settings.Id);
        builder.Property(settings => settings.Id).ValueGeneratedNever();

        builder.Property(settings => settings.ActiveStorageProfileId)
            .HasConversion(
                storageProfileId => storageProfileId.HasValue
                    ? storageProfileId.Value.Value
                    : (Guid?)null,
                value => value.HasValue
                    ? StorageProfileId.From(value.Value)
                    : null);

        builder.Property(settings => settings.UpdatedAt).IsRequired();

        builder.HasOne<StorageProfile>()
            .WithMany()
            .HasForeignKey(settings => settings.ActiveStorageProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
