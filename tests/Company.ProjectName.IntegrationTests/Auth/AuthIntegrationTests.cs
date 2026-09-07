using Company.ProjectName.Application.Auth;
using Company.ProjectName.Infrastructure;
using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Company.ProjectName.IntegrationTests.Auth;

public sealed class AuthIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("appdb")
        .WithUsername("app")
        .WithPassword("localdev")
        .Build();

    private ServiceProvider _services = null!;

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync(CancellationToken.None);

        var configuration = new ConfigurationManager();
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = _postgres.GetConnectionString(),
            ["Jwt:Issuer"] = "Company.ProjectName.Tests",
            ["Jwt:Audience"] = "Company.ProjectName.TestClients",
            ["Jwt:SigningKey"] = "test-only-signing-key-that-is-at-least-64-characters-long-0123456789",
            ["Jwt:AccessTokenMinutes"] = "15",
            ["Jwt:RefreshTokenDays"] = "7",
        });

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        _services = services.BuildServiceProvider();

        await using var scope = _services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        await _services.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Register_creates_user_with_user_role_and_tokens()
    {
        await using var scope = _services.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

        var result = await auth.RegisterAsync(
            new RegisterRequest("user@example.com", "Valid-password-123!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("user@example.com", result.Value.Email);
        Assert.Contains("User", result.Value.Roles);
        Assert.False(string.IsNullOrWhiteSpace(result.Value.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Login_returns_tokens_for_valid_credentials()
    {
        await using var scope = _services.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        await auth.RegisterAsync(
            new RegisterRequest("user@example.com", "Valid-password-123!"),
            TestContext.Current.CancellationToken);

        var result = await auth.LoginAsync(
            new LoginRequest("user@example.com", "Valid-password-123!"),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(string.Empty, result.Value.AccessToken);
    }

    [Fact]
    public async Task Refresh_rotates_token_and_reuse_revokes_family()
    {
        await using var scope = _services.CreateAsyncScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var registered = await auth.RegisterAsync(
            new RegisterRequest("user@example.com", "Valid-password-123!"),
            TestContext.Current.CancellationToken);

        var rotated = await auth.RefreshAsync(
            new RefreshRequest(registered.Value.RefreshToken),
            TestContext.Current.CancellationToken);

        Assert.True(rotated.IsSuccess);
        Assert.NotEqual(registered.Value.RefreshToken, rotated.Value.RefreshToken);

        var reused = await auth.RefreshAsync(
            new RefreshRequest(registered.Value.RefreshToken),
            TestContext.Current.CancellationToken);

        Assert.True(reused.IsFailure);
        Assert.Equal(AuthErrors.InvalidRefreshToken, reused.Error);

        var currentTokenAfterReuse = await auth.RefreshAsync(
            new RefreshRequest(rotated.Value.RefreshToken),
            TestContext.Current.CancellationToken);

        Assert.True(currentTokenAfterReuse.IsFailure);
        Assert.Equal(AuthErrors.InvalidRefreshToken, currentTokenAfterReuse.Error);
    }
}
