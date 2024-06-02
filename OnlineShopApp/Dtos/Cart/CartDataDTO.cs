namespace OnlineShopApp.Dtos.Cart
{
	public class CartDataDTO
	{
		public int ProductId { get; set; }
		public int SizeId { get; set; }
		public int Quantity { get; set; }
		public bool? Guest { get; set; }
	}
}
