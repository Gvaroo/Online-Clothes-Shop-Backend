using FluentValidation;
using OnlineShopApp.Dtos.Product;

namespace OnlineShopApp.Validators
{
	public class AddProductReviewAndRatingValidator : AbstractValidator<AddProductReviewAndRatingDTO>
	{
		public AddProductReviewAndRatingValidator()
		{
			RuleFor(x => x.ProductId).NotEmpty().WithMessage("productId is Required");
			RuleFor(x => x.Review).NotEmpty().WithMessage("Review is Required")
								  .MinimumLength(10).WithMessage("Review must be atleast 10 character");
		}
	}
}
