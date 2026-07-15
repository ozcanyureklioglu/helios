namespace Helios.Application.Common.Events;

public class CreateWarehouseInventoryEvent
{
    public Guid InventoryId { get; set; }
    public string? Description { get; set; }
}
