using Company.ProjectName.Application.Products;
using Company.ProjectName.Application.Products.ChangePrice;
using Company.ProjectName.Domain.Products;
using NSubstitute;

namespace Company.ProjectName.Application.UnitTests.Products;

public sealed class ChangeProductPriceHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly ChangeProductPriceHandler _sut;

    public ChangeProductPriceHandlerTests()
    {
        _sut = new ChangeProductPriceHandler(_repository);
    }

    [Fact]
    public async Task Handle_when_product_is_missing_returns_not_found()
    {
        var id = Guid.CreateVersion7();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Product?)null);

        var result = await _sut.HandleAsync(
            new ChangeProductPriceCommand(id, 75m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NotFound, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_invalid_price_returns_domain_error()
    {
        var product = Product.Create("Keyboard", 50m).Value;
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.HandleAsync(
            new ChangeProductPriceCommand(product.Id, 0m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.PriceMustBePositive, result.Error);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_with_valid_price_updates_and_persists_product()
    {
        var product = Product.Create("Keyboard", 50m).Value;
        _repository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        var result = await _sut.HandleAsync(
            new ChangeProductPriceCommand(product.Id, 75m),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(product.Id, result.Value.ProductId);
        Assert.Equal(75m, result.Value.NewPrice);
        await _repository.Received(1).UpdateAsync(product, Arg.Any<CancellationToken>());
    }
}
