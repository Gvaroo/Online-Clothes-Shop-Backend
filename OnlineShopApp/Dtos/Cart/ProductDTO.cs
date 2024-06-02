namespace OnlineShopApp.Dtos.Cart
{
	public class ProductDTO
	{
		public int ProductId { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
		public decimal Price { get; set; }
		public int quantity { get; set; }
		public int? MaxQuantity { get; set; }
		public decimal? MaxPrice { get; set; }
		public string ProductSize { get; set; }
		public int? SizeId { get; set; }
	}
}
