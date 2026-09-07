using Company.ProjectName.Domain.Products;

namespace Company.ProjectName.Application.Products.Responses;

public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static ProductResponse FromProduct(Product product) => new(
        product.Id,
        product.Name,
        product.Price,
        product.CreatedAtUtc,
        product.UpdatedAtUtc);
}

public sealed record PagedResponse<TItem>(IReadOnlyList<TItem> Items, int Page, int PageSize, int TotalCount);
