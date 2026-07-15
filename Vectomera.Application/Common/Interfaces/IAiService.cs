using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.Ai.Dtos;
using Vectomera.Application.Features.Ai.Requests;

namespace Vectomera.Application.Common.Interfaces;

public interface IAiService
{
    Task<ApiResponse<AiAdviceResponse>> GetAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default);
}

