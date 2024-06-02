using System.ComponentModel.DataAnnotations;

namespace OnlineShopApp.Dtos.Auth
{
	public class RegisterUserDTO
	{
		[Required]
		public string FullName { get; set; }
		[Required]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }
		[Required]
		public string Password { get; set; }
	}
}
