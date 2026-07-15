using Vectomera.Domain.Common;

namespace Vectomera.Domain.Entities;

public class ProductReview : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Point { get; set; }

    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public ICollection<ProductReviewVectorChunk> VectorChunks { get; set; } = new List<ProductReviewVectorChunk>();
}

