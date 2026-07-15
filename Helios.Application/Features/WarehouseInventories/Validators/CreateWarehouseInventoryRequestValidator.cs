using FluentValidation;
using Helios.Application.Features.WarehouseInventories.Requests;

namespace Helios.Application.Features.WarehouseInventories.Validators;

public class CreateWarehouseInventoryRequestValidator : AbstractValidator<CreateWarehouseInventoryRequest>
{
    public CreateWarehouseInventoryRequestValidator()
    {
        RuleFor(x => x.WarehouseId)
            .NotEmpty().WithMessage("WarehouseId zorunludur.");

        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId zorunludur.");

        RuleFor(x => x.AvailableStock)
            .GreaterThanOrEqualTo(0).WithMessage("Mevcut stok 0'dan küçük olamaz.");

        RuleFor(x => x.IncomingStock)
            .GreaterThanOrEqualTo(0).WithMessage("Gelecek stok 0'dan küçük olamaz.");

        RuleFor(x => x.OutgoingStock)
            .GreaterThanOrEqualTo(0).WithMessage("Giden stok 0'dan küçük olamaz.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat 0'dan küçük olamaz.");
    }
}
