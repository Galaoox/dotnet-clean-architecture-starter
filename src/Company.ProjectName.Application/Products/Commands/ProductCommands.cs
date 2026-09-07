namespace Company.ProjectName.Application.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price);

public sealed record UpdateProductCommand(Guid Id, string Name, decimal Price);
