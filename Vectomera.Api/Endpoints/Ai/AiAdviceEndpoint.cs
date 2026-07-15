using Vectomera.Api.Abstractions;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Features.Ai.Dtos;
using Vectomera.Application.Features.Ai.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Endpoints.Ai;

public class AiAdviceEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/ai/advice", async (AiAdviceRequest request, IAiService aiService, CancellationToken cancellationToken) =>
        {
            var response = await aiService.GetAdviceAsync(request, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("AiAdvice")
        .WithTags("AI")
        .Produces<Application.Common.Models.ApiResponse<AiAdviceResponse>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<AiAdviceResponse>>(StatusCodes.Status400BadRequest);
    }
}

