using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Dtos.Auth
{
	public class UpdateProfileDTO
	{
		public string FullName { get; set; }
		public string Email { get; set; }		
		public string? CurrentPassword { get; set; }
		public string? NewPassword { get; set; }
		public string verificationCode { get; set; }
	}
}
