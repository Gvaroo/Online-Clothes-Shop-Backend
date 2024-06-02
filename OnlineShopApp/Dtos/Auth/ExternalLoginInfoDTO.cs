namespace OnlineShopApp.Dtos.Auth
{
	public class ExternalLoginInfoDTO
	{

		public string Email { get; set; }
		public string? FullName { get; set; }
		public string? Provider { get; set; }
		public string? ProviderKey { get; set; }
		public bool? NewUser { get; set; }
	}
}
