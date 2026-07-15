namespace Vectomera.Application.Features.WarehouseInventories.Dtos;

public record WarehouseInventoryDto(
    string ProductName,
    string Sku,
    string WarehouseName,
    string WarehouseCity,
    decimal Price,
    int Stock,
    string? ProductDescription,
    string? InventoryDescription
);

