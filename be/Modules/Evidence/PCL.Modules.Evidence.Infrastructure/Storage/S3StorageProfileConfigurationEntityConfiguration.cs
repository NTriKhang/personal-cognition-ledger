using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class S3StorageProfileConfigurationEntityConfiguration
    : IEntityTypeConfiguration<S3StorageProfileConfiguration>
{
    public void Configure(EntityTypeBuilder<S3StorageProfileConfiguration> builder)
    {
        builder.ToTable("s3_storage_profile_configuration");

        builder.HasKey(configuration => configuration.StorageProfileId);

        builder.Property(configuration => configuration.StorageProfileId)
            .HasConversion(
                storageProfileId => storageProfileId.Value,
                value => StorageProfileId.From(value))
            .ValueGeneratedNever();

        builder.Property(configuration => configuration.BucketName)
            .HasMaxLength(S3StorageProfileConfiguration.MaximumBucketNameLength)
            .IsRequired();

        builder.Property(configuration => configuration.Region)
            .HasMaxLength(S3StorageProfileConfiguration.MaximumRegionLength)
            .IsRequired();

        builder.Property(configuration => configuration.KeyPrefix)
            .HasMaxLength(S3StorageProfileConfiguration.MaximumKeyPrefixLength);

        builder.HasOne<StorageProfile>()
            .WithOne(profile => profile.S3Configuration)
            .HasForeignKey<S3StorageProfileConfiguration>(
                configuration => configuration.StorageProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
