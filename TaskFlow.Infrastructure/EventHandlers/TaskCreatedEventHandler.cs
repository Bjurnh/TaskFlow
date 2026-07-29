using MediatR;
using TaskFlow.Domain.Events;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Analytics;

namespace TaskFlow.Infrastructure.EventHandlers;

/// <summary>
/// Lives in Infrastructure, not Application - this handler maintains a derived read model
/// (AnalyticsDailySnapshot), it doesn't enforce any business rule. Application handlers own
/// business logic; this is closer to plumbing, so it sits alongside the Outbox machinery
/// that triggers it.
/// </summary>
public class TaskCreatedEventHandler : INotificationHandler<TaskCreatedEvent>
{
    private readonly TaskFlowDbContext _dbContext;

    public TaskCreatedEventHandler(TaskFlowDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(TaskCreatedEvent notification, CancellationToken cancellationToken)
    {
        var snapshot = await AnalyticsSnapshotHelper.GetOrCreateTodaySnapshotAsync(
            _dbContext, notification.OrganizationId, notification.ProjectId, cancellationToken);

        snapshot.TasksCreated++;

        // Deliberately no SaveChangesAsync here - OutboxProcessorService saves once per
        // batch after all handlers for that batch have run, not once per event.
    }
}
