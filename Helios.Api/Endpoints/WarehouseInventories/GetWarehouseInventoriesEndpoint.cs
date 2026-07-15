using Helios.Api.Abstractions;
using Helios.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;

namespace Helios.Api.Endpoints.WarehouseInventories;

public class GetWarehouseInventoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/warehouse-inventories", async ([FromQuery] Guid? productId, IWarehouseInventoryService warehouseInventoryService, CancellationToken cancellationToken) =>
        {
            var response = await warehouseInventoryService.GetWarehouseInventoriesAsync(productId, cancellationToken);
            return Results.Ok(response);
        })
        .WithName("GetWarehouseInventories")
        .WithTags("WarehouseInventories")
        .Produces<Application.Common.Models.ApiResponse<List<Helios.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto>>>(StatusCodes.Status200OK);
    }
}
