using FluentValidation;
using Helios.Application.Common.Events;
using Helios.Application.Common.Interfaces;
using Helios.Application.Common.Models;
using Helios.Application.Features.ProductReviews.Requests;
using Helios.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Helios.Infrastructure.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IAppDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IValidator<CreateProductReviewRequest> _validator;

    public ProductReviewService(
        IAppDbContext context,
        IPublishEndpoint publishEndpoint,
        IValidator<CreateProductReviewRequest> validator)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _validator = validator;
    }

    public async Task<ApiResponse<Helios.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>> CreateProductReviewAsync(
        List<CreateProductReviewRequest> requests,
        CancellationToken cancellationToken = default)
    {
        var responseData = new Helios.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse();

        if (requests == null || !requests.Any())
            return ApiResponse<Helios.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>.Fail("Liste boş olamaz.");

        var productIds = requests.Select(r => r.ProductId).Distinct().ToList();
        var warehouseIds = requests.Where(r => r.WarehouseId.HasValue).Select(r => r.WarehouseId!.Value).Distinct().ToList();

        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Sku })
            .ToListAsync(cancellationToken);

        var existingWarehouses = warehouseIds.Any()
            ? await _context.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .Select(w => w.Id)
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        var validReviews = new List<ProductReview>();

        foreach (var request in requests)
        {
            var product = existingProducts.FirstOrDefault(p => p.Id == request.ProductId);
            var sku = product?.Sku;

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                responseData.Errors.Add(new Helios.Application.Features.ProductReviews.Dtos.ProductReviewErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))
                });
                continue;
            }

            if (product == null)
            {
                responseData.Errors.Add(new Helios.Application.Features.ProductReviews.Dtos.ProductReviewErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = "Ürün bulunamadı."
                });
                continue;
            }

            if (request.WarehouseId.HasValue && !existingWarehouses.Contains(request.WarehouseId.Value))
            {
                responseData.Errors.Add(new Helios.Application.Features.ProductReviews.Dtos.ProductReviewErrorDto
                {
                    ProductId = request.ProductId,
                    Sku = sku,
                    ErrorMessage = "Depo bulunamadı."
                });
                continue;
            }

            var review = new ProductReview
            {
                ProductId = request.ProductId,
                WarehouseId = request.WarehouseId,
                Title = request.Title,
                Description = request.Description,
                Point = request.Point
            };

            validReviews.Add(review);
        }

        if (validReviews.Any())
        {
            _context.ProductReviews.AddRange(validReviews);
            await _context.SaveChangesAsync(cancellationToken);

            responseData.SuccessfulReviewIds = validReviews.Select(v => v.Id).ToList();

            var publishTasks = validReviews.Select(review => 
                _publishEndpoint.Publish(new Helios.Application.Common.Events.CreateProductReviewEvent
                {
                    ReviewId = review.Id,
                    Title = review.Title,
                    Description = review.Description,
                    Point = review.Point
                }, cancellationToken)
            );

            await Task.WhenAll(publishTasks);
        }

        return ApiResponse<Helios.Application.Features.ProductReviews.Dtos.BulkCreateProductReviewResponse>.Ok(responseData, "İşlem tamamlandı.");
    }
}
