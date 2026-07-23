using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;

    private readonly List<User> _users = new();
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    private Organization() { }

    public static Organization Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Organization naame is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Organization slug is required.", nameof(slug));

        return new Organization
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant()
        };
    }





}
