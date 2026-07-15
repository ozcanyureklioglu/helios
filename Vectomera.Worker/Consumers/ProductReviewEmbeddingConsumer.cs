using Vectomera.Application.Common.Events;
using Vectomera.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Vectomera.Worker.Consumers;

public class ProductReviewEmbeddingConsumer : IConsumer<CreateProductReviewEvent>
{
    private readonly ILogger<ProductReviewEmbeddingConsumer> _logger;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChunkService _chunkService;

    public ProductReviewEmbeddingConsumer(
        ILogger<ProductReviewEmbeddingConsumer> logger,
        ITextEmbeddingService embeddingService,
        IChunkService chunkService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
        _chunkService = chunkService;
    }

    public async Task Consume(ConsumeContext<CreateProductReviewEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Processing CreateProductReviewEvent for ReviewId: {ReviewId}", message.ReviewId);

        var semanticText = $"Puan: {message.Point}/5. BaÅŸlÄ±k: {message.Title}. Yorum: {message.Description}";

        // Generate embeddings and chunks
        var chunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(semanticText, cancellationToken: context.CancellationToken);
        
        if (chunks == null || !chunks.Any())
        {
            _logger.LogWarning("No chunks generated for ReviewId: {ReviewId}", message.ReviewId);
            return;
        }

        // Remove old chunks if any exist for this review (e.g. if this is used as an update later)
        await _chunkService.DeleteProductReviewVectorChunksAsync(message.ReviewId, context.CancellationToken);

        // Map and save new chunks
        await _chunkService.CreateProductReviewVectorChunksAsync(message.ReviewId, chunks, context.CancellationToken);

        _logger.LogInformation("Successfully saved {Count} vector chunks for ReviewId: {ReviewId}", chunks.Count, message.ReviewId);
    }
}

