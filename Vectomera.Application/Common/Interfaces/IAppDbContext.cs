using Vectomera.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Vectomera.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Brand> Brands { get; }
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<Property> Properties { get; }
    DbSet<PropertyValue> PropertyValues { get; }
    DbSet<ProductProperty> ProductProperties { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<WarehouseInventory> WarehouseInventories { get; }
    DbSet<BrandVectorChunk> BrandVectorChunks { get; }
    DbSet<ProductVectorChunk> ProductVectorChunks { get; }
    DbSet<WarehouseInventoryVectorChunk> WarehouseInventoryVectorChunks { get; }
    DbSet<ProductReview> ProductReviews { get; }
    DbSet<ProductReviewVectorChunk> ProductReviewVectorChunks { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

