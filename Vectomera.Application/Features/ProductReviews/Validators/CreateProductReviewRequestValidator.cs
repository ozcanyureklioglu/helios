using FluentValidation;
using Vectomera.Application.Features.ProductReviews.Requests;

namespace Vectomera.Application.Features.ProductReviews.Validators;

public class CreateProductReviewRequestValidator : AbstractValidator<CreateProductReviewRequest>
{
    public CreateProductReviewRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("BaÅŸlÄ±k zorunludur.")
            .MaximumLength(200).WithMessage("BaÅŸlÄ±k en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("AÃ§Ä±klama/Yorum zorunludur.");

        RuleFor(x => x.Point)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasÄ±nda olmalÄ±dÄ±r.");
    }
}

