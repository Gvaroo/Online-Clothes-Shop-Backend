using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShopApp.Dtos.Auth;
using OnlineShopApp.Models;
using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Data
{
	public interface IAuthRepository
	{
		Task<ServiceResponse<int>> Register(RegisterUserDTO newUser);
		Task<ServiceResponse<UserInfoDTO>> Login(LoginUserDTO credentials);
		Task<ServiceResponse<UserInfoDTO>> UpdateProfile(UpdateProfileDTO newProfile);
		Task<ServiceResponse<bool>> GenerateAndSendVerificationCode();
		Task<bool> SentEmailConfirmationEmail(User? user);
		Task<ServiceResponse<string>> VerifyUserEmail(string code);
		ServiceResponse<bool> IsLoggedIn();
		Task<ServiceResponse<bool>> IsVerified();
		Task<ServiceResponse<bool>> IsAdmin();
		Task<bool> UserExists(string email);
		Task<string> LogOut();
		Task<ServiceResponse<ExternalLoginInfoDTO>> HandleExternalAuthenticationResponseAsync(ExternalLoginInfoDTO externalUser);
		Task<ServiceResponse<UserInfoDTO>> RegisterNewExternalUser(ExternalLoginInfoDTO newUser);
	}
}
