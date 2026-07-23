using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Project : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    private readonly List<TaskItem> _tasks = new();
    public IReadOnlyCollection<TaskItem> Tasks => _tasks.AsReadOnly();

    public bool IsArchived => ArchivedAt.HasValue;

    private Project() { } // EF Core

    public static Project Create(Guid organizationId, string name, string? description, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name is required.", nameof(name));

        return new Project
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Description = description?.Trim(),
            CreatedByUserId = createdByUserId
        };
    }

    public void Archive() => ArchivedAt = DateTime.UtcNow;
}
