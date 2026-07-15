using FluentValidation;
using Vectomera.Application.Features.Products.Requests;

namespace Vectomera.Application.Features.Products.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("ÃœrÃ¼n adÄ± boÅŸ olamaz.")
            .MaximumLength(200).WithMessage("ÃœrÃ¼n adÄ± en fazla 200 karakter olabilir.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU boÅŸ olamaz.")
            .MaximumLength(100).WithMessage("SKU en fazla 100 karakter olabilir.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Marka seÃ§imi zorunludur.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori seÃ§imi zorunludur.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("AÃ§Ä±klama en fazla 2000 karakter olabilir.")
            .When(x => x.Description != null);
    }
}

