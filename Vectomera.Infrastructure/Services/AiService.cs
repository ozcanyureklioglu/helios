using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.Ai.Dtos;
using Vectomera.Application.Features.Ai.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;

#pragma warning disable SKEXP0070 // Ollama is experimental

namespace Vectomera.Infrastructure.Services;

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
                return ApiResponse<AiAdviceResponse>.Fail("Soru boÅŸ olamaz.");

            var queryChunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(request.Query, cancellationToken: cancellationToken);
            var queryEmbedding = queryChunks.FirstOrDefault()?.Embedding;

            if (queryEmbedding == null)
                return ApiResponse<AiAdviceResponse>.Fail("Soru metni vektÃ¶re dÃ¶nÃ¼ÅŸtÃ¼rÃ¼lemedi.");

            var queryVector = new Vector(queryEmbedding);

            var topProductChunks = await _context.ProductVectorChunks
                .Include(x => x.Product)
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var topInventoryChunks = await _context.WarehouseInventoryVectorChunks
                .Include(x => x.WarehouseInventory)
                    .ThenInclude(wi => wi.Product)
                .Include(x => x.WarehouseInventory)
                    .ThenInclude(wi => wi.Warehouse)
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var topReviewChunks = await _context.ProductReviewVectorChunks
                .Include(x => x.ProductReview)
                    .ThenInclude(pr => pr.Product)
                .Include(x => x.ProductReview)
                    .ThenInclude(pr => pr.Warehouse)
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("--- ÃœrÃ¼n Bilgileri ---");
            foreach (var chunk in topProductChunks)
            {
                contextBuilder.AppendLine($"[ÃœrÃ¼n: {chunk.Product.Name} | Stok Kodu (SKU): {chunk.Product.Sku}]");
                contextBuilder.AppendLine($"AÃ§Ä±klama: {chunk.ChunkText}\n");
            }

            contextBuilder.AppendLine("--- Stok Bilgileri ---");
            foreach (var chunk in topInventoryChunks)
            {
                var wi = chunk.WarehouseInventory;
                contextBuilder.AppendLine($"[Depo: {wi.Warehouse.Name} ({wi.Warehouse.CityName}) | ÃœrÃ¼n: {wi.Product.Name} | SKU: {wi.Product.Sku}]");
                contextBuilder.AppendLine($"Stok/Fiyat DetayÄ±: {chunk.ChunkText}\n");
            }

            contextBuilder.AppendLine("--- ÃœrÃ¼n YorumlarÄ± ---");
            foreach (var chunk in topReviewChunks)
            {
                var pr = chunk.ProductReview;
                var warehouseInfo = pr.Warehouse != null ? $" | Depo: {pr.Warehouse.Name}" : "";
                contextBuilder.AppendLine($"[ÃœrÃ¼n: {pr.Product.Name} | SKU: {pr.Product.Sku}{warehouseInfo}]");
                contextBuilder.AppendLine($"Yorum DetayÄ±: {chunk.ChunkText}\n");
            }

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Sen Vectomera e-ticaret/depo sisteminin akÄ±llÄ± bir asistanÄ±sÄ±n. LÃ¼tfen kullanÄ±cÄ±nÄ±n sorusunu sadece aÅŸaÄŸÄ±da verilen 'BaÄŸlam (Context)' bilgilerini kullanarak cevapla.Not kullanÄ±cÄ±nÄ±n sorduÄŸu dile gÃ¶re cevap ver(Ä°ngilizce,TÃ¼rkÃ§e vb.)");

            var prompt = $"BaÄŸlam (Context):\n{contextBuilder}\n\nKullanÄ±cÄ±nÄ±n Sorusu: {request.Query}";
            chatHistory.AddUserMessage(prompt);

            var chatResult = await _chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: cancellationToken);

            var response = new AiAdviceResponse
            {
                Answer = chatResult.Content ?? "ÃœzgÃ¼nÃ¼m, bu soruya cevap Ã¼retemedim."
            };

            return ApiResponse<AiAdviceResponse>.Ok(response, "BaÅŸarÄ±lÄ±");
        }
        catch (Exception ex)
        {
            return ApiResponse<AiAdviceResponse>.Fail($"Bir hata oluÅŸtu: {ex.Message}");
        }
    }
}

