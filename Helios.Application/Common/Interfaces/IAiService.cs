using Helios.Application.Common.Models;
using Helios.Application.Features.Ai.Dtos;
using Helios.Application.Features.Ai.Requests;

namespace Helios.Application.Common.Interfaces;

public interface IAiService
{
    Task<ApiResponse<AiAdviceResponse>> GetAdviceAsync(AiAdviceRequest request, CancellationToken cancellationToken = default);
}
