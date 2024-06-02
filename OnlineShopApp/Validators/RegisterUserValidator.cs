using FluentValidation;
using OnlineShopApp.Dtos.Auth;

namespace OnlineShopApp.Validators
{
	public class RegisterUserValidator : AbstractValidator<RegisterUserDTO>
	{
		public RegisterUserValidator()
		{
			RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Required")
					.EmailAddress().WithMessage("Email must be email format");
			RuleFor(x => x.Password).NotEmpty().WithMessage("Password is Required")
				   .MinimumLength(5).WithMessage("Password must be greater than 5 characters.");
			RuleFor(x => x.FullName).NotEmpty().WithMessage("Fullname is Required")
				   .MinimumLength(3).WithMessage("Name must be greater than 3 characters.");

		}
	}
}
