using TaskFlow.Domain.Common;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Events;

namespace TaskFlow.Domain.Entities;

/// <summary>
/// Named TaskItem (not Task) to avoid colliding with System.Threading.Tasks.Task -
/// see the comment on Enums/TaskStatus.cs for the full reasoning.
///
/// This is an AggregateRoot: all state transitions are exposed as intention-revealing
/// methods (ChangeStatus, AssignTo) rather than public setters, and every meaningful
/// transition raises a domain event. Those events get picked up by an EF Core
/// SaveChangesInterceptor in Infrastructure (Step 3) and written to the Outbox table
/// in the SAME transaction as the state change - guaranteeing we never persist a task
/// completion without also durably recording the event that says it happened.
/// </summary>
public class TaskItem : AggregateRoot
{
    public Guid OrganizationId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskStatus Status { get; private set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; private set; } = TaskPriority.Medium;
    public Guid? AssigneeId { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    private TaskItem() { } // EF Core

    public static TaskItem Create(
        Guid organizationId,
        Guid projectId,
        string title,
        string? description,
        TaskPriority priority,
        Guid? assigneeId,
        DateTime? dueDate,
        Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));

        var task = new TaskItem
        {
            OrganizationId = organizationId,
            ProjectId = projectId,
            Title = title.Trim(),
            Description = description?.Trim(),
            Priority = priority,
            AssigneeId = assigneeId,
            DueDate = dueDate,
            CreatedByUserId = createdByUserId,
            Status = TaskStatus.Todo
        };

        task.AddDomainEvent(new TaskCreatedEvent(task.Id, task.OrganizationId, task.ProjectId, task.Title, task.CreatedAt));

        return task;
    }

    public void ChangeStatus(TaskStatus newStatus)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        if (newStatus == TaskStatus.Done)
        {
            CompletedAt = DateTime.UtcNow;
            AddDomainEvent(new TaskCompletedEvent(Id, OrganizationId, ProjectId, CompletedAt.Value));
        }
        else
        {
            CompletedAt = null;
        }

        AddDomainEvent(new TaskStatusChangedEvent(Id, OrganizationId, oldStatus, newStatus));
    }

    public void AssignTo(Guid? userId)
    {
        if (AssigneeId == userId) return;

        var previousAssignee = AssigneeId;
        AssigneeId = userId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new TaskAssignedEvent(Id, OrganizationId, previousAssignee, userId));
    }

    public void UpdateDetails(string title, string? description, TaskPriority priority, DateTime? dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        DueDate = dueDate;
        UpdatedAt = DateTime.UtcNow;
    }
}
