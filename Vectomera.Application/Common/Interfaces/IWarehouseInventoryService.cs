using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.WarehouseInventories.Requests;

namespace Vectomera.Application.Common.Interfaces;

public interface IWarehouseInventoryService
{
    Task<ApiResponse<Vectomera.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>> CreateWarehouseInventoryAsync(List<CreateWarehouseInventoryRequest> requests, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto>>> GetWarehouseInventoriesAsync(Guid? productId = null, CancellationToken cancellationToken = default);
}

