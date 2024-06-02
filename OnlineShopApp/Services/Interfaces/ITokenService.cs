using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Services.Interfaces
{
	public interface ITokenService
	{		
		string GetToken();
		bool CheckToken();

	}
}
