using System.Text.Json.Serialization;

namespace Vectomera.Application.Features.Ai.Dtos;

public class AiIntentExtractionResponse
{
    [JsonPropertyName("categoryId")]
    public Guid? CategoryId { get; set; }

    [JsonPropertyName("vectorSearchText")]
    public string VectorSearchText { get; set; } = string.Empty;
}
