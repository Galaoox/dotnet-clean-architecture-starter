using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using Company.ProjectName.Api.Features.Products;
using Company.ProjectName.Application.Products;
using Company.ProjectName.Domain.Products;
using Company.ProjectName.Infrastructure.Persistence;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Company.ProjectName.ArchitectureTests;

public sealed class DependencyRulesTests
{
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            typeof(ProductsController).Assembly,
            typeof(ProductService).Assembly,
            typeof(Product).Assembly,
            typeof(ApplicationDbContext).Assembly)
        .Build();

    [Fact]
    public void Domain_must_not_depend_on_application_infrastructure_or_api()
    {
        var domain = Types().That().ResideInAssembly(typeof(Product).Assembly);

        domain.Should().NotDependOnAny(Types().That().ResideInAssembly(typeof(ProductService).Assembly));
        domain.Should().NotDependOnAny(Types().That().ResideInAssembly(typeof(ApplicationDbContext).Assembly));
        domain.Should().NotDependOnAny(Types().That().ResideInAssembly(typeof(ProductsController).Assembly));
    }

    [Fact]
    public void Application_must_not_depend_on_infrastructure_or_api()
    {
        var application = Types().That().ResideInAssembly(typeof(ProductService).Assembly);

        application.Should().NotDependOnAny(Types().That().ResideInAssembly(typeof(ApplicationDbContext).Assembly));
        application.Should().NotDependOnAny(Types().That().ResideInAssembly(typeof(ProductsController).Assembly));
    }

    [Fact]
    public void Infrastructure_must_not_depend_on_api()
    {
        var infrastructure = Types().That().ResideInAssembly(typeof(ApplicationDbContext).Assembly);
        var api = Types().That().ResideInAssembly(typeof(ProductsController).Assembly);

        infrastructure.Should().NotDependOnAny(api);
    }

    [Fact]
    public void Product_controllers_must_live_in_api()
    {
        Classes().That().HaveNameEndingWith("Controller")
            .Should().ResideInNamespace("Company.ProjectName.Api.Features.Products");
    }
}
