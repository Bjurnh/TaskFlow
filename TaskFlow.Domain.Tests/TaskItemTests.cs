using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Tests;

public class TaskItemTests
{
    [Fact]
    public void Create_WhenTitleIsWhitespace_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            TaskItem.Create(
                Guid.NewGuid(),
                Guid.NewGuid(),
                "   ",
                "Description",
                TaskPriority.High,
                null,
                null,
                Guid.NewGuid()));

        Assert.Equal("title", exception.ParamName);
    }

    [Fact]
    public void Create_WhenTitleIsValid_SetsDefaults()
    {
        var organizationId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        var task = TaskItem.Create(
            organizationId,
            projectId,
            "Ship feature",
            "Feature details",
            TaskPriority.High,
            null,
            null,
            createdByUserId);

        Assert.Equal(organizationId, task.OrganizationId);
        Assert.Equal(projectId, task.ProjectId);
        Assert.Equal("Ship feature", task.Title);
        Assert.Equal(TaskPriority.High, task.Priority);
        Assert.Equal(WorkItemStatus.Todo, task.Status);
        Assert.NotNull(task.CreatedAt);
    }

    [Fact]
    public void ChangeStatus_WhenSetToDone_SetsCompletedAt()
    {
        var task = TaskItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ship feature",
            "Feature details",
            TaskPriority.High,
            null,
            null,
            Guid.NewGuid());

        task.ChangeStatus(WorkItemStatus.Done);

        Assert.Equal(WorkItemStatus.Done, task.Status);
        Assert.NotNull(task.CompletedAt);
    }

    [Fact]
    public void AssignTo_WhenCalled_UpdatesAssignee()
    {
        var task = TaskItem.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Ship feature",
            "Feature details",
            TaskPriority.High,
            null,
            null,
            Guid.NewGuid());

        var assigneeId = Guid.NewGuid();
        task.AssignTo(assigneeId);

        Assert.Equal(assigneeId, task.AssigneeId);
    }
}