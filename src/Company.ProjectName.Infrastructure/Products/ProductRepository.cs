using Company.ProjectName.Application.Products;
using Company.ProjectName.Domain.Products;
using Company.ProjectName.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Company.ProjectName.Infrastructure.Products;

internal sealed class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        => dbContext.Products.SingleOrDefaultAsync(product => product.Name == name, cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        => dbContext.Products.AnyAsync(product => product.Name == name, cancellationToken);

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Update(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Products.AsNoTracking().OrderBy(product => product.Name);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
