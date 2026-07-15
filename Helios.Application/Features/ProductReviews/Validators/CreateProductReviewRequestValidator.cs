using FluentValidation;
using Helios.Application.Features.ProductReviews.Requests;

namespace Helios.Application.Features.ProductReviews.Validators;

public class CreateProductReviewRequestValidator : AbstractValidator<CreateProductReviewRequest>
{
    public CreateProductReviewRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId zorunludur.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık zorunludur.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama/Yorum zorunludur.");

        RuleFor(x => x.Point)
            .InclusiveBetween(1, 5).WithMessage("Puan 1 ile 5 arasında olmalıdır.");
    }
}
