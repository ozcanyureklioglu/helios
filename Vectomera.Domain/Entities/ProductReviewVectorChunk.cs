using Vectomera.Domain.Common;
using Pgvector;

namespace Vectomera.Domain.Entities;

public class ProductReviewVectorChunk : BaseEntity
{
    public Guid ProductReviewId { get; set; }
    public ProductReview ProductReview { get; set; } = null!;

    public string ChunkText { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public int ChunkIndex { get; set; }
    public int? TokenCount { get; set; }
}

