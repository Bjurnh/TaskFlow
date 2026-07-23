using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Tag : BaseEntity
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Color { get; private set; } = "#6366f1";

    private Tag() { } // EF Core

    public static Tag Create(Guid organizationId, string name, string? color = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        return new Tag
        {
            OrganizationId = organizationId,
            Name = name.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? "#6366f1" : color
        };
    }
}
