using Company.ProjectName.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Company.ProjectName.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ProductService>();
        services.AddScoped<Products.ChangePrice.ChangeProductPriceHandler>();

        return services;
    }
}
