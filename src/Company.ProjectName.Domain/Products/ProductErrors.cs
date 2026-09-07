using Company.ProjectName.Domain.Common;

namespace Company.ProjectName.Domain.Products;

public static class ProductErrors
{
    public static readonly Error NameIsRequired = Error.Validation(
        "Products.NameIsRequired",
        "Product name is required.");

    public static readonly Error NameTooLong = Error.Validation(
        "Products.NameTooLong",
        "Product name must not exceed 200 characters.");

    public static readonly Error PriceMustBePositive = Error.Validation(
        "Products.PriceMustBePositive",
        "Product price must be greater than zero.");

    public static readonly Error NotFound = Error.NotFound(
        "Products.NotFound",
        "Product was not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Products.NameAlreadyExists",
        "A product with the same name already exists.");
}
