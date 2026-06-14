using Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using PCL.Modules.Evidence.Application.Abstractions.Data;
using PCL.Modules.Evidence.Domain.EvidenceItems;
using PCL.Modules.Evidence.Infrastructure.EvidenceItems;

namespace PCL.Modules.Evidence.Infrastructure.Database;

public sealed class EvidenceDbContext(DbContextOptions<EvidenceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<EvidenceItem> EvidenceItems { get; set; } = null!;
    public DbSet<EvidenceFile> EvidenceFiles { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schemas.Evidence);

        modelBuilder.ApplyConfiguration(new EvidenceItemConfiguration());
        modelBuilder.ApplyConfiguration(new EvidenceFileConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
    }
}
