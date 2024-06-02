using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Dtos.Auth
{
	public class LoginUserDTO
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
