namespace OnlineShopApp.Models.Auth
{
	public class SecurityVerificationCodes
	{
		public int Id { get; set; }
		public string Code { get; set; }
		public DateTime ExpireDate { get; set; }

		//relation
		public User User { get; set; }
		
	}
}
