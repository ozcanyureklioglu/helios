using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Features.ProductReviews.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Endpoints.ProductReviews;

public class CreateProductReviewEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/product-reviews", async (List<CreateProductReviewRequest> requests, IProductReviewService productReviewService, CancellationToken cancellationToken) =>
        {
            var response = await productReviewService.CreateProductReviewAsync(requests, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("CreateProductReview")
        .WithTags("ProductReviews")
        .Produces<Application.Common.Models.ApiResponse<Vectomera.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<Vectomera.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>>(StatusCodes.Status400BadRequest);
    }
}

