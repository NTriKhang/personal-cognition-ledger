using Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Domain.Storage;
using PCL.Modules.Evidence.Infrastructure.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.Storage;

namespace PCL.Modules.Evidence.Infrastructure.Database;

public sealed class EvidenceDbContext(DbContextOptions<EvidenceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<EvidenceItem> EvidenceItems { get; set; } = null!;
    public DbSet<EvidenceFile> EvidenceFiles { get; set; } = null!;
    public DbSet<EvidenceFileUploadAttempt> EvidenceFileUploadAttempts { get; set; } = null!;
    public DbSet<EvidenceFileInitialization> EvidenceFileInitializations { get; set; } = null!;
    public DbSet<StorageProfile> StorageProfiles { get; set; } = null!;
    public DbSet<EvidenceStorageSettings> StorageSettings { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.Evidence);

        modelBuilder.ApplyConfiguration(new EvidenceItemConfiguration());
        modelBuilder.ApplyConfiguration(new EvidenceFileConfiguration());
        modelBuilder.ApplyConfiguration(new EvidenceFileUploadAttemptConfiguration());
        modelBuilder.ApplyConfiguration(new EvidenceFileInitializationConfiguration());
        modelBuilder.ApplyConfiguration(new StorageProfileConfiguration());
        modelBuilder.ApplyConfiguration(
            new LocalStorageProfileConfigurationEntityConfiguration());
        modelBuilder.ApplyConfiguration(
            new S3StorageProfileConfigurationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new EvidenceStorageSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
    }
}
