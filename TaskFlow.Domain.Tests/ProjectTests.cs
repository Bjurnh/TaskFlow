using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Tests;

public class ProjectTests
{
    [Fact]
    public void Create_WhenNameIsWhitespace_ThrowsArgumentException()
    {
        var organizationId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        var exception = Assert.Throws<ArgumentException>(() =>
            Project.Create(organizationId, "   ", "Some description", createdByUserId));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Create_WhenNameIsValid_SetsExpectedValues()
    {
        var organizationId = Guid.NewGuid();
        var createdByUserId = Guid.NewGuid();

        var project = Project.Create(organizationId, "Roadmap", "Project notes", createdByUserId);

        Assert.Equal(organizationId, project.OrganizationId);
        Assert.Equal("Roadmap", project.Name);
        Assert.Equal("Project notes", project.Description);
        Assert.Equal(createdByUserId, project.CreatedByUserId);
        Assert.False(project.IsArchived);
    }

    [Fact]
    public void Archive_WhenCalled_SetsArchivedAt()
    {
        var project = Project.Create(Guid.NewGuid(), "Support", null, Guid.NewGuid());

        project.Archive();

        Assert.True(project.IsArchived);
        Assert.NotNull(project.ArchivedAt);
    }
}