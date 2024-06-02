using FluentValidation;
using OnlineShopApp.Dtos.Product;

namespace OnlineShopApp.Validators
{
	public class AddProductValidator : AbstractValidator<AddProductDTO>
	{
		public AddProductValidator()
		{
			RuleFor(x => x.Name).NotEmpty().WithMessage("Product Name is Required");
			RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is Required")
					.GreaterThan(0).WithMessage("CategoryId must be greater than 0.");
			RuleFor(x => x.SubCategoryId).NotEmpty().WithMessage("Sub Category is Required")
					.GreaterThan(0).WithMessage("SubCategoryId must be greater than 0.");
			RuleFor(x => x.Description).NotEmpty().WithMessage("Description is Required")
					.MinimumLength(10).WithMessage("Description must be greater than 10 characters.");
			RuleFor(x => x.Price).NotEmpty().WithMessage("Price is Required")
					.GreaterThan(0).WithMessage("Price must be greater than 0.");
			RuleFor(x => x.Quantity).NotEmpty().WithMessage("Quantity is Required")
					.GreaterThan(0).WithMessage("Quantity must be greater than 0.");
			RuleFor(x => x.Images).NotEmpty().WithMessage("Image is Required");

		}
	}
}
