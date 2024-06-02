using FluentValidation;
using OnlineShopApp.Dtos.Auth;

namespace OnlineShopApp.Validators
{
	public class UpdateProfileValidator : AbstractValidator<UpdateProfileDTO>
	{
		public UpdateProfileValidator()
		{
			RuleFor(x => x.Email).NotEmpty().WithMessage("Email is Required")
					.EmailAddress().WithMessage("Email must be email format");
			RuleFor(x => x.FullName).NotEmpty().WithMessage("Fullname is Required")
				   .MinimumLength(3).WithMessage("Name must be greater than 3 characters.");			
            RuleFor(x=>x.NewPassword).MinimumLength(5).WithMessage("Password must be at least 5 characters long if provided.");


		}

	}
}
