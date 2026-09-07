using Company.ProjectName.Api.Common.Errors;
using Company.ProjectName.Application.Products;
using Company.ProjectName.Application.Products.ChangePrice;
using Company.ProjectName.Application.Products.Commands;
using Company.ProjectName.Application.Products.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Company.ProjectName.Api.Features.Products;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(
    ProductService productService,
    ChangeProductPriceHandler changeProductPriceHandler) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet]
    public async Task<ActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await productService.ListAsync(
            new ListProductsQuery(page, pageSize),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = "Products.Write")]
    public async Task<ActionResult> Create(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productService.CreateAsync(
            new CreateProductCommand(request.Name, request.Price),
            cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return this.ToActionResult(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Products.Write")]
    public async Task<ActionResult> Update(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productService.UpdateAsync(
            new UpdateProductCommand(id, request.Name, request.Price),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Products.Write")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await productService.DeleteAsync(id, cancellationToken);
        return this.ToActionResult(result);
    }

    /// <summary>
    /// Direct Command Handler example. This intentionally bypasses ProductService
    /// to demonstrate one use case per handler without MediatR.
    /// </summary>
    [HttpPatch("{id:guid}/price")]
    [Authorize(Policy = "Products.Write")]
    public async Task<ActionResult> ChangePrice(
        Guid id,
        ChangeProductPriceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await changeProductPriceHandler.HandleAsync(
            new ChangeProductPriceCommand(id, request.NewPrice),
            cancellationToken);

        return this.ToActionResult(result);
    }
}

public sealed record CreateProductRequest(string Name, decimal Price);

public sealed record UpdateProductRequest(string Name, decimal Price);

public sealed record ChangeProductPriceRequest(decimal NewPrice);
