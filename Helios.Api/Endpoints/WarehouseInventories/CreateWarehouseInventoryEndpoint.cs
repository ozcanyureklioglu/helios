using Helios.Api.Abstractions;
using Helios.Application.Common.Interfaces;
using Helios.Application.Features.WarehouseInventories.Requests;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Helios.Api.Endpoints.WarehouseInventories;

public class CreateWarehouseInventoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/warehouse-inventories", async (List<CreateWarehouseInventoryRequest> requests, IWarehouseInventoryService warehouseInventoryService, CancellationToken cancellationToken) =>
        {
            var response = await warehouseInventoryService.CreateWarehouseInventoryAsync(requests, cancellationToken);

            if (response.Success)
                return Results.Ok(response);

            return Results.BadRequest(response);
        })
        .WithName("CreateWarehouseInventory")
        .WithTags("WarehouseInventories")
        .Produces<Application.Common.Models.ApiResponse<Helios.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>>(StatusCodes.Status200OK)
        .Produces<Application.Common.Models.ApiResponse<Helios.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>>(StatusCodes.Status400BadRequest);
    }
}
