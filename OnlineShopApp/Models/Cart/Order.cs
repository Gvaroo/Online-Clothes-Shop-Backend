using OnlineShopApp.Models.Auth;

namespace OnlineShopApp.Models.Cart
{
	public class Order
	{
		public int OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalAmount { get; set; }

		//relation
		public User User { get; set; }
		public List<OrderItems> OrderItems { get; set; } = new List<OrderItems>();

	}
}
