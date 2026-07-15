namespace Vectomera.Application.Features.ProductReviews.Dtos;

public class ProductReviewDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? WarehouseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Point { get; set; }
}
