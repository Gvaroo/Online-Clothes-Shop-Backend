using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using OnlineShopApp.Data;
using OnlineShopApp.Dtos.Auth;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Services.Interfaces;
using System.Net;

namespace OnlineShopApp.Controllers
{
	/// <summary>
	/// Controller for managing user authentication and authorization.
	/// </summary>
	[Route("api/[controller]/[action]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IAuthRepository _auth;

		public AuthController(IAuthRepository auth, ITokenService tokenService)
		{
			_auth = auth;
		}

		/// <summary>
		/// Registers a new user.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> Register(RegisterUserDTO newUser)
		{
			var result = await _auth.Register(newUser);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Logs in a user.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> Login(LoginUserDTO credentials)
		{
			var result = await _auth.Login(credentials);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Sends a verification code to the user.
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> SendVerificationCode()
		{
			var result = await _auth.GenerateAndSendVerificationCode();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Updates the user's profile.
		/// </summary>
		[HttpPut]
		[Authorize]
		public async Task<IActionResult> UpdateProfile(UpdateProfileDTO newProfile)
		{
			var result = await _auth.UpdateProfile(newProfile);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Checks if the user is an admin.
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> IsAdmin()
		{
			var result = await _auth.IsAdmin();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Handles external authentication response.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> HandleExternalAuthenticationResponseAsync(ExternalLoginInfoDTO externalUser)
		{
			var result = await _auth.HandleExternalAuthenticationResponseAsync(externalUser);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Registers a new external user.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> RegisterNewExternalUser(ExternalLoginInfoDTO newUser)
		{
			var result = await _auth.RegisterNewExternalUser(newUser);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Checks if the user is logged in.
		/// </summary>
		[HttpGet]
		[Authorize]
		public ActionResult IsLoggedIn()
		{
			var result = _auth.IsLoggedIn();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Logs out the user.
		/// </summary>
		[HttpGet]
		[Authorize]
		public IActionResult LogOut()
		{
			var result = _auth.LogOut();
			return Ok(result); ;
		}

		/// <summary>
		/// Checks if the user's email is verified.
		/// </summary>
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> IsVerified()
		{
			var result = await _auth.IsVerified();
			return result.Success ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Sends an email confirmation email.
		/// </summary>
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> SentEmailConfirmationEmail(User? user)
		{
			var result = await _auth.SentEmailConfirmationEmail(user);
			return result ? Ok(result) : BadRequest(result);
		}

		/// <summary>
		/// Verifies the user's email with the provided code.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> VerifyUserEmail(string code)
		{
			var result = await _auth.VerifyUserEmail(code);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}

}
