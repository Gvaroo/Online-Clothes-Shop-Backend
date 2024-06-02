using System.Security.Claims;

namespace OnlineShopApp.Services.Interfaces
{
	public interface ITokenValidationService
	{
		bool TryValidateToken(string token, out ClaimsPrincipal claimsPrincipal);
		int GetUserIdFromToken();
	}
}
