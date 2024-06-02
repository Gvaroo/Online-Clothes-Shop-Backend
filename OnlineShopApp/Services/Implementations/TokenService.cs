using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineShopApp.Models;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OnlineShopApp.Services.Implementations
{
	public class TokenService : ITokenService
	{

		private readonly IHttpContextAccessor _httpContextAccessor;
		

		public TokenService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
		{
			_httpContextAccessor = httpContextAccessor;
			
		}


		// Get Token from Cookie
		public string GetToken()
		{
			_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("access_token", out string accessToken);
			return accessToken;
		}


		// Checks if token is in cookie
		public bool CheckToken()
		{
			if (_httpContextAccessor.HttpContext.Request.Cookies.ContainsKey("access_token"))
				return true;
			else
				return false;
		}
	
	}
}
