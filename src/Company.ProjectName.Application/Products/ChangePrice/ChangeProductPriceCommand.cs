namespace Company.ProjectName.Application.Products.ChangePrice;

/// <summary>
/// Intent: change the price of an existing product.
/// Kept as a separate type from CRUD commands on purpose (Command Handler style).
/// </summary>
public sealed record ChangeProductPriceCommand(Guid ProductId, decimal NewPrice);

public sealed record ChangeProductPriceResult(Guid ProductId, decimal NewPrice, DateTimeOffset UpdatedAtUtc);
