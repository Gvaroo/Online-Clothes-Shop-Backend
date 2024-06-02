using Microsoft.IdentityModel.Tokens;
using OnlineShopApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnlineShopApp.Services.Implementations
{
	public class TokenValidationService : ITokenValidationService
	{
		private readonly IConfiguration _configuration;
		private readonly JwtSecurityTokenHandler _handler;
		private readonly ITokenService _tokenService;

		public TokenValidationService(IConfiguration configuration, ITokenService tokenService)
		{
			_configuration = configuration;
			_handler = new JwtSecurityTokenHandler();
			_tokenService = tokenService;
		}

		// Gets Token
		private string GetTokenFromContext()		{
		
			return _tokenService.GetToken();
		}

		// Tries to validate token
		public bool TryValidateToken(string token, out ClaimsPrincipal claimsPrincipal)
		{
			var key = Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value);
			var TokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = false,				                                     
				ValidateAudience = false,
				                                
			};
			try
			{
				claimsPrincipal = _handler.ValidateToken(token, TokenValidationParameters, out _);
				return true;
			}
			catch (Exception)
			{
				claimsPrincipal = null;
				return false;
			}
		}
		// get userId from token
		public int GetUserIdFromToken()
		{
			var token = GetTokenFromContext();
			if (TryValidateToken(token, out ClaimsPrincipal claimsPrincipal))
			{
				var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				return !string.IsNullOrEmpty(userId) ? Convert.ToInt32(userId) : 0;
			}
			return 0;
		}
	}
}
