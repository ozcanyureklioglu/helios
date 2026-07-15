using Helios.Application.Common.Events;
using Helios.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Helios.Worker.Consumers;

public class WarehouseInventoryEmbeddingConsumer : IConsumer<CreateWarehouseInventoryEvent>
{
    private readonly ILogger<WarehouseInventoryEmbeddingConsumer> _logger;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChunkService _chunkService;

    public WarehouseInventoryEmbeddingConsumer(
        ILogger<WarehouseInventoryEmbeddingConsumer> logger,
        ITextEmbeddingService embeddingService,
        IChunkService chunkService)
    {
        _logger = logger;
        _embeddingService = embeddingService;
        _chunkService = chunkService;
    }

    public async Task Consume(ConsumeContext<CreateWarehouseInventoryEvent> context)
    {
        var message = context.Message;
        
        _logger.LogInformation("Processing CreateWarehouseInventoryEvent for InventoryId: {InventoryId}", message.InventoryId);

        if (string.IsNullOrWhiteSpace(message.Description))
        {
            _logger.LogInformation("Description is empty. Removing existing vector chunks if any for InventoryId: {InventoryId}", message.InventoryId);
            
            // If description is cleared, we should remove existing embeddings
            await _chunkService.DeleteWarehouseInventoryVectorChunksAsync(message.InventoryId, context.CancellationToken);
                
            return;
        }

        // Generate embeddings and chunks
        var chunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(message.Description, cancellationToken: context.CancellationToken);
        
        if (chunks == null || !chunks.Any())
        {
            _logger.LogWarning("No chunks generated for InventoryId: {InventoryId}", message.InventoryId);
            return;
        }

        // Remove old chunks
        await _chunkService.DeleteWarehouseInventoryVectorChunksAsync(message.InventoryId, context.CancellationToken);

        // Map and save new chunks
        await _chunkService.CreateWarehouseInventoryVectorChunksAsync(message.InventoryId, chunks, context.CancellationToken);

        _logger.LogInformation("Successfully saved {Count} vector chunks for InventoryId: {InventoryId}", chunks.Count, message.InventoryId);
    }
}
