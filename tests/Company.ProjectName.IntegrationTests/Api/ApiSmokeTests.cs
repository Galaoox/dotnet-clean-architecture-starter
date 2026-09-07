using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace Company.ProjectName.IntegrationTests.Api;

public sealed class ApiSmokeTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("appdb")
        .WithUsername("app")
        .WithPassword("localdev")
        .Build();

    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync(CancellationToken.None);

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("ConnectionStrings:Database", _postgres.GetConnectionString());
            builder.UseSetting("Jwt:Issuer", "Company.ProjectName.Tests");
            builder.UseSetting("Jwt:Audience", "Company.ProjectName.TestClients");
            builder.UseSetting("Jwt:SigningKey", "test-only-signing-key-that-is-at-least-64-characters-long-0123456789");
            builder.UseSetting("Jwt:AccessTokenMinutes", "15");
            builder.UseSetting("Jwt:RefreshTokenDays", "7");
            builder.UseSetting(WebHostDefaults.EnvironmentKey, "Testing");
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(CancellationToken.None);

        _client = _factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Health_live_is_available_without_authentication()
    {
        var response = await _client.GetAsync("/health/live", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_then_create_product_uses_jwt_authorization()
    {
        var registerResponse = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email = "api@example.com", password = "Valid-password-123!" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        using var authJson = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var accessToken = authJson.RootElement.GetProperty("accessToken").GetString();
        Assert.False(string.IsNullOrWhiteSpace(accessToken));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/products",
            new { name = "API Keyboard", price = 100m },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
    }
}
