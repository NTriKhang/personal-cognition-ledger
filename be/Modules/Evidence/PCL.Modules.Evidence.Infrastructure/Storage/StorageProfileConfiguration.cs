using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Storage;

internal sealed class StorageProfileConfiguration : IEntityTypeConfiguration<StorageProfile>
{
    public void Configure(EntityTypeBuilder<StorageProfile> builder)
    {
        builder.ToTable("storage_profile");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.Id)
            .HasConversion(
                storageProfileId => storageProfileId.Value,
                value => StorageProfileId.From(value))
            .ValueGeneratedNever();

        builder.Property(profile => profile.Name)
            .HasMaxLength(StorageProfile.MaximumNameLength)
            .IsRequired();

        builder.Property(profile => profile.ProviderType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(profile => profile.IsEnabled).IsRequired();
        builder.Property(profile => profile.CreatedAt).IsRequired();
        builder.Property(profile => profile.UpdatedAt);

        builder.HasIndex(profile => profile.Name).IsUnique();
        builder.HasIndex(profile => profile.ProviderType);
    }
}
