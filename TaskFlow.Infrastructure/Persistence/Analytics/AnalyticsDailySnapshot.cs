namespace TaskFlow.Infrastructure.Persistence.Analytics;

/// <summary>
/// A materialized read model, NOT part of the Domain - nothing here enforces business
/// invariants, it's purely a projection maintained by event handlers reacting to the
/// Outbox. One row per (organization, project, day). ProjectId is nullable to allow an
/// org-wide rollup row alongside per-project rows, if that's ever needed - for now,
/// handlers always populate it.
/// </summary>
public class AnalyticsDailySnapshot
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public int TasksCreated { get; set; }
    public int TasksCompleted { get; set; }
    public decimal? AvgCompletionHours { get; set; }
}
