using Company.ProjectName.Domain.Common;
using Company.ProjectName.Domain.Products;

namespace Company.ProjectName.Application.Products.ChangePrice;

/// <summary>
/// Command Handler reference implementation: one class, one use case.
/// Contrasts with the grouped <see cref="ProductService"/> CRUD operations.
/// No MediatR: the controller injects and calls this handler directly.
/// </summary>
public sealed class ChangeProductPriceHandler(IProductRepository repository)
{
    public async Task<Result<ChangeProductPriceResult>> HandleAsync(
        ChangeProductPriceCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(command.ProductId, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ChangeProductPriceResult>(ProductErrors.NotFound);
        }

        var changeResult = product.ChangePrice(command.NewPrice);

        if (changeResult.IsFailure)
        {
            return Result.Failure<ChangeProductPriceResult>(changeResult.Error);
        }

        await repository.UpdateAsync(product, cancellationToken);

        return Result.Success(new ChangeProductPriceResult(product.Id, product.Price, product.UpdatedAtUtc));
    }
}
