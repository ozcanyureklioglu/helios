using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace Helios.Infrastructure.Services;

public class ChunkService : IChunkService
{
    private readonly IAppDbContext _context;

    public ChunkService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task DeleteProductVectorChunksAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        await _context.ProductVectorChunks
            .Where(x => x.ProductId == productId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CreateProductVectorChunksAsync(Guid productId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default)
    {
        var vectorChunks = chunks.Select(chunk => new ProductVectorChunk
        {
            ProductId = productId,
            ChunkText = chunk.ChunkText,
            Embedding = new Vector(chunk.Embedding),
            ChunkIndex = chunk.ChunkIndex,
            TokenCount = chunk.TokenCount
        }).ToList();

        _context.ProductVectorChunks.AddRange(vectorChunks);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteWarehouseInventoryVectorChunksAsync(Guid inventoryId, CancellationToken cancellationToken = default)
    {
        await _context.WarehouseInventoryVectorChunks
            .Where(x => x.WarehouseInventoryId == inventoryId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CreateWarehouseInventoryVectorChunksAsync(Guid inventoryId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default)
    {
        var vectorChunks = chunks.Select(chunk => new WarehouseInventoryVectorChunk
        {
            WarehouseInventoryId = inventoryId,
            ChunkText = chunk.ChunkText,
            Embedding = new Vector(chunk.Embedding),
            ChunkIndex = chunk.ChunkIndex,
            TokenCount = chunk.TokenCount
        }).ToList();

        _context.WarehouseInventoryVectorChunks.AddRange(vectorChunks);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteProductReviewVectorChunksAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        await _context.ProductReviewVectorChunks
            .Where(x => x.ProductReviewId == reviewId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task CreateProductReviewVectorChunksAsync(Guid reviewId, List<VectorChunkDto> chunks, CancellationToken cancellationToken = default)
    {
        var vectorChunks = chunks.Select(chunk => new ProductReviewVectorChunk
        {
            ProductReviewId = reviewId,
            ChunkText = chunk.ChunkText,
            Embedding = new Vector(chunk.Embedding),
            ChunkIndex = chunk.ChunkIndex,
            TokenCount = chunk.TokenCount
        }).ToList();

        _context.ProductReviewVectorChunks.AddRange(vectorChunks);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
