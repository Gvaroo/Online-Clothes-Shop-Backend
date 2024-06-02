using OnlineShopApp.Models.Auth;
using OnlineShopApp.Models.Product;

namespace OnlineShopApp.Models.Cart
{
	public class Cart
	{
		public int CartId { get; set; }
		public User User { get; set; }
		public int UserId { get; set; }
		public List<CartItem> CartItem { get; set; } = new List<CartItem>();

	}
}
