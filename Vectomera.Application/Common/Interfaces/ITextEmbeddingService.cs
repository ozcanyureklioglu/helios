using Vectomera.Application.Common.Models;

namespace Vectomera.Application.Common.Interfaces;

public interface ITextEmbeddingService
{
    Task<List<VectorChunkDto>> GenerateChunksAndEmbeddingsAsync(string text, int maxTokensPerChunk = 512, CancellationToken cancellationToken = default);
}

