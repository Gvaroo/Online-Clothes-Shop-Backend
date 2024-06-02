using AutoMapper;
using Microsoft.AspNetCore.Http;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineShopApp.Dtos.Auth;
using OnlineShopApp.Models;
using OnlineShopApp.Models.Auth;
using OnlineShopApp.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OnlineShopApp.Services.Implementations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Google.Apis.Auth;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using Newtonsoft.Json.Linq;
using OnlineShopApp.Validators;

namespace OnlineShopApp.Data
{
	public class AuthRepository : IAuthRepository
	{
		private readonly ApplicationDbContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _configuration;
		private readonly IEmailSender _emailSender;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ITokenValidationService _tokenValidation;
		private readonly ITokenService _tokenService;


		public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration, IEmailSender emailSender, IHttpContextAccessor httpContextAccessor, ITokenValidationService tokenValidation, ITokenService tokenService)
		{
			_db = db;
			_mapper = mapper;
			_configuration = configuration;
			_emailSender = emailSender;
			_httpContextAccessor = httpContextAccessor;
			_tokenValidation = tokenValidation;
			_tokenService = tokenService;
		}


		
		public async Task<ServiceResponse<UserInfoDTO>> Login(LoginUserDTO credentials)
		{
			var response = new ServiceResponse<UserInfoDTO>();
			try
			{
				var user = await _db.Users
				.Include(user => user.Role)
				.FirstOrDefaultAsync(user => user.Email == credentials.Email);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
				}
				else if (user != null && user.PasswordHash == null)
				{
					response.Success = false;
					response.Message = "You tried signing in with a different authentication method than the one you used during signup. Please try again using your original authentication method";
				}
				else if (!VerifyPasswordHash(credentials.Password, user.PasswordHash, user.PasswordSalt))
				{
					response.Success = false;
					response.Message = "Password is incorrect!";

				}
				else
				{
					var userInfo = new UserInfoDTO()
					{
						Email = user.Email,
						FullName = user.FullName,
					};
					//generate token
					var token = GenerateToken(user);

					// Call the SetCookie method to set the token in the response
					SetCookie(token, _httpContextAccessor.HttpContext);
					response.Data = userInfo;

				}

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;

			}
			return response;

		}

		public async Task<ServiceResponse<int>> Register(RegisterUserDTO newUser)
		{
			var response = new ServiceResponse<int>();
			try
			{
				var user = _mapper.Map<User>(newUser);
				if (await UserExists(newUser.Email))
				{
					response.Success = false;
					response.Message = "User with that email is already registered!";
					return response;
				}
				CreatePasswordHash(newUser.Password, out string passwordHash, out string passwordSalt);
				user.PasswordSalt = passwordSalt;
				user.PasswordHash = passwordHash;
				var userRole = await _db.Roles.FirstOrDefaultAsync(role => role.Role == "User");
				user.Role = userRole;

				await _db.Users.AddAsync(user);
				await _db.SaveChangesAsync();

				//Send Email Confirmation to user
				await SentEmailConfirmationEmail(user);
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}
		// Create http only cookie that will save jwt token
		public void SetCookie(string token, HttpContext httpContext)
		{
			// Create a new cookie with the token
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None,
				Expires = DateTime.UtcNow.AddDays(1),
				Domain = "localhost",
				Path = "/"
			};

			httpContext.Response.Cookies.Append("access_token", token, cookieOptions);
		}


		// Checks if User is in database
		public async Task<bool> UserExists(string email)
		{
			var user = await _db.Users.FirstOrDefaultAsync(e => e.Email == email);
			if (user == null) return false;
			else return true;
		}
		private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
		{
			passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
			passwordHash = BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);
		}
		private bool VerifyPasswordHash(string password, string passwordHash, string passwordSalt)
		{
			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);
			return hashedPassword == passwordHash;
		}
		

		// Updates user profile (external or not)
		public async Task<ServiceResponse<UserInfoDTO>> UpdateProfile(UpdateProfileDTO newProfile)
		{
			var response = new ServiceResponse<UserInfoDTO>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
							.Include(c => c.Role)
							.Include(c => c.securityVerificationCodes)
							.FirstOrDefaultAsync(u => u.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found!";
					return response;
				}
				var validVerificationCode = user.securityVerificationCodes.Any(b =>
				b.Code == newProfile.verificationCode && b.ExpireDate > DateTime.UtcNow);

				if (!validVerificationCode)
				{
					response.Success = false;
					response.Message = "Invalid verification code or code has expired.";
					return response;
				}

				//Verification code is valid, processing with updating user information	

				//This checks if user is external or not
				if (user.PasswordHash == null)
				{
					//if its external only change user full name, since email is bound to external authorization and password isnt needed.
					//usually user cant type password or change email anyway but this checks anyway just incase.
					user.FullName = newProfile.FullName;
				}
				else
				{
					//user is not external

					//Update user profile
					_mapper.Map(newProfile, user);

					// Checks if user requested password change
					if (IsPasswordChangeRequested(newProfile))
					{
						if (IsCurrentPasswordValid(newProfile.CurrentPassword, user))
						{
							UpdateUserPassword(newProfile.NewPassword, user);
						}
						else
						{
							response.Success = false;
							response.Message = "Incorrect Password";
							return response;
						}
					}


				}

				//Deleting verification code of user
				user.securityVerificationCodes.Clear();

				//return new userInfo to client

				var userInfo = new UserInfoDTO()
				{
					Email = user.Email,
					FullName = user.FullName,
				};

				await _db.SaveChangesAsync();
				response.Data = userInfo;

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Checks if user is Administrator
		public async Task<ServiceResponse<bool>> IsAdmin()
		{
			var response = new ServiceResponse<bool>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users
				.Include(u => u.Role)
				.FirstOrDefaultAsync(u => u.Id == userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
				}

				if (user.Role.Role == "Admin")
					response.Data = true;

				else response.Data = false;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}


		//  sends a verification code via email
		public async Task<ServiceResponse<bool>> GenerateAndSendVerificationCode()
		{
			var response = new ServiceResponse<bool>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users.FindAsync(userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "user not found";
					return response;
				}
				var verificationCode = GenerateRandomString(6);
				var verification = new SecurityVerificationCodes
				{
					Code = verificationCode,
					ExpireDate = DateTime.UtcNow.AddMinutes(15)
				};
				user.securityVerificationCodes.Add(verification);

				//send verifiicaiton code to user email
				await SendVerificationCodeByEmail(user, verificationCode);

				await _db.SaveChangesAsync();
				response.Data = true;

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}


		// Helper Method. Generates random string for verification code
		private string GenerateRandomString(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var random = new Random();

			var result = new string(Enumerable.Repeat(chars, length)
				.Select(s => s[random.Next(s.Length)]).ToArray());

			return result;
		}

		// Helper Method. checks if passwordChange was requested
		private bool IsPasswordChangeRequested(UpdateProfileDTO newProfile)
		{
			return newProfile.NewPassword != null;
		}
		// Helper method. checks if received password is same as in database
		private bool IsCurrentPasswordValid(string currentPassword, User user)
		{
			return VerifyPasswordHash(currentPassword, user.PasswordHash, user.PasswordSalt);
		}
		// Helper method. Updates user password
		private void UpdateUserPassword(string newPassword, User user)
		{
			CreatePasswordHash(newPassword, out string passwordHash, out string passwordSalt);
			user.PasswordSalt = passwordSalt;
			user.PasswordHash = passwordHash;
		}

		// Sends verification code by email
		public async Task SendVerificationCodeByEmail(User user, string verificationCode)
		{
			string subject = "Account Verification Code";

			string body = $"Hello {user.FullName},\n\n"
				+ "You have requested to update your profile on our website. "
				+ "To proceed with the update, please use the following verification code within the next 15 minutes:\n\n"
				+ $"Verification Code: {verificationCode}\n\n"
				+ "If you didn't initiate this action, you can safely ignore this email.\n\n"
				+ "Thank you!\n\n"
				+ "Best regards,\n"
				+ "Your Website Team";

			// Send email as backgounrd process
			await _emailSender.SendEmailAsyncBackground(user.Email, subject, body);
		}

		public async Task<string> LogOut()
		{

			// Clear http only cookie
			var cookieOptions = new CookieOptions
			{
				HttpOnly = true,
				Secure = true,
				SameSite = SameSiteMode.None
			};
			
			_httpContextAccessor.HttpContext.Response.Cookies.Delete("access_token", cookieOptions);

			return "Logged out successfully.";
		}

		public ServiceResponse<bool> IsLoggedIn()
		{
			var response = new ServiceResponse<bool>();
			try
			{

				if (_tokenService.CheckToken())
					response.Data = true;
				else
					response.Data = false;
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}


		// Handles the response from external authentication (Google login) for the given user.
		public async Task<ServiceResponse<ExternalLoginInfoDTO>> HandleExternalAuthenticationResponseAsync(ExternalLoginInfoDTO externalUser)
		{
			var response = new ServiceResponse<ExternalLoginInfoDTO>();
			try
			{
				// Prepare settings for validating the Google ID token using Google's .NET client library
				GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings();
				settings.Audience = new[] { _configuration.GetSection($"ExternalAuthentication:Google").Value };

				// Validate the Google ID token using the provided settings
				GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(externalUser.ProviderKey, settings);

				// Find if a user with this email exists in the database
				var user = await _db.Users
					.Include(c => c.ExternalLogins)
					.Include(c => c.Role)
					.FirstOrDefaultAsync(c => c.Email == payload.Email);

				// Check if the user is a new external user
				if (user == null)
				{
					// This means the user is new to the system and needs to be registered
					response.Data = new ExternalLoginInfoDTO
					{
						Email = payload.Email,
						FullName = payload.GivenName,
						Provider = externalUser.Provider,
						ProviderKey = payload.JwtId,
						NewUser = true,
					};
				}
				else if (user.ExternalLogins.Any(externalLogin => externalLogin.LoginProvider == externalUser.Provider))
				{
					// If the user has logged in with the same externalProvider before, update user provider key
					var externalLogin = await _db.ExternalLogins.FirstOrDefaultAsync(c => c.Users.Id == user.Id);
					externalLogin.ProviderKey = payload.JwtId;
					externalLogin.ProviderDisplayName = payload.GivenName;

					// Generate Token for authentication					
					var token = GenerateToken(user);
					// Set the token in the http-only cookie for secure authentication
					SetCookie(token, _httpContextAccessor.HttpContext);

					// Send relevant user information for client-side handling
					response.Data = new ExternalLoginInfoDTO
					{
						Email = user.Email,
						FullName = payload.GivenName,
					};

					// Update the database to save the changes
					await _db.SaveChangesAsync();
				}
				else
				{
					// This means that the user is already registered with another authentication method
					response.Success = false;
					response.Message = "User with this email is registered already! Please try logging in with another authentication method.";
				}
			}
			catch (Exception ex)
			{
				// Handle any unexpected errors that might occur during the process
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}

		// Registers a new external user based on the provided external login information.
		public async Task<ServiceResponse<UserInfoDTO>> RegisterNewExternalUser(ExternalLoginInfoDTO newUser)
		{
			var response = new ServiceResponse<UserInfoDTO>();
			try
			{
				// Check if the user with the provided email already exists in the system
				if (await UserExists(newUser.Email))
				{
					response.Success = false;
					response.Message = "User with that email is already registered!";
					return response;
				}

				// Create a new user with the provided external login information
				var user = new User() { Email = newUser.Email, FullName = newUser.FullName, IsVerified = true };
				// Assign a default role to the new user
				var role = await _db.Roles.FindAsync(2);
				user.Role = role;
				var externalLogin = new ExternalLogins { LoginProvider = newUser.Provider, ProviderKey = newUser.ProviderKey, ProviderDisplayName = newUser.FullName };
				user.ExternalLogins.Add(externalLogin);
				await _db.Users.AddAsync(user);

				// Save the new user information in the database
				await _db.SaveChangesAsync();

				// Generate Token for authentication
				var token = GenerateToken(user);
				// Set the token in the http-only cookie for secure authentication
				SetCookie(token, _httpContextAccessor.HttpContext);

				// Map and provide relevant user information to the client-side
				response.Data = _mapper.Map<UserInfoDTO>(user);
			}
			catch (Exception ex)
			{
				// Handle any unexpected errors that might occur during the registration process
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}
		public async Task<bool> SentEmailConfirmationEmail(User? user)
		{
			try
			{
				User targetUser = new User();

				//If received user is null this means that user trying to verify email again after registering
				if (user == null)
				{

					var userId = _tokenValidation.GetUserIdFromToken();
					targetUser = await _db.Users
						.Include(c => c.Role)
						.FirstOrDefaultAsync(c => c.Id == userId);
				}
				//if received user is not null this means user just registered.
				else
					targetUser = user;

				// Generate the Email Confirmation Token
				var confirmationToken = GenerateToken(targetUser);

				//Get Web App Url
				var WebAppUrl = _configuration.GetSection("WebAppUrl:Development").Value;

				// Create the callback URL
				var callbackUrl = $"{WebAppUrl}/emailConfirmed?code={Uri.EscapeDataString(confirmationToken)}";

				// Send the confirmation email
				await _emailSender.SendEmailAsyncBackground(targetUser.Email, "Email Confirmation By OnlineShop", $"Please confirm your email by clicking the link below:<br/><a href='{callbackUrl}'>{callbackUrl}</a>");
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}

		}

		public async Task<ServiceResponse<string>> VerifyUserEmail(string code)
		{
			var response = new ServiceResponse<string>();
			try
			{
				//validate token and get userId from it
				if (_tokenValidation.TryValidateToken(code, out ClaimsPrincipal claimsPrincipal))
				{
					var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
					var user = await _db.Users.FindAsync(Convert.ToInt32(userId));
					if (user == null)
					{
						response.Success = false;
						response.Message = "User not found!";
						return response;
					}

					//verify user
					user.IsVerified = true;
					await _db.SaveChangesAsync();
					response.Data = $"Your email address <{user.Email}> has been verified";
				}
				else
				{
					response.Success = false;
					response.Message = "Your email verification failed. Please use correct link!";

				}
			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}
		private string GenerateToken(User user)
		{
			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
				new Claim(ClaimTypes.Email,user.Email),
				new Claim(ClaimTypes.Role,user.Role.Role)
			};

			var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
			var descriptor = new SecurityTokenDescriptor()
			{
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.UtcNow.AddDays(1),
				SigningCredentials = credentials

			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(descriptor);			
			return tokenHandler.WriteToken(token);
		}

		public async Task<ServiceResponse<bool>> IsVerified()
		{
			
			var response = new ServiceResponse<bool>();
			try
			{
				var userId = _tokenValidation.GetUserIdFromToken();
				var user = await _db.Users.FindAsync(userId);
				if (user == null)
				{
					response.Success = false;
					response.Message = "User not found";
					return response;
				};

				//Check if user is verified
				if (user.IsVerified)
					response.Data = true;
				else
					response.Data = false;

			}
			catch (Exception ex)
			{
				response.Success = false;
				response.Message = ex.Message;
			}
			return response;
		}
	}
}

