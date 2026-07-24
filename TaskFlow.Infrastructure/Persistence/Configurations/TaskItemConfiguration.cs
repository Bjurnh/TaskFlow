using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnType("text");

        // Enums stored as strings, not ints - protects against silently reordering values
        // if an entry is ever inserted in the middle of the enum later (e.g. adding
        // "Blocked" between InProgress and Done would renumber everything after it if
        // stored as an int, corrupting every existing row).
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        // Matches the most common query pattern: "list tasks for project X, optionally
        // filtered by status" - see GetTasksByProjectQueryHandler.
        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.OrganizationId);

        // Optimistic concurrency, Postgres-idiomatic: use the system "xmin" column instead
        // of a manually maintained rowversion/byte[] column (that's the SQL Server pattern -
        // Postgres already tracks a row version internally, so use it rather than
        // reinventing it).
        builder.Property<uint>("xmin")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .IsRowVersion();
        // DomainEvents is a public in-memory property on AggregateRoot used only for
        // collecting events before SaveChanges - it must never be mapped to a column,
        // or the first migration will either fail or generate a garbage column for it.
        builder.Ignore(t => t.DomainEvents);
    }
}
