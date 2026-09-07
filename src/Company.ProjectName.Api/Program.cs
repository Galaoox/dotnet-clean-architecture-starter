using Company.ProjectName.Api.Common.Errors;
using Company.ProjectName.Api.HealthChecks;
using Company.ProjectName.Api.Observability;
using Company.ProjectName.Application;
using Company.ProjectName.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplicationHealthChecks();
builder.Services.AddObservability(builder.Configuration, builder.Environment);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Products.Write", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Company.ProjectName API"));
}

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
});

app.MapGet("/", () => Results.Ok(new { status = "Company.ProjectName.Api" }));

app.Run();

public partial class Program;
