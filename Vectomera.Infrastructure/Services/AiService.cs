using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.Ai.Dtos;
using Vectomera.Application.Features.Ai.Requests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OllamaSharp;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;
using System.Text.Json;


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
                return ApiResponse<AiAdviceResponse>.Fail("Soru boş olamaz.");

            // 1. Intent Extraction Step (Geçici olarak devre dışı bırakıldı)
            /*
            var categoriesList = await _context.Categories
                .Select(c => new { c.Id, c.Name })
                .ToListAsync(cancellationToken);
            var categoriesJson = JsonSerializer.Serialize(categoriesList);

            var intentChatHistory = new ChatHistory();
            intentChatHistory.AddSystemMessage($@"You are a helpful assistant that extracts category intent from a user's search query.
You must return a valid JSON object with EXACTLY two properties:
- 'categoryId': The Guid of the matching category if found in the catalog list, otherwise null.
- 'vectorSearchText': The remaining part of the user's query after removing the category name. If no category matches, this should be the original query.

Available Categories:
{categoriesJson}");
            intentChatHistory.AddUserMessage(request.Query);

            var executionSettings = new OllamaPromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object> { { "format", "json" } }
            };
            
            var intentResult = await _chatCompletionService.GetChatMessageContentAsync(intentChatHistory, executionSettings, cancellationToken: cancellationToken);
            
            AiIntentExtractionResponse? intentResponse = null;
            try
            {
                var responseContent = intentResult.Content ?? string.Empty;
                if (responseContent.StartsWith("```json"))
                {
                    responseContent = responseContent.Substring(7);
                    if (responseContent.EndsWith("```"))
                        responseContent = responseContent.Substring(0, responseContent.Length - 3);
                }
                else if (responseContent.StartsWith("```"))
                {
                    responseContent = responseContent.Substring(3);
                    if (responseContent.EndsWith("```"))
                        responseContent = responseContent.Substring(0, responseContent.Length - 3);
                }
                
                intentResponse = JsonSerializer.Deserialize<AiIntentExtractionResponse>(responseContent.Trim(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                // Fallback if parsing fails
            }

            var categoryId = intentResponse?.CategoryId;
            var vectorSearchText = !string.IsNullOrWhiteSpace(intentResponse?.VectorSearchText) 
                                        ? intentResponse.VectorSearchText 
                                        : request.Query;
            */
            
            Guid? categoryId = null;
            var vectorSearchText = request.Query;

            // 2. Vector Search Step
            var queryChunks = await _embeddingService.GenerateChunksAndEmbeddingsAsync(vectorSearchText, cancellationToken: cancellationToken);
            var queryEmbedding = queryChunks.FirstOrDefault()?.Embedding;

            if (queryEmbedding == null)
                return ApiResponse<AiAdviceResponse>.Fail("Soru metni vektöre dönüştürülemedi.");

            var queryVector = new Vector(queryEmbedding);

            var productQuery = _context.ProductVectorChunks
                .Include(x => x.Product)
                .AsQueryable();
            if (categoryId.HasValue)
                productQuery = productQuery.Where(x => x.Product.CategoryId == categoryId.Value);

            var topProductChunks = await productQuery
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var inventoryQuery = _context.WarehouseInventoryVectorChunks
                .Include(x => x.WarehouseInventory)
                    .ThenInclude(wi => wi.Product)
                .Include(x => x.WarehouseInventory)
                    .ThenInclude(wi => wi.Warehouse)
                .AsQueryable();
            if (categoryId.HasValue)
                inventoryQuery = inventoryQuery.Where(x => x.WarehouseInventory.Product.CategoryId == categoryId.Value);

            var topInventoryChunks = await inventoryQuery
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var reviewQuery = _context.ProductReviewVectorChunks
                .Include(x => x.ProductReview)
                    .ThenInclude(pr => pr.Product)
                .Include(x => x.ProductReview)
                    .ThenInclude(pr => pr.Warehouse)
                .AsQueryable();
            if (categoryId.HasValue)
                reviewQuery = reviewQuery.Where(x => x.ProductReview.Product.CategoryId == categoryId.Value);

            var topReviewChunks = await reviewQuery
                .OrderBy(x => x.Embedding!.L2Distance(queryVector))
                .Take(3)
                .ToListAsync(cancellationToken);

            var contextBuilder = new StringBuilder();
            contextBuilder.AppendLine("--- Ürün Bilgileri ---");
            foreach (var chunk in topProductChunks)
            {
                contextBuilder.AppendLine($"[Ürün: {chunk.Product.Name} | Stok Kodu (SKU): {chunk.Product.Sku}]");
                contextBuilder.AppendLine($"Açıklama: {chunk.ChunkText}\n");
            }

            contextBuilder.AppendLine("--- Stok Bilgileri ---");
            foreach (var chunk in topInventoryChunks)
            {
                var wi = chunk.WarehouseInventory;
                contextBuilder.AppendLine($"[Depo: {wi.Warehouse.Name} ({wi.Warehouse.CityName}) | Ürün: {wi.Product.Name} | SKU: {wi.Product.Sku}]");
                contextBuilder.AppendLine($"Stok/Fiyat Detayı: {chunk.ChunkText}\n");
            }

            contextBuilder.AppendLine("--- Ürün Yorumları ---");
            foreach (var chunk in topReviewChunks)
            {
                var pr = chunk.ProductReview;
                var warehouseInfo = pr.Warehouse != null ? $" | Depo: {pr.Warehouse.Name}" : "";
                contextBuilder.AppendLine($"[Ürün: {pr.Product.Name} | SKU: {pr.Product.Sku}{warehouseInfo}]");
                contextBuilder.AppendLine($"Yorum Detayı: {chunk.ChunkText}\n");
            }

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage("Sen Vectomera e-ticaret/depo sisteminin akıllı bir asistanısın. Lütfen kullanıcının sorusunu sadece aşağıda verilen 'Bağlam (Context)' bilgilerini kullanarak cevapla.Not kullanıcı türkçe sorarsa türkçe cevap ver, ingilizce sorarsa ingilizce cevap ver. ");

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

