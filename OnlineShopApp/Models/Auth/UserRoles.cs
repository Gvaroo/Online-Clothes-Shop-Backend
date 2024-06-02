namespace OnlineShopApp.Models.Auth
{
	public class UserRoles
	{
		public int Id { get; set; }
		public string Role { get; set; }

		//relation
		public List<User> Users { get; set; } = new List<User>();

	}
}
