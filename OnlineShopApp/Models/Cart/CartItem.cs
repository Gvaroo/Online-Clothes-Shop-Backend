namespace OnlineShopApp.Models.Cart
{
	public class CartItem
	{
		public int CartItemId { get; set; }
		public Cart Cart { get; set; }
		public Product.Product Product { get; set; }
		public int Quantity { get; set; }
		public int SizeId { get; set; }

	}
}
