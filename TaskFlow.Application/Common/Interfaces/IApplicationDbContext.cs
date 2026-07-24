using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Common.Interfaces;

/// <summary>
/// Application handlers depend on this interface, not on TaskFlowDbContext directly.
/// Infrastructure's TaskFlowDbContext (Step 3) implements it. This is what lets us
/// unit-test handlers against an in-memory/SQLite provider without referencing
/// Infrastructure from the Application test project.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Organization> Organizations { get; }
    DbSet<User> Users { get; }
    DbSet<Project> Projects { get; }
    DbSet<TaskItem> Tasks { get; }
    DbSet<Tag> Tags { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
