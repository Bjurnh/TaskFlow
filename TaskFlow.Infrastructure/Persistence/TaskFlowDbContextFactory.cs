using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TaskFlow.Infrastructure.Persistence;

/// <summary>
/// Used ONLY by `dotnet ef migrations add` / `dotnet ef database update` at design time.
/// The running app never touches this class - Program.cs wires up TaskFlowDbContext through
/// normal DI in DependencyInjection.AddInfrastructure(). This factory exists purely so EF's
/// CLI tooling can construct a context without spinning up the whole application host.
///
/// Run migration commands from the solution root once TaskFlow.Api exists:
///   dotnet ef migrations add InitialCreate -p src/TaskFlow.Infrastructure -s src/TaskFlow.Api
///   dotnet ef database update -p src/TaskFlow.Infrastructure -s src/TaskFlow.Api
/// Until the API project exists, this factory's hardcoded fallback connection string lets
/// you run migrations directly against Infrastructure for a quick local sanity check.
/// </summary>
public class TaskFlowDbContextFactory : IDesignTimeDbContextFactory<TaskFlowDbContext>
{
    public TaskFlowDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=taskflow;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<TaskFlowDbContext>();
        optionsBuilder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();

        return new TaskFlowDbContext(optionsBuilder.Options);
    }
}
