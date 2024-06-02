namespace OnlineShopApp.Dtos.Cart
{
	public class GetOrderHistoryDTO
	{
		public int OrderId { get; set; }
		public string? Name { get; set; }
		public int? Quantity { get; set; }
		public decimal OriginalPrice { get; set; }
		public decimal TotalPrice { get; set; }
		public string ProductSize { get; set; }
		public String? OrderDate { get; set; }            //DateTime
	
	}
}
