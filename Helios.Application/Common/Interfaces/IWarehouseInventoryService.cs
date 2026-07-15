using Helios.Application.Common.Models;
using Helios.Application.Features.WarehouseInventories.Requests;

namespace Helios.Application.Common.Interfaces;

public interface IWarehouseInventoryService
{
    Task<ApiResponse<Helios.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>> CreateWarehouseInventoryAsync(List<CreateWarehouseInventoryRequest> requests, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<Helios.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto>>> GetWarehouseInventoriesAsync(Guid? productId = null, CancellationToken cancellationToken = default);
}
