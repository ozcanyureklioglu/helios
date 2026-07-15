namespace Vectomera.Application.Features.ProductReviews.Requests;

public class CreateProductReviewRequest
{
    public Guid ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Point { get; set; }
}

