using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PCL.Modules.Session.Domain.LSessions;

namespace PCL.Modules.Session.Infrastructure.LSessions;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<LSession>
{
    public void Configure(EntityTypeBuilder<LSession> builder)
    {
        builder.HasKey(e => e.Id);
    }
}
