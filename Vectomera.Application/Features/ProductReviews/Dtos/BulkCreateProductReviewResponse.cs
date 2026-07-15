namespace Vectomera.Application.Features.ProductReviews.Dtos;

public class ProductReviewErrorDto
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class BulkCreateProductReviewResponse
{
    public List<Guid> SuccessfulReviewIds { get; set; } = new List<Guid>();
    public List<ProductReviewErrorDto> Errors { get; set; } = new List<ProductReviewErrorDto>();
}

