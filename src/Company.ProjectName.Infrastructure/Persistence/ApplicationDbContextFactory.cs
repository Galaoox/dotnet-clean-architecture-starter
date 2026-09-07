using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Company.ProjectName.Infrastructure.Persistence;

/// <summary>
/// Allows `dotnet ef` to create migrations without booting the API.
/// Production startup never uses this factory.
/// </summary>
internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=appdb;Username=app;Password=localdev";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new ApplicationDbContext(options);
    }
}
