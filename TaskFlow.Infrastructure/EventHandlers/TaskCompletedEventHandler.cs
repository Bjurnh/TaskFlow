using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Analytics;

namespace TaskFlow.Infrastructure.EventHandlers;

public class TaskCompletedEventHandler : INotificationHandler<TaskCompletedEvent>
{
    private readonly TaskFlowDbContext _dbContext;
    private readonly ICacheService _cache;

    public TaskCompletedEventHandler(TaskFlowDbContext dbContext, ICacheService cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    public async Task Handle(TaskCompletedEvent notification, CancellationToken cancellationToken)
    {
        var snapshot = await AnalyticsSnapshotHelper.GetOrCreateTodaySnapshotAsync(
            _dbContext, notification.OrganizationId, notification.ProjectId, cancellationToken);

        // The event itself only carries CompletedAt - CreatedAt has to come from the task
        // row directly, since domain events are deliberately kept small (only what a
        // subscriber needs), not a full snapshot of the entity.
        var createdAt = await _dbContext.Tasks
            .Where(t => t.Id == notification.TaskId)
            .Select(t => (DateTime?)t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        snapshot.TasksCompleted++;

        if (createdAt is not null)
        {
            var hoursToComplete = (decimal)(notification.CompletedAt - createdAt.Value).TotalHours;

            // Rolling average: new_avg = old_avg + (new_value - old_avg) / new_count.
            // Avoids storing every individual completion time just to compute a mean.
            snapshot.AvgCompletionHours = snapshot.AvgCompletionHours is null
                ? hoursToComplete
                : snapshot.AvgCompletionHours.Value + (hoursToComplete - snapshot.AvgCompletionHours.Value) / snapshot.TasksCompleted;
        }

        // Refresh the Redis-backed read model so a future dashboard endpoint can serve
        // "today's numbers" straight from cache instead of hitting Postgres on every request.
        var cacheKey = $"analytics:{notification.OrganizationId}:{DateOnly.FromDateTime(DateTime.UtcNow)}";
        await _cache.SetAsync(
            cacheKey,
            new
            {
                snapshot.OrganizationId,
                snapshot.ProjectId,
                snapshot.SnapshotDate,
                snapshot.TasksCreated,
                snapshot.TasksCompleted,
                snapshot.AvgCompletionHours
            },
            TimeSpan.FromDays(2),
            cancellationToken);
    }
}
