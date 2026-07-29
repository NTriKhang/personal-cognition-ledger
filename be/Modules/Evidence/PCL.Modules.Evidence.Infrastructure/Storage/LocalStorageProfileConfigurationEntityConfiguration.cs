using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class LocalStorageProfileConfigurationEntityConfiguration
    : IEntityTypeConfiguration<LocalStorageProfileConfiguration>
{
    public void Configure(EntityTypeBuilder<LocalStorageProfileConfiguration> builder)
    {
        builder.ToTable("local_storage_profile_configuration");

        builder.HasKey(configuration => configuration.StorageProfileId);

        builder.Property(configuration => configuration.StorageProfileId)
            .HasConversion(
                storageProfileId => storageProfileId.Value,
                value => StorageProfileId.From(value))
            .ValueGeneratedNever();

        builder.Property(configuration => configuration.RootDirectory)
            .HasMaxLength(LocalStorageProfileConfiguration.MaximumRootDirectoryLength)
            .IsRequired();

        builder.HasOne<StorageProfile>()
            .WithOne(profile => profile.LocalConfiguration)
            .HasForeignKey<LocalStorageProfileConfiguration>(
                configuration => configuration.StorageProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
