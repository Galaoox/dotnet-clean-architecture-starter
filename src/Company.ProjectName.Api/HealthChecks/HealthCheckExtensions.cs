using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Company.ProjectName.Api.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddDbContextCheck<ApplicationDbContext>(
                name: "postgresql",
                tags: ["ready"]);

        return services;
    }
}
