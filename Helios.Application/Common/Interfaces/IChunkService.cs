using Helios.Application.Common.Models;

namespace Helios.Application.Common.Interfaces;

public interface IChunkService
{
    Task DeleteProductVectorChunksAsync(Guid productId, CancellationToken cancellationToken = default);
    Task CreateProductVectorChunksAsync(Guid productId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default);
    
    Task DeleteWarehouseInventoryVectorChunksAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    Task CreateWarehouseInventoryVectorChunksAsync(Guid inventoryId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default);

    Task DeleteProductReviewVectorChunksAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task CreateProductReviewVectorChunksAsync(Guid reviewId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default);
}
