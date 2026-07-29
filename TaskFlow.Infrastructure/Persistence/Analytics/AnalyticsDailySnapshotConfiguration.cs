using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TaskFlow.Infrastructure.Persistence.Analytics;

public class AnalyticsDailySnapshotConfiguration : IEntityTypeConfiguration<AnalyticsDailySnapshot>
{
    public void Configure(EntityTypeBuilder<AnalyticsDailySnapshot> builder)
    {
        builder.ToTable("analytics_daily_snapshots");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.AvgCompletionHours)
            .HasColumnType("numeric(10,2)");

        // One row per (org, project, day) - this is what the handlers query against to
        // decide "update existing row" vs "insert new row" for today.
        builder.HasIndex(a => new { a.OrganizationId, a.ProjectId, a.SnapshotDate })
            .IsUnique();
    }
}
