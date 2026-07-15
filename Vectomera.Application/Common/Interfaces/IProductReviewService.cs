using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.ProductReviews.Requests;

namespace Vectomera.Application.Common.Interfaces;

public interface IProductReviewService
{
    Task<ApiResponse<Vectomera.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>> CreateProductReviewAsync(List<CreateProductReviewRequest> requests, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<Vectomera.Application.Features.ProductReviews.Dtos.ProductReviewDto>>> GetProductReviewsAsync(Guid? productId, CancellationToken cancellationToken = default);
}

