using Microsoft.EntityFrameworkCore;
using TaskFlow.Infrastructure.Persistence;

namespace TaskFlow.Infrastructure.Persistence.Analytics;

/// <summary>
/// Both TaskCreatedEventHandler and TaskCompletedEventHandler need the same "find today's
/// snapshot row for this org/project, or create it" logic - centralized here rather than
/// duplicated. Not safe under true concurrent writes to the SAME row (no row locking) - at
/// portfolio scale that's an acceptable trade-off, but at real production scale you'd
/// replace this with a single atomic upsert (Postgres "INSERT ... ON CONFLICT DO UPDATE")
/// instead of a read-then-write round trip.
/// </summary>
internal static class AnalyticsSnapshotHelper
{
    public static async Task<AnalyticsDailySnapshot> GetOrCreateTodaySnapshotAsync(
        TaskFlowDbContext dbContext,
        Guid organizationId,
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var snapshot = await dbContext.AnalyticsDailySnapshots
            .FirstOrDefaultAsync(
                s => s.OrganizationId == organizationId && s.ProjectId == projectId && s.SnapshotDate == today,
                cancellationToken);

        if (snapshot is not null)
        {
            return snapshot;
        }

        snapshot = new AnalyticsDailySnapshot
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            ProjectId = projectId,
            SnapshotDate = today,
            TasksCreated = 0,
            TasksCompleted = 0,
            AvgCompletionHours = null
        };

        dbContext.AnalyticsDailySnapshots.Add(snapshot);
        return snapshot;
    }
}
