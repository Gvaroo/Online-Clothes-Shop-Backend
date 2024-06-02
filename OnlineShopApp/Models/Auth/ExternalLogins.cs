namespace OnlineShopApp.Models.Auth
{
	public class ExternalLogins
	{
		public int Id { get; set; }
		public string LoginProvider { get; set; }
		public string ProviderKey { get; set; }
		public string? ProviderDisplayName { get; set; }

		//relation
		public User Users { get; set; }
	}
}
