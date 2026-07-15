using Helios.Application.Common.Models;
using Helios.Application.Features.ProductReviews.Requests;

namespace Helios.Application.Common.Interfaces;

public interface IProductReviewService
{
    Task<ApiResponse<Helios.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>> CreateProductReviewAsync(List<CreateProductReviewRequest> requests, CancellationToken cancellationToken = default);
}
