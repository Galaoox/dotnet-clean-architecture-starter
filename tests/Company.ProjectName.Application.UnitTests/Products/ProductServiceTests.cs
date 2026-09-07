using Company.ProjectName.Application.Products;
using Company.ProjectName.Application.Products.Commands;
using Company.ProjectName.Application.Products.Queries;
using Company.ProjectName.Domain.Products;
using NSubstitute;

namespace Company.ProjectName.Application.UnitTests.Products;

public sealed class ProductServiceTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _sut = new ProductService(_repository);
    }

    [Fact]
    public async Task Create_with_duplicate_name_returns_conflict()
    {
        _repository.ExistsByNameAsync("Keyboard", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _sut.CreateAsync(new CreateProductCommand("Keyboard", 50m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameAlreadyExists, result.Error);
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_with_invalid_domain_input_returns_validation_error()
    {
        _repository.ExistsByNameAsync("", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync(new CreateProductCommand("", 50m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameIsRequired, result.Error);
    }

    [Fact]
    public async Task Create_with_valid_command_persists_product()
    {
        _repository.ExistsByNameAsync("Keyboard", Arg.Any<CancellationToken>()).Returns(false);

        var result = await _sut.CreateAsync(new CreateProductCommand("Keyboard", 50m), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal("Keyboard", result.Value.Name);
        await _repository.Received(1).AddAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_when_missing_returns_not_found()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task GetById_when_present_returns_response()
    {
        var product = Product.Create("Keyboard", 50m).Value;
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.GetByIdAsync(product.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(product.Id, result.Value.Id);
    }

    [Fact]
    public async Task Update_when_missing_returns_not_found()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _sut.UpdateAsync(new UpdateProductCommand(Guid.NewGuid(), "New name", 10m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Update_with_name_owned_by_other_product_returns_conflict()
    {
        var product = Product.Create("Keyboard", 50m).Value;
        var other = Product.Create("Mouse", 30m).Value;

        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetByNameAsync("Mouse", Arg.Any<CancellationToken>()).Returns(other);

        var result = await _sut.UpdateAsync(new UpdateProductCommand(product.Id, "Mouse", 30m), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameAlreadyExists, result.Error);
    }

    [Fact]
    public async Task Update_with_same_name_succeeds()
    {
        var product = Product.Create("Keyboard", 50m).Value;

        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetByNameAsync("Keyboard", Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.UpdateAsync(new UpdateProductCommand(product.Id, "Keyboard", 60m), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(60m, result.Value.Price);
        await _repository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_when_missing_returns_not_found()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _sut.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_when_present_removes_product()
    {
        var product = Product.Create("Keyboard", 50m).Value;
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.DeleteAsync(product.Id, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        await _repository.Received(1).DeleteAsync(product, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task List_clamps_page_and_page_size()
    {
        IReadOnlyList<Product> products = new[] { Product.Create("Keyboard", 50m).Value };
        _repository.ListAsync(1, 100, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(((IReadOnlyList<Product>)products, 1)));

        var result = await _sut.ListAsync(new ListProductsQuery(Page: 0, PageSize: 500), TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(100, result.Value.PageSize);
    }
}
