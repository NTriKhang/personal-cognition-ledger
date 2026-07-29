using Common.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using PCL.Modules.Session.Application.Abstractions.Data;
using PCL.Modules.Session.Domain.LSessions;
using PCL.Modules.Session.Infrastructure.LSessions;

namespace PCL.Modules.Session.Infrastructure.Database
{
    /// <summary>
    /// EF Core DbContext for LearningSession service.
    /// </summary>
    public class LSessionDbContext(DbContextOptions<LSessionDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<LSession> LSessions { get; set; } = null!;
        public DbSet<SessionTaskAssignment> SessionTaskAssignments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.Session);

            LSessionConfiguration.ConfigureSequence(modelBuilder);

            modelBuilder.ApplyConfiguration(new LSessionConfiguration());
            modelBuilder.ApplyConfiguration(new SessionTaskAssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        }
    }
}

