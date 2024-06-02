using FluentValidation;
using OnlineShopApp.Dtos.Auth;

namespace OnlineShopApp.Validators
{
	public class LoginUserValidator : AbstractValidator<LoginUserDTO>
	{
		public LoginUserValidator()
		{
			RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Required")
					.EmailAddress().WithMessage("Login must be email");
			RuleFor(x => x.Password).NotEmpty().WithMessage("Password is Required");



		}
	}
}
