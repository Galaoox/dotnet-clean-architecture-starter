using Company.ProjectName.Domain.Products;

namespace Company.ProjectName.Domain.UnitTests.Products;

public sealed class ProductTests
{
    [Fact]
    public void Create_with_valid_name_and_price_succeeds()
    {
        var result = Product.Create("Keyboard", 50m);

        Assert.True(result.IsSuccess);
        Assert.Equal("Keyboard", result.Value.Name);
        Assert.Equal(50m, result.Value.Price);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
    }

    [Fact]
    public void Create_generates_version7_identifier()
    {
        var result = Product.Create("Keyboard", 50m);

        // UUID version 7: version nibble (first hex digit of the third group) is 7.
        Assert.Equal('7', result.Value.Id.ToString("N")[12]);
    }

    [Fact]
    public void Create_with_blank_name_fails()
    {
        var result = Product.Create("   ", 50m);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameIsRequired, result.Error);
    }

    [Fact]
    public void Create_with_too_long_name_fails()
    {
        var result = Product.Create(new string('x', 201), 50m);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameTooLong, result.Error);
    }

    [Fact]
    public void Create_with_non_positive_price_fails()
    {
        var result = Product.Create("Keyboard", 0m);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.PriceMustBePositive, result.Error);
    }

    [Fact]
    public void Create_trims_surrounding_whitespace()
    {
        var result = Product.Create("  Keyboard  ", 50m);

        Assert.Equal("Keyboard", result.Value.Name);
    }

    [Fact]
    public void ChangePrice_with_positive_price_succeeds()
    {
        var product = Product.Create("Keyboard", 50m).Value;

        var result = product.ChangePrice(75m);

        Assert.True(result.IsSuccess);
        Assert.Equal(75m, product.Price);
    }

    [Fact]
    public void ChangePrice_returns_failure_when_price_is_not_positive()
    {
        var product = Product.Create("Keyboard", 50m).Value;

        var result = product.ChangePrice(0m);

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.PriceMustBePositive, result.Error);
    }

    [Fact]
    public void Rename_with_valid_name_succeeds()
    {
        var product = Product.Create("Keyboard", 50m).Value;

        var result = product.Rename("Mechanical Keyboard");

        Assert.True(result.IsSuccess);
        Assert.Equal("Mechanical Keyboard", product.Name);
    }

    [Fact]
    public void Rename_with_blank_name_fails()
    {
        var product = Product.Create("Keyboard", 50m).Value;

        var result = product.Rename("");

        Assert.True(result.IsFailure);
        Assert.Equal(ProductErrors.NameIsRequired, result.Error);
    }
}
