using Microsoft.EntityFrameworkCore;
using PCL.Modules.Session.Application.Abstractions.Data;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Infrastructure.Database
{
    /// <summary>
    /// EF Core DbContext for LearningSession service.
    /// The domain keeps the activity ids as an internal collection; we do not map it to a dedicated column here.
    /// </summary>
    public class LSessionDbContext(DbContextOptions<LSessionDbContext> options) : DbContext(options), IUnitOfWork
    {
        public DbSet<LSession> LSessions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schemas.Session);

            modelBuilder.HasSequence<int>(Sequences.LSessionSq, schema: Schemas.Session)
                .StartsAt(1)
                .IncrementsBy(2);

            modelBuilder.Entity<LSession>(b =>
            {
                b.ToTable("lsession");
                b.HasKey(e => e.Id);

                b.Property(e => e.OwnerId).IsRequired();
                b.Property(e => e.StartedAt).IsRequired();
                b.Property(e => e.EndedAt);
                b.Property(e => e.Title).IsRequired();

                b.Property(e => e.Code)
                        .HasDefaultValueSql($"nextval('\"{Schemas.Session}\".\"{Sequences.LSessionSq}\"')")
                        .ValueGeneratedOnAdd();

                // store enum as string
                b.Property(e => e.Status)
                    .HasConversion<string>()
                    .IsRequired();

                b.HasIndex(e => e.OwnerId);
                b.HasIndex(e => new { e.OwnerId, e.Status });

                // Do not map the in-memory activity id collection to a column.
                b.Ignore(e => e.TaskIds);
            });
        }
    }
}

