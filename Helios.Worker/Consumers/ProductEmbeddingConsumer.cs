using Helios.Application.Common.Events;
using Helios.Application.Common.Interfaces;
using Helios.Domain.Entities;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Helios.Worker.Consumers;

public class ProductEmbeddingConsumer : IConsumer<ProductEmbeddingEvent>
{
    private readonly ILogger<ProductEmbeddingConsumer> _logger;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChunkService _chunkService;

    public ProductEmbeddingConsumer(
        ILogger<ProductEmbeddingConsumer> logger,
        ITextEmbeddingService embeddingService,
        IChunkService chunkService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
        _chunkService = chunkService;
    }

    public async Task Consume(ConsumeContext<ProductEmbeddingEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Processing ProductEmbeddingEvent for ProductId: {ProductId}", message.ProductId);

        if (string.IsNullOrWhiteSpace(message.Description))
        {
            _logger.LogInformation("Description is empty. Removing existing vector chunks if any for ProductId: {ProductId}", message.ProductId);
            
            // If description is cleared, we should remove existing embeddings
            await _chunkService.DeleteProductVectorChunksAsync(message.ProductId, context.CancellationToken);
                
            return;
        }

        // Generate embeddings and chunks
        var chunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(message.Description, cancellationToken: context.CancellationToken);
        
        if (chunks == null || !chunks.Any())
        {
            _logger.LogWarning("No chunks generated for ProductId: {ProductId}", message.ProductId);
            return;
        }

        // Remove old chunks
        await _chunkService.DeleteProductVectorChunksAsync(message.ProductId, context.CancellationToken);

        // Map and save new chunks
        await _chunkService.CreateProductVectorChunksAsync(message.ProductId, chunks, context.CancellationToken);

        _logger.LogInformation("Successfully saved {Count} vector chunks for ProductId: {ProductId}", chunks.Count, message.ProductId);
    }
}
