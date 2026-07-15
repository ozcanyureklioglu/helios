using FluentValidation;
using Vectomera.Application.Common.Events;
using Vectomera.Application.Common.Interfaces;
using Vectomera.Application.Common.Models;
using Vectomera.Application.Features.WarehouseInventories.Requests;
using Vectomera.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Vectomera.Infrastructure.Services;

public class WarehouseInventoryService : IWarehouseInventoryService
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<CreateWarehouseInventoryRequest> _validator;

    public WarehouseInventoryService(
        IAppDbContext context,
        IPublishEndpoint publishEndpoint,
        IValidator<CreateWarehouseInventoryRequest> validator)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _validator = validator;
    }

    public async Task<ApiResponse<Vectomera.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>> CreateWarehouseInventoryAsync(
        List<CreateWarehouseInventoryRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var responseData = new Vectomera.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse();

        if (requests == null || !requests.Any())
            return ApiResponse<Vectomera.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>.Fail("Liste boş olamaz.");

        var productIds = requests.Select(r => r.ProductId).Distinct().ToList();
        var warehouseIds = requests.Select(r => r.WarehouseId).Distinct().ToList();

        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Sku })
            .ToListAsync(cancellationToken);

        var existingWarehouses = await _context.Warehouses
            .Where(w => warehouseIds.Contains(w.Id))
            .Select(w => w.Id)
            .ToListAsync(cancellationToken);

        var existingInventories = await _context.WarehouseInventories
            .Where(wi => warehouseIds.Contains(wi.WarehouseId) && productIds.Contains(wi.ProductId))
            .Select(wi => new { wi.WarehouseId, wi.ProductId })
            .ToListAsync(cancellationToken);

        var validInventories = new List<WarehouseInventory>();

        foreach (var request in requests)
        {
            var product = existingProducts.FirstOrDefault(p => p.Id == request.ProductId);
            var sku = product?.Sku;

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                responseData.Errors.Add(new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                });
                continue;
            }

            if (product == null)
            {
                responseData.Errors.Add(new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = "Ürün bulunamadı."
                });
                continue;
            }

            if (!existingWarehouses.Contains(request.WarehouseId))
            {
                responseData.Errors.Add(new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = "Depo bulunamadı."
                });
                continue;
            }

            if (validInventories.Any(v => v.WarehouseId == request.WarehouseId && v.ProductId == request.ProductId))
            {
                 responseData.Errors.Add(new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryErrorDto
                 {
                     ProductId = request.ProductId,
                     Sku = sku,
                     ErrorMessage = "Bu listede bu ürün bu depo için mükerrer gönderilmiş."
                 });
                 continue;
            }

            if (existingInventories.Any(ei => ei.WarehouseId == request.WarehouseId && ei.ProductId == request.ProductId))
            {
                responseData.Errors.Add(new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = "Bu ürün bu depo için zaten kayıtlı."
                });
                continue;
            }

            var inventory = new WarehouseInventory
            {
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                AvailableStock = request.AvailableStock,
                IncomingStock = request.IncomingStock,
                OutgoingStock = request.OutgoingStock,
                Price = request.Price,
                Description = request.Description
            };

            validInventories.Add(inventory);
        }

        if (validInventories.Any())
        {
            _context.WarehouseInventories.AddRange(validInventories);
            await _context.SaveChangesAsync(cancellationToken);

            responseData.SuccessfulInventoryIds = validInventories.Select(v => v.Id).ToList();

            var publishTasks = validInventories.Select(inventory => 
                _publishEndpoint.Publish(new Vectomera.Application.Common.Events.CreateWarehouseInventoryEvent
                {
                    InventoryId = inventory.Id,
                    Description = inventory.Description
                }, cancellationToken)
            );

            await Task.WhenAll(publishTasks);
        }

        return ApiResponse<Vectomera.Application.Features.WarehouseInventories.Dtos.BulkCreateWarehouseInventoryResponse>.Ok(responseData, "İşlem tamamlandı.");
    }

    public async Task<ApiResponse<List<Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto>>> GetWarehouseInventoriesAsync(
        Guid? productId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.WarehouseInventories
            .Include(wi => wi.Product)
            .Include(wi => wi.Warehouse)
            .AsNoTracking();

        if (productId.HasValue)
        {
            query = query.Where(wi => wi.ProductId == productId.Value);
        }

        var inventories = await query
            .Select(wi => new Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto(
                wi.Product.Name,
                wi.Product.Sku,
                wi.Warehouse.Name,
                wi.Warehouse.CityName,
                wi.Price,
                wi.AvailableStock,
                wi.Product.Description,
                wi.Description))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<Vectomera.Application.Features.WarehouseInventories.Dtos.WarehouseInventoryDto>>.Ok(inventories);
    }
}

