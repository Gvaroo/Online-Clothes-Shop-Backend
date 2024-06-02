using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Models.Cart
{
	public class ShippingInfo
	{
		public int Id { get; set; }
		public string ShippingAddress { get; set; }
		public string ShippingCountry { get; set; }
		public string ShippingCity { get; set; }
		public string ZipCode { get; set; }
		public int PhoneNumber { get; set; }

		//relation
		public User User { get; set; }
		public int UserId { get; set; }
	}
}
