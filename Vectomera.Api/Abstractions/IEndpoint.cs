using Microsoft.AspNetCore.Routing;

namespace Vectomera.Api.Abstractions;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

