using Company.ProjectName.Application.Products.Commands;
using Company.ProjectName.Application.Products.Queries;
using Company.ProjectName.Application.Products.Responses;
using Company.ProjectName.Domain.Common;
using Company.ProjectName.Domain.Products;

namespace Company.ProjectName.Application.Products;

/// <summary>
/// Application service grouping the product CRUD use cases.
/// Depends only on the domain and the persistence port.
/// </summary>
public sealed class ProductService(IProductRepository repository)
{
    private const int MaxPageSize = 100;

    public async Task<Result<ProductResponse>> CreateAsync(
        CreateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsByNameAsync(command.Name, cancellationToken))
        {
            return Result.Failure<ProductResponse>(ProductErrors.NameAlreadyExists);
        }

        var productResult = Product.Create(command.Name, command.Price);

        if (productResult.IsFailure)
        {
            return Result.Failure<ProductResponse>(productResult.Error);
        }

        await repository.AddAsync(productResult.Value, cancellationToken);

        return Result.Success(ProductResponse.FromProduct(productResult.Value));
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        return product is null
            ? Result.Failure<ProductResponse>(ProductErrors.NotFound)
            : Result.Success(ProductResponse.FromProduct(product));
    }

    public async Task<Result<PagedResponse<ProductResponse>>> ListAsync(
        ListProductsQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var (items, totalCount) = await repository.ListAsync(page, pageSize, cancellationToken);

        var responses = items.Select(ProductResponse.FromProduct).ToList();

        return Result.Success(new PagedResponse<ProductResponse>(responses, page, pageSize, totalCount));
    }

    public async Task<Result<ProductResponse>> UpdateAsync(
        UpdateProductCommand command,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);
        }

        var conflict = await repository.GetByNameAsync(command.Name, cancellationToken);

        if (conflict is not null && conflict.Id != product.Id)
        {
            return Result.Failure<ProductResponse>(ProductErrors.NameAlreadyExists);
        }

        var renameResult = product.Rename(command.Name);

        if (renameResult.IsFailure)
        {
            return Result.Failure<ProductResponse>(renameResult.Error);
        }

        var priceResult = product.ChangePrice(command.Price);

        if (priceResult.IsFailure)
        {
            return Result.Failure<ProductResponse>(priceResult.Error);
        }

        await repository.UpdateAsync(product, cancellationToken);

        return Result.Success(ProductResponse.FromProduct(product));
    }

    public async Task<Result> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(ProductErrors.NotFound);
        }

        await repository.DeleteAsync(product, cancellationToken);

        return Result.Success();
    }
}
