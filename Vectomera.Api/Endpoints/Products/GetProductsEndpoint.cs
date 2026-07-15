using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Features.Products.Dtos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Endpoints.Products;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/products", async ([FromQuery] string? searchText, IProductService productService, CancellationToken cancellationToken) =>
        {
            var response = await productService.GetProductsAsync(searchText, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<Application.Common.Models.ApiResponse<List<ProductDto>>>(StatusCodes.Status200OK);
    }
}

