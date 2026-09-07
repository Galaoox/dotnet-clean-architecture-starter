using Company.ProjectName.Domain.Products;
using Company.ProjectName.Infrastructure;
using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Company.ProjectName.IntegrationTests.Infrastructure;

public sealed class ProductRepositoryTests : IAsyncLifetime
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
        if (_services is not null)
        {
            await _services.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task Add_and_get_by_id_persists_product_in_postgresql()
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<Company.ProjectName.Application.Products.IProductRepository>();
        var product = Product.Create("Keyboard", 50m).Value;

        await repository.AddAsync(product, TestContext.Current.CancellationToken);
        var result = await repository.GetByIdAsync(product.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result.Id);
        Assert.Equal("Keyboard", result.Name);
        Assert.Equal(50m, result.Price);
    }

    [Fact]
    public async Task List_returns_products_sorted_by_name_with_total_count()
    {
        await using var scope = _services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<Company.ProjectName.Application.Products.IProductRepository>();
        await repository.AddAsync(Product.Create("Mouse", 30m).Value, TestContext.Current.CancellationToken);
        await repository.AddAsync(Product.Create("Keyboard", 50m).Value, TestContext.Current.CancellationToken);

        var (items, totalCount) = await repository.ListAsync(1, 20, TestContext.Current.CancellationToken);

        Assert.Equal(2, totalCount);
        Assert.Equal(["Keyboard", "Mouse"], items.Select(product => product.Name).ToArray());
    }
}
