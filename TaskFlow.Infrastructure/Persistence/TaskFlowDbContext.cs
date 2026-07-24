using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Persistence.Outbox;

namespace TaskFlow.Infrastructure.Persistence;

public class TaskFlowDbContext : DbContext, IApplicationDbContext
{
    public TaskFlowDbContext(DbContextOptions<TaskFlowDbContext> options) : base(options)
    {
    }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Infrastructure-only - deliberately NOT on IApplicationDbContext, since Application
    // handlers never need to query the outbox directly.
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskFlowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    // NOTE: no explicit SaveChangesAsync override needed here - DbContext's own
    // Task<int> SaveChangesAsync(CancellationToken) already matches IApplicationDbContext's
    // signature exactly, so this class satisfies the interface "for free."
}
