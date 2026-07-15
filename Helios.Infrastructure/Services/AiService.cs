using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Application.Features.Ai.Dtos;
using Helios.Application.Features.Ai.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;

#pragma warning disable SKEXP0070 // Ollama is experimental

namespace Helios.Infrastructure.Services;

public class AiService : IAiService
{
    private readonly IAppDbContext _context;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly IChatCompletionService _chatCompletionService;

    public AiService(
        IAppDbContext context,
        ITextEmbeddingService embeddingService,
        IConfiguration configuration)
    {
        _context = context;
        _embeddingService = embeddingService;

        var endpoint = configuration["OllamaOptions:Endpoint"] ?? "http://localhost:11434";
        var chatModelId = configuration["OllamaOptions:ChatModel"] ?? "gemma3:4b";

        #pragma warning disable CS0618
        var ollamaClient = new OllamaApiClient(endpoint, chatModelId);
        _chatCompletionService = ollamaClient.AsChatCompletionService();
        #pragma warning restore CS0618
    }

    public async Task<ApiResponse<AiAdviceResponse>> GetAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
                return ApiResponse<AiAdviceResponse>.Fail("Soru boş olamaz.");

            var queryChunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(request.Query, cancellationToken: cancellationToken);
            var queryEmbedding = queryChunks.FirstOrDefault()?.Embedding;

            if (queryEmbedding == null)
                return ApiResponse<AiAdviceResponse>.Fail("Soru metni vektöre dönüştürülemedi.");

            var queryVector = new Vector(queryEmbedding);

            var topProductChunks = await _context.ProductVectorChunks
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Select(x => x.ChunkText)
                .Take(3)
                .ToListAsync(cancellationToken);

            var topInventoryChunks = await _context.WarehouseInventoryVectorChunks
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Select(x => x.ChunkText)
                .Take(3)
                .ToListAsync(cancellationToken);

            var topReviewChunks = await _context.ProductReviewVectorChunks
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Select(x => x.ChunkText)
                .Take(3)
                .ToListAsync(cancellationToken);

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("--- Ürün Bilgileri ---");
            foreach (var chunk in topProductChunks) contextBuilder.AppendLine(chunk);

            contextBuilder.AppendLine("\n--- Stok Bilgileri ---");
            foreach (var chunk in topInventoryChunks) contextBuilder.AppendLine(chunk);

            contextBuilder.AppendLine("\n--- Ürün Yorumları ---");
            foreach (var chunk in topReviewChunks) contextBuilder.AppendLine(chunk);

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Sen Helios e-ticaret/depo sisteminin akıllı bir asistanısın. Lütfen kullanıcının sorusunu sadece aşağıda verilen 'Bağlam (Context)' bilgilerini kullanarak cevapla.");

            var prompt = $"Bağlam (Context):\n{contextBuilder}\n\nKullanıcının Sorusu: {request.Query}";
            chatHistory.AddUserMessage(prompt);

            var chatResult = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var response = new AiAdviceResponse
            {
                Answer = chatResult.Content ?? "Üzgünüm, bu soruya cevap üretemedim."
            };

            return ApiResponse<AiAdviceResponse>.Ok(response, "Başarılı");
        }
        catch (Exception ex)
        {
            return ApiResponse<AiAdviceResponse>.Fail($"Bir hata oluştu: {ex.Message}");
        }
    }
}
