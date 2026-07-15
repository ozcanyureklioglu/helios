namespace Vectomera.Application.Features.WarehouseInventories.Requests;

public class CreateWarehouseInventoryRequest
{
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int AvailableStock { get; set; }
    public int IncomingStock { get; set; }
    public int OutgoingStock { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
}

