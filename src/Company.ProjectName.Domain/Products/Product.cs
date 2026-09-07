using Company.ProjectName.Domain.Common;

namespace Company.ProjectName.Domain.Products;

/// <summary>
/// Product aggregate root. Invariants are enforced inside the entity;
/// callers must go through the factory and behavior methods.
/// </summary>
public sealed class Product
{
    private const int NameMaxLength = 200;

    private Product(Guid id, string name, decimal price, DateTimeOffset createdAtUtc, DateTimeOffset updatedAtUtc)
    {
        Id = id;
        Name = name;
        Price = price;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Creates a product applying the aggregate invariants.
    /// The identifier is a UUID version 7 (time-ordered, database-friendly).
    /// </summary>
    public static Result<Product> Create(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Product>(ProductErrors.NameIsRequired);
        }

        if (name.Length > NameMaxLength)
        {
            return Result.Failure<Product>(ProductErrors.NameTooLong);
        }

        if (price <= 0)
        {
            return Result.Failure<Product>(ProductErrors.PriceMustBePositive);
        }

        var now = DateTimeOffset.UtcNow;

        return new Product(
            Guid.CreateVersion7(),
            name.Trim(),
            price,
            now,
            now);
    }

    public Result ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
        {
            return Result.Failure(ProductErrors.PriceMustBePositive);
        }

        Price = newPrice;
        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(ProductErrors.NameIsRequired);
        }

        if (newName.Length > NameMaxLength)
        {
            return Result.Failure(ProductErrors.NameTooLong);
        }

        Name = newName.Trim();
        UpdatedAtUtc = DateTimeOffset.UtcNow;

        return Result.Success();
    }
}
