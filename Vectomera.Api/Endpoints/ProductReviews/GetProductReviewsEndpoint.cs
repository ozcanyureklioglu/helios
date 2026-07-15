using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Endpoints.ProductReviews;

public class GetProductReviewsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/product-reviews", async ([FromQuery] Guid? productId, IProductReviewService productReviewService, CancellationToken cancellationToken) =>
        {
            var response = await productReviewService.GetProductReviewsAsync(productId, cancellationToken);
            return response.Success ? Results.Ok(response) : Results.BadRequest(response);
        })
        .WithName("GetProductReviews")
        .WithSummary("Gets a list of product reviews")
        .WithDescription("Gets product reviews with an optional product filter.")
        .WithTags("ProductReviews");
    }
}
