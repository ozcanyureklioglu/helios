using Pgvector;
using Vectomera.Domain.Common;

namespace Vectomera.Domain.Entities;

public class ProductVectorChunk : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string ChunkText { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
    public int ChunkIndex { get; set; }
    public int? TokenCount { get; set; }
}

