using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Evidence.Domain.EvidenceItems;

namespace PCL.Modules.Evidence.Infrastructure.EvidenceItems;

internal sealed class EvidenceFileInitializationConfiguration : IEntityTypeConfiguration<EvidenceFileInitialization>
{
    public void Configure(EntityTypeBuilder<EvidenceFileInitialization> builder)
    {
        builder.ToTable("evidence_file_initialization");
        builder.HasKey(x => new { x.OwnerId, x.IdempotencyKey });
        builder.Property(x => x.IdempotencyKey).HasMaxLength(200);
        builder.Property(x => x.RequestHash).HasMaxLength(64);
        builder.Property(x => x.EvidenceItemId).HasConversion(x => x.Value, x => EvidenceItemId.From(x));
        builder.HasIndex(x => x.EvidenceItemId).IsUnique();
        builder.HasOne<EvidenceItem>().WithOne().HasForeignKey<EvidenceFileInitialization>(x => x.EvidenceItemId).OnDelete(DeleteBehavior.Restrict);
    }
}
