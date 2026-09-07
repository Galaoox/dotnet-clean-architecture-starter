using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Company.ProjectName.Api.Observability;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var serviceName = environment.ApplicationName;
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        var openTelemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        openTelemetry.WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation());

        if (Uri.TryCreate(otlpEndpoint, UriKind.Absolute, out var endpoint))
        {
            openTelemetry.WithTracing(tracing => tracing.AddOtlpExporter(options => options.Endpoint = endpoint));
            openTelemetry.WithMetrics(metrics => metrics.AddOtlpExporter(options => options.Endpoint = endpoint));
        }

        return services;
    }
}
