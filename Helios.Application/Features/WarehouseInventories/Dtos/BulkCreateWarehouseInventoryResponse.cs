namespace Helios.Application.Features.WarehouseInventories.Dtos;

public class WarehouseInventoryErrorDto
{
    public Guid ProductId { get; set; }
    public string? Sku { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

public class BulkCreateWarehouseInventoryResponse
{
    public List<Guid> SuccessfulInventoryIds { get; set; } = new List<Guid>();
    public List<WarehouseInventoryErrorDto> Errors { get; set; } = new List<WarehouseInventoryErrorDto>();
}
